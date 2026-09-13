namespace UWGame.SimSide.Overland.Locations;

public struct GeodeticCoordinate
{
	private readonly double latitude;

	private readonly double longitude;

	private const int EqualityDecimals = 4;

	public double Latitude => latitude;

	public double Longitude => longitude;

	public GeodeticCoordinate(double longitude, double latitude)
	{
		this.longitude = longitude;
		this.latitude = latitude;
	}

	public override string ToString()
	{
		return $"{latitude:N2} (latitude), {longitude:N2} (longitude)";
	}
}
