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

	private OnExitFlagAction onExitFlagActionValue = OnExitFlagAction.Clear;

	private bool scaleAnimationToFillWaitPeriod;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double? MaxTimeToWait { get; set; }

	public override bool IsSame(Job job)
	{
		return false;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: base(owner)
	{
		if (maxPeriodInSeconds.HasValue)
		{
			MaxTimeToWait = maxPeriodInSeconds.Value;
			MaxTimeToWait = Common.ClampBottom(maxPeriodInSeconds.Value, 0.0);
		}
		this.scaleAnimationToFillWaitPeriod = scaleAnimationToFillWaitPeriod;
		onExitFlagActionValue = onExitAction;
		ModifierStatesToSet = new List<AnimModifier>();
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
	{
		ActionStateToSet = actionStateToSet;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, List<AnimModifier> statesToSet, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet = statesToSet;
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state);
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state1);
		ModifierStatesToSet.Add(state2);
	}

	public GoalWait(Entity owner, double? maxPeriodInSeconds, AnimAction? actionStateToSet, AnimModifier state1, AnimModifier state2, AnimModifier state3, bool scaleAnimationToFillWaitPeriod = false, OnExitFlagAction onExitAction = OnExitFlagAction.Clear)
		: this(owner, maxPeriodInSeconds, scaleAnimationToFillWaitPeriod, onExitAction)
	{
		ActionStateToSet = actionStateToSet;
		ModifierStatesToSet.Add(state1);
		ModifierStatesToSet.Add(state2);
		ModifierStatesToSet.Add(state3);
	}

	public GoalWait(Entity owner)
		: base(owner)
	{
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
		if (ModifierStatesToSet != null || ActionStateToSet.HasValue)
		{
			if (ModifierStatesToSet != null)
			{
				foreach (AnimModifier item in ModifierStatesToSet)
				{
					entity.Renderable.SetAnimationStateFlag(item);
				}
			}
			if (ActionStateToSet.HasValue)
			{
				entity.Renderable.SetAnimationActionStateFlag(ActionStateToSet.Value);
			}
		}
		else if (!entity.DrivingVehicle.HasValue && !entity.PassengerInVehicle.HasValue)
		{
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
		if ((ModifierStatesToSet == null && !ActionStateToSet.HasValue) || onExitFlagActionValue != OnExitFlagAction.Clear)
		{
			return;
		}
		if (ModifierStatesToSet != null)
		{
			foreach (AnimModifier item in ModifierStatesToSet)
			{
				entity.Renderable.ClearAnimationStateFlag(item);
			}
		}
		if (ActionStateToSet.HasValue)
		{
			entity.Renderable.ClearAnimationActionStateFlag(ActionStateToSet.Value);
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (MaxTimeToWait.HasValue)
		{
			base.Status = ProcessCountdown(elapsed, MaxTimeToWait.Value, out var scalar);
			if (scaleAnimationToFillWaitPeriod)
			{
				entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
			}
		}
	}

	public override bool CanReactToInterest()
	{
		return HeadTurnAllowed;
	}

	public override bool HandleMessage(Message message)
	{
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
		version = sn.DoVersion(Snapshotter.Version.Original);
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
		return this;
	}
}
