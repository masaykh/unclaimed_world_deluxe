namespace UWGame.SimSide.Systems;

public class UpdateTimePoints
{
	public static double ComputeTimePointFromInterval(double interval)
	{
		return The.Sim.TotalUnPausedGameTimeInSeconds + interval;
	}

	public static void GetSoonestInterval(double? tempInterval, ref double? currentInterval)
	{
		if (tempInterval.HasValue)
		{
			if (!currentInterval.HasValue)
			{
				currentInterval = tempInterval;
			}
			else if (tempInterval.Value < currentInterval.Value)
			{
				currentInterval = tempInterval;
			}
		}
	}

	public static double? ComputeIntervalFromTimepoint(double? expiryTimePointInSeconds)
	{
		double? result = null;
		if (expiryTimePointInSeconds.HasValue)
		{
			result = Common.ClampBottom(expiryTimePointInSeconds.Value - The.Sim.TotalUnPausedGameTimeInSeconds, 0.01);
		}
		return result;
	}
}
