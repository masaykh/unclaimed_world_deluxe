using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

public class GoalFollowPath : CompositeGoal
{
	private const int numberSpacing = 100;

	public GroupMoveActivity GroupMoveActivity;

	public List<PathFinderNode> Path;

	public List<Waypoint> WaypointPath;

	public GoalMoveToPosition parentGoal;

	private GoalID? snapshotParent;

	private Dictionary<Point, Point> rejectedPassengerDestinations;

	public bool MovingOutOfHarmsWay;

	private Vector3? endLocation;

	public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalFollowPath(Entity owner, List<PathFinderNode> path, GoalMoveToPosition parentGoal, List<EntityGroupID> ownersOfVehicles, Vector3? endLocation, GroupMoveActivity groupMoveActivity)
		: base(owner)
	{
		if (path == null)
		{
			throw new Exception("Path is null!");
		}
		Path = path;
		this.parentGoal = parentGoal;
		base.ownersOfVehicles = ownersOfVehicles;
		this.endLocation = endLocation;
		GroupMoveActivity = groupMoveActivity;
	}

	public GoalFollowPath()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (Path != null)
		{
			List<Vector3> list = SplitPathIntoSegmentsAndSmoothe(Path);
			Path = null;
			InsertAdditionalWaypoints(list);
			int num = 100;
			WaypointPath = new List<Waypoint>();
			foreach (Vector3 item in list)
			{
				WaypointPath.Add(new Waypoint(num, item));
				num += 100;
			}
			if (WaypointPath.Count == 0)
			{
				throw new Exception();
			}
		}
		if (WaypointPath.Count == 0)
		{
			throw new Exception();
		}
		if (!entity.GetDrivenVehicle(out var vehicle))
		{
			base.Status = Status.Failed;
			return;
		}
		if (vehicle != null)
		{
			vehicle.Vehicle.WaitingForPassengers = 0;
			ManagePassengersWithNewPath(vehicle);
		}
		GetNextWaypoint();
	}

	private void InsertAdditionalWaypoints(List<Vector3> path)
	{
		for (int i = 0; i < path.Count; i++)
		{
			Vector3 vector = path[i];
			if (i > 0)
			{
				Vector3 vector2 = path[i - 1];
				if (!WaypointsAreAdjacent(vector2, vector))
				{
					i += AddExtraWaypointsBetweenWaypoints(path, i, vector2, vector);
				}
			}
			else
			{
				Vector3 playSiteLocation = entity.PlaySiteLocation;
				i += AddExtraWaypointsBetweenWaypoints(path, 0, playSiteLocation, vector);
			}
		}
	}

	private void ManagePassengersWithNewPath(Entity drivenVehicle)
	{
		drivenVehicle.Find<Vehicle>(out var c);
		List<Passenger> list = new List<Passenger>();
		for (int i = 0; i < c.PassengerItinerary.Count; i++)
		{
			Passenger passenger = c.PassengerItinerary[i];
			if (c.Passengers.Contains(passenger.Entity))
			{
				if (FindDropoffPoint(entity.PlaySiteLocation, passenger.Destination, out var dropOffWayPointIndex))
				{
					list.Add(new Passenger(null, dropOffWayPointIndex, passenger.Destination, passenger.Entity));
				}
				else
				{
					passenger.Entity.SendMessage(new Message(entity, Message.MessageTypes.GetOff, null));
				}
			}
		}
		c.PassengerItinerary = list;
	}

	private void GetNextWaypoint()
	{
		if (WaypointPath.Count == 0)
		{
			throw new Exception();
		}
		Vector2? lookAheadDirection = null;
		Vector3 playSiteLocation = entity.PlaySiteLocation;
		Waypoint waypoint;
		do
		{
			waypoint = WaypointPath[0];
			WaypointPath.RemoveAt(0);
		}
		while (WaypointPath.Count > 0 && Vector2.DistanceSquared(playSiteLocation.ToVector2(), waypoint.Location.ToVector2()) < 4f);
		if (WaypointPath.Count > 0)
		{
			Vector2 value = (WaypointPath[0].Location - waypoint.Location).ToVector2();
			value.Normalize();
			lookAheadDirection = value;
		}
		base.Status = Status.Active;
		bool flag = WaypointPath.Count == 0;
		float permittedDistanceSquaredToDestination = ((!flag) ? GameData.Instance.Constants.DistanceSquaredLimitForWaypoints : PermittedDistanceSquaredToDestination);
		GoalTraverseEdgeBetweenWaypoints goal = new GoalTraverseEdgeBetweenWaypoints(entity, waypoint.Location, lookAheadDirection, GoalTraverseEdgeBetweenWaypoints.ComputeWaypointRay(waypoint.Location, entity), waypoint.Number, flag, MovingOutOfHarmsWay, ownersOfVehicles, GroupMoveActivity)
		{
			PermittedDistanceSquaredToDestination = permittedDistanceSquaredToDestination
		};
		AddSubgoal(goal);
		if (GroupMoveActivity != null && GroupMoveActivity.IsLeader(entity))
		{
			Waypoint leadersFollowingWaypoint = null;
			if (WaypointPath.Count > 0 && WaypointPath[0].Location != waypoint.Location)
			{
				leadersFollowingWaypoint = WaypointPath[0];
			}
			AssignWaypointsToFollowers(waypoint, leadersFollowingWaypoint, entity, GroupMoveActivity, WaypointPath.Count == 0);
		}
	}

	public static void AssignWaypointsToFollowers(Waypoint leadersNextWaypoint, Waypoint leadersFollowingWaypoint, Entity leader, GroupMoveActivity groupMoveActivity, bool isLastEdge)
	{
		if (groupMoveActivity.Members.Count <= 0)
		{
			return;
		}
		Vector3 playSiteLocation = leader.PlaySiteLocation;
		Vector2 vector = ((leadersFollowingWaypoint != null) ? new Vector2(leadersFollowingWaypoint.Location.X - playSiteLocation.X, leadersFollowingWaypoint.Location.Y - playSiteLocation.Y) : new Vector2(leadersNextWaypoint.Location.X - playSiteLocation.X, leadersNextWaypoint.Location.Y - playSiteLocation.Y));
		Matrix rotMatrix = Matrix.CreateRotationZ((float)Math.Atan2(vector.Y, vector.X));
		for (int i = 0; i < groupMoveActivity.Members.Count; i++)
		{
			Entity entity = groupMoveActivity.Members[i];
			if (entity != leader)
			{
				Intelligence intelligence = entity.Intelligence;
				Vector3 vector2 = groupMoveActivity.ComputePositionFromLeader(entity, leadersNextWaypoint.Location, rotMatrix);
				MovementMap movementMap = intelligence.Allegiance.SharedKnowledge.GetMovementMap(intelligence.ProtectionLevel, entity.EntityType, intelligence.ThreatStance);
				SurfaceType.TransportType transportType = entity.GetTransportType();
				Waypoint groupMoveAssignedWaypoint;
				if (MapManager.IsPathClearToPoint(leadersNextWaypoint.Location, entity.PlaySiteLocation, movementMap, transportType))
				{
					groupMoveAssignedWaypoint = new Waypoint(leadersNextWaypoint.Number, vector2, isLastEdge);
				}
				else
				{
					MovementMap movementMap2 = intelligence.Allegiance.SharedKnowledge.GetMovementMap(intelligence.ProtectionLevel, entity.EntityType, intelligence.ThreatStance);
					Point closestPointToDestination = leader.Intelligence.PathPlanner.GetClosestPointToDestination(movementMap2.Layers[entity.GetTransportType()], vector2, 400, leader.PlaySiteLocation);
					groupMoveAssignedWaypoint = new Waypoint(leadersNextWaypoint.Number, MapManager.TileToWorldPos(closestPointToDestination), isLastEdge);
				}
				intelligence.GroupMoveAssignedWaypoint = groupMoveAssignedWaypoint;
				entity.SendMessage(new Message(Message.MessageTypes.StartGroupMovement));
			}
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (IsDelayed(elapsed))
		{
			return;
		}
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status != Status.Completed)
		{
			return;
		}
		if (WaypointPath.Count > 0)
		{
			GoalTraverseEdgeBetweenWaypoints goalTraverseEdgeBetweenWaypoints = null;
			if (Subgoals.Count > 0)
			{
				goalTraverseEdgeBetweenWaypoints = Subgoals.Peek() as GoalTraverseEdgeBetweenWaypoints;
			}
			if (goalTraverseEdgeBetweenWaypoints == null)
			{
				return;
			}
			if (!entity.GetDrivenVehicle(out var vehicle))
			{
				base.Status = Status.Failed;
			}
			else if (vehicle != null)
			{
				vehicle.Find<Vehicle>(out var c);
				if (c.PassengerItinerary != null && c.PassengerItinerary.Count > 0)
				{
					HandlePassengerDropoffsAndPickups(goalTraverseEdgeBetweenWaypoints, c);
				}
				else
				{
					GetNextWaypoint();
				}
			}
			else
			{
				GetNextWaypoint();
			}
		}
		else
		{
			_ = GroupMoveActivity;
		}
	}

	private void HandlePassengerDropoffsAndPickups(GoalTraverseEdgeBetweenWaypoints traverseEdge, Vehicle vehicleComponent)
	{
		bool flag = false;
		int lastIndex = 0;
		Passenger passenger;
		while (IsAtRendezvous(vehicleComponent, traverseEdge.Number, out passenger, ref lastIndex))
		{
			AddSubgoal(new GoalWaitForPassenger(entity, 12.0, passenger.Entity));
			flag = true;
		}
		while (IsAtDropoff(vehicleComponent, traverseEdge.Number, out passenger))
		{
			vehicleComponent.TellPassengerToGetOff(entity, passenger);
			StartDelay(0.8);
		}
		base.Status = Status.Active;
		if (!flag)
		{
			GetNextWaypoint();
		}
	}

	private bool IsAtRendezvous(Vehicle vehicleComponent, int waypointNumber, out Passenger passenger, ref int lastIndex)
	{
		passenger = null;
		for (int i = lastIndex; i < vehicleComponent.PassengerItinerary.Count; i++)
		{
			int? rendezvousWaypointNumber = vehicleComponent.PassengerItinerary[i].RendezvousWaypointNumber;
			if (waypointNumber == rendezvousWaypointNumber.GetValueOrDefault() && rendezvousWaypointNumber.HasValue && !vehicleComponent.Passengers.Contains(vehicleComponent.PassengerItinerary[i].Entity))
			{
				passenger = vehicleComponent.PassengerItinerary[i];
				lastIndex = i + 1;
				return true;
			}
		}
		return false;
	}

	private bool IsAtDropoff(Vehicle vehicleComponent, int waypointNumber, out Passenger passenger)
	{
		passenger = null;
		for (int i = 0; i < vehicleComponent.PassengerItinerary.Count; i++)
		{
			if (waypointNumber == vehicleComponent.PassengerItinerary[i].DropoffWaypointNumber && vehicleComponent.Passengers.Contains(vehicleComponent.PassengerItinerary[i].Entity))
			{
				passenger = vehicleComponent.PassengerItinerary[i];
				return true;
			}
		}
		return false;
	}

	private Vector3? GetDestination()
	{
		if (WaypointPath.Count > 0)
		{
			return WaypointPath[WaypointPath.Count - 1].Location;
		}
		return null;
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.DrivenVehicleIsNear:
				if (parentGoal != null && parentGoal.VehicleUseByGoal == GoalMoveToPosition.VehicleUse.FreeUpAfterUse && !parentGoal.UsedVehicle.HasValue && !entity.IsInAVehicle())
				{
					if (!message.Sender.GetDrivenVehicle(out var vehicle2))
					{
						return true;
					}
					if (vehicle2 != null)
					{
						vehicle2.Find<Vehicle>(out var c2);
						if (c2.Passengers.Count + c2.WaitingForPassengers < ((VehicleContainerType)vehicle2.EntityType.ContainerType).MaxPassengers)
						{
							Vector3? destination = GetDestination();
							if (destination.HasValue)
							{
								message.Sender.SendMessage(new Message(entity, Message.MessageTypes.PickMeUp, new FromTo(entity.PlaySiteLocation, destination.Value)));
							}
						}
					}
				}
				return true;
			case Message.MessageTypes.PickMeUp:
			{
				if (!entity.GetDrivenVehicle(out var vehicle))
				{
					base.Status = Status.Failed;
					return true;
				}
				if (vehicle != null && FindPassengerRoute(((FromTo)message.OtherInfo).From, ((FromTo)message.OtherInfo).To, out var dropOffWaypointIndex))
				{
					FindPassengerRendezvousPoint(((FromTo)message.OtherInfo).From, out var rendezvousWayPointIndex);
					message.Sender.SendMessage(new Message(entity, Message.MessageTypes.GoToRendezvousPoint, WaypointPath[rendezvousWayPointIndex].Location));
					vehicle.Find<Vehicle>(out var c);
					c.PassengerItinerary.Add(new Passenger(WaypointPath[rendezvousWayPointIndex].Number, WaypointPath[dropOffWaypointIndex.Value].Number, ((FromTo)message.OtherInfo).To, message.Sender));
					c.WaitingForPassengers++;
				}
				return true;
			}
			case Message.MessageTypes.GoToRendezvousPoint:
			{
				if (!message.Sender.GetDrivenVehicle(out var vehicle3))
				{
					return true;
				}
				if (vehicle3 != null)
				{
					vehicle3.Find<Vehicle>(out var c3);
					c3.GetRendezvousPoint((Vector3)message.OtherInfo, entity.PlaySiteLocation, out var rendezvousLocation);
					Vector3 value = rendezvousLocation;
					Vector3? location = entity.Location;
					if (value != location)
					{
						AddSubgoal(new GoalMoveToPosition(entity, rendezvousLocation, ownersOfVehicles, GoalMoveToPosition.VehicleUse.NoVehicle));
					}
					AddSubgoal(new GoalEnterVehicleAsPassenger(entity, vehicle3, this, null, ownersOfVehicles));
				}
				return true;
			}
			case Message.MessageTypes.StartGroupMovement:
				if (GroupMoveActivity.IsFollower(entity))
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}
		return true;
	}

	private void AddRejectedPassengerDestination(Point destination)
	{
		if (rejectedPassengerDestinations == null)
		{
			rejectedPassengerDestinations = new Dictionary<Point, Point>();
		}
		rejectedPassengerDestinations.Add(destination, destination);
	}

	private bool FindPassengerRoute(Vector3 passengerLocation, Vector3 destination, out int? dropOffWaypointIndex)
	{
		dropOffWaypointIndex = null;
		Point point = MapManager.WorldPosToTile(destination);
		if ((rejectedPassengerDestinations != null && rejectedPassengerDestinations.ContainsKey(point)) || WaypointPath.Count < GameData.Instance.AIConstants.ShortestRouteThatAcceptsPassengers)
		{
			return false;
		}
		if (FindDropoffPoint(passengerLocation, destination, out var dropOffWayPointIndex))
		{
			dropOffWaypointIndex = dropOffWayPointIndex;
			return true;
		}
		AddRejectedPassengerDestination(point);
		return false;
	}

	private bool WaypointsAreAdjacent(Vector3 w1, Vector3 w2)
	{
		Point point = MapManager.WorldPosToTile(w1);
		Point point2 = MapManager.WorldPosToTile(w2);
		if (!(point == point2))
		{
			return Common.IsAdjacent(point, point2);
		}
		return true;
	}

	private int AddExtraWaypointsBetweenWaypoints(List<Vector3> path, int startingIndex, Vector3 location1, Vector3 location2)
	{
		float num = Vector2.Distance(location1.ToVector2(), location2.ToVector2());
		float num2 = GameData.Instance.Constants.TilesBetweenAddedWaypoints * 48f;
		int i = 0;
		if (num > num2)
		{
			Vector2 vector = (location2 - location1).ToVector2() / num;
			int num3 = (int)(num / num2);
			Vector3 item = location1;
			for (i = 0; i < num3; i++)
			{
				item += new Vector3(48f * vector, 0f);
				path.Insert(startingIndex, item);
				startingIndex++;
			}
		}
		return i;
	}

	private bool FindDropoffPoint(Vector3 passengerLocation, Vector3 passengerDestination, out int dropOffWayPointIndex)
	{
		float minDistance = 1E+09f;
		FindClosestWaypoint(new Vector3(passengerDestination.X, passengerDestination.Y, 0f), 3, out dropOffWayPointIndex, ref minDistance);
		int num = WaypointPath.Count - 1;
		float num2 = Common.DistanceOctile(WaypointPath[num].Location, passengerDestination);
		if (num2 < minDistance)
		{
			minDistance = num2;
			dropOffWayPointIndex = num;
		}
		if (minDistance < Common.DistanceOctile(passengerLocation, passengerDestination))
		{
			return true;
		}
		return false;
	}

	private bool FindClosestWaypoint(Vector3 targetLocation, int cutOff, out int closestWayPointIndex, ref float minDistance)
	{
		closestWayPointIndex = -1;
		for (int i = 0; i < WaypointPath.Count - cutOff; i++)
		{
			Waypoint waypoint = WaypointPath[i];
			float num = Common.DistanceOctile(targetLocation, waypoint.Location);
			if (num < minDistance)
			{
				minDistance = num;
				closestWayPointIndex = i;
			}
		}
		if (closestWayPointIndex != -1)
		{
			return true;
		}
		return false;
	}

	private void FindPassengerRendezvousPoint(Vector3 passengerLocation, out int rendezvousWayPointIndex)
	{
		float minDistance = 1E+09f;
		FindClosestWaypoint(passengerLocation, 3, out rendezvousWayPointIndex, ref minDistance);
	}

	public override void Terminate()
	{
		base.Terminate();
		entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
	}

	private bool EdgeIsOnRoadOrPath(PathFinderNode from, PathFinderNode to)
	{
		return false;
	}

	private List<Vector3> SplitPathIntoSegmentsAndSmoothe(List<PathFinderNode> path)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3 playSiteLocation = entity.PlaySiteLocation;
		MovementMap movementMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
		int num = path.Count - 1;
		int num2 = 0;
		int num3 = 0;
		num3 = num;
		list.Add(playSiteLocation);
		do
		{
			if (num2 != -1)
			{
				if (num2 > num3)
				{
					ConvertPathSegmentWithoutSmoothing(path, num3, num2, list);
				}
				if (num3 - num2 > 0)
				{
					List<Vector3> list2 = new List<Vector3>();
					ConvertPathSegmentWithoutSmoothing(path, num2, num3, list2);
					if (list.Count == 1)
					{
						list2.Insert(0, playSiteLocation);
						path.Insert(0, path[0]);
						num2++;
						num++;
						num3++;
					}
					if (endLocation.HasValue && num3 == num)
					{
						list2.Add(endLocation.Value);
						path.Add(path[path.Count - 1]);
					}
					if (list2.Count > 2)
					{
						SmoothSegmentNotOnRoads(path, list2, movementMap);
					}
					list.AddRange(list2);
				}
				continue;
			}
			ConvertPathSegmentWithoutSmoothing(path, num3, num, list);
			if (endLocation.HasValue)
			{
				list.Add(endLocation.Value);
			}
			break;
		}
		while (num3 < num);
		int num4 = 0;
		while (num4 < list.Count - 1)
		{
			if (list[num4] == list[num4 + 1])
			{
				list.RemoveAt(num4);
			}
			else
			{
				num4++;
			}
		}
		return list;
	}

	private void SmoothSegmentNotOnRoads(List<PathFinderNode> originalPath, List<Vector3> path, MovementMap moveMap)
	{
		int num = 1;
		int num2 = 0;
		int num3 = 1;
		Vector3 vector = path[path.Count - 1];
		Vector3 vector2 = path[num3 + 1];
		List<Vector3> list = null;
		if (moveMap.IDName.Contains("Human") && path.Count < 30 && Common.DistanceOctile(vector, path[0]) * 2f < (float)(path.Count * 16))
		{
			list = new List<Vector3>(path);
		}
		SubtileLayers subtileLayers = moveMap.Layers[entity.GetTransportType()];
		PathFinderNode pathFinderNode = originalPath[num2 + 1];
		byte newLowestCostAlongLine = MapManager.GetCost(subtileLayers.GetValue(pathFinderNode.AbsoluteX, pathFinderNode.AbsoluteY));
		do
		{
			PathFinderNode pathFinderNode2 = originalPath[num + 1];
			byte cost = MapManager.GetCost(subtileLayers.GetValue(pathFinderNode2.AbsoluteX, pathFinderNode2.AbsoluteY));
			byte highestCost = Math.Max(newLowestCostAlongLine, cost);
			Vector3 vector3 = path[num2];
			vector2 = path[num3 + 1];
			if (EdgeHasLowerOrEqualCost(vector3, vector2, highestCost, subtileLayers, ref newLowestCostAlongLine))
			{
				path.RemoveAt(num3);
			}
			else
			{
				num2 = num3;
				num3++;
			}
			num++;
		}
		while (vector2 != vector);
		if (list != null && list.Count < 30 && path.Count > 6)
		{
			_ = Common.DistanceOctile(vector, path[0]) * 2f;
			_ = (float)(path.Count * 16);
		}
	}

	private bool EdgeHasLowerOrEqualCost(Vector3 from, Vector3 to, byte highestCost, SubtileLayers map, ref byte newLowestCostAlongLine)
	{
		entity.GetTransportType();
		List<Point> subtilesTouchedByLine = MapManager.GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
		Point point = subtilesTouchedByLine[0];
		for (int i = 1; i < subtilesTouchedByLine.Count; i++)
		{
			Point point2 = subtilesTouchedByLine[i];
			if (point2 != point)
			{
				if (!The.Map.SubtileIsOnMap(point2))
				{
					continue;
				}
				byte cost = MapManager.GetCost(map.GetValue(point2));
				if (cost == 0)
				{
					return false;
				}
				byte b = cost;
				if (b > highestCost)
				{
					return false;
				}
				newLowestCostAlongLine = b;
			}
			point = point2;
		}
		return true;
	}

	private void ConvertPathSegmentWithoutSmoothing(List<PathFinderNode> path, int startAt, int endAt, List<Vector3> appendToList)
	{
		for (int i = startAt; i <= endAt; i++)
		{
			appendToList.Add(MapManager.SubTileToWorldPos(path[i]));
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
		Path = sn.DoList(Path);
		WaypointPath = sn.DoList(WaypointPath);
		snapshotParent = sn.SnapshotID<Goal, GoalID>(parentGoal);
		rejectedPassengerDestinations = sn.DoDictionary(rejectedPassengerDestinations);
		MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);
		endLocation = sn.DoVector3Nullable(endLocation);
		PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);
		sn.Ignore(100);
		sn.Ignore(GroupMoveActivity);
		sn.Ignore(parentGoal);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (Path != null)
		{
			foreach (PathFinderNode item in Path)
			{
				item.LoadPostProcess(sn);
			}
		}
		if (WaypointPath != null)
		{
			foreach (Waypoint item2 in WaypointPath)
			{
				item2.LoadPostProcess(sn);
			}
		}
		if (snapshotParent.HasValue)
		{
			parentGoal = (GoalMoveToPosition)LookUpGoals.FindByID(snapshotParent.Value);
		}
	}
}
