using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// Firewood from any wood, compiled OUT. See PeatMod.Absent.cs for the pattern: the stock game's
/// spoak-branch route only.
/// </summary>
public static class FirewoodMod
{
    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "firewood";

    /// <summary>No recipes.</summary>
    public static readonly (string key, string wood, int count)[] Recipes = new (string, string, int)[0];

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Adds nothing.</summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
    }
}
