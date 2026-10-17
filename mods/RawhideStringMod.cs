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
/// VINES, NOT RAWHIDE. Kastuk, "Fishing net recipe" (2026-10-17): "rawhide string is ineffective
/// in water ... Switch rawhide string to vine in fishing net process, but raise needful amount of
/// material to 2 and double the needful time to complete." So the variant is now
/// makeFishingNetFromVine: 2 item:vine where the stock recipe takes 1 cotton string, and twice its
/// time. Rawhide string keeps its studio uses as a tool ("lightString", "cordage" tool tags). The
/// class, the mod id and the setting id keep their old names, so settings files and save stamps
/// still match; the old recipe key is gone.
///
/// NOT a tag. The right fix is one recipe matching a "string" TAG, and Input.Tag exists in the
/// data - but nothing reads it: inputs are matched by key, only fuel by tag. Until tag inputs are
/// implemented, a variant per string is what works, and there is one other string to vary by.
/// </summary>
public static class RawhideStringMod
{
    public const string ModId = "rawhidestring";

    private static ModSetting stringAlternatives;

    /// <summary>Whether a fishing net can be made from vines as well as cotton string.</summary>
    public static ModSetting StringAlternatives =>
        stringAlternatives ?? (stringAlternatives = ModSettings.Toggle(
            ModId, "stringAlternatives", "NETS FROM VINES", defaultValue: true,
            toolTip: "Adds a fishing net recipe that uses 2 vines and takes twice as long, as well as " +
                     "the stock one from cotton string.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "VINE NETS");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "A fishing net can be made from vines - two of them, and twice as slowly - as well as from cotton string.",
            "HUD_thumbnail_fishingNet");
        _ = StringAlternatives;
    }

    public const string VineNetKey = "makeFishingNetFromVine";

    private const string Vine = "item:vine";

    /// <summary>Vines per net, where the stock recipe takes 1 cotton string.</summary>
    public const int VinesPerNet = 2;

    /// <summary>The vine net takes this many times the stock recipe's time.</summary>
    public const float TimeFactor = 2f;

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

        Input[] inputs = RecipeVariants.SwapInput(stock.Inputs, "item:cottonString", Vine);
        if (inputs == null)
        {
            return;
        }
        foreach (Input input in inputs)
        {
            if (input.Entity == Vine)
            {
                input.Amount.NoOfItems = VinesPerNet;
            }
        }

        WorkOrTime stockTime = stock.WorkOrTimeNeeded;
        ProcessType vine = new ProcessType(stock, VineNetKey, null)
        {
            Inputs = inputs,
            WorkOrTimeNeeded = stockTime == null ? null : new WorkOrTime
            {
                DaysNeeded = stockTime.DaysNeeded * TimeFactor,
                TimeInSecondsNeeded = stockTime.TimeInSecondsNeeded * TimeFactor,
                MultiplyByBulk = stockTime.MultiplyByBulk
            },
            // Deep-copied although nothing in them changes: the loader initialises Output objects
            // in place, and one reachable from two ProcessTypes would be initialised twice.
            Outputs = RecipeVariants.SwapOutput(stock.Outputs, null, null, requireMatch: false)
        };
        listOfProcessTypes.Add(vine);
        variants.Add(vine);
    }

    /// <summary>Called at the end of InventorySettings.EndRecomputeAttainability.</summary>
    public static void RegisterAttainability(Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
        RecipeVariants.RegisterAttainability(variants, attainableInfo);
    }
}
