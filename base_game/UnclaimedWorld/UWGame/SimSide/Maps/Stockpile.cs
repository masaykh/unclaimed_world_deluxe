using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class Stockpile : ISnapshot
{
	public enum TypesOfStockpiles
	{
		Normal,
		OfferedForTrade
	}

	private TypesOfStockpiles stockpileType;

	private Dictionary<EntityCategory, bool> mayStockpileCategory = new Dictionary<EntityCategory, bool>();

	private Dictionary<EntityType, int> mayStockpileItem = new Dictionary<EntityType, int>();

	private bool limitsAreDirty = true;

	private Dictionary<EntityType, int> itemsWithLimits = new Dictionary<EntityType, int>();

	private Dictionary<EntityCategory, List<EntityType>> categoryItemSettings = new Dictionary<EntityCategory, List<EntityType>>();

	private DefaultStorageSettings defaultSettings;

	private bool settingsAreDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Stockpile()
	{
	}

	public Stockpile(TypesOfStockpiles typeOfStockpile, DefaultStorageSettings defaultSettings)
	{
		stockpileType = typeOfStockpile;
		Init(defaultSettings);
	}

	public Stockpile(TypesOfStockpiles typeOfStockpile, DefaultStorageSettings defaultSettings, Dictionary<EntityCategory, bool> mayStockpileCategory, Dictionary<EntityType, int> mayStockpileItem)
	{
		stockpileType = typeOfStockpile;
		this.mayStockpileCategory = mayStockpileCategory;
		this.mayStockpileItem = mayStockpileItem;
		Init(defaultSettings);
	}

	private void Init(DefaultStorageSettings defaultSettings)
	{
		this.defaultSettings = defaultSettings;
		MergeWithDefaultSettings();
		settingsAreDirty = true;
		limitsAreDirty = true;
	}

	public Dictionary<EntityType, int> GetMaxLimits()
	{
		if (limitsAreDirty)
		{
			itemsWithLimits.Clear();
			foreach (KeyValuePair<EntityType, int> item in mayStockpileItem)
			{
				if (!HasNoLimit(item.Value))
				{
					itemsWithLimits.Add(item.Key, item.Value);
				}
			}
			limitsAreDirty = false;
		}
		return itemsWithLimits;
	}

	public static bool HasNoLimit(int limit)
	{
		return limit == -1;
	}

	public bool CategoryHasDifferentItemSetting(bool categorySetting, EntityCategory category, List<EntityType> exclusionList)
	{
		if (settingsAreDirty)
		{
			RecomputeCategoryHasItemSettings();
		}
		if (categoryItemSettings.TryGetValue(category, out var value))
		{
			return value.Exists((EntityType e) => (exclusionList == null || !exclusionList.Contains(e)) && ItemSettingDiffers(mayStockpileItem, e, categorySetting));
		}
		return false;
	}

	public static bool ItemSettingDiffers(Dictionary<EntityType, int> mayStockpileItem, EntityType entityType, bool categorySetting)
	{
		if (mayStockpileItem.TryGetValue(entityType, out var value))
		{
			if (categorySetting)
			{
				return value == 0;
			}
			return value != 0;
		}
		return false;
	}

	private void RecomputeCategoryHasItemSettings()
	{
		categoryItemSettings.Clear();
		foreach (KeyValuePair<EntityType, int> item in mayStockpileItem)
		{
			Common.AddToMultiList(categoryItemSettings, item.Key.Category, item.Key);
		}
		settingsAreDirty = false;
	}

	public static bool GetAllowBaseSetting(TypesOfStockpiles typeOfStockpile, out int limit)
	{
		if (typeOfStockpile == TypesOfStockpiles.Normal)
		{
			limit = -1;
			return true;
		}
		limit = 0;
		return false;
	}

	public bool GetAllowBaseSetting(out int limit)
	{
		return GetAllowBaseSetting(stockpileType, out limit);
	}

	public bool MayStockpile(EntityType entityType, out int limit)
	{
		if (mayStockpileItem.TryGetValue(entityType, out var value))
		{
			limit = value;
			return value != 0;
		}
		if (mayStockpileCategory.TryGetValue(entityType.Category, out var value2))
		{
			limit = -1;
			return value2;
		}
		return GetAllowBaseSetting(out limit);
	}

	public bool TryGetCategory(EntityCategory entityCategory, out bool categorySetting)
	{
		return mayStockpileCategory.TryGetValue(entityCategory, out categorySetting);
	}

	public bool TryGetItem(EntityType entityType, out int setting)
	{
		return mayStockpileItem.TryGetValue(entityType, out setting);
	}

	private void MergeWithDefaultSettings()
	{
		if (defaultSettings == null)
		{
			return;
		}
		if (defaultSettings.MayStockpileCategoryFinal != null)
		{
			foreach (KeyValuePair<EntityCategory, bool> item in defaultSettings.MayStockpileCategoryFinal)
			{
				if (!mayStockpileCategory.ContainsKey(item.Key))
				{
					mayStockpileCategory.Add(item.Key, item.Value);
				}
			}
		}
		if (defaultSettings.MayStockpileItemFinal == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, int> item2 in defaultSettings.MayStockpileItemFinal)
		{
			if (!mayStockpileItem.ContainsKey(item2.Key))
			{
				mayStockpileItem.Add(item2.Key, item2.Value);
			}
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		categoryItemSettings = sn.DoMultiMap(categoryItemSettings);
		mayStockpileCategory = sn.DoDictionary(mayStockpileCategory);
		mayStockpileItem = sn.DoDictionary(mayStockpileItem);
		settingsAreDirty = sn.DoBool(settingsAreDirty);
		defaultSettings = sn.DoGameData(defaultSettings);
		stockpileType = sn.DoEnum(stockpileType);
		sn.Ignore(limitsAreDirty);
		sn.Ignore(itemsWithLimits);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
