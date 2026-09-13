using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Sim.Sim.Entities.Container;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

internal class VehicleContainer : Container, ICrew, ITransport, IGarrison, IStorage, IExit, IHasReplenishItems
{
	private ItemStorage storage;

	private Residence residence;

	private Garrison garrison;

	private ReplenishItems replenishItems;

	private bool preventRecursion;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ReplenishItems ReplenishItems => replenishItems;

	public float TotalStored => storage.TotalStored;

	public float TotalItemStorageCapacity => storage.TotalCapacity;

	public EntityID? Driver
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool DockOpen => true;

	public bool UsesRallyPointAfterUndock => true;

	public VehicleContainer(Entity parent)
		: base(parent)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)parent.EntityType.ContainerType;
		storage = new ItemStorage(parent, hasFixedCapacity: true, vehicleContainerType.ItemStorageType);
		if (vehicleContainerType.MaxPassengers > 0)
		{
			garrison = new Garrison(parent);
		}
		if (parent.EntityType.ContainerType.ResidenceType != null)
		{
			residence = new Residence(parent);
		}
	}

	public VehicleContainer()
	{
	}

	public override void Destroy()
	{
		storage.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
		replenishItems.UncontainAllEntities(GameData.Instance.Constants.ConditionDamageMeanToContentsOfDestroyedContainers, GameData.Instance.Constants.ConditionDamageSpreadToContentsOfDestroyedContainers);
		replenishItems.Destroy();
		if (garrison != null)
		{
			garrison.UncontainAllEntities();
		}
	}

	public Storage FindStorage(StorageID storageID)
	{
		return storage.FindStorage(storageID);
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
		return list;
	}

	public Storage GetStoredIn(Entity entity)
	{
		return storage.GetStoredIn(entity.EntityID);
	}

	public override bool Contains(EntityID entityID)
	{
		if (!storage.Contains(entityID))
		{
			if (garrison != null)
			{
				return garrison.Contains(entityID);
			}
			return false;
		}
		return true;
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return storage.StorageSpaces;
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		return StorageCompartment.NormalStorage;
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		if (slots != null)
		{
			foreach (PassengerOrCargoSlot slot in slots)
			{
				slot.CargoSlot.TargetedByHauler = null;
			}
		}
		bool flag = storage.Remove(entity, removeFromChildStorage: true);
		if (!flag && garrison != null)
		{
			flag = garrison.Remove(entity.EntityID);
		}
		if (flag)
		{
			float num = entity.Bulk;
			if (slots != null)
			{
				foreach (PassengerOrCargoSlot slot2 in slots)
				{
					float bulkCarried = slot2.CargoSlot.BulkCarried;
					if (bulkCarried >= num)
					{
						slot2.CargoSlot.BulkCarried -= num;
						break;
					}
					slot2.CargoSlot.BulkCarried -= bulkCarried;
					num -= bulkCarried;
				}
			}
			if (TotalStored == 0f && Parent.Vehicle != null)
			{
				Parent.Vehicle.EmptyAllCargoSlots();
			}
			if (Parent.Vehicle != null)
			{
				Parent.Vehicle.UpdateCargoSlotsWithAttachedModels();
			}
		}
		if (flag && Parent.Locomotor != null)
		{
			Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
			Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
		}
		return true;
	}

	public int GetNoOfAgentsInside()
	{
		if (garrison != null)
		{
			return garrison.GetNoOfAgentsInside();
		}
		return 0;
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		if (Garrison.EntityBelongs(entity))
		{
			if (garrison != null)
			{
				return garrison.Add(entity.EntityID);
			}
			return false;
		}
		return storage.Add(entity, placeInStorage, ignoreCapacity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		StorageCondition storageConditions = storage.GetStorageConditions(itemToRemove.EntityID);
		if (Remove(itemToRemove))
		{
			AddToContain(exchangeWithItem, null, storageConditions, ignoreCapacity: true);
		}
	}

	public void GetDebugMarkers()
	{
		ExitAndEntrance.GetDebugMarkers(this, ref preventRecursion);
	}

	public bool IsDoorAvailable()
	{
		return false;
	}

	public ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting)
	{
		return ExitDoor.NoneNeeded;
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
		return Parent.PlaySiteLocation;
	}

	public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
	{
		rallyPoint.X = (rallyPoint.X = (rallyPoint.X = 0f));
		return false;
	}

	public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
	{
		PassengerOrCargoSlotType passengerOrCargoSlotType = ((VehicleContainerType)Parent.EntityType.ContainerType).PassengerOrCargoSlotTypes.FirstOrDefault((PassengerOrCargoSlotType s) => s.PassengerSlotType != null && !s.PassengerSlotType.IsDriversSeat);
		position = ExitAndEntrance.GetDoorPosition(this, passengerOrCargoSlotType.Entrance.Offset ?? Vector2.Zero);
		doorThatWasUsed = passengerOrCargoSlotType.Entrance.ExitDoor;
		return true;
	}

	public Vector3 ComputeAccessPoint()
	{
		return GetRallyPoint();
	}

	public bool IsClearToApproach(Entity docker)
	{
		return IsDoorAvailable();
	}

	public bool ReserveApproachPosition(ref Entity docker, ref Vector3 position, out int index)
	{
		ExitDoor exitDoor = ReserveDoorForEntryOrExit(docker, exiting: false);
		if (exitDoor != ExitDoor.NoneAvailable)
		{
			index = (int)exitDoor;
			GetDoorPosition(ref position, exiting: false, out var _, exitDoor);
		}
		index = -1;
		return false;
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
		replenishItems = (ReplenishItems)sn.DoISnapshot(replenishItems);
		residence = (Residence)sn.DoISnapshot(residence);
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
		if (residence != null)
		{
			residence.LoadPostProcess(sn);
		}
		if (garrison != null)
		{
			garrison.LoadPostProcess(sn);
		}
		if (replenishItems != null)
		{
			replenishItems.LoadPostProcess(sn);
		}
	}
}
