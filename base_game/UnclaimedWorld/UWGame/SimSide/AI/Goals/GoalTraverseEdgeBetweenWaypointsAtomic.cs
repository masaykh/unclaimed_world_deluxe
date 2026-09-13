using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.AllGameData.Constants;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalTraverseEdgeBetweenWaypointsAtomic : Goal
{
	public Vector3 To;

	private Point toTile;

	public Vector3 WaypointRay;

	private Vector2? lookAheadDirection;

	private Vector2? normalizedVectorToWaypoint;

	public GroupMoveActivity GroupMoveActivity;

	private float speedModifier = 1f;

	private const double maxPeriodInSeconds = 0.2;

	private double goalProgress;

	public bool MovingOutOfHarmsWay;

	private static Queue<GoalTraverseEdgeBetweenWaypointsAtomic> freeGoals = new Queue<GoalTraverseEdgeBetweenWaypointsAtomic>();

	private float permittedDistanceSquared;

	public double TimeSpentSliding;

	public Common.Direction detectedDirection;

	public bool movesAlong8Dir;

	public GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress;

	private bool isLastNode;

	private bool hasEverBeenOnUnblockedTile;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private Vector2? NormalizedVectorToWaypoint
	{
		get
		{
			if (!normalizedVectorToWaypoint.HasValue)
			{
				Vector2 value = new Vector2(To.X - entity.PlaySiteLocation.X, To.Y - entity.PlaySiteLocation.Y);
				value.Normalize();
				normalizedVectorToWaypoint = value;
			}
			return normalizedVectorToWaypoint.Value;
		}
		set
		{
			normalizedVectorToWaypoint = value;
		}
	}

	private void Init(Entity owner, Vector3 to, Vector2? lookAhead, Point toTile, Vector3 waypointRay, double timeSpentSliding, bool isLastEdge, bool movingOutOfHarmsWay, Common.Direction detectedDirection, bool movesAlong8Dir, GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress, GroupMoveActivity groupMoveActivity, float permittedDistanceSquared)
	{
		To = to;
		lookAheadDirection = lookAhead;
		this.toTile = toTile;
		WaypointRay = waypointRay;
		TimeSpentSliding = timeSpentSliding;
		this.permittedDistanceSquared = permittedDistanceSquared;
		isLastNode = isLastEdge;
		MovingOutOfHarmsWay = movingOutOfHarmsWay;
		goalProgress = 0.0;
		this.detectedDirection = detectedDirection;
		this.movesAlong8Dir = movesAlong8Dir;
		this.tileProgress = tileProgress;
		GroupMoveActivity = groupMoveActivity;
		Init(owner);
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public static void ClearPool()
	{
		freeGoals.Clear();
	}

	public static GoalTraverseEdgeBetweenWaypointsAtomic GetGoal(Entity owner, Vector3 to, Vector2? lookAhead, Point toTile, Vector3 waypointRay, double timeSpentSliding, bool isLastEdge, bool movingOutOfHarmsWay, Common.Direction detectedDirection, bool movesAlong8Dir, GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress, GroupMoveActivity groupMoveActivity, float permittedDistanceSquared)
	{
		if (freeGoals.Count == 0)
		{
			for (int i = 0; i < 30; i++)
			{
				freeGoals.Enqueue(new GoalTraverseEdgeBetweenWaypointsAtomic());
			}
		}
		GoalTraverseEdgeBetweenWaypointsAtomic goalTraverseEdgeBetweenWaypointsAtomic = freeGoals.Dequeue();
		goalTraverseEdgeBetweenWaypointsAtomic.Init(owner, to, lookAhead, toTile, waypointRay, timeSpentSliding, isLastEdge, movingOutOfHarmsWay, detectedDirection, movesAlong8Dir, tileProgress, groupMoveActivity, permittedDistanceSquared);
		return goalTraverseEdgeBetweenWaypointsAtomic;
	}

	public override void RetireGoal()
	{
		freeGoals.Enqueue(this);
	}

	public override float GetExertionLevel()
	{
		PhysicalWork physicalWork = GameData.Instance.Constants.PhysicalWork;
		if (!entity.DrivingVehicle.HasValue)
		{
			if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
			{
				return physicalWork.Running;
			}
			if (entity.AgentStorage != null)
			{
				float num = entity.AgentStorage.ItemStorage.TotalStored / entity.AgentStorage.ItemStorage.TotalCapacity;
				if (num > 0.9f)
				{
					return physicalWork.HaulingHeavyLoad;
				}
				if (num > 0.3f)
				{
					return physicalWork.HaulingMediumLoad;
				}
				if (num > 0.1f)
				{
					return physicalWork.HaulingLightLoad;
				}
				return physicalWork.Walking;
			}
			return physicalWork.Walking;
		}
		return physicalWork.Driving;
	}

	public override StealthFactor GetStealthFactor()
	{
		if (!entity.DrivingVehicle.HasValue)
		{
			if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
			{
				return StealthFactor.Bad;
			}
			return StealthFactor.NotGood;
		}
		return StealthFactor.ExtremelyBad;
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectGood;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	protected override void Activate()
	{
		if (GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, permittedDistanceSquared))
		{
			base.Status = Status.Completed;
			return;
		}
		hasEverBeenOnUnblockedTile = !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[entity.GetTransportType()], MapManager.WorldPosToSubtile(entity.PlaySiteLocation));
		base.Status = Status.Active;
		GoalTraverseEdgeBetweenWaypoints.SelectMoveSpeed(entity);
		if (GroupMoveActivity != null)
		{
			if (GroupMoveActivity.IsFollower(entity))
			{
				speedModifier = GroupMoveActivity.ComputeSpeedModifier(entity);
			}
			else if (GroupMoveActivity.IsLeader(entity))
			{
				GroupMoveActivity.ComputeLeadersRotationMatrix();
			}
		}
	}

	private void TestProgress(Vector3 location, ref Vector3 moveVector, Entity vehicleForLeavingTrails)
	{
		NormalizedVectorToWaypoint = null;
		bool changeTilePosition = false;
		Vector3 vector = location + moveVector;
		if (!The.Map.WorldLocationIsOnMap(vector.ToVector2()))
		{
			vector = The.Map.ClampWorldPosition(vector);
			moveVector = vector - location;
		}
		TestForMovingPastSubtileBoundary(vector, ref moveVector, vehicleForLeavingTrails);
		if (base.Status == Status.Failed)
		{
			return;
		}
		switch (tileProgress)
		{
		case GoalTraverseEdgeBetweenWaypoints.TileProgress.NoDescription:
			if (!TestForMovingPastTileBoundary(vector, out changeTilePosition, vehicleForLeavingTrails))
			{
				TestForEnteringCenterOfTile(vector, vehicleForLeavingTrails);
			}
			break;
		case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge:
			if (!TestForEnteringCenterOfTile(vector, vehicleForLeavingTrails))
			{
				TestForMovingPastTileBoundary(vector, out changeTilePosition, vehicleForLeavingTrails);
			}
			break;
		case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromCenter:
			TestForMovingPastTileBoundary(vector, out changeTilePosition, vehicleForLeavingTrails);
			break;
		}
	}

	private bool TestForMovingPastTileBoundary(Vector3 newLocation, out bool changeTilePosition, Entity vehicle)
	{
		changeTilePosition = false;
		Point value = MapManager.WorldPosToTile(newLocation);
		Point? mapPosition = entity.MapPosition;
		if (value != mapPosition)
		{
			Common.Direction direction = detectedDirection;
			bool num = movesAlong8Dir;
			if (The.Map.DetectMovementAlong8Dir(newLocation, NormalizedVectorToWaypoint.Value, ref detectedDirection))
			{
				movesAlong8Dir = true;
			}
			else
			{
				movesAlong8Dir = false;
			}
			if (num && (!movesAlong8Dir || direction != detectedDirection))
			{
				GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, direction, entity.MapPosition.Value, vehicle);
			}
			changeTilePosition = true;
			return true;
		}
		return false;
	}

	private bool TestForMovingPastSubtileBoundary(Vector3 newLocation, ref Vector3 moveVector, Entity vehicle)
	{
		Point point = MapManager.WorldPosToSubtile(newLocation);
		Point point2 = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
		if (point != point2)
		{
			tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge;
			_ = detectedDirection;
			_ = movesAlong8Dir;
			if (!The.Map.SubtileIsOnMap(point))
			{
				base.Status = Status.Failed;
				entityIntelligence.GroupMoveAssignedWaypoint = null;
				TimeSpentSliding = 0.0;
				return true;
			}
			if (MapManager.IsBlocked(The.Map.TerrainCosts[entity.GetTransportType()].GetValue(point)) && hasEverBeenOnUnblockedTile)
			{
				float num = vehicle?.Locomotor.MoveSpeed ?? entity.Locomotor.MoveSpeed;
				if (The.Map.TileIsCompletelyBlocked(The.Map.TerrainCosts[entity.GetTransportType()], toTile))
				{
					base.Status = Status.Failed;
					entityIntelligence.GroupMoveAssignedWaypoint = null;
					TimeSpentSliding = 0.0;
					return true;
				}
				if (TimeSpentSliding > GameData.Instance.AIConstants.MaxTimeForSliding / (double)(num * 0.1f))
				{
					base.Status = Status.Failed;
					TimeSpentSliding = 0.0;
					return true;
				}
				switch (Common.GetDirection(point2, point))
				{
				case Common.Direction.North:
				case Common.Direction.South:
					if (moveVector.X == 0f)
					{
						base.Status = Status.Failed;
						return true;
					}
					moveVector = SlideEastOrWest(moveVector);
					return true;
				case Common.Direction.East:
				case Common.Direction.West:
					if (moveVector.Y == 0f)
					{
						base.Status = Status.Failed;
						return true;
					}
					moveVector = SlideNorthOrSouth(moveVector);
					return true;
				default:
					base.Status = Status.Failed;
					return true;
				}
			}
			if (vehicle != null)
			{
				vehicle.Locomotor.SetTerrainModifier(point);
			}
			else
			{
				entity.Locomotor.SetTerrainModifier(point);
			}
			TimeSpentSliding = 0.0;
			hasEverBeenOnUnblockedTile = true;
			return true;
		}
		TimeSpentSliding = 0.0;
		return false;
	}

	private Vector3 SlideNorthOrSouth(Vector3 moveVector)
	{
		moveVector.Y += (float)Math.Sign(moveVector.Y) * 0.5f * Math.Abs(moveVector.X);
		moveVector.X = 0f;
		TimeSpentSliding += The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
		return moveVector;
	}

	private Vector3 SlideEastOrWest(Vector3 moveVector)
	{
		moveVector.X += (float)Math.Sign(moveVector.X) * 0.5f * Math.Abs(moveVector.Y);
		moveVector.Y = 0f;
		TimeSpentSliding += The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
		return moveVector;
	}

	public bool TestForEnteringCenterOfTile(Vector3 newLocation, Entity vehicle)
	{
		if (MapManager.DetectIsAtCenterOfTile(newLocation))
		{
			tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromCenter;
			Common.Direction direction = detectedDirection;
			bool num = movesAlong8Dir;
			if (The.Map.DetectMovementAlong8Dir(newLocation, NormalizedVectorToWaypoint.Value, ref detectedDirection))
			{
				movesAlong8Dir = true;
			}
			else
			{
				movesAlong8Dir = false;
			}
			if (num && (!movesAlong8Dir || direction != detectedDirection))
			{
				GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, direction, entity.MapPosition.Value, vehicle);
			}
			return true;
		}
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		entity.ToString().Contains("onlan");
		if (GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, permittedDistanceSquared))
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
		SteerableFrontWheels c = null;
		float headingDifference;
		if (vehicle != null)
		{
			vehicle.Find<SteerableFrontWheels>(out c);
			Move(vehicle.EntityType.LocomotorType.MaxAngularSpeed, vehicle.Rotation, 0, vehicle.PlaySiteLocation, elapsed.ElapsedGameTime.TotalSeconds, ((VehicleContainerType)vehicle.EntityType.ContainerType).MaxAcceleration, vehicle.Locomotor.MoveSpeed, vehicle.Locomotor.CurrentMaximumSpeed, out var moveVector, out var newRotation, out var newDirection, out var newSpeed, out headingDifference);
			TestProgress(vehicle.PlaySiteLocation, ref moveVector, vehicle);
			if (base.Status == Status.Active)
			{
				moveVector.Z = 0f;
				entity.Location = vehicle.Location + moveVector;
				newDirection.Z = 0f;
				vehicle.Rotation = newRotation;
				vehicle.FacingNormal = newDirection;
				vehicle.Locomotor.MoveSpeed = newSpeed;
				if (vehicle.Locomotor.MoveSpeed > 0f)
				{
					float intensity = MathHelper.Clamp(vehicle.Locomotor.MoveSpeed / GameData.Instance.Constants.VehicleSpeedCausingMaximumDust, 0f, 1f) * The.Map.GetTile(vehicle.MapPosition.Value).GetDustFactor();
					The.Client.ParticleManager.AddDust(vehicle, vehicle.PlaySiteLocation - 30f * entity.FacingNormal, intensity, GameData.Instance.Constants.VehicleDustScale);
				}
			}
		}
		else
		{
			int cornerNo;
			float rotation = GoalTurnToFace.FindClosestCorner(entity, entity.PlaySiteLocation, To.ToVector2(), entity.Rotation, out cornerNo);
			if (cornerNo != 0)
			{
				entity.Locomotor.FlipFourSidedSymmetryCreature(rotation);
			}
			entity.Find<SteerableFrontWheels>(out c);
			Move(entity.EntityType.LocomotorType.MaxAngularSpeed, entity.Rotation, cornerNo, entity.PlaySiteLocation, elapsed.ElapsedGameTime.TotalSeconds, entity.Locomotor.GetAcceleration(), entity.Locomotor.MoveSpeed, entity.Locomotor.CurrentMaximumSpeed, out var moveVector2, out var newRotation2, out var newDirection2, out var newSpeed2, out headingDifference);
			TestProgress(entity.PlaySiteLocation, ref moveVector2, vehicle);
			if (base.Status == Status.Active)
			{
				moveVector2.Z = 0f;
				newDirection2.Z = 0f;
				if (newSpeed2 != entity.Locomotor.MoveSpeed)
				{
					entity.Locomotor.MoveSpeed = newSpeed2;
				}
				Entity obj = entity;
				obj.Location += moveVector2;
				entity.Rotation = newRotation2;
				entity.FacingNormal = newDirection2;
				if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run && entity.Find<BiologicalEntity>(out var c2))
				{
					Common.DecreaseValueBetweenZeroAndOneBySecondsAmount(c2.OxygenAndMuscleEnergy, GameData.Instance.Constants.DecreaseInOxygenMuscleEnergyWhenRunningPerSecond, elapsed.ElapsedGameTime.TotalSeconds);
				}
			}
		}
		c?.SetAngle(headingDifference, (float)elapsed.ElapsedGameTime.TotalSeconds);
		goalProgress += elapsed.ElapsedGameTime.TotalSeconds;
		if (goalProgress > 0.2 && base.Status != Status.Failed)
		{
			base.Status = Status.Completed;
		}
	}

	private void Move(float maxAngularVelocity, float rotation, int cornerNo, Vector3 location, double elapsedTotalSeconds, float? maxAcceleration, float? currentSpeed, float targetSpeed, out Vector3 moveVector, out float newRotation, out Vector3 newDirection, out float newSpeed, out float headingDifference)
	{
		float num = (newRotation = GoalFlyToPosition.TurnToFace(location, To.ToVector2(), rotation, (float)((double)maxAngularVelocity * elapsedTotalSeconds), out headingDifference));
		newDirection = new Vector3((float)Math.Cos(num), (float)Math.Sin(num), 0f);
		float num2 = Math.Abs(headingDifference);
		float num3 = (float)Math.PI / 2f;
		float num4;
		if (num2 > num3)
		{
			num4 = 0f;
		}
		else
		{
			float num5 = 1f;
			if (GroupMoveActivity != null)
			{
				num5 = (GroupMoveActivity.IsLeader(entity) ? 1f : speedModifier);
			}
			num4 = ((!Common.IsZero(num2)) ? MathHelper.Lerp(0f, num5 * targetSpeed, 1f - num2 / num3) : (num5 * targetSpeed));
			if (lookAheadDirection.HasValue)
			{
				Vector2 value = To.ToVector2();
				value.X -= location.X;
				value.Y -= location.Y;
				float num6 = value.Length();
				if (num6 > 0f)
				{
					value /= num6;
					float num7 = Common.ClampTop(num6, 30f);
					num7 /= 30f;
					float num8 = Vector2.Dot(value, lookAheadDirection.Value);
					num8 = 0.5f + num8 / 2f;
					num8 = 1f - num8;
					float num9 = 200f;
					num9 = MathHelper.Lerp(200f, 80f, num8 * num7);
					num4 = Common.ClampTop(num4, num9);
				}
			}
			if (maxAcceleration.HasValue)
			{
				num4 = MathHelper.Clamp(num4, (float)((double)currentSpeed.Value - (double)maxAcceleration.Value * elapsedTotalSeconds), (float)((double)currentSpeed.Value + (double)maxAcceleration.Value * elapsedTotalSeconds));
			}
		}
		moveVector = (float)(elapsedTotalSeconds * (double)num4) * newDirection;
		newSpeed = num4;
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
		To = sn.DoVector3(To);
		toTile = sn.DoPoint(toTile);
		WaypointRay = sn.DoVector3(WaypointRay);
		lookAheadDirection = sn.DoVector2Nullable(lookAheadDirection);
		normalizedVectorToWaypoint = sn.DoVector2Nullable(normalizedVectorToWaypoint);
		speedModifier = sn.DoFloat(speedModifier);
		goalProgress = sn.DoDouble(goalProgress);
		MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);
		permittedDistanceSquared = sn.DoFloat(permittedDistanceSquared);
		TimeSpentSliding = sn.DoDouble(TimeSpentSliding);
		detectedDirection = sn.DoEnum(detectedDirection);
		movesAlong8Dir = sn.DoBool(movesAlong8Dir);
		tileProgress = sn.DoEnum(tileProgress);
		isLastNode = sn.DoBool(isLastNode);
		hasEverBeenOnUnblockedTile = sn.DoBool(hasEverBeenOnUnblockedTile);
		sn.Ignore(freeGoals);
		sn.Ignore(GroupMoveActivity);
		return this;
	}
}
