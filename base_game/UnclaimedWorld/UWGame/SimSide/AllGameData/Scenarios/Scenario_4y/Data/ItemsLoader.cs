using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4y.Data;

internal class ItemsLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("item:muleVehicleBody")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:wheelMotor")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:suspension")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:vehicleSeat")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:controlPanel")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:wheel")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:tyre")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:metalParts")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:cement")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("item:meshTest")
		{
			DeleteRecord = true
		});
	}
}
