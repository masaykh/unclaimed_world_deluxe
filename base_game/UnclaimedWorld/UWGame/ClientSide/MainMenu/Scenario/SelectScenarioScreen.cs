using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Scenarios;

namespace UWGame.ClientSide.MainMenu.Scenario;

public class SelectScenarioScreen : GameScreen
{
	private SelectScenarioInterface intf;

	private InputData frameInput;

	public SelectScenarioScreen(Controller screenManager)
	{
		intf = new SelectScenarioInterface(this, screenManager.Game);
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

	public void SelectScenario()
	{
		ExitScreen();
		base.Controller.AddScreen(new SelectScenarioScreen(base.Controller));
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

	public void SelectScenario(UWGame.SimSide.Scenarios.Scenario scenario)
	{
		scenario.ScenarioData = AllScenarioLoader.LoadScenarioData(scenario);
		ExitScreen();
		base.Controller.AddScreen(new CustomizeScenarioScreen(base.Controller, scenario));
	}
}
