using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class ListCondition
{
	public int? CountEqual;

	public EvalNode CountMinimum;

	public EvalNode CountMaximum;

	public bool IsFulfilled(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, List<IHasExposedProperties> list)
	{
		int num = 0;
		if (list != null)
		{
			num = list.Count;
		}
		if (CountEqual.HasValue)
		{
			return num == CountEqual.Value;
		}
		if (CountMinimum != null)
		{
			float? number = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMinimum);
			if (number.HasValue && (float)num >= number.Value)
			{
				if (CountMaximum != null)
				{
					float? number2 = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMaximum);
					if (number2.HasValue)
					{
						return Common.IsLessThanOrEqual(num, number2.Value);
					}
				}
				return true;
			}
		}
		else if (CountMaximum != null)
		{
			float? number3 = GetNumber(triggeringEntity, targetEntity, polledEventSource, dynamicTarget, CountMaximum);
			if (number3.HasValue)
			{
				return Common.IsLessThanOrEqual(num, number3.Value);
			}
		}
		return false;
	}

	private float? GetNumber(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, EvalNode evalNode)
	{
		PropertyResult? propertyResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		if (propertyResult.HasValue)
		{
			return propertyResult.Value.NumberResult;
		}
		return null;
	}
}
