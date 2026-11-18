namespace UWGame.SimSide.Snapshots;

public interface ILookUpCollectible : ISnapshot
{
	bool SnapshotThis { get; }

	/// <summary>
	/// PORT FIX. Whether this collection holds the running game's state, so that a load empties it
	/// before reading the save (Sim.ClearData). A saved collection does. So can one that is not
	/// written as a collection because each member is saved by its owner - a vehicle's seats.
	/// </summary>
	bool ClearedWithGame => SnapshotThis;

	int LoadPostProcessOrder { get; }

	void ClearCollection();
}
