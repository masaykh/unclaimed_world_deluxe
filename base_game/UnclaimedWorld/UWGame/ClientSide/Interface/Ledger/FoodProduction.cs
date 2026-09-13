using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class FoodProduction : LedgerSheet
{
	private const int columnWidth = 90;

	private const int nameWidth = 215;

	private const int itemTypeIconColumnX = 12;

	private const int nameX = 30;

	private const int productionX = 245;

	private const int consumedX = 335;

	private int degradedX = 425;

	private int critterEatenX = 515;

	private int disappearedX = 605;

	private int bottomPartHeight = 40;

	private string wastedTooltip;

	private string nameTooltip;

	private string producedTooltip;

	private string consumedTooltip;

	private string verminTooltip;

	private string disappearedTooltip;

	private Grid outerGrid;

	public const int ItemHeight = 26;

	private SortingButtons<FoodProductionSettings.SortColumns> sortingButtons;

	private List<Grid> categoryGrids = new List<Grid>();

	private List<EntityCategory> categoriesToShow;

	private UIComponent bottomContainer;

	private Label lblTotalProduced;

	private Label lblTotalConsumed;

	private Label lblTotalDegraded;

	private Label lblTotalCritterEaten;

	private Label lblTotalDisappeared;

	private int gridYPos;

	public override string DisplayName => "Food production";

	public override string Tooltip => "Shows food produced, consumed and wasted. Listed by food types.";

	public override bool ShowRangeSelector => true;

	private int GridMaxHeight => Height - gridYPos - bottomPartHeight;

	public FoodProduction(GUIManager gui, int width, int height)
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
		lblTotalCritterEaten = new Label(gui);
		bottomContainer.Add(lblTotalCritterEaten);
		lblTotalCritterEaten.Init(Label.LabelType.LCDNormal);
		lblTotalCritterEaten.X = critterEatenX;
		bottomContainer.CenterChildVertically(lblTotalCritterEaten);
		lblTotalDisappeared = new Label(gui);
		bottomContainer.Add(lblTotalDisappeared);
		lblTotalDisappeared.Init(Label.LabelType.LCDNormal);
		lblTotalDisappeared.X = disappearedX;
		bottomContainer.CenterChildVertically(lblTotalDisappeared);
		if (GameData.Instance.GUIConstants.FoodProductionCategories != null && GameData.Instance.GUIConstants.FoodProductionCategories.Length != 0)
		{
			categoriesToShow = GameData.Instance.GUIConstants.FoodProductionCategories.Select((string c) => GameData.Instance.AllEntityCategories[c]).ToList();
		}
	}

	private void CreateTooltips()
	{
		nameTooltip = CreateTooltip("Name", "Name of food type");
		producedTooltip = CreateTooltip("Produced", "Produced by the colony");
		consumedTooltip = CreateTooltip("Consumed", "Consumed by colony members");
		wastedTooltip = CreateTooltip("Spoiled", "Spoiled/degraded into waste");
		verminTooltip = CreateTooltip("Vermin", "Eaten by creatures");
		disappearedTooltip = CreateTooltip("Disappeared", "Disappeared");
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

	public void LoadUserSettings(SortingSettings<FoodProductionSettings.SortColumns> settings)
	{
		sortingButtons.Fill(settings);
	}

	private void outerGrid_HeightResize(UIComponent sender)
	{
		ResizeHeight();
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtons = new SortingButtons<FoodProductionSettings.SortColumns>(base.GUIManager);
		sortingButtons.Width = Width;
		sortingButtons.Height = 30;
		sortingButtons.Position = new Point(0, 0);
		Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		sortingButtons.AddTextButton(215, "NAME", FoodProductionSettings.SortColumns.Name, nameTooltip);
		sortingButtons.AddTextButton(90, "PROD.", FoodProductionSettings.SortColumns.Produced, producedTooltip);
		sortingButtons.AddTextButton(90, "CONS.", FoodProductionSettings.SortColumns.Consumed, consumedTooltip);
		sortingButtons.AddTextButton(90, "SPOIL.", FoodProductionSettings.SortColumns.Wasted, wastedTooltip);
		sortingButtons.AddTextButton(90, "VERMIN", FoodProductionSettings.SortColumns.EatenByCreatures, verminTooltip);
		sortingButtons.AddTextButton(90, "DISAPP.", FoodProductionSettings.SortColumns.Disappeared, disappearedTooltip);
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
		Dictionary<EntityType, List<DataPoint<float>>> dict2 = stats.Stats[ProductionStatistics.StatTypes.ConsumedFood];
		Dictionary<EntityType, List<DataPoint<float>>> dict3 = stats.Stats[ProductionStatistics.StatTypes.Degraded];
		Dictionary<EntityType, List<DataPoint<float>>> dict4 = stats.Stats[ProductionStatistics.StatTypes.EatenByCreatures];
		Dictionary<EntityType, List<DataPoint<float>>> dict5 = stats.Stats[ProductionStatistics.StatTypes.Disappeared];
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		DateAndTime.TimeDateYear fromDate = base.FromDate;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		outerGrid.BeginAddingEntries();
		if (categoriesToShow != null)
		{
			foreach (EntityType typesWithStat in stats.TypesWithStats)
			{
				if (typesWithStat.ItemType == null || typesWithStat.ItemType.FoodType == null || !categoriesToShow.Contains(typesWithStat.Category))
				{
					continue;
				}
				object category = typesWithStat.Category;
				int noOfAvailableItems = ownedEntities.CountAvailableItems(typesWithStat);
				int num6 = Statistic.SumDataPoints(dict, typesWithStat, fromDate, currentTimeDateYear);
				int num7 = Statistic.SumDataPoints(dict2, typesWithStat, fromDate, currentTimeDateYear);
				int num8 = Statistic.SumDataPoints(dict3, typesWithStat, fromDate, currentTimeDateYear);
				int num9 = Statistic.SumDataPoints(dict4, typesWithStat, fromDate, currentTimeDateYear);
				int num10 = Statistic.SumDataPoints(dict5, typesWithStat, fromDate, currentTimeDateYear);
				num += num6;
				num2 += num7;
				num3 += num8;
				num4 += num9;
				num5 += num10;
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
				UpdateItemRow(item2, typesWithStat, noOfAvailableItems, num6, num7, num8, num9, num10);
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
		InventoryPanel.DoCategorySorting(outerGrid, categoryGrids, The.InGameUI.FoodProductionSettings.SortingSettings.SortOrder);
		outerGrid.EndAddingEntries();
		UpdateTotals(num, num2, num3, num4, num5);
		ResizeHeight();
	}

	private void UpdateTotals(int producedTotal, int consumedTotal, int degradedTotal, int critterEatenTotal, int disappearedTotal)
	{
		SetLabelPositiveColorAndValue(lblTotalProduced, producedTotal);
		lblTotalConsumed.Text = consumedTotal.ToString();
		SetLabelNegativeColorAndValue(lblTotalDegraded, degradedTotal);
		SetLabelNegativeColorAndValue(lblTotalCritterEaten, critterEatenTotal);
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

	private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int noOfAvailableItems, int produced, int consumed, int degraded, int critterEaten, int disappeared)
	{
		((DataTypeButton)itemRow.FindChildById(DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableItems > 0);
		itemRow.FindChildById<Label>(DataControlID.Produced, out var child, firstLevelOnly: false);
		SetLabelPositiveColorAndValue(child, produced);
		child.FitToText();
		child.ToolTip = producedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Consumed, out var child2, firstLevelOnly: false);
		child2.Text = consumed.ToString();
		child2.FitToText();
		child2.ToolTip = consumedTooltip;
		itemRow.FindChildById<Label>(DataControlID.Degraded, out var child3, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child3, degraded);
		child3.FitToText();
		child3.ToolTip = wastedTooltip;
		itemRow.FindChildById<Label>(DataControlID.CritterEaten, out var child4, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child4, critterEaten);
		child4.FitToText();
		child4.ToolTip = verminTooltip;
		itemRow.FindChildById<Label>(DataControlID.Disappeared, out var child5, firstLevelOnly: false);
		SetLabelNegativeColorAndValue(child5, disappeared);
		child5.FitToText();
		child5.ToolTip = disappearedTooltip;
		object orderByTag = null;
		switch (The.InGameUI.FoodProductionSettings.SortingSettings.SortedBy)
		{
		case FoodProductionSettings.SortColumns.Name:
			orderByTag = entityType.Name;
			break;
		case FoodProductionSettings.SortColumns.Consumed:
			orderByTag = consumed;
			break;
		case FoodProductionSettings.SortColumns.Produced:
			orderByTag = produced;
			break;
		case FoodProductionSettings.SortColumns.Disappeared:
			orderByTag = disappeared;
			break;
		case FoodProductionSettings.SortColumns.Wasted:
			orderByTag = degraded;
			break;
		case FoodProductionSettings.SortColumns.EatenByCreatures:
			orderByTag = critterEaten;
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
		label4.X = critterEatenX;
		label4.ID = DataControlID.CritterEaten;
		uIComponent.CenterChildVertically(label4);
		Label label5 = new Label(gUIManager);
		uIComponent.Add(label5);
		label5.Init(Label.LabelType.LCDNormal);
		label5.X = disappearedX;
		label5.ID = DataControlID.Disappeared;
		uIComponent.CenterChildVertically(label5);
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
