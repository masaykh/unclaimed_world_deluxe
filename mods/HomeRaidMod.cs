using System;
using System.Collections.Generic;
using System.Linq;
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
/// At integrity 0 the building is broken, not destroyed - "just like abandoned unclaimed
/// structures", Kastuk - and stays standing to be repaired, while Container.ThrowOutContents puts
/// the occupants and the stock on the ground. From there the ordinary AI takes over: sleepers are
/// visible targets, and food is on the ground. A broken building is not raided again. The raid is
/// part of the predator's saved brain, so it carries on after a load.
///
/// ONLY WHAT IT KNOWS. tripleacoder: "RaidableBuildings should probably scan known entities,
/// otherwise the predators become omniscient." A predator raids only a building its kind has seen
/// and not yet forgotten - the game's own perception and memory, the same SharedKnowledge.
/// AllKnownEntities a creature's EvaluateEat scans for food (<see cref="Knows"/>). Its sensor sees
/// the building when it comes into view (Sensor.RollToDetect -> SharedKnowledge.SeeDetectable),
/// remembers it out of sight, and forgets it after its species' MemoryInDays. The studio's memory
/// only tracks what a critter has an interest in (Entity.HasInterestInEntity), and only the
/// whipjaw's container tag makes it interested in buildings, so the mod adds that interest for the
/// raider species while it is on (<see cref="TakesInterestIn"/>).
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
            toolTip: "Patricians, whipjaws and megapods nearby may break into a home with colonists " +
                     "asleep inside (at night) or a store holding food. A broken-into building is " +
                     "left broken, to be repaired, and its occupants and contents are thrown out.",
            affectsSimulation: true));

    public static ModSetting BreakInSeconds =>
        breakInSeconds ?? (breakInSeconds = ModSettings.Choice(
            ModId, "breakInSeconds", "TIME TO BREAK IN (GAME SECONDS)", new[] { "30", "60", "120", "240" }, "60",
            toolTip: "How long one predator takes to break a sound building open. Two take half as long.",
            affectsSimulation: true));

    public static ModSetting SightFromHome =>
        sightFromHome ?? (sightFromHome = ModSettings.Choice(
            ModId, "sightFromHome", "COLONISTS INSIDE A HOME SEE", SightChoices, "1/3",
            toolTip: "How far colonists inside a home (a hut, a tent) see, compared to outside. " +
                     "Less, and a predator gets to the door before they notice it.",
            affectsSimulation: true, stockValue: "ALL THE WAY"));

    private static ModSetting sightFromHome;

    private static readonly string[] SightChoices = { "ALL THE WAY", "1/2", "1/3", "1/4" };

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HOME RAIDS");
        _ = EnabledSetting;
        _ = BreakInSeconds;
        _ = SightFromHome;
    }

    /// <summary>Whether raids happen at all. Read by EvaluateBreakIn and GoalBreakIn.</summary>
    public static bool Enabled => EnabledSetting.On;

    /// <summary>
    /// Kastuk, 2026-10-04, testing raids: "colonists is not really sleep, but actively attack from
    /// inside the huts, killing any incoming predator. Let their detection range be reduced from
    /// inside the buildings to like 1/3 of basic." A contained colonist's Sensor sees from its
    /// home's tile with the full range of one standing outside, so the sleepers spot a raider long
    /// before it reaches the door.
    ///
    /// The player's people inside a residence (IResidence: the huts, tents, tipis and shelters; not a
    /// workshop or a vehicle, which hold people too) see COLONISTS INSIDE A HOME SEE of their range, day and night.
    /// Read by Entity.GetDaySensorRange and GetNightSensorRange. The Sensor rebuilds the tiles it
    /// sees whenever the range changes (Sensor.UpdatePlaySiteRegulated), so going in and coming out
    /// takes effect at the next sensor update.
    /// </summary>
    public static float SensorFactorInside(Entity sensing)
    {
        // Cheapest first: this runs for every detectable a sensor rolls for (Sensor.RollToDetect).
        if (sensing == null || !Enabled)
        {
            return 1f;
        }
        if (sensing.Intelligence?.Allegiance?.AllegianceType != UWGame.SimSide.Allegiances.AllegianceType.Player
            || !sensing.ContainedBy.HasValue)
        {
            return 1f;
        }
        if (!sensing.GetContainedBy(out Entity container) || !(container?.Contains is IResidence))
        {
            return 1f;
        }
        return SightFraction(SightFromHome.Value);
    }

    /// <summary>"1/3" -> 0.333; "ALL THE WAY", or anything unreadable, -> 1.</summary>
    public static float SightFraction(string value)
    {
        if (value != null && value.StartsWith("1/", StringComparison.Ordinal)
            && int.TryParse(value.Substring(2), out int parts) && parts > 0)
        {
            return 1f / parts;
        }
        return 1f;
    }

    /// <summary>
    /// The raiders: the big hunters. Not the bush dragon - Kastuk: "not such aggressive and their
    /// attacking by poison must be not such damageable for structures".
    /// </summary>
    public static readonly string[] RaiderKeys = { "entity:patrician", "entity:whipjaw", "entity:megapod" };

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

    // ---- who raids what --------------------------------------------------------------------
    //
    // tripleacoder: triggered "by hunger or aggression (territorial). Either way, it should probably
    // be an uncommon behavior." Kastuk: "If raid is triggered by hunger, then predators must have
    // ability to eat raided food." So:
    // - a store of food is raided only by a HUNGRY predator (stomach under half full), and only for
    //   food that predator eats. The three raiders eat cooked, raw and small raw meat, spoiled meals
    //   and rotten meat (CreatureLoader, FoodItemTagsThatCanBeConsumed) and butcher carcasses
    //   (ExtractionProcessTypes) - checked per predator against its own ConsumeProcesses and
    //   FoodExtractionProcesses. Once the store is open the food is on the ground, and the
    //   predator's own EvaluateEat takes it from there.
    // - a home with colonists asleep inside, at night, is the territorial case.
    // - either way, a predator raids at most once a game day (RaidCooldownDays), remembered in its
    //   saved CustomFields.

    /// <summary>Stomach below this is hungry enough to raid a food store.</summary>
    public const float HungryBelowStomach = 0.5f;

    /// <summary>Game days between two raids by one predator.</summary>
    public const double RaidCooldownDays = 1.0;

    private const string LastRaidKey = "homeRaidLastDay";

    private sealed class Raidable
    {
        public Entity Building;
        public bool Sleepers;
        public HashSet<EntityType> Food;
    }

    private static readonly List<Raidable> raidable = new List<Raidable>();
    private static double builtAt = double.MinValue;
    private static object builtFor;

    private static double Today => The.Sim?.DateAndTime?.CurrentTimeDateYear.TotalDays ?? 0.0;

    /// <summary>Called by GoalBreakIn when a raid starts: the day it did, for the cooldown.</summary>
    public static void RaidStarted(Entity predator)
    {
        if (predator == null)
        {
            return;
        }
        Entity.SetPropertyValue(ref predator.CustomFields, LastRaidKey,
            new UWGame.ClientSide.PropertyPresentation.PropertyResult { NumberResult = (float)Today });
    }

    private static bool OnCooldown(Entity predator)
    {
        if (predator.CustomFields != null && predator.CustomFields.TryGetValue(LastRaidKey, out var last) && last.NumberResult is float day)
        {
            return Today - day < RaidCooldownDays && Today >= day;
        }
        return false;
    }

    private static bool Eats(Entity predator, EntityType food)
    {
        var bio = predator.BiologicalEntity;
        return bio != null && ((bio.ConsumeProcesses != null && bio.ConsumeProcesses.ContainsKey(food))
                               || (bio.FoodExtractionProcesses != null && bio.FoodExtractionProcesses.ContainsKey(food)));
    }

    /// <summary>
    /// Called by Entity.HasInterestInEntity: whether <paramref name="watcher"/>'s kind keeps
    /// <paramref name="seen"/> in its memory because of this mod - a building with room inside, seen
    /// by a raider species, while raids are on. Without it the studio's memory leaves buildings out
    /// for the patrician and the megapod, and <see cref="Knows"/> would never be true for them.
    /// </summary>
    public static bool TakesInterestIn(EntityType watcher, EntityType seen)
    {
        return seen?.StructureType != null && seen.ContainerType != null && Enabled && IsRaider(watcher);
    }

    /// <summary>
    /// Whether a predator whose kind knows <paramref name="known"/> (its allegiance's
    /// SharedKnowledge.AllKnownEntities - seen now, or remembered) knows this building.
    /// </summary>
    public static bool Knows(EntityGroup known, Entity building)
    {
        return building != null && known != null && known.Contains(building);
    }

    /// <summary>
    /// The building this predator would raid: the nearest within its aggro range that its kind
    /// knows of (<see cref="Knows"/>), with colonists asleep inside at night, or - if it is hungry -
    /// food in store it eats. Null when there is none, or it raided today. Called from
    /// EvaluateBreakIn.
    /// </summary>
    public static Entity FindTarget(Entity predator)
    {
        if (!Enabled || predator?.Location == null || OnCooldown(predator))
        {
            return null;
        }
        EntityGroup known = predator.Intelligence?.Allegiance?.SharedKnowledge?.AllKnownEntities;
        if (known == null)
        {
            return null;
        }
        bool hungry = predator.BiologicalEntity != null && predator.BiologicalEntity.StomachContents < HungryBelowStomach;
        Entity best = null;
        float bestDistance = predator.GetAggroRange() ?? 0f;
        foreach (Raidable r in RaidableBuildings())
        {
            Entity building = r.Building;
            if (!Knows(known, building))
            {
                // Never seen, or forgotten: it does not know the building is there.
                continue;
            }
            if (building.AccessPoint == null || Entity.FindByID(building.EntityID) == null
                || (building.NonLivingEntity?.Integrity is float integrity && integrity <= 0f))
            {
                // Gone, or already broken open.
                continue;
            }
            if (!r.Sleepers && !(hungry && r.Food.Any(f => Eats(predator, f))))
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
    private static List<Raidable> RaidableBuildings()
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
        var foodIn = new Dictionary<EntityID, HashSet<EntityType>>();
        foreach (Expedition expedition in sim.PlaySite.PlayerAllegiance.Expeditions)
        {
            var food = expedition?.OwnedEntities?.Food;
            if (food == null) continue;
            foreach (var byType in food)
            {
                foreach (EntityID id in byType.Value)
                {
                    Entity item = Entity.FindByID(id);
                    if (item?.ContainedBy == null) continue;
                    if (!foodIn.TryGetValue(item.ContainedBy.Value, out var types))
                    {
                        types = new HashSet<EntityType>();
                        foodIn[item.ContainedBy.Value] = types;
                    }
                    types.Add(byType.Key);
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
                    foodIn.TryGetValue(id, out var types);
                    if (sleepers || types != null)
                    {
                        raidable.Add(new Raidable { Building = building, Sleepers = sleepers, Food = types ?? new HashSet<EntityType>() });
                    }
                }
            }
        }
        return raidable;
    }

    public static float ParseSeconds(string value) =>
        float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float s) && s > 0f ? s : 60f;
}
