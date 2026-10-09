using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalTakeFive : CompositeGoal, ITopLevelGoal
{
	private Snapshotter.Version version = Snapshotter.Version.Original;

	public const string IdlingText = "Idling";

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalTakeFive(Entity entity)
		: base(entity)
	{
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}

	public GoalTakeFive()
	{
	}

	public double ScoreGoal()
	{
		return GameData.Instance.AIConstants.IdleGoalDesirability;
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	private void MoveShortDistance()
	{
		// MOD: BirdHopMod lets an immobile creature it lists take this same short step - to a free
		// neighbouring cell, weighted back towards where it spawned. See mods/BirdHopMod.cs.
		bool hop = !entity.EntityType.IntelligenceType.IsMobile && UWGame.Mods.BirdHopMod.MayHop(entity);
		if (entity.ContainedBy.HasValue || (!entity.EntityType.IntelligenceType.IsMobile && !hop))
		{
			return;
		}
		// A startled hopper (a person or big creature close by) hops for certain, and away.
		Entity startler = hop ? UWGame.Mods.BirdHopMod.Startler(entity) : null;
		bool startled = startler != null;
		float? chanceToIdleWalkShortDistanceAway = startled ? new float?(1f) : (entity.GetChanceToIdleWalkShortDistanceAway() ?? (hop ? new float?(UWGame.Mods.BirdHopMod.HopChance) : null));
		float moveAbility = entity.Locomotor.MoveAbility;
		if (!chanceToIdleWalkShortDistanceAway.HasValue || !(moveAbility > 0.15f))
		{
			return;
		}
		float num = 1f;
		if (entity.BiologicalEntity != null)
		{
			num = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyIdleWalkFactor, 1f, entity.BiologicalEntity.EnergyLevel);
		}
		if (!startled && !(The.Sim.GameplayRandomGenerator.NextDouble("GoalTakeFive") < (double)(num * chanceToIdleWalkShortDistanceAway.Value * moveAbility)))
		{
			return;
		}
		MovementMap movementMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
		float innerRadius = (startled ? UWGame.Mods.BirdHopMod.StartledMinDistance : (hop ? UWGame.Mods.BirdHopMod.HopMinDistance : (entity.GetShortIdleWalkMinDistance() ?? 48f)));
		float num2 = (hop ? UWGame.Mods.BirdHopMod.HopMaxDistance : (entity.GetShortIdleWalkMaxDistance() ?? 110f));
		int sizeOfMapInSubtiles = (int)(num2 * 2.5f * 0.0625f);
		SubtileInfluence subtileInfluence = new SubtileInfluence(entity.Location.Value, sizeOfMapInSubtiles);
		subtileInfluence.DrawRadius(entity.PlaySiteLocation, innerRadius, num2, 4);
		if (!startled)
		{
			subtileInfluence.DrawDistanceGradientOnInfluenceMap(entityIntelligence.CurrentExpedition.Center.Value, 1f, 4f);
		}
		else
		{
			// A startled bird keeps a gentle pull home (without it, Kastuk: "roaming freely without leash")
			// and may not land near whoever scared it (with the full pull it went straight back to them).
			subtileInfluence.DrawDistanceGradientOnInfluenceMap(entityIntelligence.CurrentExpedition.Center.Value, 1f, 2f);
			subtileInfluence.DrawNegativeInfluenceFromEntities(UWGame.Mods.BirdHopMod.FrightCircle(startler), subtileInfluence.TopLeftSubtilePositionOfMap);
		}
		subtileInfluence.DrawNegativeInfluenceFromEntities(entity, drawStationaryAgents: true, drawItems: true);
		subtileInfluence.BlockOutBlockedSubtiles(movementMap.Layers[SurfaceType.TransportType.Foot], blockReserved: true);
		subtileInfluence.AddWhiteNoise(4);
		List<Tuple<byte, SubtilePos>> bestRelativePositions = subtileInfluence.GetBestRelativePositions(10);
		int num3 = 0;
		foreach (Tuple<byte, SubtilePos> item in bestRelativePositions)
		{
			SubtilePos subtilePos = item.Item2 + new SubtilePos(subtileInfluence.TopLeftSubtilePositionOfMap);
			if (!(MapManager.WorldPosToSubtilePos(entity.PlaySiteLocation) != subtilePos))
			{
				continue;
			}
			// MOD: BirdHopMod - a bird on the water stays on the water, one on land on land.
			if (hop && !UWGame.Mods.BirdHopMod.SameGround(entity, subtilePos.ToPoint()))
			{
				continue;
			}
			float distance = 0f;
			switch (movementMap.Layers[SurfaceType.TransportType.Foot].RegionMap.GetDistance(entity, MapManager.WorldPosToSubtile(entity.AccessPoint.Value), subtilePos.ToPoint(), ref distance, sendMessageToEntity: false))
			{
			case RegionMap.Result.OK:
			{
				Vector3 location = MapManager.SubTileToWorldPos3(subtilePos.ToPoint());
				location = MapManager.VaryLocationWithinSubtile(location);
				AddSubgoal(new GoalMoveToPosition(entity, location, null));
				if (entity.Locomotor.LeggedLocomotor != null)
				{
					entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkSlowly;
				}
				return;
			}
			case RegionMap.Result.NoAccess:
				num3++;
				if (num3 >= 10)
				{
					return;
				}
				break;
			case RegionMap.Result.Wait:
				return;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (entity.Locomotor != null && entity.Locomotor.LeggedLocomotor != null && entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.WalkSlowly)
		{
			entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
		}
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (!entity.EntityType.IntelligenceType.IsMobile)
		{
			// MOD: in the same order as the mobile branch below - the step, then the rest.
			if (UWGame.Mods.BirdHopMod.MayHop(entity))
			{
				MoveShortDistance();
			}
			GoalDoTakeFive goalDoTakeFive = new GoalDoTakeFive(entity);
			goalDoTakeFive.TestForDanger = false;
			AddSubgoal(goalDoTakeFive);
			base.Status = Status.Active;
		}
		else
		{
			if (!ValidateSafetyAndTakeAction(null))
			{
				return;
			}
			DiscomfortMap discomfortMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
			int num = discomfortMap.Map.GetValue(entity.MapPosition.Value);
			if (entity.ContainedBy.HasValue)
			{
				num -= GameData.Instance.AIConstants.ComfortBonusFromBeingInsideBuildingsOrVehicles;
			}
			if (num <= GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart)
			{
				MoveShortDistance();
				AddSubgoal(new GoalDoTakeFive(entity));
				base.Status = Status.Active;
				return;
			}
			if (!entity.GetContainedBy(out Entity container))
			{
				base.Status = Status.Failed;
				return;
			}
			if (container != null)
			{
				AddSubgoal(new GoalExit(entity));
				base.Status = Status.Active;
				return;
			}
			if (entity.IsInsideVehicle())
			{
				AddSubgoal(new GoalExitVehicle(entity, entity.InsideVehicle.Value));
				base.Status = Status.Active;
				return;
			}
			List<PathFinderNode> list = CompositeGoal.FindPathToComfort(discomfortMap, entity, GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart);
			if (list != null)
			{
				if (list.Count > 6)
				{
					int num2 = list.Count / 2;
					list.RemoveRange(list.Count - num2, num2);
				}
				GoalFollowPath goalFollowPath = new GoalFollowPath(entity, list, null, null, null, null);
				goalFollowPath.MovingOutOfHarmsWay = true;
				AddSubgoal(goalFollowPath);
				AddSubgoal(new GoalDoTakeFive(entity));
				base.Status = Status.Active;
			}
			else
			{
				GoalDoTakeFive goalDoTakeFive2 = new GoalDoTakeFive(entity);
				goalDoTakeFive2.TestForDanger = false;
				AddSubgoal(goalDoTakeFive2);
				base.Status = Status.Active;
			}
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
	}

	public override string GetStatus()
	{
		return UWGame.Locale.Text("Idling");
	}
}
