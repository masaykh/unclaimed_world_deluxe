namespace UWGame.SimSide.Processes;

public class WorkOrTime
{
	public float? DaysNeeded;

	public float? TimeInSecondsNeeded;

	public bool MultiplyByBulk;

	public void Initialize()
	{
		if (TimeInSecondsNeeded.HasValue)
		{
			DaysNeeded = (float)((double?)TimeInSecondsNeeded / DateAndTime.secondsPerDay).Value;
		}
	}

	public bool ShouldSerializeManSecondsOfWorkNeeded()
	{
		return DaysNeeded.HasValue;
	}
}
