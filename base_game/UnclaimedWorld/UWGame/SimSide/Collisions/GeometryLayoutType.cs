using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Collisions;

public class GeometryLayoutType
{
	public bool CausesCollisions;

	public bool GridAlignedPlacement;

	public CollideShape2D[] Shapes;

	public CollideShape2D[] SelectionShapes;

	private float pad;

	private CollidePrim padShape;

	public float Pad
	{
		get
		{
			return pad;
		}
		set
		{
			pad = value;
		}
	}

	public CollidePrim PadShape
	{
		get
		{
			return padShape;
		}
		set
		{
			padShape = value;
		}
	}

	public GeometryLayoutType()
	{
		Shapes = new CollideShape2D[1]
		{
			new CollideShape2D(Vector2.Zero, 0f)
		};
	}

	public void Initialize(EntityType parent)
	{
	}

	public void PostLoadContentInitialize()
	{
	}
}
