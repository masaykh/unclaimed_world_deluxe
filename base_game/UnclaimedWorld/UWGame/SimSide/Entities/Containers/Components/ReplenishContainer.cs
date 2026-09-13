using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

internal class ReplenishContainer : Container, IReplenishes, IHasReplenishItems
{
	private ReplenishItems replenishItems;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ReplenishItems ReplenishItems => replenishItems;

	public ReplenishContainer(Entity parent)
		: base(parent)
	{
		replenishItems = new ReplenishItems(parent);
	}

	public ReplenishContainer()
	{
	}

	public override void Destroy()
	{
		replenishItems.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
		replenishItems.Destroy();
	}

	public bool IsReplenishing(EntityID entityID)
	{
		return Contains(entityID);
	}

	private void RemoveOutdatedItem(AmmoOfType ammoOfType, EntityID entityID)
	{
		ammoOfType.Items.Remove(entityID);
		ammoOfType.TotalIsDirty = true;
	}

	public override void IterateContained(Action<Entity> iterateMethod)
	{
		replenishItems.IterateContained(iterateMethod);
	}

	public override void ResetPlaySiteRegulators()
	{
		replenishItems.ResetPlaySiteRegulators();
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		replenishItems.GetContainedItemsList(rule, list);
		return list;
	}

	public override bool Contains(EntityID entityID)
	{
		return replenishItems.Contains(entityID);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (Remove(itemToRemove))
		{
			AddToContain(exchangeWithItem, null, null, ignoreCapacity: true);
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		replenishItems.Remove(entity.EntityID);
		return true;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		replenishItems.Add(entity.EntityID);
		return true;
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
		replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		replenishItems.LoadPostProcess(sn);
	}
}
