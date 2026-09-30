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
# and two more, in OUR code only - mods/, tools/, base_game/Shared, UWGame/Port, UWGame/Mods and
# ReplayTrace.cs:
#
#   IDE0060  an unused parameter
#   IDE0059  a value assigned and overwritten or dropped before anyone reads it
#
# In the decompiled game these are ~800 findings of decompiler noise and signatures that overrides
# and event handlers must keep. In code we wrote they are mistakes. A new file of our own belongs
# in OURS below.
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
# NOT CHECKED. Public members nobody calls (no analyzer can see a mod or Harmony patch that might):
# tools/build/86-verify-no-orphans.sh covers the cases where we can know better.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

ALLOW=tools/build/85-dead-code-allowlist.txt
[ -f "$ALLOW" ] || { echo "missing $ALLOW"; exit 1; }

# Every entry says why it is kept: a "# reason" on its own line, or a comment block above its
# group (the lines since the last blank one). Checked before the slow part.
unexplained=$(tr -d '\r' < "$ALLOW" | awk '
  /^[[:space:]]*$/ { why = 0; next }
  /^[[:space:]]*#/ { why = 1; next }
  { if (!why && $0 !~ /#/) print "        " NR ": " $0 }')
if [ -n "$unexplained" ]; then
  echo "  FAIL  allowlist entries with no reason (add a # comment on the line or above its group):"
  echo "$unexplained"
  exit 1
fi

if [ -e .editorconfig ]; then
  echo "a root .editorconfig exists; this gate writes its own and will not overwrite yours"
  exit 1
fi

WORK=$(mktemp -d)
trap 'rm -f "$UW_REPO/.editorconfig"; rm -rf "$WORK"' EXIT INT TERM
OURS='{mods/**.cs,tools/**.cs,base_game/Shared/**.cs,base_game/UnclaimedWorld/UWGame/Port/**.cs,base_game/UnclaimedWorld/UWGame/Mods/**.cs,base_game/UnclaimedWorld/UWGame/Control/Replays/ReplayTrace.cs}'
cat > .editorconfig <<EOF
root = true
[*.cs]
dotnet_diagnostic.IDE0051.severity = warning
dotnet_diagnostic.IDE0052.severity = warning
dotnet_diagnostic.CS0162.severity = warning
[$OURS]
dotnet_diagnostic.IDE0060.severity = warning
dotnet_diagnostic.IDE0059.severity = warning
EOF

# GL builds everywhere, so CI (ubuntu) can run this. DX is added on Windows, which is the only
# place it builds: code under #if UW_DX is then checked too. The findings of both are merged, so
# an allowlist entry needs to match in only one of them.
# Off Windows, a tool that targets only net8.0-windows (DataExport, XmlProxyGen) is skipped: it
# does not build there, and it is checked on every Windows run.
case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*) PLATFORMS="${UW_DEADCODE_PLATFORMS:-GL DX}"; WINDOWS=1 ;;
  *)                    PLATFORMS="${UW_DEADCODE_PLATFORMS:-GL}";    WINDOWS=0 ;;
esac
# --quick, for before a commit: GL only, the game, and only the tools with uncommitted changes.
# Anything not built reports nothing, so the stale-entry check is skipped. CI runs the full check.
QUICK=0
[ "${1:-}" = "--quick" ] && { QUICK=1; PLATFORMS=GL; }
PROJECTS=base_game/UnclaimedWorld/UnclaimedWorld.csproj
for proj in tools/*/*.csproj; do
  if [ $WINDOWS -eq 0 ] && ! grep -q '<TargetFramework>net[0-9.]*</TargetFramework>' "$proj"; then
    echo "  --    $proj: Windows-only, skipped"
    continue
  fi
  if [ $QUICK -eq 1 ] && [ -z "$(git status --porcelain -- "$(dirname "$proj")")" ]; then
    continue
  fi
  PROJECTS="$PROJECTS $proj"
done

echo "==> dead-code check ($PLATFORMS)"
for platform in $PLATFORMS; do
  for proj in $PROJECTS; do
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
grep -E 'warning (IDE0051|IDE0052|IDE0059|IDE0060|CS0162):' "$WORK/build.log" |
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

tr -d '\r' < "$ALLOW" | grep -vE '^[[:space:]]*(#|$)' | sed 's/[[:space:]]*#.*$//' |
  awk '{ print $1 "\t" $2 "\t" $3 }' | sort -u > "$WORK/allowed.txt"

comm -23 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/new.txt"
comm -13 "$WORK/found.txt" "$WORK/allowed.txt" > "$WORK/stale.txt"
[ $QUICK -eq 1 ] && : > "$WORK/stale.txt"

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
