using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using UWGame.Port;

namespace UWGame.Control;

internal class BackgroundScreen : GameScreen
{
	public enum Background
	{
		Normal,
		BlueTint
	}

	// PORT DEVIATION 17 (see PORTING-NOTES.md).
	//
	// Was a MonoGame VideoPlayer playing Content/MainMenu/TauCetiMainMenu.wmv. It now plays
	// MainMenuIntro.uwanim from the game root - the same footage as a motion-JPEG frame
	// sequence, decoded by the StbImageSharp already inside MonoGame.
	//
	// This retires PORT DEVIATIONS 5, 7 and 11, which all existed to work around VideoPlayer:
	//
	//   5 - `IsLooped = true` throws NotImplementedException in MonoGame (the studio's 3.6 fork
	//       had an empty PlatformSetIsLooped, so the assignment never did anything).
	//   7 - a second concurrent VideoPlayer never reaches MediaState.Playing, so opening Load
	//       Game on top of the menu threw "cannot start video". Hence one shared player.
	//   11 - MonoGame has no DesktopGL VideoPlayer at all, so GL had no menu animation.
	//
	// None of the three has anything to work around now, and GL gets the animation for the first
	// time. MenuAnimation is shared for the same reason the VideoPlayer was: several menu screens
	// sit on the stack at once and must not each run their own copy.
	private MenuAnimation animation;

	private Texture2D animationTexture;

	private ContentManager content;

	private Texture2D backgroundTexture;

	private Rectangle backgroundDest;

	private Rectangle titleDest;

	private Background backgroundType;

	public BackgroundScreen(Background background)
	{
		backgroundType = background;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new UWGame.Port.UwContentManager(base.Controller.Game.Services);
			content.RootDirectory = "Content";
		}

		// MenuAnimation.Shared is null when MainMenuIntro.uwanim is absent, which is the
		// supported way to turn the animation off - the still image is then used, exactly as it
		// already is when the PlayVideo option is cleared.
		if (base.Controller.Options.PlayVideo)
		{
			animation = MenuAnimation.Shared;

			// A .uwanim can parse and still be undecodable - the container says nothing about the
			// JPEG encoding inside it. Ask before committing to it, because the alternative is
			// what a modder actually got: no animation AND no still image, because the still is
			// only loaded when the animation is missing, and this one was present.
			if (animation != null && !animation.TryPrepare(base.Controller.GraphicsDevice))
			{
				animation = null;
			}
		}
		if (animation == null)
		{
			backgroundTexture = content.Load<Texture2D>("MainMenu/TauCetiMainMenuBGOnly_1280px");
		}
		InitSize();
	}

	public override void UnloadContent()
	{
		// The shared animation is deliberately NOT disposed - other BackgroundScreens may still
		// be on the stack using it, which is the same reason the old shared VideoPlayer was left
		// alone here.
		animation = null;
		animationTexture = null;
		content.Unload();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
		// Nothing to do: the animation advances in Draw. Restarting a stopped video used to
		// happen here - a frame sequence simply loops, so there is no stopped state to recover
		// from and no way for the background to stay frozen.
	}

	private void InitSize()
	{
		Dimension drawArea = base.Controller.DrawArea;
		int num = 1280;
		int num2 = 720;
		float num3 = (float)drawArea.Height / (float)num2;
		int height = drawArea.Height;
		int num4 = (int)(num3 * (float)num);
		backgroundDest = new Rectangle((drawArea.Width - num4) / 2, (drawArea.Height - height) / 2, num4, height);
	}

	public override void Draw(GameTime gameTime)
	{
		byte transitionAlpha = base.TransitionAlpha;
		base.Controller.GraphicsDevice.Clear(Color.Black);

		if (animation != null)
		{
			// The destination rectangle is derived from 1280x720 and scaled to the draw area, so
			// the stored frames need not match the screen or the original video size - they are
			// stretched to the same rectangle the video was.
			animationTexture = animation.GetFrame(base.Controller.GraphicsDevice, gameTime);
		}
		if (animationTexture != null)
		{
			base.Controller.SpriteBatch.Begin();
			base.Controller.SpriteBatch.Draw(animationTexture, backgroundDest, new Color(transitionAlpha, transitionAlpha, transitionAlpha));
			base.Controller.SpriteBatch.End();
		}
		if (backgroundTexture != null)
		{
			base.Controller.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			base.Controller.SpriteBatch.Draw(backgroundTexture, backgroundDest, new Color(transitionAlpha, transitionAlpha, transitionAlpha));
			base.Controller.SpriteBatch.End();
		}
	}
}
