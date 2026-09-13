using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class UpgradableContainer : Container, IStorage, IUpgrades
{
	private ItemStorage storage;

	private UpgradeItems upgradeItems;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades
	{
		get
		{
			if (upgradeItems != null)
			{
				return upgradeItems.ContainedUpgrades;
			}
			return null;
		}
	}

	public UpgradableContainer(Entity parent)
		: base(parent)
	{
		UpgradableContainerType upgradableContainerType = (UpgradableContainerType)parent.EntityType.ContainerType;
		storage = new ItemStorage(parent, hasFixedCapacity: true, upgradableContainerType.ItemStorageType);
		if (parent.EntityType.ContainerType.CanBeUpgraded)
		{
			upgradeItems = new UpgradeItems(parent);
		}
	}

	public UpgradableContainer()
	{
	}

	public Storage FindStorage(StorageID storageID)
	{
		return storage.FindStorage(storageID);
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		if (storage != null && storage.HasStorage(storageID))
		{
			return StorageCompartment.NormalStorage;
		}
		return StorageCompartment.Stomach;
	}

	public override void IterateContained(Action<Entity> del)
	{
		if (storage != null)
		{
			storage.IterateContained(del);
		}
		if (upgradeItems != null)
		{
			upgradeItems.IterateContained(del);
		}
	}

	public override void NotifyBrokenContainedEntity(Entity entity)
	{
		EndEffectsFromUpgraderItem(entity);
	}

	public override void NotifyFunctionalContainedEntity(Entity entity)
	{
		StartEffectsAndSpriteFromUpgraderItem(entity);
	}

	public void EndEffectsFromUpgraderItem(Entity entity)
	{
		if (upgradeItems != null && upgradeItems.Contains(entity.EntityID))
		{
			upgradeItems.EndEffects(entity);
		}
	}

	public void StartEffectsAndSpriteFromUpgraderItem(Entity entity)
	{
		if (upgradeItems != null && upgradeItems.Contains(entity.EntityID))
		{
			upgradeItems.ApplyUpgradeEffects(entity);
		}
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		if (storage != null)
		{
			storage.GetContainedItemsList(rule, list);
		}
		if (upgradeItems != null)
		{
			upgradeItems.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public override bool Contains(EntityID entityID)
	{
		if (!storage.Contains(entityID))
		{
			if (upgradeItems != null)
			{
				return upgradeItems.Contains(entityID);
			}
			return false;
		}
		return true;
	}

	public Storage GetStoredIn(Entity entity)
	{
		Storage result = null;
		if (storage != null)
		{
			result = storage.GetStoredIn(entity.EntityID);
		}
		return result;
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return storage.StorageSpaces;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		if (upgradeCategory != null)
		{
			return upgradeItems.Add(upgradeCategory, entity);
		}
		return storage.Add(entity, placeInStorage, ignoreCapacity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (upgradeItems != null && upgradeItems.Contains(itemToRemove.ID))
		{
			UpgradeCategory upgradeCategory = upgradeItems.GetUpgradeCategory(itemToRemove.ID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, null, ignoreCapacity: true, replenish: false, isProductionOutput: false, assertContainment: true, upgradeCategory);
			}
		}
		else if (storage != null && storage.Contains(itemToRemove))
		{
			StorageCondition storageConditions = storage.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, null, storageConditions, ignoreCapacity: true);
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
		if (!flag && upgradeItems != null)
		{
			flag = upgradeItems.Remove(entity.EntityID);
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
		upgradeItems.DestroyAllEntities();
	}

	public bool IsUpgrade(EntityID entityID)
	{
		if (upgradeItems != null)
		{
			return upgradeItems.Contains(entityID);
		}
		return false;
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
		upgradeItems = (UpgradeItems)sn.DoISnapshot(upgradeItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (storage != null)
		{
			storage.LoadPostProcess(sn);
		}
		if (upgradeItems != null)
		{
			upgradeItems.LoadPostProcess(sn);
		}
	}
}
