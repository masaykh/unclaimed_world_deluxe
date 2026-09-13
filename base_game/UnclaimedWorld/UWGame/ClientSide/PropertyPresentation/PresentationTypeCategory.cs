using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.PropertyPresentation;

public class PresentationTypeCategory : IGameData, ICategoryType
{
	public bool Collapsable = true;

	public bool StartsAsExpanded = true;

	public int PanelSortOrder;

	public Sorting PrimarySortingOfItems;

	public Sorting SecondarySortingOfItems;

	public PresentationNode[] Nodes;

	public bool SetCountAsSummary;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
		if (Nodes == null)
		{
			return;
		}
		PresentationNode[] nodes = Nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] is GroupNode groupNode)
			{
				groupNode.IsOuterGroup = true;
			}
		}
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (Nodes != null)
		{
			PresentationNode[] nodes = Nodes;
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].PreInitValidate(ref listOfErrors);
			}
		}
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
