namespace UWGame.SimSide.Entities;

public enum AgentActionHooks
{
	StartProducing,
	CompletedProducing,
	CompletedConstructing,
	StartConstructing,
	StartHarvesting,
	CompletedHarvesting,
	MissedAnAttackOnAnEnemy,
	MissedAnAttackOnPrey,
	StartedEffect,
	EndedEffect,
	TakingAHit,
	HitEnemy,
	HitPrey,
	KilledEnemy,
	KilledPrey,
	Fleeing,
	DiedOnPlaySite,
	KilledInCombat,
	Dying,
	GoingToSleep,
	Eating,
	SwitchedToPlayerAllegiance,
	DisembarkedNewPlayerAllegianceMember,
	DecidedToLeaveAllegiance,
	StartsToLeaveAllegiance,
	LeavesSiteForNewAllegiance,
	ForceDropsItem
}
