using System;

namespace UWGame.SimSide.Overland.Locations.Projections;

public abstract class Projection : IEquatable<Projection>
{
	public double CentralMeridianLon { get; protected set; }

	public double CentralMeridianScale { get; protected set; }

	public double LongitudeDelta { get; protected set; }

	public double FalseNorthing { get; protected set; }

	public double FalseEasting { get; protected set; }

	public Ellipsoid Ellipsoid { get; protected set; }

	public bool Equals(Projection other)
	{
		bool num = CentralMeridianLon == other.CentralMeridianLon;
		bool flag = CentralMeridianScale == other.CentralMeridianScale;
		bool flag2 = LongitudeDelta == other.LongitudeDelta;
		bool flag3 = FalseNorthing == other.FalseNorthing;
		bool flag4 = FalseEasting == other.FalseEasting;
		return (num && flag && flag2 && flag3 && flag4) & Ellipsoid.Equals(other.Ellipsoid);
	}
}
