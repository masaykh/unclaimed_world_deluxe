using System;

namespace UWGame.SimSide.Maps;

public abstract class SurfaceType
{
	public enum TransportType : ulong
	{
		Foot,
		Car,
		OffRoad,
		Air
	}

	public enum TerrainFeatures : ulong
	{
		None,
		FootPath,
		WheelPath,
		GravelRoad,
		PavedRoad,
		Obstacle
	}

	protected byte[,] costs;

	public abstract string Name { get; }

	public virtual byte Cost(TransportType transport, TerrainFeatures feature)
	{
		if (transport == TransportType.Air)
		{
			return 1;
		}
		return costs[(uint)transport, (uint)feature];
	}

	public float MovementFactor(TransportType transport, TerrainFeatures feature)
	{
		return Convert.ToSingle(Cost(transport, feature) - 1) * 0.2f + 0.5f;
	}

	public float MovementFactor(byte cost)
	{
		return Convert.ToSingle(cost - 1) * 0.2f + 0.5f;
	}
}
