using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Expeditions;

public class ProductionOrders : ISnapshot
{
	public Dictionary<EntityType, ProductionOrder> Orders = new Dictionary<EntityType, ProductionOrder>();

	private bool ordersAreDirty = true;

	private int totalOrders;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int TotalDirectOrders
	{
		get
		{
			if (ordersAreDirty)
			{
				totalOrders = 0;
				foreach (KeyValuePair<EntityType, ProductionOrder> order in Orders)
				{
					_ = order.Key;
					ProductionOrder value = order.Value;
					if (value.ProductionJobsToComplete.HasValue)
					{
						totalOrders += value.ProductionJobsToComplete.Value;
					}
				}
				ordersAreDirty = false;
			}
			return totalOrders;
		}
	}

	public bool IsSnapshotted { get; set; }

	public ProductionOrders()
	{
		if (Snapshotter.IsSnapshotting)
		{
			return;
		}
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			Orders.Add(allItemType.Value, new ProductionOrder());
		}
	}

	public void SetDirectOrder(EntityType entityType, int amount)
	{
		ProductionOrder productionOrder = Orders[entityType];
		productionOrder.ProductionJobsToComplete = amount;
		productionOrder.AmountToKeepInStore = null;
		ordersAreDirty = true;
	}

	public void SetStandingOrder(EntityType entityType, int amount)
	{
		ProductionOrder productionOrder = Orders[entityType];
		productionOrder.AmountToKeepInStore = amount;
		productionOrder.ProductionJobsToComplete = null;
	}

	public bool OrdersExist(EntityType entityType)
	{
		if (Orders.TryGetValue(entityType, out var value))
		{
			if (value.ProductionJobsToComplete.HasValue && value.ProductionJobsToComplete.Value > 0)
			{
				return true;
			}
			if (value.AmountToKeepInStore.HasValue && value.AmountToKeepInStore.Value > 0)
			{
				return true;
			}
		}
		return false;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Orders = sn.DoDictionary(Orders);
		sn.Ignore(totalOrders);
		sn.Ignore(ordersAreDirty);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (KeyValuePair<EntityType, ProductionOrder> order in Orders)
		{
			order.Value.LoadPostProcess(sn);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
