using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public abstract class AttackJob : Job
{
	public EntityID? Target;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override bool RequiresBoldStance => true;

	public AttackJob(EntityGroup entityGroup, bool addToJobsGroupNow = true)
		: base(entityGroup, addToJobsGroupNow)
	{
	}

	public AttackJob()
	{
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapon(Entity entity, Intelligence entityIntelligence, WeaponInstanceCombo combo, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, double? fitnessScore, float priority, float? energyLevelFactor)
	{
		rating = 0.0;
		if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(combo.AttackData.JobData.Job.Target.Value, out var data)))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		if (this is HuntingJob)
		{
			if (ScoreHuntSuccessEstimate(entity, combo.AttackData.AttackType, data) <= 0.0)
			{
				EvaluateAttackJobs.SetHuntingJobNotFeasible(this, value: true);
				return GoalEvaluator.CalculateResult.Done;
			}
			EvaluateAttackJobs.SetHuntingJobNotFeasible(this, value: false);
			if (GoalHunt.TargetIsTooFarFromExpedition(entity, data.PlaySiteLocation, 100f))
			{
				EvaluateAttackJobs.SetJobTooFarFromExpedition(this, value: true);
				return GoalEvaluator.CalculateResult.Done;
			}
			EvaluateAttackJobs.SetJobTooFarFromExpedition(this, value: false);
		}
		if (!combo.AttackData.JobData.AttackDirection.HasValue)
		{
			combo.AttackData.JobData.AttackDirection = GoalDoAttack.GetAttackDirection(entity, data.PlaySiteLocation, data.Rotation);
		}
		double retaliationScore = ScoreRetaliation(entity, data);
		double threatScore = 0.0;
		if (this is ThreatJob threatJob)
		{
			CombatAreaJob patrolJob = null;
			if (threatJob.IsVermin && !entityIntelligence.CanAttackVermin(out patrolJob))
			{
				return GoalEvaluator.CalculateResult.Done;
			}
			if (patrolJob != null && !patrolJob.CanAttackTargetsOutsideZone && !patrolJob.Zone.MapArea.BoundingRectangle.Value.Contains(data.MapPosition.Value) && Common.DistanceOctile(entity.PlaySiteLocation, data.PlaySiteLocation) > GameData.Instance.AIConstants.MaxDistanceOutsidePatrolZoneToChaseTargets)
			{
				return GoalEvaluator.CalculateResult.Done;
			}
			threatScore = ScoreThreatRating(threatJob);
		}
		double damageAndChanceToHitScore = ScoreAttackDamageAndChanceToHit(entity, combo, data, energyLevelFactor.Value, combo.AttackData.EstimatedDamageScore);
		double num = ScoreInertia(entity, data);
		if (num == 0.0)
		{
			num = entity.Intelligence.Memory.GetRecentlyFoundPreyScore(data.EntityID);
		}
		GoalEvaluator.WeightedRating WeightedRating = default(GoalEvaluator.WeightedRating);
		if (this is HuntingJob)
		{
			ScoreHunting(entity, combo, ref rating, data, retaliationScore, threatScore, damageAndChanceToHitScore, num, ref WeightedRating);
		}
		else
		{
			ScoreAttack(entity, combo, ref rating, data, retaliationScore, threatScore, damageAndChanceToHitScore, num, ref WeightedRating);
		}
		rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		return GoalEvaluator.CalculateResult.Done;
	}

	public override Vector3? GetCircaLocation()
	{
		if (Target.HasValue && ResolveOwner(out var owner) && !GoalEvaluator.EntityDataResultCausesSkip(owner.GetAllegiance().SharedKnowledge.GetKnownData(Target.Value, out var data)))
		{
			return data.Location;
		}
		return null;
	}

	private void ScoreAttack(Entity entity, WeaponInstanceCombo combo, ref double rating, IKnownEntityData targetData, double retaliationScore, double threatScore, double damageAndChanceToHitScore, double inertiaScore, ref GoalEvaluator.WeightedRating WeightedRating)
	{
		switch (combo.AttackData.AttackType.RangeType)
		{
		case AttackType.RangeTypes.Melee:
		{
			double score4 = ScoreAdjacencyToTarget(entity, targetData);
			WeightedRating.AddScore(0.23, inertiaScore);
			WeightedRating.AddScore(0.28, score4);
			WeightedRating.AddScore(0.23, retaliationScore);
			WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
			WeightedRating.AddScore(0.13, threatScore);
			break;
		}
		case AttackType.RangeTypes.Ray:
		case AttackType.RangeTypes.Ballistic:
		case AttackType.RangeTypes.Rocket:
		{
			double score = ScoreStationaryTarget(targetData);
			double score2 = ScoreRange(entity, targetData, combo.AttackData.AttackType);
			double score3 = ScoreInFieldOfView(entity, targetData);
			WeightedRating.AddScore(0.23, inertiaScore);
			WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
			WeightedRating.AddScore(0.23, score);
			WeightedRating.AddScore(0.23, score2);
			WeightedRating.AddScore(0.18, score3);
			break;
		}
		}
		rating = WeightedRating.Result;
	}

	private void ScoreHunting(Entity entity, WeaponInstanceCombo combo, ref double rating, IKnownEntityData targetData, double retaliationScore, double threatScore, double damageAndChanceToHitScore, double inertiaScore, ref GoalEvaluator.WeightedRating WeightedRating)
	{
		switch (combo.AttackData.AttackType.RangeType)
		{
		case AttackType.RangeTypes.Melee:
		{
			double score4 = ScoreAdjacencyToTarget(entity, targetData);
			if (!Common.IsZero(inertiaScore))
			{
				WeightedRating.AddScore(0.55, inertiaScore);
				WeightedRating.AddScore(0.3, score4);
				WeightedRating.AddScore(0.15, damageAndChanceToHitScore);
			}
			else
			{
				WeightedRating.AddScore(0.23, inertiaScore);
				WeightedRating.AddScore(0.28, score4);
				WeightedRating.AddScore(0.23, retaliationScore);
				WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
				WeightedRating.AddScore(0.13, threatScore);
			}
			break;
		}
		case AttackType.RangeTypes.Ray:
		case AttackType.RangeTypes.Ballistic:
		case AttackType.RangeTypes.Rocket:
		{
			double score = ScoreStationaryTarget(targetData);
			double score2 = ScoreRange(entity, targetData, combo.AttackData.AttackType);
			double score3 = ScoreInFieldOfView(entity, targetData);
			if (!Common.IsZero(inertiaScore))
			{
				WeightedRating.AddScore(0.8, inertiaScore);
				WeightedRating.AddScore(0.05, damageAndChanceToHitScore);
				WeightedRating.AddScore(0.05, score);
				WeightedRating.AddScore(0.05, score2);
				WeightedRating.AddScore(0.05, score3);
			}
			else
			{
				WeightedRating.AddScore(0.23, inertiaScore);
				WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
				WeightedRating.AddScore(0.23, score);
				WeightedRating.AddScore(0.23, score2);
				WeightedRating.AddScore(0.18, score3);
			}
			break;
		}
		}
		rating = WeightedRating.Result;
	}

	public GoalEvaluator.CalculateResult ScoreWeapon(Entity entity, IKnownEntityData weapon, double locationScore, double? ageContribution, double? timeContribution, float priority, AttackType attackType, out double score)
	{
		score = 0.0;
		double num = ScoreWeaponCondition(weapon);
		if (Common.IsZero(num) || Common.IsZero(locationScore))
		{
			score = 0.0;
		}
		else
		{
			score = 0.699999988079071 * locationScore + 0.30000001192092896 * num;
		}
		if (Common.IsZero(GetWeaponPolicyScore(entity, weapon, attackType)))
		{
			score = 0.0;
			return GoalEvaluator.CalculateResult.Done;
		}
		if (locationScore == 0.0)
		{
			score = 0.0;
			return GoalEvaluator.CalculateResult.Done;
		}
		score = 0.699999988079071 * locationScore + 0.30000001192092896 * num;
		if (Common.IsZero(score))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		score = GoalEvaluator.AddTimeAgeAndPriority(score, timeContribution.Value, ageContribution.Value, priority);
		return GoalEvaluator.CalculateResult.Done;
	}

	public virtual double GetWeaponPolicyScore(Entity entity, IKnownEntityData weapon, AttackType attackType)
	{
		return 1.0;
	}

	private double ScoreWeaponCondition(IKnownEntityData weapon)
	{
		return weapon.Condition.Value;
	}

	public static float ComputeEstimatedDamageScore(AttackType attackType, BodyPartType bodyPart, float bodyHitpoints, out float meanDamage)
	{
		attackType.ComputeDamage(bodyPart, out var damage, out var resistance, out var reductionConstant, out var armorLayer, doAttackRoll: false, 1f, attackType.DamageMean);
		float damageDone = BodyPart.GetDamageDone(damage, bodyPart.HitpointsFraction * bodyHitpoints);
		meanDamage = damageDone;
		if (damage == 0f)
		{
			attackType.ComputeDamage(bodyPart, out damage, out resistance, out reductionConstant, out armorLayer, doAttackRoll: false, 1f, attackType.DamageMean + GameData.Instance.AIConstants.UpperRangeOfEffectiveDamageInStandardDeviations * attackType.DamageStandardDeviation);
			damage = 0.1f * damage;
			damage = Common.ClampBottom(damage, 0f);
		}
		float num = 1f;
		if (bodyPart.IsVital())
		{
			num = 2f;
		}
		damage *= num;
		damage *= attackType.AccuracyFactor;
		float num2 = GameData.Instance.OneOverMeanDamageFromHumanPunch * damage;
		num2 = 0.3f * num2;
		return Common.Clamp(num2, 0f, 1f);
	}

	public static float ScoreAttackDamageAndChanceToHit(Entity attacker, WeaponInstanceCombo combo, IKnownEntityData targetData, float energyLevelFactor, float estimatedDamageScore)
	{
		return GoalDoAttack.ComputeChanceToHit(attacker, targetData, combo.AttackData.bodyPartID, combo.AttackData.JobData.AttackDirection.Value, combo.AttackData.AttackType) * estimatedDamageScore;
	}

	public static double ScoreHuntSuccessEstimate(Entity attacker, AttackType attackType, IKnownEntityData targetData)
	{
		if (attacker.Intelligence.Allegiance.HumanActivities != null && !attacker.Intelligence.Allegiance.HumanActivities.HasChasedHuntedCritter.TryGetValue(targetData.EntityType, out var _))
		{
			return 1.0;
		}
		if (targetData.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed < attacker.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed)
		{
			return 1.0;
		}
		if (attackType.RangeType != AttackType.RangeTypes.Melee)
		{
			return 1.0;
		}
		bool flag = false;
		if (targetData is Entity entity && entity.CanSeeEntity(attacker))
		{
			flag = true;
		}
		if (flag)
		{
			return 0.0;
		}
		return 1.0;
	}

	public double CombineJobAndWeaponScore(double jobScore, double? totalweaponsScore)
	{
		if (!totalweaponsScore.HasValue)
		{
			return 0.9 * jobScore;
		}
		if (totalweaponsScore.Value == 0.0)
		{
			return 0.0;
		}
		return 0.9 * jobScore + 0.1 * totalweaponsScore.Value;
	}

	private double ScoreDistribution(AttackJob threatJob)
	{
		return 0.0;
	}

	private double ScoreInFieldOfView(Entity entity, IKnownEntityData targetData)
	{
		if (Vector3.Dot(entity.FacingNormal, targetData.PlaySiteLocation - entity.PlaySiteLocation) > 0f)
		{
			return 1.0;
		}
		return 0.0;
	}

	private double ScoreMovingOutOfRange()
	{
		return 0.0;
	}

	private double ScoreClosestTarget()
	{
		return 0.0;
	}

	private double ScoreMinimalCollateralDamage()
	{
		return 0.0;
	}

	private double ScoreStationaryTarget(IKnownEntityData targetData)
	{
		if (targetData is Entity entity && entity.Locomotor.IsMoving())
		{
			return 0.0;
		}
		return 1.0;
	}

	private double ScoreTimeToKillTarget()
	{
		return 0.0;
	}

	private double ScoreRetaliation(Entity attacker, IKnownEntityData targetData)
	{
		if (targetData is Entity entity && entity.Intelligence.CombatInfo.Target == attacker.EntityID)
		{
			return 1.0;
		}
		return 0.0;
	}

	private double ScoreInertia(Entity attacker, IKnownEntityData targetData)
	{
		return attacker.Intelligence.Memory.GetLastAttackScore(targetData.EntityID);
	}

	public static bool IsCorrectMeleeDistanceRoundedToSubtiles(Entity attacker, Entity target)
	{
		Vector3 pos;
		if (target.Locomotor.IsMoving())
		{
			Vector3 vector = target.Locomotor.MoveSpeed * target.FacingNormal * 0.5f;
			pos = target.PlaySiteLocation + vector;
		}
		else
		{
			pos = target.PlaySiteLocation;
		}
		Point p = MapManager.WorldPosToSubtile(attacker.PlaySiteLocation);
		Point p2 = MapManager.WorldPosToSubtile(pos);
		if (Math.Abs(Common.DistanceOctile(p, p2) * 16f - (attacker.EntityType.LocomotorType.MeleeRadius + target.EntityType.LocomotorType.MeleeRadius)) <= GameData.Instance.AIConstants.Combat.DistanceToleranceInMeleeCombat)
		{
			return true;
		}
		return false;
	}

	public static bool IsCorrectMeleeDistance(float distance, float correctMeleeDistance)
	{
		if (Math.Abs(distance - correctMeleeDistance) <= GameData.Instance.AIConstants.Combat.DistanceToleranceInMeleeCombat)
		{
			return true;
		}
		return false;
	}

	private double ScoreAdjacencyToTarget(Entity entity, IKnownEntityData targetData)
	{
		if (targetData is Entity target && IsCorrectMeleeDistanceRoundedToSubtiles(entity, target))
		{
			return 1.0;
		}
		return 0.0;
	}

	private double ScoreRange(Entity entity, IKnownEntityData targetData, AttackType attackType)
	{
		double num = (entity.PlaySiteLocation - targetData.PlaySiteLocation).LengthSquared();
		if (num > (double)attackType.MaxRangeSquared.Value)
		{
			return 0.5;
		}
		if (attackType.MinRange.HasValue && num < (double)attackType.MinRange.Value)
		{
			return 0.0;
		}
		return 1.0;
	}

	private double ScoreThreatRating(ThreatJob threatJob)
	{
		return threatJob.ThreatRating;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Target = sn.DoEnumNullable(Target);
		return this;
	}
}
