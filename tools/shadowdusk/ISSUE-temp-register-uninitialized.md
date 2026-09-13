# FNA target: vkd3d emits reads of uninitialised temps, which MojoShader rejects

*(Text prepared for filing at https://github.com/kaltinril/ShadowDusk/issues — paste from the
horizontal rule down.)*

---

**Version:** ShadowDusk.Cli 0.20.0 (also reproduced on `main` @ `e4b1c878`)
**Target:** `/Profile:FNA`
**Host:** Windows 11 x64, .NET 8.0.420
**Consumer:** FNA (current `main`), FNA3D + MojoShader, via `ContentManager.Load<Effect>`

## Symptom

Two effects compile cleanly with `/Profile:FNA` but fail at load in FNA:

```
MOJOSHADER_compileEffect Error: Temp register r1 used uninitialized   (multiTex.fx)
MOJOSHADER_compileEffect Error: Temp register r3 used uninitialized   (Billboard.fx)
```

thrown from `FNA3D_CreateEffect` via `Effect..ctor` → `EffectReader.Read`. The other 17 effects in
the same set load fine.

## Cause

MojoShader validates temp-register initialisation **linearly, in token order** — this is in
`mojoshader.c`, in the destination-operand walk:

```c
RegisterList *reg;
reg = set_used_register(ctx, info->regtype, info->regnum, 0);
// !!! FIXME: Microsoft's shader validation explicitly checks temp
// !!! FIXME:  registers for this...do they check other writable ones?
if ((info->regtype == REG_TYPE_TEMP) && (reg) && (!reg->written))
    failf(ctx, "Temp register r%d used uninitialized", info->regnum);
```

vkd3d-shader emits streams in which a temp is *sourced* before any instruction names it as a
destination. The D3D9 runtime never enforced this and fxc-era shaders relied on that, so it only
surfaces through MojoShader — i.e. only on the FNA target.

It is not the input HLSL. Both shaders initialise correctly at source level
(`PixelShaderOutput Output = (PixelShaderOutput)0;`, `float alphaNoise1 = 0;`), and both compile
and render correctly through MonoGame's own `mgfxc` from the same sources.

## Reproduction

I have not managed to reduce this to a small shader — the constructs involved are ordinary and my
attempts to shrink it stopped reproducing. The two real shaders do reproduce it 100% of the time.
They are redistributable (Unclaimed World Community License, which permits distributing the
original materials), so I can attach both, or point at them in the released source:
<https://github.com/spunky44/UnclaimedWorld> — `UnclaimedWorld/Content/multiTex.fx` and
`Billboard.fx`, with the technique `compile` profiles changed from `vs_4_0`/`ps_4_0` to
`vs_3_0`/`ps_3_0` (the FNA target requires SM2–3, `SD0300`).

```sh
ShadowDuskCLI multiTex.fx multiTex.xnb /Profile:FNA     # compiles fine
# then, in FNA:  Content.Load<Effect>("multiTex")       # throws
```

Given your FNA harness already covers this class of problem, it will likely pin the exact
instruction faster than I did.

## Suggested fix

This is a fourth member of the family `D3d9BytecodePatcher` already handles — it carries three
vkd3d/MojoShader canonicalisations found the same way. A pass that gives such temps a defined
value fixes it:

- Walk instructions in order, tracking which temps have appeared as a destination.
- When a temp is *sourced* before its first write, record it.
- Emit `sub rN, c0, c0` for each such register **once, in the declaration prologue** — after the
  `dcl`/`def` block, before the first real instruction.

**The prologue placement matters.** Emitting the initialiser immediately before the offending
read is the obvious implementation, and it is wrong: MojoShader validates linearly, but execution
is not linear. Inside a loop, a register written at the end of one iteration is legitimately read
at the start of the next, and the linear scan flags it — so initialising at the read re-zeroes
the accumulator on every iteration. That version loaded cleanly and rendered garbage: the whole
shadow pass became overlapping trapezoids and the diffuse pass a fan of shards. Hoisting fixes
it, and costs one instruction per affected register.

`sub rN, c0, c0` rather than a `mov` from a literal because it needs no new `def` constant — one
could collide with a slot the effect's constant table already uses — and it yields zero whatever
`c0` holds, including when `c0` is unbound and reads as zero. The value cannot change behaviour
that was defined before: by construction the register had no defined contents at that point.

A working implementation is attached as a patch against `e4b1c878`. It is conservative:
instructions using relative addressing are skipped rather than mis-parsed, and anything that
cannot be walked cleanly returns the input untouched.

**Result with the patch:** 19 of 19 effects compile and **33 of 33 assets load** under FNA, up
from 30 of 33 — and the game renders.

---

## Second observation — NOT minimised, may or may not be related

Posting separately unless you want it here.

`multiTex.fx` declares seven textures (`texture0`–`texture4`, `perlinTexture`, and `NormalMap`
from an included header). The FNA build exposes only `texture0`, `texture1`, `texture2` and
`perlinTexture`; `texture3`, `texture4` and `NormalMap` are absent from `Effect.Parameters`.
MonoGame's `mgfxc` keeps `NormalMap` from the same source.

`NormalMap` looks reachable — `SpriteShader.fxh`'s `ApplyLighting` samples it through
`NormalMapSampler`, and `RenderRocksPS` calls `ApplyLighting`, and the *other* uniforms from that
same header (`ShadowFactor`, `LightPosition`, `LightColor`, `AmbientColorForNormalMapping`) **are**
exposed. But I could not reproduce it in a reduced shader: a 7-texture case with the same
declaration order, an included-header sampler and the same `ApplyLighting` shape exposes every
texture correctly. So this may be legitimate dead-code elimination in the full shader rather than
a bug, and I would rather say so than file it as one.
