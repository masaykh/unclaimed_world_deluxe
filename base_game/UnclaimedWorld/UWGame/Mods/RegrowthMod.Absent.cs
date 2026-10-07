using UWGame.SimSide.Resources;

namespace UWGame.Mods;

/// <summary>
/// The regrowth mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
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

    public static float Curve(float fractionLeft, float floor = 0.5f) => 1f;

    public static float Floor(ResourceType type) => 1f;

    public static float ForecastRegrowth(ResourceContainer tile, float yearlyRegrowth) => yearlyRegrowth;

    public static string RegrowthLabel(float current, bool maximumReached) => null;

    public static string RegrowthToolTipNote(ResourceType type) => null;
}
