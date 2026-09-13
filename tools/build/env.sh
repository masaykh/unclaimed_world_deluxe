#!/bin/sh
# Shell environment for this repository. Sourced by every script here:
#   . "$(dirname "$0")/env.sh"
#
# Everything is derived or overridable. A path baked in for one machine is how a repository stops
# building on anyone else's - so the root is COMPUTED from this file's own location, and the two
# things that genuinely vary (where Steam put the game, where dotnet is) are looked up, with an
# environment variable always taking precedence over the guess.

# --- the repository, from where this file actually is -----------------------------------------
UW_REPO=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)

# --- your copy of the game --------------------------------------------------------------------
# Needed for CONTENT only: the art, audio, models and compiled assets that are not part of what
# Refactored Games released and are not in this repository. See license.md.
#
#   export UW_STEAM="/path/to/Unclaimed World"
if [ -z "$UW_STEAM" ]; then
  for candidate in \
    "/c/Program Files (x86)/Steam/steamapps/common/Unclaimed World" \
    "/c/Program Files/Steam/steamapps/common/Unclaimed World" \
    "$HOME/.steam/steam/steamapps/common/Unclaimed World" \
    "$HOME/.local/share/Steam/steamapps/common/Unclaimed World" \
    "$HOME/Library/Application Support/Steam/steamapps/common/Unclaimed World"
  do
    [ -d "$candidate" ] && { UW_STEAM="$candidate"; break; }
  done
fi

# Where the port is run and tested from. NEVER $UW_STEAM: that one is the golden reference and is
# never written to.
UW_GAME="${UW_GAME:-$UW_REPO/game}"

# --- toolchain ---------------------------------------------------------------------------------
if [ -z "$DOTNET" ]; then
  if command -v dotnet >/dev/null 2>&1; then
    DOTNET=$(command -v dotnet)
  elif [ -x "/c/Program Files/dotnet/dotnet.exe" ]; then
    DOTNET="/c/Program Files/dotnet/dotnet.exe"
    PATH="/c/Program Files/dotnet:$PATH"
  else
    DOTNET=dotnet   # fail loudly at the point of use rather than silently here
  fi
fi

if [ -z "$SEVENZIP" ] && [ -x "/c/Program Files/7-Zip/7z.exe" ]; then
  SEVENZIP="/c/Program Files/7-Zip/7z.exe"
  PATH="/c/Program Files/7-Zip:$PATH"
fi

export PATH UW_REPO UW_STEAM UW_GAME DOTNET SEVENZIP
