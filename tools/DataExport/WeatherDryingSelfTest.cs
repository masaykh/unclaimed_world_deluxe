using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;
using WM = UWGame.Mods.WeatherMod;

namespace UW.Tools.DataExport;

/// <summary>
/// --weather-selftest, step 5: RAIN AND WIND DRY OR WET.
///
/// Which of the studio's processes dry on a drying structure, read from its tables. Then a real
/// SimProcess of makeDriedThunderChicken - its own EstimateProgressDelta and Produce, stepped with the
/// real DateAndTime one update interval at a time through hours the seeded schedule makes wet, windy
/// or still - on a meat drying rack in the open, on a drying shed, on a rack indoors, and on a rack
/// under a sanctuary tree's caps on a map of real TerrainTiles: an hour of rain takes it back by
/// -0.3 x the rain x what the hour would have dried, never below 1%; a dry, windy hour dries it faster;
/// the caps keep off 70% of the rain's effect; a shed and indoors are the studio's; switched off, or
/// in still, dry air, the studio's to the bit. The completion estimate is none while a batch goes
/// back. Things that sun dry where they lie (wet firewood): stopped by rain, faster in wind, through
/// the hook NonLivingEntity.Degrade calls (checked by IL).
///
/// NOT HERE. Whether colonists build drying sheds or put racks under trees, and what the "Checking
/// progress" visits do when a batch is late, need a running colony.
/// </summary>
internal static partial class Program
{
    private static void DryingSelfTest(Action<bool, string> Check, DateAndTime date)
    {
        Console.WriteLine("  -- rain and wind dry or wet");
        var drying = UWGame.Mods.ModSettings.Find("weather.drying");
        var rain = UWGame.Mods.ModSettings.Find("weather.rain");
        var enabled = UWGame.Mods.ModSettings.Find("weather.enabled");
        Check(drying != null && drying.DefaultValue == "false" && drying.AffectsSimulation && !drying.TakesEffectOnNextLoad,
              "RAIN AND WIND DRY OR WET: off by default, a simulation setting that applies at once");
        if (drying == null || rain == null || enabled == null) return;
        static string F(double v) => v.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
        static string F1(double v) => v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        enabled.Value = "true";
        rain.Value = "true";
        drying.Value = "true";
        int seed = WM.Seed;

        // ------------------------------------------------------------ which processes dry
        var processes = GameData.Instance.AllProcessTypes;
        var dryingKeys = processes.Values.Where(WM.IsDryingProcess).Select(p => p.KeyName).OrderBy(k => k, StringComparer.Ordinal).ToList();
        var expected = new[]
        {
            "makeDriedStreakFin", "makeDriedThunderChicken", "makeDryPeat", "makeFirewood", "makeMegapodRawhide",
            "makeThunderChickenRawhide", "makeTurnipSalami", "makeWhipjawRawhide"
        };
        Check(dryingKeys.SequenceEqual(expected),
              "the processes that dry on their own on a rack, a shed or a stack (the studio's \"dries on its own\"): " + string.Join(", ", dryingKeys));
        Check(!WM.IsDryingProcess(processes["constructMeatDryingRack"]) && !WM.IsDryingProcess(processes["salvageDryingShed"]),
              "  building or salvaging a rack is work, not drying");

        // ------------------------------------------------------------ hours from the schedule
        const double Hour = 1.0 / 24.0;
        bool Hourly(double from, Func<WM.State, bool> holds)
        {
            for (int i = 0; i <= 60; i++) if (!holds(WM.Scheduled(seed, from + i * Hour / 60))) return false;
            return true;
        }
        double Find(Func<WM.State, bool> holds)
        {
            for (double d = 16 * 12; d < 30 * 12; d += Hour / 4) if (Hourly(d, holds)) return d;
            return double.NaN;
        }
        double wetHour = Find(s => s.Rain >= 0.5f);
        double windyHour = Find(s => s.Rain == 0f && s.Wind >= 9f);
        double stillHour = Find(s => s.Rain == 0f && s.Wind <= WM.DryingWindFrom);
        Check(!double.IsNaN(wetHour) && !double.IsNaN(windyHour) && !double.IsNaN(stillHour),
              "the schedule has an hour of steady rain (0.5 and over), a dry hour of wind of 9 m/s and over, and a dry, still hour");
        if (double.IsNaN(wetHour) || double.IsNaN(windyHour) || double.IsNaN(stillHour)) return;

        // ------------------------------------------------------------ a map, a rack, a shed, a batch
        MapManager oldMap = UWGame.The.Map;
        const int Size = 16;
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = Size;
        map.mapTileHeight = Size;
        map.TileMap = new TerrainTile[Size][];
        for (int x = 0; x < Size; x++)
        {
            map.TileMap[x] = new TerrainTile[Size];
            for (int y = 0; y < Size; y++) map.TileMap[x][y] = new TerrainTile { TilePos = new TilePos(x, y) };
        }
        UWGame.The.Map = map;
        FieldInfo location = typeof(UWGame.GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo idField = typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!;
        static Vector3 Centre(int x, int y) => new Vector3((x + 0.5f) * MapManager.tileSize, (y + 0.5f) * MapManager.tileSize, 0f);
        var types = GameData.Instance.AllEntityTypes;
        long nextId = 9801;
        Entity Make(string key, Vector3? where)
        {
            var e = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
            typeof(Entity).GetProperty("EntityType")!.SetValue(e, types[key]);
            e.Components = new Dictionary<Type, Component>();
            var id = (EntityID)nextId++;
            idField.SetValue(e, id);
            location.SetValue(e, where);
            LookUp<Entity, EntityID>.Add(id, e);
            return e;
        }

        ProcessType chicken = processes["makeDriedThunderChicken"];
        float ProductivityOn(string structure) =>
            chicken.ProcessToolSet.Tools.SelectMany(a => a.ToolsAndProductivity ?? new List<Tuple<EntityType, float>>())
                .First(t => t.Item1.KeyName == structure).Item2;
        Entity output = Make("item:driedThunderChicken", null);
        var nonLiving = (NonLivingEntity)RuntimeHelpers.GetUninitializedObject(typeof(NonLivingEntity));
        nonLiving.Parent = output;
        output.Components.Add(typeof(NonLivingEntity), nonLiving);
        // What the game updates it at: ProcessType.FinalUpdateInterval, set by PostLoadContentInitialize,
        // which needs the content this test does not load - so its own GetUpdateInterval (2 s: it creates outputs).
        float interval = (float)typeof(ProcessType).GetMethod("GetUpdateInterval", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(chicken, null)!;

        SimProcess Batch(Entity structure)
        {
            var p = (SimProcess)RuntimeHelpers.GetUninitializedObject(typeof(SimProcess));
            typeof(SimProcess).GetProperty("ProcessType")!.SetValue(p, chicken);
            typeof(SimProcess).GetProperty("OutputEntities")!.SetValue(p, new List<EntityID> { output.EntityID });
            typeof(SimProcess).GetProperty("IsStarted")!.SetValue(p, true);
            SetField(p, "updateInterval", (double?)interval);
            p.StationaryTools = new List<EntityID> { structure.EntityID };
            p.StationaryToolsTypeCombination = new ToolTypeCombination
            {
                Tools = new List<Tuple<EntityType, float>> { Tuple.Create(structure.EntityType, 0f) },
                Productivity = ProductivityOn(structure.EntityType.KeyName)
            };
            return p;
        }
        void GoTo(double days)
        {
            int y = (int)Math.Floor(days / 12.0);
            date.Year = y;
            date.TimeOfYear = days / 12.0 - y;
            date.TimeOfDay = days - Math.Floor(days);
            date.AdvanceTime(0.0);
        }
        var frame = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(interval));
        // An hour (or more) of the real SimProcess.Update from a progress, with the real clock moving
        // one update interval at a time. Also what the studio's code adds up to: the same delta each
        // update, added the way Produce adds it.
        float Run(SimProcess p, double from, float start, out float studio, out double meanRain, double hours = 1.0)
        {
            nonLiving.Progress = start;
            GoTo(from);
            int steps = (int)Math.Round(hours * date.SecondsPerDay / 24.0 / interval);
            float stock = chicken.CalculateProgressDelta(interval, 1f, 1f, p.StationaryToolsTypeCombination.Productivity);
            studio = start;
            meanRain = 0;
            for (int i = 0; i < steps; i++)
            {
                meanRain += WM.Scheduled(seed, WM.ScheduleDays(date.Year, date.TimeOfYear)).Rain / (double)steps;
                p.Update(frame, out _);
                studio += stock;
                date.AdvanceTime(interval);
            }
            return nonLiving.Progress;
        }

        Entity rack = Make("structure:meatDryingRack", Centre(2, 2));
        SimProcess onRack = Batch(rack);
        float rackHours = 1f / chicken.CalculateProgressDelta(date.SecondsPerDay / 24.0, 1f, 1f, ProductivityOn("structure:meatDryingRack"));
        const float Start = 0.5f;

        // ------------------------------------------------------------ before: switched off, the studio's to the bit
        drying.Value = "false";
        float offWet = Run(onRack, wetHour, Start, out float studioWet, out double rainOfHour);
        Check(offWet == studioWet && offWet > Start,
              "switched off: an hour of rain " + F1(rainOfHour) + " dries a batch of dried thunder chicken on a meat drying rack " + F(Start) + " -> " + F(offWet) +
              ", the studio's to the bit (a batch takes " + F1(rackHours) + " hours there)");
        drying.Value = "true";
        rain.Value = "false";
        Check(Run(onRack, wetHour, Start, out _, out _) == studioWet, "  and with RAIN AND WIND off (no rain to dry or wet), the same");
        rain.Value = "true";

        // ------------------------------------------------------------ rain: back, slowly
        float wet = Run(onRack, wetHour, Start, out _, out _);
        double normal = studioWet - Start, back = wet - Start;
        Check(back < 0 && Math.Abs(back / normal - (-WM.RainUndoesDrying * rainOfHour)) < 0.01 * WM.RainUndoesDrying * rainOfHour,
              "now: the same hour of rain (" + F1(rainOfHour) + ") takes it back " + F(Start) + " -> " + F(wet) + ": " + F(back / normal) +
              " x what the hour would have dried, -0.3 x the rain");
        float floor = Run(onRack, wetHour, 0.02f, out _, out _);
        Check(floor >= WM.DryingFloor - 1e-6f && floor < 0.02f && nonLiving.IsStarted(),
              "  a batch just begun goes back no further than " + F(WM.DryingFloor) + ": " + F(floor) + " - still started");
        nonLiving.Progress = 0.4f;
        GoTo(wetHour);
        onRack.Update(frame, out _);
        Check(onRack.ProgressSpeed < 0f && onRack.ComputeEstimatedCompletionTime() == null,
              "  while it goes back it has no completion time (the studio's would be \"now\")");
        GoTo(stillHour);
        onRack.Update(frame, out _);
        Check(onRack.ComputeEstimatedCompletionTime().HasValue, "  and dry, it has one again");

        // ------------------------------------------------------------ wind: faster; still air: the studio's
        float windy = Run(onRack, windyHour, Start, out float studioWindy, out _);
        double windRate = (windy - Start) / (studioWindy - Start);
        double expectedRate = 0;
        for (int i = 0; i < 60; i++)
        {
            var s = WM.Scheduled(seed, windyHour + (i + 0.5) * Hour / 60);
            expectedRate += WM.DryingRate(s, WM.Cover.None, WM.DryingPlace.Open) / 60.0;
        }
        Check(windRate > 1.25 && windRate <= WM.MaxWindDrying + 1e-4 && Math.Abs(windRate - expectedRate) < 0.01,
              "a dry, windy hour (" + F1(WM.Scheduled(seed, windyHour).Wind) + " m/s and over) dries it x" + F(windRate) + " (1 + 0.05 a m/s above 3, at most x1.5)");
        float still = Run(onRack, stillHour, Start, out float studioStill, out _);
        Check(still == studioStill, "a dry, still hour (3 m/s or less): the studio's to the bit");

        // ------------------------------------------------------------ a shed, and indoors
        Entity shed = Make("structure:dryingShed", Centre(4, 4));
        SimProcess inShed = Batch(shed);
        float shedWet = Run(inShed, wetHour, Start, out float shedStudio, out _);
        Check(shedWet == shedStudio && shedWet > Start, "on a drying shed (\"will protect the meat from rain\"): the hour of rain dries it as the studio's does");
        float shedWindy = Run(inShed, windyHour, Start, out float shedStudioWindy, out _);
        Check(Math.Abs((shedWindy - Start) / (shedStudioWindy - Start) - windRate) < 1e-3, "  and (\"while letting the wind blow through\") the wind dries it x" + F((shedWindy - Start) / (shedStudioWindy - Start)));
        Entity indoorRack = Make("structure:meatDryingRack", Centre(6, 6));
        SetField(indoorRack, "containedBy", (EntityID?)(EntityID)9899);
        float indoors = Run(Batch(indoorRack), wetHour, Start, out float indoorStudio, out _);
        Check(indoors == indoorStudio, "a rack inside something (a building's storage): the studio's to the bit, rain or not");

        // ------------------------------------------------------------ under the trees
        FieldInfo size = typeof(Tree).GetField("size", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo season = typeof(Tree).GetField("inSeason", BindingFlags.Instance | BindingFlags.NonPublic)!;
        void Plant(string key, int x, int y)
        {
            var tree = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
            typeof(Entity).GetProperty("EntityType")!.SetValue(tree, types[key]);
            tree.Components = new Dictionary<Type, Component>();
            var living = (Tree)RuntimeHelpers.GetUninitializedObject(typeof(Tree));
            living.Parent = tree;
            size.SetValue(living, (float?)1f);
            season.SetValue(living, InSeason.Summer);
            tree.Components.Add(typeof(Tree), living);
            location.SetValue(tree, (Vector3?)Centre(x, y));
            map.TileMap[x][y].TreesOnTile ??= new List<Entity>();
            map.TileMap[x][y].TreesOnTile.Add(tree);
        }
        Plant(WM.SanctuaryTreeKey, 10, 10);
        Entity rackUnderCaps = Make("structure:meatDryingRack", Centre(12, 10));
        SimProcess underCaps = Batch(rackUnderCaps);
        float capsWet = Run(underCaps, wetHour, Start, out _, out _);
        double capsBack = capsWet - Start;
        Check(capsBack < 0 && Math.Abs(capsBack / back - WM.SanctuaryCapsCover.Rain) < 0.005,
              "a rack two tiles from a sanctuary tree with its caps: the hour of rain takes it back " + F(capsBack) + ", x" + F(capsBack / back) + " of the open rack's (the caps' rain x0.3)");
        float capsWindy = Run(underCaps, windyHour, Start, out float capsStudioWindy, out _);
        double capsWindRate = (capsWindy - Start) / (capsStudioWindy - Start);
        Check(Math.Abs((capsWindRate - 1) / (windRate - 1) - WM.SanctuaryCapsCover.Wind) < 0.01,
              "  and the wind's speed-up x" + F((capsWindRate - 1) / (windRate - 1)) + " of the open rack's (the caps' wind x0.5): x" + F(capsWindRate));
        Plant(WM.SpoakKey, 2, 3);
        float spoakWet = Run(onRack, wetHour, Start, out _, out _);
        Check(Math.Abs((spoakWet - Start) / back - WM.SpoakCover.Rain) < 0.005, "a rack under a grown, living spoak: x" + F((spoakWet - Start) / back) + " of the rain's effect (the spoak's x0.6)");

        // ------------------------------------------------------------ things that sun dry where they lie
        var sunDrying = types.Values.Where(t => t.NonLivingType?.FinalDegradeType?.KeyName == WM.SunDryingKey).Select(t => t.KeyName).OrderBy(k => k, StringComparer.Ordinal).ToList();
        Check(sunDrying.Contains("item:wetFirewood") && sunDrying.Contains("item:wetMudBrick"),
              "the studio's \"sun drying\" items (\"can be dried in the open\"): " + string.Join(", ", sunDrying));
        Entity wood = Make("item:wetFirewood", Centre(1, 14));
        Entity meat = Make("item:thunderChickenMeat", Centre(1, 14));
        const float Damage = 0.01f;
        float At(double days, Entity item) { GoTo(days + Hour / 2); return WM.SunDryingDamage(item, Damage); }
        var mid = WM.Scheduled(seed, windyHour + Hour / 2);
        Check(At(wetHour, wood) == 0f && At(stillHour, wood) == Damage
              && Math.Abs(At(windyHour, wood) - Damage * WM.DryingRate(mid, WM.Cover.None, WM.DryingPlace.Open)) < 1e-7f && At(windyHour, wood) > Damage,
              "wet firewood left out: stops drying in the rain, dries as the studio's in still air, x" + F(At(windyHour, wood) / Damage) + " in the wind");
        Check(At(wetHour, meat) == Damage, "  raw meat (not sun drying) spoils as the studio's in the rain");
        drying.Value = "false";
        Check(At(wetHour, wood) == Damage, "  switched off, the studio's");
        drying.Value = "true";
        Check(CallsOf(typeof(NonLivingEntity), "Degrade").Contains("WeatherMod.SunDryingDamage")
              && CallsOf(typeof(SimProcess), "EstimateProgressDelta").Contains("WeatherMod.DryingProgress"),
              "the hooks are where they should be: NonLivingEntity.Degrade calls WeatherMod.SunDryingDamage, SimProcess.EstimateProgressDelta WeatherMod.DryingProgress (by IL)");

        UWGame.The.Map = oldMap;
        drying.Value = drying.DefaultValue;
        rain.Value = rain.DefaultValue;
    }

    /// <summary>The methods a method calls, as "Type.Method", read from its IL.</summary>
    private static List<string> CallsOf(Type type, string method)
    {
        var calls = new List<string>();
        foreach (MethodInfo m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(m => m.Name == method))
        {
            byte[] il = m.GetMethodBody()?.GetILAsByteArray();
            if (il == null) continue;
            for (int i = 0; i + 4 < il.Length; i++)
            {
                if (il[i] != 0x28 && il[i] != 0x6F) continue; // call, callvirt
                try
                {
                    MethodBase target = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1));
                    calls.Add(target.DeclaringType?.Name + "." + target.Name);
                }
                catch (Exception)
                {
                }
            }
        }
        return calls;
    }
}
