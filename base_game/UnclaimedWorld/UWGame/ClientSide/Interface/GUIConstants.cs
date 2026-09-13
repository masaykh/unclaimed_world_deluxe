using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.XmlCollections;

namespace UWGame.ClientSide.Interface;

public class GUIConstants : IGameDataObject, IXmlSerializable
{
	public bool EnableFilters = true;

	public bool EnableStandingOrders = true;

	public Color StandingOrderTint = "#FFF58E".ColorFromHex();

	public string[] TrackingColorsHex = new string[5] { "#76ca70", "#50bbff", "#ed833a", "#13cead", "#7691d5" };

	[XmlIgnore]
	public List<Color> TrackingColors;

	public Color AttainableColor = "#4AA863".ColorFromHex();

	public Color UnattainableColor = "#c6000e".ColorFromHex();

	private const string positiveHex = "#048628";

	private const string negativeHex = "#953540";

	public string PositiveTintHex = "#048628";

	public string NegativeTintHex = "#953540";

	[XmlIgnore]
	public Color NegativeColor = "#953540".ColorFromHex();

	[XmlIgnore]
	public Color PositiveColor = "#048628".ColorFromHex();

	public string ValueTintHex = "#0181C6";

	public string FoodColor = "#59C24E";

	public string SecurityColor = "#559DBA";

	public string ComfortColor = "#C76098";

	public string FoodColorConstant = "#COLORFOOD";

	public string SecurityColorConstant = "#COLORSECURITY";

	public string ComfortColorConstant = "#COLORCOMFORT";

	public Color SellingButtonTint = "#75D668".ColorFromHex();

	public Color SellingTint = "#75D668".ColorFromHex();

	public Color BuyingButtonTint = "#ffba77".ColorFromHex();

	public Color BuyingTint = "#D2984D".ColorFromHex();

	public SerializableDictionary<string, string> CustomColors = new SerializableDictionary<string, string>
	{
		{ "#COLORPOSITIVE", "#048628" },
		{ "#COLORNEGATIVE", "#953540" },
		{ "#COLORDATE", "#914B6F" },
		{ "#COLORMEMBER", "#A08500" },
		{ "#COLORHEADER", "#FFA500" },
		{ "#COLORHEADERDARK", "#A55D00" },
		{ "#COLORACTION", "#0073A0" },
		{ "#COLORTYPE", "#CCCC00" },
		{ "#COLORFOOD", "#59C24E" },
		{ "#COLORSECURITY", "#559DBA" },
		{ "#COLORCOMFORT", "#C76098" }
	};

	public byte OverlayLowAlpha = 100;

	public byte OverlayHiAlpha = 150;

	public string PadColorHex = "FFFF00";

	public string BlockedColorHex = "BA0B3C";

	public string StructureBeingPlacedColorHex = "0000FF";

	public string StructurePreventingPlacementColorHex = "BA0B3C";

	public string StructureBeingPlacedOtherPointPreventingPlacementColorHex = "FF6100";

	public string StructureBeingPlacedOtherPointPreventingPlacementPadColorHex = "FFC300";

	public string InPadAndReservedColorHex = "FFFFE0";

	[XmlIgnore]
	public Color PadColor;

	[XmlIgnore]
	public Color BlockedColor;

	[XmlIgnore]
	public Color StructureBeingPlacedColor;

	[XmlIgnore]
	public Color StructureBeingPlacedOtherPointPreventingPlacementColor;

	[XmlIgnore]
	public Color StructureBeingPlacedOtherPointPreventingPlacementPadColor;

	[XmlIgnore]
	public Color StructurePreventingPlacementColor;

	[XmlIgnore]
	public Color InPadAndReservedColor;

	[XmlIgnore]
	public Color PositiveTint;

	[XmlIgnore]
	public Color NegativeTint;

	public string FirstToolOption = "PRIMARY TOOL OPTIONS:";

	public string SecondToolOption = "SECONDARY TOOL OPTIONS:";

	public string ThirdToolOption = "THIRD TOOL OPTIONS:";

	public int NoOfToolsToDisplayWhenCollapsed = 3;

	public int NoOfToolsToDisplayWhenExpanded = 15;

	public Color sidePanelTextColor = "#3a7177".ColorFromHex();

	public float BetterToolsFilterLimit = 0.71f;

	public double DefaultTimeInGameSecondsBeforeGroupMeeting = 400.0;

	public double MinimumTimeInInGameSecondsBetweenGroupMeetings = 3000.0;

	public float TimeInDaysToKeepStatistics = 100f;

	public float SliderButtonDelay = 0.4f;

	public float TimeBetweenSliderButtonIncrements = 0.1f;

	public int OrderedJobsWithSameOutputToTriggerWarning = 5;

	public double TimeBetweenReplenishAlerts = 10.0;

	public int UnlimitedStockpileValue = 99;

	public int MaxStandingOrder = 99;

	public int UnlimitedStandingOrderValue = 99;

	public int MaxLogEventsToKeep = 200;

	public int MaxToolsToShowInUsedForList = 15;

	public string[] ProductionFilterSettings = new string[28]
	{
		"containers", "storage", "fuel", "betterTools", "structures", "items", "usableAsWeapon", "highProteinForHumans", "highCaloriesForHumans", "highMicronientsForHumans",
		"highStimulantsForHumans", "ammunition", "waste", "weapons", "tools", "rawMaterials", "ingredients", "preparedFood", "affectsFoodRating", "affectsComfortRating",
		"affectsSecurityRating", "survivalTier", "basicTier", "mediumTier", "advancedTier", "comfortPolicy", "foodPolicy", "securityPolicy"
	};

	public SerializableDictionary<OverlayTypes, bool> OverlayDefaultTypeDisplaySettings = new SerializableDictionary<OverlayTypes, bool>
	{
		{
			OverlayTypes.ColonyMembers,
			false
		},
		{
			OverlayTypes.BuildAreas,
			false
		},
		{
			OverlayTypes.Threats,
			false
		}
	};

	public SerializableDictionary<EntityGrouping, bool> OverlayDefaultDisplaySettings = new SerializableDictionary<EntityGrouping, bool>
	{
		{
			EntityGrouping.Animals,
			false
		},
		{
			EntityGrouping.ColonyMembers,
			false
		},
		{
			EntityGrouping.Interest,
			false
		},
		{
			EntityGrouping.Structures,
			false
		}
	};

	public SerializableDictionary<EditorOverlayTypes, bool> OverlayDefaultTypeEditorDisplaySettings = new SerializableDictionary<EditorOverlayTypes, bool>
	{
		{
			EditorOverlayTypes.Coords,
			false
		},
		{
			EditorOverlayTypes.BuildAreas,
			false
		},
		{
			EditorOverlayTypes.EntityIDs,
			false
		},
		{
			EditorOverlayTypes.TerrainDivision,
			false
		}
	};

	public SerializableDictionary<string, bool> OverlayDefaultEntityTypeDisplaySettings = new SerializableDictionary<string, bool>();

	public SerializableDictionary<string, bool> OverlayDefaultResourceCategoryDisplaySettings = new SerializableDictionary<string, bool> { { "food", true } };

	public SerializableDictionary<string, bool> OverlayDefaultResourceTypeDisplaySettings = new SerializableDictionary<string, bool>();

	[XmlIgnore]
	public HashSet<EntityCategory> StockpileExcludesCategoriesFinal;

	public string[] StockpileExcludesCategories = new string[1] { "upgrades" };

	public StorageDurationToDisplay[] StorageDurationToDisplay = new StorageDurationToDisplay[6]
	{
		new StorageDurationToDisplay
		{
			DisplayName = "Outside",
			StorageCondition = "exposed",
			IsStorageOfWeatherProofPart = false,
			DisplayForStructure = true,
			DisplayAlways = true
		},
		new StorageDurationToDisplay
		{
			DisplayName = "Outside (protected part)",
			StorageCondition = "exposed",
			Tooltip = "Used as a part in a structure that provides some protection against the environment",
			SortAtTop = true,
			IsStorageOfWeatherProofPart = true,
			DisplayForStructure = false
		},
		new StorageDurationToDisplay
		{
			DisplayName = "Inside",
			StorageCondition = "isolated",
			SortAtTop = true,
			IsStorageOfWeatherProofPart = false,
			DisplayForStructure = false
		},
		new StorageDurationToDisplay
		{
			DisplayName = "Earth cooled",
			StorageCondition = "earthCooled",
			IsStorageOfWeatherProofPart = false,
			DisplayForStructure = false,
			DisplayForFoodOnly = true
		},
		new StorageDurationToDisplay
		{
			DisplayName = "Refrigerated",
			StorageCondition = "refrigerator",
			IsStorageOfWeatherProofPart = false,
			DisplayForStructure = false,
			DisplayForFoodOnly = true
		},
		new StorageDurationToDisplay
		{
			DisplayName = "Freezer",
			StorageCondition = "freezer",
			IsStorageOfWeatherProofPart = false,
			DisplayForStructure = false,
			DisplayForFoodOnly = true
		}
	};

	public bool ShowOverlaysOnGameAreaDefault = true;

	public string[] FoodProductionCategories = new string[2] { "ingredients", "preparedFood" };

	public string[] ProductionCategories = new string[11]
	{
		"rawMaterials", "tools", "weapons", "equipment", "ammunition", "shelter", "production", "defense", "miscellaneous", "ingredients",
		"preparedFood"
	};

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(GUIConstants))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<Color, string>
			{
				GetterMethod = (Color t) => t.ToHex(includeHash: true),
				SetterMethod = (string s) => s.ColorFromHex()
			}
		}
	};

	public void Initialize()
	{
		PositiveTint = PositiveTintHex.ColorFromHex();
		NegativeTint = NegativeTintHex.ColorFromHex();
		PadColor = PadColorHex.ColorFromHex();
		BlockedColor = BlockedColorHex.ColorFromHex();
		StructureBeingPlacedColor = StructureBeingPlacedColorHex.ColorFromHex();
		StructurePreventingPlacementColor = StructurePreventingPlacementColorHex.ColorFromHex();
		StructureBeingPlacedOtherPointPreventingPlacementColor = StructureBeingPlacedOtherPointPreventingPlacementColorHex.ColorFromHex();
		StructureBeingPlacedOtherPointPreventingPlacementPadColor = StructureBeingPlacedOtherPointPreventingPlacementPadColorHex.ColorFromHex();
		InPadAndReservedColor = InPadAndReservedColorHex.ColorFromHex();
		if (TrackingColorsHex != null)
		{
			TrackingColors = new List<Color>();
			string[] trackingColorsHex = TrackingColorsHex;
			foreach (string hexString in trackingColorsHex)
			{
				TrackingColors.Add(hexString.ColorFromHex());
			}
		}
	}

	public void PostDataCompleteInitialize()
	{
		StockpileExcludesCategoriesFinal = new HashSet<EntityCategory>();
		if (StockpileExcludesCategories != null)
		{
			string[] stockpileExcludesCategories = StockpileExcludesCategories;
			foreach (string key in stockpileExcludesCategories)
			{
				StockpileExcludesCategoriesFinal.Add(GameData.Instance.AllEntityCategories[key]);
			}
		}
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
