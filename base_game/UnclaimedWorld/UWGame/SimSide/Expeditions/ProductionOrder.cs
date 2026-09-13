using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions;

public class ProductionOrder : ISnapshot
{
	public int? ProductionJobsToComplete;

	public int? AmountToKeepInStore;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ProductionJobsToComplete = sn.DoInt32Nullable(ProductionJobsToComplete);
		AmountToKeepInStore = sn.DoInt32Nullable(AmountToKeepInStore);
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
