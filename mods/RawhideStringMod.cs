using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using Tool = UWGame.SimSide.Processes.Tool;

namespace UWGame.Mods;

/// <summary>
/// A fishing net from vines, alongside the stock cotton-string one, and a string twisted from
/// plant fibre. From the community Unhidden Mod, split out of it on Kastuk's request (2026-10-11).
/// The setting was unhidden.stringAlternatives; ModSettings.LegacyIds reads that from an older
/// file or save.
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
///
/// PLANT FIBRE STRING (its own switch). Kastuk, the same thread: "not vines branch, but bark been
/// used for plant fiber strings. Let's try to add that strings alternative as new item. Can be
/// created from vines and also from elastic shadeleaf canes. Strings are used as lowest tier tool
/// under rawhide strings in processes." So:
///   * item:plantFibreString, made like the studio's item:rawhideString (a hand tool tagged
///     "lightString" and "cordage", survival tier, drawn with the "wire" sprite every string
///     uses) but with durability 0.6 where rawhide string has 1 and a vine 0.2.
///   * two recipes CLONED from makeRawhideString, so the skill (bushcraft), the knife tool set
///     (toolSetButcherFlesh: the bark is stripped with a knife), the work and the animations stay
///     the studio's: <see cref="VinesPerString"/> vines, or <see cref="CanesPerString"/>
///     shadeleaf cane, for one string, in <see cref="FibreTimeFactor"/> times the rawhide time
///     (the fibre is stripped, then twisted). Two vines because a vine is a thin stem (bulk 0.07)
///     and the vine net already counts two of them for one string; one cane because a cane is a
///     whole sapling (bulk 0.2) with bark enough for a string.
///   * as a TOOL: every list of tool alternatives that names item:rawhideString takes plant fibre
///     string beside it at <see cref="FibreProductivityFactor"/> of rawhide's productivity
///     (<see cref="AddToolAlternatives"/>). Every stock list names strings by KEY; none uses the
///     "lightString" or "cordage" tag, which is why the new item's tags alone would not do it.
///
/// The vine net is left as Kastuk asked for it: raw vines, not this string. Making the net from
/// plant fibre string instead would be one more variant here, if he wants it.
///
/// SAVES. A save holding a plant fibre string names item:plantFibreString, and an unknown item key
/// stops a save loading (KeyNotFoundException). So the ITEM is registered whatever the switch:
/// with the switch off nothing makes it and no tool set takes it, and a save that holds some still
/// loads. Only the recipes and the tool alternatives follow the switch - ProcessType keys, which a
/// save names only for a job in progress, and stamped like the vine net (ModSettings.Signature:
/// rawhidestring.plantFibreString=true). That is why the switch can be on by default, where
/// HideGearMod's, whose items follow its switch, is off.
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

    private static ModSetting plantFibreString;

    /// <summary>Whether plant fibre string can be made, and serves as the lowest string where rawhide string does.</summary>
    public static ModSetting PlantFibreString =>
        plantFibreString ?? (plantFibreString = ModSettings.Toggle(
            ModId, "plantFibreString", "PLANT FIBRE STRING", defaultValue: true,
            toolTip: "Adds string twisted from bark fibres: 2 vines or 1 shadeleaf cane make one, " +
                     "with a knife. Wherever rawhide string can be used as a tool, this string can " +
                     "too, 20% less productively.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switches.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "VINE NETS");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "A fishing net can be made from vines - two of them, and twice as slowly - as well as from cotton string. " +
            "String can be twisted from the bark fibres of vines and shadeleaf canes.",
            "HUD_thumbnail_fishingNet");
        _ = StringAlternatives;
        _ = PlantFibreString;
    }

    public const string VineNetKey = "makeFishingNetFromVine";

    private const string Vine = "item:vine";

    /// <summary>Vines per net, where the stock recipe takes 1 cotton string.</summary>
    public const int VinesPerNet = 2;

    /// <summary>The vine net takes this many times the stock recipe's time.</summary>
    public const float TimeFactor = 2f;

    public const string FibreStringKey = "item:plantFibreString";

    public const string RawhideStringKey = "item:rawhideString";

    public const string FibreFromVineKey = "makePlantFibreStringFromVine";

    public const string FibreFromCanesKey = "makePlantFibreStringFromShadeleafCanes";

    private const string ShadeleafCanes = "item:shadeleafCanes";

    /// <summary>Vines per plant fibre string.</summary>
    public const int VinesPerString = 2;

    /// <summary>Shadeleaf canes per plant fibre string.</summary>
    public const int CanesPerString = 1;

    /// <summary>A plant fibre string takes this many times makeRawhideString's time.</summary>
    public const float FibreTimeFactor = 1.5f;

    /// <summary>Rawhide string's is 1, a vine's 0.2.</summary>
    public const float FibreDurability = 0.6f;

    /// <summary>Plant fibre string's productivity as a tool, as a share of rawhide string's in the same list.</summary>
    public const float FibreProductivityFactor = 0.8f;

    /// <summary>The recipes added by <see cref="AddProcesses"/>, for attainability.</summary>
    private static readonly List<ProcessType> variants = new List<ProcessType>();

    /// <summary>
    /// Called at the end of BaseDataLoader.InitEntityTypes, whatever the switch: see the class
    /// comment's SAVES. Modelled on the studio's item:rawhideString.
    /// </summary>
    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
        listOfEntityTypes.Add(new EntityType(FibreStringKey)
        {
            Name = "String (plant fibre)",
            SummaryDescription = "Crude string twisted from bark fibres",
            Description = "Fibres stripped from vines or shadeleaf canes and twisted into a cord. " +
                          "Weaker than rawhide, but adequate for the simplest tasks.",
            TierOrArea = new TierOrArea
            {
                Tier = "survival"
            },
            ToolType = new ToolType
            {
                Durability = FibreDurability,
                ToolHandling = ToolHandlingType.HandTool,
                ToolTag = new string[2] { "lightString", "cordage" }
            },
            ItemType = new ItemType
            {
                MaximumBulk = 0.05f
            },
            NonLivingType = new NonLivingType
            {
                Repairability = 0f,
                DegradeType = "improvisedEquipment"
            },
            CategoryKey = "tools",
            RenderableType = new RenderableType
            {
                DefaultClientState = new ClientStateInfo
                {
                    RenderAsBillboardType = new RenderAsBillboardType[1]
                    {
                        new RenderAsBillboardType
                        {
                            AssetName = "wire"
                        }
                    }
                }
            }
        });
    }

    /// <summary>
    /// Called at the end of BaseDataLoader.InitProcessToolSets. Puts plant fibre string beside
    /// rawhide string in every list of alternatives naming it, at <see cref="FibreProductivityFactor"/>
    /// of its productivity and with its wear. A list that already takes the new item by key or by
    /// one of its tags is left alone: naming it twice is a validation error.
    /// </summary>
    public static void AddToolAlternatives(List<ProcessToolSet> toolSets)
    {
        if (!PlantFibreString.On)
        {
            return;
        }
        foreach (ProcessToolSet set in toolSets)
        {
            if (set?.Tools == null)
            {
                continue;
            }
            foreach (ToolAlternatives alternatives in set.Tools)
            {
                if (alternatives?.Tools == null)
                {
                    continue;
                }
                Tool rawhide = null;
                bool taken = false;
                foreach (Tool tool in alternatives.Tools)
                {
                    if (tool.UsesToolKeyName == RawhideStringKey && tool.ProductivityFactor.HasValue)
                    {
                        rawhide = tool;
                    }
                    taken |= tool.UsesToolKeyName == FibreStringKey
                             || tool.UsesToolTag == "lightString" || tool.UsesToolTag == "cordage";
                }
                if (rawhide == null || taken)
                {
                    continue;
                }
                Tool[] tools = new Tool[alternatives.Tools.Length + 1];
                alternatives.Tools.CopyTo(tools, 0);
                tools[tools.Length - 1] = new Tool
                {
                    UsesToolKeyName = FibreStringKey,
                    ProductivityFactor = rawhide.ProductivityFactor * FibreProductivityFactor,
                    DegradePerSecondOfUse = rawhide.DegradePerSecondOfUse
                };
                alternatives.Tools = tools;
            }
        }
    }

    /// <summary>Called at the end of ProcessLoader.InitProcessTypes.</summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
        variants.Clear();
        if (StringAlternatives.On)
        {
            AddVineNet(listOfProcessTypes);
        }
        if (PlantFibreString.On)
        {
            AddFibreString(listOfProcessTypes, FibreFromVineKey, Vine, VinesPerString);
            AddFibreString(listOfProcessTypes, FibreFromCanesKey, ShadeleafCanes, CanesPerString);
        }
    }

    private static void AddVineNet(List<ProcessType> listOfProcessTypes)
    {
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

        ProcessType vine = new ProcessType(stock, VineNetKey, null)
        {
            Inputs = inputs,
            WorkOrTimeNeeded = Scaled(stock.WorkOrTimeNeeded, TimeFactor),
            // Deep-copied although nothing in them changes: the loader initialises Output objects
            // in place, and one reachable from two ProcessTypes would be initialised twice.
            Outputs = RecipeVariants.SwapOutput(stock.Outputs, null, null, requireMatch: false)
        };
        listOfProcessTypes.Add(vine);
        variants.Add(vine);
    }

    /// <summary>makeRawhideString with its rawhide swapped for <paramref name="count"/> of <paramref name="material"/>.</summary>
    private static void AddFibreString(List<ProcessType> listOfProcessTypes, string key, string material, int count)
    {
        ProcessType stock = listOfProcessTypes.Find(
            (ProcessType p) => p != null && p.KeyName == "makeRawhideString" && !p.DeleteRecord);
        if (stock == null || stock.Inputs == null || stock.Outputs == null)
        {
            return;
        }
        Input[] inputs = RecipeVariants.SwapInput(stock.Inputs, "item:thunderChickenRawhide", material);
        Output[] outputs = RecipeVariants.SwapOutput(stock.Outputs, RawhideStringKey, FibreStringKey);
        if (inputs == null || outputs == null)
        {
            return;
        }
        foreach (Input input in inputs)
        {
            if (input.Entity == material)
            {
                input.Amount.NoOfItems = count;
            }
        }
        ProcessType fibre = new ProcessType(stock, key, null)
        {
            Inputs = inputs,
            Outputs = outputs,
            WorkOrTimeNeeded = Scaled(stock.WorkOrTimeNeeded, FibreTimeFactor)
        };
        listOfProcessTypes.Add(fibre);
        variants.Add(fibre);
    }

    /// <summary>A copy of a recipe's time, multiplied.</summary>
    private static WorkOrTime Scaled(WorkOrTime stockTime, float factor)
    {
        return stockTime == null ? null : new WorkOrTime
        {
            DaysNeeded = stockTime.DaysNeeded * factor,
            TimeInSecondsNeeded = stockTime.TimeInSecondsNeeded * factor,
            MultiplyByBulk = stockTime.MultiplyByBulk
        };
    }

    /// <summary>Called at the end of InventorySettings.EndRecomputeAttainability.</summary>
    public static void RegisterAttainability(Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
        RecipeVariants.RegisterAttainability(variants, attainableInfo);
    }
}
