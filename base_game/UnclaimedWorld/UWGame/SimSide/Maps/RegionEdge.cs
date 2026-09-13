using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class RegionEdge : ISnapshot
{
	public bool HasRoad;

	public ushort FromRegion;

	public ushort ToRegion;

	public float Length;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public RegionEdge(ushort from, ushort to, float length, bool hasRoad)
	{
		FromRegion = from;
		ToRegion = to;
		Length = length;
		HasRoad = hasRoad;
	}

	public RegionEdge()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		HasRoad = sn.DoBool(HasRoad);
		FromRegion = sn.DoUInt16(FromRegion);
		ToRegion = sn.DoUInt16(ToRegion);
		Length = sn.DoFloat(Length);
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
