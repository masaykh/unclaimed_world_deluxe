using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps;

[DebuggerDisplay("{X},{Y}")]
public struct SubtilePos
{
	public ushort X;

	public ushort Y;

	public SubtilePos(ushort x, ushort y)
	{
		X = x;
		Y = y;
	}

	public SubtilePos(Point pos)
	{
		X = (ushort)pos.X;
		Y = (ushort)pos.Y;
	}

	public static SubtilePos operator +(SubtilePos t1, SubtilePos t2)
	{
		return new SubtilePos((ushort)(t1.X + t2.X), (ushort)(t1.Y + t2.Y));
	}

	public static SubtilePos operator -(SubtilePos t1, SubtilePos t2)
	{
		return new SubtilePos((ushort)(t1.X - t2.X), (ushort)(t1.Y - t2.Y));
	}

	public bool Equals(SubtilePos p)
	{
		if (X == p.X)
		{
			return Y == p.Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SubtilePos p))
		{
			return false;
		}
		return Equals(p);
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static bool operator ==(SubtilePos c1, SubtilePos c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(SubtilePos c1, SubtilePos c2)
	{
		return !c1.Equals(c2);
	}

	public Point ToPoint()
	{
		return new Point(X, Y);
	}
}
