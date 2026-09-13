using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.PropertyPresentation;

internal class PresentationTypeCategoryProcessor
{
	private static List<IHasExposedProperties> hasPropertiesList = new List<IHasExposedProperties>();

	private static List<LeafNode> emptyPropertyPresentations = new List<LeafNode>();

	private static Dictionary<object, object> currentKeys = new Dictionary<object, object>();

	public static bool KeyedEntryComponentHasDataToShow(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, Func<string, string, float?, bool> canShowData)
	{
		Dictionary<object, PresentationData> dictionary = ProcessCategoryData(categoryToProcess, hasExposedProperties);
		if (dictionary.Count == 0)
		{
			return false;
		}
		foreach (KeyValuePair<object, PresentationData> item in dictionary)
		{
			if (canShowData(item.Value.Term, item.Value.IconName, item.Value.NormalizedValue))
			{
				return true;
			}
		}
		return false;
	}

	public static bool DisplayCategory(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties, IKeyedEntryComponent populatable, ref int? numberOfItems)
	{
		populatable.BeginAddingEntries();
		PresentationNode[] nodes = categoryToProcess.Nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].Display(categoryToProcess, hasExposedProperties, null, populatable, isIndented: false, ref currentKeys, ref numberOfItems);
		}
		CleanupAndSortEntries(populatable, categoryToProcess.PrimarySortingOfItems, categoryToProcess.SecondarySortingOfItems, currentKeys);
		populatable.EndAddingEntries();
		if (currentKeys != null && currentKeys.Count > 0)
		{
			currentKeys.Clear();
			return true;
		}
		return false;
	}

	public static void CleanupAndSortEntries(IKeyedEntryComponent populatable, Sorting primarySorting, Sorting secondarySorting, Dictionary<object, object> entryKeys)
	{
		CleanupUnusedEntries(populatable, entryKeys);
		populatable.CapNoOfEntries();
		Sort(populatable, primarySorting, secondarySorting);
	}

	private static void Sort(IKeyedEntryComponent populatable, Sorting primarySorting, Sorting secondarySorting)
	{
		if (populatable.Entries.Count > 0)
		{
			List<UIComponent> entriesToSort = populatable.Entries;
			Func<UIComponent, object> selector;
			Grid.Sorting sorting;
			if (primarySorting == null)
			{
				selector = Sorting.GetSelector(SortingMethod.StaticSortOrder);
				sorting = Grid.Sorting.Ascending;
			}
			else
			{
				selector = primarySorting.GetSelector();
				sorting = primarySorting.SortingDirection ?? Grid.Sorting.Ascending;
			}
			if (secondarySorting != null)
			{
				Func<UIComponent, object> selector2 = secondarySorting.GetSelector();
				Grid.Sort(selector, sorting, selector2, secondarySorting.SortingDirection ?? Grid.Sorting.Ascending, ref entriesToSort, null);
			}
			else
			{
				Grid.Sort(selector, sorting, ref entriesToSort, null);
			}
			populatable.Entries = entriesToSort;
			populatable.EndAddingEntries();
		}
	}

	private static void CleanupUnusedEntries(IKeyedEntryComponent entryComponent, Dictionary<object, object> entryKeys)
	{
		for (int i = 0; i < entryComponent.Count; i++)
		{
			object keyFromIndex = entryComponent.GetKeyFromIndex(i);
			if ((entryKeys == null || !entryKeys.ContainsKey(keyFromIndex)) && entryComponent.TryRemoveEntry(keyFromIndex))
			{
				i--;
			}
		}
	}

	private static void RemoveEmptyPresentations(Dictionary<LeafNode, List<PresentationData>> processedData)
	{
		foreach (LeafNode emptyPropertyPresentation in emptyPropertyPresentations)
		{
			processedData.Remove(emptyPropertyPresentation);
		}
		emptyPropertyPresentations.Clear();
	}

	public static void DetermineEntryKey(string caption, Presentation presentation, IHasExposedProperties hasExposedProperties, out string entryKey)
	{
		_ = presentation.PresentationTypeKey == "storageCapacityPresentation";
		hasExposedProperties.GetDefaultKey(out entryKey);
		entryKey = entryKey + caption + presentation.PropertyNameForValue + presentation.PresentationTypeKey;
	}

	private static float GetInvertedValue(float value)
	{
		return 1f - value;
	}

	private static Dictionary<object, PresentationData> ProcessCategoryData(PresentationTypeCategory categoryToProcess, IHasExposedProperties hasExposedProperties)
	{
		Dictionary<object, PresentationData> dictionary = new Dictionary<object, PresentationData>();
		PresentationNode[] nodes = categoryToProcess.Nodes;
		foreach (PresentationNode presentationNode in nodes)
		{
			if (presentationNode is LeafNode propertyPresentation)
			{
				ProcessLeaf(hasExposedProperties, propertyPresentation, dictionary);
				continue;
			}
			GroupNode groupNode = presentationNode as GroupNode;
			ProcessGroup(hasExposedProperties, groupNode, dictionary);
		}
		return dictionary;
	}

	private static void ProcessGroup(IHasExposedProperties hasExposedProperties, GroupNode groupNode, Dictionary<object, PresentationData> processedData)
	{
		if (groupNode.DynamicList != null)
		{
			List<IHasExposedProperties> listToFillWithProperties = new List<IHasExposedProperties>();
			hasExposedProperties.GetChildren(groupNode.DynamicList.HasPropertiesList, ref listToFillWithProperties, groupNode.DynamicList.Filter, null, null, null, null);
			{
				foreach (IHasExposedProperties item in listToFillWithProperties)
				{
					ProcessSingleObject(item, hasExposedProperties, item.KeyName, processedData, groupNode.DynamicList.Presentation);
				}
				return;
			}
		}
		PresentationNode[] nodes = groupNode.Nodes;
		foreach (PresentationNode presentationNode in nodes)
		{
			if (presentationNode is LeafNode propertyPresentation)
			{
				ProcessLeaf(hasExposedProperties, propertyPresentation, processedData);
				continue;
			}
			GroupNode groupNode2 = presentationNode as GroupNode;
			ProcessGroup(hasExposedProperties, groupNode2, processedData);
		}
	}

	private static void ProcessLeaf(IHasExposedProperties hasExposedProperties, LeafNode propertyPresentation, Dictionary<object, PresentationData> processedData)
	{
		ProcessSingleObject(hasExposedProperties, null, propertyPresentation, processedData, propertyPresentation.Presentation);
	}

	private static void ProcessSingleObject(IHasExposedProperties hasExposedProperties, IHasExposedProperties parent, object key, Dictionary<object, PresentationData> processedData, Presentation presentation)
	{
		PresentationType presentationTypeFromData = GetPresentationTypeFromData(presentation);
		PresentationNode.GetDataToDisplay(hasExposedProperties, parent, key, processedData, presentation, presentationTypeFromData, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _);
	}

	public static PresentationType GetPresentationTypeFromData(Presentation presentation)
	{
		if (presentation.PresentationTypeKey != null)
		{
			return GameData.Instance.AllPresentationTypes[presentation.PresentationTypeKey];
		}
		return null;
	}

	public static string DetermineKeyName(Presentation presentation, PropertyResult result, IHasExposedProperties hasExposedProperties)
	{
		if (presentation.KeyNameForTypeDependentPresentationToUse == Presentation.KeyNameForTypeDependentPresentation.Custom)
		{
			return result.PropertyKeyName;
		}
		return hasExposedProperties.KeyName;
	}
}
