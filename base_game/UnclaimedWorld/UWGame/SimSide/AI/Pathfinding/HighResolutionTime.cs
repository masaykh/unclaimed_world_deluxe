using System.Diagnostics;

namespace UWGame.SimSide.AI.Pathfinding;

public class HighResolutionTime
{
	private long mStartCounter;

	public void Start()
	{
		mStartCounter = Stopwatch.GetTimestamp();
	}

	public double GetTime()
	{
		return (double)(Stopwatch.GetTimestamp() - mStartCounter) / Stopwatch.Frequency;
	}
}
