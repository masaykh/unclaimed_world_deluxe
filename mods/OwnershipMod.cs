using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// Households are paid in food and cook for themselves - the first step from a communal expedition
/// towards private ownership.
///
/// THE PLAN (tripleacoder, "Private ownership", agreed with Kastuk): "For the first iteration: pay in
/// food items to the household; communal houses; a food needs planner that runs per household and
/// creates cooking jobs for the household members using the earned food."
///
/// THE STUDIO BUILT THE LEVELS AND LEFT THEM IDLE. Every person has three owners - itself, its
/// household and the expedition - and GoalThink already gives each member an EvaluateJob for its
/// household's jobs in the LEISURE phase ("#region personal & household jobs - not yet in use!"). A
/// household has its own EntityGroup, Food list and hauling. Nothing ever gives a household anything.
///
/// WHAT THIS DOES, when the day turns from work to leisure (Sim.Update, <see cref="OnSimTick"/>):
/// - PAY (<see cref="PayRations"/>): each household with a worker is topped up to
///   <see cref="DaysOfFood"/> days of its members' food need, with food moved out of the expedition's
///   stock by Entity.ChangeOwnership - the studio's own transfer, which does every list and every
///   piece of knowledge. A top-up, not "+N a day", so nothing is counted in the save and a reload
///   cannot pay twice. Only free food moves: not reserved (ReserveMod), not on a job, not in use, not
///   carried. Picked in a fixed order (item type, then id), as replays need.
/// - COOK (<see cref="PlanCooking"/>): if the household has no meal on order, one of the meal recipes
///   whose ingredients it owns - a different one each day - becomes a job in the household's own group, with the hauling jobs for
///   its ingredients - what the JobManager does for the colony. A member takes it after work; the meal
///   belongs to the household.
///
/// THROUGH COMMANDS. "All planners use Commands - the Commands create Jobs" (the studio,
/// PhysicalNeedsPlanner), and tripleacoder asked that this planner keep to it, so planner and player
/// share one code path. Paying is a GiveToHousehold command and cooking a SetHouseholdProduction - the
/// household counterpart of SetProduction, which can only address an expedition. Both are executed
/// directly with no client feedback, as the studio's GoapFindPreyAction executes HuntArea.
///
/// THREE THINGS THE STUDIO'S CODE NEEDED, as core hooks:
/// - a person searched only the EXPEDITION's food (EvaluateEat.GetFoodEntityGroup), so household food
///   would never be eaten: EvaluateEat.GetAllFoodItems now adds the household's (<see cref="AddHouseholdFood"/>).
///   GoalEat keeps the owner it is given but never reads it.
/// - a household job looked for tools and worksites only among the household's own things, so a
///   communal campfire was invisible: EvaluateJob falls back to the expedition's (<see cref="CommunalFallback"/>).
/// - InventoryPanel.HasAllInputsAndToolsForProcess crashes for a household (owner.GetExpedition() is
///   null there), so the planner checks ingredients, tools, skill and policy itself.
///
/// KNOWN LIMITS OF THIS FIRST STEP: household food no longer counts in the colony's stock, so the
/// colony may make more food; a household that emigrates takes its food; a campfire's fuel is looked
/// for in the household, so an unfuelled fire refuses household cooking (the colony keeps fires lit).
/// Houses stay communal. Off by default.
/// </summary>
public static class OwnershipMod
{
    public const string ModId = "ownership";

    private static ModSetting enabled;

    private static ModSetting daysOfFood;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "HOUSEHOLDS ARE PAID IN FOOD AND COOK FOR THEMSELVES", defaultValue: false,
            toolTip: "After each workday, every household with a worker is topped up with food from " +
                     "the colony's stock, and its members cook their own meals from it after work. " +
                     "Houses stay communal. Household food no longer counts in the colony's stock.",
            affectsSimulation: true));

    public static ModSetting DaysOfFoodSetting =>
        daysOfFood ?? (daysOfFood = ModSettings.Choice(
            ModId, "daysOfFood", "HOUSEHOLDS ARE PAID UP TO (DAYS OF FOOD)", new[] { "1", "2", "3" }, "1",
            toolTip: "How many days of its members' food need each household is topped up to.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "OWNERSHIP");
        _ = EnabledSetting;
        _ = DaysOfFoodSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    public static int DaysOfFood => int.TryParse(DaysOfFoodSetting.Value, out int d) && d > 0 ? d : 1;

    // ---- the daily turn ---------------------------------------------------------------------

    private static Sim.DayPhases? lastPhase;

    /// <summary>Called from Sim.Update every frame; acts once, when work turns to leisure.</summary>
    public static void OnSimTick(Sim sim)
    {
        if (!Enabled || sim?.PlaySite?.PlayerAllegiance == null || sim.Mode != Sim.EngineMode.Game || sim.DateAndTime == null)
        {
            lastPhase = null;
            return;
        }
        Sim.DayPhases phase = sim.DateAndTime.CurrentPhase;
        Sim.DayPhases? previous = lastPhase;
        lastPhase = phase;
        if (previous != Sim.DayPhases.Work || phase != Sim.DayPhases.Leisure)
        {
            return;
        }
        foreach (Expedition expedition in sim.PlaySite.PlayerAllegiance.Expeditions)
        {
            if (expedition?.Households == null)
            {
                continue;
            }
            foreach (Household household in expedition.Households)
            {
                if (household?.OwnedEntities == null)
                {
                    continue;
                }
                PayRations(expedition, household);
                PlanCooking(expedition, household);
            }
        }
    }

    // ---- pay --------------------------------------------------------------------------------

    /// <summary>
    /// Items of food a household eats in a day - EntityGroup.RecomputeFoodConsumeRate's own sum (which
    /// writes a private field), over the household's members.
    /// </summary>
    public static int DailyNeedItems(Household household)
    {
        var totals = new Dictionary<SimSide.AI.Needs.NeedType, float>();
        household.IterateMembers(delegate(Entity e)
        {
            if (e?.Intelligence == null || !e.Intelligence.IsIndependent() || e.EntityType.BiologicalType == null)
            {
                return;
            }
            foreach (var need in e.BiologicalEntity.Needs.NeedsList)
            {
                if (need.Value.FoodNeed != null)
                {
                    totals.TryGetValue(need.Value.NeedType, out float v);
                    totals[need.Value.NeedType] = v + need.Value.FoodNeed.TotalNeededNutrientBulk;
                }
            }
        });
        int items = 0;
        EntityType generic = GameData.Instance.AIConstants.GenericFoodItem;
        foreach (var total in totals)
        {
            if (!total.Key.FoodNeedType.IsEssential)
            {
                continue;
            }
            var amount = generic.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.FirstOrDefault(n => n.Nutrient == total.Key.FoodNeedType.FoodNutrientType);
            if (amount != null)
            {
                float perItem = amount.Amount * generic.ItemType.MaximumBulk.Value;
                if (perItem > 0f)
                {
                    items = Math.Max(items, (int)Math.Ceiling(total.Value / perItem));
                }
            }
        }
        return items;
    }

    private static int FoodCount(EntityGroup group) => group?.Food?.Values.Sum(l => l.Count) ?? 0;

    /// <summary>Tops a working household up to its days of food from the expedition's free stock.</summary>
    public static int PayRations(Expedition expedition, Household household)
    {
        bool works = false;
        household.IterateMembers(delegate(Entity e)
        {
            works |= e != null && expedition.Workers.Contains(e.EntityID);
        });
        if (!works)
        {
            return 0;
        }
        int deficit = DailyNeedItems(household) * DaysOfFood - FoodCount(household.OwnedEntities);
        if (deficit <= 0)
        {
            return 0;
        }
        EntityGroup colony = expedition.OwnedEntities;
        var knowledge = expedition.Allegiance?.SharedKnowledge;
        // Free food of each type, types in key order and items in id order - then one of each type in
        // turn, so a household gets a mix it can cook from rather than a dozen of one ingredient.
        var byTypeFree = new List<Queue<Entity>>();
        foreach (var byType in colony.Food.OrderBy(f => f.Key.KeyName, StringComparer.Ordinal))
        {
            if (ReserveMod.HoldsBackFood(colony, byType.Key, starving: false, eater: null))
            {
                continue;
            }
            var free = new Queue<Entity>();
            foreach (EntityID id in byType.Value.OrderBy(i => (ulong)i))
            {
                Entity item = Entity.FindByID(id);
                // Never below the reserve: HoldsBackFood above only asks whether ANY may go (Kastuk:
                // reserved pickled carbon tail was still consumed).
                if (IsFreeFood(item, knowledge) && ReserveMod.MayTakeFood(colony, byType.Key, free.Count, eater: null))
                {
                    free.Enqueue(item);
                }
            }
            if (free.Count > 0)
            {
                byTypeFree.Add(free);
            }
        }
        var pay = new List<EntityID>();
        while (pay.Count < deficit && byTypeFree.Count > 0)
        {
            for (int t = 0; t < byTypeFree.Count && pay.Count < deficit; t++)
            {
                pay.Add(byTypeFree[t].Dequeue().EntityID);
            }
            byTypeFree.RemoveAll(q => q.Count == 0);
        }
        if (pay.Count == 0)
        {
            return 0;
        }
        // Through a command, as planners must (the studio's PhysicalNeedsPlanner; tripleacoder),
        // executed directly with no client feedback, as GoapFindPreyAction does HuntArea.
        var give = new UWGame.SimSide.Commands.GiveToHousehold(household.ID, pay);
        give.Execute(giveClientFeedback: false);
        return give.Given;
    }

    private static bool IsFreeFood(Entity item, SimSide.AI.SharedKnowledge knowledge)
    {
        if (item == null || item.AssignedToJob.HasValue || !item.IsOnPlaySite() || !item.IsCompleted())
        {
            return false;
        }
        if (knowledge != null && knowledge.GetInUseBy(item.EntityID).HasValue)
        {
            return false;
        }
        // Not in someone's hands or pack.
        return !(item.ContainedBy.HasValue && Entity.FindByID(item.ContainedBy.Value)?.Intelligence != null);
    }

    // ---- cook -------------------------------------------------------------------------------

    private static List<ProcessType> mealProcesses;

    /// <summary>Every recipe whose output is a meal (FoodType.IsMeal), in a fixed order.</summary>
    public static List<ProcessType> MealProcesses()
    {
        if (mealProcesses != null)
        {
            return mealProcesses;
        }
        // The recipe table itself, with the rules GameData.AddProcessToProductionGraph uses for its
        // index - not the index (ProcessYieldsThisOutput), which only GameData.Initialize builds, on
        // the game's loading screen, so a tool reading the tables would find it empty.
        var found = new List<ProcessType>();
        foreach (ProcessType p in GameData.Instance.AllProcessTypes.Values)
        {
            if (p == null || p.IsOriginalSpecialAction || p.IsSalvageProcess || !p.IsPartOfProductionChain()
                || p.InputsByType == null || p.InputsByType.Count == 0 || p.Outputs == null)
            {
                continue;
            }
            if (p.Outputs.Any(o => !o.IsWasteProduct && o.FinalEntityTypeToCreate?.ItemType?.FoodType?.IsMeal == true))
            {
                found.Add(p);
            }
        }
        mealProcesses = found.OrderBy(p => p.KeyName, StringComparer.Ordinal).ToList();
        return mealProcesses;
    }

    private static EntityType MealOutput(ProcessType process) =>
        process.Outputs?.Select(o => o.FinalEntityTypeToCreate).FirstOrDefault(t => t?.ItemType?.FoodType?.IsMeal == true);

    /// <summary>Orders one meal for the household from what it owns, unless one is already on order.</summary>
    public static ProcessJob PlanCooking(Expedition expedition, Household household)
    {
        EntityGroup group = household.OwnedEntities;
        foreach (var jobs in group.ProductionJobs)
        {
            if (jobs.Key?.ItemType?.FoodType?.IsMeal == true && jobs.Value != null && jobs.Value.Count > 0)
            {
                return null;
            }
        }
        // Every meal it can cook, then one of them by the day's number: variety, where "the first that
        // fits" would cook the same dish every day - which BalancedDietMod's monotony punishes -
        // and still no random number, so a replay picks the same.
        List<ProcessType> cookable = MealProcesses().Where(p => HouseholdCanCook(expedition, household, p)).ToList();
        if (cookable.Count > 0)
        {
            int day = (int)(The.Sim?.DateAndTime?.CurrentTimeDateYear.TotalDays ?? 0.0);
            ProcessType process = cookable[Math.Abs(day) % cookable.Count];
            // The order goes through SetHouseholdProduction, the household's SetProduction: planners
            // act through commands, and the command creates the job and its hauling.
            var order = new UWGame.SimSide.Commands.SetHouseholdProduction(household.ID, process.KeyName, MealOutput(process).KeyName);
            order.Execute(giveClientFeedback: false);
            return order.CreatedJob;
        }
        return null;
    }

    /// <summary>
    /// Ingredients from the household, tools and worksite from the colony, skill and policy from the
    /// expedition. Not HasAllInputsAndToolsForProcess, which crashes for a household.
    /// </summary>
    public static bool HouseholdCanCook(Expedition expedition, Household household, ProcessType process)
    {
        if (MealOutput(process) == null || !expedition.HasSkill(process.RequiredSkillType)
            || (expedition.Policy != null && !expedition.Policy.CanUseProcess(process, out _)))
        {
            return false;
        }
        foreach (var input in process.InputsByType)
        {
            int needed = input.Value?.Amount?.NoOfItems ?? 1;
            int owned = 0;
            if (household.OwnedEntities.Items.TryGetValue(input.Key, out var items))
            {
                owned = items.Count;
            }
            else if (household.OwnedEntities.Food.TryGetValue(input.Key, out var food))
            {
                owned = food.Count;
            }
            if (owned < needed)
            {
                return false;
            }
        }
        return UWGame.ClientSide.Interface.Inventory.InventoryPanel.HasToolsForProcess(process, expedition.OwnedEntities);
    }

    // ---- hooks ------------------------------------------------------------------------------

    /// <summary>Called from EvaluateEat.GetAllFoodItems: the eater's household food, as the expedition's is.</summary>
    public static void AddHouseholdFood(Entity eater, BiologicalEntity bio, SimSide.AI.SharedKnowledge knowledge, List<EntityID> allFoodItems)
    {
        if (!Enabled || eater?.PersonEntity?.Household?.OwnedEntities?.Food == null || allFoodItems == null)
        {
            return;
        }
        EntityGroup household = eater.PersonEntity.Household.OwnedEntities;
        foreach (var byType in household.Food)
        {
            for (int i = byType.Value.Count - 1; i >= 0; i--)
            {
                if (EvaluateEat.IsValidFoodItem(byType.Value[i], eater, bio, knowledge, household, out var data) && !allFoodItems.Contains(data.EntityID))
                {
                    allFoodItems.Add(data.EntityID);
                }
            }
        }
    }

    /// <summary>Called from EvaluateJob's tool search: for a household's job, the colony's tools and worksites.</summary>
    public static EntityGroup CommunalFallback(EntityGroup ownerOfJobs) =>
        Enabled && ownerOfJobs?.Parent is Household household ? household.Expedition?.OwnedEntities : null;
}
