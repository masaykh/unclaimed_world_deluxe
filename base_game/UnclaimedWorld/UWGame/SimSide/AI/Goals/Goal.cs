using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.AI.Goals;

public abstract class Goal : ISnapshot, ILookUp<Goal, GoalID>
{
	public enum MovementSpeeds
	{
		WalkSlowly,
		Normal,
		WalkFast,
		Run,
		Haul
	}

	public enum StealthFactor
	{
		None,
		NotGood,
		Bad,
		ExtremelyBad
	}

	public enum DetectionFactor
	{
		CannotDetect,
		DetectSome,
		DetectGood,
		DetectVeryGood
	}

	public Entity entity;

	private EntityID snapshotEntity;

	protected Intelligence entityIntelligence;

	protected Person personEntity;

	private Status status;

	protected double? delayPeriodInSeconds;

	protected double delayProgress;

	public GoalEvaluator GoalEvaluator;

	private int? snapshotGoalEvaluatorIndex;

	protected Regulator preconditionsRegulator;

	protected bool hasEntered;

	protected bool hasActivated;

	protected bool hasTerminated;

	private Snapshotter.Version version;

	private GoalID id = GoalID.Invalid;

	private static GoalID IDCounter = GoalID.First;

	public Status Status
	{
		get
		{
			return status;
		}
		set
		{
			if (value != status)
			{
				_ = 2;
				if (value == Status.Failed && !(this is GoalDoTakeFive) && !(this is GoalTakeFive) && !(this is GoalDoProduceAtomic) && entity != null && entity.Name != null && !entity.Name.Contains("Bob"))
				{
					entity.Name.Contains("nez");
				}
				status = value;
			}
		}
	}

	public double TimeLeftInSeconds { get; set; }

	public bool IsSnapshotted { get; set; }

	public GoalID ID
	{
		get
		{
			return id;
		}
		private set
		{
			if (id != value)
			{
				_ = uint.MaxValue;
				id = value;
			}
		}
	}

	public int LoadPostProcessOrder => 0;

	public Goal(Entity entity)
	{
		Init(entity);
	}

	protected Goal()
	{
	}

	protected void Init(Entity entity)
	{
		AddToLookup();
		hasTerminated = false;
		hasEntered = false;
		hasActivated = false;
		Status = Status.Inactive;
		delayProgress = 0.0;
		delayPeriodInSeconds = null;
		TimeLeftInSeconds = 0.0;
		this.entity = entity;
		entityIntelligence = this.entity.Intelligence;
		personEntity = this.entity.PersonEntity;
		CreateRegulators();
	}

	protected bool AreThereNonMovingEntitiesInThisSpot()
	{
		if (!entity.ContainedBy.HasValue && MapManager.IsStandingOnNonMovingEntity(entity))
		{
			if (!The.Map.FindUnoccupiedSubtileInsideTile(entity, entity.MapPosition.Value).HasValue)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected bool UsedEntityResultShouldFailGoal(EntityResult result)
	{
		if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
		{
			return true;
		}
		return false;
	}

	protected bool EntityResultCausesFailedGoal(EntityResult result)
	{
		if (UsedEntityResultShouldFailGoal(result))
		{
			Status = Status.Failed;
			return true;
		}
		return false;
	}

	protected bool AreToolsOK(Entity entity, List<EntityID> Tools)
	{
		bool result = true;
		foreach (EntityID Tool in Tools)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(Tool, out var data)))
			{
				return false;
			}
			if (!ToolDistanceIsValid(entity.PlaySiteLocation, data))
			{
				result = false;
				break;
			}
			if (data.EntityType.ToolType.PrepareProcess != null && data.IsPrepared != true)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static bool ToolDistanceIsValid(Vector3 workerLocation, IKnownEntityData toolData)
	{
		if (Common.DistanceOctile(workerLocation, toolData.AccessPoint.Value) > 80f && Common.DistanceOctile(workerLocation, toolData.PlaySiteLocation) > 80f)
		{
			return false;
		}
		return true;
	}

	protected bool EntityIsNotSeenDirectly(EntityID entityID, out Entity entity)
	{
		entity = null;
		if (entityIntelligence.GetKnownData(entityID, out var data) != EntityResult.SeenDirectly)
		{
			Status = Status.Failed;
			ExitIfFailedOrCompleted();
			return true;
		}
		entity = (Entity)data;
		return false;
	}

	protected void SetProcessAnimStates(ProcessType processType)
	{
		entity.Renderable.SetAnimationActionStateFlag(processType.AgentActionState);
		if (processType.AgentAnimationStates != null)
		{
			AnimModifier[] agentAnimationStates = processType.AgentAnimationStates;
			foreach (AnimModifier animationStateFlag in agentAnimationStates)
			{
				entity.Renderable.SetAnimationStateFlag(animationStateFlag);
			}
		}
	}

	protected void ClearProcessAnimStates(ProcessType processType)
	{
		entity.Renderable.ClearAnimationActionStateFlag(processType.AgentActionState);
		if (processType.AgentAnimationStates != null)
		{
			AnimModifier[] agentAnimationStates = processType.AgentAnimationStates;
			foreach (AnimModifier state in agentAnimationStates)
			{
				entity.Renderable.ClearAnimationStateFlag(state);
			}
		}
	}

	public override string ToString()
	{
		return GetType().ToString().Remove(0, GetType().ToString().LastIndexOf(".") + 1) + " (" + Status.ToString() + ")";
	}

	public virtual string GetStatus()
	{
		return "";
	}

	public virtual string ComposeIndentedString(string indent)
	{
		return indent + ToString();
	}

	public void ActivateIfInactive()
	{
		if (isInactive())
		{
			Activate();
			hasActivated = true;
		}
	}

	protected void ReactivateIfFailed()
	{
		if (HasFailed())
		{
			Status = Status.Inactive;
		}
	}

	protected bool IsDelayed(GameTime elapsed)
	{
		if (delayPeriodInSeconds.HasValue)
		{
			delayProgress += elapsed.ElapsedGameTime.TotalSeconds;
			if (delayProgress > delayPeriodInSeconds)
			{
				delayPeriodInSeconds = null;
				delayProgress = 0.0;
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual void CreateRegulators()
	{
		preconditionsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 2.0, "GoalPreconditions");
	}

	protected virtual bool ArePreconditionsOK()
	{
		return true;
	}

	protected void StartDelay(double seconds)
	{
		delayPeriodInSeconds = seconds;
	}

	protected double GetCurrentGoalScore()
	{
		double? currentGoalUtility = entityIntelligence.GetCurrentGoalUtility();
		if (currentGoalUtility.HasValue && currentGoalUtility.HasValue)
		{
			return currentGoalUtility.Value;
		}
		return 0.0;
	}

	protected void ResetThreatStance(Job job)
	{
		if (job.RequiresBoldStance)
		{
			entity.Intelligence.ResetThreatStance();
		}
	}

	protected double ScoreJobGoal(Job job, ToolParams? toolParams = null, AttackParams? attackParams = null, HaulingParams? haulingParams = null)
	{
		double rating = 0.0;
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, job, out threatStanceToUse);
		if (((IScoreJob)GoalEvaluator).ScoreThisJob(regionMapAndStanceForEvaluator, threatStanceToUse, entity, job, job.TakenBy.Count, null, null, out rating, toolParams, attackParams, haulingParams) == GoalEvaluator.CalculateResult.Done)
		{
			return rating;
		}
		return GetCurrentGoalScore();
	}

	public abstract bool IsSame(Job job);

	protected abstract void Activate();

	public Status Process(GameTime elapsed)
	{
		ActivateIfInactive();
		if (Status == Status.Active)
		{
			ProcessWhileActive(elapsed);
		}
		ExitIfFailedOrCompleted();
		return Status;
	}

	protected abstract void ProcessWhileActive(GameTime elapsed);

	protected Status ProcessCountdown(GameTime elapsedTime, double maxTimeToWait, out double scalar)
	{
		Status result = Status.Active;
		TimeLeftInSeconds -= elapsedTime.ElapsedGameTime.TotalSeconds;
		if (maxTimeToWait <= 0.0)
		{
			scalar = 0.0;
			return result;
		}
		if (TimeLeftInSeconds <= 0.0)
		{
			TimeLeftInSeconds = 0.0;
			result = Status.Completed;
		}
		else if (TimeLeftInSeconds > maxTimeToWait)
		{
			TimeLeftInSeconds = maxTimeToWait;
		}
		scalar = 1.0 - TimeLeftInSeconds / maxTimeToWait;
		return result;
	}

	public virtual void Deactivate()
	{
	}

	public virtual void Terminate()
	{
		if (!hasTerminated)
		{
			if (hasActivated)
			{
				Deactivate();
			}
			if (hasEntered)
			{
				OnExit();
			}
			hasTerminated = true;
		}
	}

	public virtual void RetireGoal()
	{
	}

	public virtual void OnEnter()
	{
		if (!CanReactToInterest())
		{
			entityIntelligence.ClearCenterOfAttention();
		}
	}

	public void EnterIfNew()
	{
		if (!hasEntered)
		{
			OnEnter();
			hasEntered = true;
		}
	}

	public virtual void OnExit()
	{
	}

	protected void ExitIfFailedOrCompleted()
	{
		if (Status == Status.Failed || Status == Status.Completed)
		{
			Terminate();
		}
	}

	public virtual bool RequiresBoldStance()
	{
		return false;
	}

	public virtual float GetExertionLevel()
	{
		return GameData.Instance.Constants.PhysicalWork.IdleExertionDefault;
	}

	public virtual void PerformWork(SimProcess process, float workedTimeInSeconds, float progressDelta)
	{
	}

	public virtual string GetToolInUseName()
	{
		return null;
	}

	public virtual string GetSkillInUseName()
	{
		return null;
	}

	public virtual float? GetSkillProductivity()
	{
		return null;
	}

	public virtual float? GetToolProductivity()
	{
		return null;
	}

	public virtual float? GetCurrentTotalProductivity()
	{
		return null;
	}

	public virtual StealthFactor GetStealthFactor()
	{
		return StealthFactor.None;
	}

	public virtual DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public virtual DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public virtual bool CanReactToInterest()
	{
		return true;
	}

	public virtual bool CanDropRequestedItem(Entity item)
	{
		return true;
	}

	public virtual Goal GetFrontMostGoal()
	{
		return this;
	}

	public virtual bool HandleMessage(Message message)
	{
		return false;
	}

	public virtual void AddSubgoal(Goal goal)
	{
		throw new Exception("Atomic goals cannot have subgoals.");
	}

	public bool IsCompleted()
	{
		return Status == Status.Completed;
	}

	public bool isActive()
	{
		return Status == Status.Active;
	}

	public bool isInactive()
	{
		return Status == Status.Inactive;
	}

	public bool HasFailed()
	{
		return Status == Status.Failed;
	}

	public static void FireEventActions(Entity entity, EntityID? targetEntity, AgentActionHooks defaultActionHook, Dictionary<AgentActionHooks, List<ActionSets>> defaultEventActions, AgentActionHooks overridingActionHook, Dictionary<AgentActionHooks, List<ActionSets>> overridingEventActions, bool isSpawning = false)
	{
		List<ActionSets> value = null;
		defaultEventActions?.TryGetValue(defaultActionHook, out value);
		List<ActionSets> value2 = null;
		overridingEventActions?.TryGetValue(overridingActionHook, out value2);
		FireEventActions(entity, targetEntity, value, value2, isSpawning);
	}

	public static void FireEventActions(Entity triggeringEntity, EntityID? targetEntity, List<ActionSets> defaultActionSets, List<ActionSets> overridingActionSets = null, bool isSpawning = false)
	{
		List<ActionSets> list = null;
		list = ((overridingActionSets == null || overridingActionSets.Count <= 0) ? defaultActionSets : overridingActionSets);
		if (list != null)
		{
			bool isExpired;
			list.ForEach(delegate(ActionSets a)
			{
				a.Fire(triggeringEntity, targetEntity, null, out isExpired, isSpawning);
			});
		}
	}

	protected float? GetInterestAndHandleTrigger(Message message, out EntityID? entity, out Vector3? location)
	{
		float? result = null;
		entity = null;
		location = null;
		if (message.OtherInfo is Tuple<Trigger, EntityID?, Vector3?, float?> { Item1: var item } tuple)
		{
			if (item != null)
			{
				Interest interest = item.TriggerType.Interest;
				if (interest != null)
				{
					result = (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, interest.InterestLevelMean, interest.InterestLevelStdDeviation);
				}
				entityIntelligence.SetTriggerCooldown(item, item.TriggerType.CooldownInTicks);
			}
			else
			{
				result = tuple.Item4;
			}
			entity = tuple.Item2;
			location = tuple.Item3;
		}
		return result;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		ID = SnapshotID(sn, ID);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotEntity = sn.SnapshotID<Entity, EntityID>(entity).Value;
		_ = snapshotEntity;
		_ = long.MaxValue;
		delayPeriodInSeconds = sn.DoDoubleNullable(delayPeriodInSeconds);
		delayProgress = sn.DoDouble(delayProgress);
		hasEntered = sn.DoBool(hasEntered);
		hasActivated = sn.DoBool(hasActivated);
		hasTerminated = sn.DoBool(hasTerminated);
		status = sn.DoEnum(status);
		TimeLeftInSeconds = sn.DoDouble(TimeLeftInSeconds);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (GoalEvaluator != null)
			{
				snapshotGoalEvaluatorIndex = entityIntelligence.Brain.GetIndexOfEvaluator(GoalEvaluator);
			}
			else
			{
				snapshotGoalEvaluatorIndex = null;
			}
		}
		snapshotGoalEvaluatorIndex = sn.DoInt32Nullable(snapshotGoalEvaluatorIndex);
		sn.Ignore(personEntity);
		sn.Ignore(entityIntelligence);
		sn.Ignore(GoalEvaluator);
		sn.Ignore(preconditionsRegulator);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		entity = Entity.FindByID(snapshotEntity);
		entityIntelligence = entity.Intelligence;
		personEntity = entity.PersonEntity;
		if (snapshotGoalEvaluatorIndex.HasValue)
		{
			GoalEvaluator = entityIntelligence.Brain.GetEvaluatorFromIndex(snapshotGoalEvaluatorIndex.Value);
		}
		CreateRegulators();
	}

	public GoalID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= GoalID.Invalid)
		{
			throw new Exception("Astounding, GoalID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public GoalID SnapshotID(Snapshotter sn, GoalID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != GoalID.Invalid)
		{
			LookUpGoals.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUpGoals.Remove(this);
	}

	void ILookUp<Goal, GoalID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = GoalID.First;
	}

	void ILookUp<Goal, GoalID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpGoals.Create();
	}

	public void SetInvalid()
	{
		id = GoalID.Invalid;
	}
}
