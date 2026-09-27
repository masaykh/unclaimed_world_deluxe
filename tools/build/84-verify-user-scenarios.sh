#!/bin/sh
# Verifies that a scenario exported by tools/DataExport can be played as a USER scenario, with no
# game launch.
#
# Exports the built-in scenarios with `dataexport --scenarios`, copies one into a throwaway
# installation's user/Scenarios under a new name, and loads it the way NEW GAME does
# (`dataexport --user-scenario=<folder>`): base tables, then the scenario's own through
# AllScenarioLoader.GetScenarioDataLoader - UserDataLoader - then every key its scenarioData.xml
# names is looked up, then GameData.PostDataCompleteInitialize runs.
#
# Two cases, and the second is the control that proves the first is checking something:
#
#   1. a full export of The Clay Pit loads, every key resolves, and validation passes
#   2. the same scenario with ONLY scenario.xml and scenarioData.xml fails on 'spawnWorld' - the
#      error Kastuk reported, and what every user scenario did before UserDataLoader read its
#      folder (Sim.CurrentSerializeMode shipped as NoSerialize, so no loader opened any XML)
#
# The Clay Pit because it is the one asked for, and because it round-trips today. Several others
# do not: DataExport cannot yet write some of their tables (actionSets.xml with SoundEffectAction,
# bodyTypes), and those fail here by NAME - that is the export's gap, not the loader's.
#
# RUN IT LOCALLY, NOT IN CI, for the same reason as 80-verify-modloader.sh: the data loader
# resolves content paths against the installation, and a runner has no copy of Content/.
#
#   sh tools/build/84-verify-user-scenarios.sh
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

[ -d "$UW_STEAM/Content" ] || {
  echo "FATAL: no Content/ under UW_STEAM=$UW_STEAM" >&2
  echo "       This runs against YOUR copy of the game's compiled content - see license.md." >&2
  echo "       export UW_STEAM=\"/path/to/Unclaimed World\"" >&2
  exit 2
}

WORK="${TMPDIR:-/tmp}/uw-user-scenario-verify"
SOURCE="The Clay Pit"
FAILURES=0

say()  { printf '%s\n' "$*"; }
fail() { printf '  FAIL  %s\n' "$*"; FAILURES=$((FAILURES + 1)); }
pass() { printf '  ok    %s\n' "$*"; }

say "==> building the game and the export tool"
# DataExport references the built DX assembly (UwGameBin), not the project - build that first or
# the check runs against whatever was built last.
"$DOTNET" build base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=DX -v q --nologo
"$DOTNET" build tools/DataExport/DataExport.csproj -c Release -v q --nologo
EXPORT="$UW_REPO/$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' | head -1)"
[ -f "$EXPORT" ] || { echo "FATAL: dataexport.exe not found" >&2; exit 1; }

# The same throwaway installation 80-verify-modloader.sh builds.
d="$WORK/install"; rm -rf "$WORK"; mkdir -p "$d/user/Mods" "$d/user/Scenarios" "$d/data/Maps" "$d/data/BaseData/Strings"
cp -rp scenarios/. "$d/data/Maps/"
cp -p  translations/*.xml "$d/data/BaseData/Strings/"
cp -rp "$UW_STEAM/Content" "$d/Content"

say "==> exporting the built-in scenarios"
( cd "$d" && "$EXPORT" . --scenarios --nomods >"$WORK/export.log" 2>&1 ) || true
[ -f "$d/data/Scenarios/$SOURCE/eventActionTypes.xml" ] || { cat "$WORK/export.log"; echo "FATAL: '$SOURCE' was not exported" >&2; exit 1; }

# <Name> in scenario.xml must equal the folder - it is the key both scenarioData.xml and the
# tables are read by - and a user copy of a built-in wants a name of its own.
copy_as() {
  cp -rp "$d/data/Scenarios/$SOURCE" "$d/user/Scenarios/$1"
  sed -i "s#<Name>$SOURCE</Name>#<Name>$1</Name>#" "$d/user/Scenarios/$1/scenario.xml"
}

say "==> 1. a full export loads as a user scenario"
copy_as "Clay Pit Full"
out=$( cd "$d" && "$EXPORT" . --nomods "--user-scenario=Clay Pit Full" 2>&1 ) && rc=0 || rc=$?
if [ "$rc" -eq 0 ] && echo "$out" | grep -q "keys named by scenarioData.xml resolve" \
   && echo "$out" | grep -q "survive the game's own validation"; then
  pass "$(echo "$out" | sed -n 's/.*all \([0-9]*\) keys.*/\1/p') keys resolve, and validation passes"
else
  echo "$out" | sed -n '/scenario tables/,$p' | sed "s/^/      /"
  fail "a full export of '$SOURCE' does not load as a user scenario"
fi

say "==> 2. control: scenario.xml and scenarioData.xml alone fail on 'spawnWorld'"
copy_as "Clay Pit Bare"
find "$d/user/Scenarios/Clay Pit Bare" -type f ! -name scenario.xml ! -name scenarioData.xml -delete
out=$( cd "$d" && "$EXPORT" . --nomods "--user-scenario=Clay Pit Bare" 2>&1 ) && rc=0 || rc=$?
if [ "$rc" -ne 0 ] && echo "$out" | grep -q "SpawnWorldAction 'spawnWorld'"; then
  pass "refused, naming spawnWorld"
else
  echo "$out" | sed -n '/scenario tables/,$p' | sed "s/^/      /"
  fail "a scenario with no tables was not refused - the check above is not checking anything"
fi

rm -rf "$WORK"
say ""
if [ "$FAILURES" -eq 0 ]; then
  say "user scenarios OK - 2/2 cases passed."
else
  say "user scenarios FAILED - $FAILURES check(s)."
  exit 1
fi
