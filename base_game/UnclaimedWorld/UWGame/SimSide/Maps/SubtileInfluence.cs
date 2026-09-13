using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Maps;

public class SubtileInfluence
{
	private byte[][] values;

	private int width;

	private int height;

	private int right;

	private int bottom;

	public Point TopLeftSubtilePositionOfMap;

	private List<SubtilePos> bestPositions = new List<SubtilePos>();

	private List<Tuple<byte, SubtilePos>> bestPositionsAndValues = new List<Tuple<byte, SubtilePos>>();

	public byte[][] Values => values;

	public SubtileInfluence(int width, int height)
	{
		Init(width, height);
	}

	private void Init(int width, int height)
	{
		Common.InitJaggedArray(ref values, width, height);
		this.width = width;
		this.height = height;
		right = width;
		bottom = height;
	}

	public SubtileInfluence(Vector3 targetLocation, int sizeOfMapInSubtiles)
	{
		TopLeftSubtilePositionOfMap = MapManager.WorldPosToSubtile(targetLocation);
		TopLeftSubtilePositionOfMap.X -= sizeOfMapInSubtiles / 2;
		TopLeftSubtilePositionOfMap.Y -= sizeOfMapInSubtiles / 2;
		MapManager.GetClampedRectangularMapAreaUsingSubtiles(TopLeftSubtilePositionOfMap, sizeOfMapInSubtiles, sizeOfMapInSubtiles, out var minX, out var maxX, out var minY, out var maxY);
		TopLeftSubtilePositionOfMap.X = minX;
		TopLeftSubtilePositionOfMap.Y = minY;
		int num = maxX - minX;
		int num2 = maxY - minY;
		Init(num, num2);
		right = TopLeftSubtilePositionOfMap.X + width;
		bottom = TopLeftSubtilePositionOfMap.Y + height;
	}

	public SubtileInfluence Clear()
	{
		byte[][] array = values;
		foreach (byte[] array2 in array)
		{
			Array.Clear(array2, 0, array2.Length);
		}
		return this;
	}

	public void DrawRadius(Vector3 targetLocation, float innerRadius, float outerRadius, byte valueToAdd)
	{
		Point p = MapManager.WorldPosToSubtile(targetLocation);
		for (int i = TopLeftSubtilePositionOfMap.X; i < right; i++)
		{
			for (int j = TopLeftSubtilePositionOfMap.Y; j < bottom; j++)
			{
				float num = 16f * Common.DistanceOctile(p, new Point(i, j));
				if (num >= innerRadius && num <= outerRadius)
				{
					values[i - TopLeftSubtilePositionOfMap.X][j - TopLeftSubtilePositionOfMap.Y] += valueToAdd;
				}
			}
		}
	}

	public void AddWhiteNoise(byte valueToAdd)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (values[i][j] != 0)
				{
					values[i][j] += (byte)The.Sim.GameplayRandomGenerator.Next(valueToAdd, null, saveMessage: false);
				}
			}
		}
	}

	public List<SubtilePos> GetBestRelativePositionsWithSameScore(bool randomize = true)
	{
		bestPositions.Clear();
		byte b = 0;
		for (int i = 0; i < width; i++)
		{
			byte[] array = values[i];
			for (int j = 0; j < height; j++)
			{
				byte b2 = array[j];
				if (b2 > 0)
				{
					if (b2 == b)
					{
						bestPositions.Add(new SubtilePos((ushort)i, (ushort)j));
					}
					else if (b2 > b)
					{
						bestPositions.Clear();
						b = b2;
						bestPositions.Add(new SubtilePos((ushort)i, (ushort)j));
					}
				}
			}
		}
		if (randomize)
		{
			bestPositions = Common.Randomize(bestPositions, The.Sim.GameplayRandomGenerator);
		}
		return bestPositions;
	}

	public List<Tuple<byte, SubtilePos>> GetBestRelativePositions(int maxResults, bool randomize = true)
	{
		bestPositionsAndValues.Clear();
		byte b = 0;
		for (int i = 0; i < width; i++)
		{
			byte[] array = values[i];
			for (int j = 0; j < height; j++)
			{
				byte b2 = array[j];
				if (b2 > 0 && b2 >= b)
				{
					bestPositionsAndValues.Add(new Tuple<byte, SubtilePos>(b2, new SubtilePos((ushort)i, (ushort)j)));
					b = b2;
				}
			}
		}
		if (bestPositionsAndValues.Count > maxResults)
		{
			bestPositionsAndValues = bestPositionsAndValues.OrderByDescending((Tuple<byte, SubtilePos> t) => t.Item1).ToList();
			bestPositionsAndValues.RemoveRange(maxResults, bestPositionsAndValues.Count - maxResults);
		}
		if (randomize)
		{
			bestPositionsAndValues = Common.Randomize(bestPositionsAndValues, The.Sim.GameplayRandomGenerator);
		}
		return bestPositionsAndValues;
	}

	public SubtilePos? GetBestPos()
	{
		byte b = 0;
		SubtilePos? result = null;
		for (int i = 0; i < width; i++)
		{
			byte[] array = values[i];
			for (int j = 0; j < height; j++)
			{
				byte b2 = array[j];
				if (b2 > b)
				{
					b = b2;
					result = new SubtilePos((ushort)i, (ushort)j);
				}
			}
		}
		return result;
	}

	private static void GetMinMaxDistances(ref Point fromLocation, ref Point toLocation, ref float minDistance, ref float maxDistance)
	{
		float val = Common.DistanceOctile(fromLocation, toLocation);
		minDistance = Math.Min(minDistance, val);
		maxDistance = Math.Max(maxDistance, val);
	}

	private static void GetMinMaxDistances(ref Vector2 fromLocation, ref Vector2 toLocation, ref float minDistance, ref float maxDistance)
	{
		float val = Common.DistanceOctile(fromLocation, toLocation);
		minDistance = Math.Min(minDistance, val);
		maxDistance = Math.Max(maxDistance, val);
	}

	public static SubtileInfluence FindFreeSpotNearLocation(Vector3 targetLocation, int sizeOfMapInSubtiles, Vector3? fromLocation, Entity entity, bool useMovementMap)
	{
		SubtileInfluence subtileInfluence = new SubtileInfluence(targetLocation, sizeOfMapInSubtiles);
		if (fromLocation.HasValue)
		{
			subtileInfluence.DrawDistanceGradientOnInfluenceMap(fromLocation.Value);
		}
		else
		{
			subtileInfluence.FillArray(10);
		}
		subtileInfluence.DrawNegativeInfluenceFromEntities(entity, drawStationaryAgents: true, drawItems: true);
		if (useMovementMap)
		{
			MovementMap movementMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, entity.Intelligence.ThreatStance);
			subtileInfluence.BlockOutBlockedSubtiles(movementMap.Layers[SurfaceType.TransportType.Foot], blockReserved: false);
		}
		return subtileInfluence;
	}

	public void BlockOutBlockedSubtiles(SubtileLayers moveMap, bool blockReserved)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				MapManager.SubtileValue value = moveMap.GetValue(i + TopLeftSubtilePositionOfMap.X, j + TopLeftSubtilePositionOfMap.Y);
				if (MapManager.IsBlocked(value) || (blockReserved && MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved)))
				{
					values[i][j] = 0;
				}
			}
		}
	}

	public void DrawNegativeInfluenceFromEntities(Entity entityToExclude, bool drawStationaryAgents = true, bool drawItems = false)
	{
		Point point = MapManager.SubTileToTilePos(TopLeftSubtilePositionOfMap);
		Point point2 = MapManager.SubTileToTilePos(new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y + height));
		Vector3 relativeTo = MapManager.SubTileEdgeToWorldPos3(TopLeftSubtilePositionOfMap);
		int paramValue = 1;
		for (int i = point.X; i <= point2.X; i++)
		{
			for (int j = point.Y; j <= point2.Y; j++)
			{
				TerrainTile terrainTile = The.Map.TileMap[i][j];
				if (terrainTile.EntitiesOnTile == null)
				{
					continue;
				}
				foreach (Entity item in terrainTile.EntitiesOnTile)
				{
					if (item == entityToExclude)
					{
						continue;
					}
					bool flag = item.EntityType.ItemType != null;
					bool flag2 = item.EntityType.IntelligenceType != null && item.Locomotor != null && !item.Locomotor.IsMoving();
					if (!(drawStationaryAgents && flag2) && !(drawItems && flag))
					{
						continue;
					}
					Point pos = MapManager.WorldPosToRelativeSubtile(item.Location.Value, relativeTo);
					if (pos.X >= 0 && pos.Y >= 0)
					{
						if (flag2)
						{
							paramValue = 2;
						}
						if (flag)
						{
							paramValue = 0;
						}
						InfluenceMap.DrawLinearInfluenceCircle(values, pos, -5, InfluenceMap.Operation.AddToExisting, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, paramValue);
					}
				}
			}
		}
	}

	public void FillArray(byte value)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				values[i][j] = value;
			}
		}
	}

	public void DrawDistanceGradientOnInfluenceMap(Vector3 fromLocation, float minValueToDraw = 0f, float maxValueToDraw = 10f)
	{
		Point fromLocation2 = MapManager.WorldPosToSubtile(fromLocation);
		Point toLocation = TopLeftSubtilePositionOfMap;
		Point toLocation2 = new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y);
		Point toLocation3 = new Point(TopLeftSubtilePositionOfMap.X, TopLeftSubtilePositionOfMap.Y + height);
		Point toLocation4 = new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y + height);
		Point toLocation5 = new Point(TopLeftSubtilePositionOfMap.X + width / 2, TopLeftSubtilePositionOfMap.Y + height / 2);
		float minDistance = 1000000f;
		float maxDistance = 0f;
		GetMinMaxDistances(ref fromLocation2, ref toLocation, ref minDistance, ref maxDistance);
		GetMinMaxDistances(ref fromLocation2, ref toLocation2, ref minDistance, ref maxDistance);
		GetMinMaxDistances(ref fromLocation2, ref toLocation3, ref minDistance, ref maxDistance);
		GetMinMaxDistances(ref fromLocation2, ref toLocation4, ref minDistance, ref maxDistance);
		GetMinMaxDistances(ref fromLocation2, ref toLocation5, ref minDistance, ref maxDistance);
		float num = 1f / (maxDistance - minDistance);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				float num2 = Common.DistanceOctile(fromLocation2, new Point(TopLeftSubtilePositionOfMap.X + i, TopLeftSubtilePositionOfMap.Y + j));
				float num3 = MathHelper.Lerp(maxValueToDraw, minValueToDraw, (num2 - minDistance) * num);
				values[i][j] = (byte)num3;
			}
		}
	}

	public void DrawNegativeInfluenceFromEntities(List<Tuple<Vector3, float>> circlesToDraw, Point topLeftSubtilePosition)
	{
		Vector3 relativeTo = MapManager.SubTileEdgeToWorldPos3(topLeftSubtilePosition);
		foreach (Tuple<Vector3, float> item in circlesToDraw)
		{
			Point pos = MapManager.WorldPosToRelativeSubtile(item.Item1, relativeTo);
			if (pos.X >= 0 && pos.Y >= 0)
			{
				InfluenceMap.DrawLinearInfluenceCircle(values, pos, 0, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, (int)item.Item2);
			}
		}
	}
}
