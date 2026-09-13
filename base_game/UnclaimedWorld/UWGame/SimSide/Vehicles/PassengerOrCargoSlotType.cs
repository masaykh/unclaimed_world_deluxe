using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Vehicles;

public class PassengerOrCargoSlotType
{
	public string AttachPointName;

	public PassengerSlotType PassengerSlotType;

	public CargoSlotType CargoSlotType;

	public Entrance Entrance;

	public Vector2? PointToFaceAtEntrance;
}
