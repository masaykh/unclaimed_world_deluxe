using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// Wet firewood from any wood, not only spoak branches.
///
/// THE REQUEST. Kastuk, "Firewood out of any wood" (2026-10-13): "Let's add alternative recipes of
/// wet firewood (how much is need): sticks (4), water cane stem (2), shadeleafBowStave (2),
/// shadeleafCanes (4)." Each makes 1 wet firewood, which dries into firewood on the studio's
/// firewood stack (makeFirewood, 10 at a time) like the studio's own chopped spoak.
///
/// HOW. The studio's chopFirewood is the template: 1 spoak branch -> 2 wet firewood, crafting,
/// menial, 1/300 day. These are lighter wood, so the weak-wood tool set (knives, hand axes,
/// machetes) rather than the tough-wood one, and nothing is left over. Adds ProcessType keys only,
/// no EntityType keys; a save made with them is stamped with the switch (ModSettings.Signature).
/// </summary>
public static class FirewoodMod
{
    public const string ModId = "firewood";

    private static ModSetting fromAnyWood;

    public static ModSetting FromAnyWood =>
        fromAnyWood ?? (fromAnyWood = ModSettings.Toggle(
            ModId, "fromAnyWood", "FIREWOOD FROM ANY WOOD", defaultValue: true,
            toolTip: "Adds recipes that cut wet firewood from sticks (4), water cane stems (2), " +
                     "shadeleaf bow staves (2) or shadeleaf canes (4), as well as from spoak " +
                     "branches. Changes what a save contains: a save made with this on names " +
                     "recipes a build without it does not have.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "Wet firewood from sticks, water cane stems, shadeleaf bow staves and canes, not only from spoak branches.",
            "HUD_thumbnail_spoak");
        _ = FromAnyWood;
    }

    /// <summary>The recipes: key, the wood, and how many of it make one wet firewood.</summary>
    public static readonly (string key, string wood, int count)[] Recipes =
    {
        ("chopFirewoodFromSticks", "item:sticks", 4),
        ("chopFirewoodFromWaterCaneStem", "item:waterCaneStem", 2),
        ("chopFirewoodFromShadeleafBowStave", "item:shadeleafBowStave", 2),
        ("chopFirewoodFromShadeleafCanes", "item:shadeleafCanes", 4),
    };

    /// <summary>Called by BaseDataLoader.InitProcessTypes. Adds nothing with the switch off.</summary>
    public static void AddProcesses(List<ProcessType> listOfProcessTypes)
    {
        if (!FromAnyWood.On)
        {
            return;
        }
        foreach (var (key, wood, count) in Recipes)
        {
            listOfProcessTypes.Add(new ProcessType
            {
                Name = "Chopping",
                KeyName = key,
                JobTypeKey = "craftingJobType",
                RequiredSkill = "menial",
                PhysicalWorkFactor = 4f,
                Stances = ProcessLoader.GetToolMakingStances(),
                Inputs = new Input[1]
                {
                    new Input
                    {
                        Entity = wood,
                        IsConsumed = true,
                        Amount = new InputAmount
                        {
                            NoOfItems = count
                        }
                    }
                },
                Outputs = new Output[1]
                {
                    new Output
                    {
                        EntityTypeToCreate = "item:wetFirewood",
                        Amount = new OutputAmount
                        {
                            NoOfItems = 1
                        }
                    }
                },
                WorkOrTimeNeeded = new WorkOrTime
                {
                    DaysNeeded = 0.0033333334f
                },
                ProcessToolSetKey = "toolSetChopWeakWood",
                AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
            });
        }
    }
}
