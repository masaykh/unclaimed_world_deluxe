using UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6;

public class Scenario6Loader : ScenarioLoader
{
	public override string FolderName => "The Clay Pit";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "The Clay Pit",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 193,
				Day = 2,
				TimeOfDay = 0.5
			},
			MapKey = "n Clay River Bank",
			MapSize = MapSize.Small,
			Allow32Bit = true,
			SummaryDescription = "TUTORIAL 2 + OPEN-ENDED: A work crew signs up for making mudbricks in a clay pit. Their temporary camp might turn into a settlement. \n \nERA: The Great Descent",
			Description = "TUTORIAL NO. 2 + OPEN-ENDED \n \n-You must all be hard up for money, else you wouldn't be interested in this... It's back breaking labor, digging clay and making mudbricks. The site is a short trip up the river. Questions? \n-When do we get paid and how much? \n-The contract ends in a couple of months. Then we'll split the profit evenly and head back to Tellus. \n-What about food? \n-When we sell our first boatload of mudbricks we'll buy provisions for some of the money. Now, who wants to sign up?",
			ThumbnailImage = "Scenarios/Scenario 6/Scenario Screen/clayPit_scenario_thumb",
			Image = "GroupMeeting",
			IsInDevelopment = false,
			SortOrder = 1
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario6DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Scenario 4/Scenario Screen/TitleImgManFence_1920px";
		scenarioData.LoadingDialogText = " \n-Hey. I heard there's patricians close to the clay pit? \n-Yeah, they have their territory at Blue Creek. But we just stay away from them. We'll probably put up a fence also. Don't be scared!";
		scenarioData.LoadingDialogImage = "RiverBoat";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_5";
		scenarioData.CustomEnabled = false;
		scenarioData.ConditionalEvents = new string[11]
		{
			"CLAYPIT_introDialogue", "triggerAtFence", "placeTerritoryLockFence", "CONTRACTEND_40mudBricksStored", "SANDBOXNOMADMAP_musicTrackList", "Scenario6_winGame", "Scenario6_loseContract", "SANDBOXMAP_loseGame", "initializeGlobalFarmingProperties", "initializeGlobalFishTrapProperties",
			"initializeGlobalAnimalTrapProperties"
		};
		scenarioData.Actions = new string[96]
		{
			"initStoryPartOver", "initStores40Mudbricks", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initEndDate", "initEnableGroupMeetings", "initTimeBeforeGroupMeeting",
			"meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy", "meeting3Security", "meeting3Food", "meeting3Comfort", "meeting2Security", "meeting2Food", "meeting2Comfort", "meetingSecurityAllUnhappy", "meetingFoodAllUnhappy",
			"meetingComfortAllUnhappy", "comfortBasicPolicyAdopted", "foodBasicPolicyAdopted", "securityBasicPolicyAdopted", "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted",
			"comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree", "securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree", "initEmigrateSecurityDialogText",
			"initEmigrateComfortDialogText", "initEmigrateFoodDialogText", "initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText", "spawnMudWormExpedition#1", "spawnMudWormExpedition#2", "spawnPatricianExpedition#1", "spawnThunderChickenExpedition#2", "spawnBirdExpedition#1",
			"spawnBinalRatExpedition#1", "spawnBinalRatExpedition#2", "spawnBinalRatExpedition#3", "spawnPlayerAllegiance", "placeExpedition", "setView", "exploreShroud", "setPlayerCredits", "startNaturalTerminal", "spawnOtherSite1",
			"spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1", "spawnPlaySiteSite1Route", "spawnPlaySiteSite1LandRoute", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist",
			"spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1MenialSpecialist", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1Random", "spawnImmigrantOtherSite1SmithingSpecialist", "startStructureLean-toSpoakLeaves1",
			"startStructureLean-toSpoakLeaves2", "startStructureA-frameSpoakLeaves1", "startStructureCampfire", "startStructureCanopyPort", "startStructureAbatis1", "startStructureAbatis2", "startStructureAbatis3", "startStructureAbatis4", "startStructureAbatis5", "startStructureAbatis6",
			"startBricksStockpile", "startPierSpot1", "smallFog1", "smallFog2", "smallFog3", "fog1"
		};
		scenarioData.MainDifficultySettings = new Difficulty[2]
		{
			new Difficulty
			{
				KeyName = "easy",
				Name = "Tutorial",
				Description = "Learn the game by reaching the objective with help from instructions and hints along the way.",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "mudBrickExpedition" }
					},
					{
						"tutorial",
						new string[1] { "tutorialOn" }
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
				Name = "No tutorial",
				Description = "Reach the objective without any instructions and hints.",
				IsDefault = false,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "mudBrickExpedition" }
					},
					{
						"tutorial",
						new string[1] { "tutorialOff" }
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
		scenarioData.OptionSets = new OptionSet[5]
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
							Name = "Average",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "CLAYPIT_triggerMudWormsWest", "CLAYPIT_triggerMudWormsCenter" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "tutorial",
				Name = "Tutorial",
				DisplayGroup = 2,
				Options = new Option[2]
				{
					new Option
					{
						KeyName = "tutorialOn",
						Name = "Tutorial On",
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "With tutorial",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setTutorialOn" },
						ConditionalEvents = new string[4] { "CLAYPIT_introScreenTut", "TUTORIAL_check40MudBricks", "TUTORIAL_40mudBricksStored", "TUTORIAL_checkIfContinueGame" }
					},
					new Option
					{
						KeyName = "tutorialOff",
						Name = "Tutorial Off",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Without tutorial",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setTutorialOff" },
						ConditionalEvents = new string[1] { "CLAYPIT_introScreenNoTut" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "expeditionType",
				Name = "Expedition type",
				DisplayGroup = 0,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "mudBrickExpedition",
						Name = "Mud brick makers",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ActionKeys = new string[42]
						{
							"setStartingLocation", "spawnMenialSpecialist1", "spawnMenialSpecialist2", "spawnMenialSpecialist4", "spawnMenialSpecialist5", "spawnMenialSpecialist6", "spawnMenialSpecialist7", "spawnMenialSpecialist8", "spawnMenialSpecialist9", "startPickaxe",
							"startPickaxe", "startSteelSpade", "startSteelSpade", "startSteelSpade", "startSteelSpade", "startSteelSpade", "startKnife", "startGoldPot", "startBrickMold", "startRawhideString",
							"startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken",
							"startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startSmokedThunderChicken", "startCommonOilTubers", "startCommonOilTubers", "startCommonOilTubers", "startCommonOilTubers", "startCommonOilTubers", "startCommonOilTubers",
							"startCommonOilTubers", "startCommonOilTubers"
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
						ActionKeys = new string[8] { "setFishSchoolMedium", "setNormalResources", "startFishTrapCreek1", "startFishTrapCreek2", "startFishTrapCreek3", "startClayDeposit1", "startBogOreDeposit1", "startPeatDeposit1" }
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
