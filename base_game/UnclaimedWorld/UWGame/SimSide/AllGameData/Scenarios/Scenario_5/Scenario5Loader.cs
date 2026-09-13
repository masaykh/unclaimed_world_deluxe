using UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5;

public class Scenario5Loader : ScenarioLoader
{
	public override string FolderName => "Making Headway";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Making Headway",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 180,
				Day = 5,
				TimeOfDay = 0.5
			},
			MapKey = "l Fjord",
			MapSize = MapSize.Small,
			Allow32Bit = true,
			SummaryDescription = "TUTORIAL 3 + OPEN-ENDED: A farm town is in decline when prices for farm produce go down. They make a plan to start manufacturing rubber with the help from chemists. \nERA: The Great Descent",
			Description = "TUTORIAL NO. 3 + OPEN-ENDED \n \nThe two friends nodded at each other as they crossed paths. They looked around their village. It had seen better days. \n-So, another farmer left this morning. \n-Can't blame him. Not much going on here anymore. \n-This place is dying! Why are people so set in their ways? \n-I think they want to improve this place. They just can't agree how. You have an idea? \n-Yes. We should do something useful with that swamp in our backyard. \n-Aaah..It's that rubber tapping you talked about years ago. \n-Yeah. Now is the time. Demand is up. \n- Hm. Ever since what happened...we always stayed away from the swamp. Besides, what do we know about producing rubber? \n-Well, I have a plan. I'm gonna make a case for it at the town meeting.",
			ThumbnailImage = "Scenarios/Scenario 5/Scenario Screen/parasolHouse_scenario_thumb",
			Image = "Farming",
			IsInDevelopment = false,
			SortOrder = 2
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario5DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px";
		scenarioData.LoadingDialogText = "Headway - Year 180 \n \nThe annual town meeting used to be a crowded affair, but these days, there was plenty of space for everyone. The old rules of order felt too formal for such a small group and the moderator tried to loosen up the meeting. \n-So. We all agree to try something new. We need to change tack if we want to reach the conditions they have up in Eden Plains. Now, we've heard Tereza's idea to set up a distillery and sell crystal brandy. John, you're next. \n-We should start tapping sap in the swamp...make rubber components. The mining operations down in Zenig Station are booming and they need this stuff for their machines. \nAt the mentioning of the word 'swamp', several people mumbled their disapproval. Though John Millet was ready to counter their arguments, the townspeople were divided when the meeting ended. His plan for rubber production was still up for debate.";
		scenarioData.LoadingDialogImage = "IndoorMeeting";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_3";
		scenarioData.EnableMissions = true;
		scenarioData.ConditionalEvents = new string[6] { "SANDBOXNOMADMAP_musicTrackList", "SANDBOXMAP_loseGame", "winGame", "initializeGlobalFarmingProperties", "initializeGlobalFishTrapProperties", "initializeGlobalAnimalTrapProperties" };
		scenarioData.Actions = new string[151]
		{
			"initGameOver", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initComfortTarget", "initFoodTarget", "initSecurityTarget", "initEnableGroupMeetings",
			"initTimeBeforeGroupMeeting", "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy", "meeting3Security", "meeting3Food", "meeting3Comfort", "meeting2Security", "meeting2Food", "meeting2Comfort", "meetingSecurityAllUnhappy",
			"meetingFoodAllUnhappy", "meetingComfortAllUnhappy", "comfortBasicPolicyAdopted", "foodBasicPolicyAdopted", "securityBasicPolicyAdopted", "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted",
			"securityAdvancedPolicyAdopted", "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree", "securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree",
			"initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText", "initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText", "spawnLeafcutterExpedition#1", "spawnThunderChickenExpedition#1", "spawnSnatcherExpedition#1", "spawnSlugExpedition#1",
			"spawnSwampDemonTreeExpedition#2", "spawnBirdExpedition#1", "spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2", "spawnBinalRatExpedition#3", "spawnTurnipExpeditionNorth", "spawnPlayerAllegiance", "placeExpedition", "setView", "exploreEntireMap",
			"setPlayerCredits", "startNaturalTerminal", "startStructureGreenhouse", "startStructureCompostPit", "startStructureCookhouse", "startStructureToolshed", "startStructureFirewoodStack", "startStructureClayGranary", "startStructureMeatDryingRack", "startStructureImprovisedWorkbench",
			"startStructureRadioHut", "startStructureCaneHut", "startStructureClayHut", "startStructureSimpleSmithy", "startStructureKiln", "startSmokeOven", "startStructureSimplePort", "startStructureFishTrapCoast2", "startStructureSmallPlot1", "startStructureSmallPlot2",
			"startStructureSmallPlot3", "startStructureSmallPlot4", "startStructureLargePlot1", "startBricksStockpile", "startFirewoodStockpile", "startMaterialsStockpile", "startUpgradeCookhouseStove", "startUpgradeSettingCookhouseStove", "startUpgradeCookhouseCommunityHall", "startUpgradeSettingCookhouseCommunityHall",
			"startUpgradeClayHut1Mats", "startUpgradeSettingClayHut1Beds", "startPierSpot1", "spawnWildernessSite1", "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1", "spawnPlaySiteWildernessSite1Route", "spawnOtherSite1", "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1",
			"spawnPlaySiteSite1Route", "spawnImmigrantOtherSite1SmithingSpecialist", "spawnImmigrantOtherSite1SmithingSpecialist", "spawnImmigrantOtherSite1SmithingSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random",
			"spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnOtherSite2", "spawnOtherSite2Allegiance1", "spawnOtherSite2Expedition1", "spawnPlaySiteSite2Route",
			"spawnImmigrantOtherSite2Chemist1", "spawnImmigrantOtherSite2Chemist2", "spawnImmigrantOtherSite2Chemist3", "spawnImmigrantOtherSite2MenialSpecialist", "spawnImmigrantOtherSite2MenialSpecialist", "spawnImmigrantOtherSite2MenialSpecialist", "spawnImmigrantOtherSite2MenialSpecialist", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random",
			"spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "spawnImmigrantOtherSite2Random", "smallFog1", "smallFog2", "fog1",
			"fog2", "fog3", "fog4", "fog5", "fog6", "fog7", "fog8", "fog9", "fog10", "fog11",
			"fog12"
		};
		scenarioData.MainDifficultySettings = new Difficulty[1]
		{
			new Difficulty
			{
				KeyName = "normal",
				Name = "Normal",
				Description = "Try to reach the objectives presented at the start of the game. \nSome tutorial windows will appear along the way, but following them is optional since the objective can be reached in many ways.",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "farmingExpedition" }
					},
					{
						"resources",
						new string[1] { "average" }
					}
				}
			}
		};
		scenarioData.OptionSets = new OptionSet[3]
		{
			new OptionSet
			{
				KeyName = "expeditionType",
				Name = "Expedition type",
				DisplayGroup = 0,
				Options = new Option[1]
				{
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
						ConditionalEvents = new string[3] { "introDialogue", "introDialogueScreen", "extrusionMachineArrivalCheck" },
						ActionKeys = new string[161]
						{
							"setStartingLocation", "spawnMillet", "spawnRains", "spawnFarmingSpecialist1", "spawnFarmingSpecialist2", "spawnConstructionSpecialist1", "spawnCookingSpecialist1", "spawnHaulRobot", "startSolidMudBrick", "startSolidMudBrick",
							"startSolidMudBrick", "startSolidMudBrick", "startSolidMudBrick", "startSolidMudBrick", "startSolidMudBrick", "startSolidMudBrick", "startSolidMudBrick", "startFirewood", "startFirewood", "startFirewood",
							"startFirewood", "startFirewood", "startFirewood", "startFirewood", "startFirewood", "startFirewood", "startFirewood", "startSpoakShingles", "startSpoakShingles", "startSpoakShingles",
							"startSticks", "startSticks", "startSticks", "startSticks", "startStones", "startStones", "startStones", "startRottenVegetables", "startRottenVegetables", "startRottenVegetables",
							"startRottenVegetables", "startRottenVegetables", "startRottenVegetables", "startRottenVegetables", "startRottenVegetables", "startRottenVegetables", "startSteelMachete", "startHoe", "startHoe", "startSteelSpade",
							"startPickaxe", "startIronHandAxe", "startTappingBucket", "startBugNet", "startIronHooks", "startBrickMold", "startImprovisedTrowel", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
							"startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
							"startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper",
							"startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startGlassyCreeper", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries",
							"startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startCrystalBerries", "startPickledCarbonTail", "startPickledCarbonTail", "startPickledCarbonTail", "startPickledCarbonTail",
							"startPickledCarbonTail", "startPickledCarbonTail", "startDriedSaltedStreakFin", "startDriedSaltedStreakFin", "startDriedSaltedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin",
							"startSmokedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin", "startSmokedStreakFin", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack", "startHardtack",
							"startHardtack", "startHardtack", "startHardtack", "startHardtack", "startKnife", "startKnife", "startKnife", "startMetalWire", "startMetalWire", "startMarshcotSap",
							"startShadeleafResin", "startBlacksmithsToolbox", "startBellows", "startMetalWorkersToolbox", "startCharcoal", "startCharcoal", "startWroughtIron", "startBlisterSteel", "startBlisterSteel", "startGoldPot",
							"startKnifeInKitchen", "startKnifeInKitchen", "startSalt", "startSalt", "startSalt", "startSalt", "startClayJar", "startClayJar", "startVinegar", "startVinegar",
							"startGunpowderRifle", "startGunpowderAmmo", "startGunpowderAmmo", "startImprovisedBow", "startIronArrow", "startCrystalWine", "startCrystalWine", "startCrystalWine", "startCrystalWine", "startCrystalWine",
							"startCrystalWine"
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
						ActionKeys = new string[19]
						{
							"setFishSchoolMedium", "setNormalResources", "startFarmSpotSmall1", "startFarmSpotSmall2", "startFarmSpotSmall3", "startFarmSpotSmall4", "startFarmSpotLarge1", "startFarmSpotSmall5", "startFarmSpotSmall6", "startFishTrapCoast1",
							"startFishTrapCoast2", "startFishTrapCoast3", "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startClayDeposit1", "startBogOreDeposit1", "startPeatDeposit1", "startSaltDeposit1"
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
