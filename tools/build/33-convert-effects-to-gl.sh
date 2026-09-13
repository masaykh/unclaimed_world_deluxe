#!/bin/sh
# Converts the shipped DirectX-profile effects to OpenGL-profile ones, straight from the DX9
# bytecode fxc left in each shader's Aon9 chunk. Output goes to content/effects-gl/.
#
# This is the cheap route to a DesktopGL content set, and it exists because 31-build-gl-effects.sh
# needs HLSL that mostly does not exist: only 3 of the 19 effects have recovered sources. This
# tool needs none - MojoShader translates the shipped bytecode, which is the same translator
# MonoGame's own OpenGLShaderProfile uses.
#
# It converts 16 of the 19. The other 3 genuinely need HLSL:
#
#   multiTex, skinFX, Vehicle       no Aon9 chunk at all - compiled at a plain SM4 profile, so
#                                   there is no DX9 bytecode to translate
#
# RoundLine used to be on that list too. Its vertex shader reads cb0[r1.x + 7] into a
# 200-element instanceData array, and MojoShader will not take relative addressing without a
# CTAB. mgfxc stripped the CTAB, but the DX11 bytecode still records the size in its
# dcl_constantbuffer - so CtabBuilder synthesises one. See PORTING-NOTES.md.
#
# DevShape and TimeOfDayAndLightsources used to be on that list. Their one blocked vertex
# shader each reads a constant fxc synthesised for the SM2 fallback - its level_9_x position
# fixup - which the DX11 bytecode proves is absent there (that shader declares no constant
# buffer at all). MojoShader emits the same correction itself as posFixup, so the array is left
# unbound at GL default zero. See PORTING-NOTES.md.
#
# Run 31-build-gl-effects.sh afterwards to build any of those from recovered .fx sources; it
# writes to the same directory, so whichever runs last wins per effect. That ordering is
# deliberate - a hand-recovered HLSL build should override a bytecode translation.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

TOOL=artifacts/bin/MgfxDxToGl/debug/mgfxdxtogl.exe
if [ ! -f "$TOOL" ]; then
  echo "==> building mgfxdxtogl"
  "$DOTNET" build tools/MgfxDxToGl/MgfxDxToGl.csproj -v q --nologo
fi
TOOL=$(find artifacts/bin/MgfxDxToGl -name 'mgfxdxtogl.exe' | head -1)
[ -n "$TOOL" ] || { echo "FATAL: mgfxdxtogl.exe not found" >&2; exit 1; }

# mojoshader.dll is delivered as tool content by the dotnet-mgfxc package, so it has to be
# copied next to the exe rather than resolved as a package asset. Using the copy from the pinned
# mgfxc means the translator is the same build MonoGame's effect compiler uses.
MOJO=$(find /c/Users/*/.nuget/packages/dotnet-mgfxc -path '*win-x64/native/mojoshader.dll' 2>/dev/null | sort | tail -1)
if [ -n "$MOJO" ]; then
  cp -p "$MOJO" "$(dirname "$TOOL")/"
else
  echo "FATAL: mojoshader.dll not found under the dotnet-mgfxc package." >&2
  echo "       Run: $DOTNET tool restore" >&2
  exit 1
fi

# Source the DirectX effects from the transcoded MGFX v10 set, not the shipped v8 - the reader
# handles v10/v11 only, and 30-transcode-effects.sh has already produced v10.
SRC="$UW_GAME/Content"
OUT=content/effects-gl
mkdir -p "$OUT/GUI"

EFFECTS="Billboard BloomCombine BloomExtract CloudShadows DevShape EdgeDetect GaussianBlur
         GUI/CRT GUI/LCD LightSourcesEffect multiTex OverlayEffect OverlayGroundSpriteEffect
         RoadsAndPaths RoundLine skinFX TimeOfDayAndLightsources Vehicle water"

converted=0
needs_hlsl=0
NEEDS=""

for name in $EFFECTS; do
  in="$SRC/$name.xnb"
  if [ ! -f "$in" ]; then
    echo "  !! $name: not found at $in" >&2
    continue
  fi

  mkdir -p "$(dirname "$OUT/$name")"
  if out=$("$TOOL" convert "$in" "$OUT/$name.xnb" 2>&1); then
    converted=$((converted + 1))
    printf '  -> %-28s %s\n' "$name" "$(echo "$out" | tail -1 | sed 's/^ *-> *[^ ]* *//')"
  else
    needs_hlsl=$((needs_hlsl + 1))
    NEEDS="$NEEDS $name"
    printf '  == %-28s needs HLSL\n' "$name"
    echo "$out" | grep '!!' | sed 's/^ */         /'
  fi
done

echo
echo "$converted effect(s) converted to the OpenGL profile in $OUT."
if [ "$needs_hlsl" -gt 0 ]; then
  echo "$needs_hlsl still need HLSL sources in content/effects/:"
  for n in $NEEDS; do echo "  $n"; done
  echo
  echo "Then run build/31-build-gl-effects.sh, which compiles those and overwrites the"
  echo "translations for any effect whose .fx exists."
fi

# Every converted effect must be a valid OpenGL-profile v10 container - re-read what was
# written rather than trusting that writing it worked.
echo
echo "==> verifying the converted set"
"$TOOL" roundtrip "$OUT" | tail -3
