using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class ConditionFunction : Condition
{
	public OperatorType Operator;

	public Condition Left;

	public Condition Right;

	public override void Initialize()
	{
		Left.Initialize();
		Right.Initialize();
	}

	public override void PreInitValidate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "Left", Left != null);
		EntityType.ValidateRequiredValue(ref errors, "Right", Right != null);
		if (Left != null && Right != null)
		{
			Left.PreInitValidate(ref errors);
			Right.PreInitValidate(ref errors);
		}
	}

	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		bool flag = Left.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		bool flag2 = Right.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		if (Operator == OperatorType.Or)
		{
			return flag || flag2;
		}
		if (Operator == OperatorType.And)
		{
			return flag && flag2;
		}
		return false;
	}
}
