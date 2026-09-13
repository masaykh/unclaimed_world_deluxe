using System.Drawing;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Interface.MapGUI;

public abstract class ÏnfluenceRender
{
	protected OverlayGroundSpriteQuad quad;

	protected Microsoft.Xna.Framework.Rectangle sourceRectangle;

	protected bool DrawTile(int x, int y, int tempIndex, Microsoft.Xna.Framework.Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
	{
		Vector2 vector = The.MapUI.TileEdgeToScreen(x, y);
		RectangleF rectangleF = new RectangleF(vector.X, vector.Y, 48f, 48f);
		quad.SetupQuadVertices(rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom, sourceRectangle, The.Client.FlatSpriteSheet.Texture, drawScanlines: true, color.ToVector4());
		if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
		{
			return true;
		}
		return false;
	}

	protected bool DrawSubtile(int x, int y, int tempIndex, Microsoft.Xna.Framework.Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
	{
		Vector2 vector = The.MapUI.SubtileEdgeToScreen(x, y);
		RectangleF rectangleF = new RectangleF(vector.X, vector.Y, 16f, 16f);
		quad.SetupQuadVertices(rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom, sourceRectangle, The.Client.FlatSpriteSheet.Texture, drawScanlines: true, color.ToVector4());
		if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
		{
			return true;
		}
		return false;
	}
}
