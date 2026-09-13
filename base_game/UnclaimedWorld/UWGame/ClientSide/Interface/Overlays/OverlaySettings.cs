using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Overlays;

public class OverlaySettings : ISnapshot
{
	public static Color threatColor = "#FF4C58".ColorFromHex();

	public static Color buildableColor = "#BCFF59".ColorFromHex();

	public static Color personsColor = "#009cff".ColorFromHex();

	public static Color animalsColor = "#ff0066".ColorFromHex();

	public static Color animalsFaded = "#b50048".ColorFromHex();

	public static Color structuresColor = "#FFFFFF".ColorFromHex();

	public static Color structuresFaded = "#c5c5c5".ColorFromHex();

	public static Color interestColor = "#ff8400".ColorFromHex();

	public static Color interestFaded = "#cd6b02".ColorFromHex();

	public Dictionary<OverlayTypes, bool> OverlayTypeSettings = new Dictionary<OverlayTypes, bool>();

	public Dictionary<EditorOverlayTypes, bool> EditorOverlayTypeSettings = new Dictionary<EditorOverlayTypes, bool>();

	public Dictionary<ResourceCategory, bool> ResourceCategoriesToDisplay = new Dictionary<ResourceCategory, bool>();

	public Dictionary<ResourceType, bool> ResourceTypesToDisplay = new Dictionary<ResourceType, bool>();

	public Dictionary<EntityGrouping, bool> EntityTypeGroupingsToDisplay = new Dictionary<EntityGrouping, bool>();

	public Dictionary<EntityType, bool> EntityTypesToDisplay = new Dictionary<EntityType, bool>();

	private Dictionary<EntityGrouping, List<EntityType>> entityCategoryItemSettings = new Dictionary<EntityGrouping, List<EntityType>>();

	private Dictionary<ResourceCategory, List<ResourceType>> resourceCategoryItemSettings = new Dictionary<ResourceCategory, List<ResourceType>>();

	public bool ShowOverlaysOnGameArea;

	private const string buildTooltip = "Shows the areas that can be built on, and where characters can go. Structures cannot be built on the red and yellow areas.";

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public OverlaySettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			LoadDefaultSettings();
		}
	}

	private void LoadDefaultSettings()
	{
		foreach (EntityGrouping value7 in Enum.GetValues(typeof(EntityGrouping)))
		{
			bool value = false;
			GameData.Instance.GUIConstants.OverlayDefaultDisplaySettings.TryGetValue(value7, out value);
			EntityTypeGroupingsToDisplay.Add(value7, value);
		}
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			bool value2 = false;
			if (GameData.Instance.GUIConstants.OverlayDefaultEntityTypeDisplaySettings.TryGetValue(allEntityType.Key, out value2))
			{
				EntityTypesToDisplay[allEntityType.Value] = value2;
				continue;
			}
			bool value3 = false;
			EntityGrouping? grouping = GetGrouping(allEntityType.Value);
			if (grouping.HasValue)
			{
				value3 = EntityTypeGroupingsToDisplay[grouping.Value];
			}
			EntityTypesToDisplay[allEntityType.Value] = value3;
		}
		foreach (KeyValuePair<string, ResourceCategory> allResourceCategory in GameData.Instance.AllResourceCategories)
		{
			bool value4 = false;
			GameData.Instance.GUIConstants.OverlayDefaultResourceCategoryDisplaySettings.TryGetValue(allResourceCategory.Key, out value4);
			ResourceCategoriesToDisplay.Add(allResourceCategory.Value, value4);
		}
		foreach (KeyValuePair<string, ResourceType> allResourceType in GameData.Instance.AllResourceTypes)
		{
			bool value5 = false;
			if (GameData.Instance.GUIConstants.OverlayDefaultResourceTypeDisplaySettings.TryGetValue(allResourceType.Key, out value5))
			{
				ResourceTypesToDisplay[allResourceType.Value] = value5;
				continue;
			}
			bool value6 = ResourceCategoriesToDisplay[allResourceType.Value.Category];
			ResourceTypesToDisplay[allResourceType.Value] = value6;
		}
		foreach (KeyValuePair<OverlayTypes, bool> overlayDefaultTypeDisplaySetting in GameData.Instance.GUIConstants.OverlayDefaultTypeDisplaySettings)
		{
			OverlayTypeSettings[overlayDefaultTypeDisplaySetting.Key] = overlayDefaultTypeDisplaySetting.Value;
		}
		InitEditorOverlaySettings();
		ShowOverlaysOnGameArea = GameData.Instance.GUIConstants.ShowOverlaysOnGameAreaDefault;
	}

	private void InitEditorOverlaySettings()
	{
		foreach (KeyValuePair<EditorOverlayTypes, bool> overlayDefaultTypeEditorDisplaySetting in GameData.Instance.GUIConstants.OverlayDefaultTypeEditorDisplaySettings)
		{
			EditorOverlayTypeSettings[overlayDefaultTypeEditorDisplaySetting.Key] = overlayDefaultTypeEditorDisplaySetting.Value;
		}
	}

	public bool EntityCategoryHasDifferentItemSetting(bool categorySetting, EntityGrouping grouping)
	{
		RecomputeEntityCategoryHasItemSettings(grouping);
		if (entityCategoryItemSettings.TryGetValue(grouping, out var value))
		{
			return value.Exists((EntityType e) => ItemSettingDiffers(EntityTypesToDisplay, e, categorySetting));
		}
		return false;
	}

	public static bool ItemSettingDiffers(Dictionary<EntityType, bool> mayStockpileItem, EntityType entityType, bool setting)
	{
		if (mayStockpileItem.TryGetValue(entityType, out var value))
		{
			return value != setting;
		}
		return false;
	}

	private void RecomputeEntityCategoryHasItemSettings(EntityGrouping grouping)
	{
		entityCategoryItemSettings.Clear();
		foreach (KeyValuePair<EntityType, bool> item in EntityTypesToDisplay)
		{
			if (grouping == GetGrouping(item.Key))
			{
				Common.AddToMultiList(entityCategoryItemSettings, GetGrouping(item.Key).Value, item.Key);
			}
		}
	}

	public bool DisplayEntityType(EntityType entityType)
	{
		if (EntityTypesToDisplay.TryGetValue(entityType, out var value))
		{
			return value;
		}
		EntityGrouping? grouping = GetGrouping(entityType);
		if (grouping.HasValue && EntityTypeGroupingsToDisplay.TryGetValue(grouping.Value, out var value2))
		{
			return value2;
		}
		return false;
	}

	public bool DisplayAnyResources()
	{
		if (!ResourceTypesToDisplay.Any((KeyValuePair<ResourceType, bool> r) => r.Value))
		{
			return ResourceCategoriesToDisplay.Any((KeyValuePair<ResourceCategory, bool> c) => c.Value);
		}
		return true;
	}

	public bool DisplayAnyEntities()
	{
		if (!EntityTypesToDisplay.Any((KeyValuePair<EntityType, bool> r) => r.Value))
		{
			return EntityTypeGroupingsToDisplay.Any((KeyValuePair<EntityGrouping, bool> c) => c.Value);
		}
		return true;
	}

	public bool ResourceCategoryHasDifferentItemSetting(bool categorySetting, ResourceCategory category)
	{
		RecomputeResourceCategoryHasItemSettings(category);
		if (resourceCategoryItemSettings.TryGetValue(category, out var value))
		{
			return value.Exists((ResourceType e) => ResourceItemSettingDiffers(ResourceTypesToDisplay, e, categorySetting));
		}
		return false;
	}

	private bool ResourceItemSettingDiffers(Dictionary<ResourceType, bool> mayStockpileItem, ResourceType resourceType, bool setting)
	{
		if (mayStockpileItem.TryGetValue(resourceType, out var value))
		{
			return value != setting;
		}
		return false;
	}

	private void RecomputeResourceCategoryHasItemSettings(ResourceCategory resourceCategory)
	{
		resourceCategoryItemSettings.Clear();
		foreach (KeyValuePair<ResourceType, bool> item in ResourceTypesToDisplay)
		{
			if (item.Key.Category == resourceCategory)
			{
				Common.AddToMultiList(resourceCategoryItemSettings, item.Key.Category, item.Key);
			}
		}
	}

	public bool DisplayResourceType(ResourceType resourceType)
	{
		if (ResourceTypesToDisplay.TryGetValue(resourceType, out var value))
		{
			return value;
		}
		if (ResourceCategoriesToDisplay.TryGetValue(resourceType.Category, out var value2))
		{
			return value2;
		}
		return false;
	}

	public bool DrawResource(ResourceContainer container, bool isInGodMode)
	{
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		if (DisplayResourceType(container.ResourceType) && (isInGodMode || sharedKnowledge.AllDetectedEntities.Contains(((ILookUp<IDetectable, DetectableID>)container).ID)) && container.NoOfHarvestableItems > 0)
		{
			return true;
		}
		return false;
	}

	public static string GetName(EntityGrouping grouping)
	{
		return grouping switch
		{
			EntityGrouping.Interest => "INTEREST", 
			EntityGrouping.ColonyMembers => "COLONY MEMBERS", 
			EntityGrouping.Animals => "ANIMALS", 
			EntityGrouping.Structures => "STRUCTURES", 
			_ => null, 
		};
	}

	public static string GetIconFromOverlayType(EditorOverlayTypes type)
	{
		return type switch
		{
			EditorOverlayTypes.BuildAreas => "HUD_icon_structure", 
			EditorOverlayTypes.EntityIDs => null, 
			EditorOverlayTypes.Coords => null, 
			EditorOverlayTypes.TerrainDivision => null, 
			_ => null, 
		};
	}

	public static string GetTextFromOverlayType(EditorOverlayTypes type)
	{
		return type switch
		{
			EditorOverlayTypes.BuildAreas => "BUILDABLE AREA", 
			EditorOverlayTypes.Coords => "COORDINATES", 
			EditorOverlayTypes.EntityIDs => "IDs", 
			EditorOverlayTypes.TerrainDivision => "TERRAIN DIVISION", 
			_ => null, 
		};
	}

	public static string GetTooltipFromOverlayType(EditorOverlayTypes type)
	{
		return type switch
		{
			EditorOverlayTypes.BuildAreas => "Shows the areas that can be built on, and where characters can go. Structures cannot be built on the red and yellow areas.", 
			EditorOverlayTypes.Coords => "Shows tile coordinates and world coordinates on each tile", 
			EditorOverlayTypes.EntityIDs => "Shows entity IDs", 
			EditorOverlayTypes.TerrainDivision => "Shows terrain division", 
			_ => null, 
		};
	}

	public static Color? GetColorFromOverlayType(EditorOverlayTypes type)
	{
		return type switch
		{
			EditorOverlayTypes.BuildAreas => buildableColor, 
			EditorOverlayTypes.EntityIDs => threatColor, 
			EditorOverlayTypes.Coords => personsColor, 
			EditorOverlayTypes.TerrainDivision => animalsColor, 
			_ => null, 
		};
	}

	public static string GetIconFromOverlayType(OverlayTypes type)
	{
		return type switch
		{
			OverlayTypes.BuildAreas => "HUD_icon_structure", 
			OverlayTypes.Threats => "HUD_icon_status_skull", 
			OverlayTypes.ColonyMembers => "HUD_icon_person", 
			_ => null, 
		};
	}

	public static string GetTextFromOverlayType(OverlayTypes type)
	{
		return type switch
		{
			OverlayTypes.BuildAreas => "BUILDABLE AREA", 
			OverlayTypes.Threats => "THREATS", 
			OverlayTypes.ColonyMembers => "COLONY MEMBERS", 
			_ => null, 
		};
	}

	public static string GetTooltipFromOverlayType(OverlayTypes type)
	{
		return type switch
		{
			OverlayTypes.BuildAreas => "Shows the areas that can be built on, and where characters can go. Structures cannot be built on the red and yellow areas.", 
			OverlayTypes.Threats => "Shows the areas considered dangerous by the colonists because threats have been spotted there. Threats can sometimes be removed with the ATTACK action.", 
			OverlayTypes.ColonyMembers => "Shows where the colony members are on the minimap", 
			_ => null, 
		};
	}

	public static Color? GetColorFromOverlayType(OverlayTypes type)
	{
		return type switch
		{
			OverlayTypes.BuildAreas => buildableColor, 
			OverlayTypes.Threats => threatColor, 
			OverlayTypes.ColonyMembers => personsColor, 
			_ => null, 
		};
	}

	public static EntityGrouping? GetGrouping(IKnownEntityData entityData)
	{
		if (entityData is Entity item && The.InGameUI.UIAllegiance.Members.Contains(item))
		{
			return EntityGrouping.ColonyMembers;
		}
		return GetGrouping(entityData.EntityType);
	}

	public static EntityGrouping? GetGrouping(EntityType entityType)
	{
		if (entityType.StructureType != null)
		{
			return EntityGrouping.Structures;
		}
		if (entityType.BiologicalType != null && entityType.Person == null)
		{
			return EntityGrouping.Animals;
		}
		if (entityType.TerrainType != null && entityType.TerrainType.IsSpecialInterestFeature)
		{
			return EntityGrouping.Interest;
		}
		return null;
	}

	public static Color? GetGroupingColorFromEntity(IKnownEntityData entityData, bool faded)
	{
		switch (GetGrouping(entityData))
		{
		case EntityGrouping.Animals:
			if (faded)
			{
				return animalsFaded;
			}
			return animalsColor;
		case EntityGrouping.Structures:
			if (faded)
			{
				return structuresFaded;
			}
			return structuresColor;
		case EntityGrouping.ColonyMembers:
			return personsColor;
		case EntityGrouping.Interest:
			if (faded)
			{
				return interestFaded;
			}
			return interestColor;
		default:
			return null;
		}
	}

	public static Color? GetGroupingColorFromEntityType(EntityType entityType)
	{
		return GetGrouping(entityType) switch
		{
			EntityGrouping.Animals => animalsColor, 
			EntityGrouping.Structures => structuresColor, 
			EntityGrouping.Interest => interestColor, 
			_ => null, 
		};
	}

	public static Color? GetGroupingColor(EntityGrouping entityGrouping, bool faded)
	{
		switch (entityGrouping)
		{
		case EntityGrouping.Animals:
			if (faded)
			{
				return animalsFaded;
			}
			return animalsColor;
		case EntityGrouping.Structures:
			if (faded)
			{
				return structuresFaded;
			}
			return structuresColor;
		case EntityGrouping.ColonyMembers:
			return personsColor;
		case EntityGrouping.Interest:
			if (faded)
			{
				return interestFaded;
			}
			return interestColor;
		default:
			return null;
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EntityTypesToDisplay = sn.DoDictionary(EntityTypesToDisplay);
		ResourceTypesToDisplay = sn.DoDictionary(ResourceTypesToDisplay);
		OverlayTypeSettings = sn.DoDictionary(OverlayTypeSettings);
		ResourceCategoriesToDisplay = sn.DoDictionary(ResourceCategoriesToDisplay);
		EntityTypeGroupingsToDisplay = sn.DoDictionary(EntityTypeGroupingsToDisplay);
		entityCategoryItemSettings = sn.DoMultiMap(entityCategoryItemSettings);
		resourceCategoryItemSettings = sn.DoMultiMap(resourceCategoryItemSettings);
		ShowOverlaysOnGameArea = sn.DoBool(ShowOverlaysOnGameArea);
		sn.Ignore(interestColor);
		sn.Ignore(interestFaded);
		sn.Ignore(animalsColor);
		sn.Ignore(animalsFaded);
		sn.Ignore(structuresColor);
		sn.Ignore(structuresFaded);
		sn.Ignore(personsColor);
		sn.Ignore(threatColor);
		sn.Ignore(buildableColor);
		sn.Ignore(EditorOverlayTypeSettings);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		InitEditorOverlaySettings();
	}
}
