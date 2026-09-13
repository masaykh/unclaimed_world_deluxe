using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;

namespace UWGame.Client.Interface.MapGUI;

public class MapAreaRender
{
	public static float Opacity = 0.38f;

	public Color Color = Color.Red;

	public bool DrawAsOverlay;

	private OverlayGroundSpriteQuad quad;

	private static Rectangle sourceRectangle;

	private MapArea parent;

	private Rectangle? boundingRectangle;

	private bool isDirty = true;

	private bool isSelected;

	public bool IsSelected
	{
		set
		{
			isSelected = value;
			if (value)
			{
				The.InGameUI.SelectedCyclePlayer.StartAnimation();
			}
		}
	}

	public MapAreaRender(MapArea parent)
	{
		this.parent = parent;
		quad = new OverlayGroundSpriteQuad();
		if (this.parent.Zone != null)
		{
			Color = Color.Turquoise;
		}
	}

	public void PostLoadContent()
	{
		sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("mapgui_zone_base");
	}

	public void SetIsDirty()
	{
		isDirty = true;
	}

	public void SetDimensions()
	{
		boundingRectangle = parent.GetBoundingBoxInTiles();
	}

	public bool IsOnScreen()
	{
		if (boundingRectangle.HasValue)
		{
			return The.MapUI.TileAreaIsOnScreen(boundingRectangle.Value);
		}
		return false;
	}

	public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
	{
		if (isDirty)
		{
			SetDimensions();
			isDirty = false;
		}
		if (!IsOnScreen())
		{
			return;
		}
		Rectangle? destination = null;
		_ = The.Map;
		Color currentColor;
		if (isSelected)
		{
			currentColor = The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color);
		}
		else
		{
			currentColor = Color;
		}
		currentColor *= Opacity;
		int tempIndex = index;
		int xScreen;
		int yScreen;
		parent.IterateArea(delegate(TerrainTile tile)
		{
			if (The.MapUI.TileIsOnScreen(tile.X, tile.Y))
			{
				The.MapUI.TileCenterToScreen(tile.X, tile.Y, out xScreen, out yScreen);
				destination = new Rectangle(xScreen - sourceRectangle.Width / 2, yScreen - sourceRectangle.Height / 2, sourceRectangle.Width, sourceRectangle.Height);
				quad.SetupQuadVertices(destination.Value.Left, destination.Value.Top, destination.Value.Right, destination.Value.Bottom, sourceRectangle, The.Client.FlatSpriteSheet.Texture, drawScanlines: false, currentColor.ToVector4());
				if (quad.CopyQuadToVertexBuffer(overlayVertices, tempIndex))
				{
					tempIndex++;
				}
			}
		});
		index = tempIndex;
	}
}
