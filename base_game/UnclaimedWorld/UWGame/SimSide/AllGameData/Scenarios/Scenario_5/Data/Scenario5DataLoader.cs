using System.Collections.Generic;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.HelpTopics;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

public class Scenario5DataLoader : DataLoader
{
	public Scenario5DataLoader()
		: base(Config.DataType.RGScenario, 0.1f)
	{
	}

	protected override List<EventActionType> InitEventActionTypes()
	{
		return EventActionLoader.Init();
	}

	protected override List<AllegianceEventType> InitAllegianceEvents()
	{
		List<AllegianceEventType> list = new List<AllegianceEventType>();
		list.Add(new AllegianceEventType
		{
			KeyName = "cargoDeliveredToPlayer",
			Event = AllegianceEvents.CargoDeliveredToPlayer,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType
					{
						KeyName = "sdfd367yw6tyutyutyutyruuetyjty4",
						Actions = new EventActionType[1]
						{
							new EventActionDialog
							{
								KeyName = "ttetttttt567567e75676eu5eu5eur",
								DisplayText = new DynamicText
								{
									Text = "BOAT ARRIVES \nThe characteristic sound and smell of its old gasifier engine filled the air as the trader's boat approached. The townspeople helped it to moor, eager to see what it brought. \n After the transactions were done and the various pieces of gossip and news had been exchanged, the boatman bid farewell. Soon the boat had disappeared behind the cliffs, only the smoke lingering in the air."
								},
								DisplayImage = "RiverBoat"
							}
						}
					}
				}
			}
		});
		list.Add(new AllegianceEventType
		{
			KeyName = "transportAbortedContract",
			Event = AllegianceEvents.TransportToPlayerAborted,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType
					{
						KeyName = "0cad94e56utydddddddddytjddgjdtyu26972",
						Actions = new EventActionType[1]
						{
							new EventActionDialog
							{
								KeyName = "24tyutyutycb12-11a4r78kuijkdeuyta2",
								DisplayText = new DynamicText
								{
									Text = "Mission aborted."
								},
								DisplayImage = "RiverBoat"
							}
						}
					}
				}
			}
		});
		return list;
	}

	protected override List<PolledEventType> InitGlobalConditionalEvents()
	{
		return PolledEventsLoader.Init();
	}

	protected override List<AgentActionHook> InitAgentActionHooks()
	{
		return EventHooksLoader.InitAgentActionHooks();
	}

	protected override List<AttackTypeActionHook> InitAttackTypeEventHooks()
	{
		return EventHooksLoader.InitAttackTypeHooks();
	}

	protected override List<ProcessTypeActionHook> InitProcessTypeEventHooks()
	{
		return EventHooksLoader.InitProcessTypeHooks();
	}

	protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
	}

	protected override List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return DetectionEventHooksLoader.InitDetectResourceTypeHooks();
	}

	protected override List<ActionSets> InitActionSets()
	{
		return ActionSetsLoader.Init();
	}

	protected override List<HelpTopic> InitTutorialTopics()
	{
		return TutorialLoader.Init();
	}

	protected override List<EntityType> InitEntityTypes()
	{
		List<EntityType> list = new List<EntityType>();
		StructureLoader.Init(list);
		ItemsLoader.Init(list);
		return list;
	}

	protected override List<EntityData> InitEntityData()
	{
		return EntityDataLoader.Init();
	}

	protected override List<ProcessType> InitProcessTypes()
	{
		return ProcessLoader.Init();
	}

	protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
	{
		return EntityTypeDescriptionLoader.Init();
	}
}
