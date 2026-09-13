using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.Entities;

public interface IHasExposedProperties
{
	string KeyName { get; }

	void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null);

	PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge = null, IHasExposedProperties parent = null);

	void SetPropertyValue(string propertyKey, PropertyResult? value);

	string GetCaption(string propertyKey);

	string GetDefaultCaption(string propertyKey);

	void GetDefaultKey(out string PropertyKey);

	EntityID? GetEntityID();

	bool GetIsSeenDirectly();
}
