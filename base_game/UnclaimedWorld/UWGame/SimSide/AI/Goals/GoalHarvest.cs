using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.AI.Goals;

public class GoalHarvest : CompositeGoal, ITopLevelGoal
{
	public ProcessJob harvestJob;

	private JobID? snapshotJob;

	public List<ProcessJob> queuedJobs = new List<ProcessJob>();

	private List<JobID> snapshotQueuedJobs;

	public OwnerID? OwnerOfHarvest;

	public List<EntityID> Tools = new List<EntityID>();

	public ToolTypeCombination ToolTypeCombination;

	private ToolTypeCombinationID? snapshotToolCombo;

	private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalHarvest(Entity entity, ProcessJob job, OwnerID? ownerOfHarvest, List<EntityGroupID> ownersOfVehicles, List<EntityID> tools, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, ToolTypeCombination toolTypeCombination)
		: base(entity)
	{
		harvestJob = job;
		base.ownersOfVehicles = ownersOfVehicles;
		OwnerOfHarvest = ownerOfHarvest;
		Tools = tools;
		ToolTypeCombination = toolTypeCombination;
		this.replenishItemsForTools = replenishItemsForTools;
	}

	private void ClaimAdditionalHarvestJobs()
	{
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(harvestJob.HarvestJob.Owner);
		if (entityGroup == null)
		{
			base.Status = Status.Failed;
			return;
		}
		List<ProcessJob> list = entityGroup.ProductionJobs[harvestJob.HarvestJob.ResourceType.ResourceItemType];
		int num = harvestJob.HarvestJob.Item.Container.NoOfHarvestableItems;
		if (num <= 1)
		{
			return;
		}
		foreach (ProcessJob item in list)
		{
			ProcessJob processJob = item as ProcessJob;
			if (processJob != harvestJob && processJob != null && processJob.HarvestJob != null && processJob.HarvestJob.CanBeQueuedInGoal() && processJob.TakenBy.Count < processJob.MaxJobPositions && processJob.HarvestJob.IsSpecificJob && processJob.HarvestJob.Item != null && harvestJob.HarvestJob.Item.Container.ResourceItems.Contains(processJob.HarvestJob.Item))
			{
				processJob.TakeJob(entity);
				queuedJobs.Add(processJob);
				num--;
				if (num <= 1)
				{
					break;
				}
			}
		}
	}

	public GoalHarvest()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
		harvestJob.TakeJob(entity);
		SetLocksOnReplenishItems(harvestJob, replenishItemsForTools);
		GetTreeAndGatheringSite(out var treeEntity, out var site);
		Vector3 destination = treeEntity?.AccessPoint.Value ?? harvestJob.HarvestJob.Item.Container.AccessPoint;
		List<ItemType.TaskType> gearTasks = null;
		CompositeGoal.AddNightActivityGear(ref gearTasks);
		FindOptionalEquipmentIfNeeded(destination, harvestJob, equipWeapon: true, equipFood: true, mountWeapon: true, gearTasks);
		List<IKnownEntityData> toolsData = null;
		if (!ResolveTools(Tools, ref toolsData) || !GatherToolsOrWeapons(toolsData, ownersOfVehicles, harvestJob, mountAfterPickup: false, StorageCompartment.Haul))
		{
			return;
		}
		ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, harvestJob);
		entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AddCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity, harvestJob.HarvestJob.Item.Container);
		float value = harvestJob.HarvestJob.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
		DropUnneededItemsToMakeCapacity(value, (Entity e) => !Tools.Contains(e.EntityID), out var _, StorageCompartment.Haul);
		if (site != null && treeEntity != null)
		{
			if (!site.CanAddVisitor(ref entity))
			{
				base.Status = Status.Failed;
				return;
			}
			AddSubgoal(new GoalArriveAsVisitor(entity, treeEntity.EntityID, ownersOfVehicles));
			PlaceStationaryToolsAtWorkSite(toolsData, null);
		}
		else
		{
			destination = harvestJob.HarvestJob.Item.Container.AccessPoint;
			AddSubgoal(new GoalMoveToPosition(entity, destination, ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			PlaceStationaryToolsAtWorkSite(toolsData, destination);
		}
		if (entity.HasStance())
		{
			ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(harvestJob.ProcessType.StanceTypes));
		}
		AddSubgoal(new GoalDoProduce(entity, harvestJob, OwnerOfHarvest, Tools, ToolTypeCombination));
		ClaimAdditionalHarvestJobs();
	}

	private void GetTreeAndGatheringSite(out Entity treeEntity, out GatheringSite site)
	{
		site = null;
		treeEntity = null;
		if (harvestJob.HarvestJob.Item is CropItem cropItem && cropItem.crop.Parent is Tree tree)
		{
			site = tree.Parent.GatheringSite;
			treeEntity = tree.Parent;
		}
	}

	public override bool RequiresBoldStance()
	{
		return harvestJob.RequiresBoldStance;
	}

	protected override bool ArePreconditionsOK()
	{
		if (!AreToolsOK(Tools))
		{
			return false;
		}
		if (harvestJob.HarvestJob.Item.AssignedToJob == harvestJob)
		{
			return !harvestJob.HarvestJob.Item.Container.IsDestroyed(entityIntelligence.Allegiance.SharedKnowledge);
		}
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		else
		{
			base.Status = Status.Failed;
		}
		if (base.Status == Status.Completed)
		{
			entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RemoveCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity);
			GetTreeAndGatheringSite(out var _, out var site);
			site?.RemoveVisitor(entity.EntityID);
			RemoveProcessToolLocks(harvestJob, Tools, replenishItemsForTools);
			if (queuedJobs.Count > 0)
			{
				harvestJob = queuedJobs[0];
				queuedJobs.RemoveAt(0);
				DropUnneededItemsToMakeCapacity(harvestJob.HarvestJob.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value, (Entity e) => e.EntityType == harvestJob.HarvestJob.ResourceType.ResourceItemType, out var _, StorageCompartment.Haul);
				SetLocksOnToolsOrWeapons(Tools, harvestJob);
				AddSubgoal(new GoalDoProduce(entity, harvestJob, OwnerOfHarvest, Tools, ToolTypeCombination));
				base.Status = Status.Active;
			}
		}
		else if (base.Status == Status.Failed)
		{
			AbandonJobs();
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				base.Status = Status.Failed;
				AbandonJobs();
				return true;
			}
			return false;
		}
		return true;
	}

	private void AbandonJobs()
	{
		harvestJob.Abandon(entity);
		foreach (ProcessJob queuedJob in queuedJobs)
		{
			queuedJob.Abandon(entity);
		}
	}

	public override string GetStatus()
	{
		return "Harvesting";
	}

	public double ScoreGoal()
	{
		ToolParams value = new ToolParams
		{
			Tools = Tools,
			ReplenishStatus = null,
			JobDurationInDays = null,
			ToolProductivity = GoalProduce.GetToolCombinationProductivity(ToolTypeCombination)
		};
		return ScoreJobGoal(harvestJob, value);
	}

	public override bool IsSame(Job job)
	{
		return job == harvestJob;
	}

	public override void Deactivate()
	{
		if (harvestJob != null)
		{
			entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RemoveCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity);
			RemoveProcessToolLocks(harvestJob, Tools, replenishItemsForTools);
			AbandonJobs();
			GetTreeAndGatheringSite(out var _, out var site);
			site?.RemoveVisitor(entity.EntityID);
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
		snapshotJob = sn.SnapshotID<Job, JobID>(harvestJob);
		if (queuedJobs != null)
		{
			snapshotQueuedJobs = queuedJobs.Select((ProcessJob j) => j.ID).ToList();
		}
		snapshotQueuedJobs = sn.DoList(snapshotQueuedJobs);
		OwnerOfHarvest = sn.DoEnumNullable(OwnerOfHarvest);
		Tools = sn.DoList(Tools);
		replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
		snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		sn.Ignore(queuedJobs);
		sn.Ignore(harvestJob);
		sn.Ignore(ToolTypeCombination);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotQueuedJobs != null)
		{
			queuedJobs = snapshotQueuedJobs.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList();
		}
		snapshotQueuedJobs.Clear();
		if (snapshotJob.HasValue)
		{
			harvestJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
		ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
	}
}
