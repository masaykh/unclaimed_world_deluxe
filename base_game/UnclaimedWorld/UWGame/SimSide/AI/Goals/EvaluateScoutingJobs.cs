using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateScoutingJobs : GoalEvaluator, IScoreJob
{
	private enum Progress
	{
		NotStarted,
		ScoreJobs
	}

	private Progress progress;

	private ScoutingJob mostDesirableJob;

	private Vector3? locationInScoutingArea;

	private Dictionary<Job, Job> processedJobs = new Dictionary<Job, Job>();

	private List<EntityGroup> ownersOfVehicles;

	private EntityGroup ownerOfJobs;

	private List<Entity> needsToBeCancelled = new List<Entity>();

	private List<Entity> itemsToBeDropped = new List<Entity>();

	public override float Priority => 1f;

	public EvaluateScoutingJobs(Entity entity, EntityGroup ownerOfJobs, List<EntityGroup> ownersOfVehicles = null)
		: base(entity)
	{
		this.ownerOfJobs = ownerOfJobs;
		this.ownersOfVehicles = ownersOfVehicles;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (progress == Progress.NotStarted)
		{
			mostDesirableJob = null;
			bestScore = minimumRatingToConsider;
			locationInScoutingArea = null;
			if (entity.Find<BiologicalEntity>(out var c) && ScoreAge(c) == 0.0)
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			ScoreTimeOfDay();
			progress = Progress.ScoreJobs;
			processedJobs.Clear();
		}
		if (progress == Progress.ScoreJobs)
		{
			double ageContribution = GetAgeContribution();
			double value = ScoreTimeOfDay();
			foreach (ScoutingJob scoutingJob in ownerOfJobs.ScoutingJobs)
			{
				if (!processedJobs.ContainsKey(scoutingJob))
				{
					if (HandleJob(scoutingJob, ageContribution, value) == CalculateResult.Processing)
					{
						return CalculateResult.Processing;
					}
					processedJobs.Add(scoutingJob, scoutingJob);
				}
			}
			progress = Progress.NotStarted;
			if (mostDesirableJob != null)
			{
				result = bestScore;
				return CalculateResult.Done;
			}
			result = 0.0;
			return CalculateResult.Done;
		}
		result = 0.0;
		return CalculateResult.Done;
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
	{
		Vector3? closestLocation;
		return ScoreThisJob(regionMap, threatStanceToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out rating, out closestLocation);
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, out Vector3? closestLocation)
	{
		rating = 0.0;
		closestLocation = null;
		ScoutingJob scoutingJob = job as ScoutingJob;
		if (!ageContribution.HasValue)
		{
			ageContribution = GetAgeContribution();
		}
		if (!timeContribution.HasValue)
		{
			timeContribution = ScoreTimeOfDay();
		}
		double travelTimeScore = 0.0;
		if (scoutingJob.Location.HasValue)
		{
			Vector3 value = scoutingJob.Location.Value;
			switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, value, entity, ref travelTimeScore, job, ownerOfJobs.Parent))
			{
			case RegionMap.Result.Wait:
				return CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				return CalculateResult.Done;
			}
		}
		else
		{
			float? distanceToClosestTile;
			TerrainTile closestTile;
			switch (scoutingJob.Zone.GetClosestSafeEdgeTile(job, entity.AccessPoint.Value, regionMap, threatStanceToUse, entity, out distanceToClosestTile, out closestTile, ownerOfJobs))
			{
			case RegionMap.Result.Wait:
				return CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				return CalculateResult.Done;
			}
			if (closestTile == null)
			{
				return CalculateResult.Done;
			}
			closestLocation = MapManager.TileToWorldPos(closestTile);
			travelTimeScore = GoalEvaluator.GetTravelScoreFromDistance(entity, distanceToClosestTile.Value);
		}
		double num = GoalEvaluator.ScoreSkill(entityIntelligence.GetSkillValue(GameData.Instance.AllSkillTypes["foraging"]));
		rating = 0.2 * num + 0.8 * travelTimeScore;
		rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, Priority);
		EvaluateJob.ApplyJobPriorityModifier(scoutingJob.Priority, ref rating);
		return CalculateResult.Done;
	}

	private CalculateResult HandleJob(ScoutingJob job, double? ageContribution, double? timeContribution)
	{
		if (entity.EntityType.IntelligenceType.CanExamine != true && job.Examine)
		{
			return CalculateResult.Done;
		}
		if (!entityIntelligence.Brain.IsSame(job))
		{
			ThreatStance threatStanceToUse;
			RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, job, out threatStanceToUse);
			Vector3? closestLocation = null;
			int proposedNumberOfWorkers = Common.Clamp(job.TakenBy.Count + 1, 0, job.MaxJobPositions);
			if (ScoreThisJob(regionMapAndStanceForEvaluator, threatStanceToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out var rating, out closestLocation) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			GetJobsToCancel(job);
			double ourScore = rating;
			if (rating > bestScore && GoalEvaluator.IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled, entity))
			{
				bestScore = rating;
				mostDesirableJob = job;
				locationInScoutingArea = closestLocation;
				if (locationInScoutingArea.HasValue)
				{
					Common.DistanceOctile(locationInScoutingArea.Value, entity.PlaySiteLocation);
					_ = 1500f;
				}
			}
			entityIntelligence.TopScoringJobs.Add(new GoalAndScore
			{
				Score = rating,
				Goal = job.ToString()
			});
		}
		return CalculateResult.Done;
	}

	private void GetJobsToCancel(ScoutingJob job)
	{
		needsToBeCancelled.Clear();
		itemsToBeDropped.Clear();
		List<Entity> list = new List<Entity>();
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
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
	}

	public override bool CancelCurrentTakers()
	{
		return CancelEntities(needsToBeCancelled, itemsToBeDropped);
	}

	public override bool CanTakeGoal()
	{
		if (mostDesirableJob != null)
		{
			if (!EvaluateJob.IsJobValid(mostDesirableJob))
			{
				return false;
			}
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			GetJobsToCancel(mostDesirableJob);
			if (GoalEvaluator.IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
			{
				return true;
			}
		}
		return false;
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		Intelligence intelligence = entity.Intelligence;
		if (entity.Name != null)
		{
			entity.Name.Contains("onlan");
		}
		intelligence.SetTopLevelGoal(new GoalScouting(entity, mostDesirableJob, GetOwnerIDs(ownersOfVehicles), locationInScoutingArea)
		{
			GoalEvaluator = this
		}, bestScore);
		return true;
	}
}
