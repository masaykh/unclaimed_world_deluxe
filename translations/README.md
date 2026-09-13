# Translations

**Be warned before you start: this is a mechanism the studio built and barely used.** The file
here is what shipped, and it contains four entries — one of which is the `MyKey` / `MyValue`
example row.

Translating the game is therefore not "translate this file". Most of the game's text is not in
it.

## How it works

`UWGame/Locale.cs`:

- `Init()` loads `English (US)` as the **invariant** table, and makes it the current one.
- `GetCultures()` lists every `*.xml` in `data/BaseData/Strings/`, so a new file is a new
  selectable culture. **No code change is needed to add a language** — the file's name is the
  culture's name.
- `Get(key)` looks the key up in the current table and **falls back to the invariant one**. So a
  partial translation is legitimate: whatever is missing comes back in English.

The format is an `XmlSerializer` round-trip of `List<String>`, where `String` is the game's own
key/value pair type:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfString>
  <String>
    <Key>(GUI)CUSTOMIZE</Key>
    <Value>CUSTOMIZE</Value>
  </String>
</ArrayOfString>
```

Keys are prefixed by area — `(GUI)` for interface text — and, in the shipped file, the key is the
English string itself.

## The catch

`Get` throws `KeyNotFoundException` if a key is in neither the current table nor the invariant
one. The invariant table is the one here, with its four entries. So the fallback protects a
*partial translation*, not a *missing key* — adding `Get("(GUI)SOMETHING")` to the code without
adding the row here is a crash, not an English string.

That is a fair part of why the mechanism went unused: the cost of routing a string through it is
an entry in this file, every time.

## Adding a language

1. Copy `English (US).xml` to `<Culture Name>.xml` in this directory.
2. Translate the `<Value>` elements. Leave `<Key>` alone.
3. Build. `GetCultures()` will find it.

Anything you leave out falls back to English.

## If you want to translate the whole game

The larger job is **routing the text through `Locale` in the first place** — most strings are
literals in the source and in `data/`. That is a base-game change, not a translation, and it
should be done area by area with the keys added here as it goes.
