using UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3;

public class Scenario3Loader : ScenarioLoader
{
	public override string FolderName => "TUTORIAL - Castaways";

	protected override Scenario InitScenarioHeader()
	{
		return new Scenario
		{
			Name = FolderName,
			DisplayName = "Castaways",
			TimeDateYear = new DateAndTime.TimeDateYear
			{
				Year = 120,
				Day = 1,
				TimeOfDay = 0.31
			},
			MapKey = "i Tutorial Island",
			MapSize = MapSize.Small,
			SummaryDescription = "TUTORIAL 1: In a primitive future, a crew of sailors are cast ashore on an island and must find food and signal for help. \n \nERA: The Great Descent",
			Description = "TUTORIAL NO. 1 \nPlaythrough time 45-90 min \n \nCenturies after planetfall, the human colony on Antheia has regressed and lost most of the knowledge and technology the pioneers brought to the planet. Still, some artifacts remain - among them, the PPU: An instrument used by the pioneers for analyzing the alien environment and planning a frontier colony. \nNow, three castaways must learn to use the PPU to survive on an uncharted island.",
			Allow32Bit = true,
			ThumbnailImage = "Scenarios/Scenario 3/Scenario Screen/catamaranWreck_scenario_thumb",
			Image = "BoatStorm",
			IsInDevelopment = false,
			SortOrder = 0
		};
	}

	public override DataLoader GetDataLoader()
	{
		return new Scenario3DataLoader
		{
			FolderName = FolderName
		};
	}

	protected override ScenarioData InitScenarioData()
	{
		ScenarioData scenarioData = new ScenarioData();
		scenarioData.LoadingBackgroundImage = "Scenarios/Scenario 3/Scenario Screen/TitleImgBoat_1920px";
		scenarioData.LoadingDialogText = "Our people are descendants of the pioneers who came to Antheia two centuries ago. They were great minds, highly skilled in every way. Even though we lost their might in the Great Descent, we still practice the customs of the pioneers. We respect our ancestors - they were not to blame for the tragedy that happened. We try to follow their example and make the best use of their technology that remains. But we are a simple people - farmers, hunters and sailors - and have to make our own way of life on planet Antheia.";
		scenarioData.LoadingDialogImage = "NightTime";
		scenarioData.SpawnWorldAction = "spawnWorld";
		scenarioData.SpawnSiteAction = "spawnPlaySite";
		scenarioData.EnableMissions = false;
		scenarioData.EnableGraphs = false;
		scenarioData.EnableContacts = false;
		scenarioData.EnablePersonell = false;
		scenarioData.EnableWorldMap = false;
		scenarioData.EnablePolicy = false;
		scenarioData.EnableLedger = false;
		scenarioData.ConditionalEvents = new string[17]
		{
			"TUTORIAL_musicTrackList", "TUTORIAL_placeCatamaran", "TUTORIAL_placeTerritoryLock1", "TUTORIAL_placeTerritoryLock2", "TUTORIAL_introDialogue", "TUTORIAL_scoutDecision", "TUTORIAL_timedSpawnBeginningPopulation", "TUTORIAL_triggerBeforeCrevice", "TUTORIAL_3commonOilTubers", "TUTORIAL_checkcommonOilTubersGatheredDelay",
			"TUTORIAL_ReadyToBuildCampfire", "TUTORIAL_ReadyToCookcommonOilTubers", "TUTORIAL_3Flint3WaterCaneStems", "TUTORIAL_3SpearsFinished", "TUTORIAL_triggerBeforeBushDragonFight", "TUTORIAL_triggerAfterBushDragonFight", "TUTORIAL_Rescue_3Left"
		};
		scenarioData.Actions = new string[26]
		{
			"spawnBushDragonTutAllegianceNorth", "spawnBushDragonTutAllegianceSouth", "setStartingLocationSouth", "exploreShroudSouth", "spawnHarron", "spawnSantilla", "spawnScoyd", "placeCrevice", "initGorgeDetected", "initCampfireFinished",
			"initspearReadyToBeMade", "initFlintSpearMade", "initSignalPyreLit", "initGameOver", "initIncludeDateInBurial", "initBurialText1", "initBurialText2", "initBurialText3", "initBurialText4", "initBurialText5",
			"placeExpedition", "setView", "startKnife", "startCookingPot", "setSparseResources", "spawnPlayerAllegiance"
		};
		scenarioData.MainDifficultySettings = new Difficulty[1]
		{
			new Difficulty
			{
				KeyName = "normal",
				Name = "Normal",
				IsDefault = true,
				OptionsToUse = new SerializableDictionary<string, string[]>()
			}
		};
		scenarioData.OptionSets = new OptionSet[3]
		{
			new OptionSet
			{
				KeyName = "startingLocations",
				Name = "Crash site",
				DisplayGroup = 0,
				Options = new Option[1]
				{
					new Option
					{
						KeyName = "southCoast",
						Name = "Grassland shore SW",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Coastline",
							ScoreModifier = 1f
						}
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
						KeyName = "3people",
						Name = "3 people",
						ShortDescription = "Group of 3 people",
						LoadingDialogText = "\nGROUP OF 3 PEOPLE TEXT",
						LoadingDialogTextMode = Option.LoadingDialogTextModes.Append,
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							IsDefault = true,
							Name = "3 people",
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[1] { "DEMOISLANDMAP_introDialogue3People" },
						ActionKeys = new string[3] { "spawnConlan", "spawnLehner", "spawnKahn" }
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
						KeyName = "castawaysGear",
						Name = "Castaways' gear",
						Difficulty = new CustomDifficulty
						{
							KeyName = "normal",
							Name = "Some gear",
							IsDefault = true,
							ScoreModifier = 1f
						},
						ConditionalEvents = new string[2] { "DEMOISLANDMAP_introDialogueSomeSupplies", "DEMOISLANDMAP_introSuggestionSomeSupplies" },
						ActionKeys = new string[2] { "startKnife", "startCookingPot" }
					}
				}
			}
		};
		return scenarioData;
	}
}
