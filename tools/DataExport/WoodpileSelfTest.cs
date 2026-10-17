using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --woodpile-selftest: FirewoodMod's KEEP DRY FIREWOOD IN THE WOODPILE.
///
/// WHAT. Kastuk, "Firewood out of any wood", 2026-10-17: "I like to have visual pile of firewood to
/// check remained quantity without looking into inventories ... Let it be a)" - the dried batch stays
/// in the woodpile and fires take it from there. The studio's tables must still say what the mod
/// counts on: makeFirewood puts 10 firewood of 0.3 bulk in the woodpile, whose 6 bulk is room for one
/// batch kept and one being made. The hauling planner's own ItemIsOK (through reflection) must leave
/// an item in a woodpile alone with the switch on, and plan it as the studio does with it off or
/// anywhere else. Starting a batch must eject only what the next batch needs room for.
///
/// NOT HERE. A woodpile with a real container on a play site: MakeRoomForBatch's ejecting and the
/// refuelling that takes from the pile need a running colony (see the mod's summary for the code
/// that says they work).
/// </summary>
internal static partial class Program
{
    private static int WoodpileSelfTest()
    {
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> woodpile self-test");
        UWGame.Mods.FirewoodMod.RegisterSettings();
        var keep = UWGame.Mods.ModSettings.Find("firewood.keepInWoodpile");
        if (keep == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        Check(keep.DefaultValue == "true" && keep.AffectsSimulation && !keep.TakesEffectOnNextLoad,
              "on by default, a simulation setting that applies at once (no table changes)");

        // The studio's tables.
        var types = GameData.Instance.AllEntityTypes;
        EntityType woodpile = types["structure:firewoodStack"];
        EntityType firewood = types["item:firewood"];
        Check(UWGame.Mods.FirewoodMod.IsWoodpile(woodpile), "structure:firewoodStack is a woodpile (tag woodpileTool, a production output)");
        var woodpiles = types.Values.Where(UWGame.Mods.FirewoodMod.IsWoodpile).Select(t => t.KeyName).ToList();
        Check(woodpiles.Count == 1, "it is the only one: " + string.Join(", ", woodpiles));
        Check(!UWGame.Mods.FirewoodMod.IsWoodpile(types["structure:compostPit"]) && !UWGame.Mods.FirewoodMod.IsWoodpile(firewood),
              "a compost pit or an item is not");
        float capacity = ((ToolContainerType)woodpile.ContainerType).ProductionOutputStorageType.GetTotalCapacity();
        var makeFirewood = GameData.Instance.AllProcessTypes["makeFirewood"];
        float? batch = UWGame.Mods.FirewoodMod.BatchBulk(makeFirewood, woodpile);
        float each = firewood.ItemType.MaximumBulk ?? 0f;
        Check(capacity == 6f && each == 0.3f, FormattableString.Invariant($"the woodpile holds {capacity} bulk, a firewood is {each}"));
        Check(batch.HasValue && UWGame.Common.IsEqual(batch.Value, 3f),
              FormattableString.Invariant($"a makeFirewood batch puts {batch} bulk in it (10 x 0.3)"));
        Check(batch.HasValue && UWGame.Common.IsLessThanOrEqual(2 * batch.Value, capacity),
              "one batch kept and one being made fit (\"room enough for one output batch and one in production\")");
        var into = GameData.Instance.AllProcessTypes.Values
            .Where(p => p.Outputs != null && p.Outputs.Any(o => o.ToolContainerToPlaceIn != null && o.ToolContainerToPlaceIn.Contains(woodpile)))
            .Select(p => p.KeyName).ToList();
        Check(into.SequenceEqual(new[] { "makeFirewood" }), "makeFirewood is the only process that fills it: " + string.Join(", ", into));
        Check(UWGame.Mods.FirewoodMod.BatchBulk(GameData.Instance.AllProcessTypes["chopFirewood"], woodpile) == 0f,
              "a process that puts nothing in it needs no room");

        // Starting a batch: what is put outside.
        List<(float bulk, bool done)> Pile(int done, int unfinished = 0) =>
            Enumerable.Repeat((each, true), done).Concat(Enumerable.Repeat((each, false), unfinished)).ToList();
        int Ejected(List<(float bulk, bool done)> pile) =>
            UWGame.Mods.FirewoodMod.ToEject(pile, i => i.bulk, i => i.done, capacity, batch ?? 3f).Count;
        int OnlyUnfinished(List<(float bulk, bool done)> pile) =>
            UWGame.Mods.FirewoodMod.ToEject(pile, i => i.bulk, i => i.done, capacity, batch ?? 3f).Count(i => !i.done);
        Check(Ejected(Pile(0)) == 0, "an empty woodpile: nothing to put outside");
        Check(Ejected(Pile(10)) == 0, "one dried batch (10) stays while the next is made");
        Check(Ejected(Pile(7)) == 0, "a batch fires have taken from (7 left) stays");
        Check(Ejected(Pile(20)) == 10, "two dried batches (20): the second is put outside, to be hauled away");
        Check(Ejected(Pile(13)) == 3, "13: three are put outside, ten stay");
        Check(Ejected(Pile(5, 10)) == 10 && OnlyUnfinished(Pile(5, 10)) == 10,
              "an unfinished batch is put outside, as the studio did, and the 5 dried ones stay");

        // The hauling planner, through its own ItemIsOK.
        var woodpileEntity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty("EntityType")!.SetValue(woodpileEntity, woodpile);
        var pitEntity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty("EntityType")!.SetValue(pitEntity, types["structure:compostPit"]);
        LookUp<Entity, EntityID>.Add((EntityID)900, woodpileEntity);
        LookUp<Entity, EntityID>.Add((EntityID)901, pitEntity);
        FieldInfo location = typeof(UWGame.GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo completed = typeof(MemoryFact).GetField("isCompleted", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MemoryFact Firewood(long? containedBy)
        {
            var fact = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
            fact.EntityType = firewood;
            fact.Bulk = each;
            fact.ContainedBy = containedBy.HasValue ? (EntityID)containedBy.Value : null;
            location.SetValue(fact, (Vector3?)new Vector3(100f, 100f, 0f));
            completed.SetValue(fact, true);
            return fact;
        }
        MethodInfo itemIsOK = typeof(HaulingJobManager).GetMethod("ItemIsOK", BindingFlags.Static | BindingFlags.NonPublic)!;
        bool Planned(IKnownEntityData item) => (bool)itemIsOK.Invoke(null, new object[] { item })!;
        var inWoodpile = Firewood(900);
        var inPit = Firewood(901);
        var onGround = Firewood(null);
        Check(inWoodpile.CanBeHauled() && Entity.CanBeHauled(inWoodpile), "dried firewood in a woodpile can be hauled - and fetched for fuel (Entity.CanBeHauled)");
        keep.Value = "false";
        Check(Planned(inWoodpile), "before (the studio's rule, switch off): the planner hauls firewood out of the woodpile");
        keep.Value = "true";
        Check(!Planned(inWoodpile), "now: it leaves firewood in the woodpile");
        Check(UWGame.Mods.FirewoodMod.StaysInWoodpile(inWoodpile) && inWoodpile.CanBeHauled(),
              "  while fuel and input fetching, which ask only CanBeHauled, still find it");
        Check(Planned(onGround) && Planned(inPit), "  firewood on the ground or in another container is planned as before");
        var noTool = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty("EntityType")!.SetValue(noTool, types["structure:compostPit"]);
        Check(!UWGame.Mods.FirewoodMod.MakeRoomForBatch(noTool, makeFirewood), "  starting a batch at any other tool empties it as the studio does");
        keep.Value = "false";
        Check(!UWGame.Mods.FirewoodMod.MakeRoomForBatch(woodpileEntity, makeFirewood), "  and, switched off, at a woodpile too");
        keep.Value = keep.DefaultValue;

        Console.WriteLine(failures == 0 ? "woodpile self-test OK" : $"woodpile self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }
}
