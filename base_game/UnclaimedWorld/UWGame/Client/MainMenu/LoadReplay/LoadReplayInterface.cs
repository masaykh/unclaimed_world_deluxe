using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;

namespace UWGame.Client.MainMenu.LoadReplay;

internal class LoadReplayInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	public LoadReplayPanel loadPanel;

	public LoadReplayScreen loadReplayScreen;

	public LoadReplayInterface(LoadReplayScreen loadReplayScreen, UnclaimedWorld game)
		: base(game)
	{
		this.loadReplayScreen = loadReplayScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		loadPanel = new LoadReplayPanel(this, new Point(left, top));
		loadPanel.CancelClick += loadPanel_CancelClick;
		loadPanel.LoadClick += loadPanel_LoadClick;
		loadPanel.Show();
		SetInterfaceCursor();
	}

	private void loadPanel_LoadClick(object sender, EventArgs e)
	{
		loadReplayScreen.LoadReplay(loadPanel.SelectedLoadReplayPath, loadPanel.TimeToPauseReplay);
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		loadReplayScreen.ExitToMainMenu();
	}
}
