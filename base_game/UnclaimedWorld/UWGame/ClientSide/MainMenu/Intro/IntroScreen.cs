using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;

namespace UWGame.ClientSide.MainMenu.Intro;

internal class IntroScreen : GameScreen
{
	private IntroInterface intf;

	private InputData frameInput;

	public IntroScreen(Controller screenManager)
	{
		intf = new IntroInterface(screenManager.Game);
		frameInput = screenManager.InputData;
	}

	public override void LoadContent()
	{
		intf.LoadContent();
		base.LoadContent();
		intf.framedCRT.Show();
		intf.framedCRT.TurnOn();
		intf.StartMovie();
	}

	public override void Draw(GameTime gameTime)
	{
		intf.Draw(gameTime);
	}

	public override void HandleInput()
	{
		if (frameInput.IsKeyDown(Keys.Escape))
		{
			ExitScreen();
			base.Controller.AddScreen(new MainMenuScreen(base.Controller.Game));
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (base.IsActive)
		{
			intf.Update(gameTime);
		}
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		intf.UnloadContent();
	}
}
