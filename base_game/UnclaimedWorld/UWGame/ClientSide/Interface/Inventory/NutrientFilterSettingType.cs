using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;

namespace UWGame.ClientSide.Interface.Inventory;

public class NutrientFilterSettingType : FilterSettingType
{
	public string Nutrient;

	[XmlIgnore]
	public FoodNutrientType NutrientType;

	public float Limit;

	public string ConsumableByEntity;

	[XmlIgnore]
	public EntityType ConsumableByEntityType;

	public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			if ((filter != null && !filter(allItemType.Value)) || allItemType.Value.ItemType.FoodType == null || allItemType.Value.ItemType.FoodType.FoodNutrientProfile == null)
			{
				continue;
			}
			string[] foodItemTagsThatCanBeConsumed = ConsumableByEntityType.BiologicalType.FoodItemTagsThatCanBeConsumed;
			foreach (string value in foodItemTagsThatCanBeConsumed)
			{
				if (allItemType.Value.ItemType.FoodType.FoodTags == null || !allItemType.Value.ItemType.FoodType.FoodTags.Contains(value))
				{
					continue;
				}
				FoodNutrientAmount[] foodNutrientTypes = allItemType.Value.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes;
				foreach (FoodNutrientAmount foodNutrientAmount in foodNutrientTypes)
				{
					if (NutrientType == foodNutrientAmount.Nutrient && foodNutrientAmount.Amount >= Limit)
					{
						if (!hashSet.Contains(allItemType.Value))
						{
							hashSet.Add(allItemType.Value);
						}
						break;
					}
				}
			}
		}
		return hashSet;
	}

	public override void Initialize()
	{
		base.Initialize();
		NutrientType = GameData.Instance.AllFoodNutrientTypes[Nutrient];
		ConsumableByEntityType = GameData.Instance.AllEntityTypes[ConsumableByEntity];
	}

	public override string GetDefaultDisplayName()
	{
		return "High in " + NutrientType.Name;
	}
}
