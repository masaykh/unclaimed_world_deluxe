using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public abstract class CombatAreaJob : Job, IRequiresWeapon, IVariableMaxTakerJob
{
	public Zone Zone;

	private ZoneID? snapshotZone;

	public Rectangle ThreatArea;

	private List<Tuple<EntityID, double>> lastAgentsOnPosts = new List<Tuple<EntityID, double>>();

	private int maxTakers;

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

	public abstract bool CanAttackVermin { get; }

	public abstract bool CanAttackTargetsOutsideZone { get; }

	public override int MaxJobPositions => maxTakers;

	public ItemType.TaskType TaskType => ItemType.TaskType.PatrolOrAttack;

	public override bool RequiresBoldStance => true;

	public CombatAreaJob()
	{
	}

	public CombatAreaJob(Zone zone, EntityGroup entityGroup, int noOfPatrollers)
		: base(entityGroup)
	{
		Zone = zone;
		maxTakers = noOfPatrollers;
		Rectangle value = zone.MapArea.BoundingRectangle.Value;
		int num = (int)Math.Round(GameData.Instance.AIConstants.MaxDistanceOutsidePatrolZoneToChaseTargets * (1f / 48f));
		value.Inflate(num, num);
		ThreatArea = The.Map.GetClampedMapAreaUsingTiles(value);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override Vector3? GetCircaLocation()
	{
		return Zone?.MapArea?.GetCenter();
	}

	public void SetJobPositions(int positions)
	{
		maxTakers = positions;
	}

	public void RemoveLastAgentOnPost(EntityID entity)
	{
		lastAgentsOnPosts.RemoveAll((Tuple<EntityID, double> t) => t.Item1 == entity);
	}

	public void SetLastAgentOnPost(EntityID entity)
	{
		lastAgentsOnPosts.RemoveAll((Tuple<EntityID, double> t) => t.Item1 == entity);
		lastAgentsOnPosts.Add(new Tuple<EntityID, double>(entity, The.Sim.TotalUnPausedGameTimeInSeconds));
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
		if (!ResolveOwner(out var owner))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		double num = weapon.EntityType.ItemType.GetTaskAppropriateLevel(TaskType);
		double valueToTest = 1.0;
		Expedition expedition = owner.GetExpedition();
		if (expedition != null && expedition.Policy != null)
		{
			valueToTest = expedition.Policy.GetWeaponPolicyScore(CanAttackVermin, weapon.EntityType, null);
		}
		if (Common.IsGreaterThan(num, 0.0) && Common.IsGreaterThan(valueToTest, 0.0))
		{
			score = 0.5 * score2 + 0.20000000298023224 * value + 0.30000001192092896 * num;
		}
		if (Common.IsZero(score))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		score = GoalEvaluator.AddTimeAgeAndPriority(score, timeContribution.Value, ageContribution.Value, priority);
		score = Common.Min((float)score, 1f);
		return GoalEvaluator.CalculateResult.Done;
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
	{
		if (entity.EntityType.IntelligenceType.CanPatrol != false)
		{
			if (!EntityIsAllowedToTakeJob(entity))
			{
				return GoalEvaluator.CalculateResult.Done;
			}
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
			double num2 = GoalEvaluator.ScoreUniqueSkillPenalty(entity);
			float mundaneJobSpecialistPenalty = GameData.Instance.AIConstants.EvaluatorWeights.MundaneJobSpecialistPenalty;
			float num3 = ScoreAwayFromPostInertia(entity);
			double num4 = 0.65 * travelScoreFromDistance + 0.2 * num + 0.1 * (double)num3 + (double)mundaneJobSpecialistPenalty * num2;
			rating = num4;
			rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		}
		else
		{
			rating = 0.0;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private float ScoreAwayFromPostInertia(Entity entity)
	{
		CombatAreaJob lastCombatAreaJob = entity.Intelligence.Memory.GetLastCombatAreaJob();
		if (lastCombatAreaJob != null && lastCombatAreaJob.ID == base.ID && !lastCombatAreaJob.TakenBy.Contains(entity))
		{
			return 1f;
		}
		return 0f;
	}

	private List<Tuple<EntityID, double>> GetLastAgentsOnPost()
	{
		for (int num = lastAgentsOnPosts.Count - 1; num >= 0; num--)
		{
			if (Common.TimepointIsOutDated(lastAgentsOnPosts[num].Item2, GameData.Instance.AIConstants.MaxTimeForLeavingPatrolPostUntilFreed))
			{
				lastAgentsOnPosts.RemoveAt(num);
			}
		}
		return lastAgentsOnPosts;
	}

	public override void Abandon(Entity entity, bool isDestroyingJob = false)
	{
		base.Abandon(entity, isDestroyingJob);
	}

	private bool EntityIsAllowedToTakeJob(Entity entity)
	{
		List<Tuple<EntityID, double>> lastAgentsOnPost = GetLastAgentsOnPost();
		if (TakenBy.Contains(entity) || lastAgentsOnPost.Exists((Tuple<EntityID, double> t) => t.Item1 == entity.ID))
		{
			return true;
		}
		if (TakenBy.Count < MaxJobPositions && lastAgentsOnPost.Count((Tuple<EntityID, double> t) => !TakenBy.Contains(t.Item1)) + TakenBy.Count < MaxJobPositions)
		{
			return true;
		}
		return false;
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
		ThreatArea = sn.DoRectangle(ThreatArea);
		lastAgentsOnPosts = sn.DoList(lastAgentsOnPosts);
		weaponsAreAvailable = sn.DoBool(weaponsAreAvailable);
		maxTakers = sn.DoInt32(maxTakers);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
	}
}
