using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class UpgradeJob : ISnapshot
{
	public UpgradeCategory UpgradeCategory;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public UpgradeJob(UpgradeCategory upgradeCategory)
	{
		UpgradeCategory = upgradeCategory;
	}

	public UpgradeJob()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		UpgradeCategory = sn.DoGameData(UpgradeCategory);
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
