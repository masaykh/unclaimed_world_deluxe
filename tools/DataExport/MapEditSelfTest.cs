using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;

namespace UW.Tools.DataExport;

/// <summary>
/// --mapedit-selftest: the Map Editor's terrain height edit, on a small map with no window.
///
/// WHY. The terrain height Pencil and Brush crashed the editor on the first click (Kastuk,
/// 2026-10-03), in MapLoader.ReduceSubdivisionScanVertically. RecomputeSubdivision was a
/// whole-map pass the studio turned into an area pass, and two of its limits kept the area's
/// SIZE where they needed its far EDGE - right only for an area at the map's corner, which is
/// the one the map loader passes. So the check is the same edit at the corner and away from it.
///
/// HOW. A 16x16 map of plain tiles (one Terrain each, as the loader leaves land away from a
/// coast), water on the left. Then what SidePanelEditorTerrainHeight.AffectMap does around its
/// height change: MapLoader.CreateTerrainSubtiles on a 4x4 tile area with a coast through it,
/// then MapLoader.RecomputeSubdivision on it. The coast tile and the land tile beside it must
/// keep their subtiles, the open water must be merged back, and nothing outside the area may
/// change.
/// </summary>
internal static partial class Program
{
    private const int EditMapSize = 16;

    private static int MapEditSelfTest()
    {
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        // Every Terrain registers itself in a lookup the Sim's constructor creates.
        The.Sim = new Sim { Mode = Sim.EngineMode.Edit };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(The.Sim, null);

        Console.WriteLine("==> Map Editor: terrain height edit (MapLoader.RecomputeSubdivision)");
        // Each row: one character per tile across the area, S = keeps its subtiles, . = merged.
        // Water fills the first two columns, the coast is the third, land the fourth.
        const string expected = "..SS";
        string atCorner = EditTerrainHeight(new Point(0, 0), out string cornerError);
        Check(cornerError == null, "an edit at the map's corner (0,0) completes" + (cornerError == null ? "" : ": " + cornerError));
        Check(atCorner == expected, "  at the corner, each row reads " + expected + " (got " + atCorner + ")");

        foreach (Point origin in new[] { new Point(8, 8), new Point(0, 9), new Point(9, 0), new Point(12, 12) })
        {
            string pattern = EditTerrainHeight(origin, out string error);
            Check(error == null, $"an edit at ({origin.X},{origin.Y}) completes" + (error == null ? "" : ": " + error));
            if (error == null)
            {
                Check(pattern == expected, $"  at ({origin.X},{origin.Y}), each row reads {expected}, as at the corner (got {pattern})");
            }
        }

        Console.WriteLine(failures == 0 ? "mapedit self-test OK" : $"mapedit self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Runs the edit on a fresh map with its 4x4 area at <paramref name="origin"/>, and returns
    /// which of the area's tiles kept their subtiles, if every row agrees; null and an error
    /// otherwise.
    /// </summary>
    private static string EditTerrainHeight(Point origin, out string error)
    {
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = EditMapSize;
        map.mapTileHeight = EditMapSize;
        map.TileMap = new TerrainTile[EditMapSize][];
        for (int x = 0; x < EditMapSize; x++)
        {
            map.TileMap[x] = new TerrainTile[EditMapSize];
            for (int y = 0; y < EditMapSize; y++)
            {
                var tile = new TerrainTile { TilePos = new TilePos(x, y) };
                // Water left of the coast column, land from it on. SetTerrainDepth is how the
                // editor turns a depth into LevelBelowWater and a surface type.
                tile.Terrain = new Terrain(tile);
                MapLoader.SetTerrainDepth(0f, tile.Terrain, x < origin.X + 2 ? 50f : -50f);
                map.TileMap[x][y] = tile;
            }
        }
        The.Map = map;

        var area = new Rectangle(origin.X, origin.Y, 4, 4);
        try
        {
            MapLoader.CreateTerrainSubtiles(area);
            MapLoader.RecomputeSubdivision(area);
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + " in " + ex.TargetSite?.Name;
            return null;
        }

        for (int x = 0; x < EditMapSize; x++)
        {
            for (int y = 0; y < EditMapSize; y++)
            {
                TerrainTile tile = map.TileMap[x][y];
                bool split = tile.TerrainSubtiles != null;
                if (!area.Contains(x, y) && (split || tile.Terrain == null))
                {
                    error = $"tile ({x},{y}) is outside the area and was changed";
                    return null;
                }
                if (split == (tile.Terrain != null))
                {
                    error = $"tile ({x},{y}) has {(split ? "both" : "neither")} a Terrain and subtiles";
                    return null;
                }
            }
        }

        string first = null;
        for (int y = area.Top; y < area.Bottom; y++)
        {
            var row = new StringBuilder();
            for (int x = area.Left; x < area.Right; x++)
            {
                row.Append(map.TileMap[x][y].TerrainSubtiles != null ? 'S' : '.');
            }
            if (first == null)
            {
                first = row.ToString();
            }
            else if (row.ToString() != first)
            {
                error = $"row {y} reads {row}, row {area.Top} {first}";
                return null;
            }
        }
        error = null;
        return first;
    }
}
