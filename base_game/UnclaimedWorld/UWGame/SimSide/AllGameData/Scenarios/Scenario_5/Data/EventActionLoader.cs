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
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double delayInSeconds = 0.0;
		double delay = 0.25;
		double num = 0.5;
		double delay2 = 0.6;
		double num2 = 0.6;
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
				String = "The town of Headway had seen its share of tragedies over the years. Everyone had hoped that those days were over, when suddenly they lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial, #EUOLOGYGIVER urged everyone to find meaning in the sacrifice that #NAMEOFDECEASED had made to rebuild their community."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "The town of Headway had seen its share of tragedies over the years. Everyone had hoped that those days were over, when suddenly they lost #NAMEOFDECEASED#CAUSEOFDEATH. At the burial, #EUOLOGYGIVER urged everyone to find meaning in the sacrifice that #NAMEOFDECEASED had made to rebuild their community."
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
			KeyName = "initGameOver",
			PropertyKey = "gameOver",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initComfortTarget",
			PropertyKey = "comfortTarget",
			Value = new ValueNode
			{
				Decimal = 0.4f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initFoodTarget",
			PropertyKey = "foodTarget",
			Value = new ValueNode
			{
				Decimal = 0.4f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initSecurityTarget",
			PropertyKey = "securityTarget",
			Value = new ValueNode
			{
				Decimal = 0.27f
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
				String = "I would hate to see our town abandoned. But it seems like everyone thinks of leaving for #EMIGRATETO and starting over."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Security",
			PropertyKey = "meeting3Security",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Friends. I'm worried about security. I think we need to work entirely on security for awhile. \n \n#CONTENT: We made a plan, remember? To match Eden Plains in every area. That means we focus on food and comfort as well. \n \n#UNHAPPY: Yes, I'm aware of the goals that were set. But our surroundings are dangerous. We have great whipjaws right here on our doorstep. Swamp men and megapods nearby! \n \n#CONTENT: Security is one of our priorities. \n \n#UNHAPPY: Look - if we don't get more guns soon, something bad will happen. So let's change priorities! #EMIGRATETHREAT \n \n#CONTENT: Alright. This has been noted. Anything else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Food",
			PropertyKey = "meeting3Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I want to talk about our food stores. They are dangerously low. \n \n#CONTENT: Well, we already set a goal for the size of food stockpiles. \n \n#UNHAPPY: I know that it's part of the bigger plan of matching Eden Plains. But food supply needs much more attention! \n \n#CONTENT: You do notice that everyone here is working hard? \n \n#UNHAPPY: Yes. But we need to focus on preserving fish and staples. Right now, a minor event could cause us to starve. I hope you come to your senses! #EMIGRATETHREAT \n \n#CONTENT: Ok, we've heard your concerns. Anyone has something to add?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Comfort",
			PropertyKey = "meeting3Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I'll get right to it: This town is not a comfortable place to live. Far from it. \n \n#CONTENT: Everyone here is working hard to improve the place. \n \n#UNHAPPY: Look. We're putting enormous emphasis on stockpiling food and weapons. Why? \n \n#CONTENT: Because those areas are also part of the goal that we set. \n \n#UNHAPPY: Yeah, but maybe those goals need to be revised. I'm getting fed up with the squalor here. Let's focus attention on our houses and at least get some minor enjoyment. #EMIGRATETHREAT \n \n#CONTENT: Sad to hear that you're not happy here. Anyone else has complaints they'd like to share?"
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
				String = "#UNHAPPY: We need better living conditions. \n \n#CONTENT: We have all we need. \n \n#UNHAPPY: Look, the conditions here are horrible, even for two people. Please, let's work on making life more tolerable. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingSecurityAllUnhappy",
			PropertyKey = "meetingSecurityAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the security situation to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingFoodAllUnhappy",
			PropertyKey = "meetingFoodAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the food situation to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingComfortAllUnhappy",
			PropertyKey = "meetingComfortAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY:  Look, we all want the comfort conditions to improve! So let's get our act together and start working together! #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityDialogText",
			PropertyKey = "securityEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: Security around here is appalling. I can't believe the risks that we're taking. \n \n#NAME2: Is it about the whipjaw? The megapods? We can deal with them. \n \n#NAME1: When an animal attack happens - AND IT WILL! I'm not going to be around. I'm leaving now. You can find me at #EMIGRATIONTARGET if you want to get in touch."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityNoConversationDialogText",
			PropertyKey = "securityEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be leaving. We're in danger from wild animals here, but I seem to be the only one who takes this threat seriously. I'm going to #EMIGRATIONTARGET where I'll be safe. \nGoodbye."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortDialogText",
			PropertyKey = "comfortEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I've had it with the poor housing here. Also, there's no enjoyment to be had. \n \n#NAME2: Oh yeah? You have higher standards? \n \n#NAME1: I've endured enough. I'm fed up. I'm leaving for #EMIGRATIONTARGET. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortNoConversationDialogText",
			PropertyKey = "comfortEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: This is my final goodbye. I'm fed up with this squalor. These conditions here, they're way below my limits. I'm going to #EMIGRATIONTARGET. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodDialogText",
			PropertyKey = "foodEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't watch this anymore. Can't you see we're this close to starvation? \n \n#NAME2: Hey! We're working hard in a lot of areas! \n#NAME1: What could be more important than food?! I've seen starvation before and I'm not about to witness it again. I'm going to #EMIGRATIONTARGET."
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
				ViewLongitudeStart = 16f,
				ViewLongitudeEnd = 20f,
				ViewLatitudeStart = 69f,
				ViewLatitudeEnd = 72f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteData = new SiteData
			{
				Name = "Headway",
				KeyName = "playSite",
				Description = "For many years our town was a thriving farm community until events forced us to find other ways to earn a living.",
				Coords = new GeodeticCoordinate(17.72, 70.65),
				IsPlaySite = true,
				ShowLabel = true,
				ShowTallPin = false,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnPlayerAllegiance",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				Name = "Headway",
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
				Decimal = 240f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite1",
			SiteData = new SiteData
			{
				Name = "Zenig Station",
				KeyName = "otherSite1",
				Description = "A site with a booming mining and metalworking industry. Sells minerals and tools and buys rubber and foodstuff.",
				Coords = new GeodeticCoordinate(17.25, 69.6),
				IsPlaySite = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite1Allegiance1",
			Site = "otherSite1",
			AllegianceData = new AllegianceData
			{
				Name = "Zenig Station",
				KeyName = "otherSite1Allegiance1",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					Security = 0.41f,
					Comfort = 0.52f,
					FoodSupply = 0.3f
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
				SizeFactor = 1.2f,
				AllegianceKey = "otherSite1Allegiance1",
				TradeProfile = "industryTradeProfile",
				AvailableForTrade = new SerializableDictionary<string, TradeAmountType>
				{
					{
						"item:gaskets",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 30.0
							},
							MaxAmountForSale = 0,
							AmountToBuy = 30,
							MaxAmountToBuy = 30,
							LinearConsumptionPerDay = 6f
						}
					},
					{
						"item:sulfurPowder",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 12.0
							},
							MaxAmountForSale = 12
						}
					}
				},
				VehiclesProfile = "bargeProfile",
				StructuresProfile = "largePierProfile",
				PricesProfile = "descentEraPrices"
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
				Name = "Batten Creek",
				FromSite = "playSite",
				ToSite = "otherSite1",
				Length = 145f,
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
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite2",
			SiteData = new SiteData
			{
				Name = "Eden Plains",
				KeyName = "otherSite2",
				Description = "A well-developed farm village. The farmers here sell their crops and occasionally buy tools.",
				Coords = new GeodeticCoordinate(18.2, 71.18),
				IsPlaySite = false
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite2Allegiance1",
			Site = "otherSite2",
			AllegianceData = new AllegianceData
			{
				Name = "Eden Plains",
				KeyName = "otherSite2Allegiance1",
				EntityType = "entity:human",
				AllegianceType = AllegianceType.Other,
				StatsData = new StatsData
				{
					DynamicFood = new ValueNode
					{
						PropertyKey = "foodTarget"
					},
					DynamicSecurity = new ValueNode
					{
						PropertyKey = "securityTarget"
					},
					DynamicComfort = new ValueNode
					{
						PropertyKey = "comfortTarget"
					}
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
				Name = "East Wharf",
				SizeFactor = 1.2f,
				AllegianceKey = "otherSite2Allegiance1",
				TradeProfile = "farmingTradeProfile",
				AvailableForTrade = new SerializableDictionary<string, TradeAmountType>
				{
					{
						"item:extrusionMachineComponents",
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
						"item:sulfurPowder",
						new TradeAmountType
						{
							StartAmount = new NormalDistribution
							{
								Mean = 12.0
							},
							MaxAmountForSale = 12
						}
					}
				},
				VehiclesProfile = "bargeProfile",
				StructuresProfile = "mediumPierProfile",
				PricesProfile = "descentEraPrices"
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnFriendlyOtherSite2Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite2Allegiance1",
				Relation = 1f
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnNeutralOtherSite2Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite2Allegiance1",
				Relation = 0.5f
			}
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnHostileOtherSite2Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "otherSite2Allegiance1",
				Relation = 0f
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteSite2Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Ritchel's Strait",
				FromSite = "playSite",
				ToSite = "otherSite2",
				Length = 70f,
				RouteType = RouteType.CalmWater
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite2Chemist1",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite2Allegiance1",
					ExpeditionKey = "otherSite2Expedition1"
				},
				Person = new Person
				{
					FirstName = "Jane",
					LastName = "Neson",
					PersonalityType = "earlyJoinerPersonality",
					Portrait = "human_w_f_adult_1"
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 40.0
					},
					CasteKey = "female",
					ModelTextureName = "ManGreenSolid1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "chemistrySpecialist"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite2Chemist2",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite2Allegiance1",
					ExpeditionKey = "otherSite2Expedition1"
				},
				Person = new Person
				{
					FirstName = "Roy",
					LastName = "Neson",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_m_adult_1"
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 26.0
					},
					CasteKey = "male",
					ModelTextureName = "ManGreenSolid1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "chemistrySpecialist"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite2Chemist3",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite2Allegiance1",
					ExpeditionKey = "otherSite2Expedition1"
				},
				Person = new Person
				{
					FirstName = "Ben",
					LastName = "Neson",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_m_adult_1"
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 29.0
					},
					CasteKey = "male",
					ModelTextureName = "ManGreenSolid1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "chemistrySpecialist"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantOtherSite2MenialSpecialist",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite2Allegiance1",
					ExpeditionKey = "otherSite2Expedition1"
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
			KeyName = "spawnImmigrantOtherSite2Random",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "otherSite2Allegiance1",
					ExpeditionKey = "otherSite2Expedition1"
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
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnWildernessSite1",
			SiteData = new SiteData
			{
				Name = "Bird Hill",
				KeyName = "wildernessSite1",
				Coords = new GeodeticCoordinate(17.83, 70.73),
				IsPlaySite = false,
				ShowLabel = false,
				ShowTallPin = true
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnWildernessSite1Allegiance1",
			Site = "wildernessSite1",
			AllegianceData = new AllegianceData
			{
				Name = "Bird Hill",
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
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteWildernessSite1Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Road to Bird Hill",
				FromSite = "playSite",
				ToSite = "wildernessSite1",
				Length = 10f,
				RouteType = RouteType.Land
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnMillet",
			DelayInSeconds = num + 1.0,
			EntityData = new EntityData
			{
				Location = new Vector3(1370f, 1589f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "John",
					LastName = "Millet",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_m_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 64.0
					},
					CasteKey = "male",
					ModelTextureName = "ManCurryClothesRedHairTexture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "electronicsSpecialist"
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
			KeyName = "spawnRains",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1370f, 1589f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Tereza",
					LastName = "Rains",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_f_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 44.0
					},
					CasteKey = "female",
					ModelTextureName = "ManBlueBrownClothes1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "farmingSpecialist"
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
			KeyName = "spawnChemistTest",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1434f, 1596f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Jane",
					LastName = "Neson",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_w_f_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 40.0
					},
					CasteKey = "female",
					ModelTextureName = "ManGreenSolid1Texture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "chemistrySpecialist"
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
			EntityData = new EntityData
			{
				Location = new Vector3(1442f, 1537f, 0f),
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
					PersonalityType = "survivalTierPersonality",
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
					PersonalityType = "survivalTierPersonality",
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
			DelayInSeconds = num + 2.0,
			EntityData = new EntityData
			{
				Location = new Vector3(1370f, 1589f, 0f),
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
			KeyName = "spawnFarmingSpecialist1",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				Location = new Vector3(1370f, 1589f, 0f),
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
				Location = new Vector3(1370f, 1589f, 0f),
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
				Location = new Vector3(-13f, 10f, 0f),
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
		list.Add(new SpawnUpgradeAction
		{
			KeyName = "startUpgradeSettingClayHut1Beds",
			DelayInSeconds = num2,
			UpgradeTargetEntityName = "Clay hut 1",
			EntityTypeKey = "item:wingweedMats4People",
			UpgradeCategoryKey = "bedsOrMats4People"
		});
		list.Add(new SpawnUpgradeAction
		{
			KeyName = "startUpgradeSettingCookhouseStove",
			DelayInSeconds = num2,
			UpgradeTargetEntityName = "Cookhouse",
			EntityTypeKey = "item:simpleStoveUpgrade",
			UpgradeCategoryKey = "stove"
		});
		list.Add(new SpawnUpgradeAction
		{
			KeyName = "startUpgradeSettingCookhouseCommunityHall",
			DelayInSeconds = num2,
			UpgradeTargetEntityName = "Cookhouse",
			EntityTypeKey = "item:communityHallUpgrade",
			UpgradeCategoryKey = "communityHall"
		});
		list.Add(new SpawnStockpileAction
		{
			KeyName = "startBricksStockpile",
			DelayInSeconds = num,
			CoveredArea = new Point[1]
			{
				new Point(37, 31)
			},
			StartDragTilePosition = new Point(37, 31),
			MayStockpileItem = new SerializableDictionary<string, int>
			{
				{ "item:solidMudBrick", -1 },
				{ "item:charcoal", -1 }
			},
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
		list.Add(new SpawnStockpileAction
		{
			KeyName = "startFirewoodStockpile",
			DelayInSeconds = num,
			CoveredArea = new Point[1]
			{
				new Point(30, 30)
			},
			StartDragTilePosition = new Point(30, 30),
			MayStockpileItem = new SerializableDictionary<string, int> { { "item:firewood", -1 } },
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
		list.Add(new SpawnStockpileAction
		{
			KeyName = "startMaterialsStockpile",
			DelayInSeconds = num,
			CoveredArea = new Point[1]
			{
				new Point(39, 34)
			},
			StartDragTilePosition = new Point(39, 34),
			MayStockpileItem = new SerializableDictionary<string, int>
			{
				{ "item:sticks", -1 },
				{ "item:stones", -1 },
				{ "item:spoakShingles", -1 },
				{ "item:waterCaneStem", -1 }
			},
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
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(-136f, 182f), "item:panelScraps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(-136f, 160f), "item:spikeTrap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(-356f, -320f), "item:varmintBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startExtrusionMachineComponents", new Vector2(0f, 0f), "item:extrusionMachineComponents", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBeds", new Vector2(0f, 0f), "item:bedFrame", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStonesTest", new Vector2(0f, 0f), "item:stones", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakTest", new Vector2(0f, 0f), "item:spoakBranches", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMudBricks", new Vector2(0f, 0f), "item:solidMudBrick", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startScrapMetal", new Vector2(0f, 0f), "item:scrapMetal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(0f, 0f), "item:textile", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPeat", new Vector2(0f, 0f), "item:dryPeat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(0f, 0f), "item:driedBeef", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(0f, 0f), "item:blackpulp", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSmokedCarbonTail", new Vector2(0f, 0f), "item:smokedCarbonTail", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(-146f, 176f), "item:musketoon", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(-146f, 176f), "item:blackPowderShotAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(-146f, 176f), "item:boltActionRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(-146f, 176f), "item:corditeAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(-146f, 176f), "item:sentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(-146f, 176f), "item:sentryGunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(-146f, 176f), "item:ironSpear", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCoilRifle", new Vector2(0f, 0f), "item:coilRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCoilRifleAmmo", new Vector2(0f, 0f), "item:coilRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(-156f, 190f), "item:advancedSnips", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(-156f, 190f), "item:advancedString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(-156f, 190f), "item:advancedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(-156f, 190f), "item:improvisedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(-156f, 190f), "item:bigBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(-156f, 190f), "item:sensor", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(-156f, 190f), "item:turnipCracker", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(-156f, 190f), "item:neonHornetsLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(-156f, 190f), "item:fishTrapBasket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(-156f, 190f), "item:fishTrapHoopNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(-156f, 190f), "item:pigFliesLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(-100f, 100f), "item:radioAntenna", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(-100f, 100f), "item:radio", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(-100f, 100f), "item:rawhideString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(-100f, 100f), "item:flintKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(-100f, 100f), "item:metalWorkersToolbox", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(-100f, 100f), "item:anvil", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(-100f, 100f), "item:barClamps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(-100f, 100f), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(-100f, 100f), "item:domeTent", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(1814f, 1516f), "item:blackPowder", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(1814f, 1516f), "item:blowpipe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(1814f, 1516f), "item:improvisedSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(1814f, 1516f), "item:goldOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(1814f, 1516f), "item:bogOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(1814f, 1516f), "item:cleanTurnipGuts", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(1814f, 1516f), "item:fingerFruit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(1814f, 1516f), "item:clayPotUnglazed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(1814f, 1516f), "item:clay", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(1814f, 1516f), "item:spoakLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(1814f, 1516f), "item:spoakBranchesTrimmed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(1814f, 1516f), "item:wingweedMat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(1814f, 1516f), "item:waterCaneStem", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(1814f, 1516f), "item:shadeleafCanes", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(1814f, 1516f), "item:vat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(1814f, 1516f), "item:improvisedGreenHouseCover", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(1814f, 1516f), "item:turnipSalami", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startSulfurPowder", new Vector2(1200f, 1634f), "item:sulfurPowder", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startStreakFin", new Vector2(1814f, 1516f), "item:streakFin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startSolidMudBrick", new Vector2(1814f, 1516f), "item:solidMudBrick", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startCharcoal", new Vector2(1810f, 1512f), "item:charcoal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startSpoakShingles", new Vector2(1890f, 1656f), "item:spoakShingles", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startSticks", new Vector2(1896f, 1654f), "item:sticks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startStones", new Vector2(1886f, 1644f), "item:stones", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnEntity("startFirewood", new Vector2(1459f, 1478f), "item:firewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startFirewoodInWoodpile", "Woodpile", "item:firewood", "playerAllegiance", null, num, null, null, null, null, true));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startOrganicMatter", "Compost pit", "item:organicMatter", "playerAllegiance", null, num, null, null, "moist"));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startRottenVegetables", "Compost pit", "item:rottenVegetables", "playerAllegiance", null, num, null, null, "moist"));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSteelMachete", "Tool shed", "item:steelMachete", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startHoe", "Tool shed", "item:farmingHoe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSteelSpade", "Tool shed", "item:steelSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startPickaxe", "Tool shed", "item:steelPickaxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startIronHandAxe", "Tool shed", "item:steelHandAxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startTappingBucket", "Tool shed", "item:tappingBucket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBugNet", "Tool shed", "item:strongBugNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startIronHooks", "Tool shed", "item:ironHooks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBrickMold", "Tool shed", "item:brickMold", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startImprovisedTrowel", "Tool shed", "item:improvisedTrowel", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startGlassyCreeper", "Granary", "item:glassyCreeperPods", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startCrystalBerries", "Granary", "item:crystalBerries", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startDriedSaltedStreakFin", "Granary", "item:driedSaltedStreakFin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startPickledCarbonTail", "Granary", "item:pickledCarbonTail", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSmokedStreakFin", "Granary", "item:smokedStreakFin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startHardtack", "Granary", "item:hardtack", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startSalt", "Granary", "item:salt", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startRubber", "Workbench", "item:gaskets", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startKnife", "Workbench", "item:steelKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startMetalWire", "Workbench", "item:metalWire", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startMarshcotSap", "Workbench", "item:marshcotSap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startShadeleafResin", "Workbench", "item:shadeleafResin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBlacksmithsToolbox", "Smithy", "item:blacksmithsToolbox", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startMetalWorkersToolbox", "Smithy", "item:metalWorkersToolbox", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBellows", "Smithy", "item:bellows", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startWroughtIron", "Smithy", "item:wroughtIron", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBlisterSteel", "Smithy", "item:blisterSteel", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startGoldPot", "Cookhouse", "item:goldPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startKnifeInKitchen", "Cookhouse", "item:steelKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startClayJar", "Cookhouse", "item:clayJar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startVinegar", "Cookhouse", "item:vinegar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startGunpowderRifle", "Cookhouse", "item:gunpowderRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startGunpowderAmmo", "Cookhouse", "item:blackPowderRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startImprovisedBow", "Cookhouse", "item:improvisedBow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startIronArrow", "Cookhouse", "item:ironArrow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startCrystalWine", "Cookhouse", "item:crystalWine", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCookhouseStove", "Cookhouse", "item:simpleStoveUpgrade", "playerAllegiance", null, num2, null, null, null, "stove"));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCookhouseCommunityHall", "Cookhouse", "item:communityHallUpgrade", "playerAllegiance", null, num2, null, null, null, "communityHall"));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeClayHut1Mats", "Clay hut 1", "item:wingweedMats4People", "playerAllegiance", null, num2, null, null, null, "bedsOrMats4People"));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startUpgradeCanopyMats", "Canopy house", "item:wingweedMats4People", "playerAllegiance", null, num2, null, null, null, "bedsOrMats4People"));
		list.Add(new SpawnEntityAction
		{
			KeyName = "startNaturalTerminal",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalLandTerminal",
				Name = "To: Bird Hill",
				Location = new Vector3(3024f, 1255f, 0f)
			}
		});
		list.Add(ScenarioLoader.SpawnEntity("startWorkshopBuilding", new Vector2(1130f, 1544f), "structure:workshopBuilding", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startSmokeOven", new Vector2(1895f, 1608f), "structure:smokeOven", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureCompostPit", new Vector2(1252f, 1392f), "structure:compostPit", "playerAllegiance", null, delay, "Compost pit"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureCookhouse", new Vector2(1396f, 1637f), "structure:cookhouse", "playerAllegiance", null, delay, "Cookhouse"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureToolshed", new Vector2(1306f, 1478f), "structure:toolshed", "playerAllegiance", null, delay, "Tool shed"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureFirewoodStack", new Vector2(1404f, 1478f), "structure:firewoodStack", "playerAllegiance", null, delay, "Woodpile"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureClayGranary", new Vector2(1302f, 1595f), "structure:clayGranary", "playerAllegiance", null, delay, "Granary"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureMeatDryingRack", new Vector2(1245f, 1544f), "structure:meatDryingRack", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureKilnImprovisedSmall", new Vector2(1400f, 1556f), "structure:kilnImprovisedSmall", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureImprovisedWorkbench", new Vector2(1584f, 1614f), "structure:improvisedWorkbench", "playerAllegiance", null, delay, "Workbench"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureRadioHut", new Vector2(1633f, 1520f), "structure:radioHut", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureCaneHut", new Vector2(1530f, 1546f), "structure:caneHut", "playerAllegiance", null, delay, "Cane hut"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureClayHut", new Vector2(1688f, 1566f), "structure:clayHut", "playerAllegiance", null, delay, "Clay hut 1"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureSimpleSmithy", new Vector2(1778f, 1536f), "structure:simpleSmithy", "playerAllegiance", null, delay, "Smithy"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureKiln", new Vector2(1850f, 1526f), "structure:kiln", "playerAllegiance", null, delay));
		list.Add(ScenarioLoader.SpawnEntity("startStructureGreenhouse", new Vector2(1210f, 1744f), "structure:greenhouse", "playerAllegiance", null, delay2, null, "constructGreenhouse"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureSimplePort", new Vector2(1760f, 1652f), "structure:simplePort", "playerAllegiance", null, delay2, "Pier", "buildSimplePort", "Pier spot"));
		list.Add(ScenarioLoader.SpawnEntity("startStructureFishTrapCoast2", new Vector2(1977f, 1814f), "structure:fishTrapCoast", "playerAllegiance", null, delay2, "Fish trap", null, "Fish trap spot Saltwater 2"));
		list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot1", delay2, "establishSmallPlot", "Small plot 1"));
		list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot2", delay2, "establishSmallPlot", "Small plot 2"));
		list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot3", delay2, "establishSmallPlot", "Small plot 3"));
		list.Add(ScenarioLoader.RunProcess("startStructureSmallPlot4", delay2, "establishSmallPlot", "Small plot 4"));
		list.Add(ScenarioLoader.RunProcess("startStructureLargePlot1", delay2, "establishLargePlot", "Large plot 1"));
		list.Add(new CreateExpeditionAction
		{
			KeyName = "placeExpedition",
			DelayInSeconds = 0.1,
			ExpeditionData = new ExpeditionData
			{
				KeyName = "Camp",
				Name = "Town Centre",
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
							"medium"
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
				Location = new Vector2(1632f, 1652f)
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
			KeyName = "exploreEntireMap",
			DelayInSeconds = num + 1.0,
			ExploreWholeMap = true,
			DetectMode = DetectMode.RollToDetectHiddenEntities,
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
			KeyName = "spawnBirdExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "birdExpedition",
				AllegianceKey = "birdAllegiance",
				Location = new ValueNode
				{
					Location = new Vector2(1680f, 2000f)
				},
				PopulationData = new PopulationData
				{
					StartMembersList = new string[5] { "bird#1", "bird#2", "bird#3", "bird#4", "bird#5" },
					StartMembers = 5,
					MaxMembers = 5,
					GrowthInMembersPerDay = 0.9f
				}
			},
			AllegianceData = new AllegianceData
			{
				KeyName = "birdAllegiance",
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
			KeyName = "spawnTurnipExpeditionNorth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "turnipAllegianceNorth",
				Name = "Turnip Allegiance North",
				AllegianceKey = "turnipAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(816f, 1104f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 100f,
					StartMembers = 1,
					MaxMembers = 1,
					GrowthInMembersPerDay = 0.2f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 300,
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
			KeyName = "spawnBinalRatExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#1",
				Name = "Binal Rat Allegiance #1",
				AllegianceKey = "binalRatAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(2304f, 528f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 200f,
					StartMembers = 1,
					MaxMembers = 3,
					GrowthInMembersPerDay = 10f
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
					Location = new Vector2(960f, 2064f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 200f,
					StartMembers = 2,
					MaxMembers = 3,
					GrowthInMembersPerDay = 10f
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
					Location = new Vector2(768f, 1200f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 200f,
					StartMembers = 2,
					MaxMembers = 3,
					GrowthInMembersPerDay = 10f
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
			KeyName = "spawnSwampDemonTreeExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "swampDemonTreeExpedition#2",
				Name = "Swamp Demon Tree Allegiance #2",
				AllegianceKey = "swampDemonTreeAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(2600f, 620f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 60f,
					StartMembers = 1,
					MaxMembers = 1,
					GrowthInMembersPerDay = 0.44f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 250,
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
			KeyName = "spawnSlugExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "slugAllegiance#1",
				Name = "Slug Allegiance #1",
				AllegianceKey = "slugAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(2400f, 480f)
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
			KeyName = "spawnSnatcherExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "snatcherExpedition#1",
				Name = "Whipjaw Allegiance",
				AllegianceKey = "snatcherAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1104f, 2400f)
				},
				PopulationData = new PopulationData
				{
					SpawnRadius = 60f,
					StartMembers = 1,
					MaxMembers = 1,
					GrowthInMembersPerDay = 0.38f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 300,
				Name = "Whipjaw Allegiance",
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
			KeyName = "spawnThunderChickenExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thunderChickenExpedition#1",
				Name = "Thunder Chicken Allegiance",
				AllegianceKey = "thunderChickenAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1488f, 1008f)
				},
				PopulationData = new PopulationData
				{
					RandomMembers = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "thunderChicken"
						}
					},
					StartMembers = 1,
					MaxMembers = 2,
					GrowthInMembersPerDay = 0.44f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 400,
				Name = "Thunder Chicken Allegiance",
				KeyName = "thunderChickenAllegiance#1",
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
			KeyName = "spawnLeafcutterExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "leafcutterExpedition#1",
				Name = "Leafcutter Expedition",
				AllegianceKey = "leafcutterAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1296f, 1056f)
				},
				PopulationData = new PopulationData
				{
					SpawnSources = new string[1] { "Field quadite nest 1" },
					StartSpawnSources = new string[1] { "fieldQuaditeNest1" },
					StartMembers = 1,
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
				Location = new Vector3(1760f, 1652f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall1",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 1",
				Location = new Vector3(1104f, 1344f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall2",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 2",
				Location = new Vector3(1015f, 1480f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall3",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 3",
				Location = new Vector3(1192f, 1480f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall4",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 4",
				Location = new Vector3(1566f, 1296f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall5",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 5",
				Location = new Vector3(1039f, 1193f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall6",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot 6",
				Location = new Vector3(1366f, 1996f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge1",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot 1",
				Location = new Vector3(1340f, 1268f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast1",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot Saltwater 1",
				Location = new Vector3(100f, 100f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast2",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot Saltwater 2",
				Location = new Vector3(1977f, 1814f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast3",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot Saltwater 3",
				Location = new Vector3(1625f, 2050f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore1",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater 1",
				Location = new Vector3(2160f, 816f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore2",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater 2",
				Location = new Vector3(2016f, 560f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore3",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater 3",
				Location = new Vector3(675f, 1393f, 0f)
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
				Location = new Vector3(2648f, 1584f, 0f)
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
				Location = new Vector3(432f, 624f, 0f)
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
				Location = new Vector3(960f, 1056f, 0f)
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
				Location = new Vector3(1728f, 2496f, 0f)
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = new Vector2(2060f, 1816f)
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
				Location = new Vector2(2400f, 1758f)
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
				Location = MapManager.TileToWorldPosVector2(new Point(42, 10))
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
				Location = MapManager.TileToWorldPosVector2(new Point(39, 9))
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
				Location = MapManager.TileToWorldPosVector2(new Point(45, 16))
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
				Location = MapManager.TileToWorldPosVector2(new Point(46, 7))
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
			Scale = 7f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(48, 10))
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
			Scale = 9f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(48, 14))
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
			Scale = 9f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(52, 17))
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
			Scale = 9f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(57, 14))
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
			Scale = 6f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(54, 7))
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
			Scale = 6f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(58, 4))
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
			Scale = 8f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(53, 3))
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
			Scale = 10f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(55, 10))
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
