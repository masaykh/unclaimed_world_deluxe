using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Maps;

public struct NoParkingSpot
{
	private Point Destination;

	private EntityType VehicleType;

	private ProtectionLevel ProtectionLevel;

	private EntityType DriverType;

	private ThreatStance Approach;

	public NoParkingSpot(EntityType vehicleType, ProtectionLevel protectionLevel, EntityType driverType, ThreatStance approach, Point destination)
	{
		Destination = destination;
		VehicleType = vehicleType;
		ProtectionLevel = protectionLevel;
		DriverType = driverType;
		Approach = approach;
	}
}
