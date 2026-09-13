using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide;

public struct VertexFeatureQuad : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector2 NormalTextureCoordinate;

	public float Random;

	public float Bendyness;

	public Vector3 WorldPosition;

	public float FlipNormals;

	public float WidthHeightRatio;

	public Vector4 Tint;

	public static int SizeInBytes = 72;

	public static VertexElement[] VertexElements = new VertexElement[9]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
		new VertexElement(48, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4),
		new VertexElement(52, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5),
		new VertexElement(56, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
