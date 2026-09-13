using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

public class DebugJobEvaluator : GoalEvaluator, IScoreJob
{
	public DebugJobEvaluator(Entity entity)
		: base(entity)
	{
	}

	public override bool SetGoal()
	{
		entity.DebugGoalPlan.GoalPlanner(entity, this);
		return true;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		result = double.MaxValue;
		return CalculateResult.Done;
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
	{
		rating = double.MaxValue;
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
}
