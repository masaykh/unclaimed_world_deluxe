using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateFindPlaceToEat : GoalEvaluator
{
	public EvaluateFindPlaceToEat(Entity entity)
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
		return true;
	}

	public override bool CanTakeGoal()
	{
		return true;
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		return false;
	}
}
