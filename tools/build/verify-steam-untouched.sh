#!/bin/sh
# Re-verify the Steam install is byte-identical to the Phase 0 snapshot. Run this any time.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_STEAM"
sha256sum -c --quiet "$UW_REPO/ref/manifest.sha256" && echo "Steam install UNCHANGED (all files match ref/manifest.sha256)"
