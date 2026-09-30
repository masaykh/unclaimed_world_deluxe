using System;

namespace UWGame.Mods;

/// <summary>
/// The procedural map generator, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Nothing in the game calls it; DataExport's --generate-map and --mapgen-selftest do, and say
/// the mod is not in this build.
/// </summary>
public static class MapGenMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "mapgen";

    public static string GenerateMap(string sourceMapsDir, string mapsDir, string name, ulong seed, int size) =>
        throw new InvalidOperationException("the map generator (MapGenMod) is not in this build");

    public static int SelfTest(string sourceMapsDir, string workDir, Action<string> log)
    {
        log("  the mod is not in this build - nothing to check");
        return 0;
    }
}
