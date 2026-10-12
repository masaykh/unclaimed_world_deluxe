using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// Peat as fuel: charcoal from peat, and a campfire or a kitchen built with peat instead of
/// firewood. From the community Unhidden Mod, split out of it on Kastuk's request (2026-10-11:
/// "Mods of peat, Rawhide string and Cap resource respawn can be separated into own categories").
/// The settings were unhidden.charcoalFromPeat and unhidden.peatBuilding; ModSettings.LegacyIds
/// reads those from an older file or save.
///
/// PROVENANCE. The Unhidden Mod's patches came from a modder working without source - BepInEx and
/// HarmonyLib, twelve [HarmonyPatch] targets and reflection into private members - and were
/// reproduced as ordinary source edits. This is the content part of them.
///
/// WHAT IT CHANGES, with CHARCOAL FROM PEAT on:
///   * item:charcoal is redefined to carry the "fuelForForge" fuel TAG
///   * structure:simpleSmithy and structure:improvisedSmithy require that tag instead of the
///     literal item:charcoal (PeatModSmithies). Only charcoal carries it, so they burn what they did
///   * makeCharcoalFromPeat: 2 dry peat -> 1 REAL charcoal in a kiln
/// and with BUILD WITH PEAT on, a peat-loaded variant of three firewood-loaded builds.
///
/// WHAT IT NO LONGER CHANGES, and why. It used to add item:peatCharcoal, a second forge fuel. A
/// NEW EntityType key is resolved by name in a save, so a save holding one could not be loaded by
/// a build without the mod at all; and it could not be made into gunpowder or sold, because much
/// of the game matches charcoal by key. Producing the real item answers both. What remains adds
/// ProcessType keys and no EntityType keys, and a save is stamped with them (ModSettings.Signature).
/// </summary>
public static class PeatMod
{
    public const string ModId = "peat";

    private static ModSetting charcoalFromPeat;

    private static ModSetting peatBuilding;

    /// <summary>
    /// Whether the peat route is in the tables. It adds the makeCharcoalFromPeat ProcessType key,
    /// which a save can name.
    /// </summary>
    public static ModSetting CharcoalFromPeat =>
        charcoalFromPeat ?? (charcoalFromPeat = ModSettings.Toggle(
            ModId, "charcoalFromPeat", "CHARCOAL FROM PEAT", defaultValue: true,
            toolTip: "Adds the recipe that turns dry peat into charcoal in a kiln, for maps " +
                     "where trees are scarce. Off gives the stock game's firewood route only. " +
                     "Changes what a save contains: a save made with this on names a recipe a " +
                     "build without it does not have.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>
    /// Whether peat can stand in for the firewood a campfire or a primitive kitchen is built with.
    /// Separate from the charcoal recipe because it is a separate thing to want, and each switch
    /// that adds a ProcessType key is listed separately in a save's stamp.
    /// </summary>
    public static ModSetting PeatBuilding =>
        peatBuilding ?? (peatBuilding = ModSettings.Toggle(
            ModId, "peatBuilding", "BUILD WITH PEAT", defaultValue: true,
            toolTip: "Lets a campfire, an improvised kitchen and a mudbrick kitchen be built with " +
                     "dry peat instead of firewood. The fuel is not spent building them - it is " +
                     "the load they start with - so peat works as well as a log does.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switches.</summary>
    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "PEAT");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "Peat as fuel: a kiln turns dry peat into charcoal, and a campfire or a primitive kitchen can be built with peat instead of firewood.",
            "HUD_thumbnail_peatStack");
        _ = CharcoalFromPeat;
        _ = PeatBuilding;
    }

    private static EntityType charcoal;

    private static ProcessType makeCharcoalFromPeat;

    /// <summary>
    /// Called at the end of ItemLoader.Init. Redefines charcoal to advertise a fuel TAG rather
    /// than being matched by item key, and the smithies to burn the tag (PeatModSmithies).
    /// </summary>
    public static void AddItems(List<EntityType> listOfEntityTypes)
    {
        charcoal = null;
        if (!CharcoalFromPeat.On)
        {
            return;
        }
        int index = listOfEntityTypes.FindIndex((EntityType s) => s.KeyName == "item:charcoal");
        if (index < 0)
        {
            return;
        }

        listOfEntityTypes[index] = new EntityType("item:charcoal")
        {
            Name = "Charcoal",
            SummaryDescription = "A traditional fuel type made from heat treated organic matter",
            Description = "Charcoal can provide a more intense heat than firewood and is often required for primitive metalworking.",
            ItemType = new ItemType
            {
                MaximumBulk = 0.3f,
                FuelType = new FuelType
                {
                    FuelTags = new string[1] { "fuelForForge" }
                }
            },
            NonLivingType = new NonLivingType
            {
                Repairability = 0f,
                DegradeType = "stored dry"
            },
            TierOrArea = new TierOrArea
            {
                Tier = "basic"
            },
            CategoryKey = "rawMaterials",
            RenderableType = new RenderableType
            {
                DefaultClientState = new ClientStateInfo
                {
                    RenderAsBillboardType = new RenderAsBillboardType[1]
                    {
                        new RenderAsBillboardType
                        {
                            AssetName = "charcoal"
                        }
                    }
                }
            }
        };
        // Kept for RegisterAttainability: the peat route has to be attached to the charcoal the
        // game actually indexes, which is this replacement instance, not the one it replaced.
        charcoal = listOfEntityTypes[index];
        PeatModSmithies.Replace(listOfEntityTypes);
    }

    /// <summary>Called at the end of ProcessLoader.InitProcessTypes.</summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
        AddPeatConstruction(listOfProcessTypes);

        makeCharcoalFromPeat = null;
        if (!CharcoalFromPeat.On)
        {
            return;
        }

        // Peat as a source of REAL charcoal - see the class comment for why not an item of its own.
        //
        //   makeCharcoalFromPeat  2 dry peat -> 1 charcoal,      1/15 day   (peat route)
        //   makeCharcoal (stock)  1 firewood -> 1 charcoal,      1/30 day   (firewood route)
        //
        // 2:1 because a peat bank does not run out: firewood is capped by how fast trees grow back,
        // peat by nothing, which makes it the route for STABLE production rather than the efficient
        // one. Kastuk, having played it: "peat reserve is endless, unlike long respawn of firewood,
        // so its useful for stable production even with less efficiency". Same kiln tool set and
        // skill as the stock recipe, so it appears beside it with nothing new to build.
        makeCharcoalFromPeat = new ProcessType
        {
            Name = "Producing",
            KeyName = "makeCharcoalFromPeat",
            JobTypeKey = "craftingJobType",
            RequiredSkill = "bushcraft",
            PhysicalWorkFactor = 4f,
            WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
            Stances = ProcessLoader.GetToolMakingStances(),
            Inputs = new Input[1]
            {
                new Input
                {
                    Entity = "item:dryPeat",
                    IsConsumed = true,
                    Amount = new InputAmount
                    {
                        NoOfItems = 2
                    }
                }
            },
            Outputs = new Output[1]
            {
                new Output
                {
                    EntityTypeToCreate = "item:charcoal",
                    Amount = new OutputAmount
                    {
                        NoOfItems = 1
                    },
                    ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
                }
            },
            WorkOrTimeNeeded = new WorkOrTime
            {
                DaysNeeded = 1f / 15f
            },
            ProcessToolSetKey = "toolSetKilnBigAndSmall",
            AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
        };
        listOfProcessTypes.Add(makeCharcoalFromPeat);
    }

    /// <summary>
    /// The stock recipes that hand a structure its first load of fuel, and which name firewood by
    /// key to do it.
    /// </summary>
    private static readonly string[] FirewoodFuelledBuilds = new string[3]
    {
        "constructCampfire", "constructImprovisedKitchen", "constructMudbrickKitchen"
    };

    /// <summary>The peat variants added by <see cref="AddPeatConstruction"/>, for attainability.</summary>
    private static readonly List<ProcessType> peatBuilds = new List<ProcessType>();

    /// <summary>
    /// Adds a peat-fuelled variant of each build in <see cref="FirewoodFuelledBuilds"/>.
    ///
    /// WHY THESE THREE. In all three the firewood is not a material: it is consumed and re-created
    /// as a waste product, which is the game's way of saying "the structure starts with one fuel in
    /// it". Each structure burns the fuelForCampfire tag, and dry peat carries that tag with the
    /// same MaximumBulk as firewood, so the swap is fair. WHY 1 PEAT, when the charcoal recipe
    /// charges two: here the input IS the output.
    /// </summary>
    private static void AddPeatConstruction(List<ProcessType> listOfProcessTypes)
    {
        peatBuilds.Clear();
        if (!PeatBuilding.On)
        {
            return;
        }

        foreach (string key in FirewoodFuelledBuilds)
        {
            ProcessType stock = listOfProcessTypes.Find(
                (ProcessType p) => p != null && p.KeyName == key && !p.DeleteRecord);
            if (stock == null || stock.Inputs == null || stock.Outputs == null)
            {
                // A scenario that deletes the recipe, or a game update that renamed it: a variant
                // would put back by the side door what the scenario's designer removed.
                continue;
            }

            Input[] inputs = RecipeVariants.SwapInput(stock.Inputs, "item:firewood", "item:dryPeat");
            Output[] outputs = RecipeVariants.SwapOutput(stock.Outputs, "item:firewood", "item:dryPeat");
            if (inputs == null || outputs == null)
            {
                // No firewood in it after all - somebody else already changed this recipe.
                continue;
            }

            ProcessType peat = new ProcessType(stock, key + "WithPeat", null)
            {
                Inputs = inputs,
                Outputs = outputs
            };
            listOfProcessTypes.Add(peat);
            peatBuilds.Add(peat);
        }
    }

    /// <summary>Called at the end of InventorySettings.EndRecomputeAttainability.</summary>
    public static void RegisterAttainability(Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
        // Charcoal already has an entry, from the stock firewood recipe - so the peat route goes
        // INTO that entry rather than creating one.
        if (charcoal != null && makeCharcoalFromPeat != null
            && attainableInfo.TryGetValue(charcoal, out Dictionary<ProcessType, AttainableInfo> charcoalRoutes)
            && !charcoalRoutes.ContainsKey(makeCharcoalFromPeat))
        {
            charcoalRoutes.Add(makeCharcoalFromPeat, RecipeVariants.Producable);
        }
        RecipeVariants.RegisterAttainability(peatBuilds, attainableInfo);
    }
}
