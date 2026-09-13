using System.Collections.Generic;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ClaimEntityAction : EventActionType
{
	public ActionType Action;

	public TargetObject TargetObject;

	public string EntityName;

	public Ownership OwnershipType;

	public TargetObject NewOwner;

	public ClaimEntityAction(string keyName)
		: base(keyName)
	{
	}

	public ClaimEntityAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		List<IHasExposedProperties> list = null;
		if (EntityName != null)
		{
			Entity entityByName = TalkAction.GetEntityByName(EntityName);
			if (entityByName == null)
			{
				failReason = "No entity with name '" + EntityName + "' exists.";
				return false;
			}
			Common.AddToList(ref list, entityByName);
		}
		else
		{
			list = TargetObject.GetResult(action);
			if (list.Count == 0)
			{
				failReason = "Lookup did not give any results.";
				return false;
			}
		}
		if (list != null)
		{
			IOwner owner = null;
			if (Action == ActionType.Claim)
			{
				owner = ResolveNewOwner(OwnershipType, NewOwner, action);
				if (owner == null)
				{
					failReason = "Owner lookup did not give any results.";
					return false;
				}
			}
			foreach (IHasExposedProperties item in list)
			{
				if (item is Entity entity)
				{
					if (Action == ActionType.Claim)
					{
						DoClaim(entity, owner);
					}
					else
					{
						Discard.DoDiscard(entity);
					}
				}
			}
			return true;
		}
		return false;
	}

	public static IOwner ResolveNewOwner(Ownership OwnershipType, TargetObject NewOwner, EventAction action)
	{
		if (NewOwner == null)
		{
			return null;
		}
		List<IHasExposedProperties> result = NewOwner.GetResult(action);
		if (result.Count > 0 && result[0] is Entity entity)
		{
			switch (OwnershipType)
			{
			case Ownership.Expedition:
				if (entity.EntityType.IntelligenceType != null)
				{
					return entity.Intelligence.CurrentExpedition;
				}
				return null;
			case Ownership.Household:
				if (entity.EntityType.Person != null)
				{
					return entity.PersonEntity.Household;
				}
				return null;
			case Ownership.Private:
				if (entity.EntityType.Person != null)
				{
					return entity.PersonEntity;
				}
				return null;
			case Ownership.OwnerOfTarget:
				if (entity.EntityType.Person == null && entity.OwnedBy.HasValue)
				{
					return LookUpOwners.FindByID(entity.OwnedBy.Value);
				}
				return null;
			}
		}
		return null;
	}

	private void DoClaim(Entity entity, IOwner newOwner)
	{
		entity.ChangeOwnership(newOwner);
	}

	public override string ToString()
	{
		return ("Destroy entity " + EntityName) ?? "";
	}
}
