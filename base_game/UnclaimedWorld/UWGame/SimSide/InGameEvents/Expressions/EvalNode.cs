using System.Xml.Serialization;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.Expressions;

[XmlInclude(typeof(ValueNode))]
[XmlInclude(typeof(FunctionNode))]
[XmlInclude(typeof(UnaryFunctionNode))]
public abstract class EvalNode
{
	public PropertyResult? Evaluate(EventAction eventAction)
	{
		return Evaluate(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
	}

	public abstract PropertyResult? Evaluate(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);

	public virtual string EvaluateConstant()
	{
		return null;
	}
}
