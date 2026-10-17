using System;
using System.Collections.Generic;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// The emergency eating mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// A colonist eats only what its food tags name, as the studio has it: no raw-food processes, no
/// "Raw food" or "Bad stomach" effects, and nutrition, speed, work time and energy as the studio computes them.
/// </summary>
public static class EmergencyEatingMod
{
    public const string ModId = "emergencyeating";

    public static bool Enabled => false;

    public static void RegisterSettings()
    {
    }

    public static void AddEffects(List<EffectType> list)
    {
    }

    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
    }

    public static void AddConsumeProcesses(EntityType parent, BiologicalType bio)
    {
    }

    public static ProcessType ConsumeProcessFor(Entity eater, EntityType food) => null;

    public static bool IsRawFood(Entity eater, EntityType food) => false;

    public static void AddRawFood(Entity eater, BiologicalEntity bio, SharedKnowledge knowledge, EntityGroup group, List<EntityID> allFoodItems)
    {
    }

    public static void OrderRawFood(Entity eater, List<Tuple<IKnownEntityData, ProcessType, double>> scores, double minimumRatingToConsider)
    {
    }

    public static void OnEaten(Entity eater, EntityType food)
    {
    }

    public static float NutritionFactor(Entity eater) => 1f;

    public static float RawNutrientFactor(Entity eater, EntityType food, FoodNutrientType nutrient) => 1f;

    public static float SpeedFactor(Entity entity) => 1f;

    public static float WorkTimeFactor(Entity worker) => 1f;

    public static float CapEnergy(Entity entity, float energy) => energy;
}
