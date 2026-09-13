using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData;

public class EntityPolledEventsLoader
{
	public static List<EntityTypePolledEvent> Init()
	{
		return new List<EntityTypePolledEvent>
		{
			new EntityTypePolledEvent
			{
				KeyName = "smallPlotSpawningLoop",
				TypeKey = "structure:smallPlot",
				Scope = Scope.Entity,
				PolledEventKey = "farmPlotLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "largePlotSpawningLoop",
				TypeKey = "structure:largePlot",
				Scope = Scope.Entity,
				PolledEventKey = "farmPlotLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "improvisedGreenhouseSpawningLoop",
				TypeKey = "structure:improvisedGreenhouse",
				Scope = Scope.Entity,
				PolledEventKey = "farmPlotLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "greenhouseSpawningLoop",
				TypeKey = "structure:greenhouse",
				Scope = Scope.Entity,
				PolledEventKey = "farmPlotLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "personWeedingJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "weedingJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "personFertilizeJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "fertilizeJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "personPlantingJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "plantingJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "personHarvestJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "harvestJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "fishTrapShoreBasketSpawningLoop",
				TypeKey = "structure:fishTrapShoreBasket",
				Scope = Scope.Entity,
				PolledEventKey = "fishTrapSpawningLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "fishTrapShoreHoopNetSpawningLoop",
				TypeKey = "structure:fishTrapShoreHoopNet",
				Scope = Scope.Entity,
				PolledEventKey = "fishTrapSpawningLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "fishTrapCoastSpawningLoop",
				TypeKey = "structure:fishTrapCoast",
				Scope = Scope.Entity,
				PolledEventKey = "fishTrapSpawningLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "fishTrapCreekSticksSpawningLoop",
				TypeKey = "structure:fishTrapCreekSticks",
				Scope = Scope.Entity,
				PolledEventKey = "fishTrapSpawningLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "fishTrapCreekNetSpawningLoop",
				TypeKey = "structure:fishTrapCreekNet",
				Scope = Scope.Entity,
				PolledEventKey = "fishTrapSpawningLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "checkFishTrapJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "checkFishTrapJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "checkAnimalTrapJobLoop",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "checkAnimalTrapJobLoop"
			},
			new EntityTypePolledEvent
			{
				KeyName = "constructDeadfallTrap",
				TypeKey = "entity:human",
				Scope = Scope.Expedition,
				PolledEventKey = "checkAnimalTrapJobLoop"
			}
		};
	}
}
