using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;

namespace UWGame.ClientSide.MainMenu.Mods;

/// <summary>
/// PORT: SELECT MODS (main menu -> MODDING), laid out like the scenario picker it was asked to
/// follow (SelectScenarioScreen). tripleacoder, "Main menu", 2026-10-10.
/// </summary>
public class SelectModsScreen : GameScreen
{
	private SelectModsInterface intf;

	private InputData frameInput;

	public SelectModsScreen(Controller screenManager)
	{
		intf = new SelectModsInterface(this, screenManager.Game);
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
}
