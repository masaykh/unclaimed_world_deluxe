using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.Mods;

/// <summary>
/// Stink and noise near where a colonist sleeps cost Comfort.
///
/// THE REQUEST. Kastuk, "Discomfort", 2026-10-05:
/// 1. "Stink discomfort. Filthy items like all of Waste category, fertilisers, sulphur blocks and
///    powder, animal carcasses and fresh hides near (or inside, like in Compost pile or Hide rack)
///    sleeping houses will affect Comfort of colonists sleeping inside (and for ones sleeping
///    outside too)." "Let's make base distance between house exit and stinky items (or any other
///    structures containing that items) become 5 grids. Base affect on Comfort be 3% per every 5
///    units of items, but no more than 40%. Add modifiers for it in mod settings for testing."
///    "Need to add dialog lines to colonists, affected by this Filth discomfort, like "What is
///    this smell!?" and "It's hard to even breathe here"."
/// 2. "Same calculations, but for Noise discomfort. Production structures like Workbench, Smithy,
///    Workshop, Refinery and various Mines during working at them must produce noise, affecting
///    all sleeping colonists in range." "Need dialog lines too, like "Can you be quiet please...""
/// 3. "Dialog lines must be limited to 1-2 colonists at time".
/// And: "It may calculate affect on Comfort once per day during first of all sleeping attempts."
///
/// HOW. When a colonist lies down (GoalDoSleep.Activate) and has not yet been measured today, the
/// place is measured from its exit - the sleeping structure's AccessPoint, or where the colonist
/// lies when sleeping out. Within the distance (5 tiles):
/// - STINK: every filthy item on the ground, and every one inside a structure (a compost pile, a
///   hide rack, a storage hut, the sleeping house itself), one unit each.
/// - NOISE: every worker at a workbench, smithy, workshop, refinery or mine counts as 5 units - so
///   one busy workbench costs what 5 filthy items do. Kastuk asked for the same numbers for both.
/// Comfort lost = 3% for every whole 5 units, stink and noise together, at most 40%. It is kept on
/// the colonist (CustomFields "discomfort:day" / "discomfort:penalty", saved with them) and taken
/// off Intelligence.Comfort, the colonist's sum of comfort effects, which feeds their own comfort
/// rating and the colony's. It holds until the next day's first sleep, and lapses if a day passes
/// without one.
///
/// WHAT IT LEAVES ALONE, by default. The discomfort influence map (DiscomfortMap/ThreatMap): its
/// values mean THREAT - they block tiles and send colonists fleeing - so filth near a compost pit
/// is not written there unless WASTE ON THE DISCOMFORT MAP is on (below). Noise is measured at
/// lying down only, as asked; work that starts later in the night is not heard until the next day.
///
/// WASTE ON THE DISCOMFORT MAP (a second switch, off by default). Kastuk, "Discomfort", 2026-10-17:
/// "I see in source code the unused DiscomfortMap. I suppose, it's expected to draw map of Dirty
/// tiles with effect radius just like Threat map", and, told it would make colonists route around a
/// dirty camp: "Must try it as option, even if it will make them reluctant to move waste into
/// compost collectors." The studio's note on DiscomfortMap: "this map should have Fire and Threat
/// as children... and maybe also a map for general discomfort such as wetness, poison etc. The
/// discomfort map is the added result of its child maps!" (Its FireMap was never built - nothing
/// constructs one - so Threat is the only child the game has.) With the switch on, each of the
/// player's colonist discomfort maps (Normal and Cautious stance; Bold is the stance that ignores
/// danger, and animals have their own maps) gets a WASTE MAP drawn the way ThreatMap draws: every
/// tile with filth on it - the same items IsFilthy names, on the ground or inside a structure -
/// paints a circle of (reach + 1) per filthy item at its centre, one less per tile out
/// (Common.DistanceOctile), to nothing just past the reach (HOW FAR STINK AND NOISE REACH). What
/// that adds to the discomfort map is one point per whole or part (reach + 1) of stink, at most
/// WasteDiscomfortCap (4) - the stance-cautious colonist's lowest panic level and the most at which
/// a rest may start (AIConstants.HighestDiscomfortLevelForLeisureActivityToStart), a quarter of
/// what stops work (16) - so stink alone never makes anyone flee or down tools, but every step
/// across dirty ground costs 1-4 more (MovementMap adds the discomfort to the terrain cost), and
/// colonists route around it. At lying down, the stink is read off the waste map at the sleeping
/// place's exit: units = value / (reach + 1), so a filthy item at the exit is a unit and one at the
/// edge of the reach a sixth of one (at 5 tiles). Noise is still counted as above.
///
/// THE STINK OVERLAY (interface only, off by default): a STINK row beside THREATS in the overlay
/// panel above the minimap colours where the filth stinks - see DiscomfortModStinkOverlay.cs.
///
/// TALK. A colonist who loses comfort may say so, through the studio's own talk path (a
/// Conversation of one line, as TalkAction.Execute does): a stink line or a noise line,
/// whichever cost more. At most two colonists a day across the colony, and never one already in
/// a conversation. The line is picked from the colonist and the day, not from the game's random
/// generator, so replays are untouched by it.
///
/// A stink line says what smells (Kastuk, 2026-10-10: "May add more lines with hints, what's
/// smelling"): the most of the filth near the bed decides - fertiliser, guano and sulphur "Somebody
/// got call from the nature." (now and then the scientific one, see RareExcrementOneIn), rotten food "That's quite unappetizing.", carcasses and fresh hides
/// "Such a breath of the wild." Other waste keeps the two general lines. Rotten food and spoiled
/// meals stink too since then.
///
/// Off by default.
/// </summary>
public static partial class DiscomfortMod
{
    public const string ModId = "discomfort";

    private static ModSetting enabled;
    private static ModSetting distance;
    private static ModSetting percent;
    private static ModSetting cap;
    private static ModSetting talk;
    private static ModSetting wasteMap;
    private static ModSetting stinkOverlay;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "STINK AND NOISE COST SLEEPERS COMFORT", defaultValue: false,
            toolTip: "Filthy items (waste, rotten food, fertiliser, sulphur, carcasses, fresh hides) and busy " +
                     "workbenches, smithies, workshops, refineries and mines near where a colonist " +
                     "sleeps lower their Comfort for the day. Measured once a day, when they first lie down.",
            affectsSimulation: true));

    public static ModSetting DistanceSetting =>
        distance ?? (distance = ModSettings.Choice(
            ModId, "distance", "HOW FAR STINK AND NOISE REACH (TILES)", new[] { "3", "5", "8" }, "5",
            toolTip: "Measured from the exit of the place a colonist sleeps in.",
            affectsSimulation: true));

    public static ModSetting PercentSetting =>
        percent ?? (percent = ModSettings.Choice(
            ModId, "percent", "COMFORT LOST PER 5 FILTHY ITEMS (%)", new[] { "1", "3", "5", "10" }, "3",
            toolTip: "Each worker at a noisy workplace counts as 5 filthy items.",
            affectsSimulation: true));

    public static ModSetting CapSetting =>
        cap ?? (cap = ModSettings.Choice(
            ModId, "cap", "MOST COMFORT STINK AND NOISE CAN TAKE (%)", new[] { "20", "40", "60" }, "40",
            affectsSimulation: true));

    public static ModSetting TalkSetting =>
        talk ?? (talk = ModSettings.Toggle(
            ModId, "talk", "COLONISTS COMPLAIN ABOUT IT", defaultValue: true,
            toolTip: "\"What is this smell!?\" - at most two colonists a day."));

    public static ModSetting WasteMapSetting =>
        wasteMap ?? (wasteMap = ModSettings.Toggle(
            ModId, "wasteMap", "WASTE ON THE DISCOMFORT MAP", defaultValue: false,
            toolTip: "Filthy items are drawn on the colonists' discomfort map - the map they flee danger by - " +
                     "fading out over the stink's reach, and the comfort lost in sleep is read from it: an item " +
                     "at the door counts fully, one further off less. Colonists will also avoid dirty ground " +
                     "when they path, and may be slower to haul waste. Stink alone never makes them flee or stop work.",
            affectsSimulation: true));

    /// <summary>The STINK row in the overlay panel above the minimap - see DiscomfortModStinkOverlay.cs.</summary>
    public static ModSetting StinkOverlaySetting =>
        stinkOverlay ?? (stinkOverlay = ModSettings.Toggle(
            ModId, "stinkOverlay", "SHOW THE STINK OVERLAY", defaultValue: false,
            toolTip: "Colours the ground where filthy items stink, brown where a bed would lose comfort to it. " +
                     "Also the STINK row in the overlay panel above the minimap, beside THREATS.",
            affectsSimulation: false));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Stink and noise near where a colonist sleeps cost Comfort - waste, fresh hides and busy workshops are better kept away from the beds - and the colonists say so.",
            "HUD_thumbnail_compostHeap");
        ModSettings.SetCategoryLabel(ModId, "DISCOMFORT");
        _ = EnabledSetting;
        _ = DistanceSetting;
        _ = PercentSetting;
        _ = CapSetting;
        _ = TalkSetting;
        _ = WasteMapSetting;
        _ = StinkOverlaySetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>Whether filth is drawn on the colonists' discomfort maps.</summary>
    public static bool WasteMapOn => Enabled && WasteMapSetting.On;

    /// <summary>Whether the STINK overlay is drawn: DISCOMFORT on and its row ticked (DiscomfortModStinkOverlay.cs).</summary>
    public static bool StinkOverlayShown => Enabled && StinkOverlaySetting.On;

    /// <summary>Waste of every kind is filthy by its category.</summary>
    private const string WasteCategory = "waste";

    /// <summary>Filthy items that are not waste by category.</summary>
    private static readonly string[] FilthyKeys =
    {
        "item:organicFertilizer", "item:guanoFertilizer", "item:guano", "item:sulfurBlocks", "item:sulfurPowder",
    };

    /// <summary>Fresh hides: item:megapodGreenHide, item:whipjawGreenHide, item:thunderChickenGreenHide.</summary>
    private const string FreshHideSuffix = "GreenHide";

    /// <summary>Rotten food: item:rottenMeat, item:rottenStaple, item:rottenVegetables, item:spoiledMeal.</summary>
    private static readonly string[] RottenKeyPrefixes = { "item:rotten", "item:spoiled" };

    /// <summary>What a filthy item smells of: which line a sleeper says about it.</summary>
    public enum Smell { None, Waste, Excrement, Rotten, Wild }

    /// <summary>Workplaces that are loud while worked at, by part of their key.</summary>
    private static readonly string[] NoisyKeyParts = { "workbench", "smithy", "workshop", "refinery", "mine" };

    /// <summary>A mine you dig is loud; a land mine is not.</summary>
    private const string LandMine = "landmine";

    /// <summary>The units one worker at a noisy workplace counts as.</summary>
    public const int UnitsPerWorker = 5;

    private const float TileSize = 48f;

    private const string DayKey = "discomfort:day";
    private const string PenaltyKey = "discomfort:penalty";

    /// <summary>Colonists who have complained today, colony-wide; not saved - a load just allows two more.</summary>
    private static int complaintsDay = -1;
    private static int complaintsToday;
    public const int ComplaintsPerDay = 2;

    // Properties, not fields: a line is in the chosen language when it is said.
    private static string[] StinkLines => new[] { UWGame.Locale.Text("What is this smell!?"), UWGame.Locale.Text("It's hard to even breathe here.") };
    private static string[] NoiseLines => new[] { UWGame.Locale.Text("Can you be quiet please..."), UWGame.Locale.Text("How is anyone supposed to sleep with that racket?") };

    /// <summary>
    /// How often the rare excrement line comes up: once in this many. Kastuk, 2026-10-13: "Who is
    /// excrementing out there?" was "too much out of game style ... Let's change it to common
    /// 'Somebody got call from the nature'", and "Still want to use it: 'Feels like important
    /// scientific excrement got a breakthrough!' in rare cases". Complain picks a line by the
    /// sleeper and the day, so the common one is listed this many times less one.
    /// </summary>
    public const int RareExcrementOneIn = 6;

    /// <summary>The lines for the filth there is most of near the bed.</summary>
    public static string[] LinesFor(Smell smell) => smell switch
    {
        Smell.Excrement => ExcrementLines(),
        Smell.Rotten => new[] { UWGame.Locale.Text("That's quite unappetizing.") },
        Smell.Wild => new[] { UWGame.Locale.Text("Such a breath of the wild.") },
        _ => StinkLines,
    };

    private static string[] ExcrementLines()
    {
        string[] lines = new string[RareExcrementOneIn];
        for (int i = 0; i < lines.Length - 1; i++)
        {
            lines[i] = UWGame.Locale.Text("Somebody got call from the nature.");
        }
        lines[lines.Length - 1] = UWGame.Locale.Text("Feels like important scientific excrement got a breakthrough!");
        return lines;
    }

    /// <summary>Whether an item stinks.</summary>
    public static bool IsFilthy(EntityType type) => SmellOf(type) != Smell.None;

    /// <summary>What an item smells of; Smell.None for an item that does not stink.</summary>
    public static Smell SmellOf(EntityType type)
    {
        if (type?.ItemType == null)
        {
            return Smell.None;
        }
        string key = type.KeyName ?? "";
        foreach (string prefix in RottenKeyPrefixes)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
            {
                return Smell.Rotten;
            }
        }
        if (type.ItemType.CarcassType != null || key.EndsWith(FreshHideSuffix, StringComparison.Ordinal))
        {
            return Smell.Wild;
        }
        if (Array.IndexOf(FilthyKeys, key) >= 0)
        {
            return Smell.Excrement;
        }
        return type.CategoryKey == WasteCategory ? Smell.Waste : Smell.None;
    }

    /// <summary>Whether a structure is a loud workplace while worked at.</summary>
    public static bool IsNoisy(EntityType type)
    {
        if (type?.StructureType == null || type.KeyName == null)
        {
            return false;
        }
        string key = type.KeyName.ToLowerInvariant();
        if (key.Contains(LandMine))
        {
            return false;
        }
        foreach (string part in NoisyKeyParts)
        {
            if (key.Contains(part))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Comfort lost for so many units of stink and noise: <paramref name="percentPerFive"/>% for
    /// every whole 5, at most <paramref name="capPercent"/>%. A fraction, like the comfort effects.
    /// </summary>
    public static float PenaltyFor(int stinkUnits, int noiseUnits, int percentPerFive, int capPercent)
    {
        int fives = (Math.Max(0, stinkUnits) + Math.Max(0, noiseUnits)) / 5;
        return Math.Min(capPercent, fives * percentPerFive) / 100f;
    }

    /// <summary>Today, as a number that grows by one a day.</summary>
    private static int Today => The.Sim.DateAndTime.Year * 1000 + The.Sim.DateAndTime.Day;

    /// <summary>
    /// Called from GoalDoSleep.Activate as a colonist lies down in <paramref name="sleepingPlace"/>
    /// (the structure, or the ground they lie on). Measures the place once a day.
    /// </summary>
    public static void OnSleepStart(Entity sleeper, Entity sleepingPlace)
    {
        if (!Enabled || sleeper?.EntityType?.Person == null || The.Sim?.DateAndTime == null || The.Map?.TileMap == null)
        {
            return;
        }
        int today = Today;
        if (Number(sleeper, DayKey) == today)
        {
            return;
        }
        Vector3 exit = (sleepingPlace?.EntityType?.StructureType != null ? sleepingPlace.AccessPoint : null) ?? sleeper.PlaySiteLocation;
        MeasureSleep(sleeper.EntityType, exit, out int stink, out int noise, out Smell strongest);
        float penalty = PenaltyFor(stink, noise, ChoiceValue(PercentSetting, 3), ChoiceValue(CapSetting, 40));
        Entity.SetPropertyValue(ref sleeper.CustomFields, DayKey, new PropertyResult { NumberResult = today });
        Entity.SetPropertyValue(ref sleeper.CustomFields, PenaltyKey, new PropertyResult { NumberResult = penalty });
        if (penalty > 0f && TalkSetting.On)
        {
            Complain(sleeper, stink >= noise ? LinesFor(strongest) : NoiseLines, today);
        }
    }

    /// <summary>
    /// Called from the Intelligence.Comfort getter: the comfort this colonist has lost to stink and
    /// noise, measured today or yesterday; 0 otherwise.
    /// </summary>
    public static float Penalty(Entity colonist)
    {
        if (!Enabled || colonist?.CustomFields == null || The.Sim?.DateAndTime == null)
        {
            return 0f;
        }
        float? day = Number(colonist, DayKey);
        if (!day.HasValue || Today - day.Value > 1f)
        {
            return 0f;
        }
        return Number(colonist, PenaltyKey) ?? 0f;
    }

    /// <summary>
    /// What a <paramref name="sleeperType"/> lying down with its exit at <paramref name="exit"/>
    /// smells and hears: Measure, with the stink read off the waste map instead while WASTE ON THE
    /// DISCOMFORT MAP is on and the map has been drawn (it is, within one movement-map update of
    /// loading or switching on; until then, and for anyone who is not the player's, it is counted).
    /// </summary>
    public static void MeasureSleep(EntityType sleeperType, Vector3 exit, out int stink, out int noise, out Smell strongest)
    {
        Measure(exit, out stink, out noise, out strongest);
        if (WasteMapOn && WasteUnitsAt(sleeperType, exit) is int units)
        {
            stink = units;
        }
    }

    /// <summary>
    /// Filthy items and workers at loud workplaces within the distance of a point, and what most of
    /// the filth smells of.
    /// </summary>
    public static void Measure(Vector3 at, out int stink, out int noise, out Smell strongest)
    {
        int s = 0, n = 0;
        int[] bySmell = new int[Enum.GetValues(typeof(Smell)).Length];
        void Smelt(EntityType type)
        {
            Smell smell = SmellOf(type);
            if (smell != Smell.None)
            {
                s++;
                bySmell[(int)smell]++;
            }
        }
        float range = Reach * TileSize;
        int reach = (int)Math.Ceiling(range / TileSize);
        int cx = (int)(at.X / TileSize), cy = (int)(at.Y / TileSize);
        for (int tx = Math.Max(0, cx - reach); tx <= Math.Min(The.Map.mapTileWidth - 1, cx + reach); tx++)
        {
            for (int ty = Math.Max(0, cy - reach); ty <= Math.Min(The.Map.mapTileHeight - 1, cy + reach); ty++)
            {
                var onTile = The.Map.TileMap[tx][ty].EntitiesOnTile;
                if (onTile == null)
                {
                    continue;
                }
                foreach (Entity e in onTile)
                {
                    if (e?.Location == null || Vector2.Distance(new Vector2(at.X, at.Y), new Vector2(e.PlaySiteLocation.X, e.PlaySiteLocation.Y)) > range)
                    {
                        continue;
                    }
                    Smelt(e.EntityType);
                    e.Contains?.IterateContained(delegate(Entity inside)
                    {
                        Smelt(inside?.EntityType);
                    });
                    if (IsNoisy(e.EntityType))
                    {
                        n += UnitsPerWorker * WorkersAt(e);
                    }
                }
            }
        }
        stink = s;
        noise = n;
        strongest = Strongest(bySmell);
    }

    /// <summary>The smell there is most of; on a tie, the one named first in Smell.</summary>
    public static Smell Strongest(int[] bySmell)
    {
        Smell strongest = Smell.None;
        for (int i = 1; i < bySmell.Length; i++)
        {
            if (bySmell[i] > bySmell[(int)strongest])
            {
                strongest = (Smell)i;
            }
        }
        return strongest;
    }

    /// <summary>How many are working at a structure now (SimProcess.Workers of its processes).</summary>
    private static int WorkersAt(Entity structure)
    {
        if (structure.Processes == null)
        {
            return 0;
        }
        int workers = 0;
        foreach (SimProcessID id in structure.Processes)
        {
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(id);
            workers += process?.Workers?.Count ?? 0;
        }
        return workers;
    }

    /// <summary>One line, through a Conversation of one as TalkAction.Execute speaks; at most two colonists a day.</summary>
    private static void Complain(Entity sleeper, string[] lines, int today)
    {
        if (complaintsDay != today)
        {
            complaintsDay = today;
            complaintsToday = 0;
        }
        if (complaintsToday >= ComplaintsPerDay || sleeper.Intelligence == null || sleeper.Intelligence.CurrentConversationID.HasValue)
        {
            return;
        }
        complaintsToday++;
        string line = lines[(int)(((uint)sleeper.EntityID + (uint)today) % (uint)lines.Length)];
        Conversation conversation = new Conversation(1);
        conversation.SpeakLine(new TalkAction { TalkPriority = TalkAction.TalkActionPriority.Low }, sleeper);
        sleeper.Intelligence.SpeakLine(null, line, 4f, turnTowardsListeners: false, conversation);
    }

    /// <summary>How far stink and noise reach, in tiles.</summary>
    private static int Reach => ChoiceValue(DistanceSetting, 5);

    private static float? Number(Entity entity, string key) =>
        entity.CustomFields != null && entity.CustomFields.TryGetValue(key, out var stored) ? stored.NumberResult : null;

    private static int ChoiceValue(ModSetting setting, int fallback) =>
        int.TryParse(setting.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int v) ? v : fallback;

    // ---------------------------------------------------------------------------------------------
    // WASTE ON THE DISCOMFORT MAP
    //
    // The waste map is a child of a DiscomfortMap in the studio's sense - drawn like ThreatMap,
    // added sector by sector - but it is not in DiscomfortMap.Children, because Children is saved
    // (DiscomfortMap.DoSnapshot -> Dependence -> an IMapID), and a save naming a map type that a
    // build without this mod does not have would not load. It lives here instead, one per colonist
    // discomfort map, and is never saved: on load it is drawn again from the items, which the save
    // holds. Three hooks in DiscomfortMap let it take part: AddWasteSectors (GetSectorsToAdd phase),
    // AddWaste (AddChildMaps phase) and WasteKeepsSector (RemoveEmptySectors phase).
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The most stink adds to a tile of a discomfort map. AIConstants: a rest may start at 4 or less
    /// (HighestDiscomfortLevelForLeisureActivityToStart), work stops above 16
    /// (HighestDiscomfortLevelForWorkToContinue); a colonist flees above their PanicLevel, never
    /// below 4 (ThreatStance.Cautious). Read from the constants, so it stays under all three.
    /// </summary>
    public static int WasteDiscomfortCap =>
        Math.Min((int)ThreatStance.Cautious, Math.Min((int)GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart,
                                                      (int)GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue));

    /// <summary>A colonist discomfort map's waste map; not saved.</summary>
    private sealed class WasteMap
    {
        /// <summary>Whether its parent is one of the player's colonist maps (Normal or Cautious stance).</summary>
        public bool Applies;

        /// <summary>The stink: (reach + 1) per filthy item at its tile, one less a tile out. Never blocks.</summary>
        public TileLayer Map;

        /// <summary>The reach it was drawn with.</summary>
        public int Reach;

        /// <summary>The tiles with filth and how much, as last drawn; an unchanged list is not drawn again.</summary>
        public List<(Point Tile, int Count)> Sources;

        /// <summary>Re-add every sector the parent has, once: on first sight, after a load or a switch-on.</summary>
        public bool RefreshAll = true;
    }

    private static readonly ConditionalWeakTable<DiscomfortMap, WasteMap> wasteMaps = new ConditionalWeakTable<DiscomfortMap, WasteMap>();

    /// <summary>
    /// Called from DiscomfortMap.DoCycle (GetSectorsToAdd), after the studio has gathered the
    /// sectors its children changed (<paramref name="changed"/>, which it then re-adds) and removed
    /// (<paramref name="childRemoved"/>). Draws the waste map again if the filth moved, and adds to
    /// <paramref name="changed"/> what has to be re-added for it: the waste map's changed sectors,
    /// the sectors it dropped, and - since stink now keeps sectors alive - the sectors a threat map
    /// dropped, whose threat would otherwise stay. Switched off, re-adds everything once without it.
    /// Does nothing at all while off with no waste map drawn, so the studio's maps are untouched.
    /// </summary>
    public static void AddWasteSectors(DiscomfortMap map, HashSet<Point> changed, HashSet<Point> childRemoved)
    {
        if (map?.Map == null || changed == null)
        {
            return;
        }
        wasteMaps.TryGetValue(map, out WasteMap waste);
        if (!WasteMapOn)
        {
            if (waste != null)
            {
                wasteMaps.Remove(map);
                if (waste.Applies)
                {
                    foreach (Point sector in map.Map.GetAllSectors())
                    {
                        if (ChildHasSector(map, sector))
                        {
                            changed.Add(sector);
                        }
                    }
                }
            }
            return;
        }
        if (waste == null)
        {
            bool? applies = IsColonistMap(map);
            if (!applies.HasValue)
            {
                return;
            }
            waste = new WasteMap { Applies = applies.Value };
            wasteMaps.Add(map, waste);
        }
        if (!waste.Applies)
        {
            return;
        }
        Draw(waste, FindWaste(), Reach, out HashSet<Point> wasteChanged, out HashSet<Point> wasteRemoved);
        changed.UnionWith(wasteChanged);
        if (waste.RefreshAll)
        {
            waste.RefreshAll = false;
            changed.UnionWith(waste.Map.GetAllSectors());
            foreach (Point sector in map.Map.GetAllSectors())
            {
                if (ChildHasSector(map, sector))
                {
                    changed.Add(sector);
                }
            }
        }
        void ReAddIfKept(Point sector)
        {
            if (map.Map.GetSector(sector.X, sector.Y) != null && (waste.Map.GetSector(sector.X, sector.Y) != null || ChildHasSector(map, sector)))
            {
                changed.Add(sector);
            }
        }
        foreach (Point sector in wasteRemoved)
        {
            ReAddIfKept(sector);
        }
        if (childRemoved != null)
        {
            foreach (Point sector in childRemoved)
            {
                ReAddIfKept(sector);
            }
        }
    }

    /// <summary>
    /// Called from DiscomfortMap.AddSectors for each sector it re-adds, after its children: adds
    /// one point per whole or part (reach + 1) of stink, at most WasteDiscomfortCap. Runs on the
    /// studio's Parallel.ForEach; reads only this sector's waste and writes only <paramref name="own"/>.
    /// </summary>
    public static void AddWaste(DiscomfortMap map, TileSector own)
    {
        if (own == null || map == null || !wasteMaps.TryGetValue(map, out WasteMap waste) || !waste.Applies || waste.Map == null)
        {
            return;
        }
        TileSector stink = waste.Map.GetSector(own.Coords.X, own.Coords.Y);
        if (stink == null)
        {
            return;
        }
        int perPoint = waste.Reach + 1;
        int cap = WasteDiscomfortCap;
        for (int i = 0; i < own.TileArea.Width; i++)
        {
            byte[] column = own.Map[i];
            byte[] stinkColumn = stink.Map[i];
            for (int j = 0; j < own.TileArea.Height; j++)
            {
                int value = stinkColumn[j];
                if (value > 0)
                {
                    column[j] = (byte)Math.Min(255, column[j] + Math.Min(cap, (value + perPoint - 1) / perPoint));
                }
            }
        }
    }

    /// <summary>Called from DiscomfortMap.RemoveEmptySectors: a sector with stink in it is kept.</summary>
    public static bool WasteKeepsSector(DiscomfortMap map, int sectorX, int sectorY) =>
        map != null && wasteMaps.TryGetValue(map, out WasteMap waste) && waste.Applies && waste.Map?.GetSector(sectorX, sectorY) != null;

    /// <summary>
    /// The stink at a point, in units (filthy items at the point; further ones count less), off the
    /// waste map of the player's <paramref name="personType"/> colonists; null with none drawn.
    /// </summary>
    public static int? WasteUnitsAt(EntityType personType, Vector3 at)
    {
        WasteMap waste = DrawnWasteMap(personType);
        if (waste == null)
        {
            return null;
        }
        int x = Math.Clamp((int)(at.X / TileSize), 0, waste.Map.Width - 1);
        int y = Math.Clamp((int)(at.Y / TileSize), 0, waste.Map.Height - 1);
        return waste.Map.GetValue(x, y) / (waste.Reach + 1);
    }

    /// <summary>The drawn waste map of the player's <paramref name="personType"/> colonists (Normal stance, else Cautious), or null.</summary>
    private static WasteMap DrawnWasteMap(EntityType personType)
    {
        var maps = The.Sim?.PlaySite?.PlayerAllegiance?.SharedKnowledge?.PlaySiteKnowledge?.DiscomfortMaps;
        if (personType == null || maps == null || !maps.TryGetValue(ProtectionLevel.Exposed, out var byType)
            || !byType.TryGetValue(personType, out var byStance))
        {
            return null;
        }
        foreach (ThreatStance stance in new[] { ThreatStance.Normal, ThreatStance.Cautious })
        {
            if (byStance.TryGetValue(stance, out DiscomfortMap map) && map != null && wasteMaps.TryGetValue(map, out WasteMap waste)
                && waste.Applies && waste.Map != null)
            {
                return waste;
            }
        }
        return null;
    }

    /// <summary>
    /// Whether a discomfort map is one of the player's colonists' (a person type, Normal or
    /// Cautious stance); null while the player's maps are not there to look in.
    /// </summary>
    private static bool? IsColonistMap(DiscomfortMap map)
    {
        var maps = The.Sim?.PlaySite?.PlayerAllegiance?.SharedKnowledge?.PlaySiteKnowledge?.DiscomfortMaps;
        if (maps == null)
        {
            return null;
        }
        foreach (var byType in maps.Values)
        {
            foreach (var byStance in byType)
            {
                foreach (var stanceMap in byStance.Value)
                {
                    if (stanceMap.Value == map)
                    {
                        return byStance.Key?.Person != null && stanceMap.Key != ThreatStance.Bold;
                    }
                }
            }
        }
        return false;
    }

    private static bool ChildHasSector(DiscomfortMap map, Point sector) =>
        map.Children.Exists(d => d.Child?.Map?.GetSector(sector.X, sector.Y) != null);

    /// <summary>
    /// Every tile with filth, and how many filthy items: on the ground, and inside whatever stands
    /// there (a compost pile, a hide rack, a storage hut). What a colonist or an animal carries is
    /// left out - it would move the map with every haul. In tile order, so the same filth always
    /// gives the same list. Within <paramref name="area"/> (tiles) if given, else the whole map.
    /// </summary>
    private static List<(Point Tile, int Count)> FindWaste(Rectangle? area = null)
    {
        var found = new List<(Point Tile, int Count)>();
        MapManager map = The.Map;
        if (map?.TileMap == null)
        {
            return found;
        }
        Rectangle within = area ?? new Rectangle(0, 0, map.mapTileWidth, map.mapTileHeight);
        for (int x = Math.Max(0, within.Left); x < Math.Min(map.mapTileWidth, within.Right); x++)
        {
            for (int y = Math.Max(0, within.Top); y < Math.Min(map.mapTileHeight, within.Bottom); y++)
            {
                var onTile = map.TileMap[x][y]?.EntitiesOnTile;
                if (onTile == null || onTile.Count == 0)
                {
                    continue;
                }
                int count = 0;
                foreach (Entity e in onTile)
                {
                    if (e?.EntityType == null || e.EntityType.IntelligenceType?.IsMobile == true)
                    {
                        continue;
                    }
                    if (IsFilthy(e.EntityType))
                    {
                        count++;
                    }
                    e.Contains?.IterateContained(delegate(Entity inside)
                    {
                        if (IsFilthy(inside?.EntityType))
                        {
                            count++;
                        }
                    });
                }
                if (count > 0)
                {
                    found.Add((new Point(x, y), count));
                }
            }
        }
        return found;
    }

    /// <summary>
    /// Draws the waste map as ThreatMap.DoCycle draws a threat map, in one go: swap the buffers,
    /// clear, paint, drop the sectors nothing was painted in, mark the ones that differ from last
    /// time. Not drawn at all when the filth and the reach are as last time.
    /// </summary>
    private static void Draw(WasteMap waste, List<(Point Tile, int Count)> sources, int reach, out HashSet<Point> changed, out HashSet<Point> removed)
    {
        changed = new HashSet<Point>();
        removed = new HashSet<Point>();
        if (waste.Map != null && waste.Reach == reach && waste.Sources != null && SameSources(waste.Sources, sources))
        {
            return;
        }
        if (waste.Map == null)
        {
            waste.Map = new TileLayer((ushort)The.Map.mapTileWidth, (ushort)The.Map.mapTileHeight);
        }
        waste.Reach = reach;
        waste.Sources = sources;
        TileLayer layer = waste.Map;
        layer.BeginDrawing();
        layer.ClearMaps();
        foreach ((Point tile, int count) in sources)
        {
            // FalloffEachTile = count gives a radius of reach + 1 and count * (reach + 1 - distance).
            layer.DrawLinearInfluenceCircle(tile, count * (reach + 1), InfluenceMap.Operation.AddToExisting,
                InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.FalloffEachTile, count);
        }
        removed.UnionWith(layer.GetAllSectors());
        removed.ExceptWith(layer.AffectedSectors);
        layer.RemoveSectors(removed);
        layer.CompareOldAndNewSectors();
        layer.GetDirtySectors(ref changed);
    }

    private static bool SameSources(List<(Point Tile, int Count)> a, List<(Point Tile, int Count)> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }
        return true;
    }
}
