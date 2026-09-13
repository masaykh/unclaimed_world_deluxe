#!/bin/sh
# Builds the OpenGL-profile effects needed by the DesktopGL target.
#
# Why this exists at all: MGFX effects are profile-specific and MonoGame does NOT tolerate a
# mismatch - Effect.ReadHeader throws "This MGFX effect was built for a different platform!"
# when header.Profile != Shader.Profile (0 = OpenGL, 1 = DirectX_11). The shipped effects are
# profile 1, so the DesktopGL build needs every one of them rebuilt as profile 0. Unlike the
# v8 -> v10 version bump (build/30-transcode-effects.sh), this CANNOT be done by rewriting the
# container: the DirectX blob holds DXBC bytecode and the OpenGL blob holds GLSL text, so the
# shader has to be compiled from HLSL source.
#
# Pipeline per effect:
#   content/effects/<name>.fx
#     --mgfxc /Profile:OpenGL-->  <name>.ogl.mgfxo        (bare MGFX blob)
#     --mgfxtranscode inject -->  content/effects-gl/<name>.xnb
#
# The inject step reuses the shipped effect's own XNB container (type-reader table, shared
# resource count, primary type id) and swaps only the length-prefixed payload, so no XNB
# writer is needed and the result is a container the game already knows how to read.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

TOOL=artifacts/bin/MgfxTranscode/debug/mgfxtranscode.exe
[ -f "$TOOL" ] || "$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -v q --nologo >/dev/null

SRCDIR=content/effects
OUT=content/effects-gl
OBJ=artifacts/obj/gl-effects
TEMPLATES=ref/effects-mgfx8-original      # pristine shipped effects, from 00-snapshot + 30-transcode
mkdir -p "$OUT" "$OBJ"

# ---- preprocessed source tree --------------------------------------------------------------
# The studio's HLSL is compiled from a COPY, with one transformation applied, rather than being
# edited in place: the sources under content/effects/ stay a faithful copy of what Refactored
# Games released, and the reason for the change lives here where it can be read.
#
# THE TRANSFORMATION: explicit vertex-shader constant-register bindings are removed.
#
#   uniform const float4x4 World : register(vs, c20);   ->   uniform const float4x4 World;
#
# mgfxc's OpenGL path mishandles them. Vehicle.fx is the only effect in the game that binds its
# World/View/Projection this way, and it was the only effect still disagreeing with DirectX after
# everything else was fixed - 7 of its 8 passes wrong, each on exactly 50% of the pixels, with
# the vertex transform visibly different while the one technique using a DIFFERENT vertex shader
# (NormalDepth) matched perfectly. Removing the bindings takes it to 6 identical and 2 within
# tolerance. In game it showed as the vehicles being see-through.
#
# Dropping them is safe: MGFX does not use raw constant registers at all. mgfxc repacks every
# uniform into its own constant buffers and MojoShader assigns its own GLSL array slots, so the
# annotation is at best a hint and at worst - here - actively wrong. The DirectX build ignores
# this script entirely and keeps using the shipped effects.
SRC="$OBJ/src"
rm -rf "$SRC"
mkdir -p "$SRC"
cp -r "$SRCDIR"/. "$SRC"/
stripped=0
for fx in $(find "$SRC" -name '*.fx' -o -name '*.fxh'); do
  if grep -q 'register *( *vs *,' "$fx"; then
    sed -i 's/: *register *( *vs *, *c[0-9]\+ *)//g' "$fx"
    stripped=$((stripped + 1))
  fi
done
[ "$stripped" = 0 ] || echo "==> removed vertex-register bindings from $stripped source file(s)"

# The 20 effects the game loads. Names are the asset paths under Content/.
EFFECTS="Billboard BloomCombine BloomExtract CloudShadows DevShape EdgeDetect GaussianBlur
         GUI/CRT GUI/LCD LightSourcesEffect multiTex OverlayEffect OverlayGroundSpriteEffect
         RoadsAndPaths RoundLine skinFX TimeOfDayAndLightsources Vehicle water"

built=0; missing=0; failed=0
MISSING_LIST=""

for name in $EFFECTS; do
  fx="$SRC/$name.fx"
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
  if ! "$DOTNET" tool run mgfxc -- "$fx" "$OBJ/$name.ogl.mgfxo" /Profile:OpenGL >/dev/null 2>"$OBJ/$name.log"; then
    echo "  !! $name: mgfxc failed"; sed 's/^/       /' "$OBJ/$name.log" >&2
    failed=$((failed + 1))
    continue
  fi

  # NOT piped into sed. `set -e` takes a pipeline's status from its LAST command, so
  # `inject ... | sed` reported success whatever inject did, and `built` was then incremented
  # unconditionally - this script printed "19 built, 0 failed" through nineteen consecutive
  # "FATAL: Expected MGFX v10, found v11" and wrote not one file. Check the status explicitly.
  # The shipped effect is also the source of truth for every parameter's INITIAL VALUE, which
  # mgfxc's OpenGL path zeroes and its DirectX path does not. Uniforms this game never assigns
  # from code - Billboard's LightColor, skinFX's Alpha and AlphaFactor - are nothing but their
  # initialiser, so zeroed they draw the world as black silhouettes and every character
  # invisible. game/Content holds the v10 transcode of the shipped effect, which the reader can
  # parse; the v8 template above cannot be read for this.
  DEFAULTS="$UW_GAME/Content/$name.xnb"
  [ -f "$DEFAULTS" ] || DEFAULTS=""

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
echo "$built built, $missing without HLSL source yet, $failed failed."
if [ "$missing" -gt 0 ]; then
  echo
  echo "Still need .fx source in $SRCDIR for:"
  for m in $MISSING_LIST; do echo "  $m"; done
  echo
  echo "Recovery notes:"
  echo "  * BloomCombine, GaussianBlur, EdgeDetect come from Microsoft's XNA Bloom Postprocess"
  echo "    and NPR samples - their shipped parameter names match the samples exactly."
  echo "  * RoundLine comes from the XNA RoundLine sample (techniques Standard/NoBlur/"
  echo "    AnimatedLinear/AnimatedRadial/Glow/Modern)."
  echo "  * water and multiTex use Riemer's XNA tutorial x-prefixed convention (xView, xCamPos,"
  echo "    xClipMap, MultiTextured0..)."
  echo "  * MOST OF THESE NO LONGER NEED HLSL. build/33-convert-effects-to-gl.sh translates the"
  echo "    DX9 bytecode in each shipped effect's Aon9 chunk straight to GLSL with MojoShader,"
  echo "    and covers 15 of the 19 - all verified loading on a real DesktopGL device. Run it"
  echo "    BEFORE this script; both write to content/effects-gl/, so an HLSL build here"
  echo "    deliberately overrides a translation."
  echo "  * Only 4 genuinely need HLSL: multiTex, skinFX and Vehicle have no Aon9 chunk at all,"
  echo "    and RoundLine has one vertex shader using relative constant addressing that"
  echo "    MojoShader will not take without a CTAB."
  echo "  * skinFX first if you are picking one: all 207 model XNBs reference it, so no skinned"
  echo "    model loads on GL until it exists. See content/effects/_reference/."
fi
[ "$failed" = 0 ] || exit 1

# ---- parameter-compatibility gate -----------------------------------------------------------
# The game reaches effect parameters by name and does not check, so an effect that exposes fewer
# than the shipped one is a NullReferenceException on the first frame that draw runs - which is
# exactly how OverlayGroundSpriteEffect losing WindowPosition crashed a build that had just been
# declared good. mgfxc prunes differently per profile: its DirectX path keeps uniforms the
# shaders never read, its OpenGL / SM3 path drops them. Compiling the studio's unmodified
# sources for DirectX_11 reproduces the shipped parameter counts exactly, so this is pruning and
# not a source difference - the dropped parameters are dead on both backends.
#
# ACCOUNTED_FOR is every name known to be dead, each either never set by the game or handled
# through UWGame.Port.EffectCompat. Anything else is a real loss and fails the build.
ACCOUNTED_FOR="BillboardWidth BillboardHeight WindowPosition AlphaAdjustment
               DirLight2SpecularColor xLightDirection xAmbient xEnableLighting"

echo
echo "==> parameter compatibility against the shipped effects"
PARAM_REPORT="$OBJ/compare-params.txt"
"$TOOL" compare-params "$UW_GAME/Content" "$OUT" > "$PARAM_REPORT" 2>&1 || true
grep -E '^\s+(!!|\?\?|MISSING|extra)' "$PARAM_REPORT" | sed 's/^/  /' || true

# A lost INITIAL VALUE fails the build outright, with no allow-list. Unlike a lost parameter it
# is never benign: mgfxc's OpenGL path zeroes every default, and the uniforms this game leaves to
# their initialisers are the lighting and the alpha of everything in the world. `inject` restores
# them from the shipped effect, so anything still reported here means that restore did not take.
if grep -q 'DEFAULT' "$PARAM_REPORT"; then
  echo >&2
  echo "FATAL: rebuilt effects lost parameter initial value(s):" >&2
  grep 'DEFAULT' "$PARAM_REPORT" >&2
  echo >&2
  echo "These are not reported by any render comparison - tools/EffectRender assigns every" >&2
  echo "parameter before it draws, so it overwrites exactly what is broken here." >&2
  exit 1
fi

unexpected=""
for name in $(awk '/MISSING/ { print $2 }' "$PARAM_REPORT"); do
  known=0
  for ok in $ACCOUNTED_FOR; do [ "$name" = "$ok" ] && known=1; done
  [ "$known" = 1 ] || unexpected="$unexpected $name"
done

if [ -n "$unexpected" ]; then
  echo >&2
  echo "FATAL: rebuilt effects lost parameter(s) nothing accounts for:$unexpected" >&2
  echo >&2
  echo "Each one crashes the game the first time it is set. Check whether src/ sets it:" >&2
  echo "  grep -rn '\"<name>\"' src/ --include=*.cs" >&2
  echo "If it does, route that call through UWGame.Port.EffectCompat.SetIfDeclared and add the" >&2
  echo "name to ACCOUNTED_FOR here. If nothing sets it, just add it to ACCOUNTED_FOR." >&2
  exit 1
fi
echo "  all missing parameters are accounted for; full report in $PARAM_REPORT"
