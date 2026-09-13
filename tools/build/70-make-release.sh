#!/bin/sh
# Builds the release archives - one 7z per platform - into artifacts/release/.
#
#     sh tools/build/70-make-release.sh                 # version from git, all four platforms
#     sh tools/build/70-make-release.sh 1.0             # explicit version
#     sh tools/build/70-make-release.sh 1.0 linux-x64   # one platform
#
# THIS IS THE SAME CODE CI RUNS. .github/workflows/release.yml calls this script rather than
# repeating it, so a local archive and a published one are built by one set of instructions and
# cannot drift apart. That mattered enough to be worth the indirection: two copies of a packaging
# recipe diverge silently, and the divergence only shows up in something a player downloads.
#
# Publishing is a separate script (71-publish-release.sh) because building is safe and publishing
# is not.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

# --full folds the game's compiled Content/ in, so the archive is standalone: extract and play,
# nothing to copy, no install steps. Everything in it traces to a source Refactored Games
# released - all 549 shipped assets were checked against their public repository, and every one
# has a released source - so distributing a build of them is what section 1 of the Community
# License grants.
#
# Without --full the archive carries code, data/ and the port-content overrides, and the player
# brings Content/ from their own copy.
FULL=""
for a in "$@"; do
  [ "$a" = "--full" ] && FULL=1
done
set -- $(echo "$@" | sed 's/--full//')

VERSION=${1:-}
if [ -z "$VERSION" ]; then
  VERSION=$(git describe --tags --abbrev=0 2>/dev/null | sed 's/^v//') || true
  [ -n "$VERSION" ] || VERSION="0.0-dev"
fi
shift 2>/dev/null || true

RIDS=${*:-"win-x64 linux-x64 osx-x64 osx-arm64"}

OUT="$UW_REPO/artifacts/release"
STAGE="$UW_REPO/artifacts/release-stage"
rm -rf "$STAGE"
mkdir -p "$OUT"

# The two build outputs a release cannot do without. Overridable so a caller can point at an
# existing build rather than re-running the steps that produce them.
EFFECTS_SRC=${UW_GL_EFFECTS:-$UW_REPO/artifacts/content/effects-gl}
MEDIA_SRC=${UW_GL_MEDIA:-$UW_REPO/artifacts/content/media-gl}

# 7-Zip goes by four different names depending on how it was installed. Find one rather than
# assume: this script is meant to run on all three platforms, same as the game.
SEVENZ=""
for cand in "$SEVENZIP" 7zz 7z 7za; do
  [ -n "$cand" ] || continue
  if [ -x "$cand" ] || command -v "$cand" >/dev/null 2>&1; then SEVENZ="$cand"; break; fi
done
if [ -z "$SEVENZ" ]; then
  echo "FATAL: no 7-Zip found. Install p7zip (apt install p7zip-full / brew install p7zip)" >&2
  echo "       or 7-Zip on Windows, or set SEVENZIP to its path." >&2
  exit 2
fi

echo "==> Unclaimed World Deluxe $VERSION"
echo "    platforms: $RIDS"
echo "    7-Zip:     $SEVENZ"
echo

for rid in $RIDS; do
  echo "==> $rid"
  app="$STAGE/$rid/UnclaimedWorldDeluxe"
  rm -rf "$STAGE/$rid"
  mkdir -p "$app"

  # --- code -----------------------------------------------------------------------------------
  "$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj \
    -c Release -p:UwPlatform=GL -r "$rid" --self-contained false -o "$app" -v q --nologo
  rm -f "$app"/*.pdb "$app"/*.dll.config

  # A DesktopGL build carrying SharpDX has picked up the WindowsDX backend. That would fail at
  # runtime, only on Linux and macOS, and only once a player ran it.
  if ls "$app"/SharpDX* >/dev/null 2>&1; then
    echo "  !! SharpDX in a DesktopGL build:" >&2; ls "$app"/SharpDX* >&2; exit 1
  fi

  # --- the assets Refactored Games released ---------------------------------------------------
  # data/ ships COMPLETE: both halves of it are in this repository (scenarios/ is the game's
  # data/Maps, translations/ is data/BaseData/Strings), so the player supplies Content/ and
  # nothing else.
  mkdir -p "$app/data/Maps" "$app/data/BaseData/Strings"
  cp -r scenarios/. "$app/data/Maps/"
  cp translations/*.xml "$app/data/BaseData/Strings/"

  # The menu background. Optional by design - without it the game uses the still image, which is
  # a supported state rather than a broken one - so this warns instead of failing.
  if [ -f assets/MainMenuIntro.uwanim ]; then
    cp assets/MainMenuIntro.uwanim "$app/"
  else
    echo "  ! assets/MainMenuIntro.uwanim missing - the menu will be a still image" >&2
  fi

  # --- port-content: WITHOUT THIS THE GAME DOES NOT START -------------------------------------
  # The player's Content/ cannot be used as it is, and both reasons are fatal rather than
  # cosmetic:
  #
  #   EFFECTS  the shipped ones are MGFX v8 (MonoGame 3.6) and DirectX-profile. MonoGame 3.8
  #            throws "This MGFX effect is for an older release of MonoGame" before it even gets
  #            as far as noticing the profile is wrong. Every model that uses one fails with it.
  #   MUSIC    the songs are WMA, which needs MediaFoundation. The port transcodes them to Ogg
  #            Vorbis and patches the XNB stubs to point at the .ogg.
  #
  # Both go in port-content/, which UwContentManager searches BEFORE Content/ - the mechanism
  # exists precisely so the player's own files are never modified.
  #
  # Verified the hard way: an archive without these, installed exactly as install.md says,
  # fails at startup with 24 of 33 assets unloadable.
  missing_pc=""
  [ -d "$EFFECTS_SRC" ] || missing_pc="$missing_pc effects"
  [ -d "$MEDIA_SRC" ]   || missing_pc="$missing_pc music"
  if [ -n "$missing_pc" ]; then
    echo >&2
    echo "FATAL: cannot build a usable release - missing:$missing_pc" >&2
    echo >&2
    echo "  effects: sh tools/shadowdusk/build-shadowdusk.sh" >&2
    echo "           export UW_SHADOWDUSK=\"\$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe\"" >&2
    echo "           sh tools/build/34-build-gl-effects-shadowdusk.sh" >&2
    echo "  music:   sh tools/build/32-convert-media.sh" >&2
    echo >&2
    echo "  Both read your own copy of the game, so they cannot run on a machine without it -" >&2
    echo "  which is why CI cannot cut a complete release and this script can." >&2
    exit 1
  fi

  if [ -n "$FULL" ]; then
    # --full: the compiled Content/ goes IN, and the overrides are written straight into it the
    # way tools/build/60-package-gl.sh does. No port-content/, because there is nothing left to
    # shadow - and no Songs problem either, since SongReader resolves an .ogg against the Content
    # root and here that is where it is.
    [ -d "$UW_STEAM/Content" ] || {
      echo "FATAL: --full needs the game's compiled Content/, and UW_STEAM is not a game folder." >&2
      echo "       UW_STEAM=$UW_STEAM" >&2
      exit 1
    }
    cp -rp "$UW_STEAM/Content" "$app/Content"
    cp -p  "$UW_STEAM/steam_appid.txt" "$app/" 2>/dev/null || true

    ( cd "$EFFECTS_SRC" && find . -name '*.xnb' -exec cp -p {} "$app/Content/{}" \; )
    cp -p "$MEDIA_SRC"/Music/*.ogg "$app/Content/Music/" 2>/dev/null || true
    cp -p "$MEDIA_SRC"/Music/*.xnb "$app/Content/Music/" 2>/dev/null || true
    rm -f "$app"/Content/Music/*.wma          # dead weight: the stubs point at the .ogg now
    rm -f "$app"/Content/MainMenu/*.wmv       # no DesktopGL VideoPlayer; the .uwanim replaces it

    [ -f native/steam/steam_api64.dll ] && cp -p native/steam/steam_api64.dll "$app/" || true

    echo "    Content: $(find "$app/Content" -name '*.xnb' | wc -l) xnb  ($(du -sh "$app/Content" | cut -f1))"
  else
    mkdir -p "$app/port-content/Music"
    ( cd "$EFFECTS_SRC" && find . -name '*.xnb' -exec sh -c 'mkdir -p "$0/$(dirname "$1")" && cp -p "$1" "$0/$1"' "$app/port-content" {} \; )
    cp -p "$MEDIA_SRC"/Music/*.ogg "$app/port-content/Music/" 2>/dev/null || true
    cp -p "$MEDIA_SRC"/Music/*.xnb "$app/port-content/Music/" 2>/dev/null || true
    echo "    port-content: $(find "$app/port-content" -name '*.xnb' | wc -l) xnb, $(ls "$app/port-content/Music"/*.ogg 2>/dev/null | wc -l) ogg"
  fi

  # --- licences and instructions --------------------------------------------------------------
  cp LICENSE-UnclaimedWorld-Community.md LICENSE-port-MIT.txt license.md how_to_use_mods.md "$app/"
  sh "$(dirname "$0")/_release-install-md.sh" ${FULL:+--full} > "$app/install.md"

  # --- archive ---------------------------------------------------------------------------------
  name="UnclaimedWorldDeluxe-$VERSION-$rid.7z"
  rm -f "$OUT/$name"
  # -mx=9: built once, downloaded many times.
  ( cd "$STAGE/$rid" && "$SEVENZ" a -t7z -mx=9 "$OUT/$name" UnclaimedWorldDeluxe/ >/dev/null )

  # An archive that looks like a release and is not is worse than a failed build. Check the
  # things a player would notice missing.
  "$SEVENZ" t "$OUT/$name" >/dev/null
  for want in UnclaimedWorld.dll install.md LICENSE-UnclaimedWorld-Community.md MapData.xml; do
    "$SEVENZ" l "$OUT/$name" | grep -q "$want" || { echo "  !! missing from archive: $want" >&2; exit 1; }
  done

  size=$(du -h "$OUT/$name" | cut -f1)
  echo "    $name  ($size)"
done

echo
echo "==> $OUT"
ls -1 "$OUT"/*.7z 2>/dev/null | sed 's|.*/|    |'
echo
echo "    publish with:  sh tools/build/71-publish-release.sh v$VERSION"
