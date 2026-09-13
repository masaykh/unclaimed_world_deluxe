using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Maps;

[DebuggerDisplay("{X},{Y},{Z}")]
public struct WorldLocation
{
	public float X;

	public float Y;

	public float Z;

	public WorldLocation(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public WorldLocation(Vector3 location)
	{
		X = location.X;
		Y = location.Y;
		Z = location.Z;
	}

	public static WorldLocation operator +(WorldLocation t1, WorldLocation t2)
	{
		return new WorldLocation(t1.X + t2.X, t1.Y + t2.Y, t1.Z + t2.Z);
	}

	public static Vector3 operator +(WorldLocation t1, Vector3 t2)
	{
		return new Vector3(t1.X + t2.X, t1.Y + t2.Y, t1.Z + t2.Z);
	}

	public static WorldLocation operator -(WorldLocation t1, WorldLocation t2)
	{
		return new WorldLocation(t1.X - t2.X, t1.Y - t2.Y, t1.Z - t2.Z);
	}

	public static Vector3 operator -(WorldLocation t1, Vector3 t2)
	{
		return new Vector3(t1.X - t2.X, t1.Y - t2.Y, t1.Z - t2.Z);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is WorldLocation p))
		{
			return false;
		}
		return Equals(p);
	}

	public bool Equals(WorldLocation p)
	{
		if (X == p.X && Y == p.Y)
		{
			return Z == p.Z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
	}

	public static bool operator ==(WorldLocation c1, WorldLocation c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(WorldLocation c1, WorldLocation c2)
	{
		return !c1.Equals(c2);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(X, Y, Z);
	}
}
