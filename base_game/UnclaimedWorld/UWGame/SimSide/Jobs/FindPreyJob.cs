using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class FindPreyJob : Job, IRequiresWeapon
{
	public Zone Zone;

	private ZoneID? snapshotZone;

	private bool weaponsAreAvailable = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool WeaponsAreAvailable
	{
		get
		{
			return weaponsAreAvailable;
		}
		set
		{
			weaponsAreAvailable = value;
		}
	}

	public ItemType.TaskType TaskType => ItemType.TaskType.UnspecifiedHunting;

	public FindPreyJob()
	{
	}

	public FindPreyJob(Zone zone, EntityGroup entityGroup)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Zone = zone;
		zone.ZoneHunt.FindPreyJobs.Add(this);
		entityGroup.AddJob(this);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override Vector3? GetCircaLocation()
	{
		return Zone?.MapArea?.GetCenter();
	}

	public GoalEvaluator.CalculateResult ScoreWeapon(Entity entity, IKnownEntityData weapon, Vector3 fromLocation, Vector3 LocationToGetTo, double? ageContribution, double? timeContribution, Intelligence entityIntelligence, RegionMap regionMap, float priority, out double score)
	{
		score = 0.0;
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		if (ProcessJob.ScoreToolLocation(entity, weapon, fromLocation, LocationToGetTo, sharedKnowledge, regionMap, out var _, out var score2) == GoalEvaluator.CalculateResult.Processing)
		{
			return GoalEvaluator.CalculateResult.Processing;
		}
		double value = weapon.Condition.Value;
		double num = weapon.EntityType.ItemType.GetTaskAppropriateLevel(TaskType);
		if (Common.IsGreaterThan(num, 0.0))
		{
			score = 0.5 * score2 + 0.20000000298023224 * value + 0.30000001192092896 * num;
		}
		if (Common.IsZero(score))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		score = GoalEvaluator.AddTimeAgeAndPriority(score, timeContribution.Value, ageContribution.Value, priority);
		return GoalEvaluator.CalculateResult.Done;
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
	{
		if (entity.EntityType.IntelligenceType.CanHunt != false)
		{
			float? distanceToClosestTile;
			TerrainTile closestTile;
			switch (Zone.GetClosestSafeEdgeTile(this, entity.PlaySiteLocation, regionMap, threatStance, entity, out distanceToClosestTile, out closestTile, ownerOfJobs))
			{
			case RegionMap.Result.Wait:
				return GoalEvaluator.CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				return GoalEvaluator.CalculateResult.Done;
			}
			if (closestTile == null)
			{
				return GoalEvaluator.CalculateResult.Done;
			}
			currentJobLocation = MapManager.TileToWorldPos(closestTile);
			double travelScoreFromDistance = GoalEvaluator.GetTravelScoreFromDistance(entity, distanceToClosestTile.Value);
			double num = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);
			double num2 = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["hunting"]));
			double num3 = ScorePreyImportance(ownerOfJobs);
			EvaluatorWeights evaluatorWeights = GameData.Instance.AIConstants.EvaluatorWeights;
			double num4 = (double)evaluatorWeights.FindPreyTravelTimeWeight * travelScoreFromDistance + (double)evaluatorWeights.FindPreySkillWeight * num2 + (double)evaluatorWeights.FindPreyImportanceWeight * num3 + 0.2 * num;
			rating = num4;
			rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		}
		else
		{
			rating = 0.0;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	public double ScorePreyImportance(EntityGroup owner)
	{
		if (Zone.ZoneHunt.CreaturesToHunt != null)
		{
			float num = 0f;
			foreach (KeyValuePair<EntityType, int> item in Zone.ZoneHunt.CreaturesToHunt)
			{
				if (item.Value > 0)
				{
					float importance = owner.GetImportance(item.Key.BiologicalType.CarcassType);
					num = Math.Max(num, importance);
				}
			}
			return num;
		}
		return 0.5;
	}

	public double CombineJobAndWeaponScore(double jobScore, double weaponScore)
	{
		if (Common.IsZero(jobScore) || Common.IsZero(weaponScore))
		{
			return 0.0;
		}
		return 0.9 * jobScore + 0.1 * weaponScore;
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
			double score = 1.0;
			if (toolParams.HasValue && toolParams.Value.Tools.Count > 0)
			{
				entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(toolParams.Value.Tools[0], out var data);
				if (!jobData.Location.HasValue)
				{
					jobIsValid = false;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (ScoreWeapon(entity, data, entity.PlaySiteLocation, jobData.Location.Value, ageContribution, timeContribution, entityIntelligence, regionMap, priority, out score) == GoalEvaluator.CalculateResult.Processing)
				{
					return GoalEvaluator.CalculateResult.Processing;
				}
			}
			rating = CombineJobAndWeaponScore(jobData.JobScore, score);
			EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating);
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	public override string GetName()
	{
		return "Finding prey";
	}

	public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
	{
		if (Zone != null)
		{
			Zone.RemoveFindPreyJob(this);
		}
		base.Destroy(removeTakers, entityToExcludeFromCancel);
	}

	public override bool UserCanCancel(out string reason)
	{
		reason = "";
		return false;
	}

	public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tile = null;
		targetEntity = null;
		zoneID = null;
		if (Zone != null)
		{
			zoneID = Zone.ID;
		}
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
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);
		weaponsAreAvailable = sn.DoBool(weaponsAreAvailable);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
	}
}
