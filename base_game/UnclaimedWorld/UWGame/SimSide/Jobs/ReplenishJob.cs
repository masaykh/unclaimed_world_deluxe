using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class ReplenishJob : ISnapshot
{
	public GoalReplenish.ReplenishAction Action;

	public EntityID EntityToReplenish;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public ReplenishJob(EntityID entityToReplenish, GoalReplenish.ReplenishAction action)
	{
		EntityToReplenish = entityToReplenish;
		Action = action;
	}

	public ReplenishJob()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EntityToReplenish = sn.DoEnum(EntityToReplenish);
		Action = sn.DoEnum(Action);
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
	}
}
