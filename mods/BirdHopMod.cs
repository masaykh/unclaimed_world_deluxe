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
/// GoalTakeFive.Activate - with a chance per idle turn where the type has none.
///
/// AFTER KASTUK TRIED IT: "Bird may occasionally move, when human is coming, but immediately move
/// back or stuck on new position ... length of the leash can be raised to 5 tiles ... May add them
/// fear of any other creatures, so they will shift from the shore into water, when someone come."
/// - The reach is now up to 5 tiles (<see cref="HopMaxDistance"/>).
/// - STARTLED: while resting, a bird watches for any creature or person of another group within
///   <see cref="StartleRange"/> (GoalDoTakeFive, <see cref="IsStartled"/>). Seeing one ends its rest,
///   and the hop that follows is certain, at least two tiles, and WITHOUT the pull back towards its
///   group's centre - the pull that sent it straight back. Others count against a spot as they
///   always did, so it steps away from them. It walks, so how far into water it can go is the
///   terrain's say: where the shallows are walkable it goes there, otherwise along the shore.
/// It still does not wander, hunt or patrol. Off by default.
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

    /// <summary>5 tiles, Kastuk's leash.</summary>
    public const float HopMaxDistance = 240f;

    /// <summary>A startled hop goes at least this far: two tiles, well clear of whoever came.</summary>
    public const float StartledMinDistance = 96f;

    /// <summary>How close another group's creature or person comes before a bird takes fright: 3 tiles.</summary>
    public const float StartleRange = 144f;

    /// <summary>How often a resting bird looks round, in game seconds.</summary>
    public const double WatchEverySeconds = 0.5;

    /// <summary>
    /// The nearest creature or person of another group within <see cref="StartleRange"/> that this
    /// bird's side knows of, or null. Read from PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite, the
    /// list the threat maps are drawn from.
    /// </summary>
    public static Entity Startler(Entity bird)
    {
        var known = bird?.Intelligence?.Allegiance?.SharedKnowledge?.PlaySiteKnowledge?.AllKnownOutsideAgentsOnPlaySite;
        if (known == null || !bird.Location.HasValue)
        {
            return null;
        }
        Entity nearest = null;
        float best = StartleRange;
        foreach (EntityID id in known.Keys)
        {
            Entity other = Entity.FindByID(id);
            if (other == null || other == bird || other.EntityType == bird.EntityType || !other.Location.HasValue || other.ContainedBy.HasValue)
            {
                continue;
            }
            float d = Microsoft.Xna.Framework.Vector3.Distance(bird.PlaySiteLocation, other.PlaySiteLocation);
            if (d <= best)
            {
                nearest = other;
                best = d;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Called from GoalDoTakeFive every frame: whether a resting hopper has just seen someone close
    /// and should stop resting (so GoalTakeFive starts again and hops away). Checks every
    /// <see cref="WatchEverySeconds"/>.
    /// </summary>
    public static bool IsStartled(Entity bird, ref double secondsSinceWatch, double elapsedSeconds)
    {
        if (!Enabled.On || bird?.EntityType?.IntelligenceType == null || bird.EntityType.IntelligenceType.IsMobile
            || Array.IndexOf(HopperKeys, bird.EntityType.KeyName) < 0)
        {
            return false;
        }
        secondsSinceWatch += elapsedSeconds;
        if (secondsSinceWatch < WatchEverySeconds)
        {
            return false;
        }
        secondsSinceWatch = 0.0;
        return MayHop(bird) && Startler(bird) != null;
    }

    /// <summary>
    /// Whether this immobile creature may take GoalTakeFive's short idle step - and, if so, that it
    /// has what walking needs.
    ///
    /// THE CRASH (Kastuk, with Errors.txt): NullReferenceException in GoalMoveToPosition.GetPath
    /// the first time a bird hopped, which was when the view first reached where birds live.
    /// Intelligence.ComeOnline gives a PathPlanner only to creatures marked mobile, and GetPath's
    /// short-path branch calls entityIntelligence.PathPlanner.FindShortPathDirectly. So no hop had
    /// ever run. Now the bird gets its planner here, the way ComeOnline makes one for a mobile
    /// creature - and only once its side has a movement map for its kind, stance and way of moving,
    /// which GetPath reads next; without one it simply stays where it is.
    /// </summary>
    public static bool MayHop(Entity entity)
    {
        if (!Enabled.On || entity?.EntityType == null || Array.IndexOf(HopperKeys, entity.EntityType.KeyName) < 0)
        {
            return false;
        }
        var intelligence = entity.Intelligence;
        var maps = intelligence?.Allegiance?.SharedKnowledge?.PlaySiteKnowledge?.AllMovementMaps;
        if (maps == null || !entity.IsOnPlaySite()
            || !maps.TryGetValue(UWGame.SimSide.Maps.ProtectionLevel.Exposed, out var byType)
            || !byType.TryGetValue(entity.EntityType, out var byStance)
            || !byStance.TryGetValue(intelligence.ThreatStance, out var map)
            || map?.GetCurrent()?.Layers == null
            || !map.GetCurrent().Layers.ContainsKey(entity.GetTransportType()))
        {
            return false;
        }
        if (intelligence.PathPlanner == null)
        {
            intelligence.PathPlanner = new UWGame.SimSide.AI.Pathfinding.PathPlanner(entity);
        }
        return true;
    }
}
