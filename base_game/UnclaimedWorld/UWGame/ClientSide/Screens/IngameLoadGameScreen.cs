using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.MainMenu.LoadMap;
using UWGame.Control;
using UWGame.SimSide;

namespace UWGame.ClientSide.Screens;

public class IngameLoadGameScreen : GameScreen
{
	private UnclaimedWorld game;

	public IngameLoadGameInterface Interface;

	public IngameLoadGameScreen(UnclaimedWorld game)
	{
		this.game = game;
		if (game.GraphicsDeviceManager.GraphicsDevice != null)
		{
			Interface = new IngameLoadGameInterface(this, game);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		Interface.Draw(gameTime);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (base.IsActive)
		{
			Interface.Update(gameTime);
		}
	}

	public override void LoadContent()
	{
		base.LoadContent();
		if (Interface == null)
		{
			Interface = new IngameLoadGameInterface(this, game);
		}
		Interface.LoadContent();
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		Interface.UnloadContent();
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
