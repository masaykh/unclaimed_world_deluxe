using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// The rawhide string mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
/// </summary>
public static class RawhideStringMod
{
    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "rawhidestring";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Adds no item: the studio's tables have no plant fibre string.</summary>
    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
    }

    /// <summary>Leaves the studio's tool sets as they are.</summary>
    public static void AddToolAlternatives(List<ProcessToolSet> toolSets)
    {
    }

    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
    }

    public static void RegisterAttainability(
        Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
    }
}
