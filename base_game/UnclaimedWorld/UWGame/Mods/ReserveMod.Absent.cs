using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// The reserve mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// No item is ever reserved: colonists eat and workshops use everything, as the studio made them.
/// Reserves a modded build left in a save's expedition custom fields are never read.
/// </summary>
public static class ReserveMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "reserve";

    public static void RegisterSettings()
    {
    }

    public static int Reserved(Expedition expedition, EntityType type) => 0;

    public static int Reserved(EntityGroup owner, EntityType type) => 0;

    public static void SetReserve(Expedition expedition, EntityType type, int amount)
    {
    }

    public static bool HoldsBackFood(EntityGroup foodOwner, EntityType type, bool starving) => false;

    public static int StockAfterReserve(EntityGroup owner, EntityType type, int stock) => stock;

    public static int SliderMax(int stock, int reserve) => 20;
}
