using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Maps;

namespace UWGame.ClientSide.Interface.MapGUI;

public class TerrainBlockingRender : ÏnfluenceRender
{
	public TerrainBlockingRender()
	{
		quad = new OverlayGroundSpriteQuad();
	}

	public void PostLoadContent()
	{
		sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("whiteRectangle");
	}

	public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
	{
		SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		byte overlayLowAlpha = GameData.Instance.GUIConstants.OverlayLowAlpha;
		byte overlayHiAlpha = GameData.Instance.GUIConstants.OverlayHiAlpha;
		byte b = overlayHiAlpha;
		int tempIndex = index;
		Point point = MapManager.TileToUpperLeftSubtile(new Point(The.Client.Renderer.TileStartX, The.Client.Renderer.TileStartY));
		Point point2 = MapManager.TileToUpperLeftSubtile(new Point(The.Client.Renderer.TileEndX, The.Client.Renderer.TileEndY));
		point2.X += 2;
		point2.Y += 2;
		Color color = Color.Black;
		byte cost;
		bool isInPad;
		for (int i = point.X; i <= point2.X; i++)
		{
			for (int j = point.Y; j <= point2.Y; j++)
			{
				MapManager.SubtileValue value = map.GetValue(i, j);
				cost = MapManager.GetCost(value);
				bool drawSubtile = false;
				GetFlagsAndColor(b, value, cost, ref color, ref drawSubtile, out isInPad, out var _);
				if (drawSubtile)
				{
					if (!DrawSubtile(i, j, tempIndex, color, overlayVertices))
					{
						index = tempIndex;
						return;
					}
					tempIndex++;
				}
				b = ((b == overlayHiAlpha) ? overlayLowAlpha : overlayHiAlpha);
			}
			b = ((b == overlayHiAlpha) ? overlayLowAlpha : overlayHiAlpha);
		}
		if (The.InGameUI.EntitiesBeingPlaced != null)
		{
			foreach (InGameInterface.EntityPosition item in The.InGameUI.EntitiesBeingPlaced)
			{
				if (item.Entity.GeometryLayout == null)
				{
					continue;
				}
				bool placementIsValid = true;
				if (item.Entity.Structure != null)
				{
					placementIsValid = item.Entity.Structure.IsPlacementValid(item.Position.ToVector3());
				}
				GeometryLayoutType geoType = item.Entity.CurrentSimState.GeometryLayoutType;
				float padRadiusSquared = item.Entity.GeometryLayout.GetPadRadiusSquared();
				item.Entity.GeometryLayout.IterateSubtiles(delegate(Vector2 worldPos)
				{
					if (The.Map.WorldLocationIsOnMap(worldPos))
					{
						bool flag = false;
						Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
						MapManager.SubtileValue value2 = map.GetValue(subtilePos);
						cost = MapManager.GetCost(value2);
						isInPad = item.Entity.GeometryLayout.IsWithinPad(geoType, padRadiusSquared, worldPos);
						bool isWithinShapes = item.Entity.GeometryLayout.IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds: false);
						if (!Structure.CanBuildOnSubtile(isWithinShapes, isInPad, value2))
						{
							flag = true;
							color = GameData.Instance.GUIConstants.StructurePreventingPlacementColor;
						}
						else if (isWithinShapes)
						{
							flag = true;
							if (placementIsValid)
							{
								color = GameData.Instance.GUIConstants.StructureBeingPlacedColor;
							}
							else
							{
								color = GameData.Instance.GUIConstants.StructureBeingPlacedOtherPointPreventingPlacementColor;
							}
						}
						else if (isInPad)
						{
							flag = true;
							if (placementIsValid)
							{
								color = GameData.Instance.GUIConstants.PadColor;
							}
							else
							{
								color = GameData.Instance.GUIConstants.StructureBeingPlacedOtherPointPreventingPlacementPadColor;
							}
						}
						if (flag && DrawSubtile(subtilePos.X, subtilePos.Y, tempIndex, color, overlayVertices))
						{
							int num = tempIndex;
							tempIndex = num + 1;
						}
					}
				});
			}
		}
		index = tempIndex;
	}

	private static void GetFlagsAndColor(byte alpha, MapManager.SubtileValue value, byte cost, ref Color color, ref bool drawSubtile, out bool isInPad, out bool isReserved)
	{
		isInPad = false;
		isReserved = false;
		switch (cost)
		{
		case 0:
			color = GameData.Instance.GUIConstants.BlockedColor;
			color.A = alpha;
			drawSubtile = true;
			break;
		case 3:
			isInPad = MapManager.TestForFlag(value, MapManager.SubtileValue.Pad);
			isReserved = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);
			if (isInPad & isReserved)
			{
				color = Color.LightYellow;
				color.A = alpha;
				drawSubtile = true;
			}
			else if (isInPad)
			{
				color = GameData.Instance.GUIConstants.PadColor;
				color.A = alpha;
				drawSubtile = true;
			}
			else if (isReserved)
			{
				color = Color.White;
				color.A = alpha;
				drawSubtile = true;
			}
			break;
		}
	}
}
