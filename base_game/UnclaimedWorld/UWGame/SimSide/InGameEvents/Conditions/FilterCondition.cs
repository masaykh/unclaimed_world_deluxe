using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

[XmlInclude(typeof(PropertyCondition))]
[XmlInclude(typeof(AgentCondition))]
[XmlInclude(typeof(FilterConditionFunction))]
public abstract class FilterCondition
{
	public abstract bool IsFulfilled(IHasExposedProperties hasProperties, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);
}
