using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.Client.Audio;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.AllGameData.EventHooks;
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
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Soil;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.AllGameData;

public abstract class DataLoader
{
	private DataLoaderQueueState queueState;

	private Dictionary<string, List<string>> allPreInitValidationErrors;

	private Dictionary<string, List<string>> allPostInitValidationErrors;

	private Config.DataType dataSource;

	private string folderName = "";

	private float progressFactor;

	public string FolderName
	{
		get
		{
			return folderName;
		}
		set
		{
			folderName = value;
		}
	}

	public DataLoader(Config.DataType dataSource, float progressFactor)
	{
		this.dataSource = dataSource;
		this.progressFactor = progressFactor;
	}

	public bool QueueInitGameData(Scenario scenario)
	{
		_ = Sim.CurrentSerializeMode;
		_ = 1;
		bool flag = scenario?.ScenarioData.EnableMissions ?? true;
		switch (queueState)
		{
		case DataLoaderQueueState.Begin:
			if (allPreInitValidationErrors == null)
			{
				allPreInitValidationErrors = new Dictionary<string, List<string>>();
			}
			if (allPostInitValidationErrors == null)
			{
				allPostInitValidationErrors = new Dictionary<string, List<string>>();
			}
			UpdateProgress(DataLoaderQueueState.AIConstants, 16);
			break;
		case DataLoaderQueueState.AIConstants:
			HandleDataTypeObject(InitAIConstants, ref GameData.Instance.AIConstants, "AIConstants.xml");
			UpdateProgress(DataLoaderQueueState.Constants, 16);
			break;
		case DataLoaderQueueState.Constants:
			HandleDataTypeObject(InitGameConstants, ref GameData.Instance.Constants, "constants.xml");
			UpdateProgress(DataLoaderQueueState.GUIConstants, 15);
			break;
		case DataLoaderQueueState.GUIConstants:
			HandleDataTypeObject(InitGUIConstants, ref GameData.Instance.GUIConstants, "GUIConstants.xml");
			UpdateProgress(DataLoaderQueueState.Sounds, 16);
			break;
		case DataLoaderQueueState.Sounds:
			HandleDataTypeList(InitSounds, GameData.Instance.AllSoundData, "sounds.xml");
			UpdateProgress(DataLoaderQueueState.SkillCategories, 0);
			break;
		case DataLoaderQueueState.SkillCategories:
			HandleDataTypeList(InitSkillCategories, GameData.Instance.AllSkillCategories, "skillCategories.xml");
			UpdateProgress(DataLoaderQueueState.SkillTypes, 0);
			break;
		case DataLoaderQueueState.SkillTypes:
			HandleDataTypeList(InitSkillTypes, GameData.Instance.AllSkillTypes, "skillTypes.xml");
			UpdateProgress(DataLoaderQueueState.ProfessionTypes, 15);
			break;
		case DataLoaderQueueState.ProfessionTypes:
			HandleDataTypeList(InitProfessionTypes, GameData.Instance.AllProfessionTypes, "professionTypes.xml");
			UpdateProgress(DataLoaderQueueState.FoodNutrientTypes, 0);
			break;
		case DataLoaderQueueState.FoodNutrientTypes:
			HandleDataTypeList(InitNutrientTypes, GameData.Instance.AllFoodNutrientTypes, "foodNutrientTypes.xml");
			UpdateProgress(DataLoaderQueueState.DefaultStorageSettings, 0);
			break;
		case DataLoaderQueueState.DefaultStorageSettings:
			HandleDataTypeList(InitDefaultStorageSettings, GameData.Instance.AllDefaultStorageSettings, "defaultStorageSettings.xml");
			UpdateProgress(DataLoaderQueueState.EntityCategories, 0);
			break;
		case DataLoaderQueueState.EntityCategories:
			HandleDataTypeList(InitEntityCategories, GameData.Instance.AllEntityCategories, "itemCategories.xml");
			UpdateProgress(DataLoaderQueueState.StorageConditions, 0);
			break;
		case DataLoaderQueueState.StorageConditions:
			HandleDataTypeList(InitStorageConditions, GameData.Instance.AllStorageConditions, "storageConditions.xml");
			UpdateProgress(DataLoaderQueueState.DegradeProfiles, 32);
			break;
		case DataLoaderQueueState.DegradeProfiles:
			HandleDataTypeList(InitDegradeTypes, GameData.Instance.AllDegradeTypes, "degradeTypes.xml");
			UpdateProgress(DataLoaderQueueState.FoodNutrientProfiles, 15);
			break;
		case DataLoaderQueueState.FoodNutrientProfiles:
			HandleDataTypeList(InitFoodNutrientProfiles, GameData.Instance.AllFoodNutrientProfiles, "foodNutrientProfiles.xml");
			UpdateProgress(DataLoaderQueueState.SoilTypes, 0);
			break;
		case DataLoaderQueueState.SoilTypes:
			HandleDataTypeList(InitSoilComponentTypes, GameData.Instance.AllSoilComponentTypes, "soilTypes.xml");
			UpdateProgress(DataLoaderQueueState.LowVegetationTypes, 16);
			break;
		case DataLoaderQueueState.LowVegetationTypes:
			HandleDataTypeList(InitLowVegetationTypes, GameData.Instance.AllLowVegetationTypes, "lowVegetationTypes.xml");
			UpdateProgress(DataLoaderQueueState.BodyLayerTypes, 0);
			break;
		case DataLoaderQueueState.BodyLayerTypes:
			HandleDataTypeList(InitBodyLayerTypes, GameData.Instance.AllBodyLayerTypes, "bodyLayerTypes.xml");
			UpdateProgress(DataLoaderQueueState.BodyTypes, 31);
			break;
		case DataLoaderQueueState.BodyTypes:
			HandleDataTypeList(InitBodyTypes, GameData.Instance.AllBodyTypes, "bodyTypes.xml");
			UpdateProgress(DataLoaderQueueState.StanceTypes, 0);
			break;
		case DataLoaderQueueState.StanceTypes:
			HandleDataTypeList(InitStanceTypes, GameData.Instance.AllStanceTypes, "stanceTypes.xml");
			UpdateProgress(DataLoaderQueueState.StancesTypes, 16);
			break;
		case DataLoaderQueueState.StancesTypes:
			HandleDataTypeList(InitStancesTypes, GameData.Instance.AllStancesTypes, "stancesTypes.xml");
			UpdateProgress(DataLoaderQueueState.ActionSets, 109);
			break;
		case DataLoaderQueueState.ActionSets:
			HandleDataTypeList(InitActionSets, GameData.Instance.AllActionSets, "actionSets.xml");
			UpdateProgress(DataLoaderQueueState.EventActionTypes, 0);
			break;
		case DataLoaderQueueState.EventActionTypes:
			HandleDataTypeList(InitEventActionTypes, GameData.Instance.AllEventActionTypes, "eventActionTypes.xml");
			UpdateProgress(DataLoaderQueueState.EntityData, 16);
			break;
		case DataLoaderQueueState.EntityData:
			HandleDataTypeList(InitEntityData, GameData.Instance.AllEntityData, "entityData.xml");
			UpdateProgress(DataLoaderQueueState.SiteData, 16);
			break;
		case DataLoaderQueueState.SiteData:
			HandleDataTypeList(InitSiteData, GameData.Instance.AllSiteData, "siteData.xml");
			UpdateProgress(DataLoaderQueueState.AllegianceData, 16);
			break;
		case DataLoaderQueueState.AllegianceData:
			HandleDataTypeList(InitAllegianceData, GameData.Instance.AllAllegianceData, "allegianceData.xml");
			UpdateProgress(DataLoaderQueueState.AllegianceTemplates, 16);
			break;
		case DataLoaderQueueState.AllegianceTemplates:
			HandleDataTypeList(InitAllegianceTemplates, GameData.Instance.AllAllegianceTemplates, "allegianceTemplates.xml");
			UpdateProgress(DataLoaderQueueState.SiteTemplates, 16);
			break;
		case DataLoaderQueueState.SiteTemplates:
			HandleDataTypeList(InitSiteTemplates, GameData.Instance.AllSiteTemplates, "siteTemplates.xml");
			UpdateProgress(DataLoaderQueueState.ExpeditionData, 16);
			break;
		case DataLoaderQueueState.ExpeditionData:
			if (flag)
			{
				HandleDataTypeList(InitExpeditionData, GameData.Instance.AllExpeditionData, "expeditionData.xml");
			}
			UpdateProgress(DataLoaderQueueState.TradeProfiles, 16);
			break;
		case DataLoaderQueueState.TradeProfiles:
			if (flag)
			{
				HandleDataTypeList(InitTradeProfiles, GameData.Instance.AllTradeProfiles, "tradeProfiles.xml");
			}
			UpdateProgress(DataLoaderQueueState.TradeGroups, 16);
			break;
		case DataLoaderQueueState.TradeGroups:
			if (flag)
			{
				HandleDataTypeList(InitTradeGroups, GameData.Instance.AllTradeGroups, "tradeGroups.xml");
			}
			UpdateProgress(DataLoaderQueueState.PricesProfiles, 16);
			break;
		case DataLoaderQueueState.PricesProfiles:
			if (flag)
			{
				HandleDataTypeList(InitPricesProfiles, GameData.Instance.AllPricesProfiles, "pricesProfiles.xml");
			}
			UpdateProgress(DataLoaderQueueState.OfferDemandProfiles, 16);
			break;
		case DataLoaderQueueState.OfferDemandProfiles:
			if (flag)
			{
				HandleDataTypeList(InitOfferDemandProfiles, GameData.Instance.AllOfferDemandProfiles, "offerDemandProfiles.xml");
			}
			UpdateProgress(DataLoaderQueueState.VehicleProfiles, 16);
			break;
		case DataLoaderQueueState.VehicleProfiles:
			HandleDataTypeList(InitVehicleProfiles, GameData.Instance.AllVehiclesProfiles, "vehicleProfiles.xml");
			UpdateProgress(DataLoaderQueueState.StructureProfiles, 16);
			break;
		case DataLoaderQueueState.StructureProfiles:
			HandleDataTypeList(InitStructureProfiles, GameData.Instance.AllStructuresProfiles, "structureProfiles.xml");
			UpdateProgress(DataLoaderQueueState.EventHooks, 16);
			break;
		case DataLoaderQueueState.EventHooks:
			HandleDataTypeList(InitEntityEventHooks, GameData.Instance.AllEntityEventHooks, "entityEventHooks.xml");
			HandleDataTypeList(InitAgentActionHooks, GameData.Instance.AllAgentActionHooks, "agentActionHooks.xml");
			HandleDataTypeList(InitAttackTypeEventHooks, GameData.Instance.AllAttackTypeEventHooks, "attackTypeEventHooks.xml");
			HandleDataTypeList(InitProcessTypeEventHooks, GameData.Instance.AllProcessTypeEventHooks, "processTypeEventHooks.xml");
			HandleDataTypeList(InitEffectTypeEventHooks, GameData.Instance.AllEffectTypeEventHooks, "effectTypeEventHooks.xml");
			HandleDataTypeList(InitAttackTypeEventHooks, GameData.Instance.AllAttackTypeEventHooks, "attackTypeEventHooks.xml");
			HandleDataTypeList(InitDetectEntityTypeHooks, GameData.Instance.AllDetectedEntityEventHooks, "detectedEntityEventHooks.xml");
			HandleDataTypeList(InitDetectResourceTypeHooks, GameData.Instance.AllDetectedResourceEventHooks, "detectedResourceEventHooks.xml");
			HandleDataTypeList(InitEntityPolledEvents, GameData.Instance.AllEntityPolledEvents, "entityPolledEvents.xml");
			UpdateProgress(DataLoaderQueueState.AttackTypes, 31);
			break;
		case DataLoaderQueueState.AttackTypes:
			HandleDataTypeList(InitAttackTypes, GameData.Instance.AllAttackTypes, "attackTypes.xml");
			UpdateProgress(DataLoaderQueueState.DamageTypes, 16);
			break;
		case DataLoaderQueueState.DamageTypes:
			HandleDataTypeList(InitDamageTypes, GameData.Instance.AllDamageTypes, "damageTypes.xml");
			UpdateProgress(DataLoaderQueueState.HelpTopics, 0);
			break;
		case DataLoaderQueueState.HelpTopics:
			HandleDataTypeList(InitHelpTopics, GameData.Instance.AllHelpTopics, "helpTopics.xml");
			HandleDataTypeList(InitTutorialTopics, GameData.Instance.AllTutorialTopics, "tutorialTopics.xml");
			UpdateProgress(DataLoaderQueueState.ResourceCategories, 0);
			break;
		case DataLoaderQueueState.ResourceCategories:
			HandleDataTypeList(InitResourceCategories, GameData.Instance.AllResourceCategories, "resourceCategories.xml");
			UpdateProgress(DataLoaderQueueState.Substances, 0);
			break;
		case DataLoaderQueueState.Substances:
			HandleDataTypeList(InitSubstanceTypes, GameData.Instance.AllSubstanceTypes, "substances.xml");
			UpdateProgress(DataLoaderQueueState.IconInfo, 15);
			break;
		case DataLoaderQueueState.IconInfo:
			HandleDataTypeList(InitIconInfo, GameData.Instance.AllIconInfo, "iconInfo.xml");
			UpdateProgress(DataLoaderQueueState.ResourceTypes, 32);
			break;
		case DataLoaderQueueState.ResourceTypes:
			HandleDataTypeList(InitResourceTypes, GameData.Instance.AllResourceTypes, "resourceTypes.xml");
			UpdateProgress(DataLoaderQueueState.TriggerTypes, 0);
			break;
		case DataLoaderQueueState.TriggerTypes:
			HandleDataTypeList(InitTriggerTypes, GameData.Instance.AllTriggerTypes, "triggerTypes.xml");
			UpdateProgress(DataLoaderQueueState.Personalities, 15);
			break;
		case DataLoaderQueueState.Personalities:
			HandleDataTypeList(InitPersonalityTypes, GameData.Instance.AllPersonalityTypes, "personalityTypes.xml");
			UpdateProgress(DataLoaderQueueState.Traits, 0);
			break;
		case DataLoaderQueueState.Traits:
			HandleDataTypeList(InitTraitTemplates, GameData.Instance.AllTraitTemplates, "traitTemplates.xml");
			UpdateProgress(DataLoaderQueueState.Cultures, 0);
			break;
		case DataLoaderQueueState.Cultures:
			HandleDataTypeList(InitCultureTemplates, GameData.Instance.AllCultureTemplates, "cultureTemplates.xml");
			UpdateProgress(DataLoaderQueueState.Tiers, 0);
			break;
		case DataLoaderQueueState.Tiers:
			HandleDataTypeList(InitTiers, GameData.Instance.AllTierTypes, "tiers.xml");
			UpdateProgress(DataLoaderQueueState.TierAreas, 0);
			break;
		case DataLoaderQueueState.TierAreas:
			HandleDataTypeList(InitTierAreas, GameData.Instance.AllTierAreas, "tierAreas.xml");
			UpdateProgress(DataLoaderQueueState.UpgradeCategories, 0);
			break;
		case DataLoaderQueueState.UpgradeCategories:
			HandleDataTypeList(InitUpgradeCategories, GameData.Instance.AllUpgradeCategories, "upgradeCategories.xml");
			UpdateProgress(DataLoaderQueueState.UpgradeProfiles, 0);
			break;
		case DataLoaderQueueState.UpgradeProfiles:
			HandleDataTypeList(InitUpgradeProfiles, GameData.Instance.AllUpgradeProfiles, "upgradeProfiles.xml");
			UpdateProgress(DataLoaderQueueState.RepairProfiles, 0);
			break;
		case DataLoaderQueueState.RepairProfiles:
			HandleDataTypeList(InitRepairProfiles, GameData.Instance.AllRepairProfiles, "repairProfiles.xml");
			UpdateProgress(DataLoaderQueueState.BioOrders, 0);
			break;
		case DataLoaderQueueState.BioOrders:
			HandleDataTypeList(InitBioOrderTypes, GameData.Instance.AllBioOrderTypes, "bioOrderTypes.xml");
			UpdateProgress(DataLoaderQueueState.AllegianceEvents, 0);
			break;
		case DataLoaderQueueState.AllegianceEvents:
			HandleDataTypeList(InitAllegianceEvents, GameData.Instance.AllAllegianceEventTypes, "allegianceEventTypes.xml");
			UpdateProgress(DataLoaderQueueState.EntityTypes, 813);
			break;
		case DataLoaderQueueState.EntityTypes:
			HandleDataTypeList(InitEntityTypes, GameData.Instance.AllEntityTypes, "entityTypes.xml");
			UpdateProgress(DataLoaderQueueState.EntityTypeDescriptions, 0);
			break;
		case DataLoaderQueueState.EntityTypeDescriptions:
			HandleDataTypeList(InitEntityTypeDescriptions, GameData.Instance.AllEntityTypeDescriptions, "entityTypeDescriptions.xml");
			UpdateProgress(DataLoaderQueueState.DetectionTypes, 15);
			break;
		case DataLoaderQueueState.DetectionTypes:
			HandleDataTypeList(InitDetectionTypes, GameData.Instance.AllDetectionTypes, "detectionTypes.xml");
			UpdateProgress(DataLoaderQueueState.FilterSettingTypes, 0);
			break;
		case DataLoaderQueueState.FilterSettingTypes:
			HandleDataTypeList(InitFilterSettingTypes, GameData.Instance.AllFilterSettingTypes, "filterSettings.xml");
			UpdateProgress(DataLoaderQueueState.JobTypes, 47);
			break;
		case DataLoaderQueueState.JobTypes:
			HandleDataTypeList(InitJobTypes, GameData.Instance.AllJobTypes, "jobTypes.xml");
			UpdateProgress(DataLoaderQueueState.PresentationTypes, 47);
			break;
		case DataLoaderQueueState.PresentationTypes:
			HandleDataTypeList(InitPresentationTypes, GameData.Instance.AllPresentationTypes, "presentationTypes.xml");
			UpdateProgress(DataLoaderQueueState.PresentationTypeCategories, 47);
			break;
		case DataLoaderQueueState.PresentationTypeCategories:
			HandleDataTypeList(InitPresentationTypeCategories, GameData.Instance.AllPresentationTypeCategories, "presentationTypeCategories.xml");
			UpdateProgress(DataLoaderQueueState.PolledEventTypes, 47);
			break;
		case DataLoaderQueueState.PolledEventTypes:
			HandleDataTypeList(InitGlobalConditionalEvents, GameData.Instance.AllPolledEvents, "globalEvents.xml");
			UpdateProgress(DataLoaderQueueState.ProcessToolSets, 47);
			break;
		case DataLoaderQueueState.ProcessToolSets:
			HandleDataTypeList(InitProcessToolSets, GameData.Instance.AllProcessToolSets, "processToolSets.xml");
			UpdateProgress(DataLoaderQueueState.ProcessTypes, 406);
			break;
		case DataLoaderQueueState.ProcessTypes:
			HandleDataTypeList(InitProcessTypes, GameData.Instance.AllProcessTypes, "processTypes.xml");
			UpdateProgress(DataLoaderQueueState.EffectTypes, 16);
			break;
		case DataLoaderQueueState.EffectTypes:
			HandleDataTypeList(InitEffectTypes, GameData.Instance.AllEffectTypes, "effectTypes.xml");
			UpdateProgress(DataLoaderQueueState.EffectProfileTypes, 16);
			break;
		case DataLoaderQueueState.EffectProfileTypes:
			HandleDataTypeList(InitEffectProfileTypes, GameData.Instance.AllEffectProfileTypes, "effectProfiles.xml");
			UpdateProgress(DataLoaderQueueState.Particles, 16);
			break;
		case DataLoaderQueueState.Particles:
			HandleDataTypeList(InitParticleSystems, GameData.Instance.AllParticleSystems, "particleSystems.xml");
			UpdateProgress(DataLoaderQueueState.GUI, 15);
			break;
		case DataLoaderQueueState.GUI:
			HandleDataTypeObject(InitStatusIconPresentation, ref GameData.Instance.CustomStatusIconData, "entityStatusIcons.xml");
			GameData.Instance.CustomStatusIconData.Initialize();
			HandleDataTypeObject(InitSidePanelPresentation, ref GameData.Instance.CustomSidePanelData, "sidePanelPresentation.xml");
			GameData.Instance.CustomSidePanelData.Initialize();
			HandleDataTypeObject(InitOtherSiteSidePanelPresentation, ref GameData.Instance.CustomOtherSiteSidePanelData, "otherSiteSidePanelPresentation.xml");
			GameData.Instance.CustomOtherSiteSidePanelData.Initialize();
			HandleDataTypeObject(InitActivityPresentation, ref GameData.Instance.CustomEntityActivityData, "entityActivityIcons.xml");
			GameData.Instance.CustomEntityActivityData.Initialize();
			UpdateProgress(DataLoaderQueueState.AttachableRenderableTypes, 0);
			break;
		case DataLoaderQueueState.AttachableRenderableTypes:
			HandleDataTypeList(InitAttachableRenderableTypes, GameData.Instance.AttachableRenderableTypes, "attachableRenderableTypes.xml");
			queueState = DataLoaderQueueState.DONE;
			break;
		case DataLoaderQueueState.DONE:
			DisplayAllValidationErrors(allPostInitValidationErrors);
			queueState = DataLoaderQueueState.Begin;
			allPreInitValidationErrors.Clear();
			allPostInitValidationErrors.Clear();
			return true;
		}
		return false;
	}

	private void UpdateProgress(DataLoaderQueueState nextStep, int lengthOfNextStepInMilliseconds)
	{
		if (nextStep < queueState)
		{
			throw new Exception("Sequence error!");
		}
		// PORT DEVIATION 13 (see PORTING-NOTES.md). Was an unguarded
		// `The.LoadScreen.Progress(...)`. The null check is what lets the data tables be loaded
		// with no UI attached, which tools/DataExport relies on to run the export headlessly -
		// progress reporting is the data layer's only dependency on the load screen, and it is
		// inherently optional. Behaviour is unchanged whenever a load screen exists, i.e. in
		// every path the game itself takes.
		The.LoadScreen?.Progress(nextStep.ToString(), (int)((float)lengthOfNextStepInMilliseconds * progressFactor));
		queueState = nextStep;
	}

	public static List<T> HandleOtherDataList<T>(Func<List<T>> Init, string xmlFileName)
	{
		List<T> list = null;
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
		{
			list = Init();
		}
		SerializeAndDeserializeOtherTypeList(list, "", xmlFileName);
		return list;
	}

	private List<T> HandleDataTypeList<T>(Func<List<T>> Init, Dictionary<string, T> finalDictionary, string xmlFileName) where T : IGameData
	{
		List<T> list = null;
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
		{
			list = Init();
		}
		SerializeAndDeserializeTypeList(list, finalDictionary, FolderName, xmlFileName);
		return list;
	}

	private void HandleDataTypeObject<T>(Func<T> Init, ref T finalObject, string xmlFileName) where T : IGameDataObject
	{
		T objectToSerialize = default(T);
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
		{
			objectToSerialize = Init();
		}
		SerializeAndDeserializeGameDataObject(objectToSerialize, ref finalObject, FolderName, xmlFileName, dataSource);
	}

	public static void DeserializeTypeList<T>(string folderPath, string filePath, out List<T> listToSerialize, Config.DataType dataType)
	{
		listToSerialize = null;
		if (Directory.Exists(folderPath))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));
			using TextReader textReader = new StreamReader(filePath);
			listToSerialize = (List<T>)xmlSerializer.Deserialize(textReader);
		}
	}

	private void InitTypeList<T>(List<T> listOfNewItems, Dictionary<string, T> finalTypeDictionary, string fileDescriptor) where T : IGameData
	{
		foreach (T listOfNewItem in listOfNewItems)
		{
			if (listOfNewItem.DeleteRecord)
			{
				finalTypeDictionary.Remove(listOfNewItem.KeyName);
			}
			else if (!finalTypeDictionary.ContainsKey(listOfNewItem.KeyName))
			{
				finalTypeDictionary.Add(listOfNewItem.KeyName, listOfNewItem);
			}
			else
			{
				finalTypeDictionary[listOfNewItem.KeyName] = listOfNewItem;
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (T listOfNewItem2 in listOfNewItems)
		{
			if (!listOfNewItem2.DeleteRecord)
			{
				List<string> listOfErrors = null;
				if (hashSet.Contains(listOfNewItem2.KeyName))
				{
					allPreInitValidationErrors.Remove(fileDescriptor + "/" + listOfNewItem2.KeyName);
					EntityType.CreateValidationError(ref listOfErrors, string.Concat(listOfNewItem2, " is a duplicate key."));
				}
				else
				{
					hashSet.Add(listOfNewItem2.KeyName);
					listOfNewItem2.PreInitValidate(ref listOfErrors);
				}
				if (listOfErrors != null)
				{
					allPreInitValidationErrors.Add(fileDescriptor + "/" + listOfNewItem2.KeyName, listOfErrors);
				}
			}
		}
		DisplayAllValidationErrors(allPreInitValidationErrors);
		foreach (T listOfNewItem3 in listOfNewItems)
		{
			if (!listOfNewItem3.DeleteRecord)
			{
				listOfNewItem3.Initialize();
			}
		}
		foreach (T listOfNewItem4 in listOfNewItems)
		{
			List<string> listOfErrors = null;
			if (!listOfNewItem4.DeleteRecord)
			{
				listOfNewItem4.PostInitValidate(ref listOfErrors);
				if (listOfErrors != null)
				{
					allPostInitValidationErrors.Add(fileDescriptor + "/" + listOfNewItem4.KeyName, listOfErrors);
				}
			}
		}
	}

	public static void AddToTagCollection<T>(T entityType, string[] tags, Dictionary<string, List<T>> tagCollection)
	{
		if (tags == null)
		{
			return;
		}
		foreach (string key in tags)
		{
			if (!tagCollection.TryGetValue(key, out var value))
			{
				value = new List<T>();
				tagCollection.Add(key, value);
			}
			value.Add(entityType);
		}
	}

	public static void AddToTagCollection<T>(T entityType, string tag, Dictionary<string, List<T>> tagCollection)
	{
		if (tag != null)
		{
			if (!tagCollection.TryGetValue(tag, out var value))
			{
				value = new List<T>();
				tagCollection.Add(tag, value);
			}
			value.Add(entityType);
		}
	}

	public static void DisplayAllValidationErrors(Dictionary<string, List<string>> allErrors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, List<string>> allError in allErrors)
		{
			if (allError.Value.Count <= 0)
			{
				continue;
			}
			stringBuilder.Append(allError.Key);
			stringBuilder.AppendLine(":");
			foreach (string item in allError.Value)
			{
				stringBuilder.AppendLine(item);
			}
		}
		DisplayErrors(stringBuilder);
	}

	public static void DisplayValidationErrors(List<string> errorList)
	{
		if (errorList == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string error in errorList)
		{
			stringBuilder.AppendLine(error);
		}
		DisplayErrors(stringBuilder);
	}

	private static void DisplayErrors(StringBuilder errors)
	{
		if (errors.ToString() != "")
		{
			throw new Exception("Errors were found during validation: " + errors.ToString());
		}
	}

	private static void SerializeAndDeserializeOtherTypeList<T>(List<T> listToSerialize, string folderName, string xmlFileName)
	{
		Config.DataType dataType = Config.DataType.BaseData;
		if (Sim.CurrentSerializeMode == Sim.SerializeMode.NoSerialize)
		{
			return;
		}
		string dataFolderPath = Config.GetDataFolderPath(dataType, folderName);
		string dataFolderPath2 = Config.GetDataFolderPath(dataType, folderName, xmlFileName);
		if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));
			if (!Directory.Exists(dataFolderPath))
			{
				Directory.CreateDirectory(dataFolderPath);
			}
			using TextWriter textWriter = new StreamWriter(dataFolderPath2);
			xmlSerializer.Serialize(textWriter, listToSerialize);
		}
		DeserializeTypeList(dataFolderPath, dataFolderPath2, out listToSerialize, dataType);
	}

	private void SerializeAndDeserializeTypeList<T>(List<T> listToSerialize, Dictionary<string, T> finalTypeDictionary, string folderName, string xmlFileName) where T : IGameData
	{
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
		{
			string dataFolderPath = Config.GetDataFolderPath(dataSource, folderName);
			string dataFolderPath2 = Config.GetDataFolderPath(dataSource, folderName, xmlFileName);
			if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));
				if (!Directory.Exists(dataFolderPath))
				{
					Directory.CreateDirectory(dataFolderPath);
				}
				using TextWriter textWriter = new StreamWriter(dataFolderPath2);
				xmlSerializer.Serialize(textWriter, listToSerialize);
			}
			DeserializeTypeList(dataFolderPath, dataFolderPath2, out listToSerialize, dataSource);
		}
		if (finalTypeDictionary != null && listToSerialize != null)
		{
			InitTypeList(listToSerialize, finalTypeDictionary, dataSource.ToString() + "/" + xmlFileName);
		}
	}

	public static void SerializeAndDeserializeGameDataObject<T>(T objectToSerialize, ref T finalDataObject, string folderName, string xmlFileName, Config.DataType dataType) where T : IGameDataObject
	{
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
		{
			if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
			{
				SerializeObject(objectToSerialize, folderName, xmlFileName, dataType);
			}
			DeserializeObject<T>(folderName, xmlFileName, out objectToSerialize, dataType);
		}
		if (objectToSerialize != null)
		{
			finalDataObject = objectToSerialize;
			finalDataObject.Initialize();
		}
	}

	public static void SerializeAndDeserializeObject<T>(T objectToSerialize, ref T finalDataObject, string folderName, string xmlFileName, Config.DataType dataType)
	{
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
		{
			if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
			{
				SerializeObject(objectToSerialize, folderName, xmlFileName, dataType);
			}
			DeserializeObject<T>(folderName, xmlFileName, out objectToSerialize, dataType);
		}
		if (objectToSerialize != null)
		{
			finalDataObject = objectToSerialize;
		}
	}

	public static void SerializeObject<T>(T objectToSerialize, string folderName, string xmlFileName, Config.DataType dataType)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		string dataFolderPath = Config.GetDataFolderPath(dataType, folderName);
		if (!Directory.Exists(dataFolderPath))
		{
			Directory.CreateDirectory(dataFolderPath);
		}
		using TextWriter textWriter = new StreamWriter(Config.GetDataFolderPath(dataType, folderName, xmlFileName));
		xmlSerializer.Serialize(textWriter, objectToSerialize);
	}

	public static void DeserializeObject<T>(string folderName, string xmlFileName, out T objectToSerialize, Config.DataType dataType)
	{
		string filePath;
		if (!string.IsNullOrEmpty(xmlFileName))
		{
			filePath = Config.GetDataFolderPath(dataType, folderName, xmlFileName);
		}
		else
		{
			string[] files = Directory.GetFiles(Config.GetDataFolderPath(dataType, folderName));
			objectToSerialize = default(T);
			filePath = files[0];
		}
		DeserializeObject<T>(filePath, out objectToSerialize);
	}

	public static void DeserializeObject<T>(string filePath, out T objectToSerialize)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using TextReader textReader = new StreamReader(filePath);
		objectToSerialize = (T)xmlSerializer.Deserialize(textReader);
	}

	private void MoveListToDictionary(List<IGameData> list, Dictionary<string, IGameData> dictionary)
	{
		foreach (IGameData item in list)
		{
			dictionary.Add(item.KeyName, item);
		}
	}

	private static bool DoReplaceOnType(Type t)
	{
		if (!(t == typeof(string)) && t.IsClass && !t.IsAbstract)
		{
			return !(t == typeof(CustomXmlSerializer.XmlProxyData));
		}
		return false;
	}

	public static void ReplaceEntityTypePlaceholdersOnObjectCollection<T>() where T : IEnumerable
	{
	}

	private static bool IsCollection(object o)
	{
		if (!typeof(ICollection).IsAssignableFrom(o.GetType()))
		{
			return typeof(ICollection<>).IsAssignableFrom(o.GetType());
		}
		return true;
	}

	public static List<CustomXmlSerializer.XmlTypeMappingBase> GetListOfTypeMappings(bool useEntityTypePlaceholders = false)
	{
		return new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			EntityType.GetEntityTypePropertySerializer(useEntityTypePlaceholders),
			new CustomXmlSerializer.XmlTypeMapping<EntityCategory, string>
			{
				GetterMethod = (EntityCategory t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllEntityCategories[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<ResourceCategory, string>
			{
				GetterMethod = (ResourceCategory t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllResourceCategories[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<ResourceType, string>
			{
				GetterMethod = (ResourceType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllResourceTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<ResourceType[], string[]>
			{
				GetterMethod = SerializeResourceTypeArray,
				SetterMethod = DeserializeResourceTypeArray
			},
			new CustomXmlSerializer.XmlTypeMapping<AttackType[], string[]>
			{
				GetterMethod = SerializeAttackTypeArray,
				SetterMethod = DeserializeAttackTypeArray
			},
			new CustomXmlSerializer.XmlTypeMapping<BodyType, string>
			{
				GetterMethod = (BodyType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllBodyTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<DetectionType, string>
			{
				GetterMethod = (DetectionType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllDetectionTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<Vector3?, string>
			{
				GetterMethod = (Vector3? t) => t.HasValue ? PersonType.Vector3ToHexString(t.Value) : null,
				SetterMethod = (string s) => (s != null) ? new Vector3?(PersonType.HexStringToVector3(s)) : ((Vector3?)null)
			},
			new CustomXmlSerializer.XmlTypeMapping<Type, string>
			{
				GetterMethod = (Type t) => (!(t == null)) ? ConvertTypeToString(t) : null,
				SetterMethod = (string s) => (s != null) ? ConvertStringToType(s) : null
			},
			new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, int>, KVP<string, int>[]>
			{
				GetterMethod = SerializeEntityTypeIntDictionary,
				SetterMethod = DeserializeEntityTypeIntDictionary
			},
			new CustomXmlSerializer.XmlTypeMapping<ProcessType, string>
			{
				GetterMethod = (ProcessType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllProcessTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<SoundData, string>
			{
				GetterMethod = (SoundData t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllSoundData[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<FoodNutrientProfile, string>
			{
				GetterMethod = (FoodNutrientProfile t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllFoodNutrientProfiles[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<FoodNutrientType, string>
			{
				GetterMethod = (FoodNutrientType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllFoodNutrientTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<ProcessToolSet, string>
			{
				GetterMethod = (ProcessToolSet t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllProcessToolSets[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<PresentationType, string>
			{
				GetterMethod = (PresentationType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllPresentationTypes[s] : null
			},
			new CustomXmlSerializer.XmlTypeMapping<EntityType[], string[]>
			{
				GetterMethod = SerializeEntityTypeArray,
				SetterMethod = DeserializeEntityTypeArray
			},
			new CustomXmlSerializer.XmlTypeMapping<BodyPartType[], string[]>
			{
				GetterMethod = SerializeBodyPartTypes,
				SetterMethod = DeserializeBodyPartTypes
			},
			new CustomXmlSerializer.XmlTypeMapping<SkillType, string>
			{
				GetterMethod = (SkillType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllSkillTypes[s] : null
			},
			GetMaterialInputSerializer(useEntityTypePlaceholders)
		};
	}

	public static CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]> GetMaterialInputSerializer(bool useEntityPlaceholders)
	{
		if (useEntityPlaceholders)
		{
			return new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]>
			{
				GetterMethod = delegate(Dictionary<EntityType, Input> t)
				{
					if (t == null)
					{
						return (Input[])null;
					}
					Input[] array = new Input[t.Count];
					int num = 0;
					foreach (KeyValuePair<EntityType, Input> item in t)
					{
						array[num] = item.Value;
						num++;
					}
					return array;
				},
				SetterMethod = delegate(Input[] s)
				{
					if (s == null)
					{
						return (Dictionary<EntityType, Input>)null;
					}
					Dictionary<EntityType, Input> dictionary = new Dictionary<EntityType, Input>();
					foreach (Input input in s)
					{
						dictionary[new EntityType(input.EntityType.KeyName)] = input;
					}
					return dictionary;
				}
			};
		}
		return new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]>
		{
			GetterMethod = delegate(Dictionary<EntityType, Input> t)
			{
				if (t == null)
				{
					return (Input[])null;
				}
				Input[] array = new Input[t.Count];
				int num = 0;
				foreach (KeyValuePair<EntityType, Input> item2 in t)
				{
					array[num] = item2.Value;
					num++;
				}
				return array;
			},
			SetterMethod = delegate(Input[] s)
			{
				if (s == null)
				{
					return (Dictionary<EntityType, Input>)null;
				}
				Dictionary<EntityType, Input> dictionary = new Dictionary<EntityType, Input>();
				foreach (Input input in s)
				{
					dictionary[input.EntityType] = input;
				}
				return dictionary;
			}
		};
	}

	private static string ConvertTypeToString(Type type)
	{
		if (type == typeof(AnimModifier))
		{
			return "AnimModifier";
		}
		if (type == typeof(StateModifier))
		{
			return "SpriteModifier";
		}
		return null;
	}

	private static Type ConvertStringToType(string s)
	{
		if (s == "AnimModifier")
		{
			return typeof(AnimModifier);
		}
		if (s == "SpriteModifier")
		{
			return typeof(StateModifier);
		}
		return null;
	}

	public static KVP<string, int>[] SerializeEntityTypeIntDictionary(Dictionary<EntityType, int> t)
	{
		if (t == null)
		{
			return null;
		}
		KVP<string, int>[] array = new KVP<string, int>[t.Count];
		int num = 0;
		foreach (KeyValuePair<EntityType, int> item in t)
		{
			array[num] = new KVP<string, int>(item.Key.KeyName, item.Value);
			num++;
		}
		return array;
	}

	public static Dictionary<EntityType, int> DeserializeEntityTypeIntDictionary(KVP<string, int>[] s)
	{
		if (s == null)
		{
			return null;
		}
		Dictionary<EntityType, int> dictionary = new Dictionary<EntityType, int>();
		foreach (KVP<string, int> kVP in s)
		{
			dictionary[new EntityType
			{
				KeyName = kVP.Key
			}] = kVP.Value;
		}
		return dictionary;
	}

	public static string[] SerializeEntityTypeArray(EntityType[] t)
	{
		if (t == null)
		{
			return null;
		}
		string[] array = new string[t.Length];
		int num = 0;
		foreach (EntityType entityType in t)
		{
			array[num] = entityType.KeyName;
			num++;
		}
		return array;
	}

	public static EntityType[] DeserializeEntityTypeArray(string[] s)
	{
		if (s == null)
		{
			return null;
		}
		EntityType[] array = new EntityType[s.Length];
		int num = 0;
		foreach (string key in s)
		{
			array[num] = GameData.Instance.AllEntityTypes[key];
			num++;
		}
		return array;
	}

	public static string[] SerializeResourceTypeArray(ResourceType[] t)
	{
		if (t == null)
		{
			return null;
		}
		string[] array = new string[t.Length];
		int num = 0;
		foreach (ResourceType resourceType in t)
		{
			array[num] = resourceType.KeyName;
			num++;
		}
		return array;
	}

	public static ResourceType[] DeserializeResourceTypeArray(string[] s)
	{
		if (s == null)
		{
			return null;
		}
		ResourceType[] array = new ResourceType[s.Length];
		int num = 0;
		foreach (string key in s)
		{
			array[num] = GameData.Instance.AllResourceTypes[key];
			num++;
		}
		return array;
	}

	public static string[] SerializeAttackTypeArray(AttackType[] t)
	{
		if (t == null)
		{
			return null;
		}
		string[] array = new string[t.Length];
		int num = 0;
		foreach (AttackType attackType in t)
		{
			array[num] = attackType.KeyName;
			num++;
		}
		return array;
	}

	public static AttackType[] DeserializeAttackTypeArray(string[] s)
	{
		if (s == null)
		{
			return null;
		}
		AttackType[] array = new AttackType[s.Length];
		int num = 0;
		foreach (string key in s)
		{
			array[num] = GameData.Instance.AllAttackTypes[key];
			num++;
		}
		return array;
	}

	public static string[] SerializeBodyPartTypes(BodyPartType[] bodyPartTypes)
	{
		if (bodyPartTypes == null)
		{
			return null;
		}
		string[] array = new string[bodyPartTypes.Length];
		int num = 0;
		foreach (BodyPartType bodyPartType in bodyPartTypes)
		{
			array[num] = bodyPartType.BodyKeyName + "_" + bodyPartType.Name;
			num++;
		}
		return array;
	}

	public static BodyPartType[] DeserializeBodyPartTypes(string[] names)
	{
		if (names == null)
		{
			return null;
		}
		BodyPartType[] array = new BodyPartType[names.Length];
		int num = 0;
		foreach (string obj in names)
		{
			int num2 = obj.IndexOf("_");
			string key = obj.Substring(0, num2);
			string name = obj.Substring(num2 + 1);
			BodyType bodyType = GameData.Instance.AllBodyTypes[key];
			array[num] = bodyType.FindBodyPart(name);
			num++;
		}
		return array;
	}

	protected virtual List<SkillCategory> InitSkillCategories()
	{
		return null;
	}

	protected virtual List<SoundData> InitSounds()
	{
		return null;
	}

	protected virtual AIConstants InitAIConstants()
	{
		return null;
	}

	protected virtual UWGame.SimSide.Constants InitGameConstants()
	{
		return null;
	}

	protected virtual GUIConstants InitGUIConstants()
	{
		return null;
	}

	protected virtual CustomDataPresentation InitStatusIconPresentation()
	{
		return null;
	}

	protected virtual CustomDataPresentation InitActivityPresentation()
	{
		return null;
	}

	protected virtual CustomDataPresentation InitSidePanelPresentation()
	{
		return null;
	}

	protected virtual CustomDataPresentation InitOtherSiteSidePanelPresentation()
	{
		return null;
	}

	protected virtual List<EntityEventHook> InitEntityEventHooks()
	{
		return EventHooksLoader.InitEntityEventHooks();
	}

	protected virtual List<AgentActionHook> InitAgentActionHooks()
	{
		return EventHooksLoader.InitAgentActionHooks();
	}

	protected virtual List<AttackTypeActionHook> InitAttackTypeEventHooks()
	{
		return EventHooksLoader.InitAttackTypeHooks();
	}

	protected virtual List<ProcessTypeActionHook> InitProcessTypeEventHooks()
	{
		return EventHooksLoader.InitProcessTypeHooks();
	}

	protected virtual List<EffectTypeActionHook> InitEffectTypeEventHooks()
	{
		return EventHooksLoader.InitEffectTypeHooks();
	}

	protected virtual List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return null;
	}

	protected virtual List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return null;
	}

	protected virtual List<EntityTypePolledEvent> InitEntityPolledEvents()
	{
		return EntityPolledEventsLoader.Init();
	}

	protected virtual List<EventActionType> InitEventActionTypes()
	{
		return null;
	}

	protected virtual List<EntityData> InitEntityData()
	{
		return null;
	}

	protected virtual List<SiteData> InitSiteData()
	{
		return null;
	}

	protected virtual List<AllegianceData> InitAllegianceData()
	{
		return null;
	}

	protected virtual List<ExpeditionData> InitExpeditionData()
	{
		return null;
	}

	protected virtual List<SiteTemplate> InitSiteTemplates()
	{
		return null;
	}

	protected virtual List<AllegianceTemplate> InitAllegianceTemplates()
	{
		return null;
	}

	protected virtual List<TradeProfile> InitTradeProfiles()
	{
		return null;
	}

	protected virtual List<PricesProfile> InitPricesProfiles()
	{
		return null;
	}

	protected virtual List<TradeGroup> InitTradeGroups()
	{
		return null;
	}

	protected virtual List<OfferDemandProfile> InitOfferDemandProfiles()
	{
		return null;
	}

	protected virtual List<VehiclesProfile> InitVehicleProfiles()
	{
		return null;
	}

	protected virtual List<StructuresProfile> InitStructureProfiles()
	{
		return null;
	}

	protected virtual List<ActionSets> InitActionSets()
	{
		return null;
	}

	protected virtual List<StancesType> InitStancesTypes()
	{
		return null;
	}

	protected virtual List<StanceType> InitStanceTypes()
	{
		return null;
	}

	protected virtual List<BodyLayerType> InitBodyLayerTypes()
	{
		return null;
	}

	protected virtual List<BodyType> InitBodyTypes()
	{
		return null;
	}

	protected virtual List<LowVegetationType> InitLowVegetationTypes()
	{
		return null;
	}

	protected virtual List<SoilComponentType> InitSoilComponentTypes()
	{
		return null;
	}

	protected virtual List<FoodNutrientProfile> InitFoodNutrientProfiles()
	{
		return null;
	}

	protected virtual List<StorageCondition> InitStorageConditions()
	{
		return null;
	}

	protected virtual List<DegradeType> InitDegradeTypes()
	{
		return null;
	}

	protected virtual List<EntityCategory> InitEntityCategories()
	{
		return null;
	}

	protected virtual List<DefaultStorageSettings> InitDefaultStorageSettings()
	{
		return null;
	}

	protected virtual List<SkillType> InitSkillTypes()
	{
		return null;
	}

	protected virtual List<ProfessionType> InitProfessionTypes()
	{
		return null;
	}

	protected virtual List<FoodNutrientType> InitNutrientTypes()
	{
		return null;
	}

	protected virtual List<HelpTopic> InitTutorialTopics()
	{
		return null;
	}

	protected virtual List<HelpTopic> InitHelpTopics()
	{
		return null;
	}

	protected virtual List<ResourceCategory> InitResourceCategories()
	{
		return null;
	}

	protected virtual List<DamageType> InitDamageTypes()
	{
		return null;
	}

	protected virtual List<AttackType> InitAttackTypes()
	{
		return null;
	}

	protected virtual List<ResourceType> InitResourceTypes()
	{
		return null;
	}

	protected virtual List<TriggerType> InitTriggerTypes()
	{
		return null;
	}

	protected virtual List<PersonalityType> InitPersonalityTypes()
	{
		return null;
	}

	protected virtual List<RepairProfile> InitRepairProfiles()
	{
		return null;
	}

	protected virtual List<TraitTemplate> InitTraitTemplates()
	{
		return null;
	}

	protected virtual List<CultureTemplate> InitCultureTemplates()
	{
		return null;
	}

	protected virtual List<TierType> InitTiers()
	{
		return null;
	}

	protected virtual List<TierArea> InitTierAreas()
	{
		return null;
	}

	protected virtual List<UpgradeCategory> InitUpgradeCategories()
	{
		return null;
	}

	protected virtual List<UpgradeProfile> InitUpgradeProfiles()
	{
		return null;
	}

	protected virtual List<BioOrderType> InitBioOrderTypes()
	{
		return null;
	}

	protected virtual List<AllegianceEventType> InitAllegianceEvents()
	{
		return null;
	}

	protected virtual List<RenderableType> InitAttachableRenderableTypes()
	{
		return null;
	}

	protected virtual List<EntityType> InitEntityTypes()
	{
		return null;
	}

	protected virtual List<EntityTypeDescription> InitEntityTypeDescriptions()
	{
		return null;
	}

	protected virtual List<FilterSettingType> InitFilterSettingTypes()
	{
		return null;
	}

	protected virtual List<JobType> InitJobTypes()
	{
		return null;
	}

	protected virtual List<PresentationType> InitPresentationTypes()
	{
		return null;
	}

	protected virtual List<PresentationTypeCategory> InitPresentationTypeCategories()
	{
		return null;
	}

	protected virtual List<PolledEventType> InitGlobalConditionalEvents()
	{
		return null;
	}

	protected virtual List<ProcessToolSet> InitProcessToolSets()
	{
		return null;
	}

	protected virtual List<ProcessType> InitProcessTypes()
	{
		return null;
	}

	protected virtual List<EffectType> InitEffectTypes()
	{
		return null;
	}

	protected virtual List<EffectProfileType> InitEffectProfileTypes()
	{
		return null;
	}

	protected virtual List<DetectionType> InitDetectionTypes()
	{
		return null;
	}

	protected virtual List<ParticleSystemType> InitParticleSystems()
	{
		return null;
	}

	protected virtual List<SubstanceType> InitSubstanceTypes()
	{
		return null;
	}

	protected virtual List<IconInfo> InitIconInfo()
	{
		return null;
	}
}
