using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Expressions;

public class UnaryFunctionNode : EvalNode
{
	public UnaryExpressionOperator Operator;

	public EvalNode Operand;

	private PropertyResult? result;

	public override PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		PropertyResult? propertyResult = Operand.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		if (!propertyResult.HasValue)
		{
			return null;
		}
		bool? flag = null;
		flag = propertyResult.Value.BoolResult;
		float? numberResult = null;
		Vector2? locationResult = null;
		string text = null;
		bool? boolResult = null;
		DateAndTime.TimeDateYear? dateResult = null;
		switch (Operator)
		{
		case UnaryExpressionOperator.ClampToWithinZeroAndOne:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				numberResult = Common.Clamp(propertyResult.Value.NumberResult.Value, 0f, 1f);
			}
			break;
		case UnaryExpressionOperator.Not:
			if (flag.HasValue)
			{
				boolResult = !flag.Value;
			}
			break;
		case UnaryExpressionOperator.DateFromRelativeDays:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				dateResult = new DateAndTime.TimeDateYear(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays + (double)propertyResult.Value.NumberResult.Value);
			}
			break;
		case UnaryExpressionOperator.DateFromAbsoluteDays:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				dateResult = new DateAndTime.TimeDateYear(propertyResult.Value.NumberResult.Value);
			}
			break;
		case UnaryExpressionOperator.DateFromRelativeSeconds:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				dateResult = new DateAndTime.TimeDateYear(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays + (double)propertyResult.Value.NumberResult.Value / DateAndTime.secondsPerDay);
			}
			break;
		case UnaryExpressionOperator.DateFromAbsoluteSeconds:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				dateResult = new DateAndTime.TimeDateYear((double)propertyResult.Value.NumberResult.Value / DateAndTime.secondsPerDay);
			}
			break;
		case UnaryExpressionOperator.DateToAbsoluteString:
			if (propertyResult.Value.DateResult.HasValue)
			{
				text = propertyResult.Value.DateResult.Value.ToString();
			}
			break;
		case UnaryExpressionOperator.DateToJournalString:
			if (propertyResult.Value.DateResult.HasValue)
			{
				text = propertyResult.Value.DateResult.Value.GetDateForJournal();
			}
			break;
		case UnaryExpressionOperator.DateToRelativeSeconds:
			if (propertyResult.Value.DateResult.HasValue)
			{
				numberResult = (float)propertyResult.Value.DateResult.Value.ToRelativeSeconds();
			}
			break;
		case UnaryExpressionOperator.DateToRelativeDays:
			if (propertyResult.Value.DateResult.HasValue)
			{
				numberResult = (float)propertyResult.Value.DateResult.Value.ToRelativeDays();
			}
			break;
		case UnaryExpressionOperator.ComfortRatingToString:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				text = FormatRating(propertyResult, RatingTypes.Comfort);
			}
			break;
		case UnaryExpressionOperator.FoodRatingToString:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				text = FormatRating(propertyResult, RatingTypes.Food);
			}
			break;
		case UnaryExpressionOperator.SecurityRatingToString:
			if (propertyResult.Value.NumberResult.HasValue)
			{
				text = FormatRating(propertyResult, RatingTypes.Security);
			}
			break;
		}
		if (numberResult.HasValue)
		{
			propertyResult = new PropertyResult
			{
				NumberResult = numberResult
			};
		}
		else if (text != null)
		{
			propertyResult = new PropertyResult
			{
				StringResult = text
			};
		}
		else if (locationResult.HasValue)
		{
			propertyResult = new PropertyResult
			{
				LocationResult = locationResult
			};
		}
		else if (boolResult.HasValue)
		{
			propertyResult = new PropertyResult
			{
				BoolResult = boolResult
			};
		}
		else if (dateResult.HasValue)
		{
			propertyResult = new PropertyResult
			{
				DateResult = dateResult
			};
		}
		return propertyResult;
	}

	private static string FormatRating(PropertyResult? result, RatingTypes ratingType)
	{
		float value = result.Value.NumberResult.Value;
		StringBuilder stringBuilder = new StringBuilder();
		Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, ratingType);
		stringBuilder.Append(" ");
		Common.AppendPercentage(stringBuilder, value, useColoring: false, null);
		return stringBuilder.ToString();
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
