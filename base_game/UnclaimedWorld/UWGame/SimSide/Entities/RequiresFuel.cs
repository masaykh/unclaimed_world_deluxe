using System.Collections.Generic;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class RequiresFuel : ISnapshot
{
	public ReplenishItems Parent;

	private Regulator fuelBurningRegulator;

	private bool fuelIsDirty = true;

	private float fuel;

	private float bulkLeftOfCurrentlyBurningItem;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Fuel
	{
		get
		{
			if (fuelIsDirty)
			{
				RecomputeFuel();
			}
			return fuel;
		}
	}

	public bool IsSnapshotted { get; set; }

	public void SetBulkLeftOfBurningItem(float value)
	{
		bulkLeftOfCurrentlyBurningItem = value;
	}

	public RequiresFuel()
	{
	}

	public RequiresFuel(ReplenishItems parent)
	{
		Parent = parent;
		CreateRegulators();
	}

	public void ResetPlaySiteRegulators()
	{
		CreateRegulators();
	}

	public bool HasFuelForDuration(float durationInDays)
	{
		return HasFuelForDuration(durationInDays, Parent.Parent.EntityType, Fuel);
	}

	public static bool HasFuelForDuration(float durationInDays, EntityType entityType, float currentFuel)
	{
		if (currentFuel > 0f)
		{
			return HasEnoughFuel(entityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.GetNeededFuel(durationInDays), currentFuel);
		}
		return false;
	}

	public bool HasEnoughFuel(float neededFuel)
	{
		return neededFuel <= Fuel;
	}

	private static bool HasEnoughFuel(float neededFuel, float currentFuel)
	{
		return neededFuel <= currentFuel;
	}

	public void Refuel(Entity fuel)
	{
		fuelIsDirty = true;
		fuel.Item.Replenishes = Parent.Parent.EntityID;
	}

	private void CreateRegulators()
	{
		fuelBurningRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / (double)GameData.Instance.Constants.UpdateIntervalForEntityComponents, "RequiresFueld");
	}

	private void RecomputeFuel()
	{
		fuelIsDirty = false;
		List<Entity> list = new List<Entity>();
		Parent.GetContainedItemsList(null, list);
		if (list.Count > 0)
		{
			float bulk = list[0].Bulk;
			float num = 0f;
			foreach (Entity item in list)
			{
				num += item.Bulk;
			}
			if (Common.IsGreaterThan(bulkLeftOfCurrentlyBurningItem, 0f))
			{
				fuel = num - bulk + bulkLeftOfCurrentlyBurningItem;
			}
			else
			{
				fuel = num;
			}
		}
		else
		{
			fuel = 0f;
		}
	}

	private bool IsBurning()
	{
		if (Parent.Parent.Find<Tool>(out var c) && c.IsPrepared == true)
		{
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (!fuelBurningRegulator.IsReadyGetTimeElapsedInSeconds(out var secondsSinceLastReady) || !IsBurning())
		{
			return;
		}
		if (Fuel > 0f)
		{
			bulkLeftOfCurrentlyBurningItem -= (float)(secondsSinceLastReady * The.Sim.DateAndTime.DaysPerSecond * (double)Parent.Parent.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.BurnRatePerDay);
			if (bulkLeftOfCurrentlyBurningItem <= 0f)
			{
				Entity entity = null;
				entity = GetValidFuelItem();
				if (entity != null)
				{
					Entity.LogProductionEvent(entity, ProductionStatistics.StatTypes.UsedAsInput, testIfSeen: true);
					entity.Destroy();
				}
				if (Parent.NoOfItems() > 0)
				{
					StartBurningNextFuelItem();
				}
			}
			fuelIsDirty = true;
		}
		if (Parent.NoOfItems() == 0)
		{
			Extinguish();
			bulkLeftOfCurrentlyBurningItem = 0f;
		}
	}

	private Entity GetValidFuelItem()
	{
		Entity result = null;
		List<Entity> list = new List<Entity>();
		Parent.GetContainedItemsList(null, list);
		if (list.Count > 0)
		{
			result = list[0];
		}
		return result;
	}

	private bool StartBurningNextFuelItem()
	{
		Entity validFuelItem = GetValidFuelItem();
		if (validFuelItem != null)
		{
			bulkLeftOfCurrentlyBurningItem = validFuelItem.Bulk;
			return true;
		}
		return false;
	}

	public bool LightFire()
	{
		if (bulkLeftOfCurrentlyBurningItem <= 0f)
		{
			return StartBurningNextFuelItem();
		}
		return true;
	}

	public void Extinguish()
	{
		if (Parent.Parent.Find<Tool>(out var c))
		{
			c.IsPrepared = false;
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		bulkLeftOfCurrentlyBurningItem = sn.DoFloat(bulkLeftOfCurrentlyBurningItem);
		fuel = sn.DoFloat(fuel);
		fuelIsDirty = sn.DoBool(fuelIsDirty);
		sn.Ignore(fuelBurningRegulator);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateRegulators();
	}
}
