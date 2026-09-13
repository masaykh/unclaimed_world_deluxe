using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateSleep : GoalEvaluator
{
	private enum Progress
	{
		NotStarted,
		ScoreNeed,
		GetLocations,
		ScoreContainers,
		ScoreGroundLocations,
		FindFinalLocation
	}

	public enum GroundSleepArea
	{
		Self,
		Home,
		Expedition
	}

	[DebuggerDisplay("{ContainerEntity}, {GroundLocation}")]
	private class SleepLocation
	{
		public ExpeditionID? ExpeditionID;

		public EntityID? ContainerEntity;

		public WorldLocation? GroundLocation;

		public GroundSleepArea? GroundSleepArea;

		public double Score;

		public double TravelTimeToCenterScore;
	}

	private EntityID? bestContainerToSleepIn;

	private WorldLocation? bestGroundLocationToSleepOn;

	private ExpeditionID? bestExpeditionToSleepAt;

	private GroundSleepArea? bestGroundSleepArea;

	private List<EntityGroup> ownersOfVehicles;

	private List<EntityGroup> otherOwnersOfStructures;

	private Progress progress;

	private List<SleepLocation> containerSleepLocations = new List<SleepLocation>();

	private Dictionary<TilePos, SleepLocation> groundSleepLocations = new Dictionary<TilePos, SleepLocation>();

	private List<SleepLocation> listOfGroundSleepLocations;

	private double sleepNeedsScore;

	private int scoreContainerIndex;

	private int scoreGroundLocationIndex;

	private int finalComboSelectionIndex;

	private bool scoringWasInterrupted;

	private float MaxSleepIncreasePerDay = GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping * GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople;

	private float priority;

	public override float Priority => priority;

	public EvaluateSleep(Entity entity, List<EntityGroup> ownersOfVehicles, List<EntityGroup> otherOwnersOfStructures)
		: base(entity)
	{
		priority = GameData.Instance.AIConstants.PriorityOfNeeds;
		this.ownersOfVehicles = ownersOfVehicles;
		this.otherOwnersOfStructures = otherOwnersOfStructures;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		result = 0.0;
		bestScore = 0.0;
		if (entityIntelligence.IsIndependent() && entityIntelligence.CurrentExpedition.Policy.SleepInShiftsIsActive() && !IsPermittedToSleep())
		{
			return CalculateResult.Done;
		}
		if (progress == Progress.NotStarted)
		{
			if (!NeedsSleep())
			{
				return CalculateResult.Done;
			}
			sleepNeedsScore = 0.0;
			scoreContainerIndex = 0;
			scoreGroundLocationIndex = 0;
			bestGroundLocationToSleepOn = null;
			bestExpeditionToSleepAt = null;
			bestContainerToSleepIn = null;
			containerSleepLocations.Clear();
			groundSleepLocations.Clear();
			progress = Progress.ScoreNeed;
		}
		if (progress == Progress.ScoreNeed)
		{
			ScoreNeed();
			progress = Progress.GetLocations;
		}
		if (progress == Progress.GetLocations)
		{
			GatherAllSleepLocations();
			progress = Progress.ScoreContainers;
		}
		if (progress == Progress.ScoreContainers)
		{
			if (ScoreContainerLocations() == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			progress = Progress.ScoreGroundLocations;
		}
		if (progress == Progress.ScoreGroundLocations)
		{
			if (ScoreGroundLocations() == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			progress = Progress.FindFinalLocation;
		}
		if (progress == Progress.FindFinalLocation)
		{
			SelectBestLocation();
			result = bestScore;
			return CalculateResult.Done;
		}
		return CalculateResult.Done;
	}

	private bool IsPermittedToSleep()
	{
		Expedition currentExpedition = entityIntelligence.CurrentExpedition;
		int numberOfSleepingIndependents = currentExpedition.GetNumberOfSleepingIndependents();
		_ = currentExpedition.IndependentMembers.Count;
		if (numberOfSleepingIndependents >= currentExpedition.NoOfIndependentMembersAllowedToSleep())
		{
			Need need = base.entity.BiologicalEntity.Needs.NeedsList["sleep"];
			if (need.CurrentLevel < GameData.Instance.AIConstants.SleepNeedLimitToSwapWithASleeper)
			{
				foreach (EntityID independentMember in entityIntelligence.CurrentExpedition.IndependentMembers)
				{
					Entity entity = Entity.FindByID(independentMember);
					if (entity != null && entity.Intelligence.IsSleeping() && entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
					{
						return true;
					}
				}
			}
			if (need.CurrentLevel > GameData.Instance.AIConstants.SleepNeedToIgnoreSleepPolicy)
			{
				return false;
			}
		}
		return true;
	}

	private void ScoreNeed()
	{
		sleepNeedsScore = ScoreSleepNeed();
	}

	private void SelectBestLocation()
	{
		containerSleepLocations.AddRange(groundSleepLocations.Values);
		containerSleepLocations.RemoveAll((SleepLocation l) => l.Score == 0.0);
		if (containerSleepLocations.Count > 0)
		{
			containerSleepLocations.Sort((SleepLocation a, SleepLocation b) => b.Score.CompareTo(a.Score));
			SleepLocation sleepLocation = containerSleepLocations[0];
			bestScore = sleepLocation.Score;
			bestContainerToSleepIn = sleepLocation.ContainerEntity;
			bestExpeditionToSleepAt = sleepLocation.ExpeditionID;
			bestGroundLocationToSleepOn = sleepLocation.GroundLocation;
			bestGroundSleepArea = sleepLocation.GroundSleepArea;
		}
		progress = Progress.NotStarted;
	}

	private bool NeedsSleep()
	{
		if (ScoreSleepNeed() < 1.0 && ScoreTimeOfDay() < 0.95)
		{
			return false;
		}
		return true;
	}

	private CalculateResult ScoreContainerLocations()
	{
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		double score = 0.0;
		while (scoreContainerIndex < containerSleepLocations.Count)
		{
			SleepLocation sleepLocation = containerSleepLocations[scoreContainerIndex];
			if (ScoreSleep(sleepNeedsScore, regionMapAndStanceForEvaluator, threatStanceToUse, sleepLocation.ExpeditionID, sleepLocation.ContainerEntity, sleepLocation.GroundLocation, sleepLocation.GroundSleepArea, sleepLocation.TravelTimeToCenterScore, ref score) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			sleepLocation.Score = score;
			scoreContainerIndex++;
		}
		return CalculateResult.Done;
	}

	private CalculateResult ScoreGroundLocations()
	{
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		double score = 0.0;
		while (scoreGroundLocationIndex < listOfGroundSleepLocations.Count)
		{
			SleepLocation sleepLocation = listOfGroundSleepLocations[scoreGroundLocationIndex];
			if (ScoreSleep(sleepNeedsScore, regionMapAndStanceForEvaluator, threatStanceToUse, sleepLocation.ExpeditionID, sleepLocation.ContainerEntity, sleepLocation.GroundLocation, sleepLocation.GroundSleepArea, sleepLocation.TravelTimeToCenterScore, ref score) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			sleepLocation.Score = score;
			scoreGroundLocationIndex++;
		}
		return CalculateResult.Done;
	}

	private CalculateResult ScoreLocation(RegionMap regionMapToUse, ThreatStance threatStanceToUse, ExpeditionID? expeditionToSleepAt, EntityID? containerEntity, WorldLocation? groundLocation, GroundSleepArea? groundSleepArea, double travelTimeToCenterScore, ref double score)
	{
		IKnownEntityData data = null;
		WorldLocation worldLocation;
		if (expeditionToSleepAt.HasValue)
		{
			worldLocation = new WorldLocation(Expedition.FindByID(expeditionToSleepAt.Value).Center.Value);
		}
		else if (containerEntity.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(containerEntity.Value, out data)))
			{
				score = 0.0;
				return CalculateResult.Done;
			}
			worldLocation = new WorldLocation(data.AccessPoint.Value);
		}
		else
		{
			worldLocation = groundLocation.Value;
		}
		double travelTimeScore = 0.0;
		switch (GoalEvaluator.ScoreTravelTime(regionMapToUse, threatStanceToUse, entity.AccessPoint.Value, worldLocation.ToVector3(), entity, ref travelTimeScore))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			score = 0.0;
			return CalculateResult.Done;
		default:
		{
			double score2 = ScoreCrowdedTile(data, expeditionToSleepAt, groundLocation);
			double score3 = ScoreComfort(data);
			double score4 = ScoreSafety(data, groundSleepArea);
			WeightedRating weightedRating = default(WeightedRating);
			weightedRating.AddScore(0.2, travelTimeScore);
			weightedRating.AddScore(0.05, travelTimeToCenterScore);
			weightedRating.AddScore(0.35, score3);
			weightedRating.AddScore(0.1, score2);
			weightedRating.AddScore(0.3, score4);
			score = weightedRating.Result;
			return CalculateResult.Done;
		}
		}
	}

	private double ScoreSafety(IKnownEntityData containerData, GroundSleepArea? area)
	{
		double result = 1.0;
		if (area.HasValue)
		{
			switch (area.Value)
			{
			case GroundSleepArea.Self:
				result = 0.0;
				break;
			case GroundSleepArea.Expedition:
				result = 0.5;
				break;
			case GroundSleepArea.Home:
				result = 0.75;
				break;
			}
		}
		return result;
	}

	private double ScoreComfort(IKnownEntityData containerData)
	{
		GoalDoSleep.ComputeSleepConditions(entity, containerData, out var maximumSleepGainFromThisLocation, out var increaseAmountPerDay);
		double num = ((!((double)maximumSleepGainFromThisLocation >= 1.0)) ? 0.5 : 1.0);
		return Common.Clamp(0.5 * num + 0.5 * (double)increaseAmountPerDay / (double)MaxSleepIncreasePerDay, 0.0, 1.0);
	}

	private double ScoreCrowdedTile(IKnownEntityData ContainerEntity, ExpeditionID? expeditionID, WorldLocation? GroundLocation)
	{
		double result = 1.0;
		if (ContainerEntity != null || expeditionID.HasValue)
		{
			return result;
		}
		TerrainTile tile = The.Map.GetTile(MapManager.WorldPosToTilePos(GroundLocation.Value));
		if (tile.EntitiesOnTile != null)
		{
			int num = 0;
			List<MemoryFact> value;
			if (tile.AllegiancesThatSeeThisTile.Contains(entityIntelligence.Allegiance))
			{
				num = tile.EntitiesOnTile.Sum((Entity e) => (e != entity) ? 1 : 0);
			}
			else if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(entityIntelligence.Allegiance.SharedKnowledge, out value))
			{
				num = value.Count;
			}
			result = 1.0 - (double)num * 0.1;
		}
		return result;
	}

	public CalculateResult ScoreSleep(double? sleepNeedsScore, RegionMap regionMapToUse, ThreatStance? threatStance, ExpeditionID? expeditionToSleepAt, EntityID? containerToSleepIn, WorldLocation? groundLocation, GroundSleepArea? groundSleepArea, double? travelTimeToCenterScore, ref double score)
	{
		bestGroundLocationToSleepOn = null;
		bestContainerToSleepIn = null;
		bestExpeditionToSleepAt = null;
		if (!sleepNeedsScore.HasValue)
		{
			sleepNeedsScore = ScoreSleepNeed();
		}
		if (!travelTimeToCenterScore.HasValue)
		{
			travelTimeToCenterScore = 1.0;
		}
		ThreatStance threatStanceToUse2;
		if (regionMapToUse == null || !threatStance.HasValue)
		{
			regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out var threatStanceToUse);
			threatStanceToUse2 = threatStanceToUse;
		}
		else
		{
			threatStanceToUse2 = threatStance.Value;
		}
		double num = ScoreTimeOfDay();
		double score2 = 0.0;
		double? num2 = sleepNeedsScore;
		double num3 = 1.0;
		if (num2.GetValueOrDefault() < num3 && num2.HasValue && num < entity.EntityType.BiologicalType.TimeOfDayToGoToSleep)
		{
			score = 0.0;
			return CalculateResult.Done;
		}
		if (ScoreLocation(regionMapToUse, threatStanceToUse2, expeditionToSleepAt, containerToSleepIn, groundLocation, groundSleepArea, travelTimeToCenterScore.Value, ref score2) == CalculateResult.Processing)
		{
			return CalculateResult.Processing;
		}
		if (Common.IsZero(score2))
		{
			score = 0.0;
			return CalculateResult.Done;
		}
		WeightedRating weightedRating = default(WeightedRating);
		weightedRating.AddScore(0.4000000059604645, score2);
		weightedRating.AddScore(0.10000000149011612, num);
		weightedRating.AddScore(0.5, sleepNeedsScore.Value);
		score = weightedRating.Result;
		score = ApplyPriority(score);
		return CalculateResult.Done;
	}

	private void GatherAllSleepLocations()
	{
		containerSleepLocations.Clear();
		TilePos? tilePos = null;
		if (entityIntelligence.CurrentExpedition != null)
		{
			TilePos key = MapManager.WorldPosToTilePos(entityIntelligence.CurrentExpedition.Center.Value);
			groundSleepLocations.Add(key, new SleepLocation
			{
				ExpeditionID = entityIntelligence.CurrentExpedition.ID,
				GroundSleepArea = GroundSleepArea.Expedition,
				TravelTimeToCenterScore = HaulingJobManager.ScoreTravelTimeToCenter(entity.PlaySiteLocation.ToPoint(), entityIntelligence.CurrentExpedition.Center.Value.ToPoint())
			});
		}
		if (entity.EntityType.Person != null && entity.PersonEntity.Household != null && entity.PersonEntity.Household.Home.HasValue)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entity.PersonEntity.Household.Home.Value, out var data)))
			{
				containerSleepLocations.Add(new SleepLocation
				{
					ContainerEntity = entity.PersonEntity.Household.Home,
					TravelTimeToCenterScore = 1.0
				});
				tilePos = MapManager.WorldPosToTilePos(data.PlaySiteLocation);
				GatherGroundSleepLocations(tilePos.Value, 3, GroundSleepArea.Home);
			}
			else
			{
				entity.PersonEntity.Household.Home = null;
			}
		}
		if (otherOwnersOfStructures != null)
		{
			foreach (EntityGroup otherOwnersOfStructure in otherOwnersOfStructures)
			{
				foreach (KeyValuePair<EntityType, List<EntityID>> structure in otherOwnersOfStructure.Structures)
				{
					if (structure.Key.GatheringSiteType == null)
					{
						continue;
					}
					foreach (EntityID item in structure.Value)
					{
						containerSleepLocations.Add(new SleepLocation
						{
							ContainerEntity = item,
							TravelTimeToCenterScore = 1.0
						});
					}
				}
			}
		}
		tilePos = MapManager.WorldPosToTilePos(entity.Location.Value);
		GatherGroundSleepLocations(tilePos.Value, 2, GroundSleepArea.Self);
		listOfGroundSleepLocations = groundSleepLocations.Values.ToList();
	}

	private void GatherGroundSleepLocations(TilePos center, int radius, GroundSleepArea sleepArea)
	{
		MapManager.GetClampedMapAreaUsingTiles(center, radius, out var minX, out var maxX, out var minY, out var maxY);
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		_ = entityIntelligence.Allegiance;
		EntityGroup entityGroup = null;
		EntityGroup entityGroup2 = null;
		if (entity.Intelligence.CurrentExpedition != null)
		{
			entityGroup2 = entity.Intelligence.CurrentExpedition.OwnedEntities;
		}
		if (entity.PersonEntity != null)
		{
			entityGroup = entity.PersonEntity.Household.OwnedEntities;
		}
		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				TilePos tilePos = new TilePos(i, j);
				SubtilePos subtilePos = MapManager.TileCenterToSubTile(tilePos);
				if (!The.Map.TileIsCompletelyBlocked(mapCosts, tilePos.ToPoint()) && !The.Map.SubtileIsCompletelyBlocked(mapCosts, subtilePos) && !The.Map.FlagIsSet(subtilePos, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved))
				{
					TerrainTile tile = The.Map.GetTile(tilePos);
					if ((tile.GeoLayoutEntitiesOnTile == null || tile.GeoLayoutEntitiesOnTile.Count == 0) && (!tile.Owner.HasValue || (entityGroup2 != null && tile.Owner == entityGroup2.ID) || (entityGroup != null && tile.Owner == entityGroup.ID)) && (entity.EntityType.Person == null || tile.GetStockpileZone(entityIntelligence.Allegiance) == null) && !groundSleepLocations.ContainsKey(tile.TilePos))
					{
						groundSleepLocations.Add(tile.TilePos, new SleepLocation
						{
							GroundLocation = MapManager.TilePosToWorldLocation(tilePos),
							GroundSleepArea = sleepArea,
							TravelTimeToCenterScore = HaulingJobManager.ScoreTravelTimeToCenter(tilePos.ToPoint(), center.ToPoint())
						});
					}
				}
			}
		}
	}

	private RegionMap.Result ScoreDistanceFromSleepLocation(Vector3 sleepLocation, out double distanceScore)
	{
		distanceScore = 0.0;
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		Point fromSubtile = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
		Point toSubtile = MapManager.WorldPosToSubtile(sleepLocation);
		float distance = -1f;
		RegionMap.Result distance2 = regionMapAndStanceForEvaluator.GetDistance(entity, fromSubtile, toSubtile, ref distance);
		if (distance2 != RegionMap.Result.OK)
		{
			return distance2;
		}
		if (distance > 400f)
		{
			distanceScore = Common.Clamp(distance * 0.0001f, 0f, 1f);
		}
		else
		{
			distanceScore = 0.0;
		}
		return distance2;
	}

	public double ScoreSleepNeed()
	{
		Need need = entity.BiologicalEntity.Needs.NeedsList["sleep"];
		return 0.9 * (1.0 - (double)need.CurrentLevel) + (double)MathHelper.Lerp(0f, 0.1f, 10f * need.PhysicalNeed.DaysAtZero / (need.NeedType.PhysicalEffects.DaysAtZeroCausingDeath ?? 1f));
	}

	private void WakeSomeoneUpIfNeeded()
	{
		Expedition currentExpedition = entityIntelligence.CurrentExpedition;
		if (!currentExpedition.Policy.SleepInShiftsIsActive() || currentExpedition.GetNumberOfSleepingIndependents() < currentExpedition.NoOfIndependentMembersAllowedToSleep())
		{
			return;
		}
		_ = base.entity.BiologicalEntity.Needs.NeedsList["sleep"];
		Entity entity = null;
		foreach (EntityID independentMember in currentExpedition.IndependentMembers)
		{
			Entity entity2 = Entity.FindByID(independentMember);
			if (entity2 == null || entity2 == base.entity || !entity2.Intelligence.IsSleeping())
			{
				continue;
			}
			float currentLevel = entity2.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel;
			if (!(currentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead))
			{
				continue;
			}
			if (entity != null)
			{
				if (entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel < currentLevel)
				{
					entity = entity2;
				}
			}
			else
			{
				entity = entity2;
			}
		}
		if (entity != null)
		{
			Message msg = new Message(Message.MessageTypes.StopGoalSleep);
			entity.SendMessage(msg);
		}
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
		WakeSomeoneUpIfNeeded();
		intelligence.SetTopLevelGoal(new GoalSleep(entity, bestGroundLocationToSleepOn, bestExpeditionToSleepAt, bestContainerToSleepIn, bestGroundSleepArea, GetOwnerIDs(ownersOfVehicles))
		{
			GoalEvaluator = this
		}, bestScore);
		return true;
	}
}
