using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;

namespace UWGame.Client.MainMenu.LoadSavedGame;

internal class LoadSavedGameInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	public SaveLoadGamePanel loadPanel;

	public LoadSavedGameScreen loadGameScreen;

	// PORT: addTooltip, which this screen never asked for. CommonInterface only creates a Tooltip
	// when told to, and without one nothing on this screen can show a tooltip at all - which
	// nobody noticed, because until the save list started marking modded saves there was nothing
	// on it with a tooltip to show. Its sibling LoadMapInterface has always passed true. Reported
	// as "Savegame menu work fine ... but Loadgame menu not show tooltip at MODDED", which is
	// exactly the difference: the save dialog runs under the in-game interface, which has one.
	public LoadSavedGameInterface(LoadSavedGameScreen loadGameScreen, UnclaimedWorld game)
		: base(game, addGuiManagerNow: true, addTooltip: true)
	{
		this.loadGameScreen = loadGameScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		loadPanel = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Load, this, new Point(left, top));
		loadPanel.CancelClick += loadPanel_CancelClick;
		loadPanel.SaveOrLoadClick += loadPanel_LoadClick;
		loadPanel.ShowDialog(modal: true);
		SetInterfaceCursor();
	}

	private void loadPanel_LoadClick(object sender, EventArgs e)
	{
		loadGameScreen.LoadSavedGameWithLoadingScreen(loadPanel.SelectedSaveGamePath);
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		loadGameScreen.ExitToMainMenu();
	}
}
