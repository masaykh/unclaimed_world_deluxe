using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The prey fear mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Prey keeps the studio's threat radius, eating animals do not check for danger, and a melee
/// strike needs the studio's two-sided distance band.
/// </summary>
public static class PreyFearMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "preyfear";

    public static void RegisterSettings()
    {
    }

    public static float BoldnessFactor(EntityType owner, float studioFactor) => studioFactor;

    public static bool ChecksSafetyWhileEating(Entity eater) => false;

    public const double WatchEverySeconds = 0.25;

    public static bool IsWatchful(Entity idler) => false;

    public const bool CloseCountsAsInReach = false;
}
