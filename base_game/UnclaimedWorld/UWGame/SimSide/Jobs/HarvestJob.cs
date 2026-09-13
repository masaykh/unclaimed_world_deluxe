using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class HarvestJob : ISnapshot
{
	public ProcessJob ProcessJob;

	private JobID snapshotJob;

	public bool IsSpecificJob;

	public IResourceItem Item;

	private ResourceItemID snapshotItem;

	public ResourceType ResourceType;

	public EntityGroupID Owner;

	public Zone Zone;

	private ZoneID snapshotZone;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public HarvestJob()
	{
	}

	public HarvestJob(ProcessJob processJob, IResourceItem item, EntityGroupID owner)
	{
		ProcessJob = processJob;
		Item = item;
		ResourceType = item.Container.ResourceType;
		SimProcess.FindById(ProcessJob.ProductionProcess).ResourceItem = Item.ID;
		IsSpecificJob = true;
		item.AssignedToJob = ProcessJob;
		Owner = owner;
		TerrainTile tile = The.Map.GetTile(item.Container.MapPosition);
		if (tile.HarvestJobs == null)
		{
			tile.HarvestJobs = new Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>>();
		}
		if (!tile.HarvestJobs.TryGetValue(owner, out var value))
		{
			value = new Dictionary<ResourceType, List<ProcessJob>>();
			tile.HarvestJobs.Add(owner, value);
		}
		if (!value.TryGetValue(ResourceType, out var value2))
		{
			value2 = new List<ProcessJob>();
			value.Add(ResourceType, value2);
		}
		value2.Add(processJob);
	}

	public GoalEvaluator.CalculateResult ScoreThisJobWithoutTools(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Intelligence entityIntelligence, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, ref double rating, ref IResourceItem currentResourceItem, EntityGroup owner, float priority, ref bool jobIsValid)
	{
		_ = entity.PersonEntity;
		ProcessType processType = ProcessJob.ProcessType;
		if (entityIntelligence.HasSkill(processType.RequiredSkillType))
		{
			if (!ProcessJob.IsStarted(out var isStarted, out var processData))
			{
				jobIsValid = false;
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			if (isStarted && processType.WorkNeeded != WorkerNeededOptions.WorkerNeeded)
			{
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			if (IsSpecificJob && Item != null)
			{
				currentResourceItem = Item;
			}
			else
			{
				currentResourceItem = null;
				ResourceMapForAgent.Result bestHarvestLocation = entity.Intelligence.CropsMapForAgent.GetBestHarvestLocation(this, ref currentResourceItem);
				if (bestHarvestLocation == ResourceMapForAgent.Result.Wait)
				{
					return GoalEvaluator.CalculateResult.Processing;
				}
				if (bestHarvestLocation == ResourceMapForAgent.Result.NoTarget || currentResourceItem == null)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
			}
			SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
			if (!processData.GetKnownProgress(sharedKnowledge, out var progress))
			{
				jobIsValid = false;
				rating = 0.0;
				return GoalEvaluator.CalculateResult.Done;
			}
			double num = GoalEvaluator.ScoreJobProgress(progress);
			double num2 = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, ProcessJob.MaxJobPositions);
			double num3 = GoalEvaluator.ScoreUniqueSkill(entityIntelligence, processType.RequiredSkillType);
			double num4 = ProcessJob.GetImportance(owner);
			double num5 = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(processType.RequiredSkillType));
			float uniqueSkillWeight = GameData.Instance.AIConstants.EvaluatorWeights.UniqueSkillWeight;
			EvaluatorWeights evaluatorWeights = GameData.Instance.AIConstants.EvaluatorWeights;
			rating = 0.3 + (double)evaluatorWeights.HarvestProgressWeight * num + 0.1 * num2 + (double)evaluatorWeights.HarvestImportanceWeight * num4 + (double)evaluatorWeights.HarvestSkillWeight * num5 + (double)uniqueSkillWeight * num3;
			rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	public bool CanBeQueuedInGoal()
	{
		return ProcessJob.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded;
	}

	public void Destroy()
	{
		if (Item != null)
		{
			if (Item.Container is Crop { ParentEntity: not null } crop)
			{
				crop.ParentEntity.Renderable.SetResourceContainerTintColor(Color.White);
			}
			else if (Item.Container is TileResourceContainer tileResourceContainer)
			{
				tileResourceContainer.Renderable.SetResourceContainerTintColor(Color.White);
			}
			if ((!IsSpecificJob || ProcessJob.HarvestJob != null) && Item.AssignedToJob == ProcessJob)
			{
				Item.AssignedToJob = null;
			}
			TerrainTile terrainTile = The.Map.TileMap[Item.Container.MapPosition.X][Item.Container.MapPosition.Y];
			if (terrainTile.HarvestJobs != null)
			{
				terrainTile.HarvestJobs[ProcessJob.EntityGroupID][Item.Container.ResourceType].Remove(ProcessJob);
			}
			if (Zone != null)
			{
				Zone.RemoveHarvestJob(ProcessJob);
			}
		}
	}

	public void Abandon(Entity entity)
	{
		if (!IsSpecificJob)
		{
			if (Item.AssignedToJob == ProcessJob)
			{
				Item.AssignedToJob = null;
			}
			Item = null;
		}
	}

	public override string ToString()
	{
		if (Item != null)
		{
			return "Harvest " + Item.Container.ResourceType.Name + " at: " + Item.Container.MapPosition;
		}
		return "Harvest " + ResourceType.Name;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotJob = sn.SnapshotID<Job, JobID>(ProcessJob).Value;
		IsSpecificJob = sn.DoBool(IsSpecificJob);
		ResourceType = sn.DoGameData(ResourceType);
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone).Value;
		snapshotItem = sn.SnapshotID<IResourceItem, ResourceItemID>(Item).Value;
		Owner = sn.DoEnum(Owner);
		sn.Ignore(ProcessJob);
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
		ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
		Item = LookUp<IResourceItem, ResourceItemID>.FindByID(snapshotItem);
	}
}
