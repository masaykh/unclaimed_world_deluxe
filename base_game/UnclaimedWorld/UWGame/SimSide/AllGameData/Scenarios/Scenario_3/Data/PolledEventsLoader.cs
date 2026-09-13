using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_musicTrackList",
			PollInterval = new ValueNode
			{
				Decimal = 1461f
			},
			AllowRandomTimeOffset = false,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("2452d652-4370-4469-9fsg-frt5hyjuip-a1ebcabe0b83")
					{
						Actions = new EventActionType[6]
						{
							new MusicAction("ytttryrty6547456456456utyudtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 0.0,
								Song = "Jesper Lundager - The Diamond Birds_320"
							},
							new MusicAction("f1254134534151345yteyuteyutes3bab")
							{
								DelayInSeconds = 181.0,
								Song = "Jesper Lundager - Building a Home_320"
							},
							new MusicAction("a13451345316214624576-8903-30d4473aa068")
							{
								DelayInSeconds = 483.0,
								Song = "Jesper Lundager - Prosperous Frontier_320"
							},
							new MusicAction("a84245654264256425642ewtyuw5rtyutywrutywraa068")
							{
								DelayInSeconds = 742.0,
								Song = "Jesper Lundager - Muckroot Toil_320"
							},
							new MusicAction("a524564256425642564256u5sw65eswtywraa068")
							{
								DelayInSeconds = 941.0,
								Song = "Jesper Lundager - Cetian Skies_320"
							},
							new MusicAction("ad24564256245642564256dygjutsduyudtyu68")
							{
								DelayInSeconds = 1223.0,
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_introDialogue",
			StartTimePoint = new TimePoint
			{
				RelativeNoOfDays = 0.0038
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("6f02yyyy814d-0a04-4796-90a4-c2c685b0ee92")
					{
						Actions = new EventActionType[3]
						{
							new TalkAction("54b75xxxxxb99-b8ad-4687-9442-bcc26655ce27")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "The storm has finally died down."
							},
							new TalkAction("cd2cxxxxxx0b10-34dd-4193-a1ac-1aa56a68c053")
							{
								DelayInSeconds = 3.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "Those were the tallest waves I've ever seen."
							},
							new EventActionDialog("532fxxxxxx-ad62-4854-bes8e-f2134bhdde4f4e51")
							{
								DelayInSeconds = 7.0,
								DisplayText = new DynamicText
								{
									Text = "DECISION #1 - Use the PPU \n \nHARRON: I'm just checking our position on the PPU. No info. It's an uncharted island. \n \nSANTILLA: What's that instrument you're using? \n \nHARRON: The PPU? The Pioneer Planning Unit. I got a few of these. I think you should take one each and hang on to them. \n \nSANTILLA: Ok, thanks! Wow...pioneer tech. I've seen people use them but never really knew what they were for... \n \nHARRON: The pioneers had these with them and used them for survival in the early days. \nThe thing tells us about the surroundings. It helps us with keeping track of everything. It'll tell us how to make things, what plants we can eat and so on. To make it out here, we have to rely on the PPU. \n \nSCOYD: They are pretty easy to use -  if you're in doubt, click the button says GUIDE. \n \nHARRON: Yeah do that. It'll get you up to speed. \n \n- // Click the GUIDE button //"
								},
								DisplayImage = "GroupMeeting",
								DialogOptions = new DialogOption[1]
								{
									new DialogOption
									{
										Text = "GUIDE #1",
										Tooltip = "See how to carry out this decision (opens separate window.)",
										ActiveInArchive = true,
										ActionSet = "showTutorial1"
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
			Comment = "should fire app 27 seconds after decision#1, skip this decision if player has already seen gorge",
			KeyName = "TUTORIAL_scoutDecision",
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 35f
				}
			},
			Condition = new CustomCondition
			{
				PropertyCondition = new PropertyCondition
				{
					PropertyKey = "gorgeDetected",
					BoolValue = false
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("6f035625465345yrtyhrsthyrty646674y76w2")
					{
						Actions = new EventActionType[1]
						{
							new EventActionDialog("532fxxrtyurwtyujtyuessyrtsyshgfdhghfhdde4f4e51")
							{
								DelayInSeconds = 0.0,
								DisplayText = new DynamicText
								{
									Text = "DECISION #2 SCOUT \n \nHARRON: Well, there's nothing on this beach, apart from rocks and thorny bramble. \nWe're gonna starve to death if we stay. \n \nSANTILLA: No mussels, no anything? \n \nHARRON: Nah, already checked the area. Look in the PPU. You can see what resources are here. \n \nSCOYD:Let's get going and explore the area further up. \n \n- //Scout the area north of the wreck // -"
								},
								DisplayImage = "GroupMeeting",
								DialogOptions = new DialogOption[1]
								{
									new DialogOption
									{
										Text = "GUIDE #2",
										Tooltip = "See how to carry out this decision (opens separate window.)",
										ActiveInArchive = true,
										ActionSet = "showTutorial2"
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
			KeyName = "TUTORIAL_placeCatamaran",
			ActionSetsKey = "placeCatamaranTerrain"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_placeTerritoryLock1",
			ActionSetsKey = "placeTerritoryLock1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_placeTerritoryLock2",
			ActionSetsKey = "placeTerritoryLock2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_timedSpawnBeginningPopulation",
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("382aa085-44c6-48d3-b7a4-a91d7d9c1ca6")
					{
						Actions = new EventActionType[4]
						{
							new SpawnEntityAction("6a2dfa36-7055-4b40-858d-ca3d7c3bc142")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonTutAllegianceNorth"
									},
									Location = new Vector3(3504f, 2420f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("3daeff2f-2bf9-41ae-8dbf-10b386e9581d")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonTutAllegianceNorth"
									},
									Location = new Vector3(3600f, 2448f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("df0971ec-ef16-4d39-8b1a-6ee4d752079c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonTutAllegianceNorth"
									},
									Location = new Vector3(3620f, 2496f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("a847c30c-0fea-42e4-8608-33acb1f76fcb")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "bushDragonSouth",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonTutAllegianceSouth"
									},
									Location = new Vector3(3456f, 2688f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
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
			KeyName = "TUTORIAL_triggerBeforeCrevice",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new AreaCondition
			{
				Area = new Rectangle(2880, 3696, 432, 192)
			},
			ActionSetsKey = "beforeCreviceRemark"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_3commonOilTubers",
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
							ConstantStringEqual = "item:commonOilTubers"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountMinimum = new ValueNode
					{
						Decimal = 3f
					}
				}
			},
			ActionSetsKey = "3commonOilTubers"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_checkcommonOilTubersGatheredDelay",
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
				PropertyCondition = new PropertyCondition
				{
					PropertyKey = "commonOilTubersGatheredDelay",
					BoolValue = true
				}
			},
			ActionSetsKey = "commonOilTubersGatheredDelayEvent"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_ReadyToBuildCampfire",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			AllowRandomTimeOffset = true,
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:stones"
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
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:firewood"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMinimum = new ValueNode
						{
							Decimal = 3f
						}
					}
				}
			},
			ActionSetsKey = "readyToBuildCampfire"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_ReadyToCookcommonOilTubers",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			AllowRandomTimeOffset = true,
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:commonOilTubers"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMinimum = new ValueNode
						{
							Decimal = 3f
						}
					}
				},
				Right = new CustomCondition
				{
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "campfireFinished",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "readyToCookcommonOilTubers"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_triggerDetectBushDragonBackup",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new AreaCondition
			{
				Area = new Rectangle(3168, 2640, 192, 144)
			},
			ActionSetsKey = "detectBushDragon"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_3Flint3WaterCaneStems",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			AllowRandomTimeOffset = true,
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:flintRough"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMinimum = new ValueNode
						{
							Decimal = 3f
						}
					}
				},
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:waterCaneStem"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMinimum = new ValueNode
						{
							Decimal = 3f
						}
					}
				}
			},
			ActionSetsKey = "readyToMakeSpears"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_3SpearsFinished",
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
							ConstantStringEqual = "item:improvisedFlintSpear"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountMinimum = new ValueNode
					{
						Decimal = 3f
					}
				}
			},
			ActionSetsKey = "3SpearsFinished"
		});
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_triggerBeforeBushDragonFight",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new AreaCondition
				{
					Area = new Rectangle(3168, 2544, 192, 192)
				},
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "finishedEntities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "type",
								ConstantStringEqual = "item:improvisedFlintSpear"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMinimum = new ValueNode
						{
							Decimal = 3f
						}
					}
				}
			},
			ActionSetsKey = "readyToAttackBushDragons"
		});
		list.Add(new PolledEventType
		{
			Comment = "triggers when they have 3 dragons killed, condition:  kill all 'guards' in north allegiance. the south 'scout' I destroyed by a destroyentity event..",
			KeyName = "TUTORIAL_triggerAfterBushDragonFight",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 5f
				}
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
						HasPropertiesListKey = "allegiances",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "keyName",
							ConstantStringEqual = "bushDragonTutAllegianceNorth"
						},
						NextList = new GetList
						{
							HasPropertiesListKey = "members"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountEqual = 0
				}
			},
			ActionSetsKey = "afterBushDragonFight"
		});
		string endGameTooltip = "Fast forward till we are rescued! (Clicking here will end the game and take you to the end screen.)";
		list.Add(new PolledEventType
		{
			KeyName = "TUTORIAL_Rescue_3Left",
			PollInterval = new ValueNode
			{
				Decimal = 10f
			},
			AllowRandomTimeOffset = true,
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 2f
				}
			},
			Condition = new CustomCondition
			{
				AllowWhileAllPlayerMembersAreSleepingOrCollapsed = false,
				AllowWhilePlayerThreatened = false,
				AllowWhilePlayerMemberIsFighting = false,
				PropertyCondition = new PropertyCondition
				{
					PropertyKey = "signalPyreLit",
					BoolValue = true
				}
			},
			ActionSets = new ActionSets
			{
				FireMode = ActionSetsToFire.AllValid,
				ChanceToFire = 1f,
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("9f789999999999999999999999999999999999ac")
					{
						MaxFirings = 1,
						Condition = new PlayerAllegiancePersons
						{
							MinMembers = 3,
							MaxMembers = 3
						},
						Actions = new EventActionType[3]
						{
							new TalkAction("626479844444444478984677867864acd")
							{
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "Wait, what is that...out there? You think we've been seen already?"
							},
							new WinGameAction("b21e3567567567567567567567567567567567567dba")
							{
								Comments = "this is a modal event screen dialog on the game area",
								DelayInSeconds = 4.0,
								ModalDialogText = new DynamicText
								{
									Text = "SCOYD: Nah...not a boat. Looks like a flock of glowbirds. \nHARRON: Well, maybe next time. Let's keep the pyre burning from now on. Until a boat passes by, we should be able to survive here. \nSANTILLA:  Sure hope we don't have to wait too long."
								},
								ModalDialogImage = "GroupMeeting",
								WinScreenText = "SHIP'S LOG: The Starhawk \nDATE: 11-11 2264 \nTEMP: 16 C; WIND: SSW; WEATHER: SUNNY; \n \nAt 13:20 we spotted a smoke signal coming from the normally uninhabited Brightburn Island, went ashore and found 3 stranded sailors from Noame. They were in good health considering they had been stranded for several months. \nWe will take them back to Noame once we complete our round trip.",
								EndGameTooltip = endGameTooltip,
								AllowContinueGame = false,
								ContinueActions = new ActionSets
								{
									SetsOfActions = new ActionSetType[1]
									{
										new ActionSetType("c28346786786786786786786786786786786786785b0")
										{
											Actions = new EventActionType[1]
											{
												new SetPropertyAction("146786786988888888888888888775416")
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
							new MusicAction("dfryuuuuuuuuuuujfxdgjhjdghf4")
							{
								DelayInSeconds = 4.0,
								Song = "Martin Hasseldam - A New World (Alt3) 320kBit"
							}
						}
					}
				}
			}
		});
		return list;
	}
}
