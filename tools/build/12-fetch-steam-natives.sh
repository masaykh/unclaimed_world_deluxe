#!/bin/sh
# Fetches the Steamworks native redistributables that MATCH the Steamworks.NET version
# pinned in Directory.Packages.props, into native/steam/.
#
# Why this is needed: the game shipped Steamworks SDK ~1.34 natives alongside
# Steamworks.NET 7.0.0.0 and the CSteamworks shim. Current Steamworks.NET P/Invokes
# flat-API exports that generation does not have - the shipped steam_api64.dll has
# SteamAPI_Init but NOT SteamInternal_SteamAPI_Init, SteamInternal_CreateInterface or
# SteamAPI_ManualDispatch_Init. Managed and native must move together.
#
# These are Valve's freely redistributable Steamworks binaries, bundled with the
# MIT-licensed Steamworks.NET release. Every Steam game ships them. They are NOT
# committed to this repo - run this script to (re)fetch.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

VER=2024.8.0            # must match <PackageVersion Include="Steamworks.NET"> in Directory.Packages.props
URL="https://github.com/rlabrecque/Steamworks.NET/releases/download/$VER/Steamworks.NET-Standalone_$VER.zip"
WORK=$(mktemp -d)

echo "==> fetching Steamworks.NET $VER standalone package"
curl -sfL -o "$WORK/sw.zip" "$URL"

mkdir -p native/steam
"$SEVENZIP" e -y -o"native/steam" "$WORK/sw.zip" "Windows-x64/steam_api64.dll" "Windows-x86/steam_api.dll" >/dev/null
rm -rf "$WORK"

echo "==> native/steam:"
ls -la native/steam

echo "==> verifying the exports current Steamworks.NET requires"
STRINGS=/c/tools/msys64/usr/bin/strings
fail=0
for sym in SteamInternal_SteamAPI_Init SteamInternal_CreateInterface SteamAPI_ManualDispatch_Init SteamAPI_Shutdown SteamAPI_RunCallbacks; do
  if "$STRINGS" -a native/steam/steam_api64.dll | grep -qx "$sym"; then
    echo "    OK      $sym"
  else
    echo "    MISSING $sym"; fail=1
  fi
done
[ "$fail" = 0 ] || { echo "FATAL: fetched native does not match the pinned Steamworks.NET." >&2; exit 1; }
