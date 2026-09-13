namespace UWGame.SimSide.Trade;

public class TradeAmountType
{
	public string EntityType;

	public string EntityDataKey;

	public int MaxAmountForSale;

	public int MaxAmountToBuy;

	public int? AmountToBuy;

	public float LinearIncreasePerDay;

	public float LinearConsumptionPerDay;

	public float? SellPrice;

	public float? BuyPrice;

	public string OfferDemandProfile;

	public NormalDistribution SpecificMaxAmount;

	public NormalDistribution SpecificIncreasePerDay;

	public NormalDistribution SpecificConsumptionPerDay;

	public NormalDistribution SpecificSellPrice;

	public NormalDistribution SpecificBuyPrice;

	public NormalDistribution StartAmount;

	public string GetEntityTypeKey()
	{
		if (EntityType != null)
		{
			EntityType.Contains("robot");
		}
		if (EntityDataKey != null)
		{
			return GameData.Instance.AllEntityData[EntityDataKey].EntityKey;
		}
		return EntityType;
	}
}
