using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateTakeFive : GoalEvaluator
{
	private double mostDesirableScore;

	public EvaluateTakeFive(Entity entity)
		: base(entity)
	{
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		result = GameData.Instance.AIConstants.IdleGoalDesirability + (The.Sim.GameplayRandomGenerator.NextDouble("EvaluateTakeFive") * 3.0 - 1.0) * 0.005;
		mostDesirableScore = result;
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
		entityIntelligence.SetTopLevelGoal(new GoalTakeFive(entity), mostDesirableScore);
		return true;
	}

	public override bool IsIdleActivity()
	{
		return true;
	}
}
