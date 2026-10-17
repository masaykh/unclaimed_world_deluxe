using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;

namespace UW.Tools.DataExport;

/// <summary>
/// Part of --discomfort-selftest: WASTE ON THE DISCOMFORT MAP (Kastuk, "Discomfort", 2026-10-17).
///
/// Builds what the hooks touch and nothing more: a 48 x 48 tile map (3 x 3 sectors of 16) whose
/// tiles hold filthy items, the player's allegiance with one colonist discomfort map in
/// PlaySiteKnowledge.DiscomfortMaps, and that map's threat child, which paints the threats the test
/// gives it. Then the studio's own DiscomfortMap.DoCycle runs, phase by phase, through the three
/// DiscomfortMod hooks in it - so a check fails if a hook is gone, not only if the mod's arithmetic
/// is wrong.
/// </summary>
internal static partial class Program
{
    private const int WasteTestSize = 48;

    /// <summary>A threat child that paints exactly the threats it is given, as ThreatMap.DoCycle paints them.</summary>
    private sealed class PaintedThreatMap : ThreatMap
    {
        public readonly List<Point> Threats = new List<Point>();

        public PaintedThreatMap()
        {
            Map = new TileLayer(WasteTestSize, WasteTestSize);
        }

        public override bool DoCycle()
        {
            Map.BeginDrawing();
            Map.ClearMaps();
            foreach (Point threat in Threats)
            {
                Map.DrawLinearInfluenceCircle(threat, 40, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes,
                                              InfluenceMap.CircleParameter.FalloffEachTile, 8);
            }
            var empty = new HashSet<Point>(Map.GetAllSectors());
            empty.ExceptWith(Map.AffectedSectors);
            Map.RemoveSectors(empty);
            Map.CompareOldAndNewSectors();
            return true;
        }
    }

    private static int DiscomfortWasteMapChecks(Action<bool, string> check, EntityType waste)
    {
        Console.WriteLine("==> WASTE ON THE DISCOMFORT MAP (DiscomfortMap.DoCycle -> DiscomfortMod.AddWasteSectors, AddWaste, WasteKeepsSector)");
        var types = GameData.Instance.AllEntityTypes;
        EntityType person = null;
        foreach (EntityType t in types.Values)
        {
            if (person == null && t.Person != null && t.IntelligenceType != null) person = t;
        }
        check(person != null, $"a colonist type to draw for ({person?.KeyName})");
        if (person == null) return 1;

        var settings = new[] { "discomfort.enabled", "discomfort.wasteMap", "discomfort.distance" };
        var before = new Dictionary<string, string>();
        foreach (string id in settings) before[id] = UWGame.Mods.ModSettings.Find(id).Value;
        void Set(string id, string value) => UWGame.Mods.ModSettings.Find(id).Value = value;
        check(UWGame.Mods.ModSettings.Find("discomfort.wasteMap")?.DefaultValue == "false", "WASTE ON THE DISCOMFORT MAP is off by default");

        // The map: tiles with nothing on them, as MapManager has them.
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = WasteTestSize;
        map.mapTileHeight = WasteTestSize;
        map.TileMap = new TerrainTile[WasteTestSize][];
        for (int x = 0; x < WasteTestSize; x++)
        {
            map.TileMap[x] = new TerrainTile[WasteTestSize];
            for (int y = 0; y < WasteTestSize; y++) map.TileMap[x][y] = (TerrainTile)RuntimeHelpers.GetUninitializedObject(typeof(TerrainTile));
        }
        MapManager oldMap = UWGame.The.Map;
        UWGame.The.Map = map;

        // Items, as Entity.Location leaves them: in their tile's EntitiesOnTile, located at the tile's centre.
        FieldInfo location = typeof(GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic);
        PropertyInfo entityType = typeof(Entity).GetProperty("EntityType");
        void Put(Point tile, EntityType type, int count)
        {
            TerrainTile t = map.TileMap[tile.X][tile.Y];
            t.EntitiesOnTile ??= new List<Entity>();
            for (int i = 0; i < count; i++)
            {
                var e = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
                entityType.SetValue(e, type);
                e.Components = new Dictionary<Type, Component>();
                location.SetValue(e, (Vector3?)Centre(tile));
                t.EntitiesOnTile.Add(e);
            }
        }
        void Clear(Point tile) => map.TileMap[tile.X][tile.Y].EntitiesOnTile = null;

        // The player's allegiance and its colonist map.
        Site oldSite = UWGame.The.Sim.PlaySite;
        var site = (Site)RuntimeHelpers.GetUninitializedObject(typeof(Site));
        var player = (Allegiance)RuntimeHelpers.GetUninitializedObject(typeof(Allegiance));
        player.SharedKnowledge = (SharedKnowledge)RuntimeHelpers.GetUninitializedObject(typeof(SharedKnowledge));
        player.SharedKnowledge.PlaySiteKnowledge = (PlaySiteKnowledge)RuntimeHelpers.GetUninitializedObject(typeof(PlaySiteKnowledge));
        var byStance = new Dictionary<ThreatStance, DiscomfortMap>();
        player.SharedKnowledge.PlaySiteKnowledge.DiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>>
        {
            [ProtectionLevel.Exposed] = new Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>> { [person] = byStance },
        };
        site.PlayerAllegiance = player;
        UWGame.The.Sim.PlaySite = site;

        int failures = 0;
        void Check(bool ok, string what)
        {
            check(ok, what);
            if (!ok) failures++;
        }
        try
        {
            static void Cycle(DiscomfortMap dm)
            {
                for (int guard = 0; guard < 50 && !dm.DoCycle(); guard++) { }
            }
            (DiscomfortMap, PaintedThreatMap) NewColonistMap(ThreatStance stance)
            {
                var threat = new PaintedThreatMap();
                var dm = new DiscomfortMap(ProtectionLevel.Exposed, threat) { IDName = "Exposed" + person.KeyName + stance };
                byStance[stance] = dm;
                return (dm, threat);
            }
            Point w = new Point(22, 20);
            int Value(DiscomfortMap dm, int x, int y) => dm.Map.GetValue(x, y);
            int? Units(Point at) => UWGame.Mods.DiscomfortMod.WasteUnitsAt(person, Centre(at));

            Set("discomfort.enabled", "true");
            Set("discomfort.distance", "5");
            int cap = UWGame.Mods.DiscomfortMod.WasteDiscomfortCap;
            Check(cap == 4 && cap <= GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart
                  && cap < GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue && cap <= (int)ThreatStance.Cautious,
                  $"stink adds at most {cap}: a rest may still start, work goes on (16), nobody flees (panic level is never below 4)");

            // OFF (the default): the studio's map, bit for bit - the waste is there and nothing draws it.
            Set("discomfort.wasteMap", "false");
            Put(w, waste, 1);
            var (offMap, offThreat) = NewColonistMap(ThreatStance.Normal);
            Cycle(offMap);
            Check(offMap.Map.GetAllSectors().Count == 0, "off: a filthy item draws nothing on the discomfort map");
            offThreat.Threats.Add(new Point(20, 20));
            Cycle(offMap);
            Check(Value(offMap, 20, 20) == 40 && Value(offMap, 22, 20) == 40 - 16 && Value(offMap, 28, 20) == 0,
                  "off: a threat is drawn as the studio draws it (40, 8 a tile less)");
            UWGame.Mods.DiscomfortMod.MeasureSleep(person, Centre(new Point(19, 20)), out int offStink, out _, out _);
            UWGame.Mods.DiscomfortMod.Measure(Centre(new Point(19, 20)), out int countedStink, out _, out _);
            Check(offStink == 1 && countedStink == 1, $"off: the sleeper's stink is counted item by item, as before ({offStink})");
            offThreat.Threats.Clear();
            Cycle(offMap);
            Check(offMap.Map.GetAllSectors().Count == 0, "off: the threat gone, its sectors go, as the studio has it");
            Check(Units(w) == null, "off: there is no waste map to read");
            byStance.Clear();

            // ON.
            Set("discomfort.wasteMap", "true");
            var (dmap, threat) = NewColonistMap(ThreatStance.Normal);
            Cycle(dmap);
            Check(Units(w) == 1, $"on: one filthy item is one unit at its own tile ({Units(w)})");
            Check(Value(dmap, 22, 20) == 1 && Value(dmap, 27, 20) == 1 && Value(dmap, 28, 20) == 0 && Value(dmap, 22, 25) == 1 && Value(dmap, 22, 26) == 0,
                  "on: and adds 1 to the discomfort map as far as the reach (5 tiles), nothing past it");
            Check(Units(new Point(23, 20)) == 0, "on: a tile away it is less than a unit (5 of 6)");

            Put(w, waste, 29);
            Cycle(dmap);
            Check(Units(w) == 30 && Units(new Point(25, 20)) == 15 && Units(new Point(25, 23)) == 7 && Units(new Point(28, 20)) == 0,
                  $"on: 30 items: 30 units at the pile, 15 at 3 tiles, 7 at (3, 3) (octile 4.5), none past the reach " +
                  $"({Units(w)}, {Units(new Point(25, 20))}, {Units(new Point(25, 23))}, {Units(new Point(28, 20))})");
            Check(Value(dmap, 22, 20) == cap && Value(dmap, 27, 20) == cap && Value(dmap, 28, 20) == 0,
                  $"on: the discomfort map gets no more than {cap} from it, anywhere");

            // The comfort penalty is read off the map.
            UWGame.Mods.DiscomfortMod.MeasureSleep(person, Centre(new Point(25, 20)), out int onStink, out _, out _);
            UWGame.Mods.DiscomfortMod.Measure(Centre(new Point(25, 20)), out int distanceStink, out _, out _);
            Check(onStink == 15 && distanceStink == 30, $"on: sleeping 3 tiles from the pile smells 15 units off the map (counted: {distanceStink})");
            Check(Math.Abs(UWGame.Mods.DiscomfortMod.PenaltyFor(onStink, 0, 3, 40) - 0.09f) < 1e-6f, "  so -9% comfort, where counting gave -18%");

            // Threat and stink together, and a threat leaving ground that still stinks.
            threat.Threats.Add(new Point(20, 20));
            Cycle(dmap);
            Check(Value(dmap, 20, 20) == 40 + cap && Value(dmap, 15, 20) == 0 && Value(dmap, 16, 20) == 8,
                  $"on: threat and stink add up ({Value(dmap, 20, 20)})");
            threat.Threats.Clear();
            Cycle(dmap);
            Check(Value(dmap, 20, 20) == cap && Value(dmap, 16, 20) == 0,
                  $"on: the threat gone, only the stink stays - none of the threat is left behind ({Value(dmap, 20, 20)})");

            // Taking the waste away.
            Clear(w);
            Cycle(dmap);
            Check(Units(w) == 0 && dmap.Map.GetAllSectors().Count == 0, "on: the waste taken away, the map clears and its sectors go");

            // Bold is the stance that ignores danger; it gets no stink.
            byStance.Clear();
            Put(w, waste, 3);
            var (bold, _) = NewColonistMap(ThreatStance.Bold);
            Cycle(bold);
            Check(bold.Map.GetAllSectors().Count == 0, "on: the Bold stance map gets no stink");
            byStance.Clear();

            // Switching it off mid-game takes the stink out at the next update.
            var (later, _) = NewColonistMap(ThreatStance.Cautious);
            Cycle(later);
            Check(Value(later, 22, 20) == 3, "on: the Cautious stance map gets it (3 items: 3)");
            Set("discomfort.wasteMap", "false");
            Cycle(later);
            Check(later.Map.GetAllSectors().Count == 0 && Units(w) == null, "switched off: the stink is gone at the next update");
            Set("discomfort.wasteMap", "true");
            Cycle(later);
            Check(Value(later, 22, 20) == 3 && Units(w) == 3, "switched on again: drawn again from the items, as after a load");
        }
        finally
        {
            foreach (var kv in before) Set(kv.Key, kv.Value);
            UWGame.The.Sim.PlaySite = oldSite;
            UWGame.The.Map = oldMap;
        }
        return failures;
    }

    private static Vector3 Centre(Point tile) => new Vector3(tile.X * 48 + 24, tile.Y * 48 + 24, 0);
}
