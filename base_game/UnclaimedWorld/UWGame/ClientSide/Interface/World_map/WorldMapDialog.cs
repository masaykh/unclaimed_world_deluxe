using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.LCD;
using UWGame.ClientSide.Interface.Missions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using WindowSystem;

namespace UWGame.ClientSide.Interface.World_map;

public class WorldMapDialog : Panel
{
	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Box display;

	private Label lblInfo;

	public WorldMap worldMap;

	public TravelLocation? SelectedTravelLocation;

	private ModalOverlay modalOverlay;

	public event EventHandler OKClick;

	public event EventHandler CancelClick;

	public WorldMapDialog(CommonInterface intf, Point position)
		: base(intf, "WORLD MAP", position, new Vector2(590f, Math.Min(617, The.InGameUI.rosterPanelHeight)), Level.Dialogs)
	{
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen);
		lcdSurface.DebugTag = "worldmapDlgLcdSurface";
		modalOverlay = new ModalOverlay(Window);
		worldMap = new WorldMap(lcdSurface, 525, 440);
		worldMap.TerminalSelected += worldMap_TerminalSelected;
		worldMap.ChildDialogDisplayed += worldMap_ChildDialogDisplayed;
		worldMap.ChildDialogClosed += worldMap_ChildDialogClosed;
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: true, 1f);
		lcdSurface.Add(lCDInnerPanel.Panel);
		lCDInnerPanel.ContentHeight = 30;
		lCDInnerPanel.Panel.Y = lcdSurface.Height - 35;
		lCDInnerPanel.VerticalContentPadding = 8;
		lblInfo = new Label(Interface.gui);
		lcdSurface.Add(lblInfo);
		lblInfo.Init(Label.LabelType.LCDNormal);
		lblInfo.Y = lCDInnerPanel.Panel.Y + 10;
		lblInfo.X = 10;
		AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right).Click += btCancel_Click;
	}

	public void ShowModalOverlay()
	{
		modalOverlay.Show(Window, lcdSurface, display);
	}

	public void RemoveModalOverlay()
	{
		modalOverlay.Remove(Window, lcdSurface, display);
	}

	private void worldMap_ChildDialogClosed()
	{
		RemoveModalOverlay();
	}

	private void worldMap_ChildDialogDisplayed()
	{
		ShowModalOverlay();
	}

	private void worldMap_TerminalSelected(TravelLocation travelLocation)
	{
		if (The.InGameUI.CreateMissionPanel.CanSelectLocation(travelLocation, out var _, out var _))
		{
			SelectedTravelLocation = travelLocation;
			AcceptAndClose();
		}
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}

	public void btOK_Click(UIComponent sender, EventArgs e)
	{
		AcceptAndClose();
	}

	private void AcceptAndClose()
	{
		if (this.OKClick != null)
		{
			this.OKClick(this, null);
		}
		Hide();
	}

	public void UnCheckSiteMarkerButtons()
	{
		worldMap.UnCheckSiteMarkerButtons();
	}

	public override void Refresh()
	{
		base.Refresh();
		worldMap.Update();
	}

	public void Fill(EntityGroupID? buyer)
	{
		if (The.InGameUI.CreateMissionPanel.worldMapDialogSource == CreateMissionPanel.WorldMapDialogSource.Start)
		{
			lblInfo.Text = "Select a starting location for the mission!";
		}
		else
		{
			lblInfo.Text = "Select a destination for the mission!";
		}
		worldMap.Fill(The.Sim.World, buyer, isMissionAction: true);
	}
}
