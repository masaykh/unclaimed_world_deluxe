using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Communication;

public class Communicator : Component
{
	public Communicator(Entity parent)
		: base(parent)
	{
	}

	public Communicator()
	{
	}

	public bool IsCommunicatorWorkingAndInRange(double distance)
	{
		if (Parent.IsCompleted() && Entity.IsFunctional(Parent) && Parent.EntityType.CommunicatorType.IsInRange(distance))
		{
			return true;
		}
		return false;
	}

	public void GainContact()
	{
		Allegiance allegianceOrOwner = Parent.GetAllegianceOrOwner();
		foreach (KeyValuePair<string, Site> allSite in The.Sim.World.AllSites)
		{
			foreach (Allegiance allegiance in allSite.Value.Allegiances)
			{
				if (allegiance != allegianceOrOwner)
				{
					bool? canTradeAndCommunicate = allegiance.RepresentativeEntityType.IntelligenceType.CanTradeAndCommunicate;
					bool flag = true;
					if (canTradeAndCommunicate == true == flag && canTradeAndCommunicate.HasValue && !allegianceOrOwner.AllegiancesWeAreInContactWith.Contains(allegiance.ID) && Communicates.IsInCommunicationRange(allegianceOrOwner, allegiance, out var _))
					{
						allegiance.GainContact(allegianceOrOwner);
						allegianceOrOwner.GainContact(allegiance);
					}
				}
			}
		}
	}

	public void Destroy()
	{
		if (NonLivingEntity.IsCompleted(Parent.Progress))
		{
			UpdateCommunications();
		}
	}

	private void UpdateCommunications()
	{
		Allegiance allegianceOrOwner = Parent.GetAllegianceOrOwner();
		if (allegianceOrOwner == null)
		{
			return;
		}
		List<AllegianceID> list = null;
		List<Allegiance> list2 = null;
		foreach (AllegianceID item in allegianceOrOwner.AllegiancesWeAreInContactWith)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(item);
			if (allegiance != null)
			{
				if (!Communicates.IsInCommunicationRange(allegianceOrOwner, allegiance, out var _))
				{
					allegiance.LoseContact(allegianceOrOwner);
					Common.AddToList(ref list2, allegiance);
				}
			}
			else
			{
				Common.AddToList(ref list, item);
			}
		}
		if (list2 != null)
		{
			foreach (Allegiance item2 in list2)
			{
				allegianceOrOwner.LoseContact(item2);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (AllegianceID item3 in list)
		{
			allegianceOrOwner.AllegiancesWeAreInContactWith.Remove(item3);
		}
	}
}
