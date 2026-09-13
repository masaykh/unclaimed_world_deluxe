using System;
using System.Collections.Generic;
using System.Linq;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Inventory;

public class InventoryPanel : RosterPanel
{
	public enum ProductionMode
	{
		Basic,
		Advanced
	}

	private class DistanceArg : EventArgs
	{
		public InventorySettings.Availability Availability;
	}

	public class Availability
	{
		public int NoOfAvailableItems;

		public int NoOfAvailableItemsIncludingIntrinsic;

		public int NoOfIncompleteEntities;

		public int NoOfEntitiesUsedAsParts;

		public int NoOfEntitiesOffSite;

		public int NoOfEntitiesOwnedByOthers;

		public int AvailableToBuy;

		public List<EntityID> AllEntities;

		public List<EntityID> AvailableEntities;

		public List<EntityID> UnavailableEntities;
	}

	private Expedition expedition;

	private const int topPanelExpandedNoTrackingHeight = 112;

	private const int topPanelExpandedWithTrackingHeight = 150;

	private const int topPanelCollapsedHeight = 34;

	public const int DefaultStocksMaxValue = 5;

	public const int MaxStockOrder = 99;

	private const int maxNumberOfMissingInputsToDisplayBuildingsWithout = 1;

	private bool haveActiveTracking = true;

	private Rectangle screenDimensions = new Rectangle(40, 700, 400, 300);

	private Grid grdCategoryView;

	private Grid grdListView;

	private SortingButtons<InventorySettings.SortColumns> sortingButtons;

	private UIComponent sortingButtonsContainer;

	private List<Grid> categoryGrids = new List<Grid>();

	private ViewType viewType;

	private int itemTypeIconColumnX = 12;

	private int captionX = 30;

	private int availableX = 204;

	private int unavailableX = 243;

	private int itemTypeProductionTargetColumnX = 300;

	private LCDInnerPanel filterAndTrackingPanel;

	private FilterPropertiesPanel filterPropertiesPanel;

	private ComboBox cbEntityTracking;

	private RadioGroup rgDistance;

	private ImageButton ibNormalDistance;

	private ImageButton ibAttainableDistance;

	private ImageButton ibCyclopedia;

	private ImageButton btIncludeSalvage;

	private const string tbTrackNotTrackedTooltip = "Track this item/structure";

	public const string TrackingLimitTooltip = "No more objects can be tracked, cancel some of the other tracked objects first.";

	public const string expandFilterTooltip = "Display search options";

	public const string collapseFilterTooltip = "Hide search options";

	private HashSet<EntityType> currentListData;

	private Dictionary<EntityType, Availability> allAvailableItems = new Dictionary<EntityType, Availability>();

	public new const int ItemHeight = 26;

	private RadioButton rbOr;

	private RadioButton rbAnd;

	private CheckBox cbInputChain;

	private CheckBox cbOutputChain;

	private CheckBox cbToolsOption;

	private ImageButton ibCategory;

	private ImageButton ibList;

	private Image rowDivider1;

	private Image rowDivider2;

	private Image columnDividerVertical1;

	private Image columnDividerVertical2;

	private TextButton tbExpand;

	private Color encyclopediaTint;

	private Dictionary<EntityType, bool> standingOrderButtonWasChecked = new Dictionary<EntityType, bool>();

	private const int orderedX = 200;

	private TextButton latestCheckedTextButton;

	public InventoryPanel()
		: base("PRODUCTION", 555, needBottomMarginForButtons: false)
	{
		expedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
		encyclopediaTint = "#636D8C".ColorFromHex();
		encyclopediaTint.A = 160;
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			CreateTopPanel();
		}
		CreateGridHeaderButtons();
		grdCategoryView = CreateOuterGrid(fixedItemHeight: false);
		grdListView = CreateOuterGrid(fixedItemHeight: true);
		LoadUserSettings();
		SetView(ViewType.Categories);
		ShowRelevantGrid();
	}

	private void CreateGridHeaderButtons()
	{
		sortingButtonsContainer = new UIComponent(Interface.gui);
		lcdSurface.Add(sortingButtonsContainer);
		sortingButtonsContainer.Width = lcdSurface.Width;
		sortingButtonsContainer.Height = 50;
		sortingButtonsContainer.Position = new Point(0, 160);
		sortingButtons = new SortingButtons<InventorySettings.SortColumns>(Interface.gui);
		sortingButtons.Width = lcdSurface.Width;
		sortingButtons.Height = 50;
		sortingButtons.Position = new Point(60, 0);
		sortingButtonsContainer.Add(sortingButtons);
		sortingButtons.SortClicked += tbSort_Click;
		ibList = new ImageButton(Interface.gui);
		sortingButtonsContainer.Add(ibList);
		ibList.InitWithIcon(ImageButtonType.LCD, "basic_icon_list", hasCheckedState: true);
		ibList.CheckedMode = CheckedModes.CanBeChecked;
		ibList.Click += tbListView_Click;
		ibList.Y = 0;
		ibList.X = 0;
		ibList.ToolTip = "List view";
		ibList.Width = 30;
		ibList.Height = 30;
		ibList.RecalculateIconPosition();
		ibCategory = new ImageButton(Interface.gui);
		sortingButtonsContainer.Add(ibCategory);
		ibCategory.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", hasCheckedState: true);
		ibCategory.CheckedMode = CheckedModes.CanBeChecked;
		ibCategory.Click += tbCategoryView_Click;
		ibCategory.ToolTip = "Category view";
		ibCategory.Width = 30;
		ibCategory.Y = 0;
		ibCategory.X = 30;
		ibCategory.IsChecked = true;
		ibCategory.Height = 30;
		ibCategory.RecalculateIconPosition();
		sortingButtons.CreateTextButton(0, 138, "NAME", InventorySettings.SortColumns.Name);
		sortingButtons.CreateImageButton(134, 72, "IN STOCK", InventorySettings.SortColumns.InStock);
		sortingButtons.CreateImageButton(202, 168, "CAN PRODUCE", InventorySettings.SortColumns.CanProduce);
		sortingButtons.CreateImageButton(366, 72, "TRACKED", InventorySettings.SortColumns.Tracking);
	}

	private Grid CreateOuterGrid(bool fixedItemHeight)
	{
		return new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal)
		{
			FixedItemHeights = fixedItemHeight,
			RenderType = RenderType.CRTAndLCD,
			Font = GUIManager.LCDandHUDBodyFontPath,
			Width = lcdSurface.Width,
			Height = lcdSurface.Height - lcdSurface.Controls[lcdSurface.Controls.Count - 1].Bottom + 15,
			ItemHeight = 26,
			Position = new Point(0, 195),
			ScrollBarEnabled = true,
			RowSpacing = 1
		};
	}

	private void ShowRelevantGrid()
	{
		switch (viewType)
		{
		case ViewType.List:
			lcdSurface.Remove(grdCategoryView);
			lcdSurface.Add(grdListView);
			break;
		case ViewType.Categories:
			lcdSurface.Remove(grdListView);
			lcdSurface.Add(grdCategoryView);
			break;
		}
	}

	private void CreateTopPanel()
	{
		filterAndTrackingPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: false);
		filterAndTrackingPanel.HorizontalContentPadding = 0;
		filterAndTrackingPanel.VerticalContentPadding = 5;
		filterAndTrackingPanel.ContentHeight = 150;
		lcdSurface.Add(filterAndTrackingPanel.Panel);
		tbExpand = CreateTextButton(Interface.gui, 4, 5, 30, "MORE", Expand_Click);
		tbExpand.ScaleWidthToFitText();
		filterAndTrackingPanel.AddContent(tbExpand);
		tbExpand.X = 4;
		tbExpand.Y = 5;
		tbExpand.ToolTip = "Hide search options";
		RadioGroup radioGroup = new RadioGroup(Interface.gui);
		radioGroup.Width = 500;
		radioGroup.Height = 300;
		radioGroup.Position = new Point(0, 0);
		rbOr = CreateRadioButton(60, 5, 50, "OR", "Display only the items that are in AT LEAST ONE of the selected filters", CheckBoxType.LCDRadioBanner, rgOrAnd_Click);
		rbOr.IsChecked = true;
		rbOr.Tag1 = InventorySettings.AndOr.Or;
		rbAnd = CreateRadioButton(120, 5, 70, "AND", "Display only the items that are in ALL of the selected filters", CheckBoxType.LCDRadioBanner, rgOrAnd_Click);
		rbAnd.IsChecked = false;
		rbAnd.Tag1 = InventorySettings.AndOr.And;
		radioGroup.Add(rbOr);
		radioGroup.Add(rbAnd);
		filterAndTrackingPanel.AddContent(radioGroup);
		columnDividerVertical1 = new Image(Interface.gui);
		filterAndTrackingPanel.AddContent(columnDividerVertical1);
		columnDividerVertical1.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		columnDividerVertical1.X = 204;
		columnDividerVertical1.Y = 6;
		columnDividerVertical1.Width = 2;
		columnDividerVertical1.Height = tbExpand.Bottom - 10;
		columnDividerVertical1.ScaleImageToSizeOfControl = true;
		rgDistance = new RadioGroup(Interface.gui);
		rgDistance.Height = 30;
		rgDistance.Width = 222;
		rgDistance.NewMemberChecked += rgDistance_NewMemberChecked;
		filterAndTrackingPanel.AddContent(rgDistance);
		rgDistance.Y = 5;
		rgDistance.X = 212;
		ibNormalDistance = new ImageButton(Interface.gui);
		rgDistance.Add(ibNormalDistance);
		ibNormalDistance.InitWithIcon(ImageButtonType.LCD, "basic_icon_branchShort", hasCheckedState: true);
		ibNormalDistance.CheckedMode = CheckedModes.CanBeChecked;
		ibNormalDistance.Width = 80;
		ibNormalDistance.X = 0;
		ibNormalDistance.Y = 0;
		ibNormalDistance.ToolTip = "'AVAILABLE NOW': Lists only objects that are owned by the colony now or can be made in one step.";
		ibNormalDistance.EventArgs = new DistanceArg
		{
			Availability = InventorySettings.Availability.AvailableNow
		};
		ibAttainableDistance = new ImageButton(Interface.gui);
		rgDistance.Add(ibAttainableDistance);
		ibAttainableDistance.InitWithIcon(ImageButtonType.LCD, "basic_icon_branchLong", hasCheckedState: true);
		ibAttainableDistance.CheckedMode = CheckedModes.CanBeChecked;
		ibAttainableDistance.Width = 80;
		ibAttainableDistance.X = ibNormalDistance.Right + 6;
		ibAttainableDistance.Y = ibNormalDistance.Y;
		ibAttainableDistance.ToolTip = "'ATTAINABLE': Also lists objects that can be made in the future, based on the resource types that have been discovered.";
		ibAttainableDistance.EventArgs = new DistanceArg
		{
			Availability = InventorySettings.Availability.Attainable
		};
		ibCyclopedia = new ImageButton(Interface.gui);
		rgDistance.Add(ibCyclopedia);
		ibCyclopedia.InitWithIcon(ImageButtonType.LCD, "basic_icon_book", hasCheckedState: true);
		ibCyclopedia.CheckedMode = CheckedModes.CanBeChecked;
		ibCyclopedia.Width = 40;
		ibCyclopedia.X = ibAttainableDistance.Right + 6;
		ibCyclopedia.Y = ibAttainableDistance.Y;
		ibCyclopedia.ToolTip = "'ENCYCLOPEDIA': Shows all known blueprints of every item/process currently stored in the PPU";
		ibCyclopedia.EventArgs = new DistanceArg
		{
			Availability = InventorySettings.Availability.AllKnownBlueprints
		};
		columnDividerVertical2 = new Image(Interface.gui);
		filterAndTrackingPanel.AddContent(columnDividerVertical2);
		columnDividerVertical2.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		columnDividerVertical2.X = 438;
		columnDividerVertical2.Y = 6;
		columnDividerVertical2.Width = 2;
		columnDividerVertical2.Height = tbExpand.Bottom - 10;
		columnDividerVertical2.ScaleImageToSizeOfControl = true;
		btIncludeSalvage = new ImageButton(Interface.gui);
		filterAndTrackingPanel.AddContent(btIncludeSalvage);
		btIncludeSalvage.InitWithIcon(ImageButtonType.LCD, "HUD_icon_recycleArrows", hasCheckedState: true);
		btIncludeSalvage.Click += chIncludeSalvage_Click;
		btIncludeSalvage.Width = 30;
		btIncludeSalvage.X = 450;
		btIncludeSalvage.Y = rgDistance.Y;
		btIncludeSalvage.ToolTip = "Include salvageable materials from items and structures when determining what is ATTAINABLE / NOT ATTAINABLE to produce";
		btIncludeSalvage.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
		rowDivider1 = new Image(Interface.gui);
		filterAndTrackingPanel.AddContent(rowDivider1);
		rowDivider1.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		rowDivider1.X = 6;
		rowDivider1.Y = tbExpand.Bottom;
		rowDivider1.Width = filterAndTrackingPanel.ContentWidth - 12;
		rowDivider1.Height = 2;
		rowDivider1.ScaleImageToSizeOfControl = true;
		filterPropertiesPanel = new FilterPropertiesPanel(Interface.gui, makeRoomForExpandButton: false);
		filterPropertiesPanel.FiltersChanged += filterPropertiesPanel_FiltersChanged;
		filterAndTrackingPanel.AddContentSetFullWidth(filterPropertiesPanel);
		filterPropertiesPanel.Y = 36;
		rowDivider2 = new Image(Interface.gui);
		filterAndTrackingPanel.AddContent(rowDivider2);
		rowDivider2.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		rowDivider2.X = 6;
		rowDivider2.Y = filterPropertiesPanel.Bottom;
		rowDivider2.Width = filterAndTrackingPanel.ContentWidth - 12;
		rowDivider2.Height = 2;
		rowDivider2.ScaleImageToSizeOfControl = true;
		cbEntityTracking = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		filterAndTrackingPanel.AddContent(cbEntityTracking);
		cbEntityTracking.Init(ComboBoxTypes.LCD);
		cbEntityTracking.X = 4;
		cbEntityTracking.Y = 120;
		cbEntityTracking.Width = 185;
		PopulateEntityTrackingCombo();
		cbEntityTracking.SelectedIndex = 0;
		cbEntityTracking.SelectionChanged += cbTracking_SelectionChanged;
		cbEntityTracking.ToolTip = "Select a tracked object to view its options";
		The.InGameUI.InventorySettings.TrackTargetsChanged += InventorySettings_TrackTargetsChanged;
		cbInputChain = CreateCheckBox(190, 123, 80, "IN", "Track objects that are NEEDED TO CREATE this object", CheckBoxType.LCDTinting);
		cbOutputChain = CreateCheckBox(280, 123, 80, "OUT", "Track objects that can be CREATED FROM this object", CheckBoxType.LCDTinting);
		cbToolsOption = CreateCheckBox(370, 123, 80, "TOOLS", "Track TOOLS that can be USED TO CREATE this object", CheckBoxType.LCDTinting);
		ImageButton imageButton = new ImageButton(Interface.gui);
		filterAndTrackingPanel.AddContent(imageButton);
		imageButton.InitWithIcon(ImageButtonType.LCD, "HUD_icon_trash", hasCheckedState: false);
		imageButton.Click += tbStopTracking_Click;
		imageButton.ToolTip = "Stop tracking this object";
		imageButton.X = 460;
		imageButton.Y = 122;
		imageButton.Height = 30;
		imageButton.Width = 30;
		imageButton.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
		imageButton.RecalculateIconPosition();
	}

	private void rgDistance_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
	{
		The.InGameUI.InventorySettings.AvailabilitySettings = ((DistanceArg)((ImageButton)arg1).EventArgs).Availability;
		UpdateAllBlueprintsSetting();
		Populate();
	}

	private void UpdateAllBlueprintsSetting()
	{
		if (ibCyclopedia.IsChecked)
		{
			base.BackgroundTint = encyclopediaTint;
			btIncludeSalvage.IsChecked = false;
			btIncludeSalvage.Enabled = false;
		}
		else
		{
			base.BackgroundTint = null;
			btIncludeSalvage.Enabled = true;
		}
	}

	private void filterPropertiesPanel_FiltersChanged()
	{
		Refresh();
	}

	private void chIncludeSalvage_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.InventorySettings.IncludeSalvageProcesses = btIncludeSalvage.IsChecked;
		Populate();
	}

	private RadioButton CreateRadioButton(int x, int y, int width, string text, string tooltip, CheckBoxType checkBoxType, ClickHandler Click)
	{
		RadioButton radioButton = new RadioButton(Interface.gui);
		radioButton.Init(checkBoxType);
		radioButton.Click += Click;
		radioButton.X = x;
		radioButton.Y = y;
		radioButton.Text = text;
		radioButton.ToolTip = tooltip;
		radioButton.Width = width;
		return radioButton;
	}

	private CheckBox CreateCheckBox(int x, int y, int width, string text, string tooltip, CheckBoxType checkBoxType)
	{
		CheckBox checkBox = new CheckBox(Interface.gui);
		filterAndTrackingPanel.AddContent(checkBox);
		checkBox.Init(checkBoxType);
		checkBox.X = x;
		checkBox.Y = y;
		checkBox.Text = text;
		checkBox.ToolTip = tooltip;
		checkBox.Width = width;
		checkBox.Click += chbEntityTracking_Click;
		return checkBox;
	}

	public static TextButton CreateTextButton(GUIManager gui, int x, int y, int width, string text, ClickHandler tbSwitch_Click)
	{
		TextButton textButton = new TextButton(gui);
		textButton.Width = width;
		textButton.Init(TextButton.TextButtonType.LCD);
		textButton.Click += tbSwitch_Click;
		textButton.Y = y;
		textButton.Text = text;
		textButton.X = x;
		textButton.Width = width;
		textButton.Height = 30;
		return textButton;
	}

	public void PopulateEntityTrackingCombo()
	{
		TrackTarget trackTarget = (TrackTarget)cbEntityTracking.SelectedKey;
		cbEntityTracking.Clear();
		foreach (TrackTarget value in The.InGameUI.InventorySettings.TrackedTargets.Values)
		{
			if (!cbEntityTracking.EntriesByKey.ContainsKey(value))
			{
				cbEntityTracking.AddEntry(value, value.EntityType.Name);
				cbEntityTracking.EntriesByKey[value].NormalColor = value.Color;
			}
		}
		if (cbEntityTracking.EntriesByKey.Count > 0)
		{
			if (trackTarget != null && cbEntityTracking.EntriesByKey.ContainsKey(trackTarget))
			{
				cbEntityTracking.SelectedKey = trackTarget;
			}
			else
			{
				cbEntityTracking.SelectedIndex = 0;
			}
		}
	}

	private void LoadUserSettings()
	{
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			rbOr.IsChecked = The.InGameUI.InventorySettings.AndOrSetting == InventorySettings.AndOr.Or;
			rbAnd.IsChecked = The.InGameUI.InventorySettings.AndOrSetting == InventorySettings.AndOr.And;
			filterPropertiesPanel.Fill(The.InGameUI.InventorySettings.FilterPropertySettings);
			PopulateEntityTrackingCombo();
			cbEntityTracking.SelectedIndex = 0;
			TrackTarget firstTrackedTarget = The.InGameUI.InventorySettings.GetFirstTrackedTarget();
			if (firstTrackedTarget != null)
			{
				FillTrackedEntityTypeControls(firstTrackedTarget.ShowInputs, firstTrackedTarget.ShowOutputs, firstTrackedTarget.ShowTools, firstTrackedTarget.Color);
			}
			FillDistance(The.InGameUI.InventorySettings.AvailabilitySettings);
			btIncludeSalvage.IsChecked = The.InGameUI.InventorySettings.IncludeSalvageProcesses;
			haveActiveTracking = cbEntityTracking.Count != 0;
			ExpandOrCollapseTopPanel(The.InGameUI.InventorySettings.IsExpanded);
		}
		else
		{
			ExpandOrCollapseTopPanel(expand: false);
		}
		sortingButtons.Fill(The.InGameUI.InventorySettings.SortingSettings);
	}

	private void FillDistance(InventorySettings.Availability distance)
	{
		switch (distance)
		{
		case InventorySettings.Availability.AvailableNow:
			rgDistance.SelectMember(ibNormalDistance);
			break;
		case InventorySettings.Availability.Attainable:
			rgDistance.SelectMember(ibAttainableDistance);
			break;
		case InventorySettings.Availability.AllKnownBlueprints:
			rgDistance.SelectMember(ibCyclopedia);
			break;
		}
		UpdateAllBlueprintsSetting();
	}

	private void cbTracking_SelectionChanged(UIComponent sender)
	{
		TrackTarget trackTarget = (TrackTarget)cbEntityTracking.SelectedKey;
		FillTrackedEntityTypeControls(trackTarget.ShowInputs, trackTarget.ShowOutputs, trackTarget.ShowTools, trackTarget.Color);
	}

	private void FillTrackedEntityTypeControls(bool showinputs, bool showoutputs, bool showtools, Color backColor)
	{
		cbInputChain.IsChecked = showinputs;
		cbOutputChain.IsChecked = showoutputs;
		cbToolsOption.IsChecked = showtools;
		cbInputChain.BackColor = backColor;
		cbOutputChain.BackColor = backColor;
		cbToolsOption.BackColor = backColor;
	}

	private void RefreshTopPanelAfterTrackingChange()
	{
		haveActiveTracking = cbEntityTracking.Count != 0;
		ExpandOrCollapseTopPanel(The.InGameUI.InventorySettings.IsExpanded);
		Refresh();
	}

	private void HideOrDisplayTrackingOptions()
	{
		if (haveActiveTracking && cbEntityTracking.Count == 0)
		{
			haveActiveTracking = false;
			UpdateGridYPosition(112);
			rowDivider2.Visible = false;
		}
		if (!haveActiveTracking && cbEntityTracking.Count > 0)
		{
			haveActiveTracking = true;
			UpdateGridYPosition(150);
			rowDivider2.Visible = true;
		}
		ExpandOrCollapseTopPanel(!The.InGameUI.InventorySettings.IsExpanded);
	}

	private void UpdateGridYPosition(int filterPanelHeight)
	{
		int y;
		if (filterAndTrackingPanel != null)
		{
			filterAndTrackingPanel.ContentHeight = filterPanelHeight;
			y = filterAndTrackingPanel.Panel.Bottom + 2;
		}
		else
		{
			y = 6;
		}
		sortingButtonsContainer.Y = y;
		grdListView.Y = sortingButtonsContainer.Bottom - 15;
		grdCategoryView.Y = sortingButtonsContainer.Bottom - 15;
		grdListView.Height = lcdSurface.Height - sortingButtonsContainer.Bottom + 15;
		grdCategoryView.Height = lcdSurface.Height - sortingButtonsContainer.Bottom + 15;
	}

	private void ExpandOrCollapseTopPanel(bool expand)
	{
		if (expand)
		{
			if (haveActiveTracking)
			{
				SetMinimizedOrExpandedContentProperties(150, 1, visible: true, "LESS", "Hide search options");
			}
			else
			{
				SetMinimizedOrExpandedContentProperties(112, 1, visible: true, "LESS", "Hide search options");
			}
		}
		else
		{
			SetMinimizedOrExpandedContentProperties(34, 0, visible: false, "MORE", "Display search options");
		}
		The.InGameUI.InventorySettings.IsExpanded = expand;
	}

	private void SetMinimizedOrExpandedContentProperties(int panelheight, int currentSkin, bool visible, string text, string btToolTip)
	{
		UpdateGridYPosition(panelheight);
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			tbExpand.ToolTip = btToolTip;
			tbExpand.Text = text;
			rowDivider1.Visible = visible;
			rbAnd.Visible = visible;
			rbOr.Visible = visible;
			rgDistance.Visible = visible;
			columnDividerVertical1.Visible = visible;
			btIncludeSalvage.Visible = visible;
			if (visible)
			{
				filterAndTrackingPanel.AddContentSetFullWidth(filterPropertiesPanel);
			}
			else
			{
				filterAndTrackingPanel.RemoveContent(filterPropertiesPanel);
			}
		}
	}

	private void Expand_Click(UIComponent sender, EventArgs e)
	{
		ExpandOrCollapseTopPanel(!The.InGameUI.InventorySettings.IsExpanded);
	}

	private void rgOrAnd_Click(UIComponent sender, EventArgs e)
	{
		RadioButton radioButton = (RadioButton)sender;
		if (The.InGameUI.InventorySettings.AndOrSetting != (InventorySettings.AndOr)radioButton.Tag1)
		{
			The.InGameUI.InventorySettings.AndOrSetting = (InventorySettings.AndOr)radioButton.Tag1;
			Refresh();
		}
	}

	private void chbEntityTracking_Click(UIComponent sender, EventArgs e)
	{
		_ = (CheckBox)sender;
		((TrackTarget)cbEntityTracking.SelectedKey)?.SetTrackingOptions(cbInputChain.IsChecked, cbOutputChain.IsChecked, cbToolsOption.IsChecked);
	}

	public void btTrack_Click(UIComponent sender, EventArgs e)
	{
		ImageButton imageButton = (ImageButton)sender;
		EntityType entityType = (EntityType)imageButton.Parent.Parent.Tag1;
		The.InGameUI.InventorySettings.ToggleTracking(entityType);
		_ = imageButton.Parent;
	}

	private void tbStopTracking_Click(UIComponent sender, EventArgs e)
	{
		TrackTarget trackTarget = (TrackTarget)cbEntityTracking.SelectedKey;
		if (!The.InGameUI.InventorySettings.ToggleTracking(trackTarget.EntityType))
		{
			ActOnRow(trackTarget.EntityType, HideTrackingButton);
		}
	}

	private void ActOnRow(EntityType entityType, Action<UIComponent> action)
	{
		if (grdListView.TryGetEntry(entityType, out var item))
		{
			action(item);
		}
		object categoryKey = GetCategoryKey(entityType);
		if (grdCategoryView.TryGetEntry(categoryKey, out var item2) && ((item2 as CollapsablePanel).ExpandedPanel.Controls[0] as Grid).TryGetEntry(entityType, out item))
		{
			action(item);
		}
	}

	private static object GetCategoryKey(EntityType entityType)
	{
		return entityType.Category;
	}

	private static void HideTrackingButton(UIComponent row)
	{
		((ImageButton)row.FindChildById(UIComponent.DataControlID.Track).Controls[0]).Visible = false;
	}

	private void SetView(ViewType viewTypeToSet)
	{
		viewType = viewTypeToSet;
		ShowRelevantGrid();
		if (viewType == ViewType.List)
		{
			ibCategory.IsChecked = false;
			ibCategory.IsChecked = false;
		}
		else
		{
			ibCategory.IsChecked = true;
			ibList.IsChecked = false;
		}
	}

	private void tbListView_Click(UIComponent sender, EventArgs e)
	{
		if (viewType != ViewType.List)
		{
			SetView(ViewType.List);
			Populate();
		}
	}

	private void tbCategoryView_Click(UIComponent sender, EventArgs e)
	{
		if (viewType != ViewType.Categories)
		{
			SetView(ViewType.Categories);
			Populate();
		}
	}

	private void tbSort_Click()
	{
		Populate();
	}

	public static void DoCategorySorting(Grid outerGrid, List<Grid> categoryGrids, Grid.Sorting itemSortOrder)
	{
		outerGrid.Sort((UIComponent i) => i.OrderByTag1, Grid.Sorting.Ascending);
		foreach (Grid categoryGrid in categoryGrids)
		{
			if (categoryGrid.Entries.Count > 0)
			{
				categoryGrid.Sort((UIComponent i) => i.OrderByTag1, itemSortOrder);
			}
			categoryGrid.EndAddingEntries();
		}
	}

	public override void Refresh()
	{
		Populate();
		base.Refresh();
	}

	public override void Show()
	{
		base.Show();
	}

	private void InventorySettings_TrackTargetsChanged()
	{
		PopulateEntityTrackingCombo();
		RefreshTopPanelAfterTrackingChange();
		Refresh();
	}

	public static int GetNoOfAvailableEntities(Dictionary<EntityType, List<EntityID>> allEntities, EntityGroup owner, EntityType entityType, out int noOfIncompleteEntities, out int noOfEntitiesUsedAsParts, out int noOfItemsOnOtherSite, out int noOfItemsOwnedByOthers, out int noOfAvailableItemsIncludingIntrinsic, Dictionary<EntityType, Availability> allAvailableEntities = null, bool countPartsOfEntities = false, bool ownedByOtherAllegiance = false, bool doCacheLookup = true)
	{
		List<EntityID> listOfAllEntities;
		List<EntityID> listOfAvailableEntities;
		List<EntityID> listOfUnavailableEntities;
		return GetNoOfAvailableEntities(allEntities, owner, entityType, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic, out listOfAllEntities, out listOfAvailableEntities, out listOfUnavailableEntities, allAvailableEntities, countPartsOfEntities, ownedByOtherAllegiance, doCacheLookup);
	}

	public static int GetNoOfAvailableEntities(Dictionary<EntityType, List<EntityID>> allEntities, EntityGroup owner, EntityType entityType, out int noOfIncompleteEntities, out int noOfEntitiesUsedAsParts, out int noOfItemsOnOtherSite, out int noOfItemsOwnedByOthers, out int noOfAvailableItemsIncludingIntrinsic, out List<EntityID> listOfAllEntities, out List<EntityID> listOfAvailableEntities, out List<EntityID> listOfUnavailableEntities, Dictionary<EntityType, Availability> allAvailableEntities = null, bool countPartsOfEntities = false, bool ownedByOtherAllegiance = false, bool doCacheLookup = true)
	{
		noOfIncompleteEntities = 0;
		noOfEntitiesUsedAsParts = 0;
		noOfItemsOnOtherSite = 0;
		noOfItemsOwnedByOthers = 0;
		noOfAvailableItemsIncludingIntrinsic = 0;
		listOfAllEntities = null;
		listOfAvailableEntities = null;
		listOfUnavailableEntities = null;
		OwnerID? ownerID = owner.GetOwnerID();
		if (allEntities == null)
		{
			return 0;
		}
		int noOfAvailableEntities = 0;
		noOfAvailableItemsIncludingIntrinsic = 0;
		if (doCacheLookup && allAvailableEntities != null && allAvailableEntities.TryGetValue(entityType, out var value))
		{
			noOfEntitiesUsedAsParts = value.NoOfEntitiesUsedAsParts;
			noOfIncompleteEntities = value.NoOfIncompleteEntities;
			noOfItemsOnOtherSite = value.NoOfEntitiesOffSite;
			noOfItemsOwnedByOthers = value.NoOfEntitiesOwnedByOthers;
			noOfAvailableItemsIncludingIntrinsic = value.NoOfAvailableItemsIncludingIntrinsic;
			listOfAllEntities = value.AllEntities;
			listOfAvailableEntities = value.AvailableEntities;
			listOfUnavailableEntities = value.UnavailableEntities;
			return value.NoOfAvailableItems;
		}
		if (!allEntities.TryGetValue(entityType, out listOfAllEntities))
		{
			noOfAvailableEntities = 0;
		}
		else
		{
			listOfAvailableEntities = new List<EntityID>();
			listOfUnavailableEntities = new List<EntityID>();
			SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
			for (int num = listOfAllEntities.Count - 1; num >= 0; num--)
			{
				EntityID entityID = listOfAllEntities[num];
				if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out var entityData))
				{
					EntityGroup.CountEntity(ownerID, entityData, ref noOfIncompleteEntities, ref noOfEntitiesUsedAsParts, ref noOfItemsOnOtherSite, ref noOfItemsOwnedByOthers, ref noOfAvailableEntities, ref noOfAvailableItemsIncludingIntrinsic, ref listOfAvailableEntities, ref listOfUnavailableEntities);
				}
			}
		}
		if (allAvailableEntities != null)
		{
			if (!allAvailableEntities.TryGetValue(entityType, out var value2))
			{
				value2 = (allAvailableEntities[entityType] = new Availability());
			}
			if (ownedByOtherAllegiance)
			{
				value2.AvailableToBuy += noOfAvailableEntities;
			}
			else
			{
				value2.NoOfAvailableItems += noOfAvailableEntities;
				value2.NoOfAvailableItemsIncludingIntrinsic += noOfAvailableItemsIncludingIntrinsic;
				value2.NoOfIncompleteEntities += noOfIncompleteEntities;
				value2.NoOfEntitiesUsedAsParts += noOfEntitiesUsedAsParts;
				value2.NoOfEntitiesOwnedByOthers += noOfItemsOwnedByOthers;
				value2.NoOfEntitiesOffSite += noOfItemsOnOtherSite;
			}
			if (listOfAllEntities != null)
			{
				if (value2.AllEntities == null)
				{
					value2.AllEntities = new List<EntityID>();
				}
				value2.AllEntities.AddRange(listOfAllEntities);
			}
			if (listOfAvailableEntities != null)
			{
				if (value2.AvailableEntities == null)
				{
					value2.AvailableEntities = new List<EntityID>();
				}
				value2.AvailableEntities.AddRange(listOfAvailableEntities);
			}
			if (listOfUnavailableEntities != null)
			{
				if (value2.UnavailableEntities == null)
				{
					value2.UnavailableEntities = new List<EntityID>();
				}
				value2.UnavailableEntities.AddRange(listOfUnavailableEntities);
			}
		}
		return noOfAvailableEntities;
	}

	private void Populate()
	{
		Expedition firstPlayerExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
		if (firstPlayerExpedition == null)
		{
			return;
		}
		EntityGroup ownedEntities = firstPlayerExpedition.OwnedEntities;
		CountAllEntities(ownedEntities, allAvailableItems, countItemsToBuy: false);
		switch (viewType)
		{
		case ViewType.Categories:
			PopulateWithCategories(ownedEntities);
			if (grdCategoryView.Count > 0 && grdListView.Count == 0)
			{
				PopulateWithList(ownedEntities);
			}
			break;
		case ViewType.List:
			PopulateWithList(ownedEntities);
			if (grdListView.Count > 0 && grdCategoryView.Count == 0)
			{
				PopulateWithCategories(ownedEntities);
			}
			break;
		}
	}

	private void PopulateWithCategories(EntityGroup owner)
	{
		Grid categoryGrid = null;
		categoryGrids = new List<Grid>();
		currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);
		grdCategoryView.BeginAddingEntries();
		foreach (EntityType currentListDatum in currentListData)
		{
			if (currentListDatum.StructureType == null && currentListDatum.ItemType == null)
			{
				continue;
			}
			object categoryKey = GetCategoryKey(currentListDatum);
			CollapsablePanel cpCategory = null;
			if (grdCategoryView.TryGetEntry(categoryKey, out var item))
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
				AddCategoryRow(ref cpCategory, ref categoryGrid, categoryKey);
				categoryGrid.BeginAddingEntries();
				categoryGrids.Add(categoryGrid);
			}
			if (!categoryGrid.TryGetEntry(currentListDatum, out var item2))
			{
				item2 = AddItemRow(categoryGrid, currentListDatum, owner, useCurrentUIOwner: false, gridList: false);
			}
			UpdateItemRow(item2, currentListDatum, owner);
		}
		foreach (CollapsablePanel entry in grdCategoryView.Entries)
		{
			((Grid)entry.ExpandedPanel.Controls[0]).DeleteEntries((EntityType j) => currentListData.Contains(j));
		}
		List<object> list = new List<object>();
		foreach (object key in grdCategoryView.EntriesByKey.Keys)
		{
			if (((Grid)(grdCategoryView.EntriesByKey[key] as CollapsablePanel).ExpandedPanel.Controls[0]).Entries.Count == 0)
			{
				list.Add(key);
			}
		}
		foreach (object item3 in list)
		{
			grdCategoryView.RemoveEntry(item3);
		}
		DoCategorySorting(grdCategoryView, categoryGrids, The.InGameUI.InventorySettings.SortingSettings.SortOrder);
		grdCategoryView.EndAddingEntries();
	}

	private void PopulateWithList(EntityGroup owner)
	{
		currentListData = The.InGameUI.InventorySettings.GetData(allAvailableItems);
		grdListView.BeginAddingEntries();
		foreach (EntityType currentListDatum in currentListData)
		{
			currentListDatum.Name.Contains("Bellows");
			if (!grdListView.TryGetEntry(currentListDatum, out var item))
			{
				item = AddItemRow(grdListView, currentListDatum, owner, useCurrentUIOwner: false, gridList: true);
			}
			UpdateItemRow(item, currentListDatum, owner);
		}
		grdListView.DeleteEntries((EntityType j) => currentListData.Contains(j));
		grdListView.Sort((UIComponent i) => i.OrderByTag1, The.InGameUI.InventorySettings.SortingSettings.SortOrder);
		grdListView.EndAddingEntries();
	}

	public static void CountAllEntities(EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool countItemsToBuy)
	{
		allAvailableItems.Clear();
		if (countItemsToBuy && The.Sim.World.Relations.TryGetValue(The.InGameUI.UIAllegiance.ID, out var value))
		{
			foreach (AllegianceRelation item in value)
			{
				if (!item.AllowTrade)
				{
					continue;
				}
				foreach (Expedition expedition in item.AllegianceB.Expeditions)
				{
					foreach (IKnownEntityData workingTerminal in expedition.GetWorkingTerminals(The.InGameUI.UIAllegiance.SharedKnowledge, null))
					{
						CountAllEntities(workingTerminal.OfferedEntitiesByType, null, allAvailableItems, isOwnedByOtherAllegiance: true);
					}
				}
			}
		}
		CountAllEntities(owner.AllEntities, owner, allAvailableItems, isOwnedByOtherAllegiance: false);
	}

	private static void CountAllEntities(Dictionary<EntityType, List<EntityID>> itemsToCount, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool isOwnedByOtherAllegiance)
	{
		foreach (KeyValuePair<EntityType, List<EntityID>> item in itemsToCount)
		{
			EntityType key = item.Key;
			CountItems(itemsToCount, owner, allAvailableItems, isOwnedByOtherAllegiance, key);
		}
	}

	public static void CountItems(Dictionary<EntityType, List<EntityID>> itemsToCount, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems, bool isOwnedByOtherAllegiance, EntityType entityType)
	{
		GetNoOfAvailableEntities(itemsToCount, owner, entityType, out var _, out var _, out var _, out var _, out var _, allAvailableItems, countPartsOfEntities: false, isOwnedByOtherAllegiance, doCacheLookup: false);
	}

	private void UpdateItemRow(UIComponent itemRow, EntityType entityType, EntityGroup owner)
	{
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			UpdateItemRowTracking(itemRow, entityType);
		}
		((ProductionOrderControl)itemRow.FindChildById(UIComponent.DataControlID.Orders)).UpdateOrders(owner, allAvailableItems, out var noOfAvailableItems);
		((DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableItems > 0);
	}

	private static void UpdateItemRowTracking(UIComponent itemRow, EntityType entityType)
	{
		The.InGameUI.InventorySettings.TrackedTargets.TryGetValue(entityType, out var value);
		Bar bar = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);
		ImageButton imageButton = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.Track).Controls[0];
		Color? color = null;
		string toolTip = null;
		if (The.InGameUI.InventorySettings.GetTrackedColorAndTooltip(entityType, out color, out toolTip))
		{
			bar.Visible = true;
			bar.SetSkinLocation(SkinState.Normal, null, color, color);
			Color value2 = new Color(color.Value.R - 40, color.Value.G - 40, color.Value.B - 40);
			bar.SetSkinLocation(SkinState.Hover, null, value2, value2);
			bar.ToolTip = toolTip;
			if (value != null)
			{
				imageButton.IsChecked = true;
				imageButton.Visible = true;
			}
		}
		else
		{
			imageButton.IsChecked = false;
			bar.Visible = false;
		}
		if (The.InGameUI.InventorySettings.SortingSettings.SortedBy == InventorySettings.SortColumns.Tracking)
		{
			itemRow.OrderByTag1 = (bar.Visible ? 1 : 2);
			itemRow.OrderByTag1 = ((value == null) ? itemRow.OrderByTag1 : ((object)0));
		}
	}

	private void InventoryPanel_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (viewType == ViewType.List)
		{
			SetTrackingVisibility(grdListView, args);
			return;
		}
		foreach (Grid categoryGrid in categoryGrids)
		{
			SetTrackingVisibility(categoryGrid, args);
		}
	}

	private void SetTrackingVisibility(Grid grid, MouseEventArgs args)
	{
		foreach (UIComponent entry in grid.Entries)
		{
			ShowHideTrackingButton(args, entry);
		}
	}

	private static void ShowHideTrackingButton(UIComponent sender, bool show)
	{
		UIComponent parent = sender.Parent;
		ImageButton imageButton = (ImageButton)sender.Controls[0];
		if (!The.InGameUI.InventorySettings.TrackedTargets.TryGetValue((EntityType)parent.Tag1, out var _))
		{
			imageButton.Visible = show;
			if (The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
			{
				imageButton.ToolTip = "Track this item/structure";
				imageButton.Enabled = true;
			}
			else
			{
				imageButton.ToolTip = "No more objects can be tracked, cancel some of the other tracked objects first.";
				imageButton.Enabled = false;
			}
		}
	}

	private static void ShowHideTrackingButton(MouseEventArgs args, UIComponent row)
	{
		ImageButton imageButton = (ImageButton)row.FindChildById(UIComponent.DataControlID.Track);
		if (row.AbsolutePosition.Y < args.Position.Y && row.AbsolutePosition.Y + row.Height > args.Position.Y)
		{
			if (row.AbsolutePosition.X < args.Position.X && row.AbsolutePosition.X + row.Width > args.Position.X)
			{
				imageButton.Visible = true;
			}
			else
			{
				imageButton.Visible = false;
			}
		}
		else
		{
			imageButton.Visible = false;
		}
		if (!The.InGameUI.InventorySettings.TrackedTargets.TryGetValue((EntityType)row.Tag1, out var _))
		{
			if (The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
			{
				imageButton.ToolTip = "Track this item/structure";
				imageButton.Enabled = true;
			}
			else
			{
				imageButton.ToolTip = "No more objects can be tracked, cancel some of the other tracked objects first.";
				imageButton.Enabled = false;
			}
		}
		else
		{
			imageButton.Enabled = true;
			imageButton.Visible = true;
		}
	}

	public static bool CanBuildNow(EntityType entityType, EntityGroup owner, bool includeSalvageProcesses)
	{
		GetBestProcessForDisplay(entityType, owner, out var _, out var _, out var maxAmountThatCanBeProduced, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, includeSalvageProcesses, null);
		if (maxAmountThatCanBeProduced > 0)
		{
			return true;
		}
		return false;
	}

	public static CollapsablePanel.SubType GetPanelSubType(EntityCategory category)
	{
		return category.CategoryColor switch
		{
			EntityCategory.CategoryColors.Blue => CollapsablePanel.SubType.Blue, 
			EntityCategory.CategoryColors.Green => CollapsablePanel.SubType.Green, 
			EntityCategory.CategoryColors.Red => CollapsablePanel.SubType.Red, 
			_ => CollapsablePanel.SubType.Normal, 
		};
	}

	private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, object key)
	{
		EntityCategory entityCategory = key as EntityCategory;
		cpCategory = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
		cpCategory.HeadingYPos = 4;
		cpCategory.CollapsedHeight = 28;
		grdCategoryView.AddEntry(key, cpCategory);
		cpCategory.OrderByTag1 = entityCategory.SortOrder;
		cpCategory.Init(GetPanelSubType(entityCategory));
		cpCategory.Title = entityCategory.Name;
		cpCategory.Width = grdCategoryView.Width;
		if (entityCategory.DisplayStructureIcon)
		{
			Image image = new Image(Interface.gui);
			image.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_structure"));
			image.ResizeControlToFitImage();
			image.X = 8;
			image.ID = UIComponent.DataControlID.StatusIcon;
			image.Visible = true;
			cpCategory.CenterOnHeader(image);
			image.Y += 2;
			cpCategory.TitlePositionX = image.Right + 6 - 2;
			cpCategory.Add(image);
		}
		categoryGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
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

	private int GetMaxProduction(EntityType entityType)
	{
		return 5;
	}

	private UIComponent AddItemRow(Grid grid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, bool gridList)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		uIComponent.DebugTag = "stocksItem";
		grid.AddEntry(entityType, uIComponent);
		uIComponent.CanHaveFocus = true;
		Bar bar = new Bar(Interface.gui);
		bar.ID = UIComponent.DataControlID.Background;
		bar.X = 0;
		bar.Width = 300;
		bar.EdgeSize = 23;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_highlightBar_white");
		bar.SetSkinLocation(SkinState.Normal, sourceRectangle);
		bar.SetSkinLocation(SkinState.Hover, sourceRectangle);
		bar.Height = sourceRectangle.Height;
		bar.Visible = false;
		uIComponent.Add(bar);
		uIComponent.CenterChildVertically(bar);
		Image image = AddEntityTypeIcon(entityType, uIComponent, itemTypeIconColumnX);
		if (entityType.StructureType != null)
		{
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
		}
		else if (entityType.TreeType != null)
		{
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
		}
		else if (entityType.BiologicalType != null)
		{
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
		}
		else if (entityType.TerrainType != null && entityType.TerrainType.IsSpecialInterestFeature)
		{
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
		}
		DataTypeButton dataTypeButton = new DataTypeButton(Interface.gui, DataSheet.InfoToShow.Production, entityType, GoalEvaluator.GetOwnerID(owner), useCurrentUIOwner);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.IsRoot = true;
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = availableX - captionX - 6;
		dataTypeButton.X = captionX;
		uIComponent.CenterChildVertically(dataTypeButton);
		ProductionTargetEventArgs eventArgs = new ProductionTargetEventArgs(entityType, null);
		ProductionOrderControl productionOrderControl = new ProductionOrderControl(entityType, eventArgs, uIComponent.guiManager, ProductionOrderControl.UILayout.LCD, 83, tbItems_Click, InventoryPanelPadlock_Click);
		uIComponent.Add(productionOrderControl);
		productionOrderControl.X = availableX;
		productionOrderControl.ID = UIComponent.DataControlID.Orders;
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			uIComponent.DebugTag = "itemRow";
			UIComponent uIComponent2 = new UIComponent(Interface.gui);
			uIComponent.Add(uIComponent2);
			uIComponent2.Position = new Point(itemTypeProductionTargetColumnX + 125, 0);
			uIComponent2.Width = 60;
			uIComponent2.Height = uIComponent.Height;
			uIComponent2.MouseOver += trackingHotspot_MouseOver;
			uIComponent2.MouseOut += trackingHotspot_MouseOut;
			uIComponent2.EventArgs = eventArgs;
			uIComponent2.ID = UIComponent.DataControlID.Track;
			ImageButton imageButton = new ImageButton(Interface.gui);
			uIComponent2.Add(imageButton);
			imageButton.InitWithIcon(ImageButtonType.LCD, "basic_icon_crosshairs", hasCheckedState: true);
			imageButton.Position = new Point(20, 0);
			imageButton.EventArgs = eventArgs;
			imageButton.Click += btTrack_Click;
			imageButton.Visible = false;
			imageButton.ToolTip = "Track this item/structure";
			imageButton.Width = 36;
			uIComponent.CenterChildVertically(imageButton);
			imageButton.MouseOut += tbTracking_MouseOut;
			if (!gridList)
			{
				imageButton.X -= 11;
			}
		}
		return uIComponent;
	}

	private void InventoryPanelPadlock_Click(UIComponent sender, EventArgs e)
	{
		Expedition expedition = The.InGameUI.GetExpedition();
		if (expedition != null)
		{
			ProductionOrderControl.Padlock_Click(sender, e);
			ProductionTargetEventArgs e2 = e as ProductionTargetEventArgs;
			EntityGroup ownedEntities = expedition.OwnedEntities;
			UpdateItemRow(sender.Parent.Parent, e2.Item, ownedEntities);
		}
	}

	private void tbTracking_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		UIComponent parent = sender.Parent;
		if (!((ImageButton)sender).IsChecked && !parent.CheckCoordinates(args.Position.X, args.Position.Y))
		{
			ShowHideTrackingButton(parent, show: false);
		}
	}

	private void trackingHotspot_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		ShowHideTrackingButton(sender, show: false);
	}

	private void trackingHotspot_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		ShowHideTrackingButton(sender, show: true);
	}

	public static Image AddEntityTypeIcon(EntityType entityType, UIComponent item, int? xPosToCenterAbout = null)
	{
		Image image = new Image(item.guiManager);
		IconInfo iconInfo;
		Rectangle iconSprite = entityType.GetIconSprite(out iconInfo);
		image.SetSkinLocation(SkinState.Normal, iconSprite);
		image.Texture = item.guiManager.GUISpriteSheet.Texture;
		item.Add(image);
		image.ResizeControlToFitImage();
		item.CenterChildVertically(image, iconInfo?.CenterYPos);
		if (xPosToCenterAbout.HasValue)
		{
			item.CenterHorizontally(xPosToCenterAbout.Value, image);
		}
		return image;
	}

	public void OnSetProduction(string entityTypeKey)
	{
		EntityType entityType = GameData.Instance.AllEntityTypes[entityTypeKey];
		ActOnRow(entityType, ResetSliderBeingDragged);
		Populate();
	}

	private void ResetSliderBeingDragged(UIComponent row)
	{
		((ProductionOrderControl)row.FindChildById(UIComponent.DataControlID.Orders))?.ResetSliderBeingDragged();
	}

	public static bool OwnsProductOrHasProcessInputsAndTools(EntityType entityType, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems = null, bool countPartsOfEntities = false)
	{
		OwnsProductOrHasProcessInputsAndTools(entityType, owner, out var ownsItem, out var _, out var hasInputs, out var _, allAvailableItems, countPartsOfEntities);
		return ownsItem || hasInputs;
	}

	public static void OwnsProductOrHasProcessInputsAndTools(EntityType entityType, EntityGroup owner, out bool ownsItem, out bool ownsItemIncludingIntrinsicPart, out bool hasInputs, out bool hasTools, Dictionary<EntityType, Availability> allAvailableItems = null, bool countPartsOfEntities = false)
	{
		hasInputs = false;
		hasTools = false;
		if (owner == null)
		{
			ownsItem = false;
			ownsItemIncludingIntrinsicPart = false;
			return;
		}
		if (GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out var noOfIncompleteEntities, out var _, out var _, out var _, out var noOfAvailableItemsIncludingIntrinsic, allAvailableItems, countPartsOfEntities) != 0 || noOfIncompleteEntities != 0)
		{
			ownsItem = true;
			ownsItemIncludingIntrinsicPart = true;
			return;
		}
		if (noOfAvailableItemsIncludingIntrinsic > 0)
		{
			ownsItemIncludingIntrinsicPart = true;
		}
		else
		{
			ownsItemIncludingIntrinsicPart = false;
		}
		ownsItem = false;
		GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, The.InGameUI.InventorySettings.IncludeSalvageProcesses, null, allAvailableItems);
	}

	private static bool HandleProcess(EntityType entityType, ProcessType processType, EntityGroup owner, out bool hasInputs, out bool hasTools, out int maxAmountThatCanBeProduced, out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out bool hasSkills, out bool hasResources, out bool hasSpecialSite, out bool hasPolicy, out EntityType needsImmovableInput, out int? outputBatchAmount, Dictionary<EntityType, Availability> allAvailableItems = null)
	{
		outputBatchAmount = processType.GetOutputAmount(entityType);
		return HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, allAvailableItems);
	}

	public static bool GetBestProcessForDisplay(EntityType entityType, EntityGroup owner, out bool hasInputs, out bool hasTools, out int maxAmountThatCanBeProduced, out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out bool hasSkills, out bool hasResources, out bool hasSpecialSite, out bool hasPolicy, out EntityType needsImmovableInput, out int? outputBatchAmount, out ProcessType processType, bool includeSalvageProcesses, Predicate<ProcessType> guiPreferProcess, Dictionary<EntityType, Availability> allAvailableItems = null, Predicate<ProcessType> allowProcess = null)
	{
		GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value);
		if (includeSalvageProcesses && GameData.Instance.SalvageProcessYieldsThisOutput.TryGetValue(entityType, out var value2))
		{
			if (value != null)
			{
				value = new List<ProcessType>(value);
				value.AddRange(value2);
			}
			else
			{
				value = value2;
			}
		}
		if (value != null)
		{
			processType = GetBestProcessForDisplay(entityType, value, guiPreferProcess, allowProcess, owner, allAvailableItems);
			if (processType != null)
			{
				bool handled = HandleProcess(entityType, processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, allAvailableItems);
				maxAmountThatCanBeProduced += MaxFromOtherRoutes(entityType, value, processType, owner, allowProcess, allAvailableItems);
				return handled;
			}
		}
		hasSkills = false;
		hasInputs = false;
		hasTools = false;
		hasResources = false;
		hasSpecialSite = false;
		hasPolicy = false;
		needsImmovableInput = null;
		maxAmountThatCanBeProduced = 0;
		noOfMissingInputTypes = null;
		noOfAvailableInputTypes = null;
		outputBatchAmount = null;
		processType = null;
		return false;
	}

	/// <summary>
	/// How many more of this item the OTHER recipes could make, on top of what the displayed one
	/// can.
	///
	/// Nothing in the stock game needed this: no item had two ways to make it, so the amount the
	/// chosen recipe could produce was the amount, full stop. The mod's alternative recipes broke
	/// that quietly - "10 dry peat, so 5 charcoal" while 14 firewood sat in the store, because the
	/// firewood route was not the one being displayed. Reported by Kastuk, with a screenshot of a
	/// slider that stopped at 5.
	///
	/// ONLY ROUTES THAT DO NOT COMPETE ARE ADDED. Two recipes drawing on the same material cannot
	/// both have their full amount: 10 sticks is 10 arrows OR 10 handles, not 20 of each. So a
	/// route is counted only when every item it consumes is untouched by the routes already
	/// counted - which is exactly true of the peat and firewood charcoal routes, and false of
	/// anything sharing a material. Under-counting a competing route is the conservative error:
	/// it can only ever say you can make fewer than you can, never more.
	/// </summary>
	private static int MaxFromOtherRoutes(EntityType entityType, List<ProcessType> processes, ProcessType shown, EntityGroup owner, Predicate<ProcessType> allowProcess, Dictionary<EntityType, Availability> allAvailableItems)
	{
		if (processes.Count < 2)
		{
			return 0;
		}

		HashSet<EntityType> claimed = ConsumedInputs(shown);
		int extra = 0;
		foreach (ProcessType candidate in processes)
		{
			if (candidate == shown || (allowProcess != null && !allowProcess(candidate)))
			{
				continue;
			}
			HashSet<EntityType> needs = ConsumedInputs(candidate);
			if (needs.Overlaps(claimed))
			{
				continue;
			}
			if (!HandleProcess(entityType, candidate, owner, out var _, out var _, out int amount, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, allAvailableItems))
			{
				continue;
			}
			if (amount > 0)
			{
				extra += amount;
				claimed.UnionWith(needs);
			}
		}
		return extra;
	}

	/// <summary>
	/// The item types a recipe uses up. Inputs it needs but does not consume - a tool, the stones
	/// a campfire is laid on - are excluded: they are still there afterwards, so two recipes both
	/// naming one are not competing for it.
	/// </summary>
	private static HashSet<EntityType> ConsumedInputs(ProcessType process)
	{
		HashSet<EntityType> consumed = new HashSet<EntityType>();
		if (process?.Inputs == null)
		{
			return consumed;
		}
		Input[] inputs = process.Inputs;
		foreach (Input input in inputs)
		{
			if (input.IsConsumed && input.EntityType != null)
			{
				consumed.Add(input.EntityType);
			}
		}
		return consumed;
	}

	private static ProcessType GetBestProcessForDisplay(EntityType entityType, List<ProcessType> processes, Predicate<ProcessType> guiPrefersProcess, Predicate<ProcessType> allowProcess, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems)
	{
		float num = 0f;
		ProcessType result = null;
		foreach (ProcessType process in processes)
		{
			float num2 = 0f;
			if (allowProcess != null && !allowProcess(process))
			{
				continue;
			}
			if (guiPrefersProcess == null || guiPrefersProcess(process))
			{
				num2 += 2f;
				if (OrdersExist(owner, entityType, process))
				{
					num2 += 10f;
				}
			}
			if (HasAllInputsAndToolsForProcess(process, owner, allAvailableItems))
			{
				num2 += 5f;
			}
			if (num2 >= num)
			{
				result = process;
				num = num2;
			}
		}
		return result;
	}

	private static bool OrdersExist(EntityGroup owner, EntityType entityType, ProcessType process)
	{
		if (!owner.ProductionOrders.OrdersExist(entityType) && owner.ProductionJobs.TryGetValue(entityType, out var value))
		{
			return value.Any((ProcessJob p) => p.ProcessType == process);
		}
		return false;
	}

	private static ProcessType SelectProcessTypeForOutput(List<ProcessType> listOfProcesses)
	{
		if (listOfProcesses.Count == 1)
		{
			return listOfProcesses[0];
		}
		ProcessType processType = listOfProcesses.FirstOrDefault((ProcessType p) => p.IsPartOfProductionChainButCannotOrderFromInventory());
		if (processType == null)
		{
			return listOfProcesses[0];
		}
		return processType;
	}

	public override void Hide()
	{
		base.Hide();
		foreach (UIComponent entry in grdCategoryView.Entries)
		{
			ResetSliderBeingDragged(entry);
		}
		foreach (UIComponent entry2 in grdListView.Entries)
		{
			ResetSliderBeingDragged(entry2);
		}
	}

	public static bool HasInputForProcess(ProcessType process, EntityGroup owner, out bool hasInputs, out int maxAmountThatCanBeProduced, out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out int? noOfAvailableItems, EntityType typeToCheck)
	{
		maxAmountThatCanBeProduced = 0;
		hasInputs = true;
		int? currentProductionLimit = null;
		if (process.InputsByType != null)
		{
			noOfMissingInputTypes = 0;
			noOfAvailableInputTypes = 0;
			noOfAvailableItems = 0;
			HasInputForProcess(typeToCheck, process.InputsByType[typeToCheck], owner, process, ref hasInputs, ref noOfMissingInputTypes, ref noOfAvailableInputTypes, ref currentProductionLimit, ref noOfAvailableItems);
		}
		else
		{
			noOfAvailableItems = null;
			noOfMissingInputTypes = null;
			noOfAvailableInputTypes = null;
		}
		if (noOfMissingInputTypes.HasValue && noOfMissingInputTypes > 0)
		{
			maxAmountThatCanBeProduced = 0;
		}
		else if (currentProductionLimit.HasValue)
		{
			maxAmountThatCanBeProduced = currentProductionLimit.Value;
		}
		return hasInputs;
	}

	private static void HasInputForProcess(EntityType inputType, Input input, EntityGroup owner, ProcessType process, ref bool hasInputs, ref int? noOfMissingInputTypes, ref int? noOfAvailableInputTypes, ref int? currentProductionLimit, ref int? noOfAvailableItems, Dictionary<EntityType, Availability> allAvailableItems = null)
	{
		int num = 0;
		noOfAvailableItems = GetNoOfAvailableEntities(owner.AllEntities, owner, inputType, out var _, out var _, out var _, out var _, out var _, allAvailableItems);
		num = GetNoOfItemsOrderedAsInput(owner, inputType, process);
		int num2 = Common.ClampBottom(noOfAvailableItems.Value - num, 0) / input.Amount.NoOfItems.Value;
		if (num2 == 0)
		{
			hasInputs = false;
			noOfMissingInputTypes++;
			return;
		}
		noOfAvailableInputTypes++;
		if (currentProductionLimit.HasValue)
		{
			currentProductionLimit = Math.Min(currentProductionLimit.Value, num2);
		}
		else
		{
			currentProductionLimit = num2;
		}
	}

	public static bool HasAllInputsAndToolsForProcess(ProcessType processType, EntityGroup owner, Dictionary<EntityType, Availability> allAvailableItems)
	{
		bool hasInputs;
		bool hasTools;
		int maxAmountThatCanBeProduced;
		int? noOfMissingInputTypes;
		int? noOfAvailableInputTypes;
		bool hasSkills;
		bool hasResource;
		bool hasSpecialSite;
		bool hasPolicy;
		EntityType immovableInput;
		return HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResource, out hasSpecialSite, out hasPolicy, out immovableInput, allAvailableItems);
	}

	public static bool HasAllInputsAndToolsForProcess(ProcessType process, EntityGroup owner, out bool hasInputs, out bool hasTools, out int maxAmountThatCanBeProduced, out int? noOfMissingInputTypes, out int? noOfAvailableInputTypes, out bool hasSkills, out bool hasResource, out bool hasSpecialSite, out bool hasPolicy, out EntityType immovableInput, Dictionary<EntityType, Availability> allAvailableItems = null)
	{
		maxAmountThatCanBeProduced = 0;
		hasInputs = true;
		hasResource = true;
		hasSpecialSite = true;
		hasPolicy = true;
		int? currentProductionLimit = null;
		SkillType requiredSkillType = process.RequiredSkillType;
		hasSkills = owner.GetExpedition().HasSkill(requiredSkillType);
		immovableInput = process.ImmovableInput();
		if (process.InputsByType != null)
		{
			noOfMissingInputTypes = 0;
			noOfAvailableInputTypes = 0;
			int? noOfAvailableItems = null;
			foreach (KeyValuePair<EntityType, Input> item in process.InputsByType)
			{
				if (item.Key.TreeType == null)
				{
					HasInputForProcess(item.Key, item.Value, owner, process, ref hasInputs, ref noOfMissingInputTypes, ref noOfAvailableInputTypes, ref currentProductionLimit, ref noOfAvailableItems, allAvailableItems);
				}
			}
		}
		else
		{
			noOfMissingInputTypes = null;
			noOfAvailableInputTypes = null;
		}
		if (owner.Parent is Expedition expedition && !expedition.Policy.CanUseProcess(process, out var _))
		{
			hasPolicy = false;
		}
		if (process.ResourceTypeInput != null && !InventorySettings.HasResources(process))
		{
			hasResource = false;
		}
		if (process.ActingOnType != null && !InventorySettings.HasSpecialSite(process))
		{
			hasSpecialSite = false;
		}
		hasTools = HasToolsForProcess(process, owner);
		bool num = hasSkills & hasInputs & hasTools & hasResource & hasSpecialSite & hasPolicy;
		if (!num)
		{
			maxAmountThatCanBeProduced = 0;
			return num;
		}
		if (currentProductionLimit.HasValue)
		{
			maxAmountThatCanBeProduced = currentProductionLimit.Value;
			return num;
		}
		if (process.ActingOnType != null)
		{
			maxAmountThatCanBeProduced = 1;
			return num;
		}
		maxAmountThatCanBeProduced = 20;
		return num;
	}

	public static bool HasToolsForProcess(ProcessType process, EntityGroup owner)
	{
		if (process.ProcessToolSet != null)
		{
			bool flag = false;
			ToolAlternatives[] tools = process.ProcessToolSet.Tools;
			for (int i = 0; i < tools.Length; i++)
			{
				Tool[] tools2 = tools[i].Tools;
				for (int j = 0; j < tools2.Length; j++)
				{
					foreach (EntityType toolEntityType in tools2[j].ToolEntityTypes)
					{
						if (HasToolOfType(owner, toolEntityType))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return false;
				}
				flag = false;
			}
		}
		return true;
	}

	private static bool HasToolOfType(EntityGroup owner, EntityType toolType)
	{
		if (owner == null)
		{
			return false;
		}
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		List<EntityID> value;
		if (toolType.ItemType != null)
		{
			if (owner.Items.TryGetValue(toolType, out value))
			{
				return HasValidTool(owner, value, sharedKnowledge);
			}
		}
		else if (toolType.StructureType != null && owner.Structures.TryGetValue(toolType, out value))
		{
			return HasValidTool(owner, value, sharedKnowledge);
		}
		return false;
	}

	private static bool HasValidTool(EntityGroup owner, List<EntityID> entities, SharedKnowledge sharedKnowledge)
	{
		for (int num = entities.Count - 1; num >= 0; num--)
		{
			if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entities[num], owner, out var entityData))
			{
				bool flag = entityData.EntityType.IsIntrinsic();
				if (entityData.IsCompleted() && GoalEvaluator.IsOnPlaySite(entityData) && (flag || !entityData.PartOfID.HasValue))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static int GetNoOfItemsOrderedAsInput(EntityGroup owner, EntityType itemType, ProcessType currentProcess)
	{
		int num = 0;
		if (GameData.Instance.ProcessesUsingThisInput.TryGetValue(itemType, out var value))
		{
			foreach (ProcessType item in value)
			{
				if (currentProcess == item || !item.HasOutput)
				{
					continue;
				}
				Output[] outputs = item.Outputs;
				foreach (Output output in outputs)
				{
					if (output.IsWasteProduct || !owner.ProductionOrders.Orders.TryGetValue(output.FinalEntityTypeToCreate, out var value2) || !value2.ProductionJobsToComplete.HasValue)
					{
						continue;
					}
					int value3 = value2.ProductionJobsToComplete.Value;
					if (value3 > 0)
					{
						int? noOfItems = item.InputsByType[itemType].Amount.NoOfItems;
						if (noOfItems.HasValue)
						{
							num += value3 * noOfItems.Value;
						}
					}
				}
			}
		}
		return num;
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		if (latestCheckedTextButton != null)
		{
			latestCheckedTextButton.IsChecked = false;
		}
		latestCheckedTextButton = sender as TextButton;
		if ((bool)latestCheckedTextButton.Tag1)
		{
			StockButton stockButton = sender as StockButton;
			The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList);
			The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
		}
	}

	private void salvage_Click(UIComponent sender, EventArgs e)
	{
	}

	private void keepInStore_CountChanged(int newCount, EventArgs e)
	{
		expedition.OwnedEntities.ProductionOrders.Orders[((ItemTypeButtonEventArgs)e).Item].AmountToKeepInStore = newCount;
	}

	private void ExpandStockItem_OnPress(object sender, EventArgs e)
	{
		_ = ((ItemTypeButtonEventArgs)e).Item;
	}
}
