using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class CheckProcessJob : Job
{
	public Vector3 Location;

	public List<JobID> ProcessJobsToCheckOn = new List<JobID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public CheckProcessJob(Vector3 location, EntityGroup entityGroup)
		: base(entityGroup)
	{
		Location = location;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public CheckProcessJob()
	{
	}

	public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tile = null;
		targetEntity = null;
		zoneID = null;
		tile = MapManager.WorldPosToTile(Location);
	}

	public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref EvaluateJob.ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams, ref double rating, ref bool jobIsValid, Dictionary<Job, EvaluateJob.ToolOrWeaponInstanceComboJobData> cachedJobScores, Intelligence entityIntelligence, float priority, EntityGroup ownerOfJobs, bool cacheScore)
	{
		if (!cachedJobScores.TryGetValue(this, out jobData))
		{
			jobData = new EvaluateJob.ToolOrWeaponInstanceComboJobData();
			if (ScoreThisJobWithoutWeapons(regionMap, threatStance, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData.JobScore, priority, ref jobData.Location, ownerOfJobs) != GoalEvaluator.CalculateResult.Done)
			{
				return GoalEvaluator.CalculateResult.Processing;
			}
			if (cacheScore)
			{
				cachedJobScores.Add(this, jobData);
			}
		}
		if (jobData.JobScore > 0.0)
		{
			rating = 0.699999988079071 * jobData.JobScore;
			EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating);
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
	{
		if (entity.EntityType.IntelligenceType.CanCheckProgress != false)
		{
			double travelTimeScore = 0.0;
			switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStance, entity.AccessPoint.Value, Location, entity, ref travelTimeScore, this, ownerOfJobs.Parent))
			{
			case RegionMap.Result.Wait:
				return GoalEvaluator.CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);
			double num = travelTimeScore;
			rating = num;
			rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		}
		else
		{
			rating = 0.0;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	public override Vector3? GetCircaLocation()
	{
		return Location;
	}

	public override string GetName()
	{
		return "Checking progress";
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
		Location = sn.DoVector3(Location);
		ProcessJobsToCheckOn = sn.DoList(ProcessJobsToCheckOn);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
	}
}
