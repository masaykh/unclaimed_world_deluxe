#!/bin/sh
# Regenerates ref/monogame-upstream-3.6/ : upstream MonoGame 3.6.0.1625 decompiled with the SAME
# ILSpy build used for decomp/, so a textual diff against the shipped (studio-forked) 3.6 build
# is meaningful. See PORTING-NOTES.md "The studio forked MonoGame itself".
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"
WORK=$(mktemp -d)
V=3.6.0.1625
echo "==> fetching MonoGame.Framework.WindowsDX $V"
curl -sfL -o "$WORK/mg.nupkg" \
  "https://api.nuget.org/v3-flatcontainer/monogame.framework.windowsdx/$V/monogame.framework.windowsdx.$V.nupkg"
"$SEVENZIP" x -y -o"$WORK/pkg" "$WORK/mg.nupkg" >/dev/null
DLL=$(find "$WORK/pkg" -name 'MonoGame.Framework.dll' | head -1)
echo "==> decompiling $DLL"
rm -rf ref/monogame-upstream-3.6
mkdir -p ref/monogame-upstream-3.6
"$DOTNET" tool run ilspycmd -- -p -o ref/monogame-upstream-3.6 "$DLL" \
  --nested-directories --disable-updatecheck 2>&1 | tail -2
echo "    $(find ref/monogame-upstream-3.6 -name '*.cs' | wc -l) cs files"
rm -rf "$WORK"
