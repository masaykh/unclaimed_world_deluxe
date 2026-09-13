using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Jobs;

public class HuntingJobManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		CleanupJobs,
		RebalanceStandingOrderJobs,
		UpdateDirectOrderJobs,
		RemoveExcessiveStandingOrderJobs,
		UpdateStandingOrderJobs
	}

	private Phase phase;

	private EntityGroup owner;

	private EntityGroupID snapshotOwnerID;

	private static double totalComputationAllInstancesInSeconds;

	private Regulator regulator;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double? UpdateInterval => 4.0;

	public bool IsPaused { get; set; }

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public bool UnregisterBeforeSnapshot => true;

	public CyclableID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public HuntingJobManager(EntityGroup owner)
	{
		this.owner = owner;
		AddToLookup();
		CreateRegulators();
	}

	public HuntingJobManager()
	{
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "HuntingJobManager");
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		RemoveIDEntry();
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"HuntingJobManager {ID}:");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.CleanupJobs:
			CleanupJobs();
			phase = Phase.UpdateDirectOrderJobs;
			break;
		case Phase.UpdateDirectOrderJobs:
			UpdateDirectOrderJobs();
			phase = Phase.UpdateStandingOrderJobs;
			break;
		case Phase.UpdateStandingOrderJobs:
			UpdateStandingOrderJobs();
			return true;
		}
		return false;
	}

	private void CleanupJobs()
	{
		List<Zone> list = null;
		foreach (Zone zone in owner.Zones)
		{
			List<EntityType> list2 = null;
			foreach (KeyValuePair<EntityType, double> item in zone.ZoneHunt.TimePointForNextHunt)
			{
				if (The.Sim.TimepointReached(item.Value))
				{
					Common.AddToList(ref list2, item.Key);
				}
			}
			if (list2 != null)
			{
				foreach (EntityType item2 in list2)
				{
					zone.ZoneHunt.ResetTimepoint(item2);
				}
			}
			if (!zone.ZoneHunt.HasFindPreyJobs())
			{
				continue;
			}
			if (!zone.ZoneHunt.HasHuntOrders())
			{
				Common.AddToList(ref list, zone);
			}
			else if (!zone.ZoneHunt.HasOrdersNotOnCooldown())
			{
				Common.AddToList(ref list, zone);
			}
			else
			{
				if (!zone.ZoneHunt.AllowsStandingOrders() || zone.ZoneHunt.HasDirectOrders())
				{
					continue;
				}
				bool flag = false;
				foreach (EntityType item3 in zone.ZoneHunt.AllowStandingOrderHunt)
				{
					EntityType carcassType = item3.BiologicalType.CarcassType;
					if (owner.StandingOrderJobIsNeeded(carcassType))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Common.AddToList(ref list, zone);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (Zone item4 in list)
		{
			item4.ZoneHunt.CancelJobs();
		}
	}

	private void UpdateStandingOrderJobs()
	{
		if (owner.ProductionOrders == null)
		{
			return;
		}
		EntityType representativeEntityType = owner.GetAllegiance().RepresentativeEntityType;
		if (representativeEntityType.IntelligenceType.PreyTypes == null)
		{
			return;
		}
		foreach (EntityType preyType in representativeEntityType.IntelligenceType.PreyTypes)
		{
			EntityType carcassType = preyType.BiologicalType.CarcassType;
			EntityType entityType = preyType;
			ProductionOrder productionOrder = owner.ProductionOrders.Orders[carcassType];
			int num = owner.CountAvailableItems(carcassType);
			int? num2 = productionOrder.AmountToKeepInStore - num;
			int num3 = 0;
			if (num2.GetValueOrDefault() <= num3 || !num2.HasValue || HuntingJobsExistForCreature(entityType))
			{
				continue;
			}
			Zone zone = FindZoneToHuntCreature(entityType);
			if (zone != null)
			{
				for (int i = 0; i < zone.ZoneHunt.MaxHuntJobs; i++)
				{
					new FindPreyJob(zone, owner);
				}
			}
		}
	}

	private Zone FindZoneToHuntCreature(EntityType creatureType)
	{
		foreach (Zone zone in owner.Zones)
		{
			if (zone.ZoneHunt.AllowStandingOrderHunt.Contains(creatureType) && !zone.ZoneHunt.TimePointForNextHunt.ContainsKey(creatureType))
			{
				return zone;
			}
		}
		return null;
	}

	private bool HuntingJobsExistForCreature(EntityType creature)
	{
		foreach (Zone zone in owner.Zones)
		{
			if (zone.ZoneHunt.HasAnyHuntOrders(creature) && zone.ZoneHunt.HasFindPreyJobs())
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateDirectOrderJobs()
	{
		foreach (Zone zone in owner.Zones)
		{
			int maxHuntJobs = zone.ZoneHunt.MaxHuntJobs;
			int count = zone.ZoneHunt.FindPreyJobs.Count;
			int num = maxHuntJobs - count;
			if (num <= 0 || !zone.ZoneHunt.HasOrdersNotOnCooldown() || zone.ZoneHunt.CreaturesToHunt == null)
			{
				continue;
			}
			bool flag = false;
			foreach (KeyValuePair<EntityType, int> item in zone.ZoneHunt.CreaturesToHunt)
			{
				if (item.Value > 0 && !zone.ZoneHunt.HuntIsOnCooldown(item.Key))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				for (int i = 0; i < num; i++)
				{
					new FindPreyJob(zone, owner);
				}
			}
		}
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
				phase = Phase.CleanupJobs;
			}
		}
	}

	private void RemoveUnneededStandingOrderJobs()
	{
		if (owner.ProductionOrders == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, ProductionOrder> order in owner.ProductionOrders.Orders)
		{
			EntityType key = order.Key;
			ProductionOrder value = order.Value;
			if (key.ItemType.CarcassType != null && value.AmountToKeepInStore.HasValue)
			{
				RemoveUnneededStandingOrderJobs(key, value);
			}
		}
	}

	private void RemoveUnneededStandingOrderJobs(EntityType entityType, ProductionOrder order)
	{
		GetAmountToProduce(entityType, order, out var amountToProduce);
		if (amountToProduce < 0)
		{
			int noOfJobsToRemove = Math.Abs(amountToProduce);
			DestroyJobsIntelligently(owner.FindPreyJobs, noOfJobsToRemove);
		}
	}

	private void GetAmountToProduce(EntityType entityType, ProductionOrder order, out int amountToProduce)
	{
		int itemsInStock = owner.CountAvailableItems(entityType);
		int count = owner.FindPreyJobs.Count;
		amountToProduce = JobManager.GetAmountToProduce(order, itemsInStock, count);
	}

	private void UpdateStandingOrder(EntityType entityType, ProductionOrder order, int maxJobs)
	{
	}

	private static void DestroyJobsIntelligently(List<Job> jobs, int noOfJobsToRemove)
	{
		if (jobs == null)
		{
			return;
		}
		int num = 0;
		for (int num2 = jobs.Count - 1; num2 >= 0; num2--)
		{
			Job job = jobs[num2];
			if (job.TakenBy.Count == 0)
			{
				job.Destroy(cancelTakers: true);
				jobs.Remove(job);
				num++;
			}
			if (num == noOfJobsToRemove)
			{
				return;
			}
		}
		for (int num3 = jobs.Count - 1; num3 >= 0; num3--)
		{
			Job job = jobs[num3];
			if (JobManager.AllTakersMinimumDistance(job) > 60f)
			{
				job.Destroy(cancelTakers: true);
				jobs.Remove(job);
				num++;
			}
			if (num == noOfJobsToRemove)
			{
				return;
			}
		}
		for (int num4 = jobs.Count - 1; num4 >= 0; num4--)
		{
			Job job = jobs[num4];
			job.Destroy(cancelTakers: true);
			jobs.Remove(job);
			num++;
			if (num == noOfJobsToRemove)
			{
				return;
			}
		}
		for (int num5 = jobs.Count - 1; num5 >= 0; num5--)
		{
			Job job = jobs[num5];
			job.Destroy(cancelTakers: true);
			jobs.Remove(job);
			num++;
			if (num == noOfJobsToRemove)
			{
				break;
			}
		}
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		ID = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		phase = sn.DoEnum(phase);
		IsPaused = sn.DoBool(IsPaused);
		snapshotOwnerID = sn.SnapshotID<EntityGroup, EntityGroupID>(owner).Value;
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(regulator);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		owner = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerID);
		CreateRegulators();
	}
}
