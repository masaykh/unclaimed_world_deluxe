using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

public class EventActionLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> list = new List<EventActionType>();
		double delayInSeconds = 0.25;
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
				String = "#NAMEOFDECEASED just died. \nI buried the remains as best I could. Guess it's only me now..."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBurialText5",
			PropertyKey = "burialTextMultipleDeathsSingleSurvivor",
			Value = new ValueNode
			{
				String = "#NAMEOFDECEASED just died. \nI buried the remains as best I could. Guess it's only me now..."
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
				String = "If not... well, I might leave for #EMIGRATETO and start over."
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
			KeyName = "meeting3Security",
			PropertyKey = "meeting3Security",
			Value = new ValueNode
			{
				String = "-#UNHAPPY: Thanks for listening... I want to talk about the security situation. The way it's handled, I don't think we're safe here. \n \n-#CONTENT: Could you elaborate on that? \n \n-#UNHAPPY: I want us to beef up security. How do we do that? Craft better weapons, take fewer risks... There's a number of ways we can protect ourselves better. Main thing is that we take action now. \n \n-#CONTENT: This is all a question of priorities... \n \n-#UNHAPPY: Exactly. And if we value our lives, we need better protection from the animals on this island. So I hope you're with me. #EMIGRATETHREAT \n \n-#CONTENT: Alright, thanks for sharing your thoughts. Anything else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Food",
			PropertyKey = "meeting3Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: I want to talk about the nutritional situation. \n \n#CONTENT: What is your concern? \n \n#UNHAPPY: Our foodstocks are grossly inadequate. We are at high risk of starvation. \n \n#CONTENT: Food has a high priority. \n \n#UNHAPPY: Not high enough. We need to work harder on acquiring and preserving food to avoid starvation. I hope you can see reason. #EMIGRATETHREAT \n \n#CONTENT: Ok we've heard you. Who else?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting3Comfort",
			PropertyKey = "meeting3Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Friends, the living conditions in this camp are distressing. \n \n#CONTENT: But surely, food and security are more important? \n \n#UNHAPPY: No. Living in squalor has a highly detrimental effect, mentally and physically. \n \n#CONTENT: We have to focus our efforts where they count. \n \n#UNHAPPY: The bad shelters and lack of recreation puts us in danger of disease and mental breakdown. I wish you would take this more seriously. #EMIGRATETHREAT \n \n#CONTENT: We'll keep this in mind. Who has something to add?"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Security",
			PropertyKey = "meeting2Security",
			Value = new ValueNode
			{
				String = "-#UNHAPPY: I've tried to convince you before. This is getting out of hand. We need to improve security. \n \n-#CONTENT: I don't see... \n \n-#UNHAPPY: Listen. We're in danger here. We need to craft better weapons, take fewer risks... and we have to act now! I hope you understand! #EMIGRATETHREAT \n \n-#CONTENT: I don't know what to say. I think there are so many other things that are more important."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Food",
			PropertyKey = "meeting2Food",
			Value = new ValueNode
			{
				String = "#UNHAPPY: We need to stock up on food. \n \n#CONTENT: Yeah, you keep saying this, but I think you're obsessing over something trivial. We have enough to eat. \n \n#UNHAPPY: You and me. We need to make this work. I do not want to end up in a situation where cannibalism is our only option. So please, see reason. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meeting2Comfort",
			PropertyKey = "meeting2Comfort",
			Value = new ValueNode
			{
				String = "#UNHAPPY: We need better shelters. \n \n#CONTENT: Why is it so important. We are wearing survival suits. \n \n#UNHAPPY: Look, the filth, the cold and the bugs are driving me mad. Please, let's work on the living conditions. #EMIGRATETHREAT \n \n"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingSecurityAllUnhappy",
			PropertyKey = "meetingSecurityAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Look, we all want the security situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingFoodAllUnhappy",
			PropertyKey = "meetingFoodAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Look, we all want the food situation to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "meetingComfortAllUnhappy",
			PropertyKey = "meetingComfortAllUnhappy",
			Value = new ValueNode
			{
				String = "#UNHAPPY: Look, we all want the comfort conditions to improve! So let's get our act together and start working as a team! What are you waiting for? #EMIGRATETHREAT"
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityDialogText",
			PropertyKey = "securityEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: We're in danger here. The twinklers scare me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving now. I know there's a safer island nearby. \n \n#NAME2: -You're actually gonna paddle to Knoll Island? You realize that's half an hour on the open sea? \n \n#NAME1: -The water is calm now and the currents are favorable. There's no doubt in my mind that I'm in greater peril if I stay here. Goodbye."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateSecurityNoConversationDialogText",
			PropertyKey = "securityEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. This place scares me. - We're in danger here. The twinklers scare me. But it scares me even more that you seem so cavalier about the threats we're facing. I'm leaving now. I know there's a safer island nearby. \nGoodbye."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortDialogText",
			PropertyKey = "comfortEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: This place is a pig sty. I can't stand it. The cold, the bugs and the filth...why are you ok with living like this? \n \n#NAME2: Because there are other things that are more important? \n \n#NAME1: I'm fed up. These conditions here, they're subhuman. I'm leaving for Knoll Island. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateComfortNoConversationDialogText",
			PropertyKey = "comfortEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. I'm fed up. These conditions here, they're subhuman. I'm leaving for Knoll Island. I can do better on my own."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodDialogText",
			PropertyKey = "foodEmigrateEventDialogText",
			Value = new ValueNode
			{
				String = "AUDIO LOG, #JOURNALDATE \n \n#NAME1: I can't do it anymore. I'm so hungry all the time. We're on the brink of starvation!  Why are you not addressing this problem? Look at us! We're dying here! \n \n#NAME2: Hey! We agreed to focus on other things! \n#NAME1: What could be more important than food?! I've had it. I'm going to Knoll Island. \n \n#NAME2: Good luck with crossing that water. Hope you'll find what you're looking for."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initEmigrateFoodNoConversationDialogText",
			PropertyKey = "foodEmigrateEventNoConversationDialogText",
			Value = new ValueNode
			{
				String = "TEXT LOG, #JOURNALDATE \n \n#NAME1: When you read this, I'll be on my way. I can't do it anymore. I'm so hungry all the time. We're on the brink of starvation. I've had it. I'm going to Knoll Island."
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initDeathCounter",
			PropertyKey = "deathCounter",
			Value = new ValueNode
			{
				Int = 0
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initSulfurDetected",
			PropertyKey = "sulfurDetected",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initNestDetected",
			PropertyKey = "nestDetected",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initNestDestroyed",
			PropertyKey = "nestDestroyed",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBushDragonDetectedShortDelay",
			PropertyKey = "bushDragonDetectedShortDelay",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initBushDragonDetectedLongDelay",
			PropertyKey = "bushDragonDetectedLongDelay",
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
		list.Add(new SetPropertyAction
		{
			KeyName = "initSandstoneWreckage",
			PropertyKey = "sandstoneWreckage",
			Value = new ValueNode
			{
				Bool = true
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initNoSandstoneWreckage",
			PropertyKey = "sandstoneWreckage",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initFledAtStart",
			PropertyKey = "fledAtStart",
			Value = new ValueNode
			{
				Bool = true
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initNotFledAtStart",
			PropertyKey = "fledAtStart",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initUnconsciousAndAlive",
			PropertyKey = "unconsciousAndAlive",
			Value = new ValueNode
			{
				Bool = true
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initCasualtyRescued",
			PropertyKey = "casualtyRescued",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "initFieldLabCannibalized",
			PropertyKey = "fieldLabCannibalized",
			Value = new ValueNode
			{
				Bool = false
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnConlan",
			DelayInSeconds = num,
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
					FirstName = "Ward",
					LastName = "Conlan",
					PersonalityType = "Conlan",
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
					RaceKey = "grey1",
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 1f },
						{ "hunting", 1f },
						{ "butchering", 0.8f },
						{ "fishing", 1f },
						{ "foraging", 1f },
						{ "cooking", 0.8f },
						{ "menial", 1f },
						{ "shooting", 1f },
						{ "armedMelee", 1f },
						{ "unarmedFighting", 0.8f },
						{ "psychology", 0.1f },
						{ "biology", 0.2f },
						{ "smithing", 0.8f },
						{ "mechanics", 0.8f },
						{ "electronics", 0.7f },
						{ "chemistry", 0.6f },
						{ "weaving", 0.15f },
						{ "carpentry", 0.6f },
						{ "farming", 0.8f },
						{ "weeding", 0.8f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "archery", 0.5f },
						{ "medicine", 0.5f },
						{ "sneaking", 0.5f }
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
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnLehner",
			DelayInSeconds = num,
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
					FirstName = "Joaquin",
					LastName = "Lehner",
					PersonalityType = "Lehner",
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
					RaceKey = "green2",
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.4f },
						{ "hunting", 0.7f },
						{ "butchering", 0.4f },
						{ "fishing", 0.5f },
						{ "foraging", 0.5f },
						{ "cooking", 0.5f },
						{ "menial", 0.6f },
						{ "shooting", 1f },
						{ "armedMelee", 0.8f },
						{ "unarmedFighting", 0.9f },
						{ "medicine", 0.6f },
						{ "psychology", 0.8f },
						{ "smithing", 0.8f },
						{ "mechanics", 0.7f },
						{ "electronics", 0.6f },
						{ "chemistry", 0.8f },
						{ "weaving", 0.28f },
						{ "carpentry", 0.4f },
						{ "biology", 0.6f },
						{ "farming", 0.4f },
						{ "weeding", 0.4f },
						{ "grasping", 0.6f },
						{ "fruitPicking", 0.5f },
						{ "construction", 0.5f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f }
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
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnYeboah",
			DelayInSeconds = num,
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
					FirstName = "Augustine",
					LastName = "Yeboah",
					PersonalityType = "Yeboah",
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
					RaceKey = "blue2",
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.6f },
						{ "hunting", 1f },
						{ "butchering", 0.7f },
						{ "fishing", 1f },
						{ "foraging", 0.6f },
						{ "cooking", 0.6f },
						{ "menial", 0.6f },
						{ "shooting", 1f },
						{ "armedMelee", 0.6f },
						{ "unarmedFighting", 0.6f },
						{ "medicine", 0.7f },
						{ "psychology", 0.5f },
						{ "smithing", 0.8f },
						{ "mechanics", 0.6f },
						{ "electronics", 0.7f },
						{ "chemistry", 0.8f },
						{ "weaving", 0.18f },
						{ "carpentry", 0.34f },
						{ "biology", 1f },
						{ "farming", 0.6f },
						{ "weeding", 0.6f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f }
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
								Mean = 0.5,
								StandardDeviation = 0.019999999552965164
							}
						}
					}
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "spawnKahn",
			DelayInSeconds = num,
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
					FirstName = "Ilya",
					LastName = "Khan",
					PersonalityType = "Khan",
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
					RaceKey = "red1",
					Skills = new SerializableDictionary<string, float>
					{
						{ "bushcraft", 0.4f },
						{ "hunting", 0.7f },
						{ "butchering", 0.6f },
						{ "fishing", 0.5f },
						{ "foraging", 0.5f },
						{ "cooking", 0.9f },
						{ "menial", 0.6f },
						{ "shooting", 1f },
						{ "armedMelee", 0.8f },
						{ "unarmedFighting", 0.9f },
						{ "medicine", 0.3f },
						{ "psychology", 0.2f },
						{ "smithing", 0.8f },
						{ "mechanics", 0.5f },
						{ "electronics", 0.5f },
						{ "chemistry", 0.6f },
						{ "weaving", 0.28f },
						{ "carpentry", 0.2f },
						{ "biology", 0.6f },
						{ "farming", 0.4f },
						{ "weeding", 0.4f },
						{ "grasping", 0.8f },
						{ "fruitPicking", 0.8f },
						{ "construction", 0.5f },
						{ "archery", 0.5f },
						{ "sneaking", 0.5f }
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
								Mean = 0.30000001192092896,
								StandardDeviation = 0.019999999552965164
							}
						}
					}
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "blackSmoke",
			DelayInSeconds = num,
			UseLocationOfEntity = new TargetObject
			{
				TargetObjectType = TargetObjectType.Root,
				GetList = new GetList
				{
					HasPropertiesListKey = "entities",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "type",
						ConstantStringEqual = "structure:skimmerHull"
					}
				}
			},
			DurationInSeconds = 20.0,
			ParticleEmitters = new ParticleEmitterEffect[1]
			{
				new ParticleEmitterEffect
				{
					ParticleSystemKey = "signalSmoke"
				}
			}
		});
		list.Add(new ParticleEffectAction
		{
			KeyName = "smallFog1",
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(20, 49))
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
				Location = MapManager.TileToWorldPosVector2(new Point(18, 43))
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
				Location = MapManager.TileToWorldPosVector2(new Point(19, 46))
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
			KeyName = "sulphurousSmoke2",
			TimeBetweenEmissions = 0.4f,
			Location = new ValueNode
			{
				Location = MapManager.TileToWorldPosVector2(new Point(41, 25))
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
				Location = MapManager.TileToWorldPosVector2(new Point(39, 27))
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
					FractionIndependentsAllowedToSleep = 0.5f,
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
			KeyName = "setStartingLocationSouth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(576f, 2400f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationNorth",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(1948f, 744f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setStartingLocationSouthEast",
			PropertyKey = "startingLocation",
			Value = new ValueNode
			{
				Location = new Vector2(2304f, 2400f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setWinGameEarly",
			PropertyKey = "winGameTime",
			Value = new ValueNode
			{
				Decimal = 3f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setWinGameMedium",
			PropertyKey = "winGameTime",
			Value = new ValueNode
			{
				Decimal = 5f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setWinGameLate",
			PropertyKey = "winGameTime",
			Value = new ValueNode
			{
				Decimal = 8f
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setRatNestSpawnIntervalOften",
			PropertyKey = "RatNestSpawnInterval",
			Value = new ValueNode
			{
				Int = 1600
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setRatNestSpawnIntervalSeldom",
			PropertyKey = "RatNestSpawnInterval",
			Value = new ValueNode
			{
				Int = 3200
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalSouthOften",
			PropertyKey = "twinklerSpawnIntervalSouth",
			Value = new ValueNode
			{
				Int = 360
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setTwinklerSpawnIntervalEastOften",
			PropertyKey = "twinklerSpawnIntervalEast",
			Value = new ValueNode
			{
				Int = 260
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
			KeyName = "setThunderChickenSpawnIntervalLow",
			PropertyKey = "thunderChickenSpawnInterval",
			Value = new ValueNode
			{
				Int = 1600
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setThunderChickenSpawnIntervalNormal",
			PropertyKey = "thunderChickenSpawnInterval",
			Value = new ValueNode
			{
				Int = 1200
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setThunderChickenSpawnIntervalHigh",
			PropertyKey = "thunderChickenSpawnInterval",
			Value = new ValueNode
			{
				Int = 800
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setBinalRatSpawnSeldom",
			PropertyKey = "binalRatSpawnInterval",
			Value = new ValueNode
			{
				Int = 120
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setBinalRatSpawnOften",
			PropertyKey = "binalRatSpawnInterval",
			Value = new ValueNode
			{
				Int = 60
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setThinThunderChickenSpawnIntervalSeldom",
			PropertyKey = "thinThunderChickenSpawnInterval",
			Value = new ValueNode
			{
				Int = 400
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setThinThunderChickenSpawnIntervalOften",
			PropertyKey = "thinThunderChickenSpawnInterval",
			Value = new ValueNode
			{
				Int = 200
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThinThunderChickenLow",
			PropertyKey = "maxThinThunderChicken",
			Value = new ValueNode
			{
				Int = 1
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThinThunderChickenNormal",
			PropertyKey = "maxThinThunderChicken",
			Value = new ValueNode
			{
				Int = 2
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThinThunderChickenHigh",
			PropertyKey = "maxThinThunderChicken",
			Value = new ValueNode
			{
				Int = 3
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxTwinklersLow",
			PropertyKey = "maxTwinklers",
			Value = new ValueNode
			{
				Int = 4
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
				Int = 25
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensLow",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 1
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensNormal",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 2
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxThunderChickensHigh",
			PropertyKey = "maxThunderChickens",
			Value = new ValueNode
			{
				Int = 3
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxBinalRatsLow",
			PropertyKey = "maxBinalRats",
			Value = new ValueNode
			{
				Int = 3
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxBinalRatsNormal",
			PropertyKey = "maxBinalRats",
			Value = new ValueNode
			{
				Int = 6
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setMaxBinalRatsHigh",
			PropertyKey = "maxBinalRats",
			Value = new ValueNode
			{
				Int = 10
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFarmSpotSmall1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:smallPlotSpot",
				Name = "Small plot ()",
				Location = new Vector3(2438f, 2574f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCreek1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCreek",
				Name = "Fish weir spot ()",
				Location = new Vector3(2424f, 1728f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot Saltwater ()",
				Location = new Vector3(2661f, 1395f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapCoast2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotCoast",
				Name = "Fish trap spot Saltwater ()",
				Location = new Vector3(336f, 1814f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore1",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater ()",
				Location = new Vector3(1086f, 832f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore2",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater ()",
				Location = new Vector3(2207f, 1646f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFishTrapShore3",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:fishTrapSpotShore",
				Name = "Fish trap spot Freshwater ()",
				Location = new Vector3(980f, 1331f, 0f)
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
				NoiseAmplitude = 1f,
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
				NoiseAmplitude = 1f,
				NoiseAddend = -2f,
				NoiseFrequency = 0.035f
			}
		});
		list.Add(new ChangeResourcesAction
		{
			KeyName = "setPlentyResources",
			AllResources = true,
			ExcludeResourceTypes = new string[22]
			{
				"stones", "firegrassSod", "crop:sticks", "crop:pigFlies", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth",
				"clamwich", "torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves",
				"crop:waterCaneStem", "crop:shadeleafCanes"
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
			ExcludeResourceTypes = new string[21]
			{
				"stones", "firegrassSod", "crop:sticks", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich",
				"torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves", "crop:waterCaneStem",
				"crop:shadeleafCanes"
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
			KeyName = "setSparseResources",
			AllResources = true,
			ExcludeResourceTypes = new string[21]
			{
				"stones", "firegrassSod", "crop:sticks", "vine", "sulfurDeposit", "streakFin", "carbonTail", "alabasterRay", "daggermouth", "clamwich",
				"torux", "minnowsLive", "phantomWeaver", "ursinix", "webWing", "crestedFoiler", "goldenCenobite", "muckGrinder", "crop:daysheenLeaves", "crop:waterCaneStem",
				"crop:shadeleafCanes"
			},
			OperationToUse = ChangeResourcesAction.Operation.Multiply,
			NoiseParameters = new NoiseParams
			{
				NoiseAddend = 0.1f,
				NoiseAmplitude = 0.5f,
				NoiseFrequency = 0.1f
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "naturalTerminalSW",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalPseudoWaterTerminal",
				Name = "To Knoll Island",
				Location = new Vector3(326f, 2496f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "naturalTerminalN",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:naturalPseudoWaterTerminal",
				Name = "To Knoll Island",
				Location = new Vector3(2090f, 400f, 0f)
			}
		});
		list.Add(new SpawnWorldAction
		{
			KeyName = "spawnWorld",
			WorldData = new WorldData
			{
				WorldRadius = GameData.Instance.Constants.DefaultWorldRadius,
				ViewLongitudeStart = 11f,
				ViewLongitudeEnd = 15f,
				ViewLatitudeStart = 69f,
				ViewLatitudeEnd = 72f
			}
		});
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnWildernessSite1",
			SiteData = new SiteData
			{
				Name = "Knoll Island",
				KeyName = "wildernessSite1",
				Description = "A tiny island some kilometers away. Just might have better conditions, but we wouldn't know unless we crossed the water somehow.",
				Coords = new GeodeticCoordinate(13.08, 70.755),
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
				Name = "",
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
		list.Add(new SpawnSiteAction
		{
			KeyName = "spawnPlaySite",
			SiteData = new SiteData
			{
				Name = "Twinkler Island",
				KeyName = "playSite",
				Description = "Where we've crashed: An uncharted, vulcanic island. The biome seems to support a substantial amount of larger predators.",
				Coords = new GeodeticCoordinate(13.2, 70.68),
				IsPlaySite = true,
				ShowLabel = true,
				ShowTallPin = false,
				SiteMarkerOrder = 10
			}
		});
		list.Add(new SpawnRouteAction
		{
			KeyName = "spawnPlaySiteWildernessSite1Route",
			DelayInSeconds = 1.0,
			RouteData = new RouteData
			{
				Name = "Strait crossing",
				FromSite = "playSite",
				ToSite = "wildernessSite1",
				Length = 8f,
				RouteType = RouteType.Land
			}
		});
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "DEMOISLANDMAP_spawnThunderChickenAllegianceNorth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thunderChickenAllegianceNorth",
				Name = "thunderChickenAllegianceNorth",
				AllegianceKey = "thunderChickenAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(816f, 1392f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 600,
				Name = "Thunderchicken Allegiance",
				KeyName = "thunderChickenAllegianceNorth",
				EntityType = "entity:pygmyThunderChicken",
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
			KeyName = "DEMOISLANDMAP_thinThunderChickenAllegianceSouth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thinThunderChickenAllegianceSouth",
				Name = "thinThunderChickenAllegianceSouth",
				AllegianceKey = "thinThunderChickenAllegianceSouth",
				Location = new ValueNode
				{
					Location = new Vector2(960f, 2640f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1200,
				Name = "Thin Thunder Chicken Allegiance South",
				KeyName = "thinThunderChickenAllegianceSouth",
				EntityType = "entity:bajingan",
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
			KeyName = "DEMOISLANDMAP_thinThunderChickenAllegianceNorth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thinThunderChickenAllegianceNorth",
				Name = "thinThunderChickenAllegianceNorth",
				AllegianceKey = "thinThunderChickenAllegianceNorth",
				Location = new ValueNode
				{
					Location = new Vector2(2688f, 816f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1200,
				Name = "Thin Thunder Chicken Allegiance North",
				KeyName = "thinThunderChickenAllegianceNorth",
				EntityType = "entity:bajingan",
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
			KeyName = "DEMOISLANDMAP_spawnThunderChickenAllegianceSouth",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				KeyName = "thunderChickenAllegianceSouth",
				Name = "thunderChickenAllegianceSouth",
				AllegianceKey = "thunderChickenAllegianceSouth",
				Location = new ValueNode
				{
					Location = new Vector2(1632f, 2064f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 600,
				Name = "Thunderchicken Allegiance",
				KeyName = "thunderChickenAllegianceSouth",
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
		list.Add(new SpawnAllegianceAction
		{
			KeyName = "DEMOISLANDMAP_spawnTwinklerAllegiance",
			Site = "playSite",
			ExpeditionData = new ExpeditionData
			{
				Name = "twinklerAllegiance",
				AllegianceKey = "twinklerAllegiance",
				Location = new ValueNode
				{
					Location = new Vector2(1296f, 1464f)
				}
			},
			AllegianceData = new AllegianceData
			{
				ForageAndHuntingRadius = 1082,
				Name = "Twinkler Allegiance",
				KeyName = "twinklerAllegiance",
				EntityType = "entity:twinkler",
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
			KeyName = "placeNestSandstone",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:quaditeNest",
				Name = "Quadite nest (coord. 15;34)",
				Location = new Vector3(732f, 1510f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "twinklerAllegiance"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeNestRockEast",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:quaditeNest",
				Name = "Quadite nest (coord. 38;38)",
				Location = new Vector3(1800f, 1800f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "twinklerAllegiance"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeNestRockSouth",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:quaditeNest",
				Name = "Quadite nest (coord. 25;44)",
				Location = new Vector3(1220f, 2130f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "twinklerAllegiance"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeNestRockNorth",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:quaditeNest",
				Name = "Quadite nest (coord. 36;20)",
				Location = new Vector3(1728f, 980f, 0f),
				Threat = new Threat
				{
					ThreatGroupName = "twinklerAllegiance"
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeCratesSandstone",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:crates",
				Name = "Crates",
				Location = new Vector3(552f, 1397f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeUnconsciousAtBramble",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:unconscious",
				Name = "Unconscious",
				Location = new Vector3(1400f, 1710f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "placeUnconsciousAtSandstone",
			DelayInSeconds = 1.0,
			EntityData = new EntityData
			{
				EntityKey = "terrain:unconscious",
				Name = "Unconscious",
				Location = new Vector3(552f, 1397f, 0f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setRescueSpawnLocationSandstone",
			PropertyKey = "rescueSpawnLocation",
			Value = new ValueNode
			{
				Location = new Vector2(480f, 1392f)
			}
		});
		list.Add(new SetPropertyAction
		{
			KeyName = "setRescueSpawnLocationBramble",
			PropertyKey = "rescueSpawnLocation",
			Value = new ValueNode
			{
				Location = new Vector2(1392f, 1776f)
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
			KeyName = "exploreShroudNorth",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(324f, 1500f),
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
			KeyName = "exploreShroudSouth",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 500f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(324f, 2648f),
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
			KeyName = "exploreShroudFledSouthWestWreckAtSandstone",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 300f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1104f, 1392f),
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
			KeyName = "exploreShroudFledSouthWestWreckAtSandstoneFlightPath",
			DelayInSeconds = num + 1.0,
			OffsetLocationStart = new Vector2(48f, 1344f),
			RadiusStart = 300f,
			RadiusEnd = 300f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1104f, 1392f),
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
			KeyName = "exploreShroudFledSouthWestWreckAtBramble",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 300f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1872f, 1680f),
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
			KeyName = "exploreShroudWreckAtBrambleFlightPath",
			DelayInSeconds = num + 1.0,
			OffsetLocationStart = new Vector2(48f, 2000f),
			RadiusStart = 300f,
			RadiusEnd = 300f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1872f, 1680f),
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
			KeyName = "exploreShroudFledSouthEastWreckAtBramble",
			DelayInSeconds = num + 1.0,
			DynamicLocationStart = new ValueNode
			{
				PropertyKey = "startingLocation"
			},
			RadiusStart = 300f,
			RadiusEnd = 300f,
			DetectMode = DetectMode.DetectAlwaysSeenEntities,
			OffsetLocationEnd = new Vector2(1872f, 1680f),
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
		list.Add(new DestroyEntityAction
		{
			KeyName = "destroySkimmerHull",
			DelayInSeconds = 1.0,
			TargetObject = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "entities",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "type",
						ConstantStringEqual = "structure:skimmerHull"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "itemParts",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "type",
							ConstantStringEqual = "item:scrapMetal"
						}
					}
				}
			}
		});
		list.Add(new DestroyEntityAction
		{
			KeyName = "destroySkimmerEngineSide",
			DelayInSeconds = 1.0,
			TargetObject = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "entities",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "type",
						ConstantStringEqual = "structure:skimmerEngineSide"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "itemParts",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "type",
							ConstantStringEqual = "item:scrapMetal"
						}
					}
				}
			}
		});
		list.Add(new DestroyEntityAction
		{
			KeyName = "destroySkimmerEngineTop",
			DelayInSeconds = 1.0,
			TargetObject = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "entities",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "type",
						ConstantStringEqual = "structure:skimmerEngineTop"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "itemParts",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "type",
							ConstantStringEqual = "item:scrapMetal"
						}
					}
				}
			}
		});
		list.Add(new DestroyEntityAction
		{
			KeyName = "destroySkimmerTail",
			DelayInSeconds = 1.0,
			TargetObject = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "entities",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "type",
						ConstantStringEqual = "structure:skimmerTail"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "itemParts",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "type",
							ConstantStringEqual = "item:scrapMetal"
						}
					}
				}
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerHullSouth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				Name = "Aircraft wreck (hull)",
				EntityKey = "structure:skimmerHull",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(672f, 2450f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineTopSouth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineTop",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(656f, 2402f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineSideSouth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineSide",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(704f, 2482f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerTailSouth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerTail",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(576f, 2450f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerHullNorth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				Name = "Aircraft wreck (hull)",
				EntityKey = "structure:skimmerHull",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(2036f, 778f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineTopNorth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineTop",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(2020f, 730f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineSideNorth",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineSide",
				OwnedBy = new AllegianceAndExpedition
				{
					AllegianceKey = "playerAllegiance"
				},
				Location = new Vector3(2068f, 810f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerHullSandstone",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				Name = "Aircraft wreck (hull)",
				EntityKey = "structure:skimmerHull",
				Location = new Vector3(1104f, 1344f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineTopSandstone",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineTop",
				Location = new Vector3(1100f, 1304f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineSideSandstone",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineSide",
				Location = new Vector3(1136f, 1376f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerTailSandstone",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerTail",
				Location = new Vector3(624f, 1358f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFieldLabSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:fieldLabPacked",
				Location = new Vector3(1026f, 1372f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startRationSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:astroRation",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentrySandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentry",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentryWeaponMountSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentryWeaponMount",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startShotgunAmmoSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:shotgunAmmo",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentryGunAmmoSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentryGunAmmo",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBasicFireExtinguisherSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:basicFireExtinguisher",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFireSuppressantCartridgeSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:fireSuppressantCartridge",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startEmptyCartridgeSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:emptyCartridge",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSnipsSandstone",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:advancedSnips",
				Location = new Vector3(1056f, 1392f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFieldLabBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:fieldLabPacked",
				Location = new Vector3(1862f, 1733f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startRationBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:astroRation",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentryBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentry",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentryWeaponMountBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentryWeaponMount",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startShotgunAmmoBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:shotgunAmmo",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSentryGunAmmoBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:sentryGunAmmo",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startBasicFireExtinguisherBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:basicFireExtinguisher",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startFireSuppressantCartridgeBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:fireSuppressantCartridge",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startEmptyCartridgeBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:emptyCartridge",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSnipsBrambleEast",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:advancedSnips",
				Location = new Vector3(1882f, 1753f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startMacheteBrambleWest",
			DelayInSeconds = 0.0,
			EntityData = new EntityData
			{
				EntityKey = "item:advancedMachete",
				Location = new Vector3(1375f, 1759f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerHullBramble",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				Name = "Aircraft wreck (hull)",
				EntityKey = "structure:skimmerHull",
				Location = new Vector3(1852f, 1700f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineTopBramble",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineTop",
				Location = new Vector3(1836f, 1652f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerEngineSideBramble",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerEngineSide",
				Location = new Vector3(1884f, 1732f, 0f)
			}
		});
		list.Add(new SpawnEntityAction
		{
			KeyName = "startSkimmerTailBramble",
			DelayInSeconds = delayInSeconds,
			EntityData = new EntityData
			{
				EntityKey = "structure:skimmerTail",
				Location = new Vector3(1460f, 1760f, 0f)
			}
		});
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBow", new Vector2(124f, -24f), "item:improvisedBow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startArrows", new Vector2(124f, -24f), "item:improvisedBasicArrow", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCrystalBerries", new Vector2(124f, -24f), "item:crystalBerries", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startCreeperPods", new Vector2(124f, -24f), "item:glassyCreeperPods", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHoe", new Vector2(124f, -24f), "item:farmingHoe", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSpear", new Vector2(124f, -24f), "item:improvisedGoodSpear", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBait", new Vector2(124f, -24f), "item:neonHornetsLive", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHook", new Vector2(124f, -24f), "item:improvisedMetalHooks", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startTwinklerMeat", new Vector2(124f, -24f), "item:twinklerMeat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVat", new Vector2(124f, -24f), "item:vat", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startWaterCaneStem", new Vector2(124f, -24f), "item:waterCaneStem", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startVine", new Vector2(124f, -24f), "item:vine", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSulfurBomb", new Vector2(124f, -24f), "item:sulfurSmokeBomb", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startImprovedFireExtinguisher", new Vector2(124f, -24f), "item:improvedFireExtinguisher", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startBushDragonCartridge", new Vector2(124f, -24f), "item:bushDragonCartridge", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentry", new Vector2(124f, -24f), "item:sentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryWeaponMount", new Vector2(124f, -24f), "item:sentryWeaponMount", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSprayGunSentry", new Vector2(124f, -24f), "item:spraySentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSentryGunAmmo", new Vector2(124f, -24f), "item:sentryGunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShotgunSentry", new Vector2(124f, -24f), "item:shotgunSentry", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShotgunAmmo", new Vector2(124f, -24f), "item:shotgunAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startShotgun", new Vector2(124f, -24f), "item:shotgun", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startGoggles", new Vector2(124f, -24f), "item:nightVisionGoggles", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSensor", new Vector2(124f, -24f), "item:sensor", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startKnife", new Vector2(124f, -24f), "item:advancedKnife", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startSnips", new Vector2(124f, -24f), "item:advancedSnips", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startString", new Vector2(124f, -24f), "item:advancedString", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startThermalTarp", new Vector2(124f, -24f), "item:thermalTarp", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRation", new Vector2(124f, -24f), "item:astroRation", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startHuntingRifle", new Vector2(124f, -24f), "item:coilRifle", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startRifleAmmo", new Vector2(124f, -24f), "item:coilRifleAmmo", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("startMachete", new Vector2(124f, -24f), "item:advancedMachete", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemAtStartLocation("testShadeleafCanes", new Vector2(124f, -24f), "item:shadeleafCanes", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startEmptyCartridgeInSkimmer", "Aircraft wreck (hull)", "item:emptyCartridge", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startBasicFireExtinguisherInSkimmer", "Aircraft wreck (hull)", "item:basicFireExtinguisher", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startFireSuppressantCartridgeInSkimmer", "Aircraft wreck (hull)", "item:fireSuppressantCartridge", "playerAllegiance", null, num));
		list.Add(ScenarioLoader.SpawnItemInsideContainer("startFieldLabInSkimmer", "Aircraft wreck (hull)", "item:fieldLabPacked", "playerAllegiance", null, num));
		return list;
	}
}
