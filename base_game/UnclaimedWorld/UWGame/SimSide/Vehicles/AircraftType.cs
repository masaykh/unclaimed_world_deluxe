using System;

namespace UWGame.SimSide.Vehicles;

public class AircraftType
{
	public float MaxVerticalAcceleration;

	public float MaxRollDegreeWhenTurning;

	public float MaxPitchInRadians;

	public float PitchChangeSpeed;

	public float DuctChangeAngleSpeed = (float)Math.PI / 2f;

	public float MaxPropellerSpeed = 30f;

	public float PropellerAcceleration = 4f;

	public float AltitudeForDust = 200f;
}
