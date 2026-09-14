# Building

The port targets **.NET 8** and **MonoGame 3.8.5.1 / DesktopGL**, and builds on Windows, Linux
and macOS. DesktopGL is the only backend that ships: one build serves all three platforms, and
the effects are OpenGL-profile everywhere, so **content is built once and used by every
platform**.

## What you need

| | |
|---|---|
| .NET 8 SDK | `dotnet --version` should report 8.x |
| your own copy of the game | for the content only — see below |
| a POSIX shell | the scripts are `sh`; on Windows use the Git Bash that ships with Git |

You do **not** need Visual Studio, the Windows SDK, `fxc`, or Wine. That last one is recent and
deliberate — see *Shaders*, below.

## The short version

```sh
# 1. code
dotnet build base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=GL

# 2. shaders  (needs the patched compiler; fetched and built for you)
sh tools/shadowdusk/build-shadowdusk.sh
export UW_SHADOWDUSK="$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe"
sh tools/build/34-build-gl-effects-shadowdusk.sh

# 3. package
sh tools/build/60-package-gl.sh Release
```

The result is `artifacts/package/UnclaimedWorld-GL/` — a self-contained, runnable game
directory.

## `UwPlatform`

| value | backend | notes |
|---|---|---|
| `GL` | MonoGame DesktopGL | **what ships.** Windows, Linux, macOS |
| `DX` | MonoGame WindowsDX | Windows only. Kept as a correctness oracle to compare renders against |
| `FNA` | FNA | a spike, on the `fna-backend` branch of the porting repository, not here |

## Building mods in or out

**Booleans, not a list:**

```sh
dotnet build … -p:UwHarmony=true -p:UwUnhiddenMod=true -p:UwGameplayMods=true  # default: all
dotnet build … -p:UwHarmony=false -p:UwUnhiddenMod=true                        # no external mod loading
dotnet build … -p:UwHarmony=false -p:UwUnhiddenMod=false                       # no bundled mod either
dotnet build … -p:UwGameplayMods=false                                         # none of the six
```

Not a list, because a list cannot be passed safely: both `;` and `,` delimit *properties* to the
`dotnet` CLI itself, so `-p:UwFeatures="harmony;unhiddenmod"` fails with
`MSBUILD : error MSB1006: Property is not valid` no matter how it is quoted. Only `%3B`-escaping
survives, which is not something to ask a human or an installer script to remember.

`-p:UwFeatures=harmony` is still accepted for a single feature. When both forms are given the
boolean wins — a command-line property is global, and MSBuild will not let the project reassign
it.

Dropping `harmony` is a real guarantee, not a flag: that build references no HarmonyLib and ships
no `0Harmony.dll`, so it **cannot** load third-party assemblies. A loader exists to run other
people's code, and someone who does not want that should be able to have a build that can't.

### Core does not depend on mods

Delete `mods/` and build with no flags at all. The two mod defaults follow the directory, so
this just works:

```sh
rm -rf mods
dotnet build base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=GL
```

`.github/workflows/publish.yml` runs exactly that on every push, in its own job so a deleted
`mods/` cannot leak into anything else. Compiling is not the claim, so it then searches the built
assembly for two strings that exist only inside a gameplay mod — plus one of the studio's own as
a control, because a check that stops finding anything has stopped checking.

Every feature has an `.Absent.cs` stub next to the framework in
`base_game/UnclaimedWorld/UWGame/Mods/`, with `const bool Enabled = false`, so the game's own call
sites are byte-identical whether a feature is in or out — no `#if` at forty call sites.

What a stub must never do is return something merely *harmless*. It returns **the studio's own
answer**: `HealingMod.Absent.RateFactor` returns `1` (not `0`, which would stop healing), and
`MagnificationMod.Absent.Clamp` carries the studio's `ClampBottom(x, 1f)` rather than the
identity. Each one is the expression that stood in the original source before the mod replaced
it, and the mod's own comment names it.

A bugfix in a core path a mod happens to reach stays in core regardless — it is a fix to the
game, not a piece of the mod. `SimProcess.CreateOutputsFromInputs` is the worked example: a no-op
on the studio's tables, and the thing that stopped the disassembly mod destroying items.

## Shaders

The game's effects are D3D9-era HLSL and have to be recompiled for the OpenGL profile. MonoGame's
own `mgfxc` P/Invokes `d3dcompiler_47.dll` **even for the OpenGL profile**, so it needs Windows
or Wine — which is why this used to be a Windows-only step.

It no longer is. `tools/shadowdusk/` builds a patched [ShadowDusk](https://github.com/kaltinril/ShadowDusk),
whose OpenGL path is `HLSL → DXC → SPIR-V → SPIRV-Cross → GLSL` with natives for win-x64,
linux-x64, osx-x64 and osx-arm64. All 19 effects compile from the studio's sources **unmodified**.

Nine gaps in ShadowDusk stood in the way; the six patches that close them, each with its reasoning
and a minimal repro, are in `tools/shadowdusk/patches/`. They belong upstream and are not specific
to this game.

The compiler is **pinned to a commit** (`UW_SHADOWDUSK_REF`) so an upstream force-push cannot
silently change what your shaders compile to. Bump it deliberately, and re-run the render
comparison when you do.

## What is committed and what is built

Committed: source, shader **sources** (`assets/effects/*.fx`), scenarios, string tables, tooling.

Built: everything under `artifacts/`, and all compiled content — `.xnb`, the effects, the media.
A compiled asset in git makes every texture tweak a binary diff, so none are.

## Verifying without launching

```sh
dotnet build tools/ContentProbe/ContentProbe.csproj -c Release -p:UwPlatform=GL
artifacts/bin/ContentProbe/release_gl/contentprobe.exe <package>/Content --all-effects
```

It loads every asset through a real `ContentManager` on a real `GraphicsDevice` and reports
pass/fail per asset — so a content problem is diagnosed without driving the UI. Pass
`-p:UwPlatform=GL` and run the `release_gl` binary: effects are profile-specific, and a DX probe
rejects GL effects with *"This MGFX effect was built for a different platform!"* whether or not
they are correct.

`tools/build/80-verify-modloader.sh` checks the mod loader the same way.
