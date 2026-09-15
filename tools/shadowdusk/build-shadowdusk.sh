#!/bin/sh
# Fetches and builds the patched ShadowDusk used to compile this game's effects.
#
# Writes a ready-to-run CLI to artifacts/tools/shadowdusk/ and prints its path. Idempotent:
# re-running with the tree already at the pinned commit rebuilds only what changed.
#
# TWO SOURCES, because they are two different things:
#
#   the COMPILER   - our fork, pinned to a commit. That is where the patches live as reviewable
#                    history; tools/shadowdusk/patches/ carries the same delta as files, for
#                    submitting upstream and for reading without cloning anything.
#
#   the NATIVES    - dxcompiler / dxil / spirv-cross / libvkd3d-shader, from the published
#                    ShadowDusk.Cli NuGet tool. They are NOT in the source repo, they are 155 MB
#                    across all RIDs, and they are byte-identical to what upstream ships - so
#                    they are fetched, never vendored. NuGet is cacheable in CI; git is not a
#                    good home for a 27 MB .so that never delta-compresses.
#
# Cross-platform on purpose: this is the whole reason ShadowDusk is here instead of mgfxc.
# mgfxc P/Invokes d3dcompiler_47.dll even for the OpenGL profile, so it needs Windows or Wine.
# ShadowDusk's OpenGL path is HLSL -> DXC -> SPIR-V -> SPIRV-Cross -> GLSL, and those natives
# ship for win-x64, linux-x64, osx-x64 and osx-arm64.
set -e

# Pinned so a build is reproducible and an upstream force-push cannot silently change what we
# compile with. Bump deliberately, and re-run build/34 + the render comparison when you do.
SD_REPO=${UW_SHADOWDUSK_REPO:-https://github.com/masaykh/ShadowDusk.git}
SD_REF=${UW_SHADOWDUSK_REF:-5e09e49dd12e4ea35d1cb1377c7c88dfac56c91e}
SD_TOOL_VERSION=${UW_SHADOWDUSK_TOOL_VERSION:-0.20.0}

REPO_ROOT=$(cd "$(dirname "$0")/../.." && pwd)
WORK="$REPO_ROOT/artifacts/tools/shadowdusk"
SRC="$WORK/src"
OUT="$WORK/bin"
NATIVES="$WORK/.natives"

DOTNET=${DOTNET:-dotnet}
command -v "$DOTNET" >/dev/null 2>&1 || DOTNET="/c/Program Files/dotnet/dotnet.exe"

mkdir -p "$WORK"

# ---- 1. source, at the pinned commit -------------------------------------------------------
if [ -d "$SRC/.git" ]; then
  ( cd "$SRC" && git fetch -q origin "$SD_REF" 2>/dev/null || git fetch -q origin )
else
  rm -rf "$SRC"
  git clone -q "$SD_REPO" "$SRC"
fi
( cd "$SRC" && git checkout -q "$SD_REF" )
echo "==> ShadowDusk source at $SD_REF"

# ---- 2. build the managed CLI --------------------------------------------------------------
# ShadowDusk's libraries multi-target net8.0;net10.0 (the CLI itself is single-TFM). With BOTH
# SDKs present, build it the way upstream intends. With only .NET 8, restrict the target
# frameworks - otherwise restore evaluates net10.0 and dies NETSDK1045, "The current .NET SDK
# does not support targeting .NET 10.0". `-f` alone is not enough: it picks the output TFM but
# restore still walks every TargetFrameworks entry.
#
# This repository's global.json pins the SDK to 8.x with rollForward:latestFeature, and the
# clone lands UNDER this repository, so it inherits that pin. Installing a .NET 10 SDK does not
# help - global.json decides which one runs, and it will never pick 10. So net8.0 is not a
# fallback here, it is the only path, and the override is always needed.
#
# Restricting the target frameworks has a consequence worth stating, because it cost two red CI
# runs. ShadowDusk sets
#
#     <RestoreLockedMode Condition="'$(CI)' == 'true'">true</RestoreLockedMode>
#
# and every CI provider sets CI=true - so on a runner, restore demands that the project's target
# frameworks match the lock file's exactly. A single-TFM override does not match, and restore
# fails NU1004 with a message that reads like the lock file is corrupt when nothing is wrong with
# it. Reproduce locally with `CI=true dotnet build ...`; without CI set the check is simply off,
# which is why this passed here and failed there.
#
# Turning locked mode off costs less than it looks. The lock file pins ShadowDusk's DEPENDENCY
# versions; we already pin ShadowDusk itself to a commit SHA above, which is the stronger
# guarantee and the one that decides what our shaders compile to.
echo "==> building ShadowDuskCLI (net8.0)"
"$DOTNET" build "$SRC/src/ShadowDusk.Cli/ShadowDusk.Cli.csproj" \
  -c Release -f net8.0 -p:TargetFrameworks=net8.0 -p:RestoreLockedMode=false -v q --nologo

BUILT="$SRC/src/ShadowDusk.Cli/bin/Release/net8.0"
[ -f "$BUILT/ShadowDuskCLI.dll" ] || { echo "FATAL: build produced no ShadowDuskCLI.dll" >&2; exit 1; }

# ---- 3. natives, from the published tool ----------------------------------------------------
# The source build does not fetch them; without these the CLI throws at first compile.
if [ ! -d "$NATIVES" ]; then
  echo "==> fetching native compilers (ShadowDusk.Cli $SD_TOOL_VERSION)"
  "$DOTNET" tool install ShadowDusk.Cli --version "$SD_TOOL_VERSION" --tool-path "$NATIVES" >/dev/null
fi
TOOLDIR=$(find "$NATIVES/.store" -type d -path '*/tools/net8.0/any' 2>/dev/null | head -1)
[ -n "$TOOLDIR" ] || { echo "FATAL: could not locate the tool payload under $NATIVES/.store" >&2; exit 1; }

# ---- 4. assemble: upstream's natives, OUR managed assemblies on top -------------------------
rm -rf "$OUT"; mkdir -p "$OUT"
cp -r "$TOOLDIR"/. "$OUT"/
cp -f "$BUILT"/ShadowDusk.*.dll "$BUILT"/ShadowDuskCLI.* "$OUT"/

CLI="$OUT/ShadowDuskCLI.exe"
[ -f "$CLI" ] || CLI="$OUT/ShadowDuskCLI.dll"

echo
echo "==> ready: $CLI"
echo "    build/34-build-gl-effects-shadowdusk.sh reads UW_SHADOWDUSK; export it as:"
echo "      export UW_SHADOWDUSK=\"$CLI\""
