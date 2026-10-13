namespace UWGame.Mods;

/// <summary>
/// The resource respawn cap, compiled OUT. See PeatMod.Absent.cs for the pattern. Without it,
/// ResourceReplenish keeps the studio's behaviour: the clamp it computes is not used.
/// </summary>
public static class RespawnCapMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "respawncap";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Records nothing: the studio's maximum.</summary>
    public static void BeforeGather(SimSide.Resources.ResourceReplenish replenish, int held)
    {
    }
}
