using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Trees;

public class LookUpIHasCrops : ILookUpCollectible, ISnapshot
{
	private static SortedDictionary<HasCropsID, LowVegetationID> snapshotLowVegetation = new SortedDictionary<HasCropsID, LowVegetationID>();

	private static SortedDictionary<HasCropsID, EntityID> snapshotTreeCollection = new SortedDictionary<HasCropsID, EntityID>();

	private static SortedDictionary<HasCropsID, IHasCrops> collection = new SortedDictionary<HasCropsID, IHasCrops>();

	private static LookUpIHasCrops instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int LoadPostProcessOrder => 0;

	public bool SnapshotThis => true;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpIHasCrops();
			Sim.AddLookupCollectible(typeof(IHasCrops), instance);
		}
	}

	public void ClearCollection()
	{
		snapshotTreeCollection.Clear();
		snapshotLowVegetation.Clear();
		collection.Clear();
	}

	public static IHasCrops FindByID(HasCropsID id)
	{
		collection.TryGetValue(id, out var value);
		return value;
	}

	public static void Remove(IHasCrops hasCrops)
	{
		collection.Remove(hasCrops.ID);
		hasCrops.SetInvalid();
	}

	public static void Add(HasCropsID hasCropsID, IHasCrops hasCrops)
	{
		collection.Add(hasCropsID, hasCrops);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<HasCropsID, IHasCrops> item in collection)
			{
				if (item.Value is Tree)
				{
					snapshotTreeCollection.Add(item.Key, ((Tree)item.Value).Parent.ID);
				}
				else if (item.Value is LowVegetation)
				{
					snapshotLowVegetation.Add(item.Key, ((LowVegetation)item.Value).ID);
				}
			}
		}
		snapshotTreeCollection = sn.DoSortedDictionary(snapshotTreeCollection);
		snapshotLowVegetation = sn.DoSortedDictionary(snapshotLowVegetation);
		sn.Ignore(collection);
		sn.Ignore(instance);
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
		foreach (KeyValuePair<HasCropsID, EntityID> item in snapshotTreeCollection)
		{
			Entity.FindByID(item.Value).Find<Tree>(out var c);
			collection.Add(item.Key, c);
		}
		foreach (KeyValuePair<HasCropsID, LowVegetationID> item2 in snapshotLowVegetation)
		{
			collection.Add(item2.Key, LookUpSortedDictionary<LowVegetation, LowVegetationID>.FindByID(item2.Value));
		}
		snapshotLowVegetation.Clear();
		snapshotTreeCollection.Clear();
	}
}
