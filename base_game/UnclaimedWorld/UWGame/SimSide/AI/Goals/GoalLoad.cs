using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

internal class GoalLoad : CompositeGoal
{
	private EntityID itemToLoad;

	private EntityID vehicleToLoad;

	public OwnerID? NewOwner;

	private List<PassengerOrCargoSlot> slots;

	private List<PassengerOrCargoSlotID> snapshotSlots;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalLoad(Entity entity, EntityID vehicle, EntityID item, OwnerID? newOwner)
		: base(entity)
	{
		itemToLoad = item;
		vehicleToLoad = vehicle;
		NewOwner = newOwner;
	}

	public GoalLoad()
	{
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		StorageCompartment compartment = StorageCompartment.Haul;
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToLoad, out var data)) || EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleToLoad, out var data2)))
		{
			return;
		}
		bool flag = entity.AgentStorage.Contains(itemToLoad);
		if (flag || data.Bulk <= entity.AgentStorage.GetCompartment(compartment).UnusedCapacity)
		{
			if (entity.DrivingVehicle.HasValue)
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
			}
			else if (entity.PassengerInVehicle.HasValue)
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.PassengerInVehicle.Value));
			}
			if (!flag)
			{
				_ = data.PlaySiteLocation;
				AddSubgoal(new GoalMoveToPosition(entity, null, data, GoalMoveToPosition.VehicleUse.NoVehicle));
				if (!PickupItemOrUnloadFirst(data, mountAfterPickup: false, bendDown: true, standUpAfterwards: true, compartment))
				{
					return;
				}
			}
			slots = data2.GetCargoSlotsForLoading(data.Bulk);
			foreach (PassengerOrCargoSlot slot in slots)
			{
				slot.CargoSlot.TargetedByHauler = entity;
			}
			slots[0].GetEntryPoints(out var transformedEntry, out var transformedPointToFace);
			AddSubgoal(new GoalMoveToPosition(entity, transformedEntry, null, GoalMoveToPosition.VehicleUse.NoVehicle));
			AddSubgoal(new GoalTurnToFace(entity, transformedPointToFace.ToVector2()));
			base.Status = Status.Active;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Completed && !EntityIsNotSeenDirectly(itemToLoad, out var entity) && !EntityIsNotSeenDirectly(vehicleToLoad, out var placeInStorageEntity))
		{
			if (base.entity.AgentStorage.Uncontain(entity, destroy: false, shouldQueue: false, null, placeInStorageEntity, null, null, slots))
			{
				IOwner newOwner = LookUpOwners.FindByID(NewOwner);
				entity.ChangeOwnership(newOwner);
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Failed;
			}
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
		itemToLoad = sn.DoEntityID(itemToLoad);
		vehicleToLoad = sn.DoEntityID(vehicleToLoad);
		NewOwner = sn.DoEnumNullable(NewOwner);
		if (slots != null)
		{
			snapshotSlots = slots.Select((PassengerOrCargoSlot s) => s.ID).ToList();
		}
		snapshotSlots = sn.DoList(snapshotSlots);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotSlots != null)
		{
			slots = snapshotSlots.Select((PassengerOrCargoSlotID s) => LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(s)).ToList();
		}
		snapshotSlots.Clear();
	}
}
