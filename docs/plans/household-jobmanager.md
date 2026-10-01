# Plan: a JobManager for every entity group

Status: **plan only — nothing here is built.** Written 2026-10-01 in answer to tripleacoder on
the *Private ownership* task: *"If the Household does not currently have an instance of
JobManager (or other needed manager classes) then they should just be added. The whole idea is to
use the same code paths across the ownership types / entity groups."*

The studio asked the same question in `JobManager.cs` and left it open:

```csharp
/*
// only expeditions use the manager... or also households?
// per owner instead?
Expedition expedition;
ExpeditionID snapshotExpedition;
*/
```

This plan answers "per owner", and replaces OwnershipMod's two side-door commands
(`SetHouseholdProduction`, `GiveToHousehold`, commit `f3c978d`) with the paths the colony already
uses.

---

## 1. What exists today

### 1.1 The managers, and who has them

There are five job managers. Four already work per `EntityGroup`. Only one is tied to the
expedition.

| manager | lives on | created for | ticked by |
|---|---|---|---|
| `HaulingJobManager` | `EntityGroup.HaulingJobManager` | expedition (if `CanHaul`), **household**, **person** | `EntityGroup.Update` |
| `OtherJobManager` | `EntityGroup.OtherJobManager` | expedition, **household**, **person** | `EntityGroup.Update` |
| `HuntingJobManager` | `EntityGroup.HuntingJobManager` | expedition (if `CanHunt`) | `EntityGroup.Update` |
| `ThreatJobManager` | `Allegiance.ThreatAndCombatJobManager` | allegiance | `Allegiance.Update` |
| **`JobManager`** | **`Expedition.JobManager`** | **expedition only** (`Expedition.InitPlaySite`) | **`Expedition.Update`** |

`Household`'s constructor already builds `new EntityGroup(this, manageTrade: true,
manageProduction: true)`, so **a household already has `ProductionOrders`**. It has orders and
nothing that reads them.

### 1.2 How tied `JobManager` actually is to an expedition

Less than its home suggests. The shipped class (`base_game/.../Jobs/JobManager.cs`) holds an
`EntityGroup owner` and nothing else. Every `expedition.` reference in `original_src` (40 of them:
`ImportJobs`, `FindBestTool(…, Expedition)`, `CreateProcessJobAndHaulingJobs(…, Expedition)`)
sits in code the studio had already commented out. The constructor is `JobManager(EntityGroup
owner) // Expedition expedition)`: the migration was made and finished. The snapshot saves
`snapshotOwnerID`, an `EntityGroupID`.

The live dependencies on the *parent* of the group:

| site | needs | household today |
|---|---|---|
| `JobManager.GetStandingOrderJobCapacity` → `EntityGroup.GetMaximumJobsBeforeWarning(owner.Parent)` | `IHasEntityGroup.NoOfWorkers` | works (`members.Count`) |
| `JobManager.CreateProcessJobAndHaulingJobs` → `FindProductionLocation(processType, owner.Parent, …)` → `EntityGroup.GetFreeGroundLocation` | `IHasEntityGroup.Location` | works (`Household.Location`: the home, else head of household) |
| `JobManager.GetJobManagerProductionProcessesThatCanProduce` and `CleanupUnstartedJob` → `InventoryPanel.HasAllInputsAndToolsForProcess` | `owner.GetExpedition().HasSkill(...)`, `expedition.Policy.CanUseProcess(...)` | **crashes**: `GetExpedition()` is `Parent as Expedition`, so it is null for a household |
| `JobManager.CreateGatherCombos` → `owner.Zones` | zones | household has none, so no gather combos (correct: households do not gather by standing order) |

The rest of the expedition assumptions sit **outside** `JobManager`:

| site | assumption |
|---|---|
| `Commands/SetProduction` | addresses an `ExpeditionID`; calls `expedition.JobManager.UpdateDirectOrderJobs`. The studio left `// public long EntityGroupID;` commented out beside it |
| `Commands/SetStandingOrder*` | expedition by ID |
| `EvaluateJob` (`:505`, `:1205`) | `itemGroup.GetExpedition()` / `ownerOfItems.GetExpedition()`: client feedback only, null-guarded |
| `EvaluateJob` tool and worksite search | looks only in `ownerOfItems`, so a household cannot see a communal campfire (OwnershipMod's `CommunalFallback` hook) |
| `EvaluateEat` (`GetFoodEntityGroup`) | searches the expedition's food only (OwnershipMod's `AddHouseholdFood` hook); the studio's `// TODO: owner group should be a list - person, household, expedition` |
| `CombatAreaJob:106`, `TradeManager:540` | `owner.GetExpedition()`; not production, out of scope |
| UI: `InventoryPanel`, stockpile/production windows | `The.InGameUI.GetExpedition()`; the player's view is the colony |

### 1.3 How a colonist already chooses between household and colony work

The studio wired it and switched it off. `GoalThink.CreateEvaluators`, under `#region personal &
household jobs - not yet in use!`:

- `EvaluateHaulingJobs(household)`: "fill food stocks", **Leisure**
- `EvaluateHaulingJobs(person)`: "haul personal stuff", **Leisure**
- `EvaluateJob(household, household, …, householdID)`: "do construction and other jobs for the
  household", **Leisure**, young adult to old
- colony `EvaluateJob` / `EvaluateHaulingJobs`: **Work**

So the rule is **the day phase**: colony jobs in Work, household jobs in Leisure. No new
arbitration is needed. The evaluators exist. What's missing is anything that *creates* household
production jobs for them to find.

### 1.4 How the studio meant food to reach a household

`Household.FillNewHome` and the daily refill (commented out, `Household.cs` ~300 and ~880): for
each type in `Expedition.FoodEntitlementPerPerson`, `AddHaulingJob(itemType,
Expedition.ExpeditionOwner, Ownership)`, which is a **hauling job from the colony store to the
household**, capped at `3 * Members.Count * FoodStockSize`. `Expedition.FoodEntitlementPerPerson`
is still filled (`Expedition.cs` ~1109). Ownership changes on delivery, through the hauling code
every item already uses. Our `GiveToHousehold` changes ownership instantly instead.

---

## 2. Target design

### 2.1 `JobManager` moves to `EntityGroup`, next to its three siblings

```
EntityGroup
  HaulingJobManager    (exists)
  OtherJobManager      (exists)
  HuntingJobManager    (exists)
  JobManager           (NEW home; snapshotJobManager CyclableID?, ticked in EntityGroup.Update)
Expedition.JobManager  => OwnedEntities.JobManager   (kept as a forwarding property, so every
                                                       existing caller compiles unchanged)
```

**Per `EntityGroup`, not per ownership type.** That is where the other three already live, it's
what the constructor takes, and it is the studio's "per owner". Which groups get one:

| group | JobManager | why |
|---|---|---|
| expedition | yes, as now | |
| household | **yes** | has `ProductionOrders`, members, a location |
| person | no, for now | a person's group is also built with `manageProduction: true` (`Person.cs`), so it has `ProductionOrders` too — but nobody orders for one. Leaves the door open |
| non-player allegiances | follows `IntelligenceType.CanProduce`, as `ProductionOrders` already does | |

The rule in code: the owner that builds the group creates the manager, as `Household` and `Person`
already do for `HaulingJobManager` and `OtherJobManager`. `Household`'s constructor adds
`ownedEntities.JobManager = new JobManager(ownedEntities)`. Persons are left out on purpose, not
by accident.

### 2.2 Skills and policy come from the group's *governing* expedition

`HasAllInputsAndToolsForProcess` crashes because it asks `owner.GetExpedition()`. A household is
not an expedition, but it is *governed* by one: its members' skills and the colony's policy tiers
apply to it. Add one core accessor:

```csharp
// EntityGroup
public Expedition GetGoverningExpedition()
    => Parent as Expedition ?? (Parent as Household)?.Expedition;   // Person -> its household's
```

`Household.Expedition` already exists (head of household's `CurrentExpedition`). Then
`HasAllInputsAndToolsForProcess` uses the governing expedition for **skills and policy only**, and
keeps `owner` for **inputs and tools**. Callers that mean "is this group the expedition" keep
`GetExpedition()`. This is a core bug fix (a null dereference on a path the studio built for
households), not a mod.

### 2.3 What a household job may see: tools, campfires, stock

Today OwnershipMod hooks `EvaluateJob` so a household job falls back to the colony's tools and
structures. The general rule we propose, as policy on the group:

| | household job may use | from |
|---|---|---|
| inputs (ingredients, materials) | **own only** | what is made belongs to whoever owned the inputs |
| tools, workshops, campfires, kitchens | **own, then communal** | houses and the camp stay communal |
| fuel for a communal fire | communal (the fire's owner) | fixes OwnershipMod's "unfuelled fire refuses household cooking" limit |

This becomes an explicit `EntityGroup.CommunalParent` (the governing expedition's group, or null),
read by `EvaluateJob`'s tool and structure search and by `HasAllInputsAndToolsForProcess`'s *tool*
check. `CommunalFallback` turns into a core rule: one property, read in two places, no `MOD:` hook.
**Open question Q2**: is "tools communal, inputs private" the rule tripleacoder wants in core?

### 2.4 Commands: one path for player and planner

| today | becomes |
|---|---|
| `SetProduction(ExpeditionID, type, count)` | `SetProduction` gains the studio's commented-out **`EntityGroupID`**. If it's set, it addresses that group's `ProductionOrders` and `JobManager`. If it's 0 (every replay written so far), it resolves the expedition as now. |
| `SetStandingOrder` | the same `EntityGroupID` field, same fallback |
| **`SetHouseholdProduction`** (ours) | **retired.** The planner issues `SetProduction(EntityGroupID = household group, meal type, 1)`, and the household's `JobManager.UpdateDirectOrderJobs` makes the job, picks the recipe (`GetJobManagerProductionProcessesThatCanProduce`) and hauls the inputs, exactly as for the colony |
| **`GiveToHousehold`** (ours) | **retired, replaced by hauling.** The studio's design (§1.4): the household's food need becomes `HaulingJobAnyItemOfType`s from the colony store to the household (the import-job shape `JobManager.ImportJobs` already had), carried out by members in Leisure through the household's existing `HaulingJobManager`. Ownership changes on delivery, through the hauling code. |

**Effect on recipe variety.** OwnershipMod currently rotates through the 45 meal recipes by day.
Under `SetProduction` the planner orders an *output type* and the `JobManager` picks the process.
`SelectBestCombo` returns `combos[0]` (CLAUDE.md §7), so variety comes from the planner rotating
the **meal type** it orders, not the recipe. That's the same behaviour for meals with distinct
output types. Meals that share an output type would always get the first recipe until
`SelectBestCombo` selects something (a separate, known bug).

**Effect on the player.** Nothing in the UI addresses a household today, and this plan adds no
household production window. The command is player-capable (a future "this household's orders"
panel issues the same command) but only the planner uses it at first.

### 2.5 The planner stays a planner

`OwnershipMod`'s planner, after this:

- once per Leisure start, per household with a worker:
  - **food**: set the household's import targets (`ProductionOrders` `AmountToKeepInStore` for the
    entitlement types, i.e. 1/2/3 days of need). The household's `JobManager` turns the shortfall
    into hauling jobs. That's the same path as a colony standing order.
  - **cooking**: if no meal is on order, `SetProduction(household, mealType, 1)`.
- nothing else: no direct job creation, no direct ownership change.

The studio rule holds: *"All planners use Commands - the Commands create Jobs"*
(`PhysicalNeedsPlanner`).

---

## 3. Save format

| change | class | version | repair on loading an older save |
|---|---|---|---|
| `EntityGroup` gains `snapshotJobManager` | `EntityGroup.DoVersion` | `Original` → **new `Snapshotter.Version.GroupJobManager`** | none in `EntityGroup`; the field reads as null |
| `Expedition` stops owning it | `Expedition.DoVersion` | → `GroupJobManager` | for a save older than `GroupJobManager`, `Expedition.LoadPostProcess` resolves its old `snapshotJobManager` and **hands it to `OwnedEntities.JobManager`**. The same object with the same `CyclableID`, so no reshuffle |
| households in an old save have none | `Household.LoadPostProcess` | none | if `ownedEntities.ProductionOrders != null && JobManager == null`, create one (§4: RNG) |
| `SetProduction.EntityGroupID` | XML replay field, not a snapshot | n/a | missing in old replays → 0 → expedition, as before |

The precedent is `SnapshotHeader` (`ModsRecorded`) and `RandomGenerator` (`RandomStreamPosition`).
Each class writes its own version, so the bump costs nothing elsewhere. A save written by the new
build does **not** load in an older build (the extra field). That's the normal direction and is
stated in the release notes.

**Coordination:** the *Autoclaim of bodies* task may add its own `Snapshotter.Version` value for
an `ExpeditionPolicy` field. The enum values must be taken in merge order, never reused.

## 4. Replays and determinism

- **RNG.** Every manager's `Regulator` is built from `The.Sim.GameplayRandomGenerator`
  (`JobManager.CreateRegulators`). Creating a household `JobManager` draws from the gameplay
  stream, so **a replay recorded before this change diverges after the first household forms**.
  This is the cost of the change, not a bug. Two mitigations, to choose between (**Q3**):
  1. accept it: replays are tied to the build that recorded them (the header records the program
     version and mods);
  2. give household managers a regulator seeded from the household's ID, drawing nothing from the
     shared stream. Divergence then comes only from the jobs they create, which is unavoidable.
- **Order.** Household managers register with `CycleManager` at `Priority.Medium` like the
  expedition's. `CycleManager` runs its request lists in **registration order**, which follows
  from the regulators and therefore from the RNG: deterministic, but changed by the above.
- **Load-repair** (§3) creates managers in `LoadPostProcess` order (deterministic), but a game
  loaded from an old save and a game that never saved will differ. Same note as above.

## 5. Core or mod

tripleacoder: *"the change belongs in the core game, not in a mod. Because many features and
behaviors will build on it."* Agreed, with this split:

| core (always on, no switch) | mod (`OwnershipMod`, off by default) |
|---|---|
| `EntityGroup.JobManager` and its save version | the planner: when households are paid, how much, what they cook |
| `GetGoverningExpedition` and the `HasAllInputsAndToolsForProcess` fix | the setting "households are paid in food and cook for themselves" |
| `CommunalParent` tool and structure visibility | |
| `SetProduction` / `SetStandingOrder` `EntityGroupID` | |
| household `EvaluateEat` reads household food (the studio's TODO) | |

**Is core behaviour unchanged with the mod off?** It should be. A household `JobManager` with no
orders creates no jobs. `EvaluateEat` reading an empty household stock finds nothing. Tools stay
where they were. The one visible difference is the RNG draw (§4). Each step's gate proves "no
orders → no jobs".

## 6. Migration: small commits, each with its own offline gate

Each step builds, passes gates 80–86, and leaves the game playable with OwnershipMod on *and*
off.

1. **`GetGoverningExpedition` + `HasAllInputsAndToolsForProcess` uses it for skills and policy.**
   Gate: a selftest calls it for a household group and gets an answer instead of a
   `NullReferenceException` (proven red first, on today's code).
2. **`JobManager` moves to `EntityGroup`; `Expedition.JobManager` forwards.** Only the expedition
   gets one in this step. Gate: save → load round-trip of a fixture game, the manager has the same
   `CyclableID`, and a **pre-change save** loads (keep one in `test_build/`) through the repair.
3. **Households get one** (constructor + load repair). Gate: a household with no orders makes 0
   jobs over N cycles. A household with one direct order makes exactly 1 `ProcessJob` in **its
   own** group, with hauling jobs for its inputs.
4. **`CommunalParent`** replaces `OwnershipMod.CommunalFallback`. Gate: the household cooking job
   finds a communal campfire and uses communal fuel. The `MOD:` hook and its Absent stub are
   deleted (gate 86 checks it).
5. **`SetProduction` / `SetStandingOrder` take `EntityGroupID`.** Gate: replay serializer
   round-trip (the test that caught the `[XmlIgnore]` break in `f3c978d`), plus an *old* replay
   XML without the field still resolving to the expedition.
6. **OwnershipMod switches to `SetProduction` and import targets; `SetHouseholdProduction` and
   `GiveToHousehold` are deleted.** Gate: `--ownership-selftest` rewritten. A household's food
   arrives by hauling job, its meal is ordered by `SetProduction`, and nothing outside the
   commands touches the sim. Gate 85/86 clear what the deletion orphans (CLAUDE.md §5: same commit).
7. **`EvaluateEat` reads the household's food in core** (the studio's TODO), removing
   `AddHouseholdFood`. Gate: a person with household food and an empty colony store eats.

Steps 1, 2 and 5 are useful on their own. Steps 3–7 depend on them in order.

## 7. Risks

- **Old saves.** Step 2 is the only one that changes the meaning of existing save data. It needs a
  real pre-change save in the gate, not a synthetic one. A late-game save from a player (Kastuk's
  suggestion) is the best fixture.
- **`ImportJobs` is a stub phase.** The shipped `CycleOnce` skips it (`case Phase.ImportJobs: phase
  = …; break;`). Household food via import targets means writing that phase, which is real logic,
  not plumbing. The studio's commented original is the reference.
- **Colony counts.** Food hauled to a household leaves `CountAvailableItems` for the colony, so
  colony standing orders may produce more. This is the same limit OwnershipMod has today. Whether
  colony targets should count household stock (**Q5**) is a design decision.
- **Job cap.** `GetMaximumJobsBeforeWarning` is per group (`NoOfWorkers * JobsPerWorkerCap`). The
  household cap is small, which is right, but its warnings must not reach the colony's UI.
- **Household merge.** `MergeHouseholds` is commented out wholesale in the original ("LARS: All of
  the ownership related code needs to be reworked, and most code moved to OwnerContent...") and is
  absent from the shipped build, so households never merge today. If it is ever revived, it must
  unregister the disappearing household's `JobManager` as its commented body does for
  `HaulingJobManager`.
- **Emigration** takes the household and its manager with it. `Household.Destroy` must `Destroy()`
  it.
- **Performance.** One extra cyclable per household at a 4 s interval. Cheap, but measured in the
  step 3 gate on a 40-colonist fixture.

## 8. Open questions

For tripleacoder:

- **Q1.** `JobManager` on `EntityGroup` (with the other three managers), not on `Household`. Is that
  the shape you meant?
- **Q2.** Visibility rule for household jobs: **inputs private, tools and campfires communal**. Is
  that right for core, or should tools also be owned?
- **Q3.** RNG: accept that old replays diverge once households have managers, or seed household
  regulators from their ID?
- **Q4.** Food via **hauling to the household** (the studio's `FoodEntitlementPerPerson` design)
  instead of an instant ownership change. Is the extra carrying acceptable?

For Kastuk:

- **Q5.** Should colony standing orders count food that households hold, or only the colony store?
- **Q6.** A late-game save (Steam) to use as the pre-change fixture in step 2's gate.
