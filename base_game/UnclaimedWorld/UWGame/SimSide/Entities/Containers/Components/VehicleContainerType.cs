using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public class VehicleContainerType : ContainerType
{
	public enum VehicleTypes
	{
		Aircraft,
		Boat,
		LandVehicle
	}

	public enum Function
	{
		PersonalTransport,
		Hauling,
		Other
	}

	public VehicleTypes VehicleType;

	public RequiresReplenishType RequiresReplenishType;

	public ItemStorageType ItemStorageType;

	private int maxPassengers;

	public SurfaceType.TransportType Transport;

	public float LoadingRadius;

	public float UnladenWeight;

	public float MaxAcceleration;

	public float Deceleration;

	public RouteType[] CanNavigateRoutes;

	public AircraftType Aircraft;

	public float AverageOverlandTravelSpeed;

	public TerminalType.TypesOfTerminal? CanUseTerminal;

	public PassengerOrCargoSlotType[] PassengerOrCargoSlotTypes;

	public Function MainFunction;

	[XmlIgnore]
	public int MaxPassengers => maxPassengers;

	public override RequiresReplenishType GetRequiresReplenishType()
	{
		return RequiresReplenishType;
	}

	public VehicleContainerType()
	{
	}

	public VehicleContainerType(float? itemStorageCapacity = null)
	{
		if (itemStorageCapacity.HasValue)
		{
			ItemStorageType = new ItemStorageType(itemStorageCapacity.Value);
		}
	}

	public override Container CreateContainer(Entity parent)
	{
		return new VehicleContainer(parent);
	}

	public override void Initialize()
	{
		base.Initialize();
		maxPassengers = 0;
		if (PassengerOrCargoSlotTypes != null)
		{
			PassengerOrCargoSlotType[] passengerOrCargoSlotTypes = PassengerOrCargoSlotTypes;
			for (int i = 0; i < passengerOrCargoSlotTypes.Length; i++)
			{
				if (passengerOrCargoSlotTypes[i].PassengerSlotType != null)
				{
					maxPassengers++;
				}
			}
		}
		if (ItemStorageType != null)
		{
			ItemStorageType.Initialize();
		}
	}

	public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
	{
		base.PostInitValidate(parent, ref listOfErrors);
		float num = 0f;
		if (ItemStorageType != null)
		{
			num = ItemStorageType.GetTotalCapacity();
		}
		float num2 = 0f;
		if (PassengerOrCargoSlotTypes == null)
		{
			return;
		}
		for (int i = 0; i < PassengerOrCargoSlotTypes.Length; i++)
		{
			CargoSlotType cargoSlotType = PassengerOrCargoSlotTypes[i].CargoSlotType;
			if (cargoSlotType != null)
			{
				num2 += cargoSlotType.Capacity;
			}
		}
		if (num2 != num)
		{
			EntityType.CreateValidationError(ref listOfErrors, $"There is a mismatch between the total cargo slot capacity ({num2}) and the total item storage capacity ({num}). They should be equal. Remember that a passenger/cargo slot has the capacity '2'.");
		}
	}

	public bool CanUseRoute(RouteType? routeType, bool isAirRoute, double distance)
	{
		if (Aircraft != null && isAirRoute)
		{
			return true;
		}
		if (routeType.HasValue && CanNavigateRoutes != null && CanNavigateRoutes.Contains(routeType.Value))
		{
			return true;
		}
		return false;
	}
}
