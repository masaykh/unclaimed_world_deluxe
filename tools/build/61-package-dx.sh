#!/bin/sh
# Assembles a shippable WindowsDX package in artifacts/package/UnclaimedWorld-Win64/.
#
# This is the build that actually works today: .NET 8, MonoGame 3.8.5.1, original assets,
# Steam achievements intact. Contents:
#
#   * the GL build's counterpart binaries (16 assemblies incl. 9 SharpDX, which
#     MonoGame.Framework.WindowsDX still requires)
#   * the pristine Content/ with the 19 effects transcoded from MGFX v8 to v10
#   * the pristine data/ (294 PNG + 22 XML, loaded as loose files - no pipeline involved)
#   * steam_appid.txt and a steam_api64.dll matched to the pinned Steamworks.NET
#
# Requires .NET 8 on the target machine (framework-dependent, as the original required
# .NET Framework 4.5.1). Pass --self-contained to bundle the runtime instead.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

CFG=${1:-Release}
SELF_CONTAINED=${2:-}
LOWER=$(echo "$CFG" | tr 'A-Z' 'a-z')
PKG="artifacts/package/UnclaimedWorld-Win64"

EXTRA=""
PUB="artifacts/publish/UnclaimedWorld/${LOWER}_dx"
if [ "$SELF_CONTAINED" = "--self-contained" ]; then
  EXTRA="-r win-x64 --self-contained true"
  PUB="artifacts/publish/UnclaimedWorld/${LOWER}_dx_win-x64"
  echo "==> self-contained (bundles the .NET 8 runtime)"
fi

echo "==> publishing WindowsDX ($CFG)"
"$DOTNET" publish src/UnclaimedWorld/UnclaimedWorld.csproj -c "$CFG" -p:UwPlatform=DX $EXTRA -v q --nologo
[ -d "$PUB" ] || PUB=$(find artifacts/publish/UnclaimedWorld -maxdepth 1 -type d -name "${LOWER}_dx*" | head -1)
[ -d "$PUB" ] || { echo "FATAL: publish output not found" >&2; ls artifacts/publish/UnclaimedWorld >&2; exit 1; }

rm -rf "$PKG"; mkdir -p "$PKG"
echo "==> binaries from $PUB"
cp -rp "$PUB"/. "$PKG"/
rm -f "$PKG"/*.pdb "$PKG"/UnclaimedWorld.dll.config

# Runtime droppings the publish step never cleans - see the same removal in 60-package-gl.sh for
# what shipping a stale Errors.txt costs.
rm -rf "$PKG"/Errors.txt "$PKG"/RenderTrace.txt "$PKG"/RenderTrace \
       "$PKG"/Options.xml "$PKG"/user "$PKG"/Saves

echo "==> original assets"
cp -rp "$UW_STEAM/Content" "$PKG/Content"
cp -rp "$UW_STEAM/data"    "$PKG/data"
cp -p  "$UW_STEAM/steam_appid.txt" "$PKG/" 2>/dev/null || true
# 16.3 MB of SharpDX IntelliSense docs and MonoGame's 936 KB XML were shipped by accident.
rm -f "$PKG"/SharpDX*.xml "$PKG"/MonoGame.Framework.xml

# The menu animation that replaces the WMV video player (PORT DEVIATION 17). Generated from the
# pristine video into the GAME ROOT - Content/ is not touched, and the original .wmv stays in
# place, so this is purely additive and reversible by deleting one file.
echo "==> menu animation"
sh build/33-make-menu-animation.sh "$PKG/Content/MainMenu/TauCetiMainMenu.wmv" "$PKG/MainMenuIntro.uwanim" 2>&1 \
  | grep -E 'frames,|FATAL' || echo "    ! could not build MainMenuIntro.uwanim - the game will"
[ -f "$PKG/MainMenuIntro.uwanim" ] || echo "      use the still menu background instead (not fatal)"

# PORT DEVIATION 18. Was: transcode the 19 effects IN PLACE inside $PKG/Content. The converted
# effects now go in a sibling port-content/ folder that the game prefers at load time, so the
# game's own Content/ is shipped exactly as the studio made it.
echo "==> port-content override (effects converted to MGFX v10, Content/ left alone)"
sh build/34-make-port-content.sh "$UW_STEAM/Content" "$PKG/port-content" 2>&1 \
  | grep -E 'effect\(s\)|rewritten|valid MGFX|byte-identical|FATAL'

echo "==> Steam native matched to the pinned Steamworks.NET"
if [ -f native/steam/steam_api64.dll ]; then
  cp -p native/steam/steam_api64.dll "$PKG/"
else
  echo "    ! native/steam missing - run build/12-fetch-steam-natives.sh (achievements will"
  echo "      be unavailable; the game degrades gracefully, see PORT DEVIATION 4)"
fi

echo
echo "==> packaged: $PKG"
printf '    %s files, %s\n' "$(find "$PKG" -type f | wc -l)" "$(du -sh "$PKG" | cut -f1)"
printf '    managed+native assemblies at root: %s\n' "$(ls "$PKG"/*.dll | wc -l)"
