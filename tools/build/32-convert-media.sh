#!/bin/sh
# Produces the DesktopGL audio assets in content/media-gl/.
#
# The game's 9 music tracks are WMA and its menu background is WMV, which MonoGame plays
# through MediaFoundation - a WindowsDX-only path. On DesktopGL:
#
#   * Song works, via Song.NVorbis -> OggStream -> OpenAL. So the music only needs
#     transcoding to Ogg Vorbis.
#   * Video does NOT exist. DesktopGL falls back to VideoPlayer.Default, whose
#     PlatformInitialize throws NotImplementedException - i.e. `new VideoPlayer()` itself
#     throws. No transcoding can fix that, so the WMV is not converted at all; the game
#     shows the still menu background instead (see PORT DEVIATION 7, and
#     PlatformMedia.IsVideoSupported which skips it without logging an error).
#
# The Song XNB is only a stub: a length-prefixed string naming the media file next to it,
# then the duration. Because ".wma" and ".ogg" are the same length, the stub can be patched
# byte-for-byte in place - no re-serialization, and the XNB's total-size header field stays
# valid. That is the whole trick that avoids needing an XNB writer here.
#
# Output goes to content/media-gl/ rather than over game/, so the WindowsDX build keeps using
# the original WMA files untouched. build/60-package-gl.sh overlays this onto the GL package.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

FFMPEG=$(command -v ffmpeg 2>/dev/null || true)
[ -n "$FFMPEG" ] || FFMPEG=$(find "/c/Users/$USERNAME/AppData/Local/Microsoft/WinGet/Packages" -name 'ffmpeg.exe' 2>/dev/null | head -1)
[ -n "$FFMPEG" ] || FFMPEG=$(find /c/Users/*/AppData/Local/Microsoft/WinGet/Packages -name 'ffmpeg.exe' 2>/dev/null | head -1)
[ -n "$FFMPEG" ] || { echo "FATAL: ffmpeg not found. Install with: winget install Gyan.FFmpeg" >&2; exit 1; }
echo "==> ffmpeg: $FFMPEG"

SRC="$UW_GAME/Content"
OUT=artifacts/content/media-gl
mkdir -p "$OUT/Music"

converted=0
for wma in "$SRC"/Music/*.wma; do
  [ -f "$wma" ] || continue
  base=$(basename "$wma" .wma)
  ogg="$OUT/Music/$base.ogg"
  if [ -f "$ogg" ] && [ "$ogg" -nt "$wma" ]; then
    echo "  == $base.ogg (up to date)"
  else
    # -q:a 6 is roughly 192 kbps VBR - the sources are 320 kbps WMA, and this is the usual
    # transparency point for Vorbis. Bump it if anyone complains about the music.
    "$FFMPEG" -nostdin -loglevel error -y -i "$wma" -vn -c:a libvorbis -q:a 6 "$ogg"
    echo "  -> $base.ogg  ($(du -h "$wma" | cut -f1) wma -> $(du -h "$ogg" | cut -f1) ogg)"
  fi

  # Patch the Song stub: same-length extension swap, so this is a pure byte substitution.
  stub="$SRC/Music/$base.xnb"
  if [ -f "$stub" ]; then
    perl -e 'binmode STDIN; binmode STDOUT; local $/; my $d = <STDIN>;
             my $n = ($d =~ s/\.wma/\.ogg/g);
             die "no .wma reference found in stub\n" unless $n;
             print $d;' < "$stub" > "$OUT/Music/$base.xnb"
  else
    echo "  !! $base: no XNB stub next to it" >&2
  fi
  converted=$((converted + 1))
done

echo
echo "$converted track(s) ready in $OUT/Music."
echo "Video (MainMenu/TauCetiMainMenu.wmv) is deliberately NOT converted: MonoGame has"
echo "no DesktopGL VideoPlayer at all, so the GL build shows the still menu background."
