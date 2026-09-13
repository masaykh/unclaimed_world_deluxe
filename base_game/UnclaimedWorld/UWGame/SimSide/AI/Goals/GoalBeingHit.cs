using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalBeingHit : Goal
{
	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TotalTimeInSeconds { get; set; }

	public GoalBeingHit(Entity owner)
		: base(owner)
	{
		if (The.Sim.TotalUnPausedGameTimeInSeconds > 24.0)
		{
			_ = entity.ID;
			_ = 19;
		}
	}

	public override bool IsSame(Job job)
	{
		return false;
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
		return this;
	}

	public GoalBeingHit()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		TotalTimeInSeconds = 0.5;
		base.TimeLeftInSeconds = TotalTimeInSeconds;
		entity.Renderable.SetAnimationActionStateFlag(AnimAction.Recoiling);
		if (entityIntelligence.ThreatStance == ThreatStance.Bold)
		{
			entity.Renderable.SetAnimationStateFlag(AnimModifier.Bold);
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessCountdown(elapsed, TotalTimeInSeconds, out var scalar);
		entity.Renderable.UpdateMainAnimationTimeScalar(scalar);
	}

	public override void OnExit()
	{
		base.OnExit();
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Recoiling);
	}

	public override bool CanReactToInterest()
	{
		return false;
	}

	public override bool HandleMessage(Message message)
	{
		return true;
	}
}
