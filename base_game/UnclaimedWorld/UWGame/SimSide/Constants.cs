using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AllGameData.Constants;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide;

public class Constants : IGameDataObject
{
	public int StartingYear = 2238;

	public float VehicleSpeedCausingMaximumDust = 60f;

	public float SkimmerDustScale = 0.3f;

	public float VehicleDustScale = 0.1f;

	public float MeanAmbientTemperature = 288f;

	public float PathActivation = 0.6f;

	public float AmountToAddToPathOnTraversal = 0.1f;

	public float TilesBetweenAddedWaypoints = 1.5f;

	public float BulkOfTreeBlockingCarTransport = 4f;

	public float BulkOfTreeBlockingATVTransport = 6f;

	public float BulkOfTreeBlockingFootTransport = 8f;

	public float FlashDuration = 2000f;

	public Color FlashingColorWhenClicked = Color.Aquamarine;

	public Color FlashingColorWhenDetected = Color.White;

	public float AverageAgentWatingTimeToTriggerAlert = 4f;

	public float NumberOfAgentsWatingToTriggerAlert = 2f;

	public float StoredPercentageMeansHauling = 0.4f;

	public float StoredPercentageMeansHeavyHaul = 0.6f;

	public float DistanceForLongHaul = 400f;

	public float MinimumSpeedForTerrainToHaveEffect = 16f;

	public float DistanceSquaredLimitForWaypoints = 4f;

	public float InteractionDistanceForAgents = 12f;

	public float BulkCapacityForSingleTile = 35f;

	public float BulkCapacityForSubTile = 1.1f;

	public float RottingSpeedOfOrganicFibers = 0.01f;

	public float DryingSpeedOfOrganicFibers = 0.01f;

	public float RottingSpeedOfOrganicMaterial = 0.02f;

	public float DryingSpeedOfOrganicMaterial = 0.01f;

	public float PhosphorusAmountInOrganicMaterial = 0.05f;

	public float NitrogenAmountInOrganicMaterial = 0.05f;

	public float MeleeToHitBonusOnDistractedTarget = 0.2f;

	public float MeleeToHitBonusOnProneTarget = 0.25f;

	public float FractionOfHitpointsCausingCollapse = 0.2f;

	public float DamageAmountFractionCausingHitReaction = 0.005f;

	public int TimeIntervalForDPSMoraleFactor = 5;

	public float MoraleDamageForZeroDamageAttacks = 0.001f;

	public float vitalBodyPartDamageEvaluationBoost = 15f;

	public float ChanceToDropWeaponWhenFleeing = 0.5f;

	public float CloseDistanceForRangedAttack = 96f;

	public float CloseDistanceRangedAttackToHitFactor = 1.4f;

	public float FractionOfMaxHitpointsLostPerSecondWhenDying = 0.05f;

	public NormalDistribution ExpertSkillDistribution = new NormalDistribution
	{
		Mean = 1.0,
		StandardDeviation = 0.0
	};

	public NormalDistribution HighSkillDistribution = new NormalDistribution
	{
		Max = 0.99f,
		Min = 0.6f
	};

	public NormalDistribution MediumSkillDistribution = new NormalDistribution
	{
		Max = 0.7f,
		Min = 0.3f
	};

	public NormalDistribution LowSkillDistribution = new NormalDistribution
	{
		Max = 0.4f,
		Min = 0.1f
	};

	public NormalDistribution ZeroSkillDistribution = new NormalDistribution
	{
		Max = 0.099f,
		Min = 0f
	};

	public float ExpertSkillBonus = 0.25f;

	public float ExpertSkillBonusThreshold = 0.95f;

	public NormalDistribution HighTradeAmountDistribution = new NormalDistribution
	{
		Max = 3f,
		Min = 2f
	};

	public NormalDistribution MediumTradeAmountDistribution = new NormalDistribution
	{
		Max = 2f,
		Min = 0.6f
	};

	public NormalDistribution LowTradeAmountDistribution = new NormalDistribution
	{
		Max = 0.5f,
		Min = 0.2f
	};

	public float ZeroEnergyProductionFactor = 0.4f;

	public float ZeroEnergyToHitFactor = 0.7f;

	public float ZeroEnergyMeleeDamageFactor = 0.7f;

	public float ZeroEnergyIdleWalkFactor = 0.2f;

	public float LowestCombinedSkillAndEnergyProductionFactors = 0.32f;

	public float OxygenEnergyRequiredToStartRunning = 0.2f;

	public float ToolUseDestroyChanceInterval = 1f;

	public int ExpeditionStorageRadius = 3;

	public SleepNeed SleepNeed = new SleepNeed();

	public PhysicalWork PhysicalWork = new PhysicalWork();

	public float DefaultDistanceToAlwaysDetectHiddenEntities = 48f;

	public float DefaultDistanceToAlwaysDetectResources = 28f;

	public float DetectionUpdatesPerSecond = 0.5f;

	public float ConditionDamageMeanToContentsOfDestroyedContainers = 0.2f;

	public float ConditionDamageSpreadToContentsOfDestroyedContainers = 0.2f;

	public float DefaultPassiveStealthFactor = 0.2f;

	public float BestDetectionFactorOfActivityWhenNotLooking = 0.5f;

	public float BestDetectionFactorOfActivityWhenSearching = 3f;

	public float ChanceToExultAfterWinning = 0.4f;

	public float InterestLevelForConversation = 15f;

	public float InterestLevelForCollidedEntityMean = 10f;

	public float InterestLevelForCollidedEntityStdDeviation = 2f;

	public float InterestLevelForSpottedEntityMean = 40f;

	public float InterestLevelForSpottedEntityStdDeviation = 3f;

	public float InterestLevelForSpottedResourceMean = 4f;

	public float InterestLevelForSpottedResourceStdDeviation = 1.2f;

	public float InterestLevelDropOffPerSecond = 2f;

	public float InterestInertiaFactor = 1.28f;

	public float InterestLevelForTurnToFace = 60f;

	public float MaxHeadTurnAngleForZeroInterest = (float)Math.PI / 4f;

	public float MaxHeadTurnAngleForMaxInterest = (float)Math.PI / 2f;

	public float DecreaseInOxygenMuscleEnergyWhenRunningPerSecond = 0.1f;

	public float CollisionResistanceFromNonMovers = 0.005f;

	public float CollisionResistanceFromMovers = 0.3f;

	public float MinimumSkillValueToUse = 0.1f;

	public float MinimumAgentBulk = 0.1f;

	public float InterestLevelToCauseBodyTurn = 30f;

	public int TalkSpeedInCharactersPerSecond = 17;

	public float MinimumTalkDurationInSeconds = 3f;

	public float MaximumTalkDurationInSeconds = 8f;

	public SerializableDictionary<StrengthRating, float> StrengthRatings = new SerializableDictionary<StrengthRating, float>
	{
		{
			StrengthRating.None,
			0f
		},
		{
			StrengthRating.VeryWeak,
			0.5f
		},
		{
			StrengthRating.WeakerThanHumans,
			1f
		},
		{
			StrengthRating.LikeHumans,
			2f
		},
		{
			StrengthRating.StrongerThanHumans,
			3f
		},
		{
			StrengthRating.VeryStrong,
			4f
		}
	};

	public int PopulationCap = 25;

	public double DefaultWorldRadius = 6371.0;

	public float AverageOverlandSpeedOnFoot = 40f;

	public float ExpertSkillLevel = 0.9f;

	public float UpdateIntervalForEntityComponents = 5f;

	public float UpdateIntervalForBioEntity = 3f;

	public float UpdateIntervalForNonLivingTypes = 10f;

	public float VisualCommunicationRangeInKms = 13f;

	public float DefaultSpawnRadius = 240f;

	public float SellPriceModifier = 1.5f;

	public void Initialize()
	{
	}

	public void PostDataCompleteInitialize()
	{
	}
}
