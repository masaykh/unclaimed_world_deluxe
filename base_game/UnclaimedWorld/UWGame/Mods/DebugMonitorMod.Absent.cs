namespace UWGame.Mods;

/// <summary>
/// The debug monitor, compiled OUT. See PeatMod.Absent.cs for the pattern.
/// </summary>
public static class DebugMonitorMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "debugmonitor";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Listens on nothing. The studio's retail game had no developer window at all.</summary>
    public static void Tick()
    {
    }
}
