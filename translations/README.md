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
| `(SETTING CHOICE)<id>=<value>` | a dropdown's choices (`WEAPONS AND GOOD TOOLS`) - not numbers like `1/3`, date patterns or language names | the setting registry |
| `(COUNT)<plural English>` | a number with its noun - see "Numbers" below | every `Locale.Count(n, "...", "...")` |
| `(AREA)<key>`, `(AREA DESCRIPTION)<key>`, `(AREA PLURALNAME)<key>`, ... | the name, plural, descriptions and other text of everything in the data tables: `ITEM`, `STRUCTURE`, `TREE`, `CREATURE`, `ENTITY` (vehicles, robots), `PROCESS` (recipes and jobs), `RESOURCE`, `SKILL`, `PROFESSION`, `CATEGORY`, `DEGRADE`, `UPGRADE`, `STORAGE`, `TIER`, `ORDER`, `EFFECT`, `SUBSTANCE`, `SOIL`, `VEGETATION`, `SITE`, `EXPEDITION` and more | `Locale.DataTexts` |
| `(HELP)<key>`, `(TUTORIAL)<key>`, `(HELP)<key>/FlowElements[2].Text.Text` | help and tutorial topics: titles and pages (keep the `§L#COLORHEADER¤...§` colour marks) | the help and tutorial tables, and each scenario's tutorials |
| `(TALKACTION DEFAULTTEXT)<id>` | what colonists and others say, line by line | every scenario's action sets |
| `(EVENTACTIONDIALOG ...)`, `(WINGAMEACTION)<id>/...` | event dialogs, the win and lose screens | every scenario's events |
| `(PRESENTATIONTYPE)<key>/...Term`, `...TermTooltip` | the words for a condition, a level, a quality ("Signs of decay", "Low") | the property presentations |
| `(SCENARIO DISPLAYNAME)<name>`, `(SCENARIO DESCRIPTION)<name>`, `(SCENARIO)<name>/...` | the New Game screens: a scenario's name and description, its difficulties and options | the scenario headers |
| `<key>@<scenario>` | a text one scenario words differently from the base game or from another scenario: that scenario's own | each scenario's tables |

Text deep inside an entry is keyed by the path to it (`(PROCESS)cook/Thresholds[2].Term`) or by the
nearest object with an id of its own (a spoken line's). A translation is used only while the text is
still the English the template has for its key, so a scenario's own wording is never replaced by the
base game's translation.

Terrain features (rocks, moss, hills) are left out: their names are the names of their art
("sulfurrock", "S: Bird 1, single") and are not shown.

The interface - side panels, HUD windows, data sheets, the missions, trade, policy and tasks panels -
goes through `Locale` too (`(GUI)...`, 907 entries). Still English: the simulation's own sentences
(the log's "was injured", what a colonist is doing, why a task is blocked), the mods' messages, the
loading screen's progress lines, and the developer panels (LOAD REPLAY, the debug overlays).
`tools/build/37-unrouted.txt` counts any interface English that comes back, file by file.

## Adding a language

1. Copy `English (US).xml` to `<Language>.xml` here (for a player: into `data/BaseData/Strings/`).
2. Translate the `<Value>`s. Leave every `<Key>` exactly as it is.
3. Choose it in OPTIONS → MODS → PORT → LANGUAGE and restart the game.

Anything you leave out, or leave empty, shows in English - a partial translation works. A key you
write twice uses the last one. A file that cannot be read is reported and the game stays in English.

`{0}` and `{1}` in a value are where the game puts a number or a name: keep them. A value whose
`{0}`/`{1}` differ from the English is not used (the English is shown, and the log says which).

**Numbers.** A `(COUNT)` value lists the noun's forms separated by `|`. English has two
(`{0} day|{0} days`); give as many as your language has: two are "one" and "the rest", three
follow the Russian/Ukrainian rule - 1, 21, 31... / 2-4, 22-24... / the rest, with 11-14 in the third:
`{0} день|{0} дня|{0} дней`.

**Keeping up with the game.** The template grows as more of the game is routed. To see what your
file lacks, has that the game no longer uses, still has in English, or has a broken `{0}` in:

```sh
bash tools/build/37-make-strings.sh --compare <your file>.xml
```

**Seeing what is not translatable yet.** `bash tools/build/37-make-strings.sh --pseudo` writes
`artifacts/strings/Pseudo.xml`: every value marked `[Ж ... Ж]`, accented and 30% longer. Copy it into
the game's `data/BaseData/Strings`, choose Pseudo, and look: text without the marks is not routed
yet, and text cut off at either end will not fit a longer translation.

Letters outside ASCII need fonts that have them - see "Fonts for translations" in `docs/build.md`
(`UW_FONT_FACE_LCD=Play` for Cyrillic; the button and status-screen fonts get theirs from
supplements the same script builds).

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

`perl tools/build/37-strings.pl route FILE...` wraps the plain cases for you: a literal assigned to
a control's Text, ToolTip, Title, Summary or Caption, a literal argument of the interface's helpers
(buttons, headings, tooltips, list entries - the list is in that script), and a label returned or
given by a switch arm. It leaves `$"..."` and `"a" + b` for you to turn into a format, and keys
(`AddEntry`'s first argument, icon and sprite names) alone. The same rules are the ratchet.

The data tables are translated once they are complete (`GameData.PostDataCompleteInitialize` →
`Locale.TranslateData`). The game identifies everything by its key, never by its name: the locale
self-test translates every data text and checks that every number, flag and enum in all the tables
is unchanged, and `37-make-strings.sh` refuses a name compared with English in the simulation or a
mod.
