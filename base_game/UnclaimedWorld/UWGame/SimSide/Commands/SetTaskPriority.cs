using UWGame.Control.Commands;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetTaskPriority : Command
{
	public long jobID;

	public Priority jobPriority;

	public SetTaskPriority()
	{
	}

	public SetTaskPriority(JobID jobID, Priority jobPriority)
	{
		this.jobID = (long)jobID;
		this.jobPriority = jobPriority;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);
		if (job == null)
		{
			return;
		}
		job.Priority = jobPriority;
		if (!(job is ProcessJob { HarvestJob: not null } processJob))
		{
			return;
		}
		Zone zone = processJob.HarvestJob.Zone;
		if (zone == null || !zone.HasHarvestJobs() || !processJob.HarvestJob.Zone.HarvestJobs.TryGetValue(processJob.HarvestJob.ResourceType, out var value))
		{
			return;
		}
		foreach (ProcessJob item in value)
		{
			item.Priority = jobPriority;
		}
	}
}
