using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers;

internal class Garrison : ISnapshot
{
	private List<EntityID> AgentsInside = new List<EntityID>();

	private Entity parent;

	private EntityID snapshotParent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Garrison(Entity parent)
	{
		this.parent = parent;
	}

	public Garrison()
	{
	}

	public bool Contains(EntityID entity)
	{
		return AgentsInside.Contains(entity);
	}

	public bool Add(EntityID entity)
	{
		if (!AgentsInside.Contains(entity))
		{
			AgentsInside.Add(entity);
		}
		return true;
	}

	public bool Remove(EntityID entity)
	{
		return AgentsInside.Remove(entity);
	}

	public static bool EntityBelongs(Entity entity)
	{
		return entity.EntityType.IntelligenceType != null;
	}

	public int GetNoOfAgentsInside()
	{
		return AgentsInside.Count;
	}

	public void IterateContained(Action<Entity> iterateMethod)
	{
		IterateList(AgentsInside, iterateMethod);
	}

	public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
	{
		for (int num = AgentsInside.Count - 1; num >= 0; num--)
		{
			EntityID entityID = AgentsInside[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				if (rule == null || rule(entity))
				{
					items.Add(entity);
				}
			}
			else
			{
				Remove(entityID);
			}
		}
	}

	public static void IterateList(List<EntityID> containedItems, Action<Entity> iterateMethod)
	{
		for (int num = containedItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = containedItems[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				iterateMethod(entity);
			}
			else
			{
				containedItems.Remove(entityID);
			}
		}
	}

	public void UncontainAllEntities()
	{
		for (int num = AgentsInside.Count - 1; num >= 0; num--)
		{
			Entity entity = Entity.FindByID(AgentsInside[num]);
			if (entity != null)
			{
				if (parent.Contains.Remove(entity))
				{
					parent.Contains.EjectEntity(entity);
				}
			}
			else
			{
				AgentsInside.RemoveAt(num);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Entity, EntityID>(parent).Value;
		AgentsInside = sn.DoList(AgentsInside);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = Entity.FindByID(snapshotParent);
	}
}
