using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Vehicles;

public class CargoSlot
{
	private PassengerOrCargoSlot parent;

	private int sequence;

	public float BulkCarried;

	public Entity TargetedByHauler;

	public bool IsFull()
	{
		return BulkCarried >= parent.PassengerOrCargoSlotType.CargoSlotType.Capacity;
	}

	public float GetRemainingRoom()
	{
		return Common.ClampBottom(parent.PassengerOrCargoSlotType.CargoSlotType.Capacity - BulkCarried, 0f);
	}

	public CargoSlot(PassengerOrCargoSlot parent)
	{
		this.parent = parent;
	}
}
