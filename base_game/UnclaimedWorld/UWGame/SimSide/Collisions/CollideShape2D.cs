using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Collisions;

public class CollideShape2D
{
	private CollidePrim primitiveType;

	private Vector2 boundsUpperLeft;

	private Vector2 boundsLowerRight;

	private Vector2 offset = Vector2.Zero;

	private bool hFlipped;

	private Vector2 center = Vector2.Zero;

	private float radius;

	private float radiusSquared;

	public CollidePrim PrimitiveType => primitiveType;

	[XmlIgnore]
	public Collidable<Entity> Parent { get; set; }

	public Vector2 BoundsUpperLeft
	{
		get
		{
			return boundsUpperLeft;
		}
		set
		{
			boundsUpperLeft = value;
			UpdateCenterAndRadius();
		}
	}

	public Vector2 BoundsUpperRight
	{
		get
		{
			return new Vector2(boundsLowerRight.X, boundsUpperLeft.Y);
		}
		set
		{
			boundsLowerRight.X = value.X;
			boundsUpperLeft.Y = value.Y;
			UpdateCenterAndRadius();
		}
	}

	public Vector2 BoundsLowerRight
	{
		get
		{
			return boundsLowerRight;
		}
		set
		{
			boundsLowerRight = value;
			UpdateCenterAndRadius();
		}
	}

	public Vector2 BoundsLowerLeft
	{
		get
		{
			return new Vector2(boundsUpperLeft.X, boundsLowerRight.Y);
		}
		set
		{
			boundsUpperLeft.X = value.X;
			boundsLowerRight.Y = value.Y;
			UpdateCenterAndRadius();
		}
	}

	public float BoundsTop
	{
		get
		{
			return BoundsUpperLeft.Y;
		}
		set
		{
			boundsUpperLeft.Y = value;
			UpdateCenterAndRadius();
		}
	}

	public float BoundsLeft
	{
		get
		{
			return BoundsUpperLeft.X;
		}
		set
		{
			boundsUpperLeft.X = value;
			UpdateCenterAndRadius();
		}
	}

	public float BoundsBottom
	{
		get
		{
			return BoundsLowerRight.Y;
		}
		set
		{
			boundsLowerRight.Y = value;
			UpdateCenterAndRadius();
		}
	}

	public float BoundsRight
	{
		get
		{
			return BoundsLowerRight.X;
		}
		set
		{
			boundsLowerRight.X = value;
			UpdateCenterAndRadius();
		}
	}

	public float BoundsWidth => boundsLowerRight.X - boundsUpperLeft.X;

	public float BoundsHeight => boundsLowerRight.Y - boundsUpperLeft.Y;

	public Vector2 Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
			UpdateCenterAndRadius();
		}
	}

	public bool HFlipped => hFlipped;

	public Vector2 Center => center;

	public float Radius => radius;

	public float RadiusSquared => radiusSquared;

	public void FlipHorizontally()
	{
		offset.X *= -1f;
		hFlipped = !hFlipped;
		UpdateCenterAndRadius();
	}

	public CollideShape2D()
	{
	}

	public CollideShape2D(Vector2 topleft, Vector2 bottomright)
	{
		primitiveType = CollidePrim.Rectangle;
		boundsUpperLeft = topleft;
		boundsLowerRight = bottomright;
		Offset = Vector2.Zero;
		UpdateCenterAndRadius();
	}

	public CollideShape2D(float top, float left, float bottom, float right)
	{
		primitiveType = CollidePrim.Rectangle;
		boundsUpperLeft = new Vector2(left, top);
		boundsLowerRight = new Vector2(right, bottom);
		Offset = Vector2.Zero;
		UpdateCenterAndRadius();
	}

	public CollideShape2D(Vector2 center, float radius)
	{
		primitiveType = CollidePrim.Circle;
		boundsUpperLeft = new Vector2(center.X - radius, center.Y - radius);
		boundsLowerRight = new Vector2(center.X + radius, center.Y + radius);
		Offset = Vector2.Zero;
		UpdateCenterAndRadius();
	}

	public void BeCircle()
	{
		primitiveType = CollidePrim.Circle;
	}

	public float GetWidthHeightRatio()
	{
		return Math.Abs(boundsUpperLeft.X - boundsLowerRight.X) / Math.Abs(boundsUpperLeft.Y - boundsLowerRight.Y);
	}

	public float GetWidth()
	{
		return Math.Abs(boundsUpperLeft.X - boundsLowerRight.X);
	}

	public float GetHeight()
	{
		return Math.Abs(boundsUpperLeft.Y - boundsLowerRight.Y);
	}

	public void AdjustShape(Vector2 offset, float? radius, float? width, float? height)
	{
		Vector2 vector = Center;
		if (primitiveType == CollidePrim.Circle)
		{
			boundsUpperLeft = new Vector2(vector.X - radius.Value, vector.Y - radius.Value);
			boundsLowerRight = new Vector2(vector.X + radius.Value, vector.Y + radius.Value);
		}
		else if (primitiveType == CollidePrim.Rectangle)
		{
			if (width.HasValue)
			{
				boundsUpperLeft.X = vector.X - 0.5f * width.Value;
				boundsLowerRight.X = vector.X + 0.5f * width.Value;
			}
			else if (height.HasValue)
			{
				boundsUpperLeft.Y = vector.Y - 0.5f * height.Value;
				boundsLowerRight.Y = vector.Y + 0.5f * height.Value;
			}
		}
		Offset = offset;
		UpdateCenterAndRadius();
	}

	public CollideShape2D(CollideShape2D other)
	{
		primitiveType = other.PrimitiveType;
		boundsUpperLeft = other.BoundsUpperLeft;
		boundsLowerRight = other.BoundsLowerRight;
		Offset = other.Offset;
		UpdateCenterAndRadius();
	}

	public bool ContainsPoint(Vector2 point)
	{
		Vector2 vector = Center;
		Vector2 vector2 = boundsUpperLeft;
		Vector2 vector3 = boundsLowerRight;
		if (Parent != null)
		{
			vector += Parent.Center + offset;
			vector2 += Parent.Center + offset;
			vector3 += Parent.Center + offset;
		}
		if (!(vector2.X <= point.X) || !(vector3.X >= point.X) || !(vector2.Y <= point.Y) || !(vector3.Y >= point.Y))
		{
			return false;
		}
		if (PrimitiveType == CollidePrim.Rectangle)
		{
			return true;
		}
		if ((point - vector).LengthSquared() < Radius * Radius)
		{
			return true;
		}
		return false;
	}

	public bool isBoundsOverlap(CollideShape2D that)
	{
		if (!(BoundsBottom < that.BoundsTop) && !(BoundsTop > that.BoundsBottom) && !(BoundsRight < that.BoundsLeft))
		{
			return !(BoundsLeft > that.BoundsRight);
		}
		return false;
	}

	public float GetShapeOverlapAmount(CollideShape2D that)
	{
		if (!isBoundsOverlap(that))
		{
			return 0f;
		}
		if (primitiveType == CollidePrim.Rectangle && that.PrimitiveType == CollidePrim.Rectangle)
		{
			return 1f;
		}
		if (primitiveType == CollidePrim.Circle && that.PrimitiveType == CollidePrim.Circle)
		{
			float num = Common.RoughVectorMagnitude((Center - that.Center).ToVector3());
			return Radius + that.Radius - num;
		}
		CollideShape2D collideShape2D = that;
		CollideShape2D collideShape2D2 = this;
		if (primitiveType == CollidePrim.Circle)
		{
			collideShape2D = this;
			collideShape2D2 = that;
		}
		if (collideShape2D2.primitiveType != CollidePrim.Rectangle)
		{
			throw new Exception("oops, flawed logic. the other rect is not a rectangle after all.");
		}
		if (collideShape2D.primitiveType != CollidePrim.Circle)
		{
			throw new Exception("oops, flawed logic. the other circ is not a circle after all.");
		}
		if (collideShape2D2.BoundsLeft < collideShape2D.Center.X && collideShape2D.Center.X < collideShape2D2.BoundsRight && collideShape2D2.BoundsTop - collideShape2D.Radius < collideShape2D.Center.Y && collideShape2D.Center.Y < collideShape2D2.BoundsBottom + collideShape2D.Radius)
		{
			return 1f;
		}
		if (collideShape2D2.BoundsTop < collideShape2D.Center.Y && collideShape2D.Center.Y < collideShape2D2.BoundsBottom && collideShape2D2.BoundsLeft - collideShape2D.Radius < collideShape2D.Center.X && collideShape2D.Center.X < collideShape2D2.BoundsRight + collideShape2D.Radius)
		{
			return 1f;
		}
		if (that.ContainsPoint(boundsUpperLeft) || that.ContainsPoint(BoundsUpperRight) || that.ContainsPoint(boundsLowerRight) || that.ContainsPoint(BoundsLowerLeft))
		{
			return 1f;
		}
		return 0f;
	}

	private void UpdateCenterAndRadius()
	{
		center = (boundsUpperLeft + boundsLowerRight) * 0.5f;
		radius = (boundsLowerRight.X - boundsUpperLeft.X) * 0.5f;
		radiusSquared = radius * radius;
	}
}
