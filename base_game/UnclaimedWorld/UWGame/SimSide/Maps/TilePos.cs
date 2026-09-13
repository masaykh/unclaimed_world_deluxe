using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps;

[DebuggerDisplay("{X},{Y}")]
public struct TilePos
{
	public int X;

	public int Y;

	public TilePos(Point pos)
	{
		X = pos.X;
		Y = pos.Y;
	}

	public TilePos(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static TilePos operator +(TilePos t1, TilePos t2)
	{
		return new TilePos(t1.X + t2.X, t1.Y + t2.Y);
	}

	public static TilePos operator -(TilePos t1, TilePos t2)
	{
		return new TilePos(t1.X - t2.X, t1.Y - t2.Y);
	}

	public Point ToPoint()
	{
		return new Point(X, Y);
	}

	public string ToLink()
	{
		return $"P{X},{Y}";
	}

	public override string ToString()
	{
		return $"(X:{X},Y:{Y})";
	}

	public bool Equals(TilePos p)
	{
		if (X == p.X)
		{
			return Y == p.Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TilePos p))
		{
			return false;
		}
		return Equals(p);
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static bool operator ==(TilePos c1, TilePos c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(TilePos c1, TilePos c2)
	{
		return !c1.Equals(c2);
	}
}
