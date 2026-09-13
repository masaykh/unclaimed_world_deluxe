using UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1;

public class Scenario1Loader : ScenarioLoader
{
	public override string FolderName => "Twinkler Island";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Twinkler Island",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 0,
				Day = 1,
				TimeOfDay = 0.36
			},
			MapKey = "fx DemoIsland",
			MapSize = MapSize.Small,
			SummaryDescription = "SCENARIO: After escaping a deadly swarm, a handful of PRECOL explorers crash-land on an island and must survive until help arrives. \n \nERA: Planetfall",
			Description = "SCENARIO. Playthrough 2-3 hrs. \n \n'AUDIO LOG: We have escaped the catastrophic attack by quadites that occurred just hours ago at Colony #1. We managed to get away in an aircraft but after a short flight we were forced to crash-land on a nearby island. \nWe've landed in a firegrass biome. Geographical data are incomplete, but we know enough about this environment to expect large numbers of quadites.'",
			Allow32Bit = true,
			ThumbnailImage = "Scenarios/Scenario 1/Scenario Screen/aircraftWreck_scenario_thumb",
			Image = "Survival",
			SortOrder = 3,
			IsInDevelopment = false
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario1DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Default/Scenario Screen/TitleImgSurvival_1920px";
		scenarioData.LoadingDialogImage = "FleeingSkimmer";
		scenarioData.LoadingDialogHeading = "JOURNAL";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.WorldMapImage = "Scenarios/Default/GUI/regionalMap_4";
		scenarioData.EnableMissions = false;
		scenarioData.ConditionalEvents = new string[21]
		{
			"DEMOISLANDMAP_musicTrackList", "DEMOISLANDMAP_winGame", "DEMOISLANDMAP_minorWinGame", "DEMOISLANDMAP_checkIfOnlyOneMemberLeft", "DEMOISLANDMAP_checkIfOnlyTwoMembersLeft", "DEMOISLANDMAP_checkIfOnlyThreeMembersLeft", "DEMOISLANDMAP_loseGame", "checkBushDragonDetectedShortDelay", "checkBushDragonDetectedLongDelay", "DEMOISLANDMAP_midnightText",
			"DEMOISLANDMAP_rescueAndPRECOLDialogue", "DEMOISLANDMAP_triggerVolcanicLandscape", "DEMOISLANDMAP_triggerMuckroot", "initializeGlobalFarmingProperties", "initializeGlobalFishTrapProperties", "initializeGlobalAnimalTrapProperties", "DEMOISLANDMAP_continualSpawnBinalRatsSouthWest", "DEMOISLANDMAP_continualSpawnBinalRatsCenterEast", "DEMOISLANDMAP_continualSpawnBinalRatsNorth", "DEMOISLANDMAP_continualSpawnThunderChickensNorth",
			"DEMOISLANDMAP_continualSpawnThunderChickensSouth"
		};
		scenarioData.Actions = new string[77]
		{
			"DEMOISLANDMAP_spawnTwinklerAllegiance", "DEMOISLANDMAP_spawnThunderChickenAllegianceNorth", "DEMOISLANDMAP_spawnThunderChickenAllegianceSouth", "DEMOISLANDMAP_thinThunderChickenAllegianceSouth", "DEMOISLANDMAP_thinThunderChickenAllegianceNorth", "spawnWildernessSite1", "spawnWildernessSite1Allegiance1", "spawnWildernessSite1Expedition1", "spawnPlaySiteWildernessSite1Route", "naturalTerminalSW",
			"naturalTerminalN", "initSulfurDetected", "initNestDetected", "initNestDestroyed", "initBushDragonDetectedShortDelay", "initBushDragonDetectedLongDelay", "initFieldLabCannibalized", "initGameOver", "initIncludeDateInBurial", "initBurialText1",
			"initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5", "initEnableGroupMeetings", "initTimeBeforeGroupMeeting", "meetingEmigrateThreat", "meetingEmigrateThreatAllUnhappy", "meeting3Security", "meeting3Food",
			"meeting3Comfort", "meeting2Security", "meeting2Food", "meeting2Comfort", "meetingSecurityAllUnhappy", "meetingFoodAllUnhappy", "meetingComfortAllUnhappy", "comfortBasicPolicyAdopted", "foodBasicPolicyAdopted", "securityBasicPolicyAdopted",
			"comfortMediumPolicyAdopted", "foodMediumPolicyAdopted", "securityMediumPolicyAdopted", "comfortAdvancedPolicyAdopted", "foodAdvancedPolicyAdopted", "securityAdvancedPolicyAdopted", "comfortBasicPolicyAdoptedAllAgree", "foodBasicPolicyAdoptedAllAgree", "securityBasicPolicyAdoptedAllAgree", "comfortMediumPolicyAdoptedAllAgree",
			"foodMediumPolicyAdoptedAllAgree", "securityMediumPolicyAdoptedAllAgree", "comfortAdvancedPolicyAdoptedAllAgree", "foodAdvancedPolicyAdoptedAllAgree", "securityAdvancedPolicyAdoptedAllAgree", "initEmigrateSecurityDialogText", "initEmigrateComfortDialogText", "initEmigrateFoodDialogText", "initEmigrateSecurityNoConversationDialogText", "initEmigrateComfortNoConversationDialogText",
			"initEmigrateFoodNoConversationDialogText", "initDeathCounter", "placeExpedition", "setView", "destroySkimmerHull", "destroySkimmerEngineSide", "destroySkimmerEngineTop", "destroySkimmerTail", "blackSmoke", "smallFog1",
			"smallFog2", "smallFog3", "sulphurousSmoke1", "sulphurousSmoke2", "haze1", "haze2", "spawnPlayerAllegiance"
		};
		scenarioData.MainDifficultySettings = new Difficulty[6]
		{
			new Difficulty
			{
				KeyName = "veryEasy",
				Name = "Very easy"
			},
			new Difficulty
			{
				KeyName = "easy",
				Name = "Normal (Recommended start)",
				IsDefault = true,
				Description = "On NORMAL difficulty, the explorers have crashed in a relatively safe corner of the island. (The start location is randomized). \nThey have enough weapons and equipment to see them through for a while but will have to improvise while waiting for rescue.",
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"startingLocations",
						new string[2] { "southCoast", "northCoast" }
					},
					{
						"people",
						new string[1] { "4people" }
					},
					{
						"equipment",
						new string[1] { "oneSentrySomeGear" }
					},
					{
						"fauna",
						new string[1] { "average" }
					},
					{
						"resources",
						new string[1] { "plenty" }
					},
					{
						"gameDuration",
						new string[1] { "medium" }
					}
				}
			},
			new Difficulty
			{
				KeyName = "normal",
				Name = "Hard",
				Description = "On HARD difficulty, the explorers crashed on top of a nest of dangerous predators and were forced to abandon an injured crew member and their gear. \nThey now prepare to go back, rescue him and if possible reclaim their equipment.",
				OptionsToUse = new SerializableDictionary<string, string[]>
				{
					{
						"startingLocations",
						new string[3] { "fledToSouthWestWreckAtSandstone", "fledToSouthWestWreckAtBramble", "fledToSouthEastWreckAtBramble" }
					},
					{
						"people",
						new string[1] { "4people" }
					},
					{
						"equipment",
						new string[1] { "oneShotgunSomeGear" }
					},
					{
						"fauna",
						new string[1] { "fierce" }
					},
					{
						"resources",
						new string[1] { "average" }
					},
					{
						"gameDuration",
						new string[1] { "medium" }
					}
				}
			},
			new Difficulty
			{
				KeyName = "hard",
				Name = "Hard"
			},
			new Difficulty
			{
				KeyName = "veryHard",
				Name = "Very hard"
			},
			new Difficulty
			{
				KeyName = "impossible",
				Name = "Impossible"
			}
		};
		scenarioData.OptionSets = new OptionSet[6]
		{
			new OptionSet
			{
				KeyName = "startingLocations",
				Name = "Camp location",
				DisplayGroup = 0,
				Options = new Option[5]
				{
					new Option
					{
						KeyName = "southCoast",
						Name = "Grassland shore SW",
						LoadingDialogText = " \nJOURNAL RECORDED BY: Ward Conlan \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n \nWe have escaped the catastrophic attack by quadites that happened just hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 2,
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Next to wreck",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[9] { "DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_triggerMarsh", "DEMOISLANDMAP_triggerNorthEastCloseRockCrevice", "DEMOISLANDMAP_triggerNorthRockCrevice", "DEMOISLANDMAP_checkQuaditeCarcassDetected", "DEMOISLANDMAP_checkQuaditeCarcassDetectedDelay", "DEMOISLANDMAP_checkNestDetectedDelay", "DEMOISLANDMAP_checkNestExpositionEventHasFired", "DEMOISLANDMAP_checkNestExpositionEventHasFiredAndSulfurSeen" },
						ActionKeys = new string[15]
						{
							"startSkimmerHullSouth", "startSkimmerEngineTopSouth", "startSkimmerEngineSideSouth", "startSkimmerTailSouth", "placeNestSandstone", "placeNestRockEast", "setStartingLocationSouth", "exploreShroudSouth", "initNoSandstoneWreckage", "initNotFledAtStart",
							"startFieldLabInSkimmer", "startBasicFireExtinguisherInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer"
						}
					},
					new Option
					{
						KeyName = "northCoast",
						Name = "Rocky river bank NE",
						LoadingDialogText = " \nJOURNAL RECORDED BY: Ward Conlan \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n \nWe have escaped the catastrophic attack by quadites that happened just hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 2,
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Next to wreck",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[9] { "DEMOISLANDMAP_beginningBushDragonPopulationWest", "DEMOISLANDMAP_triggerMarsh", "DEMOISLANDMAP_triggerNorthEastCloseRockCrevice", "DEMOISLANDMAP_triggerNorthRockCrevice", "DEMOISLANDMAP_checkQuaditeCarcassDetected", "DEMOISLANDMAP_checkQuaditeCarcassDetectedDelay", "DEMOISLANDMAP_checkNestDetectedDelay", "DEMOISLANDMAP_checkNestExpositionEventHasFired", "DEMOISLANDMAP_checkNestExpositionEventHasFiredAndSulfurSeen" },
						ActionKeys = new string[17]
						{
							"startSkimmerHullNorth", "startSkimmerEngineTopNorth", "startSkimmerEngineSideNorth", "startSkimmerTailSandstone", "placeCratesSandstone", "placeNestSandstone", "placeNestRockEast", "setStartingLocationNorth", "placeCratesSandstone", "exploreShroudNorth",
							"initSandstoneWreckage", "initNotFledAtStart", "startFieldLabInSkimmer", "startBasicFireExtinguisherInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer", "startEmptyCartridgeInSkimmer"
						}
					},
					new Option
					{
						KeyName = "fledToSouthWestWreckAtSandstone",
						Name = "Wreck at north",
						LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain - Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.7 km south of crash site.",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 2,
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Fled from wreck",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[5] { "DEMOISLANDMAP_beginningTwinklerPopulationSandstone", "DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies", "DEMOISLANDMAP_checkCasualtyRescued" },
						ActionKeys = new string[25]
						{
							"startSkimmerTailSandstone", "startSkimmerHullSandstone", "startSkimmerEngineTopSandstone", "startSkimmerEngineSideSandstone", "startFieldLabSandstone", "startRationSandstone", "startBasicFireExtinguisherSandstone", "startEmptyCartridgeSandstone", "startEmptyCartridgeSandstone", "startEmptyCartridgeSandstone",
							"startSentrySandstone", "startSentryWeaponMountSandstone", "startShotgunAmmoSandstone", "startShotgunAmmoSandstone", "placeNestSandstone", "placeNestRockEast", "setStartingLocationSouth", "exploreShroudFledSouthWestWreckAtSandstone", "exploreShroudFledSouthWestWreckAtSandstoneFlightPath", "placeUnconsciousAtSandstone",
							"setRescueSpawnLocationSandstone", "initUnconsciousAndAlive", "initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued"
						}
					},
					new Option
					{
						KeyName = "fledToSouthWestWreckAtBramble",
						Name = "Wreck at northeast",
						LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain - Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.8 km southwest of crash site.",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 2,
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Fled from wreck",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[5] { "DEMOISLANDMAP_beginningTwinklerPopulationBramble", "DEMOISLANDMAP_beginningBushDragonPopulationEast", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies", "DEMOISLANDMAP_checkCasualtyRescued" },
						ActionKeys = new string[29]
						{
							"startSkimmerHullBramble", "startSkimmerEngineTopBramble", "startSkimmerEngineSideBramble", "startSkimmerTailBramble", "startFieldLabBrambleEast", "startRationBrambleEast", "startBasicFireExtinguisherBrambleEast", "startEmptyCartridgeBrambleEast", "startEmptyCartridgeBrambleEast", "startEmptyCartridgeBrambleEast",
							"startSnipsBrambleEast", "startSentryBrambleEast", "startSentryWeaponMountBrambleEast", "startShotgunAmmoBrambleEast", "startShotgunAmmoBrambleEast", "startMacheteBrambleWest", "placeNestSandstone", "placeNestRockEast", "placeNestRockSouth", "placeNestRockNorth",
							"setStartingLocationSouth", "exploreShroudFledSouthWestWreckAtBramble", "exploreShroudWreckAtBrambleFlightPath", "placeUnconsciousAtBramble", "setRescueSpawnLocationBramble", "initUnconsciousAndAlive", "initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued"
						}
					},
					new Option
					{
						KeyName = "fledToSouthEastWreckAtBramble",
						Name = "Wreck at northwest",
						LoadingDialogText = " \n//PPU AUTO-GENERATED REPORT: \n \n00.00: Aircraft crash-landed - Severe damage prevents flight and communication. \nOne passenger lost during crash and isolated from team in inaccessible terrain -  Alive but unconscious. \n \n01.20: Dangerous species (Twinkler quadites) detected in immediate vicinity. \n02.00: Team members attacked by numerous Twinklers. \n09.50: Camp relocated 1.4 km southeast of crash site.",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 2,
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Fled from wreck",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[5] { "DEMOISLANDMAP_beginningTwinklerPopulationBramble", "DEMOISLANDMAP_beginningBushDragonPopulationWest", "DEMOISLANDMAP_dialogueRescueCountdown", "DEMOISLANDMAP_unconsciousDies", "DEMOISLANDMAP_checkCasualtyRescued" },
						ActionKeys = new string[27]
						{
							"startSkimmerHullBramble", "startSkimmerEngineTopBramble", "startSkimmerEngineSideBramble", "startSkimmerTailBramble", "startFieldLabBrambleEast", "startRationBrambleEast", "startBasicFireExtinguisherBrambleEast", "startEmptyCartridgeBrambleEast", "startEmptyCartridgeBrambleEast", "startEmptyCartridgeBrambleEast",
							"startSnipsBrambleEast", "startSentryBrambleEast", "startSentryWeaponMountBrambleEast", "startShotgunAmmoBrambleEast", "startShotgunAmmoBrambleEast", "startMacheteBrambleWest", "placeNestSandstone", "placeNestRockEast", "setStartingLocationSouthEast", "exploreShroudFledSouthEastWreckAtBramble",
							"exploreShroudWreckAtBrambleFlightPath", "placeUnconsciousAtBramble", "setRescueSpawnLocationBramble", "initUnconsciousAndAlive", "initSandstoneWreckage", "initFledAtStart", "initCasualtyRescued"
						}
					}
				}
			},
			new OptionSet
			{
				KeyName = "people",
				Name = "Camp members",
				DisplayGroup = 1,
				Options = new Option[3]
				{
					new Option
					{
						KeyName = "4people",
						Name = "4 people",
						ShortDescription = "Group of 4 people",
						LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner, Yeboah, Khan",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 1,
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							IsDefault = true,
							Name = "4 people",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[5] { "DEMOISLANDMAP_introDialogue3People", "DEMOISLANDMAP_introDialogue3PeopleFled", "DEMOISLANDMAP_onlyThreeMembersLeftDialogue", "DEMOISLANDMAP_onlyTwoMembersLeftDialogue", "DEMOISLANDMAP_onlyOneMemberLeftDialogue" },
						ActionKeys = new string[4] { "spawnConlan", "spawnLehner", "spawnKahn", "spawnYeboah" }
					},
					new Option
					{
						KeyName = "3people",
						Name = "3 people",
						ShortDescription = "Group of 3 people",
						LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner, Yeboah",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 1,
						Difficulty = new CustomDifficulty
						{
							KeyName = "hard",
							Name = "3 people",
							ScoreModifier = 1.5f
						},
						ConditionalEvents = new string[4] { "DEMOISLANDMAP_introDialogue3People", "DEMOISLANDMAP_introDialogue3PeopleFled", "DEMOISLANDMAP_onlyTwoMembersLeftDialogue", "DEMOISLANDMAP_onlyOneMemberLeftDialogue" },
						ActionKeys = new string[3] { "spawnConlan", "spawnLehner", "spawnYeboah" }
					},
					new Option
					{
						KeyName = "2people",
						Name = "2 people",
						ShortDescription = "Group of 2 people",
						LoadingDialogText = "//CAMP MEMBERS: Conlan, Lehner",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						LoadingDialogTextOrder = 1,
						Difficulty = new CustomDifficulty
						{
							KeyName = "veryHard",
							Name = "2 people",
							ScoreModifier = 2f
						},
						ConditionalEvents = new string[3] { "DEMOISLANDMAP_introDialogue2People", "DEMOISLANDMAP_introDialogue2PeopleFled", "DEMOISLANDMAP_onlyOneMemberLeftDialogue" },
						ActionKeys = new string[2] { "spawnConlan", "spawnLehner" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "equipment",
				Name = "Supplies",
				DisplayGroup = 0,
				Options = new Option[4]
				{
					new Option
					{
						KeyName = "lotsOfWeaponsSomeGear",
						Name = "Several sentries and firearms",
						Difficulty = new CustomDifficulty
						{
							KeyName = "veryEasy",
							Name = "Many firearms",
							ScoreModifier = 0.3f
						},
						ConditionalEvents = new string[2] { "DEMOISLANDMAP_introDialogueLotsOfSupplies", "DEMOISLANDMAP_introSuggestionLotsOfSupplies" },
						ActionKeys = new string[21]
						{
							"startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", "startHuntingRifle", "startSentry", "startSentry",
							"startShotgun", "startSentryGunAmmo", "startSentryGunAmmo", "startRifleAmmo", "startRifleAmmo", "startShotgunAmmo", "startMachete", "startSensor", "startRation", "startRation",
							"startGoggles"
						}
					},
					new Option
					{
						KeyName = "oneSentrySomeGear",
						Name = "Sentry + rifle",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Some gear",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies" },
						ActionKeys = new string[13]
						{
							"startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", "startHuntingRifle", "startRifleAmmo", "startMachete",
							"startSentry", "startSentryGunAmmo", "startGoggles"
						}
					},
					new Option
					{
						KeyName = "oneShotgunSomeGear",
						Name = "Shotgun + some gear",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Some gear",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies" },
						ActionKeys = new string[11]
						{
							"startKnife", "startKnife", "startKnife", "startSnips", "startString", "startString", "startThermalTarp", "startShotgun", "startShotgunAmmo", "startMachete",
							"startSensor"
						}
					},
					new Option
					{
						KeyName = "fewSupplies",
						Name = "Little equipment",
						Difficulty = new CustomDifficulty
						{
							KeyName = "hard",
							Name = "Little equipment",
							ScoreModifier = 1.5f
						},
						ConditionalEvents = new string[2] { "DEMOISLANDMAP_introDialogueFewSupplies", "DEMOISLANDMAP_introSuggestionFewSupplies" },
						ActionKeys = new string[2] { "startKnife", "startKnife" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "fauna",
				Name = "Fauna",
				DisplayGroup = 2,
				Options = new Option[3]
				{
					new Option
					{
						KeyName = "benign",
						Name = "Benign",
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Benign",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[7] { "DEMOISLANDMAP_continualSpawnThinThunderChickensSouth", "DEMOISLANDMAP_continualSpawnThinThunderChickensNorth", "DEMOISLANDMAP_timedSpawnBeginningPopulationEasy", "DEMOISLANDMAP_continualHunterSpawnSouthSandstoneCave", "DEMOISLANDMAP_continualHunterSpawnEastRockCave", "DEMOISLANDMAP_triggerHunterSouthSandstoneCave", "DEMOISLANDMAP_triggerHunterRockCaveEast" },
						ActionKeys = new string[9] { "setMaxThinThunderChickenLow", "setThinThunderChickenSpawnIntervalOften", "setThunderChickenSpawnIntervalHigh", "setMaxTwinklersLow", "setMaxThunderChickensHigh", "setMaxBinalRatsLow", "setBinalRatSpawnSeldom", "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastSeldom" }
					},
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
						ConditionalEvents = new string[8] { "DEMOISLANDMAP_continualSpawnThinThunderChickensSouth", "DEMOISLANDMAP_continualSpawnThinThunderChickensNorth", "DEMOISLANDMAP_timedSpawnBeginningPopulationNormal", "DEMOISLANDMAP_continualSpawnTwinklerSouthSandstoneCave", "DEMOISLANDMAP_continualSpawnTwinklerSouthRockCave", "DEMOISLANDMAP_continualSpawnTwinklerEastRockCave", "DEMOISLANDMAP_triggerGuardsSouthSandstoneCave", "DEMOISLANDMAP_triggerGuardsRockCaveEast" },
						ActionKeys = new string[9] { "setMaxThinThunderChickenNormal", "setThinThunderChickenSpawnIntervalOften", "setThunderChickenSpawnIntervalNormal", "setMaxTwinklersNormal", "setMaxThunderChickensNormal", "setMaxBinalRatsNormal", "setBinalRatSpawnOften", "setTwinklerSpawnIntervalSouthSeldom", "setTwinklerSpawnIntervalEastOften" }
					},
					new Option
					{
						KeyName = "fierce",
						Name = "Fierce",
						Difficulty = new CustomDifficulty
						{
							KeyName = "hard",
							Name = "Fierce",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[8] { "DEMOISLANDMAP_continualSpawnThinThunderChickensSouth", "DEMOISLANDMAP_continualSpawnThinThunderChickensNorth", "DEMOISLANDMAP_timedSpawnBeginningPopulationNormal", "DEMOISLANDMAP_continualSpawnTwinklerSouthSandstoneCave", "DEMOISLANDMAP_continualSpawnTwinklerSouthRockCave", "DEMOISLANDMAP_continualSpawnTwinklerEastRockCave", "DEMOISLANDMAP_triggerGuardsSouthSandstoneCave", "DEMOISLANDMAP_triggerGuardsRockCaveEast" },
						ActionKeys = new string[9] { "setMaxThinThunderChickenHigh", "setThinThunderChickenSpawnIntervalOften", "setThunderChickenSpawnIntervalLow", "setMaxTwinklersHigh", "setMaxThunderChickensNormal", "setMaxBinalRatsHigh", "setBinalRatSpawnOften", "setTwinklerSpawnIntervalSouthOften", "setTwinklerSpawnIntervalEastOften" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "resources",
				Name = "Resources",
				DisplayGroup = 2,
				Options = new Option[3]
				{
					new Option
					{
						KeyName = "plenty",
						Name = "Plenty",
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Plenty",
							ScoreModifier = 1f
						},
						ActionKeys = new string[9] { "setFishSchoolMedium", "setPlentyResources", "startFarmSpotSmall1", "startFishTrapCreek1", "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapShore1", "startFishTrapShore2", "startFishTrapShore3" }
					},
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
						ActionKeys = new string[7] { "setFishSchoolSparse", "setSparseResources", "startFarmSpotSmall1", "startFishTrapCreek1", "startFishTrapCoast1", "startFishTrapCoast2", "startFishTrapShore1" }
					},
					new Option
					{
						KeyName = "sparse",
						Name = "Sparse",
						Difficulty = new CustomDifficulty
						{
							KeyName = "hard",
							Name = "Sparse",
							ScoreModifier = 1f
						},
						ActionKeys = new string[5] { "setFishSchoolSparse", "setSparseResources", "startFarmSpotSmall1", "startFishTrapCoast1", "startFishTrapCoast2" }
					}
				}
			},
			new OptionSet
			{
				KeyName = "gameDuration",
				Name = "Duration",
				DisplayGroup = 3,
				Options = new Option[3]
				{
					new Option
					{
						KeyName = "short",
						Name = "Short",
						Difficulty = new CustomDifficulty
						{
							KeyName = "easy",
							Name = "Short",
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setWinGameEarly" }
					},
					new Option
					{
						KeyName = "medium",
						Name = "Medium",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Medium",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setWinGameMedium" }
					},
					new Option
					{
						KeyName = "long",
						Name = "Long",
						Difficulty = new CustomDifficulty
						{
							KeyName = "hard",
							Name = "Long",
							ScoreModifier = 1f
						},
						ActionKeys = new string[1] { "setWinGameLate" }
					}
				}
			}
		};
		return scenarioData;
	}
}
