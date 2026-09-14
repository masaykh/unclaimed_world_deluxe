using System.Collections.Generic;
using HarmonyLib;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Processes;

namespace FasterCharcoalMod;

/// <summary>
/// An example mod: makes charcoal burn twice as fast to produce.
///
/// This is a complete, working mod in about ten lines of substance. It is also the mod loader's
/// regression test - build/80-verify-modloader.sh builds it, drops it in user/Mods, runs the
/// data export, and asserts that makeCharcoal's DaysNeeded actually halved. So it proves three
/// separate things at once: that HarmonyLib patches work on .NET 8, that UWGame.Mods.ModLoader
/// discovers and applies a mod, and that the reference layout in the .csproj beside this file is
/// the one a modder should copy.
///
/// WHAT TO NOTICE
///
///   * No BepInEx, no plugin base class, no attributes on an entry point, no manifest. A mod is
///     a DLL with Harmony patch classes in it, dropped in user/Mods. The loader calls
///     PatchAll on the assembly, and Harmony finds these by attribute.
///
///   * Patching a data loader works because the loader runs BEFORE any game data is read. Data
///     tables are built once per scenario load, so a postfix here shapes what the game runs
///     with. Patch UI or simulation methods the same way.
///
///   * The target is a private-by-convention static method on ProcessLoader, named as a string.
///     Harmony does not care about accessibility - that is its entire point - so the whole
///     surface of the game is reachable, not just its public API.
///
///   * This mod does NOT ship 0Harmony.dll, and the .csproj marks every reference Private=false
///     for that reason. The game ships exactly one Harmony; a second copy in the process keeps
///     its own patch state and breaks in ways that are very hard to diagnose.
/// </summary>
[HarmonyPatch(typeof(ProcessLoader), "InitProcessTypes")]
internal static class FasterCharcoalPatch
{
    /// <summary>
    /// The multiplier applied to charcoal's production time. Halving it is arbitrary - it is
    /// chosen to be unmistakable in the exported processTypes.xml, so the verification script
    /// can assert on it rather than on something that might also occur naturally.
    /// </summary>
    private const float SpeedUp = 0.5f;

    /// <summary>
    /// Runs after ProcessLoader.InitProcessTypes has built the list. `__result` is Harmony's name
    /// for the patched method's return value; taking it by ref lets a postfix modify what the
    /// caller receives, though here the list is mutated in place.
    /// </summary>
    private static void Postfix(ref List<ProcessType> __result)
    {
        if (__result == null)
        {
            return;
        }

        foreach (ProcessType process in __result)
        {
            if (process?.KeyName != "makeCharcoal" || process.WorkOrTimeNeeded == null)
            {
                continue;
            }
            process.WorkOrTimeNeeded.DaysNeeded *= SpeedUp;
        }
    }
}
