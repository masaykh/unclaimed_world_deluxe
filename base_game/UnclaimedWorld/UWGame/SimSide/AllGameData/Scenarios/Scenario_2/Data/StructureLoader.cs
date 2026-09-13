using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

internal class StructureLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("structure:signalPyre")
		{
			DeleteRecord = true
		});
	}
}
