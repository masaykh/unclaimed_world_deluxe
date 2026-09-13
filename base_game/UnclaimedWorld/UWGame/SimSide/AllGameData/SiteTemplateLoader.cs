using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Templates;

namespace UWGame.SimSide.AllGameData;

public class SiteTemplateLoader
{
	public static List<SiteTemplate> Init()
	{
		List<SiteTemplate> list = new List<SiteTemplate>();
		list.Add(new SiteTemplate
		{
			KeyName = "smallFarmingSite",
			SizeFactor = 0.8f,
			Names = new string[2] { "Batten Fields", "Cudale" },
			Description = "A small farming community which exports seasonal crops. Has an occasional need for tools or medical supplies.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "farmingAllegiance"
						}
					}
				}
			}
		});
		list.Add(new SiteTemplate
		{
			KeyName = "smallMiningSite",
			SizeFactor = 0.8f,
			Names = new string[4] { "Sulfur Lake", "Conlans Claim", "Breakneck", "Kooten Pass" },
			Description = "A small community which specializes in mineral extraction and refining. Imports some food.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "miningAllegiance"
						}
					}
				}
			}
		});
		list.Add(new SiteTemplate
		{
			KeyName = "smallFishingSite",
			SizeFactor = 0.8f,
			Names = new string[2] { "Riverbend", "Yellowwater" },
			Description = "A small fishing community. Imports some tools and spirits.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "fishingAllegiance"
						}
					}
				}
			}
		});
		list.Add(new SiteTemplate
		{
			KeyName = "smallAdvancedFarmingSite",
			SizeFactor = 1f,
			Names = new string[1] { "Haven" },
			Description = "A small commune of independent-minded people. The farmers here sell their crops and occasionally buy tools.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedFarmingAllegiance"
						}
					}
				}
			}
		});
		list.Add(new SiteTemplate
		{
			KeyName = "smallAdvancedFishingSite",
			SizeFactor = 1f,
			Names = new string[1] { "Clearbrook" },
			Description = "A small commune of independent-minded people, fishing the nearby waters. Imports tools and supplies.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedFishingAllegiance"
						}
					}
				}
			}
		});
		list.Add(new SiteTemplate
		{
			KeyName = "smallAdvancedMiningSite",
			SizeFactor = 1f,
			Names = new string[1] { "Rockfall" },
			Description = "A small commune of independent-minded people, producing hand-crafted items. Imports food and supplies.",
			Allegiances = new StringChanceSet[1]
			{
				new StringChanceSet
				{
					Chances = new StringChance[1]
					{
						new StringChance
						{
							Edge = 1f,
							String = "advancedMiningAllegiance"
						}
					}
				}
			}
		});
		return list;
	}
}
