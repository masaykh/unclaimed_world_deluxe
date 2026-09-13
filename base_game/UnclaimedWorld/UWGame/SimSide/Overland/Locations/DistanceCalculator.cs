using System;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Overland.Locations;

public static class DistanceCalculator
{
	public static double Haversine(GeodeticCoordinate coordinate1, GeodeticCoordinate coordinate2, double worldRadius)
	{
		double num = (coordinate1.Latitude - coordinate2.Latitude) * (Math.PI / 180.0);
		double num2 = (coordinate1.Longitude - coordinate2.Longitude) * (Math.PI / 180.0);
		double num3 = Math.Sin(num / 2.0) * Math.Sin(num / 2.0) + Math.Cos(coordinate1.Latitude * (Math.PI / 180.0)) * Math.Cos(coordinate2.Latitude * (Math.PI / 180.0)) * Math.Sin(num2 / 2.0) * Math.Sin(num2 / 2.0);
		double num4 = 2.0 * Math.Atan2(Math.Sqrt(num3), Math.Sqrt(1.0 - num3));
		return worldRadius * num4;
	}

	public static double Spherical(GeodeticCoordinate coordinate1, GeodeticCoordinate coordinate2, double worldRadius)
	{
		return Math.Acos(Math.Sin(coordinate1.Latitude * (Math.PI / 180.0)) * Math.Sin(coordinate2.Latitude * (Math.PI / 180.0)) + Math.Cos(coordinate1.Latitude * (Math.PI / 180.0)) * Math.Cos(coordinate2.Latitude * (Math.PI / 180.0)) * Math.Cos(coordinate2.Longitude * (Math.PI / 180.0) - coordinate1.Longitude * (Math.PI / 180.0))) * worldRadius;
	}

	public static GeodeticCoordinate CoordFromDistance(GeodeticCoordinate start, double bearing, double distance, double worldRadius)
	{
		distance /= worldRadius;
		double num = start.Latitude.ToRadians();
		double num2 = start.Longitude.ToRadians();
		double num3 = Math.Asin(Math.Sin(num) * Math.Cos(distance) + Math.Cos(num) * Math.Sin(distance) * Math.Cos(bearing));
		return new GeodeticCoordinate(((num2 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance) * Math.Cos(num), Math.Cos(distance) - Math.Sin(num) * Math.Sin(num3)) + Math.PI * 3.0) % (Math.PI * 2.0) - Math.PI).ToDegrees(), num3.ToDegrees());
	}

	public static GeodeticCoordinate GetIntermediatePoint(GeodeticCoordinate start, GeodeticCoordinate end, double fraction, double distance, double worldRadius)
	{
		distance /= worldRadius;
		double num = MathHelper.ToRadians((float)start.Latitude);
		double num2 = MathHelper.ToRadians((float)start.Longitude);
		double num3 = MathHelper.ToRadians((float)end.Latitude);
		double num4 = MathHelper.ToRadians((float)end.Longitude);
		double num5 = Math.Sin((1.0 - fraction) * distance) / Math.Sin(distance);
		double num6 = Math.Sin(fraction * distance) / Math.Sin(distance);
		double num7 = Math.Cos(num);
		double num8 = Math.Cos(num3);
		double x = num5 * num7 * Math.Cos(num2) + num6 * num8 * Math.Cos(num4);
		double num9 = num5 * num7 * Math.Sin(num2) + num6 * num8 * Math.Sin(num4);
		double num10 = Math.Atan2(num5 * Math.Sin(num) + num6 * Math.Sin(num3), Math.Sqrt(Math.Pow(x, 2.0) + Math.Pow(num9, 2.0)));
		double num11 = Math.Atan2(num9, x);
		return new GeodeticCoordinate(latitude: MathHelper.ToDegrees((float)num10), longitude: MathHelper.ToDegrees((float)num11));
	}

	public static double GetBearing(GeodeticCoordinate? start, GeodeticCoordinate? end)
	{
		if (!start.HasValue || !end.HasValue)
		{
			return 0.0;
		}
		double latitude = start.Value.Latitude;
		double num = start.Value.Longitude * -1.0;
		double latitude2 = end.Value.Latitude;
		double num2 = end.Value.Longitude * -1.0;
		double num3 = Math.Atan2(Math.Sin(num - num2) * Math.Cos(latitude2), Math.Cos(latitude) * Math.Sin(latitude2) - Math.Sin(latitude) * Math.Cos(latitude2) * Math.Cos(num - num2));
		return num3 - Math.PI * 2.0 * Math.Floor(num3 / (Math.PI * 2.0));
	}
}
