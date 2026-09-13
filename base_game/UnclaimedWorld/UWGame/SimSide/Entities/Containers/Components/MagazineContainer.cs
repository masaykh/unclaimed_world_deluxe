using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

internal class MagazineContainer : Container, IReplenishes
{
	private Dictionary<EntityType, AmmoOfType> AmmoItems = new Dictionary<EntityType, AmmoOfType>();

	private List<EntityID> DegradedItems = new List<EntityID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public MagazineContainer(Entity parent)
		: base(parent)
	{
	}

	public MagazineContainer()
	{
	}

	public override void Destroy()
	{
		UncontainAllEntities(0.4f, 0.3f);
	}

	public bool IsReplenishing(EntityID entityID)
	{
		if (Contains(entityID))
		{
			return !DegradedItems.Contains(entityID);
		}
		return false;
	}

	public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null)
	{
		foreach (KeyValuePair<EntityType, AmmoOfType> ammoItem in AmmoItems)
		{
			for (int num = ammoItem.Value.Items.Count - 1; num >= 0; num--)
			{
				EntityID entityID = ammoItem.Value.Items[num];
				Entity entity = Entity.FindByID(entityID);
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
					RemoveOutdatedItem(ammoItem.Value, entityID);
				}
			}
		}
	}

	private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
	{
		ammoOfType.Items.Remove(entityID);
		ammoOfType.TotalIsDirty = true;
	}

	public override void IterateContained(Action<Entity> iterateMethod)
	{
		foreach (KeyValuePair<EntityType, AmmoOfType> ammoItem in AmmoItems)
		{
			for (int num = ammoItem.Value.Items.Count - 1; num >= 0; num--)
			{
				EntityID entityID = ammoItem.Value.Items[num];
				Entity entity = Entity.FindByID(entityID);
				if (entity != null)
				{
					iterateMethod(entity);
				}
				else
				{
					RemoveOutdatedItem(ammoItem.Value, entityID);
				}
			}
		}
		for (int num2 = DegradedItems.Count - 1; num2 >= 0; num2--)
		{
			EntityID entityID = DegradedItems[num2];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				iterateMethod(entity);
			}
			else
			{
				DegradedItems.RemoveAt(num2);
			}
		}
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		foreach (KeyValuePair<EntityType, AmmoOfType> ammoItem in AmmoItems)
		{
			for (int num = ammoItem.Value.Items.Count - 1; num >= 0; num--)
			{
				EntityID entityID = ammoItem.Value.Items[num];
				Entity entity = Entity.FindByID(entityID);
				if (entity != null)
				{
					if (rule == null || rule(entity))
					{
						list.Add(entity);
					}
				}
				else
				{
					RemoveOutdatedItem(ammoItem.Value, entityID);
				}
			}
		}
		for (int num2 = DegradedItems.Count - 1; num2 >= 0; num2--)
		{
			EntityID entityID = DegradedItems[num2];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				if (rule(entity))
				{
					list.Add(entity);
				}
			}
			else
			{
				DegradedItems.RemoveAt(num2);
			}
		}
		return list;
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		if (AmmoItems.TryGetValue(entity.EntityType, out var value))
		{
			value.Items.Remove(entity.EntityID);
			value.TotalIsDirty = true;
		}
		else
		{
			DegradedItems.Remove(entity.EntityID);
		}
		return true;
	}

	public override bool Contains(EntityID entityID)
	{
		foreach (KeyValuePair<EntityType, AmmoOfType> ammoItem in AmmoItems)
		{
			foreach (EntityID item in ammoItem.Value.Items)
			{
				if (item == entityID)
				{
					return true;
				}
			}
		}
		return DegradedItems.Contains(entityID);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (AmmoItems.TryGetValue(itemToRemove.EntityType, out var value))
		{
			value.Items.Remove(itemToRemove.EntityID);
			value.TotalIsDirty = true;
		}
		DegradedItems.Add(exchangeWithItem.EntityID);
		exchangeWithItem.ContainedBy = Parent.EntityID;
	}

	public bool HasAmmo(EntityType ammoType, int noOfRounds)
	{
		if (AmmoItems.TryGetValue(ammoType, out var value))
		{
			return value.TotalRounds >= noOfRounds;
		}
		return false;
	}

	public int GetTotalAmmo()
	{
		return AmmoItems.Sum((KeyValuePair<EntityType, AmmoOfType> a) => a.Value.TotalRounds);
	}

	public void GetAmmoStatus(ref Dictionary<EntityType, int> ammo)
	{
		ammo = new Dictionary<EntityType, int>();
		foreach (KeyValuePair<EntityType, AmmoOfType> ammoItem in AmmoItems)
		{
			ammo.Add(ammoItem.Key, ammoItem.Value.TotalRounds);
		}
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		throw new NotImplementedException();
	}

	protected override bool AddToContainList(Entity entityToReplenishWith, out Entity exceedingAmmoItem, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
	{
		if (!AmmoItems.TryGetValue(entityToReplenishWith.EntityType, out var value))
		{
			value = new AmmoOfType
			{
				Items = new List<EntityID>()
			};
			AmmoItems.Add(entityToReplenishWith.EntityType, value);
		}
		MagazineContainerType magazineContainerType = (MagazineContainerType)Parent.EntityType.ContainerType;
		exceedingAmmoItem = null;
		int num = value.TotalRounds + entityToReplenishWith.Item.Ammunition.NoOfRounds - magazineContainerType.MaxCapacity;
		if (num > 0)
		{
			entityToReplenishWith.Item.Ammunition.NoOfRounds = magazineContainerType.MaxCapacity - value.TotalRounds;
			exceedingAmmoItem = new Entity(entityToReplenishWith.EntityType);
			exceedingAmmoItem.Initialize(entityToReplenishWith.Site);
			exceedingAmmoItem.InitializeModelAndOnScreenFunctionality();
			exceedingAmmoItem.Item.Ammunition.NoOfRounds = num;
		}
		if (!MergeAmmoItems(entityToReplenishWith, value.Items))
		{
			entityToReplenishWith.Item.Replenishes = Parent.EntityID;
			value.Items.Add(entityToReplenishWith.EntityID);
		}
		value.RecomputeTotalRounds();
		for (int num2 = DegradedItems.Count - 1; num2 >= 0; num2--)
		{
			Entity entity = Entity.FindByID(DegradedItems[num2]);
			if (entity != null)
			{
				Parent.Contains.Uncontain(entity);
			}
		}
		return true;
	}

	private bool MergeAmmoItems(Entity entityToReplenishWith, List<EntityID> existingItems)
	{
		Ammunition ammunition = entityToReplenishWith.Item.Ammunition;
		int maxNoOfRounds = entityToReplenishWith.EntityType.ItemType.AmmunitionType.MaxNoOfRounds;
		for (int num = existingItems.Count - 1; num >= 0; num--)
		{
			Entity entity = Entity.FindByID(existingItems[num]);
			if (entity != null)
			{
				Item item = entity.Item;
				Ammunition ammunition2 = entity.Item.Ammunition;
				if (ammunition2.NoOfRounds + ammunition.NoOfRounds <= maxNoOfRounds)
				{
					ammunition2.NoOfRounds += ammunition.NoOfRounds;
					item.MergeItems(entityToReplenishWith);
					return true;
				}
			}
			else
			{
				existingItems.RemoveAt(num);
			}
		}
		return false;
	}

	public int SpendAmmo(EntityType typeOfAmmo, int noOfRounds, EntityGroup owner)
	{
		int num = noOfRounds;
		if (AmmoItems.TryGetValue(typeOfAmmo, out var value))
		{
			for (int num2 = value.Items.Count - 1; num2 >= 0; num2--)
			{
				Entity entity = Entity.FindByID(value.Items[num2]);
				if (entity != null)
				{
					Item item = entity.Item;
					int num3 = Math.Min(item.Ammunition.NoOfRounds, num);
					item.Ammunition.NoOfRounds = item.Ammunition.NoOfRounds - num3;
					num -= num3;
					if (item.Ammunition.NoOfRounds <= 0)
					{
						entity.Destroy();
					}
					if (num <= 0)
					{
						break;
					}
				}
				else
				{
					RemoveOutdatedItem(value, value.Items[num2]);
				}
			}
		}
		value?.RecomputeTotalRounds();
		owner.SetAmmoDirty(typeOfAmmo);
		return noOfRounds - num;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		AmmoItems = sn.DoDictionary(AmmoItems);
		DegradedItems = sn.DoList(DegradedItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
	}
}
