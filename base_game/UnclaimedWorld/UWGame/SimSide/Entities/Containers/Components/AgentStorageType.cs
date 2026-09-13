using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Containers.Components;

public class AgentStorageType : ContainerType
{
	public ItemStorageType ItemStorageType;

	public ItemStorageType EquipmentStorageType;

	public ItemStorageType StomachStorageType;

	public override Container CreateContainer(Entity parent)
	{
		return new AgentStorage(parent);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (ItemStorageType != null)
		{
			ItemStorageType.Initialize();
		}
		if (EquipmentStorageType != null)
		{
			EquipmentStorageType.Initialize();
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
