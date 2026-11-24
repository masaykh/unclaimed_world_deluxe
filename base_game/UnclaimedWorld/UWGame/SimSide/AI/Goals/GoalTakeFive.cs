using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
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
			if (TryFlyHome())
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
				AddSubgoal(GoalExit.GetContainerExitGoal(entity)); // PORT: a vehicle lands before it is left
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

	/// <summary>
	/// BASE: an idle pilot who is still in the air flies home instead of hovering until
	/// GoalDoTakeFive's ContainerIsTooCrowded puts him out, wherever he happens to be (issue #25).
	/// Home is the nearest free helipad, for an aircraft that uses them (Lars: "Return to base
	/// should prioritize helipads"); else the standable tile nearest the expedition's centre. He
	/// lands and takes five there, which gets him out at home.
	///
	/// A job that turns up on the way takes him straight from the air: ProcessWhileActive
	/// arbitrates while he flies, so he does not land first and take off again (Lars's other point).
	/// A job that exists when his last one ends needs nothing new - GoalThink arbitrates over every
	/// evaluator before it settles on idling, and GoalMoveToPosition flies an airborne pilot onwards.
	/// No state of its own: the subgoals are saved, and a loaded game finds him already flying.
	/// </summary>
	private bool TryFlyHome()
	{
		if (!entity.DrivingVehicle.HasValue)
		{
			return false;
		}
		Entity aircraft = Entity.FindByID(entity.DrivingVehicle.Value);
		if (aircraft == null || !aircraft.MapPosition.HasValue || !GoalExitVehicle.IsAirborne(aircraft))
		{
			return false;
		}
		// NEW (issue #33): home is StoppingSpot's ReturnHome - the nearest helipad of the kind this
		// aircraft uses (VehicleContainerType.CanUseTerminal) that nobody has reserved and his side
		// knows no vehicle on, else a spot near the expedition's centre. Reserved, so two pilots
		// flying home at once no longer choose the same pad, and re-checked on the way.
		Expedition expedition = entityIntelligence.CurrentExpedition;
		if (expedition == null)
		{
			return false;
		}
		Point near = expedition.Center.HasValue ? MapManager.WorldPosToTile(expedition.Center.Value) : aircraft.MapPosition.Value;
		if (!StoppingSpot.Find(entity, aircraft, near, StopPurpose.ReturnHome, out Vector3 spot))
		{
			return false;
		}
		Point home = MapManager.WorldPosToTile(spot);
		if (home != aircraft.MapPosition.Value)
		{
			AddSubgoal(new GoalFlyToPosition(entity, home, StopPurpose.ReturnHome, near));
		}
		AddSubgoal(new GoalLand(entity));

		base.Status = Status.Active;
		return true;
	}

	/// <summary>
	/// The tile of the terminal nearest <paramref name="from"/> that is not taken; the first of
	/// equals in list order, so a replay picks the same one. Null when every one is taken, or there
	/// are none. StoppingSpot ranks terminals with it.
	/// </summary>
	public static Point? ChooseTerminalTile(IList<Vector3> terminals, Vector3 from, Func<Point, bool> taken)
	{
		Point? best = null;
		float bestDistance = float.MaxValue;
		foreach (Vector3 location in terminals)
		{
			Point tile = MapManager.WorldPosToTile(location);
			if (taken(tile))
			{
				continue;
			}
			float distance = Vector2.DistanceSquared(location.ToVector2(), from.ToVector2());
			if (distance < bestDistance)
			{
				best = tile;
				bestDistance = distance;
			}
		}
		return best;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		// BASE: see TryFlyHome. While flying home, look for work as a busy colonist does, at the
		// slower while-busy rate: GoalTakeFive's inertia starts at zero, so EvaluateTakeFive can win
		// against it, and each such restart only re-issues the same flight.
		if (Subgoals.Count > 0 && Subgoals.Peek() is GoalFlyToPosition && entityIntelligence.Brain.ArbitrateWhileBusy(out var _))
		{
			HandleSubstitutedGoalByArbitrator();
			return;
		}
		base.Status = ProcessSubgoals(elapsed);
	}

	public override string GetStatus()
	{
		return UWGame.Locale.Text("Idling");
	}
}
