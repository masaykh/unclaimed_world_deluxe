using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;

namespace UWGame.Client.MainMenu.LoadReplay;

internal class LoadReplayScreen : GameScreen
{
	private LoadReplayInterface intf;

	private InputData frameInput;

	public LoadReplayScreen(Controller screenManager)
	{
		intf = new LoadReplayInterface(this, screenManager.Game);
		frameInput = screenManager.InputData;
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void HandleInput()
	{
		if (frameInput.IsKeyDown(Keys.Escape))
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

	internal void LoadReplay(string path, float? timeToPauseReplay)
	{
		base.Controller.LoadReplay(path, timeToPauseReplay);
	}
}
