using System.Collections.Generic;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class VehiclesProfileLoader
{
	public static List<VehiclesProfile> Init()
	{
		return new List<VehiclesProfile>
		{
			new VehiclesProfile
			{
				KeyName = "bargeProfile",
				VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType> { 
				{
					"entity:barge",
					new VehiclesForHireType
					{
						StartAmount = 1,
						Price = 10f,
						PricePerKilometer = 0.2f
					}
				} }
			},
			new VehiclesProfile
			{
				KeyName = "advancedBargeProfile",
				VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType> { 
				{
					"entity:advancedBarge",
					new VehiclesForHireType
					{
						StartAmount = 1,
						Price = 10f,
						PricePerKilometer = 0.2f
					}
				} }
			},
			new VehiclesProfile
			{
				KeyName = "airliftProfile",
				VehiclesForHire = new SerializableDictionary<string, VehiclesForHireType>
				{
					{
						"entity:skimmer",
						new VehiclesForHireType
						{
							StartAmount = 1,
							Price = 300f
						}
					},
					{
						"entity:harpy",
						new VehiclesForHireType
						{
							StartAmount = 1,
							Price = 500f
						}
					}
				}
			}
		};
	}
}
