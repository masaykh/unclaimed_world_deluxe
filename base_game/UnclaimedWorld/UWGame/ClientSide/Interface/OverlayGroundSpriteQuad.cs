using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Interface;

public class OverlayGroundSpriteQuad
{
	protected VertexOverlayGroundSpriteQuad[] quad;

	public bool CopyQuadToVertexBuffer(VertexOverlayGroundSpriteQuad[] crtVertices, int index)
	{
		index *= 4;
		int num = index + 4;
		if (num < crtVertices.Length)
		{
			for (int i = index; i < num; i++)
			{
				crtVertices[i] = quad[i - index];
			}
			return true;
		}
		return false;
	}

	public void DrawQuad(VertexOverlayGroundSpriteQuad[] featureVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			featureVertices[i] = quad[i - index];
		}
	}

	public void SetupQuadVertices(float posLeft, float posTop, float posRight, float posBottom, Rectangle spriteSheetSourceRect, Texture2D spriteSheetTexture, bool drawScanlines, Vector4 color)
	{
		if (The.Client != null)
		{
			bool flag = false;
			float z = 0f;
			if (quad == null)
			{
				quad = new VertexOverlayGroundSpriteQuad[4];
			}
			int width = The.Client.Renderer.Scanlines.Width;
			int width2 = The.Client.Renderer.Scanlines.Width;
			float num = (float)spriteSheetSourceRect.X / (float)spriteSheetTexture.Width;
			float num2 = (float)spriteSheetSourceRect.Y / (float)spriteSheetTexture.Height;
			float x = num + (float)spriteSheetSourceRect.Width / (float)spriteSheetTexture.Width;
			float y = num2 + (float)spriteSheetSourceRect.Height / (float)spriteSheetTexture.Height;
			float x2 = 0f;
			float y2 = 0f;
			float x3 = (float)spriteSheetSourceRect.Width / (float)width;
			float y3 = (float)spriteSheetSourceRect.Height / (float)width2;
			quad[0] = default(VertexOverlayGroundSpriteQuad);
			quad[0].Position = new Vector3(posLeft, posTop, z);
			quad[0].TextureCoordinate = (flag ? new Vector2(x, num2) : new Vector2(num, num2));
			quad[0].TruncatedEffectCoordinate = new Vector2(x2, y2);
			quad[1] = default(VertexOverlayGroundSpriteQuad);
			quad[1].Position = new Vector3(posRight, posTop, z);
			quad[1].TextureCoordinate = (flag ? new Vector2(num, num2) : new Vector2(x, num2));
			quad[1].TruncatedEffectCoordinate = new Vector2(x3, y2);
			quad[2] = default(VertexOverlayGroundSpriteQuad);
			quad[2].Position = new Vector3(posRight, posBottom, z);
			quad[2].TextureCoordinate = (flag ? new Vector2(num, y) : new Vector2(x, y));
			quad[2].TruncatedEffectCoordinate = new Vector2(x3, y3);
			quad[3] = default(VertexOverlayGroundSpriteQuad);
			quad[3].Position = new Vector3(posLeft, posBottom, z);
			quad[3].TextureCoordinate = (flag ? new Vector2(x, y) : new Vector2(num, y));
			quad[3].TruncatedEffectCoordinate = new Vector2(x2, y3);
			float drawScanlines2 = (drawScanlines ? 1f : 0f);
			for (int i = 0; i < quad.Length; i++)
			{
				quad[i].DrawScanlines = drawScanlines2;
				quad[i].Color = color;
			}
		}
	}
}
