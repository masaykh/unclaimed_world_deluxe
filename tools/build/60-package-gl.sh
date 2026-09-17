#!/bin/sh
# Assembles a self-contained, cross-platform DesktopGL package in
# artifacts/package/UnclaimedWorld-GL/.
#
# It deliberately FAILS if any effect is still missing an OpenGL build, because a package that
# is short even one effect does not degrade - MonoGame's Effect.ReadHeader throws
# "This MGFX effect was built for a different platform!" and the game dies at load. Better to
# refuse than to hand someone a broken build.
#
# Inputs, in overlay order:
#   1. the pristine Steam Content/ and data/            (original assets, untouched)
#   2. the OpenGL-profile effects   from tools/build/34-build-gl-effects-shadowdusk.sh
#   3. content/media-gl/**         from 32-convert-media.sh      (Ogg Vorbis music + stubs)
#   4. the GL publish output                                     (8 managed assemblies, no SharpDX)
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

if [ ! -d "$UW_STEAM/Content" ]; then
  echo "FATAL: no such content directory: $UW_STEAM/Content" >&2
  echo >&2
  # A variable set WITHOUT export looks right in your own shell - `ls $UW_STEAM/` lists the folder
  # perfectly - and is invisible to `sh script`, because that is a child process and a plain
  # assignment is not an environment variable. Kastuk lost an afternoon to exactly this.
  echo "  UW_STEAM is empty or does not exist. It has to be EXPORTED:" >&2
  echo >&2
  echo "      export UW_STEAM=\"/c/Program Files (x86)/Steam/steamapps/common/Unclaimed World\"" >&2
  echo >&2
  echo "  If your UW_GAME is the same folder:" >&2
  echo >&2
  echo "      export UW_STEAM=\"\$UW_GAME\"" >&2
  exit 1
fi

CFG=${1:-Release}
LOWER=$(echo "$CFG" | tr 'A-Z' 'a-z')
PUB="artifacts/publish/UnclaimedWorld/${LOWER}_gl"

# Which effect build to overlay, and what to call the package. The default is what
# 34-build-gl-effects-shadowdusk.sh produces. Both are overridable so two packages can be built
# SIDE BY SIDE from different effect builds and run against each other - everything but the 19
# effect files is identical between them, which is what makes that a fair comparison.
EFFECTS_DIR=${UW_GL_EFFECTS:-artifacts/content/effects-gl}
MEDIA_DIR=${UW_GL_MEDIA:-artifacts/content/media-gl}
PKG="artifacts/package/UnclaimedWorld-GL${UW_GL_PKG_SUFFIX:-}"

# ---- 1. gate on content completeness -------------------------------------------------------
EFFECTS="Billboard BloomCombine BloomExtract CloudShadows DevShape EdgeDetect GaussianBlur
         GUI/CRT GUI/LCD LightSourcesEffect multiTex OverlayEffect OverlayGroundSpriteEffect
         RoadsAndPaths RoundLine skinFX TimeOfDayAndLightsources Vehicle water"
missing=""
for e in $EFFECTS; do
  [ -f "$EFFECTS_DIR/$e.xnb" ] || missing="$missing $e"
done
if [ -n "$missing" ] && [ "$UW_ALLOW_INCOMPLETE_EFFECTS" != 1 ]; then
  echo "REFUSING to package: these effects have no OpenGL build yet." >&2
  for m in $missing; do echo "    $m" >&2; done
  echo >&2
  echo "MonoGame throws on a shader-profile mismatch, so a package missing any one of these" >&2
  echo "crashes at content load rather than degrading. Add the .fx source under" >&2
  echo "assets/effects/ and re-run tools/build/34-build-gl-effects-shadowdusk.sh." >&2
  echo >&2
  echo "To build anyway - for testing the parts that do not touch them - set" >&2
  echo "UW_ALLOW_INCOMPLETE_EFFECTS=1. The result is NOT shippable." >&2
  exit 1
fi
if [ -n "$missing" ]; then
  # An effect is loaded on first use, not at startup, so a package short a few of them still
  # gets as far as anything that does not need one. The three outstanding ones are all in-world
  # (terrain, animated models, vehicles), which is why the menus can be tested without them.
  echo "WARNING: packaging WITHOUT an OpenGL build of:$missing" >&2
  echo "         The game will throw when it first loads one. Menus are reachable; a game is not." >&2
  INCOMPLETE=1
fi

# ---- 2. binaries ---------------------------------------------------------------------------
echo "==> publishing GL ($CFG)"
"$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj -c "$CFG" -p:UwPlatform=GL -v q --nologo
[ -d "$PUB" ] || { echo "FATAL: $PUB not found" >&2; exit 1; }

# The directory's CONTENTS are cleared rather than the directory itself: on Windows anything
# holding it open - a running copy of the game, an Explorer window, a shell sitting in it - makes
# `rm -rf` fail with "Device or resource busy", and with `set -e` that aborts the whole package
# after the publish has already run.
mkdir -p "$PKG"

# Player state survives a rebuild. This directory is not only a build output - it is where the
# port is actually played and tested from, and wiping it every time threw away the tester's saves
# and their entire user/ModSettings.xml. That is its own kind of bug report: "the render trace
# produced no file" was simply port.renderTrace having been reset to its default by a repackage
# between switching it on and launching.
STASH="$PKG/../.uw-gl-userstate"
rm -rf "$STASH"; mkdir -p "$STASH"
for keep in user Saves Options.xml; do
  [ -e "$PKG/$keep" ] && mv "$PKG/$keep" "$STASH/" || true
done

find "$PKG" -mindepth 1 -maxdepth 1 -exec rm -rf {} + 2>/dev/null || true
echo "==> binaries"
cp -rp "$PUB"/. "$PKG"/
rm -f "$PKG"/*.pdb "$PKG"/UnclaimedWorld.dll.config

# Runtime droppings from the publish directory, which `dotnet publish` does not clean and
# `cp -rp` faithfully carries over WITH ITS ORIGINAL TIMESTAMP. Anyone who has ever launched the
# game out of artifacts/publish leaves an Errors.txt there, and every package built afterwards
# ships it looking like a report on the build you just made. That cost a real debugging session:
# a ten-day-old "The content file was not found" was read as current, and sent us looking for a
# font that was present and correct the whole time.
rm -rf "$PKG"/Errors.txt "$PKG"/RenderTrace.txt "$PKG"/RenderTrace \
       "$PKG"/Options.xml "$PKG"/user "$PKG"/Saves

# ...and only now put the tester's own state back, so a dropping copied out of the publish
# directory can never masquerade as it.
for keep in user Saves Options.xml; do
  [ -e "$STASH/$keep" ] && { mv "$STASH/$keep" "$PKG/"; echo "    kept $keep"; } || true
done
rmdir "$STASH" 2>/dev/null || true

# ---- 3. original assets --------------------------------------------------------------------
echo "==> original assets from the pristine install"
cp -rp "$UW_STEAM/Content" "$PKG/Content"
cp -rp "$UW_STEAM/data"    "$PKG/data"
cp -p  "$UW_STEAM/steam_appid.txt" "$PKG/" 2>/dev/null || true

# ---- 4. GL content overlays ----------------------------------------------------------------
echo "==> overlaying OpenGL effects"
( cd "$EFFECTS_DIR" && find . -name '*.xnb' -exec cp -p {} "$UW_REPO/$PKG/Content/{}" \; )

echo "==> overlaying Ogg Vorbis music"
cp -p "$MEDIA_DIR"/Music/*.ogg "$PKG/Content/Music/"
cp -p "$MEDIA_DIR"/Music/*.xnb "$PKG/Content/Music/"
# The WMAs are now dead weight - the patched stubs point at the .ogg files.
rm -f "$PKG"/Content/Music/*.wma
# ---- 4b. the animated menu background ------------------------------------------------------
# MonoGame has no DesktopGL VideoPlayer (still true in 3.8.5.1), so Content/MainMenu's WMV can
# never play on this backend. Without a replacement the menu falls back to a still image
# (PORT DEVIATIONS 7 and 11) - so the WMV is dropped and MainMenuIntro.uwanim goes in instead:
# the same footage as a motion-JPEG frame sequence, which MonoGame's ILMerged StbImageSharp
# already decodes on every backend with no new dependency.
#
# It loads from the GAME ROOT, not Content/ - see UWGame/Control/BackgroundScreen.cs.
#
# Committed as an asset rather than generated here because generating it needs a video decoder,
# which is a heavy thing to require of a build (and of CI) for one 4.7 MB file that changes
# never. It is derived from TauCetiMainMenu.wmv, which IS part of the released source, so it is
# redistributable - see license.md.
rm -f "$PKG"/Content/MainMenu/*.wmv

if [ -f assets/MainMenuIntro.uwanim ]; then
  cp -p assets/MainMenuIntro.uwanim "$PKG/"
  echo "==> menu animation: MainMenuIntro.uwanim"
else
  # Not fatal: the game treats a missing .uwanim as "use the still background", which is a
  # supported state rather than a broken one. Say so, because a silently still menu looks like
  # a bug to whoever tests it.
  echo "    ! assets/MainMenuIntro.uwanim not present - the menu will use the still background" >&2
fi

# ---- 5. Steam natives ----------------------------------------------------------------------
echo "==> Steam natives"
if [ -f native/steam/steam_api64.dll ]; then
  cp -p native/steam/steam_api64.dll "$PKG/"
  echo "    + steam_api64.dll (Windows)"
fi
for extra in libsteam_api.so libsteam_api.dylib; do
  [ -f "native/steam/$extra" ] && { cp -p "native/steam/$extra" "$PKG/"; echo "    + $extra"; }
done
if [ ! -f native/steam/libsteam_api.so ]; then
  echo "    ! libsteam_api.so / libsteam_api.dylib not present - Steam achievements will be"
  echo "      unavailable on Linux/macOS (the game degrades gracefully, see PORT DEVIATION 4)."
  echo "      Both ship in the Steamworks.NET standalone zip alongside steam_api64.dll."
fi

echo
echo "==> packaged: $PKG"
echo "    $(find "$PKG" -type f | wc -l) files, $(du -sh "$PKG" | cut -f1)"
echo "    managed assemblies: $(ls "$PKG"/*.dll | wc -l)"
echo "    SharpDX assemblies: $(ls "$PKG"/SharpDX*.dll 2>/dev/null | wc -l)"
if [ -n "$INCOMPLETE" ]; then
  echo
  echo "    NOT SHIPPABLE: no OpenGL build of$missing"
fi
