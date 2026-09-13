using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalHaul : CompositeGoal, ITopLevelGoal
{
	private enum ExtraCargoResult
	{
		OK,
		NoneFound,
		Wait
	}

	private enum RateCargoResult
	{
		Wait,
		Finished,
		KeepLooking
	}

	public HaulingJob job;

	private JobID? snapshotJob;

	public OwnerID? NewOwner;

	private EntityGroupID ownerOfJobsID;

	private Queue<GoalHaul> goalsToNest = new Queue<GoalHaul>();

	private Queue<GoalID> snapshotGoalsToNest;

	private List<GoalHaul> addedNestedGoals = new List<GoalHaul>();

	private List<GoalID> snapshotAddedNestedGoals;

	public GoalHaul ParentGoal;

	private GoalID? snapshotParent;

	private EntityID? vehicleID;

	public List<HaulingJob> DestinationPiledItemJobs;

	private List<JobID> snapshotDestinationPiledItemJobs;

	private List<Tuple<HaulingJob, IKnownEntityData>> SourcePiledItemJobs;

	public bool DestinationPileIsHandled;

	public bool SourcePileIsHandled;

	private bool isFindingExtraJobs;

	public bool IsOuterGoal = true;

	public bool IsStandingBentOverItem;

	private bool isCancelled;

	private EntityID itemToHaul;

	private bool isActivated;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalHaul(Entity entity, HaulingJob job, EntityID item, EntityID? vehicle, OwnerID? newOwner, List<EntityGroupID> ownersOfVehicles, EntityGroupID ownerOfJobs)
		: base(entity)
	{
		itemToHaul = item;
		this.job = job;
		_ = job.ID;
		_ = 3288;
		if (job is HaulingJobAnyItemOfType haulingJobAnyItemOfType)
		{
			string text = "";
			if (job.Item.HasValue)
			{
				text = "job item before: " + job.Item.Value.ToString() + ", now: ";
			}
			text = text + item.ToString() + "was set by " + entity.ToString();
			haulingJobAnyItemOfType.AddLog(text);
		}
		this.job.Item = item;
		vehicleID = vehicle;
		base.ownersOfVehicles = ownersOfVehicles;
		NewOwner = newOwner;
		ownerOfJobsID = ownerOfJobs;
		if (vehicleID.HasValue)
		{
			SourcePiledItemJobs = new List<Tuple<HaulingJob, IKnownEntityData>>();
			DestinationPiledItemJobs = new List<HaulingJob>();
		}
	}

	public GoalHaul(Entity entity, HaulingJob job, EntityID item, EntityID? vehicle, GoalHaul parentGoal, bool isOuterGoal, OwnerID? newOwner, List<EntityGroupID> ownersOfVehicles)
		: base(entity)
	{
		itemToHaul = item;
		this.job = job;
		_ = job.ID;
		_ = 3288;
		if (job is HaulingJobAnyItemOfType haulingJobAnyItemOfType)
		{
			string text = "";
			if (job.Item.HasValue)
			{
				text = "job item before: " + job.Item.Value.ToString() + ", now: ";
			}
			text = text + item.ToString() + "was set by " + entity.ToString();
			haulingJobAnyItemOfType.AddLog(text);
		}
		this.job.Item = item;
		vehicleID = vehicle;
		base.ownersOfVehicles = ownersOfVehicles;
		IsOuterGoal = isOuterGoal;
		ParentGoal = parentGoal;
		NewOwner = newOwner;
		if (vehicleID.HasValue)
		{
			SourcePiledItemJobs = new List<Tuple<HaulingJob, IKnownEntityData>>();
			DestinationPiledItemJobs = new List<HaulingJob>();
		}
	}

	public GoalHaul()
	{
	}

	protected override void Activate()
	{
		if (IsOuterGoal)
		{
			job.TakeJob(entity);
		}
		if (!job.Item.HasValue)
		{
			base.Status = Status.Failed;
		}
		else
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out var data)))
			{
				return;
			}
			IKnownEntityData data2 = null;
			if (vehicleID.HasValue)
			{
				if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out data2)))
				{
					return;
				}
				entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(data2.EntityID, entity.EntityID);
			}
			if (!data.AssignedToJob.HasValue || data.AssignedToJob == job.ID)
			{
				base.Status = Status.Active;
				isActivated = true;
				data.AssignedToJob = job.ID;
				RemoveAllSubgoals();
				entity.AgentStorage.MountedToolOrWeapon = null;
				if (IsOuterGoal)
				{
					ExtraCargoResult extraCargoResult = FindExtraCargo(data, data2);
					if (base.Status == Status.Failed)
					{
						return;
					}
					if (extraCargoResult == ExtraCargoResult.Wait)
					{
						base.Status = Status.Inactive;
						return;
					}
					DropItemsOverCapacity(data);
					if (base.Status == Status.Failed)
					{
						return;
					}
					if (job.RequiresBoldStance)
					{
						entityIntelligence.SetBoldStance();
					}
				}
				UnfoldGoal(data);
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
	}

	private void SetLongDistanceHaulingFlag()
	{
		if (vehicleID.HasValue || goalsToNest.Count != 0)
		{
			return;
		}
		if (job.GetToLocation(entity, out var toLocation))
		{
			if (Common.DistanceOctile(entity.PlaySiteLocation, toLocation.Value) > GameData.Instance.Constants.DistanceForLongHaul)
			{
				entity.Renderable.SetAnimationStateFlag(AnimModifier.Far);
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private bool IsNextPickupItemInSameSpot(Vector3 spot)
	{
		if (goalsToNest.Count > 0)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(goalsToNest.Peek().job.Item.Value, out var data)))
			{
				return false;
			}
			if (MapManager.WorldPosToSubtile(spot) == MapManager.WorldPosToSubtile(data.PlaySiteLocation))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void Terminate()
	{
		base.Terminate();
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Far);
	}

	private void UnfoldGoal(IKnownEntityData itemData)
	{
		Vector3 playSiteLocation = itemData.PlaySiteLocation;
		if (!vehicleID.HasValue)
		{
			if (IsOuterGoal && job.ToLocation.HasValue)
			{
				float num = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, job.ToLocation.Value);
				float num2 = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, playSiteLocation);
				Vector3 destination = ((!(num > num2)) ? playSiteLocation : job.ToLocation.Value);
				FindOptionalEquipmentIfNeeded(destination, job, equipWeapon: true, equipFood: true, mountWeapon: false);
			}
			if (MapManager.WorldPosToSubtile(entity.Location.Value) != MapManager.WorldPosToSubtile(itemData.PlaySiteLocation))
			{
				float permittedDistanceSquaredToDestination = (float)Math.Pow(GameData.Instance.Constants.InteractionDistanceForAgents, 2.0);
				AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, itemData, GoalMoveToPosition.VehicleUse.NoVehicle)
				{
					PermittedDistanceSquaredToDestination = permittedDistanceSquaredToDestination
				});
			}
			bool mountAfterPickup = false;
			if (IsOuterGoal && goalsToNest.Count == 0)
			{
				mountAfterPickup = itemData.EntityType.CanBeMounted(entity);
			}
			bool standUpAfterwards;
			if (IsNextPickupItemInSameSpot(itemData.PlaySiteLocation))
			{
				goalsToNest.Peek().IsStandingBentOverItem = true;
				standUpAfterwards = false;
			}
			else
			{
				standUpAfterwards = true;
			}
			bool bendDown = !IsStandingBentOverItem;
			if (PickupItemOrUnloadFirst(itemData, mountAfterPickup, bendDown, standUpAfterwards))
			{
				if (goalsToNest.Count > 0)
				{
					GoalHaul goalHaul = goalsToNest.Dequeue();
					goalHaul.ParentGoal = this;
					goalHaul.goalsToNest = goalsToNest;
					AddSubgoal(goalHaul);
					addedNestedGoals.Add(goalHaul);
				}
				SetLongDistanceHaulingFlag();
				AddSubgoal(new GoalDropItem(entity, job.Item.Value, job.RequiredByProcessJob, job.ToLocation, job.ToStorage, isHauledItemDestination: true, job.ID));
			}
		}
		else
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(vehicleID.Value, out var data)))
			{
				return;
			}
			if (!entity.GetDrivenVehicle(out var vehicle))
			{
				base.Status = Status.Failed;
				return;
			}
			if (vehicle != data && Common.DistanceOctile(data.PlaySiteLocation, playSiteLocation) > ((VehicleContainerType)data.EntityType.ContainerType).LoadingRadius)
			{
				AddSubgoal(new GoalEnterVehicleAsDriver(entity, vehicleID.Value, null));
				AddSubgoal(new GoalMoveToPosition(entity, playSiteLocation, ownersOfVehicles, GoalMoveToPosition.VehicleUse.KeepVehicle));
			}
			if (!SourcePileIsHandled)
			{
				HandleItemsThatWillBePiledAtSource(SourcePiledItemJobs, itemData);
				SourcePiledItemJobs.Add(new Tuple<HaulingJob, IKnownEntityData>(job, itemData));
			}
			MoveItemsInSourcePile(SourcePiledItemJobs);
			bool num3 = goalsToNest.Count > 0;
			if (num3)
			{
				HandleItemsThatWillBePiledAtDestination();
				GoalHaul goalHaul2 = goalsToNest.Dequeue();
				goalHaul2.goalsToNest = goalsToNest;
				goalHaul2.ParentGoal = this;
				AddSubgoal(goalHaul2);
			}
			if (!num3 && (ParentGoal != null || IsOuterGoal))
			{
				AddSubgoal(new GoalMoveToPosition(entity, job.ToLocation.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.KeepVehicle)
				{
					UsedVehicle = vehicleID
				});
				AddSubgoal(new GoalExitVehicle(entity, vehicleID.Value));
			}
			if (!DestinationPileIsHandled)
			{
				DestinationPiledItemJobs.Add(job);
			}
			MoveItemsInDestinationPile(DestinationPiledItemJobs);
		}
	}

	private ExtraCargoResult FindExtraCargo(IKnownEntityData itemData, IKnownEntityData vehicleData)
	{
		bool usingAirTransport = false;
		float num;
		if (vehicleData != null)
		{
			entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(vehicleData.EntityID, base.entity.EntityID);
			num = ((!vehicleData.ContainsEntity(job.Item.Value)) ? (vehicleData.TotalItemStorageCapacity.Value - vehicleData.TotalStored.Value - itemData.Bulk) : (vehicleData.TotalItemStorageCapacity.Value - vehicleData.TotalStored.Value));
			if (((VehicleContainerType)vehicleData.EntityType.ContainerType).Aircraft != null)
			{
				usingAirTransport = true;
			}
		}
		else
		{
			num = base.entity.AgentStorage.ItemStorage.TotalCapacity;
			if (!base.entity.AgentStorage.ItemStorage.Contains(job.Item.Value))
			{
				num -= itemData.Bulk;
			}
			for (int num2 = base.entity.AgentStorage.ItemStorage.StoredItems.Count - 1; num2 >= 0; num2--)
			{
				EntityID entityID = base.entity.AgentStorage.ItemStorage.StoredItems[num2];
				if (entityIntelligence.GetKnownData(entityID, out var data) != EntityResult.SeenDirectly)
				{
					base.entity.AgentStorage.ItemStorage.RemoveOutdatedItem(entityID);
				}
				else
				{
					Entity entity = (Entity)data;
					_ = entity.Item;
					if (ItemIsCurrentlyHauledByUs(entity))
					{
						num -= entity.Bulk;
					}
				}
			}
		}
		Vector3? toLocation = job.ToLocation;
		EntityID? lastDestinationEntity = job.GetToStorageEntity;
		_ = itemData.PlaySiteLocation;
		while (num > 0f)
		{
			if (base.entity.ID == (EntityID)25178L)
			{
				_ = itemData.EntityID;
				_ = 25221;
			}
			HaulingJob addedJob;
			IKnownEntityData addedItem;
			ExtraCargoResult extraCargoResult = FindExtraCargo(base.entity, itemData, toLocation, lastDestinationEntity, num, usingAirTransport, out addedJob, out addedItem);
			if (base.Status == Status.Failed)
			{
				return ExtraCargoResult.NoneFound;
			}
			switch (extraCargoResult)
			{
			case ExtraCargoResult.Wait:
				AddSubgoal(new GoalWait(base.entity));
				isFindingExtraJobs = true;
				CancelNestedGoals();
				return ExtraCargoResult.Wait;
			case ExtraCargoResult.OK:
			{
				if (base.entity.ID == (EntityID)25178L)
				{
					_ = addedItem.EntityID;
					_ = 25221;
				}
				addedJob.TakeJob(base.entity);
				addedItem.AssignedToJob = addedJob.ID;
				GoalHaul item = new GoalHaul(base.entity, addedJob, addedItem.EntityID, vehicleID, this, isOuterGoal: false, addedJob.NewOwner, ownersOfVehicles);
				goalsToNest.Enqueue(item);
				toLocation = addedJob.ToLocation;
				lastDestinationEntity = ((!addedJob.ToStorage.HasValue) ? ((EntityID?)null) : new EntityID?(addedJob.ToStorage.Value.StorageEntity));
				break;
			}
			default:
				return ExtraCargoResult.OK;
			}
			_ = addedItem.PlaySiteLocation;
			num -= addedItem.Bulk;
		}
		return ExtraCargoResult.OK;
	}

	private bool ItemIsCurrentlyHauledByUs(Entity item)
	{
		return EvaluateJob.ResolveAssignedToJob(item)?.TakenBy.Contains(entity) ?? false;
	}

	private void DropItemsOverCapacity(IKnownEntityData itemData)
	{
		float num = 0f;
		if (!base.entity.AgentStorage.ItemStorage.Contains(job.Item.Value))
		{
			num += itemData.Bulk;
		}
		foreach (GoalHaul item in goalsToNest)
		{
			if (!base.entity.AgentStorage.ItemStorage.Contains(item.job.Item.Value))
			{
				IKnownEntityData data;
				EntityResult knownData = entityIntelligence.GetKnownData(item.job.Item.Value, out data);
				if (EntityResultCausesFailedGoal(knownData))
				{
					return;
				}
				num += data.Bulk;
			}
		}
		float num2 = base.entity.AgentStorage.ItemStorage.TotalStored + num;
		if (!Common.IsGreaterThan(num2, base.entity.AgentStorage.ItemStorage.TotalCapacity))
		{
			return;
		}
		List<Entity> list = new List<Entity>();
		for (int num3 = base.entity.AgentStorage.ItemStorage.StoredItems.Count - 1; num3 >= 0; num3--)
		{
			EntityID entityID = base.entity.AgentStorage.ItemStorage.StoredItems[num3];
			if (EntityIsNotSeenDirectly(entityID, out var entity))
			{
				base.entity.AgentStorage.ItemStorage.StoredItems.RemoveAt(num3);
			}
			else
			{
				_ = entity.Item;
				if (!ItemIsCurrentlyHauledByUs(entity))
				{
					num2 -= entity.Bulk;
					list.Add(entity);
					if (Common.IsLessThanOrEqual(num2, base.entity.AgentStorage.ItemStorage.TotalCapacity))
					{
						break;
					}
				}
			}
		}
		foreach (Entity item2 in list)
		{
			AddSubgoal(new GoalDropItem(base.entity, item2.EntityID));
		}
	}

	private ExtraCargoResult FindExtraCargo(Entity entity, IKnownEntityData lastItem, Vector3? lastDestination, EntityID? lastDestinationEntity, double capacity, bool usingAirTransport, out HaulingJob addedJob, out IKnownEntityData addedItem)
	{
		addedJob = null;
		addedItem = null;
		if (capacity <= 0.0)
		{
			return ExtraCargoResult.NoneFound;
		}
		IKnownEntityData bestItem = null;
		HaulingJob bestJob = null;
		float bestDistance = 1000000f;
		MovementMap movementMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, entity.Intelligence.ThreatStance);
		IKnownEntityData data = null;
		if (lastDestinationEntity.HasValue && GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(lastDestinationEntity.Value, out data)))
		{
			return ExtraCargoResult.NoneFound;
		}
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfJobsID);
		if (entityGroup == null)
		{
			base.Status = Status.Failed;
			return ExtraCargoResult.NoneFound;
		}
		for (int num = entityGroup.HaulingJobs.Count - 1; num >= 0; num--)
		{
			Job jobToDestroy = entityGroup.HaulingJobs[num];
			if (jobToDestroy.TakenBy.Count < jobToDestroy.MaxJobPositions)
			{
				EntityID? getToStorageEntity = ((HaulingJob)jobToDestroy).GetToStorageEntity;
				IKnownEntityData data2 = null;
				if (getToStorageEntity.HasValue && GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(getToStorageEntity.Value, out data2)))
				{
					DestroyJobAndRemoveLocks(ref jobToDestroy);
				}
				else if (jobToDestroy is HaulingJobAnyItemOfType)
				{
					HaulingJobAnyItemOfType haulingJobAnyItemOfType = (HaulingJobAnyItemOfType)jobToDestroy;
					EntityGroup entityGroup2 = LookUp<EntityGroup, EntityGroupID>.FindByID(haulingJobAnyItemOfType.ItemsToHaulGroup);
					if (entityGroup2 == null)
					{
						DestroyJobAndRemoveLocks(ref jobToDestroy);
					}
					else if (entityGroup2.Items.ContainsKey(haulingJobAnyItemOfType.RequiredItemType) && (double?)haulingJobAnyItemOfType.RequiredItemType.ItemType.MaximumBulk <= capacity)
					{
						List<EntityID> list = entityGroup2.Items[haulingJobAnyItemOfType.RequiredItemType];
						for (int num2 = list.Count - 1; num2 >= 0; num2--)
						{
							EntityID entityID = list[num2];
							if (GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, entityID, entityGroup2, out var entityData))
							{
								switch (RateItemAsAdditionalCargo(haulingJobAnyItemOfType, data2, entityData, lastItem, lastDestination, data, usingAirTransport, ref bestItem, ref bestDistance, ref bestJob, movementMap, entity))
								{
								case RateCargoResult.Finished:
									entityData?.IsCompleted();
									addedJob = haulingJobAnyItemOfType;
									addedItem = entityData;
									return ExtraCargoResult.OK;
								case RateCargoResult.Wait:
									return ExtraCargoResult.Wait;
								}
							}
						}
					}
				}
				else if (jobToDestroy is HaulingJobSpecificItem)
				{
					HaulingJobSpecificItem haulingJobSpecificItem = (HaulingJobSpecificItem)jobToDestroy;
					IKnownEntityData data3;
					EntityResult knownData = entityIntelligence.GetKnownData(haulingJobSpecificItem.Item.Value, out data3);
					if (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown)
					{
						DestroyJobAndRemoveLocks(ref jobToDestroy);
					}
					else if ((double)data3.Bulk <= capacity)
					{
						switch (RateItemAsAdditionalCargo(haulingJobSpecificItem, data2, data3, lastItem, lastDestination, data, usingAirTransport, ref bestItem, ref bestDistance, ref bestJob, movementMap, entity))
						{
						case RateCargoResult.Finished:
							data3?.IsCompleted();
							addedJob = haulingJobSpecificItem;
							addedItem = data3;
							return ExtraCargoResult.OK;
						case RateCargoResult.Wait:
							return ExtraCargoResult.Wait;
						}
					}
				}
			}
		}
		if (bestItem != null)
		{
			addedItem = bestItem;
			addedJob = bestJob;
			return ExtraCargoResult.OK;
		}
		return ExtraCargoResult.NoneFound;
	}

	private void DestroyJobAndResetThreatstance()
	{
		if (IsOuterGoal)
		{
			ResetThreatStance(job);
		}
		DestroyJobAndRemoveLocks(ref job);
	}

	private RateCargoResult RateItemAsAdditionalCargo(HaulingJob hj, IKnownEntityData toStorageEntityData, IKnownEntityData item, IKnownEntityData lastItem, Vector3? lastDestination, IKnownEntityData lastDestinationEntity, bool usingAirTransport, ref IKnownEntityData bestItem, ref float bestDistance, ref HaulingJob bestJob, MovementMap moveMap, Entity entity)
	{
		if (entity.ID == (EntityID)25178L)
		{
			_ = item.EntityID;
			_ = 25221;
		}
		HaulingJobAnyItemOfType obj = hj as HaulingJobAnyItemOfType;
		HaulingJobSpecificItem haulingJobSpecificItem = hj as HaulingJobSpecificItem;
		if (((obj != null && item.IsUnassigned(entityIntelligence.Allegiance.SharedKnowledge)) || (haulingJobSpecificItem != null && item.IsUnassignedToAnythingButThisJob(haulingJobSpecificItem, entityIntelligence.Allegiance.SharedKnowledge))) && item.IsItemValidForHauling(entity, entityIntelligence, hj))
		{
			if (item.ContainedBy.HasValue)
			{
				Entity entity2 = Entity.FindByID(item.ContainedBy.Value);
				if (entity2 != null && entity2.AgentStorage != null && entity2.AgentStorage.Contains(item.EntityID))
				{
					return RateCargoResult.KeepLooking;
				}
			}
			float distance = -1f;
			float distance2 = -1f;
			float distance3 = -1f;
			if (!usingAirTransport)
			{
				RegionMap regionMap = moveMap.Layers[SurfaceType.TransportType.Foot].RegionMap;
				switch (regionMap.GetDistanceToEntity(entity, lastItem, item, ref distance))
				{
				case RegionMap.Result.Wait:
					return RateCargoResult.Wait;
				case RegionMap.Result.NoAccess:
					return RateCargoResult.KeepLooking;
				}
				switch (regionMap.GetDistanceToEntityUsingWorldLocation(entity, lastDestinationEntity, toStorageEntityData, ref distance2, lastDestination, hj.ToLocation))
				{
				case RegionMap.Result.Wait:
					return RateCargoResult.Wait;
				case RegionMap.Result.NoAccess:
					return RateCargoResult.KeepLooking;
				}
				switch (regionMap.GetDistanceToEntityUsingWorldLocation(entity, lastDestinationEntity, item, ref distance3, lastDestination, hj.ToLocation))
				{
				case RegionMap.Result.Wait:
					return RateCargoResult.Wait;
				case RegionMap.Result.NoAccess:
					return RateCargoResult.KeepLooking;
				}
			}
			else
			{
				distance = Common.DistanceOctile(lastItem.PlaySiteLocation, item.PlaySiteLocation);
				Vector3 p = lastDestination ?? lastDestinationEntity.AccessPoint.Value;
				Vector3 p2 = hj.ToLocation ?? toStorageEntityData.AccessPoint.Value;
				distance2 = Common.DistanceOctile(p, p2);
				distance3 = Common.DistanceOctile(p, item.PlaySiteLocation);
			}
			if (distance + distance2 < 0.6f * distance3)
			{
				if (distance + distance2 == 0f)
				{
					bestItem = item;
					bestJob = hj;
					bestDistance = distance + distance2;
					return RateCargoResult.Finished;
				}
				if (distance + distance2 < bestDistance || bestItem == null)
				{
					bestItem = item;
					bestJob = hj;
					bestDistance = distance + distance2;
				}
			}
		}
		return RateCargoResult.KeepLooking;
	}

	public void HandleItemsThatWillBePiledAtSource(List<Tuple<HaulingJob, IKnownEntityData>> piledSourceJobs, IKnownEntityData itemData)
	{
		Point point = MapManager.WorldPosToTile(itemData.PlaySiteLocation);
		if (goalsToNest == null || goalsToNest.Count <= 0)
		{
			return;
		}
		foreach (GoalHaul item in goalsToNest)
		{
			if (!EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(item.job.Item.Value, out var data)) && MapManager.WorldPosToTile(data.PlaySiteLocation) == point)
			{
				piledSourceJobs.Add(new Tuple<HaulingJob, IKnownEntityData>(item.job, data));
				item.SourcePileIsHandled = true;
				continue;
			}
			break;
		}
	}

	public void HandleItemsThatWillBePiledAtDestination()
	{
		if (DestinationPileIsHandled || goalsToNest == null || goalsToNest.Count <= 0)
		{
			return;
		}
		foreach (GoalHaul item in goalsToNest)
		{
			if ((item.job.ToStorage.HasValue && job.ToStorage.HasValue && item.job.ToStorage.Value.StorageEntity == job.ToStorage.Value.StorageEntity) || MapManager.WorldPosToTile(item.job.ToLocation.Value) == MapManager.WorldPosToTile(job.ToLocation.Value))
			{
				DestinationPiledItemJobs.Add(item.job);
				item.DestinationPileIsHandled = true;
				continue;
			}
			break;
		}
	}

	private void MoveItemsInSourcePile(List<Tuple<HaulingJob, IKnownEntityData>> piledSourceJobs)
	{
		if (piledSourceJobs == null)
		{
			return;
		}
		if (piledSourceJobs.Count == 1)
		{
			AddSubgoal(new GoalPickup(entity, piledSourceJobs[0].Item1.Item.Value, NewOwner, StorageCompartment.Haul, false));
			AddSubgoal(new GoalLoad(entity, vehicleID.Value, piledSourceJobs[0].Item1.Item.Value, NewOwner));
		}
		else
		{
			if (piledSourceJobs.Count <= 1)
			{
				return;
			}
			double num = entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored;
			List<Tuple<HaulingJob, IKnownEntityData>> list = new List<Tuple<HaulingJob, IKnownEntityData>>();
			int num2 = 0;
			while (num2 < piledSourceJobs.Count)
			{
				double num3 = num;
				list.Clear();
				while (num3 > 0.0 && num2 < piledSourceJobs.Count && (double)piledSourceJobs[num2].Item2.Bulk <= num3)
				{
					list.Add(piledSourceJobs[num2]);
					num3 -= (double)piledSourceJobs[num2].Item2.Bulk;
					num2++;
				}
				for (int i = 0; i < list.Count; i++)
				{
					EntityID? containedBy = list[i].Item2.ContainedBy;
					if (containedBy.HasValue)
					{
						entityIntelligence.GetKnownData(containedBy.Value, out var data);
						if (data != null && data.EntityType.ContainerType is VehicleContainerType && containedBy != vehicleID)
						{
							AddSubgoal(new GoalUnload(entity, containedBy.Value, list[i].Item1.Item.Value));
						}
					}
					AddSubgoal(new GoalPickup(entity, list[i].Item1.Item.Value, NewOwner, StorageCompartment.Haul, false));
				}
				for (int j = 0; j < list.Count; j++)
				{
					AddSubgoal(new GoalLoad(entity, vehicleID.Value, list[j].Item1.Item.Value, NewOwner));
				}
			}
		}
	}

	private void MoveItemsInDestinationPile(List<HaulingJob> piledDestinationJobs)
	{
		if (piledDestinationJobs == null)
		{
			return;
		}
		if (piledDestinationJobs.Count == 1)
		{
			AddSubgoal(new GoalUnload(entity, vehicleID.Value, piledDestinationJobs[0].Item.Value));
			AddSubgoal(new GoalPickup(entity, piledDestinationJobs[0].Item.Value, NewOwner, StorageCompartment.Haul, false));
			AddSubgoal(new GoalDropItem(entity, piledDestinationJobs[0].Item.Value, piledDestinationJobs[0].RequiredByProcessJob, job.ToLocation, job.ToStorage));
		}
		else
		{
			if (piledDestinationJobs.Count <= 1)
			{
				return;
			}
			double num = entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored;
			List<HaulingJob> list = new List<HaulingJob>();
			int num2 = 0;
			foreach (HaulingJob piledDestinationJob in piledDestinationJobs)
			{
				AddSubgoal(new GoalUnload(entity, vehicleID.Value, piledDestinationJob.Item.Value));
			}
			while (num2 < piledDestinationJobs.Count)
			{
				double num3 = num;
				list.Clear();
				if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(piledDestinationJobs[num2].Item.Value, out var data)))
				{
					break;
				}
				while (num3 > 0.0 && num2 < piledDestinationJobs.Count && (double)data.Bulk <= num3)
				{
					if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(piledDestinationJobs[num2].Item.Value, out data)))
					{
						return;
					}
					list.Add(piledDestinationJobs[num2]);
					num3 -= (double)data.Bulk;
					num2++;
				}
				for (int i = 0; i < list.Count; i++)
				{
					AddSubgoal(new GoalPickup(entity, list[i].Item.Value, NewOwner, StorageCompartment.Haul, false));
				}
				for (int j = 0; j < list.Count; j++)
				{
					HaulingJob haulingJob = list[j];
					AddSubgoal(new GoalDropItem(entity, haulingJob.Item.Value, haulingJob.RequiredByProcessJob, haulingJob.ToLocation, job.ToStorage));
				}
			}
		}
	}

	public double ScoreGoal()
	{
		if (ParentGoal != null)
		{
			return ParentGoal.ScoreGoal();
		}
		IKnownEntityData data = null;
		if (vehicleID.HasValue && EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out data)))
		{
			base.Status = Status.Failed;
			return 0.0;
		}
		_ = entity.ID;
		_ = 4889;
		return ScoreJobGoal(job, null, null, new HaulingParams
		{
			Item = job.Item.Value,
			Vehicle = data
		});
	}

	public override bool IsSame(Job job)
	{
		if (DestinationPiledItemJobs != null && DestinationPiledItemJobs.Count > 0)
		{
			foreach (HaulingJob destinationPiledItemJob in DestinationPiledItemJobs)
			{
				if (job == destinationPiledItemJob)
				{
					return true;
				}
			}
		}
		if (goalsToNest != null && goalsToNest.Count > 0)
		{
			foreach (GoalHaul item in goalsToNest)
			{
				if (item.job == job)
				{
					return true;
				}
			}
		}
		if (addedNestedGoals != null && addedNestedGoals.Count > 0)
		{
			foreach (GoalHaul addedNestedGoal in addedNestedGoals)
			{
				if (addedNestedGoal.job == job)
				{
					return true;
				}
			}
		}
		return job == this.job;
	}

	protected override bool ArePreconditionsOK()
	{
		if (!job.Item.HasValue)
		{
			return false;
		}
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out var data)))
		{
			return false;
		}
		if (!data.CanBeHauled())
		{
			return false;
		}
		return true;
	}

	public void AssertAllSubgoalJobsTaken()
	{
	}

	private bool GetsRecentlyHauledScoreBonus()
	{
		return TimeSpentInTopLevelGoal > 3.0;
	}

	private void SetRecentlyHauledOnAllItems()
	{
		if (job != null && job.Item.HasValue)
		{
			entityIntelligence.Memory.SetLastHauledItem(job.Item.Value);
		}
		if (addedNestedGoals == null)
		{
			return;
		}
		foreach (GoalHaul addedNestedGoal in addedNestedGoals)
		{
			if (addedNestedGoal.job != null && addedNestedGoal.job.Item.HasValue)
			{
				entityIntelligence.Memory.SetLastHauledItem(addedNestedGoal.job.Item.Value);
			}
		}
		foreach (GoalHaul item in goalsToNest)
		{
			if (item.job != null && item.job.Item.HasValue)
			{
				entityIntelligence.Memory.SetLastHauledItem(item.job.Item.Value);
			}
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (job is HaulingJobAnyItemOfType && !job.Item.HasValue)
		{
			base.Status = Status.Failed;
			return;
		}
		AssertAllSubgoalJobsTaken();
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			if (base.Status == Status.Active)
			{
				base.Status = ProcessSubgoals(elapsed);
				if (GetsRecentlyHauledScoreBonus())
				{
					SetRecentlyHauledOnAllItems();
				}
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
		if (base.Status == Status.Completed)
		{
			job.MarkClientAsInputNotEnroute();
			DestroyJobAndResetThreatstance();
			FreeUpVehicle();
		}
		else if (base.Status == Status.Failed)
		{
			job.MarkClientAsInputNotEnroute();
			FreeUpVehicle();
		}
	}

	public override string ToString()
	{
		return string.Format(arg1: (job == null) ? "Job Has Been Destroyed" : job.ToString(), format: "{0} {1}", arg0: base.ToString());
	}

	public override string GetStatus()
	{
		return "Hauling";
	}

	private void AbandonJob()
	{
		if (isCancelled)
		{
			return;
		}
		if (job != null)
		{
			if (job.TakenBy.Count > 1)
			{
				throw new Exception("more than one taker? that's wrong");
			}
			if (job.TakenBy.Count > 0 && (job.TakenBy.Count <= 0 || job.TakenBy.Get(0) == base.entity))
			{
				if (entityIntelligence.GetKnownData(job.Item.Value, out var data) == EntityResult.SeenDirectly)
				{
					Entity entity = (Entity)data;
					if (!entityIntelligence.Memory.NeedsItemForSwitchedGoal(entity.ID))
					{
						base.entity.AgentStorage.Uncontain(entity);
					}
				}
				if (data != null && data.AssignedToJob == job.ID)
				{
					data.AssignedToJob = null;
				}
				if (job is HaulingJobAnyItemOfType haulingJobAnyItemOfType && job.Item.HasValue)
				{
					haulingJobAnyItemOfType.AddLog(job.Item.Value.ToString() + " was set to null by " + base.entity.ToString());
					job.Item = null;
				}
			}
			FreeUpVehicle();
			RemoveLocksFromJob(job);
			isCancelled = true;
		}
		CancelNestedGoals();
	}

	public bool GoalHasJob(Job job)
	{
		if (this.job == job)
		{
			return true;
		}
		foreach (Goal subgoal in Subgoals)
		{
			if (subgoal is GoalHaul goalHaul && goalHaul.GoalHasJob(job))
			{
				return true;
			}
		}
		foreach (GoalHaul addedNestedGoal in addedNestedGoals)
		{
			if (addedNestedGoal != null && addedNestedGoal.GoalHasJob(job))
			{
				return true;
			}
		}
		foreach (GoalHaul item in goalsToNest)
		{
			if (item != null && item.GoalHasJob(job))
			{
				return true;
			}
		}
		return false;
	}

	public override void Deactivate()
	{
		AbandonJob();
		if (job != null && IsOuterGoal)
		{
			ResetThreatStance(job);
		}
	}

	public override bool RequiresBoldStance()
	{
		return job.RequiresBoldStance;
	}

	private void CancelNestedGoals()
	{
		if (goalsToNest != null && goalsToNest.Count > 0)
		{
			foreach (GoalHaul item in goalsToNest)
			{
				item.AbandonJob();
				item.RemoveIDEntry();
			}
			goalsToNest.Clear();
		}
		if (addedNestedGoals == null)
		{
			return;
		}
		foreach (GoalHaul addedNestedGoal in addedNestedGoals)
		{
			if (!addedNestedGoal.isActivated)
			{
				addedNestedGoal.AbandonJob();
			}
			addedNestedGoal.RemoveIDEntry();
		}
		addedNestedGoals.Clear();
	}

	private void FreeUpVehicle()
	{
		if (vehicleID.HasValue && IsOuterGoal)
		{
			entityIntelligence.GetKnownData(vehicleID.Value, out var data);
			if (data != null)
			{
				entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
			}
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message) || message.MessageType == Message.MessageTypes.CancelJobOrItemInUse || message.MessageType == Message.MessageTypes.CancelJobForAIReset)
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.CancelJobOrItemInUse:
			case Message.MessageTypes.CancelJobForAIReset:
				base.Status = Status.Failed;
				return true;
			case Message.MessageTypes.DistanceFound:
			case Message.MessageTypes.DistanceFoundNoAccess:
				if (isFindingExtraJobs)
				{
					isFindingExtraJobs = false;
					if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out var _)))
					{
						return true;
					}
					IKnownEntityData data2 = null;
					if (vehicleID.HasValue && EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out data2)))
					{
						return true;
					}
					RemoveAllSubgoals();
					return true;
				}
				return false;
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
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		NewOwner = sn.DoEnumNullable(NewOwner);
		ownerOfJobsID = sn.DoEnum(ownerOfJobsID);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		if (goalsToNest != null)
		{
			if (snapshotGoalsToNest == null)
			{
				snapshotGoalsToNest = new Queue<GoalID>();
			}
			foreach (GoalHaul item in goalsToNest)
			{
				if (item.ID != GoalID.Invalid)
				{
					snapshotGoalsToNest.Enqueue(item.ID);
				}
			}
		}
		snapshotGoalsToNest = sn.DoQueue(snapshotGoalsToNest);
		if (addedNestedGoals != null)
		{
			if (snapshotAddedNestedGoals == null)
			{
				snapshotAddedNestedGoals = new List<GoalID>();
			}
			snapshotAddedNestedGoals.Clear();
			foreach (GoalHaul addedNestedGoal in addedNestedGoals)
			{
				if (addedNestedGoal.ID != GoalID.Invalid)
				{
					snapshotAddedNestedGoals.Add(addedNestedGoal.ID);
				}
			}
		}
		snapshotAddedNestedGoals = sn.DoList(snapshotAddedNestedGoals);
		snapshotParent = sn.SnapshotID<Goal, GoalID>(ParentGoal);
		vehicleID = sn.DoEntityIDNullable(vehicleID);
		DestinationPileIsHandled = sn.DoBool(DestinationPileIsHandled);
		SourcePileIsHandled = sn.DoBool(SourcePileIsHandled);
		isFindingExtraJobs = sn.DoBool(isFindingExtraJobs);
		IsOuterGoal = sn.DoBool(IsOuterGoal);
		IsStandingBentOverItem = sn.DoBool(IsStandingBentOverItem);
		isCancelled = sn.DoBool(isCancelled);
		itemToHaul = sn.DoEntityID(itemToHaul);
		isActivated = sn.DoBool(isActivated);
		sn.Postpone(snapshotDestinationPiledItemJobs);
		sn.Ignore(job);
		sn.Ignore(goalsToNest);
		sn.Ignore(addedNestedGoals);
		sn.Ignore(DestinationPiledItemJobs);
		sn.Ignore(SourcePiledItemJobs);
		sn.Ignore(ParentGoal);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotGoalsToNest != null)
		{
			goalsToNest = new Queue<GoalHaul>(snapshotGoalsToNest.Select((GoalID g) => (GoalHaul)LookUpGoals.FindByID(g)));
		}
		snapshotGoalsToNest.Clear();
		if (snapshotAddedNestedGoals != null)
		{
			foreach (GoalID snapshotAddedNestedGoal in snapshotAddedNestedGoals)
			{
				_ = snapshotAddedNestedGoal;
				_ = uint.MaxValue;
			}
			addedNestedGoals = snapshotAddedNestedGoals.Select((GoalID g) => (GoalHaul)LookUpGoals.FindByID(g)).ToList();
		}
		snapshotAddedNestedGoals.Clear();
		if (snapshotParent.HasValue)
		{
			ParentGoal = (GoalHaul)LookUpGoals.FindByID(snapshotParent.Value);
		}
		if (snapshotJob.HasValue)
		{
			job = (HaulingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
