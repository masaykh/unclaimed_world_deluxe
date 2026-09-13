using Microsoft.Xna.Framework;

namespace RoundLineCode;

public class Disc : RoundLine
{
	public Vector2 Pos
	{
		get
		{
			return base.P0;
		}
		set
		{
			base.P0 = value;
			base.P1 = value;
		}
	}

	public Disc(Vector2 p)
		: base(p, p)
	{
	}

	public Disc(float x, float y)
		: base(x, y, x, y)
	{
	}
}
