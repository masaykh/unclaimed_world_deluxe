using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UWGame.SimSide;

namespace UWGame.Mods;

/// <summary>
/// Procedural maps: writes an ordinary map folder - MapData.xml plus the PNG layers - that the
/// studio's MapLoader reads like any shipped map, so it opens in EDIT and plays through TEST MAP.
///
/// THE REQUEST. Kastuk and tripleacoder, "Procedural maps": generated maps, with tripleacoder's
/// order of work - geology, soil, biomes, then placement - and his correction that species belong
/// to an ECOSYSTEM, which belongs to a biome, so one biome can carry different species in
/// different places.
///
/// THE FORMAT (verified against MapLoader and all 21 shipped maps). A tile is 48 world units and
/// 3x3 subtiles. terrainHeights.png red = depth below the top of the land, per subtile: 0 is dry
/// land, 70 the shoreline, 71 and more is water (the water plane is at TerrainZLevel + 70). Land in
/// the shipped maps is almost all flat at 0 with a thin slope down to the shore - the relief comes
/// from terrain:* entities, not from the height field. Soil/NAME.png and Vegetation/NAME.png give
/// soil:NAME and veg:NAME amounts (red / (9 x 255) per subtile). Entities are EntityData with
/// Location in world units; a tree:* entity must carry a Tree element and nothing else may. A
/// Tile's resource KeyName must exist or the load throws.
///
/// ECOSYSTEMS ARE LEARNED, NOT INVENTED. Each shipped map is read and turned into a model of what
/// grows, lies and can be harvested in each SETTING - deep water, shallow water, shore, land, by
/// dominant soil - and how densely. A generated tile draws its entities, trees, resources and
/// vegetation from the model of its region's ecosystem, for its own setting. So every key is one
/// the game ships and loads, a riveraxle grows where riveraxles grow, and the density is the
/// studio's. A key the loaded tables do not define (three shipped ones) is dropped.
///
/// BIOMES are climate: rainfall, temperature and an "x factor" noise, each a range, with a minimum
/// radius so there are no micro-biomes (<see cref="Biomes"/>). A region of one biome picks one
/// ecosystem, weighted by how well the ecosystem's soils fit the biome, so two regions of the same
/// biome can differ in species.
///
/// Its own random generator (<see cref="MapGenRandom"/>), never the game's: a seed is a map, and
/// generating one changes nothing the simulation or a replay depends on.
///
/// Entry points: DataExport --generate-map NAME [--seed N] [--size N] writes into data/Maps, where
/// EDIT and TEST MAP list it; --mapgen-selftest generates and checks several.
/// </summary>
public static class MapGenMod
{
    public const string ModId = "mapgen";

    /// <summary>Tiles along a side: the shipped playable maps are 64, 80 and 128.</summary>
    public const int MinSize = 32, MaxSize = 256;

    /// <summary>TEST MAP puts its start expedition here (MapLoader), so it is kept dry and clear.</summary>
    public const int StartTileX = 15, StartTileY = 5;

    private const int Sub = 3;               // subtiles per tile side
    private const int TileWorld = 48;        // world units per tile side
    private const byte ShoreHeight = 70;     // the last land value; 71 and up is water

    // ------------------------------------------------------------------------------ biomes

    /// <summary>
    /// A biome type: the climate it occupies and the soils it favours. Rainfall, temperature and
    /// the x factor are each 0..1 fields over the map. The soils decide which learned ecosystems
    /// suit it: an ecosystem whose land is mostly these soils is the likelier pick.
    /// </summary>
    public sealed class BiomeType
    {
        public string Name;
        public double RainMin, RainMax, TempMin, TempMax, XMin, XMax;
        public int MinRadiusTiles;
        public string[] Soils;
    }

    /// <summary>
    /// The biome table. Data, in one place, so a mod can replace it before generating; a loader
    /// from a data file is the next step (tripleacoder: "the biome types should be placed in the
    /// data files").
    /// </summary>
    public static readonly List<BiomeType> Biomes = new List<BiomeType>
    {
        new BiomeType { Name = "riverlands", RainMin = 0.35, RainMax = 0.75, TempMin = 0.3, TempMax = 0.8, XMin = 0, XMax = 1, MinRadiusTiles = 10, Soils = new[] { "clay", "humus", "sand" } },
        new BiomeType { Name = "wetland", RainMin = 0.7, RainMax = 1, TempMin = 0.3, TempMax = 1, XMin = 0, XMax = 1, MinRadiusTiles = 8, Soils = new[] { "muckroot", "humus", "clay" } },
        new BiomeType { Name = "dry uplands", RainMin = 0, RainMax = 0.4, TempMin = 0.4, TempMax = 1, XMin = 0, XMax = 1, MinRadiusTiles = 10, Soils = new[] { "rocks", "rockssandstone", "sand", "limestone" } },
        new BiomeType { Name = "cold highlands", RainMin = 0.2, RainMax = 0.8, TempMin = 0, TempMax = 0.35, XMin = 0, XMax = 1, MinRadiusTiles = 10, Soils = new[] { "rocks", "limestone" } },
        new BiomeType { Name = "volcanic", RainMin = 0, RainMax = 1, TempMin = 0.5, TempMax = 1, XMin = 0.82, XMax = 1, MinRadiusTiles = 7, Soils = new[] { "vulcanic", "rocks" } },
    };

    // ------------------------------------------------------------------------------ result

    /// <summary>What was generated, for the report and the self-test.</summary>
    public sealed class Result
    {
        public string Folder;
        public int Size;
        public ulong Seed;
        public double WaterFraction;
        public int Entities, Trees, ResourceTiles;
        public List<string> Regions = new List<string>();
        public List<string> Ecosystems = new List<string>();

        public override string ToString() =>
            $"{Size}x{Size} seed {Seed}: water {WaterFraction:P0}, {Entities} terrain entities, {Trees} trees, " +
            $"{ResourceTiles} resource tiles, {Regions.Count} biome region(s): {string.Join("; ", Regions)}";
    }

    /// <summary>
    /// Generates a map into <paramref name="mapsDir"/>/<paramref name="name"/>, learning its
    /// ecosystems from the shipped maps in <paramref name="sourceMapsDir"/> (usually the same
    /// data/Maps). The type tables must be loaded (GameData.Instance), which is what keeps every
    /// written key one the game defines. Throws on bad arguments or no usable source map.
    /// </summary>
    public static Result Generate(string sourceMapsDir, string mapsDir, string name, ulong seed, int size)
    {
        if (size < MinSize || size > MaxSize)
        {
            throw new ArgumentException($"size {size}: must be {MinSize}..{MaxSize}");
        }
        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"map name '{name}' is not a folder name");
        }
        List<Ecosystem> ecosystems = LearnEcosystems(sourceMapsDir, name);
        if (ecosystems.Count == 0)
        {
            throw new InvalidOperationException($"no shipped map to learn from in {sourceMapsDir}");
        }
        var gen = new Generator(size, seed, ecosystems);
        gen.Run();
        string folder = Path.Combine(mapsDir, name);
        gen.Write(folder, name);
        gen.Result.Folder = folder;
        return gen.Result;
    }

    /// <summary>Generate and describe, for DataExport --generate-map.</summary>
    public static string GenerateMap(string sourceMapsDir, string mapsDir, string name, ulong seed, int size)
    {
        Result r = Generate(sourceMapsDir, mapsDir, name, seed, size);
        return $"{r.Folder}: {r}";
    }

    /// <summary>
    /// DataExport --mapgen-selftest: learns from <paramref name="sourceMapsDir"/>, generates four
    /// maps into <paramref name="workDir"/>, and checks each the way the game would read it - the
    /// studio's own MapData type through XmlSerializer, every key against the loaded tables, every
    /// position in range, a tree element on exactly the trees, a dry and connected start for TEST
    /// MAP - then that a seed gives byte-identical files twice and different seeds differ.
    /// Returns the number of failed checks.
    /// </summary>
    public static int SelfTest(string sourceMapsDir, string workDir, Action<string> log)
    {
        int failures = 0;
        void Check(bool ok, string what)
        {
            log((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        GameData data = GameData.Instance;
        Check(data?.AllEntityTypes != null && data.AllResourceTypes != null, "the type tables are loaded");
        if (failures > 0) return failures;
        List<Ecosystem> learned = LearnEcosystems(sourceMapsDir, null);
        Check(learned.Count >= 5, $"{learned.Count} shipped maps learned as ecosystems ({string.Join(", ", learned.Select(e => e.Name))})");
        if (learned.Count == 0) return failures;

        var cases = new (ulong seed, int size)[] { (1, 64), (2, 64), (3, 80), (4, 128) };
        foreach (var (seed, size) in cases)
        {
            string name = $"mapgen-selftest-{seed}";
            Result r;
            try
            {
                r = Generate(sourceMapsDir, workDir, name, seed, size);
            }
            catch (Exception ex)
            {
                Check(false, $"seed {seed}: generation threw {ex.GetType().Name}: {ex.Message}");
                continue;
            }
            log($"        {r}");
            CheckFolder(r.Folder, size, data, (ok, what) => Check(ok, $"seed {seed}: {what}"));   // Check counts
        }

        // Same seed and name, same bytes (into another folder); another seed, another map.
        string again = Generate(sourceMapsDir, Path.Combine(workDir, "again"), "mapgen-selftest-1", 1, 64).Folder;
        string first = Path.Combine(workDir, "mapgen-selftest-1");
        var differing = new List<string>();
        foreach (string f in Directory.GetFiles(first, "*", SearchOption.AllDirectories))
        {
            string other = Path.Combine(again, Path.GetRelativePath(first, f));
            if (!File.Exists(other) || !File.ReadAllBytes(f).AsSpan().SequenceEqual(File.ReadAllBytes(other))) differing.Add(Path.GetRelativePath(first, f));
        }
        Check(differing.Count == 0, "seed 1 twice gives byte-identical files" + (differing.Count > 0 ? " - differ: " + string.Join(", ", differing) : ""));
        Check(!File.ReadAllBytes(Path.Combine(first, "terrainHeights.png")).AsSpan()
                .SequenceEqual(File.ReadAllBytes(Path.Combine(workDir, "mapgen-selftest-2", "terrainHeights.png"))),
              "seeds 1 and 2 give different terrain");
        return failures;
    }

    private static void CheckFolder(string folder, int size, GameData data, Action<bool, string> Check)
    {
        foreach (string f in new[] { "MapData.xml", "terrainHeights.png", "moisture.png", "waterColors.png", "waterBottomTint.png" })
        {
            if (!File.Exists(Path.Combine(folder, f))) Check(false, $"{f} is missing");
        }
        UWGame.SimSide.Maps.MapEditor.MapData map;
        try
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(UWGame.SimSide.Maps.MapEditor.MapData));
            using var reader = new StreamReader(Path.Combine(folder, "MapData.xml"));
            map = (UWGame.SimSide.Maps.MapEditor.MapData)serializer.Deserialize(reader);
        }
        catch (Exception ex)
        {
            Check(false, $"MapData.xml does not read as the game's MapData: {ex.Message}");
            return;
        }
        Check(map.Dimensions.X == size && map.Dimensions.Y == size, $"Dimensions {map.Dimensions.X}x{map.Dimensions.Y}");

        double world = size * TileWorld;
        bool InMap(Microsoft.Xna.Framework.Vector3? v) => v.HasValue && v.Value.X >= 0 && v.Value.Y >= 0 && v.Value.X < world && v.Value.Y < world;
        int badEntity = 0, badTree = 0, outside = 0, inStart = 0;
        foreach (var ed in map.SavedMapEntities ?? new List<UWGame.SimSide.Maps.MapEditor.EntityData>())
        {
            if (!data.AllEntityTypes.TryGetValue(ed.EntityKey, out var type) || type.TreeType != null || ed.Tree != null) badEntity++;
            if (!InMap(ed.Location)) outside++;
            else if (Math.Abs((int)(ed.Location.Value.X / TileWorld) - StartTileX) <= 2 && Math.Abs((int)(ed.Location.Value.Y / TileWorld) - StartTileY) <= 2) inStart++;
        }
        foreach (var ed in map.Trees ?? new List<UWGame.SimSide.Maps.MapEditor.EntityData>())
        {
            if (!data.AllEntityTypes.TryGetValue(ed.EntityKey, out var type) || type.TreeType == null || ed.Tree == null
                || ed.Tree.ShapeFactor < 0.85f || ed.Tree.ShapeFactor > 1.15f) badTree++;
            if (!InMap(ed.Location)) outside++;
            else if (Math.Abs((int)(ed.Location.Value.X / TileWorld) - StartTileX) <= 2 && Math.Abs((int)(ed.Location.Value.Y / TileWorld) - StartTileY) <= 2) inStart++;
        }
        int entities = map.SavedMapEntities?.Count ?? 0, trees = map.Trees?.Count ?? 0;
        Check(badEntity == 0, $"{entities} terrain entities: every key defined, none a tree, none with a Tree element ({badEntity} bad)");
        Check(badTree == 0, $"{trees} trees: every key a tree type with a Tree element and a sane shape ({badTree} bad)");
        Check(outside == 0, $"every entity inside the {world}x{world} world ({outside} outside)");
        Check(inStart == 0, $"TEST MAP's start area is clear ({inStart} in it)");
        double per100 = 100.0 / (size * size);
        Check(entities * per100 >= 10 && entities * per100 <= 250 && trees * per100 >= 10 && trees * per100 <= 250,
              $"density {entities * per100:0} entities and {trees * per100:0} trees per 100 tiles (shipped: ~55 and ~70)");

        int badTile = 0;
        foreach (var tile in map.Tiles ?? new List<UWGame.SimSide.Maps.MapEditor.Tile>())
        {
            if (tile.Position.X < 0 || tile.Position.Y < 0 || tile.Position.X >= size || tile.Position.Y >= size) badTile++;
            foreach (var res in tile.Resources ?? Array.Empty<UWGame.SimSide.Maps.MapEditor.Resource>())
            {
                if (!data.AllResourceTypes.ContainsKey(res.KeyName) || res.MinResourceItems > res.MaxResourceItems) badTile++;
            }
        }
        Check(badTile == 0, $"{map.Tiles?.Count ?? 0} resource tiles: in range, every resource defined ({badTile} bad)");

        foreach (string f in Directory.GetFiles(Path.Combine(folder, "Soil"), "*.png"))
        {
            string key = "soil:" + Path.GetFileNameWithoutExtension(f);
            Check(data.AllSoilComponentTypes.ContainsKey(key), $"Soil/{Path.GetFileName(f)} is {key}");
        }
        foreach (string f in Directory.GetFiles(Path.Combine(folder, "Vegetation"), "*.png"))
        {
            string key = "veg:" + Path.GetFileNameWithoutExtension(f);
            Check(data.AllLowVegetationTypes.ContainsKey(key), $"Vegetation/{Path.GetFileName(f)} is {key}");
        }

        MapGenPng.Image heights = MapGenPng.Read(Path.Combine(folder, "terrainHeights.png"));
        int s = size * Sub;
        Check(heights.Width == s && heights.Height == s, $"terrainHeights {heights.Width}x{heights.Height}");
        int wet = 0;
        for (int i = 0; i < s * s; i++) if (heights.Rgba[i * 4] > ShoreHeight) wet++;
        double waterShare = (double)wet / (s * s);
        Check(waterShare >= 0.08 && waterShare <= 0.6, $"water {waterShare:P0} of the map (shipped playable maps: 12-52%)");
        int startCell = (StartTileY * Sub + 1) * s + StartTileX * Sub + 1;
        bool startDry = heights.Rgba[startCell * 4] <= ShoreHeight;
        Check(startDry, "TEST MAP's start tile is land");
        if (startDry)
        {
            var seen = new bool[s * s];
            var queue = new Queue<int>();
            queue.Enqueue(startCell);
            seen[startCell] = true;
            int reached = 0;
            while (queue.Count > 0)
            {
                int c = queue.Dequeue();
                reached++;
                int cx = c % s, cy = c / s;
                foreach (int nb in new[] { cx > 0 ? c - 1 : -1, cx < s - 1 ? c + 1 : -1, cy > 0 ? c - s : -1, cy < s - 1 ? c + s : -1 })
                {
                    if (nb >= 0 && !seen[nb] && heights.Rgba[nb * 4] <= ShoreHeight) { seen[nb] = true; queue.Enqueue(nb); }
                }
            }
            double share = (double)reached / Math.Max(1, s * s - wet);
            Check(share >= 0.3, $"{share:P0} of the land is reachable on foot from the start");
        }
    }

    // ============================================================================== learning

    /// <summary>
    /// The model of one shipped map: per SETTING, how many tiles it had, and on them how many of
    /// each terrain entity and tree, which resources and in what amounts, and how much of each
    /// vegetation. Plus the shares of its land soils, which match it to biomes.
    /// </summary>
    internal sealed class Ecosystem
    {
        public string Name;
        public Dictionary<string, int> Tiles = new Dictionary<string, int>();
        public Dictionary<string, Dictionary<string, int>> EntityCounts = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> TreeCounts = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, int>> ResourceTiles = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, List<(int min, int max)>> ResourceAmounts = new Dictionary<string, List<(int, int)>>();
        public Dictionary<string, Dictionary<string, double>> VegetationSums = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, List<XElement>> TreeSamples = new Dictionary<string, List<XElement>>();
        public Dictionary<string, double> LandSoilShares = new Dictionary<string, double>();
        public int EntityTotal;
    }

    private static readonly string[] SoilNames = { "clay", "sand", "vulcanic", "seabed", "deepseabed", "humus", "rocks", "rockssandstone", "limestone", "muckroot" };
    private static readonly string[] VegetationNames = { "muckrootthin", "firegrass", "greengrass", "billowgrass" };

    /// <summary>The setting of a tile: its water class, and on land its dominant soil.</summary>
    internal static string Setting(byte height, bool nearWater, string soil)
    {
        if (height > 150) return "deep";
        if (height > ShoreHeight) return "shallow";
        return (nearWater ? "shore|" : "land|") + (soil ?? "groundrock");
    }

    /// <summary>The next-coarser setting, for a tile whose exact setting an ecosystem never had.</summary>
    internal static string Coarser(string setting)
    {
        int bar = setting.IndexOf('|');
        return bar < 0 ? null : setting.Substring(0, bar);
    }

    private static List<Ecosystem> LearnEcosystems(string sourceMapsDir, string skipName)
    {
        var list = new List<Ecosystem>();
        if (!Directory.Exists(sourceMapsDir)) return list;
        foreach (string dir in Directory.GetDirectories(sourceMapsDir).OrderBy(d => d, StringComparer.Ordinal))
        {
            string name = Path.GetFileName(dir);
            if (name == skipName || File.Exists(Path.Combine(dir, GeneratedMarker))) continue;   // never learn from our own output
            try
            {
                Ecosystem e = Learn(dir);
                if (e != null) list.Add(e);
            }
            catch (Exception)
            {
                // A map that cannot be read is not a map to learn from; the others still are.
            }
        }
        return list;
    }

    /// <summary>Written into every generated folder, so a later generation does not learn from it.</summary>
    public const string GeneratedMarker = "generated-by-mapgen.txt";

    private static Ecosystem Learn(string dir)
    {
        string xmlPath = Path.Combine(dir, "MapData.xml");
        string heightPath = Path.Combine(dir, "terrainHeights.png");
        if (!File.Exists(xmlPath) || !File.Exists(heightPath)) return null;
        XDocument doc = XDocument.Load(xmlPath);
        XElement root = doc.Root;
        int w = (int?)root.Element("Dimensions")?.Element("X") ?? 0;
        int h = (int?)root.Element("Dimensions")?.Element("Y") ?? 0;
        List<XElement> entities = root.Element("SavedMapEntities")?.Elements("EntityData").ToList() ?? new List<XElement>();
        List<XElement> trees = root.Element("Trees")?.Elements("EntityData").ToList() ?? new List<XElement>();
        // Legacy-format maps (SavedMapEntity) load with nothing on them; too few to learn from.
        if (w < 16 || h < 16 || entities.Count + trees.Count < 200) return null;

        MapGenPng.Image heights = MapGenPng.Read(heightPath);
        var soils = new Dictionary<string, MapGenPng.Image>();
        foreach (string s in SoilNames)
        {
            string p = Path.Combine(dir, "Soil", s + ".png");
            if (File.Exists(p)) soils[s] = MapGenPng.Read(p);
        }
        var vegs = new Dictionary<string, MapGenPng.Image>();
        foreach (string v in VegetationNames)
        {
            string p = Path.Combine(dir, "Vegetation", v + ".png");
            if (File.Exists(p)) vegs[v] = MapGenPng.Read(p);
        }

        byte Sample(MapGenPng.Image img, int tx, int ty)
        {
            int px = Math.Min(img.Width - 1, (int)((tx + 0.5) / w * img.Width));
            int py = Math.Min(img.Height - 1, (int)((ty + 0.5) / h * img.Height));
            return img.R(px, py);
        }

        var e = new Ecosystem { Name = Path.GetFileName(dir) };
        var setting = new string[w, h];
        var water = new bool[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                water[x, y] = Sample(heights, x, y) > ShoreHeight;
        var landSoil = new Dictionary<string, int>();
        int landTiles = 0;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                byte ht = Sample(heights, x, y);
                bool near = false;
                for (int dx = -1; dx <= 1 && !near; dx++)
                    for (int dy = -1; dy <= 1 && !near; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h && water[nx, ny]) near = true;
                    }
                string dominant = null;
                int best = 1;
                foreach (var kv in soils)
                {
                    int r = Sample(kv.Value, x, y);
                    if (r > best) { best = r; dominant = kv.Key; }
                }
                string s = Setting(ht, near, dominant);
                setting[x, y] = s;
                Increment(e.Tiles, s);
                if (ht <= ShoreHeight)
                {
                    landTiles++;
                    Increment(landSoil, dominant ?? "groundrock");
                }
                foreach (var kv in vegs)
                {
                    Add(e.VegetationSums, s, kv.Key, Sample(kv.Value, x, y));
                }
            }
        }
        foreach (var kv in landSoil) e.LandSoilShares[kv.Key] = landTiles == 0 ? 0 : (double)kv.Value / landTiles;

        string SettingAt(XElement ed)
        {
            XElement loc = ed.Element("Location");
            if (loc == null) return null;
            double lx = ParseDouble(loc.Element("X")?.Value), ly = ParseDouble(loc.Element("Y")?.Value);
            int tx = Math.Clamp((int)(lx / TileWorld), 0, w - 1), ty = Math.Clamp((int)(ly / TileWorld), 0, h - 1);
            return setting[tx, ty];
        }
        foreach (XElement ed in entities)
        {
            string key = ed.Element("EntityKey")?.Value;
            string s = SettingAt(ed);
            if (key == null || s == null || ed.Element("Tree") != null) continue;
            Add(e.EntityCounts, s, key, 1);
            e.EntityTotal++;
        }
        foreach (XElement ed in trees)
        {
            string key = ed.Element("EntityKey")?.Value;
            string s = SettingAt(ed);
            XElement tree = ed.Element("Tree");
            if (key == null || s == null || tree == null) continue;
            Add(e.TreeCounts, s, key, 1);
            e.EntityTotal++;
            if (!e.TreeSamples.TryGetValue(key, out var samples)) e.TreeSamples[key] = samples = new List<XElement>();
            if (samples.Count < 64) samples.Add(new XElement(tree));
        }
        foreach (XElement tile in root.Element("Tiles")?.Elements("Tile") ?? Enumerable.Empty<XElement>())
        {
            int tx = (int?)tile.Element("Position")?.Element("X") ?? -1, ty = (int?)tile.Element("Position")?.Element("Y") ?? -1;
            if (tx < 0 || ty < 0 || tx >= w || ty >= h) continue;
            string s = setting[tx, ty];
            foreach (XElement res in tile.Element("Resources")?.Elements("Resource") ?? Enumerable.Empty<XElement>())
            {
                string key = res.Element("KeyName")?.Value;
                int? min = (int?)res.Element("MinResourceItems"), max = (int?)res.Element("MaxResourceItems");
                if (key == null || min == null || max == null) continue;   // Min without Max does nothing in the loader
                Add(e.ResourceTiles, s, key, 1);
                if (!e.ResourceAmounts.TryGetValue(key, out var amounts)) e.ResourceAmounts[key] = amounts = new List<(int, int)>();
                if (amounts.Count < 256) amounts.Add((min.Value, max.Value));
            }
        }
        return e;
    }

    private static void Increment(Dictionary<string, int> d, string key) => d[key] = d.TryGetValue(key, out int n) ? n + 1 : 1;

    private static void Add(Dictionary<string, Dictionary<string, int>> d, string outer, string inner, int n)
    {
        if (!d.TryGetValue(outer, out var m)) d[outer] = m = new Dictionary<string, int>();
        m[inner] = m.TryGetValue(inner, out int c) ? c + n : n;
    }

    private static void Add(Dictionary<string, Dictionary<string, double>> d, string outer, string inner, double n)
    {
        if (!d.TryGetValue(outer, out var m)) d[outer] = m = new Dictionary<string, double>();
        m[inner] = m.TryGetValue(inner, out double c) ? c + n : n;
    }

    private static double ParseDouble(string s) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0;

    // ============================================================================== generation

    private sealed class Generator
    {
        private readonly int n, s;               // tiles and subtiles per side
        private readonly MapGenRandom root;
        private readonly List<Ecosystem> ecosystems;
        public readonly Result Result = new Result();

        private bool[] water;                    // per subtile
        private byte[] height;                   // per subtile
        private int[] landDistance, waterDistance;
        private double[] rain, temperature, xFactor;   // per tile, 0..1
        private int[] biomeOf;                   // per tile, index into Biomes
        private int[] regionOf;                  // per tile
        private int[] lookTile;                  // per subtile: the tile whose biome and ecosystem it takes (warped)
        private readonly List<Ecosystem> regionEcosystem = new List<Ecosystem>();
        private readonly Dictionary<string, byte[]> soil = new Dictionary<string, byte[]>();      // per subtile
        private readonly Dictionary<string, byte[]> vegetation = new Dictionary<string, byte[]>();
        private string[] tileSetting;
        private readonly List<XElement> entities = new List<XElement>();
        private readonly List<XElement> trees = new List<XElement>();
        private readonly List<XElement> tiles = new List<XElement>();

        public Generator(int size, ulong seed, List<Ecosystem> ecosystems)
        {
            n = size;
            s = size * Sub;
            root = new MapGenRandom(seed);
            this.ecosystems = ecosystems;
            Result.Size = size;
            Result.Seed = seed;
        }

        public void Run()
        {
            Terrain(root.Fork("terrain"));
            Climate(root.Fork("climate"));
            BiomeRegions(root.Fork("biomes"));
            Borders(root.Fork("borders"));
            Soil(root.Fork("soil"));
            Settings();
            Vegetation(root.Fork("vegetation"));
            Place(root.Fork("placement"));
        }

        // -------------------------------------------------------------- 1. water and height

        private void Terrain(MapGenRandom rnd)
        {
            var noise = new MapGenNoise(rnd.Fork("elevation"));
            var warp = new MapGenNoise(rnd.Fork("warp"));
            double scale = 1.0 / (s * 0.35);
            var elevation = new double[s * s];
            // A coast along one edge on about half the seeds: the sea the shipped coastal maps have.
            int coastSide = rnd.Chance(0.5) ? rnd.Next(4) : -1;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    double e = noise.Fractal(x * scale, y * scale, 5);
                    if (coastSide >= 0)
                    {
                        double edge = coastSide switch { 0 => x, 1 => s - 1 - x, 2 => y, _ => s - 1 - y } / (double)s;
                        e -= Math.Max(0, 0.22 - edge) * 3.0;
                    }
                    elevation[y * s + x] = e;
                }
            }
            // Rivers: one or two noise-warped lines from edge to edge, carved to water.
            var river = new bool[s * s];
            int rivers = 1 + rnd.Next(2);
            for (int r = 0; r < rivers; r++)
            {
                bool vertical = rnd.Chance(0.5);
                double start = rnd.Range(0.2, 0.8) * s, end = rnd.Range(0.2, 0.8) * s;
                double width = rnd.Range(3.0, 6.0);
                double phase = rnd.Range(0, 1000);
                for (int t = 0; t < s; t++)
                {
                    double f = t / (double)(s - 1);
                    double c = start + (end - start) * f + warp.Fractal(phase + t * 0.006, phase, 2) * s * 0.22;
                    int lo = (int)(c - width), hi = (int)(c + width);
                    for (int k = lo; k <= hi; k++)
                    {
                        int x = vertical ? k : t, y = vertical ? t : k;
                        if (x >= 0 && y >= 0 && x < s && y < s) river[y * s + x] = true;
                    }
                }
            }
            // Sea level at a quantile: 12-30% of the map under noise water, before rivers.
            double target = rnd.Range(0.12, 0.30);
            double[] sorted = (double[])elevation.Clone();
            Array.Sort(sorted);
            double level = sorted[(int)(target * (sorted.Length - 1))];
            water = new bool[s * s];
            for (int i = 0; i < water.Length; i++) water[i] = elevation[i] < level || river[i];
            KeepStartDry();
            RemoveSpecks(water, 12);
            KeepStartDry();
            Fords();
            landDistance = DistanceTo(water, true);
            waterDistance = DistanceTo(water, false);
            height = new byte[s * s];
            int wet = 0;
            for (int i = 0; i < height.Length; i++)
            {
                if (water[i])
                {
                    wet++;
                    // 71 at the edge, down to 255 within a few subtiles: the shipped shelf.
                    height[i] = (byte)Math.Min(255, 71 + (waterDistance[i] - 1) * 34);
                }
                else
                {
                    // Flat land, sloping to the shoreline over the last tile.
                    int d = landDistance[i];
                    height[i] = d >= 4 ? (byte)0 : (byte)(ShoreHeight * (4 - d) / 4);
                }
            }
            Result.WaterFraction = (double)wet / height.Length;
        }

        private void KeepStartDry()
        {
            for (int ty = StartTileY - 3; ty <= StartTileY + 3; ty++)
                for (int tx = StartTileX - 3; tx <= StartTileX + 3; tx++)
                    for (int sy = 0; sy < Sub; sy++)
                        for (int sx = 0; sx < Sub; sx++)
                        {
                            int x = tx * Sub + sx, y = ty * Sub + sy;
                            if (x >= 0 && y >= 0 && x < s && y < s) water[y * s + x] = false;
                        }
        }

        /// <summary>
        /// Fords: every sizeable piece of land a river or lake cut off is joined to the start's land
        /// along the shortest crossing, three subtiles wide. Without it a colony can begin on a
        /// fraction of the map (seed 1 did: 29% of the land reachable).
        /// </summary>
        private void Fords()
        {
            for (int round = 0; round < 16; round++)
            {
                int[] label = LandComponents(out List<int> sizes);
                int start = label[(StartTileY * Sub + 1) * s + StartTileX * Sub + 1];
                int threshold = Math.Max(40, sizes.Sum() / 50);
                if (!sizes.Where((size, id) => id != start && size >= threshold).Any()) return;
                // Breadth-first from the start's land, through water, to the first sizeable other piece.
                var parent = new int[s * s];
                Array.Fill(parent, -2);
                var queue = new Queue<int>();
                for (int i = 0; i < label.Length; i++)
                {
                    if (label[i] == start) { parent[i] = -1; queue.Enqueue(i); }
                }
                int reached = -1;
                while (queue.Count > 0 && reached < 0)
                {
                    int c = queue.Dequeue();
                    foreach (int nb in Neighbours4(c % s, c / s))
                    {
                        if (parent[nb] != -2) continue;
                        parent[nb] = c;
                        if (label[nb] >= 0 && label[nb] != start && sizes[label[nb]] >= threshold) { reached = nb; break; }
                        if (label[nb] < 0) queue.Enqueue(nb);
                    }
                }
                if (reached < 0) return;
                for (int c = parent[reached]; c >= 0 && label[c] != start; c = parent[c])
                {
                    int cx = c % s, cy = c / s;
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int x = cx + dx, y = cy + dy;
                            if (x >= 0 && y >= 0 && x < s && y < s) water[y * s + x] = false;
                        }
                }
            }
        }

        /// <summary>A component id per land subtile (-1 for water), and each component's size.</summary>
        private int[] LandComponents(out List<int> sizes)
        {
            var label = new int[s * s];
            Array.Fill(label, -1);
            sizes = new List<int>();
            var queue = new Queue<int>();
            for (int i = 0; i < label.Length; i++)
            {
                if (water[i] || label[i] >= 0) continue;
                int id = sizes.Count, size = 0;
                label[i] = id;
                queue.Enqueue(i);
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    size++;
                    foreach (int nb in Neighbours4(c % s, c / s))
                    {
                        if (!water[nb] && label[nb] < 0) { label[nb] = id; queue.Enqueue(nb); }
                    }
                }
                sizes.Add(size);
            }
            return label;
        }

        /// <summary>Turns any connected patch of water or land smaller than <paramref name="min"/> subtiles into the other.</summary>
        private void RemoveSpecks(bool[] field, int min)
        {
            var seen = new bool[field.Length];
            var queue = new Queue<int>();
            var patch = new List<int>();
            for (int i = 0; i < field.Length; i++)
            {
                if (seen[i]) continue;
                bool kind = field[i];
                patch.Clear();
                queue.Enqueue(i);
                seen[i] = true;
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    patch.Add(c);
                    int cx = c % s, cy = c / s;
                    foreach (int nb in Neighbours4(cx, cy))
                    {
                        if (!seen[nb] && field[nb] == kind) { seen[nb] = true; queue.Enqueue(nb); }
                    }
                }
                if (patch.Count < min) foreach (int c in patch) field[c] = !kind;
            }
        }

        private IEnumerable<int> Neighbours4(int x, int y)
        {
            if (x > 0) yield return y * s + x - 1;
            if (x < s - 1) yield return y * s + x + 1;
            if (y > 0) yield return (y - 1) * s + x;
            if (y < s - 1) yield return (y + 1) * s + x;
        }

        /// <summary>For each cell not of <paramref name="target"/> kind, steps to the nearest one that is (0 for the target cells).</summary>
        private int[] DistanceTo(bool[] field, bool target)
        {
            var d = new int[field.Length];
            var queue = new Queue<int>();
            for (int i = 0; i < field.Length; i++)
            {
                if (field[i] == target) { d[i] = 0; queue.Enqueue(i); }
                else d[i] = int.MaxValue;
            }
            while (queue.Count > 0)
            {
                int c = queue.Dequeue();
                foreach (int nb in Neighbours4(c % s, c / s))
                {
                    if (d[nb] == int.MaxValue) { d[nb] = d[c] + 1; queue.Enqueue(nb); }
                }
            }
            for (int i = 0; i < d.Length; i++) if (d[i] == int.MaxValue) d[i] = s;
            return d;
        }

        private int TileLandDistance(int tx, int ty) => landDistance[(ty * Sub + 1) * s + tx * Sub + 1];

        private bool TileWater(int tx, int ty) => water[(ty * Sub + 1) * s + tx * Sub + 1];

        // -------------------------------------------------------------- 2. climate

        private void Climate(MapGenRandom rnd)
        {
            var rainNoise = new MapGenNoise(rnd.Fork("rain"));
            var tempNoise = new MapGenNoise(rnd.Fork("temperature"));
            var xNoise = new MapGenNoise(rnd.Fork("x"));
            double northSouth = rnd.Range(-0.25, 0.25);   // a gentle temperature gradient across the map
            double baseRain = rnd.Range(0.35, 0.65), baseTemp = rnd.Range(0.35, 0.65);
            rain = new double[n * n];
            temperature = new double[n * n];
            xFactor = new double[n * n];
            double scale = 1.0 / (n * 0.6);
            for (int ty = 0; ty < n; ty++)
            {
                for (int tx = 0; tx < n; tx++)
                {
                    int i = ty * n + tx;
                    double nearWater = Math.Max(0, 1 - TileLandDistance(tx, ty) / 24.0);
                    rain[i] = Clamp01(baseRain + rainNoise.Fractal(tx * scale, ty * scale, 3) * 0.45 + nearWater * 0.2);
                    temperature[i] = Clamp01(baseTemp + tempNoise.Fractal(tx * scale, ty * scale, 3) * 0.4 + northSouth * (ty / (double)n - 0.5));
                    xFactor[i] = Clamp01(0.5 + xNoise.Fractal(tx * scale * 1.5, ty * scale * 1.5, 2) * 0.9);
                }
            }
        }

        private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

        // -------------------------------------------------------------- 3. biomes, regions, ecosystems

        private void BiomeRegions(MapGenRandom rnd)
        {
            biomeOf = new int[n * n];
            for (int i = 0; i < biomeOf.Length; i++) biomeOf[i] = BestBiome(rain[i], temperature[i], xFactor[i]);
            // Merge regions under the biome's minimum area into their commonest neighbour, until none is left.
            for (int pass = 0; pass < 8; pass++)
            {
                bool changed = false;
                foreach (List<int> region in Regions(biomeOf))
                {
                    BiomeType b = Biomes[biomeOf[region[0]]];
                    double minArea = Math.PI * b.MinRadiusTiles * b.MinRadiusTiles;
                    if (region.Count >= minArea) continue;
                    var around = new Dictionary<int, int>();
                    var inRegion = new HashSet<int>(region);
                    foreach (int c in region)
                        foreach (int nb in TileNeighbours(c))
                            if (!inRegion.Contains(nb)) Increment(around, biomeOf[nb]);
                    if (around.Count == 0) continue;   // the whole map is one small region
                    int into = around.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First().Key;
                    foreach (int c in region) biomeOf[c] = into;
                    changed = true;
                }
                if (!changed) break;
            }
            regionOf = new int[n * n];
            int id = 0;
            foreach (List<int> region in Regions(biomeOf))
            {
                BiomeType b = Biomes[biomeOf[region[0]]];
                Ecosystem e = PickEcosystem(b, rnd);
                regionEcosystem.Add(e);
                foreach (int c in region) regionOf[c] = id;
                Result.Regions.Add($"{b.Name} ({region.Count} tiles) as {e.Name}");
                if (!Result.Ecosystems.Contains(e.Name)) Result.Ecosystems.Add(e.Name);
                id++;
            }
        }

        private static void Increment(Dictionary<int, int> d, int key) => d[key] = d.TryGetValue(key, out int v) ? v + 1 : 1;

        private int BestBiome(double r, double t, double x)
        {
            int best = 0;
            double bestScore = double.MaxValue;
            for (int i = 0; i < Biomes.Count; i++)
            {
                BiomeType b = Biomes[i];
                // Distance outside each range, weighted; 0 means inside all three.
                double score = Outside(r, b.RainMin, b.RainMax) + Outside(t, b.TempMin, b.TempMax) + 2 * Outside(x, b.XMin, b.XMax);
                // Among biomes that contain the point, prefer the narrower one (volcanic over the rest).
                score += 0.001 * ((b.RainMax - b.RainMin) + (b.TempMax - b.TempMin) + (b.XMax - b.XMin));
                if (score < bestScore) { bestScore = score; best = i; }
            }
            return best;
        }

        private static double Outside(double v, double min, double max) => v < min ? min - v : v > max ? v - max : 0;

        private Ecosystem PickEcosystem(BiomeType b, MapGenRandom rnd)
        {
            var weights = new double[ecosystems.Count];
            double total = 0;
            for (int i = 0; i < ecosystems.Count; i++)
            {
                double fit = b.Soils.Sum(soilName => ecosystems[i].LandSoilShares.TryGetValue(soilName, out double v) ? v : 0);
                weights[i] = 0.02 + fit * fit;
                total += weights[i];
            }
            double pick = rnd.NextDouble() * total;
            for (int i = 0; i < weights.Length; i++)
            {
                pick -= weights[i];
                if (pick <= 0) return ecosystems[i];
            }
            return ecosystems[ecosystems.Count - 1];
        }

        private IEnumerable<int> TileNeighbours(int c)
        {
            int x = c % n, y = c / n;
            if (x > 0) yield return c - 1;
            if (x < n - 1) yield return c + 1;
            if (y > 0) yield return c - n;
            if (y < n - 1) yield return c + n;
        }

        private IEnumerable<List<int>> Regions(int[] labels)
        {
            var seen = new bool[labels.Length];
            var list = new List<List<int>>();
            for (int i = 0; i < labels.Length; i++)
            {
                if (seen[i]) continue;
                var region = new List<int>();
                var queue = new Queue<int>();
                queue.Enqueue(i);
                seen[i] = true;
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    region.Add(c);
                    foreach (int nb in TileNeighbours(c))
                        if (!seen[nb] && labels[nb] == labels[i]) { seen[nb] = true; queue.Enqueue(nb); }
                }
                list.Add(region);
            }
            return list;
        }

        /// <summary>
        /// Biome borders as shapes, not staircases: each subtile takes its biome and ecosystem from a
        /// tile displaced by up to about four tiles of smooth noise. Regions are found per tile, so
        /// without this every border in soil, vegetation and placement followed the tile grid.
        /// </summary>
        private void Borders(MapGenRandom rnd)
        {
            var wx = new MapGenNoise(rnd.Fork("x"));
            var wy = new MapGenNoise(rnd.Fork("y"));
            double scale = 1.0 / (s * 0.1), amplitude = 4.0 * Sub;
            lookTile = new int[s * s];
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int ox = (int)Math.Round(x + wx.Fractal(x * scale, y * scale, 3) * amplitude);
                    int oy = (int)Math.Round(y + wy.Fractal(x * scale, y * scale, 3) * amplitude);
                    ox = Math.Clamp(ox, 0, s - 1);
                    oy = Math.Clamp(oy, 0, s - 1);
                    lookTile[y * s + x] = (oy / Sub) * n + ox / Sub;
                }
            }
        }

        // -------------------------------------------------------------- 4. soil

        private void Soil(MapGenRandom rnd)
        {
            var fields = new Dictionary<string, MapGenNoise>();
            foreach (string name in SoilNames) fields[name] = new MapGenNoise(rnd.Fork("soil:" + name));
            foreach (string name in SoilNames) soil[name] = new byte[s * s];
            double scale = 1.0 / (s * 0.12);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int i = y * s + x;
                    if (water[i])
                    {
                        soil[waterDistance[i] <= 4 ? "seabed" : "deepseabed"][i] = 255;
                        continue;
                    }
                    int t = lookTile[i];
                    Ecosystem e = regionEcosystem[regionOf[t]];
                    BiomeType b = Biomes[biomeOf[t]];
                    // Weight of each soil: half the ecosystem's own land shares, half the biome's liking,
                    // modulated by that soil's noise field; sand along the shore, clay on river banks.
                    string best = null, second = null;
                    double bestScore = 0, secondScore = 0;
                    foreach (string name in SoilNames)
                    {
                        if (name == "seabed" || name == "deepseabed") continue;
                        double w = (e.LandSoilShares.TryGetValue(name, out double share) ? share : 0) * 0.5
                                 + (Array.IndexOf(b.Soils, name) >= 0 ? 0.5 / b.Soils.Length : 0);
                        if (landDistance[i] <= 2 && name == "sand") w += 0.35;
                        if (landDistance[i] <= 5 && name == "clay") w += 0.12;
                        if (w <= 0) continue;
                        double score = w * (0.35 + 0.65 * (0.5 + 0.5 * fields[name].Fractal(x * scale, y * scale, 3)));
                        if (score > bestScore) { second = best; secondScore = bestScore; best = name; bestScore = score; }
                        else if (score > secondScore) { second = name; secondScore = score; }
                    }
                    if (best == null) continue;   // bare groundrock
                    double mix = second == null ? 0 : Math.Clamp((secondScore / bestScore - 0.7) / 0.3, 0, 1) * 0.45;
                    soil[best][i] = (byte)Math.Round(255 * (1 - mix));
                    if (second != null && mix > 0) soil[second][i] = (byte)Math.Round(255 * mix);
                }
            }
        }

        /// <summary>Each tile's setting, computed the way the ecosystems were learned.</summary>
        private void Settings()
        {
            tileSetting = new string[n * n];
            for (int ty = 0; ty < n; ty++)
            {
                for (int tx = 0; tx < n; tx++)
                {
                    int c = (ty * Sub + 1) * s + tx * Sub + 1;
                    bool near = false;
                    for (int dy = -1; dy <= 1 && !near; dy++)
                        for (int dx = -1; dx <= 1 && !near; dx++)
                        {
                            int nx = tx + dx, ny = ty + dy;
                            if (nx >= 0 && ny >= 0 && nx < n && ny < n && TileWater(nx, ny)) near = true;
                        }
                    string dominant = null;
                    int best = 1;
                    foreach (var kv in soil)
                        if (kv.Value[c] > best) { best = kv.Value[c]; dominant = kv.Key; }
                    tileSetting[ty * n + tx] = Setting(height[c], near, dominant);
                }
            }
        }

        // -------------------------------------------------------------- 5. vegetation

        private void Vegetation(MapGenRandom rnd)
        {
            var fields = new Dictionary<string, MapGenNoise>();
            foreach (string v in VegetationNames)
            {
                fields[v] = new MapGenNoise(rnd.Fork("veg:" + v));
                vegetation[v] = new byte[s * s];
            }
            double scale = 1.0 / (s * 0.08);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int i = y * s + x;
                    int t = (y / Sub) * n + x / Sub;
                    Ecosystem e = regionEcosystem[regionOf[lookTile[i]]];
                    string setting = tileSetting[t];
                    Dictionary<string, double> sums = Lookup(e.VegetationSums, setting, out string used);
                    if (sums == null) continue;
                    int tilesThere = e.Tiles.TryGetValue(used, out int nt) ? nt : 1;
                    foreach (string v in VegetationNames)
                    {
                        if (!sums.TryGetValue(v, out double sum)) continue;
                        double mean = sum / tilesThere;
                        double value = mean * (0.4 + 1.2 * (0.5 + 0.5 * fields[v].Fractal(x * scale, y * scale, 3)));
                        vegetation[v][i] = (byte)Math.Clamp(Math.Round(value), 0, 255);
                    }
                }
            }
        }

        /// <summary>The ecosystem's table for a setting, or for the coarser setting if it never had this one.</summary>
        private static T Lookup<T>(Dictionary<string, T> table, string setting, out string used) where T : class
        {
            used = setting;
            if (table.TryGetValue(setting, out T v)) return v;
            used = Coarser(setting);
            if (used == null) return null;
            // Coarser: merge every "land|*" (or "shore|*") setting's table - the first found is enough.
            foreach (var kv in table)
            {
                if (kv.Key.StartsWith(used + "|", StringComparison.Ordinal)) { used = kv.Key; return kv.Value; }
            }
            return null;
        }

        // -------------------------------------------------------------- 6. entities, trees, resources

        private void Place(MapGenRandom rnd)
        {
            GameData data = GameData.Instance;
            for (int ty = 0; ty < n; ty++)
            {
                for (int tx = 0; tx < n; tx++)
                {
                    int t = ty * n + tx;
                    bool startArea = Math.Abs(tx - StartTileX) <= 2 && Math.Abs(ty - StartTileY) <= 2;
                    Ecosystem e = regionEcosystem[regionOf[lookTile[(ty * Sub + 1) * s + tx * Sub + 1]]];
                    string setting = tileSetting[t];

                    Dictionary<string, int> ents = Lookup(e.EntityCounts, setting, out string usedE);
                    if (ents != null && !startArea)
                    {
                        int tilesThere = e.Tiles.TryGetValue(usedE, out int nt) ? nt : 1;
                        foreach (var kv in ents.OrderBy(k => k.Key, StringComparer.Ordinal))
                        {
                            int count = Poisson(rnd, (double)kv.Value / tilesThere);
                            for (int k = 0; k < count; k++)
                            {
                                if (!IsKnown(data, kv.Key, out bool isTree) || isTree) continue;
                                entities.Add(EntityElement(kv.Key, tx, ty, rnd, null));
                            }
                        }
                    }

                    Dictionary<string, int> tr = Lookup(e.TreeCounts, setting, out string usedT);
                    if (tr != null && !startArea)
                    {
                        int tilesThere = e.Tiles.TryGetValue(usedT, out int nt) ? nt : 1;
                        foreach (var kv in tr.OrderBy(k => k.Key, StringComparer.Ordinal))
                        {
                            int count = Poisson(rnd, (double)kv.Value / tilesThere);
                            for (int k = 0; k < count; k++)
                            {
                                if (!IsKnown(data, kv.Key, out bool isTree) || !isTree) continue;
                                List<XElement> samples = e.TreeSamples[kv.Key];
                                trees.Add(EntityElement(kv.Key, tx, ty, rnd, new XElement(samples[rnd.Next(samples.Count)])));
                            }
                        }
                    }

                    Dictionary<string, int> res = Lookup(e.ResourceTiles, setting, out string usedR);
                    if (res != null)
                    {
                        int tilesThere = e.Tiles.TryGetValue(usedR, out int nt) ? nt : 1;
                        var chosen = new List<XElement>();
                        foreach (var kv in res.OrderBy(k => k.Key, StringComparer.Ordinal))
                        {
                            if (!rnd.Chance(Math.Min(1.0, (double)kv.Value / tilesThere))) continue;
                            if (data?.AllResourceTypes != null && !data.AllResourceTypes.ContainsKey(kv.Key)) continue;
                            List<(int min, int max)> amounts = e.ResourceAmounts[kv.Key];
                            var (min, max) = amounts[rnd.Next(amounts.Count)];
                            chosen.Add(new XElement("Resource",
                                new XElement("KeyName", kv.Key),
                                new XElement("Modifier", 100),
                                new XElement("MinResourceItems", min),
                                new XElement("MaxResourceItems", max)));
                        }
                        if (chosen.Count > 0)
                        {
                            tiles.Add(new XElement("Tile",
                                new XElement("Position", new XElement("X", tx), new XElement("Y", ty)),
                                new XElement("Resources", chosen)));
                        }
                    }
                }
            }
            Result.Entities = entities.Count;
            Result.Trees = trees.Count;
            Result.ResourceTiles = tiles.Count;
        }

        /// <summary>Whether the loaded tables define the key, and whether it is a tree (which must carry a Tree element).</summary>
        private static bool IsKnown(GameData data, string key, out bool isTree)
        {
            isTree = key.StartsWith("tree:", StringComparison.Ordinal);
            if (data?.AllEntityTypes == null) return true;
            if (!data.AllEntityTypes.TryGetValue(key, out var type)) return false;
            isTree = type.TreeType != null;
            return true;
        }

        private static int Poisson(MapGenRandom rnd, double lambda)
        {
            if (lambda <= 0) return 0;
            double l = Math.Exp(-lambda), p = 1;
            int k = 0;
            do { k++; p *= rnd.NextDouble(); } while (p > l && k < 64);
            return k - 1;
        }

        private XElement EntityElement(string key, int tx, int ty, MapGenRandom rnd, XElement tree)
        {
            double x = tx * TileWorld + rnd.Range(0.5, TileWorld - 0.5);
            double y = ty * TileWorld + rnd.Range(0.5, TileWorld - 0.5);
            var e = new XElement("EntityData",
                new XElement("EntityKey", key),
                new XElement("Location",
                    new XElement("X", Format(x)),
                    new XElement("Y", Format(y)),
                    new XElement("Z", 0)),
                new XElement("FlipHorizontally", rnd.Chance(0.5) ? "true" : "false"));
            if (tree == null)
            {
                e.Add(new XElement("Bulk", 0));
            }
            else
            {
                e.Add(tree);
            }
            e.Add(new XElement("DeleteRecord", "false"));
            return e;
        }

        private static string Format(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        // -------------------------------------------------------------- 7. the folder

        public void Write(string folder, string name)
        {
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(Path.Combine(folder, "Soil"));
            Directory.CreateDirectory(Path.Combine(folder, "Vegetation"));
            MapGenPng.WriteGrey(Path.Combine(folder, "terrainHeights.png"), s, s, height);
            foreach (var kv in soil)
            {
                if (kv.Value.Any(b => b > 1)) MapGenPng.WriteGrey(Path.Combine(folder, "Soil", kv.Key + ".png"), s, s, kv.Value);
            }
            foreach (var kv in vegetation)
            {
                if (kv.Value.Any(b => b > 0)) MapGenPng.WriteGrey(Path.Combine(folder, "Vegetation", kv.Key + ".png"), s, s, kv.Value);
            }
            // Per-tile layers: moisture from rain and nearness to water; a soft water tint; a light lake bottom.
            var moisture = new byte[n * n];
            var waterColors = new byte[n * n * 4];
            var bottom = new byte[n * n * 3];
            for (int ty = 0; ty < n; ty++)
            {
                for (int tx = 0; tx < n; tx++)
                {
                    int t = ty * n + tx;
                    double near = Math.Max(0, 1 - TileLandDistance(tx, ty) / 12.0);
                    moisture[t] = (byte)Math.Round(255 * Clamp01(rain[t] * 0.6 + near * 0.4));
                    // A grey-blue tint at about a quarter opacity - within the shipped maps' 6-66%.
                    waterColors[t * 4] = 34; waterColors[t * 4 + 1] = 44; waterColors[t * 4 + 2] = 52;
                    waterColors[t * 4 + 3] = TileWater(tx, ty) ? (byte)60 : (byte)0;
                    bottom[t * 3] = 214; bottom[t * 3 + 1] = 236; bottom[t * 3 + 2] = 206;
                }
            }
            MapGenPng.WriteGrey(Path.Combine(folder, "moisture.png"), n, n, moisture);
            MapGenPng.WriteRgb(Path.Combine(folder, "waterBottomTint.png"), n, n, bottom);
            MapGenPng.WriteRgba(Path.Combine(folder, "waterColors.png"), n, n, waterColors);

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("MapData",
                    new XElement("Name", name),
                    new XElement("Dimensions", new XElement("X", n), new XElement("Y", n)),
                    new XElement("SavedMapEntities", entities),
                    new XElement("Trees", trees),
                    new XElement("Tiles", tiles)));
            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            using (XmlWriter w = XmlWriter.Create(Path.Combine(folder, "MapData.xml"), settings))
            {
                doc.Save(w);
            }
            File.WriteAllText(Path.Combine(folder, GeneratedMarker),
                $"Generated by MapGenMod: {Result}{Environment.NewLine}Ecosystems learned from: {string.Join(", ", ecosystems.Select(e => e.Name))}{Environment.NewLine}");
        }
    }
}
