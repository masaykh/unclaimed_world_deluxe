# Unclaimed World — modding port

A source-level port of *Unclaimed World* (Steam app 284100) from its shipped
**MonoGame 3.6 / .NET Framework 4.5.1** build onto **.NET 8 + MonoGame 3.8.5.1**, so the
game can be modified, rebuilt and run against your own original game assets.

Refactored Games released the source **and the assets** under the Unclaimed World Community
License, so the whole game is here: 3343 textures, 247 models, 235 sounds, the music, the fonts,
the shaders, the maps, the string table. What is not here is compiled content — no `.xnb`,
because that is a build output. Producing a runnable package therefore still reads `Content/`
from your own install, and never writes to it.

**Please [buy the game](https://store.steampowered.com/app/284100/)** if you have not.

## What works

Everything the retail game does, on .NET 8:

- main menu with its animated background and music
- new game, scenario load (including the 17 MB `MapData.xml` maps), save and load
- terrain, models, animation, the full WindowSystem HUD, the simulation
- **Steam achievements**, with the `CSteamworks` shim retired

**Verification status, honestly.** The port was played end to end on game **1.0.3.5** — new game
→ save → load → exit with nothing written to `Errors.txt`. It has since been re-baselined onto
**1.0.4.8** and has *not* had that full play-through repeated. What is checked automatically on
every build, without launching the game, is:

| gate | what it proves |
|---|---|
| `tools/ContentProbe` | every asset loads - 33 of 33, including songs, the menu animation and all 19 effects - on a real GraphicsDevice |
| `tools/build/80-verify-modloader.sh` | Harmony patching works, mods load, the failure paths behave, and `user/ModSettings.xml` decides what gets built |
| `tools/DataExport` | every data table builds, and what the mods actually changed |


So: content, media, data and modding are gated; **rendering and gameplay are not**, and cannot
be — those need a person to look at the screen.

Plus one thing retail did not: `--export-data` dumps the hard-coded game tables to editable
XML — 56 of 69 tables, 3.2 MB — and `--data-from-xml` loads them back. See
[Where the mod hooks are](#where-the-mod-hooks-are).

## Requirements

| | |
|---|---|
| .NET SDK | 8.0 (`global.json` pins 8.0.420, rolls forward on feature band) |
| OS | Windows, Linux or macOS - DesktopGL is the only target that ships |
| A legal copy of the game | to build a runnable package - the compiled `Content/` is a build output and is not committed |

No Visual Studio needed — `dotnet build` is enough. Everything else (the decompiler, the
MonoGame effect compiler) installs as a repo-local dotnet tool.

## Quick start

Building the game is [build.md](build.md); this is the short version.

```sh
sh tools/build/12-fetch-steam-natives.sh          # steam_api64.dll for the pinned Steamworks.NET
sh tools/shadowdusk/build-shadowdusk.sh           # the shader compiler
export UW_SHADOWDUSK="$PWD/artifacts/tools/shadowdusk/bin/ShadowDuskCLI.exe"
sh tools/build/34-build-gl-effects-shadowdusk.sh  # the 19 effects
sh tools/build/32-convert-media.sh                # WMA -> Ogg Vorbis
sh tools/build/60-package-gl.sh Release           # -> artifacts/package/UnclaimedWorld-GL
```

Nothing here writes to your Steam folder. It is only ever **read**, for the compiled `Content/`
- a build output, so not committed here. `game/` is the disposable run target, and Steam's *Verify
integrity of game files* is always the final escape hatch.

`sh tools/build/40-deploy.sh` deploys a build over `game/`, and
`powershell -File tools/build/50-run.ps1 30` launches it and reports.

If you are here to write a mod rather than to work on the port, you need far less than the
above — see [Writing a mod](#writing-a-mod-harmonylib), and check your work with:

```sh
sh tools/build/80-verify-modloader.sh     # proves the loader works, end to end, no launch
```

## Layout

```
base_game/UnclaimedWorld/        the game - 2170 types. Entry point GameStateManagement.Program
base_game/UnclaimedWorld/UWGame/Mods/   the modding FRAMEWORK: loader, settings, Absent stubs
base_game/WindowSystem/          the UI layer (also the merged InputEventSystem + RoundLines)
base_game/SpriteSheetRuntime/    sprite-sheet types - LOAD-CRITICAL, see below
base_game/AnimationComponentRuntime/  skeletal animation (Xclna.Xna.Animationx86) - LOAD-CRITICAL
mods/                            the gameplay mods themselves - compiled in, switchable
assets/Content/                  the studio's asset sources - textures, models, sounds, music,
                                 fonts, the .fx originals, Content.mgcb
assets/WindowSystem-Content/     the UI layer's own fonts and button sounds
assets/effects/                  the HLSL the build compiles - SM3, and NOT the same files as
                                 assets/Content/*.fx. See assets/README.md
assets/MainMenuIntro.uwanim      the menu background animation
scenarios/                       the studio's maps
translations/                    string tables - see translations/README.md before starting
tools/shadowdusk/                the shader compiler: patches, and a script to fetch and build it
tools/MgfxTranscode/             MGFX shader-container tool (transcode / inject / validate)
tools/DataExport/                runs the data loader headlessly - test data mods without launching
tools/ContentProbe/              loads assets through a real ContentManager and reports pass/fail
tools/SpriteExport/              cuts the sprite atlases into one named PNG each, with index pages
tools/build/                     numbered build scripts, run in order
```

The split is the thing to understand: **`base_game/` is the original game plus core work — the
port and bugfixes. `mods/` is everything that changes how the game plays.** A crash is a bug and
is fixed in `base_game`; a balance change is a mod, however clearly it was a mistake.

The split is **enforced, not intended**: `rm -rf mods` and `base_game/` still builds, and CI does
that on every push. Each mod has an `.Absent.cs` stub in
`base_game/UnclaimedWorld/UWGame/Mods/` answering what the studio's own code answered, so the
fifteen places the game calls a gameplay mod are byte-identical either way. Add a mod, add a
stub — otherwise that job is where you find out.

The direction it does *not* go: a bugfix in a core path that only a mod currently reaches is
still core. `SimProcess.CreateOutputsFromInputs` is the example — a salvage process hands back
the destroyed input's parts, and an item with no declared parts got nothing back. That was the
game's bug even though only `DisassemblyMod` could reach it, and it is fixed in `base_game`.

At runtime, in the game folder:

```
user/Mods/*.dll              your mods go here; loaded at startup in filename order
user/ModSettings.xml        mod configuration; also editable in MODDING -> SELECT MODS (MOD OPTIONS)
port-content/**.xnb          ASSET OVERRIDES - any .xnb here wins over Content/ (see below)
data/BaseData/*.xml          written by --export-data, read by --data-from-xml
MainMenuIntro.uwanim         the menu background animation (see below); delete for a still one
Errors.txt                   crash reports AND mod load failures
```

### Asset overrides: `port-content/`

The port never modifies the game's `Content/`. Anything it needs to change ships in
`port-content/`, which `UwContentManager` searches first — so **you can override any asset the
same way**, for any content type, by dropping an `.xnb` of the same relative name in there:

```
port-content/GUI/GUISprites.xnb      replaces Content/GUI/GUISprites.xnb
port-content/Models/demonTree_idle.xnb
```

The mechanism is still there and is the supported way for **you** to override an asset. The port
itself no longer needs it: this DesktopGL build compiles its own effects from
`assets/effects/*.fx` and the packager writes them straight into the package's `Content/`, so
there is nothing left for it to shadow. Your own Steam install is untouched either way — the
package is a separate directory.

### New 3D models: `tools/build/38-build-models.sh`

A new rigid model (a cart, a vehicle, a tool) is made in Blender and built into a model `.xnb`
with the studio's own processors, ported unchanged in `tools/ModelPipeline` (from
`original_src/AnimationComponentPipeline` and `VehiclePipeline`). Put the FBX and its textures
in `assets/port-models/`, add a line to `assets/port-models/models.txt`, and run the script. It
writes to `artifacts/content/models/`.

**Making one.**
- **Format: FBX from Blender** (binary 7.4, the default export). mgcb's importer, Assimp,
  reads FBX 2011 and later.
- **Model in metres, at real size.** Make it inside the reference scene that
  `38-build-models.sh --reference DIR` writes (`model_reference.blend`): the colonist (1.76 m)
  and the MULE as the game draws them. Face the way the MULE faces. The build scales metres to
  game units (13.75 a metre).
- **Rigid, no armature deformation.** Name the body mesh `hull`, which is what
  `ModelData.DriverAttachor` and the passenger seats hang from by bone name. Put each wheel
  under its own empty: `wheel_rear_left_joint`, `wheel_rear_right_joint`, and
  `wheel_front_left_joint` / `wheel_front_right_joint` for wheels that steer
  (`SteerableFrontWheels`). Add an empty named `DriversSeat` where the driver goes. The node
  names come through as the model's bones.
- **One 256×256 TGA** (a power of two), assigned in the material and saved next to the FBX.
  A details map and a dirt map (also 256) are optional, as the MULE's are.

**The studio's own FBX files.** All 247 sources in `assets/Content/Models` are ASCII FBX 6.1.
mgcb's Assimp can't read them. Blender 4.5+ can, but only through its newer importer,
`bpy.ops.wm.fbx_import` (File → Import → FBX, built on ufbx); the older Python importer rejects
them. That makes them usable as reference or for editing: a converted MULE builds back within 2%
of the studio's. It does not make them exact for animation. The studio walk converted this way
comes back up to 16° off on some bones, so new colonist animations use the rig below instead.

**Effects are not compiled here.** A model names its effect as a separate asset (`../Vehicle`,
`../skinFX`), as the studio's do. The port builds those effects from `assets/effects` with
ShadowDusk.

`38-build-models.sh --selftest` builds a stand-in cart (`tools/ModelPipeline/test`) and loads it
through the game's own readers with `contentprobe --model-details`. It checks the bones above, the
scale and the effect reference. `contentprobe --export-obj DIR` writes any loaded model as an OBJ
in metres.

### New colonist animations: the colonist rig

The game keeps every colonist animation inside `man_idle` and `woman_idle`, one clip per bone
channel, keyed by bone name. A new clip (pushing a cart, for one) is made on the skeleton the game
actually plays clips on:

```
sh tools/build/38-build-models.sh --rig DIR      # DIR/colonist_rig.blend
```

`contentprobe --export-rig` writes `man_idle` as the game holds it in memory: bones and their rest
pose, the skin, the skinned mesh, and four studio clips (walk, heavy haul, construction pull,
idle). `tools/ModelPipeline/make_rig.py` turns that into a Blender armature. Every bone keeps the
game's own local frame, so the game's bones point along X and look sideways in Blender. The
clips become actions, and the armature object alone is stood upright.

**The proof.** `38-build-models.sh --selftest` takes the studio's walk onto the rig, exports it
with `tools/ModelPipeline/export_clip.py` as an animator would, builds it, and compares every
key with the original (`compare_clip.py`). All 30 compared channels come back within 0.06°.
Positions and scale come back exact, and so do the duration, the start offset and the gait flag.

**Making one.** Animate on `colonist_rig.blend` at 30 fps, in place (the game moves the
colonist). Frame 0 is the rest pose, and the clip starts after it, as the studio's do. The
studio's clips carry their start offset and flags in a sidecar `Models/<file>.xml`
(`<startoffset>` in ticks, `<isgait>`, `<actionpoint>`), which the processor reads.

**Adding it to the colonist.** In `assets/port-models/models.txt`, a line
`man_<clip> Clip` builds the clip into `Models/clips/man_<clip>.xnb`. When the game loads
`man_idle` or `woman_idle`, `UWGame.Port.ExtraClips` (called from `UwContentManager.Load`) adds
every `Models/clips/man_*` clip under its own name: `man_pushCart` becomes `pushCart`, and the
woman gets the man's clips, as in the studio's project. The studio's clips are never replaced.
The two hand attachments keep the studio's key: every studio clip that animates them holds one
key, and `AttachorPalmRight`'s is mirrored (scale 1, −1, 1), which Blender's FBX can't carry. The
self-test reads the rebuilt walk back out of the real `man_idle`: all 31 channels match, the
mirrored one included.

### Seeing every sprite: `tools/SpriteExport`

The world, the ground sprites, the ghosted buildings and the interface are drawn from six compiled
atlases. A data table names a sprite by its name in one of them (`tree_daysheen_1_grown_cut`,
`HUD_thumbnail_spoak`). To see them all, with their names:

```sh
dotnet build tools/SpriteExport/SpriteExport.csproj -c Release
artifacts/bin/SpriteExport/release/spriteexport.exe "<game>/Content" sprites --sources assets/Content
```

`sprites/index.html` links one page per atlas, with every sprite, its name, its size and the
source file it was built from, and a name filter. `sprites.csv` lists each sprite's rectangle in its
atlas. `--normals` also exports the depth-derived normal maps of `BuildingsAndTrees` into `_normals/`:
652 of its 1055 sprites have one, and the rest, which had no `_d` depth file, are skipped.
Windows only; nothing appears on screen.

The sources in `assets/Content` carry the same names, so why export? Because the atlas is what
the game draws: a sheet is whatever was in its folders when it was built, and `GhostedBuildings`
is greyed and blurred on the way in. Against the shipped content all 3077 sprites match their
sources in size, opaque pixels exactly; three have no source file at all (`whiteFace`,
`Construction`, `EmptyBG_Dark`). Half-transparent edges differ by a few levels, because the
pipeline premultiplied the alpha and that rounding cannot be undone.

### The menu background is no longer a video

`MainMenuIntro.uwanim` in the game root replaces MonoGame's `VideoPlayer` and the shipped
`Content/MainMenu/TauCetiMainMenu.wmv` playback. It is the same footage as a motion-JPEG frame
sequence (854×480, 12 fps, 4.7 MB — half the `.wmv`), decoded by the StbImageSharp already inside
MonoGame, so it needs no new dependency and **works on DesktopGL**, which never had a menu
animation at all because MonoGame has no GL `VideoPlayer`.

`Content/` and the original `.wmv` are untouched. The file is optional: **delete it and the game
uses the still background image**, the same thing it does when the *PlayVideo* option is off.

**There is no video path left at all** — this is not a fallback arrangement. No assembly the port
ships references MonoGame's `VideoPlayer` or `VideoReader`, and
`tools/build/81-verify-no-videoplayer.sh` fails the build if one ever does:

```sh
sh tools/build/81-verify-no-videoplayer.sh    # VideoPlayer-independent: 17 assemblies clean
```

(`MediaPlayer` and `Song` *do* remain — that is the WMA music, which still uses MediaFoundation
on Windows. Only video is gone.)

Rebuild it at a different size or quality from your own copy of the video:

```sh
UW_ANIM_WIDTH=1280 UW_ANIM_HEIGHT=720 UW_ANIM_FPS=15 UW_ANIM_Q=3 \
  sh tools/build/33-make-menu-animation.sh <game>/Content/MainMenu/TauCetiMainMenu.wmv \
                                     <game>/MainMenuIntro.uwanim
```

Needs `ffmpeg` (`winget install Gyan.FFmpeg`). The container is eight bytes of magic, a small
header and the JPEG frames back to back — the layout is documented at the top of that script and
parsed by `UWGame.Port.MenuAnimation`, so writing your own generator is easy. `tools/ContentProbe`
validates one, including decoding frames, so a bad file is caught without launching the game.

`decomp/` is the decompiled baseline exactly as `ilspycmd` produced it. `src/` started as a
copy of it. So `diff decomp/<Asm> src/<Proj>` shows the complete set of hand edits — which is
how you can audit every change the port makes. Don't edit `decomp/`.

## Two things that will bite you

**1. Don't rename these assemblies.** XNB content files name their readers by
assembly-qualified name, and MonoGame only normalizes its own:

- 6 XNBs resolve `SpriteSheetRuntime.SpriteSheet, SpriteSheetRuntime`
- all 47 model XNBs resolve
  `Xclna.Xna.Animation.Content.AnimationReader, Xclna.Xna.Animationx86`

Change either assembly's **name** and those 53 assets stop loading. (The *version* is fine —
MonoGame's `PrepareType` strips it.)

The `x86` in `Xclna.Xna.Animationx86` is upstream naming, not bitness — the assembly is pure
AnyCPU IL with no native code, and the game runs as x64.

**2. The shipped effects cannot be used as they are.** They are MGFX **v8** (MonoGame 3.6) and
DirectX-profile. MonoGame 3.8.x requires **v10** — *"This MGFX effect is for an older release of
MonoGame"* otherwise — and this build is DesktopGL, where a DirectX-profile effect throws
*"This MGFX effect was built for a different platform!"* whatever its version.

Both are moot here, because the effects are not converted at all: they are **compiled** from the
studio's HLSL in `assets/effects/` by `tools/build/34-build-gl-effects-shadowdusk.sh`, which
emits a v10 OpenGL-profile container directly. Your own `Content/` is never touched.

## The simulation runs in fixed steps — `port.simulationRate`

The studio switched XNA's fixed time step off ("LARS: I get atrocious framerates without these
settings!") and kept vsync, so `Sim.Update` ran once per monitor refresh. Most of the simulation
multiplies by the elapsed time and does not care. Some of it counts *updates* instead, and that ran
2.4 times as fast on a 144 Hz screen as on a 60 Hz one: four colonists jammed in a creek for good
because `Locomotor.ApplyPushVector` pushed 0.3 px an update. An audit found the rest — a dead update
at every waypoint and every subgoal change (`GoalTraverseEdgeBetweenWaypointsAtomic`,
`CompositeGoal`), the weather's cloud and wind lerp (`WeatherManager`), the head-turn ease
(`Intelligence`), a thrown projectile's hit test once per update (`BallisticLocomotor`), regulators
rounding up to whole updates (`Regulator`, `SleepyUpdater`), and the time-sliced AI's nine
milliseconds of CPU per update (`CycleManager`).

Rather than rescale each, the simulation now advances in **fixed steps of game time**
(`UWGame.Port.FixedSimStep`, `Sim.RunFrame`; PORT DEVIATION 22). Drawing, input and the interface
still run once per frame. PORT's **FIXED SIMULATION RATE** in SELECT MODS sets it:

| value | |
|---|---|
| `60` (default) | the studio's own figure — "1 frame, or update, takes 0.016 s" (`AIConstants`), `CalculateProgressDelta(0.0167, ...)` (`ProcessJob`), the 1/60 s taken off every `Component` regulator. A 60 Hz player sees the shipped game step for step |
| `30` | half the simulation's CPU |
| `OFF` | one update per drawn frame, as the studio shipped it |

- **Speed and pause.** Twice and four times speed run two and four times the steps; paused, none.
  The same save at 30, 60, 120 or 144 frames a second, at any speed, with or without pauses,
  reaches the same state to the bit (`dataexport --save-steprate`, gate 80 case 32d).
- **The cap.** A frame runs at most 8 steps. A machine that cannot keep up — or the first frame
  after a long load — runs the game slower instead of owing it time.
- **The time-sliced AI** (`CycleManager`) keeps the studio's nine milliseconds *per frame*: a frame
  with several steps splits it between them, so a frame costs what it did.
- **Drawing.** Movers are drawn between their last two simulated positions
  (`RenderAsModel.LocationBetweenSteps`), one step behind the simulation, so they move smoothly on
  a 144 Hz screen and the walk animation, which reads speed from drawn movement, does not flicker.
- **Replays** record the rate and play back at it whatever the setting (`CycleSchedule`'s first
  line); a replay recorded before this is played at `OFF`, as it was made.
- **Saves** are unaffected: nothing about the step is saved, and a game loads with nothing owed.

For a mod: code called from `Sim` (entities, goals, systems) runs once per step, and
`The.Sim.GameTime.ElapsedGameTime` there is the step's length. Code in the client runs once per
drawn frame, and sees the frame's time. A constant you add per update is now per 1/60 s — but
multiply by the elapsed time anyway, so it also holds with the setting `OFF`.

## SCOUT and EXAMINE end

*"Currently SCOUT and EXAMINE commands are endless, is there reason for them to be endless?"*
(2026-10-19). No: by the studio's design an order is one sweep of its zone. `GoalScouting` walks to
the zone's edge and hands over to `GoalSearchArea`, which samples points over the zone
(`UniformPoissonDiskSampler`, 100 px apart), visits the nearest each time, drops those it cannot
reach (`RegionMap.Result.NoAccess`), and when none are left `GoalScouting` destroys the job, which
clears the zone's `ScoutingJob` / `ExamineJob`. But the points lived on the **goal**: anything that
ended it — a meal, sleep, nightfall, a better job — threw them away, and the next `GoalSearchArea`
sampled the zone again and started from nothing. A zone larger than one uninterrupted stretch of
work was never finished. A sweep that visited nothing itself also failed rather than ending, so an
order whose points were all unreachable was taken, and failed, for ever.

The port keeps the sweep on the order (`ScoutingJob.SweepPointsLeft`, `SweepPointsVisited`; core
bug fix, marked `PORT:`):

- the first sweep samples the zone **once**, for the job; every later sweep — after an
  interruption, by another colonist, after loading — continues from what is left
- a sweep takes a point out of the job's list when it sets off for it (`GoalSearchArea.holdsPoint`),
  so two colonists on one order never walk to the same point; the point is counted on arrival,
  **dropped** if the walk there fails, and **handed back** if the sweep stops on the way
  (`GoalSearchArea.Deactivate`)
- an unreachable point is dropped from the order, not a reason to start again; when the list is
  empty the order ends through the studio's own path (`GoalScouting` → `DestroyJobAndRemoveLocks`)
- **saves**: `ScoutingJob` and `GoalSearchArea` are written at `Snapshotter.Version.ScoutingSweep`
  with the points left, the count visited, the job a sweep works for and the point it holds. An
  order from an older save reads with no sweep of its own and its next sweep samples the zone
  afresh, once; an older sweep already under way goes on with its own points, as before

Unchanged: a single-location `ScoutingJob` (`Location`), EXAMINE still examines and SCOUT still
only scouts (`GoalSearchArea.GetDetectResourcesFactor`), and patrols and `GoalFindPrey` — which
pass no job — sweep exactly as the studio's did. One studio quirk is left alone:
`ComputeLocationsToVisit` removes points outside an irregular zone without stepping back, so it
keeps a few just outside it, which are visited like the rest.

`dataexport --save-scout=<file.sav>` (gate 80 case 32e) orders a SCOUT and an EXAMINE zone on a
save through the game's commands, interrupts each sweeper twice, puts a second colonist on each
order, adds a point nobody can reach, and checks that each zone is sampled once and both orders
end; then saves mid-sweep and checks the save reads back the same points and ends too.

## Where the mod hooks are

The game looks for ~80 base-data XML files under `data/BaseData`, `data/Scenarios`,
`user/Mods`, `user/Scenarios` and `user/Maps` — `entityTypes.xml`, `processTypes.xml`,
`GameParams.xml`, `particleSystems.xml` and friends. **None of them ship**: a retail
`data/BaseData` contains only `Strings/English (US).xml`, and the base data is hard-coded in IL.

The reason is smaller than it looks. The game contains a complete exporter for all of it,
driven by `Sim.CurrentSerializeMode`, and that field is initialised to `NoSerialize` and
**never assigned anywhere in the shipped game**. The tables were always meant to round-trip
through `data/`; the switch was just never wired to anything.

It is wired up now:

```
UnclaimedWorld.exe --export-data      write the built-in tables to data/BaseData/*.xml,
                                      then load from them
UnclaimedWorld.exe --data-from-xml    load from data/ only, ignoring the built-in defaults
UnclaimedWorld.exe --help
```

Data loading with no flags is exactly as retail: built-in tables, nothing under `data/`
written or read. So those two flags are opt-in and cannot surprise a normal player.

## Writing a mod: HarmonyLib

The port ships **HarmonyLib 2.4.2** and loads mods from `user/Mods/*.dll` at startup. A mod is a
class library with Harmony patch classes in it — no plugin base class, no manifest, no injector.

```csharp
[HarmonyPatch(typeof(ProcessLoader), "InitProcessTypes")]
internal static class FasterCharcoalPatch
{
    private static void Postfix(ref List<ProcessType> __result)
    {
        foreach (ProcessType p in __result)
            if (p?.KeyName == "makeCharcoal")
                p.WorkOrTimeNeeded.DaysNeeded *= 0.5f;
    }
}
```

That is a complete, working mod — the `using` directives and the `net8.0-windows` target are the
only other things it needs. `samples/FasterCharcoalMod/` is exactly this, with a `.csproj` set up
the way yours should be, so **copy that directory** rather than writing one from scratch.

```sh
dotnet build samples/FasterCharcoalMod/FasterCharcoalMod.csproj -c Release \
  -p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Unclaimed World"
```

Then put the single output DLL in `user/Mods/` **in the game folder** — beside
`UnclaimedWorld.exe`, *not* under `Documents\Unclaimed World\` (that is where saves and replays
go, and a mod there is invisible). **You will have to create the folder**; the game neither ships
nor creates an empty `user/Mods`, so its absence is not a fault. Nothing but mod DLLs goes in it.

Strictly, `user/Mods` resolves against the process's **working directory**
(`Config.GetDataFolderPath` → `Directory.GetCurrentDirectory()`), which is the game folder for a
normal Steam or double-click launch. If you run the game from a shortcut with a different *Start
in*, or from a terminal in another directory, that is where it will look — and it is the usual
explanation for a mod that silently does not load.

**Reference the game's DLLs with `<Private>false</Private>`** — `UnclaimedWorld.dll`,
`MonoGame.Framework.dll`, `0Harmony.dll` — and ship **only your own DLL**. In particular do not
ship `0Harmony.dll`: the game provides exactly one copy, and two Harmony assemblies in one
process keep separate patch state over the same methods, which fails in ways that are very hard
to diagnose. The loader refuses a `0Harmony.dll` found in `user/Mods` and tells you to delete it.

Harmony ignores accessibility, so the whole game is reachable — private methods by name string,
private fields via `___fieldName` parameters, return values via `__result`, the instance via
`__instance`. See the [Harmony docs](https://harmony.pardeike.net/).

**When mods load.** Before the game object is constructed, in filename order — so patches on the
data loaders (`ItemLoader.Init`, `ProcessLoader.InitProcessTypes`, `StructureLoader.Init`) shape
the tables on the first scenario load. Where two mods patch the same method, Harmony's priority
attributes decide; the loader does not order them beyond the filename.

**Failure is per-mod.** A mod that throws while loading or patching is skipped, named in
`Errors.txt`, and the game starts without it. That is the normal outcome after a game update
whose patch targets moved, so the message names the mod — you can tell which one to remove.

`-nomods` disables the loader and the bundled mod below, and holds every other mod's
setting at its stock value for that session (`ModSettings.StockOnly`). `user/ModSettings.xml`
keeps your own values, so the next start without it has them back.

### Testing a mod without launching

`tools/DataExport` runs the loader and the data loader headlessly, so a data mod's effect is
visible in a second:

```sh
dataexport <game-dir>            # loads user/Mods, exports the tables the game would run with
dataexport <game-dir> --nomods   # stock tables, for diffing
```

`tools/build/80-verify-modloader.sh` is the loader's own regression test and a worked example of this
loop: it builds the sample mod, asserts `makeCharcoal`'s `DaysNeeded` actually halved in the
exported XML, and checks the three failure cases (`-nomods` suppression, a corrupt DLL not
stopping later mods, a stray `0Harmony.dll` refused).

### Listing your mod in SELECT MODS

The main menu's MODDING → SELECT MODS lists every mod: the ones that ship with the game and every
DLL in `user/Mods`, loaded, failed or switched off. Each has a switch, and unfolds to a picture, an
author and a description. A DLL is listed under its file name; to say more, put two files beside
it with the same name:

```
user/Mods/MyMod.dll
user/Mods/MyMod.xml    <Mod><Name>MY MOD</Name><Author>You</Author><Description>What it changes.</Description></Mod>
user/Mods/MyMod.png    its picture, about 100 x 100
```

Both are optional. Switching a DLL off there lists it in `user/ModSettings.xml` as
`<DisabledMod id="dll:MyMod"/>`, and the loader skips it from the next start. If your mod
registers settings with its file name as the mod id (`ModSettings.Toggle("MyMod", ...)`), they
appear in the same entry, under its MOD OPTIONS (`ModSettingRows`), key settings included, as a key
button. That is the only place a mod's settings are shown: OPTIONS keeps the studio's own
settings, the port's LANGUAGE (`ModCatalog.InOptions`), the studio's own keys and a MOD OPTIONS...
button that opens this list over it - in game titled MOD OPTIONS, without the switches. The mods that ship with the game describe themselves with
`ModSettings.Describe(modId, author, description, thumbnail)`, where the thumbnail is a sprite of
the game's own (`HUD_thumbnail_cookhouse`) or a PNG's path. A built-in mod switched off has every
setting read as the studio's game until it is switched on again, and gets its own values back then.

## What was the Unhidden Mod

A community mod, contributed as BepInEx / HarmonyLib patches and applied here directly in source.
It shipped as one mod until 2026-10-11, when Kastuk asked for it to be dissolved — "standing
alongside of all other mods that Unhidden one make just confusion". It is now:

| where | what |
|---|---|
| core | the scenario picker lists user scenarios from `user/Scenarios` (`SelectScenarioPanel.AllScenarioHeaders`); saves sort newest first; stockpile categories sort ascending; the mouse wheel scrolls data sheets |
| `PortSettings` | `port.culture` (DISPLAY CULTURE) and `port.shadowsKey` (`O` toggles shadows) |
| `PeatMod` (PEAT) | `peat.charcoalFromPeat`, `peat.peatBuilding`, and the charcoal / smithy fuel tag |
| `RawhideStringMod` (PLANT FIBRE STRING) | `rawhidestring.stringAlternatives`, `rawhidestring.plantFibreString` (added 2026-10-18) |
| `RespawnCapMod` (RESOURCE RESPAWN CAP) | `respawncap.capResourceRespawn` |
| `MainMenuMod` (MAIN MENU) | `mainmenu.mapEditorButtons`, `mainmenu.revealTestMap` |

Each of the four mods has its own entry in SELECT MODS. A settings file or a save stamp written
before the split names the old `unhidden.*` ids; `ModSettings.LegacyIds` reads them as the new
ones, and a file is written back with the new ids. `-nomods` holds every mod at its stock values.

```
UnclaimedWorld.exe -nomods            stock game, every mod at its stock values
```

**Interface** — the core part has no effect on simulation state. MODDING's MAP EDITOR and TEST
SCENE (MainMenuMod) call the studio's `MainMenuScreen.EditMap` / `TestMap`, fully implemented and
never given a button; MODDING itself is the port's, `ModdingDialog`. `Config.Culture` is a
**choice** (`port.culture`) and defaults to the studio's own `InvariantCulture`; `en-GB`, which the
contributed patch imposed on every player, is the other setting. The studio assigns invariant in
`Config`'s static constructor and `GameStateManagement.UnclaimedWorld` pushes it onto
`Thread.CurrentThread.CurrentCulture`, so it governs every number and date formatted or parsed
without an explicit provider — a deliberate decision, and one to offer rather than overwrite.

**Simulation content** (PeatMod) — changes balance, and **changes what a save contains**.
`item:charcoal` is redefined to carry a `fuelForForge` *tag* instead of being matched by item key,
both smithies are redefined to require the tag rather than the literal item, and
`makeCharcoalFromPeat` is added — all with `peat.charcoalFromPeat` on, in `user/ModSettings.xml`
or under PEAT's MOD OPTIONS in SELECT MODS.

### Charcoal from peat

| recipe | in | out | time |
|---|---|---|---|
| `makeCharcoal` (stock) | 1 firewood | 1 charcoal | 1/30 day |
| `makeCharcoalFromPeat` | 2 dry peat | 1 **charcoal** | 1/15 day |

It produces the **real** item rather than a peat-flavoured one of its own, and that is the whole
design. Much of the game matches charcoal by **key** rather than by tag: gunpowder takes
`item:charcoal` as a literal input, and `PricesProfileLoader` and `TradeGroupLoader` price and
trade `item:charcoal` while knowing nothing about anything else. An earlier version of this mod
added `item:peatCharcoal` as a second forge fuel with its own `makePeatCharcoal` recipe; it burned
in a forge, because of the retagging, but could never be turned into gunpowder or sold — and, worse,
it put a **new EntityType key** into saves, which a build without the mod cannot resolve at all.
Producing the real item closes all of that at once, so the separate item and its recipe were
**removed** rather than kept alongside. `tools/build/80-verify-modloader.sh` asserts they are gone, from
the tables and from the assembly.

It costs **two** dry peat where the firewood recipe costs one log, and the reason is not the work
upstream — dry peat costs plenty of that already, gathered WET four at a time from a bank and then
dried ten at a time through a peat stack with its own tool set. It is that **a peat bank does not
run out**. Firewood is limited by how fast trees grow back, so a colony's charcoal is capped by its
forest; peat is capped by nothing, which makes it the route for *stable* production rather than the
efficient one. Paying twice the input is what keeps it from being simply better, and 1/15 day
against firewood's 1/30 keeps it slower as well.

The full peat chain, all of it stock except the last step: peat bank → `makeWetPeatFromBank` →
`item:wetPeat` → `makeDryPeat` → `item:dryPeat` → `makeCharcoalFromPeat` → `item:charcoal`.

### Building with peat

`peat.peatBuilding` adds a peat-fuelled variant of the three stock recipes that hand a
structure its first load of fuel — `constructCampfire`, `constructImprovisedKitchen`,
`constructMudbrickKitchen`. In all three the firewood is not a *material*: it is consumed and
re-created as a waste product, which is how the game says "the structure starts with one fuel in
it". All three burn the `fuelForCampfire` tag, and `item:dryPeat` carries that tag with the same
`MaximumBulk` as firewood — so a peat-loaded campfire burns exactly as long, and the variant is a
fair swap rather than a discount. That is why it is 1 peat and not 2, where the charcoal recipe
charges two: there the peat is consumed and something else comes out; here the input *is* the
output.

The variants are **cloned** from the studio's recipes at load time and only the fuel slot is
swapped, so the tools, skill, work time, stances and animations stay whatever the studio's are.

One port change makes them reachable. `Structure.StartBuildingJob` picked
`ProcessYieldsThisOutput[structure][0]` — the first recipe registered, whatever the colony had in
store. No stock structure has a second recipe, so nothing ever noticed; but it means an
alternative recipe can never be used, because the job is ordered against the firewood recipe and
the haulers wait for firewood while the peat sits in the stockpile. It now takes the first
candidate whose inputs the owner can actually supply, falling back to the first when it can supply
none — which is the old behaviour of ordering the build and waiting. **With one candidate it is
the old behaviour exactly**, so the stock game is untouched, and any mod that adds an alternative
recipe for a structure now works.

### Fixes and alternatives from the task list

Four more switches, all defaulting on, all in `user/ModSettings.xml` and SELECT MODS, each in
the mod named by its prefix:

| setting | what it does |
|---|---|
| `rawhidestring.stringAlternatives` | `makeFishingNetFromPlantFibreString` — a net from 2 plant fibre strings, taking 1.5 times the stock recipe's time; needs `rawhidestring.plantFibreString` too. Stock `makeFishingNet` names `item:cottonString` by key. Until 2026-10-17 the variant took rawhide string (useless in water, Kastuk), until 2026-10-18 2 raw vines |
| `rawhidestring.plantFibreString` | a new item, `item:plantFibreString` ("String (plant fibre)"): the studio's rawhide string as a tool (tags `lightString`, `cordage`, a hand tool) with durability 0.6 where rawhide has 1. Made by `makePlantFibreStringFromVine` (2 vines) and `makePlantFibreStringFromShadeleafCanes` (1 shadeleaf cane), both cloned from `makeRawhideString` - bushcraft, `toolSetButcherFlesh` - at 1.5 times its time. Every list of tool alternatives naming `item:rawhideString` (12 in the studio's tables; none takes a string by tag) takes it beside rawhide at 0.8 of rawhide's productivity (`RawhideStringMod.AddToolAlternatives`). The **item** is registered whatever the switch, so a save holding one loads with it off; the recipes and the tool alternatives follow the switch, and a save is stamped with it. The fishing net is made from it (above) |
| `respawncap.capResourceRespawn` | tops a replenishing resource tile back up to its high-water mark instead of adding that much *again* on top. `ResourceReplenish.ReplenishResourceItems` computes exactly this clamp and drops the result — `Common.Clamp` is a pure function called as a statement — so a tile with `FractionOfMaximumToReplenishEachTime = 1.0` (torux shellfish among others) roughly **doubles** every time it has been harvested, and the maximum is a ratchet that records the larger count |
| `peat.peatBuilding` | the peat-fuelled build variants, above |

The respawn one is a gameplay fix, so it is in the mod rather than in `patches/` — the port's own
patches leave gameplay alone. It takes effect immediately and no save can tell whether it was on.

## Disassembly, generated from the recipes — `disassembly.generate`

Asked for by Kastuk: *"there is action to Disassemble the object, but its rarely available (just
for technical things, like rifles). How can you add Disassembly of the metal tools, like to recover
metal material?"* It is rare for a dull reason — **every disassembly in the game is hand-written**.
An item gets one by naming a process in `NonLivingType.SalvageProcess`, which
`NonLivingType.PostLoadContentInitialize` resolves against `GameData.AllProcessTypes`, and that
process is an ordinary `ProcessType` carrying `IsSalvageProcess`. Exactly **33 items** declare one:
the guns, the sentries, the workshop upgrades, three toolboxes, two tapping buckets, beds and mats.
No knife, no machete, no hammer.

`UWGame/Mods/DisassemblyMod.cs` writes no recipe down. `DisassemblyMod.AddDisassembly` reads the
recipe that **produces** an item and turns it round — `makeHammer` is 1 wrought iron + 1 sticks, so
a hammer comes apart into 1 wrought iron + 1 sticks — which means nothing is typed twice and the
disassembly follows the studio's numbers when they change. It runs at the end of
`BaseDataLoader.InitProcessTypes`, the same hook PeatMod's recipes use, and hangs each
generated recipe on its item through the studio's own `NonLivingType.SalvageProcess` field, so the
DISASSEMBLE button appears by the studio's own path (`EntityType.CanBeSalvagedDirectly`, read by
`HUDEntityContextMenu.RefreshEntityContent`).

**What counts as "complex", and why it is not a list.** The tables answer it: an item whose
production consumed **two or more materials** was assembled; an item whose production consumed one
was transformed. A hammer is iron bolted to a handle; a file is one piece of iron beaten into
shape, and handing the iron back would be melting it down rather than taking it apart. So
one-material recipes are left alone — unless the exception table says what the material comes back
**as**.

| rule | meaning | example |
|---|---|---|
| default | give back what the recipe consumed | `item:steelMachete` → 1 blister steel + 1 sticks |
| *degrades to* | a material comes back as something lesser, per **material** | `item:cotton` → `item:cottonString`, which is what lets `item:textile` come apart |
| *destroyed* | burned, fired or dissolved, and does not come back | clay, salt, the fuels, the unfired precursors |

That last row is what keeps a glazed clay jar out of it: `makeGlazedClayJar` has two inputs, so the
complexity rule alone would have "disassembled" a fired jar into its salt glaze and its unfired
self.

Twenty-one recipes are generated from the stock tables, including the three **complex spears**
(`item:improvisedFlintSpear`, `item:improvisedGoodSpear`, `item:ironSpear`) and Kastuk's textile.
Three deliberate gaps are worth knowing:

* An item the studio already gave a disassembly keeps **theirs**. A generated recipe is a fallback
  for what nobody wrote, never a replacement.
* `item:steelKnife` gets none. `makeSteelKnife` yields **two** knives for 1 blister steel + 1
  sticks, so one knife is half of each; the amounts are divided by the batch and rounded **down**,
  because rounding up would let a colony make two knives from one steel and take back two steels.
  The salvage machinery takes one object at a time (`Salvage.CreateSalvageJob` assigns exactly one
  as the job's immovable input), so "disassemble two at once" is not available to fix it.
* **No new item.** The wooden half of a tool is `item:sticks` — literally what the recipe consumed,
  and an input to about 45 recipes. A new EntityType key is the one thing that makes a save
  unloadable without the mod, which is why `item:peatCharcoal` was deleted.

**Where this departs from the studio's model, and what keeps it honest.** Their design makes a
salvage recipe return the item's *declared parts*: `NonLivingType.Parts` is documented as what the
item is composed of, noting that "production may also consume other materials though", and
`ProcessType.PostDataCompleteValidate` requires every non-waste output of a salvage process to be
one of those parts. This mod returns the **materials** instead, because the tools it covers declare
no parts at all — and declaring parts for them is not a free fix: parts are instantiated as child
entities (`NonLivingEntity`) and drive repair and condition, so it would change gameplay and what
saves contain.

So the rule is relaxed rather than met, and two checks in `DisassemblyMod` stand in for it:

* `AgreesWithDeclaredParts` — an item that *does* declare `PartKeys` is governed by the studio's
  rule, not this one. Unless the recovery is a subset of its declared parts, no recipe is generated.
  Six stock items declare parts without a disassembly (the three arrows, the wheel, the spike trap,
  the power unit) and none is in a category this mod considers — but a scenario or another mod can
  put one there, and without this the validator would throw on the loading screen.
* `ReturnsNoMoreThanWasConsumed` — never hand back more of a material than making one consumed,
  counting a degraded material against the material it came from. That is the invariant the
  studio's rule was protecting, restated in terms this generator can satisfy.

Both refusals are reported by `dataexport --disassembly` rather than being silent, and case 12 of
the verify script pins that no stock item trips either.

Two port fixes come with it, and they are the same bug in two places: the studio reads the
salvaged item's `Parts` list without checking it for null. All 33 items they gave a disassembly to
also declare their `PartKeys`, so neither can fire in the stock game, and both fire on the first
generated recipe.

* `ProcessType.GetIsSalvageWithoutWaste` now answers `false` for an item with no parts list -
  callers only choose a caption by it, PACKING DOWN against BEGIN.
* `ProcessType.PostDataCompleteValidate` walked that list to check each output against it. This one
  was fatal: the tables built correctly and the game died on the next screen, in the validation
  pass `Sim.QueueGameDataAndSimInit` runs. The check is skipped when there is no parts list, which
  is the honest answer - the rule asks whether the outputs match the declared parts, and an item
  with none is outside the rule rather than failing it.

That second one shipped for a day before Kastuk hit it, and the gate has been extended so it cannot
recur: `dataexport --disassembly` now runs `GameData.PostDataCompleteInitialize` - the pass that
crashed - before it prints anything, because building the tables and surviving them are different
things.

Checking it without launching: `dataexport <game-dir> --disassembly` loads the tables the way the
*game* does and prints one line per generated recipe, with what each gives back and whether the
item actually points at it. (It loads without exporting on purpose: the recipes are computed from
the entity table, and `entityTypes.xml` is one of the 7 that cannot serialize — in an exporting
run the entity table dies half-built.) `tools/build/80-verify-modloader.sh` case 12 asserts against that
output, comparing each recovery against the production recipe it came from rather than against
numbers written in the test.

## Seasons and temperature — `weather.enabled`

Kastuk, "Weather and temperature" (2026-10-17): *"rainstorms during winter time with cold winds
between them. Not freezing temperatures, but still cold enough for sickness. Then high solar heat
during usually hot summer. Can take references from subtropic seasons."* This is step 1, the
seasons and the air; rain and wind (step 2), what the cold does to people (steps 3 and 4) and what
rain and wind do to things drying (step 5) are the three switches below. Off by default.

| hook | what changes with the switch on |
|---|---|
| `DateAndTime.ComputeSunAndLight` → `WeatherMod.Declination` | the studio's declination is `axialTilt * (-1 + 2 * TimeOfYear)`, a sawtooth: highest on "Mid winter", lowest the next day. The mod's is `axialTilt * cos(2π(TimeOfYear - 5.5/12))`, highest mid "Mid summer" |
| `NonLivingEntity.ComputeDegradeDamage` → `WeatherMod.OutsideTemperature` | a tile at the studio's 288 K (`Constants.MeanAmbientTemperature`, "15 C - replace this!") spoils things at the air temperature; storage that derives from it ("isolated") follows; a tile set to anything else (the studio's `DecompositionTest`) keeps its own |
| `DegradeType.PostDataCompleteInitialize` → `WeatherMod.ShelfLifeTemperature` | the shelf-life tooltips are worked out for the mid-summer day's average (~23.5 C) instead of 15 C, from the next game loaded |
| `InGameInterface.SetTimeAndDate` → `WeatherMod.ShowTemperature` | the date panel's WEATHER NOW line shows the air temperature through the studio's `HUDDateAndWeather.SetTemp`, which nothing called |

The air is `NightCelsius + NightSeasonalSwing * cos(season) + DaySwing * sunshine`, sunshine being
the studio's `SunIntensity` formula and `DaySwing` the studio's 20 degrees from
`WeatherManager.globalTemperature`. The constants are at the top of `mods/WeatherMod.cs`. Nothing
new is saved and no tile is written: the temperature is computed from `TimeOfYear`, the sun's
elevation and the cloud cover, which a save already holds. `dataexport --weather-selftest` (case 42)
steps the real `DateAndTime` through a year and spoils a food through the real
`ComputeDegradeDamage`.

### Rain and wind — `weather.rain`

Step 2, a second switch in the WEATHER category that works with SEASONS AND TEMPERATURE on; off by
default. It is separate because it replaces the studio's random cloud cover, and with it the
sunshine that dries and spoils things. The studio's `WeatherManager` rolls a new cloud target (1 in
80 every 2 s, any value equally likely) and a new wind direction (1 in 50); its wind speed is a
fixed 1.5 m/s; there is no rain anywhere in the game.

The weather is a schedule, a function of the game's random seed (the gameplay `RandomGenerator`'s
`RandomSeed`, saved with every game and reused by replays) and of the clock, `(Year + TimeOfYear) * 12`
days. Nothing is drawn from the random stream, so the rest of the game's random numbers are what
they were, and the weather can be looked ahead at. Each calendar day rolls one rain spell with a
chance that follows the season (`WeatherMod.RainChance`): about 83% of winter days, 28% at the
equinoxes, 4% in summer (ten seeds of thirty years). A winter spell is a rainstorm of up to 12
hours, a summer one a shower of 2–4. The wind follows the season — 1–4.6 m/s in summer, about
8 m/s ("Fresh breeze") in winter between the storms — and a storm adds up to 10 m/s, to "Gale" or
"Strong gale", never the studio's "Storm".

| hook | what changes with the switch on |
|---|---|
| `WeatherManager.Update` → `WeatherMod.SteerCloudCover`, `SteerWindSpeed` | the cloud cover (and its target) and the wind speed are the schedule's; the studio's random rolls still happen. Off, the wind is the studio's 1.5 m/s |
| `WeatherManager.UpdateWeatherDisplay` → `WeatherMod.SkyTerm` | WEATHER NOW says "Light rain", "Rain" or "Rainstorm" instead of the cloud term while it rains |
| `WeatherMod.CurrentAirTemperature` (step 1's hooks) | rain cools the air up to 3 C and wind 0.12 C a m/s above 3 m/s — a winter storm 3–5 C colder — never below 2 C |
| `InGameInterface.SetTimeAndDate` → `WeatherMod.ShowTemperature` → `HUDDateAndWeather.SetWeatherAhead`, `SetTempAhead` | WEATHER AHEAD (the studio's `SetWeatherAhead`, which nothing called) shows tomorrow, midnight to midnight: the wettest it gets or the daytime cloud, the strongest wind and the high |

Cloud cuts the studio's `SunIntensity`, so a rainy day is cooler and things dry and spoil less in
the sun; the cloud shadows follow it, and the wind blows the cloud shadows and the smoke the studio
lets the wind push. **Saves:** nothing new. `WeatherManager`'s own saved cloud cover, target and
wind speed hold the schedule's values, in the fields and format they always had, and on load the
schedule is worked out again from the seed and the date. **Not here:** no rain is drawn or heard —
the game has no rain particle, texture or sound, its particles are puffs drawn with the map tile
their emitter stands on, and rain would need a streak texture (a new binary asset) or a particle
drawn stretched along its velocity, plus an emitter that follows the camera. `--weather-selftest`
checks the schedule, the climate over ten seeds, the real `WeatherManager.Update` through a winter
storm and the forecast against the day that comes.

### Cold and wet hurt — `weather.exposure`

Step 3, a third switch in the WEATHER category: it works with SEASONS AND TEMPERATURE on, and brings
in the rain and the wind when RAIN AND WIND is on too. Off by default. *"Not freezing temperatures,
but still cold enough for sickness."* The studio decides only how well a place lets a colonist sleep
(`GoalDoSleep.ComputeSleepConditions`: the open, a prepared campfire, a residence) and
`EvaluateSleep.ScoreComfort` prefers huts by that; nothing about the weather. Its
`PhysicalEffects.LimitForIncreasedSickness` / `SicknessFactor` are read nowhere, and describe
sickness from an unmet need, not the weather, so they are left alone.

**Exposure** (`WeatherMod.ExposureOf`, `NightExposure`): an hour in the open is
`cold × (0.5 + 0.3 × rain + 0.2 × wind)`, with `cold = (12 C − air) / 10` clamped to 0–1 (full at the
2 C floor) and `wind` 0 at 3 m/s, 1 at 15 m/s. The night is the next 8 hours from lying down, from the
schedule — the night that comes, with no random draw; without RAIN AND WIND it is dry and still. A
prepared campfire lets half of it through, a residence (or any other container, which the studio
sleeps like a house) none. Thirty years of one seed, lying down at 21:00: the stormiest mid-winter
night 0.95 in the open, 0.47 by a fire; dry winter nights 0.30–0.48 (0.26–0.34 without rain and
wind); "Spring" and "Mid autumn" mostly below 0.25; every summer night 0.

| hook | what changes with the switch on |
|---|---|
| `GoalDoSleep.Activate` → `WeatherMod.OnSleepStart` | measures the night where the colonist lies (the worst of the day is kept in `CustomFields` `weather:exposureDay` / `weather:exposure`); once a day, at exposure 0.25 or more, rolls the game's seeded generator for a cold at `0.3 × exposure` — up to ~28% after a storm night out, ~14% by a fire, none in a hut and no draw then; not while already ill |
| `Intelligence.Comfort` → `WeatherMod.ExposurePenalty` | the next day's comfort is `0.2 × exposure` lower (up to ~19% after a storm night out, ~9.5% by a fire), until the next day's first sleep, beside DiscomfortMod's |
| `EvaluateSleep.ScoreComfort` → `WeatherMod.ShelterScore` | (the hook passes where the sleeper would lie) a place's comfort score loses `0.5 × its exposure tonight`, under what covers them there (step 4): on the storm night the ground 0.32 (studio 0.61), a fire 0.50 (0.66), a hut 0.92 as before — at most ~4% of the sleep goal's score, so it reorders the places rather than delaying sleep |
| `BaseDataLoader.InitEffectTypes` / `InitEffectProfileTypes` → `WeatherMod.AddEffects`, `AddEffectProfiles` | the "Cold" effect profile, always registered: comfort −10% for 1.5 days, shown in the colonist's effect panel and saved with them |
| `Locomotor.CalculateSpeed`, `ProcessJob.CalculateWorkDurationInDays`, `BiologicalEntity.EnergyLevel` → `SicknessEffects` | with a cold, walking ×0.9, work time ×1.2, energy at most 0.75. `mods/SicknessEffects.cs` is shared with EmergencyEatingMod's "Bad stomach": each mod registers its profiles' factors; two at once multiply and the lower cap holds. They follow the effect, not the switch |

The numbers are constants at the `cold and wet` block of `mods/WeatherMod.cs`, documented as
tunable. **Saves:** no new format — three numbers in a colonist's `CustomFields` and an effect that
is registered either way. `--weather-selftest` checks the exposure of those nights by place, the
real `Intelligence.Comfort`, the real `EvaluateSleep.ScoreComfort`, the cold's rate over 4000 seeded
nights, and the cold (alone and with a bad stomach) through the real `Locomotor`, `ProcessJob` and
`BiologicalEntity`. **Not verified in a running colony:** whether colonists walk to a hut instead of
lying down by the fire is for a game.

#### What keeps it off — trees, PRECOL suits, clothes

Step 4, under the same switch. Kastuk: *"the trees can be a shelter too (living spoak, not cut),
especially live sanctuary tree with caps"*, *"Explorers with precol suits surely must be more
protected by default"*, and in "Colonist equipment": *"New clothes need to protect against weather
too ... Mackintosh to withstand the rain seasons."* Each is a `WeatherMod.Cover`: a factor on each of
the three shares of an hour's exposure — the dry cold (`0.5`), the rain (`0.3 × rain`), the wind
(`0.2 × wind`) — so leaves can keep off rain and leave the cold. Covers multiply with each other and
with a campfire's ×0.5; a residence is still 0 (`WeatherMod.CoverOf`, `Exposed`). With no tree, suit or
clothes, every number is step 3's to the bit (`WeatherMod.Night.Under`).

| cover | cold | rain | wind | how it is found |
|---|---|---|---|---|
| grown, living spoak, within 1.5 tiles | ×1 | ×0.6 | ×0.8 | `WeatherMod.TreeCover` / `CanopyOf`: the trees in `TerrainTile.TreesOnTile` of the 7 × 7 tiles round the sleeper; key `tree:spoak` (dead spoaks are the `tree:spoakdead…` types, a felled tree leaves its tile's list); `Tree.Size` ≥ 0.9 (below it the studio draws the young sprite). Its plates let "strong winds pass through the canopy" (TreeLoader) |
| grown, living sanctuary tree with its caps, within 2.5 tiles | ×0.9 | ×0.3 | ×0.5 | key `tree:sanctuarytree`; the caps are its summer canopy (`tree_sanctuarytree_1_grown_summer`). The studio blocks 45 units (~1 tile) round the trunk; the caps spread ~2.5× that |
| the same, bare (`Tree.InSeason` Winter, the studio's winter sprite) | ×1 | ×0.8 | ×0.7 | only the map editor sets a tree's season; several trees give the best of each share |
| grown clearleaf (`tree:rhubarb`), within 1.5 tiles | ×1 | ×0.5 | ×0.8 | Kastuk, 2026-10-18: "Clearleaf (Rhubarb) and Daysheen ... can give protection at mature states too"; `WeatherMod.CanopyOf` |
| grown daysheen, within 1.5 tiles, **by day only** | ×1 | ×0.5 | ×0.7 | "only during day, as it must fold the leaf spiral during night": `WeatherMod.IsDaylight` (`DateAndTime.SunElevation` > 0), so a sleeper lying down at night gets nothing from it, a drying rack under it does by day |
| PRECOL survival suit | ×0.4 | ×0.5 | ×0.5 | `WeatherMod.IsPrecolExplorer`: the loaded scenario's `EntityTypeDescription` names the person's type "PRECOL explorer" — Twinkler Island's `personDescription`, whose text issues each explorer a SURVIVAL SUIT against "heat and cold" that "collects and rinses water from the wearer". That table is never translated. The white suits are also among the model textures of the `…HumanAncestor` races, but that is a client-side pick shared with the Muckroot research team, so it is not used |
| hide cloak (HideGearMod) | ×0.8 | ×1 | ×0.7 | `WeatherMod.GarmentCover` → `HideGearMod.Wears`: in the Equipment storage when the colonist lies down (the studio's `GoalDoSleep` then drops everything beside them — "just drop everything where we sleep") |
| Mackintosh (HideGearMod) | ×0.8 | ×0.3 | ×0.6 | the same; with both, the Mackintosh |

The stormiest mid-winter night of the self-test, 0.95 in the open (dry cold 0.50, rain 0.25, wind
0.20): by a spoak 0.81, under a sanctuary tree's caps 0.62 (bare 0.84), in a hide cloak 0.79, in a
Mackintosh 0.59, a PRECOL explorer 0.42 (a cold 12.7% likely instead of 28.4%; 0.27 under the caps),
an explorer in a Mackintosh by a lit fire under the caps 0.09 — no cold. `EvaluateSleep.ScoreComfort`
scores a spot by its cover too: that night the bare ground 0.32, a spot under the caps 0.42 (studio
0.61). The core change is one argument: `EvaluateSleep.ScoreLocation` passes `worldLocation` to
`ScoreComfort`, which passes it to `WeatherMod.ShelterScore`.

**The Mackintosh** is HideGearMod's (it owns the cloak and its item keys): `item:mackintosh`, made by
`makeMackintosh` from 2 `item:textile` and 2 `item:marshcotSap` (Kastuk, 2026-10-18: "Truly mackintosh been made from cotton textile") — bushcraft, 1/15 day, on the
cloak's `toolSetCombineLightImprovisedObjects` (the studio's rubber, `makeGaskets`, is chemistry at a
polymer workshop; brushing sap on a cloak is not making rubber sheets). It is worn like the cloak —
long trips and at night beyond normal range, Normal at night where the cloak is Minor — with the
cloak's comfort under its own profile, "Waterproof clothing"; drawn with the studio's `tarp`
sprite. DisassemblyMod gives the textile back and no sap (`item:marshcotSap` joins its `Destroyed`
list). **Saves:** as for every HideGearMod item, a save holding one names `item:mackintosh`, so it is
stamped `hidegear.enabled=true` and loading it switches HIDE CLOTHING AND ARMOUR on; nothing else new
is saved. `--weather-selftest` checks every row above on a map of real `TerrainTile`s and tree
entities, through the real `OnSleepStart` and `ScoreComfort`; `--hidegear-selftest` (case 40) the
item, the recipe and its disassembly after `GameData.PostDataCompleteInitialize`. **Not verified in
a running colony:** whether colonists sleep under trees, and whether they still have a cloak on when
they lie down — gear is fetched for long trips and night work (`FindOptionalEquipmentIfNeeded` needs
a job, so going to bed fetches nothing).

### Rain and wind dry or wet — `weather.drying`

Step 5, a fourth switch in the WEATHER category: it works with RAIN AND WIND on. Off by default.
Kastuk, "Weather and temperature" (2026-10-18): *"Wonder how rain can affect processes at drying food
and hides ... No, just revert slowly. Then wind without rain will speed up the drying process. Way to
put drying structures under trees protection."* tripleacoder: *"SimProcess is the class that should
handle physical processes like that."*

**What the studio has.** A drying batch is a `SimProcess` that runs on its own once started
(`WorkerOnlyNeededToStart`: *"dries on its own"*, *"the hide is drying on the hide rack on its own"*)
on a stationary tool; every update (2 s) `EstimateProgressDelta` adds `interval / (DaysNeeded /
tool productivity)`, whatever the weather. The drying processes, read from the tables by
`WeatherMod.IsDryingProcess` (not `WorkerNeeded`, and a drying structure among the tools):

| process | on | a batch takes |
|---|---|---|
| `makeDriedThunderChicken`, `makeDriedStreakFin`, `makeTurnipSalami` | meat drying rack ×0.5 / drying shed ×0.8 / the cookhouse's drying shed addition ×0.9 | 4.8 / 3 / 2.7 hours |
| `makeDryPeat`, `makeFirewood` | peat stack, woodpile ×0.6 | 4 hours |
| `makeThunderChickenRawhide`, `makeWhipjawRawhide`, `makeMegapodRawhide` | hide rack (with the knife and brain of `toolSetCleanHide`) | by those tools |

| hook | what changes with the switch on |
|---|---|
| `SimProcess.EstimateProgressDelta` → `WeatherMod.DryingProgress` | after the studio's progress factors, before its cap. On an open rack or stack (meat drying rack, hide rack, peat stack, woodpile) not inside anything: while it rains, the batch goes **back** at `0.3 × rain × the normal rate`, never below 1% (at 0 the studio takes an output as not started) — an hour of rain 0.7 takes a rack's batch from 0.50 to 0.45 instead of on to 0.71, a 12-hour winter storm undoes about half a batch; dry, `× (1 + 0.05 per m/s above 3)`, at most ×1.5 (13 m/s) — a winter wind of 9 m/s ×1.3; still, dry air the studio's to the bit. A drying shed or the addition (*"will protect the meat from rain while letting the wind blow through"*) keeps the rain off — the studio's rate while it rains — and takes the wind. Under a living tree (`WeatherMod.TreeCover` at the structure, step 4) the rain is the cover's share of it (a sanctuary tree's caps ×0.3, a spoak ×0.6) and so is the wind's speed-up (×0.5, ×0.8) |
| `SimProcess.ComputeEstimatedCompletionTime` (`&& ProgressSpeed > 0f`) | a batch going back has no completion estimate instead of "now"; the studio's speed is never negative |
| `NonLivingEntity.Degrade` → `WeatherMod.SunDryingDamage` | the items whose degrade type is "sun drying" (`item:wetFirewood`, `item:wetMudBrick`, `item:cleanedUrsinix`), lying in no working storage: the same rate, but never below 0 — rain stops them drying rather than wetting them again (a negative damage would raise their condition and `MaxCondition`). The haulers' estimates (`HaulingJobManager`) and the tooltips are left alone |

**Saves:** nothing new — a batch's progress is its outputs' `Progress`, as it always was, and the
weather is the deterministic schedule. `--weather-selftest` steps a real `SimProcess` of
`makeDriedThunderChicken` through hours of the schedule with the real clock — on a rack in the open,
on a shed, on a rack indoors, under a sanctuary tree's caps and a spoak — and the sun-drying hook, and
finds both hooks in the IL. **Not verified in a running colony:** colonists' "Checking progress" visit
is timed by the completion estimate made at a batch's first update, so after rain it may come before
the batch is done; whether that costs much walking is for a game.

## Stink and noise — `discomfort.enabled`, `discomfort.wasteMap`, `discomfort.stinkOverlay`

Kastuk, "Discomfort" (2026-10-05): filthy items (all waste, fertilisers, sulphur, carcasses, fresh
hides) and busy workbenches, smithies, workshops, refineries and mines near where a colonist sleeps
cost Comfort, measured once a day at the first sleep. `GoalDoSleep.Activate` → `DiscomfortMod.OnSleepStart`
counts what is within the reach (5 tiles) of the sleeping place's exit; `Intelligence.Comfort` →
`DiscomfortMod.Penalty` takes off 3% per 5 units, at most 40%. Off by default; the numbers and the
details are in `mods/DiscomfortMod.cs`, and `dataexport --discomfort-selftest` is gate 80's case 34.

### Waste on the discomfort map — `discomfort.wasteMap`

Kastuk (2026-10-17): *"I see in source code the unused DiscomfortMap. I suppose, it's expected to
draw map of Dirty tiles with effect radius just like Threat map"*, and, told it would make colonists
route around a dirty camp, *"Must try it as option, even if it will make them reluctant to move waste
into compost collectors."* A second switch in the DISCOMFORT category, off by default.

`DiscomfortMap` is not unused: it is the sum of its child maps, and colonists flee by it, route by
it (`MovementMap` adds it to the terrain cost) and pick where to rest by it (`GoalTakeFive`). The
studio's note on it reads *"this map should have Fire and Threat as children... and maybe also a map
for general discomfort such as wetness, poison etc."* — but `FireMap` was never constructed, so
`ThreatMap` is its only child. The switch gives the player's colonist maps (Normal and Cautious
stance; Bold ignores danger, and animals have maps of their own) a second child, a waste map drawn
the way `ThreatMap.DoCycle` draws: every tile with filth — the items `DiscomfortMod.IsFilthy` names,
on the ground or inside what stands there, not what a colonist or animal carries — paints
`count × (reach + 1)` at its centre, `count` less per tile out (`Common.DistanceOctile`), nothing past
the reach.

| hook | what changes with the switch on |
|---|---|
| `DiscomfortMap.DoCycle` (GetSectorsToAdd) → `DiscomfortMod.AddWasteSectors` | the waste map is drawn again if the filth moved, and its changed and dropped sectors are re-added along with the threat map's. Also the sectors a threat map dropped where stink keeps the sector alive — with one child the studio removes such a sector, with two its threat would stay behind |
| `DiscomfortMap.AddSectors` → `DiscomfortMod.AddWaste` | each re-added sector gets one point per whole or part `reach + 1` of stink, at most `DiscomfortMod.WasteDiscomfortCap` = 4 |
| `DiscomfortMap.RemoveEmptySectors` → `DiscomfortMod.WasteKeepsSector` | a sector with stink is kept, as one with threat is |
| `DiscomfortMod.OnSleepStart` → `MeasureSleep` | the stink a sleeper smells is the waste map's value at the exit divided by `reach + 1`: an item at the exit is one unit, one at the edge of the reach a sixth (at 5 tiles). Noise is still counted |

The cap of 4 is the cautious stance's lowest panic level and the most at which a rest may start
(`AIConstants.HighestDiscomfortLevelForLeisureActivityToStart`), a quarter of what stops work (16):
stink alone never makes a colonist flee, down tools or give up a rest. What it does do is cost 1–4
more per step across dirty ground, so colonists path around it and may be slower to haul waste; a
threat over dirty ground reaches panic up to 4 sooner. The waste map never blocks a tile.

**Saves:** nothing new. The waste map is kept beside its discomfort map
(`ConditionalWeakTable`), not in `DiscomfortMap.Children`, which is saved as map ids — a save naming a
map type that a build without the mod lacks would not load. It is drawn again from the items after a
load, and the first update re-adds every sector. What a save does hold is the discomfort map's own
values, stink included, in the format they always had. Loaded with the mod compiled out, sectors with
only stink in them go at the first update; a sector shared with a threat keeps up to 4 of stale stink
until that threat moves. Switched off in a game, the next update takes it all out.

**Cost:** a redraw scans the map's tiles once (80 × 80 to 256 × 256: 6,400–65,536 lookups) and
paints `(2 × reach + 3)²` tiles per filthy tile (169 at 5), per colonist discomfort map — two per
colonist type — once per movement-map update (`AIConstants.PlayerMovementMapUpdateInterval`, 5 s).
When the filth and the reach are as last time nothing is painted and nothing downstream moves; when
they change, only the sectors that differ are re-added to the discomfort and movement maps, as for a
threat. Reading it at bedtime is one lookup. `--discomfort-selftest` runs the studio's
`DiscomfortMap.DoCycle` through the three hooks on a 48 × 48 map: values and falloff, the cap, the
sleeper's units, a threat leaving dirty ground, removal, Bold, switching off and on, and the map left
untouched with the switch off.

### The STINK overlay — `discomfort.stinkOverlay`

Kastuk (2026-10-17): *"I sure the visible layer of discomfort map will help players to see the sources
of stink at least."* A **STINK** row in the overlay panel above the minimap, beside the studio's
THREATS, while DISCOMFORT is on (`HUDOverlayPanel.AddStinkOverlayRow`; also a switch under DISCOMFORT
in MOD OPTIONS, kept in `ModSettings.xml` like HudMod's rows, not in the save). It is drawn the way
THREATS is: `GameWorldRenderer.SetupInfluenceQuads` → `DiscomfortMod.SetupStinkOverlay`, after
`ThreatRender`, puts one tile quad per tile with stink on screen into the same influence-map buffer
through `ÏnfluenceRender.DrawTile` (`DiscomfortMod.StinkRender`).

What it shows (`DiscomfortMod.StinkOverlayLayer`): with WASTE ON THE DISCOMFORT MAP on, the waste map
itself — what colonists route by and bedtime comfort is read from. Without it, the same drawing done
on demand for the filth on screen and within the reach of it, into a layer of its own that nothing
in the simulation reads — so the overlay works whenever DISCOMFORT is on, without the waste map's
effect on pathing. The tile values are the same either way. Colour (`DiscomfortMod.StinkColorFor`):
olive under 5 units of stink (`value / (reach + 1)`), brown from 5 — a bed whose door is there loses
comfort to it — faded in by `units / 10`, never below 0.2 so the edge of the reach shows. With the
waste map off the comfort cost counts every item within the reach fully, so there the fade is a
guide to where the stink is, not the exact cost.

**Cost:** off, or with DISCOMFORT off, one settings check a frame; nothing is scanned and the
on-demand layer is let go. On with the waste map, a lookup per tile on screen. On without it, a scan
of the tiles on screen plus the reach each frame (a few thousand lookups), redrawn only when the
filth there or the view changes. **Verified offline** in `--discomfort-selftest`: the on-demand
values equal the waste map's over whole-map, on-pile, just-outside and out-of-reach windows; the
waste map shown tile for tile as the sleeper reads it; nothing scanned while off or with DISCOMFORT
off; the colour mapping. **Not verified on screen:** the quads' look, the row's place in the panel,
and the cost in a large colony.

## Mod settings: `user/ModSettings.xml`

Mods get a configuration file and a **MOD OPTIONS** section under their entry in **SELECT MODS**
(main menu → MODDING, or OPTIONS → MOD OPTIONS..., also in game). Both come from one registry, so a
mod declares a setting once and gets the file entry, the menu control and the save stamp together.

```xml
<ModSettings>
  <!-- CHARCOAL FROM PEAT - Adds the recipe that turns 1 dry peat into 1 charcoal ...  AFFECTS SAVES -->
  <Setting id="peat.charcoalFromPeat" value="true" />
  <Setting id="port.saveDateFormat" value="yyyy-MM-dd HH:mm" />
</ModSettings>
```

It is in the **game folder**, beside `user/Mods`, not in `Documents\Unclaimed World` where
`Options.xml` lives: mod settings describe which code is running, so they belong with the DLLs they
configure and not with the player's resolution and volume. Edit it by hand or use the menu — the
menu rewrites it as each setting is changed (LANGUAGE, in OPTIONS, on OK), keeping your comments' subjects, your order, and any entry belonging to a
mod you do not currently have installed. A malformed file is reported and ignored, never fatal.

Declaring one from a mod:

```csharp
private static readonly ModSetting Fast = ModSettings.Toggle(
    "myMod", "fasterCharcoal", "FASTER CHARCOAL", defaultValue: true,
    toolTip: "Halves the time the kiln takes.",
    affectsSimulation: false, takesEffectOnNextLoad: true);

if (Fast.On) { ... }
```

`Toggle` gives a checkbox, `Choice(..., string[] choices, ...)` a combo box, `Text` a text box,
`Key` a key button (click, press the new key) under the mod's own MOD OPTIONS.
Register before the first data load — from the same place your Harmony patches are applied, which
the loader runs before the game object exists.

`affectsSimulation: true` means "a save made with this on may not load with it off". Those, and
only those, are recorded in the save.

### Saves know what they were made with

Every save carries a one-line stamp of the modded content it was written with: the third-party mods
that were loaded, and every simulation-affecting setting that was not at its stock value. It lives
in the snapshot **header**, which the save list already reads, so:

- a save with modded content is marked **MODDED** in red in the save and load lists, and its
  tooltip names what it needs, each line **green** if this session has that thing and **red** if it
  does not — so "will this open?" is answerable by looking at it
- the save panel says, once, what this session's modded content is
- **loading** a save whose stamp does not match the running session offers to load it *with the
  settings it was made with* — which works, because the data tables are rebuilt on the way in. Turn
  that prompt off with `port.askAboutModsOnLoad` to always load with your own settings instead,
  which is what you want when deliberately testing a save against a different configuration.
- those applied settings are **scoped to that session**: the file is never rewritten, and returning
  to the main menu puts your own back. Opening one modded save does not quietly change what your
  next new game is played with.

Older saves have no stamp and read as stock, which is what they almost always are.

The stamp only covers what a setting can control. Loading a save that needs a third-party mod DLL
you no longer have is still your problem to fix — the prompt says which one, and no amount of
switching can load an assembly that is not there.

### Verifying data changes without launching

`tools/DataExport` runs the game's data loader with no window, so content edits can be checked
offline:

```
dataexport <game-dir>              export the tables as the game will run them (mod ON)
dataexport <game-dir> --nomods     export the stock tables, for diffing
dataexport <game-dir> --read-back  also load back in Read mode - the real test that the
                                   exported XML is sufficient to run from
```

Diffing the two exports is how the content above was checked: `processTypes.xml` is the only
table that differs, by exactly the two new keys, and no table fails to export either way — so
`entityTypes.xml` shows the item and smithy changes too.

Why it is in source rather than loaded: BepInEx 5 targets .NET Framework / Mono and cannot load
into a .NET 8 process, and BepInEx 6's CoreCLR loader is pre-release. But the deeper reason is
that HarmonyLib exists to reach what you cannot name from outside an assembly — and from inside,
the twelve patch targets, six reflected private methods and seven `___field` injections are just
fields and methods. The port has the source, so the whole apparatus collapsed into ordinary call
sites. See `PeatMod`, `RawhideStringMod`, `RespawnCapMod` and `MainMenuMod` in `mods/`.

Two bugs were fixed in the translation. The scenario patch called
`AllScenarioLoader.LoadAllScenarioHeaders()`, which enumerates `user/Scenarios` with
`Directory.GetDirectories` and therefore throws on any install that has never created that
folder — every fresh one; it now falls back to the built-in list. And the wheel-scroll baseline
was a `static` that `Client.HandleInput` also wrote to, coupling two unrelated classes; it is now
per-`DataSheet`.

**Workflow.** Run `--export-data` once — the data loads when a game starts, so start a new game
or load a save, then quit. You now have editable XML under `data/BaseData/`. Edit it, then run
with `--data-from-xml`. Keep a copy of the pristine export to diff against; Steam's *Verify
integrity of game files* will not restore `data/BaseData` because those files were never part of
the download.

`dataexport <game-dir> --read-back` does the same thing without launching the game,
which is faster to iterate on and prints exactly which tables worked.

### What actually round-trips today

**All 69 tables export, read back from the XML alone, and pass the game's validation — 79 files,
8.8 MB.** `processTypes.xml` alone is over 2 MB, so the interesting content is very much in scope.
Gate 80 case 35 runs `dataexport --read-back` and fails on any table that does not.

Until October 2026, 7 did not. What it took, for anyone extending a data type:

| cause | fix | tables |
|---|---|---|
| `TileResourceType.RenderableTypeMode` reads `The.Sim.Mode`, and the tool had no Sim | `DataExport` makes one before loading, as the game has by its loading screen (`EnsureSim`) | ResourceTypes |
| `XmlSerializer` will not use an `IXmlSerializable` class as the derived type an `[XmlInclude]` names, and `MachineBodyPartType` was one | `MachineBodyPartType` is plain data now, like `BiologicalBodyPartType`; its generated proxy is gone | BodyTypes |
| `BodyLayerType.DamageReductionFactor` / `DamageReductionConstant` are `Dictionary<string, float>`, which `XmlSerializer` cannot serialize | `[XmlIgnore]`, and written as `KVP<string, float>[]` under the same element names | BodyLayerTypes |
| `ContainerType`'s `[XmlInclude]` list stopped at 8 of its 12 subtypes | the other four added: Magazine, Other, UpgradableBuilding, Upgradable | EntityTypes |
| `ParticleSystemType.spriteBlendState` is a `BlendState`, a graphics object (`TextureCollection` and all); `NoOfEmitters` had a private setter | the blend state is written by name (`Additive`, `AlphaBlend`, ...); the setter is public | Particles |
| knock-ons: a table whose write fails stays empty, and the next one to look something up in it dies | nothing more | AttackTypes (`'humanoid'`), FilterSettingTypes (`'entity:human'`) |

**Two tables came off an earlier version of this list with one attribute each.** `XmlSerializer`
flattens a type to its UNQUALIFIED name, so `ChangeCreditsAction.Operation` collided with
`ChangeResourcesAction.Operation` and `PropertyPresentation.Sorting` with `WindowSystem.Grid.Sorting`
— and it refuses the whole graph rather than picking one. `[XmlType("ChangeCreditsOperation")]` and
`[XmlType("PresentationSorting")]` took six tables with them: ActionSets, EventActionTypes,
AllegianceEvents, PolledEventTypes, PresentationTypeCategories and GUI. Worth knowing if you try
the same trick: the attribute has to go on the type that is **not** nested. Putting it on
`Grid.Sorting` changed nothing; putting it on the top-level `PropertyPresentation.Sorting` fixed
both tables.

**If you add a subtype** of a polymorphic data type (`ContainerType`, `BodyPartType`, ...), add it
to the base's `[XmlInclude]` list, and do not make it `IXmlSerializable`: either one breaks the
export of every table that holds one, and case 35 will say which.

Two things had to be true before that could be seen, and both are in `tools/DataExport`:
`PrepareLookups` creates the two ID lookup collections the *data loaders* use
(`GatheringSiteType`, `NeedType`) — a game gets them from `Sim.CreateLookupCollections`, and
without them `StructureLoader.Init` died on a null dictionary inside `LookUp<T, Id>.Add` before any
of this was visible. And `--traces` prints the stack for each failed table, which is how those two
were found; the failure list alone says which table, never where.

**Careful:** a table whose write fails leaves a truncated XML file behind. None fails today, but
if you break one, delete its file before running `--data-from-xml`, or you get an
`XmlException` that looks like an unrelated problem.

### If you add a proxied data type

`CustomXmlSerializer` no longer compiles proxies at runtime — .NET 8 removed that — so they are
generated at build time into `base_game/UnclaimedWorld/Generated/XmlProxies/`. If you give a type a
`_proxyData` field, run:

```
tools/build/20-generate-xml-proxies.sh            regenerate
tools/build/20-generate-xml-proxies.sh --check    fail if what is committed is stale
```

Run `--check` before you ship anything. A stale proxy does not error — the field just silently
disappears from the XML.

## DesktopGL

DesktopGL is not a side target any more — **it is the build**, and the only one that ships. One
build serves Windows, Linux and macOS: `dotnet build -p:UwPlatform=GL` produces 8 managed
assemblies with **no SharpDX**, plus SDL2/OpenAL natives for every RID. Music is transcoded to
Ogg Vorbis by `tools/build/32-convert-media.sh`.

MGFX effects are shader-profile-specific and MonoGame throws on a mismatch, so all 19 must be
built for the OpenGL profile from HLSL. They are, by
`tools/build/34-build-gl-effects-shadowdusk.sh`, from the studio's sources in `assets/effects/`
with no transformation — and on any of the three platforms, because that compiler does not need
Windows. `tools/build/60-package-gl.sh` refuses to package until all 19 are present: a package
short even one crashes at content load rather than degrading.

WindowsDX still exists (`-p:UwPlatform=DX`) and is kept as a correctness oracle — something to
compare renders against — not as a shipping target.

MonoGame has no DesktopGL `VideoPlayer` at all, which is why the menu background is a motion-JPEG
animation rather than the shipped video. That turned out better than a workaround: the GL build
never had a menu animation before, and now it does.

## Licensing, briefly

Full version in [license.md](../license.md). Short version: the game is under the **Unclaimed World
Community License**, which Refactored Games wrote to permit exactly this — use, modify,
distribute, build derivative projects — with three conditions that matter to anything you make
from it:

- **non-commercial**, and that travels with a fork;
- it must say it is **unofficial** and not endorsed by Refactored Games;
- redistribution carries the license with it.

The port's own code and tooling are MIT. The third-party libraries in `base_game/` are
studio-modified forks of MS-PL XNA-era projects — Aaron MacDougall's WindowSystem, David Astle's
XNA Animation Component Library, Microsoft's Sprite Sheet and RoundLine samples — and keep their
own terms.

No compiled content is committed — no `.xnb`, because that is a build output, not because it
could not be. The studio released the sources it is built from, and they are all in `assets/`.

**Please [buy the game](https://store.steampowered.com/app/284100/).** The licence that makes
this possible was a gift from its authors, and a port of it should send people toward their work
rather than around it.
