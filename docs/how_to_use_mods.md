# Using mods

For players. If you want to *write* one, see [modding.md](modding.md).

## What ships with the game

This build includes a set of mods, **on by default**, each individually switchable. They are in
`mods/` in the source, and every one of them can be turned off without reinstalling anything.

The main menu's **MODDING → SELECT MODS** lists them all, and any in `user/Mods`: what each does, who
made it, and a switch for the whole mod. A mod switched off there acts as the studio's game until
it is switched on again, and keeps your settings meanwhile. A mod that is on unfolds to **MOD
OPTIONS**, its individual settings - a change there is applied and saved at once. The same settings
are in OPTIONS → MODS, which is also where they are changed during a game; key bindings are only
there.

| mod | what it changes |
|---|---|
| **Peat** | a kiln turns dry peat into charcoal, and a campfire or a primitive kitchen can be built with peat instead of firewood |
| **Weather** | seasons and temperature, off by default. The sun is highest on the "Mid summer" day and lowest on "Mid winter" (the stock sun climbs all year and is highest in mid winter). The air follows a subtropical year: about 5 C on a mid-winter night to 12 C at noon, 17 C on a mid-summer night to 34 C at noon, cooler under cloud. Food and materials outside, and in storage that is not cooled, spoil faster in the heat and slower in the cold; the shelf-life tooltips show the mid-summer figure; the date panel's WEATHER NOW line (drag the panel open) shows the temperature |
| **Firewood from any wood** | wet firewood can be cut from sticks (3), water cane stems (2), shadeleaf bow staves (2) or shadeleaf canes (3), not only from spoak branches. Dried firewood stays in the woodpile, so the pile shows how much is left, and fires and kitchens take it from there; a woodpile keeps one dried batch while the next dries, and anything beyond that is hauled to a stockpile as before |
| **Vine nets** | a fishing net can be made from 2 vines, twice as slowly, as well as from cotton string |
| **Resource respawn cap** | shellfish and other resources that grow back are topped up to the most a tile has held, instead of multiplying every time they are harvested, and firewood that came with the map grows back once gathered, which the studio's game never lets it |
| **Main menu** | MAP EDITOR and TEST SCENE under the main menu's MODDING, and a test map shown whole |
| **Healing** | wounds may heal fully rather than stopping halfway, and recovery speed depends on food, sleep and morale rather than muscle energy alone |
| **Debug overlays** | the studio's own developer overlays - job scores, interest, ranges, path search, region maps - as switches, plus a picker for which of the 94 built-in test scenarios the main menu's TEST button loads, and a switch to record sessions for replay. All off by default |
| **State dump** | writes `StateDump.txt` beside the game - every colonist's position, health and current goal, sampled on a game-time interval. For checking a change by reading rather than watching. Off by default |
| **Self-preservation** | colonists are reluctant to take a fight nobody ordered - the wounded and the unarmed stay out entirely, and an animal waits for a person. They still defend the camp |
| **Disassembly** | salvage recipes generated from the recipes that made the thing, so metal tools, weapons, furniture and textiles can be taken apart instead of only the handful the studio hand-wrote |
| **Balanced diet** | meat carries protein, plants carry micronutrients, and only a cooked meal carries both — so one food source no longer feeds a colonist indefinitely and the kitchen stops being optional |
| **Magnification** | zoom *below* 1, which the stock game clamps away: the interface takes less of the screen and more of the world is visible. Costs fill rate — 0.5 is four times the pixels — so it warns when the interface will no longer fit |
| **Map edge** | stops the camera at the edge of the map. The stock rule allows half a screen of overscroll, so the map can be pushed until half the window shows the empty grid behind the world |
| **Hide gear** | gear made from tanned hide, wood and iron, off by default (a save with these items needs it on). A **backpack** lets a colonist haul up to 5 instead of 1, a **hand cart** up to 10 with no slowdown, both taken for hauling. A **cloak** gives a little comfort on long trips and at night. **Hide armour** is worn on patrol by Security and Hunting colonists only: it takes 30% of a cut or a bite to the body, arms and legs (less of a blow or a shot, nothing to the head), and makes them easier to notice. **Weapons kept at hand** keeps a weapon fetched for a fight in what a colonist wears, so the next haul does not leave it behind. **Fighters keep a full kit**: Security and Hunting colonists fetch a big weapon (a gun, a bow, a spear) and a small one (a knife, a machete, a hand axe), and armour once the security policy is at its third tier; missing them costs comfort until one fighter is kitted out (three with a threat near); in full kit they fight better and do other work more slowly. A fighter can be ordered out of their armour from their context menu |
| **Emergency eating** | off by default. A starving colonist with no food left eats raw ingredients, best first: crystal berries, glassy creeper pods, common oil tubers, raw fish, raw meat - and only while no proper food will do. Each costs comfort for a day (5%, 15%, 25%, 40%, 60%) and may give a **bad stomach** for a day (5% to 60% chance, rising the same way): slower walking (up to 20%), slower work (up to 30%), up to 20% less comfort, and energy at most 60%. Both show in the colonist's effects. The bad stomach and its chance have their own settings |

Most of these exist because **Kastuk** asked for them, and each mod's source opens with the
report that prompted it.

## Turning them on and off

**In the game:** options menu → **MODS**. Every setting is there, with a description.

Each category says **STABLE** or **TESTING** on its header, and each setting's description starts
with the same word. STABLE means the setting has been confirmed working in a real game; TESTING
means it has not been yet - most new settings start there. A category is STABLE only when every
setting in it is. The list of confirmed settings is kept in `ModSettings.cs`.

**In a file:** `user/ModSettings.xml`, next to the game. One line per setting:

```xml
<setting id="peat.charcoalFromPeat" value="true" />
<setting id="respawncap.capResourceRespawn" value="true" />
<setting id="port.culture" value="invariant" />
```

**All of them at once:** launch with `-nomods`.

```
UnclaimedWorld.exe -nomods
```

That is the stock game: every bundled mod off.

## Two things worth knowing

**Some settings change what a save contains, and some do not.** A change to the interface — sort
order, scrolling, buttons — is invisible to a save. A change to *simulation content* is not:
`peat.charcoalFromPeat` adds a recipe, and a save made with it on names that recipe. Load it
with the setting off and the game will tell you rather than silently dropping it.

The game asks about this on load; `port.askAboutModsOnLoad` controls whether it does.

**`respawncap.capResourceRespawn` is a fix, but it is still a mod.** The studio's code computes a
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
