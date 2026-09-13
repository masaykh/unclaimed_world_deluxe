using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Jobs;

public class JobManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		ImportJobs,
		CleanupExistingProductionJobs,
		RebalanceStandingOrderJobs,
		UpdateDirectOrderJobs,
		RemoveExcessiveStandingOrderJobs,
		ScoreImportance
	}

	private struct ProcessCombo
	{
		public ProcessType ProcessType;

		public float Score;

		public Zone Zone;

		public IResourceItem ResourceItem;
	}

	private Regulator regulator;

	private EntityGroup owner;

	private EntityGroupID snapshotOwnerID;

	private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private Phase phase;

	private static double totalComputationAllInstancesInSeconds;

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

	public bool UnregisterBeforeSnapshot => true;

	public bool IsSnapshotted { get; set; }

	public JobManager(EntityGroup owner)
	{
		this.owner = owner;
		AddToLookup();
		CreateRegulators();
	}

	public JobManager()
	{
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		RemoveIDEntry();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "JobManager");
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
				phase = Phase.ImportJobs;
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
		id = CyclableID.Invalid;
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

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"JobManager {ID}:");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.ImportJobs:
			phase = Phase.CleanupExistingProductionJobs;
			break;
		case Phase.CleanupExistingProductionJobs:
			CleanupProductionJobs();
			phase = Phase.UpdateDirectOrderJobs;
			break;
		case Phase.UpdateDirectOrderJobs:
			UpdateDirectOrderJobs();
			phase = Phase.RemoveExcessiveStandingOrderJobs;
			break;
		case Phase.RemoveExcessiveStandingOrderJobs:
			RemoveUnneededStandingOrderJobs();
			phase = Phase.RebalanceStandingOrderJobs;
			break;
		case Phase.RebalanceStandingOrderJobs:
			RebalanceStandingOrderJobs();
			phase = Phase.ScoreImportance;
			break;
		case Phase.ScoreImportance:
			ScoreImportance();
			allAvailableItems.Clear();
			phase = Phase.ImportJobs;
			return true;
		}
		return false;
	}

	private void ScoreImportance()
	{
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in owner.ProductionJobs)
		{
			EntityType key = productionJob.Key;
			if (!owner.GetAllegiance().FoodExtraction.IsEatable(key))
			{
				if (productionJob.Value.Count > 0)
				{
					owner.RecomputeImportance(key, allAvailableItems);
				}
				else
				{
					owner.SetNeutralImportance(key);
				}
			}
		}
		foreach (FindPreyJob findPreyJob in owner.FindPreyJobs)
		{
			if (findPreyJob.Zone.ZoneHunt.CreaturesToHunt == null)
			{
				continue;
			}
			foreach (KeyValuePair<EntityType, int> item in findPreyJob.Zone.ZoneHunt.CreaturesToHunt)
			{
				if (item.Value > 0)
				{
					owner.RecomputeImportance(item.Key.BiologicalType.CarcassType, allAvailableItems);
				}
			}
		}
		owner.RecomputeFoodProductionImportance(allAvailableItems);
	}

	private void CreateFixedNoOfProcessJobs(int jobsToCreate, EntityType entityType)
	{
		List<ProcessType> jobManagerProductionProcessesThatCanProduce = GetJobManagerProductionProcessesThatCanProduce(entityType);
		if (jobManagerProductionProcessesThatCanProduce == null)
		{
			return;
		}
		int num = 0;
		do
		{
			List<ProcessCombo> combos = CreateProductionCombos(jobManagerProductionProcessesThatCanProduce);
			ProcessCombo? processCombo = SelectBestCombo(combos);
			if (processCombo.HasValue)
			{
				if (processCombo.Value.ResourceItem != null)
				{
					CreateHarvestJob(owner, entityType, processCombo.Value.ProcessType, processCombo.Value.Zone, null, processCombo.Value.ResourceItem);
				}
				else
				{
					CreateProcessJobAndHaulingJobs(entityType, processCombo.Value.ProcessType);
				}
				num++;
				continue;
			}
			break;
		}
		while (num < jobsToCreate);
	}

	private void UpdateDirectOrderJobs()
	{
		if (owner.ProductionOrders == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, ProductionOrder> order in owner.ProductionOrders.Orders)
		{
			EntityType key = order.Key;
			if (order.Value.ProductionJobsToComplete.HasValue)
			{
				UpdateDirectOrderJobs(key);
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
			if (value.AmountToKeepInStore.HasValue && value.AmountToKeepInStore.Value != -1)
			{
				RemoveUnneededStandingOrderJobs(key, value);
			}
		}
	}

	private bool ManagerCanRemoveProductionJob(ProcessJob job)
	{
		if (job.HarvestJob != null && job.HarvestJob.Zone.AllowStandingOrderHarvest != null)
		{
			if (job.HarvestJob.Zone.AllowStandingOrderHarvest.Contains(job.HarvestJob.ResourceType))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void RebalanceStandingOrderJobs()
	{
		if (owner.ProductionOrders == null)
		{
			return;
		}
		int standingOrderJobCapacity = GetStandingOrderJobCapacity();
		int num = owner.ProductionOrders.Orders.Sum((KeyValuePair<EntityType, ProductionOrder> o) => (o.Value.AmountToKeepInStore.HasValue && o.Value.AmountToKeepInStore.Value != 0) ? 1 : 0);
		int num2 = 0;
		num2 = ((num <= standingOrderJobCapacity) ? ((int)Math.Floor((float)standingOrderJobCapacity / (float)num)) : GameData.Instance.AIConstants.MinimumJobsPerStandingOrder);
		foreach (KeyValuePair<EntityType, ProductionOrder> order in owner.ProductionOrders.Orders)
		{
			EntityType key = order.Key;
			ProductionOrder value = order.Value;
			if (value.AmountToKeepInStore.HasValue)
			{
				UpdateStandingOrder(key, value, num2);
			}
		}
	}

	private int GetStandingOrderJobCapacity()
	{
		int maximumJobsBeforeWarning = EntityGroup.GetMaximumJobsBeforeWarning(owner.Parent);
		int totalDirectOrders = owner.ProductionOrders.TotalDirectOrders;
		return maximumJobsBeforeWarning - totalDirectOrders;
	}

	private void RemoveUnneededStandingOrderJobs(EntityType entityType, ProductionOrder order)
	{
		GetAmountToProduce(owner, entityType, order, out var _, out var amountToProduce);
		if (amountToProduce < 0)
		{
			int outputAmountToRemove = Math.Abs(amountToProduce);
			owner.ManagedProductionJobs.TryGetValue(entityType, out var value);
			RemoveStandingOrderProcessJobsByOutputAmount(entityType, value, outputAmountToRemove);
		}
	}

	private void UpdateStandingOrder(EntityType entityType, ProductionOrder order, int maxJobs)
	{
		owner.ManagedProductionJobs.TryGetValue(entityType, out var value);
		GetAmountToProduce(owner, entityType, order, out var currentJobs, out var amountToProduce);
		if (currentJobs > maxJobs)
		{
			int noOfJobsToRemove = currentJobs - maxJobs;
			DestroyJobsIntelligently(value, noOfJobsToRemove, (ProcessJob j) => ManagerCanRemoveProductionJob(j));
		}
		else if (amountToProduce > 0)
		{
			CreateProcessJobsToMatchOutput(entityType, amountToProduce, currentJobs, maxJobs);
		}
		else if (amountToProduce < 0)
		{
			RemoveStandingOrderProcessJobsByOutputAmount(entityType, value, Math.Abs(amountToProduce));
		}
	}

	private static void GetAmountToProduce(EntityGroup owner, EntityType entityType, ProductionOrder order, out int currentJobs, out int amountToProduce)
	{
		int itemsInStock = owner.CountAvailableItems(entityType);
		float averageSpeed;
		int totalOutstandingOutput = owner.CountOutstandingJobOutput(entityType, countUnstarted: true, out currentJobs, out averageSpeed);
		amountToProduce = GetAmountToProduce(order, itemsInStock, totalOutstandingOutput);
	}

	public static int GetAmountToProduce(ProductionOrder order, int itemsInStock, int totalOutstandingOutput)
	{
		if (order.AmountToKeepInStore.Value == -1)
		{
			return Common.Clamp(10 - totalOutstandingOutput, 0, 10);
		}
		return order.AmountToKeepInStore.Value - itemsInStock - totalOutstandingOutput;
	}

	private void CreateProcessJobsToMatchOutput(EntityType entityType, int amountToProduce, int currentJobs, int maxJobs)
	{
		List<ProcessType> jobManagerProductionProcessesThatCanProduce = GetJobManagerProductionProcessesThatCanProduce(entityType);
		if (jobManagerProductionProcessesThatCanProduce == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		do
		{
			List<ProcessCombo> combos = CreateProductionCombos(jobManagerProductionProcessesThatCanProduce);
			ProcessCombo? processCombo = SelectBestCombo(combos);
			if (processCombo.HasValue)
			{
				if (processCombo.Value.ResourceItem != null)
				{
					CreateHarvestJob(owner, entityType, processCombo.Value.ProcessType, processCombo.Value.Zone, null, processCombo.Value.ResourceItem);
				}
				else
				{
					CreateProcessJobAndHaulingJobs(entityType, processCombo.Value.ProcessType);
				}
				num++;
				num2 += processCombo.Value.ProcessType.GetOutputAmount(entityType) ?? 0;
				continue;
			}
			break;
		}
		while (currentJobs + num < maxJobs && num2 < amountToProduce);
	}

	private ProcessCombo? SelectBestCombo(List<ProcessCombo> combos)
	{
		if (combos != null && combos.Count > 0)
		{
			return combos[0];
		}
		return null;
	}

	private List<ProcessCombo> CreateProductionCombos(List<ProcessType> processesThatCanProduce)
	{
		List<ProcessCombo> list = new List<ProcessCombo>();
		foreach (ProcessType item2 in processesThatCanProduce)
		{
			if (item2.IsGathering)
			{
				CreateGatherCombos(item2, list);
				continue;
			}
			ProcessCombo item = new ProcessCombo
			{
				ProcessType = item2
			};
			list.Add(item);
		}
		return list;
	}

	private void CreateGatherCombos(ProcessType process, List<ProcessCombo> combos)
	{
		ResourceType resourceTypeInput = process.ResourceTypeInput;
		List<IResourceItem> list = new List<IResourceItem>();
		foreach (Zone zone in owner.Zones)
		{
			if (zone.AllowStandingOrderHarvest.Contains(resourceTypeInput))
			{
				FindResourceItemsToGather(owner.GetAllegiance().SharedKnowledge, zone.MapArea, resourceTypeInput, list, 1);
				if (list.Count > 0)
				{
					ProcessCombo item = new ProcessCombo
					{
						ProcessType = process,
						Zone = zone,
						ResourceItem = list[0]
					};
					combos.Add(item);
					list.Clear();
				}
			}
		}
	}

	private List<ProcessType> GetJobManagerProductionProcessesThatCanProduce(EntityType entityType)
	{
		List<ProcessType> list = null;
		GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value);
		if (value != null)
		{
			foreach (ProcessType item in value)
			{
				if (IsManagedProcess(item) && InventoryPanel.HasAllInputsAndToolsForProcess(item, owner, allAvailableItems))
				{
					Common.AddToList(ref list, item);
				}
			}
		}
		return list;
	}

	public static bool IsManagedProcess(ProcessType processType)
	{
		if (!processType.IsSalvageProcess && !processType.IsKilling)
		{
			return !processType.IsPseudoProcess;
		}
		return false;
	}

	public static bool IsManagedProductionJob(ProcessJob processJob)
	{
		return IsManagedProcess(processJob.ProcessType);
	}

	private void RemoveStandingOrderProcessJobsByOutputAmount(EntityType entityType, List<ProcessJob> jobs, int outputAmountToRemove)
	{
		if (jobs == null)
		{
			return;
		}
		int count;
		do
		{
			count = jobs.Count;
			DestroyJobsIntelligently(jobs, 1, (ProcessJob j) => j.ProcessType.GetOutputAmount(entityType) <= outputAmountToRemove && ManagerCanRemoveProductionJob(j), (ProcessJob j) => OnRemoveProcessJob(j, entityType, ref outputAmountToRemove));
		}
		while (jobs.Count != count);
	}

	private bool OnRemoveProcessJob(ProcessJob j, EntityType entityType, ref int outputAmountToRemove)
	{
		outputAmountToRemove -= j.ProcessType.GetOutputAmount(entityType) ?? 0;
		return true;
	}

	public void UpdateDirectOrderJobs(EntityType entityType)
	{
		entityType.Name.Contains("glassyPorridge");
		int num = 0;
		GameData.Instance.ItemHarvestSource.TryGetValue(entityType, out var _);
		ProductionOrder productionOrder = owner.ProductionOrders.Orders[entityType];
		if (!productionOrder.ProductionJobsToComplete.HasValue)
		{
			return;
		}
		int value2 = productionOrder.ProductionJobsToComplete.Value;
		owner.ManagedProductionJobs.TryGetValue(entityType, out var value3);
		int num2 = CountUnstartedJobs(entityType, value3);
		num = value2 - num2;
		if (num > 0)
		{
			CreateFixedNoOfProcessJobs(num, entityType);
		}
		else if (num < 0 && entityType.ItemType.CarcassType == null)
		{
			DestroyJobsIntelligently(value3, Math.Abs(num), (ProcessJob j) => ManagerCanRemoveProductionJob(j));
		}
	}

	private int CountUnstartedJobs(EntityType entityType, List<ProcessJob> existingProductionJobs)
	{
		if (existingProductionJobs == null || existingProductionJobs.Count == 0)
		{
			return 0;
		}
		int num = existingProductionJobs.Count;
		for (int num2 = existingProductionJobs.Count - 1; num2 >= 0; num2--)
		{
			ProcessJob processJob = existingProductionJobs[num2];
			if (processJob.IsStarted(out var isStarted))
			{
				if (isStarted)
				{
					num--;
				}
			}
			else
			{
				GoalEvaluator.HandleInvalidJob(processJob);
				num--;
			}
		}
		return num;
	}

	private void CancelHaulingJobsForProcessJobs(List<ProcessJob> jobs)
	{
		List<Job> haulingJobs = owner.HaulingJobs;
		for (int num = haulingJobs.Count - 1; num >= 0; num--)
		{
			Job job = haulingJobs[num];
			for (int num2 = jobs.Count - 1; num2 >= 0; num2--)
			{
				if ((job as HaulingJob).RequiredByProcessJob == jobs[num2])
				{
					job.Destroy(cancelTakers: true);
				}
			}
		}
	}

	private static bool TestIsUnstarted(ProcessJob processJob, ref int removedJobs)
	{
		if (processJob.IsStarted(out var isStarted) && isStarted)
		{
			return false;
		}
		return true;
	}

	public static void DestroyJobsIntelligently(List<ProcessJob> jobs, int noOfJobsToRemove, Predicate<ProcessJob> allowJobToBeRemoved = null, Predicate<ProcessJob> breakOnDestroy = null)
	{
		if (jobs == null)
		{
			return;
		}
		int removedJobs = 0;
		for (int num = jobs.Count - 1; num >= 0; num--)
		{
			ProcessJob processJob = jobs[num];
			if (!processJob.IsStarted(out var _))
			{
				processJob.Destroy(removeTakers: true);
				jobs.Remove(processJob);
				removedJobs++;
				if ((breakOnDestroy != null && breakOnDestroy(processJob)) || removedJobs == noOfJobsToRemove)
				{
					return;
				}
			}
		}
		for (int num2 = jobs.Count - 1; num2 >= 0; num2--)
		{
			ProcessJob processJob = jobs[num2];
			if (TestIsUnstarted(processJob, ref removedJobs))
			{
				if (processJob.TakenBy.Count == 0 && (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob)))
				{
					processJob.Destroy(removeTakers: true);
					jobs.Remove(processJob);
					removedJobs++;
					if (breakOnDestroy != null && breakOnDestroy(processJob))
					{
						return;
					}
				}
				if (removedJobs == noOfJobsToRemove)
				{
					return;
				}
			}
		}
		for (int num3 = jobs.Count - 1; num3 >= 0; num3--)
		{
			ProcessJob processJob = jobs[num3];
			if (TestIsUnstarted(processJob, ref removedJobs))
			{
				if ((allowJobToBeRemoved == null || allowJobToBeRemoved(processJob)) && AllTakersMinimumDistance(processJob) > 60f)
				{
					processJob.Destroy(removeTakers: true);
					jobs.Remove(processJob);
					removedJobs++;
					if (breakOnDestroy != null && breakOnDestroy(processJob))
					{
						return;
					}
				}
				if (removedJobs == noOfJobsToRemove)
				{
					return;
				}
			}
		}
		for (int num4 = jobs.Count - 1; num4 >= 0; num4--)
		{
			ProcessJob processJob = jobs[num4];
			if (TestIsUnstarted(processJob, ref removedJobs))
			{
				if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
				{
					processJob.Destroy(removeTakers: true);
					jobs.Remove(processJob);
					removedJobs++;
					if (breakOnDestroy != null && breakOnDestroy(processJob))
					{
						return;
					}
				}
				if (removedJobs == noOfJobsToRemove)
				{
					return;
				}
			}
		}
		for (int num5 = jobs.Count - 1; num5 >= 0; num5--)
		{
			ProcessJob processJob = jobs[num5];
			if (allowJobToBeRemoved == null || allowJobToBeRemoved(processJob))
			{
				processJob.Destroy(removeTakers: true);
				jobs.Remove(processJob);
				removedJobs++;
				if (breakOnDestroy != null && breakOnDestroy(processJob))
				{
					break;
				}
			}
			if (removedJobs == noOfJobsToRemove)
			{
				break;
			}
		}
	}

	public static float AllTakersMinimumDistance(Job job)
	{
		float num = 1000000f;
		((ProcessJob)job).GetCurrentJobLocation(out var location);
		if (location.HasValue)
		{
			for (int i = 0; i < job.TakenBy.Count; i++)
			{
				Entity entity = job.TakenBy.Get(i);
				float num2 = Common.DistanceOctile(location.Value, entity.PlaySiteLocation);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public static void AddHarvestJobs(int jobDifference, EntityGroup expeditionOwner, EntityType outputType, ProcessType processType, ResourceType resourceType, Zone zone, Priority? priority)
	{
		List<IResourceItem> list = new List<IResourceItem>();
		int num = FindResourceItemsToGather(expeditionOwner.GetAllegiance().SharedKnowledge, zone.MapArea, resourceType, list, jobDifference);
		for (int i = 0; i < num; i++)
		{
			IResourceItem item = list[list.Count - 1];
			CreateHarvestJob(expeditionOwner, outputType, processType, zone, priority, item);
			list.RemoveAt(list.Count - 1);
		}
	}

	private static void CreateHarvestJob(EntityGroup expeditionOwner, EntityType outputType, ProcessType processType, Zone zone, Priority? priority, IResourceItem item)
	{
		ProcessJob processJob = new ProcessJob(processType, outputType, expeditionOwner, item);
		if (priority.HasValue)
		{
			processJob.Priority = priority.Value;
		}
		zone.AddHarvestJob(processJob);
	}

	private static int FindResourceItemsToGather(SharedKnowledge sharedKnowledge, MapArea area, ResourceType resourceType, List<IResourceItem> items, int noOfRequiredItems)
	{
		items.Clear();
		Tree treeComponent;
		Crop crop;
		TileResourceContainer container;
		area.IterateAreaBreakOnTrue(delegate(TerrainTile terrainTile)
		{
			if (resourceType.CropType != null)
			{
				if (terrainTile.TreesOnTile != null)
				{
					foreach (Entity item in terrainTile.TreesOnTile)
					{
						item.Find<Tree>(out treeComponent);
						if (treeComponent.Crops != null && treeComponent.Crops.TryGetValue(resourceType, out crop) && AllowGatherJobForResource(sharedKnowledge, crop))
						{
							foreach (IResourceItem resourceItem in crop.ResourceItems)
							{
								if (resourceItem.AssignedToJob == null)
								{
									items.Add(resourceItem);
								}
								if (items.Count == noOfRequiredItems)
								{
									return true;
								}
							}
						}
					}
				}
			}
			else if (resourceType.TileResourceType != null && terrainTile.TileResources != null && terrainTile.TileResources.TryGetValue(resourceType, out container) && AllowGatherJobForResource(sharedKnowledge, container))
			{
				foreach (IResourceItem resourceItem2 in container.ResourceItems)
				{
					if (resourceItem2.AssignedToJob == null)
					{
						items.Add(resourceItem2);
					}
					if (items.Count == noOfRequiredItems)
					{
						return true;
					}
				}
			}
			return (items.Count == noOfRequiredItems) ? true : false;
		});
		return Math.Min(items.Count, noOfRequiredItems);
	}

	public static bool AllowGatherJobForResource(SharedKnowledge sharedKnowledge, ResourceContainer container)
	{
		if (sharedKnowledge.AllDetectedEntities.Contains(container.DetectableID))
		{
			return !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtilePos(container.AccessPoint));
		}
		return false;
	}

	private Zone SelectGatherZone(ResourceType resource)
	{
		foreach (Zone zone in owner.Zones)
		{
			if (zone.AllowStandingOrderHarvest.Contains(resource))
			{
				return zone;
			}
		}
		return null;
	}

	private bool CreateProcessJobAndHaulingJobs(EntityType entityType, ProcessType processType)
	{
		if (processType != null)
		{
			Vector3? location = null;
			if (FindProductionLocation(processType, owner.Parent, out location))
			{
				ProcessJob processJob = CreateProcessJob(entityType, owner, processType, null, location);
				if (!processJob.GetFixedJobLocation(out var location2))
				{
					return false;
				}
				if (location2.HasValue)
				{
					processJob.CreateHaulingJobsForProcessInputs(owner, location2.Value);
				}
			}
			return true;
		}
		return false;
	}

	public static ProcessJob CreateSpecialActionJob(EntityGroup entityGroup, IKnownEntityData actingOnEntityData, ProcessType processType)
	{
		Vector3 value = actingOnEntityData.AccessPoint.Value;
		ProcessJob processJob = CreateProcessJob(null, entityGroup, processType, null, value, actingOnEntityData.GetAsEntityAndRoot());
		if (processType.InputsByType != null && processType.InputsByType.TryGetValue(actingOnEntityData.EntityType, out var value2) && value2.InputIsImmovable())
		{
			processJob.AssignImmovableInput(actingOnEntityData);
		}
		processJob.CreateHaulingJobsForProcessInputs(entityGroup, value);
		return processJob;
	}

	private void CleanupProductionJobs()
	{
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in owner.ProductionJobs)
		{
			EntityType key = productionJob.Key;
			List<ProcessJob> value = productionJob.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				ProcessJob processJob = value[num];
				if (!processJob.IsStarted(out var isStarted) || !isStarted)
				{
					CleanupUnstartedJob(owner, key, processJob);
				}
			}
		}
	}

	private static void CleanupUnstartedJob(EntityGroup owner, EntityType entityType, ProcessJob pJob)
	{
		bool hasInputs = false;
		bool hasTools = false;
		pJob.RepairAssignedInputs(owner.GetAllegiance().SharedKnowledge);
		if (pJob.ID == JobID.Invalid)
		{
			return;
		}
		bool flag = true;
		if (pJob.HarvestJob != null && !AllowGatherJobForResource(owner.GetAllegiance().SharedKnowledge, pJob.HarvestJob.Item.Container))
		{
			flag = false;
		}
		if (pJob.BuildingJob != null)
		{
			return;
		}
		InventoryPanel.HasAllInputsAndToolsForProcess(pJob.ProcessType, owner, out hasInputs, out hasTools, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _);
		if (!hasInputs || !flag)
		{
			if (owner.ProductionOrders.Orders.TryGetValue(entityType, out var value) && value.ProductionJobsToComplete > 0)
			{
				value.ProductionJobsToComplete--;
			}
			pJob.Destroy(removeTakers: true);
		}
	}

	public static ProcessJob CreateProcessJob(EntityType entityType, EntityGroup entityGroup, ProcessType processType, EntityID? containerToPlaceOutputsIn, Vector3? productionSiteLocation, EntityAndRoot? processActingOn = null, bool isSalvage = false, UpgradeCategory upgradeCategory = null)
	{
		ProcessJob processJob = new ProcessJob(containerToPlaceOutputsIn, processType, entityType, entityGroup, processActingOn, isSalvage, upgradeCategory);
		if (productionSiteLocation.HasValue)
		{
			LookUp<SimProcess, SimProcessID>.FindByID(processJob.ProductionProcess).GroundLocation = productionSiteLocation;
		}
		return processJob;
	}

	public static ProcessJob CreateProcessJob(EntityType entityType, EntityGroup entityGroup, ProcessType processType, EntityID? containerToPlaceOutputsIn, Vector3? productionSiteLocation, EntityAndRoot processActingOn, RepairAction action, EntityAndRoot? partToFix)
	{
		ProcessJob processJob = new ProcessJob(containerToPlaceOutputsIn, processType, entityType, entityGroup, action, processActingOn, partToFix);
		if (productionSiteLocation.HasValue)
		{
			LookUp<SimProcess, SimProcessID>.FindByID(processJob.ProductionProcess).GroundLocation = productionSiteLocation;
		}
		return processJob;
	}

	public static IKnownEntityData FindBestTool(List<EntityType> toolTypes, Expedition expedition)
	{
		foreach (EntityType toolType in toolTypes)
		{
			if (!expedition.OwnedEntities.Structures.TryGetValue(toolType, out var value))
			{
				continue;
			}
			foreach (EntityID item in Common.Randomize(new List<EntityID>(value), The.Sim.GameplayRandomGenerator))
			{
				if (GoalEvaluator.HandleOwnerDataResult(expedition.Allegiance.SharedKnowledge, item, expedition.OwnedEntities, out var entityData))
				{
					return entityData;
				}
			}
		}
		return null;
	}

	public static bool FindProductionLocation(ProcessType processType, IHasEntityGroup hasEntityGroup, out Vector3? location)
	{
		location = null;
		if (processType.ProcessToolSet != null)
		{
			foreach (ToolTypeCombination toolTypeCombination in processType.ProcessToolSet.ToolTypeCombinations)
			{
				if (toolTypeCombination.Tools.Exists((Tuple<EntityType, float> t) => ToolType.IsImmovable(t.Item1)))
				{
					return true;
				}
			}
		}
		if (processType.Inputs != null)
		{
			if (processType.Inputs.Length == 1)
			{
				if (processType.Inputs[0].InputIsImmovable())
				{
					return true;
				}
			}
			else if (processType.Inputs.Count((Input i) => i.InputIsImmovable()) > 0)
			{
				return false;
			}
		}
		location = FindProductionLocation(hasEntityGroup);
		return true;
	}

	public static Vector3 FindProductionLocation(IHasEntityGroup hasEntityGroup)
	{
		return EntityGroup.GetFreeGroundLocation(hasEntityGroup);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		phase = sn.DoEnum(phase);
		IsPaused = sn.DoBool(IsPaused);
		snapshotOwnerID = sn.SnapshotID<EntityGroup, EntityGroupID>(owner).Value;
		sn.Ignore(regulator);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(allAvailableItems);
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
