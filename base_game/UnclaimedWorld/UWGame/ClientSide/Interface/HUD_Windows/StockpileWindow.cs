using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class StockpileWindow : HUDWindow
{
	public enum InfoClickResult
	{
		Production,
		Data
	}

	private Stockpile.TypesOfStockpiles typeOfStockpile;

	private EntityID? structure;

	protected int itemHeight = 18;

	private const int quantityX = 238;

	private const int itemTypeIconColumnX = 18;

	private const int captionX = 38;

	private const int stageIconX = 41;

	private const int allowX = 251;

	private const int blockX = 337;

	private Grid outerGrid;

	private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private TextButton tbRemove;

	private TextButton btOK;

	private TextButton btCancel;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private UIComponent listSurface;

	private const int rowHeight = 36;

	private const int expandedPanelMargin = 3;

	private Dictionary<EntityType, int> allItems = new Dictionary<EntityType, int>();

	private List<Tuple<EntityCategory, CollapsablePanel>> categoryPanels = new List<Tuple<EntityCategory, CollapsablePanel>>();

	private List<Grid> categoryGridsThatWereAddedTo = new List<Grid>();

	public StockpileWindow()
		: base(448, 500, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 200;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		AddZoneNameAndHeader("", "STOCKPILE", "HUD_icon_stockpile", 18, out lblName, out lblHeader, out headerIcon);
		listSurface = new UIComponent(gui);
		Add(listSurface);
		listSurface.X = 18;
		listSurface.Y = 50;
		listSurface.Width = DisplayWindow.ViewPort.Width - 36;
		listSurface.Height = 354;
		outerGrid = HUDWindow.CreateOuterGridForCollapsableLists(The.InGameUI.gui, listSurface);
		outerGrid.ItemHeight = 36;
		btCancel = new TextButton(gui);
		Add(btCancel);
		btCancel.Text = "CANCEL";
		btCancel.Init(TextButton.TextButtonType.HUD);
		btCancel.Click += btCancel_Click;
		btCancel.Width = 72;
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 18;
		btCancel.X = DisplayWindow.Width - 18 - btCancel.Width;
		btOK = new TextButton(gui);
		Add(btOK);
		btOK.Text = "OK";
		btOK.Init(TextButton.TextButtonType.HUD);
		btOK.Click += btOk_Click;
		btOK.Width = 72;
		btOK.Y = DisplayWindow.Height - btOK.Height - 18;
		btOK.X = btCancel.X - 2 - btOK.Width;
		tbRemove = new TextButton(gui);
		Add(tbRemove);
		tbRemove.Text = "Delete";
		tbRemove.ToolTip = "Remove the stockpile";
		tbRemove.Init(TextButton.TextButtonType.HUD);
		tbRemove.Click += bt_RemoveClick;
		tbRemove.Y = 12;
		tbRemove.X = DisplayWindow.Width - tbRemove.Width - 12;
		tbRemove.ScaleWidthToFitText();
		_ = DisplayWindow.Height;
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 18;
		btOK.Y = btCancel.Y;
		listSurface.Height = btOK.Y - 13 - listSurface.Y;
		outerGrid.Height = listSurface.Height;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	public override void Refresh()
	{
		Fill();
	}

	private void UpdateCategoryRow(CollapsablePanel cpCategory, Grid categoryGrid, EntityCategory entityCategory, Stockpile stockpile, bool fillUserControls)
	{
		if (fillUserControls)
		{
			GetCategorySetting(entityCategory, stockpile, out var allowSetting);
			cpCategory.FindChildById<ImageButton>(UIComponent.DataControlID.CurrentCategoryOrders, out var child, firstLevelOnly: false);
			child.IsChecked = allowSetting;
		}
	}

	private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, EntityCategory entityCategory, Stockpile stockpile)
	{
		GetCategorySetting(entityCategory, stockpile, out var _);
		cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.StockpileHUD);
		cpCategory.CollapsedHeight = outerGrid.ItemHeight;
		outerGrid.AddEntry(entityCategory, cpCategory);
		// UNHIDDEN MOD: same for the category header. SortOrder rather than the name, so categories
		// keep the order the data gives them and only the items inside go alphabetical.
		cpCategory.OrderByTag1 = entityCategory.SortOrder;
		cpCategory.Init();
		cpCategory.Title = entityCategory.Name.ToUpper(Config.Culture);
		cpCategory.Width = outerGrid.Width;
		cpCategory.TitleSummaryRightAlignXPos = 241;
		cpCategory.TitlePositionX = 41;
		Image image = new Image(gui);
		image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_exclamationmark_parenthesis"));
		image.ResizeControlToFitImage();
		image.ToolTip = "Some items in the category have overriding settings.";
		cpCategory.Add(image);
		image.X = 256;
		cpCategory.CenterOnHeader(image);
		image.ID = UIComponent.DataControlID.HasOverridingItemIcon;
		image.Visible = false;
		ItemCategoryButtonEventArgs eventArgs = new ItemCategoryButtonEventArgs(entityCategory);
		ImageButton imageButton = new ImageButton(gui);
		cpCategory.Add(imageButton);
		imageButton.Init(ImageButtonType.HUDCheckbox);
		imageButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		imageButton.Click += cbCategory_Click;
		imageButton.X = 318;
		imageButton.EventArgs = eventArgs;
		imageButton.ToolTip = "Stockpile everything in this category";
		imageButton.ID = UIComponent.DataControlID.CurrentCategoryOrders;
		cpCategory.CenterOnHeader(imageButton);
		categoryGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		categoryGrid.DebugTag = "categoryGrid";
		categoryGrid.FixedItemHeights = true;
		categoryGrid.Width = cpCategory.Width;
		cpCategory.AddContent(categoryGrid);
		categoryGrid.VMargin = 8;
		categoryGrid.ScrollBarEnabled = false;
		categoryGrid.ItemHeight = 22;
		categoryGrid.CanGrowInHeight = true;
		categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid.IsOuterGrid = false;
	}

	private void cbItem_Click(UIComponent sender, EventArgs e)
	{
		ImageButton obj = sender as ImageButton;
		sender.Parent.FindChildById<FillableBar>(UIComponent.DataControlID.MaxOrders, out var child, firstLevelOnly: false);
		if (obj.IsChecked)
		{
			child.Value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
			child.Visible = true;
			child.UpdateSliderPosition();
		}
		else
		{
			child.Value = 0;
			child.UpdateSliderPosition();
			child.Visible = false;
		}
	}

	private void cbCategory_Click(UIComponent sender, EventArgs e)
	{
		if ((sender as ImageButton).IsChecked)
		{
			EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
			CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[category];
			AllowItems(cpCategory);
			UpdateOverridingSettingIcon(cpCategory, category);
		}
		else
		{
			EntityCategory category2 = ((ItemCategoryButtonEventArgs)e).Category;
			CollapsablePanel cpCategory2 = (CollapsablePanel)outerGrid.EntriesByKey[category2];
			ProhibitItems(cpCategory2);
			UpdateOverridingSettingIcon(cpCategory2, category2);
		}
	}

	private static void AllowItems(CollapsablePanel cpCategory)
	{
		foreach (UIComponent entry in ((Grid)cpCategory.ExpandedPanel.Controls[0]).Entries)
		{
			((ImageButton)entry.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked = true;
			entry.FindChildById<FillableBar>(UIComponent.DataControlID.MaxOrders, out var child, firstLevelOnly: false);
			child.Value = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
			child.Visible = true;
			child.UpdateSliderPosition();
		}
	}

	private static void ProhibitItems(CollapsablePanel cpCategory)
	{
		foreach (UIComponent entry in ((Grid)cpCategory.ExpandedPanel.Controls[0]).Entries)
		{
			((ImageButton)entry.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked = false;
			entry.FindChildById<FillableBar>(UIComponent.DataControlID.MaxOrders, out var child, firstLevelOnly: false);
			child.Value = 0;
			child.Visible = false;
			child.UpdateSliderPosition();
		}
	}

	private void DeselectTopLevel()
	{
	}

	private UIComponent AddItemRow(Grid categoryGrid, EntityType entityType, EntityGroup owner)
	{
		UIComponent uIComponent = new UIComponent(gui);
		categoryGrid.AddEntry(entityType, uIComponent);
		// UNHIDDEN MOD: give the row a sort key. Without one, DoCategorySorting below orders every
		// row by a null OrderByTag1 - OrderBy is stable, so nothing moved and the sort looked like it
		// did nothing at all. Reported by Kastuk as "already tried in Unhidden mod without success".
		// PluralName is what BuySellPanel uses for the same job, so the stockpile now reads the way
		// the trade window already does.
		uIComponent.OrderByTag1 = entityType.PluralName;
		CreateItemGridRow(entityType, GoalEvaluator.GetOwnerID(owner), useUIOwner: false, uIComponent, DataSheet.InfoToShow.Data, usePluralName: true, isRoot: true, out var _);
		Label label = new Label(gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.ID = UIComponent.DataControlID.Stock;
		uIComponent.CenterChildVertically(label);
		label.Y += 2;
		ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
		ImageButton imageButton = new ImageButton(gui);
		uIComponent.Add(imageButton);
		imageButton.Init(ImageButtonType.HUDCheckbox);
		imageButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		imageButton.X = 251;
		uIComponent.CenterChildVertically(imageButton);
		imageButton.EventArgs = eventArgs;
		imageButton.ToolTip = "Check the box to allow the item to be stockpiled. This will override the category setting.";
		imageButton.ID = UIComponent.DataControlID.CurrentOrders;
		imageButton.Click += cbItem_Click;
		FillableBar fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSlider, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		uIComponent.Add(fillableBar);
		fillableBar.X = imageButton.Right + 6;
		uIComponent.CenterChildVertically(fillableBar);
		fillableBar.ID = UIComponent.DataControlID.MaxOrders;
		fillableBar.ShowMaxValueLabelAtEnd = false;
		fillableBar.Width = 140;
		fillableBar.MaxValue = GameData.Instance.GUIConstants.UnlimitedStockpileValue;
		fillableBar.SliderTooltip = "Set the maximum that can be stockpiled";
		fillableBar.ButtonTooltip = "Set the maximum that can be stockpiled";
		fillableBar.MaxSliderValueSymbol = "...";
		fillableBar.MaxSliderValueTooltip = "No limit";
		return uIComponent;
	}

	private void itemRadioGroup_NewMemberChecked(ICanBeChecked obj, EventArgs e)
	{
		EntityType item = ((ItemTypeButtonEventArgs)e).Item;
		CollapsablePanel cpCategory = (CollapsablePanel)outerGrid.EntriesByKey[item.Category];
		UpdateOverridingSettingIcon(cpCategory, item.Category);
	}

	private void UpdateOverridingSettingIcon(CollapsablePanel cpCategory, EntityCategory category, Stockpile stockpile = null)
	{
		if (stockpile == null)
		{
			MapArea mapArea = null;
			if (!GetData(out var _, out var _, out mapArea, out stockpile))
			{
				Hide();
				return;
			}
		}
		Image image = (Image)cpCategory.FindChildById(UIComponent.DataControlID.HasOverridingItemIcon);
		if (OneOrMoreItemsInCategoryAreDifferent(cpCategory, category, stockpile, out var hiddenItemsAreDifferent))
		{
			image.Visible = true;
			if (hiddenItemsAreDifferent)
			{
				image.ToolTip = "Some hidden item types have overriding settings that are different.";
			}
			else
			{
				image.ToolTip = "Some item types have overriding settings that are different.";
			}
		}
		else
		{
			image.Visible = false;
		}
	}

	public static void CreateItemGridRow(EntityType entityType, EntityGroupID? owner, bool useUIOwner, UIComponent item, DataSheet.InfoToShow infoClickResult, bool usePluralName, bool isRoot, out DataTypeButton entityTypeButton, bool includeIcon = true)
	{
		InventoryPanel.AddEntityTypeIcon(entityType, item, 18).ID = UIComponent.DataControlID.Icon;
		entityTypeButton = new DataTypeButton(The.InGameUI.gui, infoClickResult, entityType, owner, useUIOwner);
		entityTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		entityTypeButton.ID = UIComponent.DataControlID.Caption;
		entityTypeButton.IsRoot = isRoot;
		entityTypeButton.Text = (usePluralName ? entityType.PluralName : entityType.Name);
		item.Add(entityTypeButton);
		entityTypeButton.TextAlignment = TextButton.TextAlign.Left;
		entityTypeButton.Width = 172;
		entityTypeButton.X = 38;
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		if (!GetData(out var owner, out var _, out var _, out var _))
		{
			Hide();
		}
		else
		{
			The.InGameUI.EntityListWindow.PopulateAndShowOnPlayfield(sender, ((ItemTypeButtonEventArgs)e).Item, owner);
		}
	}

	private void bt_RemoveClick(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.SelectedZone != null)
		{
			The.InGameUI.SelectedZone.Stockpile = null;
			Hide();
		}
	}

	private void btTopAll_Click(UIComponent sender, EventArgs e)
	{
	}

	private bool GetData(out EntityGroup owner, out IKnownEntityData structureData, out MapArea mapArea, out Stockpile stockpile)
	{
		stockpile = null;
		if (!structure.HasValue)
		{
			mapArea = TileSelectionContextMenu.GetMapArea();
			structureData = null;
			owner = mapArea.GetOwner();
			if (mapArea.Zone != null)
			{
				stockpile = mapArea.Zone.Stockpile;
			}
			return true;
		}
		mapArea = null;
		if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(structure.Value, out structureData)))
		{
			owner = null;
			return false;
		}
		LookUpOwners.ResolveEntityOwner(structureData, out IOwner owner2);
		if (owner2 == null)
		{
			owner = null;
			return false;
		}
		owner = owner2.OwnedEntities;
		if (typeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
		{
			owner.StructureStockpiles.TryGetValue(structure.Value, out stockpile);
		}
		else
		{
			owner.TerminalTradeOffers.TryGetValue(structure.Value, out stockpile);
		}
		return true;
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void btOk_Click(UIComponent sender, EventArgs e)
	{
		if (!GetData(out var owner, out var structureData, out var _, out var _))
		{
			Hide();
			return;
		}
		GetUserSettings(out var mayStockpileCategory, out var mayStockpileItem);
		DefaultStorageSettings defaultStorageSettings = null;
		if (structureData != null)
		{
			defaultStorageSettings = structureData.EntityType.ContainerType.GetDefaultStorageSettings();
		}
		CreateStockpile command = ((structureData != null) ? new CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultStorageSettings, structure.Value, typeOfStockpile, giveClientFeedback: true) : ((The.InGameUI.SelectedZone == null) ? new CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultStorageSettings, The.InGameUI.SelectedTiles, giveClientFeedback: true) : new CreateStockpile(mayStockpileCategory, mayStockpileItem, owner.ID, defaultStorageSettings, The.InGameUI.SelectedZone.ID, giveClientFeedback: true)));
		The.Client.Controller.StoreAndExecuteCommand(command);
		Hide();
	}

	private void GetUserSettings(out SerializableDictionary<string, bool> mayStockpileCategory, out SerializableDictionary<string, int> mayStockpileItem)
	{
		mayStockpileCategory = new SerializableDictionary<string, bool>();
		mayStockpileItem = new SerializableDictionary<string, int>();
		foreach (KeyValuePair<object, UIComponent> item in outerGrid.EntriesByKey)
		{
			EntityCategory entityCategory = item.Key as EntityCategory;
			CollapsablePanel obj = item.Value as CollapsablePanel;
			if (((ImageButton)obj.FindChildById(UIComponent.DataControlID.CurrentCategoryOrders)).IsChecked)
			{
				mayStockpileCategory.Add(entityCategory.KeyName, value: true);
			}
			else
			{
				mayStockpileCategory.Add(entityCategory.KeyName, value: false);
			}
			foreach (KeyValuePair<object, UIComponent> item2 in ((Grid)obj.ExpandedPanel.Controls[0]).EntriesByKey)
			{
				EntityType entityType = (EntityType)item2.Key;
				if (((ImageButton)item2.Value.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked)
				{
					item2.Value.FindChildById<FillableBar>(UIComponent.DataControlID.MaxOrders, out var child, firstLevelOnly: false);
					int value = ((child.Value != GameData.Instance.GUIConstants.UnlimitedStockpileValue) ? child.Value : (-1));
					mayStockpileItem.Add(entityType.KeyName, value);
				}
				else
				{
					mayStockpileItem.Add(entityType.KeyName, 0);
				}
			}
		}
	}

	private void btTopNone_Click(UIComponent sender, EventArgs e)
	{
	}

	public new void Hide()
	{
		DisplayWindow.Hide();
	}

	public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
	{
		base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);
		tbRemove.X = lblHeader.Right + 60;
	}

	public void FillFromArea()
	{
		tbRemove.Visible = true;
		structure = null;
		typeOfStockpile = Stockpile.TypesOfStockpiles.Normal;
		Fill(fillUserControls: true);
	}

	public void FillFromStructure(EntityID? structure, Stockpile.TypesOfStockpiles typeOfStockpile)
	{
		tbRemove.Visible = false;
		this.structure = structure;
		this.typeOfStockpile = typeOfStockpile;
		Fill(fillUserControls: true);
	}

	public static bool ItemIsOwnedByAllegiance(IKnownEntityData e)
	{
		if (e.EntityType.ItemType != null && e.OwnedBy.HasValue)
		{
			return The.InGameUI.UIAllegiance.IsOwnedByAllegiance(e);
		}
		return false;
	}

	private void Fill(bool fillUserControls = false)
	{
		MapArea mapArea = null;
		if (!GetData(out var owner, out var structureData, out mapArea, out var stockpile))
		{
			Hide();
			return;
		}
		if (typeOfStockpile == Stockpile.TypesOfStockpiles.Normal)
		{
			lblHeader.Text = "STOCKPILE";
		}
		else
		{
			lblHeader.Text = "TRADE";
		}
		string zoneName = "";
		if (mapArea != null && mapArea.Zone != null)
		{
			zoneName = mapArea.Zone.GetDisplayName();
		}
		else if (structureData != null)
		{
			zoneName = structureData.EntityType.Name.ToUpper(Config.Culture);
		}
		SetDisplayName(zoneName, lblName, lblHeader, headerIcon);
		Populate(fillUserControls, mapArea, owner, structureData, stockpile);
	}

	private void Populate(bool fillUserControls, MapArea mapArea, EntityGroup owner, IKnownEntityData structureData, Stockpile stockpile)
	{
		allItems.Clear();
		CountItemsThatAreAlreadyHere(mapArea, structureData);
		Grid grid = null;
		allAvailableItems.Clear();
		categoryGridsThatWereAddedTo.Clear();
		categoryPanels.Clear();
		outerGrid.BeginAddingEntries();
		bool flag = false;
		CollapsablePanel cpCategory;
		foreach (KeyValuePair<EntityType, List<EntityID>> item4 in owner.Items)
		{
			EntityType key = item4.Key;
			EntityCategory category = key.Category;
			if (GameData.Instance.GUIConstants.StockpileExcludesCategoriesFinal.Contains(category))
			{
				continue;
			}
			if (!allItems.TryGetValue(key, out var value))
			{
				value = 0;
			}
			flag = false;
			cpCategory = null;
			grid = null;
			if (outerGrid.TryGetEntry(category, out var item))
			{
				cpCategory = item as CollapsablePanel;
				grid = (Grid)cpCategory.ExpandedPanel.Controls[0];
				if (!categoryPanels.Exists((Tuple<EntityCategory, CollapsablePanel> t) => t.Item2 == cpCategory))
				{
					categoryPanels.Add(new Tuple<EntityCategory, CollapsablePanel>(category, cpCategory));
					UpdateCategoryRow(cpCategory, grid, category, stockpile, fillUserControls);
				}
			}
			else
			{
				if (!InventoryPanel.OwnsProductOrHasProcessInputsAndTools(key, owner, allAvailableItems))
				{
					continue;
				}
				AddCategoryRow(ref cpCategory, ref grid, category, stockpile);
				UpdateCategoryRow(cpCategory, grid, category, stockpile, fillUserControls);
				categoryPanels.Add(new Tuple<EntityCategory, CollapsablePanel>(category, cpCategory));
				flag = true;
				grid.BeginAddingEntries();
				categoryGridsThatWereAddedTo.Add(grid);
			}
			if (!flag && grid.TryGetEntry(key, out var item2))
			{
				UpdateItemRow(fillUserControls, item2, key, value, stockpile);
			}
			else if (InventoryPanel.OwnsProductOrHasProcessInputsAndTools(key, owner, allAvailableItems))
			{
				grid.BeginAddingEntries();
				item2 = AddItemRow(grid, key, owner);
				UpdateItemRow(fillUserControls, item2, key, value, stockpile);
				if (!categoryGridsThatWereAddedTo.Contains(grid))
				{
					categoryGridsThatWereAddedTo.Add(grid);
				}
			}
		}
		foreach (Tuple<EntityCategory, CollapsablePanel> item3 in categoryPanels)
		{
			item3.Item2.Summary = allItems.Sum((KeyValuePair<EntityType, int> e) => (e.Key.Category == item3.Item1) ? e.Value : 0).ToString();
			if (fillUserControls)
			{
				UpdateOverridingSettingIcon(item3.Item2, item3.Item1, stockpile);
			}
		}
		foreach (Grid item5 in categoryGridsThatWereAddedTo)
		{
			item5.EndAddingEntries();
		}
		outerGrid.EndAddingEntries();
		// UNHIDDEN MOD: sort the stockpile the way the inventory panel already sorts itself.
		// InventoryPanel.DoCategorySorting is the studio's own routine, used at
		// InventoryPanel.cs:949; the stockpile window just never called it.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			InventoryPanel.DoCategorySorting(outerGrid, categoryGridsThatWereAddedTo, Grid.Sorting.Ascending);
		}
	}

	private void CountItemsThatAreAlreadyHere(MapArea mapArea, IKnownEntityData structureData)
	{
		if (mapArea != null)
		{
			mapArea.GetNoOfEntitiesInArea(allItems, (IKnownEntityData e) => ItemIsOwnedByAllegiance(e), The.InGameUI.UIAllegiance);
			return;
		}
		Dictionary<EntityType, List<EntityID>> offeredEntitiesByType = structureData.OfferedEntitiesByType;
		if (typeOfStockpile == Stockpile.TypesOfStockpiles.OfferedForTrade)
		{
			if (!(structureData is Entity))
			{
				return;
			}
			{
				foreach (KeyValuePair<EntityType, List<EntityID>> item in offeredEntitiesByType)
				{
					foreach (EntityID item2 in item.Value)
					{
						if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item2, out var data)))
						{
							MapArea.CountEntity(allItems, data, ItemIsOwnedByAllegiance);
						}
					}
				}
				return;
			}
		}
		if (structureData.ContainedEntities == null)
		{
			return;
		}
		foreach (EntityID containedEntity in structureData.ContainedEntities)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(containedEntity, out var data2)) && (offeredEntitiesByType == null || !offeredEntitiesByType.TryGetValue(data2.EntityType, out var value) || !value.Contains(data2.EntityID)))
			{
				MapArea.CountEntity(allItems, data2, ItemIsOwnedByAllegiance);
			}
		}
	}

	private void FillToplevelControls(Stockpile stockpile)
	{
	}

	private bool OneOrMoreItemsInCategoryAreDifferent(CollapsablePanel cp, EntityCategory category, Stockpile stockpile, out bool hiddenItemsAreDifferent)
	{
		hiddenItemsAreDifferent = false;
		cp.FindChildById<ImageButton>(UIComponent.DataControlID.CurrentCategoryOrders, out var cbCategory, firstLevelOnly: false);
		Grid grid = (Grid)cp.ExpandedPanel.Controls[0];
		if (grid.Entries.Exists((UIComponent c) => ((ICanBeChecked)c.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked != cbCategory.IsChecked))
		{
			return true;
		}
		if (stockpile != null && stockpile.CategoryHasDifferentItemSetting(cbCategory.IsChecked, category, grid.EntriesByKey.Keys.Select((object o) => (EntityType)o).ToList()))
		{
			hiddenItemsAreDifferent = true;
			return true;
		}
		return false;
	}

	private void UpdateItemRow(bool fillUserControls, UIComponent itemRow, EntityType entityType, int noOfStockpiledItems, Stockpile stockpile)
	{
		UIComponent uIComponent = itemRow.FindChildById(UIComponent.DataControlID.Stock);
		if (uIComponent != null)
		{
			Label obj = (Label)uIComponent;
			obj.Text = noOfStockpiledItems.ToString();
			obj.FitToText();
			obj.AlignRight(238);
		}
		if (fillUserControls)
		{
			ImageButton obj2 = (ImageButton)itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders);
			bool isChecked = false;
			GetItemSetting(stockpile, entityType, out isChecked, out var maxOrders);
			obj2.IsChecked = isChecked;
			FillableBar fillableBar = (FillableBar)itemRow.FindChildById(UIComponent.DataControlID.MaxOrders);
			int value = ((maxOrders != -1) ? maxOrders : GameData.Instance.GUIConstants.UnlimitedStockpileValue);
			fillableBar.Value = value;
			fillableBar.UpdateSliderPosition();
			if (isChecked)
			{
				fillableBar.Visible = true;
			}
			else
			{
				fillableBar.Visible = false;
			}
		}
	}

	private void GetCategorySetting(EntityCategory entityCategory, Stockpile stockpile, out bool allowSetting)
	{
		if (stockpile != null)
		{
			if (stockpile.TryGetCategory(entityCategory, out var categorySetting))
			{
				if (categorySetting)
				{
					allowSetting = true;
				}
				else
				{
					allowSetting = false;
				}
			}
			else
			{
				allowSetting = stockpile.GetAllowBaseSetting(out var _);
			}
		}
		else
		{
			allowSetting = Stockpile.GetAllowBaseSetting(typeOfStockpile, out var _);
		}
	}

	private void GetItemSetting(Stockpile stockpile, EntityType entityType, out bool isChecked, out int maxOrders)
	{
		if (stockpile != null)
		{
			isChecked = stockpile.MayStockpile(entityType, out maxOrders);
		}
		else
		{
			isChecked = Stockpile.GetAllowBaseSetting(typeOfStockpile, out maxOrders);
		}
	}

	public static bool OverlapsWithOtherStockpiles(MapArea mapArea)
	{
		bool otherStockpilesOnTile = false;
		mapArea.IterateAreaBreakOnTrue(delegate(TerrainTile tile)
		{
			List<Zone> listOfZones = tile.GetListOfZones(The.InGameUI.UIAllegiance);
			if (listOfZones != null && listOfZones.Count > 0 && listOfZones.Exists((Zone z) => z != The.InGameUI.SelectedZone && z.Stockpile != null))
			{
				otherStockpilesOnTile = true;
				return true;
			}
			return false;
		});
		return otherStockpilesOnTile;
	}

	public void ResourceClicked(UIComponent sender, EventArgs eventArgs)
	{
	}

	public void SaveJobChanges(ResourceType resourceType, int noOfJobs)
	{
	}
}
