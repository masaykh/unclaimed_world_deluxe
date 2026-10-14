#!/bin/sh
# Builds new 3D models - FBX made in Blender - into model .xnb files the game loads, with the
# studio's own model processors (tools/ModelPipeline, ported from original_src).
#
#   sh tools/build/38-build-models.sh [OUT]       every model in assets/port-models/models.txt
#   sh tools/build/38-build-models.sh --selftest  build the stand-in cart and check it in the game's loader
#   sh tools/build/38-build-models.sh --reference DIR
#                                                  write DIR/model_reference.blend: the colonist,
#                                                  the woman and the MULE at true size, in metres
#   sh tools/build/38-build-models.sh --rig DIR    write DIR/colonist_rig.blend: the colonist's
#                                                  skeleton and skinned mesh as the game holds them,
#                                                  with studio clips as actions - to animate on
#
# OUT defaults to artifacts/content/models; a model lands at OUT/Models/<name>.xnb with its
# textures beside it. Not packaged yet: nothing in the game asks for a new model until code does.
#
# WHAT IT BUILDS. mgcb's FBX importer is Assimp, which reads FBX 2011 and later - what Blender
# writes. The studio's 247 sources in assets/Content/Models are ASCII FBX 6.1, which Assimp refuses;
# none of them is rebuilt here, and none needs to be (docs/modding.md, "New 3D models").
#
# models.txt, one model per line:   <name> <processor> [Param=Value ...]
#   AnimatedVehicleProcessor   rigid models: vehicles, tools, props (how the studio built the MULE)
#   Clip                       a colonist animation made on the colonist rig (--rig): built with
#                              the studio's AnimatedModelProcessor into Models/clips/<name>.xnb,
#                              and added to man_idle and woman_idle when the game loads them
#                              (UWGame.Port.ExtraClips). Name it man_<clip>; its start offset and
#                              flags go in <name>.xml beside the FBX, as the studio's do.
# Scale defaults to the game's units per metre (13.75: a colonist, drawn at scale 1, is 24 units
# tall in the rest pose, taken as 1.75 m), so a model made at real size in metres stands beside
# them at true size. Set Scale= on the line to override.
#
# THE EFFECTS ARE NOT COMPILED HERE. A model names its effect as a separate asset - ..\Vehicle or
# ..\skinFX, as the studio's do - and the port builds those from assets/effects with ShadowDusk
# (34-build-gl-effects-shadowdusk.sh); MonoGame's compiler refuses their vs_4_0 shaders for OpenGL.
# The environment map is staged so its reference (..\landscapeSquare_0) matches the game's; its
# rebuilt copy is not part of the output that matters.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

say() { printf '%s\n' "$*"; }
UNITS_PER_METRE=13.75
WORK="${TMPDIR:-/tmp}/uw-build-models"

MODE=build
case "$1" in
  --selftest) MODE=selftest; OUT="$WORK/selftest-out" ;;
  --reference) MODE=reference; REFDIR=${2:?usage: 38-build-models.sh --reference DIR} ;;
  --rig) MODE=rig; REFDIR=${2:?usage: 38-build-models.sh --rig DIR} ;;
  *) OUT=${1:-artifacts/content/models} ;;
esac
case "${OUT:-}" in ''|/*|?:*) ;; *) OUT="$UW_REPO/$OUT" ;; esac

# ---- tools -------------------------------------------------------------------------------
MG_VERSION=$(sed -n 's:.*<MonoGameVersion>\([^<]*\)</MonoGameVersion>.*:\1:p' Directory.Packages.props | head -1)
[ -n "$MG_VERSION" ] || { echo "FATAL: MonoGameVersion not found in Directory.Packages.props" >&2; exit 1; }
MGCB_DIR=artifacts/tools/mgcb-$MG_VERSION
if [ ! -d "$MGCB_DIR" ]; then
  say "==> installing dotnet-mgcb $MG_VERSION into $MGCB_DIR"
  "$DOTNET" tool install dotnet-mgcb --version "$MG_VERSION" --tool-path "$MGCB_DIR" >/dev/null
fi
MGCB=$(find "$UW_REPO/$MGCB_DIR" -maxdepth 1 \( -name 'mgcb' -o -name 'mgcb.exe' \) | head -1)
[ -n "$MGCB" ] || { echo "FATAL: mgcb not found in $MGCB_DIR" >&2; exit 1; }

"$DOTNET" build tools/ModelPipeline/ModelPipeline.csproj -c Release -v q --nologo >/dev/null
PIPELINE="$UW_REPO/artifacts/bin/ModelPipeline/release/UwModelPipeline.dll"
[ -f "$PIPELINE" ] || { echo "FATAL: $PIPELINE not built" >&2; exit 1; }

winpath() { if command -v cygpath >/dev/null 2>&1; then cygpath -m "$1"; else printf '%s' "$1"; fi; }

# The probe loads what was built through the game's own readers, on a real GraphicsDevice, with
# the game's GL effects in front of the Steam content - the combination a player has.
probe() {
  # The probe loads through the game's own UwContentManager (ExtraClips included), from the GL
  # game build - so that build must be this tree's, not whatever was built last.
  if [ -z "$GAME_BUILT" ]; then
    "$DOTNET" build base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=GL -v q --nologo >/dev/null
    GAME_BUILT=1
  fi
  "$DOTNET" build tools/ContentProbe/ContentProbe.csproj -c Release -p:UwPlatform=GL -v q --nologo >/dev/null
  PROBE="$UW_REPO/$(find artifacts/bin/ContentProbe -name 'contentprobe*' -path '*release_gl*' ! -name '*.pdb' ! -name '*.json' ! -name '*.dll' | head -1)"
  [ -d artifacts/content/effects-gl ] || { echo "FATAL: no artifacts/content/effects-gl - run tools/build/34-build-gl-effects-shadowdusk.sh" >&2; exit 1; }
  rm -rf "$WORK/override"; mkdir -p "$WORK/override"
  cp -r artifacts/content/effects-gl/. "$WORK/override/"
  [ -n "$1" ] && cp -r "$1/." "$WORK/override/"
  shift
  "$PROBE" "$(winpath "$UW_STEAM/Content")" --override "$(winpath "$WORK/override")" "$@"
}

BLENDER=${UW_BLENDER:-"/c/Program Files/Blender Foundation/Blender 5.0/blender.exe"}
# The studio clips a new colonist animation is most likely to start from: walking, carrying
# something heavy, pulling (construction), standing.
RIG_CLIPS=gaitWalk,haulHeavy,constructPull,idle

# The colonist rig (tools/ModelPipeline/make_rig.py), from man_idle as the game loads it.
make_rig() {   # <dir>
  mkdir -p "$1"
  probe "" --export-rig "$(winpath "$1")" --clips "$RIG_CLIPS" Models/man_idle:Model | grep -E 'wrote|FAIL'
  "$BLENDER" -b --factory-startup --python "$(winpath tools/ModelPipeline/make_rig.py)" --     "$(winpath "$1/man_idle.rig.json")" "$(winpath "$1/colonist_rig.blend")" | grep -E 'colonist|SAVED|Error'
}

# ---- reference, rig -----------------------------------------------------------------------
if [ "$MODE" = rig ]; then
  [ -x "$BLENDER" ] || { echo "FATAL: no Blender at $BLENDER - set UW_BLENDER" >&2; exit 1; }
  make_rig "$REFDIR"
  exit 0
fi
if [ "$MODE" = reference ]; then
  [ -x "$BLENDER" ] || { echo "FATAL: no Blender at $BLENDER - set UW_BLENDER" >&2; exit 1; }
  mkdir -p "$REFDIR"
  probe "" --export-obj "$(winpath "$REFDIR")" Models/man_idle:Model Models/woman_idle:Model Models/utility_vehicle:Model | grep -E 'wrote|FAIL'
  "$BLENDER" -b --factory-startup --python "$(winpath tools/ModelPipeline/make_reference.py)" -- \
    "$(winpath "$REFDIR")" "$(winpath "$REFDIR/model_reference.blend")" | grep -E 'colonist|SAVED'
  exit 0
fi

# ---- sources -------------------------------------------------------------------------------
# mgcb takes the folder its response file is in as the content root, and writes a dependency
# (a texture) relative to that root - so every source is staged under one.
rm -rf "$WORK/src" "$WORK/obj" "$OUT"
mkdir -p "$WORK/src/Models" "$OUT"
cp assets/Content/landscapeSquare.png "$WORK/src/"

if [ "$MODE" = selftest ]; then
  cp tools/ModelPipeline/test/cart_test.fbx tools/ModelPipeline/test/cart_base_256.tga "$WORK/src/Models/"
  LIST="cart_test AnimatedVehicleProcessor"
  # The animation round trip: the studio's walk, taken off the game's own man_idle onto the
  # Blender rig and exported the way an animator would, must build back into the same keys.
  if [ -x "$BLENDER" ]; then
    make_rig "$WORK/rig"
    "$BLENDER" -b "$(winpath "$WORK/rig/colonist_rig.blend")" --python "$(winpath tools/ModelPipeline/export_clip.py)" --       gaitWalk "$(winpath "$WORK/src/Models/man_rtWalk.fbx")" | grep -E 'WROTE|Error'
    # The studio's walk: a gait, its first 0.0625 s the bind pose.
    printf '<root>
  <startoffset>625000</startoffset>
  <isgait>true</isgait>
</root>
' > "$WORK/src/Models/man_rtWalk.xml"
    LIST="$LIST
man_rtWalk Clip"
  else
    say "  SKIP  no Blender at $BLENDER (set UW_BLENDER): the animation round trip does not run"
  fi
else
  cp -r assets/port-models/. "$WORK/src/Models/"
  LIST=$(grep -v '^[[:space:]]*#' assets/port-models/models.txt | grep -v '^[[:space:]]*$' || true)
fi

{
  echo "/outputDir:$(winpath "$OUT")"
  echo "/intermediateDir:$(winpath "$WORK/obj")"
  echo "/platform:DesktopGL"
  echo "/profile:HiDef"
  echo "/compress:False"
  echo "/reference:$(winpath "$PIPELINE")"
  echo "$LIST" | while read -r name processor params; do
    [ -n "$name" ] || continue
    echo ""
    echo "#begin Models/$name.fbx"
    echo "/importer:FbxImporter"
    dest=Models/$name.fbx
    if [ "$processor" = Clip ]; then processor=AnimatedModelProcessor; dest=Models/clips/$name.fbx; fi
    echo "/processor:$processor"
    case " $params " in *" Scale="*) ;; *) echo "/processorParam:Scale=$UNITS_PER_METRE" ;; esac
    for p in $params; do echo "/processorParam:$p"; done
    echo "/build:Models/$name.fbx;$dest"
  done
} > "$WORK/src/models.mgcb"

if [ -z "$LIST" ]; then
  say "==> no models listed in assets/port-models/models.txt - nothing to build"
  exit 0
fi

# ---- build ---------------------------------------------------------------------------------
say "==> building models with mgcb $MG_VERSION and the studio's processors"
log="$WORK/mgcb.log"
( cd "$WORK/src" && "$MGCB" "/@:models.mgcb" ) > "$log" 2>&1 || true
grep -E 'error|warning|succeeded' "$log" | sed 's/^/    /'
grep -q ' 0 failed' "$log" || { echo "FATAL: mgcb reported failures (full log: $log)" >&2; exit 1; }
find "$OUT" -name '*.xnb' | sed "s#^#    built #"

[ "$MODE" = selftest ] || exit 0

# ---- self-test -----------------------------------------------------------------------------
# The stand-in cart (tools/ModelPipeline/test/make_cart_test.py) must load through the game's
# readers and keep what the game finds a vehicle's parts by: the "hull" node seats hang from,
# the wheel joints, the driver's empty - at game size, and pointing at the game's own effect.
say "==> self-test: the stand-in cart, in the game's loader"
set +e
failures=0
check() { if [ "$1" = 0 ]; then say "  ok    $2"; else say "  FAIL  $2"; failures=$((failures + 1)); fi; }
xnb="$OUT/Models/cart_test.xnb"
[ -f "$xnb" ] && [ -f "$OUT/Models/cart_base_256_0.xnb" ]; check $? "the model and its texture were written"
grep -aq '\.\./Vehicle' "$xnb"; check $? "its material names the game's Vehicle effect, not a compiled copy"
[ ! -f "$OUT/Vehicle.xnb" ]; check $? "no effect was compiled alongside it"
out=$(probe "$OUT" --model-details Models/cart_test:Model 2>&1) || true
echo "$out" | grep -q '^  OK      Models/cart_test'; check $? "it loads through the game's ContentManager on a GL device"
bones=$(echo "$out" | sed -n 's/^ *bones: //p')
for b in 'hull<RootNode' 'wheel_rear_left_joint<RootNode' 'wheel_rear_left<wheel_rear_left_joint' 'wheel_rear_right<wheel_rear_right_joint' 'DriversSeat<RootNode'; do
  case " $bones " in *" $b "*) check 0 "bone $b" ;; *) check 1 "bone $b (got: $bones)" ;; esac
done
radius=$(echo "$out" | sed -n 's/.*bounds: radius \([0-9.]*\).*/\1/p')
# 1.04 m in Blender (a 1.2 m box on 0.35 m wheels) x 13.75 units a metre
awk -v r="$radius" 'BEGIN { exit !(r > 13.5 && r < 15.0) }'; check $? "made in metres, built to game size (radius ${radius:-?}, expected ~14.3)"
if [ -f "$OUT/Models/clips/man_rtWalk.xnb" ]; then
  # Built as a clip, the walk is added to the game's own man_idle and woman_idle when they load
  # (UWGame.Port.ExtraClips) as "rtWalk" - and must then be the studio's walk, key for key, the
  # mirrored hand attachment included (kept from the studio's clips, as Blender loses a mirror).
  say "==> self-test: the studio walk through the Blender rig, back into man_idle and woman_idle"
  probe "$OUT" --export-rig "$(winpath "$WORK/rt")" --clips rtWalk Models/man_idle:Model Models/woman_idle:Model | grep -E 'wrote|FAIL'
  cmp=$("$BLENDER" -b --factory-startup --python "$(winpath tools/ModelPipeline/compare_clip.py)" --         "$(winpath "$WORK/rig/man_idle.rig.json")" gaitWalk "$(winpath "$WORK/rt/man_idle.rig.json")" rtWalk 0.5 0.05 all 2>&1 | grep -E '^  ')
  echo "$cmp"
  echo "$cmp" | grep -q 'channels compared' && ! echo "$cmp" | grep -q 'FAIL'
  check $? "man_idle plays the rebuilt walk as the studio's (rotation within 0.5 degrees, position within 0.05 units, every bone)"
  grep -q '"rtWalk"' "$WORK/rt/woman_idle.rig.json"; check $? "woman_idle has it too"
  grep -q '"gaitWalk"' "$WORK/rt/man_idle.rig.json" && grep -o '"allClipNames":\[[^]]*' "$WORK/rt/man_idle.rig.json" | grep -q 'useHoe'
  check $? "the studio's own clips are all still there"
fi
[ "$failures" = 0 ] && say "model pipeline self-test OK" || { say "model pipeline self-test FAILED - $failures check(s)"; exit 1; }
