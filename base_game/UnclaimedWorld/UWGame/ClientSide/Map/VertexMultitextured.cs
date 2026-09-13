using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Map;

public struct VertexMultitextured : IVertexType
{
	public Vector3 Position;

	public Vector2 TextureCoordinate;

	public Vector4 TexWeights;

	public Vector4 TintColor0;

	public Vector4 TintColor1;

	public Vector4 TintColor2;

	public Vector4 AlphaSharpness;

	public Vector4 NoiseScaling;

	public static int SizeInBytes = 116;

	public static VertexElement[] VertexElements = new VertexElement[8]
	{
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
		new VertexElement(52, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
		new VertexElement(68, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
		new VertexElement(84, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
		new VertexElement(100, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6)
	};

	private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);

	public VertexDeclaration VertexDeclaration => vertexDeclaration;
}
