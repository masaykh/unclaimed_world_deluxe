using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.AI;

public class PlaySiteKnowledge : ISnapshot, IIDEventSubscriber
{
	public SharedKnowledge Parent;

	public List<ThreatSource> TransientThreats = new List<ThreatSource>();

	public Dictionary<EntityType, ResourceMap> ResourceMaps = new Dictionary<EntityType, ResourceMap>();

	private Dictionary<EntityType, CyclableID> snapshotResourceMaps;

	public Dictionary<EntityType, List<Tuple<EntityID, ResourceID>>> ResourceHarvesters = new Dictionary<EntityType, List<Tuple<EntityID, ResourceID>>>();

	public Dictionary<SimProcessID, ProcessMemory> ProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemory>();

	private Dictionary<SimProcessID, ProcessMemoryID> snapshotProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemoryID>();

	public Dictionary<ResourceType, HashSet<ResourceID>> AllKnownResourceContainers = new Dictionary<ResourceType, HashSet<ResourceID>>();

	public Dictionary<EntityID, EntityID> AllKnownOutsideAgentsOnPlaySite = new Dictionary<EntityID, EntityID>();

	public Dictionary<EntityID, EntityID> AllKnownThreatSources = new Dictionary<EntityID, EntityID>();

	private const int maxKnownEntityDatasPerNode = 10;

	private PointQuadTree<EntityID> knownEntityDataTree;

	private List<Pair<EntityID, Vector2>> snapshotKnownEntityDataTree;

	public HashSet<EntityType> SpottedPrey = new HashSet<EntityType>();

	public HashSet<EntityType> SpottedAnimals = new HashSet<EntityType>();

	private Regulator assertRegulator;

	private Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessDestroyedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

	private Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessCompletedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

	private Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessStartedEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

	private Dictionary<SimProcessID, IDActionEvent<IKnownProcess>> ProcessProducingEvents = new Dictionary<SimProcessID, IDActionEvent<IKnownProcess>>();

	private MethodID processCompletedMethodID;

	private MethodID processDestroyedMethodID;

	private MethodID processStartedMethodID;

	private MethodID processProducingMethodID;

	public Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>> ThreatMaps;

	private Dictionary<EntityType, Dictionary<ThreatStance, IMapID>> snapshotThreatMaps;

	public Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>> DiscomfortMaps;

	private Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>> snapshotAllDiscomfortMaps;

	public Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> AllMovementMaps;

	private Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>> snapshotAllMovementMaps;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public PointQuadTree<EntityID> KnownEntityDataTree => knownEntityDataTree;

	public bool IsSnapshotted { get; set; }

	public PlaySiteKnowledge()
	{
	}

	public PlaySiteKnowledge(SharedKnowledge parent)
	{
		Parent = parent;
		processDestroyedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessDestroyed);
		processCompletedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessCompleted);
		processStartedMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessStarted);
		processProducingMethodID = ActionLookup<SimProcess>.AddWithNewID(ProcessProducing);
		knownEntityDataTree = new PointQuadTree<EntityID>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), 10, 7);
		CreateRegulators();
	}

	public void AddOrUpdateKnownEntityLocation(Entity entity, Vector2 location)
	{
		_ = entity.EntityType.TreeType;
		EntityID iD = entity.ID;
		if (IsOutsiderAgentOnPlaySite(entity))
		{
			if (!AllKnownOutsideAgentsOnPlaySite.ContainsKey(iD))
			{
				AllKnownOutsideAgentsOnPlaySite.Add(iD, iD);
			}
		}
		else if (IsOutsideThreat(entity) && !AllKnownThreatSources.ContainsKey(iD))
		{
			AllKnownThreatSources.Add(iD, iD);
		}
		if (KnownEntityDataTree.Contains(iD))
		{
			KnownEntityDataTree.UpdateObject(iD, location);
			return;
		}
		entity.ToString().Contains("Mudbrick");
		KnownEntityDataTree.AddObject(iD, location);
	}

	private bool IsOutsiderAgentOnPlaySite(Entity entity)
	{
		if (entity.EntityType.IntelligenceType != null && entity.Location.HasValue)
		{
			AllegianceID? allegianceID = entity.AllegianceID;
			if (allegianceID.HasValue && allegianceID != Parent.Allegiance.ID)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateRegulators()
	{
		assertRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "PlaySiteKnowledge");
	}

	private bool IsOutsideThreat(Entity entity)
	{
		if (entity.EntityType.ThreatType != null && entity.EntityType.IntelligenceType == null)
		{
			ThreatGroup threatGroup = entity.ThreatGroup;
			if (threatGroup != null && threatGroup != Parent.Allegiance.ThreatGroup)
			{
				return true;
			}
		}
		return false;
	}

	private bool EntityTypeRequiresAuxiliaryMaps(EntityType entityType)
	{
		if (entityType.IntelligenceType != null)
		{
			return entityType.IntelligenceType.IsMobile;
		}
		return false;
	}

	public void InitAuxiliaryMaps()
	{
		if (AllMovementMaps != null)
		{
			return;
		}
		ThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>>();
		DiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>>();
		AllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>>();
		AddAuxiliaryMaps(Parent.Allegiance.RepresentativeEntityType);
		foreach (Entity members in Parent.Allegiance.MembersList)
		{
			if (EntityTypeRequiresAuxiliaryMaps(members.EntityType) && !ThreatMaps.ContainsKey(members.EntityType))
			{
				AddAuxiliaryMaps(members.EntityType);
			}
		}
	}

	private void RemoveAuxiliaryMaps(EntityType entityType)
	{
		if (!ThreatMaps.TryGetValue(entityType, out var value))
		{
			return;
		}
		foreach (KeyValuePair<ThreatStance, ThreatMap> item in value)
		{
			item.Value.Destroy();
		}
		ThreatMaps.Remove(entityType);
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>> discomfortMap in DiscomfortMaps)
		{
			foreach (KeyValuePair<ThreatStance, DiscomfortMap> item2 in discomfortMap.Value[entityType])
			{
				item2.Value.Destroy();
			}
			discomfortMap.Value.Remove(entityType);
		}
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap in AllMovementMaps)
		{
			foreach (KeyValuePair<ThreatStance, MovementMap> item3 in allMovementMap.Value[entityType])
			{
				item3.Value.Destroy();
			}
			allMovementMap.Value.Remove(entityType);
		}
	}

	private void AddAuxiliaryMaps(EntityType entityType)
	{
		MapManager map = The.Map;
		int mapTileWidth = map.mapTileWidth;
		int mapTileHeight = map.mapTileHeight;
		ThreatMap threatMap = new ThreatMap(Parent, mapTileWidth, mapTileHeight, entityType, ThreatStance.Normal)
		{
			IDName = entityType.KeyName + "Normal"
		};
		ThreatMap threatMap2 = new ThreatMap(Parent, mapTileWidth, mapTileHeight, entityType, ThreatStance.Bold)
		{
			IDName = entityType.KeyName + "Bold"
		};
		ThreatMap threatMap3 = new ThreatMap(Parent, mapTileWidth, mapTileHeight, entityType, ThreatStance.Cautious)
		{
			IDName = entityType.KeyName + "Cautious"
		};
		AddThreatMap(threatMap);
		AddThreatMap(threatMap2);
		AddThreatMap(threatMap3);
		DiscomfortMap discomfortMap = new DiscomfortMap(ProtectionLevel.Exposed, threatMap)
		{
			IDName = "Exposed" + threatMap.IDName
		};
		AddDiscomfortMap(discomfortMap, threatMap);
		DiscomfortMap discomfortMap2 = new DiscomfortMap(ProtectionLevel.Exposed, threatMap2)
		{
			IDName = "Exposed" + threatMap2.IDName
		};
		AddDiscomfortMap(discomfortMap2, threatMap2);
		DiscomfortMap discomfortMap3 = new DiscomfortMap(ProtectionLevel.Exposed, threatMap3)
		{
			IDName = "Exposed" + threatMap3.IDName
		};
		AddDiscomfortMap(discomfortMap3, threatMap3);
		if (Parent.Allegiance.AllegianceType == AllegianceType.Player)
		{
			MovementMap map2 = new MovementMap(0.2f, discomfortMap, threatMap, discomfortMap.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
			AddMovementMap(map2, discomfortMap, threatMap);
			MovementMap map3 = new MovementMap(0.2f, discomfortMap2, threatMap2, discomfortMap2.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
			AddMovementMap(map3, discomfortMap2, threatMap2);
			MovementMap map4 = new MovementMap(0.2f, discomfortMap3, threatMap3, discomfortMap3.IDName, GameData.Instance.AIConstants.PlayerMovementMapUpdateInterval, SurfaceType.TransportType.Foot, SurfaceType.TransportType.OffRoad);
			AddMovementMap(map4, discomfortMap3, threatMap3);
		}
		else
		{
			MovementMap map5 = new MovementMap(0.2f, discomfortMap, threatMap, discomfortMap.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, default(SurfaceType.TransportType));
			AddMovementMap(map5, discomfortMap, threatMap);
			MovementMap map6 = new MovementMap(0.2f, discomfortMap2, threatMap2, discomfortMap2.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, default(SurfaceType.TransportType));
			AddMovementMap(map6, discomfortMap2, threatMap2);
			MovementMap map7 = new MovementMap(0.2f, discomfortMap3, threatMap3, discomfortMap3.IDName, GameData.Instance.AIConstants.OtherMovementMapUpdateInterval, default(SurfaceType.TransportType));
			AddMovementMap(map7, discomfortMap3, threatMap3);
		}
	}

	public MovementMap GetMovementMap(Entity entity, ThreatStance? threatStance = null)
	{
		ProtectionLevel key = ProtectionLevel.Exposed;
		if (!threatStance.HasValue)
		{
			threatStance = entity.Intelligence.ThreatStance;
		}
		return AllMovementMaps[key][entity.EntityType][threatStance.Value].GetCurrent();
	}

	public ThreatMap GetThreatMap(EntityType c, ThreatStance a)
	{
		return (ThreatMap)ThreatMaps[c][a].GetCurrent();
	}

	public DiscomfortMap GetDiscomfortMap(ProtectionLevel p, EntityType c, ThreatStance a)
	{
		p = ProtectionLevel.Exposed;
		return (DiscomfortMap)DiscomfortMaps[p][c][a].GetCurrent();
	}

	public MovementMap GetMovementMap(ProtectionLevel p, EntityType c, ThreatStance a)
	{
		p = ProtectionLevel.Exposed;
		return AllMovementMaps[p][c][a].GetCurrent();
	}

	private void AddMovementMap(MovementMap map, DiscomfortMap childDMap, ThreatMap childThreatMap)
	{
		if (!AllMovementMaps.ContainsKey(childDMap.ProtectionLevel))
		{
			AllMovementMaps.Add(childDMap.ProtectionLevel, new Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>());
		}
		if (!AllMovementMaps[childDMap.ProtectionLevel].ContainsKey(childThreatMap.EntityType))
		{
			AllMovementMaps[childDMap.ProtectionLevel].Add(childThreatMap.EntityType, new Dictionary<ThreatStance, MovementMap>());
		}
		AllMovementMaps[childDMap.ProtectionLevel][childThreatMap.EntityType].Add(childThreatMap.Approach, map);
	}

	private void AddThreatMap(ThreatMap threatMap)
	{
		if (!ThreatMaps.ContainsKey(threatMap.EntityType))
		{
			ThreatMaps.Add(threatMap.EntityType, new Dictionary<ThreatStance, ThreatMap>());
		}
		ThreatMaps[threatMap.EntityType].Add(threatMap.Approach, threatMap);
	}

	private void AddDiscomfortMap(DiscomfortMap dMap, ThreatMap childThreatMap)
	{
		if (!DiscomfortMaps.ContainsKey(dMap.ProtectionLevel))
		{
			DiscomfortMaps.Add(dMap.ProtectionLevel, new Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>());
		}
		if (!DiscomfortMaps[dMap.ProtectionLevel].ContainsKey(childThreatMap.EntityType))
		{
			DiscomfortMaps[dMap.ProtectionLevel].Add(childThreatMap.EntityType, new Dictionary<ThreatStance, DiscomfortMap>());
		}
		DiscomfortMaps[dMap.ProtectionLevel][childThreatMap.EntityType].Add(childThreatMap.Approach, dMap);
	}

	public void AddMember(Entity newMember)
	{
		if (ThreatMaps != null && EntityTypeRequiresAuxiliaryMaps(newMember.EntityType) && !ThreatMaps.ContainsKey(newMember.EntityType))
		{
			AddAuxiliaryMaps(newMember.EntityType);
		}
	}

	public void RemoveMember(Entity memberToRemove)
	{
		if (memberToRemove.EntityType != Parent.Allegiance.RepresentativeEntityType && !Parent.Allegiance.MembersList.Exists((Entity m) => m.EntityType == memberToRemove.EntityType))
		{
			RemoveAuxiliaryMaps(memberToRemove.EntityType);
		}
	}

	public void AssertSeenEntitiesOnPlaySiteNotInFOW()
	{
	}

	private void ValidateSeenEntityNotInFOW(EntityID item)
	{
	}

	public void RemoveFromCollectionsOfKnownEntities(EntityID entityID)
	{
		KnownEntityDataTree.RemoveObject(entityID);
		if (!AllKnownOutsideAgentsOnPlaySite.Remove(entityID))
		{
			AllKnownThreatSources.Remove(entityID);
		}
	}

	public void RemoveCropHarvester(EntityType cropItem, Entity harvester)
	{
		if (ResourceHarvesters.TryGetValue(cropItem, out var value))
		{
			value.RemoveAll((Tuple<EntityID, ResourceID> c) => c.Item1 == harvester.EntityID);
		}
	}

	public void AddCropHarvester(EntityType cropItem, Entity harvester, ResourceContainer crop)
	{
		if (!ResourceHarvesters.TryGetValue(cropItem, out var value))
		{
			value = new List<Tuple<EntityID, ResourceID>>();
			ResourceHarvesters.Add(cropItem, value);
		}
		value.Add(new Tuple<EntityID, ResourceID>(harvester.EntityID, crop.ID));
	}

	public ResourceMap GetCropsMap(ResourceType resourceType)
	{
		if (!ResourceMaps.TryGetValue(resourceType.ResourceItemType, out var value))
		{
			value = new ResourceMap(resourceType);
			ResourceMaps.Add(resourceType.ResourceItemType, value);
		}
		return value;
	}

	public void Update(GameTime gameTime)
	{
		foreach (ThreatSource transientThreat in TransientThreats)
		{
			transientThreat.Update(gameTime);
		}
		for (int num = TransientThreats.Count - 1; num >= 0; num--)
		{
			if (TransientThreats[num].IsExpired())
			{
				TransientThreats.Remove(TransientThreats[num]);
			}
		}
		foreach (KeyValuePair<EntityType, ResourceMap> resourceMap in ResourceMaps)
		{
			resourceMap.Value.Update(gameTime);
		}
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap in AllMovementMaps)
		{
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item in allMovementMap.Value)
			{
				foreach (KeyValuePair<ThreatStance, MovementMap> item2 in item.Value)
				{
					item2.Value.Update(gameTime);
				}
			}
		}
	}

	public void Destroy()
	{
		if (AllMovementMaps != null)
		{
			foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap in AllMovementMaps)
			{
				foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item in allMovementMap.Value)
				{
					foreach (KeyValuePair<ThreatStance, MovementMap> item2 in item.Value)
					{
						item2.Value.Destroy();
					}
				}
			}
		}
		foreach (KeyValuePair<EntityType, ResourceMap> resourceMap in ResourceMaps)
		{
			resourceMap.Value.Destroy();
		}
		if (DiscomfortMaps != null)
		{
			foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>> discomfortMap in DiscomfortMaps)
			{
				foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, DiscomfortMap>> item3 in discomfortMap.Value)
				{
					foreach (KeyValuePair<ThreatStance, DiscomfortMap> item4 in item3.Value)
					{
						item4.Value.Destroy();
					}
				}
			}
		}
		if (ThreatMaps == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, ThreatMap>> threatMap in ThreatMaps)
		{
			foreach (KeyValuePair<ThreatStance, ThreatMap> item5 in threatMap.Value)
			{
				item5.Value.Destroy();
			}
		}
	}

	public void RegisterProcessDestroyedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
	{
		process.ProcessDestroyedEvent.Add(processDestroyedMethodID, this);
		RegisterProcessEvent(ProcessDestroyedEvents, action, process, subscriber, out methodID);
	}

	public void RegisterProcessCompletedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
	{
		process.ProcessCompletedEvent.Add(processCompletedMethodID, this);
		RegisterProcessEvent(ProcessCompletedEvents, action, process, subscriber, out methodID);
	}

	public void RegisterProcessProducingEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
	{
		process.ProcessProducingEvent.Add(processProducingMethodID, this);
		RegisterProcessEvent(ProcessProducingEvents, action, process, subscriber, out methodID);
	}

	public void RegisterProcessStartedEvent(Action<IKnownProcess> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
	{
		process.ProcessStartedEvent.Add(processStartedMethodID, this);
		RegisterProcessEvent(ProcessStartedEvents, action, process, subscriber, out methodID);
	}

	private static void RegisterProcessEvent<T>(Dictionary<SimProcessID, IDActionEvent<T>> events, Action<T> action, SimProcess process, IIDEventSubscriber subscriber, out MethodID methodID)
	{
		if (!events.TryGetValue(process.ID, out var value))
		{
			value = new IDActionEvent<T>();
			events.Add(process.ID, value);
		}
		value.AddAndRegister(action, subscriber, out methodID);
	}

	public void DeleteMemoryOfProcesses(MemoryFact memoryFact)
	{
		if (memoryFact.Processes == null)
		{
			return;
		}
		foreach (SimProcessID process in memoryFact.Processes)
		{
			DeleteMemoryOfProcess(process);
		}
		memoryFact.Processes = null;
	}

	public bool DeleteMemoryOfProcess(SimProcessID id)
	{
		if (ProcessMemoryFacts.TryGetValue(id, out var value))
		{
			ProcessMemoryFacts.Remove(id);
			SimProcess simProcess = SimProcess.FindById(id);
			if (simProcess != null)
			{
				simProcess.ImmovableInput = value.ImmovableInput;
				simProcess.ImmovableTool = value.ImmovableTool;
				simProcess.GroundLocation = value.GroundLocation;
			}
			else
			{
				if (value.RealProcessIsCompleted)
				{
					InvokeProcessEvent(ProcessCompletedEvents, id, value);
				}
				InvokeProcessEvent(ProcessDestroyedEvents, id, value);
				DeregisterProcessEvents(id);
			}
			if (value.MapPosition.HasValue)
			{
				The.Map.TileMap[value.MapPosition.Value.X][value.MapPosition.Value.Y].RemoveRememberedProcess(Parent, value);
			}
			value.Destroy();
			return true;
		}
		return false;
	}

	public void SeeProcess(SimProcess process)
	{
		if (UsesMemory(process))
		{
			DeleteMemoryOfProcess(process.ID);
		}
	}

	private bool CanSeeProcessItems(SimProcess process)
	{
		if (process.ContainerToPlaceOutputsIn.HasValue && GoalEvaluator.EntityDataResultCausesSkip(Parent.GetKnownData(process.ContainerToPlaceOutputsIn.Value, out var _)))
		{
			return false;
		}
		if (process.OutputEntities != null)
		{
			foreach (EntityID outputEntity in process.OutputEntities)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(Parent.GetKnownData(outputEntity, out var _)))
				{
					return false;
				}
			}
		}
		if (process.StationaryTools != null)
		{
			foreach (EntityID stationaryTool in process.StationaryTools)
			{
				if (GoalEvaluator.EntityDataResultCausesSkip(Parent.GetKnownData(stationaryTool, out var _)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void StoreMemoryOfProcess(SimProcess process)
	{
		if (!UsesMemory(process))
		{
			return;
		}
		if (!CanSeeProcessItems(process))
		{
			DeleteMemoryOfProcess(process.ID);
			return;
		}
		if (ProcessMemoryFacts.TryGetValue(process.ID, out var value))
		{
			if (!value.Init(process, Parent))
			{
				DeleteMemoryOfProcess(process.ID);
			}
			return;
		}
		value = new ProcessMemory(process);
		if (value.Init(process, Parent))
		{
			if (StoreProcessOnTerrainTile(value) && value.MapPosition.HasValue)
			{
				The.Map.TileMap[value.MapPosition.Value.X][value.MapPosition.Value.Y].AddRememberedProcess(Parent, value);
			}
			ProcessMemoryFacts.Add(process.ID, value);
		}
	}

	private bool StoreProcessOnTerrainTile(IKnownProcess processData)
	{
		if (processData.GroundLocation.HasValue)
		{
			return true;
		}
		return false;
	}

	public ProcessResult GetKnownProcessData(SimProcessID processID, out IKnownProcess data)
	{
		if (TryGetMemoryProcess(processID, out data, out var returnValue))
		{
			return returnValue.Value;
		}
		data = LookUp<SimProcess, SimProcessID>.FindByID(processID);
		if (data == null)
		{
			return ProcessResult.Destroyed;
		}
		return ProcessResult.SeenDirectly;
	}

	private bool TryGetMemoryProcess(SimProcessID processID, out IKnownProcess data, out ProcessResult? returnValue)
	{
		if (ProcessMemoryFacts.TryGetValue(processID, out var value))
		{
			data = value;
			returnValue = ProcessResult.Remembered;
			return true;
		}
		returnValue = null;
		data = null;
		return false;
	}

	public void UnSeeProcess(SimProcess process)
	{
		if (UsesMemory(process))
		{
			StoreMemoryOfProcess(process);
		}
	}

	private bool UsesMemory(SimProcess process)
	{
		return true;
	}

	public void ProcessDestroyed(SimProcess process)
	{
		if (GetKnownProcessData(process.ID, out var data) == ProcessResult.SeenDirectly)
		{
			InvokeProcessEvent(ProcessDestroyedEvents, process.ID, data);
			DeregisterProcessEvents(process.ID);
		}
	}

	private void DeregisterProcessEvents(SimProcessID processID)
	{
		ProcessStartedEvents.Remove(processID);
		ProcessProducingEvents.Remove(processID);
		ProcessCompletedEvents.Remove(processID);
		ProcessDestroyedEvents.Remove(processID);
	}

	public void ProcessCompleted(SimProcess process)
	{
		IKnownProcess data;
		switch (GetKnownProcessData(process.ID, out data))
		{
		case ProcessResult.SeenDirectly:
			InvokeProcessEvent(ProcessCompletedEvents, process.ID, data);
			ProcessProducingEvents.Remove(process.ID);
			ProcessCompletedEvents.Remove(process.ID);
			break;
		case ProcessResult.Remembered:
			((ProcessMemory)data).SetCompletedProcess(process);
			break;
		}
	}

	public void ProcessStarted(SimProcess process)
	{
		if (GetKnownProcessData(process.ID, out var data) == ProcessResult.SeenDirectly)
		{
			InvokeProcessEvent(ProcessStartedEvents, process.ID, data);
		}
		ProcessStartedEvents.Remove(process.ID);
	}

	public void ProcessProducing(SimProcess process)
	{
		if (GetKnownProcessData(process.ID, out var data) == ProcessResult.SeenDirectly)
		{
			InvokeProcessEvent(ProcessProducingEvents, process.ID, data);
		}
	}

	private static void InvokeProcessEvent<T>(Dictionary<SimProcessID, IDActionEvent<T>> events, SimProcessID processID, T eventArgs)
	{
		if (events.TryGetValue(processID, out var value))
		{
			value.Invoke(eventArgs);
		}
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup<SimProcess>.Add(processCompletedMethodID, ProcessCompleted);
		ActionLookup<SimProcess>.Add(processDestroyedMethodID, ProcessDestroyed);
		ActionLookup<SimProcess>.Add(processStartedMethodID, ProcessStarted);
		ActionLookup<SimProcess>.Add(processProducingMethodID, ProcessProducing);
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
		snapshotAllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>>();
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap in AllMovementMaps)
			{
				Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>> dictionary = new Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>();
				snapshotAllMovementMaps.Add(allMovementMap.Key, dictionary);
				foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item in allMovementMap.Value)
				{
					Dictionary<ThreatStance, CyclableID> dictionary2 = new Dictionary<ThreatStance, CyclableID>();
					dictionary.Add(item.Key, dictionary2);
					foreach (KeyValuePair<ThreatStance, MovementMap> item2 in item.Value)
					{
						dictionary2.Add(item2.Key, item2.Value.ID);
					}
				}
			}
			snapshotAllDiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>>();
			foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>> discomfortMap in DiscomfortMaps)
			{
				Dictionary<EntityType, Dictionary<ThreatStance, IMapID>> dictionary3 = new Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>();
				snapshotAllDiscomfortMaps.Add(discomfortMap.Key, dictionary3);
				foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, DiscomfortMap>> item3 in discomfortMap.Value)
				{
					Dictionary<ThreatStance, IMapID> dictionary4 = new Dictionary<ThreatStance, IMapID>();
					dictionary3.Add(item3.Key, dictionary4);
					foreach (KeyValuePair<ThreatStance, DiscomfortMap> item4 in item3.Value)
					{
						dictionary4.Add(item4.Key, item4.Value.ID);
					}
				}
			}
			snapshotThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>();
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, ThreatMap>> threatMap in ThreatMaps)
			{
				Dictionary<ThreatStance, IMapID> dictionary5 = new Dictionary<ThreatStance, IMapID>();
				snapshotThreatMaps.Add(threatMap.Key, dictionary5);
				foreach (KeyValuePair<ThreatStance, ThreatMap> item5 in threatMap.Value)
				{
					dictionary5.Add(item5.Key, item5.Value.ID);
				}
			}
			snapshotProcessMemoryFacts = new Dictionary<SimProcessID, ProcessMemoryID>();
			foreach (KeyValuePair<SimProcessID, ProcessMemory> processMemoryFact in ProcessMemoryFacts)
			{
				snapshotProcessMemoryFacts.Add(processMemoryFact.Key, processMemoryFact.Value.ID);
			}
			snapshotResourceMaps = ResourceMaps.ToDictionary((KeyValuePair<EntityType, ResourceMap> r) => r.Key, (KeyValuePair<EntityType, ResourceMap> r) => r.Value.ID);
			snapshotKnownEntityDataTree = ((knownEntityDataTree != null) ? knownEntityDataTree.GetAllObjectsAndPositions() : null);
		}
		snapshotAllMovementMaps = sn.DoDoubleNestedDictionary(snapshotAllMovementMaps);
		snapshotAllDiscomfortMaps = sn.DoDoubleNestedDictionary(snapshotAllDiscomfortMaps);
		snapshotThreatMaps = sn.DoNestedDictionary(snapshotThreatMaps);
		snapshotProcessMemoryFacts = sn.DoDictionary(snapshotProcessMemoryFacts);
		snapshotKnownEntityDataTree = sn.DoList(snapshotKnownEntityDataTree);
		knownEntityDataTree = (PointQuadTree<EntityID>)sn.DoISnapshot(knownEntityDataTree);
		AllKnownOutsideAgentsOnPlaySite = sn.DoDictionary(AllKnownOutsideAgentsOnPlaySite);
		AllKnownThreatSources = sn.DoDictionary(AllKnownThreatSources);
		ResourceHarvesters = sn.DoMultiMap(ResourceHarvesters);
		processCompletedMethodID = sn.DoEnum(processCompletedMethodID);
		processDestroyedMethodID = sn.DoEnum(processDestroyedMethodID);
		processStartedMethodID = sn.DoEnum(processStartedMethodID);
		processProducingMethodID = sn.DoEnum(processProducingMethodID);
		ProcessStartedEvents = sn.DoDictionary(ProcessStartedEvents);
		ProcessCompletedEvents = sn.DoDictionary(ProcessCompletedEvents);
		ProcessDestroyedEvents = sn.DoDictionary(ProcessDestroyedEvents);
		ProcessProducingEvents = sn.DoDictionary(ProcessProducingEvents);
		AllKnownResourceContainers = sn.DoMultiMapHashSet(AllKnownResourceContainers);
		snapshotResourceMaps = sn.DoDictionary(snapshotResourceMaps);
		SpottedPrey = sn.DoHashSet(SpottedPrey);
		SpottedAnimals = sn.DoHashSet(SpottedAnimals);
		sn.Ignore(ProcessMemoryFacts);
		sn.Ignore(ThreatMaps);
		sn.Ignore(AllMovementMaps);
		sn.Ignore(DiscomfortMaps);
		sn.Ignore(ResourceMaps);
		sn.Ignore(Parent);
		sn.Ignore(TransientThreats);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (knownEntityDataTree != null)
		{
			knownEntityDataTree.SetPreLoadPostProcess(snapshotKnownEntityDataTree);
			knownEntityDataTree.LoadPostProcess(sn);
			snapshotKnownEntityDataTree = null;
		}
		ResourceMaps = snapshotResourceMaps.ToDictionary((KeyValuePair<EntityType, CyclableID> r) => r.Key, (KeyValuePair<EntityType, CyclableID> r) => (ResourceMap)LookUp<ICyclable, CyclableID>.FindByID(r.Value));
		AllMovementMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>>();
		DiscomfortMaps = new Dictionary<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>>();
		ThreatMaps = new Dictionary<EntityType, Dictionary<ThreatStance, ThreatMap>>();
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, CyclableID>>> snapshotAllMovementMap in snapshotAllMovementMaps)
		{
			Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>> dictionary = new Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>();
			AllMovementMaps.Add(snapshotAllMovementMap.Key, dictionary);
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, CyclableID>> item in snapshotAllMovementMap.Value)
			{
				Dictionary<ThreatStance, MovementMap> dictionary2 = new Dictionary<ThreatStance, MovementMap>();
				dictionary.Add(item.Key, dictionary2);
				foreach (KeyValuePair<ThreatStance, CyclableID> item2 in item.Value)
				{
					dictionary2.Add(item2.Key, (MovementMap)LookUp<ICyclable, CyclableID>.FindByID(item2.Value));
				}
			}
		}
		snapshotAllMovementMaps = null;
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, IMapID>>> snapshotAllDiscomfortMap in snapshotAllDiscomfortMaps)
		{
			Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>> dictionary3 = new Dictionary<EntityType, Dictionary<ThreatStance, DiscomfortMap>>();
			DiscomfortMaps.Add(snapshotAllDiscomfortMap.Key, dictionary3);
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, IMapID>> item3 in snapshotAllDiscomfortMap.Value)
			{
				Dictionary<ThreatStance, DiscomfortMap> dictionary4 = new Dictionary<ThreatStance, DiscomfortMap>();
				dictionary3.Add(item3.Key, dictionary4);
				foreach (KeyValuePair<ThreatStance, IMapID> item4 in item3.Value)
				{
					dictionary4.Add(item4.Key, (DiscomfortMap)LookUp<IMap, IMapID>.FindByID(item4.Value));
				}
			}
		}
		snapshotAllDiscomfortMaps = null;
		foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, IMapID>> snapshotThreatMap in snapshotThreatMaps)
		{
			Dictionary<ThreatStance, ThreatMap> dictionary5 = new Dictionary<ThreatStance, ThreatMap>();
			ThreatMaps.Add(snapshotThreatMap.Key, dictionary5);
			foreach (KeyValuePair<ThreatStance, IMapID> item5 in snapshotThreatMap.Value)
			{
				dictionary5.Add(item5.Key, (ThreatMap)LookUp<IMap, IMapID>.FindByID(item5.Value));
			}
		}
		snapshotThreatMaps = null;
		ProcessMemoryFacts = snapshotProcessMemoryFacts.ToDictionary((KeyValuePair<SimProcessID, ProcessMemoryID> m) => m.Key, (KeyValuePair<SimProcessID, ProcessMemoryID> m) => LookUp<ProcessMemory, ProcessMemoryID>.FindByID(m.Value));
		LoadPostProcessRegisterMethodIDs();
		CreateRegulators();
	}
}
