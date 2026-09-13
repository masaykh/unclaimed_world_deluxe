using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateWander : GoalEvaluator
{
	private double mostDesirableScore;

	public override float Priority => 1000f;

	public EvaluateWander(Entity entity)
		: base(entity)
	{
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (entity.Locomotor.LeggedLocomotor.TestWander)
		{
			result = 1000.0;
			mostDesirableScore = result;
		}
		else
		{
			result = 0.0;
			mostDesirableScore = 0.0;
		}
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
		int num = -1 * Math.Sign(entity.FacingNormal.X);
		int num2 = -1 * Math.Sign(entity.FacingNormal.Y);
		Rectangle clampedMapAreaUsingTiles = The.Map.GetClampedMapAreaUsingTiles(new Point(entity.MapPosition.Value.X - 2 + 2 * num, entity.MapPosition.Value.Y - 2 + 2 * num2), 4, 4);
		intelligence.SetBoldStance();
		intelligence.SetTopLevelGoal(new GoalWander(entity, clampedMapAreaUsingTiles), mostDesirableScore);
		return true;
	}
}
