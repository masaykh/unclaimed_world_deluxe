#!/bin/sh
# Phase 2: decompile every shipped managed assembly to a C# project under decomp/.
# The output of this script is COMMITTED UNTOUCHED and never hand-edited - it is the
# reference baseline that every later fix in src/ is diffed against.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

ASMS="SpriteSheetRuntime.dll RoundLines.dll InputEventSystem.dll Xclna.Xna.Animationx86.dll WindowSystem.dll UnclaimedWorld.exe MonoGame.Framework.dll"

for a in ${1:-$ASMS}; do
  name="${a%.*}"
  echo "==> $a -> decomp/$name"
  rm -rf "decomp/$name"
  mkdir -p "decomp/$name"
  "$DOTNET" tool run ilspycmd -- -p -o "decomp/$name" "ref/original/$a" \
    --nested-directories --use-varnames-from-pdb 2>&1 | tail -5
  echo "    $(find "decomp/$name" -name '*.cs' | wc -l) .cs files, $(find "decomp/$name" -type f | wc -l) files total"
done
