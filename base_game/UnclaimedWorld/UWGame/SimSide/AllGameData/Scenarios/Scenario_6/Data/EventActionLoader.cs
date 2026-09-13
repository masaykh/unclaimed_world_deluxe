using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double delayInSeconds = 0.0;
		double delay = 0.25;
		double num = 0.5;
		double delay2 = 0.6;
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
				String = "The crew in the clay pit were all too familiar with hardship and tragedies. Else they wouldn't have signed up for this work. When they lost #NAMEOFDECEASED#CAUSEOFDEATH,  #EUOLOGYGIVER made it clear that everyone was here voluntarily and could leave at any time..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "The crew in the clay pit were all too familiar with hardship and tragedies. Else they wouldn't have signed up for this work. When they lost #NAMEOFDECEASED#CAUSEOFDEATH,  #EUOLOGYGIVER made it clear that everyone was here voluntarily and could leave at any time..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText4",
			PropertyKey = "burialTextSingleDeathSingleSurvivor",
			Value = new ValueNode
			{
				String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText5",
			PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
			Value = new ValueNode
			{
				String = "Losing #NAMEOFDECEASED came as a natural continuation of past tragedies more than a sudden shock. #EUOLOGYGIVER carried out the burial with as much dignity as possible and without reflecting on the past nor speculating on the future..."
			}
		});
		list.Add(new SetPropertyAction
		{
			Comments = "used for determining when the player has continued game after story part is over",
			KeyName = "initStoryPartOver",
			PropertyKey = "storyPartOver",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			Comments = "Used to check on when the player have achived the 40 mudbricks objective",
			KeyName = "initStores40Mudbricks",
			PropertyKey = "Stores40Mudbricks",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			Comments = "play with tut",
			KeyName = "setTutorialOn",
			PropertyKey = "tutorialOn",
			Value = new ValueNode
			{
				Bool = true
			}
		});
		list.Add(new SetPropertyAction
		{
			Comments = "play without tut",
			KeyName = "setTutorialOff",
			PropertyKey = "tutorialOn",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initTimeBeforeGroupMeeting",
			Comments = "200 seconds after contract ends",
			PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
			Value = new FunctionNode
			{
				Left = new UnaryFunctionNode
				{
					Operator = UnaryExpressionOperator.DateToRelativeSeconds,
					Operand = new ValueNode
					{
						PropertyKey = "endDate"
					}
				},
				Right = new ValueNode
				{
					Decimal = 200f
				},
				Operator = ExpressionOperator.Plus
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreat",
			PropertyKey = "meetingEmigrateThreat",
			Value = new ValueNode
			{
				String = "If not, then I'm gonna go back to #EMIGRATETO."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingEmigrateThreatAllUnhappy",
			PropertyKey = "meetingEmigrateThreatAllUnhappy",
			Value = new ValueNode
			{
				String = "I'm thinking, maybe we should all just give up and go back to #EMIGRATETO."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Security",
			PropertyKey = "meeting3Security",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Guys, thanks for hearing me out...it's about the security here... \n \n#CONTENT: What is it? \n \n#UNHAPPY: I think we need to be much more careful here. And we need more weapons. \n \n#CONTENT: It's up to all of us to keep an eye out. \n \n#UNHAPPY: It's not enough - if we don't get better weapons soon, something bad will happen. So let's do something about it! #EMIGRATETHREAT \n \n#CONTENT: Well, you're free to leave if you're afraid. Anything else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Food",
			PropertyKey = "meeting3Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I want to talk about the food here. Or the lack of it. \n \n#CONTENT: We don't want to waste money on expensive provisions. \n \n#UNHAPPY: No, but if people are hungry, we can't work hard. So we need to get more food soon. #EMIGRATETHREAT \n \n#CONTENT: Who else has something to complain about?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Comfort",
			PropertyKey = "meeting3Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I don't normally complain, but the living conditions here are testing my limits. \n \n#CONTENT: You're not comfortable here? \n \n#UNHAPPY: Look. I don't ask for much. Let's set aside some time for making good shelters. #EMIGRATETHREAT  \n \n#CONTENT: It would be great if we could have a comfortable time here. Not sure if that's possible though. Anything else?"
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
				String = "#UNHAPPY: We need better living conditions. \n \n#CONTENT: We have all we need. \n \n#UNHAPPY: Look, the conditions here are horrible, even for two people. Please, let's work on the living conditions. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingSecurityAllUnhappy",
			PropertyKey = "meetingSecurityAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingFoodAllUnhappy",
			PropertyKey = "meetingFoodAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingComfortAllUnhappy",
			PropertyKey = "meetingComfortAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityDialogText",
			PropertyKey = "securityEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'll keep it short - I'm quitting because of the bad security here. \n \n#NAME2: Yeah well, this is no place for sissies. If you quit, you're not getting paid. That was the deal. \n \n#NAME1: Whatever. Good luck fighting the patricians. I hope to see you back in #EMIGRATIONTARGET some day."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityNoConversationDialogText",
			PropertyKey = "securityEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going back to #EMIGRATIONTARGET. Keep my share of the money."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortDialogText",
			PropertyKey = "comfortEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I've had enough with the bad shelters, the cold and the filth. \n \n#NAME2: If you don't like it, you're free to leave before time. But then you're not getting paid. That was the deal. \n \n#NAME1: Yeah, yeah. I'm going back to #EMIGRATIONTARGET. See you there."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortNoConversationDialogText",
			PropertyKey = "comfortEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know that I'm quitting. I'm fed up with this squalor and these bad shelters. I'm going back to #EMIGRATIONTARGET. You can keep my share of the money."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodDialogText",
			PropertyKey = "foodEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I'm quitting because of the lack of food. I need to eat, you know. \n \n#NAME2: You can go back to #EMIGRATIONTARGET. That's one mouth less to feed. But you're not getting your share of the money. That was the deal.  \n#NAME1: Yeah I know. Enjoy your stay."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodNoConversationDialogText",
			PropertyKey = "foodEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is a note to let you know I'm quitting because of the lack of food here. I'm going back to #EMIGRATIONTARGET. Keep my share of the money."
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
						Mean = 0.6000000238418579,
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
				ViewLongitudeStart = 17.6f,
				ViewLongitudeEnd = 17.9f,
				ViewLatitudeStart = 70.6f,
				ViewLatitudeEnd = 70.8f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteData = new SiteData
			{
				Name = "Clay Pit",
				KeyName = "playSite",
				Description = "Our camp: Next to a muddy creek, large deposits of clay and not much else. To the north is a dangerous marsh with some iron.",
				Coords = new GeodeticCoordinate(17.775, 70.75),
				IsPlaySite = true,
				ShowLabel = true,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnPlayerAllegiance",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				Name = "Clay Pit",
				KeyName = "playerAllegiance",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Player,
				StatsData = new StatsData()
			}
		});
		list.Add(new ChangeCreditsAction
		{
			KeyName = "setPlayerCredits",
			AllegianceKey = "playerAllegiance",
			Amount = new ValueNode
			{
				Decimal = 10f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite1",
			SiteData = new SiteData
			{
				Name = "Tellus",
				KeyName = "otherSite1",
				Description = "The small town where we come from. A recent flooding caused a demand for mudbricks which we try to fulfil.",
				Coords = new GeodeticCoordinate(17.825, 70.68),
				IsPlaySite = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite1Allegiance1",
			Site = "otherSite1",
			AllegianceData = new AllegianceData
			{
				Name = "Tellus",
				KeyName = "otherSite1Allegiance1",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Other,
				PermitsImmigration = true,
				StatsData = new StatsData
				{
					Security = 0.32f,
					Comfort = 0.41f,
					FoodSupply = 0.29f
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
				Name = "The Wharf",
				AllegianceKey = "otherSite1Allegiance1",
				TradeProfile = "farmingTradeProfile",
				AvailableForTrade = new SerializableDictionary<string, TradeAmountType>
				{
					{
						"item:commonOilTubers",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 55.0
							},
							MaxAmountForSale = 55
						}
					},
					{
						"item:hardtack",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 30.0
							},
							MaxAmountForSale = 30
						}
					},
					{
						"item:driedThunderChicken",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 18.0
							},
							MaxAmountForSale = 18
						}
					},
					{
						"item:anvil",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 1.0
							},
							MaxAmountForSale = 1
						}
					},
					{
						"item:hammer",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 2.0
							},
							MaxAmountForSale = 2
						}
					},
					{
						"item:musket",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 1.0
							},
							LinearIncreasePerDay = 0.7f,
							MaxAmountForSale = 5
						}
					},
					{
						"item:blackPowderShotAmmo",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 2.0
							},
							LinearIncreasePerDay = 3f,
							MaxAmountForSale = 8
						}
					},
					{
						"item:solidMudBrick",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 0.0
							},
							AmountToBuy = 100,
							MaxAmountToBuy = 100,
							LinearConsumptionPerDay = 10f,
							MaxAmountForSale = 0
						}
					}
				},
				StructuresProfile = "largePierProfile",
				PricesProfile = "descentEraPrices",
				VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType> { 
				{
					"entity:smallBarge",
					new VehiclesForHireType
					{
						SpecificPrice = new NormalDistribution
						{
							Mean = 20.0,
							StandardDeviation = 0.0
						},
						StartAmount = 1
					}
				} }
			}
		});
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
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteSite1Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Tellus River",
				FromSite = "playSite",
				ToSite = "otherSite1",
				Length = 12f,
				RouteType = RouteType.CalmWater
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite1SmithingSpecialist",
			DelayInSeconds = num,
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
					PersonalityType = "survivalTierPersonality"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite1MenialSpecialist",
			DelayInSeconds = num,
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
					PersonalityType = "survivalTierPersonality"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite1Random",
			DelayInSeconds = num,
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
					TraitTemplates = new StringChance[11]
					{
						new StringChance
						{
							Edge = 0.035f,
							String = "chemistrySpecialist"
						},
						new StringChance
						{
							Edge = 0.07f,
							String = "mechanicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.11f,
							String = "electronicsSpecialist"
						},
						new StringChance
						{
							Edge = 0.22f,
							String = "smithingSpecialist"
						},
						new StringChance
						{
							Edge = 0.33f,
							String = "farmingSpecialist"
						},
						new StringChance
						{
							Edge = 0.44f,
							String = "constructionSpecialist"
						},
						new StringChance
						{
							Edge = 0.55f,
							String = "huntingSpecialist"
						},
						new StringChance
						{
							Edge = 0.66f,
							String = "menialSpecialist"
						},
						new StringChance
						{
							Edge = 0.77f,
							String = "cookingSpecialist"
						},
						new StringChance
						{
							Edge = 0.88f,
							String = "bushcraftSpecialist"
						},
						new StringChance
						{
							Edge = 1f,
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteSite1LandRoute",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Road to Tellus",
				FromSite = "playSite",
				ToSite = "otherSite1",
				Length = 10f,
				RouteType = RouteType.Land
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startNaturalTerminal",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalLandTerminal",
				Name = "To: Tellus",
				Location = new Vector3(1210f, 1902f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist1",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1560f, 1584f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist2",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1546f, 1600f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist3",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1534f, 1718f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist4",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1488f, 1766f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist5",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1524f, 1756f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist6",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1564f, 1762f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist7",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1695f, 1594f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist8",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1758f, 1594f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist9",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1728f, 1612f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMenialSpecialist10",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1779f, 1591f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "menialSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-20f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "bushcraftSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-0f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "bushcraftSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(0f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "securitySpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-10f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "cookingSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-10f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "cookingSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-3f, -10f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "huntingSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
				Location = new Vector3(-1f, -6f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
					SimulateJoinedExpeditionNow = true
				},
				EffectProfiles = new string[1] { "contract" },
				BioEntity = new BiologicalEntity
				{
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "huntingSpecialist"
						}
					},
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnConstructionSpecialist1",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1334f, 1696f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
					PersonalityType = "survivalTierPersonality",
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
					PersonalityType = "survivalTierPersonality",
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
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
					SimulateJoinedExpeditionNow = true,
					PersonalityType = "survivalTierPersonality"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnFarmingSpecialist1",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1334f, 1696f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnFarmingSpecialist2",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1334f, 1696f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality",
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture2"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture2"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture2"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture2"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "playSite"
					}
				} }
			}
		});
		list.Add(new SpawnStockpileAction
		{
			KeyName = "startBricksStockpile",
			DelayInSeconds = num,
			CoveredArea = new Point[1]
			{
				new Point(31, 38)
			},
			StartDragTilePosition = new Point(31, 38),
			MayStockpileItem = new SerializableDictionary<string, int> { { "item:solidMudBrick", -1 } },
			MayStockpileCategory = new SerializableDictionary<string, bool>
			{
				{ "preparedFood", false },
				{ "ingredients", false },
				{ "waste", false },
				{ "bodies", false },
				{ "rawMaterials", false },
				{ "tools", false },
				{ "weapons", false },
				{ "ammunition", false },
				{ "equipment", false }
			},
			OwnedBy = new AllegianceAndExpedition
			{
				AllegianceKey = "playerAllegiance",
				ExpeditionKey = "Camp"
			}
		});
		list.Add(ScenarioLoader.SpawnEntity("startClay", new Vector2(1814f, 1516f), "item:clay", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startStone", new Vector2(1814f, 1516f), "item:stones", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startWetMudBricks", new Vector2(1814f, 1516f), "item:wetMudBrick", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startSolidMudBricks", new Vector2(1814f, 1516f), "item:solidMudBrick", "playerAllegiance", null, num, null, null, null, 40));
		list.Add(ScenarioLoader.SpawnEntity("startFirewood", new Vector2(1814f, 1516f), "item:firewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startFlintRough", new Vector2(1814f, 1516f), "item:flintRough", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startScrapMetal", new Vector2(1814f, 1516f), "item:scrapMetal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startwaterCaneLeaves", new Vector2(1814f, 1516f), "item:waterCaneLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startarrowshafts", new Vector2(1814f, 1516f), "item:improvisedArrowShaftBundle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSmokedThunderChicken", "Lean-to1", "item:smokedThunderChicken", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startCommonOilTubers", "Lean-to1", "item:commonOilTubers", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startPickaxe", "Lean-to1", "item:steelPickaxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSteelSpade", "Lean-to1", "item:steelSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startHoe", "Lean-to1", "item:farmingHoe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startKnife", "Lean-to1", "item:steelKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startGoldPot", "Lean-to1", "item:goldPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBrickMold", "Lean-to1", "item:brickMold", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startRawhideString", "Lean-to1", "item:rawhideString", "playerAllegiance", null, num));
		list.Add(new SetPropertyAction
		{
			KeyName = "initEndDate",
			PropertyKey = "endDate",
			Value = new UnaryFunctionNode
			{
				Operator = UnaryExpressionOperator.DateFromRelativeDays,
				Operand = new ValueNode
				{
					Decimal = 2f
				}
			}
		});
		list.Add(ScenarioLoader.SpawnEntity("startStructureKiln", new Vector2(1452f, 1726f), "structure:kiln", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureCampfire", new Vector2(1642f, 1748f), "structure:campfire", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureLean-toSpoakLeaves1", new Vector2(1630f, 1646f), "structure:lean-toSpoakLeaves", "playerAllegiance", null, delay, "Lean-to1"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureLean-toSpoakLeaves2", new Vector2(1733f, 1796f), "structure:lean-toSpoakLeaves", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureA-frameSpoakLeaves1", new Vector2(1584f, 1692f), "structure:A-frameSpoakLeaves", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureA-frameSpoakLeaves2", new Vector2(1720f, 1709f), "structure:A-frameSpoakLeaves", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis1", new Vector2(1528f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis2", new Vector2(1544f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis3", new Vector2(1560f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis4", new Vector2(1576f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis5", new Vector2(1592f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureAbatis6", new Vector2(1608f, 1324f), "structure:abatis", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureCanopyPort", new Vector2(1584f, 1898f), "structure:canopyPort", "playerAllegiance", null, delay2, "Port", "buildCanopyPort", "Pier spot"));
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
							"survival"
						},
						{
							RatingTypes.Food,
							"survival"
						},
						{
							RatingTypes.Security,
							"basic"
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
			KeyName = "setStartingLocation",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(1632f, 1752f)
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
			KeyName = "exploreShroud",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 500f,
			RadiusEnd = 600f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1152f, 3024f),
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
					Location = new Vector2(624f, 1930f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 2,
					GrowthInMembersPerDay = 10f,
					RandomMembers = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "binalRat#1Allegiance#1"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 600,
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
			KeyName = "spawnBinalRatExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#2",
				Name = "Binal Rat Allegiance #2",
				AllegianceKey = "binalRatAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(2074f, 1872f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 4,
					GrowthInMembersPerDay = 10f,
					RandomMembers = new StringChance[2]
					{
						new StringChance
						{
							Edge = 0.5f,
							String = "binalRat#1Allegiance#2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "binalRat#2Allegiance#2"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 800,
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
			KeyName = "spawnBinalRatExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#3",
				Name = "Binal Rat Allegiance #3",
				AllegianceKey = "binalRatAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(1728f, 894f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 2,
					GrowthInMembersPerDay = 10f,
					RandomMembers = new StringChance[2]
					{
						new StringChance
						{
							Edge = 0.5f,
							String = "binalRat#1Allegiance#3"
						},
						new StringChance
						{
							Edge = 1f,
							String = "binalRat#2Allegiance#3"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 600,
				Name = "Binal Rat Allegiance #3",
				KeyName = "binalRatAllegiance#3",
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
			KeyName = "spawnPatricianExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "patricianAllegiance#1",
				Name = "Patrician Allegiance",
				AllegianceKey = "patricianAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1256f, 300f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 4,
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
			KeyName = "spawnMudWormExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "mudWormAllegiance#1",
				Name = "Mud Worm Allegiance",
				AllegianceKey = "mudWormAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1152f, 616f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 15,
					StartMembers = 3,
					GrowthInMembersPerDay = 8f,
					RandomMembers = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "mudWorm#1"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 380,
				Name = "Mud Worm Allegiance",
				KeyName = "mudWormAllegiance#1",
				EntityType = "entity:mudWorm",
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
			KeyName = "spawnMudWormExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "mudWormAllegiance#2",
				Name = "Mud Worm Allegiance",
				AllegianceKey = "mudWormAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(2037f, 1809f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 180,
				Name = "Mud Worm Allegiance",
				KeyName = "mudWormAllegiance#2",
				EntityType = "entity:mudWorm",
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
				Name = "Thunder Chicken Allegiance",
				AllegianceKey = "thunderChickenAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(2824f, 432f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 2,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.4f,
					RandomMembers = new StringChance[3]
					{
						new StringChance
						{
							Edge = 0.33f,
							String = "studdedThunderChicken#1"
						},
						new StringChance
						{
							Edge = 0.66f,
							String = "studdedThunderChicken#2"
						},
						new StringChance
						{
							Edge = 1f,
							String = "studdedThunderChicken#3"
						}
					}
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 400,
				Name = "Thunder Chicken Allegiance",
				KeyName = "thunderChickenAllegiance#2",
				EntityType = "entity:studdedThunderChicken",
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
			KeyName = "spawnBirdExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "birdExpedition#1",
				AllegianceKey = "birdAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(2688f, 1008f)
				},
				PopulationData = new PopulationData
				{
					StartMembersList = new string[6] { "bird#1", "bird#2", "bird#3", "bird#4", "bird#5", "bird#6" }
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
			KeyName = "startPierSpot1",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:pierSpot",
				Name = "Pier spot",
				Location = new Vector3(1584f, 1898f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot",
				Location = new Vector3(1018f, 1000f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot",
				Location = new Vector3(1610f, 1132f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot",
				Location = new Vector3(1980f, 1014f, 0f)
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
				Location = new Vector3(790f, 1510f, 0f)
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
				Location = new Vector3(624f, 528f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startPeatDeposit1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:peatDeposit",
				Name = "Bog ore deposit",
				Location = new Vector3(2190f, 630f, 0f)
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = new Vector2(1930f, 1500f)
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
				Location = new Vector2(1968f, 1392f)
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
				Location = new Vector2(1344f, 960f)
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
				Location = new Vector2(1920f, 1440f)
			},
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "fog"
				}
			}
		});
		return list;
	}
}
