using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
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
/// NOT HERE. HaulingJobManager's whole pass - storages, items and region searches need a play
/// site. What it calls, RankStorage and PrioritiseHaul, is what is checked.
/// </summary>
internal static partial class Program
{
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
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(haul, high, null);
        Check(haul.Priority == Priority.High, "  a planned haul takes it (PrioritiseHaul)");
        var toTrade = (HaulingJobSpecificItem)RuntimeHelpers.GetUninitializedObject(typeof(HaulingJobSpecificItem));
        toTrade.IsHaulJobToStorage = true;
        toTrade.IsToTradeOfferStorage = true;
        UWGame.Mods.StockpilePriorityMod.PrioritiseHaul(toTrade, high, null);
        Check(toTrade.Priority == Priority.Normal, "  a haul to a trade offer does not");
        Check(UWGame.Mods.StockpilePriorityMod.Next(Priority.Normal) == Priority.High && UWGame.Mods.StockpilePriorityMod.Next(Priority.High) == Priority.Low
              && UWGame.Mods.StockpilePriorityMod.Next(Priority.Low) == Priority.Normal, "the button goes NORMAL, HIGH, LOW, NORMAL");
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
