using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class SubtileLayer : Layer, ISnapshot, ILookUp<SubtileLayer, SubtileLayerID>
{
	public SubtileSector[][] Sectors;

	private SubtileLayerID id = SubtileLayerID.Invalid;

	private static SubtileLayerID IDCounter = SubtileLayerID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override int SectorSize => 48;

	public SubtileLayerID ID
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

	public SubtileLayer(ushort subTileWidth, ushort subTileHeight)
		: base(subTileWidth, subTileHeight)
	{
		AddToLookup();
		Common.InitJaggedArray(ref Sectors, The.Map.NoOfSectorsAcrossWidth, The.Map.NoOfSectorsAcrossHeight);
	}

	public SubtileLayer()
	{
	}

	public void CreateAllSectors()
	{
		for (int i = 0; i < base.SectorsAcrossHeight; i++)
		{
			for (int j = 0; j < base.SectorsAcrossWidth; j++)
			{
				Sectors[j][i] = new SubtileSector(this, j, i, setHasFinished: true);
			}
		}
	}

	public SubtileSector GetSector(int sectorX, int sectorY)
	{
		return Sectors[sectorX][sectorY];
	}

	public SubtileSector GetSectorFromSubtiles(int absoluteSubtileX, int absoluteSubtileY)
	{
		return Sectors[absoluteSubtileX / 48][absoluteSubtileY / 48];
	}

	public SubtileSector GetOrCreateSector(int sectorX, int sectorY, out bool isNew)
	{
		SubtileSector subtileSector = Sectors[sectorX][sectorY];
		if (subtileSector == null)
		{
			subtileSector = new SubtileSector(this, sectorX, sectorY);
			Sectors[sectorX][sectorY] = subtileSector;
			isNew = true;
		}
		else
		{
			isNew = false;
		}
		return subtileSector;
	}

	public void SetValue(int x, int y, MapManager.SubtileValue value)
	{
		GetSectorAndRelativeCoords(x, y, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		SubtileSector subtileSector = Sectors[sectorX][sectorY];
		if (subtileSector != null)
		{
			subtileSector.Values[relativeX][relativeY] = value;
		}
	}

	public MapManager.SubtileValue? GetValue(int x, int y)
	{
		GetSectorAndRelativeCoords(x, y, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		SubtileSector subtileSector = Sectors[sectorX][sectorY];
		if (subtileSector != null)
		{
			return subtileSector.Values[relativeX][relativeY];
		}
		return null;
	}

	public void IterateDirtySectors(Action<SubtileSector> action)
	{
		IterateSectors(delegate(SubtileSector s)
		{
			if (s.BlockedStatusHasChanged)
			{
				action(s);
			}
		});
	}

	public void IterateSectors(Action<SubtileSector> action)
	{
		for (int i = 0; i < base.SectorsAcrossHeight; i++)
		{
			for (int j = 0; j < base.SectorsAcrossWidth; j++)
			{
				SubtileSector subtileSector = Sectors[j][i];
				if (subtileSector != null)
				{
					action(subtileSector);
				}
			}
		}
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public SubtileLayerID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= SubtileLayerID.Invalid)
		{
			throw new Exception("Astounding, SubtileValueArrayID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SubtileLayerID SnapshotID(Snapshotter sn, SubtileLayerID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SubtileLayerID.Invalid)
		{
			LookUp<SubtileLayer, SubtileLayerID>.Add(ID, this);
			LookUp<SubtileLayer, SubtileLayerID>.SetLoadPostProcessOrder(0);
		}
	}

	public void SetInvalid()
	{
		id = SubtileLayerID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<SubtileLayer, SubtileLayerID>.Remove(this);
	}

	void ILookUp<SubtileLayer, SubtileLayerID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SubtileLayerID.First;
	}

	void ILookUp<SubtileLayer, SubtileLayerID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<SubtileLayer, SubtileLayerID>.Create();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		Sectors = sn.DoJaggedArray(Sectors);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		sn.RegisterLoadPostProcessCall(this);
		IterateSectors(delegate(SubtileSector s)
		{
			s.LoadPostProcess(sn);
		});
	}
}
