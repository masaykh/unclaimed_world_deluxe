using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class SalvageJob : ISnapshot
{
	public ProcessJob ProcessJob;

	private JobID snapshotJob;

	private Dictionary<EntityID, Point> workersSubtilePositions = new Dictionary<EntityID, Point>();

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public SalvageJob()
	{
	}

	public SalvageJob(ProcessJob processJob)
	{
		ProcessJob = processJob;
	}

	public void Abandon(Entity entity)
	{
		if (workersSubtilePositions.ContainsKey(entity.EntityID))
		{
			workersSubtilePositions.Remove(entity.EntityID);
		}
	}

	public override string ToString()
	{
		return new StringBuilder("Salvage ").ToString();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotJob = sn.SnapshotID<Job, JobID>(ProcessJob).Value;
		workersSubtilePositions = sn.DoDictionary(workersSubtilePositions);
		sn.Ignore(ProcessJob);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
	}
}
