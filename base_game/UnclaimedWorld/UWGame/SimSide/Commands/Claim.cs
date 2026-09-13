using UWGame.Control.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class Claim : Command
{
	public long EntityID;

	public long NewOwner;

	public long AllegianceID;

	public bool GiveClientFeedback;

	public Claim()
	{
	}

	public Claim(EntityID entityID, AllegianceID allegianceID, OwnerID newOwner, bool giveClientFeedback)
	{
		EntityID = (long)entityID;
		AllegianceID = (long)allegianceID;
		NewOwner = (long)newOwner;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoClaim();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnClaimEntity();
		}
	}

	private bool DoClaim()
	{
		LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID).SharedKnowledge.GetKnownData((EntityID)EntityID, out var data);
		Entity entity = data as Entity;
		IOwner newOwner = LookUpOwners.FindByID((OwnerID)NewOwner);
		if (entity != null)
		{
			entity.ChangeOwnership(newOwner);
			return true;
		}
		return false;
	}
}
