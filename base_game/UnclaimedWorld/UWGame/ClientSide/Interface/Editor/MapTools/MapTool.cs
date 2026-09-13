using System;
using System.Collections.Generic;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.Editor.MapTools;

public abstract class MapTool
{
	public struct SubTileAndChange
	{
		public SubtilePos SubtilePos;

		public float Change;

		public SubTileAndChange(SubtilePos pos, float change)
		{
			SubtilePos = pos;
			Change = change;
		}
	}

	public struct TileAndChange
	{
		public TilePos TilePos;

		public float Change;
	}

	public virtual List<SubTileAndChange> GetAffectedSubtiles(SubtilePos subtilePos)
	{
		return null;
	}

	public virtual List<TileAndChange> GetAffectedTiles(TilePos subtilePos)
	{
		return null;
	}

	protected void GetSubtileBounds(SubtilePos pos, int radius, out int minX, out int minY, out int maxX, out int maxY)
	{
		int mapSubtileWidth = The.Map.mapSubtileWidth;
		int mapSubtileHeight = The.Map.mapSubtileHeight;
		minX = Math.Max(0, pos.X - radius);
		minY = Math.Max(0, pos.Y - radius);
		maxX = Math.Min(mapSubtileWidth - 1, pos.X + radius);
		maxY = Math.Min(mapSubtileHeight - 1, pos.Y + radius);
	}
}
