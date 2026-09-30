using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// The pest mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Every population fills to the studio's MaxMembers at the studio's growth rate.
/// </summary>
public static class PestMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "pests";

    public const string RatKey = null;

    public const string QuaditeKey = null;

    public const int MaxExtra = 0;

    public static void RegisterSettings()
    {
    }

    public static int Classify(string profileKey) => 0;

    public static float Threshold(string sensitivityValue) => 0f;

    public static int ExtraFor(float exposedBulk, float threshold) => 0;

    public static int ExtraMembers(Expedition population) => 0;

    public static float GrowthFactor(int extra) => 1f;
}
