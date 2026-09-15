#!/bin/sh
# Builds the game's OpenGL-profile effects from the HLSL in assets/effects/.
#
# The output is profile-0 MGFX inside the shipped effect's own XNB container, which is what the
# game loads. What makes this the build rather than one of two is the compiler: MonoGame's own
# mgfxc P/Invokes the Windows-only d3dcompiler_47.dll EVEN FOR THE OPENGL PROFILE, so it can only
# run on Windows or under Wine. ShadowDusk's OpenGL path is
# HLSL -> DXC -> SPIR-V -> SPIRV-Cross -> GLSL, with natives for win-x64, linux-x64, osx-x64 and
# osx-arm64 - so the effects can be built on any of the three platforms, and in CI on a Linux
# runner.
#
# REQUIRES A PATCHED ShadowDusk. Stock 0.20.0 compiles 6 of these 19; the nine gaps and the six
# patches that close them are in tools/shadowdusk/. Build one with
# tools/shadowdusk/build-shadowdusk.sh and point UW_SHADOWDUSK at the result.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

SD=${UW_SHADOWDUSK:-}
if [ -z "$SD" ] || [ ! -f "$SD" ]; then
  echo "Set UW_SHADOWDUSK to a PATCHED ShadowDuskCLI." >&2
  echo >&2
  echo "  sh tools/shadowdusk/build-shadowdusk.sh" >&2
  echo "  export UW_SHADOWDUSK=\"\$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe\"" >&2
  echo >&2
  echo "See tools/shadowdusk/README.md for what the patches fix and why." >&2
  exit 2
fi

TOOL=artifacts/bin/MgfxTranscode/debug/mgfxtranscode.exe
[ -f "$TOOL" ] || "$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -v q --nologo >/dev/null

# Shader SOURCES are in the repository; everything else here is a build output or comes from
# your own copy of the game.
SRCDIR=assets/effects
OUT=artifacts/content/effects-gl
OBJ=artifacts/obj/gl-effects-sd

# The XNB CONTAINER to reuse, and the source of truth for every parameter's INITIAL VALUE. Both
# come from a copy of the game, because both are compiled content and neither is in this
# repository. UW_TEMPLATES lets a checkout that HAS a pristine snapshot use it instead.
TEMPLATES="${UW_TEMPLATES:-$UW_GAME/Content}"
mkdir -p "$OUT" "$OBJ"

# NO PREPROCESSING. The sources in assets/effects/ are compiled exactly as they are.
#
# Worth stating, because the mgfxc route could not do that. It had to compile from a COPY with
# the explicit vertex constant-register bindings stripped - 'uniform const float4x4 World :
# register(vs, c20);' - because mgfxc's OpenGL path mishandles them: 7 of Vehicle's 8 passes came
# out wrong, half the pixels each, and in game the vehicles were see-through.
#
# ShadowDusk needs no such workaround. Compiling Vehicle.fx with and without the annotations
# produces BYTE-IDENTICAL output, so it ignores them exactly as the MGFX model says it should -
# uniforms are repacked into generated constant buffers, and a raw register index is meaningless
# by the time GLSL is emitted.

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
  # WHERE THE DEFAULTS COME FROM, and why it is not $tmpl even though that is the same effect.
  #
  # MgfxDefaults reads the reference with the V10 reader, and the shipped effect is v8 - hand it
  # the pristine .xnb and it dies with "Unable to read beyond the end of the stream". So the
  # reference has to be a TRANSCODED copy: same parameter table, same defaults, v10 container.
  #
  # tools/build/35-make-dx-effects.sh produces exactly that, for the DX archive, and it is the
  # only copy in this repository that is both v10 and still carries the studio's initial values.
  # $UW_GAME/Content is the old answer and still works IF an earlier step transcoded it in place.
  #
  # THIS USED TO FAIL SILENTLY, and that is the reason for the check below rather than a
  # fallback. It read $UW_GAME/Content, which does not exist in a fresh checkout, and
  # `|| DEFAULTS=""` turned the miss into a skip: the effects built, loaded, and passed every
  # gate with every parameter default ZEROED. The paragraph above says what that costs - black
  # silhouettes, invisible characters. Nothing reported it; contentprobe printing "NO parameter
  # carries a non-zero initial value" for skinFX is what eventually gave it away.
  DEFAULTS="$UW_REPO/artifacts/content/effects-dx/$name.xnb"
  [ -f "$DEFAULTS" ] || DEFAULTS="$UW_GAME/Content/$name.xnb"
  if [ ! -f "$DEFAULTS" ]; then
    echo "  !! $name: no v10 effect to take parameter defaults from." >&2
    echo "     sh tools/build/35-make-dx-effects.sh   (transcodes the shipped effects to v10)" >&2
    failed=$((failed + 1))
    continue
  fi

  # NOT piped into sed - `set -e` takes a pipeline's status from its last command, so piping
  # would report success whatever inject did. (build/31 learned this the hard way: it printed
  # "19 built, 0 failed" through nineteen consecutive failures and wrote not one file.)
  # $DEFAULTS QUOTED. Unquoted it was harmless only while it was always empty; with a real path
  # in it, "C:\Program Files (x86)\..." word-splits and the tool answers "Unrecognised arguments".
  if "$TOOL" inject "$tmpl" "$OBJ/$name.ogl.mgfxo" "$OUT/$name.xnb" "$DEFAULTS" >"$OBJ/$name.inject.log" 2>&1; then
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
