#!/bin/sh
# Rebuilds the game's TrueType-based fonts from assets/, with more of Unicode than the studio's
# ASCII, into artifacts/content/fonts/{dx,gl}/.
#
#   sh tools/build/36-build-fonts.sh [OUT]
#
# WHAT IT BUILDS. The five fonts the game loads that have a TrueType source - the rest are
# hand-drawn bitmaps (CRTGlow, CRT_18pt, newtown_8pt) and cannot gain letters by rebuilding:
#
#   Arial                      Arial 9           InGameInterface, the dev overlay
#   Fonts/DefaultHeading       Arial Bold 10     window titles (Window.cs)
#   Fonts/LCD_bold             Arial Bold 10     GUIManager.LCDInterfaceBoldFont
#   Fonts/LCDandHUDBody        Electrolize 10    the main UI font
#   Fonts/LCDandHUDSubHeading  Electrolize 12
#
# Size, spacing and kerning come from the studio's .spritefont unchanged. What changes:
#
#   - CharacterRegions: UW_FONT_RANGES (default Latin, Latin-1 + Latin Extended-A, Greek,
#     Cyrillic), CUT DOWN TO WHAT THE TYPEFACE ACTUALLY HAS (tools/FontCoverage reads its cmap).
#     mgcb draws a character the face lacks as an empty box, and asked for Cyrillic, Electrolize -
#     which has none - produced 256 of them. Left out, a character falls to the fallback below.
#   - DefaultCharacter: UW_FONT_FALLBACK, default '?', in every font. The shipped Arial,
#     DefaultHeading and LCD_bold have none, which makes MonoGame THROW on the first character
#     outside the font. (The running game also sets one on load - UwContentManager - so this is
#     belt and braces for anything that reads these files without it.)
#   - The face is a FILE, not a name: UW_FONT_FACE_ARIAL, UW_FONT_FACE_ARIAL_BOLD and
#     UW_FONT_FACE_LCD. By name, mgcb resolved "Arial" + Style Bold to regular arial.ttf; by file,
#     the bold face is the bold face (and Style is set to Regular so it is not emboldened twice).
#     The Arial defaults are Windows' own files; elsewhere, point these at a font you may use.
#
# Swapping a typeface is one variable - e.g. a replacement for Electrolize that has Cyrillic:
#
#   UW_FONT_FACE_LCD=/path/to/Play-Regular.ttf sh tools/build/36-build-fonts.sh
#
# BOTH platforms are built because an XNB records its target: dx/ for WindowsDX, gl/ for
# DesktopGL. Each is laid out as an OVERRIDE folder - copy it into the game's port-content\ and
# UwContentManager prefers it over Content\, so nothing the player owns is modified.
#
# NOT SHIPPED BY THE RELEASE. Rebuilt Latin glyphs are not pixel-identical to the studio's
# (about 1px of advance here and there), so a release should carry these only once someone has
# looked at the screens. This script produces them; 70-make-release.sh does not pick them up.
#
# The content builder is dotnet-mgcb at the same version as the MonoGame runtime pin
# (Directory.Packages.props), installed locally into artifacts/tools/mgcb on first use.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

OUT=${1:-artifacts/content/fonts}
RANGES=${UW_FONT_RANGES:-"32-126 160-383 880-1023 1024-1279"}
FALLBACK=${UW_FONT_FALLBACK:-?}
WORK="${TMPDIR:-/tmp}/uw-build-fonts"

WINFONTS="/c/Windows/Fonts"
if command -v cygpath >/dev/null 2>&1 && [ -n "$WINDIR" ]; then
  WINFONTS="$(cygpath -u "$WINDIR")/Fonts"
fi
FACE_ARIAL=${UW_FONT_FACE_ARIAL:-$WINFONTS/arial.ttf}
FACE_ARIAL_BOLD=${UW_FONT_FACE_ARIAL_BOLD:-$WINFONTS/arialbd.ttf}
FACE_LCD=${UW_FONT_FACE_LCD:-assets/WindowSystem-Content/Fonts/Electrolize.ttf}

for face in "$FACE_ARIAL" "$FACE_ARIAL_BOLD" "$FACE_LCD"; do
  [ -f "$face" ] || {
    echo "FATAL: no font file at $face" >&2
    echo "       Set UW_FONT_FACE_ARIAL / UW_FONT_FACE_ARIAL_BOLD / UW_FONT_FACE_LCD to .ttf files." >&2
    exit 2
  }
done

say() { printf '%s\n' "$*"; }

# ---- tools -------------------------------------------------------------------------------
MG_VERSION=$(sed -n 's:.*<MonoGameVersion>\([^<]*\)</MonoGameVersion>.*:\1:p' Directory.Packages.props | head -1)
[ -n "$MG_VERSION" ] || { echo "FATAL: MonoGameVersion not found in Directory.Packages.props" >&2; exit 1; }
MGCB_DIR=artifacts/tools/mgcb-$MG_VERSION
if [ ! -d "$MGCB_DIR" ]; then
  say "==> installing dotnet-mgcb $MG_VERSION into $MGCB_DIR"
  "$DOTNET" tool install dotnet-mgcb --version "$MG_VERSION" --tool-path "$MGCB_DIR" >/dev/null
fi
MGCB=$(find "$UW_REPO/$MGCB_DIR" -maxdepth 1 -name 'mgcb' -o -maxdepth 1 -name 'mgcb.exe' | head -1)
[ -n "$MGCB" ] || { echo "FATAL: mgcb not found in $MGCB_DIR" >&2; exit 1; }

"$DOTNET" build tools/FontCoverage/FontCoverage.csproj -c Release -v q --nologo >/dev/null
COVERAGE="$UW_REPO/$(find artifacts/bin/FontCoverage -name 'fontcoverage*' -path '*release*' ! -name '*.pdb' ! -name '*.json' ! -name '*.dll' | head -1)"
[ -f "$COVERAGE" ] || { echo "FATAL: fontcoverage not built" >&2; exit 1; }

# ---- sources ------------------------------------------------------------------------------
rm -rf "$WORK"; mkdir -p "$WORK/src/Fonts"

# One line per font: the studio's .spritefont, the asset name the game loads, the face file.
FONTS="assets/Content/Arial.spritefont|Arial|$FACE_ARIAL
assets/WindowSystem-Content/Fonts/DefaultHeading.spritefont|Fonts/DefaultHeading|$FACE_ARIAL_BOLD
assets/WindowSystem-Content/Fonts/LCD_bold.spritefont|Fonts/LCD_bold|$FACE_ARIAL_BOLD
assets/WindowSystem-Content/Fonts/LCDandHUDBody.spritefont|Fonts/LCDandHUDBody|$FACE_LCD
assets/WindowSystem-Content/Fonts/LCDandHUDSubHeading.spritefont|Fonts/LCDandHUDSubHeading|$FACE_LCD"

BUILT=""
echo "$FONTS" | while IFS='|' read -r source asset face; do
  dir=$(dirname "$WORK/src/$asset")
  mkdir -p "$dir"
  # The face goes beside the .spritefont: FontDescriptionProcessor looks for FontName in the
  # source file's directory before the system font folders.
  faceName=$(basename "$face")
  cp -p "$face" "$dir/$faceName"

  # ASCII always, whatever UW_FONT_RANGES says - the game's own text is ASCII.
  regions=$("$COVERAGE" "$face" 32-126 $RANGES)
  [ -n "$regions" ] || { echo "FATAL: $face covers none of $RANGES" >&2; exit 1; }
  xml=""
  for r in $regions; do
    xml="$xml      <CharacterRegion><Start>&#${r%-*};</Start><End>&#${r#*-};</End></CharacterRegion>\n"
  done

  # Rewrite the studio's file: comments dropped (they contain sample <DefaultCharacter> lines),
  # face, style, fallback and regions replaced, everything else kept.
  tr -d '\r' < "$source" | sed 's/^\xEF\xBB\xBF//' | awk -v face="$faceName" -v fallback="$FALLBACK" -v regions="$xml" '
    /<!--/ { incomment = 1 }
    incomment { if (/-->/) incomment = 0; next }
    /<FontName>/ { sub(/<FontName>[^<]*<\/FontName>/, "<FontName>" face "</FontName>") }
    /<Style>/ { sub(/<Style>[^<]*<\/Style>/, "<Style>Regular</Style>") }
    /<DefaultCharacter>/ { next }
    /<CharacterRegions>/ {
      print "    <DefaultCharacter>" fallback "</DefaultCharacter>"
      print "    <CharacterRegions>"
      printf "%s", regions
      print "    </CharacterRegions>"
      skipping = 1; next
    }
    skipping { if (/<\/CharacterRegions>/) skipping = 0; next }
    { print }
  ' > "$WORK/src/$asset.spritefont"
  say "    $asset  <- $faceName  [$regions]"
done

# ---- build --------------------------------------------------------------------------------
for platform in Windows:dx DesktopGL:gl; do
  name=${platform%%:*}; sub=${platform#*:}
  say "==> $name -> $OUT/$sub"
  set --
  for f in $(cd "$WORK/src" && find . -name '*.spritefont' | sed 's:^\./::'); do
    set -- "$@" "/build:$f"
  done
  outDir="$UW_REPO/$OUT/$sub"; objDir="$WORK/obj-$sub"
  mkdir -p "$outDir" "$objDir"
  # mgcb is a Windows program under Git Bash / msys2, and msys rewrites any argument that looks
  # like "/option:/c/some/path" as a path LIST before it gets there - which mgcb reads as extra
  # arguments ("Too many arguments"). Windows-form paths, and the rewriting switched off for this
  # one call. Both are no-ops on Linux.
  if command -v cygpath >/dev/null 2>&1; then
    outDir=$(cygpath -m "$outDir"); objDir=$(cygpath -m "$objDir")
  fi
  ( cd "$WORK/src" && MSYS2_ARG_CONV_EXCL='*' "$MGCB" /platform:$name /profile:HiDef \
      "/outputDir:$outDir" "/intermediateDir:$objDir" "$@" ) || {
    echo "FATAL: mgcb failed for $name" >&2; exit 1; }
done

say ""
say "fonts built - copy $OUT/dx (WindowsDX) or $OUT/gl (DesktopGL) into the game's port-content\\"
say "to try them; tools/ContentProbe <Content> checks each one loads and survives foreign text."
