# Unclaimed World Deluxe

A community port of [*Unclaimed World*](https://store.steampowered.com/app/284100/) — the
survival colony sim by **Refactored Games** — from its shipped .NET Framework 4.5.1 / MonoGame 3.6
build onto **.NET 8 + MonoGame 3.8.5.1 / DesktopGL**, so it runs natively on Windows, Linux and
macOS and can be modified and rebuilt.

> **This is an unofficial community project. It is not endorsed by, affiliated with, or supported
> by Refactored Games.** Please do not report problems with this build to them. See
> [license.md](license.md).

**You need to own the game.** This repository contains code, build tooling, the studio's released
source and the data files that came with it. It does **not** contain the game's art, audio,
models or compiled content — those stay in your own Steam install.

## How this repository is organised

The organising rule, and the one thing to understand before changing anything:

> **`base_game/` is the original game plus core work. `mods/` is everything that changes how the
> game plays.**

Core means the port itself — .NET 8, MonoGame 3.8.5.1, DesktopGL — and bugfixes. A crash is a
bug; a balance change is not. If a change makes the game behave differently at the player's
level and it is not fixing something broken, it belongs in `mods/`, switchable, off-able, and
readable on its own.

| directory | what is in it |
|---|---|
| **`base_game/`** | the game: four projects, the port, the modding framework |
| **`mods/`** | gameplay mods, compiled in but individually switchable at runtime |
| **`assets/`** | asset **sources** a human edits — `.fx` shaders today |
| **`scenarios/`** | the studio's maps, as released |
| **`translations/`** | the string tables |
| **`tools/`** | the build scripts and the tools they drive |

Compiled content is a build output and is not committed. `assets/` holds what you would edit;
`Content/` is produced from it.

## Documents

| | |
|---|---|
| [build.md](build.md) | building and packaging, on any of the three platforms |
| [modding.md](modding.md) | writing a mod, the hook points, the settings system |
| [how_to_use_mods.md](how_to_use_mods.md) | installing and switching mods, for players |
| [license.md](license.md) | the Community License, and what it does and does not allow |

## Status

The port runs. It is played end to end — new game, scenario load, save and load, terrain, models,
animation, the full HUD, the simulation, Steam achievements.

Honest about what is not done: the render comparison between the two shader compilers has not
been measured, several cross-backend passes still differ slightly, and the rebase onto the
studio's released 1.0.4.7 source (rather than the decompiled tree this was built from) is in
progress. See `todo.md` in the porting repository for the live list.
