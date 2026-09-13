using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalTraverseEdgeBetweenWaypoints : CompositeGoal
{
	public enum TileProgress
	{
		NoDescription,
		MovingFromEdge,
		MovingFromCenter
	}

	public Vector3 To;

	public Vector3 WaypointRay;

	public int Number;

	public Point toTile;

	public Vector2? LookAheadDirection;

	private double timeSpentSliding;

	private Common.Direction detectedDirection;

	private bool movesAlong8Dir;

	private TileProgress tileProgress;

	private bool isLastNode;

	public bool StopMovingWhenTerminating = true;

	public bool MovingOutOfHarmsWay;

	public GroupMoveActivity GroupMoveActivity;

	private List<Entity> GroupMoveStragglers;

	public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalTraverseEdgeBetweenWaypoints(Entity owner, Vector3 to, Vector2? lookAheadDirection, Vector3 waypointRay, int number, bool isLastEdge, bool movingOutOfHarmsWay, List<EntityGroupID> ownersOfVehicles, GroupMoveActivity groupMoveActivity)
		: base(owner)
	{
		To = to;
		WaypointRay = waypointRay;
		Number = number;
		LookAheadDirection = lookAheadDirection;
		toTile = MapManager.WorldPosToTile(To);
		isLastNode = isLastEdge;
		base.ownersOfVehicles = ownersOfVehicles;
		GroupMoveActivity = groupMoveActivity;
		MovingOutOfHarmsWay = movingOutOfHarmsWay;
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
		detectedDirection = sn.DoEnum(detectedDirection);
		isLastNode = sn.DoBool(isLastNode);
		LookAheadDirection = sn.DoVector2Nullable(LookAheadDirection);
		movesAlong8Dir = sn.DoBool(movesAlong8Dir);
		MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);
		Number = sn.DoInt32(Number);
		PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);
		StopMovingWhenTerminating = sn.DoBool(StopMovingWhenTerminating);
		tileProgress = sn.DoEnum(tileProgress);
		timeSpentSliding = sn.DoDouble(timeSpentSliding);
		To = sn.DoVector3(To);
		toTile = sn.DoPoint(toTile);
		WaypointRay = sn.DoVector3(WaypointRay);
		sn.Postpone(GroupMoveStragglers);
		sn.Postpone(GroupMoveActivity);
		return this;
	}

	public GoalTraverseEdgeBetweenWaypoints()
	{
	}

	protected override void Activate()
	{
		if (entity.HasStance())
		{
			entity.Locomotor.Stance.CurrentStance = entity.EntityType.LocomotorType.StancesType.MovingStanceType;
		}
		if (IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, PermittedDistanceSquaredToDestination))
		{
			base.Status = Status.Completed;
			return;
		}
		Entity vehicle = null;
		if (!entity.GetDrivenVehicle(out vehicle))
		{
			base.Status = Status.Failed;
			return;
		}
		Common.Direction direction = Common.Direction.North;
		Vector2 vectorToWaypoint = default(Vector2);
		vectorToWaypoint.X = To.X - entity.PlaySiteLocation.X;
		vectorToWaypoint.Y = To.Y - entity.PlaySiteLocation.Y;
		vectorToWaypoint.Normalize();
		if (The.Map.DetectMovementAlong8Dir(entity.PlaySiteLocation, vectorToWaypoint, ref direction))
		{
			movesAlong8Dir = true;
		}
		else
		{
			movesAlong8Dir = false;
		}
		MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
		GoalTraverseEdgeBetweenWaypointsAtomic goal = GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay, direction, movesAlong8Dir, tileProgress, GroupMoveActivity, PermittedDistanceSquaredToDestination);
		AddSubgoal(goal);
		SelectMoveSpeed(entity);
		if (GroupMoveActivity != null && GroupMoveActivity.IsFollower(entity))
		{
			GroupMoveTestStatus();
		}
		base.Status = Status.Active;
	}

	public static void SelectMoveSpeed(Entity entity)
	{
		if (entity.EntityType.IntelligenceType.IsMobile && entity.Locomotor.LeggedLocomotor != null)
		{
			if (entity.IsHaulingNothingMounted())
			{
				entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Haul;
			}
			else if (!entity.DrivingVehicle.HasValue && entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
			{
				CompositeGoal.SetFleeingSpeedType(entity);
			}
		}
	}

	public static Vector3 ComputeWaypointRay(Vector3 waypoint, Entity entity)
	{
		return Vector3.Cross(waypoint - entity.PlaySiteLocation, Vector3.UnitZ);
	}

	private void AddStraggler(Entity member)
	{
		if (GroupMoveStragglers == null)
		{
			GroupMoveStragglers = new List<Entity>();
		}
		if (!GroupMoveStragglers.Contains(member))
		{
			GroupMoveStragglers.Add(member);
		}
	}

	private bool AreAllStragglersReady()
	{
		if (GroupMoveStragglers == null)
		{
			return true;
		}
		int num;
		for (num = GroupMoveStragglers.Count - 1; num >= 0; num--)
		{
			Entity entity = GroupMoveStragglers[num];
			if (!IsAtWaypoint(entity.PlaySiteLocation, entity.Intelligence.GroupMoveAssignedWaypointRay.Value, entity.Intelligence.GroupMoveAssignedWaypoint.Location, PermittedDistanceSquaredToDestination))
			{
				return false;
			}
			GroupMoveStragglers.RemoveAt(num);
			num--;
		}
		return true;
	}

	private void GroupMoveTestStatus()
	{
		if (!GroupMoveActivity.IsLeader(entity))
		{
			float num = Common.DistanceOctile(entity.PlaySiteLocation, To);
			if ((double)num > GameData.Instance.AIConstants.DistancePromptingHaltRequest && Common.DistanceOctile(GroupMoveActivity.Leader.PlaySiteLocation, To) < num)
			{
				GroupMoveActivity.Leader.SendMessage(new Message(entity, Message.MessageTypes.WaitForMeGroup, null));
				entityIntelligence.FollowerStatus = FollowerStatus.IsCatchingUp;
			}
		}
	}

	public static void LeaveTrails(Entity entity, Common.Direction dir, Point p, Entity vehicle)
	{
	}

	public static bool IsAtWaypoint(Vector3 worldLocation, Vector3 waypoint)
	{
		if (MapManager.WorldPosToSubtile(worldLocation) == MapManager.WorldPosToSubtile(waypoint))
		{
			return Vector2.DistanceSquared(worldLocation.ToVector2(), waypoint.ToVector2()) < GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;
		}
		return false;
	}

	public static bool IsAtWaypoint(Vector3 location, Vector3 waypointRay, Vector3 to, float permittedDistanceSquared)
	{
		if (Vector2.DistanceSquared(location.ToVector2(), to.ToVector2()) < permittedDistanceSquared)
		{
			return true;
		}
		if (waypointRay != Vector3.Zero)
		{
			return Vector3.Cross(new Vector3(location.X - to.X, location.Y - to.Y, 0f), waypointRay).Z < 0f;
		}
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		switch (ProcessSubgoals(elapsed))
		{
		case Status.Completed:
		{
			if (entityIntelligence.Brain.ArbitrateWhileBusy(out var _))
			{
				HandleSubstitutedGoalByArbitrator();
			}
			else if (IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, PermittedDistanceSquaredToDestination))
			{
				base.Status = Status.Completed;
				if (GroupMoveActivity != null)
				{
					if (entityIntelligence.FollowerStatus == FollowerStatus.IsCatchingUp || (entityIntelligence.FollowerStatus == FollowerStatus.IsFollowingPathToWaypoint && isLastNode))
					{
						GroupMoveActivity.Leader.SendMessage(new Message(Message.MessageTypes.StartGroupMovement));
						entityIntelligence.FollowerStatus = FollowerStatus.Normal;
					}
					else if (!AreAllStragglersReady())
					{
						base.Status = Status.Active;
						AddSubgoal(new GoalWait(entity));
					}
					if (isLastNode && (GroupMoveActivity.IsLeader(entity) || (GroupMoveActivity.IsFollower(entity) && entityIntelligence.FollowerStatus != FollowerStatus.IsFollowingPathToWaypoint && entityIntelligence.GroupMoveAssignedWaypoint != null && entityIntelligence.GroupMoveAssignedWaypoint.Number == Number)))
					{
						entityIntelligence.IsAtGroupMoveDestination = true;
					}
				}
			}
			else
			{
				if (Subgoals.Count > 0 && Subgoals.Peek() is GoalTraverseEdgeBetweenWaypointsAtomic goalTraverseEdgeBetweenWaypointsAtomic)
				{
					movesAlong8Dir = goalTraverseEdgeBetweenWaypointsAtomic.movesAlong8Dir;
					detectedDirection = goalTraverseEdgeBetweenWaypointsAtomic.detectedDirection;
					tileProgress = goalTraverseEdgeBetweenWaypointsAtomic.tileProgress;
					timeSpentSliding = goalTraverseEdgeBetweenWaypointsAtomic.TimeSpentSliding;
				}
				_ = base.ID;
				_ = uint.MaxValue;
				ValidateSafetyAndTakeAction();
			}
			break;
		}
		case Status.Failed:
			base.Status = Status.Failed;
			break;
		}
	}

	private void ValidateSafetyAndTakeAction()
	{
		if (!MovingOutOfHarmsWay)
		{
			DiscomfortMap discomfortMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
			if (discomfortMap.Map.GetIsBlocked(entity.MapPosition.Value))
			{
				if ((float)(int)discomfortMap.Map.GetValue(entity.MapPosition.Value) > (float)entityIntelligence.PanicLevel)
				{
					PerformPanicFleeing(discomfortMap);
				}
				else
				{
					base.Status = Status.Failed;
				}
				return;
			}
			Point value = toTile;
			Point? mapPosition = entity.MapPosition;
			if (value != mapPosition)
			{
				Point pos = MapManager.WorldPosToTile(entity.PlaySiteLocation + entity.FacingNormal * 24f);
				pos = The.Map.ClampTileMapPosition(pos);
				value = pos;
				mapPosition = entity.MapPosition;
				if (value != mapPosition && discomfortMap.Map.GetIsBlocked(pos))
				{
					base.Status = Status.Failed;
					return;
				}
			}
			SkipThisAssert = true;
			AddSubgoal(GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay, detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, PermittedDistanceSquaredToDestination));
			SkipThisAssert = false;
			base.Status = Status.Active;
		}
		else
		{
			AddSubgoal(GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay, detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, GameData.Instance.Constants.DistanceSquaredLimitForWaypoints));
			base.Status = Status.Active;
		}
	}

	public override void Deactivate()
	{
		if (isLastNode && StopMovingWhenTerminating)
		{
			Entity vehicle = null;
			if (!entity.GetDrivenVehicle(out vehicle))
			{
				base.Status = Status.Failed;
			}
			else
			{
				StopMoving(entity, vehicle);
			}
		}
	}

	public static void StopMoving(Entity entity, Entity vehicle)
	{
		if (vehicle != null)
		{
			vehicle.Locomotor.MoveSpeed = 0f;
		}
		else
		{
			entity.Locomotor.MoveSpeed = 0f;
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.StartGroupMovement:
				if (Subgoals.Count > 0 && !(Subgoals.Peek() is GoalWait))
				{
					return false;
				}
				return true;
			case Message.MessageTypes.WaitForMeGroup:
				AddStraggler(message.Sender);
				return true;
			case Message.MessageTypes.CancelJobOrItemInUse:
			case Message.MessageTypes.CancelJobForAIReset:
			{
				Entity vehicle = null;
				if (!entity.GetDrivenVehicle(out vehicle))
				{
					base.Status = Status.Failed;
					return true;
				}
				StopMoving(entity, vehicle);
				if (message.OtherInfo is Message.CancelJobKeepVehicle && (Message.CancelJobKeepVehicle)message.OtherInfo == Message.CancelJobKeepVehicle.LeaveVehicle && vehicle != null)
				{
					entityIntelligence.Brain.AddSubgoal(new GoalExitVehicle(entity, vehicle.EntityID));
				}
				return false;
			}
			case Message.MessageTypes.WaitAndMakeRoom:
			{
				entity.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent = true;
				double value = Common.ClampBottom(The.Sim.GameplayRandomGenerator.RandomNormalDistribution(2.0, 0.5), 0.5);
				AddSubgoal(new GoalWait(entity, value));
				return true;
			}
			default:
				return false;
			}
		}
		return true;
	}
}
