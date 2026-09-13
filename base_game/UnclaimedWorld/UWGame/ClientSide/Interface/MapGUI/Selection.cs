using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.MapGUI;

public class Selection
{
	private enum SelectionSprite
	{
		Small,
		Medium
	}

	private Animation2DPlayer selectedCirclePlayer;

	private Animation2DPlayer hoverCirclePlayer;

	private Animation2D mediumSelection;

	private Animation2D mediumSelectionLoop;

	private Animation2D smallSelection;

	private Animation2D smallSelectionLoop;

	private InGameInterface intf = The.InGameUI;

	private Regulator selectionUpdateRegulator = new Regulator(The.Client.ClientRandomGenerator, 4.0, "SelectionSelection");

	private Regulator hoverUpdateRegulator = new Regulator(The.Client.ClientRandomGenerator, 4.0, "SelectionHover");

	private Color selectionTintingColor = Color.White;

	private Color selectionCurrentColor;

	public Color HoverTintingColor = Color.White;

	private Color hoverCurrentColor;

	private Rectangle selectionSourceRect;

	private Rectangle hoverSourceRect;

	private SelectionSprite selectionSprite;

	private SelectionSprite hoverSprite;

	private OverlayGroundSpriteQuad quad;

	private Color hoverCircleFullTintColor = new Color(81, 134, 183);

	private Color hoverCircleEmptyTintColor = new Color(137, 168, 196);

	public Selection()
	{
		quad = new OverlayGroundSpriteQuad();
	}

	public void PostLoadContent()
	{
		mediumSelection = new Animation2D(The.Client.FlatSpriteSheet.Texture, 0.08f, isLooping: false);
		mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame2"))
		{
			Color = Animation2D.transp
		});
		mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame1"))
		{
			Color = Animation2D.halfTransp
		});
		mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame3")));
		mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame2")));
		mediumSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));
		mediumSelectionLoop = new Animation2D(The.Client.FlatSpriteSheet.Texture, 1f, isLooping: true);
		mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));
		mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4"))
		{
			Color = Animation2D.thirdTransp
		});
		mediumSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame4")));
		smallSelection = new Animation2D(The.Client.FlatSpriteSheet.Texture, 0.08f, isLooping: false);
		smallSelection.Cells.Add(new Cell
		{
			Frame = The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1"),
			Color = Animation2D.transp
		});
		smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame2"))
		{
			Color = Animation2D.halfTransp
		});
		smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame3")));
		smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1")));
		smallSelection.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));
		smallSelectionLoop = new Animation2D(The.Client.FlatSpriteSheet.Texture, 1f, isLooping: true);
		smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));
		smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4"))
		{
			Color = Animation2D.thirdTransp
		});
		smallSelectionLoop.Cells.Add(new Cell(The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame4")));
		selectedCirclePlayer = new Animation2DPlayer();
		hoverCirclePlayer = new Animation2DPlayer();
	}

	public void SelectEntity(IKnownEntityData selectedEntity)
	{
		SetupSelectionDimensionsAndColor(selectedEntity, out selectionSourceRect, out selectionSprite, isHover: false);
		if (selectionSprite == SelectionSprite.Small)
		{
			selectedCirclePlayer.StartAnimation(smallSelection);
			selectedCirclePlayer.AnimationEndedEvent += player_smallSelectionAnimationEndedEvent;
		}
		else if (selectionSprite == SelectionSprite.Medium)
		{
			selectedCirclePlayer.StartAnimation(mediumSelection);
			selectedCirclePlayer.AnimationEndedEvent += player_mediumSelectionAnimationEndedEvent;
		}
	}

	public void StartHoverOverEntity(Color? hoverTintingColor = null)
	{
		if (!intf.HoverEntity.HasValue)
		{
			return;
		}
		IKnownEntityData data = null;
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out data);
		if (data != null)
		{
			SetupSelectionDimensionsAndColor(data, out hoverSourceRect, out hoverSprite, isHover: true, hoverTintingColor);
			if (hoverSprite == SelectionSprite.Small)
			{
				hoverCirclePlayer.StartAnimation(smallSelectionLoop);
			}
			else if (hoverSprite == SelectionSprite.Medium)
			{
				hoverCirclePlayer.StartAnimation(mediumSelectionLoop);
			}
		}
	}

	public void StartHoverOverTile(Color? hoverTintingColor = null)
	{
		SetupSelectionDimensionsAndColor(null, out hoverSourceRect, out hoverSprite, isHover: true, hoverTintingColor);
		hoverCirclePlayer.StartAnimation(smallSelectionLoop);
	}

	private void player_smallSelectionAnimationEndedEvent()
	{
		selectedCirclePlayer.StartAnimation(smallSelectionLoop);
		selectedCirclePlayer.AnimationEndedEvent -= player_smallSelectionAnimationEndedEvent;
	}

	private void player_mediumSelectionAnimationEndedEvent()
	{
		selectedCirclePlayer.StartAnimation(mediumSelectionLoop);
		selectedCirclePlayer.AnimationEndedEvent -= player_mediumSelectionAnimationEndedEvent;
	}

	public void Update(GameTime gameTime)
	{
		if (intf.SelectedEntity.HasValue && selectionUpdateRegulator.IsReady())
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.SelectedEntity.Value, out var data);
			if (data != null)
			{
				SetupSelectionDimensionsAndColor(data, out selectionSourceRect, out selectionSprite, isHover: false);
			}
		}
		if (intf.HoverEntity.HasValue && hoverUpdateRegulator.IsReady())
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out var data2);
			if (data2 != null)
			{
				SetupSelectionDimensionsAndColor(data2, out hoverSourceRect, out hoverSprite, isHover: true);
			}
		}
		selectedCirclePlayer.Update(gameTime);
		hoverCirclePlayer.Update(gameTime);
	}

	private void SetupSelectionDimensionsAndColor(IKnownEntityData entityData, out Rectangle sourceRect, out SelectionSprite sprite, bool isHover, Color? hoverTintingColor = null)
	{
		float num = 24f;
		if (entityData != null)
		{
			num = Entity.GetSelectionRadius(entityData);
		}
		bool flag = false;
		if (entityData is Entity)
		{
			List<IKnownEntityData> agentsInside = null;
			flag = Entity.ContainsIntelligence(entityData, The.InGameUI.UIAllegiance.SharedKnowledge, ref agentsInside);
		}
		Color hoverTintingColor2 = ((!isHover) ? (flag ? Color.Gold : Color.White) : (hoverTintingColor.HasValue ? hoverTintingColor.Value : ((entityData == null) ? hoverCircleEmptyTintColor : (flag ? hoverCircleFullTintColor : hoverCircleEmptyTintColor))));
		if (isHover)
		{
			HoverTintingColor = hoverTintingColor2;
		}
		else
		{
			selectionTintingColor = hoverTintingColor2;
		}
		if (num < 48f)
		{
			sourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle("smallcircle_frame1");
			sprite = SelectionSprite.Small;
		}
		else
		{
			sourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle("mediumcircle_frame1");
			sprite = SelectionSprite.Medium;
		}
	}

	public void SetupQuad(VertexOverlayGroundSpriteQuad[] overlayVertices, ref int index)
	{
		Rectangle? rectangle = null;
		if (selectedCirclePlayer.Animation != null && intf.SelectedEntity.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.SelectedEntity.Value, out var data);
			if (!InGameInterface.CanSelectEntity(data))
			{
				The.InGameUI.SelectEntity(null);
				return;
			}
			Vector2 vector = The.MapUI.WorldPosToScreen(data.PlaySiteLocation);
			if (data is Entity entity)
			{
				Entity rootAndContainer = entity.GetRootAndContainer();
				if (rootAndContainer.Renderable != null && rootAndContainer.Renderable.RenderAsModel != null)
				{
					vector = The.MapUI.WorldPosToScreen(rootAndContainer.Renderable.RenderAsModel.Location);
				}
			}
			rectangle = new Rectangle((int)(vector.X - (float)(selectionSourceRect.Width / 2)), (int)(vector.Y - (float)(selectionSourceRect.Height / 2)), selectionSourceRect.Width, selectionSourceRect.Height);
			selectionCurrentColor = selectedCirclePlayer.GetCurrentColor(selectionTintingColor);
			quad.SetupQuadVertices(rectangle.Value.Left, rectangle.Value.Top, rectangle.Value.Right, rectangle.Value.Bottom, selectedCirclePlayer.GetFrame(), The.Client.FlatSpriteSheet.Texture, drawScanlines: true, selectionCurrentColor.ToVector4());
			quad.CopyQuadToVertexBuffer(overlayVertices, index);
			index++;
		}
		if (hoverCirclePlayer.Animation == null)
		{
			return;
		}
		Vector2? vector2 = null;
		if (intf.HoverEntity.HasValue && intf.HoverEntity != intf.SelectedEntity)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intf.HoverEntity.Value, out var data2);
			if (data2 == null || !data2.Location.HasValue)
			{
				intf.SetHoverEntity(null);
				return;
			}
			if (data2 != null)
			{
				vector2 = The.MapUI.WorldPosToScreen(data2.PlaySiteLocation);
				if (data2 is Entity { Renderable: not null } entity2 && entity2.Renderable.RenderAsModel != null)
				{
					vector2 = The.MapUI.WorldPosToScreen(entity2.Renderable.RenderAsModel.Location);
				}
			}
		}
		else if (intf.HoverTile != null)
		{
			vector2 = The.MapUI.TilePosToScreen(new Point(intf.HoverTile.X, intf.HoverTile.Y));
		}
		if (vector2.HasValue)
		{
			rectangle = new Rectangle((int)(vector2.Value.X - (float)hoverSourceRect.Width * 0.5f), (int)(vector2.Value.Y - (float)hoverSourceRect.Height * 0.5f), hoverSourceRect.Width, hoverSourceRect.Height);
			hoverCurrentColor = hoverCirclePlayer.GetCurrentColor(HoverTintingColor);
			quad.SetupQuadVertices(rectangle.Value.Left, rectangle.Value.Top, rectangle.Value.Right, rectangle.Value.Bottom, hoverCirclePlayer.GetFrame(), The.Client.FlatSpriteSheet.Texture, drawScanlines: true, hoverCurrentColor.ToVector4());
			quad.CopyQuadToVertexBuffer(overlayVertices, index);
			index++;
		}
	}
}
