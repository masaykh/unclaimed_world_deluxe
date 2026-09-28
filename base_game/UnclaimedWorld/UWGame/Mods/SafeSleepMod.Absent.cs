using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The safe sleep mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// A sleeping spot is scored as the studio scored it: by kind of place, never by what is nearby.
/// </summary>
public static class SafeSleepMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "safesleep";

    public static void RegisterSettings()
    {
    }

    public static bool IsDangerous(EntityType type) => false;

    public static double SafetyFactor(Entity sleeper, Vector3 location) => 1.0;
}
