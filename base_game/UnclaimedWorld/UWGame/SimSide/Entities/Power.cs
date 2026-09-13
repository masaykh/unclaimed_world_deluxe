using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Power : Component
{
	public float Energy = 0.81f;

	public float PowerOutput;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string EnergyLevelToString()
	{
		return (int)(Energy * 100f) + "/100";
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		sn.DoFloat(Energy);
		sn.DoFloat(PowerOutput);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public Power(Entity parent)
		: base(parent)
	{
	}

	public Power()
	{
	}
}
