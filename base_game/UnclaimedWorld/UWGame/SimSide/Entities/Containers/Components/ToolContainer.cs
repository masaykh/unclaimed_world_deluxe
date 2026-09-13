using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class ToolContainer : Container, IReplenishes, IHasReplenishItems, IHoldsProductionOutput
{
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

	public ToolContainer(Entity parent)
		: base(parent)
	{
		ToolContainerType toolContainerType = (ToolContainerType)parent.EntityType.ContainerType;
		if (toolContainerType.RequiresReplenishType != null)
		{
			replenishItems = new ReplenishItems(parent);
		}
		if (toolContainerType.ProductionOutputStorageType != null)
		{
			productionOutput = new ItemStorage(parent, hasFixedCapacity: true, toolContainerType.ProductionOutputStorageType);
		}
	}

	public ToolContainer()
	{
	}

	public bool IsReplenishing(EntityID entityID)
	{
		if (replenishItems != null)
		{
			return replenishItems.Contains(entityID);
		}
		return false;
	}

	public bool HasCapacityForOutput(Entity item)
	{
		if (productionOutput != null)
		{
			return productionOutput.HasCapacityForItem(item);
		}
		return false;
	}

	public override void IterateContained(Action<Entity> del)
	{
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
		if (productionOutput != null)
		{
			productionOutput.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public override bool Contains(EntityID entityID)
	{
		if (productionOutput == null || !productionOutput.Contains(entityID))
		{
			if (replenishItems != null)
			{
				return replenishItems.Contains(entityID);
			}
			return false;
		}
		return true;
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
		return false;
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (replenishItems != null && replenishItems.Contains(itemToRemove.ID))
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
		productionOutput = (ItemStorage)sn.DoISnapshot(productionOutput);
		replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
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
