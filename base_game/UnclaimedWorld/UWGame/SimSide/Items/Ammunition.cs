using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class Ammunition : ISnapshot
{
	public int NoOfRounds;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public bool HasEnoughAmmo(int neededRounds)
	{
		return neededRounds <= NoOfRounds;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		NoOfRounds = sn.DoInt32(NoOfRounds);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
