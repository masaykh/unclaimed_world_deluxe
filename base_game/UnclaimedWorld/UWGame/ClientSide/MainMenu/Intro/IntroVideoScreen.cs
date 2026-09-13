using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;
using UWGame.Port;

namespace UWGame.ClientSide.MainMenu.Intro;

/// <summary>
/// The full-screen intro playback reached from MainMenuScreen.ShowIntro.
///
/// PORT DEVIATION 17 (see PORTING-NOTES.md). Was a MonoGame VideoPlayer on
/// Content/MainMenu/TauCetiMainMenu.wmv; now plays MainMenuIntro.uwanim through
/// <see cref="MenuAnimation"/>, like BackgroundScreen.
///
/// This screen also carried a live bug worth recording, now gone with the video player: its
/// Update assigned `player.IsLooped = true`, and MonoGame's VideoPlayer throws
/// NotImplementedException from PlatformSetIsLooped. PORT DEVIATION 5 removed that assignment
/// from BackgroundScreen but missed this one, so reaching this screen threw. A frame sequence
/// loops by construction, so there is nothing to set.
/// </summary>
internal class IntroVideoScreen : GameScreen
{
	private MenuAnimation animation;

	private Texture2D animationTexture;

	private SpriteBatch spriteBatch;

	private ContentManager content;

	private InputData frameInput;

	public IntroVideoScreen(Controller screenManager)
	{
		frameInput = screenManager.InputData;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		content = new UWGame.Port.UwContentManager(base.Controller.Game.Services);
		content.RootDirectory = "Content";
		spriteBatch = new SpriteBatch(base.Controller.GraphicsDevice);
		animation = MenuAnimation.Shared;
	}

	public override void Draw(GameTime gameTime)
	{
		base.Controller.GraphicsDevice.Clear(Color.Black);
		if (animation != null)
		{
			animationTexture = animation.GetFrame(base.Controller.GraphicsDevice, gameTime);
		}
		Rectangle destinationRectangle = new Rectangle(base.Controller.GraphicsDevice.Viewport.X, base.Controller.GraphicsDevice.Viewport.Y, 1280, 720);
		if (animationTexture != null)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(animationTexture, destinationRectangle, Color.White);
			spriteBatch.End();
		}
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
		// The animation loops on its own; there is no stopped state to restart. If the .uwanim
		// file is absent this screen shows black until Escape, which is the same as the old
		// behaviour when the video failed to start.
	}

	public override void UnloadContent()
	{
		// The shared animation outlives this screen - BackgroundScreen uses it too.
		animation = null;
		animationTexture = null;
		content.Unload();
		base.UnloadContent();
	}
}
