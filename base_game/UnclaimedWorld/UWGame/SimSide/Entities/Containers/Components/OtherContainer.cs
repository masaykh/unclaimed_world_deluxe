using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class OtherContainer : Container
{
	private ItemStorage storage;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float? TotalStoredOutput => storage.TotalStored;

	public float? TotalOutputCapacity => storage.TotalCapacity;

	public OtherContainer(Entity parent)
		: base(parent)
	{
		OtherContainerType otherContainerType = (OtherContainerType)parent.EntityType.ContainerType;
		storage = new ItemStorage(parent, hasFixedCapacity: true, otherContainerType.StorageType);
	}

	public OtherContainer()
	{
	}

	public bool HasCapacityForOutput(Entity item)
	{
		return storage.HasCapacityForItem(item);
	}

	public override void IterateContained(Action<Entity> del)
	{
		storage.IterateContained(del);
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		storage.GetContainedItemsList(rule, list);
		return list;
	}

	public override bool Contains(EntityID entityID)
	{
		if (storage != null)
		{
			return storage.Contains(entityID);
		}
		return false;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		return storage.Add(entity, placeInStorage, ignoreCapacity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (storage.Contains(itemToRemove) && Remove(itemToRemove))
		{
			AddToContain(exchangeWithItem, null, null, ignoreCapacity: true, replenish: false, isProductionOutput: true);
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		return storage.Remove(entity, removeFromChildStorage: true);
	}

	public override void Destroy()
	{
		storage.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
		storage.Destroy();
	}

	public void UncontainAllProductionOutput()
	{
		storage.UncontainAllEntities(0f, 0f);
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
		if (storage != null)
		{
			storage.LoadPostProcess(sn);
		}
	}
}
