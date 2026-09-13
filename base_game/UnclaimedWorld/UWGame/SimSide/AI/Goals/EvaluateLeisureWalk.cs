using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateLeisureWalk : GoalEvaluator
{
	private Rectangle stayInside;

	private LeisureWalkActivity bestActivity;

	public List<EntityGroup> OwnersOfActivities;

	private double score = 0.1;

	public EvaluateLeisureWalk(Entity entity, List<EntityGroup> OwnersOfActivities)
		: base(entity)
	{
		this.OwnersOfActivities = OwnersOfActivities;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		bestActivity = null;
		_ = entity.Intelligence;
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
		Intelligence intelligence = entity.Intelligence;
		if (bestActivity == null)
		{
			Vector3 vector = new Vector3(80f, 40f, 0f);
			if (Common.DistanceOctile(entity.PlaySiteLocation, vector) < 50f)
			{
				vector = new Vector3(120f, 20f, 0f);
			}
			bestActivity = new LeisureWalkActivity(OwnersOfActivities[0].Activities, vector);
		}
		intelligence.SetTopLevelGoal(new GoalLeisureWalk(entity, bestActivity), score);
		return true;
	}
}
