using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UWGame.SimSide.Entities;

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
/// Both off by default. The prey list is read at load, so the switch takes effect on the next load.
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

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUNTING");
        _ = ChaseRange;
        _ = SwarmersInZones;
    }

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
