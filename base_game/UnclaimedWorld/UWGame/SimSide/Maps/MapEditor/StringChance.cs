using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps.MapEditor;

public class StringChance : IEdge, ISnapshot
{
	public string String;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Edge { get; set; }

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Edge = sn.DoFloat(Edge);
		String = sn.DoString(String);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
