using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The home raid mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// No animal ever targets a building: sleepers and stores are as safe as the studio made them.
/// GoalThink gives no species EvaluateBreakIn, and a GoalBreakIn loaded from a modded save fails on
/// its first update because Enabled is false.
/// </summary>
public static class HomeRaidMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "homeraid";

    public static readonly string[] RaiderKeys = new string[0];

    public const float ReachDistance = 72f;

    public const double GiveUpAfterSeconds = 60.0;

    public static void RegisterSettings()
    {
    }

    public static bool IsRaider(EntityType type) => false;

    public static Entity FindTarget(Entity predator) => null;

    public static float DamagePerSecond() => 0f;

    public static void RaidStarted(Entity predator)
    {
    }

    public static float ParseSeconds(string value) => 60f;
}
