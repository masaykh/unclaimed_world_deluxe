using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes;

[DebuggerDisplay("{KeyName}")]
public class ProcessToolSet : IGameData
{
	private class ToolData
	{
		public EntityType ToolEntityType;

		public float Productivity;

		public float DegradePerSecond;
	}

	public ToolAlternatives[] Tools;

	public string Comments;

	[XmlIgnore]
	public List<ToolTypeCombination> ToolTypeCombinations;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
	}

	private IEnumerable<IEnumerable<ToolData>> GetAllToolCombos()
	{
		List<List<ToolData>> list = new List<List<ToolData>>();
		if (Tools.Length != 0)
		{
			ToolAlternatives[] tools = Tools;
			foreach (ToolAlternatives obj in tools)
			{
				List<ToolData> list2 = new List<ToolData>();
				Tool[] tools2 = obj.Tools;
				foreach (Tool t in tools2)
				{
					list2.AddRange(t.ToolEntityTypes.Select((EntityType te) => new ToolData
					{
						ToolEntityType = te,
						Productivity = t.ProductivityFactor.Value,
						DegradePerSecond = t.DegradePerSecondOfUse.Value
					}));
				}
				list.Add(list2);
			}
		}
		return Common.CartesianProduct(list);
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (Tools != null)
		{
			ToolAlternatives[] tools = Tools;
			for (int i = 0; i < tools.Length; i++)
			{
				tools[i].PreInitValidate(ref listOfErrors);
			}
		}
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Tools != null)
		{
			ToolAlternatives[] tools = Tools;
			for (int i = 0; i < tools.Length; i++)
			{
				tools[i].PreDataCompleteValidate(ref listOfErrors);
			}
		}
	}

	public void PostDataCompleteInitialize()
	{
		if (Tools == null)
		{
			return;
		}
		ToolAlternatives[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i].PostDataCompleteInitialize();
		}
		IEnumerable<IEnumerable<ToolData>> allToolCombos = GetAllToolCombos();
		ToolTypeCombinations = new List<ToolTypeCombination>();
		foreach (IEnumerable<ToolData> item2 in allToolCombos)
		{
			ToolTypeCombination item = new ToolTypeCombination
			{
				Tools = item2.Select((ToolData t) => new Tuple<EntityType, float>(t.ToolEntityType, t.DegradePerSecond)).ToList(),
				Productivity = item2.Average((ToolData t) => t.Productivity)
			};
			ToolTypeCombinations.Add(item);
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Tools == null)
		{
			return;
		}
		ToolAlternatives[] tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			tools[i].PostDataCompleteValidate(ref listOfErrors);
		}
		bool flag = false;
		tools = Tools;
		for (int i = 0; i < tools.Length; i++)
		{
			if (tools[i].NeedsImmobileTool())
			{
				if (flag)
				{
					EntityType.CreateValidationError(ref listOfErrors, "Only one immobile tool may be required");
				}
				else
				{
					flag = true;
				}
			}
		}
		foreach (ToolTypeCombination toolTypeCombination in ToolTypeCombinations)
		{
			if (!CanCarryAllTools(toolTypeCombination.Tools))
			{
				EntityType.CreateValidationError(ref listOfErrors, "All mobile tools must be able to be carried at once (total bulk <= 1)" + string.Join(",", toolTypeCombination.Tools.Select((Tuple<EntityType, float> p) => p.Item1.KeyName.ToString())));
			}
		}
	}

	private bool CanCarryAllTools(List<Tuple<EntityType, float>> tools)
	{
		float num = 0f;
		foreach (Tuple<EntityType, float> tool in tools)
		{
			if (!ToolType.IsImmovable(tool.Item1))
			{
				num += tool.Item1.ItemType.MaximumBulk.Value;
			}
		}
		return num <= 1f;
	}

	public List<EntityType> GetImmobileToolChoices()
	{
		List<EntityType> list = new List<EntityType>();
		if (Tools != null)
		{
			ToolAlternatives[] tools = Tools;
			for (int i = 0; i < tools.Length; i++)
			{
				Tool[] tools2 = tools[i].Tools;
				for (int j = 0; j < tools2.Length; j++)
				{
					foreach (EntityType toolEntityType in tools2[j].ToolEntityTypes)
					{
						if (ToolType.IsImmovable(toolEntityType))
						{
							list.Add(toolEntityType);
							break;
						}
					}
				}
			}
		}
		return list;
	}
}
