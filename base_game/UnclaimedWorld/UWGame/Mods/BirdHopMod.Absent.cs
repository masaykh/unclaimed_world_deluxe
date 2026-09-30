using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The bird hop mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Immobile creatures stay put, as the studio set them.
/// </summary>
public static class BirdHopMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "birdhop";

    public static readonly string[] HopperKeys = new string[0];

    public const float HopChance = 0f;

    public const float HopMinDistance = 48f;

    public const float HopMaxDistance = 110f;

    public static void RegisterSettings()
    {
    }

    public static bool MayHop(Entity entity) => false;

    public const float StartledMinDistance = 96f;

    public static Entity Startler(Entity bird) => null;

    public static bool IsStartled(Entity bird, ref double secondsSinceWatch, double elapsedSeconds) => false;
}
