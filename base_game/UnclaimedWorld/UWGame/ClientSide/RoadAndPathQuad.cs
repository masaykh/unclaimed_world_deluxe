using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide;

public class RoadAndPathQuad
{
	protected VertexGroundFeature[] quad;

	public bool CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, int index)
	{
		index *= 4;
		if (index + 4 >= featureVertices.Length)
		{
			return false;
		}
		for (int i = index; i < index + 4; i++)
		{
			featureVertices[i] = quad[i - index];
		}
		return true;
	}

	public void SetProperties(Vector4 tint)
	{
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].TintColor = tint;
		}
	}

	public void SetupQuadVertices(Vector3 worldPosition, Vector2 baseCenterOffset, Rectangle sourceRect, Texture2D texture, bool flipSprite)
	{
		float num = sourceRect.Width;
		float num2 = sourceRect.Height;
		float num3 = 0f - baseCenterOffset.X;
		float x = num3 + num;
		float num4 = 0f - baseCenterOffset.Y;
		float y = num4 + num2;
		float z = 0f;
		quad = new VertexGroundFeature[4];
		float num5 = (float)sourceRect.X / (float)texture.Width;
		float num6 = (float)sourceRect.Y / (float)texture.Height;
		float x2 = num5 + (float)sourceRect.Width / (float)texture.Width;
		float y2 = num6 + (float)sourceRect.Height / (float)texture.Height;
		Vector4 one = Vector4.One;
		quad[0] = default(VertexGroundFeature);
		quad[0].Position = new Vector3(num3, num4, z);
		quad[0].TextureCoordinate = (flipSprite ? new Vector2(x2, num6) : new Vector2(num5, num6));
		quad[0].WorldPosition = worldPosition;
		quad[0].TintColor = one;
		quad[1] = default(VertexGroundFeature);
		quad[1].Position = new Vector3(x, num4, z);
		quad[1].TextureCoordinate = (flipSprite ? new Vector2(num5, num6) : new Vector2(x2, num6));
		quad[1].WorldPosition = worldPosition;
		quad[1].TintColor = one;
		quad[2] = default(VertexGroundFeature);
		quad[2].Position = new Vector3(x, y, z);
		quad[2].TextureCoordinate = (flipSprite ? new Vector2(num5, y2) : new Vector2(x2, y2));
		quad[2].WorldPosition = worldPosition;
		quad[2].TintColor = one;
		quad[3] = default(VertexGroundFeature);
		quad[3].Position = new Vector3(num3, y, z);
		quad[3].TextureCoordinate = (flipSprite ? new Vector2(x2, y2) : new Vector2(num5, y2));
		quad[3].WorldPosition = worldPosition;
		quad[3].TintColor = one;
	}
}
