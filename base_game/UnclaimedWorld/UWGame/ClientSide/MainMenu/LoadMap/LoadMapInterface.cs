using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;

namespace UWGame.ClientSide.MainMenu.LoadMap;

public class LoadMapInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	public MapEditorSaveLoadPanel loadPanel;

	public LoadMapScreen loadMapScreen;

	public LoadMapInterface(LoadMapScreen createGameScreen, UnclaimedWorld game)
		: base(game, addGuiManagerNow: true, addTooltip: true)
	{
		loadMapScreen = createGameScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public override void LoadContent()
	{
		base.LoadContent();
		loadPanel = new MapEditorSaveLoadPanel(MapEditorSaveLoadPanel.SaveOrLoad.Load, this, new Point(0, 0));
		loadPanel.Window.X = (Game.Controller.DrawArea.Width - loadPanel.Window.Width) / 2;
		loadPanel.Window.Y = (Game.Controller.DrawArea.Height - loadPanel.Window.Height) / 2;
		loadPanel.SaveOrLoadClick += loadPanel_SaveOrLoadClick;
		loadPanel.CancelClick += loadPanel_CancelClick;
		loadPanel.ShowDialog(modal: true);
		SetInterfaceCursor();
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		loadMapScreen.ExitToMainMenu();
	}

	private void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
	{
		loadMapScreen.StartMapEditor(loadPanel.SelectedMapFolder);
	}
}
