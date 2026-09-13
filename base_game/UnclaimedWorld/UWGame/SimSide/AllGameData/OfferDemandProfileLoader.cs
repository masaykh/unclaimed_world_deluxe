using System.Collections.Generic;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData;

public class OfferDemandProfileLoader
{
	public static List<OfferDemandProfile> Init()
	{
		return new List<OfferDemandProfile>
		{
			new OfferDemandProfile
			{
				KeyName = "occasionallyOfferedGood",
				NonlinearAmountForSale = new NoiseParams
				{
					NoiseAmplitude = 2f,
					NoiseAddend = -0.25f,
					NoiseFrequency = 0.005f
				}
			},
			new OfferDemandProfile
			{
				KeyName = "occasionallyOfferedGoodHigherQuantity",
				NonlinearAmountForSale = new NoiseParams
				{
					NoiseAmplitude = 4f,
					NoiseAddend = -0.25f,
					NoiseFrequency = 0.005f
				}
			}
		};
	}
}
