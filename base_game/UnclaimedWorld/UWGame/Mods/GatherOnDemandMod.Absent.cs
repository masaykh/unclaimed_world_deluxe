using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The gather-on-demand mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// A standing order keeps its stock and nothing more, as the studio made it.
/// </summary>
public static class GatherOnDemandMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "gatherondemand";

    public static void RegisterSettings()
    {
    }

    public static int Demand(EntityGroup owner, EntityType type) => 0;

    public static int StockAfterDemand(EntityGroup owner, EntityType type, int stock) => stock;

    public static bool HasDemand(EntityGroup owner, EntityType type) => false;
}
