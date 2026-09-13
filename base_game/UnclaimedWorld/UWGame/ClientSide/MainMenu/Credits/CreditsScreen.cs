using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;

namespace UWGame.ClientSide.MainMenu.Credits;

public class CreditsScreen : GameScreen
{
	private CreditsInterface intf;

	private InputData frameInput;

	public CreditsScreen(Controller screenManager)
	{
		intf = new CreditsInterface(this, screenManager.Game);
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
