using System;

namespace UWGame.SimSide.Overland.Locations;

public static class Double
{
	public static double ToDegrees(this double d)
	{
		return d * (180.0 / Math.PI);
	}

	public static double ToRadians(this double d)
	{
		return d * (Math.PI / 180.0);
	}
}
