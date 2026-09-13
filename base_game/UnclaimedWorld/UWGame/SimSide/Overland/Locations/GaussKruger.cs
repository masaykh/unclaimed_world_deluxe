using System;
using UWGame.SimSide.Overland.Locations.Projections;

namespace UWGame.SimSide.Overland.Locations;

public static class GaussKruger
{
	public static GridCoordinate GeodeticToGrid(GeodeticCoordinate coordinate, Projection projection)
	{
		GridCoordinate obj = new GridCoordinate
		{
			Projection = projection
		};
		double flattening = projection.Ellipsoid.Flattening;
		double semiMajorAxis = projection.Ellipsoid.SemiMajorAxis;
		double num = flattening * (2.0 - flattening);
		double num2 = flattening / (2.0 - flattening);
		double num3 = semiMajorAxis / (1.0 + num2) * (1.0 + num2 * num2 / 4.0 + num2 * num2 * num2 * num2 / 64.0);
		double num4 = num;
		double num5 = (5.0 * num * num - num * num * num) / 6.0;
		double num6 = (104.0 * num * num * num - 45.0 * num * num * num * num) / 120.0;
		double num7 = 1237.0 * num * num * num * num / 1260.0;
		double num8 = num2 / 2.0 - 2.0 * num2 * num2 / 3.0 + 5.0 * num2 * num2 * num2 / 16.0 + 41.0 * num2 * num2 * num2 * num2 / 180.0;
		double num9 = 13.0 * num2 * num2 / 48.0 - 3.0 * num2 * num2 * num2 / 5.0 + 557.0 * num2 * num2 * num2 * num2 / 1440.0;
		double num10 = 61.0 * num2 * num2 * num2 / 240.0 - 103.0 * num2 * num2 * num2 * num2 / 140.0;
		double num11 = 49561.0 * num2 * num2 * num2 * num2 / 161280.0;
		double num12 = coordinate.Latitude.ToRadians();
		double num13 = coordinate.Longitude.ToRadians();
		double num14 = projection.CentralMeridianLon.ToRadians();
		double num15 = num13 - num14;
		double num16 = num12 - Math.Sin(num12) * Math.Cos(num12) * (num4 + num5 * Math.Pow(Math.Sin(num12), 2.0) + num6 * Math.Pow(Math.Sin(num12), 4.0) + num7 * Math.Pow(Math.Sin(num12), 6.0));
		double num17 = Math.Atan(Math.Tan(num16) / Math.Cos(num15));
		double num18 = Atanh(Math.Cos(num16) * Math.Sin(num15));
		obj.X = projection.CentralMeridianScale * num3 * (num17 + num8 * Math.Sin(2.0 * num17) * Math.Cosh(2.0 * num18) + num9 * Math.Sin(4.0 * num17) * Math.Cosh(4.0 * num18) + num10 * Math.Sin(6.0 * num17) * Math.Cosh(6.0 * num18) + num11 * Math.Sin(8.0 * num17) * Math.Cosh(8.0 * num18)) + projection.FalseNorthing;
		obj.Y = projection.CentralMeridianScale * num3 * (num18 + num8 * Math.Cos(2.0 * num17) * Math.Sinh(2.0 * num18) + num9 * Math.Cos(4.0 * num17) * Math.Sinh(4.0 * num18) + num10 * Math.Cos(6.0 * num17) * Math.Sinh(6.0 * num18) + num11 * Math.Cos(8.0 * num17) * Math.Sinh(8.0 * num18)) + projection.FalseEasting;
		return obj;
	}

	public static GeodeticCoordinate GridToGeodetic(GridCoordinate coordinate)
	{
		Projection projection = coordinate.Projection;
		double flattening = projection.Ellipsoid.Flattening;
		double semiMajorAxis = projection.Ellipsoid.SemiMajorAxis;
		double num = flattening * (2.0 - flattening);
		double num2 = flattening / (2.0 - flattening);
		double num3 = semiMajorAxis / (1.0 + num2) * (1.0 + num2 * num2 / 4.0 + num2 * num2 * num2 * num2 / 64.0);
		double num4 = num + num * num + num * num * num + num * num * num * num;
		double num5 = (0.0 - (7.0 * num * num + 17.0 * num * num * num + 30.0 * num * num * num * num)) / 6.0;
		double num6 = (224.0 * num * num * num + 889.0 * num * num * num * num) / 120.0;
		double num7 = (0.0 - 4279.0 * num * num * num * num) / 1260.0;
		double num8 = num2 / 2.0 - 2.0 * num2 * num2 / 3.0 + 37.0 * num2 * num2 * num2 / 96.0 - num2 * num2 * num2 * num2 / 360.0;
		double num9 = num2 * num2 / 48.0 + num2 * num2 * num2 / 15.0 - 437.0 * num2 * num2 * num2 * num2 / 1440.0;
		double num10 = 17.0 * num2 * num2 * num2 / 480.0 - 37.0 * num2 * num2 * num2 * num2 / 840.0;
		double num11 = 4397.0 * num2 * num2 * num2 * num2 / 161280.0;
		double num12 = projection.CentralMeridianLon.ToRadians();
		double num13 = (coordinate.X - projection.FalseNorthing) / (projection.CentralMeridianScale * num3);
		double num14 = (coordinate.Y - projection.FalseEasting) / (projection.CentralMeridianScale * num3);
		double num15 = num13 - num8 * Math.Sin(2.0 * num13) * Math.Cosh(2.0 * num14) - num9 * Math.Sin(4.0 * num13) * Math.Cosh(4.0 * num14) - num10 * Math.Sin(6.0 * num13) * Math.Cosh(6.0 * num14) - num11 * Math.Sin(8.0 * num13) * Math.Cosh(8.0 * num14);
		double value = num14 - num8 * Math.Cos(2.0 * num13) * Math.Sinh(2.0 * num14) - num9 * Math.Cos(4.0 * num13) * Math.Sinh(4.0 * num14) - num10 * Math.Cos(6.0 * num13) * Math.Sinh(6.0 * num14) - num11 * Math.Cos(8.0 * num13) * Math.Sinh(8.0 * num14);
		double num16 = Math.Asin(Math.Sin(num15) / Math.Cosh(value));
		double num17 = Math.Atan(Math.Sinh(value) / Math.Cos(num15));
		double d = num12 + num17;
		double d2 = num16 + Math.Sin(num16) * Math.Cos(num16) * (num4 + num5 * Math.Pow(Math.Sin(num16), 2.0) + num6 * Math.Pow(Math.Sin(num16), 4.0) + num7 * Math.Pow(Math.Sin(num16), 6.0));
		return new GeodeticCoordinate(d.ToDegrees(), d2.ToDegrees());
	}

	private static double Atanh(double d)
	{
		return 0.5 * Math.Log((1.0 + d) / (1.0 - d));
	}
}
