using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers;

public class ReplenishItems : ISnapshot
{
	private List<EntityID> containedItems = new List<EntityID>();

	public Entity Parent;

	private EntityID parentID;

	public RequiresFuel RequiresFuel;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ReplenishItems()
	{
	}

	public ReplenishItems(Entity parent)
	{
		Parent = parent;
		if (parent.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType != null)
		{
			RequiresFuel = new RequiresFuel(this);
		}
	}

	public void ResetPlaySiteRegulators()
	{
		if (RequiresFuel != null)
		{
			RequiresFuel.ResetPlaySiteRegulators();
		}
	}

	public bool Start()
	{
		if (RequiresFuel != null)
		{
			return RequiresFuel.LightFire();
		}
		return true;
	}

	public void Destroy()
	{
		if (RequiresFuel != null)
		{
			RequiresFuel.Extinguish();
		}
	}

	public bool HasEnergyForDuration(float durationInDays)
	{
		bool flag = true;
		if (RequiresFuel != null)
		{
			flag = RequiresFuel.HasFuelForDuration(durationInDays);
		}
		bool flag2 = true;
		return flag && flag2;
	}

	public void Update()
	{
		if (RequiresFuel != null)
		{
			RequiresFuel.Update();
		}
	}

	public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null)
	{
		for (int num = containedItems.Count - 1; num >= 0; num--)
		{
			Entity entity = Entity.FindByID(containedItems[num]);
			if (entity != null)
			{
				if (Parent.Contains.Remove(entity))
				{
					Parent.Contains.EjectEntity(entity);
					if (damageStandardDev.HasValue && damageSpread.HasValue)
					{
						float damage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(damageStandardDev.Value, damageSpread.Value);
						entity.DoDamage(damage);
					}
				}
			}
			else
			{
				containedItems.RemoveAt(num);
			}
		}
	}

	private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
	{
		ammoOfType.Items.Remove(entityID);
		ammoOfType.TotalIsDirty = true;
	}

	public bool Add(EntityID entity)
	{
		containedItems.Add(entity);
		return true;
	}

	public void IterateContained(Action<Entity> iterateMethod)
	{
		Garrison.IterateList(containedItems, iterateMethod);
	}

	public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
	{
		for (int num = containedItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = containedItems[num];
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

	public bool Contains(EntityID entityID)
	{
		return containedItems.Contains(entityID);
	}

	public bool Remove(EntityID entity)
	{
		return containedItems.Remove(entity);
	}

	public int NoOfItems()
	{
		return containedItems.Count;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		containedItems = sn.DoList(containedItems);
		parentID = sn.SnapshotID<Entity, EntityID>(Parent).Value;
		RequiresFuel = (RequiresFuel)sn.DoISnapshot(RequiresFuel);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(parentID);
		if (RequiresFuel != null)
		{
			RequiresFuel.Parent = this;
			RequiresFuel.LoadPostProcess(sn);
		}
	}
}
