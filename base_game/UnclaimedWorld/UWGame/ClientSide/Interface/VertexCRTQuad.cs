using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Interface;

public struct VertexCRTQuad : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector2 TruncatedEffectCoordinate;

	public Vector2 ScaledEffectCoordinate;

	public float IsMonochrome;

	public float Alpha;

	public static int SizeInBytes = 44;

	public static VertexElement[] VertexElements = new VertexElement[6]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(40, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
