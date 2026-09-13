using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4.Data;

public class EventHooksLoader
{
	public static List<EntityEventHook> InitEntityEventHooks()
	{
		return new List<EntityEventHook>();
	}

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
				KeyName = "humanEatingRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.Eating,
				ActionSetsKey = "humanEatingRemark"
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
				KeyName = "newMemberRemark",
				TypeKey = "entity:human",
				Hook = AgentActionHooks.DisembarkedNewPlayerAllegianceMember,
				ActionSetsKey = "SANDBOXMAP_newPlayerAllegianceMemberRemark"
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
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithImprovedFireExtinguisherBushDragonPoison",
				TypeKey = "shootImprovedFireExtinguisherBushDragonPoison",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitEnemyWithBushDragonPoisonRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanKilledEnemyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.KilledEnemy,
				ActionSetsKey = "humanKilledEnemyWithRifleRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitEnemyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.HitEnemy,
				ActionSetsKey = "humanHitEnemyWithRifleRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanMissedEnemyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.MissedAnAttackOnAnEnemy,
				ActionSetsKey = "humanMissedEnemyWithRifleRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanKilledPreyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.KilledPrey,
				ActionSetsKey = "humanKilledPreyWithRifleRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanHitPreyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.HitPrey,
				ActionSetsKey = "humanHitPreyWithRifleRemark"
			},
			new AttackTypeActionHook
			{
				KeyName = "humanMissedPreyWithRifle",
				TypeKey = "shootCoilRifle",
				Hook = AgentActionHooks.MissedAnAttackOnPrey,
				ActionSetsKey = "humanMissedPreyWithRifleRemark"
			}
		};
	}

	public static List<ProcessTypeActionHook> InitProcessTypeHooks()
	{
		return new List<ProcessTypeActionHook>
		{
			new ProcessTypeActionHook
			{
				KeyName = "bigBombActivatedHook1",
				TypeKey = "useVarmintBomb",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "bigBombActivated"
			},
			new ProcessTypeActionHook
			{
				KeyName = "bigBombActivatedHook2",
				TypeKey = "useVarmintBomb",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "bigBombTalk"
			},
			new ProcessTypeActionHook
			{
				KeyName = "endConstructWigwamSpoakShinglesHook",
				TypeKey = "constructWigwamSpoakShingles",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "endConstructWigwamSpoakShingles"
			},
			new ProcessTypeActionHook
			{
				KeyName = "produceCampfire",
				TypeKey = "constructCampfire",
				Hook = AgentActionHooks.CompletedProducing,
				ActionSetsKey = "produceCampfire"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedPhantomWeaver",
				TypeKey = "makeRoastedPhantomWeaver",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedWebWing",
				TypeKey = "makeRoastedWebWing",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedCrestedFoiler",
				TypeKey = "makeRoastedCrestedFoiler",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedGoldenCenobite",
				TypeKey = "makeRoastedGoldenCenobite",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedMuckGrinder",
				TypeKey = "makeRoastedMuckGrinder",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedCrazyDweller",
				TypeKey = "makeRoastedCrazyDweller",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedDaggermouth",
				TypeKey = "makeRoastedDaggermouth",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "startMakeRoastedImpEel",
				TypeKey = "makeRoastedImpEel",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startMakeStrangeAnimalMeal"
			},
			new ProcessTypeActionHook
			{
				KeyName = "AFrameTarpProduceShelter",
				TypeKey = "constructA-frameTarp",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "AFrameSpoakLeavesProduceShelter",
				TypeKey = "constructA-frameSpoakLeaves",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "AFrameScrapsProduceShelter",
				TypeKey = "constructA-frameScraps",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "DaysheenTipiProduceShelter",
				TypeKey = "constructDaysheenTipi",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "Lean-toTarpProduceShelter",
				TypeKey = "constructLean-toTarp",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "Lean-toSpoakLeavesProduceShelter",
				TypeKey = "constructLean-toSpoakLeaves",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "Lean-toScrapsProduceShelter",
				TypeKey = "constructLean-toScraps",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			},
			new ProcessTypeActionHook
			{
				KeyName = "DomeShelterTarpProduceShelter",
				TypeKey = "constructDomeShelterTarp",
				Hook = AgentActionHooks.StartProducing,
				ActionSetsKey = "startConstructShelter"
			}
		};
	}
}
