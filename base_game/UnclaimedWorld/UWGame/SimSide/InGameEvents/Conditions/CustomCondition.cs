using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class CustomCondition : ConditionValue
{
	public TargetObject TargetObject;

	public ListCondition ListCondition;

	public PropertyCondition PropertyCondition;

	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
		{
			List<IHasExposedProperties> list = null;
			EntityID? triggeringEntity2 = ((triggeringEntity != null) ? new EntityID?(triggeringEntity.EntityID) : ((EntityID?)null));
			list = ((TargetObject == null) ? TargetObject.GetRootElementAsList() : TargetObject.GetResult(triggeringEntity2, targetEntity, polledEventSource, dynamicTarget));
			if (PropertyCondition != null)
			{
				if (list != null && list.Count > 0)
				{
					if (PropertyCondition.IsFulfilled(list[0], triggeringEntity2, targetEntity, polledEventSource, dynamicTarget))
					{
						if (list[0] is Entity entity)
						{
							triggeringEntity = entity;
						}
						return true;
					}
					return false;
				}
			}
			else if (ListCondition != null)
			{
				return ListCondition.IsFulfilled(triggeringEntity2, targetEntity, polledEventSource, dynamicTarget, list);
			}
			return false;
		}
		return false;
	}
}
