using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Containers.Components;

public class MagazineContainerType : ContainerType
{
	public string UsesAmmoTag;

	public string UsesAmmoTypeKeyName;

	public int MaxCapacity;

	public string ReplenishProcess;

	[XmlIgnore]
	private ProcessType ReplenishProcessType;

	[XmlIgnore]
	public Dictionary<EntityType, ProcessType> ReplenishProcesses;

	[XmlIgnore]
	public List<EntityType> AmmoEntityTypes;

	public override Container CreateContainer(Entity parent)
	{
		return new MagazineContainer(parent);
	}

	public override Dictionary<EntityType, ProcessType> GetReplenishProcesses()
	{
		return ReplenishProcesses;
	}

	public override void PostLoadContentInitialize(EntityType parent)
	{
		base.PostLoadContentInitialize(parent);
		if (!string.IsNullOrEmpty(UsesAmmoTag))
		{
			AmmoEntityTypes = GameData.Instance.AmmoByTag[UsesAmmoTag];
		}
		if (!string.IsNullOrEmpty(UsesAmmoTypeKeyName))
		{
			EntityType item = GameData.Instance.AllEntityTypes[UsesAmmoTypeKeyName];
			if (AmmoEntityTypes == null)
			{
				AmmoEntityTypes = new List<EntityType>();
			}
			if (!AmmoEntityTypes.Contains(item))
			{
				AmmoEntityTypes.Add(item);
			}
		}
		if (string.IsNullOrEmpty(ReplenishProcess))
		{
			return;
		}
		ReplenishProcessType = GameData.Instance.AllProcessTypes[ReplenishProcess];
		ReplenishProcessType.IsReplenishProcess = true;
		ReplenishProcessType.ReplenishAction = GoalReplenish.ReplenishAction.Reload;
		foreach (EntityType ammoEntityType in AmmoEntityTypes)
		{
			RequiresReplenishType.InitReplenishProcess(parent, ReplenishProcessType, ammoEntityType, ref ReplenishProcesses);
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
}
