using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;
using WM = UWGame.Mods.WeatherMod;

namespace UW.Tools.DataExport;

/// <summary>
/// --weather-selftest, step 4: what keeps the cold and the wet off (COLD AND WET HURT).
///
/// Checked on the stormiest mid-winter night step 3 found, through the real GoalDoSleep hook
/// (WeatherMod.OnSleepStart) and the real EvaluateSleep.ScoreComfort, on a small map of real
/// TerrainTiles holding tree entities with real Tree components: a grown, living spoak and sanctuary
/// tree cover a sleeper within their reach and not beyond it; a dead one, a young one and a felled
/// one (gone from the tile) do not; a sanctuary tree without its caps covers less. A person the
/// loaded scenario calls a "PRECOL explorer" (Twinkler Island's own EntityTypeDescription loader)
/// wears the suit, and the Muckroot research team does not. A hide cloak and a Mackintosh in the
/// colonist's Equipment storage (found through the real LookUp, as HideGearMod.Wears reads it).
/// Covers multiply with each other and with the campfire's half; a hut is still none. With no tree,
/// suit or cloak, every number is step 3's to the bit; switched off, nothing.
///
/// NOT HERE. Whether colonists walk to a tree, and whether they have a cloak on when they lie down,
/// needs a running colony. The Mackintosh as an item and a recipe is case 40's (--hidegear-selftest).
/// </summary>
internal static partial class Program
{
    private const int ShelterMapSize = 16;

    private static void ShelterSelfTest(Action<bool, string> Check, long stormDay, Action<long, double> at, Func<Entity> sleeper)
    {
        Console.WriteLine("  -- what keeps it off: trees, PRECOL suits, cloaks");
        var exposure = UWGame.Mods.ModSettings.Find("weather.exposure");
        static string F(double v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        const double Bedtime = 0.875;
        exposure.Value = "true";
        at(stormDay, Bedtime);
        WM.Night? tonight = WM.Tonight();
        Check(tonight.HasValue, "the night ahead, with its shares");
        if (!tonight.HasValue) return;
        WM.Night night = tonight.Value;
        float open = night.Open;
        Check(Math.Abs(night.Cold + night.Rain + night.Wind - open) < 1e-5f && night.Under(WM.Cover.None) == open && night.Rain > 0.1f && night.Wind > 0.05f,
              "the storm night " + F(open) + " in the open is its shares: the dry cold " + F(night.Cold) + " + the rain " + F(night.Rain) + " + the wind " + F(night.Wind) +
              "; with no cover, step 3's number to the bit");

        // ------------------------------------------------------------ a map with trees on it
        MapManager oldMap = UWGame.The.Map;
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = ShelterMapSize;
        map.mapTileHeight = ShelterMapSize;
        map.TileMap = new TerrainTile[ShelterMapSize][];
        for (int x = 0; x < ShelterMapSize; x++)
        {
            map.TileMap[x] = new TerrainTile[ShelterMapSize];
            for (int y = 0; y < ShelterMapSize; y++) map.TileMap[x][y] = new TerrainTile { TilePos = new TilePos(x, y) };
        }
        UWGame.The.Map = map;
        FieldInfo location = typeof(UWGame.GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo size = typeof(Tree).GetField("size", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo season = typeof(Tree).GetField("inSeason", BindingFlags.Instance | BindingFlags.NonPublic)!;
        static Vector3 Centre(int x, int y) => new Vector3((x + 0.5f) * MapManager.tileSize, (y + 0.5f) * MapManager.tileSize, 0f);
        var types = GameData.Instance.AllEntityTypes;
        Entity Plant(string key, int x, int y, float grown = 1f, InSeason inSeason = InSeason.Summer)
        {
            var tree = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
            typeof(Entity).GetProperty("EntityType")!.SetValue(tree, types[key]);
            tree.Components = new Dictionary<Type, Component>();
            var living = (Tree)RuntimeHelpers.GetUninitializedObject(typeof(Tree));
            living.Parent = tree;
            size.SetValue(living, (float?)grown);
            season.SetValue(living, inSeason);
            tree.Components.Add(typeof(Tree), living);
            location.SetValue(tree, (Vector3?)Centre(x, y));
            TerrainTile tile = map.TileMap[x][y];
            tile.TreesOnTile ??= new List<Entity>();
            tile.TreesOnTile.Add(tree);
            return tree;
        }
        void ClearTrees()
        {
            foreach (TerrainTile[] column in map.TileMap) foreach (TerrainTile t in column) t.TreesOnTile = null;
        }
        Check(types.ContainsKey(WM.SpoakKey) && types.ContainsKey(WM.SanctuaryTreeKey) && types.ContainsKey("tree:spoakdeadnaked") && types.ContainsKey("tree:sanctuarytreedead")
              && types[WM.SpoakKey].TreeType != null && types[WM.SanctuaryTreeKey].TreeType?.HasSummerWinterCycle == true,
              "the studio's living spoak and sanctuary tree (which has a summer and a winter) and their dead ones are trees in the tables");

        // A sleeper at a tile's centre, through the real hook; what it keeps in CustomFields.
        float Measure(Entity who, Entity place, Vector3? where)
        {
            who.CustomFields = null;
            foreach (var p in who.SimEffects.EffectProfiles.Select(p => p.EffectProfileType).ToList()) who.SimEffects.Remove(p);
            location.SetValue(who, where);
            WM.OnSleepStart(who, place);
            return who.CustomFields != null && who.CustomFields.TryGetValue(WM.ExposureKey, out var kept) ? kept.NumberResult ?? -1f : -1f;
        }
        Entity colonist = sleeper();
        const int TX = 8, TY = 8;
        bool Same(float a, float b) => Math.Abs(a - b) < 1e-6f;
        Check(Measure(colonist, null, null) == open && Measure(colonist, null, Centre(TX, TY)) == open,
              "no tree, no suit, no cloak: " + F(open) + " in the open, step 3's to the bit, wherever they lie");

        // ------------------------------------------------------------ trees
        Plant(WM.SpoakKey, TX, TY);
        float spoak = Measure(colonist, null, Centre(TX + 1, TY));
        Check(Same(spoak, night.Under(WM.SpoakCover)) && spoak < open && Same(spoak, night.Cold + 0.6f * night.Rain + 0.8f * night.Wind),
              "a tile from a grown, living spoak: " + F(spoak) + " (the rain x0.6, the wind x0.8, the cold as it is)");
        Check(Measure(colonist, null, Centre(TX + 2, TY)) == open, "  two tiles from it, beyond its reach (" + F(WM.SpoakReachTiles) + " tiles): " + F(open));
        ClearTrees();
        Check(Measure(colonist, null, Centre(TX + 1, TY)) == open, "  felled (gone from its tile's TreesOnTile): nothing");
        Plant("tree:spoakdeadnaked", TX, TY);
        Plant("tree:spoakdeadleaves", TX, TY);
        Check(Measure(colonist, null, Centre(TX + 1, TY)) == open, "  a dead spoak, naked or with its dead leaves: nothing");
        ClearTrees();
        Plant(WM.SpoakKey, TX, TY, grown: 0.5f);
        Check(Measure(colonist, null, Centre(TX + 1, TY)) == open, "  a young spoak (size 0.5, the studio's young sprite): nothing");
        ClearTrees();

        Plant(WM.SanctuaryTreeKey, TX, TY);
        float caps = Measure(colonist, null, Centre(TX + 2, TY));
        Check(Same(caps, night.Under(WM.SanctuaryCapsCover)) && Same(caps, 0.9f * night.Cold + 0.3f * night.Rain + 0.5f * night.Wind) && caps < spoak,
              "two tiles from a living sanctuary tree with its caps: " + F(caps) + " (the rain x0.3, the wind x0.5, the cold x0.9)");
        Check(Measure(colonist, null, Centre(TX + 3, TY)) == open, "  three tiles away, beyond its reach (" + F(WM.SanctuaryReachTiles) + " tiles): " + F(open));
        ClearTrees();
        Plant(WM.SanctuaryTreeKey, TX, TY, inSeason: InSeason.Winter);
        float bare = Measure(colonist, null, Centre(TX + 2, TY));
        Check(Same(bare, night.Under(WM.SanctuaryBareCover)) && bare > caps && bare < open,
              "  the same tree bare, without its caps (InSeason.Winter, the studio's winter sprite): " + F(bare));
        ClearTrees();
        Plant("tree:sanctuarytreedead", TX, TY);
        Plant("tree:sanctuarytreedeadvines", TX, TY);
        Check(Measure(colonist, null, Centre(TX + 1, TY)) == open, "  a dead sanctuary tree, bare or overgrown: nothing");
        ClearTrees();
        Plant(WM.SpoakKey, TX, TY);
        Plant(WM.SanctuaryTreeKey, TX + 1, TY, inSeason: InSeason.Winter);
        float both = Measure(colonist, null, Centre(TX, TY));
        Check(Same(both, 1f * night.Cold + 0.6f * night.Rain + 0.7f * night.Wind),
              "under a spoak and beside a bare sanctuary tree: the best of each - the spoak's rain, the sanctuary tree's wind: " + F(both));
        ClearTrees();

        // ------------------------------------------------------------ the PRECOL suit
        var descriptions = GameData.Instance.AllEntityTypeDescriptions;
        descriptions.TryGetValue("personDescription", out EntityTypeDescription baseDescription);
        Check(!WM.IsPrecolExplorer(colonist), "the base game's tables: a colonist is no PRECOL explorer");
        var twinkler = UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data.EntityTypeDescriptionLoader.Init();
        var muckroot = UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data.EntityTypeDescriptionLoader.Init();
        foreach (var d in muckroot) descriptions[d.KeyName] = d;
        Check(!WM.IsPrecolExplorer(colonist), "Muckroot Research Station's (\"" + muckroot[0].EntityName + "\", who wear PRECOL suits in some of their textures): not one");
        foreach (var d in twinkler) descriptions[d.KeyName] = d;
        bool precol = WM.IsPrecolExplorer(colonist);
        float suit = Measure(colonist, null, Centre(TX, TY));
        Check(precol && Same(suit, night.Under(WM.PrecolSuitCover)) && Same(suit, 0.4f * night.Cold + 0.5f * night.Rain + 0.5f * night.Wind),
              "Twinkler Island's (\"" + twinkler[0].EntityName + "\"): the storm night out in the survival suit " + F(suit) + " (the cold x0.4, the rain and the wind x0.5)");
        Check(WM.ColdChance(suit) < WM.ColdChance(open), "  a cold " + F(100 * WM.ColdChance(suit)) + "% likely instead of " + F(100 * WM.ColdChance(open)) + "%");
        Plant(WM.SanctuaryTreeKey, TX, TY);
        float suitCaps = Measure(colonist, null, Centre(TX, TY));
        Check(Same(suitCaps, 0.9f * 0.4f * night.Cold + 0.3f * 0.5f * night.Rain + 0.5f * 0.5f * night.Wind),
              "  under a sanctuary tree's caps too: " + F(suitCaps) + " - the covers multiply");
        ClearTrees();
        descriptions.Remove(twinkler[0].KeyName);
        Check(!WM.IsPrecolExplorer(colonist) && Measure(colonist, null, Centre(TX, TY)) == open, "  another scenario loaded: the suit is gone");

        // ------------------------------------------------------------ clothes, in the Equipment storage
        var storage = (AgentStorage)RuntimeHelpers.GetUninitializedObject(typeof(AgentStorage));
        storage.Equipment = (ItemStorage)RuntimeHelpers.GetUninitializedObject(typeof(ItemStorage));
        storage.Equipment.StoredItems = new List<EntityID>();
        colonist.Contains = storage;
        EntityID Garment(long id, string key)
        {
            var item = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
            typeof(Entity).GetProperty("EntityType")!.SetValue(item, new EntityType(key));
            item.Components = new Dictionary<Type, Component>();
            LookUp<Entity, EntityID>.Add((EntityID)id, item);
            return (EntityID)id;
        }
        EntityID cloak = Garment(9701, UWGame.Mods.HideGearMod.CloakKey), mackintosh = Garment(9702, UWGame.Mods.HideGearMod.MackintoshKey);
        Check(Measure(colonist, null, Centre(TX, TY)) == open, "an empty Equipment storage: " + F(open));
        storage.Equipment.StoredItems.Add(cloak);
        float inCloak = Measure(colonist, null, Centre(TX, TY));
        Check(Same(inCloak, night.Under(WM.CloakCover)) && Same(inCloak, 0.8f * night.Cold + night.Rain + 0.7f * night.Wind),
              "in a hide cloak: " + F(inCloak) + " (the cold x0.8, the wind x0.7, the rain as it is)");
        storage.Equipment.StoredItems.Clear();
        storage.Equipment.StoredItems.Add(mackintosh);
        float inMack = Measure(colonist, null, Centre(TX, TY));
        Check(Same(inMack, night.Under(WM.MackintoshCover)) && Same(inMack, 0.8f * night.Cold + 0.3f * night.Rain + 0.6f * night.Wind) && inMack < inCloak,
              "in a Mackintosh: " + F(inMack) + " (the cold x0.8, the rain x0.3, the wind x0.6)");
        storage.Equipment.StoredItems.Add(cloak);
        Check(Measure(colonist, null, Centre(TX, TY)) == inMack, "  with the cloak as well: the Mackintosh, not both");

        // ------------------------------------------------------------ everything at once, and the places
        Entity hut = Colonist(types.Values.First(t => t.ContainerType?.ResidenceType != null));
        EntityType fireType = types.Values.First(t => t.GatheringSiteType != null && t.ToolType != null);
        Entity fire = Colonist(fireType);
        var tool = (Tool)RuntimeHelpers.GetUninitializedObject(typeof(Tool));
        tool.Parent = fire;
        fire.Components.Add(typeof(Tool), tool);
        SetField(tool, "isPrepared", (bool?)true);
        foreach (var d in twinkler) descriptions[d.KeyName] = d;
        Plant(WM.SanctuaryTreeKey, TX, TY);
        float all = Measure(colonist, fire, Centre(TX, TY));
        float allByHand = 0.5f * (0.9f * 0.4f * 0.8f * night.Cold + 0.3f * 0.5f * 0.3f * night.Rain + 0.5f * 0.5f * 0.6f * night.Wind);
        Check(Same(all, allByHand) && all < WM.ColdFromExposure,
              "a PRECOL explorer in a Mackintosh, by a lit campfire under a sanctuary tree's caps: " + F(all) +
              " (tree x suit x Mackintosh x campfire) - below " + F(WM.ColdFromExposure) + ", no cold");
        Check(Measure(colonist, hut, Centre(TX, TY)) == 0f, "  in a hut: none, as before");
        exposure.Value = "false";
        Check(Measure(colonist, null, Centre(TX, TY)) == -1f, "  switched off: nothing measured, nothing kept");
        exposure.Value = "true";
        foreach (var d in twinkler.Concat(muckroot)) descriptions.Remove(d.KeyName);
        if (baseDescription != null) descriptions["personDescription"] = baseDescription;

        // ------------------------------------------------------------ the real EvaluateSleep.ScoreComfort, by where they would lie
        var evaluator = (EvaluateSleep)RuntimeHelpers.GetUninitializedObject(typeof(EvaluateSleep));
        Entity plain = sleeper();
        SetField(evaluator, "entity", plain);
        SetField(evaluator, "MaxSleepIncreasePerDay", GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping * GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople);
        MethodInfo scoreComfort = typeof(EvaluateSleep).GetMethod("ScoreComfort", BindingFlags.Instance | BindingFlags.NonPublic)!;
        double Score(Vector3 where) => (double)scoreComfort.Invoke(evaluator, new object[] { null, where })!;
        double underCaps = Score(Centre(TX + 1, TY)), bareGround = Score(Centre(1, 1));
        exposure.Value = "false";
        double stock = Score(Centre(1, 1));
        exposure.Value = "true";
        Check(Math.Abs(bareGround / stock - (1.0 - 0.5 * open)) < 1e-6 && underCaps > bareGround
              && Math.Abs(underCaps / stock - (1.0 - 0.5 * night.Under(WM.SanctuaryCapsCover))) < 1e-6,
              "EvaluateSleep.ScoreComfort on the storm night: the bare ground " + F(bareGround) + ", a spot under a sanctuary tree's caps " + F(underCaps) +
              " (studio's " + F(stock) + ")");
        long summerDay = 20 * 12 + 5;
        at(summerDay, Bedtime);
        Check(Measure(colonist, null, Centre(TX, TY)) == 0f && Score(Centre(TX + 1, TY)) == stock, "a mid-summer night: nothing to keep off - 0 and the studio's score");
        at(stormDay, Bedtime);

        colonist.Contains = null;
        UWGame.The.Map = oldMap;
    }
}
