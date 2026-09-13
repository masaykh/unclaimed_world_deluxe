using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.InGameEvents.Actions;

public class CreateJobAction : EventActionType
{
	public TargetObject TargetObject;

	public string EntityName;

	public EvalNode ProcessTypeKey;

	public AllegianceAndExpedition AllegianceAndExpedition;

	public CreateJobAction(string keyName)
		: base(keyName)
	{
	}

	public CreateJobAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		IKnownEntityData knownEntityData = null;
		if (EntityName != null)
		{
			knownEntityData = TalkAction.GetEntityByName(EntityName);
			if (knownEntityData == null)
			{
				failReason = "No entity with name '" + EntityName + "' exists.";
			}
		}
		else
		{
			List<IHasExposedProperties> result = TargetObject.GetResult(action);
			if (result.Count == 0)
			{
				failReason = "Lookup did not give any results.";
				return false;
			}
			knownEntityData = (IKnownEntityData)result[0];
		}
		Allegiance allegiance;
		EntityGroup ownedEntities;
		if (AllegianceAndExpedition != null)
		{
			allegiance = The.Sim.World.GetAllegianceFromKey(AllegianceAndExpedition.AllegianceKey);
			if (allegiance == null)
			{
				failReason = "The allegiance '" + AllegianceAndExpedition.AllegianceKey + "' was not found.";
				return false;
			}
			Expedition expedition = allegiance.GetExpedition(AllegianceAndExpedition.ExpeditionKey);
			if (expedition == null)
			{
				failReason = "The expedition '" + AllegianceAndExpedition.ExpeditionKey + "' was not found.";
				return false;
			}
			ownedEntities = expedition.OwnedEntities;
		}
		else
		{
			if (!knownEntityData.OwnedBy.HasValue)
			{
				failReason = "Could not resolve a job owner.";
				return false;
			}
			IOwner owner = LookUpOwners.FindByID(knownEntityData.OwnedBy);
			if (owner == null)
			{
				failReason = "The entity owner was not found.";
				return false;
			}
			ownedEntities = owner.OwnedEntities;
			allegiance = owner.Allegiance;
		}
		ProcessType value = null;
		PropertyResult? propertyResult = ProcessTypeKey.Evaluate(action);
		if (propertyResult.HasValue)
		{
			GameData.Instance.AllProcessTypes.TryGetValue(propertyResult.Value.StringResult, out value);
		}
		if (value == null)
		{
			failReason = "Could not resolve process type.";
			return false;
		}
		if (knownEntityData != null)
		{
			TryCreateActionJob(knownEntityData, allegiance, ownedEntities, value);
			return true;
		}
		return false;
	}

	private void TryCreateActionJob(IKnownEntityData targetEntityData, Allegiance allegiance, EntityGroup owner, ProcessType processType)
	{
		List<Job> otherJobs = owner.OtherJobs;
		if (!SpecialAction.ActionJobExists(targetEntityData, processType, otherJobs))
		{
			JobManager.CreateSpecialActionJob(owner, targetEntityData, processType);
		}
	}

	public override string ToString()
	{
		return "Create special action: " + ProcessTypeKey;
	}
}
