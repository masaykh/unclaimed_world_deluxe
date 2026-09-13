using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.Vehicles;

public class Vehicle : Component
{
	public float Roll;

	public float Pitch;

	public const float CommonLowestHaulingSpeed = 0.8f;

	public const float CommonCarryLimit = 6f;

	public Entity DrivenBy;

	public Aircraft Aircraft;

	public List<Entity> Passengers = new List<Entity>();

	public List<Passenger> PassengerItinerary = new List<Passenger>();

	public float CargoCapacityTakenUpByPassengers;

	public List<PassengerOrCargoSlot> Slots = new List<PassengerOrCargoSlot>();

	public int WaitingForPassengers;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Vehicle(Entity parent)
		: base(parent)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)Parent.EntityType.ContainerType;
		if (vehicleContainerType.Aircraft != null)
		{
			Aircraft = new Aircraft();
		}
		if (vehicleContainerType.PassengerOrCargoSlotTypes == null)
		{
			return;
		}
		PassengerOrCargoSlotType[] passengerOrCargoSlotTypes = vehicleContainerType.PassengerOrCargoSlotTypes;
		foreach (PassengerOrCargoSlotType passengerOrCargoSlotType in passengerOrCargoSlotTypes)
		{
			PassengerOrCargoSlot passengerOrCargoSlot = new PassengerOrCargoSlot(this)
			{
				PassengerOrCargoSlotType = passengerOrCargoSlotType
			};
			if (passengerOrCargoSlotType.CargoSlotType != null)
			{
				passengerOrCargoSlot.CargoSlot = new CargoSlot(passengerOrCargoSlot);
			}
			if (passengerOrCargoSlotType.PassengerSlotType != null)
			{
				passengerOrCargoSlot.PassengerSlot = new PassengerSlot();
				if (passengerOrCargoSlotType.PassengerSlotType.IsDriversSeat)
				{
					passengerOrCargoSlot.DriversSlot = new DriversSlot();
				}
			}
			Slots.Add(passengerOrCargoSlot);
		}
	}

	public Vehicle()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		sn.DoUnknownObject(Aircraft);
		sn.DoFloat(CargoCapacityTakenUpByPassengers);
		Snapshotter.Log("Not snapshotting Vehicle.DrivenBy... use an EntityID", Snapshotter.LogPriority.low);
		sn.DoList(PassengerItinerary);
		sn.DoList(Passengers);
		sn.DoFloat(Pitch);
		sn.DoFloat(Roll);
		sn.DoList(Slots);
		sn.DoInt32(WaitingForPassengers);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void TellPassengerToGetOff(Entity sendingEntity, Passenger passenger)
	{
		passenger.Entity.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));
		if (PassengerItinerary.Contains(passenger))
		{
			PassengerItinerary.Remove(passenger);
		}
	}

	public void TellPassengerToGetOff(Entity sendingEntity, Entity passenger)
	{
		passenger.SendMessage(new Message(sendingEntity, Message.MessageTypes.GetOff, null));
		int num = -1;
		for (int i = 0; i < PassengerItinerary.Count; i++)
		{
			if (PassengerItinerary[i].Entity == passenger)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			PassengerItinerary.RemoveAt(num);
		}
	}

	private void RecalculateCargoCapacity()
	{
		CargoCapacityTakenUpByPassengers = 0f;
		foreach (PassengerOrCargoSlot slot in Slots)
		{
			if (slot.CargoSlot != null && ((slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null) || (slot.DriversSlot != null && slot.DriversSlot.Driver != null)))
			{
				CargoCapacityTakenUpByPassengers += slot.PassengerOrCargoSlotType.CargoSlotType.Capacity;
			}
		}
		if (Parent.AgentStorage.ItemStorage != null)
		{
			Parent.AgentStorage.ItemStorage.RecalculateTotalCapacity();
		}
	}

	public bool EnterVehicleAsPassenger(Entity entity, PassengerOrCargoSlot slot)
	{
		Passengers.Add(entity);
		entity.PassengerInVehicle = Parent.EntityID;
		slot.PassengerSlot.Passenger = entity;
		slot.PassengerSlot.TargetedBy = null;
		RecalculateCargoCapacity();
		return false;
	}

	public bool EnterVehicleAsDriver(Entity entity, PassengerOrCargoSlot slot)
	{
		DrivenBy = entity;
		entity.DrivingVehicle = Parent.EntityID;
		slot.DriversSlot.Driver = entity;
		slot.DriversSlot.TargetedBy = null;
		RecalculateCargoCapacity();
		return false;
	}

	public bool ExitVehicle(Entity entity)
	{
		bool flag = false;
		if (DrivenBy == entity)
		{
			DrivenBy = null;
			entity.DrivingVehicle = null;
			flag = true;
		}
		else if (Passengers != null && Passengers.Contains(entity))
		{
			Passengers.Remove(entity);
			entity.PassengerInVehicle = null;
			flag = true;
		}
		if (flag)
		{
			PassengerOrCargoSlot entityPlaceInVehicle = GetEntityPlaceInVehicle(entity);
			if (entityPlaceInVehicle.PassengerSlot != null)
			{
				entityPlaceInVehicle.PassengerSlot.Passenger = null;
				entityPlaceInVehicle.PassengerSlot.TargetedBy = null;
			}
			if (entityPlaceInVehicle.DriversSlot != null)
			{
				entityPlaceInVehicle.DriversSlot.Driver = null;
				entityPlaceInVehicle.DriversSlot.TargetedBy = null;
			}
			RecalculateCargoCapacity();
		}
		return flag;
	}

	public static void DivideVehicles(List<EntityID> vehiclesToDivide, List<EntityGroup> newOwners)
	{
	}

	public PassengerOrCargoSlot GetEntityPlaceInVehicle(Entity entity)
	{
		foreach (PassengerOrCargoSlot slot in Slots)
		{
			if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger != null && slot.PassengerSlot.Passenger == entity)
			{
				return slot;
			}
			if (slot.DriversSlot != null && slot.DriversSlot.Driver != null && slot.DriversSlot.Driver == entity)
			{
				return slot;
			}
		}
		return null;
	}

	public void GetRendezvousPoint(Vector3 vehicleLocation, Vector3 entityLocation, out Vector3 rendezvousLocation)
	{
		Common.GetLocationAtDistance(vehicleLocation, entityLocation, 60f, out rendezvousLocation);
	}

	public bool HasIntelligenceOnboard()
	{
		if (DrivenBy != null)
		{
			return true;
		}
		foreach (Entity passenger in Passengers)
		{
			if (passenger.Intelligence != null)
			{
				return true;
			}
		}
		return false;
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
	{
		if (Slots != null)
		{
			List<PassengerOrCargoSlot> list = null;
			foreach (PassengerOrCargoSlot slot in Slots)
			{
				if (slot.CargoSlot != null && !slot.CargoSlot.IsFull() && (slot.PassengerSlot == null || (slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null)) && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)))
				{
					if (list == null)
					{
						list = new List<PassengerOrCargoSlot>();
					}
					list.Add(slot);
				}
			}
			if (list != null)
			{
				list.Sort((PassengerOrCargoSlot a, PassengerOrCargoSlot b) => b.CargoSlot.BulkCarried.CompareTo(a.CargoSlot.BulkCarried));
				float num = bulkToLoad;
				List<PassengerOrCargoSlot> list2 = new List<PassengerOrCargoSlot>();
				foreach (PassengerOrCargoSlot item in list)
				{
					float remainingRoom = item.CargoSlot.GetRemainingRoom();
					num -= remainingRoom;
					list2.Add(item);
					if (num <= 0f)
					{
						break;
					}
				}
				return list2;
			}
			return null;
		}
		return null;
	}

	public void Destroy()
	{
		if (Slots == null)
		{
			return;
		}
		foreach (PassengerOrCargoSlot slot in Slots)
		{
			slot.Destroy();
		}
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToUnload)
	{
		if (Slots != null)
		{
			List<PassengerOrCargoSlot> list = null;
			foreach (PassengerOrCargoSlot slot in Slots)
			{
				if (slot.CargoSlot != null && slot.CargoSlot.BulkCarried > 0f)
				{
					if (list == null)
					{
						list = new List<PassengerOrCargoSlot>();
					}
					list.Add(slot);
				}
			}
			if (list != null)
			{
				list.Sort((PassengerOrCargoSlot a, PassengerOrCargoSlot b) => a.CargoSlot.BulkCarried.CompareTo(b.CargoSlot.BulkCarried));
				float num = bulkToUnload;
				List<PassengerOrCargoSlot> list2 = new List<PassengerOrCargoSlot>();
				foreach (PassengerOrCargoSlot item in list)
				{
					float bulkCarried = item.CargoSlot.BulkCarried;
					num -= bulkCarried;
					list2.Add(item);
					if (num <= 0f)
					{
						break;
					}
				}
				return list2;
			}
			return null;
		}
		return null;
	}

	public void UpdateCargoSlotsWithAttachedModels()
	{
		foreach (PassengerOrCargoSlot slot in Slots)
		{
			if (slot.CargoSlot == null)
			{
				continue;
			}
			AttachPoint attachPointFromKeyName = Parent.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName);
			if (attachPointFromKeyName != null)
			{
				if (slot.CargoSlot.BulkCarried > 0f)
				{
					Parent.Renderable.AttachPooledObjectIfPossible("box", attachPointFromKeyName, AttacheePoint.Bottom, saveToSnapshot: true);
				}
				else
				{
					Parent.Renderable.RemoveAttachedBoxModelsFromHand();
				}
			}
		}
	}

	public void EmptyAllCargoSlots()
	{
		foreach (PassengerOrCargoSlot slot in Slots)
		{
			if (slot.CargoSlot != null)
			{
				slot.CargoSlot.BulkCarried = 0f;
			}
		}
	}

	public PassengerOrCargoSlot GetFreeDriversSlot()
	{
		if (Slots != null)
		{
			float num = 0f;
			PassengerOrCargoSlot result = null;
			{
				foreach (PassengerOrCargoSlot slot in Slots)
				{
					if (slot.DriversSlot != null && slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null && slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > num)
					{
						result = slot;
					}
				}
				return result;
			}
		}
		return null;
	}

	public PassengerOrCargoSlot GetPlaceForNewPassenger()
	{
		if (Slots != null)
		{
			float num = 0f;
			PassengerOrCargoSlot result = null;
			{
				foreach (PassengerOrCargoSlot slot in Slots)
				{
					if (slot.PassengerSlot != null && slot.PassengerSlot.Passenger == null && slot.PassengerSlot.TargetedBy == null && (slot.DriversSlot == null || (slot.DriversSlot.Driver == null && slot.DriversSlot.TargetedBy == null)) && (slot.CargoSlot == null || (slot.CargoSlot.BulkCarried == 0f && slot.CargoSlot.TargetedByHauler == null)) && slot.PassengerOrCargoSlotType.PassengerSlotType.Comfort > num)
					{
						result = slot;
					}
				}
				return result;
			}
		}
		return null;
	}

	public Vector2 GetRelativePointRotated(Vector2? pointOnModel)
	{
		if (pointOnModel.HasValue)
		{
			Matrix matrix = Matrix.CreateRotationZ(Parent.Rotation);
			return new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y) + Vector2.Transform(pointOnModel.Value, matrix);
		}
		return new Vector2(Parent.PlaySiteLocation.X, Parent.PlaySiteLocation.Y);
	}

	public void ComputeRelativePointInWorld(Vector2 pointOnModel, out Vector3 transformedLocation)
	{
		Matrix matrix = Matrix.CreateRotationX(Roll) * Matrix.CreateRotationZ(Parent.Rotation);
		Vector2 location = Parent.PlaySiteLocation.ToVector2() + Vector2.Transform(pointOnModel, matrix);
		if (MapManager.IsPointReachableInStraightLine(Parent.PlaySiteLocation, location.ToVector3()))
		{
			transformedLocation = location.ToVector3();
		}
		else
		{
			transformedLocation = Parent.PlaySiteLocation;
		}
	}

	public static bool FindParkingSpot(Point from, Allegiance allegiance, IKnownEntityData vehicle, Rectangle tileArea, SurfaceType.TransportType transport, ProtectionLevel protectionLevel, EntityType driverType, ThreatStance approach, Point targetTile, out Vector3 foundLocation)
	{
		Point? point = null;
		foundLocation = Vector3.Zero;
		Point point2 = targetTile;
		if (The.Map.IsNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile))
		{
			return false;
		}
		allegiance.SharedKnowledge.GetMovementMap(protectionLevel, driverType, approach);
		SurfaceType.TransportType transportType = transport;
		if (transportType == SurfaceType.TransportType.Air)
		{
			transportType = SurfaceType.TransportType.OffRoad;
		}
		bool[][] array = Structure.CreateIsBlockedMapAvoidConstructions(tileArea, The.Map.TerrainCosts[transportType]);
		byte[][] map = null;
		Common.InitJaggedArray(ref map, Common.GetJaggedArrayWidth(array), Common.GetJaggedArrayHeight(array));
		byte b = 40;
		byte notPassableValue = 10;
		int centerValue = 40;
		int paramValue = 18;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(map);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(map);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				map[i][j] = b;
			}
		}
		CreatePassageMap(vehicle, tileArea, map, vehicle.BoundingRadius3D, array, notPassableValue);
		byte[][] array2 = null;
		if (point.HasValue)
		{
			array2 = Common.CloneJaggedArray(map);
		}
		InfluenceMap.DrawLinearInfluenceCircle(pos: new Point((point2.X - tileArea.X) * 3, (point2.Y - tileArea.Y) * 3), map: map, centerValue: centerValue, operation: InfluenceMap.Operation.AddToExisting, falloffYesNo: InfluenceMap.Falloff.Yes, circleParam: InfluenceMap.CircleParameter.Radius, paramValue: paramValue);
		Point point3 = new Point(-1, -1);
		Point bestSubtilePoint;
		int bestSubtileLocationThatIsntBlocked = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(map, array, out bestSubtilePoint);
		if (point.HasValue)
		{
			Point pos = new Point((point.Value.X - tileArea.X) * 3, (point.Value.Y - tileArea.Y) * 3);
			InfluenceMap.DrawLinearInfluenceCircle(array2, pos, centerValue, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.Radius, paramValue);
			Point bestSubtilePoint2;
			int bestSubtileLocationThatIsntBlocked2 = InfluenceMap.GetBestSubtileLocationThatIsntBlocked(array2, array, out bestSubtilePoint2);
			if (bestSubtileLocationThatIsntBlocked2 > bestSubtileLocationThatIsntBlocked)
			{
				point3 = bestSubtilePoint2;
			}
			else if (bestSubtileLocationThatIsntBlocked > bestSubtileLocationThatIsntBlocked2)
			{
				point3 = bestSubtilePoint;
			}
			else
			{
				if (bestSubtileLocationThatIsntBlocked == -1)
				{
					The.Map.RegisterNoParkingSpot(vehicle.EntityType, protectionLevel, driverType, approach, targetTile);
					return false;
				}
				point3 = ((!(Common.DistanceOctile(from, bestSubtilePoint) < Common.DistanceOctile(from, bestSubtilePoint2))) ? bestSubtilePoint2 : bestSubtilePoint);
			}
		}
		else
		{
			point3 = bestSubtilePoint;
		}
		if (point3.X != -1)
		{
			MapManager.SubtileAndTilePosToWorldPos(point3, new Point(tileArea.Left, tileArea.Top), out foundLocation);
			if (foundLocation.X < 0f || foundLocation.Y < 0f)
			{
				throw new Exception();
			}
			return true;
		}
		return false;
	}

	public static Rectangle GetSurroundingAreaUsingEntityRadius(IKnownEntityData entity, Point destination)
	{
		int num = (int)((float)GameData.Instance.AIConstants.AreaSizeRadiusForFindingParkingSpot * entity.BoundingRadius3D / 48f);
		Entity mainBuildingOnTile = The.Map.TileMap[destination.X][destination.Y].GetMainBuildingOnTile();
		Point point;
		Point point2;
		if (mainBuildingOnTile == null)
		{
			point = The.Map.ClampTileMapPosition(new Point(destination.X - num, destination.Y - num));
			point2 = The.Map.ClampTileMapPosition(new Point(destination.X + num, destination.Y + num));
		}
		else
		{
			point = The.Map.ClampTileMapPosition(new Point(mainBuildingOnTile.MapPosition.Value.X - num, mainBuildingOnTile.MapPosition.Value.Y - num));
			point2 = The.Map.ClampTileMapPosition(new Point(mainBuildingOnTile.MapPosition.Value.X + mainBuildingOnTile.EntityType.StructureType.WidthInTiles + num, mainBuildingOnTile.MapPosition.Value.Y + mainBuildingOnTile.EntityType.StructureType.HeightInTiles + num));
		}
		return new Rectangle(point.X, point.Y, point2.X - point.X, point2.Y - point.Y);
	}

	public static void CreatePassageMap(IKnownEntityData entity, Rectangle tileArea, byte[][] subtileMap, float agentRadius, bool[][] isBlockedMap, byte notPassableValue)
	{
		int paramValue = (int)(agentRadius / 16f);
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(subtileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(subtileMap);
		for (int i = 0; i < jaggedArrayWidth; i++)
		{
			for (int j = 0; j < jaggedArrayHeight; j++)
			{
				if (isBlockedMap[i][j])
				{
					InfluenceMap.DrawLinearInfluenceCircle(subtileMap, new Point(i, j), notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, paramValue);
				}
			}
		}
		TerrainTile[][] tileMap = The.Map.TileMap;
		Vector3 relativeTo = MapManager.EdgeOfTileToWorldPos(tileArea.Left, tileArea.Top);
		for (int k = tileArea.X; k < tileArea.X + tileArea.Width; k++)
		{
			for (int l = tileArea.Y; l < tileArea.Y + tileArea.Height; l++)
			{
				TerrainTile terrainTile = tileMap[k][l];
				if (terrainTile.EntitiesOnTile == null)
				{
					continue;
				}
				foreach (Entity item in terrainTile.EntitiesOnTile)
				{
					if (item != entity && !item.ContainedBy.HasValue && !item.IsInsideVehicle() && item.Renderable.RenderAsModel != null)
					{
						int paramValue2 = (int)((agentRadius + item.BoundingRadius3D) / 16f);
						Point pos = MapManager.WorldPosToRelativeSubtile(item.PlaySiteLocation, relativeTo);
						InfluenceMap.DrawLinearInfluenceCircle(subtileMap, pos, notPassableValue, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, paramValue2);
					}
				}
			}
		}
	}
}
