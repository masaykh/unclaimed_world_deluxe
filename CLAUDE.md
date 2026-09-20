# Working in this repository

Notes for anyone — human or agent — changing code here. Everything below was learned by getting
it wrong first; each rule names the failure that produced it.

`build.md`, `modding.md` and `license.md` describe what the project *is*. This file is about how
not to break it.

---

## 1. Read `original_src/` first. Always.

Before any functionality investigation and before any core change, open the studio's own source
for the file you are about to touch. Paths map:

```
original_src/UnclaimedWorld/Sim/...   <->   base_game/UnclaimedWorld/UWGame/...
```

`base_game/` is ILSpy output from the shipped 1.0.4.8 assembly, and **comments do not exist in
IL**. It is missing roughly **68,400 comment lines** — including commented-out code that records
unfinished features, abandoned approaches, and the reason a thing is the way it is.

This is not a nicety. Three separate investigations turned entirely on material that only exists
as a comment:

- **Vehicles.** `Entity.cs`: `container = new VehicleContainer(this); // Add(new Vehicle(this));`
  — the migration caught mid-step. Nothing constructs `Vehicle` any more, yet
  `GoalEnterVehicleAsDriver`, `GoalEnterVehicleAsPassenger` and `GoalExitVehicle` all open by
  reading it, so every vehicle goal dies on its first line.
- **Workshop upgrades.** `// the salvage process may still be ongoing... perhaps avoid this?` sits
  at the exact decision point, with a bare `// salvage upgrades:` heading and nothing beneath it.
  That is what showed the system was *unfinished* rather than *broken*.
- **`Sim/Vehicles/VehicleType.cs`** is commented out wholesale (`/* public class VehicleType`). It
  is in the studio's csproj but has no live code, so it cannot appear in `base_game/` at any path.
  Searching there for it finds nothing and tells you nothing.

**The original is sometimes more correct than the port.** MonoGame added `MathHelper.Max(int,
int)` overloads XNA never had, so the decompiler resolved them and dropped casts the studio
actually wrote. Some of the port's differences are decompiler damage to undo, not logic to keep.

**Decompiler noise has a readable counterpart.** `_ = Body.Parent.ID; _ = 19;` in
`BodyPart.DoDamage` is an empty `if (... && Body.Parent.ID == (EntityID)19) { }` debug
breakpoint. `Body.RegainHitpoints` has the same one keyed on a colonist named "Conlan". Neither
is corruption, and neither is worth "fixing".

`original_src/` is reference material. **Nothing in it is built** — two deliberately empty
`Directory.Build.props` / `Directory.Packages.props` files stop MSBuild and NuGet inheriting the
repository's real settings into its 11 stray `.csproj` files. Do not delete them.

---

## 2. Every `.cs` file is CRLF, on every platform

`.gitattributes` says `*.cs text eol=crlf`. That applies to Linux CI runners too.

**This kept CI red for three days and nobody could see why.** `tools/build/83-verify-debug-scenarios.sh`
read a method body by printing lines until one equalled a tab and a closing brace. On Linux the
line is tab, brace, **CR** — never equal — so the body ran to end of file and every scenario
measured the largest `Point` in the *rest of the file*. It reported "91 of 94 do not fit their
map", with nearly every scenario naming the same coordinates. It was not finding 91 failures; it
was reporting one number 91 times. `HuntTest` reads 17 lines correctly and 2697 that way.

**Nothing local catches this.** msys2's gawk reads in text mode and strips the CR before awk ever
sees it, so the script passes on Windows and cannot pass on Linux.

- In shell gates, `sub(/\r$/, "")` before comparing a whole line. `grep -o` is already safe — its
  output never carries the line terminator.
- Reproduce a Linux runner locally with **`awk -v BINMODE=3`**, which disables that translation.
  (As an environment variable it is ignored; it must be `-v`.)
- When patching a `.cs` file with a script, match on `\r\n` or split lines first. A literal `\n`
  anchor silently matches nothing.

---

## 3. The version the game displays is a hand-edited attribute

`base_game/Directory.Build.props` sets `GenerateAssemblyInfo=false`, so **`-p:Version` does
nothing**. The only source is:

```
base_game/UnclaimedWorld/Properties/AssemblyInfo.cs
```

`UnclaimedWorld.GetVersion()` reads `AssemblyVersion` off the running assembly, and that feeds the
main menu, the fatal-error title, the save/load panel's "current version" notes and the replay
header. It sat at 1.0 through the v1.1, v1.2 and v1.3 tags — three releases shipped in archives
named for a version the game itself denied.

`tools/build/70-make-release.sh` now refuses to cut a release whose number disagrees with that
file, and says what to change. Bump `AssemblyVersion` **and** `AssemblyFileVersion` in the same
commit as the tag. Pre-release cuts with a suffix (`0.0-dev`) are exempt so testing is not blocked.

`UnclaimedWorld.deps.json` saying `1.0.0` is unrelated — that is the MSBuild `$(Version)` default
and nothing reads it.

### Assembly identity that must never change

Changing `UnclaimedWorld`'s own version is safe: nothing names it in any XNB, and
`ContentTypeReaderManager.PrepareType` strips versions from reader names. Saves are unaffected —
`SnapshotHeader` reads `ProgramVersion` back through `sn.Ignore`, a no-op.

**The sibling `AssemblyInfo.cs` files are not safe.** XNBs embed assembly-qualified
`ContentTypeReader` names, so `SpriteSheetRuntime` and `Xclna.Xna.Animationx86` must keep their
exact `AssemblyVersion`, `Guid`, `Company` and `Copyright`. 47 model XNBs name
`Xclna.Xna.Animationx86, Version=1.0.2.0` explicitly. Do not tidy those files.

---

## 4. Core versus mods

> Base game code is the original source with our **core** changes on top — the move to modern
> MonoGame and DesktopGL, and bug fixes. Any gameplay change that is **not** a bug fix goes in
> `mods/`.

A bug fix in a core path a mod happens to reach stays in core: it is a fix to the game, not a
piece of the mod. `SimProcess.CreateOutputsFromInputs` is the worked example.

Every feature has an `.Absent.cs` stub in `base_game/UnclaimedWorld/UWGame/Mods/`, so call sites
are byte-identical whether a feature is in or out. **A stub returns the studio's own answer, not a
merely harmless one** — `HealingMod.Absent.RateFactor` returns `1`, not `0`, which would stop
healing altogether.

Core must build with `mods/` deleted. CI runs exactly that, then greps the built assembly for
strings that only exist inside a gameplay mod — plus one of the studio's own as a control,
because a check that stops finding anything has stopped checking.

---

## 5. Gates, not launches

Verify offline. Ask before launching the game, and batch the questions — do not ask per
iteration. A gap in the automated gates is a gap that reaches players.

**The gates:**

| | |
|---|---|
| `tools/build/80-verify-modloader.sh` | the mod loader, through a real `ContentManager` |
| `tools/build/82-verify-docs-current.sh` | documents that assert something the repo stopped doing |
| `tools/build/83-verify-debug-scenarios.sh` | every debug scenario fits the map it is registered against |
| `tools/ContentProbe` | every asset loads on a real `GraphicsDevice` |

**Three rules learned the hard way:**

1. **Prove a new gate fails on the known-bad input** before trusting it. A gate that has never
   gone red has not been tested; it has been written.
2. **Loading the data is not validating it.** `GameData.PostDataCompleteInitialize` — every type's
   `PostDataCompleteValidate` — runs after the tables exist, and that is where bad data kills the
   game. A disassembly mod shipped with 12 green cases and crashed every save load the next day,
   in `ProcessType.PostDataCompleteValidate`. If a change touches recipes, items or tables, the
   gate must load **and** validate.
3. **Some failures are invisible to every gate.** SharpDX pinned 4.0.1 → 4.2.0 built clean, passed
   the asset probe, was packaged and shipped — then killed the game at startup, because the game
   does not reference SharpDX and loading content never touches MediaFoundation. When you find
   such a class of bug, add it to a gate rather than leaving it to somebody launching the game.

**Scope backend workarounds.** `#if UW_GL` / `#if UW_DX` exist for this. Two GL workarounds were
applied to both targets, and one silently disabled the 4x MSAA the DirectX build requests.

**Prefer numeric measurement to reading pixels.** Measuring a constant colour across all 411,840
pixels is what actually found the vertex-usage-enum bug.

---

## 6. Building and packaging

`UW_STEAM` must be **exported**, not just assigned — the scripts are child processes and a plain
assignment is invisible to them.

```sh
export UW_STEAM="/c/Program Files (x86)/Steam/steamapps/common/Unclaimed World"
```

It is needed for **compiled `Content/` only**. `assets/` holds 4015 *source* files and zero
`.xnb`, and nothing in the repo compiles them yet. Steam integration needs nothing from your
install: `native/steam/steam_api64.dll` is committed, and the appid is written from a constant.

**There is no `61-package-dx.sh`.** It was pruned at the split, and `82-verify-docs-current.sh`
fails any document that names it. The DirectX route is the release script, one platform at a time
— see the DirectX section of `build.md`. `35-make-dx-effects.sh` compiles nothing; it rewrites the
studio's v8 MGFX container to v10 and copies the DXBC through, so DX needs no shader compiler at
all.

---

## 7. A recurring shape worth recognising

Four separate bugs in this codebase turned out to be *a feature that exists and does not work*:

- `ProcessJob.UserCanCancel` tells the player upgrades can only be cancelled in the upgrade
  window. That window did not cancel.
- `JobManager.SelectBestCombo` — a method named "best" — returns `combos[0]`. It selects nothing,
  so a second recipe for any item can only ever be a fallback.
- `StockpileWindow` called the studio's `DoCategorySorting` correctly, and sorted by
  `OrderByTag1`, which it never assigned. `OrderBy` is stable, so it reordered nothing.
- Three vehicle AI goals read a component nothing constructs.

When something "does not work", check whether it is wired up and inert before assuming it is
missing. And when the studio left a `TODO`, a commented-out call or an empty heading, that is
evidence about intent — read it before replacing it.

---

## 8. Reporting work

Say what was **not** checked. "Builds clean and the gates pass" and "someone played it" are
different claims, and only one of them is usually true. Name the classes and methods involved —
the people reading these updates read the source, and naming the code tells them where to look.
