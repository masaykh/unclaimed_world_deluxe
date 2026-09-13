using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalWaitForRide : Goal
{
	private double? maxPeriodInSeconds;

	private double waitProgress;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalWaitForRide(Entity owner, double? maxPeriod)
		: base(owner)
	{
		maxPeriodInSeconds = maxPeriod;
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
		maxPeriodInSeconds = sn.DoDoubleNullable(maxPeriodInSeconds);
		waitProgress = sn.DoDouble(waitProgress);
		return this;
	}

	public GoalWaitForRide()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (maxPeriodInSeconds.HasValue)
		{
			waitProgress += elapsed.ElapsedGameTime.TotalSeconds;
			if (waitProgress > maxPeriodInSeconds)
			{
				base.Status = Status.Completed;
			}
		}
	}

	public override bool HandleMessage(Message message)
	{
		Message.MessageTypes messageType = message.MessageType;
		if (messageType == Message.MessageTypes.HopOnBoard)
		{
			base.Status = Status.Completed;
			return true;
		}
		return false;
	}
}
