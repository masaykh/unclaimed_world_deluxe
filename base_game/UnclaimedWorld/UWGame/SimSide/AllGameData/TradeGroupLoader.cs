using System.Collections.Generic;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData;

public class TradeGroupLoader
{
	public static List<TradeGroup> Init()
	{
		List<TradeGroup> list = new List<TradeGroup>();
		list.Add(new TradeGroup
		{
			KeyName = "foodCropsTrade",
			AvailableForTrade = new TradeAmountType[3]
			{
				new TradeAmountType
				{
					EntityType = "item:glassyCreeperPods",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 30,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 30
				},
				new TradeAmountType
				{
					EntityType = "item:crystalBerries",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 30,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 30
				},
				new TradeAmountType
				{
					EntityType = "item:fingerFruit",
					LinearIncreasePerDay = 4f,
					MaxAmountForSale = 18,
					LinearConsumptionPerDay = 4f,
					MaxAmountToBuy = 18
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "cottonTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:cotton",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 25,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 25
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "firewoodTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:firewood",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 20,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "peatTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:dryPeat",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 20,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "bakedFoodTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:hardtack",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 25,
					LinearConsumptionPerDay = 5f,
					MaxAmountToBuy = 25
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "farmToolsTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:steelHoe",
					LinearIncreasePerDay = 1f,
					MaxAmountForSale = 5,
					LinearConsumptionPerDay = 1f,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelSpade",
					LinearIncreasePerDay = 1f,
					MaxAmountForSale = 3,
					LinearConsumptionPerDay = 0.5f,
					MaxAmountToBuy = 3
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "toolsTrade",
			AvailableForTrade = new TradeAmountType[21]
			{
				new TradeAmountType
				{
					EntityType = "item:anvil",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:barClamps",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:steelHoe",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelSpade",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelPickaxe",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelMachete",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelHandAxe",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:hammer",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:file",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:handDrill",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:tongs",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:hacksaw",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:blacksmithsToolbox",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:metalWorkersToolbox",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:turnipCracker",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:steelKnife",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:metalWire",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:rawhideString",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:cottonString",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:bulletMold",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:shadeleafResin",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "weaponsTrade",
			AvailableForTrade = new TradeAmountType[8]
			{
				new TradeAmountType
				{
					EntityType = "item:boltActionRifle",
					LinearIncreasePerDay = 0.4f,
					LinearConsumptionPerDay = 0.3f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:gunpowderRifle",
					LinearIncreasePerDay = 0.7f,
					LinearConsumptionPerDay = 0.5f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:musket",
					LinearIncreasePerDay = 0.7f,
					LinearConsumptionPerDay = 0.5f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:musketoon",
					LinearIncreasePerDay = 0.7f,
					LinearConsumptionPerDay = 0.5f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:corditeAmmo",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:blackPowderRifleAmmo",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:blackPowderShotAmmo",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:ironArrow",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 6,
					MaxAmountToBuy = 6
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "mediumWeaponsTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:boltActionRifle",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 10,
					MaxAmountToBuy = 10
				},
				new TradeAmountType
				{
					EntityType = "item:corditeAmmo",
					LinearIncreasePerDay = 8f,
					LinearConsumptionPerDay = 8f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "advancedHandWeaponsTrade",
			AvailableForTrade = new TradeAmountType[4]
			{
				new TradeAmountType
				{
					EntityType = "item:coilRifle",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:shotgun",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:coilRifleAmmo",
					LinearIncreasePerDay = 10f,
					LinearConsumptionPerDay = 10f,
					MaxAmountForSale = 35,
					MaxAmountToBuy = 35
				},
				new TradeAmountType
				{
					EntityType = "item:shotgunAmmo",
					LinearIncreasePerDay = 7f,
					LinearConsumptionPerDay = 7f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "rareMetalsTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:scandium",
					LinearIncreasePerDay = 0f,
					LinearConsumptionPerDay = 0f,
					MaxAmountForSale = 40,
					MaxAmountToBuy = 40
				},
				new TradeAmountType
				{
					EntityType = "item:terbium",
					LinearIncreasePerDay = 0f,
					LinearConsumptionPerDay = 0f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "advancedFoodTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:astroRation",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 50,
					MaxAmountToBuy = 50
				},
				new TradeAmountType
				{
					EntityType = "item:simCoffeeBeans",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 50,
					MaxAmountToBuy = 50
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "advancedToolsTrade",
			AvailableForTrade = new TradeAmountType[5]
			{
				new TradeAmountType
				{
					EntityType = "item:advancedKnife",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:advancedString",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:advancedMachete",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 6,
					MaxAmountToBuy = 6
				},
				new TradeAmountType
				{
					EntityType = "item:advancedSnips",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 6,
					MaxAmountToBuy = 6
				},
				new TradeAmountType
				{
					EntityType = "item:advancedCookingPot",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 6,
					MaxAmountToBuy = 6
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "advancedEquipmentTrade",
			AvailableForTrade = new TradeAmountType[14]
			{
				new TradeAmountType
				{
					EntityType = "item:fieldLabPacked",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:groundScanner",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:cloak",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:nightVisionGoggles",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:sensor",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 6,
					MaxAmountToBuy = 6
				},
				new TradeAmountType
				{
					EntityType = "item:satelliteGroundStation",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:octagonalTent",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:smallTent",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:domeTent",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:thermalTarp",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 10,
					MaxAmountToBuy = 10
				},
				new TradeAmountType
				{
					EntityType = "item:diamondGlass",
					LinearIncreasePerDay = 3f,
					LinearConsumptionPerDay = 3f,
					MaxAmountForSale = 12,
					MaxAmountToBuy = 12
				},
				new TradeAmountType
				{
					EntityType = "item:fieldKitchenStove",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:fieldKitchenEquipment",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "entity:haulingRobot",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "advancedMaterialsTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:liquidGas",
					LinearIncreasePerDay = 8f,
					LinearConsumptionPerDay = 8f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "electronicsTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:radio",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:radioAntenna",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "trapsTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:spikeTrap",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "fishingEquipmentTrade",
			AvailableForTrade = new TradeAmountType[5]
			{
				new TradeAmountType
				{
					EntityType = "item:strongBugNet",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 9,
					MaxAmountToBuy = 9
				},
				new TradeAmountType
				{
					EntityType = "item:fishTrapHoopNet",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:fishTrapBasket",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:fishingNet",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:ironHooks",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "foodTrade",
			AvailableForTrade = new TradeAmountType[18]
			{
				new TradeAmountType
				{
					EntityType = "item:hardtack",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:fermentedFingerFruit",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:driedBeef",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:waterCaneSeeds",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:smokedTurnip",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:turnipSalami",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				},
				new TradeAmountType
				{
					EntityType = "item:smokedThunderChicken",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:driedThunderChicken",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:pickledAlabasterRay",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 15
				},
				new TradeAmountType
				{
					EntityType = "item:smokedAlabasterRay",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:smokedStreakFin",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:driedSaltedStreakFin",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:smokedCarbonTail",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:pickledCarbonTail",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				},
				new TradeAmountType
				{
					EntityType = "item:commonOilTubers",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:glassyCreeperPods",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:crystalBerries",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 18,
					MaxAmountToBuy = 18
				},
				new TradeAmountType
				{
					EntityType = "item:powderedCrystalBerries",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 18,
					MaxAmountToBuy = 18
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "bakedTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:hardtack",
					LinearIncreasePerDay = 5f,
					MaxAmountForSale = 25
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "thunderChickenTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:smokedThunderChicken",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:driedThunderChicken",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "turnipTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:smokedTurnip",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:turnipSalami",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "textileTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityType = "item:textile",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 10,
					MaxAmountToBuy = 10
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "hidesTrade",
			AvailableForTrade = new TradeAmountType[7]
			{
				new TradeAmountType
				{
					EntityType = "item:megapodRawhide",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:megapodTannedHide",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:thunderChickenRawhide",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 10,
					MaxAmountToBuy = 10
				},
				new TradeAmountType
				{
					EntityType = "item:thunderChickenTannedHide",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 10,
					MaxAmountToBuy = 10
				},
				new TradeAmountType
				{
					EntityType = "item:whipjawRawhide",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:whipjawTannedHide",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:improvisedGreenHouseCover",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 2,
					MaxAmountToBuy = 5
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "cropsTrade",
			AvailableForTrade = new TradeAmountType[4]
			{
				new TradeAmountType
				{
					EntityType = "item:cotton",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:glassyCreeperPods",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:crystalBerries",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 18,
					MaxAmountToBuy = 18
				},
				new TradeAmountType
				{
					EntityType = "item:fingerFruit",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 18,
					MaxAmountToBuy = 18
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "fishTrade",
			AvailableForTrade = new TradeAmountType[6]
			{
				new TradeAmountType
				{
					EntityType = "item:pickledAlabasterRay",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 15
				},
				new TradeAmountType
				{
					EntityType = "item:smokedAlabasterRay",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:smokedStreakFin",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:driedSaltedStreakFin",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:smokedCarbonTail",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 25,
					MaxAmountToBuy = 25
				},
				new TradeAmountType
				{
					EntityType = "item:pickledCarbonTail",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "mineralsTrade",
			AvailableForTrade = new TradeAmountType[5]
			{
				new TradeAmountType
				{
					EntityType = "item:bogOre",
					LinearIncreasePerDay = 10f,
					LinearConsumptionPerDay = 10f,
					MaxAmountForSale = 40,
					MaxAmountToBuy = 40
				},
				new TradeAmountType
				{
					EntityType = "item:goldOre",
					LinearIncreasePerDay = 10f,
					LinearConsumptionPerDay = 10f,
					MaxAmountForSale = 40,
					MaxAmountToBuy = 40
				},
				new TradeAmountType
				{
					EntityType = "item:sulfurPowder",
					LinearIncreasePerDay = 6f,
					LinearConsumptionPerDay = 6f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:salt",
					LinearIncreasePerDay = 6f,
					LinearConsumptionPerDay = 6f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:saltpeter",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 12,
					MaxAmountToBuy = 12
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "metalsTrade",
			AvailableForTrade = new TradeAmountType[4]
			{
				new TradeAmountType
				{
					EntityType = "item:roughBloomIron",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:wroughtIron",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:blisterSteel",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 14,
					MaxAmountToBuy = 14
				},
				new TradeAmountType
				{
					EntityType = "item:gold",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "fuelTrade",
			AvailableForTrade = new TradeAmountType[3]
			{
				new TradeAmountType
				{
					EntityType = "item:firewood",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:dryPeat",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:charcoal",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "plasticsTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:marshcotSap",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:gaskets",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "animalsTrade",
			AvailableForTrade = new TradeAmountType[1]
			{
				new TradeAmountType
				{
					EntityDataKey = "dog",
					LinearIncreasePerDay = 2f,
					LinearConsumptionPerDay = 2f,
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "industrialComponentsTrade",
			AvailableForTrade = new TradeAmountType[6]
			{
				new TradeAmountType
				{
					EntityType = "item:loomComponents",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:stillComponents",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:extrusionMachineComponents",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:humanPowerUnit",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:humanPowerUnitComponents",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:metalLatheComponents",
					LinearIncreasePerDay = 1f,
					LinearConsumptionPerDay = 1f,
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "containersTrade",
			AvailableForTrade = new TradeAmountType[4]
			{
				new TradeAmountType
				{
					EntityType = "item:clayJar",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:plasticTappingBucket",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:tappingBucket",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:goldPot",
					LinearIncreasePerDay = 4f,
					LinearConsumptionPerDay = 4f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "comfortTrade",
			AvailableForTrade = new TradeAmountType[2]
			{
				new TradeAmountType
				{
					EntityType = "item:crystalWine",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 20,
					MaxAmountToBuy = 20
				},
				new TradeAmountType
				{
					EntityType = "item:crystalBrandy",
					LinearIncreasePerDay = 5f,
					LinearConsumptionPerDay = 5f,
					MaxAmountForSale = 15,
					MaxAmountToBuy = 15
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "occasionalAdvancedTradeItems",
			OfferDemandProfile = "occasionallyOfferedGood",
			AvailableForTrade = new TradeAmountType[10]
			{
				new TradeAmountType
				{
					EntityType = "item:fieldLabPacked",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:coilRifle",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:sentry",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "entity:haulingRobot",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:shotgun",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:cloak",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:nightVisionGoggles",
					MaxAmountForSale = 2,
					MaxAmountToBuy = 2
				},
				new TradeAmountType
				{
					EntityType = "item:sensor",
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:advancedKnife",
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				},
				new TradeAmountType
				{
					EntityType = "item:advancedString",
					MaxAmountForSale = 3,
					MaxAmountToBuy = 3
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "occasionalAdvancedAmmoTradeItems",
			OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity",
			AvailableForTrade = new TradeAmountType[3]
			{
				new TradeAmountType
				{
					EntityType = "item:coilRifleAmmo",
					MaxAmountForSale = 8,
					MaxAmountToBuy = 6
				},
				new TradeAmountType
				{
					EntityType = "item:sentryGunAmmo",
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				},
				new TradeAmountType
				{
					EntityType = "item:shotgunAmmo",
					MaxAmountForSale = 5,
					MaxAmountToBuy = 5
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "occasionalEquipmentTradeItems",
			Comments = "in planet fall era, medium/basic tier industrial items should be rare (this is opposite from descent era)",
			OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity",
			AvailableForTrade = new TradeAmountType[8]
			{
				new TradeAmountType
				{
					EntityType = "item:loomComponents",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:stillComponents",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:extrusionMachineComponents",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:humanPowerUnit",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:humanPowerUnitComponents",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:metalLatheComponents",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:bellows",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				},
				new TradeAmountType
				{
					EntityType = "item:turnipCracker",
					MaxAmountForSale = 4,
					MaxAmountToBuy = 4
				}
			}
		});
		list.Add(new TradeGroup
		{
			KeyName = "occasionallyOfferedSeeds",
			OfferDemandProfile = "occasionallyOfferedGoodHigherQuantity",
			AvailableForTrade = new TradeAmountType[4]
			{
				new TradeAmountType
				{
					EntityType = "item:cotton",
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:glassyCreeperPods",
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:crystalBerries",
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				},
				new TradeAmountType
				{
					EntityType = "item:fingerFruit",
					MaxAmountForSale = 8,
					MaxAmountToBuy = 8
				}
			}
		});
		return list;
	}
}
