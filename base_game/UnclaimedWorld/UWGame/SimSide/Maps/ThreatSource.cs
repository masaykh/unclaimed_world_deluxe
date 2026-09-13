using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Maps;

public class ThreatSource
{
	public Point MapPosition;

	public Rectangle ThreatArea;

	public int Radius;

	public float LifeTime;

	private float elapsedTime;

	public byte[,] ThreatMapping;

	public byte MaxValue = byte.MaxValue;

	public float PropagationConstant = 0.8f;

	public bool IsTransient;

	public Entity EntitySource;

	public ThreatSource(Point pos, int radius, float lifetime)
	{
		MapPosition = pos;
		Radius = radius;
		LifeTime = lifetime;
		ThreatArea = MapManager.GetClampedMapAreaUsingTiles(new TilePos(pos.X, pos.Y), radius, out var minX, out var maxX, out var minY, out var maxY);
		ThreatMapping = new byte[maxX - minX, maxY - minY];
		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				int num = Math.Abs(i - MapPosition.X) + Math.Abs(j - MapPosition.Y);
				double d = (double)(int)MaxValue * Math.Pow(PropagationConstant, num);
				ThreatMapping[i - minX, j - minY] = (byte)Common.ClampTop(d, 255.0);
			}
		}
	}

	public ThreatSource()
	{
	}

	public void Update(GameTime elapsed)
	{
		elapsedTime += (float)elapsed.ElapsedGameTime.TotalSeconds;
	}

	public float Decay()
	{
		return Common.ClampBottom((LifeTime - elapsedTime) / LifeTime, 0f);
	}

	public bool IsExpired()
	{
		return elapsedTime > LifeTime;
	}
}
