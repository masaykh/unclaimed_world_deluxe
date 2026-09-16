using UWGame.Control.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
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

			// PORT FIX. Ownership is not membership, and a creature needs both.
			//
			// ChangeOwnership moves property: the owner, the new owner's knowledge of the item,
			// contained upgrades, households. It never touches allegiance. For an item that is
			// the whole story; for anything alive it is half of one, because what gives an
			// entity an allegiance to think for is being a MEMBER of an expedition.
			//
			// A creature bought at a Port arrives as merchandise - ScenarioLoader.SpawnItemOtherSite
			// sets EntityData.OwnedBy and nothing else, which is right for a crate of nails - and
			// claiming it moved the deed without naturalising it. Reported by Kastuk: "Dogs which
			// appear by Port (by Trade), cannot move any way and stay at Port. Can discard them
			// like an item, and cannot Claim them back." Owned, inert, and item-like in exactly
			// the ways ownership governs. Same for the GOPHER robots.
			//
			// ChangeExpedition is the studio's own complete operation - it changes allegiance when
			// the expedition belongs to a different one, fires their SwitchedToPlayerAllegiance
			// hook, moves the expedition membership and the household - and it returns immediately
			// when EntityType.IntelligenceType is null, so claiming an ITEM still does exactly
			// what it did before. That guard is why this needs no test of its own for what is
			// alive and what is not.
			if (newOwner is Expedition expedition && entity.EntityType?.IntelligenceType != null
				&& entity.Intelligence?.Allegiance != null)
			{
				entity.ChangeExpedition(expedition, simulateJoinedNow: true);
			}
			return true;
		}
		return false;
	}
}
