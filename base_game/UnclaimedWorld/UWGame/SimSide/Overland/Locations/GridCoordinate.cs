using System;
using UWGame.SimSide.Overland.Locations.Projections;

namespace UWGame.SimSide.Overland.Locations;

public class GridCoordinate : IEquatable<GridCoordinate>
{
	private const double MaximumDelta = 1.5;

	public double X { get; set; }

	public double Y { get; set; }

	public Projection Projection { get; set; }

	public bool Equals(GridCoordinate other)
	{
		double num = X - other.X;
		double num2 = Y - other.Y;
		bool num3 = num < 1.5 && num > -1.5;
		bool flag = num2 < 1.5 && num2 > -1.5;
		bool flag2 = Projection.Equals(other.Projection);
		return num3 && flag && flag2;
	}
}
