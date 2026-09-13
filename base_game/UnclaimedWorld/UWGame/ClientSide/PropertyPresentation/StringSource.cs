using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation;

public class StringSource
{
	public string StaticString;

	public string PropertyName;

	public bool UseDefaultName;

	public string GetValue(IHasExposedProperties hasExposedProperties, IHasExposedProperties parent)
	{
		if (StaticString != null)
		{
			return StaticString;
		}
		if (PropertyName != null)
		{
			PropertyResult? propertyValue = hasExposedProperties.GetPropertyValue(PropertyName, The.InGameUI.UIAllegiance.SharedKnowledge, parent);
			if (propertyValue.HasValue && propertyValue.Value.StringResult != null)
			{
				return propertyValue.Value.StringResult;
			}
		}
		else if (UseDefaultName)
		{
			return hasExposedProperties.GetDefaultCaption(null);
		}
		return null;
	}
}
