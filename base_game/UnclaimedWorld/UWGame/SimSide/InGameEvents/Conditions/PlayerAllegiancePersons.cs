using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class PlayerAllegiancePersons : ConditionValue
{
	public int? MinMembers;

	public int? MaxMembers;

	public AgentCondition AgentCondition;

	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
		{
			return IsFulfilled();
		}
		return false;
	}

	public bool IsFulfilled()
	{
		Allegiance playerAllegiance = The.Sim.PlaySite.PlayerAllegiance;
		int num = ((playerAllegiance != null) ? ((AgentCondition == null) ? playerAllegiance.GetNoOfPersons(null) : playerAllegiance.GetNoOfPersons((Entity e) => AgentCondition.IsFulfilled(e))) : 0);
		if (MinMembers.HasValue && num < MinMembers.Value)
		{
			return false;
		}
		if (MaxMembers.HasValue && num > MaxMembers.Value)
		{
			return false;
		}
		return true;
	}
}
