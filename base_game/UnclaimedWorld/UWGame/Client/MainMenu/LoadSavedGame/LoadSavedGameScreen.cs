using System.IO;
using System.IO.Compression;
using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.Client.MainMenu.LoadSavedGame;

internal class LoadSavedGameScreen : GameScreen
{
	private LoadSavedGameInterface intf;

	private InputData inputData;

	public LoadSavedGameScreen(Controller screenManager)
	{
		intf = new LoadSavedGameInterface(this, screenManager.Game);
		inputData = screenManager.InputData;
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void HandleInput()
	{
		if (inputData.IsKeyDown(Keys.Escape))
		{
			ExitToMainMenu();
		}
	}

	public override void Destroy()
	{
		intf.Destroy();
		intf = null;
	}

	public void ExitToMainMenu()
	{
		ExitScreen();
		base.Controller.AddScreen(new MainMenuScreen(base.Controller.Game));
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

	public static void LoadSavedGameWithLoadingScreen(Controller controller, string savegamePath, SnapshotHeader header)
	{
		header.StartGameParams.SavedGameToLoad = savegamePath;
		if (header.StartGameParams.StartScenarioParams != null)
		{
			header.StartGameParams.StartScenarioParams.LoadScenarioFromName();
		}
		LoadingScreen.StartTransitioningToGame(controller, header.StartGameParams, loadingIsSlow: true, showInterface: true);
	}

	public void LoadSavedGameWithLoadingScreen(string savegamePath)
	{
		SnapshotHeader header;
		using (BinaryReader reader = new BinaryReader(new BufferedStream(new GZipStream(File.Open(savegamePath, FileMode.Open), CompressionMode.Decompress), 65536)))
		{
			header = The.Snapshotter.LoadHeader(reader);
		}
		LoadSavedGameWithLoadingScreen(base.Controller, savegamePath, header);
	}
}
