using System.Collections.Generic;
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

public class Kills : LedgerSheet
{
	private const int columnWidth = 90;

	private const int nameWidth = 215;

	private const int itemTypeIconColumnX = 12;

	private const int nameX = 30;

	private const int killedX = 245;

	private int bottomPartHeight = 40;

	private string nameTooltip;

	private string killedTooltip;

	private Grid outerGrid;

	public const int ItemHeight = 26;

	private SortingButtons<KillsSettings.SortColumns> sortingButtons;

	private UIComponent bottomContainer;

	private Label lblTotalKilled;

	private int gridYPos;

	public override string DisplayName => "Kills";

	public override string Tooltip => "Shows the number of killed creatures.";

	public override bool ShowRangeSelector => false;

	private int GridMaxHeight => Height - gridYPos - bottomPartHeight;

	public Kills(GUIManager gui, int width, int height)
		: base(gui, width, height)
	{
		CreateTooltips();
		CreateGridHeaderButtons();
		gridYPos = sortingButtons.Bottom + 6;
		outerGrid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		Add(outerGrid);
		outerGrid.RenderType = RenderType.CRTAndLCD;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Width = Width;
		outerGrid.Height = GridMaxHeight;
		outerGrid.Position = new Point(0, gridYPos);
		outerGrid.RowSpacing = 1;
		outerGrid.FixedItemHeights = true;
		outerGrid.ItemHeight = 26;
		outerGrid.CanGrowInHeight = false;
		outerGrid.ScrollBarEnabled = true;
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
		lblTotalKilled = new Label(gui);
		bottomContainer.Add(lblTotalKilled);
		lblTotalKilled.Init(Label.LabelType.LCDNormal);
		lblTotalKilled.X = 245;
		bottomContainer.CenterChildVertically(lblTotalKilled);
	}

	private void CreateTooltips()
	{
		nameTooltip = CreateTooltip("Name", "Name of product");
		killedTooltip = CreateTooltip("Killed", "Killed by the colony");
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

	public void LoadUserSettings(SortingSettings<KillsSettings.SortColumns> settings)
	{
		sortingButtons.Fill(settings);
	}

	private void outerGrid_HeightResize(UIComponent sender)
	{
		ResizeHeight();
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtons = new SortingButtons<KillsSettings.SortColumns>(base.GUIManager);
		sortingButtons.Width = Width;
		sortingButtons.Height = 30;
		sortingButtons.Position = new Point(0, 0);
		Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		sortingButtons.AddTextButton(215, "NAME", KillsSettings.SortColumns.Name, nameTooltip);
		sortingButtons.AddTextButton(90, "KILLED.", KillsSettings.SortColumns.Kills, killedTooltip);
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
		KillStatistics killStatistics = The.InGameUI.UIAllegiance.Statistics.KillStatistics;
		_ = The.Sim.DateAndTime.CurrentTimeDateYear;
		_ = base.FromDate;
		int num = 0;
		outerGrid.BeginAddingEntries();
		foreach (KeyValuePair<EntityType, int> kill in killStatistics.Kills)
		{
			EntityType key = kill.Key;
			int value = kill.Value;
			num += value;
			if (!outerGrid.TryGetEntry(key, out var item))
			{
				item = AddItemRow(key, ownedEntities, useCurrentUIOwner: false);
			}
			UpdateItemRow(item, key, value);
		}
		outerGrid.Sort((UIComponent u) => u.OrderByTag1, The.InGameUI.KillsSettings.SortingSettings.SortOrder);
		outerGrid.EndAddingEntries();
		UpdateTotals(num);
		ResizeHeight();
	}

	private void UpdateTotals(int killedTotal)
	{
		lblTotalKilled.Text = killedTotal.ToString();
		lblTotalKilled.FitToText();
		lblTotalKilled.AlignRight(245);
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

	private void UpdateItemRow(UIComponent itemRow, EntityType entityType, int killed)
	{
		_ = (DataTypeButton)itemRow.FindChildById(DataControlID.Caption);
		itemRow.FindChildById<Label>(DataControlID.Killed, out var child, firstLevelOnly: false);
		child.Text = killed.ToString();
		child.FitToText();
		child.ToolTip = killedTooltip;
		child.AlignRight(245);
		object orderByTag = null;
		switch (The.InGameUI.KillsSettings.SortingSettings.SortedBy)
		{
		case KillsSettings.SortColumns.Name:
			orderByTag = entityType.Name;
			break;
		case KillsSettings.SortColumns.Kills:
			orderByTag = killed;
			break;
		}
		itemRow.OrderByTag1 = orderByTag;
	}

	private UIComponent AddItemRow(EntityType entityType, EntityGroup owner, bool useCurrentUIOwner)
	{
		GUIManager gUIManager = base.GUIManager;
		UIComponent uIComponent = new UIComponent(gUIManager);
		outerGrid.AddEntry(entityType, uIComponent);
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
		label.ID = DataControlID.Killed;
		uIComponent.CenterChildVertically(label);
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
