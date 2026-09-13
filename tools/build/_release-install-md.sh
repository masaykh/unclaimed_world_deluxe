#!/bin/sh
# Writes the install.md that goes inside every release archive, to stdout.
#
# Its own file because it is the one piece of the release that a PLAYER reads, and it is shared
# by the local build (70-make-release.sh) and CI. Leading underscore: called by other scripts,
# not run directly.
cat <<'EOF'
# Unclaimed World Deluxe — install

**This is an unofficial community project. It is not endorsed by, affiliated with, or supported
by Refactored Games.** Please do not report problems with this build to them.

**You need to own the game** — for one folder.

This archive already has the maps, the string table and the menu animation, because Refactored
Games released those under the Unclaimed World Community License. What it does not have is the
compiled `Content/`: the textures, audio and models.

## Install

1. Find your copy of *Unclaimed World*. In Steam: right-click the game → *Manage* →
   *Browse local files*.
2. Copy the **`Content`** folder from there into this folder, next to the executable.
3. Copy the music across:

   ```sh
   # Linux / macOS
   cp port-content/Music/* Content/Music/
   ```
   ```bat
   :: Windows
   copy /Y port-content\Music\* Content\Music\
   ```

4. Run it.

`Content` is the only thing you need to bring — `data/` is already here.

**Why step 3.** The rest of `port-content/` overrides your files without touching them: the game
looks there first. Songs are the exception, and it is MonoGame's doing rather than a choice —
a song's `.xnb` holds the filename of its audio, and MonoGame resolves that name against
`Content/` no matter which folder the `.xnb` itself came from. So the `.ogg` files have to sit
beside your originals. Your `.wma` files are left where they are and simply stop being used.

Your Steam install is not modified. Nothing here writes to it.

## Running

| | |
|---|---|
| Windows | `UnclaimedWorld.exe` |
| Linux | `./UnclaimedWorld` — you may need `chmod +x UnclaimedWorld` first |
| macOS | `./UnclaimedWorld` — see Gatekeeper, below |

.NET 8 must be installed: <https://dotnet.microsoft.com/download/dotnet/8.0>
(the *Runtime* is enough; the SDK also works).

### macOS: Gatekeeper

This build is not code-signed — signing needs a paid Apple Developer account — so macOS will
refuse to run it until you say otherwise:

```sh
xattr -dr com.apple.quarantine .
chmod +x UnclaimedWorld
./UnclaimedWorld
```

## Mods

Seven gameplay mods ship with this build, **on by default** and individually switchable:
options menu → **MODS**, or `user/ModSettings.xml`. `-nomods` turns all of them off.

```
UnclaimedWorld.exe -nomods
```

`how_to_use_mods.md` describes each one, and the two things worth knowing about saves.

## Licensing

The game is under the Unclaimed World Community License; this port's own code is MIT. Both are
in this folder, and `license.md` explains which covers what. Non-commercial.
EOF
