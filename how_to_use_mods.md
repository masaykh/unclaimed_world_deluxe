# Using mods

For players. If you want to *write* one, see [modding.md](modding.md).

## What ships with the game

This build includes a set of mods, **on by default**, each individually switchable. They are in
`mods/` in the source, and every one of them can be turned off without reinstalling anything.

| mod | what it changes |
|---|---|
| **Unhidden Mod** | EDIT and TEST buttons on the main menu, user scenarios in the picker, newest-first save sorting, mouse-wheel scrolling on data sheets, `O` toggles shadows — plus charcoal from peat and the resource-respawn fix |
| **Healing** | wounds may heal fully rather than stopping halfway, and recovery speed depends on food, sleep and morale rather than muscle energy alone |
| **Debug overlays** | the studio's own developer overlays - job scores, interest, ranges, path search, region maps - as switches, plus a picker for which of the 94 built-in test scenarios the main menu's TEST button loads, and a switch to record sessions for replay. All off by default |
| **State dump** | writes `StateDump.txt` beside the game - every colonist's position, health and current goal, sampled on a game-time interval. For checking a change by reading rather than watching. Off by default |
| **Self-preservation** | colonists are reluctant to take a fight nobody ordered - the wounded and the unarmed stay out entirely, and an animal waits for a person. They still defend the camp |
| **Disassembly** | salvage recipes generated from the recipes that made the thing, so metal tools, weapons, furniture and textiles can be taken apart instead of only the handful the studio hand-wrote |
| **Balanced diet** | meat carries protein, plants carry micronutrients, and only a cooked meal carries both — so one food source no longer feeds a colonist indefinitely and the kitchen stops being optional |
| **Magnification** | zoom *below* 1, which the stock game clamps away: the interface takes less of the screen and more of the world is visible. Costs fill rate — 0.5 is four times the pixels — so it warns when the interface will no longer fit |
| **Map edge** | stops the camera at the edge of the map. The stock rule allows half a screen of overscroll, so the map can be pushed until half the window shows the empty grid behind the world |

Most of these exist because **Kastuk** asked for them, and each mod's source opens with the
report that prompted it.

## Turning them on and off

**In the game:** options menu → **MODS**. Every setting is there, with a description.

**In a file:** `user/ModSettings.xml`, next to the game. One line per setting:

```xml
<setting id="unhidden.charcoalFromPeat" value="true" />
<setting id="unhidden.capResourceRespawn" value="true" />
<setting id="unhidden.culture" value="Invariant" />
```

**All of them at once:** launch with `-nomods`.

```
UnclaimedWorld.exe -nomods
```

That is the stock game: every bundled mod off.

## Two things worth knowing

**Some settings change what a save contains, and some do not.** A change to the interface — sort
order, scrolling, buttons — is invisible to a save. A change to *simulation content* is not:
`unhidden.charcoalFromPeat` adds a recipe, and a save made with it on names that recipe. Load it
with the setting off and the game will tell you rather than silently dropping it.

The game asks about this on load; `port.askAboutModsOnLoad` controls whether it does.

**`unhidden.capResourceRespawn` is a fix, but it is still a mod.** The studio's code computes a
clamp and then discards it — a pure function called as a statement — so a replenishing tile adds
a *fraction of its maximum* on top of what is already there rather than topping up to the
maximum. With a fraction of 1.0 the tile roughly doubles each time, and the maximum ratchets up
with it.

It is in the mod rather than in the port's own patches because it changes the food economy of a
running game, and the port's patches leave gameplay alone. That is the rule this whole repository
is organised by: a crash is a bug and gets fixed in the base game; a balance change is a mod,
however clearly it was a mistake.

## Installing someone else's mod

Drop the assembly in `user/Mods/`. It is loaded through [HarmonyLib](https://github.com/pardeike/Harmony)
at startup.

**This runs their code in your process**, with everything that implies. If you would rather have
a build that cannot do that at all, build with the `harmony` feature off — see
[build.md](build.md). It ships no `0Harmony.dll` and never looks at `user/Mods`.
