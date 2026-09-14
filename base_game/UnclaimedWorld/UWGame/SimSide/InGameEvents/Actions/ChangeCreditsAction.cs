using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ChangeCreditsAction : EventActionType
{
	// PORT FIX. XmlSerializer flattens nested types to their UNQUALIFIED name, so this and
	// ChangeResourcesAction.Operation both wanted to be <Operation> in the same schema and it
	// refused the whole graph:
	//
	//   "Types 'UWGame...ChangeCreditsAction+Operation' and
	//    'UWGame...ChangeResourcesAction+Operation' both use the XML type name 'Operation'."
	//
	// That took out actionSets.xml, eventActionTypes.xml, allegianceEvents.xml and
	// polledEventTypes.xml - which between them are what every scenario's Actions and
	// ConditionalEvents point AT, so a scenario exported as a list of keys to content that could
	// not be exported. Nothing in the shipped game ever serialized these types, so there is no
	// existing XML for a rename to break.
	[System.Xml.Serialization.XmlType("ChangeCreditsOperation")]
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
