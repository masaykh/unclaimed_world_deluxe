using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide;

public class QuadRendererComponent
{
	private VertexPositionTexture[] verts;

	private short[] ib;

	public void Render(GraphicsDevice device, Vector2 v1, Vector2 v2)
	{
		verts[0].Position.X = v2.X;
		verts[0].Position.Y = v1.Y;
		verts[1].Position.X = v1.X;
		verts[1].Position.Y = v1.Y;
		verts[2].Position.X = v1.X;
		verts[2].Position.Y = v2.Y;
		verts[3].Position.X = v2.X;
		verts[3].Position.Y = v2.Y;
		device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
	}

	public void LoadContent()
	{
		verts = new VertexPositionTexture[4]
		{
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 1f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f)),
			new VertexPositionTexture(new Vector3(0f, 0f, 0f), new Vector2(1f, 0f))
		};
		ib = new short[6] { 0, 1, 2, 2, 3, 0 };
	}
}
