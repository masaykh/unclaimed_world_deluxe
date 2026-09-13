using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems;

public class LookUpSleepyUpdater<T> : ILookUpCollectible, ISnapshot where T : ISleepingUpdatable
{
	private static Dictionary<SleepyUpdaterID, SleepyUpdater<T>> collection = new Dictionary<SleepyUpdaterID, SleepyUpdater<T>>();

	private static LookUpSleepyUpdater<T> instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int LoadPostProcessOrder => 0;

	public bool SnapshotThis => true;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpSleepyUpdater<T>();
			Sim.AddLookupCollectible(typeof(LookUpSleepyUpdater<T>), instance);
		}
	}

	public void ClearCollection()
	{
		collection.Clear();
		SleepyUpdater<T>.ResetIDCounter();
	}

	public static SleepyUpdater<T> FindByID(SleepyUpdaterID id)
	{
		if (collection.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public static void Remove(SleepyUpdater<T> instance)
	{
		collection.Remove(instance.ID);
		instance.SetInvalid();
	}

	public static void Add(SleepyUpdaterID id, SleepyUpdater<T> instance)
	{
		collection.Add(id, instance);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		sn.Ignore(instance);
		sn.Ignore(collection);
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
