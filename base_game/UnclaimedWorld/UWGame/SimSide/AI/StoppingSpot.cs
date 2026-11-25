using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI;

/// <summary>
/// What a vehicle is stopping for. The ranking of spots depends on it (Lars: "This heuristic
/// depends on what the agent is doing").
/// </summary>
public enum StopPurpose
{
	/// <summary>On the way to a destination: nearest to it. A terminal (helipad) wins only near it.</summary>
	Travel,
	/// <summary>Nothing to do: a terminal of the vehicle's kind first, else near the camp.</summary>
	ReturnHome,
	/// <summary>Down now (the pilot was told to get out): nearest to where the vehicle is.</summary>
	PutDown,
	/// <summary>A ground vehicle parking at its destination: VehicleContainer.FindParkingSpot's scoring.</summary>
	Park,
	/// <summary>
	/// That exact spot or nothing - medevac, lifting a heavy load (Lars, Discord 2026-11-24). If
	/// it is unsuitable, or goes bad on the way, there is no other: the flight fails.
	/// Last in the enum: GoalFlyToPosition saves the purpose as its number.
	/// </summary>
	TravelPrecise
}

/// <summary>
/// NEW (issue #33): the one place that decides where a vehicle lands or parks, and the
/// per-allegiance reservation of that spot (SharedKnowledge.StopLocks).
///
/// Before this, four places chose, each its own way: GoalMoveToPosition landed on the destination
/// tile whatever stood there (the studio's "TODO: Find Landing spot!"), GoalExitVehicle on the
/// nearest standable tile, GoalTakeFive on the nearest helipad none of OUR vehicles stood on, and
/// ground vehicles parked through FindParkingSpot. None reserved anything, so two vehicles could
/// pick the same spot, and none looked again once chosen.
///
/// A spot is acceptable when (Lars, Discord 2026-11-24):
/// - the driver's movement map for his stance does not block it - the same map his paths use,
///   which already holds the threats his side knows about;
/// - unless he is Bold, its discomfort is not above his panic level, as
///   CompositeGoal.ValidateSafetyAndTakeAction judges it;
/// - no vehicle of his side, or of any side not at war with his, has reserved it (Lars: "the
///   reservation system should be made so it works for non-hostile allegiances"), and his side
///   knows of no vehicle, of ANY allegiance, on the ground there. Only known entities: "No
///   psychic checks!" A reservation is announced, not seen, so other sides' locks are not psychic.
/// Two spots clash when they are nearer than the two vehicles' radii added together, so a lock
/// covers the vehicle's footprint and not one tile.
/// </summary>
public static class StoppingSpot
{
	// How often a vehicle on its way looks at its spot again (issue #33, question 2; Lars: "that
	// seems OK"). Far
	// out every RecheckFarSeconds of GAME time; inside RecheckNearDistance - where
	// GoalFlyToPosition lowers the landing gear - every RecheckNearSeconds. Game time, not
	// updates: the sim runs at the monitor's rate (see the creek jam, ea479e6).
	public const float RecheckFarSeconds = 2f;
	public const float RecheckNearSeconds = 0.5f;
	public const float RecheckNearDistance = 180f;

	// On the way somewhere, a free terminal of the vehicle's kind wins over open ground only within
	// this many tiles of the destination (issue #33, question 4; Lars: "5 tiles?").
	public const int TravelTerminalTiles = 5;

	// OPEN (issue #33): below this AllegianceRelation.Relation two sides are at war, and neither
	// sees the other's reservations. The studio's data uses 0 for hostile, 0.5 for neutral and 1
	// for friendly ("0 war/hate, 1: alliance?" in AllegianceRelation), so neutral sides share.
	// Two sides with no relation at all share too.
	public const float HostileRelationBelow = 0.25f;

	// How long the driver's remark stays over his head when no spot is found.
	private const float RemarkSeconds = 4f;

	// A lock whose vehicle has no driver and is further than this from its spot is not going there.
	private const float AtSpotDistance = MapManager.tileSize;

	/// <summary>
	/// Chooses a spot near <paramref name="near"/> for <paramref name="purpose"/>, and reserves it
	/// for the vehicle in the driver's allegiance, replacing any spot it held. False when there is
	/// none; nothing is reserved then.
	/// </summary>
	public static bool Find(Entity driver, Entity vehicle, Point near, StopPurpose purpose, out Vector3 spot)
	{
		spot = Vector3.Zero;
		SharedKnowledge knowledge = KnowledgeOf(driver);
		if (knowledge == null || vehicle == null || !vehicle.MapPosition.HasValue)
		{
			return false;
		}

		if (purpose == StopPurpose.Park)
		{
			if (!FindParking(driver, vehicle, knowledge, near, out spot))
			{
				return false;
			}
		}
		else
		{
			Func<Point, bool> usable = tile => IsSuitable(driver, vehicle, MapManager.TileToWorldPos(tile));
			Point? tile = null;
			switch (purpose)
			{
				case StopPurpose.Travel:
					tile = ChooseTerminal(driver, vehicle, knowledge, MapManager.TileToWorldPos(near), TravelTerminalTiles, usable)
						?? GoalExitVehicle.FindLandingTile(near, usable, GoalExitVehicle.LandingSearchRadius);
					break;
				case StopPurpose.ReturnHome:
					tile = ChooseTerminal(driver, vehicle, knowledge, vehicle.PlaySiteLocation, null, usable)
						?? GoalExitVehicle.FindLandingTile(near, usable, GoalExitVehicle.LandingSearchRadius);
					break;
				case StopPurpose.PutDown:
					tile = GoalExitVehicle.FindLandingTile(near, usable, GoalExitVehicle.LandingSearchRadius);
					break;
				case StopPurpose.TravelPrecise:
					tile = usable(near) ? near : null;
					break;
			}
			if (!tile.HasValue)
			{
				return false;
			}
			spot = MapManager.TileToWorldPos(tile.Value);
		}

		Reserve(knowledge, vehicle, spot);
		return true;
	}

	/// <summary>
	/// Whether the spot is still acceptable for this vehicle - the test Find applies to each
	/// candidate, used again on the way in. Its own reservation does not count against it.
	/// </summary>
	public static bool IsSuitable(Entity driver, Entity vehicle, Vector3 spot)
	{
		SharedKnowledge knowledge = KnowledgeOf(driver);
		if (knowledge == null || vehicle == null)
		{
			return false;
		}
		Point tile = MapManager.WorldPosToTile(spot);
		return IsSafe(driver, vehicle, knowledge, tile, MapManager.WorldPosToSubtile(spot))
			&& !IsTaken(driver.Intelligence.Allegiance, knowledge, vehicle, spot);
	}

	/// <summary>The spot this vehicle has reserved with the driver's allegiance, if any.</summary>
	public static bool TryGetReserved(Entity driver, Entity vehicle, out Vector3 spot)
	{
		spot = Vector3.Zero;
		SharedKnowledge knowledge = KnowledgeOf(driver);
		return knowledge != null && vehicle != null && knowledge.StopLocks.TryGetValue(vehicle.EntityID, out spot);
	}

	/// <summary>Reserves the spot again - for a vehicle whose lock an older save did not have.</summary>
	public static void Reserve(Entity driver, Entity vehicle, Vector3 spot)
	{
		SharedKnowledge knowledge = KnowledgeOf(driver);
		if (knowledge != null && vehicle != null)
		{
			Reserve(knowledge, vehicle, spot);
		}
	}

	/// <summary>The vehicle is leaving its spot, or giving it up.</summary>
	public static void Release(Entity driver, Entity vehicle)
	{
		SharedKnowledge knowledge = KnowledgeOf(driver);
		if (knowledge != null && vehicle != null)
		{
			knowledge.StopLocks.Remove(vehicle.EntityID);
		}
	}

	/// <summary>Seconds of game time to the next look at the spot, from this far away.</summary>
	public static float RecheckInterval(float distanceToSpot)
	{
		return distanceToSpot < RecheckNearDistance ? RecheckNearSeconds : RecheckFarSeconds;
	}

	/// <summary>Two vehicles at these locations, with these radii, would touch. Height is ignored.</summary>
	public static bool Overlaps(Vector3 a, float radiusA, Vector3 b, float radiusB)
	{
		float reach = radiusA + radiusB;
		return Vector2.DistanceSquared(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y)) < reach * reach;
	}

	/// <summary>
	/// A reservation nobody is coming to use: its vehicle has left the play site, or has no driver
	/// and is not on the spot. That covers a pilot who died or got out on the way, and a trader
	/// who flew off, so no hook is needed in every place that can happen; a removed vehicle drops
	/// out of StopLocks with the rest of its knowledge.
	/// </summary>
	public static bool IsStale(bool holderOnPlaySite, bool holderHasDriver, Vector3 holderLocation, Vector3 spot)
	{
		return !holderOnPlaySite
			|| (!holderHasDriver
				&& Vector2.Distance(new Vector2(holderLocation.X, holderLocation.Y), new Vector2(spot.X, spot.Y)) > AtSpotDistance);
	}

	/// <summary>
	/// Two sides at war, by AllegianceRelation.Relation - null when they have none. Pure, for the
	/// tests; IsHostile looks the relation up.
	/// </summary>
	public static bool IsHostileRelation(float? relation)
	{
		return relation.HasValue && relation.Value < HostileRelationBelow;
	}

	/// <summary>Whether <paramref name="other"/>'s reservations are hidden from <paramref name="own"/>.</summary>
	public static bool IsHostile(Allegiance own, Allegiance other)
	{
		if (own == null || other == null || own == other)
		{
			return false;
		}
		AllegianceRelation relation = The.Sim?.World?.GetRelationBetweenAllegianceIDs(own.ID, other.ID);
		return IsHostileRelation(relation?.Relation);
	}

	/// <summary>
	/// The driver says there is nowhere to stop, when a flight or a landing is given up for it
	/// (Lars: "let the pilot or driver utter something"). Only the player hears it, and only from
	/// someone his side can see - Client.SpeakLine decides that.
	/// </summary>
	public static void SayNoSpot(Entity driver)
	{
		driver?.Intelligence?.Remark("noStoppingSpot", UWGame.Locale.Text("Nowhere safe to set down here."), RemarkSeconds);
	}

	/// <summary>
	/// NEW (issue #33): where a mission arriving from another site comes down. Replaces taking the
	/// first working terminal whatever stood on it (TravelAction.LocateLandingAreaInExpedition): the
	/// terminal asked for if it is free, else another free one of the vehicle's kind, else open
	/// ground near them - so a trader never has to turn back because a pad is busy (Lars: "Let's
	/// allow the traders to land outside the helipads too"). The spot is reserved for the vehicle
	/// in the traveller's allegiance, which the host respects unless they are at war.
	///
	/// The traveller is off the play site and knows nothing of what stands there, so the HOST's
	/// knowledge says which vehicles are on the ground - the pad is the host's.
	/// False when there is no vehicle or nowhere at all; the caller keeps the studio's fallback.
	/// </summary>
	public static bool FindArrival(Allegiance traveller, Allegiance host, Entity vehicle, Vector3? askedTerminal,
		IList<Vector3> terminals, Vector3 openGroundNear, out Vector3 spot)
	{
		spot = Vector3.Zero;
		SharedKnowledge seeing = host?.SharedKnowledge;
		if (traveller?.SharedKnowledge == null || seeing == null || vehicle == null)
		{
			return false;
		}
		Func<Vector3, bool> free = location => !IsTaken(traveller, seeing, vehicle, location);

		Vector3? chosen = null;
		if (askedTerminal.HasValue && free(askedTerminal.Value))
		{
			chosen = askedTerminal.Value;
		}
		if (!chosen.HasValue && terminals != null)
		{
			foreach (Vector3 terminal in terminals)
			{
				if (free(terminal))
				{
					chosen = terminal;
					break;
				}
			}
		}
		if (!chosen.HasValue)
		{
			Vector3 from = askedTerminal ?? (terminals != null && terminals.Count > 0 ? terminals[0] : openGroundNear);
			Point? tile = GoalExitVehicle.FindLandingTile(MapManager.WorldPosToTile(from),
				t => IsOpenGround(t) && free(MapManager.TileToWorldPos(t)), GoalExitVehicle.LandingSearchRadius);
			if (!tile.HasValue)
			{
				return false;
			}
			chosen = MapManager.TileToWorldPos(tile.Value);
		}
		spot = chosen.Value;
		Reserve(traveller.SharedKnowledge, vehicle, spot);
		return true;
	}

	/// <summary>
	/// The terminals no nearer than <paramref name="maxTiles"/> to <paramref name="from"/> (all
	/// when null), in their original order - the filter Travel puts before
	/// GoalTakeFive.ChooseTerminalTile.
	/// </summary>
	public static List<Vector3> TerminalsWithin(IList<Vector3> terminals, Vector3 from, int? maxTiles)
	{
		List<Vector3> result = new List<Vector3>();
		float limit = maxTiles.HasValue ? maxTiles.Value * MapManager.tileSize : float.MaxValue;
		foreach (Vector3 location in terminals)
		{
			if (Vector2.Distance(new Vector2(location.X, location.Y), new Vector2(from.X, from.Y)) <= limit)
			{
				result.Add(location);
			}
		}
		return result;
	}

	private static SharedKnowledge KnowledgeOf(Entity driver)
	{
		return driver?.Intelligence?.Allegiance?.SharedKnowledge;
	}

	private static Point? ChooseTerminal(Entity driver, Entity vehicle, SharedKnowledge knowledge, Vector3 from, int? maxTiles, Func<Point, bool> usable)
	{
		Expedition expedition = driver.Intelligence.CurrentExpedition;
		TerminalType.TypesOfTerminal? kind = (vehicle.EntityType.ContainerType as VehicleContainerType)?.CanUseTerminal;
		if (expedition == null || !kind.HasValue)
		{
			return null;
		}
		List<IKnownEntityData> terminals = expedition.GetWorkingTerminals(knowledge, kind.Value);
		if (terminals == null)
		{
			return null;
		}
		List<Vector3> locations = TerminalsWithin(terminals.ConvertAll((IKnownEntityData t) => t.PlaySiteLocation), from, maxTiles);
		return GoalTakeFive.ChooseTerminalTile(locations, from, tile => !usable(tile));
	}

	private static bool FindParking(Entity driver, Entity vehicle, SharedKnowledge knowledge, Point near, out Vector3 spot)
	{
		Intelligence intelligence = driver.Intelligence;
		List<(Vector3 location, float radius)> reserved = new List<(Vector3, float)>();
		foreach (KeyValuePair<EntityID, Vector3> held in VisibleLocks(intelligence.Allegiance))
		{
			if (LiveLockRadius(held, vehicle, out float radius))
			{
				reserved.Add((held.Value, radius));
			}
		}
		Rectangle area = VehicleContainer.GetSurroundingAreaUsingEntityRadius(vehicle, near);
		return VehicleContainer.FindParkingSpot(vehicle.MapPosition.Value, intelligence.Allegiance, vehicle, area,
			((VehicleContainerType)vehicle.EntityType.ContainerType).Transport, intelligence.ProtectionLevel, driver.EntityType,
			intelligence.ThreatStance, near, out spot,
			tile => !IsSafe(driver, vehicle, knowledge, tile, null), reserved);
	}

	/// <summary>
	/// The movement map and discomfort half of the test. <paramref name="subtile"/> is the exact
	/// spot when there is one; a whole tile is judged by its centre.
	/// </summary>
	private static bool IsSafe(Entity driver, Entity vehicle, SharedKnowledge knowledge, Point tile, Point? subtile)
	{
		if (tile.X < 0 || tile.Y < 0 || tile.X >= The.Map.mapTileWidth || tile.Y >= The.Map.mapTileHeight)
		{
			return false;
		}
		Intelligence intelligence = driver.Intelligence;

		// An aircraft is judged for the pilot who gets out of it; a ground vehicle for itself.
		SurfaceType.TransportType transport = (vehicle.EntityType.ContainerType as VehicleContainerType)?.Transport ?? SurfaceType.TransportType.Foot;
		if (transport == SurfaceType.TransportType.Air)
		{
			transport = SurfaceType.TransportType.Foot;
		}
		// Both maps live in PlaySiteKnowledge, which a side away from the play site does not have:
		// then the terrain alone decides, as it did before.
		bool onPlaySite = knowledge.PlaySiteKnowledge != null;
		MovementMap moveMap = onPlaySite ? knowledge.GetMovementMap(intelligence.ProtectionLevel, driver.EntityType, intelligence.ThreatStance) : null;
		SubtileLayers layer = moveMap != null && moveMap.Layers != null && moveMap.Layers.TryGetValue(transport, out SubtileLayers own)
			? own
			: The.Map.TerrainCosts[transport];
		Point at = subtile ?? MapManager.TileToUpperLeftSubtile(tile) + new Point(1, 1);
		if (The.Map.SubtileIsCompletelyBlocked(layer, at))
		{
			return false;
		}

		if (onPlaySite && intelligence.ThreatStance != ThreatStance.Bold)
		{
			DiscomfortMap discomfort = knowledge.GetDiscomfortMap(intelligence.ProtectionLevel, driver.EntityType, intelligence.ThreatStance);
			if (discomfort != null && discomfort.Map.GetValue(tile) > intelligence.PanicLevel)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Reserved by <paramref name="own"/> or a side not at war with it, or a vehicle
	/// <paramref name="knowledge"/> knows of is on the ground there.
	/// </summary>
	private static bool IsTaken(Allegiance own, SharedKnowledge knowledge, Entity vehicle, Vector3 spot)
	{
		float radius = vehicle.BoundingRadius3D;
		foreach (KeyValuePair<EntityID, Vector3> held in VisibleLocks(own))
		{
			if (LiveLockRadius(held, vehicle, out float heldRadius) && Overlaps(spot, radius, held.Value, heldRadius))
			{
				return true;
			}
		}

		List<EntityID> known = knowledge.AllKnownEntities?.Vehicles;
		if (known != null)
		{
			foreach (EntityID id in known)
			{
				if (id == vehicle.EntityID
					|| GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(id, out IKnownEntityData data))
					|| data.PlaySiteLocation.Z > 1f) // in the air: passing over, not standing there
				{
					continue;
				}
				if (Overlaps(spot, radius, data.PlaySiteLocation, data.BoundingRadius3D))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>A lock held by another vehicle that is still coming or still there, and its radius.</summary>
	private static bool LiveLockRadius(KeyValuePair<EntityID, Vector3> held, Entity self, out float radius)
	{
		radius = 0f;
		if (held.Key == self.EntityID)
		{
			return false;
		}
		Entity holder = Entity.FindByID(held.Key);
		if (holder == null || IsStale(holder, held.Value))
		{
			return false;
		}
		radius = holder.BoundingRadius3D;
		return true;
	}

	private static void Reserve(SharedKnowledge knowledge, Entity vehicle, Vector3 spot)
	{
		// Drop the stale ones on the way, so the table does not grow with every pilot who died.
		List<EntityID> stale = null;
		foreach (KeyValuePair<EntityID, Vector3> held in knowledge.StopLocks)
		{
			Entity holder = Entity.FindByID(held.Key);
			if (holder == null || (held.Key != vehicle.EntityID && IsStale(holder, held.Value)))
			{
				(stale ??= new List<EntityID>()).Add(held.Key);
			}
		}
		if (stale != null)
		{
			foreach (EntityID id in stale)
			{
				knowledge.StopLocks.Remove(id);
			}
		}
		knowledge.StopLocks[vehicle.EntityID] = spot;
	}

	private static bool IsStale(Entity holder, Vector3 spot)
	{
		return IsStale(holder.MapPosition.HasValue, holder.Vehicle?.DrivenBy != null, holder.PlaySiteLocation, spot);
	}

	/// <summary>Every reservation <paramref name="own"/> respects: its own, and those of every side not at war with it.</summary>
	private static List<KeyValuePair<EntityID, Vector3>> VisibleLocks(Allegiance own)
	{
		List<KeyValuePair<EntityID, Vector3>> locks = new List<KeyValuePair<EntityID, Vector3>>();
		if (own?.SharedKnowledge != null)
		{
			locks.AddRange(own.SharedKnowledge.StopLocks);
		}
		LookUp<Allegiance, AllegianceID>.IterateMembers(delegate(Allegiance other)
		{
			if (other != own && other.SharedKnowledge != null && other.SharedKnowledge.StopLocks.Count > 0 && !IsHostile(own, other))
			{
				locks.AddRange(other.SharedKnowledge.StopLocks);
			}
		});
		return locks;
	}

	/// <summary>Open ground an arriving vehicle may come down on: the terrain alone, since the traveller has no maps of this site.</summary>
	private static bool IsOpenGround(Point tile)
	{
		if (tile.X < 0 || tile.Y < 0 || tile.X >= The.Map.mapTileWidth || tile.Y >= The.Map.mapTileHeight)
		{
			return false;
		}
		return !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot],
			MapManager.TileToUpperLeftSubtile(tile) + new Point(1, 1));
	}
}
