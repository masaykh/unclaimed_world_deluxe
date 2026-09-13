using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class LCDScreen : DisplayScreen
{
	public enum ReflectionToUse
	{
		Circular,
		Lamp
	}

	protected LCDQuad quad;

	public bool DrawDust = true;

	protected int grungeTextureWidth;

	protected int grungeTextureHeight;

	public LCDScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight, int grungeTextureWidth, int grungeTextureHeight, Window window, bool drawDust)
		: base(displayBox, src, dest, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, window)
	{
		this.grungeTextureWidth = grungeTextureWidth;
		this.grungeTextureHeight = grungeTextureHeight;
		DrawDust = drawDust;
		quad = new LCDQuad();
		SetupQuadVertices();
	}

	protected override void SetupQuadVertices()
	{
		base.SetupQuadVertices();
		_ = Vector2.Zero;
		float num = destinationRectangle.X;
		float posRight = num + (float)destinationRectangle.Width;
		float num2 = destinationRectangle.Y;
		float posBottom = num2 + (float)destinationRectangle.Height;
		quad.SetupQuadVertices(num, num2, posRight, posBottom, sourceRectangle, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, grungeTextureWidth, grungeTextureHeight, destinationRectangle.Width, destinationRectangle.Height, DrawDust, (ParentWindow != null) ? ParentWindow.TransitionValue : 1f);
	}

	public void CopyQuadToVertexBuffer(VertexLCDQuad[] featureVertices, int index)
	{
		if (IsDirty)
		{
			SetupQuadVertices();
		}
		quad.CopyQuadToVertexBuffer(featureVertices, index);
	}

	public virtual void Update(GameTime gameTime)
	{
	}

	public virtual void DrawContent()
	{
	}
}
