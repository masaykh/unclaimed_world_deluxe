using UWGame.Control.Commands;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class Hunt : Command
{
	public long EntityID;

	public long AllegianceID;

	public bool GiveClientFeedback;

	public long EntityGroup;

	public Hunt()
	{
	}

	public Hunt(EntityID entityID, AllegianceID allegianceID, EntityGroupID ownerOfJobID, bool giveClientFeedback)
	{
		EntityID = (long)entityID;
		EntityGroup = (long)ownerOfJobID;
		AllegianceID = (long)allegianceID;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = HuntCreature();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnHuntCreature();
		}
	}

	private bool HuntCreature()
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID).SharedKnowledge.GetKnownData((EntityID)EntityID, out var creatureToHunt)))
		{
			EntityGroup entityGroup = LookUp<UWGame.SimSide.Entities.EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup);
			if (!entityGroup.OtherJobs.Exists((Job j) => j is HuntingJob && ((HuntingJob)j).Target == creatureToHunt.EntityID))
			{
				new HuntingJob(creatureToHunt.EntityID, creatureToHunt.EntityType, entityGroup);
				return true;
			}
		}
		return false;
	}
}
