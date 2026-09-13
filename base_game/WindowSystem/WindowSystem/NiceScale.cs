using System;

namespace WindowSystem;

public class NiceScale
{
	private double minPoint;

	private double maxPoint;

	private double maxTicks = 10.0;

	private double range;

	public double tickSpacing { get; private set; }

	public double niceMin { get; private set; }

	public double niceMax { get; private set; }

	public NiceScale(double min, double max)
	{
		minPoint = min;
		maxPoint = max;
		Calculate();
	}

	private void Calculate()
	{
		range = niceNum(maxPoint - minPoint, round: false);
		tickSpacing = niceNum(range / (maxTicks - 1.0), round: true);
		niceMin = Math.Floor(minPoint / tickSpacing) * tickSpacing;
		niceMax = Math.Ceiling(maxPoint / tickSpacing) * tickSpacing;
	}

	private double niceNum(double range, bool round)
	{
		double y = Math.Floor(Math.Log10(range));
		double num = range / Math.Pow(10.0, y);
		double num2 = (round ? ((num < 1.5) ? 1.0 : ((num < 3.0) ? 2.0 : ((!(num < 7.0)) ? 10.0 : 5.0))) : ((num <= 1.0) ? 1.0 : ((num <= 2.0) ? 2.0 : ((!(num <= 5.0)) ? 10.0 : 5.0))));
		return num2 * Math.Pow(10.0, y);
	}

	public void setMinMaxPoints(double minPoint, double maxPoint)
	{
		this.minPoint = minPoint;
		this.maxPoint = maxPoint;
		Calculate();
	}

	public void setMaxTicks(double maxTicks)
	{
		this.maxTicks = maxTicks;
		Calculate();
	}
}
