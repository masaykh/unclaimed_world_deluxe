using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.Maps.MapEditor;

public class AllegianceAndExpedition
{
	public string AllegianceKey;

	public string ExpeditionKey;

	public EvalNode DynamicAllegianceKey;

	public EvalNode DynamicExpeditionKey;

	public void Resolve(EventAction action, ref string allegiance, ref string expedition)
	{
		Resolve(action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget, ref allegiance, ref expedition);
	}

	public void Resolve(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, ref string allegiance, ref string expedition)
	{
		allegiance = ResolveAllegiance(DynamicAllegianceKey, AllegianceKey, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		expedition = ResolveExpedition(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
	}

	private string ResolveExpedition(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		string result = "";
		if (DynamicExpeditionKey != null)
		{
			PropertyResult? propertyResult = DynamicExpeditionKey.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (propertyResult.HasValue)
			{
				result = propertyResult.Value.StringResult;
			}
		}
		else if (ExpeditionKey != null)
		{
			result = ExpeditionKey;
		}
		return result;
	}

	public static bool ResolveAllegiance(EvalNode dynamicAllegianceKey, string allegianceKey, EventAction eventAction, out Allegiance allegiance, ref string failReason)
	{
		allegianceKey = ResolveAllegiance(dynamicAllegianceKey, allegianceKey, eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
		allegiance = The.Sim.World.GetAllegianceFromKey(allegianceKey);
		if (allegiance == null)
		{
			failReason = "Allegiance not found";
			return false;
		}
		return true;
	}

	public static string ResolveAllegiance(EvalNode dynamicAllegianceKey, string allegianceKey, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		string result = "";
		if (dynamicAllegianceKey != null)
		{
			PropertyResult? propertyResult = dynamicAllegianceKey.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			if (propertyResult.HasValue)
			{
				result = propertyResult.Value.StringResult;
			}
		}
		else if (allegianceKey != null)
		{
			result = allegianceKey;
		}
		return result;
	}

	public bool Resolve(EventAction eventAction, out Allegiance allegiance, out Expedition expedition, ref string failReason)
	{
		allegiance = null;
		expedition = null;
		if (!ResolveAllegiance(DynamicAllegianceKey, AllegianceKey, eventAction, out allegiance, ref failReason))
		{
			return false;
		}
		string key = ResolveExpedition(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
		expedition = allegiance.GetExpedition(key);
		if (expedition == null)
		{
			failReason = "Expedition not found";
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		string text = "";
		if (DynamicAllegianceKey != null)
		{
			text = DynamicAllegianceKey.ToString();
		}
		else if (AllegianceKey != null)
		{
			text = AllegianceKey;
		}
		if (DynamicExpeditionKey != null)
		{
			text = text + " " + DynamicExpeditionKey.ToString();
		}
		else if (ExpeditionKey != null)
		{
			text = text + " " + ExpeditionKey.ToString();
		}
		return text;
	}
}
