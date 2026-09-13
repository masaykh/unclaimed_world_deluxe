using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class GatherResourcesWindow : HUDWindow
{
	private class HarvestJobsButtonEventArgs : EventArgs
	{
		public object Item;

		public MapArea.ResourcesAndJobs ResourcesAndJobs;

		public HarvestJobsButtonEventArgs(object item, MapArea.ResourcesAndJobs resourcesAndJobs)
		{
			Item = item;
			ResourcesAndJobs = resourcesAndJobs;
		}
	}

	private MapArea mapArea;

	private Expedition expedition;

	private List<ResourceCategory> categoryGridKeys = new List<ResourceCategory>();

	private Grid outerGrid;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	private Dictionary<ResourceType, MapArea.ResourcesAndJobs> data = new Dictionary<ResourceType, MapArea.ResourcesAndJobs>();

	private Dictionary<FillableBar, bool> userChangedData = new Dictionary<FillableBar, bool>();

	private int itemTypeIconColumnX = 10;

	private bool isFirstUpdate;

	private TextButton btAll;

	private TextButton btNone;

	private TextButton btOK;

	private TextButton btCancel;

	private const int orderedX = 175;

	private const int regrowthX = 290;

	private const int gridHeaderY = 28;

	private const string gridKey = "Grid";

	public GatherResourcesWindow()
		: base(364, 275, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 200;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		AddZoneNameAndHeader("", "GATHER", "HUD_icon_gather", 12, out lblName, out lblHeader, out headerIcon);
		SetSummaryDelegate = SidePanelEntity.SetSummaryAsTotal;
		CreateGridHeader();
		outerGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		outerGrid.IsOuterGrid = true;
		outerGrid.X = 12;
		outerGrid.Y = 54;
		outerGrid.FixedItemHeights = false;
		outerGrid.Width = DisplayWindow.ViewPort.Width - 24;
		outerGrid.ScrollBarEnabled = true;
		outerGrid.ItemHeight = 27;
		outerGrid.CanGrowInHeight = false;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Height = 136;
		Add(outerGrid);
		_ = DisplayWindow.Height;
		btCancel = new TextButton(gui);
		Add(btCancel);
		btCancel.Text = "CANCEL";
		btCancel.Init(TextButton.TextButtonType.HUD);
		btCancel.Click += btCancel_Click;
		btCancel.Width = 72;
		btCancel.X = DisplayWindow.Width - 12 - btCancel.Width;
		btOK = new TextButton(gui);
		Add(btOK);
		btOK.Text = "OK";
		btOK.Init(TextButton.TextButtonType.HUD);
		btOK.Click += btOk_Click;
		btOK.Width = 72;
		btOK.X = btCancel.X - 2 - btOK.Width;
		btNone = new TextButton(gui);
		Add(btNone);
		btNone.Text = "None";
		btNone.Init(TextButton.TextButtonType.HUDHasState);
		btNone.Click += btGatherNone_Click;
		btNone.X = 127;
		btNone.ToolTip = "Cancel all gathering";
		btNone.Width = 48;
		btNone.CheckedMode = CheckedModes.CannotBeChecked;
		btAll = new TextButton(gui);
		Add(btAll);
		btAll.Text = "All";
		btAll.Init(TextButton.TextButtonType.HUDHasState);
		btAll.Click += btGatherAll_Click;
		btAll.X = btNone.Right + 2;
		btAll.ToolTip = "Gather all resources";
		btAll.Width = 48;
		btAll.CheckedMode = CheckedModes.CannotBeChecked;
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 12;
		btOK.Y = DisplayWindow.Height - btOK.Height - 12;
		btNone.Y = btOK.Y - 43;
		btAll.Y = btNone.Y;
		outerGrid.Height = btNone.Y - 13 - outerGrid.Y;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	private void CreateGridHeader()
	{
		Label label = new Label(gui);
		Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.Text = "Ordered / available";
		label.FitToText();
		label.X = 175;
		label.Y = 28;
		Label label2 = new Label(gui);
		Add(label2);
		label2.Init(Label.LabelType.HUDWindow);
		label2.Text = "Regrowth";
		label2.FitToText();
		label2.ToolTip = "The yearly regrowth of each resource in the zone";
		label2.X = 290;
		label2.Y = 28;
	}

	private void btGatherAll_Click(UIComponent sender, EventArgs e)
	{
		foreach (ResourceCategory categoryGridKey in categoryGridKeys)
		{
			if (!outerGrid.TryGetEntry(categoryGridKey, out var item))
			{
				continue;
			}
			(item as Grid).TryGetEntry("Grid", out item);
			foreach (KeyValuePair<object, UIComponent> item2 in (item as Grid).EntriesByKey)
			{
				UIComponent uIComponent = item2.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);
				if (uIComponent != null)
				{
					FillableBar fillableBar = (FillableBar)uIComponent;
					if (fillableBar.Visible)
					{
						fillableBar.Value = fillableBar.MaxValue;
						SetUserChangedSliderState(fillableBar);
						fillableBar.UpdateSliderPosition();
					}
				}
			}
		}
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void btOk_Click(UIComponent sender, EventArgs e)
	{
		bool flag = false;
		EntityGroupID iD = expedition.OwnedEntities.ID;
		Zone zone = mapArea.Zone;
		foreach (ResourceCategory categoryGridKey in categoryGridKeys)
		{
			if (!outerGrid.TryGetEntry(categoryGridKey, out var item))
			{
				continue;
			}
			(item as Grid).TryGetEntry("Grid", out item);
			foreach (KeyValuePair<object, UIComponent> item2 in (item as Grid).EntriesByKey)
			{
				FillableBar fillableBar = item2.Value.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
				ImageButton imageButton = null;
				if (GameData.Instance.GUIConstants.EnableStandingOrders)
				{
					imageButton = item2.Value.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
				}
				ResourceType resourceType = (ResourceType)item2.Key;
				EntityType resourceItemType = resourceType.ResourceItemType;
				if (GameData.Instance.GUIConstants.EnableStandingOrders && imageButton.IsChecked)
				{
					if (expedition.OwnedEntities.ProductionOrders.Orders[resourceItemType].AmountToKeepInStore != fillableBar.Value)
					{
						SetStandingOrder command = new SetStandingOrder(expedition.ID, resourceItemType.KeyName, fillableBar.Value, giveClientFeedback: true);
						The.Client.Controller.StoreAndExecuteCommand(command);
					}
					if (mapArea.Zone == null || !mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType))
					{
						if (fillableBar.Value > 0)
						{
							SetStandingOrderGatherInZone command2 = ((zone == null) ? new SetStandingOrderGatherInZone(mapArea, giveClientFeedback: true, resourceType.KeyName, enable: true, iD) : new SetStandingOrderGatherInZone(zone.ID, giveClientFeedback: true, resourceType.KeyName, enable: true, iD));
							The.Client.Controller.StoreAndExecuteCommand(command2);
						}
						else
						{
							SetStandingOrderGatherInZone command3 = ((zone == null) ? new SetStandingOrderGatherInZone(mapArea, giveClientFeedback: true, resourceType.KeyName, enable: false, iD) : new SetStandingOrderGatherInZone(zone.ID, giveClientFeedback: true, resourceType.KeyName, enable: false, iD));
							The.Client.Controller.StoreAndExecuteCommand(command3);
						}
					}
				}
				else
				{
					MapArea.ResourcesAndJobs value = data[(ResourceType)item2.Key];
					if (value.NumberOfJobsInZone != fillableBar.Value)
					{
						flag = true;
						value.NumberOfJobsInZone = fillableBar.Value;
						value.UserChangedData = true;
						data[resourceType] = value;
					}
					if (mapArea.Zone != null && mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType))
					{
						SetStandingOrderGatherInZone command4 = ((zone == null) ? new SetStandingOrderGatherInZone(mapArea, giveClientFeedback: true, resourceType.KeyName, enable: false, iD) : new SetStandingOrderGatherInZone(zone.ID, giveClientFeedback: true, resourceType.KeyName, enable: false, iD));
						The.Client.Controller.StoreAndExecuteCommand(command4);
					}
				}
				zone = The.InGameUI.SelectedZone;
			}
		}
		if (flag)
		{
			zone = The.InGameUI.SelectedZone;
			foreach (KeyValuePair<ResourceType, MapArea.ResourcesAndJobs> datum in data)
			{
				if (datum.Value.UserChangedData)
				{
					int num = zone?.MapArea.GetNoOfJobs(datum.Key) ?? 0;
					int jobDifference = datum.Value.NumberOfJobsInZone - num;
					ProcessType processType = GameData.Instance.ProcessYieldsThisOutput[datum.Key.ResourceItemType].FirstOrDefault((ProcessType p) => p.IsGathering);
					Command command5 = ((zone == null) ? new Gather(The.InGameUI.SelectedTiles, giveClientFeedback: true, jobDifference, datum.Key.ResourceItemType.KeyName, processType.KeyName, datum.Key.KeyName, iD) : new Gather(zone.ID, giveClientFeedback: true, jobDifference, datum.Key.ResourceItemType.KeyName, processType.KeyName, datum.Key.KeyName, iD));
					The.Client.Controller.StoreAndExecuteCommand(command5);
					zone = The.InGameUI.SelectedZone;
				}
			}
		}
		Hide();
	}

	private void btGatherNone_Click(UIComponent sender, EventArgs e)
	{
		foreach (ResourceCategory categoryGridKey in categoryGridKeys)
		{
			if (!outerGrid.TryGetEntry(categoryGridKey, out var item))
			{
				continue;
			}
			(item as Grid).TryGetEntry("Grid", out item);
			foreach (KeyValuePair<object, UIComponent> item2 in (item as Grid).EntriesByKey)
			{
				UIComponent uIComponent = item2.Value.FindChildById(UIComponent.DataControlID.CurrentOrders);
				if (uIComponent != null)
				{
					FillableBar fillableBar = (FillableBar)uIComponent;
					fillableBar.Value = 0;
					SetUserChangedSliderState(fillableBar);
					fillableBar.UpdateSliderPosition();
				}
			}
		}
	}

	public new void Hide()
	{
		DisplayWindow.Hide();
	}

	public override void Refresh()
	{
		data.Clear();
		mapArea.GetSumOfAllResourcesInArea(The.InGameUI.UIAllegiance.SharedKnowledge, data);
		mapArea.GetSumOfAllHarvestJobsInArea(data);
		GetStandingOrdersForDepletedResources();
		Populate();
	}

	private void GetStandingOrdersForDepletedResources()
	{
		if (mapArea.Zone == null)
		{
			return;
		}
		foreach (ResourceType item in mapArea.Zone.AllowStandingOrderHarvest)
		{
			if (!data.ContainsKey(item))
			{
				data.Add(item, default(MapArea.ResourcesAndJobs));
			}
		}
	}

	private void Populate()
	{
		UIComponent item = null;
		EntityGroup owner = mapArea.GetOwner();
		if (owner == null)
		{
			outerGrid.Clear();
			return;
		}
		Grid grid = null;
		int count = outerGrid.Entries.Count;
		foreach (KeyValuePair<ResourceType, MapArea.ResourcesAndJobs> datum in data)
		{
			if (!outerGrid.TryGetEntry(datum.Key.Category, out item))
			{
				CreateInnerGrid(datum.Key.Category);
			}
			else
			{
				outerGrid.TryGetEntry(datum.Key.Category, out item);
				(item as Grid).TryGetEntry("Grid", out item);
				grid = item as Grid;
			}
			int availableResources = GetAvailableResources(datum.Value);
			int numberOfJobsInZone = datum.Value.NumberOfJobsInZone;
			outerGrid.TryGetEntry(datum.Key.Category, out item);
			(item as Grid).TryGetEntry("Grid", out item);
			grid = item as Grid;
			grid.BeginAddingEntries();
			if (!grid.TryGetEntry(datum.Key, out item))
			{
				item = AddRow(datum.Key, datum.Key.Name, numberOfJobsInZone, datum.Value, owner, grid);
			}
			UpdateRow(item, availableResources, numberOfJobsInZone, owner, datum.Key, datum.Value.MaximumRegrowth, datum.Value.CurrentRegrowth, datum.Value.MaximumReached);
			grid.EndAddingEntries();
		}
		for (int num = categoryGridKeys.Count - 1; num >= 0; num--)
		{
			object key = categoryGridKeys[num];
			if (outerGrid.TryGetEntry(key, out item))
			{
				(item as Grid).TryGetEntry("Grid", out item);
				grid = item as Grid;
				grid.Sort((UIComponent i) => i.OrderByTag2, Grid.Sorting.Ascending);
				CleanUpGrid(grid, key);
			}
		}
		if (count != outerGrid.Entries.Count)
		{
			outerGrid.Sort((UIComponent i) => i.OrderByTag1, Grid.Sorting.Ascending);
		}
		isFirstUpdate = false;
	}

	private static int GetAvailableResources(MapArea.ResourcesAndJobs kvp)
	{
		return Common.ClampBottom(kvp.NumberOfResources - kvp.NumberOfJobsInOtherZones, 0);
	}

	private void CreateInnerGrid(ResourceCategory category)
	{
		Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.Initialize();
		grid.FixedItemHeights = false;
		grid.CanGrowInHeight = true;
		grid.Width = DisplayWindow.ViewPort.Width - 24;
		grid.ScrollBarEnabled = false;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		Grid grid2 = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid2.IsOuterGrid = true;
		grid2.FixedItemHeights = true;
		grid2.Width = DisplayWindow.ViewPort.Width - 24;
		grid2.ScrollBarEnabled = false;
		grid2.ItemHeight = 27;
		grid2.CanGrowInHeight = true;
		grid2.Font = GUIManager.LCDandHUDBodyFontPath;
		grid2.Height = 136;
		categoryGridKeys.Add(category);
		AddTextRow(category.Name, grid);
		grid.OrderByTag1 = category.Name;
		grid.AddEntry("Grid", grid2);
		outerGrid.AddEntry(category, grid);
	}

	private void CleanUpGrid(Grid grid, object key)
	{
		grid.DeleteEntries((ResourceType e) => data.ContainsKey(e));
		if (grid.Count == 0)
		{
			outerGrid.RemoveEntry(key);
			categoryGridKeys.Remove(key as ResourceCategory);
		}
	}

	private void UpdateRow(UIComponent item, int available, int currentOrder, EntityGroup owner, ResourceType resourceType, float? maxRegrowth, float? currentRegrowth, bool maxReached)
	{
		EntityType resourceItemType = resourceType.ResourceItemType;
		int noOfIncompleteEntities;
		int noOfEntitiesUsedAsParts;
		int noOfItemsOnOtherSite;
		int noOfItemsOwnedByOthers;
		int noOfAvailableItemsIncludingIntrinsic;
		int noOfAvailableEntities = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, resourceItemType, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic);
		UIComponent uIComponent = item.FindChildById(UIComponent.DataControlID.Stock);
		if (uIComponent != null)
		{
			TextButton obj = (TextButton)uIComponent;
			obj.Text = noOfAvailableEntities.ToString();
			obj.Enabled = noOfAvailableEntities > 0;
		}
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
		bool bestProcessForDisplay = InventoryPanel.GetBestProcessForDisplay(resourceItemType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, out processType, includeSalvageProcesses: false, null, null, (ProcessType p) => p.IsGathering);
		((DataTypeButton)item.FindChildById(UIComponent.DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableEntities > 0);
		FillableBar fillableBar = item.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
		ImageButton imageButton = null;
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			imageButton = item.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
		}
		if (owner.ProductionOrders.Orders.TryGetValue(resourceItemType, out var value))
		{
			if (GameData.Instance.GUIConstants.EnableStandingOrders)
			{
				if (isFirstUpdate)
				{
					if (mapArea.Zone != null)
					{
						if (isFirstUpdate)
						{
							ProductionOrderControl.SetPadlockButtonState(mapArea.Zone.AllowStandingOrderHarvest.Contains(resourceType) && value.AmountToKeepInStore.HasValue, imageButton);
						}
					}
					else
					{
						imageButton.IsChecked = false;
					}
				}
				if (imageButton.IsChecked)
				{
					UpdateStandingOrderProduction(value, fillableBar, imageButton);
				}
				else
				{
					UpdateDirectOrders(available, currentOrder, bestProcessForDisplay, fillableBar, imageButton);
				}
			}
			else
			{
				UpdateDirectOrders(available, currentOrder, bestProcessForDisplay, fillableBar, null);
			}
		}
		HorizontalList horizontalList = item.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
		HorizontalList horizontalList2 = item.FindChildById(UIComponent.DataControlID.AttainableIcons) as HorizontalList;
		if (!fillableBar.Visible)
		{
			Dictionary<ProcessType, AttainableInfo> attainableInfos = null;
			if (!bestProcessForDisplay)
			{
				attainableInfos = The.InGameUI.InventorySettings.GetAttainableInfo(resourceType.ResourceItemType);
			}
			ProductionOrderControl.UpdateAttainable(attainableInfos, horizontalList2, horizontalList, hasTools, hasInputs, hasSkills, hasResources, bestProcessForDisplay, processType);
		}
		else
		{
			horizontalList2.Visible = false;
			horizontalList.Visible = false;
		}
		Label label = item.FindChildById(UIComponent.DataControlID.Regrowth) as Label;
		if (resourceType.CanReplenish())
		{
			label.Visible = true;
			if (currentRegrowth.HasValue)
			{
				string text;
				string t;
				if (maxReached)
				{
					text = "max.";
					t = "0 (at max.)";
				}
				else
				{
					text = $"+{currentRegrowth.Value:N1}";
					t = $"{currentRegrowth.Value:N1}";
				}
				label.Text = text;
				label.FitToText();
				StringBuilder stringBuilder = new StringBuilder();
				Common.AppendHeaderOnLightBG(stringBuilder, "Regrowth rate");
				Common.AppendFormat(stringBuilder, "Shows the expected yearly regrowth of {0} in the zone.", false, resourceType.Name);
				Common.AppendDividerOnOwnLine(stringBuilder);
				Common.Append(stringBuilder, "Maximum regrowth per year: ");
				Common.AppendFormat(stringBuilder, "{0:N1}", true, maxRegrowth.Value);
				Common.AppendLine(stringBuilder);
				Common.Append(stringBuilder, "Current regrowth per year: ");
				Common.Append(stringBuilder, t, tintAsValue: true);
				label.ToolTip = stringBuilder.ToString();
			}
		}
		else
		{
			label.Visible = false;
		}
	}

	private void UpdateStandingOrderProduction(ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		fillableBar.Visible = true;
		int num = stockTarget.AmountToKeepInStore ?? 0;
		fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint;
		btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
		bool flag = false;
		fillableBar.StepSize = 1;
		if (fillableBar.MaxValue != GameData.Instance.GUIConstants.MaxStandingOrder)
		{
			fillableBar.MaxValue = GameData.Instance.GUIConstants.MaxStandingOrder;
			flag = true;
		}
		if (!userChangedData.ContainsKey(fillableBar) && fillableBar.Value != num)
		{
			fillableBar.Value = num;
			flag = true;
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
	}

	private void UpdateDirectOrders(int available, int currentOrder, bool canProduce, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		HuntWindow.SetNormalTint(fillableBar, btStandingOrder);
		bool flag = false;
		if (fillableBar.MaxValue != available)
		{
			flag = true;
			fillableBar.MaxValue = available;
		}
		if (!userChangedData.ContainsKey(fillableBar) && fillableBar.Value != currentOrder)
		{
			fillableBar.Value = currentOrder;
			flag = true;
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
		if (!canProduce)
		{
			fillableBar.Visible = false;
		}
		else
		{
			fillableBar.Visible = true;
		}
	}

	public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
	{
		isFirstUpdate = true;
		base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);
		userChangedData.Clear();
		mapArea = TileSelectionContextMenu.GetMapArea();
		expedition = mapArea.GetOwner().Parent as Expedition;
		string zoneName = "";
		if (mapArea.Zone != null)
		{
			zoneName = mapArea.Zone.GetDisplayName();
		}
		SetDisplayName(zoneName, lblName, lblHeader, headerIcon);
	}

	private void AddTextRow(string textRow, Grid gridToAddTo)
	{
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.Text = textRow;
		label.FitToText();
		label.X = 10;
		label.Y = 0;
		gridToAddTo.AddEntry(textRow, label);
	}

	private UIComponent AddRow(object key, string caption, int currentOrder, MapArea.ResourcesAndJobs resourcesAndJobs, EntityGroup owner, Grid grid)
	{
		EntityType resourceItemType = ((ResourceType)key).ResourceItemType;
		EventArgs eventArgs = new HarvestJobsButtonEventArgs(key, resourcesAndJobs);
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(key, uIComponent);
		InventoryPanel.AddEntityTypeIcon(resourceItemType, uIComponent, itemTypeIconColumnX);
		DataTypeButton dataTypeButton = new DataTypeButton(gui, DataSheet.InfoToShow.Data, resourceItemType, owner.ID, useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = caption;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 125;
		dataTypeButton.X = 22;
		dataTypeButton.DebugTag = "entityTypeButton";
		int num = 164;
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			ImageButton imageButton = new ImageButton(Interface.gui);
			uIComponent.Add(imageButton);
			imageButton.Init(ImageButtonType.HUDPadlock);
			imageButton.Position = new Point(num - 13, 0);
			imageButton.Click += btPadlock_Click;
			imageButton.ToolTip = "Switch to standing order mode.";
			imageButton.ID = UIComponent.DataControlID.StandingOrderModePadlock;
			uIComponent.CenterChildVertically(imageButton);
		}
		FillableBar fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSliderWhite, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		uIComponent.Add(fillableBar);
		fillableBar.ID = UIComponent.DataControlID.CurrentOrders;
		fillableBar.Width = 138;
		fillableBar.X = num;
		fillableBar.EventArgs = eventArgs;
		fillableBar.Value = currentOrder;
		fillableBar.SliderMouseDown += fillableBar_SliderMouseDown;
		fillableBar.Tag1 = key;
		fillableBar.Y = 4;
		fillableBar.UpdateSliderPosition();
		HorizontalList horizontalList = new HorizontalList(Interface.gui);
		uIComponent.Add(horizontalList);
		horizontalList.ID = UIComponent.DataControlID.NotAttainableIcons;
		horizontalList.X = num;
		horizontalList.Height = 21;
		uIComponent.CenterChildVertically(horizontalList);
		HorizontalList horizontalList2 = new HorizontalList(Interface.gui);
		uIComponent.Add(horizontalList2);
		horizontalList2.ID = UIComponent.DataControlID.AttainableIcons;
		horizontalList2.X = num;
		horizontalList2.Height = 21;
		uIComponent.CenterChildVertically(horizontalList2);
		Label label = new Label(gui);
		uIComponent.Add(label);
		label.ID = UIComponent.DataControlID.Regrowth;
		label.Init(Label.LabelType.HUDWindow);
		label.X = 290;
		uIComponent.CenterChildVertically(label);
		label.Y--;
		label.TooltipWidth = 260;
		uIComponent.OrderByTag2 = resourceItemType.PluralName;
		return uIComponent;
	}

	private void btPadlock_Click(UIComponent sender, EventArgs e)
	{
		Expedition firstPlayerExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
		if (firstPlayerExpedition != null)
		{
			EntityGroup ownedEntities = firstPlayerExpedition.OwnedEntities;
			ImageButton imageButton = sender as ImageButton;
			if (imageButton.IsChecked)
			{
				imageButton.ToolTip = "Standing order mode. In this mode, production will start and continue whenever the inventory is below the slider value. \nClick to switch back to direct order mode.";
			}
			else
			{
				imageButton.ToolTip = "Switch to standing order mode.";
			}
			UIComponent parent = sender.Parent;
			ResourceType resourceType = (ResourceType)parent.Tag1;
			int availableResources = GetAvailableResources(data[resourceType]);
			UpdateRow(parent, availableResources, 0, ownedEntities, resourceType, null, null, maxReached: false);
		}
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		EntityGroup ownedEntities = The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities;
		The.InGameUI.EntityListWindow.PopulateAndShowOnPlayfield(sender, ((ItemTypeButtonEventArgs)e).Item, ownedEntities);
	}

	private void fillableBar_SliderMouseDown(object sender, EventArgs e)
	{
		SetUserChangedSliderState((FillableBar)sender);
	}

	private void fillableBar_SliderMouseUp(object sender, EventArgs e)
	{
		SetUserChangedSliderState((FillableBar)sender);
	}

	private void SetUserChangedSliderState(FillableBar control)
	{
		if (!userChangedData.ContainsKey(control))
		{
			userChangedData.Add(control, value: true);
		}
	}

	public void SaveJobChanges(ResourceType resourceType, int noOfJobs)
	{
		MapArea.ResourcesAndJobs value = data[resourceType];
		value.NumberOfJobsInZone = noOfJobs;
		value.UserChangedData = true;
		data[resourceType] = value;
		Populate();
	}
}
