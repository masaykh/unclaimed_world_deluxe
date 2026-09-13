using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class FilterConditionFunction : FilterCondition
{
	public OperatorType Operator;

	public FilterCondition Left;

	public FilterCondition Right;

	public override bool IsFulfilled(IHasExposedProperties hasProperties, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		bool flag = Left.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		if (Operator == OperatorType.Or)
		{
			if (flag)
			{
				return true;
			}
			return Right.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		if (Operator == OperatorType.And)
		{
			if (flag)
			{
				return Right.IsFulfilled(hasProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			}
			return false;
		}
		return false;
	}
}
