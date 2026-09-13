using System.Collections.Generic;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.AllGameData;

public class ExpeditionDataLoader
{
	public static List<ExpeditionData> Init()
	{
		return new List<ExpeditionData>
		{
			new ExpeditionData
			{
				KeyName = "farmingProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "farmingTradeProfile",
				StructuresProfile = "mediumPierProfile",
				VehiclesProfile = "bargeProfile",
				PricesProfile = "descentEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "miningProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "miningTradeProfile",
				StructuresProfile = "mediumPierProfile",
				VehiclesProfile = "bargeProfile",
				PricesProfile = "descentEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "fishingProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "fishingTradeProfile",
				StructuresProfile = "mediumPierProfile",
				VehiclesProfile = "bargeProfile",
				PricesProfile = "descentEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "advancedFarmingProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "advancedFarmingTradeProfile",
				StructuresProfile = "mediumPierSatelliteProfile",
				VehiclesProfile = "advancedBargeProfile",
				PricesProfile = "planetFallEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "advancedMiningProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "advancedMiningTradeProfile",
				StructuresProfile = "mediumPierSatelliteProfile",
				VehiclesProfile = "advancedBargeProfile",
				PricesProfile = "planetFallEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "advancedFishingProfileExpedition",
				Name = "The Wharf",
				TradeProfile = "advancedFishingTradeProfile",
				StructuresProfile = "mediumPierSatelliteProfile",
				VehiclesProfile = "advancedBargeProfile",
				PricesProfile = "planetFallEraPrices"
			},
			new ExpeditionData
			{
				KeyName = "destinyRiverDeltaDescentExpedition",
				Name = "The Wharf",
				SizeFactor = 2.5f,
				TradeProfile = "bigTradingProfile",
				StructuresProfile = "largePierProfile",
				VehiclesProfile = "bargeProfile",
				PricesProfile = "descentEraPrices"
			}
		};
	}
}
