using System;
using System.IO;
using System.Linq;
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

        Console.WriteLine("==> Map Editor: Eraser, then Pencil (MapLoader.ChangeTerrainDepth)");
        CheckEraseThenRaise(Check);

        Console.WriteLine("==> Map Editor: terrain heights written as the loader reads them (MapManager.TerrainHeightsTexels)");
        CheckTerrainHeightsTexels(Check);

        Console.WriteLine("==> Map Editor: SAVE under a new name takes the map's pictures (MapManager.CopyMapPictures)");
        CheckCopyMapPictures(Check);

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

    /// <summary>
    /// Kastuk, 2026-10-06: once the Eraser had taken ground down to black, Pencil and Brush could
    /// not raise it. The Eraser lowers by 60 a step, they raise by at most 1, and the depth was
    /// unclamped - so a few seconds of erasing put it thousands of steps out of reach.
    /// </summary>
    private static void CheckEraseThenRaise(Action<bool, string> check)
    {
        The.Map = NewEditMap(1, 1);
        var terrain = new Terrain(new TerrainTile { TilePos = new TilePos(0, 0) });
        MapLoader.SetTerrainDepth(0f, terrain, 100f);
        for (int step = 0; step < 200; step++)
        {
            MapLoader.ChangeTerrainDepth(0f, terrain, 60f);
        }
        check(terrain.TerrainDepth == MapLoader.MaxTerrainDepth, $"200 Eraser steps stop at the deepest a heightmap holds, {MapLoader.MaxTerrainDepth} (got {terrain.TerrainDepth})");
        MapLoader.ChangeTerrainDepth(0f, terrain, -1f);
        check(terrain.TerrainDepth == MapLoader.MaxTerrainDepth - 1f, $"  then one Pencil click raises it by 1 (got {terrain.TerrainDepth})");
        for (int step = 0; step < 400; step++)
        {
            MapLoader.ChangeTerrainDepth(0f, terrain, -1f);
        }
        check(terrain.TerrainDepth == 0f, $"  and raising stops at 0 (got {terrain.TerrainDepth})");
        check(The.Map.TerrainHeightsEdited, "  the map is marked as having edited heights, for SAVE");
    }

    /// <summary>
    /// terrainHeights.png made from the map in memory must give back every subtile's depth when
    /// read the studio's way - MapLoader.ReadColor's arithmetic restated here rather than called,
    /// so a change to the loader shows. A 7x5 map, split and merged tiles, a depth per subtile.
    /// </summary>
    private static void CheckTerrainHeightsTexels(Action<bool, string> check)
    {
        const int width = 7, height = 5;
        MapManager map = NewEditMap(width, height);
        The.Map = map;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = new TerrainTile { TilePos = new TilePos(x, y) };
                if ((x + y) % 2 == 0)
                {
                    tile.Terrain = new Terrain(tile);
                    MapLoader.SetTerrainDepth(0f, tile.Terrain, ExpectedDepth(x, y, 0, 0));
                }
                else
                {
                    tile.TerrainSubtiles = new Terrain[3][];
                    for (int j = 0; j < 3; j++)
                    {
                        tile.TerrainSubtiles[j] = new Terrain[3];
                        for (int k = 0; k < 3; k++)
                        {
                            tile.TerrainSubtiles[j][k] = new Terrain(tile);
                            MapLoader.SetTerrainDepth(0f, tile.TerrainSubtiles[j][k], ExpectedDepth(x, y, j, k));
                        }
                    }
                }
                map.TileMap[x][y] = tile;
            }
        }

        Color[] texels = map.TerrainHeightsTexels(out int textureWidth, out int textureHeight);
        int wrong = 0;
        string firstWrong = null;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool split = (x + y) % 2 != 0;
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        // MapLoader.ReadColor, as the studio wrote it.
                        float x2 = (float)x + (float)j * 0.33f;
                        float y2 = (float)y + (float)k * 0.33f;
                        int index = (int)(x2 / (float)width * (float)textureWidth) + (int)(y2 / (float)height * (float)(textureHeight - 1)) * textureWidth;
                        int expected = split ? ExpectedDepth(x, y, j, k) : ExpectedDepth(x, y, 0, 0);
                        if (texels[index].R != expected)
                        {
                            wrong++;
                            firstWrong ??= $"tile ({x},{y}) subtile ({j},{k}) reads {texels[index].R}, not {expected}";
                        }
                    }
                }
            }
        }
        check(wrong == 0, "every subtile reads back its own depth" + (wrong == 0 ? "" : $" - {wrong} do not, e.g. {firstWrong}"));
        check(texels.All(t => t.A == 255), "  every texel of the picture is filled");
    }

    private static int ExpectedDepth(int x, int y, int j, int k) => (x * 37 + y * 11 + j * 3 + k * 7 + 5) % 256;

    /// <summary>
    /// Kastuk, 2026-10-06: a map saved under a new name had MapData.xml alone, and loading it threw
    /// "File not found: ...terrainHeights.png".
    /// </summary>
    private static void CheckCopyMapPictures(Action<bool, string> check)
    {
        string root = Path.Combine(Path.GetTempPath(), "uw-mapedit-" + Guid.NewGuid().ToString("N"));
        string from = Path.Combine(root, "c Micromap Test");
        string to = Path.Combine(root, "micromap_test_terrain");
        try
        {
            Directory.CreateDirectory(Path.Combine(from, "Soil"));
            Directory.CreateDirectory(Path.Combine(from, "Vegetation"));
            File.WriteAllText(Path.Combine(from, "MapData.xml"), "from");
            foreach (string picture in new[] { "terrainHeights.png", "moisture.png", "waterColors.png", "waterBottomTint.png", Path.Combine("Soil", "clay.png"), Path.Combine("Vegetation", "grass.png") })
            {
                File.WriteAllText(Path.Combine(from, picture), picture);
            }
            Directory.CreateDirectory(Path.Combine(to, "Soil"));
            File.WriteAllText(Path.Combine(to, "MapData.xml"), "to");
            File.WriteAllText(Path.Combine(to, "stale.png"), "stale");
            File.WriteAllText(Path.Combine(to, "Soil", "sand.png"), "stale");

            MapManager.CopyMapPictures(from, to);
            check(File.Exists(Path.Combine(to, "terrainHeights.png")), "the new map has terrainHeights.png, which the loader cannot do without");
            check(File.Exists(Path.Combine(to, "moisture.png")) && File.Exists(Path.Combine(to, "waterColors.png")) && File.Exists(Path.Combine(to, "waterBottomTint.png")), "  and moisture, water colours and water bottom tint");
            check(File.Exists(Path.Combine(to, "Soil", "clay.png")) && File.Exists(Path.Combine(to, "Vegetation", "grass.png")), "  and its soil and vegetation pictures");
            check(!File.Exists(Path.Combine(to, "stale.png")) && !File.Exists(Path.Combine(to, "Soil", "sand.png")), "  the pictures of the map it replaces are gone");
            check(File.ReadAllText(Path.Combine(to, "MapData.xml")) == "to", "  its MapData.xml is left to SaveMap");

            MapManager.CopyMapPictures(from, from + Path.DirectorySeparatorChar);
            check(File.Exists(Path.Combine(from, "terrainHeights.png")) && File.Exists(Path.Combine(from, "Soil", "clay.png")), "saving over the map's own folder leaves its pictures in place");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static MapManager NewEditMap(int width, int height)
    {
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = width;
        map.mapTileHeight = height;
        map.TileMap = new TerrainTile[width][];
        for (int x = 0; x < width; x++)
        {
            map.TileMap[x] = new TerrainTile[height];
        }
        return map;
    }
}
