using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

public class GroupNode : PresentationNode
{
	public enum HeaderType
	{
		None,
		Static,
		Dynamic
	}

	public bool HasBorder;

	public bool ShowConnectors;

	public GroupHeader GroupHeader;

	public PresentationNode[] Nodes;

	public DynamicList DynamicList;

	[XmlIgnore]
	public bool IsOuterGroup;

	private const int headingHeight = 25;

	private const int nestedGroupIndentation = 14;

	public override void Display(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, IKeyedEntryComponent populatable, bool isIndented, ref Dictionary<object, object> entryKeys, ref int? numberOfItems)
	{
		Dictionary<object, object> entryKeys2 = null;
		IKeyedEntryComponent keyedEntryComponent = null;
		if (!populatable.TryGetEntry(this, out var entry))
		{
			entry = populatable.AddGroup(this, HasBorder, (!IsOuterGroup) ? 14 : 0);
			entry.OrderByTag2 = SortOrder ?? 0;
		}
		keyedEntryComponent = (IKeyedEntryComponent)entry;
		Grid grid = keyedEntryComponent as Grid;
		keyedEntryComponent.BeginAddingEntries();
		HeaderType headerType = HeaderType.None;
		bool? flag = null;
		bool isIndented2 = false;
		if (GroupHeader != null)
		{
			headerType = GroupHeader.GetHeaderType();
			bool flag2 = DisplayEntry(categoryToProcess, hasExposedProperties, parent, keyedEntryComponent, GroupHeader.Presentation, 0, ref entryKeys2, 25, 5, 8, headerType == HeaderType.Static, showIconBackground: true);
			if (headerType == HeaderType.Dynamic && flag2)
			{
				flag = true;
			}
			isIndented2 = true;
			if (grid != null)
			{
				grid.TopMargin = 1;
				grid.BottomMargin = 1;
			}
		}
		List<string> childGroupsWithHeaders = null;
		if (DynamicList != null)
		{
			List<IHasExposedProperties> listToFillWithProperties = new List<IHasExposedProperties>();
			hasExposedProperties.GetChildren(DynamicList.HasPropertiesList, ref listToFillWithProperties, DynamicList.Filter, null, null, null, null, The.InGameUI.UIAllegiance.SharedKnowledge);
			if (SetCountAsSummary)
			{
				numberOfItems = listToFillWithProperties.Count;
			}
			int num = 1;
			foreach (IHasExposedProperties item in listToFillWithProperties)
			{
				DisplayEntry(categoryToProcess, item, hasExposedProperties, keyedEntryComponent, DynamicList.Presentation, num, ref entryKeys2, null, 19, 20);
				num++;
			}
			if (listToFillWithProperties.Count > 0 && grid != null)
			{
				grid.BottomMargin = 5;
				if (GroupHeader == null)
				{
					grid.TopMargin = 4;
				}
			}
		}
		else if (Nodes != null)
		{
			if (SetCountAsSummary)
			{
				numberOfItems = Nodes.Length;
			}
			PresentationNode[] nodes = Nodes;
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].Display(categoryToProcess, hasExposedProperties, null, keyedEntryComponent, isIndented2, ref entryKeys2, ref numberOfItems);
			}
		}
		PresentationTypeCategoryProcessor.CleanupAndSortEntries(keyedEntryComponent, null, null, entryKeys2);
		keyedEntryComponent.EndAddingEntries();
		if (entryKeys2 != null && ((headerType == HeaderType.None && entryKeys2.Count > 0) || (headerType == HeaderType.Dynamic && flag == true) || (headerType == HeaderType.Static && entryKeys2.Count > 1)))
		{
			Common.AddToDictionary(ref entryKeys, this, this);
			DisplayConnectors(grid, childGroupsWithHeaders);
		}
	}

	public override void PreInitValidate(ref List<string> listOfErrors)
	{
		if (GroupHeader != null)
		{
			GroupHeader.PreInitValidate(ref listOfErrors);
		}
		if (Nodes != null)
		{
			PresentationNode[] nodes = Nodes;
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].PreInitValidate(ref listOfErrors);
			}
		}
	}

	private void DisplayConnectors(Grid container, List<string> childGroupsWithHeaders)
	{
		if (!ShowConnectors)
		{
			return;
		}
		_ = container.Height;
		int num = 21;
		UIComponent uIComponent = container.FindChildById(UIComponent.DataControlID.Connector, firstLevelOnly: true);
		int x = (IsOuterGroup ? 9 : 15);
		if (uIComponent == null)
		{
			Image image = new Image(The.InGameUI.gui);
			image.SetSkinLocation(SkinState.Normal, The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("lcd_connectionline_vertical"));
			image.Position = new Point(x, num);
			image.ScaleImageToSizeOfControl = true;
			image.ID = UIComponent.DataControlID.Connector;
			container.Add(image);
			uIComponent = image;
		}
		List<UIComponent> foundChildren = null;
		container.FindChildrenById(UIComponent.DataControlID.HorizontalConnector, ref foundChildren, firstLevelOnly: true);
		int? num2 = null;
		foreach (UIComponent entry in container.Entries)
		{
			if (entry.Tag1 is GroupNode { GroupHeader: not null })
			{
				Image image2;
				if (foundChildren != null && foundChildren.Count > 0)
				{
					image2 = (Image)foundChildren[0];
					foundChildren.RemoveAt(0);
				}
				else
				{
					image2 = new Image(The.InGameUI.gui);
					image2.SetSkinLocation(SkinState.Normal, The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("lcd_connectionline_horizontal"));
					image2.ScaleImageToSizeOfControl = true;
					image2.Width = 13;
					image2.ID = UIComponent.DataControlID.HorizontalConnector;
					container.Add(image2);
				}
				num2 = entry.Y + 9;
				image2.Position = new Point(x, num2.Value);
			}
		}
		if (num2.HasValue)
		{
			uIComponent.Height = num2.Value - num;
		}
		else
		{
			UIComponent uIComponent2 = container.Entries[container.Count - 1];
			int num3 = uIComponent2.Y + (int)(0.85f * (float)uIComponent2.Height);
			uIComponent.Height = num3 - num;
		}
		if (foundChildren == null || foundChildren.Count <= 0)
		{
			return;
		}
		foreach (UIComponent item in foundChildren)
		{
			container.Remove(item);
		}
	}
}
