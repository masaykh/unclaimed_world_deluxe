using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class MachineEntity : Component
{
	public float Condition = 1f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public MachineEntity(Entity parent)
		: base(parent)
	{
	}

	public MachineEntity()
	{
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
		sn.DoFloat(Condition);
		return this;
	}
}
