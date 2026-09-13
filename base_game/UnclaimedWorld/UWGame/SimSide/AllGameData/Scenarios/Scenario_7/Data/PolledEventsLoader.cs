using System.Collections.Generic;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;

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
					new ActionSetType("793567w5zgdewqertuyhrjdfsrfjsrtyurysu20")
					{
						Actions = new EventActionType[3]
						{
							new TalkAction("ab94etyjtyejdfhfsdryyueyuyurt9a")
							{
								DelayInSeconds = 2.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Irina Nadova",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "Now, have you all read the briefing?"
							},
							new TalkAction("4e8476567e56yu7teureuf322378ue585e6756e7c")
							{
								DelayInSeconds = 5.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.RandomInAllegiance,
								SpeakerDenomination = TalkAction.SpeakerInConversation.Second,
								DefaultText = "I'd like to look at it again."
							},
							new TalkAction("agfasetyuetyujyhtdjdghedrtyuhjdyrtuhjsytruysrt9a")
							{
								DelayInSeconds = 8.0,
								TalkPriority = TalkAction.TalkActionPriority.High,
								CanTalkWhileFighting = false,
								CanTalkWhileSleeping = false,
								CanTalkWhileThreatened = false,
								TurnTowardsListeners = true,
								ActionByAgent = ActionByAgent.OnlySpecific,
								NameOfSpeaker = "Irina Nadova",
								SpeakerDenomination = TalkAction.SpeakerInConversation.First,
								DefaultText = "When you're done reading, I'm gonna tell you what I think."
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
				RelativeNoOfDays = 0.01428
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("6f0467867tyjudhgdstyjhsytdjdtyjdghj478ee92")
					{
						Actions = new EventActionType[1]
						{
							new EventActionDialog("5325367dghjdghjdyttdyjdgjdg644f4e51")
							{
								DelayInSeconds = 0.0,
								DisplayText = new DynamicText
								{
									Text = "From: THE OVERSEER AI (PROJECT CANOPY) \nTo: THE NADOVA MINING CREW \n \n///////////////////////////////////////////////////// \nMATERIALS REQUESTED: \n \nScandium (refined) \nTerbium (refined) \n \nContact Duke's Landing to see the quantity we require and the price we offer. \nNOTE: For more detailed advice, click the 'GUIDE' button underneath this message \nBest of luck to your team. \n///////////////////////////////////////////////////// \n \nAfter they had read the message, Irina looked at her crew. \n'Getting the scandium will be easy. But if you're worried about the swarmers up north, I'm willing to skip the terbium. We don't owe Project CANOPY anything. In fact, I think the whole project is a waste of resources.' \nThe crew members nodded. \n'As you know, my dream is to settle on the grasslands to the south, that's my main reason for going out here. There's a neighbor settlement that we can trade with.' \n'So whenever you feel like starting a homestead, we'll do it. But let's do some mining first.' "
								},
								DisplayImage = "GroupMeeting",
								DialogOptions = new DialogOption[1]
								{
									new DialogOption
									{
										Text = "GUIDE",
										Tooltip = "How to provide metals to Duke's Landing (opens separate window.)",
										ActiveInArchive = true,
										ActionSet = "showTutorialMiningScenario7"
									}
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
