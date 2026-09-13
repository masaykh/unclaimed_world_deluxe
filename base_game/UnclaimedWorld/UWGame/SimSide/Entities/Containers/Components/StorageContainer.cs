using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

internal class StorageContainer : Container, IStorage
{
	private ItemStorage storage;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public StorageContainer(Entity parent)
		: base(parent)
	{
		storage = new ItemStorage(parent, hasFixedCapacity: true, ((StorageContainerType)parent.EntityType.ContainerType).ItemStorageType);
	}

	public StorageContainer()
	{
	}

	public override void IterateContained(Action<Entity> del)
	{
		if (storage != null)
		{
			storage.IterateContained(del);
		}
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		if (storage != null)
		{
			storage.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		return StorageCompartment.NormalStorage;
	}

	public override void Destroy()
	{
		storage.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
		storage.Destroy();
	}

	public override bool Contains(EntityID item)
	{
		return storage.Contains(item);
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return storage.StorageSpaces;
	}

	public Storage FindStorage(StorageID storageID)
	{
		return storage.FindStorage(storageID);
	}

	public Storage GetStoredIn(Entity entity)
	{
		return storage.GetStoredIn(entity.EntityID);
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		return storage.Add(entity, placeInStorage, ignoreCapacity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		StorageCondition storageConditions = storage.GetStorageConditions(itemToRemove.EntityID);
		if (Remove(itemToRemove))
		{
			AddToContain(exchangeWithItem, null, storageConditions, ignoreCapacity: true);
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		return storage.Remove(entity, removeFromChildStorage: true);
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
		storage = (ItemStorage)sn.DoISnapshot(storage);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		storage.LoadPostProcess(sn);
	}
}
