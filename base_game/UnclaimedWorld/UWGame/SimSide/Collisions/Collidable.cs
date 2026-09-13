using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Collisions;

[DebuggerDisplay("Parent={parent}")]
public class Collidable<T>
{
	public delegate void MoveHandlerDelegate(Collidable<T> collidableMoving);

	public delegate void DestroyHandlerDelegate(Collidable<T> collidableToDestroy);

	private Vector2 center;

	private Vector2 size;

	private bool enabled;

	private CollideShape2D bounds;

	private List<CollideShape2D> childShapes;

	private bool flipHorizontally;

	private T parent;

	private static Vector2 spread1 = new Vector2(-8f, -8f);

	private static Vector2 spread2 = new Vector2(0f, -8f);

	private static Vector2 spread3 = new Vector2(8f, -8f);

	private static Vector2 spread4 = new Vector2(-8f, 0f);

	private static Vector2 spread5 = new Vector2(8f, 0f);

	private static Vector2 spread6 = new Vector2(-8f, 8f);

	private static Vector2 spread7 = new Vector2(0f, 8f);

	private static Vector2 spread8 = new Vector2(8f, 8f);

	public Vector2 Center
	{
		get
		{
			return center;
		}
		set
		{
			if (center != value)
			{
				center = value;
				OnChange();
			}
		}
	}

	public Vector2 Size
	{
		get
		{
			return size;
		}
		set
		{
			if (size != value)
			{
				size = value;
				OnChange();
			}
		}
	}

	public bool Enabled => enabled;

	public CollideShape2D Bounds
	{
		get
		{
			return bounds;
		}
		set
		{
			if (bounds != value)
			{
				bounds = value;
				OnChange();
			}
		}
	}

	public bool IsComposite
	{
		get
		{
			if (childShapes != null)
			{
				return childShapes.Count > 0;
			}
			return false;
		}
	}

	public bool FlipHorizontally
	{
		get
		{
			return flipHorizontally;
		}
		set
		{
			if (flipHorizontally == value)
			{
				return;
			}
			flipHorizontally = value;
			if (childShapes == null)
			{
				return;
			}
			foreach (CollideShape2D childShape in childShapes)
			{
				childShape.FlipHorizontally();
			}
			OnChange();
		}
	}

	public T Parent => parent;

	public event MoveHandlerDelegate Move;

	public event DestroyHandlerDelegate Destroy;

	public void AddChildShape(CollideShape2D child)
	{
		CollideShape2D collideShape2D = new CollideShape2D(child);
		if (childShapes == null)
		{
			childShapes = new List<CollideShape2D>();
		}
		childShapes.Add(collideShape2D);
		collideShape2D.Parent = this as Collidable<Entity>;
		OnChange();
	}

	protected void OnChange()
	{
		if (childShapes != null)
		{
			Vector2 zero = Vector2.Zero;
			Vector2 zero2 = Vector2.Zero;
			foreach (CollideShape2D childShape in childShapes)
			{
				if (zero.X > childShape.BoundsUpperLeft.X + childShape.Offset.X)
				{
					zero.X = childShape.BoundsUpperLeft.X + childShape.Offset.X;
				}
				if (zero.Y > childShape.BoundsUpperLeft.Y + childShape.Offset.Y)
				{
					zero.Y = childShape.BoundsUpperLeft.Y + childShape.Offset.Y;
				}
				if (zero2.X < childShape.BoundsLowerRight.X + childShape.Offset.X)
				{
					zero2.X = childShape.BoundsLowerRight.X + childShape.Offset.X;
				}
				if (zero2.Y < childShape.BoundsLowerRight.Y + childShape.Offset.Y)
				{
					zero2.Y = childShape.BoundsLowerRight.Y + childShape.Offset.Y;
				}
			}
			size.X = Math.Max(Math.Abs(zero.X), Math.Abs(zero2.X));
			size.Y = Math.Max(Math.Abs(zero.Y), Math.Abs(zero2.Y));
		}
		bounds.BoundsUpperLeft = center - size;
		bounds.BoundsLowerRight = center + size;
		if (this.Move != null)
		{
			this.Move(this);
		}
		if (parent is Entity { GeometryLayout: not null } entity)
		{
			entity.FootprintIsDirty = true;
		}
	}

	public void IterateChildShapes(Action<CollideShape2D, int> iterateMethod)
	{
		if (childShapes != null && childShapes.Count != 0)
		{
			for (int i = 0; i < childShapes.Count; i++)
			{
				iterateMethod(childShapes[i], i);
			}
		}
	}

	public bool IterateChildShapes(IterateCollidableMethod iterateMethod, Color color, bool isSelected = false)
	{
		if (childShapes == null || childShapes.Count == 0)
		{
			return false;
		}
		bool flag = false;
		foreach (CollideShape2D childShape in childShapes)
		{
			flag |= iterateMethod(childShape, color, isSelected);
		}
		return flag;
	}

	public bool IterateChildShapes(IterateCollidableMethod iterateMethod, ref CollideShape2D other)
	{
		if (childShapes == null || childShapes.Count == 0)
		{
			return false;
		}
		bool flag = false;
		foreach (CollideShape2D childShape in childShapes)
		{
			flag |= iterateMethod(childShape, Color.Black, sel: false);
		}
		return flag;
	}

	public void NudgeChildShape(int idx, int x, int y, int? r = null, int? w = null)
	{
		if (childShapes == null || idx >= childShapes.Count)
		{
			return;
		}
		CollideShape2D collideShape2D = childShapes[idx];
		Vector2 offset = collideShape2D.Offset + new Vector2(x, y);
		float? num = null;
		float? num2 = null;
		float? num3 = null;
		if (collideShape2D.PrimitiveType == CollidePrim.Circle)
		{
			num = collideShape2D.Radius + (float?)r;
			if (num <= 4f)
			{
				num = 4f;
			}
		}
		if (collideShape2D.PrimitiveType == CollidePrim.Rectangle)
		{
			if (r.HasValue)
			{
				int num4 = 2 * r.Value;
				num2 = collideShape2D.GetHeight() + (float)num4;
				if (num2 <= 4f)
				{
					num2 = 4f;
				}
			}
			if (w.HasValue)
			{
				num3 = collideShape2D.GetWidth() + (float)w.Value;
				if (num3 <= 8f)
				{
					num3 = 8f;
				}
			}
		}
		collideShape2D.AdjustShape(offset, num, num3, num2);
		OnChange();
	}

	public void BeCircle()
	{
		bounds.BeCircle();
	}

	protected void OnDestroy()
	{
		if (this.Destroy != null)
		{
			this.Destroy(this);
		}
	}

	public Collidable(T parent, Vector2? position, Vector2 size)
	{
		bounds = new CollideShape2D(0f, 0f, 1f, 1f);
		this.parent = parent;
		this.size = size;
		if (position.HasValue)
		{
			center = position.Value;
			OnChange();
		}
	}

	public void Delete()
	{
		OnDestroy();
	}

	public void SetEnabled()
	{
		enabled = true;
	}

	public void SetDisabled()
	{
		enabled = false;
	}

	public bool IsWithinShapes(Vector2 worldPos, bool testIfAnyPartOfSubtileIsInBounds)
	{
		if (testIfAnyPartOfSubtileIsInBounds)
		{
			return ContainsPoint(worldPos + spread1) || ContainsPoint(worldPos + spread2) || ContainsPoint(worldPos + spread3) || ContainsPoint(worldPos + spread4) || ContainsPoint(worldPos + spread5) || ContainsPoint(worldPos + spread6) || ContainsPoint(worldPos + spread7) || ContainsPoint(worldPos + spread8) || ContainsPoint(worldPos);
		}
		return ContainsPoint(worldPos);
	}

	public void GetBoundsWithPadding(float pad, out Vector2 from, out Vector2 to)
	{
		Vector2 vector = new Vector2(pad);
		from = Bounds.BoundsUpperLeft - vector;
		to = Bounds.BoundsLowerRight + vector;
		from = The.Map.ClampWorldPosition(from);
		to = The.Map.ClampWorldPosition(to);
		from = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(from));
		to = MapManager.SubTileToWorldPos(MapManager.WorldPosToSubtile(to));
	}

	public bool ContainsPoint(Vector2 point)
	{
		if (!Bounds.ContainsPoint(point))
		{
			return false;
		}
		if (childShapes != null)
		{
			foreach (CollideShape2D childShape in childShapes)
			{
				if (childShape.ContainsPoint(point))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ShapesContainEntities(float pad, Predicate<Entity> countEntity)
	{
		return IterateSubtilesBreakOnTrue(pad, (Vector2 s) => GeoLayoutSubtileContainsEntities(s, countEntity));
	}

	public bool IterateSubtilesBreakOnTrue(float pad, Predicate<Vector2> iterateMethod)
	{
		GetBoundsWithPadding(pad, out var from, out var to);
		return MapManager.IterateSubtilesBreakOnTrue(from, to, iterateMethod);
	}

	private bool GeoLayoutSubtileContainsEntities(Vector2 worldPos, Predicate<Entity> countEntity)
	{
		if (IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds: false))
		{
			Point subtile = MapManager.WorldPosToSubtile(worldPos);
			if (The.Map.SubtileContainsEntities(subtile, countEntity))
			{
				return true;
			}
		}
		return false;
	}

	public bool TestCollision(Collidable<T> other, out float overlap, out Vector2 ctr)
	{
		overlap = 0f;
		ctr = other.Center;
		if (IsComposite)
		{
			if (other.IsComposite)
			{
				foreach (CollideShape2D childShape in childShapes)
				{
					other.TestCollision(childShape, out overlap, out ctr);
				}
			}
			else
			{
				foreach (CollideShape2D childShape2 in childShapes)
				{
					if (childShape2.GetShapeOverlapAmount(other.Bounds) > overlap)
					{
						overlap = 0f;
						ctr = other.Center;
					}
				}
			}
		}
		else
		{
			if (other.IsComposite)
			{
				return other.TestCollision(Bounds, out overlap, out ctr);
			}
			overlap = Bounds.GetShapeOverlapAmount(other.Bounds);
			ctr = other.Center;
		}
		return overlap > 0f;
	}

	public bool TestCollision(CollideShape2D other, out float overlap, out Vector2 ctr)
	{
		overlap = 0f;
		ctr = Center;
		foreach (CollideShape2D childShape in childShapes)
		{
			Vector2 topleft = childShape.BoundsUpperLeft + childShape.Offset + Center;
			Vector2 bottomright = childShape.BoundsLowerRight + childShape.Offset + Center;
			CollideShape2D collideShape2D = new CollideShape2D(topleft, bottomright);
			if (childShape.PrimitiveType == CollidePrim.Circle)
			{
				collideShape2D.BeCircle();
			}
			float shapeOverlapAmount = collideShape2D.GetShapeOverlapAmount(other);
			if (shapeOverlapAmount > overlap)
			{
				overlap = shapeOverlapAmount;
				ctr = collideShape2D.Center;
			}
		}
		return overlap > 0f;
	}
}
