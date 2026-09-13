using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.GatheringSites;

public class GatheringSite : IExit, ILookUp<GatheringSite, GatheringSiteID>, ISnapshot
{
	private GatheringSiteType gatheringSiteType;

	private GatheringSiteTypeID snapshotType;

	private bool closed;

	private bool positionsDirty = true;

	private List<VisitorSpot> visitorSpotList;

	private Vector3 location;

	private bool hasEverAddedVisitor;

	private GatheringSiteID id = GatheringSiteID.Invalid;

	private static GatheringSiteID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private List<VisitorSpot> VisitorSpots
	{
		get
		{
			if (visitorSpotList == null)
			{
				visitorSpotList = new List<VisitorSpot>();
			}
			if (positionsDirty)
			{
				RefreshVisitorSpots();
			}
			return visitorSpotList;
		}
	}

	public bool DockOpen => true;

	public bool UsesRallyPointAfterUndock => true;

	public GatheringSiteID ID
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

	public bool IsSnapshotted { get; set; }

	public GatheringSite(GatheringSiteType siteType, Vector3 position)
	{
		AddToLookup();
		gatheringSiteType = siteType;
		location = position;
	}

	public GatheringSite(Entity newParent)
	{
		AddToLookup();
		gatheringSiteType = newParent.EntityType.GatheringSiteType;
	}

	public GatheringSite()
	{
	}

	public void SetLocation(Vector3 location)
	{
		this.location = location;
	}

	public int GetNumCurrentVisitors()
	{
		if (hasEverAddedVisitor)
		{
			return 0;
		}
		int num = 0;
		foreach (VisitorSpot visitorSpot in VisitorSpots)
		{
			if (visitorSpot.EntityID != EntityID.Invalid)
			{
				num++;
			}
		}
		return num;
	}

	public bool CanAddVisitor(ref Entity visitor)
	{
		RefreshVisitorSpots();
		return IsDoorAvailable();
	}

	public Vector2? AddVisitor(ref Entity visitingEntity)
	{
		if (!CanAddVisitor(ref visitingEntity))
		{
			return null;
		}
		hasEverAddedVisitor = true;
		UnreserveMissingVisitors();
		VisitorSpot visitorSpot = IsVisitor(visitingEntity.EntityID);
		if (visitorSpot != null && visitorSpot.Arrived)
		{
			return visitorSpot.WorldLocation;
		}
		RemoveVisitor(visitingEntity.EntityID);
		RefreshVisitorSpots();
		VisitorSpot visitorSpot2 = VisitorSpots.Find((VisitorSpot v) => v.EntityID == EntityID.Invalid);
		if (visitorSpot2 == null)
		{
			return null;
		}
		float num = (visitingEntity.PlaySiteLocation.ToVector2() - visitorSpot2.WorldLocation).LengthSquared();
		int num2 = 7;
		foreach (VisitorSpot visitorSpot3 in VisitorSpots)
		{
			if (visitorSpot3.EntityID == EntityID.Invalid && visitorSpot3 != visitorSpot2)
			{
				float num3 = (visitingEntity.PlaySiteLocation.ToVector2() - visitorSpot3.WorldLocation).LengthSquared();
				if (num3 < num)
				{
					num = num3;
					visitorSpot2 = visitorSpot3;
				}
				if (--num2 <= 0)
				{
					break;
				}
			}
		}
		visitorSpot2.EntityID = visitingEntity.EntityID;
		UpdateVisitorPositionsToDraw();
		return visitorSpot2.WorldLocation;
	}

	private void UnreserveMissingVisitors()
	{
		List<EntityID> list = new List<EntityID>();
		foreach (VisitorSpot visitorSpot in VisitorSpots)
		{
			Entity entity = Entity.FindByID(visitorSpot.EntityID);
			if (entity == null)
			{
				list.Add(visitorSpot.EntityID);
			}
			else if (visitorSpot.Arrived && (entity.PlaySiteLocation - visitorSpot.WorldLocation.ToVector3()).LengthSquared() > 81f)
			{
				list.Add(visitorSpot.EntityID);
			}
		}
		foreach (EntityID item in list)
		{
			RemoveVisitor(item);
		}
		UpdateVisitorPositionsToDraw();
	}

	public void RemoveVisitor(EntityID visitorID)
	{
		if (hasEverAddedVisitor)
		{
			VisitorSpot visitorSpot = VisitorSpots.Find((VisitorSpot v) => v.EntityID == visitorID);
			if (visitorSpot != null)
			{
				visitorSpot.EntityID = EntityID.Invalid;
				visitorSpot.Arrived = false;
				UpdateVisitorPositionsToDraw();
			}
		}
	}

	private VisitorSpot IsVisitor(EntityID visitorID)
	{
		if (!hasEverAddedVisitor)
		{
			return null;
		}
		return VisitorSpots.Find((VisitorSpot v) => v.EntityID == visitorID);
	}

	public void RefreshVisitorSpots()
	{
		if (visitorSpotList == null)
		{
			visitorSpotList = new List<VisitorSpot>();
		}
		visitorSpotList.Clear();
		float num = gatheringSiteType.arc.Radius;
		if (gatheringSiteType.MaxVisitors <= 0)
		{
			throw new Exception("zero points or less in a visitor place finder");
		}
		Vector2 vector = location.ToVector2();
		float num2 = (float)(gatheringSiteType.arc.MinAngle * (Math.PI / 180.0));
		float num3 = (float)(gatheringSiteType.arc.MaxAngle * (Math.PI / 180.0));
		float num4 = num3 - num2;
		float num5 = (float)Math.PI * 2f;
		float num6 = num5 * num * num4 / num5;
		float seatSize = gatheringSiteType.SeatSize;
		float num7 = num4 * seatSize / num6;
		float num8 = num2;
		int num9 = gatheringSiteType.MaxVisitors * 2;
		int num10 = 0;
		for (int i = 0; i < num9; i++)
		{
			vector = location.ToVector2();
			vector.X += (float)(Math.Sin(num8) * (double)num);
			vector.Y += (float)(Math.Cos(num8) * (double)num);
			Vector2 vector2 = vector;
			vector2 -= location.ToVector2();
			vector2.Normalize();
			float num11 = (float)The.Sim.GameplayRandomGenerator.NextDouble("VisitorPlaceFinder") * (seatSize * 0.25f);
			vector2.X *= num11;
			vector2.Y *= num11;
			vector += vector2;
			num8 += num7;
			if (num8 > num3 - num7 * 0.5f)
			{
				num8 -= num4;
				num += seatSize;
				num6 = num5 * num * num4 / num5;
				num7 = num4 * seatSize / num6;
			}
			if (!The.Map.WorldLocationIsOnMap(vector))
			{
				continue;
			}
			Point point = MapManager.WorldPosToSubtile(new Vector3(vector.X, vector.Y, location.Z));
			if (!MapManager.IsBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot].GetValue(point.X, point.Y)))
			{
				VisitorSpot visitorSpot = new VisitorSpot(EntityID.Invalid, vector);
				Vector2 vector3 = vector;
				visitorSpot.DistanceSquared = (vector3 - location.ToVector2()).LengthSquared();
				visitorSpotList.Add(visitorSpot);
				if (++num10 >= gatheringSiteType.MaxVisitors)
				{
					break;
				}
			}
		}
		visitorSpotList.Sort();
		UpdateVisitorPositionsToDraw();
		positionsDirty = false;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	private void UpdateVisitorPositionsToDraw()
	{
		_ = hasEverAddedVisitor;
	}

	public bool IsDoorAvailable()
	{
		if (closed)
		{
			return false;
		}
		if (GetNumCurrentVisitors() >= gatheringSiteType.MaxVisitors)
		{
			return false;
		}
		return true;
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
		return location;
	}

	public bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true)
	{
		rallyPoint.X = (rallyPoint.Y = (rallyPoint.Z = 0f));
		return false;
	}

	public void GetDebugMarkers()
	{
	}

	public bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable)
	{
		position.X = (position.Y = (position.Z = 0f));
		doorThatWasUsed = door;
		return false;
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
		return IsDoorAvailable();
	}

	public void GetEnterPosition(ref Entity docker, ref Vector3 position)
	{
		position = GetRallyPoint();
	}

	public void GetDockPosition(ref Entity docker, ref Vector3 position)
	{
	}

	public void GetDeparturePosition(ref Entity docker, ref Vector3 position)
	{
		position = GetRallyPoint();
	}

	public void OnApproachRallyReached(ref Entity docker)
	{
	}

	public void OnDockReached(ref Entity d)
	{
		Entity docker = d;
		VisitorSpot visitorSpot = VisitorSpots.Find((VisitorSpot v) => v.EntityID == docker.EntityID);
		if (visitorSpot != null && (visitorSpot.WorldLocation.ToVector3() - docker.PlaySiteLocation).LengthSquared() < 4f)
		{
			visitorSpot.Arrived = true;
			UpdateVisitorPositionsToDraw();
		}
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

	public GatheringSiteID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= GatheringSiteID.Invalid)
		{
			throw new Exception("Astounding, GatheringSiteID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public GatheringSiteID SnapshotID(Snapshotter sn, GatheringSiteID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != GatheringSiteID.Invalid)
		{
			LookUp<GatheringSite, GatheringSiteID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = GatheringSiteID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<GatheringSite, GatheringSiteID>.Remove(this);
	}

	void ILookUp<GatheringSite, GatheringSiteID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = GatheringSiteID.First;
	}

	void ILookUp<GatheringSite, GatheringSiteID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<GatheringSite, GatheringSiteID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		closed = sn.DoBool(closed);
		snapshotType = sn.SnapshotID<GatheringSiteType, GatheringSiteTypeID>(gatheringSiteType).Value;
		hasEverAddedVisitor = sn.DoBool(hasEverAddedVisitor);
		location = sn.DoVector3(location);
		positionsDirty = sn.DoBool(positionsDirty);
		visitorSpotList = sn.DoList(visitorSpotList);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		gatheringSiteType = LookUp<GatheringSiteType, GatheringSiteTypeID>.FindByID(snapshotType);
		if (visitorSpotList == null)
		{
			return;
		}
		foreach (VisitorSpot visitorSpot in visitorSpotList)
		{
			visitorSpot.LoadPostProcess(sn);
		}
	}
}
