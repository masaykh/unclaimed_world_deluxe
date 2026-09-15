#!/bin/sh
# Publishes the archives in artifacts/release/ as a GitHub Release.
#
#     sh tools/build/70-make-release.sh 1.0
#     sh tools/build/71-publish-release.sh v1.0
#
# Separate from the build on purpose. Building is repeatable and local; publishing is neither -
# it puts files somewhere other people can download them, under a tag that is awkward to move
# once anyone has fetched it. So it is its own command, it shows you exactly what it is about to
# do, and it asks before doing it.
#
#     --draft     publish as a draft, visible only to you until you release it
#     --yes       skip the confirmation (for scripting; think before using it)
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

TAG=""
DRAFT=""
ASSUME_YES=""
for arg in "$@"; do
  case "$arg" in
    --draft) DRAFT="--draft" ;;
    --yes|-y) ASSUME_YES=1 ;;
    -*) echo "unknown option: $arg" >&2; exit 2 ;;
    *)  TAG="$arg" ;;
  esac
done

if [ -z "$TAG" ]; then
  echo "usage: sh tools/build/71-publish-release.sh <tag> [--draft] [--yes]" >&2
  echo "  e.g. sh tools/build/71-publish-release.sh v1.0" >&2
  exit 2
fi

command -v gh >/dev/null 2>&1 || {
  echo "FATAL: GitHub CLI (gh) not found on PATH." >&2
  echo "       On Windows it installs to /c/Program Files/GitHub CLI/ and is not added to PATH:" >&2
  echo "         export PATH=\"/c/Program Files/GitHub CLI:\$PATH\"" >&2
  exit 2
}
gh auth status >/dev/null 2>&1 || { echo "FATAL: gh is not authenticated. Run: gh auth login" >&2; exit 2; }

OUT="$UW_REPO/artifacts/release"
set -- "$OUT"/*.7z
[ -e "$1" ] || { echo "FATAL: no archives in $OUT - run 70-make-release.sh first" >&2; exit 1; }

# A release built from a dirty tree is not reproducible from its tag, and nobody can tell that by
# looking at it afterwards. Warn loudly; it is still the user's call.
DIRTY=""
if [ -n "$(git status --porcelain 2>/dev/null)" ]; then
  DIRTY=" (WORKING TREE IS DIRTY - this release will not match its tag)"
fi

REPO=$(gh repo view --json nameWithOwner --jq .nameWithOwner 2>/dev/null || echo "?")
PRIVATE=$(gh repo view --json isPrivate --jq .isPrivate 2>/dev/null || echo "?")

echo "About to publish:"
echo
echo "    repository  $REPO   (private: $PRIVATE)"
echo "    tag         $TAG${DRAFT:+   [DRAFT]}"
echo "    commit      $(git rev-parse --short HEAD)$DIRTY"
echo "    assets:"
for f in "$@"; do printf "      %-52s %s\n" "$(basename "$f")" "$(du -h "$f" | cut -f1)"; done
echo
if [ "$PRIVATE" = "false" ]; then
  echo "    This repository is PUBLIC. These files become downloadable by anyone."
  echo
fi

if [ -z "$ASSUME_YES" ]; then
  printf "Publish? [y/N] "
  read -r reply
  case "$reply" in y|Y|yes|YES) ;; *) echo "Cancelled."; exit 0 ;; esac
fi

# The tag has to exist before a release can hang off it. Create it here only if it does not,
# and push it - `gh release create` would make one from the default branch otherwise, which is
# not necessarily what is being published.
if ! git rev-parse "$TAG" >/dev/null 2>&1; then
  echo "==> tagging $TAG at $(git rev-parse --short HEAD)"
  git tag "$TAG"
fi
git push origin "$TAG" 2>&1 | tail -2

echo "==> creating the release"
gh release create "$TAG" "$@" \
  $DRAFT \
  --title "Unclaimed World Deluxe ${TAG#v}" \
  --generate-notes \
  --notes "
*Unclaimed World* on .NET 8 and DesktopGL — Windows, Linux and macOS, with seven gameplay mods
you can switch off.

**Download, extract, run.** Nothing to install, nothing to copy.

| download | for |
|---|---|
| \`...-win-x64.7z\` | **Windows — start here** |
| \`...-linux-x64.7z\` | Linux |
| \`...-osx-x64.7z\` | macOS, Intel |
| \`...-osx-arm64.7z\` | macOS, Apple Silicon |
| \`...-win-x64-dx.7z\` | Windows, older graphics drivers — see below |

### If a map will not load on Windows

The game now checks at startup and **tells you** when this machine's OpenGL driver is too small
for its shaders, naming the numbers. If you see that message - or if an older build died as soon
as a map loaded, with \`Failed to compile vertex shader\` in \`Errors.txt\` - take the
**\`win-x64-dx\`** archive instead. It is the same game built on DirectX, and it
runs the studio's own compiled shaders rather than recompiled ones — which is what some older
Intel drivers refuse. Reported on Intel HD Graphics with a 2016 driver.

Everyone else should take \`win-x64\`. DirectX is Windows-only and is not the build this port is
developed against.

Needs the [.NET 8 runtime](https://dotnet.microsoft.com/download/dotnet/8.0). macOS builds are
unsigned, so Gatekeeper needs a word — \`install.md\` inside has the command.

---

**This is an unofficial community project — not endorsed by, affiliated with, or supported by
Refactored Games.** Please don't take problems with this build to them.

It exists because Refactored Games released *Unclaimed World*'s source **and its assets** under
the Unclaimed World Community License. That was a generous thing to do, and it is the only
reason any of this is possible.

**Please [buy the game](https://store.steampowered.com/app/284100/)** if you have not — this is
a port of someone's work, not a way around paying for it.
"

echo
gh release view "$TAG" --json url --jq .url
