using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Trees;

namespace UWGame.Client.MapRender;

internal class MapResourceRenderer
{
	private struct TileIcons
	{
		public List<IconToRender> iconsToRender;

		public Color oreColor;

		public Color hookColor;

		public Color bugColor;

		public TileIcons(bool doInit)
		{
			if (doInit)
			{
				iconsToRender = new List<IconToRender>();
			}
			else
			{
				iconsToRender = null;
			}
			oreColor = Color.White;
			hookColor = Color.White;
			bugColor = Color.White;
		}

		public void SetIconToRender(IconToRender aIcon, Color aColor)
		{
			for (int i = 0; i < iconsToRender.Count; i++)
			{
				if (aIcon == iconsToRender[i])
				{
					return;
				}
			}
			switch (aIcon)
			{
			case IconToRender.Bug:
				bugColor = aColor;
				break;
			case IconToRender.Hook:
				hookColor = aColor;
				break;
			case IconToRender.Ore:
				oreColor = aColor;
				break;
			}
			if (aIcon != IconToRender.None)
			{
				iconsToRender.Add(aIcon);
			}
		}

		public void SortIcons()
		{
			if (iconsToRender.Count <= 1)
			{
				return;
			}
			if (iconsToRender.Count == 3)
			{
				if (iconsToRender[0] > iconsToRender[1])
				{
					IconToRender value = iconsToRender[0];
					iconsToRender[0] = iconsToRender[1];
					iconsToRender[1] = value;
				}
				if (iconsToRender[1] > iconsToRender[2])
				{
					IconToRender value = iconsToRender[1];
					iconsToRender[1] = iconsToRender[2];
					iconsToRender[2] = value;
				}
				if (iconsToRender[0] > iconsToRender[1])
				{
					IconToRender value = iconsToRender[0];
					iconsToRender[0] = iconsToRender[1];
					iconsToRender[1] = value;
				}
			}
			else if (iconsToRender[0] > iconsToRender[1])
			{
				IconToRender value = iconsToRender[0];
				iconsToRender[0] = iconsToRender[1];
				iconsToRender[1] = value;
			}
		}
	}

	private Rectangle iconCatchingSpriteRectangle;

	private Rectangle iconFishingSpriteRectangle;

	private Rectangle iconGatheringSpriteRectangle;

	private Rectangle iconFrameSingle;

	private Rectangle iconFrameDouble;

	private Rectangle iconFrameTriple;

	private int iconWidth;

	private int iconHalfHeight;

	private List<RenderAsGroundSprite> groundOutlineSprites = new List<RenderAsGroundSprite>();

	private VertexFeatureQuad[] outlineVertices;

	public void Init()
	{
		iconCatchingSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_catching");
		iconFishingSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_fishing");
		iconGatheringSpriteRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_gathering");
		iconFrameSingle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_single");
		iconFrameDouble = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_double");
		iconFrameTriple = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_resource_frame_triple");
		iconWidth = iconCatchingSpriteRectangle.Width;
		iconHalfHeight = iconCatchingSpriteRectangle.Height / 2;
	}

	public void PostLoadContent()
	{
		outlineVertices = new VertexFeatureQuad[60000];
	}

	public void Render(MapManager map, GameWorldRenderer renderer)
	{
		RenderBillboardOverlays(renderer, map);
		RenderTileResourceContainerOverlays(renderer);
	}

	private void RenderBillboardOverlays(GameWorldRenderer renderer, MapManager map)
	{
		The.Client.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		CollectBillboardOverlays(map, renderer);
		DrawBillboardOverlays(renderer.OverlayBillboards, renderer);
		renderer.OverlayBillboards.Clear();
	}

	private void DrawGroundOutlines()
	{
		GameWorldRenderer renderer = The.Client.Renderer;
		int num = 0;
		num = 0;
		foreach (RenderAsGroundSprite groundOutlineSprite in groundOutlineSprites)
		{
			groundOutlineSprite.CopyQuadToVertexBuffer(renderer.groundFeatureVertices, ref num, Renderable.AdditionalEffect.Outline);
		}
		if (num > 0)
		{
			renderer.DrawGroundOutlineUserVertices(num);
		}
	}

	private void CollectBillboardOverlays(MapManager map, GameWorldRenderer renderer)
	{
		bool isInGodMode = GameWorldRenderer.GetIsInGodMode();
		foreach (List<ILocatable> item in renderer.sortedObjectsToDraw)
		{
			foreach (ILocatable item2 in item)
			{
				RenderAsBillboard renderAsBillboard = item2 as RenderAsBillboard;
				bool flag = false;
				if (renderAsBillboard == null || renderAsBillboard.Parent == null || renderAsBillboard.Parent.Entity == null)
				{
					continue;
				}
				_ = renderAsBillboard.Parent.AsRenderable;
				if (renderAsBillboard.Parent.Entity.Find<Tree>(out var c) && c != null && c.Crops != null)
				{
					foreach (KeyValuePair<ResourceType, Crop> crop in c.Crops)
					{
						if (The.InGameUI.OverlaySettings.DrawResource(crop.Value, isInGodMode))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					renderer.OverlayBillboards.Add(renderAsBillboard);
				}
			}
		}
	}

	private void DrawBillboardOverlays(List<RenderAsBillboard> listOfBillBoards, GameWorldRenderer renderer, bool doubleSpeed = false)
	{
		int index = 0;
		for (int i = 0; i < listOfBillBoards.Count; i++)
		{
			listOfBillBoards[i].CopyQuadToVertexBuffer(outlineVertices, ref index, Renderable.AdditionalEffect.Outline);
		}
		if (index > 0)
		{
			DrawOutlineBillboards(index, renderer, doubleSpeed);
		}
	}

	private void DrawOutlineBillboards(int featureQuadIndex, GameWorldRenderer renderer, bool doubleSpeed = false)
	{
		Effect billboardEffect = renderer.billboardEffect;
		billboardEffect.Parameters["UseIntegerPositions"].SetValue(!The.MapUI.IsScrolling);
		The.Client.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		billboardEffect.CurrentTechnique = billboardEffect.Techniques["Outline"];
		Dimension drawArea = The.Client.Controller.DrawArea;
		Vector2 value = new Vector2(drawArea.Width, drawArea.Height);
		billboardEffect.Parameters["ViewportSize"].SetValue(value);
		billboardEffect.Parameters["WindowPosition"].SetValue(The.MapUI.MapWindowWorldPosition);
		billboardEffect.Parameters["DiffuseTexture"].SetValue(GameData.Instance.BillboardSpriteSheet.Texture);
		foreach (EffectPass pass in billboardEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			The.Client.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, outlineVertices, 0, featureQuadIndex * 4, renderer.featureIndices, 0, featureQuadIndex * 2);
		}
	}

	private void RenderTileResourceContainerOverlays(GameWorldRenderer renderer)
	{
		groundOutlineSprites.Clear();
		bool isInGodMode = GameWorldRenderer.GetIsInGodMode();
		_ = The.InGameUI.UIAllegiance.SharedKnowledge;
		The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		for (int i = renderer.TileStartX; i <= renderer.TileEndX; i++)
		{
			TerrainTile[] array = The.Map.TileMap[i];
			for (int j = renderer.TileStartY; j <= renderer.TileEndY; j++)
			{
				TerrainTile terrainTile = array[j];
				if (terrainTile.TileResources == null && terrainTile.TreesOnTile == null)
				{
					continue;
				}
				TileIcons tileStruct = new TileIcons(doInit: true);
				Vector2 renderPosition = new Vector2(0f, 0f);
				foreach (KeyValuePair<ResourceType, TileResourceContainer> tileResource in terrainTile.TileResources)
				{
					foreach (IResourceItem resourceItem in tileResource.Value.ResourceItems)
					{
						resourceItem.Container.ResourceType.Name.Equals("Clamwich");
						bool value = false;
						if (The.InGameUI.OverlaySettings.ResourceTypesToDisplay.TryGetValue(resourceItem.Container.ResourceType, out value) && value)
						{
							TileResourceContainer value2 = tileResource.Value;
							CollectGroundOutline(value2, isInGodMode);
							CollectIconToDraw(value2, isInGodMode, ref tileStruct, ref renderPosition);
						}
					}
				}
				List<Zone> listOfZones = terrainTile.GetListOfZones(The.InGameUI.UIAllegiance);
				bool isInZone = listOfZones != null && listOfZones.Count > 0;
				DrawTileIcons(tileStruct, renderPosition, isInZone);
			}
		}
		The.Client.spriteBatch.End();
		DrawGroundOutlines();
	}

	private bool CollectIconToDraw(TileResourceContainer tileResourceContainer, bool isInGodMode, ref TileIcons tileStruct, ref Vector2 renderPosition)
	{
		if (tileResourceContainer.Renderable == null || tileResourceContainer.Renderable.RenderAsIcon == null || !The.InGameUI.OverlaySettings.DrawResource(tileResourceContainer, isInGodMode))
		{
			return false;
		}
		renderPosition = tileResourceContainer.AccessPoint.ToVector2();
		Color combinedEffectsAsColor = tileResourceContainer.Renderable.GetCombinedEffectsAsColor();
		tileStruct.SetIconToRender(tileResourceContainer.Renderable.RenderAsIcon.GetIconToRender(), combinedEffectsAsColor);
		tileResourceContainer.Renderable.IsOnScreen = true;
		return true;
	}

	private void CollectGroundOutline(TileResourceContainer tileResourceContainer, bool isInGodMode)
	{
		if (tileResourceContainer.Renderable.RenderAsGroundSprite != null && The.InGameUI.OverlaySettings.DrawResource(tileResourceContainer, isInGodMode))
		{
			groundOutlineSprites.Add(tileResourceContainer.Renderable.RenderAsGroundSprite);
		}
	}

	private void DrawTileIcons(TileIcons tileIcons, Vector2 renderPosition, bool isInZone)
	{
		Color white = Color.White;
		Rectangle destinationRectangle = default(Rectangle);
		Point point = The.MapUI.WorldPosToScreenPoint(renderPosition);
		tileIcons.SortIcons();
		for (int i = 0; i < tileIcons.iconsToRender.Count; i++)
		{
			Rectangle value;
			switch (tileIcons.iconsToRender[i])
			{
			default:
				return;
			case IconToRender.Bug:
				value = iconCatchingSpriteRectangle;
				white = tileIcons.bugColor;
				break;
			case IconToRender.Hook:
				value = iconFishingSpriteRectangle;
				white = tileIcons.hookColor;
				break;
			case IconToRender.Ore:
				value = iconGatheringSpriteRectangle;
				white = tileIcons.oreColor;
				break;
			}
			destinationRectangle.Height = value.Height;
			destinationRectangle.Width = value.Width;
			int num = 0;
			switch (i)
			{
			case 0:
				num = 0;
				break;
			case 1:
				num = iconWidth;
				break;
			case 2:
				num = -iconWidth;
				break;
			}
			destinationRectangle.X = point.X - num - iconWidth / 2;
			destinationRectangle.Y = point.Y - iconHalfHeight;
			if (tileIcons.iconsToRender.Count == 2)
			{
				destinationRectangle.X += iconWidth / 2;
			}
			The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, destinationRectangle, value, white);
		}
		if (tileIcons.iconsToRender.Count > 0 && !isInZone)
		{
			Rectangle destinationRectangle2 = default(Rectangle);
			Rectangle value2 = default(Rectangle);
			switch (tileIcons.iconsToRender.Count)
			{
			case 1:
				value2 = iconFrameSingle;
				break;
			case 2:
				value2 = iconFrameDouble;
				break;
			case 3:
				value2 = iconFrameTriple;
				break;
			}
			destinationRectangle2.Width = value2.Width;
			destinationRectangle2.Height = value2.Height;
			destinationRectangle2.X = point.X - destinationRectangle2.Width / 2;
			destinationRectangle2.Y = point.Y - destinationRectangle2.Height / 2;
			The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, destinationRectangle2, value2, The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color.White));
		}
	}
}
