using System.Collections.Generic;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class EntityGroupPolicy : ISnapshot
{
	public Dictionary<JobType, Priority> JobTypePriorities = new Dictionary<JobType, Priority>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Priority GetPriority(JobType jobType)
	{
		if (!JobTypePriorities.TryGetValue(jobType, out var value))
		{
			return Priority.Normal;
		}
		return value;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		JobTypePriorities = sn.DoDictionary(JobTypePriorities);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
