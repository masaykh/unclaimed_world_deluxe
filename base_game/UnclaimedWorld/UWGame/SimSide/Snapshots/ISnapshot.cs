namespace UWGame.SimSide.Snapshots;

public interface ISnapshot
{
	bool IsSnapshotted { get; set; }

	ISnapshot DoSnapshot(Snapshotter sn);

	Snapshotter.Version DoVersion(Snapshotter sn);

	void LoadPostProcess(Snapshotter sn);
}
