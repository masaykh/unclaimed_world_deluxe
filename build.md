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

**Two booleans, not a list:**

```sh
dotnet build … -p:UwHarmony=true  -p:UwUnhiddenMod=true      # default: both
dotnet build … -p:UwHarmony=false -p:UwUnhiddenMod=true      # no external mod loading
dotnet build … -p:UwHarmony=false -p:UwUnhiddenMod=false     # neither
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

Each feature has an `.Absent.cs` stub with `const bool Enabled = false`, so the game's own call
sites are byte-identical whether a feature is in or out — no `#if` at forty call sites.

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
