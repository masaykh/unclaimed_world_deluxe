using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class DynamicLocation
{
	public TargetObject TargetObject;

	public string PropertyKey;

	public Vector2? GetLocation(EventAction action)
	{
		List<IHasExposedProperties> list = null;
		list = ((TargetObject == null) ? TargetObject.GetRootElementAsList() : TargetObject.GetResult(action));
		if (list != null && list.Count > 0)
		{
			IHasExposedProperties hasExposedProperties = list[0];
			PropertyResult? propertyResult = null;
			if (hasExposedProperties != null && PropertyKey != null)
			{
				propertyResult = hasExposedProperties.GetPropertyValue(PropertyKey);
				if (propertyResult.HasValue)
				{
					return propertyResult.Value.LocationResult;
				}
			}
		}
		return null;
	}
}
