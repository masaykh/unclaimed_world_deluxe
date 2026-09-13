using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double delay = 0.25;
		double num = 0.5;
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText1",
			PropertyKey = "burialTextStart",
			Value = new ValueNode
			{
				String = " \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText2",
			PropertyKey = "burialTextMultipleDeathsMultipleSurvivors",
			Value = new ValueNode
			{
				String = "Life on Antheia was hard and death could happen at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "Life on Antheia was hard and death could happen at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText4",
			PropertyKey = "burialTextSingleDeathSingleSurvivor",
			Value = new ValueNode
			{
				String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText5",
			PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
			Value = new ValueNode
			{
				String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initTimeBeforeGroupMeeting",
			PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
			Value = new ValueNode
			{
				Decimal = 200f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreat",
			PropertyKey = "meetingEmigrateThreat",
			Value = new ValueNode
			{
				String = "If not...well, I might leave for #EMIGRATETO and start over."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreatAllUnhappy",
			PropertyKey = "meetingEmigrateThreatAllUnhappy",
			Value = new ValueNode
			{
				String = "It seems like everyone is just waiting for an excuse to pack up and leave for #EMIGRATETO to start over."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Security",
			PropertyKey = "meeting3Security",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Thanks for taking time to listen... It's about the security situation. The way it's handled, I don't think we're safe here. \n \n#CONTENT: I don't see the problem. What do you want done? \n \n#UNHAPPY: I want us to beef up security. How we do it? Stock more and better weapons, take fewer risks... There's a number of ways we can protect our colony better. Main thing is that we take action now. \n \n#CONTENT: This is all a question of priorities... \n \n#UNHAPPY: Exactly. And if we value our lives, we need better protection from wild animals. So I hope you're with me. #EMIGRATETHREAT \n \n#CONTENT: Let's make sure it doesn't come to that. Anyone has more to add?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Food",
			PropertyKey = "meeting3Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I'm really worried about the food situation. \n \n#CONTENT: What do you want done? \n \n#UNHAPPY: I want us to build up a bigger stockpile of food. We need more smoked meat and fish, dry staples...  \n \n#CONTENT: But aren't we already working on that? \n \n#UNHAPPY: It's not going quick enough. At this rate, a minor event could cause starvation. So I hope you'll improve this. #EMIGRATETHREAT \n \n#CONTENT: That would be pity. Who else has any gripes?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Comfort",
			PropertyKey = "meeting3Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Ok, I'm gonna go out on a limb here, at the risk of sounding like I'm whining... \n \n#CONTENT: Speak up. \n \n#UNHAPPY: When I came to this place, I was prepared to endure hardships on the frontier, but... \n \n#CONTENT: But? \n \n#UNHAPPY: Living conditions here are dreadful and they improve so slowly. Could we please work on getting better houses and some minor luxuries. I'm not asking for much, so I hope you agree. #EMIGRATETHREAT \n \n#CONTENT: As if we don't have enough to worry about. Anyone else has complaints?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Security",
			PropertyKey = "meeting2Security",
			Value = new ValueNode
			{
				String = "-#UNHAPPY: I've tried to convince you before. This is getting out of hand. We need to improve security. \n \n-#CONTENT: I don't see... \n \n-#UNHAPPY: Listen. We're only two people left. We need to look out for each other. We need to get better weapons, take fewer risks... right now! I hope you understand! #EMIGRATETHREAT \n \n-#CONTENT: I don't know what to say. I think there are so many other things that are more important."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Food",
			PropertyKey = "meeting2Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: We need to stock up on food. \n \n#CONTENT: Yeah, you keep saying this, but I think you're obsessing over something trivial. We have enough to eat. \n \n#UNHAPPY: We're only two people left here. I don't want us to fight over food. So please, see reason. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Comfort",
			PropertyKey = "meeting2Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: We need better shelters. \n \n#CONTENT: Why is it so important. We have all we need. \n \n#UNHAPPY: Look, the conditions here are horrible, even for two people. Please, let's work on the living conditions. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingSecurityAllUnhappy",
			PropertyKey = "meetingSecurityAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingFoodAllUnhappy",
			PropertyKey = "meetingFoodAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingComfortAllUnhappy",
			PropertyKey = "meetingComfortAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! #EMIGRATETHREAT"
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
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. This place scares me. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going to #EMIGRATIONTARGET where I'll be safe. \nGoodbye."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortDialogText",
			PropertyKey = "comfortEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: This place is a pig sty. I can't stand it. The cold, the bugs and the filth...why are you ok with living like this? \n \n#NAME2: Because there are other things that are more important? \n \n#NAME1: I'm fed up. These conditions here, they're subhuman. I'm leaving for #EMIGRATIONTARGET. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortNoConversationDialogText",
			PropertyKey = "comfortEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is my final goodbye. I'm fed up with this squalor. These conditions here, they're subhuman. I'm going to #EMIGRATIONTARGET. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodDialogText",
			PropertyKey = "foodEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't do it anymore. I'm so worried about food all the time! Can't you see we're this close to starvation? \n \n#NAME2: Hey! We agreed to focus on other things! \n#NAME1: What could be more important than food?! I've had it. I'm going to #EMIGRATIONTARGET."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodNoConversationDialogText",
			PropertyKey = "foodEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. This constant fretting about food and waiting for an inevitable disaster has me worried sick. I'm going to #EMIGRATIONTARGET. Don't come knocking when your food runs out."
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
				ViewLongitudeStart = 5f,
				ViewLongitudeEnd = 30f,
				ViewLatitudeStart = 50f,
				ViewLatitudeEnd = 75f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteData = new SiteData
			{
				Name = "Nadova's Site",
				KeyName = "playSite",
				Description = "The location of the Nadova Mining expedition. Designated by the Overseer AI as a mineral extraction site for Project CANOPY",
				Coords = new GeodeticCoordinate(14.0, 59.0),
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
				Name = "Mining Camp",
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
				Decimal = 350f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite2",
			SiteData = new SiteData
			{
				KeyName = "randomSmallAdvancedSite",
				SiteTemplates = new StringChance[3]
				{
					new StringChance
					{
						Edge = 0.33f,
						String = "smallAdvancedFarmingSite"
					},
					new StringChance
					{
						Edge = 0.66f,
						String = "smallAdvancedMiningSite"
					},
					new StringChance
					{
						Edge = 1f,
						String = "smallAdvancedFishingSite"
					}
				},
				Coords = new GeodeticCoordinate(15.5, 58.6),
				AllegianceKeyName = "othersite2Allegiance",
				ExpeditionKeyName = "othersite2Expedition",
				IsPlaySite = false,
				ShowLabel = true,
				ShowTallPin = false,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite1",
			SiteData = new SiteData
			{
				KeyName = "dukesLanding",
				Name = "Duke's Landing",
				Description = "The planet's largest settlement is the focal point for the Project CANOPY effort. They will receive our shipments of minerals.",
				Coords = new GeodeticCoordinate(16.0, 71.0),
				IsPlaySite = false,
				ShowLabel = true,
				ShowTallPin = true,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite1Allegiance1",
			Site = "dukesLanding",
			AllegianceData = new AllegianceData
			{
				KeyName = "dukesLandingAllegiance",
				Name = "Duke's Landing",
				EntityType = "entity:human",
				PermitsImmigration = false,
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 0.6f,
					Comfort = 0.4f,
					FoodSupply = 0.35f
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnOtherSite1Expedition1",
			DelayInSeconds = 0.1,
			AllegianceKey = "dukesLandingAllegiance",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "dukesLandingPlanetFallExpedition",
				Name = "Duke's Landing",
				AllegianceKey = "dukesLandingAllegiance",
				SizeFactor = 2.5f,
				AvailableForTrade = new SerializableDictionary<string, TradeAmountType>
				{
					{
						"item:scandium",
						new TradeAmountType
						{
							LinearIncreasePerDay = 0f,
							LinearConsumptionPerDay = 0f,
							AmountToBuy = 40,
							MaxAmountToBuy = 40
						}
					},
					{
						"item:terbium",
						new TradeAmountType
						{
							LinearIncreasePerDay = 0f,
							LinearConsumptionPerDay = 0f,
							AmountToBuy = 20,
							MaxAmountToBuy = 20
						}
					},
					{
						"item:sentry",
						new TradeAmountType
						{
							MaxAmountForSale = 12,
							StartAmount = new NormalDistribution
							{
								Mean = 12.0
							}
						}
					},
					{
						"item:sentryGunAmmo",
						new TradeAmountType
						{
							MaxAmountForSale = 100,
							StartAmount = new NormalDistribution
							{
								Mean = 100.0
							}
						}
					},
					{
						"item:coilRifle",
						new TradeAmountType
						{
							MaxAmountForSale = 15,
							StartAmount = new NormalDistribution
							{
								Mean = 15.0
							}
						}
					},
					{
						"item:shotgun",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:coilRifleAmmo",
						new TradeAmountType
						{
							MaxAmountForSale = 75,
							StartAmount = new NormalDistribution
							{
								Mean = 75.0
							}
						}
					},
					{
						"item:shotgunAmmo",
						new TradeAmountType
						{
							MaxAmountForSale = 50,
							StartAmount = new NormalDistribution
							{
								Mean = 50.0
							}
						}
					},
					{
						"item:astroRation",
						new TradeAmountType
						{
							MaxAmountForSale = 50,
							StartAmount = new NormalDistribution
							{
								Mean = 50.0
							}
						}
					},
					{
						"item:simCoffeeBeans",
						new TradeAmountType
						{
							MaxAmountForSale = 40,
							StartAmount = new NormalDistribution
							{
								Mean = 40.0
							}
						}
					},
					{
						"item:advancedKnife",
						new TradeAmountType
						{
							MaxAmountForSale = 20,
							StartAmount = new NormalDistribution
							{
								Mean = 20.0
							}
						}
					},
					{
						"item:advancedString",
						new TradeAmountType
						{
							MaxAmountForSale = 30,
							StartAmount = new NormalDistribution
							{
								Mean = 30.0
							}
						}
					},
					{
						"item:advancedMachete",
						new TradeAmountType
						{
							MaxAmountForSale = 15,
							StartAmount = new NormalDistribution
							{
								Mean = 15.0
							}
						}
					},
					{
						"item:advancedSnips",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:advancedCookingPot",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:fieldLabPacked",
						new TradeAmountType
						{
							MaxAmountForSale = 5,
							StartAmount = new NormalDistribution
							{
								Mean = 5.0
							}
						}
					},
					{
						"item:groundScanner",
						new TradeAmountType
						{
							MaxAmountForSale = 5,
							StartAmount = new NormalDistribution
							{
								Mean = 5.0
							}
						}
					},
					{
						"item:cloak",
						new TradeAmountType
						{
							MaxAmountForSale = 5,
							StartAmount = new NormalDistribution
							{
								Mean = 5.0
							}
						}
					},
					{
						"item:nightVisionGoggles",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:sensor",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:satelliteGroundStation",
						new TradeAmountType
						{
							MaxAmountForSale = 3,
							StartAmount = new NormalDistribution
							{
								Mean = 3.0
							}
						}
					},
					{
						"item:octagonalTent",
						new TradeAmountType
						{
							MaxAmountForSale = 6,
							StartAmount = new NormalDistribution
							{
								Mean = 6.0
							}
						}
					},
					{
						"item:smallTent",
						new TradeAmountType
						{
							MaxAmountForSale = 8,
							StartAmount = new NormalDistribution
							{
								Mean = 8.0
							}
						}
					},
					{
						"item:domeTent",
						new TradeAmountType
						{
							MaxAmountForSale = 5,
							StartAmount = new NormalDistribution
							{
								Mean = 5.0
							}
						}
					},
					{
						"item:thermalTarp",
						new TradeAmountType
						{
							MaxAmountForSale = 6,
							StartAmount = new NormalDistribution
							{
								Mean = 6.0
							}
						}
					},
					{
						"item:diamondGlass",
						new TradeAmountType
						{
							MaxAmountForSale = 10,
							StartAmount = new NormalDistribution
							{
								Mean = 10.0
							}
						}
					},
					{
						"item:fieldKitchenStove",
						new TradeAmountType
						{
							MaxAmountForSale = 4,
							StartAmount = new NormalDistribution
							{
								Mean = 4.0
							}
						}
					},
					{
						"item:fieldKitchenEquipment",
						new TradeAmountType
						{
							MaxAmountForSale = 4,
							StartAmount = new NormalDistribution
							{
								Mean = 4.0
							}
						}
					},
					{
						"item:liquidGas",
						new TradeAmountType
						{
							MaxAmountForSale = 30,
							StartAmount = new NormalDistribution
							{
								Mean = 30.0
							}
						}
					}
				},
				StructuresProfile = "largeHeliportProfile",
				VehiclesProfile = "airliftProfile",
				PricesProfile = "planetFallEraPrices"
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnFriendlyOtherSite1Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "dukesLandingAllegiance",
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
				Allegiance2 = "dukesLandingAllegiance",
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
				Allegiance2 = "dukesLandingAllegiance",
				Relation = 0f
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteWildernessSite1Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Passage to The Plains",
				FromSite = "playSite",
				ToSite = "wildernessSite1",
				Length = 10f,
				RouteType = RouteType.Land
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteSite2Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "North coast",
				FromSite = "playSite",
				ToSite = "randomSmallAdvancedSite",
				Length = 100f,
				RouteType = RouteType.CalmWater
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnWildernessSite1",
			SiteData = new SiteData
			{
				Name = "The Valley",
				KeyName = "wildernessSite1",
				Description = "An alternative location in the wilderness, considered by some as a better place for settling.",
				Coords = new GeodeticCoordinate(13.8, 58.8),
				IsPlaySite = false,
				ShowLabel = false,
				ShowTallPin = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnWildernessSite1Allegiance1",
			Site = "wildernessSite1",
			AllegianceData = new AllegianceData
			{
				Name = "The Valley",
				KeyName = "wildernessSite1Allegiance1",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Other,
				PermitsImmigration = true,
				StatsData = new StatsData
				{
					Security = 0f,
					Comfort = 0f,
					FoodSupply = 0.1f
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnWildernessSite1Expedition1",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "wildernessSite1Expedition1",
				Name = "Camp",
				AllegianceKey = "wildernessSite1Allegiance1"
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantSmithingSpecialist",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "dukesLandingAllegiance",
					ExpeditionKey = "dukesLandingPlanetFallExpedition"
				},
				Person = new Person
				{
					PersonalityType = "earlyJoinerPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "smithingSpecialist"
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
			KeyName = "spawnImmigrantMenialSpecialist",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "dukesLandingAllegiance",
					ExpeditionKey = "dukesLandingPlanetFallExpedition"
				},
				Person = new Person
				{
					PersonalityType = "earlyJoinerPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "menialSpecialist"
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
			KeyName = "spawnImmigrantMediumTier",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "dukesLandingAllegiance",
					ExpeditionKey = "dukesLandingPlanetFallExpedition"
				},
				Person = new Person
				{
					PersonalityType = "mediumTierPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.091f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.182f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.273f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.364f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.455f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.545f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.636f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.727f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.818f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.909f,
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
			KeyName = "spawnImmigrantAdvancedTier",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "dukesLandingAllegiance",
					ExpeditionKey = "dukesLandingPlanetFallExpedition"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.091f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.182f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.273f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.364f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.455f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.545f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.636f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.727f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.818f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.909f,
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
			KeyName = "spawnImmigrantRandom",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "dukesLandingAllegiance",
					ExpeditionKey = "dukesLandingPlanetFallExpedition"
				},
				Person = new Person
				{
					PersonalityType = "randomTierPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.091f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.182f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.273f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.364f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.455f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.545f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.636f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.727f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.818f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.909f,
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
			KeyName = "spawnImmigrantAdvancedTier2",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "othersite2Allegiance",
					ExpeditionKey = "othersite2Expedition"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.091f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.182f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.273f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.364f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.455f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.545f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.636f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.727f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.818f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.909f,
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
			KeyName = "spawnImmigrantMediumTier2",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "othersite2Allegiance",
					ExpeditionKey = "othersite2Expedition"
				},
				Person = new Person
				{
					PersonalityType = "mediumTierPersonality"
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.091f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.182f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.273f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.364f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.455f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.545f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.636f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.727f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.818f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.909f,
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
			KeyName = "spawnNadova",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-60f, 100f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Irina",
					LastName = "Nadova",
					PersonalityType = "advancedSecurityPersonality",
					Portrait = "human_w_f_adult_2",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 44.0
					},
					CasteKey = "female",
					ModelTextureName = "ManGreyClothes1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "constructionSpecialist"
						}
					}
				},
				EffectProfiles = new string[1] { "leader" },
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMike",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-75f, -8f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Mike",
					LastName = "Lewis",
					PersonalityType = "advancedSecurityPersonality",
					Portrait = "human_w_m_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 28.0
					},
					CasteKey = "male",
					ModelTextureName = "ManGreyClothes1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "securitySpecialist"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnBushcraftSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-48f, 38f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "bushcraftSpecialist"
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
			KeyName = "spawnBushcraftSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-38f, 48f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "bushcraftSpecialist"
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
			KeyName = "spawnSecuritySpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-85f, -18f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
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
			KeyName = "spawnSecuritySpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-70f, -18f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
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
			KeyName = "spawnCookingSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 128f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "cookingSpecialist"
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
			KeyName = "spawnCookingSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-88f, 56f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "cookingSpecialist"
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
			KeyName = "spawnHuntingSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-39f, 99f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "huntingSpecialist"
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
			KeyName = "spawnHuntingSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-118f, -6f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "huntingSpecialist"
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
			KeyName = "spawnMenialSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-42f, 120f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
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
			KeyName = "spawnMenialSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-51f, 10f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
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
			KeyName = "spawnConstructionSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-65f, 109f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "constructionSpecialist"
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
			KeyName = "spawnConstructionSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-81f, 24f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "constructionSpecialist"
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
			KeyName = "spawnSmithingSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 23f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "smithingSpecialist"
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
			KeyName = "spawnSmithingSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 47f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "smithingSpecialist"
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
			KeyName = "spawnFarmingSpecialist1",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 61f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "advancedSecurityPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "farmingSpecialist"
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
			KeyName = "spawnFarmingSpecialist2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 78f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "farmingSpecialist"
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
			KeyName = "spawnDog",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(10f, 28f, 0f),
				EntityKey = "entity:dog",
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 4.0
					},
					CultureTemplates = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "dogCulture"
						}
					}
				},
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
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-30f, 10f, 0f),
				EntityKey = "entity:haulingRobot",
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
			KeyName = "spawnHaulRobot2",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-80f, 15f, 0f),
				EntityKey = "entity:haulingRobot",
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
			KeyName = "spawnHaulRobot3",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(50f, 5f, 0f),
				EntityKey = "entity:haulingRobot",
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
			KeyName = "spawnMiningRobot",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-40f, -50f, 0f),
				EntityKey = "entity:diggingRobot",
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
			KeyName = "spawnGuardRobot",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-40f, 10f, 0f),
				EntityKey = "entity:guardRobot",
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
		float x = -100f;
		float y = 100f;
		float x2 = 160f;
		float y2 = 144f;
		float x3 = 160f;
		float y3 = 144f;
		float x4 = 160f;
		float y4 = 144f;
		float x5 = -156f;
		float y5 = 0f;
		float x6 = -24f;
		float y6 = 80f;
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(x5, y5), "item:panelScraps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(x5, y5), "item:spikeTrap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(x5, y5), "item:varmintBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startNet", new Vector2(x5, y5), "item:fishingNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(x5, y5), "item:textile", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPeat", new Vector2(x5, y5), "item:dryPeat", "playerAllegiance", null, num, null, null, 20));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(x3, y3), "item:astroRation", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(x3, y3), "item:driedBeef", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryGunAmmo", new Vector2(x3, y3), "item:sentryGunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(x3, y3), "item:blackpulp", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGlassyCreeper", new Vector2(x3, y3), "item:glassyCreeperPods", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(x4, y4), "item:gunpowderRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(x4, y4), "item:blackPowderRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(x4, y4), "item:musketoon", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(x4, y4), "item:blackPowderShotAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(x4, y4), "item:boltActionRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(x4, y4), "item:corditeAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(x4, y4), "item:ironSpear", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(x4, y4), "item:improvisedBow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCoilRifle", new Vector2(x4, y4), "item:coilRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCoilRifleAmmo", new Vector2(x4, y4), "item:coilRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShotgun", new Vector2(x4, y4), "item:shotgun", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShotgunAmmo", new Vector2(x4, y4), "item:shotgunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(x4, y4), "item:ironArrow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(x2, y2), "item:metalWire", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoldPot", new Vector2(x2, y2), "item:goldPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(x2, y2), "item:improvisedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(x2, y2), "item:farmingHoe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHammer", new Vector2(x2, y2), "item:hammer", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBellows", new Vector2(x2, y2), "item:bellows", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(x2, y2), "item:bigBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(x2, y2), "item:sensor", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(x2, y2), "item:turnipCracker", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBugNet", new Vector2(x2, y2), "item:strongBugNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronHooks", new Vector2(x2, y2), "item:ironHooks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(x2, y2), "item:neonHornetsLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(x2, y2), "item:fishTrapBasket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(x2, y2), "item:fishTrapHoopNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(x2, y2), "item:pigFliesLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(x, y), "item:radioAntenna", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(x, y), "item:radio", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShadeleafResin", new Vector2(x, y), "item:shadeleafResin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(x, y), "item:rawhideString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(x, y), "item:flintKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(x, y), "item:metalWorkersToolbox", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(x, y), "item:anvil", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(x, y), "item:barClamps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClayJar", new Vector2(x, y), "item:clayJar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBrickMold", new Vector2(x, y), "item:brickMold", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(x, y), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(x, y), "item:domeTent", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishtrapTest", new Vector2(x5, y5), "item:fishTrapBasket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDaysheenLeaves", new Vector2(x5, y5), "item:daysheenLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFiregrassSod", new Vector2(x5, y5), "item:firegrassSod", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(x5, y5), "item:stones", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSticks", new Vector2(x5, y5), "item:sticks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(x5, y5), "item:blackPowder", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(x5, y5), "item:blowpipe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(x5, y5), "item:steelPickaxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(x5, y5), "item:steelSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(x5, y5), "item:improvisedSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(x5, y5), "item:charcoal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(x5, y5), "item:wetFirewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(x5, y5), "item:goldOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(x5, y5), "item:bogOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(x5, y5), "item:cleanTurnipGuts", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(x5, y5), "item:fingerFruit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(x5, y5), "item:crystalBerries", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(x5, y5), "item:salt", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(x5, y5), "item:clayPotUnglazed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(x5, y5), "item:tappingBucket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(x5, y5), "item:clay", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(x5, y5), "item:firewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSolidMudBrick", new Vector2(x5, y5), "item:solidMudBrick", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(x5, y5), "item:spoakLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(x5, y5), "item:spoakBranchesTrimmed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(x5, y5), "item:marshcotSap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(x5, y5), "item:wingweedMat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakShingles", new Vector2(x5, y5), "item:spoakShingles", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(x5, y5), "item:steelHandAxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(x5, y5), "item:waterCaneStem", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(x5, y5), "item:shadeleafCanes", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(x5, y5), "item:vat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(x5, y5), "item:improvisedGreenHouseCover", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(x5, y5), "item:turnipSalami", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(x5, y5), "item:driedSaltedStreakFin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(x5, y5), "item:vinegar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHardtack", new Vector2(x5, y5), "item:hardtack", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRareMetal2", new Vector2(x5, y5), "item:terbium", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSmallTent", new Vector2(0f, 0f), "structure:smallTent", "playerAllegiance", null, delay, "Small Tent"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureDomeTent", new Vector2(80f, 0f), "structure:domeTent", "playerAllegiance", null, delay, "Dome Tent"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureFieldKitchen", new Vector2(128f, 80f), "structure:fieldKitchen", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig", new Vector2(0f, 188f), "structure:helipadBig", "playerAllegiance", null, delay, "Helipad"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipadBig2", new Vector2(-100f, 188f), "structure:helipadBig", "playerAllegiance", null, delay, "Helipad2"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCloak", new Vector2(x6, y6), "item:cloak", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startNightVisionGoggles", new Vector2(x6, y6), "item:nightVisionGoggles", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGroundScanner", new Vector2(x6, y6), "item:groundScanner", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRefinery1", new Vector2(x6, y6), "item:metalRefineryEquipment", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRefinery2", new Vector2(x6, y6), "item:metalRefineryPart1", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryOutside", new Vector2(x6, y6), "item:sentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSatelliteGroundStation", new Vector2(x6, y6), "item:satelliteGroundStation", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startLiquidGas", new Vector2(x6, y6), "item:liquidGas", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(x6, y6), "item:thermalTarp", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startIronCanisterForSale", "Helipad", "item:ironCanister", "playerAllegiance", null, num, true));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startHuntingRifle", "Small Tent", "item:coilRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startRifleAmmo", "Small Tent", "item:coilRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSentry", "Small Tent", "item:sentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(x6, y6), "item:sentryGunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSimCoffeeBeans", new Vector2(x6, y6), "item:simCoffeeBeans", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startCookingPot", "Dome Tent", "item:advancedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(x6, y6), "item:advancedMachete", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSnips", "Dome Tent", "item:advancedSnips", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(x6, y6), "item:advancedString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(x6, y6), "item:advancedKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBasicFireExtinguisher", new Vector2(x6, y6), "item:basicFireExtinguisher", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startChickenMeat", new Vector2(x6, y6), "item:thunderChickenMeat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenStove", new Vector2(x6, y6), "item:fieldKitchenStove", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFieldKitchenEquipment", new Vector2(x6, y6), "item:fieldKitchenEquipment", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDomeTentItem", new Vector2(x6, y6), "item:domeTent", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSmallTentItem", new Vector2(x6, y6), "item:smallTent", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSatteliteGroundStationItem", new Vector2(x6, y6), "item:satelliteGroundStation", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFieldLabPacked", new Vector2(x6, y6), "item:fieldLabPacked", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructurePanels", new Vector2(x6, y6), "item:structurePanels", "playerAllegiance", null, num));
		list.Add(new SpawnEntityAction
		{
			KeyName = "startNaturalTerminalGenericPosition",
			DelayInSeconds = 1.0,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalLandTerminal",
				Name = "To: The Valley",
				Location = new Vector3(0f, -150f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startNaturalTerminalFishingPosition",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalLandTerminal",
				Name = "To: The Plains",
				Location = new Vector3(3360f, 3024f, 0f)
			}
		});
		list.Add(new ExploreAction
		{
			Comments = "detect North passage from the start",
			KeyName = "exploreShroudNaturalTerminal",
			DelayInSeconds = num + 1.0,
			OffsetLocationStart = new Vector2(1800f, 0f),
			RadiusStart = 120f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
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
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureGroundStation", new Vector2(-100f, 40f), "structure:satelliteGroundStation", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureHelipad", new Vector2(-200f, 40f), "structure:helipadBig", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureLanding", new Vector2(0f, -480f), "structure:landingImprovised", "playerAllegiance", null, delay, "Landing"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureSimplePort", new Vector2(0f, 0f), "structure:simplePort", "playerAllegiance", null, delay, "Pier"));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureRadioHutImprovised", new Vector2(0f, -100f), "structure:radioHutImprovised", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStructureImprovisedSmithy", new Vector2(-80f, -50f), "structure:improvisedSmithy", "playerAllegiance", null, delay));
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
					AllowAmmoUseAgainstVermin = new SerializableDictionary<string, bool>
					{
						{ "item:coilRifleAmmo", false },
						{ "item:sentryGunAmmo", false },
						{ "item:shotgunAmmo", false },
						{ "item:bushDragonCartridge", false }
					},
					CurrentTiers = new SerializableDictionary<RatingTypes, string>
					{
						{
							RatingTypes.Comfort,
							"basic"
						},
						{
							RatingTypes.Food,
							"basic"
						},
						{
							RatingTypes.Security,
							"advanced"
						}
					}
				},
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
				Location = new Vector2(2640f, 2880f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(2640f, 2880f)
			}
		});
		list.Add(new SetViewAction
		{
			KeyName = "setView",
			DelayInSeconds = num,
			CenterOnLocation = new Vector2(0f, 0f),
			OffsetToLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			}
		});
		list.Add(new ExploreAction
		{
			KeyName = "exploreShroudFromSouth",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1000f, 0f),
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
		list.Add(new SpawnTriggerAction
		{
			KeyName = "spawnAnimalMigrateTriggerWest",
			Location = new ValueNode
			{
				Location = new Vector2(48f, 2112f)
			},
			AreaDimensions = new Vector2(36f, 500f),
			TriggerType = "animalMigrateTrigger"
		});
		list.Add(new SpawnTriggerAction
		{
			KeyName = "spawnAnimalMigrateTriggerRiver",
			Location = new ValueNode
			{
				Location = new Vector2(3168f, 3744f)
			},
			AreaDimensions = new Vector2(300f, 38f),
			TriggerType = "animalMigrateTrigger"
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnBirdExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "birdExpedition#1",
				AllegianceKey = "birdAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(2736f, 5280f)
				},
				PopulationData = new PopulationData
				{
					StartMembersList = new string[3] { "bird#1", "bird#2", "bird#3" },
					StartMembers = 3,
					MaxMembers = 3,
					GrowthInMembersPerDay = 0f
				}
			},
			AllegianceData = new AllegianceData
			{
				KeyName = "birdAllegiance#1",
				EntityType = "entity:bird",
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
			KeyName = "spawnBirdExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "birdExpedition#2",
				AllegianceKey = "birdAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(5520f, 4080f)
				},
				PopulationData = new PopulationData
				{
					StartMembersList = new string[3] { "bird#4", "bird#5", "bird#6" },
					StartMembers = 3,
					MaxMembers = 3,
					GrowthInMembersPerDay = 0f
				}
			},
			AllegianceData = new AllegianceData
			{
				KeyName = "birdAllegiance#2",
				EntityType = "entity:bird",
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
			KeyName = "spawnBushDragonExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "bushDragonAllegianceNorth",
				Name = "Bush Dragon Allegiance North",
				AllegianceKey = "bushDragonAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(2592f, 432f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 150f,
					StartMembers = 2,
					MaxMembers = 2,
					GrowthInMembersPerDay = 0.3f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 400,
				Name = "Bush Dragon Allegiance North",
				KeyName = "bushDragonAllegianceNorth",
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
			KeyName = "spawnBinalRatExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#2",
				Name = "Binal Rat Allegiance #2",
				AllegianceKey = "binalRatAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(3456f, 3984f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 4,
					StartMembers = 4,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 1500f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 2000,
				Name = "Binal Rat Allegiance #2",
				KeyName = "binalRatAllegiance#2",
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
			KeyName = "spawnBinalRatExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#1",
				Name = "Binal Rat Allegiance #1",
				AllegianceKey = "binalRatAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(3446f, 3974f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 4,
					StartMembers = 4,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 1500f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 2000,
				Name = "Binal Rat Allegiance #1",
				KeyName = "binalRatAllegiance#1",
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
			KeyName = "spawnSlugExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "slugAllegiance#1",
				Name = "Slug Allegiance #1",
				AllegianceKey = "slugAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(4224f, 1104f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 150f,
					StartMembers = 3,
					MaxMembers = 6,
					GrowthInMembersPerDay = 0.9f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 400,
				Name = "Slug Allegiance #1",
				KeyName = "slugAllegiance#1",
				EntityType = "entity:megapod",
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
			KeyName = "spawnTurnipExpeditionSouth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "turnipAllegianceNorth",
				Name = "Turnip Allegiance North",
				AllegianceKey = "turnipAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(3456f, 4416f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 100f,
					StartMembers = 5,
					MaxMembers = 5,
					GrowthInMembersPerDay = 0.2f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 500,
				Name = "Turnip Allegiance North",
				KeyName = "turnipAllegianceNorth",
				EntityType = "entity:turnip",
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
			KeyName = "spawnLeafcutterExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "leafcutterExpedition#1",
				Name = "Leafcutter Expedition",
				AllegianceKey = "leafcutterAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(4368f, 4848f)
				},
				PopulationData = new PopulationData
				{
					SpawnSources = new string[1] { "Field Quadite Nest 1" },
					StartSpawnSources = new string[1] { "fieldQuaditeNest1" },
					StartMembers = 2,
					MaxMembers = 5,
					GrowthInMembersPerDay = 9.2f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 500,
				Name = "Leafcutter allegiance",
				KeyName = "leafcutterAllegiance#1",
				EntityType = "entity:fieldQuadite",
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
			KeyName = "spawnDemonTreeExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "demonTreeAllegiance#3",
				Name = "Demon Tree Allegiance #3",
				AllegianceKey = "demonTreeAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(2496f, 480f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 1,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.7f,
					SpawnRadius = 100f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 300,
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
			KeyName = "spawnPatricianExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "patricianAllegiance#1",
				Name = "Patrician Allegiance",
				AllegianceKey = "patricianAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1152f, 4804f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 3,
					GrowthInMembersPerDay = 0.8f,
					SpawnRadius = 300f,
					RandomMembers = new StringChance[2]
					{
						new StringChance
						{
							Edge = 0.2f,
							String = "patrician#1"
						},
						new StringChance
						{
							Edge = 1f,
							String = "patrician#2"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 480,
				Name = "Patrician Allegiance",
				KeyName = "patricianAllegiance#1",
				EntityType = "entity:patrician",
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
				Name = "Demon Tree Allegiance #1",
				AllegianceKey = "demonTreeAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(4608f, 2976f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 1,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.7f,
					SpawnRadius = 100f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 200,
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
			KeyName = "spawnSwampDemonTreeExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "swampDemonTreeAllegiance#2",
				Name = "Swamp Demon Tree Allegiance #2",
				AllegianceKey = "swampDemonTreeAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(5088f, 480f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 1,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.7f,
					SpawnRadius = 100f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 200,
				Name = "Swamp Demon Tree Allegiance #2",
				KeyName = "swampDemonTreeAllegiance#2",
				EntityType = "entity:swampDendront",
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
			KeyName = "spawnSnatcherExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "snatcherAllegiance#1",
				Name = "Whipjaw Allegiance #1",
				AllegianceKey = "snatcherAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(768f, 3264f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 1,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.7f,
					SpawnRadius = 250f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 500,
				Name = "Whipjaw Allegiance #1",
				KeyName = "snatcherAllegiance#1",
				EntityType = "entity:whipjaw",
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
			KeyName = "spawnThunderChickenExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thunderChickenAllegiance#2",
				Name = "Thunder Chicken Allegiance #2",
				AllegianceKey = "thunderChickenAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(5472f, 2400f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 3,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 400f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 700,
				Name = "Thunder Chicken Allegiance #2",
				KeyName = "thunderChickenAllegiance#2",
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
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnSwarmerExpedition1",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "swarmerExpedition#1",
				AllegianceKey = "swarmerAllegiance#1",
				Name = "Swarmer Expedition",
				Location = new ValueNode
				{
					Location = new Vector2(960f, 1104f)
				},
				PopulationData = new PopulationData
				{
					SpawnSources = new string[3] { "Swarmer nest 3", "Swarmer nest 2", "Swarmer nest 4" },
					StartSpawnSources = new string[3] { "swarmerNest3", "swarmerNest2", "swarmerNest4" },
					StartMembers = 5,
					MaxMembers = 10,
					GrowthInMembersPerDay = 13.2f
				}
			}
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnSwarmerExpedition2",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "swarmerExpedition#2",
				AllegianceKey = "swarmerAllegiance#1",
				Name = "Swarmer Expedition",
				Location = new ValueNode
				{
					Location = new Vector2(1824f, 1632f)
				},
				PopulationData = new PopulationData
				{
					SpawnSources = new string[3] { "Swarmer nest 1", "Swarmer nest 5", "Swarmer nest 6" },
					StartSpawnSources = new string[3] { "swarmerNest1", "swarmerNest5", "swarmerNest6" },
					StartMembers = 5,
					MaxMembers = 10,
					GrowthInMembersPerDay = 13.2f
				}
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnSwarmerAllegiance1",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 700,
				Name = "Swarmer allegiance ",
				KeyName = "swarmerAllegiance#1",
				EntityType = "entity:swarmer",
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
			KeyName = "setSpawnIntervalLeafcutterNormal",
			PropertyKey = "leafcutterSpawnInterval",
			Value = new ValueNode
			{
				Int = 173
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setSpawnIntervalLesserWhipjawMigration",
			PropertyKey = "lesserWhipjawMigrationInterval",
			Value = new ValueNode
			{
				Int = 3200
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setSpawnIntervalBajinganMigration",
			PropertyKey = "bajinganMigrationInterval",
			Value = new ValueNode
			{
				Int = 4800
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxLeafcuttersNormal",
			PropertyKey = "maxLeafcutters",
			Value = new ValueNode
			{
				Int = 5
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxLesserWhipjawNormal",
			PropertyKey = "maxLesserWhipjaw",
			Value = new ValueNode
			{
				Int = 14
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxBajinganNormal",
			PropertyKey = "maxBajingan",
			Value = new ValueNode
			{
				Int = 19
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setPlentyResources",
			AllResources = true,
			ExcludeResourceTypes = new string[20]
			{
				"stones", "clay", "firegrassSod", "vine", "crop:sticks", "sulfurDeposit", "guanoDeposit", "streakFin", "carbonTail", "alabasterRay",
				"daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = 0.175f,
				NoiseAmplitude = 0.75f,
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setNormalResources",
			AllResources = true,
			ExcludeResourceTypes = new string[20]
			{
				"stones", "clay", "firegrassSod", "vine", "crop:sticks", "sulfurDeposit", "guanoDeposit", "streakFin", "carbonTail", "alabasterRay",
				"daggermouth", "clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = 0.15f,
				NoiseAmplitude = 0.75f,
				NoiseFrequency = 0.1f
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
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 1",
				Location = new Vector3(3446f, 886f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 2",
				Location = new Vector3(3362f, 1005f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 3",
				Location = new Vector3(2923f, 1013f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 4",
				Location = new Vector3(2971f, 1132f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 5",
				Location = new Vector3(4889f, 2158f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 6",
				Location = new Vector3(3884f, 2820f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 7",
				Location = new Vector3(4075f, 3077f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall8",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 8",
				Location = new Vector3(5142f, 3156f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall9",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 9",
				Location = new Vector3(2255f, 2827f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall10",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 10",
				Location = new Vector3(2589f, 2996f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall11",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 11",
				Location = new Vector3(2985f, 2975f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall12",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 12",
				Location = new Vector3(3189f, 4296f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall13",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 13",
				Location = new Vector3(4164f, 4379f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall14",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 14",
				Location = new Vector3(3887f, 3996f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall15",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 15",
				Location = new Vector3(3444f, 4609f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall16",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 16",
				Location = new Vector3(4180f, 4813f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall17",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 17",
				Location = new Vector3(980f, 4716f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall18",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 18",
				Location = new Vector3(999f, 4835f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot 1",
				Location = new Vector3(3221f, 867f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot 2",
				Location = new Vector3(3825f, 3176f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot 3",
				Location = new Vector3(3420f, 4314f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot 4",
				Location = new Vector3(2126f, 2600f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPierSpot1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:pierSpot",
				Name = "Pier spot",
				Location = new Vector3(3840f, 922f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPierSpot2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:pierSpot",
				Name = "Pier spot",
				Location = new Vector3(1865f, 4810f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPierSpot3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:pierSpot",
				Name = "Pier spot",
				Location = new Vector3(4684f, 5084f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (5360f, 969f)",
				Location = new Vector3(5360f, 969f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (5618f, 1767f)",
				Location = new Vector3(5618f, 1767f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (4978f, 1881f)",
				Location = new Vector3(4978f, 1881f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (2690f, 2691f)",
				Location = new Vector3(2690f, 2691f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3421f, 3206f)",
				Location = new Vector3(3421f, 3206f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (5454f, 3356f)",
				Location = new Vector3(5454f, 3356f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3908f, 3440f)",
				Location = new Vector3(3908f, 3470f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek8",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3305f, 3770f)",
				Location = new Vector3(3305f, 3770f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek9",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (1640f, 4242f)",
				Location = new Vector3(1640f, 4242f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek10",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (2380f, 3200f)",
				Location = new Vector3(1122f, 4292f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(2089f, 4777f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(5808f, 4944f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(634f, 5331f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(1838f, 5680f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(5116f, 5542f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(2612f, 5732f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot coast",
				Location = new Vector3(4103f, 5749f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(3362f, 469f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(616f, 533f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(4630f, 585f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(3582f, 1110f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(1679f, 2311f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(3312f, 2616f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBogOreDeposit1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:bogOreDeposit",
				Name = "Bog ore deposit",
				Location = new Vector3(2612f, 412f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBogOreDeposit2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:bogOreDeposit",
				Name = "Bog ore deposit",
				Location = new Vector3(1724f, 1435f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBogOreDeposit3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:bogOreDeposit",
				Name = "Bog ore deposit",
				Location = new Vector3(2480f, 2783f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBogOreDeposit4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:bogOreDeposit",
				Name = "Bog ore deposit",
				Location = new Vector3(3528f, 4118f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(3026f, 850f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(1495f, 1292f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(5100f, 1442f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(3693f, 2420f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(2789f, 2693f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(5086f, 3547f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(4100f, 4274f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit8",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(2880f, 4343f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit9",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Peat deposit",
				Location = new Vector3(1440f, 4992f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSaltDeposit1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:saltDeposit",
				Name = "Salt deposit",
				Location = new Vector3(1578f, 2885f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSaltDeposit2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:saltDeposit",
				Name = "Salt deposit",
				Location = new Vector3(3295f, 3503f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSaltDeposit3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:saltDeposit",
				Name = "Salt deposit",
				Location = new Vector3(5487f, 4707f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSaltDeposit4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:saltDeposit",
				Name = "Salt deposit",
				Location = new Vector3(624f, 5040f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSaltDeposit5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:saltDeposit",
				Name = "Salt deposit",
				Location = new Vector3(4125f, 5146f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startClayDeposit1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:clayDeposit",
				Name = "Clay deposit",
				Location = new Vector3(3934f, 710f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startClayDeposit2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:clayDeposit",
				Name = "Clay deposit",
				Location = new Vector3(4810f, 2966f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startClayDeposit3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:clayDeposit",
				Name = "Clay deposit",
				Location = new Vector3(4171f, 3115f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startClayDeposit4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:clayDeposit",
				Name = "Clay deposit",
				Location = new Vector3(4318f, 3908f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startClayDeposit5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:clayDeposit",
				Name = "Clay deposit",
				Location = new Vector3(1272f, 5139f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startRareMetalOreDepositNorth",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:rareMetalOreDeposit2",
				Name = "RareMetal ore Deposit2",
				Location = new Vector3(1440f, 288f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startRareMetalOreDepositCenter",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:rareMetalOreDeposit1",
				Name = "RareMetal ore Deposit1",
				Location = new Vector3(1536f, 3120f, 0f)
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = new Vector2(672f, 826f)
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
				Location = new Vector2(1200f, 1200f)
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
				Location = new Vector2(1104f, 1440f)
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
				Location = new Vector2(1968f, 1440f)
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
				Location = new Vector2(2688f, 1152f)
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
				Location = new Vector2(1872f, 864f)
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
				Location = new Vector2(2688f, 1104f)
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
			KeyName = "smallFog8",
			Location = new ValueNode
			{
				Location = new Vector2(3264f, 1872f)
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
			KeyName = "smallFog9",
			Location = new ValueNode
			{
				Location = new Vector2(3744f, 1920f)
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
			KeyName = "smallFog10",
			Location = new ValueNode
			{
				Location = new Vector2(3072f, 2640f)
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
			KeyName = "smallFog11",
			Location = new ValueNode
			{
				Location = new Vector2(1824f, 2352f)
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
			KeyName = "smallFog12",
			Location = new ValueNode
			{
				Location = new Vector2(3792f, 3936f)
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
			KeyName = "smallFog13",
			Location = new ValueNode
			{
				Location = new Vector2(2352f, 4224f)
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
			KeyName = "smallFog14",
			Location = new ValueNode
			{
				Location = new Vector2(2688f, 4944f)
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
			KeyName = "smallFog15",
			Location = new ValueNode
			{
				Location = new Vector2(2976f, 5184f)
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
			KeyName = "fog1",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(88, 21))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog2",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(86, 29))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog3",
			Scale = 7f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(92, 21))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog4",
			Scale = 7f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(95, 14))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog5",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(98, 20))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog6",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(99, 10))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog7",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(108, 10))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog8",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(116, 7))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog9",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(121, 8))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog10",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(89, 28))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog11",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(97, 76))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog12",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(102, 86))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog13",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(101, 98))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "fog14",
			Scale = 5f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(104, 104))
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "sulphurousSmoke1",
			Scale = 2f,
			TimeBetweenEmissions = 0.5f,
			Location = new ValueNode
			{
				Location = new Vector2(480f, 2976f)
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
			KeyName = "sulphurousSmoke2",
			Scale = 2f,
			TimeBetweenEmissions = 0.5f,
			Location = new ValueNode
			{
				Location = new Vector2(528f, 3120f)
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
			KeyName = "sulphurousSmoke3",
			Scale = 3f,
			TimeBetweenEmissions = 0.4f,
			Location = new ValueNode
			{
				Location = new Vector2(864f, 3024f)
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
			KeyName = "sulphurousSmoke4",
			Scale = 2.5f,
			TimeBetweenEmissions = 0.4f,
			Location = new ValueNode
			{
				Location = new Vector2(1104f, 3168f)
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
			Scale = 3f,
			Location = new ValueNode
			{
				Location = new Vector2(864f, 3168f)
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
			Scale = 3f,
			Location = new ValueNode
			{
				Location = new Vector2(1104f, 3360f)
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
			KeyName = "haze3",
			Scale = 3f,
			Location = new ValueNode
			{
				Location = new Vector2(1872f, 3505f)
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
			KeyName = "haze4",
			Scale = 3f,
			Location = new ValueNode
			{
				Location = new Vector2(1056f, 432f)
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
