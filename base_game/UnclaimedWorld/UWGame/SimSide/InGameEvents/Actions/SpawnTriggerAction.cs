using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnTriggerAction : EventActionType
{
	public string TriggerType;

	public EvalNode Location;

	public TargetObject TargetObject;

	public EvalNode Range;

	public Vector2? AreaDimensions;

	public SpawnTriggerAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnTriggerAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		float? range = null;
		if (Range != null)
		{
			PropertyResult? propertyResult = Range.Evaluate(action);
			if (propertyResult.HasValue && propertyResult.Value.NumberResult.HasValue)
			{
				range = propertyResult.Value.NumberResult.Value;
			}
		}
		Vector3? fixedLocation = null;
		Entity entity = null;
		if (Location != null)
		{
			PropertyResult? propertyResult2 = Location.Evaluate(action);
			if (!propertyResult2.HasValue || !propertyResult2.Value.LocationResult.HasValue)
			{
				failReason = "Location did not give a result.";
				return false;
			}
			fixedLocation = propertyResult2.Value.LocationResult.Value.ToVector3();
		}
		else
		{
			List<IHasExposedProperties> result = TargetObject.GetResult(action);
			if (result.Count == 0)
			{
				failReason = "Lookup did not give any results.";
				return false;
			}
			entity = (Entity)result[0];
		}
		if (entity != null)
		{
			entity.AttachTrigger(new Trigger(entity, null, GameData.Instance.AllTriggerTypes[TriggerType], null, range, AreaDimensions));
		}
		else
		{
			The.Sim.TriggerSystem.RegisterTrigger(new Trigger(null, fixedLocation, GameData.Instance.AllTriggerTypes[TriggerType], null, range, AreaDimensions));
		}
		return true;
	}

	public override string ToString()
	{
		return "Spawn trigger " + TriggerType;
	}
}
