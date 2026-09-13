using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Inventory;

public class CategoryFilterSettingType : FilterSettingType
{
	public string EntityCategory;

	[XmlIgnore]
	public EntityCategory EntityCategoryType;

	public override string GetDefaultDisplayName()
	{
		return EntityCategoryType.Name;
	}

	public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		return (from kvp in GameData.Instance.AllEntityTypes
			where kvp.Value.Category == EntityCategoryType && (filter == null || filter(kvp.Value))
			select kvp.Value).ToHashSet();
	}

	public override void Initialize()
	{
		base.Initialize();
		EntityCategoryType = GameData.Instance.AllEntityCategories[EntityCategory];
	}
}
