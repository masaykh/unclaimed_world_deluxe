#!/bin/sh
# Builds the release archives - one 7z per platform - into artifacts/release/.
#
#     sh tools/build/70-make-release.sh                 # version from git, all four platforms
#     sh tools/build/70-make-release.sh 1.0             # explicit version
#     sh tools/build/70-make-release.sh 1.0 linux-x64   # one platform
#
# RELEASES ARE CUT LOCALLY, NOT BY CI, and that is structural rather than a preference. A usable
# archive needs the compiled effects and the transcoded music, and both are built FROM YOUR COPY
# OF THE GAME - the effects reuse the shipped XNB containers and take every parameter's initial
# value from them, and the music is transcoded from the shipped WMAs. A runner has neither.
#
# There was a .github/workflows/release.yml that called this script. It could not work, and on
# the first real tag it failed four times over with the message below. Deleted rather than
# special-cased: a job that cannot do its job should not exist. CI still builds, cross-publishes
# every RID and checks the docs (publish.yml) - it just does not pretend to make releases.
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

# win-x64-dx is a PSEUDO-RID: win-x64 built against MonoGame.Framework.WindowsDX instead of
# DesktopGL. It exists because a recompiled shader can be rejected by a driver that would have
# accepted the studio's own - reported from an Intel HD Graphics machine on a 2016 driver, where
# skinFX's vertex shader failed to compile and every animated model took the game down. The DX
# build runs the studio's DXBC unmodified (tools/build/35-make-dx-effects.sh rewrites the
# container and copies the bytecode through), so it has no recompiled shader to be rejected.
#
# GL stays the default and the only cross-platform build. This is a fallback for old Windows
# graphics drivers, not a second first-class target.
RIDS=${*:-"win-x64 linux-x64 osx-x64 osx-arm64 win-x64-dx"}

OUT="$UW_REPO/artifacts/release"
STAGE="$UW_REPO/artifacts/release-stage"
rm -rf "$STAGE"
mkdir -p "$OUT"

# The two build outputs a release cannot do without. Overridable so a caller can point at an
# existing build rather than re-running the steps that produce them.
EFFECTS_SRC=${UW_GL_EFFECTS:-$UW_REPO/artifacts/content/effects-gl}
MEDIA_SRC=${UW_GL_MEDIA:-$UW_REPO/artifacts/content/media-gl}

# The DX archive's effects are the studio's own bytecode in a v10 container, not a ShadowDusk
# recompile. Different folder, different script, same override mechanism.
EFFECTS_SRC_DX=${UW_DX_EFFECTS:-$UW_REPO/artifacts/content/effects-dx}

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

  # The pseudo-RID split. Everything downstream reads $UWPLATFORM rather than testing the name
  # again, so one place knows what win-x64-dx means.
  case "$rid" in
    win-x64-dx) UWPLATFORM=DX; PUBRID=win-x64 ;;
    *)          UWPLATFORM=GL; PUBRID=$rid ;;
  esac

  # --- code -----------------------------------------------------------------------------------
  # NO -r FOR THE DX BUILD, and this is not a preference. MonoGame.Framework.WindowsDX depends -
  # mistakenly - on the legacy .NET Core 2.1 runtime packages, and publishing WITH a RID flattens
  # RID assets into the output root. That drops .NET Core 2.1's hostfxr.dll and hostpolicy.dll
  # beside the exe, the framework-dependent apphost loads that stale hostpolicy, and the process
  # dies immediately with 'Could not resolve CoreCLR path' (0x80008087) - no window, no log,
  # nothing at all. tools/build/40-deploy.sh has carried this warning since long before a DX
  # archive existed; this script shipped one with -r anyway, and the v1.1 DX archive cannot start
  # because of it. It reached a player, who spent an afternoon assembling a working build by hand.
  #
  # Without a RID those natives stay under runtimes/<rid>/native/ and the apphost uses the
  # installed runtime. DesktopGL has no such dependency, so GL keeps its RID.
  if [ "$UWPLATFORM" = "DX" ]; then
    "$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj \
      -c Release -p:UwPlatform=DX --self-contained false -o "$app" -v q --nologo
  else
    "$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj \
      -c Release -p:UwPlatform=GL -r "$PUBRID" --self-contained false -o "$app" -v q --nologo
  fi
  rm -f "$app"/*.pdb "$app"/*.dll.config
  # 16.3 MB of SharpDX IntelliSense docs and MonoGame's 936 KB XML, shipped by accident once.
  rm -f "$app"/SharpDX*.xml "$app"/MonoGame.Framework.xml
  # Belt and braces, should a RID-flattened asset ever reach the root again. apphost.exe is the
  # un-renamed template UnclaimedWorld.exe is made from and is pure confusion in a release; the
  # rest are .NET Core 2.1 host and debugger binaries.
  rm -f "$app"/apphost.exe "$app"/hostfxr.dll "$app"/hostpolicy.dll "$app"/dbgshim.dll \
        "$app"/SOS.NETCore.dll "$app"/sos*.dll "$app"/mscordaccore*.dll "$app"/mscorrc*.dll \
        "$app"/api-ms-win-*.dll "$app"/ucrtbase.dll "$app"/clretwrc.dll

  if [ "$UWPLATFORM" = "GL" ]; then
    # A DesktopGL build carrying SharpDX has picked up the WindowsDX backend. That would fail at
    # runtime, only on Linux and macOS, and only once a player ran it.
    if ls "$app"/SharpDX* >/dev/null 2>&1; then
      echo "  !! SharpDX in a DesktopGL build:" >&2; ls "$app"/SharpDX* >&2; exit 1
    fi
  else
    # The mirror of it: a WindowsDX build WITHOUT SharpDX has picked up DesktopGL, which would
    # leave the DX archive a second copy of the GL one - no use at all to the player it is for.
    ls "$app"/SharpDX* >/dev/null 2>&1 || {
      echo "  !! no SharpDX in a WindowsDX build - this is not a DX build" >&2; exit 1; }
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
  if [ "$UWPLATFORM" = "DX" ]; then
    EFFECTS=$EFFECTS_SRC_DX
    EFFECTS_HOWTO="sh tools/build/35-make-dx-effects.sh"
  else
    EFFECTS=$EFFECTS_SRC
    EFFECTS_HOWTO="sh tools/build/34-build-gl-effects-shadowdusk.sh  (see below)"
  fi

  # MUSIC IS A GL PROBLEM ONLY, and finding that out cost an archive. The port transcodes the
  # shipped WMA to Ogg Vorbis because DesktopGL has no MediaFoundation - but WindowsDX has
  # nothing else, and MediaFoundation cannot play Ogg. A DX archive built with the GL music in it
  # loads 32 of 33 assets and fails the 33rd with
  #
  #   SharpDXException HRESULT 0xC00D36C4 - "The byte stream type of the given URL is unsupported"
  #
  # So the DX build ships the studio's own .wma and their own Music XNBs, untouched, which is
  # what MediaFoundation is there for.
  missing_pc=""
  [ -d "$EFFECTS" ] || missing_pc="$missing_pc effects"
  if [ "$UWPLATFORM" != "DX" ]; then
    [ -d "$MEDIA_SRC" ] || missing_pc="$missing_pc music"
  fi
  if [ -n "$missing_pc" ]; then
    echo >&2
    echo "FATAL: cannot build a usable release - missing:$missing_pc" >&2
    echo >&2
    echo "  effects: $EFFECTS_HOWTO" >&2
    if [ "$UWPLATFORM" != "DX" ]; then
      echo "           sh tools/shadowdusk/build-shadowdusk.sh" >&2
      echo "           export UW_SHADOWDUSK=\"\$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe\"" >&2
    fi
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

    ( cd "$EFFECTS" && find . -name '*.xnb' -exec cp -p {} "$app/Content/{}" \; )
    if [ "$UWPLATFORM" != "DX" ]; then
      cp -p "$MEDIA_SRC"/Music/*.ogg "$app/Content/Music/" 2>/dev/null || true
      cp -p "$MEDIA_SRC"/Music/*.xnb "$app/Content/Music/" 2>/dev/null || true
      rm -f "$app"/Content/Music/*.wma        # dead weight: the stubs point at the .ogg now
    fi
    rm -f "$app"/Content/MainMenu/*.wmv       # no DesktopGL VideoPlayer; the .uwanim replaces it

    [ -f native/steam/steam_api64.dll ] && cp -p native/steam/steam_api64.dll "$app/" || true

    echo "    Content: $(find "$app/Content" -name '*.xnb' | wc -l) xnb  ($(du -sh "$app/Content" | cut -f1))"
  else
    mkdir -p "$app/port-content/Music"
    ( cd "$EFFECTS" && find . -name '*.xnb' -exec sh -c 'mkdir -p "$0/$(dirname "$1")" && cp -p "$1" "$0/$1"' "$app/port-content" {} \; )
    if [ "$UWPLATFORM" != "DX" ]; then
      cp -p "$MEDIA_SRC"/Music/*.ogg "$app/port-content/Music/" 2>/dev/null || true
      cp -p "$MEDIA_SRC"/Music/*.xnb "$app/port-content/Music/" 2>/dev/null || true
    fi
    echo "    port-content: $(find "$app/port-content" -name '*.xnb' | wc -l) xnb, $(ls "$app/port-content/Music"/*.ogg 2>/dev/null | wc -l) ogg"
  fi

  # --- licences and instructions --------------------------------------------------------------
  cp LICENSE-UnclaimedWorld-Community.md LICENSE-port-MIT.txt license.md how_to_use_mods.md "$app/"
  sh "$(dirname "$0")/_release-install-md.sh" ${FULL:+--full} > "$app/install.md"

  # The DX archive needs one paragraph the GL one does not, because a player who downloads it
  # has usually already been sent here by a crash.
  if [ "$UWPLATFORM" = "DX" ]; then
    cat >> "$app/install.md" <<'DXNOTE'

## This is the DirectX build

Take the ordinary `win-x64` archive unless it does not work. This one exists for older Windows
graphics drivers that reject a recompiled shader — the symptom is `Failed to compile vertex
shader` in `Errors.txt`, and every animated model taking the game down as soon as a map loads.

The difference is the shaders. The DesktopGL build recompiles all 19 effects from the studio's
HLSL; this one runs the studio's own compiled shaders, with only the container header rewritten
for MonoGame 3.8. Nothing else about the game differs.

Windows only, and it is not the build the port is developed against — if something is wrong here
that is right in the GL build, that is worth reporting rather than working around.
DXNOTE
  fi

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

  # THE ARCHIVE MUST BE STARTABLE, which is a different question from whether it contains files.
  # Both of these were true of the v1.1 DX archive and nothing noticed until a player could not
  # run it.
  [ -f "$app/UnclaimedWorld.exe" ] || [ -f "$app/UnclaimedWorld" ] || {
    echo "  !! no launchable UnclaimedWorld executable in $rid" >&2; exit 1; }
  for stale in hostfxr.dll hostpolicy.dll apphost.exe; do
    [ -f "$app/$stale" ] && { echo "  !! $stale beside the exe - this build will not start" >&2; exit 1; }
  done

  size=$(du -h "$OUT/$name" | cut -f1)
  echo "    $name  ($size)"
done

echo
echo "==> $OUT"
ls -1 "$OUT"/*.7z 2>/dev/null | sed 's|.*/|    |'
echo
echo "    publish with:  sh tools/build/71-publish-release.sh v$VERSION"
