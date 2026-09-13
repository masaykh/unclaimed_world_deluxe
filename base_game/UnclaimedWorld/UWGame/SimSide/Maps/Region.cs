using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class Region : ISnapshot
{
	public Color DebugColor;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Vector2 CenterLocation { get; private set; }

	public Point CenterInSubtiles { get; private set; }

	public ushort Color { get; private set; }

	public bool IsSnapshotted { get; set; }

	public Region(Point centerInSubTiles, ushort color)
	{
		if (color == 321 && centerInSubTiles.X == 118)
		{
			_ = centerInSubTiles.Y;
			_ = 61;
		}
		CenterInSubtiles = centerInSubTiles;
		CenterLocation = MapManager.SubTileToWorldPos(centerInSubTiles);
		Color = color;
		DebugColor = Common.GetRandomColorFromSeed(color);
	}

	public Region()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		CenterLocation = sn.DoVector2(CenterLocation);
		CenterInSubtiles = sn.DoPoint(CenterInSubtiles);
		Color = sn.DoUInt16(Color);
		sn.Ignore(DebugColor);
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
		DebugColor = Common.GetRandomColorFromSeed(Color);
	}
}
