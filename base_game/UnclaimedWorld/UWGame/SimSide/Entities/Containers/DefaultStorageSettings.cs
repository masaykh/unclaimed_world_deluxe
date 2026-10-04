using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Containers;

/// <summary>
/// Defines which items a structure may store by default.
/// Positive values allow storage and make this structure the preferred destination for that item.
/// If a setting is omitted, storage is allowed by default. When settings overlap, entity-type entries override tag entries.
/// </summary>
public class DefaultStorageSettings : IGameData
{
	public string Comments;

	/// <summary>
	/// Item tags are shorthand for naming groups such as food, ammo, or fuel.
	/// Settings here can be overridden by <see cref="MayStockpileEntityType"/>.
	/// </summary>
	public SerializableDictionary<string, int> MayStockpileItemTag;

	/// <summary>
	/// Category-specific settings override broader category groupings.
	/// </summary>
	public SerializableDictionary<string, bool> MayStockpileCategory;

	/// <summary>
	/// Entity-type settings override tag-based settings.
	/// Using tags is usually safer long term than listing many specific entity types.
	/// </summary>
	public SerializableDictionary<string, int> MayStockpileEntityType;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	[XmlIgnore]
	public Dictionary<EntityType, int> MayStockpileItemFinal { get; private set; }

	[XmlIgnore]
	public Dictionary<EntityCategory, bool> MayStockpileCategoryFinal { get; private set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostLoadContentInitialize()
	{
		if (MayStockpileItemTag != null)
		{
			foreach (KeyValuePair<string, int> item in MayStockpileItemTag)
			{
				if (!TryTagCollection(GameData.Instance.AmmoByTag, item) && !TryTagCollection(GameData.Instance.FoodByTag, item) && !TryTagCollection(GameData.Instance.ToolsByTag, item))
				{
					TryTagCollection(GameData.Instance.FuelByTag, item);
				}
			}
		}
		if (MayStockpileCategory != null)
		{
			MayStockpileCategoryFinal = new Dictionary<EntityCategory, bool>();
			foreach (KeyValuePair<string, bool> item2 in MayStockpileCategory)
			{
				MayStockpileCategoryFinal.Add(GameData.Instance.AllEntityCategories[item2.Key], item2.Value);
			}
		}
		if (MayStockpileEntityType == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> item3 in MayStockpileEntityType)
		{
			AddItem(GameData.Instance.AllEntityTypes[item3.Key], item3.Value);
		}
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	private bool TryTagCollection(Dictionary<string, List<EntityType>> tagCollection, KeyValuePair<string, int> tag)
	{
		if (tagCollection.TryGetValue(tag.Key, out var value))
		{
			foreach (EntityType item in value)
			{
				AddItem(item, tag.Value);
			}
			return true;
		}
		return false;
	}

	private void AddItem(EntityType item, int value)
	{
		if (MayStockpileItemFinal == null)
		{
			MayStockpileItemFinal = new Dictionary<EntityType, int>();
		}
		if (MayStockpileItemFinal.ContainsKey(item))
		{
			MayStockpileItemFinal[item] = value;
		}
		else
		{
			MayStockpileItemFinal.Add(item, value);
		}
	}
}
