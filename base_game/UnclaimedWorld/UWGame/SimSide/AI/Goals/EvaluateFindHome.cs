using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateFindHome : GoalEvaluator
{
	private IKnownEntityData mostDesirableHome;

	private float priority;

	public override float Priority => priority;

	public EvaluateFindHome(Entity entity)
		: base(entity)
	{
		priority = GameData.Instance.AIConstants.PriorityOfFindingANewHome;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (entity.PersonEntity.IsHeadOfHousehold())
		{
			if (entity.Find<BiologicalEntity>(out var c) && ScoreAge(c) == 0.0)
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			Vector3 freeGroundLocation = EntityGroup.GetFreeGroundLocation(entityIntelligence.CurrentExpedition);
			ThreatStance threatStanceToUse;
			RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
			bool incompleteHomesInHousehold = false;
			double num = 0.0;
			double staticScore = 0.0;
			if (entity.PersonEntity.Household.Home.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entity.PersonEntity.Household.Home.Value, out var data)))
			{
				double score = 0.0;
				bool belongsToHousehold = data.OwnedBy == entity.PersonEntity.Household.GetOwnerID();
				if (ScoreResidence(entity, regionMapAndStanceForEvaluator, threatStanceToUse, freeGroundLocation, data, belongsToHousehold, ref score, ref staticScore, isOwnResidence: true) == CalculateResult.Processing)
				{
					return CalculateResult.Processing;
				}
			}
			mostDesirableHome = null;
			double num2 = ScoreTimeOfDay();
			if (entityIntelligence.CurrentExpedition != null && LookForSuitableHomeInList(entity, entityIntelligence.CurrentExpedition.OwnedEntities, regionMapAndStanceForEvaluator, threatStanceToUse, freeGroundLocation, belongsToHousehold: false, ref incompleteHomesInHousehold, ref num, staticScore) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			bool incompleteHomesInHousehold2 = false;
			if (LookForSuitableHomeInList(entity, entity.PersonEntity.Household.OwnedEntities, regionMapAndStanceForEvaluator, threatStanceToUse, freeGroundLocation, belongsToHousehold: true, ref incompleteHomesInHousehold2, ref num, staticScore) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			if (mostDesirableHome != null)
			{
				_ = entity.ID;
				_ = 5142;
				result = 0.8 * num + 0.2 * num2;
				result *= Priority;
				return CalculateResult.Done;
			}
		}
		result = 0.0;
		return CalculateResult.Done;
	}

	private CalculateResult LookForSuitableHomeInList(Entity entity, EntityGroup ownerOfBuildings, RegionMap regionMapToUse, ThreatStance threatStanceToUse, Vector3 closestUnblockedExpeditionLocation, bool belongsToHousehold, ref bool incompleteHomesInHousehold, ref double bestScore, double staticScoreOfCurrentHome)
	{
		double score = 0.0;
		double staticScore = 0.0;
		double num = 0.0;
		if (Common.IsGreaterThan(staticScoreOfCurrentHome, 0.0))
		{
			num = staticScoreOfCurrentHome + (double)GameData.Instance.AIConstants.ResidenceScoreMustBeBetterToMove;
		}
		foreach (KeyValuePair<EntityType, List<EntityID>> structure in ownerOfBuildings.Structures)
		{
			if (structure.Key.ContainerType == null || structure.Key.ContainerType.ResidenceType == null)
			{
				continue;
			}
			for (int num2 = structure.Value.Count - 1; num2 >= 0; num2--)
			{
				EntityID entityID = structure.Value[num2];
				if (entityID != entity.PersonEntity.Household.Home && GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, entityID, ownerOfBuildings, out var entityData))
				{
					if (GoalEvaluator.IsOnPlaySite(entityData) && entityData.IsCompleted())
					{
						CalculateResult calculateResult = ScoreResidence(entity, regionMapToUse, threatStanceToUse, closestUnblockedExpeditionLocation, entityData, belongsToHousehold, ref score, ref staticScore, isOwnResidence: false);
						if (calculateResult == CalculateResult.Processing)
						{
							return calculateResult;
						}
						if (score > bestScore && staticScore > num)
						{
							bestScore = score;
							mostDesirableHome = entityData;
						}
					}
					else
					{
						incompleteHomesInHousehold = true;
					}
				}
			}
		}
		return CalculateResult.Done;
	}

	private CalculateResult ScoreResidence(Entity entity, RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 closestUnblockedExpeditionLocation, IKnownEntityData residence, bool belongsToHousehold, ref double score, ref double staticScore, bool isOwnResidence)
	{
		bool flag = true;
		if (isOwnResidence)
		{
			if (!Residence.HasCapacity(entity.PersonEntity.Household, 0, residence.EntityType))
			{
				flag = false;
			}
		}
		else if (!Residence.HasCapacity(entity.PersonEntity.Household, residence.Residents.Value, residence.EntityType))
		{
			flag = false;
		}
		if (!flag)
		{
			staticScore = 0.0;
			score = 0.0;
			return CalculateResult.Done;
		}
		double travelTimeScore = 0.0;
		switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, residence.AccessPoint.Value, entity, ref travelTimeScore))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			score = 0.0;
			return CalculateResult.Done;
		default:
		{
			double travelTimeScore2 = 0.0;
			switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, residence.AccessPoint.Value, closestUnblockedExpeditionLocation, entity, ref travelTimeScore2))
			{
			case RegionMap.Result.Wait:
				return CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				score = 0.0;
				return CalculateResult.Done;
			default:
			{
				double num = 1.0;
				double num2 = 0.0;
				if (belongsToHousehold)
				{
					num2 = 1.0;
				}
				if (Common.IsZero(GoalEvaluator.ScoreIsEntityFunctional(residence)))
				{
					score = 0.0;
				}
				else
				{
					double num3 = residence.ComfortLevel.Value;
					staticScore = 0.4 * travelTimeScore2 + 0.1 * travelTimeScore + 0.1 * num + 0.1 * num2 + 0.3 * num3;
					score = staticScore + 0.1 * travelTimeScore;
				}
				return CalculateResult.Done;
			}
			}
		}
		}
	}

	private Point GetRandomTilePosNearExpeditionForNewHome()
	{
		Point point = MapManager.WorldPosToTile(entityIntelligence.CurrentExpedition.Center.Value);
		return new Point(point.X + The.Sim.GameplayRandomGenerator.RandomSign() * The.Sim.GameplayRandomGenerator.Next(8, 15, "EvaluateFindHome"), point.Y + The.Sim.GameplayRandomGenerator.RandomSign() * The.Sim.GameplayRandomGenerator.Next(8, 15, "EvaluateFindHome"));
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
		entity.Intelligence.SetTopLevelGoal(new GoalMoveInToNewHome(ownersOfVehicles: GetOwnerIDs(new List<EntityGroup>
		{
			entity.PersonEntity.OwnedEntities,
			entity.PersonEntity.Household.OwnedEntities
		}), owner: entity, newHome: mostDesirableHome.EntityID), bestScore);
		return true;
	}
}
