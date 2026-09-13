using System.Collections.Generic;
using UWGame.Control.Commands;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SpecialAction : Command
{
	public long EntityGroup;

	public long EntityID;

	public long AllegianceID;

	public string ProcessType;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public SpecialAction()
	{
	}

	public SpecialAction(EntityID entityID, AllegianceID allegianceID, EntityGroupID entityGroupID, bool giveClientFeedback, string processTypeKey)
	{
		EntityID = (long)entityID;
		EntityGroup = (long)entityGroupID;
		AllegianceID = (long)allegianceID;
		GiveClientFeedback = giveClientFeedback;
		ProcessType = processTypeKey;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoSpecialAction();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnSpecialAction();
		}
	}

	private bool DoSpecialAction()
	{
		LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID).SharedKnowledge.GetKnownData((EntityID)EntityID, out var data);
		if (data != null)
		{
			ProcessType processType = GameData.Instance.AllProcessTypes[ProcessType];
			ProcessJob processJob = JobManager.CreateSpecialActionJob(LookUp<UWGame.SimSide.Entities.EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup), data, processType);
			if (Priority.HasValue)
			{
				processJob.Priority = Priority.Value;
			}
			return true;
		}
		return false;
	}

	public static bool ActionJobExists(IKnownEntityData entity, ProcessType processType, List<Job> jobs)
	{
		if (jobs.Exists((Job j) => JobExistsForEntity(j, entity, processType)))
		{
			return true;
		}
		return false;
	}

	private static bool JobExistsForEntity(Job j, IKnownEntityData entity, ProcessType processType)
	{
		if (j is ProcessJob processJob && processJob.ProcessType == processType && processJob.GetActingOnEntity(out EntityID? actingOnEntity))
		{
			return actingOnEntity == entity.EntityID;
		}
		return false;
	}
}
