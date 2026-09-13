using System;
using System.IO;
using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.Scenarios;

namespace UWGame.ClientSide.MainMenu.LoadMap;

public class LoadMapScreen : GameScreen
{
	private LoadMapInterface intf;

	private Sim.EngineMode gameMode;

	private bool isTestingGame;

	private InputData frameInput;

	public LoadMapScreen(Controller screenManager, Sim.EngineMode gameMode, bool isTestingGame = false)
	{
		intf = new LoadMapInterface(this, screenManager.Game);
		this.gameMode = gameMode;
		this.isTestingGame = isTestingGame;
		frameInput = screenManager.InputData;
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void Destroy()
	{
		intf.Destroy();
		intf = null;
	}

	public override void HandleInput()
	{
		if (frameInput.IsKeyDown(Keys.Escape))
		{
			ExitToMainMenu();
		}
	}

	public void ExitToMainMenu()
	{
		ExitScreen();
		base.Controller.AddScreen(new MainMenuScreen(base.Controller.Game));
	}

	public void StartMapEditor(DirectoryInfo mapToLoad)
	{
		new Random().Next();
		LoadingScreen.StartTransitioningToGame(base.Controller, new StartGameParams
		{
			StartGameEditorParams = new StartGameEditorParams
			{
				MapToLoadPath = mapToLoad.FullName,
				EngineMode = gameMode
			}
		}, loadingIsSlow: true);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (base.IsActive)
		{
			intf.Update(gameTime);
		}
	}

	public override void LoadContent()
	{
		base.LoadContent();
		intf.LoadContent();
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		intf.UnloadContent();
	}
}
