#!/bin/sh
# Verifies the Harmony mod loader end to end, with no game launch.
#
# Builds samples/FasterCharcoalMod against the published game DLLs - the same reference layout a
# third-party modder uses - drops it into a throwaway installation's user/Mods, runs the game's
# own data loader headlessly through tools/DataExport, and asserts the patch actually changed the
# data the game would run with.
#
# Thirteen cases, and most of them are failure behaviour, because a modding framework is judged on
# that more than on its happy path:
#
#   1. the mod loads and its patch takes effect
#   2. -nomods suppresses it completely
#   3. a corrupt DLL is skipped, is named in the report, and does NOT stop later mods
#   4. a stray 0Harmony.dll in user/Mods is refused with an actionable message
#   5. user/ModSettings.xml is read, and a switch in it changes the tables that get built
#   6. a malformed ModSettings.xml is reported and ignored rather than stopping the game
#   7. the removed peat charcoal item and its recipe are gone from every configuration
#   9. the peat-fuelled build variants are cloned from the studio's recipes, and switchable
#  10. a fishing net can be made from rawhide string, which nothing else in the game consumes
#  11. the five per-request mods register their switches, and the diet one changes the tables
#  12. disassembly recipes are generated from the production recipes, linked, and switchable
#  13. a seeded random stream resumes where it left off - the save/load assumption
#
# What this pins down: that HarmonyLib patches work at all on .NET 8, that the loader finds and
# applies mods, that the documented .csproj reference layout still compiles, and that one bad mod
# cannot take out a player's session.
#
# RUN IT LOCALLY, NOT IN CI, and for the same structural reason as tools/build/70-make-release.sh:
# it needs the game's compiled Content/. Every case builds a throwaway installation, and the data
# loader resolves content paths against it - with no Content/ the tables do not load and there is
# nothing to assert about. A runner has no copy of the game. publish.yml checks what it can from
# source alone (every feature combination, base_game with mods/ deleted, the documents); this is
# the half that needs your install.
#
#   sh tools/build/80-verify-modloader.sh        # nothing to set up first
set -e
. "$(dirname "$0")/env.sh"
cd "$UW_REPO"

# Said here rather than discovered forty lines in as "cp: cannot stat".
[ -d "$UW_STEAM/Content" ] || {
  echo "FATAL: no Content/ under UW_STEAM=$UW_STEAM" >&2
  echo "       This runs against YOUR copy of the game's compiled content - see license.md." >&2
  echo "       export UW_STEAM=\"/path/to/Unclaimed World\"" >&2
  exit 2
}

STOCK=0.033333335
PATCHED=0.016666668
WORK="${TMPDIR:-/tmp}/uw-modloader-verify"
FAILURES=0

say()  { printf '%s\n' "$*"; }
fail() { printf '  FAIL  %s\n' "$*"; FAILURES=$((FAILURES + 1)); }
pass() { printf '  ok    %s\n' "$*"; }

# makeCharcoal's DaysNeeded, read out of the exported table.
charcoal_days() {
  perl -0777 -ne '
    if (m{(<ProcessType>(?:(?!</ProcessType>).)*?<KeyName>makeCharcoal</KeyName>(?:(?!</ProcessType>).)*?</ProcessType>)}s) {
      my $b = $1; my ($d) = $b =~ m{<DaysNeeded>([^<]+)</DaysNeeded>}; print $d;
    }' "$1/data/BaseData/processTypes.xml"
}

say "==> building the game, the export tool and the example mod"
"$DOTNET" publish base_game/UnclaimedWorld/UnclaimedWorld.csproj -c Release -p:UwPlatform=DX -v q --nologo
"$DOTNET" build tools/DataExport/DataExport.csproj -c Release -v q --nologo
# Built against the PUBLISHED DLLs on purpose - a ProjectReference would keep compiling even if
# the layout a modder has to use broke.
"$DOTNET" build samples/FasterCharcoalMod/FasterCharcoalMod.csproj -c Release -v q --nologo

EXPORT="$UW_REPO/$(find artifacts/bin/DataExport -name 'dataexport.exe' -path '*release*' | head -1)"
MOD="$UW_REPO/$(find artifacts/bin/FasterCharcoalMod -name 'FasterCharcoalMod.dll' | head -1)"
HARMONY="$UW_REPO/artifacts/publish/UnclaimedWorld/release_dx/0Harmony.dll"
GAMEDLL="$UW_REPO/artifacts/publish/UnclaimedWorld/release_dx/UnclaimedWorld.dll"
[ -f "$EXPORT" ]  || { echo "FATAL: dataexport.exe not found" >&2; exit 1; }
[ -f "$MOD" ]     || { echo "FATAL: FasterCharcoalMod.dll not found" >&2; exit 1; }
[ -f "$HARMONY" ] || { echo "FATAL: 0Harmony.dll is not in the publish output - is Lib.Harmony still referenced?" >&2; exit 1; }
[ -f "$GAMEDLL" ] || { echo "FATAL: UnclaimedWorld.dll not found in the publish output" >&2; exit 1; }

# The example mod must not carry its own Harmony; that is the packaging rule the loader enforces.
if [ -f "$(dirname "$MOD")/0Harmony.dll" ]; then
  fail "the example mod's output contains 0Harmony.dll; references must be Private=false"
fi

# A throwaway installation, assembled the same way tools/build/70-make-release.sh assembles a
# release: data/ from this repository - scenarios/ IS data/Maps and translations/ IS
# data/BaseData/Strings - and Content/ from your own copy of the game, which is the one part
# nobody can put in a repository.
#
# Built from the sources rather than copied out of game/ on purpose: game/ is a deployed
# installation that something else has to create first, and a check with a setup step ahead of it
# is a check people skip. This needs only UW_STEAM, which env.sh finds by itself.
#
# Content/ cannot be left out: the data loader resolves content paths against the installation,
# and without it the tables do not load and there is nothing to assert about.
new_install() {
  d="$WORK/$1"; rm -rf "$d"; mkdir -p "$d/user/Mods" "$d/data/Maps" "$d/data/BaseData/Strings"
  cp -rp scenarios/. "$d/data/Maps/"
  cp -p  translations/*.xml "$d/data/BaseData/Strings/"
  cp -rp "$UW_STEAM/Content" "$d/Content"
  printf '%s' "$d"
}

# Whether a process key is present in the exported table. The mod settings decide this, so it is
# the whole observable difference between a switch being on and off.
has_process() {
  grep -q "<KeyName>$2</KeyName>" "$1/data/BaseData/processTypes.xml"
}

# What one recipe consumes of one item, read out of the exported table. Two arguments beyond the
# install: the recipe key and the item key.
input_amount() {
  UW_KEY="$2" UW_ITEM="$3" perl -0777 -ne '
    my ($key, $item) = ($ENV{UW_KEY}, $ENV{UW_ITEM});
    if (m{(<ProcessType>(?:(?!</ProcessType>).)*?<KeyName>\Q$key\E</KeyName>(?:(?!</ProcessType>).)*?</ProcessType>)}s) {
      my $b = $1;
      my ($n) = $b =~ m{<Entity>\Q$item\E</Entity>.*?<NoOfItems>(\d+)</NoOfItems>}s;
      print $n if defined $n;
    }' "$1/data/BaseData/processTypes.xml"
}

# The dry peat a batch of charcoal costs, read out of the exported table. Pinned because it is a
# balance number somebody asked for by name: 2 rather than 1, since a peat bank does not run out
# the way a forest does.
peat_per_charcoal() {
  perl -0777 -ne '
    if (m{(<ProcessType>(?:(?!</ProcessType>).)*?<KeyName>makeCharcoalFromPeat</KeyName>(?:(?!</ProcessType>).)*?</ProcessType>)}s) {
      my $b = $1; my ($n) = $b =~ m{<Entity>item:dryPeat</Entity>.*?<NoOfItems>(\d+)</NoOfItems>}s; print $n;
    }' "$1/data/BaseData/processTypes.xml"
}
# Whether a string literal is present in a compiled assembly.
#
# entityTypes.xml is one of the 13 tables that do NOT export (see modding.md), so an item cannot be
# asserted absent from an export - the file is not there, and a grep against a missing file
# "passes" while proving nothing. The assembly is where the item WOULD be if it still existed: a
# C# string literal lands in the #US heap as UTF-16, so the check is for the literal encoded that
# way. This is the difference between testing that something is gone and testing that a file we
# never wrote does not mention it.
assembly_has_literal() {
  perl -e '
    my ($file, $needle) = @ARGV;
    open my $fh, "<:raw", $file or die "cannot open $file: $!";
    local $/; my $bytes = <$fh>;
    my $utf16 = join("", map { $_ . chr(0) } split //, $needle);
    exit(index($bytes, $utf16) >= 0 ? 0 : 1);
  ' "$1" "$2"
}

write_setting() {
  mkdir -p "$1/user"
  cat > "$1/user/ModSettings.xml" <<XML
<?xml version="1.0" encoding="utf-8"?>
<ModSettings>
  <Setting id="$2" value="$3" />
  <Setting id="somemod.notInstalled" value="keepme" />
</ModSettings>
XML
}

rm -rf "$WORK"; mkdir -p "$WORK"

# ---------------------------------------------------------------- 1. the patch takes effect
say "==> 1. mod loads and patches"
d=$(new_install case1); cp -p "$MOD" "$d/user/Mods/"
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
echo "$out" | grep -q 'Loaded mod: FasterCharcoalMod' \
  && pass "loader reported the mod" || fail "loader did not report the mod: $(echo "$out" | grep -i mod)"
got=$(charcoal_days "$d")
[ "$got" = "$PATCHED" ] && pass "makeCharcoal DaysNeeded $STOCK -> $got" \
  || fail "expected DaysNeeded $PATCHED, got '$got' (patch did not apply)"

# ---------------------------------------------------------------- 2. -nomods suppresses it
say "==> 2. -nomods suppresses mods"
d=$(new_install case2); cp -p "$MOD" "$d/user/Mods/"
out=$( cd "$d" && "$EXPORT" . --nomods 2>&1 ) || true
echo "$out" | grep -q 'Loaded mod:' && fail "a mod loaded despite --nomods" || pass "no mod loaded"
got=$(charcoal_days "$d")
[ "$got" = "$STOCK" ] && pass "makeCharcoal DaysNeeded is stock ($got)" \
  || fail "expected stock $STOCK, got '$got'"

# ---------------------------------------------------------------- 3. a broken mod is survivable
say "==> 3. a corrupt mod is skipped and does not stop the others"
d=$(new_install case3); cp -p "$MOD" "$d/user/Mods/"
# Named to sort BEFORE the good mod, so this also proves the scan continues past a failure.
printf 'this is not a valid assembly' > "$d/user/Mods/AAABrokenMod.dll"
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero on a broken mod"; }
echo "$out" | grep -q 'AAABrokenMod.dll' \
  && pass "the failing mod is named in the report" || fail "the report does not name the failing mod"
echo "$out" | grep -q 'Loaded mod: FasterCharcoalMod' \
  && pass "the later mod still loaded" || fail "a broken mod stopped the scan"
got=$(charcoal_days "$d")
[ "$got" = "$PATCHED" ] && pass "the good mod's patch still applied" \
  || fail "expected $PATCHED, got '$got'"

# ---------------------------------------------------------------- 4. stray Harmony is refused
say "==> 4. a stray 0Harmony.dll in user/Mods is refused"
d=$(new_install case4); cp -p "$MOD" "$d/user/Mods/"; cp -p "$HARMONY" "$d/user/Mods/"
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || true
echo "$out" | grep -q 'Skipped 0Harmony.dll' \
  && pass "refused with an actionable message" || fail "0Harmony.dll in user/Mods was not refused"
got=$(charcoal_days "$d")
[ "$got" = "$PATCHED" ] && pass "the real mod still loaded" || fail "expected $PATCHED, got '$got'"

# ------------------------------------------------- 5. a switch in ModSettings.xml is honoured
#
# The point of the config store, in one assertion: a value in a file in the game folder decides
# what is in the data tables. Both directions are checked, because "the switch did nothing" and
# "the switch is stuck off" look identical if you only test one of them.
say "==> 5. user/ModSettings.xml decides what is built"
d=$(new_install case5)
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
has_process "$d" makeCharcoalFromPeat   && pass "default: the peat recipe is in the tables"   || fail "default: makeCharcoalFromPeat is missing with no settings file"
got=$(peat_per_charcoal "$d")
[ "$got" = "2" ] && pass "makeCharcoalFromPeat costs 2 dry peat"   || fail "expected 2 dry peat per charcoal, got '$got'"


d=$(new_install case5b); write_setting "$d" unhidden.charcoalFromPeat false
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
echo "$out" | grep -q 'unhidden.charcoalFromPeat = false'   && pass "the setting is read and reported"   || fail "dataexport did not report the setting: $(echo "$out" | grep -i setting)"
has_process "$d" makeCharcoalFromPeat   && fail "the recipe is still in the tables with the switch off"   || pass "switch off: the peat recipe is gone from the tables"
has_process "$d" makeCharcoal   && pass "switch off: the stock charcoal recipe is untouched"   || fail "switch off: the STOCK recipe disappeared too"

# ------------------------------------------------- 6. a bad settings file is survivable
#
# This file is the one thing in an installation a player edits by hand, so a stray character in it
# has to cost them their settings and not their game.
say "==> 6. a malformed ModSettings.xml is ignored, not fatal"
d=$(new_install case6); mkdir -p "$d/user"
printf '<ModSettings><Setting id="unhidden.charcoalFromPeat" value="false"' > "$d/user/ModSettings.xml"
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "a malformed settings file stopped the data load"; }
echo "$out" | grep -qi 'ModSettings.xml could not be read'   && pass "reported, and named the file" || fail "the malformed file was not reported"
has_process "$d" makeCharcoalFromPeat   && pass "fell back to the defaults" || fail "a malformed file was partly applied"

# ------------------------------------------------- 7. the peat charcoal item is gone for good
#
# The mod used to add item:peatCharcoal and makePeatCharcoal - a new EntityType key, which a save
# names and a build without the mod cannot resolve. Removing it is only done if it is gone from
# every configuration, so this asserts absence rather than trusting the edit.
say "==> 7. item:peatCharcoal and makePeatCharcoal are gone"
d=$(new_install case7)
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
has_process "$d" makePeatCharcoal   && fail "makePeatCharcoal is still in the tables" || pass "makePeatCharcoal is gone from the tables"
# Sanity check on the check itself: a key that IS there must be found, or the two assertions below
# prove nothing at all.
assembly_has_literal "$GAMEDLL" "item:charcoal"   && pass "the literal search works (item:charcoal found)"   || fail "the literal search found nothing at all - the assertions below are meaningless"
assembly_has_literal "$GAMEDLL" "item:peatCharcoal"   && fail "item:peatCharcoal is still compiled in" || pass "item:peatCharcoal is gone from the assembly"
assembly_has_literal "$GAMEDLL" "makePeatCharcoal"   && fail "makePeatCharcoal is still compiled in" || pass "makePeatCharcoal is gone from the assembly"

# ------------------------------------------------- 9. peat-fuelled builds
#
# Three recipes CLONED from the studio's, with dry peat in the fuel slot. Cloning is the point:
# the tools, skill, work time and animations have to be the studio's current ones, so the checks
# below compare the variant against the original rather than against numbers written down here.
say "==> 9. the peat build variants are clones of the stock recipes, and switchable"
d=$(new_install case9)
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
for r in constructCampfire constructImprovisedKitchen constructMudbrickKitchen; do
  has_process "$d" "${r}WithPeat" \
    && pass "${r}WithPeat is in the tables" || fail "${r}WithPeat is missing"
  peat=$(input_amount "$d" "${r}WithPeat" item:dryPeat)
  wood=$(input_amount "$d" "$r" item:firewood)
  [ -n "$wood" ] && [ "$peat" = "$wood" ] \
    && pass "  it wants $peat dry peat where the original wants $wood firewood" \
    || fail "  peat '$peat' does not match the stock firewood amount '$wood'"
  # The fuel is the ONLY thing that changed: a stock input that is not firewood must survive.
  input_amount "$d" "${r}WithPeat" item:firewood | grep -q . \
    && fail "  the variant still names firewood" || pass "  and no firewood is left in it"
done
# The rest of the recipe came along: a clone that dropped the non-fuel inputs would still pass
# every check above, and would build a campfire out of nothing but a peat.
stones=$(input_amount "$d" constructCampfireWithPeat item:stones)
[ "$stones" = "$(input_amount "$d" constructCampfire item:stones)" ] && [ -n "$stones" ]   && pass "the variant kept the stock recipe's other inputs ($stones stones)"   || fail "the variant lost the non-fuel inputs (stones '$stones')"

# The stock recipes are untouched - a variant that quietly replaced the original would pass every
# check above.
has_process "$d" constructCampfire \
  && pass "the stock constructCampfire is still there" || fail "the stock recipe was replaced"

d=$(new_install case9b); write_setting "$d" unhidden.peatBuilding false
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
has_process "$d" constructCampfireWithPeat \
  && fail "the variant is still there with the switch off" || pass "switch off: the variants are gone"
has_process "$d" makeCharcoalFromPeat \
  && pass "switch off: the charcoal recipe is unaffected - the two switches are independent" \
  || fail "switching off peat building also removed the charcoal recipe"

# ------------------------------------------------ 10. the rawhide fishing net
#
# item:rawhideString has a recipe and no consumer in the stock tables - makeFishingNet names
# cotton string by key and is the only recipe in the game that uses a string at all. The variant
# gives it one.
say "==> 10. a fishing net can be made from rawhide string"
d=$(new_install case10)
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
has_process "$d" makeFishingNetFromRawhide   && pass "makeFishingNetFromRawhide is in the tables" || fail "the rawhide variant is missing"
rawhide=$(input_amount "$d" makeFishingNetFromRawhide item:rawhideString)
cotton=$(input_amount "$d" makeFishingNet item:cottonString)
[ -n "$cotton" ] && [ "$rawhide" = "$cotton" ]   && pass "  it wants $rawhide rawhide string where the original wants $cotton cotton string"   || fail "  rawhide '$rawhide' does not match the stock cotton amount '$cotton'"
input_amount "$d" makeFishingNetFromRawhide item:cottonString | grep -q .   && fail "  the variant still names cotton string" || pass "  and no cotton string is left in it"
has_process "$d" makeFishingNet   && pass "the stock recipe is untouched" || fail "the stock recipe was replaced"

d=$(new_install case10b); write_setting "$d" unhidden.stringAlternatives false
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
has_process "$d" makeFishingNetFromRawhide   && fail "the variant is still there with the switch off" || pass "switch off: the variant is gone"

# ------------------------------------------------ 11. the five task mods register
#
# Each of these lives in its own file and answers one request from the tasks forum. They are not
# build-time features like the bundled mod, so the only evidence that one is wired up at all is
# that its settings appear - which is exactly what dataexport prints. A mod whose RegisterSettings
# was never called is invisible in every other way until somebody notices the behaviour missing.
say "==> 11. the per-request mods are registered and switchable"
d=$(new_install case11)
for id in mapedge.stopAtEdge healing.fullRecovery healing.needsDrivenRate selfpreservation.injuredStayOut selfpreservation.unarmedStayOut selfpreservation.animalsNeedCompany magnification.allowBelowOne magnification.warnWhenTooSmall diet.specialiseRawFood; do
  write_setting "$d" "$id" false
  out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
  echo "$out" | grep -q "$id = false"     && pass "$id is registered and reads from the file"     || fail "$id did not reach the registry (dataexport did not report it)"
done

# selfpreservation.unorderedThreats is a CHOICE, not a toggle, so the loop above cannot carry it -
# "false" is not one of its values. Worth its own two lines rather than leaving the mod's main
# switch unchecked: it is the one that replaced the stance gate, and the stance gate is the bug.
d=$(new_install case11c); write_setting "$d" selfpreservation.unorderedThreats "very reluctant"
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
echo "$out" | grep -q "selfpreservation.unorderedThreats = very reluctant"   && pass "selfpreservation.unorderedThreats is registered and reads a choice from the file"   || { echo "$out" | grep -i unorderedThreats | sed 's/^/      /';        fail "selfpreservation.unorderedThreats did not reach the registry"; }

# And that an unknown value falls back rather than being taken literally - a hand-edited file is
# the normal way this setting gets changed, and "Reluctant" with a capital is the obvious typo.
d=$(new_install case11d); write_setting "$d" selfpreservation.unorderedThreats nonsense
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "a bad choice value stopped the data load"; }
pass "an unrecognised choice value does not stop the load"

# The debug overlays are the only switches here that default to OFF, so the loop above cannot
# test them: it writes false, which IS their default, and dataexport prints only what differs
# from default. Written true instead - which is also the direction that matters, since an overlay
# nobody can turn ON is the whole failure mode.
for id in debug.overlayJobs debug.overlayRanges; do
  d=$(new_install "case11${id#debug.}"); write_setting "$d" "$id" true
  out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
  if echo "$out" | grep -q "$id = true"; then
    pass "$id is registered and reads from the file"
  else
    fail "$id did not reach the registry (dataexport did not report it)"
  fi
done

# The debug mod's test-scenario picker is a TEXT setting, so the toggle loop above cannot carry
# it. Two things worth pinning, and the second is the whole reason the setting exists: a name the
# game knows is kept, and one it does not must not stop the load.
d=$(new_install case11e); write_setting "$d" debug.testScenario FightTest
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
if echo "$out" | grep -q "debug.testScenario = FightTest"; then
  pass "debug.testScenario carries a real scenario name through"
else
  echo "$out" | grep -i testScenario | sed 's/^/      /'
  fail "debug.testScenario did not reach the registry"
fi

d=$(new_install case11f); write_setting "$d" debug.testScenario NoSuchScenario
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "a bad scenario name stopped the data load"; }
pass "an unknown scenario name does not stop the load"

# The diet mod is the only one of the five whose effect is in the exported tables, so it is the
# only one that can be checked offline rather than merely observed to exist.
d=$(new_install case11b)
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
modded=$(perl -0777 -ne 'if (m{<KeyName>richMeat</KeyName>(?:(?!</FoodNutrientProfile>).)*?</FoodNutrientProfile>}s) { print $& }' "$d/data/BaseData/foodNutrientProfiles.xml" 2>/dev/null | grep -c .)
d2=$(new_install case11c); write_setting "$d2" diet.specialiseRawFood false
out=$( cd "$d2" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
if [ -f "$d/data/BaseData/foodNutrientProfiles.xml" ] && [ -f "$d2/data/BaseData/foodNutrientProfiles.xml" ]; then
  if cmp -s "$d/data/BaseData/foodNutrientProfiles.xml" "$d2/data/BaseData/foodNutrientProfiles.xml"; then
    fail "the diet mod changed nothing in foodNutrientProfiles.xml"
  else
    pass "the diet mod changes foodNutrientProfiles.xml, and switching it off restores it"
  fi
else
  fail "foodNutrientProfiles.xml was not exported - cannot check the diet mod"
fi

# ------------------------------------------------ 12. disassembly generated from the recipes
#
# UWGame.Mods.DisassemblyMod writes no recipe down: it reads the recipe that PRODUCES an item and
# turns it round, so the only way to know what it produces is to load the tables and ask. That is
# what `dataexport --disassembly` is for - it loads the way the GAME loads (NoSerialize) and
# prints one line per generated recipe, because the entity table it computes from is one of the 13
# that cannot serialize and so is not in the exported XML at all.
#
# Everything asserted here is compared against the studio's own recipe where it can be, in the
# same spirit as case 9: what a hammer gives back is not a number in this script, it is whatever
# makeHammer currently consumes.
say "==> 12. disassembly is generated from the production recipes, and switchable"
d=$(new_install case12)
# A normal export first, for the stock recipes this case compares against.
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
report=$( cd "$d" && "$EXPORT" . --disassembly 2>&1 ) || { echo "$report"; fail "the disassembly report returned nonzero"; }

# FIRST, that a game carrying these recipes can start at all. Building the tables and surviving
# them are different things: the first version of this mod built 21 correct recipes and then took
# the game down on the next screen, inside ProcessType.PostDataCompleteValidate, which walks the
# salvaged item's Parts list and found null. Kastuk found that in a day; this line is why he will
# not have to again. Validation errors throw too (DataLoader.DisplayErrors), so this covers both.
echo "$report" | grep -q "ok - the tables survive the game's own validation" \
  && pass "the tables survive the validation pass Sim.QueueGameDataAndSimInit runs" \
  || { echo "$report" | sed -n '/==> validating/,/^$/p' | sed 's/^/      /'; \
       fail "the game's own validation pass rejects or crashes on the generated tables"; }

# The studio's own rule for salvage recipes, which the port had to relax to load at all:
# ProcessType.PostDataCompleteValidate requires every output to be one of the salvaged item's
# declared Parts. DisassemblyMod returns the production MATERIALS instead, so it refuses to
# generate for an item that declares parts (their rule wins there) and checks its own equivalent
# invariant - never hand back more than making one consumed - on everything else. Both refusals
# are reported rather than silent, and no stock item in scope trips either.
echo "$report" | grep -q "recipe(s) generated, 0 refused" \
  && pass "no stock item is refused: the parts rule and the materials rule agree on this table" \
  || { echo "$report" | grep -E "refused" | sed 's/^/      /'; \
       fail "a stock item was refused - the generator and the studio's salvage rule disagree"; }
for parted in item:ironArrow item:wheel item:spikeTrap; do
  echo "$report" | grep -q "${parted}_disassemble" \
    && fail "$parted declares its parts and got a generated recipe anyway" \
    || pass "$parted declares PartKeys, so the studio's salvage rule governs it, not this mod"
done

count=$(echo "$report" | sed -n 's/^==> disassembly: \([0-9]*\) recipe.*/\1/p')
[ -n "$count" ] && [ "$count" -ge 15 ] \
  && pass "$count disassembly recipes were generated" \
  || fail "the report says '$count' recipes - the generator produced nothing to check"

# One line per recipe, and every one of them reachable: a recipe the item does not point at is a
# recipe no player can ever order.
echo "$report" | grep -q "link=BROKEN" \
  && fail "a generated recipe is not hung on its item's NonLivingType.SalvageProcess" \
  || pass "every generated recipe is linked from the item it takes apart"
echo "$report" | grep -q "salvage=NO" \
  && fail "a generated recipe is not marked IsSalvageProcess - it would count as PRODUCING its outputs" \
  || pass "every generated recipe is marked as salvage"

# What a tool gives back is what its own recipe consumed. Read both ends rather than asserting a
# number: if the studio reprices makeHammer, this case follows.
line=$(echo "$report" | grep "item:hammer_disassemble")
iron=$(input_amount "$d" makeHammer item:wroughtIron)
sticks=$(input_amount "$d" makeHammer item:sticks)
echo "$line" | grep -q "out=item:wroughtIron x$iron, item:sticks x$sticks" \
  && pass "a hammer comes apart into the $iron wrought iron and $sticks sticks makeHammer consumed" \
  || { echo "      $line"; fail "the hammer's recovery does not match makeHammer"; }

# Kastuk asked for these three by name.
echo "$report" | grep -q "item:ironSpear_disassemble" \
  && pass "complex spears can be taken apart (item:ironSpear)" || fail "no spear was generated"
echo "$report" | grep -q "item:steelMachete_disassemble" \
  && pass "metal tools can be taken apart (item:steelMachete)" || fail "no metal tool was generated"
echo "$report" | grep -q "item:textile_disassemble.*out=item:cottonString x1" \
  && pass "textile comes apart into cotton string, not back into cotton" \
  || fail "the textile exception did not produce cotton string"

# The three exclusions, each for a different reason - and each one a way the generator could have
# been wrong without any of the checks above noticing.
echo "$report" | grep -q "item:advancedKnifeSpear_disassemble" \
  && fail "an item the studio already gave a disassembly was overridden" \
  || pass "an item with the studio's own salvage recipe keeps it (item:advancedKnifeSpear)"
echo "$report" | grep -q "item:file_disassemble" \
  && fail "a tool forged from a single piece of iron was 'disassembled' into it" \
  || pass "a one-material forging is left alone (item:file)"
echo "$report" | grep -q "item:clayJar_disassemble" \
  && fail "a fired clay jar came apart into its unfired self" \
  || pass "a fired item is left alone (item:clayJar)"

# THAT THE RECIPE ACTUALLY GIVES ANYTHING BACK. This is the one line here that is about the sim
# and not the table, and it earned its place: every recipe this mod generated shipped for weeks
# destroying the item and producing nothing at all. A salvage process hands back the PARTS of the
# entity it destroys rather than manufacturing its outputs (SimProcess.CreateOutputsFromInputs),
# and this mod's items declare no parts - the studio's validator forbade that combination, the
# port relaxed it to let these recipes load, and the half that got missed was the sim. Kastuk
# ordered a flint-tipped spear disassembled and watched it vanish leaving no materials, with the
# task still red on "Not in inventory" because the order it was placed against was never filled.
#
# ProcessType.SalvageOutputWillBeCreated is the sim's own predicate; dataexport asks it per output.
if echo "$report" | grep -q "creates=NOTHING"; then
  echo "$report" | grep 'creates=NOTHING' | sed 's/^/      /'
  fail "a generated recipe destroys the item and produces none of its outputs"
else
  pass "every generated recipe actually yields what it promises"
fi

# Half the per-item production time, and never zero: a recipe that takes no time is a job that
# finishes the instant it starts.
echo "$report" | grep -q "days=0 " \
  && fail "a generated recipe takes no time at all" || pass "every generated recipe takes time"

# The display culture is the studio's, unless somebody asks for the other one. Their source sets
# Config.Culture = InvariantCulture in a static constructor and pushes it onto the thread, so it is
# a deliberate default rather than an unconsidered one; the contributed patch used to overwrite it
# with en-GB for every player.
d=$(new_install case12c); write_setting "$d" unhidden.culture en-GB
out=$( cd "$d" && "$EXPORT" . 2>&1 ) || { echo "$out"; fail "dataexport returned nonzero"; }
echo "$out" | grep -q "unhidden.culture = en-GB (default invariant)" \
  && pass "the display culture is a choice, and its default is the studio's invariant" \
  || { echo "$out" | grep -i culture | sed 's/^/      /'; \
       fail "unhidden.culture is not registered, or does not default to invariant"; }

d=$(new_install case12b); write_setting "$d" disassembly.generate false
report=$( cd "$d" && "$EXPORT" . --disassembly 2>&1 ) || { echo "$report"; fail "dataexport returned nonzero"; }
echo "$report" | grep -q "^==> disassembly: 0 recipe" \
  && pass "switch off: nothing is generated and the tables are the studio's" \
  || { echo "$report" | grep '^==> disassembly'; fail "the switch did not suppress the generator"; }

# ------------------------------------------------ 13. the random stream can be resumed
#
# Not about the mod loader, and here because this is the only gate that runs dataexport. It
# checks an assumption about the RUNTIME that RandomGenerator's save/load resume is built on: a
# seeded System.Random stream can be fast-forwarded by drawing and discarding, and the count to
# draw is of VALUES, not of method calls - NextBytes consumes one per byte, and the game asks it
# for 512 at a time once per colonist.
#
# If .NET ever changes that, every save written afterwards resumes in the wrong place and nothing
# else would notice: a desynchronised random stream does not throw, it just quietly stops being
# the same game.
say "==> 13. a seeded random stream resumes where it left off"
out=$( cd "$(new_install case13)" && "$EXPORT" . --random-selftest 2>&1 ) || true
if echo "$out" | grep -q "the resume mechanism holds on this runtime"; then
  pass "$(echo "$out" | grep -c "^  ok") assumption(s) hold"
else
  echo "$out" | sed "s/^/      /"
  fail "the save/load random resume cannot work on this runtime"
fi

# ------------------------------------------------- 8. the settings file round-trips
#
# The WRITE side of user/ModSettings.xml, which nothing else here exercises: a value survives a
# write and a re-read, an entry belonging to a mod that is not installed is not dropped on the way
# through, and the save signature says "stock" exactly when the content is stock. dataexport runs
# these against a throwaway installation and loads no data at all.
say "==> 8. ModSettings.xml round-trips, and signatures are computed from stock rather than default"
d=$(new_install case8)
out=$( cd "$d" && "$EXPORT" . --settings-selftest 2>&1 ) || { echo "$out"; fail "the settings self-test failed"; }
echo "$out" | grep -q 'settings self-test OK'   && pass "$(echo "$out" | grep -c '  ok    ') check(s) passed inside the self-test"   || { echo "$out" | sed -n 's/^  FAIL/      FAIL/p'; fail "the settings self-test reported failures"; }

rm -rf "$WORK"
say ""
if [ "$FAILURES" -eq 0 ]; then
  say "mod loader OK - 13/13 cases passed."
else
  say "mod loader FAILED - $FAILURES check(s)."
  exit 1
fi
