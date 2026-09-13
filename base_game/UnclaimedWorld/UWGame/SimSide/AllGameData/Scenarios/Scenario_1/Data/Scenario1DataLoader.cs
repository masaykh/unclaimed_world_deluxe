using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

public class Scenario1DataLoader : DataLoader
{
	public Scenario1DataLoader()
		: base(Config.DataType.RGScenario, 0.1f)
	{
	}

	protected override List<EntityType> InitEntityTypes()
	{
		base.InitEntityTypes();
		List<EntityType> list = new List<EntityType>();
		ItemsLoader.Init(list);
		StructureLoader.Init(list);
		CreatureLoader.Init(list);
		return list;
	}

	protected override List<PersonalityType> InitPersonalityTypes()
	{
		return PersonalityLoader.Init();
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

	protected override List<ProcessType> InitProcessTypes()
	{
		return ProcessLoader.Init();
	}

	protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
	{
		return EntityTypeDescriptionLoader.Init();
	}
}
