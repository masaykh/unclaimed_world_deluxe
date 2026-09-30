using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateBuildHomeAddon : GoalEvaluator
{
	public EvaluateBuildHomeAddon(Entity entity)
		: base(entity)
	{
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		_ = entity.PersonEntity;
		result = 0.0;
		return CalculateResult.Done;
	}

	public override bool CancelCurrentTakers()
	{
		throw new NotImplementedException();
	}

	public override bool CanTakeGoal()
	{
		throw new NotImplementedException();
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		return false;
	}
}
