using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Buildings;

public class Heating : Component
{
	public bool HeatIsAvailable;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		sn.DoBool(HeatIsAvailable);
		return this;
	}
}
