using System.Collections.Generic;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		list.Add(new PolledEventType
		{
			Comment = "Has hardcoded relative start times. Attempts to play night music at the appropriate time, but this will fail if the sceario starts at a different time",
			KeyName = "SANDBOXNOMADMAP_musicTrackList",
			PollInterval = new ValueNode
			{
				Decimal = 3200f
			},
			AllowRandomTimeOffset = false,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("245afrthyjuiop2-4370-4469-9c4e-a1ebcabe0b83")
					{
						Actions = new EventActionType[9]
						{
							new MusicAction("faf18teyudtyyyyyyyteyuteyutes3bab")
							{
								Song = "Jesper Lundager - Building a Home_320"
							},
							new MusicAction("a84teyuye567u856eureewtyuw5rtyutywrutywraa068")
							{
								DelayInSeconds = 302.0,
								Song = "Jesper Lundager - Muckroot Toil_320"
							},
							new MusicAction("a5e67u56tuuuutysdurtydsu5sw65eswtywraa068")
							{
								DelayInSeconds = 501.0,
								Song = "Jesper Lundager - Cetian Skies_320"
							},
							new MusicAction("a84fff54060-6bb5-4176-8903-30d4473aa068")
							{
								DelayInSeconds = 775.0,
								Song = "Jesper Lundager - Prosperous Frontier_320"
							},
							new MusicAction("ad6udtyuddgjid7eyudygjutsduyudtyu68")
							{
								DelayInSeconds = 1034.0,
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							},
							new MusicAction("adddddf6e7uteyudtyutydtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 1272.0,
								Song = "Martin Hasseldam - Life in the Wilderness"
							},
							new MusicAction("wretwertrewtyrtwywrtytyutydtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 1796.0,
								Song = "Martin Hasseldam - Settle"
							},
							new MusicAction("yttutyutyutyut6t7uityutyutyudtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 2711.0,
								Song = "Jesper Lundager - The Diamond Birds_320"
							},
							new MusicAction("klkjkljhlk67ii67iyuiyuiyuiyuidtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 2892.0,
								Song = "Martin Hasseldam - A New World (Alt3) 320kBit"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueFarming",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("793567w56efa25646673hgioppxzsusrtyurysu20")
					{
						Actions = new EventActionType[4]
						{
							new TalkAction("4e8dasfeafgudgkjsankugrnksjevnsjawdwdawpastyjutysdjr657e78ue585e6756e7c")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "These grasslands. Have you seen anything like it?"
							},
							new TalkAction("ab94vfrcfxfxfwqqfpp65edrtyuhjdyrtuhjsytruysrt9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Very fertile, yeah."
							},
							new TalkAction("f6ddtyu65e78u5euiteduetsyursetyuty4")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
								DefaultText = "Just a bit of hard work and this could all be fields!"
							},
							new TalkAction("cetyu5adwadvaef67fxfueysdtrhjufsghsrfhjsyhtr0")
							{
								DelayInSeconds = 8.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "That's the spirit!"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueHunting",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("793srthgfshhhhhhhhhhrsrdyuxgfyhyxfghxfghxfg0")
					{
						Actions = new EventActionType[3]
						{
							new TalkAction("4e8dstyjutysdjr657e78ue585e6756edfsgijdhzfpgidzfpogih")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "This is turnip territory!"
							},
							new TalkAction("ab94srt76rtyrtsytesgh07sygh08se7ryhysrt9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Yep. Some good hunting can be done here."
							},
							new TalkAction("csfgh54675333333333333333333333re7rt7rt7rtr0")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "That's what we're here for. Let's see who takes down the first!"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueHuntingChickens",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("793srthjdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgsdtgshxfg0")
					{
						Actions = new EventActionType[3]
						{
							new TalkAction("4e8dstyjuttyj56y7y7y7y7y7u5y7uhzfpgidzfpogih")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "You'll see these woods are teeming with thunder chickens..."
							},
							new TalkAction("ab94srt5e6ujdtyujhtdyjhesd5r6y7jryhysrt9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Our spring traps should find good use."
							},
							new TalkAction("csfgh54675rthjurtysjtysjurt7rt7rtr0")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "Let's get those rawhides!"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueFishing",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("7fsghasrthdfuaghdyfaugoldfabhoglidfagda20")
					{
						Actions = new EventActionType[4]
						{
							new TalkAction("4egdfa968gt9r7aetg9t9d6fatg97ag76rae9tgrae7c")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "What do you think? Is this our new home?"
							},
							new TalkAction("ab954t96g53e97t684g3e96t78g89ae47tg98er7atgt9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "I like the river."
							},
							new TalkAction("f6g8u574wth58047ygh807rtysdoyiutw0s95tygs4")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
								DefaultText = "If there's fish, we stay. Else we move on."
							},
							new TalkAction("csrtg895ys98y7ghs0r8et7ygoidusghpsiuhhfsr0")
							{
								DelayInSeconds = 8.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "I agree. We follow the carbon tail!"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueVersatile",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("7fsgha6y56rtyrtwyrtwyrtwyrtwyrtswya20")
					{
						Actions = new EventActionType[4]
						{
							new TalkAction("4eetyutdyudtygyuh65rtu5eutyjuty7c")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "Well. Not bad, huh?"
							},
							new TalkAction("abdtyuyttttttttttttju567eir68okiryuidtt9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "This is a nice corner of the world."
							},
							new TalkAction("f6g8u5rstyhu6r4h4eyhusrtyhsryhsryetygs4")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
								DefaultText = "I'm impressed. Good job finding this place."
							},
							new TalkAction("csr5e675e7u5yeurtysyuyrsysrtyshhfsr0")
							{
								DelayInSeconds = 8.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "I cannot promise life'll be easy, but we're off to a good start!"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueThrong",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("793dfsghjghfsjxdgdte6y75edtyjd20")
					{
						Actions = new EventActionType[4]
						{
							new TalkAction("4edgje7e6ddurtyjutysrjudfyjsfj7c")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "Great to see you all again. Sorry for the delay!"
							},
							new TalkAction("abdghj5e76eiyrtukjiddghjdhjdsfj9a")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "It's ok. We knew you'd probably get held up."
							},
							new TalkAction("f6dghj576e5ejdtyjdghjdghjdghjdgy4")
							{
								DelayInSeconds = 6.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Castor Hernes",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "So. This is a good place to settle?"
							},
							new TalkAction("cet5e678utyhjdfgjdgfjdghjdgj675tjjy7r0")
							{
								DelayInSeconds = 10.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Sure hope so. We got a lot of mouths to feed."
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "introDialogueScreen",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.015
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("6f04af432652448674886748674864786478ee92")
					{
						Actions = new EventActionType[1]
						{
							new EventActionDialog("5325vfvfvcxfdgtty56378563876487648644f4e51")
							{
								DelayInSeconds = 0.0,
								DisplayText = new DynamicText
								{
									Text = "CASTOR HERNES: \nAlright. This is how I see it: We have radio equipment. As soon as it's set up, we can get in touch with Starsnare Point and make a deal to bring supplies and more people that are willing to join. We just need to make a landing for a boat to moor. There's a good spot nearby that I'm sure you've seen. \n \nBut the transport is not going to be cheap and we don't have much money. \nSo, my suggestion is to wait until we have goods to sell. Of course, in order to trade we need to build a simple port to store the goods. \n \nLINSEY CATTIER: \nYeah. We should also make the most of the animal migrations that are happening this time of year. They will be migrating in flocks across this landscape. Bajingan and lesser whipjaw. We should get ready to bag as many as possible, their hides can be worth a lot. \n \nCASTOR HERNES: Talk it over and we'll see what we can agree on!"
								},
								DisplayImage = "GroupMeeting"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXMAP_loseGame",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new PlayerAllegiancePersons
			{
				MaxMembers = 0
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("24zdfcxvbnm456782-4370-4469-9c4e-a1ebcabe0b83")
					{
						Actions = new EventActionType[2]
						{
							new LoseGameAction("db7sad23571a4-fe7b-413d-a67a-1ec7bbe0c209")
							{
								LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth."
							},
							new MusicAction("a8454060-6gggbb5-4176-8903-30d4473aa068")
							{
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "initializeGlobalFarmingProperties",
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 0.5f
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("b7413ad2-6bb6-4856-8527-1a9b0f508c74")
					{
						Actions = new EventActionType[7]
						{
							new SetPropertyAction("b0e4d7df-6d48-47c9-86d2-4a5b77192b62")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "FarmingUpdateInterval",
								Value = new ValueNode
								{
									Decimal = 16f
								}
							},
							new SetPropertyAction("f3a23fc7-9956-4ed6-324524tsgfhp-322f7885041f")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "weedGrowthSpeed",
								Value = new ValueNode
								{
									Decimal = 0.025f
								}
							},
							new SetPropertyAction("f3a23fc7-99564fdfeaf-eppetet-eta885041f")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "weedingJobTriggerLimit",
								Value = new ValueNode
								{
									Decimal = 0.45f
								}
							},
							new SetPropertyAction("16f435yuiklope9-dcfa-4de1-9dc7-80aagdd7f27ecabe")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "maxTimeBetweenWeeding",
								Value = new ValueNode
								{
									Decimal = 200f
								}
							},
							new SetPropertyAction("f3axrf23fc7-9956-4ed6-998d-322f7885041f")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "fertilizeJobTriggerLimit",
								Value = new ValueNode
								{
									Decimal = 0.45f
								}
							},
							new SetPropertyAction("16e3254thkjppe9-dcfa-4de1-9dc7-80a7f27ecabe")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "maxTimeBetweenFertilizing",
								Value = new ValueNode
								{
									Decimal = 1600f
								}
							},
							new SetPropertyAction("16e7d4xre9-dcfa-4de1-9dcasf565pp7f27ecabe")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "fertilizeReductionFactor",
								Value = new ValueNode
								{
									Decimal = 0.08f
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "initializeGlobalFishTrapProperties",
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 0.5f
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("b892083b-156d-4873-b2c9-95eaa6071837")
					{
						Actions = new EventActionType[3]
						{
							new SetPropertyAction("b4958be2-2da3-405e-bdc6-06120d16beab")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "FishTrapSpawningLoopInterval",
								Value = new ValueNode
								{
									Decimal = 12f
								}
							},
							new SetPropertyAction("712465b0-2005-4139-a15f-b813fa236198")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "FishTrapCheckingLoopInterval",
								Value = new ValueNode
								{
									Int = 65
								}
							},
							new SetPropertyAction("16e7agdd4e9-dcfa-4de1-9dc7-80a7f624fjlopp27ecabe")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "maxTimeBetweenFishCheckJobs",
								Value = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.Root
										},
										PropertyKey = "FishTrapSpawningLoopInterval"
									},
									Operator = ExpressionOperator.Multiply,
									Right = new ValueNode
									{
										Decimal = 7f
									}
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "initializeGlobalAnimalTrapProperties",
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 0.5f
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("b8920834db-156d-4873-b2c9-95eaa60d71837")
					{
						Actions = new EventActionType[2]
						{
							new SetPropertyAction("b4958dgfhbe2-2da3-405e-bdc6-06120d16bdfgheab")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
								Value = new ValueNode
								{
									Decimal = 180f
								}
							},
							new SetPropertyAction("6dsb4958dge2-2da3-405e-bdc6-06120dxdf")
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
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "weedingJobLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FarmingUpdateInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isFarmPlot",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("createWeedingJob")
					{
						Condition = new ConditionFunction
						{
							Left = new ConditionFunction
							{
								Left = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "weedGrowthProgress",
										NumberMinimumInclusive = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.Root
											},
											PropertyKey = "weedingJobTriggerLimit"
										}
									}
								},
								Operator = OperatorType.Or,
								Right = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "weedingLastTimeStamp",
										NumberMaximumNotInclusive = new FunctionNode
										{
											Left = new ValueNode
											{
												PropertyKey = "getTime"
											},
											Operator = ExpressionOperator.Minus,
											Right = new ValueNode
											{
												TargetObject = new TargetObject
												{
													TargetObjectType = TargetObjectType.Root
												},
												PropertyKey = "maxTimeBetweenWeeding"
											}
										}
									}
								}
							},
							Operator = OperatorType.And,
							Right = new ConditionFunction
							{
								Left = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "plantJobProcessKey",
										IsNull = false
									}
								},
								Operator = OperatorType.And,
								Right = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "cropsAreGrowing",
										BoolValue = true
									}
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new CreateJobAction("c10ef45b-a390-487b-97f7-de3282fcc0ec")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "weedJobProcessKey"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "fertilizeJobLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FarmingUpdateInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isFarmPlot",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("createFertilizeJob")
					{
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "nutrientLevel",
									NumberMaximumNotInclusive = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.Root
										},
										PropertyKey = "fertilizeJobTriggerLimit"
									}
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropsAreGrowing",
									BoolValue = true
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new CreateJobAction("c10ef45b-a390-487b-97f7-de328zdgw2fcc0ec")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "fertilizeJob"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "harvestJobLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FarmingUpdateInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isFarmPlot",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("createHarvestJob")
					{
						Comments = "crops finished growing, run if: cropGrowthElapsedTime > cropGrowthPeriod AND cropGrowthProgress > 0",
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropGrowthElapsedTime",
									NumberMinimumInclusive = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.DynamicTarget
										},
										PropertyKey = "cropGrowthPeriod"
									}
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropGrowthProgress",
									NumberNotEqual = new ValueNode
									{
										Decimal = 0f
									}
								}
							}
						},
						Actions = new EventActionType[2]
						{
							new CreateJobAction("92517e64-5775-4e5f-b428-5a819aaa6ad3")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "harvestJobProcessKey"
								}
							},
							new CancelJobAction("580d7c0f-1267-4b58-9783-aa0856c84470")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "weedJobProcessKey"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "plantingJobLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FarmingUpdateInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isFarmPlot",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("createPlantingJob")
					{
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "plantJobProcessKey",
									IsNull = false
								}
							},
							Operator = OperatorType.And,
							Right = new ConditionFunction
							{
								Left = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "cropsAreGrowing",
										BoolValue = false
									}
								},
								Operator = OperatorType.And,
								Right = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "cropGrowthProgress",
										NumberEqual = new ValueNode
										{
											Decimal = 0f
										}
									}
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new CreateJobAction("e787ae79-d4aa-40b7-8edc-915374bd310d")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "plantJobProcessKey"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "farmPlotLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FarmingUpdateInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				SetsOfActions = new ActionSetType[9]
				{
					new ActionSetType("deltaCropCalc")
					{
						Condition = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.PolledEventSource
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "cropsAreGrowing",
								BoolValue = true
							}
						},
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("664683a1-16f9-48f7-914e-9afe45tiupp01bc45e")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "deltaCropGrowthProgress",
								Value = new FunctionNode
								{
									Left = new UnaryFunctionNode
									{
										Operand = new FunctionNode
										{
											Left = new ValueNode
											{
												TargetObject = new TargetObject
												{
													TargetObjectType = TargetObjectType.PolledEventSource
												},
												PropertyKey = "cropGrowthProgress"
											},
											Operator = ExpressionOperator.Plus,
											Right = new FunctionNode
											{
												Left = new ValueNode
												{
													TargetObject = new TargetObject
													{
														TargetObjectType = TargetObjectType.PolledEventSource
													},
													PropertyKey = "cropGrowthSpeed"
												},
												Operator = ExpressionOperator.Multiply,
												Right = new FunctionNode
												{
													Left = new FunctionNode
													{
														Left = new ValueNode
														{
															Decimal = 0.8f
														},
														Operator = ExpressionOperator.Minus,
														Right = new FunctionNode
														{
															Left = new ValueNode
															{
																TargetObject = new TargetObject
																{
																	TargetObjectType = TargetObjectType.PolledEventSource
																},
																PropertyKey = "weedGrowthProgress"
															},
															Operator = ExpressionOperator.Minus,
															Right = new ValueNode
															{
																TargetObject = new TargetObject
																{
																	TargetObjectType = TargetObjectType.PolledEventSource
																},
																PropertyKey = "cropGrowthProgress"
															}
														}
													},
													Operator = ExpressionOperator.Multiply,
													Right = new ValueNode
													{
														TargetObject = new TargetObject
														{
															TargetObjectType = TargetObjectType.PolledEventSource
														},
														PropertyKey = "nutrientLevel"
													}
												}
											}
										},
										Operator = UnaryExpressionOperator.ClampToWithinZeroAndOne
									},
									Operator = ExpressionOperator.Minus,
									Right = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "cropGrowthProgress"
									}
								}
							}
						}
					},
					new ActionSetType("deltaWeedCalc")
					{
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("ddbb3419-17b8-40e6-a356-86ae5ssdfgxffd06ad")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "deltaWeedGrowthProgress",
								Value = new FunctionNode
								{
									Left = new FunctionNode
									{
										Left = new FunctionNode
										{
											Left = new ValueNode
											{
												TargetObject = new TargetObject
												{
													TargetObjectType = TargetObjectType.PolledEventSource
												},
												PropertyKey = "weedGrowthProgress"
											},
											Operator = ExpressionOperator.Plus,
											Right = new FunctionNode
											{
												Left = new FunctionNode
												{
													Left = new ValueNode
													{
														TargetObject = new TargetObject
														{
															TargetObjectType = TargetObjectType.PolledEventSource
														},
														PropertyKey = "weedSpeedFactor"
													},
													Operator = ExpressionOperator.Multiply,
													Right = new ValueNode
													{
														TargetObject = new TargetObject
														{
															TargetObjectType = TargetObjectType.Root
														},
														PropertyKey = "weedGrowthSpeed"
													}
												},
												Operator = ExpressionOperator.Multiply,
												Right = new ValueNode
												{
													TargetObject = new TargetObject
													{
														TargetObjectType = TargetObjectType.PolledEventSource
													},
													PropertyKey = "nutrientLevel"
												}
											}
										},
										Operator = ExpressionOperator.ClampTop,
										Right = new ValueNode
										{
											Decimal = 1f
										}
									},
									Operator = ExpressionOperator.Minus,
									Right = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "weedGrowthProgress"
									}
								}
							}
						}
					},
					new ActionSetType("setOvergrownFlag")
					{
						Condition = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.PolledEventSource
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "weedGrowthProgress",
								NumberMinimumInclusive = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.Root
									},
									PropertyKey = "weedingJobTriggerLimit"
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("83723b01-4dd9-4bd2-a816-239ea2443272")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "spriteFlag",
								Value = new ValueNode
								{
									String = "Overgrown"
								}
							}
						}
					},
					new ActionSetType("growCrops")
					{
						Comments = "simulate crop growth & increment cropGrowthElapsedTime by FarmingUpdateInterval, only runs when the cropCycle is active",
						Condition = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.PolledEventSource
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "cropsAreGrowing",
								BoolValue = true
							}
						},
						Actions = new EventActionType[2]
						{
							new SetPropertyAction("664683a1-16f9-4afg3tyip007-914e-9d91d01bc45e")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropGrowthProgress",
								Value = new FunctionNode
								{
									Left = new FunctionNode
									{
										Left = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "cropGrowthProgress"
										},
										Operator = ExpressionOperator.Plus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "deltaCropGrowthProgress"
										}
									},
									Operator = ExpressionOperator.ClampTop,
									Right = new ValueNode
									{
										Decimal = 1f
									}
								}
							},
							new SetPropertyAction("aca584a3-e3d2-44ee-a49e-fa5949663974")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropGrowthElapsedTime",
								Value = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "cropGrowthElapsedTime"
									},
									Operator = ExpressionOperator.Plus,
									Right = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.Root
										},
										PropertyKey = "FarmingUpdateInterval"
									}
								}
							}
						}
					},
					new ActionSetType("3ea6be94-dd99-4425-94f6-5ed3daed0cf8")
					{
						Comments = "set the spriteflag to display the growing crops, run if: cropsAreGrowing == true && cropGrowthProgress > 0.2f",
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropsAreGrowing",
									BoolValue = true
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropGrowthProgress",
									NumberMinimumInclusive = new ValueNode
									{
										Decimal = 0.2f
									}
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("cd821abb-8e6a-4d17-97b1-88481677bc8d")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "spriteFlag",
								Value = new ValueNode
								{
									String = "HasCrops"
								}
							}
						}
					},
					new ActionSetType("growWeeds")
					{
						Comments = "update the timeStamp & grow the weeds also: this runs everytime",
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("66af132exzvr3a1-16f9-48f7-914e-9d91d01bc45e")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "weedGrowthProgress",
								Value = new FunctionNode
								{
									Left = new FunctionNode
									{
										Left = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "weedGrowthProgress"
										},
										Operator = ExpressionOperator.Plus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "deltaWeedGrowthProgress"
										}
									},
									Operator = ExpressionOperator.ClampTop,
									Right = new ValueNode
									{
										Decimal = 1f
									}
								}
							}
						}
					},
					new ActionSetType("reduceNutrients")
					{
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("ddbb3419-17b8-40e6-a356-86ae5ffd06ad")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
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
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "nutrientLevel"
										},
										Operator = ExpressionOperator.Minus,
										Right = new FunctionNode
										{
											Left = new FunctionNode
											{
												Left = new ValueNode
												{
													TargetObject = new TargetObject
													{
														TargetObjectType = TargetObjectType.PolledEventSource
													},
													PropertyKey = "deltaWeedGrowthProgress"
												},
												Operator = ExpressionOperator.Minus,
												Right = new ValueNode
												{
													TargetObject = new TargetObject
													{
														TargetObjectType = TargetObjectType.PolledEventSource
													},
													PropertyKey = "deltaCropGrowthProgress"
												}
											},
											Operator = ExpressionOperator.Multiply,
											Right = new ValueNode
											{
												TargetObject = new TargetObject
												{
													TargetObjectType = TargetObjectType.Root
												},
												PropertyKey = "fertilizeReductionFactor"
											}
										}
									},
									Operator = ExpressionOperator.ClampBottom,
									Right = new ValueNode
									{
										Decimal = 0f
									}
								}
							}
						}
					},
					new ActionSetType("cropsFinishedGrowing")
					{
						Comments = "crops finished growing, run if: cropGrowthElapsedTime > cropGrowthPeriod && cropsAreGrowing == true",
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropGrowthElapsedTime",
									NumberMinimumInclusive = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "cropGrowthPeriod"
									}
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "cropsAreGrowing",
									BoolValue = true
								}
							}
						},
						Actions = new EventActionType[5]
						{
							new SetPropertyAction("e7a023c6-fe66-4733-a0dd-75a0d86d0c1d")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropsAreGrowing",
								Value = new ValueNode
								{
									Bool = false
								}
							},
							new SetPropertyAction("15cb4a30-96cb-4d51-857c-412aa3bb15a3")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "clearFlag",
								Value = new ValueNode
								{
									String = "HasCrops"
								}
							},
							new SetPropertyAction("8ca79361-8b71-40da-a0c0-8fe3fa3d577f")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "spriteFlag",
								Value = new ValueNode
								{
									String = "Ripe"
								}
							},
							new SetPropertyAction("820f5954-60a4-4c54-a15c-5eaa5b472e8d")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropsFullyGrownAt",
								Value = new ValueNode
								{
									PropertyKey = "getTime"
								}
							},
							new SetPropertyAction("0f2491e0-5d3b-4ff5-b512-58cb9e8ca8d4")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropState",
								Value = new ValueNode
								{
									String = "Ready for harvest"
								}
							}
						}
					},
					new ActionSetType("removeUnharvestedCrops")
					{
						Comments = "removes the crops if they got too old, their age is measured from the timepoint cropsFullyGrownAt",
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "resourceTimeToLive",
									NumberMaximumNotInclusive = new FunctionNode
									{
										Left = new ValueNode
										{
											PropertyKey = "getTime"
										},
										Operator = ExpressionOperator.Minus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "cropsFullyGrownAt"
										}
									}
								}
							},
							Operator = OperatorType.And,
							Right = new ConditionFunction
							{
								Left = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.PolledEventSource
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "cropGrowthProgress",
										NumberMinimumNotInclusive = new ValueNode
										{
											Decimal = 0f
										}
									}
								},
								Operator = OperatorType.And,
								Right = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.PolledEventSource
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "cropsAreGrowing",
										BoolValue = false
									}
								}
							}
						},
						Actions = new EventActionType[5]
						{
							new SetPropertyAction("e6f1c115-3f28-4700-b9f9-a8c78d7f018f")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "clearFlag",
								Value = new ValueNode
								{
									String = "Ripe"
								}
							},
							new SetPropertyAction("6df49a90-147d-4f0e-9454-6de439c464a2")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "spriteFlag",
								Value = new ValueNode
								{
									String = "Dead"
								}
							},
							new SetPropertyAction("bd147d5c-0d5c-453f-a7d2-052a7373307c")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropState",
								Value = new ValueNode
								{
									String = "Withered"
								}
							},
							new SetPropertyAction("3e0e1f23-0ee1-43dd-b866-3c57f3947d52")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "cropGrowthProgress",
								Value = new ValueNode
								{
									Decimal = 0f
								}
							},
							new SetPropertyAction("e5aaf3527-62e3-4d8a-bafy46058-e1cd9ed57d0e")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "harvestDate",
								SetValueToNull = true
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "checkFishTrapJobLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FishTrapCheckingLoopInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isFishTrap",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("createCheckFishTrapJob")
					{
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "maxTimeBetweenFishCheckJobs",
									NumberMaximumNotInclusive = new FunctionNode
									{
										Left = new ValueNode
										{
											PropertyKey = "getTime"
										},
										Operator = ExpressionOperator.Minus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.DynamicTarget
											},
											PropertyKey = "fishTrapLastTimeChecked"
										}
									}
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "maxNoOfFishToSpawnAtATime",
									NumberMinimumInclusive = new ValueNode
									{
										Int = 0
									}
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new CreateJobAction("c10ef45b-a390-487b-97f7-defdsafa3245uyoppfcc0ec")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									String = "checkFishTrap"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			Comment = "Polled entity\r\n                            * Checks if the current trap has room for a new fish,\r\n                            * check if the fish gets spawned, spawns the fish\r\n                            * and updates the timers.",
			KeyName = "fishTrapSpawningLoop",
			PollInterval = new ValueNode
			{
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root
				},
				PropertyKey = "FishTrapSpawningLoopInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				SetsOfActions = new ActionSetType[2]
				{
					new ActionSetType("0cd83c05-8239-4275-9f9f-1c93d7e189b4")
					{
						Comments = "calculate the amount of fish to spawn and save it in the amountOfFish property. Will only spawn if the number is > 1",
						Actions = new EventActionType[1]
						{
							new SetPropertyAction("1a91a62c-244a-48c1-baed-a1b25a34cf5b")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyKey = "amountOfFish",
								Value = new FunctionNode
								{
									Left = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "maxNoOfFishToSpawnAtATime"
									},
									Operator = ExpressionOperator.Multiply,
									Right = new ValueNode
									{
										PropertyKey = "getRandomSimNumber"
									}
								}
							}
						}
					},
					new ActionSetType("24ae08c6-3297-4106-9241-16ff5f097a3b")
					{
						Comments = "condition run if room for fish & fish spawns",
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "freeStorage",
									NumberMinimumInclusive = new FunctionNode
									{
										Left = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "fishTypeBulk"
										},
										Operator = ExpressionOperator.Multiply,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.PolledEventSource
											},
											PropertyKey = "amountOfFish"
										}
									}
								}
							},
							Operator = OperatorType.And,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.PolledEventSource
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "spawnChance",
									NumberMinimumInclusive = new ValueNode
									{
										PropertyKey = "getRandomSimNumber"
									}
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new SpawnEntityAction("fdc4358c-3924-4184-bf72-c01917a4a8ef")
							{
								DelayInSeconds = 0.0,
								LogAsProduction = true,
								AddToContainer = new ContainerLocation
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.PolledEventSource
									}
								},
								Amount = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.PolledEventSource
									},
									PropertyKey = "amountOfFish"
								},
								OwnedBy = new AllegianceAndExpedition
								{
									DynamicAllegianceKey = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "owningAllegiance"
									},
									DynamicExpeditionKey = new ValueNode
									{
										TargetObject = new TargetObject
										{
											TargetObjectType = TargetObjectType.PolledEventSource
										},
										PropertyKey = "owningExpedition"
									}
								},
								EntityData = null,
								EntityType = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.PolledEventSource
									},
									PropertyKey = "fishType"
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "checkAnimalTrapJobLoop",
			PollInterval = new ValueNode
			{
				PropertyKey = "animalTrapPollInterval"
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.FirstValid,
				ActionTargets = new TargetObject
				{
					TargetObjectType = TargetObjectType.PolledEventSource,
					GetList = new GetList
					{
						HasPropertiesListKey = "OwnedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "isAnimalTrap",
							BoolValue = true
						}
					}
				},
				SetsOfActions = new ActionSetType[2]
				{
					new ActionSetType("animalTrapJob")
					{
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
									NumberMaximumNotInclusive = new FunctionNode
									{
										Left = new ValueNode
										{
											PropertyKey = "getTime"
										},
										Operator = ExpressionOperator.Minus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.DynamicTarget
											},
											PropertyKey = "animalTrapLastTimeChecked"
										}
									}
								}
							},
							Operator = OperatorType.Or,
							Right = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "active",
									BoolValue = false
								}
							}
						},
						Actions = new EventActionType[1]
						{
							new CreateJobAction("c10ef45b-a3afs325wt5-fdw4b-97f7-de3282sr324fcc0ec")
							{
								DelayInSeconds = 0.0,
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.DynamicTarget
								},
								ProcessTypeKey = new ValueNode
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyKey = "jobName"
								}
							}
						}
					},
					new ActionSetType("animalTrapJobTalk")
					{
						Condition = new ConditionFunction
						{
							Left = new CustomCondition
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyCondition = new PropertyCondition
								{
									PropertyKey = "maxTimeBetweenAnimalTrapCheckJobs",
									NumberMaximumNotInclusive = new FunctionNode
									{
										Left = new ValueNode
										{
											PropertyKey = "getTime"
										},
										Operator = ExpressionOperator.Minus,
										Right = new ValueNode
										{
											TargetObject = new TargetObject
											{
												TargetObjectType = TargetObjectType.DynamicTarget
											},
											PropertyKey = "animalTrapLastTimeChecked"
										}
									}
								}
							},
							Operator = OperatorType.Or,
							Right = new ConditionFunction
							{
								Left = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.DynamicTarget
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "active",
										BoolValue = false
									}
								},
								Operator = OperatorType.And,
								Right = new CustomCondition
								{
									TargetObject = new TargetObject
									{
										TargetObjectType = TargetObjectType.Root
									},
									PropertyCondition = new PropertyCondition
									{
										PropertyKey = "animalCheckCanTalk",
										BoolValue = true
									}
								}
							}
						},
						Actions = new EventActionType[2]
						{
							new TalkAction("c8c619b3-6180-4dd4-a0c0-08dddfd7a558")
							{
								TalkPriority = TalkAction.TalkActionPriority.Low,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = false,
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								DefaultText = "Someone should check up on our animal trap."
							},
							new SetPropertyAction("c8c619as-f324u-ipppdfsfdd4-a0c0-08dddfd7456a558")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "animalCheckCanTalk",
								Value = new ValueNode
								{
									Bool = false
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
