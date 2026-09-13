#!/bin/sh
# Re-baselines src/ onto a newly decompiled decomp/ after a GAME VERSION change.
#
# src/ is the decompiled source plus every fix and port deviation we have made, so it must not
# be overwritten with a fresh decompilation - that would silently discard all of them. But
# decomp/ is committed untouched precisely so the studio's own changes are a reviewable diff,
# which makes this a textbook three-way merge, per file:
#
#     base   = decomp/<asm>/<path> at git HEAD   (the version src/ was derived from)
#     theirs = decomp/<asm>/<path> in the tree   (the version just decompiled)
#     ours   = src/<mapped path>                 (base plus our edits)
#
# Anything the studio changed in a file we never touched applies cleanly. Anything they changed
# in a region we also changed conflicts, and those are exactly the places that need a human -
# our port deviations. Nothing is guessed at.
#
# Usage:
#   build/11-rebaseline.sh            report only, changes nothing
#   build/11-rebaseline.sh --apply    write merged files into src/ (conflicts included, marked)
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

APPLY=0
[ "$1" = "--apply" ] && APPLY=1

# decomp subtree -> src subtree. NOT a plain per-assembly mapping: the port builds four projects
# rather than the shipped six, having folded RoundLines and InputEventSystem into WindowSystem
# (which already depended on both). Each rule is "<decompDir>|<relPrefix>|<srcPrefix>", and only
# paths under relPrefix are mapped - which is what keeps those two assemblies' Properties/ and
# .csproj out of it.
RULES="SpriteSheetRuntime|.|src/SpriteSheetRuntime
RoundLines|./RoundLineCode|src/WindowSystem/RoundLineCode
InputEventSystem|./InputEventSystem|src/WindowSystem/InputEventSystem
WindowSystem|.|src/WindowSystem
UnclaimedWorld|.|src/UnclaimedWorld
Xclna.Xna.Animationx86|.|src/AnimationComponentRuntime"

# Never merged: the project files and assembly-info are ours, hand-written, and carry the pinned
# assembly identities that 53 XNBs depend on (see PORTING-NOTES). Taking the studio's versions
# of those would break asset loading.
excluded() {
  case "$1" in
    */Properties/AssemblyInfo.cs|Properties/AssemblyInfo.cs) return 0 ;;
    *.csproj) return 0 ;;
    *) return 1 ;;
  esac
}

TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT

clean=0; conflict=0; added=0; removed=0; identical=0; skipped=0
CONFLICTS="$TMP/conflicts.txt"; : > "$CONFLICTS"
ADDED="$TMP/added.txt"; : > "$ADDED"
REMOVED="$TMP/removed.txt"; : > "$REMOVED"

echo "==> three-way merging decomp/ -> src/ ($([ $APPLY = 1 ] && echo APPLY || echo 'report only'))"

echo "$RULES" | while IFS='|' read -r asm prefix dest; do
  [ -n "$asm" ] || continue
  echo "$asm|$prefix|$dest"
done > "$TMP/rules.txt"

while IFS='|' read -r asm prefix dest; do
  # Every .cs path the new decompilation has under prefix, plus every one HEAD had, so that
  # files the studio DELETED are considered too.
  {
    [ -d "decomp/$asm" ] && ( cd "decomp/$asm" && find "$prefix" -name '*.cs' 2>/dev/null )
    git ls-tree -r --name-only HEAD "decomp/$asm" 2>/dev/null \
      | sed "s|^decomp/$asm/|./|" | grep '\.cs$' || true
  } | sed 's|^\./\./|./|' | sort -u > "$TMP/paths.txt"

  while IFS= read -r rel; do
    [ -n "$rel" ] || continue
    rel=${rel#./}
    case "./$rel" in "$prefix"/*|"$prefix") ;; *) [ "$prefix" = "." ] || continue ;; esac
    excluded "$rel" && { skipped=$((skipped + 1)); continue; }

    theirs="decomp/$asm/$rel"
    # Strip the rule's prefix so the destination is not doubled up.
    sub=$rel
    [ "$prefix" != "." ] && sub=${rel#"${prefix#./}/"}
    ours="$dest/$sub"
    base="$TMP/base.cs"

    # .gitattributes normalises .cs to LF in the repository and checks it out as CRLF, so
    # `git show` and the working tree NEVER match byte for byte. Comparing them directly made
    # every line of every file look changed and manufactured a conflict in all 621 files that
    # had any edit of ours. All three inputs are therefore compared and merged with line endings
    # stripped, and the result written back as CRLF to match the rest of the tree.
    if git show "HEAD:decomp/$asm/$rel" 2>/dev/null | tr -d '\r' > "$base"; then :; else : > "$base"; fi
    tr -d '\r' < "$theirs" > "$TMP/theirs.cs" 2>/dev/null || : > "$TMP/theirs.cs"

    if [ ! -f "$theirs" ]; then
      if [ -f "$ours" ]; then echo "  $ours" >> "$REMOVED"; removed=$((removed + 1)); fi
      continue
    fi

    if [ ! -s "$base" ]; then
      # No base: a file the studio added, or one we never carried.
      if [ ! -f "$ours" ]; then
        echo "  $ours" >> "$ADDED"; added=$((added + 1))
        [ $APPLY = 1 ] && { mkdir -p "$(dirname "$ours")"; cp -p "$theirs" "$ours"; }
      fi
      continue
    fi

    if cmp -s "$base" "$TMP/theirs.cs"; then identical=$((identical + 1)); continue; fi

    if [ ! -f "$ours" ]; then
      echo "  $ours" >> "$ADDED"; added=$((added + 1))
      [ $APPLY = 1 ] && { mkdir -p "$(dirname "$ours")"; cp -p "$theirs" "$ours"; }
      continue
    fi

    out="$TMP/merged.cs"
    tr -d '\r' < "$ours" > "$out"
    if git merge-file --diff3 -L ours -L base -L theirs "$out" "$base" "$TMP/theirs.cs" >/dev/null 2>&1; then
      clean=$((clean + 1))
      [ $APPLY = 1 ] && sed 's/$/\r/' "$out" > "$ours"
    else
      conflict=$((conflict + 1))
      echo "  $ours" >> "$CONFLICTS"
      [ $APPLY = 1 ] && sed 's/$/\r/' "$out" > "$ours"
    fi
  done < "$TMP/paths.txt"

done < "$TMP/rules.txt"

# Counters accumulate in this shell - the loops read from files rather than pipes, so no
# subshell is involved and they must be printed ONCE here. Emitting a running total per rule
# and summing it afterwards counted every file several times over.
printf '\n    unchanged upstream : %d\n' "$identical"
printf '    merged cleanly     : %d\n' "$clean"
printf '    CONFLICTED         : %d\n' "$conflict"
printf '    added upstream     : %d\n' "$added"
printf '    removed upstream   : %d\n' "$removed"
printf '    skipped (ours)     : %d\n\n' "$skipped"

[ -s "$CONFLICTS" ] && { echo "conflicts (our edits overlap theirs - review each):"; cat "$CONFLICTS"; echo; }
[ -s "$ADDED" ]     && { echo "added upstream:"; cat "$ADDED"; echo; }
[ -s "$REMOVED" ]   && { echo "deleted upstream (still in src/ - decide per file):"; cat "$REMOVED"; echo; }

if [ $APPLY = 0 ]; then
  echo "Nothing was written. Re-run with --apply to merge into src/."
else
  echo "Merged into src/. Conflicts carry <<<<<<< ours / ||||||| base / >>>>>>> theirs markers."
  echo "Find them with:  grep -rl '^<<<<<<< ours' src/"
fi
