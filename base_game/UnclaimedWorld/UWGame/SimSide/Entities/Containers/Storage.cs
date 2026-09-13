using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers;

public class Storage : ISnapshot
{
	public const float AirConTemperature = 294f;

	public const float RefrigeratorTemperature = 278f;

	public const float EarthCooledTemperature = 284f;

	public const float FreezerTemperature = 255f;

	public const float RoomTemperature = 293f;

	public const float Hot = 313f;

	public float TotalCapacity;

	public StorageCondition StorageConditions;

	private bool totalStoredIsDirty = true;

	private float totalStored;

	public List<EntityID> StoredItems = new List<EntityID>();

	public bool IsPowered = true;

	public EntityID Parent;

	private StorageID id = StorageID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float TotalStored
	{
		get
		{
			if (totalStoredIsDirty)
			{
				CalculateTotalStored();
				totalStoredIsDirty = false;
			}
			return totalStored;
		}
	}

	public StorageID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public bool IsSnapshotted { get; set; }

	public Storage(ItemStorage parent, StorageCondition conditions)
	{
		ItemStorage.IDCounter++;
		ID = ItemStorage.IDCounter;
		Parent = parent.Parent.ID;
		StorageConditions = conditions;
	}

	public Storage(Storage original)
	{
		id = original.ID;
		StorageConditions = original.StorageConditions;
		StoredItems = original.StoredItems.ToList();
		TotalCapacity = original.TotalCapacity;
		totalStored = original.TotalStored;
		IsPowered = original.IsPowered;
		Parent = original.Parent;
	}

	public Storage()
	{
	}

	public void Destroy()
	{
	}

	public void Add(EntityID item)
	{
		StoredItems.Add(item);
		totalStoredIsDirty = true;
	}

	public bool Remove(EntityID item)
	{
		if (StoredItems.Remove(item))
		{
			totalStoredIsDirty = true;
			return true;
		}
		return false;
	}

	public bool RemoveOutdatedItem(EntityID item)
	{
		if (StoredItems.Remove(item))
		{
			totalStoredIsDirty = true;
			return true;
		}
		return false;
	}

	public void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem)
	{
		Remove(itemToRemove.EntityID);
		Add(exchangeWithItem.EntityID);
		StoredItems.Remove(itemToRemove.EntityID);
		StoredItems.Add(exchangeWithItem.EntityID);
	}

	public float CalculateTotalStored()
	{
		float num = 0f;
		for (int num2 = StoredItems.Count - 1; num2 >= 0; num2--)
		{
			Entity entity = Entity.FindByID(StoredItems[num2]);
			if (entity != null)
			{
				num += entity.Bulk;
			}
			else
			{
				StoredItems.RemoveAt(num2);
			}
		}
		totalStored = num;
		return num;
	}

	public bool HasCapacityForItem(IKnownEntityData itemToPickUp)
	{
		return itemToPickUp.Bulk <= TotalCapacity - TotalStored;
	}

	public bool HasCapacityForItem(float itemToPickUpBulk)
	{
		return itemToPickUpBulk <= TotalCapacity - TotalStored;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		Parent = sn.DoEnum(Parent);
		IsPowered = sn.DoBool(IsPowered);
		StoredItems = sn.DoList(StoredItems);
		StorageConditions = sn.DoGameData(StorageConditions);
		TotalCapacity = sn.DoFloat(TotalCapacity);
		totalStored = sn.DoFloat(totalStored);
		totalStoredIsDirty = sn.DoBool(totalStoredIsDirty);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
