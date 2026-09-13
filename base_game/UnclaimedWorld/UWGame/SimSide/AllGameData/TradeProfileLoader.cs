using System.Collections.Generic;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class TradeProfileLoader
{
	public static List<TradeProfile> Init()
	{
		List<TradeProfile> list = new List<TradeProfile>();
		list.Add(new TradeProfile
		{
			KeyName = "farmingTradeProfile",
			Comments = "has 1 random cash crop and 1 products export",
			HighExport = new string[1] { "foodCropsTrade" },
			RandomHighExport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[3] { "cottonTrade", "firewoodTrade", "peatTrade" }
			},
			RandomMediumExport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[4] { "thunderChickenTrade", "bakedFoodTrade", "comfortTrade", "animalsTrade" }
			},
			MediumImport = new string[1] { "farmToolsTrade" },
			LowImport = new string[2] { "toolsTrade", "textileTrade" },
			RandomLowImport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[2] { "hidesTrade", "turnipTrade" }
			}
		});
		list.Add(new TradeProfile
		{
			KeyName = "miningTradeProfile",
			HighExport = new string[1] { "mineralsTrade" },
			MediumExport = new string[1] { "metalsTrade" },
			LowExport = new string[1] { "toolsTrade" },
			MediumImport = new string[3] { "foodTrade", "comfortTrade", "textileTrade" }
		});
		list.Add(new TradeProfile
		{
			KeyName = "fishingTradeProfile",
			HighExport = new string[1] { "fishTrade" },
			MediumImport = new string[2] { "fishingEquipmentTrade", "comfortTrade" },
			LowImport = new string[1] { "toolsTrade" }
		});
		list.Add(new TradeProfile
		{
			KeyName = "advancedFarmingTradeProfile",
			Comments = "exports food, has 1 random cash crop and 1 products export. Also trades advanced goods, since these are supposed to be more common",
			HighExport = new string[1] { "foodCropsTrade" },
			MediumExport = new string[1] { "mediumWeaponsTrade" },
			RandomHighExport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[3] { "cottonTrade", "firewoodTrade", "peatTrade" }
			},
			RandomMediumExport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[4] { "thunderChickenTrade", "bakedFoodTrade", "comfortTrade", "animalsTrade" }
			},
			MediumImport = new string[1] { "farmToolsTrade" },
			LowImport = new string[2] { "toolsTrade", "textileTrade" },
			RandomLowImport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[2] { "hidesTrade", "turnipTrade" }
			},
			LowTrade = new string[7] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems", "occasionallyOfferedSeeds" },
			TradeGroupPriority = new SerializableDictionary<string, int>
			{
				{ "occasionalEquipmentTradeItems", 10 },
				{ "foodCropsTrade", 10 },
				{ "cottonTrade", 10 }
			}
		});
		list.Add(new TradeProfile
		{
			KeyName = "advancedFishingTradeProfile",
			HighExport = new string[1] { "fishTrade" },
			MediumExport = new string[1] { "mediumWeaponsTrade" },
			MediumImport = new string[2] { "fishingEquipmentTrade", "comfortTrade" },
			LowImport = new string[1] { "toolsTrade" },
			MediumTrade = new string[1] { "occasionallyOfferedSeeds" },
			LowTrade = new string[6] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems" },
			TradeGroupPriority = new SerializableDictionary<string, int> { { "occasionalEquipmentTradeItems", 10 } }
		});
		list.Add(new TradeProfile
		{
			KeyName = "advancedMiningTradeProfile",
			HighExport = new string[1] { "mineralsTrade" },
			MediumExport = new string[2] { "metalsTrade", "mediumWeaponsTrade" },
			LowExport = new string[1] { "toolsTrade" },
			MediumImport = new string[3] { "foodTrade", "comfortTrade", "textileTrade" },
			MediumTrade = new string[1] { "occasionallyOfferedSeeds" },
			LowTrade = new string[6] { "advancedToolsTrade", "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedEquipmentTrade", "advancedMaterialsTrade", "occasionalEquipmentTradeItems" },
			TradeGroupPriority = new SerializableDictionary<string, int> { { "occasionalEquipmentTradeItems", 10 } }
		});
		list.Add(new TradeProfile
		{
			KeyName = "industryTradeProfile",
			HighExport = new string[1] { "mineralsTrade" },
			MediumExport = new string[5] { "metalsTrade", "toolsTrade", "weaponsTrade", "industrialComponentsTrade", "textileTrade" },
			MediumImport = new string[3] { "foodTrade", "comfortTrade", "plasticsTrade" },
			RandomLowImport = new RandomTrade
			{
				NoOfGroups = 1,
				Options = new string[2] { "hidesTrade", "fuelTrade" }
			}
		});
		list.Add(new TradeProfile
		{
			KeyName = "bigTradingProfile",
			MediumTrade = new string[18]
			{
				"mineralsTrade", "metalsTrade", "foodTrade", "toolsTrade", "weaponsTrade", "hidesTrade", "cropsTrade", "fuelTrade", "textileTrade", "industrialComponentsTrade",
				"occasionalAdvancedTradeItems", "occasionalAdvancedAmmoTradeItems", "fishingEquipmentTrade", "trapsTrade", "electronicsTrade", "containersTrade", "plasticsTrade", "animalsTrade"
			}
		});
		list.Add(new TradeProfile
		{
			KeyName = "advancedTradingProfile",
			HighExport = new string[5] { "advancedHandWeaponsTrade", "advancedFoodTrade", "advancedToolsTrade", "advancedEquipmentTrade", "advancedMaterialsTrade" }
		});
		return list;
	}
}
