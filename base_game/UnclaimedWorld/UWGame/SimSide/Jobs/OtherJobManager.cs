using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Jobs;

public class OtherJobManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		CleanupJobs,
		StokeFireJobs,
		LightFireJobs,
		ReplenishJobs,
		RepairJobs,
		UpgradeJobs,
		CheckingJobs,
		RemoveExcessTakers
	}

	private Regulator regulator;

	private EntityGroup owner;

	private EntityGroupID snapshotOwnerID;

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

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public OtherJobManager(EntityGroup owner)
	{
		this.owner = owner;
		AddToLookup();
		CreateRegulators();
	}

	public OtherJobManager()
	{
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "OtherJobManager");
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		RemoveIDEntry();
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

	private bool LightFireJobAlreadyExists(Entity fireplace)
	{
		foreach (Job otherJob in owner.OtherJobs)
		{
			if (otherJob is LightFireJob lightFireJob && lightFireJob.FireSite == fireplace)
			{
				return true;
			}
		}
		return false;
	}

	private Job GetReloadJobIfExists(EntityID entity)
	{
		foreach (Job otherJob in owner.OtherJobs)
		{
			if (otherJob is ProcessJob { ReplenishJob: not null } processJob && processJob.ReplenishJob.EntityToReplenish == entity && processJob.ReplenishJob.Action == GoalReplenish.ReplenishAction.Reload)
			{
				return processJob;
			}
		}
		return null;
	}

	private Job GetRepairJobIfExists(RepairPackageAction partRepairAction, IKnownEntityData entityData)
	{
		if (owner.RepairJobs.TryGetValue(entityData.EntityID, out var value))
		{
			foreach (ProcessJob item in value)
			{
				if (item.RepairJob.RepairActionToUse == partRepairAction.RepairAction)
				{
					return item;
				}
			}
		}
		return null;
	}

	private LightFireJob GetLightFireJobIfExists(Entity fireplace)
	{
		foreach (Job otherJob in owner.OtherJobs)
		{
			if (otherJob is LightFireJob lightFireJob && lightFireJob.FireSite == fireplace)
			{
				return lightFireJob;
			}
		}
		return null;
	}

	private StokeFireJob GetStokeFireJobIfExists(IKnownEntityData fireplace)
	{
		foreach (Job otherJob in owner.OtherJobs)
		{
			if (otherJob is StokeFireJob stokeFireJob && stokeFireJob.FireSite == fireplace)
			{
				return stokeFireJob;
			}
		}
		return null;
	}

	private bool JobTypeAlreadyExists(Type typeOfJob)
	{
		foreach (Job otherJob in owner.OtherJobs)
		{
			if (typeOfJob.IsInstanceOfType(otherJob))
			{
				return true;
			}
		}
		return false;
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

	private void CreateSalvageUpgradeJob(IKnownEntityData entityData, EntityGroup owner)
	{
		Salvage.CreateSalvageJob(entityData, owner);
	}

	private bool CreateUpgradeJob(IKnownEntityData entityData, ProcessType processType, UpgradeCategory upgradeCategory, EntityGroup owner)
	{
		if (processType != null && CreateProcessJob(entityData, owner, processType, canCancel: false, upgradeCategory, out var _))
		{
			return true;
		}
		return false;
	}

	private bool CreateReplenishJob(IKnownEntityData entityData, EntityGroup owner)
	{
		ProcessType processType = null;
		processType = entityData.EntityType.ContainerType.GetReplenishProcesses().First().Value;
		if (processType != null && CreateProcessJob(entityData, owner, processType, canCancel: false, null, out var processJobToAdd))
		{
			processJobToAdd.ReplenishJob = new ReplenishJob(entityData.EntityID, GoalReplenish.ReplenishAction.Reload);
			return true;
		}
		return false;
	}

	private static bool CreateProcessJob(IKnownEntityData entityData, EntityGroup owner, ProcessType processType, bool canCancel, UpgradeCategory upgradeCategory, out ProcessJob processJobToAdd)
	{
		Vector3? productionSiteLocation = null;
		EntityID? containerToPlaceOutputsIn = null;
		if (entityData.ContainedBy.HasValue)
		{
			containerToPlaceOutputsIn = entityData.ContainedBy;
		}
		else
		{
			productionSiteLocation = entityData.AccessPoint;
		}
		processJobToAdd = JobManager.CreateProcessJob(null, owner, processType, containerToPlaceOutputsIn, productionSiteLocation, entityData.GetAsEntityAndRoot(), isSalvage: false, upgradeCategory);
		if (!processJobToAdd.GetFixedJobLocation(out var location))
		{
			return false;
		}
		if (location.HasValue)
		{
			processJobToAdd.CreateHaulingJobsForProcessInputs(owner, location.Value);
		}
		return true;
	}

	private static bool CreateRepairJob(IKnownEntityData entityData, RepairPackageAction repairPart, EntityGroup owner)
	{
		ProcessType repairProcess = repairPart.RepairProcess;
		if (repairProcess != null)
		{
			Vector3? productionSiteLocation = null;
			EntityID? containerToPlaceOutputsIn = null;
			if (entityData.ContainedBy.HasValue)
			{
				containerToPlaceOutputsIn = entityData.ContainedBy;
			}
			else
			{
				productionSiteLocation = entityData.AccessPoint;
			}
			ProcessJob processJob = JobManager.CreateProcessJob(null, owner, repairProcess, containerToPlaceOutputsIn, productionSiteLocation, entityData.GetAsEntityAndRoot(), repairPart.RepairAction, (repairPart.Part != null) ? new EntityAndRoot?(repairPart.Part.GetAsEntityAndRoot()) : ((EntityAndRoot?)null));
			if (!processJob.GetFixedJobLocation(out var location))
			{
				return false;
			}
			if (location.HasValue)
			{
				processJob.CreateHaulingJobsForProcessInputs(owner, location.Value);
			}
			return true;
		}
		return false;
	}

	private static double ScoreNeedForFireAsProtection(IKnownEntityData buildingEntity)
	{
		double result = 0.0;
		if (buildingEntity.EntityType.StructureType.IsCamp)
		{
			result = 1.0;
		}
		return result;
	}

	private static void RecordOutdatedJob(ref List<Job> outdatedJobs, Job job)
	{
		if (outdatedJobs != null)
		{
			outdatedJobs.Add(job);
			return;
		}
		outdatedJobs = new List<Job>();
		outdatedJobs.Add(job);
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"OtherJobManager {ID}:");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.CleanupJobs:
			CleanupJobs();
			phase = Phase.StokeFireJobs;
			break;
		case Phase.StokeFireJobs:
			_ = owner.GetAllegiance().SharedKnowledge;
			phase = Phase.ReplenishJobs;
			break;
		case Phase.ReplenishJobs:
			CycleReplenish();
			phase = Phase.CheckingJobs;
			break;
		case Phase.CheckingJobs:
			CycleChecking();
			phase = Phase.RepairJobs;
			break;
		case Phase.RepairJobs:
			CycleRepair();
			phase = Phase.UpgradeJobs;
			break;
		case Phase.UpgradeJobs:
			CycleUpgrade();
			phase = Phase.RemoveExcessTakers;
			break;
		case Phase.RemoveExcessTakers:
			CycleRemoveExcessTakers();
			phase = Phase.LightFireJobs;
			break;
		case Phase.LightFireJobs:
			phase = Phase.CleanupJobs;
			return true;
		}
		return false;
	}

	private void CycleReplenish()
	{
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		foreach (KeyValuePair<EntityType, List<EntityID>> allEntity in owner.AllEntities)
		{
			if (!allEntity.Key.RequiresOutsideReplenishment())
			{
				continue;
			}
			for (int num = allEntity.Value.Count - 1; num >= 0; num--)
			{
				EntityID entityID = allEntity.Value[num];
				if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out var entityData) && entityData.IsCompleted() && GoalEvaluator.IsOnPlaySite(entityData) && Entity.IsFunctional(entityData) && entityData.NeedsReload(sharedKnowledge, out var itemToReload) && GetReloadJobIfExists(itemToReload.EntityID) == null)
				{
					CreateReplenishJob(itemToReload, owner);
				}
			}
		}
	}

	private static ProcessType GetUpgradeProcess(EntityType upgradeType)
	{
		if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(upgradeType, out var value))
		{
			return value.FirstOrDefault((ProcessType p) => p.IsUpgrade);
		}
		return null;
	}

	private void CycleUpgrade()
	{
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		List<EntityID> invalidEntities = null;
		List<Job> otherJobs = owner.OtherJobs;
		foreach (KeyValuePair<EntityID, Dictionary<UpgradeCategory, EntityType>> upgrade in owner.Upgrades)
		{
			EntityID key = upgrade.Key;
			if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, key, owner, out var entityData, ref invalidEntities))
			{
				continue;
			}
			foreach (KeyValuePair<UpgradeCategory, EntityType> item in upgrade.Value)
			{
				UpgradeCategory key2 = item.Key;
				EntityType value = item.Value;
				if (entityData.ContainedUpgrades != null && entityData.ContainedUpgrades.TryGetValue(key2, out var value2))
				{
					if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, value2, owner, out var entityData2, ref invalidEntities))
					{
						continue;
					}
					if (entityData2.EntityType != value || !Entity.IsFunctional(entityData2))
					{
						_ = entityData2.EntityType.NonLivingType.SalvageProcessType;
						if (!Salvage.SalvageJobExists(entityData2))
						{
							CreateSalvageUpgradeJob(entityData2, owner);
							continue;
						}
					}
				}
				if (value != null && (entityData.ContainedUpgrades == null || !entityData.ContainedUpgrades.ContainsKey(key2)))
				{
					ProcessType upgradeProcess = GetUpgradeProcess(value);
					if (!SpecialAction.ActionJobExists(entityData, upgradeProcess, otherJobs))
					{
						CreateUpgradeJob(entityData, upgradeProcess, key2, owner);
					}
				}
			}
		}
		if (invalidEntities == null)
		{
			return;
		}
		foreach (EntityID item2 in invalidEntities)
		{
			owner.DeleteEntity(item2, null);
		}
	}

	private void CycleChecking()
	{
		_ = owner.GetAllegiance().SharedKnowledge;
		List<ProcessJob> list = null;
		owner.UnattendedProcessJobs.Sort(ProcessJob.CompareByEstimatedCompletion);
		foreach (ProcessJob unattendedProcessJob in owner.UnattendedProcessJobs)
		{
			double? estimatedCompletionTime = unattendedProcessJob.GetEstimatedCompletionTime();
			if (estimatedCompletionTime.HasValue && The.Sim.TimepointReached(estimatedCompletionTime.Value))
			{
				if (!CheckingJobExists(unattendedProcessJob))
				{
					Common.AddToList(ref list, unattendedProcessJob);
				}
				continue;
			}
			break;
		}
		if (list != null)
		{
			foreach (ProcessJob item in list)
			{
				if (item.GetCurrentJobLocation(out var location) && location.HasValue)
				{
					List<Pair<JobID, Vector2>> resultsList = null;
					owner.CheckProcessJobsQuadTree.GetEntitiesInRange(location.Value.ToVector2(), GameData.Instance.AIConstants.CheckingJobRange, null, ref resultsList);
					CheckProcessJob checkProcessJob = ((resultsList == null || resultsList.Count <= 0) ? new CheckProcessJob(location.Value, owner) : ((CheckProcessJob)LookUp<Job, JobID>.FindByID(resultsList[0].First)));
					item.CheckingJobID = checkProcessJob.ID;
					checkProcessJob.ProcessJobsToCheckOn.Add(item.ID);
				}
			}
		}
		for (int num = owner.CheckProcessJobs.Count - 1; num >= 0; num--)
		{
			CheckProcessJob checkProcessJob2 = owner.CheckProcessJobs[num] as CheckProcessJob;
			for (int num2 = checkProcessJob2.ProcessJobsToCheckOn.Count - 1; num2 >= 0; num2--)
			{
				if (LookUp<Job, JobID>.FindByID(checkProcessJob2.ProcessJobsToCheckOn[num2]) == null)
				{
					checkProcessJob2.ProcessJobsToCheckOn.RemoveAt(num2);
				}
			}
			if (checkProcessJob2.ProcessJobsToCheckOn.Count == 0)
			{
				checkProcessJob2.Destroy(cancelTakers: true);
			}
		}
	}

	private bool CheckingJobExists(ProcessJob processJob)
	{
		if (processJob.CheckingJobID.HasValue)
		{
			if (LookUp<Job, JobID>.FindByID(processJob.CheckingJobID.Value) != null)
			{
				return true;
			}
			processJob.CheckingJobID = null;
		}
		return false;
	}

	private void CycleRepair()
	{
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		foreach (KeyValuePair<EntityType, List<EntityID>> allEntity in owner.AllEntities)
		{
			if (!allEntity.Key.IsRepairable())
			{
				continue;
			}
			for (int num = allEntity.Value.Count - 1; num >= 0; num--)
			{
				EntityID entityID = allEntity.Value[num];
				if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out var entityData))
				{
					CreateRepairJobIfNeeded(owner, entityData);
				}
			}
		}
	}

	public static void CreateRepairJobIfNeeded(EntityGroup owner, IKnownEntityData entityData)
	{
		if (entityData.IsCompleted() && GoalEvaluator.IsOnPlaySite(entityData))
		{
			if (entityData.NeedsRepair())
			{
				RepairPackage repairPackage = entityData.ComputeBestRepairPackage();
				CreateRepairJobs(owner, entityData, repairPackage);
			}
			else
			{
				RemoveAllRepairJobs(owner, entityData);
			}
		}
	}

	private static void CreateRepairJobs(EntityGroup owner, IKnownEntityData entityData, RepairPackage repairPackage)
	{
		owner.RepairJobs.TryGetValue(entityData.EntityID, out var value);
		if (value != null)
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				ProcessJob processJob = value[num];
				if (repairPackage == null || !repairPackage.MatchesJob(processJob))
				{
					processJob.Destroy(removeTakers: true);
				}
			}
		}
		if (repairPackage == null)
		{
			return;
		}
		foreach (RepairPackageAction action in repairPackage.Actions)
		{
			if (value == null || !value.Any((ProcessJob j) => repairPackage.MatchesJob(j)))
			{
				CreateRepairJob(entityData, action, owner);
			}
		}
	}

	private static void RemoveAllRepairJobs(EntityGroup owner, IKnownEntityData entityData)
	{
		owner.RepairJobs.TryGetValue(entityData.EntityID, out var value);
		if (value == null)
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			ProcessJob processJob = value[num];
			if (processJob.TakenBy.Count == 0)
			{
				processJob.Destroy(removeTakers: true);
			}
		}
	}

	private bool CleanupJobs()
	{
		Allegiance allegiance = owner.GetAllegiance();
		List<Job> list = null;
		OwnerID? ownerID = owner.GetOwnerID();
		for (int i = 0; i < owner.OtherJobs.Count; i++)
		{
			Job job = owner.OtherJobs[i];
			if (job is ProcessJob processJob && processJob.GetActingOnEntity(out EntityAndRoot? actingOnEntity) && actingOnEntity.HasValue)
			{
				if (!actingOnEntity.Value.IsValid(allegiance.SharedKnowledge, out var actingOnEntityData))
				{
					Common.AddToList(ref list, job);
				}
				else if (processJob.RequiresOwnedActingOnEntity() && actingOnEntityData.OwnedBy != ownerID)
				{
					Common.AddToList(ref list, job);
				}
				else if (!allegiance.SharedKnowledge.SpecialActionIsAvailable(actingOnEntityData, processJob.ProcessType))
				{
					Common.AddToList(ref list, job);
				}
			}
		}
		if (list != null)
		{
			foreach (Job item in list)
			{
				item.Destroy(cancelTakers: true);
			}
		}
		return true;
	}

	private void CycleRemoveExcessTakers()
	{
		for (int num = owner.VariableMaxTakerJobs.Count - 1; num >= 0; num--)
		{
			Job job = owner.VariableMaxTakerJobs[num];
			if (job.MaxJobPositions == 0)
			{
				job.Destroy(cancelTakers: true);
			}
			else if (job.TakenBy.Count > 0)
			{
				int num2 = job.TakenBy.Count - job.MaxJobPositions;
				if (num2 > 0)
				{
					List<Entity> takersSortedByDistance = job.GetTakersSortedByDistance();
					int num3 = 0;
					foreach (Entity item in takersSortedByDistance)
					{
						if (item.Intelligence.Brain.SendMessage(new Message(Message.MessageTypes.CancelJobOrItemInUse)))
						{
							num3++;
							if (num3 == num2)
							{
								break;
							}
						}
					}
				}
			}
		}
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
