using System.Xml.Serialization;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents;

public class TimePoint
{
	public DateAndTime.TimeDateYear? Date;

	public double? RelativeNoOfDays;

	public EvalNode DynamicRelativeNoOfDays;

	public EvalNode RelativeTimeInSeconds;

	public EvalNode AbsoluteDate;

	[XmlIgnore]
	private double? timeInSeconds;

	[XmlIgnore]
	public double TimeInSeconds
	{
		get
		{
			if (!timeInSeconds.HasValue)
			{
				timeInSeconds = GetTimepointInSecondsOfElapsedGameTime();
			}
			return timeInSeconds.Value;
		}
	}

	public double? GetUpdateInterval()
	{
		return UpdateTimePoints.ComputeIntervalFromTimepoint(TimeInSeconds).Value;
	}

	private double? GetTimepointInSecondsOfElapsedGameTime()
	{
		if (AbsoluteDate != null)
		{
			PropertyResult? propertyResult = AbsoluteDate.Evaluate(null, null, null, null);
			if (propertyResult.HasValue && propertyResult.Value.DateResult.HasValue)
			{
				DateAndTime.TimeDateYear startTimeDateYear = The.Sim.DateAndTime.StartTimeDateYear;
				return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(propertyResult.Value.DateResult.Value) - The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(startTimeDateYear);
			}
		}
		else if (DynamicRelativeNoOfDays != null)
		{
			PropertyResult? propertyResult2 = DynamicRelativeNoOfDays.Evaluate(null, null, null, null);
			if (propertyResult2.HasValue && propertyResult2.Value.NumberResult.HasValue)
			{
				double totalDays = propertyResult2.Value.NumberResult.Value;
				DateAndTime.TimeDateYear date = new DateAndTime.TimeDateYear(totalDays);
				return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(date);
			}
		}
		else
		{
			if (RelativeNoOfDays.HasValue)
			{
				DateAndTime.TimeDateYear date2 = new DateAndTime.TimeDateYear(RelativeNoOfDays.Value);
				return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(date2);
			}
			if (RelativeTimeInSeconds == null)
			{
				DateAndTime.TimeDateYear startTimeDateYear2 = The.Sim.DateAndTime.StartTimeDateYear;
				DateAndTime.TimeDateYear? timeDateYear = null;
				if (Date.HasValue)
				{
					timeDateYear = Date.Value;
				}
				if (timeDateYear.HasValue)
				{
					return The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(timeDateYear.Value) - The.Sim.DateAndTime.ConvertDateToRealTimeSeconds(startTimeDateYear2);
				}
				return null;
			}
			PropertyResult? propertyResult3 = RelativeTimeInSeconds.Evaluate(null, null, null, null);
			if (propertyResult3.HasValue)
			{
				return propertyResult3.Value.NumberResult;
			}
		}
		return null;
	}
}
