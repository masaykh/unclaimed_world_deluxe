using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The discomfort mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Comfort is the studio's sum of comfort effects; nothing near a sleeping place changes it.
/// </summary>
public static class DiscomfortMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "discomfort";

    public static void RegisterSettings()
    {
    }

    public static void OnSleepStart(Entity sleeper, Entity sleepingPlace)
    {
    }

    public static float Penalty(Entity colonist) => 0f;
}
