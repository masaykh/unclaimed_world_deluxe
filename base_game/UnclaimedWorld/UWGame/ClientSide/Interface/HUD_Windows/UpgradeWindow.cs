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
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class UpgradeWindow : HUDWindow
{
	private class UpgradeEventArgs : EventArgs
	{
		public EntityType EntityType;

		public UpgradeCategory UpgradeCategory;
	}

	public enum InfoClickResult
	{
		Production,
		Data
	}

	private EntityID entityID;

	private Image headerIcon;

	protected int itemHeight = 18;

	private const int produceColumnX = 226;

	private const int maxOrdersX = 238;

	private const int itemTypeIconColumnX = 18;

	private const int captionX = 38;

	private const int categoryCheckedX = 330;

	private const int stageIconX = 41;

	private const int allowX = 267;

	private const int blockX = 337;

	private Grid outerGrid;

	private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private UIComponent listSurface;

	private Label lblName;

	private Label lblHeader;

	private TextButton btCancel;

	private const int rowHeight = 36;

	private const int expandedPanelMargin = 3;

	private List<Tuple<EntityCategory, CollapsablePanel>> categoryPanels = new List<Tuple<EntityCategory, CollapsablePanel>>();

	private List<Grid> categoryGridsThatWereAddedTo = new List<Grid>();

	public UpgradeWindow()
		: base(448, 500, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 100;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		AddZoneNameAndHeader("", "UPGRADE", "HUD_icon_uparrow", 18, out lblName, out lblHeader, out headerIcon);
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
		btCancel.Text = "CLOSE";
		btCancel.Init(TextButton.TextButtonType.HUD);
		btCancel.Click += btCancel_Click;
		btCancel.Width = 72;
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 18;
		btCancel.X = DisplayWindow.Width - 18 - btCancel.Width;
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 18;
		listSurface.Height = btCancel.Y - 13 - listSurface.Y;
		outerGrid.Height = listSurface.Height;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	public override void Refresh()
	{
		Populate(fillUserControls: false);
	}

	private void UpdateCategoryRow(EntityGroup owner, CollapsablePanel cpCategory, Grid categoryGrid, UpgradeCategory upgradeCategory)
	{
		Icon icon = cpCategory.FindChildById(UIComponent.DataControlID.HasSelection, firstLevelOnly: true) as Icon;
		if (owner.GetOrderedUpgrade(entityID, upgradeCategory) != null)
		{
			icon.Visible = true;
		}
		else
		{
			icon.Visible = false;
		}
	}

	private void AddCategoryRow(out CollapsablePanel cpCategory, out Grid categoryGrid, UpgradeCategory upgradeCategory)
	{
		cpCategory = new CollapsablePanel(gui, CollapsablePanel.PanelType.StockpileHUD);
		cpCategory.CollapsedHeight = outerGrid.ItemHeight;
		outerGrid.AddEntry(upgradeCategory, cpCategory);
		cpCategory.Init();
		cpCategory.Title = upgradeCategory.Name.ToUpper(Config.Culture);
		cpCategory.Width = outerGrid.Width;
		cpCategory.TitleSummaryRightAlignXPos = 241;
		cpCategory.TitlePositionX = 41;
		cpCategory.TitleTooltip = upgradeCategory.Description;
		cpCategory.OrderByTag1 = upgradeCategory.SortOrder;
		Image image = new Image(gui);
		image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_checkmark"));
		image.ResizeControlToFitImage();
		image.ToolTip = "An upgrade is selected";
		cpCategory.Add(image);
		image.X = 330;
		cpCategory.CenterOnHeader(image);
		image.ID = UIComponent.DataControlID.HasSelection;
		image.Visible = false;
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

	private void tbCategoryAllow_Click(UIComponent sender, EventArgs e)
	{
		if ((sender as TextButton).IsChecked)
		{
			EntityCategory category = ((ItemCategoryButtonEventArgs)e).Category;
			CollapsablePanel obj = (CollapsablePanel)outerGrid.EntriesByKey[category];
			((TextButton)obj.FindChildById(UIComponent.DataControlID.UpgradeProhibit)).IsChecked = false;
			AllowItems(obj);
		}
	}

	private static void AllowItems(CollapsablePanel cpCategory)
	{
		foreach (UIComponent entry in ((Grid)cpCategory.ExpandedPanel.Controls[0]).Entries)
		{
			((ImageButton)entry.FindChildById(UIComponent.DataControlID.CurrentOrders)).IsChecked = true;
		}
	}

	private static void ProhibitItems(CollapsablePanel cpCategory)
	{
		foreach (UIComponent entry in ((Grid)cpCategory.ExpandedPanel.Controls[0]).Entries)
		{
			((TextButton)entry.FindChildById(UIComponent.DataControlID.UpgradeAllowItem)).IsChecked = false;
			((TextButton)entry.FindChildById(UIComponent.DataControlID.UpgradeProhibitIem)).IsChecked = true;
		}
	}

	private UIComponent AddItemRow(Grid categoryGrid, UpgradeCategory upgradeCategory, EntityType entityType, EntityGroup owner)
	{
		UIComponent uIComponent = new UIComponent(gui);
		categoryGrid.AddEntry(entityType, uIComponent);
		uIComponent.OrderByTag1 = ((entityType.TierOrAreaType != null) ? entityType.TierOrAreaType.GetTier().Index : 0);
		uIComponent.OrderByTag2 = entityType.PluralName;
		CreateItemGridRow(entityType, GoalEvaluator.GetOwnerID(owner), useUIOwner: false, uIComponent, DataSheet.InfoToShow.Production, usePluralName: true, isRoot: true, out var _);
		Label label = new Label(gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.ID = UIComponent.DataControlID.MaxOrders;
		uIComponent.CenterChildVertically(label);
		label.Y += 2;
		label.X = 226;
		HorizontalList horizontalList = new HorizontalList(Interface.gui);
		uIComponent.Add(horizontalList);
		horizontalList.ID = UIComponent.DataControlID.NotAttainableIcons;
		horizontalList.X = 226;
		horizontalList.Height = 21;
		uIComponent.CenterChildVertically(horizontalList);
		HorizontalList horizontalList2 = new HorizontalList(Interface.gui);
		uIComponent.Add(horizontalList2);
		horizontalList2.ID = UIComponent.DataControlID.AttainableIcons;
		horizontalList2.X = 226;
		horizontalList2.Height = 21;
		uIComponent.CenterChildVertically(horizontalList2);
		CheckBox checkBox = new CheckBox(Interface.gui);
		checkBox.Init(CheckBoxType.HUDCheckBox);
		uIComponent.Add(checkBox);
		checkBox.FitToText();
		checkBox.X = 326;
		checkBox.Click += cbSelect_Click;
		uIComponent.CenterChildVertically(checkBox);
		checkBox.Y += 2;
		checkBox.EventArgs = new UpgradeEventArgs
		{
			EntityType = entityType,
			UpgradeCategory = upgradeCategory
		};
		checkBox.ID = UIComponent.DataControlID.Selector;
		checkBox.ToolTip = "When selected, the upgrade will be installed as soon as possible. If it breaks, a new upgrade will be installed. If unselected, the upgrade will be removed again.";
		categoryGrid.DebugTag = "categoryGrid";
		return uIComponent;
	}

	private void cbSelect_Click(UIComponent sender, EventArgs e)
	{
		if (!ResolveEntity(out var entityData, out var owner))
		{
			Hide();
			return;
		}
		UpgradeEventArgs e2 = e as UpgradeEventArgs;
		string upgradeTypeKey = null;
		bool isChecked = ((ICanBeChecked)sender).IsChecked;
		if (isChecked)
		{
			upgradeTypeKey = e2.EntityType.KeyName;
		}
		SetUpgrade command = new SetUpgrade(entityData.EntityID, owner.GetAllegiance().ID, owner.ID, giveClientFeedback: true, e2.UpgradeCategory, upgradeTypeKey);
		The.Client.Controller.StoreAndExecuteCommand(command);
		if (isChecked && e2.EntityType.Upgrader.StorageSettingsFinal != null)
		{
			CreateStockpile command2 = new CreateStockpile(null, null, owner.ID, e2.EntityType.Upgrader.StorageSettingsFinal, entityData.EntityID, Stockpile.TypesOfStockpiles.Normal, giveClientFeedback: true);
			The.Client.Controller.StoreAndExecuteCommand(command2);
		}
		Populate(fillUserControls: true);
		GetJobs(owner);
	}

	private static List<Job> GetJobs(EntityGroup owner)
	{
		return owner.OtherJobs;
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

	private void btTopAll_Click(UIComponent sender, EventArgs e)
	{
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void GetUserSettings(out SerializableDictionary<string, bool> mayStockpileCategory, out SerializableDictionary<string, bool> mayStockpileItem)
	{
		mayStockpileCategory = new SerializableDictionary<string, bool>();
		mayStockpileItem = new SerializableDictionary<string, bool>();
		foreach (KeyValuePair<object, UIComponent> item in outerGrid.EntriesByKey)
		{
			EntityCategory entityCategory = item.Key as EntityCategory;
			CollapsablePanel obj = item.Value as CollapsablePanel;
			TextButton textButton = (TextButton)obj.FindChildById(UIComponent.DataControlID.UpgradeAllow);
			TextButton textButton2 = (TextButton)obj.FindChildById(UIComponent.DataControlID.UpgradeProhibit);
			if (textButton.IsChecked)
			{
				mayStockpileCategory.Add(entityCategory.KeyName, value: true);
			}
			else if (textButton2.IsChecked)
			{
				mayStockpileCategory.Add(entityCategory.KeyName, value: false);
			}
			foreach (KeyValuePair<object, UIComponent> item2 in ((Grid)obj.ExpandedPanel.Controls[0]).EntriesByKey)
			{
				EntityType entityType = (EntityType)item2.Key;
				TextButton textButton3 = (TextButton)item2.Value.FindChildById(UIComponent.DataControlID.UpgradeAllowItem);
				mayStockpileItem.Add(entityType.KeyName, textButton3.IsChecked);
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
	}

	public static bool ItemIsOwnedByAllegiance(IKnownEntityData e)
	{
		if (e.EntityType.ItemType != null && e.OwnedBy.HasValue && LookUpOwners.ResolveEntityOwner(e, out IOwner owner) && owner != null && owner.Allegiance == The.InGameUI.UIAllegiance)
		{
			return true;
		}
		return false;
	}

	private bool ResolveEntity(out IKnownEntityData entityData, out EntityGroup owner)
	{
		owner = null;
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out entityData);
		if (entityData == null)
		{
			Hide();
			return false;
		}
		LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner2);
		if (owner2 == null)
		{
			return false;
		}
		owner = owner2.OwnedEntities;
		if (owner.GetAllegiance() != The.InGameUI.UIAllegiance)
		{
			Hide();
			return false;
		}
		return true;
	}

	private void Populate(bool fillUserControls)
	{
		if (!ResolveEntity(out var entityData, out var owner))
		{
			Hide();
			return;
		}
		allAvailableItems.Clear();
		categoryGridsThatWereAddedTo.Clear();
		categoryPanels.Clear();
		outerGrid.BeginAddingEntries();
		HashSet<EntityType> includedEntityTypes = null;
		List<UpgradeCategory> upgradeOptions = entityData.EntityType.ContainerType.GetUpgradeOptions();
		CollapsablePanel cpCategory;
		foreach (UpgradeCategory item3 in upgradeOptions)
		{
			Grid categoryGrid;
			if (!outerGrid.TryGetEntry(item3, out var item))
			{
				AddCategoryRow(out cpCategory, out categoryGrid, item3);
				if (categoryPanels.Exists((Tuple<EntityCategory, CollapsablePanel> t) => t.Item2 == cpCategory))
				{
				}
			}
			else
			{
				cpCategory = item as CollapsablePanel;
				categoryGrid = (Grid)cpCategory.ExpandedPanel.Controls[0];
			}
			UpdateCategoryRow(owner, cpCategory, categoryGrid, item3);
			if (!GameData.Instance.UpgraderEntityTypesByUpgradeCategory.TryGetValue(item3, out var value))
			{
				continue;
			}
			foreach (EntityType item4 in value)
			{
				if (!categoryGrid.TryGetEntry(item4, out var item2))
				{
					categoryGrid.BeginAddingEntries();
					item2 = AddItemRow(categoryGrid, item3, item4, owner);
					if (!categoryGridsThatWereAddedTo.Contains(categoryGrid))
					{
						categoryGridsThatWereAddedTo.Add(categoryGrid);
					}
				}
				UpdateItemRow(item2, item3, item4, entityData, owner, fillUserControls);
				Common.AddToList(ref includedEntityTypes, item4);
			}
		}
		foreach (CollapsablePanel entry in outerGrid.Entries)
		{
			((Grid)entry.ExpandedPanel.Controls[0]).DeleteEntries((EntityType e) => includedEntityTypes != null && includedEntityTypes.Contains(e));
		}
		List<object> list = null;
		foreach (object key in outerGrid.EntriesByKey.Keys)
		{
			if (((Grid)(outerGrid.EntriesByKey[key] as CollapsablePanel).ExpandedPanel.Controls[0]).Entries.Count == 0 || !upgradeOptions.Contains(key))
			{
				Common.AddToList(ref list, key);
			}
		}
		if (list != null)
		{
			foreach (object item5 in list)
			{
				outerGrid.RemoveEntry(item5);
			}
		}
		InventoryPanel.DoCategorySorting(outerGrid, categoryGridsThatWereAddedTo, Grid.Sorting.Ascending);
		foreach (Grid item6 in categoryGridsThatWereAddedTo)
		{
			item6.EndAddingEntries();
		}
		outerGrid.EndAddingEntries();
	}

	public void Fill(EntityID entityID, bool fillUserControls = false)
	{
		this.entityID = entityID;
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out var data);
		if (data == null)
		{
			Hide();
			return;
		}
		string zoneName = "";
		if (data != null)
		{
			zoneName = data.GetDisplayName().ToUpper(Config.Culture);
		}
		SetDisplayName(zoneName, lblName, lblHeader, headerIcon);
		Populate(fillUserControls);
	}

	private void FillToplevelControls(Stockpile stockpile)
	{
	}

	private void UpdateItemRow(UIComponent itemRow, UpgradeCategory category, EntityType entityType, IKnownEntityData entityData, EntityGroup owner, bool fillUserControls)
	{
		bool hasInputs;
		bool hasTools;
		int maxAmountThatCanBeProduced;
		int? noOfMissingInputTypes;
		int? noOfAvailableInputTypes;
		bool hasSkills;
		bool hasResources;
		bool hasSpecialSite;
		bool hasPolicy;
		EntityType needsImmovableInput;
		int? outputBatchAmount;
		ProcessType processType;
		bool bestProcessForDisplay = InventoryPanel.GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, out processType, includeSalvageProcesses: false, (ProcessType p) => p.IsUpgrade);
		_ = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
		HorizontalList horizontalList = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
		HorizontalList horizontalList2 = itemRow.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;
		Label label = itemRow.FindChildById(UIComponent.DataControlID.MaxOrders) as Label;
		CheckBox checkBox = itemRow.FindChildById(UIComponent.DataControlID.Selector) as CheckBox;
		if (fillUserControls)
		{
			bool isChecked = false;
			GetItemSetting(owner, category, entityType, out isChecked);
			checkBox.IsChecked = isChecked;
		}
		bool flag = false;
		if (entityData.ContainedUpgrades != null && entityData.ContainedUpgrades.TryGetValue(category, out var value) && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(value, out var data)) && data.EntityType == entityType)
		{
			flag = true;
		}
		if (!bestProcessForDisplay && !flag)
		{
			ProductionOrderControl.UpdateAttainable(The.InGameUI.InventorySettings.GetAttainableInfo(entityType), horizontalList2, horizontalList, hasTools, hasInputs, hasSkills, hasResources, bestProcessForDisplay, processType);
			label.Visible = false;
			return;
		}
		label.Visible = true;
		if (flag)
		{
			label.Text = "INSTALLED";
			label.ToolTip = "The upgrade is installed.";
		}
		else
		{
			label.Text = "AVAILABLE";
			label.ToolTip = "The upgrade can be installed now.";
		}
		label.FitToText();
		horizontalList2.Visible = false;
		horizontalList.Visible = false;
	}

	private void GetItemSetting(EntityGroup owner, UpgradeCategory category, EntityType entityType, out bool isChecked)
	{
		isChecked = owner.GetIsUpgrade(entityID, category, entityType);
	}

	public void ResourceClicked(UIComponent sender, EventArgs eventArgs)
	{
	}

	public void SaveJobChanges(ResourceType resourceType, int noOfJobs)
	{
	}
}
