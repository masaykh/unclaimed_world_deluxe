using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Trade;

public class TradeGroup : IGameData
{
	public string OfferDemandProfile;

	public TradeAmountType[] AvailableForTrade;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public string Comments { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		TradeAmountType[] availableForTrade = AvailableForTrade;
		foreach (TradeAmountType tradeAmountType in availableForTrade)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, tradeAmountType.GetEntityTypeKey(), GameData.Instance.AllEntityTypes, out var _);
		}
		if (OfferDemandProfile != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, OfferDemandProfile, GameData.Instance.AllOfferDemandProfiles, out var _);
		}
	}
}
