using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double num = 0.25;
		double num2 = 0.5;
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
			KeyName = "initBurialText2",
			PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED. An evacuation aircraft has arrived to carry their remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText4",
			PropertyKey = "burialTextSingleDeathSingleSurvivor",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText5",
			PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
			Value = new ValueNode
			{
				String = "We have lost #NAMEOFDECEASED#CAUSEOFDEATH. An evacuation aircraft has arrived to carry the remains back to Duke's Landing. \n \nGoodbye, #NAMEOFDECEASED. You will be remembered as a shining example for future generations of settlers."
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
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Security",
			PropertyKey = "meeting3Security",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Thanks for taking time out of your day... I want to talk about the security situation. The way it's handled, I don't think we're safe here. \n \n#CONTENT: What do you want done? \n \n#UNHAPPY: I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks... There's a number of ways we can protect our colony better. Main thing is that we take action now. \n \n#CONTENT: This is all a question of priorities... \n \n#UNHAPPY: Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. #EMIGRATETHREAT \n \n#CONTENT: Alright. This has been noted. Anything else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreat",
			PropertyKey = "meetingEmigrateThreat",
			Value = new ValueNode
			{
				String = "If not...well, I might leave for #EMIGRATETO."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreatAllUnhappy",
			PropertyKey = "meetingEmigrateThreatAllUnhappy",
			Value = new ValueNode
			{
				String = "Seriously, I'm sometimes wondering whether it would be better to go to #EMIGRATETO and start over."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Food",
			PropertyKey = "meeting3Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Thanks for taking time to listen. We need to improve the food situation. \n \n#CONTENT: In what respect? \n \n#UNHAPPY: As an insurance against unforeseen events - we need larger food stores. \n \n#CONTENT: You are worried about running out of food? \n \n#UNHAPPY: Having this little food stockpiled, in my eyes, is very risky. So I hope you will act. #EMIGRATETHREAT \n \n#CONTENT: Alright. This has been noted. Anything else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Comfort",
			PropertyKey = "meeting3Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Living conditions here need to be improved. \n \n#CONTENT: You're not happy with the standard we have here? \n \n#UNHAPPY: No. Living under these conditions has a negative effect on physical abilities, not to mention the risk of conflict within this work community. \n \n#CONTENT: You do realize that we have other concerns as well... \n \n#UNHAPPY: In my eyes, this is vital: We need better housing and we need to increase our well-being. #EMIGRATETHREAT \n \n#CONTENT: Alright - this has been duly noted. Who has anything to add?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityDialogText",
			PropertyKey = "securityEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: Security around here is appalling. We're in the middle of a dangerous wilderness and I can't believe the risks that we're taking. It's come to the point that I'm afraid to sleep. \n \n#NAME2: Come on. Of course there are wild animals out there but nothing we can't deal with. \n \n#NAME1: When an animal attack happens - AND IT WILL! I'm not going to be around. I'm leaving now. You can find me at #EMIGRATIONTARGET if you want to get in touch."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityNoConversationDialogText",
			PropertyKey = "securityEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. The security here is appalling. This place scares me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving for #EMIGRATIONTARGET. \nGoodbye."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortDialogText",
			PropertyKey = "comfortEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: It's clear now that my concerns about the living standards here have fallen on deaf ears. \n \n#NAME2: We've had to postpone those efforts in favor of other areas. \n \n#NAME1: Well, you won't be hearing my complaints any longer. I'm leaving for #EMIGRATIONTARGET. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortNoConversationDialogText",
			PropertyKey = "comfortEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know that I'm leaving the camp and will go to #EMIGRATIONTARGET. As I've repeatedly stated, the living conditions here are unacceptable and since there's no sign of improvement you leave me no choice."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodDialogText",
			PropertyKey = "foodEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'm very disappointed that my concerns about food have been ignored. We are heading for disaster, but I will not be around when that happens. \n \n#NAME2: With all respect - you are overreacting. \n#NAME1: You can run this place your way now. I'm out. I'm going to #EMIGRATIONTARGET."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodNoConversationDialogText",
			PropertyKey = "foodEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. My warnings regarding the food situation have been ignored for too long and so I'm leaving. I'm going to #EMIGRATIONTARGET."
			}
		});
		SerializableDictionary<string, NeedData> needLevels = new SerializableDictionary<string, NeedData>
		{
			{
				"protein",
				new NeedData
				{
					Level = new NormalDistribution
					{
						Mean = 0.8999999761581421,
						StandardDeviation = 0.019999999552965164
					}
				}
			},
			{
				"foodEnergy",
				new NeedData
				{
					Level = new NormalDistribution
					{
						Mean = 0.8999999761581421,
						StandardDeviation = 0.019999999552965164
					}
				}
			},
			{
				"micronutrients",
				new NeedData
				{
					Level = new NormalDistribution
					{
						Mean = 0.8999999761581421,
						StandardDeviation = 0.019999999552965164
					}
				}
			},
			{
				"stimulants",
				new NeedData
				{
					Level = new NormalDistribution
					{
						Mean = 0.4000000059604645,
						StandardDeviation = 0.019999999552965164
					}
				}
			}
		};
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
				Name = "Zone J-23",
				KeyName = "playSite",
				Coords = new GeodeticCoordinate(10.24655, 55.83451),
				IsPlaySite = true,
				ShowLabel = true,
				ShowTallPin = true,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnPlayerAllegiance",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				Name = "Muckroot Research Station",
				KeyName = "playerAllegiance",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Player
			}
		});
		list.Add(new ChangeCreditsAction
		{
			KeyName = "setPlayerCredits",
			AllegianceKey = "playerAllegiance",
			Amount = new ValueNode
			{
				Decimal = 600f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite1",
			SiteData = new SiteData
			{
				Name = "Duke's Landing",
				KeyName = "otherSite1",
				Coords = new GeodeticCoordinate(8.24655, 69.83451),
				IsPlaySite = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite1Allegiance1",
			Site = "otherSite1",
			AllegianceData = new AllegianceData
			{
				Name = "North Base",
				KeyName = "otherSite1Allegiance1",
				EntityType = "entity:human",
				PermitsImmigration = true,
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 0.85f,
					Comfort = 0.69f,
					FoodSupply = 0.59f
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnOtherSite1Expedition1",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "otherSite1Expedition1",
				Name = "Helipad A",
				AllegianceKey = "otherSite1Allegiance1",
				AvailableForTrade = new SerializableDictionary<string, TradeAmountType>
				{
					{
						"item:astroRation",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:simCoffeeBeans",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 1.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:sentry",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 50.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:coilRifle",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 30.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:inactivatedFoodCoolerUnit",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 10.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:textile",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:smallTent",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 7.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:octagonalTent",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 8.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:domeTent",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 8.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:thermalTarp",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 4.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:sentryGunAmmo",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 15.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:coilRifleAmmo",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 10.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:advancedKnife",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 3.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:advancedMachete",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 4.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:steelPickaxe",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:hammer",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:metalWorkersToolbox",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 11.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:steelSpade",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 4.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:advancedString",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 1.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:metalWire",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 1.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:advancedSnips",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:advancedCookingPot",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 2.0,
								StandardDeviation = 0.02
							}
						}
					},
					{
						"item:sensor",
						new TradeAmountType
						{
							SpecificSellPrice = new NormalDistribution
							{
								Mean = 6.0,
								StandardDeviation = 0.02
							}
						}
					}
				},
				VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>
				{
					{
						"entity:skimmer",
						new VehiclesForHireType
						{
							SpecificPrice = new NormalDistribution
							{
								Mean = 300.0,
								StandardDeviation = 0.0
							},
							StartAmount = 1
						}
					},
					{
						"entity:harpy",
						new VehiclesForHireType
						{
							SpecificPrice = new NormalDistribution
							{
								Mean = 500.0,
								StandardDeviation = 0.0
							},
							StartAmount = 1
						}
					}
				}
			}
		});
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_precolRation", "item:astroRation", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_simCoffeeBeans", "item:simCoffeeBeans", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sentry", "item:sentry", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_coilRifle", "item:coilRifle", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_acetylene", "item:acetylene", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_liquidGas", "item:liquidGas", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_ironCanister", "item:ironCanister", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sentryGunAmmo", "item:sentryGunAmmo", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_coilRifleAmmo", "item:coilRifleAmmo", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_hammer", "item:hammer", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_knife", "item:advancedKnife", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_metalworkersToolbox", "item:metalWorkersToolbox", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_BlacksmithsToolbox", "item:blacksmithsToolbox", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_metalWire", "item:metalWire", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_machete", "item:advancedMachete", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelHandAxe", "item:steelHandAxe", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelPickaxe", "item:steelPickaxe", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_inactivatedFoodCoolerUnit", "item:inactivatedFoodCoolerUnit", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_textile", "item:textile", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_smallTent", "item:smallTent", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_octagonalTent", "item:octagonalTent", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_domeTent", "item:domeTent", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_thermalTarp", "item:thermalTarp", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_steelSpade", "item:steelSpade", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_string", "item:advancedString", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_snips", "item:advancedSnips", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_cookingPot", "item:advancedCookingPot", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(ScenarioLoader.SpawnItemOtherSite("otherSite1Expedition1_sensor", "item:sensor", "otherSite1Allegiance1", "otherSite1Expedition1", num2, null, "otherSite1", "helipad", true));
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnFriendlyOtherSite1Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite1Allegiance1",
				Relation = 1f
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnNeutralOtherSite1Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite1Allegiance1",
				Relation = 0.5f
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnHostileOtherSite1Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite1Allegiance1",
				Relation = 0f
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnOtherSite1Expedition1SatelliteGroundStation",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "structure:satelliteGroundStation",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite1Allegiance1",
					ExpeditionKey = "otherSite1Expedition1"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnOtherSite1Expedition1Helipad",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "structure:heliportLarge",
				Name = "helipad",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite1Allegiance1",
					ExpeditionKey = "otherSite1Expedition1"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnOtherSite1Expedition1Skimmer",
			DelayInSeconds = num2,
			EntityData = new EntityData
			{
				EntityKey = "entity:skimmer",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite1Allegiance1",
					ExpeditionKey = "otherSite1Expedition1"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnOtherSite1Expedition1Harpy",
			DelayInSeconds = num2,
			EntityData = new EntityData
			{
				EntityKey = "entity:harpy",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite1Allegiance1",
					ExpeditionKey = "otherSite1Expedition1"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMuckrootRandomImmigrant",
			DelayInSeconds = num2,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite1Allegiance1",
					ExpeditionKey = "otherSite1Expedition1"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[8]
					{
						new StringChance
						{
							Edge = 0.1f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.2f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 0.9f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				}
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite2",
			SiteData = new SiteData
			{
				Name = "Zone I-25",
				KeyName = "otherSite2",
				Coords = new GeodeticCoordinate(13.24655, 79.83451),
				IsPlaySite = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite2Allegiance1",
			Site = "otherSite2",
			AllegianceData = new AllegianceData
			{
				Name = "Desert base",
				KeyName = "otherSite2Allegiance1",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 0.32f,
					Comfort = 0.27f,
					FoodSupply = 0.59f
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnOtherSite2Expedition1",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "otherSite2Expedition1",
				Name = "Central",
				AllegianceKey = "otherSite2Allegiance1"
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson1",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(170f, -30f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson2",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(206f, 48f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson3",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(144f, -48f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson4",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(96f, 96f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson5",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(96f, -48f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson6",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(180f, 48f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnRandomMuckrootPerson7",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(96f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.15f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.6f,
							String = "medicineSpecialist"
						},
						new StringChance
						{
							Edge = 0.8f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[9]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "malePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "malePlanetfallWhiteRussianCulture"
						},
						new StringChance
						{
							Edge = 0.3f,
							String = "malePlanetfallChineseCulture"
						},
						new StringChance
						{
							Edge = 0.35f,
							String = "malePlanetfallJapaneseCulture"
						},
						new StringChance
						{
							Edge = 0.45f,
							String = "malePlanetfallHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "malePlanetfallAfricanCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femalePlanetfallWhiteAngloCulture"
						},
						new StringChance
						{
							Edge = 0.89f,
							String = "femalePlanetfallRussianCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femalePlanetfallAfricanCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnGuardAnimal",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(10f, 20f, 0f),
				EntityKey = "entity:dog",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnHaulRobot",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(3f, 40f, 0f),
				EntityKey = "entity:robotSmall",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnGunDog",
			DelayInSeconds = num2,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(5f, 50f, 0f),
				EntityKey = "entity:gunDog",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				}
			}
		});
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSmallTent", new Vector2(0f, 0f), "structure:smallTent", "playerAllegiance", null, num, "Small Tent"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureDomeTent", new Vector2(80f, 0f), "structure:domeTent", "playerAllegiance", null, num, "Dome Tent"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureFieldKitchen", new Vector2(128f, 80f), "structure:fieldKitchen", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig", new Vector2(0f, 188f), "structure:helipadBig", "playerAllegiance", null, num, "Helipad"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureGroundStation", new Vector2(-100f, 40f), "structure:satelliteGroundStation", "playerAllegiance", null, num));
		float x = -24f;
		float y = 80f;
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(x, y), "item:sensor", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(x, y), "item:thermalTarp", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(x, y), "item:domeTent", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startOctagonalTent", new Vector2(x, y), "item:octagonalTent", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSmallTent", new Vector2(x, y), "item:smallTent", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenStove", new Vector2(x, y), "item:fieldKitchenStove", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenEquipment", new Vector2(x, y), "item:fieldKitchenEquipment", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWeatherStationMast", new Vector2(x, y), "item:weatherStationMast", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWeatherStationSensors", new Vector2(x, y), "item:weatherStationSensors", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSatelliteGroundStation", new Vector2(x, y), "item:satelliteGroundStation", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAssemblerCabinet", new Vector2(x, y), "item:assemblerCabinet", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVacuumChamber", new Vector2(x, y), "item:vacuumChamber", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAssemblerCooling", new Vector2(x, y), "item:assemblerCooling", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPaint", new Vector2(x, y), "item:paint", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startLiquidGas", new Vector2(x, y), "item:liquidGas", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(x, y), "item:astroRation", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBasicFireExtinguisher", new Vector2(x, y), "item:basicFireExtinguisher", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startChickenMeat", new Vector2(x, y), "item:thunderChickenMeat", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAcetylene", new Vector2(x, y), "item:acetylene", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronCanister", new Vector2(x, y), "item:ironCanister", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startIronCanisterForSale", "Helipad", "item:ironCanister", "playerAllegiance", null, num2, true));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startHuntingRifle", "Small Tent", "item:coilRifle", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startRifleAmmo", "Small Tent", "item:coilRifleAmmo", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSentry", "Small Tent", "item:sentry", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSentryAmmo", "Small Tent", "item:sentryGunAmmo", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSimCoffeeBeans", "Dome Tent", "item:simCoffeeBeans", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startCookingPot", "Dome Tent", "item:advancedCookingPot", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startAssemblerPlateA", "Dome Tent", "item:assemblerPlateA", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startAssemblerMasterPlateA", "Dome Tent", "item:masterAssemblerPlateA", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startAssemblerPlateB", "Dome Tent", "item:assemblerPlateB", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startAssemblerMasterPlateB", "Dome Tent", "item:masterAssemblerPlateB", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startMachete", "Dome Tent", "item:advancedMachete", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSnips", "Dome Tent", "item:advancedSnips", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startString", "Dome Tent", "item:advancedString", "playerAllegiance", null, num2));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startKnife", "Dome Tent", "item:advancedKnife", "playerAllegiance", null, num2));
		list.Add(new CreateExpeditionAction
		{
			KeyName = "placeExpedition",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "Camp",
				Name = "Camp",
				AllegianceKey = "playerAllegiance",
				PolicyData = new ExpeditionPolicyData
				{
					FractionIndependentsAllowedToSleep = 0.7f,
					CurrentTiers = new SerializableDictionary<RatingTypes, string>
					{
						{
							RatingTypes.Comfort,
							"advanced"
						},
						{
							RatingTypes.Food,
							"advanced"
						},
						{
							RatingTypes.Security,
							"advanced"
						}
					},
					AllowAmmoUseAgainstVermin = new SerializableDictionary<string, bool>
					{
						{ "item:coilRifleAmmo", false },
						{ "item:sentryGunAmmo", false },
						{ "item:shotgunAmmo", false },
						{ "item:bushDragonCartridge", false }
					}
				},
				Location = new ValueNode
				{
					PropertyKey = "startingLocation"
				}
			},
			StatsData = new StatsData
			{
				Security = 1f,
				Comfort = 1f,
				FoodSupply = 1f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationSouth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(1680f, 2016f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(2400f, 720f)
			}
		});
		list.Add(new SetViewAction
		{
			KeyName = "setView",
			DelayInSeconds = num2,
			CenterOnLocation = new Vector2(240f, 0f),
			OffsetToLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			}
		});
		list.Add(new ExploreAction
		{
			KeyName = "exploreEntireMap",
			DelayInSeconds = num2 + 1.0,
			ExploreWholeMap = true,
			DetectMode = DetectMode.NoEntityDetection
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnDemonTreeExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "demonTreeAllegiance#3",
				Name = "demonTreeAllegiance#3",
				AllegianceKey = "demonTreeAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(1524f, 1680f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 144,
				Name = "Demon Tree Allegiance #3",
				KeyName = "demonTreeAllegiance#3",
				EntityType = "entity:spoakDendront",
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
			KeyName = "spawnDemonTreeExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "demonTreeAllegiance#1",
				Name = "demonTreeAllegiance#1",
				AllegianceKey = "demonTreeAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(4224f, 2592f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 600,
				Name = "Demon Tree Allegiance #1",
				KeyName = "demonTreeAllegiance#1",
				EntityType = "entity:spoakDendront",
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
			KeyName = "spawnDemonTreeExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "demonTreeAllegiance#2",
				Name = "demonTreeAllegiance#2",
				AllegianceKey = "demonTreeAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(2419f, 558f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 380,
				Name = "Demon Tree Allegiance #2",
				KeyName = "demonTreeAllegiance#2",
				EntityType = "entity:spoakDendront",
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
			KeyName = "binalRatAllegianceNorthWest1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegianceNorthWest1",
				Name = "binalRatAllegianceNorthWest1",
				AllegianceKey = "binalRatAllegianceNorthWest1",
				Location = new ValueNode
				{
					Location = new Vector2(1680f, 1872f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1440,
				Name = "binalRatAllegianceNorthWest1",
				KeyName = "binalRatAllegianceNorthWest1",
				EntityType = "entity:binalRat",
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
			KeyName = "binalRatAllegianceNorthWest2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegianceNorthWest2",
				Name = "binalRatAllegianceNorthWest2",
				AllegianceKey = "binalRatAllegianceNorthWest2",
				Location = new ValueNode
				{
					Location = new Vector2(3312f, 1584f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1440,
				Name = "binalRatAllegianceNorthWest2",
				KeyName = "binalRatAllegianceNorthWest2",
				EntityType = "entity:binalRat",
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
			KeyName = "binalRatAllegianceNorthWest3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegianceNorthWest3",
				Name = "binalRatAllegianceNorthWest3",
				AllegianceKey = "binalRatAllegianceNorthWest3",
				Location = new ValueNode
				{
					Location = new Vector2(480f, 2160f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1440,
				Name = "binalRatAllegianceNorthWest3",
				KeyName = "binalRatAllegianceNorthWest3",
				EntityType = "entity:binalRat",
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
			KeyName = "spawnThunderChickenAllegiance",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "",
				Name = "",
				AllegianceKey = "thunderChickenAllegiance",
				Location = new ValueNode
				{
					Location = new Vector2(500f, 500f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 200,
				Name = "Thunderchicken Allegiance",
				KeyName = "thunderChickenAllegiance",
				EntityType = "entity:whiteThunderChicken",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 1f,
					Comfort = 1f,
					FoodSupply = 1f
				}
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalSouthOften",
			PropertyKey = "twinklerSpawnIntervalSouth",
			Value = new ValueNode
			{
				Int = 5
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalEastOften",
			PropertyKey = "twinklerSpawnIntervalEast",
			Value = new ValueNode
			{
				Int = 5
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalSouthSeldom",
			PropertyKey = "twinklerSpawnIntervalSouth",
			Value = new ValueNode
			{
				Int = 800
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalEastSeldom",
			PropertyKey = "twinklerSpawnIntervalEast",
			Value = new ValueNode
			{
				Int = 1000
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxTwinklersLow",
			PropertyKey = "maxTwinklers",
			Value = new ValueNode
			{
				Int = 2
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxTwinklersNormal",
			PropertyKey = "maxTwinklers",
			Value = new ValueNode
			{
				Int = 12
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxTwinklersHigh",
			PropertyKey = "maxTwinklers",
			Value = new ValueNode
			{
				Int = 30
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensLow",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 10
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensNormal",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 15
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensHigh",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 25
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
			ExcludeResourceTypes = new string[17]
			{
				"stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux",
				"minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder"
			},
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
			ExcludeResourceTypes = new string[17]
			{
				"stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux",
				"minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder"
			},
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
			ExcludeResourceTypes = new string[17]
			{
				"stones", "firegrassSod", "commonOilTubers", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich", "torux",
				"minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = -0.4f,
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(23, 30))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog2",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(16, 28))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog3",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(7, 39))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog4",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(20, 38))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog5",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(25, 25))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog6",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(35, 49))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog7",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(22, 43))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "smallFog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "sulphurousSmoke1",
			TimeBetweenEmissions = 0.5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(44, 28))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "sulphurousSmoke"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "haze1",
			Scale = 4f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(8, 9))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "haze"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "haze2",
			Scale = 6f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(36, 23))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "haze"
				}
			}
		});
		return list;
	}
}
