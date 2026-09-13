using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Trees;
using WindowSystem;

namespace UWGame.ClientSide.Renderables;

public class RenderAsBillboard : ILocatable, IComparable, IUpdatable
{
	public Point? MapPosition;

	protected FeatureQuad quad = new FeatureQuad();

	protected FeatureQuad shadowQuad = new FeatureQuad();

	protected OverlayQuad overlayQuad = new OverlayQuad();

	private bool spriteIsDirty = true;

	private bool overlaySpriteIsDirty = true;

	private bool propertiesAreDirty = true;

	private bool overlayPropertiesAreDirty = true;

	public RenderAsBillboardType renderAsBillboardType;

	public Renderable Parent;

	public Renderable AsRenderable => null;

	public RenderAsBillboard AsRenderAsBillboard => this;

	public Vector3 Location { get; set; }

	public RenderAsBillboard(Renderable parent, RenderAsBillboardType renderAsBillboardType)
	{
		Parent = parent;
		this.renderAsBillboardType = renderAsBillboardType;
	}

	public void SetIsDirty()
	{
		spriteIsDirty = true;
		overlaySpriteIsDirty = true;
	}

	public void SetPropertiesAreDirty()
	{
		propertiesAreDirty = true;
	}

	public void SetOverlayPropertiesAreDirty()
	{
		overlayPropertiesAreDirty = true;
	}

	public void Redraw(SpriteSheet spritesheet, RenderAsBillboardType billboardType, bool drawAsOverlay)
	{
		renderAsBillboardType = billboardType;
		if (spritesheet != null && !string.IsNullOrEmpty(renderAsBillboardType.AssetName))
		{
			Texture2D texture = spritesheet.Texture;
			Rectangle sourceRectangle = spritesheet.GetSourceRectangle(renderAsBillboardType.AssetName);
			_ = renderAsBillboardType.AssetName == "clayGranary_construct";
			quad.SetStaticFrame(sourceRectangle, texture);
			shadowQuad.SetStaticFrame(sourceRectangle, texture);
			if (drawAsOverlay)
			{
				RedrawOverlay(sourceRectangle);
				overlayPropertiesAreDirty = true;
			}
		}
		else if (!string.IsNullOrEmpty(renderAsBillboardType.AnimationAssetName))
		{
			Animation2D animationFrames = GameData.Instance.Animation2Ds[renderAsBillboardType.AnimationAssetName];
			quad.SetAnimationFrames(animationFrames);
			shadowQuad.SetAnimationFrames(animationFrames);
			quad.Player.FrameChangedEvent += Player_FrameChangedEvent;
		}
		else
		{
			Texture2D texture2 = spritesheet.Texture;
			quad.SetStaticFrame(null, texture2);
			shadowQuad.SetStaticFrame(null, texture2);
			if (drawAsOverlay)
			{
				RedrawOverlay(null);
			}
		}
		spriteIsDirty = true;
		overlaySpriteIsDirty = true;
	}

	public void RedrawOverlay(Rectangle? spriteRect)
	{
		Texture2D texture = The.Client.Renderer.GhostedStructuresSpriteSheet.Texture;
		if (overlayQuad == null)
		{
			overlayQuad = new OverlayQuad();
		}
		overlayQuad.SetStaticFrame(spriteRect, texture);
		spriteIsDirty = true;
		overlaySpriteIsDirty = true;
	}

	public void SetOverlayGradientColors(Color gradient1, Color gradient2, Color gradient3)
	{
		overlayQuad.GradientColor1 = gradient1.ToVector4();
		overlayQuad.GradientColor2 = gradient2.ToVector4();
		overlayQuad.GradientColor3 = gradient3.ToVector4();
		overlaySpriteIsDirty = true;
	}

	public void CopyShadowQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, ref int index)
	{
		if (spriteIsDirty)
		{
			SetupQuadVertices();
		}
		if (shadowQuad.CopyQuadToVertexBuffer(featureVertices, index))
		{
			index++;
		}
	}

	public void CopyQuadToVertexBuffer(VertexFeatureQuad[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null)
	{
		if (spriteIsDirty)
		{
			SetupQuadVertices();
		}
		else if (propertiesAreDirty)
		{
			SetQuadProperties();
		}
		if (overridingEffectID.HasValue)
		{
			Vector4 combinedEffects = Parent.GetCombinedEffects(overridingEffectID.Value);
			quad.SetTint(combinedEffects);
		}
		if (quad.CopyQuadToVertexBuffer(featureVertices, index))
		{
			index++;
		}
		if (overridingEffectID.HasValue)
		{
			SetQuadProperties();
		}
	}

	public void CopyOverlayQuadToVertexBuffer(VertexOverlayQuad[] overlayVertices, ref int index)
	{
		if (overlaySpriteIsDirty)
		{
			SetupOverlayQuadVertices();
		}
		else if (overlayPropertiesAreDirty)
		{
			SetOverlayQuadProperties();
		}
		overlayQuad.CopyQuadToVertexBuffer(overlayVertices, index);
		index++;
	}

	private void SetupOverlayQuadVertices()
	{
		SetupDimensions(out var _, out var posLeft, out var posRight, out var posTop, out var posBottom, out var baseCenterWorld);
		overlayQuad.SetupQuadVertices(baseCenterWorld, posLeft, posTop, posRight, posBottom, drawScanlines: true, Parent.FlipHorizontally);
		overlaySpriteIsDirty = false;
	}

	private void Player_FrameChangedEvent()
	{
		spriteIsDirty = true;
		overlaySpriteIsDirty = true;
	}

	public void ComputeMapPosition()
	{
		SetupDimensions(out var _, out var _, out var _, out var _, out var _, out var _);
	}

	private void SetupQuadVertices()
	{
		SetupDimensions(out var baseCenterOffset, out var posLeft, out var posRight, out var posTop, out var posBottom, out var baseCenterWorld);
		quad.SetupQuadVertices(baseCenterWorld, posLeft, posTop, posRight, posBottom, Parent.FlipHorizontally);
		posTop = 0f - baseCenterOffset.Y;
		posBottom = 0f;
		shadowQuad.WidthHeightRatio = renderAsBillboardType.AspectRatio;
		shadowQuad.SetupQuadVerticesFromBaseCenter(baseCenterWorld, posLeft, posTop, posRight, posBottom, baseCenterOffset, Parent.FlipHorizontally);
		SetQuadProperties();
		spriteIsDirty = false;
	}

	private void SetQuadProperties()
	{
		Vector4 combinedEffects = Parent.GetCombinedEffects();
		quad.SetProperties(renderAsBillboardType.Bendyness, Parent.RandomConstant, combinedEffects);
		shadowQuad.SetProperties(renderAsBillboardType.Bendyness, Parent.RandomConstant, combinedEffects);
		propertiesAreDirty = false;
	}

	private void SetOverlayQuadProperties()
	{
		Vector4 combinedOverlayEffects = Parent.GetCombinedOverlayEffects();
		Parent.GetOverlayGradientColors(out var gradient, out var gradient2, out var gradient3);
		overlayQuad.SetProperties(combinedOverlayEffects, gradient, gradient2, gradient3);
		overlayPropertiesAreDirty = false;
	}

	private void SetupDimensions(out Vector2 baseCenterOffset, out float posLeft, out float posRight, out float posTop, out float posBottom, out Vector3 baseCenterWorld)
	{
		float width = quad.StaticSourceRectangle.Width;
		float height = quad.StaticSourceRectangle.Height;
		Entity entity = null;
		if (Parent != null && Parent.Parent != null)
		{
			entity = Parent.Parent as Entity;
		}
		if (entity != null && entity.Find<Tree>(out var c))
		{
			c.GetSizeScaling(ref width, ref height);
			baseCenterOffset = new Vector2(width / 2f, height - 14f);
		}
		else
		{
			if (renderAsBillboardType.BaseCenter == Vector2.Zero)
			{
				baseCenterOffset = new Vector2(width / 2f, height / 2f);
				baseCenterOffset.X = (float)Math.Floor(baseCenterOffset.X);
				baseCenterOffset.Y = (float)Math.Floor(baseCenterOffset.Y);
			}
			else
			{
				baseCenterOffset = renderAsBillboardType.BaseCenter;
			}
			baseCenterOffset.X = Common.FlipOffset(baseCenterOffset.X, width, Parent.FlipHorizontally);
		}
		posLeft = 0f - baseCenterOffset.X;
		posRight = posLeft + width;
		posTop = 0f - baseCenterOffset.Y;
		posBottom = posTop + height;
		baseCenterWorld = Parent.Location.Value;
		Vector2 offset = renderAsBillboardType.Offset;
		if (Parent.FlipHorizontally && offset != Vector2.Zero)
		{
			offset.X = 0f - offset.X;
		}
		baseCenterWorld.X += offset.X;
		baseCenterWorld.Y += offset.Y;
		Location = baseCenterWorld;
		MapPosition = MapManager.WorldPosToTile(Location);
	}

	public void Update(GameTime gameTime)
	{
		if (quad != null)
		{
			quad.Update(gameTime);
		}
		if (shadowQuad != null)
		{
			shadowQuad.Update(gameTime);
		}
		if (overlayQuad != null)
		{
			overlayQuad.Update(gameTime);
		}
	}

	public double? GetUpdateInterval()
	{
		bool flag = false;
		if (quad != null)
		{
			flag = flag || quad.RequiresUpdate;
		}
		if (overlayQuad != null)
		{
			flag = flag || overlayQuad.RequiresUpdate;
		}
		if (shadowQuad != null)
		{
			flag = flag || shadowQuad.RequiresUpdate;
		}
		if (flag)
		{
			return 0.0;
		}
		return null;
	}

	public int CompareTo(object obj)
	{
		ILocatable locatable = obj as ILocatable;
		return (int)(Location.Y - locatable.Location.Y);
	}
}
