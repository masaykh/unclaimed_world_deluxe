using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Map;

public class LightSource : ILocatable, IComparable
{
	public Renderable Parent;

	protected Vector3 location = Vector3.Zero;

	public bool LightIsOn;

	protected VertexLightSourceQuad[] quad;

	private bool spriteOrLocationIsDirty = true;

	public LightingType LightingType;

	public virtual Vector3 Location
	{
		get
		{
			return location;
		}
		set
		{
			location = The.Map.ClampWorldPosition(value);
		}
	}

	public Renderable AsRenderable => null;

	public RenderAsBillboard AsRenderAsBillboard => null;

	public LightSource(LightingType lightingType, Renderable parent)
	{
		Parent = parent;
		LightingType = lightingType;
	}

	public LightSource(Vector3 location)
	{
		Location = new Vector3(location.X, location.Y + 1f, location.Z);
	}

	public void SetIsDirty()
	{
		spriteOrLocationIsDirty = true;
	}

	public void CopyQuadToVertexBuffer(VertexLightSourceQuad[] lightSourceVertices, int index)
	{
		if (spriteOrLocationIsDirty)
		{
			SetupQuadVertices();
		}
		index *= 4;
		for (int i = index; i < index + 4; i++)
		{
			lightSourceVertices[i] = quad[i - index];
		}
	}

	public void SetupQuadVertices()
	{
		float heightOverGround = 20f;
		Rectangle sourceRectangle = GameData.Instance.LightSourcesSpriteSheet.GetSourceRectangle(LightingType.SpriteName);
		Vector2 vector = default(Vector2);
		vector.X = LightingType.Offset.X;
		vector.Y = LightingType.Offset.Y;
		Vector2 offset = vector;
		Location = new Vector3(Parent.Location.Value.X, Parent.Location.Value.Y + 1f, Parent.Location.Value.Z);
		SetupQuadVertices(Location, offset, heightOverGround, sourceRectangle, GameData.Instance.LightSourcesSpriteSheet.Texture);
	}

	public void SetupQuadVertices(Vector3 worldPosition, Vector2 offset, float heightOverGround, Rectangle textureRectangle, Texture2D texture)
	{
		float x = offset.X;
		float x2 = (float)textureRectangle.Width + offset.X;
		float y = offset.Y;
		float y2 = (float)textureRectangle.Height + offset.Y;
		Location = worldPosition;
		float z = 0f;
		quad = new VertexLightSourceQuad[4];
		float num = (float)textureRectangle.X / (float)texture.Width;
		float num2 = (float)textureRectangle.Y / (float)texture.Height;
		float x3 = num + (float)textureRectangle.Width / (float)texture.Width;
		float y3 = num2 + (float)textureRectangle.Height / (float)texture.Height;
		float x4 = Location.Z + (float)texture.Height / 2f;
		float x5 = Common.ClampBottom(Location.Z - (float)texture.Height / 2f, 0f);
		quad[0] = default(VertexLightSourceQuad);
		quad[0].Position = new Vector3(x, y, z);
		quad[0].WorldPosition = worldPosition;
		ref VertexLightSourceQuad reference = ref quad[0];
		Renderable parent = Parent;
		reference.TextureCoordinate = ((parent != null && parent.FlipHorizontally) ? new Vector2(x3, num2) : new Vector2(num, num2));
		quad[0].HeightAboveGround = new Vector2(x4, 0f);
		quad[1] = default(VertexLightSourceQuad);
		quad[1].Position = new Vector3(x2, y, z);
		quad[1].WorldPosition = worldPosition;
		ref VertexLightSourceQuad reference2 = ref quad[1];
		Renderable parent2 = Parent;
		reference2.TextureCoordinate = ((parent2 != null && parent2.FlipHorizontally) ? new Vector2(num, num2) : new Vector2(x3, num2));
		quad[1].HeightAboveGround = new Vector2(x4, 0f);
		quad[2] = default(VertexLightSourceQuad);
		quad[2].Position = new Vector3(x2, y2, z);
		quad[2].WorldPosition = worldPosition;
		ref VertexLightSourceQuad reference3 = ref quad[2];
		Renderable parent3 = Parent;
		reference3.TextureCoordinate = ((parent3 != null && parent3.FlipHorizontally) ? new Vector2(num, y3) : new Vector2(x3, y3));
		quad[2].HeightAboveGround = new Vector2(x5, 0f);
		quad[3] = default(VertexLightSourceQuad);
		quad[3].Position = new Vector3(x, y2, z);
		quad[3].WorldPosition = worldPosition;
		ref VertexLightSourceQuad reference4 = ref quad[3];
		Renderable parent4 = Parent;
		reference4.TextureCoordinate = ((parent4 != null && parent4.FlipHorizontally) ? new Vector2(x3, y3) : new Vector2(num, y3));
		quad[3].HeightAboveGround = new Vector2(x5, 0f);
		spriteOrLocationIsDirty = false;
	}

	public int CompareTo(object obj)
	{
		ILocatable locatable = obj as ILocatable;
		return (int)(Location.Y - locatable.Location.Y);
	}
}
