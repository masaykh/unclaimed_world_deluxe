using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalWait : Goal
{
	public enum OnExitFlagAction
	{
		Leave,
		Clear
	}

	public bool HeadTurnAllowed = true;

	public AnimAction? ActionStateToSet;

	public List<AnimModifier> ModifierStatesToSet;

	// Independent client animation requests; they do not replace the ordinary action and modifiers.
	public List<AnimConditions> ParallelConditionsToSet;

	private OnExitFlagAction onExitFlagActionValue = OnExitFlagAction.Clear;

	private bool scaleAnimationToFillWaitPeriod;

	/// <summary>
	/// Allows starting anims on non-agents like vehicles.
	/// Never start animations directly on other agents - send a message instead!
	/// </summary>
	private EntityID? animationTargetEntityID;

	private bool animationConditionsPublished;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double? MaxTimeToWait { get; set; }

	public override bool IsSame(Job job)
	{
		return false;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: base(owner)
	{
		if (maxPeriodInSeconds.HasValue)
		{
			MaxTimeToWait = maxPeriodInSeconds.Value;
			MaxTimeToWait = Common.ClampBottom(maxPeriodInSeconds.Value, 0.0);
		}
		this.scaleAnimationToFillWaitPeriod = scaleAnimationToFillWaitPeriod;
		onExitFlagActionValue = onExitAction;
		animationTargetEntityID = animationTargetEntity?.EntityID;
		ModifierStatesToSet = new List<AnimModifier>();
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction, animationTargetEntity)
	{
		ActionStateToSet = actionStateToSet;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, List<AnimModifier> statesToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction, animationTargetEntity)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet = statesToSet;
	}

	// The simulation waits for maxPeriodInSeconds, not for client animation completion.
	public GoalWait(Entity owner, List<AnimConditions> parallelConditionsToSet, double? maxPeriodInSeconds, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, onExitAction: onExitAction, animationTargetEntity: animationTargetEntity)
	{
		ParallelConditionsToSet = parallelConditionsToSet;
	}

	public GoalWait(Entity owner, List<AnimConditions> parallelConditionsToSet, double? maxPeriodInSeconds, EntityID animationTargetEntity, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, parallelConditionsToSet, maxPeriodInSeconds, onExitAction)
	{
		animationTargetEntityID = animationTargetEntity;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction, animationTargetEntity)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state);
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction, animationTargetEntity)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state1);
		ModifierStatesToSet.Add(state2);
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, AnimModifier state3, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear, Entity animationTargetEntity = null)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction, animationTargetEntity)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state1);
		ModifierStatesToSet.Add(state2);
		ModifierStatesToSet.Add(state3);
	}

	public GoalWait(Entity owner, Entity animationTargetEntity = null)
		: base(owner)
	{
		animationTargetEntityID = animationTargetEntity?.EntityID;
	}

	public GoalWait()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (MaxTimeToWait.HasValue)
		{
			base.TimeLeftInSeconds = MaxTimeToWait.Value;
		}
		if (entity.Find<Locomotor>(out var c))
		{
			c.CurrentMoveTarget = entity.PlaySiteLocation;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Entity animationTarget = GetAnimationTarget();
		if (animationTarget == null || animationTarget.Renderable == null)
		{
			base.Status = Status.Failed;
			return;
		}
		animationConditionsPublished = true;
		// Publish each set separately so the client can select multiple animations at once.
		if (ParallelConditionsToSet != null)
		{
			foreach (AnimConditions condition in ParallelConditionsToSet)
			{
				animationTarget.Renderable.AddParallelAnimationCondition(condition);
			}
		}

		if (ModifierStatesToSet != null || ActionStateToSet.HasValue)
		{
			if (ModifierStatesToSet != null)
			{
				foreach (AnimModifier item in ModifierStatesToSet)
				{
					animationTarget.Renderable.SetAnimationStateFlag(item);
				}
			}
			if (ActionStateToSet.HasValue)
			{
				animationTarget.Renderable.SetAnimationActionStateFlag(ActionStateToSet.Value);
			}
		}
		else if (animationTargetEntityID == null
			&& !entity.DrivingVehicle.HasValue && !entity.PassengerInVehicle.HasValue)
		{   
			// This sets a waiting agent in the idle animation:
            entity.Renderable.SetAnimationActionStateFlag(AnimAction.Idle);
		}
	}

	public override void OnExit()
	{
		if (entity.Locomotor != null && entity.Locomotor.CollisionResponder != null)
		{
			entity.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent = false;
		}
		base.OnExit();
		Entity animationTarget = GetAnimationTarget();
		if (!animationConditionsPublished || animationTarget?.Renderable == null)
		{
			return;
		}
		animationConditionsPublished = false;
		// Clear this goal's requests on completion or interruption unless the caller asked to leave them set.
		if (onExitFlagActionValue == OnExitFlagAction.Clear && ParallelConditionsToSet != null)
		{
			foreach (AnimConditions condition in ParallelConditionsToSet)
			{
				animationTarget.Renderable.RemoveParallelAnimationCondition(condition);
			}
		}
		if ((ModifierStatesToSet == null && !ActionStateToSet.HasValue) || onExitFlagActionValue != OnExitFlagAction.Clear)
		{
			return;
		}
		if (ModifierStatesToSet != null)
		{
			foreach (AnimModifier item in ModifierStatesToSet)
			{
				animationTarget.Renderable.ClearAnimationStateFlag(item);
			}
		}
		if (ActionStateToSet.HasValue)
		{
			animationTarget.Renderable.ClearAnimationActionStateFlag(ActionStateToSet.Value);
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		Entity animationTarget = GetAnimationTarget();
		if (animationTarget?.Renderable == null)
		{
			base.Status = Status.Failed;
			return;
		}
		if (MaxTimeToWait.HasValue)
		{
			base.Status = ProcessCountdown(elapsed, MaxTimeToWait.Value, out var scalar);
			if (scaleAnimationToFillWaitPeriod)
			{
				animationTarget.Renderable.UpdateMainAnimationTimeScalar(scalar);
			}
		}
	}

	public override bool CanReactToInterest()
	{
		return HeadTurnAllowed;
	}

	public override bool HandleMessage(Message message)
	{
		if (GetAnimationTarget()?.Renderable == null)
		{
			base.Status = Status.Failed;
			return false;
		}
		switch (message.MessageType)
		{
		case Message.MessageTypes.HopOnBoard:
		case Message.MessageTypes.GetOff:
			base.Status = Status.Completed;
			return true;
		case Message.MessageTypes.PathFound:
		case Message.MessageTypes.PathNotFound:
			base.Status = Status.Completed;
			The.Sim.WaitingAgents.Remove(entity.ID);
			return false;
		case Message.MessageTypes.StartGroupMovement:
			base.Status = Status.Completed;
			return false;
		case Message.MessageTypes.EndGroupMovement:
			base.Status = Status.Completed;
			return false;
		default:
			return false;
		}
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.GoalWaitAnimationTarget);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		HeadTurnAllowed = sn.DoBool(HeadTurnAllowed);
		MaxTimeToWait = sn.DoDoubleNullable(MaxTimeToWait);
		ActionStateToSet = sn.DoEnumNullable(ActionStateToSet);
		ModifierStatesToSet = sn.DoList(ModifierStatesToSet);
		onExitFlagActionValue = sn.DoEnum(onExitFlagActionValue);
		scaleAnimationToFillWaitPeriod = sn.DoBool(scaleAnimationToFillWaitPeriod);
		// Older snapshots have no parallel requests to restore.
		if (version >= Snapshotter.Version.ParallelAnimationConditions)
			ParallelConditionsToSet = sn.DoList(ParallelConditionsToSet);
		else
			sn.Ignore(ParallelConditionsToSet);
		if (version >= Snapshotter.Version.GoalWaitAnimationTarget)
		{
			animationTargetEntityID = sn.DoEntityIDNullable(animationTargetEntityID);
			animationConditionsPublished = sn.DoBool(animationConditionsPublished);
		}
		else
		{
			animationTargetEntityID = null;
			animationConditionsPublished = hasEntered && !hasTerminated;
		}
		return this;
	}

	private Entity GetAnimationTarget()
	{
		return animationTargetEntityID.HasValue ? Entity.FindByID(animationTargetEntityID.Value) : entity;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		// Restore the nested condition masks as well as the goal's own references.
		if (ParallelConditionsToSet != null)
		{
			foreach (AnimConditions condition in ParallelConditionsToSet)
				condition.LoadPostProcess(sn);
		}
	}
}
