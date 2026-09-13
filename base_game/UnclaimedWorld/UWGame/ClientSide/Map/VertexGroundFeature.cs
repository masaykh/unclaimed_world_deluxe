using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Map;

public struct VertexGroundFeature : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector3 WorldPosition;

	public Vector4 TintColor;

	public static int SizeInBytes = 48;

	public static VertexElement[] VertexElements = new VertexElement[4]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
		new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
