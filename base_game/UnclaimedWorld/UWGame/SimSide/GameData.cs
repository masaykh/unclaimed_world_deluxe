using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using UWGame.Client.Audio;
using UWGame.ClientSide;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Hints;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Vegetation;
using WindowSystem;
using Xclna.Xna.Animation;

namespace UWGame.SimSide;

public class GameData
{
	private static GameData instance;

	public Dictionary<string, EntityType> AllItemTypes = new Dictionary<string, EntityType>();

	public Dictionary<string, EntityType> AllTreeTypes = new Dictionary<string, EntityType>();

	public Dictionary<string, EntityType> AllTerrainFeatureTypes = new Dictionary<string, EntityType>();

	public Dictionary<string, EntityType> AllStructureTypes = new Dictionary<string, EntityType>();

	public Dictionary<string, EntityType> AllCreatureTypes = new Dictionary<string, EntityType>();

	public Dictionary<string, EntityType> AllVerminTypes = new Dictionary<string, EntityType>();

	public Dictionary<Type, IGameDataCollection> AllGameDataCollections = new Dictionary<Type, IGameDataCollection>();

	public GameDataCollection<EntityCategory> AllEntityCategories;

	public GameDataCollection<ResourceCategory> AllResourceCategories;

	public GameDataCollection<AttackType> AllAttackTypes;

	public GameDataCollection<DamageType> AllDamageTypes;

	public GameDataCollection<StancesType> AllStancesTypes;

	public GameDataCollection<StanceType> AllStanceTypes;

	public GameDataCollection<DegradeType> AllDegradeTypes;

	public GameDataCollection<StorageCondition> AllStorageConditions;

	public GameDataCollection<FoodNutrientProfile> AllFoodNutrientProfiles;

	public GameDataCollection<ProcessToolSet> AllProcessToolSets;

	public GameDataCollection<SkillType> AllSkillTypes;

	public GameDataCollection<SkillCategory> AllSkillCategories;

	public GameDataCollection<ProfessionType> AllProfessionTypes;

	public GameDataCollection<EntityType> AllEntityTypes;

	public GameDataCollection<DetectionType> AllDetectionTypes;

	public GameDataCollection<TriggerType> AllTriggerTypes;

	public GameDataCollection<RenderableType> AttachableRenderableTypes;

	public GameDataCollection<EntityTypeDescription> AllEntityTypeDescriptions;

	public GameDataCollection<BodyType> AllBodyTypes;

	public GameDataCollection<BodyLayerType> AllBodyLayerTypes;

	public GameDataCollection<LowVegetationType> AllLowVegetationTypes;

	public GameDataCollection<SoilComponentType> AllSoilComponentTypes;

	public GameDataCollection<FoodNutrientType> AllFoodNutrientTypes;

	public GameDataCollection<DefaultStorageSettings> AllDefaultStorageSettings;

	public GameDataCollection<PresentationTypeCategory> AllPresentationTypeCategories;

	public GameDataCollection<PresentationType> AllPresentationTypes;

	public GameDataCollection<ResourceType> AllResourceTypes;

	public GameDataCollection<SoundData> AllSoundData;

	public GameDataCollection<TierType> AllTierTypes;

	public GameDataCollection<TierArea> AllTierAreas;

	public GameDataCollection<UpgradeCategory> AllUpgradeCategories;

	public GameDataCollection<UpgradeProfile> AllUpgradeProfiles;

	public GameDataCollection<PersonalityType> AllPersonalityTypes;

	public GameDataCollection<TraitTemplate> AllTraitTemplates;

	public GameDataCollection<CultureTemplate> AllCultureTemplates;

	public GameDataCollection<BioOrderType> AllBioOrderTypes;

	public GameDataCollection<PolledEventType> AllPolledEvents;

	public GameDataCollection<RepairProfile> AllRepairProfiles;

	public GameDataCollection<SubstanceType> AllSubstanceTypes;

	public GameDataCollection<ProcessType> AllProcessTypes;

	public GameDataCollection<EffectProfileType> AllEffectProfileTypes;

	public GameDataCollection<EffectType> AllEffectTypes;

	public GameDataCollection<ActionSets> AllActionSets;

	public GameDataCollection<ActionSetType> AllActionSetTypes;

	public GameDataCollection<EventActionType> AllEventActionTypes;

	public GameDataCollection<EntityData> AllEntityData;

	public GameDataCollection<SiteTemplate> AllSiteTemplates;

	public GameDataCollection<AllegianceTemplate> AllAllegianceTemplates;

	public GameDataCollection<SiteData> AllSiteData;

	public GameDataCollection<AllegianceData> AllAllegianceData;

	public GameDataCollection<ExpeditionData> AllExpeditionData;

	public GameDataCollection<TradeProfile> AllTradeProfiles;

	public GameDataCollection<VehiclesProfile> AllVehiclesProfiles;

	public GameDataCollection<StructuresProfile> AllStructuresProfiles;

	public GameDataCollection<TradeGroup> AllTradeGroups;

	public GameDataCollection<PricesProfile> AllPricesProfiles;

	public GameDataCollection<OfferDemandProfile> AllOfferDemandProfiles;

	public GameDataCollection<IconInfo> AllIconInfo;

	public GameDataCollection<JobType> AllJobTypes;

	public GameDataCollection<FilterSettingType> AllFilterSettingTypes;

	public Dictionary<string, AgentActionHook> AllAgentActionHooks = new Dictionary<string, AgentActionHook>();

	public Dictionary<string, EntityEventHook> AllEntityEventHooks = new Dictionary<string, EntityEventHook>();

	public Dictionary<string, AttackTypeActionHook> AllAttackTypeEventHooks = new Dictionary<string, AttackTypeActionHook>();

	public Dictionary<string, ProcessTypeActionHook> AllProcessTypeEventHooks = new Dictionary<string, ProcessTypeActionHook>();

	public Dictionary<string, EffectTypeActionHook> AllEffectTypeEventHooks = new Dictionary<string, EffectTypeActionHook>();

	public Dictionary<string, DetectEntityTypeHook> AllDetectedEntityEventHooks = new Dictionary<string, DetectEntityTypeHook>();

	public Dictionary<string, DetectResourceTypeHook> AllDetectedResourceEventHooks = new Dictionary<string, DetectResourceTypeHook>();

	public Dictionary<string, EntityTypePolledEvent> AllEntityPolledEvents = new Dictionary<string, EntityTypePolledEvent>();

	public Dictionary<EntityType, List<AgentActionHook>> AgentActionHooksByEntityType = new Dictionary<EntityType, List<AgentActionHook>>();

	public Dictionary<EntityType, List<EntityEventHook>> EntityEventHooksByEntityType = new Dictionary<EntityType, List<EntityEventHook>>();

	public Dictionary<EntityType, List<DetectEntityTypeHook>> DetectedEntityHooksByEntityType = new Dictionary<EntityType, List<DetectEntityTypeHook>>();

	public Dictionary<EntityType, List<DetectResourceTypeHook>> DetectedResourceHooksByEntityType = new Dictionary<EntityType, List<DetectResourceTypeHook>>();

	public Dictionary<AttackType, List<AttackTypeActionHook>> EventHooksByAttackType = new Dictionary<AttackType, List<AttackTypeActionHook>>();

	public Dictionary<ProcessType, List<ProcessTypeActionHook>> EventHooksByProcessType = new Dictionary<ProcessType, List<ProcessTypeActionHook>>();

	public Dictionary<EffectProfileType, List<EffectTypeActionHook>> EventHooksByEffectType = new Dictionary<EffectProfileType, List<EffectTypeActionHook>>();

	public Dictionary<EntityType, List<EntityTypePolledEvent>> EntityPolledEventByEntityType = new Dictionary<EntityType, List<EntityTypePolledEvent>>();

	public Dictionary<string, AllegianceEventType> AllAllegianceEventTypes = new Dictionary<string, AllegianceEventType>();

	public Dictionary<AllegianceEvents, List<ActionSets>> AllegianceEvents = new Dictionary<AllegianceEvents, List<ActionSets>>();

	public Dictionary<string, HelpTopic> AllHelpTopics = new Dictionary<string, HelpTopic>();

	public Dictionary<string, HelpTopic> AllTutorialTopics = new Dictionary<string, HelpTopic>();

	public Dictionary<string, List<EntityType>> ToolsByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, List<EntityType>> FuelByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, List<EntityType>> AmmoByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, List<IDetectableType>> DetectableTypeByTag = new Dictionary<string, List<IDetectableType>>();

	public Dictionary<string, List<EntityType>> FoodByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, List<EntityType>> ServantEntityTypeByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, List<EntityType>> GeneralTags = new Dictionary<string, List<EntityType>>();

	public Dictionary<string, int> ContainerTags = new Dictionary<string, int>();

	public Dictionary<string, List<EntityType>> ContainersByTag = new Dictionary<string, List<EntityType>>();

	public Dictionary<EntityCategory, List<EntityType>> ItemTypesInCategory = new Dictionary<EntityCategory, List<EntityType>>();

	public Dictionary<UpgradeCategory, List<EntityType>> UpgraderEntityTypesByUpgradeCategory = new Dictionary<UpgradeCategory, List<EntityType>>();

	public Dictionary<UpgradeCategory, List<EntityType>> EntityTypesToUpgradeByUpgradeCategory = new Dictionary<UpgradeCategory, List<EntityType>>();

	public Dictionary<EntityType, ResourceType> ItemHarvestSource = new Dictionary<EntityType, ResourceType>();

	public Dictionary<EntityType, List<ProcessType>> ProcessYieldsThisOutput = new Dictionary<EntityType, List<ProcessType>>();

	public Dictionary<EntityType, List<ProcessType>> SalvageProcessYieldsThisOutput = new Dictionary<EntityType, List<ProcessType>>();

	public List<ProcessType> AllProductionProcesses = new List<ProcessType>();

	public List<ProcessType> NonSalvageProductionProcesses = new List<ProcessType>();

	public Dictionary<EntityType, List<ProcessType>> ProcessesUsingThisInput = new Dictionary<EntityType, List<ProcessType>>();

	public Dictionary<EntityType, HashSet<ProcessType>> ToolsUsedFor = new Dictionary<EntityType, HashSet<ProcessType>>();

	public Dictionary<string, ParticleSystemType> AllParticleSystems = new Dictionary<string, ParticleSystemType>();

	public List<Hint> AllHints;

	public float OneOverMeanDamageFromHumanPunch;

	public Dictionary<AttackType, Dictionary<BodyPartType, float>> AttackScoresAgainstBodyParts = new Dictionary<AttackType, Dictionary<BodyPartType, float>>();

	public Dictionary<AttackType, Dictionary<BodyType, float>> MeanAttackDamageAgainstEnemyTypes = new Dictionary<AttackType, Dictionary<BodyType, float>>();

	public Dictionary<AttackType, Dictionary<BodyType, float>> HighAttackDamageAgainstEnemyTypes = new Dictionary<AttackType, Dictionary<BodyType, float>>();

	public Dictionary<string, ModelData> AllModels;

	public Dictionary<string, Texture> ExtraModelTextures;

	public ExtendedSpriteSheet BillboardSpriteSheet;

	// PORT DEVIATION 16 (see PORTING-NOTES.md). Was LightSourceSpriteSheet, following the game
	// itself: 1.0.4.8 deleted that class and loads LightSources as a plain SpriteSheet. Typed as
	// the base class so BOTH versions' asset loads here - see the load site.
	public SpriteSheet LightSourcesSpriteSheet;

	public Dictionary<string, Animation2D> Animation2Ds = new Dictionary<string, Animation2D>();

	public Constants Constants;

	public AIConstants AIConstants;

	public GUIConstants GUIConstants;

	public CustomDataPresentation CustomEntityActivityData;

	public CustomDataPresentation CustomStatusIconData;

	public CustomDataPresentation CustomSidePanelData;

	public CustomDataPresentation CustomOtherSiteSidePanelData;

	public CustomDataPresentation CustomEntityTypeTooltipData;

	public TierType[] Tiers;

	private int state;

	private Dictionary<string, List<string>> allPostLoadContentValidationErrors;

	private Dictionary<BodyType, HashSet<EntityType>> bodyToEntityMappings = new Dictionary<BodyType, HashSet<EntityType>>();

	public static GameData Instance
	{
		get
		{
			if (instance != null)
			{
				return instance;
			}
			instance = new GameData();
			return instance;
		}
	}

	private GameData()
	{
		AllEntityCategories = new GameDataCollection<EntityCategory>(AllGameDataCollections, DataLoaderQueueState.EntityCategories);
		AllResourceCategories = new GameDataCollection<ResourceCategory>(AllGameDataCollections, DataLoaderQueueState.ResourceCategories);
		AllAttackTypes = new GameDataCollection<AttackType>(AllGameDataCollections, DataLoaderQueueState.AttackTypes);
		AllDamageTypes = new GameDataCollection<DamageType>(AllGameDataCollections, DataLoaderQueueState.DamageTypes);
		AllStancesTypes = new GameDataCollection<StancesType>(AllGameDataCollections, DataLoaderQueueState.StancesTypes);
		AllStanceTypes = new GameDataCollection<StanceType>(AllGameDataCollections, DataLoaderQueueState.StanceTypes);
		AllDegradeTypes = new GameDataCollection<DegradeType>(AllGameDataCollections, DataLoaderQueueState.DegradeProfiles);
		AllStorageConditions = new GameDataCollection<StorageCondition>(AllGameDataCollections, DataLoaderQueueState.StorageConditions);
		AllFoodNutrientProfiles = new GameDataCollection<FoodNutrientProfile>(AllGameDataCollections, DataLoaderQueueState.FoodNutrientProfiles);
		AllProcessToolSets = new GameDataCollection<ProcessToolSet>(AllGameDataCollections, DataLoaderQueueState.ProcessToolSets);
		AllSkillTypes = new GameDataCollection<SkillType>(AllGameDataCollections, DataLoaderQueueState.SkillTypes);
		AllSkillCategories = new GameDataCollection<SkillCategory>(AllGameDataCollections, DataLoaderQueueState.SkillCategories);
		AllProfessionTypes = new GameDataCollection<ProfessionType>(AllGameDataCollections, DataLoaderQueueState.ProfessionTypes);
		AllEntityTypes = new GameDataCollection<EntityType>(AllGameDataCollections, DataLoaderQueueState.EntityTypes);
		AllDetectionTypes = new GameDataCollection<DetectionType>(AllGameDataCollections, DataLoaderQueueState.DetectionTypes);
		AllTriggerTypes = new GameDataCollection<TriggerType>(AllGameDataCollections, DataLoaderQueueState.TriggerTypes);
		AttachableRenderableTypes = new GameDataCollection<RenderableType>(AllGameDataCollections, DataLoaderQueueState.AttachableRenderableTypes);
		AllEntityTypeDescriptions = new GameDataCollection<EntityTypeDescription>(AllGameDataCollections, DataLoaderQueueState.EntityTypeDescriptions);
		AllBodyTypes = new GameDataCollection<BodyType>(AllGameDataCollections, DataLoaderQueueState.BodyTypes);
		AllBodyLayerTypes = new GameDataCollection<BodyLayerType>(AllGameDataCollections, DataLoaderQueueState.BodyLayerTypes);
		AllLowVegetationTypes = new GameDataCollection<LowVegetationType>(AllGameDataCollections, DataLoaderQueueState.LowVegetationTypes);
		AllSoilComponentTypes = new GameDataCollection<SoilComponentType>(AllGameDataCollections, DataLoaderQueueState.SoilTypes);
		AllFoodNutrientTypes = new GameDataCollection<FoodNutrientType>(AllGameDataCollections, DataLoaderQueueState.FoodNutrientTypes);
		AllDefaultStorageSettings = new GameDataCollection<DefaultStorageSettings>(AllGameDataCollections, DataLoaderQueueState.DefaultStorageSettings);
		AllPresentationTypes = new GameDataCollection<PresentationType>(AllGameDataCollections, DataLoaderQueueState.PresentationTypes);
		AllPresentationTypeCategories = new GameDataCollection<PresentationTypeCategory>(AllGameDataCollections, DataLoaderQueueState.PresentationTypeCategories);
		AllResourceTypes = new GameDataCollection<ResourceType>(AllGameDataCollections, DataLoaderQueueState.ResourceTypes);
		AllSoundData = new GameDataCollection<SoundData>(AllGameDataCollections, DataLoaderQueueState.Sounds);
		AllPersonalityTypes = new GameDataCollection<PersonalityType>(AllGameDataCollections, DataLoaderQueueState.Personalities);
		AllTraitTemplates = new GameDataCollection<TraitTemplate>(AllGameDataCollections, DataLoaderQueueState.Traits);
		AllCultureTemplates = new GameDataCollection<CultureTemplate>(AllGameDataCollections, DataLoaderQueueState.Cultures);
		AllTierTypes = new GameDataCollection<TierType>(AllGameDataCollections, DataLoaderQueueState.Tiers);
		AllTierAreas = new GameDataCollection<TierArea>(AllGameDataCollections, DataLoaderQueueState.TierAreas);
		AllUpgradeCategories = new GameDataCollection<UpgradeCategory>(AllGameDataCollections, DataLoaderQueueState.UpgradeCategories);
		AllUpgradeProfiles = new GameDataCollection<UpgradeProfile>(AllGameDataCollections, DataLoaderQueueState.UpgradeProfiles);
		AllBioOrderTypes = new GameDataCollection<BioOrderType>(AllGameDataCollections, DataLoaderQueueState.BioOrders);
		AllPolledEvents = new GameDataCollection<PolledEventType>(AllGameDataCollections, DataLoaderQueueState.PolledEventTypes);
		AllRepairProfiles = new GameDataCollection<RepairProfile>(AllGameDataCollections, DataLoaderQueueState.RepairProfiles);
		AllSubstanceTypes = new GameDataCollection<SubstanceType>(AllGameDataCollections, DataLoaderQueueState.Substances);
		AllProcessTypes = new GameDataCollection<ProcessType>(AllGameDataCollections, DataLoaderQueueState.ProcessTypes);
		AllEffectProfileTypes = new GameDataCollection<EffectProfileType>(AllGameDataCollections, DataLoaderQueueState.EffectProfileTypes);
		AllEffectTypes = new GameDataCollection<EffectType>(AllGameDataCollections, DataLoaderQueueState.EffectTypes);
		AllActionSets = new GameDataCollection<ActionSets>(AllGameDataCollections, DataLoaderQueueState.ActionSets);
		AllActionSetTypes = new GameDataCollection<ActionSetType>(AllGameDataCollections, DataLoaderQueueState.ActionSets);
		AllEventActionTypes = new GameDataCollection<EventActionType>(AllGameDataCollections, DataLoaderQueueState.EventActionTypes);
		AllIconInfo = new GameDataCollection<IconInfo>(AllGameDataCollections, DataLoaderQueueState.IconInfo);
		AllEntityData = new GameDataCollection<EntityData>(AllGameDataCollections, DataLoaderQueueState.EntityData);
		AllSiteData = new GameDataCollection<SiteData>(AllGameDataCollections, DataLoaderQueueState.SiteData);
		AllAllegianceData = new GameDataCollection<AllegianceData>(AllGameDataCollections, DataLoaderQueueState.AllegianceData);
		AllAllegianceTemplates = new GameDataCollection<AllegianceTemplate>(AllGameDataCollections, DataLoaderQueueState.AllegianceTemplates);
		AllSiteTemplates = new GameDataCollection<SiteTemplate>(AllGameDataCollections, DataLoaderQueueState.SiteTemplates);
		AllExpeditionData = new GameDataCollection<ExpeditionData>(AllGameDataCollections, DataLoaderQueueState.ExpeditionData);
		AllTradeProfiles = new GameDataCollection<TradeProfile>(AllGameDataCollections, DataLoaderQueueState.TradeProfiles);
		AllStructuresProfiles = new GameDataCollection<StructuresProfile>(AllGameDataCollections, DataLoaderQueueState.StructureProfiles);
		AllVehiclesProfiles = new GameDataCollection<VehiclesProfile>(AllGameDataCollections, DataLoaderQueueState.VehicleProfiles);
		AllTradeGroups = new GameDataCollection<TradeGroup>(AllGameDataCollections, DataLoaderQueueState.TradeGroups);
		AllOfferDemandProfiles = new GameDataCollection<OfferDemandProfile>(AllGameDataCollections, DataLoaderQueueState.OfferDemandProfiles);
		AllPricesProfiles = new GameDataCollection<PricesProfile>(AllGameDataCollections, DataLoaderQueueState.PricesProfiles);
		AllFilterSettingTypes = new GameDataCollection<FilterSettingType>(AllGameDataCollections, DataLoaderQueueState.FilterSettingTypes);
		AllJobTypes = new GameDataCollection<JobType>(AllGameDataCollections, DataLoaderQueueState.JobTypes);
	}

	private bool Advance()
	{
		The.LoadScreen.Progress("LoadGameData stage " + (state + 1), 100);
		state++;
		return false;
	}

	public bool LoadContent(UnclaimedWorld game, ContentManager content)
	{
		switch (state)
		{
		case 0:
			Instance.ExtraModelTextures = new Dictionary<string, Texture>();
			Instance.ExtraModelTextures.Add("PatricianPurpleTexture", content.Load<Texture>("Models\\patrician_texture7"));
			Instance.ExtraModelTextures.Add("PatricianWhiteTexture", content.Load<Texture>("Models\\patrician_texture6"));
			Instance.ExtraModelTextures.Add("PatricianZebraTexture", content.Load<Texture>("Models\\patrician_texture5"));
			Instance.ExtraModelTextures.Add("PatricianWaspTexture", content.Load<Texture>("Models\\patrician_texture4"));
			Instance.ExtraModelTextures.Add("PatricianBrownTexture", content.Load<Texture>("Models\\patrician_texture3"));
			return Advance();
		case 1:
			Instance.ExtraModelTextures.Add("PatricianPaleTexture", content.Load<Texture>("Models\\patrician_texture2"));
			Instance.ExtraModelTextures.Add("PatricianBlackTexture", content.Load<Texture>("Models\\patrician_texture1"));
			Instance.ExtraModelTextures.Add("QuaditeYellowTexture", content.Load<Texture>("Models\\twinkler_texture1"));
			Instance.ExtraModelTextures.Add("QuaditeRedTexture", content.Load<Texture>("Models\\twinkler_texture2"));
			Instance.ExtraModelTextures.Add("QuaditeTurquoiseTexture", content.Load<Texture>("Models\\twinkler_texture3"));
			Instance.ExtraModelTextures.Add("QuaditeThinTexture", content.Load<Texture>("Models\\twinklerThin_texture1"));
			Instance.ExtraModelTextures.Add("QuaditeThinSpikyTexture", content.Load<Texture>("Models\\twinklerThinSpiky_texture1"));
			return Advance();
		case 2:
			Instance.ExtraModelTextures.Add("QuaditeStripedTexture", content.Load<Texture>("Models\\twinkler_texture4"));
			Instance.ExtraModelTextures.Add("BirdPaleTexture", content.Load<Texture>("Models\\bird_texture1"));
			Instance.ExtraModelTextures.Add("BirdDarkTexture", content.Load<Texture>("Models\\bird_texture2"));
			Instance.ExtraModelTextures.Add("BirdBlackTexture", content.Load<Texture>("Models\\bird_texture3"));
			Instance.ExtraModelTextures.Add("BirdPurpleTexture", content.Load<Texture>("Models\\bird_texture3b"));
			Instance.ExtraModelTextures.Add("BirdYellowTexture", content.Load<Texture>("Models\\bird_texture4"));
			Instance.ExtraModelTextures.Add("BirdRedTexture", content.Load<Texture>("Models\\bird_texture5"));
			Instance.ExtraModelTextures.Add("BirdVeryDarkTurqoiseTexture", content.Load<Texture>("Models\\bird_texture5b"));
			Instance.ExtraModelTextures.Add("BirdVeryDarkGreenTexture", content.Load<Texture>("Models\\bird_texture5c"));
			return Advance();
		case 3:
			Instance.ExtraModelTextures.Add("BushdragonPaleTexture", content.Load<Texture>("Models\\scarecrow_texture1"));
			Instance.ExtraModelTextures.Add("BushdragonDarkTexture", content.Load<Texture>("Models\\scarecrow_texture2"));
			Instance.ExtraModelTextures.Add("ThunderchickenPaleTexture", content.Load<Texture>("Models\\thunderChicken_texture2"));
			Instance.ExtraModelTextures.Add("ThunderchickenDarkTexture", content.Load<Texture>("Models\\thunderChicken_texture1"));
			Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture1", content.Load<Texture>("Models\\thunderChickenBulky_texture1"));
			Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture2", content.Load<Texture>("Models\\thunderChickenBulky_texture2"));
			Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture3", content.Load<Texture>("Models\\thunderChickenBulky_texture3"));
			Instance.ExtraModelTextures.Add("ThunderchickenThinTexture1", content.Load<Texture>("Models\\thunderChickenThin_texture1"));
			Instance.ExtraModelTextures.Add("ThunderchickenThinTexture2", content.Load<Texture>("Models\\thunderChickenThin_texture2"));
			Instance.ExtraModelTextures.Add("ThunderchickenThinTexture3", content.Load<Texture>("Models\\thunderChickenThin_texture3"));
			Instance.ExtraModelTextures.Add("SkinnedTestTexture", content.Load<Texture>("Models\\dogShepherd_texture1"));
			Instance.ExtraModelTextures.Add("DemonTreeTexture", content.Load<Texture>("Models\\demonTree_texture1"));
			Instance.ExtraModelTextures.Add("DemonTreeMossTexture1", content.Load<Texture>("Models\\demonTreeMoss_texture1"));
			Instance.ExtraModelTextures.Add("DemonTreeMossTexture2", content.Load<Texture>("Models\\demonTreeMoss_texture2"));
			Instance.ExtraModelTextures.Add("DemonTreeMossTexture3", content.Load<Texture>("Models\\demonTreeMoss_texture3"));
			Instance.ExtraModelTextures.Add("SpikePlantTexture", content.Load<Texture>("Models\\spikePlant_texture1"));
			return Advance();
		case 4:
			Instance.ExtraModelTextures.Add("DogGermanShepherdTexture", content.Load<Texture>("Models\\dogShepherd_texture1"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture7", content.Load<Texture>("Models\\snatcher_texture7"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture6", content.Load<Texture>("Models\\snatcher_texture6"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture5", content.Load<Texture>("Models\\snatcher_texture5"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture4", content.Load<Texture>("Models\\snatcher_texture4"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture3", content.Load<Texture>("Models\\snatcher_texture3"));
			Instance.ExtraModelTextures.Add("SnatcherVariantTexture2", content.Load<Texture>("Models\\snatcher_texture1"));
			Instance.ExtraModelTextures.Add("SnatcherFemaleTexture", content.Load<Texture>("Models\\snatcher_texture2"));
			Instance.ExtraModelTextures.Add("SnatcherArmoredTexture4", content.Load<Texture>("Models\\snatcherArmored_texture4"));
			Instance.ExtraModelTextures.Add("SnatcherArmoredTexture3", content.Load<Texture>("Models\\snatcherArmored_texture3"));
			Instance.ExtraModelTextures.Add("SnatcherArmoredTexture2", content.Load<Texture>("Models\\snatcherArmored_texture2"));
			Instance.ExtraModelTextures.Add("SnatcherArmoredTexture1", content.Load<Texture>("Models\\snatcherArmored_texture1"));
			Instance.ExtraModelTextures.Add("WormTexture", content.Load<Texture>("Models\\worm_texture1"));
			Instance.ExtraModelTextures.Add("WormThinTexture", content.Load<Texture>("Models\\wormThin_texture1"));
			Instance.ExtraModelTextures.Add("WormSimpleTexture", content.Load<Texture>("Models\\wormSimple_texture2"));
			Instance.ExtraModelTextures.Add("BushbackPaleTexture", content.Load<Texture>("Models\\bushback_texture1"));
			Instance.ExtraModelTextures.Add("BushbackDarkTexture", content.Load<Texture>("Models\\bushback_texture2"));
			Instance.ExtraModelTextures.Add("TurnipPaleTexture", content.Load<Texture>("Models\\turnip_texture1"));
			Instance.ExtraModelTextures.Add("TurnipDarkTexture", content.Load<Texture>("Models\\turnip_texture2"));
			return Advance();
		case 5:
			Instance.ExtraModelTextures.Add("ManColorReplaceTexture", content.Load<Texture>("Models\\man_texture"));
			Instance.ExtraModelTextures.Add("ManBlue1Texture", content.Load<Texture>("Models\\man_texture1"));
			Instance.ExtraModelTextures.Add("ManBlue2Texture", content.Load<Texture>("Models\\man_texture2"));
			Instance.ExtraModelTextures.Add("ManRed1Texture", content.Load<Texture>("Models\\man_texture3"));
			Instance.ExtraModelTextures.Add("ManRed2Texture", content.Load<Texture>("Models\\man_texture4"));
			Instance.ExtraModelTextures.Add("ManGreen1Texture", content.Load<Texture>("Models\\man_texture5"));
			return Advance();
		case 6:
			Instance.ExtraModelTextures.Add("ManGreen2Texture", content.Load<Texture>("Models\\man_texture6"));
			Instance.ExtraModelTextures.Add("ManGrey1Texture", content.Load<Texture>("Models\\man_texture7"));
			Instance.ExtraModelTextures.Add("ManGrey2Texture", content.Load<Texture>("Models\\man_texture8"));
			Instance.ExtraModelTextures.Add("ManGrey3Texture", content.Load<Texture>("Models\\man_texture9"));
			Instance.ExtraModelTextures.Add("ManBlueSolid1Texture", content.Load<Texture>("Models\\man_texture10"));
			Instance.ExtraModelTextures.Add("ManGreenSolid1Texture", content.Load<Texture>("Models\\man_texture15"));
			Instance.ExtraModelTextures.Add("ManGreySolid1Texture", content.Load<Texture>("Models\\man_texture16"));
			Instance.ExtraModelTextures.Add("ManGreySolid2Texture", content.Load<Texture>("Models\\man_texture18"));
			Instance.ExtraModelTextures.Add("ManBlueBrownClothes1Texture", content.Load<Texture>("Models\\man_textureClothes1"));
			Instance.ExtraModelTextures.Add("ManGreenGreyClothes1Texture", content.Load<Texture>("Models\\man_textureClothes2"));
			Instance.ExtraModelTextures.Add("ManGreyClothes1Texture", content.Load<Texture>("Models\\man_textureClothes3"));
			Instance.ExtraModelTextures.Add("ManWhitePantsClothes1Texture", content.Load<Texture>("Models\\man_textureClothes4"));
			Instance.ExtraModelTextures.Add("ManGreenBlueClothes1Texture", content.Load<Texture>("Models\\man_textureClothes5"));
			Instance.ExtraModelTextures.Add("ManWhiteBlueClothes1Texture", content.Load<Texture>("Models\\man_textureClothes6"));
			Instance.ExtraModelTextures.Add("ManBlueBrownClothesBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes7"));
			Instance.ExtraModelTextures.Add("ManOrangeGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes8"));
			Instance.ExtraModelTextures.Add("ManCurryClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes9"));
			Instance.ExtraModelTextures.Add("ManDarkRedClothesDarkSkinBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes10"));
			Instance.ExtraModelTextures.Add("ManBrownGreyClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes11"));
			Instance.ExtraModelTextures.Add("ManTurquoiseDarkClothesBlondHairTexture", content.Load<Texture>("Models\\man_textureClothes12"));
			Instance.ExtraModelTextures.Add("ManSandyClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes13"));
			Instance.ExtraModelTextures.Add("ManBurgundyClothesWhiteHairTexture", content.Load<Texture>("Models\\man_textureClothes14"));
			Instance.ExtraModelTextures.Add("ManDarkBlueBeigeClothesBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes15"));
			Instance.ExtraModelTextures.Add("ManOrangeDarkGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes16"));
			Instance.ExtraModelTextures.Add("ManDarkBrownClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes17"));
			Instance.ExtraModelTextures.Add("ManBrownBeigeClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes18"));
			Instance.ExtraModelTextures.Add("ManSandyClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes19"));
			Instance.ExtraModelTextures.Add("ManBlueGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes20"));
			Instance.ExtraModelTextures.Add("ManOrangeGreyClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes21"));
			Instance.ExtraModelTextures.Add("ManCurryGreyClothesBrownSkinTexture", content.Load<Texture>("Models\\man_textureClothes22"));
			Instance.ExtraModelTextures.Add("ManOchreClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes23"));
			Instance.ExtraModelTextures.Add("ManBrownGreyClothesBlondHairTexture", content.Load<Texture>("Models\\man_textureClothes24"));
			Instance.ExtraModelTextures.Add("ManTurquoiseDarkClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes25"));
			Instance.ExtraModelTextures.Add("ManSandyDarkClothesWhiteHairTexture", content.Load<Texture>("Models\\man_textureClothes26"));
			Instance.ExtraModelTextures.Add("BinalRatBrownTexture", content.Load<Texture>("Models\\binalRat_texture1"));
			Instance.ExtraModelTextures.Add("RobotTexture1", content.Load<Texture>("Models\\robot_texture1"));
			Instance.ExtraModelTextures.Add("RobotTexture2", content.Load<Texture>("Models\\robot_texture2"));
			return Advance();
		case 7:
			Instance.AllModels = new Dictionary<string, ModelData>();
			Instance.AllModels.Add("sentry", new ModelData
			{
				Model = content.Load<Model>("Models\\sentry_idle"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 6f,
				CRTDisplayLightIntensity = 5f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 8:
			Instance.AllModels.Add("skimmer", new ModelData
			{
				Model = content.Load<Model>("Models\\skimmer"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 0.8f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				DriverAttachor = new AttachPoint
				{
					KeyName = "DriversSeat",
					Translation = new Vector3(0f, 0f, 0f),
					BoneName = "HULL",
					AttachedAnimationName = "driving"
				},
				PassengerAttachors = new List<AttachPoint>
				{
					new AttachPoint
					{
						KeyName = "FrontSeat",
						BoneName = "HULL",
						AttachedAnimationName = "passenger_1_seated",
						Translation = new Vector3(-5f, 10f, 0f)
					}
				}
			});
			return Advance();
		case 9:
			Instance.AllModels.Add("utilityvehicle", new ModelData
			{
				Model = content.Load<Model>("Models\\utility_vehicle"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2.4f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				DriverAttachor = new AttachPoint
				{
					KeyName = "DriversSeat",
					Translation = new Vector3(5f, 10f, 0f),
					BoneName = "hull",
					AttachedAnimationName = "driving"
				},
				PassengerAttachors = new List<AttachPoint>
				{
					new AttachPoint
					{
						KeyName = "FrontSeat",
						BoneName = "hull",
						AttachedAnimationName = "passenger_1_seated",
						Translation = new Vector3(-5f, 10f, 0f)
					},
					new AttachPoint
					{
						KeyName = "CargoLeft",
						BoneName = "hull",
						AttachedAnimationName = "passenger_2_lying_left",
						Translation = new Vector3(5f, 10f, -10f)
					},
					new AttachPoint
					{
						KeyName = "CargoRight",
						BoneName = "hull",
						AttachedAnimationName = "passenger_3_lying_right",
						Translation = new Vector3(-5f, 10f, -10f)
					}
				}
			});
			return Advance();
		case 10:
			Instance.AllModels.Add("patrician", new ModelData
			{
				Model = content.Load<Model>("Models\\patrician_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 11:
			Instance.AllModels.Add("turnip", new ModelData
			{
				Model = content.Load<Model>("Models\\turnip_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 12:
			Instance.AllModels.Add("twinkler", new ModelData
			{
				Model = content.Load<Model>("Models\\twinkler_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 13:
			Instance.AllModels.Add("bird", new ModelData
			{
				Model = content.Load<Model>("Models\\bird_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 14:
			Instance.AllModels.Add("thunderchicken", new ModelData
			{
				Model = content.Load<Model>("Models\\thunderChicken_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 15:
			Instance.AllModels.Add("bushdragon", new ModelData
			{
				Model = content.Load<Model>("Models\\scarecrow_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 16:
			Instance.AllModels.Add("forestguardian", new ModelData
			{
				Model = content.Load<Model>("Models\\bushback_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				BackAttachor = new AttachPoint
				{
					BoneName = "Root",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(0f, 0f, 0f)
				}
			});
			return Advance();
		case 17:
			Instance.AllModels.Add("man", new ModelData
			{
				Model = content.Load<Model>("Models\\man_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				LeftHandAttachor = new AttachPoint
				{
					Tag = "leftHand",
					IsHand = true,
					BoneName = "FingerLeftA",
					Translation = new Vector3(0.25f, -0.6f, 0f)
				},
				RightHandAttachor = new AttachPoint
				{
					Tag = "rightHand",
					IsHand = true,
					BoneName = "FingerRightA",
					Translation = new Vector3(0.25f, -0.6f, 0f)
				},
				BackAttachor = new AttachPoint
				{
					Tag = "back",
					BoneName = "SpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				HelmetAttachor = new AttachPoint
				{
					Tag = "head",
					BoneName = "Head",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(0f, 0f, 0f)
				},
				BottomAttachee = new AttachPoint
				{
					Tag = "bottom",
					BoneName = "SpineD",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(0f, 0f, 0f)
				}
			});
			return Advance();
		case 18:
			Instance.AllModels.Add("woman", new ModelData
			{
				Model = content.Load<Model>("Models\\woman_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				LeftHandAttachor = new AttachPoint
				{
					Tag = "leftHand",
					IsHand = true,
					BoneName = "FingerLeftA",
					Translation = new Vector3(0.25f, -0.6f, 0f)
				},
				RightHandAttachor = new AttachPoint
				{
					Tag = "rightHand",
					IsHand = true,
					BoneName = "FingerRightA",
					Translation = new Vector3(0.25f, -0.6f, 0f)
				},
				BackAttachor = new AttachPoint
				{
					Tag = "back",
					BoneName = "SpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				HelmetAttachor = new AttachPoint
				{
					Tag = "head",
					BoneName = "Head",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(0f, 0f, 0f)
				},
				BottomAttachee = new AttachPoint
				{
					Tag = "bottom",
					BoneName = "SpineD",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(0f, 0f, 0f)
				}
			});
			return Advance();
		case 19:
			Instance.AllModels.Add("box", new ModelData
			{
				Model = content.Load<Model>("Models\\box"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Rotation = new Vector3(178.11f, 103.465f, -147.874f),
					Translation = new Vector3(2.966f, -4.173f, -3.281f)
				},
				LeftHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerLeftA",
					Rotation = new Vector3(-180f, 180f, -180f),
					Translation = new Vector3(0.367f, -3.832f, -5.617f)
				},
				BottomAttachee = new AttachPoint
				{
					BoneName = "AttacheeBottom",
					Rotation = new Vector3(180f, -1.417f, -180f),
					Translation = new Vector3(0.079f, 0.236f, -11f)
				}
			});
			Instance.AllModels.Add("backpackHeavy", new ModelData
			{
				Model = content.Load<Model>("Models\\backpackHeavy"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Rotation = new Vector3(-25.984f, 91.18101f, -133.701f),
					Translation = new Vector3(-3.491f, -6.01f, -0.131f)
				}
			});
			return Advance();
		case 20:
			Instance.AllModels.Add("backpack", new ModelData
			{
				Model = content.Load<Model>("Models\\backpack"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f
			});
			return Advance();
		case 21:
			Instance.AllModels.Add("hammer", new ModelData
			{
				Model = content.Load<Model>("Models\\hammer"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "HammerJoint",
					Translation = new Vector3(1.129f, -3.438f, -0.6f),
					Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Rotation = new Vector3(180f, -177.165f, -103.465f),
					Translation = new Vector3(-1.942f, 0.157f, 0.052f)
				}
			});
			return Advance();
		case 22:
			Instance.AllModels.Add("spear", new ModelData
			{
				Model = content.Load<Model>("Models\\spear"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "SpearJoint",
					Translation = new Vector3(0.656f, 0.079f, 0.079f),
					Rotation = new Vector3(-83.622f, -0.472f, -17.48f)
				}
			});
			Instance.AllModels.Add("machete", new ModelData
			{
				Model = content.Load<Model>("Models\\machete"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(1.129f, -3.438f, -0.6f),
					Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "MacheteJoint",
					Rotation = new Vector3(180f, -175.3f, -115.7f),
					Translation = new Vector3(-0.341f, -0.236f, -0.026f)
				}
			});
			Instance.AllModels.Add("pickaxe", new ModelData
			{
				Model = content.Load<Model>("Models\\pickaxe"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "PickaxeJoint",
					Translation = new Vector3(1.129f, -3.438f, -0.6f),
					Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Rotation = new Vector3(180f, -177.165f, -103.465f),
					Translation = new Vector3(-2.467f, -4.462f, -0.262f)
				}
			});
			Instance.AllModels.Add("axe", new ModelData
			{
				Model = content.Load<Model>("Models\\axe"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AxeJoint",
					Translation = new Vector3(1.129f, -3.438f, -0.6f),
					Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Rotation = new Vector3(180f, -177.165f, -103.465f),
					Translation = new Vector3(-1.942f, 0.157f, 0.052f)
				}
			});
			Instance.AllModels.Add("knife", new ModelData
			{
				Model = content.Load<Model>("Models\\knife"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "KnifeJoint",
					Rotation = new Vector3(180f, -175.3f, -115.7f),
					Translation = new Vector3(-0.341f, -0.236f, -0.026f)
				}
			});
			Instance.AllModels.Add("watergun", new ModelData
			{
				Model = content.Load<Model>("Models\\watergun"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "WatergunJoint",
					Translation = new Vector3(1.6f, 0.236f, -0.184f),
					Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
				}
			});
			Instance.AllModels.Add("watergunTank", new ModelData
			{
				Model = content.Load<Model>("Models\\watergunTank"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(-0.656f, -2.598f, 1.076f),
					Rotation = new Vector3(-96.85f, -92.126f, -8.031f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "WatergunTankJoint",
					Translation = new Vector3(1.6f, 0.236f, -0.184f),
					Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
				}
			});
			Instance.AllModels.Add("bow", new ModelData
			{
				Model = content.Load<Model>("Models\\bow"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0.236f, -2.3f, -3.176f),
					Rotation = new Vector3(65.669f, 62.835f, 24.094f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "BowJoint",
					Translation = new Vector3(0.341f, 0.184f, -0.026f),
					Rotation = new Vector3(-55.276f, 180f, 75.118f)
				}
			});
			Instance.AllModels.Add("tablet", new ModelData
			{
				Model = content.Load<Model>("Models\\tablet"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0.236f, -2.3f, -3.176f),
					Rotation = new Vector3(65.669f, 62.835f, 24.094f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "TabletJoint",
					Translation = new Vector3(-29.764f, 15.591f, 8.031f),
					Rotation = new Vector3(1.601f, 0.709f, 0.026f)
				}
			});
			return Advance();
		case 23:
			Instance.AllModels.Add("armsling", new ModelData
			{
				Model = content.Load<Model>("Models\\armsling"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0f, 0f, 0f),
					Rotation = new Vector3(180f, 0f, 90f)
				}
			});
			return Advance();
		case 24:
			Instance.AllModels.Add("rifle", new ModelData
			{
				Model = content.Load<Model>("Models\\rifle"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeSpineD",
					Translation = new Vector3(0f, -2.3f, 0f),
					Rotation = new Vector3(110f, 0f, 70f)
				},
				RightHandAttachee = new AttachPoint
				{
					BoneName = "RifleJoint",
					Translation = new Vector3(1.6f, 0.236f, -0.184f),
					Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
				}
			});
			return Advance();
		case 25:
			Instance.AllModels.Add("bush", new ModelData
			{
				Model = content.Load<Model>("Models\\bushbackPlant"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				BackAttachee = new AttachPoint
				{
					BoneName = "AttacheeRoot"
				}
			});
			return Advance();
		case 26:
			Instance.AllModels.Add("meshtest", new ModelData
			{
				Model = content.Load<Model>("Models\\meshtest"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 27:
			Instance.AllModels.Add("skinnedtest", new ModelData
			{
				Model = content.Load<Model>("Models\\skinnedtest_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 28:
			ExamineModelsAndSetProperties();
			return Advance();
		case 29:
			Instance.BillboardSpriteSheet = content.Load<ExtendedSpriteSheet>("BuildingsAndTrees");
			return Advance();
		case 30:
			// PORT DEVIATION 16. Loaded as the base SpriteSheet, which is what 1.0.4.8's own code
			// does. Asking for LightSourceSpriteSheet threw on 1.0.4.8 content with
			// "Unable to cast object of type 'SpriteSheet' to type 'LightSourceSpriteSheet'",
			// because the XNB now names the base type. Requesting the base accepts either the
			// new asset or an older one - a derived instance assigns to a base field - so this
			// one line is what makes the port version-tolerant here.
			Instance.LightSourcesSpriteSheet = game.Content.Load<SpriteSheet>("LightSources");
			return Advance();
		case 31:
			Instance.AllModels.Add("demonTree", new ModelData
			{
				Model = content.Load<Model>("Models\\demonTree_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 32:
			Instance.AllModels.Add("spikePlant", new ModelData
			{
				Model = content.Load<Model>("Models\\spikePlant_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 33:
			Instance.AllModels.Add("worm", new ModelData
			{
				Model = content.Load<Model>("Models\\worm_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 34:
			Instance.AllModels.Add("snatcher", new ModelData
			{
				Model = content.Load<Model>("Models\\snatcher_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 35:
			Instance.AllModels.Add("farmingHoe", new ModelData
			{
				Model = content.Load<Model>("Models\\farmingHoe"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Translation = new Vector3(0.394f, -2.861f, -0.079f),
					Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
				}
			});
			Instance.AllModels.Add("shovel", new ModelData
			{
				Model = content.Load<Model>("Models\\shovel"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 2f,
				RightHandAttachee = new AttachPoint
				{
					BoneName = "AttacheeFingerRightA",
					Translation = new Vector3(0.394f, -2.861f, -0.079f),
					Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
				}
			});
			return Advance();
		case 36:
			Instance.AllModels.Add("demonTreeMoss", new ModelData
			{
				Model = content.Load<Model>("Models\\demonTreeMoss_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 37:
			Instance.AllModels.Add("snatcherArmored", new ModelData
			{
				Model = content.Load<Model>("Models\\snatcherArmored_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 38:
			Instance.AllModels.Add("thunderChickenBulky", new ModelData
			{
				Model = content.Load<Model>("Models\\thunderChickenBulky_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 39:
			Instance.AllModels.Add("thunderChickenThin", new ModelData
			{
				Model = content.Load<Model>("Models\\thunderChickenThin_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 40:
			Instance.AllModels.Add("twinklerThin", new ModelData
			{
				Model = content.Load<Model>("Models\\twinklerThin_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 41:
			Instance.AllModels.Add("twinklerThinSpiky", new ModelData
			{
				Model = content.Load<Model>("Models\\twinklerThinSpiky_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 42:
			Instance.AllModels.Add("dog", new ModelData
			{
				Model = content.Load<Model>("Models\\dog_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 43:
			Instance.AllModels.Add("robotLight", new ModelData
			{
				Model = content.Load<Model>("Models\\robotLight_idle"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 6f,
				CRTDisplayLightIntensity = 5f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				RightHandAttachor = new AttachPoint
				{
					Tag = "rightHand",
					IsHand = true,
					BoneName = "robotArm_R_end_jnt"
				}
			});
			return Advance();
		case 44:
			Instance.AllModels.Add("robotHeavy", new ModelData
			{
				Model = content.Load<Model>("Models\\robotHeavy_idle"),
				ModelType = ModelType.Stiff,
				CRTDisplayScale = 6f,
				CRTDisplayLightIntensity = 5f,
				ModelOffset = new Vector3(0f, 0f, 0f),
				LeftHandAttachor = new AttachPoint
				{
					Tag = "leftHand",
					IsHand = true,
					BoneName = "robotArm_L_end_jnt"
				},
				RightHandAttachor = new AttachPoint
				{
					Tag = "rightHand",
					IsHand = true,
					BoneName = "robotArm_R_end_jnt"
				},
				BackAttachor = new AttachPoint
				{
					Tag = "back",
					BoneName = "robotBody_jnt"
				}
			});
			return Advance();
		case 45:
			Instance.AllModels.Add("wormThin", new ModelData
			{
				Model = content.Load<Model>("Models\\wormThin_idle"),
				ModelType = ModelType.Skinned,
				CRTDisplayScale = 2f,
				ModelOffset = new Vector3(0f, 0f, 0f)
			});
			return Advance();
		case 46:
			foreach (KeyValuePair<string, SoundData> allSoundDatum in AllSoundData)
			{
				allSoundDatum.Value.LoadContent(content);
			}
			foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
			{
				allEntityType.Value.LoadContent(content);
			}
			foreach (KeyValuePair<string, ActionSets> allActionSet in AllActionSets)
			{
				allActionSet.Value.LoadContent(content);
			}
			foreach (KeyValuePair<string, PolledEventType> allPolledEvent in AllPolledEvents)
			{
				allPolledEvent.Value.LoadContent(content);
			}
			foreach (KeyValuePair<string, ParticleSystemType> allParticleSystem in AllParticleSystems)
			{
				allParticleSystem.Value.LoadContent(content);
			}
			return true;
		default:
			state++;
			return false;
		}
	}

	public static void ResolveEntityTypeTags(ref List<EntityType> resultList, string[] tags, string[] types, Dictionary<string, List<EntityType>> tagCollection)
	{
		if (tags != null)
		{
			string[] array = tags;
			foreach (string key in array)
			{
				Common.AddRangeToList(ref resultList, tagCollection[key]);
			}
		}
		if (types != null)
		{
			string[] array = types;
			foreach (string key2 in array)
			{
				EntityType value = Instance.AllEntityTypes[key2];
				Common.AddToList(ref resultList, value);
			}
		}
		if (resultList != null)
		{
			resultList = resultList.Distinct().ToList();
		}
	}

	public static void ResolveEntityTypeTags(ref HashSet<EntityType> resultList, string[] tags, string[] types, Dictionary<string, List<EntityType>> tagCollection)
	{
		if (tags != null)
		{
			string[] array = tags;
			foreach (string key in array)
			{
				Common.AddRangeToSet(ref resultList, tagCollection[key]);
			}
		}
		if (types != null)
		{
			string[] array = types;
			foreach (string key2 in array)
			{
				EntityType value = Instance.AllEntityTypes[key2];
				Common.AddToSet(ref resultList, value);
			}
		}
	}

	public void Initialize()
	{
		allPostLoadContentValidationErrors = new Dictionary<string, List<string>>();
		Init2DAnims();
		SetupEventHooks();
		SetupOtherEvents();
		PostLoadContentInitialize();
		CreateProcessGraph();
		ValidateProcessTypeGraph();
		CreateRepairProcesses(allPostLoadContentValidationErrors);
		CreateSpecialActionProcesses();
		MarkAnchorStructures();
		List<string> duplicateKeyErrors = null;
		ExtractNestedGameData(ref duplicateKeyErrors);
		DataLoader.DisplayValidationErrors(duplicateKeyErrors);
		OverrideEntityTypeDescriptions();
		InitContainerTransactHints();
		InitOtherLists();
		InitMetaConstants();
		InitDamageScoreTables();
		InitWeaponEffectiveness();
		DataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);
	}

	private void CreateSpecialActionProcesses()
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			allEntityType.Value.CreateSpecialProcesses();
		}
	}

	private void CreateRepairProcesses(Dictionary<string, List<string>> errors)
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			if (allEntityType.Value.NonLivingType != null && allEntityType.Value.NonLivingType.EntityRepairProfile != null)
			{
				List<string> list = new List<string>();
				allPostLoadContentValidationErrors.Add("EntityTypes/" + allEntityType.Key, list);
				allEntityType.Value.NonLivingType.EntityRepairProfile.GenerateProcesses(allEntityType.Value, list);
			}
		}
	}

	private void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
	{
		foreach (KeyValuePair<string, ActionSets> allActionSet in AllActionSets)
		{
			allActionSet.Value.ExtractNestedGameData(ref duplicateKeyErrors);
		}
		foreach (KeyValuePair<string, PolledEventType> allPolledEvent in AllPolledEvents)
		{
			if (allPolledEvent.Value.ActionSetsKey == null)
			{
				allPolledEvent.Value.ActionSets.ExtractNestedGameData(ref duplicateKeyErrors);
			}
		}
		foreach (KeyValuePair<string, AllegianceEventType> allAllegianceEventType in AllAllegianceEventTypes)
		{
			allAllegianceEventType.Value.ActionSets.ExtractNestedGameData(ref duplicateKeyErrors);
		}
	}

	private void OverrideEntityTypeDescriptions()
	{
		foreach (KeyValuePair<string, EntityTypeDescription> allEntityTypeDescription in AllEntityTypeDescriptions)
		{
			if (AllEntityTypes.TryGetValue(allEntityTypeDescription.Value.EntityType, out var value))
			{
				allEntityTypeDescription.Value.ApplyDescriptions(value);
			}
		}
	}

	public static string CreateKeyName()
	{
		return Guid.NewGuid().ToString();
	}

	private void Init2DAnims()
	{
		Animation2Ds.Add("campfireFast", new Animation2D(BillboardSpriteSheet.Texture, 0.1f, isLooping: true)
		{
			Origin = new Vector2(16f, 43f),
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_10"))
			}
		});
		Animation2Ds.Add("campfireSmall", new Animation2D(BillboardSpriteSheet.Texture, 0.1f, isLooping: true)
		{
			Origin = new Vector2(16f, 48f),
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_10"))
			}
		});
		Animation2Ds.Add("fishCircling", new Animation2D(BillboardSpriteSheet.Texture, 0.075f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_39"))
			}
		});
		Animation2Ds.Add("fishSwarming", new Animation2D(BillboardSpriteSheet.Texture, 0.075f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_39")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_40"))
			}
		});
		Animation2Ds.Add("butterfliesSwarm", new Animation2D(BillboardSpriteSheet.Texture, 0.04f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_39")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_40")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_41")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_42")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_43")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_44")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_45")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_46")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_47")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_48")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_49")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_50")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_51")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_52")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_53")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_54")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_55")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_56")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_57")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_58"))
			}
		});
		Animation2Ds.Add("mosquitoSwarming", new Animation2D(BillboardSpriteSheet.Texture, 0.04f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_01")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_02")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_03")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_04")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_05")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_06")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_07")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_08")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_09")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_39")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_40")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_41")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_42")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_43")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_44")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_45")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_46")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_47")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_48")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_49")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_50")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_51")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_52")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_53")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_54")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_55")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_56")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_57")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_58"))
			}
		});
		Animation2Ds.Add("groundBugsSwarm", new Animation2D(BillboardSpriteSheet.Texture, 0.05f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_1")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_2")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_3")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_4")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_5")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_6")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_7")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_8")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_9")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_39")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_40")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_41")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_42")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_43")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_44")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_45")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_46")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_47")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_48")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_49")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_50"))
			}
		});
		Animation2Ds.Add("dragonflies", new Animation2D(BillboardSpriteSheet.Texture, 0.03f, isLooping: true)
		{
			Cells = new List<Cell>
			{
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_1")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_2")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_3")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_4")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_5")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_6")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_7")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_8")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_9")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_10")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_11")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_12")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_13")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_14")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_15")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_16")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_17")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_18")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_19")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_20")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_21")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_22")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_23")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_24")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_25")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_26")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_27")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_28")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_29")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_30")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_31")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_32")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_33")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_34")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_35")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_36")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_37")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_38")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_39")),
				new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_40"))
			}
		});
	}

	public static void InitializeComputerGeneratedData(IGameData data)
	{
		data.Initialize();
		data.PostDataCompleteInitialize();
	}

	private void PostLoadContentInitialize()
	{
		List<string> list = null;
		foreach (KeyValuePair<string, DefaultStorageSettings> allDefaultStorageSetting in AllDefaultStorageSettings)
		{
			allDefaultStorageSetting.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			allEntityType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, ParticleSystemType> allParticleSystem in AllParticleSystems)
		{
			allParticleSystem.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, BodyType> allBodyType in AllBodyTypes)
		{
			allBodyType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, AttackType> allAttackType in AllAttackTypes)
		{
			allAttackType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, ProcessType> allProcessType in AllProcessTypes)
		{
			allProcessType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, EffectProfileType> allEffectProfileType in AllEffectProfileTypes)
		{
			allEffectProfileType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, SoilComponentType> allSoilComponentType in AllSoilComponentTypes)
		{
			allSoilComponentType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, LowVegetationType> allLowVegetationType in AllLowVegetationTypes)
		{
			allLowVegetationType.Value.PostLoadContentInitialize();
		}
		foreach (KeyValuePair<string, DetectionType> allDetectionType in AllDetectionTypes)
		{
			list = new List<string>();
			allPostLoadContentValidationErrors.Add("DetectionTypes/" + allDetectionType.Key, list);
			allDetectionType.Value.PostLoadContentInitialize(ref list);
		}
		foreach (KeyValuePair<string, ResourceType> allResourceType in AllResourceTypes)
		{
			allResourceType.Value.PostLoadContentInitialize();
		}
	}

	private void SetupOtherEvents()
	{
		AllegianceEvents.Clear();
		foreach (KeyValuePair<string, AllegianceEventType> allAllegianceEventType in AllAllegianceEventTypes)
		{
			Common.AddToMultiList(AllegianceEvents, allAllegianceEventType.Value.Event, allAllegianceEventType.Value.ActionSets);
		}
	}

	private void SetupEventHooks()
	{
		SetupEventHooksList(AllAgentActionHooks, ref AgentActionHooksByEntityType, AllEntityTypes);
		SetupEventHooksList(AllEntityEventHooks, ref EntityEventHooksByEntityType, AllEntityTypes);
		SetupEventHooksList(AllAttackTypeEventHooks, ref EventHooksByAttackType, AllAttackTypes);
		SetupEventHooksList(AllProcessTypeEventHooks, ref EventHooksByProcessType, AllProcessTypes);
		SetupEventHooksList(AllEffectTypeEventHooks, ref EventHooksByEffectType, AllEffectProfileTypes);
		SetupEventHooksList(AllDetectedEntityEventHooks, ref DetectedEntityHooksByEntityType, AllEntityTypes);
		SetupEventHooksList(AllDetectedResourceEventHooks, ref DetectedResourceHooksByEntityType, AllEntityTypes);
		SetupEventHooksList(AllEntityPolledEvents, ref EntityPolledEventByEntityType, AllEntityTypes);
	}

	private void SetupEventHooksList<T, U>(Dictionary<string, U> allEventHooks, ref Dictionary<T, List<U>> eventHooksByType, Dictionary<string, T> allTypes) where U : IHook
	{
		eventHooksByType = new Dictionary<T, List<U>>();
		foreach (KeyValuePair<string, U> allEventHook in allEventHooks)
		{
			T key = allTypes[allEventHook.Value.TypeKey];
			if (!eventHooksByType.TryGetValue(key, out var value))
			{
				value = new List<U>();
				eventHooksByType.Add(key, value);
			}
			value.Add(allEventHook.Value);
			value = value.OrderBy((U h) => h.ExecutionOrder).ToList();
		}
	}

	private void InitContainerTransactHints()
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			if (allEntityType.Value.ContainerType != null)
			{
				allEntityType.Value.ContainerType.SetVerminCanAccess();
			}
		}
	}

	private void InitOtherLists()
	{
		foreach (KeyValuePair<string, EntityCategory> allEntityCategory in Instance.AllEntityCategories)
		{
			List<EntityType> list = new List<EntityType>();
			Instance.ItemTypesInCategory.Add(allEntityCategory.Value, list);
			foreach (KeyValuePair<string, EntityType> allItemType in Instance.AllItemTypes)
			{
				if (allItemType.Value.Category == allEntityCategory.Value)
				{
					list.Add(allItemType.Value);
				}
			}
		}
	}

	private void InitMetaConstants()
	{
		try
		{
			AttackType attackType = AllEntityTypes["entity:human"].IntelligenceType.AttackTypes.First((AttackType a) => a.DamageFinal == instance.AllDamageTypes["blunt"] && a.KeyName.ToLowerInvariant().Contains("punch"));
			OneOverMeanDamageFromHumanPunch = 1f / attackType.DamageMean;
		}
		catch (Exception)
		{
			throw new Exception("Missing punch attack for person.");
		}
	}

	private void InitDamageScoreTables()
	{
		foreach (KeyValuePair<string, AttackType> allAttackType in AllAttackTypes)
		{
			_ = allAttackType.Key == "shootImprovedFireExtinguisherBushDragonPoison";
			Dictionary<BodyPartType, float> dictionary = new Dictionary<BodyPartType, float>();
			AttackScoresAgainstBodyParts.Add(allAttackType.Value, dictionary);
			Dictionary<BodyType, float> dictionary2 = new Dictionary<BodyType, float>();
			MeanAttackDamageAgainstEnemyTypes.Add(allAttackType.Value, dictionary2);
			foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
			{
				BodyType bodyType = allEntityType.Value.BodyType;
				if (bodyType == null)
				{
					continue;
				}
				Common.AddToMultiList(bodyToEntityMappings, bodyType, allEntityType.Value);
				if (!dictionary2.ContainsKey(bodyType))
				{
					float value = allEntityType.Value.GetMeanHitpoints().Value;
					BodyPartType[] bodyPartTypes = bodyType.BodyPartTypes;
					foreach (BodyPartType bodypart in bodyPartTypes)
					{
						SaveDamageScore(bodyType, value, allAttackType.Value, dictionary, bodypart, dictionary2);
					}
				}
			}
		}
	}

	private void InitWeaponEffectiveness()
	{
		Dictionary<BodyType, float> value = null;
		Dictionary<EntityType, EntityType> dictionary = new Dictionary<EntityType, EntityType>();
		foreach (KeyValuePair<string, EntityType> allItemType in AllItemTypes)
		{
			if (allItemType.Value.ItemType.WeaponType == null)
			{
				continue;
			}
			dictionary.Clear();
			if (allItemType.Value.ItemType.WeaponType.AttackTypes != null)
			{
				AttackType[] attackTypes = allItemType.Value.ItemType.WeaponType.AttackTypes;
				foreach (AttackType key in attackTypes)
				{
					if (!MeanAttackDamageAgainstEnemyTypes.TryGetValue(key, out value))
					{
						continue;
					}
					foreach (KeyValuePair<BodyType, float> item in value)
					{
						foreach (EntityType item2 in bodyToEntityMappings[item.Key])
						{
							if (!dictionary.ContainsKey(item2))
							{
								float value2 = item2.GetMeanHitpoints().Value;
								if (item.Value > 0.3f * value2)
								{
									dictionary.Add(item2, item2);
								}
							}
						}
					}
				}
			}
			List<EntityType> list = dictionary.Keys.OrderBy((EntityType k) => k.Name).ToList();
			StringBuilder stringBuilder = new StringBuilder();
			string value3 = "";
			foreach (EntityType item3 in list)
			{
				stringBuilder.Append(value3);
				stringBuilder.Append(item3.Name);
				value3 = ", ";
			}
			allItemType.Value.ItemType.WeaponType.HighlyEffectiveAgainst = stringBuilder.ToString();
		}
		bodyToEntityMappings.Clear();
	}

	private void SaveDamageScore(BodyType bodyType, float bodyHitpoints, AttackType attackType, Dictionary<BodyPartType, float> scores, BodyPartType bodypart, Dictionary<BodyType, float> meanDamages)
	{
		float meanDamage;
		float num = AttackJob.ComputeEstimatedDamageScore(attackType, bodypart, bodyHitpoints, out meanDamage);
		if (meanDamages.TryGetValue(bodyType, out var value))
		{
			if (meanDamage > value)
			{
				meanDamages[bodyType] = meanDamage;
			}
		}
		else
		{
			meanDamages[bodyType] = meanDamage;
		}
		if (num > 0f)
		{
			scores.Add(bodypart, num);
		}
		if (bodypart.BodyPartTypes != null)
		{
			BodyPartType[] bodyPartTypes = bodypart.BodyPartTypes;
			foreach (BodyPartType bodypart2 in bodyPartTypes)
			{
				SaveDamageScore(bodyType, bodyHitpoints, attackType, scores, bodypart2, meanDamages);
			}
		}
	}

	private void CreateProcessGraph()
	{
		foreach (KeyValuePair<string, ProcessType> allProcessType in AllProcessTypes)
		{
			if (!allProcessType.Value.IsOriginalSpecialAction)
			{
				AddProcessToProductionGraph(allProcessType.Value);
			}
		}
	}

	public void AddProcessToProductionGraph(ProcessType process)
	{
		if (process.IsPartOfProductionChain())
		{
			if (process.Outputs != null)
			{
				Output[] outputs = process.Outputs;
				foreach (Output output in outputs)
				{
					if (!output.IsWasteProduct)
					{
						Common.AddToMultiList(ProcessYieldsThisOutput, output.FinalEntityTypeToCreate, process);
					}
				}
			}
			if (process.InputsByType != null)
			{
				foreach (KeyValuePair<EntityType, Input> item in process.InputsByType)
				{
					Common.AddToMultiList(ProcessesUsingThisInput, item.Key, process);
				}
			}
			if (process.ProcessToolSet != null)
			{
				ToolAlternatives[] tools = process.ProcessToolSet.Tools;
				for (int i = 0; i < tools.Length; i++)
				{
					foreach (Tuple<EntityType, float> item2 in tools[i].ToolsAndProductivity)
					{
						Common.AddToMultiList(ToolsUsedFor, item2.Item1, process);
					}
				}
			}
		}
		else if (process.IsSalvageProcess && process.Outputs != null)
		{
			Output[] outputs = process.Outputs;
			foreach (Output output2 in outputs)
			{
				if (!output2.IsWasteProduct)
				{
					Common.AddToMultiList(SalvageProcessYieldsThisOutput, output2.FinalEntityTypeToCreate, process);
				}
			}
		}
		if (process.IsPartOfAttainableCalculation())
		{
			AllProductionProcesses.Add(process);
			if (!process.IsSalvageProcess)
			{
				NonSalvageProductionProcesses.Add(process);
			}
		}
	}

	private void MarkAnchorStructures()
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			allEntityType.Value.MarkAnchorStructures();
		}
	}

	private void ValidateProcessTypeGraph()
	{
		foreach (KeyValuePair<string, ProcessType> allProcessType in Instance.AllProcessTypes)
		{
			List<string> list = new List<string>();
			allPostLoadContentValidationErrors.Add("ValidateProcessTypeGraph/" + allProcessType.Value.KeyName, list);
			allProcessType.Value.PostProcessGraphValidate(list);
		}
	}

	public static void UnloadAllData()
	{
		instance = null;
		// The tables are gone, so nothing is built from any configuration any more. Clearing this
		// is what lets the next game be started with different mod switches.
		UWGame.Mods.ModSettings.MarkDataUnloaded();
	}

	public void PostDataCompleteInitialize()
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		List<KeyValuePair<Type, IGameDataCollection>> list = AllGameDataCollections.OrderBy((KeyValuePair<Type, IGameDataCollection> k) => k.Value.Order).ToList();
		foreach (KeyValuePair<Type, IGameDataCollection> item in list)
		{
			item.Value.PreDataCompleteValidate(dictionary);
		}
		DataLoader.DisplayAllValidationErrors(dictionary);
		dictionary.Clear();
		foreach (KeyValuePair<Type, IGameDataCollection> item2 in list)
		{
			item2.Value.PostDataCompleteInitialize();
		}
		GUIConstants.PostDataCompleteInitialize();
		Constants.PostDataCompleteInitialize();
		AIConstants.PostDataCompleteInitialize();
		foreach (KeyValuePair<Type, IGameDataCollection> item3 in list)
		{
			item3.Value.PostDataCompleteValidate(dictionary);
		}
		DataLoader.DisplayAllValidationErrors(dictionary);
		List<TierType> source = AllTierTypes.Values.ToList();
		Tiers = source.OrderBy((TierType t) => t.UpperEdge).ToArray();
		int num = 0;
		TierType[] tiers = Tiers;
		for (int num2 = 0; num2 < tiers.Length; num2++)
		{
			tiers[num2].Index = num;
			num++;
		}
		// The tables are now what this session will run with, so record the mod configuration they
		// were built from. A save is compared against THIS rather than against what is currently
		// switched on: changing a switch changes the next build, not the tables in memory.
		UWGame.Mods.ModSettings.MarkDataBuilt();
	}

	private static void ExamineModelsAndSetProperties()
	{
		foreach (KeyValuePair<string, ModelData> allModel in Instance.AllModels)
		{
			foreach (ModelBone bone in allModel.Value.Model.Bones)
			{
				if (bone.Name == "jet_engine_joint")
				{
					allModel.Value.HasAircraftDucts = true;
				}
				else if (bone.Name == "wheel_front_right_joint")
				{
					allModel.Value.HasSteerableFrontWheels = true;
				}
			}
			foreach (ModelMesh mesh in allModel.Value.Model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					if (meshPart.Effect.Parameters["EmissiveColor"].GetValueVector3() != Vector3.Zero)
					{
						allModel.Value.HasEmittingParts = true;
						break;
					}
				}
			}
			BoundingSphere boundingSphere = (BoundingSphere)(((Dictionary<string, object>)allModel.Value.Model.Tag) ?? throw new InvalidOperationException("Model.Tag is not set correctly. Make sure your model was built using the custom TrianglePickingProcessor."))["BoundingSphere"];
			allModel.Value.BoundingSphereRadius = boundingSphere.Radius;
		}
	}

	public void PostLoadContentValidate()
	{
		allPostLoadContentValidationErrors = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, EntityType> allEntityType in AllEntityTypes)
		{
			List<string> listOfErrors = new List<string>();
			allPostLoadContentValidationErrors.Add(allEntityType.Value.KeyName, listOfErrors);
			allEntityType.Value.PostLoadContentValidate(ref listOfErrors);
		}
		foreach (KeyValuePair<string, ActionSets> allActionSet in AllActionSets)
		{
			List<string> listOfErrors = new List<string>();
			allPostLoadContentValidationErrors.Add(allActionSet.Value.KeyName, listOfErrors);
			allActionSet.Value.PostLoadContentValidate(listOfErrors);
		}
		foreach (KeyValuePair<string, EventActionType> allEventActionType in AllEventActionTypes)
		{
			List<string> listOfErrors = new List<string>();
			allPostLoadContentValidationErrors.Add(allEventActionType.Value.KeyName, listOfErrors);
			allEventActionType.Value.PostLoadContentValidate(ref listOfErrors);
		}
		DataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);
	}

	public static BitArray CreateBitArrayFromTags(Dictionary<string, int> allTags, string[] theseTags)
	{
		BitArray bitArray = new BitArray(allTags.Count);
		if (theseTags != null)
		{
			foreach (string key in theseTags)
			{
				bitArray[allTags[key]] = true;
			}
		}
		return bitArray;
	}

	public IGameData GetGameData(Type type, string keyName)
	{
		if (AllGameDataCollections.TryGetValue(type, out var value))
		{
			return value.Get(keyName);
		}
		throw new Exception("Missing GameData type in mappings");
	}
}
