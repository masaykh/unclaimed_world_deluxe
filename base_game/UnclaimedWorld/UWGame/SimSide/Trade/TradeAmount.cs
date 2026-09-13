using System;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trade;

public class TradeAmount : ISnapshot
{
	public EntityData EntityData;

	public int? StartAmount;

	public int? MaxAmountForSale;

	public int? MaxAmountToBuy;

	public float Progress;

	public float IncreasePerDay;

	public float ConsumptionPerDay;

	public OfferDemandProfile OfferDemandProfile;

	public float OfferDemandProfileScaleFactor;

	public SimplexNoise OfferedForTradeNoise;

	public float? SellPrice;

	public float? BuyPrice;

	public float TimeInDaysElapsedSinceItemConsumed;

	public int AmountToBuy;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public TradeAmount()
	{
	}

	public TradeAmount(string entityType, TradeAmountType amountType, float scaleAmounts, OfferDemandProfile offerDemandProfile, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
	{
		if (amountType.EntityDataKey != null)
		{
			EntityData = GameData.Instance.AllEntityData[amountType.EntityDataKey];
		}
		if (allowProduction)
		{
			IncreasePerDay = amountType.LinearIncreasePerDay * scaleAmounts;
			MaxAmountForSale = (int)((float)amountType.MaxAmountForSale * scaleAmounts);
		}
		if (allowDemand)
		{
			ConsumptionPerDay = amountType.LinearConsumptionPerDay * scaleAmounts;
			MaxAmountToBuy = (int)((float)amountType.MaxAmountToBuy * scaleAmounts);
		}
		if (offerDemandProfile != null)
		{
			OfferDemandProfile = offerDemandProfile;
			OfferDemandProfileScaleFactor = scaleAmounts;
			if (offerDemandProfile.NonlinearAmountForSale != null)
			{
				OfferedForTradeNoise = new SimplexNoise();
			}
		}
		SellPrice = amountType.SellPrice;
		BuyPrice = amountType.BuyPrice;
		if (amountType.SpecificSellPrice != null)
		{
			SellPrice = (float)amountType.SpecificSellPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
		}
		if (amountType.SpecificBuyPrice != null)
		{
			BuyPrice = (float)amountType.SpecificBuyPrice.GetRandomValue(The.Sim.GameplayRandomGenerator);
		}
		if (pricesProfile != null)
		{
			int? maxAmountForSale = MaxAmountForSale;
			int num = 0;
			if (maxAmountForSale.GetValueOrDefault() > num && maxAmountForSale.HasValue && !SellPrice.HasValue && pricesProfile.Prices.TryGetValue(entityType, out var value))
			{
				SellPrice = GameData.Instance.Constants.SellPriceModifier * value;
			}
			maxAmountForSale = MaxAmountToBuy;
			num = 0;
			if (maxAmountForSale.GetValueOrDefault() > num && maxAmountForSale.HasValue && !BuyPrice.HasValue)
			{
				if (!pricesProfile.Prices.TryGetValue(entityType, out value))
				{
					throw new Exception("Price not found: " + entityType);
				}
				BuyPrice = value;
			}
		}
		if (amountType.SpecificIncreasePerDay != null)
		{
			IncreasePerDay = amountType.SpecificIncreasePerDay.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
		}
		if (amountType.SpecificMaxAmount != null)
		{
			MaxAmountForSale = amountType.SpecificMaxAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
		}
		if (amountType.StartAmount != null)
		{
			StartAmount = amountType.StartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator);
		}
		if (amountType.AmountToBuy.HasValue)
		{
			AmountToBuy = amountType.AmountToBuy.Value;
		}
		else if (MaxAmountToBuy > 0)
		{
			AmountToBuy = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0, MaxAmountToBuy.Value + 1);
		}
		else
		{
			AmountToBuy = 0;
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		StartAmount = sn.DoInt32Nullable(StartAmount);
		MaxAmountForSale = sn.DoInt32Nullable(MaxAmountForSale);
		MaxAmountToBuy = sn.DoInt32Nullable(MaxAmountToBuy);
		IncreasePerDay = sn.DoFloat(IncreasePerDay);
		ConsumptionPerDay = sn.DoFloat(ConsumptionPerDay);
		SellPrice = sn.DoFloatNullable(SellPrice);
		BuyPrice = sn.DoFloatNullable(BuyPrice);
		Progress = sn.DoFloat(Progress);
		TimeInDaysElapsedSinceItemConsumed = sn.DoFloat(TimeInDaysElapsedSinceItemConsumed);
		AmountToBuy = sn.DoInt32(AmountToBuy);
		OfferDemandProfile = sn.DoGameData(OfferDemandProfile);
		OfferDemandProfileScaleFactor = sn.DoFloat(OfferDemandProfileScaleFactor);
		OfferedForTradeNoise = (SimplexNoise)sn.DoISnapshot(OfferedForTradeNoise);
		EntityData = sn.DoGameData(EntityData);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (OfferedForTradeNoise != null)
		{
			OfferedForTradeNoise.LoadPostProcess(sn);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
