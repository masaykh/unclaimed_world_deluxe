using System;
using UWGame.Control.Replays;

namespace UWGame;

public class NormalDistribution
{
	public double? Mean;

	public double? StandardDeviation;

	public float? Max;

	public float? Min;

	public double GetRandomValue(RandomGenerator generator, bool clampBetweenZeroAndOne = false)
	{
		double num;
		if (Mean.HasValue)
		{
			double stdDev = StandardDeviation ?? 0.0;
			num = GetRandomValue(generator, Mean.Value, stdDev);
		}
		else
		{
			if (!Min.HasValue || !Max.HasValue)
			{
				return 0.0;
			}
			Common.GetNormalDistributionFromMinMaxValues(Min.Value, Max.Value, out var mean, out var stdDev2);
			num = GetRandomValue(generator, mean.Value, stdDev2.Value);
		}
		if (Max.HasValue)
		{
			num = Common.ClampTop(num, Max.Value);
		}
		if (Min.HasValue)
		{
			num = Common.ClampBottom(num, Min.Value);
		}
		if (clampBetweenZeroAndOne)
		{
			num = Common.Clamp(num, 0.0, 1.0);
		}
		return num;
	}

	public double GetMean()
	{
		if (!Mean.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(Min.Value, Max.Value, out var mean, out var _);
			Mean = mean;
		}
		return Mean.Value;
	}

	public float GetMin()
	{
		if (!Min.HasValue)
		{
			Min = (float)(Mean.Value - 3.0 * StandardDeviation.Value);
		}
		return Min.Value;
	}

	public float GetMax()
	{
		if (!Max.HasValue)
		{
			Max = (float)(Mean.Value + 3.0 * StandardDeviation.Value);
		}
		return Max.Value;
	}

	public int GetRandomIntegerValue(RandomGenerator generator)
	{
		return (int)Math.Round(GetRandomValue(generator));
	}

	public static double GetRandomValue(RandomGenerator generator, double mean, double stdDev)
	{
		bool saveMessage = generator.Type == RandomGenerator.GeneratorType.Sim;
		double d = generator.NextDouble("Common - RandomNormalDistribution", saveMessage);
		double num = generator.NextDouble("Common - RandomNormalDistribution", saveMessage);
		return mean + stdDev * (Math.Sqrt(-2.0 * Math.Log(d)) * Math.Cos(6.28 * num));
	}
}
