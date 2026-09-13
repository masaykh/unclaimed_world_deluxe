using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.AI;

public class SharedKnowledge : ISnapshot
{
	private AllegianceID snapshotAllegiance;

	public Dictionary<EntityID, MemoryFact> MemoryFacts = new Dictionary<EntityID, MemoryFact>();

	private Dictionary<EntityID, MemoryFactID> snapshotMemoryFacts = new Dictionary<EntityID, MemoryFactID>();

	private Dictionary<EntityID, List<EntityID>> memoryFactLeafs = new Dictionary<EntityID, List<EntityID>>();

	public Dictionary<EntityID, EntityLock> EntityLocks;

	public EntityGroup AllKnownEntities;

	private EntityGroupID snapshotAllKnownEntities;

	public HashSet<DetectableID> AllDetectedEntities = new HashSet<DetectableID>();

	public PlaySiteKnowledge PlaySiteKnowledge;

	private Regulator memoryFactsCleanupRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Allegiance Allegiance { get; private set; }

	public Dictionary<EntityID, List<ProcessType>> SpecialActionLocks { get; set; }

	public bool IsSnapshotted { get; set; }

	public SharedKnowledge()
	{
	}

	public SharedKnowledge(Allegiance allegiance)
	{
		Allegiance = allegiance;
		snapshotAllegiance = Allegiance.ID;
		AllKnownEntities = new EntityGroup(allegiance, manageTrade: false, manageProduction: false);
		EntityLocks = new Dictionary<EntityID, EntityLock>();
		SpecialActionLocks = new Dictionary<EntityID, List<ProcessType>>();
		CreateRegulators();
		if (allegiance.Site.IsPlaySite)
		{
			PlaySiteKnowledge = new PlaySiteKnowledge(this);
		}
	}

	private EntityLock GetOrCreateLock(EntityID item)
	{
		if (!EntityLocks.TryGetValue(item, out var value))
		{
			value = new EntityLock();
			EntityLocks[item] = value;
		}
		return value;
	}

	public bool HasSpecialActionsForDisplay(IKnownEntityData entityData)
	{
		List<ProcessType> sharedSpecialActionsForDisplay = Entity.GetSharedSpecialActionsForDisplay(entityData);
		if (sharedSpecialActionsForDisplay != null && sharedSpecialActionsForDisplay.Count > 0)
		{
			return true;
		}
		sharedSpecialActionsForDisplay = GetOrCreateAvailableSpecialActionLocks(entityData);
		if (sharedSpecialActionsForDisplay != null && sharedSpecialActionsForDisplay.Count > 0)
		{
			return true;
		}
		return false;
	}

	public bool SpecialActionIsAvailable(IKnownEntityData entityData, ProcessType processType)
	{
		bool flag = false;
		if (entityData.EntityType.SharedSpecialActionTypes == null || !entityData.EntityType.SharedSpecialActionTypes.Contains(processType) || (entityData.AvailableSharedSpecialActions != null && entityData.AvailableSharedSpecialActions.Contains(processType)))
		{
			return true;
		}
		if (0 == 0)
		{
			List<ProcessType> orCreateAvailableSpecialActionLocks = GetOrCreateAvailableSpecialActionLocks(entityData);
			if (orCreateAvailableSpecialActionLocks != null && orCreateAvailableSpecialActionLocks.Contains(processType))
			{
				return true;
			}
		}
		return false;
	}

	public List<ProcessType> GetSpecialActionsForDisplay(IKnownEntityData entityData)
	{
		List<ProcessType> list = Entity.GetSharedSpecialActionsForDisplay(entityData);
		List<ProcessType> orCreateAvailableSpecialActionLocks = GetOrCreateAvailableSpecialActionLocks(entityData);
		Common.AddRangeToList(ref list, orCreateAvailableSpecialActionLocks);
		return list;
	}

	public void EnableSpecialActionLock(IKnownEntityData entityData, ProcessType sharedProcessType)
	{
		if (entityData.EntityType.SpecialActionLockTypes != null)
		{
			ProcessType processType = entityData.EntityType.SpecialActionLockTypes.FirstOrDefault((ProcessType p) => p.OriginalProcess == sharedProcessType);
			List<ProcessType> orCreateAvailableSpecialActionLocks = GetOrCreateAvailableSpecialActionLocks(entityData);
			if (processType != null && !orCreateAvailableSpecialActionLocks.Any((ProcessType p) => p.OriginalProcess == sharedProcessType))
			{
				orCreateAvailableSpecialActionLocks.Add(processType);
			}
		}
	}

	public void DisableSpecialActionLock(IKnownEntityData entityData, ProcessType sharedProcessType)
	{
		if (entityData.EntityType.SpecialActionLockTypes != null)
		{
			ProcessType processType = entityData.EntityType.SpecialActionLockTypes.FirstOrDefault((ProcessType p) => p.OriginalProcess == sharedProcessType);
			if (processType != null)
			{
				GetOrCreateAvailableSpecialActionLocks(entityData).Remove(processType);
			}
		}
	}

	private List<ProcessType> GetOrCreateAvailableSpecialActionLocks(IKnownEntityData entityData)
	{
		List<ProcessType> value = null;
		if ((SpecialActionLocks == null || !SpecialActionLocks.TryGetValue(entityData.EntityID, out value)) && entityData.EntityType.SpecialActionLockTypes != null)
		{
			value = new List<ProcessType>();
			SpecialActionLocks[entityData.EntityID] = value;
			foreach (ProcessType specialActionLockType in entityData.EntityType.SpecialActionLockTypes)
			{
				if (specialActionLockType.SpecialActionEnabledAtStart == true)
				{
					value.Add(specialActionLockType);
				}
			}
		}
		return value;
	}

	public void ClearInUseBy(EntityID itemID, EntityID userID)
	{
		if (EntityLocks.TryGetValue(itemID, out var value))
		{
			if (value.InUseBy == userID)
			{
				value.InUseBy = null;
			}
			DestroyLockIfEmpty(value, itemID);
		}
	}

	private void DestroyLockIfEmpty(EntityLock entityLock, EntityID itemID)
	{
		if (entityLock.IsEmpty())
		{
			EntityLocks.Remove(itemID);
		}
	}

	public void SetInUseBy(EntityID itemID, EntityID userID)
	{
		GetOrCreateLock(itemID).InUseBy = userID;
	}

	public EntityID? GetInUseBy(EntityID itemID)
	{
		if (EntityLocks.TryGetValue(itemID, out var value))
		{
			return value.InUseBy;
		}
		return null;
	}

	public void SetFoodDirty()
	{
		AllKnownEntities.SetFoodDirty();
	}

	public void Destroy()
	{
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.Destroy();
		}
		foreach (KeyValuePair<EntityID, MemoryFact> memoryFact in MemoryFacts)
		{
			memoryFact.Value.Destroy();
		}
		if (AllKnownEntities != null)
		{
			AllKnownEntities.Destroy();
		}
	}

	private void CreateRegulators()
	{
		memoryFactsCleanupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "SharedKnowledge");
	}

	public void Update(GameTime gameTime)
	{
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.Update(gameTime);
		}
		if (memoryFactsCleanupRegulator.IsReady())
		{
			List<MemoryFact> list = null;
			double totalUnPausedGameTimeInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds;
			double num = totalUnPausedGameTimeInSeconds - The.Sim.DateAndTime.SecondsPerDay * (double)Allegiance.RepresentativeEntityType.IntelligenceType.MemoryInDays;
			foreach (KeyValuePair<EntityID, MemoryFact> memoryFact in MemoryFacts)
			{
				if (!memoryFact.Value.IsDeprecated && !memoryFact.Value.OwnedBy.HasValue)
				{
					if (memoryFact.Value.TimeStampInSecondsOfGameTime < num)
					{
						memoryFact.Value.IsDeprecated = true;
						memoryFact.Value.ToBeDeletedOnTimeStampInSecondsOfGameTime = totalUnPausedGameTimeInSeconds + GameData.Instance.AIConstants.SecondsToKeepDeprecatedMemoryFacts;
					}
				}
				else if (memoryFact.Value.ToBeDeletedOnTimeStampInSecondsOfGameTime < totalUnPausedGameTimeInSeconds)
				{
					if (list == null)
					{
						list = new List<MemoryFact>();
					}
					list.Add(memoryFact.Value);
				}
			}
			if (list != null)
			{
				foreach (MemoryFact item in list)
				{
					DeleteMemoryOfEntity(item.EntityID, null, removeAllKnowledge: true);
				}
			}
		}
		List<DetectableID> list2 = null;
		foreach (DetectableID allDetectedEntity in AllDetectedEntities)
		{
			IDetectable detectable = LookUpIDetectables.FindByID(allDetectedEntity);
			if (detectable != null)
			{
				if (!(detectable is Entity entity))
				{
					continue;
				}
				if (entity.EntityID != EntityID.Invalid)
				{
					if (entity.Site != null && entity.Site.IsPlaySite && PlaySiteKnowledge != null)
					{
						PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(entity.EntityID, entity.PlaySiteLocation.ToVector2());
					}
				}
				else
				{
					Common.AddToList(ref list2, allDetectedEntity);
				}
			}
			else
			{
				Common.AddToList(ref list2, allDetectedEntity);
			}
		}
		if (list2 == null)
		{
			return;
		}
		foreach (DetectableID item2 in list2)
		{
			_ = 12947;
			AllDetectedEntities.Remove(item2);
		}
	}

	public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
	{
		if (TryGetMemoryFacts(entityID, out data, out var returnValue))
		{
			return returnValue;
		}
		data = Entity.FindByID(entityID);
		if (data == null)
		{
			return EntityResult.Destroyed;
		}
		if (data.AllegianceID == Allegiance.ID || AllDetectedEntities.Contains(((Entity)data).DetectableID) || !UsesMemory((Entity)data))
		{
			return EntityResult.SeenDirectly;
		}
		return EntityResult.NewUnknownEntity;
	}

	public Entity GetKnownDataAsEntity(EntityID entityID)
	{
		if (GetKnownData(entityID, out var data) == EntityResult.SeenDirectly)
		{
			return (Entity)data;
		}
		return null;
	}

	public bool TryGetMemoryFacts(EntityID entityID, out IKnownEntityData data, out EntityResult returnValue)
	{
		if (MemoryFacts.TryGetValue(entityID, out var value))
		{
			if (value.IsDeprecated)
			{
				data = null;
				returnValue = EntityResult.EntityStatusIsNowUnknown;
			}
			else
			{
				data = value;
				returnValue = EntityResult.Remembered;
			}
			return true;
		}
		returnValue = EntityResult.EntityStatusIsNowUnknown;
		data = null;
		return false;
	}

	public Vector3 GetLocation(Entity gameObject)
	{
		if (MemoryFacts.TryGetValue(gameObject.EntityID, out var value))
		{
			return value.PlaySiteLocation;
		}
		return gameObject.PlaySiteLocation;
	}

	public bool UsesMemory(IDetectable detectable)
	{
		return detectable.UsesMemory(this);
	}

	public void SeeDetectableIfRelevant(IDetectable detectable, bool testForUsesMemory = true, bool suppressClientFeedback = false, DetectionFactor resourceDetectionFactor = null, Entity detectingEntity = null, bool doAssert = true)
	{
		Entity entity = detectable as Entity;
		if (!testForUsesMemory || entity == null || UsesMemory(entity))
		{
			SeeDetectable(detectable, suppressClientFeedback, resourceDetectionFactor, detectingEntity, doAssert);
		}
	}

	public void SeeDetectable(IDetectable detectable, bool suppressClientFeedback = false, DetectionFactor resourceDetectionFactor = null, Entity detectingEntity = null, bool doAssert = true)
	{
		if (detectable == detectingEntity || AllDetectedEntities.Contains(detectable.ID))
		{
			return;
		}
		AllDetectedEntities.Add(detectable.ID);
		if (detectingEntity != null)
		{
			HandleInterestOfDetectable(detectingEntity, detectable, resourceDetectionFactor);
		}
		Entity entity = detectable as Entity;
		if (detectingEntity != null && entity != null && entity.CanBeHunted(Allegiance))
		{
			detectingEntity.SendMessage(new Message(entity, Message.MessageTypes.PreyIsNear, null));
		}
		if (!suppressClientFeedback)
		{
			if (Allegiance.AllegianceType == AllegianceType.Player)
			{
				The.Client.GiveDetectionFeedback(detectable, resourceDetectionFactor, detectingEntity, Allegiance);
			}
			if (detectingEntity != null)
			{
				FirePlayerDetectionEvents(detectingEntity, detectable);
			}
		}
		if (entity != null)
		{
			EntityID entityID = entity.EntityID;
			AddToCollectionsOfKnownEntities(entity);
			DeleteMemoryOfEntity(entity.ID, null, removeAllKnowledge: false);
			if (entity.EntityType.ContainerType != null && CanSeeInsideContainer(entity))
			{
				entity.Contains.IterateContained(delegate(Entity containedEntity)
				{
					SeeDetectableIfRelevant(containedEntity, testForUsesMemory: true, suppressClientFeedback, null, detectingEntity, doAssert);
				});
			}
			if (entity.Parts != null)
			{
				foreach (Entity part in entity.Parts)
				{
					SeeDetectableIfRelevant(part, testForUsesMemory: true, suppressClientFeedback, resourceDetectionFactor, detectingEntity, doAssert);
				}
			}
			TerrainTile.DeprecateDistance value = TerrainTile.DeprecateDistance.Near;
			DeprecateMemoryFactLeafs(detectingEntity, entityID, value);
			if (PlaySiteKnowledge != null && entity.Processes != null)
			{
				foreach (SimProcessID process in entity.Processes)
				{
					SimProcess simProcess = SimProcess.FindById(process);
					if (simProcess != null)
					{
						PlaySiteKnowledge.SeeProcess(simProcess);
					}
				}
			}
		}
		if (PlaySiteKnowledge == null)
		{
			return;
		}
		if (detectable is ResourceContainer resourceContainer)
		{
			Common.AddToMultiList(PlaySiteKnowledge.AllKnownResourceContainers, resourceContainer.ResourceType, resourceContainer.ID);
		}
		if (entity != null && entity.EntityType.BiologicalType != null)
		{
			HashSet<EntityType> preyTypes = Allegiance.RepresentativeEntityType.IntelligenceType.PreyTypes;
			if (preyTypes != null && preyTypes.Contains(entity.EntityType))
			{
				PlaySiteKnowledge.SpottedPrey.Add(entity.EntityType);
			}
			PlaySiteKnowledge.SpottedAnimals.Add(entity.EntityType);
		}
		if (entity != null && entity.SpawnedByOwner.HasValue)
		{
			IOwner owner = LookUpOwners.FindByID(entity.SpawnedByOwner);
			if (owner != null && owner.Allegiance == Allegiance)
			{
				owner.Allegiance.LogProductionStatistics(entity, null);
				entity.SpawnedByOwner = null;
			}
		}
		if (doAssert)
		{
			PlaySiteKnowledge.AssertSeenEntitiesOnPlaySiteNotInFOW();
		}
	}

	public void DeprecateMemoryFactLeafs(Entity detectingEntity, EntityID parentOfLeafsEntityID, TerrainTile.DeprecateDistance? deprecateDistance)
	{
		if (!memoryFactLeafs.TryGetValue(parentOfLeafsEntityID, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (MemoryFacts.TryGetValue(value[num], out var value2))
			{
				if (value2.DeprecateIfNeeded(detectingEntity, deprecateDistance))
				{
					value.RemoveAt(num);
				}
			}
			else
			{
				value.RemoveAt(num);
			}
		}
		if (value.Count == 0)
		{
			memoryFactLeafs.Remove(parentOfLeafsEntityID);
		}
	}

	private void FirePlayerDetectionEvents(Entity detectingEntity, IDetectable detectable)
	{
		if (Allegiance.AllegianceType == AllegianceType.Player)
		{
			if (detectable is Entity detectedEntity)
			{
				Sensor.HandleEntityTypeDetectionEvents(detectingEntity, detectedEntity);
			}
			else if (detectable is ResourceContainer resourceContainer)
			{
				Sensor.HandleResourceDetectionEvents(detectingEntity, resourceContainer.ResourceType);
			}
		}
	}

	private static void HandleInterestOfDetectable(Entity detectingEntity, IDetectable detectable, DetectionFactor resourceDetectionFactor)
	{
		float num = 0f;
		Vector3? location = null;
		EntityID? entity = null;
		if (detectable is Entity entity2)
		{
			num = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(GameData.Instance.Constants.InterestLevelForSpottedEntityMean, GameData.Instance.Constants.InterestLevelForSpottedEntityStdDeviation);
			entity = entity2.EntityID;
		}
		else
		{
			float d;
			if (resourceDetectionFactor != null)
			{
				d = resourceDetectionFactor.InterestLevelForSpottedResourceMean ?? GameData.Instance.Constants.InterestLevelForSpottedResourceMean;
				float? interestLevelForSpottedResourceStdDeviation = resourceDetectionFactor.InterestLevelForSpottedResourceStdDeviation;
				if (!interestLevelForSpottedResourceStdDeviation.HasValue)
				{
					_ = GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation;
				}
				else
				{
					interestLevelForSpottedResourceStdDeviation.GetValueOrDefault();
				}
			}
			else
			{
				d = GameData.Instance.Constants.InterestLevelForSpottedResourceMean;
				_ = GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation;
			}
			if (!Common.IsZero(d))
			{
				num = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(GameData.Instance.Constants.InterestLevelForSpottedResourceMean, GameData.Instance.Constants.InterestLevelForSpottedResourceStdDeviation);
			}
			location = detectable.Location;
		}
		if (num > 0f)
		{
			Message msg = Trigger.CreateInterestMessage(null, null, entity, location, num);
			detectingEntity.SendMessage(msg);
		}
	}

	private bool IsMemoryRoot(IKnownEntityData entityData)
	{
		if (!entityData.PartOfID.HasValue)
		{
			return !entityData.ContainedBy.HasValue;
		}
		return false;
	}

	private EntityID? GetMemoryParent(IKnownEntityData entityData)
	{
		if (entityData.ContainedBy.HasValue)
		{
			return entityData.ContainedBy;
		}
		if (entityData.PartOfID.HasValue && LookUpIComposites.FindByID(entityData.PartOfID.Value) is Entity entity)
		{
			return entity.EntityID;
		}
		return null;
	}

	public void UnSeeEntity(Entity entity)
	{
		if (AllDetectedEntities.Contains(entity.DetectableID) && UsesMemory(entity))
		{
			StoreMemoryOfEntity(entity, testIfEntityBelongs: false);
		}
	}

	public void StoreMemoryOfEntity(Entity entity, bool testIfEntityBelongs = true)
	{
		if (testIfEntityBelongs && !UsesMemory(entity))
		{
			return;
		}
		EntityID entityID = entity.EntityID;
		DetectableID detectableID = entity.DetectableID;
		if (MemoryFacts.TryGetValue(entityID, out var value))
		{
			if (value.MapPosition != entity.MapPosition)
			{
				if (value.MapPosition.HasValue)
				{
					The.Map.GetTile(value.MapPosition.Value).RemoveRememberedRootEntity(this, value);
				}
				if (IsMemoryRoot(value) && entity.MapPosition.HasValue)
				{
					The.Map.GetTile(entity.MapPosition.Value).AddRememberedRootEntity(this, value);
				}
				if (PlaySiteKnowledge != null)
				{
					PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, value.PlaySiteLocation.ToVector2());
				}
			}
			if (!value.Init(entity, Allegiance))
			{
				DeleteMemoryOfEntity(entityID, detectableID, removeAllKnowledge: true);
				return;
			}
		}
		else
		{
			value = MemoryFact.GetNew(entity, Allegiance);
			if (value == null)
			{
				DeleteMemoryOfEntity(entityID, detectableID, removeAllKnowledge: true);
				return;
			}
			if (entity.IsOnPlaySite())
			{
				if (IsMemoryRoot(value))
				{
					The.Map.GetTile(entity.MapPosition.Value).AddRememberedRootEntity(this, value);
				}
				else
				{
					Common.AddToMultiList(memoryFactLeafs, GetMemoryParent(value).Value, value.EntityID);
				}
				if (PlaySiteKnowledge != null)
				{
					PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, value.PlaySiteLocation.ToVector2());
				}
			}
			MemoryFacts.Add(entityID, value);
		}
		if (PlaySiteKnowledge != null && entity.Processes != null)
		{
			for (int num = entity.Processes.Count - 1; num >= 0; num--)
			{
				SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(entity.Processes[num]);
				if (simProcess != null)
				{
					PlaySiteKnowledge.StoreMemoryOfProcess(simProcess);
				}
				else
				{
					entity.Processes.RemoveAt(num);
				}
			}
		}
		if (entity.EntityType.ContainerType != null && CanSeeInsideContainer(entity))
		{
			entity.Contains.IterateContained(UnSeeEntity);
		}
		if (entity.Parts != null)
		{
			foreach (Entity part in entity.Parts)
			{
				UnSeeEntity(part);
			}
		}
		AllDetectedEntities.Remove(entity.DetectableID);
	}

	private bool CanSeeEntity(EntityID entityID, out bool processIsInvalid)
	{
		processIsInvalid = false;
		Entity entity = Entity.FindByID(entityID);
		if (entity != null && entity.ContainedBy.HasValue)
		{
			Entity entity2 = Entity.FindByID(entity.ContainedBy.Value);
			if (entity2 == null)
			{
				processIsInvalid = true;
				return false;
			}
			if (!CanSeeInsideContainer(entity2))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanSeeInsideContainer(Entity containerEntity)
	{
		if (containerEntity.AllegianceID == Allegiance.ID)
		{
			return true;
		}
		return containerEntity.EntityType.ContainerType.CanTransactWithContainer(Allegiance.RepresentativeEntityType);
	}

	public void AddKnowledgeOfItemToNewOwner(Entity gameEntity)
	{
		if (gameEntity.ContainedBy.HasValue)
		{
			IKnownEntityData data;
			EntityResult knownData = GetKnownData(gameEntity.ContainedBy.Value, out data);
			if (gameEntity.IsOnPlaySite() && knownData == EntityResult.Remembered)
			{
				StoreMemoryOfEntity(gameEntity);
			}
			else if (knownData == EntityResult.SeenDirectly)
			{
				SeeDetectableIfRelevant(gameEntity, testForUsesMemory: true, suppressClientFeedback: false, null, null, doAssert: false);
			}
		}
		else if (gameEntity.IsOnPlaySite() && The.Map.EntityIsInFogOfWar(gameEntity, Allegiance))
		{
			StoreMemoryOfEntity(gameEntity);
		}
		else
		{
			SeeDetectableIfRelevant(gameEntity);
		}
		if (gameEntity.Parts == null)
		{
			return;
		}
		foreach (Entity part in gameEntity.Parts)
		{
			AddKnowledgeOfItemToNewOwner(part);
		}
	}

	public void DeletePlaySiteKnowledgeOfEntity(EntityID entityID)
	{
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.RemoveFromCollectionsOfKnownEntities(entityID);
		}
	}

	public bool DeleteMemoryOfEntity(EntityID entityID, DetectableID? detectableID, bool removeAllKnowledge)
	{
		Entity entity = Entity.FindByID(entityID);
		if (entity == null || removeAllKnowledge)
		{
			EntityType entityType = null;
			if (entity != null)
			{
				entityType = entity.EntityType;
			}
			RemoveFromCollectionsOfKnownEntities(entityType, entityID);
		}
		if (removeAllKnowledge && detectableID.HasValue)
		{
			AllDetectedEntities.Remove(detectableID.Value);
		}
		if (MemoryFacts.TryGetValue(entityID, out var value))
		{
			MemoryFacts.Remove(entityID);
			EntityID? memoryParent = GetMemoryParent(value);
			if (memoryParent.HasValue)
			{
				Common.RemoveFromMultiList(memoryFactLeafs, memoryParent.Value, value.EntityID, removeEmptyList: true);
			}
			entity?.SyncWithMemoryFact(Allegiance, value);
			if (PlaySiteKnowledge != null)
			{
				PlaySiteKnowledge.DeleteMemoryOfProcesses(value);
			}
			value.Destroy();
			return true;
		}
		return false;
	}

	public void RemoveInvalidEntityIDs(List<EntityID> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (EntityID item in list)
		{
			RemoveInvalidEntityID(item);
		}
	}

	public void RemoveInvalidEntityID(EntityID entityID)
	{
		RemoveFromCollectionsOfKnownEntities(null, entityID);
	}

	public void AddToCollectionsOfKnownEntities(Entity entity)
	{
		_ = entity.ID;
		if (!AllKnownEntities.Contains(entity))
		{
			AllKnownEntities.AddEntity(entity);
		}
		if (entity.Site != null && entity.Site.IsPlaySite && PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(entity, entity.PlaySiteLocation.ToVector2());
		}
	}

	private void RemoveFromCollectionsOfKnownEntities(EntityType entityType, EntityID entityID)
	{
		AllKnownEntities.DeleteEntity(entityID, entityType);
		SpecialActionLocks.Remove(entityID);
		EntityLocks.Remove(entityID);
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.RemoveFromCollectionsOfKnownEntities(entityID);
		}
	}

	public DiscomfortMap GetDiscomfortMap(ProtectionLevel p, EntityType c, ThreatStance a)
	{
		return PlaySiteKnowledge.GetDiscomfortMap(p, c, a);
	}

	public ThreatMap GetThreatMap(EntityType c, ThreatStance a)
	{
		return PlaySiteKnowledge.GetThreatMap(c, a);
	}

	public MovementMap GetMovementMap(ProtectionLevel p, EntityType c, ThreatStance a)
	{
		return PlaySiteKnowledge.GetMovementMap(p, c, a);
	}

	public RegionMap GetFootRegionMap(Entity entity, ThreatStance? threatStance = null)
	{
		return GetMovementMap(entity, threatStance).Layers[SurfaceType.TransportType.Foot].RegionMap;
	}

	public RegionMap GetVehicleRegionMap(Entity entity, ThreatStance? threatStance = null)
	{
		return GetMovementMap(entity, threatStance).Layers[SurfaceType.TransportType.OffRoad].RegionMap;
	}

	public RegionMap GetRegionMapToUseForEntity(Entity entity, ThreatStance? threatStance = null)
	{
		if (entity.DrivingVehicle.HasValue)
		{
			return GetVehicleRegionMap(entity, threatStance);
		}
		return GetFootRegionMap(entity, threatStance);
	}

	public MovementMap GetMovementMap(Entity entity, ThreatStance? threatStance = null)
	{
		return PlaySiteKnowledge.GetMovementMap(entity, threatStance);
	}

	public void AddMember(Entity newMember)
	{
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.AddMember(newMember);
		}
	}

	public void RemoveMember(Entity memberToRemove)
	{
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.RemoveMember(memberToRemove);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	private DetectableID GetID(KeyValuePair<IDetectable, bool> kvp)
	{
		return kvp.Key.ID;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotAllegiance = sn.DoEnum(snapshotAllegiance);
		PlaySiteKnowledge = (PlaySiteKnowledge)sn.DoISnapshot(PlaySiteKnowledge);
		AllDetectedEntities = sn.DoHashSet(AllDetectedEntities);
		snapshotAllKnownEntities = sn.SnapshotID<EntityGroup, EntityGroupID>(AllKnownEntities).Value;
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotMemoryFacts = MemoryFacts.ToDictionary((KeyValuePair<EntityID, MemoryFact> m) => m.Key, (KeyValuePair<EntityID, MemoryFact> m) => m.Value.ID);
		}
		snapshotMemoryFacts = sn.DoDictionary(snapshotMemoryFacts);
		memoryFactLeafs = sn.DoMultiMap(memoryFactLeafs);
		EntityLocks = sn.DoDictionary(EntityLocks);
		SpecialActionLocks = sn.DoMultiMap(SpecialActionLocks);
		sn.Ignore(Allegiance);
		sn.Ignore(AllDetectedEntities);
		sn.Ignore(MemoryFacts);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
		AllKnownEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotAllKnownEntities);
		if (PlaySiteKnowledge != null)
		{
			PlaySiteKnowledge.Parent = this;
			PlaySiteKnowledge.LoadPostProcess(sn);
		}
		if (snapshotMemoryFacts != null)
		{
			MemoryFacts = snapshotMemoryFacts.ToDictionary((KeyValuePair<EntityID, MemoryFactID> m) => m.Key, (KeyValuePair<EntityID, MemoryFactID> m) => LookUp<MemoryFact, MemoryFactID>.FindByID(m.Value));
		}
		if (EntityLocks != null)
		{
			foreach (KeyValuePair<EntityID, EntityLock> entityLock in EntityLocks)
			{
				entityLock.Value.LoadPostProcess(sn);
			}
		}
		CreateRegulators();
	}
}
