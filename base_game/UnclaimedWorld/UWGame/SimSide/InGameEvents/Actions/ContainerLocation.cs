using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ContainerLocation
{
	public TargetObject TargetObject;

	public string StorageCondition;

	public bool OfferForSale;

	public bool IsProductionOutput;

	public string UpgradeCategory;

	public Entity GetContainer(EventAction action)
	{
		List<IHasExposedProperties> list = null;
		list = ((TargetObject == null) ? TargetObject.GetRootElementAsList() : TargetObject.GetResult(action));
		if (list != null && list.Count > 0)
		{
			IHasExposedProperties hasExposedProperties = list[0];
			if (hasExposedProperties != null && hasExposedProperties is Entity { Contains: not null } entity)
			{
				return entity;
			}
		}
		return null;
	}
}
