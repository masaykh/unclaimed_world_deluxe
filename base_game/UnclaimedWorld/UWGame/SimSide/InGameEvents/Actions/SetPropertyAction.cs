using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SetPropertyAction : EventActionType
{
	public TargetObject TargetObject;

	public string PropertyKey;

	public EvalNode Value;

	private string logMessage;

	public bool SetValueToNull;

	public SetPropertyAction()
	{
	}

	public SetPropertyAction(string keyName)
		: base(keyName)
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		List<IHasExposedProperties> list = null;
		_ = PropertyKey == "disableSpecialAction";
		_ = PropertyKey == "addTrigger";
		list = ((TargetObject == null) ? TargetObject.GetRootElementAsList() : TargetObject.GetResult(action));
		IHasExposedProperties hasExposedProperties = null;
		if (list != null && list.Count > 0)
		{
			hasExposedProperties = list[0];
			SetValue(hasExposedProperties, action);
		}
		return true;
	}

	private void SetValue(IHasExposedProperties firstResult, EventAction action)
	{
		if (firstResult == null)
		{
			return;
		}
		if (Value != null)
		{
			PropertyResult? propertyResult = Value.Evaluate(action);
			if (propertyResult.HasValue)
			{
				logMessage = "set property: " + PropertyKey + " to " + propertyResult.Value.ToString() + " at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
				firstResult.SetPropertyValue(PropertyKey, propertyResult.Value);
			}
			else
			{
				logMessage = "Failed to set property " + PropertyKey + " at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
			}
		}
		else if (SetValueToNull)
		{
			logMessage = "set property: " + PropertyKey + " to null at " + firstResult.ToString() + firstResult.GetPropertyValue("location").ToString();
			firstResult.SetPropertyValue(PropertyKey, null);
		}
	}

	public void PreInitValidate(List<string> errors)
	{
	}

	public override string ToString()
	{
		return logMessage;
	}
}
