using UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7;

public class Scenario7Loader : ScenarioLoader
{
	public override string FolderName => "Muckroot Mining Site";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Muckroot Mining Site",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 21,
				Day = 3,
				TimeOfDay = 0.36
			},
			MapKey = "o Mountain Pass",
			MapSize = MapSize.Large,
			Allow32Bit = false,
			SummaryDescription = "OPEN-ENDED: A mining expedition extracts metals in a dangerous biome, using advanced equipment. The crew consider settling on the nearby grasslands. \nERA: The Great Descent",
			Description = "OPEN-ENDED MAP.\n \nSome decades after their disastrous arrival to Antheia, the pioneers have established a modest existence. \nThey are now further challenged by an approaching risk of solar flares. As protection, the pioneers have begun to construct large electromagnetic shields - a megaproject called CANOPY which requires huge amounts of resources. The project is managed by an A.I. overseer which commissions materials by setting prices and offering incentives. \n \nHowever, there's disagreement about the risk of solar eruptions. Therefore, many pioneers are reluctant to work for CANOPY and instead focus on building their own frontier settlements. Because of the restricted resources, these settlements use a mix of primitive and advanced technology.",
			ThumbnailImage = "Scenarios/Scenario 2/Scenario Screen/muckrootCamp_scenario_thumb",
			Image = "Survival",
			IsInDevelopment = false,
			SortOrder = 7
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario7DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Scenario 7/Scenario Screen/TitleImgTown1920";
		scenarioData.LoadingDialogText = "MUCKROOT PASS, YEAR 21 \n \n'Hey. See those? Behind that grey clump. There's more over there...' \nIrina looked out the window where Mike was pointing. \n'Woah, those creatures are fast...' \nThe Skimmer aircraft was approaching their destination. On the way in, they were flying above a 'muckroot' landscape: Grey-blue carpets of moss covered the ground between the hills. This was the territory of the ferocious Swarmer quadites. \nAll the miners in Irina's crew were staring at the animals below. Mike turned to Irina: \n'The terbium deposit is right in their neighborhood. Don't you think the swarmers are gonna be, well, swarming all over our operation?' \nIrina nodded. 'Yeah. But we'll land in a safe area to the south. We'll see what we do from there.' \n \nThe aircraft landed as the crew got ready to unload their gear.";
		scenarioData.LoadingDialogImage = "FleeingSkimmer";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_1";
		scenarioData.EnableMissions = true;
		scenarioData.ConditionalEvents = new string[5] { "SANDBOXNOMADMAP_musicTrackList", "SANDBOXMAP_loseGame", "initializeGlobalFarmingProperties", "initializeGlobalFishTrapProperties", "initializeGlobalAnimalTrapProperties" };
		scenarioData.Actions = new string[135]
		{
			"initGameOver", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy",
			"meeting3Security", "meeting3Food", "meeting3Comfort", "meeting2Security", "meeting2Food", "meeting2Comfort", "meetingSecurityAllUnhappy", "meetingFoodAllUnhappy", "meetingComfortAllUnhappy", "comfortBasicPolicyAdopted",
			"foodBasicPolicyAdopted", "securityBasicPolicyAdopted", "comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted", "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree",
			"securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree", "foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree", "initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText",
			"initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText", "initEmigrateFoodNoConversationDialogText", "spawnSwarmerExpedition1", "spawnSwarmerExpedition2", "spawnSwarmerAllegiance1", "spawnLeafcutterExpedition#1", "spawnBirdExpedition#1", "spawnBirdExpedition#2", "spawnBinalRatExpedition#1",
			"spawnBinalRatExpedition#2", "spawnSnatcherExpedition#1", "spawnThunderChickenExpedition#2", "spawnBushDragonExpedition#1", "spawnSlugExpedition#1", "spawnPatricianExpedition#1", "spawnTurnipExpeditionSouth", "spawnDemonTreeExpedition#1", "spawnSwampDemonTreeExpedition#2", "spawnAnimalMigrateTriggerWest",
			"spawnAnimalMigrateTriggerRiver", "spawnPlayerAllegiance", "placeExpedition", "setView", "setPlayerCredits", "exploreShroudNaturalTerminal", "spawnWildernessSite1", "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1", "spawnPlaySiteWildernessSite1Route",
			"spawnOtherSite1", "spawnOtherSite1Allegiance1", "spawnOtherSite1Expedition1", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier", "spawnImmigrantMediumTier",
			"spawnImmigrantAdvancedTier", "spawnImmigrantAdvancedTier", "spawnImmigrantAdvancedTier", "spawnImmigrantAdvancedTier", "spawnImmigrantAdvancedTier", "spawnOtherSite2", "spawnPlaySiteSite2Route", "spawnImmigrantMediumTier2", "spawnImmigrantMediumTier2", "spawnImmigrantMediumTier2",
			"spawnImmigrantMediumTier2", "spawnImmigrantMediumTier2", "spawnImmigrantAdvancedTier2", "spawnImmigrantAdvancedTier2", "spawnImmigrantAdvancedTier2", "startPierSpot1", "startPierSpot2", "startPierSpot3", "smallFog1", "smallFog2",
			"smallFog3", "smallFog4", "smallFog5", "smallFog6", "smallFog7", "smallFog8", "smallFog9", "smallFog10", "smallFog11", "smallFog12",
			"smallFog13", "smallFog14", "smallFog15", "sulphurousSmoke1", "sulphurousSmoke2", "sulphurousSmoke3", "sulphurousSmoke4", "haze1", "haze2", "haze3",
			"haze4", "fog1", "fog2", "fog3", "fog4", "fog5", "fog6", "fog7", "fog8", "fog9",
			"fog10", "fog11", "fog12", "fog13", "fog14"
		};
		scenarioData.MainDifficultySettings = new Difficulty[1]
		{
			new Difficulty
			{
				KeyName = "normal",
				Name = "Normal",
				Description = "Your crew of miners can choose to fulfill an order for metals or they can start a settlement however they want.",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"expeditionType",
						new string[1] { "northExpedition" }
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
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "northExpedition",
						Name = "north",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Specialized",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "introDialogue", "introDialogueScreen" },
						ActionKeys = new string[75]
						{
							"setStartingLocationNorth", "exploreShroudFromSouth", "startNaturalTerminalGenericPosition", "spawnNadova", "spawnMike", "spawnSecuritySpecialist2", "spawnCookingSpecialist1", "spawnMiningRobot", "spawnHaulRobot", "spawnHaulRobot2",
							"spawnGuardRobot", "startString", "startKnife", "startKnife", "startKnife", "startMachete", "startMachete", "startSpade", "startShotgun", "startShotgunAmmo",
							"startShotgunAmmo", "startShotgunAmmo", "startCoilRifle", "startCoilRifle", "startCoilRifle", "startCoilRifleAmmo", "startCoilRifleAmmo", "startCoilRifleAmmo", "startCoilRifleAmmo", "startCoilRifleAmmo",
							"startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation",
							"startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation", "startRation",
							"startRation", "startRation", "startRation", "startRation", "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", "startSimCoffeeBeans", "startFieldLabPacked",
							"startFieldKitchenStove", "startFieldKitchenEquipment", "startCookingPot", "startSensor", "startDomeTentItem", "startSmallTentItem", "startSmallTentItem", "startStructurePanels", "startStructurePanels", "startRefinery1",
							"startRefinery2", "startStructureGroundStation", "startLiquidGas", "startLiquidGas", "startLiquidGas"
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
						ActionKeys = new string[70]
						{
							"setFishSchoolMedium", "setNormalResources", "startRareMetalOreDepositCenter", "startRareMetalOreDepositNorth", "startFarmSpotSmall1", "startFarmSpotSmall2", "startFarmSpotSmall3", "startFarmSpotSmall4", "startFarmSpotSmall5", "startFarmSpotSmall6",
							"startFarmSpotSmall7", "startFarmSpotSmall8", "startFarmSpotSmall9", "startFarmSpotSmall10", "startFarmSpotSmall11", "startFarmSpotSmall12", "startFarmSpotSmall13", "startFarmSpotSmall14", "startFarmSpotSmall5", "startFarmSpotSmall16",
							"startFarmSpotSmall17", "startFarmSpotSmall18", "startFarmSpotLarge1", "startFarmSpotLarge2", "startFarmSpotLarge3", "startFishTrapCreek1", "startFishTrapCreek2", "startFishTrapCreek3", "startFishTrapCreek4", "startFishTrapCreek5",
							"startFishTrapCreek6", "startFishTrapCreek7", "startFishTrapCreek8", "startFishTrapCreek9", "startFishTrapCreek10", "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3", "startFishTrapShore4", "startFishTrapShore5",
							"startFishTrapShore6", "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapCoast3", "startFishTrapCoast4", "startFishTrapCoast5", "startFishTrapCoast6", "startFishTrapCoast7", "startBogOreDeposit1", "startBogOreDeposit2",
							"startBogOreDeposit3", "startBogOreDeposit4", "startPeatDeposit1", "startPeatDeposit2", "startPeatDeposit3", "startPeatDeposit4", "startPeatDeposit5", "startPeatDeposit6", "startPeatDeposit7", "startPeatDeposit8",
							"startPeatDeposit9", "startSaltDeposit2", "startSaltDeposit3", "startSaltDeposit4", "startSaltDeposit5", "startClayDeposit1", "startClayDeposit2", "startClayDeposit3", "startClayDeposit4", "startClayDeposit5"
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
