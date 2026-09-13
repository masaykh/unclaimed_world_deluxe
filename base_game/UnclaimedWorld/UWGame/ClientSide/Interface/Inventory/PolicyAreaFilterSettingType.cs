using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Inventory;

public class PolicyAreaFilterSettingType : FilterSettingType
{
	public RatingTypes RatingType;

	public override string GetDefaultDisplayName()
	{
		return "Policy area: " + Statistic.RatingsTypeToString(RatingType);
	}

	public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		return (from kvp in GameData.Instance.AllEntityTypes
			where kvp.Value.TierOrAreaType != null && kvp.Value.TierOrAreaType.TierArea != null && kvp.Value.TierOrAreaType.TierArea.Area == RatingType && (filter == null || filter(kvp.Value))
			select kvp.Value).ToHashSet();
	}

	public override void Initialize()
	{
		base.Initialize();
	}
}
