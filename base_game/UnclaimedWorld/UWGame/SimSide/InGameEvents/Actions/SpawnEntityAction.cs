using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnEntityAction : EventActionType
{
	public EntityData EntityData;

	public bool LogAsProduction;

	public bool SuppressSpawnEvents;

	public EvalNode EntityType;

	public AllegianceAndExpedition OwnedBy;

	public EvalNode EntityDataKey;

	public string ActingOnEntityName;

	public TargetObject ActingOnEntityObject;

	public string ProductionProcessToUse;

	public EvalNode Site;

	public DynamicLocation DynamicLocation;

	public ContainerLocation AddToContainer;

	[XmlIgnore]
	private string lastSpawnInfo;

	public EvalNode Amount;

	public SpawnEntityAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnEntityAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		EntityData entityData = null;
		if (EntityData != null)
		{
			entityData = EntityData;
		}
		else if (EntityDataKey != null)
		{
			PropertyResult? propertyResult = EntityDataKey.Evaluate(action);
			if (propertyResult.HasValue)
			{
				string stringResult = propertyResult.Value.StringResult;
				entityData = GameData.Instance.AllEntityData[stringResult];
			}
		}
		string text;
		if (EntityType != null)
		{
			PropertyResult? propertyResult2 = EntityType.Evaluate(action);
			if (!propertyResult2.HasValue)
			{
				lastSpawnInfo = ComposeInfoString(null, null, null);
				failReason = "Failed to find entity type key";
				return false;
			}
			text = propertyResult2.Value.StringResult;
		}
		else
		{
			if (entityData == null)
			{
				return false;
			}
			text = entityData.EntityKey;
		}
		Entity entity = null;
		StorageCondition placeInStorage = null;
		bool offerForSale = false;
		bool isProductionOutput = false;
		UpgradeCategory upgradeCategory = null;
		Vector3? vector = null;
		int? amount = null;
		EntityType entityType = GameData.Instance.AllEntityTypes[text];
		if (AddToContainer != null)
		{
			entity = AddToContainer.GetContainer(action);
			offerForSale = AddToContainer.OfferForSale;
			isProductionOutput = AddToContainer.IsProductionOutput;
			if (AddToContainer.UpgradeCategory != null)
			{
				upgradeCategory = GameData.Instance.AllUpgradeCategories[AddToContainer.UpgradeCategory];
			}
			if (entity == null)
			{
				failReason = string.Concat(AddToContainer.TargetObject, " container not found.");
				lastSpawnInfo = ComposeInfoString(amount, entity, vector);
				return false;
			}
			if (AddToContainer.StorageCondition != null)
			{
				placeInStorage = GameData.Instance.AllStorageConditions[AddToContainer.StorageCondition];
			}
		}
		else
		{
			vector = ((entityData == null) ? new Vector3?(new Vector3(0f, 0f, 0f)) : entityData.Location);
			if (DynamicLocation != null)
			{
				Vector2? location = DynamicLocation.GetLocation(action);
				if (!location.HasValue)
				{
					failReason = DynamicLocation.PropertyKey + " dynamic offset location was null.";
					lastSpawnInfo = ComposeInfoString(amount, entity, vector);
					return false;
				}
				vector += location.Value.ToVector3();
			}
			if (vector.HasValue && !The.Map.WorldLocationIsInsideMap(vector.Value))
			{
				failReason = string.Concat(MapManager.WorldPosToSubtile(vector.Value), " outside map.");
				lastSpawnInfo = ComposeInfoString(amount, entity, vector);
				return false;
			}
			if (vector.HasValue && entityType.IntelligenceType != null && !entityType.IsFlyer && The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], MapManager.WorldPosToSubtile(vector.Value)))
			{
				failReason = string.Concat(MapManager.WorldPosToSubtile(vector.Value), " subtile blocked.");
				lastSpawnInfo = ComposeInfoString(amount, entity, vector);
				return false;
			}
		}
		Entity entity2 = null;
		if ((ActingOnEntityName != null || ActingOnEntityObject != null) && !EventActionType.GetEntity(ActingOnEntityName, ActingOnEntityObject, action, out entity2, ref failReason))
		{
			return false;
		}
		amount = 1;
		if (Amount != null)
		{
			PropertyResult? propertyResult3 = Amount.Evaluate(action);
			amount = ((!propertyResult3.HasValue) ? new int?(0) : new int?(propertyResult3.Value.GetIntegerResult() ?? 0));
		}
		if (entityData == null)
		{
			entityData = new EntityData();
			entityData.EntityKey = text;
		}
		string siteKey = null;
		if (Site != null)
		{
			PropertyResult? propertyResult4 = Site.Evaluate(action);
			if (propertyResult4.HasValue)
			{
				siteKey = propertyResult4.Value.StringResult;
			}
		}
		string allegiance = null;
		string expedition = null;
		string memberOfAllegianceKey = null;
		string memberOfExpeditionKey = null;
		if (OwnedBy != null)
		{
			OwnedBy.Resolve(action, ref allegiance, ref expedition);
		}
		else if (entityData != null)
		{
			if (entityData.OwnedBy != null)
			{
				allegiance = entityData.OwnedBy.AllegianceKey;
				expedition = entityData.OwnedBy.ExpeditionKey;
			}
			else if (entityData.MemberOf != null)
			{
				_ = entityData.MemberOf.AllegianceKey == "otherSite1Allegiance1";
				memberOfAllegianceKey = entityData.MemberOf.AllegianceKey;
				memberOfExpeditionKey = entityData.MemberOf.ExpeditionKey;
			}
		}
		EntityID? entityID = null;
		if (entity2 != null)
		{
			entityID = entity2.ID;
		}
		ProcessType processType = null;
		if (ProductionProcessToUse != null)
		{
			processType = GameData.Instance.AllProcessTypes[ProductionProcessToUse];
		}
		Allegiance allegiance2 = null;
		for (int i = 0; i < amount; i++)
		{
			EntityID? entityID2 = null;
			bool placementFailed;
			Entity entity3 = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placementFailed, entity, placeInStorage, offerForSale, isProductionOutput, vector, allegiance, expedition, memberOfAllegianceKey, memberOfExpeditionKey, siteKey, entityID, assertContainment: false, upgradeCategory, null, null, LogAsProduction, SuppressSpawnEvents);
			entityID2 = entity3.ID;
			if (placementFailed)
			{
				entity3.Destroy();
				failReason = "Failed to place entity.";
				lastSpawnInfo = ComposeInfoString(amount, entity, vector);
				return false;
			}
			The.Sim.World.LastSpawnedEntity = entityID2;
			if (processType != null)
			{
				if (entity2 != null)
				{
					Goal.FireEventActions(null, entity2.ID, processType.GetStartHook(), processType.EventActions, AgentActionHooks.StartProducing, null);
				}
				SimProcess.ProcessProductionFinished(null, processType, entityID, entityID2, SuppressSpawnEvents);
			}
			allegiance2 = entity3.GetAllegianceOrOwner();
		}
		lastSpawnInfo = ComposeInfoString(amount, entity, vector);
		if (allegiance2 != null && entity2 != null && entity2.ID != EntityID.Invalid)
		{
			ProcessAction.DetectEntity(entity2, allegiance2);
		}
		return true;
	}

	public override void PostInitValidate(ref List<string> listOfErrors)
	{
		if (EntityData == null && EntityType == null && EntityDataKey == null)
		{
			UWGame.SimSide.Entities.EntityType.CreateValidationError(ref listOfErrors, "Neither EntityData nor EntityKey were filled out!");
		}
		if (EntityData != null && EntityData.BioEntity != null)
		{
			EntityData.PostInitValidate(ref listOfErrors);
		}
	}

	public override void PostLoadContentValidate(ref List<string> listOfErrors)
	{
		if (EntityData != null)
		{
			EntityData.PostLoadContentValidate(listOfErrors);
		}
	}

	public override void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (EntityData != null)
		{
			EntityData.PostDataCompleteValidate(ref listOfErrors);
		}
		if (EntityDataKey != null)
		{
			string text = EntityDataKey.EvaluateConstant();
			if (text != null)
			{
				UWGame.SimSide.Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, text, GameData.Instance.AllEntityData, out var _);
			}
		}
		if (ProductionProcessToUse != null)
		{
			UWGame.SimSide.Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProductionProcessToUse, GameData.Instance.AllProcessTypes, out var _);
		}
		if (AddToContainer != null && AddToContainer.StorageCondition != null)
		{
			UWGame.SimSide.Entities.EntityType.ValidateGameDataTypeExists(ref listOfErrors, AddToContainer.StorageCondition, GameData.Instance.AllStorageConditions, out var _);
		}
	}

	public override string ToString()
	{
		return lastSpawnInfo ?? "Spawn entity";
	}

	private string ComposeInfoString(int? amount, Entity container, Vector3? spawnLocation)
	{
		string text = "Spawn ";
		if (amount.HasValue)
		{
			text = text + amount.Value + " ";
		}
		if (EntityData != null && EntityData.EntityKey != null)
		{
			text += EntityData.EntityKey;
		}
		else if (EntityDataKey != null)
		{
			text += EntityDataKey.ToString();
		}
		else
		{
			if (EntityType == null)
			{
				throw new Exception("No valid EntityKey found for EntityData");
			}
			text += EntityType.ToString();
		}
		if (spawnLocation.HasValue)
		{
			text = text + " at " + spawnLocation.Value.ToString();
		}
		else if (container != null)
		{
			text += " inside ";
			text = (string.IsNullOrEmpty(container.Name) ? (text + container.KeyName) : (text + container.Name));
			text = text + " at " + container.Location.ToString();
		}
		return text;
	}
}
