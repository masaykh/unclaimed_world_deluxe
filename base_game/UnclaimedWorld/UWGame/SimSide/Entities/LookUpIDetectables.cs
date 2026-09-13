using System.Collections.Generic;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class LookUpIDetectables : ILookUpCollectible, ISnapshot
{
	private static Dictionary<DetectableID, EntityID> snapshotEntityCollection = new Dictionary<DetectableID, EntityID>();

	private static Dictionary<DetectableID, ResourceID> snapshotResourceCollection = new Dictionary<DetectableID, ResourceID>();

	private static Dictionary<DetectableID, IDetectable> collection = new Dictionary<DetectableID, IDetectable>();

	private static LookUpIDetectables instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpIDetectables();
			Sim.AddLookupCollectible(typeof(IDetectable), instance);
		}
	}

	public void ClearCollection()
	{
		snapshotResourceCollection.Clear();
		snapshotEntityCollection.Clear();
		collection.Clear();
	}

	public static IDetectable FindByID(DetectableID id)
	{
		collection.TryGetValue(id, out var value);
		return value;
	}

	public static void Remove(IDetectable detectable)
	{
		collection.Remove(detectable.ID);
		detectable.SetInvalid();
	}

	public static void Add(DetectableID detectableID, IDetectable iDetectable)
	{
		collection.Add(detectableID, iDetectable);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<DetectableID, IDetectable> item in collection)
			{
				if (item.Value is ResourceContainer)
				{
					snapshotResourceCollection.Add(item.Key, ((ResourceContainer)item.Value).ID);
				}
				else if (item.Value is Entity)
				{
					snapshotEntityCollection.Add(item.Key, ((Entity)item.Value).ID);
				}
			}
		}
		snapshotResourceCollection = sn.DoDictionary(snapshotResourceCollection);
		snapshotEntityCollection = sn.DoDictionary(snapshotEntityCollection);
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
		collection.Clear();
		foreach (KeyValuePair<DetectableID, ResourceID> item in snapshotResourceCollection)
		{
			collection.Add(item.Key, LookUp<ResourceContainer, ResourceID>.FindByID(item.Value));
		}
		foreach (KeyValuePair<DetectableID, EntityID> item2 in snapshotEntityCollection)
		{
			collection.Add(item2.Key, Entity.FindByID(item2.Value));
		}
		snapshotEntityCollection.Clear();
		snapshotResourceCollection.Clear();
	}
}
