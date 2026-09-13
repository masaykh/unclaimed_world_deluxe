using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface;

public class CRTQuad
{
	protected VertexCRTQuad[] quad;

	public void CopyQuadToVertexBuffer(VertexCRTQuad[] crtVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			crtVertices[i] = quad[i - index];
		}
	}

	public void DrawQuad(VertexCRTQuad[] featureVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			featureVertices[i] = quad[i - index];
		}
	}

	public void SetupQuadVertices(float posLeft, float posTop, float posRight, float posBottom, Rectangle spriteSheetSourceRect, int contentTextureWidth, int contentTextureHeight, int crtEffectTextureWidth, int crtEffectTextureHeight, float panelWidth, float panelHeight, ReflectionToUse toUse, bool isMonochrome, float alpha)
	{
		float z = 0f;
		quad = new VertexCRTQuad[4];
		float num = (float)spriteSheetSourceRect.X / (float)contentTextureWidth;
		float num2 = (float)spriteSheetSourceRect.Y / (float)contentTextureHeight;
		float x = num + (float)spriteSheetSourceRect.Width / (float)contentTextureWidth;
		float y = num2 + (float)spriteSheetSourceRect.Height / (float)contentTextureHeight;
		float x2 = 0f;
		float y2 = 0f;
		float x3 = panelWidth / (float)crtEffectTextureWidth;
		float y3 = panelHeight / (float)crtEffectTextureHeight;
		float isMonochrome2 = (isMonochrome ? 1f : 0f);
		quad[0] = default(VertexCRTQuad);
		quad[0].Position = new Vector3(posLeft, posTop, z);
		quad[0].TextureCoordinate = new Vector2(num, num2);
		quad[0].TruncatedEffectCoordinate = new Vector2(x2, y2);
		quad[0].ScaledEffectCoordinate = new Vector2(0f, 0f);
		quad[1] = default(VertexCRTQuad);
		quad[1].Position = new Vector3(posRight, posTop, z);
		quad[1].TextureCoordinate = new Vector2(x, num2);
		quad[1].TruncatedEffectCoordinate = new Vector2(x3, y2);
		quad[1].ScaledEffectCoordinate = new Vector2(1f, 0f);
		quad[2] = default(VertexCRTQuad);
		quad[2].Position = new Vector3(posRight, posBottom, z);
		quad[2].TextureCoordinate = new Vector2(x, y);
		quad[2].TruncatedEffectCoordinate = new Vector2(x3, y3);
		quad[2].ScaledEffectCoordinate = new Vector2(1f, 1f);
		quad[3] = default(VertexCRTQuad);
		quad[3].Position = new Vector3(posLeft, posBottom, z);
		quad[3].TextureCoordinate = new Vector2(num, y);
		quad[3].TruncatedEffectCoordinate = new Vector2(x2, y3);
		quad[3].ScaledEffectCoordinate = new Vector2(0f, 1f);
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].IsMonochrome = isMonochrome2;
			quad[i].Alpha = alpha;
		}
	}
}
