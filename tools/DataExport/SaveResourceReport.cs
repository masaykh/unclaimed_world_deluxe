using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-resources=&lt;file.sav&gt;: reads a saved game the way --save-selftest does and prints,
/// for every gathered resource on the ground, how much is left against the most each place ever
/// held, and when the places grow back next. Then the items and structures the colony has.
///
/// WHY. Kastuk, "Nature can be more natural" (2026-10-12): "all of firewood resource zones at
/// Making Headway savegame got exhausted quickly", with the save. Whether that is RegrowthMod or
/// the colony's appetite is a question the save's own numbers answer.
/// </summary>
internal static partial class Program
{
    private static string saveReport;

    private static int SaveResourceReport(Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        double nowSeconds = sim.TotalUnPausedGameTimeInSeconds;
        double nowDays = nowSeconds / DateAndTime.secondsPerDay;
        Console.WriteLine($"==> the clock: {nowDays.ToString("0.0", inv)} days of play"
                          + (sim.DateAndTime == null ? "" : $"; calendar year {sim.DateAndTime.CurrentTimeDateYear.Year}, day {sim.DateAndTime.CurrentTimeDateYear.Day} of 12"));

        var tiles = (IDictionary)typeof(LookUpSortedDictionary<TerrainTile, TerrainTileID>).GetField("collection", AnyStatic).GetValue(null);
        var resources = (IDictionary)typeof(LookUp<ResourceContainer, ResourceID>).GetField("collection", AnyStatic).GetValue(null);
        var byType = new SortedDictionary<string, List<(int held, int most, double? nextDays, int x, int y)>>(StringComparer.Ordinal);
        foreach (ResourceContainer r in resources.Values)
        {
            if (r is not TileResourceContainer || r.ResourceType == null) continue;
            var items = (IList)GetField(r, "snapshotResourceItems");
            int held = items?.Count ?? 0;
            int most = r.Replenish?.MaxItemsEverSet ?? held;
            double? next = r.Replenish == null ? null : (double?)GetField(r.Replenish, "replenishTimePointInSeconds");
            object tileId = GetField(r, "snapshotTerrainTile");
            var tile = tiles.Contains(tileId) ? (TerrainTile)tiles[tileId] : null;
            if (!byType.TryGetValue(r.ResourceType.KeyName, out var list)) byType[r.ResourceType.KeyName] = list = new();
            list.Add((held, most, next.HasValue ? (next.Value - nowSeconds) / DateAndTime.secondsPerDay : null, tile?.X ?? -1, tile?.Y ?? -1));
        }

        Console.WriteLine("==> resources on the ground (places, items left / most ever held)");
        foreach (var kv in byType)
        {
            var l = kv.Value;
            int held = l.Sum(t => t.held), most = l.Sum(t => t.most);
            int empty = l.Count(t => t.held == 0);
            Console.WriteLine($"    {kv.Key,-28} {l.Count,5} places  {held,6} / {most,-6} ({(most == 0 ? 0 : 100.0 * held / most).ToString("0", inv)}%)  {empty} empty");
        }

        byType.TryGetValue("firewood", out var wood);
        if (wood != null)
        {
            ResourceType type = GameData.Instance.AllResourceTypes["firewood"];
            Console.WriteLine("==> firewood in detail");
            Console.WriteLine("    grows back on year days " + string.Join(", ", type.OrderedDaysOfYearToReplenish.Select(d => (d.GetMean() * 12.0).ToString("0.0", inv)))
                              + ", by " + type.FractionOfMaximumToReplenishEachTime.ToString("0.##", inv) + " of a place's most each time");
            foreach (var g in wood.GroupBy(t => t.most).OrderBy(g => g.Key))
            {
                int grows = (int)Math.Round(type.FractionOfMaximumToReplenishEachTime * g.Key);
                Console.WriteLine($"    most {g.Key,3}: {g.Count(),4} places, {g.Sum(t => t.held),5} left, {g.Count(t => t.held == 0),4} empty;"
                                  + $" each regrowth adds {grows} (before GATHERED-DOWN PLANTS REGROW SLOWLY)");
            }
            var nexts = wood.Where(t => t.nextDays.HasValue).Select(t => t.nextDays.Value).OrderBy(d => d).ToList();
            if (nexts.Count > 0)
            {
                Console.WriteLine($"    next regrowth: in {nexts.First().ToString("0.0", inv)} to {nexts.Last().ToString("0.0", inv)} days"
                                  + $" (median {nexts[nexts.Count / 2].ToString("0.0", inv)}); {wood.Count - nexts.Count} places have none due");
            }
        }

        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);
        var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        foreach (Entity e in entities.Values)
        {
            string key = e.EntityType?.KeyName;
            if (key == null) continue;
            if (key.Contains("irewood") || key.Contains("Peat") || key.Contains(":peat") || key.Contains("harcoal") || key.Contains("tove") || key.Contains("iln")
                || key.Contains("ven") || key.Contains("ampfire") || key.Contains("still") || key.Contains("itchen") || key.Contains("urnace"))
            {
                counts[key] = counts.TryGetValue(key, out int n) ? n + 1 : 1;
            }
        }
        Console.WriteLine("==> fuel, and what burns it (entities anywhere in the save)");
        foreach (var kv in counts) Console.WriteLine($"    {kv.Key,-40} {kv.Value}");
        Console.WriteLine("save resource report done");
        return 0;
    }
}
