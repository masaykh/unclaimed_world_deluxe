using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.InGameEvents.PropertyObjects;

public class GetList
{
	public string HasPropertiesListKey;

	public FilterCondition FilterCondition;

	public GetList NextList;

	public int? MaxResults;

	public List<IHasExposedProperties> GetResult(List<IHasExposedProperties> context, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		List<IHasExposedProperties> listToFillWithProperties = new List<IHasExposedProperties>();
		if (context == null)
		{
			The.Sim.PlaySite.GetChildren(HasPropertiesListKey, ref listToFillWithProperties, FilterCondition, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		else
		{
			context[0].GetChildren(HasPropertiesListKey, ref listToFillWithProperties, FilterCondition, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		if (listToFillWithProperties.Count > 0)
		{
			if (MaxResults.HasValue && MaxResults.Value < listToFillWithProperties.Count)
			{
				for (int num = listToFillWithProperties.Count - 1; num >= MaxResults.Value; num--)
				{
					listToFillWithProperties.RemoveAt(num);
				}
			}
			if (NextList != null)
			{
				return NextList.GetResult(listToFillWithProperties, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			}
		}
		return listToFillWithProperties;
	}
}
