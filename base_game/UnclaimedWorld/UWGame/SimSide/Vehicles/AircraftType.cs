using System;
using System.Xml.Serialization;

namespace UWGame.SimSide.Vehicles;

public class AircraftType
{
	public float MaxAirSpeed;

	public float MaxVerticalAcceleration;

	public float MaxVerticalMoveSpeed;

	public float MaxRollDegreeWhenTurning;

	public float MaxPitchInRadians;

	public float PitchChangeSpeed;

	public float DuctChangeAngleSpeed = (float)Math.PI / 2f;

	public float MaxPropellerSpeed = 30f;

	public float PropellerAcceleration = 4f;

	public float CruiseAltitude = 200f;

	public float AltitudeForDust = 200f;

	[XmlIgnore]
	public float EstimatedTakeOffLandingTime;

	public AircraftType()
	{
		EstimatedTakeOffLandingTime = 1.5f * MaxVerticalMoveSpeed / CruiseAltitude;
	}
}
