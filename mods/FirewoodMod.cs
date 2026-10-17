using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// Wet firewood from any wood, not only spoak branches.
///
/// THE REQUEST. Kastuk, "Firewood out of any wood" (2026-10-13): "Let's add alternative recipes of
/// wet firewood (how much is need): sticks (4), water cane stem (2), shadeleafBowStave (2),
/// shadeleafCanes (4)." Then, 2026-10-14: "Tweak recipes: 3 sticks, 3 shadeleaf canes." Each makes 1 wet firewood, which dries into firewood on the studio's
/// firewood stack (makeFirewood, 10 at a time) like the studio's own chopped spoak.
///
/// HOW. The studio's chopFirewood is the template: 1 spoak branch -> 2 wet firewood, crafting,
/// menial, 1/300 day. These are lighter wood, so the weak-wood tool set (knives, hand axes,
/// machetes) rather than the tough-wood one, and nothing is left over. Adds ProcessType keys only,
/// no EntityType keys; a save made with them is stamped with the switch (ModSettings.Signature).
///
/// THE WOODPILE KEEPS ITS FIREWOOD. Kastuk, same thread, 2026-10-14/17: "I think Woodpile can be used
/// as stockpile for Firewood, just like Compost shed for waste ... I like to have visual pile of
/// firewood to check remained quantity without looking into inventories ... Let it be a) for now."
/// Option (a): no new storage, the dried batch simply stays where the studio puts it.
///
/// What the studio does (original_src). makeFirewood (ProcessLoader ~8490) dries 10 wet firewood
/// into 10 firewood placed in the woodpile's production output (ToolContainerTagsToPlaceIn
/// "woodpileTool"), 6 bulk "room enough for one output batch and one in production" (StructureLoader
/// ~8759), its sprite Full above 0.8 "when it holds a batch of 10 in storage and has another batch
/// being made" and HalfFull above 0.15. Two things empty it all the same:
/// - HaulingJobManager plans a haul for every item its colony owns (CreateAllItemsList). A woodpile is
///   no storage location (HandleStructureForStorage wants HasItemStorage and IsPermanentStorage), so an
///   item in it is never IsCurrentlyInThisStorage, and once dry (Entity.CanBeHauled: IsCompleted) it is
///   hauled to a stockpile or the ground near camp.
/// - GoalDoProduce.Activate calls EmptyToolContainers ("empty old products") before every batch,
///   which ejects whatever the woodpile still holds (ItemStorage.UncontainAllEntities).
/// The outputs of a batch are created and placed in the woodpile when it STARTS, at Progress 0
/// (SimProcess.CreateOutputsFromInputs/PlaceNewItem), which is the "one in production" half; output
/// that does not fit is put outside, "don't fail production" - production never stalls on room.
///
/// What this changes, behind KEEP DRY FIREWOOD IN THE WOODPILE: the hauling planner leaves an item in
/// a woodpile alone (StaysInWoodpile, from HaulingJobManager.ItemIsOK), and starting a batch ejects
/// only what the new batch needs room for (MakeRoomForBatch, from GoalDoProduce.EmptyToolContainers):
/// one batch stays, a second is made beside it, and the overflow - a third - goes on the ground and
/// from there to stockpiles as before. Nothing about what a save holds changes.
///
/// Who takes from it, unchanged: a fire's refuelling (GoalEvaluator.IsReplenishItemOk via
/// GetAllReplenishItemsSortedByDistance, which searches the KnownEntityDataTree - contained items
/// included, Entity.cs "now see inside containers", the woodpile "humanTransact") and a process input
/// (EvaluateHaulingJobs.GetAllHaulingCombosForItemType over the colony's Items, IsItemValidForHauling)
/// both ask only Entity.CanBeHauled, and fetch with the same GoalPickup the haul to a stockpile uses
/// on the woodpile today. The studio's own Scenario 5 starts with firewood in the woodpile
/// ("startFirewoodInWoodpile", isProductionOutput: true).
/// </summary>
public static class FirewoodMod
{
    public const string ModId = "firewood";

    private static ModSetting fromAnyWood;

    public static ModSetting FromAnyWood =>
        fromAnyWood ?? (fromAnyWood = ModSettings.Toggle(
            ModId, "fromAnyWood", "FIREWOOD FROM ANY WOOD", defaultValue: true,
            toolTip: "Adds recipes that cut wet firewood from sticks (3), water cane stems (2), " +
                     "shadeleaf bow staves (2) or shadeleaf canes (3), as well as from spoak " +
                     "branches. Changes what a save contains: a save made with this on names " +
                     "recipes a build without it does not have.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Registered from Program.Main BEFORE any data table is built: AddProcesses reads the switch.</summary>
    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "Wet firewood from sticks, water cane stems, shadeleaf bow staves and canes, not only from spoak branches. Dried firewood stays in the woodpile.",
            "HUD_thumbnail_spoak");
        _ = FromAnyWood;
        _ = KeepInWoodpile;
    }

    private static ModSetting keepInWoodpile;

    public static ModSetting KeepInWoodpile =>
        keepInWoodpile ?? (keepInWoodpile = ModSettings.Toggle(
            ModId, "keepInWoodpile", "KEEP DRY FIREWOOD IN THE WOODPILE", defaultValue: true,
            toolTip: "Dried firewood stays in the woodpile instead of being hauled to a stockpile, so " +
                     "the pile shows how much is left. Fires, kitchens and kilns take it from there. " +
                     "A woodpile holds one dried batch and the next one drying; more than that is " +
                     "put down beside it and hauled away as before.",
            affectsSimulation: true));

    /// <summary>The tag the studio gives the woodpile's tool (StructureLoader ~8715): "used for placing the output in the tool container".</summary>
    private const string WoodpileTag = "woodpileTool";

    /// <summary>A woodpile: a tool tagged woodpileTool with somewhere to keep its output.</summary>
    public static bool IsWoodpile(EntityType type) =>
        type?.ToolType?.ToolTag != null && type.ToolType.ToolTag.Contains(WoodpileTag)
        && (type.ContainerType as ToolContainerType)?.ProductionOutputStorageType != null;

    /// <summary>
    /// Called by HaulingJobManager.ItemIsOK: true for an item the hauling planner must leave where it
    /// is - one held by a woodpile. Fuel and input fetching do not ask this (see the class summary).
    /// </summary>
    public static bool StaysInWoodpile(IKnownEntityData item)
    {
        return KeepInWoodpile.On && item.ContainedBy.HasValue && IsWoodpile(Entity.FindByID(item.ContainedBy)?.EntityType);
    }

    /// <summary>
    /// The bulk a batch of the process puts in this woodpile: makeFirewood's 10 firewood at 0.3 is 3.
    /// Null when an output's count or bulk is not fixed - then nobody can say, and the studio's
    /// emptying stands.
    /// </summary>
    public static float? BatchBulk(ProcessType processType, EntityType woodpile)
    {
        float bulk = 0f;
        foreach (Output output in processType?.Outputs ?? new Output[0])
        {
            if (output.ToolContainerToPlaceIn == null || !output.ToolContainerToPlaceIn.Contains(woodpile))
            {
                continue;
            }
            int? count = output.Amount?.NoOfItems;
            float? each = output.FinalEntityTypeToCreate?.ItemType?.MaximumBulk;
            if (!count.HasValue || !each.HasValue)
            {
                return null;
            }
            bulk += count.Value * each.Value;
        }
        return bulk;
    }

    /// <summary>
    /// What to put outside so that room is left free: unfinished items first (a batch that never
    /// finished, which the studio ejected too), then finished ones from the end, until what stays
    /// leaves room - with the tolerance the container's own capacity test uses (ItemStorage.HasCapacityForItem).
    /// </summary>
    public static List<T> ToEject<T>(IList<T> stored, Func<T, float> bulkOf, Func<T, bool> isFinished, float capacity, float room)
    {
        List<T> eject = stored.Where(item => !isFinished(item)).ToList();
        List<T> finished = stored.Where(isFinished).ToList();
        float kept = finished.Sum(bulkOf);
        for (int i = finished.Count - 1; i >= 0 && !Common.IsLessThanOrEqual(room, capacity - kept); i--)
        {
            eject.Add(finished[i]);
            kept -= bulkOf(finished[i]);
        }
        return eject;
    }

    /// <summary>
    /// Called by GoalDoProduce.EmptyToolContainers for each tool before a batch starts. True when
    /// it has made the room itself (a woodpile, switch on), false for the studio's "empty old
    /// products" (UncontainAllProductionOutput).
    /// </summary>
    public static bool MakeRoomForBatch(Entity tool, ProcessType processType)
    {
        if (!KeepInWoodpile.On || !IsWoodpile(tool.EntityType) || !(tool.Contains is IHoldsProductionOutput output)
            || !output.TotalOutputCapacity.HasValue)
        {
            return false;
        }
        float? room = BatchBulk(processType, tool.EntityType);
        if (!room.HasValue)
        {
            return false;
        }
        List<Entity> stored = tool.Contains.GetContainedItemsList(_ => true);
        foreach (Entity item in ToEject(stored, e => e.Bulk, e => e.IsCompleted(), output.TotalOutputCapacity.Value, room.Value))
        {
            // As ItemStorage.UncontainAllEntities does it, one item at a time.
            if (tool.Contains.Remove(item))
            {
                tool.Contains.EjectEntity(item);
            }
        }
        return true;
    }

    /// <summary>The recipes: key, the wood, and how many of it make one wet firewood.</summary>
    public static readonly (string key, string wood, int count)[] Recipes =
    {
        ("chopFirewoodFromSticks", "item:sticks", 3),
        ("chopFirewoodFromWaterCaneStem", "item:waterCaneStem", 2),
        ("chopFirewoodFromShadeleafBowStave", "item:shadeleafBowStave", 2),
        ("chopFirewoodFromShadeleafCanes", "item:shadeleafCanes", 3),
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
