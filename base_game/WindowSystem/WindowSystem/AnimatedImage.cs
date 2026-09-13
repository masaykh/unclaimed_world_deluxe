using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class AnimatedImage : Icon
{
	private Rectangle? currentRect;

	public Animation2DPlayer Player;

	public Texture2D Texture
	{
		set
		{
			if (GetSkin(0) == null)
			{
				SetSkinLocation(0, new Rectangle(0, 0, value.Width, value.Height));
			}
			ComponentSkin skin = GetSkin(0);
			skin.UseCustomSkin = true;
			skin.Skin = value;
			RefreshSkins();
		}
	}

	public AnimatedImage(GUIManager guiManager)
		: base(guiManager)
	{
		Player = new Animation2DPlayer();
	}

	public void Reset()
	{
		base.CurrentSkin = -1;
		Redraw();
	}

	public override void Update(GameTime gameTime)
	{
		Player.Update(gameTime);
		base.Update(gameTime);
	}

	public void StartAnimation(Animation2D animation)
	{
		Player.StartAnimation(animation);
	}

	protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
	{
		if (Player.Animation == null)
		{
			return;
		}
		Rectangle frame = Player.GetFrame();
		if (currentRect.HasValue)
		{
			Rectangle value = frame;
			Rectangle? rectangle = currentRect;
			if (!(value != rectangle))
			{
				goto IL_0078;
			}
		}
		SetSkinLocation(0, frame);
		currentRect = frame;
		goto IL_0078;
		IL_0078:
		base.Color = Player.GetCurrentColor(null);
		base.DrawControl(spriteBatch, parentScissor, alpha);
	}
}
