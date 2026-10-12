using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// A fishing net from rawhide string, alongside the stock cotton-string one. From the community
/// Unhidden Mod, split out of it on Kastuk's request (2026-10-11). The setting was
/// unhidden.stringAlternatives; ModSettings.LegacyIds reads that from an older file or save.
///
/// WHY. makeFishingNet names item:cottonString by key, and it is the only recipe in the game that
/// consumes a string of any kind. So item:rawhideString - which has its own recipe,
/// makeRawhideString - is consumed by NOTHING. Kastuk: "illogical to not use all other
/// alternative strings". A gap rather than a balance decision.
///
/// NOT a tag. The right fix is one recipe matching a "string" TAG, and Input.Tag exists in the
/// data - but nothing reads it: inputs are matched by key, only fuel by tag. Until tag inputs are
/// implemented, a variant per string is what works, and there is one other string to vary by.
/// </summary>
public static class RawhideStringMod
{
    public const string ModId = "rawhidestring";

    private static ModSetting stringAlternatives;

    /// <summary>Whether a fishing net can be made from rawhide string as well as cotton string.</summary>
    public static ModSetting StringAlternatives =>
        stringAlternatives ?? (stringAlternatives = ModSettings.Toggle(
            ModId, "stringAlternatives", "NETS FROM ANY STRING", defaultValue: true,
            toolTip: "Adds a fishing net recipe that uses rawhide string. Stock only accepts " +
                     "cotton string, which leaves rawhide string with no use at all.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "RAWHIDE STRING");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "A fishing net can be made from rawhide string as well as cotton string, so rawhide string has a use.",
            "HUD_thumbnail_fishingNet");
        _ = StringAlternatives;
    }

    /// <summary>The recipe added by <see cref="AddProcesses"/>, for attainability.</summary>
    private static readonly List<ProcessType> variants = new List<ProcessType>();

    /// <summary>Called at the end of ProcessLoader.InitProcessTypes.</summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
        variants.Clear();
        if (!StringAlternatives.On)
        {
            return;
        }

        ProcessType stock = listOfProcessTypes.Find(
            (ProcessType p) => p != null && p.KeyName == "makeFishingNet" && !p.DeleteRecord);
        if (stock == null || stock.Inputs == null || stock.Outputs == null)
        {
            return;
        }

        Input[] inputs = RecipeVariants.SwapInput(stock.Inputs, "item:cottonString", "item:rawhideString");
        if (inputs == null)
        {
            return;
        }

        ProcessType rawhide = new ProcessType(stock, "makeFishingNetFromRawhide", null)
        {
            Inputs = inputs,
            // Deep-copied although nothing in them changes: the loader initialises Output objects
            // in place, and one reachable from two ProcessTypes would be initialised twice.
            Outputs = RecipeVariants.SwapOutput(stock.Outputs, null, null, requireMatch: false)
        };
        listOfProcessTypes.Add(rawhide);
        variants.Add(rawhide);
    }

    /// <summary>Called at the end of InventorySettings.EndRecomputeAttainability.</summary>
    public static void RegisterAttainability(Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
        RecipeVariants.RegisterAttainability(variants, attainableInfo);
    }
}
