using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.Mods;

/// <summary>
/// A priority on every stockpile: HIGH ones fill first and are hauled to first, LOW ones last.
///
/// THE REQUEST. Kastuk, "Stockpile improvements", 2026-09-12: "Need selectable priority of
/// stockpiles to enforce storage things here by selected amount, then carry to other less
/// prioritized stockpiles (when overflow)." And 2026-10-10: "Let it affect priority of related
/// hauling jobs too, as we cannot set it in tasks window." "Like if I need to clean camp area out
/// of waste, I raise Compost pit stockpile priority to high and all tasks to hauling of waste will
/// be prioritised over other hauling tasks. Then I will reduce priority, so colonists will stop
/// moving waste first over everything else."
///
/// HOW. The stockpile window (StockpileWindow) gets a PRIORITY button - NORMAL, HIGH, LOW - on a
/// stockpile zone and on a storage structure alike: both keep their settings in the studio's
/// Stockpile, which now carries a Priority (saved since Snapshotter.Version.StockpilePriority,
/// set through the CreateStockpile command the window's OK sends).
/// 1. WHERE THINGS GO. HaulingJobManager scores every item-and-storage pair and hands each item
///    the best (ScoreCombo, SortCombos). With the mod on, a stockpile's score is lifted by its
///    tier (RankStorage): HIGH above every NORMAL, NORMAL above every LOW, and any stockpile above
///    loose ground storage near camp. Within a tier the studio's own score decides as before -
///    condition, vermin, distance. A full stockpile drops out of the pairs, so its items go to the
///    next tier down: the overflow Kastuk asked for. Trade offers are left alone.
/// 2. HOW URGENT. A haul to a HIGH stockpile is a HIGH job, a haul to a LOW one a LOW job
///    (PrioritiseHaul) - the same job priority the TASKS window sets, so EvaluateJob's modifier
///    applies (EvaluateHaulingJobs). A NORMAL stockpile leaves the TASKS window's hauling priority
///    as it is. Changing a stockpile's priority re-prioritises the hauls already planned to it
///    (OnStockpileChanged).
///
/// NOT DONE. Items already stored are not moved out of a LOW stockpile into a HIGH one when it has
/// room - the studio's planner leaves stored items where they are. Point 3 of the request, a
/// stockpile whose items are not used, is a separate step.
///
/// Off by default. While off, a stockpile's priority is kept but means nothing.
/// </summary>
public static class StockpilePriorityMod
{
    public const string ModId = "stockpilepriority";

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "STOCKPILE PRIORITY", defaultValue: false,
            toolTip: "A PRIORITY button in the stockpile window. HIGH stockpiles are filled first and " +
                     "hauling to them comes before other hauling; LOW ones are filled when the others are " +
                     "full, and hauling to them comes last.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "A priority on every stockpile: HIGH ones are filled and hauled to first, LOW ones only when the rest are full.",
            "HUD_thumbnail_storageholeYellowleaves");
        ModSettings.SetCategoryLabel(ModId, "STOCKPILE PRIORITY");
        _ = EnabledSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>
    /// How far apart the tiers are. The studio's score for a pair is at most about 1.1 (0.7 condition,
    /// 0.2 distance, 0.1 stockpile settings, 0.1 the current haul), so steps of 2 never overlap.
    /// </summary>
    public const double TierStep = 2.0;

    /// <summary>
    /// A storage pair's score with its stockpile's tier added: loose ground +0, LOW +2, NORMAL +4,
    /// HIGH +6. 0 stays 0 - it means the pair is not allowed - and trade offers keep theirs.
    /// </summary>
    public static double RankStorage(double score, Stockpile stockpile, bool isTradeOffer)
    {
        if (!Enabled || stockpile == null || isTradeOffer || score <= 0.0)
        {
            return score;
        }
        return score + TierStep * Tier(stockpile.Priority);
    }

    /// <summary>1 for LOW, 2 for NORMAL, 3 for HIGH.</summary>
    public static int Tier(Priority priority) => priority switch
    {
        Priority.High => 3,
        Priority.Low => 1,
        _ => 2,
    };

    /// <summary>
    /// A haul into storage takes a HIGH or LOW stockpile's priority; to a NORMAL one, or to no
    /// stockpile, the TASKS window's priority for hauling.
    /// </summary>
    public static void PrioritiseHaul(HaulingJobSpecificItem haul, Stockpile stockpile, EntityGroup owner)
    {
        if (!Enabled || haul == null || !haul.IsHaulJobToStorage || haul.IsToTradeOfferStorage)
        {
            return;
        }
        Priority? fromStockpile = PriorityFor(stockpile);
        if (fromStockpile.HasValue)
        {
            haul.Priority = fromStockpile.Value;
        }
        else if (owner?.Policy != null && haul.GetJobType() is JobType jobType)
        {
            haul.Priority = owner.Policy.GetPriority(jobType);
        }
    }

    /// <summary>The job priority a stockpile gives its hauls: HIGH or LOW, or none for NORMAL.</summary>
    public static Priority? PriorityFor(Stockpile stockpile) =>
        stockpile != null && (stockpile.Priority == Priority.High || stockpile.Priority == Priority.Low)
            ? stockpile.Priority
            : null;

    /// <summary>
    /// Called by CreateStockpile once a stockpile's settings are replaced: every haul already
    /// planned into it takes its new priority.
    /// </summary>
    public static void OnStockpileChanged(EntityGroup owner, Stockpile stockpile)
    {
        if (!Enabled || owner == null || stockpile == null)
        {
            return;
        }
        LookUp<EntityGroup, EntityGroupID>.IterateMembers(delegate(EntityGroup group)
        {
            if (group.HaulingJobs == null)
            {
                return;
            }
            foreach (Job job in group.HaulingJobs)
            {
                if (job is HaulingJobSpecificItem haul && haul.IsHaulJobToStorage && !haul.IsToTradeOfferStorage
                    && Destination(haul, owner) == stockpile)
                {
                    PrioritiseHaul(haul, stockpile, group);
                }
            }
        });
    }

    /// <summary>The stockpile a haul is headed for: its structure's, or the zone's it unloads in.</summary>
    private static Stockpile Destination(HaulingJobSpecificItem haul, EntityGroup owner)
    {
        if (haul.ToStorage.HasValue)
        {
            return owner.StructureStockpiles.TryGetValue(haul.ToStorage.Value.StorageEntity, out Stockpile stockpile) ? stockpile : null;
        }
        if (haul.ToLocation.HasValue && The.Map != null)
        {
            return The.Map.GetTile(MapManager.WorldPosToTile(haul.ToLocation.Value))?.GetStockpileZone(owner.GetAllegiance())?.Stockpile;
        }
        return null;
    }

    /// <summary>What the stockpile window's PRIORITY button reads.</summary>
    public static string Label(Priority priority) => priority switch
    {
        Priority.High => UWGame.Locale.Text("PRIORITY: HIGH"),
        Priority.Low => UWGame.Locale.Text("PRIORITY: LOW"),
        _ => UWGame.Locale.Text("PRIORITY: NORMAL"),
    };

    /// <summary>The next priority a click on the button sets: NORMAL, HIGH, LOW, NORMAL...</summary>
    public static Priority Next(Priority priority) => priority switch
    {
        Priority.Normal => Priority.High,
        Priority.High => Priority.Low,
        _ => Priority.Normal,
    };

    public static string ButtonToolTip => UWGame.Locale.Text("HIGH: filled first, and hauling here comes before other hauling. LOW: filled when the other stockpiles are full, and hauling here comes last. NORMAL: as the TASKS window says.");
}
