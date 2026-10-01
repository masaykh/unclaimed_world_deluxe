using System;
using System.Collections.Generic;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// Small prey runs from what hunts it, and a dog standing right against its prey can bite it.
///
/// THE REPORT. Kastuk, "Dangerous fauna": "two dogs can kill one field quadite by like 10 hits ...
/// over 1 minute. Vermin just peacefully walking around, idling, even eating something, not
/// escaping at all ... Raising attack speed is not changing much ... Still cannot hit the moving
/// prey." His screenshot has a field quadite EATING while a dog bites it.
///
/// WHY PREY DOES NOT RUN (original_src). Fleeing is CompositeGoal.ValidateSafetyAndTakeAction: a
/// creature panics when its discomfort map at its tile is above its PanicLevel. Two things stop it:
/// - GoalEat never calls it. The studio's own comment: "Unless this method is called during the
///   goal, the agent will not be able to flee". An eating animal cannot flee at all.
/// - The quadite's and the rat's Boldness is 1.49, so ThreatMap's boldness factor is
///   1 - 1.49 + 0.5 = 0.01 and a dog paints a threat of about 20 that has faded to the panic level
///   of 4 about a tile away. The studio chose it: "1.49f gives a small threat radius".
/// So: eating prey runs the same check idle prey does, and vermin that cannot fight back get a
/// floor under that factor (<see cref="FearFloor"/>, about four tiles from a dog, further from
/// stronger hunters, since danger scales with the hunter's strength).
///
/// WHY DOGS MISS (original_src). Once a bite starts it lands - there is no range check at the
/// strike, and ComputeChanceToHit has no penalty for a moving target. Bites are lost BEFORE the
/// swing: GoalDoAttack only starts when AttackJob's melee test passes, and that test is two-sided -
/// within 10 px either side of the two melee radii, a 20 px band for dog and quadite. Too close
/// fails as surely as too far, a quadite walking at 35-50 px/s crosses the band during the dog's
/// turn, and each failure re-plans (with a 35% chance of giving up the chase). That fixed overhead
/// is why ATTACK SPEED changed little. <see cref="CloseCountsAsInReach"/> makes it one-sided:
/// standing closer than the band counts as in reach.
///
/// THE REST IS DATA, and DANGEROUS FAUNA already reaches it: a quadite has 140 hitpoints
/// (resilience 7 at 20 kg) under a shell that takes 4 off a 10-point bite; a binal rat has 15 and
/// a hide that takes 2 - which is why the rat goes down in two bites. The dog's DAMAGE multiplier
/// applies before armour, so x2 triples what a bite does to a quadite.
///
/// Both switches off by default.
/// </summary>
public static class PreyFearMod
{
    public const string ModId = "preyfear";

    private static ModSetting preyFlees;

    private static ModSetting closeInReach;

    public static ModSetting PreyFleesSetting =>
        preyFlees ?? (preyFlees = ModSettings.Toggle(
            ModId, "preyFlees", "SMALL PREY RUNS FROM HUNTERS", defaultValue: false,
            toolTip: "Rats, field quadites, mud worms and other vermin that cannot fight back keep " +
                     "further from dogs and predators, and stop eating to run when one comes close.",
            affectsSimulation: true));

    public static ModSetting CloseInReachSetting =>
        closeInReach ?? (closeInReach = ModSettings.Toggle(
            ModId, "closeInReach", "IN MELEE, CLOSE COUNTS AS IN REACH", defaultValue: false,
            toolTip: "A melee attacker standing closer to its target than its ideal striking distance " +
                     "may strike, instead of stepping back and turning again - so dogs can bite prey " +
                     "that keeps moving. Applies to every melee fighter.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "PREY AND MELEE");
        _ = PreyFleesSetting;
        _ = CloseInReachSetting;
    }

    public static bool Enabled => PreyFleesSetting.On || CloseInReachSetting.On;

    /// <summary>The lowest boldness factor a timid creature's threat map uses: a dog's threat fades to panic level about four tiles out.</summary>
    public const float FearFloor = 0.025f;

    /// <summary>Vermin that cannot fight back: the prey this is about.</summary>
    public static bool IsTimidPrey(EntityType type) =>
        type?.BiologicalType != null && type.BiologicalType.IsVermin && type.IntelligenceType?.CanAttack != true;

    /// <summary>Called from ThreatMap for the creature whose map it is: the studio's factor, or the floor for timid prey.</summary>
    public static float BoldnessFactor(EntityType owner, float studioFactor) =>
        PreyFleesSetting.On && IsTimidPrey(owner) ? Math.Max(studioFactor, FearFloor) : studioFactor;

    /// <summary>Called from GoalEat: whether this eater should run the panic check the studio left out of eating.</summary>
    public static bool ChecksSafetyWhileEating(Entity eater) =>
        PreyFleesSetting.On && eater?.EntityType?.Person == null && IsTimidPrey(eater.EntityType);

    /// <summary>How often idle timid prey looks round for a hunter (GoalDoTakeFive), in game seconds.</summary>
    public const double WatchEverySeconds = 0.25;

    /// <summary>
    /// Called from GoalDoTakeFive: whether this idler should check for danger continuously rather than
    /// between idle animations. Kastuk, after the first version: quadites ran, then stopped "and
    /// reconsider situation, trying to scout or idle, then got bitten and running away again" - the
    /// studio checks only when an idle animation ends, and the dog arrived in between.
    /// </summary>
    public static bool IsWatchful(Entity idler) =>
        PreyFleesSetting.On && idler?.EntityType?.Person == null && IsTimidPrey(idler.EntityType);

    /// <summary>Called from AttackJob's melee test: whether being closer than the ideal distance still counts.</summary>
    public static bool CloseCountsAsInReach => CloseInReachSetting.On;

    /// <summary>
    /// Called from CompositeGoal.PerformPanicFleeing when FindPathToSafety found nothing: a way out for
    /// cornered timid prey, or null for the studio's behaviour (stand still two seconds, look again).
    ///
    /// Kastuk: a quadite chased by dogs to the shore of a lake was "trapped there, never trying to find
    /// other way". Safety is a tile with no discomfort at all; with a dog on one side and water on the
    /// other, none is reachable, so the studio stood the animal still and tried again - in the same
    /// place. Instead it now heads for anywhere LESS uncomfortable than where it stands (the studio's
    /// own FindPathToComfort), which runs it along the shore and round the dog; and if even that is
    /// blocked by the threat itself, the same search through threatened ground (the Exposed movement
    /// map), as a cornered animal would bolt past.
    /// </summary>
    public static List<PathFinderNode> PathWhenCornered(DiscomfortMap dMap, Entity entity)
    {
        if (!PreyFleesSetting.On || entity?.MapPosition == null || !IsTimidPrey(entity.EntityType))
        {
            return null;
        }
        byte here = dMap.Map.GetValue(entity.MapPosition.Value);
        if (here == 0)
        {
            return null;
        }
        List<PathFinderNode> path = CompositeGoal.FindPathToComfort(dMap, entity, here);
        if (path != null && path.Count > 1)
        {
            return path;
        }
        var predicate = (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate),
            new InfluenceMap.DiscomfortTileIsBelowValueParameters(dMap, here), The.Map.InfluenceMapTileIsComfortableInfo);
        SubtileLayers exposed = entity.Intelligence.Allegiance.SharedKnowledge
            .GetMovementMap(ProtectionLevel.Exposed, entity.EntityType, ThreatStance.Bold).Layers[entity.GetTransportType()];
        path = entity.Intelligence.PathPlanner.FindItemAndGetPath(exposed, predicate, 6000, entity.PlaySiteLocation);
        return path != null && path.Count > 1 ? path : null;
    }
}
