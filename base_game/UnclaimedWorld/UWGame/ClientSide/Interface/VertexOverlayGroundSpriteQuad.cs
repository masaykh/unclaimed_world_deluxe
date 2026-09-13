using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Interface;

public struct VertexOverlayGroundSpriteQuad : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector2 TruncatedEffectCoordinate;

	public float DrawScanlines;

	public Vector4 Color;

	public static int SizeInBytes = 48;

	public static VertexElement[] VertexElements = new VertexElement[5]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
