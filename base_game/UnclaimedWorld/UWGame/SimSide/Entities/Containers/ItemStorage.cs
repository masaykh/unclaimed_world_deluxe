using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers;

public class ItemStorage : ISnapshot
{
	private bool capacityIsFixed;

	public float TotalCapacity;

	private float? storageTypeFixedCapacity;

	public static StorageID IDCounter;

	public List<EntityID> StoredItems = new List<EntityID>();

	public Dictionary<StorageCondition, Storage> StorageSpaces;

	private Dictionary<EntityID, Storage> StorageLookup = new Dictionary<EntityID, Storage>();

	private Dictionary<EntityID, StorageID> snapshotStorageLookup = new Dictionary<EntityID, StorageID>();

	private bool totalStoredIsDirty = true;

	private float totalStored;

	public Entity Parent;

	private EntityID snapshotParent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored
	{
		get
		{
			if (totalStoredIsDirty)
			{
				CalculateTotalStored();
				totalStoredIsDirty = false;
			}
			return totalStored;
		}
	}

	public float UnusedCapacity => TotalCapacity - TotalStored;

	public bool IsSnapshotted { get; set; }

	public bool HasStorage(StorageID storageID)
	{
		return StorageSpaces.Any((KeyValuePair<StorageCondition, Storage> s) => s.Value.ID == storageID);
	}

	public Storage GetStoredIn(EntityID entity)
	{
		Storage value = null;
		StorageLookup.TryGetValue(entity, out value);
		return value;
	}

	public StorageCondition GetStorageConditions(EntityID entity)
	{
		return GetStoredIn(entity)?.StorageConditions;
	}

	public void RecalculateTotalCapacity()
	{
		if (!capacityIsFixed)
		{
			TotalCapacity = storageTypeFixedCapacity.Value;
			if (Parent.Vehicle != null && Parent.Vehicle.Slots != null)
			{
				TotalCapacity -= Parent.Vehicle.CargoCapacityTakenUpByPassengers;
			}
		}
	}

	public bool Add(Entity item, StorageCondition storageCondition, bool ignoreCapacity = false)
	{
		if (ignoreCapacity || HasCapacityForItem(item))
		{
			if (storageCondition == null)
			{
				if (item.EntityType.NonLivingType != null && StorageSpaces.Count > 1)
				{
					foreach (DegradeType.StorageDamageEstimation conditionDamage in item.EntityType.NonLivingType.FinalDegradeType.ConditionDamages)
					{
						if (StorageSpaces.ContainsKey(conditionDamage.Condition) && AddToStorage(item, conditionDamage.Condition))
						{
							return true;
						}
					}
				}
				else if (AddToStorage(item, GameData.Instance.AllStorageConditions["isolated"]))
				{
					return true;
				}
			}
			else if (StorageSpaces.ContainsKey(storageCondition) && AddToStorage(item, storageCondition))
			{
				return true;
			}
			AddToStorage(item, GameData.Instance.AllStorageConditions["isolated"], testCapacity: false);
			return true;
		}
		return false;
	}

	private bool AddToStorage(Entity item, StorageCondition condition, bool testCapacity = true)
	{
		Storage storage = StorageSpaces[condition];
		if (!testCapacity || storage.HasCapacityForItem(item))
		{
			_ = item.Item;
			storage.Add(item.EntityID);
			StoredItems.Add(item.EntityID);
			StorageLookup.Add(item.EntityID, storage);
			item.ContainedBy = Parent.EntityID;
			totalStoredIsDirty = true;
			return true;
		}
		return false;
	}

	public void IterateContained(Action<Entity> iterateMethod)
	{
		for (int num = StoredItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = StoredItems[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				iterateMethod(entity);
			}
			else
			{
				RemoveOutdatedItem(entityID);
			}
		}
	}

	public bool IterateContainedBreakOnTrue(Container.IterateBoolMethod iterateMethod)
	{
		for (int num = StoredItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = StoredItems[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				if (iterateMethod(entity))
				{
					return true;
				}
			}
			else
			{
				RemoveOutdatedItem(entityID);
			}
		}
		return false;
	}

	public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
	{
		for (int num = StoredItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = StoredItems[num];
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
				RemoveOutdatedItem(entityID);
			}
		}
	}

	public bool Contains(EntityID item)
	{
		return StoredItems.Contains(item);
	}

	public bool Contains(Entity item)
	{
		return StoredItems.Contains(item.EntityID);
	}

	public void UncontainAllEntities(float? damageStandardDev = null, float? damageSpread = null)
	{
		for (int num = StoredItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = StoredItems[num];
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
				RemoveOutdatedItem(entityID);
			}
		}
	}

	public bool Remove(Entity item, bool removeFromChildStorage)
	{
		return Remove(item.EntityID, removeFromChildStorage);
	}

	public bool Remove(EntityID item, bool removeFromChildStorage)
	{
		if (removeFromChildStorage)
		{
			if (StorageLookup.TryGetValue(item, out var value))
			{
				value.Remove(item);
				RemoveFromAuxiliaryCollections(item);
				return true;
			}
			return false;
		}
		RemoveFromAuxiliaryCollections(item);
		return true;
	}

	private void RemoveFromAuxiliaryCollections(EntityID item)
	{
		StorageLookup.Remove(item);
		StoredItems.Remove(item);
		totalStoredIsDirty = true;
	}

	public bool RemoveOutdatedItem(EntityID item)
	{
		foreach (KeyValuePair<StorageCondition, Storage> storageSpace in StorageSpaces)
		{
			if (storageSpace.Value.RemoveOutdatedItem(item))
			{
				RemoveFromAuxiliaryCollections(item);
				return true;
			}
		}
		return false;
	}

	public void Destroy()
	{
		foreach (KeyValuePair<EntityID, Storage> item in StorageLookup)
		{
			item.Value.Destroy();
		}
	}

	public ItemStorage()
	{
	}

	public ItemStorage(Entity entity, bool hasFixedCapacity, ItemStorageType parentStorageType)
	{
		Parent = entity;
		if (hasFixedCapacity)
		{
			TotalCapacity = parentStorageType.GetTotalCapacity();
			storageTypeFixedCapacity = TotalCapacity;
		}
		StorageSpaces = new Dictionary<StorageCondition, Storage>();
		capacityIsFixed = hasFixedCapacity;
		foreach (KeyValuePair<string, StorageType> storageSpace in parentStorageType.StorageSpaces)
		{
			StorageSpaces.Add(GameData.Instance.AllStorageConditions[storageSpace.Key], new Storage(this, GameData.Instance.AllStorageConditions[storageSpace.Key])
			{
				TotalCapacity = storageSpace.Value.Capacity
			});
		}
	}

	private float CalculateTotalStored()
	{
		float num = 0f;
		for (int num2 = StoredItems.Count - 1; num2 >= 0; num2--)
		{
			EntityID entityID = StoredItems[num2];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				num += entity.Bulk;
			}
			else
			{
				StoredItems.Remove(entityID);
			}
		}
		totalStored = num;
		return num;
	}

	public bool HasCapacityForItem(Entity itemToPickUp)
	{
		return HasCapacityForItem(itemToPickUp.Bulk);
	}

	public bool HasCapacityForItem(float itemBulk)
	{
		return Common.IsLessThanOrEqual(itemBulk, TotalCapacity - TotalStored);
	}

	public bool HasCapacityForItemWhenEmpty(Entity itemToPickUp)
	{
		return itemToPickUp.Bulk <= TotalCapacity;
	}

	public bool HasCapacityForItemWhenEmpty(float bulk)
	{
		return bulk <= TotalCapacity;
	}

	public Storage FindStorage(StorageID storageID)
	{
		foreach (KeyValuePair<StorageCondition, Storage> storageSpace in StorageSpaces)
		{
			if (storageSpace.Value.ID == storageID)
			{
				return storageSpace.Value;
			}
		}
		return null;
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = StorageID.First;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		capacityIsFixed = sn.DoBool(capacityIsFixed);
		snapshotParent = sn.SnapshotID<Entity, EntityID>(Parent).Value;
		IDCounter = sn.DoEnum(IDCounter);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotStorageLookup = StorageLookup.ToDictionary((KeyValuePair<EntityID, Storage> s) => s.Key, (KeyValuePair<EntityID, Storage> s) => s.Value.ID);
		}
		snapshotStorageLookup = sn.DoDictionary(snapshotStorageLookup);
		StorageSpaces = sn.DoDictionary(StorageSpaces);
		storageTypeFixedCapacity = sn.DoFloatNullable(storageTypeFixedCapacity);
		StoredItems = sn.DoList(StoredItems);
		TotalCapacity = sn.DoFloat(TotalCapacity);
		totalStored = sn.DoFloat(totalStored);
		totalStoredIsDirty = sn.DoBool(totalStoredIsDirty);
		sn.Ignore(StorageLookup);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(snapshotParent);
		StorageLookup = snapshotStorageLookup.ToDictionary((KeyValuePair<EntityID, StorageID> s) => s.Key, (KeyValuePair<EntityID, StorageID> s) => FindStorage(s.Value));
		if (StorageLookup == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityID, Storage> item in StorageLookup)
		{
			item.Value.LoadPostProcess(sn);
		}
	}
}
