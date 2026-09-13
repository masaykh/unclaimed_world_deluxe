using UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x;

public class Scenario4xLoader : ScenarioLoader
{
	public override string FolderName => "Fields of Tau Ceti - Cudgel Hills (M)";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Fields of Tau Ceti - Cudgel Hills (M)",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 180,
				Day = 5,
				TimeOfDay = 0.5
			},
			MapKey = "jx Hills Rivers Small",
			MapSize = MapSize.Medium,
			Allow32Bit = true,
			SummaryDescription = "OPEN-ENDED MAP: A small community sets out to build a settlement in the wilderness. Lacking advanced technology, they rely on ancient methods when working the land. \nERA: The Great Descent",
			Description = "OPEN-ENDED MAP. \n \n'Out here, a new life awaited. Any differences lay behind them and they could now do things their own way. What they lacked in possessions they made up for with determination. The wilderness would welcome those who were brave and stood undivided.'",
			ThumbnailImage = "Scenarios/Scenario 4/Scenario Screen/colonyFarmDogRobot_scenario_thumb",
			Image = "Farming",
			IsInDevelopment = false,
			SortOrder = 5
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario4xDataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px";
		scenarioData.LoadingDialogText = "CUDGEL HILLS, SUMMER, YEAR 180 \n \nCastor took the last bag ashore and put it next to the others. He looked at his companions who stood dispersed on the riverside and tried to guess their thoughts. Their faces showed expressions ranging from thrill to apprehension. \n \nThe boatman was ready to set off. 'Good luck, people. I don't agree with all that you did back there, but I don't think you were treated fairly either. I know there are a few people who would like to join you, so give a call when you have that radio set up.' \n \nAs the boat sailed away, Castor turned his attention to Linsey who sat hunched, a small display in her hands. She was waking up from a long nap on the boat. Squinting, she studied her PPU which was also coming alive now. It was busily piling up data as her fellow travellers examined the location, revealing its secrets. \n \nShe looked up and smiled: 'It looks promising...good fishing, good soil.' Castor turned towards the other companions and spread out his arms, embracing the orange hills, the yellow trees, the horizon.";
		scenarioData.LoadingDialogImage = "RiverBoat";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_2";
		scenarioData.EnableMissions = true;
		scenarioData.ConditionalEvents = new string[7] { "SANDBOXNOMADMAP_musicTrackList", "SANDBOXMAP_loseGame", "SANDBOXNOMADMAP_migrationLesserWhipjawEast", "SANDBOXNOMADMAP_migrationBajinganNorth", "initializeGlobalFarmingProperties", "initializeGlobalFishTrapProperties", "initializeGlobalAnimalTrapProperties" };
		scenarioData.Actions = new string[111]
		{
			"initGameOver", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",
			"meeting3Security", "meeting3Food", "meeting3Comfort", "meeting2Security", "meeting2Food", "meeting2Comfort", "meetingSecurityAllUnhappy", "meetingFoodAllUnhappy", "meetingComfortAllUnhappy", "comfortBasicPolicyAdopted",
			"foodBasicPolicyAdopted", "securityBasicPolicyAdopted", "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted", "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree",
			"securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree", "initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText",
			"initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText", "spawnBirdExpedition#1", "spawnBirdExpedition#2", "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2", "spawnLeafcutterExpedition#1", "spawnLeafcutterExpedition#3", "spawnSnatcherExpedition#1",
			"spawnThunderChickenExpedition#2", "spawnTurnipExpeditionNorth", "spawnDemonTreeExpedition#1", "spawnDemonTreeExpedition#3", "spawnSwampDemonTreeExpedition#2", "spawnAnimalMigrateTriggerWest", "spawnAnimalMigrateTriggerRiver", "spawnPlayerAllegiance", "placeExpedition", "setView",
			"setPlayerCredits", "exploreShroudNaturalTerminal", "spawnWildernessSite1", "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1", "spawnPlaySiteWildernessSite1Route", "spawnOtherSite1", "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1", "spawnPlaySiteSite1Route",
			"spawnImmigrantSmithingSpecialist", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantSurvivalTier", "spawnImmigrantBasicTier", "spawnImmigrantBasicTier",
			"spawnImmigrantBasicTier", "spawnImmigrantBasicTier", "spawnImmigrantBasicTier", "spawnImmigrantBasicTier", "spawnImmigrantBasicTier", "spawnImmigrantBasicTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier",
			"spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantAdvancedSecurityTier", "spawnImmigrantAdvancedSecurityTier", "spawnImmigrantAdvancedSecurityTier", "spawnOtherSite2", "spawnPlaySiteSite2Route", "startPierSpot1",
			"startPierSpot3", "smallFog1", "smallFog2", "smallFog3", "smallFog4", "smallFog5", "smallFog6", "sulphurousSmoke4", "haze6", "fog1",
			"fog2"
		};
		scenarioData.MainDifficultySettings = new Difficulty[3]
		{
			new Difficulty
			{
				KeyName = "easy",
				Name = "Easy",
				Description = "On EASY difficulty, you will start with an expedition equipped with: \nVERSATILE TOOLS and MANY PROVISIONS \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony.",
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "versatileExpedition" }
					},
					{
						"fauna",
						new string[1] { "average" }
					},
					{
						"resources",
						new string[1] { "average" }
					}
				}
			},
			new Difficulty
			{
				KeyName = "normal",
				Name = "Normal",
				Description = "On NORMAL difficulty, you will start with a random expedition, situated and equipped for either: \n \nFARMING,    FISHING   or   HUNTING \n(These can also be individually selected under CUSTOM) \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony's technology.",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[3] { "farmingExpedition", "huntingExpedition", "fishingExpedition" }
					},
					{
						"fauna",
						new string[1] { "average" }
					},
					{
						"resources",
						new string[1] { "average" }
					}
				}
			},
			new Difficulty
			{
				KeyName = "hard",
				Name = "Hard",
				Description = "On HARD difficulty, you will start with an expedition SPARSELY equipped with: \nBUSHCRAFT TOOLS. \n \nHINT: \nMost of the colony members have low principles, which means that they are content with little and accept bad conditions. It also means that you need to attract immigrants with higher principles if you want to advance your colony's technology.",
				IsDefault = false,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "bushcraftExpedition" }
					},
					{
						"fauna",
						new string[1] { "average" }
					},
					{
						"resources",
						new string[1] { "average" }
					}
				}
			}
		};
		scenarioData.OptionSets = new OptionSet[4]
		{
			new OptionSet
			{
				KeyName = "fauna",
				Name = "Fauna",
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
							Name = "Bush dragons",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[6] { "setMaxLeafcuttersNormal", "setSpawnIntervalLeafcutterNormal", "setMaxLesserWhipjawNormal", "setSpawnIntervalLesserWhipjawMigration", "setMaxBajinganNormal", "setSpawnIntervalBajinganMigration" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "expeditionType",
				Name = "Expedition type",
				DisplayGroup = 0,
				Options = new Option[5]
				{
					new Option
					{
						KeyName = "versatileExpedition",
						Name = "N/A",
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Versatile",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogueVersatile", "introDialogueScreen" },
						ActionKeys = new string[61]
						{
							"setStartingLocationNorthArableLand", "exploreShroudNorthArableLand", "startNaturalTerminalGenericPosition", "spawnCastor", "spawnLinsey", "spawnFarmingSpecialist1", "spawnHuntingSpecialist1", "spawnSmithingSpecialist1", "spawnConstructionSpecialist2", "spawnDog",
							"spawnHaulRobot", "startRadioAntenna", "startRadio", "startString", "startString", "startKnife", "startKnife", "startMachete", "startMachete", "startGunpowderRifle",
							"startGunpowderAmmo", "startGunpowderAmmo", "startBoltActionRifle", "startBoltActionAmmo", "startHammer", "startWroughtIron", "startMetalworkersToolbox", "startAnvil", "startBarClamps", "startBellows",
							"startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries", "startHoe", "startPickaxe", "startBrickMold", "startSpade",
							"startCookingPot", "startRefrigerator", "startClayJar", "startClayJar", "startVinegar", "startSalt", "startTurnipSalami", "startTurnipSalami", "startTurnipSalami", "startDriedSaltedStreakFin",
							"startDriedSaltedStreakFin", "startDriedSaltedStreakFin", "startDriedSaltedStreakFin", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack",
							"startHardtack"
						}
					},
					new Option
					{
						KeyName = "farmingExpedition",
						Name = "Farming",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogueFarming", "introDialogueScreen" },
						ActionKeys = new string[43]
						{
							"setStartingLocationNorthArableLand", "exploreShroudNorthArableLand", "startNaturalTerminalGenericPosition", "spawnCastor", "spawnLinsey", "spawnFarmingSpecialist1", "spawnFarmingSpecialist1", "spawnConstructionSpecialist1", "spawnCookingSpecialist2", "spawnDog",
							"spawnHaulRobot", "startRadioAntenna", "startRadio", "startString", "startKnife", "startKnife", "startMachete", "startMachete", "startGunpowderRifle", "startGunpowderAmmo",
							"startGunpowderAmmo", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startHoe", "startHoe", "startGoldPot", "startJerky", "startJerky",
							"startJerky", "startJerky", "startJerky", "startJerky", "startJerky", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack",
							"startHardtack", "startHardtack", "startHardtack"
						}
					},
					new Option
					{
						KeyName = "huntingExpedition",
						Name = "Hunting",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogueHunting", "introDialogueScreen" },
						ActionKeys = new string[43]
						{
							"setStartingLocationNorthArableLand", "exploreShroudNorthArableLand", "startNaturalTerminalGenericPosition", "spawnCastor", "spawnLinsey", "spawnHuntingSpecialist1", "spawnHuntingSpecialist1", "spawnSecuritySpecialist1", "spawnCookingSpecialist2", "spawnDog",
							"spawnDog", "startRadioAntenna", "startRadio", "startString", "startKnife", "startKnife", "startMachete", "startGunpowderRifle", "startGunpowderAmmo", "startBoltActionRifle",
							"startBoltActionRifle", "startBoltActionAmmo", "startBoltActionAmmo", "startBoltActionAmmo", "startSpikeTrap", "startSpikeTrap", "startTurnipCracker", "startCookingPot", "startJerky", "startJerky",
							"startJerky", "startJerky", "startJerky", "startJerky", "startJerky", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack",
							"startHardtack", "startHardtack", "startHardtack"
						}
					},
					new Option
					{
						KeyName = "fishingExpedition",
						Name = "Fishing",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogueFishing", "introDialogueScreen" },
						ActionKeys = new string[52]
						{
							"setStartingLocationRiverBank", "exploreShroudRiverBank", "startNaturalTerminalFishingPosition", "spawnCastor", "spawnLinsey", "spawnHuntingSpecialist1", "spawnBushcraftSpecialist1", "spawnSecuritySpecialist1", "spawnCookingSpecialist2", "spawnDog",
							"spawnHaulRobot", "startRadioAntenna", "startRadio", "startString", "startString", "startString", "startKnife", "startKnife", "startKnife", "startIronHandAxe",
							"startGunpowderRifle", "startGunpowderAmmo", "startGunpowderAmmo", "startImprovisedBow", "startIronArrow", "startFishTrapHoopNet", "startCottonString", "startCottonString", "startString", "startIronHooks",
							"startIronHooks", "startNeonHornetsLive", "startPigFliesLive", "startBugNet", "startIronSpear", "startPickaxe", "startImprovisedCookingPot", "startJerky", "startJerky", "startJerky",
							"startJerky", "startJerky", "startJerky", "startJerky", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack",
							"startHardtack", "startHardtack"
						}
					},
					new Option
					{
						KeyName = "bushcraftExpedition",
						Name = "Bushcraft",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogueThrong", "introDialogueScreen" },
						ActionKeys = new string[31]
						{
							"setStartingLocationNorthArableLand", "exploreShroudNorthArableLand", "startNaturalTerminalGenericPosition", "spawnCastor", "spawnLinsey", "spawnHuntingSpecialist1", "spawnBushcraftSpecialist1", "spawnMenialSpecialist1", "spawnSecuritySpecialist1", "spawnDog",
							"startRadioAntenna", "startRadio", "startMetalWire", "startString", "startShadeleafResin", "startRawhideString", "startRawhideString", "startRawhideString", "startKnife", "startKnife",
							"startKnife", "startFlintKnife", "startIronHandAxe", "startMachete", "startGunpowderRifle", "startGunpowderAmmo", "startGunpowderAmmo", "startSteelSpade", "startGoldPot", "startClayPotUnglazed",
							"startHardtack"
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
						ActionKeys = new string[40]
						{
							"setFishSchoolMedium", "setNormalResources", "startFarmSpotSmall1", "startFarmSpotSmall2", "startFarmSpotSmall4", "startFarmSpotSmall5", "startFarmSpotSmall7", "startFarmSpotSmall8", "startFarmSpotSmall9", "startFarmSpotSmall11",
							"startFarmSpotSmall12", "startFarmSpotSmall13", "startFarmSpotSmall14", "startFarmSpotLarge1", "startFarmSpotLarge2", "startFarmSpotLarge4", "startFishTrapCreek2", "startFishTrapCreek5", "startFishTrapCreek6", "startFishTrapCreek7",
							"startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4", "startFishTrapShore6", "startFishTrapShore7", "startFishTrapShore8", "startFishTrapShore17", "startFishTrapShore18", "startFishTrapShore19",
							"startBogOreDeposit1", "startBogOreDeposit2", "startPeatDeposit1", "startPeatDeposit2", "startPeatDeposit3", "startSaltDeposit1", "startClayDeposit1", "startClayDeposit2", "startClayDeposit3", "startClayDeposit4"
						}
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
