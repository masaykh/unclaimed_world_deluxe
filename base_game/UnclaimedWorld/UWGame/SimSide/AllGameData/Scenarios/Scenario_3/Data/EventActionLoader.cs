using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Systems;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double num = 0.5;
		list.Add(new SetPropertyAction
		{
			KeyName = "initIncludeDateInBurial",
			PropertyKey = "includeDateInBurialHeader",
			Value = new ValueNode
			{
				Bool = true
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText1",
			PropertyKey = "burialTextStart",
			Value = new ValueNode
			{
				String = "Location: 4° 12' 22'' South, 7° 12' 19.2'' West \nJournal entry #2 \n#JOURNALNAMES \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText2",
			PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial a eulogy was delivered by #EUOLOGYGIVER. The speech is included as an audio file. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers. Rest in peace."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText4",
			PropertyKey = "burialTextSingleDeathSingleSurvivor",
			Value = new ValueNode
			{
				String = "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText5",
			PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
			Value = new ValueNode
			{
				String = "#NAMEOFDECEASED is also dead now. \nI buried the remains as best I could. Guess it's only me now..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initGorgeDetected",
			PropertyKey = "gorgeDetected",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initCampfireFinished",
			PropertyKey = "campfireFinished",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initspearReadyToBeMade",
			PropertyKey = "spearReadyToBeMade",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initFlintSpearMade",
			PropertyKey = "flintSpearMade",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initSignalPyreLit",
			PropertyKey = "signalPyreLit",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initGameOver",
			PropertyKey = "gameOver",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnHarron",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-30f, 68f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Celoy",
					LastName = "Harron",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_m_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 52.0
					},
					CasteKey = "male",
					RaceKey = "greenBlueClothes1",
					StomachContent = new NormalDistribution
					{
						Mean = 0.0
					},
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.9f },
						{ "hunting", 1f },
						{ "butchering", 0.8f },
						{ "fishing", 1f },
						{ "foraging", 1f },
						{ "cooking", 0.8f },
						{ "menial", 0.7f },
						{ "shooting", 1f },
						{ "armedMelee", 1f },
						{ "unarmedFighting", 0.8f },
						{ "psychology", 0.1f },
						{ "biology", 0.08f },
						{ "farming", 0.8f },
						{ "weeding", 0.8f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "smithing", 0.5f },
						{ "mechanics", 0.2f },
						{ "electronics", 0.08f },
						{ "chemistry", 0.05f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f },
						{ "medicine", 0.5f },
						{ "carpentry", 0.5f },
						{ "weaving", 0.5f }
					}
				},
				NeedLevels = new SerializableDictionary<string, NeedData>
				{
					{
						"protein",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.30000001192092896
							}
						}
					},
					{
						"foodEnergy",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"micronutrients",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"stimulants",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					}
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnSantilla",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-44f, 96f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Javin",
					LastName = "Santilla",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_h_m_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 44.0
					},
					CasteKey = "male",
					RaceKey = "greyClothes1",
					StomachContent = new NormalDistribution
					{
						Mean = 0.0
					},
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.9f },
						{ "hunting", 0.7f },
						{ "butchering", 0.4f },
						{ "fishing", 0.5f },
						{ "foraging", 0.5f },
						{ "cooking", 0.5f },
						{ "menial", 0.7f },
						{ "shooting", 1f },
						{ "armedMelee", 0.8f },
						{ "unarmedFighting", 0.9f },
						{ "medicine", 0.6f },
						{ "psychology", 0.8f },
						{ "biology", 0.1f },
						{ "smithing", 0.5f },
						{ "mechanics", 0.1f },
						{ "electronics", 0.07f },
						{ "chemistry", 0.05f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f },
						{ "farming", 0.8f },
						{ "weeding", 0.8f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "carpentry", 0.5f },
						{ "weaving", 0.5f }
					}
				},
				NeedLevels = new SerializableDictionary<string, NeedData>
				{
					{
						"protein",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.30000001192092896
							}
						}
					},
					{
						"foodEnergy",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"micronutrients",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"stimulants",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					}
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnScoyd",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-94f, 74f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Taryn",
					LastName = "Scoyd",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_a_m_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 38.0
					},
					CasteKey = "male",
					RaceKey = "greenGreyClothes1",
					StomachContent = new NormalDistribution
					{
						Mean = 0.0
					},
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.9f },
						{ "hunting", 0.7f },
						{ "butchering", 0.6f },
						{ "fishing", 0.5f },
						{ "foraging", 0.5f },
						{ "cooking", 0.9f },
						{ "menial", 0.8f },
						{ "shooting", 1f },
						{ "armedMelee", 0.8f },
						{ "unarmedFighting", 0.9f },
						{ "medicine", 0.3f },
						{ "psychology", 0.1f },
						{ "biology", 0.06f },
						{ "smithing", 0.5f },
						{ "mechanics", 0.1f },
						{ "electronics", 0.05f },
						{ "chemistry", 0.02f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f },
						{ "farming", 0.8f },
						{ "weeding", 0.8f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "carpentry", 0.5f },
						{ "weaving", 0.5f }
					}
				},
				NeedLevels = new SerializableDictionary<string, NeedData>
				{
					{
						"protein",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.30000001192092896
							}
						}
					},
					{
						"foodEnergy",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"micronutrients",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					},
					{
						"stimulants",
						new NeedData
						{
							Level = new NormalDistribution
							{
								Mean = 0.20000000298023224
							}
						}
					}
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "placeExpedition",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "Camp",
				Name = "Camp",
				AllegianceKey = "playerAllegiance",
				Location = new ValueNode
				{
					PropertyKey = "startingLocation"
				}
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationSouth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(3056f, 3890f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(3024f, 3216f)
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setFishSchoolMedium",
			ResourceType = new string[13]
			{
				"streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing",
				"crestedFoiler", "goldenCenobite", "muckGrinder"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAmplitude = 1.5f,
				NoiseAddend = -1f,
				NoiseFrequency = 0.035f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setFishSchoolSparse",
			ResourceType = new string[13]
			{
				"streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing",
				"crestedFoiler", "goldenCenobite", "muckGrinder"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAmplitude = 1.5f,
				NoiseAddend = -2f,
				NoiseFrequency = 0.035f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setPlentyResources",
			AllResources = true,
			ExcludeResourceTypes = new string[5] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" },
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = 0.2f,
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setNormalResources",
			AllResources = true,
			ExcludeResourceTypes = new string[5] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" },
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setSparseResources",
			AllResources = true,
			ExcludeResourceTypes = new string[5] { "stones", "firewood", "flint", "crop:waterCaneStem", "commonOilTubers" },
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = -0.4f,
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new SpawnWorldAction
		{
			KeyName = "spawnWorld",
			WorldData = new WorldData
			{
				WorldRadius = GameData.Instance.Constants.DefaultWorldRadius,
				ViewLongitudeStart = -5f,
				ViewLongitudeEnd = 35f,
				ViewLatitudeStart = 40f,
				ViewLatitudeEnd = 85f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteData = new SiteData
			{
				Name = "TUTORIAL: Castaways",
				KeyName = "playSite",
				IsPlaySite = true,
				ShowLabel = true,
				ShowTallPin = true,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnBushDragonTutAllegianceNorth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "bushDragonTutAllegianceNorth",
				Name = "bushDragonTutAllegianceNorth",
				AllegianceKey = "bushDragonTutAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(3504f, 2458f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 95,
				Name = "Bush Dragon Allegiance",
				KeyName = "bushDragonTutAllegianceNorth",
				EntityType = "entity:bushDragon",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 1f,
					Comfort = 1f,
					FoodSupply = 1f
				}
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnBushDragonTutAllegianceSouth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "bushDragonTutAllegianceSouth",
				Name = "bushDragonTutAllegianceSouth",
				AllegianceKey = "bushDragonTutAllegianceSouth",
				Location = new ValueNode
				{
					Location = new Vector2(3456f, 2688f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 50,
				Name = "Bush Dragon Allegiance",
				KeyName = "bushDragonTutAllegianceSouth",
				EntityType = "entity:bushDragon",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 1f,
					Comfort = 1f,
					FoodSupply = 1f
				}
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnPlayerAllegiance",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				Name = "Player Allegiance",
				KeyName = "playerAllegiance",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Player
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeCrevice",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:crevice",
				Name = "Impassable gorge",
				Location = new Vector3(2976f, 3528f, 0f)
			}
		});
		list.Add(new SetViewAction
		{
			KeyName = "setView",
			DelayInSeconds = num,
			CenterOnLocation = new Vector2(240f, 0f),
			OffsetToLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			}
		});
		list.Add(new ExploreAction
		{
			KeyName = "exploreShroudSouth",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(3624f, 5904f),
			EntityToExploreWith = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "allegiances",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "keyName",
						ConstantStringEqual = "playerAllegiance"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "persons"
					}
				}
			}
		});
		list.Add(new MusicAction
		{
			KeyName = "stopMusic",
			StopMusic = true
		});
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(8f, 4f), "item:advancedKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(0f, 0f), "item:advancedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startLines", new Vector2(0f, -24f), "item:lines", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(0f, -24f), "item:firewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(0f, -24f), "item:spoakLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startcommonOilTubers", new Vector2(0f, -24f), "item:commonOilTubers", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(0f, -24f), "item:metalWire", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedFlintSpear", new Vector2(0f, -24f), "item:improvisedFlintSpear", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMashedcommonOilTubers", new Vector2(0f, -24f), "item:mashedcommonOilTubers", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(0f, -24f), "item:stones", "playerAllegiance", null, num));
		return list;
	}
}
