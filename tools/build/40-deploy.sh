#!/bin/sh
# Phase 7: publish the port and deploy it over game/ (the working copy of the install).
#
# Uses `dotnet publish` WITHOUT a RID, and copies the whole tree including runtimes/.
# Both details matter:
#
#  1. Steamworks.NET is a RID-specific package, so its assembly lives at
#     runtimes/win-x64/lib/netstandard2.1/Steamworks.NET.dll and deps.json points there.
#     A flat file copy misses it: "Could not load file or assembly 'Steamworks.NET'".
#     Hence `cp -rp`, not `cp *.dll`.
#
#  2. Publishing WITH `-r win-x64` flattens RID assets into the output root - and
#     MonoGame.Framework.WindowsDX (3.8.2 and 3.8.5.1 alike) mistakenly depends on the legacy
#     .NET Core 2.1 runtime packages (Microsoft.NETCore.App 2.1.30 -> DotNetHostPolicy /
#     DotNetHostResolver / Jit 2.0.8). That drops .NET Core 2.1's hostfxr.dll and
#     hostpolicy.dll next to the exe, so the framework-dependent apphost loads that stale
#     hostpolicy and dies with
#     "Could not resolve CoreCLR path" (exit 0x80008087). Without a RID those native assets
#     stay harmlessly under runtimes/<rid>/native/ and the apphost uses the installed runtime.
#     The cleanup below is belt-and-braces in case they ever reach the root again.
#
# game/ is disposable: it is a working copy of your install, and re-copying it from the
# pristine Steam folder is always safe.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"
CFG=${1:-Debug}
UWPLATFORM=${2:-DX}
LOWER=$(echo "$CFG" | tr 'A-Z' 'a-z')
PLAT=$(echo "$UWPLATFORM" | tr 'A-Z' 'a-z')
OUT="artifacts/publish/UnclaimedWorld/${LOWER}_${PLAT}"

echo "==> publishing ($CFG, $UWPLATFORM)"
"$DOTNET" publish src/UnclaimedWorld/UnclaimedWorld.csproj -c "$CFG" -p:UwPlatform="$UWPLATFORM" -v q --nologo
[ -d "$OUT" ] || { echo "FATAL: $OUT not found" >&2; ls artifacts/publish/UnclaimedWorld >&2; exit 1; }

echo "==> copying $OUT -> game/ (including runtimes/)"
cp -rp "$OUT"/. "$UW_GAME/"

echo "==> removing files the port supersedes"
for f in \
  UnclaimedWorld.vshost.exe UnclaimedWorld.vshost.exe.config UnclaimedWorld.vshost.exe.manifest \
  UnclaimedWorld.exe.config UnclaimedWorld.dll.config \
  CSteamworks.dll CSteamworks64.dll \
  MonoGame.Framework.xml \
  steam_api.dll \
  InputEventSystem.dll \
  RoundLines.dll
do
  [ -e "$UW_GAME/$f" ] && { rm -f "$UW_GAME/$f"; echo "    - $f"; }
done
# .NET Core 2.1 host binaries must never sit beside the exe (see note 2 above).
for f in hostfxr.dll hostpolicy.dll SOS.NETCore.dll; do
  [ -e "$UW_GAME/$f" ] && { rm -f "$UW_GAME/$f"; echo "    - $f (stale .NET Core 2.1 host binary)"; }
done
# The Steamworks natives must match the pinned Steamworks.NET (see 12-fetch-steam-natives.sh).
# The shipped SDK ~1.34 steam_api64.dll lacks SteamInternal_SteamAPI_Init and would make
# SteamManager.Initialize fall into its EntryPointNotFoundException guard, silently losing
# achievements.
if [ -f native/steam/steam_api64.dll ]; then
  cp -p native/steam/steam_api64.dll "$UW_GAME/"
  echo "    + steam_api64.dll (matched to Steamworks.NET)"
else
  echo "    ! native/steam missing - run build/12-fetch-steam-natives.sh for Steam achievements"
fi

# 16.3 MB of SharpDX IntelliSense docs were shipped by accident; dead weight at runtime.
rm -f "$UW_GAME"/SharpDX*.xml

echo "==> deployed"
