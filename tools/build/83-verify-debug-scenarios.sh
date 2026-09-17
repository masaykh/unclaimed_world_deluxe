#!/bin/sh
# Checks that every debug scenario's hard-coded tiles fit the map it is registered against.
#
#     sh tools/build/83-verify-debug-scenarios.sh
#
# WHY THIS EXISTS. PlaceGameEntities registers ninety-odd prepared situations, each as a map key
# and a method full of hard-coded `new Point(x, y)`. The two can disagree, and when they do the
# failure is an IndexOutOfRangeException deep inside MapManager.GetTile - which names neither the
# scenario nor the map. Five of them were in that state: MapApril2013 reached tile (114,122) on a
# 64x64 map, and PlaceGameEntitiesMilestoneBuild reached (223,100) on the same one while naming
# its own 256x256 map in its own name.
#
# The cause is that the table lost its per-scenario map assignments at some point - 86 of the 90
# point at "d Mezzomap MLo", and the only two that kept distinct names named folders that had
# since been renamed. Nothing stopped that happening and nothing would have noticed it.
#
# This runs offline against the source and the shipped maps. It needs no game, no content and no
# graphics device, which is the whole point: a scenario that cannot load is found here rather than
# by somebody choosing it from a menu.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

SRC="base_game/UnclaimedWorld/UWGame/SimSide/PlaceGameEntities.cs"
MAPS="scenarios"

[ -f "$SRC" ] || { echo "FATAL: $SRC not found" >&2; exit 2; }
[ -d "$MAPS" ] || { echo "FATAL: $MAPS/ not found" >&2; exit 2; }

WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT

# --- the shipped maps and their dimensions ------------------------------------------------------
for d in "$MAPS"/*/; do
  [ -f "$d/MapData.xml" ] || continue
  name=$(basename "$d")
  x=$(grep -o '<X>[0-9]*</X>' "$d/MapData.xml" | head -1 | tr -cd '0-9')
  y=$(grep -o '<Y>[0-9]*</Y>' "$d/MapData.xml" | head -1 | tr -cd '0-9')
  [ -n "$x" ] && [ -n "$y" ] && echo "$name|$x|$y" >> "$WORK/maps.txt"
done
[ -s "$WORK/maps.txt" ] || { echo "FATAL: no maps with dimensions under $MAPS/" >&2; exit 2; }

# --- the scenario table -------------------------------------------------------------------------
grep -o 'AddScenario(DebugScenarios\.[A-Za-z0-9_]*, "[^"]*", [A-Za-z0-9_]*)' "$SRC" \
  | sed 's/AddScenario(DebugScenarios\.//; s/)$//' \
  | awk -F', ' '{gsub(/"/,"",$2); print $1"|"$2"|"$3}' > "$WORK/triples.txt"

total=$(wc -l < "$WORK/triples.txt")
[ "$total" -gt 0 ] || { echo "FATAL: no AddScenario calls found in $SRC" >&2; exit 2; }

echo "==> $total debug scenarios, against $(wc -l < "$WORK/maps.txt") shipped maps"
echo

fails=0
checked=0
nopoints=0

while IFS='|' read -r key map fn; do
  mw=$(awk -F'|' -v m="$map" '$1==m {print $2}' "$WORK/maps.txt")
  mh=$(awk -F'|' -v m="$map" '$1==m {print $3}' "$WORK/maps.txt")
  if [ -z "$mw" ]; then
    printf '  !! %-32s names a map that does not exist: %s\n' "$key" "$map"
    fails=$((fails + 1))
    continue
  fi

  start=$(grep -nE "^	(public|private) static void $fn()" "$SRC" | head -1 | cut -d: -f1)
  if [ -z "$start" ]; then
    printf '  !! %-32s has no method %s\n' "$key" "$fn"
    fails=$((fails + 1))
    continue
  fi

  # The method body, to its closing brace at column zero plus one tab - the file is tab-indented
  # and every method ends on a line that is exactly one tab and a brace.
  awk -v s="$start" 'NR>s { if ($0=="\t}") exit; print }' "$SRC" > "$WORK/body.txt"

  set -- $(grep -o 'new Point([0-9]*, *[0-9]*)' "$WORK/body.txt" \
    | tr -d 'newPoint() ' \
    | awk -F, 'BEGIN{mx=-1;my=-1} {if($1+0>mx)mx=$1+0; if($2+0>my)my=$2+0} END{print mx, my}')
  mx=$1; my=$2
  if [ "$mx" = "-1" ]; then
    nopoints=$((nopoints + 1))
    continue
  fi
  checked=$((checked + 1))

  if [ "$mx" -ge "$mw" ] || [ "$my" -ge "$mh" ]; then
    printf '  !! %-30s %-24s map is %sx%s but the scenario reaches tile (%s,%s)\n' \
      "$key" "$map" "$mw" "$mh" "$mx" "$my"
    fails=$((fails + 1))
  fi
done < "$WORK/triples.txt"

echo
if [ "$fails" -gt 0 ]; then
  echo "debug scenarios FAILED - $fails of $total do not fit their map." >&2
  echo >&2
  echo "A scenario reaching a tile its map does not have dies in MapManager.GetTile with an" >&2
  echo "IndexOutOfRangeException. Either give it a map that contains its coordinates, or move" >&2
  echo "the coordinates. PlaceGameEntities.BeginRun will name the scenario and the map size if" >&2
  echo "one reaches a player." >&2
  exit 1
fi

echo "debug scenarios OK - $checked with hard-coded tiles all fit their maps"
echo "                     ($nopoints place nothing by tile and were not checked)"
