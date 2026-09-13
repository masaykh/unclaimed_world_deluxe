using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Containers.Components;

public class OtherContainerType : ContainerType
{
	public ItemStorageType StorageType;

	public override float? FullStatePercentage => StorageType.FullStatePercentage;

	public override float? HalfFullStatePercentage => StorageType.HalfFullStatePercentage;

	public override Container CreateContainer(Entity parent)
	{
		return new OtherContainer(parent);
	}

	public OtherContainerType(float isolatedCapacity)
	{
		StorageType = new ItemStorageType(isolatedCapacity);
	}

	public OtherContainerType(string condition1, float capacity1, string condition2 = null, float? capacity2 = null, string condition3 = null, float? capacity3 = null)
	{
		StorageType = new ItemStorageType(condition1, capacity1, condition2, capacity2, condition3, capacity3);
	}

	public OtherContainerType()
	{
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
	}
}
