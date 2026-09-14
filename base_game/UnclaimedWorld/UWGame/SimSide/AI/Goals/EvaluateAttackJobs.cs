using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateAttackJobs : GoalEvaluator, IScoreJob
{
	private enum Progress
	{
		NotStarted,
		GetCombos,
		ScoreCombos,
		FindFinalCombo
	}

	public enum JobTypes
	{
		Hunt,
		AssetThreat,
		Threat
	}

	private float priority;

	private Progress progress;

	private Dictionary<Job, List<WeaponInstanceCombo>> currentAttackCombos = new Dictionary<Job, List<WeaponInstanceCombo>>();

	private Allegiance allegiance;

	private EntityGroup huntingJobs;

	private List<EntityGroup> ownersOfWeapons;

	private List<EntityGroup> ownersOfVehicles;

	private OwnerID? ownerOfCarcass;

	private List<AttackType> AvailableAttackTypes;

	private List<WeaponInstanceCombo> allCombos = new List<WeaponInstanceCombo>();

	private int scoreComboIndex;

	private int finalComboSelectionIndex;

	public Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates = new Dictionary<AttackType, ReplenishWeaponStatus>();

	private List<Entity> needsToBeCancelled = new List<Entity>();

	private List<Entity> itemsToBeDropped = new List<Entity>();

	private Dictionary<EntityType, List<ItemDistance>> replenishItemsSortedByDistanceToEntity = new Dictionary<EntityType, List<ItemDistance>>();

	private List<WeaponInstanceCombo> jobAttackCombosToSelectFrom = new List<WeaponInstanceCombo>();

	private int noOfCombosToSelectFrom = 10;

	private JobTypes jobTypes;

	private WeaponInstanceCombo bestCombo;

	private List<EntityID> tempIntrinsicWeaponsList = new List<EntityID>();

	private Dictionary<Job, WeaponInstanceComboJobData> jobData = new Dictionary<Job, WeaponInstanceComboJobData>();

	private Dictionary<Job, WeaponInstanceComboAttackData> attackData = new Dictionary<Job, WeaponInstanceComboAttackData>();

	private WeaponInstanceCombo combo;

	private WeaponInstanceCombo bestCarriedCombo;

	private List<WeaponInstanceCombo> carriedCombos = new List<WeaponInstanceCombo>();

	private WeaponInstanceCombo selectedCombo;

	private List<ReplenishItemsForAction> replenishItemsForWeapon;

	private int comboProgress;

	private bool scoringWasInterrupted;

	public override float Priority => priority;

	public EvaluateAttackJobs(Entity entity, JobTypes jobTypes, Allegiance allegiance, EntityGroup huntingJobs, OwnerID? ownerOfCarcass, List<EntityGroup> weaponsGroups = null, List<EntityGroup> vehiclesGroups = null)
		: base(entity)
	{
		this.allegiance = allegiance;
		this.huntingJobs = huntingJobs;
		this.jobTypes = jobTypes;
		if (this.jobTypes == JobTypes.Threat)
		{
			priority = GameData.Instance.AIConstants.PriorityOfThreatJobs;
			noOfCombosToSelectFrom = 10;
		}
		else if (this.jobTypes == JobTypes.AssetThreat)
		{
			priority = GameData.Instance.AIConstants.PriorityOfAssetThreatJobs;
			noOfCombosToSelectFrom = 10;
		}
		else
		{
			priority = GameData.Instance.AIConstants.PriorityOfWorkJobs;
			noOfCombosToSelectFrom = 1;
			if (this.huntingJobs == null)
			{
				throw new Exception();
			}
		}
		this.ownerOfCarcass = ownerOfCarcass;
		ownersOfWeapons = weaponsGroups;
		ownersOfVehicles = vehiclesGroups;
	}

	public static void SetHuntingJobNotFeasible(Job job, bool value)
	{
		job.ResolveOwner(out var owner);
		if (owner != null)
		{
			The.Client.SetHuntingJobNotFeasible(job, owner.Parent, value);
		}
	}

	public static void SetJobTooFarFromExpedition(Job job, bool value)
	{
		job.ResolveOwner(out var owner);
		if (owner != null)
		{
			The.Client.SetJobTooFarFromExpedition(job, owner.Parent, value);
		}
	}

	public static void SetJobInaccessibleDueToBoldStance(Job job, bool IsBlocked)
	{
		job.ResolveOwner(out var owner);
		if (owner != null)
		{
			The.Client.SetJobBlockedByBoldStance(job, owner.Parent, IsBlocked);
		}
	}

	private void SetHuntingJobsAccessibility(bool isNotAccessible)
	{
		if (huntingJobs == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in huntingJobs.ProductionJobs)
		{
			foreach (ProcessJob item in productionJob.Value)
			{
				SetJobInaccessibleDueToBoldStance(item, isNotAccessible);
			}
		}
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		bestCombo = null;
		if (progress == Progress.NotStarted)
		{
			bestCarriedCombo = null;
			carriedCombos.Clear();
			if (!entityIntelligence.StanceCanBeBold())
			{
				SetHuntingJobsAccessibility(isNotAccessible: true);
				result = 0.0;
				return CalculateResult.Done;
			}
			SetHuntingJobsAccessibility(isNotAccessible: false);
			bestScore = minimumRatingToConsider;
			currentAttackCombos.Clear();
			if (entity.Find<BiologicalEntity>(out var c) && ScoreAge(c) == 0.0)
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			ScoreTimeOfDay();
			progress = Progress.GetCombos;
		}
		if (progress == Progress.GetCombos)
		{
			GetAllJobCombos();
		}
		if (progress == Progress.ScoreCombos && ScoreAllCombos(minimumRatingToConsider) == CalculateResult.Processing)
		{
			return CalculateResult.Processing;
		}
		if (progress == Progress.FindFinalCombo)
		{
			if (FindBestCombo(minimumRatingToConsider) != CalculateResult.Done)
			{
				return CalculateResult.Processing;
			}
			if (jobAttackCombosToSelectFrom.Count > 0)
			{
				bestScore = jobAttackCombosToSelectFrom[0].Score;
				// MOD: how hard an unordered fight argues against eating, sleeping and working.
				// Applied to the finished score rather than anywhere inside the scoring, so every
				// factor the game weighs still decides WHO the best candidate is; this decides
				// only whether that candidate would rather be doing something else.
				bestScore = UWGame.Mods.SelfPreservationMod.AdjustThreatDesirability(
					entity, jobAttackCombosToSelectFrom[0], jobTypes == JobTypes.AssetThreat, bestScore);
				result = bestScore;
				bestCombo = jobAttackCombosToSelectFrom[0];
				entityIntelligence.TopScoringJobs.Add(new GoalAndScore
				{
					Score = bestScore,
					Goal = jobAttackCombosToSelectFrom[0].AttackData.JobData.Job.ToString()
				});
				return CalculateResult.Done;
			}
		}
		result = 0.0;
		return CalculateResult.Done;
	}

	private void GetAllJobCombos()
	{
		scoreComboIndex = 0;
		allCombos.Clear();
		if (jobTypes == JobTypes.Threat || jobTypes == JobTypes.AssetThreat)
		{
			List<Job> list = null;
			if (jobTypes == JobTypes.Threat)
			{
				list = allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs;
			}
			else if (jobTypes == JobTypes.AssetThreat)
			{
				list = allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobs;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				AttackJob attackJob = list[num] as AttackJob;
				// MOD: an animal of the colony with no person already on the job does not go
				// looking for a fight. People are not filtered here - they are made reluctant
				// where the score is decided, in CalculateDesirability below.
				if (UWGame.Mods.SelfPreservationMod.DeclinesThreat(entity, attackJob, jobTypes == JobTypes.AssetThreat))
				{
					continue;
				}
				if (!entityIntelligence.Brain.IsSame(attackJob))
				{
					if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(attackJob.Target.Value, out var data)))
					{
						attackJob.Destroy(cancelTakers: true);
					}
					else
					{
						WeaponInstanceComboJobData weaponInstanceComboJobData = new WeaponInstanceComboJobData();
						weaponInstanceComboJobData.Job = attackJob;
						GetAllWeaponInstanceCombos(attackJob, weaponInstanceComboJobData, data, allCombos);
					}
				}
			}
		}
		else
		{
			for (int num2 = huntingJobs.OtherJobs.Count - 1; num2 >= 0; num2--)
			{
				if (huntingJobs.OtherJobs[num2] is HuntingJob huntingJob && !entityIntelligence.Brain.IsSame(huntingJob))
				{
					IKnownEntityData data2 = null;
					if (!huntingJob.Target.HasValue || GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(huntingJob.Target.Value, out data2)))
					{
						huntingJob.Destroy(cancelTakers: true);
					}
					else
					{
						WeaponInstanceComboJobData weaponInstanceComboJobData2 = new WeaponInstanceComboJobData();
						weaponInstanceComboJobData2.Job = huntingJob;
						GetAllWeaponInstanceCombos(huntingJob, weaponInstanceComboJobData2, data2, allCombos);
					}
				}
			}
		}
		progress = Progress.ScoreCombos;
	}

	public void GetAllWeaponInstanceCombos(AttackJob job, WeaponInstanceComboJobData jobData, IKnownEntityData targetData, List<WeaponInstanceCombo> allCombos)
	{
		entity.Find<BodyComponent>(out var c);
		Body body = targetData.Body;
		if (entity.EntityType.IntelligenceType.CanUseWeapons != false && ownersOfWeapons != null)
		{
			foreach (EntityGroup ownersOfWeapon in ownersOfWeapons)
			{
				foreach (KeyValuePair<AttackType, List<EntityID>> item in ownersOfWeapon.WeaponsByAttackType)
				{
					if (item.Value.Count > 0)
					{
						GetCombosForAttackType(entity, jobData, c.Body, body, item.Key, item.Value, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
					}
				}
			}
		}
		if (entity.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
		{
			foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in entityIntelligence.IntrinsicWeapons)
			{
				AttackType[] attackTypes = intrinsicWeapon.Key.ItemType.WeaponType.AttackTypes;
				foreach (AttackType attackType in attackTypes)
				{
					tempIntrinsicWeaponsList.Add(intrinsicWeapon.Value);
					GetCombosForAttackType(entity, jobData, c.Body, body, attackType, tempIntrinsicWeaponsList, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
					tempIntrinsicWeaponsList.Clear();
				}
			}
		}
		if (entity.EntityType.IntelligenceType.AttackTypes != null)
		{
			foreach (AttackType attackType2 in entity.EntityType.IntelligenceType.AttackTypes)
			{
				GetCombosForAttackType(entity, jobData, c.Body, body, attackType2, null, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
			}
		}
		tempIntrinsicWeaponsList.Clear();
	}

	public static void GetAllWeaponInstanceCombosWithCarriedWeapons(Entity attacker, AttackJob job, WeaponInstanceComboJobData jobData, IKnownEntityData targetData, List<WeaponInstanceCombo> allCombos)
	{
		attacker.Find<BodyComponent>(out var c);
		Body body = targetData.Body;
		if (attacker.Contains != null)
		{
			List<Entity> containedItemsList = attacker.Contains.GetContainedItemsList((Entity e) => e.EntityType.ItemType != null && e.EntityType.ItemType.WeaponType != null);
			Dictionary<AttackType, List<EntityID>> dictionary = new Dictionary<AttackType, List<EntityID>>();
			foreach (Entity item in containedItemsList)
			{
				AttackType[] attackTypes = item.EntityType.ItemType.WeaponType.AttackTypes;
				foreach (AttackType key in attackTypes)
				{
					if (!dictionary.TryGetValue(key, out var value))
					{
						value = new List<EntityID>();
						dictionary.Add(key, value);
					}
					value.Add(item.EntityID);
				}
			}
			List<Entity> containedItemsList2 = attacker.Contains.GetContainedItemsList((Entity e) => e.EntityType.ItemType != null && e.EntityType.ItemType.AmmunitionType != null);
			Dictionary<AttackType, ReplenishWeaponStatus> dictionary2 = new Dictionary<AttackType, ReplenishWeaponStatus>();
			foreach (Entity item2 in containedItemsList)
			{
				AttackType[] attackTypes = item2.EntityType.ItemType.WeaponType.AttackTypes;
				foreach (AttackType attackType in attackTypes)
				{
					if (attackType.UsesAmmo != null && !dictionary2.TryGetValue(attackType, out var value2))
					{
						value2 = default(ReplenishWeaponStatus);
						int num2 = RoundsAvailableForAttack(containedItemsList2.FindAll((Entity i) => i.EntityType == attackType.UsesAmmoType));
						value2.RoundsAvailable = num2;
						value2.OwnsAmmo = num2 > 0;
						dictionary2[attackType] = value2;
					}
				}
			}
			foreach (KeyValuePair<AttackType, List<EntityID>> item3 in dictionary)
			{
				GetCombosForAttackType(attacker, jobData, c.Body, body, item3.Key, item3.Value, allCombos, null, dictionary2);
			}
		}
		foreach (AttackType attackType2 in attacker.EntityType.IntelligenceType.AttackTypes)
		{
			GetCombosForAttackType(attacker, jobData, c.Body, body, attackType2, null, allCombos, null, null);
		}
	}

	public static int RoundsAvailableForAttack(List<Entity> ammoItemsForAttack)
	{
		return ammoItemsForAttack.Sum((Entity i) => i.Item.Ammunition.NoOfRounds);
	}

	private static void GetCombosForAttackType(Entity attacker, WeaponInstanceComboJobData jobData, Body attackerBody, Body targetBody, AttackType attackType, List<EntityID> weapons, List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
	{
		if (!IntelligenceType.AttackTypeIsFunctional(attackerBody, attackType))
		{
			return;
		}
		Dictionary<BodyPartType, float> estimatedDamageOfAttackType = GameData.Instance.AttackScoresAgainstBodyParts[attackType];
		foreach (BodyPart bodyPart in targetBody.BodyParts)
		{
			GetAllWeaponInstanceCombosAgainstBodyParts(attacker, jobData, attackType, bodyPart, weapons, estimatedDamageOfAttackType, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
		}
	}

	private static void GetAllWeaponInstanceCombosAgainstThisBodyPart(Entity attacker, WeaponInstanceComboJobData jobData, AttackType attackType, BodyPart bodyPart, WeaponInstanceComboAttackData attackData, List<EntityID> weapons, List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
	{
		if (!bodyPart.IsFunctional())
		{
			return;
		}
		Intelligence intelligence = attacker.Intelligence;
		WeaponInstanceCombo item;
		if (weapons != null)
		{
			foreach (EntityID weapon in weapons)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(intelligence.GetKnownData(weapon, out var data)) && IsValidPlaysiteWeapon(attacker, data, attackType, ownersOfWeapons, cachedWeaponReplenishStates))
				{
					item = new WeaponInstanceCombo
					{
						Weapon = data,
						AttackData = attackData
					};
					allCombos.Add(item);
				}
			}
			return;
		}
		item = new WeaponInstanceCombo
		{
			Weapon = null,
			AttackData = attackData
		};
		allCombos.Add(item);
	}

	private static void GetAllWeaponInstanceCombosAgainstBodyParts(Entity attacker, WeaponInstanceComboJobData jobData, AttackType attackType, BodyPart bodyPart, List<EntityID> weapons, Dictionary<BodyPartType, float> estimatedDamageOfAttackType, List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
	{
		if (!IsRangedAttackAgainsNonVitalBodyPart(attackType, bodyPart.BodyPartType) && estimatedDamageOfAttackType.TryGetValue(bodyPart.BodyPartType, out var value))
		{
			WeaponInstanceComboAttackData weaponInstanceComboAttackData = new WeaponInstanceComboAttackData();
			weaponInstanceComboAttackData.bodyPartID = bodyPart.BodyPartID;
			weaponInstanceComboAttackData.AttackType = attackType;
			weaponInstanceComboAttackData.JobData = jobData;
			weaponInstanceComboAttackData.EstimatedDamageScore = value;
			GetAllWeaponInstanceCombosAgainstThisBodyPart(attacker, jobData, attackType, bodyPart, weaponInstanceComboAttackData, weapons, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
		}
		if (bodyPart.BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart2 in bodyPart.BodyParts)
		{
			GetAllWeaponInstanceCombosAgainstBodyParts(attacker, jobData, attackType, bodyPart2, weapons, estimatedDamageOfAttackType, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
		}
	}

	private static bool IsRangedAttackAgainsNonVitalBodyPart(AttackType attackType, BodyPartType bodyPartType)
	{
		if (attackType.RangeType != AttackType.RangeTypes.Melee && !bodyPartType.IsVital())
		{
			return true;
		}
		return false;
	}

	private CalculateResult ScoreAllCombos(double minimumRatingToConsider)
	{
		double score = -1.0;
		RegionMap regionMap = null;
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			regionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
		}
		double ageContribution = GetAgeContribution();
		double value = ScoreTimeOfDay();
		float value2 = ScoreFitness(entity);
		float energyLevelFactorOnDamageForMelee = GoalDoAttack.GetEnergyLevelFactorOnDamageForMelee(entity);
		if (allCombos.Count <= 0 || entity.Name == null || entity.Name.Contains("coyd"))
		{
		}
		while (scoreComboIndex < allCombos.Count)
		{
			WeaponInstanceCombo weaponInstanceCombo = allCombos[scoreComboIndex];
			if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, weaponInstanceCombo, cachedWeaponReplenishStates))
			{
				int proposedNumberOfWorkers = Common.Clamp(weaponInstanceCombo.AttackData.JobData.Job.TakenBy.Count + 1, 0, weaponInstanceCombo.AttackData.JobData.Job.MaxJobPositions);
				if (ScoreThisAttackJob(regionMap, entity, proposedNumberOfWorkers, ageContribution, value, value2, energyLevelFactorOnDamageForMelee, weaponInstanceCombo, out score) != CalculateResult.Done)
				{
					scoringWasInterrupted = true;
					return CalculateResult.Processing;
				}
				weaponInstanceCombo.Score = (float)score;
				if (weaponInstanceCombo.Weapon != null && entity.AgentStorage != null && entity.AgentStorage.Contains(weaponInstanceCombo.Weapon.EntityID))
				{
					if (bestCarriedCombo == null || weaponInstanceCombo.AttackData.AttackScore > bestCarriedCombo.AttackData.AttackScore)
					{
						bestCarriedCombo = weaponInstanceCombo;
					}
					carriedCombos.Add(weaponInstanceCombo);
				}
			}
			else
			{
				weaponInstanceCombo.Score = 0f;
			}
			scoreComboIndex++;
		}
		scoreComboIndex = 0;
		jobAttackCombosToSelectFrom.Clear();
		cachedWeaponReplenishStates.Clear();
		progress = Progress.FindFinalCombo;
		return CalculateResult.Done;
	}

	private CalculateResult FindBestCombo(double minimumRatingToConsider)
	{
		combo = null;
		allCombos.RemoveAll((WeaponInstanceCombo c) => c.Score == 0f);
		allCombos.Sort((WeaponInstanceCombo a, WeaponInstanceCombo b) => b.Score.CompareTo(a.Score));
		if (allCombos.Count > 0 && entity.Name != null)
		{
			entity.Name.Contains("coyd");
		}
		AttackJob attackJob = null;
		IKnownEntityData effectiveWeaponInHand = null;
		for (; finalComboSelectionIndex < allCombos.Count; finalComboSelectionIndex++)
		{
			combo = allCombos[finalComboSelectionIndex];
			if ((attackJob != null && combo.AttackData.JobData.Job != attackJob) || (scoringWasInterrupted && !IsComboValid(entity, entityIntelligence, combo, cachedWeaponReplenishStates)))
			{
				continue;
			}
			if ((double)combo.Score < minimumRatingToConsider)
			{
				break;
			}
			if (attackJob == null)
			{
				attackJob = combo.AttackData.JobData.Job;
			}
			GetJobsToCancel(combo);
			double ourScore = combo.Score;
			if (CanCancelAndForceDropItems(ourScore))
			{
				if (SkipThisCombo(combo))
				{
					continue;
				}
				if (combo.Weapon != null)
				{
					SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
					RegionMap footRegionMap = null;
					if (entity.EntityType.IntelligenceType.IsMobile)
					{
						footRegionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
					}
					if (FindReplenishItemsForToolOrWeapon(entity, null, ownersOfWeapons, ref combo.ReplenishItemsForWeapon, ourScore, sharedKnowledge, footRegionMap, combo.Weapon, replenishItemsSortedByDistanceToEntity, out var success, combo.AttackData.AttackType.UsesAmmoType, combo.AttackData.AttackType.RoundsToSpend) == CalculateResult.Processing)
					{
						return CalculateResult.Processing;
					}
					if (!success)
					{
						continue;
					}
					GetReplenishItemsToCancel(combo.ReplenishItemsForWeapon, ref needsToBeCancelled, ref itemsToBeDropped);
					jobAttackCombosToSelectFrom.Add(combo);
					if (entity.AgentStorage != null && entity.AgentStorage.MountedToolOrWeapon == combo.Weapon.EntityID)
					{
						effectiveWeaponInHand = combo.Weapon;
					}
				}
				else
				{
					if (IsUnarmedAttackUsingBodypartWithEffectiveWeapon(effectiveWeaponInHand, combo))
					{
						continue;
					}
					jobAttackCombosToSelectFrom.Add(combo);
				}
			}
			if (jobAttackCombosToSelectFrom.Count >= noOfCombosToSelectFrom)
			{
				break;
			}
		}
		finalComboSelectionIndex = 0;
		replenishItemsSortedByDistanceToEntity.Clear();
		progress = Progress.NotStarted;
		return CalculateResult.Done;
	}

	private bool SkipThisCombo(WeaponInstanceCombo combo)
	{
		if (!UncarriedWeaponIsBetterThanCarriedWeapons(combo))
		{
			return true;
		}
		if (ComboNotInRangeAndGotComboInRangeEquipped(combo))
		{
			return true;
		}
		return false;
	}

	private bool ComboNotInRangeAndGotComboInRangeEquipped(WeaponInstanceCombo combo)
	{
		IKnownEntityData data = null;
		if (entity.Intelligence.GetKnownData(combo.AttackData.JobData.Job.Target.Value, out data) == EntityResult.Destroyed || data == null)
		{
			return false;
		}
		float num = Common.DistanceOctile(entity.PlaySiteLocation, data.PlaySiteLocation);
		if (num > 48f)
		{
			return false;
		}
		float? maxRange = combo.AttackData.AttackType.MaxRange;
		if (maxRange.HasValue && maxRange > num)
		{
			return false;
		}
		foreach (WeaponInstanceCombo carriedCombo in carriedCombos)
		{
			if (carriedCombo.AttackData.AttackScore > combo.AttackData.AttackScore)
			{
				float? maxRange2 = carriedCombo.AttackData.AttackType.MaxRange;
				if (maxRange2.HasValue && maxRange2.Value > num)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsUnarmedAttackUsingBodypartWithEffectiveWeapon(IKnownEntityData effectiveWeaponInHand, WeaponInstanceCombo combo)
	{
		if (effectiveWeaponInHand != null && combo.AttackData.AttackType.DependsOn.Any((BodyPartType p) => p.Name == effectiveWeaponInHand.EntityType.ItemType.AttachesToBodyPart))
		{
			return true;
		}
		return false;
	}

	private bool WeaponIsCarried(IKnownEntityData weapon)
	{
		return entity.AgentStorage.Contains(weapon.EntityID);
	}

	private bool WeCarrySameOrBetterWeaponType(IKnownEntityData weapon)
	{
		if (WeaponIsSameOrBetterType(entity.AgentStorage.MountedToolOrWeapon, weapon))
		{
			return true;
		}
		return false;
	}

	public bool UncarriedWeaponIsBetterThanCarriedWeapons(WeaponInstanceCombo weaponCombo)
	{
		if (combo.Weapon != null && entity.AgentStorage != null && !WeaponIsCarried(combo.Weapon))
		{
			bool carriesSameType = false;
			entity.AgentStorage.IterateContainedBreakOnTrue(delegate(Entity e)
			{
				if (e.EntityType == weaponCombo.Weapon.EntityType)
				{
					carriesSameType = true;
					return true;
				}
				return false;
			});
			if (carriesSameType)
			{
				return false;
			}
			if (bestCarriedCombo != null && weaponCombo.AttackData.AttackScore < bestCarriedCombo.AttackData.AttackScore)
			{
				return false;
			}
		}
		return true;
	}

	public bool WeaponIsSameOrBetterType(EntityID? carriedWeapon, IKnownEntityData otherWeapon)
	{
		if (!carriedWeapon.HasValue)
		{
			return false;
		}
		if (Entity.FindByID(carriedWeapon.Value).EntityType == otherWeapon.EntityType)
		{
			return true;
		}
		return false;
	}

	public override void PreSetGoal()
	{
		base.PreSetGoal();
		if (jobAttackCombosToSelectFrom.Count > 1)
		{
			Common.BuildEdgesFromBucketSizes(jobAttackCombosToSelectFrom, doSort: true, out var totalScore);
			Common.GetStairStepIndex(jobAttackCombosToSelectFrom, out var stairstep, The.Sim.GameplayRandomGenerator, totalScore);
			selectedCombo = jobAttackCombosToSelectFrom[stairstep];
		}
		else
		{
			selectedCombo = jobAttackCombosToSelectFrom[0];
		}
		List<Tuple<ProcessType, List<IKnownEntityData>>> value = null;
		if (selectedCombo.Weapon != null && selectedCombo.ReplenishItemsForWeapon != null && selectedCombo.ReplenishItemsForWeapon.TryGetValue(selectedCombo.Weapon, out value))
		{
			replenishItemsForWeapon = new List<ReplenishItemsForAction>();
			foreach (Tuple<ProcessType, List<IKnownEntityData>> item in value)
			{
				List<EntityID> list = new List<EntityID>();
				foreach (IKnownEntityData item2 in item.Item2)
				{
					list.Add(item2.EntityID);
				}
				replenishItemsForWeapon.Add(new ReplenishItemsForAction(item.Item1, list));
			}
		}
		if (selectedCombo.Weapon != null)
		{
			entityIntelligence.Memory.SetNeededItemForSwitchedGoal(selectedCombo.Weapon.EntityID);
		}
		if (replenishItemsForWeapon == null)
		{
			return;
		}
		foreach (ReplenishItemsForAction item3 in replenishItemsForWeapon)
		{
			foreach (EntityID item4 in item3.Items)
			{
				entityIntelligence.Memory.SetNeededItemForSwitchedGoal(item4);
			}
		}
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		if (selectedCombo.AttackData.JobData.Job is HuntingJob)
		{
			entityIntelligence.SetTopLevelGoal(new GoalHunt(entity, (HuntingJob)selectedCombo.AttackData.JobData.Job, GetOwnerIDs(ownersOfVehicles), ownerOfCarcass, selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID, (selectedCombo.Weapon != null) ? new EntityAndRoot?(selectedCombo.Weapon.GetAsEntityAndRoot()) : ((EntityAndRoot?)null), replenishItemsForWeapon)
			{
				GoalEvaluator = this
			}, selectedCombo.Score);
		}
		else
		{
			entityIntelligence.SetTopLevelGoal(new GoalAttack(entity, selectedCombo.AttackData.JobData.Job, GetOwnerIDs(ownersOfVehicles), null, selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID, (selectedCombo.Weapon != null) ? new EntityAndRoot?(selectedCombo.Weapon.GetAsEntityAndRoot()) : ((EntityAndRoot?)null), replenishItemsForWeapon)
			{
				GoalEvaluator = this
			}, selectedCombo.Score);
		}
		selectedCombo = null;
		replenishItemsForWeapon = null;
		return true;
	}

	private bool IsComboValid(Entity entity, Intelligence entityIntelligence, WeaponInstanceCombo combo, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
	{
		if (combo.AttackData.JobData.Job.ID == JobID.Invalid || (combo.Weapon != null && !IsValidPlaysiteWeapon(entity, combo.Weapon, combo.AttackData.AttackType, ownersOfWeapons, cachedWeaponReplenishStates)))
		{
			return false;
		}
		return true;
	}

	public static bool IsValidPlaysiteWeapon(Entity entity, IKnownEntityData weapon, AttackType attackType, List<EntityGroup> ownersOfWeapons, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates, bool doNotCheckAmmunitionAndBulk = false)
	{
		bool flag = true;
		bool itemIsInvalidOrDestroyed = false;
		bool flag2 = false;
		bool num = weapon.EntityType.IsIntrinsic();
		bool flag3 = false;
		if (num)
		{
			if (entity.IntrinsicWeapons == null || !entity.IntrinsicWeapons.ContainsValue(weapon.EntityID))
			{
				return false;
			}
			flag3 = true;
		}
		if (!flag3 && weapon.PartOfID.HasValue)
		{
			return false;
		}
		if (weapon.EntityType.IsMountable() && !entity.IsCarrying(weapon.EntityID))
		{
			flag2 = IsCarriedByEntityWhoIsOccupied(weapon, out itemIsInvalidOrDestroyed);
		}
		if (itemIsInvalidOrDestroyed)
		{
			return false;
		}
		bool isInUseByNonWorkerProcess = false;
		if (!EvaluateJob.IsInUseByNonWorkerProcess(weapon, out isInUseByNonWorkerProcess))
		{
			return false;
		}
		if (isInUseByNonWorkerProcess)
		{
			return false;
		}
		bool flag4 = true;
		if (!doNotCheckAmmunitionAndBulk)
		{
			float num2 = 0f;
			num2 = ((attackType.UsesAmmo == null) ? weapon.Bulk : (weapon.Bulk + attackType.UsesAmmoType.ItemType.MaximumBulk.Value));
			flag = flag3 || (!flag2 && weapon.NotOnboardDrivenVehicle && entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(num2));
			flag4 = attackType.UsesAmmo == null || HasAmmoForAttack(entity, weapon, attackType, null, ownersOfWeapons, cachedWeaponReplenishStates);
		}
		else
		{
			flag = flag3 || (!flag2 && weapon.NotOnboardDrivenVehicle);
		}
		if (flag && flag4 && GoalEvaluator.IsOnPlaySite(weapon) && Entity.IsFunctional(weapon))
		{
			return weapon.IsCompleted();
		}
		return false;
	}

	public static bool HasAmmoForAttack(Entity agent, IKnownEntityData weapon, AttackType attackType, EntityGroup ownerOfItems, List<EntityGroup> ownersOfItems, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
	{
		bool flag = true;
		if (weapon.EntityType.ContainerType != null && weapon.EntityType.ContainerType is MagazineContainerType)
		{
			flag = weapon.HasEnoughAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value);
			if (!flag)
			{
				if (!cachedWeaponReplenishStates.TryGetValue(attackType, out var value))
				{
					value = default(ReplenishWeaponStatus);
					if (agent.EntityType.IntelligenceType.CanReplenish == true)
					{
						int availableRounds;
						if (ownerOfItems != null)
						{
							value.OwnsAmmo = HasAmmoForAttack(attackType, ownerOfItems, out availableRounds);
							value.RoundsAvailable = availableRounds;
						}
						else if (ownersOfItems != null)
						{
							value.OwnsAmmo = HasAmmoForAttack(attackType, ownersOfItems, out availableRounds);
							value.RoundsAvailable = availableRounds;
						}
					}
					else
					{
						value.OwnsAmmo = false;
						value.RoundsAvailable = 0;
					}
					flag = value.OwnsAmmo.Value;
					cachedWeaponReplenishStates.Add(attackType, value);
				}
				else
				{
					flag = attackType.RoundsToSpend.Value <= value.RoundsAvailable.Value;
				}
			}
		}
		return flag;
	}

	private static bool HasAmmoForAttack(AttackType attackType, List<EntityGroup> ownersOfItems, out int availableRounds)
	{
		int num = 0;
		foreach (EntityGroup ownersOfItem in ownersOfItems)
		{
			HasAmmoForAttack(attackType, ownersOfItem, out var availableRounds2);
			num += availableRounds2;
		}
		availableRounds = num;
		return availableRounds >= attackType.RoundsToSpend.Value;
	}

	public static bool HasAmmoForAttack(AttackType attackType, EntityGroup ownerOfItems, out int availableRounds)
	{
		if (ownerOfItems.AmmoItems.TryGetValue(attackType.UsesAmmoType, out var value))
		{
			availableRounds = value.TotalRounds;
			return value.TotalRounds >= attackType.RoundsToSpend.Value;
		}
		availableRounds = 0;
		return false;
	}

	public static bool IsCarriedByEntityWhoIsOccupied(IKnownEntityData item, out bool itemIsInvalidOrDestroyed)
	{
		itemIsInvalidOrDestroyed = false;
		if (item is Entity entity)
		{
			if (!entity.CarriedByAgent(out var carrier))
			{
				itemIsInvalidOrDestroyed = true;
				return false;
			}
			if (carrier != null)
			{
				if (!carrier.IsAttacking())
				{
					return carrier.IsFleeing();
				}
				return true;
			}
		}
		return false;
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
	{
		WeaponInstanceCombo weaponInstanceCombo = new WeaponInstanceCombo();
		weaponInstanceCombo.AttackData = new WeaponInstanceComboAttackData();
		weaponInstanceCombo.AttackData.JobData = new WeaponInstanceComboJobData();
		weaponInstanceCombo.AttackData.JobData.Job = (AttackJob)job;
		if (attackParams.HasValue)
		{
			weaponInstanceCombo.AttackData.bodyPartID = attackParams.Value.BodyPartToAttackID;
			weaponInstanceCombo.AttackData.AttackType = attackParams.Value.AttackType;
			if (attackParams.Value.Weapon.HasValue)
			{
				IKnownEntityData data;
				EntityResult knownData = entityIntelligence.GetKnownData(attackParams.Value.Weapon.Value.Entity, out data);
				if (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown)
				{
					rating = 0.0;
					return CalculateResult.Done;
				}
				weaponInstanceCombo.Weapon = data;
			}
			if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(weaponInstanceCombo.AttackData.JobData.Job.Target.Value, out var data2)))
			{
				rating = 0.0;
				return CalculateResult.Done;
			}
			BodyPart bodyPart = data2.Body.FindBodyPart(weaponInstanceCombo.AttackData.bodyPartID);
			weaponInstanceCombo.AttackData.EstimatedDamageScore = GameData.Instance.AttackScoresAgainstBodyParts[weaponInstanceCombo.AttackData.AttackType][bodyPart.BodyPartType];
		}
		float energyLevelFactorOnDamage = GoalDoAttack.GetEnergyLevelFactorOnDamage(entity, weaponInstanceCombo.AttackData.AttackType);
		float value = ScoreFitness(entity);
		return ScoreThisAttackJob(regionMap, entity, proposedNumberOfWorkers, ageContribution, timeContribution, value, energyLevelFactorOnDamage, weaponInstanceCombo, out rating);
	}

	private static float ScoreFitness(Entity entity)
	{
		entity.Find<BodyComponent>(out var c);
		return c.Body.GlobalHitpoints / c.Body.MaxHitpoints;
	}

	public CalculateResult ScoreThisAttackJob(RegionMap regionMap, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, float? fitnessScore, float? energyFactor, WeaponInstanceCombo combo, out double score)
	{
		score = 0.0;
		if (!ageContribution.HasValue)
		{
			ageContribution = GetAgeContribution();
		}
		if (!timeContribution.HasValue)
		{
			timeContribution = ScoreTimeOfDay();
		}
		AttackJob job = combo.AttackData.JobData.Job;
		Intelligence intelligence = entity.Intelligence;
		if (!combo.AttackData.AttackScore.HasValue)
		{
			double rating = 0.0;
			if (job.ScoreThisJobWithoutWeapon(entity, intelligence, combo, proposedNumberOfWorkers, ageContribution, timeContribution, ref rating, fitnessScore, Priority, energyFactor) != CalculateResult.Done)
			{
				return CalculateResult.Processing;
			}
			combo.AttackData.AttackScore = rating;
		}
		double? num = null;
		if (combo.AttackData.AttackScore > 0.0)
		{
			if (ScoreDistance(entity, combo, job.Target.Value, ageContribution, timeContribution, intelligence, regionMap, Priority, out var score2) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			if (combo.Weapon != null)
			{
				double score3;
				CalculateResult num2 = job.ScoreWeapon(entity, combo.Weapon, score2, ageContribution, timeContribution, Priority, combo.AttackData.AttackType, out score3);
				num = score3;
				if (num2 == CalculateResult.Processing)
				{
					return CalculateResult.Processing;
				}
				if (!Common.IsZero(num) && combo.AttackData.AttackType.UsesAmmo != null && !combo.Weapon.HasEnoughAmmo(combo.AttackData.AttackType.UsesAmmoType, combo.AttackData.AttackType.RoundsToSpend.Value))
				{
					num *= 0.6499999761581421;
				}
			}
			else
			{
				num = score2;
			}
		}
		score = job.CombineJobAndWeaponScore(combo.AttackData.AttackScore.Value, num);
		EvaluateJob.ApplyJobPriorityModifier(job.Priority, ref score);
		return CalculateResult.Done;
	}

	private CalculateResult ScoreDistance(Entity entity, WeaponInstanceCombo combo, EntityID target, double? ageContribution, double? timeContribution, Intelligence entityIntelligence, RegionMap regionMap, float priority, out double score)
	{
		score = 0.0;
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(target, out var data)))
		{
			return CalculateResult.Done;
		}
		float num = 0f;
		if (combo.AttackData.AttackType.MaxRange.HasValue)
		{
			num = combo.AttackData.AttackType.MaxRange.Value;
		}
		CalculateResult calculateResult;
		double score2;
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			calculateResult = ((combo.Weapon == null) ? ProcessJob.ScoreToLocation(entity, entity.PlaySiteLocation, data.Location.Value, sharedKnowledge, regionMap, out score2, num) : ProcessJob.ScoreToolLocation(entity, combo.Weapon, entity.PlaySiteLocation, data.Location.Value, sharedKnowledge, regionMap, out var _, out score2, num));
		}
		else
		{
			score2 = ((!(Common.DistanceOctile(entity.PlaySiteLocation, data.PlaySiteLocation) <= num)) ? 0.0 : 1.0);
			calculateResult = CalculateResult.Done;
		}
		if (calculateResult == CalculateResult.Processing)
		{
			return CalculateResult.Processing;
		}
		score = score2 * 0.699999988079071;
		return CalculateResult.Done;
	}

	private bool GetJobsToCancel(WeaponInstanceCombo combo)
	{
		needsToBeCancelled.Clear();
		itemsToBeDropped.Clear();
		List<Entity> list = new List<Entity>();
		Job job = combo.AttackData.JobData.Job;
		if (job.TakenBy.Count > 0)
		{
			job.TakenBy.GetLowestScorer();
			for (int i = 0; i < job.TakenBy.Count; i++)
			{
				list.Add(job.TakenBy.Get(i));
			}
			while (list.Count >= job.MaxJobPositions)
			{
				needsToBeCancelled.Add(list[0]);
				list.RemoveAt(0);
			}
		}
		if (combo.Weapon != null && combo.Weapon.EntityType.ItemType != null)
		{
			GetItemUsersToCancel(combo.Weapon, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: false);
		}
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
		return true;
	}

	public override bool CancelCurrentTakers()
	{
		return CancelEntities(needsToBeCancelled, itemsToBeDropped);
	}

	public override bool CanTakeGoal()
	{
		if (bestCombo != null)
		{
			if (!EvaluateJob.IsJobValid(bestCombo.AttackData.JobData.Job))
			{
				return false;
			}
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			if (bestCombo.Weapon != null)
			{
				entity.Contains.Contains(bestCombo.Weapon.EntityID);
			}
			if (GetJobsToCancel(bestCombo) && CanCancelAndForceDropItems(bestCombo.Score))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanCancelAndForceDropItems(double ourScore)
	{
		if (CanForceDropItems(itemsToBeDropped) && GoalEvaluator.IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled))
		{
			return true;
		}
		return false;
	}
}
