using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// Colonists do not bed down beside a known predator nest.
///
/// THE REPORT. Kastuk and tripleacoder, dev chat: colonists at Muckroot slept next to the swarmer
/// nests. Kastuk checked the threat overlay there, and it shows "yellow zone only near active
/// predators movement, not near their nests". The colonists slept out in the field near their
/// patrol zones rather than at the camp centre.
///
/// WHY. EvaluateSleep.ScoreSafety is a fixed number per kind of place - ground where you stand 0,
/// the expedition centre 0.5, home 0.75, buildings 1 - under the studio's own
/// "TODO: replace this score with a true 'safety' score??". It never looks at what is nearby. The
/// only danger check is the route (ScoreTravelTime on the stance's region map), and the threat map
/// that feeds it draws creatures, not the structures they come from. A dormant nest with no
/// swarmer standing on it is therefore perfectly safe as far as the AI can tell.
///
/// WHAT THIS DOES. A nest is what the studio's own spawning calls it: a named spawn source of a
/// wild group's Population ("Swarmer nest 1", Population.FindRandomSpawnLocation). For each group
/// whose creature hunts - a predator that can attack and has an aggro range, and is not a people
/// who trade - every spawn source the sleeper's allegiance knows about marks a circle of that
/// creature's aggro range. A sleep spot inside a circle keeps a tenth of its location score
/// (EvaluateSleep.ScoreLocation). Not zero: the expedition centre is always a candidate, so a
/// colonist in the field walks back to camp, a campfire or home, and only if every choice is by a
/// nest do they still sleep, rather than stay awake until they drop.
///
/// Only the player's people are affected; wild animals sleep at their nests as before. The nest
/// list is rebuilt every 10 game seconds and kept in memory only, so nothing is saved. Off by
/// default.
/// </summary>
public static class SafeSleepMod
{
    public const string ModId = "safesleep";

    private static ModSetting avoidNests;

    public static ModSetting AvoidNests =>
        avoidNests ?? (avoidNests = ModSettings.Toggle(
            ModId, "avoidNests", "COLONISTS DO NOT SLEEP BY PREDATOR NESTS", defaultValue: false,
            toolTip: "A sleeping spot within a known predator nest's aggro range counts for much " +
                     "less, so colonists out in the field walk back to camp, a campfire or home " +
                     "instead. Only nests your colony has seen count.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Colonists do not bed down beside a known predator nest.",
            "HUD_thumbnail_leanToBigTarp");
        ModSettings.SetCategoryLabel(ModId, "SAFE SLEEP");
        _ = AvoidNests;
    }

    public static bool Enabled => AvoidNests.On;

    /// <summary>What a spot inside a nest's circle keeps of its location score.</summary>
    public const double NearNestFactor = 0.1;

    private const double RebuildEverySeconds = 10.0;

    public struct Nest
    {
        public EntityID ID;
        public Vector3 Location;
        public float Radius;
        public Allegiance Owner;
    }

    private static readonly List<Nest> nests = new List<Nest>();
    private static double builtAt = double.MinValue;
    private static object builtFor;

    /// <summary>Whether a wild group of this creature is a danger to sleep beside.</summary>
    public static bool IsDangerous(EntityType type)
    {
        IntelligenceType intelligence = type?.IntelligenceType;
        return intelligence != null && intelligence.IsPredator && intelligence.CanAttack == true
            && intelligence.AggroRange is float range && range > 0f
            && intelligence.CanTradeAndCommunicate != true;
    }

    /// <summary>1, or <see cref="NearNestFactor"/> when this spot is inside a nest the sleeper's side knows of.</summary>
    public static double SafetyFactor(Entity sleeper, Vector3 location)
    {
        if (!Enabled || sleeper?.Intelligence?.Allegiance == null
            || sleeper.Intelligence.Allegiance.AllegianceType != AllegianceType.Player)
        {
            return 1.0;
        }
        List<Nest> known = Nests();
        if (known.Count == 0)
        {
            return 1.0;
        }
        SharedKnowledge knowledge = sleeper.Intelligence.Allegiance.SharedKnowledge;
        foreach (Nest nest in known)
        {
            if (nest.Owner == sleeper.Intelligence.Allegiance
                || Vector2.Distance(new Vector2(location.X, location.Y), new Vector2(nest.Location.X, nest.Location.Y)) > nest.Radius)
            {
                continue;
            }
            EntityResult result = knowledge.GetKnownData(nest.ID, out _);
            if (result == EntityResult.SeenDirectly || result == EntityResult.Remembered)
            {
                return NearNestFactor;
            }
        }
        return 1.0;
    }

    /// <summary>Every dangerous wild group's spawn sources on the play site, rebuilt now and then.</summary>
    public static List<Nest> Nests()
    {
        var sim = The.Sim;
        var site = sim?.PlaySite;
        if (site == null)
        {
            nests.Clear();
            return nests;
        }
        double now = sim.TotalUnPausedGameTimeInSeconds;
        if (builtFor == site && now >= builtAt && now - builtAt < RebuildEverySeconds)
        {
            return nests;
        }
        builtFor = site;
        builtAt = now;
        nests.Clear();
        foreach (Allegiance owner in site.Allegiances)
        {
            if (owner == null || owner.AllegianceType == AllegianceType.Player || !IsDangerous(owner.RepresentativeEntityType))
            {
                continue;
            }
            float radius = owner.RepresentativeEntityType.IntelligenceType.AggroRange.Value;
            foreach (Expedition group in owner.Expeditions)
            {
                Population population = group?.Population;
                if (population?.SpawnSources == null)
                {
                    continue;
                }
                foreach (string name in population.SpawnSources)
                {
                    if (name != null && site.EntitiesByName.TryGetValue(name, out EntityID id))
                    {
                        Entity source = Entity.FindByID(id);
                        if (source != null && source.Location.HasValue)
                        {
                            nests.Add(new Nest { ID = id, Location = source.PlaySiteLocation, Radius = radius, Owner = owner });
                        }
                    }
                }
            }
        }
        return nests;
    }
}
