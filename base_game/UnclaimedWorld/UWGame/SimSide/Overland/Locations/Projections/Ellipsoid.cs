using System;

namespace UWGame.SimSide.Overland.Locations.Projections;

public abstract class Ellipsoid : IEquatable<Ellipsoid>
{
	public double SemiMajorAxis { get; protected set; }

	public double Flattening { get; protected set; }

	public bool Equals(Ellipsoid other)
	{
		bool num = SemiMajorAxis == other.SemiMajorAxis;
		bool flag = Flattening == other.Flattening;
		return num && flag;
	}
}
