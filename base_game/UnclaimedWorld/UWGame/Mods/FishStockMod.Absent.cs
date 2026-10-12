using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;

namespace UWGame.Mods;

/// <summary>
/// The fish stock mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
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

    /// <summary>Every decorative fish is drawn, as the studio drew them.</summary>
    public static bool HidesDecoration(UWGame.SimSide.AI.IKnownEntityData shown) => false;

    public static readonly string[] DecorationKeys = new string[0];

    public static readonly string[] FishResourceKeys = new string[0];

    public static bool IsFishResource(UWGame.SimSide.Resources.ResourceType type) => false;

    public static bool ShowsFish(Microsoft.Xna.Framework.Point at, int range, int width, int height, System.Func<int, int, int> fishAt) => true;
}
