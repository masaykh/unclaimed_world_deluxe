using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.MainMenu.LoadMap;
using UWGame.Control;
using UWGame.SimSide;

namespace UWGame.ClientSide.Screens;

public class LoseGameScreen : GameScreen
{
	private LoseGameInterface intf;

	private string text;

	public LoseGameScreen(Controller screenManager, string text)
	{
		intf = new LoseGameInterface(this, screenManager.Game);
		this.text = text;
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (base.IsActive)
		{
			intf.Update(gameTime);
		}
	}

	public void ExitToMainMenu()
	{
		ExitScreen();
		base.Controller.AddScreen(new MainMenuScreen(base.Controller.Game));
	}

	public override void LoadContent()
	{
		base.LoadContent();
		intf.LoadContent();
		intf.Panel.Text = text;
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		intf.UnloadContent();
	}

	public void DummyEventHandler(object caller, EventArgs e)
	{
	}

	private void ExitMessageBoxAccepted(object sender, EventArgs e)
	{
		base.Controller.Game.Exit();
	}

	private void ShowMapEditorScreen(object sender, EventArgs e)
	{
		base.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		LoadMapScreen screen = new LoadMapScreen(base.Controller, Sim.EngineMode.Edit);
		base.Controller.AddScreen(screen);
	}

	private void ShowTestGameScreen(object sender, EventArgs e)
	{
		base.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		LoadMapScreen screen = new LoadMapScreen(base.Controller, Sim.EngineMode.Game, isTestingGame: true);
		base.Controller.AddScreen(screen);
	}
}
