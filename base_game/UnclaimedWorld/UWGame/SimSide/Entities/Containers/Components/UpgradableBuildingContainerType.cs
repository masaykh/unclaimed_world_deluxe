using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers.Components;

public class UpgradableBuildingContainerType : ContainerType, IHasItemStorageType
{
	public Vector2[] Doors;

	public bool HasRallyPointInCourtyard;

	public string UpgradesProfile;

	[XmlIgnore]
	public UpgradeProfile UpgradesProfileFinal;

	public string DefaultStorageSettings;

	public bool AllowStockpiling = true;

	public ItemStorageType ItemStorageType { get; set; }

	[XmlIgnore]
	public DefaultStorageSettings DefaultStorageSettingsFinal { get; private set; }

	public bool AllowsStockpiling => AllowStockpiling;

	public override bool CanBeUpgraded => UpgradesProfileFinal != null;

	public override float? FullStatePercentage => ItemStorageType.FullStatePercentage;

	public override float? HalfFullStatePercentage => ItemStorageType.HalfFullStatePercentage;

	public override Container CreateContainer(Entity parent)
	{
		return new UpgradableBuildingContainer(parent);
	}

	public override List<UpgradeCategory> GetUpgradeOptions()
	{
		if (UpgradesProfileFinal != null)
		{
			return UpgradesProfileFinal.UpgradeCategoriesFinal;
		}
		return null;
	}

	public UpgradableBuildingContainerType()
	{
	}

	public override Vector2[] GetDoors()
	{
		return Doors;
	}

	public override bool GetHasCourtyard()
	{
		return HasRallyPointInCourtyard;
	}

	public UpgradableBuildingContainerType(string condition1, float capacity1, string condition2 = null, float? capacity2 = null, string condition3 = null, float? capacity3 = null)
	{
		ItemStorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
	}

	public override DefaultStorageSettings GetDefaultStorageSettings()
	{
		return DefaultStorageSettingsFinal;
	}

	public override void Initialize()
	{
		base.Initialize();
		ItemStorageType.Initialize();
		if (DefaultStorageSettings != null)
		{
			DefaultStorageSettingsFinal = GameData.Instance.AllDefaultStorageSettings[DefaultStorageSettings];
		}
	}

	public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
	{
		base.PostInitValidate(parent, ref listOfErrors);
	}

	public override void PostLoadContentInitialize(EntityType parent)
	{
		base.PostLoadContentInitialize(parent);
	}

	public override void PostDataCompleteInitialize(EntityType parent)
	{
		if (UpgradesProfile != null)
		{
			UpgradesProfileFinal = GameData.Instance.AllUpgradeProfiles[UpgradesProfile];
		}
		base.PostDataCompleteInitialize(parent);
	}
}
