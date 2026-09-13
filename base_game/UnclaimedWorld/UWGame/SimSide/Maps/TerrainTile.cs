using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Maps;

public class TerrainTile : IDrawnAsGroundSprite, ISnapshot, ILookUp<TerrainTile, TerrainTileID>
{
	public enum DeprecateDistance
	{
		Near,
		Far
	}

	private enum TreatStructuresOrTerrain
	{
		Structure,
		Terrain
	}

	public TilePos TilePos;

	public Expedition OperatingAreaOf;

	private ExpeditionID? snapshotOperatingAreaOf;

	public EntityGroupID? Owner;

	public TerrainLevel Level = TerrainLevel.Middle;

	public SurfaceType TerrainType = PlainsType.Instance;

	public Resource[] DesignerPlacedResources;

	public Dictionary<ResourceType, TileResourceContainer> TileResources;

	private Dictionary<string, ResourceID> snapshotTileResources;

	public Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>> HarvestJobs;

	private Dictionary<EntityGroupID, Dictionary<ResourceType, List<JobID>>> snapshotHarvestJobs;

	public HashSet<Allegiance> AllegiancesThatSeeThisTile = new HashSet<Allegiance>();

	private List<AllegianceID> snapshotAllegiancesThatSeeThisTile;

	public HashSet<Entity> EntitiesThatSeeThisTile = new HashSet<Entity>();

	private List<EntityID> snapshotEntitiesThatSeeThisTile;

	private List<RoadAndPathQuad> renderedRoads;

	private bool roadConnectionsAreDirty = true;

	public Terrain Terrain;

	private TerrainID? snapshotTerrain;

	public Terrain[][] TerrainSubtiles;

	private TerrainID[][] snapshotTerrainSubtiles;

	private float moisture = 0.3f;

	public Dictionary<Allegiance, List<Zone>> Zones;

	private Dictionary<AllegianceID, List<ZoneID>> snapshotZones;

	public const float FreezingPointInKelvin = 273.15f;

	private const float WaterAmountToConsiderSoilFullyFlooded = 10000f;

	public float Humidity = 0.2f;

	private float waterAmount;

	public Vector4 ColorOfWater = new Vector4(0.3f, 0.3f, 0.5f, 1f);

	public Vector4 WaterBottomTint = Vector4.One;

	public float Temperature;

	public List<Entity> EdgeLayoutEntities;

	public Entity[] Roads;

	public Entity[] WheelPaths;

	private Entity[] FootPaths;

	public List<Entity> EntitiesOnTile;

	private List<EntityID> snapshotEntitiesOnTile;

	public List<SimProcessID> ProcessesOnTile;

	public List<Renderable> RenderablesOnTile;

	public Dictionary<SharedKnowledge, List<MemoryFact>> RememberedRootEntitiesOnTile;

	private Dictionary<AllegianceID, List<MemoryFactID>> snapshotRememberedEntitiesOnTile;

	public Dictionary<SharedKnowledge, List<ProcessMemory>> RememberedProcessesOnTile;

	private Dictionary<AllegianceID, List<ProcessMemoryID>> snapshotRememberedProcessesOnTile;

	public List<EntityID> GeoLayoutEntitiesOnTile;

	public List<Entity> BaseCenterForMultiTileEntities;

	private List<EntityID> snapshotBaseCenterEntities;

	public List<Entity> TreesOnTile;

	private List<EntityID> snapshotTreesOnTile;

	private TerrainTileID id = TerrainTileID.Invalid;

	private static TerrainTileID IDCounter = TerrainTileID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int X => TilePos.X;

	public int Y => TilePos.Y;

	public float Moisture
	{
		get
		{
			return moisture;
		}
		set
		{
			moisture = value;
		}
	}

	public float WaterAmount
	{
		get
		{
			return waterAmount;
		}
		set
		{
			waterAmount = value;
			ComputeMoistureLevelFromWaterAmount();
		}
	}

	public bool HasEverBeenSeenByPlayer { get; set; }

	public TerrainTileID ID
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

	public float GetTotalBulkOfItems()
	{
		float num = 0f;
		if (EntitiesOnTile != null)
		{
			foreach (Entity item in EntitiesOnTile)
			{
				if (item.EntityType.ItemType != null)
				{
					num += item.Bulk;
				}
			}
		}
		return num;
	}

	public void UpdateSimulationInParallel(double deltaTimeInSeconds)
	{
		if (TreesOnTile == null)
		{
			return;
		}
		float nitrogen = 0f;
		float phosphorous = 0f;
		float water = 0f;
		List<PlantResourceNeeds> list = new List<PlantResourceNeeds>();
		foreach (Entity item in TreesOnTile)
		{
			item.Find<UWGame.SimSide.Trees.Tree>(out var c);
			c.GetGrowthNeeds(deltaTimeInSeconds, out water, out nitrogen, out phosphorous);
			list.Add(new PlantResourceNeeds
			{
				Tree = item,
				Nitrogen = nitrogen,
				Water = water,
				Phosphorous = phosphorous
			});
		}
	}

	private void ComputeMoistureLevelFromWaterAmount()
	{
		Moisture = (float)Math.Pow(WaterAmount / 10000f, 0.5);
		Moisture = Common.ClampTop(Moisture, 1f);
	}

	private float ComputeDryingFactor()
	{
		float num = Common.ClampBottom(Temperature - 273.15f, 0f) / 100f;
		float num2 = 0.5f - Moisture;
		float num3 = 0.2f - Humidity;
		float num4 = Common.ClampTop(The.Sim.PlaySite.PlaySite.Weather.WindSpeed / 10f, 2f);
		return Common.ClampBottom(num + num2 + num3 + num4, 0f);
	}

	private float ComputeRottingFactor()
	{
		float num = Temperature - 273.15f;
		float num2 = Common.Clamp((0f - num * num + 64f * num + 300f) / 1400f, 0f, 1f);
		float num3 = Moisture;
		float humidity = Humidity;
		return num2 + num3 + humidity;
	}

	public bool TileIsInFogOfWar(Allegiance allegiance)
	{
		return !AllegiancesThatSeeThisTile.Contains(allegiance);
	}

	public void DeleteMemoryOfDestroyedEntity(Entity entity)
	{
		foreach (Allegiance item in AllegiancesThatSeeThisTile)
		{
			item.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, removeAllKnowledge: true);
		}
	}

	public void DeleteMemoryOfEntityMovingOffSite(Entity entity)
	{
		foreach (Allegiance item in AllegiancesThatSeeThisTile)
		{
			item.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, removeAllKnowledge: true);
		}
	}

	public void ChangeTileSeenBy(Entity byEntity, Sensor.TileStatus tileStatus)
	{
		Allegiance allegiance = byEntity.Intelligence.Allegiance;
		if (tileStatus == Sensor.TileStatus.Seen)
		{
			if (allegiance.AllegianceType == AllegianceType.Player)
			{
				HasEverBeenSeenByPlayer = true;
			}
			EntitiesThatSeeThisTile.Add(byEntity);
			if (allegiance != null)
			{
				SeeEntitiesOnTile(byEntity, allegiance.SharedKnowledge);
				SeeAllProcessesOnTile(allegiance.SharedKnowledge);
			}
			AllegiancesThatSeeThisTile.Add(allegiance);
			DeprecateMemoryFactsOnTile(byEntity, DeprecateDistance.Far);
			return;
		}
		EntitiesThatSeeThisTile.Remove(byEntity);
		if (allegiance == null)
		{
			return;
		}
		bool flag = false;
		foreach (Entity item in EntitiesThatSeeThisTile)
		{
			if (item.Intelligence.Allegiance == allegiance)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		AllegiancesThatSeeThisTile.Remove(allegiance);
		if (EntitiesOnTile != null)
		{
			foreach (Entity item2 in EntitiesOnTile)
			{
				allegiance.SharedKnowledge.UnSeeEntity(item2);
			}
		}
		if (ProcessesOnTile == null)
		{
			return;
		}
		for (int num = ProcessesOnTile.Count - 1; num >= 0; num--)
		{
			SimProcess simProcess = LookUp<SimProcess, SimProcessID>.FindByID(ProcessesOnTile[num]);
			if (simProcess != null)
			{
				allegiance.SharedKnowledge.PlaySiteKnowledge.UnSeeProcess(simProcess);
			}
			else
			{
				ProcessesOnTile.RemoveAt(num);
			}
		}
	}

	private void SeeEntitiesOnTile(Entity byEntity, SharedKnowledge sharedKnowledge)
	{
		List<IDetectable> allDetectables = new List<IDetectable>();
		if (byEntity.Find<Sensor>(out var c))
		{
			GetDetectablesOnTile(allDetectables);
			c.RollToDetect(byEntity, sharedKnowledge, allDetectables, suppressClientFeedbackAndEvents: false, detectAllWhichHasMinimumRange: false, unseeAfterDetecting: false, rollToDetectHiddenEntities: true, doAssert: false);
		}
	}

	private void SeeAllProcessesOnTile(SharedKnowledge sharedKnowledge)
	{
		if (ProcessesOnTile == null)
		{
			return;
		}
		for (int num = ProcessesOnTile.Count - 1; num >= 0; num--)
		{
			SimProcess simProcess = SimProcess.FindById(ProcessesOnTile[num]);
			if (simProcess != null)
			{
				sharedKnowledge.PlaySiteKnowledge.SeeProcess(simProcess);
			}
			else
			{
				ProcessesOnTile.RemoveAt(num);
			}
		}
	}

	public void GetDetectablesOnTile(List<IDetectable> allDetectables)
	{
		if (EntitiesOnTile != null)
		{
			allDetectables.AddRange(EntitiesOnTile);
		}
		if (TileResources != null)
		{
			foreach (TileResourceContainer value in TileResources.Values)
			{
				if (value.ResourceItems.Count > 0)
				{
					allDetectables.Add(value);
				}
			}
		}
		if (TreesOnTile == null)
		{
			return;
		}
		foreach (Entity item in TreesOnTile)
		{
			if (item.EntityType.TreeType == null || item.EntityType.TreeType.CropTypes == null)
			{
				continue;
			}
			item.Find<UWGame.SimSide.Trees.Tree>(out var c);
			foreach (KeyValuePair<ResourceType, Crop> crop in c.Crops)
			{
				if (crop.Value.ResourceItems.Count > 0)
				{
					allDetectables.Add(crop.Value);
				}
			}
		}
	}

	public void AddToFootPath(Common.Direction dir)
	{
		AddToFootPath(dir, GameData.Instance.Constants.AmountToAddToPathOnTraversal);
	}

	public void AddToFootPath(Common.Direction dir, float increment)
	{
		if (FootPaths == null)
		{
			FootPaths = new Entity[8];
		}
		if (FootPaths[(int)dir] == null)
		{
			FootPaths[(int)dir] = new Entity(GameData.Instance.AllEntityTypes["footpath"]);
		}
		FootPaths[(int)dir].Find<TerrainPath>(out var c);
		float value = c.Value;
		c.Value = Common.ClampTop(c.Value + increment, 1f);
		float pathActivation = GameData.Instance.Constants.PathActivation;
		if ((value <= pathActivation && c.Value > pathActivation) || (value > pathActivation && c.Value <= pathActivation))
		{
			RedrawTerrainCostsAroundEdge(dir);
		}
	}

	public void AddToWheelPath(Common.Direction dir, float increment)
	{
		if (WheelPaths == null)
		{
			WheelPaths = new Entity[8];
		}
		if (WheelPaths[(int)dir] == null)
		{
			WheelPaths[(int)dir] = new Entity(GameData.Instance.AllEntityTypes["tracks"]);
		}
		TerrainPath terrainPath = WheelPaths[(int)dir].TerrainPath;
		float value = terrainPath.Value;
		terrainPath.Value = Common.ClampTop(terrainPath.Value + increment, 1f);
		float pathActivation = GameData.Instance.Constants.PathActivation;
		if ((value <= pathActivation && terrainPath.Value > pathActivation) || (value > pathActivation && terrainPath.Value <= pathActivation))
		{
			RedrawTerrainCostsAroundEdge(dir);
		}
	}

	public void AddCenterForGeoLayoutEntities(Entity entity)
	{
		if (BaseCenterForMultiTileEntities == null)
		{
			BaseCenterForMultiTileEntities = new List<Entity>();
		}
		if (!BaseCenterForMultiTileEntities.Contains(entity))
		{
			BaseCenterForMultiTileEntities.Add(entity);
		}
	}

	public void RemoveCenterForGeoLayoutEntities(Entity entity)
	{
		if (BaseCenterForMultiTileEntities != null)
		{
			BaseCenterForMultiTileEntities.Remove(entity);
		}
	}

	public Entity GetMainBuildingOnTile()
	{
		return null;
	}

	public bool GetStructuresOnTile(ref List<Entity> listOfStructures)
	{
		if (GeoLayoutEntitiesOnTile != null)
		{
			for (int num = GeoLayoutEntitiesOnTile.Count - 1; num >= 0; num--)
			{
				Entity entity = Entity.FindByID(GeoLayoutEntitiesOnTile[num]);
				if (entity != null && entity.EntityType.StructureType != null)
				{
					Common.AddToList(ref listOfStructures, entity);
				}
				else
				{
					GeoLayoutEntitiesOnTile.RemoveAt(num);
				}
			}
		}
		if (listOfStructures != null)
		{
			return listOfStructures.Count > 0;
		}
		return false;
	}

	public bool ContainsTileEntity(Entity entity)
	{
		if (GeoLayoutEntitiesOnTile != null)
		{
			return GeoLayoutEntitiesOnTile.Contains(entity.ID);
		}
		return false;
	}

	public void RedrawTerrainCostsAroundEdge(Common.Direction edge)
	{
		foreach (Point neighboringTile in GetNeighboringTiles(new Point(X, Y), edge))
		{
			_ = neighboringTile;
		}
	}

	private void RedrawRoadCosts(Common.Direction dir)
	{
		if (Roads != null && Roads[(int)dir] != null && Roads[(int)dir].IsCompleted())
		{
			Roads[(int)dir].DirectionalLayout.RedrawRoadCost();
			return;
		}
		float pathActivation = GameData.Instance.Constants.PathActivation;
		if (WheelPaths != null && WheelPaths[(int)dir] != null && WheelPaths[(int)dir].TerrainPath.Value > pathActivation)
		{
			WheelPaths[(int)dir].DirectionalLayout.RedrawRoadCost();
		}
		else if (FootPaths != null && FootPaths[(int)dir] != null && FootPaths[(int)dir].TerrainPath.Value > pathActivation)
		{
			FootPaths[(int)dir].DirectionalLayout.RedrawRoadCost();
		}
		else
		{
			SetEdgeTerrainCost(GameData.Instance.AllEntityTypes["terrain:plains"].TerrainType.PathType, dir);
		}
	}

	private void SetEdgeTerrainCost(PathType pathType, Common.Direction dir)
	{
		Point point = MapManager.TileEdgeToSubtile(new Point(X, Y));
		Point subtilePosition = MapManager.DirectionToRelativeSubtile(dir);
		subtilePosition.X += point.X;
		subtilePosition.Y += point.Y;
		Point subtilePosition2 = new Point(point.X + 1, point.Y);
		foreach (SurfaceType.TransportType item in MapManager.MapTransportTypeArray)
		{
			byte newCost = pathType.TransportCosts[(uint)item];
			The.Map.SetSubtileCost(subtilePosition, item, newCost);
			The.Map.SetSubtileCost(subtilePosition2, item, newCost);
		}
	}

	public List<Zone> GetListOfZones(Allegiance allegiance)
	{
		List<Zone> value = null;
		if (Zones != null)
		{
			Zones.TryGetValue(allegiance, out value);
		}
		return value;
	}

	public bool IsInZone(Allegiance allegiance, Zone zone)
	{
		List<Zone> listOfZones = GetListOfZones(allegiance);
		if (listOfZones != null && listOfZones.Count > 0)
		{
			return listOfZones.Contains(zone);
		}
		return false;
	}

	public Zone GetStockpileZone(Allegiance allegiance)
	{
		return GetListOfZones(allegiance)?.Find((Zone z) => z.Stockpile != null);
	}

	public void AddZone(Allegiance allegiance, Zone zone)
	{
		List<Zone> value = null;
		if (Zones == null)
		{
			Zones = new Dictionary<Allegiance, List<Zone>>();
		}
		if (!Zones.TryGetValue(allegiance, out value))
		{
			value = new List<Zone>();
			Zones.Add(allegiance, value);
		}
		value.Add(zone);
	}

	public void RemoveZone(Allegiance allegiance, Zone zone)
	{
		if (Zones == null)
		{
			return;
		}
		List<Zone> value = null;
		if (Zones.TryGetValue(allegiance, out value))
		{
			value.Remove(zone);
			if (value.Count == 0)
			{
				Zones.Remove(allegiance);
			}
		}
	}

	public void RemoveZone(Zone zone)
	{
		if (Zones == null)
		{
			return;
		}
		using Dictionary<Allegiance, List<Zone>>.Enumerator enumerator = Zones.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(zone))
		{
		}
	}

	public List<Point> GetNeighboringTiles(Point tilePos, Common.Direction edge)
	{
		List<Point> list = new List<Point>();
		list.Add(tilePos);
		switch (edge)
		{
		case Common.Direction.East:
			AddEastTile(tilePos, list);
			break;
		case Common.Direction.North:
			AddNorthTile(tilePos, list);
			break;
		case Common.Direction.West:
			AddWestTile(tilePos, list);
			break;
		case Common.Direction.South:
			AddSouthTile(tilePos, list);
			break;
		case Common.Direction.NorthEast:
			AddNorthTile(tilePos, list);
			AddEastTile(tilePos, list);
			AddNeighbouringTile(tilePos, list, 1, -1);
			break;
		case Common.Direction.NorthWest:
			AddNorthTile(tilePos, list);
			AddWestTile(tilePos, list);
			AddNeighbouringTile(tilePos, list, -1, -1);
			break;
		case Common.Direction.SouthEast:
			AddSouthTile(tilePos, list);
			AddEastTile(tilePos, list);
			AddNeighbouringTile(tilePos, list, 1, 1);
			break;
		case Common.Direction.SouthWest:
			AddSouthTile(tilePos, list);
			AddWestTile(tilePos, list);
			AddNeighbouringTile(tilePos, list, -1, 1);
			break;
		}
		return list;
	}

	private static void AddNeighbouringTile(Point tilePos, List<Point> listOfNeighbours, int dX, int dY)
	{
		Point point = The.Map.ClampTileMapPosition(new Point(tilePos.X + dX, tilePos.Y + dY));
		if (tilePos != point)
		{
			listOfNeighbours.Add(point);
		}
	}

	private static void AddSouthTile(Point tilePos, List<Point> listOfNeighbours)
	{
		Point point = The.Map.ClampTileMapPosition(new Point(tilePos.X, tilePos.Y + 1));
		if (tilePos != point)
		{
			listOfNeighbours.Add(point);
		}
	}

	private static void AddWestTile(Point tilePos, List<Point> listOfNeighbours)
	{
		Point point = The.Map.ClampTileMapPosition(new Point(tilePos.X - 1, tilePos.Y));
		if (tilePos != point)
		{
			listOfNeighbours.Add(point);
		}
	}

	private static void AddNorthTile(Point tilePos, List<Point> listOfNeighbours)
	{
		Point point = The.Map.ClampTileMapPosition(new Point(tilePos.X, tilePos.Y - 1));
		if (tilePos != point)
		{
			listOfNeighbours.Add(point);
		}
	}

	private static void AddEastTile(Point tilePos, List<Point> listOfNeighbours)
	{
		Point point = The.Map.ClampTileMapPosition(new Point(tilePos.X + 1, tilePos.Y));
		if (tilePos != point)
		{
			listOfNeighbours.Add(point);
		}
	}

	public void DeprecateMemoryFactsOnTile(Entity detectingEntity, DeprecateDistance deprecateDistance)
	{
		if (RememberedRootEntitiesOnTile == null || RememberedRootEntitiesOnTile.Count <= 0)
		{
			return;
		}
		Allegiance allegiance = detectingEntity.Intelligence.Allegiance;
		if (!RememberedRootEntitiesOnTile.TryGetValue(allegiance.SharedKnowledge, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			MemoryFact memoryFact = value[num];
			bool flag = false;
			if (memoryFact.DeprecateIfNeeded(detectingEntity, deprecateDistance))
			{
				value.RemoveAt(num);
				flag = true;
			}
			DeprecateDistance? deprecateDistance2 = deprecateDistance;
			if (flag)
			{
				deprecateDistance2 = null;
			}
			allegiance.SharedKnowledge.DeprecateMemoryFactLeafs(detectingEntity, memoryFact.EntityID, deprecateDistance2);
		}
	}

	public bool HasPath(int directionIndex)
	{
		if (Roads != null && Roads[directionIndex] != null)
		{
			return true;
		}
		float pathActivation = GameData.Instance.Constants.PathActivation;
		if (WheelPaths != null && WheelPaths[directionIndex] != null && WheelPaths[directionIndex].TerrainPath.Value > pathActivation)
		{
			return true;
		}
		if (FootPaths != null && FootPaths[directionIndex] != null && FootPaths[directionIndex].TerrainPath.Value > pathActivation)
		{
			return true;
		}
		return false;
	}

	public SurfaceType.TerrainFeatures HighestRankedPathFeature(int directionIndex)
	{
		if (Roads != null && Roads[directionIndex] != null)
		{
			return SurfaceType.TerrainFeatures.GravelRoad;
		}
		float pathActivation = GameData.Instance.Constants.PathActivation;
		if (WheelPaths != null && WheelPaths[directionIndex] != null && WheelPaths[directionIndex].TerrainPath.Value > pathActivation)
		{
			return SurfaceType.TerrainFeatures.WheelPath;
		}
		if (FootPaths != null && FootPaths[directionIndex] != null && FootPaths[directionIndex].TerrainPath.Value > pathActivation)
		{
			return SurfaceType.TerrainFeatures.FootPath;
		}
		return SurfaceType.TerrainFeatures.None;
	}

	public void AddProcess(SimProcess process)
	{
		if (ProcessesOnTile == null)
		{
			ProcessesOnTile = new List<SimProcessID>();
		}
		if (!ProcessesOnTile.Contains(process.ID))
		{
			ProcessesOnTile.Add(process.ID);
		}
	}

	public void RemoveProcess(SimProcess process)
	{
		if (ProcessesOnTile != null)
		{
			ProcessesOnTile.Remove(process.ID);
		}
	}

	public void AddEntity(Entity entity)
	{
		if (EntitiesOnTile == null)
		{
			EntitiesOnTile = new List<Entity>();
		}
		if (!EntitiesOnTile.Contains(entity))
		{
			EntitiesOnTile.Add(entity);
		}
	}

	public void RemoveEntity(Entity entity)
	{
		if (EntitiesOnTile != null)
		{
			EntitiesOnTile.Remove(entity);
		}
	}

	public bool ContainsEntity(Entity entity)
	{
		if (EntitiesOnTile != null)
		{
			return EntitiesOnTile.Contains(entity);
		}
		return false;
	}

	public void AddRememberedProcess(SharedKnowledge sharedKnowledge, ProcessMemory memoryFact)
	{
		if (RememberedProcessesOnTile == null)
		{
			RememberedProcessesOnTile = new Dictionary<SharedKnowledge, List<ProcessMemory>>();
		}
		Common.AddToMultiList(RememberedProcessesOnTile, sharedKnowledge, memoryFact);
	}

	public void AddRememberedRootEntity(SharedKnowledge sharedKnowledge, MemoryFact memoryFact)
	{
		_ = memoryFact.EntityID;
		_ = 4852;
		if (RememberedRootEntitiesOnTile == null)
		{
			RememberedRootEntitiesOnTile = new Dictionary<SharedKnowledge, List<MemoryFact>>();
		}
		Common.AddToMultiList(RememberedRootEntitiesOnTile, sharedKnowledge, memoryFact);
	}

	public void RemoveRememberedRootEntity(SharedKnowledge sharedKnowledge, MemoryFact entity)
	{
		if (RememberedRootEntitiesOnTile != null && RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out var value))
		{
			value.Remove(entity);
			if (value.Count == 0)
			{
				RememberedRootEntitiesOnTile.Remove(sharedKnowledge);
			}
		}
	}

	public void RemoveRememberedProcess(SharedKnowledge sharedKnowledge, ProcessMemory process)
	{
		if (RememberedProcessesOnTile != null && RememberedProcessesOnTile.TryGetValue(sharedKnowledge, out var value))
		{
			value.Remove(process);
			if (value.Count == 0)
			{
				RememberedProcessesOnTile.Remove(sharedKnowledge);
			}
		}
	}

	public void AddGeoLayoutEntity(Entity entity)
	{
		if (GeoLayoutEntitiesOnTile == null)
		{
			GeoLayoutEntitiesOnTile = new List<EntityID>();
		}
		if (!GeoLayoutEntitiesOnTile.Contains(entity.ID))
		{
			GeoLayoutEntitiesOnTile.Add(entity.ID);
		}
	}

	public void RemoveGeoLayoutEntity(Entity entity)
	{
		if (GeoLayoutEntitiesOnTile != null)
		{
			GeoLayoutEntitiesOnTile.Remove(entity.ID);
		}
	}

	public bool HasGatherableResources()
	{
		if (TileResources != null)
		{
			foreach (KeyValuePair<ResourceType, TileResourceContainer> tileResource in TileResources)
			{
				if (tileResource.Value.NoOfHarvestableItems > 0)
				{
					return true;
				}
			}
		}
		if (TreesOnTile != null)
		{
			foreach (Entity item in TreesOnTile)
			{
				item.Find<UWGame.SimSide.Trees.Tree>(out var c);
				if (c.Crops == null)
				{
					continue;
				}
				foreach (KeyValuePair<ResourceType, Crop> crop in c.Crops)
				{
					if (crop.Value.NoOfHarvestableItems > 0)
					{
						return true;
					}
				}
			}
		}
		if (Terrain != null)
		{
			Terrain terrain = Terrain;
			if (TerrainHasGatherableResources(terrain))
			{
				return true;
			}
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Terrain terrain = TerrainSubtiles[i][j];
					if (TerrainHasGatherableResources(terrain))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool TerrainHasGatherableResources(Terrain terrain)
	{
		_ = terrain.Vegetation;
		return false;
	}

	public void AddEdgeStructure(Entity feature)
	{
		if (EdgeLayoutEntities == null)
		{
			EdgeLayoutEntities = new List<Entity>();
		}
		EdgeLayoutEntities.Add(feature);
	}

	public void RemoveEdgeStructure(Entity feature)
	{
		if (EdgeLayoutEntities != null)
		{
			EdgeLayoutEntities.Remove(feature);
		}
	}

	public void AddRoad(Entity road, Common.Direction dir)
	{
		if (Roads == null)
		{
			Roads = new Entity[8];
		}
		Roads[(int)dir] = road;
		roadConnectionsAreDirty = true;
		RedrawTerrainCostsAroundEdge(dir);
	}

	public Terrain GetTerrain(Point subtilePos)
	{
		if (Terrain != null)
		{
			return Terrain;
		}
		return TerrainSubtiles[subtilePos.X % 3][subtilePos.Y % 3];
	}

	public void ClearPaths(Common.Direction dir)
	{
		if (FootPaths != null && FootPaths[(int)dir] != null)
		{
			FootPaths[(int)dir].TerrainPath.Value = 0f;
		}
		if (WheelPaths != null && WheelPaths[(int)dir] != null)
		{
			WheelPaths[(int)dir].TerrainPath.Value = 0f;
		}
		roadConnectionsAreDirty = true;
		RedrawTerrainCostsAroundEdge(dir);
	}

	public void RemoveRoad(Entity road, Common.Direction dir)
	{
		if (Roads != null)
		{
			Roads[(int)dir] = null;
			roadConnectionsAreDirty = true;
		}
	}

	public void AddRenderable(Renderable renderable)
	{
		if (RenderablesOnTile == null)
		{
			RenderablesOnTile = new List<Renderable>();
		}
		RenderablesOnTile.Add(renderable);
	}

	public void AddTree(Entity tree)
	{
		if (TreesOnTile == null)
		{
			TreesOnTile = new List<Entity>();
		}
		TreesOnTile.Add(tree);
	}

	public void RemoveTree(Entity tree)
	{
		if (TreesOnTile != null)
		{
			TreesOnTile.Remove(tree);
		}
	}

	public bool ContainsTree(Entity tree)
	{
		if (TreesOnTile != null)
		{
			return TreesOnTile.Contains(tree);
		}
		return false;
	}

	public bool ContainsTreeAtSubtile(Point subtile)
	{
		if (TreesOnTile != null)
		{
			foreach (Entity item in TreesOnTile)
			{
				if (MapManager.WorldPosToSubtile(item.PlaySiteLocation) == subtile)
				{
					return true;
				}
			}
		}
		return false;
	}

	public TerrainTile(int x, int y)
	{
		AddToLookup();
		TilePos = new TilePos(x, y);
		TileResources = new Dictionary<ResourceType, TileResourceContainer>();
		Temperature = GameData.Instance.Constants.MeanAmbientTemperature;
		WaterAmount = 700f;
	}

	public TerrainTile()
	{
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public void ComputeRoadConnections()
	{
		if (Roads != null || WheelPaths != null || FootPaths != null)
		{
			if (renderedRoads == null)
			{
				renderedRoads = new List<RoadAndPathQuad>();
			}
			renderedRoads.Clear();
			if (Roads != null)
			{
				ComputeRoadConnections(Roads, TreatStructuresOrTerrain.Structure);
			}
			if (WheelPaths != null)
			{
				ComputeRoadConnections(WheelPaths, TreatStructuresOrTerrain.Terrain);
			}
			if (FootPaths != null)
			{
				ComputeRoadConnections(FootPaths, TreatStructuresOrTerrain.Terrain);
			}
		}
	}

	private void ComputeRoadConnections(Entity[] roadsOrPaths, TreatStructuresOrTerrain itemsToTreat)
	{
		if (roadsOrPaths == null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			if (roadsOrPaths[i] != null)
			{
				roadsOrPaths[i].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection = false;
			}
		}
		_ = The.Map;
		for (int j = 0; j < 8; j++)
		{
			if (roadsOrPaths[j] == null)
			{
				continue;
			}
			RenderAsConnectedGroundSprite renderAsConnectedGroundSprite = roadsOrPaths[j].Renderable.RenderAsConnectedGroundSprite;
			for (int k = 0; k < 8; k++)
			{
				if (j != k && roadsOrPaths[k] != null && (itemsToTreat == TreatStructuresOrTerrain.Structure || roadsOrPaths[k].TerrainPath.Value > 0f) && !roadsOrPaths[k].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection && roadsOrPaths[j].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName == roadsOrPaths[k].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName)
				{
					Rectangle?[,] array = UWGame.ClientSide.Client.AllConnectedGroundSprites[roadsOrPaths[j].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName];
					if (array[j, k].HasValue)
					{
						Vector4 color = roadsOrPaths[j].TerrainPath.GetColor();
						Vector4 color2 = roadsOrPaths[k].TerrainPath.GetColor();
						Vector4 color3 = (color + color2) / 2f;
						new Renderable((Entity)null, (RenderableType)null).SetTintColor(new Color(color3));
						RoadAndPathQuad roadAndPathQuad = new RoadAndPathQuad();
						Rectangle value = array[j, k].Value;
						roadAndPathQuad.SetupQuadVertices(MapManager.TileToWorldPos(this), new Vector2((float)value.Width / 2f, (float)value.Height / 2f), value, The.Client.FlatSpriteSheet.Texture, flipSprite: false);
						renderedRoads.Add(roadAndPathQuad);
						renderAsConnectedGroundSprite.IsRenderedAsConnection = true;
						roadsOrPaths[k].Renderable.RenderAsConnectedGroundSprite.IsRenderedAsConnection = true;
					}
				}
			}
		}
		for (int l = 0; l < 8; l++)
		{
			if (roadsOrPaths[l] != null)
			{
				RenderAsConnectedGroundSprite renderAsConnectedGroundSprite2 = roadsOrPaths[l].Renderable.RenderAsConnectedGroundSprite;
				if (!renderAsConnectedGroundSprite2.IsRenderedAsConnection && (itemsToTreat == TreatStructuresOrTerrain.Structure || roadsOrPaths[l].TerrainPath.Value > 0f))
				{
					Vector4 color4 = roadsOrPaths[l].TerrainPath.GetColor();
					new Renderable((Entity)null, (RenderableType)null).SetTintColor(new Color(color4));
					RoadAndPathQuad roadAndPathQuad2 = new RoadAndPathQuad();
					Rectangle?[,] array = UWGame.ClientSide.Client.AllConnectedGroundSprites[roadsOrPaths[l].EntityType.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName];
					Rectangle value2 = array[l, l].Value;
					roadAndPathQuad2.SetupQuadVertices(MapManager.TileToWorldPos(this), new Vector2((float)value2.Width / 2f, (float)value2.Height / 2f), value2, The.Client.FlatSpriteSheet.Texture, flipSprite: false);
					renderedRoads.Add(roadAndPathQuad2);
					renderAsConnectedGroundSprite2.IsRenderedAsConnection = true;
				}
			}
		}
	}

	public byte GetCost(SurfaceType.TransportType transport, SurfaceType.TerrainFeatures features)
	{
		return TerrainType.Cost(transport, features);
	}

	public void CopyQuadToVertexBuffer(VertexGroundFeature[] roadAndPathVertices, ref int quadIndex, Renderable.AdditionalEffect? overridingEffectID = null)
	{
		if (roadConnectionsAreDirty)
		{
			ComputeRoadConnections();
			roadConnectionsAreDirty = false;
		}
		if (renderedRoads == null)
		{
			return;
		}
		using List<RoadAndPathQuad>.Enumerator enumerator = renderedRoads.GetEnumerator();
		while (enumerator.MoveNext() && enumerator.Current.CopyQuadToVertexBuffer(roadAndPathVertices, quadIndex))
		{
			quadIndex++;
		}
	}

	public float GetDustFactor()
	{
		return 1f - Moisture;
	}

	public void AddFirewood()
	{
	}

	public TileResourceContainer AddResource(string resourceKeyName, float totalHarvestableBulk)
	{
		TileResourceContainer tileResourceContainer = GetTileResourceContainer(resourceKeyName);
		tileResourceContainer.SetTotalHarvestableBulk(totalHarvestableBulk);
		return tileResourceContainer;
	}

	public TileResourceContainer AddResource(ResourceType resourceType, float totalHarvestableBulk)
	{
		TileResourceContainer tileResourceContainer = GetTileResourceContainer(resourceType);
		tileResourceContainer.SetTotalHarvestableBulk(totalHarvestableBulk);
		return tileResourceContainer;
	}

	public TileResourceContainer AddResource(string resourceKeyName, int noOfResourceItems)
	{
		TileResourceContainer tileResourceContainer = GetTileResourceContainer(resourceKeyName);
		tileResourceContainer.SetResourceItems(noOfResourceItems);
		return tileResourceContainer;
	}

	public TileResourceContainer AddResource(ResourceType resourceType, int noOfResourceItems)
	{
		TileResourceContainer tileResourceContainer = GetTileResourceContainer(resourceType);
		tileResourceContainer.SetResourceItems(noOfResourceItems);
		return tileResourceContainer;
	}

	private TileResourceContainer GetTileResourceContainer(string resourceKeyName)
	{
		ResourceType resourceType = GameData.Instance.AllResourceTypes[resourceKeyName];
		return GetTileResourceContainer(resourceType);
	}

	private TileResourceContainer GetTileResourceContainer(ResourceType resourceType)
	{
		if (TileResources == null)
		{
			TileResources = new Dictionary<ResourceType, TileResourceContainer>();
		}
		if (!TileResources.TryGetValue(resourceType, out var value))
		{
			return new TileResourceContainer(this, resourceType);
		}
		return value;
	}

	public Terrain GetCenterTerrain()
	{
		if (Terrain != null)
		{
			return Terrain;
		}
		return TerrainSubtiles[1][1];
	}

	public bool IsPartlyUnderWater()
	{
		if (Terrain != null)
		{
			return Terrain.IsUnderWater();
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (TerrainSubtiles[i][j].IsUnderWater())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsFullyUnderWater()
	{
		if (Terrain != null)
		{
			return Terrain.IsUnderWater();
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!TerrainSubtiles[i][j].IsUnderWater())
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string ToString()
	{
		return X + "," + Y;
	}

	public TerrainTileID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= TerrainTileID.Invalid)
		{
			throw new Exception("Astounding, TerrainTileID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public TerrainTileID SnapshotID(Snapshotter sn, TerrainTileID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != TerrainTileID.Invalid)
		{
			LookUpSortedDictionary<TerrainTile, TerrainTileID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = TerrainTileID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUpSortedDictionary<TerrainTile, TerrainTileID>.Remove(this);
	}

	void ILookUp<TerrainTile, TerrainTileID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = TerrainTileID.First;
	}

	void ILookUp<TerrainTile, TerrainTileID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUpSortedDictionary<TerrainTile, TerrainTileID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (X == 18)
		{
			_ = Y;
			_ = 10;
		}
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotAllegiancesThatSeeThisTile = AllegiancesThatSeeThisTile.Select((Allegiance a) => a.ID).ToList();
			snapshotEntitiesThatSeeThisTile = EntitiesThatSeeThisTile.Select((Entity e) => e.EntityID).ToList();
			if (TileResources != null)
			{
				snapshotTileResources = TileResources.ToDictionary((KeyValuePair<ResourceType, TileResourceContainer> r) => r.Key.KeyName, (KeyValuePair<ResourceType, TileResourceContainer> r) => r.Value.ID);
			}
			if (EntitiesOnTile != null)
			{
				snapshotEntitiesOnTile = EntitiesOnTile.Select((Entity e) => e.EntityID).ToList();
			}
			if (TreesOnTile != null)
			{
				snapshotTreesOnTile = TreesOnTile.Select((Entity t) => t.ID).ToList();
			}
			if (RememberedRootEntitiesOnTile != null)
			{
				snapshotRememberedEntitiesOnTile = new Dictionary<AllegianceID, List<MemoryFactID>>();
				foreach (KeyValuePair<SharedKnowledge, List<MemoryFact>> item in RememberedRootEntitiesOnTile)
				{
					if (item.Key.Allegiance.ID != AllegianceID.Invalid)
					{
						snapshotRememberedEntitiesOnTile.Add(item.Key.Allegiance.ID, item.Value.Select((MemoryFact m) => m.ID).ToList());
					}
				}
			}
			if (RememberedProcessesOnTile != null)
			{
				snapshotRememberedProcessesOnTile = new Dictionary<AllegianceID, List<ProcessMemoryID>>();
				foreach (KeyValuePair<SharedKnowledge, List<ProcessMemory>> item2 in RememberedProcessesOnTile)
				{
					if (item2.Key.Allegiance.ID != AllegianceID.Invalid)
					{
						snapshotRememberedProcessesOnTile.Add(item2.Key.Allegiance.ID, item2.Value.Select((ProcessMemory m) => m.ID).ToList());
					}
				}
			}
			if (HarvestJobs != null)
			{
				snapshotHarvestJobs = new Dictionary<EntityGroupID, Dictionary<ResourceType, List<JobID>>>();
				foreach (KeyValuePair<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>> harvestJob in HarvestJobs)
				{
					Dictionary<ResourceType, List<JobID>> dictionary = new Dictionary<ResourceType, List<JobID>>();
					snapshotHarvestJobs.Add(harvestJob.Key, dictionary);
					foreach (KeyValuePair<ResourceType, List<ProcessJob>> item3 in harvestJob.Value)
					{
						dictionary.Add(item3.Key, item3.Value.Select((ProcessJob j) => j.ID).ToList());
					}
				}
			}
			if (Zones != null)
			{
				snapshotZones = new Dictionary<AllegianceID, List<ZoneID>>();
				foreach (KeyValuePair<Allegiance, List<Zone>> zone in Zones)
				{
					if (zone.Key.ID != AllegianceID.Invalid)
					{
						snapshotZones.Add(zone.Key.ID, zone.Value.Select((Zone z) => z.ID).ToList());
					}
				}
			}
			if (BaseCenterForMultiTileEntities != null)
			{
				snapshotBaseCenterEntities = new List<EntityID>();
				foreach (Entity baseCenterForMultiTileEntity in BaseCenterForMultiTileEntities)
				{
					if (baseCenterForMultiTileEntity.ID != EntityID.Invalid)
					{
						snapshotBaseCenterEntities.Add(baseCenterForMultiTileEntity.ID);
					}
				}
			}
			if (TerrainSubtiles != null)
			{
				int jaggedArrayWidth = Common.GetJaggedArrayWidth(TerrainSubtiles);
				int jaggedArrayHeight = Common.GetJaggedArrayHeight(TerrainSubtiles);
				Common.InitJaggedArray(ref snapshotTerrainSubtiles, jaggedArrayWidth, jaggedArrayHeight);
				for (int num = 0; num < jaggedArrayWidth; num++)
				{
					for (int num2 = 0; num2 < jaggedArrayHeight; num2++)
					{
						snapshotTerrainSubtiles[num][num2] = TerrainSubtiles[num][num2].ID;
					}
				}
			}
		}
		ID = SnapshotID(sn, ID);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotEntitiesOnTile = sn.DoList(snapshotEntitiesOnTile);
		snapshotAllegiancesThatSeeThisTile = sn.DoList(snapshotAllegiancesThatSeeThisTile);
		snapshotEntitiesThatSeeThisTile = sn.DoList(snapshotEntitiesThatSeeThisTile);
		snapshotRememberedEntitiesOnTile = sn.DoMultiMap(snapshotRememberedEntitiesOnTile);
		snapshotRememberedProcessesOnTile = sn.DoMultiMap(snapshotRememberedProcessesOnTile);
		if (snapshotZones != null)
		{
			foreach (KeyValuePair<AllegianceID, List<ZoneID>> snapshotZone in snapshotZones)
			{
				_ = snapshotZone.Key;
				_ = long.MaxValue;
			}
		}
		snapshotZones = sn.DoMultiMap(snapshotZones);
		snapshotTreesOnTile = sn.DoList(snapshotTreesOnTile);
		snapshotBaseCenterEntities = sn.DoList(snapshotBaseCenterEntities);
		snapshotTileResources = sn.DoDictionary(snapshotTileResources);
		snapshotHarvestJobs = sn.DoNestedMultiMap(snapshotHarvestJobs);
		snapshotOperatingAreaOf = sn.SnapshotID<Expedition, ExpeditionID>(OperatingAreaOf);
		snapshotTerrainSubtiles = sn.DoJaggedArray(snapshotTerrainSubtiles);
		snapshotTerrain = sn.SnapshotID<Terrain, TerrainID>(Terrain);
		GeoLayoutEntitiesOnTile = sn.DoList(GeoLayoutEntitiesOnTile);
		ColorOfWater = sn.DoVector4(ColorOfWater);
		HasEverBeenSeenByPlayer = sn.DoBool(HasEverBeenSeenByPlayer);
		Humidity = sn.DoFloat(Humidity);
		moisture = sn.DoFloat(moisture);
		Owner = sn.DoEnumNullable(Owner);
		Temperature = sn.DoFloat(Temperature);
		TilePos = sn.DoTilePos(TilePos);
		waterAmount = sn.DoFloat(waterAmount);
		WaterBottomTint = sn.DoVector4(WaterBottomTint);
		ProcessesOnTile = sn.DoList(ProcessesOnTile);
		sn.Ignore(DesignerPlacedResources);
		sn.Ignore(Terrain);
		sn.Ignore(TerrainSubtiles);
		sn.Ignore(roadConnectionsAreDirty);
		sn.Ignore(TreesOnTile);
		sn.Ignore(EntitiesOnTile);
		sn.Ignore(BaseCenterForMultiTileEntities);
		sn.Ignore(GeoLayoutEntitiesOnTile);
		sn.Ignore(EdgeLayoutEntities);
		sn.Ignore(RememberedRootEntitiesOnTile);
		sn.Ignore(RememberedProcessesOnTile);
		sn.Ignore(RenderablesOnTile);
		sn.Ignore(Zones);
		sn.Ignore(EntitiesThatSeeThisTile);
		sn.Ignore(AllegiancesThatSeeThisTile);
		sn.Ignore(HarvestJobs);
		sn.Ignore(TileResources);
		sn.Ignore(Level);
		sn.Ignore(FootPaths);
		sn.Ignore(WheelPaths);
		sn.Ignore(Roads);
		sn.Ignore(renderedRoads);
		sn.Ignore(TerrainType);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		if (X == 9)
		{
			_ = Y;
			_ = 9;
		}
		sn.RegisterLoadPostProcessCall(this);
		AllegiancesThatSeeThisTile = new HashSet<Allegiance>(snapshotAllegiancesThatSeeThisTile.Select((AllegianceID a) => LookUp<Allegiance, AllegianceID>.FindByID(a)));
		EntitiesThatSeeThisTile = new HashSet<Entity>(snapshotEntitiesThatSeeThisTile.Select((EntityID e) => Entity.FindByID(e)));
		if (snapshotTileResources != null)
		{
			TileResources = snapshotTileResources.ToDictionary((KeyValuePair<string, ResourceID> r) => GameData.Instance.AllResourceTypes[r.Key], (KeyValuePair<string, ResourceID> r) => (TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(r.Value));
			snapshotTileResources = null;
		}
		if (snapshotEntitiesOnTile != null)
		{
			EntitiesOnTile = snapshotEntitiesOnTile.Select((EntityID e) => Entity.FindByID(e)).ToList();
			snapshotEntitiesOnTile = null;
		}
		if (snapshotTreesOnTile != null)
		{
			TreesOnTile = snapshotTreesOnTile.Select((EntityID e) => Entity.FindByID(e)).ToList();
			snapshotTreesOnTile = null;
		}
		if (snapshotRememberedEntitiesOnTile != null)
		{
			RememberedRootEntitiesOnTile = new Dictionary<SharedKnowledge, List<MemoryFact>>();
			foreach (KeyValuePair<AllegianceID, List<MemoryFactID>> item in snapshotRememberedEntitiesOnTile)
			{
				RememberedRootEntitiesOnTile.Add(LookUp<Allegiance, AllegianceID>.FindByID(item.Key).SharedKnowledge, item.Value.Select((MemoryFactID m) => LookUp<MemoryFact, MemoryFactID>.FindByID(m)).ToList());
			}
			snapshotRememberedEntitiesOnTile = null;
		}
		if (snapshotRememberedProcessesOnTile != null)
		{
			RememberedProcessesOnTile = new Dictionary<SharedKnowledge, List<ProcessMemory>>();
			foreach (KeyValuePair<AllegianceID, List<ProcessMemoryID>> item2 in snapshotRememberedProcessesOnTile)
			{
				RememberedProcessesOnTile.Add(LookUp<Allegiance, AllegianceID>.FindByID(item2.Key).SharedKnowledge, item2.Value.Select((ProcessMemoryID m) => LookUp<ProcessMemory, ProcessMemoryID>.FindByID(m)).ToList());
			}
			snapshotRememberedProcessesOnTile = null;
		}
		if (snapshotHarvestJobs != null)
		{
			HarvestJobs = new Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>>();
			foreach (KeyValuePair<EntityGroupID, Dictionary<ResourceType, List<JobID>>> snapshotHarvestJob in snapshotHarvestJobs)
			{
				Dictionary<ResourceType, List<ProcessJob>> dictionary = new Dictionary<ResourceType, List<ProcessJob>>();
				HarvestJobs.Add(snapshotHarvestJob.Key, dictionary);
				foreach (KeyValuePair<ResourceType, List<JobID>> item3 in snapshotHarvestJob.Value)
				{
					dictionary.Add(item3.Key, item3.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
				}
			}
			snapshotHarvestJobs = null;
		}
		if (snapshotZones != null)
		{
			Zones = new Dictionary<Allegiance, List<Zone>>();
			foreach (KeyValuePair<AllegianceID, List<ZoneID>> snapshotZone in snapshotZones)
			{
				_ = snapshotZone.Key;
				Zones.Add(LookUp<Allegiance, AllegianceID>.FindByID(snapshotZone.Key), snapshotZone.Value.Select((ZoneID z) => LookUp<Zone, ZoneID>.FindByID(z)).ToList());
			}
			snapshotZones = null;
		}
		if (snapshotBaseCenterEntities != null)
		{
			BaseCenterForMultiTileEntities = snapshotBaseCenterEntities.Select((EntityID e) => Entity.FindByID(e)).ToList();
			snapshotBaseCenterEntities = null;
		}
		if (snapshotTerrainSubtiles != null)
		{
			int jaggedArrayWidth = Common.GetJaggedArrayWidth(snapshotTerrainSubtiles);
			int jaggedArrayHeight = Common.GetJaggedArrayHeight(snapshotTerrainSubtiles);
			Common.InitJaggedArray(ref TerrainSubtiles, jaggedArrayWidth, jaggedArrayHeight);
			for (int num = 0; num < jaggedArrayWidth; num++)
			{
				for (int num2 = 0; num2 < jaggedArrayHeight; num2++)
				{
					TerrainSubtiles[num][num2] = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotTerrainSubtiles[num][num2]);
				}
			}
			snapshotTerrainSubtiles = null;
		}
		Terrain = LookUpSortedDictionary<Terrain, TerrainID>.FindByID(snapshotTerrain);
		OperatingAreaOf = LookUp<Expedition, ExpeditionID>.FindByID(snapshotOperatingAreaOf);
	}
}
