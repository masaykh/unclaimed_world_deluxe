using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Processes;

[DebuggerDisplay("{ProcessType}")]
public class SimProcess : ILookUp<SimProcess, SimProcessID>, ISnapshot, ISleepingUpdatable, IKnownProcess
{
	public enum StatusOfProcess
	{
		Complete,
		Failed,
		Active
	}

	private enum Phase
	{
		RequestSubstances,
		Produce
	}

	public ResourceItemID? ResourceItem;

	public StorageCompartment? PlaceProductsInCompartment;

	private float Progress;

	public float? MaxBulkToExtract;

	public bool LimitBulkExtractionByNutrients;

	private List<Tuple<EntityID, ToolTypeCombinationID?>> workers;

	public IDActionEvent<SimProcess> ProcessStartedEvent = new IDActionEvent<SimProcess>();

	public IDActionEvent<SimProcess> ProcessCompletedEvent = new IDActionEvent<SimProcess>();

	public IDActionEvent<SimProcess> ProcessDestroyedEvent = new IDActionEvent<SimProcess>();

	public IDActionEvent<SimProcess> ProcessProducingEvent = new IDActionEvent<SimProcess>();

	private EntityID? immovableInput;

	private Vector3? groundLocation;

	private EntityID? immovableTool;

	public ToolTypeCombination StationaryToolsTypeCombination;

	private ToolTypeCombinationID? snapshotProcessToolCombo;

	private Productivity productivity;

	private bool ignoreProgressCap;

	private bool suppressSpawningEvents;

	private OwnerID? ownerOfOutput;

	private StatusOfProcess status;

	private double? timePointInSeconds;

	private double? updateInterval;

	private Phase phase;

	private Dictionary<SubstanceType, Tuple<SubstancePoolID, float>> requestedSubstances;

	private Dictionary<SubstanceType, Tuple<SubstancePoolID, float>> assignedSubstances;

	private SimProcessID id = SimProcessID.Invalid;

	private static SimProcessID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ProcessType ProcessType { get; private set; }

	public Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

	public UpgradeCategory UpgradeCategory { get; set; }

	public List<EntityID> OutputEntities { get; private set; }

	public Point? MapPosition { get; set; }

	public List<Tuple<EntityID, ToolTypeCombinationID?>> Workers => workers;

	public SimProcessID ProcessID => ID;

	public EntityID? ContainerToPlaceOutputsIn { get; set; }

	public EntityID? ImmovableInput
	{
		get
		{
			return immovableInput;
		}
		set
		{
			immovableInput = value;
			if (value.HasValue)
			{
				groundLocation = null;
			}
			RecomputeMapPosition();
		}
	}

	public Vector3? GroundLocation
	{
		get
		{
			return groundLocation;
		}
		set
		{
			groundLocation = value;
			RecomputeMapPosition();
		}
	}

	public EntityID? ImmovableTool
	{
		get
		{
			return immovableTool;
		}
		set
		{
			immovableTool = value;
			if (value.HasValue)
			{
				groundLocation = null;
			}
			RecomputeMapPosition();
		}
	}

	public List<EntityID> StationaryTools { get; set; }

	public Productivity Productivity => productivity;

	public float ProgressSpeed { get; private set; }

	public EntityAndRoot? ActingOnEntity { get; set; }

	private StatusOfProcess Status
	{
		get
		{
			return status;
		}
		set
		{
			status = value;
			_ = 1;
		}
	}

	public bool IsStarted { get; private set; }

	public double? TimePointInSeconds => timePointInSeconds;

	public double? UpdateInterval
	{
		get
		{
			return updateInterval;
		}
		private set
		{
			if (!Common.IsEqual(updateInterval, value))
			{
				updateInterval = value;
				LookUpSleepyUpdater<SimProcess>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	public SleepyUpdaterID SleepyUpdater { get; set; }

	private Site Site => The.Sim.PlaySite;

	public SimProcessID ID
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

	public SimProcess()
	{
	}

	public SimProcess(ProcessType processType, EntityAndRoot? actingOnEntity, bool ignoreProgressCap = false, bool isSpawning = false)
	{
		ProcessType = processType;
		ActingOnEntity = actingOnEntity;
		this.ignoreProgressCap = ignoreProgressCap;
		suppressSpawningEvents = isSpawning;
		if (ProcessType.InputsByType != null)
		{
			AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
			foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
			{
				Input value = item.Value;
				AssignedInputs.Add(value.EntityType, new List<Tuple<EntityID, WorldLocation>>());
			}
		}
		if (ProcessType.ProcessToolSet != null || (ProcessType.MaxWorkers > 0 && ProcessType.WorkNeeded != WorkerNeededOptions.StartRemotely))
		{
			productivity = new Productivity();
		}
		AddToLookup();
		CreateRegulators();
		bool intervalWasChanged = false;
		RecomputeUpdateInterval(out intervalWasChanged);
	}

	private void RecomputeMapPosition()
	{
		GetCurrentLocation(out var location, null, ignoreKnowledge: true);
		if (location.HasValue)
		{
			MapPosition = MapManager.WorldPosToTile(location.Value);
		}
		else
		{
			MapPosition = null;
		}
	}

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		wasDestroyed = false;
		float? progressDelta = null;
		float toolProductivity = 0f;
		float skillProductivity = 0f;
		float energyProductivity = 0f;
		if (RequiresContinuousSubstances() && phase == Phase.RequestSubstances)
		{
			if (requestedSubstances == null || requestedSubstances.Count <= 0)
			{
				if (!EstimateProgressDelta(out progressDelta, out toolProductivity, out skillProductivity, out energyProductivity))
				{
					HandleDestroyedOutput();
					return;
				}
				RequestSubstances(progressDelta.Value);
				phase = Phase.Produce;
			}
		}
		else if (!progressDelta.HasValue && !EstimateProgressDelta(out progressDelta, out toolProductivity, out skillProductivity, out energyProductivity))
		{
			HandleDestroyedOutput();
		}
		else
		{
			Produce(progressDelta.Value, out wasDestroyed, out var clampedProgressDelta);
			if (productivity != null)
			{
				productivity.SaveProductivityStats(clampedProgressDelta, toolProductivity, skillProductivity, energyProductivity, GetUpdateInterval());
			}
		}
	}

	public void ProduceTillCompletion()
	{
		Produce(1f, out var _, out var _);
	}

	private void Produce(float progressDelta, out bool wasDestroyed, out float clampedProgressDelta)
	{
		wasDestroyed = false;
		Produce(progressDelta, out clampedProgressDelta);
		FireProducingEvent();
		if (IsCompleted())
		{
			ProductionFinished();
			wasDestroyed = true;
		}
		else if (RequiresContinuousSubstances())
		{
			assignedSubstances.Clear();
			phase = Phase.RequestSubstances;
		}
		else
		{
			phase = Phase.Produce;
		}
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<SimProcess>.Create();
	}

	public void RecomputeUpdateInterval(out bool intervalWasChanged)
	{
		intervalWasChanged = false;
		double? currentInterval = null;
		UpdateTimePoints.GetSoonestInterval(ProcessType.FinalUpdateInterval, ref currentInterval);
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalWasChanged = true;
		}
	}

	private bool RequiresContinuousSubstances()
	{
		if (ProcessType.TotalContinuousSubstanceInputTypes != null)
		{
			return ProcessType.TotalContinuousSubstanceInputTypes.Count > 0;
		}
		return false;
	}

	private void RequestSubstances(float estimatedProgressDelta)
	{
	}

	public static SimProcess FindById(SimProcessID? id)
	{
		return LookUp<SimProcess, SimProcessID>.FindByID(id);
	}

	private bool EstimateProgressDelta(out float? progressDelta, out float toolProductivity, out float skillProductivity, out float energyProductivity)
	{
		progressDelta = null;
		if (!GetRealProgress(out var progress))
		{
			toolProductivity = (skillProductivity = (energyProductivity = 0f));
			return false;
		}
		float cumulativelyEstimatedProgress = progress;
		progressDelta = 0f;
		if (ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded)
		{
			EstimateProgressFromWorkers(ref progressDelta, ref cumulativelyEstimatedProgress, out toolProductivity, out skillProductivity, out energyProductivity);
		}
		else
		{
			EstimateProgress(null, StationaryToolsTypeCombination, progress, 1f, out var progressDelta2, out toolProductivity, out skillProductivity, out energyProductivity);
			progressDelta = progressDelta2;
		}
		EstimateProgressFromProperties(ref progressDelta);
		if (!ignoreProgressCap)
		{
			CapProgress(ref progressDelta);
		}
		return true;
	}

	private void CapProgress(ref float? totalProgressDelta)
	{
		if (ProcessType.ProgressPerSecondCap.HasValue)
		{
			float val = ProcessType.ProgressPerSecondCap.Value * GetUpdateInterval();
			totalProgressDelta = Math.Min(val, totalProgressDelta.Value);
		}
	}

	private float GetUpdateInterval()
	{
		return (float)(updateInterval ?? The.Sim.GameTime.ElapsedGameTime.TotalSeconds);
	}

	private void EstimateProgressFromProperties(ref float? totalProgressDelta)
	{
		if (ProcessType.ProgressFactorProperties == null)
		{
			return;
		}
		foreach (ProgressFactorProperty progressFactorProperty in ProcessType.ProgressFactorProperties)
		{
			PropertyResult? propertyResult = null;
			float? num = null;
			Entity entity = null;
			if (progressFactorProperty.UseActingOnEntity && ActingOnEntity.HasValue)
			{
				entity = Entity.FindByID(ActingOnEntity.Value.Entity);
			}
			else if (progressFactorProperty.InputType != null)
			{
				if (AssignedInputs.TryGetValue(GameData.Instance.AllEntityTypes[progressFactorProperty.InputType], out var value))
				{
					entity = Entity.FindByID(value[0].Item1);
				}
			}
			else if (progressFactorProperty.Factor != null)
			{
				propertyResult = progressFactorProperty.Factor.Evaluate(null, EntityAndRoot.GetEntity(ActingOnEntity), null, null);
				if (propertyResult.HasValue && propertyResult.Value.NumberResult.HasValue)
				{
					num = propertyResult.Value.NumberResult.Value;
				}
			}
			if (entity != null)
			{
				SubstanceAmount value2;
				if (progressFactorProperty.PropertyKey != null)
				{
					propertyResult = entity.GetPropertyValue(progressFactorProperty.PropertyKey, null);
					if (propertyResult.HasValue && propertyResult.Value.NumberResult.HasValue)
					{
						num = propertyResult.Value.NumberResult.Value;
					}
				}
				else if (progressFactorProperty.SubstanceKey != null && entity.SubstanceBulkAmounts.TryGetValue(GameData.Instance.AllSubstanceTypes[progressFactorProperty.SubstanceKey], out value2))
				{
					num = value2.Amount;
				}
			}
			if (num.HasValue)
			{
				if (progressFactorProperty.ShiftByAmount.HasValue)
				{
					num += progressFactorProperty.ShiftByAmount.Value;
				}
				if (progressFactorProperty.ScaleByAmount.HasValue)
				{
					num *= progressFactorProperty.ScaleByAmount.Value;
				}
				totalProgressDelta *= num;
			}
		}
	}

	private void EstimateProgressFromWorkers(ref float? totalProgressDelta, ref float cumulativelyEstimatedProgress, out float toolProductivity, out float skillProductivity, out float energyProductivity)
	{
		toolProductivity = (skillProductivity = (energyProductivity = 0f));
		if (workers != null && workers.Count > 0)
		{
			float averageLaborEfficiency = (float)ProcessJob.GetAverageLaborReturn(workers.Count, ProcessType.MaxWorkers);
			for (int num = workers.Count - 1; num >= 0; num--)
			{
				Tuple<EntityID, ToolTypeCombinationID?> tuple = workers[num];
				Entity entity = Entity.FindByID(tuple.Item1);
				if (entity != null)
				{
					EstimateProgress(entity, LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(tuple.Item2), cumulativelyEstimatedProgress, averageLaborEfficiency, out var progressDelta, out var toolProductivity2, out var skillProductivity2, out var energyProductivity2);
					toolProductivity += toolProductivity2 / (float)workers.Count;
					skillProductivity += skillProductivity2 / (float)workers.Count;
					energyProductivity += energyProductivity2 / (float)workers.Count;
					totalProgressDelta += progressDelta;
					cumulativelyEstimatedProgress += progressDelta;
					if (NonLivingEntity.IsCompleted(cumulativelyEstimatedProgress))
					{
						break;
					}
				}
				else
				{
					workers.Remove(tuple);
					if (workers.Count == 0)
					{
						totalProgressDelta = 0f;
						HandleNoWorkers();
					}
				}
			}
		}
		else
		{
			totalProgressDelta = 0f;
			HandleNoWorkers();
		}
	}

	public bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
	{
		location = null;
		if (!GetFirstOutputEntityData(out var outputData, sharedKnowledge, ignoreKnowledge))
		{
			return false;
		}
		if (outputData != null)
		{
			location = outputData.AccessPoint;
			return true;
		}
		return GetProductionSiteLocation(out location, sharedKnowledge, ignoreKnowledge);
	}

	public bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
	{
		location = null;
		if (ContainerToPlaceOutputsIn.HasValue)
		{
			IKnownEntityData data;
			EntityResult result;
			if (ignoreKnowledge)
			{
				data = Entity.FindByID(ContainerToPlaceOutputsIn.Value);
				result = ((data != null) ? EntityResult.SeenDirectly : EntityResult.Destroyed);
			}
			else
			{
				result = sharedKnowledge.GetKnownData(ContainerToPlaceOutputsIn.Value, out data);
			}
			if (GoalEvaluator.EntityDataResultCausesSkip(result))
			{
				HandleDestroyedOutputContainer();
				return false;
			}
			location = data.AccessPoint;
			return true;
		}
		if (GroundLocation.HasValue)
		{
			location = GroundLocation.Value;
			return true;
		}
		return true;
	}

	public bool GetImmovableEntityLocation(EntityID item, out Vector3? location, ref IKnownEntityData siteData, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
	{
		location = null;
		EntityResult result;
		if (ignoreKnowledge)
		{
			siteData = Entity.FindByID(item);
			result = ((siteData != null) ? EntityResult.SeenDirectly : EntityResult.Destroyed);
		}
		else
		{
			result = sharedKnowledge.GetKnownData(item, out siteData);
		}
		if (GoalEvaluator.EntityDataResultCausesSkip(result))
		{
			return false;
		}
		location = siteData.AccessPoint;
		return true;
	}

	private bool GetFirstOutputEntityData(out IKnownEntityData outputData, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
	{
		EntityID? firstOutputEntity = GetFirstOutputEntity();
		if (firstOutputEntity.HasValue)
		{
			EntityResult result;
			if (ignoreKnowledge)
			{
				outputData = Entity.FindByID(firstOutputEntity.Value);
				result = ((outputData != null) ? EntityResult.SeenDirectly : EntityResult.Destroyed);
			}
			else
			{
				result = sharedKnowledge.GetKnownData(firstOutputEntity.Value, out outputData);
			}
			if (GoalEvaluator.EntityDataResultCausesSkip(result))
			{
				_ = 4;
				HandleDestroyedOutput();
				return false;
			}
			return true;
		}
		outputData = null;
		return true;
	}

	public bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
	{
		IKnownEntityData siteData = null;
		IKnownEntityData siteData2 = null;
		if (!GetFixedJobLocation(out var location2, sharedKnowledge, ignoreKnowledge))
		{
			location = null;
			return false;
		}
		if (!location2.HasValue)
		{
			if (ImmovableInput.HasValue)
			{
				if (GetImmovableEntityLocation(ImmovableInput.Value, out location, ref siteData, sharedKnowledge, ignoreKnowledge))
				{
					return true;
				}
				HandleDestroyedInput();
				ImmovableInput = null;
				return false;
			}
			if (ImmovableTool.HasValue)
			{
				if (GetImmovableEntityLocation(ImmovableTool.Value, out location, ref siteData2, sharedKnowledge, ignoreKnowledge))
				{
					return true;
				}
				HandleDestroyedImmovableTool();
				return false;
			}
			location = null;
			return true;
		}
		location = location2;
		return true;
	}

	public bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge)
	{
		outputExists = false;
		if (GetFirstOutputEntityData(out var outputData, sharedKnowledge))
		{
			if (outputData != null)
			{
				outputExists = true;
			}
			else
			{
				outputExists = false;
			}
			return true;
		}
		return false;
	}

	public bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge)
	{
		isCompleted = false;
		if (GetKnownProgress(sharedKnowledge, out var progress))
		{
			isCompleted = NonLivingEntity.IsCompleted(progress);
			return true;
		}
		return false;
	}

	private bool IsCompleted()
	{
		if (ProcessType.HasNoEndpoint)
		{
			return false;
		}
		if (GetRealProgress(out var progress) && NonLivingEntity.IsCompleted(progress))
		{
			return true;
		}
		return false;
	}

	private void CreateRegulators()
	{
	}

	public void AssignSubstance(SubstancePool pool, float amount)
	{
		requestedSubstances.Remove(pool.SubstanceType);
		Common.AddToDictionary(ref assignedSubstances, pool.SubstanceType, new Tuple<SubstancePoolID, float>(pool.ID, amount));
	}

	private void RequestSubstance()
	{
	}

	private void HandleDestroyedOutput()
	{
		Destroy();
	}

	private void HandleDestroyedOutputContainer()
	{
		Destroy();
	}

	private void HandleDestroyedInput()
	{
		Destroy();
	}

	private void HandleDestroyedActingOn()
	{
		Destroy();
	}

	private void HandleDestroyedImmovableTool()
	{
		Destroy();
	}

	private void HandleNoWorkers()
	{
	}

	private void HandleNoSubstances()
	{
		Destroy();
	}

	public StatusOfProcess Start(Entity startingAgent, OwnerID? ownerOfOutput, ToolTypeCombination stationaryToolsCombo, List<EntityID> stationaryTools)
	{
		_ = startingAgent.EntityID;
		_ = 4941;
		this.ownerOfOutput = ownerOfOutput;
		StationaryTools = stationaryTools;
		StationaryToolsTypeCombination = stationaryToolsCombo;
		Status = StatusOfProcess.Active;
		if (ProcessType.IsConstruction() && OutputEntities != null && !CheckStructureAreaCleared(0.01f))
		{
			Status = StatusOfProcess.Failed;
			return Status;
		}
		if (!ConsumeAndCreateOutputs(startingAgent))
		{
			Status = StatusOfProcess.Failed;
			return Status;
		}
		RecreateActingOnEntityRoot();
		AssignProcessToInvolvedEntitiesOrTile(remove: false);
		IsStarted = true;
		SeeOutputs(startingAgent);
		FireProductionStartedEvent();
		if (startingAgent != null)
		{
			Goal.FireEventActions(startingAgent, EntityAndRoot.GetEntity(ActingOnEntity), ProcessType.GetStartHook(), startingAgent.EntityType.IntelligenceType.EventActions, AgentActionHooks.StartProducing, ProcessType.EventActions, suppressSpawningEvents);
		}
		else
		{
			Goal.FireEventActions(null, EntityAndRoot.GetEntity(ActingOnEntity), ProcessType.GetStartHook(), ProcessType.EventActions, AgentActionHooks.StartProducing, null, suppressSpawningEvents);
		}
		if (Status == StatusOfProcess.Active)
		{
			if (IsInstantProcess())
			{
				ProduceTillCompletion();
			}
			else
			{
				The.Sim.PlaySite.PlaySite.AddProcess(this);
			}
		}
		return Status;
	}

	private void RecreateActingOnEntityRoot()
	{
		if (ActingOnEntity.HasValue)
		{
			Entity entity = Entity.FindByID(ActingOnEntity.Value.Entity);
			ActingOnEntity = entity.GetAsEntityAndRoot();
		}
	}

	private bool IsInstantProcess()
	{
		if (EstimateProgressDelta(out var progressDelta, out var _, out var _, out var _))
		{
			return progressDelta >= 1f;
		}
		return false;
	}

	private void SeeOutputs(Entity byEntity)
	{
		if (OutputEntities != null)
		{
			IterateOutputEntities(delegate(EntityID e)
			{
				byEntity.Intelligence.Allegiance.SharedKnowledge.SeeDetectable(Entity.FindByID(e));
			});
		}
	}

	private void DestroyUnstartedOutputs()
	{
		if (OutputEntities == null)
		{
			return;
		}
		for (int num = OutputEntities.Count - 1; num >= 0; num--)
		{
			Entity entity = Entity.FindByID(OutputEntities[num]);
			if (entity != null && !entity.IsCompleted())
			{
				bool? flag = entity.IsStarted();
				bool flag2 = true;
				if (flag == true != flag2 || !flag.HasValue || ProcessType.IsGathering)
				{
					entity.Destroy();
				}
			}
		}
	}

	private void AssignProcessToInvolvedEntitiesOrTile(bool remove)
	{
		bool flag = false;
		if (StationaryTools != null)
		{
			foreach (EntityID stationaryTool in StationaryTools)
			{
				Entity entity = Entity.FindByID(stationaryTool);
				if (entity != null)
				{
					if (remove)
					{
						entity.RemoveProcess(ID);
						continue;
					}
					entity.AddProcess(this);
					flag = true;
				}
			}
		}
		if (AssignedInputs != null)
		{
			foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> assignedInput in AssignedInputs)
			{
				foreach (Tuple<EntityID, WorldLocation> item in assignedInput.Value)
				{
					Entity entity2 = Entity.FindByID(item.Item1);
					if (entity2 != null)
					{
						if (remove)
						{
							entity2.RemoveProcess(ID);
							continue;
						}
						entity2.AddProcess(this);
						flag = true;
					}
				}
			}
		}
		if (OutputEntities != null)
		{
			foreach (EntityID outputEntity in OutputEntities)
			{
				Entity entity3 = Entity.FindByID(outputEntity);
				if (entity3 != null)
				{
					if (remove)
					{
						entity3.RemoveProcess(ID);
						continue;
					}
					entity3.AddProcess(this);
					flag = true;
				}
			}
		}
		if (ActingOnEntity.HasValue)
		{
			Entity entity4 = Entity.FindByID(ActingOnEntity.Value.Entity);
			if (entity4 != null)
			{
				if (remove)
				{
					entity4.RemoveProcess(ID);
				}
				else
				{
					entity4.AddProcess(this);
					flag = true;
				}
			}
		}
		if (remove)
		{
			if (MapPosition.HasValue)
			{
				The.Map.GetTile(MapPosition.Value).RemoveProcess(this);
			}
		}
		else if (!flag && MapPosition.HasValue)
		{
			The.Map.GetTile(MapPosition.Value).AddProcess(this);
		}
	}

	public void AssignWorker(Entity entity, ToolTypeCombination tools)
	{
		if (workers == null)
		{
			workers = new List<Tuple<EntityID, ToolTypeCombinationID?>>();
		}
		if (workers.Exists((Tuple<EntityID, ToolTypeCombinationID?> w) => w.Item1 == entity.ID))
		{
			RemoveWorker(entity);
		}
		workers.Add(new Tuple<EntityID, ToolTypeCombinationID?>(entity.ID, tools?.ID));
	}

	public void RemoveWorker(Entity entity)
	{
		if (workers != null)
		{
			workers.RemoveAll((Tuple<EntityID, ToolTypeCombinationID?> e) => e.Item1 == entity.ID);
		}
	}

	private float ConsumePooledSubstances(float progressDelta)
	{
		if (!RequiresContinuousSubstances())
		{
			return progressDelta;
		}
		float num = progressDelta;
		foreach (KeyValuePair<SubstanceType, float> totalContinuousSubstanceInputType in ProcessType.TotalContinuousSubstanceInputTypes)
		{
			if (!assignedSubstances.TryGetValue(totalContinuousSubstanceInputType.Key, out var value))
			{
				return 0f;
			}
			float item = value.Item2;
			float num2 = item / totalContinuousSubstanceInputType.Value;
			if (num2 < 1f)
			{
				float num3 = progressDelta * num2;
				if (num3 < num)
				{
					num = num3;
				}
			}
		}
		if (num > 0f)
		{
			foreach (KeyValuePair<SubstanceType, float> totalContinuousSubstanceInputType2 in ProcessType.TotalContinuousSubstanceInputTypes)
			{
				assignedSubstances.TryGetValue(totalContinuousSubstanceInputType2.Key, out var value2);
				SubstancePool substancePool = LookUp<SubstancePool, SubstancePoolID>.FindByID(value2.Item1);
				float amount = totalContinuousSubstanceInputType2.Value * num;
				if (substancePool != null)
				{
					substancePool.Consume(amount);
					continue;
				}
				return 0f;
			}
		}
		return num;
	}

	private StatusOfProcess Produce(float progressDelta, out float realizedProgressDelta)
	{
		Status = StatusOfProcess.Active;
		if (!GetRealProgress(out var progress))
		{
			Status = StatusOfProcess.Failed;
			realizedProgressDelta = 0f;
			return Status;
		}
		progressDelta = ConsumePooledSubstances(progressDelta);
		float newProgress = progress + progressDelta;
		if (newProgress > 1f)
		{
			float num = newProgress - 1f;
			realizedProgressDelta = progressDelta - num;
		}
		else
		{
			realizedProgressDelta = progressDelta;
		}
		float num2 = GetUpdateInterval();
		ProgressSpeed = progressDelta / num2;
		if (Common.IsZero(progressDelta))
		{
			Status = StatusOfProcess.Failed;
			realizedProgressDelta = 0f;
			return Status;
		}
		HandleWorkersAndTools(num2, progressDelta);
		if (ProcessType.HasOutput)
		{
			bool hasMissingOutput = false;
			IterateOutputEntities(delegate(EntityID output)
			{
				Entity entity = Entity.FindByID(output);
				if (entity != null)
				{
					entity.NonLivingEntity.Progress = newProgress;
				}
				else
				{
					hasMissingOutput = true;
				}
			});
			if (hasMissingOutput)
			{
				Status = StatusOfProcess.Failed;
				realizedProgressDelta = 0f;
				return Status;
			}
		}
		else
		{
			SetProgressForNoOutput(newProgress);
		}
		return Status;
	}

	public double? ComputeEstimatedCompletionTime()
	{
		if (IsStarted)
		{
			if (!Common.IsZero(ProgressSpeed))
			{
				if (!GetRealProgress(out var progress))
				{
					return null;
				}
				double d = (1f - progress) / ProgressSpeed;
				d = Common.ClampBottom(d, 0.0);
				return The.Sim.TotalUnPausedGameTimeInSeconds + d;
			}
			return null;
		}
		return null;
	}

	private void HandleWorkersAndTools(float elapsed, float progressDelta)
	{
		if (workers != null)
		{
			for (int num = workers.Count - 1; num >= 0; num--)
			{
				Entity.FindByID(workers[num].Item1)?.Intelligence.PerformWorkAndAffectHandTools(this, elapsed, progressDelta);
			}
		}
		if (StationaryTools != null && StationaryTools.Count > 0)
		{
			WearDownTools(null, elapsed, StationaryTools, StationaryToolsTypeCombination);
		}
	}

	public void SetProgressForNoOutput(float progress)
	{
		if (ProcessType.RepairAction == RepairAction.Integrity || ProcessType.RepairAction == RepairAction.Condition || ProcessType.RepairAction == RepairAction.PartsCondition)
		{
			if (ActingOnEntity.HasValue)
			{
				Entity.FindByID(ActingOnEntity.Value.Entity)?.NonLivingEntity.Repair(ProcessType.RepairAction.Value, progress);
			}
		}
		else if (ProcessType.RepairAction.HasValue && RepairType.IsReplaceAction(ProcessType.RepairAction.Value))
		{
			Progress = progress;
		}
		else
		{
			Progress = progress;
		}
	}

	public void IterateOutputEntities(Action<EntityID> iterateMethod)
	{
		if (OutputEntities == null)
		{
			return;
		}
		foreach (EntityID outputEntity in OutputEntities)
		{
			iterateMethod(outputEntity);
		}
	}

	private bool GetRealProgressFromOutputs(out float progress)
	{
		EntityID? firstOutputEntity = GetFirstOutputEntity(OutputEntities);
		progress = 0f;
		if (firstOutputEntity.HasValue)
		{
			Entity entity = Entity.FindByID(firstOutputEntity.Value);
			if (entity == null)
			{
				progress = 0f;
				return false;
			}
			progress = entity.Progress.Value;
		}
		return true;
	}

	private bool GetRealRepairProgress(out float progress)
	{
		progress = 0f;
		if (ActingOnEntity.HasValue)
		{
			Entity entity = Entity.FindByID(ActingOnEntity.Value.Entity);
			if (entity == null)
			{
				progress = 0f;
				return false;
			}
			progress = entity.GetRepairProgress(ProcessType.RepairAction.Value);
		}
		return true;
	}

	private bool GetKnownRepairProgress(SharedKnowledge sharedKnowledge, out float progress)
	{
		progress = 0f;
		if (ActingOnEntity.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(ActingOnEntity.Value.Entity, out var data)))
			{
				progress = 0f;
				return false;
			}
			progress = data.GetRepairProgress(ProcessType.RepairAction.Value);
		}
		return true;
	}

	private static bool GetKnownProgressFromOutputs(SharedKnowledge sharedKnowledge, List<EntityID> outputs, out float progress)
	{
		EntityID? firstOutputEntity = GetFirstOutputEntity(outputs);
		progress = 0f;
		if (firstOutputEntity.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(firstOutputEntity.Value, out var data)))
			{
				progress = 0f;
				return false;
			}
			progress = data.Progress.Value;
		}
		return true;
	}

	public static EntityID? GetFirstOutputEntity(List<EntityID> outputs)
	{
		if (outputs != null && outputs.Count > 0)
		{
			return outputs[0];
		}
		return null;
	}

	public EntityID? GetFirstOutputEntity()
	{
		return GetFirstOutputEntity(OutputEntities);
	}

	public bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress)
	{
		if (ProcessType.RepairAction.HasValue)
		{
			if (RepairType.IsReplaceAction(ProcessType.RepairAction.Value))
			{
				progress = Progress;
				return true;
			}
			return GetKnownRepairProgress(sharedKnowledge, out progress);
		}
		if (OutputEntities != null && OutputEntities.Count > 0)
		{
			return GetKnownProgressFromOutputs(sharedKnowledge, OutputEntities, out progress);
		}
		progress = Progress;
		return true;
	}

	private bool GetRealProgress(out float progress)
	{
		if (ProcessType.RepairAction.HasValue)
		{
			if (RepairType.IsReplaceAction(ProcessType.RepairAction.Value))
			{
				progress = Progress;
				return true;
			}
			return GetRealRepairProgress(out progress);
		}
		if (OutputEntities != null && OutputEntities.Count > 0)
		{
			return GetRealProgressFromOutputs(out progress);
		}
		progress = Progress;
		return true;
	}

	public void AddOutputEntity(Entity outputEntity)
	{
		if (OutputEntities == null)
		{
			OutputEntities = new List<EntityID>();
		}
		OutputEntities.Add(outputEntity.EntityID);
	}

	private void EstimateProgress(Entity worker, ToolTypeCombination tools, float currentProgress, float averageLaborEfficiency, out float progressDelta, out float toolProductivity, out float skillProductivity, out float energyProductivity)
	{
		double seconds = GetUpdateInterval();
		toolProductivity = tools?.Productivity ?? 1f;
		if (worker != null)
		{
			skillProductivity = worker.Intelligence.GetSkillProductionFactor(ProcessType.RequiredSkillType);
			energyProductivity = ProcessType.GetEnergyProductivity(worker);
		}
		else
		{
			skillProductivity = 1f;
			energyProductivity = 1f;
		}
		progressDelta = ProcessType.CalculateProgressDelta(seconds, skillProductivity, energyProductivity, toolProductivity, 1f, averageLaborEfficiency);
	}

	public void ProductionFinished()
	{
		Status = StatusOfProcess.Complete;
		Entity worker = null;
		if (workers != null && workers.Count > 0)
		{
			worker = Entity.FindByID(workers[0].Item1);
		}
		IterateOutputEntities(delegate(EntityID output)
		{
			Entity entity2 = Entity.FindByID(output);
			if (entity2 != null)
			{
				ProductionFinishedForSingleOutput(entity2, worker);
			}
		});
		if (ProcessType.IsReplenishProcess)
		{
			Entity entity = Entity.FindByID(EntityAndRoot.GetEntity(ActingOnEntity));
			if (entity != null)
			{
				IOwner owner = null;
				if (LookUpOwners.ResolveEntityOwner((IKnownEntityData)entity, out owner))
				{
					Dictionary<EntityType, List<Entity>> dictionary = new Dictionary<EntityType, List<Entity>>();
					Entity itemToConsumeEntity = null;
					if (!ValidateInputs(worker, ref itemToConsumeEntity, dictionary))
					{
						Status = StatusOfProcess.Failed;
						Destroy();
						return;
					}
					foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
					{
						Input value = item.Value;
						if (!dictionary.TryGetValue(value.EntityType, out var value2))
						{
							continue;
						}
						for (int num = 0; num < value.Amount.NoOfItems; num++)
						{
							itemToConsumeEntity = HandleSingleInputItem(value2);
							if (itemToConsumeEntity.ContainedBy.HasValue)
							{
								Entity.FindByID(itemToConsumeEntity.ContainedBy)?.Contains.Remove(itemToConsumeEntity);
							}
							GoalReplenish.Replenish(worker, ProcessType.ReplenishAction.Value, itemToConsumeEntity, entity, owner, StorageCompartment.Haul);
						}
					}
				}
			}
		}
		if (!ProcessProductionFinished(worker, ProcessType, EntityAndRoot.GetEntity(ActingOnEntity), GetFirstOutputEntity(), suppressSpawningEvents))
		{
			Status = StatusOfProcess.Failed;
			Destroy();
		}
		else
		{
			FireProductionFinishedEvent();
			Destroy();
		}
	}

	public static bool ProcessProductionFinished(Entity worker, ProcessType processType, EntityID? ActingOnEntity, EntityID? firstOutputEntity, bool isSpawning)
	{
		if (processType.SetPreparedProperty == true)
		{
			Entity entity = Entity.FindByID(ActingOnEntity);
			if (entity != null && entity.Find<UWGame.SimSide.Items.Tool>(out var c))
			{
				c.IsPrepared = true;
				if (c.IsPrepared != true)
				{
					return false;
				}
			}
		}
		Entity entity2 = Entity.FindByID(ActingOnEntity);
		if (processType.AttacksVerminValueToSet.HasValue && entity2 != null && entity2.EntityType.IntelligenceType != null)
		{
			entity2.Intelligence.AttacksVermin = processType.AttacksVerminValueToSet.Value;
		}
		AgentActionHooks? agentActionHooks = null;
		EntityID? targetEntity;
		if (processType.IsConstruction())
		{
			agentActionHooks = AgentActionHooks.CompletedConstructing;
			targetEntity = firstOutputEntity;
		}
		else if (!processType.IsGathering)
		{
			targetEntity = ((!ActingOnEntity.HasValue) ? firstOutputEntity : new EntityID?(ActingOnEntity.Value));
		}
		else
		{
			agentActionHooks = AgentActionHooks.CompletedHarvesting;
			targetEntity = firstOutputEntity;
		}
		Dictionary<AgentActionHooks, List<ActionSets>> defaultEventActions = null;
		if (worker != null)
		{
			defaultEventActions = worker.EntityType.IntelligenceType.EventActions;
		}
		if (agentActionHooks.HasValue)
		{
			Goal.FireEventActions(worker, targetEntity, agentActionHooks.Value, defaultEventActions, agentActionHooks.Value, processType.EventActions, isSpawning);
		}
		Goal.FireEventActions(worker, targetEntity, AgentActionHooks.CompletedProducing, defaultEventActions, AgentActionHooks.CompletedProducing, processType.EventActions, isSpawning);
		if (entity2 != null)
		{
			if (processType.DisablesSharedActionProcessTypes != null)
			{
				foreach (ProcessType disablesSharedActionProcessType in processType.DisablesSharedActionProcessTypes)
				{
					entity2.DisableSharedSpecialAction(disablesSharedActionProcessType.OriginalProcess);
				}
			}
			if (processType.EnablesSharedActionProcessTypes != null)
			{
				foreach (ProcessType enablesSharedActionProcessType in processType.EnablesSharedActionProcessTypes)
				{
					entity2.EnableSharedSpecialAction(enablesSharedActionProcessType.OriginalProcess);
				}
			}
			if (processType.UsesAnchor())
			{
				entity2.DisableSpecialActionsUsingAnchor();
			}
		}
		return true;
	}

	private bool PickupHarvestedItem(Entity worker, Entity harvestedItem)
	{
		IOwner owner = LookUpOwners.FindByID(ownerOfOutput);
		if (owner == null)
		{
			return false;
		}
		if (harvestedItem.Item.Pickup(worker, owner, StorageCompartment.Haul))
		{
			return true;
		}
		harvestedItem.PlaceEntityOnPlaySite(worker.PlaySiteLocation, null, null, new Entity.SetOwnerInfo(owner));
		return false;
	}

	private void FireProductionStartedEvent()
	{
		if (ProcessStartedEvent != null)
		{
			ProcessStartedEvent.Invoke(this);
		}
	}

	private void FireProductionFinishedEvent()
	{
		if (ProcessCompletedEvent != null)
		{
			ProcessCompletedEvent.Invoke(this);
		}
	}

	private void FireProducingEvent()
	{
		if (ProcessProducingEvent != null)
		{
			ProcessProducingEvent.Invoke(this);
		}
	}

	private void FireProcessDestroyedEvent()
	{
		if (ProcessDestroyedEvent != null)
		{
			ProcessDestroyedEvent.Invoke(this);
		}
	}

	private void ProductionFinishedForSingleOutput(Entity outputEntity, Entity worker)
	{
		outputEntity.ComeOnline(suppressSpawningEvents);
		if (outputEntity.Structure != null)
		{
			EntityID? anchorID = null;
			if (ProcessType.IsSpecialActionType)
			{
				anchorID = EntityAndRoot.GetEntity(ActingOnEntity);
			}
			outputEntity.Structure.ConstructionFinished(anchorID);
		}
		else if (ProcessType.MoveOutputToWorkerWhenCompleted && worker != null && worker.EntityType.ContainerType != null && worker.AgentStorage != null)
		{
			PickupHarvestedItem(worker, outputEntity);
		}
	}

	public static bool WearDownTools(Entity producingEntity, float elapsedTime, List<EntityID> tools, ToolTypeCombination toolTypeCombination)
	{
		if (tools == null)
		{
			return true;
		}
		Entity toolEntity;
		foreach (EntityID tool in tools)
		{
			toolEntity = Entity.FindByID(tool);
			if (toolEntity == null)
			{
				return false;
			}
			float item = toolTypeCombination.Tools.Find((Tuple<EntityType, float> tt) => tt.Item1 == toolEntity.EntityType).Item2;
			float num = 1f - toolEntity.EntityType.ToolType.Durability.Value;
			float num2 = elapsedTime * num * item;
			if (num2 > 0f && toolEntity.DoDamage(num2))
			{
				if (producingEntity != null)
				{
					The.Client.AddLogEvent(producingEntity.Intelligence.Allegiance, The.Client.Log.EconomicEvent, producingEntity, string.Concat("A ", toolEntity.EntityType.Name.ToLower(Config.Culture), " broke while ", producingEntity, " was working with it."));
				}
				return false;
			}
		}
		return true;
	}

	public List<Tuple<EntityID, WorldLocation>> GetAssignedInputs(EntityType entityType)
	{
		List<Tuple<EntityID, WorldLocation>> value = null;
		if (AssignedInputs != null)
		{
			AssignedInputs.TryGetValue(entityType, out value);
		}
		return value;
	}

	private bool ConsumeAndCreateOutputs(Entity startingAgent)
	{
		_ = ContainerToPlaceOutputsIn.HasValue;
		Entity entity = null;
		if (ContainerToPlaceOutputsIn.HasValue)
		{
			entity = Entity.FindByID(ContainerToPlaceOutputsIn.Value);
			if (entity == null)
			{
				Status = StatusOfProcess.Failed;
				return false;
			}
		}
		Dictionary<EntityType, List<Entity>> dictionary = new Dictionary<EntityType, List<Entity>>();
		Entity itemToConsumeEntity = null;
		Vector3? lastDestroyedInputLocation = null;
		Dictionary<EntityType, List<Entity>> parts;
		float totalBulkOfInput;
		Dictionary<string, float> extractedSubstances;
		if (!suppressSpawningEvents)
		{
			if (!ValidateInputs(startingAgent, ref itemToConsumeEntity, dictionary))
			{
				return false;
			}
			ConsumeInputsAndGatherParts(startingAgent, dictionary, out parts, out totalBulkOfInput, out extractedSubstances, out lastDestroyedInputLocation);
		}
		else
		{
			parts = new Dictionary<EntityType, List<Entity>>();
			totalBulkOfInput = 0f;
			extractedSubstances = null;
		}
		if (ProcessType.HasOutput)
		{
			Output[] outputs = ProcessType.Outputs;
			foreach (Output output in outputs)
			{
				int noOfItemsToCreate = 1;
				if (!output.GetOutputAmountsToCreate(totalBulkOfInput, extractedSubstances, out noOfItemsToCreate, out var bulkOfEachOutputItem))
				{
					Status = StatusOfProcess.Failed;
					return false;
				}
				if (!CreateOutputsFromInputs(startingAgent, ProcessType, entity, parts, output, noOfItemsToCreate, bulkOfEachOutputItem))
				{
					Status = StatusOfProcess.Failed;
					return false;
				}
			}
			if (OutputEntities == null)
			{
				Status = StatusOfProcess.Failed;
				return false;
			}
			foreach (EntityID outputEntity in OutputEntities)
			{
				Entity entity2 = Entity.FindByID(outputEntity);
				if (entity2 != null)
				{
					if (entity2.Find<Structure>(out var c) && !c.ConstructionHasStarted())
					{
						EntityID? anchorID = null;
						if (ProcessType.IsSpecialActionType)
						{
							anchorID = EntityAndRoot.GetEntity(ActingOnEntity);
						}
						c.ConstructionStarted(anchorID);
					}
					continue;
				}
				Status = StatusOfProcess.Failed;
				return false;
			}
		}
		if (ProcessType.IsSalvageProcess)
		{
			if (dictionary.Count > 1)
			{
				throw new Exception("Salvaging processes are assumed to only have one input!");
			}
			DestroyUnusedInputParts(startingAgent, parts, itemToConsumeEntity, lastDestroyedInputLocation);
		}
		return true;
	}

	private bool ValidateInputs(Entity startingAgent, ref Entity itemToConsumeEntity, Dictionary<EntityType, List<Entity>> assignedInputData)
	{
		if (ProcessType.InputsByType != null)
		{
			foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
			{
				if (item.Key.TreeType != null)
				{
					continue;
				}
				Input value = item.Value;
				List<Entity> list = new List<Entity>();
				assignedInputData.Add(item.Key, list);
				List<Tuple<EntityID, WorldLocation>> assignedInputs = GetAssignedInputs(value.EntityType);
				if (assignedInputs != null)
				{
					if (assignedInputs.Count >= item.Value.Amount.NoOfItems)
					{
						for (int i = 0; i < item.Value.Amount.NoOfItems; i++)
						{
							Tuple<EntityID, WorldLocation> tuple = assignedInputs[i];
							itemToConsumeEntity = Entity.FindByID(tuple.Item1);
							if (!IsMaterialOnSite(startingAgent, itemToConsumeEntity, tuple.Item2))
							{
								HandleDestroyedInput();
								Status = StatusOfProcess.Failed;
								return false;
							}
							list.Add(itemToConsumeEntity);
						}
						continue;
					}
					Status = StatusOfProcess.Failed;
					return false;
				}
				Status = StatusOfProcess.Failed;
				return false;
			}
		}
		if (ResourceItem.HasValue && LookUp<IResourceItem, ResourceItemID>.FindByID(ResourceItem) == null)
		{
			Status = StatusOfProcess.Failed;
			return false;
		}
		return true;
	}

	private void ConsumeInputsAndGatherParts(Entity startingAgent, Dictionary<EntityType, List<Entity>> assignedInputData, out Dictionary<EntityType, List<Entity>> parts, out float totalBulkOfInput, out Dictionary<string, float> extractedSubstances, out Vector3? lastDestroyedInputLocation)
	{
		lastDestroyedInputLocation = null;
		extractedSubstances = null;
		parts = new Dictionary<EntityType, List<Entity>>();
		totalBulkOfInput = 0f;
		float? totalBulkToConvert = MaxBulkToExtract;
		if (LimitBulkExtractionByNutrients)
		{
			totalBulkToConvert = GoalEat.CapAmountToConsume(assignedInputData.First().Value[0], startingAgent);
		}
		if (!ProcessType.IsReplenishProcess && ProcessType.InputsByType != null)
		{
			foreach (KeyValuePair<EntityType, Input> item in ProcessType.InputsByType)
			{
				Input value = item.Value;
				if (!assignedInputData.TryGetValue(value.EntityType, out var value2))
				{
					continue;
				}
				for (int i = 0; i < value.Amount.NoOfItems; i++)
				{
					Entity entity = HandleSingleInputItem(value2);
					if (ProcessType.IsSalvageProcess)
					{
						if (entity.Parts != null)
						{
							foreach (Entity part in entity.Parts)
							{
								Common.AddToMultiList(parts, part.EntityType, part);
							}
						}
						LogInputDestroyedStatistics(entity);
						lastDestroyedInputLocation = entity.AccessPoint;
						entity.Destroy(destroyParts: false);
						continue;
					}
					if (value.BecomesPartOfProductType != null)
					{
						LogInputDestroyedStatistics(entity);
						Common.AddToMultiList(parts, value.BecomesPartOfProductType, entity);
						entity.AssignedToJob = null;
						continue;
					}
					if (value.Amount.SubstanceTypes != null)
					{
						if (!entity.Find<SubstanceComponent>(out var c))
						{
							return;
						}
						foreach (SubstanceType substanceType in value.Amount.SubstanceTypes)
						{
							float amount = c.BulkAmounts[substanceType].Amount;
							float bulkToExtract = GetBulkToExtract(totalBulkToConvert, amount);
							c.ChangeSubstanceBulk(substanceType, 0f - bulkToExtract);
							AddToExtractedSubstances(ref extractedSubstances, substanceType, bulkToExtract);
							totalBulkOfInput += bulkToExtract;
						}
					}
					else
					{
						float bulkToExtract = GetBulkToExtract(totalBulkToConvert, entity.Bulk);
						entity.Bulk -= bulkToExtract;
						totalBulkOfInput += bulkToExtract;
					}
					if (Common.IsLessThanOrEqual(entity.Bulk, 0f))
					{
						LogInputDestroyedStatistics(entity);
						lastDestroyedInputLocation = entity.AccessPoint;
						entity.Destroy();
					}
					else
					{
						startingAgent.Intelligence.Allegiance.SharedKnowledge.ClearInUseBy(entity.EntityID, startingAgent.EntityID);
					}
				}
			}
		}
		if (ProcessType.IsGathering)
		{
			IResourceItem resourceItem = LookUp<IResourceItem, ResourceItemID>.FindByID(ResourceItem);
			resourceItem?.Container.GatherResource(resourceItem);
		}
	}

	private void LogInputDestroyedStatistics(Entity input)
	{
		LookUpOwners.ResolveEntityOwner((IKnownEntityData)input, out IOwner owner);
		if (owner == null || owner.Allegiance.SharedKnowledge.GetKnownData(input.ID, out var _) != EntityResult.SeenDirectly)
		{
			return;
		}
		if (Workers != null && Workers.Count > 0 && (ProcessType.IsInnateExtractionProcess || ProcessType.IsConsumeProcess))
		{
			Entity entity = Entity.FindByID(Workers[0].Item1);
			if (entity != null)
			{
				if (owner.Allegiance != entity.Intelligence.Allegiance)
				{
					owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, ProductionStatistics.StatTypes.EatenByCreatures, 1);
					return;
				}
				if (ProductionStatistics.CountMemberConsumption(entity))
				{
					owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, ProductionStatistics.StatTypes.ConsumedFood, 1);
					return;
				}
			}
		}
		owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, ProductionStatistics.StatTypes.UsedAsInput, 1);
	}

	private void AddToExtractedSubstances(ref Dictionary<string, float> extractedSubstances, SubstanceType substanceType, float extractedBulk)
	{
		if (extractedSubstances == null)
		{
			extractedSubstances = new Dictionary<string, float>();
		}
		float value2;
		float value = ((!extractedSubstances.TryGetValue(substanceType.KeyName, out value2)) ? extractedBulk : (value2 + extractedBulk));
		extractedSubstances[substanceType.KeyName] = value;
	}

	private float GetBulkToExtract(float? totalBulkToConvert, float availableBulk)
	{
		if (totalBulkToConvert.HasValue)
		{
			return Math.Min(availableBulk, totalBulkToConvert.Value);
		}
		return availableBulk;
	}

	private Entity HandleSingleInputItem(List<Entity> itemEntities)
	{
		Entity itemToConsumeEntity = itemEntities[0];
		itemEntities.RemoveAt(0);
		if (ImmovableInput == itemToConsumeEntity.EntityID)
		{
			ImmovableInput = null;
			GroundLocation = itemToConsumeEntity.Location;
		}
		if (itemToConsumeEntity.Contains != null)
		{
			itemToConsumeEntity.Contains.IterateContained(delegate(Entity e)
			{
				itemToConsumeEntity.Contains.Uncontain(e);
			});
		}
		return itemToConsumeEntity;
	}

	private bool GetGroundLocationForNewOutput(Entity agentEntity, Entity outputEntity, ProcessType process, out Vector3? location, Vector2? offset = null, Entity creatorOfItem = null)
	{
		location = null;
		if (!GetCurrentLocation(out var location2, null, ignoreKnowledge: true))
		{
			return false;
		}
		if (!location2.HasValue)
		{
			location2 = agentEntity.Location;
		}
		if (outputEntity != null || !process.IsSalvageProcess)
		{
			Entity.AddRandomOffset addRandomOffset;
			if (offset.HasValue)
			{
				addRandomOffset = Entity.AddRandomOffset.No;
				location2 += offset.Value.ToVector3();
			}
			else
			{
				addRandomOffset = Entity.AddRandomOffset.Yes;
			}
			location = MapManager.FindFreeLocation(location2.Value, outputEntity.EntityType.ItemType != null, addRandomOffset, creatorOfItem);
		}
		return true;
	}

	private bool PlaceNewItem(Entity agentEntity, Entity outputEntity, ProcessType process, Entity container = null, StorageCompartment? placeProductsInCompartment = null, Vector2? offset = null, Entity creatorOfItem = null, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		Entity entity = null;
		Vector3? location = null;
		if (container != null && container.Contains != null)
		{
			if (outputEntity != null || !process.IsSalvageProcess)
			{
				entity = container;
			}
		}
		else if (!GetGroundLocationForNewOutput(agentEntity, outputEntity, process, out location, offset, creatorOfItem))
		{
			return false;
		}
		IOwner owner = null;
		Expedition newExpedition = null;
		if (ownerOfOutput.HasValue)
		{
			owner = LookUpOwners.FindByID(ownerOfOutput);
			if (owner == null)
			{
				Status = StatusOfProcess.Failed;
				return false;
			}
			newExpedition = owner as Expedition;
		}
		if (isProductionOutput && entity != null && upgradeCategory == null && entity.Contains is IHoldsProductionOutput holdsProductionOutput && !holdsProductionOutput.HasCapacityForOutput(outputEntity))
		{
			Entity container2 = null;
			if (!entity.GetContainerOrLocation(ref container2, ref location))
			{
				Status = StatusOfProcess.Failed;
				return false;
			}
			if (container2 != null)
			{
				entity = container2;
				location = null;
			}
			else
			{
				entity = null;
			}
		}
		if (!outputEntity.PlaceEntityOnPlaySite(location, entity, Entity.StructureState.Unfinished, new Entity.SetOwnerInfo(owner), newExpedition, placeProductsInCompartment, null, isProductionOutput, null, assertContainment: true, simulateJoinedExpeditionNow: true, upgradeCategory))
		{
			outputEntity.PlaceEntityOnPlaySite(location, null, Entity.StructureState.Unfinished, new Entity.SetOwnerInfo(owner), newExpedition, placeProductsInCompartment, null, isProductionOutput);
		}
		if (outputEntity != null || !process.IsSalvageProcess)
		{
			AddOutputEntity(outputEntity);
		}
		return true;
	}

	private bool CreateRegularProduct(Output output, ref Entity outputEntity, List<Entity> listOfParts, List<Entity> partsForOneItem, Allegiance allegiance)
	{
		foreach (KeyValuePair<EntityType, int> item in output.FinalEntityTypeToCreate.Parts)
		{
			for (int i = 0; i < item.Value; i++)
			{
				Entity entity = listOfParts.Find((Entity e) => e.EntityType == item.Key);
				if (entity != null)
				{
					partsForOneItem.Add(entity);
					listOfParts.Remove(entity);
				}
			}
		}
		if (outputEntity != null)
		{
			outputEntity.NonLivingEntity.SetPartsOrCreateNew(partsForOneItem);
			outputEntity.Initialize(Site, allegiance);
			outputEntity.InitializeModelAndOnScreenFunctionality();
		}
		else
		{
			outputEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, partsForOneItem, null, allegiance);
		}
		return true;
	}

	private static void CreateSalvageProduct(Output output, Entity outputEntity, List<Entity> listOfParts)
	{
		if (outputEntity != null)
		{
			listOfParts.Remove(outputEntity);
		}
	}

	public static bool IsMaterialOnSite(Entity startingAgent, IKnownEntityData materialEntity, WorldLocation originalItemLocation)
	{
		if (materialEntity == null)
		{
			return false;
		}
		if (startingAgent != null && startingAgent.ContainsEntity(materialEntity.EntityID))
		{
			return true;
		}
		WorldLocation p = new WorldLocation(materialEntity.PlaySiteLocation);
		if (Common.DistanceOctile(originalItemLocation, p) > GameData.Instance.Constants.InteractionDistanceForAgents)
		{
			return false;
		}
		return true;
	}

	private void DestroyUnusedInputParts(Entity startingAgent, Dictionary<EntityType, List<Entity>> parts, Entity salvagedEntity, Vector3? salvagedEntityLocation)
	{
		Vector3? placeOnGround = null;
		if (!salvagedEntity.Location.HasValue && !salvagedEntity.AccessPoint.HasValue)
		{
			placeOnGround = salvagedEntityLocation;
		}
		foreach (KeyValuePair<EntityType, List<Entity>> part in parts)
		{
			if (salvagedEntity.EntityType.IsInOriginalBlueprint(part.Key))
			{
				for (int num = part.Value.Count - 1; num >= 0; num--)
				{
					Entity entity = part.Value[num];
					Container.EjectEntity(entity, salvagedEntity, null, null, null, null, placeOnGround);
					entity.Destroy();
					part.Value.RemoveAt(num);
				}
			}
			else
			{
				while (part.Value.Count != 0)
				{
					Entity outputEntity = part.Value[0];
					PlaceNewItem(startingAgent, outputEntity, ProcessType);
					part.Value.RemoveAt(0);
				}
			}
		}
	}

	public bool GetOutputEntityData(Predicate<IKnownEntityData> predicate, out IKnownEntityData matchingData, SharedKnowledge sharedKnowledge)
	{
		matchingData = null;
		if (OutputEntities != null && OutputEntities.Count > 0)
		{
			foreach (EntityID outputEntity in OutputEntities)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(outputEntity, out var data)))
				{
					return false;
				}
				if (predicate(data))
				{
					matchingData = data;
					return true;
				}
			}
		}
		return true;
	}

	private bool GetOutputEntity(Predicate<Entity> predicate, out Entity matchingEntity)
	{
		matchingEntity = null;
		if (OutputEntities != null && OutputEntities.Count > 0)
		{
			foreach (EntityID outputEntity in OutputEntities)
			{
				Entity entity = Entity.FindByID(outputEntity);
				if (entity != null)
				{
					if (predicate(entity))
					{
						matchingEntity = entity;
						return true;
					}
					continue;
				}
				return false;
			}
		}
		return true;
	}

	private bool CreateOutputsFromInputs(Entity startingAgent, ProcessType process, Entity placeProductsInContainer, Dictionary<EntityType, List<Entity>> parts, Output output, int noOfItemsToCreate, float? bulkOfEachOutputItem)
	{
		Entity matchingEntity = null;
		List<Entity> list = new List<Entity>();
		if (placeProductsInContainer == null)
		{
			placeProductsInContainer = output.GetToolContainerToPlaceOutputIn(StationaryTools);
			if (placeProductsInContainer == null && output.FinalEntityTypeToCreate.Upgrader != null)
			{
				placeProductsInContainer = Entity.FindByID(ActingOnEntity.Value.Entity);
			}
		}
		if (!GetOutputEntity((Entity e) => e.EntityType == output.FinalEntityTypeToCreate, out matchingEntity))
		{
			return false;
		}
		Allegiance allegiance = null;
		IOwner owner = LookUpOwners.FindByID(ownerOfOutput);
		if (owner != null)
		{
			allegiance = owner.Allegiance;
		}
		for (int num = 0; num < noOfItemsToCreate; num++)
		{
			list.Clear();
			if (parts.TryGetValue(output.FinalEntityTypeToCreate, out var value) && value.Count > 0)
			{
				if (process.IsSalvageProcess)
				{
					matchingEntity = value.Find((Entity e) => e.EntityType == output.FinalEntityTypeToCreate);
					CreateSalvageProduct(output, matchingEntity, value);
				}
				else if (!CreateRegularProduct(output, ref matchingEntity, value, list, allegiance))
				{
					return false;
				}
			}
			else if (process.IsSalvageProcess)
			{
				if (!output.IsWasteProduct)
				{
					return true;
				}
				matchingEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, null, null, allegiance);
			}
			else if (matchingEntity == null)
			{
				matchingEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, null, null, allegiance);
			}
			if (matchingEntity != null || !process.IsSalvageProcess)
			{
				matchingEntity.NonLivingEntity.Progress = 0f;
			}
			if (!process.IsSalvageProcess && bulkOfEachOutputItem.HasValue)
			{
				matchingEntity.Bulk = bulkOfEachOutputItem.Value;
			}
			if (!matchingEntity.HasLocation && !PlaceNewItem(startingAgent, matchingEntity, process, placeProductsInContainer, PlaceProductsInCompartment, output.RelativePlacement, startingAgent, isProductionOutput: true, UpgradeCategory))
			{
				return false;
			}
			matchingEntity = null;
		}
		return true;
	}

	public void AssignInput(IKnownEntityData item, out bool wasAssigned)
	{
		wasAssigned = false;
		if (IsStarted)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, List<Tuple<EntityID, WorldLocation>>> assignedInput in AssignedInputs)
		{
			for (int num = assignedInput.Value.Count - 1; num >= 0; num--)
			{
				if (Entity.FindByID(assignedInput.Value[num].Item1) == null)
				{
					assignedInput.Value.RemoveAt(num);
				}
			}
		}
		if (!ProcessType.InputsByType.TryGetValue(item.EntityType, out var value))
		{
			return;
		}
		if (value.InputIsImmovable())
		{
			ImmovableInput = item.EntityID;
		}
		if (!AssignedInputs.TryGetValue(item.EntityType, out var value2))
		{
			return;
		}
		Tuple<EntityID, WorldLocation> tuple = value2.Find((Tuple<EntityID, WorldLocation> t) => t.Item1 == item.EntityID);
		if (tuple != null)
		{
			value2.Remove(tuple);
			value2.Add(new Tuple<EntityID, WorldLocation>(item.EntityID, new WorldLocation(item.PlaySiteLocation)));
			wasAssigned = true;
			return;
		}
		InputAmount amount = value.Amount;
		if (amount.NoOfItems > 0)
		{
			if (!amount.NoOfItems.HasValue)
			{
				throw new Exception("Not implemented!!");
			}
			if (value2.Count < amount.NoOfItems.Value)
			{
				value2.Add(new Tuple<EntityID, WorldLocation>(item.EntityID, new WorldLocation(item.PlaySiteLocation)));
				wasAssigned = true;
			}
		}
	}

	private bool CheckStructureAreaCleared(float newProgress)
	{
		return true;
	}

	public void Abort()
	{
	}

	public static bool HasFixedLocation(Vector3? groundLocation)
	{
		return groundLocation.HasValue;
	}

	public bool HasFixedLocation()
	{
		return HasFixedLocation(GroundLocation);
	}

	public void Destroy()
	{
		if (id == (SimProcessID)32L)
		{
			id = (SimProcessID)32L;
		}
		if (id != SimProcessID.Invalid)
		{
			The.Sim.PlaySite.PlaySite.RemoveProcess(this);
			DestroyUnstartedOutputs();
			FireProcessDestroyedEvent();
			AssignProcessToInvolvedEntitiesOrTile(remove: true);
			RemoveIDEntry();
		}
	}

	public SimProcessID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= SimProcessID.Invalid)
		{
			throw new Exception("Astounding, SimProcessID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SimProcessID SnapshotID(Snapshotter sn, SimProcessID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SimProcessID.Invalid)
		{
			LookUp<SimProcess, SimProcessID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = SimProcessID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<SimProcess, SimProcessID>.Remove(this);
	}

	void ILookUp<SimProcess, SimProcessID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SimProcessID.First;
	}

	void ILookUp<SimProcess, SimProcessID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<SimProcess, SimProcessID>.Create();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion((Snapshotter.Version)2u);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ID = SnapshotID(sn, ID);
		IDCounter = sn.DoEnum(IDCounter);
		AssignedInputs = sn.DoMultiMap(AssignedInputs);
		immovableTool = sn.DoEntityIDNullable(immovableTool);
		immovableInput = sn.DoEntityIDNullable(immovableInput);
		groundLocation = sn.DoVector3Nullable(groundLocation);
		ContainerToPlaceOutputsIn = sn.DoEntityIDNullable(ContainerToPlaceOutputsIn);
		MaxBulkToExtract = sn.DoFloatNullable(MaxBulkToExtract);
		OutputEntities = sn.DoList(OutputEntities);
		PlaceProductsInCompartment = sn.DoEnumNullable(PlaceProductsInCompartment);
		Progress = sn.DoFloat(Progress);
		ProgressSpeed = sn.DoFloat(ProgressSpeed);
		IsStarted = sn.DoBool(IsStarted);
		Status = sn.DoEnum(Status);
		workers = sn.DoList(workers);
		ProcessType = sn.DoGameData(ProcessType);
		StationaryTools = sn.DoList(StationaryTools);
		ResourceItem = sn.DoEnumNullable(ResourceItem);
		ActingOnEntity = sn.DoEntityAndRootNullable(ActingOnEntity);
		snapshotProcessToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(StationaryToolsTypeCombination);
		UpgradeCategory = sn.DoGameData(UpgradeCategory);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		ownerOfOutput = sn.DoEnumNullable(ownerOfOutput);
		updateInterval = sn.DoDoubleNullable(updateInterval);
		timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
		phase = sn.DoEnum(phase);
		productivity = (Productivity)sn.DoISnapshot(productivity);
		ProcessCompletedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessCompletedEvent);
		ProcessDestroyedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessDestroyedEvent);
		ProcessStartedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessStartedEvent);
		ProcessProducingEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessProducingEvent);
		assignedSubstances = sn.DoDictionary(assignedSubstances);
		requestedSubstances = sn.DoDictionary(requestedSubstances);
		ignoreProgressCap = sn.DoBool(ignoreProgressCap);
		suppressSpawningEvents = sn.DoBool(suppressSpawningEvents);
		MapPosition = sn.DoPointNullable(MapPosition);
		if (version >= (Snapshotter.Version)2u)
		{
			LimitBulkExtractionByNutrients = sn.DoBool(LimitBulkExtractionByNutrients);
		}
		else
		{
			LimitBulkExtractionByNutrients = false;
		}
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotProcessToolCombo.HasValue)
		{
			StationaryToolsTypeCombination = ToolTypeCombination.FindByID(snapshotProcessToolCombo.Value);
		}
		if (productivity != null)
		{
			productivity.LoadPostProcess(sn);
		}
		RecomputeMapPosition();
		CreateRegulators();
	}
}
