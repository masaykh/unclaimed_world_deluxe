using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface;

public class LCDQuad
{
	protected VertexLCDQuad[] quad;

	public void CopyQuadToVertexBuffer(VertexLCDQuad[] crtVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			crtVertices[i] = quad[i - index];
		}
	}

	public void DrawQuad(VertexLCDQuad[] featureVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			featureVertices[i] = quad[i - index];
		}
	}

	public void SetupQuadVertices(float posLeft, float posTop, float posRight, float posBottom, Rectangle spriteSheetSourceRect, int contentTextureWidth, int contentTextureHeight, int reflectionTextureWidth, int reflectionTextureHeight, int grungeTextureWidth, int grungeTextureHeight, float panelWidth, float panelHeight, bool drawWithDust, float alpha)
	{
		float z = 0f;
		quad = new VertexLCDQuad[4];
		float num = (float)spriteSheetSourceRect.X / (float)contentTextureWidth;
		float num2 = (float)spriteSheetSourceRect.Y / (float)contentTextureHeight;
		float x = num + (float)spriteSheetSourceRect.Width / (float)contentTextureWidth;
		float y = num2 + (float)spriteSheetSourceRect.Height / (float)contentTextureHeight;
		_ = panelWidth / (float)reflectionTextureWidth;
		_ = panelHeight / (float)reflectionTextureHeight;
		float x2 = 0f;
		float y2 = 1f - panelHeight / (float)grungeTextureHeight;
		float x3 = panelWidth / (float)grungeTextureWidth;
		float y3 = 1f;
		float drawDust = (drawWithDust ? 1f : 0f);
		quad[0] = default(VertexLCDQuad);
		quad[0].Position = new Vector3(posLeft, posTop, z);
		quad[0].TextureCoordinate = new Vector2(num, num2);
		quad[0].ScaledEffectCoordinate = new Vector2(0f, 0f);
		quad[0].GrungeCoordinate = new Vector2(x2, y2);
		quad[1] = default(VertexLCDQuad);
		quad[1].Position = new Vector3(posRight, posTop, z);
		quad[1].TextureCoordinate = new Vector2(x, num2);
		quad[1].ScaledEffectCoordinate = new Vector2(1f, 0f);
		quad[1].GrungeCoordinate = new Vector2(x3, y2);
		quad[2] = default(VertexLCDQuad);
		quad[2].Position = new Vector3(posRight, posBottom, z);
		quad[2].TextureCoordinate = new Vector2(x, y);
		quad[2].ScaledEffectCoordinate = new Vector2(1f, 1f);
		quad[2].GrungeCoordinate = new Vector2(x3, y3);
		quad[3] = default(VertexLCDQuad);
		quad[3].Position = new Vector3(posLeft, posBottom, z);
		quad[3].TextureCoordinate = new Vector2(num, y);
		quad[3].ScaledEffectCoordinate = new Vector2(0f, 1f);
		quad[3].GrungeCoordinate = new Vector2(x2, y3);
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].DrawDust = drawDust;
			quad[i].Alpha = alpha;
		}
	}
}
