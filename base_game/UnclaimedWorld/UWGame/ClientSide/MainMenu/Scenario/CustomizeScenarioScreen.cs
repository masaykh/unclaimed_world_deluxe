using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;

namespace UWGame.ClientSide.MainMenu.Scenario;

public class CustomizeScenarioScreen : GameScreen
{
	private CustomizeScenarioInterface intf;

	public UWGame.SimSide.Scenarios.Scenario Scenario;

	private InputData frameInput;

	public CustomizeScenarioScreen(Controller screenManager, UWGame.SimSide.Scenarios.Scenario scenario)
	{
		intf = new CustomizeScenarioInterface(this, screenManager.Game);
		Scenario = scenario;
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

	public void StartGame(SerializableDictionary<string, Option> options, Difficulty mainDifficulty)
	{
		string mainDifficultyKey = null;
		if (mainDifficulty != null)
		{
			mainDifficultyKey = mainDifficulty.KeyName;
		}
		LoadingScreen.StartTransitioningToGame(base.Controller, new StartGameParams
		{
			StartScenarioParams = new StartScenarioParams
			{
				Scenario = Scenario,
				Options = options,
				MainDifficultyKey = mainDifficultyKey,
				Source = Scenario.Source,
				ScenarioName = Scenario.Name
			}
		}, loadingIsSlow: true, showInterface: true);
	}
}
