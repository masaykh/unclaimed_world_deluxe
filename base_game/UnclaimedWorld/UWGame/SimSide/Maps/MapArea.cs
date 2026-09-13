using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface.MapGUI;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Maps;

[DebuggerDisplay("{BoundingRectangle}")]
public class MapArea : ISnapshot
{
	public delegate void IterateMethod(TerrainTile tile);

	public delegate bool IterateBooleanMethod(TerrainTile tile);

	public delegate bool CycleEntityPredicate(IKnownEntityData entityData);

	public struct ResourcesAndJobs
	{
		public int NumberOfResources;

		public int NumberOfJobsInZone;

		public int NumberOfJobsInOtherZones;

		public float CurrentRegrowth;

		public float MaximumRegrowth;

		public bool MaximumReached;

		public bool UserChangedData;
	}

	public struct FindPreyJobs
	{
		public int NumberOfJobsInZone;

		public bool UserChangedData;
	}

	private List<TerrainTile> Coverage = new List<TerrainTile>();

	private List<TerrainTileID> snapshotCoverage;

	public MapAreaRender MapAreaRender;

	public Point? StartDragTile;

	public Rectangle? BoundingRectangle;

	public TerrainTile BottomLeftTile;

	private TerrainTileID snapshotBottomLeft;

	public TerrainTile UpperLeftTile;

	private TerrainTileID snapshotUpperLeft;

	public TerrainTile BottomRightTile;

	private TerrainTileID snapshotBottomRight;

	public TerrainTile UpperRightTile;

	private TerrainTileID snapshotUpperRight;

	public Zone Zone;

	private ZoneID? snapshotZone;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int Count => Coverage.Count;

	public bool IsSnapshotted { get; set; }

	public MapArea()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			Initialize();
		}
	}

	public List<TilePos> GetTileLocations()
	{
		List<TilePos> list = new List<TilePos>();
		foreach (TerrainTile item in Coverage)
		{
			list.Add(item.TilePos);
		}
		return list;
	}

	public void Initialize()
	{
		MapAreaRender = new MapAreaRender(this);
	}

	public MapArea(MapArea mapArea, Zone zone = null)
	{
		Coverage.AddRange(mapArea.Coverage);
		StartDragTile = mapArea.StartDragTile;
		Zone = zone;
		zone.MapArea = this;
		RecomputeBoundingRectangleAndEdges(computeEdges: false);
		Initialize();
	}

	public void Clear()
	{
		Coverage.Clear();
		MapAreaRender.SetIsDirty();
	}

	public void Add(TerrainTile tile)
	{
		Coverage.Add(tile);
		MapAreaRender.SetIsDirty();
	}

	public Vector3? GetCenter()
	{
		if (BoundingRectangle.HasValue)
		{
			return MapManager.TileToWorldPos(BoundingRectangle.Value.Center);
		}
		return null;
	}

	public Rectangle? GetBoundingBoxInTiles()
	{
		if (Coverage.Count == 0)
		{
			return null;
		}
		int num = Coverage.Min((TerrainTile t) => t.X);
		int num2 = Coverage.Max((TerrainTile t) => t.X);
		int num3 = Coverage.Min((TerrainTile t) => t.Y);
		int num4 = Coverage.Max((TerrainTile t) => t.Y);
		return new Rectangle(num, num3, num2 - num + 1, num4 - num3 + 1);
	}

	public void IterateArea(IterateMethod iterateMethod)
	{
		foreach (TerrainTile item in Coverage)
		{
			iterateMethod(item);
		}
	}

	public void IterateAreaBreakOnTrue(IterateBooleanMethod iterateMethod)
	{
		foreach (TerrainTile item in Coverage)
		{
			if (iterateMethod(item))
			{
				break;
			}
		}
	}

	public void HandleFirstTile(IterateMethod handleMethod)
	{
		handleMethod(Coverage[0]);
	}

	public void PostLoadContent()
	{
		MapAreaRender.PostLoadContent();
	}

	public TerrainTile GetFirst()
	{
		return Coverage[0];
	}

	public TerrainTile GetBottomLeft()
	{
		List<TerrainTile> source = Coverage.FindAll((TerrainTile tile) => tile.X == BoundingRectangle.Value.Left);
		int maxY = source.Max((TerrainTile tile) => tile.Y);
		return source.FirstOrDefault((TerrainTile tile) => tile.Y == maxY);
	}

	public TerrainTile GetUpperLeft()
	{
		List<TerrainTile> source = Coverage.FindAll((TerrainTile tile) => tile.X == BoundingRectangle.Value.Left);
		int maxY = source.Min((TerrainTile tile) => tile.Y);
		return source.FirstOrDefault((TerrainTile tile) => tile.Y == maxY);
	}

	public TerrainTile GetBottomRight()
	{
		List<TerrainTile> source = Coverage.FindAll((TerrainTile tile) => tile.X == BoundingRectangle.Value.Right - 1);
		int maxY = source.Max((TerrainTile tile) => tile.Y);
		return source.FirstOrDefault((TerrainTile tile) => tile.Y == maxY);
	}

	public TerrainTile GetUpperRight()
	{
		List<TerrainTile> source = Coverage.FindAll((TerrainTile tile) => tile.X == BoundingRectangle.Value.Right - 1);
		int maxY = source.Min((TerrainTile tile) => tile.Y);
		return source.FirstOrDefault((TerrainTile tile) => tile.Y == maxY);
	}

	public EntityGroup GetOwner()
	{
		if (Zone != null)
		{
			return LookUp<EntityGroup, EntityGroupID>.FindByID(Zone.Owner);
		}
		return The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
	}

	public void RecomputeBoundingRectangleAndEdges(bool computeEdges = true)
	{
		if (Coverage.Count == 0)
		{
			return;
		}
		int num = Coverage.Min((TerrainTile t) => t.X);
		int num2 = Coverage.Min((TerrainTile t) => t.Y);
		int num3 = Coverage.Max((TerrainTile t) => t.X);
		int num4 = Coverage.Max((TerrainTile t) => t.Y);
		BoundingRectangle = new Rectangle(num, num2, num3 - num + 1, num4 - num2 + 1);
		BottomLeftTile = GetBottomLeft();
		UpperLeftTile = GetUpperLeft();
		BottomRightTile = GetBottomRight();
		UpperRightTile = GetUpperRight();
		if (!BoundingRectangle.Value.Contains(StartDragTile.Value))
		{
			TilePos startPos = new TilePos(StartDragTile.Value);
			TerrainTile minimum = Common.GetMinimum(Coverage, (TerrainTile t) => Common.DistanceOctile(t.TilePos, startPos));
			StartDragTile = new Point(minimum.X, minimum.Y);
		}
		if (computeEdges && Zone != null)
		{
			Zone.ComputeEdges();
		}
	}

	public TerrainTile GetCornerFromIndex(int index)
	{
		return index switch
		{
			0 => BottomLeftTile, 
			1 => BottomRightTile, 
			2 => UpperRightTile, 
			3 => UpperLeftTile, 
			_ => null, 
		};
	}

	public bool CheckConnectivity(bool createList, ref List<TerrainTile> connectedTiles)
	{
		FloodFill floodFill = new FloodFill();
		SubtileLayers subtileLayers = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		ushort[][] map = null;
		Common.InitJaggedArray(ref map, 3 * BoundingRectangle.Value.Width, 3 * BoundingRectangle.Value.Height);
		Point point = MapManager.TileEdgeToSubtile(new Point(BoundingRectangle.Value.X, BoundingRectangle.Value.Y));
		floodFill.DoFloodFillOfArea(areaSubtiles: new Rectangle(point.X, point.Y, 3 * BoundingRectangle.Value.Width, 3 * BoundingRectangle.Value.Height), terrainGrid: subtileLayers, allNodes: map, fromSubtile: MapManager.TileEdgeToSubtile(StartDragTile.Value));
		Common.GetJaggedArrayWidth(map);
		Common.GetJaggedArrayHeight(map);
		bool result = true;
		if (createList)
		{
			connectedTiles = new List<TerrainTile>();
		}
		Point point2 = The.Map.ClampTileMapPosition(new Point(BoundingRectangle.Value.Left, BoundingRectangle.Value.Top));
		Point point3 = The.Map.ClampTileMapPosition(new Point(BoundingRectangle.Value.Right, BoundingRectangle.Value.Bottom));
		for (int i = point2.X; i < point3.X; i++)
		{
			for (int j = point2.Y; j < point3.Y; j++)
			{
				Point point4 = new Point(i, j);
				Point subtileCorner = MapManager.TileEdgeToSubtile(i - BoundingRectangle.Value.Left, j - BoundingRectangle.Value.Top);
				if (IsTileVisited(map, subtileCorner))
				{
					if (createList)
					{
						connectedTiles.Add(The.Map.GetTile(point4));
					}
				}
				else if (!The.Map.TileIsCompletelyBlocked(subtileLayers, point4))
				{
					result = false;
					if (!createList)
					{
						return false;
					}
				}
			}
		}
		return result;
	}

	public void CropTilesToConnectedArea()
	{
		List<TerrainTile> connectedTiles = null;
		The.InGameUI.SelectedTiles.CheckConnectivity(createList: true, ref connectedTiles);
		Clear();
		foreach (TerrainTile item in connectedTiles)
		{
			Add(item);
		}
		RecomputeBoundingRectangleAndEdges();
		if (Zone == null || Zone.HarvestJobs == null)
		{
			return;
		}
		foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in Zone.HarvestJobs)
		{
			Point jobPosition;
			for (int num = harvestJob.Value.Count - 1; num >= 0; num--)
			{
				ProcessJob processJob = harvestJob.Value[num];
				jobPosition = processJob.HarvestJob.Item.Container.MapPosition;
				if (!Coverage.Exists((TerrainTile t) => t.X == jobPosition.X && t.Y == jobPosition.Y))
				{
					processJob.Destroy(removeTakers: true);
				}
			}
		}
	}

	public EntityID? CycleKnownEntities(EntityID? previousID, SharedKnowledge sharedKnowledge, CycleEntityPredicate predicate, out bool foundEntities, bool returnFirstMatch = false)
	{
		new List<EntityID>();
		foundEntities = false;
		if (!previousID.HasValue)
		{
			returnFirstMatch = true;
		}
		EntityID? entityID2;
		foreach (TerrainTile item in Coverage)
		{
			if (item.EntitiesOnTile != null)
			{
				foreach (Entity item2 in item.EntitiesOnTile)
				{
					if (!predicate(item2) || sharedKnowledge.GetKnownData(item2.EntityID, out var _) != EntityResult.SeenDirectly)
					{
						continue;
					}
					foundEntities = true;
					EntityID entityID = item2.EntityID;
					entityID2 = previousID;
					if (entityID == entityID2)
					{
						returnFirstMatch = true;
						continue;
					}
					if (!returnFirstMatch)
					{
						continue;
					}
					entityID2 = item2.EntityID;
					goto IL_0178;
				}
			}
			if (item.RememberedRootEntitiesOnTile == null || !item.RememberedRootEntitiesOnTile.TryGetValue(sharedKnowledge, out var value))
			{
				continue;
			}
			foreach (MemoryFact item3 in value)
			{
				if (!predicate(item3))
				{
					continue;
				}
				foundEntities = true;
				if (returnFirstMatch)
				{
					entityID2 = item3.EntityID;
					goto IL_0178;
				}
				if (item3.EntityID == previousID)
				{
					returnFirstMatch = true;
				}
			}
		}
		return null;
		IL_0178:
		return entityID2;
	}

	private static bool IsTileVisited(ushort[][] nodes, Point subtileCorner)
	{
		for (int i = subtileCorner.X; i < subtileCorner.X + 3; i++)
		{
			for (int j = subtileCorner.Y; j < subtileCorner.Y + 3; j++)
			{
				if (nodes[i][j] == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector3? SelectBestGroundLocationForStorage(IKnownEntityData item, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData)
	{
		foreach (TerrainTile item2 in Coverage)
		{
			Point tilePos = new Point(item2.X, item2.Y);
			Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileGroundStorageData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);
			Vector3? vector = HaulingJobManager.FindSimilarItemToStackWithInTile(item, tileGroundStorageData, item2);
			if (vector.HasValue)
			{
				return vector.Value;
			}
		}
		foreach (TerrainTile item3 in Coverage)
		{
			Point tilePos = new Point(item3.X, item3.Y);
			Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileGroundStorageData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);
			Vector3? vector = HaulingJobManager.FindEmptySubtileForStorage(tilePos, item, tileGroundStorageData, leftMostSubtilesOnly: true);
			if (vector.HasValue)
			{
				return vector.Value;
			}
		}
		foreach (TerrainTile item4 in Coverage)
		{
			Point tilePos = new Point(item4.X, item4.Y);
			Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileGroundStorageData = HaulingJobManager.GetTileGroundStorageData(allTileData, tilePos);
			Vector3? vector = HaulingJobManager.FindFirstSubtileWithRoomForStorage(item, tilePos, tileGroundStorageData);
			if (vector.HasValue)
			{
				return vector.Value;
			}
		}
		return null;
	}

	public void GetNoOfItemsInAreaByType(Dictionary<EntityType, List<EntityID>> allItems, Predicate<IKnownEntityData> entitiesToCount, Allegiance allegianceViewpoint)
	{
		IterateArea(delegate(TerrainTile tile)
		{
			List<MemoryFact> value;
			if (tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint))
			{
				if (tile.EntitiesOnTile != null)
				{
					foreach (Entity item in tile.EntitiesOnTile)
					{
						allItems = AddToAllItems(allItems, item.EntityType, item.EntityID);
					}
				}
			}
			else if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out value) && value != null)
			{
				foreach (MemoryFact item2 in value)
				{
					allItems = AddToAllItems(allItems, item2.EntityType, item2.EntityID);
				}
			}
		});
	}

	private static Dictionary<EntityType, List<EntityID>> AddToAllItems(Dictionary<EntityType, List<EntityID>> allItems, EntityType entityType, EntityID entityID)
	{
		if (entityType.ItemType != null)
		{
			if (!allItems.TryGetValue(entityType, out var value))
			{
				allItems.Add(entityType, value);
				allItems.TryGetValue(entityType, out value);
			}
			if (value != null)
			{
				if (!value.Contains(entityID))
				{
					value.Add(entityID);
					allItems[entityType] = value;
				}
			}
			else
			{
				value = new List<EntityID>();
				value.Add(entityID);
				allItems[entityType] = value;
			}
		}
		return allItems;
	}

	public void GetNoOfEntitiesInArea(Dictionary<EntityType, int> allItems, Predicate<IKnownEntityData> entitiesToCount, Allegiance allegianceViewpoint)
	{
		IterateArea(delegate(TerrainTile tile)
		{
			List<MemoryFact> value;
			if (The.Sim.Mode == Sim.EngineMode.Edit || tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint))
			{
				if (tile.EntitiesOnTile != null)
				{
					foreach (Entity item in tile.EntitiesOnTile)
					{
						CountEntity(allItems, item, entitiesToCount);
					}
				}
			}
			else if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out value) && value != null)
			{
				foreach (MemoryFact item2 in value)
				{
					if (!item2.ContainedBy.HasValue && !item2.PartOfID.HasValue)
					{
						CountEntity(allItems, item2, entitiesToCount);
					}
				}
			}
		});
	}

	public static void CountEntity(Dictionary<EntityType, int> allItems, IKnownEntityData entity, Predicate<IKnownEntityData> entitiesToCount)
	{
		if (entitiesToCount(entity))
		{
			int value = ((!allItems.TryGetValue(entity.EntityType, out value)) ? 1 : (value + 1));
			allItems[entity.EntityType] = value;
		}
	}

	public void GetSumOfAllResourcesInArea(SharedKnowledge sharedKnowledge, Dictionary<ResourceType, ResourcesAndJobs> sum)
	{
		IterateArea(delegate(TerrainTile tile)
		{
			if (tile.TreesOnTile != null)
			{
				foreach (Entity item in tile.TreesOnTile)
				{
					item.Find<Tree>(out var c);
					if (c.Crops != null)
					{
						foreach (KeyValuePair<ResourceType, Crop> crop in c.Crops)
						{
							if (JobManager.AllowGatherJobForResource(sharedKnowledge, crop.Value))
							{
								AddToResourceSum(crop.Value, sum);
							}
						}
					}
				}
			}
			if (tile.TileResources != null)
			{
				foreach (KeyValuePair<ResourceType, TileResourceContainer> tileResource in tile.TileResources)
				{
					if (JobManager.AllowGatherJobForResource(sharedKnowledge, tileResource.Value))
					{
						AddToResourceSum(tileResource.Value, sum);
					}
				}
			}
		});
	}

	private void AddToResourceSum(ResourceContainer container, Dictionary<ResourceType, ResourcesAndJobs> sum)
	{
		bool maximumReached = false;
		if (sum.TryGetValue(container.ResourceType, out var value))
		{
			value.NumberOfResources += container.ResourceItems.Count;
			value.MaximumRegrowth += container.GetMaximumRegrowth();
			value.CurrentRegrowth += container.GetCurrentRegrowth(out maximumReached);
			if (value.MaximumReached && !maximumReached)
			{
				value.MaximumReached = false;
			}
			sum[container.ResourceType] = value;
		}
		else
		{
			ResourcesAndJobs value2 = new ResourcesAndJobs
			{
				NumberOfResources = container.ResourceItems.Count,
				NumberOfJobsInZone = GetNoOfJobs(container.ResourceType),
				NumberOfJobsInOtherZones = 0,
				MaximumRegrowth = container.GetMaximumRegrowth(),
				UserChangedData = false
			};
			value2.CurrentRegrowth = container.GetCurrentRegrowth(out maximumReached);
			value2.MaximumReached = maximumReached;
			sum.Add(container.ResourceType, value2);
		}
	}

	public int GetNoOfJobs(ResourceType resourceType)
	{
		if (Zone != null && Zone.HarvestJobs.TryGetValue(resourceType, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public void GetSumOfAllHarvestJobsInArea(Dictionary<ResourceType, ResourcesAndJobs> data)
	{
		EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
		ResourcesAndJobs resourcesAndJobs;
		IterateArea(delegate(TerrainTile tile)
		{
			if (tile.HarvestJobs != null && tile.HarvestJobs.TryGetValue(expeditionOwner.ID, out var value))
			{
				foreach (KeyValuePair<ResourceType, List<ProcessJob>> item in value)
				{
					if (item.Value.Count > 0)
					{
						int num = item.Value.Count((ProcessJob j) => Zone == null || j.HarvestJob.Zone != Zone);
						if (data.TryGetValue(item.Key, out resourcesAndJobs))
						{
							resourcesAndJobs.NumberOfJobsInOtherZones += num;
							data[item.Key] = resourcesAndJobs;
						}
						else
						{
							data.Add(item.Key, new ResourcesAndJobs
							{
								NumberOfResources = 0,
								NumberOfJobsInOtherZones = num,
								NumberOfJobsInZone = GetNoOfJobs(item.Key),
								UserChangedData = false
							});
						}
					}
				}
			}
		});
	}

	private void GetAllHarvestJobsInArea(Dictionary<ResourceType, List<ProcessJob>> allJobs)
	{
		EntityGroup expeditionOwner = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
		IterateArea(delegate(TerrainTile tile)
		{
			if (tile.HarvestJobs != null && tile.HarvestJobs.TryGetValue(expeditionOwner.ID, out var value))
			{
				foreach (KeyValuePair<ResourceType, List<ProcessJob>> item in value)
				{
					if (item.Value.Count > 0)
					{
						allJobs.Add(item.Key, item.Value);
					}
				}
			}
		});
	}

	public void GetEntitiesInArea(List<IKnownEntityData> entities, List<EntityID> entityIDs, Predicate<IKnownEntityData> entitiesToInclude, Allegiance allegianceViewpoint)
	{
		IKnownEntityData entityData = null;
		IterateArea(delegate(TerrainTile tile)
		{
			List<MemoryFact> value;
			if (tile.AllegiancesThatSeeThisTile.Contains(allegianceViewpoint))
			{
				if (tile.EntitiesOnTile != null)
				{
					foreach (Entity item in tile.EntitiesOnTile)
					{
						if (entitiesToInclude(item) && !GoalEvaluator.EntityDataResultCausesSkip(allegianceViewpoint.SharedKnowledge.GetKnownData(item.EntityID, out entityData)))
						{
							if (entities != null)
							{
								entities.Add(item);
							}
							if (entityIDs != null)
							{
								entityIDs.Add(item.EntityID);
							}
						}
					}
				}
			}
			else if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(allegianceViewpoint.SharedKnowledge, out value) && value != null)
			{
				foreach (MemoryFact item2 in value)
				{
					if (entitiesToInclude(item2))
					{
						if (entities != null)
						{
							entities.Add(item2);
						}
						if (entityIDs != null)
						{
							entityIDs.Add(item2.EntityID);
						}
					}
				}
			}
		});
		if (entities != null)
		{
			entities = entities.Distinct().ToList();
		}
		if (entityIDs != null)
		{
			entityIDs = entityIDs.Distinct().ToList();
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		BoundingRectangle = sn.DoRectangleNullable(BoundingRectangle);
		snapshotBottomLeft = sn.SnapshotID<TerrainTile, TerrainTileID>(BottomLeftTile).Value;
		snapshotBottomRight = sn.SnapshotID<TerrainTile, TerrainTileID>(BottomRightTile).Value;
		snapshotUpperLeft = sn.SnapshotID<TerrainTile, TerrainTileID>(UpperLeftTile).Value;
		snapshotUpperRight = sn.SnapshotID<TerrainTile, TerrainTileID>(UpperRightTile).Value;
		StartDragTile = sn.DoPointNullable(StartDragTile);
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotCoverage = Coverage.Select((TerrainTile t) => t.ID).ToList();
		}
		snapshotCoverage = sn.DoList(snapshotCoverage);
		sn.Ignore(BottomLeftTile);
		sn.Ignore(BottomRightTile);
		sn.Ignore(UpperLeftTile);
		sn.Ignore(UpperRightTile);
		sn.Ignore(Coverage);
		sn.Ignore(Zone);
		sn.Ignore(MapAreaRender);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		BottomLeftTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotBottomLeft);
		BottomRightTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotBottomRight);
		UpperLeftTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotUpperLeft);
		UpperRightTile = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(snapshotUpperRight);
		if (snapshotZone.HasValue)
		{
			Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone.Value);
		}
		Coverage = snapshotCoverage.Select((TerrainTileID t) => LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(t)).ToList();
		Initialize();
	}
}
