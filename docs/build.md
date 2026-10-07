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
export UW_STEAM="/c/Program Files (x86)/Steam/steamapps/common/Unclaimed World"   # your copy, only read
# (UW_STEAM, not UW_GAME: UW_GAME is the working copy 40-deploy.sh writes into)
sh tools/build/34-build-gl-effects-shadowdusk.sh   # runs step 35 first if its output is missing

# 3. package
sh tools/build/60-package-gl.sh Release   # runs step 32 (music to Ogg, needs ffmpeg) if not done
```

The result is `artifacts/package/UnclaimedWorld-GL/` — a self-contained, runnable game
directory.

### …and for DirectX

There is no `61-package-dx.sh`, and looking for one is the obvious mistake — `60-package-gl.sh`
is GL-only and refuses anything else. The DX package comes out of the release script, one
platform at a time:

```sh
# 1. code + effects  (the studio's own DXBC, rewritten into a v10 container - no recompile)
sh tools/build/35-make-dx-effects.sh

# 2. package
sh tools/build/70-make-release.sh 0.0-dev win-x64-dx          # code + data/ + port-content/
sh tools/build/70-make-release.sh 0.0-dev win-x64-dx --full   # the above plus Content/
sh tools/build/70-make-release.sh --folder 0.0-dev win-x64-dx # a plain folder to test, no 7-Zip
```

`70-make-release.sh` builds the code itself, so there is no separate `dotnet build` step — and
**do not** build DX with `-r win-x64` by hand, which is what the script is carefully avoiding;
see the comment at its `--- code ---` block. The archive lands in `artifacts/release/` and the
staged directory it was cut from is in `artifacts/release-stage/win-x64-dx/`, which is the one to
run from.

Without `--full` the archive has no `Content/` — you point it at your own copy, exactly as the
GL archives do. With `--full` it is standalone. Both need 7-Zip on `PATH` (or `SEVENZIP` set),
unless `--folder` is given: then each platform is left as a folder in `artifacts/release/`, for
local testing, and nothing is compressed.

## `UwPlatform`

| value | backend | notes |
|---|---|---|
| `GL` | MonoGame DesktopGL | **what ships.** Windows, Linux, macOS |
| `DX` | MonoGame WindowsDX | Windows only. The correctness oracle to compare renders against — and now also a shipped fallback archive. Build one with […and for DirectX](#and-for-directx) |
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

### The DirectX fallback archive

`tools/build/70-make-release.sh` cuts five archives, not four. `win-x64-dx` is win-x64 built
against `MonoGame.Framework.WindowsDX`, and it exists for one reason: **a recompiled shader can
be rejected by a driver that would have accepted the studio's own.**

Reported from an Intel HD Graphics machine on a 2016 driver — `Failed to compile vertex shader`
out of `SkinnedAnimatedModel.DrawModel`, so every animated model took the game down as soon as a
map loaded. ShadowDusk had in fact *warned* about that shader at build time (`SD0403`, twice on
`skinFX`), and the warning was shipped past.

The DX build has no recompiled shader to reject. `tools/build/35-make-dx-effects.sh` rewrites the
shipped v8 container to v10 and copies every byte of DXBC through unchanged — the bytecode is
the studio's.

Two things differ from the GL archive, both forced:

- **Music is the shipped `.wma`, not the transcoded Ogg.** DesktopGL has no MediaFoundation, so
  GL needs Ogg; WindowsDX has nothing *but* MediaFoundation, and MediaFoundation cannot play
  Ogg. A DX archive with the GL music in it loads 32 of 33 assets and fails the 33rd with
  `SharpDXException 0xC00D36C4`.
- **SharpDX ships with it.** The GL archive fails the build if SharpDX appears; the DX archive
  fails if it does *not*.

Verified with `tools/ContentProbe` against the staged archive: **33 of 33 assets load**, all 19
effects among them.

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

## Fonts for translations

The studio's fonts cover ASCII only. `tools/build/36-build-fonts.sh` rebuilds the five TrueType
ones from their `.spritefont` sources, with the same sizes and spacing, plus the Latin-1, Latin
Extended-A, Greek and Cyrillic letters each typeface actually has:

```sh
sh tools/build/36-build-fonts.sh                         # Arial, Arial Bold, Electrolize
UW_FONT_FACE_LCD=Play sh tools/build/36-build-fonts.sh   # Play replaces Electrolize (fetched, SIL OFL)
UW_FONT_FACE_LCD=Jura sh tools/build/36-build-fonts.sh   # or Jura
```

Copy the **contents** of `artifacts/content/fonts/gl` (DesktopGL) or `…/dx` (DirectX) into the
game's `port-content\` - the folder beside `Content\` - so that it holds
`port-content\Arial.xnb` and `port-content\Fonts\LCDandHUDBody.xnb` (a `port-content\dx\` folder
is never looked in). English text looks the same by design: it is the same typeface. The new
letters show only where text has them.

**Typing in another alphabet needs the LCD font.** Every text box - save names, map names, the
trade and task windows - draws with `Fonts/LCDandHUDBody`, which is Electrolize, and Electrolize
has no Cyrillic at all. A plain run of the script adds Cyrillic to the Arial fonts only, so typed
Russian still shows as `.` (the fallback). Build with `UW_FONT_FACE_LCD=Play` (or `Jura`). The
keyboard side needs nothing: since v1.4 a text box takes characters from the system's keyboard
layout (`GUIManager.Window_TextInput` → `TextBox.OnTextInput`), so switch Windows to the Russian
layout and type.

To check which fonts the game will actually load, without launching it, use the probe for the
platform you play - `release_dx` for DirectX, `release_gl` for DesktopGL - built **after** the game
for that platform (`dotnet build tools/ContentProbe/ContentProbe.csproj -c Release`):

```sh
artifacts/bin/ContentProbe/release_dx/contentprobe.exe "<game>/Content" Fonts/LCDandHUDBody Arial
#   override:     <game>\port-content (found)
#   OK  Fonts/LCDandHUDBody -> SpriteFont, ..., 612 glyphs, fallback='.', cyrillic=yes, greek=yes, from port-content
```

`cyrillic=yes` on `Fonts/LCDandHUDBody` is what typing needs. The stock fonts report
`cyrillic=no` and `from Content`. If the `override:` line says `NOT CONSULTED`, the
probe was built before the game and is reading `Content\` alone - build the game for that
platform, then rebuild the probe.

## Procedural maps

`MapGenMod` (in `mods/`) writes an ordinary map folder, `MapData.xml` plus its PNG layers, which
EDIT and TEST MAP open like a shipped map. Run it against your game folder:

```sh
artifacts/bin/DataExport/release/dataexport.exe "<game>" --generate-map=MyMap --seed=42 --size=80
```

The map lands in `<game>/data/Maps/MyMap`, which is the only place the map picker looks. The same
seed always gives the same map. Sizes run from 32 to 256; the shipped playable maps are 64, 80 and
128.

The generator learns from the shipped maps in `<game>/data/Maps`, or from `--maps-from=DIR`.
Each shipped map becomes an *ecosystem*: which rocks, plants, trees and resources it has in each
setting (deep or shallow water, shore, or land of a given soil), and how densely. The steps:

1. Water and rivers first, with fords so the land stays connected.
2. Then climate: rainfall, temperature and an "x factor".
3. Then biome regions (`MapGenMod.Biomes`), each at least a minimum size. Each region picks an
   ecosystem whose soils suit it, so the same biome can carry different species in different places.
4. Soil, vegetation and placement then follow the region's ecosystem. Every key written is one the
   game defines.

Gate 80 case 26 generates four maps and reads each back through the game's own `MapData` type.

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
