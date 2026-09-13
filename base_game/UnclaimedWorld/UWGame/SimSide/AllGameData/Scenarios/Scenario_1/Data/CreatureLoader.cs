using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

internal class CreatureLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("entity:weedingRobot")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:patrolRobot")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:gunDog")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:domesticatedTwinkler")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:robotSmall")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:haulingRobot")
		{
			DeleteRecord = true
		});
		listOfEntityTypes.Add(new EntityType("entity:weedingRobot")
		{
			DeleteRecord = true
		});
	}
}
