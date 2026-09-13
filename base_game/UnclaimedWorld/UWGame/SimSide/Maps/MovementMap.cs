using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps;

public class MovementMap : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		ComputeChildMaps,
		GetSectorsToAdd,
		AddChildMaps,
		RemoveEmptySectors,
		ComputeRegionMap,
		Completed
	}

	private Dictionary<SurfaceType.TransportType, SubtileLayer> moveMapLayer;

	private Dictionary<SurfaceType.TransportType, SubtileLayerID> snapshotMoveMapLayer;

	private Dictionary<SurfaceType.TransportType, SubtileLayers> layers;

	private Dictionary<SurfaceType.TransportType, SubtileLayersID> snapshotClientLayers;

	private Dictionary<SurfaceType.TransportType, Sector[][]> terrainSectors;

	private bool terrainSectorsAreDirty;

	public List<Dependence> Children = new List<Dependence>();

	public string IDName;

	public static double totalComputationAllInstancesInSeconds;

	private SurfaceType.TransportType[] transportsToInclude;

	private int cycleRegionMapTransportIndex;

	private const Phase StartPhase = Phase.ComputeChildMaps;

	private Phase phase;

	private List<Point> listOfSectorsToAdd = new List<Point>();

	private bool sectorsWereRemoved;

	private int cycleSectorIndex;

	private double updateInterval;

	private Regulator regulator;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Dictionary<SurfaceType.TransportType, SubtileLayers> Layers => layers;

	public bool IsPaused { get; set; }

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

	public bool IsReady { get; set; }

	public double? UpdateInterval => updateInterval;

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

	public bool UnregisterBeforeSnapshot => false;

	public MovementMap(float weight, DiscomfortMap childDMap, ThreatMap childThreatMap, string idName, float updateInterval, params SurfaceType.TransportType[] transportsToInclude)
	{
		AddToLookup();
		The.Map.AllMovementMaps.Add(this);
		IDName = idName;
		this.updateInterval = updateInterval;
		if (transportsToInclude.Length == 0)
		{
			transportsToInclude = new SurfaceType.TransportType[3]
			{
				SurfaceType.TransportType.Foot,
				SurfaceType.TransportType.Car,
				SurfaceType.TransportType.OffRoad
			};
		}
		this.transportsToInclude = transportsToInclude;
		InitMap();
		InitTerrainSectors();
		Dependence item = new Dependence(childDMap, weight);
		Children.Add(item);
		CreateRegulators();
		GetCurrent();
	}

	public MovementMap()
	{
	}

	public void SetTerrainSectorDirty(SurfaceType.TransportType transport, Point subtile)
	{
		if (terrainSectors.TryGetValue(transport, out var value))
		{
			value[subtile.X / 48][subtile.Y / 48].SubtilesAreDirty = true;
			terrainSectorsAreDirty = true;
		}
	}

	private void InitMap()
	{
		Dictionary<SurfaceType.TransportType, SubtileLayers> terrainCosts = The.Map.TerrainCosts;
		ushort subTileWidth = (ushort)The.Map.mapSubtileWidth;
		ushort subTileHeight = (ushort)The.Map.mapSubtileHeight;
		if (moveMapLayer != null)
		{
			return;
		}
		moveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayer>();
		snapshotMoveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayerID>();
		layers = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
		snapshotClientLayers = new Dictionary<SurfaceType.TransportType, SubtileLayersID>();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> kvp in terrainCosts)
		{
			if (transportsToInclude.Length == 0 || Array.Exists(transportsToInclude, (SurfaceType.TransportType t) => t == kvp.Key))
			{
				SubtileLayer subtileLayer = new SubtileLayer(subTileWidth, subTileHeight);
				moveMapLayer.Add(kvp.Key, subtileLayer);
				snapshotMoveMapLayer.Add(kvp.Key, subtileLayer.ID);
				SubtileLayers subtileLayers = terrainCosts[kvp.Key];
				SubtileLayers subtileLayers2 = new SubtileLayers(this, IDName + kvp.Key, subtileLayers.Layers[0], subtileLayer, (TerrainRegionMap)subtileLayers.RegionMap);
				layers.Add(kvp.Key, subtileLayers2);
				snapshotClientLayers.Add(kvp.Key, subtileLayers2.ID);
			}
		}
	}

	private void InitTerrainSectors()
	{
		terrainSectors = new Dictionary<SurfaceType.TransportType, Sector[][]>();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayer> item in moveMapLayer)
		{
			Sector[][] map = null;
			Common.InitJaggedArray(ref map, The.Map.NoOfSectorsAcrossWidth, The.Map.NoOfSectorsAcrossHeight);
			for (int i = 0; i < Common.GetJaggedArrayWidth(map); i++)
			{
				for (int j = 0; j < Common.GetJaggedArrayHeight(map); j++)
				{
					map[i][j] = new Sector(new Point(i, j), 48, The.Map.mapSubtileWidth, The.Map.mapSubtileHeight);
				}
			}
			terrainSectors.Add(item.Key, map);
		}
	}

	public DiscomfortMap GetDiscomfortMap()
	{
		return (DiscomfortMap)Children[0].Child;
	}

	public bool CycleOnce()
	{
		DiscomfortMap discomfortMap = GetDiscomfortMap();
		_ = IDName == "ExposedHumanNormal";
		switch (phase)
		{
		case Phase.ComputeChildMaps:
			if (discomfortMap.DoCycle())
			{
				phase = Phase.RemoveEmptySectors;
			}
			return false;
		case Phase.RemoveEmptySectors:
			RemoveEmptySectors();
			phase = Phase.GetSectorsToAdd;
			return false;
		case Phase.GetSectorsToAdd:
			GetSectorsToAdd();
			phase = Phase.AddChildMaps;
			IsReady = false;
			return false;
		case Phase.AddChildMaps:
			if (AddChildMapSectors(discomfortMap))
			{
				phase = Phase.ComputeRegionMap;
			}
			return false;
		case Phase.ComputeRegionMap:
			if (ComputeRegionMaps())
			{
				phase = Phase.Completed;
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private bool ComputeRegionMaps()
	{
		SurfaceType.TransportType key = transportsToInclude[cycleRegionMapTransportIndex];
		DependentRegionMap dependentRegionMap = (DependentRegionMap)Layers[key].RegionMap;
		if (dependentRegionMap.IsPaused)
		{
			IsPaused = true;
			return false;
		}
		dependentRegionMap.ResetComputePointIfNeeded();
		if (dependentRegionMap.CycleOnce())
		{
			cycleRegionMapTransportIndex++;
		}
		if (cycleRegionMapTransportIndex == transportsToInclude.Length)
		{
			cycleRegionMapTransportIndex = 0;
			return true;
		}
		return false;
	}

	private void RemoveEmptySectors()
	{
		sectorsWereRemoved = false;
		List<Point> list = new List<Point>();
		GetAllSectors(list);
		foreach (Point sectorCoords in list)
		{
			if (!Children.TrueForAll((Dependence d) => d.Child.Map.GetSector(sectorCoords.X, sectorCoords.Y) == null))
			{
				continue;
			}
			foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayer> item in moveMapLayer)
			{
				SubtileSector subtileSector = item.Value.Sectors[sectorCoords.X][sectorCoords.Y];
				if (subtileSector != null)
				{
					DestroySector(subtileSector, item.Key, item.Value);
					sectorsWereRemoved = true;
				}
			}
		}
	}

	private void DestroySector(SubtileSector sector, SurfaceType.TransportType transport, SubtileLayer map)
	{
		RegionMap regionMap = Layers[transport].RegionMap;
		map.Sectors[sector.Coords.X][sector.Coords.Y] = null;
		HashSet<ushort> regions = sector.GetRegions();
		((DependentRegionMap)regionMap).SetRegionsFromRemovedSectors(regions);
	}

	private void GetSectorsToAdd()
	{
		listOfSectorsToAdd.Clear();
		List<Point> listOfDirtySectors = null;
		List<Point> allDiscomfortSectors = null;
		GetDiscomfortSectorsToAdd(ref allDiscomfortSectors, ref listOfDirtySectors);
		if (listOfDirtySectors != null)
		{
			listOfSectorsToAdd.AddRange(listOfDirtySectors);
		}
		if (allDiscomfortSectors == null)
		{
			return;
		}
		HashSet<Point> hashSet = new HashSet<Point>();
		if (!terrainSectorsAreDirty)
		{
			return;
		}
		GetSectorsToAddFromTerrain(hashSet);
		if (hashSet.Count > 0)
		{
			List<Point> list = hashSet.ToList();
			list.RemoveAll((Point s) => !allDiscomfortSectors.Contains(s));
			listOfSectorsToAdd.AddRange(list);
		}
	}

	public void NotifyRegionMapUnpaused()
	{
		if (phase == Phase.ComputeRegionMap && IsPaused)
		{
			IsPaused = false;
		}
		else if (!The.Sim.CycleManager.IsRegistered(this) && phase == Phase.Completed)
		{
			phase = Phase.ComputeRegionMap;
			The.Sim.CycleManager.Register(this, CycleManager.Priority.High);
		}
	}

	private void GetAllSectors(List<Point> listOfSectors)
	{
		int noOfSectorsAcrossWidth = The.Map.NoOfSectorsAcrossWidth;
		int noOfSectorsAcrossHeight = The.Map.NoOfSectorsAcrossHeight;
		for (int i = 0; i < noOfSectorsAcrossWidth; i++)
		{
			for (int j = 0; j < noOfSectorsAcrossHeight; j++)
			{
				listOfSectors.Add(new Point(i, j));
			}
		}
	}

	private void GetDiscomfortSectorsToAdd(ref List<Point> listOfAllSectors, ref List<Point> listOfDirtySectors)
	{
		foreach (Dependence child in Children)
		{
			child.Child.Map.GetAllSectors(ref listOfAllSectors);
			child.Child.Map.GetDirtySectors(ref listOfDirtySectors);
		}
	}

	private void GetSectorsToAddFromTerrain(HashSet<Point> listOfSectors)
	{
		if (!terrainSectorsAreDirty)
		{
			return;
		}
		foreach (KeyValuePair<SurfaceType.TransportType, Sector[][]> terrainSector in terrainSectors)
		{
			int jaggedArrayWidth = Common.GetJaggedArrayWidth(terrainSector.Value);
			int jaggedArrayHeight = Common.GetJaggedArrayHeight(terrainSector.Value);
			for (int i = 0; i < jaggedArrayWidth; i++)
			{
				for (int j = 0; j < jaggedArrayHeight; j++)
				{
					Sector sector = terrainSector.Value[i][j];
					if (sector.SubtilesAreDirty)
					{
						listOfSectors.Add(new Point(i, j));
						sector.SubtilesAreDirty = false;
					}
				}
			}
		}
		terrainSectorsAreDirty = false;
	}

	private bool AddChildMapSectors(DiscomfortMap dMap)
	{
		if (listOfSectorsToAdd.Count == 0)
		{
			if (!sectorsWereRemoved)
			{
				phase = Phase.ComputeChildMaps;
				return true;
			}
			phase = Phase.ComputeRegionMap;
			return false;
		}
		Point sectorCoords = listOfSectorsToAdd[cycleSectorIndex];
		TileSector sector = dMap.Map.Sectors[sectorCoords.X][sectorCoords.Y];
		Parallel.ForEach(transportsToInclude, delegate(SurfaceType.TransportType transport)
		{
			RegionMap regionMap = layers[transport].RegionMap;
			float num = 1f;
			num = ((transport != SurfaceType.TransportType.Foot) ? 0.25f : 1f);
			SubtileSector sector2 = The.Map.TerrainCosts[transport].GetSector(sectorCoords.X, sectorCoords.Y);
			TileSector sector3 = dMap.Map.GetSector(sectorCoords.X, sectorCoords.Y);
			bool isNew;
			SubtileSector orCreateSector = moveMapLayer[transport].GetOrCreateSector(sectorCoords.X, sectorCoords.Y, out isNew);
			MapManager.SubtileValue[][] values = orCreateSector.Values;
			if (isNew)
			{
				regionMap.MarkDirtySector(orCreateSector);
			}
			byte b = 0;
			bool flag = false;
			byte[] array = null;
			bool[] array2 = null;
			for (int i = 0; i < sector.TileArea.Width; i++)
			{
				if (sector3 != null)
				{
					array = sector3.Map[i];
					array2 = sector3.IsBlocked[i];
				}
				for (int j = 0; j < sector.TileArea.Height; j++)
				{
					int num2 = 3 * i;
					int num3 = 3 * j;
					if (sector3 != null)
					{
						b = (byte)(num * (float)(int)array[j]);
						flag = array2[j];
					}
					for (int k = num2; k < num2 + 3; k++)
					{
						MapManager.SubtileValue[] array3 = values[k];
						for (int l = num3; l < num3 + 3; l++)
						{
							byte b2;
							if (flag)
							{
								b2 = 0;
							}
							else
							{
								byte cost = MapManager.GetCost(sector2.Values[k][l]);
								b2 = (byte)((cost > 0) ? ((byte)Common.ClampTop(cost + b, 31)) : 0);
							}
							byte cost2 = MapManager.GetCost(array3[l]);
							array3[l] = MapManager.SetCost(array3[l], b2);
							if (!isNew && ((b2 == 0 && cost2 != 0) || (b2 != 0 && cost2 == 0)))
							{
								regionMap.MarkDirtySector(orCreateSector);
							}
						}
					}
				}
			}
		});
		cycleSectorIndex++;
		if (cycleSectorIndex == listOfSectorsToAdd.Count)
		{
			phase = Phase.ComputeChildMaps;
			cycleSectorIndex = 0;
			return true;
		}
		return false;
	}

	private static void ClearTilesInSector(Sector sector, byte[][] map)
	{
		for (int i = sector.TileArea.Left; i < sector.TileArea.Right; i++)
		{
			byte[] array = map[i];
			for (int j = sector.TileArea.Top; j < sector.TileArea.Bottom; j++)
			{
				array[j] = 0;
			}
		}
	}

	public MovementMap GetCurrent()
	{
		return this;
	}

	public override string ToString()
	{
		return "Move map: " + IDName;
	}

	public void ClearMap()
	{
		throw new NotImplementedException();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "MovementMap");
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				phase = Phase.ComputeChildMaps;
				The.Sim.CycleManager.Register(this, CycleManager.Priority.High);
			}
		}
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"Movemap {IDName}, {phase}");
	}

	public void Destroy()
	{
		if (The.Sim.CycleManager.IsRegistered(this))
		{
			The.Sim.CycleManager.UnRegister(this);
		}
		The.Map.AllMovementMaps.Remove(this);
		RemoveIDEntry();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayers> layer in layers)
		{
			layer.Value.Destroy(destroyRegionMapAndBottomLayer: false);
		}
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
		terrainSectorsAreDirty = sn.DoBool(terrainSectorsAreDirty);
		terrainSectors = sn.DoMultiArray(terrainSectors);
		phase = sn.DoEnum(phase);
		listOfSectorsToAdd = sn.DoList(listOfSectorsToAdd);
		cycleSectorIndex = sn.DoInt32(cycleSectorIndex);
		IDName = sn.DoString(IDName);
		IsReady = sn.DoBool(IsReady);
		IsPaused = sn.DoBool(IsPaused);
		Children = sn.DoList(Children);
		transportsToInclude = sn.DoArray(transportsToInclude);
		cycleRegionMapTransportIndex = sn.DoInt32(cycleRegionMapTransportIndex);
		sectorsWereRemoved = sn.DoBool(sectorsWereRemoved);
		updateInterval = sn.DoDouble(updateInterval);
		snapshotMoveMapLayer = sn.DoDictionary(snapshotMoveMapLayer);
		snapshotClientLayers = sn.DoDictionary(snapshotClientLayers);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(moveMapLayer);
		sn.Ignore(layers);
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
		moveMapLayer = new Dictionary<SurfaceType.TransportType, SubtileLayer>();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayerID> item in snapshotMoveMapLayer)
		{
			moveMapLayer.Add(item.Key, LookUp<SubtileLayer, SubtileLayerID>.FindByID(item.Value));
		}
		layers = new Dictionary<SurfaceType.TransportType, SubtileLayers>();
		foreach (KeyValuePair<SurfaceType.TransportType, SubtileLayersID> snapshotClientLayer in snapshotClientLayers)
		{
			layers.Add(snapshotClientLayer.Key, LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotClientLayer.Value));
		}
		foreach (Dependence child in Children)
		{
			child.LoadPostProcess(sn);
		}
		foreach (KeyValuePair<SurfaceType.TransportType, Sector[][]> terrainSector in terrainSectors)
		{
			Sector[][] value = terrainSector.Value;
			foreach (Sector[] array in value)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j].LoadPostProcess(sn);
				}
			}
		}
		CreateRegulators();
	}
}
