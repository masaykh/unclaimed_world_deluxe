using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal abstract class Motivation : ISnapshot
{
	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public bool IsFulfilled()
	{
		return false;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
