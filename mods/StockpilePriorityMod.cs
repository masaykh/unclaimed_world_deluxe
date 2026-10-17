using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
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
/// 3. WHAT A COLONIST HAS ON THEM IS NOT HURRIED. Kastuk, 2026-10-12: with a tools stockpile on HIGH
///    "all colonists [come] to store every equipment, take it back, drop, pick up and store again".
///    Picking an item up plans its haul to storage at once (Item.PickupByEntity calls
///    CreateHaulingJobsForAllCarriedItemsOutOfBand), and the planner plans one for every carried
///    item no job holds. While a job uses a tool it is held (ProcessJob.AssignTool,
///    CompositeGoal.SetLocksOnToolsOrWeapons); the moment the job ends it is free, and a HIGH haul
///    is worth twice a NORMAL one (AIConstants.HighJobModifier) - enough to beat the next work job.
///    So the colonist walked the tool to the stockpile, the next job fetched it back, and round
///    again. A haul of an item a colonist carries now keeps the TASKS window's priority, as in the
///    studio's game (PriorityFor): put away when there is nothing better to do. Loose items, and
///    items in other storage, are still hauled to a HIGH stockpile first.
/// 4. AN ITEM ITS STOCKPILE NO LONGER ALLOWS LEAVES. Kastuk, 2026-10-12: waste unchecked in a hut's
///    stockpile stayed in the hut. The studio's planner does score it out - the hut scores nothing
///    for it (ScoreStockpileSettings) - but before scoring it drops every item of a type a process
///    is waiting for (ItemTypeIsNeededForProcess: "don't haul items that may be needed for
///    processes!"), and organic matter is compost fertilizer's input. While any such job waits for
///    an item, which can be hours, no item of the type moves anywhere. An item stored where its
///    stockpile does not allow it is exempt from that, and so is the haul taking it out (MustLeave).
///
/// NOT DONE. Point 3 of the request, a stockpile whose items are not used, is a separate step. An
/// item lying on a stockpile ZONE that does not allow it may still stay: the planner's loose
/// ground storage near camp covers zone tiles too, and an item scores best on the tile it is
/// already on (IsCurrentlyInThisStorage).
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
    /// A haul into storage takes a HIGH or LOW stockpile's priority (PriorityFor); to a NORMAL one,
    /// or to no stockpile, the TASKS window's priority for hauling. Called as each haul is planned,
    /// so it also remembers a haul that takes an item out of a stockpile that does not allow it
    /// (MustLeave).
    /// </summary>
    public static void PrioritiseHaul(HaulingJobSpecificItem haul, Stockpile stockpile, EntityGroup owner, IKnownEntityData item)
    {
        if (!Enabled || haul == null || !haul.IsHaulJobToStorage || haul.IsToTradeOfferStorage)
        {
            return;
        }
        if (IsStoredAgainstItsStockpile(owner, item))
        {
            RememberEviction(haul);
        }
        Priority? fromStockpile = PriorityFor(stockpile, item);
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
    /// The job priority a haul of this item to this stockpile takes: the stockpile's, except that
    /// HIGH does not hurry an item a colonist has on them (point 3) - that one is put away at the
    /// TASKS window's priority, as the studio had it.
    /// </summary>
    public static Priority? PriorityFor(Stockpile stockpile, IKnownEntityData item)
    {
        Priority? priority = PriorityFor(stockpile);
        return priority == Priority.High && IsCarried(item) ? null : priority;
    }

    /// <summary>
    /// Whether an item is in someone's keeping rather than stored or lying loose: contained by
    /// something that is not permanent storage (Entity.StoredPermanentlyIn is set only for that) -
    /// a colonist's hands and pack, or a vehicle's hold.
    /// </summary>
    public static bool IsCarried(IKnownEntityData item) =>
        item != null && item.ContainedBy.HasValue && !item.StoredPermanentlyIn.HasValue;

    // ---- Point 4: an item its stockpile no longer allows leaves ----

    /// <summary>
    /// Whether HaulingJobManager hauls this item although a process may want its type (its
    /// ItemTypeIsNeededForProcess rule): it is stored where its structure's stockpile does not
    /// allow it, or the haul asked about is the one taking it out (picked up, it is no longer
    /// stored, and the studio's rule would cancel the haul mid-way).
    /// </summary>
    public static bool MustLeave(EntityGroup owner, IKnownEntityData item, HaulingJob haul = null) =>
        Enabled && (IsStoredAgainstItsStockpile(owner, item) || (haul != null && Evictions().Contains(haul.ID)));

    /// <summary>Stored in a structure whose stockpile, in the owner's settings, does not allow its type.</summary>
    public static bool IsStoredAgainstItsStockpile(EntityGroup owner, IKnownEntityData item)
    {
        StorageTarget? storedIn = item?.StoredPermanentlyIn;
        return storedIn.HasValue && item.EntityType != null && owner?.StructureStockpiles != null
            && owner.StructureStockpiles.TryGetValue(storedIn.Value.StorageEntity, out Stockpile stockpile)
            && stockpile != null && !stockpile.MayStockpile(item.EntityType, out _);
    }

    /// <summary>
    /// The hauls planned to take an item out of a stockpile that does not allow it, for the game in
    /// progress. Not saved: after a load, one under way falls back to the studio's rule.
    /// </summary>
    private static readonly HashSet<JobID> evictions = new HashSet<JobID>();

    private static Sim evictionsOf;

    private static HashSet<JobID> Evictions()
    {
        if (!ReferenceEquals(evictionsOf, The.Sim))
        {
            evictions.Clear();
            evictionsOf = The.Sim;
        }
        return evictions;
    }

    private static void RememberEviction(HaulingJob haul)
    {
        HashSet<JobID> set = Evictions();
        if (set.Count >= 256)
        {
            // A haul ends without saying so; forget the ones that are gone.
            set.RemoveWhere((JobID id) => LookUp<Job, JobID>.FindByID(id) == null);
        }
        set.Add(haul.ID);
    }

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
                    PrioritiseHaul(haul, stockpile, group, KnownItem(group, haul));
                }
            }
        });
    }

    /// <summary>The item a haul moves, as its owner knows it, or null.</summary>
    private static IKnownEntityData KnownItem(EntityGroup group, HaulingJob haul)
    {
        SharedKnowledge knowledge = group.GetAllegiance()?.SharedKnowledge;
        if (knowledge == null || !haul.Item.HasValue
            || GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(haul.Item.Value, out IKnownEntityData item)))
        {
            return null;
        }
        return item;
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
