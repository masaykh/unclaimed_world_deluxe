using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class ScoutingJob : Job
{
	public Vector3? Location;

	public Zone Zone;

	private ZoneID? snapshotZone;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	// PORT: the sweep of the zone belongs to the order, not to whoever is walking it. The studio
	// kept the points still to visit on GoalSearchArea, so anything that ended the goal - a meal,
	// sleep, nightfall, a better job - threw the sweep away, and the next GoalSearchArea sampled the
	// zone again and started from nothing. A zone larger than one uninterrupted stretch of work was
	// never finished, and SCOUT and EXAMINE ran for ever. GoalSearchArea now computes the points
	// once, here, and every later sweep of this job - after an interruption, by another colonist,
	// after a load - continues from what is left. Null until the first sweep computes them.
	/// <summary>The points of the zone no sweep has visited, claimed or dropped yet; null before the first sweep.</summary>
	public List<Vector2> SweepPointsLeft;

	/// <summary>The points of the zone visited so far, by every sweep of this job.</summary>
	public int SweepPointsVisited;

	public bool Examine { get; private set; }

	public ScoutingJob(Zone mapArea, EntityGroup entityGroup, bool examine)
		: base(entityGroup)
	{
		Zone = mapArea;
		Examine = examine;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public ScoutingJob()
	{
	}

	public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
	{
		if (Zone != null)
		{
			if (Examine)
			{
				if (Zone.ExamineJob != null)
				{
					Zone.ExamineJob = null;
				}
			}
			else if (Zone.ScoutingJob != null)
			{
				Zone.ScoutingJob = null;
			}
		}
		base.Destroy(removeTakers, entityToExcludeFromCancel);
	}

	public override Vector3? GetCircaLocation()
	{
		return Zone?.MapArea?.GetCenter();
	}

	public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tile = null;
		targetEntity = null;
		zoneID = null;
		if (Zone == null && Location.HasValue)
		{
			tile = MapManager.WorldPosToTile(Location.Value);
		}
		else if (Zone != null)
		{
			zoneID = Zone.ID;
		}
	}

	public override string GetName()
	{
		if (Examine)
		{
			return UWGame.Locale.Text("Examining area");
		}
		return UWGame.Locale.Text("Scouting");
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		// PORT: ScoutingSweep - the sweep's points are saved with the job (see SweepPointsLeft).
		version = sn.DoVersion(Snapshotter.Version.ScoutingSweep);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);
		Location = sn.DoVector3Nullable(Location);
		Examine = sn.DoBool(Examine);
		if (version >= Snapshotter.Version.ScoutingSweep)
		{
			SweepPointsLeft = sn.DoList(SweepPointsLeft);
			SweepPointsVisited = sn.DoInt32(SweepPointsVisited);
		}
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
	}
}
