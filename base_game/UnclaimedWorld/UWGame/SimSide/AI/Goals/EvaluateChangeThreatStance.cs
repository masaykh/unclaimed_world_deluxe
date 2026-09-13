using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateChangeThreatStance : GoalEvaluator
{
	public override float Priority => 10000f;

	public EvaluateChangeThreatStance(Entity entity)
		: base(entity)
	{
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (entityIntelligence.ThreatStance != ThreatStance.Bold || (entityIntelligence.Brain.Subgoals.Count > 0 && entityIntelligence.Brain.Subgoals.Peek().RequiresBoldStance()))
		{
			bestScore = 0.0;
			result = bestScore;
			return CalculateResult.Done;
		}
		if (entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs.Count == 0)
		{
			bestScore = 1f * Priority;
		}
		else if (!entityIntelligence.StanceCanBeBold())
		{
			bestScore = 1f * Priority;
		}
		else
		{
			foreach (ThreatJob threatJob in entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(threatJob.Target.Value, out var data)) && Common.DistanceOctile(entity.Location.Value, data.Location.Value) < 400f)
				{
					bestScore = 0.0;
					result = bestScore;
					return CalculateResult.Done;
				}
			}
			bestScore = 1.0 * (double)Priority;
		}
		result = bestScore;
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
		entityIntelligence.ResetThreatStance();
		return true;
	}
}
