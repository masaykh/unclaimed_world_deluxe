using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class DummyCondition : ConditionValue
{
	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
		{
			return true;
		}
		return false;
	}
}
