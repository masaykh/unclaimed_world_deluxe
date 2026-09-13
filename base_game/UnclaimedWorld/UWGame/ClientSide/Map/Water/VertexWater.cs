using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Map.Water;

public struct VertexWater : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector2 BumpTextureCoordinate;

	public float WaterDepth;

	public Vector4 WaterColor;

	public Vector4 BottomTint;

	public static int SizeInBytes = 64;

	public static VertexElement[] VertexElements = new VertexElement[6]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
