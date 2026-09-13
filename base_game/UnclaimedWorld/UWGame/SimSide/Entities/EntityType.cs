using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Trees;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities;

public class EntityType : IXmlSerializable, IGameData, IHasCategory<EntityCategory>, IDetectableType
{
	public enum ShowMarkerWindowMode
	{
		OnlyWhenSelected,
		Always,
		ByStatus
	}

	public bool? IsNeverInFogOfWar;

	public bool? UsesMemory;

	public bool? RequiresRollToDetect;

	public string FormalName;

	public string ThumbnailBig;

	public bool UseTypeNameForDisplay = true;

	public string ThumbnailSmall;

	public string[] Tags;

	public string Icon;

	public string WorldMapIcon;

	public string Description = "";

	public string SummaryDescription;

	public SerializableDictionary<string, PropertyResult> CustomFields;

	public PersonType Person;

	public BiologicalType BiologicalType;

	public IntelligenceType IntelligenceType;

	public TerrainFeatureType TerrainType;

	public HeatingType HeatingType;

	public TreeType TreeType;

	public BodyType BodyType;

	public RockType RockType;

	public ItemType ItemType;

	public ThreatType ThreatType;

	public GatheringSiteType GatheringSiteType;

	public ContainerType ContainerType;

	public NonLivingType NonLivingType;

	public SubstancesType SubstancesType;

	public CommunicatorType CommunicatorType;

	public ToolType ToolType;

	public string DetectionTag;

	public string[] CanEatDesignerTags;

	public Upgrader Upgrader;

	public TriggerType[] Triggers;

	public TierOrArea TierOrArea;

	[XmlIgnore]
	public TierOrAreaType TierOrAreaType;

	[XmlIgnore]
	public Dictionary<Scope, List<PolledEventType>> PolledEvents = new Dictionary<Scope, List<PolledEventType>>();

	[XmlIgnore]
	public Dictionary<EntityEventHooks, List<ActionSets>> EventActions = new Dictionary<EntityEventHooks, List<ActionSets>>();

	public Pair<string, bool>[] SharedSpecialActions;

	public Pair<string, bool>[] SpecialActionLocks;

	public bool? IsSelectable;

	public bool IsFlyer;

	public Vector3? AccessPointDirection;

	public RenderableType RenderableType;

	public RenderableType EditorRenderableType;

	public StructureType StructureType;

	public SensorType SensorType;

	public DirectionalLayoutType DirectionalLayoutType;

	public TerminalType TerminalType;

	public PointLayoutType PointLayoutType;

	public CollidableType CollidableType;

	public SimStateInfo[] SimStateConditions;

	public SimStateInfo DefaultSimState;

	public ShowMarkerWindowMode? ShowMarkerWindowSetting;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(EntityType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public string Name { get; set; }

	public string PluralName { get; set; }

	public string CategoryKey { get; set; }

	[XmlIgnore]
	public EntityCategory Category { get; set; }

	[XmlIgnore]
	public List<ProcessType> SharedSpecialActionTypes { get; private set; }

	[XmlIgnore]
	public List<ProcessType> SpecialActionLockTypes { get; private set; }

	public LocomotorType LocomotorType { get; set; }

	public Dictionary<EntityType, int> Parts
	{
		get
		{
			if (NonLivingType != null)
			{
				return NonLivingType.Parts;
			}
			return null;
		}
	}

	public RenderableType RenderableTypeMode { get; private set; }

	public EntityType()
	{
	}

	public EntityType(string keyName)
	{
		KeyName = keyName;
	}

	public EntityType ShallowCopy()
	{
		return (EntityType)MemberwiseClone();
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (ToolType != null)
		{
			ToolType.PreInitValidate(ref listOfErrors);
			if (StructureType != null && !ToolType.IsPseudoTool && ToolType.ToolHandling != ToolHandlingType.Stationary)
			{
				CreateValidationError(ref listOfErrors, "Structures can only be stationary tools.");
			}
			if (Upgrader != null && !ToolType.IsPseudoTool && ToolType.ToolHandling != ToolHandlingType.Stationary)
			{
				CreateValidationError(ref listOfErrors, "Upgrades can only be stationary tools.");
			}
		}
		if (CommunicatorType != null && SensorType == null)
		{
			CreateValidationError(ref listOfErrors, "Communicators require a SensorType to be defined also. They must not be in FOW.");
		}
		if (ContainerType != null && ContainerType is TerminalContainerType { OfferedForTradeStorageType: not null } && SensorType == null)
		{
			CreateValidationError(ref listOfErrors, "Buy/Sell Terminals require a SensorType to be defined also. They must not be in FOW.");
		}
		if (SensorType != null && IntelligenceType == null)
		{
			CreateValidationError(ref listOfErrors, "SensorType without IntelligenceType is not supported.");
		}
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
		if (RenderableType != null)
		{
			RenderableType.PostInitValidate(ref listOfErrors);
		}
		if (EditorRenderableType != null)
		{
			EditorRenderableType.PostInitValidate(ref listOfErrors);
		}
		if (((DirectionalLayoutType != null) ? 1 : 0) + ((PointLayoutType != null) ? 1 : 0) > 1)
		{
			CreateValidationError(ref listOfErrors, "Only one of the EdgeLayout and PointLayout may be specified.");
		}
		if (BiologicalType != null)
		{
			BiologicalType.Validate(ref listOfErrors);
		}
		if (ItemType != null)
		{
			ItemType.Validate(ref listOfErrors);
		}
		if (SensorType != null)
		{
			SensorType.PostInitValidate(ref listOfErrors);
		}
		if (ContainerType != null)
		{
			ContainerType.PostInitValidate(this, ref listOfErrors);
		}
		ValidateRequiredValue(ref listOfErrors, "Name", Name != null);
	}

	public void PostLoadContentValidate(ref List<string> listOfErrors)
	{
		if (!string.IsNullOrEmpty(ThumbnailSmall) && The.InGameUI != null && !The.InGameUI.gui.GUISpriteSheet.TryGetSourceRectangle(ThumbnailSmall, out var _))
		{
			CreateValidationError(ref listOfErrors, "ThumbnailSmall asset " + ThumbnailSmall + " not found.");
		}
		if (RenderableType != null)
		{
			RenderableType.PostLoadContentValidate(ref listOfErrors, this);
		}
		if (EditorRenderableType != null)
		{
			EditorRenderableType.PostLoadContentValidate(ref listOfErrors, this);
		}
	}

	public void PostDataCompleteInitialize()
	{
		DataLoader.AddToTagCollection(this, Tags, GameData.Instance.GeneralTags);
		if (ToolType != null)
		{
			DataLoader.AddToTagCollection(this, ToolType.ToolTag, GameData.Instance.ToolsByTag);
		}
		if (ItemType != null)
		{
			ItemType.PostDataCompleteInitialize();
			if (ItemType.FuelType != null)
			{
				DataLoader.AddToTagCollection(this, ItemType.FuelType.FuelTags, GameData.Instance.FuelByTag);
			}
			if (ItemType.AmmunitionType != null)
			{
				DataLoader.AddToTagCollection(this, ItemType.AmmunitionType.AmmoTags, GameData.Instance.AmmoByTag);
			}
			if (ItemType.FoodType != null)
			{
				DataLoader.AddToTagCollection(this, ItemType.FoodType.FoodTags, GameData.Instance.FoodByTag);
			}
		}
		if (NonLivingType != null)
		{
			NonLivingType.PostDataCompleteInitialize(this);
		}
		if (Upgrader != null)
		{
			Upgrader.PostDataCompleteInitialize(this);
		}
		if (ContainerType != null)
		{
			DataLoader.AddToTagCollection(this, ContainerType.StorageTags, GameData.Instance.ContainersByTag);
			if (ContainerType.CanTransactWithTags != null)
			{
				string[] canTransactWithTags = ContainerType.CanTransactWithTags;
				foreach (string key in canTransactWithTags)
				{
					if (!GameData.Instance.ContainerTags.ContainsKey(key))
					{
						GameData.Instance.ContainerTags.Add(key, GameData.Instance.ContainerTags.Count);
					}
				}
			}
			if (ContainerType.CanBeEnteredByTags != null)
			{
				string[] canTransactWithTags = ContainerType.CanBeEnteredByTags;
				foreach (string key2 in canTransactWithTags)
				{
					if (!GameData.Instance.ContainerTags.ContainsKey(key2))
					{
						GameData.Instance.ContainerTags.Add(key2, GameData.Instance.ContainerTags.Count);
					}
				}
			}
			ContainerType.PostDataCompleteInitialize(this);
		}
		if (TierOrArea != null)
		{
			TierOrAreaType = new TierOrAreaType(TierOrArea);
		}
		if (BiologicalType != null)
		{
			BiologicalType.PostDataCompleteInitialize();
		}
		InitSpecialActions(SharedSpecialActions);
		InitSpecialActions(SpecialActionLocks);
		if (StructureType != null)
		{
			GameData.Instance.AllStructureTypes.Add(KeyName, this);
		}
		if (TerrainType != null)
		{
			GameData.Instance.AllTerrainFeatureTypes.Add(KeyName, this);
		}
		if (TreeType != null)
		{
			GameData.Instance.AllTreeTypes.Add(KeyName, this);
		}
		if (ItemType != null)
		{
			GameData.Instance.AllItemTypes.Add(KeyName, this);
		}
		if (BiologicalType != null)
		{
			GameData.Instance.AllCreatureTypes.Add(KeyName, this);
			if (BiologicalType.IsVermin)
			{
				GameData.Instance.AllVerminTypes.Add(KeyName, this);
			}
		}
		if (CategoryKey != null)
		{
			Category = GameData.Instance.AllEntityCategories[CategoryKey];
		}
	}

	private static void InitSpecialActions(Pair<string, bool>[] specialActions)
	{
		if (specialActions == null)
		{
			return;
		}
		foreach (Pair<string, bool> pair in specialActions)
		{
			if (GameData.Instance.AllProcessTypes.TryGetValue(pair.First, out var value))
			{
				value.IsOriginalSpecialAction = true;
			}
		}
	}

	public bool CanBeHuntedBy(Allegiance allegiance)
	{
		if ((this != allegiance.RepresentativeEntityType && allegiance.RepresentativeEntityType.IntelligenceType.HasServants == null) || (!allegiance.RepresentativeEntityType.IntelligenceType.HasServants.Contains(this) && BiologicalType != null && IntelligenceType != null))
		{
			return true;
		}
		return false;
	}

	public void MarkAnchorStructures()
	{
		if (StructureType != null)
		{
			StructureType.MarkAnchorStructures(this);
		}
	}

	public bool IsInOriginalBlueprint(EntityType entityTypeToCheck)
	{
		foreach (KeyValuePair<EntityType, int> part in Parts)
		{
			if (entityTypeToCheck == part.Key)
			{
				return true;
			}
		}
		return false;
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (NonLivingType != null)
		{
			NonLivingType.PreDataCompleteValidate(ref listOfErrors);
		}
		if (Upgrader != null)
		{
			Upgrader.PreDataCompleteValidate(ref listOfErrors);
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public ShowMarkerWindowMode GetShowMarkerWindowMode()
	{
		if (ShowMarkerWindowSetting.HasValue)
		{
			return ShowMarkerWindowSetting.Value;
		}
		if (ItemType != null || StructureType != null || (BiologicalType != null && BiologicalType.IsTerritorial) || (IntelligenceType != null && IntelligenceType.IsPredator))
		{
			return ShowMarkerWindowMode.ByStatus;
		}
		return ShowMarkerWindowMode.OnlyWhenSelected;
	}

	public bool IsEatable(Dictionary<EntityType, HashSet<ProcessType>> extractionResultsInConsumable, Dictionary<EntityType, ProcessType> consumeProcesses)
	{
		if (!consumeProcesses.ContainsKey(this))
		{
			return extractionResultsInConsumable.ContainsKey(this);
		}
		return true;
	}

	public bool CanBeSalvagedDirectly()
	{
		if (NonLivingType != null && NonLivingType.SalvageProcessType != null && Upgrader == null)
		{
			return true;
		}
		return false;
	}

	public bool CanBeUpgradedBy(EntityType upgrader)
	{
		if (ContainerType != null)
		{
			List<UpgradeCategory> upgradeOptions = ContainerType.GetUpgradeOptions();
			foreach (UpgradeCategory item in upgrader.Upgrader.UpgradeCategoryFinal)
			{
				if (upgradeOptions.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBeMounted(Entity agent)
	{
		if (ItemType != null)
		{
			if (ItemType.WeaponType != null && agent.EntityType.IntelligenceType.CanUseWeapons == true)
			{
				return true;
			}
			if (ToolType != null)
			{
				ToolHandlingType? toolHandling = ToolType.ToolHandling;
				ToolHandlingType toolHandlingType = ToolHandlingType.HandTool;
				if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && agent.EntityType.IntelligenceType.CanMountTools == true)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool MemoryFactIsDeprecatedInstantly()
	{
		if (StructureType != null)
		{
			return true;
		}
		return false;
	}

	public bool RenderWithOverlayWhenMemoryFact()
	{
		if (StructureType == null)
		{
			return true;
		}
		return false;
	}

	public static void CreateValidationError(ref List<string> listOfErrors, string errorMessage)
	{
		Common.AddToList(ref listOfErrors, errorMessage);
	}

	public static bool ValidateGameDataTypeExists<T>(ref List<string> listOfErrors, string key, GameDataCollection<T> collection) where T : IGameData
	{
		T dataType;
		return ValidateGameDataTypeExists(ref listOfErrors, key, collection, out dataType);
	}

	public static bool ValidateGameDataTypeExists<T>(ref List<string> listOfErrors, string key, GameDataCollection<T> collection, out T dataType) where T : IGameData
	{
		if (!collection.TryGetValue(key, out dataType))
		{
			CreateValidationError(ref listOfErrors, typeof(T).Name + " key value '" + key + "' not found.");
			return false;
		}
		return true;
	}

	public static void ValidateEntityTypeKeyExists(ref List<string> listOfErrors, string key)
	{
		if (!GameData.Instance.AllEntityTypes.ContainsKey(key))
		{
			CreateValidationError(ref listOfErrors, "Entity type: " + key + " not found.");
		}
	}

	public static void ValidateRequiredValue(ref List<string> listOfErrors, string fieldName, bool hasValue)
	{
		if (!hasValue)
		{
			Common.AddToList(ref listOfErrors, fieldName + " is a required value.");
		}
	}

	public void Initialize()
	{
		if (!string.IsNullOrEmpty(DetectionTag))
		{
			_ = DetectionTag == "inDeeperWaterFishingSpot";
			DataLoader.AddToTagCollection(this, DetectionTag, GameData.Instance.DetectableTypeByTag);
		}
		if (Person != null)
		{
			Person.Initialize();
		}
		if (BiologicalType != null)
		{
			BiologicalType.Initialize(this);
		}
		if (RenderableType != null)
		{
			RenderableType.Initialize();
		}
		if (EditorRenderableType != null)
		{
			EditorRenderableType.Initialize();
		}
		if (StructureType != null)
		{
			StructureType.Initialize(this);
		}
		if (ContainerType != null)
		{
			ContainerType.Initialize();
		}
		if (TreeType != null)
		{
			TreeType.Initialize();
		}
		if (SubstancesType != null)
		{
			SubstancesType.Initialize();
		}
		if (ItemType != null)
		{
			ItemType.Initialize();
		}
		if (string.IsNullOrEmpty(PluralName))
		{
			PluralName = Name;
		}
		if (LocomotorType != null)
		{
			LocomotorType.Initialize();
		}
		if (IntelligenceType != null)
		{
			IntelligenceType.Initialize(this);
		}
		if (SensorType != null)
		{
			SensorType.Initialize();
		}
	}

	public void InitRenderableTypeMode()
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			RenderableTypeMode = RenderableType;
		}
		else
		{
			RenderableTypeMode = EditorRenderableType ?? RenderableType;
		}
		if (RenderableTypeMode == null)
		{
			RenderableType = new RenderableType();
			InitRenderableTypeMode();
		}
	}

	public void LoadContent(ContentManager content)
	{
		if (RenderableType != null)
		{
			RenderableType.LoadContent(content);
		}
		if (EditorRenderableType != null)
		{
			EditorRenderableType.LoadContent(content);
		}
	}

	public void PostLoadContentInitialize()
	{
		if (IntelligenceType != null)
		{
			_ = Person;
			IntelligenceType.PostLoadContentInitialize(this);
		}
		if (StructureType != null)
		{
			StructureType.PostLoadContentInitialize(this);
		}
		if (BiologicalType != null)
		{
			BiologicalType.PostLoadContentInitialize(this);
		}
		if (ContainerType != null)
		{
			ContainerType.PostLoadContentInitialize(this);
		}
		if (ToolType != null)
		{
			ToolType.PostLoadContentInitialize();
		}
		if (NonLivingType != null)
		{
			NonLivingType.PostLoadContentInitialize();
		}
		if (Parts != null)
		{
			foreach (KeyValuePair<EntityType, int> part in Parts)
			{
				if (part.Value > 0)
				{
					part.Key.ItemType.CanBeAPart = true;
				}
			}
		}
		if (SensorType != null)
		{
			SensorType.PostLoadContentInitialize();
		}
		if (TreeType != null)
		{
			TreeType.PostLoadContentInitialize();
		}
		if (ItemType != null)
		{
			ItemType.PostLoadContentInitialize();
		}
		if (GameData.Instance.EntityEventHooksByEntityType.TryGetValue(this, out var value))
		{
			foreach (EntityEventHook item in value)
			{
				Common.AddToMultiList(EventActions, item.Hook, GameData.Instance.AllActionSets[item.ActionSetsKey]);
			}
		}
		if (!GameData.Instance.EntityPolledEventByEntityType.TryGetValue(this, out var value2))
		{
			return;
		}
		foreach (EntityTypePolledEvent item2 in value2)
		{
			Common.AddToMultiList(PolledEvents, item2.Scope, GameData.Instance.AllPolledEvents[item2.PolledEventKey]);
		}
	}

	public void CreateSpecialProcesses()
	{
		Pair<string, bool>[] sharedSpecialActions;
		if (SharedSpecialActions != null)
		{
			SharedSpecialActionTypes = new List<ProcessType>();
			sharedSpecialActions = SharedSpecialActions;
			foreach (Pair<string, bool> pair in sharedSpecialActions)
			{
				if (GameData.Instance.AllProcessTypes.TryGetValue(pair.First, out var value))
				{
					bool second = pair.Second;
					ProcessType item = CreateUniqueSpecialProcess(value, second);
					SharedSpecialActionTypes.Add(item);
				}
			}
		}
		if (SpecialActionLocks == null)
		{
			return;
		}
		SpecialActionLockTypes = new List<ProcessType>();
		sharedSpecialActions = SpecialActionLocks;
		foreach (Pair<string, bool> pair2 in sharedSpecialActions)
		{
			if (GameData.Instance.AllProcessTypes.TryGetValue(pair2.First, out var value2))
			{
				bool second2 = pair2.Second;
				ProcessType item2 = CreateUniqueSpecialProcess(value2, second2);
				SpecialActionLockTypes.Add(item2);
			}
		}
	}

	private ProcessType CreateUniqueSpecialProcess(ProcessType originalProcessType, bool isAvailable)
	{
		string newKeyname = originalProcessType.KeyName + "_" + KeyName;
		ProcessType processType = new ProcessType(originalProcessType, newKeyname, isAvailable);
		processType.ActingOnType = this;
		processType.InitDynamicProcess();
		return processType;
	}

	public bool IsRepairable()
	{
		if ((IntelligenceType == null || !IntelligenceType.IsMobile) && (ItemType == null || Upgrader != null) && NonLivingType != null && NonLivingType.RepairProfile != null)
		{
			return true;
		}
		return false;
	}

	public bool RequiresOutsideReplenishment()
	{
		if (IntelligenceType != null && IntelligenceType.CanReplenish != true && !IntelligenceType.IsMobile && IntelligenceType.IntrinsicWeaponTypes != null)
		{
			foreach (EntityType intrinsicWeaponType in IntelligenceType.IntrinsicWeaponTypes)
			{
				if (intrinsicWeaponType.ContainerType != null && intrinsicWeaponType.ContainerType is MagazineContainerType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsSpecialActionOutput(EntityType structureType)
	{
		if (SharedSpecialActionTypes != null)
		{
			if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(structureType, out var processTypes) && SharedSpecialActionTypes.Exists((ProcessType p) => processTypes.Contains(p)))
			{
				return true;
			}
		}
		return false;
	}

	public Rectangle GetIconSprite(out IconInfo iconInfo)
	{
		Rectangle? spriteRect;
		string text = ((ItemType != null) ? ((!string.IsNullOrEmpty(Icon)) ? Icon : ((RenderableTypeMode == null || RenderableTypeMode.DefaultClientState == null || RenderableTypeMode.DefaultClientState.RenderAsBillboardType == null || RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName == null || !The.InGameUI.gui.GUISpriteSheet.TryGetSourceRectangle(RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName, out spriteRect)) ? "boxes" : RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName)) : ((StructureType != null) ? "HUD_icon_structure" : ((TreeType != null) ? "HUD_icon_plant" : ((BiologicalType != null) ? ((Person == null) ? "HUD_icon_animal" : "HUD_icon_person") : ((TerrainType == null) ? "boxes" : ((!TerrainType.IsSpecialInterestFeature) ? "boxes" : "HUD_icon_star"))))));
		Rectangle sourceRectangle = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle(text);
		GameData.Instance.AllIconInfo.TryGetValue(text, out iconInfo);
		return sourceRectangle;
	}

	public float? GetMeanHitpoints()
	{
		if (BiologicalType != null)
		{
			return BiologicalType.MeanHitpointsOfAdultMember;
		}
		if (BodyType != null)
		{
			return BodyType.Hitpoints;
		}
		return null;
	}

	public bool IsMountableWeapon()
	{
		if (ItemType != null && ItemType.WeaponType != null)
		{
			return IsMountable();
		}
		return false;
	}

	public bool IsMountable()
	{
		if (ToolType != null)
		{
			if (ToolType.ToolHandling == ToolHandlingType.HandTool)
			{
				return true;
			}
		}
		else if (ItemType != null && ItemType.WeaponType != null)
		{
			return ItemType.WeaponType.IsIntrinsic != true;
		}
		return false;
	}

	public bool IsImmovable()
	{
		if (ItemType != null)
		{
			if (!ItemType.HasNoMaximumBulk)
			{
				if (ItemType.MaximumBulk.HasValue)
				{
					return Item.IsImmovable(ItemType.MaximumBulk.Value);
				}
				return false;
			}
			return true;
		}
		if (Upgrader != null)
		{
			return true;
		}
		if (StructureType != null)
		{
			return true;
		}
		return false;
	}

	public bool IsIntrinsic()
	{
		if (ToolType != null)
		{
			return ToolType.ToolHandling == ToolHandlingType.Intrinsic;
		}
		if (ItemType != null && ItemType.WeaponType != null)
		{
			return ItemType.WeaponType.IsIntrinsic == true;
		}
		return false;
	}

	public bool GetIsNeverInFogOfWar()
	{
		if (IsNeverInFogOfWar.HasValue)
		{
			return IsNeverInFogOfWar.Value;
		}
		if (TreeType == null && TerrainType == null)
		{
			return RockType != null;
		}
		return true;
	}

	public List<EffectProfileType> GetEffectProfiles()
	{
		List<EffectProfileType> list = null;
		if (Upgrader != null)
		{
			Common.AddRangeToList(ref list, Upgrader.EffectsFinal);
		}
		if (ItemType != null)
		{
			Common.AddRangeToList(ref list, ItemType.FinalEffectsWhenEquipped);
			if (ItemType.FoodType != null)
			{
				Common.AddRangeToList(ref list, ItemType.FoodType.EffectTypes);
			}
			if (ItemType.WeaponType != null)
			{
				AttackType[] attackTypes = ItemType.WeaponType.AttackTypes;
				foreach (AttackType attackType in attackTypes)
				{
					Common.AddRangeToList(ref list, attackType.FinalEffectsOnVictim);
				}
			}
		}
		if (IntelligenceType != null && IntelligenceType.IntrinsicWeaponTypes != null)
		{
			foreach (EntityType intrinsicWeaponType in IntelligenceType.IntrinsicWeaponTypes)
			{
				AttackType[] attackTypes = intrinsicWeaponType.ItemType.WeaponType.AttackTypes;
				foreach (AttackType attackType2 in attackTypes)
				{
					Common.AddRangeToList(ref list, attackType2.FinalEffectsOnVictim);
				}
			}
		}
		return list;
	}

	public bool GetUsesMemory()
	{
		if (UsesMemory.HasValue)
		{
			return UsesMemory.Value;
		}
		if (TreeType == null && TerrainType == null)
		{
			return RockType == null;
		}
		return false;
	}

	public bool HasStance()
	{
		if (LocomotorType != null)
		{
			return LocomotorType.StancesType != null;
		}
		return false;
	}

	public override string ToString()
	{
		return Name + "(" + KeyName + ")";
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

	public static CustomXmlSerializer.XmlTypeMapping<EntityType, string> GetEntityTypePropertySerializer(bool useEntityPlaceholder)
	{
		if (useEntityPlaceholder)
		{
			return new CustomXmlSerializer.XmlTypeMapping<EntityType, string>
			{
				GetterMethod = (EntityType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? new EntityType(s) : null
			};
		}
		return new CustomXmlSerializer.XmlTypeMapping<EntityType, string>
		{
			GetterMethod = (EntityType t) => t?.KeyName,
			SetterMethod = (string s) => (s != null) ? GameData.Instance.AllEntityTypes[s] : null
		};
	}
}
