using Microsoft.Xna.Framework;

namespace UWGame.ClientSide;

public class OverlayQuad : QuadBase
{
	protected VertexOverlayQuad[] quad;

	public Vector4 GradientColor1 = Color.DarkGray.ToVector4();

	public Vector4 GradientColor2 = Color.LightGray.ToVector4();

	public Vector4 GradientColor3 = Color.White.ToVector4();

	public void SetProperties(Vector4 tint, Vector4 gradient1, Vector4 gradient2, Vector4 gradient3)
	{
		GradientColor1 = gradient1;
		GradientColor2 = gradient2;
		GradientColor3 = gradient3;
		for (int i = 0; i < quad.Length; i++)
		{
			VertexOverlayQuad vertexOverlayQuad = quad[i];
			vertexOverlayQuad.GradientColor1 = GradientColor1 * tint;
			vertexOverlayQuad.GradientColor2 = GradientColor2 * tint;
			vertexOverlayQuad.GradientColor3 = GradientColor3 * tint;
			quad[i] = vertexOverlayQuad;
		}
	}

	public void CopyQuadToVertexBuffer(VertexOverlayQuad[] crtVertices, int index)
	{
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			crtVertices[i] = quad[i - index];
		}
	}

	public void SetupQuadVertices(Vector3 worldPosition, float posLeft, float posTop, float posRight, float posBottom, bool drawScanlines, bool flipSprite)
	{
		float z = worldPosition.Z;
		quad = new VertexOverlayQuad[4];
		int width = The.Client.Renderer.Scanlines.Width;
		int width2 = The.Client.Renderer.Scanlines.Width;
		float num = (float)base.StaticSourceRectangle.X / (float)base.Texture.Width;
		float num2 = (float)base.StaticSourceRectangle.Y / (float)base.Texture.Height;
		float x = num + (float)base.StaticSourceRectangle.Width / (float)base.Texture.Width;
		float y = num2 + (float)base.StaticSourceRectangle.Height / (float)base.Texture.Height;
		float x2 = 0f;
		float y2 = 0f;
		float x3 = (float)base.StaticSourceRectangle.Width / (float)width;
		float y3 = (float)base.StaticSourceRectangle.Height / (float)width2;
		quad[0] = default(VertexOverlayQuad);
		quad[0].Position = new Vector3(posLeft, posTop, z);
		quad[0].WorldPosition = worldPosition;
		quad[0].TextureCoordinate = (flipSprite ? new Vector2(x, num2) : new Vector2(num, num2));
		quad[0].TruncatedEffectCoordinate = new Vector2(x2, y2);
		quad[1] = default(VertexOverlayQuad);
		quad[1].Position = new Vector3(posRight, posTop, z);
		quad[1].WorldPosition = worldPosition;
		quad[1].TextureCoordinate = (flipSprite ? new Vector2(num, num2) : new Vector2(x, num2));
		quad[1].TruncatedEffectCoordinate = new Vector2(x3, y2);
		quad[2] = default(VertexOverlayQuad);
		quad[2].Position = new Vector3(posRight, posBottom, z);
		quad[2].WorldPosition = worldPosition;
		quad[2].TextureCoordinate = (flipSprite ? new Vector2(num, y) : new Vector2(x, y));
		quad[2].TruncatedEffectCoordinate = new Vector2(x3, y3);
		quad[3] = default(VertexOverlayQuad);
		quad[3].Position = new Vector3(posLeft, posBottom, z);
		quad[3].WorldPosition = worldPosition;
		quad[3].TextureCoordinate = (flipSprite ? new Vector2(x, y) : new Vector2(num, y));
		quad[3].TruncatedEffectCoordinate = new Vector2(x2, y3);
		float drawScanlines2 = (drawScanlines ? 1f : 0f);
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].DrawScanlines = drawScanlines2;
			quad[i].GradientColor1 = GradientColor1;
			quad[i].GradientColor2 = GradientColor2;
			quad[i].GradientColor3 = GradientColor3;
		}
	}
}
