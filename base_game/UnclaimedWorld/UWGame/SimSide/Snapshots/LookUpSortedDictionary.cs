using System;
using System.Collections.Generic;
using System.Linq;

namespace UWGame.SimSide.Snapshots;

public class LookUpSortedDictionary<T, Id> : ILookUpCollectible, ISnapshot where T : ILookUp<T, Id> where Id : struct
{
	private static SortedDictionary<Id, T> collection = new SortedDictionary<Id, T>();

	private static LookUpSortedDictionary<T, Id> instance;

	private static bool performSnapshot = true;

	private static int order = 100;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => performSnapshot;

	public int LoadPostProcessOrder => order;

	public bool IsSnapshotted { get; set; }

	public static void SetPerformSnapshot(bool value)
	{
		performSnapshot = value;
	}

	public static void SetLoadPostProcessOrder(int orderToUse)
	{
		order = orderToUse;
	}

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpSortedDictionary<T, Id>();
			Sim.AddLookupCollectible(typeof(T), instance);
		}
	}

	public void ClearCollection()
	{
		collection.Clear();
		Type typeFromHandle = typeof(T);
		if (!typeFromHandle.IsInterface)
		{
			typeFromHandle.GetMethod("ResetIDCounter").Invoke(null, null);
		}
	}

	public static T FindByID(Id id)
	{
		if (collection.TryGetValue(id, out var value))
		{
			return value;
		}
		return default(T);
	}

	public static T FindByID(Id? id)
	{
		if (id.HasValue && collection.TryGetValue(id.Value, out var value))
		{
			return value;
		}
		return default(T);
	}

	public static void Remove(ILookUp<T, Id> instance)
	{
		collection.Remove(instance.ID);
		instance.SetInvalid();
	}

	public static void Add(Id id, T instance)
	{
		collection.Add(id, instance);
	}

	public static void IterateMembers(Action<T> iterateMethod)
	{
		foreach (KeyValuePair<Id, T> item in collection)
		{
			iterateMethod(item.Value);
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (performSnapshot)
		{
			collection = sn.DoSortedDictionary(collection);
			order = sn.DoInt32(order);
		}
		else
		{
			sn.Ignore(collection);
		}
		sn.Ignore(instance);
		sn.Ignore(performSnapshot);
		sn.Ignore(order);
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
		if (!performSnapshot)
		{
			return;
		}
		foreach (KeyValuePair<Id, T> item in collection.OrderBy((KeyValuePair<Id, T> l) => l.Value.LoadPostProcessOrder))
		{
			if (item.Value is ISnapshot snapshot)
			{
				snapshot.LoadPostProcess(sn);
				snapshot.IsSnapshotted = false;
			}
		}
	}
}
