namespace UWGame.SimSide.Snapshots;

public interface ILookUpCollectible : ISnapshot
{
	bool SnapshotThis { get; }

	int LoadPostProcessOrder { get; }

	void ClearCollection();
}
