# ShadowDusk patches

Local patches against [ShadowDusk](https://github.com/kaltinril/ShadowDusk), the cross-platform
HLSL compiler evaluated here as a replacement for `mgfxc` (MonoGame OpenGL target) and as the
effect compiler for the FNA target (`build/62-package-fna.sh`). Base commit: `e4b1c878`
(2026-09-11).

They live on the `uw-patches` branch of <https://github.com/masaykh/ShadowDusk> as six commits,
one per patch, each written to stand alone as an upstream pull request. **They belong upstream** —
none of them is specific to this game. The include-ordering one (04) breaks any shader split
across `.fxh` headers; the integer-uniform one (03b) fixes a diagnostic that prescribes the wrong
remedy. The fork exists so the work does not wait on a release, not as a permanent home.

Every one of these makes ShadowDusk accept HLSL that is **already valid**. **No game shader is
modified**: the studio's sources are what Refactored Games released, and bending them to suit a
tool would be a permanent fork of someone else's code to work around someone else's bug.

## Result

Effects compiling for `/Profile:OpenGL` against **unmodified** studio sources:

| | effects compiling |
|---|---|
| stock ShadowDusk 0.20.0 | 6 of 19 |
| + these patches | **19 of 19** |

And they load. Swapped into the shipping DesktopGL content and probed through a real MonoGame
`ContentManager` on a real `GraphicsDevice` (`contentprobe -p:UwPlatform=GL … --all-effects`):

    33 loaded, 0 failed, of 33 probed.

with technique counts matching the mgfxc golden effect-for-effect (multiTex 18, Vehicle 8,
skinFX 7, RoundLine 7, water 1, …) and every parameter's initial value restored by the existing
`mgfxtranscode inject` step — the same step the mgfxc pipeline already needs, because mgfxc's
OpenGL path zeroes those defaults too. The exposed parameter set is a strict SUPERSET of mgfxc's
(ShadowDusk additionally exposes the sampler names), so nothing the game calls
`Parameters["…"].SetValue` on is missing.

**And they render.** Packaged as `artifacts/package/UnclaimedWorld-GL-ShadowDusk` — byte-identical
to the shipping GL package except the 19 effect files — and confirmed working in a play test
(2026-09-13). Not yet measured against the mgfxc build pixel-for-pixel; `build/compare-renders.pl`
is the tool for that if this is ever to become the default.

The 13 effects that compiled before these patches produce **byte-identical** output after them,
verified by `cmp` over the full set. ShadowDusk's own unit suite is green except for 5
`ShadowDusk.Compiler.Tests` failures that are **pre-existing on the unmodified checkout** (they
reproduce with every patch reverted):

| suite | before | after |
|---|---|---|
| ShadowDusk.HLSL.Tests | 369 pass | 382 pass |
| ShadowDusk.Core.Tests | 638 pass | 638 pass |
| ShadowDusk.GLSL.Tests | 151 pass | 157 pass |
| ShadowDusk.ShaderToy.Tests | 443 pass | 443 pass |
| ShadowDusk.Compiler.Tests | 186 pass, **5 fail** | 186 pass, **5 fail** (unchanged) |

## Getting a patched compiler

```sh
sh tools/shadowdusk/build-shadowdusk.sh
export UW_SHADOWDUSK="$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe"
sh build/34-build-gl-effects-shadowdusk.sh        # -> content/effects-gl-sd/
```

That script is what CI runs. It fetches two different things from two different places, on
purpose:

| | from | why |
|---|---|---|
| the **compiler** | our fork <https://github.com/masaykh/ShadowDusk>, branch `uw-patches`, pinned to a commit | where these patches live as reviewable history |
| the **natives** (`dxcompiler`, `dxil`, `spirv-cross`, `libvkd3d-shader`) | the published `ShadowDusk.Cli` NuGet tool | not in the source repo; 155 MB across all RIDs; byte-identical to upstream's |

The natives are fetched rather than vendored because git is a poor home for a 27 MB `.so` that
never delta-compresses — every rebuild would add a full copy to history forever. NuGet is
cacheable in CI; a git object is permanent.

`patches/` here carries the same delta as files. The fork is the source of truth for *building*;
these are for reading without cloning anything, and for submitting upstream.

**The pin is deliberate.** `UW_SHADOWDUSK_REF` in the script fixes the exact commit, so an
upstream force-push cannot silently change what the effects are compiled with. Bump it on
purpose, and re-run `build/34` plus the render comparison when you do.

Building by hand instead: ShadowDusk multi-targets `net8.0;net10.0`, so with only the .NET 8 SDK
installed you must pass **both** `-f net8.0` and `-p:TargetFrameworks=net8.0` — `-f` alone still
restores net10.0 and fails.

Patch 01 is for the **FNA** target; 02–06 are for the **MonoGame OpenGL** target, where the open
question is whether ShadowDusk can replace `mgfxc` outright — which would give cross-platform
shader builds on the backend that already ships, and remove most of the reason to consider FNA.

---

## 01 — initialize read-before-write temps

**Symptom.** Two of this game's nineteen effects failed to load under FNA:

    MOJOSHADER_compileEffect Error: Temp register r1 used uninitialized   (multiTex)
    MOJOSHADER_compileEffect Error: Temp register r3 used uninitialized   (Billboard)

**Cause.** MojoShader validates temp-register initialisation linearly, in token order
(`mojoshader.c`, in the destination-operand walk):

```c
if ((info->regtype == REG_TYPE_TEMP) && (reg) && (!reg->written))
    failf(ctx, "Temp register r%d used uninitialized", info->regnum);
```

vkd3d-shader emits streams where a temp is sourced before any instruction names it as a
destination. The D3D9 runtime never enforced this and fxc-era shaders relied on that, so it only
surfaces through MojoShader. Both shaders initialise correctly at the HLSL level
(`PixelShaderOutput Output = (PixelShaderOutput)0;`) — this is purely a code-generation artifact.

**Fix.** A fourth pass in `D3d9BytecodePatcher`, alongside the three vkd3d/MojoShader
canonicalisations already there. It inserts `sub rN, c0, c0` **once, in the declaration
prologue** — after the `dcl`/`def` block and before the first real instruction.

**Not immediately before the offending read**, which is what the first version did and which is
wrong: MojoShader's check is linear, but execution is not. Inside a loop, a register written at
the end of one iteration is legitimately read at the start of the next, and the linear scan flags
that as uninitialised. Initialising at the read then re-zeroes the register every iteration and
destroys the accumulator the shader was carrying. That version loaded cleanly and rendered
garbage — `Billboard` in particular threw the entire shadow pass into overlapping trapezoids and
the diffuse pass into a fan of shards. Hoisting to the prologue satisfies the validator and
cannot clobber a computed value.

`sub rN, c0, c0` rather than a `mov` from a literal because it needs no new `def` constant — one
could collide with a slot the effect's constant table already uses — and it yields zero whatever
`c0` holds, including when `c0` is unbound and reads as zero. The value cannot change behaviour
that was defined before: the register had no defined contents at that point by construction.

Conservative by design: instructions using relative addressing are left alone rather than
mis-parsed, and anything that cannot be walked cleanly returns the input untouched.

**Known limitation.** The pass reads `texkill`'s single operand as a WRITE (it is
destination-encoded) and so never initialises a temp whose only prior appearance is a `texkill`.
MojoShader parses that operand in the same destination walk that carries the check, so it would
flag it. Conservative — the pass fails to fix a case, it never breaks one.

**Test-fixture change.** Four `D3d9BytecodePatcherTests` fixtures are two-instruction synthetic
streams whose only reference to a temp is a READ — not a stream any compiler emits, and one
MojoShader rejects outright. This pass legitimately fires on them and shifts every token their
index-based assertions address. The fixtures now define the temp first (`mov rN, c0`, via a
`Define()` helper) so each test stays aimed at the one pass it is about.

**Result.** 19 of 19 effects compile and **33 of 33 assets load** under FNA
(`contentprobe -p:UwPlatform=FNA ... --all-effects`), up from 30 of 33.

---

## 02 — FX9 declarations and sampling intrinsics

Three related gaps in `FxPreParser`, which rewrites legacy FX9 constructs into the modern forms
DXC accepts.

### 02a — texture declarations carrying storage-class modifiers

**Symptom.** Five effects failed with `use of undeclared identifier` naming a texture that is
plainly declared a few lines above:

    uniform const texture PanelContentTexture;
    uniform const sampler PanelContentSampler : register(s0) = sampler_state
    { Texture = (PanelContentTexture); ... };

**Cause.** The rewrite of the legacy `texture` object type to a modern `Texture2D` guards against
misfiring on a modern declaration whose VARIABLE is named `Texture`:

```csharp
bool prevIsNamePosition = prevKind is TokenKind.Identifier or TokenKind.RAngle;
```

`uniform` and `const` lex as ordinary identifiers, so `uniform const texture X;` looks like "type
followed by name" and the rewrite declines — leaving the texture undeclared, and the
`sampler_state` that references it dangling.

**Fix.** Exclude HLSL storage classes and type modifiers (`uniform`, `const`, `static`, `extern`,
`shared`, `row_major`, `precise`, …) from counting as name position. A modern declaration never
places a modifier immediately before the variable name — `Texture2D const Tex;` is not legal — so
the discrimination stays unambiguous and every previously-rewritten shape is untouched.

Minimal repro (form A compiles, form B does not):

```hlsl
// A - accepted
Texture Tex;
sampler S = sampler_state { texture = <Tex>; magfilter = LINEAR; };

// B - "use of undeclared identifier 'Tex'"
uniform const texture Tex;
uniform const sampler S : register(s0) = sampler_state { Texture = (Tex); MipFilter = Linear; };
```

### 02b — the dimensioned sampling intrinsics

**Symptom.** `error FX0012: The legacy D3D9 sampling intrinsic 'texCUBE' is not supported on this
target` (Vehicle), and the same for `tex1D` (skinFX).

**Cause.** `texCUBE`, `tex3D` and their `grad` forms align **one-to-one** with the modern Texture
methods, exactly like the already-supported `tex2D`. What blocked them is a different thing,
stated in the original comment: "the 1D/3D/CUBE families additionally need a non-Texture2D
resource the sampler rewrite does not synthesize." The FX9 `texture` object type is
**dimensionless** — `texture EnvironmentMap;` says nothing about its dimension — so the rewrite
emitted `Texture2D` for everything.

**Fix.** Take the dimension from the **call site**, which is the only place it is stated. A
pre-scan records, per sampler, the dimension implied by the intrinsics sampling through it; the
declaration rewrite then emits `TextureCube` / `Texture3D` / `Texture2D` accordingly. Because a
texture declaration is reached *before* the sampler that binds it, the dimensionless forms are
resolved in a deferred pass after the whole file is scanned — the same mechanism the existing
`: COLOR` rewrite uses. A sampler used at two different dimensions is not expressible (one
resource cannot be two types) and fails loudly.

`tex1D` is the one that does not align 1:1, and it gets a per-sampler helper:

```hlsl
float4 GradientSampler_SDTex1D(SamplerState s, float t)
    { return Gradient.Sample(s, float2(t, 0.0f)); }
```

D3D10+ dropped the notion of a sampler whose coordinate is narrower than its resource, because a
1D sample is not a distinct operation: it is a 2D sample of a texture one texel tall, with the
second coordinate don't-care. That is also what the D3D9 stack did — the application binds an
ordinary 2D texture (this game's `overlayGradient` is a 76×1 `Texture2D`), and fxc replicates the
scalar across the coordinate register. MojoShader lowers it the same way, decaying the sampler to
`sampler2D` and emitting `texture2D(ps_s2, ps_r0.xx)` — verified against this project's
mgfxc-built `skinFX` golden. Emitting the decay as a helper rather than restructuring the call's
argument list keeps the call-site edit a pure identifier swap, which is all the span-substitution
machinery there can do safely.

`tex2Dlod` / `tex2Dproj` / the `bias` forms still fail with FX0012 — their arguments genuinely do
restructure.

### 02c — a `sampler` passed as a function parameter

**Symptom.** `OutlineShader.fxh(22,11): error X0000: Unsupported intrinsic.` — a `tex2D` that the
rewrite declined, because its sampler is not declared anywhere:

```hlsl
float4 GetOutlineColor(sampler texSampler, float2 texCoord, float3 tintingColor)
{
    Color =  tex2D(texSampler, texCoord);
    ...
}
```

**Cause.** Shader model 4 separated the texture from the sampler, so `tex2D(s, uv)` lowers to
`T.Sample(s, uv)` — which needs a `T`. For a sampler declared at file scope the `sampler_state`
block names it. For one that arrives as a PARAMETER there is nothing to name, so the rewrite had
no binding and left the call alone for DXC to reject.

**Fix.** The parameter carries the texture:

```hlsl
float4 GetOutlineColor(Texture2D texSampler_SDTexture, SamplerState texSampler, float2 texCoord, …)
//                     ^ texture FIRST, matching the call-site edit below

GetOutlineColor(TextureSampler, uv, tint)  ->  GetOutlineColor(BasicTexture, TextureSampler, uv, tint)
```

The sampler argument's own span is replaced with `<texture>, <sampler>`, so the call-site edit
needs no zero-width insertion; argument indices are the ORIGINAL ones on both sides and each
insertion shifts both equally, so a function with several sampler parameters stays aligned.

A sampler parameter is recognized without detecting function definitions at all: the type keyword
sits immediately after a `(` or a `,`, which is a parameter position and nothing else (a call
passes an expression, never `Type ident`). Walking back from there to the parameter list's own
`(` gives the function name and the parameter's index.

Three things this has to get right, each of which cost a test:

- **Passing a sampler to such a function IS sampling through it.** Otherwise its declaration is
  erased as unused, no texture is bound, and the call site has nothing to pass. Which arguments
  those are is only known once the callees are known, and a callee is only recognized once its own
  parameter is known to be sampled — so the scan iterates to a fixed point. The parameter's
  dimension propagates to the argument too, or the caller's texture would be synthesized as the
  wrong resource type.
- **A parameter nothing samples through is left exactly as written** — the same gate the
  file-scope sampler declarations use. There is nothing to fix, and rewriting it would change a
  signature, and every call site, for its own sake.
- **Two functions sharing a parameter name is not shadowing.** They are separate scopes. Only a
  parameter shadowing a FILE-SCOPE sampler is ambiguous (the binding map is keyed by name, so the
  declaration would overwrite the parameter's entry and the body would silently sample the global
  texture through the parameter's sampler) — that one fails loudly.

`tex1D` through a sampler parameter also fails loudly: its decay helper binds to the sampler's
texture, which only exists for a file-scope sampler.

---

## 03 — GLSL rewriter: semantics, uniforms, attributes

Three gaps in `MonoGameGlslRewriter`, which lowers SPIRV-Cross GLSL into the MojoShader dialect.

### 03a — the vertex-input semantic table

**Symptom.** `Unsupported vertex-input semantic 'SV_POSITION' for the MonoGame GL target. The
attribute table models POSITION / COLOR / TEXCOORD / NORMAL; extend SemanticToVertexUsage.`

**Fix.** Strip a leading `SV_` (DXC mirrors the author's spelling, and a D3D9-era shader may write
`float4 pos : SV_Position` on a vertex INPUT, where it means exactly what `POSITION` means — D3D9
has no system-value inputs), and model the whole `VertexElementUsage` enum rather than its first
four members: BINORMAL, TANGENT, BLENDINDICES, BLENDWEIGHT, DEPTH, FOG, PSIZE/POINTSIZE and
TESSFACTOR. A tangent frame, skinning or fog are ordinary things for such a shader to declare, and
each was a hard compile failure. `BINORMAL` is tested before `NORMAL` so the prefix match cannot
claim it.

### 03b — scalar integer/boolean uniforms

**Symptom.** `error SD0210: Unsupported uniform type in 'uint UseIntegerPositions;':
integer/boolean uniforms are not modelled for the MonoGame OpenGL target (MojoShader places them
in the separate {vs,ps}_uniforms_ivec4/_bool register sets, which ShadowDusk does not emit yet).`
Four effects, from the ordinary `uniform const bool UseIntegerPositions;`.

**Fix.** No separate register set is needed, because MonoGame's GL runtime does not use one the
way the diagnostic assumes: `ConstantBuffer.PlatformApply` hands the whole buffer to
`glUniform4fv`, and a Bool parameter's storage is one 4-byte slot exactly like a float's. So an
integer/boolean uniform shares the float register file, and only the **test** has to be re-typed
— GLSL ES 1.00 and versionless desktop GLSL 1.10 (the two profiles one MonoGame GL artifact must
satisfy) have no `uint` type at all and reject the `!= 0u` SPIRV-Cross emits:

```glsl
if (_Globals.UseIntegerPositions != 0u)   ->   if (ps_uniforms_vec4[1].x != 0.0)
```

Only the boolean-test shape is modelled — an HLSL `uniform bool` used as a condition, which is
what the FX9 sources needing this actually write. Integer ARITHMETIC needs `int()` conversions and
int literals through the whole surrounding expression plus the `uint` locals SPIRV-Cross declares
alongside, so an arithmetic use fails loudly, naming the uniform, rather than being half-lowered
into something that compiles and computes the wrong thing. Vector forms (`ivec4`, `uvec2`) still
fail loudly.

> **Worth knowing, and not ShadowDusk's to fix.** MonoGame stores a Bool parameter's value as
> `int[]`, and `ConstantBuffer.SetData` `Buffer.BlockCopy`s the raw bytes, so integer `1` reaches
> the shader as float bit-pattern `0x00000001` — a denormal, not `1.0`. `!= 0.0` is true for it in
> strict IEEE, but a GPU that flushes denormals to zero reads the bool as false. This is exactly
> what mgfxc + MonoGame ship today, so matching it is the correct behaviour for a drop-in
> replacement; it is recorded here because it is a real hazard, not because this patch introduces
> it.

### 03c — explicit `layout(offset = N)` on uniform-block members

**Symptom.** `Unsupported uniform-block member ... 'layout(offset = 160) mat4 MatrixPalette[56];'`
— a member shape that is only unusual in carrying the qualifier.

**Cause.** SPIRV-Cross emits explicit offsets whenever the block's std140 layout is not the
natural one; the member regex did not allow the prefix, so every member of such a block was
rejected.

**Fix.** Accept the qualifier and **ignore** the offsets. They describe the D3D constant-buffer
byte layout, which nothing downstream reads: the register index is assigned in declaration order,
and the `.mgfx` cbuffer record the pipeline writes is built from that same list (parameter offset
= `BaseRegister * 16`), so the two sides agree by construction. Vehicle.fx is the proof that
honouring them is not even an option — SPIRV-Cross emits all 35 of its members with explicit
offsets, in USE order (552, 556, 560, 564, 576, then back to 80, 96, …).

### 03d — integer vertex attributes

**Symptom.** `stage interface identifier 'in_var_BLENDINDICES0' survived the I/O rewrite`
(skinFX), `'in_var_TEXCOORD1'` (RoundLine) — the declarations are `ivec4` and `int`.

**Fix.** A D3D9 vertex attribute is always float on the GL side: mgfxc declares every attribute
`attribute vec4 vs_v{k}` whatever the HLSL type — verified against this project's skinFX golden,
whose `int4 BlendIndices : BLENDINDICES0` is `attribute vec4 vs_v2`. So an integer input is a
declaration to CONVERT at its uses, not one to reject:

```glsl
in ivec4 in_var_BLENDINDICES0;  ->  attribute vec4 vs_v2;   … uses become ivec4(vs_v2)
in int   in_var_TEXCOORD1;      ->  attribute vec4 vs_v1;   … uses become int(vs_v1.x)
```

The conversion is always parenthesized, so a trailing swizzle on the use stays well-typed.
`uint`/`uvec` fail loudly: GLSL ES 1.00 and GLSL 1.10 have no unsigned integer type. An integer
PIXEL-stage input also fails loudly — SM3 varyings are floating point and carry no flat-
interpolation qualifier, so one cannot be expressed.

---

## 04 — run the FX9 pre-parse AFTER include flattening

**Symptom.** DXC errors pointing into a `.fxh` header, with nothing connecting them to the rewrite
that should have fired:

    OutlineShader.fxh(22,11): error X0000: Unsupported intrinsic.
    ModelShader.fxh(28,70): error X0000: invalid semantic 'COLOR' for ps 6.0

**Cause.** `CompilationPipeline` ran `FxPreParser.Parse(hlslSource)` on the **entry file only**,
and flattened `#include`s afterwards. The pre-parse is what rewrites every legacy FX9 construct —
`texture` / `sampler_state` declarations, `tex2D` calls, the `: COLOR` return semantic — and it
can only rewrite what it can see. Every one of those constructs reached DXC untouched when it
lived in a header.

Splitting a shader across `.fxh` headers is ordinary practice, so this affected whole shaders
rather than edge cases: four of this project's nineteen effects failed for no reason other than
which FILE their sampler or their PS entry sat in.

**Fix.** Flatten first, pre-parse the flattened text. Flattening is a pure text operation (prepend
macros, inline `#include`) with no dependency on the strip, so the order is free to be this way
round. The pre-parse then feeds its stripped text back into the same `PreprocessedSource` rather
than re-flattening (which would double-prepend the macros) — the pattern the zero-technique
fallback further down already uses.

**Cost.** `FxPreParser`'s own diagnostics now carry line numbers in the flattened text rather than
the entry file. Worth fixing with a `#line` map; not fixed here.

---

## 05 — pixel entries that write through an `out` parameter

**Symptom.** `error: Semantic COLOR is invalid for shader model: ps`, naming a struct whose
members carry `: COLOR0` / `: COLOR1`.

**Cause.** `GlStructOutputColorRewriter` retargets a PS output struct's `: COLOR<n>` members to
`: SV_Target<n>`, but resolves the struct from the entry's RETURN TYPE only. The other D3D9 form
is a `void` entry writing through an `out` struct parameter:

```hlsl
void RenderEmitters(in VertexShaderOutput input, out EmitterPixelShaderOutput output)
```

Its return type is `void`, which matches no struct. Worse, the existing safety check —
"never rewrite a struct that is also used as a function parameter type, because there its COLOR0
is a valid input interpolant" — would have vetoed the struct even if it were found, since an `out`
parameter *is* a parameter position.

**Fix.** Resolve output structs from a PS entry's `out`-qualified parameters as well as its return
type, and narrow the safety check to INPUT parameter positions. A struct that is an `out`
parameter of a pixel entry and never an input parameter anywhere is unambiguously an output
struct. `inout` is deliberately not treated as an output: it is both, so its semantics cannot be
retargeted without breaking the input side, and it is left for the loud DXC error.

---

## 06 — the GL target asks DXC for a GL cbuffer layout

**Symptom.** `error SD0100: SPIRV-Cross [compile]: Buffer block cannot be expressed as any of
std430, std140, scalar, even with enhanced layouts.` (multiTex, water — a hard failure with no
indication of which member is at fault.)

**Cause.** The OpenGL target passed `-fvk-use-dx-layout`, so DXC emitted **D3D constant-buffer
packing** offsets. HLSL packing lets a `float3` start at offset 4 when it fits in the remaining 12
bytes of the register; std140 requires a `vec3` to be 16-byte aligned. The result is a block
SPIRV-Cross cannot express as a GLSL uniform block at all — and which shaders hit it is decided by
nothing more than the order their globals happen to be declared in.

**Fix.** Pass `-fvk-use-gl-layout` for the OpenGL target, so DXC emits std140/std430-conformant
offsets. The offsets themselves are never read on this path — as patch 03c describes, the GL
register layout is assigned in GLSL declaration order and the `.mgfx` cbuffer record is built from
that same list — so this changes which shaders COMPILE and nothing else. Every other SPIR-V target
keeps `-fvk-use-dx-layout`: they do read the offsets.

Confirmed empirically: the 13 effects that compiled before this flag change are byte-identical
after it.

**A related note.** `IndexOfParamByRegister` bridges a GL uniform to its parameter by assuming
`reflected StartOffset == BaseRegister * 16`. That assumption already did not hold under DX layout
(Vehicle's members start at 552, 556, 560 …), and does not hold under GL layout either. It is a
fallback that only fires when SPIRV-Cross renames a uniform whose name collides with a GLSL
reserved word, so nothing here depends on it — but it is not sound, and this patch does not make
it so.
