using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class EntityDataSheet : DataSheet
{
	protected Bar coloredBar;

	protected ImageButton btTrack;

	private const string trackingButtonTooltip = "Toggle tracking this item";

	private const float edibleIndex = 10f;

	private Label lblHumanEdible;

	private UIComponent humanEdibleHeader;

	private const float nutritionIndex = 15f;

	private Grid grdNutrition;

	private Label lblNutrition;

	private UIComponent nutritionHeader;

	private const float comfortIndex = 20f;

	private Label lblComfort;

	private UIComponent comfortHeader;

	private const float replenishIndex = 30f;

	private UIComponent replenishHeader;

	private Grid grdRequiresReplenish;

	private const float upgradeForIndex = 40f;

	private UIComponent upgradeForHeader;

	private Grid grdUpgradeFor;

	private const float upgradesIndex = 50f;

	private UIComponent possibleUpgradesHeader;

	private Grid grdPossibleUpgrades;

	private const float effectsIndex = 60f;

	private UIComponent effectsHeader;

	private Grid grdEffects;

	private const float storageIndex = 80f;

	private UIComponent storageHeader;

	private Grid grdStorage;

	private const float ammoIndex = 90f;

	private UIComponent ammoHeader;

	private Grid grdAmmo;

	private const float weaponIndex = 100f;

	private Label lblWeapon;

	private UIComponent weaponHeader;

	private Label lblWeaponDefenseRating;

	private Label lblWeaponNotWieldable;

	private TextArea taHighlyEffective;

	private const float durabilityIndex = 120f;

	private UIComponent durabilityHeader;

	private Label lblDegradeType;

	private Grid grdDurability;

	private const float ordersIndex = 0f;

	private const float usedInIndex = 80f;

	private const float toolUsedForIndex = 100f;

	private const float toolUsedForCapMessageIndex = 105f;

	private int processIndex;

	private int noOfProcesses;

	protected ImageButton btSelectProcess;

	protected Label lblProcessIndex;

	private UIComponent settingsHeader;

	protected Label lblSettings;

	private ProductionOrderControl productionOrderControl;

	private Grid grdUsedIn;

	private UIComponent usedInHeader;

	private Label lblUsedIn;

	private Grid grdToolUsedFor;

	private UIComponent toolUsedForHeader;

	private Label lbltoolUsedFor;

	private Label lbltoolUsedForCapNotice;

	private EntityType entityType;

	private EntityTypeTooltipInstanceData instanceData;

	private static Color blue = "0FF8FD".ColorFromHex();

	private static Color brown = "CEB57D".ColorFromHex();

	private static Color green = "02CC6D".ColorFromHex();

	private static Color lightgreen = "70CC7D".ColorFromHex();

	private static Color foodColorGreen = "59C24E".ColorFromHex();

	private static Color securityColorBlue = "69C2E5".ColorFromHex();

	private static Color comfortColorPink = "CC82B7".ColorFromHex();

	private const int fixedGridItemHeight = 18;

	private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private readonly Color outOfStockColorForToolTip = "CCC86E".ColorFromHex();

	public EntityType EntityType => entityType;

	public EntityDataSheet()
	{
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			coloredBar = CreateTrackedColorBar(145);
			Add(coloredBar);
			coloredBar.Y = 4;
			btTrack = new ImageButton(gui);
			Add(btTrack);
			btTrack.Init(ImageButtonType.HUDCrosshair);
			btTrack.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
			btTrack.X = btClose.X;
			btTrack.Y = btClose.Bottom + 5;
			btTrack.Click += btTrack_Click;
			btTrack.ToolTip = "Toggle tracking this item";
		}
		btSelectProcess = new ImageButton(gui);
		Add(btSelectProcess);
		btSelectProcess.Init(ImageButtonType.HUDArrowRight);
		btSelectProcess.CheckedMode = CheckedModes.CannotBeChecked;
		btSelectProcess.X = DisplayWindow.Width - 12 - btSelectProcess.Width;
		btSelectProcess.Y = expandedPanelHeadingY;
		btSelectProcess.Click += btProcess_Click;
		btSelectProcess.ToolTip = "Click to view the next production process";
		btSelectProcess.DebugTag = "btProcess";
		btSelectProcess.NormalColor = DataSheet.productionColor;
		lblProcessIndex = new Label(gui);
		lblProcessIndex.Init(Label.LabelType.HUDWindow);
		Add(lblProcessIndex);
		lblProcessIndex.Y = expandedPanelHeadingY;
		lblProcessIndex.Text = "1/1";
		lblProcessIndex.FitToText();
		lblProcessIndex.X = btSelectProcess.X - lblProcessIndex.Width - 6;
		lblProcessIndex.NormalColor = DataSheet.productionColor;
	}

	private void btOpenManager_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ShowInventoryPanel();
	}

	private void btProcess_Click(UIComponent sender, EventArgs e)
	{
		int num = processIndex + 1;
		if (num >= noOfProcesses)
		{
			num = 0;
		}
		SelectProcess(num);
		Refresh();
	}

	public void Fill(EntityType entityType, EntityID? entityID)
	{
		if (entityID.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data);
			if (data != null)
			{
				instanceData = data.TooltipEntityData;
			}
			else
			{
				instanceData = null;
			}
		}
		else
		{
			instanceData = null;
		}
		if (this.entityType != entityType)
		{
			this.entityType = entityType;
			if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value))
			{
				noOfProcesses = value.Count;
			}
			else
			{
				noOfProcesses = 0;
			}
			SelectProcess(0);
			if (productionOrderControl != null)
			{
				grdProductionOuter.RemoveEntry(productionOrderControl);
				productionOrderControl = null;
			}
			CreateOrders();
			Fill();
		}
	}

	private void SelectProcess(int index)
	{
		processIndex = index;
		PopulateProcessSelector();
	}

	private void PopulateProcessSelector()
	{
		if (noOfProcesses > 1)
		{
			lblProcessIndex.Text = $"{processIndex + 1}/{noOfProcesses}";
			btSelectProcess.Visible = true;
			lblProcessIndex.Visible = true;
		}
		else
		{
			btSelectProcess.Visible = false;
			lblProcessIndex.Visible = false;
		}
	}

	protected override void PopulatePolicy()
	{
		base.PopulatePolicy();
		if (entityType.TierOrAreaType != null)
		{
			grdProductionOuter.TryRemoveEntry(policyHeader);
			ShowPolicyArea(entityType.TierOrAreaType);
		}
	}

	private void SetMarginAndWidth(UIComponent component)
	{
		component.X = SideMarginOutsideGrid();
		component.Width = grdProductionOuter.Width - component.X - 15;
	}

	protected override void CreateGeneralPanelContents()
	{
		humanEdibleHeader = AddSubHeader(grdGeneralOuter, "EDIBLE BY HUMANS:", out lblHumanEdible, addToGrid: true, foodColorGreen);
		humanEdibleHeader.OrderByTag1 = 10f;
		nutritionHeader = AddSubHeader(grdGeneralOuter, "POTENTIAL NUTRITIONAL CONTENT:", out lblNutrition, addToGrid: true, foodColorGreen);
		nutritionHeader.OrderByTag1 = 15f;
		grdNutrition = CreateSubGrid();
		grdNutrition.OrderByTag1 = 16f;
		weaponHeader = AddSubHeader(grdGeneralOuter, "WEAPON DATA:", out lblWeapon, addToGrid: true, securityColorBlue);
		weaponHeader.OrderByTag1 = 100f;
		lblWeaponNotWieldable = new Label(gui);
		lblWeaponNotWieldable.Init(Label.LabelType.EntityTypeTooltip);
		lblWeaponNotWieldable.Text = "NOT WIELDABLE";
		lblWeaponNotWieldable.ToolTip = "The weapon is a part of a structure/robot and cannot be used directly by characters.";
		lblWeaponNotWieldable.FitToText();
		lblWeaponNotWieldable.X = SideMarginOutsideGrid();
		lblWeaponNotWieldable.OrderByTag1 = 101f;
		grdGeneralOuter.AddEntry(lblWeaponNotWieldable, lblWeaponNotWieldable);
		lblWeaponDefenseRating = new Label(gui);
		lblWeaponDefenseRating.Init(Label.LabelType.EntityTypeTooltip);
		lblWeaponDefenseRating.FitToText();
		lblWeaponDefenseRating.X = SideMarginOutsideGrid();
		lblWeaponDefenseRating.OrderByTag1 = 102f;
		grdGeneralOuter.AddEntry(lblWeaponDefenseRating, lblWeaponDefenseRating);
		taHighlyEffective = new TextArea(gui, ListBoxType.HUDAndLCD);
		grdGeneralOuter.AddEntry(taHighlyEffective, taHighlyEffective);
		taHighlyEffective.Init(Label.LabelType.HUDWindow);
		taHighlyEffective.CanGrowInHeight = true;
		taHighlyEffective.ScrollBarEnabled = false;
		SetMarginAndWidth(taHighlyEffective);
		taHighlyEffective.OrderByTag1 = 103f;
		ammoHeader = AddSubHeader(grdGeneralOuter, "AMMUNITION:", out var lbl, addToGrid: true, securityColorBlue);
		ammoHeader.OrderByTag1 = 90f;
		lbl.ToolTip = "The following ammunition type is required. \nNOTE: A ranged weapon will not count in the colony Security rating unless it has ammunition available.";
		grdAmmo = CreateFixedItemHeightGrid();
		grdAmmo.OrderByTag1 = 91f;
		storageHeader = AddSubHeader(grdGeneralOuter, "STORAGE:", out lbl, addToGrid: false, brown);
		storageHeader.OrderByTag1 = 80f;
		lbl.ToolTip = "The storage conditions and capacity offered by this object";
		grdStorage = CreateFixedItemHeightGrid(18);
		grdStorage.OrderByTag1 = 81f;
		replenishHeader = AddSubHeader(grdGeneralOuter, "FUEL/ENERGY:", out lbl, addToGrid: true, brown);
		replenishHeader.OrderByTag1 = 30f;
		lbl.ToolTip = "One of the following fuel or energy types is required";
		grdRequiresReplenish = CreateFixedItemHeightGrid();
		grdRequiresReplenish.OrderByTag1 = 31f;
		upgradeForHeader = AddSubHeader(grdGeneralOuter, "UPGRADE FOR:", out lbl, addToGrid: false);
		upgradeForHeader.OrderByTag1 = 40f;
		lbl.ToolTip = "The following structures can be upgraded with this item (using the UPGRADE action)";
		grdUpgradeFor = CreateFixedItemHeightGrid();
		grdUpgradeFor.OrderByTag1 = 41f;
		possibleUpgradesHeader = AddSubHeader(grdGeneralOuter, "UPGRADE OPTIONS:", out lbl, addToGrid: false, lightgreen);
		possibleUpgradesHeader.OrderByTag1 = 50f;
		lbl.ToolTip = "The structure has these optional upgrades (using the UPGRADE action)";
		grdPossibleUpgrades = CreateFixedItemHeightGrid();
		grdPossibleUpgrades.OrderByTag1 = 51f;
		durabilityHeader = AddSubHeader(grdGeneralOuter, "DURABILITY:", out lbl, addToGrid: false, blue);
		durabilityHeader.OrderByTag1 = 120f;
		lbl.ToolTip = "Shows how long the object will last under different conditions. \nStructures can have their lifespan extended with regular maintenance. Items CANNOT.";
		lblDegradeType = new Label(gui);
		lblDegradeType.Init(Label.LabelType.EntityTypeTooltip);
		lblDegradeType.FitToText();
		lblDegradeType.X = SideMarginOutsideGrid();
		lblDegradeType.OrderByTag1 = 121f;
		grdDurability = CreateFixedItemHeightGrid(18);
		grdDurability.OrderByTag1 = 122f;
		effectsHeader = AddSubHeader(grdGeneralOuter, "EFFECTS:", out lbl, addToGrid: false, green);
		effectsHeader.OrderByTag1 = 60f;
		lbl.ToolTip = "The resulting effects";
		grdEffects = CreateFixedItemHeightGrid();
		grdEffects.ItemHeight = 18;
		grdEffects.OrderByTag1 = 61f;
		comfortHeader = AddSubHeader(grdGeneralOuter, "COMFORT:", out lblComfort, addToGrid: true, comfortColorPink);
		lblComfort.ToolTip = "The base comfort level when in perfect condition, and without upgrades";
		comfortHeader.OrderByTag1 = 20f;
	}

	private Grid CreateSubGrid()
	{
		int num = 18;
		Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.EntityTypeTooltip);
		grid.IsOuterGrid = false;
		grid.FixedItemHeights = true;
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = num;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Height = 160;
		SetMarginAndWidth(grid);
		grdGeneralOuter.AddEntry(grid, grid);
		return grid;
	}

	private void TooltipPadlock_Click(UIComponent sender, EventArgs e)
	{
		Expedition expedition = The.InGameUI.GetExpedition();
		if (expedition != null)
		{
			ProductionOrderControl.Padlock_Click(sender, e);
			EntityGroup ownedEntities = expedition.OwnedEntities;
			PopulateOrdersRefresh(ownedEntities);
		}
	}

	private void PopulateOrdersRefresh(EntityGroup owner)
	{
		allAvailableItems.Clear();
		InventoryPanel.CountItems(owner.AllEntities, owner, allAvailableItems, isOwnedByOtherAllegiance: false, entityType);
		productionOrderControl.UpdateOrders(owner, allAvailableItems, out var _);
	}

	public override void OnSetProduction()
	{
		productionOrderControl.ResetSliderBeingDragged();
		ResolveOwner(out var resolvedOwner);
		PopulateOrdersRefresh(resolvedOwner);
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		StockButton stockButton = sender as StockButton;
		The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList);
		The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
	}

	protected override void CreateProductionPanelContents()
	{
		settingsHeader = AddSubHeader(grdProductionOuter, "IN STOCK / ORDERS:", out lblSettings);
		settingsHeader.OrderByTag1 = 0f;
		CreateGridAndHeader(grdProductionOuter, out usedInHeader, "USED IN:", "Used as a material in these objects", 80f, out grdUsedIn, out lblUsedIn);
		CreateGridAndHeader(grdProductionOuter, out toolUsedForHeader, "USED FOR:", "Used as a tool for making these objects", 100f, out grdToolUsedFor, out lbltoolUsedFor);
		lbltoolUsedForCapNotice = new Label(gui);
		lbltoolUsedForCapNotice.Init(Label.LabelType.HUDWindow);
		lbltoolUsedForCapNotice.Text = "...Used for more objects than shown!";
		lbltoolUsedForCapNotice.FitToText();
		lbltoolUsedForCapNotice.OrderByTag1 = 105f;
		lbltoolUsedForCapNotice.X = 35;
	}

	private void CreateOrders()
	{
		ProductionTargetEventArgs eventArgs = new ProductionTargetEventArgs(entityType, null);
		productionOrderControl = new ProductionOrderControl(entityType, eventArgs, DisplayWindow.guiManager, ProductionOrderControl.UILayout.HUD, 83, tbItems_Click, TooltipPadlock_Click);
		productionOrderControl.X = 11;
		grdProductionOuter.AddEntry(productionOrderControl, productionOrderControl);
		productionOrderControl.OrderByTag1 = 1f;
	}

	public override void Hide()
	{
		base.Hide();
		productionOrderControl.ResetSliderBeingDragged();
	}

	private void btTrack_Click(UIComponent sender, EventArgs e)
	{
		EntityType entityType = (EntityType)((ImageButton)sender).Tag1;
		The.InGameUI.InventorySettings.ToggleTracking(entityType);
	}

	private Color GetStockButtonColor(EntityType entityType, bool isToolContext)
	{
		ResolveOwner(out var resolvedOwner);
		if (resolvedOwner == null)
		{
			resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		}
		return DataTypeButton.GetStockStatusColor(GetIsAvailable(entityType, isToolContext, resolvedOwner), Color.White, TextButton.TextButtonType.LCDToolTipBlack);
	}

	private static bool GetIsAvailable(EntityType entityType, bool isToolContext, EntityGroup resolvedOwner)
	{
		int noOfAvailableItemsIncludingIntrinsic;
		int noOfAvailableEntities = GetNoOfAvailableEntities(entityType, resolvedOwner, out noOfAvailableItemsIncludingIntrinsic);
		if (isToolContext)
		{
			return noOfAvailableItemsIncludingIntrinsic > 0;
		}
		return noOfAvailableEntities > 0;
	}

	private static int GetNoOfAvailableEntities(EntityType entityType, EntityGroup resolvedOwner, out int noOfAvailableItemsIncludingIntrinsic)
	{
		int noOfIncompleteEntities;
		int noOfEntitiesUsedAsParts;
		int noOfItemsOnOtherSite;
		int noOfItemsOwnedByOthers;
		return InventoryPanel.GetNoOfAvailableEntities(resolvedOwner.AllEntities, resolvedOwner, entityType, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic);
	}

	protected override void RefreshCollapsedFields()
	{
		base.RefreshCollapsedFields();
		RefreshHeader();
	}

	protected override void RefreshTrackTargets()
	{
		base.RefreshTrackTargets();
		RefreshTrackButton();
		DataSheet.RefreshTrackedColorBar(entityType, coloredBar);
	}

	private void RefreshTrackButton()
	{
		if (The.InGameUI.InventorySettings.TrackedTargets.TryGetValue(entityType, out var _))
		{
			btTrack.IsChecked = true;
			return;
		}
		btTrack.IsChecked = false;
		if (!The.InGameUI.InventorySettings.HasAvailableTrackingSlots())
		{
			btTrack.Enabled = false;
			btTrack.ToolTip = "No more objects can be tracked, cancel some of the other tracked objects first.";
		}
		else
		{
			btTrack.Enabled = true;
			btTrack.ToolTip = "Toggle tracking this item";
		}
	}

	protected override void PopulateCollapsedFieldsContents()
	{
		lblName.Text = entityType.Name;
		lblName.ToolTip = entityType.Name;
		RefreshHeader();
		if (GameData.Instance.GUIConstants.EnableFilters)
		{
			RefreshTrackTargets();
			coloredBar.X = lblName.Right + 5;
			btTrack.Tag1 = entityType;
		}
		summaryDescription.Text = entityType.SummaryDescription;
		if (entityType.ItemType != null)
		{
			IconInfo iconInfo;
			Rectangle iconSprite = entityType.GetIconSprite(out iconInfo);
			icon.SetSkinLocation(SkinState.Normal, iconSprite);
			icon.ResizeControlToFitImage();
			icon.Visible = true;
		}
		else if (entityType.ThumbnailSmall != null)
		{
			Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle(entityType.ThumbnailSmall);
			icon.SetSkinLocation(SkinState.Normal, sourceRectangle);
			icon.ResizeControlToFitImage();
			icon.Visible = true;
		}
	}

	private void RefreshHeader()
	{
		lblName.NormalColor = GetStockButtonColor(entityType, entityType.IsIntrinsic());
		if (lblName.NormalColor != Color.White)
		{
			lblName.NormalColor = outOfStockColorForToolTip;
		}
	}

	protected override void PopulateGeneralDataContent()
	{
		btSelectProcess.Visible = false;
		lblProcessIndex.Visible = false;
		PopulateWeaponData();
		PopulateNutritionData();
		PopulateComfortData();
		PopulateReplenishTypeData();
		PopulateUpgradeForData();
		PopulatePossibleUpgradesData();
		PopulateDurationData();
		PopulateStorageData();
		PopulateEffectsData();
	}

	protected override void PopulateGeneralDataContentRefresh()
	{
		PopulateAmmo();
		PopulateUpgradeForDataRefresh();
		PopulatePossibleUpgradesDataRefresh();
		PopulateReplenishTypeDataRefresh();
	}

	protected override string GetDescription()
	{
		if (instanceData != null)
		{
			if (instanceData.RaceTypeDescription != null)
			{
				return instanceData.Description + instanceData.RaceTypeDescription;
			}
			return instanceData.Description + entityType.Description;
		}
		return entityType.Description;
	}

	private void PopulateComfortData()
	{
		grdGeneralOuter.TryRemoveEntry(comfortHeader);
		if (entityType.ContainerType != null && entityType.ContainerType is HomeContainerType { ResidenceType: not null } homeContainerType)
		{
			grdGeneralOuter.AddEntry(comfortHeader, comfortHeader);
			lblComfort.Text = "COMFORT LEVEL: " + Common.PercentageToString(homeContainerType.ResidenceType.ComfortLevel);
		}
	}

	private void PopulateWeaponData()
	{
		grdGeneralOuter.TryRemoveEntry(weaponHeader);
		grdGeneralOuter.TryRemoveEntry(taHighlyEffective);
		grdGeneralOuter.TryRemoveEntry(lblWeaponDefenseRating);
		grdGeneralOuter.TryRemoveEntry(grdAmmo);
		grdGeneralOuter.TryRemoveEntry(ammoHeader);
		grdGeneralOuter.TryRemoveEntry(lblWeaponNotWieldable);
		WeaponType weaponType = GetWeaponType();
		if (weaponType != null)
		{
			_ = GameData.Instance.AIConstants.Ratings.Security;
			grdGeneralOuter.AddEntry(weaponHeader, weaponHeader);
			grdGeneralOuter.AddEntry(lblWeaponDefenseRating, lblWeaponDefenseRating);
			if (weaponType.IsIntrinsic == true)
			{
				grdGeneralOuter.AddEntry(lblWeaponNotWieldable, lblWeaponNotWieldable);
			}
			float highestDefenseRating = weaponType.GetHighestDefenseRating();
			float num = highestDefenseRating;
			lblWeaponDefenseRating.Text = $"SECURITY RATING: {num:N2} ({DefenseRatingToString(highestDefenseRating)})";
			lblWeaponDefenseRating.ToolTip = "This number is used in the colony's Security rating. The more highly rated weapons the colony has (up to 2 per person), the higher the colony's Security rating";
			if (GetMagazineType() != null)
			{
				grdGeneralOuter.AddEntry(ammoHeader, ammoHeader);
				grdGeneralOuter.AddEntry(grdAmmo, grdAmmo);
				PopulateAmmo();
			}
			grdGeneralOuter.AddEntry(taHighlyEffective, taHighlyEffective);
			taHighlyEffective.Text = "EFFECTIVE AGAINST: " + weaponType.HighlyEffectiveAgainst;
		}
	}

	private WeaponType GetWeaponType()
	{
		if (entityType.ItemType != null && entityType.ItemType.WeaponType != null)
		{
			return entityType.ItemType.WeaponType;
		}
		if (entityType.IntelligenceType != null && entityType.IntelligenceType.IntrinsicWeaponTypes != null && entityType.IntelligenceType.IntrinsicWeaponTypes.Count > 0)
		{
			return entityType.IntelligenceType.IntrinsicWeaponTypes[0].ItemType.WeaponType;
		}
		return null;
	}

	private void PopulateStorageData(ItemStorageType itemStorageType, bool isTrade, ref List<string> keys)
	{
		foreach (KeyValuePair<string, StorageType> storageSpace in itemStorageType.StorageSpaces)
		{
			string text = CreateStorageKey(isTrade, storageSpace.Key);
			Common.AddToList(ref keys, text);
			StorageCondition storageCondition = GameData.Instance.AllStorageConditions[storageSpace.Key];
			if (!grdStorage.TryGetEntry(text, out var item))
			{
				if (!Common.IsGreaterThan(storageSpace.Value.Capacity, 0f))
				{
					continue;
				}
				item = AddStorageRow(text, storageCondition, storageSpace.Value, isTrade);
			}
			UpdateStorageRow(item, storageSpace.Value);
		}
	}

	private static string CreateStorageKey(bool isTrade, string storageTypeKey)
	{
		if (isTrade)
		{
			return "trade" + storageTypeKey;
		}
		return storageTypeKey;
	}

	private void PopulateStorageDataOnce(IHasItemStorageType hasItemStorage, TerminalContainerType terminalContainerType)
	{
		grdStorage.BeginAddingEntries();
		List<string> keys = null;
		if (hasItemStorage != null)
		{
			PopulateStorageData(hasItemStorage.ItemStorageType, isTrade: false, ref keys);
		}
		if (terminalContainerType != null)
		{
			PopulateStorageData(terminalContainerType.OfferedForTradeStorageType, isTrade: true, ref keys);
		}
		grdStorage.DeleteEntries((object e) => keys.Contains((string)e));
		grdStorage.EndAddingEntries();
	}

	private void PopulateAmmo()
	{
		grdAmmo.BeginAddingEntries();
		MagazineContainerType magazineType = GetMagazineType();
		if (magazineType != null)
		{
			ResolveOwner(out var resolvedOwner);
			foreach (EntityType ammoEntityType in magazineType.AmmoEntityTypes)
			{
				if (!grdAmmo.TryGetEntry(ammoEntityType, out var item))
				{
					item = AddEntityTypeItemRow(grdAmmo, ammoEntityType);
				}
				UpdateItemRow(ammoEntityType, item, resolvedOwner);
			}
		}
		grdAmmo.DeleteEntries((EntityType e) => magazineType != null && magazineType.AmmoEntityTypes.Contains(e));
		grdAmmo.EndAddingEntries();
	}

	private MagazineContainerType GetMagazineType()
	{
		MagazineContainerType magazineContainerType = null;
		if (entityType.IntelligenceType != null && entityType.IntelligenceType.IntrinsicWeaponTypes != null)
		{
			foreach (EntityType intrinsicWeaponType in entityType.IntelligenceType.IntrinsicWeaponTypes)
			{
				if (intrinsicWeaponType.ContainerType != null && intrinsicWeaponType.ContainerType is MagazineContainerType)
				{
					magazineContainerType = intrinsicWeaponType.ContainerType as MagazineContainerType;
					break;
				}
			}
		}
		if (magazineContainerType == null)
		{
			magazineContainerType = entityType.ContainerType as MagazineContainerType;
		}
		return magazineContainerType;
	}

	private UIComponent AddEntityTypeItemRow(Grid grid, EntityType inputEntityType)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(inputEntityType, uIComponent);
		CreateItemGridRow(inputEntityType, uIComponent, out var entityTypeButton);
		uIComponent.CenterChildVertically(entityTypeButton);
		return uIComponent;
	}

	private UIComponent AddEffectRow(Grid grid, EffectType effectType, out string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		effectType.AppendAsString(stringBuilder, EffectType.Background.EntityTypeTooltipGreen);
		text = stringBuilder.ToString();
		return grid.AddEntry(effectType, text, useLineBreaks: false, null, 5);
	}

	private UIComponent AddAmmoItemRow(EntityType inputEntityType)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grdAmmo.AddEntry(inputEntityType, uIComponent);
		CreateItemGridRow(inputEntityType, uIComponent, out var entityTypeButton);
		uIComponent.CenterChildVertically(entityTypeButton);
		return uIComponent;
	}

	private void UpdateItemRow(EntityType item, UIComponent itemRow, EntityGroup resolvedOwner, bool isToolContext = false)
	{
		DataTypeButton obj = (DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption);
		bool isAvailable = GetIsAvailable(entityType, isToolContext, resolvedOwner);
		obj.SetAvailableStatusColor(isAvailable);
	}

	private string DefenseRatingToString(float rating)
	{
		if (rating == 0f)
		{
			return "None";
		}
		if (rating <= 0.05f)
		{
			return "Very low";
		}
		if (rating <= 0.15f)
		{
			return "Low";
		}
		if (rating <= 0.25f)
		{
			return "Middle";
		}
		if (rating <= 0.55f)
		{
			return "High";
		}
		return "Highest";
	}

	private void PopulateDurationData()
	{
		List<StorageDuration> list = null;
		if (entityType.NonLivingType != null)
		{
			list = entityType.NonLivingType.GetRepresentativeStorageConditions(entityType);
		}
		if (list != null && list.Count > 0)
		{
			if (!grdGeneralOuter.TryGetEntry(durabilityHeader, out var _))
			{
				grdGeneralOuter.AddEntry(durabilityHeader, durabilityHeader);
				grdGeneralOuter.AddEntry(lblDegradeType, lblDegradeType);
				grdGeneralOuter.AddEntry(grdDurability, grdDurability);
			}
			PopulateDurationDataOnce(list);
		}
		else
		{
			grdGeneralOuter.TryRemoveEntry(durabilityHeader);
			grdGeneralOuter.TryRemoveEntry(lblDegradeType);
			grdGeneralOuter.TryRemoveEntry(grdDurability);
		}
	}

	private void PopulateStorageData()
	{
		IHasItemStorageType hasItemStorageType = null;
		TerminalContainerType terminalContainerType = null;
		if (entityType.ContainerType != null)
		{
			hasItemStorageType = entityType.ContainerType as IHasItemStorageType;
			terminalContainerType = entityType.ContainerType as TerminalContainerType;
		}
		if (hasItemStorageType != null || terminalContainerType != null)
		{
			if (!grdGeneralOuter.TryGetEntry(storageHeader, out var _))
			{
				grdGeneralOuter.AddEntry(storageHeader, storageHeader);
				grdGeneralOuter.AddEntry(grdStorage, grdStorage);
			}
			PopulateStorageDataOnce(hasItemStorageType, terminalContainerType);
		}
		else
		{
			grdGeneralOuter.TryRemoveEntry(storageHeader);
			grdGeneralOuter.TryRemoveEntry(grdStorage);
		}
	}

	private void PopulateEffectsData()
	{
		List<EffectProfileType> effectProfiles = entityType.GetEffectProfiles();
		if (effectProfiles != null && effectProfiles.Count > 0)
		{
			if (!grdGeneralOuter.TryGetEntry(effectsHeader, out var _))
			{
				grdGeneralOuter.AddEntry(effectsHeader, effectsHeader);
				grdGeneralOuter.AddEntry(grdEffects, grdEffects);
			}
			PopulateEffectsDataOnce(effectProfiles);
		}
		else
		{
			grdGeneralOuter.TryRemoveEntry(effectsHeader);
			grdGeneralOuter.TryRemoveEntry(grdEffects);
		}
	}

	private void PopulatePossibleUpgradesData()
	{
		if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
		{
			if (!grdGeneralOuter.TryGetEntry(possibleUpgradesHeader, out var _))
			{
				grdGeneralOuter.AddEntry(possibleUpgradesHeader, possibleUpgradesHeader);
				grdGeneralOuter.AddEntry(grdPossibleUpgrades, grdPossibleUpgrades);
			}
			PopulatePossibleUpgradesDataRefresh();
		}
		else
		{
			grdGeneralOuter.TryRemoveEntry(possibleUpgradesHeader);
			grdGeneralOuter.TryRemoveEntry(grdPossibleUpgrades);
		}
	}

	private void PopulateUpgradeForData()
	{
		if (entityType.Upgrader != null)
		{
			if (!grdGeneralOuter.TryGetEntry(upgradeForHeader, out var _))
			{
				grdGeneralOuter.AddEntry(upgradeForHeader, upgradeForHeader);
				grdGeneralOuter.AddEntry(grdUpgradeFor, grdUpgradeFor);
			}
			PopulateUpgradeForDataRefresh();
		}
		else
		{
			grdGeneralOuter.TryRemoveEntry(upgradeForHeader);
			grdGeneralOuter.TryRemoveEntry(grdUpgradeFor);
		}
	}

	private UIComponent AddStorageRow(object key, StorageCondition storageCondition, StorageType storageType, bool isTrade)
	{
		string text = storageCondition.Name;
		if (isTrade)
		{
			text += " (trading)";
		}
		UIComponent uIComponent = grdStorage.AddEntryRightJustifyValue(key, null, null, 5, text, 18, Common.DecimalToStringSignificant(storageType.Capacity), "The storage capacity in BLK", storageCondition.Description);
		uIComponent.OrderByTag1 = storageCondition.Name;
		return uIComponent;
	}

	private void UpdateStorageRow(UIComponent itemRow, StorageType storageType)
	{
		itemRow.FindChildById<Label>(UIComponent.DataControlID.Value, out var child, firstLevelOnly: false);
		child.Text = Common.DecimalToStringSignificant(storageType.Capacity);
	}

	private UIComponent AddDurationRow(StorageDuration duration)
	{
		UIComponent uIComponent = grdDurability.AddEntryRightJustifyValue(duration, null, null, 5, duration.StorageDurationToDisplay.DisplayName, 18, Common.DecimalToStringSignificant(duration.Duration), "The number of days the object will last when exposed to this condition. \nStructures can often have their lifespan extended with regular maintenance. Items CANNOT.", duration.StorageDurationToDisplay.Tooltip ?? duration.StorageCondition.Description);
		uIComponent.OrderByTag1 = duration.SortOrder;
		return uIComponent;
	}

	private void PopulateDurationDataOnce(List<StorageDuration> storageDurations)
	{
		if (storageDurations != null)
		{
			lblDegradeType.Text = "'" + entityType.NonLivingType.FinalDegradeType.Name + "'";
			lblDegradeType.FitToText();
			lblDegradeType.ToolTip = entityType.NonLivingType.FinalDegradeType.Description;
			ResolveOwner(out var _);
			grdDurability.BeginAddingEntries();
			foreach (StorageDuration storageDuration in storageDurations)
			{
				if (!grdDurability.TryGetEntry(storageDuration, out var item))
				{
					item = AddDurationRow(storageDuration);
				}
			}
			grdDurability.DeleteEntries((StorageDuration e) => storageDurations.Contains(e));
			grdDurability.Sort(Grid.Sorting.Descending, useFirstTag: true);
			grdDurability.EndAddingEntries();
		}
		else
		{
			lblDegradeType.Text = "";
			grdDurability.Clear();
		}
	}

	private void PopulateEffectsDataOnce(List<EffectProfileType> effectProfileTypes)
	{
		if (effectProfileTypes != null)
		{
			ResolveOwner(out var _);
			grdEffects.BeginAddingEntries();
			HashSet<EffectType> encounteredEffects = null;
			foreach (EffectProfileType effectProfileType2 in effectProfileTypes)
			{
				float num = (float)effectProfileType2.SortOrder * 500f;
				if (!grdEffects.TryGetEntry(effectProfileType2, out var item))
				{
					item = grdEffects.AddEntry(effectProfileType2, effectProfileType2.Name.ToUpper(Config.Culture), useLineBreaks: false, null, 5);
					item.OrderByTag1 = num;
				}
				foreach (EffectType effectType in effectProfileType2.EffectTypes)
				{
					if (!grdEffects.TryGetEntry(effectType, out var item2))
					{
						item2 = AddEffectRow(grdEffects, effectType, out var text);
						item2.OrderByTag1 = num + (float)(int)text[0];
					}
					Common.AddToList(ref encounteredEffects, effectType);
				}
			}
			Grid.DeleteEntriesWithMixedKeyTypes(grdEffects, (object e) => !(e is EffectType) || (encounteredEffects != null && encounteredEffects.Contains(e)));
			grdEffects.Sort(Grid.Sorting.Ascending, useFirstTag: true);
			List<object> list = null;
			EffectProfileType effectProfileType = null;
			foreach (UIComponent entry in grdEffects.Entries)
			{
				if (!(entry.Tag1 is EffectType))
				{
					if (effectProfileType != null)
					{
						Common.AddToList(ref list, effectProfileType);
					}
					effectProfileType = entry.Tag1 as EffectProfileType;
				}
				else
				{
					effectProfileType = null;
				}
			}
			if (grdEffects.Count > 0)
			{
				UIComponent uIComponent = grdEffects.Entries[grdEffects.Entries.Count - 1];
				if (uIComponent.Tag1 is EffectProfileType)
				{
					Common.AddToList(ref list, uIComponent.Tag1);
				}
			}
			if (list != null)
			{
				foreach (object item3 in list)
				{
					grdEffects.RemoveEntry(item3);
				}
			}
			grdEffects.EndAddingEntries();
		}
		else
		{
			grdEffects.Clear();
		}
	}

	private void PopulatePossibleUpgradesDataRefresh()
	{
		if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
		{
			ResolveOwner(out var resolvedOwner);
			grdPossibleUpgrades.BeginAddingEntries();
			HashSet<EntityType> encounteredUpgrades = null;
			foreach (UpgradeCategory upgradeOption in entityType.ContainerType.GetUpgradeOptions())
			{
				float num = (float)upgradeOption.SortOrder * 500f;
				if (!grdPossibleUpgrades.TryGetEntry(upgradeOption, out var item))
				{
					item = grdPossibleUpgrades.AddEntry(upgradeOption, upgradeOption.Name.ToUpper(Config.Culture), useLineBreaks: false, null, 5);
					item.OrderByTag1 = num;
				}
				if (!GameData.Instance.UpgraderEntityTypesByUpgradeCategory.TryGetValue(upgradeOption, out var value))
				{
					continue;
				}
				foreach (EntityType item3 in value)
				{
					if (!grdPossibleUpgrades.TryGetEntry(item3, out var item2))
					{
						item2 = AddEntityTypeItemRow(grdPossibleUpgrades, item3);
						item2.OrderByTag1 = num + (float)(int)item3.Name[0];
					}
					UpdateItemRow(item3, item2, resolvedOwner);
					Common.AddToList(ref encounteredUpgrades, item3);
				}
			}
			Grid.DeleteEntriesWithMixedKeyTypes(grdPossibleUpgrades, (object e) => !(e is EntityType) || (encounteredUpgrades != null && encounteredUpgrades.Contains(e)));
			grdPossibleUpgrades.Sort(Grid.Sorting.Ascending, useFirstTag: true);
			List<object> list = null;
			UpgradeCategory upgradeCategory = null;
			foreach (UIComponent entry in grdPossibleUpgrades.Entries)
			{
				if (!(entry.Tag1 is EntityType))
				{
					if (upgradeCategory != null)
					{
						Common.AddToList(ref list, upgradeCategory);
					}
					upgradeCategory = entry.Tag1 as UpgradeCategory;
				}
				else
				{
					upgradeCategory = null;
				}
			}
			if (grdPossibleUpgrades.Count > 0)
			{
				UIComponent uIComponent = grdPossibleUpgrades.Entries[grdPossibleUpgrades.Entries.Count - 1];
				if (uIComponent.Tag1 is UpgradeCategory)
				{
					Common.AddToList(ref list, uIComponent.Tag1);
				}
			}
			if (list != null)
			{
				foreach (object item4 in list)
				{
					grdPossibleUpgrades.RemoveEntry(item4);
				}
			}
			grdPossibleUpgrades.EndAddingEntries();
		}
		else
		{
			grdPossibleUpgrades.Clear();
		}
	}

	private void PopulateUpgradeForDataRefresh()
	{
		Upgrader upgrader = entityType.Upgrader;
		if (upgrader != null)
		{
			ResolveOwner(out var resolvedOwner);
			grdUpgradeFor.BeginAddingEntries();
			HashSet<EntityType> hashSet = new HashSet<EntityType>();
			foreach (UpgradeCategory item2 in upgrader.UpgradeCategoryFinal)
			{
				foreach (EntityType item3 in GameData.Instance.EntityTypesToUpgradeByUpgradeCategory[item2])
				{
					hashSet.Add(item3);
				}
			}
			foreach (EntityType item4 in hashSet)
			{
				if (!grdUpgradeFor.TryGetEntry(item4, out var item))
				{
					item = AddEntityTypeItemRow(grdUpgradeFor, item4);
				}
				UpdateItemRow(item4, item, resolvedOwner);
			}
			grdUpgradeFor.DeleteEntries((EntityType e) => upgrader != null && e.CanBeUpgradedBy(entityType));
			grdUpgradeFor.EndAddingEntries();
		}
		else
		{
			grdUpgradeFor.Clear();
		}
	}

	private RequiresReplenishType GetRequiresReplenishType()
	{
		if (entityType.ContainerType != null)
		{
			RequiresReplenishType requiresReplenishType = entityType.ContainerType.GetRequiresReplenishType();
			if (requiresReplenishType != null)
			{
				return requiresReplenishType;
			}
		}
		return null;
	}

	private void PopulateReplenishTypeData()
	{
		grdGeneralOuter.TryRemoveEntry(replenishHeader);
		grdGeneralOuter.TryRemoveEntry(grdRequiresReplenish);
		RequiresReplenishType requiresReplenishType = GetRequiresReplenishType();
		if (requiresReplenishType != null && requiresReplenishType.RequiresFuelType != null)
		{
			grdGeneralOuter.AddEntry(replenishHeader, replenishHeader);
			grdGeneralOuter.AddEntry(grdRequiresReplenish, grdRequiresReplenish);
			PopulateReplenishTypeDataRefresh();
		}
	}

	private void PopulateReplenishTypeDataRefresh()
	{
		RequiresReplenishType requiresReplenishType = GetRequiresReplenishType();
		if (requiresReplenishType != null)
		{
			grdRequiresReplenish.BeginAddingEntries();
			ResolveOwner(out var resolvedOwner);
			if (requiresReplenishType.RequiresFuelType != null)
			{
				foreach (EntityType fuelEntityType in requiresReplenishType.RequiresFuelType.FuelEntityTypes)
				{
					if (!grdRequiresReplenish.TryGetEntry(fuelEntityType, out var item))
					{
						item = AddEntityTypeItemRow(grdRequiresReplenish, fuelEntityType);
					}
					UpdateItemRow(fuelEntityType, item, resolvedOwner);
				}
			}
			grdRequiresReplenish.DeleteEntries((EntityType e) => requiresReplenishType.RequiresFuelType != null && requiresReplenishType.RequiresFuelType.FuelEntityTypes.Contains(e));
			grdRequiresReplenish.EndAddingEntries();
		}
		else
		{
			grdRequiresReplenish.Clear();
		}
	}

	private void PopulateNutritionData()
	{
		grdGeneralOuter.TryRemoveEntry(humanEdibleHeader);
		grdGeneralOuter.TryRemoveEntry(nutritionHeader);
		grdGeneralOuter.TryRemoveEntry(grdNutrition);
		if (entityType.ItemType == null || entityType.ItemType.FoodType == null)
		{
			return;
		}
		grdGeneralOuter.AddEntry(humanEdibleHeader, humanEdibleHeader);
		grdGeneralOuter.AddEntry(nutritionHeader, nutritionHeader);
		BiologicalType biologicalType = The.InGameUI.UIAllegiance.RepresentativeEntityType.BiologicalType;
		if (biologicalType != null)
		{
			string text = biologicalType.SpeciesPlural.ToUpper(Config.Culture);
			if (biologicalType.ConsumeProcesses.ContainsKey(entityType))
			{
				lblHumanEdible.Text = "EDIBLE TO " + text;
			}
			else
			{
				lblHumanEdible.Text = "INEDIBLE TO " + text + " IN THIS CONDITION";
			}
		}
		grdGeneralOuter.AddEntry(grdNutrition, grdNutrition);
		grdNutrition.BeginAddingEntries();
		grdNutrition.Clear();
		if (entityType.ItemType.MaximumBulk.HasValue)
		{
			float weight;
			NeedType[] adultNeedsAndWeight = The.InGameUI.UIAllegiance.RepresentativeEntityType.BiologicalType.GetAdultNeedsAndWeight(out weight);
			FoodNutrientAmount[] foodNutrientTypes = entityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes;
			foreach (FoodNutrientAmount foodNutrientAmount in foodNutrientTypes)
			{
				_ = foodNutrientAmount.Amount;
				string satisfiedDailyIntake = foodNutrientAmount.GetSatisfiedDailyIntake(entityType.ItemType.MaximumBulk.Value, adultNeedsAndWeight, weight);
				if (satisfiedDailyIntake != null)
				{
					grdNutrition.AddEntryRightJustifyValue(foodNutrientAmount.Nutrient.KeyName, null, null, null, foodNutrientAmount.Nutrient.Name, 0, satisfiedDailyIntake, "Of recommended daily intake for an adult human");
				}
			}
		}
		grdNutrition.EndAddingEntries();
	}

	protected override ProcessType GetProcessToShow()
	{
		ProcessType result = null;
		if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(entityType, out var value))
		{
			result = value.OrderByDescending((ProcessType p) => p.IsGathering ? 1 : 0).ToList()[processIndex];
		}
		return result;
	}

	private UIComponent AddUsedForItemRow(EntityType outputEntityType, float score)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grdToolUsedFor.AddEntry(outputEntityType, uIComponent);
		CreateItemGridRow(outputEntityType, uIComponent, out var _);
		uIComponent.OrderByTag1 = score;
		uIComponent.OrderByTag2 = outputEntityType.Name;
		return uIComponent;
	}

	private UIComponent AddUsedInItemRow(EntityType outputEntityType, float score, bool hasInputs, bool hasTools, bool ownsItem)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grdUsedIn.AddEntry(outputEntityType, uIComponent);
		CreateItemGridRow(outputEntityType, uIComponent, out var _);
		uIComponent.OrderByTag1 = score;
		uIComponent.OrderByTag2 = outputEntityType.Name;
		return uIComponent;
	}

	protected override void PopulateProductionContentRefresh()
	{
		PopulateProcessSelector();
		ResolveOwner(out var resolvedOwner);
		PopulateProcessName();
		PopulateUsedInList(resolvedOwner);
		PopulateToolUsedForList(resolvedOwner);
		PopulateOrdersRefresh(resolvedOwner);
	}

	private void PopulateProcessName()
	{
		if (processTypeToShowProductionFor != null)
		{
			lblProcessIndex.Text = $"{processIndex + 1}/{noOfProcesses} {processTypeToShowProductionFor.Name}";
			lblProcessIndex.FitToText();
			int num = btSelectProcess.X - 2;
			int width = Math.Min(lblProcessIndex.Width, num - lblExpandedHeading.Right - 6);
			lblProcessIndex.Width = width;
			lblProcessIndex.AlignRight(num);
			lblProcessIndex.ToolTip = processTypeToShowProductionFor.Name;
		}
	}

	private void PopulateUsedInList(EntityGroup resolvedOwner)
	{
		grdUsedIn.BeginAddingEntries();
		if (GameData.Instance.ProcessesUsingThisInput.TryGetValue(entityType, out var processesUsingThisInput))
		{
			foreach (ProcessType item2 in processesUsingThisInput)
			{
				if (item2.Outputs == null)
				{
					continue;
				}
				Output[] outputs = item2.Outputs;
				foreach (Output obj in outputs)
				{
					EntityType finalEntityTypeToCreate = obj.FinalEntityTypeToCreate;
					if (!obj.IsWasteProduct)
					{
						bool hasInputs;
						bool hasTools;
						bool isAvailable;
						float score = ScoreItem(resolvedOwner, finalEntityTypeToCreate, 0f, out hasInputs, out hasTools, out isAvailable, isToolContext: false);
						if (!grdUsedIn.TryGetEntry(finalEntityTypeToCreate, out var item))
						{
							item = AddUsedInItemRow(finalEntityTypeToCreate, score, hasTools, hasInputs, isAvailable);
						}
						UpdateRequiredItemRow(item, finalEntityTypeToCreate, score, hasTools, hasInputs, isAvailable);
					}
				}
			}
			grdUsedIn.DeleteEntries((EntityType e) => processesUsingThisInput.Exists((ProcessType p) => ProcessHasEntityTypeAsOutput(p, e)));
			grdUsedIn.Sort((UIComponent uIComponent) => (float)uIComponent.OrderByTag1, Grid.Sorting.Descending, (UIComponent uIComponent) => uIComponent.OrderByTag2, Grid.Sorting.Ascending);
		}
		else
		{
			grdUsedIn.Clear();
		}
		grdUsedIn.EndAddingEntries();
		grdProductionOuter.TryRemoveEntry(usedInHeader);
		if (grdUsedIn.Entries.Count > 0)
		{
			grdProductionOuter.AddEntry(usedInHeader, usedInHeader);
			if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.IsKilling)
			{
				lblUsedIn.Text = "YIELDS:";
				lblUsedIn.ToolTip = "Yields these products";
			}
			else
			{
				lblUsedIn.Text = "USED IN:";
				lblUsedIn.ToolTip = $"Used as a material input for making these objects (max {GameData.Instance.GUIConstants.MaxToolsToShowInUsedForList} items are shown)";
			}
		}
	}

	private void PopulateToolUsedForList(EntityGroup resolvedOwner)
	{
		grdToolUsedFor.BeginAddingEntries();
		bool flag = false;
		if (entityType.ToolType != null)
		{
			if (GameData.Instance.ToolsUsedFor.TryGetValue(entityType, out var processesUsingTool))
			{
				foreach (ProcessType item2 in processesUsingTool)
				{
					if (item2.Outputs == null)
					{
						continue;
					}
					Output[] outputs = item2.Outputs;
					foreach (Output obj in outputs)
					{
						EntityType finalEntityTypeToCreate = obj.FinalEntityTypeToCreate;
						if (!obj.IsWasteProduct)
						{
							bool hasInputs;
							bool hasTools;
							bool isAvailable;
							float score = ScoreItem(resolvedOwner, finalEntityTypeToCreate, 0f, out hasInputs, out hasTools, out isAvailable, isToolContext: false);
							if (!grdToolUsedFor.TryGetEntry(finalEntityTypeToCreate, out var item))
							{
								item = AddUsedForItemRow(finalEntityTypeToCreate, score);
							}
							UpdateRequiredItemRow(item, finalEntityTypeToCreate, score, hasTools, hasInputs, isAvailable);
						}
					}
				}
				grdToolUsedFor.DeleteEntries((EntityType e) => processesUsingTool.Any((ProcessType p) => ProcessHasEntityTypeAsOutput(p, e)));
				flag = grdToolUsedFor.CapNoOfEntries(GameData.Instance.GUIConstants.MaxToolsToShowInUsedForList);
				grdToolUsedFor.Sort((UIComponent uIComponent) => (float)uIComponent.OrderByTag1, Grid.Sorting.Descending, (UIComponent uIComponent) => uIComponent.OrderByTag2, Grid.Sorting.Ascending);
			}
			else
			{
				grdToolUsedFor.Clear();
			}
		}
		else
		{
			grdToolUsedFor.Clear();
		}
		grdToolUsedFor.EndAddingEntries();
		grdProductionOuter.TryRemoveEntry(toolUsedForHeader);
		grdProductionOuter.TryRemoveEntry(lbltoolUsedForCapNotice);
		if (grdToolUsedFor.Entries.Count > 0)
		{
			grdProductionOuter.AddEntry(toolUsedForHeader, toolUsedForHeader);
			if (flag)
			{
				grdProductionOuter.AddEntry(lbltoolUsedForCapNotice, lbltoolUsedForCapNotice);
			}
		}
	}

	protected override void Retire()
	{
		The.InGameUI.poolOfEntityTypeTooltips.Retire(this);
	}

	private bool ProcessHasEntityTypeAsOutput(ProcessType processType, EntityType entityType)
	{
		if (processType.Outputs != null)
		{
			return processType.Outputs.FirstOrDefault((Output o) => o.FinalEntityTypeToCreate == entityType) != null;
		}
		return false;
	}

	protected override void SetProductionHeading(Label lbl)
	{
		lbl.Text = "PRODUCTION";
		if (noOfProcesses <= 1)
		{
			PadHeader(lbl);
		}
	}

	protected override string GetInputHeading()
	{
		return "MADE FROM:";
	}
}
