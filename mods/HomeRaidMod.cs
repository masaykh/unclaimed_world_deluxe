using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// Big predators break into homes with sleeping colonists, and into stores with food.
///
/// THE REQUEST. Kastuk, "Nature can be more natural", point 5: "Hungry animals can start attacking
/// structures with contained food, damaging integrity. Broken structure will expose all contained
/// items to open air." And later: "All big apex predators is fit for break and enter crimes. They
/// may be not just hungry, but also attack homes with sleeping colonists inside."
///
/// WHY SLEEPERS ARE SAFE TODAY. A sleeping colonist is contained in its home's garrison, and
/// SharedKnowledge.CanSeeEntity hides a contained entity from anyone outside its allegiance - so no
/// animal can ever pick a sleeper as a target. And the combat path cannot hit a structure at all:
/// AttackType.HitTarget needs a Body, and buildings have none.
///
/// WHAT THIS DOES. Every few game seconds, each listed predator that is idle (nothing but its
/// resting goal) looks for the nearest player building within its aggro range that has colonists
/// inside at night, or food in storage. It walks there (the studio's GoalMoveToPosition) and, once
/// at the door, wears the building down (Entity.DoDamage, which already damages integrity and parts)
/// playing its attack animation. When the building breaks it is destroyed through the studio's own
/// path - HomeContainer / UpgradableBuildingContainer.Destroy - which already ejects the occupants
/// and drops the stored items. From there the ordinary AI takes over: sleepers are visible targets,
/// and food is on the ground.
///
/// A raid gives up if the predator is attacked or turns to anything else, if the building is gone,
/// or after a minute without reaching it. The raid list is kept in memory only - after a load, raids
/// simply start again; the damage done is already in the building's saved integrity. Off by default.
/// </summary>
public static class HomeRaidMod
{
    public const string ModId = "homeraid";

    private static ModSetting enabled;
    private static ModSetting breakInSeconds;

    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "PREDATORS BREAK INTO HOMES AND STORES", defaultValue: false,
            toolTip: "Patricians, whipjaws, megapods and bush dragons nearby may break into a home " +
                     "with colonists asleep inside (at night) or a store holding food. A broken " +
                     "building is destroyed: its occupants and contents are thrown out.",
            affectsSimulation: true));

    public static ModSetting BreakInSeconds =>
        breakInSeconds ?? (breakInSeconds = ModSettings.Choice(
            ModId, "breakInSeconds", "TIME TO BREAK IN (GAME SECONDS)", new[] { "30", "60", "120", "240" }, "60",
            toolTip: "How long one predator takes to break a sound building open. Two take half as long.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HOME RAIDS");
        _ = Enabled;
        _ = BreakInSeconds;
    }

    /// <summary>The raiders: the big hunters, all around 100 bulk.</summary>
    public static readonly string[] RaiderKeys = { "entity:patrician", "entity:whipjaw", "entity:megapod", "entity:bushDragon" };

    /// <summary>How close to the door counts as at it.</summary>
    public const float ReachDistance = 72f;

    private const double CheckEverySeconds = 5.0;
    private const double GiveUpAfterSeconds = 60.0;
    private const float NightBelowLightLevel = 0.3f;

    private sealed class Raid
    {
        public EntityID Target;
        public double StartedAt;
        public bool AtDoor;
    }

    private static readonly Dictionary<EntityID, Raid> raids = new Dictionary<EntityID, Raid>();
    private static double lastCheck = double.MinValue;
    private static double lastTick = double.MinValue;

    /// <summary>Called from Sim.Update every frame; returns at once unless switched on.</summary>
    public static void Update(Sim sim)
    {
        if (!Enabled.On || sim?.PlaySite?.PlayerAllegiance == null || sim.Mode != Sim.EngineMode.Game)
        {
            return;
        }
        double now = sim.TotalUnPausedGameTimeInSeconds;
        double dt = lastTick == double.MinValue || now < lastTick ? 0.0 : now - lastTick;
        lastTick = now;
        ContinueRaids(now, dt);
        if (now - lastCheck < CheckEverySeconds && now >= lastCheck)
        {
            return;
        }
        lastCheck = now;
        StartRaids(sim, now);
    }

    private static void StartRaids(Sim sim, double now)
    {
        List<Entity> targets = null;
        foreach (string key in RaiderKeys)
        {
            if (!sim.PlaySite.EntitiesByType.TryGetValue(key, out var ids))
            {
                continue;
            }
            foreach (EntityID id in ids)
            {
                Entity predator = Entity.FindByID(id);
                if (predator == null || raids.ContainsKey(id) || !IsIdle(predator) || !predator.Location.HasValue)
                {
                    continue;
                }
                targets = targets ?? RaidableBuildings(sim);
                Entity target = Nearest(predator, targets, predator.GetAggroRange() ?? 0f);
                if (target?.AccessPoint == null)
                {
                    continue;
                }
                predator.Intelligence.Brain.RemoveAllSubgoals();
                predator.Intelligence.Brain.AddSubgoal(new GoalMoveToPosition(predator, target.AccessPoint.Value, null));
                raids[id] = new Raid { Target = target.EntityID, StartedAt = now };
            }
        }
    }

    private static void ContinueRaids(double now, double dt)
    {
        if (raids.Count == 0)
        {
            return;
        }
        float perSecond = 1f / Math.Max(1f, ParseSeconds(BreakInSeconds.Value));
        var finished = new List<EntityID>();
        foreach (var pair in raids)
        {
            Entity predator = Entity.FindByID(pair.Key);
            Entity target = Entity.FindByID(pair.Value.Target);
            bool attacked = predator?.Intelligence?.Memory?.GetLastAttacker() != null;
            if (predator == null || target == null || !predator.Location.HasValue || attacked
                || (!pair.Value.AtDoor && now - pair.Value.StartedAt > GiveUpAfterSeconds))
            {
                finished.Add(pair.Key);
                continue;
            }
            float distance = Vector3.Distance(predator.PlaySiteLocation, target.AccessPoint ?? target.PlaySiteLocation);
            if (distance > ReachDistance)
            {
                // Still on its way - unless its own brain has moved on to something else.
                if (!(Front(predator) is GoalMoveToPosition))
                {
                    finished.Add(pair.Key);
                }
                continue;
            }
            pair.Value.AtDoor = true;
            predator.Renderable?.SetAnimationActionStateFlag(AnimAction.Attacking);
            if (target.DoDamage(perSecond * (float)dt) && Entity.FindByID(pair.Value.Target) != null)
            {
                // Broken: out through the studio's own Destroy, which throws out who and what was inside.
                target.Destroy();
                finished.Add(pair.Key);
            }
        }
        foreach (EntityID id in finished)
        {
            Entity.FindByID(id)?.Renderable?.ClearAnimationActionStateFlag(AnimAction.Attacking);
            raids.Remove(id);
        }
    }

    /// <summary>Resting, or doing nothing: the only state a raid may interrupt.</summary>
    private static bool IsIdle(Entity predator)
    {
        var brain = predator.Intelligence?.Brain;
        if (brain == null) return false;
        Goal front = Front(predator);
        return front == null || front is GoalTakeFive;
    }

    private static Goal Front(Entity entity)
    {
        var goals = entity.Intelligence?.Brain?.Subgoals;
        return goals != null && goals.Count > 0 ? goals.Peek() : null;
    }

    /// <summary>The player's buildings with colonists asleep inside (at night) or food in storage.</summary>
    private static List<Entity> RaidableBuildings(Sim sim)
    {
        var found = new List<Entity>();
        bool night = sim.DateAndTime != null && sim.DateAndTime.LightLevel < NightBelowLightLevel;
        var holdingFood = new HashSet<EntityID>();
        foreach (Expedition expedition in sim.PlaySite.PlayerAllegiance.Expeditions)
        {
            var food = expedition?.OwnedEntities?.Food;
            if (food == null) continue;
            foreach (var byType in food)
            {
                foreach (EntityID id in byType.Value)
                {
                    Entity item = Entity.FindByID(id);
                    if (item?.ContainedBy != null) holdingFood.Add(item.ContainedBy.Value);
                }
            }
        }
        foreach (Expedition expedition in sim.PlaySite.PlayerAllegiance.Expeditions)
        {
            var structures = expedition?.OwnedEntities?.Structures;
            if (structures == null) continue;
            foreach (var byType in structures)
            {
                foreach (EntityID id in byType.Value)
                {
                    Entity building = Entity.FindByID(id);
                    if (building == null || !building.IsOnPlaySite()) continue;
                    bool sleepers = night && building.Contains is IGarrison garrison && garrison.GetNoOfAgentsInside() > 0;
                    if (sleepers || holdingFood.Contains(id))
                    {
                        found.Add(building);
                    }
                }
            }
        }
        return found;
    }

    private static Entity Nearest(Entity predator, List<Entity> buildings, float range)
    {
        Entity best = null;
        float bestDistance = range;
        foreach (Entity building in buildings)
        {
            float d = Vector3.Distance(predator.PlaySiteLocation, building.PlaySiteLocation);
            if (d <= bestDistance)
            {
                best = building;
                bestDistance = d;
            }
        }
        return best;
    }

    public static float ParseSeconds(string value) =>
        float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float s) && s > 0f ? s : 60f;
}
