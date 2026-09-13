#!/bin/sh
# Syncs kit/ into the published repository checkout and REFUSES if the two disagree.
#
# WHY THIS EXISTS. The kit was being copied into artifacts/gh-repo by hand with tar, and git then
# quietly declined to track five of the files: .gitignore carried `port/` - meant for the build
# output directory at the repo root, written without a leading slash, so it matched a directory
# named "port" anywhere in the tree, and on Windows (core.ignoreCase) "Port" as well. That ate
# newfiles/src/UnclaimedWorld/UWGame/Port/, which is five source files the build cannot do
# without, so every fresh clone failed in phase 1 with
#
#     error CS0234: The type or namespace name 'Port' does not exist in the namespace 'UWGame'
#
# while the zipped kit - not a git checkout, so not subject to .gitignore - built perfectly. The
# pattern is anchored now, but the class of mistake is not one to rely on remembering: anything
# .gitignore swallows is invisible in `git status`, and a published repo that cannot be built is
# the worst failure this project has. So the sync checks rather than trusts.
#
#   usage: sh build/91-sync-gh-repo.sh [<repo-checkout>]      (default artifacts/gh-repo)
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

REPO=${1:-artifacts/gh-repo}
[ -d "$KIT" ] || KIT=kit
[ -d "$REPO/.git" ] || { echo "FATAL: $REPO is not a git checkout" >&2; exit 1; }
[ -f "kit/kit-version.txt" ] || { echo "FATAL: kit/ not generated - run build/90-make-patch-kit.sh first" >&2; exit 1; }

# uwkit.exe is deliberately not published - the README points at the release for it.
echo "==> copying kit/ into $REPO"
tar -cf - --exclude=orchestrator/uwkit.exe -C kit . | tar -xf - -C "$REPO"

# Files that live only in the published repo: its own git metadata and the features/ tree, which
# is documentation the kit does not carry.
echo "==> checking that git will actually track all of it"
( cd kit && find . -type f ! -name uwkit.exe | sed 's|^\./||' | sort ) > "$UW_REPO/.sync-kit.tmp"
( cd "$REPO" && git add -A >/dev/null 2>&1 || true
  git ls-files | grep -v '^features/' | sort ) > "$UW_REPO/.sync-repo.tmp"

MISSING=$(comm -23 "$UW_REPO/.sync-kit.tmp" "$UW_REPO/.sync-repo.tmp" || true)
rm -f "$UW_REPO/.sync-kit.tmp" "$UW_REPO/.sync-repo.tmp"

if [ -n "$MISSING" ]; then
  echo "FATAL: the kit has files the repository is not tracking:" >&2
  echo "$MISSING" | sed 's/^/    /' >&2
  echo >&2
  echo "  Most likely .gitignore is swallowing them. Check with:" >&2
  echo "    git -C $REPO check-ignore -v <one of the paths above>" >&2
  echo "  A published repo missing a source file cannot be built by anyone who clones it." >&2
  exit 1
fi

COUNT=$(find kit/newfiles -name '*.cs' | wc -l | tr -d ' ')
TRACKED=$(cd "$REPO" && git ls-files newfiles | grep -c '\.cs$')
echo "    newfiles: $COUNT in the kit, $TRACKED tracked"
[ "$COUNT" = "$TRACKED" ] || { echo "FATAL: newfiles count disagrees" >&2; exit 1; }

echo "    every kit file is tracked"
echo
echo "==> staged in $REPO - review with 'git -C $REPO status' and commit there"
