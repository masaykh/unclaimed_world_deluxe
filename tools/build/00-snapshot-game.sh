#!/bin/sh
# Phase 0 safety net.
#   1. SHA-256 every file in the pristine Steam install  -> ref/manifest.sha256
#   2. Copy the whole install                            -> game/      (the run target)
#   3. Copy just the managed binaries + configs          -> ref/original/  (never modified)
# The Steam folder is only ever READ. `Verify integrity of game files` stays the escape hatch.
set -e
. "$(dirname "$0")/env.sh"

if [ ! -d "$UW_STEAM" ]; then echo "FATAL: Steam install not found at $UW_STEAM" >&2; exit 1; fi

echo "==> hashing pristine install"
( cd "$UW_STEAM" && find . -type f -print0 | sort -z | xargs -0 sha256sum ) > "$UW_REPO/ref/manifest.sha256"
echo "    $(wc -l < "$UW_REPO/ref/manifest.sha256") files hashed"

echo "==> copying install -> game/"
mkdir -p "$UW_GAME"
cp -r "$UW_STEAM/." "$UW_GAME/"

# Delete anything left in game/ that the install no longer has. Copying over the top is not
# enough across a GAME VERSION change: 1.0.4.8 dropped seven files (Content/skinFX_0.xnb,
# SharpDX.Direct3D9, the vshost stubs), and a stale copy of a removed asset is invisible to the
# verify below, which only checks the files the manifest lists. Files are removed one by one
# rather than by wiping game/ first, because on Windows anything holding the directory open
# makes `rm -rf` fail with "Device or resource busy" and, under `set -e`, abort mid-snapshot.
echo "==> removing files the install no longer has"
stale=0
while IFS= read -r rel; do
  if [ ! -f "$UW_STEAM/$rel" ]; then rm -f "$UW_GAME/$rel"; stale=$((stale + 1)); fi
done <<EOF
$(cd "$UW_GAME" && find . -type f)
EOF
echo "    $stale stale file(s) removed"

echo "==> copying shipped binaries -> ref/original/"
mkdir -p "$UW_REPO/ref/original"
# Each pattern separately and tolerating absence: 1.0.4.8 ships no *.manifest at all (the
# vshost stubs went away), and one missing pattern used to abort the whole snapshot under
# `set -e` before the verify below ever ran.
for pattern in '*.dll' '*.exe' '*.config' '*.manifest' '*.txt'; do
  ( cd "$UW_STEAM" && cp -p $pattern "$UW_REPO/ref/original/" 2>/dev/null ) || true
done

echo "==> verifying game/ against manifest"
( cd "$UW_GAME" && sha256sum -c --quiet "$UW_REPO/ref/manifest.sha256" ) && echo "    game/ matches manifest"
echo "done."
