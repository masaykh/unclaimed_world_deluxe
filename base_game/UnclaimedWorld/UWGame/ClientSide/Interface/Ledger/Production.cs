using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class Production : LedgerSheet
{
	private const int columnWidth = 90;

	private const int nameWidth = 215;

	private const int itemTypeIconColumnX = 12;

	private const int nameX = 30;

	private const int productionX = 245;

	private const int consumedX = 335;

	private int degradedX = 425;

	private int disappearedX = 515;

	private int productivityX = 605;

	private int bottomPartHeight = 40;

	private string wastedTooltip;

	private string nameTooltip;

	private string producedTooltip;

	private string usedTooltip;

	private string disappearedTooltip;

	private string productivityTooltip;

	private Grid outerGrid;

	public const int ItemHeight = 26;

	private SortingButtons<ProductionSettings.SortColumns> sortingButtons;

	private List<Grid> categoryGrids = new List<Grid>();

	private List<EntityCategory> categoriesToShow;

	private UIComponent bottomContainer;

	private Label lblTotalProduced;

	private Label lblTotalConsumed;

	private Label lblTotalDegraded;

	private Label lblTotalCritterEaten;

	private Label lblTotalDisappeared;

	private int gridYPos;

	public override string DisplayName => "Production";

	public override string Tooltip => "Shows production details including productivity.";

	public override bool ShowRangeSelector => true;

	private int GridMaxHeight => Height - gridYPos - bottomPartHeight;

	public Production(GUIManager gui, int width, int height)
		: base(gui, width, height)
	{
		CreateTooltips();
		CreateGridHeaderButtons();
		gridYPos = sortingButtons.Bottom + 6;
		outerGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		Add(outerGrid);
		outerGrid.FixedItemHeights = false;
		outerGrid.RenderType = RenderType.CRTAndLCD;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Width = Width;
		outerGrid.Height = Height - gridYPos - bottomPartHeight;
		outerGrid.Position = new Point(0, gridYPos);
		outerGrid.RowSpacing = 1;
		outerGrid.CanGrowInHeight = false;
		outerGrid.ScrollBarEnabled = true;
		outerGrid.SurfaceHeightResize += outerGrid_HeightResize;
		bottomContainer = new UIComponent(gui);
		bottomContainer.Height = bottomPartHeight;
		bottomContainer.Width = Width;
		Add(bottomContainer);
		bottomContainer.Y = outerGrid.Bottom;
		bottomContainer.X = 6;
		Label label = new Label(gui);
		bottomContainer.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = 30;
		label.Text = "TOTAL:";
		label.FitToText();
		bottomContainer.CenterChildVertically(label);
		lblTotalProduced = new Label(gui);
		bottomContainer.Add(lblTotalProduced);
		lblTotalProduced.Init(Label.LabelType.LCDNormal);
		lblTotalProduced.X = 245;
		bottomContainer.CenterChildVertically(lblTotalProduced);
		lblTotalConsumed = new Label(gui);
		bottomContainer.Add(lblTotalConsumed);
		lblTotalConsumed.Init(Label.LabelType.LCDNormal);
		lblTotalConsumed.X = 335;
		bottomContainer.CenterChildVertically(lblTotalConsumed);
		lblTotalDegraded = new Label(gui);
		bottomContainer.Add(lblTotalDegraded);
		lblTotalDegraded.Init(Label.LabelType.LCDNormal);
		lblTotalDegraded.X = degradedX;
		bottomContainer.CenterChildVertically(lblTotalDegraded);
		lblTotalDisappeared = new Label(gui);
		bottomContainer.Add(lblTotalDisappeared);
		lblTotalDisappeared.Init(Label.LabelType.LCDNormal);
		lblTotalDisappeared.X = disappearedX;
		bottomContainer.CenterChildVertically(lblTotalDisappeared);
		if (GameData.Instance.GUIConstants.ProductionCategories != null && GameData.Instance.GUIConstants.ProductionCategories.Length != 0)
		{
			categoriesToShow = GameData.Instance.GUIConstants.ProductionCategories.Select((string c) => GameData.Instance.AllEntityCategories[c]).ToList();
		}
	}

	private void CreateTooltips()
	{
		nameTooltip = CreateTooltip("Name", "Name of product");
		producedTooltip = CreateTooltip("Produced", "Produced by the colony");
		usedTooltip = CreateTooltip("Used", "Used in production");
		wastedTooltip = CreateTooltip("Spoiled", "Degraded into waste");
		disappearedTooltip = CreateTooltip("Disappeared", "Disappeared");
		productivityTooltip = CreateTooltip("Productivity", "Mean productivity");
	}

	private void ResizeHeight()
	{
		if (outerGrid.surface.Height < GridMaxHeight)
		{
			outerGrid.Height = outerGrid.surface.Height;
		}
		else
		{
			outerGrid.Height = GridMaxHeight;
		}
		bottomContainer.Y = outerGrid.Bottom;
	}

	public void LoadUserSettings(SortingSettings<ProductionSettings.SortColumns> settings)
	{
		sortingButtons.Fill(settings);
	}

	private void outerGrid_HeightResize(UIComponent sender)
	{
		ResizeHeight();
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtons = new SortingButtons<ProductionSettings.SortColumns>(base.GUIManager);
		sortingButtons.Width = Width;
		sortingButtons.Height = 30;
		sortingButtons.Position = new Point(0, 0);
		Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		sortingButtons.AddTextButton(215, "NAME", ProductionSettings.SortColumns.Name, nameTooltip);
		sortingButtons.AddTextButton(90, "PROD.", ProductionSettings.SortColumns.Produced, producedTooltip);
		sortingButtons.AddTextButton(90, "USED", ProductionSettings.SortColumns.UsedInProduction, usedTooltip);
		sortingButtons.AddTextButton(90, "DEGR.", ProductionSettings.SortColumns.Wasted, wastedTooltip);
		sortingButtons.AddTextButton(90, "DISAPP.", ProductionSettings.SortColumns.Disappeared, disappearedTooltip);
		sortingButtons.AddTextButton(90, "PRDCTIV.", ProductionSettings.SortColumns.Productivity, productivityTooltip);
	}

	private void tbSort_Click()
	{
		Populate();
	}

	public override void RefreshData()
	{
		Populate();
		base.RefreshData();
	}

	private void Populate()
	{
		Expedition firstPlayerExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
		if (firstPlayerExpedition == null)
		{
			return;
		}
		EntityGroup ownedEntities = firstPlayerExpedition.OwnedEntities;
		Grid categoryGrid = null;
		categoryGrids = new List<Grid>();
		ProductionStatistics stats = The.InGameUI.UIAllegiance.Statistics.ProductionStatistics;
		Dictionary<EntityType, List<DataPoint<float>>> dict = stats.Stats[ProductionStatistics.StatTypes.Produced];
		Dictionary<EntityType, List<DataPoint<float>>> dict2 = stats.Stats[ProductionStatistics.StatTypes.UsedAsInput];
		Dictionary<EntityType, List<DataPoint<float>>> dict3 = stats.Stats[ProductionStatistics.StatTypes.Degraded];
		Dictionary<EntityType, List<DataPoint<float>>> dict4 = stats.Stats[ProductionStatistics.StatTypes.Disappeared];
		Dictionary<EntityType, List<DataPoint<Productivity>>> productivityStats = stats.ProductivityStats;
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		DateAndTime.TimeDateYear fromDate = base.FromDate;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		outerGrid.BeginAddingEntries();
		if (categoriesToShow != null)
		{
			foreach (EntityType typesWithStat in stats.TypesWithStats)
			{
				if (!categoriesToShow.Contains(typesWithStat.Category))
				{
					continue;
				}
				object category = typesWithStat.Category;
				int noOfAvailableItems = ownedEntities.CountAvailableItems(typesWithStat);
				int num5 = Statistic.SumDataPoints(dict, typesWithStat, fromDate, currentTimeDateYear);
				int num6 = Statistic.SumDataPoints(dict2, typesWithStat, fromDate, currentTimeDateYear);
				int num7 = Statistic.SumDataPoints(dict3, typesWithStat, fromDate, currentTimeDateYear);
				int num8 = Statistic.SumDataPoints(dict4, typesWithStat, fromDate, currentTimeDateYear);
				ProductionStatistics.GetMeanOfDataPoints(productivityStats, typesWithStat, fromDate, currentTimeDateYear, out var totalProductivity, out var toolProductivity, out var skillProductivity, out var energyProductivity);
				num += num5;
				num2 += num6;
				num3 += num7;
				num4 += num8;
				CollapsablePanel cpCategory = null;
				if (outerGrid.TryGetEntry(category, out var item))
				{
					cpCategory = item as CollapsablePanel;
					categoryGrid = (Grid)cpCategory.ExpandedPanel.Controls[0];
					if (!categoryGrids.Contains(categoryGrid))
					{
						categoryGrid.BeginAddingEntries();
						categoryGrids.Add(categoryGrid);
					}
				}
				if (cpCategory == null)
				{
					AddCategoryRow(ref cpCategory, ref categoryGrid, category);
					categoryGrid.BeginAddingEntries();
					categoryGrids.Add(categoryGrid);
				}
				if (!categoryGrid.TryGetEntry(typesWithStat, out var item2))
				{
					item2 = AddItemRow(categoryGrid, typesWithStat, ownedEntities, useCurrentUIOwner: false);
				}
				UpdateItemRow(item2, typesWithStat, noOfAvailableItems, num5, num6, num7, num8, totalProductivity, toolProductivity, skillProductivity, energyProductivity);
			}
		}
		foreach (CollapsablePanel entry in outerGrid.Entries)
		{
			((Grid)entry.ExpandedPanel.Controls[0]).DeleteEntries((EntityType j) => stats.TypesWithStats.Contains(j));
		}
		List<object> list = new List<object>();
		foreach (object key in outerGrid.EntriesByKey.Keys)
		{
			if (((Grid)(outerGrid.EntriesByKey[key] as CollapsablePanel).ExpandedPanel.Controls[0]).Entries.Count == 0)
			{
				list.Add(key);
			}
		}
		foreach (object item3 in list)
		{
			outerGrid.RemoveEntry(item3);
		}
		InventoryPanel.DoCategorySorting(outerGrid, categoryGrids, The.InGameUI.ProductionSettings.SortingSettings.SortOrder);
		outerGrid.EndAddingEntries();
		UpdateTotals(num, num2, num3, num4);
		ResizeHeight();
	}

	private void UpdateTotals(int producedTotal, int consumedTotal, int degradedTotal, int disappearedTotal)
	{
		SetLabelPositiveColorAndValue(lblTotalProduced, producedTotal);
		lblTotalConsumed.Text = consumedTotal.ToString();
		SetLabelNegativeColorAndValue(lblTotalDegraded, degradedTotal);
		SetLabelNegativeColorAndValue(lblTotalDisappeared, disappearedTotal);
	}

	private void SetLabelNegativeColorAndValue(Label lbl, int value)
	{
		lbl.Text = value.ToString();
		if (value > 0)
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
		}
	}

	private void SetLabelPositiveColorAndValue(Label lbl, int value)
	{
		lbl.Text = value.ToString();
		if (value > 0)
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
		}
	}

	private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int noOfAvailableItems, int produced, int used, int degraded, int disappeared, float? productivity, float? toolProd, float? skillProd, float? energyProd)
	{
		((DataTypeButton)itemRow.FindChildById(DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableItems > 0);
		itemRow.FindChildById<Label>(DataControlID.Produced, out var child, firstLevelOnly: false);
		SetLabelPositiveColorAndValue(child, produced);
		child.FitToText();
		child.ToolTip = producedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Consumed, out var child2, firstLevelOnly: false);
		child2.Text = used.ToString();
		child2.FitToText();
		child2.ToolTip = usedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Degraded, out var child3, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child3, degraded);
		child3.FitToText();
		child3.ToolTip = wastedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Disappeared, out var child4, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child4, disappeared);
		child4.FitToText();
		child4.ToolTip = disappearedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Productivity, out var child5, firstLevelOnly: false);
		if (productivity.HasValue)
		{
			child5.Visible = true;
			child5.Text = productivity.Value.ToString("N2");
			child5.FitToText();
			child5.ToolTip = productivityTooltip;
			StringBuilder stringBuilder = new StringBuilder();
			Common.AppendHeaderOnLightBG(stringBuilder, "Productivity");
			Common.Append(stringBuilder, "Mean (average) productivity in the selected timespan.");
			Common.AppendDividerOnOwnLine(stringBuilder);
			Common.Append(stringBuilder, "Tools: ");
			Common.AppendFormat(stringBuilder, "{0:N2}", true, toolProd);
			Common.AppendLine(stringBuilder);
			Common.Append(stringBuilder, "Skill: ");
			Common.AppendFormat(stringBuilder, "{0:N2}", true, skillProd);
			Common.AppendLine(stringBuilder);
			Common.Append(stringBuilder, "Worker energy: ");
			Common.AppendFormat(stringBuilder, "{0:N2}", true, energyProd);
			Common.AppendDividerOnOwnLine(stringBuilder);
			Common.Append(stringBuilder, "Total productivity: ");
			Common.AppendFormat(stringBuilder, "{0:N2}", true, productivity);
			child5.ToolTip = stringBuilder.ToString();
		}
		else
		{
			child5.Visible = false;
		}
		object orderByTag = null;
		switch (The.InGameUI.ProductionSettings.SortingSettings.SortedBy)
		{
		case ProductionSettings.SortColumns.Name:
			orderByTag = entityType.Name;
			break;
		case ProductionSettings.SortColumns.Consumed:
			orderByTag = used;
			break;
		case ProductionSettings.SortColumns.Produced:
			orderByTag = produced;
			break;
		case ProductionSettings.SortColumns.Disappeared:
			orderByTag = disappeared;
			break;
		case ProductionSettings.SortColumns.Wasted:
			orderByTag = degraded;
			break;
		case ProductionSettings.SortColumns.Productivity:
			orderByTag = productivity;
			break;
		}
		itemRow.OrderByTag1 = orderByTag;
	}

	private UIComponent AddItemRow(Grid grid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner)
	{
		GUIManager gUIManager = base.GUIManager;
		UIComponent uIComponent = new UIComponent(gUIManager)
		{
			DebugTag = "stocksItem"
		};
		grid.AddEntry(entityType, uIComponent);
		InventoryPanel.AddEntityTypeIcon(entityType, uIComponent, 12);
		DataTypeButton dataTypeButton = new DataTypeButton(gUIManager, DataSheet.InfoToShow.Production, entityType, GoalEvaluator.GetOwnerID(owner), useCurrentUIOwner);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = DataControlID.Caption;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.IsRoot = true;
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 175;
		dataTypeButton.X = 30;
		uIComponent.CenterChildVertically(dataTypeButton);
		Label label = new Label(gUIManager);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = 245;
		label.ID = DataControlID.Produced;
		uIComponent.CenterChildVertically(label);
		Label label2 = new Label(gUIManager);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.X = 335;
		label2.ID = DataControlID.Consumed;
		uIComponent.CenterChildVertically(label2);
		Label label3 = new Label(gUIManager);
		uIComponent.Add(label3);
		label3.Init(Label.LabelType.LCDNormal);
		label3.X = degradedX;
		label3.ID = DataControlID.Degraded;
		uIComponent.CenterChildVertically(label3);
		Label label4 = new Label(gUIManager);
		uIComponent.Add(label4);
		label4.Init(Label.LabelType.LCDNormal);
		label4.X = disappearedX;
		label4.ID = DataControlID.Disappeared;
		uIComponent.CenterChildVertically(label4);
		Label label5 = new Label(gUIManager);
		uIComponent.Add(label5);
		label5.Init(Label.LabelType.LCDNormal);
		label5.X = productivityX;
		label5.ID = DataControlID.Productivity;
		uIComponent.CenterChildVertically(label5);
		label5.TooltipExpires = false;
		label5.TooltipWidth = 260;
		return uIComponent;
	}

	private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, object key)
	{
		GUIManager gUIManager = base.GUIManager;
		EntityCategory entityCategory = key as EntityCategory;
		cpCategory = new CollapsablePanel(gUIManager, CollapsablePanel.PanelType.DropDownBig);
		cpCategory.HeadingYPos = 4;
		cpCategory.CollapsedHeight = 28;
		outerGrid.AddEntry(key, cpCategory);
		cpCategory.OrderByTag1 = entityCategory.Name;
		cpCategory.Init();
		cpCategory.Title = entityCategory.Name;
		cpCategory.Width = outerGrid.Width;
		categoryGrid = new Grid(gUIManager, ListBoxType.LCD, Label.LabelType.LCDNormal);
		categoryGrid.DebugTag = "categoryGrid";
		categoryGrid.FixedItemHeights = true;
		categoryGrid.Width = cpCategory.Width;
		cpCategory.AddContent(categoryGrid);
		categoryGrid.ScrollBarEnabled = false;
		categoryGrid.ItemHeight = 26;
		categoryGrid.CanGrowInHeight = true;
		categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid.IsOuterGrid = false;
	}
}
