using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class AttackAreaJob : CombatAreaJob
{
	public bool AttackVermin;

	public bool AttackThreats;

	private double? areaReachedTimePoint;

	private CollideShape2D area;

	private List<Pair<Entity, Vector2>> threatsInArea = new List<Pair<Entity, Vector2>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override bool CanAttackVermin => AttackVermin;

	public override bool CanAttackTargetsOutsideZone => true;

	public override string GetName()
	{
		return "Attacking";
	}

	public AttackAreaJob()
	{
	}

	public AttackAreaJob(Zone zone, EntityGroup entityGroup, bool attackVermin, bool attackThreats, int noOfPatrollers)
		: base(zone, entityGroup, noOfPatrollers)
	{
		AttackVermin = attackVermin;
		AttackThreats = attackThreats;
		CreateArea();
	}

	private void CreateArea()
	{
		Vector3 vector = MapManager.TileToWorldPos(ThreatArea.Location);
		Vector3 vector2 = MapManager.TileToWorldPos(new Point(ThreatArea.Right, ThreatArea.Bottom));
		area = new CollideShape2D(vector.Y, vector.X, vector2.Y, vector2.X);
	}

	public bool IsDurationReached()
	{
		if (areaReachedTimePoint.HasValue)
		{
			return The.Sim.TimepointReached(areaReachedTimePoint + GameData.Instance.AIConstants.MinimumSearchTimeInAttackZone);
		}
		return false;
	}

	public bool AreaContainsThreats()
	{
		if (ResolveOwner(out var owner))
		{
			The.AgentQuadTree.GetObjectsIntersectingBounds(area, (Entity e) => owner.ThreatJobsByTarget.ContainsKey(e.ID) || (AttackVermin && owner.AssetThreatJobsByTarget.ContainsKey(e.ID)), ref threatsInArea);
			bool result = threatsInArea.Count > 0;
			threatsInArea.Clear();
			return result;
		}
		return false;
	}

	public void SetAreaReached()
	{
		if (!areaReachedTimePoint.HasValue)
		{
			areaReachedTimePoint = The.Sim.TotalUnPausedGameTimeInSeconds;
		}
	}

	public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
	{
		if (Zone != null && Zone.AttackAreaJob != null)
		{
			Zone.AttackAreaJob = null;
		}
		base.Destroy(removeTakers, entityToExcludeFromCancel);
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
		AttackThreats = sn.DoBool(AttackThreats);
		AttackVermin = sn.DoBool(AttackVermin);
		areaReachedTimePoint = sn.DoDoubleNullable(areaReachedTimePoint);
		sn.Ignore(area);
		sn.Ignore(threatsInArea);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateArea();
		base.LoadPostProcess(sn);
	}
}
