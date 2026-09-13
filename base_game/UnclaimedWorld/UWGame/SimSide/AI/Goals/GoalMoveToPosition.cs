using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

public class GoalMoveToPosition : CompositeGoal
{
	public enum VehicleUse
	{
		NoVehicle,
		FreeUpAfterUse,
		KeepVehicle
	}

	public enum Nesting
	{
		IsNested,
		NotNested
	}

	public WorldLocation? Destination;

	public TilePos? DestinationTilePos;

	public EntityID? DestinationEntity;

	public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;

	private AttackType attackType;

	private bool useMeleeLocationAsDestination;

	public bool IsChasingTargetCenterLocation = true;

	public bool IsFinalDestination;

	private Nesting nesting = Nesting.NotNested;

	public EntityID? UsedVehicle;

	public VehicleUse VehicleUseByGoal;

	public GroupMoveActivity GroupMoveActivity;

	private int noOfFailures;

	private Vector3 pathStartLocation;

	private bool isEvaluatingPersonalTransport;

	private Regulator repathRegulator;

	private Vector3 lastTargetLocation;

	private TimeSpan timePointForLastRepath;

	public IDActionEvent<float> RepathDoneEvent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalMoveToPosition(Entity entity, Vector3 destination, Nesting nesting, List<EntityGroupID> ownersOfVehicles, EntityID? targetEntity = null, bool useMeleeLocationAsDestination = false, AttackType attackType = null)
		: base(entity)
	{
		ResetDestination(destination);
		this.nesting = nesting;
		VehicleUseByGoal = VehicleUse.FreeUpAfterUse;
		base.ownersOfVehicles = ownersOfVehicles;
		DestinationEntity = targetEntity;
		this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;
		this.attackType = attackType;
		Init();
	}

	public GoalMoveToPosition(Entity entity, Vector3 destination, List<EntityGroupID> ownersOfVehicles, VehicleUse vehicleUse = VehicleUse.FreeUpAfterUse, EntityID? targetEntity = null, bool useMeleeLocationAsDestination = false, AttackType attackType = null)
		: base(entity)
	{
		VehicleUseByGoal = vehicleUse;
		base.ownersOfVehicles = ownersOfVehicles;
		DestinationEntity = targetEntity;
		this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;
		this.attackType = attackType;
		ResetDestination(destination);
		Init();
	}

	public GoalMoveToPosition(Entity entity, List<EntityGroupID> ownersOfVehicles, IKnownEntityData targetEntityData, VehicleUse vehicleUse = VehicleUse.FreeUpAfterUse, bool useMeleeLocationAsDestination = false, AttackType attackType = null)
		: base(entity)
	{
		VehicleUseByGoal = vehicleUse;
		base.ownersOfVehicles = ownersOfVehicles;
		this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;
		if (targetEntityData.ContainedBy.HasValue)
		{
			DestinationEntity = targetEntityData.ContainedBy.Value;
		}
		else
		{
			Vector3 value = targetEntityData.AccessPoint.Value;
			ResetDestination(value);
		}
		this.attackType = attackType;
		Init();
	}

	private void Init()
	{
		_ = entity.ID;
		_ = 12615;
		timePointForLastRepath = The.Sim.TotalUnPausedGameTime;
	}

	public GoalMoveToPosition()
	{
	}

	public override string ToString()
	{
		return $"{base.ToString()} {DestinationTilePos}";
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		ActivateAndGetPath();
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		repathRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 5.0, "GoalMoveToPositionPath");
	}

	private void ActivateAndGetPath()
	{
		base.Status = Status.Active;
		pathStartLocation = entity.AccessPoint.Value;
		if (Destination.HasValue && MapManager.WorldPosToSubtile(entity.PlaySiteLocation) == MapManager.WorldPosToSubtile(Destination.Value))
		{
			base.Status = Status.Completed;
			return;
		}
		IKnownEntityData data = null;
		if (DestinationEntity.HasValue)
		{
			EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out data);
			if (EntityResultCausesFailedGoal(knownData))
			{
				return;
			}
			if (data == entity)
			{
				base.Status = Status.Completed;
				return;
			}
		}
		UpdateMoveTargetForLerping();
		if (!entity.GetContainedBy(out Entity container))
		{
			base.Status = Status.Failed;
			return;
		}
		if (container != null)
		{
			pathStartLocation = container.AccessPoint.Value;
		}
		if (GroupMoveActivity != null)
		{
			if (GroupMoveActivity.IsLeader(entity))
			{
				entityIntelligence.IsAtGroupMoveDestination = false;
				GetPath(pathStartLocation, data);
			}
			else if (GroupMoveActivity.IsFollower(entity))
			{
				ActivateGroupFollower(pathStartLocation);
			}
		}
		else if (!entity.DrivingVehicle.HasValue && VehicleUseByGoal != VehicleUse.NoVehicle)
		{
			if (UsedVehicle.HasValue)
			{
				if (!EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(UsedVehicle.Value, out var data2)))
				{
					EnterVehicleAndGo(data2);
				}
			}
			else
			{
				DecideOnVehicleOrGoByFoot(data);
			}
		}
		else if (entity.DrivingVehicle.HasValue)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entity.DrivingVehicle.Value, out var data3)))
			{
				return;
			}
			if (nesting != Nesting.IsNested && VehicleUseByGoal != VehicleUse.NoVehicle)
			{
				if (((VehicleContainerType)data3.EntityType.ContainerType).Aircraft != null)
				{
					AddSubgoal(new GoalTakeoff(entity));
					AddSubgoal(new GoalFlyToPosition(entity, DestinationTilePos.Value.ToPoint()));
					AddSubgoal(new GoalLand(entity));
				}
				else
				{
					Trigger trigger = new Trigger(entity, null, GameData.Instance.AllTriggerTypes["drivenVehicle"]);
					entity.AttachTrigger(trigger);
					GoByVehicle(data3);
				}
			}
			else if (VehicleUseByGoal == VehicleUse.NoVehicle)
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
				GetPath(pathStartLocation, data);
			}
		}
		else
		{
			GetPath(pathStartLocation, data);
		}
	}

	private void UpdateMoveTargetForLerping()
	{
		IKnownEntityData data = null;
		if (DestinationEntity.HasValue)
		{
			entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out data);
		}
		if (data != null)
		{
			entity.Locomotor.CurrentMoveTarget = data.AccessPoint.Value;
		}
		else if (Destination.HasValue)
		{
			entity.Locomotor.CurrentMoveTarget = Destination.Value.ToVector3();
		}
		else
		{
			entity.Locomotor.CurrentMoveTarget = entity.PlaySiteLocation;
		}
	}

	private void ResetDestination(Vector3 destination)
	{
		Destination = new WorldLocation(destination);
		DestinationTilePos = MapManager.WorldPosToTilePos(destination);
	}

	private bool DecideOnVehicleOrGoByFoot(IKnownEntityData destinationData)
	{
		GetPath(pathStartLocation, destinationData);
		return true;
	}

	private void EnterVehicleAndGo(IKnownEntityData vehicleToUse)
	{
		entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(vehicleToUse.EntityID, entity.EntityID);
		UsedVehicle = vehicleToUse.EntityID;
		vehicleToUse.GetFreeDriversSlot().GetEntryPoints(out var transformedEntry, out var _);
		GoalMoveToPosition goal = new GoalMoveToPosition(entity, transformedEntry, ownersOfVehicles, VehicleUse.NoVehicle);
		AddSubgoal(goal);
		AddSubgoal(new GoalEnterVehicleAsDriver(entity, vehicleToUse.EntityID, new Trigger(entity, null, GameData.Instance.AllTriggerTypes["drivenVehicle"])));
		GoByVehicle(vehicleToUse);
	}

	private RegionMap.Result GetVehicleForPersonTransport(Entity entity, Point to, out IKnownEntityData bestVehicle, List<EntityGroupID> ownersOfVehicles, ProtectionLevel protectionLevel, ThreatStance approach)
	{
		bestVehicle = null;
		if (entity.PersonEntity != null && entity.PersonEntity.CanDrive())
		{
			RegionMap regionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap;
			_ = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.OffRoad].RegionMap;
			Point toSubtile = MapManager.TileCenterToSubTile(to);
			Vector3 p = MapManager.TileToWorldPos(to);
			Point fromSubtile = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
			float distance = -1f;
			RegionMap.Result distance2 = regionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance);
			if (distance2 != RegionMap.Result.OK)
			{
				return distance2;
			}
			if (distance < (float)GameData.Instance.AIConstants.ShortestDistanceToConsiderAVehicle)
			{
				return RegionMap.Result.OK;
			}
			double num = GoalEvaluator.ScoreTravelTime(distance / entity.Locomotor.CurrentMaximumSpeedNoTerrain);
			double num2 = ScoreComfort(null);
			double num3 = 0.6 * num + 0.1 * num2 + 0.3;
			float distance3 = 0f;
			float distance4 = 0f;
			Allegiance allegiance = entity.Intelligence.Allegiance;
			foreach (EntityGroupID ownersOfVehicle in ownersOfVehicles)
			{
				EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ownersOfVehicle);
				if (entityGroup == null)
				{
					base.Status = Status.Failed;
					return RegionMap.Result.NoAccess;
				}
				for (int num4 = entityGroup.Vehicles.Count - 1; num4 >= 0; num4--)
				{
					EntityID entityID = entityGroup.Vehicles[num4];
					if (GoalEvaluator.HandleOwnerDataResult(allegiance.SharedKnowledge, entityID, entityGroup, out var entityData) && !entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(entityData.EntityID).HasValue && Entity.IsFunctional(entityData) && !The.Map.IsNoParkingSpot(entityData.EntityType, protectionLevel, entity.EntityType, approach, to))
					{
						Vector3 playSiteLocation = entityData.PlaySiteLocation;
						Point point = MapManager.WorldPosToSubtile(playSiteLocation);
						RegionMap.Result distance5 = regionMap.GetDistance(entity, fromSubtile, point, ref distance3);
						switch (distance5)
						{
						case RegionMap.Result.Wait:
							return distance5;
						default:
						{
							float num5 = distance3 / entity.Locomotor.CurrentMaximumSpeedNoTerrain;
							VehicleContainerType vehicleContainerType = (VehicleContainerType)entityData.EntityType.ContainerType;
							float num6;
							if (vehicleContainerType.Transport == SurfaceType.TransportType.Air)
							{
								num6 = Common.DistanceOctile(playSiteLocation, p) / entityData.CurrentMaximumSpeed.Value;
								num6 += 2f * vehicleContainerType.Aircraft.EstimatedTakeOffLandingTime;
							}
							else
							{
								RegionMap.Result distance6 = regionMap.GetDistance(entity, point, toSubtile, ref distance4);
								if (distance6 == RegionMap.Result.Wait)
								{
									return distance6;
								}
								if (distance6 == RegionMap.Result.NoAccess)
								{
									break;
								}
								num6 = distance4 / entityData.CurrentMaximumSpeed.Value;
							}
							double num7 = GoalEvaluator.ScoreTravelTime(num5 + num6);
							double num8 = GoalEvaluator.ScoreIsEntityFunctional(entityData);
							double num9 = ScoreMainVehicleFunction(entityData.EntityType);
							double num10 = ScoreComfort(entityData);
							double num11 = 0.6 * num7 + 0.1 * num8 + 0.15 * num9 + 0.1 * num10 + 0.05;
							if (num11 > num3)
							{
								bestVehicle = entityData;
								num3 = num11;
							}
							break;
						}
						case RegionMap.Result.NoAccess:
							break;
						}
					}
				}
			}
		}
		return RegionMap.Result.OK;
	}

	private static double ScoreSafety()
	{
		return 1.0;
	}

	private static double ScoreComfort(IKnownEntityData vehicle)
	{
		if (vehicle != null)
		{
			return 1.0;
		}
		return 0.3;
	}

	private static double ScoreMainVehicleFunction(EntityType vehicle)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)vehicle.ContainerType;
		if (vehicleContainerType.MainFunction == VehicleContainerType.Function.PersonalTransport)
		{
			return 1.0;
		}
		if (vehicleContainerType.MainFunction == VehicleContainerType.Function.Hauling)
		{
			return 0.4;
		}
		return 0.0;
	}

	private void GoByVehicle(IKnownEntityData vehicleToUse)
	{
		Rectangle surroundingAreaUsingEntityRadius = Vehicle.GetSurroundingAreaUsingEntityRadius(vehicleToUse, DestinationTilePos.Value.ToPoint());
		if (Vehicle.FindParkingSpot(vehicleToUse.MapPosition.Value, entity.Intelligence.Allegiance, vehicleToUse, surroundingAreaUsingEntityRadius, ((VehicleContainerType)vehicleToUse.EntityType.ContainerType).Transport, entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance, DestinationTilePos.Value.ToPoint(), out var foundLocation))
		{
			AddSubgoal(new GoalMoveToPosition(entity, foundLocation, Nesting.IsNested, ownersOfVehicles));
			AddSubgoal(new GoalExitVehicle(entity, vehicleToUse.EntityID));
			if (IsFinalDestination)
			{
				AddSubgoal(new GoalMoveToPosition(entity, Destination.Value.ToVector3(), ownersOfVehicles, VehicleUse.NoVehicle));
			}
		}
	}

	private void GetPath(Vector3 from, IKnownEntityData destinationData)
	{
		bool flag = false;
		bool flag2 = false;
		if (destinationData != null && destinationData.EntityType.ContainerType != null)
		{
			if (destinationData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
			{
				flag = true;
			}
			if (destinationData.EntityType.ContainerType.CanTransactWithContainer(entity.EntityType))
			{
				flag2 = true;
			}
			else if (destinationData.EntityType.ContainerType is AgentStorageType && attackType == null)
			{
				base.Status = Status.Failed;
				return;
			}
		}
		bool flag3 = false;
		if (destinationData != null && destinationData.ContainedBy.HasValue)
		{
			IKnownEntityData data = null;
			EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(destinationData.ContainedBy.Value, out data);
			if (EntityResultCausesFailedGoal(knownData))
			{
				return;
			}
			if (data.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
			{
				flag3 = true;
			}
		}
		if (destinationData != null && (flag || flag3 || !Common.IsLocationEqual(destinationData.PlaySiteLocation, destinationData.AccessPoint.Value)))
		{
			AddSubgoal(new GoalMoveToPosition(entity, destinationData.AccessPoint.Value, null, VehicleUse.NoVehicle));
			if (flag)
			{
				AddSubgoal(new GoalEnter(entity, DestinationEntity.Value));
			}
			else if (flag3)
			{
				AddSubgoal(new GoalEnter(entity, destinationData.ContainedBy.Value));
			}
		}
		else if (Destination.HasValue || flag2)
		{
			if (!Destination.HasValue && flag2)
			{
				Destination = new WorldLocation(destinationData.AccessPoint.Value);
			}
			if (Common.DistanceOctile(from, Destination.Value) < 210f)
			{
				MovementMap movementMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
				List<PathFinderNode> list = entityIntelligence.PathPlanner.FindShortPathDirectly(movementMap.Layers[entity.GetTransportType()], from, Destination.Value.ToVector3(), 2000);
				if (list != null)
				{
					FollowNewPath(list);
				}
				else
				{
					base.Status = Status.Failed;
				}
			}
			else
			{
				RequestPath(entity.GetTransportType(), from);
			}
		}
		else
		{
			entity.ToString().Contains("August");
			base.Status = Status.Failed;
		}
	}

	private void FollowNewPath(List<PathFinderNode> path)
	{
		foreach (Goal subgoal in Subgoals)
		{
			SetDontStopFlagOnMoveGoals(subgoal);
		}
		RemoveAllSubgoals();
		if (!entity.GetContainedBy(out Entity container))
		{
			base.Status = Status.Failed;
			return;
		}
		if (entity.HasStance())
		{
			ChangeStance(entity.EntityType.LocomotorType.StancesType.MovingStanceType);
		}
		if (container != null)
		{
			AddSubgoal(new GoalExit(entity));
			pathStartLocation = container.AccessPoint.Value;
		}
		AddSubgoal(new GoalFollowPath(entity, path, this, ownersOfVehicles, Destination.Value.ToVector3(), GroupMoveActivity)
		{
			PermittedDistanceSquaredToDestination = PermittedDistanceSquaredToDestination
		});
		UpdateMoveTargetForLerping();
	}

	private void SetDontStopFlagOnMoveGoals(Goal subgoal)
	{
		if (subgoal is GoalTraverseEdgeBetweenWaypoints goalTraverseEdgeBetweenWaypoints)
		{
			goalTraverseEdgeBetweenWaypoints.StopMovingWhenTerminating = false;
		}
		else
		{
			if (!(subgoal is CompositeGoal compositeGoal))
			{
				return;
			}
			foreach (Goal subgoal2 in compositeGoal.Subgoals)
			{
				SetDontStopFlagOnMoveGoals(subgoal2);
			}
		}
	}

	private bool GetFollowerPathToWaypoint(Vector3 from)
	{
		Vector3 location = entityIntelligence.GroupMoveAssignedWaypoint.Location;
		MovementMap movementMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
		List<PathFinderNode> list = entityIntelligence.PathPlanner.FindShortPathDirectly(movementMap.Layers[entity.GetTransportType()], from, location, 10000);
		if (list != null)
		{
			AddSubgoal(new GoalFollowPath(entity, list, this, ownersOfVehicles, location, GroupMoveActivity));
			entityIntelligence.FollowerStatus = FollowerStatus.IsFollowingPathToWaypoint;
			return true;
		}
		base.Status = Status.Failed;
		return false;
	}

	private void RequestPath(SurfaceType.TransportType transportType, Vector3 start)
	{
		SubtileLayers layers = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance).Layers[transportType];
		if (entityIntelligence.PathPlanner.FindPathByRequest(layers, start, Destination.Value.ToVector3()))
		{
			AddSubgoal(new GoalWait(entity));
			The.Sim.AddWaitingAgent(entity, Sim.WaitingFor.Path);
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private void ActivateGroupFollower(Vector3 from)
	{
		if (entityIntelligence.GroupMoveAssignedWaypoint != null)
		{
			StartGroupFollowerMoving(from);
		}
		else
		{
			AddSubgoal(new GoalWait(entity));
		}
	}

	private void StartGroupFollowerMoving(Vector3 from)
	{
		entityIntelligence.IsAtGroupMoveDestination = false;
		MovementMap movementMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
		SurfaceType.TransportType transportType = entity.GetTransportType();
		if (MapManager.IsPathClearToPoint(entity.PlaySiteLocation, entityIntelligence.GroupMoveAssignedWaypoint.Location, movementMap, transportType))
		{
			AddSubgoal(new GoalTraverseEdgeBetweenWaypoints(entity, entityIntelligence.GroupMoveAssignedWaypoint.Location, null, entityIntelligence.GroupMoveAssignedWaypointRay.Value, entityIntelligence.GroupMoveAssignedWaypoint.Number, entityIntelligence.GroupMoveAssignedWaypoint.IsLastWaypoint, movingOutOfHarmsWay: false, ownersOfVehicles, GroupMoveActivity));
		}
		else if (GetFollowerPathToWaypoint(from))
		{
			AddSubgoal(new GoalTraverseEdgeBetweenWaypoints(entity, entityIntelligence.GroupMoveAssignedWaypoint.Location, null, entityIntelligence.GroupMoveAssignedWaypointRay.Value, entityIntelligence.GroupMoveAssignedWaypoint.Number, entityIntelligence.GroupMoveAssignedWaypoint.IsLastWaypoint, movingOutOfHarmsWay: false, ownersOfVehicles, GroupMoveActivity));
		}
	}

	private TimeSpan GetTimeBetweenRepaths(float distance)
	{
		if (distance < 70f)
		{
			return new TimeSpan(0, 0, 0, 0, 500);
		}
		if (distance < 200f)
		{
			return new TimeSpan(0, 0, 2);
		}
		if (distance < 400f)
		{
			return new TimeSpan(0, 0, 4);
		}
		if (distance < 1000f)
		{
			return new TimeSpan(0, 0, 8);
		}
		return new TimeSpan(0, 0, 12);
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Failed)
		{
			noOfFailures++;
			if (!hasTerminated && noOfFailures < 3)
			{
				Activate();
			}
		}
		else if (attackType != null)
		{
			IKnownEntityData data;
			EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out data);
			if (!EntityResultCausesFailedGoal(knownData))
			{
				bool isSeen = knownData == EntityResult.SeenDirectly;
				if (IsWithinRangeOfEntity(data, isSeen))
				{
					base.Status = Status.Completed;
				}
				else
				{
					HandlePeriodicRepathing(data, isSeen);
				}
			}
		}
		if (base.Status == Status.Completed && GroupMoveActivity != null && !GroupMoveActivity.HasEnded)
		{
			PerformGroupBehaviour();
		}
	}

	private bool IsRangedAttack()
	{
		return attackType.RangeType != AttackType.RangeTypes.Melee;
	}

	private bool IsRangedAttackWithinRange(bool isSeen, IKnownEntityData targetData)
	{
		Vector3 playSiteLocation = targetData.PlaySiteLocation;
		if (isSeen)
		{
			Entity entity = (Entity)targetData;
			if (IsTargetEntityMoving(entity))
			{
				Vector3 vector = entity.Locomotor.MoveSpeed * entity.FacingNormal;
				playSiteLocation += vector;
			}
			if ((base.entity.PlaySiteLocation - playSiteLocation).LengthSquared() < attackType.MaxRangeSquared)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWithinRangeOfEntity(IKnownEntityData targetData, bool isSeen)
	{
		if (IsRangedAttack())
		{
			return IsRangedAttackWithinRange(isSeen, targetData);
		}
		if (isSeen)
		{
			Entity entity = (Entity)targetData;
			if (IsTargetEntityMoving(entity) || IsChasingTargetCenterLocation)
			{
				return AttackJob.IsCorrectMeleeDistanceRoundedToSubtiles(base.entity, entity);
			}
		}
		return false;
	}

	private bool IsTargetEntityMoving(Entity e)
	{
		return e.Locomotor.IsMoving();
	}

	private void HandlePeriodicRepathing(IKnownEntityData targetData, bool isSeen)
	{
		if (!repathRegulator.IsReady())
		{
			return;
		}
		Vector3 playSiteLocation = targetData.PlaySiteLocation;
		if (!(playSiteLocation != lastTargetLocation))
		{
			return;
		}
		lastTargetLocation = playSiteLocation;
		float num = Common.DistanceOctile(playSiteLocation, entity.PlaySiteLocation);
		if (The.Sim.TotalUnPausedGameTime.Subtract(timePointForLastRepath).CompareTo(GetTimeBetweenRepaths(num)) <= 0)
		{
			return;
		}
		timePointForLastRepath = The.Sim.TotalUnPausedGameTime;
		Vector3? meleeLocation = null;
		if (num < 350f && useMeleeLocationAsDestination)
		{
			if (!CombatInfo.GetMeleeLocation(entity, targetData, out meleeLocation, out IsChasingTargetCenterLocation))
			{
				base.Status = Status.Failed;
				return;
			}
		}
		else
		{
			meleeLocation = targetData.Location;
		}
		ResetDestination(meleeLocation.Value);
		ActivateAndGetPath();
	}

	private void PerformGroupBehaviour()
	{
		if (GroupMoveActivity.AllAreReady())
		{
			GroupMoveActivity.HasEnded = true;
			GroupMoveActivity.SendMessageToEveryoneElse(entity, new Message(Message.MessageTypes.EndGroupMovement));
		}
		else if (Subgoals.Count > 0 && Subgoals.Peek() is GoalTraverseEdgeBetweenWaypoints goalTraverseEdgeBetweenWaypoints && entityIntelligence.GroupMoveAssignedWaypoint != null && goalTraverseEdgeBetweenWaypoints.Number < entityIntelligence.GroupMoveAssignedWaypoint.Number)
		{
			base.Status = Status.Active;
			StartGroupFollowerMoving(entity.AccessPoint.Value);
		}
		else
		{
			base.Status = Status.Active;
			AddSubgoal(new GoalWait(entity));
		}
	}

	public override void Deactivate()
	{
		entity.Locomotor.MoveSpeed = 0f;
		if (UsedVehicle.HasValue && VehicleUseByGoal == VehicleUse.FreeUpAfterUse)
		{
			entityIntelligence.GetKnownData(UsedVehicle.Value, out var data);
			if (data != null)
			{
				entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
			}
		}
	}

	public void RegisterPathDoneSubscriber(Action<float> method, IIDEventSubscriber subscriber, out MethodID? methodID)
	{
		if (RepathDoneEvent == null)
		{
			RepathDoneEvent = new IDActionEvent<float>();
		}
		RepathDoneEvent.AddAndRegister(method, subscriber, out methodID);
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.PathFound:
				if (Destination.HasValue)
				{
					List<PathFinderNode> path = entityIntelligence.PathPlanner.Path;
					FollowNewPath(path);
					if (RepathDoneEvent != null)
					{
						RepathDoneEvent.Invoke(path.Count * 16);
					}
				}
				else
				{
					base.Status = Status.Failed;
				}
				return true;
			case Message.MessageTypes.PathNotFound:
				base.Status = Status.Failed;
				return true;
			case Message.MessageTypes.DistanceFound:
			case Message.MessageTypes.DistanceFoundNoAccess:
				if (isEvaluatingPersonalTransport)
				{
					isEvaluatingPersonalTransport = false;
					RemoveAllSubgoals();
					IKnownEntityData data = null;
					if (DestinationEntity.HasValue)
					{
						EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out data);
						if (EntityResultCausesFailedGoal(knownData))
						{
							return true;
						}
					}
					DecideOnVehicleOrGoByFoot(data);
					return true;
				}
				return false;
			case Message.MessageTypes.StartGroupMovement:
				if (GroupMoveActivity != null && GroupMoveActivity.IsFollower(entity))
				{
					RemoveAllSubgoals();
					StartGroupFollowerMoving(entity.AccessPoint.Value);
				}
				return true;
			case Message.MessageTypes.EndGroupMovement:
				if (GroupMoveActivity != null)
				{
					base.Status = Status.Completed;
				}
				return true;
			default:
				return false;
			}
		}
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
		Destination = sn.DoWorldLocationNullable(Destination);
		DestinationTilePos = sn.DoTilePosNullable(DestinationTilePos);
		DestinationEntity = sn.DoEntityIDNullable(DestinationEntity);
		PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);
		attackType = sn.DoGameData(attackType);
		IsChasingTargetCenterLocation = sn.DoBool(IsChasingTargetCenterLocation);
		IsFinalDestination = sn.DoBool(IsFinalDestination);
		nesting = sn.DoEnum(nesting);
		UsedVehicle = sn.DoEntityIDNullable(UsedVehicle);
		VehicleUseByGoal = sn.DoEnum(VehicleUseByGoal);
		noOfFailures = sn.DoInt32(noOfFailures);
		pathStartLocation = sn.DoVector3(pathStartLocation);
		isEvaluatingPersonalTransport = sn.DoBool(isEvaluatingPersonalTransport);
		lastTargetLocation = sn.DoVector3(lastTargetLocation);
		timePointForLastRepath = sn.DoTimeSpan(timePointForLastRepath);
		useMeleeLocationAsDestination = sn.DoBool(useMeleeLocationAsDestination);
		RepathDoneEvent = (IDActionEvent<float>)sn.DoISnapshot(RepathDoneEvent);
		sn.Ignore(repathRegulator);
		sn.Ignore(GroupMoveActivity);
		return this;
	}
}
