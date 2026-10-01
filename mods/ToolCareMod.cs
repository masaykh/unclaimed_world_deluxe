using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Tiers;

namespace UWGame.Mods;

/// <summary>
/// Colonists don't leave good tools and weapons lying in the woods. When a job needs their hands,
/// they put a tool back where they took it from - unless something more pressing is going on, and
/// then they drop it where they stand, as the studio made it.
///
/// THE REQUEST. Kastuk, "Dropping tools": colonists drop high-tech weapons and tools far from camp
/// when they switch jobs, and they lie there for hours. tripleacoder: dropping is often right, but
/// valuable items deserve better; don't use task priority for it (priority is the player's), let the
/// carrier take it back itself; and say so in the log.
///
/// THE SECOND ROUND. Never dropping a weapon to make room (the first fix for the hunter's coil
/// rifle) was too much (Kastuk); the real fault was the shuffle - a drop followed by the same
/// colonist picking the same item up again. The agreed rule (Jerrybi):
/// - a fight is waiting, or the colonist is hurt or hungry: drop it where you stand;
/// - otherwise: take it back to where it came from (the storage or stockpile it was picked up
///   from), or to camp when that place is gone, full or out in the field.
/// Every condition is a setting under MODS -> TOOL CARE.
///
/// WHICH DROPS. The "this job needs my hands" drops only: CompositeGoal.DropUnneededItems (reached
/// from DropUnneededItemsToMakeCapacity - GoalHarvest, GoalProduce, GoalEat, GoalReplenish and
/// picking up a tool) and GoalHaul.DropItemsOverCapacity (a hauler making room for cargo, where the
/// coil rifle went). Someone better claiming the tool, stationary tools set down at a work site,
/// sleeping and dying are untouched. Goals that may walk first say so (CompositeGoal.MayCarryBackFirst);
/// GoalProduce does not, since it may already be inside a building.
///
/// WHAT IS LOOKED AFTER. Weapons, ammunition for a weapon the colonist is carrying, and items above
/// the survival tier: an item's own TierOrAreaType where it has one, otherwise the LOWEST tier among
/// the processes that make it (ProcessType.GetTierArea). LOOK AFTER can widen that to every movable
/// tool. Looked-after items are always dropped LAST: everything else goes first, in place.
///
/// THE SHUFFLE. GoalHaul makes room (DropItemsOverCapacity) and then, in the same activation, looks
/// for a weapon to take along (FindOptionalEquipmentIfNeeded) - and the best weapon in reach was the
/// one it had just set down. CompositeGoal now remembers what a goal set down to make room and does
/// not take it along as optional equipment. A job that NEEDS the item still picks it up.
///
/// WHERE IT CAME FROM is remembered when a colonist picks a looked-after item up (GoalPickup, via
/// <see cref="NoteTakenFrom"/>) and is not saved: after a load, a tool carried at the time goes to
/// camp instead. Off by default.
/// </summary>
public static class ToolCareMod
{
    public const string ModId = "toolcare";

    private const string BackWhereItCameFrom = "BACK WHERE IT CAME FROM";
    private const string ToCamp = "TO CAMP";
    private const string DropIt = "NOWHERE - DROP IT";

    private const string GoodTools = "WEAPONS AND GOOD TOOLS";
    private const string AllTools = "ALL TOOLS AND WEAPONS";

    private const string Off = "OFF";
    private const string Starving = "WHEN STARVING";
    private const string Hungry = "WHEN HUNGRY";

    private static ModSetting enabled;
    private static ModSetting lookAfter;
    private static ModSetting placement;
    private static ModSetting forFights;
    private static ModSetting whenHurt;
    private static ModSetting whenHungry;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "COLONISTS LOOK AFTER TOOLS", defaultValue: false,
            toolTip: "When a job needs a colonist's hands, weapons and good tools are dropped last, " +
                     "and are put back where they came from instead of on the ground - unless " +
                     "one of the conditions below says to drop them where they stand.",
            affectsSimulation: true));

    public static ModSetting LookAfter =>
        lookAfter ?? (lookAfter = ModSettings.Choice(
            ModId, "lookAfter", "LOOK AFTER", new[] { GoodTools, AllTools }, GoodTools,
            toolTip: "Good tools are anything above the survival tier. Weapons, and ammunition for " +
                     "a weapon being carried, are always looked after.",
            affectsSimulation: true));

    public static ModSetting Placement =>
        placement ?? (placement = ModSettings.Choice(
            ModId, "placement", "A TOOL THAT MUST GO IS TAKEN", new[] { BackWhereItCameFrom, ToCamp, DropIt }, BackWhereItCameFrom,
            toolTip: "BACK WHERE IT CAME FROM: to the storage or stockpile it was taken from, or to " +
                     "camp if that is gone or full. TO CAMP: to the camp centre, only when out in " +
                     "the field. NOWHERE: dropped where the colonist stands, as in the original game.",
            affectsSimulation: true));

    public static ModSetting ForFights =>
        forFights ?? (forFights = ModSettings.Toggle(
            ModId, "forFights", "A FIGHT: DROP IT WHERE THEY STAND", defaultValue: true,
            toolTip: "A colonist setting off to fight a threat (the most urgent work there is) " +
                     "drops what it must where it stands, without a walk first.",
            affectsSimulation: true));

    public static ModSetting WhenHurt =>
        whenHurt ?? (whenHurt = ModSettings.Choice(
            ModId, "whenHurt", "HURT: DROP IT WHERE THEY STAND BELOW", new[] { Off, "25% HEALTH", "50% HEALTH", "75% HEALTH" }, "50% HEALTH",
            toolTip: "A colonist with less health than this drops what it must where it stands.",
            affectsSimulation: true));

    public static ModSetting WhenHungry =>
        whenHungry ?? (whenHungry = ModSettings.Choice(
            ModId, "whenHungry", "HUNGRY: DROP IT WHERE THEY STAND", new[] { Off, Starving, Hungry }, Hungry,
            toolTip: "HUNGRY is the game's own 'has not eaten' level (a food need at 70% or less); " +
                     "STARVING is a food need run out.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "TOOL CARE");
        _ = EnabledSetting;
        _ = LookAfter;
        _ = Placement;
        _ = ForFights;
        _ = WhenHurt;
        _ = WhenHungry;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>Whether this carrier looks after its things: a colonist, with the mod on.</summary>
    public static bool IsCarefulCarrier(Entity carrier) => Enabled && carrier?.EntityType?.Person != null;

    private static readonly Dictionary<EntityType, bool> protectedCache = new Dictionary<EntityType, bool>();

    /// <summary>Weapons, and items above the survival tier by <see cref="TierIndexOf"/>.</summary>
    public static bool IsProtected(EntityType type)
    {
        if (type == null || type.IsIntrinsic())
        {
            return false;
        }
        if (protectedCache.TryGetValue(type, out bool cached))
        {
            return cached;
        }
        bool result = type.ItemType?.WeaponType != null || TierIndexOf(type) > SurvivalIndex();
        protectedCache[type] = result;
        return result;
    }

    /// <summary>
    /// What this carrier drops last and puts back: <see cref="IsProtected"/>, ammunition for a weapon
    /// it carries, and with LOOK AFTER at ALL TOOLS, any movable tool.
    /// </summary>
    public static bool IsLookedAfter(Entity carrier, EntityType type)
    {
        if (type == null || type.IsIntrinsic())
        {
            return false;
        }
        if (IsProtected(type))
        {
            return true;
        }
        if (LookAfter.Value == AllTools && type.ToolType != null && !ToolType.IsImmovable(type))
        {
            return true;
        }
        return type.ItemType?.AmmunitionType != null && IsAmmoForCarriedWeapon(carrier, type);
    }

    private static bool IsAmmoForCarriedWeapon(Entity carrier, EntityType ammo)
    {
        bool found = false;
        carrier?.AgentStorage?.ItemStorage?.IterateContainedBreakOnTrue(delegate(Entity carried)
        {
            UWGame.SimSide.Items.WeaponType weapon = carried.EntityType?.ItemType?.WeaponType;
            if (weapon?.AttackTypes != null)
            {
                foreach (var attack in weapon.AttackTypes)
                {
                    if (attack?.UsesAmmoType == ammo)
                    {
                        found = true;
                        return true;
                    }
                }
            }
            return false;
        });
        return found;
    }

    private static int SurvivalIndex() =>
        GameData.Instance.AllTierTypes.TryGetValue("survival", out var survival) ? survival.Index : 0;

    /// <summary>The item's own tier if it has one, else the lowest tier of any process that makes it; -1 when neither says.</summary>
    public static int TierIndexOf(EntityType type)
    {
        TierType own = type.TierOrAreaType?.GetTier();
        if (own != null)
        {
            return own.Index;
        }
        int lowest = int.MaxValue;
        if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(type, out var processes))
        {
            foreach (ProcessType process in processes)
            {
                TierType tier = process.GetTierArea()?.GetTier();
                // A process with no tier at all is available to anyone: the item is as cheap as it gets.
                lowest = System.Math.Min(lowest, tier?.Index ?? -1);
            }
        }
        return lowest == int.MaxValue ? -1 : lowest;
    }

    // ---- When to drop it where they stand ----

    /// <summary>
    /// Whether this carrier drops a looked-after item where it stands: the placement is NOWHERE, or a
    /// fight is what it is setting off to do, or it is hurt or hungry past the settings.
    /// </summary>
    public static bool DropsWhereItStands(Entity carrier) =>
        DropsWhereItStands(IsSettingOffToFight(carrier), HealthFraction(carrier), LowestFoodLevel(carrier));

    /// <summary>The rule itself, on numbers: health and food are fractions of full, null when the carrier has none.</summary>
    public static bool DropsWhereItStands(bool settingOffToFight, float? health, float? food)
    {
        if (Placement.Value == DropIt)
        {
            return true;
        }
        if (settingOffToFight && ForFights.On)
        {
            return true;
        }
        float? hurtBelow = HurtThreshold(WhenHurt.Value);
        if (hurtBelow.HasValue && health.HasValue && health.Value < hurtBelow.Value)
        {
            return true;
        }
        if (food.HasValue)
        {
            // EvaluateEat.GetLowestFoodLevel: above 0.7 is (almost) full, above 0 "has not eaten", 0 is starving.
            string hunger = WhenHungry.Value;
            if (hunger == Hungry && food.Value <= 0.7f)
            {
                return true;
            }
            if (hunger == Starving && Common.IsZero(food.Value))
            {
                return true;
            }
        }
        return false;
    }

    private static float? HurtThreshold(string value)
    {
        string number = value?.Split('%')[0];
        return int.TryParse(number, out int percent) && percent > 0 ? percent / 100f : (float?)null;
    }

    /// <summary>
    /// Whether the goal the carrier is starting is a fight: its top-level goal was set by the attack
    /// evaluator for threats or threats to the colony's assets (EvaluateAttackJobs at
    /// PriorityOfThreatJobs or PriorityOfAssetThreatJobs, the studio's most urgent work) - not a hunt,
    /// which that evaluator runs at PriorityOfWorkJobs.
    /// </summary>
    private static bool IsSettingOffToFight(Entity carrier)
    {
        GoalThink brain = carrier?.Intelligence?.Brain;
        if (brain == null || brain.Subgoals.Count == 0)
        {
            return false;
        }
        GoalEvaluator evaluator = brain.Subgoals.Peek().GoalEvaluator;
        return evaluator is EvaluateAttackJobs && evaluator.Priority > GameData.Instance.AIConstants.PriorityOfWorkJobs;
    }

    /// <summary>Health as a fraction of full (Body.GlobalHitpoints over MaxHitpoints), or null.</summary>
    private static float? HealthFraction(Entity carrier)
    {
        if (carrier == null || !carrier.Find<BodyComponent>(out var component) || component?.Body == null || component.Body.MaxHitpoints <= 0f)
        {
            return null;
        }
        return component.Body.GlobalHitpoints / component.Body.MaxHitpoints;
    }

    /// <summary>The lowest food need's level, as EvaluateEat.GetLowestFoodLevel reads it, or null.</summary>
    private static float? LowestFoodLevel(Entity carrier)
    {
        var needs = carrier?.EntityType?.BiologicalType != null ? carrier.BiologicalEntity?.Needs?.NeedsList : null;
        if (needs == null)
        {
            return null;
        }
        float? lowest = null;
        foreach (var need in needs)
        {
            if (need.Value?.NeedType?.FoodNeedType != null && (!lowest.HasValue || need.Value.CurrentLevel < lowest.Value))
            {
                lowest = need.Value.CurrentLevel;
            }
        }
        return lowest;
    }

    // ---- Where it came from ----

    private struct Origin
    {
        public EntityID? Container;
        public StorageID Storage;
        public Vector3 Location;
    }

    /// <summary>Where each looked-after item was picked up, for the game in progress. Not saved.</summary>
    private static readonly Dictionary<EntityID, Origin> origins = new Dictionary<EntityID, Origin>();

    private static Sim originsOf;

    private static Dictionary<EntityID, Origin> Origins()
    {
        if (!ReferenceEquals(originsOf, The.Sim))
        {
            origins.Clear();
            originsOf = The.Sim;
        }
        return origins;
    }

    /// <summary>
    /// Called from GoalPickup just before the item changes hands: remember the storage it is in, or
    /// the spot it lies on.
    /// </summary>
    public static void NoteTakenFrom(Entity carrier, Entity item)
    {
        if (!IsCarefulCarrier(carrier) || item == null || !IsLookedAfter(carrier, item.EntityType))
        {
            return;
        }
        Origin origin = new Origin { Location = item.PlaySiteLocation };
        if (item.GetContainedBy(out Entity container) && container != null)
        {
            Storage storage = container.Intelligence == null ? container.StorageContainer?.GetStoredIn(item) : null;
            if (storage == null)
            {
                // Carried by someone, or in something that is not storage: not a place to put it back.
                Origins().Remove(item.EntityID);
                return;
            }
            origin.Container = container.EntityID;
            origin.Storage = storage.ID;
        }
        Origins()[item.EntityID] = origin;
    }

    /// <summary>
    /// Where a looked-after item that must go is taken, by A TOOL THAT MUST GO IS TAKEN: back into the
    /// storage it came from, or onto the stockpile it came from; else, out in the field, to where it
    /// lay in camp or to a free spot at the camp centre. False: drop it here.
    /// </summary>
    public static bool PlaceToTakeItTo(Entity carrier, Entity item, out Vector3? location, out StorageTarget? storage, out string where)
    {
        location = null;
        storage = null;
        where = null;
        string mode = Placement.Value;
        if (mode == DropIt)
        {
            return false;
        }
        if (mode == BackWhereItCameFrom && Origins().TryGetValue(item.EntityID, out Origin origin))
        {
            Origins().Remove(item.EntityID);
            if (origin.Container.HasValue)
            {
                Entity container = Entity.FindByID(origin.Container.Value);
                Storage space = container?.StorageContainer?.FindStorage(origin.Storage);
                if (space != null && !container.IsTradeOfferStorage(origin.Storage) && space.HasCapacityForItem(item.Bulk))
                {
                    storage = new StorageTarget(container.EntityID, origin.Storage);
                    where = "the " + NameOf(container.EntityType);
                    return true;
                }
            }
            else if (IsOnStockpile(carrier, origin.Location))
            {
                location = origin.Location;
                where = "the stockpile";
                return true;
            }
            else if (!IsOutOfCamp(carrier, origin.Location) && IsOutOfCamp(carrier, carrier.PlaySiteLocation))
            {
                // It lay loose in camp: back to that spot. Already in camp, it is simply dropped here.
                location = origin.Location;
                where = "camp";
                return true;
            }
        }
        if (!IsOutOfCamp(carrier, carrier.PlaySiteLocation))
        {
            return false;
        }
        location = DropSpotInCamp(carrier);
        where = "camp";
        return location.HasValue;
    }

    private static bool IsOnStockpile(Entity carrier, Vector3 location)
    {
        Allegiance allegiance = carrier?.Intelligence?.Allegiance;
        Point tile = MapManager.WorldPosToTile(location);
        return allegiance != null && The.Map != null && The.Map.TileIsOnMap(tile)
            && (The.Map.GetTile(tile).GetListOfZones(allegiance)?.Exists((Zone z) => z.Stockpile != null) ?? false);
    }

    /// <summary>Whether a spot is out of camp: further from the expedition's centre than a colonist is sent home from (EvaluateReturnHome).</summary>
    private static bool IsOutOfCamp(Entity carrier, Vector3 location)
    {
        Vector3? center = carrier?.Intelligence?.CurrentExpedition?.Center;
        return center.HasValue && Vector3.Distance(location, center.Value) > GameData.Instance.AIConstants.DistanceFromExpeditionToReturnHome;
    }

    /// <summary>A free spot near the camp centre - GoalReturnHome's own search - or null.</summary>
    private static Vector3? DropSpotInCamp(Entity carrier)
    {
        Vector3? center = carrier?.Intelligence?.CurrentExpedition?.Center;
        if (!center.HasValue || !carrier.Location.HasValue)
        {
            return null;
        }
        SubtileInfluence influence = SubtileInfluence.FindFreeSpotNearLocation(center.Value, 15, carrier.Location, carrier, useMovementMap: true);
        if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(influence.Values, out var best) == -1)
        {
            return null;
        }
        return MapManager.SubTileToWorldPos(new Point(influence.TopLeftSubtilePositionOfMap.X + best.X, influence.TopLeftSubtilePositionOfMap.Y + best.Y)).ToVector3();
    }

    private static string NameOf(EntityType type) => (type.Name ?? type.KeyName).ToLowerInvariant();

    /// <summary>The log line tripleacoder asked for: "takes the steel axe back to the stockpile before going on."</summary>
    public static void LogTakenBack(Entity carrier, Entity item, string where)
    {
        if (The.Client == null || carrier?.Intelligence?.Allegiance == null || item == null)
        {
            return;
        }
        The.Client.AddLogEvent(carrier.Intelligence.Allegiance, The.Client.Log.GeneralEvent, carrier,
            "takes the " + NameOf(item.EntityType) + " back to " + where + " before going on.");
    }
}
