using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalChangeStance : Goal
{
	private double maxTimeToWait;

	public AnimAction? ActionStateToSet;

	public List<AnimModifier> ModifierStatesToSet;

	private bool scaleAnimationToFillWaitPeriod;

	private StanceType stanceToTake;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalChangeStance(Entity entity, StanceType stanceToTake)
		: base(entity)
	{
		this.stanceToTake = stanceToTake;
		scaleAnimationToFillWaitPeriod = true;
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
		maxTimeToWait = sn.DoDouble(maxTimeToWait);
		ActionStateToSet = sn.DoEnumNullable(ActionStateToSet);
		ModifierStatesToSet = sn.DoList(ModifierStatesToSet);
		scaleAnimationToFillWaitPeriod = sn.DoBool(scaleAnimationToFillWaitPeriod);
		stanceToTake = sn.DoGameData(stanceToTake);
		return this;
	}

	public GoalChangeStance()
	{
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void Activate()
	{
		Stance stance = null;
		if (entity.Find<Locomotor>(out var c))
		{
			if (c == null || c.Stance == null)
			{
				base.Status = Status.Completed;
				return;
			}
			stance = c.Stance;
		}
		StanceType currentStance = stance.CurrentStance;
		if (currentStance == stanceToTake)
		{
			base.Status = Status.Completed;
			return;
		}
		GetChangeStanceAnim(entity.EntityType.LocomotorType.StancesType, stanceToTake, currentStance, out var currentAnimStance, out var animStanceToTake, out var reverse, out var duration);
		if (duration.HasValue)
		{
			ModifierStatesToSet = new List<AnimModifier>();
			if (currentAnimStance.HasValue)
			{
				ModifierStatesToSet.Add(currentAnimStance.Value);
			}
			if (animStanceToTake.HasValue)
			{
				ModifierStatesToSet.Add(animStanceToTake.Value);
			}
			if (reverse)
			{
				ModifierStatesToSet.Add(AnimModifier.Reverse);
			}
			base.TimeLeftInSeconds = duration.Value;
			maxTimeToWait = duration.Value;
			foreach (AnimModifier item in ModifierStatesToSet)
			{
				entity.Renderable.SetAnimationStateFlag(item);
			}
			ActionStateToSet = AnimAction.ChangingStance;
			entity.Renderable.SetAnimationActionStateFlag(ActionStateToSet.Value);
			base.Status = Status.Active;
		}
		else
		{
			stance.CurrentStance = stanceToTake;
			base.Status = Status.Completed;
		}
	}

	private static void GetChangeStanceAnim(StancesType stancesType, StanceType stanceToTake, StanceType currentStance, out AnimModifier? currentAnimStance, out AnimModifier? animStanceToTake, out bool reverse, out double? duration)
	{
		duration = null;
		animStanceToTake = stanceToTake.AnimModifier;
		currentAnimStance = currentStance.AnimModifier;
		if (stanceToTake.Number < currentStance.Number)
		{
			reverse = true;
		}
		else
		{
			reverse = false;
		}
		if (stancesType.StanceChangeDurations != null && stancesType.StanceChangeDurationsMapping.TryGetValue(currentStance, out var value) && value.TryGetValue(stanceToTake, out var value2))
		{
			duration = value2;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (ModifierStatesToSet != null || ActionStateToSet.HasValue)
		{
			foreach (AnimModifier item in ModifierStatesToSet)
			{
				entity.Renderable.ClearAnimationStateFlag(item);
			}
			if (ActionStateToSet.HasValue)
			{
				entity.Renderable.ClearAnimationActionStateFlag(ActionStateToSet.Value);
			}
		}
		if (entity.HasStance())
		{
			entity.Locomotor.Stance.CurrentStance = stanceToTake;
		}
		List<AnimModifier> list = new List<AnimModifier>();
		List<AnimModifier> list2 = new List<AnimModifier>();
		AnimModifier? animModifier = stanceToTake.AnimModifier;
		if (animModifier.HasValue)
		{
			list.Add(animModifier.Value);
		}
		Renderable.GetExcludedStanceFlags(animModifier, list2);
		foreach (AnimModifier item2 in list)
		{
			entity.Renderable.SetAnimationStateFlag(item2);
		}
		foreach (AnimModifier item3 in list2)
		{
			entity.Renderable.ClearAnimationStateFlag(item3);
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessCountdown(elapsed, maxTimeToWait, out var scalar);
		if (scaleAnimationToFillWaitPeriod)
		{
			entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
		}
	}
}
