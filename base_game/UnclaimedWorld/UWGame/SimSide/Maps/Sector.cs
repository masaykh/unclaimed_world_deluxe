using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class Sector : ISnapshot
{
	public bool SectorIsClear = true;

	public bool SubtilesAreDirty;

	public Rectangle TileArea;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Sector(Point sectorCoords, int sectorSizeInTiles, int mapWidth, int mapHeight)
	{
		TileArea = default(Rectangle);
		TileArea.X = sectorCoords.X * sectorSizeInTiles;
		TileArea.Y = sectorCoords.Y * sectorSizeInTiles;
		TileArea.Width = Common.ClampTop(sectorSizeInTiles, sectorSizeInTiles - (TileArea.X + sectorSizeInTiles - mapWidth));
		TileArea.Height = Common.ClampTop(sectorSizeInTiles, sectorSizeInTiles - (TileArea.Y + sectorSizeInTiles - mapHeight));
	}

	public Sector()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		SubtilesAreDirty = sn.DoBool(SubtilesAreDirty);
		SectorIsClear = sn.DoBool(SectorIsClear);
		TileArea = sn.DoRectangle(TileArea);
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
	}
}
