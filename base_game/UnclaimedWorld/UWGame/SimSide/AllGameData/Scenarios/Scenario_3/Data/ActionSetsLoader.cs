using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class ActionSetsLoader
{
	public static List<ActionSets> Init()
	{
		List<ActionSets> list = new List<ActionSets>();
		list.Add(new ActionSets
		{
			KeyName = "humanHitEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.15f,
			SetsOfActions = new ActionSetType[11]
			{
				new ActionSetType("f5eefb76-e3ca-4c82-b51a-3a4bd1a9a984")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("06726590-a652-4315-9b58-5f8d3664be90")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Back off!"
						}
					}
				},
				new ActionSetType("bc8933c4-eecf-4b74-9930-a646515f56a6")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("2cdb8ff5-f726-4520-b974-19ff5006ebd0")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Get off this island!"
						}
					}
				},
				new ActionSetType("a8cf338a-7d50-4904-b1cd-a312515e063f")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("47c966e7-5919-4872-a9a3-ea21708b32fb")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Flap those wings and fly!"
						}
					}
				},
				new ActionSetType("d5bc7712-1bb8-4d88-813a-4906aa37ac9d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("f818f61b-82ce-427e-a9a5-dda9dbbda3d2")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "How'd you like that?"
						}
					}
				},
				new ActionSetType("0b7a5928-0854-445a-b265-69b40ca9a082")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("9ef7371e-a152-4772-9a2a-531a875079f6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You think you can mess with me?"
						}
					}
				},
				new ActionSetType("3e0e4797-ca67-419e-b426-1eba8516bcd3")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("836e0d92-c4d8-4969-a7e5-1c70fb756f33")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "What did you do to my friends, huh?"
						}
					}
				},
				new ActionSetType("62c48267-59f8-448a-9dab-2dff0a83abfc")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("26c44cba-0dba-4e4b-9db2-3179e127e08e")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This is your final chance!"
						}
					}
				},
				new ActionSetType("b2bfc39c-371e-4fef-94b1-3a9b3b0ef3b6")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ebcd0929-18db-45df-b011-b9119917b1cc")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "UGLY WINGED LUMP!!"
						}
					}
				},
				new ActionSetType("996bbcb1-8617-4f46-b7bd-ec6d58de1c79")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("6ceabbdd-d2a1-4387-8db2-46b5bb99c7d6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "AAH! Stop squirting that stuff at me!"
						}
					}
				},
				new ActionSetType("83061f86-0f46-4d2e-90a1-f3cc2aa2dd60")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ef8cdc05-fcb7-4f97-adff-9dd2e264f37b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You. You again."
						}
					}
				},
				new ActionSetType("3fb856be-fc06-4144-ad26-a461b1199278")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d3ea051b-be37-4051-85ba-139430475d8f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm gonna make you into furniture."
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
			SetsOfActions = new ActionSetType[8]
			{
				new ActionSetType("c44f53fc-c52b-4c59-a996-35326ead7b9c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("b8798555-a8b0-48fc-94f8-a0baa312fd13")
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
				new ActionSetType("d002a6af-2350-43ff-bdc8-c3fc514f80ff")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("b916997c-9c41-406a-be08-f9509db54595")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "..Hnnngrlll..."
						},
						new TalkAction("ddd1b7c3-35f1-4cd1-a6a0-a007609663de")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Hey! Where are you?! I'm losing you!! Come back!!"
						}
					}
				},
				new ActionSetType("73f62d26-ed16-4e42-91f9-7b1fd80281e8")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("46fcc1fe-ec96-4919-81d3-0408fd825dcd")
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
						new TalkAction("81c0736d-be0f-497c-8c57-219d4d2ebdf3")
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
				new ActionSetType("0409567b-471b-4991-8613-cfe759ccf586")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e64fb463-ee57-4378-aea8-e658eeee52c6")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "..HGHKRRRLL.."
						},
						new TalkAction("ecff45d2-dbb7-4c0e-94ba-4d819682b3e2")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "What's happening?! We're losing you! Hang in there! "
						}
					}
				},
				new ActionSetType("1264b32e-73ba-47bf-85ae-1264fbc07dfe")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("0eecbbe5-2e71-48be-80b9-099fb6091118")
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
				new ActionSetType("5ab472e0-f7c2-46ec-b344-d30137d15554")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("6a271afd-efb5-4e25-bcb2-09e717aef7bb")
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
				new ActionSetType("44ceacd8-1b96-42a5-8cd8-73641c9164b5")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("6e5d5ad5-8137-4cd9-bc32-10edc687578d")
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
				new ActionSetType("a12e0fc6-2070-46de-bbaa-affe6bfed753")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5495eb94-e72e-43e3-af47-e5e25fadb693")
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
			SetsOfActions = new ActionSetType[12]
			{
				new ActionSetType("eff4a79e-506b-4de7-a1ab-3363838398ca")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("3b3b58e5-d2ef-40fc-931d-14ab79e7cfe4")
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
				new ActionSetType("c658a306-a2b9-4fce-91ae-e4998972b30d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("6b84915e-d182-419c-9ca6-d91dc0aa41ca")
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
				new ActionSetType("87a1ad1c-86ed-4f1d-8fec-797f1a4b3b1a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("bb69034f-b513-449a-be87-fca26defecd2")
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
						new TalkAction("ea95db22-8efb-4873-9dd7-b59a13b84ff9")
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
				new ActionSetType("b2b82d38-010d-44ab-a5f2-15ffc062d6eb")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("642196e1-7640-4b30-91e9-e444f9363dc0")
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
						new TalkAction("7a1df5ab-2699-4d06-915d-de06f86b003c")
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
				new ActionSetType("61493b3c-5979-49e8-bb22-7d22ec8a1c0a")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("e27f854a-cb40-4799-b48f-7a81c06b14f6")
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
				new ActionSetType("7d039130-6657-40de-b650-c5ece5ad3cfd")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ea81a563-7483-4a6a-a0eb-4c47df0b8ea6")
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
				new ActionSetType("874c384b-5beb-484e-8e9f-10309c02da1c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("490c0114-95c0-4033-a5f8-52bdcdbf0132")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "D-do they never give up?"
						}
					}
				},
				new ActionSetType("87cd3d0e-4a65-468e-a5ed-2b07e8011b7c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 2,
					Actions = new EventActionType[1]
					{
						new TalkAction("b4b17a9d-22b7-4bff-af4e-93797345ff1c")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Fight or flight...what to choose...fight or fl..."
						}
					}
				},
				new ActionSetType("d7a6d3af-7067-484d-b4b0-7b77d53b0f27")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new TalkAction("bae63bac-2d8e-4a66-8489-c6da8c4cd396")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Epinephrine...my old friend...help me out..."
						}
					}
				},
				new ActionSetType("50802a46-1bc3-46a7-bc3d-17b90d78e02c")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 2,
					Actions = new EventActionType[1]
					{
						new TalkAction("79761f3a-73cd-4006-9665-bcc4738f27db")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Flight. F-flight is the right choice now."
						}
					}
				},
				new ActionSetType("97600472-90a7-450c-9e94-344bd76925fa")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("e84c40b7-541b-4368-948f-e4d076792cd9")
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
				},
				new ActionSetType("54a7b80b-63ad-4127-b709-9815a72b5233")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 2,
					Actions = new EventActionType[1]
					{
						new TalkAction("ccc36e07-cdd8-420e-bc8e-eacf63e719d6")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Not again...!"
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
				new ActionSetType("99eaef5d-b3aa-48fd-9750-0050c9b25795")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("a7d8d0d9-e37c-4639-9545-8c182436e445")
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
				new ActionSetType("364f1fb4-404c-47de-be15-bc7f64dd61f3")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("a65161ee-eef3-425b-8385-b3a1a8a91a0a")
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
				new ActionSetType("a79542f2-f986-465a-8175-b81d1cbf419d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("725167e1-15a0-42f4-9f82-6ad722dbdff4")
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
				new ActionSetType("ca937998-6b34-4b5e-9b2b-0d93ee811216")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("e691ca17-d32f-4492-ac2d-cbbed5a23c61")
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
				new ActionSetType("e5e6a89f-8608-459f-93e3-ce932d91ea59")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("cf647e82-2d62-4d60-bfde-0b9cda3b20fb")
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
				new ActionSetType("4b2c706d-8273-48e8-ac9e-3a2c8d9687e4")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("41eccec3-13e1-4c2c-bc29-eec6f7c99f49")
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
			KeyName = "endHarvestRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("767ecff5-75eb-451d-bd94-0ddb03c03a91")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("c44671e6-eff4-4f67-bc73-f957e97779a7")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This island has all we need."
						},
						new TalkAction("121dd250-0a88-4fdf-afe2-beff4cc0d398")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "For a while, maybe. But let's get home soon."
						}
					}
				},
				new ActionSetType("64474297-ad87-4896-8b00-7dc4f35a2cd3")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("9c66eec1-6efb-473d-a499-8ff444ad4f2b")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I grew up in a landscape like this..."
						},
						new TalkAction("43f2ea03-45c9-4070-af6b-6d057e67a1d2")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "These rocks, this grass. It's all familiar."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial1",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453cc6d-ef90-4e25-9904-a171be10982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28eeb-2b2a-45a4-a688-d7448d7d2f30")
						{
							TutorialPageKey = "tutorial1"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial2",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453ccfgshgfshgfsh71be10982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28eeb-sfghgfshfsghfsgh48d7d2f30")
						{
							TutorialPageKey = "tutorial2"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial2a",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("64dghjhdkdghjdjdhjdhjd982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01gjklgkjfdgryrtyrtwy5ygkklgjgk2f30")
						{
							TutorialPageKey = "tutorial2a"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial3",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453cc6d-ese56y6756ysa6ye0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01fs547wryzst545745y7a56y7ad2f30")
						{
							TutorialPageKey = "tutorial3"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial4",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645s547s767syrtsyrtsyyrtsytsr82b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28s5677s567rs6t7srrtsf30")
						{
							TutorialPageKey = "tutorial4"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial5",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453s65767s5y7s56ytrsys50982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28s6756s75syrtdytgfyfsys7d2f30")
						{
							TutorialPageKey = "tutorial5"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial6a",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453teuytutujtysdhjsrgfhgfshi0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01fsfghgfshgfshgsfghgfhsfghfsggsff30")
						{
							TutorialPageKey = "tutorial6a"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial7",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453cr678i789tiyfuidyuft7uf0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01tot78goiyguoifyuifyu2f30")
						{
							TutorialPageKey = "tutorial7"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial7a",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("64sfghhhhhhhhhghfgfhsgfjfff82b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("0dgggggggggggggggggggdgjjdg0")
						{
							TutorialPageKey = "tutorial7a"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial8",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453ct789o7tokivgyuyfiuky10982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28t87ot7gy8iogyoiygf2f30")
						{
							TutorialPageKey = "tutorial8"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial9",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453t789oyufkiuo567e8d57e8u60982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2t87ogf8fyhuki6r78i86r79d2f30")
						{
							TutorialPageKey = "tutorial9"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial10",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645t78oklufhkidyufu86s4e0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2fy7ikd6rfkg8h67rfi6rid2f30")
						{
							TutorialPageKey = "tutorial10"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial11",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453ct78ofyuiofyuifyi7ye10982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28srtyh5r46y56rsgfysrtd7d2f30")
						{
							TutorialPageKey = "tutorial11"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial12a",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645sr6yhudgfu56e756e764e74esrt982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28srt6yurx6xu765x7657658d7d2f30")
						{
							TutorialPageKey = "tutorial12a"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial12c",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645wrtyrtyrtyrtyrtyrtyrtyrtyrtyrtyrtyrtyrtyrtyrty2b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2sfgyhyhyhyhyhyhyhyhyhhsgfhgfshgfshs8d7d2f30")
						{
							TutorialPageKey = "tutorial12c"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial12b",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645wrtyrtyrt343434343434343434343434rtyrty2b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2sfgyhyhyhrerererererererey4566shs8d7d2f30")
						{
							TutorialPageKey = "tutorial12b"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial13",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453cs56r7u6ytsrytrsytrye10982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f286s57u6dtyjidytujj67587d2f30")
						{
							TutorialPageKey = "tutorial13"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial14",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6476r9i8d9tygujis56r7546582b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2d7i7d5euytdjudxyhujs4d2f30")
						{
							TutorialPageKey = "tutorial14"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial15",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453c6r7idtsy7ujxrs467s4w8u5764sw0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2s647utfurs565sd754swu2f30")
						{
							TutorialPageKey = "tutorial15"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial16",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6453d68us56syrturs574e76857982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f28rt8foi8r76f8iydt7us478d7d2f30")
						{
							TutorialPageKey = "tutorial16"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial17",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645d678u67i5t6878i68oikyufokldyfu0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2dtyujzae564sw6sa5y7s4u7s52f30")
						{
							TutorialPageKey = "tutorial17"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorial18",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645d7ik7688ikhfjukdtysux6su0982b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f27fikdtygucji6765476867r867tf30")
						{
							TutorialPageKey = "tutorial18"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "beforeCreviceRemark",
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5a6etyututyuyutyuyueyueyuuebfe33")
				{
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new EventActionDialog("532ffsghertyrtuyteuyeteuteyuegfs4f4e51")
						{
							DelayInSeconds = 0.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #2a NAVIGATE THE MAP \n \nSANTILLA: Hey...how do I navigate the map on this thing? \n \nSCOYD: Check the guide, it'll tell you. \n \n- // Navigate the map // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #2a",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial2a"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectCreviceRemark",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5111111e-1a7f-46d4-b52a-22f8d599d88b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[6]
					{
						new TalkAction("9333352-9f67-4ab2-a19e-4a2c93065708")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Oh no, look at this..."
						},
						new EventActionDialog("53yyyyyyya-ad62-4854-bes8e-f2134bhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #3 SALVAGE WRECK \n \nSCOYD: There's a narrow gorge blocking our way. I don't think there's any way around it, this island is covered in thorny brambles, no way we're getting through those. \nWe have to get across the gorge somehow. \n \nHARRON: A simple bridge... \n \nSCOYD: Yeah. With some wire rope. We can take it off the wreck. \n \nSANTILLA: Hey...sure that's a good idea? \n \nSCOYD: Look at it. See that hole? That boat's never gonna sail again. \n \nSANTILLA: Okay. I'm gonna see what the PPU says about making a bridge... \n \n- // Salvage the boat wreck for useful materials // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #3",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial3"
								}
							}
						},
						new SetPropertyAction("67wrtyyyyyyyyyyyyyyyfsgyhgfshxfgnbvnvbvnbbb")
						{
							DelayInSeconds = 0.0,
							PropertyKey = "gorgeDetected",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new DestroyEntityAction("88d113yyyyyyyyyyyyyyydfghgf0a4760")
						{
							DelayInSeconds = 1.0,
							EntityName = "Catamaran start"
						},
						new SpawnEntityAction("bdd3ettttttttttttttutuuuyutyueecca2cf14")
						{
							DelayInSeconds = 1.100000023841858,
							EntityData = new EntityData
							{
								EntityKey = "structure:boatWreck",
								OwnedBy = new AllegianceAndExpedition
								{
									AllegianceKey = "playerAllegiance",
									ExpeditionKey = "Camp"
								},
								Location = new Vector3(3202f, 3988f, 0f)
							}
						},
						new DestroyEntityAction("88d1dghjhjhjhjhjhjhjhjhjhjgdhj0a4760")
						{
							DelayInSeconds = 1.2000000476837158,
							TargetObject = new TargetObject
							{
								GetList = new GetList
								{
									HasPropertiesListKey = "entities",
									FilterCondition = new PropertyCondition
									{
										PropertyKey = "type",
										ConstantStringEqual = "structure:boatWreck"
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
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startSalvageBoatWreckRemark",
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5a62xxxx33346-5d71-4cef-8f33-ddf58xxx40bfe33")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("5dexxx9dad0-f062-4f2d-9825-039c8xxx0b04953")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Sorry about this, you were a fine catamaran..."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endSalvageBoatWreckRemark",
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5a62yyyy33346-5d71-4cef-8f33-ddf58yyyy40bfe33")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("5deyyy9dad0-f062-4f2d-9825-039c8yyy0b04953")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Done."
						},
						new EventActionDialog("532fgh7e22a-ad62-4854-bes8e-f2134bhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #4 BUILD ROPE BRIDGE \n \nHARRON: Alright. We took off some rigging, and some wire we can use. The mast is jammed though, can't get that up. \n \nSCOYD: Yeah, but with these lines, we should be able to make a rope bridge. \nLet's put that job in the PPU... \n \n- // Build rope bridge // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #4",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial4"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startBuildRopeBridgeRemark",
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5a626646-5d71-4cef-8f33-ddf5840bfe33")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("5de9dad0-f062-4f2d-9825-039c80b04953")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "This shouldn't be too difficult..."
						},
						new TalkAction("2184be01-d478-4c54-99ff-ea4cac0de299")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "It has to be sturdy."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "ropeBridgeFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("c4c53af1-5d85-4f0b-9cb6-345f09b10019")
				{
					Actions = new EventActionType[3]
					{
						new DestroyEntityAction("26c6cfbc-01e0-4538-ae9a-052104d508d4")
						{
							DelayInSeconds = 4.0,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							}
						},
						new SetPropertyAction("671fc078-522a-4468-b535-23922b6edbbb")
						{
							DelayInSeconds = 2.0,
							PropertyKey = "bridgeBuilt",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new SpawnEntityAction("bdd357bd-4435-47d9-b25b-353ccca2cf14")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "terrain:ropeBridge",
								Location = new Vector3(2994f, 3545f, 0f)
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "ropeBridgeFinishedRemark",
			FireMode = ActionSetsToFire.RandomValid,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("54764ea7-f695-4e7d-989b-616e7a91d4d8")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("3b19022f-1c55-47cd-a638-b083f0f2e7ba")
						{
							DelayInSeconds = 1.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Done. Who's first?"
						},
						new EventActionDialog("532fzzzzzzd62-4854-bes8e-f2134bhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #5 EXAMINE AREA TO FIND FOOD \n \nSCOYD: Alright, the bridge looks solid enough. Let's get to the other side - I see firegrass soil. There's often food to find. \n \nHARRON: I hope you're right. Let's examine that area. \n \n- // Examine firegrass to find food // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #5",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial5"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectcommonOilTubersRemark",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("rtyueyetetyu-1a7f-46d4-b52a-22f8d599d88b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("eyuyutyuty-9f67-4ab2-a19e-4a2c93065708")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Oh man, look at this. Oil tubers. My favorite."
						},
						new EventActionDialog("eyuteyuteyutyu-ad62-4854-bes8e-f2134bhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #6a GATHER 3 X COMMON OIL TUBERS\n \nSCOYD: Great, we all like oil tubers. Let's see if we can gather enough for everyone. \n \n- // Gather 3 X Common oil tubers // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #6a",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial6a"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "3commonOilTubers",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("51xxdfsgdfsghdfshadgfhdfasgdafgdafgdfad88b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new TalkAction("9xxsgfdhsdfggsdfdghfshgfghfsghsgshghghs08")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Got 3 tubers now."
						},
						new SetPropertyAction("671fcsyghfdgggggggggggggggggggfhhgfshssgfhedbbb")
						{
							DelayInSeconds = 62.0,
							PropertyKey = "commonOilTubersGatheredDelay",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new EventActionDialog("53sgfhhhhhhhhhhhhhhhhhhhghfghfghfhg51")
						{
							DelayInSeconds = 2.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #7 MOVE CAMP\n \nSCOYD: That should be enough for a meal. I see firewood in this area as well, so let's make this our new campsite! \n \nSANTILLA: What about having a look on the other side of that creek? \n \nHARRON: No, I'm with Scoyd - we have what we need, let's make camp right here. \n \nSANTILLA: OK then. \n \n - // Move camp to the firegrass patch // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #7",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial7"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "commonOilTubersGatheredDelayEvent",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("51xgfshssssssssssssssssssghffghgfhhgf9d88b")
				{
					Condition = new CustomCondition
					{
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "campfireFinished",
							BoolValue = false
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new EventActionDialog("53z536777777777777778756565656565656565656e4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #7a GATHER MATERIALS: 3 X FIREWOOD, 1 X STONE \n \nHARRON: Let's start gathering what we need for a campfire. \n \nSANTILLA: Will do. \n \n - // Gather 3 X Firewood, 1 X Stone // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #7a",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial7a"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "readyToBuildCampfire",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("51xxxxaesre-1a7f-46d4-b52a-22f8aerd599d88b")
				{
					Condition = new CustomCondition
					{
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "campfireFinished",
							BoolValue = false
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("9xxxxaersx52-9f67-4ab2-a19e-4a2c930aesr65708")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "We got some firewood and a few stones now."
						},
						new EventActionDialog("53zzzzaerzza-ad62-4854-bes8e-f2134aesrbhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #8: BUILD FIREPLACE \n \nHARRON: Alright we got all we need. \n \nSCOYD: Yep. Let's build the fireplace. \n \n- // Build a fireplace // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #8",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial8"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "constructCampfireFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("d4b0c097-f997-4111-94d3-28b4ba8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("0ae24413-11a8-4fc3-a3f9-ce2e0c016b95")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Campfire is ready!"
						},
						new SetPropertyAction("0d35e68j536856ej86r79jdt8htrteeeee58064")
						{
							DelayInSeconds = 1.0,
							PropertyKey = "campfireFinished",
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
			KeyName = "readyToCookcommonOilTubers",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("d4bdfgggggggggggggggjghgh4ba8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new EventActionDialog("tyudujdgeeeeeeeeetyuetyutyetuityuidtytyhdde4f4e51")
						{
							DelayInSeconds = 3.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #9: PRODUCE FOOD (MASHED OIL TUBERS) \n \nSCOYD: Let's get the pot boiling! \n \nSANTILLA: Can't wait. \n \n- // Make 3 X mashed oil tubers // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #9",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial9"
								}
							}
						},
						new TalkAction("cb5a7976-2255-4cb0-94c8-8cf7f3a66b0e")
						{
							DelayInSeconds = 6.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							TurnTowardsListeners = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "I'm so, so hungry."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "makeMashedCommonOilTubersFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbrrrrrrrrrrrrtrfhsgf4ba8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("011167867idtyujdgj6b95")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Got mashed tubers here, mmm...smells good."
						},
						new EventActionDialog("t999999999465i-ad62-4854-bes8e-f2134bhdde4f4e51")
						{
							DelayInSeconds = 24.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #10: SCOUT TO FIND ACCESS TO TREES \n \nHARRON:Get some food in your bellies and let's decide what to do next. I'm absolutely sure we're on an island. And I'm pretty sure it's one of the Brightburn islands. \n \nSCOYD: All uninhabited...but there are some trade routes and fishing going on here. \n \nHARRON: Yeah. Let's make sure that we get seen next time a boat passes by. \n \nSANTILLA: Smoke signals. A signal pyre. We need lots of firewood and fresh leaves. \n \nHARRON: That's our only option. Further up the hill there's trees and bushes we can use. Just have to find a way up. \n \nSANTILLA: Let's go! \n \n- // Use SCOUT to find access to trees // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #10",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial10"
								}
							}
						},
						new DestroyEntityAction("88d113cdfghdfghdgfhdfghdfghgf0a4760")
						{
							DelayInSeconds = 1.0,
							EntityName = "territoryLock1"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanEatingRemark",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbbbbbb-f997-4111-94d3-28b4ba8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("01111111-11a8-4fc3-a3f9-ce2e0c016b95")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Oh wow."
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
				new ActionSetType("5b56d1ee-1a7f-46d4-b52a-22f8d599d88b")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new TalkAction("99267952-9f67-4ab2-a19e-4a2c93065708")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Uh-oh, what's that moving?!"
						},
						new EventActionDialog("tdghj55555555554345434hghhfggf51")
						{
							DelayInSeconds = 5.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #11 CANCEL ALL ACTION ZONES \n \nHARRON: Woah! Bush dragons! Don't go near them! \n \nSCOYD: We should turn back. Guys, we need to meet back in camp, so drop what you're doing. \n \n- // CANCEL all action zones // -"
							},
							DisplayImage = "BushDragon",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #11",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial11"
								}
							}
						},
						new EventActionDialog("tdgertrewtyrwywrt434hghhfggf51")
						{
							DelayInSeconds = 30.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #12a: MAKE 3 SPEARS \n \nSCOYD:Alright we've seen at least one bush dragon. There's bound to be a herd of them. They are dangerous animals, if we're ever gonna get up there, we need weapons, this knife is not enough. \n \nSANTILLA: Why don't we just stay away from them? \n \nHARRON: Because there's nothing down here. What are you gonna eat - the few oil tubers we found? And how will you make signal fires without fresh leaves? \n \nSCOYD: We have to get up there and kill them while we still have the strength. \n \nSANTILLA: I see your point...Ok. We need weapons then. \n \nSCOYD: Spears. Shafts made from water cane, some kind of spearhead… \n \nSANTILLA: 3 Spears. I'll have a look in the PPU... \n \n- // Make 3 Flint-tipped spears // -"
							},
							DisplayImage = "BushDragon",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #12a",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial12a"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "makeFlintSpearheadFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbdf452222222222222222222222222226246sg26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("06478888888888888888888899897898db95")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Spearhead done."
						},
						new SetPropertyAction("67adgfd578999999999999999999999999999990057075hbbb")
						{
							DelayInSeconds = 0.0,
							PropertyKey = "spearReadyToBeMade",
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
			KeyName = "makeImprovisedFlintSpearFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbdfggggeeeeeeeeeeeeeeyudgfgfdhfsg26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("01dgggggggggggggggghjdghjjdddb95")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Spear ready."
						},
						new SetPropertyAction("67adgfdgfdgfdgfdgfdgfdgfdgfdgfdgfdgfdgfdgfdgfhdagghbbb")
						{
							DelayInSeconds = 0.0,
							PropertyKey = "flintSpearMade",
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
			KeyName = "readyToMakeSpears",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dberrrrrrrrrhtttttttttttttgfshgfhgfghf0")
				{
					MaxFirings = 1,
					Condition = new ConditionFunction
					{
						Left = new PlayerAllegiancePersons
						{
							MinMembers = 2
						},
						Operator = OperatorType.And,
						Right = new CustomCondition
						{
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "spearReadyToBeMade",
								BoolValue = false
							}
						}
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("0dgfgfgfgfgfgfgfgfgfgfhjgfdhhjghddj5")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I think we got enough gathered now."
						},
						new EventActionDialog("t99sfghhhhhhhhhhhhhhhhhhsghghfsshgf4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #12b: MAKE 3 SPEARS \n \nSCOYD: Ok, we gathered the stuff. Who can make spears? \n \nHARRON: You kidding? Everyone knows how to do that. \n \n- // Make 3 Flint spears // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #12b",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial12b"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "3SpearsFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbrdghjhdgkdghjhdhja8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[3]
					{
						new TalkAction("01tyutyudgyjdghfjkhjfffdjdb95")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "All 3 spears are done."
						},
						new DestroyEntityAction("88d4356347367563756378567563560a4760")
						{
							DelayInSeconds = 0.0,
							EntityName = "bushDragonSouth"
						},
						new EventActionDialog("t9999999fhjkhfjkfhjkhfjfkfjkfhjhdde4f4e51")
						{
							DelayInSeconds = 4.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #13: SCOUT FOR BUSH DRAGONS \n \nSCOYD: OK now we're ready to take them on. \nHARRON: I saw a way through the brambles. We should send a scout to find the bush dragons and then make an attack plan. \n \n- // SCOUT a way up to the bush dragons // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #13",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial13"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "readyToAttackBushDragons",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("d4bdfggggdhjhjhjhjhjhjhjhjhjhjhjhj8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[4]
					{
						new TalkAction("cb545656fghfghfgshfsghgfshgfshghfsghf66646446b0e")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							TurnTowardsListeners = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I can see three of them."
						},
						new EventActionDialog("tyu4566666666654666666666645645651")
						{
							DelayInSeconds = 3.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #14: KILL 3 BUSH DRAGONS USING ATTACK \n \nHARRON: They have a venomous spray, so cover your face. \n \nSCOYD: We have to rush them at the same time. Stay close together. It's important we don't get split up. \n \n- // Kill the 3 bush dragons // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #14",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial14"
								}
							}
						},
						new TalkAction("cb545656456455666666666666646446b0e")
						{
							DelayInSeconds = 5.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							TurnTowardsListeners = true,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Are we all set?"
						},
						new DestroyEntityAction("88d1eeeeeeeeeeeeeeeeeeuetyueyuetufghgf0a4760")
						{
							DelayInSeconds = 1.0,
							EntityName = "territoryLock2"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "afterBushDragonFight",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbrerererererererererereret6c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("t9999dfgggggggggggggggdfghgfdghdhfe4f4e51")
						{
							DelayInSeconds = 1.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #15: MOVE CAMP, BUILD SIGNAL PYRE \n \nHARRON: That was tough. \n \nSCOYD: Yeah. But we made it through. \n \nHARRON: OK, soon as you're rested, I think we should move camp up here, and start gathering materials and build that pyre... \n \nSANTILLA: Yep, lemme just have a look at my PPU... \n \n- // Move camp to the glade, build signal pyre // -"
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #15",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial15"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "constructSignalPyreFinished",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbtyudthjdgdghjjjjjjjhdethgjgh6c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new EventActionDialog("t9956758674846876865867846768467fe4f4e51")
						{
							DelayInSeconds = 1.0,
							DisplayText = new DynamicText
							{
								Text = "DECISION #16: LIGHT SIGNAL PYRE \n \nHARRON: No reason to wait around. Let's light this and keep it burning at all times. \n \nSCOYD: Yeah. there's plenty of branches here for making more fires."
							},
							DisplayImage = "GroupMeeting",
							DialogOptions = new DialogOption[1]
							{
								new DialogOption
								{
									Text = "GUIDE #16",
									Tooltip = "See how to carry out this decision (opens separate window.)",
									ActiveInArchive = true,
									ActionSet = "showTutorial16"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "startLightSignalPyreRemark",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("dbbfgjhdsghjsgrfhsgf4ba8d26c0")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("0111678srtgytratwy6yyyyujdgj6b95")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Time to let them know we're here!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "lightSignalPyreFinished",
			FireMode = ActionSetsToFire.AllValid,
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("ab9aertyueyeyetyertyyy7-a36d-391f7f4262a8")
				{
					Actions = new EventActionType[2]
					{
						new SetPropertyAction("0d31retyuet46yu6u5eb92ac8358064")
						{
							DelayInSeconds = 12.0,
							PropertyKey = "signalPyreLit",
							Value = new ValueNode
							{
								Bool = true
							}
						},
						new ParticleEffectAction("91edyhjhhhhhhhhhhhhhhhh-24a284ce1504")
						{
							UseLocationOfEntity = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							DurationInSeconds = 100.0,
							ParticleEmitters = new ParticleEmitterEffect[1]
							{
								new ParticleEmitterEffect
								{
									ParticleSystemKey = "whiteSignalSmoke"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "lightSignalPyreRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("96487f70-b8a9-4fd8-a21b-554ff219cd2a")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("19eb32dfghjdkjdyekuiuety9ff195")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "CAN YOU SEE US!?"
						},
						new TalkAction("d4687b4f-d3c3-4f897cvbhgfh63445645b")
						{
							DelayInSeconds = 4.0,
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "OR HEAR US!"
						}
					}
				},
				new ActionSetType("ff6fghhhhhhhhhhhhhhhhhhhhghjkkked2")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("4c245654444444444444444446e7-994c88175d85")
						{
							TalkPriority = TalkAction.TalkActionPriority.High,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I AM HERE! PLEASE SEE ME!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "placeCatamaranTerrain",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("3c8ettttttttttttttyuetdyudtyusruwru0db9")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("37edjhhdggggggggggggggggggdgjdgjdgjjf9a")
						{
							DelayInSeconds = 0.10000000149011612,
							EntityData = new EntityData
							{
								EntityKey = "terrain:catamaranWreck",
								Name = "Catamaran start",
								Location = new Vector3(3202f, 3988f, 0f)
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "placeTerritoryLock1",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("3c89f1dgjdhdetghjey340db9")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("37e4a4eghhhhhhhhhhhhffgfgffgc58ef9a")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "terrain:terrainBlockerWide",
								Name = "territoryLock1",
								Location = new Vector3(3168f, 2976f, 0f)
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "placeTerritoryLock2",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("3c8eeeeeeeeeeeeeeytyutrurtyuy340db9")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("37e4a4egghghghghghghdjhjdhdghjdgj58ef9a")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "terrain:terrainBlockerWide",
								Name = "territoryLock2",
								Location = new Vector3(3458f, 2534f, 0f)
							}
						}
					}
				}
			}
		});
		return list;
	}
}
