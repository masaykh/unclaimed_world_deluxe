namespace UWGame.Mods;

/// <summary>
/// The debug overlays and test-scenario picker, compiled OUT. See UnhiddenMod.Absent.cs for the
/// pattern and HealingMod.Absent.cs for why every member answers what the studio's code answered.
/// </summary>
public static class DebugMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "debug";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>
    /// The caller's own fallback, which is the studio's hard-coded TwinklerEatTest. The TEST
    /// button behaves exactly as it did before this mod existed.
    /// </summary>
    public static string TestScenarioName(string fallback)
    {
        return fallback;
    }

    /// <summary>
    /// The studio's own Options.RecordGame, unchanged. Without the mod there is no control for it
    /// and it stays exactly as reachable - or not - as the studio left it.
    /// </summary>
    public static bool RecordGame(bool studioValue)
    {
        return studioValue;
    }

    /// <summary>
    /// Writes nothing. Every Kensei.Dev overlay stays at its shipped value - which is off, since
    /// nothing in the retail game ever set one.
    /// </summary>
    public static void ApplyOverlays()
    {
    }
}
