using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps;

public abstract class RegionMap : ISnapshot, ICyclable, ILookUp<ICyclable, CyclableID>
{
	protected enum Progress
	{
		InitFromScratch,
		InitCleanupUnneededSectors,
		InitCopyVariablesSubtiles,
		InitCopyVariablesRegions,
		InitCopyVariablesRegionGraph,
		InitCopyVariablesClearDistances,
		InitClearDirtySectors,
		InitRemoveRegionsAndLinks,
		FillRegions,
		RemoveEdgesBetweenLayers,
		BuildInternalRegionGraph,
		BuildRegionGraphBetweenSectors
	}

	public enum Result
	{
		OK,
		Wait,
		NoAccess
	}

	private enum AdjacentSector
	{
		Left,
		Right,
		Over,
		Under
	}

	public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

	public string IDName;

	public const ushort BaseLayerRegionColors = 40000;

	protected int currentSectorIndex;

	public static double totalComputationAllInstancesInSeconds;

	private ushort regionColorCounter = 1;

	protected SubtileLayers layers;

	private SubtileLayersID snapshotLayers;

	protected SubtileLayer targetLayer;

	private SubtileLayerID snapshotTargetLayer;

	protected bool sectorsAreDirty;

	private int subtileMapWidth;

	private int subtileMapHeight;

	protected Dictionary<ushort, Region> regions = new Dictionary<ushort, Region>();

	protected List<Point> dirtySectors = new List<Point>();

	private List<Point> dirtySectorsRollbackAfterSave;

	private HashSet<Point> scannedSectors = new HashSet<Point>();

	public Dictionary<ushort, Dictionary<ushort, RegionEdge>> RegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

	public Dictionary<ushort, Dictionary<ushort, RegionPathFinderNodeBFS>> AllRegionCosts;

	public Dictionary<ushort, Dictionary<ushort, RegionPathID>> RegionPaths;

	protected Dictionary<Point, Tuple<ushort, double, int>> cachedClosestRegionToBlockedSubtile;

	protected Dictionary<ushort, Region> newRegions = new Dictionary<ushort, Region>();

	public Dictionary<ushort, Dictionary<ushort, RegionEdge>> newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();

	private List<ushort> regionGraphKeyListForCopying;

	private int regionGraphCopyProgressIndex;

	protected Progress progress;

	protected bool isComputing;

	public FloodFill FloodFill;

	private Dictionary<ushort, CyclableID> regionBFSPlanners = new Dictionary<ushort, CyclableID>();

	private Dictionary<ushort, Dictionary<ushort, CyclableID>> regionAStarPlanners = new Dictionary<ushort, Dictionary<ushort, CyclableID>>();

	private List<RegionSearchRequestID> regionSearchRequests = new List<RegionSearchRequestID>();

	private Queue<ushort> UnusedRegionColors = new Queue<ushort>();

	private HashSet<Region> regionsToRemove = new HashSet<Region>();

	private int cyclesToFillRegions;

	private int cyclesToBuildGraph;

	private double timeTaken;

	private HighResolutionTime timer;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

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

	public double? UpdateInterval => null;

	protected abstract ushort ColorStartOfRange { get; }

	public abstract Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph { get; }

	public abstract Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges { get; }

	public abstract Dictionary<ushort, Region> BottomRegions { get; }

	public bool IsPaused { get; set; }

	protected virtual bool CanServiceRequests => true;

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

	public bool IsSnapshotted { get; set; }

	public bool UnregisterBeforeSnapshot => true;

	public void AssertSearch(RegionSearchPlanner planner, RegionSearchRequest searchRequest)
	{
		_ = planner.Start;
		layers.GetRegion(searchRequest.FromSubtile.X, searchRequest.FromSubtile.Y);
	}

	public RegionMap()
	{
	}

	public RegionMap(SubtileLayers layers, SubtileLayer targetLayer)
	{
		AddToLookup();
		this.layers = layers;
		this.targetLayer = targetLayer;
		subtileMapWidth = The.Map.mapSubtileWidth;
		subtileMapHeight = The.Map.mapSubtileHeight;
		AllRegionCosts = new Dictionary<ushort, Dictionary<ushort, RegionPathFinderNodeBFS>>();
		RegionPaths = new Dictionary<ushort, Dictionary<ushort, RegionPathID>>();
		cachedClosestRegionToBlockedSubtile = new Dictionary<Point, Tuple<ushort, double, int>>();
		Init();
		regionColorCounter = ColorStartOfRange;
		timer = new HighResolutionTime();
	}

	public virtual void Destroy()
	{
		if (The.Sim.CycleManager.IsRegistered(this))
		{
			The.Sim.CycleManager.UnRegister(this);
		}
		AddLog("Destroyed, ID = " + id);
		CancelAllSearches();
		RemoveIDEntry();
	}

	private void Init()
	{
		FloodFill = new FloodFill();
		CreateRegulators();
	}

	protected virtual void CreateRegulators()
	{
	}

	public abstract Region GetRegion(ushort color);

	public abstract Region GetRegionInProgress(ushort color);

	public Region GetRegion(int subtileX, int subtileY)
	{
		return GetRegion(layers.GetRegion(subtileX, subtileY));
	}

	public SubtileSector GetSector(Point subtile)
	{
		return layers.GetSectorFromSubtile(subtile);
	}

	public SubtileSector GetSector(Point subtile, out int layerIndex)
	{
		return layers.GetSectorFromSubtile(subtile, out layerIndex);
	}

	public void MarkDirtySector(int subtileX, int subtileY)
	{
		SubtileSector sectorFromSubtiles = targetLayer.GetSectorFromSubtiles(subtileX, subtileY);
		if (sectorFromSubtiles != null)
		{
			MarkDirtySector(sectorFromSubtiles);
		}
	}

	public void MarkDirtySector(SubtileSector sector)
	{
		sector.BlockedStatusHasChanged = true;
		sectorsAreDirty = true;
	}

	public override string ToString()
	{
		return "Region map: " + IDName;
	}

	public ushort GetRegionColor(int subtileX, int subtileY)
	{
		return layers.GetRegion(subtileX, subtileY);
	}

	public ushort GetRegionColorInProgress(int subtileX, int subtileY)
	{
		return layers.GetRegion(subtileX, subtileY, inProgress: true);
	}

	private static bool InitCopyVariablesRegionGraph(Dictionary<ushort, Dictionary<ushort, RegionEdge>> RegionGraph, ref Dictionary<ushort, Dictionary<ushort, RegionEdge>> newRegionGraph, ref int regionGraphCopyProgressIndex, ref List<ushort> regionGraphKeyListForCopying)
	{
		if (regionGraphKeyListForCopying == null)
		{
			newRegionGraph = new Dictionary<ushort, Dictionary<ushort, RegionEdge>>();
			regionGraphKeyListForCopying = RegionGraph.Keys.ToList();
			regionGraphCopyProgressIndex = 0;
			return false;
		}
		int num = Math.Min(regionGraphCopyProgressIndex + 50, regionGraphKeyListForCopying.Count);
		for (int i = regionGraphCopyProgressIndex; i < num; i++)
		{
			ushort key = regionGraphKeyListForCopying[i];
			Dictionary<ushort, RegionEdge> dictionary = new Dictionary<ushort, RegionEdge>();
			newRegionGraph.Add(key, dictionary);
			foreach (KeyValuePair<ushort, RegionEdge> item in RegionGraph[key])
			{
				dictionary.Add(item.Key, new RegionEdge(item.Value.FromRegion, item.Value.ToRegion, item.Value.Length, item.Value.HasRoad));
			}
		}
		regionGraphCopyProgressIndex = num;
		if (regionGraphCopyProgressIndex == regionGraphKeyListForCopying.Count)
		{
			regionGraphKeyListForCopying = null;
			return true;
		}
		return false;
	}

	private void GetDirtySectors()
	{
		dirtySectors.Clear();
		targetLayer.IterateDirtySectors(delegate(SubtileSector s)
		{
			dirtySectors.Add(s.Coords);
			s.BlockedStatusHasChanged = false;
		});
		sectorsAreDirty = false;
	}

	private void InitCopyVariablesRegions()
	{
		newRegions = new Dictionary<ushort, Region>(regions);
		AddLog("InitCopyVariablesRegions done.");
	}

	private bool ClearDirtySectorSubtiles()
	{
		if (dirtySectors.Count > 0)
		{
			bool isLastSector;
			SubtileSector currentDirtySectorAndIncrement = GetCurrentDirtySectorAndIncrement(out isLastSector);
			List<ushort> list = new List<ushort>();
			currentDirtySectorAndIncrement.ClearRegions(list);
			{
				foreach (ushort item in list)
				{
					regionsToRemove.Add(newRegions[item]);
				}
				return isLastSector;
			}
		}
		return true;
	}

	protected virtual void RecomputeStarted()
	{
		if (sectorsAreDirty)
		{
			AddLog("Recompute started, isComputing = true");
			isComputing = true;
		}
	}

	protected virtual void RecomputeFinished()
	{
		isComputing = false;
	}

	protected virtual void RecomputeAborted()
	{
		AddLog("Recompute aborted.");
		isComputing = false;
	}

	protected virtual void StartBuildingGraphBetweenSectors()
	{
	}

	public virtual void PrintInfo(StringBuilder text)
	{
		text.Append($"RegionMap {ID}:");
	}

	public bool CycleOnce()
	{
		bool result = false;
		timer.Start();
		_ = IDName == "ExposedIndigHerbivoreCautiousFoot";
		switch (progress)
		{
		case Progress.InitFromScratch:
			RecomputeStarted();
			GetDirtySectors();
			if (dirtySectors.Count == 0)
			{
				RecomputeAborted();
				return true;
			}
			progress = Progress.FillRegions;
			break;
		case Progress.InitCleanupUnneededSectors:
			RecomputeStarted();
			if (!sectorsAreDirty)
			{
				RecomputeAborted();
				return true;
			}
			GetDirtySectors();
			progress = Progress.InitCopyVariablesSubtiles;
			break;
		case Progress.InitCopyVariablesSubtiles:
			if (InitCopyVariablesSubtiles())
			{
				AddLog("InitCopyVariablesSubtiles done.");
				progress = Progress.InitCopyVariablesRegions;
			}
			break;
		case Progress.InitCopyVariablesRegions:
			InitCopyVariablesRegions();
			progress = Progress.InitCopyVariablesRegionGraph;
			break;
		case Progress.InitCopyVariablesRegionGraph:
			if (InitCopyVariablesRegionGraph(RegionGraph, ref newRegionGraph, ref regionGraphCopyProgressIndex, ref regionGraphKeyListForCopying))
			{
				AddLog("InitCopyVariablesRegionGraph done.");
				progress = Progress.InitCopyVariablesClearDistances;
			}
			break;
		case Progress.InitCopyVariablesClearDistances:
			if (InitCopyVariablesClearDistances())
			{
				AddLog("InitCopyVariablesClearDistances done.");
				progress = Progress.InitClearDirtySectors;
			}
			break;
		case Progress.InitClearDirtySectors:
			if (ClearDirtySectorSubtiles())
			{
				AddLog("ClearDirtySectorSubtiles done.");
				progress = Progress.InitRemoveRegionsAndLinks;
			}
			break;
		case Progress.InitRemoveRegionsAndLinks:
			RemoveRegions();
			progress = Progress.FillRegions;
			break;
		case Progress.FillRegions:
			cyclesToFillRegions++;
			if (CreateRegions())
			{
				AddLog("FillRegions done");
				progress = Progress.BuildInternalRegionGraph;
			}
			break;
		case Progress.BuildInternalRegionGraph:
			cyclesToBuildGraph++;
			if (BuildInternalEdgesForDirtySectors())
			{
				AddLog("BuildRegionGraph done");
				progress = Progress.RemoveEdgesBetweenLayers;
				StartBuildingGraphBetweenSectors();
			}
			break;
		case Progress.RemoveEdgesBetweenLayers:
			if (RemoveAllEdgesBetweenLayers())
			{
				AssertRegionGraph();
				AddLog("RemoveEdgesBetweenLayers done");
				progress = Progress.BuildRegionGraphBetweenSectors;
				cyclesToBuildGraph = 0;
			}
			break;
		case Progress.BuildRegionGraphBetweenSectors:
			cyclesToBuildGraph++;
			if (!BuildRegionGraphBetweenSectors())
			{
				break;
			}
			AddLog("BuildRegionGraphBetweenSectors done");
			SetProgressAtRepairStart();
			CancelAllSearches();
			AssertRegionGraph();
			regions = newRegions;
			RegionGraph = newRegionGraph;
			FinishSectors();
			scannedSectors.Clear();
			AllRegionCosts.Clear();
			foreach (KeyValuePair<ushort, Dictionary<ushort, RegionPathID>> regionPath in RegionPaths)
			{
				foreach (KeyValuePair<ushort, RegionPathID> item in regionPath.Value)
				{
					LookUp<RegionPath, RegionPathID>.FindByID(item.Value)?.Destroy();
				}
			}
			RegionPaths.Clear();
			cachedClosestRegionToBlockedSubtile.Clear();
			RecomputeFinished();
			StartPendingSearchRequests();
			AssertRegionGraph(useFieldsInProgress: false);
			dirtySectors.Clear();
			cyclesToFillRegions = 0;
			cyclesToBuildGraph = 0;
			result = true;
			break;
		}
		timeTaken = timer.GetTime();
		_ = timeTaken;
		_ = 0.002;
		return result;
	}

	protected void SetProgressAtRepairStart()
	{
		progress = Progress.InitCleanupUnneededSectors;
		currentSectorIndex = 0;
		AddLog("Progress set to repair start (InitCleanupUnneededSectors)");
	}

	protected virtual void AssertRegionGraph(bool useFieldsInProgress = true)
	{
	}

	public Region CreateRegion(Point center)
	{
		ushort color;
		if (UnusedRegionColors.Count > 0)
		{
			color = UnusedRegionColors.Dequeue();
		}
		else
		{
			color = regionColorCounter;
			regionColorCounter++;
		}
		Region region = new Region(center, color);
		newRegions.Add(region.Color, region);
		return region;
	}

	private bool CreateRegions()
	{
		if (dirtySectors.Count > 0)
		{
			if (GetCurrentDirtySector().FloodFill(this))
			{
				return IncrementSectorIndex();
			}
			return false;
		}
		return true;
	}

	private bool InitCopyVariablesSubtiles()
	{
		if (dirtySectors.Count > 0)
		{
			GetCurrentDirtySectorAndIncrement(out var isLastSector).InitCopyVariablesSubtiles();
			return isLastSector;
		}
		return true;
	}

	private bool InitCopyVariablesClearDistances()
	{
		if (dirtySectors.Count > 0)
		{
			GetCurrentDirtySectorAndIncrement(out var isLastSector).InitCopyVariablesClearDistances();
			return isLastSector;
		}
		return true;
	}

	private void FinishSectors()
	{
		foreach (Point dirtySector in dirtySectors)
		{
			targetLayer.GetSector(dirtySector.X, dirtySector.Y).FinishRegions();
		}
	}

	protected SubtileSector GetCurrentDirtySectorAndIncrement(out bool isLastSector)
	{
		SubtileSector currentDirtySector = GetCurrentDirtySector();
		isLastSector = IncrementSectorIndex();
		return currentDirtySector;
	}

	private bool IncrementSectorIndex()
	{
		bool result;
		if (currentSectorIndex == dirtySectors.Count - 1)
		{
			result = true;
			currentSectorIndex = 0;
		}
		else
		{
			result = false;
			currentSectorIndex++;
		}
		return result;
	}

	private SubtileSector GetCurrentDirtySector()
	{
		Point point = dirtySectors[currentSectorIndex];
		return targetLayer.GetSector(point.X, point.Y);
	}

	protected SubtileSector GetCurrentSector(out bool isLastSector)
	{
		SubtileSector subtileSector = null;
		do
		{
			int result;
			int x = Math.DivRem(currentSectorIndex, targetLayer.SectorsAcrossWidth, out result);
			Point point = new Point(x, result);
			subtileSector = targetLayer.GetSector(point.X, point.Y);
			if (point.X == targetLayer.SectorsAcrossWidth - 1 && point.Y == targetLayer.SectorsAcrossHeight - 1)
			{
				isLastSector = true;
				currentSectorIndex = 0;
			}
			else
			{
				isLastSector = false;
				currentSectorIndex++;
			}
		}
		while (subtileSector == null && !isLastSector);
		return subtileSector;
	}

	private void StartPendingSearchRequests()
	{
		if (regionSearchRequests.Count > 0)
		{
			for (int num = regionSearchRequests.Count - 1; num >= 0; num--)
			{
				RegionSearchRequest regionSearchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(regionSearchRequests[num]);
				RegisterSearch(regionSearchRequest);
				AddLog("(Re)started search from " + regionSearchRequest.FromSubtile);
			}
		}
	}

	protected void CancelAllSearches()
	{
		foreach (KeyValuePair<ushort, CyclableID> regionBFSPlanner in regionBFSPlanners)
		{
			((RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(regionBFSPlanner.Value))?.Destroy();
		}
		foreach (KeyValuePair<ushort, Dictionary<ushort, CyclableID>> regionAStarPlanner in regionAStarPlanners)
		{
			foreach (KeyValuePair<ushort, CyclableID> item in regionAStarPlanner.Value)
			{
				((RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(item.Value))?.Destroy();
			}
		}
		regionBFSPlanners.Clear();
		regionAStarPlanners.Clear();
	}

	public Result GetDistanceToEntityUsingEntityType(EntityID? fromEntityID, EntityID? toEntityID, SharedKnowledge sharedKnowledge, ref float distance, Allegiance allegiance, EntityType intelligentEntityType, out EntityResult fromEntityKnowledgeResult, out EntityResult toEntityKnowledgeResult, Point? startingSubtile = null, Point? endingSubtile = null, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null)
	{
		fromEntityKnowledgeResult = EntityResult.SeenDirectly;
		toEntityKnowledgeResult = EntityResult.SeenDirectly;
		Point? subtilePosition;
		if (fromEntityID.HasValue)
		{
			if (!GetEntityPosition(fromEntityID.Value, intelligentEntityType, allegiance, null, sharedKnowledge, out subtilePosition, out fromEntityKnowledgeResult))
			{
				return Result.NoAccess;
			}
		}
		else
		{
			subtilePosition = startingSubtile.Value;
		}
		Point? subtilePosition2;
		if (toEntityID.HasValue)
		{
			if (!GetEntityPosition(toEntityID.Value, intelligentEntityType, allegiance, null, sharedKnowledge, out subtilePosition2, out toEntityKnowledgeResult))
			{
				return Result.NoAccess;
			}
		}
		else
		{
			subtilePosition2 = endingSubtile.Value;
		}
		if (subtilePosition.HasValue && subtilePosition2.HasValue)
		{
			return GetDistance(null, subtilePosition.Value, subtilePosition2.Value, ref distance, sendMessageToEntity, notifyWhenFinished);
		}
		return Result.NoAccess;
	}

	public Result GetDistanceToEntityUsingWorldLocation(Entity activeEntity, IKnownEntityData fromEntity, IKnownEntityData toEntity, ref float distance, Vector3? fromLocation = null, Vector3? toLocation = null, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool doRegionSearchNow = false)
	{
		Point? startingSubtile = null;
		Point? endingSubtile = null;
		if (fromLocation.HasValue)
		{
			startingSubtile = MapManager.WorldPosToSubtile(fromLocation.Value);
		}
		if (toLocation.HasValue)
		{
			endingSubtile = MapManager.WorldPosToSubtile(toLocation.Value);
		}
		return GetDistanceToEntity(activeEntity, fromEntity, toEntity, ref distance, startingSubtile, endingSubtile, sendMessageToEntity, notifyWhenFinished, doRegionSearchNow);
	}

	public Result GetDistanceToEntity(Entity activeEntity, IKnownEntityData fromEntity, IKnownEntityData toEntity, ref float distance, Point? startingSubtile = null, Point? endingSubtile = null, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool giveClientFeedback = true, Allegiance allegiance = null, bool allowTransactingWithAgentsInAllegiance = false)
	{
		Point? fromSubtile;
		Point? toSubtile;
		return GetDistanceToEntity(activeEntity, fromEntity, toEntity, ref distance, out fromSubtile, out toSubtile, startingSubtile, endingSubtile, sendMessageToEntity, notifyWhenFinished, giveClientFeedback, allegiance, allowTransactingWithAgentsInAllegiance);
	}

	public Result GetDistanceToEntity(Entity activeEntity, IKnownEntityData fromEntity, IKnownEntityData toEntity, ref float distance, out Point? fromSubtile, out Point? toSubtile, Point? startingSubtile = null, Point? endingSubtile = null, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool giveClientFeedback = true, Allegiance allegiance = null, bool allowTransactingWithAgentsInAllegiance = false)
	{
		fromSubtile = null;
		toSubtile = null;
		try
		{
			if (fromEntity != null)
			{
				if (!GetEntityPosition(fromEntity, activeEntity, activeEntity.EntityType, allegiance, allowTransactingWithAgentsInAllegiance, out fromSubtile))
				{
					return Result.NoAccess;
				}
			}
			else
			{
				fromSubtile = startingSubtile.Value;
			}
			if (toEntity != null)
			{
				if (!GetEntityPosition(toEntity, activeEntity, activeEntity.EntityType, allegiance, allowTransactingWithAgentsInAllegiance, out toSubtile))
				{
					return Result.NoAccess;
				}
			}
			else
			{
				toSubtile = endingSubtile.Value;
			}
			if (fromSubtile.HasValue && toSubtile.HasValue)
			{
				Result distance2 = GetDistance(activeEntity, fromSubtile.Value, toSubtile.Value, ref distance, sendMessageToEntity, notifyWhenFinished);
				if (giveClientFeedback && toEntity != null)
				{
					if (distance2 == Result.NoAccess)
					{
						The.Client.SetEntityInaccessible(activeEntity.Intelligence.Allegiance, toEntity, isInaccessible: true);
						bool? flag = GoalEvaluator.ComputeIsBlockedByThreat(activeEntity, toEntity);
						if (flag.HasValue)
						{
							The.Client.SetEntityBlockedByThreat(activeEntity.Intelligence.Allegiance, toEntity, flag.Value);
						}
					}
					else
					{
						The.Client.SetEntityInaccessible(activeEntity.Intelligence.Allegiance, toEntity, isInaccessible: false);
					}
				}
				return distance2;
			}
			return Result.NoAccess;
		}
		catch (InvalidOperationException ex)
		{
			string text = $"fromSubtile: {fromSubtile}, toSubtile: {toSubtile}, startingSubtile: {startingSubtile}, endingSubtile: {endingSubtile}";
			throw new Exception(ex.Message + " " + text, ex);
		}
	}

	private bool GetEntityPosition(EntityID entityToGetPositionFor, EntityType intelligentEntityType, Allegiance allegiance, Entity activeEntityToGoToEntrance, SharedKnowledge sharedKnowledge, out Point? subtilePosition, out EntityResult entityKnowledgeResult)
	{
		subtilePosition = null;
		entityKnowledgeResult = sharedKnowledge.GetKnownData(entityToGetPositionFor, out var data);
		if (entityKnowledgeResult == EntityResult.Remembered || entityKnowledgeResult == EntityResult.SeenDirectly)
		{
			return GetEntityPosition(data, activeEntityToGoToEntrance, intelligentEntityType, allegiance, allowTransactingWithAgentsInAllegiance: true, out subtilePosition);
		}
		return false;
	}

	private bool GetEntityPosition(IKnownEntityData entityData, Entity activeEntityToGoToEntrance, EntityType intelligentEntityType, Allegiance allegiance, bool allowTransactingWithAgentsInAllegiance, out Point? subtilePosition)
	{
		subtilePosition = null;
		if (entityData.ContainedBy.HasValue)
		{
			if (activeEntityToGoToEntrance != null && entityData.ContainedBy == activeEntityToGoToEntrance.EntityID)
			{
				subtilePosition = MapManager.WorldPosToSubtile(activeEntityToGoToEntrance.AccessPoint.Value);
				return true;
			}
			Entity entity = Entity.FindByID(entityData.ContainedBy.Value);
			if (entity == null)
			{
				return false;
			}
			if (entity == activeEntityToGoToEntrance)
			{
				subtilePosition = MapManager.WorldPosToSubtile(activeEntityToGoToEntrance.PlaySiteLocation);
			}
			else
			{
				if (!entity.EntityType.ContainerType.AllowedInContainer(intelligentEntityType) && !entity.EntityType.ContainerType.CanTransactWithContainer(intelligentEntityType) && (!allowTransactingWithAgentsInAllegiance || entity.EntityType.IntelligenceType == null || entity.Intelligence.Allegiance != allegiance))
				{
					return false;
				}
				subtilePosition = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
			}
		}
		else
		{
			if (!entityData.Location.HasValue)
			{
				return false;
			}
			subtilePosition = MapManager.WorldPosToSubtile(entityData.AccessPoint.Value);
		}
		return true;
	}

	public virtual Result GetDistance(Entity entity, Point fromSubtile, Point toSubtile, ref float distance, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool registerIfNotReady = true)
	{
		if (fromSubtile == toSubtile)
		{
			distance = 0f;
			return Result.OK;
		}
		if (CanServiceRequests)
		{
			ushort num = layers.GetRegion(fromSubtile.X, fromSubtile.Y);
			ushort region = layers.GetRegion(toSubtile.X, toSubtile.Y);
			if (region == 0)
			{
				return Result.NoAccess;
			}
			if (num == 0)
			{
				num = GetClosestRegionToBlockedSubtile(ref fromSubtile, region);
				fromSubtile = GetRegion(num).CenterInSubtiles;
			}
			if (num == region)
			{
				distance = 16f * Common.DistanceOctile(fromSubtile, toSubtile);
				return Result.OK;
			}
			if (!AllRegionCosts.TryGetValue(num, out var value))
			{
				return SearchDirectly(fromSubtile, toSubtile, ref distance);
			}
			if (value.TryGetValue(region, out var value2))
			{
				float g = value2.G;
				return GetApproximateDistanceFromCost(fromSubtile, toSubtile, ref distance, num, region, g);
			}
			return SearchDirectly(fromSubtile, toSubtile, ref distance);
		}
		if (registerIfNotReady)
		{
			StoreSearchRequest(entity, ref fromSubtile, ref toSubtile, sendMessageToEntity, notifyWhenFinished);
		}
		return Result.Wait;
	}

	private RegionSearchRequest StoreSearchRequest(Entity entity, ref Point fromSubtile, ref Point toSubtile, bool sendMessageToEntity, MethodID? notifyWhenFinished)
	{
		RegionSearchRequest regionSearchRequest = new RegionSearchRequest(entity?.EntityID, fromSubtile, toSubtile, sendMessageToEntity, notifyWhenFinished);
		regionSearchRequests.Add(regionSearchRequest.ID);
		return regionSearchRequest;
	}

	private ushort GetClosestRegionToBlockedSubtile(ref Point fromSubtile, ushort toRegion)
	{
		ushort num;
		if (!cachedClosestRegionToBlockedSubtile.TryGetValue(fromSubtile, out var value))
		{
			num = RegionSearchPlanner.GetClosestRegionToDestination(RegionSearcher.PathInfo.CostOnly, this, toRegion, 100, MapManager.SubTileToWorldPos3(fromSubtile));
			Region region = GetRegion(num);
			int item = (int)Common.DistanceOctile(fromSubtile, region.CenterInSubtiles);
			cachedClosestRegionToBlockedSubtile.Add(fromSubtile, new Tuple<ushort, double, int>(num, The.Sim.TotalUnPausedGameTimeInSeconds, item));
		}
		else
		{
			num = value.Item1;
		}
		return num;
	}

	public bool GetConnectors(ushort fromRegion, out Dictionary<ushort, RegionEdge> connections)
	{
		if (fromRegion < 40000)
		{
			if (BottomBlockedEdges != null)
			{
				Dictionary<ushort, RegionEdge> dictionary = null;
				RegionGraph.TryGetValue(fromRegion, out var value);
				BottomRegionGraph.TryGetValue(fromRegion, out var value2);
				if (value != null && value2 != null)
				{
					dictionary = new Dictionary<ushort, RegionEdge>(value);
					foreach (KeyValuePair<ushort, RegionEdge> item in value2)
					{
						dictionary.Add(item.Key, item.Value);
					}
				}
				else
				{
					value = value ?? value2;
				}
				if (BottomBlockedEdges != null && BottomBlockedEdges.TryGetValue(fromRegion, out var value3) && value3.Count > 0)
				{
					foreach (ushort item2 in value3)
					{
						if (dictionary == null)
						{
							dictionary = new Dictionary<ushort, RegionEdge>(value);
						}
						dictionary.Remove(item2);
					}
				}
				connections = dictionary ?? value;
				if (connections != null)
				{
					return connections.Count > 0;
				}
				return false;
			}
			return BottomRegionGraph.TryGetValue(fromRegion, out connections);
		}
		return RegionGraph.TryGetValue(fromRegion, out connections);
	}

	protected bool IsSameOrNeighbour(ushort fromRegion, ushort toRegion)
	{
		if (GetConnectors(fromRegion, out var connections) && connections.ContainsKey(toRegion))
		{
			return true;
		}
		return false;
	}

	private Result GetApproximateDistanceFromCost(Point fromSubtile, Point toSubtile, ref float distance, ushort fromRegion, ushort toRegion, float cost)
	{
		if (Common.IsGreaterThanOrEqual(cost, 0.0))
		{
			if (IsSameOrNeighbour(fromRegion, toRegion))
			{
				distance = 16f * Common.DistanceOctile(fromSubtile, toSubtile);
				return Result.OK;
			}
			distance = cost;
			return Result.OK;
		}
		return Result.NoAccess;
	}

	private void RegisterSearch(RegionSearchRequest searchRequest, bool doBFS = false)
	{
		Point FromSubtile = searchRequest.FromSubtile;
		Point ToSubtile = searchRequest.ToSubtile;
		if (!PrepareSearch(ref FromSubtile, ref ToSubtile, out var fromRegion, out var toRegion, out var searchEndpointsMoved))
		{
			Entity entity = null;
			if (searchRequest.Entity.HasValue)
			{
				entity = Entity.FindByID(searchRequest.Entity.Value);
			}
			RemoveSearchAndNotify(entity, searchRequest, 0f, Result.NoAccess);
			return;
		}
		if (searchEndpointsMoved)
		{
			searchRequest.AddLog($"Start and destinations moved, from {searchRequest.FromSubtile} - {searchRequest.ToSubtile} to {FromSubtile} - {ToSubtile}");
			searchRequest.FromSubtile = FromSubtile;
			searchRequest.ToSubtile = ToSubtile;
		}
		RegionSearchPlanner regionSearchPlanner = null;
		CyclableID value2;
		if (!doBFS)
		{
			if (regionAStarPlanners.TryGetValue(fromRegion, out var value) && value.TryGetValue(toRegion, out value2))
			{
				regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(value2);
			}
			if (regionSearchPlanner == null)
			{
				regionSearchPlanner = new RegionSearchPlanner(this, fromRegion, toRegion);
			}
		}
		else
		{
			if (regionBFSPlanners.TryGetValue(fromRegion, out value2))
			{
				regionSearchPlanner = (RegionSearchPlanner)LookUp<ICyclable, CyclableID>.FindByID(value2);
			}
			if (regionSearchPlanner == null)
			{
				regionSearchPlanner = new RegionSearchPlanner(this, fromRegion);
			}
		}
		regionSearchPlanner.AddSearchRequest(searchRequest);
	}

	private bool PrepareSearch(ref Point FromSubtile, ref Point ToSubtile, out ushort fromRegion, out ushort toRegion, out bool searchEndpointsMoved)
	{
		searchEndpointsMoved = false;
		toRegion = layers.GetRegion(ToSubtile);
		fromRegion = layers.GetRegion(FromSubtile);
		if (toRegion == 0)
		{
			return false;
		}
		if (fromRegion == 0)
		{
			AssertRegionGraph(useFieldsInProgress: false);
			ushort closestRegionToBlockedSubtile = GetClosestRegionToBlockedSubtile(ref FromSubtile, toRegion);
			Point centerInSubtiles = GetRegion(closestRegionToBlockedSubtile).CenterInSubtiles;
			Point centerInSubtiles2 = GetRegion(toRegion).CenterInSubtiles;
			searchEndpointsMoved = true;
			fromRegion = closestRegionToBlockedSubtile;
			FromSubtile = centerInSubtiles;
			ToSubtile = centerInSubtiles2;
		}
		return true;
	}

	public virtual RegionPathID? GetRegionPath(Point startSubtile, Point destinationSubtile)
	{
		RegionPathID? result = null;
		ushort regionColor = GetRegionColor(startSubtile.X, startSubtile.Y);
		ushort regionColor2 = GetRegionColor(destinationSubtile.X, destinationSubtile.Y);
		if (RegionPaths.TryGetValue(regionColor, out var value) && value.TryGetValue(regionColor2, out var value2))
		{
			result = value2;
		}
		return result;
	}

	private Result SearchDirectly(Point FromSubtile, Point ToSubtile, ref float distance)
	{
		if (!PrepareSearch(ref FromSubtile, ref ToSubtile, out var fromRegion, out var toRegion, out var _))
		{
			return Result.NoAccess;
		}
		RegionPath pathList = null;
		float cost = 0f;
		RegionSearchPlanner.GetPathDirectly(RegionSearcher.PathInfo.FullPath, this, fromRegion, toRegion, ref pathList, ref cost);
		Result num = CachePathResult(FromSubtile, ToSubtile, fromRegion, toRegion, pathList);
		if (num == Result.OK)
		{
			GetApproximateDistanceFromCost(FromSubtile, ToSubtile, ref distance, fromRegion, toRegion, pathList.Cost);
		}
		return num;
	}

	private Result CachePathResult(Point fromSubtile, Point toSubtile, ushort fromRegion, ushort toRegion, RegionPath path)
	{
		float cost;
		if (path != null && Common.IsGreaterThanOrEqual(path.Cost, 0f))
		{
			cost = path.Cost;
			Common.AddToNestedDictionary(AllRegionCosts, fromRegion, toRegion, new RegionPathFinderNodeBFS(cost, toRegion));
			Common.AddToNestedDictionary(RegionPaths, fromRegion, toRegion, path.ID);
			return Result.OK;
		}
		cost = -1f;
		Common.AddToNestedDictionary(AllRegionCosts, fromRegion, toRegion, new RegionPathFinderNodeBFS(cost, toRegion));
		return Result.NoAccess;
	}

	public void PlannerBFSFinished(RegionSearchPlanner regionSearchPlanner, Dictionary<ushort, RegionPathFinderNodeBFS> result)
	{
		AllRegionCosts[regionSearchPlanner.Start] = result;
		regionBFSPlanners.Remove(regionSearchPlanner.Start);
		foreach (RegionSearchRequestID searchRequest2 in regionSearchPlanner.SearchRequests)
		{
			RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequest2);
			SearchFinished(searchRequest);
		}
	}

	public void PlannerAStarFinished(RegionSearchPlanner regionSearchPlanner, RegionPath result)
	{
		Common.RemoveFromNestedDictionary(regionAStarPlanners, regionSearchPlanner.Start, regionSearchPlanner.Destination);
		foreach (RegionSearchRequestID searchRequest in regionSearchPlanner.SearchRequests)
		{
			RegionSearchRequest regionSearchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequest);
			CachePathResult(regionSearchRequest.FromSubtile, regionSearchRequest.ToSubtile, regionSearchPlanner.Start, regionSearchPlanner.Destination, result);
			SearchFinished(regionSearchRequest);
		}
	}

	public void AddSearchPlanner(RegionSearchPlanner planner)
	{
		if (planner.IsBFS)
		{
			regionBFSPlanners.Add(planner.Start, planner.ID);
		}
		else
		{
			Common.AddToNestedDictionary(regionAStarPlanners, planner.Start, planner.Destination, planner.ID);
		}
	}

	private void SearchFinished(RegionSearchRequest searchRequest)
	{
		float distance = -1f;
		Result result = Result.NoAccess;
		Entity entity = null;
		if (searchRequest.Entity.HasValue)
		{
			entity = Entity.FindByID(searchRequest.Entity.Value);
		}
		if (entity != null)
		{
			result = GetDistance(entity, searchRequest.FromSubtile, searchRequest.ToSubtile, ref distance);
		}
		RemoveSearchAndNotify(entity, searchRequest, distance, result);
	}

	private void RemoveSearchAndNotify(Entity entity, RegionSearchRequest searchRequest, float distance, Result result)
	{
		regionSearchRequests.Remove(searchRequest.ID);
		searchRequest.Notify(entity, result, distance);
		searchRequest.Destroy();
	}

	protected virtual void RemoveRegions()
	{
		AddLog("RemoveRegions " + regionsToRemove.Count);
		foreach (Region item in regionsToRemove)
		{
			RemoveRegion(item);
		}
		regionsToRemove.Clear();
	}

	protected void RemoveRegion(Region region)
	{
		newRegions.Remove(region.Color);
		UnusedRegionColors.Enqueue(region.Color);
		if (!newRegionGraph.TryGetValue(region.Color, out var value))
		{
			return;
		}
		foreach (KeyValuePair<ushort, RegionEdge> item in value)
		{
			if (newRegionGraph.TryGetValue(item.Value.ToRegion, out var value2))
			{
				value2.Remove(region.Color);
			}
		}
		newRegionGraph.Remove(region.Color);
	}

	private void ScanTopLeftCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
	{
		SubtileSector unfinishedSector = layers.GetUnfinishedSector(otherSectorCoords);
		Point subtile = new Point(0, 0);
		ushort regionInProgress = thisSector.GetRegionInProgress(subtile);
		int num = targetLayer.SectorSize - 1;
		ushort regionInProgress2 = unfinishedSector.GetRegionInProgress(num, num);
		if (regionInProgress2 > 0)
		{
			AddEdgesOrConnectors(regionInProgress2, regionInProgress);
			if (thisOveriddenSector != null)
			{
				ushort region = thisOveriddenSector.GetRegion(subtile);
				BlockBottomConnection(region, regionInProgress2);
			}
		}
	}

	private void ScanTopRightCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
	{
		SubtileSector unfinishedSector = layers.GetUnfinishedSector(otherSectorCoords);
		int num = targetLayer.SectorSize - 1;
		Point subtile = new Point(num, 0);
		ushort regionInProgress = thisSector.GetRegionInProgress(subtile);
		ushort regionInProgress2 = unfinishedSector.GetRegionInProgress(0, num);
		if (regionInProgress2 > 0)
		{
			AddEdgesOrConnectors(regionInProgress2, regionInProgress);
			if (thisOveriddenSector != null)
			{
				ushort region = thisOveriddenSector.GetRegion(subtile);
				BlockBottomConnection(region, regionInProgress2);
			}
		}
	}

	private void ScanBottomRightCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
	{
		SubtileSector unfinishedSector = layers.GetUnfinishedSector(otherSectorCoords);
		int num = targetLayer.SectorSize - 1;
		Point subtile = new Point(num, num);
		ushort regionInProgress = thisSector.GetRegionInProgress(subtile);
		ushort regionInProgress2 = unfinishedSector.GetRegionInProgress(0, 0);
		if (regionInProgress2 > 0)
		{
			AddEdgesOrConnectors(regionInProgress2, regionInProgress);
			if (thisOveriddenSector != null)
			{
				ushort region = thisOveriddenSector.GetRegion(subtile);
				BlockBottomConnection(region, regionInProgress2);
			}
		}
	}

	private void ScanBottomLeftCornerOfSector(SubtileSector thisSector, SubtileSector thisOveriddenSector, Point otherSectorCoords)
	{
		SubtileSector unfinishedSector = layers.GetUnfinishedSector(otherSectorCoords);
		int num = targetLayer.SectorSize - 1;
		Point subtile = new Point(0, num);
		ushort regionInProgress = thisSector.GetRegionInProgress(subtile);
		ushort regionInProgress2 = unfinishedSector.GetRegionInProgress(num, 0);
		if (regionInProgress2 > 0)
		{
			AddEdgesOrConnectors(regionInProgress2, regionInProgress);
			if (thisOveriddenSector != null)
			{
				ushort region = thisOveriddenSector.GetRegion(subtile);
				BlockBottomConnection(region, regionInProgress2);
			}
		}
	}

	private void ScanVerticalSideForEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector, AdjacentSector adjacentSectorDir)
	{
		int relativeX;
		SubtileSector unfinishedSector;
		int relativeX2;
		if (adjacentSectorDir == AdjacentSector.Right)
		{
			relativeX = thisSector.SubtileArea.Width - 1;
			Point sectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y);
			unfinishedSector = layers.GetUnfinishedSector(sectorCoords);
			relativeX2 = 0;
		}
		else
		{
			relativeX = 0;
			Point sectorCoords2 = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y);
			unfinishedSector = layers.GetUnfinishedSector(sectorCoords2);
			relativeX2 = unfinishedSector.SubtileArea.Width - 1;
		}
		int num = thisSector.SubtileArea.Height - 1;
		for (int i = 0; i < num; i++)
		{
			_ = num - 1;
			ushort regionInProgress = thisSector.GetRegionInProgress(relativeX, i);
			ushort regionInProgress2 = thisSector.GetRegionInProgress(relativeX, i + 1);
			ushort regionInProgress3 = unfinishedSector.GetRegionInProgress(relativeX2, i);
			ushort regionInProgress4 = unfinishedSector.GetRegionInProgress(relativeX2, i + 1);
			if (thisOveriddenSector != null)
			{
				ushort region = thisOveriddenSector.GetRegion(relativeX, i);
				ushort region2 = thisOveriddenSector.GetRegion(relativeX, i + 1);
				BlockBottomConnection(region, regionInProgress3);
				BlockBottomConnection(region, regionInProgress4);
				BlockBottomConnection(region2, regionInProgress3);
			}
			AddEdgesOrConnectors(regionInProgress, regionInProgress3);
			AddEdgesOrConnectors(regionInProgress, regionInProgress4);
			AddEdgesOrConnectors(regionInProgress2, regionInProgress3);
		}
	}

	private void ScanHorizontalSideForEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector, AdjacentSector adjacentSectorDir)
	{
		int relativeY;
		SubtileSector unfinishedSector;
		int relativeY2;
		if (adjacentSectorDir == AdjacentSector.Under)
		{
			relativeY = thisSector.SubtileArea.Height - 1;
			Point sectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y + 1);
			unfinishedSector = layers.GetUnfinishedSector(sectorCoords);
			relativeY2 = 0;
		}
		else
		{
			relativeY = 0;
			Point sectorCoords2 = new Point(thisSector.Coords.X, thisSector.Coords.Y - 1);
			unfinishedSector = layers.GetUnfinishedSector(sectorCoords2);
			relativeY2 = unfinishedSector.SubtileArea.Height - 1;
		}
		int num = thisSector.SubtileArea.Width - 1;
		for (int i = 0; i < num; i++)
		{
			ushort regionInProgress = thisSector.GetRegionInProgress(i, relativeY);
			ushort regionInProgress2 = thisSector.GetRegionInProgress(i + 1, relativeY);
			ushort regionInProgress3 = unfinishedSector.GetRegionInProgress(i, relativeY2);
			ushort regionInProgress4 = unfinishedSector.GetRegionInProgress(i + 1, relativeY2);
			ushort? num2 = null;
			ushort? num3 = null;
			if (thisOveriddenSector != null)
			{
				num2 = thisOveriddenSector.GetRegion(i, relativeY);
				num3 = thisOveriddenSector.GetRegion(i + 1, relativeY);
				BlockBottomConnection(num2.Value, regionInProgress3);
				BlockBottomConnection(num2.Value, regionInProgress4);
				BlockBottomConnection(num3.Value, regionInProgress3);
			}
			AddEdgesOrConnectors(regionInProgress, regionInProgress3);
			AddEdgesOrConnectors(regionInProgress, regionInProgress4);
			AddEdgesOrConnectors(regionInProgress2, regionInProgress3);
		}
	}

	protected virtual void BlockBottomConnection(ushort overriddenRegion, ushort otherRegion)
	{
	}

	protected virtual void AddEdgesOrConnectors(ushort region1, ushort region2)
	{
	}

	protected bool SectorHasBeenScanned(Point sectorCoords)
	{
		if (sectorCoords.X < 0 || sectorCoords.Y < 0 || sectorCoords.X >= targetLayer.SectorsAcrossWidth || sectorCoords.Y >= targetLayer.SectorsAcrossHeight)
		{
			return true;
		}
		return scannedSectors.Contains(sectorCoords);
	}

	protected virtual bool RemoveAllEdgesBetweenLayers()
	{
		return true;
	}

	protected bool BuildInternalEdgesForDirtySectors()
	{
		if (dirtySectors.Count > 0)
		{
			GetCurrentDirtySectorAndIncrement(out var isLastSector).BuildRegionGraph(this);
			return isLastSector;
		}
		return true;
	}

	protected abstract bool BuildRegionGraphBetweenSectors();

	protected void ScanSectorEdges(SubtileSector thisSector, SubtileSector thisOveriddenSector)
	{
		Point sectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanVerticalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Left);
		}
		sectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanVerticalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Right);
		}
		sectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y - 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanHorizontalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Over);
		}
		sectorCoords = new Point(thisSector.Coords.X, thisSector.Coords.Y + 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanHorizontalSideForEdges(thisSector, thisOveriddenSector, AdjacentSector.Under);
		}
		sectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y - 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanTopLeftCornerOfSector(thisSector, thisOveriddenSector, sectorCoords);
		}
		sectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y - 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanTopRightCornerOfSector(thisSector, thisOveriddenSector, sectorCoords);
		}
		sectorCoords = new Point(thisSector.Coords.X - 1, thisSector.Coords.Y + 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanBottomLeftCornerOfSector(thisSector, thisOveriddenSector, sectorCoords);
		}
		sectorCoords = new Point(thisSector.Coords.X + 1, thisSector.Coords.Y + 1);
		if (!SectorHasBeenScanned(sectorCoords))
		{
			ScanBottomRightCornerOfSector(thisSector, thisOveriddenSector, sectorCoords);
		}
		scannedSectors.Add(thisSector.Coords);
	}

	private bool IsDependentRegion(ushort region)
	{
		return region > ColorStartOfRange;
	}

	public void AddEdgesIfNotExists(ushort fromRegionColor, ushort toRegionColor)
	{
		if (fromRegionColor == 0 || toRegionColor == 0 || fromRegionColor == toRegionColor)
		{
			return;
		}
		Region region = newRegions[fromRegionColor];
		float num = Common.DistanceOctile(p2: newRegions[toRegionColor].CenterLocation, p1: region.CenterLocation);
		if (!newRegionGraph.TryGetValue(fromRegionColor, out var value))
		{
			AddEdge(newRegionGraph, fromRegionColor, toRegionColor, num);
		}
		else
		{
			if (value.ContainsKey(toRegionColor))
			{
				return;
			}
			value.Add(toRegionColor, new RegionEdge(fromRegionColor, toRegionColor, num, hasRoad: false));
		}
		if (!newRegionGraph.TryGetValue(toRegionColor, out value))
		{
			value = new Dictionary<ushort, RegionEdge>();
			newRegionGraph.Add(toRegionColor, value);
		}
		value.Add(fromRegionColor, new RegionEdge(toRegionColor, fromRegionColor, num, hasRoad: false));
	}

	protected static float AddEdge(Dictionary<ushort, Dictionary<ushort, RegionEdge>> regionGraph, ushort fromRegion, ushort toRegion, float distanceBetweenRegionCenters)
	{
		Dictionary<ushort, RegionEdge> dictionary = new Dictionary<ushort, RegionEdge>();
		regionGraph.Add(fromRegion, dictionary);
		dictionary.Add(toRegion, new RegionEdge(fromRegion, toRegion, distanceBetweenRegionCenters, hasRoad: false));
		return distanceBetweenRegionCenters;
	}

	public void AddLog(string text)
	{
	}

	private bool Intersects(Rectangle r1, Rectangle r2)
	{
		if (r2.Left <= r1.Left + r1.Width && r2.Left + r2.Width >= r1.Left && r2.Top <= r1.Top + r1.Height)
		{
			return r2.Top + r2.Height >= r1.Top;
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

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		regions = sn.DoDictionary(regions);
		IsPaused = sn.DoBool(IsPaused);
		IDName = sn.DoString(IDName);
		RegionGraph = sn.DoNestedDictionary(RegionGraph);
		AllRegionCosts = sn.DoNestedDictionary(AllRegionCosts);
		cachedClosestRegionToBlockedSubtile = sn.DoDictionary(cachedClosestRegionToBlockedSubtile);
		RegionPaths = sn.DoNestedDictionary(RegionPaths);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			dirtySectorsRollbackAfterSave = new List<Point>(dirtySectors);
		}
		dirtySectorsRollbackAfterSave = sn.DoList(dirtySectorsRollbackAfterSave);
		sectorsAreDirty = sn.DoBool(sectorsAreDirty);
		regionSearchRequests = sn.DoList(regionSearchRequests);
		regionBFSPlanners = sn.DoDictionary(regionBFSPlanners);
		regionAStarPlanners = sn.DoNestedDictionary(regionAStarPlanners);
		snapshotTargetLayer = sn.SnapshotID<SubtileLayer, SubtileLayerID>(targetLayer).Value;
		subtileMapWidth = sn.DoInt32(subtileMapWidth);
		subtileMapHeight = sn.DoInt32(subtileMapHeight);
		snapshotLayers = sn.SnapshotID<SubtileLayers, SubtileLayersID>(layers).Value;
		sn.Ignore(dirtySectors);
		sn.Ignore(layers);
		sn.Ignore(progress);
		sn.Ignore(isComputing);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(newRegionGraph);
		sn.Ignore(regionGraphKeyListForCopying);
		sn.Ignore(regionGraphCopyProgressIndex);
		sn.Ignore(newRegions);
		sn.Ignore(timeTaken);
		sn.Ignore(scannedSectors);
		sn.Ignore(UnusedRegionColors);
		sn.Ignore(regionColorCounter);
		sn.Ignore(cyclesToBuildGraph);
		sn.Ignore(cyclesToFillRegions);
		sn.Ignore(FloodFill);
		sn.Ignore(currentSectorIndex);
		sn.Ignore(regionsToRemove);
		sn.Ignore(Log);
		sn.Ignore(timer);
		return this;
	}

	private void ResetColorCounter()
	{
		if (regions.Count > 0)
		{
			ushort num = regions.Max((KeyValuePair<ushort, Region> k) => k.Key);
			regionColorCounter = (ushort)(num + 1);
		}
		else
		{
			regionColorCounter = ColorStartOfRange;
		}
		AddLog("Color counter was reset to: " + regionColorCounter);
	}

	private void GetUnusedColors()
	{
		List<ushort> list = (from c in regions.Keys.ToList()
			orderby c
			select c).ToList();
		ushort? num = null;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			ushort num3 = list[num2];
			if (num.HasValue && num3 > num + 1)
			{
				for (int num4 = num.Value + 1; num4 < num3; num4++)
				{
					UnusedRegionColors.Enqueue((ushort)num4);
				}
			}
			num = num3;
		}
		AddLog("Gathered unused colors, no.: " + UnusedRegionColors.Count);
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		targetLayer = LookUp<SubtileLayer, SubtileLayerID>.FindByID(snapshotTargetLayer);
		layers = LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotLayers);
		foreach (Point item in dirtySectorsRollbackAfterSave)
		{
			targetLayer.Sectors[item.X][item.Y].BlockedStatusHasChanged = true;
		}
		if (dirtySectorsRollbackAfterSave.Count > 0)
		{
			sectorsAreDirty = true;
		}
		Init();
		foreach (KeyValuePair<ushort, Region> region in regions)
		{
			region.Value.LoadPostProcess(sn);
		}
		foreach (KeyValuePair<ushort, Dictionary<ushort, RegionEdge>> item2 in RegionGraph)
		{
			foreach (KeyValuePair<ushort, RegionEdge> item3 in item2.Value)
			{
				item3.Value.LoadPostProcess(sn);
			}
		}
		ResetColorCounter();
		GetUnusedColors();
		InitCopyVariablesRegions();
		bool flag = false;
		int num = 0;
		List<ushort> list = null;
		while (!InitCopyVariablesRegionGraph(RegionGraph, ref newRegionGraph, ref num, ref list))
		{
		}
		if (RegionGraph.Count == 0 && regions.Count == 0)
		{
			progress = Progress.InitFromScratch;
		}
		else
		{
			SetProgressAtRepairStart();
		}
		timer = new HighResolutionTime();
	}
}
