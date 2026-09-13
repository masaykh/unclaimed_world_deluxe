using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class EvaluateEmigrate : GoalEvaluator
{
	private EntityID? mostDesirableTerminal;

	private RouteID? routeToUse;

	private float priority;

	public override float Priority => priority;

	public EvaluateEmigrate(Entity entity)
		: base(entity)
	{
		priority = GameData.Instance.AIConstants.PriorityOfEmigrating;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		mostDesirableTerminal = null;
		routeToUse = null;
		result = 0.0;
		if (!entityIntelligence.Brain.IsSame(typeof(GoalEmigrate)) && entityIntelligence.Memory.EmigrateTarget.HasValue)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(entityIntelligence.Memory.EmigrateTarget.Value);
			if (allegiance != null)
			{
				RouteID? route = GetRoute(RouteType.Land, allegiance.Site);
				if (route.HasValue)
				{
					if (FindClosestTerminal(TerminalType.TypesOfTerminal.Land) == CalculateResult.Processing)
					{
						return CalculateResult.Processing;
					}
					if (mostDesirableTerminal.HasValue)
					{
						routeToUse = route;
						result = Priority;
						bestScore = result;
						return CalculateResult.Done;
					}
				}
			}
			entityIntelligence.Memory.SetEmigrateDecision(null, entity);
		}
		return CalculateResult.Done;
	}

	private CalculateResult ScoreTerminal(Entity entity, RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 closestUnblockedExpeditionLocation, IKnownEntityData terminal, ref double score)
	{
		double travelTimeScore = 0.0;
		switch (GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, terminal.AccessPoint.Value, entity, ref travelTimeScore))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			score = 0.0;
			return CalculateResult.Done;
		default:
			if (Common.IsZero(GoalEvaluator.ScoreIsEntityFunctional(terminal)))
			{
				score = 0.0;
			}
			else
			{
				score = travelTimeScore;
			}
			return CalculateResult.Done;
		}
	}

	private CalculateResult FindClosestTerminal(TerminalType.TypesOfTerminal terminalType)
	{
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		Vector3 freeGroundLocation = EntityGroup.GetFreeGroundLocation(entityIntelligence.CurrentExpedition);
		double score = 0.0;
		bestScore = 0.0;
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.Terminals)
		{
			if (terminal.Key != terminalType)
			{
				continue;
			}
			for (int num = terminal.Value.Count - 1; num >= 0; num--)
			{
				EntityID entityID = terminal.Value[num];
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out var data)) && GoalEvaluator.IsOnPlaySite(data) && data.IsCompleted())
				{
					CalculateResult calculateResult = ScoreTerminal(entity, regionMapAndStanceForEvaluator, threatStanceToUse, freeGroundLocation, data, ref score);
					if (calculateResult == CalculateResult.Processing)
					{
						return calculateResult;
					}
					if (score > bestScore)
					{
						bestScore = score;
						mostDesirableTerminal = data.EntityID;
					}
				}
			}
		}
		return CalculateResult.Done;
	}

	private RouteID? GetRoute(RouteType routeType, Site toSite)
	{
		List<Tuple<Route, double>> routesAndDistances = The.Sim.World.GetRoutesAndDistances(entity.Site, toSite);
		if (routesAndDistances != null)
		{
			foreach (Tuple<Route, double> item in routesAndDistances)
			{
				_ = item.Item2;
				if (item.Item1 != null && item.Item1.RouteType == routeType)
				{
					return item.Item1.ID;
				}
			}
		}
		return null;
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
		Intelligence intelligence = entity.Intelligence;
		List<EntityGroup> list = new List<EntityGroup>();
		list.Add(entity.PersonEntity.OwnedEntities);
		list.Add(entity.PersonEntity.Household.OwnedEntities);
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(intelligence.Memory.EmigrateTarget.Value);
		if (allegiance != null)
		{
			intelligence.SetTopLevelGoal(new GoalEmigrate(entity, allegiance.Site.ID, allegiance.ID, allegiance.Expeditions[0].ID, mostDesirableTerminal.Value, routeToUse, GetOwnerIDs(list)), bestScore);
			return true;
		}
		return false;
	}
}
