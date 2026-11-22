using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Vehicles;

public class PassengerOrCargoSlot : ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>, ISnapshot
{
	public PassengerOrCargoSlotType PassengerOrCargoSlotType;

	public PassengerSlot PassengerSlot;

	public CargoSlot CargoSlot;

	public DriversSlot DriversSlot;

	private VehicleContainer parent;

	private EntityID snapshotParent;
	private EntityID? snapshotPassenger;
	private EntityID? snapshotPassengerTarget;
	private EntityID? snapshotDriver;
	private EntityID? snapshotDriverTarget;
	private EntityID? snapshotCargoTarget;
	private float snapshotCargoBulk;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private PassengerOrCargoSlotID id = PassengerOrCargoSlotID.Invalid;

	private static PassengerOrCargoSlotID IDCounter = PassengerOrCargoSlotID.First;

	public bool IsSnapshotted { get; set; }

	public PassengerOrCargoSlotID ID
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

	public int LoadPostProcessOrder => 0;

	public PassengerOrCargoSlot()
	{
	}

	public PassengerOrCargoSlot(VehicleContainer parent)
	{
		AddToLookup();
		this.parent = parent;
	}

	public void GetEntryPoints(out Vector3 transformedEntry, out Vector3 transformedPointToFace)
	{
		Vector2? offset = PassengerOrCargoSlotType.Entrance.Offset;
		Vector2? pointToFaceAtEntrance = PassengerOrCargoSlotType.PointToFaceAtEntrance;
		if (offset.HasValue)
		{
			parent.ComputeRelativePointInWorld(offset.Value, out transformedEntry);
		}
		else
		{
			transformedEntry = parent.Parent.PlaySiteLocation;
		}
		if (pointToFaceAtEntrance.HasValue)
		{
			parent.ComputeRelativePointInWorld(pointToFaceAtEntrance.Value, out transformedPointToFace);
		}
		else
		{
			transformedPointToFace = parent.Parent.PlaySiteLocation;
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.VehicleSeatState);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (parent != null)
		{
			snapshotParent = parent.Parent.ID;
		}
		snapshotParent = sn.DoEntityID(snapshotParent);
		id = sn.DoEnum(id);
		if (sn.mode == Snapshotter.Mode.Load)
		{
			LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Add(id, this);
			if (id > IDCounter)
			{
				IDCounter = id;
			}
		}
		if (version >= Snapshotter.Version.VehicleSeatState)
		{
			if (sn.mode != Snapshotter.Mode.Load)
			{
				snapshotPassenger = PassengerSlot?.Passenger?.EntityID;
				snapshotPassengerTarget = PassengerSlot?.TargetedBy?.EntityID;
				snapshotDriver = DriversSlot?.Driver?.EntityID;
				snapshotDriverTarget = DriversSlot?.TargetedBy?.EntityID;
				snapshotCargoTarget = CargoSlot?.TargetedByHauler?.EntityID;
				snapshotCargoBulk = CargoSlot?.BulkCarried ?? 0f;
			}
			snapshotPassenger = sn.DoEntityIDNullable(snapshotPassenger);
			snapshotPassengerTarget = sn.DoEntityIDNullable(snapshotPassengerTarget);
			snapshotDriver = sn.DoEntityIDNullable(snapshotDriver);
			snapshotDriverTarget = sn.DoEntityIDNullable(snapshotDriverTarget);
			snapshotCargoTarget = sn.DoEntityIDNullable(snapshotCargoTarget);
			snapshotCargoBulk = sn.DoFloat(snapshotCargoBulk);
		}
		sn.Ignore(CargoSlot);
		sn.Ignore(PassengerSlot);
		sn.Ignore(PassengerOrCargoSlotType);
		sn.Ignore(parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = Entity.FindByID(snapshotParent).Contains as VehicleContainer;
		int index = parent.Slots.IndexOf(this);
		PassengerOrCargoSlotType = ((VehicleContainerType)parent.Parent.EntityType.ContainerType).PassengerOrCargoSlotTypes[index];
		if (PassengerOrCargoSlotType.CargoSlotType != null)
		{
			CargoSlot = new CargoSlot(this)
			{
				BulkCarried = snapshotCargoBulk,
				TargetedByHauler = Entity.FindByID(snapshotCargoTarget)
			};
		}
		if (PassengerOrCargoSlotType.PassengerSlotType != null)
		{
			PassengerSlot = new PassengerSlot
			{
				Passenger = Entity.FindByID(snapshotPassenger),
				TargetedBy = Entity.FindByID(snapshotPassengerTarget)
			};
			if (PassengerOrCargoSlotType.PassengerSlotType.IsDriversSeat)
			{
				DriversSlot = new DriversSlot
				{
					Driver = Entity.FindByID(snapshotDriver),
					TargetedBy = Entity.FindByID(snapshotDriverTarget)
				};
			}
		}
	}

	public PassengerOrCargoSlotID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= PassengerOrCargoSlotID.Invalid)
		{
			throw new Exception("Astounding, PassengerOrCargoSlotID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public PassengerOrCargoSlotID SnapshotID(Snapshotter sn, PassengerOrCargoSlotID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != PassengerOrCargoSlotID.Invalid)
		{
			LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Add(ID, this);
			LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.SetPerformSnapshot(value: false);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Remove(this);
	}

	void ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = PassengerOrCargoSlotID.First;
	}

	public void SetInvalid()
	{
		id = PassengerOrCargoSlotID.Invalid;
	}

	void ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.Create();
		LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.SetPerformSnapshot(value: false);
		// BASE: Not saved as a collection - each vehicle saves its seats - but they are the
		// running game's, so a load empties the lookup and restarts the ID counter (Sim.ClearData).
		LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.SetClearedWithGame(value: true);
	}
}
