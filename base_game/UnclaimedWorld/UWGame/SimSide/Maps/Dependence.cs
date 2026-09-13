using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class Dependence : ISnapshot
{
	public IMap Child;

	private IMapID snapshotChild;

	public float Weight;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Dependence(IMap child, float weight)
	{
		Child = child;
		Weight = weight;
	}

	public Dependence()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Weight = sn.DoFloat(Weight);
		snapshotChild = sn.SnapshotID<IMap, IMapID>(Child).Value;
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
		Child = LookUp<IMap, IMapID>.FindByID(snapshotChild);
	}
}
