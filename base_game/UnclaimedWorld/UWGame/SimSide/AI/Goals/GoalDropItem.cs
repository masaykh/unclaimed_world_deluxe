using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalDropItem : CompositeGoal
{
	private EntityID itemToDrop;

	private JobID? assignAsInputToJob;

	private JobID? haulJobID;

	private Vector3? dropAtLocation;

	private StorageTarget? placeInStorage;

	private bool dropItHere;

	private bool firstPickupHalfAnimStateWasSet;

	private bool secondPickupHalfAnimStateWasSet;

	private AnimAction? actionStateToSetInSecondHalf;

	private List<AnimModifier> statesToSetInSecondHalf = new List<AnimModifier>();

	private float durationOfSecondHalf;

	private bool isHauledItemDestination;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDropItem()
	{
	}

	public GoalDropItem(Entity owner, EntityID item, ProcessJob assignToJob = null)
		: base(owner)
	{
		Init(item, assignToJob, null);
		dropItHere = true;
	}

	public GoalDropItem(Entity owner, EntityID item, ProcessJob assignAsInputToJob, Vector3? dropAtLocation, StorageTarget? placeInStorage, bool isHauledItemDestination = false, JobID? haulJob = null)
		: base(owner)
	{
		Init(item, assignAsInputToJob, placeInStorage);
		this.dropAtLocation = dropAtLocation;
		this.isHauledItemDestination = isHauledItemDestination;
		haulJobID = haulJob;
		if (!dropAtLocation.HasValue && !placeInStorage.HasValue)
		{
			dropItHere = true;
		}
	}

	private void Init(EntityID item, ProcessJob assignToJob, StorageTarget? placeInStorage)
	{
		itemToDrop = item;
		assignAsInputToJob = assignToJob?.ID;
		this.placeInStorage = placeInStorage;
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		if (EntityIsNotSeenDirectly(itemToDrop, out var entity))
		{
			return;
		}
		if (base.entity.AgentStorage.Contains(entity))
		{
			if (!dropItHere)
			{
				if (!base.entity.GetContainedBy(out Entity container))
				{
					base.Status = Status.Failed;
					return;
				}
				if (container != null)
				{
					if (placeInStorage.HasValue)
					{
						if (container.EntityID != placeInStorage.Value.StorageEntity)
						{
							AddSubgoal(new GoalExit(base.entity));
						}
					}
					else
					{
						AddSubgoal(new GoalExit(base.entity));
					}
				}
				if (placeInStorage.HasValue)
				{
					if (container == null || container.EntityID != placeInStorage.Value.StorageEntity)
					{
						IKnownEntityData data;
						EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(placeInStorage.Value.StorageEntity, out data);
						if (EntityResultCausesFailedGoal(knownData))
						{
							return;
						}
						AddSubgoal(new GoalMoveToPosition(base.entity, data.AccessPoint.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle, placeInStorage.Value.StorageEntity));
					}
				}
				else
				{
					AddSubgoal(new GoalMoveToPosition(base.entity, dropAtLocation.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle)
					{
						PermittedDistanceSquaredToDestination = (float)Math.Pow(GameData.Instance.Constants.InteractionDistanceForAgents, 2.0)
					});
					AddSubgoal(new GoalTurnToFace(base.entity, dropAtLocation.Value.ToVector2()));
				}
			}
			else
			{
				SetFirstPartAnimationState(entity);
			}
			base.Status = Status.Active;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private void SetFirstPartAnimationState(IKnownEntityData itemToDropData)
	{
		AgentStorage.BurdenState currentBurdenState = entity.AgentStorage.GetCurrentBurdenState();
		AgentStorage.BurdenState burdenStateAfterDrop = GetBurdenStateAfterDrop(currentBurdenState, itemToDropData);
		bool scaleAnimationToFillWaitPeriod = false;
		bool headTurnAllowed = false;
		IntelligenceType intelligenceType = entity.EntityType.IntelligenceType;
		switch (currentBurdenState)
		{
		case AgentStorage.BurdenState.Mounted:
			if (burdenStateAfterDrop == AgentStorage.BurdenState.Mounted)
			{
				float dropHeavyActionPointDuration = intelligenceType.dropMountToMountActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Mount, AnimModifier.Same, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.dropMountToMountDuration - intelligenceType.dropMountToMountActionPointDuration;
			}
			else
			{
				float dropHeavyActionPointDuration = intelligenceType.dropMountedActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Mount, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.dropMountedDuration - intelligenceType.dropMountedActionPointDuration;
			}
			break;
		case AgentStorage.BurdenState.None:
		case AgentStorage.BurdenState.Equipped:
		{
			float dropHeavyActionPointDuration = intelligenceType.dropEquippedActionPointDuration;
			AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Equip, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
			{
				HeadTurnAllowed = headTurnAllowed
			});
			durationOfSecondHalf = intelligenceType.dropEquippedDuration - intelligenceType.dropEquippedActionPointDuration;
			break;
		}
		case AgentStorage.BurdenState.HaulLight:
			switch (burdenStateAfterDrop)
			{
			case AgentStorage.BurdenState.None:
			case AgentStorage.BurdenState.Equipped:
			{
				float dropHeavyActionPointDuration = intelligenceType.DropLightActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.DropLightDuration - intelligenceType.DropLightActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.HaulLight:
			{
				float dropHeavyActionPointDuration = intelligenceType.DropLightToLightActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Same, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.DropLightToLightDuration - intelligenceType.DropLightToLightActionPointDuration;
				break;
			}
			}
			break;
		case AgentStorage.BurdenState.HaulHeavy:
			switch (burdenStateAfterDrop)
			{
			case AgentStorage.BurdenState.None:
			case AgentStorage.BurdenState.Equipped:
			{
				float dropHeavyActionPointDuration = intelligenceType.DropHeavyActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.DropHeavyDuration - intelligenceType.DropHeavyActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.HaulLight:
			{
				float dropHeavyActionPointDuration = intelligenceType.DropHeavyActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimationToFillWaitPeriod)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupLightDuration - intelligenceType.PickupLightActionPointDuration;
				actionStateToSetInSecondHalf = AnimAction.PickingUp;
				statesToSetInSecondHalf.Add(AnimModifier.Post);
				break;
			}
			case AgentStorage.BurdenState.HaulHeavy:
			{
				float dropHeavyActionPointDuration = intelligenceType.DropHeavyActionPointDuration;
				AddSubgoal(new GoalWait(entity, dropHeavyActionPointDuration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimationToFillWaitPeriod)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupHeavyDuration - intelligenceType.PickupHeavyActionPointDuration;
				actionStateToSetInSecondHalf = AnimAction.PickingUp;
				statesToSetInSecondHalf.Add(AnimModifier.Heavy);
				statesToSetInSecondHalf.Add(AnimModifier.Post);
				break;
			}
			}
			break;
		}
		firstPickupHalfAnimStateWasSet = true;
	}

	private AgentStorage.BurdenState GetBurdenStateAfterDrop(AgentStorage.BurdenState currentState, IKnownEntityData itemToDropData)
	{
		if (entity.AgentStorage.MountedToolOrWeapon == itemToDrop)
		{
			return AgentStorage.BurdenState.None;
		}
		switch (currentState)
		{
		case AgentStorage.BurdenState.HaulHeavy:
			return GetHaulBurdenStateAfterDrop(entity, itemToDropData);
		case AgentStorage.BurdenState.HaulLight:
			return GetHaulBurdenStateAfterDrop(entity, itemToDropData);
		default:
			if (entity.AgentStorage.MountedToolOrWeapon.HasValue)
			{
				return AgentStorage.BurdenState.Mounted;
			}
			return AgentStorage.BurdenState.None;
		}
	}

	public static AgentStorage.BurdenState GetHaulBurdenStateAfterDrop(Entity entity, IKnownEntityData itemToDropData)
	{
		if (entity.AgentStorage.RecomputeIsHauling(itemToDropData))
		{
			return AgentStorage.GetHaulBurdenState(entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored - itemToDropData.Bulk));
		}
		return AgentStorage.BurdenState.None;
	}

	public static AgentStorage.BurdenState GetHaulBurdenStateAfterPickup(Entity entity, IKnownEntityData itemToPickupData)
	{
		if (entity.AgentStorage.RecomputeIsHauling(null, itemToPickupData))
		{
			return AgentStorage.GetHaulBurdenState(entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored + itemToPickupData.Bulk));
		}
		return AgentStorage.BurdenState.None;
	}

	private void SetSecondPartAnimationState(IKnownEntityData itemToPickUpData)
	{
		bool headTurnAllowed = false;
		if (statesToSetInSecondHalf.Count > 0)
		{
			AddSubgoal(new GoalWait(entity, durationOfSecondHalf, actionStateToSetInSecondHalf, statesToSetInSecondHalf)
			{
				HeadTurnAllowed = headTurnAllowed
			});
		}
		else if (durationOfSecondHalf > 0f)
		{
			AddSubgoal(new GoalWait(entity, durationOfSecondHalf)
			{
				HeadTurnAllowed = headTurnAllowed
			});
		}
		secondPickupHalfAnimStateWasSet = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.PickingUp);
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Dropping);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Pre);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Post);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Heavy);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Mount);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Equip);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Same);
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status != Status.Completed || EntityIsNotSeenDirectly(itemToDrop, out var entity))
		{
			return;
		}
		Entity entity2 = null;
		if (placeInStorage.HasValue)
		{
			if (EntityIsNotSeenDirectly(placeInStorage.Value.StorageEntity, out entity2))
			{
				return;
			}
			((IStorage)entity2.Contains).FindStorage(placeInStorage.Value.StorageID);
		}
		if (!firstPickupHalfAnimStateWasSet)
		{
			SetFirstPartAnimationState(entity);
			base.Status = Status.Active;
		}
		else if (!secondPickupHalfAnimStateWasSet)
		{
			if (dropAtLocation.HasValue && Common.DistanceOctile(base.entity.PlaySiteLocation, dropAtLocation.Value) < GameData.Instance.Constants.InteractionDistanceForAgents)
			{
				_ = dropAtLocation.Value;
			}
			if (base.entity.Contains.Uncontain(entity, destroy: false, shouldQueue: false, placeInStorage, entity2, null, null, null, dropAtLocation))
			{
				if (assignAsInputToJob.HasValue)
				{
					Job job = LookUp<Job, JobID>.FindByID(assignAsInputToJob);
					if (job != null)
					{
						((ProcessJob)job).AssignInput(entity);
					}
				}
				if (haulJobID.HasValue)
				{
					Job job2 = LookUp<Job, JobID>.FindByID(haulJobID);
					if (job2 != null)
					{
						((HaulingJob)job2).IsCompleted = true;
					}
				}
				base.Status = Status.Active;
				if (isHauledItemDestination)
				{
					entityIntelligence.Memory.ResetHauledItem(entity.EntityID);
				}
				SetSecondPartAnimationState(entity);
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
		else
		{
			base.Status = Status.Completed;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} {itemToDrop}";
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
		itemToDrop = sn.DoEntityID(itemToDrop);
		assignAsInputToJob = sn.DoEnumNullable(assignAsInputToJob);
		dropAtLocation = sn.DoVector3Nullable(dropAtLocation);
		placeInStorage = sn.DoStorageTargetNullable(placeInStorage);
		dropItHere = sn.DoBool(dropItHere);
		firstPickupHalfAnimStateWasSet = sn.DoBool(firstPickupHalfAnimStateWasSet);
		secondPickupHalfAnimStateWasSet = sn.DoBool(secondPickupHalfAnimStateWasSet);
		actionStateToSetInSecondHalf = sn.DoEnumNullable(actionStateToSetInSecondHalf);
		statesToSetInSecondHalf = sn.DoList(statesToSetInSecondHalf);
		durationOfSecondHalf = sn.DoFloat(durationOfSecondHalf);
		isHauledItemDestination = sn.DoBool(isHauledItemDestination);
		haulJobID = sn.DoEnumNullable(haulJobID);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
	}
}
