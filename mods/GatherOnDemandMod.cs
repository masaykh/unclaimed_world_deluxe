using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// A standing order also covers what a job is waiting for: a padlocked resource is gathered when a
/// construction site, a workshop or a production order needs it, not only to keep a stock.
///
/// THE REQUEST. jerrybi, "auto harvesting": "instead of player selecting stuff that possible is not
/// even required in that amount, allow colonists to just go and take that X item they need ... to
/// create lean-to you need spoak branches, instead of gathering lets say 10 of them and make excess
/// amount laying around, gather only the amount needed when they are needed." Kastuk confirmed the
/// studio's half works: a padlock on a zone's Gather Resources slider makes gather jobs whenever the
/// stock is below the slider, per zone.
///
/// WHAT A JOB IS WAITING FOR. Two kinds of demand:
/// - Every construction site and every production job that exists asks for each missing input unit
///   with its own hauling job - a HaulingJobAnyItemOfType carrying RequiredByProcessJob (Structure,
///   ProcessJob). One that has not been matched to an item yet (Item is null) is an input nobody
///   has found.
/// - A production order whose job was never made. The studio only creates a production job when
///   its inputs are in stock (JobManager.GetJobManagerProductionProcessesThatCanProduce, through
///   InventoryPanel.HasAllInputsAndToolsForProcess), so a clamwich soup order with no clams makes no
///   job, no hauling job asks for clams, and a clam order at 0 never moved (Kastuk, "auto
///   harvesting", 2026-10-02: "no one is going to collect needful clams"; the same for trimmed
///   branches and spoak branches). What such an order is still short of - the studio's own
///   JobManager.GetAmountToProduce for a standing order, ProductionJobsToComplete less the unstarted
///   jobs for a direct order - is turned into whole batches of its recipe, and those batches into
///   units of each input. The recipe is the first managed, non-gathering one that makes the item,
///   in the order the studio's list has them. That count goes through the same question for the
///   item it is made from, so a chain is pulled through: a bed frame order asks for trimmed
///   branches, which at 0 ask for spoak branches (at most four steps deep, which also stops a
///   recipe loop).
///
/// WHAT THIS DOES. JobManager.GetAmountToProduce - the standing order's "keep N minus stock minus
/// what is already coming" - sees the stock less that demand, so the target becomes N plus the
/// demand. With the slider at 0, a padlocked resource is gathered only as jobs ask: three branches
/// for a lean-to, three clams for six clamwich soup. And JobManager.RebalanceStandingOrderJobs
/// counts such an order as active while it has demand, where the studio skips an order at 0.
///
/// WHAT IT LEAVES ALONE. Only items with a standing order: the zones' padlocks still say what may
/// be gathered and where, so "how far may they wander" is answered by the zones as before. Once
/// enough has been gathered the studio makes the production job, its outstanding output leaves the
/// order's shortfall, and from then on its hauling jobs are the demand - nothing is counted twice.
/// A production order whose recipe also lacks a tool or a skill still asks for its inputs, which
/// are then ready when the tool is; it asks for no more than the order is short of. Nothing is
/// saved; the demand is counted at every rebalance. Off by default.
/// </summary>
public static class GatherOnDemandMod
{
    public const string ModId = "gatherondemand";

    /// <summary>How many production steps a demand is followed back through.</summary>
    private const int MaxChainDepth = 4;

    private static ModSetting enabled;

    // How deep the current Demand question is in a chain of recipes; see MaxChainDepth.
    [ThreadStatic]
    private static int chainDepth;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "STANDING ORDERS ALSO GATHER WHAT JOBS NEED", defaultValue: false,
            toolTip: "An item with a padlocked standing order is also gathered or made when a " +
                     "construction site, a workshop or a production order is waiting for it, on top " +
                     "of the amount kept. Set the slider to 0 to gather only on demand.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "GATHER ON DEMAND");
        _ = EnabledSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>How many units of this item jobs and production orders of this group are waiting for.</summary>
    public static int Demand(EntityGroup owner, EntityType type)
    {
        if (!Enabled || owner == null || type == null)
        {
            return 0;
        }
        return UnmatchedHaulingJobs(owner, type) + ProductionWaitingFor(owner, type) + FuelWaitingFor(owner, type);
    }

    /// <summary>
    /// Fuel that production orders will burn and the colony does not have. Kastuk, 2026-10-04:
    /// gathering on demand "not works ... for gathering the firewood, when cooking tasks need fuel".
    /// Fuel is no recipe input: the recipe names a tool that burns it (a campfire, a field kitchen,
    /// RequiresFuelType), the tool is topped up from stock by its own replenish goal, and a job
    /// whose tool cannot be fuelled is not taken (EvaluateJob.HasEnergyForToolType). So no hauling
    /// job ever asks for firewood and the counts above never see it.
    ///
    /// Counted here per order: the recipe's batches still to make (those short with no job, and
    /// jobs not started) times its days, at the tool's burn rate (RequiresFuelType.GetNeededFuel),
    /// less the bulk of every fuel that tool accepts already in stock. It is asked of the FIRST
    /// fuel in the tool's list with a standing order, so two padlocked fuels do not both gather
    /// for one fire. An estimate: fuel already in the fire is not counted, so it errs towards a
    /// little more.
    /// </summary>
    private static int FuelWaitingFor(EntityGroup owner, EntityType type)
    {
        float unitBulk = type.ItemType?.MaximumBulk ?? 0f;
        if (type.ItemType?.FuelType == null || unitBulk <= 0f || owner.ProductionOrders == null)
        {
            return 0;
        }
        Dictionary<RequiresFuelType, float> burns = new Dictionary<RequiresFuelType, float>();
        foreach (KeyValuePair<EntityType, ProductionOrder> pair in owner.ProductionOrders.Orders)
        {
            ProductionOrder order = pair.Value;
            if (!order.AmountToKeepInStore.HasValue && !order.ProductionJobsToComplete.HasValue)
            {
                continue;
            }
            ProcessType recipe = PreferredRecipe(pair.Key);
            RequiresFuelType fuel = recipe == null ? null : FuelBurnedBy(recipe);
            float? days = recipe?.WorkOrTimeNeeded?.DaysNeeded;
            if (fuel?.FuelEntityTypes == null || !days.HasValue || FirstOrderedFuel(owner, fuel) != type)
            {
                continue;
            }
            int perBatch = recipe.GetOutputAmount(pair.Key) ?? 1;
            int batches = BatchesShort(owner, pair.Key, order, perBatch) + UnstartedJobs(owner, pair.Key);
            if (batches > 0)
            {
                burns.TryGetValue(fuel, out float bulk);
                burns[fuel] = bulk + fuel.GetNeededFuel(days.Value * batches);
            }
        }
        int units = 0;
        foreach (KeyValuePair<RequiresFuelType, float> burn in burns)
        {
            float shortBulk = burn.Value - FuelInStock(owner, burn.Key);
            if (shortBulk > 0f)
            {
                units += (int)Math.Ceiling(shortBulk / unitBulk);
            }
        }
        return units;
    }

    /// <summary>
    /// The fuel a recipe's tools burn: a tool slot whose every alternative burns fuel, at the
    /// lowest rate among them. Null when a slot can be filled without fuel, or none burns any.
    /// </summary>
    private static RequiresFuelType FuelBurnedBy(ProcessType recipe)
    {
        if (recipe.ProcessToolSet?.Tools == null)
        {
            return null;
        }
        foreach (ToolAlternatives slot in recipe.ProcessToolSet.Tools)
        {
            RequiresFuelType cheapest = null;
            bool allBurn = slot.ToolsAndProductivity != null && slot.ToolsAndProductivity.Count > 0;
            foreach (Tuple<EntityType, float> tool in slot.ToolsAndProductivity ?? new List<Tuple<EntityType, float>>())
            {
                RequiresFuelType burns = tool.Item1?.ContainerType?.GetRequiresReplenishType()?.RequiresFuelType;
                if (burns == null)
                {
                    allBurn = false;
                    break;
                }
                if (cheapest == null || burns.BurnRatePerDay < cheapest.BurnRatePerDay)
                {
                    cheapest = burns;
                }
            }
            if (allBurn && cheapest != null)
            {
                return cheapest;
            }
        }
        return null;
    }

    /// <summary>The first fuel this tool accepts that the group has a standing order for.</summary>
    private static EntityType FirstOrderedFuel(EntityGroup owner, RequiresFuelType fuel)
    {
        foreach (EntityType candidate in fuel.FuelEntityTypes)
        {
            if (owner.ProductionOrders.Orders.TryGetValue(candidate, out ProductionOrder order) && order.AmountToKeepInStore.HasValue)
            {
                return candidate;
            }
        }
        return null;
    }

    /// <summary>The bulk of every fuel this tool accepts that the group holds.</summary>
    private static float FuelInStock(EntityGroup owner, RequiresFuelType fuel)
    {
        float bulk = 0f;
        foreach (EntityType candidate in fuel.FuelEntityTypes)
        {
            if (owner.AllEntities != null && owner.AllEntities.TryGetValue(candidate, out List<EntityID> ids) && ids != null)
            {
                bulk += ids.Count * (candidate.ItemType?.MaximumBulk ?? 0f);
            }
        }
        return bulk;
    }

    /// <summary>Inputs that construction sites and existing production jobs ask for and nobody has found.</summary>
    private static int UnmatchedHaulingJobs(EntityGroup owner, EntityType type)
    {
        if (owner.HaulingJobsAnyItemOfType == null
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

    /// <summary>
    /// Units of this item that production orders are still short of, in whole batches of the recipe
    /// each order would use. Gross: the standing order's own formula takes the stock off.
    /// </summary>
    private static int ProductionWaitingFor(EntityGroup owner, EntityType type)
    {
        if (owner.ProductionOrders == null || chainDepth >= MaxChainDepth
            || !GameData.Instance.ProcessesUsingThisInput.TryGetValue(type, out List<ProcessType> processes))
        {
            return 0;
        }
        chainDepth++;
        try
        {
            int units = 0;
            foreach (ProcessType process in processes)
            {
                if (process.Outputs == null || process.InputsByType == null
                    || !process.InputsByType.TryGetValue(type, out Input input)
                    || !(input.Amount?.NoOfItems is int perBatch) || perBatch <= 0)
                {
                    continue;
                }
                // One batch makes all of a recipe's outputs: the output that wants the most batches decides.
                int batches = 0;
                foreach (Output output in process.Outputs)
                {
                    EntityType made = output.FinalEntityTypeToCreate;
                    if (output.IsWasteProduct || made == null || made == type
                        || !owner.ProductionOrders.Orders.TryGetValue(made, out ProductionOrder order)
                        || (!order.AmountToKeepInStore.HasValue && !order.ProductionJobsToComplete.HasValue)
                        || PreferredRecipe(made) != process)
                    {
                        continue;
                    }
                    batches = Math.Max(batches, BatchesShort(owner, made, order, output.Amount?.NoOfItems ?? 1));
                }
                units += batches * perBatch;
            }
            return units;
        }
        finally
        {
            chainDepth--;
        }
    }

    /// <summary>The recipe an order for this item waits on: the first the job manager may run that is not gathering.</summary>
    private static ProcessType PreferredRecipe(EntityType made)
    {
        if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(made, out List<ProcessType> recipes))
        {
            foreach (ProcessType recipe in recipes)
            {
                if (JobManager.IsManagedProcess(recipe) && !recipe.IsGathering && recipe.InputsByType != null)
                {
                    return recipe;
                }
            }
        }
        return null;
    }

    /// <summary>How many batches this order still wants that no job has been made for.</summary>
    private static int BatchesShort(EntityGroup owner, EntityType made, ProductionOrder order, int outputPerBatch)
    {
        if (order.ProductionJobsToComplete is int jobsOrdered)
        {
            // A direct order counts jobs; started ones have already left ProductionJobsToComplete.
            return Math.Max(0, jobsOrdered - UnstartedJobs(owner, made));
        }
        JobManager.GetAmountToProduce(owner, made, order, out _, out int amount);
        int each = Math.Max(1, outputPerBatch);
        return amount > 0 ? (amount + each - 1) / each : 0;
    }

    /// <summary>JobManager.CountUnstartedJobs without its side effect of destroying invalid jobs.</summary>
    private static int UnstartedJobs(EntityGroup owner, EntityType made)
    {
        if (owner.ManagedProductionJobs == null || !owner.ManagedProductionJobs.TryGetValue(made, out List<ProcessJob> jobs) || jobs == null)
        {
            return 0;
        }
        int count = 0;
        foreach (ProcessJob job in jobs)
        {
            if (job != null && job.IsStarted(out bool isStarted) && !isStarted)
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

    /// <summary>
    /// Whether a padlocked slider at this value lets the zone gather the resource
    /// (GatherResourcesWindow.btOk_Click). The studio reads 0 as "no order" and takes the resource
    /// out of Zone.AllowStandingOrderHarvest, and a zone left with no orders destroys itself
    /// (Zone.RemoveZoneOrFireOrdersChangedEvent). With the mod on, 0 is the "only on demand" order,
    /// so it has to stay. Kastuk: "No way to set Gathering zone at standing order with value 0".
    /// </summary>
    public static bool ZoneGathersAt(int sliderValue) => sliderValue > 0 || Enabled;
}
