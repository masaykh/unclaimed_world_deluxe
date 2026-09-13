using System;
using UWGame.Control.Commands;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class Salvage : Command
{
	public long EntityID;

	public long AllegianceID;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public Salvage()
	{
	}

	public Salvage(EntityID entityID, AllegianceID allegianceID, bool giveClientFeedback)
	{
		EntityID = (long)entityID;
		AllegianceID = (long)allegianceID;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoSalvage();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnSalvageEntity();
		}
	}

	private bool DoSalvage()
	{
		LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID).SharedKnowledge.GetKnownData((EntityID)EntityID, out var data);
		if (!data.OwnedBy.HasValue || !LookUpOwners.ResolveEntityOwner(data, out EntityGroup ownedEntities))
		{
			return false;
		}
		ProcessJob processJob = CreateSalvageJob(data, ownedEntities);
		if (Priority.HasValue)
		{
			processJob.Priority = Priority.Value;
		}
		return true;
	}

	public static ProcessJob CreateSalvageJob(IKnownEntityData entity, EntityGroup resolvedOwner)
	{
		ProcessJob processJob = JobManager.CreateProcessJob(null, resolvedOwner, entity.EntityType.NonLivingType.SalvageProcessType, null, entity.AccessPoint, null, isSalvage: true);
		processJob.AssignImmovableInput(entity);
		return processJob;
	}

	public static bool SalvageJobExists(IKnownEntityData entity)
	{
		LookUpOwners.ResolveEntityOwner(entity, out IOwner owner);
		if (owner != null && owner.OwnedEntities.OtherJobs.Exists((Job j) => SalvageJobExistsForEntity(j, entity)))
		{
			return true;
		}
		return false;
	}

	private static bool SalvageJobExistsForEntity(Job j, IKnownEntityData entity)
	{
		if (j is ProcessJob { SalvageJob: not null } processJob && processJob.GetAssignedInputs(out var inputs) && inputs.TryGetValue(entity.EntityType, out var value) && value.Exists((Tuple<EntityID, WorldLocation> i) => i.Item1 == entity.EntityID))
		{
			return true;
		}
		return false;
	}
}
