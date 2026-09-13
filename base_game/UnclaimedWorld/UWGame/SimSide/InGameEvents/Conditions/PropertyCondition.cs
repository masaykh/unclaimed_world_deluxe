using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class PropertyCondition : FilterCondition
{
	public string PropertyKey;

	public string ConstantStringEqual;

	public EvalNode StringEqual;

	public EvalNode StringNotEqual;

	public EvalNode NumberMinimumInclusive;

	public EvalNode NumberMinimumNotInclusive;

	public EvalNode NumberMaximumNotInclusive;

	public EvalNode NumberEqual;

	public EvalNode NumberNotEqual;

	public bool? BoolValue;

	public bool? IsNull;

	public float? LocationDistance;

	public EvalNode DynamicLocation;

	private bool FulfillsCondition(PropertyResult result, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (result.BoolResult.HasValue)
		{
			if (BoolValue.HasValue)
			{
				return result.BoolResult.Value == BoolValue.Value;
			}
		}
		else if (result.NumberResult.HasValue)
		{
			float value = result.NumberResult.Value;
			if (NumberEqual != null)
			{
				float? numberCompareValue = GetNumberCompareValue(NumberEqual, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (!numberCompareValue.HasValue)
				{
					return false;
				}
				if (Common.IsEqual(value, numberCompareValue.Value))
				{
					return true;
				}
			}
			else if (NumberNotEqual != null)
			{
				float? numberCompareValue2 = GetNumberCompareValue(NumberNotEqual, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (!numberCompareValue2.HasValue)
				{
					return false;
				}
				if (!Common.IsEqual(value, numberCompareValue2.Value))
				{
					return true;
				}
			}
			else if (NumberMinimumInclusive != null)
			{
				float? numberCompareValue3 = GetNumberCompareValue(NumberMinimumInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (!numberCompareValue3.HasValue)
				{
					return false;
				}
				if (Common.IsGreaterThanOrEqual(value, numberCompareValue3.Value))
				{
					if (NumberMaximumNotInclusive != null)
					{
						float? numberCompareValue4 = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
						if (numberCompareValue4.HasValue)
						{
							return Common.IsLessThan(value, numberCompareValue4.Value);
						}
						return false;
					}
					return true;
				}
			}
			else if (NumberMinimumNotInclusive != null)
			{
				float? numberCompareValue5 = GetNumberCompareValue(NumberMinimumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (!numberCompareValue5.HasValue)
				{
					return false;
				}
				if (Common.IsGreaterThan(value, numberCompareValue5.Value))
				{
					if (NumberMaximumNotInclusive != null)
					{
						float? numberCompareValue6 = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
						if (numberCompareValue6.HasValue)
						{
							return Common.IsLessThan(value, numberCompareValue6.Value);
						}
						return false;
					}
					return true;
				}
			}
			else if (NumberMaximumNotInclusive != null)
			{
				float? numberCompareValue7 = GetNumberCompareValue(NumberMaximumNotInclusive, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (numberCompareValue7.HasValue)
				{
					return Common.IsLessThan(value, numberCompareValue7.Value);
				}
				return false;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(result.StringResult) && (StringEqual != null || ConstantStringEqual != null))
			{
				return result.StringResult == GetStringCompareValue(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			}
			if (StringNotEqual != null)
			{
				string text = null;
				PropertyResult? propertyResult = StringNotEqual.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (propertyResult.HasValue)
				{
					text = propertyResult.Value.StringResult;
				}
				return result.StringResult != text;
			}
			if (result.LocationResult.HasValue && DynamicLocation != null)
			{
				Vector2? locationCompareValue = GetLocationCompareValue(DynamicLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				float num = LocationDistance ?? 0f;
				if (!locationCompareValue.HasValue)
				{
					return false;
				}
				return Common.DistanceOctile(result.LocationResult.Value, locationCompareValue.Value) < num;
			}
		}
		_ = IsNull == true;
		return false;
	}

	public override bool IsFulfilled(IHasExposedProperties hasProperties, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		PropertyResult? propertyResult = null;
		if (hasProperties != null)
		{
			propertyResult = hasProperties.GetPropertyValue(PropertyKey);
		}
		if (IsNull.HasValue)
		{
			if (propertyResult.HasValue)
			{
				return !IsNull.Value;
			}
			return IsNull.Value;
		}
		if (propertyResult.HasValue)
		{
			return FulfillsCondition(propertyResult.Value, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		return false;
	}

	public string GetStringCompareValue(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (StringEqual != null)
		{
			PropertyResult? propertyResult = StringEqual.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (propertyResult.HasValue)
			{
				return propertyResult.Value.StringResult;
			}
			return null;
		}
		return ConstantStringEqual;
	}

	public float? GetNumberCompareValue(EvalNode evalNode, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (evalNode != null)
		{
			PropertyResult? propertyResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (propertyResult.HasValue)
			{
				return propertyResult.Value.NumberResult;
			}
			return null;
		}
		return null;
	}

	public Vector2? GetLocationCompareValue(EvalNode evalNode, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (evalNode != null)
		{
			PropertyResult? propertyResult = evalNode.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (propertyResult.HasValue && propertyResult.Value.LocationResult.HasValue)
			{
				return propertyResult.Value.LocationResult.Value;
			}
			return null;
		}
		return null;
	}
}
