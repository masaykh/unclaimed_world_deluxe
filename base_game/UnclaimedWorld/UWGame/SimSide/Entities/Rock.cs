using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Rock : Component
{
	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Rock()
	{
	}

	public Rock(Entity parent)
		: base(parent)
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
