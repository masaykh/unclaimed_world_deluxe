using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData;

public class AllegianceDataLoader
{
	public static List<AllegianceData> Init()
	{
		List<AllegianceData> list = new List<AllegianceData>();
		list.Add(new AllegianceData
		{
			KeyName = "farmingAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "farmingAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			KeyName = "miningAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "miningAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			KeyName = "fishingAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.15f,
					Max = 0.35f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "fishingAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			KeyName = "advancedFarmingAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.55f,
					Max = 0.95f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.35f,
					Max = 0.55f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.65f,
					Max = 0.85f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "advancedFarmingAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			KeyName = "advancedMiningAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.55f,
					Max = 0.95f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.35f,
					Max = 0.55f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.65f,
					Max = 0.85f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "advancedMiningAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			KeyName = "advancedFishingAllegiance",
			AllegianceType = AllegianceType.Other,
			EntityType = "entity:human",
			StatsData = new StatsData
			{
				RandomComfort = new NormalDistribution
				{
					Min = 0.55f,
					Max = 0.95f
				},
				RandomFood = new NormalDistribution
				{
					Min = 0.35f,
					Max = 0.55f
				},
				RandomSecurity = new NormalDistribution
				{
					Min = 0.65f,
					Max = 0.85f
				}
			},
			AllegianceTemplates = new StringChance[1]
			{
				new StringChance
				{
					Edge = 1f,
					String = "advancedFishingAllegianceTemplate"
				}
			}
		});
		list.Add(new AllegianceData
		{
			Name = "Starsnare Point",
			KeyName = "destinyRiverDeltaDescentAllegiance",
			EntityType = "entity:human",
			AllegianceType = AllegianceType.Other,
			StatsData = new StatsData
			{
				Security = 0.49f,
				Comfort = 0.43f,
				FoodSupply = 0.32f
			}
		});
		return list;
	}
}
