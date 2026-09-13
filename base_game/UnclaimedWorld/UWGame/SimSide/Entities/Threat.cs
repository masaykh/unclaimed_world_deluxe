using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Threat : Component
{
	public ThreatGroup ThreatGroup;

	private ThreatGroupID snapshotThreatGroup;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Threat()
	{
	}

	public Threat(Entity parent)
		: base(parent)
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotThreatGroup = sn.SnapshotID<ThreatGroup, ThreatGroupID>(ThreatGroup).Value;
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroup);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
