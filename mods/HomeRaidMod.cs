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
/// HOW. tripleacoder's shape: an evaluator and a top-level goal, both in core because goals are
/// saved by type name (EvaluateBreakIn, GoalBreakIn). GoalThink gives the evaluator to the raider
/// species only. It scores a raid just above idling, so the brain chooses it when the predator has
/// nothing better to do, and leaves it for anything that scores higher - eating, sleeping,
/// answering an attack. The goal walks to the door (GoalMoveToPosition), then wears the building
/// down (Entity.DoDamage, which already damages integrity and parts) playing the attack animation.
/// When it breaks it is destroyed through the studio's own path - HomeContainer /
/// UpgradableBuildingContainer.Destroy - which already ejects the occupants and drops the stored
/// items. From there the ordinary AI takes over: sleepers are visible targets, and food is on the
/// ground. The raid is part of the predator's saved brain, so it carries on after a load.
///
/// This file keeps the rules: who raids, what is worth raiding, how fast a building gives way.
/// Off by default.
/// </summary>
public static class HomeRaidMod
{
    public const string ModId = "homeraid";

    private static ModSetting enabled;
    private static ModSetting breakInSeconds;

    public static ModSetting EnabledSetting =>
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
        _ = EnabledSetting;
        _ = BreakInSeconds;
    }

    /// <summary>Whether raids happen at all. Read by EvaluateBreakIn and GoalBreakIn.</summary>
    public static bool Enabled => EnabledSetting.On;

    /// <summary>The raiders: the big hunters, all around 100 bulk.</summary>
    public static readonly string[] RaiderKeys = { "entity:patrician", "entity:whipjaw", "entity:megapod", "entity:bushDragon" };

    /// <summary>Whether this species gets EvaluateBreakIn. By species only, so switching the mod on mid-game works.</summary>
    public static bool IsRaider(EntityType type) => type != null && Array.IndexOf(RaiderKeys, type.KeyName) >= 0;

    /// <summary>How close to the door counts as at it.</summary>
    public const float ReachDistance = 72f;

    /// <summary>A raid that has not reached the door by then is given up.</summary>
    public const double GiveUpAfterSeconds = 60.0;

    private const double RebuildEverySeconds = 5.0;
    private const float NightBelowLightLevel = 0.3f;

    /// <summary>A building's integrity lost per second of one predator at its door: a sound one breaks in BREAK IN seconds.</summary>
    public static float DamagePerSecond() => 1f / Math.Max(1f, ParseSeconds(BreakInSeconds.Value));

    private static readonly List<Entity> raidable = new List<Entity>();
    private static double builtAt = double.MinValue;
    private static object builtFor;

    /// <summary>
    /// The building this predator would raid: the nearest raidable one within its aggro range, or
    /// null. Called from EvaluateBreakIn.
    /// </summary>
    public static Entity FindTarget(Entity predator)
    {
        if (!Enabled || predator?.Location == null)
        {
            return null;
        }
        List<Entity> buildings = RaidableBuildings();
        Entity best = null;
        float bestDistance = predator.GetAggroRange() ?? 0f;
        foreach (Entity building in buildings)
        {
            if (building.AccessPoint == null || Entity.FindByID(building.EntityID) == null)
            {
                continue;
            }
            float d = Vector3.Distance(predator.PlaySiteLocation, building.PlaySiteLocation);
            if (d <= bestDistance)
            {
                best = building;
                bestDistance = d;
            }
        }
        return best;
    }

    /// <summary>The player's buildings with colonists asleep inside (at night) or food in storage, rebuilt every few seconds.</summary>
    private static List<Entity> RaidableBuildings()
    {
        Sim sim = The.Sim;
        if (sim?.PlaySite?.PlayerAllegiance == null || sim.Mode != Sim.EngineMode.Game)
        {
            raidable.Clear();
            return raidable;
        }
        double now = sim.TotalUnPausedGameTimeInSeconds;
        if (builtFor == sim.PlaySite && now >= builtAt && now - builtAt < RebuildEverySeconds)
        {
            return raidable;
        }
        builtFor = sim.PlaySite;
        builtAt = now;
        raidable.Clear();
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
                        raidable.Add(building);
                    }
                }
            }
        }
        return raidable;
    }

    public static float ParseSeconds(string value) =>
        float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float s) && s > 0f ? s : 60f;
}
