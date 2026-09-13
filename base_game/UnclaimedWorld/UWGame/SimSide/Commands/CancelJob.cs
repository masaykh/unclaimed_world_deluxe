using UWGame.Control.Commands;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class CancelJob : Command
{
	public long jobID;

	public CancelJob()
	{
	}

	public CancelJob(JobID jobID)
	{
		this.jobID = (long)jobID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		LookUp<Job, JobID>.FindByID((JobID)jobID).Destroy(cancelTakers: true);
	}
}
