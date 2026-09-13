using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps.Regions;

public class DependentRegionMap : RegionMap, IIDEventSubscriber
{
	private TerrainRegionMap terrainRegionMap;

	private CyclableID snapshotTerrainRegionMap;

	private MethodID onTerrainRegionsAreInvalidID;

	private MethodID onTerrainRegionsAreDoneID;

	private CyclableID parentMovementMap;

	private Dictionary<ushort, List<ushort>> bottomToUpperLayerConnectors = new Dictionary<ushort, List<ushort>>();

	private Dictionary<ushort, HashSet<ushort>> bottomBlockedEdges = new Dictionary<ushort, HashSet<ushort>>();

	private Dictionary<ushort, HashSet<ushort>> newBottomBlockedEdges = new Dictionary<ushort, HashSet<ushort>>();

	private HashSet<ushort> regionColorsToRemove = new HashSet<ushort>();

	private HashSet<ushort> regionColorsToRemoveRollbackAfterSave = new HashSet<ushort>();

	private bool waitForRebuildAfterRemovedSector;

	private bool isWaitingForTerrainRegions;

	public override Dictionary<ushort, HashSet<ushort>> BottomBlockedEdges => bottomBlockedEdges;

	public override Dictionary<ushort, Region> BottomRegions => terrainRegionMap.BottomRegions;

	public override Dictionary<ushort, Dictionary<ushort, RegionEdge>> BottomRegionGraph => terrainRegionMap.RegionGraph;

	protected override ushort ColorStartOfRange => 40000;

	protected override bool CanServiceRequests
	{
		get
		{
			if (!isWaitingForTerrainRegions)
			{
				return !waitForRebuildAfterRemovedSector;
			}
			return false;
		}
	}

	public DependentRegionMap()
	{
	}

	public DependentRegionMap(MovementMap moveMap, SubtileLayers layers, TerrainRegionMap terrainRegionMap)
		: base(layers, layers.Layers[1])
	{
		parentMovementMap = moveMap.ID;
		this.terrainRegionMap = terrainRegionMap;
		this.terrainRegionMap.RegionsAreInvalid.AddAndRegister((Action)OnTerrainRegionsAreInvalid, (IIDEventSubscriber)this, out onTerrainRegionsAreInvalidID);
		this.terrainRegionMap.RegionsFinished.AddAndRegister((Action)OnTerrainRegionsAreDone, (IIDEventSubscriber)this, out onTerrainRegionsAreDoneID);
	}

	public void SetRegionsFromRemovedSectors(HashSet<ushort> regionColorsToRemove)
	{
		foreach (ushort item in regionColorsToRemove)
		{
			this.regionColorsToRemove.Add(item);
		}
		regionColorsToRemoveRollbackAfterSave = new HashSet<ushort>(this.regionColorsToRemove);
		CancelAllSearches();
		cachedClosestRegionToBlockedSubtile.Clear();
		waitForRebuildAfterRemovedSector = true;
		sectorsAreDirty = true;
	}

	protected override void RemoveRegions()
	{
		base.RemoveRegions();
		if (regionColorsToRemove == null || regionColorsToRemove.Count <= 0)
		{
			return;
		}
		foreach (ushort item in regionColorsToRemove)
		{
			RemoveRegion(newRegions[item]);
		}
		AddLog("Removed discarded sector regions: " + regionColorsToRemove.Count);
		regionColorsToRemove.Clear();
	}

	public void OnTerrainRegionsAreInvalid()
	{
		if (IsComputingLayerEdges())
		{
			AddLog("OnTerrainRegionsAreInvalid while computing edges, IsPaused = true, isWaitingForTerrainRegions = true");
			ResetEdgeBuilding();
			base.IsPaused = true;
		}
		else
		{
			AddLog("OnTerrainRegionsAreInvalid before computing edges, isWaitingForTerrainRegions = true");
		}
		CancelAllSearches();
		cachedClosestRegionToBlockedSubtile.Clear();
		isWaitingForTerrainRegions = true;
	}

	public void OnTerrainRegionsAreDone()
	{
		bool isPaused = base.IsPaused;
		base.IsPaused = false;
		((MovementMap)LookUp<ICyclable, CyclableID>.FindByID(parentMovementMap)).NotifyRegionMapUnpaused();
		if (sectorsAreDirty)
		{
			AddLog($"OnTerrainRegionsAreDone, Case #1 ({progress}), new progress = InitCleanupUnneededSectors");
			foreach (Point dirtySector in dirtySectors)
			{
				targetLayer.Sectors[dirtySector.X][dirtySector.Y].BlockedStatusHasChanged = true;
			}
			SetProgressAtRepairStart();
		}
		else if (progress == Progress.InitCleanupUnneededSectors)
		{
			AddLog($"OnTerrainRegionsAreDone, Case #2 ({progress}), new progress = RemoveEdgesBetweenLayers");
			progress = Progress.RemoveEdgesBetweenLayers;
			currentSectorIndex = 0;
		}
		else if (ProgressIsBeforeRemoveEdgesBetweenLayers())
		{
			AddLog($"OnTerrainRegionsAreDone, Case #3 ({progress})");
		}
		else if (progress == Progress.RemoveEdgesBetweenLayers)
		{
			AddLog($"OnTerrainRegionsAreDone, Case #4 (RemoveEdgesBetweenLayers), WasPaused: {isPaused}, IsPaused = false");
		}
		else if (progress == Progress.BuildRegionGraphBetweenSectors)
		{
			AddLog("OnTerrainRegionsAreDone, Case #5 (BuildRegionGraphBetweenSectors), new progress = RemoveEdgesBetweenLayers");
			progress = Progress.RemoveEdgesBetweenLayers;
			currentSectorIndex = 0;
		}
		else
		{
			AddLog($"OnTerrainRegionsAreDone, Case??? ({progress})");
		}
	}

	private bool ProgressIsBeforeRemoveEdgesBetweenLayers()
	{
		return progress < Progress.RemoveEdgesBetweenLayers;
	}

	public override RegionPathID? GetRegionPath(Point startSubtile, Point destinationSubtile)
	{
		if (SameDataAsTerrain())
		{
			return terrainRegionMap.GetRegionPath(startSubtile, destinationSubtile);
		}
		return base.GetRegionPath(startSubtile, destinationSubtile);
	}

	private bool SameDataAsTerrain()
	{
		if (regions.Count == 0 && RegionGraph.Count == 0 && AllRegionCosts.Count == 0)
		{
			return true;
		}
		return false;
	}

	public override Result GetDistance(Entity entity, Point fromSubtile, Point toSubtile, ref float distance, bool sendMessageToEntity = true, MethodID? notifyWhenFinished = null, bool registerIfNotReady = true)
	{
		if (SameDataAsTerrain())
		{
			return terrainRegionMap.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity, notifyWhenFinished, registerIfNotReady);
		}
		return base.GetDistance(entity, fromSubtile, toSubtile, ref distance, sendMessageToEntity, notifyWhenFinished, registerIfNotReady);
	}

	private void ResetEdgeBuilding()
	{
		progress = Progress.RemoveEdgesBetweenLayers;
		currentSectorIndex = 0;
	}

	public void ResetComputePointIfNeeded()
	{
		if (progress == Progress.RemoveEdgesBetweenLayers && currentSectorIndex == 0 && sectorsAreDirty)
		{
			SetProgressAtRepairStart();
		}
	}

	public override void Destroy()
	{
		terrainRegionMap.RegionsFinished.Remove(onTerrainRegionsAreDoneID);
		terrainRegionMap.RegionsAreInvalid.Remove(onTerrainRegionsAreInvalidID);
		base.Destroy();
	}

	protected override void RecomputeFinished()
	{
		base.RecomputeFinished();
		bottomBlockedEdges = new Dictionary<ushort, HashSet<ushort>>(newBottomBlockedEdges);
		if (regionColorsToRemoveRollbackAfterSave != null)
		{
			regionColorsToRemoveRollbackAfterSave.Clear();
		}
		AddLog("Recompute ended, isWaitingForTerrainRegions = false, progress = " + progress);
		isWaitingForTerrainRegions = false;
		waitForRebuildAfterRemovedSector = false;
	}

	protected override void AssertRegionGraph(bool useFieldsInProgress = true)
	{
	}

	protected override void StartBuildingGraphBetweenSectors()
	{
		if (terrainRegionMap.IsComputing)
		{
			AddLog("StartBuildingGraph, IsPaused = true");
			base.IsPaused = true;
		}
	}

	private bool IsComputingLayerEdges()
	{
		Progress progress = base.progress;
		if (progress == Progress.RemoveEdgesBetweenLayers || progress == Progress.BuildRegionGraphBetweenSectors)
		{
			return true;
		}
		return false;
	}

	public override Region GetRegion(ushort color)
	{
		if (color >= 40000)
		{
			return regions[color];
		}
		if (color > 0)
		{
			return terrainRegionMap.GetRegion(color);
		}
		return null;
	}

	public override Region GetRegionInProgress(ushort color)
	{
		if (color >= 40000)
		{
			return newRegions[color];
		}
		if (color > 0)
		{
			return terrainRegionMap.GetRegionInProgress(color);
		}
		return null;
	}

	private void AddConnectors(ushort fromRegion, ushort toRegion)
	{
		ushort num = Math.Min(fromRegion, toRegion);
		ushort num2 = Math.Max(fromRegion, toRegion);
		if (num != 0 && num2 != 0)
		{
			AddLayerConnector(num, num2);
		}
	}

	private void AddLayerConnector(ushort bottomRegionColor, ushort topRegionColor)
	{
		Region region = terrainRegionMap.BottomRegions[bottomRegionColor];
		float num = Common.DistanceOctile(p2: newRegions[topRegionColor].CenterLocation, p1: region.CenterLocation);
		if (!newRegionGraph.TryGetValue(bottomRegionColor, out var value))
		{
			RegionMap.AddEdge(newRegionGraph, bottomRegionColor, topRegionColor, num);
		}
		else
		{
			if (value.ContainsKey(topRegionColor))
			{
				return;
			}
			value.Add(topRegionColor, new RegionEdge(bottomRegionColor, topRegionColor, num, hasRoad: false));
		}
		Common.AddToMultiList(bottomToUpperLayerConnectors, bottomRegionColor, topRegionColor);
		if (!newRegionGraph.TryGetValue(topRegionColor, out value))
		{
			value = new Dictionary<ushort, RegionEdge>();
			newRegionGraph.Add(topRegionColor, value);
		}
		value.Add(bottomRegionColor, new RegionEdge(topRegionColor, bottomRegionColor, num, hasRoad: false));
	}

	protected override bool RemoveAllEdgesBetweenLayers()
	{
		foreach (KeyValuePair<ushort, List<ushort>> bottomToUpperLayerConnector in bottomToUpperLayerConnectors)
		{
			if (!newRegionGraph.TryGetValue(bottomToUpperLayerConnector.Key, out var value))
			{
				continue;
			}
			foreach (ushort item in bottomToUpperLayerConnector.Value)
			{
				value.Remove(item);
				if (newRegionGraph.TryGetValue(item, out var value2))
				{
					value2.Remove(bottomToUpperLayerConnector.Key);
				}
			}
		}
		bottomToUpperLayerConnectors.Clear();
		newBottomBlockedEdges.Clear();
		return true;
	}

	protected override bool BuildRegionGraphBetweenSectors()
	{
		if (base.ID == (CyclableID)202uL)
		{
			_ = The.Sim.TotalUnPausedGameTimeInSeconds;
			_ = 15.0;
		}
		bool isLastSector;
		SubtileSector currentSector = GetCurrentSector(out isLastSector);
		if (currentSector != null)
		{
			SubtileSector overriddenSector = layers.GetOverriddenSector(currentSector.Coords.X, currentSector.Coords.Y);
			ScanSectorEdges(currentSector, overriddenSector);
		}
		return isLastSector;
	}

	protected override void BlockBottomConnection(ushort overriddenRegion, ushort otherRegion)
	{
		if (overriddenRegion != 0 && otherRegion != 0)
		{
			Common.AddToMultiList(newBottomBlockedEdges, overriddenRegion, otherRegion);
			Common.AddToMultiList(newBottomBlockedEdges, otherRegion, overriddenRegion);
		}
	}

	protected override void AddEdgesOrConnectors(ushort fromRegion, ushort toRegion)
	{
		if (fromRegion >= ColorStartOfRange && toRegion >= ColorStartOfRange)
		{
			AddEdgesIfNotExists(fromRegion, toRegion);
		}
		else
		{
			AddConnectors(fromRegion, toRegion);
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		parentMovementMap = sn.DoEnum(parentMovementMap);
		onTerrainRegionsAreInvalidID = sn.DoEnum(onTerrainRegionsAreInvalidID);
		onTerrainRegionsAreDoneID = sn.DoEnum(onTerrainRegionsAreDoneID);
		bottomBlockedEdges = sn.DoMultiMapHashSet(bottomBlockedEdges);
		bottomToUpperLayerConnectors = sn.DoMultiMap(bottomToUpperLayerConnectors);
		isWaitingForTerrainRegions = sn.DoBool(isWaitingForTerrainRegions);
		regionColorsToRemoveRollbackAfterSave = sn.DoHashSet(regionColorsToRemoveRollbackAfterSave);
		waitForRebuildAfterRemovedSector = sn.DoBool(waitForRebuildAfterRemovedSector);
		snapshotTerrainRegionMap = sn.SnapshotID<ICyclable, CyclableID>(terrainRegionMap).Value;
		sn.Ignore(terrainRegionMap);
		sn.Ignore(regionColorsToRemove);
		sn.Ignore(newBottomBlockedEdges);
		sn.Ignore(RegionMap.totalComputationAllInstancesInSeconds);
		sn.Ignore(base.StartedOnTimeInSeconds);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		sn.RegisterLoadPostProcessCall(this);
		terrainRegionMap = (TerrainRegionMap)LookUp<ICyclable, CyclableID>.FindByID(snapshotTerrainRegionMap);
		if (regionColorsToRemoveRollbackAfterSave != null && regionColorsToRemoveRollbackAfterSave.Count > 0)
		{
			sectorsAreDirty = true;
			regionColorsToRemove = new HashSet<ushort>(regionColorsToRemoveRollbackAfterSave);
		}
		LoadPostProcessRegisterMethodIDs();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup.Add(onTerrainRegionsAreInvalidID, OnTerrainRegionsAreInvalid);
		ActionLookup.Add(onTerrainRegionsAreDoneID, OnTerrainRegionsAreDone);
	}
}
