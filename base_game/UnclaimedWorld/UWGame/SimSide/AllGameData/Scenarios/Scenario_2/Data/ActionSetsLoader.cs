using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class ActionSetsLoader
{
	public static List<ActionSets> Init()
	{
		List<ActionSets> list = new List<ActionSets>();
		list.Add(new ActionSets
		{
			KeyName = "humanKilledEnemyRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.5f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("02faa79e-45cd-463a-a7ff-66e5212e46b1")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("d0420cfc-aa9f-4b02-bbd6-c2ee03ab5db4")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I had no alternative..."
						}
					}
				},
				new ActionSetType("2ffdbd1f-47e5-43e2-841e-e599102c755d")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("893bd632-951c-4748-a64a-f8823d6e4062")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I had to take that one down."
						}
					}
				},
				new ActionSetType("b1ac3bbf-7d86-441c-8688-a908129cac53")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("ae791812-2b68-4c60-8af8-0ccdf810fab8")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Sorry pal..."
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
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("ea70642b-efaf-45ad-844b-a6265b7e75a0")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("6ab5eab3-720d-4a78-9863-1d6a67185f24")
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
						new TalkAction("cf4fd263-cec6-4968-8957-195e40e5da9d")
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
				new ActionSetType("8e867d03-4224-4458-8687-528a078f83a8")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("97415593-ef14-4eca-a732-a443de69f31b")
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
						new TalkAction("cbc55706-edcb-49e1-9d40-6b8b74404c13")
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
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "humanFleeingRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.85f,
			SetsOfActions = new ActionSetType[3]
			{
				new ActionSetType("a1278136-79df-40b3-93b2-9ff60a03e965")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 2,
					Actions = new EventActionType[1]
					{
						new TalkAction("e8230ee8-68c0-4784-a829-7ca4cb26603e")
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
				new ActionSetType("b383cbe8-4e9b-43b3-b862-8236015c0569")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new TalkAction("5913e861-94d5-4cf9-9913-83935041337c")
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
				new ActionSetType("5ebe0661-8367-4e08-8127-7b255c84d9e3")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1,
						MaxMembers = 1
					},
					MaxFirings = 2,
					Actions = new EventActionType[1]
					{
						new TalkAction("7a913d8e-4fbc-4139-afac-444e29828caf")
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
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endHarvestRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 0.13f,
			SetsOfActions = new ActionSetType[4]
			{
				new ActionSetType("8caa431c-c360-4271-be5b-6837dd9470fe")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("8d253d4d-b139-4b67-a1c1-0653801ca52f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm so grateful for these gloves!"
						}
					}
				},
				new ActionSetType("8aea92da-c002-45f7-85f0-7596a980da83")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("8286ab70-0954-477e-b8ad-47a0fe51bdac")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Ouch! This little devil bit me!"
						},
						new TalkAction("17f8be7d-5a70-4d8d-bd32-462f8772a2ee")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm going to dissect you later...you just wait."
						}
					}
				},
				new ActionSetType("68072e57-3d07-486a-ac58-e6d9ddeb02cd")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("1a2f2f66-d461-4bd2-9712-91859d10cf7c")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Found a strangely behaving arthropod here...it seems to like me!"
						},
						new TalkAction("24792890-aef0-4305-b445-697bd5edb0e8")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Ha! Yeah that is indeed strange."
						}
					}
				},
				new ActionSetType("04501cb4-a300-4449-b6cf-425dc9922b52")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("4b994233-479b-4cdb-9291-d4ce4edc5505")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Whoa! There's a species here that is crying out to be documented..."
						},
						new TalkAction("e4149b17-2e5a-4f54-8162-7e9a53733358")
						{
							DelayInSeconds = 2.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "If it's not dangerous or edible it'll have to wait. We need to prioritize."
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
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("64838843-dd71-44bb-8f87-5134406ed247")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("339f1a3d-e540-439f-9ae5-654b31ca69f4")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "God I'm hungry. I could eat a horse."
						},
						new TalkAction("25a58cdc-156c-4826-8f42-c6bdfa8a8e4b")
						{
							DelayInSeconds = 2.5,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Or whatever passes for horses on this planet."
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
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("75811100-3d53-412b-80f1-ea1dda662dca")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 1
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("446d2295-8f54-402c-9494-a60ffeff9c98")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I remember first documenting this species down in sector Beta..."
						},
						new TalkAction("88851b71-98e7-43b0-adb6-a187d39058b6")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Never dreamed I would see it on a plate one day."
						}
					}
				},
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
				new ActionSetType("edc91985-f525-464d-bb92-85d6ab9f21fc")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("0c4b0100-8978-4c89-be10-4037c4a74a6d")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "When you eat this dish...please be aware of any symptoms."
						},
						new TalkAction("44e3b39e-f2b8-4a25-8894-87194f560e2e")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "'Cause I'm not entirely sure what pathogens it contains."
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
							DefaultText = "You ever tried eating this?"
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
							DefaultText = "Never had the opportunity, no."
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
							DefaultText = "Well, you're in for a treat."
						}
					}
				},
				new ActionSetType("6de2b3e3-e803-4eb9-8bb3-8bf387a76505")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 3
					},
					Actions = new EventActionType[4]
					{
						new TalkAction("0fad0d85-a494-4df4-a188-deceacd7f6c5")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "One of you wrote the notes on this species, right?"
						},
						new TalkAction("3b6c98df-9ff5-43e8-a169-f1d67f283bb4")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Yes, I did. Anything wrong?"
						},
						new TalkAction("38bc8d9b-2895-4f9b-bf32-60cd408d2fe7")
						{
							DelayInSeconds = 6.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "You've stated that it's edible. But I don't see any evidence supporting that claim."
						},
						new TalkAction("29e5eb8c-e20a-4e96-9778-bbe927cde667")
						{
							DelayInSeconds = 9.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "I'll gladly prove it."
						}
					}
				},
				new ActionSetType("0e158211-420e-41c8-ac38-42d48d41c498")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("e842c143-f6aa-4609-908f-ef1eb0719940")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hmm? The pulmonary tract connects...there??"
						},
						new TalkAction("2e1d56be-c93a-4a86-b956-7de3a3913a84")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "You were supposed to prepare a meal, not do a dissection."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "newPlayerAllegianceMemberRemark",
			FireMode = ActionSetsToFire.RandomValid,
			ChanceToFire = 1f,
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("1dfafd2545yuiopppddxxx-83a3-a65c34756a91")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("de0c6aa5-80d3-adwff423f8-e3c68a3150a3")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = true,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "I'm ready to get started!"
						},
						new TalkAction("fecd21423gf-dhjkipxx75-8204-7f38aa3b0878")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							TurnTowardsListeners = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							CanTalk = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "origin",
									ConstantStringEqual = "playSite"
								}
							},
							DefaultText = "Great! we'll put you to work."
						}
					}
				},
				new ActionSetType("0af7fas235rt-adqeryopxx-xwer9c47-de1f2fdcb2c2")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[2]
					{
						new TalkAction("373254twgfhiopp-9445-39d4d79c959f")
						{
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							TurnTowardsListeners = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Hi, I'm new here. Anything you need done, just ask."
						},
						new TalkAction("9c0987af-szd3254trhgp-gsewp-sad48-fb385d2a5a3a")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							CanTalk = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.TriggeringEntity
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "origin",
									ConstantStringEqual = "playSite"
								}
							},
							DefaultText = "Will do! Welcome!"
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "endConstructSensor",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("6e051e95-904b-4912-bb98-31f988346564")
				{
					MaxFirings = 1,
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					Actions = new EventActionType[1]
					{
						new TalkAction("55102808-e40a-44f8-991c-f2f8d55c188a")
						{
							TalkPriority = TalkAction.TalkActionPriority.Normal,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Sensor is online. Can't have too many of those, just in case."
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "detectTwinkler",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("01d043d0-df8fwa249-4ee1-9b49-3784495aab81")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new TalkAction("2f7c6ec7-b61d-4db4a24awsff0691271e6ad")
						{
							DelayInSeconds = 0.0,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Twinklers are rather common in these parts."
						},
						new TalkAction("84165e97-24a4asfa3w53af-1467aa92ad16")
						{
							DelayInSeconds = 3.5,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Yeah, I think by now everyone is used to seeing them."
						},
						new TalkAction("5cab6aaf-e290-4a07-8cc2-82dd3a0bf906")
						{
							DelayInSeconds = 5.5,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							TalkPriority = TalkAction.TalkActionPriority.High,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							DefaultText = "Still, we need to be on guard."
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
				new ActionSetType("a5232bf4-536756375637d1e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new TalkAction("c1618c9sdfg835675367567er115dc6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "There's a population of whipjaw here."
						}
					}
				},
				new ActionSetType("dsda5232bf4-6764-4d5csdf-be7b-0f0dz1e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c9sdfg8-etyue656eueu1115dc6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Whipjaw. Fascinating physiognomy."
						},
						new TalkAction("f3f4209f-a0yueey65e65eutdc47271ee0")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "The way they feed really is something else."
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
				new ActionSetType("a5232bf4546756375671e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c9sd56uetyuute5dc6d4507")
						{
							DelayInSeconds = 0.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.First,
							CanTalkWhileFighting = false,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = false,
							ActionByAgent = ActionByAgent.OnlyTriggeringEntity,
							DefaultText = "Everyone: Got a fine dendront specimen here."
						},
						new TalkAction("f3f42etyuty5656u56utdc4tyy7271ee0")
						{
							DelayInSeconds = 3.0,
							TalkPriority = TalkAction.TalkActionPriority.Low,
							SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
							CanTalkWhileFighting = true,
							CanTalkWhileSleeping = false,
							CanTalkWhileThreatened = true,
							ActionByAgent = ActionByAgent.RandomInAllegiance,
							DefaultText = "Take care!"
						}
					}
				},
				new ActionSetType("a5232bf4-5637563756375637e4b2a55")
				{
					Condition = new PlayerAllegiancePersons
					{
						MinMembers = 2
					},
					MaxFirings = 1,
					Actions = new EventActionType[2]
					{
						new TalkAction("c1618c93566yurturtyurtsyc6d4507")
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
						new TalkAction("f3f4209fsrtyrtsy556e4yrtsysrty47271ee0")
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
			KeyName = "spawnFirstGuardsSouthSandstoneCave",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("b45c33b1-611c-4d9d-a28d-9e2558b72958")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							GetList = new GetList
							{
								HasPropertiesListKey = "entities",
								FilterCondition = new PropertyCondition
								{
									PropertyKey = "name",
									ConstantStringEqual = "Quadite nest (coord. 15;34)"
								}
							}
						},
						ListCondition = new ListCondition
						{
							CountEqual = 1
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new SpawnEntityAction("09b5863d-14cf-43cb-8c98-b2d10c638a08")
						{
							DelayInSeconds = 7.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("f15ed676-0838-4c90-8dc2-07dae02f5f1d")
						{
							DelayInSeconds = 7.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("446f715a-d083-4c81-9eff-5ee2bede5144")
						{
							DelayInSeconds = 7.2,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "spawnFirstGuardsEastRockCave",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("5670b4cb-94f3-48c7-9861-7d189f0f1b2e")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							GetList = new GetList
							{
								HasPropertiesListKey = "entities",
								FilterCondition = new PropertyCondition
								{
									PropertyKey = "name",
									ConstantStringEqual = "Quadite nest (coord. 38;38)"
								}
							}
						},
						ListCondition = new ListCondition
						{
							CountEqual = 1
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[3]
					{
						new SpawnEntityAction("08319208-49dc-44a6-8032-652a91dfaf59")
						{
							DelayInSeconds = 7.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("e06e4778-cd93-4d24-bd63-47d2d6470be7")
						{
							DelayInSeconds = 7.2,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("d5ebf0a3-bf3d-4bc3-96b8-0d59215f842f")
						{
							DelayInSeconds = 7.7,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "spawnFirstHunterSouthSandstoneCave",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("2db15ab9-6ac6-4b95-8eda-53e96979426f")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							GetList = new GetList
							{
								HasPropertiesListKey = "entities",
								FilterCondition = new PropertyCondition
								{
									PropertyKey = "name",
									ConstantStringEqual = "Quadite nest (coord. 15;34)"
								}
							}
						},
						ListCondition = new ListCondition
						{
							CountEqual = 1
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("9ccb156d-8656-419c-979d-1a6c3d300ba7")
						{
							DelayInSeconds = 7.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "spawnFirstHunterEastRockCave",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("0b5a8dbe-ddee-4444-bd55-6ff9c920bca8")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							GetList = new GetList
							{
								HasPropertiesListKey = "entities",
								FilterCondition = new PropertyCondition
								{
									PropertyKey = "name",
									ConstantStringEqual = "Quadite nest (coord. 38;38)"
								}
							}
						},
						ListCondition = new ListCondition
						{
							CountEqual = 1
						}
					},
					MaxFirings = 1,
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("75560309-4f7a-4b90-bf4f-6327d7a474af")
						{
							DelayInSeconds = 7.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualTwinklerSpawnSouthSandstoneCave",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("edbeebab-a405-4c1b-ab26-fe238d851140")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("2349ab3c-e7b9-4a37-94fb-bbc6f7de7746")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				},
				new ActionSetType("e0a19ae5-18f9-489a-861a-9b165f165147")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("ad5429df-af49-49ed-8618-872c8b9476c9")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("7f76c07b-9d60-4445-a34e-c2f6e477a133")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnTwinklersNorthWestCrevice",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("4fc3937c-ce6d-427e-a904-36da3a2daa77")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("5b588bab-6511-46ce-8504-fc164d3e5d69")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(336f, 864f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				},
				new ActionSetType("64d6e8b3-9f31-45e9-abb4-e170d443ff9a")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("d8e3180c-9117-4c42-aa76-39e83f58a0d2")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(336f, 864f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						},
						new SpawnEntityAction("328eec1a-a4f2-483b-8adc-e9187a9cb5d3")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(336f, 864f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualTwinklerSpawnEastRockCave",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("bcb5cf3f-f81d-468f-840e-b33ab6f0e76f")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("76c98bfd-7912-4429-85be-cfb02207cc64")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				},
				new ActionSetType("a5c16577-d745-4343-bb8f-39833c90b254")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("47e230f3-c47a-49a8-88a6-93dba94448db")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						},
						new SpawnEntityAction("e67850dd-35ca-4073-b56e-aa7596a7ca3d")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "guard"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualHunterSpawnEastRockCave",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("83404770-6816-49b4-a6d6-417c1bcacb5c")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("308b8247-e77f-47d1-b3f4-6223cbc9836d")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				},
				new ActionSetType("08c4c953-7cd9-4366-91a9-8590026c8ce7")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("91fc547c-baa2-473e-9b02-8fdb5cca2c66")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(1860f, 1824f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualHunterSpawnSouthSandstoneCave",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("93934fcc-a495-478d-a2f6-cb38318bb125")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("8fbc5c7d-25c5-4bd6-83f8-90880c4bab60")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				},
				new ActionSetType("5305e8a9-96a7-4b4b-bbcf-4ccaa5124487")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("97d4efee-02d4-40f3-8cfb-eba73abe6205")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:twinkler",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "twinklerAllegiance"
								},
								Location = new Vector3(720f, 1536f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "hunter"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnThunderChickens",
			SetsOfActions = new ActionSetType[2]
			{
				new ActionSetType("ac260fd8-a1d5-4f46-8ea7-5b535c896d11")
				{
					Actions = new EventActionType[3]
					{
						new SpawnEntityAction("9933ecf9-b0b5-4385-b937-df7a2cb79f7d")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(1056f, 1584f, 0f),
								Bulk = 0.35f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("ce4afc68-1a61-4f09-bd8e-ead52bf3e793")
						{
							DelayInSeconds = 5.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(2352f, 2352f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("d96c6a3f-58e4-4344-b894-6db83fd1c3bf")
						{
							DelayInSeconds = 8.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(1008f, 1420f, 0f),
								Bulk = 0.25f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("bf85ee31-a417-40a1-966e-f41dc80a37e3")
				{
					Actions = new EventActionType[3]
					{
						new SpawnEntityAction("7366ba3d-4855-47f9-b129-069763b3b970")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(432f, 1968f, 0f),
								Bulk = 0.31f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("4daaceb1-9102-48dc-aba5-281a78b118a7")
						{
							DelayInSeconds = 7.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(1776f, 2016f, 0f),
								Bulk = 0.35f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("f1cece47-606b-45d4-8d4f-d0a73393dd5d")
						{
							DelayInSeconds = 3.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance"
								},
								Location = new Vector3(1124f, 2304f, 0f),
								Bulk = 0.25f,
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
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnThunderChickenNorthWest",
			SetsOfActions = new ActionSetType[6]
			{
				new ActionSetType("d3d0e288-d570-4993-a9c4-700e1ffa7afa")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("e707ab3c-0e23-4ded-b7ed-6082d4fe3432")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1296f, 1344f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("d1841f29-ca82-4a77-870d-ca7e01528dcd")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1584f, 1584f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("9ae79fd0-455d-4a6d-9973-d3a3f096d6ca")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("b933040f-6d52-481c-89ed-e72dd8ee2bd0")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1296f, 1392f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("c1a72546-11c9-4b6f-a821-51d3130ba461")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1440f, 384f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("4091a097-e693-4d32-8b2b-5780dd7e0fc3")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("a7614fc4-d553-4b51-9203-58f6a1db6009")
						{
							DelayInSeconds = 2.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(432f, 2160f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("438ae113-8991-44e0-a8b5-c22b537f8f14")
						{
							DelayInSeconds = 8.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1344f, 1344f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("c9653218-9225-4ffe-81f0-895a44754e85")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("4fd9bb3d-d2de-44c7-9938-1d3a758326e8")
						{
							DelayInSeconds = 2.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(2352f, 3024f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("58ce5fc2-3cad-435d-af21-a686bbbed0e8")
						{
							DelayInSeconds = 8.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(624f, 3408f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("b9c5ccf5-50a1-4a84-8e02-cd33cfe6ff66")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("19071c19-f604-4501-a8c7-923bbecef043")
						{
							DelayInSeconds = 2.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:pygmyThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1344f, 2448f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("94645a7c-8af8-478c-9209-431fcb62bcb1")
						{
							DelayInSeconds = 8.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(1968f, 1968f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("39477bdf-7d07-4c83-ab49-ccf154e23bea")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("31d5cad2-8c30-4f87-a7e3-11460342af25")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:whiteThunderChicken",
								Name = "Thunder Chicken",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegianceNorthWest"
								},
								Location = new Vector3(528f, 2160f, 0f),
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
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnBinalRatsNorthWest1",
			SetsOfActions = new ActionSetType[5]
			{
				new ActionSetType("099485e2-c127-4c73-bae5-c0b166c6335a")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("98aab509-7785-4748-9304-6d4e0be31d2a")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(1392f, 384f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("7b6d5436-ac4e-4e6c-b417-b753570436e5")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2832f, 960f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("69cd43f7-c537-42f1-b5cc-366a9f11b71a")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("ca94dabc-985b-4bf4-802c-e58db8207273")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(1680f, 144f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("6d670082-95da-477a-b6c6-a820b239ee50")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2448f, 720f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("220618cd-547e-4114-87b4-303f501c7e1c")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("bab9135f-d0f3-4a32-a8e3-ce650f1cd1ef")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(1680f, 384f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("bc3d38f0-2f9b-424a-938c-536a15e34d97")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2064f, 624f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("b94ad113-23fd-4d41-aa0d-d4601a6d22d9")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("9a4633ef-da12-49e7-81f9-39ebd64beda7")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2448f, 336f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("1380cee9-6365-4144-86c5-d7c218124a91")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2640f, 576f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("a565e785-3141-4fc0-87d9-4ad8cd06bae1")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("1380248e-6947-47db-b2a4-da7609d09ad3")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest1"
								},
								Location = new Vector3(2832f, 336f, 0f),
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
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnBinalRatsNorthWest2",
			SetsOfActions = new ActionSetType[4]
			{
				new ActionSetType("d6ac3331-ea08-4d57-9f76-8463c0b69385")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("6315ac26-aec1-4a57-b445-2421f90c1f71")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(2784f, 1872f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("28bc493d-0eba-4922-83f8-b6336ee5cdf3")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(2928f, 1392f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("af301aa6-c956-48a4-b074-225bd09b0349")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("6e46b82b-2e31-4eac-bb1b-ac9974dd5690")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(3312f, 1584f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("a0dce722-8037-4706-a0a4-cf61d16d2ca5")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(3312f, 1584f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("7e3a0e01-dd11-4618-b735-5498f62547cc")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("509a5f6f-7d9e-41b4-a5bf-92b04507dc5f")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(3264f, 1584f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("365da443-4185-4f1a-8e3f-462ab1dfa4e3")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(2832f, 1920f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("257fcda6-c220-453e-a44b-643b8ed9916f")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("ab97c968-642d-4552-86a1-ef97d9a08170")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest2"
								},
								Location = new Vector3(3504f, 1440f, 0f),
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
			FireMode = ActionSetsToFire.RandomValid,
			KeyName = "continualSpawnBinalRatsNorthWest3",
			SetsOfActions = new ActionSetType[4]
			{
				new ActionSetType("55a589fa-9355-439d-9ca2-cda0837582bc")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("69c7473e-ee48-4730-a332-c9def13c13f4")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(1968f, 1968f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("461583e8-4e9a-4bd7-b5ec-d25f504e7b63")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(1152f, 3696f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("e5ddf2e6-770c-4dfa-9698-009ef44a1af9")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("b3e0295a-1fc5-48a8-98cf-138d40d4fe7e")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(864f, 3264f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("941b1763-1d1e-4b01-a40e-5017ece8b494")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(1344f, 2448f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("c06bd989-1ee4-48b5-84f1-b3db40b2ec88")
				{
					Actions = new EventActionType[2]
					{
						new SpawnEntityAction("1f7689be-7364-4c7f-8b52-d21b4ea06348")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(480f, 2160f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("045ebdbc-c86e-406c-9baa-19c77194c7a0")
						{
							DelayInSeconds = 0.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(336f, 3264f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				},
				new ActionSetType("d30cc088-faf2-4e3e-a4cd-717070948e1d")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("111b5814-31ae-4f34-a605-b26aad6dd051")
						{
							DelayInSeconds = 9.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat11",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegianceNorthWest3"
								},
								Location = new Vector3(48f, 3072f, 0f),
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
			KeyName = "placeNests",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("3f913c8f-be82-4e80-be58-14931a373d1e")
				{
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("c9da2a3d-839a-40c7-9d02-01684fee187d")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "terrain:quaditeNest",
								Name = "Quadite nest",
								Location = new Vector3(288f, 864f, 0f),
								Threat = new Threat
								{
									ThreatGroupName = "twinklerAllegiance"
								}
							}
						}
					}
				}
			}
		});
		return list;
	}
}
