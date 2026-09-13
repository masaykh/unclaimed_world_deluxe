#!/bin/sh
# Builds the shareable modding archives in artifacts/release/:
#
#   UnclaimedWorld-Port-Source-<date>.zip     source, build tooling, decompiled baseline
#   UnclaimedWorld-Port-Win64-<date>.zip      Windows binaries + installer + shader tool

#   SHA256SUMS
#
# NEITHER archive contains game content. Content/ and data/ are deliberately excluded - they
# are the game's copyrighted assets and recipients are expected to have their own copy. The
# binaries archive installs over an existing installation.
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

STAMP=$(date +%Y%m%d)
REL=artifacts/release
ZIP="$SEVENZIP"
mkdir -p "$REL"
rm -f "$REL"/*.zip "$REL"/SHA256SUMS

# ---------------------------------------------------------------- source archive
SRCZIP="$REL/UnclaimedWorld-Port-Source-$STAMP.zip"
echo "==> source archive"
# -x! patterns keep game assets, build output and the working copy out.
"$ZIP" a -tzip -mx=7 "$SRCZIP" \
  src tools build samples patches content/effects decomp \
  Directory.Build.props Directory.Packages.props NuGet.config global.json \
  .config/dotnet-tools.json .gitignore .gitattributes \
  MODDING.md PORTING-NOTES.md ref/manifest.sha256 \
  -x'!*/bin' -x'!*/obj' -x'!*.user' >/dev/null
echo "    $SRCZIP  ($(du -h "$SRCZIP" | cut -f1))"

# ---------------------------------------------------------------- binaries archive
echo "==> Windows binaries"
"$DOTNET" publish src/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=DX -v q --nologo
"$DOTNET" build tools/MgfxTranscode/MgfxTranscode.csproj -c Release -v q --nologo
PUB=artifacts/publish/UnclaimedWorld/release_dx
TOOLBIN=$(find artifacts/bin/MgfxTranscode -name 'mgfxtranscode.exe' -path '*release*' | head -1)
[ -n "$TOOLBIN" ] || TOOLBIN=$(find artifacts/bin/MgfxTranscode -name 'mgfxtranscode.exe' | head -1)

STAGE="$REL/stage-win64"
rm -rf "$STAGE"; mkdir -p "$STAGE/tools"
cp -rp "$PUB"/. "$STAGE"/
rm -f "$STAGE"/*.pdb "$STAGE"/UnclaimedWorld.dll.config
[ -f native/steam/steam_api64.dll ] && cp -p native/steam/steam_api64.dll "$STAGE"/

# The menu animation replacing the WMV video player (PORT DEVIATION 17), built from the game's
# own video into the game root. Content/ and the original .wmv are untouched.
#
# NOTE, deliberately: this is the ONE piece of derived game artwork in the binaries archive.
# Everything else here is code. It is included because the alternative is generating it on each
# user's machine, which needs a video decoder we cannot assume, and because it replaces a video
# the recipient already owns. If that trade is ever unwanted, delete these four lines: the game
# treats a missing MainMenuIntro.uwanim as "use the still background" with no other change.
echo "==> menu animation"
sh build/33-make-menu-animation.sh game/Content/MainMenu/TauCetiMainMenu.wmv "$STAGE/MainMenuIntro.uwanim" 2>&1 \
  | grep -E 'frames,|FATAL' || true

# PORT DEVIATION 18. The 19 effects converted from MGFX v8 to v10, shipped as an override folder
# the game prefers at load time. The installer no longer rewrites the player's Content/ - it
# copies this alongside it, so the game's own assets are never modified and there is no
# conversion step that can be skipped. Same "derived asset" note as the menu animation above:
# these are container rewrites of the studio's own shaders, with the bytecode copied through
# unchanged.
echo "==> port-content override"
sh build/34-make-port-content.sh "$UW_STEAM/Content" "$STAGE/port-content" 2>&1 \
  | grep -E 'effect\(s\)|rewritten|valid MGFX|byte-identical|FATAL'

# The shader tool has to ship: a fresh install's effects are MGFX v8 and MonoGame 3.8 refuses
# them, so the installer transcodes them in place on the user's own copy.
TOOLDIR=$(dirname "$TOOLBIN")
cp -p "$TOOLDIR"/mgfxtranscode.exe "$TOOLDIR"/mgfxtranscode.dll "$STAGE/tools/"

# Ship the content probe too: it loads assets through a real ContentManager and reports
# pass/fail per asset, which is how a modder can turn "it will not start" into a precise
# answer without driving the UI.
"$DOTNET" build tools/ContentProbe/ContentProbe.csproj -c Release -v q --nologo
# ContentProbe now builds per platform (release_dx / release_gl), so pin the DX flavour rather
# than taking whichever the glob finds first - this archive is the Windows/DirectX build.
PROBEDIR=$(dirname "$(find artifacts/bin/ContentProbe -name contentprobe.exe -path "*release_dx*" | head -1)")
[ -n "$PROBEDIR" ] && cp -p "$PROBEDIR"/*.dll "$PROBEDIR"/contentprobe.exe "$PROBEDIR"/*.json "$STAGE/tools/" 2>/dev/null || true

# ...and RUN it, as the gate on shipping at all. `set -e` aborts the packaging if it fails.
#
# This is here because an archive was once shared that could not start the game: SharpDX had
# been pinned forward to 4.2.0, which broke an interface MonoGame's precompiled Callback type
# implements, and MediaPlayer's static constructor threw on launch. It compiled clean and every
# asset loaded, so nothing before this line noticed. The probe reads MediaPlayer.State first
# thing, which is the same call the game's AudioManager makes, so that class of failure now
# stops the build instead of reaching a modder.
# Probed against the PRISTINE Steam content with the freshly built override, which is the
# combination that actually ships. game/Content was used here until PORT DEVIATION 18 and was
# quietly the wrong target afterwards: earlier build steps transcode its effects in place, so it
# would have passed whether or not the override worked - and would have passed on a package
# missing port-content\ entirely. Steam is only read; verify-steam-untouched.sh checks that.
echo "==> gate: content + media probe (pristine content through the override)"
"$PROBEDIR/contentprobe.exe" "$UW_STEAM/Content" --override "$STAGE/port-content"

# Build DataExport as a gate too. It drives the game's own data loader headlessly, so it is the
# only thing that compiles against DataLoader's internals - and that is exactly why it rots
# silently: game 1.0.4.8 added a parameter to QueueInitGameData and this tool stayed broken
# through the whole rebase because nothing built it. Compiling it here catches that class of
# drift; running it needs a content directory, so that stays a manual step (see MODDING.md).
echo "==> gate: DataExport compiles against the current DataLoader"
"$DOTNET" build tools/DataExport/DataExport.csproj -c Release -v q --nologo

# The mod loader is a promise to third parties, so it is verified rather than assumed: this
# builds the example mod against the published DLLs, loads it, and asserts the patch changed the
# game's data - plus the three failure cases. It also catches a Harmony version bump that breaks
# patching on .NET 8, which nothing else would notice until a modder's DLL stopped working.
echo "==> gate: Harmony mod loader"
sh build/80-verify-modloader.sh | tail -14

# PORT DEVIATION 17 is a removal, and removals rot back in. The video types are still present in
# MonoGame.Framework, so one `using Microsoft.Xna.Framework.Media;` and a `new VideoPlayer()`
# would restore the dependency - and since that constructor throws on DesktopGL, it would surface
# as a crash on the platform tested least. Verified to fail when a VideoPlayer reference is
# reintroduced, not merely to pass today.
echo "==> gate: no VideoPlayer dependency"
sh build/81-verify-no-videoplayer.sh | tail -3
cp -p "$TOOLDIR"/*.json "$STAGE/tools/" 2>/dev/null || true

# The game version this archive is built against, taken from the pristine binary rather than
# hardcoded so it cannot drift. install.ps1 refuses to install over a different version: the
# port is compiled from one version's code and a mismatch surfaces as an unexplained cast
# exception at content load, which is exactly how one modder lost an afternoon.
GAMEVER=$(powershell -NoProfile -Command \
  "(Get-Item '$(cygpath -w "$UW_REPO/ref/original/UnclaimedWorld.exe")').VersionInfo.FileVersion" \
  2>/dev/null | tr -d '\r')
{
  echo "# Game version this port archive was built from. install.ps1 compares the target"
  echo "# installation against it and refuses a mismatch (override: -SkipVersionCheck)."
  echo "$GAMEVER"
} > "$STAGE/game-version.txt"
echo "==> built for game version $GAMEVER"

cp -p build/install.ps1 "$STAGE/install.ps1"
# setup.ps1 is the DEFAULT route: it copies Content and data out of the player's game into
# the archive folder and never writes to the game at all. install.ps1 remains for people who
# want the port to be the game Steam launches, which does mean replacing six assemblies.
cp -p build/setup.ps1 "$STAGE/setup.ps1"
# setup.cmd does the same in plain cmd.exe, for machines where PowerShell is disabled by policy
# or blocked by security software - which is common enough that a PowerShell-only installer means
# some people simply cannot install at all.
cp -p build/setup.cmd "$STAGE/setup.cmd"
cp -p build/INSTALL.md  "$STAGE/INSTALL.md"

# Explicit payload manifest. install.ps1 installs exactly these entries and nothing else -
# which is what makes it safe to run from inside the game folder, where "every file next to
# the script" would wrongly include the game's own files and copy them onto themselves.
{
  echo "# Files install.ps1 copies into the game folder. One relative path per line."
  echo "# Everything else in this archive (install.ps1, INSTALL.md, tools/) stays put."
  ( cd "$STAGE" && ls -1 ) | grep -vE "^(install.ps1|setup.ps1|setup.cmd|INSTALL.md|payload.txt|game-version.txt|tools)$"
} > "$STAGE/payload.txt"

BINZIP="$REL/UnclaimedWorld-Port-Win64-$STAMP.zip"
( cd "$STAGE" && "$ZIP" a -tzip -mx=7 "../$(basename "$BINZIP")" . >/dev/null )
rm -rf "$STAGE"
echo "    $BINZIP  ($(du -h "$BINZIP" | cut -f1))"

# ---------------------------------------------------------------- checksums
( cd "$REL" && sha256sum ./*.zip > SHA256SUMS )
echo
echo "==> artifacts/release:"
ls -la "$REL" | grep -vE '^total|^d'
echo
echo "Neither archive contains Content/ or data/ - verifying:"
echo "(the binaries archive does carry ONE derived asset at its root, MainMenuIntro.uwanim,"
echo " re-encoded from the game's own menu video - see PORT DEVIATION 17)"
for z in "$SRCZIP" "$BINZIP"; do
  # Only TOP-LEVEL Content\ and data\ count - those are the game's asset directories.
  # Anchored and case-sensitive on purpose: a loose match also hits the port's own
  # content/effects/*.fx shader sources and the 300-odd decompiled
  # decomp/MonoGame.Framework/Microsoft/Xna/Framework/Content/*.cs readers, neither of which
  # is game content. -slt prints one "Path = ..." line per entry, so there are no columns to
  # split wrongly.
  n=$("$ZIP" l -ba -slt "$z" | sed -n 's/^Path = //p' | grep -cE '^(Content|data)\\' || true)
  printf '    %-52s %s game-asset path(s)\n' "$(basename "$z")" "$n"
done
