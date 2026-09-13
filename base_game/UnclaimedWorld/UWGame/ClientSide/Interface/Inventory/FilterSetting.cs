using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface.Inventory;

public class FilterSetting
{
	public FilterSettingType FilterSettingType;

	private HashSet<EntityType> Results;

	public FilterSetting()
	{
	}

	public FilterSetting(FilterSettingType type)
	{
		FilterSettingType = type;
	}

	public string GetDisplayString()
	{
		return FilterSettingType.GetDisplayName();
	}

	public HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		if (Results == null)
		{
			Results = FilterSettingType.GetData(filter);
		}
		return Results;
	}
}
