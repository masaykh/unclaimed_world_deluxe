using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.PropertyObjects;

public class TargetObject
{
	public TargetObjectType TargetObjectType;

	public GetList GetList;

	public List<IHasExposedProperties> GetResult(EventAction eventAction)
	{
		return GetResult(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
	}

	public List<IHasExposedProperties> GetResult(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		IHasExposedProperties hasExposedProperties = null;
		List<IHasExposedProperties> list = new List<IHasExposedProperties>();
		switch (TargetObjectType)
		{
		case TargetObjectType.Root:
			hasExposedProperties = GetRootElement();
			break;
		case TargetObjectType.World:
			hasExposedProperties = The.Sim.World;
			break;
		case TargetObjectType.TriggeringEntity:
			if (triggeringEntity.HasValue)
			{
				hasExposedProperties = Entity.FindByID(triggeringEntity.Value);
			}
			break;
		case TargetObjectType.TargetEntity:
			if (targetEntity.HasValue)
			{
				hasExposedProperties = Entity.FindByID(targetEntity.Value);
			}
			break;
		case TargetObjectType.PolledEventSource:
			hasExposedProperties = polledEventSource;
			break;
		case TargetObjectType.DynamicTarget:
			hasExposedProperties = dynamicTarget;
			break;
		case TargetObjectType.LastSpawnActionResult:
			hasExposedProperties = Entity.FindByID(The.Sim.World.LastSpawnedEntity);
			break;
		}
		if (hasExposedProperties != null)
		{
			list.Add(hasExposedProperties);
			if (GetList != null)
			{
				list = GetList.GetResult(list, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			}
		}
		return list;
	}

	public static IHasExposedProperties GetRootElement()
	{
		return The.Sim.PlaySite;
	}

	public static List<IHasExposedProperties> GetRootElementAsList()
	{
		return new List<IHasExposedProperties> { GetRootElement() };
	}

	public override string ToString()
	{
		return TargetObjectType.ToString();
	}
}
