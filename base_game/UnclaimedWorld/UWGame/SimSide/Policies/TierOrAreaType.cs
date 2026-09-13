using System.Linq;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.Policies;

public class TierOrAreaType
{
	public TierArea TierArea;

	public TierType TierType;

	public string Icon
	{
		get
		{
			if (TierType != null)
			{
				return TierType.Icon;
			}
			return TierArea.Icon;
		}
	}

	public string Description
	{
		get
		{
			if (TierType != null)
			{
				return TierType.Description;
			}
			return TierArea.Description;
		}
	}

	public TierOrAreaType(TierOrArea tierArea)
	{
		if (tierArea.Area.HasValue)
		{
			TierArea = GameData.Instance.AllTierAreas.Values.FirstOrDefault((TierArea t) => t.Tier == tierArea.Tier && t.Area == tierArea.Area);
		}
		else if (tierArea.Tier != null)
		{
			TierType = GameData.Instance.AllTierTypes[tierArea.Tier];
		}
	}

	public TierType GetTier()
	{
		if (TierType != null)
		{
			return TierType;
		}
		return TierArea.TierType;
	}

	public string GetNotAvailableTooltip()
	{
		return "The following policy needs to be enacted first: " + ToString();
	}

	public override string ToString()
	{
		if (TierArea != null)
		{
			return TierArea.ToString();
		}
		return TierType.Name + " tier (any area)";
	}
}
