using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class ProcessJob : Job, IIDEventSubscriber
{
	public BuildingJob BuildingJob;

	public ReplenishJob ReplenishJob;

	public SimProcessID? ProductionProcess;

	public JobID? CheckingJobID;

	public Dictionary<EntityType, List<EntityID>> InputsBeingHauled = new Dictionary<EntityType, List<EntityID>>();

	public List<EntityID> AssignedTools = new List<EntityID>();

	public float CumulativelyEstimatedProgress;

	public ProcessType ProcessType;

	private Collidable<Entity> UpgradeFootprint;

	public bool AllToolsInUse;

	public bool AllToolsAreBroken;

	private MethodID processCompleteMethodID;

	private MethodID processDestroyedMethodID;

	private MethodID processStartedMethodID;

	private MethodID? processProducingMethodID;

	private Regulator areaIsClearedRegulator;

	private bool? areaIsCleared;

	private JobID? oldID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public HarvestJob HarvestJob { get; private set; }

	public SalvageJob SalvageJob { get; private set; }

	public RepairJob RepairJob { get; private set; }

	public double? EstimatedCompletion { get; private set; }

	public override Priority Priority
	{
		get
		{
			return base.Priority;
		}
		set
		{
			if (value != base.Priority)
			{
				base.Priority = value;
				IterateHaulingJobs(delegate(Job j)
				{
					j.Priority = value;
				});
			}
		}
	}

	public EntityType OutputEntityType { get; private set; }

	public bool ToolsAreAvailable
	{
		get
		{
			if (!AllToolsAreBroken)
			{
				return !AllToolsInUse;
			}
			return false;
		}
	}

	public override int MaxJobPositions => ProcessType.MaxWorkers;

	public override bool RequiresBoldStance => ProcessType.RequiresBoldStance;

	public float GetImportance(EntityGroup owner)
	{
		if (ProcessType.HasOutput)
		{
			Output output = ProcessType.Outputs.FirstOrDefault((Output o) => !o.IsWasteProduct);
			if (output != null)
			{
				EntityType finalEntityTypeToCreate = output.FinalEntityTypeToCreate;
				return owner.GetImportance(finalEntityTypeToCreate);
			}
		}
		return 0.5f;
	}

	public ProcessJob()
	{
	}

	public ProcessJob(EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, EntityAndRoot? actingOn = null, bool isSalvage = false, UpgradeCategory upgradeCategory = null)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Init(productionContainer, processType, outputEntityType, entityGroup, actingOn, upgradeCategory);
		if (isSalvage)
		{
			SalvageJob = new SalvageJob(this);
		}
		entityGroup.AddJob(this);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public ProcessJob(EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, RepairAction repairAction, EntityAndRoot actingOn, EntityAndRoot? partToFix)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Init(productionContainer, processType, outputEntityType, entityGroup, partToFix ?? actingOn, null);
		RepairJob = new RepairJob(actingOn.Entity, repairAction, partToFix);
		entityGroup.AddJob(this);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public ProcessJob(ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, IResourceItem resourceItem)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Init(null, processType, outputEntityType, entityGroup, null, null);
		HarvestJob = new HarvestJob(this, resourceItem, entityGroup.ID);
		entityGroup.AddJob(this);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	private void Init(EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, EntityAndRoot? actingOn, UpgradeCategory upgradeCategory)
	{
		SimProcess simProcess = new SimProcess(processType, actingOn);
		ProductionProcess = simProcess.ID;
		simProcess.ContainerToPlaceOutputsIn = productionContainer;
		simProcess.UpgradeCategory = upgradeCategory;
		SharedKnowledge sharedKnowledge = entityGroup.GetAllegiance().SharedKnowledge;
		sharedKnowledge.PlaySiteKnowledge.RegisterProcessDestroyedEvent(ProcessDestroyed, simProcess, this, out processDestroyedMethodID);
		sharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, simProcess, this, out processCompleteMethodID);
		sharedKnowledge.PlaySiteKnowledge.RegisterProcessStartedEvent(ProcessStarted, simProcess, this, out processStartedMethodID);
		OutputEntityType = outputEntityType;
		ProcessType = processType;
		if (IsUnattended())
		{
			sharedKnowledge.PlaySiteKnowledge.RegisterProcessProducingEvent(ProcessProducing, simProcess, this, out var methodID);
			processProducingMethodID = methodID;
		}
		CreateRegulators();
	}

	public double? GetEstimatedCompletionTime()
	{
		return EstimatedCompletion;
	}

	private void ComputeEstimatedCompletionTime(IKnownProcess knownProcess)
	{
		if (knownProcess is SimProcess simProcess)
		{
			EstimatedCompletion = simProcess.ComputeEstimatedCompletionTime();
		}
	}

	public float GetProgressSpeed()
	{
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			return process.ProgressSpeed;
		}
		return 0f;
	}

	private void CreateRegulators()
	{
		areaIsClearedRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "ProcessJob");
	}

	public bool GetAreaIsCleared(out bool areaIsClear)
	{
		if (BuildingJob == null && !GetIsUpgradeJob())
		{
			areaIsClear = true;
			return true;
		}
		if (!areaIsCleared.HasValue || areaIsClearedRegulator.IsReady())
		{
			if (!GetCollidableForOutput(out var collidable, out var pad, out var _))
			{
				areaIsClear = false;
				return false;
			}
			if (collidable != null)
			{
				areaIsCleared = !collidable.ShapesContainEntities(pad, (Entity e) => e.EntityType.ItemType != null);
			}
			else
			{
				areaIsCleared = areaIsCleared ?? true;
			}
		}
		areaIsClear = areaIsCleared.Value;
		return true;
	}

	private bool GetCollidableForOutput(out Collidable<Entity> collidable, out float pad, out Entity structureEntity)
	{
		pad = 0f;
		structureEntity = null;
		collidable = null;
		if (BuildingJob != null)
		{
			if (!GetStructureOutputData(out var outputData))
			{
				return false;
			}
			structureEntity = outputData as Entity;
			if (structureEntity != null && structureEntity.CurrentSimState != null)
			{
				pad = structureEntity.CurrentSimState.GeometryLayoutType.Pad;
				collidable = structureEntity.Collidable;
				return true;
			}
		}
		else
		{
			GetIsUpgradeJob();
		}
		return true;
	}

	public bool GetAssignedInputs(out Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs)
	{
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			inputs = process.AssignedInputs;
			return true;
		}
		inputs = null;
		return false;
	}

	protected override void ComputeJobType()
	{
		foreach (KeyValuePair<string, JobType> allJobType in GameData.Instance.AllJobTypes)
		{
			if (allJobType.Value is ProcessJobType && allJobType.Value.IsType(this))
			{
				jobType = allJobType.Value;
				return;
			}
		}
		foreach (KeyValuePair<string, JobType> allJobType2 in GameData.Instance.AllJobTypes)
		{
			if (allJobType2.Value is StaticJobType && allJobType2.Value.IsType(this))
			{
				jobType = allJobType2.Value;
				break;
			}
		}
	}

	public override Vector3? GetCircaLocation()
	{
		GetCurrentJobLocation(out var location);
		return location;
	}

	public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tile = null;
		zoneID = null;
		targetEntity = null;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (ownerIsDestroyed || processIsDestroyed)
		{
			return;
		}
		if (process.ActingOnEntity.HasValue)
		{
			targetEntity = process.ActingOnEntity.Value.Entity;
			return;
		}
		if (HarvestJob != null && HarvestJob.Zone != null)
		{
			zoneID = HarvestJob.Zone.ID;
			return;
		}
		if (process.ImmovableInput.HasValue)
		{
			targetEntity = process.ImmovableInput;
			return;
		}
		if (process.ImmovableTool.HasValue)
		{
			targetEntity = process.ImmovableTool;
			return;
		}
		process.GetProductionSiteLocation(out var location, sharedKnowledge);
		if (location.HasValue)
		{
			tile = MapManager.WorldPosToTile(location.Value);
		}
	}

	public bool RequiresOwnedActingOnEntity()
	{
		if (SalvageJob != null || RepairJob != null || ReplenishJob != null || GetIsUpgradeJob())
		{
			return true;
		}
		return false;
	}

	public float GetWorstCaseDurationForEnergyEstimation()
	{
		float timeNeeded = ProcessType.GetTimeNeeded();
		return GameData.Instance.AIConstants.WorkTimeFactorToEvaluateToolEnergyUse * timeNeeded;
	}

	public bool GetStructureOutputData(out IKnownEntityData outputData)
	{
		outputData = null;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed && process.OutputEntities != null)
		{
			foreach (EntityID outputEntity in process.OutputEntities)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(outputEntity, out var data)))
				{
					if (data.EntityType.StructureType != null)
					{
						outputData = data;
						return true;
					}
					continue;
				}
				return false;
			}
		}
		return true;
	}

	public bool IsStarted(out bool isStarted, out IKnownProcess processData)
	{
		isStarted = false;
		ResolveProcess(out processData, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			isStarted = processData.IsStarted;
			return true;
		}
		return false;
	}

	public bool IsStarted(out bool isStarted)
	{
		IKnownProcess processData;
		return IsStarted(out isStarted, out processData);
	}

	public bool IsUnattended()
	{
		return ProcessType.WorkNeeded == WorkerNeededOptions.WorkerOnlyNeededToStart;
	}

	public static double GetMarginalLaborReturn(int proposedNumber, int maxNumber)
	{
		if (maxNumber > 1)
		{
			proposedNumber--;
			maxNumber--;
			double x = (double)proposedNumber / (double)maxNumber;
			return 1.0 - 0.3 * Math.Pow(x, 2.0);
		}
		return 1.0;
	}

	public static double GetAverageLaborReturn(int proposedNumber, int maxNumber)
	{
		if (proposedNumber == 0)
		{
			return 1.0;
		}
		double num = 0.0;
		for (int i = 1; i <= proposedNumber; i++)
		{
			num += GetMarginalLaborReturn(i, maxNumber);
		}
		return num / (double)proposedNumber;
	}

	private float CalculateProgressDelta(double seconds, Entity worker, float manSecondsOfWorkNeeded, SkillType skill, IKnownProcess processData, float? toolProductivityFactor = 1f)
	{
		float averageLaborEfficiency = 1f;
		if (processData is SimProcess { Workers: not null } simProcess)
		{
			averageLaborEfficiency = (float)GetAverageLaborReturn(simProcess.Workers.Count, ProcessType.MaxWorkers);
		}
		float num = CalculateWorkDurationInDays(worker, manSecondsOfWorkNeeded, skill, toolProductivityFactor, averageLaborEfficiency);
		return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / (double)num);
	}

	public static float CalculateWorkDurationInDays(Entity worker, float workTimeNeeded, SkillType skill, float? toolProductivityFactor = 1f, float averageLaborEfficiency = 1f)
	{
		float energyLevelFactor = 1f;
		if (worker.BiologicalEntity != null)
		{
			energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyProductionFactor, 1f, worker.BiologicalEntity.EnergyLevel);
		}
		float totalFactorProductivity = GetTotalFactorProductivity(energyLevelFactor, toolProductivityFactor.Value, worker.Intelligence.GetSkillProductionFactor(skill), averageLaborEfficiency);
		return workTimeNeeded / totalFactorProductivity;
	}

	public static float GetTotalFactorProductivity(float energyLevelFactor, float toolFactor, float skillFactor, float averageLaborEfficiency = 1f)
	{
		return Common.ClampBottom(energyLevelFactor * skillFactor, GameData.Instance.Constants.LowestCombinedSkillAndEnergyProductionFactors) * toolFactor * averageLaborEfficiency;
	}

	public float CalculateProgressDelta(double seconds, float timeNeeded, float? toolProductivityFactor = 1f)
	{
		return Common.CalculateProgressDelta(seconds, timeNeeded) * toolProductivityFactor.Value;
	}

	public void AssignTool(IKnownEntityData toolData)
	{
		toolData.AssignedToJob = base.ID;
		if (!AssignedTools.Contains(toolData.EntityID))
		{
			AssignedTools.Add(toolData.EntityID);
			AddLog("Assigned tool: " + toolData.EntityType.KeyName + ", " + toolData.EntityID);
		}
	}

	public void UnassignTool(IKnownEntityData toolData)
	{
		if (toolData.AssignedToJob == base.ID)
		{
			toolData.AssignedToJob = null;
			AddLog("Unassigned tool: " + toolData.EntityType.KeyName + ", " + toolData.EntityID);
		}
		AssignedTools.Remove(toolData.EntityID);
	}

	public void AssignInput(IKnownEntityData item)
	{
		SimProcess.FindById(ProductionProcess).AssignInput(item, out var wasAssigned);
		if (wasAssigned)
		{
			item.AssignedToJob = base.ID;
			AddLog("Assigned input: " + item.EntityType.KeyName + ", " + item.EntityID);
		}
		else
		{
			AddLog("Failed to assign input: " + item.EntityType.KeyName + ", " + item.EntityID);
		}
	}

	public bool GetProductionSiteLocation(out Vector3? location)
	{
		location = null;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			return process.GetProductionSiteLocation(out location, sharedKnowledge);
		}
		return false;
	}

	public bool GetFixedJobLocation(out Vector3? location)
	{
		location = null;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			return process.GetFixedJobLocation(out location, sharedKnowledge);
		}
		return false;
	}

	public bool GetActingOnEntity(out EntityID? actingOnEntity)
	{
		EntityAndRoot? actingOnEntity2 = null;
		actingOnEntity = null;
		if (GetActingOnEntity(out actingOnEntity2))
		{
			actingOnEntity = EntityAndRoot.GetEntity(actingOnEntity2);
			return true;
		}
		return false;
	}

	public bool GetActingOnEntity(out EntityAndRoot? actingOnEntity)
	{
		actingOnEntity = null;
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			actingOnEntity = process.ActingOnEntity;
			return true;
		}
		return false;
	}

	public bool GetImmovableTool(out EntityID? tool)
	{
		tool = null;
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			tool = process.ImmovableTool;
			return true;
		}
		return false;
	}

	public bool GetImmovableInput(out EntityID? input, out IKnownProcess processData)
	{
		input = null;
		ResolveProcess(out processData, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			input = processData.ImmovableInput;
			return true;
		}
		return false;
	}

	public bool GetCurrentJobLocation(out Vector3? location)
	{
		IKnownProcess processData = null;
		return GetCurrentJobLocation(out location, out processData);
	}

	public bool GetCurrentJobLocation(out Vector3? location, out IKnownProcess processData)
	{
		location = null;
		processData = null;
		ResolveProcess(out processData, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			if (!processData.GetCurrentLocation(out location, sharedKnowledge))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private GoalEvaluator.CalculateResult FindInputItemForProcessJobAndScoreIt(RegionMap regionMapToUse, ThreatStance threatStance, EntityGroup ownerOfInputItems, Entity entity, out IKnownEntityData foundInputItem)
	{
		IKnownEntityData knownEntityData = null;
		foundInputItem = null;
		SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
		float num = 1000000f;
		if (ProcessType.Inputs != null && ProcessType.Inputs.Length == 1)
		{
			Input input = ProcessType.Inputs[0];
			if (input.InputIsImmovable() && ownerOfInputItems.Items.TryGetValue(input.EntityType, out var value))
			{
				float distance = 0f;
				for (int num2 = value.Count - 1; num2 >= 0; num2--)
				{
					EntityID entityID = value[num2];
					GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, ownerOfInputItems, out var entityData);
					if (entityData != null && entityData.IsUnassignedToAnythingButThisJob(this, sharedKnowledge) && GoalEvaluator.IsOnPlaySite(entityData) && entityData.IsCompleted() && (!Item.IsImmovable(entityData.Bulk) || !entityData.ContainedBy.HasValue) && GoalEvaluator.WorkSiteIsSafe(entity, entityData.PlaySiteLocation, threatStance))
					{
						switch (regionMapToUse.GetDistanceToEntity(entity, entity, entityData, ref distance))
						{
						case RegionMap.Result.OK:
							if (distance < num)
							{
								knownEntityData = entityData;
								num = distance;
							}
							break;
						default:
							return GoalEvaluator.CalculateResult.Processing;
						case RegionMap.Result.NoAccess:
							break;
						}
					}
				}
			}
		}
		foundInputItem = knownEntityData;
		return GoalEvaluator.CalculateResult.Done;
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutTools(RegionMap regionMap, ThreatStance threatStance, Entity entity, Intelligence entityIntelligence, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, ref IKnownEntityData foundInputItem, EntityGroup ownerOfInputItems, float priority, ref bool jobIsValid)
	{
		ProcessType.KeyName.Contains("alvage");
		if (entityIntelligence.HasSkill(ProcessType.RequiredSkillType))
		{
			if (!GetCurrentJobLocation(out var location, out var processData))
			{
				jobIsValid = false;
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			if (processData.IsStarted && ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded)
			{
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			foundInputItem = null;
			if (!location.HasValue && !processData.HasFixedLocation() && ProcessType.NeedsImmovableInput())
			{
				if (FindInputItemForProcessJobAndScoreIt(regionMap, threatStance, ownerOfInputItems, entity, out foundInputItem) != GoalEvaluator.CalculateResult.Done)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Processing;
				}
				if (foundInputItem == null)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				AssignImmovableInput(foundInputItem);
			}
			SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
			if (!processData.GetKnownProgress(sharedKnowledge, out var progress))
			{
				jobIsValid = false;
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			if (processData.ActingOnEntity.HasValue)
			{
				if (!processData.ActingOnEntity.Value.IsValid(sharedKnowledge, out var actingOnEntityData))
				{
					jobIsValid = false;
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (!ProcessType.AllowProcessOnBrokenTarget() && !Entity.IsFunctional(actingOnEntityData))
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (SalvageJob != null && actingOnEntityData.ContainedUpgrades != null && actingOnEntityData.ContainedUpgrades.Count > 0)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
			}
			double num = EstimatePowerAndMaterialsReady(entity, sharedKnowledge, foundInputItem, processData, progress);
			if (num == 0.0)
			{
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			double score = 1.0;
			if (!processData.IsStarted)
			{
				if (!ScoreConstructionSiteIsCleared(out score))
				{
					jobIsValid = false;
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (Common.IsZero(score))
				{
					rating = 0.0;
					EvaluateJob.SetAreaNotCleared(this, value: true);
					return GoalEvaluator.CalculateResult.Done;
				}
				EvaluateJob.SetAreaNotCleared(this, value: false);
			}
			double num2 = GoalEvaluator.ScoreJobProgress(progress);
			double num3 = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);
			double num4 = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(ProcessType.RequiredSkillType));
			double num5 = GoalEvaluator.ScoreUniqueSkill(entityIntelligence, ProcessType.RequiredSkillType);
			double num6 = GetImportance(ownerOfInputItems);
			if (Common.IsEqual(num, 0.0))
			{
				rating = GameData.Instance.AIConstants.JobWithZeroMaterialsDesirability;
			}
			else
			{
				EvaluatorWeights evaluatorWeights = GameData.Instance.AIConstants.EvaluatorWeights;
				double num7 = 0.3 * num + (double)evaluatorWeights.ProcessProgressWeight * num2 + 0.1 * num3 + (double)evaluatorWeights.ProcessImportanceWeight * num6 + (double)evaluatorWeights.ProcessSkillWeight * num4 + (double)evaluatorWeights.UniqueSkillWeight * num5;
				rating = num7;
				rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
			}
		}
		else
		{
			rating = 0.0;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private bool ScoreConstructionSiteIsCleared(out double score)
	{
		score = 1.0;
		if (GetAreaIsCleared(out var areaIsClear))
		{
			if (areaIsClear)
			{
				score = 1.0;
			}
			else
			{
				score = 0.0;
			}
			return true;
		}
		score = 0.0;
		return false;
	}

	public void AssignImmovableInput(IKnownEntityData entityData)
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
		GetCurrentJobLocation(out var location);
		if (entityData != null)
		{
			AssignInput(entityData);
		}
		else
		{
			simProcess.ImmovableInput = null;
		}
		if (GetCurrentJobLocation(out var location2))
		{
			HandleJobLocationChange(location, location2);
		}
	}

	public void AssignImmovableTool(IKnownEntityData entityData)
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
		GetCurrentJobLocation(out var location);
		if (entityData != null)
		{
			simProcess.ImmovableTool = entityData.EntityID;
			AssignTool(entityData);
		}
		else
		{
			simProcess.ImmovableTool = null;
		}
		simProcess.GroundLocation = null;
		if (GetCurrentJobLocation(out var location2))
		{
			HandleJobLocationChange(location, location2);
		}
	}

	public void AssignJobLocation(Vector3 newLocation)
	{
		SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
		simProcess.ImmovableTool = null;
		GetProductionSiteLocation(out var location);
		simProcess.GroundLocation = newLocation;
		if (GetCurrentJobLocation(out var location2))
		{
			HandleJobLocationChange(location, location2);
		}
	}

	private void HandleJobLocationChange(Vector3? previousLocation, Vector3? location)
	{
		if ((previousLocation.HasValue && !location.HasValue) || (!previousLocation.HasValue && location.HasValue) || (previousLocation.HasValue && location.HasValue && (double)Common.DistanceOctile(location.Value, previousLocation.Value) > 0.5))
		{
			CancelHaulingJobs();
			CancelAllTakers(null);
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
			if (location.HasValue)
			{
				CreateHaulingJobsForProcessInputs(entityGroup, location.Value);
			}
		}
	}

	public GoalEvaluator.CalculateResult ScoreTools(RegionMap regionMap, Entity entity, Intelligence entityIntelligence, double? ageContribution, double? timeContribution, Vector3? location, ToolParams toolParams, ref Vector3? temporaryGroundLocation, ref double rating, float priority)
	{
		double totalToolsScore = 1.0;
		if (ScoreToolSet(entity, toolParams, entity.Location.Value, location, ref temporaryGroundLocation, entityIntelligence.Allegiance.SharedKnowledge, regionMap, out totalToolsScore) == GoalEvaluator.CalculateResult.Processing)
		{
			return GoalEvaluator.CalculateResult.Processing;
		}
		if (Common.IsZero(totalToolsScore))
		{
			rating = 0.0;
			return GoalEvaluator.CalculateResult.Done;
		}
		rating = totalToolsScore;
		rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		return GoalEvaluator.CalculateResult.Done;
	}

	public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref EvaluateJob.ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams, ref double rating, ref bool jobIsValid, bool cacheScore, Dictionary<Job, EvaluateJob.ToolOrWeaponInstanceComboJobData> cachedJobScores, Intelligence entityIntelligence, float priority, EntityGroup ownerOfJobs)
	{
		if (!cachedJobScores.TryGetValue(this, out jobData))
		{
			jobData = new EvaluateJob.ToolOrWeaponInstanceComboJobData();
			GoalEvaluator.CalculateResult calculateResult = GoalEvaluator.CalculateResult.Done;
			calculateResult = ((HarvestJob == null) ? ScoreThisJobWithoutTools(regionMap, threatStanceToUse, entity, entityIntelligence, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData.JobScore, ref jobData.InputItem, ownerOfJobs, priority, ref jobIsValid) : HarvestJob.ScoreThisJobWithoutTools(regionMap, threatStanceToUse, entity, entityIntelligence, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData.JobScore, ref jobData.ResourceItem, ownerOfJobs, priority, ref jobIsValid));
			if (!jobIsValid)
			{
				return GoalEvaluator.CalculateResult.Done;
			}
			if (calculateResult != GoalEvaluator.CalculateResult.Done)
			{
				return GoalEvaluator.CalculateResult.Processing;
			}
			if (cacheScore)
			{
				cachedJobScores.Add(this, jobData);
			}
			EvaluateJob.SetDebugScoreNoTools(entity, this, jobData.JobScore);
		}
		Vector3? location = null;
		if (HarvestJob != null)
		{
			if (jobData.ResourceItem != null)
			{
				location = jobData.ResourceItem.Container.AccessPoint;
			}
		}
		else if (jobData.InputItem != null)
		{
			location = jobData.InputItem.AccessPoint;
		}
		else if (!GetCurrentJobLocation(out location))
		{
			jobIsValid = false;
			return GoalEvaluator.CalculateResult.Done;
		}
		if ((!location.HasValue && HarvestJob == null) || jobData.JobScore > 0.0)
		{
			double rating2 = 1.0;
			if (toolParams.HasValue && toolParams.Value.Tools.Count > 0)
			{
				if (ScoreTools(regionMap, entity, entityIntelligence, ageContribution, timeContribution, location, toolParams.Value, ref jobData.TemporaryGroundLocation, ref rating2, priority) == GoalEvaluator.CalculateResult.Processing)
				{
					return GoalEvaluator.CalculateResult.Processing;
				}
				if (!location.HasValue)
				{
					double valueToTestWith = jobData.HighestToolScore ?? 0.0;
					if (Common.IsGreaterThan(rating2, valueToTestWith))
					{
						jobData.HighestToolScore = rating2;
						jobData.HighestScoringToolCombo = toolParams.Value.Tools;
					}
				}
			}
			if (location.HasValue)
			{
				EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
				if (entityGroup == null)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				double travelTimeScore = 0.0;
				switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, location.Value, entity, ref travelTimeScore, this, entityGroup.Parent))
				{
				case RegionMap.Result.Wait:
					return GoalEvaluator.CalculateResult.Processing;
				case RegionMap.Result.NoAccess:
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (ProcessType.WorkNeeded == WorkerNeededOptions.StartRemotely)
				{
					travelTimeScore = 1.0;
				}
				rating = CombineJobAndToolScore(jobData.JobScore, rating2, travelTimeScore);
				EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating);
			}
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private double CombineJobAndToolScore(double jobScore, double totalToolsScore, double travelScore)
	{
		if (Common.IsZero(jobScore) || Common.IsZero(totalToolsScore) || Common.IsZero(travelScore))
		{
			return 0.0;
		}
		EvaluatorWeights evaluatorWeights = GameData.Instance.AIConstants.EvaluatorWeights;
		if (HarvestJob != null)
		{
			return (double)evaluatorWeights.HarvestJobScoreWeight * jobScore + (double)evaluatorWeights.HarvestTravelScoreWeight * travelScore + (double)evaluatorWeights.HarvestToolScoreWeight * totalToolsScore;
		}
		return (double)evaluatorWeights.ProcessJobScoreWeight * jobScore + (double)evaluatorWeights.ProcessTravelScoreWeight * travelScore + (double)evaluatorWeights.ProcessToolScoreWeight * totalToolsScore;
	}

	private GoalEvaluator.CalculateResult ScoreToolSet(Entity entity, ToolParams toolParams, Vector3 fromLocation, Vector3? jobLocation, ref Vector3? temporaryGroundLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, out double totalToolsScore)
	{
		totalToolsScore = 1.0;
		if (toolParams.Tools.Count > 0)
		{
			totalToolsScore = 0.0;
			float num = 10000000f;
			IKnownEntityData knownEntityData = null;
			if (!toolParams.JobDurationInDays.HasValue)
			{
				toolParams.JobDurationInDays = CalculateWorkDurationInDays(entity, ProcessType.GetTimeNeeded(), ProcessType.RequiredSkillType, toolParams.ToolProductivity);
			}
			bool needsToAssignImmovableTool = false;
			IKnownEntityData data;
			if (!jobLocation.HasValue)
			{
				if (toolParams.ImmovableTool.HasValue)
				{
					if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(toolParams.ImmovableTool.Value, out data)))
					{
						totalToolsScore = 0.0;
						return GoalEvaluator.CalculateResult.Done;
					}
					needsToAssignImmovableTool = true;
					jobLocation = data.AccessPoint;
				}
				else
				{
					if (ProcessType.NeedsImmovableInput())
					{
						totalToolsScore = 0.0;
						return GoalEvaluator.CalculateResult.Done;
					}
					if (!temporaryGroundLocation.HasValue)
					{
						Expedition hasEntityGroup = (Expedition)LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID).Parent;
						temporaryGroundLocation = JobManager.FindProductionLocation(hasEntityGroup);
					}
					jobLocation = temporaryGroundLocation.Value;
				}
			}
			foreach (EntityID tool in toolParams.Tools)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(tool, out data)))
				{
					totalToolsScore = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (ScoreTool(entity, data, entity.PlaySiteLocation, jobLocation.Value, sharedKnowledge, regionMap, toolParams.JobDurationInDays.Value, toolParams.ReplenishStatus, needsToAssignImmovableTool, out var distanceToTool, out var score) == GoalEvaluator.CalculateResult.Processing)
				{
					return GoalEvaluator.CalculateResult.Processing;
				}
				if (Common.IsZero(score))
				{
					totalToolsScore = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				if (distanceToTool < num && !ToolType.IsImmovable(data.EntityType))
				{
					num = distanceToTool;
					knownEntityData = data;
				}
				totalToolsScore += score;
			}
			double num2 = ScoreToolProductivity(toolParams.ToolProductivity.Value);
			if (knownEntityData != null)
			{
				toolParams.Tools.Remove(knownEntityData.EntityID);
				toolParams.Tools.Insert(0, knownEntityData.EntityID);
			}
			totalToolsScore = 0.5 * (totalToolsScore / (double)toolParams.Tools.Count) + 0.5 * num2;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private double ScoreToolEnergy(Entity entity, IKnownEntityData tool, float durationInDays)
	{
		if (tool.HasEnergyForDuration(durationInDays))
		{
			return 1.0;
		}
		return 0.0;
	}

	private GoalEvaluator.CalculateResult ScoreTool(Entity entity, IKnownEntityData tool, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, float durationInDays, Dictionary<EntityType, ReplenishStatus> replenishStatus, bool needsToAssignImmovableTool, out float distanceToTool, out double score)
	{
		distanceToTool = 0f;
		score = 0.0;
		double num = 1.0;
		if (tool.EntityType.ContainerType != null && tool.EntityType.ContainerType.GetRequiresReplenishType() != null)
		{
			num = ScoreToolEnergy(entity, tool, durationInDays);
			if (Common.IsZero(num))
			{
				if (replenishStatus != null)
				{
					if (!replenishStatus.TryGetValue(tool.EntityType, out var value))
					{
						return GoalEvaluator.CalculateResult.Done;
					}
					if (value.OwnsItem != true)
					{
						return GoalEvaluator.CalculateResult.Done;
					}
					num = 0.1;
				}
				else
				{
					num = 0.1;
				}
			}
		}
		if (ScoreToolLocation(entity, tool, fromLocation, jobLocation, sharedKnowledge, regionMap, out distanceToTool, out var score2) == GoalEvaluator.CalculateResult.Processing)
		{
			return GoalEvaluator.CalculateResult.Processing;
		}
		double num2 = ScoreToolCondition(tool);
		double num3 = ScoreNeededImmovableTool(tool, needsToAssignImmovableTool);
		if (Common.IsZero(num) || Common.IsZero(score2) || Common.IsZero(num2))
		{
			score = 0.0;
		}
		else
		{
			score = 0.699999988079071 * score2 + 0.1 * num3 + 0.10000000149011612 * num2 + 0.10000000149011612 * num;
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private double ScoreNeededImmovableTool(IKnownEntityData tool, bool needsToAssignImmovableTool)
	{
		if (needsToAssignImmovableTool && ToolType.IsImmovable(tool.EntityType) && !tool.AssignedToJob.HasValue)
		{
			return 1.0;
		}
		return 0.0;
	}

	public static bool ScoreIsCarriedToolLocation(Entity entity, IKnownEntityData tool, out double score)
	{
		score = 0.0;
		if (entity.Intelligence.IntrinsicTools != null && entity.Intelligence.IntrinsicTools.TryGetValue(tool.EntityType, out var value) && tool.EntityID == value)
		{
			score = 1.0;
			return true;
		}
		if (entity.AgentStorage != null)
		{
			if (entity.AgentStorage.MountedToolOrWeapon == tool.EntityID)
			{
				score = 1.0;
				return true;
			}
			if (entity.AgentStorage.ItemStorage.Contains(tool.EntityID))
			{
				score = 0.99;
				return true;
			}
			if (entity.AgentStorage.Equipment != null && entity.AgentStorage.Equipment.Contains(tool.EntityID))
			{
				score = 0.98;
				return true;
			}
		}
		return false;
	}

	public static GoalEvaluator.CalculateResult ScoreToLocation(Entity entity, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, out double score, float maxRange)
	{
		score = 0.0;
		MapManager.WorldPosToSubtile(fromLocation);
		float distance = -1f;
		Point value = MapManager.WorldPosToSubtile(jobLocation);
		switch (regionMap.GetDistanceToEntity(entity, entity, null, ref distance, null, value))
		{
		case RegionMap.Result.NoAccess:
			return GoalEvaluator.CalculateResult.Done;
		case RegionMap.Result.Wait:
			return GoalEvaluator.CalculateResult.Processing;
		default:
		{
			double evaluatorTimeCostOfDistance = GoalEvaluator.GetEvaluatorTimeCostOfDistance(entity, distance - maxRange);
			score = GoalEvaluator.ScoreTravelTime(evaluatorTimeCostOfDistance);
			return GoalEvaluator.CalculateResult.Done;
		}
		}
	}

	public static GoalEvaluator.CalculateResult ScoreToolLocation(Entity entity, IKnownEntityData tool, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, out float distanceToTool, out double score, float toolRange = 0f)
	{
		score = 0.0;
		distanceToTool = 0f;
		if (ScoreIsCarriedToolLocation(entity, tool, out score))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		MapManager.WorldPosToSubtile(fromLocation);
		switch (regionMap.GetDistanceToEntity(entity, entity, tool, ref distanceToTool, null, null, sendMessageToEntity: true, null, giveClientFeedback: true, entity.Intelligence.Allegiance, allowTransactingWithAgentsInAllegiance: true))
		{
		case RegionMap.Result.NoAccess:
			return GoalEvaluator.CalculateResult.Done;
		case RegionMap.Result.Wait:
			return GoalEvaluator.CalculateResult.Processing;
		default:
		{
			Point value = MapManager.WorldPosToSubtile(jobLocation);
			float distance = -1f;
			switch (regionMap.GetDistanceToEntity(entity, tool, null, ref distance, null, value, sendMessageToEntity: true, null, giveClientFeedback: true, entity.Intelligence.Allegiance, allowTransactingWithAgentsInAllegiance: true))
			{
			case RegionMap.Result.NoAccess:
				return GoalEvaluator.CalculateResult.Done;
			case RegionMap.Result.Wait:
				return GoalEvaluator.CalculateResult.Processing;
			default:
			{
				distance = Common.Max(0f, distance - toolRange);
				double evaluatorTimeCostOfDistance = GoalEvaluator.GetEvaluatorTimeCostOfDistance(entity, distance + distanceToTool);
				score = GoalEvaluator.ScoreTravelTime(evaluatorTimeCostOfDistance);
				score = Common.ClampBottom(score - 0.05, 0.0);
				return GoalEvaluator.CalculateResult.Done;
			}
			}
		}
		}
	}

	private double ScoreToolCondition(IKnownEntityData tool)
	{
		return tool.Condition.Value;
	}

	private double ScoreToolProductivity(float productivity)
	{
		return Common.Clamp(productivity, 0.0, 1.0);
	}

	public bool GetKnownProgress(out float progress)
	{
		IKnownProcess processData;
		return GetKnownProgress(out progress, out processData);
	}

	public bool GetKnownProgress(out float progress, out IKnownProcess processData)
	{
		progress = 0f;
		ResolveProcess(out processData, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			return processData.GetKnownProgress(sharedKnowledge, out progress);
		}
		return false;
	}

	public bool GetContainerToPlaceOutputsIn(out EntityID? container)
	{
		container = null;
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed)
		{
			container = process.ContainerToPlaceOutputsIn;
			return true;
		}
		return false;
	}

	public bool IsCompleted(out bool isCompleted)
	{
		isCompleted = false;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed && process.IsCompleted(out isCompleted, sharedKnowledge))
		{
			return true;
		}
		return false;
	}

	public bool OutputExists(out bool outputExists)
	{
		outputExists = false;
		ResolveProcess(out var process, out var sharedKnowledge, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed && process.OutputExists(out outputExists, sharedKnowledge))
		{
			return true;
		}
		return false;
	}

	private void ProcessProducing(IKnownProcess process)
	{
		if (!EstimatedCompletion.HasValue)
		{
			ComputeEstimatedCompletionTime(process);
		}
	}

	private void ProcessStarted(IKnownProcess process)
	{
		UpdateProductionTargets();
	}

	private void UpdateProductionTargets()
	{
		if (ProcessType.Outputs == null || !ResolveOwner(out var owner))
		{
			return;
		}
		Output[] outputs = ProcessType.Outputs;
		foreach (Output output in outputs)
		{
			if (owner.ProductionOrders.Orders.TryGetValue(output.FinalEntityTypeToCreate, out var value) && value.ProductionJobsToComplete > 0)
			{
				value.ProductionJobsToComplete--;
			}
		}
	}

	private void ProcessDestroyed(IKnownProcess process)
	{
		CleanupDestroyedProcess(process);
		Destroy(removeTakers: true);
	}

	private void ProcessCompleted(IKnownProcess process)
	{
		EntityGroup owner = null;
		if (process.ActingOnEntity.HasValue && ResolveOwner(out owner))
		{
			SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
			if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(process.ActingOnEntity.Value.Entity, out var data)))
			{
				if (ProcessType.DisablesSpecialActionLockProcessTypes != null)
				{
					foreach (ProcessType disablesSpecialActionLockProcessType in ProcessType.DisablesSpecialActionLockProcessTypes)
					{
						sharedKnowledge.DisableSpecialActionLock(data, disablesSpecialActionLockProcessType.OriginalProcess);
					}
				}
				if (ProcessType.EnablesSpecialActionLockProcessTypes != null)
				{
					foreach (ProcessType enablesSpecialActionLockProcessType in ProcessType.EnablesSpecialActionLockProcessTypes)
					{
						sharedKnowledge.EnableSpecialActionLock(data, enablesSpecialActionLockProcessType.OriginalProcess);
					}
				}
			}
		}
		if (process.OutputEntities != null && (owner != null || ResolveOwner(out owner)))
		{
			SharedKnowledge sharedKnowledge2 = owner.GetAllegiance().SharedKnowledge;
			foreach (EntityID outputEntity in process.OutputEntities)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge2.GetKnownData(outputEntity, out var data2)))
				{
					if (process.ProcessType.IsGathering)
					{
						HaulingJobManager.CreateHaulingJobsForItemOutOfBand(data2, owner);
					}
					sharedKnowledge2.Allegiance.LogProductionStatistics(data2, process);
				}
			}
		}
		AddLog("Completed");
		LogProductionFinished(process);
		Destroy(removeTakers: true);
	}

	private static void RepairTools(EntityGroup owner, List<EntityID> itemsToCheckForRepairs)
	{
		if (itemsToCheckForRepairs == null || owner == null)
		{
			return;
		}
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		for (int num = itemsToCheckForRepairs.Count - 1; num >= 0; num--)
		{
			EntityID entityID = itemsToCheckForRepairs[num];
			if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out var data)))
			{
				OtherJobManager.CreateRepairJobIfNeeded(owner, data);
			}
		}
	}

	public void ResolveProcess(out IKnownProcess process, out SharedKnowledge sharedKnowledge, out bool ownerIsDestroyed, out bool processIsDestroyed)
	{
		process = null;
		processIsDestroyed = false;
		ownerIsDestroyed = false;
		if (ResolveOwner(out var owner))
		{
			sharedKnowledge = owner.Parent.Allegiance.SharedKnowledge;
			if (sharedKnowledge.PlaySiteKnowledge.GetKnownProcessData(ProductionProcess.Value, out process) == ProcessResult.Destroyed)
			{
				processIsDestroyed = true;
			}
		}
		else
		{
			sharedKnowledge = null;
			ownerIsDestroyed = true;
		}
	}

	private void LogProductionFinished(IKnownProcess process)
	{
		if (!(process is SimProcess simProcess) || process.OutputEntities == null || ProcessType.IsConsumeProcess || ProcessType.IsInnateExtractionProcess || ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded || simProcess.Workers == null || simProcess.Workers.Count <= 0)
		{
			return;
		}
		Entity entity = Entity.FindByID(simProcess.Workers[0].Item1);
		if (entity == null)
		{
			return;
		}
		foreach (EntityID outputEntity in process.OutputEntities)
		{
			Entity entity2 = Entity.FindByID(outputEntity);
			if (entity2 == null)
			{
				continue;
			}
			string arg = ".";
			string text = null;
			if (ProcessType.InputsByType != null)
			{
				string text2 = "";
				foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
				{
					text += text2;
					text += item.Key.Name.ToLower(Config.Culture);
					text2 = ", ";
				}
				arg = " from " + text + ".";
			}
			The.Client.AddLogEvent(entity.Intelligence.Allegiance, The.Client.Log.EconomicEvent, entity, $"finished producing {entity2.EntityType.Name.ToLower(Config.Culture)}{arg}");
		}
	}

	public void CreateHaulingJobsForProcessInputs(EntityGroup entityGroup, Vector3 jobLocation)
	{
		if (ProcessType.InputsByType == null)
		{
			return;
		}
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (ownerIsDestroyed || processIsDestroyed)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
		{
			Input value = item.Value;
			int num = ProcessType.InputsByType[value.EntityType].Amount.NoOfItems.Value - process.AssignedInputs[value.EntityType].Count;
			if (!value.InputIsImmovable())
			{
				for (int i = 0; i < num; i++)
				{
					new HaulingJobAnyItemOfType(jobLocation, entityGroup, value.EntityType, entityGroup.ID, null)
					{
						Priority = Priority,
						RequiredByProcessJob = this
					};
				}
			}
			else if (num > 0 && process.HasFixedLocation())
			{
				Destroy(removeTakers: true);
				break;
			}
		}
	}

	public void RepairAssignedInputs(SharedKnowledge sharedKnowledge)
	{
		if (!IsStarted(out var isStarted, out var processData) || isStarted || processData.ProcessType.Inputs == null)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> assignedInput in processData.AssignedInputs)
		{
			for (int num = assignedInput.Value.Count - 1; num >= 0; num--)
			{
				Tuple<EntityID, WorldLocation> tuple = assignedInput.Value[num];
				if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(tuple.Item1, out var data)))
				{
					assignedInput.Value.RemoveAt(num);
					flag = true;
				}
				else if (!SimProcess.IsMaterialOnSite(null, data, tuple.Item2))
				{
					if (data.AssignedToJob == base.ID)
					{
						data.AssignedToJob = null;
					}
					assignedInput.Value.RemoveAt(num);
					flag = true;
				}
			}
		}
		if (flag && ResolveOwner(out var owner) && GetCurrentJobLocation(out var location) && location.HasValue)
		{
			CreateHaulingJobsForProcessInputs(owner, location.Value);
		}
	}

	private double EstimatePowerAndMaterialsReady(Entity worker, SharedKnowledge sharedKnowledge, IKnownEntityData estimateUsingThisInput, IKnownProcess processData, float currentProgress)
	{
		if (base.ID == JobID.Invalid)
		{
			return 0.0;
		}
		if (ProcessType.WorkOrTimeNeeded.DaysNeeded.HasValue)
		{
			CalculateProgressDelta(0.0167, worker, ProcessType.GetTimeNeeded(), ProcessType.RequiredSkillType, processData, 1f);
		}
		else
		{
			CalculateProgressDelta(0.0167, ProcessType.GetTimeNeeded(), 1f);
		}
		double num = 0.0;
		float num2 = 0f;
		bool flag = true;
		if (!processData.IsStarted && ProcessType.InputsByType != null)
		{
			RepairAssignedInputs(sharedKnowledge);
			foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
			{
				Input value = item.Value;
				int num3 = processData.AssignedInputs[value.EntityType].Count;
				if (estimateUsingThisInput != null && estimateUsingThisInput.EntityType == value.EntityType)
				{
					num3++;
				}
				num2 = ((num3 < (value.Amount.NoOfItems ?? 0)) ? 0f : 1f);
				num += (double)num2;
				if (num2 == 0f)
				{
					flag = false;
				}
			}
			num /= (double)ProcessType.InputsByType.Count;
		}
		else
		{
			num = 1.0;
		}
		if (!flag)
		{
			return 0.0;
		}
		return num;
	}

	public override void Abandon(Entity entity, bool isDestroyingJob = false)
	{
		if (BuildingJob != null)
		{
			BuildingJob.Abandon(entity);
		}
		if (HarvestJob != null)
		{
			HarvestJob.Abandon(entity);
		}
		base.Abandon(entity, isDestroyingJob);
	}

	public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
	{
		if (base.ID != JobID.Invalid)
		{
			oldID = base.ID;
			base.Destroy(removeTakers, entityToExcludeFromCancel);
			CancelHaulingJobs();
			if (BuildingJob != null)
			{
				BuildingJob.DestroyUnstartedStructure();
			}
			if (HarvestJob != null)
			{
				HarvestJob.Destroy();
			}
			if (ProductionProcess.HasValue)
			{
				SimProcess.FindById(ProductionProcess)?.Destroy();
			}
		}
	}

	private void CleanupDestroyedProcess(IKnownProcess process)
	{
		JobID jobID = ((base.ID != JobID.Invalid) ? base.ID : oldID.Value);
		if (!ResolveOwner(out var owner))
		{
			return;
		}
		SharedKnowledge sharedKnowledge = owner.Parent.Allegiance.SharedKnowledge;
		IKnownEntityData data;
		if (process.AssignedInputs != null)
		{
			foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> assignedInput in process.AssignedInputs)
			{
				foreach (Tuple<EntityID, WorldLocation> item in assignedInput.Value)
				{
					if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item.Item1, out data)) && data.AssignedToJob == jobID)
					{
						data.AssignedToJob = null;
						AddLog("Removed lock on input: " + data.EntityType.KeyName + ", " + data.EntityID);
					}
				}
			}
		}
		if (process.ImmovableTool.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(process.ImmovableTool.Value, out data)) && data.AssignedToJob == jobID)
		{
			data.AssignedToJob = null;
			AddLog("Removed lock on tool: " + data.EntityType.KeyName + ", " + data.EntityID);
		}
		if (process.StationaryTools == null)
		{
			return;
		}
		foreach (EntityID stationaryTool in process.StationaryTools)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(stationaryTool, out data)) && data.AssignedToJob == jobID)
			{
				data.AssignedToJob = null;
				AddLog("Removed lock on tool: " + data.EntityType.KeyName + ", " + data.EntityID);
			}
		}
	}

	private void CancelHaulingJobs()
	{
		IterateHaulingJobs(delegate(Job j)
		{
			j.Destroy(cancelTakers: true);
		});
	}

	public void IterateHaulingJobs(Action<Job> iterateFunction)
	{
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
		if (entityGroup == null)
		{
			return;
		}
		List<Job> haulingJobs = entityGroup.HaulingJobs;
		for (int num = haulingJobs.Count - 1; num >= 0; num--)
		{
			Job job = haulingJobs[num];
			if ((job as HaulingJob).RequiredByProcessJob == this)
			{
				iterateFunction(job);
			}
		}
	}

	public bool GetIsUpgradeJob()
	{
		ResolveProcess(out var process, out var _, out var ownerIsDestroyed, out var processIsDestroyed);
		if (!ownerIsDestroyed && !processIsDestroyed && process.UpgradeCategory != null)
		{
			return true;
		}
		return false;
	}

	public override string GetName()
	{
		return ProcessType.Name;
	}

	public override bool UserCanCancel(out string reason)
	{
		reason = "";
		if (ProcessType.UserCanCancel.HasValue)
		{
			reason = ProcessType.UserCannotCancelReason ?? "";
			return ProcessType.UserCanCancel.Value;
		}
		if (HarvestJob != null)
		{
			reason = "'Gather' tasks can only be cancelled using the 'gather' window at the zone location";
			return false;
		}
		if (ProcessType.IsUpgrade)
		{
			reason = "Upgrade tasks can only be cancelled in the upgrade window (accessed from the structure's 'UPGRADE' button)";
			return false;
		}
		if (RepairJob != null)
		{
			reason = "Maintenance tasks cannot be cancelled. Use Abandon on items or structures that are not needed.";
			return false;
		}
		if (ReplenishJob != null)
		{
			reason = "Replenish tasks cannot be cancelled. Use Abandon on items or structures that are not needed.";
			return false;
		}
		if (ProcessType.GetProductionUI() == ProcessType.ProductionUI.Slider)
		{
			reason = "Production tasks can only be cancelled in the production manager panel";
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		if (HarvestJob != null)
		{
			return HarvestJob.ToString();
		}
		GetCurrentJobLocation(out var location);
		if (location.HasValue)
		{
			return ProcessType.Name + ": " + location.ToString();
		}
		return ProcessType.Name + ": no location!";
	}

	public static int CompareByEstimatedCompletion(Job job1, Job job2)
	{
		double? estimatedCompletion = ((ProcessJob)job1).EstimatedCompletion;
		double? estimatedCompletion2 = ((ProcessJob)job2).EstimatedCompletion;
		if (!estimatedCompletion.HasValue)
		{
			if (!estimatedCompletion2.HasValue)
			{
				return 0;
			}
			return 1;
		}
		if (!estimatedCompletion2.HasValue)
		{
			return -1;
		}
		return estimatedCompletion.Value.CompareTo(estimatedCompletion2.Value);
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
		BuildingJob = (BuildingJob)sn.DoISnapshot(BuildingJob);
		HarvestJob = (HarvestJob)sn.DoISnapshot(HarvestJob);
		SalvageJob = (SalvageJob)sn.DoISnapshot(SalvageJob);
		ReplenishJob = (ReplenishJob)sn.DoISnapshot(ReplenishJob);
		RepairJob = (RepairJob)sn.DoISnapshot(RepairJob);
		AssignedTools = sn.DoList(AssignedTools);
		CumulativelyEstimatedProgress = sn.DoFloat(CumulativelyEstimatedProgress);
		ProcessType = sn.DoGameData(ProcessType);
		AllToolsInUse = sn.DoBool(AllToolsInUse);
		AllToolsAreBroken = sn.DoBool(AllToolsAreBroken);
		InputsBeingHauled = sn.DoMultiMap(InputsBeingHauled);
		OutputEntityType = sn.DoGameData(OutputEntityType);
		processCompleteMethodID = sn.DoEnum(processCompleteMethodID);
		processDestroyedMethodID = sn.DoEnum(processDestroyedMethodID);
		processStartedMethodID = sn.DoEnum(processStartedMethodID);
		processProducingMethodID = sn.DoEnumNullable(processProducingMethodID);
		EstimatedCompletion = sn.DoDoubleNullable(EstimatedCompletion);
		CheckingJobID = sn.DoEnumNullable(CheckingJobID);
		ProductionProcess = sn.DoEnumNullable(ProductionProcess);
		sn.Ignore(oldID);
		sn.Ignore(areaIsCleared);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (HarvestJob != null)
		{
			HarvestJob.LoadPostProcess(sn);
		}
		if (BuildingJob != null)
		{
			BuildingJob.LoadPostProcess(sn);
		}
		if (SalvageJob != null)
		{
			SalvageJob.LoadPostProcess(sn);
		}
		if (ReplenishJob != null)
		{
			ReplenishJob.LoadPostProcess(sn);
		}
		LoadPostProcessRegisterMethodIDs();
		ComputeJobType();
		CreateRegulators();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup<IKnownProcess>.Add(processCompleteMethodID, ProcessCompleted);
		ActionLookup<IKnownProcess>.Add(processDestroyedMethodID, ProcessDestroyed);
		ActionLookup<IKnownProcess>.Add(processStartedMethodID, ProcessStarted);
		if (processProducingMethodID.HasValue)
		{
			ActionLookup<IKnownProcess>.Add(processProducingMethodID.Value, ProcessProducing);
		}
	}

	public bool AssertInputIsAssigned(EntityID? Item)
	{
		if (Item.HasValue)
		{
			SimProcess simProcess = SimProcess.FindById(ProductionProcess);
			if (simProcess != null)
			{
				if (simProcess.IsStarted)
				{
					return true;
				}
				if (simProcess.AssignedInputs.Any((KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> k) => k.Value.Any((Tuple<EntityID, WorldLocation> e) => e.Item1 == Item.Value)))
				{
					return true;
				}
			}
		}
		return false;
	}
}
