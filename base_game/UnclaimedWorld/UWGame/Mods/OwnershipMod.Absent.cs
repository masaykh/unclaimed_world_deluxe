using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.Mods;

/// <summary>
/// The ownership mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Everything belongs to the expedition, as the studio shipped it: households are paid nothing, cook
/// nothing, and a person eats from the expedition's stock alone.
/// </summary>
public static class OwnershipMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "ownership";

    public static void RegisterSettings()
    {
    }

    public static void OnSimTick(Sim sim)
    {
    }

    public static void AddHouseholdFood(Entity eater, BiologicalEntity bio, SimSide.AI.SharedKnowledge knowledge, List<EntityID> allFoodItems)
    {
    }

    public static EntityGroup CommunalFallback(EntityGroup ownerOfJobs) => null;
}
