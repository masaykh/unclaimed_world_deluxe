using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class UpgradableBuildingContainer : Container, IGarrison, IStorage, IUpgrades, IExit
{
	private ItemStorage storage;

	private Garrison garrison;

	private UpgradeItems upgradeItems;

	private bool preventRecursion;

	private ExitDoor simplifiedDoorToUseIndex;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public bool DockOpen => true;

	public bool UsesRallyPointAfterUndock => true;

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

	public UpgradableBuildingContainer(Entity parent)
		: base(parent)
	{
		UpgradableBuildingContainerType upgradableBuildingContainerType = (UpgradableBuildingContainerType)parent.EntityType.ContainerType;
		storage = new ItemStorage(parent, hasFixedCapacity: true, upgradableBuildingContainerType.ItemStorageType);
		garrison = new Garrison(parent);
		if (parent.EntityType.ContainerType.CanBeUpgraded)
		{
			upgradeItems = new UpgradeItems(parent);
		}
	}

	public UpgradableBuildingContainer()
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
		if (garrison != null)
		{
			garrison.IterateContained(del);
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

	public void GetDebugMarkers()
	{
		ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		if (storage != null)
		{
			storage.GetContainedItemsList(rule, list);
		}
		if (garrison != null)
		{
			garrison.GetContainedItemsList(rule, list);
		}
		if (upgradeItems != null)
		{
			upgradeItems.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public int GetNoOfAgentsInside()
	{
		return garrison.GetNoOfAgentsInside();
	}

	public override bool Contains(EntityID entityID)
	{
		if (!storage.Contains(entityID) && (garrison == null || !garrison.Contains(entityID)))
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
		if (Garrison.EntityBelongs(entity))
		{
			return garrison.Add(entity.EntityID);
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
		if (!flag)
		{
			flag = garrison.Remove(entity.EntityID);
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
		garrison.UncontainAllEntities();
		upgradeItems.DestroyAllEntities();
	}

	public bool IsDoorAvailable()
	{
		return false;
	}

	public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
	{
		return ExitAndEntrance.ReserveDoorForEntryOrExit(Parent, entity, exiting, ref simplifiedDoorToUseIndex);
	}

	public void UseDoor(Entity entity, ExitDoor door, bool exiting)
	{
	}

	public void UnreserveDoor(ExitDoor door)
	{
	}

	public void SetRallyPoint(Vector3 pos, ExitDoor door)
	{
	}

	public Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable)
	{
		return ExitAndEntrance.GetRallyPoint(this, door);
	}

	public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
	{
		return ExitAndEntrance.GetNaturalRallyPoint(this, ref rallyPoint, offset);
	}

	public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
	{
		return ExitAndEntrance.GetDoorPosition(this, ref position, ref door, out doorThatWasUsed);
	}

	public Vector3 ComputeAccessPoint()
	{
		return GetRallyPoint();
	}

	public bool IsClearToApproach(Entity docker)
	{
		return IsDoorAvailable();
	}

	public bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index)
	{
		index = -1;
		return false;
	}

	public bool IsClearToEnter(Entity docker)
	{
		return IsDoorAvailable();
	}

	public bool IsClearToAdvance(Entity docker, int dockerIndex)
	{
		return true;
	}

	public void GetEnterPosition(ref Entity docker, ref Vector3 position)
	{
	}

	public void GetDockPosition(ref Entity docker, ref Vector3 position)
	{
	}

	public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
	{
	}

	public void OnApproachRallyReached(ref Entity docker)
	{
	}

	public void OnDockReached(ref Entity docker)
	{
	}

	public void OnDepartureRallyReached(ref Entity docker)
	{
	}

	public bool Action(ref Entity docker)
	{
		return true;
	}

	public void CancelDock(ref Entity docker)
	{
	}

	public bool IsAllowedtoDock(ref Entity dockingEntity)
	{
		return true;
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
		garrison = (Garrison)sn.DoISnapshot(garrison);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (storage != null)
		{
			storage.LoadPostProcess(sn);
		}
		garrison.LoadPostProcess(sn);
		if (upgradeItems != null)
		{
			upgradeItems.LoadPostProcess(sn);
		}
	}
}
