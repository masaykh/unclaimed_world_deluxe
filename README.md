# Unclaimed World Deluxe

**1.0**

A community port of [*Unclaimed World*](https://store.steampowered.com/app/284100/) — the
survival colony sim by **Refactored Games** — from its shipped .NET Framework 4.5.1 / MonoGame 3.6
build onto **.NET 8 + MonoGame 3.8.5.1 / DesktopGL**. It runs natively on Windows, Linux and
macOS, can be rebuilt from source, and ships a set of gameplay mods you can turn off.

> **This is an unofficial community project. It is not endorsed by, affiliated with, or supported
> by Refactored Games.** Please do not report problems with this build to them.
> See [license.md](license.md).

**You need to own the game** — for its `Content/`, and only that.

Refactored Games released the game's source *and* its assets under the Community License, so a
fair amount of it is here: the shader sources, the menu animation, all 21 maps, the string table.
What is **not** here is the compiled `Content/` — the textures, audio and models the content
pipeline produces. You copy that one directory from your own install. Your Steam copy is never
modified.

## Getting it

**Download** a release archive for your platform, copy `Content/` and `data/` from your own
install next to the executable, and run it. Each archive carries an `install.md` with the details,
including the [.NET 8 runtime](https://dotnet.microsoft.com/download/dotnet/8.0) requirement and
— for macOS, which ships unsigned — the Gatekeeper command.

**Or build it**, on any of the three platforms: [build.md](build.md).

## What is different from the stock game

The port itself is meant to be invisible: same game, running on a current runtime, on three
platforms instead of one. What you will actually notice is the mods — **seven of them, on by
default, each individually switchable** in the options menu under **MODS**, or all off with
`-nomods`.

| mod | what it changes |
|---|---|
| **Unhidden** | EDIT and TEST buttons the studio implemented and never wired up, user scenarios in the picker, newest-first saves, wheel-scrolling data sheets, `O` for shadows — plus charcoal from peat and a resource-respawn fix |
| **Healing** | wounds can heal fully rather than stopping halfway, at a rate set by food, sleep and morale rather than muscle energy alone |
| **Self-preservation** | a colonist not in a Bold stance, and an animal nobody is already handling, stop going looking for fights nobody ordered |
| **Disassembly** | salvage recipes generated from the recipes that built the thing, so metal tools, weapons, furniture and textiles come apart — not just the handful the studio hand-wrote |
| **Balanced diet** | meat carries protein, plants carry micronutrients, only a cooked meal carries both; one food source stops feeding a colonist indefinitely |
| **Magnification** | zoom *below* 1, which the stock game clamps away: less screen for the interface, more for the world |
| **Map edge** | the camera stops at the edge of the map instead of allowing half a screen of empty grid |

Most exist because **Kastuk** asked for them, and each mod's source opens with the report that
prompted it. [how_to_use_mods.md](how_to_use_mods.md) has the switches and the two things worth
knowing about saves.

## How this repository is organised

The organising rule, and the one thing to understand before changing anything:

> **`base_game/` is the original game plus core work. `mods/` is everything that changes how the
> game plays.**

Core means the port — .NET 8, MonoGame 3.8.5.1, DesktopGL — and bugfixes. A crash is a bug; a
balance change is not, however clearly it was a mistake. `unhidden.capResourceRespawn` fixes a
clamp the studio computed and discarded, and it is *still* a mod, because it changes the food
economy of a running game.

| directory | what is in it |
|---|---|
| **`base_game/`** | the game: four projects, the port, and the modding *framework* |
| **`mods/`** | the gameplay mods, compiled in but individually switchable at runtime |
| **`assets/`** | asset sources a human edits — the `.fx` shaders and the menu animation |
| **`scenarios/`** | the studio's maps, as released |
| **`translations/`** | the string table — read `translations/README.md` first, it is smaller than it looks |
| **`tools/`** | the build scripts and the tools they drive |

Compiled shaders are a build output and are not committed: `assets/effects/*.fx` is the source,
and the build compiles it. Everything else in `Content/` comes from your own copy of the game —
the build never generates it and this repository never contains it.

## Documents

| | |
|---|---|
| [build.md](build.md) | building and packaging, on any of the three platforms |
| [modding.md](modding.md) | writing a mod: the hook points, the settings system, the data export |
| [how_to_use_mods.md](how_to_use_mods.md) | installing and switching mods, for players |
| [license.md](license.md) | which licence covers what, and what they require |

## Status, honestly

It runs, and is played end to end: new game, scenario load, save and load, terrain, models,
animation, the full HUD, the simulation. Every asset — 33 of 33, all 19 effects among them — is
checked to load on a real graphics device on every build, without launching the game.

What is **not** done:

- **`base_game/` is still the decompiled tree, not the studio's own source.** Not for want of the
  source: Refactored Games released it, it is public at
  [spunky44/UnclaimedWorld](https://github.com/spunky44/UnclaimedWorld), and it has been compared
  against this tree — 1801 types, 25,974 members, **zero differences at declaration level**. The
  port was simply built from ILSpy output first, and moving it onto the real source is pending
  work rather than a missing input. The whole delta is 51 files and ~3,400 lines, and the
  released source is in places *more* correct — MonoGame added `MathHelper.Max(int, int)`
  overloads XNA never had, so the decompiler resolved them and dropped casts the studio actually
  wrote.
- **Rendering is verified by eye, not measured.** Nobody has diffed this build's output against
  the original frame by frame, and a handful of cross-backend passes are known to differ slightly.
- **Steam achievements work on Windows only.** The Linux and macOS natives are not bundled; the
  game degrades gracefully without them.
- **The Linux and macOS archives are built but not yet play-tested** on those platforms.

CI builds and packages all four platforms on every push, and cuts the release archives from a tag.
