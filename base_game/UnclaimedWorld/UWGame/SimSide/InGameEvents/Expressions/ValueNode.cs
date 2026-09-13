using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Expressions;

public class ValueNode : EvalNode
{
	public int? Int;

	public float? Decimal;

	public string String;

	public bool? Bool;

	public Vector2? Location;

	public TargetObject TargetObject;

	public string PropertyKey;

	private PropertyResult? result;

	public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (Int.HasValue)
		{
			result = new PropertyResult
			{
				NumberResult = Int.Value
			};
		}
		else if (Decimal.HasValue)
		{
			result = new PropertyResult
			{
				NumberResult = Decimal.Value
			};
		}
		else if (Bool.HasValue)
		{
			result = new PropertyResult
			{
				BoolResult = Bool.Value
			};
		}
		else if (String != null)
		{
			result = new PropertyResult
			{
				StringResult = String
			};
		}
		else if (Location.HasValue)
		{
			result = new PropertyResult
			{
				LocationResult = Location.Value
			};
		}
		else
		{
			List<IHasExposedProperties> list = null;
			list = ((TargetObject == null) ? TargetObject.GetRootElementAsList() : TargetObject.GetResult(triggeringEntity, targetEntity, polledEventSource, dynamicTarget));
			if (list != null && list.Count > 0)
			{
				IHasExposedProperties hasExposedProperties = list[0];
				result = null;
				if (hasExposedProperties != null && PropertyKey != null)
				{
					result = hasExposedProperties.GetPropertyValue(PropertyKey);
				}
			}
		}
		return result;
	}

	public override string EvaluateConstant()
	{
		if (String != null)
		{
			return String;
		}
		return null;
	}

	public override string ToString()
	{
		if (result.HasValue)
		{
			return result.ToString();
		}
		return "";
	}
}
