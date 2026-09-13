using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class Scenario2DataLoader : DataLoader
{
	public Scenario2DataLoader()
		: base(Config.DataType.RGScenario, 0.1f)
	{
	}

	protected override List<EventActionType> InitEventActionTypes()
	{
		return EventActionLoader.Init();
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

	protected override List<EntityType> InitEntityTypes()
	{
		List<EntityType> list = new List<EntityType>();
		StructureLoader.Init(list);
		ItemsLoader.Init(list);
		return list;
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
