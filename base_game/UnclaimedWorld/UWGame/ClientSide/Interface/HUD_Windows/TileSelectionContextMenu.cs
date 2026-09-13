using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class TileSelectionContextMenu : HUDWindow
{
	private const int collapsedHeight = 16;

	private int fullHeight;

	private ImageButton btDelete;

	private TextButton tbStockpile;

	private TextButton tbGather;

	private TextButton tbHunt;

	private TextButton tbScout;

	private TextButton tbForage;

	private TextButton tbPatrol;

	private TextButton tbAttack;

	private TextButton tbConnect;

	private ImageButton btCycle;

	private ImageButton btModify;

	public const string overlapsWarning = "The area overlaps with an existing stockpile";

	public const string notConnectedWarning = "Unable to do this, because the zone is not continuous";

	private const string stockpileTooltip = "Stockpile items in this zone";

	public Window OpeningWindow;

	public GatherResourcesWindow ZoneGatherResourcesWindow;

	private StockpileWindow zoneStockpileWindow;

	private PatrolWindow patrolWindow;

	private HuntWindow huntWindow;

	private AttackWindow attackWindow;

	private Image[] harvestResourceImages = new Image[3];

	private Image[] stockpileImages = new Image[3];

	public TileSelectionContextMenu()
		: base(209, 16, hasSurface: true, hasCloseButton: false, isMovable: false, "HUD_window_base", hideWhenMouseExits: false, Level.Bottom)
	{
		ZoneGatherResourcesWindow = new GatherResourcesWindow();
		zoneStockpileWindow = new StockpileWindow();
		patrolWindow = new PatrolWindow();
		huntWindow = new HuntWindow();
		attackWindow = new AttackWindow();
		DisplayWindow.ViewPort.MouseOut += DisplayWindow_MouseOut;
		CreateCycleButton(ref btCycle, gui, The.InGameUI.ContextMenuOpener.CycleButtonXPos);
		btCycle.DebugTag = "cycleButton";
		Add(btCycle);
		btCycle.Click += btCycle_Click;
		tbConnect = new TextButton(gui);
		tbConnect.Text = "Crop";
		tbConnect.ToolTip = "Remove the areas that are not connected to the start point";
		tbConnect.Init(TextButton.TextButtonType.HUD);
		tbConnect.Click += bt_ConnectClick;
		int num = 2;
		int x = 5;
		int num2 = 0;
		btDelete = new ImageButton(gui);
		Add(btDelete);
		btDelete.Init(ImageButtonType.HUDDelete);
		btDelete.ToolTip = "Delete the zone";
		btDelete.Y = 6;
		btDelete.X = btCycle.Right + 6;
		btDelete.Click += bt_DeleteClick;
		num = 43;
		tbGather = new TextButton(gui);
		Add(tbGather);
		tbGather.Text = "GATHER";
		tbGather.ToolTip = "Select resources to harvest in the zone";
		tbGather.Init(TextButton.TextButtonType.HUDGather);
		tbGather.Click += btGather_Click;
		tbGather.Y = num;
		tbGather.X = x;
		tbGather.ScaleWidthToFitText();
		tbGather.CheckedMode = CheckedModes.CanBeChecked;
		int num3 = tbGather.Height + num2;
		num += num3;
		tbStockpile = new TextButton(gui);
		Add(tbStockpile);
		tbStockpile.Text = "STOCKPILE";
		tbStockpile.ToolTip = "Stockpile items in this zone";
		tbStockpile.Init(TextButton.TextButtonType.HUDStockpile);
		tbStockpile.Click += btStockpile_Click;
		tbStockpile.Y = num;
		tbStockpile.X = x;
		tbStockpile.ScaleWidthToFitText();
		tbStockpile.CheckedMode = CheckedModes.CanBeChecked;
		num += num3;
		tbScout = new TextButton(gui);
		Add(tbScout);
		tbScout.Text = "SCOUT";
		tbScout.ToolTip = "Explore the zone briefly";
		tbScout.Init(TextButton.TextButtonType.HUDScout);
		tbScout.Click += btScout_Click;
		tbScout.Y = num;
		tbScout.X = x;
		tbScout.ScaleWidthToFitText();
		tbScout.CheckedMode = CheckedModes.CanBeChecked;
		num += num3;
		tbForage = new TextButton(gui);
		Add(tbForage);
		tbForage.Text = "EXAMINE";
		tbForage.ToolTip = "Make a thorough examination of the area to uncover hidden resources";
		tbForage.Init(TextButton.TextButtonType.HUDForage);
		tbForage.Click += btExamine_Click;
		tbForage.Y = num;
		tbForage.X = x;
		tbForage.ScaleWidthToFitText();
		tbForage.CheckedMode = CheckedModes.CanBeChecked;
		num += num3;
		tbPatrol = new TextButton(gui);
		Add(tbPatrol);
		tbPatrol.Text = "PATROL";
		tbPatrol.ToolTip = "Patrol the zone continously and engage any threats that appear (Required stance: Fearless)";
		tbPatrol.Init(TextButton.TextButtonType.HUDPatrol);
		tbPatrol.Click += tbPatrol_Click;
		tbPatrol.Y = num;
		tbPatrol.X = x;
		tbPatrol.ScaleWidthToFitText();
		tbPatrol.CheckedMode = CheckedModes.CanBeChecked;
		num += num3;
		tbAttack = new TextButton(gui);
		Add(tbAttack);
		tbAttack.Text = "ATTACK";
		tbAttack.ToolTip = "Do a combat sweep of the area, attacking any entities of the specified type (Required stance: Fearless)";
		tbAttack.Init(TextButton.TextButtonType.HUDAttack);
		tbAttack.Click += tbAttack_Click;
		tbAttack.Y = num;
		tbAttack.X = x;
		tbAttack.ScaleWidthToFitText();
		tbAttack.CheckedMode = CheckedModes.CanBeChecked;
		num += num3;
		tbHunt = new TextButton(gui);
		Add(tbHunt);
		tbHunt.Text = "HUNT";
		tbHunt.ToolTip = "Locate and hunt prey in the zone.";
		tbHunt.Init(TextButton.TextButtonType.HUDHunt);
		tbHunt.Click += tbHunt_Click;
		tbHunt.Y = num;
		tbHunt.X = x;
		tbHunt.ScaleWidthToFitText();
		tbHunt.CheckedMode = CheckedModes.CanBeChecked;
		for (int i = 0; i < 3; i++)
		{
			Image image = new Image(gui)
			{
				Texture = GameData.Instance.BillboardSpriteSheet.Texture,
				X = 0
			};
			harvestResourceImages[i] = image;
		}
		for (int j = 0; j < 3; j++)
		{
			Image image2 = new Image(gui)
			{
				Texture = GameData.Instance.BillboardSpriteSheet.Texture,
				X = 0
			};
			stockpileImages[j] = image2;
		}
		fullHeight = tbHunt.Bottom + 6;
		DisplayWindow.Height = fullHeight;
	}

	public static void CreateCycleButton(ref ImageButton btCycle, GUIManager gui, int xPos)
	{
		btCycle = new ImageButton(gui);
		btCycle.Init(ImageButtonType.HUDCycleEntity);
		btCycle.ToolTip = "Cycle through entities in the zone";
		btCycle.Y = 6;
		btCycle.X = xPos;
	}

	private bool ChildWindowIsVisible()
	{
		if (!ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive && !zoneStockpileWindow.DisplayWindow.IsVisibleAndActive && !patrolWindow.DisplayWindow.IsVisibleAndActive && !huntWindow.DisplayWindow.IsVisibleAndActive)
		{
			return attackWindow.DisplayWindow.IsVisibleAndActive;
		}
		return true;
	}

	private void DisplayWindow_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (!TooltipIsShownForThisOrOpeningWindow() && !ChildWindowIsVisible())
		{
			Hide();
		}
	}

	private bool TooltipIsShownForThisOrOpeningWindow()
	{
		if (The.InGameUI.Tooltip.DisplayWindow.Visible)
		{
			if (The.InGameUI.Tooltip.SpawningWindow == DisplayWindow)
			{
				return true;
			}
			if (The.InGameUI.Tooltip.SpawningWindow == OpeningWindow)
			{
				return true;
			}
		}
		return false;
	}

	private void bt_ConnectClick(UIComponent sender, EventArgs e)
	{
		MapArea mapArea = GetMapArea();
		List<TerrainTile> connectedTiles = null;
		if (!mapArea.CheckConnectivity(createList: false, ref connectedTiles))
		{
			mapArea.CropTilesToConnectedArea();
		}
	}

	private void bt_DeleteClick(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.SelectedZone != null)
		{
			Command command = new DeleteZone(The.InGameUI.SelectedZone.ID);
			The.Client.Controller.StoreAndExecuteCommand(command);
			The.InGameUI.SelectedZone = null;
			Hide();
		}
	}

	private void bt_MouseOver(MouseEventArgs args)
	{
	}

	public static bool CreateAndSelectZone(EntityGroup expeditionOwner)
	{
		if (CanCreateAndSelectZone())
		{
			if (The.InGameUI.SelectedZone == null)
			{
				Zone zone = new Zone(expeditionOwner, The.InGameUI.SelectedTiles);
				The.InGameUI.SelectedZone = zone;
				zone.MapArea.MapAreaRender.IsSelected = true;
			}
			return true;
		}
		return false;
	}

	public static bool CanCreateAndSelectZone()
	{
		if (The.InGameUI.SelectedZone == null)
		{
			return The.InGameUI.SelectedTiles.Count > 0;
		}
		return true;
	}

	public override void Refresh()
	{
		base.Refresh();
		RefreshThisPanel();
		if (ZoneGatherResourcesWindow.DisplayWindow.IsVisibleAndActive)
		{
			ZoneGatherResourcesWindow.Refresh();
		}
		if (huntWindow.DisplayWindow.IsVisibleAndActive)
		{
			huntWindow.Refresh();
		}
	}

	public void RefreshThisPanel()
	{
		MapArea mapArea = GetMapArea();
		if (mapArea.Count == 0 || !mapArea.BoundingRectangle.HasValue)
		{
			Hide();
			return;
		}
		if (mapArea.Zone != null)
		{
			tbHunt.IsChecked = mapArea.Zone.ZoneHunt.HasHuntOrders() || mapArea.Zone.ZoneHunt.HasFindPreyJobs();
			tbScout.IsChecked = mapArea.Zone.ScoutingJob != null;
			tbForage.IsChecked = mapArea.Zone.ExamineJob != null;
			tbStockpile.IsChecked = mapArea.Zone.Stockpile != null;
			tbPatrol.IsChecked = mapArea.Zone.PatrolJob != null;
			tbAttack.IsChecked = mapArea.Zone.AttackAreaJob != null;
			RemoveEntityIcons();
			if (mapArea.Zone.HarvestJobs.Count > 0)
			{
				int num = 0;
				int x = tbGather.Right + 2;
				foreach (KeyValuePair<ResourceType, List<ProcessJob>> harvestJob in mapArea.Zone.HarvestJobs)
				{
					if (harvestJob.Value.Count > 0)
					{
						tbGather.IsChecked = true;
						Image image = harvestResourceImages[num];
						IconInfo iconInfo;
						Rectangle iconSprite = harvestJob.Key.ResourceItemType.GetIconSprite(out iconInfo);
						image.SetSkinLocation(SkinState.Normal, iconSprite);
						image.Texture = gui.GUISpriteSheet.Texture;
						image.ResizeControlToFitImage();
						Add(image);
						image.X = x;
						int num2 = 0;
						if (iconInfo != null)
						{
							num2 = iconInfo.GetYPosAdjustment(iconSprite.Height);
						}
						image.Y = tbGather.Y + tbGather.Height / 2 - iconSprite.Height / 2 + num2;
						x = image.Right + 4;
						num++;
						if (num >= 3)
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			TextButton textButton = tbPatrol;
			TextButton textButton2 = tbHunt;
			TextButton textButton3 = tbForage;
			TextButton textButton4 = tbScout;
			TextButton textButton5 = tbStockpile;
			TextButton textButton6 = tbGather;
			bool flag = (tbAttack.IsChecked = false);
			bool flag3 = (textButton6.IsChecked = flag);
			bool flag5 = (textButton5.IsChecked = flag3);
			bool flag7 = (textButton4.IsChecked = flag5);
			bool flag9 = (textButton3.IsChecked = flag7);
			bool isChecked = (textButton2.IsChecked = flag9);
			textButton.IsChecked = isChecked;
			RemoveEntityIcons();
		}
		btDelete.Visible = mapArea.Zone != null;
		bool flag12 = StockpileWindow.OverlapsWithOtherStockpiles(mapArea);
		if (flag12)
		{
			tbStockpile.Enabled = false;
			tbStockpile.ToolTip = "The area overlaps with an existing stockpile";
		}
		List<TerrainTile> connectedTiles = null;
		bool flag13 = mapArea.CheckConnectivity(createList: false, ref connectedTiles);
		if (!flag13 && !flag12)
		{
			tbStockpile.Enabled = false;
			tbStockpile.ToolTip = "Unable to do this, because the zone is not continuous";
		}
		if (!flag12 && flag13)
		{
			tbStockpile.Enabled = true;
			tbStockpile.ToolTip = "Stockpile items in this zone";
		}
	}

	private void RemoveEntityIcons()
	{
		for (int i = 0; i < harvestResourceImages.Length; i++)
		{
			Remove(harvestResourceImages[i]);
		}
		for (int j = 0; j < stockpileImages.Length; j++)
		{
			Remove(stockpileImages[j]);
		}
	}

	public static MapArea GetMapArea()
	{
		if (The.InGameUI.SelectedZone != null)
		{
			return The.InGameUI.SelectedZone.MapArea;
		}
		return The.InGameUI.SelectedTiles;
	}

	public override void Hide()
	{
		base.Hide();
		OpeningWindow = null;
		HideChildWindows();
	}

	private void btModify_Click(UIComponent sender, EventArgs e)
	{
	}

	public static int GetXPositionOfChildWindow(Window window)
	{
		return window.X + window.Width - 4;
	}

	private void btGather_Click(UIComponent sender, EventArgs e)
	{
		HideChildWindows();
		ZoneGatherResourcesWindow.ShowOnPlayfield(GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
		ZoneGatherResourcesWindow.Refresh();
	}

	private void btStockpile_Click(UIComponent sender, EventArgs e)
	{
		HideChildWindows();
		if (GetMapArea() != null)
		{
			zoneStockpileWindow.ShowOnPlayfield(GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			zoneStockpileWindow.FillFromArea();
		}
	}

	private void btCycle_Click(UIComponent sender, EventArgs e)
	{
		ContextMenuOpener.CycleEntities(GetMapArea());
	}

	private void HideChildWindows()
	{
		ZoneGatherResourcesWindow.Hide();
		zoneStockpileWindow.Hide();
		patrolWindow.Hide();
		huntWindow.Hide();
		attackWindow.Hide();
	}

	public static bool GetExpedition(out EntityGroupID expeditionGroupID)
	{
		Expedition expedition = The.InGameUI.GetExpedition();
		if (expedition != null)
		{
			expeditionGroupID = expedition.OwnedEntities.ID;
			return true;
		}
		expeditionGroupID = EntityGroupID.Invalid;
		return false;
	}

	private void btScout_Click(UIComponent sender, EventArgs e)
	{
		if (CanCreateAndSelectZone() && tbScout.IsChecked && GetExpedition(out var expeditionGroupID))
		{
			Command command = ((The.InGameUI.SelectedZone != null) ? new Scout(The.InGameUI.SelectedZone.ID, giveClientFeedback: true, expeditionGroupID) : new Scout(The.InGameUI.SelectedTiles, giveClientFeedback: true, expeditionGroupID));
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
	}

	private void SelectZone(Zone zoneToSelect)
	{
		The.InGameUI.SelectedZone = zoneToSelect;
		zoneToSelect.MapArea.MapAreaRender.IsSelected = true;
	}

	public void OnScoutArea(Zone zoneToScout)
	{
		SelectZone(zoneToScout);
		Hide();
	}

	public void OnCreateStockpile(Zone zone)
	{
		if (zone != null)
		{
			SelectZone(zone);
		}
		Hide();
	}

	private void tbPatrol_Click(UIComponent sender, EventArgs e)
	{
		if (CanCreateAndSelectZone())
		{
			HideChildWindows();
			if (GetMapArea() != null)
			{
				patrolWindow.ShowOnPlayfield(GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			}
		}
	}

	private void tbAttack_Click(UIComponent sender, EventArgs e)
	{
		if (CanCreateAndSelectZone())
		{
			HideChildWindows();
			if (GetMapArea() != null)
			{
				attackWindow.ShowOnPlayfield(GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			}
		}
	}

	private void tbHunt_Click(UIComponent sender, EventArgs e)
	{
		if (CanCreateAndSelectZone())
		{
			HideChildWindows();
			huntWindow.ShowOnPlayfield(GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			huntWindow.Refresh();
		}
	}

	public void OnHuntArea(Zone zoneToHuntIn)
	{
		SelectZone(zoneToHuntIn);
		Hide();
	}

	private void btExamine_Click(UIComponent sender, EventArgs e)
	{
		if (CanCreateAndSelectZone() && tbForage.IsChecked && GetExpedition(out var expeditionGroupID))
		{
			Command command = ((The.InGameUI.SelectedZone == null) ? new Examine(The.InGameUI.SelectedTiles, giveClientFeedback: true, expeditionGroupID) : new Examine(The.InGameUI.SelectedZone.ID, giveClientFeedback: true, expeditionGroupID));
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
	}

	public void OnForageArea(Zone zoneToForage)
	{
		SelectZone(zoneToForage);
		Hide();
	}

	public void OnSetStandingGatherOrder(Zone zone)
	{
		SelectZone(zone);
	}

	public void OnGather(Zone zone)
	{
		SelectZone(zone);
	}
}
