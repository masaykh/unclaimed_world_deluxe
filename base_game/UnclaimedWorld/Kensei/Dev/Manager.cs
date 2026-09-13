using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kensei.Dev;

public static class Manager
{
	private static SpriteFont s_font;

	private static SpriteBatch s_spriteBatch;

	internal static SpriteFont SpriteFont => s_font;

	internal static SpriteBatch SpriteBatch => s_spriteBatch;

	public static void Initialise(ContentManager content, GraphicsDevice device, int drawableAreaX, int drawableAreaY, int drawableAreaWidth, int drawableAreaHeight)
	{
		s_font = content.Load<SpriteFont>("Arial");
		s_spriteBatch = new SpriteBatch(device);
		Options.Initialise();
		DevText.Initialise(device, drawableAreaX, drawableAreaY, drawableAreaWidth, drawableAreaHeight);
		Shape.Initialise(content, device);
	}

	public static void Update()
	{
		Command.Update(Keyboard.GetState());
	}

	public static void Draw(GraphicsDevice device, Matrix renderMatrix, float width, float height)
	{
		Command.PreDraw(width, height);
		Shape.Draw(device, renderMatrix, width, height);
		Command.Draw(width, height);
		DevText.Draw(renderMatrix, width, height);
	}

	public static void Shutdown()
	{
	}
}
