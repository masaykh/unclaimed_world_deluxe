using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class EventHooksLoader
{
	public static List<AgentActionHook> InitAgentActionHooks()
	{
		return new List<AgentActionHook>
		{
			new AgentActionHook
			{
				KeyName = "startHarvestRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.StartHarvesting,
				ActionSetsKey = "startHarvestRemark"
			},
			new AgentActionHook
			{
				KeyName = "endHarvestRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.CompletedHarvesting,
				ActionSetsKey = "endHarvestRemark"
			},
			new AgentActionHook
			{
				KeyName = "startConstructionRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.StartConstructing,
				ActionSetsKey = "startConstructionRemark"
			},
			new AgentActionHook
			{
				KeyName = "endConstructionRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.CompletedConstructing,
				ActionSetsKey = "endConstructionRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanKilledEnemy",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.KilledEnemy,
				ActionSetsKey = "humanKilledEnemyRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanMissedEnemy",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.MissedAnAttackOnAnEnemy,
				ActionSetsKey = "humanMissedEnemyRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanHitEnemy",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitEnemyRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanTakingAHit",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.TakingAHit,
				ActionSetsKey = "humanHitByEnemyRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanKilledInCombat",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.KilledInCombat,
				ActionSetsKey = "humanKilledInCombatRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanFleeing",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.Fleeing,
				ActionSetsKey = "humanFleeingRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanGoingToSleepRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.GoingToSleep,
				ActionSetsKey = "humanGoingToSleepRemark"
			},
			new AgentActionHook
			{
				KeyName = "humanEatingRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.Eating,
				ActionSetsKey = "humanEatingRemark"
			}
		};
	}

	public static List<AttackTypeActionHook> InitAttackTypeHooks()
	{
		return new List<AttackTypeActionHook>
		{
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithLowPunch",
				TypeKey = "personPunchLowRight",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitEnemyWithPunchRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithHighPunch",
				TypeKey = "personPunchHighRight",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitEnemyWithPunchRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanKilledEnemyWithImprovisedBasicArrow",
				TypeKey = "shootImprovisedBasicArrow",
				Hook = AgentActionHooks.KilledEnemy,
				ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanKilledEnemyWithImprovisedChitinousArrow",
				TypeKey = "shootImprovisedChitinousArrow",
				Hook = AgentActionHooks.KilledEnemy,
				ActionSetsKey = "humanKilledWithImprovisedBasicArrowRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanKilledEnemyWithImprovisedMetalArrow",
				TypeKey = "shootImprovisedMetalArrow",
				Hook = AgentActionHooks.KilledEnemy,
				ActionSetsKey = "humanKilledWithImprovisedMetalArrowRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithImprovisedBasicArrow",
				TypeKey = "shootImprovisedBasicArrow",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitWithImprovisedArrowRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithImprovisedChitinousArrow",
				TypeKey = "shootImprovisedChitinousArrow",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitWithImprovisedArrowRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithImprovisedMetalArrow",
				TypeKey = "shootImprovisedMetalArrow",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitWithImprovisedArrowRemark"
			}
		};
	}

	public static List<ProcessTypeActionHook> InitProcessTypeHooks()
	{
		return new List<ProcessTypeActionHook>
		{
			new ProcessTypeActionHook
			{
				KeyName = "startSalvageBoatWreckRemark",
				TypeKey = "salvageBoatWreck",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startSalvageBoatWreckRemark"
			},
			new ProcessTypeActionHook
			{
				KeyName = "endSalvageBoatWreckRemark",
				TypeKey = "salvageBoatWreck",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "endSalvageBoatWreckRemark"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startBuildRopeBridgeRemark",
				TypeKey = "buildRopeBridge",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startBuildRopeBridgeRemark"
			},
			new ProcessTypeActionHook
			{
				KeyName = "ropeBridgeFinished",
				TypeKey = "buildRopeBridge",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "ropeBridgeFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "ropeBridgeFinishedRemark",
				TypeKey = "buildRopeBridge",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "ropeBridgeFinishedRemark"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructCampfireFinished",
				TypeKey = "constructCampfire",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "constructCampfireFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "makeMashedCommonOilTubersFinished",
				TypeKey = "makeMashedCommonOilTubers",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "makeMashedCommonOilTubersFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "makeImprovisedFlintSpearFinished",
				TypeKey = "makeImprovisedFlintSpear",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "makeImprovisedFlintSpearFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "constructSignalPyreFinished",
				TypeKey = "constructSignalPyre",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "constructSignalPyreFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startLightSignalPyreRemark",
				TypeKey = "lightSignalPyre",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startLightSignalPyreRemark"
			},
			new ProcessTypeActionHook
			{
				KeyName = "lightSignalPyreFinished",
				TypeKey = "lightSignalPyre",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "lightSignalPyreFinished"
			},
			new ProcessTypeActionHook
			{
				KeyName = "lightSignalPyreRemark",
				TypeKey = "lightSignalPyre",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "lightSignalPyreRemark"
			}
		};
	}
}
