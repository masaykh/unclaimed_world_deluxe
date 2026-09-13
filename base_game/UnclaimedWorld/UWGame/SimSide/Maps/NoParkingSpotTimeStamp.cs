using System;

namespace UWGame.SimSide.Maps;

public struct NoParkingSpotTimeStamp
{
	public NoParkingSpot NoParkingSpot;

	public DateTime Timestamp;

	public NoParkingSpotTimeStamp(NoParkingSpot noParkingSpot, DateTime time)
	{
		NoParkingSpot = noParkingSpot;
		Timestamp = time;
	}
}
