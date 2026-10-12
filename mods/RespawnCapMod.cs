namespace UWGame.Mods;

/// <summary>
/// A replenishing resource tile is topped up to its high-water mark instead of growing past it.
/// From the community Unhidden Mod, split out of it on Kastuk's request (2026-10-11). The setting
/// was unhidden.capResourceRespawn; ModSettings.LegacyIds reads that from an older file.
///
/// The studio's ResourceReplenish.ReplenishResourceItems computes exactly this clamp and then
/// throws the result away - Common.Clamp is a pure function and its return value is not assigned.
/// So a tile adds FractionOfMaximumToReplenishEachTime x its own maximum on every replenish point,
/// on top of whatever is there, and the maximum is a ratchet that then records the larger count.
/// For a resource with a fraction of 1.0 - torux shellfish, and it is not alone - that is a
/// near-doubling every time the tile has been harvested at all. Kastuk: shellfish "spawning
/// endlessly, until coast become covered in hundreds of that easy food".
///
/// A mod rather than a core fix because it changes the food economy of a running game. It takes
/// effect immediately, and no save can tell whether it was on.
/// </summary>
public static class RespawnCapMod
{
    public const string ModId = "respawncap";

    private static ModSetting capResourceRespawn;

    /// <summary>Whether ResourceReplenish uses the clamp the studio computed.</summary>
    public static ModSetting CapResourceRespawn =>
        capResourceRespawn ?? (capResourceRespawn = ModSettings.Toggle(
            ModId, "capResourceRespawn", "CAP RESOURCE RESPAWN", defaultValue: true,
            toolTip: "Tops a resource tile back up to the most it has ever held, instead of adding " +
                     "that much again on top. Without it, shellfish and other fully-replenishing " +
                     "resources multiply every time they are harvested."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "RESOURCE RESPAWN CAP");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "Shellfish and other resources that grow back are topped up to the most a tile has held, instead of multiplying every time they are harvested.",
            "HUD_thumbnail_fishWeir");
        _ = CapResourceRespawn;
    }

    /// <summary>Read by ResourceReplenish.ReplenishResourceItems.</summary>
    public static bool Enabled => CapResourceRespawn.On;
}
