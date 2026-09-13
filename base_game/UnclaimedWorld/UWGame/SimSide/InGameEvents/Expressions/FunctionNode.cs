using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Expressions;

public class FunctionNode : EvalNode
{
	public ExpressionOperator Operator;

	public EvalNode Left;

	public EvalNode Right;

	private PropertyResult? result;

	public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		PropertyResult? leftResult = Left.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		PropertyResult? leftResult2 = Right.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		float? numberValue = null;
		Vector2? locationValue = null;
		string stringResult = null;
		GetValues(leftResult, ref numberValue, ref locationValue, ref stringResult);
		float? numberValue2 = null;
		Vector2? locationValue2 = null;
		string stringResult2 = null;
		GetValues(leftResult2, ref numberValue2, ref locationValue2, ref stringResult2);
		float? numberResult = null;
		Vector2? locationResult = null;
		string text = null;
		switch (Operator)
		{
		case ExpressionOperator.Plus:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = numberValue + numberValue2;
			}
			else if (stringResult != null && stringResult2 != null)
			{
				text = stringResult + stringResult2;
			}
			else if (locationValue.HasValue && locationValue2.HasValue)
			{
				locationResult = locationValue.Value + locationValue2.Value;
			}
			break;
		case ExpressionOperator.Minus:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = numberValue - numberValue2;
			}
			else if (stringResult != null && stringResult2 != null)
			{
				text = stringResult.Replace(stringResult2, "");
			}
			else if (locationValue.HasValue && locationValue2.HasValue)
			{
				locationResult = locationValue.Value - locationValue2.Value;
			}
			break;
		case ExpressionOperator.Multiply:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = numberValue * numberValue2;
			}
			break;
		case ExpressionOperator.Divide:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = numberValue / numberValue2;
			}
			break;
		case ExpressionOperator.ClampTop:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = Common.ClampTop(numberValue.Value, numberValue2.Value);
			}
			break;
		case ExpressionOperator.ClampBottom:
			if (numberValue.HasValue && numberValue2.HasValue)
			{
				numberResult = Common.ClampBottom(numberValue.Value, numberValue2.Value);
			}
			break;
		}
		if (numberResult.HasValue)
		{
			result = new PropertyResult
			{
				NumberResult = numberResult
			};
		}
		else if (text != null)
		{
			result = new PropertyResult
			{
				StringResult = text
			};
		}
		else if (locationResult.HasValue)
		{
			result = new PropertyResult
			{
				LocationResult = locationResult
			};
		}
		return result;
	}

	public static void GetValues(PropertyResult? leftResult, ref float? numberValue, ref Vector2? locationValue, ref string stringResult)
	{
		if (leftResult.HasValue)
		{
			if (leftResult.Value.NumberResult.HasValue)
			{
				numberValue = leftResult.Value.NumberResult.Value;
			}
			else if (leftResult.Value.LocationResult.HasValue)
			{
				locationValue = leftResult.Value.LocationResult.Value;
			}
			else if (leftResult.Value.StringResult != null)
			{
				stringResult = leftResult.Value.StringResult;
			}
		}
	}

	public override string ToString()
	{
		if (result.HasValue)
		{
			return result.ToString();
		}
		return Left.ToString() + " " + Operator.ToString() + " " + Right.ToString();
	}
}
