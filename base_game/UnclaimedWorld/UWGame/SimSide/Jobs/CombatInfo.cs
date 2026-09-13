using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class CombatInfo : ISnapshot
{
	public EntityID? Target;

	private const byte valueForCorrectDistance = 10;

	private static Dictionary<Entity, Vector3> setOfEntitiesToDraw = new Dictionary<Entity, Vector3>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public static bool GetRangedLocation(Entity attacker, IKnownEntityData target, AttackType attackType, out Vector3 rangedLocation)
	{
		rangedLocation = attacker.PlaySiteLocation;
		if (!attackType.MaxRange.HasValue)
		{
			return false;
		}
		if ((double)(attacker.PlaySiteLocation - target.PlaySiteLocation).LengthSquared() < (double?)attackType.MaxRangeSquared)
		{
			return true;
		}
		Common.GetLocationAtDistance(target.Location.Value, attacker.Location.Value, attackType.MaxRange.Value, out rangedLocation);
		return false;
	}

	public static bool GetMeleeLocation(Entity attacker, IKnownEntityData target, out Vector3? meleeLocation, out bool isCenterLocation)
	{
		bool? isMoving = target.IsMoving;
		bool flag = true;
		if (isMoving == true == flag && isMoving.HasValue && Common.DistanceOctile(target.PlaySiteLocation, attacker.PlaySiteLocation) > 22f)
		{
			meleeLocation = target.Location;
			isCenterLocation = true;
		}
		else
		{
			isCenterLocation = false;
			_ = attacker.Intelligence;
			SubtileInfluence subtileInfluence = DrawMeleePositionInfluenceMap(attacker, target);
			if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subtileInfluence.Values, out var bestSubtilePoint) == -1)
			{
				meleeLocation = null;
				return false;
			}
			meleeLocation = MapManager.SubTileToWorldPos(new Point(subtileInfluence.TopLeftSubtilePositionOfMap.X + bestSubtilePoint.X, subtileInfluence.TopLeftSubtilePositionOfMap.Y + bestSubtilePoint.Y)).ToVector3();
		}
		return true;
	}

	private static SubtileInfluence DrawMeleePositionInfluenceMap(Entity attacker, IKnownEntityData target)
	{
		float num = 2f * target.EntityType.LocomotorType.MeleeRadius + 4f * attacker.EntityType.LocomotorType.MeleeRadius;
		int sizeOfMapInSubtiles = (int)(0.0625f * num);
		Vector3 playSiteLocation = target.PlaySiteLocation;
		SubtileInfluence subtileInfluence = new SubtileInfluence(playSiteLocation, sizeOfMapInSubtiles);
		subtileInfluence.DrawDistanceGradientOnInfluenceMap(attacker.PlaySiteLocation);
		DrawBestMeleeRadius(attacker, target, playSiteLocation, subtileInfluence.TopLeftSubtilePositionOfMap, subtileInfluence.Values);
		DrawNegativeInfluenceFromEntities(attacker, target, subtileInfluence.TopLeftSubtilePositionOfMap, subtileInfluence.Values);
		MovementMap movementMap = attacker.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(attacker.Intelligence.ProtectionLevel, attacker.EntityType, ThreatStance.Bold);
		subtileInfluence.BlockOutBlockedSubtiles(movementMap.Layers[SurfaceType.TransportType.Foot], blockReserved: false);
		return subtileInfluence;
	}

	private static void DrawBestMeleeRadius(Entity attacker, IKnownEntityData target, Vector3 targetLocation, Point subTilePositionOfMap, byte[][] subtileMap)
	{
		Point point = MapManager.WorldPosToSubtile(targetLocation);
		Point p = new Point(point.X - subTilePositionOfMap.X, point.Y - subTilePositionOfMap.Y);
		float correctMeleeDistance = target.EntityType.LocomotorType.MeleeRadius + attacker.EntityType.LocomotorType.MeleeRadius;
		for (int i = 0; i < Common.GetJaggedArrayWidth(subtileMap); i++)
		{
			for (int j = 0; j < Common.GetJaggedArrayHeight(subtileMap); j++)
			{
				if (AttackJob.IsCorrectMeleeDistance(16f * Common.DistanceOctile(p, new Point(i, j)), correctMeleeDistance))
				{
					subtileMap[i][j] += 10;
				}
			}
		}
	}

	private static void DrawNegativeInfluenceFromEntities(Entity attacker, IKnownEntityData target, Point topLeftSubtilePosition, byte[][] subtileMap)
	{
		Point point = MapManager.SubTileToTilePos(topLeftSubtilePosition);
		Point point2 = MapManager.SubTileToTilePos(new Point(topLeftSubtilePosition.X + Common.GetJaggedArrayWidth(subtileMap), topLeftSubtilePosition.Y + Common.GetJaggedArrayHeight(subtileMap)));
		setOfEntitiesToDraw.Clear();
		if (The.Sim.MeleeAttackers.TryGetValue(target.EntityID, out var value))
		{
			List<EntityID> list = null;
			foreach (KeyValuePair<EntityID, Vector3> item in value)
			{
				Entity entity = Entity.FindByID(item.Key);
				if (entity != null)
				{
					setOfEntitiesToDraw.Add(entity, item.Value);
				}
				else
				{
					Common.AddToList(ref list, item.Key);
				}
			}
			if (list != null)
			{
				foreach (EntityID item2 in list)
				{
					value.Remove(item2);
				}
				if (value.Count == 0)
				{
					The.Sim.MeleeAttackers.Remove(target.EntityID);
				}
			}
		}
		for (int i = point.X; i <= point2.X; i++)
		{
			for (int j = point.Y; j <= point2.Y; j++)
			{
				TerrainTile terrainTile = The.Map.TileMap[i][j];
				if (terrainTile.EntitiesOnTile == null)
				{
					continue;
				}
				foreach (Entity item3 in terrainTile.EntitiesOnTile)
				{
					if (item3 != target && item3 != attacker && item3.Intelligence != null && item3.Renderable.RenderAsModel != null && !item3.Locomotor.IsMoving() && !setOfEntitiesToDraw.ContainsKey(item3))
					{
						setOfEntitiesToDraw.Add(item3, item3.PlaySiteLocation);
					}
				}
			}
		}
		Vector3 relativeTo = MapManager.SubTileEdgeToWorldPos3(topLeftSubtilePosition);
		foreach (KeyValuePair<Entity, Vector3> item4 in setOfEntitiesToDraw)
		{
			if (item4.Key != attacker)
			{
				Point pos = MapManager.WorldPosToRelativeSubtile(item4.Value, relativeTo);
				if (pos.X >= 0 && pos.Y >= 0)
				{
					int paramValue = (int)(item4.Key.EntityType.LocomotorType.MeleeRadius * 0.0625f);
					InfluenceMap.DrawLinearInfluenceCircle(subtileMap, pos, 0, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, paramValue);
				}
			}
		}
		setOfEntitiesToDraw.Clear();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Target = sn.DoEnumNullable(Target);
		sn.Ignore(setOfEntitiesToDraw);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
