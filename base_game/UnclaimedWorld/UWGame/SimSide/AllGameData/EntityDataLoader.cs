using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData;

public class EntityDataLoader
{
	public static List<EntityData> Init()
	{
		List<EntityData> list = new List<EntityData>();
		list.Add(new EntityData
		{
			KeyName = "dog",
			EntityKey = "entity:dog",
			BioEntity = new BiologicalEntity
			{
				AgeInYears = new NormalDistribution
				{
					Mean = 4.0
				},
				CultureTemplates = new StringChance[1]
				{
					new StringChance
					{
						Edge = 1f,
						String = "dogCulture"
					}
				}
			}
		});
		return list;
	}
}
