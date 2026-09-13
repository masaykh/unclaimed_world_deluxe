using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using UWGame.Client.MainMenu.LoadReplay;
using UWGame.Client.MainMenu.LoadSavedGame;
using UWGame.ClientSide.MainMenu.Credits;
using UWGame.ClientSide.MainMenu.Intro;
using UWGame.ClientSide.MainMenu.LoadMap;
using UWGame.ClientSide.MainMenu.Scenario;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.Scenarios;

namespace GameStateManagement;

public class MainMenuScreen : GameScreen
{
	private MainMenuInterface intf;

	private UnclaimedWorld game;

	private Song titleSong;

	public MainMenuScreen(UnclaimedWorld game)
	{
		this.game = game;
		// Back at the menu means no game is running, which is where a save's mod settings stop
		// standing in for the player's own. Applying them is scoped to the session that opened the
		// save; the file was never rewritten, so this is only putting the in-memory values back.
		UWGame.Mods.ModSettings.RestoreAfterSaveApplied();
		if (game.GraphicsDeviceManager.GraphicsDevice != null)
		{
			intf = new MainMenuInterface(this, game);
		}
	}

	protected override float getStringEnlargementFactor()
	{
		return 1.4f;
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		titleSong = base.Controller.Content.Load<Song>("Music\\Jesper Lundager - Prosperous Frontier_320");
		base.Controller.AudioManager.StopSong();
		base.Controller.AudioManager.PlaySong(titleSong, loop: true);
		base.Controller.AudioManager.Resume();
		if (intf != null)
		{
			intf.LoadContent();
		}
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		intf.UnloadContent();
	}

	public override void Destroy()
	{
		intf.Destroy();
		intf = null;
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (intf == null)
		{
			intf = new MainMenuInterface(this, game);
			intf.LoadContent();
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (base.IsActive)
		{
			intf.Update(gameTime);
		}
	}

	public void Exit()
	{
		base.Controller.Game.Exit();
	}

	public void ShowIntro()
	{
		ExitScreen();
		base.Controller.AddScreen(new IntroVideoScreen(base.Controller));
	}

	public void ShowCredits()
	{
		ExitScreen();
		base.Controller.AddScreen(new CreditsScreen(base.Controller));
	}

	public void ShowScenarios()
	{
		ExitScreen();
		base.Controller.AddScreen(new SelectScenarioScreen(base.Controller));
	}

	public void TestMap()
	{
		LoadingScreen.StartTransition(base.Controller, ShowTestGameScreen, loadingIsSlow: false);
	}

	public void EditMap()
	{
		LoadingScreen.StartTransition(base.Controller, ShowMapEditorScreen, loadingIsSlow: false);
	}

	public void LoadReplay()
	{
		LoadingScreen.StartTransition(base.Controller, ShowLoadReplayScreen, loadingIsSlow: false);
	}

	public void LoadGame()
	{
		LoadingScreen.StartTransition(base.Controller, ShowLoadGameScreen, loadingIsSlow: false);
	}

	public void StartTest()
	{
		LoadingScreen.StartTransitioningToGame(base.Controller, new StartGameParams
		{
			StartDebugScenarioParams = PlaceGameEntities.GetNewScenarioParams()
		}, loadingIsSlow: true, showInterface: true);
	}

	public void DummyEventHandler(object caller, EventArgs e)
	{
	}

	public override void HandleInput()
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

	private void ShowLoadReplayScreen(object sender, EventArgs e)
	{
		base.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		LoadReplayScreen screen = new LoadReplayScreen(base.Controller);
		base.Controller.AddScreen(screen);
	}

	private void ShowLoadGameScreen(object sender, EventArgs e)
	{
		base.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		LoadSavedGameScreen screen = new LoadSavedGameScreen(base.Controller);
		base.Controller.AddScreen(screen);
	}

	private void ShowTestGameScreen(object sender, EventArgs e)
	{
		base.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		LoadMapScreen screen = new LoadMapScreen(base.Controller, Sim.EngineMode.Game, isTestingGame: true);
		base.Controller.AddScreen(screen);
	}
}
