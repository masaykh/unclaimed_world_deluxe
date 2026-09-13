using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide;

public static class UniformPoissonDiskSampler
{
	private struct Settings
	{
		public Vector2 TopLeft;

		public Vector2 LowerRight;

		public Vector2 Center;

		public Vector2 Dimensions;

		public float? RejectionSqDistance;

		public float MinimumDistance;

		public float CellSize;

		public int GridWidth;

		public int GridHeight;
	}

	private struct State
	{
		public Vector2?[,] Grid;

		public List<Vector2> ActivePoints;

		public List<Vector2> Points;
	}

	public const int DefaultPointsPerIteration = 30;

	private static readonly float SquareRootTwo = (float)Math.Sqrt(2.0);

	public static List<Vector2> SampleCircle(Vector2 center, float radius, float minimumDistance)
	{
		return SampleCircle(center, radius, minimumDistance, 30);
	}

	public static List<Vector2> SampleCircle(Vector2 center, float radius, float minimumDistance, int pointsPerIteration)
	{
		return Sample(center - new Vector2(radius), center + new Vector2(radius), radius, minimumDistance, pointsPerIteration);
	}

	public static List<Vector2> SampleRectangle(Vector2 topLeft, Vector2 lowerRight, float minimumDistance)
	{
		return SampleRectangle(topLeft, lowerRight, minimumDistance, 30);
	}

	public static List<Vector2> SampleRectangle(Vector2 topLeft, Vector2 lowerRight, float minimumDistance, int pointsPerIteration)
	{
		return Sample(topLeft, lowerRight, null, minimumDistance, pointsPerIteration);
	}

	private static List<Vector2> Sample(Vector2 topLeft, Vector2 lowerRight, float? rejectionDistance, float minimumDistance, int pointsPerIteration)
	{
		Settings settings = new Settings
		{
			TopLeft = topLeft,
			LowerRight = lowerRight,
			Dimensions = lowerRight - topLeft,
			Center = (topLeft + lowerRight) / 2f,
			CellSize = minimumDistance / SquareRootTwo,
			MinimumDistance = minimumDistance,
			RejectionSqDistance = ((!rejectionDistance.HasValue) ? ((float?)null) : (rejectionDistance * rejectionDistance))
		};
		settings.GridWidth = (int)(settings.Dimensions.X / settings.CellSize) + 1;
		settings.GridHeight = (int)(settings.Dimensions.Y / settings.CellSize) + 1;
		State state = new State
		{
			Grid = new Vector2?[settings.GridWidth, settings.GridHeight],
			ActivePoints = new List<Vector2>(),
			Points = new List<Vector2>()
		};
		AddFirstPoint(ref settings, ref state);
		while (state.ActivePoints.Count != 0)
		{
			int index = The.Sim.GameplayRandomGenerator.Next(state.ActivePoints.Count, "PoissonDisc");
			Vector2 point = state.ActivePoints[index];
			bool flag = false;
			for (int i = 0; i < pointsPerIteration; i++)
			{
				flag |= AddNextPoint(point, ref settings, ref state);
			}
			if (!flag)
			{
				state.ActivePoints.RemoveAt(index);
			}
		}
		return state.Points;
	}

	private static void AddFirstPoint(ref Settings settings, ref State state)
	{
		bool flag = false;
		while (!flag)
		{
			double num = The.Sim.GameplayRandomGenerator.NextDouble("PoissionDisc");
			double num2 = (double)settings.TopLeft.X + (double)settings.Dimensions.X * num;
			num = The.Sim.GameplayRandomGenerator.NextDouble("PoissionDisc");
			double num3 = (double)settings.TopLeft.Y + (double)settings.Dimensions.Y * num;
			Vector2 vector = new Vector2((float)num2, (float)num3);
			if (!settings.RejectionSqDistance.HasValue || !(Vector2.DistanceSquared(settings.Center, vector) > settings.RejectionSqDistance))
			{
				flag = true;
				Vector2 vector2 = Denormalize(vector, settings.TopLeft, settings.CellSize);
				state.Grid[(int)vector2.X, (int)vector2.Y] = vector;
				state.ActivePoints.Add(vector);
				state.Points.Add(vector);
			}
		}
	}

	private static bool AddNextPoint(Vector2 point, ref Settings settings, ref State state)
	{
		bool result = false;
		Vector2 vector = GenerateRandomAround(point, settings.MinimumDistance);
		if (vector.X >= settings.TopLeft.X && vector.X < settings.LowerRight.X && vector.Y > settings.TopLeft.Y && vector.Y < settings.LowerRight.Y && (!settings.RejectionSqDistance.HasValue || Vector2.DistanceSquared(settings.Center, vector) <= settings.RejectionSqDistance))
		{
			Vector2 vector2 = Denormalize(vector, settings.TopLeft, settings.CellSize);
			bool flag = false;
			for (int i = (int)Math.Max(0f, vector2.X - 2f); (float)i < Math.Min(settings.GridWidth, vector2.X + 3f); i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = (int)Math.Max(0f, vector2.Y - 2f); (float)j < Math.Min(settings.GridHeight, vector2.Y + 3f); j++)
				{
					if (flag)
					{
						break;
					}
					if (state.Grid[i, j].HasValue && Vector2.Distance(state.Grid[i, j].Value, vector) < settings.MinimumDistance)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				result = true;
				state.ActivePoints.Add(vector);
				state.Points.Add(vector);
				state.Grid[(int)vector2.X, (int)vector2.Y] = vector;
			}
		}
		return result;
	}

	private static Vector2 GenerateRandomAround(Vector2 center, float minimumDistance)
	{
		double num = The.Sim.GameplayRandomGenerator.NextDouble("PoissionDisc");
		double num2 = (double)minimumDistance + (double)minimumDistance * num;
		num = The.Sim.GameplayRandomGenerator.NextDouble("PoissionDisc");
		double num3 = 6.2831854820251465 * num;
		double num4 = num2 * Math.Sin(num3);
		double num5 = num2 * Math.Cos(num3);
		return new Vector2((float)((double)center.X + num4), (float)((double)center.Y + num5));
	}

	private static Vector2 Denormalize(Vector2 point, Vector2 origin, double cellSize)
	{
		return new Vector2((int)((double)(point.X - origin.X) / cellSize), (int)((double)(point.Y - origin.Y) / cellSize));
	}
}
