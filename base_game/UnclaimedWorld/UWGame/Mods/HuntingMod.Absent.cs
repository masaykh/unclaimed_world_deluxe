using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The hunting mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Hunters chase prey as far as the studio set, and the human prey list is the studio's.
/// </summary>
public static class HuntingMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "hunting";

    public static void RegisterSettings()
    {
    }

    public static float ChaseRangeFactor() => 1f;

    public static void AdjustCreatures(List<EntityType> types)
    {
    }
}
