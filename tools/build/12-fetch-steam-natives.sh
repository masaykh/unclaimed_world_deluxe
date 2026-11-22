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
# MIT-licensed Steamworks.NET release. Every Steam game ships them.
#
# native/steam/steam_api64.dll, libsteam_api.so and libsteam_api.dylib ARE COMMITTED to this
# repository - about 1.3 MB together, redistributable, and they change only when the
# Steamworks.NET pin does. This script is how they get
# REFRESHED, not how it arrives: a fresh clone already has them, and packaging needs no
# Steam install to put Steam integration in a build. (An earlier version of this comment
# said the opposite, and sent someone looking for a fetch step they did not need.)
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

VER=2024.8.0            # must match <PackageVersion Include="Steamworks.NET"> in Directory.Packages.props
URL="https://github.com/rlabrecque/Steamworks.NET/releases/download/$VER/Steamworks.NET-Standalone_$VER.zip"
# PORT: the Linux and macOS natives too. Steamworks.NET's linux-x64 and osx-x64 assemblies
# P/Invoke "steam_api", which .NET resolves to libsteam_api.so / libsteam_api.dylib. Until these
# were fetched a Linux build had no Steam at all (Kastuk, Ubuntu: DllNotFoundException).
# The dylib is universal (i386, x86_64, arm64).
MEMBERS="Windows-x64/steam_api64.dll Windows-x86/steam_api.dll OSX-Linux-x64/libsteam_api.so OSX-Linux-x64/steam_api.bundle/Contents/MacOS/libsteam_api.dylib"

# PORT: the unpacker. env.sh only finds 7-Zip at its Windows install path, so on Linux $SEVENZIP
# was empty and the unpack ran "" as a command -"12-fetch-steam-natives.sh: 31: : Permission denied" (Kastuk,
# Ubuntu 24.04). Any of these can pull two named members out of a zip, flattened.
UNZIP=""
for cand in "$SEVENZIP" 7zz 7z 7za unzip bsdtar; do
  [ -n "$cand" ] || continue
  if [ -x "$cand" ] || command -v "$cand" >/dev/null 2>&1; then UNZIP="$cand"; break; fi
done
if [ -z "$UNZIP" ]; then
  echo "FATAL: nothing to unpack a .zip with. Install one of 7-Zip, unzip or bsdtar" >&2
  echo "       (apt install unzip / brew install p7zip), or set SEVENZIP to 7-Zip's path." >&2
  exit 2
fi

WORK=$(mktemp -d)

echo "==> fetching Steamworks.NET $VER standalone package"
curl -sfL -o "$WORK/sw.zip" "$URL"

mkdir -p native/steam
case "$(basename "$UNZIP")" in
  unzip*)  "$UNZIP" -o -j -q "$WORK/sw.zip" $MEMBERS -d native/steam ;;
  bsdtar*) "$UNZIP" -x -f "$WORK/sw.zip" -C native/steam -s ',^.*/,,' $MEMBERS ;;   # flatten: the dylib is 4 deep
  *)       "$UNZIP" e -y -o"native/steam" "$WORK/sw.zip" $MEMBERS >/dev/null ;;
esac
rm -rf "$WORK"

echo "==> native/steam:"
ls -la native/steam

echo "==> verifying the exports current Steamworks.NET requires"
# PORT: the MSYS2 strings this was written against, else binutils' on PATH, else the same thing
# done with tr - a symbol name is a run of printable bytes either way.
STRINGS=/c/tools/msys64/usr/bin/strings
[ -x "$STRINGS" ] || STRINGS=$(command -v strings 2>/dev/null || true)
dll_strings() {
  if [ -n "$STRINGS" ]; then "$STRINGS" -a "$1"; else tr -c '[:print:]' '\n' < "$1"; fi
}
fail=0
for lib in steam_api64.dll libsteam_api.so libsteam_api.dylib; do
  echo "    $lib"
  for sym in SteamInternal_SteamAPI_Init SteamInternal_CreateInterface SteamAPI_ManualDispatch_Init SteamAPI_Shutdown SteamAPI_RunCallbacks; do
    # Mach-O symbol names carry a leading underscore; PE and ELF ones do not.
    if dll_strings "native/steam/$lib" | grep -qx "_\{0,1\}$sym"; then
      echo "      OK      $sym"
    else
      echo "      MISSING $sym"; fail=1
    fi
  done
done
[ "$fail" = 0 ] || { echo "FATAL: fetched native does not match the pinned Steamworks.NET." >&2; exit 1; }
