using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
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
/// WHAT IT LEAVES ALONE. The discomfort influence map (DiscomfortMap/ThreatMap): its values mean
/// THREAT - they block tiles and send colonists fleeing - so filth near a compost pit is not
/// written there. Noise is measured at lying down only, as asked; work that starts later in the
/// night is not heard until the next day.
///
/// TALK. A colonist who loses comfort may say so, through the studio's own talk path (a
/// Conversation of one line, as TalkAction.Execute does): a stink line or a noise line,
/// whichever cost more. At most two colonists a day across the colony, and never one already in
/// a conversation. The line is picked from the colonist and the day, not from the game's random
/// generator, so replays are untouched by it.
///
/// Off by default.
/// </summary>
public static class DiscomfortMod
{
    public const string ModId = "discomfort";

    private static ModSetting enabled;
    private static ModSetting distance;
    private static ModSetting percent;
    private static ModSetting cap;
    private static ModSetting talk;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "STINK AND NOISE COST SLEEPERS COMFORT", defaultValue: false,
            toolTip: "Filthy items (waste, fertiliser, sulphur, carcasses, fresh hides) and busy " +
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

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "DISCOMFORT");
        _ = EnabledSetting;
        _ = DistanceSetting;
        _ = PercentSetting;
        _ = CapSetting;
        _ = TalkSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>Waste of every kind is filthy by its category.</summary>
    private const string WasteCategory = "waste";

    /// <summary>Filthy items that are not waste by category.</summary>
    private static readonly string[] FilthyKeys =
    {
        "item:organicFertilizer", "item:guanoFertilizer", "item:guano", "item:sulfurBlocks", "item:sulfurPowder",
    };

    /// <summary>Fresh hides: item:megapodGreenHide, item:whipjawGreenHide, item:thunderChickenGreenHide.</summary>
    private const string FreshHideSuffix = "GreenHide";

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

    /// <summary>Whether an item stinks.</summary>
    public static bool IsFilthy(EntityType type)
    {
        if (type?.ItemType == null)
        {
            return false;
        }
        if (type.CategoryKey == WasteCategory || type.ItemType.CarcassType != null || Array.IndexOf(FilthyKeys, type.KeyName) >= 0)
        {
            return true;
        }
        return type.KeyName != null && type.KeyName.EndsWith(FreshHideSuffix, StringComparison.Ordinal);
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
        Measure(exit, out int stink, out int noise);
        float penalty = PenaltyFor(stink, noise, ChoiceValue(PercentSetting, 3), ChoiceValue(CapSetting, 40));
        Entity.SetPropertyValue(ref sleeper.CustomFields, DayKey, new PropertyResult { NumberResult = today });
        Entity.SetPropertyValue(ref sleeper.CustomFields, PenaltyKey, new PropertyResult { NumberResult = penalty });
        if (penalty > 0f && TalkSetting.On)
        {
            Complain(sleeper, stink >= noise ? StinkLines : NoiseLines, today);
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

    /// <summary>Filthy items and workers at loud workplaces within the distance of a point.</summary>
    public static void Measure(Vector3 at, out int stink, out int noise)
    {
        int s = 0, n = 0;
        float range = ChoiceValue(DistanceSetting, 5) * TileSize;
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
                    if (IsFilthy(e.EntityType))
                    {
                        s++;
                    }
                    e.Contains?.IterateContained(delegate(Entity inside)
                    {
                        if (IsFilthy(inside?.EntityType))
                        {
                            s++;
                        }
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

    private static float? Number(Entity entity, string key) =>
        entity.CustomFields != null && entity.CustomFields.TryGetValue(key, out var stored) ? stored.NumberResult : null;

    private static int ChoiceValue(ModSetting setting, int fallback) =>
        int.TryParse(setting.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int v) ? v : fallback;
}
