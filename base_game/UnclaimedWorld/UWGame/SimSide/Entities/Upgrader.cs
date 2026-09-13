using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Entities;

public class Upgrader
{
	public string[] UpgradeCategories;

	public StateModifier? SpriteModifier;

	[XmlIgnore]
	public List<UpgradeCategory> UpgradeCategoryFinal;

	public string StorageSettings;

	[XmlIgnore]
	public DefaultStorageSettings StorageSettingsFinal;

	public string[] Effects;

	[XmlIgnore]
	public List<EffectProfileType> EffectsFinal;

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		EntityType.ValidateRequiredValue(ref listOfErrors, "UpgradeCategories", UpgradeCategories != null);
		if (UpgradeCategories != null)
		{
			string[] upgradeCategories = UpgradeCategories;
			foreach (string key in upgradeCategories)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, key, GameData.Instance.AllUpgradeCategories, out var _);
			}
		}
		if (StorageSettings != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, StorageSettings, GameData.Instance.AllDefaultStorageSettings, out var _);
		}
	}

	public void PostDataCompleteInitialize(EntityType parent)
	{
		string[] effects;
		if (Effects != null)
		{
			EffectsFinal = new List<EffectProfileType>();
			effects = Effects;
			foreach (string key in effects)
			{
				EffectsFinal.Add(GameData.Instance.AllEffectProfileTypes[key]);
			}
		}
		UpgradeCategoryFinal = new List<UpgradeCategory>();
		effects = UpgradeCategories;
		foreach (string key2 in effects)
		{
			UpgradeCategoryFinal.Add(GameData.Instance.AllUpgradeCategories[key2]);
			Common.AddToMultiList(GameData.Instance.UpgraderEntityTypesByUpgradeCategory, GameData.Instance.AllUpgradeCategories[key2], parent);
		}
		if (StorageSettings != null)
		{
			StorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[StorageSettings];
		}
	}
}
