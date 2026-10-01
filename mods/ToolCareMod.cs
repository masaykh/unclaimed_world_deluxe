using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Tiers;

namespace UWGame.Mods;

/// <summary>
/// Colonists don't leave good tools and weapons lying in the woods: they drop them last, and when
/// one must go, they carry it back to camp first.
///
/// THE REQUEST. Kastuk, "Dropping tools": colonists drop high-tech weapons and tools far from camp
/// when they switch jobs, and they lie there for hours. tripleacoder: dropping is often right, but
/// valuable items deserve better; don't use task priority for it (priority is the player's), let the
/// carrier take it back itself; and say so in the log.
///
/// WHICH DROP (the plan posted in the thread). Only CompositeGoal.DropUnneededItemsToMakeCapacity -
/// the "this job needs my hands" drop that GoalHarvest, GoalProduce, GoalEat and GoalReplenish make.
/// Someone better claiming the tool, stationary tools set down at a work site, sleeping and hauling
/// are untouched.
///
/// WHAT IS PROTECTED (Kastuk: "any items above Tier survival"). An item's tier is its own
/// TierOrAreaType where it has one; not all do, so otherwise it is the LOWEST tier among the
/// processes that make it (ProcessType.GetTierArea) - the cheapest way anyone could have it. Above
/// "survival" (basic, medium, advanced) is protected. Weapons are protected at any tier. Computed
/// once per item type.
///
/// WHAT CHANGES, for a colonist:
/// - Protected items are dropped LAST: everything else droppable goes first, and often that frees
///   enough.
/// - If a protected item must still go and the colonist is outside the camp (further from its
///   centre than EvaluateReturnHome's DistanceFromExpeditionToReturnHome), and the goal is one that
///   works outdoors (GoalHarvest, GoalEat - CompositeGoal.MayCarryHomeFirst), the drop becomes a
///   walk to a free spot at the camp centre and the drop there - queued before the rest of the job,
///   which then carries on from camp. The haulers tidy it from there, as they do everything in camp.
/// - The log says so: "takes the steel axe back to camp before going on."
/// Off by default.
/// </summary>
public static class ToolCareMod
{
    public const string ModId = "toolcare";

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "GOOD TOOLS COME BACK TO CAMP", defaultValue: false,
            toolTip: "When a job needs a colonist's hands, weapons and anything above the survival " +
                     "tier are dropped last - and if one must go while out of camp, the colonist " +
                     "carries it back to camp first (when gathering or eating), then goes on.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "TOOL CARE");
        _ = EnabledSetting;
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
    /// What a colonist never drops just to make room: a weapon, and ammunition for a weapon it is
    /// carrying. Kastuk, "Dropping tools": a hunter switched to hauling, dropped its coil rifle at the
    /// camp's edge, walked back for it, dropped it again at the centre with its ammunition and tools,
    /// picked it up a third time and ran to a fight with no ammunition. The rifle was dropped LAST, as
    /// a protected item, but still dropped - and the studio's weapon logic then sent the colonist back
    /// for it. A haul that cannot fit everything makes more trips (GoalReplenish), so keeping these
    /// costs walking, not the job.
    /// </summary>
    public static bool KeptWhenMakingRoom(Entity carrier, EntityType type)
    {
        if (type?.ItemType == null)
        {
            return false;
        }
        if (type.ItemType.WeaponType != null)
        {
            return true;
        }
        bool isAmmoForCarriedWeapon = false;
        carrier?.AgentStorage?.ItemStorage?.IterateContainedBreakOnTrue(delegate(Entity carried)
        {
            UWGame.SimSide.Items.WeaponType weapon = carried.EntityType?.ItemType?.WeaponType;
            if (weapon?.AttackTypes != null)
            {
                foreach (var attack in weapon.AttackTypes)
                {
                    if (attack?.UsesAmmoType == type)
                    {
                        isAmmoForCarriedWeapon = true;
                        return true;
                    }
                }
            }
            return false;
        });
        return isAmmoForCarriedWeapon;
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

    /// <summary>Whether the carrier is out of camp: further from its expedition's centre than a colonist is sent home from.</summary>
    public static bool IsOutOfCamp(Entity carrier)
    {
        Vector3? center = carrier?.Intelligence?.CurrentExpedition?.Center;
        return center.HasValue && carrier.Location.HasValue
            && Vector3.Distance(carrier.PlaySiteLocation, center.Value) > GameData.Instance.AIConstants.DistanceFromExpeditionToReturnHome;
    }

    /// <summary>A free spot near the camp centre to leave a protected item at - GoalReturnHome's own search - or null.</summary>
    public static Vector3? DropSpotInCamp(Entity carrier)
    {
        Vector3? center = carrier?.Intelligence?.CurrentExpedition?.Center;
        if (!center.HasValue)
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

    /// <summary>The log line tripleacoder asked for, for protected items only.</summary>
    public static void LogCarriedHome(Entity carrier, Entity item)
    {
        if (The.Client == null || carrier?.Intelligence?.Allegiance == null || item == null)
        {
            return;
        }
        The.Client.AddLogEvent(carrier.Intelligence.Allegiance, The.Client.Log.GeneralEvent, carrier,
            "takes the " + (item.EntityType.Name ?? item.EntityType.KeyName).ToLowerInvariant() + " back to camp before going on.");
    }
}
