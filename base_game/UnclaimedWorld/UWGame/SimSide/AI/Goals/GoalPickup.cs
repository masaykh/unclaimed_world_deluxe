using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalPickup : CompositeGoal
{
	private EntityID itemToPickUp;

	public OwnerID? NewOwner;

	private StorageCompartment placeInCompartment;

	private bool mountItemAfterPickup;

	private bool bendDown;

	private bool standUpAfterwards;

	private bool firstPickupHalfAnimStateWasSet;

	private bool secondPickupHalfAnimStateWasSet;

	private AnimAction? actionStateToSetInSecondHalf;

	private List<AnimModifier> modifierStatesToSetInSecondHalf = new List<AnimModifier>();

	private float durationOfSecondHalf;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalPickup(Entity owner, EntityID item, OwnerID? newOwner, StorageCompartment? placeInCompartment = StorageCompartment.Haul, bool? mountItemAfterPickup = false, bool bendDown = true, bool standUpAfterwards = true)
		: base(owner)
	{
		itemToPickUp = item;
		NewOwner = newOwner;
		this.placeInCompartment = placeInCompartment.Value;
		this.mountItemAfterPickup = mountItemAfterPickup.Value;
		this.bendDown = bendDown;
		this.standUpAfterwards = standUpAfterwards;
	}

	public GoalPickup()
	{
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToPickUp, out var data)))
		{
			return;
		}
		if (entity.AgentStorage != null && entity.AgentStorage.Contains(itemToPickUp))
		{
			base.Status = Status.Active;
		}
		else if (entity.AgentStorage != null && entity.AgentStorage.GetCompartment(placeInCompartment).HasCapacityForItem(data.Bulk))
		{
			if (entity.GetContainedBy(out Entity container))
			{
				if (container != null)
				{
					if (data.ContainedBy.HasValue)
					{
						if (container.EntityID != data.ContainedBy.Value)
						{
							AddSubgoal(new GoalExit(entity));
						}
					}
					else
					{
						AddSubgoal(new GoalExit(entity));
					}
				}
				if (data.ContainedBy.HasValue)
				{
					IKnownEntityData data2;
					EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(data.ContainedBy.Value, out data2);
					if (!data2.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
					{
						base.Status = Status.Failed;
						return;
					}
					if (!EntityResultCausesFailedGoal(knownData))
					{
						if (container == null || container.EntityID != data.ContainedBy.Value)
						{
							AddSubgoal(new GoalMoveToPosition(entity, data2.AccessPoint.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle, data2.EntityID));
						}
						else if (bendDown)
						{
							SetFirstPartAnimationState(data);
						}
					}
				}
				else
				{
					float num = (float)Math.Pow(GameData.Instance.Constants.InteractionDistanceForAgents, 2.0);
					if (Vector3.DistanceSquared(entity.PlaySiteLocation, data.PlaySiteLocation) > num)
					{
						AddSubgoal(new GoalMoveToPosition(entity, data.PlaySiteLocation, null, GoalMoveToPosition.VehicleUse.NoVehicle)
						{
							PermittedDistanceSquaredToDestination = num
						});
						AddSubgoal(new GoalTurnToFace(entity, data.PlaySiteLocation.ToVector2()));
					}
					else if (bendDown)
					{
						SetFirstPartAnimationState(data);
					}
				}
				base.Status = Status.Active;
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override bool ArePreconditionsOK()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToPickUp, out var data)))
		{
			return false;
		}
		if (!data.CanBeHauled())
		{
			return false;
		}
		return true;
	}

	private void SetFirstPartAnimationState(IKnownEntityData itemToPickUpData)
	{
		AgentStorage.BurdenState currentBurdenState = entity.AgentStorage.GetCurrentBurdenState();
		AgentStorage.BurdenState burdenStateAfterPickup = GetBurdenStateAfterPickup(currentBurdenState, itemToPickUpData);
		bool scaleAnimationToFillWaitPeriod = false;
		bool headTurnAllowed = false;
		IntelligenceType intelligenceType = entity.EntityType.IntelligenceType;
		switch (currentBurdenState)
		{
		case AgentStorage.BurdenState.None:
		case AgentStorage.BurdenState.Equipped:
			switch (burdenStateAfterPickup)
			{
			case AgentStorage.BurdenState.HaulLight:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.PickupLightActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupLightDuration - intelligenceType.PickupLightActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.HaulHeavy:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.PickupHeavyActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, AnimModifier.Heavy, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupHeavyDuration - intelligenceType.PickupHeavyActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.Mounted:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.pickupMountActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, AnimModifier.Mount, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.pickupMountDuration - intelligenceType.pickupMountActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.None:
			case AgentStorage.BurdenState.Equipped:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.pickupEquipActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, AnimModifier.Equip, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.pickupEquipDuration - intelligenceType.pickupEquipActionPointDuration;
				break;
			}
			}
			break;
		case AgentStorage.BurdenState.HaulLight:
			switch (burdenStateAfterPickup)
			{
			case AgentStorage.BurdenState.HaulLight:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.SwitchLightToLightActionPointDuration;
				scaleAnimationToFillWaitPeriod = true;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, AnimModifier.Same, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.SwitchLightToLightDuration - intelligenceType.SwitchLightToLightActionPointDuration;
				break;
			}
			case AgentStorage.BurdenState.HaulHeavy:
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.DropLightActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.Dropping, scaleAnimationToFillWaitPeriod)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupHeavyDuration - intelligenceType.PickupHeavyActionPointDuration;
				actionStateToSetInSecondHalf = AnimAction.PickingUp;
				modifierStatesToSetInSecondHalf.Add(AnimModifier.Heavy);
				modifierStatesToSetInSecondHalf.Add(AnimModifier.Post);
				break;
			}
			}
			break;
		case AgentStorage.BurdenState.HaulHeavy:
			if (burdenStateAfterPickup == AgentStorage.BurdenState.HaulHeavy)
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.DropHeavyActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.Dropping, AnimModifier.Heavy, scaleAnimationToFillWaitPeriod)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.PickupHeavyDuration - intelligenceType.PickupHeavyActionPointDuration;
				actionStateToSetInSecondHalf = AnimAction.PickingUp;
				modifierStatesToSetInSecondHalf.Add(AnimModifier.Heavy);
				modifierStatesToSetInSecondHalf.Add(AnimModifier.Post);
			}
			break;
		case AgentStorage.BurdenState.Mounted:
			if ((uint)burdenStateAfterPickup <= 2u)
			{
				float pickupMountedEquippedActionPointDuration = intelligenceType.pickupMountedEquippedActionPointDuration;
				AddSubgoal(new GoalWait(entity, pickupMountedEquippedActionPointDuration, AnimAction.PickingUp, AnimModifier.Mount, AnimModifier.Equip, scaleAnimationToFillWaitPeriod, GoalWait.OnExitFlagAction.Leave)
				{
					HeadTurnAllowed = headTurnAllowed
				});
				durationOfSecondHalf = intelligenceType.pickupMountedEquippedDuration - intelligenceType.pickupMountedEquippedActionPointDuration;
			}
			break;
		}
		firstPickupHalfAnimStateWasSet = true;
	}

	private void SetSecondPartAnimationState(IKnownEntityData itemToPickUpData)
	{
		if (actionStateToSetInSecondHalf.HasValue || modifierStatesToSetInSecondHalf.Count > 0)
		{
			AddSubgoal(new GoalWait(entity, durationOfSecondHalf, actionStateToSetInSecondHalf, modifierStatesToSetInSecondHalf));
		}
		else if (durationOfSecondHalf > 0f)
		{
			AddSubgoal(new GoalWait(entity, durationOfSecondHalf));
		}
		secondPickupHalfAnimStateWasSet = true;
	}

	private AgentStorage.BurdenState GetBurdenStateAfterPickup(AgentStorage.BurdenState currentState, IKnownEntityData itemToPickUpData)
	{
		if (mountItemAfterPickup)
		{
			return AgentStorage.BurdenState.Mounted;
		}
		switch (currentState)
		{
		case AgentStorage.BurdenState.HaulHeavy:
			return AgentStorage.BurdenState.HaulHeavy;
		case AgentStorage.BurdenState.HaulLight:
			return GoalDropItem.GetHaulBurdenStateAfterPickup(entity, itemToPickUpData);
		default:
		{
			AgentStorage.BurdenState haulBurdenStateAfterPickup = GoalDropItem.GetHaulBurdenStateAfterPickup(entity, itemToPickUpData);
			if (haulBurdenStateAfterPickup == AgentStorage.BurdenState.None)
			{
				if (placeInCompartment == StorageCompartment.Equipment)
				{
					return AgentStorage.BurdenState.Equipped;
				}
				if (currentState == AgentStorage.BurdenState.Equipped)
				{
					return AgentStorage.BurdenState.Equipped;
				}
				return AgentStorage.BurdenState.None;
			}
			return haulBurdenStateAfterPickup;
		}
		}
	}

	private AgentStorage.BurdenState GetNewHaulBurdenState(IKnownEntityData itemToPickUpData)
	{
		return AgentStorage.GetHaulBurdenState(entity.AgentStorage.GetHaulingPercentageOfCapacity(entity.AgentStorage.TotalStored + itemToPickUpData.Bulk));
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
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
			if (base.Status != Status.Completed || EntityIsNotSeenDirectly(itemToPickUp, out var entity))
			{
				return;
			}
			if (Common.DistanceOctile(base.entity.AccessPoint.Value, entity.AccessPoint.Value) > 30f)
			{
				base.Status = Status.Failed;
			}
			else if (!firstPickupHalfAnimStateWasSet)
			{
				if (bendDown)
				{
					SetFirstPartAnimationState(entity);
				}
				else
				{
					firstPickupHalfAnimStateWasSet = true;
				}
				base.Status = Status.Active;
			}
			else if (!secondPickupHalfAnimStateWasSet)
			{
				if (!base.entity.AgentStorage.GetCompartment(placeInCompartment).StoredItems.Contains(itemToPickUp))
				{
					IOwner newOwner = LookUpOwners.FindByID(NewOwner);
					if (!entity.Item.Pickup(base.entity, newOwner, placeInCompartment))
					{
						base.Status = Status.Failed;
						return;
					}
					if (mountItemAfterPickup)
					{
						base.entity.AgentStorage.MountedToolOrWeapon = entity.EntityID;
					}
				}
				if (standUpAfterwards)
				{
					SetSecondPartAnimationState(entity);
				}
				else
				{
					secondPickupHalfAnimStateWasSet = true;
				}
				base.entity.Renderable.UpdateAttachBoxModelToHand();
				base.Status = Status.Active;
			}
			else if (base.entity.AgentStorage.Contains(itemToPickUp))
			{
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} {itemToPickUp}";
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
		itemToPickUp = sn.DoEntityID(itemToPickUp);
		NewOwner = sn.DoEnumNullable(NewOwner);
		placeInCompartment = sn.DoEnum(placeInCompartment);
		mountItemAfterPickup = sn.DoBool(mountItemAfterPickup);
		bendDown = sn.DoBool(bendDown);
		standUpAfterwards = sn.DoBool(standUpAfterwards);
		firstPickupHalfAnimStateWasSet = sn.DoBool(firstPickupHalfAnimStateWasSet);
		secondPickupHalfAnimStateWasSet = sn.DoBool(secondPickupHalfAnimStateWasSet);
		actionStateToSetInSecondHalf = sn.DoEnumNullable(actionStateToSetInSecondHalf);
		modifierStatesToSetInSecondHalf = sn.DoList(modifierStatesToSetInSecondHalf);
		durationOfSecondHalf = sn.DoFloat(durationOfSecondHalf);
		return this;
	}
}
