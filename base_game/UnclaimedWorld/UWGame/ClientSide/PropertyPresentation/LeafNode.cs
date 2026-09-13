using System.Collections.Generic;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

public class LeafNode : PresentationNode
{
	public Presentation Presentation;

	public override void Display(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, IKeyedEntryComponent populatable, bool isIndented, ref Dictionary<object, object> entryKeys, ref int? numberOfItems)
	{
		DisplayEntry(categoryToProcess, hasExposedProperties, parent, populatable, Presentation, SortOrder, ref entryKeys, null, isIndented ? 19 : 5, isIndented ? 20 : 8);
	}

	public override void PreInitValidate(ref List<string> listOfErrors)
	{
		Presentation.PreInitValidate(ref listOfErrors);
	}
}
