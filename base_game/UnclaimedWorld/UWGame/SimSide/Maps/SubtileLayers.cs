using System;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Maps;

public class SubtileLayers : ISnapshot, ILookUp<SubtileLayers, SubtileLayersID>
{
	public SubtileLayer[] Layers;

	private SubtileLayerID[] snapshotLayers;

	public RegionMap RegionMap;

	private CyclableID snapshotRegionMap;

	private SubtileLayersID id = SubtileLayersID.Invalid;

	private static SubtileLayersID IDCounter = SubtileLayersID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SubtileLayersID ID
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

	public SubtileLayers()
	{
	}

	public SubtileLayers(MovementMap moveMap, string name, SubtileLayer bottomLayer, SubtileLayer secondLayer, TerrainRegionMap terrainRegionMap)
	{
		Layers = new SubtileLayer[2] { bottomLayer, secondLayer };
		RegionMap = new DependentRegionMap(moveMap, this, terrainRegionMap);
		RegionMap.IDName = name;
		AddToLookup();
	}

	public SubtileLayers(string name, SubtileLayer bottomLayer)
	{
		Layers = new SubtileLayer[1] { bottomLayer };
		RegionMap = new TerrainRegionMap(this);
		RegionMap.IDName = name;
		AddToLookup();
	}

	public ushort GetRegion(Point subtile)
	{
		return GetRegion(subtile.X, subtile.Y);
	}

	public ushort GetRegion(int subtileX, int subtileY, bool inProgress = false)
	{
		Layers[0].GetSectorAndRelativeCoords(subtileX, subtileY, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		return GetRegion(sectorX, sectorY, relativeX, relativeY, inProgress);
	}

	public ushort GetRegion(int sectorX, int sectorY, int relativeX, int relativeY, bool inProgress = false)
	{
		SubtileSector subtileSector = (inProgress ? GetUnfinishedSector(new Point(sectorX, sectorY)) : GetSector(sectorX, sectorY));
		if (subtileSector != null)
		{
			if (!inProgress)
			{
				return subtileSector.GetRegion(relativeX, relativeY);
			}
			return subtileSector.GetRegionInProgress(relativeX, relativeY);
		}
		return 0;
	}

	public SubtileSector GetSectorFromSubtile(Point subtile, out int layerIndex)
	{
		Layers[0].GetSectorAndRelativeCoords(subtile.X, subtile.Y, out var sectorX, out var sectorY, out var _, out var _);
		return GetSector(sectorX, sectorY, out layerIndex);
	}

	public SubtileSector GetUnfinishedSectorFromSubtile(Point subtile, out int layerIndex)
	{
		Layers[0].GetSectorAndRelativeCoords(subtile.X, subtile.Y, out var sectorX, out var sectorY, out var _, out var _);
		return GetSector(sectorX, sectorY, out layerIndex, ignoreNewUnfinishedSectors: false);
	}

	public SubtileSector GetSectorFromSubtile(Point subtile)
	{
		int layerIndex;
		return GetSectorFromSubtile(subtile, out layerIndex);
	}

	public SubtileSector GetUnfinishedSector(Point sectorCoords)
	{
		int layerIndex;
		return GetSector(sectorCoords.X, sectorCoords.Y, out layerIndex, ignoreNewUnfinishedSectors: false);
	}

	public SubtileSector GetSector(int sectorX, int sectorY)
	{
		int layerIndex;
		return GetSector(sectorX, sectorY, out layerIndex);
	}

	public SubtileSector GetOverriddenSector(int sectorX, int sectorY)
	{
		return Layers[0].Sectors[sectorX][sectorY];
	}

	public SubtileSector GetSector(int sectorX, int sectorY, out int layerIndex, bool ignoreNewUnfinishedSectors = true)
	{
		layerIndex = -1;
		for (int num = Layers.Length - 1; num >= 0; num--)
		{
			SubtileSector subtileSector = Layers[num].Sectors[sectorX][sectorY];
			if (subtileSector != null && (!ignoreNewUnfinishedSectors || subtileSector.HasFinishedFirstRun))
			{
				layerIndex = num;
				return subtileSector;
			}
		}
		return null;
	}

	public void SetValueOnBottomLayer(int x, int y, MapManager.SubtileValue value)
	{
		Layers[0].SetValue(x, y, value);
	}

	public void SetValueOnBottomLayer(Point subtilePos, MapManager.SubtileValue value)
	{
		Layers[0].SetValue(subtilePos.X, subtilePos.Y, value);
	}

	public MapManager.SubtileValue GetValue(int sectorX, int sectorY, int relativeX, int relativeY, out int layerIndex)
	{
		return GetSector(sectorX, sectorY, out layerIndex)?.GetValue(relativeX, relativeY).Value ?? MapManager.SubtileValue.Blocked;
	}

	public MapManager.SubtileValue GetValue(int sectorX, int sectorY, int relativeX, int relativeY)
	{
		return GetSector(sectorX, sectorY)?.GetValue(relativeX, relativeY).Value ?? MapManager.SubtileValue.Blocked;
	}

	public MapManager.SubtileValue GetValue(Point subtilePos)
	{
		return GetValue(subtilePos.X, subtilePos.Y);
	}

	public MapManager.SubtileValue GetValue(SubtilePos subtilePos)
	{
		return GetValue(subtilePos.X, subtilePos.Y);
	}

	public MapManager.SubtileValue GetValue(int absoluteX, int absoluteY)
	{
		Layers[0].GetSectorAndRelativeCoords(absoluteX, absoluteY, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		return GetValue(sectorX, sectorY, relativeX, relativeY);
	}

	public MapManager.SubtileValue GetValue(int absoluteX, int absoluteY, out int layerIndex)
	{
		Layers[0].GetSectorAndRelativeCoords(absoluteX, absoluteY, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		return GetValue(sectorX, sectorY, relativeX, relativeY, out layerIndex);
	}

	public void Destroy(bool destroyRegionMapAndBottomLayer)
	{
		RegionMap.Destroy();
		for (int i = 0; i < Layers.Length; i++)
		{
			if (i > 0 || destroyRegionMapAndBottomLayer)
			{
				Layers[i].Destroy();
			}
		}
		RemoveIDEntry();
	}

	public SubtileLayersID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= SubtileLayersID.Invalid)
		{
			throw new Exception("Astounding, SubtileLayersID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SubtileLayersID SnapshotID(Snapshotter sn, SubtileLayersID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SubtileLayersID.Invalid)
		{
			LookUp<SubtileLayers, SubtileLayersID>.Add(ID, this);
			LookUp<SubtileLayers, SubtileLayersID>.SetLoadPostProcessOrder(10);
		}
	}

	public void SetInvalid()
	{
		id = SubtileLayersID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<SubtileLayers, SubtileLayersID>.Remove(this);
	}

	void ILookUp<SubtileLayers, SubtileLayersID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SubtileLayersID.First;
	}

	void ILookUp<SubtileLayers, SubtileLayersID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<SubtileLayers, SubtileLayersID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotLayers = Layers.Select((SubtileLayer l) => l.ID).ToArray();
		}
		snapshotLayers = sn.DoArray(snapshotLayers);
		snapshotRegionMap = sn.SnapshotID<ICyclable, CyclableID>(RegionMap).Value;
		sn.Ignore(Layers);
		sn.Ignore(RegionMap);
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
		Layers = snapshotLayers.Select((SubtileLayerID l) => LookUp<SubtileLayer, SubtileLayerID>.FindByID(l)).ToArray();
		RegionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(snapshotRegionMap);
	}
}
