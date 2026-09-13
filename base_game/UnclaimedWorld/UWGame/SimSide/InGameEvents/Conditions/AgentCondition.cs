using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class AgentCondition : FilterCondition
{
	public bool AllowSleeping;

	public bool AllowFighting;

	public bool AllowThreatened;

	public bool AllowEmigrating;

	public bool AllowTravelling;

	public bool AllowUnconscious;

	public override bool IsFulfilled(IHasExposedProperties hasProperties, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (!(hasProperties is Entity entity) || !IsFulfilled(entity))
		{
			return false;
		}
		return true;
	}

	public bool IsFulfilled(Entity entity)
	{
		return TalkAction.TestAgentProperties(entity, AllowSleeping, AllowUnconscious, AllowFighting, AllowThreatened, AllowEmigrating, AllowTravelling);
	}
}
