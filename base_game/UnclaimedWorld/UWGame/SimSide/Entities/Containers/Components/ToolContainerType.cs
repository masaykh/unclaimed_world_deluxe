using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Containers.Components;

public class ToolContainerType : ContainerType
{
	public RequiresReplenishType RequiresReplenishType;

	public ItemStorageType ProductionOutputStorageType;

	public override float? FullStatePercentage => ProductionOutputStorageType.FullStatePercentage;

	public override float? HalfFullStatePercentage => ProductionOutputStorageType.HalfFullStatePercentage;

	public override bool HasOutputStorage => ProductionOutputStorageType != null;

	public override Container CreateContainer(Entity parent)
	{
		return new ToolContainer(parent);
	}

	public override RequiresReplenishType GetRequiresReplenishType()
	{
		return RequiresReplenishType;
	}

	public override Dictionary<EntityType, ProcessType> GetReplenishProcesses()
	{
		if (RequiresReplenishType != null)
		{
			return RequiresReplenishType.ReplenishProcesses;
		}
		return null;
	}

	public override float GetOutputStorageCapacity()
	{
		return ProductionOutputStorageType.GetTotalCapacity();
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
