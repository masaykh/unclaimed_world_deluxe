using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Inventory;

public class InventorySettings : ISnapshot
{
	public enum SortColumns
	{
		Name,
		InStock,
		CanProduce,
		Tracking
	}

	public enum Availability
	{
		AvailableNow,
		Attainable,
		AllKnownBlueprints
	}

	public enum AndOr
	{
		Or,
		And
	}

	public enum InOutOrTool
	{
		Input,
		Output,
		Tool
	}

	public SortingSettings<SortColumns> SortingSettings;

	public FilterPropertySettings FilterPropertySettings;

	private List<ProcessType> processesToDo;

	public Dictionary<EntityType, InventoryPanel.Availability> AllAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private Regulator attainabilityRegulator = new Regulator(The.Client.ClientRandomGenerator, 0.3, "InventorySettings");

	private bool settingsAreDirty = true;

	private Availability availabilitySettings;

	private AndOr andOrSetting;

	public bool IsExpanded = true;

	private List<Color> AvailableColors;

	public const string barTooltipBeingTracked = "Being tracked";

	public Dictionary<EntityType, TrackTarget> TrackedTargets = new Dictionary<EntityType, TrackTarget>();

	private Dictionary<EntityType, List<TrackTarget>> InputForTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();

	private Dictionary<EntityType, List<TrackTarget>> OutputFromTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();

	private Dictionary<EntityType, List<TrackTarget>> ToolForTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();

	private bool includeSalvageProcesses;

	private HashSet<EntityType> baseData;

	private HashSet<EntityType> staticData;

	private HashSet<EntityType> gameStateDependentData = new HashSet<EntityType>();

	private HashSet<EntityType> isOwnedOrTradable = new HashSet<EntityType>();

	private Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();

	private Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfoInProgress = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();

	private HashSet<EntityType> beingEvaluated = new HashSet<EntityType>();

	private const int noOfProcessesToComputePerFrame = 50;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Availability AvailabilitySettings
	{
		get
		{
			return availabilitySettings;
		}
		set
		{
			if (availabilitySettings != value)
			{
				availabilitySettings = value;
			}
		}
	}

	public AndOr AndOrSetting
	{
		get
		{
			return andOrSetting;
		}
		set
		{
			if (andOrSetting != value)
			{
				andOrSetting = value;
				settingsAreDirty = true;
			}
		}
	}

	public bool IncludeSalvageProcesses
	{
		get
		{
			return includeSalvageProcesses;
		}
		set
		{
			if (value == includeSalvageProcesses)
			{
				return;
			}
			includeSalvageProcesses = value;
			foreach (KeyValuePair<EntityType, TrackTarget> trackedTarget in TrackedTargets)
			{
				trackedTarget.Value.RecomputeRelatedEntityTypes();
			}
			RecomputeTrackedSuperSets();
		}
	}

	public bool IsSnapshotted { get; set; }

	public event Action TrackTargetsChanged;

	public static bool FilterProductionManagerItems(EntityType entityType)
	{
		if (entityType.TerrainType == null && entityType.TreeType == null && entityType.BiologicalType == null && (entityType.StructureType != null || entityType.ItemType != null))
		{
			return true;
		}
		return false;
	}

	public static HashSet<EntityType> GetBaseData(ref HashSet<EntityType> cachedBaseData)
	{
		if (cachedBaseData == null)
		{
			cachedBaseData = new HashSet<EntityType>();
			foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
			{
				if (FilterProductionManagerItems(allEntityType.Value))
				{
					cachedBaseData.Add(allEntityType.Value);
				}
			}
		}
		return cachedBaseData;
	}

	public HashSet<EntityType> GetData(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		settingsAreDirty = settingsAreDirty || FilterPropertySettings.SettingsAreDirty;
		if (settingsAreDirty)
		{
			HashSet<EntityType> filteredEntities = GetFilteredEntities();
			HashSet<EntityType> trackedEntities = GetTrackedEntities();
			if (AndOrSetting == AndOr.Or)
			{
				staticData = trackedEntities.Union(filteredEntities).ToHashSet();
			}
			else if (trackedEntities.Count == 0)
			{
				staticData = filteredEntities;
			}
			else
			{
				staticData = trackedEntities.Intersect(filteredEntities).ToHashSet();
			}
			settingsAreDirty = false;
			FilterPropertySettings.SetSettingsNotDirty();
		}
		return GetGameStateDependentData(allAvailableItems);
	}

	public TrackTarget GetFirstTrackedTarget()
	{
		if (TrackedTargets.Count > 0)
		{
			Dictionary<EntityType, TrackTarget>.Enumerator enumerator = TrackedTargets.GetEnumerator();
			enumerator.MoveNext();
			return enumerator.Current.Value;
		}
		return null;
	}

	private HashSet<EntityType> GetTrackedEntities()
	{
		List<HashSet<EntityType>> list = new List<HashSet<EntityType>>();
		HashSet<EntityType> result = new HashSet<EntityType>();
		if (TrackedTargets != null)
		{
			foreach (KeyValuePair<EntityType, TrackTarget> trackedTarget in TrackedTargets)
			{
				result = new HashSet<EntityType>();
				if (trackedTarget.Value.ShowInputs)
				{
					result = trackedTarget.Value.InputForTrackTarget.Union(result).ToHashSet();
				}
				if (trackedTarget.Value.ShowOutputs)
				{
					result = trackedTarget.Value.OutputForTrackTarget.Union(result).ToHashSet();
				}
				if (trackedTarget.Value.ShowTools)
				{
					result = trackedTarget.Value.ToolsForTrackTarget.Union(result).ToHashSet();
				}
				if (!result.Contains(trackedTarget.Value.EntityType))
				{
					result.Add(trackedTarget.Value.EntityType);
				}
				list.Add(result);
			}
			result = new HashSet<EntityType>();
			if (list.Count > 0)
			{
				result = ((AndOrSetting != AndOr.Or) ? list.Aggregate((HashSet<EntityType> previousList, HashSet<EntityType> nextList) => previousList.Intersect(nextList).ToHashSet()) : list.Aggregate((HashSet<EntityType> previousList, HashSet<EntityType> nextList) => previousList.Union(nextList).ToHashSet()));
			}
		}
		return result;
	}

	private void RecomputeTrackedSuperSets()
	{
		InputForTrackTargets.Clear();
		OutputFromTrackTargets.Clear();
		ToolForTrackTargets.Clear();
		foreach (KeyValuePair<EntityType, TrackTarget> trackedTarget in TrackedTargets)
		{
			AddTrackedTargetToSupersets(trackedTarget.Value);
		}
	}

	private HashSet<EntityType> GetFilteredEntities()
	{
		HashSet<EntityType> result = new HashSet<EntityType>();
		if (FilterPropertySettings.HasActiveFilters())
		{
			staticData = new HashSet<EntityType>();
			List<HashSet<EntityType>> filteredEntities = FilterPropertySettings.GetFilteredEntities(FilterProductionManagerItems);
			if (filteredEntities.Count > 0)
			{
				result = ((AndOrSetting != AndOr.Or) ? filteredEntities.Aggregate((HashSet<EntityType> previousList, HashSet<EntityType> nextList) => previousList.Intersect(nextList).ToHashSet()) : filteredEntities.Aggregate((HashSet<EntityType> previousList, HashSet<EntityType> nextList) => previousList.Union(nextList).ToHashSet()));
			}
		}
		else
		{
			result = GetBaseData(ref baseData);
		}
		return result;
	}

	private HashSet<EntityType> GetGameStateDependentData(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		if (The.InGameUI.GetExpedition() == null)
		{
			return staticData;
		}
		gameStateDependentData.Clear();
		foreach (EntityType staticDatum in staticData)
		{
			int noOfIncompleteEntities;
			int noOfEntitiesUsedAsParts;
			int noOfItemsOnOtherSite;
			int noOfItemsOwnedByOthers;
			int noOfAvailableItemsIncludingIntrinsic;
			int noOfAvailableEntities = InventoryPanel.GetNoOfAvailableEntities(entityGroup.AllEntities, entityGroup, staticDatum, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic, allAvailableItems);
			bool flag = false;
			if (staticDatum.ItemType != null)
			{
				flag = entityGroup.ProductionOrders.OrdersExist(staticDatum);
			}
			bool flag2 = InventoryPanel.CanBuildNow(staticDatum, entityGroup, includeSalvageProcesses);
			bool flag3 = false;
			if (noOfAvailableEntities > 0 || noOfEntitiesUsedAsParts > 0 || noOfIncompleteEntities > 0 || noOfItemsOnOtherSite > 0 || flag || flag2)
			{
				flag3 = true;
			}
			attainableInfo.TryGetValue(staticDatum, out var value);
			bool flag4 = false;
			switch (AvailabilitySettings)
			{
			case Availability.AvailableNow:
				flag4 = flag3;
				break;
			case Availability.Attainable:
				if (flag3 || (value != null && value.Any((KeyValuePair<ProcessType, AttainableInfo> a) => a.Value.IsAttainable)))
				{
					flag4 = true;
				}
				break;
			case Availability.AllKnownBlueprints:
				flag4 = true;
				break;
			}
			if (flag4)
			{
				gameStateDependentData.Add(staticDatum);
			}
		}
		return gameStateDependentData;
	}

	public void Update(GameTime gameTime)
	{
		if (processesToDo == null && attainabilityRegulator.IsReady())
		{
			StartRecomputeAttainability();
		}
		if (processesToDo != null && CycleRecomputeAttainability())
		{
			EndRecomputeAttainability();
		}
	}

	private bool CycleRecomputeAttainability()
	{
		int num = 0;
		EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		bool flag = true;
		while (flag)
		{
			if (num > 50)
			{
				return false;
			}
			flag = false;
			for (int num2 = processesToDo.Count - 1; num2 >= 0; num2--)
			{
				ProcessType processType = processesToDo[num2];
				if (ProcessCanProduce(processType, AllAvailableItems, owner, gatherMissingInfo: false))
				{
					processesToDo.RemoveAt(num2);
					Output[] outputs = processType.Outputs;
					foreach (Output output in outputs)
					{
						if (!output.IsWasteProduct)
						{
							if (!attainableInfoInProgress.TryGetValue(output.FinalEntityTypeToCreate, out var value))
							{
								value = new Dictionary<ProcessType, AttainableInfo>();
								attainableInfoInProgress.Add(output.FinalEntityTypeToCreate, value);
								flag = true;
							}
							if (!value.TryGetValue(processType, out var value2))
							{
								value2 = new AttainableInfo(1);
								value.Add(processType, value2);
							}
							value2.IsProducable = true;
						}
					}
				}
				num++;
			}
		}
		return true;
	}

	private void EndRecomputeAttainability()
	{
		EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		foreach (ProcessType item in processesToDo)
		{
			ProcessCanProduce(item, AllAvailableItems, owner, gatherMissingInfo: true);
		}
		attainableInfo = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();
		foreach (KeyValuePair<EntityType, Dictionary<ProcessType, AttainableInfo>> item2 in attainableInfoInProgress)
		{
			attainableInfo.Add(item2.Key, new Dictionary<ProcessType, AttainableInfo>(item2.Value));
		}
		attainableInfoInProgress.Clear();
		isOwnedOrTradable.Clear();
		processesToDo = null;
		// UNHIDDEN MOD: the added item is producible but would never be offered, because
		// attainability is computed from the tables as they stood before the addition.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			UWGame.Mods.UnhiddenMod.RegisterAttainability(attainableInfo);
		}
	}

	private void StartRecomputeAttainability()
	{
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		AllAvailableItems.Clear();
		InventoryPanel.CountAllEntities(entityGroup, AllAvailableItems, countItemsToBuy: true);
		attainableInfoInProgress.Clear();
		isOwnedOrTradable.Clear();
		foreach (KeyValuePair<EntityType, List<EntityID>> allEntity in entityGroup.AllEntities)
		{
			InventoryPanel.Availability availability = AllAvailableItems[allEntity.Key];
			if (availability.NoOfAvailableItems > 0 || availability.AvailableToBuy > 0)
			{
				isOwnedOrTradable.Add(allEntity.Key);
			}
		}
		if (includeSalvageProcesses)
		{
			processesToDo = new List<ProcessType>(GameData.Instance.AllProductionProcesses);
		}
		else
		{
			processesToDo = new List<ProcessType>(GameData.Instance.NonSalvageProductionProcesses);
		}
	}

	private bool IsAttainable(EntityType entityType)
	{
		if (isOwnedOrTradable.Contains(entityType))
		{
			return true;
		}
		if (attainableInfoInProgress.TryGetValue(entityType, out var value))
		{
			return value.Any((KeyValuePair<ProcessType, AttainableInfo> a) => a.Value.IsAttainable);
		}
		return false;
	}

	private bool ProcessCanProduce(ProcessType process, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, EntityGroup owner, bool gatherMissingInfo)
	{
		ResourceType resourceType = null;
		TierOrAreaType tierOrAreaType = null;
		SkillType skillType = null;
		EntityType entityType = null;
		List<EntityType> list = null;
		List<EntityType> list2 = null;
		SkillType requiredSkillType = process.RequiredSkillType;
		if (!owner.GetExpedition().HasSkill(requiredSkillType))
		{
			if (!gatherMissingInfo)
			{
				return false;
			}
			skillType = process.RequiredSkillType;
		}
		if (process.ResourceTypeInput != null && !HasResources(process))
		{
			if (!gatherMissingInfo)
			{
				return false;
			}
			resourceType = process.ResourceTypeInput;
		}
		if (owner.Parent is Expedition expedition && !expedition.Policy.CanUseProcess(process, out var policy))
		{
			if (!gatherMissingInfo)
			{
				return false;
			}
			tierOrAreaType = policy;
		}
		if (process.ActingOnType != null && !HasSpecialSite(process))
		{
			if (!gatherMissingInfo)
			{
				return false;
			}
			entityType = process.ActingOnType;
		}
		if (process.InputsByType != null)
		{
			foreach (KeyValuePair<EntityType, Input> item2 in process.InputsByType)
			{
				if (item2.Key.TreeType == null && !IsAttainable(item2.Key))
				{
					if (gatherMissingInfo)
					{
						Common.AddToList(ref list, item2.Key);
						break;
					}
					return false;
				}
			}
		}
		if (process.ProcessToolSet != null)
		{
			ToolAlternatives[] tools = process.ProcessToolSet.Tools;
			foreach (ToolAlternatives toolAlternatives in tools)
			{
				bool flag = false;
				foreach (Tuple<EntityType, float> item3 in toolAlternatives.ToolsAndProductivity)
				{
					EntityType item = item3.Item1;
					if (IsAttainable(item))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				if (gatherMissingInfo)
				{
					foreach (Tuple<EntityType, float> item4 in toolAlternatives.ToolsAndProductivity)
					{
						Common.AddToList(ref list2, item4.Item1);
					}
					break;
				}
				return false;
			}
		}
		if (resourceType == null && skillType == null && list == null && list2 == null && entityType == null && tierOrAreaType == null)
		{
			return true;
		}
		if (gatherMissingInfo)
		{
			Output[] outputs = process.Outputs;
			foreach (Output output in outputs)
			{
				if (!output.IsWasteProduct)
				{
					if (!attainableInfoInProgress.TryGetValue(output.FinalEntityTypeToCreate, out var value))
					{
						value = new Dictionary<ProcessType, AttainableInfo>();
						attainableInfoInProgress[output.FinalEntityTypeToCreate] = value;
					}
					if (!value.TryGetValue(process, out var value2))
					{
						value2 = new AttainableInfo(-1);
						value.Add(process, value2);
					}
					value2.UnavailableResource = resourceType;
					value2.UnavailablePolicy = tierOrAreaType;
					value2.UnavailableSkill = skillType;
					value2.UnavailableInputs = list;
					value2.UnavailableTools = list2;
					value2.UnavailableSpecialSite = entityType;
				}
			}
		}
		return false;
	}

	public Dictionary<ProcessType, AttainableInfo> GetAttainableInfo(EntityType entityType)
	{
		attainableInfo.TryGetValue(entityType, out var value);
		return value;
	}

	public static bool HasSpecialSite(ProcessType process)
	{
		if (process.ActingOnType.IntelligenceType == null)
		{
			if (The.InGameUI.UIAllegiance.SharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(process.ActingOnType, out var value) && value.Count > 0)
			{
				return true;
			}
		}
		else if (The.InGameUI.UIAllegiance.Members.Any((Entity m) => m.EntityType == process.ActingOnType))
		{
			return true;
		}
		return false;
	}

	public static bool HasResources(ProcessType process)
	{
		if (The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers.TryGetValue(process.ResourceTypeInput, out var value) && value.Count > 0 && value.Any((ResourceID r) => HasResourceItems(r)))
		{
			return true;
		}
		return false;
	}

	private static bool HasResourceItems(ResourceID resourceID)
	{
		if (LookUp<ResourceContainer, ResourceID>.FindByID(resourceID).NoOfHarvestableItems > 0)
		{
			return true;
		}
		return false;
	}

	public InventorySettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			FilterPropertySettings = new FilterPropertySettings();
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
		}
		AvailableColors = GameData.Instance.GUIConstants.TrackingColors.ToList();
	}

	public bool GetTrackedColorAndTooltip(EntityType entityType, out Color? color, out string toolTip)
	{
		toolTip = null;
		color = null;
		bool isTrackTarget = false;
		if (entityType != null)
		{
			IsTracked(entityType, out isTrackTarget, out var _, out var _, out var _, out var inputForTrackedEntityTypes, out var outputFromTrackedEntityTypes, out var toolForTrackedEntityTypes);
			color = GetRowColorAndToolTip(inputForTrackedEntityTypes, outputFromTrackedEntityTypes, toolForTrackedEntityTypes, out toolTip);
			if (isTrackTarget)
			{
				TrackedTargets.TryGetValue(entityType, out var value);
				color = value.Color;
				toolTip = "Being tracked" + ((toolTip == null) ? null : (" \n" + toolTip));
			}
			if (!color.HasValue)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static Color? GetRowColorAndToolTip(List<TrackTarget> inputForTrackedEntityTypes, List<TrackTarget> outputFromTrackedEntityTypes, List<TrackTarget> toolForTrackedEntityTypes, out string toolTip)
	{
		Color? color = null;
		int noOfColorChanges = 0;
		toolTip = "";
		string modifiedToolTip = "";
		if (inputForTrackedEntityTypes != null)
		{
			color = GetColorFromCollections(InOutOrTool.Input, inputForTrackedEntityTypes, color, noOfColorChanges, out noOfColorChanges, modifiedToolTip, out modifiedToolTip);
			toolTip += modifiedToolTip;
			modifiedToolTip = "";
		}
		if (outputFromTrackedEntityTypes != null)
		{
			color = GetColorFromCollections(InOutOrTool.Output, outputFromTrackedEntityTypes, color, noOfColorChanges, out noOfColorChanges, modifiedToolTip, out modifiedToolTip);
			toolTip += modifiedToolTip;
			modifiedToolTip = "";
		}
		if (toolForTrackedEntityTypes != null)
		{
			color = GetColorFromCollections(InOutOrTool.Tool, toolForTrackedEntityTypes, color, noOfColorChanges, out noOfColorChanges, modifiedToolTip, out modifiedToolTip);
			toolTip += modifiedToolTip;
			modifiedToolTip = "";
		}
		toolTip = ((toolTip != "") ? ("This item is:" + toolTip) : null);
		return color;
	}

	private static Color? GetColorFromCollections(InOutOrTool status, List<TrackTarget> trackTargets, Color? color, int startAmount, out int noOfColorChanges, string toolTip, out string modifiedToolTip)
	{
		noOfColorChanges = startAmount;
		modifiedToolTip = toolTip;
		foreach (TrackTarget trackTarget in trackTargets)
		{
			bool flag = false;
			switch (status)
			{
			case InOutOrTool.Input:
				flag = trackTarget.ShowInputs;
				break;
			case InOutOrTool.Output:
				flag = trackTarget.ShowOutputs;
				break;
			case InOutOrTool.Tool:
				flag = trackTarget.ShowTools;
				break;
			}
			if (flag)
			{
				if (color != trackTarget.Color)
				{
					color = trackTarget.Color;
					noOfColorChanges++;
				}
				if (noOfColorChanges > 1)
				{
					color = Color.White;
				}
				modifiedToolTip = modifiedToolTip + "\n - " + trackTarget.EntityType.Name;
			}
		}
		if (trackTargets.Count > 0 && modifiedToolTip != toolTip)
		{
			switch (status)
			{
			case InOutOrTool.Input:
				modifiedToolTip = "\n Input for:\n" + modifiedToolTip;
				break;
			case InOutOrTool.Output:
				modifiedToolTip = "\n Output from:\n" + modifiedToolTip;
				break;
			case InOutOrTool.Tool:
				modifiedToolTip = "\n Tool for making:\n" + modifiedToolTip;
				break;
			}
		}
		return color;
	}

	public bool IsTracked(EntityType entityType, out bool isTrackTarget, out bool isTrackedInput, out bool isTrackedOutput, out bool isTrackedTool, out List<TrackTarget> inputForTrackedEntityTypes, out List<TrackTarget> outputFromTrackedEntityTypes, out List<TrackTarget> toolForTrackedEntityTypes)
	{
		TrackedTargets.TryGetValue(entityType, out var value);
		isTrackTarget = value != null;
		isTrackedInput = InputForTrackTargets.TryGetValue(entityType, out inputForTrackedEntityTypes);
		isTrackedOutput = OutputFromTrackTargets.TryGetValue(entityType, out outputFromTrackedEntityTypes);
		isTrackedTool = ToolForTrackTargets.TryGetValue(entityType, out toolForTrackedEntityTypes);
		return isTrackTarget;
	}

	public bool HasAvailableTrackingSlots()
	{
		return AvailableColors.Count > 0;
	}

	public void StartTracking(EntityType entityType)
	{
		TrackTarget trackTarget = new TrackTarget(entityType, showInputs: false, showOutputs: false, showTools: false);
		Color item = (trackTarget.Color = AvailableColors[0]);
		AvailableColors.Remove(item);
		TrackedTargets.Add(entityType, trackTarget);
		AddTrackTarget(trackTarget);
	}

	private void AddTrackTarget(TrackTarget trackedEntityType)
	{
		AddTrackedTargetToSupersets(trackedEntityType);
		if (this.TrackTargetsChanged != null)
		{
			this.TrackTargetsChanged();
		}
	}

	private void AddTrackedTargetToSupersets(TrackTarget trackedEntityType)
	{
		if (trackedEntityType.InputForTrackTarget != null)
		{
			foreach (EntityType item in trackedEntityType.InputForTrackTarget)
			{
				Common.AddToMultiList(InputForTrackTargets, item, trackedEntityType);
			}
		}
		if (trackedEntityType.OutputForTrackTarget != null)
		{
			foreach (EntityType item2 in trackedEntityType.OutputForTrackTarget)
			{
				Common.AddToMultiList(OutputFromTrackTargets, item2, trackedEntityType);
			}
		}
		if (trackedEntityType.ToolsForTrackTarget == null)
		{
			return;
		}
		foreach (EntityType item3 in trackedEntityType.ToolsForTrackTarget)
		{
			Common.AddToMultiList(ToolForTrackTargets, item3, trackedEntityType);
		}
	}

	public bool ToggleTracking(EntityType entityType)
	{
		settingsAreDirty = true;
		if (!TrackedTargets.TryGetValue(entityType, out var value))
		{
			if (HasAvailableTrackingSlots())
			{
				StartTracking(entityType);
				return true;
			}
			return false;
		}
		StopTracking(value);
		return false;
	}

	public void StopTracking(TrackTarget trackedEntityType)
	{
		AvailableColors.Add(trackedEntityType.Color);
		TrackedTargets.Remove(trackedEntityType.EntityType);
		if (trackedEntityType.InputForTrackTarget != null)
		{
			foreach (EntityType item in trackedEntityType.InputForTrackTarget)
			{
				Common.RemoveFromMultiList(InputForTrackTargets, item, trackedEntityType, removeEmptyList: true);
			}
		}
		if (trackedEntityType.OutputForTrackTarget != null)
		{
			foreach (EntityType item2 in trackedEntityType.OutputForTrackTarget)
			{
				Common.RemoveFromMultiList(OutputFromTrackTargets, item2, trackedEntityType, removeEmptyList: true);
			}
		}
		if (trackedEntityType.ToolsForTrackTarget != null)
		{
			foreach (EntityType item3 in trackedEntityType.ToolsForTrackTarget)
			{
				Common.RemoveFromMultiList(ToolForTrackTargets, item3, trackedEntityType, removeEmptyList: true);
			}
		}
		if (this.TrackTargetsChanged != null)
		{
			this.TrackTargetsChanged();
		}
	}

	public void TrackTargetSettingsChanged()
	{
		settingsAreDirty = true;
		if (this.TrackTargetsChanged != null)
		{
			this.TrackTargetsChanged();
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IsExpanded = sn.DoBool(IsExpanded);
		includeSalvageProcesses = sn.DoBool(includeSalvageProcesses);
		AvailabilitySettings = sn.DoEnum(AvailabilitySettings);
		TrackedTargets = sn.DoDictionary(TrackedTargets);
		AvailableColors = sn.DoList(AvailableColors);
		AndOrSetting = sn.DoEnum(AndOrSetting);
		settingsAreDirty = sn.DoBool(settingsAreDirty);
		staticData = sn.DoHashSet(staticData);
		FilterPropertySettings = (FilterPropertySettings)sn.DoISnapshot(FilterPropertySettings);
		SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);
		sn.Ignore(gameStateDependentData);
		sn.Ignore(beingEvaluated);
		sn.Ignore(baseData);
		sn.Ignore(this.TrackTargetsChanged);
		sn.Ignore(attainableInfo);
		sn.Ignore(attainableInfoInProgress);
		sn.Ignore(isOwnedOrTradable);
		sn.Ignore(processesToDo);
		sn.Ignore(AllAvailableItems);
		sn.Ignore(ToolForTrackTargets);
		sn.Ignore(InputForTrackTargets);
		sn.Ignore(OutputFromTrackTargets);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		FilterPropertySettings.LoadPostProcess(sn);
		SortingSettings.LoadPostProcess(sn);
		foreach (KeyValuePair<EntityType, TrackTarget> trackedTarget in TrackedTargets)
		{
			trackedTarget.Value.LoadPostProcess(sn);
			AddTrackTarget(trackedTarget.Value);
		}
	}
}
