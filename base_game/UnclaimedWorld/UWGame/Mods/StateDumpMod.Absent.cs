namespace UWGame.Mods;

/// <summary>
/// The state dump, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
/// </summary>
public static class StateDumpMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "statedump";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Samples nothing. The studio's game wrote no such file.</summary>
    public static void Sample()
    {
    }

    /// <summary>Nothing to close.</summary>
    public static void Close()
    {
    }
}
