using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Pathfinding;

public class FloodFill
{
	public enum FloodFillResult
	{
		MaxRadiusReached,
		StayedWithinRadius
	}

	private Queue<Point> openQueue = new Queue<Point>();

	private sbyte[][] mDirection = new sbyte[8][]
	{
		new sbyte[2] { 0, -1 },
		new sbyte[2] { 1, 0 },
		new sbyte[2] { 0, 1 },
		new sbyte[2] { -1, 0 },
		new sbyte[2] { 1, -1 },
		new sbyte[2] { 1, 1 },
		new sbyte[2] { -1, 1 },
		new sbyte[2] { -1, -1 }
	};

	private const byte distance = 1;

	public FloodFillResult DoFloodFill(MapManager.SubtileValue[][] terrainGrid, ushort[][] allNodes, byte[][] allNodeDistances, Point from, ushort regionColor, int radius)
	{
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(allNodes);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(allNodes);
		openQueue.Clear();
		openQueue.Enqueue(from);
		allNodes[from.X][from.Y] = regionColor;
		while (openQueue.Count > 0)
		{
			Point point = openQueue.Dequeue();
			byte b = (byte)(allNodeDistances[point.X][point.Y] + 1);
			if (b >= radius)
			{
				return FloodFillResult.MaxRadiusReached;
			}
			for (int i = 0; i < 8; i++)
			{
				sbyte[] array = mDirection[i];
				ushort num = (ushort)(point.X + array[0]);
				ushort num2 = (ushort)(point.Y + array[1]);
				if (num < jaggedArrayWidth && num2 < jaggedArrayHeight && !MapManager.IsBlocked(terrainGrid[num][num2]) && allNodes[num][num2] <= 0)
				{
					openQueue.Enqueue(new Point(num, num2));
					allNodes[num][num2] = regionColor;
					allNodeDistances[num][num2] = b;
				}
			}
		}
		return FloodFillResult.StayedWithinRadius;
	}

	public FloodFillResult DoFloodFillOfArea(SubtileLayers terrainGrid, ushort[][] allNodes, Point fromSubtile, Rectangle areaSubtiles, byte[][] allNodeDistances = null, int? radius = null)
	{
		byte b = 0;
		Common.GetJaggedArrayWidth(allNodes);
		Common.GetJaggedArrayHeight(allNodes);
		ushort num = 1;
		openQueue.Clear();
		int num2 = Common.Clamp(fromSubtile.X, 0, The.Map.mapSubtileWidth);
		int num3 = Common.Clamp(fromSubtile.Y, 0, The.Map.mapSubtileHeight);
		openQueue.Enqueue(new Point(num2, num3));
		allNodes[num2 - areaSubtiles.X][num3 - areaSubtiles.Y] = num;
		while (openQueue.Count > 0)
		{
			Point point = openQueue.Dequeue();
			if (allNodeDistances != null)
			{
				b = (byte)(allNodeDistances[point.X - areaSubtiles.X][point.Y - areaSubtiles.Y] + 1);
				if (b >= radius)
				{
					return FloodFillResult.MaxRadiusReached;
				}
			}
			for (int i = 0; i < 8; i++)
			{
				sbyte[] array = mDirection[i];
				ushort num4 = (ushort)(point.X + array[0]);
				ushort num5 = (ushort)(point.Y + array[1]);
				if (num4 >= areaSubtiles.Right || num5 < areaSubtiles.Top || num4 < areaSubtiles.Left || num5 >= areaSubtiles.Bottom || MapManager.IsBlocked(terrainGrid.GetValue(num4, num5)))
				{
					continue;
				}
				int num6 = num4 - areaSubtiles.X;
				int num7 = num5 - areaSubtiles.Y;
				if (allNodes[num6][num7] <= 0)
				{
					openQueue.Enqueue(new Point(num4, num5));
					allNodes[num6][num7] = num;
					if (allNodeDistances != null)
					{
						allNodeDistances[num6][num7] = b;
					}
				}
			}
		}
		return FloodFillResult.StayedWithinRadius;
	}
}
