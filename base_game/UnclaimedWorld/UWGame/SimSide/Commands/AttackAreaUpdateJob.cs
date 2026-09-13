using UWGame.Control.Commands;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class AttackAreaUpdateJob : Command
{
	public long jobID;

	public bool GiveClientFeedback;

	public int NoOfAttackers;

	public AttackAreaUpdateJob()
	{
	}

	public AttackAreaUpdateJob(JobID jobID, bool giveClientFeedback, int noOfPatrollers)
	{
		this.jobID = (long)jobID;
		GiveClientFeedback = giveClientFeedback;
		NoOfAttackers = noOfPatrollers;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Zone zone;
		bool flag = DoUpdateAttackArea(out zone);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnPatrolOrAttackArea(zone);
		}
	}

	private bool DoUpdateAttackArea(out Zone zone)
	{
		zone = null;
		Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);
		if (job != null)
		{
			AttackAreaJob attackAreaJob = job as AttackAreaJob;
			attackAreaJob.SetJobPositions(NoOfAttackers);
			zone = attackAreaJob.Zone;
			return true;
		}
		return false;
	}
}
