using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --stockpile-priority-selftest: StockpilePriorityMod, and the priority a Stockpile now saves.
///
/// WHAT. Kastuk, "Stockpile improvements", 2026-09-12 and 2026-10-10: HIGH stockpiles are filled
/// first, LOW ones when the rest are full, and hauling to a HIGH or LOW stockpile takes its
/// priority. The tiers must never overlap the studio's own score for a pair, a pair the stockpile
/// does not allow (score 0) must stay out, and trade offers must keep theirs. Then the save: a
/// Stockpile written and read back through the game's Snapshotter keeps its priority.
///
/// Kastuk, 2026-10-12, two reports against it:
/// - "a crowd of all colonists coming to store every equipment, take it back, drop, pick up and
///   store again" with a tools stockpile on HIGH: a haul of an item a colonist carries must not be
///   made HIGH (it was: PriorityFor(stockpile) alone), only loose and stored items.
/// - waste unchecked in a hut stayed there: HaulingJobManager's own ItemIsHeldForProcess, through
///   reflection, with a compost-fertilizer job waiting for organic matter. Before the fix (the mod
///   off: the studio's rule) the organic matter in the hut is held; with it on, it leaves - and so
///   does the haul taking it out, once picked up. Held items elsewhere stay held.
///
/// NOT HERE. HaulingJobManager's whole pass - storages, items and region searches need a play
/// site. What it calls, RankStorage and PrioritiseHaul, is what is checked.
/// </summary>
internal static partial class Program
{
    /// <summary>An item as a colony knows it: contained by an entity, and stored in it when that is storage.</summary>
    private static MemoryFact Fact(string type, long? containedBy, long? storedIn)
    {
        var fact = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
        fact.EntityType = GameData.Instance.AllEntityTypes[type];
        fact.ContainedBy = containedBy.HasValue ? (EntityID)containedBy.Value : null;
        fact.StoredPermanentlyIn = storedIn.HasValue ? new StorageTarget((EntityID)storedIn.Value, (StorageID)1) : null;
        return fact;
    }

    /// <summary>A planned haul to storage with its own job ID, as HaulingJobManager makes them.</summary>
    private static HaulingJobSpecificItem NewHaul(long id)
    {
        var haul = (HaulingJobSpecificItem)RuntimeHelpers.GetUninitializedObject(typeof(HaulingJobSpecificItem));
        haul.IsHaulJobToStorage = true;
        typeof(Job).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(haul, (JobID)id);
        return haul;
    }

    private static int StockpilePrioritySelfTest()
    {
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> stockpile priority self-test");
        UWGame.Mods.StockpilePriorityMod.RegisterSettings();
        var enabled = UWGame.Mods.ModSettings.Find("stockpilepriority.enabled");
        if (enabled == null)
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
        DefaultStorageSettings defaults = GameData.Instance.AllDefaultStorageSettings.Values.First();
        Stockpile With(Priority p) => new Stockpile(Stockpile.TypesOfStockpiles.Normal, defaults) { Priority = p };
        Stockpile high = With(Priority.High), normal = With(Priority.Normal), low = With(Priority.Low);
        double Rank(double score, Stockpile s, bool trade = false) => UWGame.Mods.StockpilePriorityMod.RankStorage(score, s, trade);

        Check(enabled.DefaultValue == "false", "off by default");
        enabled.Value = "false";
        Check(Rank(0.5, high) == 0.5 && Rank(0.5, low) == 0.5, "switched off: every stockpile scores as the studio scored it");
        enabled.Value = "true";

        // The studio's best pair scores about 1.1 and its worst a little above 0.
        Check(Rank(0.01, high) > Rank(1.1, normal) && Rank(0.01, normal) > Rank(1.1, low) && Rank(0.01, low) > Rank(1.1, null),
              "HIGH beats every NORMAL, NORMAL every LOW, LOW any loose ground, however near or far");
        Check(Rank(0.9, normal) > Rank(0.3, normal) && Rank(0.9, high) > Rank(0.3, high), "within a tier the studio's score still decides");
        Check(Rank(0.0, high) == 0.0, "a pair the stockpile does not allow stays out (0)");
        Check(Rank(10.5, high, trade: true) == 10.5, "a trade offer keeps its score");
        Check(Rank(0.7, null) == 0.7, "loose ground storage keeps its score");

        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(high) == Priority.High && UWGame.Mods.StockpilePriorityMod.PriorityFor(low) == Priority.Low,
              "hauling to a HIGH stockpile is a HIGH job, to a LOW one a LOW job");
        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(normal) == null && UWGame.Mods.StockpilePriorityMod.PriorityFor(null) == null,
              "  to a NORMAL stockpile or the ground, the TASKS window decides");
        var haul = (HaulingJobSpecificItem)RuntimeHelpers.GetUninitializedObject(typeof(HaulingJobSpecificItem));
        haul.IsHaulJobToStorage = true;
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(haul, high, null, null);
        Check(haul.Priority == Priority.High, "  a planned haul takes it (PrioritiseHaul)");
        var toTrade = (HaulingJobSpecificItem)RuntimeHelpers.GetUninitializedObject(typeof(HaulingJobSpecificItem));
        toTrade.IsHaulJobToStorage = true;
        toTrade.IsToTradeOfferStorage = true;
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(toTrade, high, null, null);
        Check(toTrade.Priority == Priority.Normal, "  a haul to a trade offer does not");
        Check(UWGame.Mods.StockpilePriorityMod.Next(Priority.Normal) == Priority.High && UWGame.Mods.StockpilePriorityMod.Next(Priority.High) == Priority.Low
              && UWGame.Mods.StockpilePriorityMod.Next(Priority.Low) == Priority.Normal, "the button goes NORMAL, HIGH, LOW, NORMAL");

        // Kastuk 2026-10-12: the tools-stockpile shuffle. Every item-and-storage haul goes through
        // PrioritiseHaul with the item as its owner knows it.
        var carried = Fact("item:stoneHammer", containedBy: 500, storedIn: null);
        var loose = Fact("item:stoneHammer", containedBy: null, storedIn: null);
        var stored = Fact("item:stoneHammer", containedBy: 600, storedIn: 600);
        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(high) == Priority.High,
              "before: a HIGH stockpile made every haul to it HIGH, a colonist's own tool too (x" + GameData.Instance.AIConstants.HighJobModifier + ", AIConstants.HighJobModifier)");
        Check(UWGame.Mods.StockpilePriorityMod.IsCarried(carried) && !UWGame.Mods.StockpilePriorityMod.IsCarried(loose) && !UWGame.Mods.StockpilePriorityMod.IsCarried(stored),
              "  a tool in a colonist's keeping is told apart from one lying loose or stored");
        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(high, carried) == null,
              "now: hauling a tool a colonist carries to a HIGH stockpile keeps the TASKS window's priority");
        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(high, loose) == Priority.High && UWGame.Mods.StockpilePriorityMod.PriorityFor(high, stored) == Priority.High,
              "  a loose tool, or one in other storage, still goes to the HIGH stockpile first");
        Check(UWGame.Mods.StockpilePriorityMod.PriorityFor(low, carried) == Priority.Low, "  LOW still applies to a carried one");
        var carriedHaul = NewHaul(11);
        carriedHaul.Priority = Priority.Low;
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(carriedHaul, high, null, carried);
        Check(carriedHaul.Priority == Priority.Low, "  PrioritiseHaul does not raise a carried tool's haul");

        // Kastuk 2026-10-12: waste unchecked in a hut stays in the hut.
        EntityType organic = GameData.Instance.AllEntityTypes["item:organicMatter"];
        DefaultStorageSettings homeStorage = GameData.Instance.AllDefaultStorageSettings["homeStorage"];
        if (homeStorage.MayStockpileCategoryFinal == null)
        {
            // GameData.PostLoadContentInitialize's step for it, which the game runs once content is loaded.
            homeStorage.PostLoadContentInitialize();
        }
        var group = (EntityGroup)RuntimeHelpers.GetUninitializedObject(typeof(EntityGroup));
        group.StructureStockpiles = new Dictionary<EntityID, Stockpile>
        {
            [(EntityID)600] = new Stockpile(Stockpile.TypesOfStockpiles.Normal, homeStorage),   // a hut: no waste
            [(EntityID)700] = new Stockpile(Stockpile.TypesOfStockpiles.Normal, defaults),      // allows it
        };
        var waiting = (HaulingJobAnyItemOfType)RuntimeHelpers.GetUninitializedObject(typeof(HaulingJobAnyItemOfType));
        group.HaulingJobsAnyItemOfType = new Dictionary<EntityType, List<HaulingJobAnyItemOfType>> { [organic] = new List<HaulingJobAnyItemOfType> { waiting } };
        var inHut = Fact("item:organicMatter", containedBy: 600, storedIn: 600);
        var inAllowed = Fact("item:organicMatter", containedBy: 700, storedIn: 700);
        var onGround = Fact("item:organicMatter", containedBy: null, storedIn: null);
        MethodInfo held = typeof(HaulingJobManager).GetMethod("ItemIsHeldForProcess", BindingFlags.Static | BindingFlags.NonPublic)!;
        bool Held(IKnownEntityData item, HaulingJob haul = null) => (bool)held.Invoke(null, new object[] { group, item, haul })!;
        Check(!group.StructureStockpiles[(EntityID)600].MayStockpile(organic, out _) && group.StructureStockpiles[(EntityID)700].MayStockpile(organic, out _),
              "the hut's stockpile (homeStorage) does not allow organic matter; the other one does");
        enabled.Value = "false";
        Check(Held(inHut), "before (the studio's rule): a compost job waiting for organic matter keeps the hut's organic matter where it is");
        enabled.Value = "true";
        Check(!Held(inHut), "now: organic matter its stockpile does not allow leaves the hut although a process wants the type");
        Check(Held(inAllowed) && Held(onGround), "  organic matter where it is allowed, or on the ground, is still left for the process");
        var eviction = NewHaul(12);
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(eviction, null, group, inHut);
        var pickedUp = Fact("item:organicMatter", containedBy: 500, storedIn: null);
        Check(!Held(pickedUp, eviction) && Held(pickedUp, NewHaul(13)),
              "  the haul taking it out is not cancelled once it is picked up; another haul of the type would be");
        group.HaulingJobsAnyItemOfType.Clear();
        Check(!Held(onGround), "  with no job waiting, nothing is held (the studio's rule unchanged)");

        // The zone half (2026-10-17): loose ground inside a stockpile zone that refuses a type is not
        // storage for it, and an item of that type lying there must leave. The tile test both use:
        Zone ZoneWith(Stockpile s)
        {
            var zone = new Zone();
            typeof(Zone).GetField("stockpile", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(zone, s);
            return zone;
        }
        var someone = (UWGame.SimSide.Allegiances.Allegiance)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Allegiances.Allegiance));
        var refusingTile = new TerrainTile { Zones = new Dictionary<UWGame.SimSide.Allegiances.Allegiance, List<Zone>> { [someone] = new List<Zone> { new Zone(), ZoneWith(group.StructureStockpiles[(EntityID)600]) } } };
        var allowingTile = new TerrainTile { Zones = new Dictionary<UWGame.SimSide.Allegiances.Allegiance, List<Zone>> { [someone] = new List<Zone> { ZoneWith(group.StructureStockpiles[(EntityID)700]) } } };
        Check(UWGame.Mods.StockpilePriorityMod.ZoneRefuses(refusingTile, organic) && !UWGame.Mods.StockpilePriorityMod.ZoneRefuses(allowingTile, organic)
              && !UWGame.Mods.StockpilePriorityMod.ZoneRefuses(new TerrainTile(), organic),
              "a tile in a stockpile zone that refuses organic matter refuses it; an allowing zone, a plain zone or no zone does not");

        enabled.Value = enabled.DefaultValue;

        // The save: written and read by the game's own Snapshotter.
        Stockpile read = null;
        string error = null;
        try
        {
            var stream = new MemoryStream();
            var sn = new Snapshotter();
            FieldInfo writer = typeof(Snapshotter).GetField("m_writer", BindingFlags.Instance | BindingFlags.NonPublic)!;
            FieldInfo reader = typeof(Snapshotter).GetField("m_reader", BindingFlags.Instance | BindingFlags.NonPublic)!;
            sn.mode = Snapshotter.Mode.Save;
            writer.SetValue(sn, new BinaryWriter(stream));
            sn.DoISnapshot(With(Priority.Low), verifyField: false);
            stream.Position = 0;
            sn.mode = Snapshotter.Mode.Load;
            writer.SetValue(sn, null);
            reader.SetValue(sn, new BinaryReader(stream));
            read = (Stockpile)sn.DoISnapshot<Stockpile>(null, verifyField: false);
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + ": " + ex.Message;
        }
        Check(error == null && read != null && read.Priority == Priority.Low, "a LOW stockpile saves and loads as LOW" + (error == null ? "" : " - " + error));

        Console.WriteLine(failures == 0 ? "stockpile priority self-test OK" : $"stockpile priority self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }
}
