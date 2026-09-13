using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class GraphPanel : RosterPanel
{
	public enum Ranges
	{
		OneDay,
		OneSeason,
		OneYear,
		TenYears
	}

	private Grid surfaceGrid;

	private LCDInnerPanel graphPanel;

	private Graph graph;

	private ComboBox cbSource;

	private ComboBox cbRange;

	private TextArea taHelp;

	private List<GroupStatistics> statisticsToShow = new List<GroupStatistics>();

	private int canvasWidth = 544;

	private int canvasHeight = 500;

	private List<GraphType> AllGraphTypes;

	private Color[] plotColors = new Color[7]
	{
		Color.AliceBlue,
		Color.Gray,
		Color.Brown,
		Color.Cornsilk,
		Color.Crimson,
		Color.LightGreen,
		Color.MediumVioletRed
	};

	private const int windowWidth = 600;

	private Dictionary<ulong, Plot> plots = new Dictionary<ulong, Plot>();

	public GraphPanel()
		: base("GRAPHS", 600, The.InGameUI.rosterPanelHeight, needBottomMarginForButton: false)
	{
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: true, titleBottom + 6);
		AllGraphTypes = new List<GraphType>
		{
			new GraphType
			{
				Graph = Graphs.StarvingPercentage,
				DisplayName = "% Undernourished",
				Tooltip = "Shows the % of colony members that are lacking a type of nutrient",
				FixedYAxisMaxValue = 100.0,
				FixedYAxisMinValue = 0.0,
				YAxisTickSpacing = 25f,
				YAxisMaxLabel = "100 %",
				YAxisMinLabel = "0 %",
				MultiplePlotAppearances = new Dictionary<string, PlotAppearance>
				{
					{
						"foodEnergy",
						new PlotAppearance
						{
							Color = "#de9239".ColorFromHex(),
							LineRadius = 1f,
							DrawOrder = 3
						}
					},
					{
						"protein",
						new PlotAppearance
						{
							Color = "#c22807".ColorFromHex(),
							LineRadius = 2f,
							DrawOrder = 2
						}
					},
					{
						"micronutrients",
						new PlotAppearance
						{
							Color = "#348252".ColorFromHex(),
							LineRadius = 3f,
							DrawOrder = 1
						}
					},
					{
						"stimulants",
						new PlotAppearance
						{
							Color = "#1f5670".ColorFromHex(),
							LineRadius = 3.5f,
							DrawOrder = 0
						}
					}
				}
			},
			new GraphType
			{
				Graph = Graphs.FoodRating,
				DisplayName = "Food Conditions",
				Tooltip = "Rating of the amount of prepared food items in stock and the intake of food and nutrients among the members. Starvation will have a big negative influence on the rating.",
				FixedYAxisMaxValue = 1.0,
				FixedYAxisMinValue = 0.0,
				YAxisTickSpacing = 0.25f,
				YAxisMaxLabel = "100 %",
				YAxisMinLabel = "0 %",
				SinglePlotAppearance = new PlotAppearance
				{
					Color = "#3c9677".ColorFromHex(),
					LineRadius = 3f
				}
			},
			new GraphType
			{
				Graph = Graphs.SecurityRating,
				DisplayName = "Security Conditions",
				Tooltip = "Rating of the number and quality of hand weapons available (in proportion to the colony size), the number and quality of defenders, and the occurrance of attacks by wildlife.",
				FixedYAxisMaxValue = 1.0,
				FixedYAxisMinValue = 0.0,
				YAxisTickSpacing = 0.25f,
				YAxisMaxLabel = "100 %",
				YAxisMinLabel = "0 %",
				SinglePlotAppearance = new PlotAppearance
				{
					Color = "#5192B5".ColorFromHex(),
					LineRadius = 2f
				}
			},
			new GraphType
			{
				Graph = Graphs.ComfortRating,
				DisplayName = "Comfort Conditions",
				Tooltip = "The colony's comfort conditions are a rating of its housing quality and how well it satisfies its inhabitants' need for stimulants (alcohol, coffee etc.)",
				FixedYAxisMaxValue = 1.0,
				FixedYAxisMinValue = 0.0,
				YAxisTickSpacing = 0.25f,
				YAxisMaxLabel = "100 %",
				YAxisMinLabel = "0 %",
				SinglePlotAppearance = new PlotAppearance
				{
					Color = "#3c9677".ColorFromHex(),
					LineRadius = 2f
				}
			},
			new GraphType
			{
				Graph = Graphs.Population,
				DisplayName = "Population",
				Tooltip = "Shows the size of the colony over time",
				SinglePlotAppearance = new PlotAppearance
				{
					Color = "#72CDFF".ColorFromHex(),
					LineRadius = 2f
				}
			}
		};
		UIComponent uIComponent = new UIComponent(Interface.gui);
		int yPosToCenterTo = 12;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDHeadingBlue);
		label.Text = "DATA:";
		label.Position = new Point(0, 0);
		label.FitToText();
		label.CenterThisVertically(yPosToCenterTo);
		label.Y++;
		cbSource = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		uIComponent.Add(cbSource);
		cbSource.Init(ComboBoxTypes.LCD);
		cbSource.X = label.Right + 6;
		cbSource.Width = 180;
		cbSource.CenterThisVertically(yPosToCenterTo);
		cbSource.SelectionChanged += cbSource_SelectionChanged;
		Label label2 = new Label(Interface.gui);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDHeadingRed);
		label2.Text = "RANGE:";
		label2.Position = new Point(280, 0);
		label2.FitToText();
		label2.CenterThisVertically(yPosToCenterTo);
		label2.Y++;
		cbRange = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		uIComponent.Add(cbRange);
		cbRange.Init(ComboBoxTypes.LCD);
		cbRange.X = label2.Right + 6;
		cbRange.Width = 105;
		cbRange.CenterThisVertically(yPosToCenterTo);
		taHelp = new TextArea(Interface.gui, ListBoxType.LCD);
		taHelp.RenderType = RenderType.CRTAndLCD;
		taHelp.Init(Label.LabelType.LCDNormal);
		taHelp.CanGrowInHeight = true;
		uIComponent.Add(taHelp);
		taHelp.Y = label.Bottom + 2;
		taHelp.Width = lcdSurface.Width;
		graphPanel = new LCDInnerPanel(Interface.gui, canvasWidth, includeDecor: true);
		uIComponent.Add(graphPanel.Panel);
		graphPanel.ContentHeight = canvasHeight;
		graphPanel.Panel.Y = 66;
		graph = new Graph(Interface.gui, canvasWidth, canvasHeight);
		graphPanel.Panel.Add(graph);
		graph.LegendClicked += graph_LegendClicked;
		uIComponent.Height = 575;
		surfaceGrid.AddEntry(uIComponent, uIComponent);
		PopulateSourceCombo();
		PopulateRangesCombo();
	}

	protected override void Destroy()
	{
		graph.Destroy();
	}

	private void graph_LegendClicked()
	{
		plots.Clear();
		Refresh();
	}

	private void cbSource_SelectionChanged(UIComponent sender)
	{
		plots.Clear();
		Refresh();
	}

	public void SelectGraphType(Graphs graph)
	{
		GraphType selectedKey = AllGraphTypes.FirstOrDefault((GraphType g) => g.Graph == graph);
		cbSource.SelectedKey = selectedKey;
	}

	private void PopulateSourceCombo()
	{
		foreach (GraphType allGraphType in AllGraphTypes)
		{
			cbSource.AddEntry(allGraphType, allGraphType.DisplayName);
		}
		cbSource.SelectionChanged -= cbSource_SelectionChanged;
		cbSource.SelectedIndex = 0;
		cbSource.SelectionChanged += cbSource_SelectionChanged;
	}

	private void PopulateRangesCombo()
	{
		cbRange.AddEntry(Ranges.OneDay, "One day");
		cbRange.AddEntry(Ranges.OneSeason, "One season");
		cbRange.AddEntry(Ranges.OneYear, "One year");
		cbRange.AddEntry(Ranges.TenYears, "Ten years");
		cbRange.SelectionChanged -= cbRange_SelectionChanged;
		cbRange.SelectedIndex = 0;
		cbRange.SelectionChanged += cbRange_SelectionChanged;
	}

	private void cbRange_SelectionChanged(UIComponent sender)
	{
		Refresh();
	}

	public static DateAndTime.TimeDateYear GetStartPoint(Ranges range, out string fromLabelText)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		fromLabelText = "";
		switch (range)
		{
		case Ranges.OneDay:
			fromLabelText = "One day ago";
			currentTimeDateYear.AddTime(-1.0);
			break;
		case Ranges.OneSeason:
			fromLabelText = "One season ago";
			currentTimeDateYear.AddTime(-3.0);
			break;
		case Ranges.OneYear:
			fromLabelText = "One year ago";
			currentTimeDateYear.AddTime(-12.0);
			break;
		case Ranges.TenYears:
			fromLabelText = "Ten years ago";
			currentTimeDateYear.AddTime(-120.0);
			break;
		}
		return currentTimeDateYear;
	}

	public override void Refresh()
	{
		base.Refresh();
		foreach (GroupStatistics item in statisticsToShow)
		{
			GetPlots(item);
		}
		if (plots.Count > 0)
		{
			GraphType graphType = plots.First().Value.GraphType;
			taHelp.Text = graphType.Tooltip;
			string fromLabelText;
			DateAndTime.TimeDateYear startPoint = GetStartPoint((Ranges)cbRange.SelectedKey, out fromLabelText);
			float minX = (float)startPoint.TotalDays;
			IOrderedEnumerable<KeyValuePair<ulong, Plot>> orderedEnumerable = plots.OrderBy((KeyValuePair<ulong, Plot> p) => p.Value.DrawOrder);
			graph.BeginAddingLegends();
			foreach (KeyValuePair<ulong, Plot> item2 in orderedEnumerable)
			{
				graph.AddLegend(item2.Value.Name, item2.Value.Color, item2.Key);
				if (graph.IsChecked(item2.Key))
				{
					item2.Value.ComputePlotRanges(startPoint);
				}
			}
			graph.EndAddingLegends();
			if (!graphType.FixedYAxisMaxValue.HasValue)
			{
				double num = plots.Max((KeyValuePair<ulong, Plot> p) => graph.IsChecked(p.Key) ? p.Value.MaxValue : 0.0);
				if (Common.IsZero(num))
				{
					num = 100.0;
				}
				graph.SetRanges(minX, (float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays, 0f, (float)num, fromLabelText, "Now");
			}
			else
			{
				double num = graphType.FixedYAxisMaxValue.Value;
				graph.SetFixedRanges(minX, (float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays, 0f, (float)num, graphType.YAxisTickSpacing.Value, fromLabelText, "Now", graphType.YAxisMinLabel, graphType.YAxisMaxLabel);
			}
			graph.BeginDraw();
			graph.DrawAxis();
			foreach (KeyValuePair<ulong, Plot> item3 in orderedEnumerable)
			{
				if (graph.IsChecked(item3.Key))
				{
					DrawPlot(item3.Value);
				}
			}
			graph.DrawLegends();
			graph.EndDraw();
		}
		else
		{
			graph.Clear();
		}
	}

	public override void Show()
	{
		statisticsToShow.Clear();
		statisticsToShow.Add(The.InGameUI.UIAllegiance.Statistics);
		base.Show();
	}

	private void GetPlots(GroupStatistics statistics)
	{
		GraphType graphType = cbSource.SelectedKey as GraphType;
		float? lineRadius = null;
		Color color;
		if (graphType.SinglePlotAppearance != null)
		{
			color = graphType.SinglePlotAppearance.Color;
			lineRadius = graphType.SinglePlotAppearance.LineRadius;
		}
		else
		{
			color = Common.GetRandomListMember(plotColors, The.Client.ClientRandomGenerator);
		}
		switch (graphType.Graph)
		{
		case Graphs.Population:
		{
			List<DataPoint<float>> population = statistics.PopulationStatistics.Population;
			UpdatePlot(statistics, graphType, color, lineRadius, population, extendLastValue: true);
			break;
		}
		case Graphs.FoodRating:
		{
			List<DataPoint<float>> ratings3 = ((FoodStatistics)statistics.Ratings[RatingTypes.Food]).Ratings;
			UpdatePlot(statistics, graphType, color, lineRadius, ratings3);
			break;
		}
		case Graphs.SecurityRating:
		{
			List<DataPoint<float>> ratings2 = ((SecurityStatisticsForAllegiance)statistics.Ratings[RatingTypes.Security]).Ratings;
			UpdatePlot(statistics, graphType, color, lineRadius, ratings2);
			break;
		}
		case Graphs.ComfortRating:
		{
			List<DataPoint<float>> ratings = ((ComfortStatistics)statistics.Ratings[RatingTypes.Comfort]).Ratings;
			UpdatePlot(statistics, graphType, color, lineRadius, ratings);
			break;
		}
		case Graphs.StarvingPercentage:
		{
			FoodStatistics obj = (FoodStatistics)statistics.Ratings[RatingTypes.Food];
			int count = obj.StarvingMemberPercentage.Count;
			float num = 1f / (float)count;
			float num2 = num;
			{
				foreach (KeyValuePair<NeedTypeID, List<DataPoint<float>>> item in obj.StarvingMemberPercentage)
				{
					ulong key = (ulong)(item.Key + 1000L * (long)statistics.CanIterateEntitiesID);
					if (!plots.TryGetValue(key, out var value))
					{
						NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
						float? lineRadius2 = null;
						int drawOrder = 0;
						Color color2;
						if (graphType.MultiplePlotAppearances != null && graphType.MultiplePlotAppearances.TryGetValue(needType.KeyName, out var value2))
						{
							color2 = value2.Color;
							lineRadius2 = value2.LineRadius;
							drawOrder = value2.DrawOrder;
						}
						else
						{
							color2 = new Color(num2 * color.ToVector3());
							num2 += num;
						}
						value = new Plot
						{
							Name = needType.ToString(),
							Color = color2,
							LineRadius = lineRadius2,
							Data = item.Value,
							GraphType = graphType,
							DrawOrder = drawOrder
						};
						plots.Add(key, value);
					}
				}
				break;
			}
		}
		case Graphs.Test:
			break;
		}
	}

	private void UpdatePlot(GroupStatistics statistics, GraphType graphType, Color color, float? lineRadius, List<DataPoint<float>> data, bool extendLastValue = false)
	{
		ulong key = (ulong)graphType.GetHashCode() + (ulong)statistics.CanIterateEntitiesID;
		if (!plots.TryGetValue(key, out var value))
		{
			value = new Plot
			{
				Color = color,
				LineRadius = lineRadius,
				Data = data,
				ExtendLastValue = extendLastValue,
				GraphType = graphType,
				Name = graphType.DisplayName
			};
			plots.Add(key, value);
		}
	}

	private void DrawPlot(Plot plot)
	{
		List<DataPoint<float>> data = plot.Data;
		if (plot.MinIndex >= 0 && plot.MaxIndex >= 0)
		{
			Color color = plot.Color;
			float lineRadius = plot.LineRadius ?? 1f;
			graph.BeginConnectedPlot(color, lineRadius);
			for (int i = plot.MinIndex; i <= plot.MaxIndex; i++)
			{
				DataPoint<float> dataPoint = data[i];
				graph.AddDataPoint((float)dataPoint.Time.TotalDays, dataPoint.Value);
			}
			if (plot.ExtendLastValue)
			{
				graph.AddDataPoint((float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays, data[plot.MaxIndex].Value);
			}
			graph.EndPlot();
		}
	}
}
