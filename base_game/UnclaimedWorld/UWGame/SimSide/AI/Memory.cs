using System;
using System.Collections.Generic;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI;

public class Memory : ISnapshot
{
	public double TimePointOfFailedLeisureWalkAttempt;

	public TimeSpan TimeSpentIdling;

	private EntityID? lastTarget;

	private double? timePointOfLastAttackAgainstTarget;

	private EntityID? lastFoundPrey;

	private double? timePointThatPreyWasFound;

	private EntityID? lastHuntedAnimalCarcass;

	private double? timePointThatCarcassWasCreated;

	private Dictionary<EntityID, double> lastHauledItems = new Dictionary<EntityID, double>();

	private const double recentlyHitByEntityMemoryDuration = 5.0;

	private EntityID? attackerID;

	private double? timePointThatWeWereLastHit;

	private const double timePointForLastCombatAlertMemoryDuration = 10.0;

	private double? timePointForLastCombatAlert;

	private double? timePointForJoiningExpedition;

	private double? timePointForLastPatrolling;

	private JobID? lastPatrolJobID;

	private List<EntityID> neededItemsForNextGoal = new List<EntityID>();

	private Regulator cleanupHauledItemsRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public AllegianceID? EmigrateTarget { get; private set; }

	public bool IsSnapshotted { get; set; }

	public Memory()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			CreateRegulators();
		}
	}

	public void RememberAttacker(EntityID entity)
	{
		attackerID = entity;
		timePointThatWeWereLastHit = The.Sim.TotalUnPausedGameTime.TotalSeconds;
	}

	private static bool RetireTimestamp(ref double? timepoint, double duration)
	{
		if (timepoint.HasValue && The.Sim.TotalUnPausedGameTime.TotalSeconds - timepoint.Value > duration)
		{
			timepoint = null;
			return true;
		}
		return false;
	}

	public void SetNeededItemForSwitchedGoal(EntityID entityID)
	{
		neededItemsForNextGoal.Add(entityID);
	}

	public void ClearNeededItemsForNextGoal()
	{
		neededItemsForNextGoal.Clear();
	}

	public bool NeedsItemForSwitchedGoal(EntityID entityID)
	{
		return neededItemsForNextGoal.Contains(entityID);
	}

	public bool WasRecentlyHitBy(EntityID potentialAttackerID)
	{
		return potentialAttackerID == GetLastAttacker();
	}

	public EntityID? GetLastAttacker()
	{
		if (timePointThatWeWereLastHit.HasValue)
		{
			if (RetireTimestamp(ref timePointThatWeWereLastHit, 5.0))
			{
				attackerID = null;
				return null;
			}
			return attackerID;
		}
		return null;
	}

	public bool RecentlyGotCombatAlert()
	{
		if (timePointForLastCombatAlert.HasValue)
		{
			if (RetireTimestamp(ref timePointForLastCombatAlert, 10.0))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetTimepointForCombatAlert()
	{
		timePointForLastCombatAlert = The.Sim.TotalUnPausedGameTimeInSeconds;
	}

	public void SetTimepointForJoiningExpedition()
	{
		timePointForJoiningExpedition = The.Sim.TotalUnPausedGameTimeInSeconds;
	}

	public void ResetTimepointForJoiningExpedition()
	{
		timePointForJoiningExpedition = null;
	}

	public bool RecentlyJoinedExpedition()
	{
		if (timePointForJoiningExpedition.HasValue)
		{
			if (RetireTimestamp(ref timePointForJoiningExpedition, GameData.Instance.AIConstants.PeriodAfterJoiningBeforeEmigrateIsPossibleInSeconds))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetRecentlyHuntedCarcass(EntityID entity)
	{
		lastHuntedAnimalCarcass = entity;
		timePointThatCarcassWasCreated = The.Sim.TotalUnPausedGameTimeInSeconds;
	}

	public double GetRecentlyHuntedCarcassScore(EntityID carcassToScore)
	{
		return GetTemporarilyIncreasedScore(carcassToScore, ref lastHuntedAnimalCarcass, ref timePointThatCarcassWasCreated, 5.0);
	}

	public void SetRecentlyFoundPrey(EntityID entity)
	{
		lastFoundPrey = entity;
		timePointThatPreyWasFound = The.Sim.TotalUnPausedGameTimeInSeconds;
	}

	public CombatAreaJob GetLastCombatAreaJob()
	{
		if (timePointForLastPatrolling.HasValue)
		{
			if (Common.TimepointIsOutDated(timePointForLastPatrolling.Value, GameData.Instance.AIConstants.MaxTimeForLeavingPatrolPostUntilFreed))
			{
				timePointForLastPatrolling = null;
				lastPatrolJobID = null;
				return null;
			}
			Job job = LookUp<Job, JobID>.FindByID(lastPatrolJobID);
			if (job != null)
			{
				return (CombatAreaJob)job;
			}
		}
		return null;
	}

	public void SetEmigrateDecision(AllegianceID? toAllegiance, Entity parent)
	{
		EmigrateTarget = toAllegiance;
		if (EmigrateTarget.HasValue)
		{
			parent.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DecidedToLeaveAllegiance, out var value);
			Goal.FireEventActions(parent, null, value);
		}
	}

	public void SetRecentlyOnPatrol(JobID? jobID)
	{
		if (jobID.HasValue)
		{
			lastPatrolJobID = jobID;
			timePointForLastPatrolling = The.Sim.TotalUnPausedGameTimeInSeconds;
		}
		else
		{
			lastPatrolJobID = null;
			timePointForLastPatrolling = null;
		}
	}

	public void SetLastAttackTarget(EntityID entity)
	{
		lastTarget = entity;
		timePointOfLastAttackAgainstTarget = The.Sim.TotalUnPausedGameTime.TotalSeconds;
	}

	public double GetRecentlyFoundPreyScore(EntityID target)
	{
		return GetTemporarilyIncreasedScore(target, ref lastFoundPrey, ref timePointThatPreyWasFound, 5.0);
	}

	public double GetLastAttackScore(EntityID target)
	{
		return GetTemporarilyIncreasedScore(target, ref lastTarget, ref timePointOfLastAttackAgainstTarget, 10.0);
	}

	public double GetRecentlyHauledItemScore(EntityID target)
	{
		if (lastHauledItems.Count > 0)
		{
			if (cleanupHauledItemsRegulator.IsReady())
			{
				CleanupLastHauledItems();
			}
			if (lastHauledItems.TryGetValue(target, out var _))
			{
				return 1.0;
			}
		}
		return 0.0;
	}

	public void SetLastHauledItem(EntityID entity)
	{
		lastHauledItems[entity] = The.Sim.TotalUnPausedGameTime.TotalSeconds;
	}

	public void ResetHauledItem(EntityID entityID)
	{
		if (lastHauledItems.ContainsKey(entityID))
		{
			lastHauledItems.Remove(entityID);
		}
	}

	private void CreateRegulators()
	{
		cleanupHauledItemsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "Memory");
	}

	private void CleanupLastHauledItems()
	{
		List<EntityID> list = null;
		foreach (KeyValuePair<EntityID, double> lastHauledItem in lastHauledItems)
		{
			if (The.Sim.TimepointReached(lastHauledItem.Value + 5.0))
			{
				Common.AddToList(ref list, lastHauledItem.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityID item in list)
		{
			lastHauledItems.Remove(item);
		}
	}

	private static double GetTemporarilyIncreasedScore(EntityID target, ref EntityID? lastTarget, ref double? timePoint, double duration)
	{
		bool flag = false;
		if (timePoint.HasValue)
		{
			flag = The.Sim.TotalUnPausedGameTimeInSeconds - timePoint.Value > duration;
			EntityID? entityID = lastTarget;
			if (entityID.GetValueOrDefault() == target && entityID.HasValue && !flag)
			{
				return 1.0;
			}
			if (flag)
			{
				lastTarget = null;
				timePoint = null;
			}
		}
		return 0.0;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		attackerID = sn.DoEnumNullable(attackerID);
		lastFoundPrey = sn.DoEnumNullable(lastFoundPrey);
		lastHuntedAnimalCarcass = sn.DoEnumNullable(lastHuntedAnimalCarcass);
		lastTarget = sn.DoEnumNullable(lastTarget);
		TimePointOfFailedLeisureWalkAttempt = sn.DoDouble(TimePointOfFailedLeisureWalkAttempt);
		timePointOfLastAttackAgainstTarget = sn.DoDoubleNullable(timePointOfLastAttackAgainstTarget);
		timePointThatCarcassWasCreated = sn.DoDoubleNullable(timePointThatCarcassWasCreated);
		timePointThatPreyWasFound = sn.DoDoubleNullable(timePointThatPreyWasFound);
		timePointThatWeWereLastHit = sn.DoDoubleNullable(timePointThatWeWereLastHit);
		TimeSpentIdling = sn.DoTimeSpan(TimeSpentIdling);
		timePointForLastCombatAlert = sn.DoDoubleNullable(timePointForLastCombatAlert);
		lastPatrolJobID = sn.DoEnumNullable(lastPatrolJobID);
		timePointForLastPatrolling = sn.DoDoubleNullable(timePointForLastPatrolling);
		timePointForJoiningExpedition = sn.DoDoubleNullable(timePointForJoiningExpedition);
		EmigrateTarget = sn.DoEnumNullable(EmigrateTarget);
		lastHauledItems = sn.DoDictionary(lastHauledItems);
		sn.Ignore(neededItemsForNextGoal);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateRegulators();
	}
}
