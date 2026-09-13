using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ChangeCreditsAction : EventActionType
{
	public enum Operation
	{
		Set,
		Add
	}

	public EvalNode Amount;

	public TargetObject TargetObject;

	public string AllegianceKey;

	public Operation OperationToUse;

	public ChangeCreditsAction(string keyName)
		: base(keyName)
	{
	}

	public ChangeCreditsAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		IHasExposedProperties hasExposedProperties;
		if (AllegianceKey != null)
		{
			hasExposedProperties = The.Sim.PlaySite.Allegiances.First((Allegiance a) => a.KeyName == AllegianceKey);
		}
		else
		{
			List<IHasExposedProperties> result = TargetObject.GetResult(action);
			hasExposedProperties = ((result.Count != 0) ? result[0] : null);
		}
		EntityGroup entityGroup = null;
		if (hasExposedProperties != null)
		{
			if (hasExposedProperties is Allegiance allegiance)
			{
				entityGroup = allegiance.SharedKnowledge.AllKnownEntities;
			}
			else if (hasExposedProperties is Entity { PersonEntity: not null } entity)
			{
				entityGroup = entity.PersonEntity.OwnedEntities;
			}
		}
		if (entityGroup == null)
		{
			failReason = "Lookup of target owner did not give any valid results.";
			return false;
		}
		if (Amount != null)
		{
			float value = Amount.Evaluate(action).Value.NumberResult.Value;
			decimal num = entityGroup.Parent.TradeCredits ?? 0m;
			Operation operationToUse = OperationToUse;
			decimal value2 = ((operationToUse == Operation.Set || operationToUse != Operation.Add) ? ((decimal)value) : ((decimal)value + num));
			entityGroup.Parent.TradeCredits = value2;
			return true;
		}
		failReason = "Amount has not been set.";
		return false;
	}

	public override string ToString()
	{
		return "Change trade credits for: " + AllegianceKey;
	}
}
