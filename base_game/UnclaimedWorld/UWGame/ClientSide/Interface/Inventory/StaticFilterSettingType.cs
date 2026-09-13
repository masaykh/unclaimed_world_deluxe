using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.ClientSide.Interface.Inventory;

public class StaticFilterSettingType : FilterSettingType
{
	public StaticFilterSettings StaticFilterSetting;

	private string category = "weapons";

	public override string GetDefaultDisplayName()
	{
		return StaticFilterSetting switch
		{
			StaticFilterSettings.Containers => "Containers ", 
			StaticFilterSettings.Storage => "Storage ", 
			StaticFilterSettings.Fuel => "Fuel", 
			StaticFilterSettings.BetterTools => "Better tools", 
			StaticFilterSettings.Structures => "Structures", 
			StaticFilterSettings.Items => "Items", 
			StaticFilterSettings.UsableAsWeapon => "Usable as weapon", 
			StaticFilterSettings.AffectsComfortRating => "Affects comfort rating", 
			StaticFilterSettings.AffectsSecurityRating => "Affects security rating", 
			StaticFilterSettings.AffectsFoodRating => "Affects food rating", 
			_ => "No display string for:" + StaticFilterSetting, 
		};
	}

	public override HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		return StaticFilterSetting switch
		{
			StaticFilterSettings.Structures => GetEntityTypes(GameData.Instance.AllStructureTypes, filter), 
			StaticFilterSettings.Items => GetEntityTypes(GameData.Instance.AllItemTypes, filter), 
			StaticFilterSettings.Containers => GetContainersAndStorages(GameData.Instance.AllItemTypes, filter), 
			StaticFilterSettings.Storage => GetContainersAndStorages(GameData.Instance.AllStructureTypes, filter), 
			StaticFilterSettings.Fuel => GetFuelsAndFertilizers(filter), 
			StaticFilterSettings.UsableAsWeapon => GetItemsUseableAsWeapon(filter), 
			StaticFilterSettings.BetterTools => GetBetterTools(filter), 
			StaticFilterSettings.AffectsFoodRating => GetFoodRatingTypes(filter), 
			StaticFilterSettings.AffectsComfortRating => GetComfortRatingTypes(filter), 
			StaticFilterSettings.AffectsSecurityRating => GetSecurityRatingTypes(filter), 
			_ => null, 
		};
	}

	private HashSet<EntityType> GetEntityTypes(Dictionary<string, EntityType> collection, Predicate<EntityType> filter)
	{
		if (filter == null)
		{
			return collection.Values.ToHashSet();
		}
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> item in collection)
		{
			if (filter == null || filter(item.Value))
			{
				hashSet.Add(item.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetItemsUseableAsWeapon(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			if ((filter == null || filter(allItemType.Value)) && allItemType.Value.ItemType.WeaponType != null && allItemType.Value.Category != GameData.Instance.AllEntityCategories[category] && allItemType.Value.ItemType.WeaponType.IsIntrinsic != true)
			{
				hashSet.Add(allItemType.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetContainersAndStorages(Dictionary<string, EntityType> collection, Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> item in collection)
		{
			if ((filter == null || filter(item.Value)) && item.Value.ContainerType != null && (item.Value.StructureType != null || item.Value.ItemType == null || item.Value.ItemType.WeaponType == null))
			{
				hashSet.Add(item.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetFuelsAndFertilizers(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			if ((filter == null || filter(allItemType.Value)) && allItemType.Value.ItemType.FuelType != null)
			{
				hashSet.Add(allItemType.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetFoodRatingTypes(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> allItemType in GameData.Instance.AllItemTypes)
		{
			if ((filter == null || filter(allItemType.Value)) && FoodStatisticsForAllegiance.AffectsFoodRating(The.InGameUI.UIAllegiance, allItemType.Value))
			{
				hashSet.Add(allItemType.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetSecurityRatingTypes(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			if ((filter == null || filter(allEntityType.Value)) && SecurityStatisticsForAllegiance.AffectsSecurityRating(allEntityType.Value))
			{
				hashSet.Add(allEntityType.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetComfortRatingTypes(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		_ = The.InGameUI.UIAllegiance.RepresentativeEntityType;
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			if ((filter == null || filter(allEntityType.Value)) && ComfortStatisticsForAllegiance.AffectsComfortRating(The.InGameUI.UIAllegiance, allEntityType.Value))
			{
				hashSet.Add(allEntityType.Value);
			}
		}
		return hashSet;
	}

	private HashSet<EntityType> GetBetterTools(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		foreach (KeyValuePair<string, ProcessToolSet> allProcessToolSet in GameData.Instance.AllProcessToolSets)
		{
			if (allProcessToolSet.Value.Tools.Length == 0)
			{
				continue;
			}
			ToolAlternatives[] tools = allProcessToolSet.Value.Tools;
			foreach (ToolAlternatives toolAlternatives in tools)
			{
				if (toolAlternatives.Tools.Length == 0)
				{
					continue;
				}
				Tool[] tools2 = toolAlternatives.Tools;
				foreach (Tool tool in tools2)
				{
					if (!(tool.ProductivityFactor > GameData.Instance.GUIConstants.BetterToolsFilterLimit))
					{
						continue;
					}
					foreach (EntityType toolEntityType in tool.ToolEntityTypes)
					{
						if ((filter == null || filter(toolEntityType)) && toolEntityType.StructureType == null)
						{
							hashSet.Add(toolEntityType);
						}
					}
				}
			}
		}
		return hashSet;
	}
}
