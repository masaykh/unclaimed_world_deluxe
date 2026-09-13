using UWGame.Control.Commands;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class Discard : Command
{
	public long EntityID;

	public long AllegianceID;

	public bool GiveClientFeedback;

	public Discard()
	{
	}

	public Discard(EntityID entityID, AllegianceID allegianceID, bool giveClientFeedback)
	{
		EntityID = (long)entityID;
		AllegianceID = (long)allegianceID;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DiscardEntity();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnDiscardEntity();
		}
	}

	private bool DiscardEntity()
	{
		LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID).SharedKnowledge.GetKnownData((EntityID)EntityID, out var data);
		if (data != null)
		{
			DoDiscard(data);
			return true;
		}
		return false;
	}

	public static void DoDiscard(IKnownEntityData entityData)
	{
		entityData.ChangeOwnership(null, Entity.GiveNewOwnerKnowledge.No);
		entityData.AssignedToJob = null;
	}
}
