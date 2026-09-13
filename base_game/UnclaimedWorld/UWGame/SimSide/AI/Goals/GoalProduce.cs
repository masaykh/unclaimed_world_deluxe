using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalProduce : CompositeGoal, ITopLevelGoal
{
	private ProcessType processType;

	private EntityAndRoot? actingOnEntity;

	private StorageCompartment? replenishCompartmentToUse;

	private ProcessJob job;

	private JobID? snapshotJob;

	public OwnerID? OwnerOfProduct;

	public List<EntityID> Tools = new List<EntityID>();

	public ToolTypeCombination ToolTypeCombination;

	private ToolTypeCombinationID? snapshotToolCombo;

	private EntityID? inputItem;

	private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

	private bool isInRangeOfRemotelyStartedJob;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	private EntityAndRoot? ActingOnEntity
	{
		get
		{
			if (job != null)
			{
				if (job.GetActingOnEntity(out EntityAndRoot? result))
				{
					return result;
				}
				return null;
			}
			return actingOnEntity;
		}
	}

	private ProcessType ProcessType
	{
		get
		{
			if (job != null)
			{
				return job.ProcessType;
			}
			return processType;
		}
	}

	public GoalProduce(Entity owner, ProcessJob job, OwnerID? ownerOfProduct, List<EntityGroupID> ownersOfVehicles, List<EntityID> tools, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, ToolTypeCombination toolTypeCombination, EntityID? inputItem)
		: base(owner)
	{
		this.job = job;
		OwnerOfProduct = ownerOfProduct;
		base.ownersOfVehicles = ownersOfVehicles;
		Tools = tools;
		ToolTypeCombination = toolTypeCombination;
		this.replenishItemsForTools = replenishItemsForTools;
		this.inputItem = inputItem;
	}

	public GoalProduce(Entity owner, ProcessType processType, EntityAndRoot? actingOnEntity, OwnerID? ownerOfProduct, List<EntityGroupID> ownersOfVehicles, List<EntityID> tools, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, ToolTypeCombination toolTypeCombination, EntityID? inputItem)
		: base(owner)
	{
		this.processType = processType;
		this.actingOnEntity = actingOnEntity;
		OwnerOfProduct = ownerOfProduct;
		base.ownersOfVehicles = ownersOfVehicles;
		Tools = tools;
		ToolTypeCombination = toolTypeCombination;
		this.replenishItemsForTools = replenishItemsForTools;
		this.inputItem = inputItem;
	}

	public GoalProduce()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
		IKnownEntityData data = null;
		IKnownProcess processData = null;
		if (job != null)
		{
			job.TakeJob(entity);
			SetLocksOnReplenishItems(job, replenishItemsForTools);
			SetLockOnActingOnEntity(job);
			if (!job.GetImmovableInput(out var input, out processData))
			{
				base.Status = Status.Failed;
				return;
			}
			if (input.HasValue && EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(input.Value, out data)))
			{
				base.Status = Status.Failed;
				return;
			}
		}
		if (job != null && job.RequiresBoldStance)
		{
			entity.Intelligence.SetBoldStance();
		}
		processData = null;
		if (!GetWorkLocation(out var location, ref processData) || !location.HasValue)
		{
			base.Status = Status.Failed;
			return;
		}
		if (ProcessType.WorkNeeded != WorkerNeededOptions.StartRemotely || !CheckIfJobTargetCanBeSeenAndStarted())
		{
			List<ItemType.TaskType> gearTasks = null;
			CompositeGoal.AddNightActivityGear(ref gearTasks);
			FindOptionalEquipmentIfNeeded(location.Value, job, equipWeapon: true, equipFood: true, mountWeapon: true, gearTasks);
			List<IKnownEntityData> toolsData = null;
			if (!ResolveTools(Tools, ref toolsData) || !GatherToolsOrWeapons(toolsData, ownersOfVehicles, job, mountAfterPickup: false, StorageCompartment.Haul))
			{
				return;
			}
			AddSubgoal(new GoalMoveToPosition(entity, location.Value, ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			PlaceStationaryToolsAtWorkSite(toolsData, location);
			if (data != null)
			{
				bool carriedBySelf = false;
				bool insideContainerWeCannotUse = false;
				IKnownEntityData buildingWeCanEnter = null;
				IKnownEntityData containerWeCanUnloadFrom = null;
				if (!GetInputInsideContainer(data, out carriedBySelf, out insideContainerWeCannotUse, out buildingWeCanEnter, out containerWeCanUnloadFrom))
				{
					return;
				}
				if (carriedBySelf)
				{
					AddSubgoal(new GoalDropItem(entity, data.EntityID));
				}
				else
				{
					if (insideContainerWeCannotUse)
					{
						base.Status = Status.Failed;
						return;
					}
					if (buildingWeCanEnter != null)
					{
						AddSubgoal(new GoalEnter(entity, data.ContainedBy.Value));
						if (!data.EntityType.IsImmovable())
						{
							JobID? thisJobID = ((job != null) ? new JobID?(job.ID) : ((JobID?)null));
							DropUnneededItemsToMakeCapacity(data.Bulk, (Entity e) => e.AssignedToJob != thisJobID, out var _, StorageCompartment.Haul);
							AddSubgoal(new GoalPickup(entity, data.EntityID, null, StorageCompartment.Haul, false));
							AddSubgoal(new GoalExit(entity));
							AddSubgoal(new GoalDropItem(entity, data.EntityID, job));
						}
					}
					else if (containerWeCanUnloadFrom != null)
					{
						AddSubgoal(new GoalUnload(entity, data.ContainedBy.Value, data.EntityID, job));
					}
				}
			}
			ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);
			PrepareTools(toolsData, ownersOfVehicles);
			if (entity.HasStance())
			{
				ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(ProcessType.StanceTypes));
			}
		}
		AddDoProduceGoal();
	}

	private void AddDoProduceGoal()
	{
		if (job != null)
		{
			AddSubgoal(new GoalDoProduce(entity, job, OwnerOfProduct, Tools, ToolTypeCombination));
		}
		else
		{
			AddSubgoal(new GoalDoProduce(entity, processType, null, null, limitExtractionByNutrients: false, OwnerOfProduct, Tools, ToolTypeCombination, null, null, actingOnEntity));
		}
	}

	private bool SetLockOnActingOnEntity(ProcessJob job)
	{
		EntityID? entityID = EntityAndRoot.GetEntity(ActingOnEntity);
		if (entityID.HasValue)
		{
			if (EntityResultCausesFailedGoal(entity.Intelligence.Allegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
			{
				return false;
			}
			data.AssignedToJob = job.ID;
		}
		return true;
	}

	private bool CheckIfJobTargetCanBeSeenAndStarted()
	{
		if (ProcessType.WorkNeeded == WorkerNeededOptions.StartRemotely)
		{
			SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
			IKnownProcess processData = null;
			if (!GetWorkLocation(out var location, ref processData) || !location.HasValue)
			{
				return false;
			}
			Point pos = MapManager.WorldPosToTile(location.Value);
			if (The.Map.GetTile(pos).AllegiancesThatSeeThisTile.Contains(entityIntelligence.Allegiance))
			{
				if (processData != null)
				{
					if (processData.ActingOnEntity.HasValue && sharedKnowledge.GetKnownDataAsEntity(processData.ActingOnEntity.Value.Entity) == null)
					{
						return false;
					}
					if (processData.ImmovableTool.HasValue && sharedKnowledge.GetKnownDataAsEntity(processData.ImmovableTool.Value) == null)
					{
						return false;
					}
					if (processData.ImmovableInput.HasValue && sharedKnowledge.GetKnownDataAsEntity(processData.ImmovableTool.Value) == null)
					{
						return false;
					}
				}
				return true;
			}
		}
		return false;
	}

	private bool GetWorkLocation(out Vector3? location, ref IKnownProcess processData)
	{
		location = null;
		if (actingOnEntity.HasValue)
		{
			entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(actingOnEntity.Value.Entity, out var data);
			if (data != null)
			{
				location = data.AccessPoint;
				return true;
			}
			return false;
		}
		return job.GetCurrentJobLocation(out location, out processData);
	}

	public override string GetStatus()
	{
		if (ProcessType.IsSalvageProcess)
		{
			return "Salvaging";
		}
		return ProcessType.Name;
	}

	protected override bool ArePreconditionsOK()
	{
		if (!AreToolsOK(Tools))
		{
			return false;
		}
		IKnownEntityData data;
		if (job != null)
		{
			if (!job.GetContainerToPlaceOutputsIn(out var container))
			{
				return false;
			}
			if (container.HasValue && EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(container.Value, out data)))
			{
				return false;
			}
		}
		if (ActingOnEntity.HasValue)
		{
			if (!ActingOnEntity.Value.IsValid(entityIntelligence.Allegiance.SharedKnowledge, out data))
			{
				return false;
			}
			if (!entityIntelligence.Allegiance.SharedKnowledge.SpecialActionIsAvailable(data, ProcessType))
			{
				return false;
			}
			if (!ProcessType.AllowProcessOnBrokenTarget() && !Entity.IsFunctional(data))
			{
				return false;
			}
			bool? setPreparedProperty = ProcessType.SetPreparedProperty;
			bool flag = true;
			if (setPreparedProperty == true == flag && setPreparedProperty.HasValue && data.IsPrepared == true)
			{
				return false;
			}
		}
		if (!IsOutputOKAndNotCompleted(job, mustExist: false))
		{
			return false;
		}
		return true;
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
		if (base.Status == Status.Active && !isInRangeOfRemotelyStartedJob && CheckIfJobTargetCanBeSeenAndStarted())
		{
			isInRangeOfRemotelyStartedJob = true;
			RemoveAllSubgoals();
			AddDoProduceGoal();
		}
		if (base.Status == Status.Completed)
		{
			_ = job;
		}
		else if (base.Status == Status.Failed && job != null)
		{
			job.Abandon(entity);
		}
	}

	public double ScoreGoal()
	{
		return ScoreJobGoal(job, new ToolParams
		{
			Tools = Tools,
			ToolProductivity = GetToolCombinationProductivity(ToolTypeCombination),
			ReplenishStatus = null,
			JobDurationInDays = null
		});
	}

	public static float GetToolCombinationProductivity(ToolTypeCombination combination)
	{
		return combination?.Productivity ?? 1f;
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				if (job != null)
				{
					job.Abandon(entity);
					base.Status = Status.Failed;
					return true;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public override bool RequiresBoldStance()
	{
		return job.RequiresBoldStance;
	}

	public override void Deactivate()
	{
		if (job != null)
		{
			RemoveProcessToolLocks(job, Tools, replenishItemsForTools);
			ResetThreatStance(job);
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
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		OwnerOfProduct = sn.DoEnumNullable(OwnerOfProduct);
		Tools = sn.DoList(Tools);
		if (ToolTypeCombination != null)
		{
			snapshotToolCombo = ToolTypeCombination.ID;
		}
		snapshotToolCombo = sn.DoEnumNullable(snapshotToolCombo);
		replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		processType = sn.DoGameData(processType);
		actingOnEntity = sn.DoEntityAndRootNullable(actingOnEntity);
		replenishCompartmentToUse = sn.DoEnumNullable(replenishCompartmentToUse);
		isInRangeOfRemotelyStartedJob = sn.DoBool(isInRangeOfRemotelyStartedJob);
		sn.Ignore(job);
		sn.Ignore(ToolTypeCombination);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
		ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
	}
}
