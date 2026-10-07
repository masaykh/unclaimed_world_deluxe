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

    public static int StockAfterDemand(EntityGroup owner, EntityType type, int stock) => stock;

    public static bool HasDemand(EntityGroup owner, EntityType type) => false;

    /// <summary>The studio's rule: a padlocked slider at 0 is no order.</summary>
    public static bool ZoneGathersAt(int sliderValue) => sliderValue > 0;

    /// <summary>The studio's BUILD: only with every material in stock.</summary>
    public static bool PlacesBeforeMaterials(System.Collections.Generic.Dictionary<UWGame.SimSide.Processes.ProcessType, UWGame.ClientSide.Interface.Inventory.AttainableInfo> attainable, UWGame.SimSide.Processes.ProcessType process) => false;

    /// <summary>The studio's priority list: LOW, NORMAL, HIGH.</summary>
    public static bool OffersStop(UWGame.SimSide.Jobs.Job job) => false;
}
