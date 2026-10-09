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

	/// <summary>
	/// PORT: the subtiles within <paramref name="radius"/> (world units) of <paramref name="pos"/>,
	/// each changed by <paramref name="strength"/> times its share: all of it out to
	/// <paramref name="hardCenterRadius"/>, then falling evenly to none at the radius. This is the
	/// "soft edge" of the Paintbrush's tooltip, which the studio had backwards - its share was
	/// (distance - hardCenterRadius) / softEdge, none at the centre and all of it at the rim, so a
	/// click raised a ring around the cursor and left the ground under it alone. Its loops also
	/// stopped one short of GetSubtileBounds' max, leaving the disc's right and bottom rows out.
	/// The Eraser shares it, with a negative strength (Kastuk, 2026-10-07: "Let Alpha slider to
	/// smooth out edges of Eraser tool (reduce height) too, just like for Paintbrush").
	/// </summary>
	protected List<SubTileAndChange> SoftDisc(SubtilePos pos, float radius, float hardCenterRadius, float strength)
	{
		List<SubTileAndChange> list = new List<SubTileAndChange>();
		GetSubtileBounds(pos, (int)(radius / 16f), out var minX, out var minY, out var maxX, out var maxY);
		float softEdge = radius - hardCenterRadius;
		for (int x = minX; x <= maxX; x++)
		{
			for (int y = minY; y <= maxY; y++)
			{
				SubtilePos subtilePos = new SubtilePos((ushort)x, (ushort)y);
				float distance = Common.Distance(MapManager.SubTileToWorldPos3(pos), MapManager.SubTileToWorldPos3(subtilePos));
				if (distance > radius)
				{
					continue;
				}
				float share = ((distance <= hardCenterRadius || softEdge <= 0f) ? 1f : Common.Clamp(1f - (distance - hardCenterRadius) / softEdge, 0f, 1f));
				if (share > 0f)
				{
					list.Add(new SubTileAndChange(subtilePos, strength * share));
				}
			}
		}
		return list;
	}
}
