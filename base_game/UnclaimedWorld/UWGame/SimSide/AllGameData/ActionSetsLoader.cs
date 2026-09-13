using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData;

public class ActionSetsLoader
{
	public static List<ActionSets> Init()
	{
		List<ActionSets> list = new List<ActionSets>();
		list.Add(new ActionSets
		{
			KeyName = "SANDBOXMAP_newPlayerAllegianceMemberRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("1dfaf2-3545yu-iopp38-83a3-a65c34756a91")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("de0c6aa5-80sdawdsdfgrrr-a098-e3c68a3150a3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Thanks for letting me join you! Hope I can be of use!"
						},
						new TalkAction("fecfav32543-wgpdwr225q-fa43qafp04-7f38aa3b0878")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Welcome to Castor's Homestead!"
						}
					}
				},
				new ActionSetType("323wefsdfghh-fqwe325trpop-q591-9c47-de1f2fdcb2c2")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("373254trgfdhjuiopx-xgwfd45-39d4d79c959f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							TurnTowardsListeners = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hello everyone. I'm glad to be part of your colony!"
						},
						new TalkAction("9c432wesdsfgrthyuiop-hgfed-ad48-fb385d2a5a3a")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Glad to have you here!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.15f,
			SetsOfActions = new ActionSetType[12]
			{
				new ActionSetType("3fbd898a-eabc-48a2-a752-425c02cb28a2")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("7f1b0330-81d0-4b1e-80e1-7d6085099040")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I got this one."
						}
					}
				},
				new ActionSetType("b077ee2e-4983-47c0-b45c-a507f9065844")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5afd04ab-5c13-4ea1-b539-b8233194fcfe")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll handle this one."
						}
					}
				},
				new ActionSetType("0d667b13-dd1b-4e64-8e64-3dcb83c8d306")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ea895067-8ac4-4312-bcb6-08ef5a475193")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Shoo!"
						}
					}
				},
				new ActionSetType("e4789a8b-1798-4e8b-9f03-2dbb0e77d789")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("02386b87-8c83-4d0a-87c8-aab054499632")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm on it."
						}
					}
				},
				new ActionSetType("bcaad631-1e7f-42ad-8bbc-2ef211883b18")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("a5d395a0-de32-4bbe-bda1-d974f96ac63d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Take that!"
						}
					}
				},
				new ActionSetType("ad580b2a-9e9b-4964-b1f1-bbca4a6d4ebd")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("7aa0972c-c308-493c-ae99-18dd5b777a91")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "YEAAHHH!"
						}
					}
				},
				new ActionSetType("1a0c4530-905e-4fca-ae06-eb8821de04a3")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("16820096-7cc7-4b35-a40c-367590b48a67")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Come get some!"
						}
					}
				},
				new ActionSetType("7b0bda50-e343-4a44-8c74-17d311c3bec6")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9aaa9334-eaff-4eca-a933-0b1d0481832d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Had enough?!"
						}
					}
				},
				new ActionSetType("5581c027-37ea-4324-9cea-3446dc446f1d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4646e45a-3003-4f23-b8b8-64fef0096c20")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Go away!"
						}
					}
				},
				new ActionSetType("555f36c7-c527-44b6-8e2a-d882ce1f07f1")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ef03bc88-8a73-4c90-92bd-115e0e317000")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Get lost!"
						}
					}
				},
				new ActionSetType("a542a822-5924-4093-b70a-b08833a36e58")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b55b1171-0c13-4079-910e-52458e9b5fd5")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Get away from me!!"
						}
					}
				},
				new ActionSetType("edab9761-08dd-4641-a66c-8c45453301fb")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("67be99a6-d1d0-4cba-ae92-06625d013e93")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I hate you."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startMakeStrangeAnimalMeal",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("2c886683-3892-4454-820c-5832104d20bc")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("1085e8fc-ced6-4b3b-9fd0-21bf981a8b72")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Are you guys serious about eating this?"
						},
						new TalkAction("54272a18-cab7-48f4-8832-b4a7114797fb")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Do your best and try to make it look like a meal."
						},
						new TalkAction("bd1c88f8-9b41-492e-a689-2d22cd369cfa")
						{
							DelayInSeconds = 6.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Alright, here goes."
						}
					}
				},
				new ActionSetType("7a29d343-d4dd-4606-913b-ddc530554669")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("eeecd7ec-23f2-4c68-adcc-574c4a5d7468")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You ever tried this dish?"
						},
						new TalkAction("d0ab1777-83d5-4c1f-b996-a056f8e76c6c")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "No, I always managed to avoid it."
						},
						new TalkAction("13a35717-4c65-4c5d-bf37-020a0570d8ee")
						{
							DelayInSeconds = 6.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Well, it's about time you had a taste."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endConstructWigwamSpoakShingles",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("b85e8b6f-3528-44d8-a1cf-54f7abf5ba6d")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("4098a24b-e6c8-4abb-9dc4-0261f13ed84c")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hope this was worth the effort?"
						},
						new TalkAction("1a30cb2d-cb55-4c6d-bd9c-83e446a5df41")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							TurnTowardsListeners = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Definitely!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectWhiteThunderChicken",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("a5232bf4-6764-4d5c-be7b-0f0d1e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c98-c8d3-4e19-9e82-1115dc6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Thunder chickens! Come here buddy."
						},
						new TalkAction("f3f4209f-a0a5-4019-bb29-dadc47271ee0")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "He's too clever for that."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectTwinklerTalk",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("01d04faw242asd0-df89-4ee1-9b49-3784495aab81")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("2f7c6ec7awd242aasfa-4db4-94be-f0691271e6ad")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Twinklers are rather common in these parts."
						},
						new TalkAction("8416dwa242asfsaf3-4ba6-9dc7-1467aa92ad16")
						{
							DelayInSeconds = 3.5,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Yeah, I see them all the time."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "detectDemonTreeTalk",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("a5232bf4-6732542353252-d5csdf-be7b-0f0d1e4aze32qb2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618fsa325265sdfg8-c8d3-4e19-9e82-1115afhhpppdc6d43wqa4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Whoa! Where did that come from!"
						},
						new TalkAction("sd35f3f4209f-a0a5-4019-bbdxvx29-z4rda34tdzvc47271ee0")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Dendronts are sneaky!"
						}
					}
				},
				new ActionSetType("z4a5232dsa-d3212352-4d5csdf-be7b-0sdf0d1ez4gta44b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c161dad529sxxdfg8-c8d3-4safa35e2219-9e82-111ffsfa5dc65746d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Everyone, watch out. There's a dendront here!"
						},
						new TalkAction("ffaf253f4209f-a0a5-4019-bb29-da34tds-adyupc4df-hw4447271ee0")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yeah, stay clear of those woods! There might be more!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectPygmyThunderChicken",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("bce52303-d312-4556-b578-1f1dec8d5abd")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new TalkAction("9df62c8c-5a7e-47b2-b41b-05693e2ab9d4")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hey little fella."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "detectWhipjawTalk",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("sxdza5232bf4-6764-4d5csdf-be7b-0f0d1e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c9sdfg8-c8d3-4e19-9e82-1115dc6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I see whipjaw."
						},
						new TalkAction("f3fasfd321352d-a0a5-4019-bb29-gdcda34tdc47271ee0")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "We should kill it."
						}
					}
				},
				new ActionSetType("a5232fe324-56879pppsdf-be7b-0f0d1e4bzsae2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c-2354tsopve-wfehtu-khgczxz9e82-1115dcze6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Uh-oh. One of those meanies."
						},
						new TalkAction("f3f420asf3ef-sdfw5-4019-bb29-da34tdc47271ee0")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Don't let it hear you!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectBushDragon",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("c4e1a847-5ab8-477f-8066-428fe264c5b4")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new TalkAction("53748e3f-c146-4dde-ac1b-816f09fd6053")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Bush dragons. Their waddling always makes me smile."
						},
						new TalkAction("d1f300f2-2c3b-4096-9664-12b4dae2104b")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yeah, and the way they shake their wings..."
						},
						new TalkAction("ff193116-35fe-43ed-b4f5-831e3982add7")
						{
							DelayInSeconds = 4.5,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "...until they spray that poison at you."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "bigBombTalk",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("d92a701d-ab98-447d-9b02-7cccbfzbe39f5")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("de0c6aa5-80d3-4a45-a0asdggegeg-8a3150a3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm blowing this up, stay clear!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.5f,
			SetsOfActions = new ActionSetType[11]
			{
				new ActionSetType("4437db39-4dac-401a-b654-83e05d04af69")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("443d2330-92f1-4665-8b5d-657d8b0cf3c9")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Gotcha!"
						}
					}
				},
				new ActionSetType("e171d2a3-99f0-45a9-a41a-585dd26f7314")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("1f6ad520-3471-47cd-8e24-6799415356e4")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I got that one."
						}
					}
				},
				new ActionSetType("60f22a6c-abd8-4d80-b581-1f9ca96b2d4e")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("fab24902-2e57-474a-b3ab-6a27574def41")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I took care of that one."
						}
					}
				},
				new ActionSetType("8863d46f-de2e-4445-9cd3-4b52cc044978")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5fbb0c01-ba77-4cbb-ac40-52f17c7224c5")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Are there more?"
						}
					}
				},
				new ActionSetType("115dedcf-5933-4fbe-9480-8744c5fbfedf")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("43195dde-622b-4911-b2a7-4054895e78fe")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I got it!"
						}
					}
				},
				new ActionSetType("b5755e8f-6c1a-4fa9-a400-06e0ac20cada")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("35476dc1-713d-49c8-8778-5c372af22553")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "It's dead now."
						}
					}
				},
				new ActionSetType("8b10e8ee-95cc-4881-a61b-e00c8f14fa32")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("710a4d12-c289-481d-bd2b-2cb096bc6d50")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "One less!"
						}
					}
				},
				new ActionSetType("7716f33d-0554-42dd-a006-ff4523725fbf")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("0a3d6e10-8b8d-47fa-b0e3-e2f8a160286d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I got you, huh?!"
						}
					}
				},
				new ActionSetType("bf26df34-56f2-4f6b-ba4a-6f9536bfa650")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("636dbb91-5685-4e13-90e8-dcd3a2819cb1")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Didn't see that coming, did you?!"
						}
					}
				},
				new ActionSetType("ea1ff963-b2fc-4dd9-8725-18d46ddc365b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("791b8a0c-07e5-4fd9-b457-298f4def66a2")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Why did you make me do that..."
						}
					}
				},
				new ActionSetType("e46834d0-f426-4d5b-964c-8e54b566e104")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("974c3824-ebd1-43fc-9240-c465bdea63a3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Who's next?!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanMissedEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.15f,
			SetsOfActions = new ActionSetType[8]
			{
				new ActionSetType("1f751986-141c-4e6c-a70a-03ba12931c63")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5ba138ac-816c-41e6-873a-d02d770025b9")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "The thing moved!"
						}
					}
				},
				new ActionSetType("c6bead81-b302-4e9b-9992-00451ae78c01")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("1e905967-5878-4433-a214-f067882fd7a3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "The heck?!"
						}
					}
				},
				new ActionSetType("6cab987f-6c3e-4173-9c1b-fa96b39be144")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("fa938087-0ef5-43b2-ade0-dce57283580b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hey?!"
						}
					}
				},
				new ActionSetType("7166c32b-d276-4b8d-b190-fc2a543c9234")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b748c97e-5d55-4b5c-b818-e718be3300d2")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Wha-?!"
						}
					}
				},
				new ActionSetType("2ca02e5e-9473-426f-9747-1182a6aa1c01")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("6683a8e0-5dd9-4c7a-a8b9-ece097b0ce82")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Oops!"
						}
					}
				},
				new ActionSetType("bea14810-1a10-4917-bfef-9bb23f4a606e")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d1bccd71-92d1-4c97-bbee-d58a885808a7")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Whoa!"
						}
					}
				},
				new ActionSetType("344d97be-4fc5-4d08-a290-f7c2c03ce32a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4a622593-3484-420c-b6d3-e86e046838c8")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Dammit!"
						}
					}
				},
				new ActionSetType("3a1939e7-2d8c-475d-a8a8-5cc7e8a7c2a5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("bd551576-5bb5-4b55-946f-1788ef6015cf")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Don't move!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitByEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.5f,
			SetsOfActions = new ActionSetType[8]
			{
				new ActionSetType("a8c828e5-20fc-4ad3-ad9d-b3b28cfecadd")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("a51fa359-3aad-46ea-b5ba-bf881d8acc51")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "AAAH! You bastard!"
						}
					}
				},
				new ActionSetType("e226952e-5a2f-4584-ad31-915554e540f0")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("73cef408-8516-4632-b604-0056ad428f71")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "That thing got me!"
						}
					}
				},
				new ActionSetType("26d8212e-87e2-4037-b530-6d36882a0623")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("58698885-4f64-49f9-927d-701758ad97fd")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ouch!"
						}
					}
				},
				new ActionSetType("5361f03d-ff38-490d-bee8-672750c3227e")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3a988027-5922-43de-8967-8a36ffc47d3f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hngh...that hurt!"
						}
					}
				},
				new ActionSetType("79a6f40c-7939-47ec-b96a-7b07066ba2bb")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("8dfdd480-bd27-4a3b-8eee-637a38acaf81")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Umph!"
						}
					}
				},
				new ActionSetType("06095282-246e-410b-bbc5-1526b7699d53")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b1c727fe-3b23-48e9-928f-75fb126d9733")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Argh!...I'm in trouble here!"
						}
					}
				},
				new ActionSetType("b6bb75e6-0cb4-4d0b-ae23-26594e99ec89")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("967dc275-92f0-4d91-bae0-3b7958a20717")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "NNNGHH...!"
						}
					}
				},
				new ActionSetType("e55275f2-4b45-40b3-8bbf-0cc13c0d2aa5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3b3b8766-1a45-4dd5-9968-df0c8d72faf6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "UGHH!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledInCombatRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("987ebe81-671c-4136-b76f-2e7479869f40")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("0020baea-d4ed-4fe4-8b6a-66efb76663d3")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hnnngrlll..."
						}
					}
				},
				new ActionSetType("fb7ed8e4-92d7-4462-8d0d-b990a9b04c50")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("43727884-4270-4c4a-9163-44f4bdbb3864")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ugh......hhhhgll.."
						},
						new TalkAction("cb8b2239-411e-4e66-a21a-264b84b70e70")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Hey! You OK? Hey! HEY!!"
						}
					}
				},
				new ActionSetType("624be9fc-4eba-40ee-8795-a74f0d201c5e")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("8eee0173-f3a3-4b7c-8b79-d43addb674a8")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Oh sh......GHKRRRLL.."
						}
					}
				},
				new ActionSetType("4edaf746-60a5-43c4-bf47-07abe83d5156")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("f636c839-104f-4d10-b748-ea4e46cc2879")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Nooo...aarrrghhhh.."
						}
					}
				},
				new ActionSetType("ec7c8082-3fd3-46ee-9618-716639b0292f")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9f3acd91-7855-4a57-a2f6-0906c6d685fa")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "No! D-damn you....."
						}
					}
				},
				new ActionSetType("48858b42-8f65-4ee3-a2c3-89bc0a41e98c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9c1b394d-ce19-469d-b746-705b5d1028e1")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "NNNGHH...!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanFleeingRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.85f,
			SetsOfActions = new ActionSetType[7]
			{
				new ActionSetType("2b99bea7-8900-40d9-8278-1c4afc78a2fd")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("bdca23d9-b908-4923-8d92-6976c4ce8045")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm getting out of here!!"
						}
					}
				},
				new ActionSetType("9994ee21-a652-4a68-ae6d-fb2be6c71bec")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("7b44db10-83fc-494d-a426-1c6f24397aa8")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ohhhh nooo..!"
						}
					}
				},
				new ActionSetType("e47a32d1-c786-4786-8c5c-4fc1a7beaf84")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("bd7154e8-3999-4373-9a38-8aa9acdeea36")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Guys!! HELP!! HELP!!"
						},
						new TalkAction("edc1ab79-2af7-4d43-8296-41ac9b935e51")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "We're coming!"
						}
					}
				},
				new ActionSetType("52541cd6-7381-47cf-a651-bfa77e10e99a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e6425265-edd0-4432-a65b-3fc32624ec11")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Help me out!!!"
						},
						new TalkAction("c4456b5a-1e9f-4822-b0b7-17f58e19aaa4")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Hang in there!"
						}
					}
				},
				new ActionSetType("ab9328cd-afbf-41c2-9c72-cbce9bed9b15")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4c6aefa4-1658-46c9-b60e-38e6421d46e0")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "AAAAAAAAAAAAAAHHHHHH!!!"
						}
					}
				},
				new ActionSetType("e7dffe7c-b8f5-429a-99d5-2aa476aef444")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5f36760b-a1ab-44a5-ad08-019079b30dfd")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "NO-NO-NO-NO!!!!"
						}
					}
				},
				new ActionSetType("7019d934-8e06-4731-b66a-404bb13ebb4b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3311d66c-b618-4de1-b54e-83a0dba1646a")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Keep on running...just...run..."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "usesStimulantRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.2f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("4e33be39-da40-43f0-a4fe-264d63651925")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("17b47964-3c0f-45cb-a790-f7ecd635c4ee")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I can feel the buzz."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endedStimulantRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.2f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("eb2d619f-c74f-473c-8242-cef218d4cb36")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("91e7410c-6485-475c-a8ca-d1276d192d2f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "The buzz is wearing off."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitEnemyWithBushDragonPoisonRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.7f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("fd22ad8f-fde1-489b-950e-dfd7de57f175")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("27b2f0d4-7caa-4532-96ed-ffd013c2ef35")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I hit it...hope this poison works."
						}
					}
				},
				new ActionSetType("d3df4366-6e2a-40d6-a785-315ba4be4c8c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("72a6375e-dfcd-4f09-9a56-0282fff2d1ec")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Let's see if the poison has any effect..."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledWithImprovisedMetalArrowRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.8f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("dc5d1cc0-c77d-4be4-b0b4-631a5bbb0bed")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b4b2e61c-3321-4f97-8841-e4d460f41b88")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "The metal heads cut right through!"
						}
					}
				},
				new ActionSetType("488ea725-e716-4bff-83ab-c35b771c937f")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("373869ff-d367-4fb3-aebe-64f42d91e532")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I like these metal head arrows!"
						}
					}
				},
				new ActionSetType("19f1f570-d613-4432-b097-f94bb09abe3f")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("12d54997-1814-4f5f-ba2b-089d335236f3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "YEAH. We need more arrows like these."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledWithImprovisedBasicArrowRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.7f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("eacc4f46-5f06-40b7-8d07-51c1ad8185aa")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("19522f8f-bbce-4644-87f6-c09655ddd54e")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "YEAH! This bow is actually working!"
						}
					}
				},
				new ActionSetType("7756427e-14d0-4b73-84eb-f8d4e2839ed1")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3fc032e2-2a42-4172-8d77-1a19c5b5929b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This bow is great you know."
						}
					}
				},
				new ActionSetType("1b1df832-9366-4a8a-9ff1-cc7a188ad837")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b50b9d25-e6ff-45a4-9929-9d83ccc92de5")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "TCHACK! These crude arrows CAN work!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitWithImprovisedArrowRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.7f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("b00477fd-5238-47da-813e-241db5b89803")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("758ea547-d1ec-4b18-ace7-1b34b279ba17")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hmm. One more arrow then..."
						}
					}
				},
				new ActionSetType("78d21bf9-a55b-43a3-a5e2-67bbc8b585e1")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b9b81124-16ae-436a-b2d8-fdfb8388784d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "OK...that arrow was not enough."
						}
					}
				},
				new ActionSetType("672937d9-0b7a-49a9-b0bb-a5d9e99dd3cf")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("2a01ade1-f41c-4111-919d-3d4573562226")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "The bow can hit well enough."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitEnemyWithRifleRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.3f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("9110f21b-200c-46cf-9ccc-b7827395e6ba")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c412fc50-012e-44e1-aacf-2641a6c4d0cf")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "It just shrugged that bullet off!!?"
						}
					}
				},
				new ActionSetType("c3862eb2-333c-awda335ffa-a23e-c66067177391")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c6dasdwadwf33b3-6180-4dd4-a0c0-08dd9da7a558")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "They can take a few shots!?"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledEnemyWithRifleRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.4f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("d4c48c62-99ca-4a06-ae19-66ed785e1b1d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d9c33222-efff-475c-8aae-173fba85b031")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Bullseye."
						}
					}
				},
				new ActionSetType("1b248d79-d992-4a20-aa0a-3dda10b0eb6d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("84e1e061-90c5-4cbc-be41-7bde24f73da3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Got it."
						}
					}
				},
				new ActionSetType("fece30e7-edeb-4cd4-97c6-9e71edb5a47a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("7969821f-d83a-4823-bbdc-ca4e4fddf046")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "BANG!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanMissedEnemyWithRifleRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.3f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("1e83826d-1a72-4f13-ad9c-e027230161e4")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("8420f7de-2495-418f-b585-8e2f08077f38")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "What! Who's been messing with the rifle?"
						}
					}
				},
				new ActionSetType("968d7738-1852-4ba9-9698-616730cb3911")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("2655fb3f-8ebf-4207-9949-837f804efe23")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Miss...!"
						}
					}
				},
				new ActionSetType("dd9186d4-af57-4c5e-91bd-05f68a7973b9")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("abb391dd-66e8-4fb1-aba5-216ed178e1e3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Sorry!...Wasting ammo!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitPreyWithRifleRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.7f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("0f4e1b30-3a79-4c1b-940e-2d2541412904")
				{
					ChanceToFire = 0.5f,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("18d27833-d9b7-45b5-8927-71446fe11502")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "OK, one shot's not enough to bring you down..."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanKilledPreyWithRifleRemark",
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("c65f835b-0dba-412f-b11b-755bc956a285")
				{
					ChanceToFire = 1f,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d0f7f671-6861-46d4-b08b-d8a982fff3f9")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Gotta love this rifle."
						}
					}
				},
				new ActionSetType("5b95c942-1295-47c7-bc48-abe142f45203")
				{
					ChanceToFire = 0.3f,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("a518dfce-4b45-408e-a37a-373297cef8fb")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Perfect hit."
						}
					}
				},
				new ActionSetType("e23df318-6460-411c-bff9-8c666e1376d1")
				{
					ChanceToFire = 0.5f,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ccd997aa-443c-4b9b-9885-4f481666d6da")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Got you, buddy."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanMissedPreyWithRifleRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("3a40234e-e7c7-4e2a-94cc-e50776d791f5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d2058fbb-77f7-4573-9cbb-7ab4ed045d6c")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You cunning thing!"
						}
					}
				},
				new ActionSetType("9b860a71-b9f5-4632-86b1-c4b71e9ccbd6")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("2e8438d2-dcdb-4b09-a28d-beb96473a189")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Rascal moved..."
						}
					}
				},
				new ActionSetType("54907dd5-85ae-4bd9-b227-46105fe71bfb")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("375a2c9b-57a6-4cbc-b1cf-01c0eb18d664")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hold still buddy."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanHitEnemyWithPunchRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.3f,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("f25441cc-edc9-457d-9f0d-5b39176de24c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c703fc4b-449f-4590-a2af-2b5f4d9e8c9a")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ugh...need a weapon here."
						}
					}
				},
				new ActionSetType("289b780e-492c-488b-80dd-b4d1dd931756")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("83dada1c-5e8c-4d07-acb0-dcc13ca55055")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "ANYONE HAS A SPEAR?"
						}
					}
				},
				new ActionSetType("70f7bc37-1439-4a51-8c67-65e978a0ffef")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("e065cf20-988a-42e4-8a3a-85ff6f541a6a")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Why am I barehanded?"
						}
					}
				},
				new ActionSetType("1d9516fb-fb4d-429f-8336-522b7851ce30")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5d13c12c-7996-430f-ac46-83e981242d1b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I need a weapon!"
						}
					}
				},
				new ActionSetType("b6908788-220c-40de-8236-106c9088ca04")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("277f24b4-0c4c-4cdc-a015-9372734e887e")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "A knife, a club, something!"
						}
					}
				},
				new ActionSetType("2bd76126-d2ab-4524-a899-2b9c8c05b18b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("fcfd3087-e8eb-4653-a97e-552fbcb0f297")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm fighting barefisted here!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startConstructionRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SupressWhenSpawning = true,
			SetsOfActions = new ActionSetType[8]
			{
				new ActionSetType("09201740-3bbf-4981-b198-626598a2fc65")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("6382411e-de36-4aea-bd98-c81d4b0e0ec6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Let's get this started..."
						}
					}
				},
				new ActionSetType("db21acf3-08f0-4250-9ae6-cbec524a5f21")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("cc8eee6d-1455-42dc-bd36-df75f377dbed")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Time to get to work..."
						}
					}
				},
				new ActionSetType("afece8c6-88d6-400a-a201-c09e856b8032")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("6b2b75b0-5b41-442c-8c58-8060a2751b89")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll take this one..."
						}
					}
				},
				new ActionSetType("1279744b-8cad-4443-b180-69abce433379")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("7950e305-7b79-4d69-a9a2-1ef15b4fb629")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I can handle this."
						}
					}
				},
				new ActionSetType("d9088548-8fc4-442e-a35c-a531031b75bb")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("945fec13-dde4-41c2-9e24-02f6b937cdf4")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Duty calls."
						}
					}
				},
				new ActionSetType("52765fd0-8bb1-4a16-8940-603a4aeb8ee4")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("021d4302-243e-4189-b9b6-3d672250b20e")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "If noone else will, then I'll do it."
						}
					}
				},
				new ActionSetType("a45c3db6-75bc-454c-b41c-078f377ad2a6")
				{
					Actions = new EventActionType[1]
					{
						new TalkAction("7eb06890-ffde-41aa-83db-36213e7634c2")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Guess someone has to do it!"
						}
					}
				},
				new ActionSetType("6b66feaa-2526-4733-a116-b4899720f2ed")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("8ca34922-4d47-4529-addc-d3e3d2d141bf")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll do this even though I don't agree with you guys' decision."
						},
						new TalkAction("a2183b3d-97e0-49f9-84fd-dab5eb80f886")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							TurnTowardsListeners = false,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "We appreciate your sacrifice."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startConstructShelter",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SupressWhenSpawning = true,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("e4aca994-e597-46ac-a85d-04c739f4e8b6")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("b7a72a56-2bcd-42e1-ba0a-7f00fdb0bef3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.PreferTriggeringEntity,
							DefaultText = "Let's get this up."
						},
						new TalkAction("8d978267-d89a-4905-b212-cd4422ea8358")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "I thought we agreed to move the camp somewhere else. What gives?"
						},
						new TalkAction("a9a8cc14-8a75-4ce6-811e-16e0fe4e68e0")
						{
							DelayInSeconds = 5.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "No, we ended up deciding to build it here."
						}
					}
				},
				new ActionSetType("c94c47a8-d9f2-4c0f-a098-b2a223216ef3")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("2344f045-e0a8-485b-9b55-2451d1e48665")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.PreferTriggeringEntity,
							DefaultText = "Getting a shelter here is the right choice."
						},
						new TalkAction("2b51baef-4cb3-4041-b1a5-43fd6d95eb6f")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "I think so too."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endConstructionRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SupressWhenSpawning = true,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("3de35ede-8c66-4bf2-849d-d2e137cd088d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("dcb42868-c1bf-436a-974a-667878eddf57")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Done."
						}
					}
				},
				new ActionSetType("93e72cb3-e77c-4bea-b43e-08271fa69758")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4747a892-3185-4adf-ae82-39fe53691201")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Alright, I did the best I could. Hope you like it."
						}
					}
				},
				new ActionSetType("4efabb85-8e7b-41cb-8c31-c7fd2497272d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("d4a9035e-bca8-4a46-9d9a-15845c1821ce")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Is this what you had in mind?"
						},
						new TalkAction("72ee02dd-ee50-48cd-a18d-82a70b6d086c")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Hmmm, yeah. It looks good!"
						}
					}
				},
				new ActionSetType("5d9b21c4-66be-4d0f-bbee-4fd03c0f8132")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("77af2d06-da18-4aaf-9eff-9e5702c6430f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Finished! What do you think?"
						},
						new TalkAction("69d044a2-c102-456e-9321-87a28dc80e92")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "It'll do."
						}
					}
				},
				new ActionSetType("87348636-aa86-45f0-b5a1-6ee28913bf48")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("0f6c9a79-bba3-4061-b039-c5d4944d1bc1")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "It's finished!"
						},
						new TalkAction("a0dc3b86-6114-4c54-ba5e-b509b2688558")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Good work - exactly what we agreed."
						}
					}
				},
				new ActionSetType("31aa054a-77c2-44f1-88d8-43fc83d544e5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("49d4dc7c-c610-4c72-9303-f77fdf3550ac")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Done. I'm kinda proud of this, actually."
						},
						new TalkAction("8967e9c3-f3c8-4105-8290-064650806bc0")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "You should be."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "produceCampfire",
			SupressWhenSpawning = true,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("489fc87d-a68f-44ec-8891-fb370d370301")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new TalkAction[2]
					{
						new TalkAction("2b3fb787-1278-4b87-b008-53e1fc04cd51")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I say we get a barbecue going."
						},
						new TalkAction("802e40d3-c2b5-4255-9fcd-27794da576c6")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							TurnTowardsListeners = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Sounds great."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "fishCheckTalk",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.15f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("c3862efddfb2-333c-4245ffsa-a23e-c66067177391")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c619b3-6180-4dd4-a0c0-0824324dda9da7a558")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Gonna go check if we caught any fish."
						}
					}
				},
				new ActionSetType("cfdsef6453g2-333c-419a-a23e-c66067177391")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c619b3-61ad24555saffwvvb4-a0c0-08dd9da7a558")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Let me see if we caught anything."
						}
					}
				},
				new ActionSetType("c3862eb2-333c-419a-a23e-cfefgs444s5sd67177391")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c644asfdfabnregt0-4dd4-a0c0-08dd9da7a558")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I\u00b4ll see if there's any luck with the fish trap."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "animalTrapCheckTalk",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.15f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("c3862eb2-333c-419a-a23e-c6xcf6067177391")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c619b3-6180-4dd4-a0c0-08dddfg9da7a558")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Let's see what we caught..."
						}
					}
				},
				new ActionSetType("c3862eb2-333c-419a-a23e-c6xcf60671773912")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c619b3-6180-4dd4-a0c0-08dddfg9da7a5582")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I wonder if the trap caught anything."
						}
					}
				},
				new ActionSetType("c3862eb2-333c-419a-a23e-c6xcf60671773913")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("c8c619b3-6180-4dd4-a0c0-08dddfg9da7a5583")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm gonna check the trap."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startHarvestRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[11]
			{
				new ActionSetType("5fc5bff9-8a9a-48fe-8411-fc4068360842")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5a36ecdsawdf1b-8a9e-434awddaaf9607-a4aefb47d283")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll get this in no time at all."
						}
					}
				},
				new ActionSetType("19d2d17b-8c7d-4c73-9c5c-a06e57649ac4")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9d4d703d-36c4-4cf6-851b-9b239e0cc4f0")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This is easy to do."
						}
					}
				},
				new ActionSetType("be126b30-ae98-4911-910d-5e0040816368")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("42a6383b-7434-48e4-ade6-d2408f0d5c80")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I think I got the stuff ready soon."
						}
					}
				},
				new ActionSetType("d9dda9f8-f820-4aeb-b434-80c570dd2138")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("f4bdfa60-d87a-444c-bed1-8b297404fe72")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll just get these things we needed..."
						}
					}
				},
				new ActionSetType("6df5fc6d-c84b-4ea2-ab68-a3db577e6584")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("23ecdbcb-123f-4b8a-bcb3-10b1a577545b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I got those things here..."
						}
					}
				},
				new ActionSetType("183f7f21-d317-40af-af94-af4f401211ce")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("aaed4972-0802-4a9e-b069-84cbd80a0276")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "If noone else will, then I'll get it."
						}
					}
				},
				new ActionSetType("198affba-921a-46b3-9a36-e844f7658d8b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3f13acf8-6300-4b52-ad7a-2d4ea11129b6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Someone has to get this stuff...might as well be me."
						}
					}
				},
				new ActionSetType("2bf6b746-791a-4df2-bb56-d0cc5b1da301")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e3429787-348c-4a38-8c57-5e5c0d798286")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Sure we need this?"
						},
						new TalkAction("12c8c4f3-c17c-4396-b5c6-aeb0d80a9da3")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yup."
						}
					}
				},
				new ActionSetType("10856047-7238-495f-99ea-947d77ee8806")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("0952cf4c-7acc-4b8b-9fb4-29083573ea74")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Unfortunately, there's only me to do this."
						}
					}
				},
				new ActionSetType("89e1b5e7-b85c-4811-a251-01e2aed6f4f5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9eb8605b-df9c-4e13-bc5d-e1b2b826ddf4")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Now, why did I decide I needed this...?"
						}
					}
				},
				new ActionSetType("b95a468f-f11e-4554-8cb7-469eb9bda636")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("59c144b7-bca7-4b75-b272-cf6510b28805")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I really need these.."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "otherAgentRequestsCarriedItemRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("ced32f36-8c6c-4614-a668-0f8ef1a642c1")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4996d8cf-6821-4dd6-b410-47184d8dc55b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll just leave this for you here!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endHarvestRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("3f330fe0-f8e0-4499-97d1-563644110947")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("bd5e3f47-c251-4692-899c-4851c21b5bef")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I have the materials we wanted..."
						}
					}
				},
				new ActionSetType("d06f0e57-ccba-4cd8-a8e0-844e5090af9a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("98add316-97a2-4756-8299-50f980062653")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This stuff will prove useful."
						}
					}
				},
				new ActionSetType("968a839b-e883-46f1-825e-5ffb9eb44b99")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new TalkAction("8e1f6863-90bd-426f-8c26-eee1d77f6be5")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Woah, don't go over here. I just stepped on something, I don't know what..."
						}
					}
				},
				new ActionSetType("99efe92c-36b4-4e60-83d5-8e67a6c5d622")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("28975e20-5fc8-4846-b887-9c5373a33436")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Not sure why we needed this, but I got it ready now."
						},
						new TalkAction("3561b164-63e8-4fad-a4b9-96daac80b0fb")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Great!"
						}
					}
				},
				new ActionSetType("ddc392d0-c8f2-439c-872d-7632a90a3267")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("73144665-2df5-4c9a-99ff-b2da7c966193")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Alright, where do you want this stuff?"
						},
						new TalkAction("1b76624d-41d6-496b-a12c-48e99927e709")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Just over there, thanks."
						}
					}
				},
				new ActionSetType("28b54068-3719-4de6-8671-b25492b4b6e8")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("7981322a-954f-41c3-8ef4-58b0f32483d0")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'll put this the usual place."
						},
						new TalkAction("49aa3c67-d770-47e6-88dd-6fb3418a263e")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yeah, do that."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startGatherClayRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[7]
			{
				new ActionSetType("5fc5bfsfhfghsf-fc4068360842")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5a36ecfawfgeagbbnmtfd34b-9607-a4aefb47d283")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I know all about digging..."
						}
					}
				},
				new ActionSetType("6df5fchxjxhfjxh7e6584")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("23ecdbcb-12xfhjxfhjxfh0b1a577545b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Time to get digging."
						}
					}
				},
				new ActionSetType("183f7f21-srtjhfsgjsfzaf4f401211ce")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("aaed4972-0802-zfgzdfgzd<fgb069-84cbd80a0276")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This clay is tough stuff."
						}
					}
				},
				new ActionSetType("19zdghdghd-46b3-9a36-e844f7658d8b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3f13adzghfgfad7a-2d4ea11129b6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ouch. I hit a rock."
						}
					}
				},
				new ActionSetType("2bfzfghzfghjzfjf2-bb56-d0cc5b1da301")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e3xghjxhxhgj4a38-8c57-5e5c0d798286")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ugh...my back."
						},
						new TalkAction("12c8c4dawdt33453dnnb-cvhjjg80a9da3")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "You ok?"
						}
					}
				},
				new ActionSetType("2bfzfrwgfhjghcggccna301")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e3fghjghfkjfkhjc0d798286")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This clay here looks different."
						},
						new TalkAction("12c8SRTHSRTHTYghjhgj0a9da3")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Try not to take that."
						}
					}
				},
				new ActionSetType("2bfzWRY45555YTRYrc5b1da301")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e3xghrwyTYUWRUWRYWYU0d798286")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Is this clay good enough? It seems coarse."
						},
						new TalkAction("12c8c4f3-cvdwadasdvfbfnbtyt5530a9da3")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yeah, it should be ok."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanEatingRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("ebd6871d-a485-4a41-8dae-e9fd5c90e8a4")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e0771d0d-05f6-4a10-8dc3-86805870e19d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.PreferTriggeringEntity,
							DefaultText = "This is really good."
						},
						new TalkAction("9a4a8f7e-a89a-416e-91f9-1899393be43e")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Hey, leave some for the rest of us."
						}
					}
				},
				new ActionSetType("06a054e9-2857-4808-b5ee-30d641fd3492")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("39c6be38-8b6f-4b88-9cf3-5f90c5cb05dc")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Mmm-mmm. Yummy."
						}
					}
				},
				new ActionSetType("25773529-94b1-483f-8106-444ef01da8fe")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("adb3fc14-980d-48dd-a417-beca83673b24")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Oh wow...this is exquisite."
						}
					}
				},
				new ActionSetType("f8729405-d247-4e0d-b249-83c5466b949a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("02676309-18d5-40a5-b234-4561bdcc5c26")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You guys need to try this."
						},
						new TalkAction("126bc84e-0d11-4441-828d-342c13b732f0")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "You like it?"
						}
					}
				},
				new ActionSetType("8f5dccbd-bb45-4a1a-906b-755a31cbb597")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("07463b19-ed3c-4dc6-86c5-f9e991fdaf59")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Dinner is served."
						},
						new TalkAction("e5d6e4eb-4d6c-4140-af9b-3917f09fe97f")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Dig in!"
						}
					}
				},
				new ActionSetType("6819ff4d-594e-4970-9963-3b6bfe6b58a2")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("2deff12c-124a-4b6a-8850-73e734cb519a")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Eating alone is not so bad."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanGoingToSleepRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.3f,
			SetsOfActions = new ActionSetType[5]
			{
				new ActionSetType("d8e16630-75c3-4e55-a0d8-4dcf35ffa69c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("989a6231-b28e-40aa-a26b-7bcf51a768ad")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = true,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm turning in. Don't do anything stupid!"
						}
					}
				},
				new ActionSetType("87995e59-96c3-409e-9760-d77894e00a27")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("df26a9cc-c9e6-4a13-88de-ad76c4a99869")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = true,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Is it my turn to sleep? Goodnight people."
						}
					}
				},
				new ActionSetType("86a2b9d7-6334-4c9a-a572-bcae5df4e842")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("3f3f414f-caef-4c11-8983-27b143ae14d3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Goodnight!"
						},
						new TalkAction("5d5a18f8-2274-4f75-946d-8b116781d59d")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "You're going to sleep? Goodnight."
						}
					}
				},
				new ActionSetType("3841dc3d-63d5-4aad-9290-643821cc722b")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("dd7af07d-5ced-4f9f-a191-f5ba66d30b7f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = true,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Can you guys do without me? I'm really tired."
						},
						new TalkAction("88ecaf47-0322-40aa-b70d-40faaff8483d")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Sure, go ahead. We'll wake you up soon."
						}
					}
				},
				new ActionSetType("8bdb4dd9-2755-49ef-9a80-695053af836d")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("db95dfe3-0e82-4a57-8753-4c312b30ab95")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = true,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "...Guess this spot is as good as any..."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "snareFinished",
			SetsOfActions = new ActionSetType[4]
			{
				new ActionSetType("3aca35523f98-2fds38a-4cd3-8cd0-4e0cbe3e34t9a1b")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("dwadwf345253a9-d4b32b-4ff4-8187-57736sdf31d9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "isAnimalTrap",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("4cfwdad5253thsd-d4b32b-4ff4-8187-57736sdf31d9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "active",
							Value = new ValueNode
							{
								Bool = true
							}
						}
					}
				},
				new ActionSetType("32sadwfa4645jklh8-2fds38a-4cd3-8cd0-4ebe3e34t9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4sadafdbvnmhkjli3a9-d4b32b-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "jobName",
							Value = new ValueNode
							{
								String = "activateSnare"
							}
						}
					}
				},
				new ActionSetType("325cd398-2asda35fa38a-4cd3-8cd0-4ebe3e34t9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4cf892wardfsghjkuyjtgrfeoppb-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "triggerName",
							Value = new ValueNode
							{
								String = "smallImprovisedTrapTrigger"
							}
						}
					}
				},
				new ActionSetType("s32fwfasf398-2fds38a-4cd3-8cd0-4ebe3e34t9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4cf8asdfbfnytrdhgaaafde4b32b-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "attackType",
							Value = new ValueNode
							{
								String = "trapSmallBluntAttack"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "noAutoCheckTrapFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd398-2awfasfewfgbv38a-4cd3-8cd0-4ebe3e34t9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4cffbdvbgnyjya9-d4b32b-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "triggerName",
							Value = new ValueNode
							{
								String = "smallImprovisedTrapTrigger"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "customSpikeTrapProperties",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325a35626gdvs-2fds38a-4cd3-8cd0-4ebe3e34t9a1b")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("4sadawrfd467hhyoip9-d4b32b-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "attackType",
							Value = new ValueNode
							{
								String = "trapMediumPiercingAttack"
							}
						},
						new SetPropertyAction("4casdawrf45673ippfaa9-d4b32b-4ff4-8187-57736sdf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "triggerName",
							Value = new ValueNode
							{
								String = "spikeTrapTrigger"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "changeBaitToNoBait",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd3xsd98-2fd1535efdafafy-4cd3-8cd0-4ebe3e34txgfdb9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4sdaa53gadbn9-d4b32b-4ff4-8187-57736sdxgf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "jobName",
							Value = new ValueNode
							{
								String = "activateSnare"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "changeBaitToBlackpulp",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd3xsd98-2fds38a-4cd3-8cd0-4ebasfagdjkuipppuyfdfdb9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4cfsadsafeag462535a9-d4b32b-4ff4-8187-57736sdxgf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "jobName",
							Value = new ValueNode
							{
								String = "activateTrapWithBlackpulp"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "changeBaitToGlassyCreeper",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("32rqrxsd98-2fds38a-qrwrqt3-8cd0-4ebe3e34txgfdb9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4csadadhjukiloppa9-d4b32b-4ff4-8187-57736sdxgf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "jobName",
							Value = new ValueNode
							{
								String = "activateTrapWithGlassyCreeper"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "changeBaitToRatMeat",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd3xsd98-2fds38a-4cd3qwrdg-e3e34txgfdb9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("4cf8zsf923a9-dafag46-sd187-57736sdxgf31d9ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "jobName",
							Value = new ValueNode
							{
								String = "activateTrapWithRatMeat"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "activateSnare",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd398-2fds38a-4cd3-8cd0-fadfg46hope3e34t9a1b")
				{
					Actions = new EventActionType[5]
					{
						new SetPropertyAction("4cf8923a9-d4b32b-4ff4-8187-57736sdfsdfzz31d9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "addTrigger",
							Value = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "triggerName"
							}
						},
						new SetPropertyAction("4cf89wad-aeagef-4ff4-8187-57736sdf31d9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "active",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("8baf32f349f-1599-49e4-990b-8f39057a1a54")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Inactive"
							}
						},
						new SetPropertyAction("4cf8923432a9-d4b32b-4ffb4-8187-57736sd9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "animalTrapLastTimeChecked",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						},
						new SetPropertyAction("c8525t4rtippa3-6180-4dd4-a0c0-08dddfd7dsa-ffdaef456a558")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.Root
							},
							PropertyKey = "animalCheckCanTalk",
							Value = new ValueNode
							{
								Bool = true
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "springSnareTriggered",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("b8aed70d-a34d-4863-a12e-0aae5f480ffd")
				{
					Actions = new EventActionType[4]
					{
						new AttackEntityAction("cf8923a9-d4bb-4ff4-8187-57736d9590ba")
						{
							AttackTypeKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "attackType"
							},
							Attacker = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							TargetToAttack = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							OwnerOfCarcass = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							OwnershipType = Ownership.OwnerOfTarget
						},
						new SetPropertyAction("sdfgf8923a9-d4bb-4ff4-8187-57736d9590432")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "removeTrigger",
							Value = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "triggerName"
							}
						},
						new SetPropertyAction("19004d23-ae6d-4a89-bacd-a9d70d18e675")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "spriteFlag",
							Value = new ValueNode
							{
								String = "Inactive"
							}
						},
						new SetPropertyAction("4cf8923a9-d4b32b-4ff4-8187-577jyfjfhkymnvergda31d9590ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "active",
							Value = new ValueNode
							{
								Bool = false
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			Comments = "the animal gets destroyed to simulate that it leaves the map",
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "animalLeavesMapEdge",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd36758r67ytuityuitytyua1b")
				{
					Actions = new EventActionType[1]
					{
						new DestroyEntityAction("cf8restyr56gfhjghfjtyty7767y90ba")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "killContainingEntities",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("610ac46e-67bd-4fe7-9641-8cce8f909e1d")
				{
					Actions = new EventActionType[1]
					{
						new AttackEntityAction("6312bef8-a20d-46e3-9dc7-4f6f22c1f63d")
						{
							AttackTypeKey = new ValueNode
							{
								String = "killAnimal"
							},
							Attacker = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							TargetToAttack = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity,
								GetList = new GetList
								{
									HasPropertiesListKey = "contained"
								}
							},
							OwnershipType = Ownership.Expedition,
							OwnerOfCarcass = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "spawnRat",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("325cd3xsd98-2ashjkiphbvvcffa-4cd3-8cd0-4ebe3e34txgfdb9a1b")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("d9csa252523asaga6-4296-48ed-a014-0ec29b836976")
						{
							AddToContainer = new ContainerLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								}
							},
							Amount = new ValueNode
							{
								Int = 1
							},
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceSouthWest"
								},
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "landMineTriggered",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("3558160a-8151-484b-b77e-5439c0346f89")
				{
					Actions = new EventActionType[2]
					{
						new AttackEntityAction("b2dc5880-f690-441d-98f7-6e13139f3fbe")
						{
							AttackTypeKey = new ValueNode
							{
								String = "smallExplosionAttack"
							},
							Attacker = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							TargetToAttack = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							OwnershipType = Ownership.OwnerOfTarget,
							OwnerOfCarcass = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							}
						},
						new DestroyEntityAction("530de34f-b8c8-4637-a927-2dd6e3dcab67")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "fishTrapFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("01cdff40-f50b-40a6-8bbf-3108132f0ff0")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("9fdef37f-571e-405e-8f73-082aec72eaaf")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "fishTrapLastTimeChecked",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						},
						new SetPropertyAction("c012eb88-8eb9-43sdafgatg352ghpif120a8c871ac")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity,
								GetList = new GetList
								{
									HasPropertiesListKey = "anchor"
								}
							},
							PropertyKey = "spotted",
							Value = new ValueNode
							{
								Bool = false
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "checkFishTrap",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("2a100fce-30f3-4a20-a359-ae2fa4ea0617")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("2e2b983f-6e8a-4fe3-9425-0b88cd0fd3ce")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fishTrapLastTimeChecked",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "smallPlotFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("9432df93-db01-4b00-9758-fc4a0e093af4")
				{
					Actions = new EventActionType[8]
					{
						new SpawnEntityAction("b84126af-480c-4074-a629-06af5434185b")
						{
							EntityData = new EntityData
							{
								EntityKey = "structure:smallPlot",
								Location = new Vector3(0f, 0f, 0f)
							},
							DynamicLocation = new DynamicLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "location"
							},
							OwnedBy = new AllegianceAndExpedition
							{
								DynamicAllegianceKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "allegiance"
								},
								DynamicExpeditionKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "expedition"
								}
							}
						},
						new SetPropertyAction("b1safyhy565ipopd-895a-46d6-b13b-safdbvccxcnyiygfsa3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "isFarmPlot",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("b653565d-895a-46d6-dggb13b-dbfdd62a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "sizeFactor",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("b1asad36225d-895a-46d6-b13b-dbaf64fdd2axzdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "speedFactor",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("b1a2225d-895a-46d6-b13b-dbfddcvcgf2axzdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "weedSpeedFactor",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("b1325452fdsgggxvf2225d-895a-46d6-b13b-d235dsfa3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "weedJobProcessKey",
							Value = new ValueNode
							{
								String = "weedPlot"
							}
						},
						new SetPropertyAction("b1acxvf2225d-895a-46d6-b13b-db35626fdaa3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "harvestJobProcessKey",
							Value = new ValueNode
							{
								String = "harvestCrops"
							}
						},
						new SetPropertyAction("b1a23sgh572225d-895a-46d6-b13b-dbfdd2a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "organicFertilizePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "largePlotFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("54d8c726-4d78-4d21-a237-0bfcc686xsd88d5")
				{
					Actions = new EventActionType[8]
					{
						new SpawnEntityAction("43242e0f-6455-4bab-a604-1316xcvzfxc0144b4c")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "structure:largePlot",
								Location = new Vector3(0f, 0f, 0f)
							},
							DynamicLocation = new DynamicLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "location"
							},
							OwnedBy = new AllegianceAndExpedition
							{
								DynamicAllegianceKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "allegiance"
								},
								DynamicExpeditionKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "expedition"
								}
							}
						},
						new SetPropertyAction("asf463292c8-c62f-4aaf-86d9-c6faft646014d449")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "isFarmPlot",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("b1asfd462-895a-46safa53d6-b13b-dbfdd2a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "sizeFactor",
							Value = new ValueNode
							{
								Decimal = 2f
							}
						},
						new SetPropertyAction("b1sad56625d-8asf-h5354253-sd35522axzdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "speedFactor",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("b63gvppug25d-895a-46d6-b13b-dbfdd2axzdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "weedSpeedFactor",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("b1aas3525d-89553fggggipoiogip6-b13b-dbfdd2a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "weedJobProcessKey",
							Value = new ValueNode
							{
								String = "weedLargePlot"
							}
						},
						new SetPropertyAction("b1acxvasf56625d-895a-46d6-b13b-dbfsa3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "harvestJobProcessKey",
							Value = new ValueNode
							{
								String = "harvestLargeCrops"
							}
						},
						new SetPropertyAction("b1ac421xvf2225d-895a-4dsf67796-b24213b-dbfdd2a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "organicFertilizeLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "greenhouseFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("54d8c726-4d78-4d21-a237-0bfcc68688d5")
				{
					Actions = new EventActionType[12]
					{
						new SetPropertyAction("dc6f92c8-c62f-4sadaaf-86d9-c6fasf356222sad449")
						{
							Comments = "used for polling",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "isFarmPlot",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("b1a2225d-895a-46d6s-af2q3513b-dasf462ghppogfd2a3c7af")
						{
							Comments = "sizeFactor is used to multiply cropSpawnBaseline",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "sizeFactor",
							Value = new ValueNode
							{
								Decimal = 0.25f
							}
						},
						new SetPropertyAction("b1a2225d-895a-46d6-b13b-fhghgsadetyyjjkkggfcxvdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "speedFactor",
							Value = new ValueNode
							{
								Decimal = 2f
							}
						},
						new SetPropertyAction("badrettuopsdfassawd-895a-46d6-b13b-dbfdd2axzdr3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedSpeedFactor",
							Value = new ValueNode
							{
								Decimal = 0.72f
							}
						},
						new SetPropertyAction("b1acxvf2225d-895a-46fse6d6-b13b-dbf24522ffsd6622sdfdsf64c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedJobProcessKey",
							Value = new ValueNode
							{
								String = "weedGreenhouse"
							}
						},
						new SetPropertyAction("asd35362225d-895a-46d6-b13b-dbfdd2a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestJobProcessKey",
							Value = new ValueNode
							{
								String = "harvestGreenhouseCrops"
							}
						},
						new SetPropertyAction("b1acx562225d-895a-46d6-b13b-ddfhsh552a3c7af")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "organicFertilizePlot"
							}
						},
						new SetPropertyAction("39asf2693a-e464-4faf-9902-12gghjoped73fd4a0e")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "nutrientLevel",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						},
						new SetPropertyAction("asf32254a4-663d-4e3b-ba6c-2da53f47")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("3afa4a4-663d-4e3b-bfasfwa6c-25bf235waf34a8bf47")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("34basf26a4-663d-4e3b-ba6c-25dsggbf34a8bf47")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "deltaCropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("121535gip694-dc4e-4b0d-9604-9bd2sf7acc6c13")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropsAreGrowing",
							Value = new ValueNode
							{
								Bool = false
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "plotEstablished",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("113c69be-d2a0-4ec6-9acc-5278sgfiop9f03662")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "nutrientLevel",
							IsNull = true
						}
					},
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("3989hejb93a-e464-4fasadf-9902-12yuped73fd4a0e")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "nutrientLevel",
							Value = new ValueNode
							{
								Decimal = 1f
							}
						}
					}
				},
				new ActionSetType("5c5dbb4e-0c87-4745-b231-97f5c6c4a45c")
				{
					Actions = new EventActionType[6]
					{
						new SetPropertyAction("34af25a4a4-663d-4e3b-ba6c-25bfsaff34a8bf47")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "weedGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("asfd34bba4a4-66faf53d-4e3b-ba6c-25bf3a3534a8bf47")
						{
							Comments = "set the property cropGrowthProgress to 0.0f. Is reset to zero when crop is harvested or withers",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "cropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("34fsaf264a4-663d-4e3b-ba6c-25bf34asa55dgioo8bf47")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "deltaCropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("1af266opi94-dc4e-4b0d-9604-9bd27acc6c13")
						{
							Comments = "reduces the resoruces required in the loop -> this makes it easier! :D",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "cropsAreGrowing",
							Value = new ValueNode
							{
								Bool = false
							}
						},
						new SetPropertyAction("625532dff-0189-4d8f-88d8-4bfa356464ffacc")
						{
							Comments = "transcribe the property nutrientLevel from the terrain",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "nutrientLevel",
							Value = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "nutrientLevel"
							}
						},
						new DestroyEntityAction("9165fb24-cec2-4b65-a0bd-cdcca75c5c73")
						{
							Comments = "when the structure is in place there is no need for the terrain anymore - it will be respawned after the farm plot is destroyed!",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "smallPlotToBeDestroyed",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("afrgthjukipggnbv32565-9ae8-4dcc-a3bb-70f02490ea3f")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("3a8ff47d-4b5e-4206-9e8a-e677dae03384")
						{
							Comments = "gets spawned at the same location as the structure",
							DelayInSeconds = 0.0,
							EntityData = null,
							EntityType = new ValueNode
							{
								String = "terrain:smallPlotSpot"
							},
							DynamicLocation = new DynamicLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "location"
							}
						},
						new DetectAction("b129dbb6-b98c-4c08-a003-f082754c4603")
						{
							Comments = "detect the new terrain entity",
							DynamicDetectorAllegianceKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "owningAllegiance"
							},
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "largePlotToBeDestroyed",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("644ba2df-39b2-4b75-a81a-fd520ff34851")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("a634c83c-3d75-4e57-8e83-08c13c4bbd64")
						{
							DelayInSeconds = 0.0,
							EntityData = null,
							EntityType = new ValueNode
							{
								String = "terrain:largePlotSpot"
							},
							DynamicLocation = new DynamicLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "location"
							}
						},
						new DetectAction("30f52033-ea82-4bce-8b2c-1abb754e4b64")
						{
							Comments = "detect the new terrain entity",
							DynamicDetectorAllegianceKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "owningAllegiance"
							},
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "transcripePlot",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("e805254hjuop65-9ae8-4dcc-a3bb-70fdaffhxx02490ea3f")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("3a8ff47d-4b5e-4206-9e8a-e677dae03zzdr384")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.LastSpawnActionResult
							},
							PropertyKey = "nutrientLevel",
							Value = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "nutrientLevel"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "initializeGlassyCreeperPodsAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("f189309c-c28d-4b64-84e0-555184dff2e1")
				{
					Actions = new EventActionType[7]
					{
						new SetPropertyAction("8cc54f40-d2f6-46c8-a50a-6a5e2f5166ee")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropTypeToSpawn",
							Value = new ValueNode
							{
								String = "item:glassyCreeperPods"
							}
						},
						new SetPropertyAction("e72f6b80-76a7-4fa7-8870-226b0c7fdad1")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropSpawnBaseline",
							Value = new ValueNode
							{
								Decimal = 17f
							}
						},
						new SetPropertyAction("1043c3bd-9d50-4089-8e1a-10918b43c394")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthPeriod",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									Decimal = 2000f
								},
								Operator = ExpressionOperator.Divide,
								Right = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								}
							}
						},
						new SetPropertyAction("6ca2df80-6d01-4079-95ff-416d9f47f76b")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestDate",
							Value = new UnaryFunctionNode
							{
								Operator = UnaryExpressionOperator.DateFromRelativeSeconds,
								Operand = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "cropGrowthPeriod"
								}
							}
						},
						new SetPropertyAction("6607c5f8-83d0-45ae-a0c0-91673c0684be")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthSpeed",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								},
								Operator = ExpressionOperator.Multiply,
								Right = new ValueNode
								{
									Decimal = 0.01f
								}
							}
						},
						new SetPropertyAction("3d5e017f-3a51-4332-9e41-c711ebe6656b")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "resourceTimeToLive",
							Value = new ValueNode
							{
								Decimal = 800f
							}
						},
						new SetPropertyAction("8b800071-a0dd-4d7e-af33-8455e51997a3")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "spriteFlag",
							Value = new ValueNode
							{
								String = "Flavour1"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "initializeCrystalBerriesAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("afrytuiopv3d-f9fe-4b19-9f13af536b78a35")
				{
					Actions = new EventActionType[7]
					{
						new SetPropertyAction("44c58b7c-fd67-45cd-8e22-0252352ipp3c06")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropTypeToSpawn",
							Value = new ValueNode
							{
								String = "item:crystalBerries"
							}
						},
						new SetPropertyAction("47afs2526577-fghopbvvbc0-4f95-9b39-4dde47a7937e")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropSpawnBaseline",
							Value = new ValueNode
							{
								Decimal = 17f
							}
						},
						new SetPropertyAction("4326467iuoppxxccdeebee7-4018-99a7-5ea88291f858")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthPeriod",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									Decimal = 3200f
								},
								Operator = ExpressionOperator.Divide,
								Right = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								}
							}
						},
						new SetPropertyAction("ba74a5dc-f88c-4883-9da9-594e2910fb0d")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestDate",
							Value = new UnaryFunctionNode
							{
								Operator = UnaryExpressionOperator.DateFromRelativeSeconds,
								Operand = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "cropGrowthPeriod"
								}
							}
						},
						new SetPropertyAction("7c9f5ad6-61b4-4829-8da8-fcaf2364666899997")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthSpeed",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								},
								Operator = ExpressionOperator.Multiply,
								Right = new ValueNode
								{
									Decimal = 0.007f
								}
							}
						},
						new SetPropertyAction("26a20470-2dfb-48b1-b3be-235tdghry43d4")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "resourceTimeToLive",
							Value = new ValueNode
							{
								Decimal = 800f
							}
						},
						new SetPropertyAction("8dafgtyuiop33d-7c68-48a1-a718-46d5cafetyub9849f8")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "spriteFlag",
							Value = new ValueNode
							{
								String = "Flavour2"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "initializeCottonAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("31cbef20-2a40-4482-85f8-32572410d4f6")
				{
					Actions = new EventActionType[7]
					{
						new SetPropertyAction("0614010d-1f94-4485-b417-e215f8c5c2dc")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropTypeToSpawn",
							Value = new ValueNode
							{
								String = "item:cotton"
							}
						},
						new SetPropertyAction("e4b9a999-d1e6-4376-9697-ee764d8178a5")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropSpawnBaseline",
							Value = new ValueNode
							{
								Decimal = 10f
							}
						},
						new SetPropertyAction("8df7f575-5231-408e-9a34-3e5e0e119ab8")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthPeriod",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									Decimal = 3400f
								},
								Operator = ExpressionOperator.Divide,
								Right = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								}
							}
						},
						new SetPropertyAction("d19e30ff-fe51-4c85-b637-d8920175c6ed")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestDate",
							Value = new UnaryFunctionNode
							{
								Operator = UnaryExpressionOperator.DateFromRelativeSeconds,
								Operand = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "cropGrowthPeriod"
								}
							}
						},
						new SetPropertyAction("dc03f08d-fd7e-47c1-85a5-f2ca1ff6e9fa")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthSpeed",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								},
								Operator = ExpressionOperator.Multiply,
								Right = new ValueNode
								{
									Decimal = 0.006f
								}
							}
						},
						new SetPropertyAction("b7774e3f-26cc-4d1d-b1a1-e72ead7ad14a")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "resourceTimeToLive",
							Value = new ValueNode
							{
								Decimal = 1200f
							}
						},
						new SetPropertyAction("f529f511-5c43-466f-b737-2b361e91efd3")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "spriteFlag",
							Value = new ValueNode
							{
								String = "Flavour3"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "initializeFingerFruitsAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("d1afs3253d-f9fe-4b19-9f13-4dffhjhyyvxxa35")
				{
					Actions = new EventActionType[7]
					{
						new SetPropertyAction("44c58b7c-fd6afa32-2525eedgf-gnjuipb323c06")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropTypeToSpawn",
							Value = new ValueNode
							{
								String = "item:fingerFruit"
							}
						},
						new SetPropertyAction("asfetyippxxb9-1330-4f95-9b39-4dde47a7937e")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropSpawnBaseline",
							Value = new ValueNode
							{
								Decimal = 20f
							}
						},
						new SetPropertyAction("4dfaet648922hhiop2-bee7-4018-99a7-5ea88291f858")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthPeriod",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									Decimal = 1600f
								},
								Operator = ExpressionOperator.Divide,
								Right = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								}
							}
						},
						new SetPropertyAction("aac59ff6-bc04-48d9-b739-745b13c97586")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestDate",
							Value = new UnaryFunctionNode
							{
								Operator = UnaryExpressionOperator.DateFromRelativeSeconds,
								Operand = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "cropGrowthPeriod"
								}
							}
						},
						new SetPropertyAction("7c92565y6uiopppdxsvxcvhuy-44829-8da8-fc0818089997")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthSpeed",
							Value = new FunctionNode
							{
								Left = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "speedFactor"
								},
								Operator = ExpressionOperator.Multiply,
								Right = new ValueNode
								{
									Decimal = 0.01f
								}
							}
						},
						new SetPropertyAction("26a20470-2dfb-48b1-b3be-cafd32526ippp3d4")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "resourceTimeToLive",
							Value = new ValueNode
							{
								Decimal = 800f
							}
						},
						new SetPropertyAction("8af235657oip-7c68-48a1-a718-46dafb9849f8")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "spriteFlag",
							Value = new ValueNode
							{
								String = "Flavour1"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "plantSeedsAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6d0fe365-80fc-4ff1-9634-2bdc74b373eb")
				{
					Actions = new EventActionType[9]
					{
						new SetPropertyAction("a7d1b81e-fb4a-4153-8bff-0b5f15ddaae1")
						{
							Comments = "set the property cropGrowthProgress to 0.0f",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("a5b3b97c-32b2-41a7-8123-28de87659fc2")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthElapsedTime",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("244667c1-b8d8-4799-adab-2e6e1989e231")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropsAreGrowing",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SetPropertyAction("2agff3254yukip0098-2090-4cf9-b6a4-d3bafhrsyjkl28348cb6c")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropState",
							Value = new ValueNode
							{
								String = "Growing"
							}
						},
						new SetPropertyAction("932fas2577dghkipzxc17-ac30-8d3c8d6a2384")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedingLastTimeStamp",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						},
						new SetPropertyAction("9eafgdsryuiozxcvewds640-8d3c8d6a2384")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeLastTimeStamp",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						},
						new SetPropertyAction("bdc2f95e-989c-4e75-a9bd-6652ab6a3441")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Flavour1"
							}
						},
						new SetPropertyAction("61af1315-afd6-437e-b061-4a4c4b125f9f")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Flavour2"
							}
						},
						new SetPropertyAction("6acb184e-f2ce-46c0-807c-86176058f4a5")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Dead"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "weedPlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("608eafdg23535-q35f3523-asf3223-f08a4292a591")
				{
					Actions = new EventActionType[4]
					{
						new SetPropertyAction("fbbfas3255648-f195-44dghs764c-8774-4846esfe2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						},
						new SetPropertyAction("8baf349f-1599-49e4-990b-8f39057a1a54")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Overgrown"
							}
						},
						new SetPropertyAction("574f8dc8-8dd8-4133-bef9-63dd25194332")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Dead"
							}
						},
						new SetPropertyAction("9e36dhgjug-fgfgjukjhfdv-rd3c8d6a2384")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "weedingLastTimeStamp",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "organicFertilizePlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("608ebeaf463uipzeqdfgwe-efsew-1-f08a4292a591")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("fbbee53asfa3213558-f195-444c-8774-4846e6266e2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "nutrientLevel",
							Value = new FunctionNode
							{
								Left = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TargetEntity
										},
										PropertyKey = "nutrientLevel"
									},
									Operator = ExpressionOperator.Plus,
									Right = new ValueNode
									{
										Decimal = 0.4f
									}
								},
								Operator = ExpressionOperator.ClampTop,
								Right = new ValueNode
								{
									Decimal = 1f
								}
							}
						},
						new SetPropertyAction("9efcvcnuytrfbxaq-4917-ac30-8d3c8d6a2384")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeLastTimeStamp",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "guanoFertilizePlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("608254hghj-w24wefqup-we2xve-9571-f08a4292a591")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("fbbga47278-f195-444c-8774-4846ee2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "nutrientLevel",
							Value = new FunctionNode
							{
								Left = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TargetEntity
										},
										PropertyKey = "nutrientLevel"
									},
									Operator = ExpressionOperator.Plus,
									Right = new ValueNode
									{
										Decimal = 0.6f
									}
								},
								Operator = ExpressionOperator.ClampTop,
								Right = new ValueNode
								{
									Decimal = 1f
								}
							}
						},
						new SetPropertyAction("9e369b44-qrfg-sdqdbnjkio-30-8d3c8d6a2384")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeLastTimeStamp",
							Value = new ValueNode
							{
								PropertyKey = "getTime"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "useOrganicFertilizerAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("608egrt2352-dfs25-23tseg4w2w-sdgfw-292a591")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("fbbee538-f195-aagfg4366fhuop-8774-4846ee2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "organicFertilizePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "useLargeOrganicFertilizerAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("asf608eaf2462a571-f08ad-429fgea-2a591")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("fbbee538-f195-444c-agfa3256-846ee2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "organicFertilizeLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "useGuanoFertilizerAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("608ebebzgd6-4c2efa3523-35536t4yw5hf-pgeqaf325gdgeazdvcxc-591")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("fbb153qedzvcxvc-f195-444cfsaf8774-4846ee2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "guanoFertilizePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "useLargeGuanoFertilizerAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("60854325324dsfdsafefvcvcfae-wfeef-f08a4292a591")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("fbbee538-f195-423524wtdgfss-ccxciuoopp44c-8774-4846ee2b195b")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							Value = new ValueNode
							{
								String = "guanoFertilizeLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growCrystalBerriesInLargePlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("8922e751-d80b-4825-86d2-b7be8ca48614")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("f09c4eb7-a84a-4fa4-bbdd-9e61c2979977")
						{
							Comments = "set the property plantJobProcessKey to plantCrystalBerriesLargePlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantCrystalBerriesInLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growGlassyCreeperPodsInLargePlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("a3ccbed9-5f72-48d8-9c76-ca2facf64304")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("7c1f1a51-0514-4ffa-86f4-bdb20117275a")
						{
							Comments = "set the property plantJobProcessKey to plantGlassyCreeperPodsInLargePlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantGlassyCreeperPodsInLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growCottonInLargePlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("082d9052-5663-482a-8c72-11c9c959914d")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("18670afd-20a5-47e7-bb48-cf02c71b5bb0")
						{
							Comments = "set the property plantJobProcessKey to plantCottonInLargePlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantCottonInLargePlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growCrystalBerriesInSmallPlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("a0d4b087-a866-43f7-8a79-be4e7ced82b0")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("cff29841-579e-4bda-952a-9caa738c8f26")
						{
							Comments = "set the property plantJobProcessKey to plantCrystalBerriesSmallPlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantCrystalBerriesInSmallPlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growGlassyCreeperPodsInSmallPlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("a0f97805-044e-446d-991a-5da9a37851e2")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("3d0185e4-cd3d-44cf-aedc-337b16cf2d51")
						{
							Comments = "set the property plantJobProcessKey to plantGlassyCreeperPodsInSmallPlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantGlassyCreeperPodsInSmallPlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growCottonInSmallPlotAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("564bcf46-c212-41aa-b3ee-6a53902f18e6")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("ef875d26-a3a4-4511-af5c-25bf208b99a5")
						{
							Comments = "set the property plantJobProcessKey to plantCottonInSmallPlot. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantCottonInSmallPlot"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growCrystalBerriesInGreenhouseAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("ce659601-736d-49a2-87de-34eaa536e067")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("c54f04e8-bd0c-4b70-a3f1-cf9e222895de")
						{
							Comments = "set the property plantJobProcessKey to plantCrystalBerriesInGreenhouse. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantCrystalBerriesInGreenhouse"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growGlassyCreeperPodsInGreenhouseAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("69ef9145-ad8c-4460-9dc9-c02edeb63eb9")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("67eaad7b-0220-407d-bf67-a5720291df8e")
						{
							Comments = "set the property plantJobProcessKey to plantGlassyCreeperPodsInGreenhouse. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantGlassyCreeperPodsInGreenhouse"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "growFingerFruitInGreenhouseAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("7690fb63-9bc1-489e-9f13-1dd9386f0615")
				{
					Actions = new EventActionType[1]
					{
						new SetPropertyAction("2ef06f6f-6aec-400b-a5a4-010114621fc7")
						{
							Comments = "set the property plantJobProcessKey to plantFingerFruitInGreenhouse. in plantingLoop, read this property if crops are not growing. then start a planting job",
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							Value = new ValueNode
							{
								String = "plantFingerFruitInGreenhouse"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "stopGrowingAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("9e72e692-b1b5-4e95-b237-69042b98f930")
				{
					Actions = new EventActionType[4]
					{
						new CancelJobAction("5e0c78d5-31c5-4d8a-b10f-128b7f6108f7")
						{
							Comments = "Read the property plantJobProcessKey, it has the name of the planting process. Then cancel any active planting jobs for the plot owner",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							ProcessTypeKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "plantJobProcessKey"
							}
						},
						new SetPropertyAction("0407a480-eb68-4a25-a098-17c32ab4a759")
						{
							Comments = "set the property plantJobProcessKey to null, to stop all job generation",
							DelayInSeconds = 0.10000000149011612,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "plantJobProcessKey",
							SetValueToNull = true
						},
						new CancelJobAction("a87a7f6f-81e1-441c-9ab1-72fac8cd33fd")
						{
							Comments = "Read the property weedJobProcessKey, it has the name of the weeding process. Then cancel any active weeding jobs for the plot owner",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							ProcessTypeKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "weedJobProcessKey"
							}
						},
						new CancelJobAction("e70271a2-be0f-4ebe-8dc2-df1a04fdddfe")
						{
							Comments = "Read the property harvestJobProcessKey, it has the name of the harvest process. Then cancel any active harvest jobs for the plot owner",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							ProcessTypeKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "harvestJobProcessKey"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "stopUsingFertilizerAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("93f6f3ca-5e3c-43db-818e-e91bcdb8437e")
				{
					Actions = new EventActionType[2]
					{
						new CancelJobAction("6fba176d-83a6-4f7e-b6db-a60a5fca3071")
						{
							Comments = "Read the property fertilizeJob. Then cancel any active fertilize jobs for the plot owner",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							ProcessTypeKey = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "fertilizeJob"
							}
						},
						new SetPropertyAction("6688b7d2-74c4-4740-9c9b-7cedafebb94d")
						{
							Comments = "set the property fertilizeJob to null, to stop all fertilize job generation",
							DelayInSeconds = 0.10000000149011612,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "fertilizeJob",
							SetValueToNull = true
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "harvestCropsAction",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("f7349fb0-4f80-40e7-9e24-69273a161b07")
				{
					Actions = new EventActionType[7]
					{
						new SpawnEntityAction("074e5eb4-f99c-47b8-acdc-a5d37cb8ed71")
						{
							LogAsProduction = true,
							EntityType = new ValueNode
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TargetEntity
								},
								PropertyKey = "cropTypeToSpawn"
							},
							OwnedBy = new AllegianceAndExpedition
							{
								DynamicAllegianceKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "allegiance"
								},
								DynamicExpeditionKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TriggeringEntity
									},
									PropertyKey = "expedition"
								}
							},
							DynamicLocation = new DynamicLocation
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyKey = "location"
							},
							Amount = new FunctionNode
							{
								Left = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TargetEntity
										},
										PropertyKey = "cropSpawnBaseline"
									},
									Operator = ExpressionOperator.Multiply,
									Right = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TargetEntity
										},
										PropertyKey = "sizeFactor"
									}
								},
								Operator = ExpressionOperator.Multiply,
								Right = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.TargetEntity
									},
									PropertyKey = "cropGrowthProgress"
								}
							}
						},
						new SetPropertyAction("bc4fa88a-479e-4cf3-8eb5-1749c6578967")
						{
							DelayInSeconds = 0.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Ripe"
							}
						},
						new SetPropertyAction("2bc54e2d-ec69-49c0-9eb2-6e1260ec0f3a")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "clearFlag",
							Value = new ValueNode
							{
								String = "Dead"
							}
						},
						new SetPropertyAction("e5a83997-62e3-4d8a-b058-e1caf325tyhuiop57d0e")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "harvestDate",
							SetValueToNull = true
						},
						new SetPropertyAction("22aff2645f-bggdvd-vctrfghty6a4-d3b28348cb6c")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropState",
							SetValueToNull = true
						},
						new SetPropertyAction("1121f322-b743-47f1-bb22-c657873dd944")
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropTypeToSpawn",
							SetValueToNull = true
						},
						new SetPropertyAction("9f36ed36-6109-4778-937c-2c2a583344ed")
						{
							Comments = "set the property cropGrowthProgress to 0.0. This allows a new planting job",
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							PropertyKey = "cropGrowthProgress",
							Value = new ValueNode
							{
								Decimal = 0f
							}
						}
					}
				}
			}
		});
		AgentCondition agentCondition = new AgentCondition();
		agentCondition.AllowEmigrating = false;
		agentCondition.AllowFighting = false;
		agentCondition.AllowTravelling = false;
		agentCondition.AllowSleeping = false;
		agentCondition.AllowThreatened = false;
		agentCondition.AllowUnconscious = false;
		AgentCondition left = agentCondition;
		TargetObject targetObject = new TargetObject();
		targetObject.GetList = new GetList
		{
			HasPropertiesListKey = "allegiances",
			FilterCondition = new PropertyCondition
			{
				PropertyKey = "keyName",
				ConstantStringEqual = "playerAllegiance"
			},
			NextList = new GetList
			{
				HasPropertiesListKey = "randomPersons",
				FilterCondition = new FilterConditionFunction
				{
					Left = left,
					Operator = OperatorType.And,
					Right = new PropertyCondition
					{
						PropertyKey = "EntityID",
						StringNotEqual = new ValueNode
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "EntityID"
						}
					}
				},
				MaxResults = 1
			}
		};
		agentCondition = new AgentCondition();
		agentCondition.AllowEmigrating = true;
		agentCondition.AllowFighting = false;
		agentCondition.AllowTravelling = false;
		agentCondition.AllowSleeping = false;
		agentCondition.AllowThreatened = false;
		agentCondition.AllowUnconscious = false;
		AgentCondition agentCondition2 = agentCondition;
		targetObject = new TargetObject();
		targetObject.GetList = new GetList
		{
			HasPropertiesListKey = "allegiances",
			FilterCondition = new PropertyCondition
			{
				PropertyKey = "keyName",
				ConstantStringEqual = "playerAllegiance"
			},
			NextList = new GetList
			{
				HasPropertiesListKey = "randomPersons",
				FilterCondition = new FilterConditionFunction
				{
					Left = agentCondition2,
					Operator = OperatorType.And,
					Right = new PropertyCondition
					{
						PropertyKey = "EntityID",
						StringNotEqual = new ValueNode
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyKey = "EntityID"
						}
					}
				},
				MaxResults = 1
			}
		};
		TargetObject targetObject2 = targetObject;
		list.Add(new ActionSets
		{
			KeyName = "humanDecidedToLeaveDialog",
			Comments = "Presents an appropriate dialog based on the highest unhappiness. Simulates two speakers. The text is retrieved from properties.",
			FireMode = ActionSetsToFire.FirstValid,
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("23372865-2171-45c8-9506-54115d8eedce")
				{
					Comments = "security unhappiness, when at least one other member is active",
					Condition = new ConditionFunction
					{
						Left = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "highestUnhappiness",
								ConstantStringEqual = "security"
							}
						},
						Operator = OperatorType.And,
						Right = new PlayerAllegiancePersons
						{
							MinMembers = 2,
							AgentCondition = agentCondition2
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("cd8asf32576jhop-2354fds4662-bbee-51aa3481de65")
						{
							Heading = "A member is leaving",
							DisplayImage = "GroupMeeting",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "securityEmigrateEventDialogText"
								},
								SubstitutionValues = new SubstituteValue[4]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#NAME2",
										TargetObject = targetObject2
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				},
				new ActionSetType("53bdea50-9ddf-4e69-994f-4aa12576582a")
				{
					Comments = "security unhappiness, when no other members are awake or active",
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TriggeringEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "highestUnhappiness",
							ConstantStringEqual = "security"
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("cd8adg24fh-42afdfdg-fa4f576qjiikopp-51aa3481de65")
						{
							Heading = "A member is leaving",
							DisplayImage = "NightTime",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "securityEmigrateEventNoConversationDialogText"
								},
								SubstitutionValues = new SubstituteValue[3]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				},
				new ActionSetType("0244d480-b8a5-492c-95d9-47716deb01ec")
				{
					Comments = "comfort unhappiness",
					Condition = new ConditionFunction
					{
						Left = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "highestUnhappiness",
								ConstantStringEqual = "comfort"
							}
						},
						Operator = OperatorType.And,
						Right = new PlayerAllegiancePersons
						{
							MinMembers = 2,
							AgentCondition = agentCondition2
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("b05af3235t45yyuippp-fgsewqqxx-9d25-73aecfec188c")
						{
							Heading = "A member is leaving",
							DisplayImage = "GroupMeeting",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "comfortEmigrateEventDialogText"
								},
								SubstitutionValues = new SubstituteValue[4]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#NAME2",
										TargetObject = targetObject2
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				},
				new ActionSetType("a48ad170-4002-4f17-b8c1-2eb9d9815ed5")
				{
					Comments = "comfort unhappiness, when no other members are awake or active",
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TriggeringEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "highestUnhappiness",
							ConstantStringEqual = "comfort"
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("b05eaafe25-45uiopj-hfghvdqvd1-9d25-73aecfec188c")
						{
							Heading = "A member is leaving",
							DisplayImage = "GroupMeeting",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "comfortEmigrateEventNoConversationDialogText"
								},
								SubstitutionValues = new SubstituteValue[3]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				},
				new ActionSetType("1258ba41-7e3c-4dc6-818f-35fe18b7c18a")
				{
					Comments = "food unhappiness",
					Condition = new ConditionFunction
					{
						Left = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TriggeringEntity
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "highestUnhappiness",
								ConstantStringEqual = "food"
							}
						},
						Operator = OperatorType.And,
						Right = new PlayerAllegiancePersons
						{
							MinMembers = 2,
							AgentCondition = agentCondition2
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("cfc4ba84-94dd-40faafvds-3254yjkiko-ppp-zxc5b961bf")
						{
							Heading = "A member is leaving",
							DisplayImage = "GroupMeeting",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "foodEmigrateEventDialogText"
								},
								SubstitutionValues = new SubstituteValue[4]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#NAME2",
										TargetObject = targetObject2
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				},
				new ActionSetType("b3a27b71-b75c-4c7e-836b-06263dc8b05a")
				{
					Comments = "food unhappiness, when no other members are awake or active",
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TriggeringEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "highestUnhappiness",
							ConstantStringEqual = "food"
						}
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("cfcsa4ba84-94dd-40fa-acf2-7afsfea5-b96146lopbf")
						{
							Heading = "A member is leaving",
							DisplayImage = "GroupMeeting",
							DisplayText = new DynamicText
							{
								EvalText = new ValueNode
								{
									PropertyKey = "foodEmigrateEventNoConversationDialogText"
								},
								SubstitutionValues = new SubstituteValue[3]
								{
									new SubstituteValue
									{
										Placeholder = "#JOURNALDATE",
										PropertyName = "getDate",
										Formatting = FormattingOptions.BothDates
									},
									new SubstituteValue
									{
										Placeholder = "#NAME1",
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.TriggeringEntity
										}
									},
									new SubstituteValue
									{
										Placeholder = "#EMIGRATIONTARGET",
										Property = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.TriggeringEntity
											},
											PropertyKey = "emigrationTarget"
										}
									}
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			Comments = "Remarks spoken right after the dialog",
			KeyName = "humanDecidedToLeaveRemark",
			FireMode = ActionSetsToFire.RandomValid,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5567rth566udj64e5u7it3b5tsry9")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("cd8dbw67w6rtb5uw46de65")
						{
							DelayInSeconds = 1.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileEmigrating = false,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Take care."
						},
						new TalkAction("f656sr45t46yeysrtyfghhdfhystrystrysrtyTfgd61efc")
						{
							DelayInSeconds = 4.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileEmigrating = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Oh, I'll be fine. Don't worry about me."
						},
						new TalkAction("cd8drsw6hwrsrty6ysrtysshde65")
						{
							DelayInSeconds = 7.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileEmigrating = false,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Good luck."
						}
					}
				}
			}
		});
		return list;
	}
}
