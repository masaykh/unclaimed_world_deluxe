using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide;

public struct VertexOverlayQuad : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector2 TruncatedEffectCoordinate;

	public Vector3 WorldPosition;

	public float DrawScanlines;

	public Vector4 GradientColor1;

	public Vector4 GradientColor2;

	public Vector4 GradientColor3;

	public static int SizeInBytes = 92;

	public static VertexElement[] VertexElements = new VertexElement[8]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
		new VertexElement(40, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(44, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
		new VertexElement(76, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
