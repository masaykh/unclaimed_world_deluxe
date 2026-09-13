using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI;

public class EntityLock : ISnapshot
{
	public EntityID? InUseBy;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public bool IsEmpty()
	{
		return !InUseBy.HasValue;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		InUseBy = sn.DoEnumNullable(InUseBy);
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
