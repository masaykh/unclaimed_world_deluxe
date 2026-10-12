using System.Collections.Generic;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// The hide gear mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// No hide cloak, armour or backpack in the tables, a patrol asks for night gear only, a haul for
/// none, a blow lands as the studio's body layers alone decide, and a load weighs what it weighs.
/// </summary>
public static class HideGearMod
{
    public const string ModId = "hidegear";

    public static void RegisterSettings()
    {
    }

    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
    }

    public static void AddEffects(List<EffectType> list)
    {
    }

    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
    }

    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
    }

    public static void AddPatrolGear(ref List<ItemType.TaskType> gearTasks)
    {
    }

    public static float ArmourFactor(Entity wearer, BodyPartType part, DamageType damage) => 1f;

    public static float ArmourFactor(BodyPart part, AttackType attack) => 1f;

    public static float LoadFelt(Entity carrier) => 1f;

    public static StorageCompartment CompartmentFor(Entity carrier, IKnownEntityData item, bool forFight, StorageCompartment asked) => asked;

    public static void AddHaulGear(ref List<ItemType.TaskType> gearTasks)
    {
    }
}
