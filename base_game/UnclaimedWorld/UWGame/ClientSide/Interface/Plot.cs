using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.ClientSide.Interface;

internal class Plot
{
	public Color Color;

	public float? LineRadius;

	public string Name;

	public List<DataPoint<float>> Data;

	public int MinIndex;

	public int MaxIndex;

	public double MaxValue;

	public bool ExtendLastValue;

	public GraphType GraphType;

	public int DrawOrder;

	public void ComputePlotRanges(DateAndTime.TimeDateYear fromDate)
	{
		_ = fromDate.TotalDays;
		if (!Statistic.GetDataPointsBetween(Data, fromDate, The.Sim.DateAndTime.CurrentTimeDateYear, out MinIndex, out MaxIndex, ExtendLastValue))
		{
			return;
		}
		MaxValue = 0.0;
		for (int i = MinIndex; i <= MaxIndex; i++)
		{
			DataPoint<float> dataPoint = Data[i];
			if ((double)dataPoint.Value > MaxValue)
			{
				MaxValue = dataPoint.Value;
			}
		}
	}
}
