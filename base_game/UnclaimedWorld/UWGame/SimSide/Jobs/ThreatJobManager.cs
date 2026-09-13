using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Jobs;

public class ThreatJobManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	public enum ThreatEvaluationStatus
	{
		Done,
		Processing
	}

	private enum Phase
	{
		CleanupJobs,
		CreateThreatJobs
	}

	public enum SpeciesThreatLevel
	{
		NoThreat,
		ThreatToYoung
	}

	private Regulator regulator;

	private Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	private Phase phase;

	public static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private const float aggroFalloffRegion = 0.25f;

	private static Vector2[] aggroScoreFunctionPoints = new Vector2[3]
	{
		new Vector2(0f, 1f),
		new Vector2(0.75f, 1f),
		new Vector2(1f, 0f)
	};

	private MethodID setToNotWaitingID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused { get; private set; }

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public double? UpdateInterval => 1.0;

	public CyclableID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public ThreatJobManager()
	{
	}

	public ThreatJobManager(Allegiance allegiance)
	{
		this.allegiance = allegiance;
		AddToLookup();
		setToNotWaitingID = ActionLookup.AddWithNewID(SetToNotWaiting);
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "ThreatJobManager");
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
				phase = Phase.CleanupJobs;
			}
		}
	}

	public void Destroy()
	{
		if (The.Sim.CycleManager.IsRegistered(this))
		{
			The.Sim.CycleManager.UnRegister(this);
		}
		ActionLookup.Remove(setToNotWaitingID);
		RemoveIDEntry();
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public void CreateThreatJobsFromAttackOutOfBand(Entity attacker, Entity attacked)
	{
		if (GetThreatJobIfExists(attacker) == null)
		{
			float? maximumAggroRange = attacked.Intelligence.Allegiance.GetMaximumAggroRange();
			HandleEntity(attacker, maximumAggroRange);
		}
	}

	public void CreateCombosForAttacksOutOfBand()
	{
	}

	public void ScoreCombosAndCreateJobsOutOfBand()
	{
	}

	private ThreatJob GetThreatJobIfExists(Entity entity)
	{
		if (allegiance.SharedKnowledge.AllKnownEntities.ThreatJobsByTarget.TryGetValue(entity.ID, out var value))
		{
			return (ThreatJob)value;
		}
		if (allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobsByTarget.TryGetValue(entity.ID, out value))
		{
			return (ThreatJob)value;
		}
		return null;
	}

	private static double ScorePolicy(Entity entity, Allegiance thisAllegiance)
	{
		if (thisAllegiance.Policy.PolicyTowardsCreatures.TryGetValue(entity.EntityType, out var value))
		{
			switch (value)
			{
			case AllegiancePolicy.CreaturePolicy.HuntToDestroy:
				return 1.0;
			case AllegiancePolicy.CreaturePolicy.NeverAttack:
				return 0.0;
			case AllegiancePolicy.CreaturePolicy.HuntForProducts:
				return 0.0;
			}
		}
		return 0.0;
	}

	private ThreatEvaluationStatus ScoreNearness(Entity entity, bool isVermin, float? allegianceAggroRange, ref EntityID? closestMemberOfAllegiance, out double nearnessScore, out bool isInAttackZone)
	{
		bool flag = entity.CanDefendItself();
		bool isOnlyVermin = isVermin && !flag;
		float? maxDistanceFromExpeditionsToHuntVermin = allegiance.RepresentativeEntityType.IntelligenceType.MaxDistanceFromExpeditionsToHuntVermin;
		if (ScoreDistanceToPatrolZones(entity, isOnlyVermin, out var distanceToPatrolZoneScore, out isInAttackZone) == ThreatEvaluationStatus.Processing)
		{
			nearnessScore = 0.0;
			return ThreatEvaluationStatus.Processing;
		}
		if (!allegiance.RepresentativeEntityType.IntelligenceType.AggroRange.HasValue)
		{
			nearnessScore = 1.0;
			return ThreatEvaluationStatus.Done;
		}
		bool otherAgentsNearExpeditionCenterAreConsideredThreats = allegiance.RepresentativeEntityType.IntelligenceType.OtherAgentsNearExpeditionCenterAreConsideredThreats;
		int num = ((!otherAgentsNearExpeditionCenterAreConsideredThreats) ? 2 : 3);
		double distanceToExpeditionScore = 0.0;
		float? closestExpeditionDistance = null;
		if (otherAgentsNearExpeditionCenterAreConsideredThreats && ScoreDistanceToExpeditionCenter(entity, allegianceAggroRange, isVermin, out closestExpeditionDistance, out distanceToExpeditionScore) == ThreatEvaluationStatus.Processing)
		{
			nearnessScore = 0.0;
			return ThreatEvaluationStatus.Processing;
		}
		if (ScoreClosestDistanceWithinAggroRange(entity, allegianceAggroRange, isOnlyVermin, closestExpeditionDistance, maxDistanceFromExpeditionsToHuntVermin, ref closestMemberOfAllegiance, out var highestAggroScore) == ThreatEvaluationStatus.Processing)
		{
			nearnessScore = 0.0;
			return ThreatEvaluationStatus.Processing;
		}
		if (isInAttackZone)
		{
			nearnessScore = 1.0;
		}
		else
		{
			nearnessScore = highestAggroScore + distanceToExpeditionScore + distanceToPatrolZoneScore / (double)num;
		}
		nearnessScore = Common.ClampTop(nearnessScore, 1.0);
		return ThreatEvaluationStatus.Done;
	}

	public ThreatEvaluationStatus ScoreDistanceToPatrolZones(Entity entityToCheckDistanceTo, bool isOnlyVermin, out double distanceToPatrolZoneScore, out bool isInAttackZone)
	{
		isInAttackZone = false;
		foreach (Expedition expedition in allegiance.Expeditions)
		{
			foreach (Job patrolJob2 in expedition.OwnedEntities.PatrolJobs)
			{
				PatrolJob patrolJob = patrolJob2 as PatrolJob;
				if (patrolJob.ThreatArea.Contains(entityToCheckDistanceTo.MapPosition.Value) && (!isOnlyVermin || patrolJob.AttackVermin))
				{
					distanceToPatrolZoneScore = 1.0;
					return ThreatEvaluationStatus.Done;
				}
			}
			foreach (Job attackAreaJob2 in expedition.OwnedEntities.AttackAreaJobs)
			{
				AttackAreaJob attackAreaJob = attackAreaJob2 as AttackAreaJob;
				if (attackAreaJob.ThreatArea.Contains(entityToCheckDistanceTo.MapPosition.Value) && (!isOnlyVermin || attackAreaJob.AttackVermin))
				{
					isInAttackZone = true;
					distanceToPatrolZoneScore = 1.0;
					return ThreatEvaluationStatus.Done;
				}
			}
		}
		distanceToPatrolZoneScore = 0.0;
		return ThreatEvaluationStatus.Done;
	}

	public ThreatEvaluationStatus ScoreDistanceToExpeditionCenter(Entity entity, float? allegianceAggroRange, bool isVermin, out float? closestExpeditionDistance, out double distanceToExpeditionScore)
	{
		closestExpeditionDistance = null;
		if (!allegianceAggroRange.HasValue)
		{
			distanceToExpeditionScore = 1.0;
			return ThreatEvaluationStatus.Done;
		}
		float value = allegianceAggroRange.Value;
		RegionMap regionMap = null;
		regionMap = allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, allegiance.RepresentativeEntityType, ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
		Vector3 value2 = entity.AccessPoint.Value;
		float num = 10000000f;
		foreach (Expedition expedition in allegiance.Expeditions)
		{
			Vector3 value3 = expedition.Center.Value;
			Point fromSubtile = MapManager.WorldPosToSubtile(value2);
			Point toSubtile = MapManager.WorldPosToSubtile(value3);
			float distance = 0f;
			if (regionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity: false, setToNotWaitingID) == RegionMap.Result.Wait)
			{
				distanceToExpeditionScore = 0.0;
				return ThreatEvaluationStatus.Processing;
			}
			if (distance < num)
			{
				num = distance;
				closestExpeditionDistance = distance;
			}
		}
		if (num <= value)
		{
			double num2 = num / value;
			distanceToExpeditionScore = 1.0 - num2;
		}
		else
		{
			distanceToExpeditionScore = 0.0;
		}
		return ThreatEvaluationStatus.Done;
	}

	private ThreatEvaluationStatus ScoreClosestDistanceWithinAggroRange(Entity entityToCheckDistanceTo, float? maximumAggroRange, bool isOnlyVermin, float? distanceToExpedition, float? maxDistanceToHuntVermin, ref EntityID? closestAllegianceMember, out double highestAggroScore)
	{
		if (!maximumAggroRange.HasValue)
		{
			highestAggroScore = 1.0;
			return ThreatEvaluationStatus.Done;
		}
		if (isOnlyVermin && maxDistanceToHuntVermin.HasValue && distanceToExpedition > maxDistanceToHuntVermin.Value)
		{
			highestAggroScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		highestAggroScore = 0.0;
		float num = 100000f;
		float distance = 1000000f;
		List<Pair<Entity, Vector2>> resultsList = null;
		Predicate<Entity> filter = (Entity entityToCheck) => allegiance.Members.Contains(entityToCheck);
		The.AgentQuadTree.GetEntitiesInRange(entityToCheckDistanceTo.PlaySiteLocation.ToVector2(), maximumAggroRange.Value, filter, ref resultsList);
		if (resultsList == null || resultsList.Count == 0)
		{
			highestAggroScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		foreach (Pair<Entity, Vector2> item in resultsList)
		{
			float? aggroRange = item.First.GetAggroRange();
			if (aggroRange.HasValue)
			{
				if (item.First.GetAttackRange(out var _, out var _) > 0f)
				{
					distance = Common.DistanceOctile(item.First.PlaySiteLocation, entityToCheckDistanceTo.PlaySiteLocation);
				}
				else if (The.Map.FootTerrainRegionMap.GetDistanceToEntity(item.First, entityToCheckDistanceTo, item.First, ref distance, null, null, sendMessageToEntity: false, setToNotWaitingID) == RegionMap.Result.Wait)
				{
					highestAggroScore = 0.0;
					return ThreatEvaluationStatus.Processing;
				}
				if (distance < num)
				{
					closestAllegianceMember = item.First.EntityID;
					num = distance;
				}
				float num2 = ((!(distance < aggroRange.Value)) ? 0f : ((!isOnlyVermin || !maxDistanceToHuntVermin.HasValue || !(distance > maxDistanceToHuntVermin.Value)) ? Common.GetInterpolatedFunctionValue(distance / aggroRange.Value, aggroScoreFunctionPoints) : 0f));
				if ((double)num2 > highestAggroScore)
				{
					highestAggroScore = num2;
				}
			}
		}
		return ThreatEvaluationStatus.Done;
	}

	public void SetToNotWaiting()
	{
		IsPaused = false;
	}

	private static ThreatEvaluationStatus ScoreAggressionOfEntity(Entity entity, Allegiance thisAllegiance, out double aggressionScore)
	{
		if (thisAllegiance.WasRecentlyAttackedBy(entity))
		{
			aggressionScore = 1.0;
			return ThreatEvaluationStatus.Done;
		}
		aggressionScore = 0.0;
		return ThreatEvaluationStatus.Done;
	}

	private ThreatEvaluationStatus ScoreCommonThreat(Entity entity, double nearnessScore, ref double threatScore)
	{
		entity.GetStatus(out var isDead, out var _, out var _, out var _);
		if (isDead)
		{
			threatScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		if (Common.IsZero(nearnessScore))
		{
			threatScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		threatScore = 1.0;
		return ThreatEvaluationStatus.Done;
	}

	private ThreatEvaluationStatus ScoreThreatToAgents(Entity entity, double nearnessScore, bool isInAttackZone, ref double threatScore)
	{
		bool flag = entity.CanDefendItself();
		if (!allegiance.RepresentativeEntityType.IntelligenceType.WillAttackNonThreatsNearby)
		{
			if (!flag)
			{
				threatScore = 0.0;
				return ThreatEvaluationStatus.Done;
			}
			if (!isInAttackZone && entity.IsFleeing() && !entity.EntityType.IntelligenceType.IsPredator)
			{
				threatScore = 0.0;
				return ThreatEvaluationStatus.Done;
			}
		}
		if (entity.EntityType.BiologicalType == null && !flag)
		{
			threatScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		if (isInAttackZone)
		{
			threatScore = 1.0;
			return ThreatEvaluationStatus.Done;
		}
		if (ScoreAggressionOfEntity(entity, allegiance, out var aggressionScore) == ThreatEvaluationStatus.Processing)
		{
			return ThreatEvaluationStatus.Processing;
		}
		if ((entity.EntityType.IntelligenceType == null || !entity.EntityType.IntelligenceType.IsPredator) && Common.IsZero(aggressionScore) && (!allegiance.RepresentativeEntityType.IntelligenceType.IsPredator || !allegiance.RepresentativeEntityType.IntelligenceType.WillAttackNonThreatsNearby) && !allegiance.RepresentativeEntityType.BiologicalType.IsTerritorial)
		{
			threatScore = 0.0;
			return ThreatEvaluationStatus.Done;
		}
		threatScore = GameData.Instance.AIConstants.AggressionScoreFraction * aggressionScore + GameData.Instance.AIConstants.NearnessScoreFraction * nearnessScore;
		return ThreatEvaluationStatus.Done;
	}

	private ThreatEvaluationStatus ScoreThreatToAssets(Entity entity, bool isVermin, bool isInAttackZone, ref double threatRating)
	{
		if (isVermin)
		{
			if (isInAttackZone)
			{
				threatRating = 1.0;
			}
			else
			{
				threatRating = 0.10000000149011612;
			}
		}
		return ThreatEvaluationStatus.Done;
	}

	public void GetSpeciesThreatLevel()
	{
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"ThreatJobManager {ID}: {phase}");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.CleanupJobs:
			phase = Phase.CreateThreatJobs;
			break;
		case Phase.CreateThreatJobs:
			if (allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite.Count > 0)
			{
				float? maximumAggroRange = allegiance.GetMaximumAggroRange();
				List<EntityID> list = null;
				foreach (KeyValuePair<EntityID, EntityID> item in allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite)
				{
					if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(item.Key, out var data)))
					{
						if (data is Entity entity && HandleEntity(entity, maximumAggroRange) == ThreatEvaluationStatus.Processing)
						{
							return true;
						}
					}
					else
					{
						Common.AddToList(ref list, item.Key);
					}
				}
				allegiance.SharedKnowledge.RemoveInvalidEntityIDs(list);
			}
			phase = Phase.CleanupJobs;
			return true;
		}
		return false;
	}

	private ThreatEvaluationStatus HandleEntity(Entity entity, float? allegianceAggroRange)
	{
		EntityID? closestMemberOfAllegiance = null;
		bool isVermin = false;
		if (entity.EntityType.BiologicalType != null && entity.EntityType.BiologicalType.IsVermin)
		{
			isVermin = true;
		}
		if (ScoreNearness(entity, isVermin, allegianceAggroRange, ref closestMemberOfAllegiance, out var nearnessScore, out var isInAttackZone) == ThreatEvaluationStatus.Processing)
		{
			return ThreatEvaluationStatus.Processing;
		}
		double threatScore = 0.0;
		if (ScoreCommonThreat(entity, nearnessScore, ref threatScore) == ThreatEvaluationStatus.Processing)
		{
			IsPaused = true;
			return ThreatEvaluationStatus.Processing;
		}
		double threatScore2 = 0.0;
		bool flag = false;
		if (!Common.IsZero(threatScore))
		{
			if (ScoreThreatToAgents(entity, nearnessScore, isInAttackZone, ref threatScore2) == ThreatEvaluationStatus.Processing)
			{
				IsPaused = true;
				return ThreatEvaluationStatus.Processing;
			}
			if (threatScore2 < GameData.Instance.AIConstants.MinimumThreatRatingToBeAThreatToAgents)
			{
				threatScore2 = 0.0;
			}
			if (Common.IsZero(threatScore2))
			{
				if (ScoreThreatToAssets(entity, isVermin, isInAttackZone, ref threatScore2) == ThreatEvaluationStatus.Processing)
				{
					IsPaused = true;
					return ThreatEvaluationStatus.Processing;
				}
				if (Common.IsGreaterThan(threatScore2, 0.0))
				{
					flag = true;
				}
			}
		}
		ThreatJob threatJobIfExists = GetThreatJobIfExists(entity);
		if (threatScore2 > 0.0)
		{
			if (threatJobIfExists == null)
			{
				threatJobIfExists = new ThreatJob(entity, allegiance.SharedKnowledge.AllKnownEntities, flag)
				{
					ThreatRating = threatScore2,
					ClosestMemberOfAllegiance = closestMemberOfAllegiance
				};
			}
			else
			{
				threatJobIfExists.ThreatRating = threatScore2;
				threatJobIfExists.IsVermin = flag;
				threatJobIfExists.ClosestMemberOfAllegiance = closestMemberOfAllegiance;
			}
		}
		else
		{
			threatJobIfExists?.Destroy(cancelTakers: true);
		}
		return ThreatEvaluationStatus.Done;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		setToNotWaitingID = sn.DoEnum(setToNotWaitingID);
		IsPaused = sn.DoBool(IsPaused);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(allegiance);
		sn.Ignore(phase);
		sn.Ignore(aggroScoreFunctionPoints);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
		ActionLookup.Add(setToNotWaitingID, SetToNotWaiting);
		CreateRegulators();
	}
}
