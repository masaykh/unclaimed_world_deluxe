using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class TileLayer : Layer
{
	public TileSector[][] Sectors;

	private const byte defaultValue = 0;

	public byte? BlockingLimit;

	public HashSet<Point> AffectedSectors = new HashSet<Point>();

	public HashSet<Point> PreviouslyAffectedSectors = new HashSet<Point>();

	private Snapshotter.Version version;

	public override int SectorSize => 16;

	public new bool IsSnapshotted { get; set; }

	public TileLayer()
	{
	}

	public TileLayer(ushort width, ushort height)
		: base(width, height)
	{
		Common.InitJaggedArray(ref Sectors, base.SectorsAcrossWidth, base.SectorsAcrossHeight);
	}

	public bool SetValue(int x, int y, double value, InfluenceMap.Operation operation, out TileSector affectedSector)
	{
		affectedSector = null;
		GetSectorAndRelativeCoords(x, y, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		TileSector tileSector = Sectors[sectorX][sectorY];
		if (sectorX > 0)
		{
			_ = 0;
		}
		byte b = 0;
		if (tileSector == null)
		{
			switch (operation)
			{
			case InfluenceMap.Operation.SetValue:
				if (value == 0.0)
				{
					return false;
				}
				tileSector = CreateSector(sectorX, sectorY);
				break;
			case InfluenceMap.Operation.AddToExisting:
				if (value <= 0.0)
				{
					return false;
				}
				tileSector = CreateSector(sectorX, sectorY);
				break;
			}
		}
		else
		{
			b = tileSector.GetValue(relativeX, relativeY);
		}
		if (tileSector != null)
		{
			byte value2;
			if (operation == InfluenceMap.Operation.AddToExisting)
			{
				value2 = (byte)Common.Clamp(value + (double)(int)b, 0.0, 255.0);
			}
			else
			{
				if (value == (double)(int)b)
				{
					return false;
				}
				value2 = (byte)Common.Clamp(value, 0.0, 255.0);
			}
			affectedSector = tileSector;
			tileSector.SetValue(relativeX, relativeY, value2, BlockingLimit);
			return true;
		}
		return false;
	}

	public void IterateSectors(Action<TileSector> action)
	{
		for (int i = 0; i < base.SectorsAcrossWidth; i++)
		{
			for (int j = 0; j < base.SectorsAcrossHeight; j++)
			{
				TileSector tileSector = Sectors[i][j];
				if (tileSector != null)
				{
					action(tileSector);
				}
			}
		}
	}

	public void ClearMaps()
	{
		IterateSectors(delegate(TileSector s)
		{
			s.ClearMap();
		});
	}

	public void SwitchBuffers()
	{
		IterateSectors(delegate(TileSector s)
		{
			s.SwitchBuffers();
		});
	}

	protected TileSector GetOrCreateSector(int x, int y, out ushort sectorX, out ushort sectorY, out ushort relativeX, out ushort relativeY)
	{
		GetSectorAndRelativeCoords(x, y, out sectorX, out sectorY, out relativeX, out relativeY);
		return GetOrCreateSector(sectorX, sectorY);
	}

	public TileSector GetOrCreateSector(int sectorX, int sectorY)
	{
		TileSector tileSector = Sectors[sectorX][sectorY];
		if (tileSector == null)
		{
			tileSector = CreateSector(sectorX, sectorY);
		}
		return tileSector;
	}

	private TileSector CreateSector(int sectorX, int sectorY)
	{
		TileSector tileSector = new TileSector(this, sectorX, sectorY);
		Sectors[sectorX][sectorY] = tileSector;
		return tileSector;
	}

	public void GetDirtySectors(List<TileSector> listToAddTo)
	{
		IterateSectors(delegate(TileSector s)
		{
			if (s.IsDirty)
			{
				listToAddTo.Add(s);
			}
		});
	}

	public void GetAllSectors(ref List<Point> listToAddTo)
	{
		List<Point> placeHolderListToAddTo = listToAddTo;
		IterateSectors(delegate(TileSector s)
		{
			Common.AddToList(ref placeHolderListToAddTo, s.Coords);
		});
		listToAddTo = placeHolderListToAddTo;
	}

	public HashSet<Point> GetAllSectors()
	{
		HashSet<Point> set = new HashSet<Point>();
		IterateSectors(delegate(TileSector s)
		{
			Common.AddToList(ref set, s.Coords);
		});
		return set;
	}

	public void GetDirtySectors(ref HashSet<Point> listToAddTo)
	{
		HashSet<Point> placeHolderListToAddTo = listToAddTo;
		IterateSectors(delegate(TileSector s)
		{
			if (s.IsDirty)
			{
				Common.AddToList(ref placeHolderListToAddTo, s.Coords);
			}
		});
		listToAddTo = placeHolderListToAddTo;
	}

	public void GetDirtySectors(ref List<Point> listToAddTo)
	{
		List<Point> placeHolderListToAddTo = listToAddTo;
		IterateSectors(delegate(TileSector s)
		{
			if (s.IsDirty)
			{
				Common.AddToList(ref placeHolderListToAddTo, s.Coords);
			}
		});
		listToAddTo = placeHolderListToAddTo;
	}

	public byte GetValue(Point pos)
	{
		TileSector sector;
		return GetValue(pos.X, pos.Y, out sector);
	}

	public byte GetValue(int x, int y)
	{
		TileSector sector;
		return GetValue(x, y, out sector);
	}

	public TileSector GetSector(int sectorX, int sectorY)
	{
		return Sectors[sectorX][sectorY];
	}

	public byte GetValue(int x, int y, out TileSector sector)
	{
		GetSectorAndRelativeCoords(x, y, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		sector = Sectors[sectorX][sectorY];
		if (sector != null)
		{
			return sector.GetValue(relativeX, relativeY);
		}
		return 0;
	}

	public bool GetIsBlocked(Point tilePos)
	{
		return GetIsBlocked(tilePos.X, tilePos.Y);
	}

	public bool GetIsBlocked(int x, int y)
	{
		GetSectorAndRelativeCoords(x, y, out var sectorX, out var sectorY, out var relativeX, out var relativeY);
		return Sectors[sectorX][sectorY]?.GetIsBlocked(relativeX, relativeY) ?? false;
	}

	public void BeginDrawing()
	{
		StoreSectorsBeforeClear();
		SwitchBuffers();
	}

	public void StoreSectorsBeforeClear()
	{
		PreviouslyAffectedSectors.Clear();
		foreach (Point affectedSector in AffectedSectors)
		{
			PreviouslyAffectedSectors.Add(affectedSector);
		}
		AffectedSectors.Clear();
	}

	public HashSet<Point> GetSectorsThatHaveChanged(HashSet<Point> sectorsToCheck)
	{
		HashSet<Point> list = new HashSet<Point>();
		foreach (Point item in sectorsToCheck)
		{
			TileSector sector = GetSector(item.X, item.Y);
			if (sector != null && sector.SectorHasChanged())
			{
				Common.AddToList(ref list, sector.Coords);
			}
		}
		return list;
	}

	public void CompareOldAndNewSectors()
	{
		HashSet<Point> hashSet = new HashSet<Point>();
		hashSet.UnionWith(PreviouslyAffectedSectors);
		hashSet.UnionWith(AffectedSectors);
		foreach (Point item in GetSectorsThatHaveChanged(hashSet))
		{
			TileSector sector = GetSector(item.X, item.Y);
			if (sector != null)
			{
				sector.IsDirty = true;
			}
		}
	}

	public void RemoveSectors(HashSet<Point> sectorsToRemove)
	{
		foreach (Point item in sectorsToRemove)
		{
			Sectors[item.X][item.Y] = null;
		}
	}

	public void DrawLinearInfluenceCircle(Point pos, int centerValue, InfluenceMap.Operation operation, InfluenceMap.Falloff falloffYesNo, InfluenceMap.CircleParameter circleParam, int paramValue, int? maxRadius = null)
	{
		pos = InfluenceMap.ComputeLinearCircle(base.Width, base.Height, pos, centerValue, falloffYesNo, circleParam, paramValue, maxRadius, out var isDrawingAPositiveCircle, out var falloffEachTile, out var minX, out var minY, out var maxX, out var maxY);
		double num = centerValue;
		for (int i = minX; i <= maxX; i++)
		{
			for (int j = minY; j <= maxY; j++)
			{
				if (falloffYesNo == InfluenceMap.Falloff.Yes)
				{
					float num2 = Common.DistanceOctile(new Point(i, j), pos);
					num = (double)centerValue + (double)num2 * falloffEachTile;
				}
				num = (isDrawingAPositiveCircle ? Common.ClampBottom(num, 0.0) : Common.ClampTop(num, 0.0));
				TileSector affectedSector = null;
				SetValue(i, j, num, operation, out affectedSector);
				if (affectedSector != null)
				{
					AffectedSectors.Add(affectedSector.Coords);
				}
			}
		}
	}

	public new virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Sectors = sn.DoJaggedArray(Sectors);
		BlockingLimit = sn.DoByteNullable(BlockingLimit);
		AffectedSectors = sn.DoHashSet(AffectedSectors);
		PreviouslyAffectedSectors = sn.DoHashSet(PreviouslyAffectedSectors);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		sn.RegisterLoadPostProcessCall(this);
		IterateSectors(delegate(TileSector s)
		{
			s.LoadPostProcess(sn);
		});
	}
}
