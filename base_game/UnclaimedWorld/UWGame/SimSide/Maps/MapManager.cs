using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Maps;

public class MapManager : ISnapshot
{
	[Flags]
	public enum SubtileValue : byte
	{
		Blocked = 0,
		Cost = 0x1F,
		Pad = 0x20,
		Reserved = 0x40
	}

	public enum BlockingAction
	{
		Block,
		Unblock
	}

	public enum ScanMethod
	{
		Fan,
		HalfCircle
	}

	private Vector2[] eightDirsAsVectors = new Vector2[8];

	private Vector2[] normalizedEightDirsAsVectors = new Vector2[8];

	private float abDiagonalLength;

	private float abCartesianLength;

	public const string MapsFolder = "Maps/";

	public const int MaxTerrainCost = 31;

	public Dictionary<SurfaceType.TransportType, SubtileLayers> TerrainCosts;

	public Dictionary<SurfaceType.TransportType, SubtileLayersID> snapshotTerrainCosts;

	public const float DiagonalFactor = 1.41f;

	public List<string> AllMaps;

	private Queue<NoParkingSpotTimeStamp> NoParkingSpotsTimeStamps = new Queue<NoParkingSpotTimeStamp>();

	private Dictionary<NoParkingSpot, NoParkingSpot> NoParkingSpots = new Dictionary<NoParkingSpot, NoParkingSpot>();

	private Regulator noParkingSpotsRegulator;

	public const float TerrainZLevel = 1400f;

	public const int SectorSizeInTiles = 16;

	public const int SectorSizeInSubtiles = 48;

	public const int tileSize = 48;

	public const int tileSizeOver2 = 24;

	public const float oneOverTileSize = 1f / 48f;

	public const int SubtilesPerTileLength = 3;

	public const int subTileSize = 16;

	public const int subTileSizeOver2 = 8;

	public const float oneOverSubtileSize = 0.0625f;

	public const int tileSizeOver4 = 12;

	public const int tileWidthSquared = 2304;

	private const float locationEpsilon = 0.01f;

	public Vector3 MaxWorldPos;

	public static sbyte[,] direction = new sbyte[8, 2]
	{
		{ 0, -1 },
		{ 1, 0 },
		{ 0, 1 },
		{ -1, 0 },
		{ 1, -1 },
		{ 1, 1 },
		{ -1, 1 },
		{ -1, -1 }
	};

	public static int[] oppositeDirection = new int[8] { 2, 3, 0, 1, 6, 7, 4, 5 };

	public int NoOfSectorsAcrossWidth;

	public int NoOfSectorsAcrossHeight;

	public int mapTileWidth;

	public int mapTileHeight;

	public int mapSubtileWidth;

	public int mapSubtileHeight;

	public float MapWorldWidth;

	public float MapWorldHeight;

	public TerrainTile[][] TileMap;

	private TerrainTileID[][] snapshotTileMap;

	public MethodInfo InfluenceMapTileIsFreeInfo;

	public MethodInfo InfluenceMapTileIsComfortableInfo;

	public static List<SurfaceType.TransportType> MapTransportTypeArray;

	private static int[] subtileIndexToDirectionMappings = new int[9] { 7, 0, 4, 3, -1, 1, 6, 2, 5 };

	public List<MovementMap> AllMovementMaps = new List<MovementMap>();

	private List<CyclableID> snapshotAllMovementMaps = new List<CyclableID>();

	private MapLoader mapLoader;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public RegionMap FootTerrainRegionMap => TerrainCosts[SurfaceType.TransportType.Foot].RegionMap;

	public bool IsSnapshotted { get; set; }

	public MapManager()
	{
		MapTransportTypeArray = new List<SurfaceType.TransportType>();
		foreach (object value in Enum.GetValues(typeof(SurfaceType.TransportType)))
		{
			if ((SurfaceType.TransportType)value != SurfaceType.TransportType.Air)
			{
				MapTransportTypeArray.Add((SurfaceType.TransportType)value);
			}
		}
		float num = 24f;
		float num2 = 24f;
		for (int i = 0; i < 8; i++)
		{
			eightDirsAsVectors[i] = new Vector2((float)direction[i, 0] * num, (float)direction[i, 1] * num2);
			normalizedEightDirsAsVectors[i] = eightDirsAsVectors[i];
			normalizedEightDirsAsVectors[i].Normalize();
		}
		abDiagonalLength = new Vector2(num, num2).Length();
		abCartesianLength = num2;
		if (!Snapshotter.IsSnapshotting)
		{
			InitDataNotSnapshotted();
		}
	}

	private void InitDataNotSnapshotted()
	{
		noParkingSpotsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "MapManagerNoParkingSpots");
		InfluenceMapTileIsFreeInfo = typeof(InfluenceMap).GetMethod("DiscomfortTileIsFree", BindingFlags.Static | BindingFlags.Public);
		InfluenceMapTileIsComfortableInfo = typeof(InfluenceMap).GetMethod("DiscomfortTileIsBelowValue", BindingFlags.Static | BindingFlags.Public);
	}

	public void LoadContent()
	{
	}

	public TerrainTile GetTile(int x, int y)
	{
		return TileMap[x][y];
	}

	public TerrainTile GetTile(Point pos)
	{
		return TileMap[pos.X][pos.Y];
	}

	public TerrainTile GetTile(TilePos pos)
	{
		return TileMap[pos.X][pos.Y];
	}

	public static bool IsPointReachableInStraightLine(Vector3 from, Vector3 to)
	{
		Point point = WorldPosToSubtile(from);
		Point point2 = WorldPosToSubtile(to);
		if (point != point2)
		{
			List<Point> subtilesTouchedByLine = GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
			Point point3 = subtilesTouchedByLine[0];
			SubtileLayers subtileLayers = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
			for (int i = 1; i < subtilesTouchedByLine.Count; i++)
			{
				Point point4 = subtilesTouchedByLine[i];
				if (point4 != point3 && IsBlocked(subtileLayers.GetValue(point4)))
				{
					return false;
				}
				point3 = subtilesTouchedByLine[i];
			}
		}
		return true;
	}

	public bool EntityIsInFogOfWar(Entity gameEntity, Allegiance allegiance)
	{
		return GetTile(gameEntity.MapPosition.Value).TileIsInFogOfWar(allegiance);
	}

	public bool ProcessIsInFogOfWar(SimProcess process, Allegiance allegiance)
	{
		if (process.MapPosition.HasValue)
		{
			return TileMap[process.MapPosition.Value.X][process.MapPosition.Value.Y].TileIsInFogOfWar(allegiance);
		}
		return false;
	}

	public void RegisterNoParkingSpot(EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverType, ThreatStance approach, Point destination)
	{
		NoParkingSpot noParkingSpot = new NoParkingSpot(vehicleType, protectionLevel, driverType, approach, destination);
		NoParkingSpotTimeStamp item = new NoParkingSpotTimeStamp(noParkingSpot, DateTime.Now);
		if (!NoParkingSpots.ContainsKey(noParkingSpot))
		{
			NoParkingSpotsTimeStamps.Enqueue(item);
			NoParkingSpots.Add(noParkingSpot, noParkingSpot);
		}
	}

	public bool IsNoParkingSpot(EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverEntityType, ThreatStance approach, Point destination)
	{
		NoParkingSpot key = new NoParkingSpot(vehicleType, protectionLevel, driverEntityType, approach, destination);
		return NoParkingSpots.ContainsKey(key);
	}

	private void CleanupNoParkingSpots()
	{
		int num = 5;
		while (NoParkingSpotsTimeStamps.Count > 0 && NoParkingSpotsTimeStamps.Peek().Timestamp.AddSeconds(num) < DateTime.Now)
		{
			NoParkingSpotTimeStamp noParkingSpotTimeStamp = NoParkingSpotsTimeStamps.Dequeue();
			NoParkingSpots.Remove(noParkingSpotTimeStamp.NoParkingSpot);
		}
	}

	public void Update(GameTime time)
	{
		if (The.Sim.IsPaused)
		{
			return;
		}
		if (noParkingSpotsRegulator.IsReady())
		{
			CleanupNoParkingSpots();
		}
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> terrainCost in TerrainCosts)
		{
			((TerrainRegionMap)terrainCost.Value.RegionMap).Update(time);
		}
	}

	public void UpdateWhoCanSeeEntityMovingBetweenTiles(Entity entity, Intelligence entityIntelligence, Point? from, Point? to)
	{
		TerrainTile terrainTile = null;
		if (from.HasValue)
		{
			terrainTile = GetTile(from.Value);
		}
		TerrainTile terrainTile2 = null;
		if (to.HasValue)
		{
			terrainTile2 = GetTile(to.Value);
		}
		if (terrainTile != null && terrainTile2 != null)
		{
			foreach (Allegiance item in terrainTile.AllegiancesThatSeeThisTile)
			{
				if ((entityIntelligence == null || item != entityIntelligence.Allegiance) && (terrainTile2 == null || !terrainTile2.AllegiancesThatSeeThisTile.Contains(item)))
				{
					item.SharedKnowledge.UnSeeEntity(entity);
				}
			}
		}
		if (terrainTile2 == null)
		{
			return;
		}
		foreach (Allegiance item2 in terrainTile2.AllegiancesThatSeeThisTile)
		{
			if ((entityIntelligence == null || item2 != entityIntelligence.Allegiance) && (terrainTile == null || !terrainTile.AllegiancesThatSeeThisTile.Contains(item2)) && item2.SharedKnowledge != null && !entity.RequiresRollToDetect())
			{
				item2.SharedKnowledge.SeeDetectableIfRelevant(entity);
			}
		}
	}

	public void UpdateWhoCanSeeEntityBeingContained(Entity entity, Intelligence entityIntelligence, EntityID? newContainerID)
	{
		Entity entity2 = Entity.FindByID(newContainerID);
		if (entity2 == null)
		{
			return;
		}
		TerrainTile terrainTile = null;
		if (entity.MapPosition.HasValue)
		{
			terrainTile = GetTile(entity.MapPosition.Value);
		}
		if (terrainTile == null)
		{
			return;
		}
		foreach (Allegiance item in terrainTile.AllegiancesThatSeeThisTile)
		{
			if ((entityIntelligence == null || item != entityIntelligence.Allegiance) && item.SharedKnowledge.GetKnownData(entity.ID, out var _) == EntityResult.SeenDirectly && (!item.SharedKnowledge.CanSeeInsideContainer(entity2) || item.SharedKnowledge.GetKnownData(entity2.ID, out var _) != EntityResult.SeenDirectly))
			{
				item.SharedKnowledge.UnSeeEntity(entity);
			}
		}
	}

	public int ClampTileMapXPosition(int x)
	{
		return Common.Clamp(x, 0, mapTileWidth - 1);
	}

	public int ClampTileMapYPosition(int y)
	{
		return Common.Clamp(y, 0, mapTileHeight - 1);
	}

	public Point ClampTileMapPosition(Point pos)
	{
		pos.X = Common.Clamp(pos.X, 0, mapTileWidth - 1);
		pos.Y = Common.Clamp(pos.Y, 0, mapTileHeight - 1);
		return pos;
	}

	public Point ClampSubtileMapPosition(Point pos)
	{
		pos.X = Common.Clamp(pos.X, 0, mapSubtileWidth - 1);
		pos.Y = Common.Clamp(pos.Y, 0, mapSubtileHeight - 1);
		return pos;
	}

	public Vector2 ClampWorldPosition(Vector2 pos)
	{
		pos.X = MathHelper.Clamp(pos.X, 0.01f, MaxWorldPos.X);
		pos.Y = MathHelper.Clamp(pos.Y, 0.01f, MaxWorldPos.Y);
		return pos;
	}

	public WorldLocation ClampWorldPosition(WorldLocation pos)
	{
		pos.X = MathHelper.Clamp(pos.X, 0.01f, MaxWorldPos.X);
		pos.Y = MathHelper.Clamp(pos.Y, 0.01f, MaxWorldPos.Y);
		return pos;
	}

	public Vector3 ClampWorldPosition(Vector3 position)
	{
		position.X = MathHelper.Clamp(position.X, 0.01f, MaxWorldPos.X);
		position.Y = MathHelper.Clamp(position.Y, 0.01f, MaxWorldPos.Y);
		return position;
	}

	public static Vector3 ClampWorldPositionToTile(Point tilePos, Vector3 position)
	{
		Vector2 vector = new Vector2(tilePos.X * 48, tilePos.Y * 48);
		position.X = MathHelper.Clamp(position.X, vector.X, vector.X + 48f - 0.01f);
		position.Y = MathHelper.Clamp(position.Y, vector.Y, vector.Y + 48f - 0.01f);
		return position;
	}

	public static Vector3 ClampWorldPositionToSubTile(Point subTilePos, Vector3 position)
	{
		Vector2 vector = new Vector2(subTilePos.X * 16, subTilePos.Y * 16);
		position.X = MathHelper.Clamp(position.X, vector.X, vector.X + 16f - 0.01f);
		position.Y = MathHelper.Clamp(position.Y, vector.Y, vector.Y + 16f - 0.01f);
		return position;
	}

	public static Point ClampSubTileToTile(Point subtilePos, Point tile)
	{
		Point point = TileCenterToSubTile(tile);
		return new Point(Common.Clamp(subtilePos.X, point.X - 1, point.X + 1), Common.Clamp(subtilePos.Y, point.Y - 1, point.Y + 1));
	}

	public bool TileIsOnMap(int x, int y)
	{
		if (x >= 0 && x < mapTileWidth && y >= 0)
		{
			return y < mapTileHeight;
		}
		return false;
	}

	public bool TileIsOnMap(Point tile)
	{
		if (tile.X >= 0 && tile.X < mapTileWidth && tile.Y >= 0)
		{
			return tile.Y < mapTileHeight;
		}
		return false;
	}

	public bool TileIsOnMap(TilePos tile)
	{
		if (tile.X >= 0 && tile.X < mapTileWidth && tile.Y >= 0)
		{
			return tile.Y < mapTileHeight;
		}
		return false;
	}

	public bool SubtileIsOnMap(Point subtile)
	{
		if (subtile.X >= 0 && subtile.X < mapSubtileWidth && subtile.Y >= 0)
		{
			return subtile.Y < mapSubtileHeight;
		}
		return false;
	}

	public bool WorldLocationIsOnMap(Vector2 location)
	{
		if (location.X >= 0f && location.X < MapWorldWidth && location.Y >= 0f)
		{
			return location.Y < MapWorldHeight;
		}
		return false;
	}

	public static Vector3 EdgeOfTileToWorldPos(int tileX, int tileY)
	{
		return new Vector3(48 * tileX, 48 * tileY, 0f);
	}

	public static Vector2 EdgeOfTileToWorldPos(Point tile)
	{
		return new Vector2(48 * tile.X, 48 * tile.Y);
	}

	public static Vector3 EdgeOfTileToWorldPosV3(Point tile)
	{
		return new Vector3(48 * tile.X, 48 * tile.Y, 0f);
	}

	public static Vector3 TileEdgeToWorldPos(Point tile)
	{
		return new Vector3(48 * tile.X, 48 * tile.Y, 0f);
	}

	public static Vector3 TileToWorldPos(Point tile)
	{
		return new Vector3(48 * tile.X + 24, 48 * tile.Y + 24, 0f);
	}

	public static Vector3 TilePosToWorldPos(TilePos tile)
	{
		return new Vector3(48 * tile.X + 24, 48 * tile.Y + 24, 0f);
	}

	public static Vector3 TileToWorldPos(TerrainTile tile)
	{
		return new Vector3(48 * tile.X + 24, 48 * tile.Y + 24, 0f);
	}

	public static WorldLocation TilePosToWorldLocation(TilePos tile)
	{
		return new WorldLocation(48 * tile.X + 24, 48 * tile.Y + 24, 0f);
	}

	public static Vector2 TileToWorldPosVector2(Point tile)
	{
		return new Vector2(48 * tile.X + 24, 48 * tile.Y + 24);
	}

	public static void TileToWorldPos(int tileX, int tileY, out float xPos, out float yPos)
	{
		xPos = 48 * tileX + 24;
		yPos = 48 * tileY + 24;
	}

	public static Point TileToCenterSubtile(Point tile)
	{
		return new Point((int)(3f * ((float)tile.X + 0.5f)), (int)(3f * ((float)tile.Y + 0.5f)));
	}

	public static Point TileToUpperLeftSubtile(Point tile)
	{
		return new Point(3 * tile.X, 3 * tile.Y);
	}

	public static WorldLocation VaryLocationWithinSubtile(WorldLocation location)
	{
		return new WorldLocation(VaryLocationWithinSubtile(location.ToVector3()));
	}

	public static Vector3 VaryLocationWithinSubtile(Vector3 location)
	{
		Point subTilePos = WorldPosToSubtile(location);
		location.X += The.Sim.GameplayRandomGenerator.RandomBetween(-8, 8);
		location.Y += The.Sim.GameplayRandomGenerator.RandomBetween(-8, 8);
		location = ClampWorldPositionToSubTile(subTilePos, location);
		return location;
	}

	public static Vector3 SubTileToWorldPos(PathFinderNode tile)
	{
		return new Vector3(16f * ((float)(int)tile.AbsoluteX + 0.5f), 16f * ((float)(int)tile.AbsoluteY + 0.5f), 0f);
	}

	public static Vector2 SubTileToWorldPos(Point subtile)
	{
		return new Vector2(16f * ((float)subtile.X + 0.5f), 16f * ((float)subtile.Y + 0.5f));
	}

	public static Vector3 SubTileToWorldPos3(SubtilePos subtile)
	{
		return new Vector3(16f * ((float)(int)subtile.X + 0.5f), 16f * ((float)(int)subtile.Y + 0.5f), 0f);
	}

	public static Vector3 SubTileToWorldPos3(Point subtile)
	{
		return new Vector3(16f * ((float)subtile.X + 0.5f), 16f * ((float)subtile.Y + 0.5f), 0f);
	}

	public static Vector3 SubTileEdgeToWorldPos3(Point tile)
	{
		return new Vector3(16 * tile.X, 16 * tile.Y, 0f);
	}

	public static Point SubTileToTilePos(Point subtile)
	{
		return new Point(subtile.X / 3, subtile.Y / 3);
	}

	public static Point WorldPosToTile(Vector2 pos)
	{
		return new Point((int)(1f / 48f * pos.X), (int)(1f / 48f * pos.Y));
	}

	public static Point WorldPosToTile(Vector3 pos)
	{
		return new Point((int)(1f / 48f * pos.X), (int)(1f / 48f * pos.Y));
	}

	public static TilePos WorldPosToTilePos(Vector3 pos)
	{
		return new TilePos((int)(1f / 48f * pos.X), (int)(1f / 48f * pos.Y));
	}

	public static TilePos WorldPosToTilePos(WorldLocation pos)
	{
		return new TilePos((int)(1f / 48f * pos.X), (int)(1f / 48f * pos.Y));
	}

	public static Point WorldPosToSubtile(Vector3 pos)
	{
		return new Point((int)(0.0625f * pos.X), (int)(0.0625f * pos.Y));
	}

	public static Point WorldPosToSubtile(Vector2 pos)
	{
		return new Point((int)(0.0625f * pos.X), (int)(0.0625f * pos.Y));
	}

	public static Point WorldPosToSubtile(WorldLocation pos)
	{
		return new Point((int)(0.0625f * pos.X), (int)(0.0625f * pos.Y));
	}

	public static SubtilePos WorldPosToSubtilePos(WorldLocation pos)
	{
		return new SubtilePos((ushort)(0.0625f * pos.X), (ushort)(0.0625f * pos.Y));
	}

	public static SubtilePos WorldPosToSubtilePos(Vector3 pos)
	{
		return new SubtilePos((ushort)(0.0625f * pos.X), (ushort)(0.0625f * pos.Y));
	}

	public static Point TileEdgeToSubtile(Point tile)
	{
		return new Point(tile.X * 3, tile.Y * 3);
	}

	public static SubtilePos TileEdgeToSubtile(TilePos tile)
	{
		return new SubtilePos((ushort)(tile.X * 3), (ushort)(tile.Y * 3));
	}

	public static Point TileEdgeToSubtile(int x, int y)
	{
		return new Point(x * 3, y * 3);
	}

	public static Point TileAndRelativeSubtileToAbsoluteSubtile(int x, int y, int relSubtileX, int relSubtileY)
	{
		return new Point(x * 3 + relSubtileX, y * 3 + relSubtileY);
	}

	public bool WorldLocationIsInsideMap(Vector3 location)
	{
		if (location.X >= 0.01f && location.X <= MaxWorldPos.X && location.Y >= 0.01f)
		{
			return location.Y <= MaxWorldPos.Y;
		}
		return false;
	}

	public static Vector2 WorldPosToPositionWithinTileFromCorner(Vector3 pos)
	{
		Vector2 result = default(Vector2);
		result.X = pos.X % 48f;
		result.Y = pos.Y % 48f;
		return result;
	}

	public static Vector2 WorldPosToPositionWithinTile(Vector3 pos)
	{
		Vector2 result = default(Vector2);
		result.X = pos.X % 48f - 24f;
		result.Y = pos.Y % 48f - 24f;
		return result;
	}

	public bool TileRectangleIsInsideMap(int x, int y, int widthInTiles, int heightInTiles)
	{
		if (x >= 0 && x + widthInTiles <= mapTileWidth && y >= 0)
		{
			return y + heightInTiles <= mapTileHeight;
		}
		return false;
	}

	public static bool PointIsWithinArea<T>(T[][] map, Point point)
	{
		if (point.X >= 0 && point.X < Common.GetJaggedArrayWidth(map) && point.Y >= 0)
		{
			return point.Y < Common.GetJaggedArrayHeight(map);
		}
		return false;
	}

	public bool GetClosestAccessiblePoint(SubtileLayers map, Vector3? fromLocation, Vector3 toLocation, bool stayInsideTile, out Point? closestSubtile)
	{
		Point point = WorldPosToSubtile(toLocation);
		if (!IsBlocked(map.GetValue(point)))
		{
			closestSubtile = point;
			return true;
		}
		Point? point2 = null;
		if (fromLocation.HasValue)
		{
			point2 = WorldPosToSubtile(fromLocation.Value);
		}
		Point point3;
		Point point4;
		if (stayInsideTile)
		{
			Point tile = new Point(point.X / 3, point.Y / 3);
			TileCenterToSubTile(tile);
			point3 = ClampSubTileToTile(new Point(point.X - 1, point.Y - 1), tile);
			point4 = ClampSubTileToTile(new Point(point.X + 1, point.Y + 1), tile);
		}
		else
		{
			point3 = ClampSubtileMapPosition(new Point(point.X - 1, point.Y - 1));
			point4 = ClampSubtileMapPosition(new Point(point.X + 1, point.Y + 1));
		}
		Point? point5 = null;
		float num = 100000000f;
		for (int i = point3.X; i < point4.X; i++)
		{
			for (int j = point3.Y; j < point4.Y; j++)
			{
				Point point6 = new Point(i, j);
				if (!IsBlocked(map.GetValue(i, j)))
				{
					if (!point2.HasValue)
					{
						closestSubtile = point6;
						return true;
					}
					float num2 = Common.DistanceOctile(point6, point2.Value);
					if (num2 < num)
					{
						point5 = point6;
						num = num2;
					}
				}
			}
		}
		if (point5.HasValue)
		{
			closestSubtile = point5.Value;
			return true;
		}
		closestSubtile = null;
		return false;
	}

	public Expedition GetClosestExpedition(Vector3 location)
	{
		Point tilePos = WorldPosToTile(location);
		return GetClosestExpedition(tilePos);
	}

	public Expedition GetClosestExpedition(Point tilePos)
	{
		return TileMap[tilePos.X][tilePos.Y].OperatingAreaOf;
	}

	public bool IsHuntingZone(Allegiance allegiance, Point mapPosition)
	{
		return GetTile(mapPosition).GetListOfZones(allegiance)?.Exists((Zone z) => z.ZoneHunt.HasFindPreyJobs()) ?? false;
	}

	public static bool IsStandingOnNonMovingEntity(Entity entity)
	{
		TerrainTile tile = The.Map.GetTile(entity.MapPosition.Value);
		Point entitySubtile = WorldPosToSubtile(entity.PlaySiteLocation);
		return SubtileHasNonMovingIntelligentEntity(entity, tile, entitySubtile);
	}

	public static bool SubtileHasNonMovingIntelligentEntity(Entity entity, TerrainTile tile, Point entitySubtile)
	{
		if (tile.EntitiesOnTile != null)
		{
			foreach (Entity item in tile.EntitiesOnTile)
			{
				if (item != entity && item.IsIntelligentAndNonMoving() && entitySubtile == WorldPosToSubtile(item.PlaySiteLocation))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Point? FindUnoccupiedSubtileInsideTile(Entity entity, Point tilePos)
	{
		TerrainTile tile = The.Map.TileMap[tilePos.X][tilePos.Y];
		SubtileLayers mapCosts = TerrainCosts[SurfaceType.TransportType.Foot];
		Point point = TileCenterToSubTile(tilePos);
		if (!SubtileIsCompletelyBlocked(mapCosts, point) && !SubtileHasNonMovingIntelligentEntity(entity, tile, point))
		{
			return point;
		}
		Point point2 = TileToUpperLeftSubtile(tilePos);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				point = new Point(point2.X + i, point2.Y + j);
				if (!SubtileIsCompletelyBlocked(mapCosts, point) && !SubtileHasNonMovingIntelligentEntity(entity, tile, point))
				{
					return point;
				}
			}
		}
		return null;
	}

	public void InitPostLoadMap()
	{
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> terrainCost in TerrainCosts)
		{
			while (!terrainCost.Value.RegionMap.CycleOnce())
			{
			}
		}
	}

	public void Init()
	{
		InitMaps();
	}

	public static string ComposeMapDataXmlFilePathFromFolderPath(string folderPath)
	{
		return Path.Combine(folderPath, "MapData.xml");
	}

	public static string ComposeMapDataXmlFilePath(string name)
	{
		return Config.GetDataFolderPath(Config.DataType.RGMap, name, "MapData.xml");
	}

	public static string ComposeMapDataFolderPath(string folderName, string fileOrFolderName = "")
	{
		return Path.Combine(Config.GetDataFolderPath(Config.DataType.RGMap), folderName, fileOrFolderName);
	}

	private void InitMaps()
	{
		List<DirectoryInfo> listOfMapFolders = MapEditorSaveLoadPanel.GetListOfMapFolders();
		AllMaps = listOfMapFolders.Select((DirectoryInfo d) => d.Name).ToList();
	}

	public void SaveMap(string fullFolderPath, MapData mapData, bool createNewFolders)
	{
		mapData.Dimensions.X = mapTileWidth;
		mapData.Dimensions.Y = mapTileHeight;
		if (mapData.SavedMapEntities != null)
		{
			mapData.SavedMapEntities.Clear();
		}
		if (mapData.Trees != null)
		{
			mapData.Trees.Clear();
		}
		if (mapData.Tiles != null)
		{
			mapData.Tiles.Clear();
		}
		for (int i = 0; i < mapTileWidth; i++)
		{
			for (int j = 0; j < mapTileHeight; j++)
			{
				TerrainTile terrainTile = TileMap[i][j];
				EditorData c;
				UWGame.SimSide.Trees.Tree c2;
				if (terrainTile.EntitiesOnTile != null)
				{
					foreach (Entity item4 in terrainTile.EntitiesOnTile)
					{
						if (item4.Find<EditorData>(out c) && !item4.Find<UWGame.SimSide.Trees.Tree>(out c2))
						{
							if (mapData.SavedMapEntities == null)
							{
								mapData.SavedMapEntities = new List<EntityData>();
							}
							EntityData item = CreateEntityDataFromEntity(item4);
							mapData.SavedMapEntities.Add(item);
						}
					}
				}
				if (terrainTile.TreesOnTile != null)
				{
					foreach (Entity item5 in terrainTile.TreesOnTile)
					{
						if (item5.Find<EditorData>(out c) && item5.Find<UWGame.SimSide.Trees.Tree>(out c2))
						{
							if (mapData.Trees == null)
							{
								mapData.Trees = new List<EntityData>();
							}
							EntityData item2 = CreateEntityDataFromEntity(item5);
							mapData.Trees.Add(item2);
						}
					}
				}
				if (terrainTile.DesignerPlacedResources != null)
				{
					if (mapData.Tiles == null)
					{
						mapData.Tiles = new List<Tile>();
					}
					Tile item3 = new Tile
					{
						Resources = terrainTile.DesignerPlacedResources,
						Position = new Point(i, j)
					};
					mapData.Tiles.Add(item3);
				}
			}
		}
		if (!Directory.Exists(fullFolderPath))
		{
			Directory.CreateDirectory(fullFolderPath);
		}
		XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
		xmlSerializerNamespaces.Add("", "");
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(MapData));
		using (TextWriter textWriter = new StreamWriter(ComposeMapDataXmlFilePathFromFolderPath(fullFolderPath)))
		{
			xmlSerializer.Serialize(textWriter, mapData, xmlSerializerNamespaces);
		}
		if (createNewFolders)
		{
			Directory.CreateDirectory(Path.Combine(fullFolderPath, "Trees"));
			Directory.CreateDirectory(Path.Combine(fullFolderPath, "Vegetation"));
			Directory.CreateDirectory(Path.Combine(fullFolderPath, "Soil"));
		}
	}

	private static EntityData CreateEntityDataFromEntity(Entity entity)
	{
		entity.Find<EditorData>(out var c);
		EntityData entityData = new EntityData
		{
			Name = entity.Name,
			EntityKey = entity.EntityType.KeyName,
			Location = entity.Location,
			FlipHorizontally = entity.FlipHorizontally
		};
		if (!entity.Find<UWGame.SimSide.Trees.Tree>(out var c2))
		{
			entityData.Bulk = entity.Bulk;
		}
		if (entity.Renderable.RenderAsModel != null)
		{
			entityData.Rotation = MathHelper.ToDegrees(entity.Rotation);
		}
		if (c2 != null)
		{
			entityData.Tree = new UWGame.SimSide.Maps.MapEditor.Tree();
			if (c.TreeAgeGroup.HasValue)
			{
				entityData.Tree.AgeGroup = c.TreeAgeGroup.Value;
			}
			else
			{
				entityData.Tree.AgeInYears = c2.AgeInYears;
			}
			entityData.Tree.ShapeFactor = c2.ShapeFactor;
			entityData.Tree.InSeason = c2.InSeason;
			entityData.Tree.Flavour = c2.Flavour;
		}
		if (entity.Find<UWGame.SimSide.Entities.Rock>(out var _))
		{
			entityData.Rock = new UWGame.SimSide.Maps.MapEditor.Rock();
		}
		if (entity.Find<UWGame.SimSide.Entities.Biological.BiologicalEntity>(out var c4))
		{
			entityData.BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
			{
				AgeInYears = new NormalDistribution
				{
					Mean = c4.AgeGroup.Age
				},
				CasteKey = c4.CasteType.Name,
				RaceKey = c4.RaceType.Name
			};
			if (c != null)
			{
				entityData.MemberOf = new AllegianceAndExpedition
				{
					AllegianceKey = c.AllegianceKey,
					ExpeditionKey = c.ExpeditionName
				};
			}
		}
		if (c != null)
		{
			entityData.Resources = c.Resources;
		}
		return entityData;
	}

	public void StartLoadMap(DirectoryInfo mapFolder)
	{
		mapLoader = new MapLoader(mapFolder);
	}

	public void StartLoadMap(string mapFolderName)
	{
		mapLoader = new MapLoader(mapFolderName);
	}

	public bool LoadMapQueued()
	{
		if (mapLoader.QueueLoad())
		{
			mapLoader = null;
			return true;
		}
		return false;
	}

	public void SetDimensions(Point dimensions)
	{
		mapTileWidth = dimensions.X;
		mapTileHeight = dimensions.Y;
		mapSubtileWidth = 3 * mapTileWidth;
		mapSubtileHeight = 3 * mapTileHeight;
		MapWorldWidth = mapTileWidth * 48;
		MapWorldHeight = mapTileHeight * 48;
		NoOfSectorsAcrossWidth = (int)Math.Ceiling((double)mapSubtileWidth / 48.0);
		NoOfSectorsAcrossHeight = (int)Math.Ceiling((double)mapSubtileHeight / 48.0);
		float x = MapWorldWidth - 0.01f;
		float y = MapWorldHeight - 0.01f;
		MaxWorldPos = new Vector3(x, y, 10000f);
	}

	public bool InitCollisionTrees()
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			int maxCollidablesPerNode = 999;
			int maxCollidablesPerNode2 = 9;
			The.CollisionManager = new CollisionManager<Entity>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxCollidablesPerNode);
			The.AgentQuadTree = new PointQuadTree<Entity>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), maxCollidablesPerNode2, 7);
		}
		return true;
	}

	public void BlockSubtile(Vector3 location, SurfaceType.TransportType transport, BlockingAction blockingAction)
	{
		Point subtile = WorldPosToSubtile(location);
		BlockTerrainSubtile(subtile, transport, blockingAction);
	}

	public void BlockTerrainSubtile(Point subtile, SurfaceType.TransportType transport, BlockingAction blockingAction)
	{
		if (The.Map.SubtileIsOnMap(subtile))
		{
			if (blockingAction == BlockingAction.Block)
			{
				SubtileValue value = SetCost(TerrainCosts[transport].GetValue(subtile), 0);
				TerrainCosts[transport].SetValueOnBottomLayer(subtile, value);
			}
			else
			{
				SubtileValue value2 = SetCost(TerrainCosts[transport].GetValue(subtile), PlainsType.Instance.Cost(transport, SurfaceType.TerrainFeatures.None));
				TerrainCosts[transport].SetValueOnBottomLayer(subtile, value2);
			}
			PropagateChangeToDependentMaps(subtile, transport, blockingAction == BlockingAction.Block);
		}
	}

	private void PropagateChangeToDependentMaps(Point subtile, SurfaceType.TransportType transport, bool isBlocking)
	{
		MarkDirtyTerrainMapRegion(transport, subtile);
		ChangeMovementMaps(subtile, transport, isBlocking);
	}

	public void IterateTileArea(Rectangle tileArea, Action<TerrainTile> iterateMethod)
	{
		for (int i = tileArea.Left; i < tileArea.Right; i++)
		{
			for (int j = tileArea.Top; j < tileArea.Bottom; j++)
			{
				TerrainTile obj = TileMap[i][j];
				iterateMethod(obj);
			}
		}
	}

	public static void IterateSubtiles(Vector2 from, Vector2 to, Action<Vector2> iterateMethod)
	{
		float num = 16f;
		for (float num2 = from.X; num2 <= to.X; num2 += num)
		{
			for (float num3 = from.Y; num3 <= to.Y; num3 += num)
			{
				Vector2 obj = new Vector2(num2, num3);
				iterateMethod(obj);
			}
		}
	}

	public static bool IterateSubtilesBreakOnTrue(Vector2 from, Vector2 to, Predicate<Vector2> iterateMethod)
	{
		float num = 16f;
		for (float num2 = from.X; num2 <= to.X; num2 += num)
		{
			for (float num3 = from.Y; num3 <= to.Y; num3 += num)
			{
				Vector2 obj = new Vector2(num2, num3);
				if (iterateMethod(obj))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ChangeMovementMaps(Point subtile, SurfaceType.TransportType transport, bool isBlocking)
	{
		if (AllMovementMaps == null)
		{
			return;
		}
		foreach (MovementMap allMovementMap in AllMovementMaps)
		{
			allMovementMap.SetTerrainSectorDirty(transport, subtile);
		}
	}

	private void MarkDirtyTerrainMapRegion(SurfaceType.TransportType transport, Point subtile)
	{
		RegionMap regionMap = TerrainCosts[transport].RegionMap;
		if (regionMap != null && regionMap.RegionGraph.Count > 0)
		{
			regionMap.MarkDirtySector(subtile.X, subtile.Y);
		}
	}

	public void SetSubtileTerrainCost(Vector3 location, byte newCost)
	{
		foreach (SurfaceType.TransportType item in MapTransportTypeArray)
		{
			SetSubtileCost(location, item, newCost);
		}
	}

	public void SetSubtileTerrainCost(Point subtilePos, byte newCost)
	{
		foreach (SurfaceType.TransportType item in MapTransportTypeArray)
		{
			SetSubtileCost(subtilePos, item, newCost);
		}
	}

	public void SetSubtileTerrainValueFlag(Point subtilePos, SubtileValue flagToSet)
	{
		foreach (SurfaceType.TransportType item in MapTransportTypeArray)
		{
			SubtileLayers subtileLayers = TerrainCosts[item];
			subtileLayers.SetValueOnBottomLayer(subtilePos, subtileLayers.GetValue(subtilePos) | flagToSet);
		}
	}

	public void ClearSubtileTerrainValueFlag(Point subtilePos, SubtileValue flagToSet)
	{
		foreach (SurfaceType.TransportType item in MapTransportTypeArray)
		{
			SubtileLayers subtileLayers = TerrainCosts[item];
			subtileLayers.SetValueOnBottomLayer(subtilePos, (SubtileValue)((uint)subtileLayers.GetValue(subtilePos) & (uint)(byte)(~(int)flagToSet)));
		}
	}

	public bool FlagIsSet(SubtilePos subtilePos, SurfaceType.TransportType transport, SubtileValue flagToTest)
	{
		return TestForFlag(TerrainCosts[transport].GetValue(subtilePos), flagToTest);
	}

	public bool FlagIsSet(Point subtilePos, SurfaceType.TransportType transport, SubtileValue flagToTest)
	{
		return TestForFlag(TerrainCosts[transport].GetValue(subtilePos), flagToTest);
	}

	public static bool TestForFlag(SubtileValue valueToTest, SubtileValue flagToTest)
	{
		return (valueToTest & flagToTest) != 0;
	}

	public void SetSubtileTerrainCostToSurfaceType(Vector3 location)
	{
		SetSubtileCostToSurfaceType(WorldPosToSubtile(location));
	}

	public void SetSubtileCostToSurfaceType(Point subTilePos)
	{
		SurfaceType surfaceType = GetSurfaceType(subTilePos);
		foreach (SurfaceType.TransportType item in MapTransportTypeArray)
		{
			SetSubtileCost(subTilePos, item, surfaceType.Cost(item, SurfaceType.TerrainFeatures.None));
		}
	}

	public SurfaceType GetSurfaceType(Point subtilePos)
	{
		return GetTerrain(subtilePos).SurfaceType;
	}

	public float GetRoughness(Point subtilePos)
	{
		TerrainTile tile = GetTile(SubTileToTilePos(subtilePos));
		float roughness = tile.GetTerrain(subtilePos).GetRoughness();
		float num = 0f;
		if (tile.TreesOnTile != null && tile.TreesOnTile.Any((Entity t) => WorldPosToSubtile(t.PlaySiteLocation) == subtilePos))
		{
			num = 0.9f;
		}
		return Common.ClampTop(roughness + num, 1f);
	}

	public Terrain GetTerrain(Point subtilePos)
	{
		return GetTile(SubTileToTilePos(subtilePos)).GetTerrain(subtilePos);
	}

	public void SetSubtileCost(Point subtilePosition, SurfaceType.TransportType transport, byte newCost)
	{
		if (subtilePosition.X < 0)
		{
			subtilePosition.X = 0;
		}
		if (subtilePosition.Y < 0)
		{
			subtilePosition.Y = 0;
		}
		if (newCost == 0)
		{
			BlockTerrainSubtile(subtilePosition, transport, BlockingAction.Block);
		}
		else
		{
			if (GetCost(TerrainCosts[transport].GetValue(subtilePosition)) <= 0)
			{
				BlockTerrainSubtile(subtilePosition, transport, BlockingAction.Unblock);
			}
			SubtileValue value = SetCost(TerrainCosts[transport].GetValue(subtilePosition.X, subtilePosition.Y), newCost);
			TerrainCosts[transport].SetValueOnBottomLayer(subtilePosition.X, subtilePosition.Y, value);
		}
		PropagateChangeToDependentMaps(subtilePosition, transport, newCost == 0);
	}

	public void SetSubtileCost(Vector3 location, SurfaceType.TransportType transport, byte newCost)
	{
		Point subtilePosition = WorldPosToSubtile(location);
		SetSubtileCost(subtilePosition, transport, newCost);
	}

	public int GetDirectionIndex(Point from, Point to)
	{
		int num = to.X - from.X;
		int num2 = to.Y - from.Y;
		for (int i = 0; i < 8; i++)
		{
			if (direction[i, 0] == num && direction[i, 1] == num2)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool DetectIsAtCornerOfTile(float xRelative, float yRelative)
	{
		if (xRelative + yRelative < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
		{
			return true;
		}
		if (48f - xRelative + yRelative < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
		{
			return true;
		}
		if (xRelative + (48f - yRelative) < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
		{
			return true;
		}
		if (48f - xRelative + (48f - yRelative) < GameData.Instance.AIConstants.DistanceToConsiderOnRoad)
		{
			return true;
		}
		return false;
	}

	public static bool DetectIsAtCenterOfTile(float xRelative, float yRelative)
	{
		if (Math.Abs(xRelative - 24f) < 4f && Math.Abs(yRelative - 24f) < 4f)
		{
			return true;
		}
		return false;
	}

	public static bool DetectIsAtCenterOfTile(Vector3 pos)
	{
		float num = pos.X % 48f;
		float num2 = pos.Y % 48f;
		if (Math.Abs(num - 24f) < 4f && Math.Abs(num2 - 24f) < 4f)
		{
			return true;
		}
		return false;
	}

	public Common.Direction WorldLocationToDirectionWithinTile(Vector3 location)
	{
		Vector2 value = WorldPosToPositionWithinTile(location);
		if (Math.Abs(value.X - 24f) < 2f && Math.Abs(value.Y - 24f) < 2f)
		{
			return (Common.Direction)The.Sim.GameplayRandomGenerator.Next(8, "MapManager");
		}
		float num = 0f;
		Common.Direction result = Common.Direction.North;
		for (int i = 0; i < 8; i++)
		{
			float num2 = Vector2.Dot(normalizedEightDirsAsVectors[i], value);
			if (num2 > num)
			{
				num = num2;
				result = (Common.Direction)i;
			}
		}
		return result;
	}

	public bool DetectMovementAlong8Dir(Vector3 location, Vector2 vectorToWaypoint, ref Common.Direction direction)
	{
		Vector2 value = default(Vector2);
		value.X = location.X % 48f;
		value.Y = location.Y % 48f;
		Vector2 vector = new Vector2(24f, 24f);
		if (DetectIsAtCenterOfTile(value.X, value.Y))
		{
			value += vectorToWaypoint * 12f;
		}
		else if (DetectIsAtCornerOfTile(value.X, value.Y))
		{
			value += vectorToWaypoint * 12f;
			value.X %= 48f;
			value.Y %= 48f;
		}
		float num = 0.1f;
		for (int i = 0; i < 8; i++)
		{
			Vector2 vector2 = normalizedEightDirsAsVectors[i];
			float num2 = Vector2.Dot(value, vector2) - Vector2.Dot(vector, vector2);
			if (num2 >= 0f && num2 <= ((i < 4) ? abCartesianLength : abDiagonalLength))
			{
				Vector2 value2 = vector + vector2 * num2;
				if (Vector2.DistanceSquared(value, value2) < GameData.Instance.AIConstants.DistanceSquaredToConsiderOnRoad && ((Math.Abs(vectorToWaypoint.X - normalizedEightDirsAsVectors[i].X) < num && Math.Abs(vectorToWaypoint.Y - normalizedEightDirsAsVectors[i].Y) < num) || (Math.Abs(vectorToWaypoint.X + normalizedEightDirsAsVectors[i].X) < num && Math.Abs(vectorToWaypoint.Y + normalizedEightDirsAsVectors[i].Y) < num)))
				{
					direction = (Common.Direction)i;
					return true;
				}
			}
		}
		return false;
	}

	public bool TileIsAccessible(SubtileLayers map, Point from, Point to)
	{
		if (!TileIsOnMap(to))
		{
			return false;
		}
		if (The.Map.TileIsCompletelyBlocked(map, to))
		{
			return false;
		}
		return true;
	}

	public bool SubtileIsCompletelyBlocked(SubtileLayers mapCosts, Point p)
	{
		return IsBlocked(mapCosts.GetValue(p));
	}

	public bool SubtileIsCompletelyBlocked(SubtileLayers mapCosts, SubtilePos p)
	{
		return IsBlocked(mapCosts.GetValue(p));
	}

	public bool SubtileIsOrAdjacentToBlockedSubtile(SubtileLayers mapCosts, Point p)
	{
		if (p.Y < 1 || p.X < 1)
		{
			return true;
		}
		if (!IsBlocked(mapCosts.GetValue(--p.X, p.Y - 1)) && !IsBlocked(mapCosts.GetValue(p.X, p.Y)) && !IsBlocked(mapCosts.GetValue(p.X, p.Y + 1)) && !IsBlocked(mapCosts.GetValue(++p.X, p.Y - 1)) && !IsBlocked(mapCosts.GetValue(p.X, p.Y)) && !IsBlocked(mapCosts.GetValue(p.X, p.Y + 1)) && !IsBlocked(mapCosts.GetValue(++p.X, p.Y - 1)) && !IsBlocked(mapCosts.GetValue(p.X, p.Y)))
		{
			return IsBlocked(mapCosts.GetValue(p.X, p.Y + 1));
		}
		return true;
	}

	public static bool IsBlocked(SubtileValue value)
	{
		return (value & SubtileValue.Cost) == 0;
	}

	public static byte GetCost(SubtileValue value)
	{
		return (byte)(value & SubtileValue.Cost);
	}

	public static string TilePosToString(Point tile)
	{
		return "[" + tile.X + "," + tile.Y + "]";
	}

	public static SubtileValue SetCost(SubtileValue value, byte cost)
	{
		value = (SubtileValue)((uint)(value & ~SubtileValue.Cost) | (uint)cost);
		return value;
	}

	public bool TileIsCompletelyBlocked(SubtileLayers mapCosts, Point p)
	{
		Point point = TileToUpperLeftSubtile(p);
		int x = point.X;
		int y = point.Y;
		if (IsBlocked(mapCosts.GetValue(x, y)) && IsBlocked(mapCosts.GetValue(x, y + 1)) && IsBlocked(mapCosts.GetValue(x, y + 2)) && IsBlocked(mapCosts.GetValue(++x, y)) && IsBlocked(mapCosts.GetValue(x, y + 1)) && IsBlocked(mapCosts.GetValue(x, y + 2)) && IsBlocked(mapCosts.GetValue(++x, y)) && IsBlocked(mapCosts.GetValue(x, y + 1)))
		{
			return IsBlocked(mapCosts.GetValue(x, y + 2));
		}
		return false;
	}

	public static Rectangle GetClampedMapAreaUsingTiles(TilePos tilePosition, int radius, out int minX, out int maxX, out int minY, out int maxY)
	{
		minX = Math.Max(0, tilePosition.X - radius);
		minY = Math.Max(0, tilePosition.Y - radius);
		maxX = Math.Min(The.Map.mapTileWidth - 1, tilePosition.X + radius);
		maxY = Math.Min(The.Map.mapTileHeight - 1, tilePosition.Y + radius);
		return new Rectangle(minX, minY, maxX - minX, maxY - minY);
	}

	public Rectangle GetClampedMapAreaUsingTiles(Point tilePosition, int widthInTiles, int heightInTiles)
	{
		int num = Math.Max(0, tilePosition.X);
		int num2 = Math.Max(0, tilePosition.Y);
		int num3 = Math.Min(mapTileWidth - 1, tilePosition.X + widthInTiles);
		int num4 = Math.Min(mapTileHeight - 1, tilePosition.Y + heightInTiles);
		return new Rectangle(num, num2, num3 - num, num4 - num2);
	}

	public Rectangle GetClampedMapAreaUsingSubTiles(Point subtilePosition, int widthInSubTiles, int heightInSubTiles)
	{
		int num = Math.Max(0, subtilePosition.X);
		int num2 = Math.Max(0, subtilePosition.Y);
		int num3 = Math.Min(mapSubtileWidth - 1, subtilePosition.X + widthInSubTiles);
		int num4 = Math.Min(mapSubtileHeight - 1, subtilePosition.Y + heightInSubTiles);
		return new Rectangle(num, num2, num3 - num, num4 - num2);
	}

	public Rectangle GetClampedMapAreaUsingTiles(Rectangle area)
	{
		return GetClampedMapAreaUsingTiles(new Point(area.X, area.Y), area.Width, area.Height);
	}

	public static Rectangle GetClampedRectangularMapAreaUsingSubtiles(Point topLeftSubtilePosition, int widthInSubtiles, int heightInSubtiles, out int minX, out int maxX, out int minY, out int maxY)
	{
		minX = Math.Max(0, topLeftSubtilePosition.X);
		minY = Math.Max(0, topLeftSubtilePosition.Y);
		maxX = Math.Min(The.Map.mapSubtileWidth - 1, topLeftSubtilePosition.X + widthInSubtiles);
		maxY = Math.Min(The.Map.mapSubtileHeight - 1, topLeftSubtilePosition.Y + heightInSubtiles);
		return new Rectangle(minX, minY, maxX - minX, maxY - minY);
	}

	public static Rectangle GetClampedRectangularMapAreaUsingSubtiles(Point topLeftSubtilePosition, int widthInSubtiles, int heightInSubtiles)
	{
		int num = Math.Max(0, topLeftSubtilePosition.X);
		int num2 = Math.Max(0, topLeftSubtilePosition.Y);
		int num3 = Math.Min(The.Map.mapSubtileWidth - 1, topLeftSubtilePosition.X + widthInSubtiles);
		int num4 = Math.Min(The.Map.mapSubtileHeight - 1, topLeftSubtilePosition.Y + heightInSubtiles);
		return new Rectangle(num, num2, num3 - num, num4 - num2);
	}

	public static Vector3 GetWorldCoordsFromDirection(Point mapPosition, Common.Direction dir)
	{
		Point point = DirectionToRelativeSubtile(dir);
		Vector3 result = EdgeOfTileToWorldPos(mapPosition.X, mapPosition.Y);
		result.X += (float)((double)(float)point.X + 0.5) * 16f;
		result.Y += (float)((double)(float)point.Y + 0.5) * 16f;
		return result;
	}

	public static Vector3 TileAndDirectionToWorldPos(Point tilePos, Common.Direction edge)
	{
		Vector3 result = TileToWorldPos(tilePos);
		Point point = DirectionToRelativeSubtile(edge);
		result.X += point.X * 16 + 8;
		result.Y += point.Y * 16 + 8;
		return result;
	}

	public static Point DirectionToRelativeSubtile(Common.Direction dir)
	{
		return dir switch
		{
			Common.Direction.East => new Point(2, 1), 
			Common.Direction.North => new Point(1, 0), 
			Common.Direction.West => new Point(0, 1), 
			Common.Direction.South => new Point(1, 2), 
			Common.Direction.NorthEast => new Point(2, 0), 
			Common.Direction.SouthEast => new Point(2, 2), 
			Common.Direction.SouthWest => new Point(0, 2), 
			Common.Direction.NorthWest => new Point(0, 0), 
			_ => new Point(1, 0), 
		};
	}

	public static void SubtileAndTilePosToWorldPos(Point bestSubtilePoint, Point relativeToTile, out Vector3 foundLocation)
	{
		foundLocation = SubTileToWorldPos3(bestSubtilePoint) + EdgeOfTileToWorldPosV3(relativeToTile);
	}

	public static Vector3 SubtileAndTilePosToWorldPos(Point bestSubtilePoint, Point relativeToTile)
	{
		return SubTileToWorldPos3(bestSubtilePoint) + EdgeOfTileToWorldPosV3(relativeToTile);
	}

	public static Point TileCenterToSubTile(Point tile)
	{
		return new Point(tile.X * 3 + 1, tile.Y * 3 + 1);
	}

	public static SubtilePos TileCenterToSubTile(TilePos tile)
	{
		return new SubtilePos((ushort)(tile.X * 3 + 1), (ushort)(tile.Y * 3 + 1));
	}

	public static int GetLocalSubtileIndex(int subtileX, int subtileY)
	{
		return subtileX % 3 + 3 * (subtileY % 3);
	}

	public static Point WorldPosToRelativeSubtile(Vector3 location, Vector3 relativeTo)
	{
		return new Point((int)((location.X - relativeTo.X) / 16f), (int)((location.Y - relativeTo.Y) / 16f));
	}

	public static Point WorldPosToRelativeSubtile(Vector3 location)
	{
		return new Point((int)(location.X * 0.0625f) % 3, (int)(location.Y * 0.0625f) % 3);
	}

	public static Point RelativePosToRelativeSubtile(Vector2 location)
	{
		return new Point((int)(location.X / 16f), (int)(location.Y / 16f));
	}

	public static List<Point> GetSubtilesTouchedByLine(Vector2 from, Vector2 to)
	{
		List<Point> list = new List<Point>();
		float tileWidth = 16f;
		float tileHeight = 16f;
		return GetSquaresTouchedByLine(ref from, ref to, list, tileWidth, tileHeight);
	}

	public static List<Point> GetTilesTouchedByLine(Vector2 from, Vector2 to)
	{
		List<Point> list = new List<Point>();
		float tileWidth = 48f;
		float tileHeight = 48f;
		return GetSquaresTouchedByLine(ref from, ref to, list, tileWidth, tileHeight);
	}

	private static List<Point> GetSquaresTouchedByLine(ref Vector2 from, ref Vector2 to, List<Point> list, float tileWidth, float tileHeight)
	{
		Vector2 vector = to - from;
		int num = Math.Sign(vector.X);
		int num2 = Math.Sign(vector.Y);
		vector.Normalize();
		int num3 = (int)(from.X / tileWidth);
		int num4 = (int)(from.Y / tileHeight);
		list.Add(new Point(num3, num4));
		int num5 = (int)(to.X / tileWidth);
		int num6 = (int)(to.Y / tileHeight);
		float num7 = Math.Abs(tileWidth / vector.X);
		float num8 = Math.Abs(tileHeight / vector.Y);
		float num9 = ((!(vector.X < 0f)) ? Math.Abs((tileWidth - from.X % tileWidth) / vector.X) : Math.Abs(from.X % tileWidth / vector.X));
		float num10 = ((!(vector.Y < 0f)) ? Math.Abs((tileHeight - from.Y % tileHeight) / vector.Y) : Math.Abs(from.Y % tileHeight / vector.Y));
		if (num5 != num3 || num6 != num4)
		{
			do
			{
				if (num9 < num10)
				{
					num9 += num7;
					num3 += num;
					list.Add(new Point(num3, num4));
				}
				else if (num9 == num10)
				{
					num9 += num7;
					num3 += num;
					num10 += num8;
					num4 += num2;
					list.Add(new Point(num3, num4));
				}
				else
				{
					num10 += num8;
					num4 += num2;
					list.Add(new Point(num3, num4));
				}
			}
			while (list.Count() <= 20000 && (num3 != num5 || num4 != num6));
		}
		return list;
	}

	public static bool IsPathClearToPoint(Vector3 from, Vector3 to, MovementMap moveMap, SurfaceType.TransportType transport)
	{
		List<Point> subtilesTouchedByLine = GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
		Point point = subtilesTouchedByLine[0];
		SubtileLayers subtileLayers = moveMap.Layers[transport];
		for (int i = 1; i < subtilesTouchedByLine.Count; i++)
		{
			Point point2 = subtilesTouchedByLine[i];
			if (point2 != point && IsBlocked(subtileLayers.GetValue(point2)))
			{
				return false;
			}
		}
		return true;
	}

	public static Vector3 FindFreeLocation(Vector3 location, bool avoidBlockedAreas, Entity.AddRandomOffset addRandomOffset, Entity creatorOfItem)
	{
		if (addRandomOffset == Entity.AddRandomOffset.Yes)
		{
			location = VaryLocationWithinSubtile(location);
		}
		if (avoidBlockedAreas)
		{
			if (addRandomOffset == Entity.AddRandomOffset.No && creatorOfItem != null && creatorOfItem.Location == location)
			{
				Vector3 vector = Common.AngleToVector((float)Math.PI / 2f + creatorOfItem.Rotation).ToVector3();
				location += vector * 30f;
			}
			location = The.Map.ClampWorldPosition(location);
			Point point = WorldPosToSubtile(location);
			SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
			if (The.Map.SubtileIsCompletelyBlocked(mapCosts, point) || The.Map.FlagIsSet(point, SurfaceType.TransportType.Foot, SubtileValue.Reserved))
			{
				location = ((creatorOfItem == null) ? FindUnblockedLocation(location, 50f, ScanMethod.Fan, null, avoidReservedSubtiles: true) : FindUnblockedLocation(location, 50f, ScanMethod.HalfCircle, creatorOfItem.Location, avoidReservedSubtiles: true));
			}
		}
		return location;
	}

	public static Vector3 FindUnblockedLocation(Vector3 locationToComputeFrom, float maxRadius, ScanMethod scanMethod = ScanMethod.Fan, Vector3? locationToStriveTowards = null, bool avoidReservedSubtiles = false, Predicate<SubtilePos> subtileIsValid = null, float? radiusIncrements = null)
	{
		Vector3 directionToLookForUnblockedLocationIn;
		if (!locationToStriveTowards.HasValue)
		{
			directionToLookForUnblockedLocationIn = new Vector3(0f, 1f, 0f);
		}
		else
		{
			directionToLookForUnblockedLocationIn = locationToStriveTowards.Value - locationToComputeFrom;
			directionToLookForUnblockedLocationIn.Normalize();
		}
		SubtileLayers terrain = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		Vector3? vector = null;
		vector = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, maxRadius, subtileIsValid);
		if (!vector.HasValue)
		{
			switch (scanMethod)
			{
			case ScanMethod.Fan:
				FanScan(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref vector, avoidReservedSubtiles, maxRadius, subtileIsValid, radiusIncrements);
				break;
			case ScanMethod.HalfCircle:
				HalfCircleScan(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref vector, avoidReservedSubtiles, maxRadius, subtileIsValid);
				break;
			}
		}
		if (vector.HasValue)
		{
			return vector.Value;
		}
		return locationToComputeFrom;
	}

	private static void HalfCircleScan(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint, bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null)
	{
		Vector3 vector = directionToLookForUnblockedLocationIn;
		int num = 5;
		float num2 = (float)Math.PI / 2f / (float)num;
		float originalRotation = (float)Math.Acos(vector.X);
		float num3 = 0f;
		int num4 = 0;
		do
		{
			num3 += num2;
			ScanWithRotation(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, originalRotation, num3, avoidReservedSubtiles, maxRadius, subtileIsValid);
			if (!foundPoint.HasValue)
			{
				ScanWithRotation(ref locationToComputeFrom, ref directionToLookForUnblockedLocationIn, terrain, ref foundPoint, originalRotation, 0f - num3, avoidReservedSubtiles, maxRadius, subtileIsValid);
				if (!foundPoint.HasValue)
				{
					num4++;
					continue;
				}
				break;
			}
			break;
		}
		while (num4 < num);
	}

	private static void ScanWithRotation(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint, float originalRotation, float currentRadianDifference, bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null)
	{
		directionToLookForUnblockedLocationIn = Common.AngleToVector(originalRotation + currentRadianDifference).ToVector3();
		foundPoint = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, maxRadius, subtileIsValid);
	}

	private static void FanScan(ref Vector3 locationToComputeFrom, ref Vector3 directionToLookForUnblockedLocationIn, SubtileLayers terrain, ref Vector3? foundPoint, bool avoidReservedSubtiles, float maxRadius, Predicate<SubtilePos> subtileIsValid = null, float? radiusIncrements = null)
	{
		float num = 0f;
		do
		{
			float num2 = 1f;
			float num3 = 1f;
			float num4 = 0f;
			num = ((!radiusIncrements.HasValue) ? maxRadius : (num + radiusIncrements.Value));
			while (!foundPoint.HasValue)
			{
				if (num4 > 1f)
				{
					if (num2 <= 0f)
					{
						break;
					}
					num2 -= 0.1f;
					num3 = ((!(num3 > 0f)) ? 1f : (-1f));
				}
				else
				{
					num4 += 0.1f;
					num3 = ((!(num3 > 0f)) ? 1f : (-1f));
				}
				directionToLookForUnblockedLocationIn = new Vector3(num4 * num3, num2, 0f);
				foundPoint = ScanDirectionFromCenter(directionToLookForUnblockedLocationIn, terrain, locationToComputeFrom, avoidReservedSubtiles, num, subtileIsValid);
			}
		}
		while (num < maxRadius);
	}

	private static Vector3? ScanDirectionFromCenter(Vector3 directionFromBaseCenter, SubtileLayers terrain, Vector3 locationToScanFrom, bool avoidReservedSubtiles, float maxDistance, Predicate<SubtilePos> subtileIsValid = null)
	{
		float num = 8f;
		for (float num2 = 0f; num2 < maxDistance; num2 += num)
		{
			WorldLocation worldLocation = new WorldLocation(locationToScanFrom + directionFromBaseCenter * num2);
			if (The.Map.ClampWorldPosition(worldLocation) != worldLocation)
			{
				return null;
			}
			SubtilePos subtilePos = WorldPosToSubtilePos(worldLocation);
			SubtileValue value = terrain.GetValue(subtilePos);
			if (!IsBlocked(value) && (!avoidReservedSubtiles || !TestForFlag(value, SubtileValue.Reserved)) && (subtileIsValid == null || subtileIsValid(subtilePos)))
			{
				return worldLocation.ToVector3();
			}
		}
		return null;
	}

	private bool TerrainDepthIsUnderWaterLevel(float terrainDepth, float waterLevelBelowTerrain)
	{
		return terrainDepth - waterLevelBelowTerrain > 0f;
	}

	public bool IsBaseCenterOfWorkingBuilding(Point pos, ref Entity structure)
	{
		return false;
	}

	public bool SubtileContainsEntities(Point subtile, Predicate<Entity> countEntity)
	{
		Point pos = SubTileToTilePos(subtile);
		TerrainTile tile = GetTile(pos);
		if (tile.EntitiesOnTile != null)
		{
			foreach (Entity item in tile.EntitiesOnTile)
			{
				if ((countEntity == null || countEntity(item)) && WorldPosToSubtile(item.PlaySiteLocation) == subtile)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearTile(TerrainTile selTile)
	{
		if (selTile == null)
		{
			return;
		}
		if (selTile.EdgeLayoutEntities != null)
		{
			int num;
			for (num = 0; num < selTile.EdgeLayoutEntities.Count; num++)
			{
				selTile.EdgeLayoutEntities[num].Destroy();
				num--;
			}
		}
		if (selTile.EntitiesOnTile != null)
		{
			int num2;
			for (num2 = 0; num2 < selTile.EntitiesOnTile.Count; num2++)
			{
				selTile.EntitiesOnTile[num2].Destroy();
				num2--;
			}
		}
		if (selTile.TreesOnTile != null)
		{
			for (int i = 0; i < selTile.TreesOnTile.Count; i++)
			{
				selTile.TreesOnTile[i]?.Destroy();
			}
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		AllMaps = sn.DoList(AllMaps);
		abCartesianLength = sn.DoFloat(abCartesianLength);
		abDiagonalLength = sn.DoFloat(abDiagonalLength);
		eightDirsAsVectors = sn.DoArray(eightDirsAsVectors);
		normalizedEightDirsAsVectors = sn.DoArray(normalizedEightDirsAsVectors);
		mapSubtileHeight = sn.DoInt32(mapSubtileHeight);
		mapSubtileWidth = sn.DoInt32(mapSubtileWidth);
		mapTileHeight = sn.DoInt32(mapTileHeight);
		mapTileWidth = sn.DoInt32(mapTileWidth);
		MapWorldHeight = sn.DoFloat(MapWorldHeight);
		MapWorldWidth = sn.DoFloat(MapWorldWidth);
		MaxWorldPos = sn.DoVector3(MaxWorldPos);
		NoParkingSpots = sn.DoDictionary(NoParkingSpots);
		NoParkingSpotsTimeStamps = sn.DoQueue(NoParkingSpotsTimeStamps);
		NoOfSectorsAcrossHeight = sn.DoInt32(NoOfSectorsAcrossHeight);
		NoOfSectorsAcrossWidth = sn.DoInt32(NoOfSectorsAcrossWidth);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotAllMovementMaps = AllMovementMaps.Select((MovementMap m) => m.ID).ToList();
			snapshotTileMap = new TerrainTileID[Common.GetJaggedArrayWidth(TileMap)][];
			snapshotTerrainCosts = new Dictionary<SurfaceType.TransportType, SubtileLayersID>();
			foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> terrainCost in TerrainCosts)
			{
				snapshotTerrainCosts.Add(terrainCost.Key, terrainCost.Value.ID);
			}
			for (int num = 0; num < TileMap.Length; num++)
			{
				TerrainTile[] array = TileMap[num];
				TerrainTileID[] array2 = new TerrainTileID[array.Length];
				snapshotTileMap[num] = array2;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					array2[num2] = array[num2].ID;
				}
			}
		}
		snapshotAllMovementMaps = sn.DoList(snapshotAllMovementMaps);
		snapshotTileMap = sn.DoJaggedArray(snapshotTileMap);
		snapshotTerrainCosts = sn.DoDictionary(snapshotTerrainCosts);
		sn.Ignore(TileMap);
		sn.Ignore(subtileIndexToDirectionMappings);
		sn.Ignore(direction);
		sn.Ignore(oppositeDirection);
		sn.Ignore(MapTransportTypeArray);
		sn.Ignore(AllMovementMaps);
		sn.Ignore(InfluenceMapTileIsComfortableInfo);
		sn.Ignore(InfluenceMapTileIsFreeInfo);
		sn.Ignore(TerrainCosts);
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
		AllMovementMaps = snapshotAllMovementMaps.Select((CyclableID i) => (MovementMap)LookUp<ICyclable, CyclableID>.FindByID(i)).ToList();
		TileMap = new TerrainTile[Common.GetJaggedArrayWidth(snapshotTileMap)][];
		for (int num = 0; num < snapshotTileMap.Length; num++)
		{
			TerrainTileID[] array = snapshotTileMap[num];
			TerrainTile[] array2 = new TerrainTile[array.Length];
			TileMap[num] = array2;
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				array2[num2] = LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(array[num2]);
			}
		}
		TerrainCosts = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayersID> snapshotTerrainCost in snapshotTerrainCosts)
		{
			TerrainCosts.Add(snapshotTerrainCost.Key, LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotTerrainCost.Value));
		}
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> terrainCost in TerrainCosts)
		{
			terrainCost.Value.LoadPostProcess(sn);
		}
		InitDataNotSnapshotted();
	}
}
