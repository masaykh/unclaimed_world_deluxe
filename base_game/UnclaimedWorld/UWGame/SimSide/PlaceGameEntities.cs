using System;
using System.Collections.Generic;
using System.Linq;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide;

public class PlaceGameEntities
{
	public delegate void LoadDebugScenario();

	public enum DebugScenarios
	{
		BushDragon,
		NeedsTest,
		WeaponsTest,
		EmptyMap,
		PlaceGameEntitiesMilestoneBuild,
		PlaceGameEntitiesTestMap,
		CollisionTest,
		TestMicroMap,
		MapApril2013,
		QuarterSizeMap,
		DemoIslandMap,
		QuarterSizeMap_Alt_Test_v1,
		BuildingSoundTest,
		BuildingTest,
		HarvestTest2,
		HarvestPickupTest,
		FindPreyTest,
		AnimTweak,
		ModelTest,
		WildernessCampScreenshot,
		HarvestTest,
		GeometryTest,
		CookingTest,
		MLoAnimationTest,
		FightTest,
		RobotTest,
		AssetGeometryMap,
		StructureGeoMap,
		ProductionTest,
		StorageTest,
		StorageTest2,
		ShelterTest,
		DecompositionTest,
		QuarterSizeMapBuildingTest,
		GaitTest,
		None,
		SoundTest,
		UnloadTest,
		AttackNestTest,
		ThreatTest,
		SkimmerSalvageTest,
		TwinklerEatTest,
		CampFireTest,
		RatEatTest,
		ChickenEatTest,
		AllStructuresBeingBuilt,
		DemonTreeTest,
		ButcherTest,
		SpikePlantTest,
		FarmTest,
		BushDragonParticleTest,
		FishTrapTest,
		ManyAgentsTest,
		LongTermFarmingTest,
		ScareCrowTest,
		ShaiHuludTest,
		PlotAssetTest,
		AmbientSoundTest,
		AllItemsTest,
		HuntTest,
		ItemsNotCarriedBugHunt,
		CarcassNoMeat,
		TrapTest,
		CommTest,
		mineShowcase,
		WorldMapTest,
		MudBrick,
		FuelTest,
		Recording,
		QuaditeTest,
		RemoveAttachablesBug,
		ReloadTest,
		MemoryTest,
		MapEdgeTest,
		SleepInShiftsTest,
		TurnipHutTest,
		BreedingTest,
		AnimTest,
		Compost,
		PathingTest,
		EatTest,
		StationaryToolTest,
		PierTest,
		MusicTest,
		DogEatTest,
		MidsizeTest,
		VerminTest,
		SkillTest,
		DetectionTest,
		RepairTest,
		ClayPitTest,
		UpgradeTest,
		NanomapTest,
		PeatTest,
		LightingTest
	}

	private static Dictionary<DebugScenarios, Tuple<StartDebugScenarioParams, LoadDebugScenario>> loadScenarioFunctions;

	private static GoalPlanner doSomething;

	private static DebugScenarios GetDefaultScenario()
	{
		return DebugScenarios.TwinklerEatTest;
	}

	static PlaceGameEntities()
	{
		loadScenarioFunctions = new Dictionary<DebugScenarios, Tuple<StartDebugScenarioParams, LoadDebugScenario>>();
		doSomething = DoSomething;
		AddScenario(DebugScenarios.BushDragon, "d Mezzomap MLo", BushDragon);
		AddScenario(DebugScenarios.NeedsTest, "d Mezzomap MLo", NeedsTest);
		AddScenario(DebugScenarios.WeaponsTest, "d Mezzomap MLo", WeaponsTest);
		AddScenario(DebugScenarios.LightingTest, "d Mezzomap MLo", LightingTest);
		AddScenario(DebugScenarios.EmptyMap, "d Mezzomap MLo", EmptyMap);
		AddScenario(DebugScenarios.PlaceGameEntitiesMilestoneBuild, "d Mezzomap MLo", PlaceGameEntitiesMilestoneBuild);
		AddScenario(DebugScenarios.PlaceGameEntitiesTestMap, "d Mezzomap MLo", PlaceGameEntitiesTestMap);
		AddScenario(DebugScenarios.CollisionTest, "d Mezzomap MLo", CollisionTest);
		AddScenario(DebugScenarios.TestMicroMap, "d Mezzomap MLo", TestMicroMap);
		AddScenario(DebugScenarios.MapApril2013, "d Mezzomap MLo", MapApril2013);
		AddScenario(DebugScenarios.QuarterSizeMap, "d Mezzomap MLo", QuarterSizeMap);
		AddScenario(DebugScenarios.DemoIslandMap, "d Mezzomap MLo", DemoIslandMap);
		AddScenario(DebugScenarios.QuarterSizeMap_Alt_Test_v1, "d Mezzomap MLo", QuarterSizeMap_Alt_Test_v1);
		AddScenario(DebugScenarios.BuildingSoundTest, "d Mezzomap MLo", BuildingSoundTest);
		AddScenario(DebugScenarios.BuildingTest, "d Mezzomap MLo", BuildingTest);
		AddScenario(DebugScenarios.HarvestTest2, "d Mezzomap MLo", HarvestTest2);
		AddScenario(DebugScenarios.HarvestPickupTest, "d Mezzomap MLo", HarvestPickupTest);
		AddScenario(DebugScenarios.FindPreyTest, "d Mezzomap MLo", FindPreyTest);
		AddScenario(DebugScenarios.AnimTweak, "d Mezzomap MLo", AnimTweak);
		AddScenario(DebugScenarios.ModelTest, "d Mezzomap MLo", ModelTest);
		AddScenario(DebugScenarios.WildernessCampScreenshot, "d Mezzomap MLo", WildernessCampScreenshot);
		AddScenario(DebugScenarios.HarvestTest, "d Mezzomap MLo", HarvestTest);
		AddScenario(DebugScenarios.GeometryTest, "d Mezzomap MLo", GeometryTest);
		AddScenario(DebugScenarios.CookingTest, "d Mezzomap MLo", CookingTest);
		AddScenario(DebugScenarios.VerminTest, "d Mezzomap MLo", VerminTest);
		AddScenario(DebugScenarios.PathingTest, "d Mezzomap MLo", PathingTest);
		AddScenario(DebugScenarios.MLoAnimationTest, "d Mezzomap MLo", MLoAnimationTest);
		AddScenario(DebugScenarios.FightTest, "d Mezzomap MLo", FightTest);
		AddScenario(DebugScenarios.AssetGeometryMap, "d Mezzomap MLo", AssetGeometryMap);
		AddScenario(DebugScenarios.StructureGeoMap, "d Mezzomap MLo", StructureGeoMap);
		AddScenario(DebugScenarios.ProductionTest, "d Mezzomap MLo", ProductionTest);
		AddScenario(DebugScenarios.AnimTest, "d Mezzomap MLo", AnimTest);
		AddScenario(DebugScenarios.CampFireTest, "d Mezzomap MLo", CampfireTest);
		AddScenario(DebugScenarios.StorageTest, "d Mezzomap MLo", StorageTest);
		AddScenario(DebugScenarios.StorageTest2, "d Mezzomap MLo", StorageTest2);
		AddScenario(DebugScenarios.ShelterTest, "d Mezzomap MLo", ShelterTest);
		AddScenario(DebugScenarios.DecompositionTest, "d Mezzomap MLo", DecompositionTest);
		AddScenario(DebugScenarios.GaitTest, "d Mezzomap MLo", GaitTest);
		AddScenario(DebugScenarios.UnloadTest, "d Mezzomap MLo", UnloadTest);
		AddScenario(DebugScenarios.QuarterSizeMapBuildingTest, "d Mezzomap MLo", QuarterSizeMapBuildingTest);
		AddScenario(DebugScenarios.AttackNestTest, "d Mezzomap MLo", AttackNestTest);
		AddScenario(DebugScenarios.SkimmerSalvageTest, "d Mezzomap MLo", SkimmerSalvageTest);
		AddScenario(DebugScenarios.TwinklerEatTest, "d Mezzomap MLo", TwinklerEatTest);
		AddScenario(DebugScenarios.RatEatTest, "d Mezzomap MLo", RatEatTest);
		AddScenario(DebugScenarios.ChickenEatTest, "d Mezzomap MLo", ChickenEatTest);
		AddScenario(DebugScenarios.AllStructuresBeingBuilt, "d Mezzomap MLo", AllStructuresBeingBuilt);
		AddScenario(DebugScenarios.DemonTreeTest, "d Mezzomap MLo", DemonTreeTest);
		AddScenario(DebugScenarios.ButcherTest, "d Mezzomap MLo", ButcherTest);
		AddScenario(DebugScenarios.SpikePlantTest, "d Mezzomap MLo", SpikePlantTest);
		AddScenario(DebugScenarios.FarmTest, "d Mezzomap MLo", FarmTest);
		AddScenario(DebugScenarios.NanomapTest, "Nanomap", NanomapTest);
		AddScenario(DebugScenarios.BushDragonParticleTest, "d Mezzomap MLo", BushDragonParticleTest);
		AddScenario(DebugScenarios.FishTrapTest, "d Mezzomap MLo", FishTrapTest);
		AddScenario(DebugScenarios.ManyAgentsTest, "d Mezzomap MLo", ManyAgentsTest);
		AddScenario(DebugScenarios.MemoryTest, "d Mezzomap MLo", MemoryTest);
		AddScenario(DebugScenarios.ScareCrowTest, "d Mezzomap MLo", ScareCrowTest);
		AddScenario(DebugScenarios.LongTermFarmingTest, "d Mezzomap MLo", LongTermFarmingTest);
		AddScenario(DebugScenarios.SoundTest, "d Mezzomap MLo", SoundTest);
		AddScenario(DebugScenarios.ShaiHuludTest, "d Mezzomap MLo", ShaiHuludTest);
		AddScenario(DebugScenarios.PlotAssetTest, "d Mezzomap MLo", PlotAssetTest);
		AddScenario(DebugScenarios.AmbientSoundTest, "d Mezzomap MLo", AmbientSoundTest);
		AddScenario(DebugScenarios.AllItemsTest, "d Mezzomap MLo", AllItemsTest);
		AddScenario(DebugScenarios.HuntTest, "d Mezzomap MLo", HuntTest);
		AddScenario(DebugScenarios.ItemsNotCarriedBugHunt, "d Mezzomap MLo", ItemsNotCarriedBugHunt);
		AddScenario(DebugScenarios.RobotTest, "d Mezzomap MLo", RobotTest);
		AddScenario(DebugScenarios.ReloadTest, "d Mezzomap MLo", ReloadTest);
		AddScenario(DebugScenarios.CarcassNoMeat, "d Mezzomap MLo", CarcassNoMeat);
		AddScenario(DebugScenarios.TrapTest, "d Mezzomap MLo", TrapTest);
		AddScenario(DebugScenarios.mineShowcase, "k Map Oasis Valley 2016", mineShowcase);
		AddScenario(DebugScenarios.WorldMapTest, "d Mezzomap MLo", WorldMapTest);
		AddScenario(DebugScenarios.MudBrick, "d Mezzomap MLo", MudBrickTest);
		AddScenario(DebugScenarios.MusicTest, "d Mezzomap MLo", MusicTest);
		AddScenario(DebugScenarios.FuelTest, "d Mezzomap MLo", FuelTest);
		AddScenario(DebugScenarios.Recording, "d Mezzomap MLo", Recording);
		AddScenario(DebugScenarios.QuaditeTest, "d Mezzomap MLo", QuaditeTest);
		AddScenario(DebugScenarios.RemoveAttachablesBug, "d Mezzomap MLo", RemoveAttachablesBug);
		AddScenario(DebugScenarios.DetectionTest, "d Mezzomap MLo", DetectionTest);
		AddScenario(DebugScenarios.MapEdgeTest, "d Mezzomap MLo", MapEdgeTest);
		AddScenario(DebugScenarios.SleepInShiftsTest, "d Mezzomap MLo", SleepInShiftsTest);
		AddScenario(DebugScenarios.TurnipHutTest, "d Mezzomap MLo", TurnipHutTest);
		AddScenario(DebugScenarios.BreedingTest, "d Mezzomap MLo", BreedingTest);
		AddScenario(DebugScenarios.Compost, "d Mezzomap MLo", Compost);
		AddScenario(DebugScenarios.ThreatTest, "d Mezzomap MLo", ThreatTest);
		AddScenario(DebugScenarios.EatTest, "d Mezzomap MLo", EatTest);
		AddScenario(DebugScenarios.SkillTest, "d Mezzomap MLo", SkillTest);
		AddScenario(DebugScenarios.DogEatTest, "d Mezzomap MLo", DogEatTest);
		AddScenario(DebugScenarios.StationaryToolTest, "d Mezzomap MLo", StationaryToolTest);
		AddScenario(DebugScenarios.CommTest, "d Mezzomap MLo", CommTest);
		AddScenario(DebugScenarios.PierTest, "d Mezzomap MLo", PierTest);
		AddScenario(DebugScenarios.MidsizeTest, "jx Map Hills Rivers Small", MidsizeTest);
		AddScenario(DebugScenarios.RepairTest, "d Mezzomap MLo", RepairTest);
		AddScenario(DebugScenarios.ClayPitTest, "d Mezzomap MLo", ClayPitTest);
		AddScenario(DebugScenarios.UpgradeTest, "d Mezzomap MLo", UpgradeTest);
		AddScenario(DebugScenarios.PeatTest, "Nanomap", PeatTest);
	}

	private static void AddScenario(DebugScenarios key, string map, LoadDebugScenario placeMethod)
	{
		StartDebugScenarioParams item = new StartDebugScenarioParams
		{
			ScenarioKey = key,
			MapKey = map
		};
		loadScenarioFunctions.Add(key, new Tuple<StartDebugScenarioParams, LoadDebugScenario>(item, placeMethod));
	}

	public static void BeginRun(DebugScenarios scenarioToRun)
	{
		loadScenarioFunctions[scenarioToRun].Item2();
	}

	public static StartDebugScenarioParams GetNewScenarioParams()
	{
		StartDebugScenarioParams item = loadScenarioFunctions[GetDefaultScenario()].Item1;
		return new StartDebugScenarioParams
		{
			MapKey = item.MapKey,
			ScenarioKey = item.ScenarioKey
		};
	}

	public static void BreedingTest()
	{
		Point point = new Point(6, 6);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		for (int i = 0; i < 100; i++)
		{
			PlacePerson("Charles", "Jacobi" + i, Reproduction.Male, point, Color.White, 40f, noSkills: false, expedition);
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), point);
	}

	public static void DetectionTest()
	{
		Point point = new Point(6, 6);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(point));
		new Point(point.X, point.Y - 1);
		for (int i = 0; i < 5; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		}
		for (int j = 0; j < 1; j++)
		{
		}
		GetBob(point, exp);
	}

	public static void RemoveAttachablesBug()
	{
		Point point = new Point(6, 6);
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), point);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
		Expedition ownerExpedition = new Expedition(allegiance, "Start2", "Start2", MapManager.TileToWorldPos(point));
		for (int i = 0; i < 1; i++)
		{
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(point.X - 1, point.Y), 20f, allegiance, null, ownerExpedition).BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
		}
		AddFinishedStructure("structure:sensor", new Point(point.X, point.Y + 1), owner);
	}

	public static void QuaditeTest()
	{
		Point point = new Point(6, 6);
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		AddFinishedStructure("structure:sensor", new Point(point.X, point.Y + 1), owner);
		The.MapUI.ZoomToMapPosition(6, 6);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);
		Expedition ownerExpedition = new Expedition(allegiance, "Start2", "Start2", MapManager.TileToWorldPos(point));
		for (int i = 0; i < 5; i++)
		{
			PlaceAnimal("entity:whiteThunderChicken", Reproduction.Female, new Point(point.X - 4, point.Y), 20f, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, ownerExpedition).BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
			PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Female, new Point(point.X - 4, point.Y), 20f, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, ownerExpedition);
			PlaceAnimal("entity:studdedThunderChicken", Reproduction.Female, new Point(point.X - 4, point.Y), 20f, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]), null, ownerExpedition);
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:musketoon"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
	}

	public static void MapEdgeTest()
	{
		Point tile = new Point(The.Map.mapTileWidth - 1, The.Map.mapTileHeight - 1);
		The.MapUI.ZoomToMapPosition(tile.X, tile.Y);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(tile));
		PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(tile.X - 4, tile.Y - 4), Color.White, 40f, noSkills: false, expedition);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
		Expedition ownerExpedition = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(tile));
		int num = 4;
		for (int i = 0; i < num; i++)
		{
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(tile.X, tile.Y), 18f, allegiance, null, ownerExpedition);
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(tile.X, tile.Y), 18f, allegiance, null, ownerExpedition);
		}
	}

	private static void FuelTest()
	{
		Point point = new Point(5, 5);
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(point.X + 1, point.Y + 1), Color.White, 40f, noSkills: false, expedition);
		PlacePerson("2", "2", Reproduction.Male, new Point(point.X + 1, point.Y + 1), Color.White, 40f, noSkills: false, expedition);
		AddFinishedStructure("structure:kilnImprovisedSmall", new Point(4, 5), expedition);
		AddFinishedStructure("structure:kilnImprovisedSmall", new Point(3, 5), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:unfiredClayPot"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:unfiredClayPot"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
	}

	public static void Compost()
	{
		Point point = new Point(6, 6);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(6, 6);
		PlacePerson("Charles", "Jacobi", Reproduction.Male, point, Color.White, 40f, noSkills: false, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetFirewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:rottenVegetables"]), point);
		AddFinishedStructure("structure:firewoodStack", new Point(7, 7), expedition);
		AddFinishedStructure("structure:compostBin", new Point(4, 5), expedition);
	}

	public static void UpgradeTest()
	{
		The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear
		{
			Year = 206,
			Day = 0,
			TimeOfDay = 0.46
		});
		Point point = new Point(9, 6);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(10, 6);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);
		Entity entity = AddFinishedStructure("structure:clayHut", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(4, 6)));
		UpgradeCategory upgradeCategory = GameData.Instance.AllUpgradeCategories["furniture4People"];
		AddColonyItemUpgrade("item:furniture4People", entity, upgradeCategory);
		expedition.OwnedEntities.SetUpgrade(entity.EntityID, upgradeCategory, GameData.Instance.AllEntityTypes["item:furniture4People"]);
		upgradeCategory = GameData.Instance.AllUpgradeCategories["bedsOrMats4People"];
		AddColonyItemUpgrade("item:beds4People", entity, upgradeCategory);
		expedition.OwnedEntities.SetUpgrade(entity.EntityID, upgradeCategory, GameData.Instance.AllEntityTypes["item:beds4People"]);
		AddFinishedStructure("structure:octagonalTent", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(8, 6)));
		entity = AddFinishedStructure("structure:clayHut", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(18, 6)));
		GetBob(null, expedition, entity);
		entity = AddFinishedStructure("structure:caneHut", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(7, 6)));
		entity = AddFinishedStructure("structure:domeShelterTarp", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(7, 4)));
		entity = AddFinishedStructure("structure:campfire", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(9, 4)));
		entity = AddFinishedStructure("structure:turnipHut", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(8, 8)));
		AddFinishedStructure("structure:workshopBuilding", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(8, 10)));
		AddFinishedStructure("structure:workshopBuilding", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(11, 10)));
		AddFinishedStructure("structure:workshopBuilding", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(14, 10)));
		AddFinishedStructure("structure:workshopBuilding", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(17, 10)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:extrusionMachineComponents"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:loomComponents"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shotgun"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalLatheComponents"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:humanPowerUnit"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:loomComponents"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:furniture"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:cotton"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurPowder"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:vinegar"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWorkersToolbox"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedMat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bedFrame"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:textile"]), point);
	}

	public static void StationaryToolTest()
	{
		The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear
		{
			Year = 206,
			Day = 0,
			TimeOfDay = 0.46
		});
		Point point = new Point(4, 6);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(10, 6);
		AddFinishedStructure("structure:clayHut", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(10, 6)));
		GetBob(point, expedition);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clayJar"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(14, 14));
		AddTree("tree:marshcotflower", new Point(1, 8), "crop:marshcotSap", 1);
		AddTree("tree:marshcotflower", new Point(1, 6), "crop:marshcotSap", 1);
		AddTree("tree:marshcotflower", new Point(1, 4), "crop:marshcotSap", 1);
		AddTree("tree:marshcotflower", new Point(2, 4), "crop:marshcotSap", 1);
	}

	private static void AddTree(string key, Point spot, string resourceKey = null, int? resourceAmount = null)
	{
		Entity entity = new Entity(GameData.Instance.AllEntityTypes[key]);
		AddTerrainItem(entity, spot);
		if (resourceKey != null)
		{
			entity.Find<UWGame.SimSide.Trees.Tree>(out var c);
			c.Crops[GameData.Instance.AllResourceTypes[resourceKey]].SetResourceItems(resourceAmount.Value);
		}
	}

	public static void MusicTest()
	{
		Point tile = new Point(6, 6);
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(tile));
		The.MapUI.ZoomToMapPosition(6, 6);
		foreach (KeyValuePair<string, PolledEventType> allPolledEvent in GameData.Instance.AllPolledEvents)
		{
			allPolledEvent.Value.LoadContent(The.Client.Content);
		}
		The.Sim.PlaySite.EventManager.AddPolledEvent("SANDBOXNOMADMAP_song1");
		The.Sim.PlaySite.EventManager.AddPolledEvent("SANDBOXNOMADMAP_song2");
	}

	public static void MudBrickTest()
	{
		Expedition expedition = new Expedition(center: MapManager.TileToWorldPos(new Point(6, 2)), allegiance: The.Sim.PlaySite.PlayerAllegiance, keyName: "Start", name: "Start");
		The.MapUI.ZoomToMapPosition(6, 6);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);
		Point point = new Point(5, 2);
		GetBob(point, expedition).Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
		GetBob(point, expedition).Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
		GetBob(point, expedition).Intelligence.Skills[GameData.Instance.AllSkillTypes["hunting"]].Value = 0.1f;
		GetBob(point, expedition);
		AddResource(new Point(2, 2), "firewood", 6);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stoneHammer"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stoneHammer"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelPickaxe"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedTrowel"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
		The.Map.GetTile(6, 6).AddResource("salt", 200);
		for (int i = 0; i < 202; i++)
		{
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(8, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), new Point(4, 3));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddFinishedStructure("structure:kiln", new Point(18, 6), expedition);
		AddFinishedStructure("structure:kiln", new Point(18, 2), expedition);
		AddFinishedStructure("structure:kiln", new Point(18, 4), expedition);
		AddFinishedStructure("structure:simpleSmithy", new Point(8, 5), expedition);
		AddFinishedStructure("structure:hideRack", new Point(4, 7), expedition);
		AddFinishedStructure("structure:firewoodStack", new Point(7, 7), expedition);
		AddFinishedStructure("structure:mudBrickKitchen", new Point(2, 8), expedition);
		AddFinishedStructure("structure:compostPit", new Point(8, 8), expedition);
		PlaceAnimal("entity:whiteThunderChicken", Reproduction.Female, new Point(point.X - 4, point.Y), 20f, new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]));
	}

	public static void Recording()
	{
		The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear
		{
			Year = 206,
			Day = 7,
			TimeOfDay = 0.36
		});
		Point point = new Point(40, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		PlacePerson("Andon", "Green", Reproduction.Male, point, Color.White, 40f, noSkills: false, "blueBrownClothes1", expedition);
		PlacePerson("Castor", "Hernes", Reproduction.Male, point, Color.White, 40f, noSkills: false, "greenGreyClothes1", expedition);
		PlacePerson("Linsey", "Cattier", Reproduction.Female, point, Color.White, 40f, noSkills: false, "greyClothes1", expedition);
		PlacePerson("Manny", "Pezal", Reproduction.Male, point, Color.White, 40f, noSkills: false, "whitePantsClothes1", expedition);
		PlacePerson("Nisa", "Manekki", Reproduction.Female, point, Color.White, 40f, noSkills: false, "greenBlueClothes1", expedition);
		PlacePerson("Illen", "Rence", Reproduction.Female, point, Color.White, 40f, noSkills: false, "whiteBlueClothes1", expedition);
		PlaceAnimal("entity:dog", Reproduction.Male, point, 5f, The.Sim.PlaySite.PlayerAllegiance);
		PlaceRobot("entity:haulingRobot", point, The.Sim.PlaySite.PlayerAllegiance, expedition);
		int num = 3;
		for (int i = 0; i < num; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:charcoal"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneSeeds"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), point);
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:pigFliesLive"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:brickMold"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalHooks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:hammer"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bellows"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bogOre"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:roughBloomIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:clay"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wetMudBrick"]), point);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
		Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
		Allegiance allegiance3 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:turnip"]);
		Allegiance allegiance4 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);
		Expedition ownerExpedition = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(point));
		Expedition ownerExpedition2 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:turnip"]), "Start3", "Start3", MapManager.TileToWorldPos(point));
		Expedition ownerExpedition3 = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]), "Start4", "Start4", MapManager.TileToWorldPos(point));
		int num2 = 4;
		for (int j = 0; j < num2; j++)
		{
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(point.X, point.Y + 1), 18f, allegiance2, null, ownerExpedition);
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(point.X + 6, point.Y - 4), 18f, allegiance2, null, ownerExpedition);
		}
		PlaceAnimal("entity:bird", Reproduction.Female, new Point(46, 37), 18f, allegiance4);
		PlaceAnimal("entity:bird", Reproduction.Female, new Point(47, 39), 8f, allegiance4);
		PlaceAnimal("entity:turnip", Reproduction.Female, new Point(46, 22), 18f, allegiance3, null, ownerExpedition2);
		PlaceAnimal("entity:turnip", Reproduction.Female, new Point(39, 21), 18f, allegiance3, null, ownerExpedition2);
		PlaceAnimal("entity:turnip", Reproduction.Female, new Point(39, 17), 18f, allegiance3, null, ownerExpedition2);
		PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(point.X + 11, point.Y - 7), 18f, allegiance, null, ownerExpedition3);
		PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(58, 21), 18f, allegiance, null, ownerExpedition3);
		PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(75, 18), 18f, allegiance, null, ownerExpedition3);
		AddFinishedStructure("structure:caneHut", new Point(point.X - 3, point.Y + 5), expedition);
		AddFinishedStructure("structure:clayHut", new Point(point.X - 3, point.Y), expedition);
		AddFinishedStructure("structure:turnipHut", new Point(point.X + 2, point.Y + 2), expedition);
		AddFinishedStructure("structure:kiln", new Point(point.X - 2, point.Y + 2), expedition);
		AddFinishedStructure("structure:improvisedSmithy", new Point(point.X, point.Y + 2), expedition);
		AddFinishedStructure("structure:improvisedKitchen", new Point(point.X - 5, point.Y + 1), expedition);
		AddFinishedStructure("structure:improvisedWorkbench", new Point(point.X + 1, point.Y - 1), expedition);
		Vector3[] array = new Vector3[1]
		{
			new Vector3(1894f, 884f, 0f)
		};
		foreach (Vector3 value in array)
		{
			AddFinishedStructure("structure:largePlot", null, expedition, flipHorizontally: false, value);
		}
		array = new Vector3[1]
		{
			new Vector3(1680f, 912f, 0f)
		};
		foreach (Vector3 value2 in array)
		{
			AddFinishedStructure("structure:smallPlot", null, expedition, flipHorizontally: false, value2);
		}
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");
		array = new Vector3[2]
		{
			new Vector3(1882f, 1042f, 0f),
			new Vector3(2030f, 536f, 0f)
		};
		foreach (Vector3 pos in array)
		{
			AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), pos);
		}
		array = new Vector3[14]
		{
			new Vector3(3168f, 3072f, 0f),
			new Vector3(2260f, 2020f, 0f),
			new Vector3(1700f, 1844f, 0f),
			new Vector3(1498f, 1882f, 0f),
			new Vector3(1550f, 1464f, 0f),
			new Vector3(2072f, 1256f, 0f),
			new Vector3(1680f, 912f, 0f),
			new Vector3(1688f, 624f, 0f),
			new Vector3(2119f, 952f, 0f),
			new Vector3(2297f, 960f, 0f),
			new Vector3(2450f, 682f, 0f),
			new Vector3(3024f, 390f, 0f),
			new Vector3(3130f, 272f, 0f),
			new Vector3(3498f, 1364f, 0f)
		};
		foreach (Vector3 pos2 in array)
		{
			AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), pos2);
		}
		array = new Vector3[2]
		{
			new Vector3(4254f, 233f, 0f),
			new Vector3(5303f, 252f, 0f)
		};
		foreach (Vector3 pos3 in array)
		{
			AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), pos3);
		}
		array = new Vector3[16]
		{
			new Vector3(3085f, 1066f, 0f),
			new Vector3(1940f, 1431f, 0f),
			new Vector3(2568f, 1738f, 0f),
			new Vector3(2708f, 2071f, 0f),
			new Vector3(3172f, 2292f, 0f),
			new Vector3(1840f, 2036f, 0f),
			new Vector3(1278f, 2420f, 0f),
			new Vector3(2089f, 2457f, 0f),
			new Vector3(3580f, 3130f, 0f),
			new Vector3(4000f, 2520f, 0f),
			new Vector3(4972f, 2150f, 0f),
			new Vector3(4618f, 2900f, 0f),
			new Vector3(2824f, 3784f, 0f),
			new Vector3(3178f, 4210f, 0f),
			new Vector3(2342f, 4362f, 0f),
			new Vector3(748f, 4282f, 0f)
		};
		foreach (Vector3 pos4 in array)
		{
			AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), pos4);
		}
	}

	public static void mineShowcase()
	{
		Point tile = new Point(50, 40);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(tile));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
		Expedition ownerExpedition = new Expedition(allegiance, "start2", "Start2", MapManager.TileToWorldPos(new Point(tile.X - 17, tile.Y - 17)));
		AddFinishedStructure("structure:sentry", null, expedition, flipHorizontally: false, new Vector3(tile.X * 48 + 40, tile.Y * 48 + 1, 0f));
		AddFinishedStructure("structure:sentry", null, expedition, flipHorizontally: false, new Vector3(tile.X * 48 + 28, tile.Y * 48 + 96, 0f));
		The.MapUI.ZoomToMapPosition(tile.X, tile.Y);
		for (int i = 0; i < 50; i++)
		{
			PlaceAnimal("entity:swarmer", Reproduction.Male, new Point(tile.X - 17, tile.Y - 17), 17f, allegiance, null, ownerExpedition).BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
		}
		PlacePerson("Andon", "Green", Reproduction.Male, new Point(tile.X - 3, tile.Y - 1), Color.MediumSeaGreen, 52f, noSkills: false, expedition, "blueBrownClothes1");
		PlacePerson("Linsey", "Cattier", Reproduction.Female, new Point(tile.X - 1, tile.Y), Color.Bisque, 39f, noSkills: false, expedition, "greenGreyClothes1");
		PlacePerson("Larsen", "Cattier", Reproduction.Female, new Point(tile.X - 1, tile.Y), Color.Bisque, 39f, noSkills: false, expedition, "greenGreyClothes1");
		PlacePerson("Andon", "Green", Reproduction.Male, new Point(tile.X - 3, tile.Y - 1), Color.MediumSeaGreen, 52f, noSkills: false, expedition, "blueBrownClothes1");
		PlacePerson("Linsey", "Cattier", Reproduction.Female, new Point(tile.X - 1, tile.Y), Color.Bisque, 39f, noSkills: false, expedition, "greenGreyClothes1");
		PlacePerson("Larsen", "Cattier", Reproduction.Female, new Point(tile.X - 1, tile.Y), Color.Bisque, 39f, noSkills: false, expedition, "greenGreyClothes1");
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(tile.X - 3, tile.Y - 1));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:landMine"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(tile.X - 3, tile.Y - 1));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:varmintBomb"]), new Point(tile.X - 1, tile.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(tile.X - 1, tile.Y));
		for (int j = 0; j < 2; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), new Point(tile.X - 2, tile.Y));
		}
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
		AddFinishedStructure("structure:smallPlot", new Point(tile.X - 2, tile.Y), expedition);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fieldQuaditeNest"]), new Vector3(3404f, 1400f, 0f));
	}

	public static void CommTest()
	{
		Point point = new Point(6, 6);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		GetBob(point, expedition);
		AddFinishedStructure("structure:simplePort", new Point(8, 8), expedition);
		for (int i = 0; i < 10; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
		}
	}

	public static void TrapTest()
	{
		Point point = new Point(3, 6);
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalAnimalTrapProperties");
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 9)));
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bajingan"]);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
		for (int i = 0; i < 1; i++)
		{
			Entity entity = PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(14, 6), 20f, allegiance);
			entity.Bulk = 0.2f;
			entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
			ImmobilizeEntity(entity);
		}
		The.MapUI.ZoomToMapPosition(6, 6);
		PlacePerson("Charles", "Jacobi", Reproduction.Male, point, Color.White, 40f, noSkills: false, expedition);
		AddFinishedStructure("structure:spikeTrap", new Point(14, 6), expedition, flipHorizontally: false, null, "constructSpikeTrap");
		for (int j = 0; j < 1; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spikeTrap"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), point);
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(7, 7));
	}

	public static void CarcassNoMeat()
	{
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(0, 15)));
		The.MapUI.ZoomToMapPosition(10, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
		for (int i = 0; i < 1; i++)
		{
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(10, 6), 8f, allegiance).BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
		}
		for (int j = 0; j < 1; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"])
			{
				Bulk = 0.35f
			}, new Point(6, 6));
		}
		AddFinishedStructure("structure:sensor", new Point(8, 7), owner);
	}

	public static void ShaiHuludTest()
	{
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
		PlaceAnimal("entity:whipjaw", Reproduction.Male, new Point(19, 15), 20f, allegiance);
		PlaceAnimal("entity:whipjaw", Reproduction.Female, new Point(19, 17), 20f, allegiance);
	}

	public static void ScareCrowTest()
	{
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(45, 45);
		AddFinishedStructure("structure:scarecrow", new Point(46, 44), owner);
	}

	public static void ReloadTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		The.MapUI.ZoomToMapPosition(5, 5);
		PlacePerson("Jamy", "Hassert", Reproduction.Male, new Point(5, 5), Color.MediumSeaGreen, 35f, noSkills: false, expedition);
		Entity entity = new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]);
		AddColonyItem(entity, new Point(5, 5));
		entity.Item.Ammunition.NoOfRounds = 1;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), new Point(5, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), new Point(5, 5));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);
		PlaceAnimal("entity:patrician", Reproduction.Male, new Point(8, 8), 20f, allegiance);
	}

	public static void RobotTest()
	{
		Point point = new Point(7, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(5, 5);
		PlacePerson("Jamy", "Hassert2", Reproduction.Male, new Point(7, 6), Color.MediumSeaGreen, 35f, noSkills: false, expedition);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["advanced"], RatingTypes.Food);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:farmingHoe"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:glassyCreeperPods"]), point);
		PlaceRobot("entity:diggingRobot", new Point(6, 8), The.Sim.PlaySite.PlayerAllegiance, expedition);
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
		AddFinishedStructure("structure:rareMetalOrePit3", new Point(10, 12), expedition);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 7)) + new Vector3(0f, 24f, 0f));
	}

	public static void ItemsNotCarriedBugHunt()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		The.MapUI.ZoomToMapPosition(5, 5);
		for (int i = 0; i < 4; i++)
		{
			GetBob(new Point(6, 6 + i), expedition);
		}
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
		AddFinishedStructure("structure:campfire", new Point(7, 7), expedition);
		for (int j = 0; j < 10; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"])
			{
				Bulk = 1f
			}, new Point(6, 6));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
	}

	public static void BenjaminsTest()
	{
		Point point = new Point(13, 48);
		Point pos = new Point(15, 44);
		Point pos2 = new Point(13, 44);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Camp", "Camp", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(6, 6));
		Entity entity = new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicArrow"]);
		AddColonyItem(entity, new Point(6, 6));
		entity.Item.Ammunition.NoOfRounds = 1;
		for (int i = 0; i < 8; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), point);
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), point);
		}
		for (int j = 0; j < 20; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), point);
		}
		for (int k = 0; k < 21; k++)
		{
			if (k != 9 && k != 10 && k != 17)
			{
				AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(528 + k * 16, 2304f, 0f));
			}
		}
		AddFinishedStructure("structure:satelliteGroundStation", null, expedition, flipHorizontally: false, new Vector3(624f, 2448f, 0f));
		GetMinion(point, expedition, "Bob", " ");
		GetMinion(point, expedition, "Peter", " ");
		GetMinion(point, expedition, "Luke", " ");
		GetMinion(point, expedition, "Kurt", " ");
		GetMinion(point, expedition, "Picard", " ");
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);
		for (int l = 0; l < 3; l++)
		{
			PlaceAnimal("entity:twinkler", Reproduction.Male, pos, 20f, allegiance);
			PlaceAnimal("entity:twinkler", Reproduction.Male, pos, 20f, allegiance);
			PlaceAnimal("entity:patrician", Reproduction.Male, pos2, 20f, allegiance2);
		}
	}

	public static Entity GetMinion(Point pos, Expedition exp, string firstName, string sirName)
	{
		Entity entity = PlacePerson(firstName, sirName, Reproduction.Male, pos, Color.Purple, 30f, noSkills: false, exp);
		entity.PersonEntity.UpdatePortrait(The.InGameUI.gui, "skimmerDark");
		foreach (SkillType value in GameData.Instance.AllSkillTypes.Values)
		{
			entity.Intelligence.Skills.Add(value, new Skill(0.5f, value));
		}
		return entity;
	}

	public static void HuntTest()
	{
		Point point = new Point(10, 10);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(5, 10);
		GetBob(point, exp);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
		for (int i = 0; i < 8; i++)
		{
			ImmobilizeEntity(PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(5 + i * 3, 15), 20f, allegiance));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]), point);
	}

	public static void MemoryTest()
	{
		Point tile = new Point(30, 10);
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(tile));
		The.MapUI.ZoomToMapPosition(tile.X, tile.Y);
		for (int i = 0; i < 20; i++)
		{
			Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
			PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(tile.X + i, tile.Y), 20f, allegiance);
			Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
			PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(tile.X + i, tile.Y), 20f, allegiance2);
		}
	}

	public static void ManyAgentsTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		The.MapUI.ZoomToMapPosition(5, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:pygmyThunderChicken"]);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		PlaceAnimal("entity:pygmyThunderChicken", Reproduction.Male, new Point(30, 11), 20f, allegiance);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		GetBob(new Point(8, 9), expedition);
		for (int i = 0; i < 40; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(6, 6));
		}
		for (int j = 0; j < 20; j++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(6, 6));
		}
		for (int k = 0; k < 50; k++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:mashedCommonOilTubers"]), new Point(6, 6));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedFlintSpear"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedCookingPot"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:boltActionRifle"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:corditeAmmo"]), new Point(6, 6));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 484f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 500f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 564f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 580f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 516f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 532f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 548f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 484f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 500f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 564f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 580f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1134f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1118f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1102f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1086f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1070f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1054f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1038f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1022f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1006f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 468f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1150f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1134f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1118f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1102f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1086f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1070f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1054f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1038f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1022f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(1006f, 596f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, new Vector3(990f, 596f, 0f));
		AddFinishedStructure("structure:octagonalTent", new Point(4, 5), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(6, 5), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(8, 5), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(4, 3), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(6, 3), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(8, 3), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(10, 3), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(10, 5), expedition);
		AddFinishedStructure("structure:cooledFoodCache", new Point(12, 5), expedition);
		AddFinishedStructure("structure:cooledFoodCache", new Point(14, 5), expedition);
		AddFinishedStructure("structure:campfire", new Point(7, 7), expedition);
	}

	public static void AmbientSoundTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(20, 10)));
		The.MapUI.ZoomToMapPosition(20, 20);
		PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(20, 10), Color.White, 52f, noSkills: false, expedition);
		PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(20, 11), Color.White, 52f, noSkills: false, expedition);
	}

	public static void AllItemsTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(15, 5);
		PlacePerson("Byggemand", "Bob", Reproduction.Male, new Point(15, 5), Color.White, 52f, noSkills: false, expedition);
		int num = 4;
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			try
			{
				for (int i = 0; i < num; i++)
				{
					if (allItemType.Value.ItemType.HasNoMaximumBulk)
					{
						AddColonyItem(new Entity(allItemType.Value)
						{
							Bulk = 1f
						}, new Point(15, 5));
					}
					else
					{
						AddColonyItem(new Entity(allItemType.Value), new Point(15, 5));
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	public static void SoundTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(45, 45);
		Entity entity = PlacePerson("Bobby", "Bob", Reproduction.Male, new Point(49, 41), Color.White, 52f, noSkills: false, expedition);
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(49, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(49, 40));
		entity.Find<BodyComponent>(out var _);
		entity.Body.ChangeMaxHitpoints(450f);
	}

	public static void DemonTreeTest()
	{
		Point point = new Point(15, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:spoakDendront"]);
		Entity entity = PlaceAnimal("entity:spoakDendront", Reproduction.Male, new Point(15, 8), 20f, allegiance);
		entity.Find<BodyComponent>(out var _);
		entity.Body.ChangeMaxHitpoints(450f);
		PlacePerson("Bobby", "Bob", Reproduction.Male, point, Color.White, 52f, noSkills: false, expedition);
		PlacePerson("Bobby", "Bob", Reproduction.Male, point, Color.White, 52f, noSkills: false, expedition);
		PlacePerson("Bobby", "Bob", Reproduction.Male, point, Color.White, 52f, noSkills: false, expedition);
		PlacePerson("Bobby", "Bob", Reproduction.Male, point, Color.White, 52f, noSkills: false, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), point);
	}

	public static void ButcherTest()
	{
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));
		The.MapUI.ZoomToMapPosition(15, 15);
		GetBob(new Point(10, 15), exp);
		GetBob(new Point(1, 2), exp);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(10, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(10, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCracker"]), new Point(10, 15));
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"])
			{
				Bulk = 4f
			}, new Point(15, 15));
		}
	}

	public static void SpikePlantTest()
	{
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(45, 45);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(49, 40));
	}

	public static void WorldMapTest()
	{
		Point point = new Point(8, 22);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		GetBob(point, exp);
		The.InGameUI.WorldMapDialog.ShowInScreenSpace(400, 100);
	}

	private static void AddResource(Point pos, string resource, int amount = 4)
	{
		The.Map.GetTile(pos).AddResource(resource, amount);
	}

	public static void ClayPitTest()
	{
		Point point = new Point(8, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		The.Map.GetTile(point).AddResource("clay", 4);
		The.Map.GetTile(new Point(5, 10)).AddResource("clay", 4);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			GetBob(point, expedition);
		}
		foreach (KeyValuePair<string, int> item in new Dictionary<string, int>
		{
			{ "advancedString", 2 },
			{ "steelSpade", 2 },
			{ "sticks", 4 },
			{ "crystalBerries", 2 },
			{ "clayJar", 1 },
			{ "steelKnife", 1 }
		})
		{
			for (int j = 0; j < item.Value; j++)
			{
				AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + item.Key]), point);
			}
		}
		The.Map.GetTile(point).AddResource("vine", 8);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:clayDeposit"]), MapManager.TileToWorldPos(new Point(11, 9)) + new Vector3(0f, 24f, 0f));
		AddFinishedStructure("structure:improvisedKitchen", new Point(point.X - 2, point.Y + 1), expedition);
	}

	public static void NanomapTest()
	{
		Point tile = new Point(7, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(tile));
		The.MapUI.ZoomToMapPosition(tile.X, tile.Y);
		GetBob(new Point(6, 5), expedition);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
	}

	public static void FarmTest()
	{
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
		Point point = new Point(8, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		AddFinishedStructure("structure:sensor", point, expedition);
		AddFinishedStructure("structure:greenhouse", new Point(4, 10), expedition, flipHorizontally: false, null, "constructGreenhouse");
		AddFinishedStructure("structure:improvisedGreenhouse", new Point(4, 8), expedition, flipHorizontally: false, null, "constructImprovisedGreenhouse");
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		The.Map.GetTile(new Point(8, 7)).AddResource("glassyCreeperPods", 4);
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			GetBob(point, expedition);
		}
		foreach (KeyValuePair<string, int> item in new Dictionary<string, int>
		{
			{ "item:farmingHoe", num },
			{ "item:glassyCreeperPods", 12 },
			{ "item:crystalBerries", 12 },
			{ "item:advancedMachete", 2 },
			{ "item:guano", 2 },
			{ "item:fingerFruit", 8 },
			{ "item:advancedString", 3 },
			{ "item:cotton", 3 },
			{ "item:steelKnife", 3 },
			{ "item:improvisedGreenHouseCover", 15 },
			{ "item:shadeleafCanes", 9 },
			{ "item:improvisedSpade", 3 },
			{ "item:astroRation", 4 }
		})
		{
			for (int j = 0; j < item.Value; j++)
			{
				AddColonyItem(new Entity(GameData.Instance.AllEntityTypes[item.Key]), point);
			}
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:organicMatter"])
		{
			Bulk = 2.5f
		}, point);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 9)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 5)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 7)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(11, 9)) + new Vector3(0f, 24f, 0f));
	}

	public static void LongTermFarmingTest()
	{
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFarmingProperties");
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");
		Point point = new Point(45, 26);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Camp", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(39, 19)));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(45, 19)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(44, 29));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), new Point(49, 28));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCreek"]), new Point(48, 18));
		foreach (KeyValuePair<string, int> item in new Dictionary<string, int>
		{
			{ "farmingHoe", 2 },
			{ "sticks", 36 },
			{ "waterCaneStem", 6 },
			{ "paracord", 6 },
			{ "glassyCreeperPods", 9 },
			{ "cotton", 9 },
			{ "crystalBerries", 9 },
			{ "machete", 2 },
			{ "organicMatter", 2 }
		})
		{
			for (int i = 0; i < item.Value; i++)
			{
				AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:" + item.Key]), point);
			}
		}
	}

	public static void PlotAssetTest()
	{
		The.Sim.PlaySite.EventManager.AddPolledEvent("initDebugStuff");
		The.Sim.PlaySite.EventManager.AddPolledEvent("debugCounter");
		Point point = new Point(45, 26);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Camp", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		GetBob(point, expedition);
		AddFinishedStructure("structure:smallPlot", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(43, 26)) + new Vector3(0f, 24f, 0f));
	}

	public static void BushDragonParticleTest()
	{
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(10, 10);
	}

	public static void FishTrapTest()
	{
		The.Sim.PlaySite.EventManager.AddPolledEvent("initializeGlobalFishTrapProperties");
		Point point = new Point(18, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		GetBob(point, expedition);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCreek"]), new Point(14, 10));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotCoast"]), new Point(14, 14));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:fishTrapSpotShore"]), new Point(8, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapHoopNet"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishTrapBasket"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingNet"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingNet"]), point);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["medium"], RatingTypes.Food);
	}

	public static void PierTest()
	{
		Point point = new Point(15, 12);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		GetBob(point, expedition);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:pierSpot"]), new Point(point.X + 2, point.Y));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranchesTrimmed"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakShingles"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:solidMudBrick"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
	}

	public static void CollisionTest()
	{
		The.MapUI.ZoomToMapPosition(10, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 9)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(15, 9));
		for (int i = 0; i < 100; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), new Point(15, 9));
		}
		for (int j = 0; j < 4; j++)
		{
			Entity entity = PlacePerson("Collisioneer", " " + j, Reproduction.Male, new Point(15 + j, 9), Color.White, 52f, noSkills: false, "blue1", expedition);
			entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
			entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
			entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
			entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		}
		AddFinishedStructure("structure:abatis", new Point(15, 7), expedition);
		AddFinishedStructure("structure:abatis", new Point(10, 10), expedition);
		AddFinishedStructure("structure:abatis", new Point(11, 10), expedition);
		AddFinishedStructure("structure:abatis", new Point(12, 10), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(12, 10)) + new Vector3(16f, 0f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(12, 10)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(13, 10), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(13, 10)) + new Vector3(16f, 0f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(13, 10)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(14, 10), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(14, 10)) + new Vector3(16f, 0f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(14, 10)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(15, 10), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(15, 10)) + new Vector3(16f, 0f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(15, 10)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:campfire", new Point(35, 11), expedition);
		AddFinishedStructure("structure:campfirePotCrane", new Point(39, 13), expedition);
		AddFinishedStructure("structure:lean-toTarp", new Point(30, 12), expedition);
		AddFinishedStructure("structure:lean-toSpoakLeaves", new Point(30, 15), expedition);
		AddFinishedStructure("structure:A-frameSpoakLeaves", new Point(36, 15), expedition);
		AddFinishedStructure("structure:A-frameScraps", new Point(39, 15), expedition);
		AddFinishedStructure("structure:A-frameTarp", new Point(32, 17), expedition);
		AddFinishedStructure("structure:lean-toScraps", new Point(31, 19), expedition);
		AddFinishedStructure("structure:domeShelterTarp", new Point(34, 19), expedition);
		AddFinishedStructure("structure:domeShelterSpoakShingles", new Point(38, 19), expedition);
		AddFinishedStructure("structure:wigwamSpoakShingles", new Point(30, 23), expedition);
		AddFinishedStructure("structure:daysheenTipi", new Point(28, 23), expedition);
		AddFinishedStructure("structure:skimmerHull", new Point(33, 22), expedition);
		AddFinishedStructure("structure:skimmerTail", new Point(34, 24), expedition);
		AddFinishedStructure("structure:skimmerEngineSide", new Point(28, 25), expedition);
		AddFinishedStructure("structure:storageHole", new Point(30, 25), expedition);
		AddFinishedStructure("structure:smokeOven", new Point(32, 25), expedition);
		AddFinishedStructure("structure:abatis", new Point(34, 26), expedition);
		AddFinishedStructure("structure:abatis", new Point(35, 26), expedition);
		AddFinishedStructure("structure:skimmerHull", new Point(8, 2), expedition);
		AddFinishedStructure("structure:skimmerTail", new Point(6, 4), expedition);
		AddFinishedStructure("structure:skimmerEngineSide", new Point(9, 2), expedition);
		AddFinishedStructure("structure:storageHole", new Point(8, 4), expedition);
		AddFinishedStructure("structure:smokeOven", new Point(9, 3), expedition);
		AddFinishedStructure("structure:lean-toTarp", new Point(10, 6), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(6, 7));
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
	}

	private static bool CheckValidEntity(Entity entity)
	{
		if (entity.Bulk == 0f)
		{
			throw new Exception("Bulk has not been set on entity");
		}
		return true;
	}

	public static Entity AddColonyItemUpgrade(string type, Entity container, UpgradeCategory upgradeCategory, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
	{
		Entity entity = new Entity(GameData.Instance.AllEntityTypes[type]);
		entity.Initialize(The.Sim.PlaySite);
		entity.InitializeModelAndOnScreenFunctionality();
		CheckValidEntity(entity);
		entity.PlaceEntityOnPlaySite(null, container, null, new Entity.SetOwnerInfo(The.Sim.PlaySite.GetFirstPlayerExpedition(), giveNewOwnerInfo), null, null, null, isProductionOutput: false, null, assertContainment: true, simulateJoinedExpeditionNow: true, upgradeCategory);
		return entity;
	}

	public static Entity AddColonyItem(Entity item, Vector3 pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
	{
		item.Initialize(The.Sim.PlaySite);
		item.InitializeModelAndOnScreenFunctionality();
		CheckValidEntity(item);
		item.PlaceEntityOnPlaySite(pos, null, null, new Entity.SetOwnerInfo(The.Sim.PlaySite.GetFirstPlayerExpedition(), giveNewOwnerInfo));
		return item;
	}

	private static Entity AddColonyItem(string type, Point pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
	{
		return AddColonyItem(new Entity(GameData.Instance.AllEntityTypes[type]), MapManager.TileToWorldPos(pos), giveNewOwnerInfo);
	}

	private static Entity AddColonyItem(Entity item, Point pos, Entity.GiveNewOwnerKnowledge giveNewOwnerInfo = Entity.GiveNewOwnerKnowledge.Yes)
	{
		return AddColonyItem(item, MapManager.TileToWorldPos(pos), giveNewOwnerInfo);
	}

	private static void AddNoOwnerItem(Entity item, Point pos)
	{
		item.Initialize(The.Sim.PlaySite);
		item.InitializeModelAndOnScreenFunctionality();
		CheckValidEntity(item);
		item.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos), null, null, null);
	}

	private static void AddTerrainItem(Entity item, Point pos)
	{
		AddTerrainItem(item, MapManager.TileToWorldPos(pos));
	}

	private static void AddTerrainItem(Entity item, Vector3 pos)
	{
		item.Initialize(The.Sim.PlaySite);
		item.InitializeModelAndOnScreenFunctionality();
		item.PlaceEntityOnPlaySite(pos, null, null, null);
	}

	private static void AddColonyItemToStorage(Entity item, Entity container, StorageCompartment? compartment = null)
	{
		item.Initialize(The.Sim.PlaySite);
		item.InitializeModelAndOnScreenFunctionality();
		CheckValidEntity(item);
		if (!container.Contains.AddToContain(item, compartment))
		{
			throw new Exception();
		}
		IOwner firstPlayerExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
		item.ChangeOwnership(firstPlayerExpedition);
	}

	public static Entity PlaceRobot(string entityKey, Point pos, Allegiance allegiance, Expedition expedition)
	{
		AllegianceAndExpedition allegianceAndExpedition = null;
		AllegianceAndExpedition allegianceAndExpedition2 = null;
		allegianceAndExpedition = new AllegianceAndExpedition
		{
			AllegianceKey = allegiance.KeyName,
			ExpeditionKey = expedition.KeyName
		};
		allegianceAndExpedition2 = new AllegianceAndExpedition
		{
			AllegianceKey = allegiance.KeyName,
			ExpeditionKey = expedition.KeyName
		};
		bool placementFailed;
		Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(new EntityData
		{
			EntityKey = entityKey,
			Location = MapManager.TileToWorldPos(pos),
			MemberOf = allegianceAndExpedition2,
			OwnedBy = allegianceAndExpedition
		}, out placementFailed);
		if (entity != null)
		{
			TestEntityNotPlacedOnblockedTerrain(entity);
		}
		return entity;
	}

	public static Entity PlaceAnimal(string entityKey, Reproduction? sex, Point pos, float age, Allegiance allegiance, string raceKey = null, Expedition ownerExpedition = null)
	{
		AllegianceAndExpedition ownedBy = null;
		AllegianceAndExpedition allegianceAndExpedition = null;
		if (ownerExpedition != null)
		{
			ownedBy = new AllegianceAndExpedition
			{
				AllegianceKey = allegiance.KeyName,
				ExpeditionKey = ownerExpedition.KeyName
			};
			allegianceAndExpedition = new AllegianceAndExpedition
			{
				AllegianceKey = allegiance.KeyName,
				ExpeditionKey = ownerExpedition.KeyName
			};
		}
		else
		{
			allegianceAndExpedition = new AllegianceAndExpedition();
			if (allegiance != null)
			{
				allegianceAndExpedition.AllegianceKey = allegiance.KeyName;
			}
		}
		string casteKey = null;
		if (sex.HasValue)
		{
			CasteType casteType = GameData.Instance.AllEntityTypes[entityKey].BiologicalType.Castes.FirstOrDefault((CasteType c) => c.Reproduction == sex.Value);
			if (casteType != null)
			{
				casteKey = casteType.KeyName;
			}
		}
		bool placementFailed;
		Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(new EntityData
		{
			EntityKey = entityKey,
			Location = MapManager.TileToWorldPos(pos),
			BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
			{
				AgeInYears = new NormalDistribution
				{
					Mean = age
				},
				RaceKey = raceKey,
				CasteKey = casteKey
			},
			MemberOf = allegianceAndExpedition,
			OwnedBy = ownedBy
		}, out placementFailed);
		if (entity != null)
		{
			TestEntityNotPlacedOnblockedTerrain(entity);
		}
		return entity;
	}

	public static void PlaceGameEntitiesMilestoneBuild()
	{
		The.MapUI.ZoomToMapPosition(215, 95);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(216, 95)));
		GameData.Instance.AllEntityTypes["entity:human"].SensorType.Range = 1200f;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 100));
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(215, 95), Color.White, 52f, noSkills: false, "blue1", expedition);
		PlacePerson("Josie", "Kane", Reproduction.Female, new Point(214, 96), Color.White, 52f, noSkills: false, "blue2", expedition);
		AddFinishedStructure("structure:abatis", new Point(218, 97), expedition);
		AddFinishedStructure("structure:abatis", new Point(218, 98), expedition);
		AddFinishedStructure("structure:abatis", new Point(213, 100), expedition);
		AddFinishedStructure("structure:abatis", new Point(214, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(214, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(215, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(215, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(216, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(216, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(217, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(217, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(218, 100), expedition);
		AddFinishedStructure("structure:A-frameTarp", new Point(217, 95), expedition);
		AddFinishedStructure("structure:skimmerHull", new Point(215, 98), expedition);
		AddFinishedStructure("structure:skimmerTail", new Point(213, 98), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(213, 95));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(215, 94));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(213, 95));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(214, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(216, 97));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(223, 88), 20f, allegiance);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(218, 84), 20f, allegiance);
	}

	public static void PlaceGameEntitiesTestMap()
	{
		The.MapUI.ZoomToMapPosition(215, 95);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		GameData.Instance.AllEntityTypes["entity:human"].SensorType.Range = 1200f;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 100));
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(215, 95), Color.White, 52f, noSkills: false, "blue1", expedition);
		PlacePerson("Josie", "Kane", Reproduction.Female, new Point(214, 95), Color.White, 52f, noSkills: false, "blue2", expedition);
		PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(216, 94), Color.White, 52f, noSkills: false, "blue2", expedition);
		AddFinishedStructure("structure:abatis", new Point(218, 97), expedition);
		AddFinishedStructure("structure:abatis", new Point(218, 98), expedition);
		AddFinishedStructure("structure:abatis", new Point(213, 100), expedition);
		AddFinishedStructure("structure:abatis", new Point(214, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(214, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(215, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(215, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(216, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(216, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(217, 100), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(217, 100)) + new Vector3(24f, 0f, 0f));
		AddFinishedStructure("structure:abatis", new Point(218, 100), expedition);
		AddFinishedStructure("structure:A-frameTarp", new Point(217, 95), expedition);
		AddFinishedStructure("structure:skimmerHull", new Point(211, 98), expedition);
		AddFinishedStructure("structure:skimmerTail", new Point(210, 99), expedition);
		AddFinishedStructure("structure:storageHole", new Point(214, 96), expedition);
		AddFinishedStructure("structure:smokeOven", new Point(217, 96), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(213, 95));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(215, 94));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(213, 95));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(215, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(216, 96));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(214, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(216, 97));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(216, 97));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(221, 93), 20f, allegiance);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(218, 84), 20f, allegiance);
		Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(214, 102), 20f, allegiance2);
		PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(215, 102), 20f, allegiance2);
		PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(216, 101), 20f, allegiance2);
	}

	public static void TestMicroMap()
	{
		The.MapUI.ZoomToMapPosition(10, 10);
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(15, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(15, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(16, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(16, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(14, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(14, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(14, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(15, 13));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(15, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(14, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(14, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(14, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]), new Point(16, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:superconductingWire"]), new Point(16, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(17, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:strongBugNet"]), new Point(15, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(16, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(15, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(15, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonPoison"]), new Point(16, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 10));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(15, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(14, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"]), new Point(14, 9));
	}

	public static void QuarterSizeMap()
	{
		The.MapUI.ZoomToMapPosition(35, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		bool noSkills = true;
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;
		entity.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, entity);
		Entity entity2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;
		entity2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");
		entity2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity3 = PlacePerson("Joaquin", "Lehner", Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);
		entity3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;
		entity3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");
		entity3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);
		entity4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f;
		entity4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		entity4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");
		entity4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity5 = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition);
		entity5.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), entity5);
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)));
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
	}

	public static void DemoIslandMap()
	{
		The.Sim.DateAndTime.ResetTimeOfYearAndTimeOfDay(new DateAndTime.TimeDateYear
		{
			Year = 0,
			Day = 1,
			TimeOfDay = 0.36
		});
		The.MapUI.ZoomToMapPosition(16, 51);
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Entity entity = AddFinishedStructure("structure:skimmerHull", new Point(13, 51), owner);
		entity.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, owner, flipHorizontally: false, MapManager.TileToWorldPos(new Point(12, 50)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, owner, flipHorizontally: false, MapManager.TileToWorldPos(new Point(14, 52)) + new Vector3(-16f, -16f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(11, 51), owner);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"]), new Point(16, 51));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(16, 51));
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]), entity);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), entity);
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(20, 49)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(18, 43)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(19, 46)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(44, 28)), null, 0.5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(39, 27)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 16)), 8f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(11, 43)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(15, 37)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(10, 33)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(9, 29)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(14, 27)), 1f, 4f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(55, 17)), 1f, 3f);
	}

	public static void QuarterSizeMapBuildingTest()
	{
		The.MapUI.ZoomToMapPosition(35, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		bool noSkills = true;
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;
		entity.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, entity);
		Entity entity2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;
		entity2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");
		entity2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity3 = PlacePerson("Joaquin", "Lehner", Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);
		entity3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;
		entity3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");
		entity3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);
		entity4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f;
		entity4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		entity4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");
		entity4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity5 = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition);
		entity5.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition);
		AddFinishedStructure("structure:lean-toTarp", null, expedition, flipHorizontally: false, new Vector3(1913f, 1167f, 0f));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), entity5);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:marshcotSap"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(38, 25));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)));
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
	}

	public static void QuarterSizeMap_Alt_Test_v1()
	{
		The.MapUI.ZoomToMapPosition(35, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		bool noSkills = true;
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "grey1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;
		entity.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_w_m_adult_1");
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, entity);
		Entity entity2 = PlacePerson("Augustine", "Yeboah", Reproduction.Female, new Point(37, 24), Color.White, 39f, noSkills, "blue2", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;
		entity2.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_b_f_adult_1");
		entity2.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.6f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.7f, GameData.Instance.AllSkillTypes["butchering"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.6f, GameData.Instance.AllSkillTypes["foraging"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.6f, GameData.Instance.AllSkillTypes["cooking"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.6f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.6f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.7f, GameData.Instance.AllSkillTypes["medicine"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.5f, GameData.Instance.AllSkillTypes["psychology"]));
		entity2.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(1f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity3 = PlacePerson("Joaquin", "Lehner", Reproduction.Male, new Point(37, 26), Color.White, 44f, noSkills, "green2", expedition);
		entity3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;
		entity3.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_h_m_adult_1");
		entity3.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.4f, GameData.Instance.AllSkillTypes["butchering"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
		entity3.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity4 = PlacePerson("Ilya", "Khan", Reproduction.Male, new Point(35, 23), Color.White, 38f, noSkills, "red1", expedition);
		entity4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.9f;
		entity4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		entity4.PersonEntity.UpdatePortrait(The.InGameUI.gui, "human_a_m_adult_1");
		entity4.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.4f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.6f, GameData.Instance.AllSkillTypes["butchering"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.9f, GameData.Instance.AllSkillTypes["cooking"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.6f, GameData.Instance.AllSkillTypes["menial"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.3f, GameData.Instance.AllSkillTypes["medicine"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["psychology"]));
		entity4.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
		Entity entity5 = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition);
		entity5.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), entity5);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:commonOilTubers"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:streakFin"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedChitinousArrow"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvedFireExtinguisher"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(38, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(38, 25));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)));
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
	}

	public static void AnimTweak()
	{
		The.MapUI.ZoomToMapPosition(35, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		bool noSkills = true;
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills, "blue1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.4f;
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		The.Sim.ExploreShroud(new TilePos(35, 25), new TilePos(0, 70), 9, 16, entity);
		Entity entity2 = AddFinishedStructure("structure:skimmerHull", new Point(35, 25), expedition);
		entity2.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(34, 24)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(36, 26)) + new Vector3(-16f, -16f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(33, 25), expedition);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity2);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:emptyCartridge"]), entity2);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity2);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity2);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), entity2);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:inactivatedFoodCoolerUnit"]), entity2);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedMetalArrow"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(34, 25));
		The.Client.ParticleManager.AddEmitter("signalSmoke", MapManager.TileToWorldPosVector2(new Point(35, 24)) + new Vector2(-5f, 5f), 0.5f);
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(31, 21)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 22)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(32, 21)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(7, 63)), 2f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 61)), 1.3f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(3, 61)), 1.7f, 0.4f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 60)));
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(21, 60)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(7, 61)), 5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(3, 59)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(43, 2)), 8f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(6, 7)), 5f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(5, 15)), 10f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(43, 22)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(54, 7)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(53, 11)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(48, 10)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(25, 44)), 1f, 4f);
	}

	public static void ModelTest()
	{
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(10, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:skinnedtest"]);
		PlaceAnimal("entity:skinnedtest", Reproduction.Male, new Point(10, 5), 20f, allegiance);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:meshTest"]), new Point(9, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"])
		{
			Bulk = 1f
		}, new Point(9, 7));
		AddFinishedStructure("structure:sensor", new Point(8, 7), owner);
	}

	public static void SleepInShiftsTest()
	{
		The.MapUI.ZoomToMapPosition(15, 10);
		The.Sim.DateAndTime.TimeOfDay = 0.3;
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		expedition.Policy.IndependentsAllowedToSleep = 5;
		Entity entity = PlacePerson("Sleepiest", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, noSkills: false, "blue1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;
	}

	public static void FindPreyTest()
	{
		The.MapUI.ZoomToMapPosition(10, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(15, 8));
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(15, 9), Color.White, 52f, noSkills: false, "blue1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		Entity entity2 = PlacePerson("The Hillismo", "Guy", Reproduction.Male, new Point(15, 9), Color.White, 52f, noSkills: false, "blue1", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(15, 9));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), new Point(15, 10));
	}

	public static void MapApril2013()
	{
		The.MapUI.ZoomToMapPosition(55, 40);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(57, 40), Color.White, 52f, noSkills: false, "blue1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.6f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.7f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.6f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.8f;
		Entity entity2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(58, 40), Color.White, 52f, noSkills: false, "blue2", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.5f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.4f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.3f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.6f;
		Entity entity3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(59, 41), Color.White, 52f, noSkills: false, "grey1", expedition);
		entity3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.7f;
		entity3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.8f;
		entity3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.4f;
		entity3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.5f;
		Entity entity4 = PlacePerson("Glen", "Tarkov", Reproduction.Male, new Point(60, 41), Color.White, 52f, noSkills: false, "red1", expedition);
		entity4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.5f;
		entity4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.4f;
		entity4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.5f;
		entity4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0.3f;
		Entity entity5 = AddFinishedStructure("structure:skimmerHull", new Point(55, 41), expedition);
		entity5.Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(54, 40)) + new Vector3(32f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(56, 42)) + new Vector3(-16f, -20f, 0f));
		AddFinishedStructure("structure:skimmerTail", new Point(53, 41), expedition);
		_ = The.Client;
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(61, 42)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(63, 43)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(64, 42)));
		The.Client.ParticleManager.AddEmitter("smallFog", MapManager.TileToWorldPosVector2(new Point(65, 41)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(29, 121)));
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(13, 122)), 2f, 0.5f);
		The.Client.ParticleManager.AddEmitter("sulphurousSmoke", MapManager.TileToWorldPosVector2(new Point(1, 122)), 3f, 0.5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(10, 111)), 4f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(13, 115)), 5f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(20, 120)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(30, 118)), 8f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(30, 105)), 10f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(42, 122)), 8f);
		The.Client.ParticleManager.AddEmitter("haze", MapManager.TileToWorldPosVector2(new Point(79, 17)), 10f);
		AddTerrainItem(new Entity(GameData.Instance.AllTerrainFeatureTypes["terrain:groundFog"]), new Point(32, 39));
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(48, 9)), 5f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(15, 20)), 10f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(30, 18)), 8f, 2f);
		The.Client.ParticleManager.AddEmitter("fog", MapManager.TileToWorldPosVector2(new Point(20, 36)), 7f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(67, 37)), 1f, 3f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(95, 24)), 1f, 7f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 15)), 1f, 11f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(109, 27)), 1f, 15f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 33)), 1f, 4f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(108, 52)), 1f, 1f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(102, 16)), 1f, 6f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 20)), 1f, 8f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(105, 28)), 1f, 1f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(98, 35)), 1f, 2f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(103, 37)), 1f, 4f);
		The.Client.ParticleManager.AddEmitter("pollen", MapManager.TileToWorldPosVector2(new Point(114, 14)), 1f, 2f);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:basicFireExtinguisher"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]), entity5);
		AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:inactivatedFoodCoolerUnit"]), entity5);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedGoodSpear"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBow"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(34, 25));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(55, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(54, 40));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(55, 39));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fireSuppressantCartridge"]), new Point(55, 39));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"])
		{
			Bulk = 1f
		}, new Point(57, 41));
	}

	public static void WildernessCampScreenshot()
	{
		The.MapUI.ZoomToMapPosition(105, 59);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(104, 60));
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(104, 59), Color.White, 52f, noSkills: false, "blue1", expedition);
		entity.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 8f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		Entity entity2 = PlacePerson("Josie", "Kane", Reproduction.Female, new Point(103, 61), Color.White, 52f, noSkills: false, "blue2", expedition);
		entity2.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		Entity entity3 = PlacePerson("Kurt", "Mansell", Reproduction.Male, new Point(105, 60), Color.White, 52f, noSkills: false, "grey1", expedition);
		entity3.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity3.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity3.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity3.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		Entity entity4 = PlacePerson("Glen", "Tarkov", Reproduction.Male, new Point(106, 59), Color.White, 52f, noSkills: false, "red1", expedition);
		entity4.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 1f;
		entity4.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 1f;
		entity4.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 1f;
		entity4.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(101, 58)) + new Vector3(-18f, 18f, 0f));
		AddFinishedStructure("structure:abatis", new Point(101, 58), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(101, 58)) + new Vector3(18f, -18f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(102, 57)) + new Vector3(0f, -18f, 0f));
		AddFinishedStructure("structure:abatis", new Point(102, 57), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(102, 57)) + new Vector3(-18f, 18f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(107, 61)) + new Vector3(-18f, 18f, 0f));
		AddFinishedStructure("structure:abatis", new Point(107, 61), expedition);
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(107, 61)) + new Vector3(18f, -18f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(107, 62)) + new Vector3(-18f, -18f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(109, 59)) + new Vector3(18f, -18f, 0f));
		AddFinishedStructure("structure:abatis", null, expedition, flipHorizontally: false, MapManager.TileToWorldPos(new Point(109, 59)) + new Vector3(18f, 0f, 0f));
		AddFinishedStructure("structure:lean-toScraps", new Point(105, 58), expedition);
		AddFinishedStructure("structure:lean-toTarp", new Point(107, 59), expedition);
		AddFinishedStructure("structure:wigwamSpoakShingles", new Point(103, 58), expedition);
		AddFinishedStructure("structure:smokeOven", new Point(103, 60), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:chainsaw"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:scrapMetal"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wingweedLeaves"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRod"]), new Point(104, 60));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(104, 60));
	}

	public static Entity AddFinishedStructure(string entityType, Point? pos, IOwner owner, bool flipHorizontally = false, Vector3? location = null, string constructionProcessToUse = null)
	{
		Entity entity = new Entity(GameData.Instance.AllStructureTypes[entityType]);
		entity.FlipHorizontally = flipHorizontally;
		entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
		entity.InitializeModelAndOnScreenFunctionality();
		if (pos.HasValue)
		{
			entity.PlaceEntityOnPlaySite(MapManager.TileToWorldPos(pos.Value), null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(owner));
		}
		else
		{
			entity.PlaceEntityOnPlaySite(location.Value, null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(owner));
		}
		entity.ComeOnline();
		if (constructionProcessToUse != null)
		{
			ProcessType processType = GameData.Instance.AllProcessTypes[constructionProcessToUse];
			Goal.FireEventActions(null, entity.ID, AgentActionHooks.CompletedProducing, processType.EventActions, AgentActionHooks.CompletedProducing, null);
		}
		return entity;
	}

	private static void AddRandomSkills(Entity entity, bool noSkills)
	{
		if (noSkills)
		{
			return;
		}
		foreach (KeyValuePair<string, SkillType> allSkillType in GameData.Instance.AllSkillTypes)
		{
			if (!entity.Intelligence.Skills.ContainsKey(allSkillType.Value))
			{
				Skill value = new Skill(Common.ClampTop((float)The.Sim.GameplayRandomGenerator.NextDouble("PlaceGameEntities") + 0.1f, 1f), allSkillType.Value);
				entity.Intelligence.Skills.Add(allSkillType.Value, value);
			}
		}
		entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["armedMelee"]].Value = 0.7f;
		entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["shooting"]].Value = 0.7f;
		entity.Intelligence.Skills[GameData.Instance.AllSkillTypes["unarmedFighting"]].Value = 0.7f;
	}

	private static void TestEntityNotPlacedOnblockedTerrain(Entity entity)
	{
		if (entity.Locomotor != null && The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(entity.PlaySiteLocation)))
		{
			_ = entity.EntityType.KeyName != "entity:bird";
		}
	}

	public static Entity GetBob(Point? pos, Expedition exp, Entity container = null, bool useRandomValues = false, float skillValue = 1f)
	{
		Entity entity = PlacePerson("Bob", "Bobson", Reproduction.Male, pos, Color.Purple, 30f, noSkills: false, exp, null, container);
		foreach (SkillType value2 in GameData.Instance.AllSkillTypes.Values)
		{
			float value = ((!useRandomValues) ? skillValue : ((float)The.Sim.GameplayRandomGenerator.NextDouble(null)));
			entity.Intelligence.Skills[value2] = new Skill(value, value2);
		}
		return entity;
	}

	private static Entity PlacePerson(string firstName, string lastName, Reproduction? sex, Point? pos, Color color, float age, bool noSkills, string raceKey, Expedition expedition, Entity container = null)
	{
		return PlacePerson(firstName, lastName, sex, pos, color, age, noSkills, expedition, raceKey, container);
	}

	private static Entity PlacePerson(string firstName, string lastName, Reproduction? sex, Point? pos, Color color, float age, bool noSkills, Expedition expedition, string raceKey = null, Entity container = null)
	{
		string text = "entity:human";
		AllegianceAndExpedition allegianceAndExpedition = null;
		allegianceAndExpedition = new AllegianceAndExpedition();
		allegianceAndExpedition.AllegianceKey = The.Sim.PlaySite.PlayerAllegiance.KeyName;
		if (expedition != null)
		{
			allegianceAndExpedition.ExpeditionKey = expedition.KeyName;
		}
		string casteKey = null;
		if (sex.HasValue)
		{
			CasteType casteType = GameData.Instance.AllEntityTypes[text].BiologicalType.Castes.FirstOrDefault((CasteType c) => c.Reproduction == sex.Value);
			if (casteType != null)
			{
				casteKey = casteType.KeyName;
			}
		}
		Vector3? location = null;
		if (pos.HasValue)
		{
			location = MapManager.TileToWorldPos(pos.Value);
		}
		bool placementFailed;
		Entity entity = MapLoader.CreateAndPlaceEntityFromEntityData(new EntityData
		{
			EntityKey = text,
			Location = location,
			BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
			{
				AgeInYears = new NormalDistribution
				{
					Mean = age
				},
				RaceKey = raceKey,
				CasteKey = casteKey
			},
			Person = new UWGame.SimSide.Maps.MapEditor.Person
			{
				FirstName = firstName,
				LastName = lastName
			},
			MemberOf = allegianceAndExpedition
		}, out placementFailed, container, null, offerForSale: false, isProductionOutput: false, null, null, null, allegianceAndExpedition.AllegianceKey, allegianceAndExpedition.ExpeditionKey);
		if (entity != null)
		{
			TestEntityNotPlacedOnblockedTerrain(entity);
			AddRandomSkills(entity, noSkills);
		}
		return entity;
	}

	public static void TwinklerEatTest()
	{
		The.Sim.DateAndTime.TimeOfDay = 0.0;
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(10, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		PlaceAnimal("entity:twinkler", Reproduction.Male, new Point(15, 8), 20f, allegiance);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"])
		{
			Bulk = 1f
		}, new Point(9, 7));
		AddFinishedStructure("structure:sensor", new Point(8, 7), owner);
	}

	public static void DogEatTest()
	{
		Point point = new Point(10, 10);
		Point point2 = new Point(10, 10);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point2));
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		PlaceAnimal("entity:dog", Reproduction.Male, point, 5f, The.Sim.PlaySite.PlayerAllegiance).BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
		GetBob(point, exp);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenCarcass"])
		{
			Bulk = 3.8f
		}, point2);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelKnife"]), point);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]);
	}

	public static void SkillTest()
	{
		Point point = new Point(24, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(15, 10);
		PlacePerson("Ib", "bi", Reproduction.Male, point, Color.White, 33f, noSkills: true, expedition, "grey1");
		GetBob(point, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:ironSpear"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
	}

	public static void EatTest()
	{
		Point point = new Point(10, 10);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(point);
		Entity bob = GetBob(point, exp, null, useRandomValues: true);
		GetBob(point, exp, null, useRandomValues: true);
		bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0.1f;
		bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.3f;
		bob.BiologicalEntity.Needs.NeedsList["micronutrients"].CurrentLevel = 0.1f;
		bob.BiologicalEntity.AddToStomachContents(-1f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:roastedCarbonTail"]), point + new Point(-3, 0));
	}

	public static void MidsizeTest()
	{
		Expedition exp = new Expedition(center: MapManager.TileToWorldPos(new Point(15, 10)), allegiance: The.Sim.PlaySite.PlayerAllegiance, keyName: "Start", name: "Start");
		The.MapUI.ZoomToMapPosition(15, 10);
		GetBob(new Point(6, 11), exp);
	}

	public static void ThreatTest()
	{
		Point point = new Point(15, 10);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.MapUI.ZoomToMapPosition(15, 10);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentryGunAmmo"]), point);
		GetBob(point, exp);
		Allegiance twinklerAllegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		CreateNest("terrain:quaditeNest", "North quadite nest", twinklerAllegiance).PlaceEntityOnPlaySite(new Vector3(50f, 100f, 0f), null, Entity.StructureState.Finished, null);
		Allegiance twinklerAllegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:fieldQuadite"]);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);
		CreateNest("terrain:fieldQuaditeNest", "Field quadite nest", twinklerAllegiance2).PlaceEntityOnPlaySite(new Vector3(250f, 250f, 0f), null, Entity.StructureState.Finished, null);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whipjaw"]);
		Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:whiteThunderChicken"]);
		for (int i = 0; i < 5; i++)
		{
			ImmobilizeEntity(PlaceAnimal("entity:whiteThunderChicken", Reproduction.Male, new Point(5, 5), 20f, allegiance2));
		}
		for (int j = 0; j < 10; j += 2)
		{
			for (int k = 10; k < 15; k += 2)
			{
				PlaceAnimal("entity:swarmer", Reproduction.Male, new Point(j + 1, k), 20f, allegiance);
			}
		}
	}

	private static Entity CreateNest(string entityType, string name, Allegiance twinklerAllegiance)
	{
		Entity entity = new Entity(GameData.Instance.AllEntityTypes[entityType]);
		entity.Initialize(The.Sim.PlaySite, twinklerAllegiance);
		entity.Name = name;
		return entity;
	}

	public static void AllStructuresBeingBuilt()
	{
		Expedition newOwner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 7)));
		The.MapUI.ZoomToMapPosition(10, 5);
		int num = 2;
		int num2 = 2;
		foreach (KeyValuePair<string, EntityType> allStructureType in GameData.Instance.AllStructureTypes)
		{
			EntityType value = allStructureType.Value;
			if (value != null && !value.KeyName.EndsWith("sentry") && !value.KeyName.Contains("sensor") && !value.StructureType.IsAddon && !value.StructureType.IsRoad)
			{
				Entity entity = new Entity(value);
				entity.PlaceEntityOnPlaySite(MapManager.TilePosToWorldPos(new TilePos(num, num2)), null, Entity.StructureState.Finished, new Entity.SetOwnerInfo(newOwner));
				entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
				entity.Structure.State = StructureStates.UnderConstruction;
				num += value.StructureType.WidthInTiles + 1;
				if (!The.Map.TileIsOnMap(new Point(num + 2, num2)) || num > 30)
				{
					num = 1;
					num2 += 4;
				}
			}
		}
	}

	public static void BuildStructuresTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(9, 7)));
		The.MapUI.ZoomToMapPosition(10, 5);
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			Entity entity = PlacePerson("Agent", "NR: " + i, Reproduction.Male, new Point(5, 8 + i), Color.White, 40f, noSkills: false, expedition);
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
			entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(9, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(9, 7));
		AddFinishedStructure("structure:sensor", new Point(8, 7), expedition);
	}

	public static void ChickenEatTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(10, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(10, 5), 20f, allegiance).BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0.1f;
		PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(5, 8), Color.White, 40f, noSkills: false, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBerries"]), new Point(9, 7));
		AddFinishedStructure("structure:sensor", new Point(8, 7), expedition);
	}

	public static void RatEatTest()
	{
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		The.MapUI.ZoomToMapPosition(10, 5);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]);
		Expedition ownerExpedition = new Expedition(allegiance, "Rat", "Rat", MapManager.TileToWorldPos(new Point(22, 5)));
		for (int i = 0; i < 1; i++)
		{
			PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(10, 5), 8f, allegiance, null, ownerExpedition);
		}
		AddNoOwnerItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), new Point(10, 7));
		AddFinishedStructure("structure:sensor", new Point(8, 7), owner);
	}

	public static void NeedsTest()
	{
		The.MapUI.ZoomToMapPosition(5, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		The.Sim.DateAndTime.TimeOfDay = 0.7;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(5, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(5, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), new Point(5, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(5, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(5, 7));
		_ = 0;
		Entity entity = PlacePerson("Sebastian", "Zyp 1", Reproduction.Male, new Point(4, 3), Color.White, 40f, noSkills: false, expedition);
		entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 1f;
		entity.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 0.5f;
		entity.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
		entity.BiologicalEntity.Needs.NeedsList["foodEnergy"].PhysicalNeed.DaysAtZero = 0f;
		entity.BiologicalEntity.Needs.NeedsList["stimulants"].CurrentLevel = 0f;
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.8f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(0.8f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(0.7f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(0.5f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(0.5f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.5f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(0.8f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.9f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.6f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.8f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.6f, GameData.Instance.AllSkillTypes["biology"]));
	}

	public static void GeometryTest()
	{
		The.MapUI.ZoomToMapPosition(5, 10);
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		_ = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
		int num = 1;
		int num2 = 2;
		foreach (KeyValuePair<string, EntityType> allStructureType in GameData.Instance.AllStructureTypes)
		{
			EntityType value = allStructureType.Value;
			if (value == null || value.KeyName.EndsWith("sentry") || value.StructureType.IsAddon)
			{
				continue;
			}
			bool flag = false;
			while (true)
			{
				num += value.StructureType.WidthInTiles + 1;
				if (!The.Map.TileIsOnMap(new Point(num + 12, num2)) || num > 30)
				{
					num = 1;
					num2 += 4;
				}
				if (flag)
				{
					break;
				}
				flag = true;
			}
		}
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:ovalrocks4"]), new Point(15, 4));
	}

	public static void CampfireTest()
	{
		Point point = new Point(10, 5);
		The.MapUI.ZoomToMapPosition(point);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, noSkills: false, expedition);
		PlacePerson("Kasdfsdfrol", "rtyrtyNikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, noSkills: false, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(6, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(6, 8));
		AddFinishedStructure("structure:campfire", new Point(16, 7), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(16, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 8));
	}

	public static void PeatTest()
	{
		The.MapUI.ZoomToMapPosition(5, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		GetBob(new Point(5, 5), expedition);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:steelSpade"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedTrowel"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), new Point(7, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(7, 6));
		AddTree("tree:spoak", new Point(7, 4), "crop:spoakBranches", 4);
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:clayDeposit"]), MapManager.TileToWorldPos(new Point(6, 8)));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:peatDeposit"]), MapManager.TileToWorldPos(new Point(9, 8)));
	}

	public static void AnimTest()
	{
		The.MapUI.ZoomToMapPosition(15, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, noSkills: false, expedition);
	}

	public static void ProductionTest()
	{
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Entity entity = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(16, 5), Color.White, 40f, noSkills: false, expedition);
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["smithing"], new Skill(0.1f, GameData.Instance.AllSkillTypes["smithing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["mechanics"], new Skill(0.1f, GameData.Instance.AllSkillTypes["mechanics"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["electronics"], new Skill(0.1f, GameData.Instance.AllSkillTypes["electronics"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["chemistry"], new Skill(0.1f, GameData.Instance.AllSkillTypes["chemistry"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["farming"], new Skill(0.2f, GameData.Instance.AllSkillTypes["farming"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["weeding"], new Skill(0.2f, GameData.Instance.AllSkillTypes["weeding"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["grasping"], new Skill(0.2f, GameData.Instance.AllSkillTypes["grasping"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fruitPicking"], new Skill(0.2f, GameData.Instance.AllSkillTypes["fruitPicking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["construction"], new Skill(0.2f, GameData.Instance.AllSkillTypes["construction"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["archery"], new Skill(0.2f, GameData.Instance.AllSkillTypes["archery"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["sneaking"], new Skill(0.2f, GameData.Instance.AllSkillTypes["sneaking"]));
		AddFinishedStructure("structure:campfire", new Point(18, 5), expedition);
		AddFinishedStructure("structure:simpleSmithy", new Point(22, 5), expedition);
		AddFinishedStructure("structure:goldFurnace", new Point(13, 5), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedCookingPot"]), new Point(16, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:shadeleafCanes"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWorkersToolbox"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gold"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gold"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:wroughtIron"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blisterSteel"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:gunBarrelUnbored"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sandMold"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:goldOre"]), new Point(15, 7));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(16, 5));
	}

	public static void GaitTest()
	{
		The.MapUI.ZoomToMapPosition(15, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Entity entity = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(2, 2), Color.White, 40f, noSkills: false, expedition);
		entity.SetRotationAndDir((float)Math.PI / 2f);
		entity.Renderable.RenderAsModel.FinalModelScale = 3f;
	}

	public static void UnloadTest()
	{
		The.MapUI.ZoomToMapPosition(11, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(11, 8)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(10, 6), Color.White, 40f, noSkills: false, expedition).SetRotationAndDir((float)Math.PI / 2f);
		AddFinishedStructure("structure:skimmerHull", new Point(8, 6), expedition).Parts.Find((Entity p) => p.EntityType == GameData.Instance.AllEntityTypes["item:scrapMetal"]).DoDamage(1f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(15, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(15, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"]), new Point(14, 5));
	}

	public static void MacheteTest()
	{
		The.MapUI.ZoomToMapPosition(15, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(2, 2)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(2, 2), Color.White, 40f, noSkills: false, expedition).SetRotationAndDir((float)Math.PI / 2f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), new Point(2, 2));
	}

	public static void StorageTest()
	{
		The.MapUI.ZoomToMapPosition(15, 12);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(13, 4)));
		Entity container = AddFinishedStructure("structure:A-frameTarp", new Point(13, 4), expedition);
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(4, 2), Color.White, 40f, noSkills: false, expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(4, 4));
		for (int i = 0; i < 80; i++)
		{
			AddColonyItemToStorage(new Entity(GameData.Instance.AllEntityTypes["item:improvisedBasicSpear"]), container);
		}
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:largePlotSpot"]), MapManager.TileToWorldPos(new Point(5, 7)));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 5)) + new Vector3(0f, 24f, 0f));
		AddTerrainItem(new Entity(GameData.Instance.AllEntityTypes["terrain:smallPlotSpot"]), MapManager.TileToWorldPos(new Point(8, 9)) + new Vector3(0f, 24f, 0f));
	}

	public static void HarvestPickupTest()
	{
		The.MapUI.ZoomToMapPosition(38, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(38, 25)));
		Entity entity = PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(38, 25), Color.White, 52f, noSkills: true, "grey1", expedition);
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(38, 25));
	}

	public static void HarvestTest2()
	{
		The.MapUI.ZoomToMapPosition(15, 15);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(20, 22)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(20, 20), Color.White, 40f, noSkills: false, expedition);
	}

	public static void BuildingSoundTest()
	{
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));
		The.MapUI.ZoomToMapPosition(15, 15);
		Entity entity = PlacePerson("Builder", "Bob", Reproduction.Male, new Point(13, 15), Color.White, 52f, noSkills: false, expedition);
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		for (int i = 0; i < 4; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakBranches"])
			{
				Bulk = 1f
			}, new Point(15, 15));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedMachete"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sensor"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:panelScraps"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:seatCushions"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:domeTent"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:octagonalTent"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smallTent"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:domeTent"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:octagonalTent"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smallTent"]), new Point(15, 15));
	}

	public static void BuildingTest()
	{
		The.MapUI.ZoomToMapPosition(15, 15);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(1, 1)));
		Entity entity = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(15, 15), Color.White, 40f, noSkills: false, expedition);
		entity.SetRotationAndDir((float)Math.PI / 2f);
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		PlacePerson("Dude", "Dudeson", Reproduction.Male, new Point(18, 15), Color.White, 40f, noSkills: false, expedition);
		for (int i = 0; i < 16; i++)
		{
			AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:daysheenLeaves"])
			{
				Bulk = 0.5f
			}, new Point(15, 12));
		}
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedSnips"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thermalTarp"]), new Point(15, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fieldLabPacked"]), new Point(15, 15));
	}

	public static void StorageTest2()
	{
		The.MapUI.ZoomToMapPosition(15, 15);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 15)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(15, 15), Color.White, 40f, noSkills: false, expedition).SetRotationAndDir((float)Math.PI / 2f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:spoakLeaves"]), new Point(15, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:stones"]), new Point(15, 12));
		Entity item = new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenGuts"])
		{
			Bulk = 0.5f
		};
		Entity container = AddFinishedStructure("structure:storageHole", new Point(13, 15), expedition);
		AddColonyItemToStorage(item, container);
	}

	public static void ShelterTest()
	{
		The.MapUI.ZoomToMapPosition(5, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(3, 3)));
		Entity container = AddFinishedStructure("structure:lean-toTarp", null, expedition, flipHorizontally: false, new Vector3(150f, 150f, 0f));
		bool noSkills = false;
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(4, 4), Color.White, 52f, noSkills, "blue1", expedition, container);
	}

	public static void MartinAITest()
	{
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 9), 20f, allegiance);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(15, 5));
		PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(14, 5), Color.White, 40f, noSkills: false, expedition);
	}

	public static void MLoAnimationTest()
	{
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:thunderChicken"]);
		PlaceAnimal("entity:thunderChicken", Reproduction.Male, new Point(16, 9), 20f, allegiance);
		PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(14, 9), Color.White, 40f, noSkills: false, expedition);
	}

	private static void DoSomething(Entity entity, DebugJobEvaluator eval)
	{
	}

	public static void DecompositionTest()
	{
		The.Sim.DateAndTime.SecondsPerDay = 80.0;
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		PlacePerson("Karol", "Nikolaev ", Reproduction.Male, new Point(8, 8), Color.White, 40f, noSkills: false, expedition);
		TerrainTile obj = The.Map.TileMap[17][11];
		obj.Temperature = 255f;
		obj.Moisture = 0f;
		TerrainTile obj2 = The.Map.TileMap[18][11];
		obj2.Temperature = 278f;
		obj2.Moisture = 0f;
		TerrainTile obj3 = The.Map.TileMap[19][11];
		obj3.Temperature = 284f;
		obj3.Moisture = 0f;
		TerrainTile obj4 = The.Map.TileMap[20][11];
		obj4.Temperature = 294f;
		obj4.Moisture = 0f;
		The.Map.TileMap[17][12].Temperature = 255f;
		The.Map.TileMap[18][12].Temperature = 278f;
		The.Map.TileMap[19][12].Temperature = 284f;
		The.Map.TileMap[20][12].Temperature = 294f;
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(17, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:fishingRodPigFly"]), new Point(2, 2));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(19, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(20, 11));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackzpacho"]), new Point(17, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackpulp"]), new Point(18, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:blackzpacho"]), new Point(19, 12));
		Entity entity = new Entity(GameData.Instance.AllEntityTypes["item:turnipMeat"]);
		entity.NonLivingEntity.DoConditionDamage(0.95f, null);
		AddColonyItem(entity, new Point(20, 12));
	}

	public static void EmptyMap()
	{
		Options.SetOption("Dev.God mode", boolToSet: true);
		The.MapUI.ZoomToMapPosition(5, 10);
	}

	public static void StructureGeoMap()
	{
		Options.SetOption("Dev.God mode", boolToSet: true);
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition owner = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		int xDistance = 200;
		int yDistance = 200;
		int num = 100;
		int num2 = 100;
		int maxX = 1200;
		Vector3 location = new Vector3(num, num2, 0f);
		AddFinishedStructure("structure:campfire", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:lean-toTarp", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:lean-toSpoakLeaves", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:A-frameSpoakLeaves", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:A-frameScraps", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:A-frameTarp", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:lean-toScraps", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:domeShelterTarp", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:domeShelterSpoakShingles", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:wigwamSpoakShingles", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:daysheenTipi", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:storageHole", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:smokeOven", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:abatis", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:abatis", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:storageHole", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:cooledFoodCache", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:smokeOven", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:lean-toTarp", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:improvisedKitchen", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:improvisedWorkbench", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fieldLab", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:cookhouse", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:domeTent", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:octagonalTent", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:smallTent", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:weatherStation", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fieldKitchen", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:molecularAssembler", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:satelliteGroundStation", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:helipad", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:helipadBig", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:rareMetalRefinery", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:improvisedGreenhouse", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:kiln", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:improvisedSmithy", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:simpleSmithy", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:firewoodStack", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:compostBin", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:hideRack", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:caneHut", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:clayHut", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:compostPit", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:meatDryingRack", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:dryingShed", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:kilnImprovisedSmall", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:goldFurnace", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:simplePort", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:radioHutImprovised", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:landingImprovised", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:helipadBig", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fishTrapCreekSticks", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fishTrapCreekNet", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fishTrapCoast", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fishTrapShoreBasket", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
		AddFinishedStructure("structure:fishTrapShoreHoopNet", null, owner, flipHorizontally: false, location);
		UpdateXYPos(ref location, xDistance, yDistance, num, maxX);
	}

	private static void UpdateXYPos(ref Vector3 location, int xDistance, int yDistance, int minX, int maxX)
	{
		location.X += xDistance;
		if (location.X > (float)maxX)
		{
			location.X = minX;
			location.Y += yDistance;
		}
	}

	public static void AssetGeometryMap()
	{
		Options.SetOption("Dev.God mode", boolToSet: true);
		The.MapUI.ZoomToMapPosition(5, 10);
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
	}

	public static void SkimmerSalvageTest()
	{
		The.MapUI.ZoomToMapPosition(35, 25);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(35, 20)));
		Vector3 vector = new Vector3(576f, 2400f, 0f);
		AddFinishedStructure("structure:skimmerHull", null, expedition, flipHorizontally: false, vector + new Vector3(96f, 48f, 0f));
		AddFinishedStructure("structure:skimmerEngineTop", null, expedition, flipHorizontally: false, vector + new Vector3(80f, 0f, 0f));
		AddFinishedStructure("structure:skimmerEngineSide", null, expedition, flipHorizontally: false, vector + new Vector3(128f, 80f, 0f));
		AddFinishedStructure("structure:skimmerTail", null, expedition, flipHorizontally: false, vector + new Vector3(0f, 48f, 0f));
		Entity entity = PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(35, 20), Color.White, 40f, noSkills: false, expedition);
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(1f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
	}

	public static void HarvestTest()
	{
		The.MapUI.ZoomToMapPosition(5, 10);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 10)));
		PlacePerson("Karol", "Nikolaev", Reproduction.Male, new Point(5, 10), Color.White, 40f, noSkills: false, expedition);
		Point pos = new Point(5, 13);
		AddResource(pos, "firewood");
		for (int i = 0; i < 12; i++)
		{
			AddColonyItem("item:wetFirewood", pos);
		}
		AddFinishedStructure("structure:firewoodStack", new Point(7, 13), expedition);
	}

	public static void TurnipHutTest()
	{
		The.MapUI.ZoomToMapPosition(3, 10);
		Point point = new Point(8, 3);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		GetBob(new Point(9, 10), exp);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipShell"])
		{
			Bulk = 3f
		}, new Point(9, 12));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipShell"])
		{
			Bulk = 3f
		}, new Point(9, 15));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"])
		{
			Bulk = 4.9f
		}, new Point(12, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCarcass"])
		{
			Bulk = 4.9f
		}, new Point(15, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:turnipCracker"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedString"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sticks"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firegrassSod"]), point);
	}

	public static void PathingTest()
	{
		Point point = new Point(24, 25);
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		GetBob(point, exp).Intelligence.SetName("Bob", "1");
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		GetBob(point, exp);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(30, 30), 20f, allegiance, "Pale race");
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(25, 30), 20f, allegiance, "Pale race");
	}

	public static void VerminTest()
	{
		Point point = new Point(16, 5);
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		Expedition exp = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		Entity bob = GetBob(point - new Point(3, 4), exp);
		bob.BiologicalEntity.Needs.NeedsList["protein"].CurrentLevel = 0f;
		bob.BiologicalEntity.Needs.NeedsList["foodEnergy"].CurrentLevel = 0f;
		bob.BiologicalEntity.AddToStomachContents(-1f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnifeSpear"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:astroRation"]), point);
		new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
		new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(point));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), point);
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(allEntityType.Value, ProductionStatistics.StatTypes.Produced, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
			The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(allEntityType.Value, ProductionStatistics.StatTypes.ConsumedFood, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
			The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(allEntityType.Value, ProductionStatistics.StatTypes.Degraded, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
			The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(allEntityType.Value, ProductionStatistics.StatTypes.EatenByCreatures, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
			The.Sim.PlaySite.PlayerAllegiance.Statistics.AddProductionEvent(allEntityType.Value, ProductionStatistics.StatTypes.Disappeared, The.Sim.GameplayRandomGenerator.Next(20, "fd"));
		}
		for (int i = 0; i < 10; i++)
		{
		}
	}

	public static void CookingTest()
	{
		Point point = new Point(16, 5);
		The.MapUI.ZoomToMapPosition(point.X, point.Y);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		The.Sim.DateAndTime.TimeOfDay = 0.4;
		GetBob(point, expedition).Intelligence.SetName("Bob", "1");
		AddFinishedStructure("structure:smokeOven", new Point(16, 3), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:firewood"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:carbonTail"]), point);
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"], "all2");
		Expedition ownerExpedition = new Expedition(new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:binalRat"]), "Start2", "Start2", MapManager.TileToWorldPos(point));
		PlaceAnimal("entity:binalRat", Reproduction.Female, new Point(12, 5), 18f, allegiance, null, ownerExpedition).Intelligence.DisableAI = true;
	}

	public static void ImmobilizeEntity(Entity e1)
	{
		e1.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed = 0.01f;
		e1.EntityType.LocomotorType.LeggedLocomotorType.WalkSlowSpeed = 0.01f;
		e1.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed = 0.01f;
		e1.EntityType.LocomotorType.LeggedLocomotorType.RunSpeed = 0.01f;
	}

	public static void RepairTest()
	{
		The.MapUI.ZoomToMapPosition(5, 2);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(8, 6)));
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(8, 8), Color.White, 52f, noSkills: false, "blue1", expedition);
		AddFinishedStructure("structure:clayHut", new Point(9, 8), expedition).NonLivingEntity.Parts.FirstOrDefault((Entity p) => p.EntityType.KeyName == "item:spoakBranchesTrimmed");
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:waterCaneStem"]), new Point(8, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:advancedKnife"]), new Point(8, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:metalWire"]), new Point(8, 6));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:improvisedSpade"]), new Point(8, 6));
	}

	public static void AttackNestTest()
	{
		The.MapUI.ZoomToMapPosition(5, 2);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(8, 6)));
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(8, 8), Color.White, 52f, noSkills: false, "blue1", expedition).SetRotationAndDir(-(float)Math.PI / 2f);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sulfurSmokeBomb"]), new Point(8, 6));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		Entity entity = new Entity(GameData.Instance.AllEntityTypes["terrain:quaditeNest"]);
		entity.Initialize(The.Sim.PlaySite, allegiance);
		entity.Name = "North quadite nest";
		entity.PlaceEntityOnPlaySite(new Vector3(100f, 600f, 0f), null, Entity.StructureState.Finished, null);
	}

	public static void LightingTest()
	{
		The.MapUI.ZoomToMapPosition(5, 2);
		The.Sim.DateAndTime.TimeOfDay = 0.8;
		Point point = new Point(5, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(point));
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Comfort);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Security);
		expedition.AdoptTierPolicy(GameData.Instance.AllTierTypes["basic"], RatingTypes.Food);
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills: false, "blue1", expedition).BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
		AddFinishedStructure("structure:domeTent", new Point(9, 2), expedition);
		AddFinishedStructure("structure:octagonalTent", new Point(12, 2), expedition);
		AddFinishedStructure("structure:smallTent", new Point(15, 2), expedition);
		AddColonyItem("item:firewood", point);
		AddColonyItem("item:firewood", point);
		AddColonyItem("item:firewood", point);
		AddFinishedStructure("structure:campfire", new Point(7, 7), expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), point);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:thunderChickenMeat"]), point);
		AddFinishedStructure("structure:kiln", new Point(4, 7), expedition);
	}

	public static void WeaponsTest()
	{
		The.MapUI.ZoomToMapPosition(5, 2);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(5, 5)));
		The.Sim.DateAndTime.TimeOfDay = 0.8;
		bool noSkills = false;
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills, "blue1", expedition).SetRotationAndDir(-(float)Math.PI / 2f);
		PlacePerson("Ward2", "Conlan2", Reproduction.Male, new Point(5, 2), Color.White, 52f, noSkills, "blue1", expedition);
		Entity entity = new Entity(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"]);
		AddColonyItem(entity, new Point(5, 5));
		entity.Item.Ammunition.NoOfRounds = 1;
		Entity entity2 = new Entity(GameData.Instance.AllEntityTypes["item:gunpowderRifle"]);
		AddColonyItem(entity2, new Point(5, 5));
		entity2.Contains.AddToContain(entity, out var _);
		entity = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
		AddColonyItem(entity, new Point(6, 5));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentrySprayGun"]), new Point(6, 5));
		AddColonyItem("item:sentryWeaponMount", new Point(6, 5));
		AddColonyItem("item:spraySentry", new Point(6, 5));
	}

	private static void FightTest()
	{
		The.MapUI.ZoomToMapPosition(15, 5);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(15, 5));
		GetBob(new Point(15, 5), expedition);
		GetBob(new Point(15, 5), expedition);
		GetBob(new Point(15, 5), expedition);
		Entity entity = PlacePerson("Charles", "Jacobi", Reproduction.Male, new Point(15, 5), Color.White, 40f, noSkills: false, expedition);
		entity.Intelligence.Skills = new Dictionary<SkillType, Skill>();
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["bushcraft"], new Skill(1f, GameData.Instance.AllSkillTypes["bushcraft"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["butchering"], new Skill(0.8f, GameData.Instance.AllSkillTypes["butchering"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["hunting"], new Skill(1f, GameData.Instance.AllSkillTypes["hunting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["fishing"], new Skill(1f, GameData.Instance.AllSkillTypes["fishing"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["foraging"], new Skill(1f, GameData.Instance.AllSkillTypes["foraging"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["cooking"], new Skill(0.8f, GameData.Instance.AllSkillTypes["cooking"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["menial"], new Skill(0.75f, GameData.Instance.AllSkillTypes["menial"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["shooting"], new Skill(1f, GameData.Instance.AllSkillTypes["shooting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["armedMelee"], new Skill(1f, GameData.Instance.AllSkillTypes["armedMelee"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["unarmedFighting"], new Skill(0.8f, GameData.Instance.AllSkillTypes["unarmedFighting"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["medicine"], new Skill(0.4f, GameData.Instance.AllSkillTypes["medicine"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["psychology"], new Skill(0.1f, GameData.Instance.AllSkillTypes["psychology"]));
		entity.Intelligence.Skills.Add(GameData.Instance.AllSkillTypes["biology"], new Skill(0.2f, GameData.Instance.AllSkillTypes["biology"]));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:patrician"]);
		PlaceAnimal("entity:patrician", Reproduction.Male, new Point(12, 5), 20f, allegiance).Find<BodyComponent>(out var c);
		c.Body.ChangeMaxHitpoints(1000f);
	}

	private void MapEditorTest()
	{
		The.MapUI.ZoomToMapPosition(16, 16);
		new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(10, 10)));
	}

	private static void BushDragon()
	{
		The.MapUI.ZoomToMapPosition(157, 56);
		Expedition expedition = new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(161, 60)));
		PlacePerson("Ward", "Conlan", Reproduction.Male, new Point(183, 81), Color.White, 52f, noSkills: false, "green2", expedition);
		AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:sentry"]), new Point(187, 80));
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bushDragon"]);
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(158, 58), 20f, allegiance, "Pale race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 59), 20f, allegiance, "Pale race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(160, 54), 20f, allegiance, "Pale race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(161, 51), 20f, allegiance, "Pale race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(157, 56), 20f, allegiance, "Pale race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(151, 55), 20f, allegiance, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(149, 56), 20f, allegiance, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(150, 58), 20f, allegiance, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(150, 62), 20f, allegiance, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 55), 20f, allegiance, "Small race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(155, 56), 20f, allegiance, "Small race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(152, 56), 20f, allegiance, "Small race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(156, 57), 20f, allegiance, "Small race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bushDragon", Reproduction.Male, new Point(153, 57), 20f, allegiance, "Small race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:turnip", Reproduction.Male, new Point(172, 54), 20f, null, "Dark race").Intelligence.DisableAI = true;
		Allegiance allegiance2 = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:bird"]);
		PlaceAnimal("entity:bird", Reproduction.Male, new Point(167, 68), 20f, allegiance2, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bird", Reproduction.Male, new Point(169, 69), 20f, allegiance2, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bird", Reproduction.Male, new Point(174, 62), 20f, allegiance2, "Dark race").Intelligence.DisableAI = true;
		PlaceAnimal("entity:bird", Reproduction.Male, new Point(145, 64), 20f, allegiance2, "Pale race").Intelligence.DisableAI = true;
	}
}
