using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetJobTypePriority : Command
{
	public long EntityGroupID;

	public string JobTypeKey;

	public Priority jobPriority;

	public bool UpdateJobInstances;

	public SetJobTypePriority()
	{
	}

	public SetJobTypePriority(EntityGroup entityGroup, string jobTypeKey, Priority jobPriority, bool updateInstances)
	{
		EntityGroupID = (long)entityGroup.ID;
		this.jobPriority = jobPriority;
		JobTypeKey = jobTypeKey;
		UpdateJobInstances = updateInstances;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityGroup entityGroup = LookUp<EntityGroup, UWGame.SimSide.Entities.EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);
		if (entityGroup != null)
		{
			JobType jobType = GameData.Instance.AllJobTypes[JobTypeKey];
			entityGroup.Policy.JobTypePriorities[jobType] = jobPriority;
			jobType.IterateJobs(entityGroup, delegate(Job j)
			{
				j.Priority = jobPriority;
			});
		}
	}
}
