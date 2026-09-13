using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Tiers;

namespace UWGame.ClientSide.Interface.Inventory;

public class TierFilterSettingType : FilterSettingType
{
	public string Tier;

	[XmlIgnore]
	public TierType TierType;

	public override string GetDefaultDisplayName()
	{
		return "Tier: " + TierType.Name;
	}

	public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		return (from kvp in GameData.Instance.AllEntityTypes
			where kvp.Value.TierOrAreaType != null && ((kvp.Value.TierOrAreaType.TierArea != null && kvp.Value.TierOrAreaType.TierArea.TierType == TierType) || (kvp.Value.TierOrAreaType.TierType != null && kvp.Value.TierOrAreaType.TierType == TierType)) && (filter == null || filter(kvp.Value))
			select kvp.Value).ToHashSet();
	}

	public override void Initialize()
	{
		base.Initialize();
		TierType = GameData.Instance.AllTierTypes[Tier];
	}
}
