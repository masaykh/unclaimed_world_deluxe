using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// The disassembly generator, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// <see cref="Generated"/> and <see cref="Refused"/> are here for tools/DataExport, which reports
/// on them with <c>--disassembly</c>. It builds against whatever UnclaimedWorld.dll it is pointed
/// at, and a build without the mod should give it an empty report rather than a compile error.
/// </summary>
public static class DisassemblyMod
{
    private static readonly List<ProcessType> generated = new List<ProcessType>();

    private static readonly List<string> refused = new List<string>();

    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "disassembly";

    /// <summary>Always empty.</summary>
    public static IReadOnlyList<ProcessType> Generated => generated;

    /// <summary>Always empty.</summary>
    public static IReadOnlyList<string> Refused => refused;

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Remembers nothing: there is no generator to feed.</summary>
    public static void CaptureItems(List<EntityType> listOfEntityTypes)
    {
    }

    /// <summary>
    /// Adds nothing, so the process table is the studio's - their salvage recipes and no others.
    ///
    /// Note what does NOT come out with the mod: the fix in SimProcess.CreateOutputsFromInputs,
    /// which is a fix to the game's own salvage path and stays in core either way. It is a no-op
    /// on the studio's tables, because their validator makes the case it handles unreachable.
    /// </summary>
    public static void AddDisassembly(List<ProcessType> listOfProcessTypes)
    {
    }
}
