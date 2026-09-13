#!/bin/sh
# Phase 5: rewrite the game's MGFX v8 effect containers (MonoGame 3.6) as MGFX v10, which is
# what MonoGame 3.8.1+ requires. Originals are copied to ref/effects-mgfx8-original/ first.
#
# Idempotent: effects already at v10 are reported and skipped, so this is safe to re-run.
# To start over, re-run build/00-snapshot-game.sh to restore game/ from the Steam install.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

TOOL=artifacts/bin/MgfxTranscode/debug/mgfxtranscode.exe
[ -f "$TOOL" ] || { echo "==> building mgfxtranscode"; "$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -v q --nologo; }

echo "==> transcoding effects in game/Content"
mkdir -p ref/effects-mgfx8-original
"$TOOL" transcode game/Content --backup ref/effects-mgfx8-original

echo "==> validating every effect parses as MGFX v10"
# skinFX_0.xnb is deliberately excluded: it is MGFX v7, was already unloadable by the game's
# own MonoGame 3.6 build, and no model XNB or line of code references it.
"$TOOL" validate game/Content 2>&1 | grep -v 'skinFX_0'
