using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

internal class GoalUnload : CompositeGoal
{
	private EntityID itemToUnload;

	private EntityID containerToUnload;

	private Job assignToJob;

	private JobID? snapshotJob;

	private List<PassengerOrCargoSlot> slots;

	private List<PassengerOrCargoSlotID> snapshotSlots;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalUnload(Entity owner, EntityID container, EntityID item)
		: base(owner)
	{
		itemToUnload = item;
		containerToUnload = container;
	}

	public GoalUnload(Entity owner, EntityID container, EntityID item, Job assignToJob)
		: base(owner)
	{
		itemToUnload = item;
		containerToUnload = container;
		this.assignToJob = assignToJob;
	}

	public GoalUnload()
	{
	}

	protected override void Activate()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(itemToUnload, out var data)) || EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(containerToUnload, out var data2)))
		{
			return;
		}
		if (data2.ContainsEntity(itemToUnload) && data.Bulk <= entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored)
		{
			base.Status = Status.Active;
			if (entity.DrivingVehicle.HasValue)
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
			}
			else if (entity.PassengerInVehicle.HasValue)
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.PassengerInVehicle.Value));
			}
			slots = data2.GetCargoSlotsForUnloading(data.Bulk);
			Vector3 transformedEntry;
			Vector3 transformedPointToFace;
			if (slots != null)
			{
				PassengerOrCargoSlot passengerOrCargoSlot = slots[0];
				passengerOrCargoSlot.CargoSlot.TargetedByHauler = entity;
				passengerOrCargoSlot.GetEntryPoints(out transformedEntry, out transformedPointToFace);
			}
			else
			{
				transformedEntry = data2.AccessPoint.Value;
				transformedPointToFace = data2.Location.Value;
			}
			AddSubgoal(new GoalMoveToPosition(entity, transformedEntry, null, GoalMoveToPosition.VehicleUse.NoVehicle));
			if (transformedPointToFace != transformedEntry)
			{
				AddSubgoal(new GoalTurnToFace(entity, transformedPointToFace.ToVector2()));
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status != Status.Completed)
		{
			return;
		}
		Entity entity;
		if (!entityIntelligence.GetEntitySeenDirectly(itemToUnload, out var item))
		{
			base.Status = Status.Failed;
		}
		else if (!entityIntelligence.GetEntitySeenDirectly(containerToUnload, out entity))
		{
			base.Status = Status.Failed;
		}
		else if (entity.Contains.Uncontain(item, destroy: false, shouldQueue: false, null, null, null, null, slots, base.entity.Location))
		{
			base.Status = Status.Completed;
			if (assignToJob != null && assignToJob is ProcessJob)
			{
				((ProcessJob)assignToJob).AssignInput(item);
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} {itemToUnload}";
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
		snapshotJob = sn.SnapshotID<Job, JobID>(assignToJob);
		containerToUnload = sn.DoEntityID(containerToUnload);
		itemToUnload = sn.DoEntityID(itemToUnload);
		if (slots != null)
		{
			snapshotSlots = slots.Select((PassengerOrCargoSlot s) => s.ID).ToList();
		}
		snapshotSlots = sn.DoList(snapshotSlots);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		assignToJob = LookUp<Job, JobID>.FindByID(snapshotJob);
		if (snapshotSlots != null)
		{
			slots = snapshotSlots.Select((PassengerOrCargoSlotID s) => LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(s)).ToList();
			snapshotSlots.Clear();
		}
	}
}
