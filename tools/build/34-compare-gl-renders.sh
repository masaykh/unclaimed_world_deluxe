#!/bin/sh
# Renders every effect technique/pass on BOTH backends and compares the frames pixel by pixel.
#
# This is the only check that says a converted shader COMPUTES the same thing. roundtrip proves
# the container survived, ContentProbe proves an effect loads, and neither caught a
# level_9_x semantic rewrite that left every vertex shader unable to bind a position stream, or
# a constant buffer too small for a partially-read matrix. Rendering caught both.
#
# It needs no knowledge of what any shader was meant to look like: both backends get the same
# geometry, the same clear colour, and parameter values derived from parameter NAMES, so
# agreement is the signal and a disagreement localises to one effect, technique and pass.
#
# Usage:  build/34-compare-gl-renders.sh [size]
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

SIZE=${1:-128}
WORK=artifacts/render
DXOUT="$WORK/dx"
GLOUT="$WORK/gl"
GLCONTENT="$WORK/gl-content"

echo "==> building both renderer flavours"
"$DOTNET" build tools/EffectRender/EffectRender.csproj -c Debug -p:UwPlatform=DX -v q --nologo
"$DOTNET" build tools/EffectRender/EffectRender.csproj -c Debug -p:UwPlatform=GL -v q --nologo

DXR=$(find artifacts/bin/EffectRender -name 'effectrender.exe' -path '*debug_dx*' | head -1)
GLR=$(find artifacts/bin/EffectRender -name 'effectrender.exe' -path '*debug_gl*' | head -1)
[ -n "$DXR" ] && [ -n "$GLR" ] || { echo "FATAL: renderer(s) not built" >&2; exit 1; }

# The OpenGL run needs the converted effects overlaid on the real content: everything else
# (textures, fonts) is profile-independent and shared.
echo "==> assembling the OpenGL content set"
mkdir -p "$GLCONTENT"
rm -rf "$GLCONTENT"/* 2>/dev/null || true
cp -rp "$UW_GAME/Content/." "$GLCONTENT"/
if [ -d content/effects-gl ]; then
  ( cd content/effects-gl && find . -name '*.xnb' -exec cp -p {} "$UW_REPO/$GLCONTENT/{}" \; )
else
  echo "FATAL: content/effects-gl is missing - run build/33-convert-effects-to-gl.sh first" >&2
  exit 1
fi

# Clear the CONTENTS rather than removing the directories. On Windows a directory can be held
# open briefly after the process that used it exits, and `rm -rf` then fails with "Device or
# resource busy" - which, with set -e, aborted this script leaving both output directories empty
# and the previous run's results gone. Deleting the files is enough to guarantee nothing stale
# is mistaken for fresh output, and it cannot fail that way.
mkdir -p "$DXOUT" "$GLOUT"
for d in "$DXOUT" "$GLOUT"; do
  rm -f "$d"/*.png "$d"/*.rgba "$d"/manifest.txt 2>/dev/null || true
done

# Absolute paths: the renderer sets ContentManager.RootDirectory from this, and a relative one
# resolves against the process working directory rather than the repo.
ABS=$(pwd -W 2>/dev/null || pwd)

echo "==> rendering on WindowsDX"
"$DXR" "$ABS/game/Content" "$ABS/$DXOUT" --size "$SIZE" > "$WORK/dx.log" 2>&1 || true
tail -1 "$WORK/dx.log"

echo "==> rendering on DesktopGL"
"$GLR" "$ABS/$GLCONTENT" "$ABS/$GLOUT" --size "$SIZE" > "$WORK/gl.log" 2>&1 || true
tail -1 "$WORK/gl.log"

echo
echo "==> comparing"
perl build/compare-renders.pl "$DXOUT" "$GLOUT"
