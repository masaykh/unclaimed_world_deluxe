using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class WorkshopContainer : Container, IStorage, IReplenishes, IHasReplenishItems, IHoldsProductionOutput
{
	private ItemStorage storage;

	private ItemStorage productionOutput;

	private ReplenishItems replenishItems;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ReplenishItems ReplenishItems => replenishItems;

	public float? TotalStoredOutput
	{
		get
		{
			if (productionOutput != null)
			{
				return productionOutput.TotalStored;
			}
			return null;
		}
	}

	public float? TotalOutputCapacity
	{
		get
		{
			if (productionOutput != null)
			{
				return productionOutput.TotalCapacity;
			}
			return null;
		}
	}

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public WorkshopContainer(Entity parent)
		: base(parent)
	{
		WorkshopContainerType workshopContainerType = (WorkshopContainerType)parent.EntityType.ContainerType;
		storage = new ItemStorage(parent, hasFixedCapacity: true, workshopContainerType.ItemStorageType);
		if (workshopContainerType.RequiresReplenishType != null)
		{
			replenishItems = new ReplenishItems(parent);
		}
		if (workshopContainerType.ProductionOutputStorageType != null)
		{
			productionOutput = new ItemStorage(parent, hasFixedCapacity: true, workshopContainerType.ProductionOutputStorageType);
		}
	}

	public WorkshopContainer()
	{
	}

	public bool HasCapacityForOutput(Entity item)
	{
		if (productionOutput != null)
		{
			return productionOutput.HasCapacityForItem(item);
		}
		return false;
	}

	public Storage FindStorage(StorageID storageID)
	{
		Storage storage = this.storage.FindStorage(storageID);
		if (storage == null)
		{
			storage = productionOutput.FindStorage(storageID);
		}
		return storage;
	}

	public bool IsReplenishing(EntityID entityID)
	{
		if (replenishItems != null)
		{
			return replenishItems.Contains(entityID);
		}
		return false;
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		if (storage != null && storage.HasStorage(storageID))
		{
			return StorageCompartment.NormalStorage;
		}
		if (productionOutput != null && productionOutput.HasStorage(storageID))
		{
			return StorageCompartment.ProductionOutput;
		}
		return StorageCompartment.Stomach;
	}

	public override void IterateContained(Action<Entity> del)
	{
		if (storage != null)
		{
			storage.IterateContained(del);
		}
		if (productionOutput != null)
		{
			productionOutput.IterateContained(del);
		}
		if (replenishItems != null)
		{
			replenishItems.IterateContained(del);
		}
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		if (storage != null)
		{
			storage.GetContainedItemsList(rule, list);
		}
		if (productionOutput != null)
		{
			productionOutput.GetContainedItemsList(rule, list);
		}
		if (replenishItems != null)
		{
			replenishItems.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public override bool Contains(EntityID entityID)
	{
		if (!storage.Contains(entityID) && (productionOutput == null || !productionOutput.Contains(entityID)))
		{
			if (replenishItems != null)
			{
				return replenishItems.Contains(entityID);
			}
			return false;
		}
		return true;
	}

	public Storage GetStoredIn(Entity entity)
	{
		Storage storage = null;
		if (this.storage != null)
		{
			storage = this.storage.GetStoredIn(entity.EntityID);
		}
		if (storage == null && productionOutput != null)
		{
			storage = productionOutput.GetStoredIn(entity.EntityID);
		}
		return storage;
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return storage.StorageSpaces;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		if (replenish)
		{
			return replenishItems.Add(entity.ID);
		}
		if (isProductionOutput)
		{
			return productionOutput.Add(entity, placeInStorage, ignoreCapacity);
		}
		return storage.Add(entity, placeInStorage, ignoreCapacity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (storage != null && storage.Contains(itemToRemove))
		{
			StorageCondition storageConditions = storage.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, storageConditions, ignoreCapacity: true);
			}
		}
		else if (replenishItems != null && replenishItems.Contains(itemToRemove.ID))
		{
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, null, ignoreCapacity: true, replenish: true);
			}
		}
		else if (productionOutput != null && productionOutput.Contains(itemToRemove) && Remove(itemToRemove))
		{
			AddToContain(exchangeWithItem, null, null, ignoreCapacity: true, replenish: false, isProductionOutput: true);
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		bool flag = false;
		if (storage != null)
		{
			flag = storage.Remove(entity, removeFromChildStorage: true);
		}
		if (!flag && productionOutput != null)
		{
			flag = productionOutput.Remove(entity, removeFromChildStorage: true);
		}
		if (!flag && replenishItems != null)
		{
			flag = replenishItems.Remove(entity.ID);
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
		if (productionOutput != null)
		{
			productionOutput.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
			productionOutput.Destroy();
		}
		if (replenishItems != null)
		{
			replenishItems.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
			replenishItems.Destroy();
		}
	}

	public void UncontainAllProductionOutput()
	{
		if (productionOutput != null)
		{
			productionOutput.UncontainAllEntities(0f, 0f);
		}
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
		productionOutput = (ItemStorage)sn.DoISnapshot(productionOutput);
		replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (storage != null)
		{
			storage.LoadPostProcess(sn);
		}
		if (productionOutput != null)
		{
			productionOutput.LoadPostProcess(sn);
		}
		if (replenishItems != null)
		{
			replenishItems.LoadPostProcess(sn);
		}
	}
}
