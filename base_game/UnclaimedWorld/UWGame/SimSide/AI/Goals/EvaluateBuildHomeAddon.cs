using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateBuildHomeAddon : GoalEvaluator
{
	private EntityType addonToStart;

	private Point mostDesirableLocation;

	private bool buildNewHome;

	public EvaluateBuildHomeAddon(Entity entity)
		: base(entity)
	{
	}

	private double ScoreNeedForAddon(Person personEntity)
	{
		return 0.0;
	}

	private static bool IsAddon(Entity entity)
	{
		if (entity.EntityType.StructureType != null)
		{
			return entity.EntityType.StructureType.IsAddon;
		}
		return false;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		_ = entity.PersonEntity;
		result = 0.0;
		return CalculateResult.Done;
	}

	private bool WeHaveAddonAlready(Person personEntity, EntityType proposedAddon)
	{
		return false;
	}

	private EntityType FindAddonToStart(Person personEntity)
	{
		return null;
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
