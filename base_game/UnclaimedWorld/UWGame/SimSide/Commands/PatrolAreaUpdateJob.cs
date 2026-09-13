using UWGame.Control.Commands;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class PatrolAreaUpdateJob : Command
{
	public long jobID;

	public bool GiveClientFeedback;

	public int NoOfAttackers;

	public PatrolAreaUpdateJob()
	{
	}

	public PatrolAreaUpdateJob(JobID jobID, bool giveClientFeedback, int noOfPatrollers)
	{
		this.jobID = (long)jobID;
		GiveClientFeedback = giveClientFeedback;
		NoOfAttackers = noOfPatrollers;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Zone zone;
		bool flag = DoUpdatePatrolArea(out zone);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnPatrolOrAttackArea(zone);
		}
	}

	private bool DoUpdatePatrolArea(out Zone zone)
	{
		zone = null;
		Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);
		if (job != null)
		{
			PatrolJob patrolJob = job as PatrolJob;
			patrolJob.SetJobPositions(NoOfAttackers);
			zone = patrolJob.Zone;
			return true;
		}
		return false;
	}
}
