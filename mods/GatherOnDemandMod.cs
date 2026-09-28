using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;

namespace UWGame.Mods;

/// <summary>
/// A standing order also covers what a job is waiting for: a padlocked resource is gathered when a
/// construction site or a workshop needs it, not only to keep a stock.
///
/// THE REQUEST. jerrybi, "auto harvesting": "instead of player selecting stuff that possible is not
/// even required in that amount, allow colonists to just go and take that X item they need ... to
/// create lean-to you need spoak branches, instead of gathering lets say 10 of them and make excess
/// amount laying around, gather only the amount needed when they are needed." Kastuk confirmed the
/// studio's half works: a padlock on a zone's Gather Resources slider makes gather jobs whenever the
/// stock is below the slider, per zone.
///
/// WHAT A JOB IS WAITING FOR. Every construction site and every running production job asks for
/// each missing input unit with its own hauling job - a HaulingJobAnyItemOfType carrying
/// RequiredByProcessJob (Structure, ProcessJob). One that has not been matched to an item yet
/// (Item is null) is an input nobody has found. Their count per item type is the demand.
///
/// WHAT THIS DOES. JobManager.GetAmountToProduce - the standing order's "keep N minus stock minus
/// what is already coming" - sees the stock less that demand, so the target becomes N plus the
/// demand. With the slider at 0, a padlocked resource is gathered only as jobs ask: three branches
/// for a lean-to, one bog ore at a time for bloom iron. And JobManager.RebalanceStandingOrderJobs
/// counts such an order as active while it has demand, where the studio skips an order at 0.
///
/// WHAT IT LEAVES ALONE. Only items with a standing order: the zones' padlocks still say what may
/// be gathered and where, so "how far may they wander" is answered by the zones as before. The
/// same rule serves a padlocked crafted item - planks made as a building asks for them - but a
/// production job is only created when its own inputs are in stock, so a chain is not pulled
/// through on demand. Nothing is saved; the demand is counted at every rebalance. Off by default.
/// </summary>
public static class GatherOnDemandMod
{
    public const string ModId = "gatherondemand";

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "STANDING ORDERS ALSO GATHER WHAT JOBS NEED", defaultValue: false,
            toolTip: "An item with a padlocked standing order is also gathered or made when a " +
                     "construction site or a workshop is waiting for it, on top of the amount kept. " +
                     "Set the slider to 0 to gather only on demand.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "GATHER ON DEMAND");
        _ = EnabledSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>How many units of this item jobs of this group are waiting for and nobody has found.</summary>
    public static int Demand(EntityGroup owner, EntityType type)
    {
        if (!Enabled || owner?.HaulingJobsAnyItemOfType == null || type == null
            || !owner.HaulingJobsAnyItemOfType.TryGetValue(type, out var jobs) || jobs == null)
        {
            return 0;
        }
        int count = 0;
        for (int i = jobs.Count - 1; i >= 0; i--)
        {
            HaulingJobAnyItemOfType job = jobs[i];
            if (job != null && !job.Item.HasValue && job.RequiredByProcessJob != null && !job.IsOfferedForTrade)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>The stock a standing order compares against: less what jobs are waiting for.</summary>
    public static int StockAfterDemand(EntityGroup owner, EntityType type, int stock) => stock - Demand(owner, type);

    /// <summary>Whether an order at 0 still has work: jobs are waiting for its item.</summary>
    public static bool HasDemand(EntityGroup owner, EntityType type) => Demand(owner, type) > 0;
}
