using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
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
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data;

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
				String = "Legend had it that back on Earth, death was something you planned long in advance. Out here, death happened at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText3",
			PropertyKey = "burialTextSingleDeathMultipleSurvivors",
			Value = new ValueNode
			{
				String = "Legend had it that back on Earth, death was something you planned long in advance. Out here, death happened at any moment. \nLosing #NAMEOFDECEASED#CAUSEOFDEATH was a cause of grief, but at the burial, #EUOLOGYGIVER emphasized the value of a life in freedom, no matter its duration."
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
				ViewLongitudeStart = 8f,
				ViewLongitudeEnd = 12f,
				ViewLatitudeStart = 70f,
				ViewLatitudeEnd = 73f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteDataKey = "playSite"
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnPlayerAllegiance",
			Site = "playSite",
			AllegianceData = new AllegianceData
			{
				Name = "Castor Homestead",
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
				Decimal = 240f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite2",
			SiteDataKey = "randomSmallSiteDestinyRiver"
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnOtherSite1",
			SiteDataKey = "destinyRiverDeltaDescentEraSite"
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnOtherSite1Allegiance1",
			Site = "destinyRiverDeltaDescentEraSite",
			AllegianceDataKey = "destinyRiverDeltaDescentAllegiance"
		});
		list.Add(new CreateExpeditionAction
		{
			KeyName = "spawnOtherSite1Expedition1",
			DelayInSeconds = 0.1,
			AllegianceKey = "destinyRiverDeltaDescentAllegiance",
			ExpeditionDataKey = "destinyRiverDeltaDescentExpedition"
		});
		list.Add(new SpawnAllegianceRelationAction
		{
			KeyName = "spawnFriendlyOtherSite1Allegiance1Relation",
			DelayInSeconds = 1.0,
			AllegianceRelationData = new AllegianceRelationData
			{
				Allegiance1 = "playerAllegiance",
				Allegiance2 = "destinyRiverDeltaDescentAllegiance",
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
				Allegiance2 = "destinyRiverDeltaDescentAllegiance",
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
				Allegiance2 = "destinyRiverDeltaDescentAllegiance",
				Relation = 0f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnWildernessSite1",
			SiteData = new SiteData
			{
				Name = "The Plains",
				KeyName = "wildernessSite1",
				Description = "An alternative location in the wilderness, considered by some as a better place for settling.",
				Coords = new GeodeticCoordinate(9.72, 72.21),
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
				Name = "The Plains",
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
				Name = "Passage to The Plains",
				FromSite = "playSite",
				ToSite = "wildernessSite1",
				Length = 10f,
				RouteType = RouteType.Land
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
				ToSite = "destinyRiverDeltaDescentEraSite",
				Length = 120f,
				RouteType = RouteType.CalmWater
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteSite2Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Batten Creek",
				FromSite = "playSite",
				ToSite = "randomSmallSiteDestinyRiver",
				Length = 60f,
				RouteType = RouteType.CalmWater
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
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
						}
					}
				},
				NeedLevels = needLevels,
				Properties = new SerializableDictionary<string, PropertyResult> { 
				{
					"origin",
					new PropertyResult
					{
						StringResult = "othersite"
					}
				} }
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
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantSurvivalTier",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
				},
				Person = new Person
				{
					PersonalityType = "survivalTierPersonality"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantBasicTier",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
				},
				Person = new Person
				{
					PersonalityType = "basicTierPersonality"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnImmigrantAdvancedSecurityTier",
			DelayInSeconds = num,
			EntityData = new EntityData
			{
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					AllegianceKey = "destinyRiverDeltaDescentAllegiance",
					ExpeditionKey = "destinyRiverDeltaDescentExpedition"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
						}
					}
				},
				NeedLevels = needLevels
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnCastor",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-146f, 186f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Castor",
					LastName = "Hernes",
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
					ModelTextureName = "ManOchreClothesBlackHairTexture",
					RaceKey = "whiteHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "electronicsSpecialist"
						}
					}
				},
				EffectProfiles = new string[1] { "leader" }
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnLinsey",
			DelayInSeconds = num,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = new EntityData
			{
				Location = new Vector3(-96f, 0f, 0f),
				EntityKey = "entity:human",
				MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance",
					ExpeditionKey = "Camp"
				},
				Person = new Person
				{
					FirstName = "Linsey",
					LastName = "Cattier",
					PersonalityType = "survivalTierPersonality",
					Portrait = "human_b_f_adult_1",
					SimulateJoinedExpeditionNow = true
				},
				BioEntity = new BiologicalEntity
				{
					AgeInYears = new NormalDistribution
					{
						Mean = 39.0
					},
					CasteKey = "female",
					ModelTextureName = "ManGreenGreyClothes1Texture",
					RaceKey = "blackHumanDescendant",
					TraitTemplates = new StringChance[1]
					{
						new StringChance
						{
							String = "bushcraftSpecialist"
						}
					}
				}
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					PersonalityType = "basicTierPersonality",
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
				Location = new Vector3(-75f, -8f, 0f),
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
				Location = new Vector3(-60f, 100f, 0f),
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
					CultureTemplates = new StringChance[6]
					{
						new StringChance
						{
							Edge = 0.125f,
							String = "maleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 0.25f,
							String = "maleDescendantAsianCulture"
						},
						new StringChance
						{
							Edge = 0.375f,
							String = "maleDescendantHispanicCulture"
						},
						new StringChance
						{
							Edge = 0.5f,
							String = "maleDescendantBlackCulture"
						},
						new StringChance
						{
							Edge = 0.75f,
							String = "femaleDescendantWhiteCulture"
						},
						new StringChance
						{
							Edge = 1f,
							String = "femaleDescendantBlackCulture"
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
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPanelScraps", new Vector2(-136f, 182f), "item:panelScraps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpikeTrap", new Vector2(-136f, 160f), "item:spikeTrap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVarmintBomb", new Vector2(-136f, 182f), "item:varmintBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startScrapMetal", new Vector2(-136f, 182f), "item:scrapMetal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTextile", new Vector2(-136f, 182f), "item:textile", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWroughtIron", new Vector2(-136f, 182f), "item:wroughtIron", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlisterSteel", new Vector2(-136f, 182f), "item:blisterSteel", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startJerky", new Vector2(-136f, 182f), "item:driedBeef", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpulp", new Vector2(-136f, 182f), "item:blackpulp", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGlassyCreeper", new Vector2(-136f, 182f), "item:glassyCreeperPods", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGunpowderRifle", new Vector2(-146f, 176f), "item:gunpowderRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGunpowderAmmo", new Vector2(-146f, 176f), "item:blackPowderRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlunderbuss", new Vector2(-146f, 176f), "item:musketoon", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackPowderShotAmmo", new Vector2(-146f, 176f), "item:blackPowderShotAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionRifle", new Vector2(-146f, 176f), "item:boltActionRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBoltActionAmmo", new Vector2(-146f, 176f), "item:corditeAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(-146f, 176f), "item:sentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryAmmo", new Vector2(-146f, 176f), "item:sentryGunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronSpear", new Vector2(-146f, 176f), "item:ironSpear", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedBow", new Vector2(-146f, 176f), "item:improvisedBow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronArrow", new Vector2(-146f, 176f), "item:ironArrow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(-156f, 190f), "item:advancedMachete", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(-156f, 190f), "item:advancedSnips", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(-156f, 190f), "item:advancedString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalWire", new Vector2(-156f, 190f), "item:metalWire", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(-156f, 190f), "item:steelKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCookingPot", new Vector2(-156f, 190f), "item:advancedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoldPot", new Vector2(-156f, 190f), "item:goldPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedCookingPot", new Vector2(-156f, 190f), "item:improvisedCookingPot", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(-156f, 190f), "item:farmingHoe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHammer", new Vector2(-156f, 190f), "item:hammer", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBellows", new Vector2(-156f, 190f), "item:bellows", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSulfurSmokeBomb", new Vector2(-156f, 190f), "item:bigBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(-156f, 190f), "item:sensor", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipCracker", new Vector2(-156f, 190f), "item:turnipCracker", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBugNet", new Vector2(-156f, 190f), "item:strongBugNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronHooks", new Vector2(-156f, 190f), "item:ironHooks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startNeonHornetsLive", new Vector2(-156f, 190f), "item:neonHornetsLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCottonString", new Vector2(-156f, 190f), "item:cottonString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishingNet", new Vector2(-156f, 190f), "item:fishingNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapBasket", new Vector2(-156f, 190f), "item:fishTrapBasket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFishTrapHoopNet", new Vector2(-156f, 190f), "item:fishTrapHoopNet", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPigFliesLive", new Vector2(-156f, 190f), "item:pigFliesLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadioAntenna", new Vector2(-100f, 100f), "item:radioAntenna", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRadio", new Vector2(-100f, 100f), "item:radio", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShadeleafResin", new Vector2(-100f, 100f), "item:shadeleafResin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRawhideString", new Vector2(-100f, 100f), "item:rawhideString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFlintKnife", new Vector2(-100f, 100f), "item:flintKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMetalworkersToolbox", new Vector2(-100f, 100f), "item:metalWorkersToolbox", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startAnvil", new Vector2(-100f, 100f), "item:anvil", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBarClamps", new Vector2(-100f, 100f), "item:barClamps", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClayJar", new Vector2(-100f, 100f), "item:clayJar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBrickMold", new Vector2(-100f, 100f), "item:brickMold", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRefrigerator", new Vector2(-100f, 100f), "item:inactivatedFoodCoolerUnit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDomeTent", new Vector2(-100f, 100f), "item:domeTent", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDaysheenLeaves", new Vector2(-156f, 190f), "item:daysheenLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFiregrassSod", new Vector2(-156f, 190f), "item:firegrassSod", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startStones", new Vector2(-156f, 190f), "item:stones", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSticks", new Vector2(-156f, 190f), "item:sticks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlackpowder", new Vector2(-156f, 190f), "item:blackPowder", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBlowpipe", new Vector2(-156f, 190f), "item:blowpipe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startPickaxe", new Vector2(-156f, 190f), "item:steelPickaxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpade", new Vector2(-156f, 190f), "item:steelSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSteelSpade", new Vector2(-156f, 190f), "item:improvisedSpade", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCharcoal", new Vector2(-156f, 190f), "item:charcoal", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWetFirewood", new Vector2(-156f, 190f), "item:wetFirewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoldOre", new Vector2(-156f, 190f), "item:goldOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBogOre", new Vector2(-156f, 190f), "item:bogOre", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCleanTurnipGuts", new Vector2(-156f, 190f), "item:cleanTurnipGuts", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFingerFruit", new Vector2(-156f, 190f), "item:fingerFruit", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(-156f, 190f), "item:crystalBerries", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSalt", new Vector2(-156f, 190f), "item:salt", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClayPotUnglazed", new Vector2(-156f, 190f), "item:clayPotUnglazed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTappingBucket", new Vector2(-156f, 190f), "item:tappingBucket", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startClay", new Vector2(-156f, 190f), "item:clay", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startFirewood", new Vector2(-156f, 190f), "item:firewood", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSolidMudBrick", new Vector2(-156f, 190f), "item:solidMudBrick", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakLeaves", new Vector2(-156f, 190f), "item:spoakLeaves", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakBranchesTrimmed", new Vector2(-156f, 190f), "item:spoakBranchesTrimmed", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMarshcotSap", new Vector2(-156f, 190f), "item:marshcotSap", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWingweedMats", new Vector2(-156f, 190f), "item:wingweedMat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpoakShingles", new Vector2(-156f, 190f), "item:spoakShingles", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startIronHandAxe", new Vector2(-156f, 190f), "item:steelHandAxe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(-156f, 190f), "item:waterCaneStem", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShadeleafCanes", new Vector2(-156f, 190f), "item:shadeleafCanes", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(-156f, 190f), "item:vat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovisedGreenHouseCover", new Vector2(-156f, 190f), "item:improvisedGreenHouseCover", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTurnipSalami", new Vector2(-156f, 190f), "item:turnipSalami", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startDriedSaltedStreakFin", new Vector2(-156f, 190f), "item:driedSaltedStreakFin", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVinegar", new Vector2(-156f, 190f), "item:vinegar", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHardtack", new Vector2(-156f, 190f), "item:hardtack", "playerAllegiance", null, num));
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
				Name = "To: The Plains",
				Location = new Vector3(0f, -200f, 0f)
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
			KeyName = "setStartingLocationRiverBank",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(3196f, 3032f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorthArableLand",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(2208f, 1248f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorthMuddyCreek",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(2928f, 938f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationSouthEastRockyFiregrass",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(4678f, 5136f)
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
			KeyName = "exploreShroudRiverBank",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(144f, 3744f),
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
		list.Add(new ExploreAction
		{
			KeyName = "exploreShroudNorthArableLand",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(4704f, 96f),
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
		list.Add(new ExploreAction
		{
			KeyName = "exploreShroudSouthEastRockyFiregrass",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(4128f, 6096f),
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
				Location = new Vector2(18f, 1650f)
			},
			AreaDimensions = new Vector2(36f, 500f),
			TriggerType = "animalMigrateTrigger"
		});
		list.Add(new SpawnTriggerAction
		{
			KeyName = "spawnAnimalMigrateTriggerRiver",
			Location = new ValueNode
			{
				Location = new Vector2(18f, 3552f)
			},
			AreaDimensions = new Vector2(36f, 300f),
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
					Location = new Vector2(2640f, 3312f)
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
					Location = new Vector2(3370f, 2199f)
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
			KeyName = "spawnTurnipExpeditionNorth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "turnipAllegianceNorth",
				Name = "Turnip Allegiance North",
				AllegianceKey = "turnipAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(2100f, 925f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 5,
					StartMembers = 5,
					GrowthInMembersPerDay = 1f,
					SpawnRadius = 800f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1000,
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
			KeyName = "spawnBinalRatExpedition#2",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#2",
				Name = "Binal Rat Allegiance #2",
				AllegianceKey = "binalRatAllegiance#2",
				Location = new ValueNode
				{
					Location = new Vector2(3072f, 1488f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 3,
					StartMembers = 3,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 1500f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1500,
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
					Location = new Vector2(1392f, 2064f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 3,
					StartMembers = 3,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 1500f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1500,
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
			KeyName = "spawnBinalRatExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "binalRatAllegiance#3",
				Name = "Binal Rat Allegiance #3",
				AllegianceKey = "binalRatAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(4944f, 5088f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 3,
					StartMembers = 3,
					GrowthInMembersPerDay = 10f,
					SpawnRadius = 1000f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1000,
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
			KeyName = "spawnDemonTreeExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "demonTreeAllegiance#3",
				Name = "Demon Tree Allegiance #3",
				AllegianceKey = "demonTreeAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(1749f, 201f)
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
				Name = "Demon Tree Allegiance #1",
				AllegianceKey = "demonTreeAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(2592f, 2750f)
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
					Location = new Vector2(624f, 336f)
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
			KeyName = "spawnSnatcherExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "snatcherAllegiance#1",
				Name = "Whipjaw Allegiance #1",
				AllegianceKey = "snatcherAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(3552f, 912f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 1,
					StartMembers = 1,
					GrowthInMembersPerDay = 0.7f,
					SpawnRadius = 200f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 900,
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
					Location = new Vector2(1824f, 2976f)
				},
				PopulationData = new PopulationData
				{
					MaxMembers = 4,
					StartMembers = 3,
					GrowthInMembersPerDay = 1.9f,
					SpawnRadius = 900f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1000,
				Name = "Thunder Chicken Allegiance #2",
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
			KeyName = "spawnLeafcutterExpedition#1",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "leafcutterExpedition#1",
				Name = "Leafcutter Expedition",
				AllegianceKey = "leafcutterAllegiance#1",
				Location = new ValueNode
				{
					Location = new Vector2(1584f, 1296f)
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
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "spawnLeafcutterExpedition#3",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "leafcutterExpedition#3",
				Name = "Leafcutter Expedition",
				AllegianceKey = "leafcutterAllegiance#3",
				Location = new ValueNode
				{
					Location = new Vector2(3600f, 1248f)
				},
				PopulationData = new PopulationData
				{
					SpawnSources = new string[1] { "Field quadite nest 3" },
					StartSpawnSources = new string[1] { "fieldQuaditeNest3" },
					StartMembers = 1,
					MaxMembers = 5,
					GrowthInMembersPerDay = 9.2f
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 500,
				Name = "Leafcutter allegiance",
				KeyName = "leafcutterAllegiance#3",
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
			KeyName = "setSpawnIntervalSlugsNormal",
			PropertyKey = "slugSpawnInterval",
			Value = new ValueNode
			{
				Int = 2400
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
				Name = "Small plot (3168, 3072)",
				Location = new Vector3(3168f, 3072f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2260, 2020)",
				Location = new Vector3(2260f, 2020f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2260, 2020)",
				Location = new Vector3(1700f, 1844f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (1498, 1882)",
				Location = new Vector3(1498f, 1882f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (1498, 1882)",
				Location = new Vector3(1550f, 1464f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (1498, 1882)",
				Location = new Vector3(2072f, 1256f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (1498, 1882)",
				Location = new Vector3(1680f, 912f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall8",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (1498, 1882)",
				Location = new Vector3(1688f, 624f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall9",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(2119f, 952f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall10",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(2297f, 960f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall11",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(2450f, 682f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall12",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(3024f, 390f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall13",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(3130f, 272f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall14",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot (2119, 1882)",
				Location = new Vector3(3498f, 1364f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot (2119, 1882)",
				Location = new Vector3(1209f, 825f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot (2119, 1882)",
				Location = new Vector3(1882f, 1042f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot (2119, 1882)",
				Location = new Vector3(1894f, 884f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotLarge4",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:largePlotSpot",
				Name = "Large plot (2119, 1882)",
				Location = new Vector3(2030f, 536f, 0f)
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
				Location = new Vector3(2804f, 3082f, 0f)
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
				Location = new Vector3(2952f, 1110f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3504f, 3456f)",
				Location = new Vector3(3488f, 3300f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek5",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3504f, 3456f)",
				Location = new Vector3(2036f, 1750f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek6",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3504f, 3456f)",
				Location = new Vector3(3075f, 865f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot (3504f, 3456f)",
				Location = new Vector3(2130f, 761f, 0f)
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
				Location = new Vector3(3085f, 1066f, 0f)
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
				Location = new Vector3(1940f, 1431f, 0f)
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
				Location = new Vector3(2568f, 1738f, 0f)
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
				Location = new Vector3(2708f, 2069f, 0f)
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
				Location = new Vector3(1840f, 2024f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore7",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(1278f, 2420f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore8",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(2080f, 2456f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore17",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(2328f, 3214f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore18",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(1752f, 3576f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore19",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater",
				Location = new Vector3(852f, 3465f, 0f)
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
				Location = new Vector3(650f, 1642f, 0f)
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
				Location = new Vector3(1152f, 2496f, 0f)
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
				Location = new Vector3(1130f, 1438f, 0f)
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
				Location = new Vector3(2286f, 698f, 0f)
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
				Location = new Vector3(2488f, 2032f, 0f)
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
				Location = new Vector3(2688f, 3590f, 0f)
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
				Location = new Vector3(2064f, 3264f, 0f)
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
				Location = new Vector3(1556f, 3514f, 0f)
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
				Location = new Vector3(1580f, 428f, 0f)
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
				Location = new Vector3(1090f, 1000f, 0f)
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = new Vector2(2160f, 1776f)
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
			KeyName = "smallFog3",
			Location = new ValueNode
			{
				Location = new Vector2(1200f, 2640f)
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
				Location = new Vector2(3120f, 2304f)
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
				Location = new Vector2(3226f, 912f)
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
				Location = new Vector2(2400f, 2448f)
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
				Location = MapManager.TileToWorldPosVector2(new Point(8, 8))
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
				Location = MapManager.TileToWorldPosVector2(new Point(12, 11))
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
			KeyName = "sulphurousSmoke4",
			Scale = 2f,
			TimeBetweenEmissions = 0.5f,
			Location = new ValueNode
			{
				Location = new Vector2(3312f, 1824f)
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
			KeyName = "haze6",
			Scale = 3f,
			Location = new ValueNode
			{
				Location = new Vector2(3312f, 1824f)
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
