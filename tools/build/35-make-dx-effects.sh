#!/bin/sh
# Builds the DirectX-profile effects for a WindowsDX build, into artifacts/content/effects-dx/.
#
# NOTHING IS COMPILED HERE, and that is the whole point. The 19 shipped effects are already
# DirectX-profile DXBC - MGFX version 8, profile byte 1 - compiled by Refactored Games with the
# studio's own fxc. MonoGame 3.8 rejects the v8 CONTAINER ("This MGFX effect is for an older
# release of MonoGame") and nothing else about them: tools/MgfxTranscode rewrites the header to
# v10 and copies every byte of shader bytecode through unchanged.
#
# WHY THIS MATTERS BEYOND CONVENIENCE. The GL build recompiles all 19 from the HLSL in
# assets/effects/ through ShadowDusk, and a recompile is where a shader can come out subtly
# different from the one the studio shipped. This path cannot: the bytecode IS the studio's.
# That is why build.md calls WindowsDX the correctness oracle, and it is why a DX archive is the
# fallback when a GL driver rejects a recompiled shader - see tools/build/70-make-release.sh.
#
# The output is an OVERRIDE folder, not a replacement Content/: UwContentManager searches it
# first, so the player's own files are never modified. Same mechanism the GL build uses.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

# Defaults to the PRISTINE Steam content, because that is the only copy guaranteed to still be
# the shipped MGFX v8 - game/Content has been transcoded in place by earlier build steps, and
# converting an already-converted effect is a no-op that would look like success. This script
# only ever READS the source; build/verify-steam-untouched.sh checks that.
SRC=${1:-$UW_STEAM/Content}
OUT=${2:-artifacts/content/effects-dx}

[ -d "$SRC" ] || { echo "FATAL: no such content directory: $SRC" >&2; exit 1; }

"$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -c Release -v q --nologo
TOOL=$(find artifacts/bin/MgfxTranscode -name 'mgfxtranscode.exe' -path '*release*' | head -1)
[ -n "$TOOL" ] || TOOL=$(find artifacts/bin/MgfxTranscode -name 'mgfxtranscode.exe' | head -1)
[ -n "$TOOL" ] || { echo "FATAL: mgfxtranscode.exe not found" >&2; exit 1; }

rm -rf "$OUT"; mkdir -p "$OUT"

HASHES=$(mktemp)
trap 'rm -f "$HASHES"' EXIT

# Only the effects are copied. Everything else in Content/ loads as shipped, and duplicating it
# here would waste hundreds of megabytes and create two copies to keep in step.
echo "==> collecting effects from $SRC"
COUNT=0
# find | while would run the loop in a subshell and lose COUNT, and `for f in $(find ...)` splits
# on the spaces in "C:\Program Files (x86)\Steam\...". A file list read by redirection avoids
# both.
LIST=$(mktemp)
find "$SRC" -name '*.xnb' -print | sort > "$LIST"
while IFS= read -r f; do
  # An effect .xnb names EffectReader near the top. Match the FULL reader name: "EffectReader"
  # is a substring of "SoundEffectReader", and matching loosely would drag in all 220 sounds.
  if head -c 4096 "$f" | tr -d '\0' | grep -q 'Microsoft.Xna.Framework.Content.EffectReader'; then
    rel=${f#"$SRC"/}
    mkdir -p "$OUT/$(dirname "$rel")"
    cp -p "$f" "$OUT/$rel"
    printf '%s  %s\n' "$(sha256sum "$f" | cut -d' ' -f1)" "$rel" >> "$HASHES"
    COUNT=$((COUNT + 1))
  fi
done < "$LIST"
rm -f "$LIST"
echo "    $COUNT effect(s)"

echo "==> converting the containers to MGFX v10"
"$UW_REPO/$TOOL" transcode "$OUT" 2>&1 | tail -2
"$UW_REPO/$TOOL" validate  "$OUT" 2>&1 | grep -vE 'skinFX_0' | tail -2

# The source must be left exactly as it was - that is the entire point of this deviation - and
# the way to establish that is to hash it before and after, not to compare it with the output.
#
# An earlier version of this check asserted that each output DIFFERS from its source, on the
# reasoning that v8 -> v10 changes the container. That is wrong: it fails on a source that is
# already v10, where copying and converting correctly produces an identical file. It tested the
# wrong invariant.
echo "==> confirming $SRC was not modified"
CHANGED=0
LIST=$(mktemp)
find "$OUT" -name '*.xnb' -print | sort > "$LIST"
while IFS= read -r f; do
  rel=${f#"$OUT"/}
  before=$(grep -F "  $rel" "$HASHES" | cut -d' ' -f1)
  after=$(sha256sum "$SRC/$rel" | cut -d' ' -f1)
  if [ "$before" != "$after" ]; then
    echo "    ! MODIFIED SOURCE: $rel"
    CHANGED=$((CHANGED + 1))
  fi
done < "$LIST"
rm -f "$LIST"
[ "$CHANGED" -eq 0 ] \
  && echo "    all $COUNT source effect(s) byte-identical to before" \
  || { echo "FATAL: the conversion wrote to $SRC - $CHANGED file(s)" >&2; exit 1; }

mgfx_version() {
  off=$(grep -abo 'MGFX' "$1" | head -1 | cut -d: -f1)
  [ -n "$off" ] || { echo "?"; return; }
  od -An -tu1 -j $((off + 4)) -N1 "$1" | tr -d ' '
}
if [ -f "$SRC/multiTex.xnb" ] && [ -f "$OUT/multiTex.xnb" ]; then
  SRCV=$(mgfx_version "$SRC/multiTex.xnb")
  OUTV=$(mgfx_version "$OUT/multiTex.xnb")
  echo "    source multiTex.xnb is still MGFX v$SRCV (untouched)"
  echo "    output multiTex.xnb is MGFX v$OUTV"
  [ "$SRCV" = "8" ] || echo "    note: the source was already v$SRCV, not the shipped v8"
fi

echo
echo "==> $OUT  ($(du -sh "$OUT" | cut -f1))"
echo "Ships as port-content/ beside the executable. The game's Content/ is never written to."
