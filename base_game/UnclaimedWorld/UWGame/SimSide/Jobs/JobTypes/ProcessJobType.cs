using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs.JobTypes;

public class ProcessJobType : JobType
{
	public override void IterateJobs(EntityGroup entityGroup, Action<Job> iterateFunction)
	{
		foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in entityGroup.ProductionJobs)
		{
			foreach (ProcessJob item in productionJob.Value)
			{
				if (IsType(item))
				{
					iterateFunction(item);
				}
			}
		}
		foreach (Job otherJob in entityGroup.OtherJobs)
		{
			if (IsType(otherJob))
			{
				iterateFunction(otherJob);
			}
		}
	}

	public override bool IsType(Job job)
	{
		if (job is ProcessJob processJob && processJob.ProcessType.JobType == this)
		{
			return true;
		}
		return false;
	}

	public override string GetDefaultDisplayName()
	{
		return base.Name;
	}
}
