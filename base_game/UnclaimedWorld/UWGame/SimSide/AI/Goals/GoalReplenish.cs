using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalReplenish : CompositeGoal
{
	public enum ReplenishAction
	{
		Reload,
		Refuel,
		Recharge
	}

	private List<EntityID> replenishItems;

	private EntityAndRoot entityToReplenish;

	private StorageCompartment compartmentToUse;

	private Job job;

	private JobID snapshotJob;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalReplenish(Entity entity, EntityAndRoot entityToReplenish, List<EntityID> replenishItems, ProcessType processType, List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment)
		: base(entity)
	{
		this.entityToReplenish = entityToReplenish;
		this.replenishItems = replenishItems;
		base.ownersOfVehicles = ownersOfVehicles;
		compartmentToUse = compartment;
		this.job = job;
	}

	public GoalReplenish()
	{
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		base.Status = Status.Active;
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToReplenish.Entity, out var data)))
		{
			return;
		}
		List<IKnownEntityData> list = new List<IKnownEntityData>();
		float num = 0f;
		float num2 = entity.AgentStorage.ItemStorage.UnusedCapacity;
		foreach (EntityID replenishItem in replenishItems)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(replenishItem, out var data2)))
			{
				return;
			}
			list.Add(data2);
			if (entity.AgentStorage.Contains(data2.EntityID))
			{
				num2 += data2.Bulk;
			}
			num += data2.Bulk;
		}
		if (Common.IsGreaterThan(entity.AgentStorage.ItemStorage.TotalStored + num, entity.AgentStorage.ItemStorage.TotalCapacity))
		{
			DropUnneededItemsToMakeCapacity(num, (Entity e) => e.AssignedToJob != job.ID, out var totalDroppedItems, compartmentToUse);
			num2 += totalDroppedItems;
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			IKnownEntityData knownEntityData = list[num3];
			if (entity.AgentStorage.Contains(knownEntityData.EntityID) && num3 > 0)
			{
				list.RemoveAt(num3);
				list.Insert(0, knownEntityData);
			}
		}
		float num4 = 0f;
		List<IKnownEntityData> list2 = new List<IKnownEntityData>();
		foreach (IKnownEntityData item in list)
		{
			if (!entity.AgentStorage.Contains(item.EntityID))
			{
				if (num4 + item.Bulk > num2)
				{
					AddSubgoal(new GoalMoveToPosition(entity, data.AccessPoint.Value, ownersOfVehicles)
					{
						IsFinalDestination = false
					});
					foreach (IKnownEntityData item2 in list2)
					{
						AddSubgoal(new GoalDropItem(entity, item2.EntityID, null, data.AccessPoint, null));
					}
					list2.Clear();
					num4 = 0f;
				}
				num4 += item.Bulk;
				list2.Add(item);
				AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, item)
				{
					IsFinalDestination = false
				});
				entity.Intelligence.Allegiance.SharedKnowledge.SetInUseBy(item.EntityID, entity.EntityID);
				if (!PickupItemOrUnloadFirst(item, mountAfterPickup: false, bendDown: true, standUpAfterwards: true, compartmentToUse))
				{
					return;
				}
			}
			else
			{
				num4 += item.Bulk;
				list2.Add(item);
			}
		}
		if (!entity.AgentStorage.Contains(entityToReplenish.Entity))
		{
			AddSubgoal(new GoalMoveToPosition(entity, data.AccessPoint.Value, ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			AddSubgoal(new GoalTurnToFace(entity, data.Location.Value.ToVector2()));
		}
		foreach (IKnownEntityData item3 in list)
		{
			AddSubgoal(new GoalDoProduce(entity, data.EntityType.ContainerType.GetReplenishProcesses()[item3.EntityType], data.OwnedBy, item3, compartmentToUse, data.AccessPoint, entityToReplenish));
		}
	}

	public static void Replenish(Entity worker, ReplenishAction action, Entity entityToReplenishWith, Entity entityToReplenish, IOwner entityToReplenishOwner, StorageCompartment compartment)
	{
		switch (action)
		{
		case ReplenishAction.Refuel:
		{
			Container contains = entityToReplenish.Contains;
			contains.AddToContain(entityToReplenishWith, null, null, ignoreCapacity: false, replenish: true);
			((IHasReplenishItems)contains).ReplenishItems.RequiresFuel.Refuel(entityToReplenishWith);
			break;
		}
		case ReplenishAction.Reload:
		{
			entityToReplenish.Contains.AddToContain(entityToReplenishWith, out var surplusEntity);
			if (surplusEntity != null)
			{
				JobID? assignedToJob = entityToReplenishWith.AssignedToJob;
				ProcessJob processJob = null;
				EvaluateJob.ResolveAssignedToProcessJob(entityToReplenishWith, out processJob);
				if (worker != null && (processJob == null || processJob.ReplenishJob == null))
				{
					surplusEntity.AssignedToJob = assignedToJob;
				}
				if (worker != null && !surplusEntity.Item.Pickup(worker, entityToReplenishOwner, compartment) && surplusEntity.AssignedToJob == assignedToJob)
				{
					surplusEntity.AssignedToJob = null;
				}
			}
			break;
		}
		}
	}

	protected override bool ArePreconditionsOK()
	{
		for (int num = replenishItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = replenishItems[num];
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityID, out var _)))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
			_ = base.Status;
			_ = 3;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	public override void Deactivate()
	{
		foreach (EntityID replenishItem in replenishItems)
		{
			entityIntelligence.GetKnownData(replenishItem, out var data);
			if (data != null)
			{
				if (data.AssignedToJob == job.ID)
				{
					data.AssignedToJob = null;
				}
				entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
			}
		}
	}

	public override string GetStatus()
	{
		entityIntelligence.GetKnownData(entityToReplenish.Entity, out var data);
		EntityID entityID = replenishItems[0];
		entityIntelligence.GetKnownData(entityID, out var data2);
		if (data != null && data2 != null)
		{
			return data.EntityType.ContainerType.GetReplenishProcesses()[data2.EntityType].Name;
		}
		return "";
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
		replenishItems = sn.DoList(replenishItems);
		entityToReplenish = sn.DoEntityAndRoot(entityToReplenish);
		snapshotJob = sn.SnapshotID<Job, JobID>(job).Value;
		compartmentToUse = sn.DoEnum(compartmentToUse);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		job = LookUp<Job, JobID>.FindByID(snapshotJob);
	}
}
