using System.Xml.Serialization;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.AI.Constants;

public class AIConstants : IGameDataObject
{
	public Combat Combat = new Combat();

	public EvaluatorWeights EvaluatorWeights = new EvaluatorWeights();

	public Ratings Ratings = new Ratings();

	public Migration Migration = new Migration();

	public AgentCondition AgentCanVote = new AgentCondition
	{
		AllowEmigrating = false,
		AllowFighting = true,
		AllowSleeping = true,
		AllowThreatened = true,
		AllowTravelling = false,
		AllowUnconscious = false
	};

	public AgentCondition PolicyCanBeAdopted = new AgentCondition
	{
		AllowEmigrating = false,
		AllowFighting = false,
		AllowSleeping = false,
		AllowThreatened = true,
		AllowTravelling = false,
		AllowUnconscious = false
	};

	public double TimeToWaitBeforeStartingGoal = 0.6;

	public int ShortestRouteThatAcceptsPassengers = 6;

	public int ShortestDistanceToConsiderAVehicle = 400;

	public byte HighestDiscomfortLevelForWorkToContinue = 16;

	public byte HighestDiscomfortLevelForLeisureActivityToContinue = 10;

	public byte HighestDiscomfortLevelForLeisureActivityToStart = 4;

	public byte ComfortBonusFromBeingInsideBuildingsOrVehicles = 4;

	public float ResidenceScoreMustBeBetterToMove = 0.05f;

	public double AtomicGoalPeriodInSeconds = 0.2;

	public float NoOfTimesPerSecondToArbitrateWhileBusy = 0.3f;

	public float CurrentGoalInertia = 0.1f;

	public float TimeToReachFullGoalSwitchInertia = 3f;

	public double CooldownTimeAfterUnsuccessfulHunt = 30.0;

	public double IncreaseCooldownTimeFactorAfterUnsuccessfulHunt = 1.5;

	public double MaxCooldownTimeAfterUnsuccessfulHunt = 180.0;

	public double CooldownTimeAfterSpottingPrey = 45.0;

	public double MaxTimeForSliding = 1.0;

	public float DistanceSquaredToConsiderOnRoad = 16f;

	public float DistanceToConsiderOnRoad = 4f;

	public double DistancePromptingHaltRequest = 200.0;

	public double DistancePromptingSlowdownRequest = 200.0;

	public float JobWithZeroMaterialsDesirability;

	public double IdleGoalDesirability = 0.01;

	public double EvaluatorTimePenaltyForUsingVehicles = 4.0;

	public float WorkTimeFactorToEvaluateToolEnergyUse = 1.1f;

	public float MaxDistanceForReplenishItems = 960f;

	public float LimitInSecondsToMaterialRunoutToMatter = 10f;

	public int AreaSizeRadiusForFindingParkingSpot = 3;

	public double EmigrateDeciderUpdateIntervalInSeconds = 10.0;

	public float PeriodAfterJoiningBeforeEmigrateIsPossibleInSeconds = 120f;

	public int DeprecateMemoryFactsWithinTileRadius = 2;

	public double SecondsToKeepDeprecatedMemoryFacts = 10.0;

	public float DistanceFromExpeditionToReturnHome = 400f;

	public float TimeSpentIdlingToConsiderReturningHome = 5f;

	public float TimeSpentIdlingToConsiderReturningHomeWithBoldStance = 25f;

	public float TimeSpentIdlingToConsiderTeleporting = 60f;

	public float MaximumRadiusOfTrapAreaToTeleportFrom = 180f;

	public float MaximumDistanceFromExpeditionToChasePrey = 1200f;

	public double MaxTimeForLeavingPatrolPostUntilFreed = 16.0;

	public float MaxDistanceOutsidePatrolZoneToChaseTargets = 120f;

	public float PriorityOfThreatJobs = 100f;

	public float PriorityOfAssetThreatJobs = 4.9f;

	public float PriorityOfNeeds = 5f;

	public float PriorityOfEmigrating = 5f;

	public float PriorityOfFindingANewHome = 2.5f;

	public float PriorityOfHauling = 1f;

	public float PriorityOfWorkJobs = 0.9f;

	public float LowJobModifier = 0.5f;

	public float HighJobModifier = 2f;

	public double MinimumThreatRatingToBeAThreatToAgents = 0.45;

	public double AggressionScoreFraction = 0.1;

	public double NearnessScoreFraction = 0.8999999761581421;

	public float RecentlyKilledCarcassScoreFraction = 0.5f;

	public float DetectionBonusForRememberedEntitiesInSameSpot = 0.5f;

	public float MinimumChanceToStopAndLookWhenSearching = 0.1f;

	public float ChanceToStopAndLookWhenSearchingFactor = 1f;

	public float TimeToWaitWhenStoppedAndSearchingMean = 3f;

	public float TimeToWaitWhenStoppedAndSearchingStdDev = 1f;

	public float MinimumChanceToStopAndLookWhenSearchingResources = 0.3f;

	public float ChanceToStopAndLookWhenSearchingResourcesFactor = 20f;

	public float TimeToWaitWhenStoppedAndSearchingResourcesMean = 4f;

	public float TimeToWaitWhenStoppedAndSearchingResourcesStdDev = 2f;

	public float MigrateStabilityFrequency = 0.001f;

	public float JobDistanceFromExpeditionToBringOptionalWeapons = 300f;

	public float JobDistanceFromExpeditionToBringOptionalFood = 300f;

	public float JobDistanceFromExpeditionToBringOptionalEquipment = 300f;

	public float MaxRadiusFromExpeditionToGatherOptionalEquipment = 390f;

	public float SleepNeedToIgnoreSleepPolicy = 0.05f;

	public float SleepNeedLimitToSwapWithASleeper = 0.25f;

	public float SleepNeedLimitToLetSomeoneElseSleepInstead = 0.4f;

	public float NutrientsScoreForEating = 0.45f;

	public float NutrientsScoreToTriggerStarvedEating = 0.9f;

	public float UpperRangeOfEffectiveDamageInStandardDeviations = 2f;

	public double TimeAllocatedInSecondsForTimeSlicedSystems = 0.009;

	public float PlayerMovementMapUpdateInterval = 5f;

	public float OtherMovementMapUpdateInterval = 7.5f;

	public float ResidenceConditionToStartRepair = 0.9f;

	public float OtherStructureConditionToStartRepair = 0.4f;

	public int JobsPerWorkerCap = 5;

	public int MinimumJobsPerStandingOrder = 2;

	public double MinimumSearchTimeInAttackZone = 10.0;

	public float MaxDistanceBetweenAgentsToAlwaysAllowForceDrop = 160f;

	public byte HighestThreatLevelToAllowForceDrop;

	public float MinimumDistanceToThreatsToAllowForceDrop = 120f;

	public float CheckingJobRange = 150f;

	public string GenericFoodItemKey = "item:smokedCarbonTail";

	public float IntervalInDaysForComputingProductImportance = 0.5f;

	public float MaxDistanceToLookForAdditionalFood = 360f;

	public int MaxAdditionalFoodItems = 5;

	public float MaxAdditionalTimeForStarvedHaulingJobScore = 120f;

	public double TimePassedForHaulingJobsToScoreHigher = 300.0;

	[XmlIgnore]
	public EntityType GenericFoodItem;

	public void Initialize()
	{
	}

	public void PostDataCompleteInitialize()
	{
		GenericFoodItem = GameData.Instance.AllEntityTypes[GenericFoodItemKey];
	}
}
