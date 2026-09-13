using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class EntityTypeDescriptionLoader
{
	public static List<EntityTypeDescription> Init()
	{
		return new List<EntityTypeDescription>
		{
			new EntityTypeDescription
			{
				KeyName = "personDescription",
				EntityType = "entity:human",
				EntityName = "Member of research team",
				SummaryDescription = "Member of the research team studying the muckroot biome",
				Description = ""
			}
		};
	}
}
