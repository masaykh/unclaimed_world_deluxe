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

# WHERE FFMPEG IS LOOKED FOR, in order: $FFMPEG if you set it; artifacts/tools/ffmpeg/ inside this
# checkout; PATH; a WinGet install. artifacts/ is git-ignored, so an ffmpeg unpacked there once is
# kept across every pull and every build - Kastuk had to fetch and extract it again for each build,
# "extraction is longer than downloading".
LOCAL_FFMPEG_DIR="$UW_REPO/artifacts/tools/ffmpeg"
FFMPEG=${FFMPEG:-}
[ -n "$FFMPEG" ] && [ ! -x "$FFMPEG" ] && [ ! -f "$FFMPEG" ] && FFMPEG=""
[ -n "$FFMPEG" ] || FFMPEG=$(find "$LOCAL_FFMPEG_DIR" \( -name 'ffmpeg.exe' -o -name 'ffmpeg' \) -type f 2>/dev/null | head -1)
[ -n "$FFMPEG" ] || FFMPEG=$(command -v ffmpeg 2>/dev/null || true)
[ -n "$FFMPEG" ] || FFMPEG=$(find "/c/Users/$USERNAME/AppData/Local/Microsoft/WinGet/Packages" -name 'ffmpeg.exe' 2>/dev/null | head -1)
[ -n "$FFMPEG" ] || FFMPEG=$(find /c/Users/*/AppData/Local/Microsoft/WinGet/Packages -name 'ffmpeg.exe' 2>/dev/null | head -1)
[ -n "$FFMPEG" ] || {
  echo "FATAL: ffmpeg not found." >&2
  echo "  Put it where every later build finds it - inside this checkout, untouched by git pull:" >&2
  echo "      $LOCAL_FFMPEG_DIR/   (the whole unpacked folder, or just ffmpeg.exe)" >&2
  echo "  or install it (winget install Gyan.FFmpeg), or export FFMPEG=/path/to/ffmpeg.exe" >&2
  exit 1; }
echo "==> ffmpeg: $FFMPEG"

# The pristine install first (UW_STEAM), as 60-package-gl.sh already requires; the deployed working
# copy only if there is no such thing. Kastuk built a package with no game/ folder at all, and
# this read nothing.
if [ -n "$UW_STEAM" ] && [ -d "$UW_STEAM/Content/Music" ]; then
  SRC="$UW_STEAM/Content"
else
  SRC="$UW_GAME/Content"
fi
ls "$SRC"/Music/*.wma >/dev/null 2>&1 || {
  echo "FATAL: no music to convert in $SRC/Music." >&2
  echo "  Export UW_STEAM as your copy of the game, which this only reads:" >&2
  echo "      export UW_STEAM=\"/c/Program Files (x86)/Steam/steamapps/common/Unclaimed World\"" >&2
  exit 1; }
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
