using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI;

public class ResourceMapForAgent : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot, IIDEventSubscriber
{
	private enum Phase
	{
		GetResourceMap,
		Clear,
		DrawDistances,
		DrawOtherAgents,
		Search
	}

	public enum Result
	{
		Wait,
		NoTarget,
		OK
	}

	private Phase phase;

	private ushort[][] Values;

	private int maxValue;

	private int twoTimesMaxMapOctileDistance;

	private Entity entity;

	private EntityID snapshotEntity;

	private HarvestJob harvestJob;

	private JobID? snapshotHarvestJob;

	private byte[][] cropsMap;

	private MethodID notifyWhenRegionSearchIsFinishedMethodID;

	private ResourceItemID? bestResourceItemID;

	private const int resourcesToDrawPerCycle = 20;

	private Regulator regulator;

	private bool isDirty = true;

	private MethodID crops_FinishedMethodID;

	private MethodID listOfCrops_ListItemRemovedMethodID;

	private int cropCounter;

	private int rowCounter;

	private bool isWaiting;

	private static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused => isWaiting;

	public double? UpdateInterval => 6.0;

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

	public ResourceMapForAgent()
	{
	}

	public ResourceMapForAgent(Entity entity)
	{
		AddToLookup();
		Values = entity.Intelligence.EvaluatorInfluenceMap;
		this.entity = entity;
		int num = (int)Common.DistanceOctile(Point.Zero, new Point(The.Map.mapTileWidth, The.Map.mapTileHeight));
		twoTimesMaxMapOctileDistance = 2 * num;
		maxValue = 65535;
		crops_FinishedMethodID = ActionLookup.AddWithNewID(crops_FinishedEvent);
		notifyWhenRegionSearchIsFinishedMethodID = ActionLookup.AddWithNewID(NotifyWhenRegionSearchIsFinished);
		listOfCrops_ListItemRemovedMethodID = ActionLookup<int>.AddWithNewID(listOfCrops_ListItemRemoved);
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "ResourceMapForAgent");
	}

	public void Destroy()
	{
		RemoveIDEntry();
		The.Sim.CycleManager.UnRegister(this);
		ActionLookup.Remove(crops_FinishedMethodID);
		ActionLookup<int>.Remove(listOfCrops_ListItemRemovedMethodID);
	}

	public Result GetBestHarvestLocation(HarvestJob harvestJob, ref IResourceItem bestResourceItem)
	{
		if (!The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out var value))
		{
			return Result.NoTarget;
		}
		if (value.Count == 0)
		{
			return Result.NoTarget;
		}
		if (this.harvestJob == null || this.harvestJob.ResourceType != harvestJob.ResourceType || isDirty)
		{
			if (!The.Sim.CycleManager.IsRegistered(this))
			{
				this.harvestJob = harvestJob;
				bestResourceItem = null;
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
			}
			return Result.Wait;
		}
		bestResourceItem = LookUp<IResourceItem, ResourceItemID>.FindByID(bestResourceItemID);
		return Result.OK;
	}

	private void listOfCrops_ListItemRemoved(int indexOfRemovedItem)
	{
		ObservableList<ResourceContainer>.UpdateCounterWhenItemIsRemoved(ref cropCounter, indexOfRemovedItem);
	}

	public void Update(GameTime gameTime)
	{
		if (regulator.IsReady())
		{
			isDirty = true;
		}
	}

	public void NotifyWhenRegionSearchIsFinished()
	{
		isWaiting = false;
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"ResourceMapForAgent {ID}:");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.GetResourceMap:
		{
			ResourceMap resourceMap = entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(harvestJob.ResourceType);
			if (resourceMap.GetMap(ref cropsMap) == ResourceMap.Result.OK)
			{
				phase = Phase.Clear;
			}
			else
			{
				resourceMap.FinishedEvent.Add(crops_FinishedMethodID, this);
				isWaiting = true;
			}
			return false;
		}
		case Phase.Clear:
		{
			phase = Phase.DrawDistances;
			The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out var value4);
			value4.ListMemberRemoved.Add(listOfCrops_ListItemRemovedMethodID, this);
			return false;
		}
		case Phase.DrawDistances:
		{
			ThreatStance threatStanceToUse;
			RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
			SubtileLayers map = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, entity.Intelligence.ThreatStance).Layers[entity.GetTransportType()];
			Point fromSubtile = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
			float distance = 0f;
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(harvestJob.Owner);
			if (entityGroup == null)
			{
				harvestJob.ProcessJob.Destroy(removeTakers: true);
				harvestJob = null;
				phase = Phase.GetResourceMap;
				EndSearch();
				return true;
			}
			Point fromSubtile2 = MapManager.WorldPosToSubtile(entityGroup.Parent.Location.Value);
			float distance2 = 0f;
			ObservableList<ResourceContainer> observableList2 = The.Sim.PlaySite.Resources[harvestJob.ResourceType];
			int num3 = cropCounter;
			int num4 = Common.Min(cropCounter + 20, observableList2.Count);
			Point zero = Point.Zero;
			for (int j = num3; j < num4; j++)
			{
				ResourceContainer resourceContainer2 = observableList2[j];
				zero = resourceContainer2.MapPosition;
				byte b = cropsMap[zero.X][zero.Y];
				if (b == 0 || Values[zero.X][zero.Y] >= b)
				{
					continue;
				}
				if (!The.Map.GetClosestAccessiblePoint(map, null, resourceContainer2.Location, stayInsideTile: true, out var closestSubtile))
				{
					Values[zero.X][zero.Y] = 0;
					continue;
				}
				Point value = closestSubtile.Value;
				switch (regionMapAndStanceForEvaluator.GetDistance(entity, fromSubtile2, value, ref distance2, sendMessageToEntity: false, notifyWhenRegionSearchIsFinishedMethodID))
				{
				case RegionMap.Result.Wait:
					isWaiting = true;
					return false;
				case RegionMap.Result.NoAccess:
					Values[zero.X][zero.Y] = 0;
					continue;
				}
				switch (regionMapAndStanceForEvaluator.GetDistance(entity, fromSubtile, value, ref distance, sendMessageToEntity: false, notifyWhenRegionSearchIsFinishedMethodID))
				{
				case RegionMap.Result.Wait:
					isWaiting = true;
					return false;
				case RegionMap.Result.NoAccess:
					Values[zero.X][zero.Y] = 0;
					continue;
				}
				int num5 = (int)Common.Clamp((float)twoTimesMaxMapOctileDistance - 1f / 48f * (distance + distance2), 0f, maxValue);
				ushort num6 = (ushort)Common.Clamp(b + num5, 0, maxValue);
				Values[zero.X][zero.Y] = num6;
			}
			cropCounter = num4;
			if (num4 == observableList2.Count)
			{
				phase = Phase.DrawOtherAgents;
				cropCounter = 0;
				The.Sim.PlaySite.Resources.TryGetValue(harvestJob.ResourceType, out var value2);
				value2.ListMemberRemoved.Remove(listOfCrops_ListItemRemovedMethodID);
			}
			return false;
		}
		case Phase.DrawOtherAgents:
		{
			if (entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.ResourceHarvesters.TryGetValue(harvestJob.ResourceType.ResourceItemType, out var value3))
			{
				foreach (Tuple<EntityID, ResourceID> item in value3)
				{
					if (item.Item1 != entity.EntityID)
					{
						ResourceContainer resourceContainer3 = LookUp<ResourceContainer, ResourceID>.FindByID(item.Item2);
						if (resourceContainer3 != null)
						{
							InfluenceMap.DrawLinearInfluenceCircle(Values, resourceContainer3.MapPosition, -10, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.Yes, InfluenceMap.CircleParameter.FalloffEachTile, 4);
						}
					}
				}
			}
			phase = Phase.Search;
			return false;
		}
		case Phase.Search:
		{
			ObservableList<ResourceContainer> observableList = The.Sim.PlaySite.Resources[harvestJob.ResourceType];
			ushort num = 0;
			bestResourceItemID = null;
			for (int i = 0; i < observableList.Count; i++)
			{
				ResourceContainer resourceContainer = observableList[i];
				if (!(resourceContainer.TotalHarvestableBulk > 0f))
				{
					continue;
				}
				Point mapPosition = resourceContainer.MapPosition;
				ushort num2 = Values[mapPosition.X][mapPosition.Y];
				if (num2 > num)
				{
					num = num2;
					IResourceItem resourceItem = resourceContainer.FindHarvestableItem();
					if (resourceItem != null)
					{
						bestResourceItemID = resourceItem.ID;
					}
					else
					{
						bestResourceItemID = null;
					}
				}
			}
			EndSearch();
			return true;
		}
		default:
			return false;
		}
	}

	private void EndSearch()
	{
		isDirty = false;
		entity.SendMessage(new Message(Message.MessageTypes.BestCropFound));
		phase = Phase.GetResourceMap;
	}

	public List<ResourceContainer> GetCropsAtTilePos(List<ResourceContainer> listOfCrops, Point pos)
	{
		return listOfCrops.FindAll((ResourceContainer c) => c.MapPosition == pos);
	}

	private void crops_FinishedEvent()
	{
		isWaiting = false;
		entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.GetCropsMap(harvestJob.ResourceType).FinishedEvent.Remove(crops_FinishedMethodID);
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

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		isWaiting = sn.DoBool(isWaiting);
		phase = sn.DoEnum(phase);
		bestResourceItemID = sn.DoEnumNullable(bestResourceItemID);
		cropCounter = sn.DoInt32(cropCounter);
		crops_FinishedMethodID = sn.DoEnum(crops_FinishedMethodID);
		listOfCrops_ListItemRemovedMethodID = sn.DoEnum(listOfCrops_ListItemRemovedMethodID);
		notifyWhenRegionSearchIsFinishedMethodID = sn.DoEnum(notifyWhenRegionSearchIsFinishedMethodID);
		cropsMap = sn.DoJaggedArray(cropsMap);
		isDirty = sn.DoBool(isDirty);
		maxValue = sn.DoInt32(maxValue);
		rowCounter = sn.DoInt32(rowCounter);
		snapshotEntity = sn.SnapshotID<Entity, EntityID>(entity).Value;
		snapshotHarvestJob = null;
		if (harvestJob != null)
		{
			snapshotHarvestJob = harvestJob.ProcessJob.ID;
		}
		snapshotHarvestJob = sn.DoEnumNullable(snapshotHarvestJob);
		twoTimesMaxMapOctileDistance = sn.DoInt32(twoTimesMaxMapOctileDistance);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(harvestJob);
		sn.Ignore(Values);
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
		LoadPostProcessRegisterMethodIDs();
		if (snapshotHarvestJob.HasValue)
		{
			harvestJob = ((ProcessJob)LookUp<Job, JobID>.FindByID(snapshotHarvestJob.Value)).HarvestJob;
		}
		snapshotHarvestJob = null;
		entity = Entity.FindByID(snapshotEntity);
		Values = entity.Intelligence.EvaluatorInfluenceMap;
		CreateRegulators();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup.Add(crops_FinishedMethodID, crops_FinishedEvent);
		ActionLookup.Add(notifyWhenRegionSearchIsFinishedMethodID, NotifyWhenRegionSearchIsFinished);
		ActionLookup<int>.Add(listOfCrops_ListItemRemovedMethodID, listOfCrops_ListItemRemoved);
	}
}
