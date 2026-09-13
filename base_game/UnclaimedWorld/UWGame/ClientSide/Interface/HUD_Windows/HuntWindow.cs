using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HuntWindow : HUDWindow
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

	private Grid grid;

	private Label lblName;

	private Label lblHeader;

	private Image headerIcon;

	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	private Dictionary<FillableBar, bool> userChangedData = new Dictionary<FillableBar, bool>();

	private TextButton btCancel;

	private TextButton btOK;

	private int itemTypeIconColumnX = 10;

	private bool isFirstUpdate;

	private const int orderedX = 195;

	private const int gridHeaderY = 28;

	private const int maxPreyToHunt = 10;

	public HuntWindow()
		: base(334, 275, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 200;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		AddZoneNameAndHeader("", "HUNT", "HUD_icon_hunt", 12, out lblName, out lblHeader, out headerIcon);
		SetSummaryDelegate = SidePanelEntity.SetSummaryAsTotal;
		CreateGridHeader();
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.IsOuterGrid = true;
		grid.X = 12;
		grid.Y = 54;
		grid.FixedItemHeights = true;
		grid.Width = DisplayWindow.ViewPort.Width - 24;
		grid.ScrollBarEnabled = true;
		grid.ItemHeight = 27;
		grid.CanGrowInHeight = false;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Height = 136;
		Add(grid);
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
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		btCancel.Y = DisplayWindow.Height - btCancel.Height - 12;
		btOK.Y = btCancel.Y;
		grid.Height = btOK.Y - 13 - grid.Y;
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
		label.Text = "Ordered";
		label.FitToText();
		label.X = 195;
		label.Y = 28;
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void btOk_Click(UIComponent sender, EventArgs e)
	{
		EntityGroupID iD = expedition.OwnedEntities.ID;
		Zone zone = mapArea.Zone;
		foreach (KeyValuePair<object, UIComponent> item in grid.EntriesByKey)
		{
			FillableBar fillableBar = item.Value.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
			ImageButton imageButton = null;
			if (GameData.Instance.GUIConstants.EnableStandingOrders)
			{
				imageButton = item.Value.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
			}
			EntityType entityType = (EntityType)item.Key;
			if (GameData.Instance.GUIConstants.EnableStandingOrders && imageButton.IsChecked)
			{
				if (expedition.OwnedEntities.ProductionOrders.Orders[entityType.BiologicalType.CarcassType].AmountToKeepInStore != fillableBar.Value)
				{
					SetStandingOrder command = new SetStandingOrder(expedition.ID, entityType.BiologicalType.CarcassType.KeyName, fillableBar.Value, giveClientFeedback: true);
					The.Client.Controller.StoreAndExecuteCommand(command);
				}
				if (mapArea.Zone == null || !mapArea.Zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType))
				{
					if (fillableBar.Value > 0)
					{
						SetStandingOrderHuntInZone command2 = ((zone == null) ? new SetStandingOrderHuntInZone(mapArea, giveClientFeedback: true, entityType.KeyName, enable: true, iD) : new SetStandingOrderHuntInZone(zone.ID, giveClientFeedback: true, entityType.KeyName, enable: true, iD));
						The.Client.Controller.StoreAndExecuteCommand(command2);
					}
					else
					{
						SetStandingOrderHuntInZone command3 = ((zone == null) ? new SetStandingOrderHuntInZone(mapArea, giveClientFeedback: true, entityType.KeyName, enable: false, iD) : new SetStandingOrderHuntInZone(zone.ID, giveClientFeedback: true, entityType.KeyName, enable: false, iD));
						The.Client.Controller.StoreAndExecuteCommand(command3);
					}
				}
			}
			else
			{
				int value = 0;
				zone?.ZoneHunt.CreaturesToHunt.TryGetValue((EntityType)item.Key, out value);
				if (value != fillableBar.Value)
				{
					bool removeAfterSuccessfulHunt = false;
					Command command4 = ((zone == null) ? new HuntArea(The.InGameUI.SelectedTiles, giveClientFeedback: true, entityType, fillableBar.Value, removeAfterSuccessfulHunt, iD) : new HuntArea(The.InGameUI.SelectedZone.ID, giveClientFeedback: true, entityType, fillableBar.Value, removeAfterSuccessfulHunt, iD));
					The.Client.Controller.StoreAndExecuteCommand(command4);
				}
				if (zone != null && zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType))
				{
					SetStandingOrderHuntInZone command5 = new SetStandingOrderHuntInZone(zone.ID, giveClientFeedback: true, entityType.KeyName, enable: false, iD);
					The.Client.Controller.StoreAndExecuteCommand(command5);
				}
			}
			zone = The.InGameUI.SelectedZone;
		}
		zone?.RemoveZoneOrFireOrdersChangedEvent();
		Hide();
	}

	public override void Hide()
	{
		DisplayWindow.Hide();
	}

	public override void Refresh()
	{
		Populate();
	}

	private List<Tuple<EntityType, bool>> GetPreyToDisplay()
	{
		List<Tuple<EntityType, bool>> list = new List<Tuple<EntityType, bool>>();
		HashSet<EntityType> habitats = GetHabitats();
		if (habitats != null)
		{
			foreach (EntityType item in habitats)
			{
				if (The.InGameUI.UIAllegiance.RepresentativeEntityType.IntelligenceType.PreyTypes.Contains(item))
				{
					list.Add(new Tuple<EntityType, bool>(item, item2: true));
				}
			}
		}
		foreach (EntityType item2 in The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.SpottedPrey)
		{
			if (habitats == null || !habitats.Contains(item2))
			{
				list.Add(new Tuple<EntityType, bool>(item2, item2: false));
			}
		}
		return list;
	}

	private HashSet<EntityType> GetHabitats()
	{
		HashSet<Collidable<Expedition>> expeditions = new HashSet<Collidable<Expedition>>();
		mapArea.IterateArea(delegate(TerrainTile tile)
		{
			GetPreyHabitatsOnTile(tile, expeditions);
		});
		HashSet<EntityType> set = null;
		foreach (Collidable<Expedition> item in expeditions)
		{
			Common.AddToSet(ref set, item.Parent.Allegiance.RepresentativeEntityType);
		}
		return set;
	}

	private void GetPreyHabitatsOnTile(TerrainTile tile, HashSet<Collidable<Expedition>> expeditions)
	{
		if (tile.HasEverBeenSeenByPlayer)
		{
			Vector2 location = MapManager.TileToWorldPosVector2(tile.TilePos.ToPoint());
			The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.GetCollidablesContainingPoint(location, expeditions);
		}
	}

	private void Populate()
	{
		UIComponent item = null;
		EntityGroup owner = mapArea.GetOwner();
		if (owner == null)
		{
			grid.Clear();
			return;
		}
		List<Tuple<EntityType, bool>> data = GetPreyToDisplay();
		_ = grid.Entries.Count;
		grid.BeginAddingEntries();
		foreach (Tuple<EntityType, bool> item2 in data)
		{
			if (!grid.TryGetEntry(item2.Item1, out item))
			{
				item = AddRow(item2.Item1, owner);
			}
			UpdateRow(item, item2.Item1, item2.Item2, owner);
		}
		grid.DeleteEntries((EntityType e) => data.Any((Tuple<EntityType, bool> t) => t.Item1 == e));
		grid.Sort((UIComponent i) => i.OrderByTag1, Grid.Sorting.Ascending);
		grid.EndAddingEntries();
		isFirstUpdate = false;
	}

	private void UpdateRow(UIComponent item, EntityType entityType, bool isInHabitat, EntityGroup owner)
	{
		item.Tag2 = isInHabitat;
		FillableBar fillableBar = item.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
		ImageButton imageButton = null;
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			imageButton = item.FindChildById(UIComponent.DataControlID.StandingOrderModePadlock) as ImageButton;
		}
		item.FindChildById<Image>(UIComponent.DataControlID.Habitat, out var child, firstLevelOnly: false);
		child.Visible = isInHabitat;
		if (!owner.ProductionOrders.Orders.TryGetValue(entityType.BiologicalType.CarcassType, out var value))
		{
			return;
		}
		int value2 = 0;
		if (mapArea.Zone != null)
		{
			mapArea.Zone.ZoneHunt.CreaturesToHunt.TryGetValue(entityType, out value2);
		}
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			if (isFirstUpdate)
			{
				if (mapArea.Zone != null)
				{
					if (isFirstUpdate)
					{
						ProductionOrderControl.SetPadlockButtonState(mapArea.Zone.ZoneHunt.AllowStandingOrderHunt.Contains(entityType) && value.AmountToKeepInStore.HasValue, imageButton);
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
				UpdateDirectOrders(value2, fillableBar, imageButton);
			}
		}
		else
		{
			UpdateDirectOrders(value2, fillableBar, null);
		}
	}

	private void UpdateStandingOrderProduction(ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		int num = stockTarget.AmountToKeepInStore ?? 0;
		fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint;
		btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
		bool flag = false;
		if (!userChangedData.ContainsKey(fillableBar) && fillableBar.Value != num)
		{
			fillableBar.Value = num;
			flag = true;
		}
		fillableBar.StepSize = 1;
		if (fillableBar.MaxValue != GameData.Instance.GUIConstants.MaxStandingOrder)
		{
			fillableBar.MaxValue = GameData.Instance.GUIConstants.MaxStandingOrder;
			flag = true;
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
	}

	private void UpdateDirectOrders(int currentOrder, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		SetNormalTint(fillableBar, btStandingOrder);
		bool flag = false;
		if (!userChangedData.ContainsKey(fillableBar) && fillableBar.Value != currentOrder)
		{
			fillableBar.Value = currentOrder;
			flag = true;
		}
		if (fillableBar.MaxValue != 10)
		{
			flag = true;
			fillableBar.MaxValue = 10;
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
	}

	public static void SetNormalTint(FillableBar fillableBar, ImageButton btStandingOrder)
	{
		fillableBar.ColorAllControls = UIComponent.HUDTint;
		if (btStandingOrder != null)
		{
			btStandingOrder.NormalColor = UIComponent.HUDLightTint;
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

	private UIComponent AddRow(EntityType entityType, EntityGroup owner)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(entityType, uIComponent);
		uIComponent.OrderByTag1 = entityType.PluralName;
		InventoryPanel.AddEntityTypeIcon(entityType, uIComponent, itemTypeIconColumnX);
		DataTypeButton dataTypeButton = new DataTypeButton(gui, DataSheet.InfoToShow.Data, entityType, owner.ID, useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = entityType.PluralName;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 125;
		dataTypeButton.X = 22;
		dataTypeButton.DebugTag = "entityTypeButton";
		int num = 184;
		Image image = new Image(gui);
		image.SetSkinLocations(gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_track"), Icon.UIType.HUD);
		uIComponent.Add(image);
		image.ResizeControlToFitImage();
		image.X = dataTypeButton.Right + 6;
		image.ID = UIComponent.DataControlID.Habitat;
		image.ToolTip = "The zone is in this creature's habitat";
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			ImageButton imageButton = new ImageButton(Interface.gui);
			uIComponent.Add(imageButton);
			imageButton.Init(ImageButtonType.HUDPadlock);
			imageButton.Position = new Point(num - 13, 0);
			imageButton.Click += btPadlock_Click;
			imageButton.ToolTip = "Switch to standing order mode.";
			uIComponent.CenterChildVertically(imageButton);
			imageButton.ID = UIComponent.DataControlID.StandingOrderModePadlock;
		}
		FillableBar fillableBar = new FillableBar(gui, FillableBar.FillableBarType.HUDSliderWhite, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		uIComponent.Add(fillableBar);
		fillableBar.ID = UIComponent.DataControlID.CurrentOrders;
		fillableBar.Width = 138;
		fillableBar.X = num;
		fillableBar.SliderMouseDown += fillableBar_SliderMouseDown;
		fillableBar.Tag1 = entityType;
		fillableBar.Y = 4;
		fillableBar.UpdateSliderPosition();
		uIComponent.OrderByTag2 = entityType.PluralName;
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
			UpdateRow(parent, (EntityType)parent.Tag1, (bool)parent.Tag2, ownedEntities);
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
}
