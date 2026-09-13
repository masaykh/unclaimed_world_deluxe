using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData.Scenarios;

public abstract class ScenarioLoader
{
	public const string scenarioHeaderFileName = "scenario.xml";

	public const string ScenarioDataFileName = "scenarioData.xml";

	public abstract string FolderName { get; }

	public void WriteScenario()
	{
		DataLoader.SerializeObject(GetScenarioHeader(), FolderName, "scenario.xml", Config.DataType.RGScenario);
		DataLoader.SerializeObject(GetScenarioData(), FolderName, "scenarioData.xml", Config.DataType.RGScenario);
	}

	public Scenario GetScenarioHeader()
	{
		Scenario finalDataObject = null;
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
		{
			finalDataObject = InitScenarioHeader();
		}
		DataLoader.SerializeAndDeserializeObject(finalDataObject, ref finalDataObject, FolderName, "scenario.xml", Config.DataType.RGScenario);
		finalDataObject.SetRGSource();
		return finalDataObject;
	}

	protected abstract Scenario InitScenarioHeader();

	public ScenarioData GetScenarioData()
	{
		ScenarioData finalDataObject = null;
		if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
		{
			finalDataObject = InitScenarioData();
		}
		DataLoader.SerializeAndDeserializeObject(finalDataObject, ref finalDataObject, FolderName, "scenarioData.xml", Config.DataType.RGScenario);
		return finalDataObject;
	}

	protected abstract ScenarioData InitScenarioData();

	public abstract DataLoader GetDataLoader();

	public static EventActionType SpawnItemOtherSite(string keyname, string entityType, string ownerAllegiance, string ownerExpeditionName, double delay, string name = null, string site = null, string container = null, bool? offerForSale = null, int amount = 1)
	{
		EntityData entityData = new EntityData
		{
			EntityKey = entityType,
			Name = name
		};
		if (!string.IsNullOrEmpty(ownerAllegiance))
		{
			entityData.OwnedBy = new AllegianceAndExpedition
			{
				AllegianceKey = ownerAllegiance,
				ExpeditionKey = ownerExpeditionName
			};
		}
		SpawnEntityAction spawnEntityAction = new SpawnEntityAction
		{
			KeyName = keyname,
			DelayInSeconds = delay,
			EntityData = entityData,
			Amount = new ValueNode
			{
				Int = amount
			}
		};
		if (container != null)
		{
			spawnEntityAction.AddToContainer = new ContainerLocation
			{
				OfferForSale = (offerForSale ?? false),
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.World,
					GetList = new GetList
					{
						HasPropertiesListKey = "sites",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = site
						},
						NextList = new GetList
						{
							HasPropertiesListKey = "entities",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "name",
								ConstantStringEqual = container
							}
						}
					}
				}
			};
		}
		return spawnEntityAction;
	}

	public static EventActionType SpawnItemAtStartLocation(string keyname, Vector2 locationOffset, string entityType, string ownerAllegiance, string ownerExpeditionName, double delay, string name = null, float? rotation = null, int amount = 1)
	{
		EntityData entityData = new EntityData
		{
			Location = locationOffset.ToVector3(),
			EntityKey = entityType,
			Rotation = rotation,
			Name = name
		};
		if (!string.IsNullOrEmpty(ownerAllegiance))
		{
			entityData.OwnedBy = new AllegianceAndExpedition
			{
				AllegianceKey = ownerAllegiance,
				ExpeditionKey = ownerExpeditionName
			};
		}
		return new SpawnEntityAction
		{
			KeyName = keyname,
			DelayInSeconds = delay,
			DynamicLocation = new DynamicLocation
			{
				PropertyKey = "startingLocation"
			},
			EntityData = entityData,
			Amount = new ValueNode
			{
				Int = amount
			}
		};
	}

	public static ProcessAction RunProcess(string newEventKeyName, double delay, string processToUse, string actingOnEntity)
	{
		return new ProcessAction
		{
			KeyName = newEventKeyName,
			DelayInSeconds = delay,
			FinishProcessImmediately = true,
			ProcessType = processToUse,
			ActingOnEntityName = actingOnEntity,
			WorkerObject = new TargetObject
			{
				GetList = new GetList
				{
					HasPropertiesListKey = "allegiances",
					FilterCondition = new PropertyCondition
					{
						PropertyKey = "keyName",
						ConstantStringEqual = "playerAllegiance"
					},
					NextList = new GetList
					{
						HasPropertiesListKey = "persons"
					}
				}
			}
		};
	}

	public static EventActionType SpawnEntity(string keyname, Vector2 location, string entityType, string ownerAllegiance, string ownerExpeditionName, double delay, string name = null, string processToUse = null, string actingOnEntity = null, int? amount = null)
	{
		EntityData entityData = null;
		if (entityType != null)
		{
			entityData = new EntityData
			{
				Location = location.ToVector3(),
				EntityKey = entityType,
				Name = name
			};
		}
		if (!string.IsNullOrEmpty(ownerAllegiance))
		{
			entityData.OwnedBy = new AllegianceAndExpedition
			{
				AllegianceKey = ownerAllegiance,
				ExpeditionKey = ownerExpeditionName
			};
		}
		EvalNode amount2 = null;
		if (amount.HasValue)
		{
			amount2 = new ValueNode
			{
				Int = amount.Value
			};
		}
		return new SpawnEntityAction(keyname)
		{
			DelayInSeconds = delay,
			EntityData = entityData,
			ProductionProcessToUse = processToUse,
			ActingOnEntityName = actingOnEntity,
			Amount = amount2
		};
	}

	public static EventActionType SpawnItemInsideContainer(string keyname, string container, string entityType, string ownerAllegiance, string ownerExpeditionName, double delay, bool? offerForSale = null, float? bulk = null, string storageCondition = null, string upgradeCategory = null, bool? isProductionOutput = null)
	{
		EntityData entityData = new EntityData
		{
			EntityKey = entityType,
			Bulk = bulk
		};
		if (!string.IsNullOrEmpty(ownerAllegiance))
		{
			entityData.OwnedBy = new AllegianceAndExpedition
			{
				AllegianceKey = ownerAllegiance,
				ExpeditionKey = ownerExpeditionName
			};
		}
		return new SpawnEntityAction
		{
			KeyName = keyname,
			DelayInSeconds = delay,
			EntityData = entityData,
			AddToContainer = new ContainerLocation
			{
				OfferForSale = (offerForSale ?? false),
				StorageCondition = storageCondition,
				UpgradeCategory = upgradeCategory,
				IsProductionOutput = (isProductionOutput ?? false),
				TargetObject = new TargetObject
				{
					TargetObjectType = TargetObjectType.Root,
					GetList = new GetList
					{
						HasPropertiesListKey = "entities",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = container
						}
					}
				}
			}
		};
	}
}
