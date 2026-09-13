using System.Collections.Generic;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		list.Add(new PolledEventType
		{
			KeyName = "introDialogue",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("793567w5zgdewqewrtyiuopppyurtysusrtyurysu20")
					{
						Actions = new EventActionType[5]
						{
							new TalkAction("ab94etyaf324trthgjuiop735673567u5e6uetyueyuyurt9a")
							{
								DelayInSeconds = 2.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "I don't know...the swamp scares me."
							},
							new TalkAction("4e8dstyjutyadkfhaifhiysdjr657eavyaeuf322378ue585e6756e7c")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "John Millet",
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "We just need to take precautions."
							},
							new TalkAction("agfasfaefaeqqgfedhfffwchvgefqjvfg56u65edrtyuhjdyrtuhjsytruysrt9a")
							{
								DelayInSeconds = 8.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Third,
								DefaultText = "But we need one of the chemists from Eden Plains?"
							},
							new TalkAction("cetyu4w6y555555555555yw45ghjsyhtr0")
							{
								DelayInSeconds = 11.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "John Millet",
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Yes, like I said. But they will come!"
							},
							new TalkAction("cetafs32525235adgagayu567hahhueysdtrhjufsghsrfhjsyhtr0")
							{
								DelayInSeconds = 14.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "John Millet",
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Everyone! Please read my plan and let's get this started!"
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
					new ActionSetType("6f046786748674886748674ga352jhiop6478ee92")
					{
						Actions = new EventActionType[1]
						{
							new EventActionDialog("53253675637gftyhy78563876487648644f4e51")
							{
								DelayInSeconds = 0.0,
								DisplayText = new DynamicText
								{
									Text = "Headway - ANNUAL TOWN MEETING \n#JOURNALNAMES \n///////////////////////////////////////////////////// \nPROPOSAL ADOPTED - Work toward the following GOAL: \n \n-Increase population to 15 \nAND \n-Achieve conditions that are equal to Eden Plains: \n#FOODTARGET    #SECURITYTARGET    #COMFORTTARGET \n///////////////////////////////////////////////////// \nThe following plan was presented: \n \nHire a barge from Eden Plains which carries: \n-One or more chemists from the Neson family \n-An extrusion machine \n-A stock of sulfur powder \n \nOnce this arrives, we will construct the polymer workshop and start harvesting marshcot sap from the swamp. The Nesons will help us produce rubber parts that we can sell, thereby financing our growth. \n \nNOTE: For instructions on how to hire the barge, click the GUIDE button which accompanies this screen.",
									SubstitutionValues = new SubstituteValue[4]
									{
										new SubstituteValue
										{
											Placeholder = "#JOURNALNAMES",
											PropertyName = "getJournalHeaderNames"
										},
										new SubstituteValue
										{
											Placeholder = "#COMFORTTARGET",
											Property = new UnaryFunctionNode
											{
												Operator = UnaryExpressionOperator.ComfortRatingToString,
												Operand = new ValueNode
												{
													PropertyKey = "comfortTarget"
												}
											}
										},
										new SubstituteValue
										{
											Placeholder = "#FOODTARGET",
											Property = new UnaryFunctionNode
											{
												Operator = UnaryExpressionOperator.FoodRatingToString,
												Operand = new ValueNode
												{
													PropertyKey = "foodTarget"
												}
											}
										},
										new SubstituteValue
										{
											Placeholder = "#SECURITYTARGET",
											Property = new UnaryFunctionNode
											{
												Operator = UnaryExpressionOperator.SecurityRatingToString,
												Operand = new ValueNode
												{
													PropertyKey = "securityTarget"
												}
											}
										}
									}
								},
								DisplayImage = "IndoorMeeting",
								DialogOptions = new DialogOption[1]
								{
									new DialogOption
									{
										Text = "GUIDE #1",
										Tooltip = "See how to hire the barge (opens separate window.)",
										ActiveInArchive = true,
										ActionSet = "showTutorialRubberScenario5_1"
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
			KeyName = "extrusionMachineArrivalCheck",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			AllowRandomTimeOffset = true,
			Condition = new CustomCondition
			{
				AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
				AllowWhilePlayerThreatened = false,
				AllowWhilePlayerMemberIsFighting = false,
				TargetObject = new TargetObject
				{
					GetList = new GetList
					{
						HasPropertiesListKey = "finishedEntities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "type",
							ConstantStringEqual = "item:extrusionMachineComponents"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountMinimum = new ValueNode
					{
						Decimal = 1f
					}
				}
			},
			ActionSetsKey = "extrusionMachineArrivalScreen"
		});
		string endGameTooltip = "You have succeeded: The colony has grown and reached the goals set by the townspeople. Clicking here will end your current game and take you to the main menu.";
		string continueGameTooltip = "You have achieved the goal of the scenario, but you can keep on playing by selecting this option. How far can you take the town from here?";
		list.Add(new PolledEventType
		{
			KeyName = "winGame",
			Comment = "Tests the ratings of the player allegiance as well as the member count",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new ConditionFunction
				{
					Left = new PlayerAllegiancePersons
					{
						MinMembers = 12
					},
					Operator = OperatorType.And,
					Right = new CustomCondition
					{
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "gameOver",
							BoolValue = false
						}
					}
				},
				Right = new ConditionFunction
				{
					Left = new ConditionFunction
					{
						Left = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.Root,
								GetList = new GetList
								{
									HasPropertiesListKey = "allegiances",
									FilterCondition = new PropertyCondition
									{
										PropertyKey = "keyName",
										ConstantStringEqual = "playerAllegiance"
									}
								}
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "foodRating",
								NumberMinimumInclusive = new ValueNode
								{
									PropertyKey = "foodTarget"
								}
							}
						},
						Operator = OperatorType.And,
						Right = new CustomCondition
						{
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.Root,
								GetList = new GetList
								{
									HasPropertiesListKey = "allegiances",
									FilterCondition = new PropertyCondition
									{
										PropertyKey = "keyName",
										ConstantStringEqual = "playerAllegiance"
									}
								}
							},
							PropertyCondition = new PropertyCondition
							{
								PropertyKey = "comfortRating",
								NumberMinimumInclusive = new ValueNode
								{
									PropertyKey = "comfortTarget"
								}
							}
						}
					},
					Operator = OperatorType.And,
					Right = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.Root,
							GetList = new GetList
							{
								HasPropertiesListKey = "allegiances",
								FilterCondition = new PropertyCondition
								{
									PropertyKey = "keyName",
									ConstantStringEqual = "playerAllegiance"
								}
							}
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "securityRating",
							NumberMinimumInclusive = new ValueNode
							{
								PropertyKey = "securityTarget"
							}
						}
					}
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("c8a8e18a-d51e-4c7a-92a0-251990e6a1f2")
					{
						Actions = new EventActionType[2]
						{
							new WinGameAction("b21eb90e-b8ef-4529-9261-083b7c39bdba")
							{
								Comments = "this is a modal event screen dialog on the game area",
								ModalDialogText = new DynamicText
								{
									Text = "A happy murmur filled the crowded meeting room but died slightly down when the toastmaster stood up. \n-Great to have you all here. Welcome to our newcomers - good to see that you're fitting in so well! As you all know, this place was in a slump not so long ago. But we turned things around. We set a goal for our town, and today we've reached it. I know there's been disagreements along the way but I think we can all be proud of what we've achieved. I honestly feel there's no limit to what we can do! Let's make a toast to the future of Headway!"
								},
								ModalDialogImage = "IndoorMeeting",
								WinScreenText = "The two old friends smiled as they crossed paths. They looked at their village, now a bustling place, voices and sounds of activity coming from all directions. \n-Making headway, huh? \n-We sure are. Hey - it's been awhile since we sat down and talked. \n-Yeah, it's hard to find the time what with all these new people and projects. So many new faces. I've heard that some of the farmers who left for Eden Plains are thinking of coming back! \n-That sounds great. It'll be just like the old days, right? \n-Definitely not! Life is a lot better now!",
								EndGameTooltip = endGameTooltip,
								ContinueGameTooltip = continueGameTooltip,
								ContinueActions = new ActionSets
								{
									SetsOfActions = new ActionSetType[1]
									{
										new ActionSetType("beafa586-82f3-4a3b-a16f-1021b1a0893f")
										{
											Actions = new EventActionType[1]
											{
												new SetPropertyAction("d85e6b69-252a-4f7f-836d-0bc3046c0066")
												{
													PropertyKey = "gameOver",
													Value = new ValueNode
													{
														Bool = true
													}
												}
											}
										}
									}
								}
							},
							new MusicAction("03855c4b-ff01-42f6-b079-92afd0a87e50")
							{
								Song = "Martin Hasseldam - A New World (Alt3) 320kBit"
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
					new ActionSetType("2452d652-4370-4469-9c4e-a1er4efedg-rhiopxcv0b83")
					{
						Actions = new EventActionType[2]
						{
							new LoseGameAction("db7871a4-fe7agagda-413d-a67a-1ec7bbe0c209")
							{
								LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth."
							},
							new MusicAction("a8454060-6bb5-4176-89nnn03-30d4473aa068")
							{
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							}
						}
					}
				}
			}
		});
		return list;
	}
}
