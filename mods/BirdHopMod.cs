using System;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// Diamond birds hop to a neighbouring cell now and then, instead of standing on one spot forever.
///
/// THE REQUEST. Kastuk, "Nature can be more natural", point 2: "Let them switch current cell to one
/// of neighbour cells around their spawn cell and move there randomly (only if there is none of
/// other things\creatures on it)." Confirmed later: "let's try this to make them less static
/// creatures." (Flying off the map and back, point 1, waits for flight animations.)
///
/// THE STUDIO ALREADY WROTE THIS MOVE. GoalTakeFive.MoveShortDistance is the idle walk every mobile
/// creature takes: a spot between an inner and outer radius, weighted towards its group's centre -
/// where it spawned - with other entities and items drawn as negative influence and blocked
/// subtiles blocked out. That is Kastuk's rule, word for word. The bird skips it only because its
/// IntelligenceType.IsMobile is false; its LeggedLocomotorType has walk speeds and its model a walk
/// animation, so it was built to take a step.
///
/// So this lets the listed creatures run that same move - from the immobile branch of
/// GoalTakeFive.Activate - with a short reach (40 to 72 units: the next cell, not a stroll) and a
/// chance per idle turn where the type has none. Nothing else about the bird changes: it still does
/// not wander, hunt, flee or patrol. Off by default.
/// </summary>
public static class BirdHopMod
{
    public const string ModId = "birdhop";

    private static ModSetting enabled;

    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "DIAMOND BIRDS HOP ABOUT", defaultValue: false,
            toolTip: "Now and then a diamond bird steps to a free neighbouring cell near where it " +
                     "settled, instead of standing on one spot forever.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        _ = Enabled;
    }

    /// <summary>The creatures that hop: the diamond bird.</summary>
    public static readonly string[] HopperKeys = { "entity:bird" };

    /// <summary>Chance per idle turn, where the type sets none. Several creatures use 0.1.</summary>
    public const float HopChance = 0.1f;

    public const float HopMinDistance = 40f;

    public const float HopMaxDistance = 72f;

    /// <summary>Whether this immobile creature may take GoalTakeFive's short idle step.</summary>
    public static bool MayHop(Entity entity) =>
        Enabled.On && entity?.EntityType != null && Array.IndexOf(HopperKeys, entity.EntityType.KeyName) >= 0;
}
