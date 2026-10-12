using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// The hide gear mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// No hide cloak or armour in the tables, and a patrol asks for night gear only, as the studio's did.
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
}
