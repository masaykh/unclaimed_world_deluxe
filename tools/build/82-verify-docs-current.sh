#!/bin/sh
# Fails if a document still asserts something the repository has stopped doing.
#
# WHY THIS EXISTS. Four times in one day a claim outlived the thing it described: the README said
# the repository shipped no assets after all of them were pushed; license.md said the compiled
# content "is not part of the released materials" after that was checked and disproved; the
# release description told a player to copy a Content folder that was already inside the archive.
# Every one was caught by a reader, not by the build, and every one was in the first paragraph
# somebody reads.
#
# These are cheap string checks, not a linter. Each pattern below was a real sentence that was
# real and became false. Add to it when the next one happens.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

DOCS="README.md license.md build.md modding.md how_to_use_mods.md assets/README.md
      translations/README.md tools/shadowdusk/README.md
      tools/build/_release-install-md.sh tools/build/71-publish-release.sh
      "

fail=0
flag() {
  # $1 = pattern, $2 = why it is wrong now
  hits=$(grep -rniE "$1" $DOCS 2>/dev/null | grep -viE '^[^:]+:[0-9]+: *#' || true)
  [ -z "$hits" ] && return 0
  echo "  !! $2" >&2
  echo "$hits" | sed 's/^/       /' >&2
  fail=1
}

echo "==> checking documents against what this repository actually does"

# The repository carries the studio's asset SOURCES - all of them.
flag "no game content|contains? no (game )?content|code and build tooling only" \
     "says the repository ships no game content; assets/ holds 3991 files of it"

flag "not part of (the|what) (the )?(studio|refactored games)? ?(released|releases)" \
     "says assets are not part of the released materials; all 549 shipped assets trace to a released source"

# A --full archive is standalone. Only the CI archives need Content copied in, and they say so
# in their own paragraph, which is why this looks for the instruction OUTSIDE that context.
flag "you need to own the game.{0,40}for (its|the) .?Content" \
     "tells a player to copy Content; the published archives are standalone"

# The GL build has one shader compiler and mgfxc is not it.
flag "mgfxc.{0,40}(builds|compiles) the effects|run mgfxc" \
     "points at mgfxc; the effects are built by patched ShadowDusk"

# There are three feature booleans now, and base_game builds with mods/ deleted. Both of those
# sentences replaced one that had been true for months, which is the usual way this file grows.
flag "two booleans" \
     "says two feature booleans; there are three (UwHarmony, UwUnhiddenMod, UwGameplayMods)"

flag "only UnhiddenMod and ModLoader have (one|a stub)|needs an Absent stub per mod" \
     "says the Absent stubs are incomplete; every gameplay mod has one and CI builds with mods/ deleted"

# Scripts that were pruned when the repository split from the porting tree.
for s in 00-snapshot-game 10-decompile 30-transcode-effects 31-build-gl-effects \
         33-convert-effects-to-gl 61-package-dx 90-make-patch-kit verify-identities; do
  flag "build/$s" "references $s, which is not in this repository"
done

echo
if [ "$fail" = 0 ]; then
  echo "==> documents are current"
else
  echo "==> FIX THESE. A document that is confidently wrong is worse than one that is missing:" >&2
  echo "    the reader has no reason to doubt it." >&2
  exit 1
fi
