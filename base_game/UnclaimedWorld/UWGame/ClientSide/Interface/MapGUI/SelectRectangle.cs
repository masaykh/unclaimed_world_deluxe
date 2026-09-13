using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.MapGUI;

public class SelectRectangle
{
	private List<GUIRect> guiRects = new List<GUIRect>();

	private Rectangle? source;

	private OverlayGroundSpriteQuad[] quads;

	public bool Visible;

	public SelectRectangle()
	{
		quads = new OverlayGroundSpriteQuad[9];
		for (int i = 0; i < 9; i++)
		{
			quads[i] = new OverlayGroundSpriteQuad();
		}
	}

	public void SetSize(int x, int y, int width, int height)
	{
		if (!source.HasValue)
		{
			source = The.Client.FlatSpriteSheet.GetSourceRectangle("selectionRectangle");
		}
		Box.CreateBox(guiRects, source.Value, new Rectangle(x, y, width, height), 10, Color.LightGray, Color.LightGray);
	}

	public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
	{
		if (!Visible)
		{
			return;
		}
		int num = 0;
		foreach (GUIRect guiRect in guiRects)
		{
			OverlayGroundSpriteQuad obj = quads[num];
			obj.SetupQuadVertices(guiRect.Destination.Left, guiRect.Destination.Top, guiRect.Destination.Right, guiRect.Destination.Bottom, guiRect.Source, The.Client.FlatSpriteSheet.Texture, drawScanlines: false, guiRect.Color.ToVector4());
			num++;
			obj.CopyQuadToVertexBuffer(overlayVertices, index);
			index++;
		}
	}
}
