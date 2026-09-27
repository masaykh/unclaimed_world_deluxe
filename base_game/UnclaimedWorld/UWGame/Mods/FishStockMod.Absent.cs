using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;

namespace UWGame.Mods;

/// <summary>
/// The fish stock mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Leaves fishTrapSpawningLoop exactly as the studio wrote it: traps catch forever.
/// </summary>
public static class FishStockMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "fishstock";

    public static void RegisterSettings()
    {
    }

    public static bool AdjustPolledEvents(List<PolledEventType> list) => false;

    public static bool IsFishTrap(EntityType type) => false;

    public static float CatchPerPoll(EntityType trap) => 0f;

    public static float BestCatchPerPoll(EntityType trap) => 0f;

    public static IEnumerable<EntityType> PlacesFor(EntityType trap) => new EntityType[0];

    public static IEnumerable<EntityType> TrapsBuiltAt(EntityType place) => new EntityType[0];

    public static float Regrow(float stock, double since, double now, float max, float perSecond) => stock;
}
