namespace UWGame.SimSide.Overland.Locations;

public struct GeodeticCoordinate
{
	private double latitude;

	private double longitude;

	// PORT FIX. The studio's were get-only, and XmlSerializer writes only what it can set: every
	// SiteData.Coords came out as an empty <Coords /> and was read back as 0, 0, so an exported
	// scenario's world map showed none of its locations (Kastuk, 2026-11-14). Saves are unaffected:
	// Snapshotter.DoGeodeticCoordinate goes through these properties and the constructor.
	public double Latitude
	{
		get { return latitude; }
		set { latitude = value; }
	}

	public double Longitude
	{
		get { return longitude; }
		set { longitude = value; }
	}

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
