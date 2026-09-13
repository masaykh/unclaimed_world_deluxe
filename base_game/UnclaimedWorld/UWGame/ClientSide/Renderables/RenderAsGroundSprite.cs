using Microsoft.Xna.Framework;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Renderables;

public class RenderAsGroundSprite : RenderAsBase, IDrawnAsGroundSprite
{
	public Rectangle? SourceRect;

	private bool spriteOrLocationIsDirty = true;

	private bool propertiesAreDirty = true;

	private bool overlayPropertiesAreDirty = true;

	public RoadAndPathQuad quad = new RoadAndPathQuad();

	public RenderAsGroundSprite(Entity parent, RenderAsGroundSpriteType type, Renderable renderable)
		: base(parent, renderable)
	{
		if (type.AssetName != null)
		{
			SourceRect = The.Client.FlatSpriteSheet.GetSourceRectangle(type.AssetName);
		}
	}

	public void Redraw(Rectangle? spriteRect)
	{
		SourceRect = spriteRect;
		spriteOrLocationIsDirty = true;
	}

	public void SetIsDirty()
	{
		spriteOrLocationIsDirty = true;
	}

	public void SetPropertiesAreDirty()
	{
		propertiesAreDirty = true;
	}

	public void SetOverlayPropertiesAreDirty()
	{
		overlayPropertiesAreDirty = true;
	}

	public void CopyQuadToVertexBuffer(VertexGroundFeature[] featureVertices, ref int index, Renderable.AdditionalEffect? overridingEffectID = null)
	{
		if (SourceRect.HasValue)
		{
			if (spriteOrLocationIsDirty)
			{
				SetupQuadVertices();
			}
			if (propertiesAreDirty)
			{
				SetQuadProperties();
			}
			if (overridingEffectID.HasValue)
			{
				Vector4 combinedEffects = Renderable.GetCombinedEffects(overridingEffectID.Value);
				quad.SetProperties(combinedEffects);
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
	}

	public void SetupQuadVertices()
	{
		Vector2 baseCenterOffset = new Vector2((float)SourceRect.Value.Width / 2f, (float)SourceRect.Value.Height / 2f);
		quad.SetupQuadVertices(Renderable.Location.Value, baseCenterOffset, SourceRect.Value, The.Client.FlatSpriteSheet.Texture, Renderable.FlipHorizontally);
		SetQuadProperties();
		spriteOrLocationIsDirty = false;
	}

	private void SetQuadProperties()
	{
		Vector4 combinedEffects = Renderable.GetCombinedEffects();
		quad.SetProperties(combinedEffects);
		propertiesAreDirty = false;
	}
}
