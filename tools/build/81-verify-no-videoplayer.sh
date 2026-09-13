#!/bin/sh
# Asserts that NOTHING the port ships references MonoGame's video stack.
#
# PORT DEVIATION 17 replaced the WMV menu background with a motion-JPEG sequence and removed
# VideoPlayer. This is the check that keeps it removed: the video types are still sitting in
# MonoGame.Framework, so a single `using Microsoft.Xna.Framework.Media;` plus a `new
# VideoPlayer()` would quietly bring the dependency back - and on DesktopGL that constructor
# throws, so the failure would show up as a crash on a platform nobody tests every day.
#
# HOW. .NET metadata stores type and member references as plain UTF-8 in the assembly's string
# heap, so an assembly that references VideoPlayer contains the literal bytes "VideoPlayer".
# Grepping for them is crude but has no false negatives, which is the direction that matters for
# a gate. A false positive - the name appearing in a string literal or an embedded resource -
# would fail the build, which is the safe way to be wrong.
#
# Only OUR assemblies are checked. MonoGame.Framework obviously contains VideoPlayer; the claim
# being tested is that the port does not reach for it.
#
# MediaPlayer, MediaState and Song are expected and NOT failures: the nine music tracks are WMA
# and still play through MediaFoundation Song on DirectX. Deviation 17 removed the video
# dependency, not the audio one - see PlatformWindow.CheckMediaCodecsAvailable.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

# Type names that must not appear. Kept narrow on purpose: "Video" alone would match
# "MainMenuIntro" documentation strings and the like.
FORBIDDEN='VideoPlayer|VideoReader|IVideoPlayer'

OURS='UnclaimedWorld.dll WindowSystem.dll SpriteSheetRuntime.dll Xclna.Xna.Animationx86.dll'
FAILURES=0
CHECKED=0

check_dir() {
  dir=$1; label=$2
  [ -d "$dir" ] || { echo "  --    $label: not built, skipped"; return; }
  for name in $OURS; do
    f="$dir/$name"
    [ -f "$f" ] || continue
    CHECKED=$((CHECKED + 1))
    hits=$(grep -aoE "$FORBIDDEN" "$f" 2>/dev/null | sort -u | tr '\n' ' ')
    if [ -n "$hits" ]; then
      printf '  FAIL  %-22s %-14s references: %s\n' "$label" "$name" "$hits"
      FAILURES=$((FAILURES + 1))
    else
      printf '  ok    %-22s %-14s clean\n' "$label" "$name"
    fi
  done
}

echo "==> no-VideoPlayer check"
check_dir artifacts/publish/UnclaimedWorld/release_dx "WindowsDX publish"
check_dir artifacts/publish/UnclaimedWorld/release_gl "DesktopGL publish"
check_dir artifacts/package/UnclaimedWorld-Win64      "Win64 package"

# The shipped tools too - ContentProbe used to probe the video asset, which made it the last
# thing in the repository that constructed a MonoGame Video.
for f in $(find artifacts/bin/ContentProbe -name 'contentprobe.dll' 2>/dev/null); do
  CHECKED=$((CHECKED + 1))
  hits=$(grep -aoE "$FORBIDDEN" "$f" 2>/dev/null | sort -u | tr '\n' ' ')
  if [ -n "$hits" ]; then
    printf '  FAIL  %-22s %-14s references: %s\n' "tool" "contentprobe" "$hits"
    FAILURES=$((FAILURES + 1))
  else
    printf '  ok    %-22s %-14s clean\n' "tool" "contentprobe"
  fi
done

echo
if [ "$CHECKED" -eq 0 ]; then
  echo "no assemblies found to check - build first." >&2
  exit 1
fi
if [ "$FAILURES" -eq 0 ]; then
  echo "VideoPlayer-independent: $CHECKED assembly/assemblies clean."
else
  echo "VideoPlayer dependency reintroduced in $FAILURES assembly/assemblies." >&2
  exit 1
fi
