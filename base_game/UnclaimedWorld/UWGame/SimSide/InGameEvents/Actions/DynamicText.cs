using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.InGameEvents.Actions;

public class DynamicText
{
	public string Text;

	public EvalNode EvalText;

	public SubstituteValue[] SubstitutionValues;

	public string GetSubstitutedText(EventAction eventAction)
	{
		return SubstituteTextVariables(eventAction);
	}

	private string SubstituteTextVariables(EventAction eventAction)
	{
		if (string.IsNullOrEmpty(Text) && EvalText == null)
		{
			return null;
		}
		string text;
		if (Text != null)
		{
			text = Text;
		}
		else
		{
			PropertyResult? propertyResult = EvalText.Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
			if (!propertyResult.HasValue)
			{
				return null;
			}
			text = propertyResult.Value.StringResult ?? "";
		}
		if (SubstitutionValues != null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(SubstitutionValues.Length);
			SubstituteValue[] substitutionValues = SubstitutionValues;
			foreach (SubstituteValue substituteValue in substitutionValues)
			{
				dictionary.Add(substituteValue.Placeholder, substituteValue.Evaluate(eventAction));
			}
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					text = text.Replace(item.Key, item.Value);
				}
				return text;
			}
		}
		return text;
	}

	public void Validate(ref List<string> listOfErrors)
	{
	}
}
