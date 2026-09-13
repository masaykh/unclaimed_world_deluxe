using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Containers.Components;

[XmlInclude(typeof(TerminalContainerType))]
[XmlInclude(typeof(VehicleContainerType))]
[XmlInclude(typeof(AgentStorageType))]
[XmlInclude(typeof(HomeContainerType))]
[XmlInclude(typeof(ReplenishContainerType))]
[XmlInclude(typeof(WorkshopContainerType))]
[XmlInclude(typeof(ToolContainerType))]
[XmlInclude(typeof(StorageContainerType))]
public abstract class ContainerType
{
	public string[] CanTransactWithTags;

	[XmlIgnore]
	private BitArray CanTransactWith;

	public string[] CanBeEnteredByTags;

	[XmlIgnore]
	private BitArray CanBeEnteredBy;

	public string[] StorageTags;

	public ResidenceType ResidenceType;

	[XmlIgnore]
	public bool VerminCanAccess;

	public virtual float? FullStatePercentage => null;

	public virtual float? HalfFullStatePercentage => null;

	public virtual bool HasOutputStorage => false;

	public virtual bool CanBeUpgraded => false;

	public virtual RequiresReplenishType GetRequiresReplenishType()
	{
		return null;
	}

	public abstract Container CreateContainer(Entity parent);

	public virtual void Initialize()
	{
		if (ResidenceType != null)
		{
			ResidenceType.Initialize();
		}
	}

	public virtual void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
	{
		if (HasOutputStorage && parent.ToolType == null)
		{
			EntityType.CreateValidationError(ref listOfErrors, "Only tools can have production output. Either make the type a tool by defining EntityType.ToolType, or change the container type.");
		}
	}

	public virtual void PostDataCompleteInitialize(EntityType parent)
	{
		if (GetUpgradeOptions() == null)
		{
			return;
		}
		foreach (UpgradeCategory upgradeOption in GetUpgradeOptions())
		{
			Common.AddToMultiList(GameData.Instance.EntityTypesToUpgradeByUpgradeCategory, upgradeOption, parent);
		}
	}

	public virtual void PostLoadContentInitialize(EntityType parent)
	{
		string[] array = CanTransactWithTags;
		if (CanTransactWithTags == null)
		{
			array = CanBeEnteredByTags;
		}
		else if (CanBeEnteredByTags != null)
		{
			int num = array.Length;
			Array.Resize(ref array, num + CanBeEnteredByTags.Length);
			Array.Copy(CanBeEnteredByTags, 0, array, num, CanBeEnteredByTags.Length);
			array = array.Distinct().ToArray();
		}
		CanTransactWith = GameData.CreateBitArrayFromTags(GameData.Instance.ContainerTags, array);
		CanBeEnteredBy = GameData.CreateBitArrayFromTags(GameData.Instance.ContainerTags, CanBeEnteredByTags);
	}

	public void SetVerminCanAccess()
	{
		foreach (KeyValuePair<string, EntityType> allVerminType in GameData.Instance.AllVerminTypes)
		{
			if (CanTransactWithContainer(allVerminType.Value))
			{
				VerminCanAccess = true;
				break;
			}
		}
	}

	public bool CanTransactWithContainer(EntityType agentType)
	{
		if (agentType.IntelligenceType != null && agentType.IntelligenceType.ContainerTransactValue.HasValue)
		{
			return CanTransactWith[agentType.IntelligenceType.ContainerTransactValue.Value];
		}
		return false;
	}

	public bool AllowedInContainer(EntityType agentType)
	{
		if (agentType.IntelligenceType != null && agentType.IntelligenceType.ContainerTransactValue.HasValue)
		{
			return CanBeEnteredBy[agentType.IntelligenceType.ContainerTransactValue.Value];
		}
		return false;
	}

	public virtual DefaultStorageSettings GetDefaultStorageSettings()
	{
		return null;
	}

	public virtual List<UpgradeCategory> GetUpgradeOptions()
	{
		return null;
	}

	public virtual Dictionary<EntityType, ProcessType> GetReplenishProcesses()
	{
		return null;
	}

	public virtual float GetOutputStorageCapacity()
	{
		return 0f;
	}

	public virtual Vector2[] GetDoors()
	{
		return null;
	}

	public virtual bool GetHasCourtyard()
	{
		return false;
	}

	public int GetCapacityForIdlingPeople()
	{
		if (ResidenceType != null)
		{
			return ResidenceType.PeopleCapacity;
		}
		return 0;
	}
}
