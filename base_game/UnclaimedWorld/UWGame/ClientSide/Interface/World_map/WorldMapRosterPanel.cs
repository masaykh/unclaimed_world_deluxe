using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.World_map;

public class WorldMapRosterPanel : RosterPanel
{
	private const int windowWidth = 600;

	private WorldMap worldMap;

	public WorldMapRosterPanel()
		: base("WORLD MAP", 600, Math.Min(584, The.InGameUI.rosterPanelHeight), needBottomMarginForButton: false)
	{
		worldMap = new WorldMap(lcdSurface, 525, 440);
		worldMap.ChildDialogDisplayed += worldMap_ChildDialogDisplayed;
		worldMap.ChildDialogClosed += worldMap_ChildDialogClosed;
	}

	public override void Hide()
	{
		base.Hide();
		worldMap.HideOpenDialogs();
	}

	private void worldMap_ChildDialogClosed()
	{
		RemoveModalOverlay();
	}

	private void worldMap_ChildDialogDisplayed()
	{
		ShowModalOverlay();
	}

	public override void Refresh()
	{
		base.Refresh();
		worldMap.Update();
	}

	public override void Show()
	{
		EntityGroupID? uIOwner = The.InGameUI.UIOwner;
		worldMap.Fill(The.Sim.World, uIOwner, isMissionAction: false);
		base.Show();
	}
}
