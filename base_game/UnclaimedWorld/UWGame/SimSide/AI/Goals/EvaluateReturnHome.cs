using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateReturnHome : GoalEvaluator
{
	private List<EntityGroup> ownersOfVehicles;

	private bool useBoldStance;

	private Vector3? teleportTo;

	private Regulator trappedDetectionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.01, "EvaluateReturnHome");

	public override float Priority => 1f;

	public EvaluateReturnHome(Entity entity, List<EntityGroup> ownersOfVehicles = null)
		: base(entity)
	{
		this.ownersOfVehicles = ownersOfVehicles;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		_ = entity.Name == "Ward Conlan";
		if (entityIntelligence.CurrentExpedition == null)
		{
			result = 0.0;
			return CalculateResult.Done;
		}
		if (ScoreIdleTime(out var isPastDesperationTime, out var isPastTeleportTime) > 0.3)
		{
			useBoldStance = false;
			if (isPastDesperationTime)
			{
				useBoldStance = true;
			}
			double distanceScore;
			switch (ScoreDistanceFromHome(useBoldStance, out distanceScore))
			{
			case RegionMap.Result.Wait:
				return CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				if (DoTeleport(isPastTeleportTime))
				{
					distanceScore = 1.0;
					break;
				}
				result = (bestScore = 0.0);
				return CalculateResult.Done;
			}
			if (distanceScore > 0.0)
			{
				bestScore = 1.1 * (GameData.Instance.AIConstants.IdleGoalDesirability + (double)GameData.Instance.AIConstants.CurrentGoalInertia) + (double)(1f - entity.Intelligence.Morale);
			}
			else
			{
				bestScore = 0.0;
			}
		}
		else
		{
			bestScore = 0.0;
		}
		result = bestScore;
		return CalculateResult.Done;
	}

	private bool DoTeleport(bool isPastTeleportTime)
	{
		if (entity.EntityType.IntelligenceType.AllowEscapeFromTinyAreas == true && isPastTeleportTime && trappedDetectionRegulator.IsReady() && IsBlockedInTinyArea())
		{
			teleportTo = FindTeleportLocation();
			return true;
		}
		return false;
	}

	private bool IsBlockedInTinyArea()
	{
		FloodFill floodFill = new FloodFill();
		SubtileLayers terrainGrid = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		int num = (int)(GameData.Instance.AIConstants.MaximumRadiusOfTrapAreaToTeleportFrom / 16f);
		Point point = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
		Point subtilePosition = new Point(point.X - num, point.Y - num);
		int widthInSubTiles = 2 * num;
		int heightInSubTiles = 2 * num;
		Rectangle clampedMapAreaUsingSubTiles = The.Map.GetClampedMapAreaUsingSubTiles(subtilePosition, widthInSubTiles, heightInSubTiles);
		ushort[][] map = null;
		byte[][] map2 = null;
		Common.InitJaggedArray(ref map, clampedMapAreaUsingSubTiles.Width, clampedMapAreaUsingSubTiles.Height);
		Common.InitJaggedArray(ref map2, clampedMapAreaUsingSubTiles.Width, clampedMapAreaUsingSubTiles.Height);
		if (floodFill.DoFloodFillOfArea(terrainGrid, map, MapManager.WorldPosToSubtile(entity.AccessPoint.Value), clampedMapAreaUsingSubTiles, map2, num) == FloodFill.FloodFillResult.MaxRadiusReached)
		{
			return false;
		}
		return true;
	}

	private Vector3? FindTeleportLocation()
	{
		Vector3 value = entity.AccessPoint.Value;
		RegionMap regionMap = The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap;
		Point destination = GetHomeDestination();
		Vector3 vector = MapManager.FindUnblockedLocation(value, GameData.Instance.AIConstants.MaximumRadiusOfTrapAreaToTeleportFrom, MapManager.ScanMethod.HalfCircle, entityIntelligence.CurrentExpedition.Center, avoidReservedSubtiles: true, (SubtilePos s) => SubtileCanReachDestination(s, regionMap, destination));
		if (MapManager.WorldPosToSubtile(value) == MapManager.WorldPosToSubtile(vector))
		{
			return null;
		}
		return vector;
	}

	private bool SubtileCanReachDestination(SubtilePos pos, RegionMap regionMap, Point destinationSubtilePos)
	{
		float distance = -1f;
		if (regionMap.GetDistance(entity, pos.ToPoint(), destinationSubtilePos, ref distance) != RegionMap.Result.OK)
		{
			return false;
		}
		return true;
	}

	private RegionMap.Result ScoreDistanceFromHome(bool useBoldStance, out double distanceScore)
	{
		distanceScore = 0.0;
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse, useBoldStance);
		Point fromSubtile = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
		Point homeDestination = GetHomeDestination();
		float distance = -1f;
		RegionMap.Result distance2 = regionMapAndStanceForEvaluator.GetDistance(entity, fromSubtile, homeDestination, ref distance);
		if (distance2 != RegionMap.Result.OK)
		{
			return distance2;
		}
		if (distance > GameData.Instance.AIConstants.DistanceFromExpeditionToReturnHome)
		{
			distanceScore = Common.Clamp(distance * 0.0001f, 0f, 1f);
		}
		else
		{
			distanceScore = 0.0;
		}
		return distance2;
	}

	private Point GetHomeDestination()
	{
		return MapManager.WorldPosToSubtile(entityIntelligence.CurrentExpedition.Center.Value);
	}

	private double ScoreIdleTime(out bool isPastDesperationTime, out bool isPastTeleportTime)
	{
		double totalSeconds = entityIntelligence.Memory.TimeSpentIdling.TotalSeconds;
		double num = GameData.Instance.AIConstants.TimeSpentIdlingToConsiderReturningHome * entity.Intelligence.Morale;
		isPastDesperationTime = false;
		isPastTeleportTime = false;
		if (totalSeconds > num)
		{
			if (totalSeconds > (double)GameData.Instance.AIConstants.TimeSpentIdlingToConsiderReturningHomeWithBoldStance)
			{
				isPastDesperationTime = true;
			}
			if (totalSeconds > (double)GameData.Instance.AIConstants.TimeSpentIdlingToConsiderTeleporting)
			{
				isPastTeleportTime = true;
			}
			return Common.Clamp((totalSeconds - num) / (4.0 * num), 0.0, 1.0);
		}
		return 0.0;
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
		entityIntelligence.SetTopLevelGoal(new GoalReturnHome(entity, GetOwnerIDs(ownersOfVehicles), useBoldStance, teleportTo), bestScore);
		teleportTo = null;
		return true;
	}
}
