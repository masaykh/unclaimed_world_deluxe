#!/usr/bin/env bash
# Writes translations/English (US).xml, the template a translator copies (translations/README.md),
# from what the game actually asks Locale for.
#
#   bash tools/build/37-make-strings.sh            rewrite the file
#   bash tools/build/37-make-strings.sh --check    fail if it is out of date (gate 80 runs this)
#
# WHERE THE ENTRIES COME FROM.
#   (GUI)...              every string literal passed to Locale.Text(...) in base_game/ and mods/,
#                         collected here. The key is "(GUI)" and the English, as in the studio's file.
#   (SETTING...)          every registered mod setting's label and tooltip, and each heading in the
#                         MODS section, from the registry - tools/DataExport --strings-literals.
#   (ITEM...)             every item's name and description, from the tables the game builds.
#
# A Locale.Text whose argument is built - "a" + b, or $"..." - has no fixed English to put in the
# template, so it is refused here: pass a literal, and string.Format the variable part into it.
#
# Needs the DX game and DataExport built (gate 80 builds both), or it builds them.
set -eo pipefail
cd "$(dirname "$0")/../.."

. tools/build/env.sh
set -u

CHECK=0
[ "${1:-}" = "--check" ] && CHECK=1
TEMPLATE="translations/English (US).xml"
WORK="${TMPDIR:-/tmp}/uw-strings"
rm -rf "$WORK"; mkdir -p "$WORK/game"

# NUL-separated and unquoted: one source file has a non-ASCII name (ÏnfluenceRender.cs).
sources() { git -c core.quotePath=false ls-files -z 'base_game/*.cs' 'mods/*.cs'; }

# A built argument cannot be a template entry.
built=$(sources | xargs -0 perl -ne 'print "$ARGV:$.: $_" if /Locale\.Text\(\s*(\$"|"(?:[^"\\]|\\.)*"\s*\+)/; close ARGV if eof' || true)
if [ -n "$built" ]; then
  echo "FAIL  Locale.Text with a built argument - pass a literal and string.Format the rest:" >&2
  echo "$built" >&2
  exit 1
fi

# One literal per line, as written in C#; DataExport reads the escapes.
sources | xargs -0 perl -ne 'while (/Locale\.Text\(\s*"((?:[^"\\]|\\.)*)"\s*\)/g) { print "$1\n" }' | sort -u > "$WORK/literals.txt"

EXPORT=$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' 2>/dev/null | head -1 || true)
if [ -z "$EXPORT" ]; then
  "$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=DX -v q --nologo
  "$DOTNET" build tools/DataExport/DataExport.csproj -c Release -v q --nologo
  EXPORT=$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' | head -1)
fi
# A fresh folder: no user/ModSettings.xml, so English and every setting at its default.
"$EXPORT" "$WORK/game" --strings-literals="$WORK/literals.txt" --strings-out="$WORK/strings.xml" | grep '^==> strings' || {
  echo "FAIL  DataExport did not write the strings" >&2; exit 1; }

if [ "$CHECK" = 1 ]; then
  # Line endings aside: git writes .xml with the platform's.
  if ! diff <(tr -d '\r' < "$TEMPLATE") <(tr -d '\r' < "$WORK/strings.xml") > "$WORK/diff.txt"; then
    echo "FAIL  $TEMPLATE is out of date - run: bash tools/build/37-make-strings.sh" >&2
    head -20 "$WORK/diff.txt" >&2
    exit 1
  fi
  echo "ok    $TEMPLATE has every string the game asks for ($(grep -c '<Key>' "$TEMPLATE") entries)"
else
  cp "$WORK/strings.xml" "$TEMPLATE"
  echo "==> wrote $TEMPLATE ($(grep -c '<Key>' "$TEMPLATE") entries)"
fi
