using System.Runtime.InteropServices;

namespace UWGame.SimSide.AI.Pathfinding;

public class HighResolutionTime
{
	private long mStartCounter;

	private long mFrequency;

	[DllImport("Kernel32.dll")]
	private static extern bool QueryPerformanceCounter(out long perfcount);

	[DllImport("Kernel32.dll")]
	private static extern bool QueryPerformanceFrequency(out long freq);

	public HighResolutionTime()
	{
		QueryPerformanceFrequency(out mFrequency);
	}

	public void Start()
	{
		QueryPerformanceCounter(out mStartCounter);
	}

	public double GetTime()
	{
		QueryPerformanceCounter(out var perfcount);
		return (double)(perfcount - mStartCounter) / (double)mFrequency;
	}
}
