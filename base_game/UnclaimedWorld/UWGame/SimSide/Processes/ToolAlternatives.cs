using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes;

public class ToolAlternatives
{
	public Tool[] Tools;

	[XmlIgnore]
	public List<Tuple<EntityType, float>> ToolsAndProductivity;

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		Tool[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i].PreDataCompleteValidate(ref listOfErrors);
		}
	}

	public void PostDataCompleteInitialize()
	{
		ToolsAndProductivity = new List<Tuple<EntityType, float>>();
		Tool[] tools = Tools;
		foreach (Tool item in tools)
		{
			item.PostDataCompleteInitialize();
			ToolsAndProductivity.AddRange(item.ToolEntityTypes.Select((EntityType t) => new Tuple<EntityType, float>(t, item.ProductivityFactor.Value)));
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
		Tool[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i].PreInitValidate(ref errors);
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		Tool[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i].PostInitValidate(ref listOfErrors);
		}
		if (ToolsAndProductivity.Count <= 1)
		{
			return;
		}
		List<EntityType> list = new List<EntityType>();
		for (int j = 0; j < ToolsAndProductivity.Count; j++)
		{
			Tuple<EntityType, float> tuple = ToolsAndProductivity[j];
			for (int k = 0; k < ToolsAndProductivity.Count; k++)
			{
				if (j != k)
				{
					Tuple<EntityType, float> tuple2 = ToolsAndProductivity[k];
					if (tuple.Item1 == tuple2.Item1 && !list.Contains(tuple.Item1))
					{
						list.Add(tuple.Item1);
						EntityType.CreateValidationError(ref listOfErrors, tuple.Item1.KeyName + " is duplicated in the tool options set. Overlapping tags?");
					}
				}
			}
		}
	}

	public bool NeedsImmobileTool()
	{
		Tool[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			foreach (EntityType toolEntityType in tools[i].ToolEntityTypes)
			{
				if (!ToolType.IsImmovable(toolEntityType))
				{
					return false;
				}
			}
		}
		return true;
	}
}
