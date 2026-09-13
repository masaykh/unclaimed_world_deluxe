using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class TerminalContainer : Container, IStorage
{
	private ItemStorage storage;

	private ItemStorage offeredItems;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public float TotalTradeItemStorageCapacity => offeredItems.TotalCapacity;

	public float TotalTradeItemsStored => offeredItems.TotalStored;

	public TerminalContainer(Entity parent)
		: base(parent)
	{
		TerminalContainerType terminalContainerType = (TerminalContainerType)parent.EntityType.ContainerType;
		offeredItems = new ItemStorage(parent, hasFixedCapacity: true, terminalContainerType.OfferedForTradeStorageType);
		if (terminalContainerType.ItemStorageType != null)
		{
			storage = new ItemStorage(parent, hasFixedCapacity: true, terminalContainerType.ItemStorageType);
		}
	}

	public TerminalContainer()
	{
	}

	public Storage FindStorage(StorageID storageID)
	{
		Storage storage = this.storage.FindStorage(storageID);
		if (storage == null)
		{
			storage = offeredItems.FindStorage(storageID);
		}
		return storage;
	}

	public override void IterateContained(Action<Entity> del)
	{
		if (storage != null)
		{
			storage.IterateContained(del);
		}
		if (offeredItems != null)
		{
			offeredItems.IterateContained(del);
		}
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		if (storage != null && storage.HasStorage(storageID))
		{
			return StorageCompartment.NormalStorage;
		}
		return StorageCompartment.OfferedForTrade;
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		if (storage != null)
		{
			storage.GetContainedItemsList(rule, list);
		}
		if (offeredItems != null)
		{
			offeredItems.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public Dictionary<EntityType, List<EntityID>> GetOfferedItems()
	{
		Dictionary<EntityType, List<EntityID>> offeredEntitiesByType = new Dictionary<EntityType, List<EntityID>>();
		if (offeredItems != null)
		{
			offeredItems.IterateContained(delegate(Entity e)
			{
				Common.AddToMultiList(offeredEntitiesByType, e.EntityType, e.ID);
			});
		}
		return offeredEntitiesByType;
	}

	public void ComeOnline()
	{
		SeeTerminalByOtherAllegiances(isDestroyed: false);
	}

	private void SeeOfferedItemByOtherAllegiances(Entity item, bool isAdded)
	{
		Parent.GetAllegianceOrOwner()?.LetOtherAllegiancesSeeEntity(item, isAdded);
	}

	private void SeeTerminalByOtherAllegiances(bool isDestroyed)
	{
		Parent.GetAllegianceOrOwner()?.LetOtherAllegiancesSeeEntity(Parent, !isDestroyed);
	}

	public override bool Contains(EntityID entityID)
	{
		if (!storage.Contains(entityID))
		{
			if (offeredItems != null)
			{
				return offeredItems.Contains(entityID);
			}
			return false;
		}
		return true;
	}

	public Storage GetStoredIn(Entity entity)
	{
		Storage storedIn = storage.GetStoredIn(entity.EntityID);
		if (storedIn == null)
		{
			storedIn = offeredItems.GetStoredIn(entity.EntityID);
		}
		return storedIn;
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return storage.StorageSpaces;
	}

	public Dictionary<StorageCondition, Storage> GetTradeOffersStorageSpaces()
	{
		return offeredItems.StorageSpaces;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool isReplenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		switch (compartment ?? StorageCompartment.NormalStorage)
		{
		case StorageCompartment.OfferedForTrade:
		{
			bool num = offeredItems.Add(entity, placeInStorage, ignoreCapacity);
			if (num)
			{
				SeeOfferedItemByOtherAllegiances(entity, isAdded: true);
			}
			return num;
		}
		case StorageCompartment.NormalStorage:
			return storage.Add(entity, placeInStorage, ignoreCapacity);
		default:
			return false;
		}
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (offeredItems != null && offeredItems.Contains(itemToRemove))
		{
			StorageCondition storageConditions = offeredItems.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, storageConditions, ignoreCapacity: true);
			}
		}
		else if (storage != null && storage.Contains(itemToRemove))
		{
			StorageCondition storageConditions2 = storage.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, storageConditions2, ignoreCapacity: true);
			}
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		bool flag = false;
		if (storage != null)
		{
			flag = storage.Remove(entity, removeFromChildStorage: true);
		}
		if (!flag && offeredItems != null)
		{
			flag = offeredItems.Remove(entity, removeFromChildStorage: true);
			if (flag)
			{
				SeeOfferedItemByOtherAllegiances(entity, isAdded: false);
			}
		}
		return flag;
	}

	public override void Destroy()
	{
		if (storage != null)
		{
			storage.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
			storage.Destroy();
		}
		if (offeredItems != null)
		{
			offeredItems.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
			offeredItems.Destroy();
		}
		SeeTerminalByOtherAllegiances(isDestroyed: true);
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
		offeredItems = (ItemStorage)sn.DoISnapshot(offeredItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (storage != null)
		{
			storage.LoadPostProcess(sn);
		}
		if (offeredItems != null)
		{
			offeredItems.LoadPostProcess(sn);
		}
	}
}
