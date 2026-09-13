using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Resources;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Maps.MapEditor;

public class MapLoader
{
	private enum QueueStateLoad
	{
		BuildMap,
		GenerateMapFromTextureData,
		ReduceMapSubdivision,
		LoadSavedMapEntities,
		PlaceFirewoodFromTrees,
		LoadTiles,
		InitCollisionTrees,
		LoadMapData,
		BuildMapTerrainSubtiles,
		BuildMapCosts,
		LoadWaterBottomTintTexture,
		LoadWaterColorTexture,
		LoadSoilTexture,
		LoadVegetationTexture,
		LoadTerrainHeightsTexture,
		LoadMoistureTexture,
		LoadSavedMapTrees,
		ReduceMapSubdivisionScanHoriz,
		ReduceMapSubdivisionScanVert,
		ReduceSubdivisionCombineTiles,
		ReduceSubdivisionGrowArea,
		InitializeEntityData
	}

	private const float minimumHeightForTreesAndGrass = -30f;

	private const float minimumHeightForMoistureFromWaterSurface = -80f;

	private string mapFolderName;

	private string mapFolderPath;

	private QueueStateLoad queueState = QueueStateLoad.LoadMapData;

	private MapData mapData;

	private int transportProgress;

	private int tileProgressX;

	private int entityIndex;

	private Array transports;

	private HashSet<TerrainTile> tilesContainingWaterEdge = new HashSet<TerrainTile>();

	private HashSet<TerrainTile> grownTilesFromWaterEdge;

	private float bitmapToSubtileAmountConversionFactor = 0.00043572986f;

	public MapLoader(string mapFolderName)
	{
		this.mapFolderName = mapFolderName;
		transports = Enum.GetValues(typeof(SurfaceType.TransportType));
	}

	public MapLoader(DirectoryInfo mapFolder)
	{
		mapFolderPath = mapFolder.FullName;
		transports = Enum.GetValues(typeof(SurfaceType.TransportType));
	}

	public static MapData LoadMapData(string mapDataXmlPath, string folderName)
	{
		Vector3 maxWorldPos = The.Map.MaxWorldPos;
		The.Map.MaxWorldPos = new Vector3(500000f, 500000f, 500000f);
		new XmlSerializerNamespaces().Add("", "");
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(MapData));
		MapData mapData;
		using (TextReader textReader = new StreamReader(mapDataXmlPath))
		{
			mapData = (MapData)xmlSerializer.Deserialize(textReader);
		}
		mapData.FolderName = folderName;
		The.Map.MaxWorldPos = maxWorldPos;
		return mapData;
	}

	public bool QueueLoad()
	{
		Rectangle tileArea = new Rectangle(0, 0, The.Map.mapTileWidth, The.Map.mapTileHeight);
		switch (queueState)
		{
		case QueueStateLoad.LoadMapData:
		{
			if (mapFolderPath == null)
			{
				mapFolderPath = MapManager.ComposeMapDataFolderPath(mapFolderName);
			}
			string mapDataXmlPath = MapManager.ComposeMapDataXmlFilePathFromFolderPath(mapFolderPath);
			mapData = LoadMapData(mapDataXmlPath, mapFolderName);
			The.Map.SetDimensions(mapData.Dimensions);
			The.MapUI.SetSize();
			queueState = QueueStateLoad.InitCollisionTrees;
			break;
		}
		case QueueStateLoad.InitCollisionTrees:
			if (The.Map.InitCollisionTrees())
			{
				Common.InitJaggedArray(ref The.Map.TileMap, The.Map.mapTileWidth, The.Map.mapTileHeight);
				queueState = QueueStateLoad.BuildMapTerrainSubtiles;
			}
			break;
		case QueueStateLoad.BuildMapTerrainSubtiles:
			if (BuildMapTerrainSubtiles())
			{
				queueState = QueueStateLoad.BuildMap;
			}
			break;
		case QueueStateLoad.BuildMap:
			if (BuildMap())
			{
				queueState = QueueStateLoad.BuildMapCosts;
			}
			break;
		case QueueStateLoad.BuildMapCosts:
			if (BuildMapCosts())
			{
				if (The.Client != null)
				{
					queueState = QueueStateLoad.LoadMoistureTexture;
				}
				else
				{
					queueState = QueueStateLoad.ReduceMapSubdivision;
				}
			}
			break;
		case QueueStateLoad.LoadMoistureTexture:
			if (LoadMoistureTexture())
			{
				queueState = QueueStateLoad.LoadTerrainHeightsTexture;
			}
			break;
		case QueueStateLoad.LoadTerrainHeightsTexture:
			if (LoadTerrainHeightsTexture())
			{
				queueState = QueueStateLoad.LoadVegetationTexture;
			}
			break;
		case QueueStateLoad.LoadVegetationTexture:
			if (LoadVegetationTexture())
			{
				queueState = QueueStateLoad.LoadSoilTexture;
			}
			break;
		case QueueStateLoad.LoadSoilTexture:
			if (LoadSoilTexture())
			{
				queueState = QueueStateLoad.LoadWaterColorTexture;
			}
			break;
		case QueueStateLoad.LoadWaterColorTexture:
			if (LoadWaterColorTexture())
			{
				queueState = QueueStateLoad.LoadWaterBottomTintTexture;
			}
			break;
		case QueueStateLoad.LoadWaterBottomTintTexture:
			if (LoadWaterBottomTintTexture())
			{
				queueState = QueueStateLoad.ReduceMapSubdivisionScanHoriz;
			}
			break;
		case QueueStateLoad.ReduceMapSubdivisionScanHoriz:
			if (ReduceSubdivisionScanHorizontally(tileArea, tilesContainingWaterEdge))
			{
				queueState = QueueStateLoad.ReduceMapSubdivisionScanVert;
			}
			break;
		case QueueStateLoad.ReduceMapSubdivisionScanVert:
			if (ReduceSubdivisionScanVertically(tileArea, tilesContainingWaterEdge))
			{
				queueState = QueueStateLoad.ReduceSubdivisionGrowArea;
			}
			break;
		case QueueStateLoad.ReduceSubdivisionGrowArea:
			if (ReduceSubdivisionGrowArea(tileArea, tilesContainingWaterEdge, ref grownTilesFromWaterEdge))
			{
				queueState = QueueStateLoad.ReduceSubdivisionCombineTiles;
			}
			break;
		case QueueStateLoad.ReduceSubdivisionCombineTiles:
			if (ReduceSubdivisionCombineTiles(tileArea, grownTilesFromWaterEdge))
			{
				queueState = QueueStateLoad.InitializeEntityData;
			}
			break;
		case QueueStateLoad.InitializeEntityData:
			InitializeEntityData();
			queueState = QueueStateLoad.LoadSavedMapEntities;
			break;
		case QueueStateLoad.LoadSavedMapEntities:
			if (LoadSavedMapEntities())
			{
				queueState = QueueStateLoad.LoadSavedMapTrees;
			}
			break;
		case QueueStateLoad.LoadSavedMapTrees:
			if (LoadSavedMapTrees())
			{
				if (The.Sim.Mode == Sim.EngineMode.Game)
				{
					queueState = QueueStateLoad.PlaceFirewoodFromTrees;
				}
				else
				{
					queueState = QueueStateLoad.LoadTiles;
				}
			}
			break;
		case QueueStateLoad.PlaceFirewoodFromTrees:
			if (PlaceFirewoodFromTrees())
			{
				queueState = QueueStateLoad.LoadTiles;
			}
			break;
		case QueueStateLoad.LoadTiles:
			if (LoadTiles(mapData))
			{
				queueState = QueueStateLoad.LoadMapData;
				return true;
			}
			break;
		}
		return false;
	}

	private bool BuildMap()
	{
		MapManager map = The.Map;
		ushort subTileWidth = (ushort)map.mapSubtileWidth;
		ushort subTileHeight = (ushort)map.mapSubtileHeight;
		map.TerrainCosts = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
		foreach (object value2 in Enum.GetValues(typeof(SurfaceType.TransportType)))
		{
			if ((SurfaceType.TransportType)value2 != SurfaceType.TransportType.Air)
			{
				SubtileLayer subtileLayer = new SubtileLayer(subTileWidth, subTileHeight);
				SubtileLayers value = new SubtileLayers(value2.ToString(), subtileLayer);
				subtileLayer.CreateAllSectors();
				map.TerrainCosts.Add((SurfaceType.TransportType)value2, value);
			}
		}
		return true;
	}

	private bool BuildMapCosts()
	{
		MapManager map = The.Map;
		ushort num = (ushort)map.mapSubtileWidth;
		ushort num2 = (ushort)map.mapSubtileHeight;
		int d = tileProgressX + 3;
		d = Common.ClampTop(d, num);
		SubtileLayers subtileLayers = map.TerrainCosts[(SurfaceType.TransportType)transportProgress];
		for (int i = tileProgressX; i < d; i++)
		{
			TerrainTile[] array = map.TileMap[i / 3];
			for (int j = 0; j < num2; j++)
			{
				TerrainTile terrainTile = array[j / 3];
				subtileLayers.SetValueOnBottomLayer(i, j, (MapManager.SubtileValue)terrainTile.GetCost((SurfaceType.TransportType)transportProgress, SurfaceType.TerrainFeatures.None));
			}
		}
		if (d < num)
		{
			tileProgressX = d;
		}
		else
		{
			tileProgressX = 0;
			do
			{
				transportProgress++;
			}
			while (transportProgress < transports.Length && (long)transportProgress == 3);
			if (transportProgress == transports.Length)
			{
				transportProgress = 0;
				return true;
			}
		}
		return false;
	}

	private bool BuildMapTerrainSubtiles()
	{
		MapManager map = The.Map;
		for (int i = 0; i < map.mapTileHeight; i++)
		{
			TerrainTile terrainTile = new TerrainTile(tileProgressX, i);
			Common.InitJaggedArray(ref terrainTile.TerrainSubtiles, 3, 3);
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					terrainTile.TerrainSubtiles[j][k] = new Terrain(terrainTile);
				}
			}
			map.TileMap[tileProgressX][i] = terrainTile;
		}
		tileProgressX++;
		if (tileProgressX == map.mapTileWidth)
		{
			tileProgressX = 0;
			return true;
		}
		return false;
	}

	private string ComposePath(string filename)
	{
		return Path.Combine(mapFolderPath, filename);
	}

	private bool LoadMoistureTexture()
	{
		Texture2D texture = null;
		Color[] colors = null;
		string path = ComposePath("moisture.png");
		if (File.Exists(path))
		{
			using Stream stream = File.OpenRead(path);
			texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
			colors = new Color[texture.Width * texture.Height];
			texture.GetData(colors);
			Parallel.For(0, The.Map.mapTileWidth, delegate(int x)
			{
				for (int i = 0; i < The.Map.mapTileHeight; i++)
				{
					The.Map.TileMap[x][i].Moisture = (ReadColor(x, i, colors, texture).ToVector4() / 9f).X;
				}
			});
		}
		return true;
	}

	private bool PlaceFirewoodFromTrees()
	{
		float num = 0f;
		ResourceType resourceType = GameData.Instance.AllResourceTypes["firewood"];
		for (int i = 0; i < The.Map.mapTileWidth; i++)
		{
			for (int j = 0; j < The.Map.mapTileHeight; j++)
			{
				TerrainTile terrainTile = The.Map.TileMap[i][j];
				num = 0f;
				if (terrainTile.TreesOnTile == null)
				{
					continue;
				}
				foreach (Entity item in terrainTile.TreesOnTile)
				{
					item.Find<UWGame.SimSide.Trees.Tree>(out var _);
					num += item.EntityType.TreeType.FibrousPercentageOfTotalMass * item.Bulk;
				}
				float num2 = 0.05f * num;
				if (Common.IsGreaterThan(num2, 0f) && resourceType.GetHarvestableItemsFromBulk(num2) > 0)
				{
					terrainTile.AddResource(resourceType, num2);
				}
			}
		}
		return true;
	}

	private bool LoadSoilTexture()
	{
		MapManager map = The.Map;
		LoadBitmapsInFolder(ComposePath("Soil"), out var vegetationTextures, out var soilTexture2ds);
		foreach (KeyValuePair<string, Color[]> kvp in vegetationTextures)
		{
			if (!GameData.Instance.AllSoilComponentTypes.TryGetValue("soil:" + kvp.Key, out var soilType))
			{
				continue;
			}
			Parallel.For(0, map.mapTileWidth, delegate(int x)
			{
				for (int i = 0; i < map.mapTileHeight; i++)
				{
					TerrainTile terrainTile = map.TileMap[x][i];
					for (int j = 0; j < 3; j++)
					{
						for (int k = 0; k < 3; k++)
						{
							float x2 = (float)x + (float)j * 0.33f;
							float y = (float)i + (float)k * 0.33f;
							Terrain terrain = terrainTile.TerrainSubtiles[j][k];
							byte r = ReadColor(x2, y, kvp.Value, soilTexture2ds[kvp.Key]).R;
							if (r > 1)
							{
								float num = (float)(int)r * bitmapToSubtileAmountConversionFactor;
								if (num > 0f)
								{
									terrain.AddSoilComponent(soilType, num);
								}
							}
						}
					}
				}
			});
		}
		Parallel.For(0, The.Map.mapTileWidth, delegate(int x)
		{
			for (int i = 0; i < map.mapTileHeight; i++)
			{
				TerrainTile terrainTile = map.TileMap[x][i];
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						terrainTile.TerrainSubtiles[j][k].NormalizeSoil();
					}
				}
			}
		});
		return true;
	}

	private static void LoadBitmapsInFolder(string folderPath, out Dictionary<string, Color[]> vegetationTextures, out Dictionary<string, Texture2D> vegetationTexture2ds)
	{
		vegetationTextures = new Dictionary<string, Color[]>();
		vegetationTexture2ds = new Dictionary<string, Texture2D>();
		if (!Directory.Exists(folderPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(folderPath, "*.png");
		foreach (string path in files)
		{
			using Stream stream = File.OpenRead(path);
			Texture2D texture2D = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
			Color[] array = new Color[texture2D.Width * texture2D.Height];
			texture2D.GetData(array);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			vegetationTextures.Add(fileNameWithoutExtension, array);
			vegetationTexture2ds.Add(fileNameWithoutExtension, texture2D);
		}
	}

	private bool LoadVegetationTexture()
	{
		LoadBitmapsInFolder(ComposePath("Vegetation"), out var vegetationTextures, out var vegetationTexture2ds);
		MapManager map = The.Map;
		foreach (KeyValuePair<string, Color[]> item in vegetationTextures)
		{
			if (!GameData.Instance.AllLowVegetationTypes.TryGetValue("veg:" + item.Key, out var value))
			{
				continue;
			}
			for (int i = 0; i < map.mapTileWidth; i++)
			{
				for (int j = 0; j < map.mapTileHeight; j++)
				{
					TerrainTile terrainTile = map.TileMap[i][j];
					for (int k = 0; k < 3; k++)
					{
						for (int l = 0; l < 3; l++)
						{
							float x = (float)i + (float)k * 0.33f;
							float y = (float)j + (float)l * 0.33f;
							Terrain terrain = terrainTile.TerrainSubtiles[k][l];
							float num = (float)(int)ReadColor(x, y, item.Value, vegetationTexture2ds[item.Key]).R * bitmapToSubtileAmountConversionFactor;
							if (!value.CanGrowUnderWater && terrain.LevelBelowWater > 0f)
							{
								num = 0f;
							}
							if (num > 0f)
							{
								terrain.AddVegetation(value, num);
							}
						}
					}
				}
			}
		}
		Parallel.For(0, map.mapTileWidth, delegate(int num2)
		{
			for (int m = 0; m < map.mapTileHeight; m++)
			{
				TerrainTile terrainTile2 = map.TileMap[num2][m];
				for (int n = 0; n < 3; n++)
				{
					for (int num3 = 0; num3 < 3; num3++)
					{
						terrainTile2.TerrainSubtiles[n][num3].NormalizeVegetation();
					}
				}
			}
		});
		return true;
	}

	private bool LoadWaterColorTexture()
	{
		Texture2D texture2D = null;
		Color[] array = null;
		MapManager map = The.Map;
		string path = ComposePath("waterColors.png");
		if (File.Exists(path))
		{
			using Stream stream = File.OpenRead(path);
			texture2D = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
			array = new Color[texture2D.Width * texture2D.Height];
			texture2D.GetData(array);
			for (int i = 0; i < map.mapTileWidth; i++)
			{
				for (int j = 0; j < map.mapTileHeight; j++)
				{
					TerrainTile obj = map.TileMap[i][j];
					Vector4 colorOfWater = ReadColor(i, j, array, texture2D).ToVector4();
					obj.ColorOfWater = colorOfWater;
				}
			}
		}
		return true;
	}

	private bool LoadWaterBottomTintTexture()
	{
		Texture2D texture2D = null;
		Color[] array = null;
		MapManager map = The.Map;
		string path = ComposePath("waterBottomTint.png");
		if (File.Exists(path))
		{
			using Stream stream = File.OpenRead(path);
			texture2D = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
			array = new Color[texture2D.Width * texture2D.Height];
			texture2D.GetData(array);
			for (int i = 0; i < map.mapTileWidth; i++)
			{
				for (int j = 0; j < map.mapTileHeight; j++)
				{
					TerrainTile obj = map.TileMap[i][j];
					Vector4 waterBottomTint = ReadColor(i, j, array, texture2D).ToVector4();
					obj.WaterBottomTint = waterBottomTint;
				}
			}
		}
		return true;
	}

	private bool GenerateMapFromTextureData()
	{
		LoadMoistureTexture();
		LoadTerrainHeightsTexture();
		LoadVegetationTexture();
		LoadSoilTexture();
		LoadWaterColorTexture();
		LoadWaterBottomTintTexture();
		return true;
	}

	private Color ReadColor(float x, float y, Color[] colors, Texture2D texture)
	{
		MapManager map = The.Map;
		float num = x / (float)map.mapTileWidth;
		float num2 = y / (float)map.mapTileHeight;
		return colors[(int)(num * (float)texture.Width) + (int)(num2 * (float)(texture.Height - 1)) * texture.Width];
	}

	private bool LoadTerrainHeightsTexture()
	{
		MapManager map = The.Map;
		Texture2D texture = null;
		Color[] colors = null;
		string text = ComposePath("terrainHeights.png");
		if (File.Exists(text))
		{
			using (Stream stream = File.OpenRead(text))
			{
				texture = Texture2D.FromStream(The.Client.GraphicsDevice, stream);
				colors = new Color[texture.Width * texture.Height];
				texture.GetData(colors);
			}
			float waterLevelBelowTerrain = The.Client.Renderer.Water.WaterHeight - 1400f;
			Parallel.For(0, map.mapTileWidth, delegate(int x)
			{
				for (int i = 0; i < map.mapTileHeight; i++)
				{
					TerrainTile terrainTile = map.TileMap[x][i];
					for (int j = 0; j < 3; j++)
					{
						for (int k = 0; k < 3; k++)
						{
							Point subtileCostToSurfaceType = MapManager.TileAndRelativeSubtileToAbsoluteSubtile(x, i, j, k);
							float x2 = (float)x + (float)j * 0.33f;
							float y = (float)i + (float)k * 0.33f;
							Terrain terrain = terrainTile.TerrainSubtiles[j][k];
							float terrainDepth = (int)ReadColor(x2, y, colors, texture).R;
							SetTerrainDepth(waterLevelBelowTerrain, terrain, terrainDepth);
							The.Map.SetSubtileCostToSurfaceType(subtileCostToSurfaceType);
						}
					}
					SetMoistureOnTile(terrainTile);
				}
			});
			return true;
		}
		throw new Exception("File not found: " + text);
	}

	public static void SetTerrainDepth(float waterLevelBelowTerrain, Terrain terrain, float terrainDepth)
	{
		terrain.TerrainDepth = terrainDepth;
		float num = terrainDepth - waterLevelBelowTerrain;
		if (num > 0f)
		{
			terrain.SurfaceType = WaterType.Instance;
		}
		else
		{
			terrain.SurfaceType = PlainsType.Instance;
		}
		terrain.LevelBelowWater = num;
	}

	private static bool ReduceSubdivisionCombineTiles(Rectangle tileArea, HashSet<TerrainTile> grownTilesFromWaterEdge)
	{
		_ = The.Map;
		The.Map.IterateTileArea(tileArea, delegate(TerrainTile tile)
		{
			if (!grownTilesFromWaterEdge.Contains(tile))
			{
				CombineTerrainSubtiles(tile);
			}
		});
		return true;
	}

	private static bool ReduceSubdivisionGrowArea(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge, ref HashSet<TerrainTile> grownTilesFromWaterEdge)
	{
		MapManager map = The.Map;
		int num = 1;
		grownTilesFromWaterEdge = new HashSet<TerrainTile>(tilesContainingWaterEdge);
		foreach (TerrainTile item in tilesContainingWaterEdge)
		{
			Point point = new Point(item.X, item.Y);
			int num2 = Math.Max(tileArea.Left, point.X - num);
			int num3 = Math.Max(tileArea.Top, point.Y - num);
			int num4 = Math.Min(tileArea.Width - 1, point.X + num);
			int num5 = Math.Min(tileArea.Height - 1, point.Y + num);
			for (int i = num2; i <= num4; i++)
			{
				for (int j = num3; j <= num5; j++)
				{
					TerrainTile terrainTile = map.TileMap[i][j];
					if (!terrainTile.IsFullyUnderWater())
					{
						grownTilesFromWaterEdge.Add(terrainTile);
					}
				}
			}
		}
		return true;
	}

	public static void RecomputeSubdivision(Rectangle tileArea)
	{
		HashSet<TerrainTile> hashSet = new HashSet<TerrainTile>();
		ReduceSubdivisionScanHorizontally(tileArea, hashSet);
		ReduceSubdivisionScanVertically(tileArea, hashSet);
		HashSet<TerrainTile> hashSet2 = null;
		ReduceSubdivisionGrowArea(tileArea, hashSet, ref hashSet2);
		ReduceSubdivisionCombineTiles(tileArea, hashSet2);
	}

	public static void CreateTerrainSubtiles(Rectangle tileArea)
	{
		The.Map.IterateTileArea(tileArea, delegate(TerrainTile mapTile)
		{
			if (mapTile.Terrain != null)
			{
				Common.InitJaggedArray(ref mapTile.TerrainSubtiles, 3, 3);
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						Terrain terrain = new Terrain(mapTile);
						mapTile.TerrainSubtiles[i][j] = terrain;
						if (mapTile.Terrain.SoilComponents != null)
						{
							foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in mapTile.Terrain.SoilComponents)
							{
								terrain.AddSoilComponent(soilComponent.Key, soilComponent.Value.Amount / 9f);
							}
						}
						if (mapTile.Terrain.Vegetation != null)
						{
							foreach (KeyValuePair<LowVegetationType, LowVegetation> item in mapTile.Terrain.Vegetation)
							{
								terrain.AddVegetation(item.Key, item.Value.Amount / 9f);
							}
						}
						terrain.LevelBelowWater = mapTile.Terrain.LevelBelowWater;
						terrain.TerrainDepth = mapTile.Terrain.TerrainDepth;
						terrain.SurfaceType = mapTile.Terrain.SurfaceType;
					}
				}
				mapTile.Terrain = null;
			}
		});
	}

	private static bool ReduceSubdivisionScanVertically(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge)
	{
		MapManager map = The.Map;
		int width = tileArea.Width;
		int height = tileArea.Height;
		int left = tileArea.Left;
		int top = tileArea.Top;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = width * 3;
		int num6 = height * 3;
		int num7 = left * 3;
		int num8 = top * 3;
		int num9 = num7 + num5;
		int num10 = num8 + num6;
		num2 = left;
		num = top;
		TerrainTile terrainTile = map.TileMap[left][top];
		num3 = 0;
		num4 = 0;
		bool flag = terrainTile.TerrainSubtiles[0][0].LevelBelowWater > 0f;
		bool flag2 = flag;
		for (int i = num7; i < num9; i++)
		{
			for (int j = num8; j < num10; j++)
			{
				flag2 = terrainTile.TerrainSubtiles[num4][num3].LevelBelowWater > 0f;
				if (j == num8)
				{
					flag = flag2;
				}
				if (flag2 != flag)
				{
					flag = flag2;
					tilesContainingWaterEdge.Add(terrainTile);
				}
				num3++;
				if (num3 == 3)
				{
					num3 = 0;
					num++;
					if (num == height)
					{
						num = top;
					}
					terrainTile = map.TileMap[num2][num];
				}
			}
			num4++;
			if (num4 == 3)
			{
				num4 = 0;
				num2++;
				if (num2 < width)
				{
					terrainTile = map.TileMap[num2][num];
				}
			}
		}
		return true;
	}

	private static bool ReduceSubdivisionScanHorizontally(Rectangle tileArea, HashSet<TerrainTile> tilesContainingWaterEdge)
	{
		MapManager map = The.Map;
		int num = 0;
		int num2 = 0;
		int width = tileArea.Width;
		int height = tileArea.Height;
		int left = tileArea.Left;
		int top = tileArea.Top;
		int num3 = left;
		int num4 = top;
		int num5 = width * 3;
		int num6 = height * 3;
		int num7 = left * 3;
		int num8 = top * 3;
		int num9 = num7 + num5;
		int num10 = num8 + num6;
		TerrainTile terrainTile = map.TileMap[left][top];
		bool flag = terrainTile.TerrainSubtiles[0][0].LevelBelowWater > 0f;
		bool flag2 = flag;
		for (int i = num8; i < num10; i++)
		{
			for (int j = num7; j < num9; j++)
			{
				flag2 = terrainTile.TerrainSubtiles[num2][num].LevelBelowWater > 0f;
				if (j == num7)
				{
					flag = flag2;
				}
				if (flag2 != flag)
				{
					flag = flag2;
					tilesContainingWaterEdge.Add(terrainTile);
				}
				num2++;
				if (num2 == 3)
				{
					num2 = 0;
					num3++;
					if (num3 == tileArea.Right)
					{
						num3 = left;
					}
					terrainTile = map.TileMap[num3][num4];
				}
			}
			num++;
			if (num == 3)
			{
				num = 0;
				num4++;
				if (num4 < tileArea.Bottom)
				{
					terrainTile = map.TileMap[num3][num4];
				}
			}
		}
		return true;
	}

	private static void CombineTerrainSubtiles(TerrainTile tile)
	{
		Terrain terrain = new Terrain(tile);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Terrain terrain2 = tile.TerrainSubtiles[i][j];
				if (terrain2.Vegetation != null)
				{
					foreach (KeyValuePair<LowVegetationType, LowVegetation> item in terrain2.Vegetation)
					{
						if (item.Value.Amount > 0f)
						{
							terrain.AddVegetation(item.Value.LowVegetationType, item.Value.Amount);
						}
					}
				}
				if (terrain2.SoilComponents == null)
				{
					continue;
				}
				foreach (KeyValuePair<SoilComponentType, SoilComponent> soilComponent in terrain2.SoilComponents)
				{
					if (soilComponent.Value.Amount > 0f)
					{
						terrain.AddSoilComponent(soilComponent.Value.SoilComponentType, soilComponent.Value.Amount);
					}
				}
			}
		}
		CombineTerrain(tile, terrain);
		tile.TerrainSubtiles = null;
		tile.Terrain = terrain;
		tile.Terrain.NormalizeVegetation();
		tile.Terrain.NormalizeSoil();
		if (tile.X == 208)
		{
			_ = tile.Y;
			_ = 38;
		}
		tile.Terrain.RecomputeDisplayAmounts();
	}

	private static void CombineTerrain(TerrainTile tile, Terrain combinedTerrain)
	{
		Terrain centerTerrain = tile.GetCenterTerrain();
		combinedTerrain.TerrainDepth = centerTerrain.TerrainDepth;
		combinedTerrain.LevelBelowWater = centerTerrain.LevelBelowWater;
		combinedTerrain.SurfaceType = centerTerrain.SurfaceType;
	}

	private void SetMoistureOnTile(TerrainTile tile)
	{
		Terrain centerTerrain = tile.GetCenterTerrain();
		if (centerTerrain.LevelBelowWater < 0f && centerTerrain.LevelBelowWater > -80f)
		{
			tile.Moisture = MathHelper.Lerp(1f, tile.Moisture, centerTerrain.LevelBelowWater / -80f);
		}
		else if (centerTerrain.LevelBelowWater > 0f)
		{
			tile.Moisture = 1f;
		}
	}

	private void InitializeEntityData()
	{
		InitializeEntityData(mapData.SavedMapEntities);
		InitializeEntityData(mapData.Trees);
	}

	private void InitializeEntityData(List<EntityData> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (EntityData item in list)
		{
			item.PostDataCompleteInitialize();
		}
	}

	private bool LoadSavedMapEntities()
	{
		if (mapData.SavedMapEntities != null)
		{
			int d = entityIndex + 100;
			d = Common.ClampTop(d, mapData.SavedMapEntities.Count);
			for (int i = entityIndex; i < d; i++)
			{
				CreateAndPlaceEntityFromEntityData(mapData.SavedMapEntities[i], out var _);
			}
			if (d == mapData.SavedMapEntities.Count)
			{
				entityIndex = 0;
				return true;
			}
			entityIndex = d;
			return false;
		}
		return true;
	}

	private bool LoadSavedMapTrees()
	{
		if (mapData.Trees != null)
		{
			int d = entityIndex + 100;
			d = Common.ClampTop(d, mapData.Trees.Count);
			for (int i = entityIndex; i < d; i++)
			{
				CreateAndPlaceEntityFromEntityData(mapData.Trees[i], out var _);
			}
			if (d == mapData.Trees.Count)
			{
				entityIndex = 0;
				return true;
			}
			entityIndex = d;
			return false;
		}
		return true;
	}

	private bool LoadTiles(MapData mapToLoad)
	{
		MapManager map = The.Map;
		if (mapToLoad.Tiles != null)
		{
			Sim.EngineMode mode = The.Sim.Mode;
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Tile tile in mapToLoad.Tiles)
			{
				if (tile.Resources != null)
				{
					TerrainTile terrainTile = map.TileMap[tile.Position.X][tile.Position.Y];
					Resource[] resources = tile.Resources;
					foreach (Resource resource in resources)
					{
						resource.PostDataCompleteInitialize();
						hashSet.Add(resource.KeyName);
					}
					if (mode == Sim.EngineMode.Edit)
					{
						terrainTile.DesignerPlacedResources = tile.Resources;
					}
					if (mode == Sim.EngineMode.Game)
					{
						SetRandomTileResourcesFromProbabilities(tile.Resources, terrainTile);
					}
				}
			}
			foreach (string item in hashSet)
			{
				The.Sim.PlaySite.EditorResources.Add(GameData.Instance.AllResourceTypes[item]);
			}
		}
		return true;
	}

	public static Entity CreateAndPlaceEntityFromEntityData(EntityData entityData, out bool placementFailed, Entity addToContainer = null, StorageCondition placeInStorage = null, bool offerForSale = false, bool isProductionOutput = false, Vector3? locationToUse = null, string owningAllegianceKey = null, string owningExpeditionKey = null, string memberOfAllegianceKey = null, string memberOfExpeditionKey = null, string siteKey = null, EntityID? anchorID = null, bool assertContainment = true, UpgradeCategory upgradeCategory = null, float? age = null, string caste = null, bool logProductionStatistics = false, bool suppressSpawningEvents = false)
	{
		placementFailed = false;
		if (!GameData.Instance.AllEntityTypes.TryGetValue(entityData.EntityKey, out var value))
		{
			return null;
		}
		Entity entity = new Entity(value);
		SetEntityDataBeforeInitialize(entityData, entity, age, caste);
		string owningAllegianceKey2 = null;
		string owningExpeditionKey2 = null;
		string memberOfAllegianceKey2 = null;
		string memberOfExpeditionKey2 = null;
		if (entityData.OwnedBy != null)
		{
			owningAllegianceKey2 = entityData.OwnedBy.AllegianceKey;
			owningExpeditionKey2 = entityData.OwnedBy.ExpeditionKey;
		}
		if (entityData.MemberOf != null)
		{
			memberOfAllegianceKey2 = entityData.MemberOf.AllegianceKey;
			memberOfExpeditionKey2 = entityData.MemberOf.ExpeditionKey;
		}
		if (memberOfExpeditionKey != null)
		{
			memberOfExpeditionKey2 = memberOfExpeditionKey;
		}
		if (memberOfAllegianceKey != null)
		{
			memberOfAllegianceKey2 = memberOfAllegianceKey;
		}
		if (owningExpeditionKey != null)
		{
			owningExpeditionKey2 = owningExpeditionKey;
		}
		if (owningAllegianceKey != null)
		{
			owningAllegianceKey2 = owningAllegianceKey;
		}
		Allegiance allegiance = null;
		Expedition expedition = null;
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			GetOrCreateAllegianceAndExpedition(entity.EntityType, out allegiance, out expedition, entityData.Location.GetValueOrDefault(), owningAllegianceKey2, owningExpeditionKey2, memberOfAllegianceKey2, memberOfExpeditionKey2);
		}
		else if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			entity.Find<EditorData>(out var c);
			if (memberOfAllegianceKey != null)
			{
				c.AllegianceKey = memberOfAllegianceKey;
			}
			if (memberOfExpeditionKey != null)
			{
				c.ExpeditionName = memberOfExpeditionKey;
			}
		}
		ThreatGroup threatGroup = GetThreatGroup(entityData, entity);
		Site site = ((siteKey != null) ? The.Sim.World.AllSites[siteKey] : ((allegiance == null) ? The.Sim.PlaySite : allegiance.Site));
		entity.Initialize(site, allegiance, expedition, threatGroup);
		if (entity.IsOnPlaySite())
		{
			entity.InitializeModelAndOnScreenFunctionality();
		}
		IOwner owner = null;
		if (entityData.MemberOf == null)
		{
		}
		if ((entityData.OwnedBy != null || owningExpeditionKey != null) && entityData.Person == null)
		{
			owner = expedition;
		}
		StorageCompartment? storageCompartment = null;
		if (offerForSale)
		{
			storageCompartment = StorageCompartment.OfferedForTrade;
		}
		Entity.SetOwnerInfo value2 = new Entity.SetOwnerInfo(owner);
		if (entity.IsOnPlaySite())
		{
			bool flag = false;
			if (entityData.Person != null)
			{
				flag = entityData.Person.SimulateJoinedExpeditionNow;
			}
			if (logProductionStatistics && owner != null)
			{
				entity.SpawnedByOwner = owner.ID;
			}
			Vector3? location = locationToUse ?? entityData.Location;
			Entity.StructureState? structureState = Entity.StructureState.Finished;
			Entity.SetOwnerInfo? setOwnerInfo = value2;
			Expedition newExpedition = expedition;
			StorageCompartment? placeProductsInCompartment = storageCompartment;
			bool assertContainment2 = assertContainment;
			bool simulateJoinedExpeditionNow = flag;
			if (!entity.PlaceEntityOnPlaySite(location, addToContainer, structureState, setOwnerInfo, newExpedition, placeProductsInCompartment, placeInStorage, isProductionOutput, anchorID, assertContainment2, simulateJoinedExpeditionNow, upgradeCategory))
			{
				placementFailed = true;
				return entity;
			}
		}
		else
		{
			bool simulateJoinedExpeditionNow2 = false;
			if (entityData.Person != null)
			{
				simulateJoinedExpeditionNow2 = entityData.Person.SimulateJoinedExpeditionNow;
			}
			entity.PlaceEntityOnOtherSite(site, addToContainer, true, value2, expedition, storageCompartment, placeInStorage, simulateJoinedExpeditionNow2);
		}
		entity.ComeOnline(suppressSpawningEvents);
		SetEntityDataAfterInitialize(entityData, entity);
		return entity;
	}

	public static void SetEntityDataAfterInitialize(EntityData entityData, Entity entity)
	{
		if (entityData.Person != null)
		{
			UWGame.SimSide.Entities.Person personEntity = entity.PersonEntity;
			if (!string.IsNullOrEmpty(entityData.Person.Portrait))
			{
				personEntity.UpdatePortrait(The.InGameUI.gui, entityData.Person.Portrait);
			}
		}
		if (entityData.Resources != null || (entity.EntityType.TreeType != null && entity.EntityType.TreeType.DefaultCrops != null))
		{
			DefaultCrops[] defaultCrops = null;
			if (entity.EntityType.TreeType != null && entity.EntityType.TreeType.DefaultCrops != null)
			{
				defaultCrops = entity.EntityType.TreeType.DefaultCrops;
			}
			SetRandomCropsFromResourceProbabilities(entityData.Resources, defaultCrops, entity);
		}
		if (entityData.NeedLevels != null)
		{
			Needs needs = entity.BiologicalEntity.Needs;
			foreach (KeyValuePair<string, NeedData> needLevel in entityData.NeedLevels)
			{
				if (needLevel.Value.Level != null)
				{
					needs.NeedsList[needLevel.Key].CurrentLevel = (float)needLevel.Value.Level.GetRandomValue(The.Sim.GameplayRandomGenerator);
				}
				if (needLevel.Value.DaysAtZero != null)
				{
					needs.NeedsList[needLevel.Key].PhysicalNeed.DaysAtZero = (float)needLevel.Value.DaysAtZero.GetRandomValue(The.Sim.GameplayRandomGenerator);
				}
			}
		}
		else if (entity.BiologicalEntity != null)
		{
			foreach (KeyValuePair<string, Need> needs2 in entity.BiologicalEntity.Needs.NeedsList)
			{
				needs2.Value.CurrentLevel = (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, 0.6000000238418579, 0.10000000149011612);
			}
		}
		if (entityData.Properties != null)
		{
			foreach (KeyValuePair<string, PropertyResult> property in entityData.Properties)
			{
				entity.SetPropertyValue(property.Key, property.Value);
			}
		}
		if (entityData.EffectProfiles != null)
		{
			string[] effectProfiles = entityData.EffectProfiles;
			foreach (string key in effectProfiles)
			{
				EffectProfileType effect = GameData.Instance.AllEffectProfileTypes[key];
				entity.SimEffects.Start(effect);
			}
		}
		if (entityData.BioEntity != null)
		{
			entity.Find<UWGame.SimSide.Entities.Biological.BiologicalEntity>(out var c);
			if (entityData.BioEntity.StomachContent != null)
			{
				c.SetStomachContents((float)entityData.BioEntity.StomachContent.GetRandomValue(The.Sim.GameplayRandomGenerator));
			}
		}
	}

	public static void SetEntityDataBeforeInitialize(EntityData entityData, Entity entity, float? age = null, string caste = null)
	{
		entity.FlipHorizontally = entityData.FlipHorizontally;
		if (entityData.Bulk.HasValue)
		{
			entity.Bulk = entityData.Bulk.Value;
		}
		entity.Name = entityData.Name;
		entity.Add(new EditorData(entity)
		{
			Resources = entityData.Resources
		});
		if (The.Sim.Mode == Sim.EngineMode.Edit && entityData.Resources != null)
		{
			Resource[] resources = entityData.Resources;
			foreach (Resource resource in resources)
			{
				The.Sim.PlaySite.EditorResources.Add(resource.ResourceType);
			}
		}
		if (entity.Locomotor != null && entityData.Rotation.HasValue)
		{
			entity.SetRotationAndDir(MathHelper.ToRadians(entityData.Rotation.Value));
		}
		_ = entityData.Threat;
		if (entityData.Tree != null)
		{
			entity.Find<UWGame.SimSide.Trees.Tree>(out var c);
			entityData.Tree.SetTreeComponentPreInit(c);
		}
		else if (entity.EntityType.BiologicalType != null)
		{
			entity.Find<UWGame.SimSide.Entities.Biological.BiologicalEntity>(out var c2);
			if (entityData.BioEntity != null)
			{
				entityData.BioEntity.FillEntity(entity, age, caste);
			}
			else
			{
				if (caste != null)
				{
					c2.SetCasteOnNewEntity(caste);
				}
				else
				{
					c2.SetRandomCaste();
				}
				c2.SetAgePreInit(age);
			}
		}
		if (entityData.Person != null)
		{
			entityData.Person.FillEntity(entity);
		}
	}

	private static void GetOrCreateAllegianceAndExpedition(EntityType entityType, out Allegiance allegiance, out Expedition expedition, Vector3 location, string owningAllegianceKey = null, string owningExpeditionKey = null, string memberOfAllegianceKey = null, string memberOfExpeditionKey = null)
	{
		allegiance = null;
		expedition = null;
		string text = null;
		string expeditionKey = null;
		if (owningAllegianceKey != null)
		{
			text = owningAllegianceKey;
		}
		else if (memberOfAllegianceKey != null)
		{
			text = memberOfAllegianceKey;
		}
		if (owningExpeditionKey != null)
		{
			expeditionKey = owningExpeditionKey;
		}
		else if (memberOfExpeditionKey != null)
		{
			expeditionKey = memberOfExpeditionKey;
		}
		if (!string.IsNullOrEmpty(text))
		{
			allegiance = FindOrCreateAllegiance(entityType, text);
		}
		if (allegiance != null)
		{
			expedition = FindOrCreateExpedition(allegiance, location, expeditionKey);
		}
	}

	private static Expedition FindOrCreateExpedition(Allegiance allegiance, Vector3 location, string expeditionKey)
	{
		Expedition expedition = null;
		expedition = (string.IsNullOrEmpty(expeditionKey) ? allegiance.GetFirstExpedition() : allegiance.GetExpedition(expeditionKey));
		if (expedition == null)
		{
			expedition = new Expedition(allegiance, expeditionKey ?? allegiance.KeyName, expeditionKey ?? allegiance.KeyName, location);
		}
		return expedition;
	}

	private static Allegiance FindOrCreateAllegiance(EntityType entityType, string allegianceKey, string allegianceName = null, AllegianceType allegianceType = AllegianceType.Other)
	{
		Allegiance allegiance = null;
		if (allegianceKey != null)
		{
			allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKey);
		}
		if (allegiance == null)
		{
			allegiance = new Allegiance(allegianceType, entityType, allegianceKey, allegianceName);
			if (entityType.Person != null)
			{
				allegiance.HumanActivities = new HumanActivities();
			}
		}
		return allegiance;
	}

	private static ThreatGroup GetThreatGroup(EntityData saved, Entity entity)
	{
		ThreatGroup result = null;
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			if (saved.Threat != null)
			{
				result = ThreatGroup.GetThreatGroup(saved.Threat.ThreatGroupName);
			}
		}
		else
		{
			entity.Find<EditorData>(out var c);
			if (saved.Threat != null)
			{
				c.ThreatGroupName = saved.Threat.ThreatGroupName;
			}
		}
		return result;
	}

	private static void SetRandomCropsFromResourceProbabilities(Resource[] resources, DefaultCrops[] defaultCrops, Entity entity)
	{
		entity.Find<UWGame.SimSide.Trees.Tree>(out var c);
		if (c == null || c.Crops == null)
		{
			return;
		}
		Resource resource = null;
		DefaultCrops defaultCrops2 = null;
		foreach (KeyValuePair<ResourceType, Crop> kvp in c.Crops)
		{
			resource = null;
			defaultCrops2 = null;
			if (resources != null)
			{
				resource = resources.FirstOrDefault((Resource r) => r.KeyName == kvp.Key.KeyName);
			}
			if (defaultCrops != null)
			{
				defaultCrops2 = defaultCrops.FirstOrDefault((DefaultCrops r) => r.KeyName == kvp.Key.KeyName);
			}
			if (resource != null && resource.AbsoluteMeanItems.HasValue)
			{
				kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, resource.AbsoluteMeanItems.Value, resource.AbsoluteStandardDeviation.Value);
			}
			else if (defaultCrops2 != null)
			{
				float? mean;
				float? stdDev;
				if (resource != null && resource.Modifier.HasValue)
				{
					Common.GetNormalDistributionFromMinMaxValues(0.01f * (float)resource.Modifier.Value * (float)defaultCrops2.MinItemsForFullGrownPlant.Value, 0.01f * (float)resource.Modifier.Value * (float)defaultCrops2.MaxItemsForFullGrownPlant.Value, out mean, out stdDev);
				}
				else
				{
					mean = defaultCrops2.AbsoluteMeanItems;
					stdDev = defaultCrops2.AbsoluteStandardDeviationItems;
				}
				kvp.Value.SetRandomCrops(entity.Bulk / entity.EntityType.TreeType.BulkPerSize, mean.Value, stdDev.Value);
			}
		}
	}

	private static void SetRandomTileResourcesFromProbabilities(Resource[] resources, TerrainTile tile)
	{
		foreach (Resource resource in resources)
		{
			if (resource.AbsoluteMeanItems.HasValue)
			{
				int noOfResourceItems = (int)Math.Round(The.Sim.GameplayRandomGenerator.RandomNormalDistribution(resource.AbsoluteMeanItems.Value, resource.AbsoluteStandardDeviation.Value), MidpointRounding.ToEven);
				tile.AddResource(resource.KeyName, noOfResourceItems);
			}
			else if (resource.Modifier.HasValue && resource.Modifier.Value != 100 && tile.TileResources != null)
			{
				ResourceType key = GameData.Instance.AllResourceTypes[resource.KeyName];
				if (tile.TileResources.TryGetValue(key, out var value))
				{
					value.SetTotalHarvestableBulk(0.01f * (float)resource.Modifier.Value * value.TotalHarvestableBulk);
				}
			}
		}
	}

	private void LoadTreeBitmapsAndPlaceTrees(MapData mapData)
	{
		MapManager map = The.Map;
		LoadBitmapsInFolder(ComposePath("Trees"), out var vegetationTextures, out var vegetationTexture2ds);
		int num = 4;
		_ = 256f / (float)num;
		float num2 = 0f;
		List<Point> list = new List<Point>
		{
			new Point(0, 0),
			new Point(0, 1),
			new Point(0, 2),
			new Point(1, 0),
			new Point(1, 1),
			new Point(1, 2),
			new Point(2, 0),
			new Point(2, 1),
			new Point(2, 2)
		};
		int num3 = 0;
		foreach (KeyValuePair<string, Color[]> item in vegetationTextures)
		{
			num3 = 0;
			if (!GameData.Instance.AllTreeTypes.TryGetValue("tree:" + item.Key, out var value))
			{
				continue;
			}
			for (int i = 0; i < map.mapTileHeight; i += 3)
			{
				for (int j = 0; j < map.mapTileWidth; j += 3)
				{
					num2 = 0f;
					int num4 = 0;
					for (int k = 0; k < list.Count; k++)
					{
						Point point = list[k];
						point.X += j;
						point.Y += i;
						if (point.X >= 0 && point.X <= map.mapTileWidth - 1 && point.Y >= 0 && point.Y <= map.mapTileHeight - 1)
						{
							TerrainTile terrainTile = map.TileMap[point.X][point.Y];
							if (terrainTile.TerrainSubtiles[1][1].LevelBelowWater < -30f)
							{
								num4++;
							}
						}
					}
					list = Common.Randomize(list, The.Sim.GameplayRandomGenerator);
					int r = ReadColor(j, i, item.Value, vegetationTexture2ds[item.Key]).R;
					float num5 = ((r > 200) ? 12f : ((r > 150) ? 8f : ((r <= 50) ? 0f : 4f)));
					if (!(num5 > 0f))
					{
						continue;
					}
					for (int l = 0; l < 6; l++)
					{
						if (num2 > num5)
						{
							break;
						}
						for (int m = 0; m < list.Count; m++)
						{
							Point point = list[m];
							point.X += j;
							point.Y += i;
							if (point.X < 0 || point.X > map.mapTileWidth - 1 || point.Y < 0 || point.Y > map.mapTileHeight - 1)
							{
								continue;
							}
							TerrainTile terrainTile = map.TileMap[point.X][point.Y];
							if (terrainTile.GetCenterTerrain().LevelBelowWater < -30f)
							{
								Vector3 position = MapManager.TileToWorldPos(new Point(terrainTile.X, terrainTile.Y));
								position += new Vector3(24 - The.Sim.GameplayRandomGenerator.Next(48, "MapLoader"), 24 - The.Sim.GameplayRandomGenerator.Next(48, "MapLoader"), 0f);
								position = map.ClampWorldPosition(position);
								The.Map.WorldLocationToDirectionWithinTile(position);
								if (!terrainTile.ContainsTreeAtSubtile(MapManager.WorldPosToSubtile(position)))
								{
									Entity entity = new Entity(value);
									entity.Initialize(The.Sim.PlaySite);
									entity.PlaceEntityOnPlaySite(position, null, null, null);
									entity.ComeOnline();
									entity.Find<UWGame.SimSide.Trees.Tree>(out var _);
									num2 += entity.EntityType.TreeType.SizeImpact;
									num3++;
								}
								if (num2 > num5)
								{
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}
