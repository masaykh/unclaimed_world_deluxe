using UWGame.Control;

namespace UWGame.Mods;

/// <summary>
/// The file-driven agent channel, compiled OUT. See UnhiddenMod.Absent.cs for the pattern and
/// HealingMod.Absent.cs for why every member answers what the studio's code answered.
/// </summary>
public static class AgentMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool IsEnabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "agent";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>
    /// Does nothing. Nothing is written, nothing is read, and the game is never paused on
    /// anything's behalf - which is the studio's behaviour, since none of this existed.
    /// </summary>
    public static void Tick(Controller controller)
    {
    }

    /// <summary>Nothing to forget.</summary>
    public static void Reset()
    {
    }
}
