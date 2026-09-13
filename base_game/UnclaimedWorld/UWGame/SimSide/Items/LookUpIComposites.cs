using System;
using System.Collections.Generic;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class LookUpIComposites : ILookUpCollectible, ISnapshot
{
	private static SortedDictionary<CompositeID, EntityID> entityCollection;

	private static Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>> machineBodyPartCollection;

	private static LookUpIComposites instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpIComposites();
			Sim.AddLookupCollectible(typeof(IComposite), instance);
			machineBodyPartCollection = new Dictionary<CompositeID, Tuple<EntityID, MemoryFactID, BodyPartID>>();
			entityCollection = new SortedDictionary<CompositeID, EntityID>();
		}
	}

	public void ClearCollection()
	{
		machineBodyPartCollection.Clear();
		entityCollection.Clear();
	}

	public static IComposite FindByID(CompositeID id)
	{
		if (machineBodyPartCollection.TryGetValue(id, out var value))
		{
			Body body = null;
			Entity entity = Entity.FindByID(value.Item1);
			if (entity != null)
			{
				body = entity.Body;
			}
			else
			{
				MemoryFact memoryFact = LookUp<MemoryFact, MemoryFactID>.FindByID(value.Item2);
				if (memoryFact != null)
				{
					body = memoryFact.Body;
				}
			}
			if (body != null)
			{
				return (MachineBodyPart)body.FindBodyPart(value.Item3);
			}
			return null;
		}
		if (entityCollection.TryGetValue(id, out var value2))
		{
			return Entity.FindByID(value2);
		}
		return null;
	}

	public static void Remove(ILookUp<IComposite, CompositeID> instance)
	{
		if (!machineBodyPartCollection.Remove(instance.ID))
		{
			entityCollection.Remove(instance.ID);
		}
		instance.SetInvalid();
	}

	public static void Add(CompositeID id, IComposite instance)
	{
		if (instance is Entity entity)
		{
			entityCollection.Add(id, entity.EntityID);
			return;
		}
		MachineBodyPart machineBodyPart = instance as MachineBodyPart;
		EntityID item = EntityID.Invalid;
		if (machineBodyPart.Body.Parent != null)
		{
			item = machineBodyPart.Body.Parent.ID;
		}
		MemoryFactID item2 = MemoryFactID.Invalid;
		if (machineBodyPart.Body.ParentMemoryFact != null)
		{
			item2 = machineBodyPart.Body.ParentMemoryFact.ID;
		}
		machineBodyPartCollection.Add(id, new Tuple<EntityID, MemoryFactID, BodyPartID>(item, item2, machineBodyPart.BodyPartID));
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		machineBodyPartCollection = sn.DoDictionary(machineBodyPartCollection);
		entityCollection = sn.DoSortedDictionary(entityCollection);
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
	}
}
