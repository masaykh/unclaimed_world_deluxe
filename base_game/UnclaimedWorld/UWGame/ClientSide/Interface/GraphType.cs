using System.Collections.Generic;

namespace UWGame.ClientSide.Interface;

internal class GraphType
{
	public string DisplayName;

	public string Tooltip;

	public Graphs Graph;

	public double? FixedYAxisMaxValue;

	public double? FixedYAxisMinValue;

	public float? YAxisTickSpacing;

	public string YAxisMaxLabel;

	public string YAxisMinLabel;

	public PlotAppearance SinglePlotAppearance;

	public Dictionary<string, PlotAppearance> MultiplePlotAppearances;
}
