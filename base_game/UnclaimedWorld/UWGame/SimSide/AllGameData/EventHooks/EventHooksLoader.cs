using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.EventHooks;

public class EventHooksLoader
{
	public static List<AgentActionHook> InitAgentActionHooks()
	{
		return new List<AgentActionHook>
		{
			new AgentActionHook
			{
				KeyName = "humanDecidedToLeave",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.DecidedToLeaveAllegiance,
				ActionSetsKey = "humanDecidedToLeaveDialog"
			},
			new AgentActionHook
			{
				KeyName = "humanDecidedToLeaveRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.StartsToLeaveAllegiance,
				ActionSetsKey = "humanDecidedToLeaveRemark"
			},
			new AgentActionHook
			{
				KeyName = "otherAgentRequestsCarriedItem",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.ForceDropsItem,
				ActionSetsKey = "otherAgentRequestsCarriedItemRemark"
			}
		};
	}

	public static List<AttackTypeActionHook> InitAttackTypeHooks()
	{
		return new List<AttackTypeActionHook>();
	}

	public static List<EntityEventHook> InitEntityEventHooks()
	{
		return new List<EntityEventHook>
		{
			new EntityEventHook
			{
				KeyName = "smallPlotToBeDestroyedHook",
				TypeKey = "structure:smallPlot",
				Hook = EntityEventHooks.ToBeDestroyed,
				ActionSetsKey = "smallPlotToBeDestroyed"
			},
			new EntityEventHook
			{
				KeyName = "smallPlotToBeDestroyedHook2",
				TypeKey = "structure:smallPlot",
				Hook = EntityEventHooks.ToBeDestroyed,
				ActionSetsKey = "transcripePlot"
			},
			new EntityEventHook
			{
				KeyName = "largePlotToBeDestroyedHook",
				TypeKey = "structure:largePlot",
				Hook = EntityEventHooks.ToBeDestroyed,
				ActionSetsKey = "largePlotToBeDestroyed"
			},
			new EntityEventHook
			{
				KeyName = "largePlotToBeDestroyedHook2",
				TypeKey = "structure:largePlot",
				Hook = EntityEventHooks.ToBeDestroyed,
				ActionSetsKey = "transcripePlot"
			},
			new EntityEventHook
			{
				KeyName = "CompleteFishTrapCreekSticks",
				TypeKey = "structure:fishTrapCreekSticks",
				Hook = EntityEventHooks.ComeOnline,
				ActionSetsKey = "fishTrapFinished"
			},
			new EntityEventHook
			{
				KeyName = "CompleteFishTrapCreekNet",
				TypeKey = "structure:fishTrapCreekNet",
				Hook = EntityEventHooks.ComeOnline,
				ActionSetsKey = "fishTrapFinished"
			},
			new EntityEventHook
			{
				KeyName = "CompleteFishTrapCoast",
				TypeKey = "structure:fishTrapCoast",
				Hook = EntityEventHooks.ComeOnline,
				ActionSetsKey = "fishTrapFinished"
			},
			new EntityEventHook
			{
				KeyName = "CompleteFishTrapShoreBasket",
				TypeKey = "structure:fishTrapShoreBasket",
				Hook = EntityEventHooks.ComeOnline,
				ActionSetsKey = "fishTrapFinished"
			},
			new EntityEventHook
			{
				KeyName = "CompleteFishTrapShoreHoopNet",
				TypeKey = "structure:fishTrapShoreHoopNet",
				Hook = EntityEventHooks.ComeOnline,
				ActionSetsKey = "fishTrapFinished"
			}
		};
	}

	public static List<EffectTypeActionHook> InitEffectTypeHooks()
	{
		return new List<EffectTypeActionHook>
		{
			new EffectTypeActionHook
			{
				KeyName = "usesStimulantHook",
				Hook = AgentActionHooks.StartedEffect,
				TypeKey = "caffeine",
				ActionSetsKey = "usesStimulantRemark"
			},
			new EffectTypeActionHook
			{
				KeyName = "endedStimulantHook",
				Hook = AgentActionHooks.EndedEffect,
				TypeKey = "caffeine",
				ActionSetsKey = "endedStimulantRemark"
			}
		};
	}

	public static List<ProcessTypeActionHook> InitProcessTypeHooks()
	{
		return new List<ProcessTypeActionHook>
		{
			new ProcessTypeActionHook
			{
				KeyName = "snareCompleted1",
				TypeKey = "constructSpringSnare",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "snareFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "snareCompleted2",
				TypeKey = "constructSpringSnare",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructDeadfallTrapCompleted1",
				TypeKey = "constructDeadfallTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "snareFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructDeadfallTrapCompleted2",
				TypeKey = "constructDeadfallTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructRatTrapCompleted1",
				TypeKey = "constructSpikeTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ExecutionOrder = 0,
				ActionSetsKey = "snareFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructRatTrapCompleted3",
				TypeKey = "constructSpikeTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ExecutionOrder = 1,
				ActionSetsKey = "customSpikeTrapProperties"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructRatTrapCompleted2",
				TypeKey = "constructSpikeTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ExecutionOrder = 2,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "changeBaitToBlackpulpCompleted",
				TypeKey = "changeBaitToBlackpulp",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "changeBaitToBlackpulp"
			},
			new ProcessTypeActionHook
			{
				KeyName = "changeBaitToGlassyCreeperCompleted",
				TypeKey = "changeBaitToGlassyCreeper",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "changeBaitToGlassyCreeper"
			},
			new ProcessTypeActionHook
			{
				KeyName = "changeBaitToRatMeatCompleted",
				TypeKey = "changeBaitToRatMeat",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "changeBaitToRatMeat"
			},
			new ProcessTypeActionHook
			{
				KeyName = "changeBaitToNoBaitCompleted",
				TypeKey = "changeBaitToNoBait",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "changeBaitToNoBait"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateSnareCompleted1",
				TypeKey = "activateSnare",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateSnareCompleted2",
				TypeKey = "activateSnare",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "animalTrapCheckTalk"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithBlackpulpCompleted1",
				TypeKey = "activateTrapWithBlackpulp",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithBlackpulpCompleted2",
				TypeKey = "activateTrapWithBlackpulp",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "animalTrapCheckTalk"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithGlassyCreeperCompleted1",
				TypeKey = "activateTrapWithGlassyCreeper",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithGlassyCreeperCompleted2",
				TypeKey = "activateTrapWithGlassyCreeper",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "animalTrapCheckTalk"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithRatMeatCompleted1",
				TypeKey = "activateTrapWithRatMeat",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "activateSnare"
			},
			new ProcessTypeActionHook
			{
				KeyName = "activateTrapWithRatMeatCompleted2",
				TypeKey = "activateTrapWithRatMeat",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "animalTrapCheckTalk"
			},
			new ProcessTypeActionHook
			{
				KeyName = "killTrappedAnimalCompleted",
				TypeKey = "killTrappedAnimal",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "killContainingEntities"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishSmallPlotCompleted1",
				TypeKey = "establishSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "smallPlotFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishSmallPlotCompleted2",
				TypeKey = "establishSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plotEstablished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishLargePlotCompleted1",
				TypeKey = "establishLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "largePlotFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishLargePlotCompleted2",
				TypeKey = "establishLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plotEstablished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishGreenhouseCompleted1",
				TypeKey = "constructImprovisedGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "greenhouseFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "establishGreenhouseCompleted2",
				TypeKey = "constructGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "greenhouseFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantGlassyCreeperPodsInSmallPlotHook",
				TypeKey = "plantGlassyCreeperPodsInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeGlassyCreeperPodsInSmallPlotHook",
				TypeKey = "plantGlassyCreeperPodsInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeGlassyCreeperPodsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantGlassyCreeperPodsInLargePlotHook",
				TypeKey = "plantGlassyCreeperPodsInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeGlassyCreeperPodsInLargePlotHook",
				TypeKey = "plantGlassyCreeperPodsInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeGlassyCreeperPodsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantGlassyCreeperPodsInGreenhouseHook",
				TypeKey = "plantGlassyCreeperPodsInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeGlassyCreeperPodsInGreenhouseHook",
				TypeKey = "plantGlassyCreeperPodsInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeGlassyCreeperPodsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantCottonInSmallPlotHook",
				TypeKey = "plantCottonInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeCottonInSmallPlotHook",
				TypeKey = "plantCottonInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeCottonAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantCottonInLargePlotHook",
				TypeKey = "plantCottonInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeCottonInLargePlotHook",
				TypeKey = "plantCottonInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeCottonAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantCrystalBerriesInSmallPlotHook",
				TypeKey = "plantCrystalBerriesInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeCrystalBerriesInSmallPlotHook",
				TypeKey = "plantCrystalBerriesInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeCrystalBerriesAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantCrystalBerriesInLargePlotHook",
				TypeKey = "plantCrystalBerriesInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeCrystalBerriesInLargePlotHook",
				TypeKey = "plantCrystalBerriesInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeCrystalBerriesAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantCrystalBerriesInGreenhouseHook",
				TypeKey = "plantCrystalBerriesInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeCrystalBerriesInGreenhouseHook",
				TypeKey = "plantCrystalBerriesInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeCrystalBerriesAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "plantFingerFruitInGreenhouseHook",
				TypeKey = "plantFingerFruitInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "plantSeedsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "initializeFingerFruitInGreenhousePlotHook",
				TypeKey = "plantFingerFruitInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "initializeFingerFruitsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "weedPlotHook1",
				TypeKey = "weedPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "weedPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "weedPlotHook2",
				TypeKey = "weedLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "weedPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "weedPlotHook3",
				TypeKey = "weedGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "weedPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "organicFertilizePlotHook1",
				TypeKey = "organicFertilizePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "organicFertilizePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "organicFertilizePlotHook2",
				TypeKey = "organicFertilizeLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "organicFertilizePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "guanoFertilizePlotHook1",
				TypeKey = "guanoFertilizePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "guanoFertilizePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "guanoFertilizePlotHook2",
				TypeKey = "guanoFertilizeLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "guanoFertilizePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "harvestCropsHook",
				TypeKey = "harvestCrops",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "harvestCropsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "harvestCropsHook2",
				TypeKey = "harvestLargeCrops",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "harvestCropsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "harvestCropsHook3",
				TypeKey = "harvestGreenhouseCrops",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "harvestCropsAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "useOrganicFertilizerHook",
				TypeKey = "useOrganicFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "useOrganicFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "useLargeOrganicFertilizerHook",
				TypeKey = "useLargeOrganicFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "useLargeOrganicFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "useGuanoFertilizerHook",
				TypeKey = "useGuanoFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "useGuanoFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "useLargeGuanoFertilizerHook",
				TypeKey = "useLargeGuanoFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "useLargeGuanoFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopUsingLargeGuanoFertilizerHook",
				TypeKey = "stopUsingLargeGuanoFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopUsingFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopUsingGuanoFertilizerHook",
				TypeKey = "stopUsingGuanoFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopUsingFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopUsingLargeOrganicFertilizerHook",
				TypeKey = "stopUsingLargeOrganicFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopUsingFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopUsingOrganicFertilizerHook",
				TypeKey = "stopUsingOrganicFertilizer",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopUsingFertilizerAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growCrystalBerriesInLargePlotHook",
				TypeKey = "growCrystalBerriesInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growCrystalBerriesInLargePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingCrystalBerriesInLargePlotHook",
				TypeKey = "stopGrowingCrystalBerriesInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growGlassyCreeperPodsInLargePlotHook",
				TypeKey = "growGlassyCreeperPodsInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growGlassyCreeperPodsInLargePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingGlassyCreeperPodsInLargePlotHook",
				TypeKey = "stopGrowingGlassyCreeperPodsInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growCottonInLargePlotHook",
				TypeKey = "growCottonInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growCottonInLargePlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingCottonInLargePlotHook",
				TypeKey = "stopGrowingCottonInLargePlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growCrystalBerriesInSmallPlotHook",
				TypeKey = "growCrystalBerriesInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growCrystalBerriesInSmallPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingCrystalBerriesInSmallPlotHook",
				TypeKey = "stopGrowingCrystalBerriesInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growGlassyCreeperPodsInSmallPlotHook",
				TypeKey = "growGlassyCreeperPodsInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growGlassyCreeperPodsInSmallPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingGlassyCreeperPodsInSmallPlotHook",
				TypeKey = "stopGrowingGlassyCreeperPodsInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growCottonInSmallPlotHook",
				TypeKey = "growCottonInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growCottonInSmallPlotAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingCottonInSmallPlotHook",
				TypeKey = "stopGrowingCottonInSmallPlot",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growCrystalBerriesInGreenhouseHook",
				TypeKey = "growCrystalBerriesInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growCrystalBerriesInGreenhouseAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingCrystalBerriesInGreenhouseHook",
				TypeKey = "stopGrowingCrystalBerriesInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growGlassyCreeperPodsInGreenhouseHook",
				TypeKey = "growGlassyCreeperPodsInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growGlassyCreeperPodsInGreenhouseAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingGlassyCreeperPodsInGreenhouseHook",
				TypeKey = "stopGrowingGlassyCreeperPodsInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "growFingerFruitInGreenhouseHook",
				TypeKey = "growFingerFruitInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "growFingerFruitInGreenhouseAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "stopGrowingFingerFruitInGreenhouseHook",
				TypeKey = "stopGrowingFingerFruitInGreenhouse",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "stopGrowingAction"
			},
			new ProcessTypeActionHook
			{
				KeyName = "checkFishTrapHook",
				TypeKey = "checkFishTrap",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "checkFishTrap"
			},
			new ProcessTypeActionHook
			{
				KeyName = "checkFishTrapHook2",
				TypeKey = "checkFishTrap",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "fishCheckTalk"
			}
		};
	}
}
