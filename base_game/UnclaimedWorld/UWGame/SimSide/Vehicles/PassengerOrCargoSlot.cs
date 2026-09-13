using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Vehicles;

public class PassengerOrCargoSlot : ILookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>, ISnapshot
{
	public PassengerOrCargoSlotType PassengerOrCargoSlotType;

	public PassengerSlot PassengerSlot;

	public CargoSlot CargoSlot;

	public DriversSlot DriversSlot;

	private Vehicle parent;

	private EntityID snapshotParent;

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

	public PassengerOrCargoSlot(Vehicle parent)
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
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (parent != null)
		{
			snapshotParent = parent.Parent.ID;
		}
		snapshotParent = sn.DoEntityID(snapshotParent);
		sn.Ignore(CargoSlot);
		sn.Ignore(PassengerSlot);
		sn.Ignore(PassengerOrCargoSlotType);
		sn.Ignore(parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = Entity.FindByID(snapshotParent).Vehicle;
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
	}
}
