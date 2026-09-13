using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.MapGUI;

public class ThreatRender : ÏnfluenceRender
{
	private List<MapClient.OverlayLimit> threatMapLimits = new List<MapClient.OverlayLimit>();

	public ThreatRender()
	{
		quad = new OverlayGroundSpriteQuad();
		MapClient.GetThreatLimits(threatMapLimits, 12);
	}

	public void PostLoadContent()
	{
		sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("whiteRectangle");
	}

	public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
	{
		_ = GameData.Instance.GUIConstants.OverlayLowAlpha;
		_ = GameData.Instance.GUIConstants.OverlayHiAlpha;
		int num = index;
		TileLayer map = The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.ThreatMaps[The.InGameUI.UIAllegiance.RepresentativeEntityType][ThreatStance.Normal].Map;
		MapClient.OverlayLimit overlayLimit = null;
		for (int i = 0; i < map.SectorsAcrossHeight; i++)
		{
			for (int j = 0; j < map.SectorsAcrossWidth; j++)
			{
				TileSector tileSector = map.Sectors[j][i];
				if (tileSector == null || !The.MapUI.TileAreaIsOnScreen(tileSector.TileArea))
				{
					continue;
				}
				for (int k = 0; k < tileSector.TileArea.Height; k++)
				{
					for (int l = 0; l < tileSector.TileArea.Width; l++)
					{
						byte value = tileSector.GetValue((ushort)l, (ushort)k);
						if (threatMapLimits != null && threatMapLimits.Count > 0)
						{
							overlayLimit = Common.GetStairStepIndex((int)value, threatMapLimits, out var _);
						}
						Microsoft.Xna.Framework.Color color = overlayLimit?.Color ?? Microsoft.Xna.Framework.Color.Blue;
						Microsoft.Xna.Framework.Color color2 = color * Common.Clamp((float)(int)value / 50f, 0f, 1f);
						if (DrawTile(l + tileSector.TileArea.Left, k + tileSector.TileArea.Top, num, color2, overlayVertices))
						{
							num++;
							continue;
						}
						index = num;
						return;
					}
				}
			}
		}
		index = num;
	}

	private new bool DrawSubtile(int x, int y, int tempIndex, Microsoft.Xna.Framework.Color color, VertexOverlayGroundSpriteQuad[] overlayVertices)
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
