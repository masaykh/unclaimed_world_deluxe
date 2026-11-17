using System.Diagnostics;

namespace UWGame.SimSide.AI.Pathfinding;

// PORT FIX. The studio read QueryPerformanceCounter and QueryPerformanceFrequency from Kernel32.dll
// through P/Invoke, which does not exist off Windows: NEW GAME on Linux died with
// DllNotFoundException 'Kernel32.dll' in LoadingScreen.LoadDataInThread, the first time
// EventManager, CycleManager or RegionMap built one (GitHub issue #1). Stopwatch.GetTimestamp IS
// QueryPerformanceCounter on Windows, and Stopwatch.Frequency its frequency, so nothing changes
// there; elsewhere they are the platform's own monotonic clock.
public class HighResolutionTime
{
	private long mStartCounter;

	private readonly long mFrequency = Stopwatch.Frequency;

	public void Start()
	{
		mStartCounter = Stopwatch.GetTimestamp();
	}

	public double GetTime()
	{
		return (double)(Stopwatch.GetTimestamp() - mStartCounter) / (double)mFrequency;
	}
}
