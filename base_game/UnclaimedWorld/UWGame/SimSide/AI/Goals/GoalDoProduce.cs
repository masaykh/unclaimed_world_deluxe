using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalDoProduce : CompositeGoal, IIDEventSubscriber
{
	public ProcessJob Job;

	private JobID? snapshotJob;

	public SimProcessID ProductionProcess;

	public OwnerID? OwnerOfProduct;

	public ToolTypeCombination ToolTypeCombination;

	private ToolTypeCombinationID? snapshotToolCombo;

	public List<EntityID> Tools;

	private List<EntityID> intrinsicAndHandTools;

	private string ToolCombinationNameForDisplay;

	private List<AnimModifier> statesToTake = new List<AnimModifier>();

	private List<AnimModifier> statesToClear = new List<AnimModifier>();

	private MethodID processCompleteMethodID;

	private bool processWasCompleted;

	public const double UpdatesPerSecondForStanceChange = 1.0;

	private Regulator changeStanceRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		changeStanceRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GoalDoProduceChangeStance");
	}

	public GoalDoProduce(Entity entity, ProcessJob job, OwnerID? ownerOfProduct, List<EntityID> tools, ToolTypeCombination toolCombo)
		: base(entity)
	{
		Job = job;
		OwnerOfProduct = ownerOfProduct;
		ProductionProcess = job.ProductionProcess.Value;
		ToolTypeCombination = toolCombo;
		Tools = tools;
		GetIntrinsicAndHandTools(Tools, out intrinsicAndHandTools);
		CreateRegulators();
	}

	public GoalDoProduce(Entity entity, ProcessType processType, IKnownEntityData inputData, float? maxBulkToExtract, bool limitExtractionByNutrients, OwnerID? ownerOfProduct, List<EntityID> tools, ToolTypeCombination toolCombo, EntityID? placeProductsInContainer, StorageCompartment? placeProductsInCompartment, EntityAndRoot? actingOnEntity = null, ResourceItemID? resourceItem = null)
		: base(entity)
	{
		OwnerOfProduct = ownerOfProduct;
		Tools = tools;
		GetIntrinsicAndHandTools(Tools, out intrinsicAndHandTools);
		SimProcess simProcess = new SimProcess(processType, actingOnEntity);
		ProductionProcess = simProcess.ID;
		if (inputData != null)
		{
			simProcess.AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
			simProcess.AssignedInputs.Add(inputData.EntityType, new List<Tuple<EntityID, WorldLocation>>
			{
				new Tuple<EntityID, WorldLocation>(inputData.EntityID, new WorldLocation(inputData.PlaySiteLocation))
			});
		}
		simProcess.ResourceItem = resourceItem;
		ToolTypeCombination = toolCombo;
		simProcess.MaxBulkToExtract = maxBulkToExtract;
		simProcess.LimitBulkExtractionByNutrients = limitExtractionByNutrients;
		simProcess.ContainerToPlaceOutputsIn = placeProductsInContainer;
		simProcess.PlaceProductsInCompartment = placeProductsInCompartment;
	}

	public GoalDoProduce(Entity entity, ProcessType processType, OwnerID? ownerOfProduct, IKnownEntityData inputData, StorageCompartment? placeProductsInCompartment, Vector3? inputLocationWhenProductionBegins, EntityAndRoot actingOnEntity)
		: base(entity)
	{
		OwnerOfProduct = ownerOfProduct;
		SimProcess simProcess = new SimProcess(processType, actingOnEntity);
		ProductionProcess = simProcess.ID;
		simProcess.AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
		simProcess.AssignedInputs.Add(inputData.EntityType, new List<Tuple<EntityID, WorldLocation>>
		{
			new Tuple<EntityID, WorldLocation>(inputData.EntityID, new WorldLocation(inputLocationWhenProductionBegins.Value))
		});
		simProcess.PlaceProductsInCompartment = placeProductsInCompartment;
	}

	public GoalDoProduce()
	{
	}

	protected override void Activate()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
		if (simProcess == null)
		{
			if (Job != null)
			{
				Job.Destroy(removeTakers: true, entity);
			}
			base.Status = Status.Failed;
			return;
		}
		List<IKnownEntityData> toolsData = null;
		if (!ResolveTools(Tools, ref toolsData))
		{
			return;
		}
		if (Job != null)
		{
			if (!Job.GetAreaIsCleared(out var areaIsClear))
			{
				base.Status = Status.Failed;
				return;
			}
			if (!areaIsClear)
			{
				base.Status = Status.Failed;
				return;
			}
		}
		if (CheckHandTools(toolsData, Job) && EmptyToolContainers(toolsData))
		{
			if (simProcess.ProcessType.IsReplenishProcess)
			{
				Entity actingOnEntity = GetActingOnEntity(simProcess);
				MountReplenishTarget(actingOnEntity);
			}
			else
			{
				MountTool(toolsData);
			}
			entity.AttachTriggers(simProcess.ProcessType.Triggers);
			base.Status = Status.Active;
			AddSubgoal(GoalDoProduceAtomic.GetGoal(entity, this));
			if (simProcess.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded)
			{
				simProcess.AssignWorker(entity, ToolTypeCombination);
			}
			else
			{
				AddSubgoal(new GoalWait(entity, 2.0));
			}
			entityIntelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, simProcess, this, out processCompleteMethodID);
		}
	}

	private void ProcessCompleted(IKnownProcess process)
	{
		processWasCompleted = true;
		if (process.ProcessType.IsConsumeProcess)
		{
			GoalEat.ConsumeStomachContents(entity);
		}
	}

	private Entity GetActingOnEntity(SimProcess process)
	{
		if (process.ActingOnEntity.HasValue)
		{
			return Entity.FindByID(process.ActingOnEntity.Value.Entity);
		}
		return null;
	}

	public static bool GetStationaryTools(List<EntityID> tools, out List<EntityID> stationaryTools)
	{
		stationaryTools = null;
		if (tools != null)
		{
			stationaryTools = new List<EntityID>();
			int num = tools.Count - 1;
			while (num >= 0)
			{
				Entity entity = Entity.FindByID(tools[num]);
				if (entity != null)
				{
					if (entity.EntityType.ToolType.ToolHandling == ToolHandlingType.Stationary)
					{
						stationaryTools.Add(entity.ID);
					}
					num--;
					continue;
				}
				return false;
			}
		}
		return true;
	}

	public static bool FilterTools(List<EntityID> tools, out List<EntityID> handTools, Predicate<Entity> predicate)
	{
		handTools = null;
		if (tools != null)
		{
			handTools = new List<EntityID>();
			bool result = true;
			for (int num = tools.Count - 1; num >= 0; num--)
			{
				Entity entity = Entity.FindByID(tools[num]);
				if (entity != null)
				{
					if (predicate(entity))
					{
						handTools.Add(entity.ID);
					}
				}
				else
				{
					result = false;
				}
			}
			return result;
		}
		return true;
	}

	public static bool GetImmobileTools(List<EntityID> tools, out List<EntityID> immobileTools)
	{
		return FilterTools(tools, out immobileTools, (Entity t) => ToolType.IsImmovable(t.EntityType));
	}

	public static bool GetMobileTools(List<EntityID> tools, out List<EntityID> mobileTools)
	{
		return FilterTools(tools, out mobileTools, (Entity t) => !ToolType.IsImmovable(t.EntityType));
	}

	public static bool GetHandTools(List<EntityID> tools, out List<EntityID> handTools)
	{
		return FilterTools(tools, out handTools, (Entity t) => t.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool);
	}

	public static bool GetIntrinsicAndHandTools(List<EntityID> tools, out List<EntityID> handTools)
	{
		return FilterTools(tools, out handTools, (Entity t) => t.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool || t.EntityType.ToolType.ToolHandling == ToolHandlingType.Intrinsic);
	}

	private bool EmptyToolContainers(List<IKnownEntityData> toolsData)
	{
		if (toolsData != null)
		{
			foreach (IKnownEntityData toolsDatum in toolsData)
			{
				if (toolsDatum is Entity entity)
				{
					if (entity.EntityType.ContainerType != null && entity.Contains is IHoldsProductionOutput holdsProductionOutput)
					{
						holdsProductionOutput.UncontainAllProductionOutput();
					}
					continue;
				}
				base.Status = Status.Failed;
				return false;
			}
		}
		return true;
	}

	public override void PerformWork(SimProcess process, float workedTimeInSeconds, float progressDelta)
	{
		UpdateClientToolInfo();
		PerformWorkerEffects(workedTimeInSeconds, progressDelta, process.ProcessType);
		SimProcess.WearDownTools(entity, workedTimeInSeconds, intrinsicAndHandTools, ToolTypeCombination);
	}

	private void PerformWorkerEffects(double workedTimeInSeconds, float progressDelta, ProcessType processType)
	{
		UseSkill(processType.RequiredSkillType);
		if (entity.EntityType.BiologicalType != null)
		{
			entity.BiologicalEntity.SatisfyNeeds(workedTimeInSeconds, progressDelta, processType.SatisfiesWorkerNeeds);
		}
	}

	private void UseSkill(SkillType newSkill)
	{
	}

	private void UpdateClientToolInfo()
	{
		if (ToolTypeCombination != null)
		{
			ToolCombinationNameForDisplay = "";
			for (int i = 0; i < ToolTypeCombination.Tools.Count; i++)
			{
				ToolCombinationNameForDisplay += ToolTypeCombination.Tools[i].Item1.Name;
				if (i != ToolTypeCombination.Tools.Count - 1)
				{
					ToolCombinationNameForDisplay += ", ";
				}
			}
		}
		else
		{
			ToolCombinationNameForDisplay = null;
		}
	}

	public override float? GetCurrentTotalProductivity()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
		if (simProcess != null && simProcess.ProcessType.RequiredSkillType != null)
		{
			return ProcessJob.GetTotalFactorProductivity(GetEnergyLevelFactor(), GetToolProductivity() ?? 1f, entityIntelligence.GetSkillProductionFactor(simProcess.ProcessType.RequiredSkillType));
		}
		return null;
	}

	private float GetEnergyLevelFactor()
	{
		float result = 1f;
		if (entity.EntityType.BiologicalType != null)
		{
			result = entity.BiologicalEntity.EnergyLevel;
		}
		return result;
	}

	public override float? GetToolProductivity()
	{
		if (ToolTypeCombination != null)
		{
			return ToolTypeCombination.Productivity;
		}
		return null;
	}

	public override string GetToolInUseName()
	{
		return ToolCombinationNameForDisplay;
	}

	public override string GetSkillInUseName()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
		if (simProcess != null && simProcess.ProcessType.RequiredSkillType != null)
		{
			return simProcess.ProcessType.RequiredSkillType.Name;
		}
		return null;
	}

	public override float? GetSkillProductivity()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
		if (simProcess != null && simProcess.ProcessType.RequiredSkillType != null)
		{
			return entityIntelligence.GetSkillProductionFactor(simProcess.ProcessType.RequiredSkillType);
		}
		return null;
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public override StealthFactor GetStealthFactor()
	{
		return StealthFactor.NotGood;
	}

	public override void OnExit()
	{
		base.OnExit();
		entityIntelligence.GetKnownProcessData(ProductionProcess, out var data);
		if (data != null)
		{
			ClearProcessAnimStates(data.ProcessType);
			if (Job == null && data is SimProcess simProcess)
			{
				simProcess.Destroy();
			}
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			Status status = ProcessSubgoals(elapsed);
			SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
			float progress2;
			if (simProcess == null)
			{
				if (!processWasCompleted)
				{
					base.Status = Status.Failed;
				}
				else
				{
					base.Status = Status.Completed;
				}
			}
			else if (status == Status.Completed)
			{
				if (simProcess.IsStarted && simProcess.ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded)
				{
					base.Status = Status.Completed;
				}
				else
				{
					if (!ValidateSafetyAndTakeAction(GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue))
					{
						return;
					}
					if (!simProcess.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out var progress))
					{
						base.Status = Status.Failed;
					}
					else if (!NonLivingEntity.IsCompleted(progress))
					{
						if (entityIntelligence.Brain.ArbitrateWhileBusy(out var _))
						{
							HandleSubstitutedGoalByArbitrator();
							return;
						}
						HandleStanceChange(changeStanceRegulator, simProcess.ProcessType.StanceTypes);
						AddSubgoal(GoalDoProduceAtomic.GetGoal(entity, this));
						base.Status = Status.Active;
					}
					else
					{
						base.Status = Status.Completed;
					}
				}
			}
			else if (!simProcess.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out progress2))
			{
				base.Status = Status.Failed;
			}
			else if (NonLivingEntity.IsCompleted(progress2))
			{
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = status;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override bool ArePreconditionsOK()
	{
		if (Tools != null)
		{
			return AreToolsOK(entity, Tools);
		}
		return true;
	}

	public override void Deactivate()
	{
		entityIntelligence.GetKnownProcessData(ProductionProcess, out var data);
		if (data != null)
		{
			if (data.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded && data is SimProcess simProcess)
			{
				simProcess.RemoveWorker(entity);
			}
			if (entity.AgentStorage != null && !data.ProcessType.IsReplenishProcess)
			{
				entity.AgentStorage.MountedToolOrWeapon = null;
			}
			entity.DeleteTriggers(data.ProcessType.Triggers);
		}
		ActionLookup<List<EntityID>>.Remove(processCompleteMethodID);
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
		snapshotJob = sn.SnapshotID<Job, JobID>(Job);
		OwnerOfProduct = sn.DoEnumNullable(OwnerOfProduct);
		snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
		Tools = sn.DoList(Tools);
		statesToTake = sn.DoList(statesToTake);
		statesToClear = sn.DoList(statesToClear);
		processCompleteMethodID = sn.DoEnum(processCompleteMethodID);
		processWasCompleted = sn.DoBool(processWasCompleted);
		ProductionProcess = sn.DoEnum(ProductionProcess);
		ToolCombinationNameForDisplay = sn.DoString(ToolCombinationNameForDisplay);
		intrinsicAndHandTools = sn.DoList(intrinsicAndHandTools);
		sn.Ignore(Job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			Job = (ProcessJob)LookUp<UWGame.SimSide.Jobs.Job, JobID>.FindByID(snapshotJob);
		}
		ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
		LoadPostProcessRegisterMethodIDs();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
		if (simProcess != null)
		{
			entityIntelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, simProcess, this, out processCompleteMethodID);
		}
	}
}
