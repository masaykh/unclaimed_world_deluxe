using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

[XmlInclude(typeof(AreaCondition))]
[XmlInclude(typeof(PlayerAllegiancePersons))]
[XmlInclude(typeof(CustomCondition))]
[XmlInclude(typeof(DummyCondition))]
public abstract class ConditionValue : Condition
{
	public bool? AllowWhileAllPlayerMembersAreSleepingOrCollapsed;

	public bool? AllowWhilePlayerMemberIsFighting;

	public bool? AllowWhilePlayerThreatened;

	public override void Initialize()
	{
	}

	public override void PreInitValidate(ref List<string> errors)
	{
	}

	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		bool? allowWhileAllPlayerMembersAreSleepingOrCollapsed = AllowWhileAllPlayerMembersAreSleepingOrCollapsed;
		bool flag = false;
		if (allowWhileAllPlayerMembersAreSleepingOrCollapsed == true == flag && allowWhileAllPlayerMembersAreSleepingOrCollapsed.HasValue && The.Sim.PlaySite.EventManager.PlayerAllegianceIsSleepingOrCollapsed())
		{
			return false;
		}
		allowWhileAllPlayerMembersAreSleepingOrCollapsed = AllowWhilePlayerMemberIsFighting;
		flag = false;
		if (allowWhileAllPlayerMembersAreSleepingOrCollapsed == true == flag && allowWhileAllPlayerMembersAreSleepingOrCollapsed.HasValue && The.Sim.PlaySite.EventManager.PlayerAllegianceIsAttacking())
		{
			return false;
		}
		allowWhileAllPlayerMembersAreSleepingOrCollapsed = AllowWhilePlayerThreatened;
		flag = false;
		if (allowWhileAllPlayerMembersAreSleepingOrCollapsed == true == flag && allowWhileAllPlayerMembersAreSleepingOrCollapsed.HasValue && The.Sim.PlaySite.EventManager.PlayerAllegianceIsUnderThreat())
		{
			return false;
		}
		return true;
	}

	public virtual double? GetUpdateInterval()
	{
		return null;
	}
}
