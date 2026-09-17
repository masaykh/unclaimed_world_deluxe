# Unclaimed World Deluxe

**1.0**

A community port of [*Unclaimed World*](https://store.steampowered.com/app/284100/) — the
survival colony sim by **Refactored Games** — from its shipped .NET Framework 4.5.1 / MonoGame 3.6
build onto **.NET 8 + MonoGame 3.8.5.1 / DesktopGL**. It runs natively on Windows, Linux and
macOS, can be rebuilt from source, and ships a set of gameplay mods you can turn off.

> **This is an unofficial community project. It is not endorsed by, affiliated with, or supported
> by Refactored Games.** Please do not report problems with this build to them.
> See [license.md](license.md).

Refactored Games released *Unclaimed World*'s source **and its assets** under the Unclaimed World
Community License. That is what makes this possible, and it is why the whole game is here: 3343
textures, 247 models, 235 sounds, the music, the fonts, all 21 maps, the string table. Nothing is
held back and nothing needs fetching from elsewhere.

**Please [buy the game](https://store.steampowered.com/app/284100/)** if you have not. This
licence is a gift from its authors; the least it deserves is that people who enjoy their work pay
for it.

## Getting it

**Download** the release archive for your platform, extract it, and run it. There is nothing to
install and nothing to copy.

You will need the [.NET 8 runtime](https://dotnet.microsoft.com/download/dotnet/8.0). macOS builds
are unsigned — signing needs a paid Apple Developer account — so Gatekeeper needs a word;
`install.md` inside the archive has the command.

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
| **Debug overlays** | the studio's own developer overlays, the 94 built-in test scenarios and session recording, reachable again - all off by default |
| **State dump** | a text record of what the colony is doing, for checking a change without watching the screen - off by default |
| **Self-preservation** | colonists are reluctant to take fights nobody ordered, the wounded and unarmed stay out, and an animal waits for a person - without stopping them defending the camp |
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
| **`assets/`** | the studio's asset sources — textures, models, sounds, music, fonts, shaders — plus the menu animation. `assets/README.md` first: two files called `Billboard.fx` live here and they are not the same file |
| **`scenarios/`** | the studio's maps, as released |
| **`translations/`** | the string table — read `translations/README.md` first, it is smaller than it looks |
| **`tools/`** | the build scripts and the tools they drive |

**Compiled content is a build output and is not committed** — no `.xnb` anywhere. `assets/` holds
what you would edit.

The shaders build from source here, on any platform. The rest of `Content/` does not yet: MGCB
compiles 425 of 425 items and 30 of 30 for WindowSystem, but **3 of 50 models** — the other 47 are
ASCII FBX 6.1, which no version of Assimp reads. So the release archives ship the compiled
`Content/` rather than rebuilding it. `assets/README.md` has the detail.

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
- **The Linux and macOS archives are built but not yet play-tested** on those platforms. They are
  structurally correct — right natives, right assets, every asset loads — which is not the same as
  somebody having played them.
- **A from-source content build is 47 models short**, as above.

CI builds and packages all four platforms on every push. **Release archives are cut locally**, not
by CI: a complete archive needs the compiled `Content/`, and a runner has no copy of the game to
take it from. `tools/build/70-make-release.sh --full` is the command, and
`tools/build/71-publish-release.sh` uploads.
