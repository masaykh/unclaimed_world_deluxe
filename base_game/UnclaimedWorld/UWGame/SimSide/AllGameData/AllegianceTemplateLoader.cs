using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Templates;

namespace UWGame.SimSide.AllGameData;

public class AllegianceTemplateLoader
{
	public static List<AllegianceTemplate> Init()
	{
		List<AllegianceTemplate> list = new List<AllegianceTemplate>();
		list.Add(new AllegianceTemplate
		{
			KeyName = "farmingAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "farmingProfileExpedition"
						}
					}
				}
			}
		});
		list.Add(new AllegianceTemplate
		{
			KeyName = "miningAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "miningProfileExpedition"
						}
					}
				}
			}
		});
		list.Add(new AllegianceTemplate
		{
			KeyName = "fishingAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "fishingProfileExpedition"
						}
					}
				}
			}
		});
		list.Add(new AllegianceTemplate
		{
			KeyName = "advancedFarmingAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedFarmingProfileExpedition"
						}
					}
				}
			}
		});
		list.Add(new AllegianceTemplate
		{
			KeyName = "advancedMiningAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedMiningProfileExpedition"
						}
					}
				}
			}
		});
		list.Add(new AllegianceTemplate
		{
			KeyName = "advancedFishingAllegianceTemplate",
			Expeditions = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedFishingProfileExpedition"
						}
					}
				}
			}
		});
		return list;
	}
}
