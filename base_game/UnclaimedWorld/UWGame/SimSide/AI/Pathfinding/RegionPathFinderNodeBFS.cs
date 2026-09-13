using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

public class RegionPathFinderNodeBFS : ISnapshot
{
	public float G;

	public ushort Color;

	public byte Status;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public RegionPathFinderNodeBFS()
	{
	}

	public RegionPathFinderNodeBFS(float cost, ushort region)
	{
		G = cost;
		Color = region;
		Status = 2;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		G = sn.DoFloat(G);
		Color = sn.DoUInt16(Color);
		Status = sn.DoByte(Status);
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
