using UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2;

public class Scenario2Loader : ScenarioLoader
{
	public override string FolderName => "Muckroot Research Station";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Muckroot Research Station",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 11,
				Day = 3,
				TimeOfDay = 0.36
			},
			MapKey = "h Muckroot Sandbox 128",
			MapSize = MapSize.Large,
			SummaryDescription = "-WORK IN PROGRESS!- A group of scientists set up an outpost in an enigmatic biome. If the area proves habitable, more people may join them. \n \nERA: Planetfall",
			Allow32Bit = false,
			Description = "-NOTE! This scenario is UNDER DEVELOPMENT!-\n \n MISSION: Assess the value of the muckroot biome as a place for settlement.\n OBJECTIVES: Explore the muckroot biome and establish a permanent research base in the vicinity.\n ASSETS: Additional supplies will be provided by air transport. Also, personnel from Duke's Landing have expressed an interest in joining the expedition at a later stage.",
			ThumbnailImage = "Scenarios/Scenario 2/Scenario Screen/muckrootCamp_scenario_thumb",
			Image = "Survival",
			IsInDevelopment = true,
			SortOrder = 8
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario2DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Default/Scenario Screen/TitleImgSurvival_1920px";
		scenarioData.LoadingDialogText = "MISSION: Assess the value of the muckroot biome as a place for settlement.\n OBJECTIVES: Explore the muckroot biome and establish a permanent research base in the vicinity.\n ASSETS: Additional supplies will be provided by air transport. Also, personnel from Duke's Landing have expressed an interest in joining the expedition at a later stage.";
		scenarioData.LoadingDialogImage = "FleeingSkimmer";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_1";
		scenarioData.EnableMissions = true;
		scenarioData.ConditionalEvents = new string[8] { "MUCKROOTMAP_loseGame", "MUCKROOTMAP_musicTrackList", "MUCKROOTMAP_continualSpawnTwinklersNorthWestCrevice", "MUCKROOTMAP_continualSpawnBinalRatsNorthWest1", "MUCKROOTMAP_continualSpawnBinalRatsNorthWest2", "MUCKROOTMAP_continualSpawnBinalRatsNorthWest3", "MUCKROOTMAP_continualSpawnThunderChickenNorthWest", "MUCKROOTMAP_timedSpawnBeginningPopulationNormal" };
		scenarioData.Actions = new string[161]
		{
			"initIncludeDateInBurial", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initGameOver", "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", "meeting3Security", "meetingEmigrateThreat",
			"meetingEmigrateThreatAllUnhappy", "meeting3Food", "meeting3Comfort", "comfortBasicPolicyAdopted", "foodBasicPolicyAdopted", "securityBasicPolicyAdopted", "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted",
			"foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted", "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree", "securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree",
			"securityAdvancedPolicyAdoptedAllAgree", "initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText", "initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText", "spawnDemonTreeExpedition#1", "spawnDemonTreeExpedition#2", "spawnDemonTreeExpedition#3",
			"setMaxTwinklersLow", "placeExpedition", "spawnPlayerAllegiance", "setPlayerCredits", "setView", "exploreEntireMap", "spawnOtherSite1", "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1", "spawnOtherSite2",
			"spawnOtherSite2Allegiance1", "spawnOtherSite2Expedition1", "spawnOtherSite1Expedition1Helipad", "spawnOtherSite1Expedition1SatelliteGroundStation", "spawnOtherSite1Expedition1Skimmer", "spawnOtherSite1Expedition1Harpy", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant",
			"spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "spawnMuckrootRandomImmigrant", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation",
			"otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_precolRation", "otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_simCoffeeBeans",
			"otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_simCoffeeBeans", "otherSite1Expedition1_sentry", "otherSite1Expedition1_sentry", "otherSite1Expedition1_sentry", "otherSite1Expedition1_coilRifle", "otherSite1Expedition1_coilRifle", "otherSite1Expedition1_coilRifle", "otherSite1Expedition1_coilRifle",
			"otherSite1Expedition1_acetylene", "otherSite1Expedition1_acetylene", "otherSite1Expedition1_acetylene", "otherSite1Expedition1_liquidGas", "otherSite1Expedition1_liquidGas", "otherSite1Expedition1_liquidGas", "otherSite1Expedition1_liquidGas", "otherSite1Expedition1_ironCanister", "otherSite1Expedition1_ironCanister", "otherSite1Expedition1_ironCanister",
			"otherSite1Expedition1_ironCanister", "otherSite1Expedition1_inactivatedFoodCoolerUnit", "otherSite1Expedition1_textile", "otherSite1Expedition1_textile", "otherSite1Expedition1_textile", "otherSite1Expedition1_smallTent", "otherSite1Expedition1_octagonalTent", "otherSite1Expedition1_domeTent", "otherSite1Expedition1_domeTent", "otherSite1Expedition1_domeTent",
			"otherSite1Expedition1_thermalTarp", "otherSite1Expedition1_thermalTarp", "otherSite1Expedition1_sentryGunAmmo", "otherSite1Expedition1_sentryGunAmmo", "otherSite1Expedition1_sentryGunAmmo", "otherSite1Expedition1_sentryGunAmmo", "otherSite1Expedition1_sentryGunAmmo", "otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_coilRifleAmmo",
			"otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_coilRifleAmmo", "otherSite1Expedition1_metalworkersToolbox", "otherSite1Expedition1_hammer", "otherSite1Expedition1_hammer", "otherSite1Expedition1_knife", "otherSite1Expedition1_knife", "otherSite1Expedition1_knife",
			"otherSite1Expedition1_knife", "otherSite1Expedition1_metalWire", "otherSite1Expedition1_metalWire", "otherSite1Expedition1_metalWire", "otherSite1Expedition1_snips", "otherSite1Expedition1_cookingPot", "otherSite1Expedition1_cookingPot", "otherSite1Expedition1_machete", "otherSite1Expedition1_machete", "otherSite1Expedition1_machete",
			"otherSite1Expedition1_machete", "otherSite1Expedition1_steelPickaxe", "otherSite1Expedition1_steelPickaxe", "otherSite1Expedition1_steelPickaxe", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string", "otherSite1Expedition1_string",
			"otherSite1Expedition1_sensor", "otherSite1Expedition1_sensor", "otherSite1Expedition1_sensor", "smallFog1", "smallFog2", "smallFog3", "smallFog4", "smallFog5", "smallFog6", "smallFog7",
			"haze1"
		};
		scenarioData.MainDifficultySettings = new Difficulty[1]
		{
			new Difficulty
			{
				KeyName = "normal",
				Name = "Normal",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"startingLocations",
						new string[1] { "southCoast" }
					},
					{
						"people",
						new string[1] { "7people" }
					},
					{
						"equipment",
						new string[1] { "muckrootStationGear" }
					},
					{
						"resources",
						new string[1] { "average" }
					},
					{
						"otherSite1Allegiance1Relation",
						new string[1] { "neutral" }
					}
				}
			}
		};
		scenarioData.OptionSets = new OptionSet[5]
		{
			new OptionSet
			{
				KeyName = "startingLocations",
				Name = "Camp site",
				DisplayGroup = 0,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "southCoast",
						Name = "Beach",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Coastline",
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setStartingLocationSouth" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "people",
				Name = "Camp members",
				DisplayGroup = 1,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "7people",
						Name = "7 people",
						ShortDescription = "Group of 7 people",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							IsDefault = true,
							Name = "7 people",
							ScoreModifier = 1f
						},
						ActionKeys = new string[8] { "spawnRandomMuckrootPerson1", "spawnRandomMuckrootPerson2", "spawnRandomMuckrootPerson3", "spawnRandomMuckrootPerson4", "spawnRandomMuckrootPerson5", "spawnRandomMuckrootPerson6", "spawnRandomMuckrootPerson7", "spawnHaulRobot" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "equipment",
				Name = "Supplies",
				DisplayGroup = 0,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "muckrootStationGear",
						Name = "Muckroot station gear",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Some gear",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[59]
						{
							"startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", "startOctagonalTent", "startSmallTent", "startHuntingRifle",
							"startHuntingRifle", "startHuntingRifle", "startHuntingRifle", "startHuntingRifle", "startSentry", "startSentryAmmo", "startRifleAmmo", "startRifleAmmo", "startRifleAmmo", "startRifleAmmo",
							"startRifleAmmo", "startMachete", "startSensor", "startCookingPot", "startWeatherStationMast", "startWeatherStationSensors", "startLiquidGas", "startLiquidGas", "startLiquidGas", "startLiquidGas",
							"startLiquidGas", "startPaint", "startPaint", "startPaint", "startPaint", "startPaint", "startPaint", "startPaint", "startPaint", "startPaint",
							"startSimCoffeeBeans", "startSimCoffeeBeans", "startAssemblerCabinet", "startVacuumChamber", "startAssemblerCooling", "startAssemblerPlateA", "startAssemblerMasterPlateA", "startAcetylene", "startAcetylene", "startAcetylene",
							"startAcetylene", "startAssemblerPlateB", "startAssemblerMasterPlateB", "startIronCanister", "startStructureDomeTent", "startStructureSmallTent", "startStructureFieldKitchen", "startStructureHelipadBig", "startStructureGroundStation"
						}
					}
				}
			},
			new OptionSet
			{
				KeyName = "resources",
				Name = "Resources",
				DisplayGroup = 2,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "average",
						Name = "Average",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Average",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[2] { "setFishSchoolMedium", "setNormalResources" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "otherSite1Allegiance1Relation",
				Name = "Other Site 1 Relation",
				DisplayGroup = 2,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "neutral",
						Name = "Neutral",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Neutral",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "spawnNeutralOtherSite1Allegiance1Relation" }
					}
				}
			}
		};
		return scenarioData;
	}
}
