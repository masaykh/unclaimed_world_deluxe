using UWGame.SimSide.Resources;

namespace UWGame.Mods;

/// <summary>
/// The regrowth mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// Every tile respawns exactly what the studio's replenish computed.
/// </summary>
public static class RegrowthMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "regrowth";

    public static readonly string[] PlantKeys = new string[0];

    public static void RegisterSettings()
    {
    }

    public static int AdjustReplenish(ResourceContainer tile, int amount) => amount;

    public static float Curve(float fractionLeft) => 1f;

    public static float ForecastRegrowth(ResourceContainer tile, float yearlyRegrowth) => yearlyRegrowth;

    public static string RegrowthLabel(float current, bool maximumReached) => null;

    public static string RegrowthToolTipNote(ResourceType type) => null;

    /// <summary>The studio's test: any bulk at all, buds included.</summary>
    public static bool ShowsGrownLook(bool stock, System.Collections.Generic.IList<IResourceItem> items) => stock;

    public static bool GrownLook(int grown, int total, string threshold) => grown > 0;
}
