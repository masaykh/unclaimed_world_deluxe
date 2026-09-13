#!/bin/sh
# Builds the OpenGL-profile effects with ShadowDusk instead of mgfxc.
#
# Same output contract as build/31-build-gl-effects.sh - profile-0 MGFX in the shipped effect's
# own XNB container - but compiled by a toolchain that runs on Linux and macOS as well as
# Windows. mgfxc needs the Windows-only fxc/d3dcompiler; ShadowDusk's OpenGL path is
# HLSL -> DXC -> SPIR-V -> SPIRV-Cross -> GLSL, and its natives ship for win-x64, linux-x64,
# osx-x64 and osx-arm64. That is the whole reason this script exists: it is what would let the
# effects be built in CI on any runner.
#
# Writes to content/effects-gl-sd/, NOT content/effects-gl/. The mgfxc output is what ships
# today and stays the reference to compare against; nothing here overwrites it.
#
# REQUIRES A PATCHED ShadowDusk. Stock 0.20.0 compiles 6 of these 19. The nine gaps and the six
# patches that close them are in patches/shadowdusk/, which also carries the build instructions.
# Point UW_SHADOWDUSK at the built CLI.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

SD=${UW_SHADOWDUSK:-}
if [ -z "$SD" ] || [ ! -f "$SD" ]; then
  echo "Set UW_SHADOWDUSK to a PATCHED ShadowDuskCLI.exe." >&2
  echo "See patches/shadowdusk/README.md for how to build one:" >&2
  echo "  git clone https://github.com/kaltinril/ShadowDusk.git && git checkout e4b1c878" >&2
  echo "  git apply \$UW_REPO/patches/shadowdusk/*.patch" >&2
  echo "  dotnet build src/ShadowDusk.Cli/ShadowDusk.Cli.csproj -c Release -f net8.0 \\" >&2
  echo "               -p:TargetFrameworks=net8.0" >&2
  echo "  # then copy the native compilers per that README" >&2
  exit 2
fi

TOOL=artifacts/bin/MgfxTranscode/debug/mgfxtranscode.exe
[ -f "$TOOL" ] || "$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -v q --nologo >/dev/null

SRCDIR=content/effects
OUT=content/effects-gl-sd
OBJ=artifacts/obj/gl-effects-sd
TEMPLATES=ref/effects-mgfx8-original
mkdir -p "$OUT" "$OBJ"

# NOTE: no preprocessed source copy, unlike build/31.
#
# That script compiles from a COPY with the explicit vertex constant-register bindings stripped
# ('uniform const float4x4 World : register(vs, c20);'), because mgfxc's OpenGL path mishandles
# them - it rendered 7 of Vehicle's 8 passes wrong, half the pixels each, and the vehicles came
# out see-through. ShadowDusk does not need that workaround: compiling Vehicle.fx with and
# without the annotations produces BYTE-IDENTICAL output, so it ignores them exactly as the MGFX
# model says it should (uniforms are repacked into generated constant buffers; a raw register
# index is meaningless by the time GLSL is emitted). So this builds the studio's sources as
# released, with no transformation at all.

EFFECTS="Billboard BloomCombine BloomExtract CloudShadows DevShape EdgeDetect GaussianBlur
         GUI/CRT GUI/LCD LightSourcesEffect multiTex OverlayEffect OverlayGroundSpriteEffect
         RoadsAndPaths RoundLine skinFX TimeOfDayAndLightsources Vehicle water"

built=0; missing=0; failed=0
MISSING_LIST=""

for name in $EFFECTS; do
  fx="$SRCDIR/$name.fx"
  if [ ! -f "$fx" ]; then
    missing=$((missing + 1))
    MISSING_LIST="$MISSING_LIST $name"
    continue
  fi

  tmpl="$TEMPLATES/$name.xnb"
  [ -f "$tmpl" ] || tmpl="$UW_GAME/Content/$name.xnb"
  if [ ! -f "$tmpl" ]; then
    echo "  !! $name: no XNB container to use as a template" >&2
    failed=$((failed + 1))
    continue
  fi

  mkdir -p "$(dirname "$OBJ/$name")" "$(dirname "$OUT/$name")"

  # A non-.xnb extension makes ShadowDusk emit the bare MGFX blob, which is what inject wants.
  if ! "$SD" "$fx" "$OBJ/$name.ogl.mgfxo" /Profile:OpenGL >"$OBJ/$name.log" 2>&1; then
    echo "  !! $name: ShadowDusk failed"; sed 's/^/       /' "$OBJ/$name.log" >&2
    failed=$((failed + 1))
    continue
  fi

  # Same reasoning as build/31: the shipped effect is the source of truth for every parameter's
  # INITIAL VALUE. DXC drops initialisers on external globals ("Initializer of external global
  # will be ignored"), exactly as mgfxc's OpenGL path zeroes them, and uniforms this game never
  # assigns from code - Billboard's LightColor, skinFX's Alpha and AlphaFactor - are nothing but
  # their initialiser. Zeroed, they draw the world as black silhouettes and every character
  # invisible.
  DEFAULTS="$UW_GAME/Content/$name.xnb"
  [ -f "$DEFAULTS" ] || DEFAULTS=""

  # NOT piped into sed - `set -e` takes a pipeline's status from its last command, so piping
  # would report success whatever inject did. (build/31 learned this the hard way: it printed
  # "19 built, 0 failed" through nineteen consecutive failures and wrote not one file.)
  if "$TOOL" inject "$tmpl" "$OBJ/$name.ogl.mgfxo" "$OUT/$name.xnb" $DEFAULTS >"$OBJ/$name.inject.log" 2>&1; then
    sed 's/^/  -> /' "$OBJ/$name.inject.log"
    built=$((built + 1))
  else
    echo "  !! $name: inject failed" >&2
    sed 's/^/       /' "$OBJ/$name.inject.log" >&2
    failed=$((failed + 1))
  fi
done

echo
echo "==> $built built, $missing missing, $failed failed  -> $OUT"
[ "$missing" = 0 ] || echo "    missing sources:$MISSING_LIST" >&2
[ "$failed" = 0 ] && [ "$missing" = 0 ]
