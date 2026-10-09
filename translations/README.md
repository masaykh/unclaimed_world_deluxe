# Translations

The files here are installed as `data/BaseData/Strings/` (the release script and gate 80 copy
them). Each one is a language: its file name is what OPTIONS → MODS → PORT → **LANGUAGE** offers.

`English (US).xml` is the **template**: every string the game asks for, generated from the code.
Don't edit it by hand; `tools/build/37-make-strings.sh` rewrites it, and gate 80 fails when it is
out of date.

## What is translatable so far

Kastuk, "Translation", 2026-10-09: "Start with adding localisation keys to strings of main menu and
in-game menu, options. Then to names and descriptions of items." So, today:

| key | what | where it comes from |
|---|---|---|
| `(GUI)<English>` | the main menu, the in-game menu, the save/load window, OPTIONS | every `Locale.Text("...")` in the code |
| `(SETTING)<id>`, `(SETTING TIP)<id>` | each mod setting's label and tooltip in OPTIONS → MODS | the setting registry |
| `(SETTING GROUP)<mod id>` | the headings in OPTIONS → MODS | the setting registry |
| `(AREA)<key>`, `(AREA DESCRIPTION)<key>`, ... | the name, description and other text of everything in the data tables: `ITEM`, `STRUCTURE`, `TREE`, `CREATURE`, `ENTITY` (vehicles, robots), `PROCESS` (recipes and jobs), `RESOURCE`, `SKILL`, `PROFESSION`, `CATEGORY`, `DEGRADE`, `UPGRADE`, `STORAGE`, `TIER`, `ORDER`, `EFFECT`, `SUBSTANCE`, `SOIL`, `VEGETATION`, `SITE`, `EXPEDITION` and a few more | `Locale.DataTexts`: the tables listed there, their `Name`, `Description`, `ShortDescription`, `Text`, `Tooltip`, `DisplayName` and `Heading` |

Terrain features (rocks, moss, hills) are left out: their names are the names of their art
("sulfurrock", "S: Bird 1, single") and are not shown. Still written straight into the code - and
English until routed the same way - are the side panels, the HUD, help texts and events.
The choices inside a dropdown (`ALL THE WAY`, `1/3`) are stored values and are not translated yet.

## Adding a language

1. Copy `English (US).xml` to `<Language>.xml` here (for a player: into `data/BaseData/Strings/`).
2. Translate the `<Value>`s. Leave every `<Key>` exactly as it is.
3. Choose it in OPTIONS → MODS → PORT → LANGUAGE and restart the game.

Anything you leave out, or leave empty, shows in English - a partial translation works. A key you
write twice uses the last one. A file that cannot be read is reported and the game stays in English.

`{0}` and `{1}` in a value are where the game puts a number or a name: keep them.

Letters outside ASCII need fonts that have them - see "Fonts for translations" in `docs/build.md`
(`UW_FONT_FACE_LCD=Play` for Cyrillic).

## How it works

`UWGame/Locale.cs`. `Locale.Text(english)` looks up `"(GUI)" + english`; `Locale.Text(key,
english)` any other key. Both give the chosen language's value, or the English passed in - **the
English always comes from the code**, so a stale template can never change what an English player
sees, and neither call ever throws. (The studio's own `Get` threw for any key missing from this
folder; it had no callers, and is left as it was.)

To make more text translatable, wrap the literal: `label.Text = Locale.Text("SAVE GAME");`. A
variable part goes through a format string, never `+`, so the template has one fixed entry:
`string.Format(Locale.Text("Illegal width entered. {0} is maximum."), max)`. Then run
`bash tools/build/37-make-strings.sh` and commit the template with the change. The script refuses a
`Locale.Text` whose argument is built from pieces.

Item names are translated once the tables are complete (`GameData.PostDataCompleteInitialize` →
`Locale.TranslateItems`); the game identifies items by their key, never by name.
