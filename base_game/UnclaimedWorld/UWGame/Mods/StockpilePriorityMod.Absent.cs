using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.Mods;

/// <summary>
/// The stockpile priority mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// Every stockpile fills and is hauled to as the studio had it; a priority a save carries is kept
/// and means nothing, and an item of a type a process is waiting for stays where it is stored.
/// </summary>
public static class StockpilePriorityMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "stockpilepriority";

    public static void RegisterSettings()
    {
    }

    public static double RankStorage(double score, Stockpile stockpile, bool isTradeOffer) => score;

    public static void PrioritiseHaul(HaulingJobSpecificItem haul, Stockpile stockpile, EntityGroup owner, IKnownEntityData item)
    {
    }

    /// <summary>The studio's rule: a type a process may want is not hauled to storage.</summary>
    public static bool MustLeave(EntityGroup owner, IKnownEntityData item, HaulingJob haul = null) => false;

    public static void OnStockpileChanged(EntityGroup owner, Stockpile stockpile)
    {
    }

    public static string Label(Priority priority) => "";

    public static Priority Next(Priority priority) => priority;

    public static string ButtonToolTip => "";
}
