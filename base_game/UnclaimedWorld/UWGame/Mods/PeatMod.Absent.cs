using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// The peat mod, compiled OUT - the build with mods/ deleted, or -p:UwGameplayMods=false. See
/// the SELECTABLE FEATURES block in UnclaimedWorld.csproj.
///
/// THE PATTERN every *.Absent.cs follows. WHY A STUB RATHER THAN #if AT EVERY CALL SITE: a mod
/// is reached from places across the game's own files, and wrapping each in an <c>#if</c> would
/// be more edits to the studio's source - harder to read, and a larger patch series for no
/// benefit. With a stub those call sites are byte-identical whether the mod is in or out.
///
/// Where a stub has an <c>Enabled</c>, it is a compile-time constant false, so every guarded block
/// is dead code the compiler drops. The methods exist only to satisfy the calls; a stub returns
/// the studio's own answer, so a build without the mod is the stock game. The real content - here
/// the recipes and the two smithies - is not compiled in at all, which is the honest meaning of
/// "I did not install that".
///
/// Exactly one of this file and mods/PeatMod.cs is ever compiled; the csproj removes the other.
/// </summary>
public static class PeatMod
{
    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "peat";

    /// <summary>Registers nothing: an installation without the mod does not carry its settings.</summary>
    public static void RegisterSettings()
    {
    }

    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
    }

    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
    }

    public static void RegisterAttainability(
        Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
    }
}
