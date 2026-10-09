#!/usr/bin/env bash
# Writes translations/English (US).xml, the template a translator copies (translations/README.md),
# from what the game actually asks Locale for.
#
#   bash tools/build/37-make-strings.sh                    rewrite the file (and the unrouted counts)
#   bash tools/build/37-make-strings.sh --check            fail if either is out of date (gate 80)
#   bash tools/build/37-make-strings.sh --compare FILE     what a translation lacks, has extra, or
#                                                          left in English; fails on a broken {0}
#   bash tools/build/37-make-strings.sh --pseudo [OUT]     a pseudo-language, for finding text not
#                                                          yet translatable and text that does not
#                                                          fit (default artifacts/strings/Pseudo.xml)
#
# UNROUTED TEXT. tools/build/37-unrouted.txt counts, per file, the English still written straight
# into an interface control, a message box or the log (37-strings.pl unrouted). It is a ratchet:
# a file may go down and never up, and a file not in it must have none. Rewriting the template
# lowers the counts; --check fails while they are stale.
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

TEMPLATE="translations/English (US).xml"
HELPER=tools/build/37-strings.pl
UNROUTED=tools/build/37-unrouted.txt

case "${1:-}" in
  --compare)
    [ -n "${2:-}" ] || { echo "usage: $0 --compare <translation.xml>" >&2; exit 2; }
    exec perl "$HELPER" compare "$TEMPLATE" "$2" ;;
  --pseudo)
    out=${2:-artifacts/strings/Pseudo.xml}
    mkdir -p "$(dirname "$out")"
    perl "$HELPER" pseudo "$TEMPLATE" "$out"
    echo "    copy it into the game's data/BaseData/Strings and choose Pseudo in PORT -> LANGUAGE:"
    echo "    text without [Ж ... Ж] is not translatable yet; a cut-off one does not fit."
    exit 0 ;;
esac

CHECK=0
[ "${1:-}" = "--check" ] && CHECK=1
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

# A display name is translated, so nothing may decide anything by comparing one with English.
# Terrain's save did ("SurfaceType.Name.Contains("Water")", the studio's HACK) and would have saved
# every water tile as plains under a translation. KeyName is the identity. The simulation and the
# mods only: a control's Name in the interface is an identifier. Model bone names are
# not display text, and comments are not code. The same for every other member Locale.DataTexts
# translates (Description, PluralName, Term, DefaultText...).
named=$(git -c core.quotePath=false ls-files -z "base_game/UnclaimedWorld/UWGame/SimSide/*.cs" "mods/*.cs" | xargs -0 perl -ne 'next if m{^\s*(//|/\*|\*)}; next if /[Bb]one\.Name/; my $m = q{(?:Name|PluralName|Description|SummaryDescription|ShortDescription|DisplayName|Heading|Text|Tooltip|ToolTip|DefaultText|LoseScreenText|UserCannotCancelReason|SpecialActionCaption|Term|TermTooltip|StaticString|TextWithPlaceholders)}; print "$ARGV:$.: $_" if /\.$m\s*(==|!=)\s*"[^"]+"|\.$m\.(Contains|StartsWith|EndsWith|Equals|IndexOf)\(\s*"/; close ARGV if eof' || true)
if [ -n "$named" ]; then
  echo "FAIL  a display name compared with English text - translated, it stops matching; compare KeyName:" >&2
  echo "$named" >&2
  exit 1
fi

# One literal per line, as written in C#; DataExport reads the escapes.
sources | xargs -0 perl -ne 'while (/Locale\.Text\(\s*"((?:[^"\\]|\\.)*)"\s*\)/g) { print "$1\n" }' | sort -u > "$WORK/literals.txt"

# Counts: Locale.Count(n, "one", "other"), one tab-separated pair per line. Both must be literals.
bad=$(sources | xargs -0 perl -ne 'next unless /Locale\.Count\(/; print "$ARGV:$.: $_" unless /Locale\.Count\([^,]+,\s*"(?:[^"\\]|\\.)*"\s*,\s*"(?:[^"\\]|\\.)*"\s*\)/; close ARGV if eof' || true)
if [ -n "$bad" ]; then
  echo "FAIL  Locale.Count needs its two forms as literals:" >&2
  echo "$bad" >&2
  exit 1
fi
sources | xargs -0 perl -ne 'while (/Locale\.Count\([^,]+,\s*"((?:[^"\\]|\\.)*)"\s*,\s*"((?:[^"\\]|\\.)*)"\s*\)/g) { print "$1\t$2\n" }' | sort -u > "$WORK/counts.txt"

EXPORT=$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' 2>/dev/null | head -1 || true)
if [ -z "$EXPORT" ]; then
  "$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=DX -v q --nologo
  "$DOTNET" build tools/DataExport/DataExport.csproj -c Release -v q --nologo
  EXPORT=$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' | head -1)
fi
# A fresh folder: no user/ModSettings.xml, so English and every setting at its default.
"$EXPORT" "$WORK/game" --strings-literals="$WORK/literals.txt" --strings-counts="$WORK/counts.txt" --strings-out="$WORK/strings.xml" | grep '^==> strings' || {
  echo "FAIL  DataExport did not write the strings" >&2; exit 1; }

# The interface's and the simulation's unrouted text, per file, against the ratchet. Not the data
# loaders: their text is the tables', translated by key (Locale.DataTexts).
git -c core.quotePath=false ls-files -z 'base_game/UnclaimedWorld/UWGame/ClientSide/*.cs' 'base_game/UnclaimedWorld/UWGame/Client/*.cs' \
  'base_game/UnclaimedWorld/UWGame/SimSide/*.cs' ':(exclude)base_game/UnclaimedWorld/UWGame/SimSide/AllGameData/*' \
  'base_game/UnclaimedWorld/GameStateManagement/*.cs' 'mods/*.cs' \
  | xargs -0 perl "$HELPER" unrouted | sort -k2 > "$WORK/unrouted.txt"
[ -f "$UNROUTED" ] || : > "$UNROUTED"
risen=$(awk -F'\t' 'NR == FNR { allowed[$2] = $1; next } $1 > ($2 in allowed ? allowed[$2] : 0) { print "      " $2 ": " $1 " (allowed " ($2 in allowed ? allowed[$2] : 0) ")" }' \
  <(tr -d '\r' < "$UNROUTED") "$WORK/unrouted.txt")
if [ -n "$risen" ]; then
  echo "FAIL  English written straight into the interface, where Locale cannot reach it - use Locale.Text:" >&2
  echo "$risen" >&2
  exit 1
fi
total=$(awk -F'\t' '{ n += $1 } END { print n + 0 }' "$WORK/unrouted.txt")

if [ "$CHECK" = 1 ]; then
  # Line endings aside: git writes .xml with the platform's.
  if ! diff <(tr -d '\r' < "$TEMPLATE") <(tr -d '\r' < "$WORK/strings.xml") > "$WORK/diff.txt"; then
    echo "FAIL  $TEMPLATE is out of date - run: bash tools/build/37-make-strings.sh" >&2
    head -20 "$WORK/diff.txt" >&2
    exit 1
  fi
  if ! diff <(tr -d '\r' < "$UNROUTED") "$WORK/unrouted.txt" > /dev/null; then
    echo "FAIL  $UNROUTED is stale - text was routed; lower the counts: bash tools/build/37-make-strings.sh" >&2
    exit 1
  fi
  echo "ok    $TEMPLATE has every string the game asks for ($(grep -c '<Key>' "$TEMPLATE") entries); $total unrouted left in $(wc -l < "$UNROUTED") files"
else
  cp "$WORK/strings.xml" "$TEMPLATE"
  cp "$WORK/unrouted.txt" "$UNROUTED"
  echo "==> wrote $TEMPLATE ($(grep -c '<Key>' "$TEMPLATE") entries); $total unrouted left in $(wc -l < "$UNROUTED") files"
fi
