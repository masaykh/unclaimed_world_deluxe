#!/bin/sh
# Fails when code nothing can reach comes back.
#
# The September 2026 sweep removed a few thousand lines of it: private methods with no caller,
# fields written and never read, branches behind a constant, public members of the decompiled
# game no file names. Most of it came from the decompiler or from a feature the studio dropped
# halfway; some of it was ours. This gate keeps the cheap, certain part of that from growing back.
#
# WHAT IS CHECKED. Three compiler/analyzer diagnostics, over the game (with every mod compiled in)
# and every project in tools/:
#
#   IDE0051  a private member nothing uses
#   IDE0052  a private member that is assigned but never read
#   CS0162   code after a constant condition, which can never run
#
# They are errors only here. The everyday build does not enforce code style, and should not: a
# half-finished edit ought to compile. So the gate writes a temporary root .editorconfig that
# raises the three to warnings, rebuilds with EnforceCodeStyleInBuild, and collects them.
#
# WHAT IS ALLOWED. tools/build/85-dead-code-allowlist.txt lists the findings that were looked at
# and kept on purpose. The usual reasons are in that file: a struct field that is part of a
# dictionary key's equality, a field a regulator's constructor fills so the replay's random stream
# stays in step, a snapshot field kept so old saves still line up. Entries are keyed by code, file
# and member, never by line, so an unrelated edit does not break the gate. An entry that no
# longer matches anything is reported as stale and also fails: the list must not rot either.
#
# NOT CHECKED. Public members nobody calls (no analyzer can see a mod or Harmony patch that might),
# unused parameters (IDE0060: overrides and event handlers must keep their signature) and
# assignments the decompiler left behind (IDE0059). Those were swept by hand and are not gated.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

ALLOW=tools/build/85-dead-code-allowlist.txt
[ -f "$ALLOW" ] || { echo "missing $ALLOW"; exit 1; }
if [ -e .editorconfig ]; then
  echo "a root .editorconfig exists; this gate writes its own and will not overwrite yours"
  exit 1
fi

WORK=$(mktemp -d)
trap 'rm -f "$UW_REPO/.editorconfig"; rm -rf "$WORK"' EXIT INT TERM
printf 'root = true\n[*.cs]\ndotnet_diagnostic.IDE0051.severity = warning\ndotnet_diagnostic.IDE0052.severity = warning\ndotnet_diagnostic.CS0162.severity = warning\n' > .editorconfig

# GL builds everywhere, so CI (ubuntu) can run this. DX is added on Windows, which is the only
# place it builds: code under #if UW_DX is then checked too. The findings of both are merged, so
# an allowlist entry needs to match in only one of them.
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) PLATFORMS="${UW_DEADCODE_PLATFORMS:-GL DX}" ;;
  *)                    PLATFORMS="${UW_DEADCODE_PLATFORMS:-GL}" ;;
esac

echo "==> dead-code check ($PLATFORMS)"
for platform in $PLATFORMS; do
  for proj in base_game/UnclaimedWorld/UnclaimedWorld.csproj tools/*/*.csproj; do
    echo "  building $proj ($platform)"
    "$DOTNET" build "$proj" -c Release -p:UwPlatform=$platform -p:EnforceCodeStyleInBuild=true \
      -p:TreatWarningsAsErrors=false --no-incremental -v q -nologo >> "$WORK/build.log" 2>&1 || {
      tail -30 "$WORK/build.log"; echo "build failed: $proj ($platform)"; exit 1; }
  done
done

# "C:\...\base_game\X\Y.cs(12,5): warning IDE0052: Private member 'Y.field' can be removed ..."
# becomes "IDE0052<TAB>base_game/X/Y.cs<TAB>Y.field". CS0162 names no member, so it is keyed by
# the method it sits in, which the allowlist spells as the file alone plus "-".
repo_win=$(cd "$UW_REPO" && pwd -W 2>/dev/null || pwd)
grep -E 'warning (IDE0051|IDE0052|CS0162):' "$WORK/build.log" |
  sed 's|\\|/|g' |
  awk -v root="$(printf '%s' "$repo_win" | sed 's|\\|/|g')" '
    {
      line = $0
      path = line; sub(/\([0-9]+,[0-9]+\): warning .*/, "", path); sub(/^ +/, "", path)
      if (index(tolower(path), tolower(root) "/") == 1) path = substr(path, length(root) + 2)
      code = line; sub(/.*warning /, "", code); sub(/:.*/, "", code)
      member = "-"
      if (match(line, /'\''[^'\'']+'\''/)) member = substr(line, RSTART + 1, RLENGTH - 2)
      print code "\t" path "\t" member
    }' | sort -u > "$WORK/found.txt"

grep -vE '^[[:space:]]*(#|$)' "$ALLOW" | sed 's/[[:space:]]*#.*$//' |
  awk '{ print $1 "\t" $2 "\t" $3 }' | sort -u > "$WORK/allowed.txt"

comm -23 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/new.txt"
comm -13 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/stale.txt"

FAIL=0
if [ -s "$WORK/new.txt" ]; then
  echo
  echo "  FAIL  unreachable or unused code (remove it, or add it to $ALLOW with a reason):"
  sed 's/^/        /' "$WORK/new.txt"
  FAIL=1
fi
if [ -s "$WORK/stale.txt" ]; then
  echo
  echo "  FAIL  allowlist entries that no longer match anything (delete them):"
  sed 's/^/        /' "$WORK/stale.txt"
  FAIL=1
fi
[ $FAIL -eq 0 ] || exit 1
echo
echo "no dead code - $(wc -l < "$WORK/found.txt" | tr -d ' ') finding(s), all allowlisted."
