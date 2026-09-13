using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Containers.Components;

public class WorkshopContainerType : ContainerType, IHasItemStorageType
{
	public string DefaultStorageSettings;

	public bool AllowStockpiling = true;

	public RequiresReplenishType RequiresReplenishType;

	public ItemStorageType ProductionOutputStorageType;

	public ItemStorageType ItemStorageType { get; set; }

	[XmlIgnore]
	public DefaultStorageSettings DefaultStorageSettingsFinal { get; private set; }

	public bool AllowsStockpiling => AllowStockpiling;

	public override bool HasOutputStorage => ProductionOutputStorageType != null;

	public override float? FullStatePercentage => ItemStorageType.FullStatePercentage;

	public override float? HalfFullStatePercentage => ItemStorageType.HalfFullStatePercentage;

	public override Container CreateContainer(Entity parent)
	{
		return new WorkshopContainer(parent);
	}

	public override RequiresReplenishType GetRequiresReplenishType()
	{
		return RequiresReplenishType;
	}

	public override float GetOutputStorageCapacity()
	{
		return ProductionOutputStorageType.GetTotalCapacity();
	}

	public override Dictionary<EntityType, ProcessType> GetReplenishProcesses()
	{
		if (RequiresReplenishType != null)
		{
			return RequiresReplenishType.ReplenishProcesses;
		}
		return null;
	}

	public WorkshopContainerType()
	{
	}

	public WorkshopContainerType(string condition1, float capacity1, string condition2 = null, float? capacity2 = null, string condition3 = null, float? capacity3 = null)
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
		if (CanBeEnteredByTags != null)
		{
			EntityType.CreateValidationError(ref listOfErrors, "CanBeEnteredByTags should not be specified because this container type has no doors and cannot be entered.");
		}
	}

	public override void PostLoadContentInitialize(EntityType parent)
	{
		base.PostLoadContentInitialize(parent);
		if (RequiresReplenishType != null)
		{
			RequiresReplenishType.PostLoadContentInitialize(parent);
		}
	}
}
