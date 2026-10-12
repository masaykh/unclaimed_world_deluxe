using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;

namespace UWGame.Mods;

/// <summary>
/// Fish traps fish down a stock, and the stock grows back.
///
/// THE REQUEST. Kastuk: "Fishing is too much reliable food source and traps can bring food
/// infinitely. Let respawning fish pool be reduced over time during every harvesting." Settled in
/// the thread: a stock PER TRAP; option B - it regrows all the time, fishing or not, at a fraction
/// of the catch rate, so an active trap slowly runs down and a rested one recovers; and the regrow
/// rate "may not be depended on type of current installed trap, but select best available for
/// current fishing place". "Don't use bait" is the existing off switch, so nothing new for that.
///
/// HOW TRAPS WORK TODAY. There is no fish population anywhere. Each trap runs the polled event
/// fishTrapSpawningLoop (PolledEventsLoader) every FishTrapSpawningLoopInterval seconds - 12, set
/// by initializeGlobalFishTrapProperties - and spawns up to maxNoOfFishToSpawnAtATime x random of
/// its fishType with probability spawnChance; all four are CustomFields on the structure type
/// (StructureLoader). The stick weir: item:carbonTail, 0.04, 5 - about 0.1 fish a poll.
///
/// WHAT THIS ADDS, on top of the studio's event rather than instead of it:
///   - a computed property, fishStockAvailable (Entity.AddExposedProperty), that regrows the trap's
///     stock for the game time since it was last read and returns it. The stock and its time live
///     in the trap's own CustomFields (fishStock, fishStockSeconds), which the save carries, so
///     there is no save-format change. A trap with no stock yet - a new one, or one already on the
///     map when the mod is switched on - starts full.
///   - one more condition on the event's spawn: fishStockAvailable >= amountOfFish;
///   - one more action after it: fishStock = fishStockAvailable - amountOfFish.
///
/// "BEST AVAILABLE FOR THE FISHING PLACE." Traps are built on anchors - terrain:fishTrapSpotCreek,
/// ...Coast, ...Shore - whose SharedSpecialActions name the placeFishTrap... processes, whose Outputs name
/// the trap. So a trap's place is known from the tables, and so is every trap that place accepts.
/// The stock's size and its regrowth are both measured against the best of those (highest
/// spawnChance x maxNoOfFishToSpawnAtATime / 2), so swapping a net weir for a stick weir neither
/// shrinks the fish nor slows their return.
///
/// FISHED-OUT WATER SHOWS NO FISH (<see cref="FishedOutLook"/>), a switch of its own. Kastuk asked
/// for this in Visually Harvested Resources, 2026-10-12: the decorative fish the maps place
/// (terrain:fishCircling, terrain:fishSwarm) keep circling over water that has been fished empty.
/// With it on, a decoration is not drawn while every fish source within <see cref="LookRange"/>
/// tiles is empty. A source is a tile holding a fish resource (minnows, alabaster ray, streak fin,
/// carbon tail: the resources drawn with the hook icon), or a trap whose stock is under one fish.
/// Traps count only with FISH TRAPS DEPLETE THE WATER on, since without it they never run out. A
/// decoration with no source in range at all is left alone. It is part of this mod's group but
/// does not need its switch: Kastuk, "not be depended on it". Drawing only: the simulation never
/// reads it, so replays and saves are unaffected.
/// </summary>
public static class FishStockMod
{
    public const string ModId = "fishstock";

    private static ModSetting enabled;
    private static ModSetting stockDays;
    private static ModSetting regrowth;

    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "FISH TRAPS DEPLETE THE WATER", defaultValue: false,
            toolTip: "Each fish trap fishes down a stock that grows back over time, instead of " +
                     "catching forever. Rest a trap with \"Don't use bait\" and it recovers.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    public static ModSetting StockDays =>
        stockDays ?? (stockDays = ModSettings.Choice(
            ModId, "stockDays", "FISH STOCK (DAYS OF CATCH)", new[] { "1", "2", "3", "5", "10" }, "3",
            toolTip: "How big a full stock is: this many days of what the best trap for that " +
                     "fishing place catches.",
            affectsSimulation: true));

    public static ModSetting Regrowth =>
        regrowth ?? (regrowth = ModSettings.Choice(
            ModId, "regrowth", "FISH REGROWTH (OF CATCH RATE)", new[] { "0.25", "0.5", "0.75", "1" }, "0.5",
            toolTip: "How fast the stock grows back, as a fraction of what the best trap for that " +
                     "place catches. Below 1, a trap that is always fishing slowly runs the water down.",
            affectsSimulation: true));

    private static ModSetting fishedOutLook;
    private static ModSetting lookRange;

    public static ModSetting FishedOutLook =>
        fishedOutLook ?? (fishedOutLook = ModSettings.Toggle(
            ModId, "fishedOutLook", "FISHED-OUT WATER SHOWS NO FISH", defaultValue: false,
            toolTip: "The fish seen circling in the water disappear while every fishing spot near " +
                     "them is fished out, and come back as the fish do. Traps count too when " +
                     "FISH TRAPS DEPLETE THE WATER is on.",
            affectsSimulation: false));

    public static ModSetting LookRange =>
        lookRange ?? (lookRange = ModSettings.Choice(
            ModId, "lookRange", "FISHING SPOTS CHECKED (TILES)", new[] { "3", "5", "10", "15", "20" }, "10",
            toolTip: "How far from the circling fish to look for fishing spots and traps that still " +
                     "have fish.",
            affectsSimulation: false));

    private static bool registered;

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Fish traps fish down a stock that grows back, so a trap left in one spot catches less and less until the water recovers.",
            "HUD_thumbnail_fishWeir");
        ModSettings.SetCategoryLabel(ModId, "FISH STOCK");
        _ = Enabled;
        _ = StockDays;
        _ = Regrowth;
        _ = FishedOutLook;
        _ = LookRange;
        if (!registered)
        {
            registered = true;
            Entity.AddExposedProperty(AvailableKey, GetStockAvailable);
        }
    }

    public const string AvailableKey = "fishStockAvailable";
    public const string StockKey = "fishStock";
    public const string SecondsKey = "fishStockSeconds";

    /// <summary>The studio's defaults for the poll interval, if the play site has not set one yet.</summary>
    private const float DefaultPollSeconds = 12f;

    // ---- the event -------------------------------------------------------------------------

    /// <summary>
    /// Called by BaseDataLoader.InitGlobalConditionalEvents with the table the game will run.
    /// Finds fishTrapSpawningLoop's spawning set - the one with the SpawnEntityAction - and gives
    /// it one more condition and one more action. Returns whether it did; false if the event is
    /// not as the studio wrote it, in which case nothing is touched.
    /// </summary>
    public static bool AdjustPolledEvents(List<PolledEventType> list)
    {
        if (!Enabled.On || list == null)
        {
            return false;
        }
        PolledEventType loop = list.FirstOrDefault(e => e.KeyName == "fishTrapSpawningLoop");
        ActionSetType spawning = loop?.ActionSets?.SetsOfActions?.FirstOrDefault(
            s => s.Actions != null && s.Actions.Any(a => a is SpawnEntityAction));
        if (spawning == null)
        {
            return false;
        }

        Condition hasStock = new CustomCondition
        {
            TargetObject = Source(),
            PropertyCondition = new PropertyCondition
            {
                PropertyKey = AvailableKey,
                NumberMinimumInclusive = new ValueNode { TargetObject = Source(), PropertyKey = "amountOfFish" }
            }
        };
        spawning.Condition = spawning.Condition == null
            ? hasStock
            : new ConditionFunction { Left = spawning.Condition, Operator = OperatorType.And, Right = hasStock };

        var takeFromStock = new SetPropertyAction("fishStockMod-takeFromStock")
        {
            DelayInSeconds = 0.0,
            TargetObject = Source(),
            PropertyKey = StockKey,
            Value = new FunctionNode
            {
                Left = new ValueNode { TargetObject = Source(), PropertyKey = AvailableKey },
                Operator = ExpressionOperator.Minus,
                Right = new ValueNode { TargetObject = Source(), PropertyKey = "amountOfFish" }
            }
        };
        spawning.Actions = spawning.Actions.Concat(new EventActionType[] { takeFromStock }).ToArray();
        return true;
    }

    private static TargetObject Source() => new TargetObject { TargetObjectType = TargetObjectType.PolledEventSource };

    // ---- the stock ---------------------------------------------------------------------------

    private static PropertyResult? GetStockAvailable(IHasExposedProperties presented, SharedKnowledge knowledge, IHasExposedProperties parent)
    {
        if (!(presented is Entity trap) || trap.CustomFields == null || !IsFishTrap(trap.EntityType) || The.Sim == null)
        {
            return null;
        }
        double now = The.Sim.TotalUnPausedGameTimeInSeconds;
        float stock = StockNow(trap, now);

        trap.CustomFields[StockKey] = new PropertyResult { NumberResult = stock };
        trap.CustomFields[SecondsKey] = new PropertyResult { NumberResult = (float)now };
        return new PropertyResult { NumberResult = stock };
    }

    /// <summary>
    /// The trap's stock regrown to <paramref name="now"/>, WITHOUT storing it. The event's property
    /// stores it; the decoration check must not, because it runs while drawing and a write there
    /// would make the simulation depend on what was on screen.
    /// </summary>
    private static float StockNow(Entity trap, double now)
    {
        float perPoll = BestCatchPerPoll(trap.EntityType);
        float pollSeconds = PollSeconds();
        float max = perPoll * (float)(DateAndTime.secondsPerDay / pollSeconds) * ParseNumber(StockDays.Value, 3f);
        float perSecond = perPoll / pollSeconds * ParseNumber(Regrowth.Value, 0.5f);

        float stock = trap.CustomFields.TryGetValue(StockKey, out PropertyResult s) && s.NumberResult.HasValue ? s.NumberResult.Value : max;
        double since = trap.CustomFields.TryGetValue(SecondsKey, out PropertyResult t) && t.NumberResult.HasValue ? t.NumberResult.Value : now;
        return Regrow(stock, since, now, max, perSecond);
    }

    // ---- the decorative fish ---------------------------------------------------------------

    /// <summary>The map-placed decorations this hides (TerrainFeatureLoader.AddBillboardAnimTerrain).</summary>
    public static readonly string[] DecorationKeys = { "terrain:fishCircling", "terrain:fishSwarm" };

    /// <summary>
    /// The fish caught in open water, minnows included (Kastuk: "Add minnows into list"). Named
    /// rather than found by the hook icon, which the imp eel shares: it lives in the muckroot
    /// canals on land, and counting its spots would keep fish circling over empty water.
    /// </summary>
    public static readonly string[] FishResourceKeys = { "minnowsLive", "alabasterRay", "streakFin", "carbonTail", "daggermouth" };

    public static bool IsFishResource(ResourceType type) =>
        type != null && Array.IndexOf(FishResourceKeys, type.KeyName) >= 0;

    /// <summary>How often the answer is worked out again, in real milliseconds. Drawing only.</summary>
    private const int RefreshMilliseconds = 2000;

    private static readonly Dictionary<Point, bool> hiddenAt = new Dictionary<Point, bool>();
    private static long refreshedAt = long.MinValue;
    private static MapManager refreshedFor;
    private static bool lookOn;
    private static int range;
    private static EntityType[] decorations = new EntityType[0];

    /// <summary>
    /// Called by RenderAsBillboard before it draws a billboard and its shadow: true skips both.
    /// Runs for every billboard every frame, so with the switch off it is one bool read, and the
    /// answer for a decoration is kept for <see cref="RefreshMilliseconds"/>.
    /// </summary>
    public static bool HidesDecoration(IKnownEntityData shown)
    {
        long tick = Environment.TickCount64;
        if (tick - refreshedAt > RefreshMilliseconds || refreshedFor != The.Map)
        {
            Refresh(tick);
        }
        if (!lookOn || shown == null || Array.IndexOf(decorations, shown.EntityType) < 0)
        {
            return false;
        }
        Point? at = shown.MapPosition;
        if (!at.HasValue)
        {
            return false;
        }
        if (!hiddenAt.TryGetValue(at.Value, out bool hidden))
        {
            hidden = The.Map?.TileMap != null
                     && !ShowsFish(at.Value, range, The.Map.mapTileWidth, The.Map.mapTileHeight, FishAt);
            hiddenAt[at.Value] = hidden;
        }
        return hidden;
    }

    private static void Refresh(long tick)
    {
        refreshedAt = tick;
        refreshedFor = The.Map;
        hiddenAt.Clear();
        lookOn = FishedOutLook.On;
        range = (int)ParseNumber(LookRange.Value, 10f);
        if (decorations.Length == 0 && GameData.Instance?.AllEntityTypes != null)
        {
            decorations = DecorationKeys
                .Select(k => GameData.Instance.AllEntityTypes.TryGetValue(k, out EntityType t) ? t : null)
                .Where(t => t != null).ToArray();
        }
    }

    /// <summary>
    /// Whether a decoration at <paramref name="at"/> should be drawn: no fish source within
    /// <paramref name="range"/> tiles (a square, as RegrowthMod's zone), or at least one with fish.
    /// <paramref name="fishAt"/> answers per tile: -1 for no fish source there, 0 for sources that
    /// are all empty, more than 0 for fish.
    /// </summary>
    public static bool ShowsFish(Point at, int range, int width, int height, Func<int, int, int> fishAt)
    {
        bool anySource = false;
        for (int x = Math.Max(0, at.X - range); x <= Math.Min(width - 1, at.X + range); x++)
        {
            for (int y = Math.Max(0, at.Y - range); y <= Math.Min(height - 1, at.Y + range); y++)
            {
                int fish = fishAt(x, y);
                if (fish > 0)
                {
                    return true;
                }
                anySource |= fish == 0;
            }
        }
        return !anySource;
    }

    /// <summary>The fish on one tile, for <see cref="ShowsFish"/>: its fish resources, and traps on it.</summary>
    private static int FishAt(int x, int y)
    {
        TerrainTile tile = The.Map.TileMap[x][y];
        int result = -1;
        if (tile.TileResources != null)
        {
            foreach (KeyValuePair<ResourceType, TileResourceContainer> held in tile.TileResources)
            {
                if (IsFishResource(held.Key))
                {
                    result = Math.Max(result, held.Value?.NoOfHarvestableItems ?? 0);
                }
            }
        }
        if (result <= 0 && Enabled.On && tile.EntitiesOnTile != null && The.Sim != null)
        {
            foreach (Entity e in tile.EntitiesOnTile)
            {
                if (e?.CustomFields != null && IsFishTrap(e.EntityType))
                {
                    result = Math.Max(result, (int)StockNow(e, The.Sim.TotalUnPausedGameTimeInSeconds));
                }
            }
        }
        return result;
    }

    /// <summary>The stock after regrowing from <paramref name="since"/> to <paramref name="now"/>, capped.</summary>
    public static float Regrow(float stock, double since, double now, float max, float perSecond)
    {
        double grown = stock + Math.Max(0.0, now - since) * perSecond;
        return (float)Math.Max(0.0, Math.Min(max, grown));
    }

    private static float PollSeconds()
    {
        PropertyResult? interval = The.Sim.PlaySite?.GetPropertyValue("FishTrapSpawningLoopInterval", null);
        float seconds = interval?.NumberResult ?? interval?.GetIntegerResult() ?? DefaultPollSeconds;
        return seconds > 0f ? seconds : DefaultPollSeconds;
    }

    // ---- the best trap for a place ---------------------------------------------------------

    private static readonly Dictionary<EntityType, float> bestPerPoll = new Dictionary<EntityType, float>();

    public static bool IsFishTrap(EntityType type) =>
        type?.CustomFields != null && type.CustomFields.TryGetValue("isFishTrap", out PropertyResult r) && r.BoolResult == true;

    /// <summary>A trap type's own average catch per poll: spawnChance x maxNoOfFishToSpawnAtATime / 2.</summary>
    public static float CatchPerPoll(EntityType trap)
    {
        if (!IsFishTrap(trap)) return 0f;
        float chance = trap.CustomFields.TryGetValue("spawnChance", out PropertyResult c) ? c.NumberResult ?? 0f : 0f;
        float most = trap.CustomFields.TryGetValue("maxNoOfFishToSpawnAtATime", out PropertyResult m) ? m.NumberResult ?? 0f : 0f;
        return chance * most / 2f;
    }

    /// <summary>
    /// The best catch per poll of any trap that can be built wherever <paramref name="trap"/> can:
    /// every anchor whose SharedSpecialActions build this trap, and every trap those anchors build.
    /// Falls back to the trap's own rate if it has no anchor in the tables.
    /// </summary>
    public static float BestCatchPerPoll(EntityType trap)
    {
        if (trap == null) return 0f;
        if (bestPerPoll.TryGetValue(trap, out float cached)) return cached;

        float best = CatchPerPoll(trap);
        foreach (EntityType place in PlacesFor(trap))
        {
            foreach (EntityType sibling in TrapsBuiltAt(place))
            {
                best = Math.Max(best, CatchPerPoll(sibling));
            }
        }
        bestPerPoll[trap] = best;
        return best;
    }

    /// <summary>The anchors - terrain features today - that can have this trap built on them.</summary>
    public static IEnumerable<EntityType> PlacesFor(EntityType trap) =>
        GameData.Instance.AllEntityTypes.Values.Where(t => TrapsBuiltAt(t).Contains(trap));

    /// <summary>The traps a place's shared special actions build.</summary>
    public static IEnumerable<EntityType> TrapsBuiltAt(EntityType place)
    {
        if (place?.SharedSpecialActions == null) yield break;
        foreach (Pair<string, bool> action in place.SharedSpecialActions)
        {
            if (!GameData.Instance.AllProcessTypes.TryGetValue(action.First ?? "", out ProcessType process) || process.Outputs == null)
                continue;
            foreach (Output output in process.Outputs)
            {
                if (output?.EntityTypeToCreate != null
                    && GameData.Instance.AllEntityTypes.TryGetValue(output.EntityTypeToCreate, out EntityType built)
                    && IsFishTrap(built))
                {
                    yield return built;
                }
            }
        }
    }

    private static float ParseNumber(string value, float fallback) =>
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) && f > 0f ? f : fallback;
}
