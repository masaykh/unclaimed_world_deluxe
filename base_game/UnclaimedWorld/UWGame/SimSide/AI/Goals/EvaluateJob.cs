using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateJob : GoalEvaluator, IScoreJob
{
	private enum Progress
	{
		NotStarted,
		GetCombos,
		ScoreCombos,
		FindFinalCombo
	}

	public enum JobType
	{
		ColonyWork,
		NonColonyWork
	}

	[DebuggerDisplay("Job: {Job}, ToolsAndWeapons: {ToolsAndWeapons}, ImmovableTool: {ImmovableTool}, Score: {Score}")]
	public class ToolOrWeaponInstanceCombo
	{
		public Job Job;

		public List<EntityID> ToolsAndWeapons = new List<EntityID>();

		public EntityID? ImmovableTool;

		public Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> ReplenishItemsForTools;

		public ToolTypeCombination ToolTypeCombination;

		public ToolOrWeaponInstanceComboJobData JobData;

		public AttackType attackType;

		public double Score;
	}

	public class ToolOrWeaponInstanceComboJobData
	{
		public IResourceItem ResourceItem;

		public IKnownEntityData InputItem;

		public double? HighestToolScore;

		public List<EntityID> HighestScoringToolCombo;

		public Vector3? TemporaryGroundLocation;

		public Vector3? Location;

		public double JobScore;
	}

	private ToolOrWeaponInstanceCombo bestCombo;

	private EntityGroup itemGroup;

	private List<EntityGroup> vehicleGroups;

	private EntityGroup ownerOfJobs;

	private OwnerID? ownerOfNewProducts;

	private Progress progress;

	private List<Entity> needsToBeCancelled = new List<Entity>();

	private List<Entity> itemsToBeDropped = new List<Entity>();

	private Dictionary<EntityType, bool> gatherJobTypeComboAlreadyAdded = new Dictionary<EntityType, bool>();

	private Dictionary<Job, ToolOrWeaponInstanceComboJobData> cachedJobScores = new Dictionary<Job, ToolOrWeaponInstanceComboJobData>();

	public Dictionary<EntityType, ReplenishStatus> cachedToolEnergyAvailableStates = new Dictionary<EntityType, ReplenishStatus>();

	private Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates = new Dictionary<AttackType, ReplenishWeaponStatus>();

	private Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity = new Dictionary<EntityType, List<ItemDistance>>();

	private int scoreComboIndex;

	private int finalComboSelectionIndex;

	private bool scoringWasInterrupted;

	private float priority;

	private List<ToolOrWeaponInstanceCombo> allCombos = new List<ToolOrWeaponInstanceCombo>();

	public override float Priority => priority;

	public EvaluateJob(Entity entity, EntityGroup ownerOfJobs, EntityGroup itemGroup, List<EntityGroup> vehicleGroups, OwnerID? ownerOfNewProducts)
		: base(entity)
	{
		this.ownerOfNewProducts = ownerOfNewProducts;
		this.ownerOfJobs = ownerOfJobs;
		this.itemGroup = itemGroup;
		this.vehicleGroups = vehicleGroups;
		priority = GameData.Instance.AIConstants.PriorityOfWorkJobs;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (entity.Name != null)
		{
			entity.Name.Contains("zov");
		}
		bestCombo = null;
		bestScore = minimumRatingToConsider;
		if (progress == Progress.NotStarted)
		{
			if (entity.Find<BiologicalEntity>(out var c) && ScoreAge(c) == 0.0)
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			ScoreTimeOfDay();
			progress = Progress.GetCombos;
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			scoringWasInterrupted = false;
		}
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		if (progress == Progress.GetCombos)
		{
			GetAllJobCombos(regionMapAndStanceForEvaluator);
		}
		if (progress == Progress.ScoreCombos && ScoreAllCombos(regionMapAndStanceForEvaluator, threatStanceToUse) == CalculateResult.Processing)
		{
			return CalculateResult.Processing;
		}
		if (progress == Progress.FindFinalCombo)
		{
			if (FindBestCombo(minimumRatingToConsider, ref bestCombo) != CalculateResult.Done)
			{
				return CalculateResult.Processing;
			}
			if (bestCombo != null)
			{
				bestScore = bestCombo.Score;
				result = bestCombo.Score;
				entityIntelligence.TopScoringJobs.Add(new GoalAndScore
				{
					Score = bestScore,
					Goal = bestCombo.Job.ToString()
				});
				return CalculateResult.Done;
			}
		}
		_ = bestCombo;
		result = 0.0;
		return CalculateResult.Done;
	}

	public static void SetAreaNotCleared(Job job, bool value)
	{
		job.ResolveOwner(out var owner);
		if (owner != null)
		{
			The.Client.SetAreaNotCleared(job, owner.Parent, value);
		}
	}

	private bool IsGatherJobAccessible(ProcessJob pJob, RegionMap regionMap)
	{
		Point point = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
		if (pJob.HarvestJob != null)
		{
			Point point2 = MapManager.WorldPosToSubtile(pJob.HarvestJob.Item.Container.AccessPoint);
			float distance = -1f;
			RegionMap.Result distanceToEntity = regionMap.GetDistanceToEntity(entity, entity, null, ref distance, point, point2);
			switch (distanceToEntity)
			{
			case RegionMap.Result.Wait:
				return false;
			case RegionMap.Result.NoAccess:
				GoalEvaluator.UpdateJobAccessibility(entity, point, point2, pJob, ownerOfJobs.Parent, distanceToEntity);
				if (pJob.HarvestJob.Item.Container is Crop { ParentEntity: not null } crop2)
				{
					crop2.ParentEntity.Renderable.SetTintColor(Color.Red);
				}
				else if (pJob.HarvestJob.Item.Container is TileResourceContainer tileResourceContainer2)
				{
					tileResourceContainer2.Renderable.SetResourceContainerTintColor(Color.Red);
				}
				return false;
			default:
				The.Client.SetJobInaccessible(pJob, ownerOfJobs.Parent, isInaccessible: false);
				if (pJob.HarvestJob.Item.Container is Crop { ParentEntity: not null } crop)
				{
					crop.ParentEntity.Renderable.SetResourceContainerTintColor(Color.Yellow);
				}
				else if (pJob.HarvestJob.Item.Container is TileResourceContainer tileResourceContainer)
				{
					tileResourceContainer.Renderable.SetResourceContainerTintColor(Color.Yellow);
				}
				return true;
			}
		}
		return true;
	}

	private void GetAllJobCombos(RegionMap regionMap)
	{
		scoreComboIndex = 0;
		allCombos.Clear();
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in ownerOfJobs.ProductionJobs)
		{
			for (int num = productionJob.Value.Count - 1; num >= 0; num--)
			{
				Job job = productionJob.Value[num];
				if (!entityIntelligence.Brain.IsSame(job) && job is ProcessJob processJob && IsGatherJobAccessible(processJob, regionMap) && !SkipCombos(processJob))
				{
					GetAllToolInstanceCombos(processJob, out var jobIsDestroyed);
					if (!jobIsDestroyed && processJob.TakenBy.Count == 0)
					{
						SetGatherJobTypeComboAsAlreadyAdded(processJob);
					}
					if (jobIsDestroyed)
					{
						processJob.Destroy(removeTakers: true);
					}
				}
			}
		}
		foreach (ScoutingJob scoutingJob in ownerOfJobs.ScoutingJobs)
		{
			entityIntelligence.Brain.IsSame(scoutingJob);
		}
		foreach (FindPreyJob findPreyJob in ownerOfJobs.FindPreyJobs)
		{
			if (!entityIntelligence.Brain.IsSame(findPreyJob))
			{
				GetAllWeaponInstanceCombos(findPreyJob);
			}
		}
		foreach (CheckProcessJob checkProcessJob in ownerOfJobs.CheckProcessJobs)
		{
			if (!entityIntelligence.Brain.IsSame(checkProcessJob))
			{
				GetEmptyCombo(checkProcessJob);
			}
		}
		foreach (PatrolJob patrolJob in ownerOfJobs.PatrolJobs)
		{
			if (!entityIntelligence.Brain.IsSame(patrolJob))
			{
				GetAllWeaponInstanceCombos(patrolJob);
			}
		}
		foreach (AttackAreaJob attackAreaJob in ownerOfJobs.AttackAreaJobs)
		{
			if (!entityIntelligence.Brain.IsSame(attackAreaJob))
			{
				GetAllWeaponInstanceCombos(attackAreaJob);
			}
		}
		for (int num2 = ownerOfJobs.OtherJobs.Count - 1; num2 >= 0; num2--)
		{
			Job job7 = ownerOfJobs.OtherJobs[num2];
			if (!(job7 is HuntingJob) && !entityIntelligence.Brain.IsSame(job7))
			{
				GetAllToolInstanceCombos(job7, out var _);
			}
		}
		if (ownerOfJobs.RepairJobs.Count > 0)
		{
			foreach (EntityID key in ownerOfJobs.RepairJobs.Keys)
			{
				List<ProcessJob> list = ownerOfJobs.RepairJobs[key];
				for (int num3 = list.Count - 1; num3 >= 0; num3--)
				{
					ProcessJob job8 = list[num3];
					if (!entityIntelligence.Brain.IsSame(job8))
					{
						GetAllToolInstanceCombos(job8, out var _);
					}
				}
			}
		}
		progress = Progress.ScoreCombos;
	}

	private bool IsComboValid(Entity entity, Intelligence entityIntelligence, ToolOrWeaponInstanceCombo combo)
	{
		if (combo.Job == null || combo.Job.ID == JobID.Invalid)
		{
			return false;
		}
		foreach (EntityID toolsAndWeapon in combo.ToolsAndWeapons)
		{
			if (!IsValidPlaysiteTool(toolsAndWeapon, null, entityIntelligence.Allegiance.SharedKnowledge, ownerOfJobs, out var _, out var _))
			{
				return false;
			}
		}
		return true;
	}

	private CalculateResult ScoreAllCombos(RegionMap footRegionMap, ThreatStance threatStance)
	{
		double rating = -1.0;
		double ageContribution = GetAgeContribution();
		double value = ScoreTimeOfDay();
		ToolOrWeaponInstanceCombo combo;
		while (scoreComboIndex < allCombos.Count)
		{
			combo = allCombos[scoreComboIndex];
			if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo))
			{
				int proposedNumberOfWorkers = Common.Clamp(combo.Job.TakenBy.Count + 1, 0, combo.Job.MaxJobPositions);
				float value2 = 1f;
				if (combo.ToolTypeCombination != null)
				{
					value2 = combo.ToolTypeCombination.Productivity;
				}
				ToolParams value3 = new ToolParams
				{
					Tools = combo.ToolsAndWeapons,
					ImmovableTool = combo.ImmovableTool,
					ToolProductivity = value2,
					ReplenishStatus = cachedToolEnergyAvailableStates,
					JobDurationInDays = null
				};
				if (ScoreThisWorkJob(footRegionMap, threatStance, entity, combo.Job, proposedNumberOfWorkers, ageContribution, value, ref combo.JobData, value3, out rating, out var jobIsValid) != CalculateResult.Done)
				{
					scoringWasInterrupted = true;
					return CalculateResult.Processing;
				}
				combo.Score = rating;
				SetDebugScore(entity, combo.Job, rating);
				if (!jobIsValid)
				{
					Job jobToRemove = combo.Job;
					GoalEvaluator.HandleInvalidJob(jobToRemove);
					allCombos.FindAll((ToolOrWeaponInstanceCombo c) => c.Job == jobToRemove).ForEach(delegate(ToolOrWeaponInstanceCombo c)
					{
						c.Job = null;
						combo.Score = 0.0;
					});
					scoringWasInterrupted = true;
				}
			}
			else
			{
				combo.Score = 0.0;
				SetDebugScore(entity, combo.Job, 0.0);
			}
			scoreComboIndex++;
		}
		AssignLocations();
		gatherJobTypeComboAlreadyAdded.Clear();
		cachedJobScores.Clear();
		cachedToolEnergyAvailableStates.Clear();
		cachedWeaponReplenishStates.Clear();
		scoreComboIndex = 0;
		progress = Progress.FindFinalCombo;
		return CalculateResult.Done;
	}

	private void AssignLocations()
	{
		for (int i = 0; i < allCombos.Count; i++)
		{
			ToolOrWeaponInstanceCombo toolOrWeaponInstanceCombo = allCombos[i];
			if (toolOrWeaponInstanceCombo.JobData == null || !toolOrWeaponInstanceCombo.JobData.HighestToolScore.HasValue || toolOrWeaponInstanceCombo.JobData.HighestScoringToolCombo != toolOrWeaponInstanceCombo.ToolsAndWeapons)
			{
				continue;
			}
			ProcessJob processJob = toolOrWeaponInstanceCombo.Job as ProcessJob;
			if (toolOrWeaponInstanceCombo.ImmovableTool.HasValue)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(toolOrWeaponInstanceCombo.ImmovableTool.Value, out var data)) && !data.AssignedToJob.HasValue)
				{
					processJob.AssignImmovableTool(data);
				}
			}
			else if (toolOrWeaponInstanceCombo.JobData.TemporaryGroundLocation.HasValue)
			{
				processJob.AssignJobLocation(toolOrWeaponInstanceCombo.JobData.TemporaryGroundLocation.Value);
			}
			toolOrWeaponInstanceCombo.JobData.HighestToolScore = null;
		}
	}

	private void ResetAllToolsInUse(ToolOrWeaponInstanceCombo combo)
	{
		if (combo.Job is ProcessJob)
		{
			ProcessJob processJob = combo.Job as ProcessJob;
			if (combo.ToolTypeCombination != null)
			{
				processJob.AllToolsInUse = false;
				processJob.AllToolsAreBroken = false;
			}
		}
	}

	private CalculateResult FindBestCombo(double minimumRatingToConsider, ref ToolOrWeaponInstanceCombo bestCombo)
	{
		FilterAndSortCombos();
		bestCombo = null;
		if (allCombos.Count <= 0 || entity.Name == null || entity.Name.Contains("coyd"))
		{
		}
		while (finalComboSelectionIndex < allCombos.Count)
		{
			ToolOrWeaponInstanceCombo toolOrWeaponInstanceCombo = allCombos[finalComboSelectionIndex];
			ResetAllToolsInUse(toolOrWeaponInstanceCombo);
			if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, toolOrWeaponInstanceCombo))
			{
				if (toolOrWeaponInstanceCombo.Score < minimumRatingToConsider)
				{
					for (int i = finalComboSelectionIndex; i < allCombos.Count; i++)
					{
						ResetAllToolsInUse(allCombos[i]);
					}
					break;
				}
				if (GetJobsToCancel(toolOrWeaponInstanceCombo))
				{
					if (GoalEvaluator.IsScoreBetterThanAllInvolveds(toolOrWeaponInstanceCombo.Score, needsToBeCancelled, entity))
					{
						if (toolOrWeaponInstanceCombo.Job is ProcessJob)
						{
							ProcessJob processJob = toolOrWeaponInstanceCombo.Job as ProcessJob;
							if (toolOrWeaponInstanceCombo.ToolTypeCombination == null)
							{
								bestCombo = toolOrWeaponInstanceCombo;
								break;
							}
							float jobDuration = processJob.ProcessType.EstimateTotalDurationInDays(entity, processJob.ProcessType.GetTimeNeeded(), toolOrWeaponInstanceCombo.ToolTypeCombination.Productivity);
							if (FindReplenishItems(toolOrWeaponInstanceCombo, jobDuration, out var success) == CalculateResult.Processing)
							{
								return CalculateResult.Processing;
							}
							if (success)
							{
								bestCombo = toolOrWeaponInstanceCombo;
								GetReplenishItemsToCancel(bestCombo.ReplenishItemsForTools, ref needsToBeCancelled, ref itemsToBeDropped);
								break;
							}
						}
						else
						{
							if (toolOrWeaponInstanceCombo.ToolsAndWeapons.Count <= 0 || toolOrWeaponInstanceCombo.attackType.UsesAmmo == null)
							{
								bestCombo = toolOrWeaponInstanceCombo;
								break;
							}
							entityIntelligence.GetKnownData(toolOrWeaponInstanceCombo.ToolsAndWeapons[0], out var _);
							if (FindReplenishItems(toolOrWeaponInstanceCombo, 0f, out var success2, toolOrWeaponInstanceCombo.attackType.UsesAmmoType, toolOrWeaponInstanceCombo.attackType.RoundsToSpend.Value) == CalculateResult.Processing)
							{
								return CalculateResult.Processing;
							}
							if (success2)
							{
								bestCombo = toolOrWeaponInstanceCombo;
								GetReplenishItemsToCancel(bestCombo.ReplenishItemsForTools, ref needsToBeCancelled, ref itemsToBeDropped);
								break;
							}
						}
					}
					else if (itemsToBeDropped.Count > 0 && toolOrWeaponInstanceCombo.ToolTypeCombination != null && toolOrWeaponInstanceCombo.Job.TakenBy.Count == 0 && toolOrWeaponInstanceCombo.Job is ProcessJob processJob2)
					{
						processJob2.AllToolsInUse = true;
					}
				}
			}
			finalComboSelectionIndex++;
		}
		finalComboSelectionIndex = 0;
		energyItemsSortedByDistanceToEntity.Clear();
		progress = Progress.NotStarted;
		return CalculateResult.Done;
	}

	private void FilterAndSortCombos()
	{
		allCombos.RemoveAll((ToolOrWeaponInstanceCombo c) => c.Score == 0.0);
		allCombos.Sort((ToolOrWeaponInstanceCombo a, ToolOrWeaponInstanceCombo b) => b.Score.CompareTo(a.Score));
	}

	private CalculateResult FindReplenishItems(ToolOrWeaponInstanceCombo combo, float jobDuration, out bool success, EntityType neededAmmoType = null, int? neededAmmoRounds = null)
	{
		double score = combo.Score;
		SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
		RegionMap footRegionMap = null;
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			footRegionMap = sharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap;
		}
		float? num = null;
		float? neededElectricalEnergy = null;
		Expedition expedition = itemGroup.GetExpedition();
		foreach (EntityID toolsAndWeapon in combo.ToolsAndWeapons)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(toolsAndWeapon, out var data)))
			{
				success = false;
				return CalculateResult.Done;
			}
			if (data.EntityType.ContainerType != null && data.EntityType.ContainerType.GetRequiresReplenishType() != null)
			{
				num = data.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType?.GetNeededFuel(jobDuration);
			}
			if (FindReplenishItemsForToolOrWeapon(entity, itemGroup, null, ref combo.ReplenishItemsForTools, score, sharedKnowledge, footRegionMap, data, energyItemsSortedByDistanceToEntity, out success, neededAmmoType, neededAmmoRounds, neededElectricalEnergy, num) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			if (!success)
			{
				if (expedition != null)
				{
					The.Client.SetToolReplenishStatusItemAvailable(expedition, data.EntityType, itemAvailable: false, num);
				}
				return CalculateResult.Done;
			}
			if (expedition != null)
			{
				The.Client.SetToolReplenishStatusItemAvailable(expedition, data.EntityType, itemAvailable: true, num);
			}
		}
		success = true;
		return CalculateResult.Done;
	}

	private bool GetJobsToCancel(ToolOrWeaponInstanceCombo combo)
	{
		needsToBeCancelled.Clear();
		itemsToBeDropped.Clear();
		List<Entity> list = new List<Entity>();
		Job job = combo.Job;
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
		if (combo.ToolsAndWeapons != null && combo.ToolsAndWeapons.Count > 0)
		{
			foreach (EntityID toolsAndWeapon in combo.ToolsAndWeapons)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(toolsAndWeapon, out var data)))
				{
					return false;
				}
				GetItemUsersToCancel(data, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: false);
			}
		}
		if (combo.Job is ProcessJob processJob)
		{
			if (processJob.HarvestJob != null && combo.JobData.ResourceItem.AssignedToJob != null && combo.JobData.ResourceItem.AssignedToJob.TakenBy.Count > 0)
			{
				needsToBeCancelled.Add(combo.JobData.ResourceItem.AssignedToJob.TakenBy.Get(0));
			}
			if (processJob.GetActingOnEntity(out EntityID? actingOnEntity) && actingOnEntity.HasValue)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(actingOnEntity.Value, out var data2)))
				{
					return false;
				}
				GetItemUsersToCancel(data2, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: false);
			}
		}
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
		return true;
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
	{
		ToolOrWeaponInstanceComboJobData jobData = null;
		bool jobIsValid;
		return ScoreThisWorkJob(regionMap, threatStance, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData, toolParams, out rating, out jobIsValid, cacheScore: false);
	}

	public CalculateResult ScoreThisWorkJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Job Job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams, out double rating, out bool jobIsValid, bool cacheScore = true)
	{
		rating = 0.0;
		jobIsValid = true;
		if (!ageContribution.HasValue)
		{
			ageContribution = GetAgeContribution();
		}
		if (!timeContribution.HasValue)
		{
			timeContribution = ScoreTimeOfDay();
		}
		if (!CanTakeStanceForJob(Job, regionMap, threatStance, out var regionMapToUse, out var threatStanceToUse))
		{
			rating = 0.0;
			return CalculateResult.Done;
		}
		if (Job is ProcessJob processJob)
		{
			return processJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData, toolParams, ref rating, ref jobIsValid, cacheScore, cachedJobScores, entityIntelligence, Priority, ownerOfJobs);
		}
		if (Job is LightFireJob lightFireJob)
		{
			return lightFireJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, ageContribution, timeContribution, ref rating, Priority);
		}
		if (Job is FindPreyJob findPreyJob)
		{
			return findPreyJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
		}
		if (Job is CheckProcessJob checkProcessJob)
		{
			return checkProcessJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
		}
		if (Job is CombatAreaJob combatAreaJob)
		{
			return combatAreaJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
		}
		throw new Exception("!!");
	}

	public static void SetDebugScore(Entity entity, Job job, double score)
	{
	}

	public static void SetDebugScoreNoTools(Entity entity, Job job, double score)
	{
	}

	public static void ApplyJobPriorityModifier(Priority priority, ref double score)
	{
		switch (priority)
		{
		case UWGame.SimSide.Jobs.Priority.High:
			score *= GameData.Instance.AIConstants.HighJobModifier;
			break;
		case UWGame.SimSide.Jobs.Priority.Low:
			score *= GameData.Instance.AIConstants.LowJobModifier;
			break;
		}
	}

	private bool GetOwnedTools(EntityType entityType, out List<EntityID> items, IKnownEntityData onlyValidToolOfType)
	{
		items = null;
		if (onlyValidToolOfType != null)
		{
			if (onlyValidToolOfType.EntityType == entityType)
			{
				Common.AddToList(ref items, onlyValidToolOfType.EntityID);
				return true;
			}
			if (ToolType.IsImmovable(onlyValidToolOfType.EntityType) && ToolType.IsImmovable(entityType))
			{
				return false;
			}
		}
		if (entityType.ToolType.ToolHandling == ToolHandlingType.Intrinsic)
		{
			if (entityIntelligence.IntrinsicTools != null && entityIntelligence.IntrinsicTools.TryGetValue(entityType, out var value))
			{
				Common.AddToList(ref items, value);
				return true;
			}
		}
		else if (entityType.ItemType != null && entity.EntityType.IntelligenceType.CanMountTools == true)
		{
			if (ownerOfJobs.Items.TryGetValue(entityType, out items))
			{
				return items.Count > 0;
			}
		}
		else if (entityType.StructureType != null && ownerOfJobs.Structures.TryGetValue(entityType, out items))
		{
			return items.Count > 0;
		}
		return false;
	}

	private bool GetOwnedWeapons(out HashSet<IKnownEntityData> weapons)
	{
		weapons = new HashSet<IKnownEntityData>();
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		if (ownerOfJobs.WeaponsByAttackType != null)
		{
			foreach (KeyValuePair<AttackType, List<EntityID>> item in ownerOfJobs.WeaponsByAttackType)
			{
				for (int num = item.Value.Count - 1; num >= 0; num--)
				{
					EntityID entityID = item.Value[num];
					if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, ownerOfJobs, out var entityData))
					{
						weapons.Add(entityData);
					}
				}
			}
			return true;
		}
		return false;
	}

	private void GetAllWeaponInstanceCombos(Job job)
	{
		IRequiresWeapon requiresWeapon = job as IRequiresWeapon;
		if (base.entity.EntityType.IntelligenceType.CanUseWeapons != false)
		{
			List<EntityGroup> list = new List<EntityGroup>();
			list.Add(ownerOfJobs);
			requiresWeapon.WeaponsAreAvailable = false;
			if (GetOwnedWeapons(out var weapons))
			{
				foreach (IKnownEntityData item in weapons)
				{
					GetWeaponInstanceCombos(job, requiresWeapon, item, list);
				}
			}
		}
		if (base.entity.EntityType.IntelligenceType.IntrinsicWeaponTypes == null)
		{
			return;
		}
		List<EntityGroup> list2 = new List<EntityGroup>();
		list2.Add(ownerOfJobs);
		foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in entityIntelligence.IntrinsicWeapons)
		{
			Entity entity = Entity.FindByID(intrinsicWeapon.Value);
			if (entity != null)
			{
				GetWeaponInstanceCombos(job, requiresWeapon, entity, list2);
			}
		}
	}

	private void GetWeaponInstanceCombos(Job job, IRequiresWeapon requiresWeapon, IKnownEntityData weapon, List<EntityGroup> ownersOfWeapons)
	{
		AttackType attackType = null;
		int num = 0;
		bool flag = false;
		if (Common.IsZero((double)weapon.EntityType.ItemType.GetTaskAppropriateLevel(requiresWeapon.TaskType)))
		{
			return;
		}
		while (num < weapon.EntityType.ItemType.WeaponType.AttackTypes.Length)
		{
			attackType = weapon.EntityType.ItemType.WeaponType.AttackTypes[num];
			flag = EvaluateAttackJobs.IsValidPlaysiteWeapon(entity, weapon, attackType, ownersOfWeapons, cachedWeaponReplenishStates);
			if (flag)
			{
				ToolOrWeaponInstanceCombo toolOrWeaponInstanceCombo = new ToolOrWeaponInstanceCombo
				{
					Job = job
				};
				allCombos.Add(toolOrWeaponInstanceCombo);
				toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(weapon.EntityID);
				toolOrWeaponInstanceCombo.attackType = attackType;
				requiresWeapon.WeaponsAreAvailable = true;
				break;
			}
			num++;
			if (flag)
			{
				break;
			}
		}
	}

	private void GetEmptyCombo(Job job)
	{
		ToolOrWeaponInstanceCombo item = new ToolOrWeaponInstanceCombo
		{
			Job = job,
			ToolTypeCombination = null
		};
		allCombos.Add(item);
	}

	private void GetAllToolInstanceCombos(Job job, out bool jobIsDestroyed)
	{
		if (job is ProcessJob processJob)
		{
			if (processJob.ProcessType.ProcessToolSet != null)
			{
				SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
				IKnownEntityData data = null;
				if (!processJob.GetImmovableTool(out var tool))
				{
					jobIsDestroyed = true;
					return;
				}
				if (tool.HasValue && GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(tool.Value, out data)))
				{
					jobIsDestroyed = true;
					return;
				}
				float timeNeeded = processJob.ProcessType.GetTimeNeeded();
				float skillProductionFactor = entity.Intelligence.GetSkillProductionFactor(processJob.ProcessType.RequiredSkillType);
				float energyProductivity = processJob.ProcessType.GetEnergyProductivity(entity);
				foreach (ToolTypeCombination toolTypeCombination in processJob.ProcessType.ProcessToolSet.ToolTypeCombinations)
				{
					if (!HasAllTools(toolTypeCombination.Tools))
					{
						continue;
					}
					float value = processJob.ProcessType.EstimateTotalDurationInDays(timeNeeded, skillProductionFactor, energyProductivity, toolTypeCombination.Productivity);
					int count = toolTypeCombination.Tools.Count;
					EntityType entityType = null;
					EntityType entityType2 = null;
					EntityType entityType3 = null;
					entityType = toolTypeCombination.Tools[0].Item1;
					if (count > 1)
					{
						entityType2 = toolTypeCombination.Tools[1].Item1;
						if (count > 2)
						{
							entityType3 = toolTypeCombination.Tools[2].Item1;
						}
					}
					EntityID? entityID = null;
					ToolOrWeaponInstanceCombo toolOrWeaponInstanceCombo = null;
					bool anyToolInUse = false;
					bool anyToolIsBroken = false;
					if (GetOwnedTools(entityType, out var items, data))
					{
						for (int num = items.Count - 1; num >= 0; num--)
						{
							EntityID entityID2 = items[num];
							if (IsValidTool(entityID2, value, sharedKnowledge, ownerOfJobs, out var inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
							{
								entityID = null;
								SaveImmovableItem(entityType, entityID2, ref entityID);
								if (entityType2 != null && GetOwnedTools(entityType2, out var items2, data))
								{
									for (int num2 = items2.Count - 1; num2 >= 0; num2--)
									{
										EntityID entityID3 = items2[num2];
										if (IsValidTool(entityID3, value, sharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
										{
											entityID = null;
											SaveImmovableItem(entityType, entityID2, ref entityID);
											SaveImmovableItem(entityType2, entityID3, ref entityID);
											if (entityType3 != null && GetOwnedTools(entityType3, out var items3, data))
											{
												for (int num3 = items3.Count - 1; num3 >= 0; num3--)
												{
													EntityID entityID4 = items3[num3];
													if (IsValidTool(entityID4, value, sharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
													{
														entityID = null;
														SaveImmovableItem(entityType, entityID2, ref entityID);
														SaveImmovableItem(entityType2, entityID3, ref entityID);
														SaveImmovableItem(entityType3, entityID4, ref entityID);
														toolOrWeaponInstanceCombo = new ToolOrWeaponInstanceCombo
														{
															Job = processJob,
															ToolTypeCombination = toolTypeCombination,
															ImmovableTool = entityID
														};
														allCombos.Add(toolOrWeaponInstanceCombo);
														toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID2);
														toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID3);
														toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID4);
													}
												}
											}
											else
											{
												toolOrWeaponInstanceCombo = new ToolOrWeaponInstanceCombo
												{
													Job = processJob,
													ToolTypeCombination = toolTypeCombination,
													ImmovableTool = entityID
												};
												allCombos.Add(toolOrWeaponInstanceCombo);
												toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID2);
												toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID3);
											}
										}
									}
								}
								else
								{
									toolOrWeaponInstanceCombo = new ToolOrWeaponInstanceCombo
									{
										Job = processJob,
										ToolTypeCombination = toolTypeCombination,
										ImmovableTool = entityID
									};
									allCombos.Add(toolOrWeaponInstanceCombo);
									toolOrWeaponInstanceCombo.ToolsAndWeapons.Add(entityID2);
								}
							}
						}
					}
					if (toolOrWeaponInstanceCombo != null)
					{
						continue;
					}
					bool isStarted2;
					if (anyToolInUse)
					{
						if (processJob.IsStarted(out var isStarted) && !isStarted)
						{
							processJob.AllToolsInUse = true;
						}
					}
					else if (anyToolIsBroken && processJob.IsStarted(out isStarted2) && !isStarted2)
					{
						processJob.AllToolsAreBroken = true;
					}
				}
			}
			else
			{
				ToolOrWeaponInstanceCombo item = new ToolOrWeaponInstanceCombo
				{
					Job = processJob,
					ToolTypeCombination = null
				};
				allCombos.Add(item);
			}
		}
		else
		{
			ToolOrWeaponInstanceCombo item2 = new ToolOrWeaponInstanceCombo
			{
				Job = job,
				ToolTypeCombination = null
			};
			allCombos.Add(item2);
		}
		jobIsDestroyed = false;
	}

	private void SaveImmovableItem(EntityType entityType, EntityID item, ref EntityID? immovableItem)
	{
		if (ToolType.IsImmovable(entityType))
		{
			immovableItem = item;
		}
	}

	private bool CanCarryAllTools(EntityType tool0, EntityType tool1, EntityType tool2 = null)
	{
		float num = 0f;
		if (!ToolType.IsImmovable(tool0))
		{
			num += tool0.ItemType.MaximumBulk.Value;
		}
		if (!ToolType.IsImmovable(tool1))
		{
			num += tool1.ItemType.MaximumBulk.Value;
		}
		if (!ToolType.IsImmovable(tool2))
		{
			num += tool2.ItemType.MaximumBulk.Value;
		}
		return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(num);
	}

	private bool CanCarryAllTools(Entity tool0, Entity tool1, Entity tool2 = null)
	{
		float num = 0f;
		if (!ToolType.IsImmovable(tool0.EntityType))
		{
			num += tool0.Bulk;
		}
		if (!ToolType.IsImmovable(tool1.EntityType))
		{
			num += tool1.Bulk;
		}
		if (!ToolType.IsImmovable(tool2.EntityType))
		{
			num += tool2.Bulk;
		}
		return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(num);
	}

	private bool CanCarryAllTools(List<Entity> tools)
	{
		float num = 0f;
		foreach (Entity tool in tools)
		{
			if (!ToolType.IsImmovable(tool.EntityType))
			{
				num += tool.Bulk;
			}
		}
		return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(num);
	}

	private bool IsValidTool(EntityID toolID, float? jobDurationInDays, SharedKnowledge sharedKnowledge, EntityGroup ownerOfTools, out bool inUseByNonWorkerProcess, ref bool anyToolInUse, ref bool anyToolIsBroken)
	{
		bool isBroken;
		bool result = IsValidPlaysiteTool(toolID, jobDurationInDays, sharedKnowledge, ownerOfTools, out inUseByNonWorkerProcess, out isBroken);
		anyToolInUse |= inUseByNonWorkerProcess;
		anyToolIsBroken |= isBroken;
		return result;
	}

	private bool IsValidPlaysiteTool(EntityID toolID, float? jobDurationInDays, SharedKnowledge sharedKnowledge, EntityGroup ownerOfTools, out bool inUseByNonWorkerProcess, out bool isBroken)
	{
		inUseByNonWorkerProcess = false;
		isBroken = false;
		if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, toolID, ownerOfTools, out var entityData))
		{
			return false;
		}
		bool flag = ToolType.IsImmovable(entityData.EntityType);
		bool flag2 = entityData.EntityType.IsIntrinsic();
		bool num = entityData.EntityType.IsMountable();
		bool flag3 = entityData.EntityType.Upgrader != null;
		bool flag4 = false;
		if (num)
		{
			flag4 = EvaluateAttackJobs.IsCarriedByEntityWhoIsOccupied(entityData, out var itemIsInvalidOrDestroyed);
			if (itemIsInvalidOrDestroyed)
			{
				return false;
			}
		}
		if (flag3)
		{
			if (!entityData.ContainedBy.HasValue)
			{
				return false;
			}
			if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityData.ContainedBy.Value, ownerOfTools, out var entityData2))
			{
				return false;
			}
			if (!Entity.IsFunctional(entityData2))
			{
				return false;
			}
		}
		if (!IsInUseByNonWorkerProcess(entityData, out inUseByNonWorkerProcess))
		{
			return false;
		}
		if (inUseByNonWorkerProcess)
		{
			return false;
		}
		if (!(flag || flag2) && (flag4 || entityData.PartOfID.HasValue || !entityData.NotOnboardDrivenVehicle || !entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(entityData.Bulk)))
		{
			return false;
		}
		bool flag5 = !jobDurationInDays.HasValue || HasEnergyForTool(entityData, jobDurationInDays.Value, ownerOfJobs, null, cachedToolEnergyAvailableStates, sharedKnowledge);
		isBroken = !Entity.IsFunctional(entityData);
		if (flag5 && GoalEvaluator.IsOnPlaySite(entityData) && !isBroken)
		{
			return entityData.IsCompleted();
		}
		return false;
	}

	public static Job ResolveAssignedToJob(IKnownEntityData entityData)
	{
		Job job = null;
		if (entityData.AssignedToJob.HasValue)
		{
			job = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
			if (job == null)
			{
				entityData.AssignedToJob = null;
			}
		}
		return job;
	}

	public static bool ResolveAssignedToProcessJob(IKnownEntityData entityData, out ProcessJob processJob)
	{
		Job job = null;
		processJob = null;
		if (entityData.AssignedToJob.HasValue)
		{
			job = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
			if (job != null)
			{
				processJob = job as ProcessJob;
				return processJob != null;
			}
			entityData.AssignedToJob = null;
		}
		return false;
	}

	public static bool IsInUseByNonWorkerProcess(IKnownEntityData tool, out bool isInUseByNonWorkerProcess)
	{
		isInUseByNonWorkerProcess = false;
		if (tool.AssignedToJob.HasValue && ResolveAssignedToProcessJob(tool, out var processJob) && processJob.ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded)
		{
			if (!processJob.IsStarted(out var isStarted))
			{
				return false;
			}
			if (isStarted)
			{
				isInUseByNonWorkerProcess = true;
			}
		}
		return true;
	}

	private bool HasAllTools(List<Tuple<EntityType, float>> listOfTools)
	{
		bool flag;
		foreach (Tuple<EntityType, float> listOfTool in listOfTools)
		{
			if (listOfTool.Item1.IsMountable())
			{
				bool? canMountTools = entity.EntityType.IntelligenceType.CanMountTools;
				flag = true;
				if (canMountTools != flag)
				{
					flag = false;
					goto IL_00e6;
				}
			}
			ToolHandlingType? toolHandling = listOfTool.Item1.ToolType.ToolHandling;
			ToolHandlingType toolHandlingType = ToolHandlingType.Intrinsic;
			if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && (entityIntelligence.IntrinsicTools == null || !entityIntelligence.IntrinsicTools.ContainsKey(listOfTool.Item1)))
			{
				flag = false;
			}
			else
			{
				if (GetOwnedTools(listOfTool.Item1, out var _, null))
				{
					continue;
				}
				flag = false;
			}
			goto IL_00e6;
		}
		return true;
		IL_00e6:
		return flag;
	}

	public static bool HasEnergyForTool(IKnownEntityData tool, float jobDurationInDays, EntityGroup ownerOfItems, List<EntityGroup> ownersOfItems, Dictionary<EntityType, ReplenishStatus> cachedToolEnergyAvailableStates, SharedKnowledge sharedKnowledge)
	{
		bool flag = true;
		flag = tool.HasEnergyForDuration(jobDurationInDays);
		if (!flag)
		{
			if (!cachedToolEnergyAvailableStates.TryGetValue(tool.EntityType, out var value))
			{
				value = new ReplenishStatus
				{
					DurationInDays = jobDurationInDays
				};
				value.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDurationInDays, ownerOfItems, ownersOfItems, sharedKnowledge);
				flag = value.OwnsItem.Value;
				cachedToolEnergyAvailableStates.Add(tool.EntityType, value);
			}
			else if (jobDurationInDays <= value.DurationInDays)
			{
				flag = value.OwnsItem.Value;
			}
			else
			{
				value.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDurationInDays, ownerOfItems, ownersOfItems, sharedKnowledge);
				value.DurationInDays = jobDurationInDays;
				cachedToolEnergyAvailableStates[tool.EntityType] = value;
			}
		}
		return flag;
	}

	public static bool HasEnergyForToolType(EntityType tool, float durationInDays, EntityGroup ownerOfItem, List<EntityGroup> ownersOfItems, SharedKnowledge sharedKnowledge)
	{
		if (ownerOfItem != null && HasEnergyForToolType(tool, durationInDays, ownerOfItem, sharedKnowledge))
		{
			return true;
		}
		if (ownersOfItems != null)
		{
			foreach (EntityGroup ownersOfItem in ownersOfItems)
			{
				if (ownersOfItem.Items.Count > 0 && HasEnergyForToolType(tool, durationInDays, ownersOfItem, sharedKnowledge))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasEnergyForToolType(EntityType tool, float durationInDays, EntityGroup ownerOfItems, SharedKnowledge sharedKnowledge, bool giveClientFeedback = true)
	{
		bool flag = false;
		if (tool.ContainerType != null && tool.ContainerType.GetRequiresReplenishType() != null)
		{
			RequiresFuelType requiresFuelType = tool.ContainerType.GetRequiresReplenishType().RequiresFuelType;
			if (requiresFuelType != null)
			{
				float num = requiresFuelType.BurnRatePerDay * durationInDays;
				float num2 = 0f;
				foreach (EntityType fuelEntityType in requiresFuelType.FuelEntityTypes)
				{
					if (ownerOfItems.Items.TryGetValue(fuelEntityType, out var value))
					{
						for (int num3 = value.Count - 1; num3 >= 0; num3--)
						{
							EntityID entityID = value[num3];
							if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, ownerOfItems, out var entityData) && entityData.IsCompleted() && Entity.IsOnPlaySite(entityData))
							{
								num2 += entityData.Bulk;
								if (num2 >= num)
								{
									flag = true;
									break;
								}
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				flag = ((num2 >= num) ? true : false);
				if (giveClientFeedback)
				{
					Expedition expedition = ownerOfItems.GetExpedition();
					if (expedition != null)
					{
						The.Client.SetToolReplenishStatusItemOwned(expedition, tool, flag, num);
					}
				}
				return flag;
			}
		}
		return true;
	}

	private void SetGatherJobTypeComboAsAlreadyAdded(Job job)
	{
		if (job is ProcessJob { HarvestJob: not null } processJob)
		{
			gatherJobTypeComboAlreadyAdded[processJob.HarvestJob.ResourceType.ResourceItemType] = true;
		}
	}

	private bool SkipCombos(ProcessJob harvestJob)
	{
		if (harvestJob.HarvestJob != null)
		{
			if (!harvestJob.HarvestJob.CanBeQueuedInGoal())
			{
				return false;
			}
			return gatherJobTypeComboAlreadyAdded.ContainsKey(harvestJob.HarvestJob.ResourceType.ResourceItemType);
		}
		return false;
	}

	private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> GetReplenishItemsEntityIDs(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> items)
	{
		if (items != null)
		{
			Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> dictionary = new Dictionary<EntityAndRoot, List<ReplenishItemsForAction>>();
			{
				foreach (KeyValuePair<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> item2 in items)
				{
					List<ReplenishItemsForAction> list = new List<ReplenishItemsForAction>();
					foreach (Tuple<ProcessType, List<IKnownEntityData>> item3 in item2.Value)
					{
						ReplenishItemsForAction item = new ReplenishItemsForAction(items: item3.Item2.Select((IKnownEntityData e) => e.EntityID).ToList(), action: item3.Item1);
						list.Add(item);
					}
					dictionary.Add(item2.Key.GetAsEntityAndRoot(), list);
				}
				return dictionary;
			}
		}
		return null;
	}

	public override bool CancelCurrentTakers()
	{
		return CancelEntities(needsToBeCancelled, itemsToBeDropped);
	}

	public static bool IsJobValid(Job job)
	{
		if (job.ID == JobID.Invalid)
		{
			return false;
		}
		return true;
	}

	public override bool CanTakeGoal()
	{
		if (bestCombo != null)
		{
			if (!IsJobValid(bestCombo.Job))
			{
				return false;
			}
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			if (GetJobsToCancel(bestCombo) && GoalEvaluator.IsScoreBetterThanAllInvolveds(bestCombo.Score, needsToBeCancelled))
			{
				return true;
			}
		}
		return false;
	}

	public override void PreSetGoal()
	{
		base.PreSetGoal();
		if (bestCombo == null)
		{
			return;
		}
		if (bestCombo.ToolsAndWeapons != null)
		{
			foreach (EntityID toolsAndWeapon in bestCombo.ToolsAndWeapons)
			{
				entityIntelligence.Memory.SetNeededItemForSwitchedGoal(toolsAndWeapon);
			}
		}
		if (bestCombo.ReplenishItemsForTools == null)
		{
			return;
		}
		foreach (KeyValuePair<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTool in bestCombo.ReplenishItemsForTools)
		{
			foreach (Tuple<ProcessType, List<IKnownEntityData>> item in replenishItemsForTool.Value)
			{
				foreach (IKnownEntityData item2 in item.Item2)
				{
					entityIntelligence.Memory.SetNeededItemForSwitchedGoal(item2.EntityID);
				}
			}
		}
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		if (bestCombo != null)
		{
			Job job = bestCombo.Job;
			List<IKnownEntityData> list = new List<IKnownEntityData>();
			foreach (EntityID toolsAndWeapon in bestCombo.ToolsAndWeapons)
			{
				entityIntelligence.GetKnownData(toolsAndWeapon, out var data);
				if (data != null)
				{
					list.Add(data);
					continue;
				}
				return false;
			}
			if (job is ProcessJob processJob)
			{
				if (processJob.BuildingJob != null)
				{
					entityIntelligence.SetTopLevelGoal(new GoalConstruct(entity, processJob, GetOwnerIDs(vehicleGroups), bestCombo.ToolsAndWeapons, GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.ToolTypeCombination)
					{
						GoalEvaluator = this
					}, bestScore);
				}
				else if (processJob.HarvestJob != null)
				{
					entityIntelligence.SetTopLevelGoal(new GoalHarvest(entity, processJob, ownerOfNewProducts, GetOwnerIDs(vehicleGroups), bestCombo.ToolsAndWeapons, GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.ToolTypeCombination)
					{
						GoalEvaluator = this
					}, bestScore);
				}
				else
				{
					EntityID? inputItem = null;
					if (bestCombo.JobData.InputItem != null)
					{
						inputItem = bestCombo.JobData.InputItem.EntityID;
					}
					entityIntelligence.SetTopLevelGoal(new GoalProduce(entity, processJob, ownerOfNewProducts, GetOwnerIDs(vehicleGroups), bestCombo.ToolsAndWeapons, GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.ToolTypeCombination, inputItem)
					{
						GoalEvaluator = this
					}, bestScore);
				}
			}
			else if (job is FindPreyJob)
			{
				FindPreyJob job2 = job as FindPreyJob;
				entityIntelligence.SetTopLevelGoal(new GoalFindPrey(entity, job2, GetOwnerIDs(vehicleGroups), list[0].GetAsEntityAndRoot(), GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.JobData.Location.Value)
				{
					GoalEvaluator = this
				}, bestScore);
			}
			else if (job is CombatAreaJob)
			{
				CombatAreaJob job3 = job as CombatAreaJob;
				entityIntelligence.SetTopLevelGoal(new GoalPatrol(entity, job3, GetOwnerIDs(vehicleGroups), list[0].GetAsEntityAndRoot(), GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.JobData.Location.Value)
				{
					GoalEvaluator = this
				}, bestScore);
			}
			else if (job is CheckProcessJob)
			{
				CheckProcessJob job4 = job as CheckProcessJob;
				entityIntelligence.SetTopLevelGoal(new GoalChecking(entity, job4, GetOwnerIDs(vehicleGroups))
				{
					GoalEvaluator = this
				}, bestScore);
			}
			return true;
		}
		return false;
	}
}
