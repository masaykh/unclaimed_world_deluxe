using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Snapshots;

public class ActionLookup<T> : ILookUpCollectible, ISnapshot
{
	private static Dictionary<MethodID, Action<T>> collection;

	private static ActionLookup<T> instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new ActionLookup<T>();
			Sim.AddLookupCollectible(typeof(ActionLookup<T>), instance);
			collection = new Dictionary<MethodID, Action<T>>();
		}
	}

	public void ClearCollection()
	{
		collection.Clear();
	}

	public static MethodID? GetID(Action<T> method)
	{
		if (method == null)
		{
			return null;
		}
		return collection.First((KeyValuePair<MethodID, Action<T>> k) => k.Value == method).Key;
	}

	public static Action<T> FindByID(MethodID id)
	{
		collection.TryGetValue(id, out var value);
		return value;
	}

	public static void Remove(MethodID id)
	{
		collection.Remove(id);
	}

	public static void Add(MethodID id, Action<T> instance)
	{
		if (id == MethodID.First || id == MethodID.Invalid)
		{
			throw new Exception("Invalid MethodID");
		}
		_ = typeof(T) == typeof(IKnownProcess);
		collection.Add(id, instance);
	}

	public static MethodID AddWithNewID(Action<T> instance)
	{
		_ = typeof(T) == typeof(IKnownProcess);
		MethodID uniqueID = MethodCounter.GetUniqueID();
		collection.Add(uniqueID, instance);
		return uniqueID;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode == Snapshotter.Mode.Save || sn.mode == Snapshotter.Mode.Load)
		{
			collection.Clear();
		}
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
public class ActionLookup : ILookUpCollectible, ISnapshot
{
	private static Dictionary<MethodID, Action> collection = new Dictionary<MethodID, Action>();

	private static ActionLookup instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new ActionLookup();
			Sim.AddLookupCollectible(typeof(ActionLookup), instance);
		}
	}

	public void ClearCollection()
	{
		collection.Clear();
	}

	public static MethodID? GetID(Action method)
	{
		if (method == null)
		{
			return null;
		}
		return collection.First((KeyValuePair<MethodID, Action> k) => k.Value == method).Key;
	}

	public static Action FindByID(MethodID id)
	{
		collection.TryGetValue(id, out var value);
		return value;
	}

	public static void Remove(MethodID id)
	{
		collection.Remove(id);
	}

	public static void Add(MethodID id, Action instance)
	{
		if (id == MethodID.First || id == MethodID.Invalid)
		{
			throw new Exception("Invalid MethodID");
		}
		collection.Add(id, instance);
	}

	public static MethodID AddWithNewID(Action instance)
	{
		MethodID uniqueID = MethodCounter.GetUniqueID();
		collection.Add(uniqueID, instance);
		return uniqueID;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode == Snapshotter.Mode.Save || sn.mode == Snapshotter.Mode.Load)
		{
			collection.Clear();
		}
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
