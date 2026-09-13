using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Jobs;

public class LightFireJob : Job
{
	public float ManSecondsOfWorkNeeded;

	public Entity FireSite;

	public float Progress;

	public LightFireJob(Entity fireSite, EntityGroup entityGroup)
		: base(entityGroup)
	{
		FireSite = fireSite;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public LightFireJob()
	{
	}

	public bool IsInProgress()
	{
		return Progress > 0f;
	}

	public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, double? ageContribution, double? timeContribution, ref double rating, float priority)
	{
		double travelTimeScore = 0.0;
		switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, FireSite.Location.Value, entity, ref travelTimeScore))
		{
		case RegionMap.Result.Wait:
			return GoalEvaluator.CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			rating = 0.0;
			return GoalEvaluator.CalculateResult.Done;
		default:
		{
			double num = 1.0;
			if (FireSite.Find<Tool>(out var c) && FireSite.EntityType.ContainerType != null && FireSite.EntityType.ContainerType.GetRequiresReplenishType() != null)
			{
				if (c.IsPrepared == true)
				{
					rating = 0.0;
					return GoalEvaluator.CalculateResult.Done;
				}
				num = Common.Clamp(((IHasReplenishItems)FireSite.Contains).ReplenishItems.RequiresFuel.Fuel, 0f, 1f);
			}
			if (Common.IsEqual(num, 0.0))
			{
				rating = GameData.Instance.AIConstants.JobWithZeroMaterialsDesirability;
			}
			else
			{
				rating = 0.2 * travelTimeScore + 0.4 * num + 0.4;
				rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
			}
			return GoalEvaluator.CalculateResult.Done;
		}
		}
	}

	public override Vector3? GetCircaLocation()
	{
		return null;
	}
}
