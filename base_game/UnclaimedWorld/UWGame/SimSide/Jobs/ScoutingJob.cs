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
			return "Examining area";
		}
		return "Scouting";
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
		snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);
		Location = sn.DoVector3Nullable(Location);
		Examine = sn.DoBool(Examine);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
	}
}
