using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class CRTScreen : DisplayScreen
{
	protected CRTQuad quad;

	protected bool isMonochrome;

	protected float alpha;

	protected ReflectionToUse reflectionToUse;

	public CRTScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight, bool isMonochrome, ReflectionToUse toUse, Window window, float alpha = 1f)
		: base(displayBox, src, dest, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, window)
	{
		this.isMonochrome = isMonochrome;
		reflectionToUse = toUse;
		this.alpha = alpha;
		quad = new CRTQuad();
		SetupQuadVertices();
	}

	public new void SetupQuadVertices()
	{
		base.SetupQuadVertices();
		_ = Vector2.Zero;
		float num = destinationRectangle.X;
		float posRight = num + (float)destinationRectangle.Width;
		float num2 = destinationRectangle.Y;
		float posBottom = num2 + (float)destinationRectangle.Height;
		quad.SetupQuadVertices(num, num2, posRight, posBottom, sourceRectangle, contentTextureWidth, contentTextureHeight, effectTextureWidth, effectTextureHeight, destinationRectangle.Width, destinationRectangle.Height, reflectionToUse, isMonochrome, alpha * ((ParentWindow != null) ? ParentWindow.TransitionValue : 1f));
	}

	public void CopyQuadToVertexBuffer(VertexCRTQuad[] featureVertices, int index)
	{
		if (IsDirty)
		{
			SetupQuadVertices();
		}
		quad.CopyQuadToVertexBuffer(featureVertices, index);
	}
}
