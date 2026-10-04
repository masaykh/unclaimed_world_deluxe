using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// Two hunting tweaks Kastuk asked for in "Autoclaim of bodies": "About hunting job. Need tweaking
/// of max range, add modifier for it. Also, Swarmers cannot be selected in hunting zone, only by
/// direct order to hunt particular beast."
///
/// CHASE RANGE. Outside a hunting zone, a hunter gives up once the prey is further from the camp
/// centre than AIConstants.MaximumDistanceFromExpeditionToChasePrey - 1200, 25 tiles
/// (GoalHunt.TargetIsTooFarFromExpedition, which AttackJob also uses for its "too far from camp"
/// warning). This multiplies that distance. Inside a hunting zone there is no limit, as before.
///
/// SWARMERS IN HUNTING ZONES. Not a port bug: the studio's human prey list (CreatureLoader,
/// entity:human Prey) names sixteen creatures and not the swarmer. That list is what the Hunt
/// window offers (HuntWindow.GetPreyToDisplay), what counts as spotted prey
/// (SharedKnowledge.SpottedPrey) and what standing hunt orders cover (HuntingJobManager). A direct
/// order never reads it, which is why that worked. With this switch the swarmer is added to the
/// list when the tables are built (BaseDataLoader.InitEntityTypes, before
/// IntelligenceType.PostLoadContentInitialize turns Prey into PreyTypes). The swarmer has a carcass
/// (item:swarmerCarcass), so a hunt order has something to keep in store. The same list also
/// feeds other human groups' own hunting (GoapFindPreyAction), so they may hunt swarmers too.
///
/// AUTOCLAIM IN CAMP. Kastuk, same thread: "May also add switcher to Policy window to autoclaim
/// animals killed in camp area." The carcass's owner is decided in Entity.Kill, from the killing
/// blow's ownerOfCarcass or the dying creature's own owner (GoalThink, Entity.UpdateBiological,
/// Entity.CarcassOwnerOnLethalBlow). Only a hunt's blow carries an owner - the studio's "don't claim
/// the carcass when not hunting" (EvaluateAttackJobs.SetGoal) - so what the colony's people, dogs,
/// HOUNDs and sentries kill outside a hunt is left with no owner, like an animal killed by wildlife
/// or one that starved. With the policy on, such a carcass goes to the expedition when it lies within the
/// camp's forage and hunting radius (Allegiance.GetForageAndHuntingRadius - the circle the scouting
/// overlay draws, MapClient.DrawExpeditionScoutingRadius) - as the Claim button would give it
/// (Commands.Claim: ChangeOwnership, with the new owner told of it), and haulers are sent at once
/// (HaulingJobManager.CreateHaulingJobsForItemOutOfBand), as for any claimed carcass.
/// • Animals only: a dead person's body is never claimed this way.
/// • A carcass that already has an owner - another group's livestock, a kill by people who respect
///   ownership - is never taken from it.
/// • A creature that dies inside something (a trap, a building) leaves its carcass there,
///   owned as the studio decides; only one that dies on open ground is claimed.
///
/// WHERE THE SWITCH IS KEPT. A checkbox on the policy window's weapons page (WeaponsPage), next to
/// the vermin ammunition switches, through a SetAutoclaimCampKills command so that replays carry
/// it. The value is kept in the expedition's saved custom fields (Expedition.SetPropertyValue), as
/// ReserveMod keeps its reserves - no new saved field and no save-format version, so a save loads
/// in any build, older or without the mod, which simply never reads the key. The checkbox appears
/// only while the AUTOCLAIM SWITCH IN THE POLICY WINDOW setting is on.
///
/// All off by default. The prey list is read at load, so that switch takes effect on the next load.
/// </summary>
public static class HuntingMod
{
    public const string ModId = "hunting";

    public const string SwarmerKey = "entity:swarmer";

    public const string HumanKey = "entity:human";

    private static ModSetting chaseRange;

    private static ModSetting swarmersInZones;

    public static ModSetting ChaseRange =>
        chaseRange ?? (chaseRange = ModSettings.Choice(
            ModId, "chaseRange", "HUNTERS CHASE PREY UP TO", new[] { "x1", "x1.5", "x2", "x3" }, "x1",
            toolTip: "How far from the camp centre a hunter keeps chasing prey outside a hunting " +
                     "zone. x1 is the studio's 25 tiles. Inside a hunting zone there is no limit.",
            affectsSimulation: true));

    public static ModSetting SwarmersInZones =>
        swarmersInZones ?? (swarmersInZones = ModSettings.Toggle(
            ModId, "swarmersInZones", "SWARMERS CAN BE HUNTED IN ZONES", defaultValue: false,
            toolTip: "Adds swarmers to what your people count as prey, so a hunting zone offers " +
                     "them and standing hunt orders cover them. Takes effect on the next load.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    private static ModSetting autoclaimSwitch;

    public static ModSetting AutoclaimSwitch =>
        autoclaimSwitch ?? (autoclaimSwitch = ModSettings.Toggle(
            ModId, "autoclaimSwitch", "AUTOCLAIM SWITCH IN THE POLICY WINDOW", defaultValue: false,
            toolTip: "Adds CLAIM ANIMALS THAT DIE IN CAMP to the policy window's weapons page. Ticked, " +
                     "an animal carcass nobody owns, within the camp's hunting radius, becomes yours " +
                     "and is hauled in. Reopen the policy window after switching this.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUNTING");
        _ = ChaseRange;
        _ = SwarmersInZones;
        _ = AutoclaimSwitch;
    }

    /// <summary>Whether the weapons page shows the autoclaim checkbox, and whether the policy is read.</summary>
    public static bool OffersAutoclaim => AutoclaimSwitch.On;

    /// <summary>The policy's key in the expedition's custom fields.</summary>
    public const string AutoclaimKey = "hunting:autoclaimCampKills";

    /// <summary>Whether this expedition's policy claims animals that die in camp unowned. Off unless set.</summary>
    public static bool AutoclaimsCampKills(Expedition expedition) =>
        expedition?.GetPropertyValue(AutoclaimKey, null, null)?.BoolResult == true;

    /// <summary>Sets the policy. Called from the SetAutoclaimCampKills command only.</summary>
    public static void SetAutoclaimCampKills(Expedition expedition, bool claim)
    {
        expedition?.SetPropertyValue(AutoclaimKey, claim
            ? new PropertyResult { PropertyKeyName = AutoclaimKey, BoolResult = true }
            : (PropertyResult?)null);
    }

    /// <summary>Animals only: whatever is alive (Entity.Kill leaves a carcass of anything with a BiologicalType) and is not a person.</summary>
    public static bool IsClaimableAnimal(EntityType type) =>
        type?.BiologicalType != null && type.Person == null;

    /// <summary>
    /// Called from Entity.Kill for a carcass about to be placed on open ground with no owner: the
    /// player expedition that claims it, or null. The first expedition, in the play site's own
    /// order, whose policy is on and whose camp radius holds the spot - deterministic, so a replay
    /// claims the same carcass for the same expedition.
    /// </summary>
    public static IOwner CampKillClaimant(Entity dying, Vector3 location)
    {
        if (!OffersAutoclaim || !IsClaimableAnimal(dying?.EntityType))
        {
            return null;
        }
        Vector2 at = location.ToVector2();
        foreach (Allegiance allegiance in The.Sim.PlaySite.Allegiances)
        {
            if (allegiance.AllegianceType != AllegianceType.Player)
            {
                continue;
            }
            float radius = allegiance.GetForageAndHuntingRadius();
            foreach (Expedition expedition in allegiance.Expeditions)
            {
                if (expedition.Location.HasValue && ClaimsKillAt(expedition, expedition.Location.Value.ToVector2(), radius, at))
                {
                    return expedition;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// The policy's whole rule for one expedition, apart from where the camp is: the mod's switch
    /// is on, this expedition's box is ticked, and the carcass lies within radius (world units, as
    /// Allegiance.GetForageAndHuntingRadius gives it) of the camp centre. --hunting-selftest checks
    /// it without a play site.
    /// </summary>
    public static bool ClaimsKillAt(Expedition expedition, Vector2 campCentre, float radius, Vector2 at) =>
        OffersAutoclaim && AutoclaimsCampKills(expedition) && Vector2.DistanceSquared(campCentre, at) <= radius * radius;

    /// <summary>The chase distance's multiplier: 1 unless changed.</summary>
    public static float ChaseRangeFactor() => ParseFactor(ChaseRange.Value);

    public static float ParseFactor(string value)
    {
        string number = value?.TrimStart('x', 'X');
        return float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) && f > 0f ? f : 1f;
    }

    /// <summary>Called from BaseDataLoader.InitEntityTypes: puts the swarmer on the human prey list.</summary>
    public static void AdjustCreatures(List<EntityType> types)
    {
        if (!SwarmersInZones.On || types == null)
        {
            return;
        }
        EntityType human = types.FirstOrDefault(t => t?.KeyName == HumanKey);
        string[] prey = human?.IntelligenceType?.Prey;
        if (prey == null || prey.Contains(SwarmerKey) || !types.Any(t => t?.KeyName == SwarmerKey))
        {
            return;
        }
        human.IntelligenceType.Prey = prey.Concat(new[] { SwarmerKey }).ToArray();
    }
}
