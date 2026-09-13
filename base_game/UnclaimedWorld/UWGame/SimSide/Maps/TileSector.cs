using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class TileSector : ISnapshot
{
	public Point Coords;

	public byte[][] Map;

	public bool[][] IsBlocked;

	protected byte[][] backBufferMap1;

	protected byte[][] backBufferMap2;

	public bool IsDirty = true;

	public Rectangle TileArea;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public TileSector()
	{
	}

	public TileSector(TileLayer parent, int sectorCoordsX, int sectorCoordsY)
	{
		Coords = new Point(sectorCoordsX, sectorCoordsY);
		TileArea = default(Rectangle);
		int sectorSize = parent.SectorSize;
		Common.InitJaggedArray(ref backBufferMap1, sectorSize, sectorSize);
		Common.InitJaggedArray(ref backBufferMap2, sectorSize, sectorSize);
		Map = backBufferMap1;
		Common.InitJaggedArray(ref IsBlocked, sectorSize, sectorSize);
		TileArea.X = sectorCoordsX * sectorSize;
		TileArea.Y = sectorCoordsY * sectorSize;
		TileArea.Width = Common.ClampTop(sectorSize, sectorSize - (TileArea.X + sectorSize - parent.Width));
		TileArea.Height = Common.ClampTop(sectorSize, sectorSize - (TileArea.Y + sectorSize - parent.Height));
	}

	public void Initialize()
	{
	}

	public void SetValue(int relativeX, int relativeY, byte value, byte? blockingLimit)
	{
		Map[relativeX][relativeY] = value;
		if (blockingLimit.HasValue)
		{
			if (value > blockingLimit.Value)
			{
				IsBlocked[relativeX][relativeY] = true;
			}
			else
			{
				IsBlocked[relativeX][relativeY] = false;
			}
		}
	}

	public byte GetValue(ushort relativeX, ushort relativeY)
	{
		return Map[relativeX][relativeY];
	}

	public bool GetIsBlocked(ushort relativeX, ushort relativeY)
	{
		return IsBlocked[relativeX][relativeY];
	}

	public void ClearMap()
	{
		Common.ClearJaggedArray(Map);
		Common.ClearJaggedArray(IsBlocked);
		IsDirty = false;
	}

	public void AddSector(TileSector sectorToAdd, float weight)
	{
		for (int i = 0; i < TileArea.Width; i++)
		{
			byte[] array = Map[i];
			bool[] array2 = IsBlocked[i];
			byte[] array3 = sectorToAdd.Map[i];
			bool[] array4 = sectorToAdd.IsBlocked[i];
			for (int j = 0; j < TileArea.Height; j++)
			{
				array[j] = (byte)Common.ClampTop((float)(int)array[j] + weight * (float)(int)array3[j], 255f);
				array2[j] = array2[j] || array4[j];
			}
		}
	}

	public void SwitchBuffers()
	{
		if (Map == backBufferMap1)
		{
			Map = backBufferMap2;
		}
		else
		{
			Map = backBufferMap1;
		}
	}

	public bool SectorHasChanged()
	{
		for (int i = 0; i < TileArea.Width; i++)
		{
			for (int j = 0; j < TileArea.Height; j++)
			{
				if (backBufferMap1[i][j] != backBufferMap2[i][j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Map = sn.DoJaggedArray(Map);
		Coords = sn.DoPoint(Coords);
		TileArea = sn.DoRectangle(TileArea);
		IsBlocked = sn.DoJaggedArray(IsBlocked);
		IsDirty = sn.DoBool(IsDirty);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (backBufferMap1 != Map)
			{
				sn.DoJaggedArray(backBufferMap1);
				sn.DoJaggedArray<byte>(null);
				sn.Ignore(backBufferMap2);
			}
			if (backBufferMap2 != Map)
			{
				sn.DoJaggedArray<byte>(null);
				sn.DoJaggedArray(backBufferMap2);
				sn.Ignore(backBufferMap1);
			}
		}
		else
		{
			backBufferMap1 = sn.DoJaggedArray(backBufferMap1);
			backBufferMap2 = sn.DoJaggedArray(backBufferMap2);
		}
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
		if (backBufferMap1 == null)
		{
			backBufferMap1 = Map;
		}
		if (backBufferMap2 == null)
		{
			backBufferMap2 = Map;
		}
	}
}
