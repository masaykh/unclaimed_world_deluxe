using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoundLineCode;

internal struct RoundLineVertex : IVertexType
{
	public Vector3 pos;

	public Vector2 rhoTheta;

	public Vector2 scaleTrans;

	public int index;

	public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.Normal, 0), new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), new VertexElement(28, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 1));

	VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

	public RoundLineVertex(Vector3 pos, Vector2 norm, Vector2 tex, int index)
	{
		this.pos = pos;
		rhoTheta = norm;
		scaleTrans = tex;
		this.index = index;
	}
}
