using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI;

[DebuggerDisplay("{EntityType.Name}{MapPosition}")]
public class MemoryFact : GameObject, IKnownEntityData, IHasExposedProperties, ILookUp<MemoryFact, MemoryFactID>, ISnapshot
{
	public enum StatusProperty
	{
		Condition,
		AllegianceId,
		Stance
	}

	private SharedKnowledge sharedKnowledge;

	private AllegianceID snapshotAllegianceID;

	public DebugLog DebugLog = new DebugLog();

	private bool isDeprecated;

	public double? ToBeDeletedOnTimeStampInSecondsOfGameTime;

	public double TimeStampInSecondsOfGameTime;

	private string casteTypeKey;

	private GatheringSiteID? snapshotGatheringSite;

	public Dictionary<string, PropertyResult> CustomFields;

	private RepairPackage repairPackage;

	private JobID? assignedToJob;

	private bool flipHorizontally;

	private Dictionary<StorageCondition, Storage> storageSpaces;

	private Dictionary<StorageCondition, Storage> tradeOfferStorageSpaces;

	private Dictionary<EntityID, EntityID> contains;

	private bool notOnboardDrivenVehicle;

	private float loadedVehicleSpeed;

	private PassengerOrCargoSlot freeDriversSlot;

	private List<PassengerOrCargoSlot> listOfCargoSlotsForLoading;

	private List<PassengerOrCargoSlot> listOfCargoSlotsForUnloading;

	private float? fuel;

	private Dictionary<EntityType, int> ammoItems = new Dictionary<EntityType, int>();

	private bool isCompleted;

	private bool? isStarted;

	private float? progress;

	private bool isWeatherProof;

	private Regulator showStatusRegulator;

	private ThreatGroupID? snapshotThreatGroup;

	public Renderable Renderable;

	private Renderable.SnapshotRenderable snapshotRenderable;

	private EntityTypeTooltipInstanceData tooltipEntityData = new EntityTypeTooltipInstanceData();

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private MemoryFactID id = MemoryFactID.Invalid;

	private static MemoryFactID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public EntityID EntityID { get; set; }

	public bool IsDeprecated
	{
		get
		{
			return isDeprecated;
		}
		set
		{
			if (value == isDeprecated)
			{
				return;
			}
			isDeprecated = value;
			if (!isDeprecated)
			{
				return;
			}
			if (sharedKnowledge.PlaySiteKnowledge != null)
			{
				sharedKnowledge.PlaySiteKnowledge.DeleteMemoryOfProcesses(this);
			}
			if (sharedKnowledge.Allegiance.IsOwnedByAllegiance(this))
			{
				if (Replenishes.HasValue && sharedKnowledge.GetKnownData(Replenishes.Value, out var _) == EntityResult.SeenDirectly)
				{
					Entity.LogProductionEvent(this, ProductionStatistics.StatTypes.UsedAsInput, testIfSeen: false);
				}
				else
				{
					sharedKnowledge.Allegiance.Statistics.AddProductionEvent(EntityType, ProductionStatistics.StatTypes.Disappeared, 1);
				}
			}
		}
	}

	public Vector3 PlaySiteLocation => Location.Value;

	public Point? MapPosition { get; set; }

	public SiteID? Site { get; set; }

	public Point? TopLeftMapPosition { get; set; }

	public float Rotation { get; set; }

	public Vector3 FacingNormal { get; set; }

	public bool? IsMoving { get; set; }

	public EntityType EntityType { get; set; }

	public float Bulk { get; set; }

	public double? Condition { get; set; }

	public float? ConditionChangeSpeed { get; set; }

	public CasteType CasteType { get; set; }

	public float? Integrity { get; set; }

	public Dictionary<EntityType, EntityID> IntrinsicWeapons { get; set; }

	public List<ProcessType> AvailableSharedSpecialActions { get; set; }

	public List<SimProcessID> Processes { get; set; }

	public Body Body { get; set; }

	public Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts { get; set; }

	public string Name { get; set; }

	public GatheringSite GatheringSite { get; set; }

	public bool PartIsBroken { get; set; }

	public double? FunctionalScore { get; set; }

	public StanceType Stance { get; set; }

	public float? StrengthRating { get; private set; }

	public Vector3? AccessPoint { get; set; }

	public OwnerID? OwnedBy { get; set; }

	public JobID? AssignedToJob
	{
		get
		{
			return assignedToJob;
		}
		set
		{
			if (assignedToJob != value)
			{
				if (value.HasValue)
				{
					DebugLog.Add($"[MF] AssignedToJob set: JobID {value.Value.ToString()}");
				}
				else
				{
					DebugLog.Add($"[MF] AssignedToJob cleared. Old JobID: {assignedToJob.Value.ToString()}");
				}
				assignedToJob = value;
			}
		}
	}

	public float BoundingRadius3D { get; set; }

	public bool FlipHorizontally
	{
		get
		{
			return flipHorizontally;
		}
		set
		{
			flipHorizontally = value;
			if (Renderable != null)
			{
				Renderable.FlipHorizontally = value;
			}
		}
	}

	public bool HasItemStorage { get; set; }

	public Dictionary<StorageCondition, Storage> StorageSpaces => storageSpaces;

	public Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces => tradeOfferStorageSpaces;

	public float? TotalItemStorageCapacity { get; set; }

	public float? TotalStored { get; set; }

	public Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType { get; set; }

	public List<EntityID> ContainedEntities
	{
		get
		{
			if (contains != null)
			{
				return contains.Keys.ToList();
			}
			return null;
		}
	}

	public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; set; }

	public Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType { get; set; }

	public EntityID? ContainedBy { get; set; }

	public EntityID? Replenishes { get; set; }

	public EntityID? UpgradeFor { get; set; }

	public StorageTarget? StoredPermanentlyIn { get; set; }

	public bool NotOnboardDrivenVehicle => notOnboardDrivenVehicle;

	public float? CurrentMaximumSpeed { get; set; }

	public int? NoOfRounds { get; set; }

	public Dictionary<FoodNutrientType, float> NutrientBulkAmounts { get; set; }

	public int? Residents { get; set; }

	public float? ComfortLevel { get; set; }

	public List<HouseholdID> Households { get; set; }

	public bool? IsPrepared { get; set; }

	public CompositeID? PartOfID { get; set; }

	public List<EntityID> PartIDs { get; set; }

	public EntityID? ParentEntityID { get; private set; }

	public EntityID RootEntityID { get; private set; }

	public float? Progress => progress;

	public AllegianceID? AllegianceID { get; set; }

	public ThreatGroup ThreatGroup { get; set; }

	public EntityTypeTooltipInstanceData TooltipEntityData
	{
		get
		{
			return tooltipEntityData;
		}
		set
		{
			tooltipEntityData = value;
		}
	}

	public Vector3 RenderedLocation => Renderable.Location.Value;

	public string KeyName => EntityType.KeyName;

	public MemoryFactID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public bool IsAlwaysShown()
	{
		return EntityType.GetIsNeverInFogOfWar();
	}

	public Storage FindStorage(StorageID storageID)
	{
		if (storageSpaces != null)
		{
			foreach (KeyValuePair<StorageCondition, Storage> storageSpace in storageSpaces)
			{
				if (storageSpace.Value.ID == storageID)
				{
					return storageSpace.Value;
				}
			}
		}
		if (tradeOfferStorageSpaces != null)
		{
			foreach (KeyValuePair<StorageCondition, Storage> tradeOfferStorageSpace in tradeOfferStorageSpaces)
			{
				if (tradeOfferStorageSpace.Value.ID == storageID)
				{
					return tradeOfferStorageSpace.Value;
				}
			}
		}
		return null;
	}

	public bool IsTradeOfferStorage(StorageID storageID)
	{
		if (tradeOfferStorageSpaces != null)
		{
			return tradeOfferStorageSpaces.Any((KeyValuePair<StorageCondition, Storage> s) => s.Value.ID == storageID);
		}
		return false;
	}

	public bool ContainsEntity(EntityID entity)
	{
		if (contains != null)
		{
			return contains.ContainsKey(entity);
		}
		return false;
	}

	public float CalculateSpeed(float bulk)
	{
		return loadedVehicleSpeed;
	}

	public PassengerOrCargoSlot GetFreeDriversSlot()
	{
		Entity entity = Entity.FindByID(EntityID);
		if (entity != null)
		{
			return entity.GetFreeDriversSlot();
		}
		return freeDriversSlot;
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
	{
		Entity entity = Entity.FindByID(EntityID);
		if (entity != null)
		{
			return entity.GetCargoSlotsForLoading(bulkToLoad);
		}
		return listOfCargoSlotsForLoading;
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad)
	{
		Entity entity = Entity.FindByID(EntityID);
		if (entity != null)
		{
			return entity.GetCargoSlotsForUnloading(bulkToLoad);
		}
		return listOfCargoSlotsForUnloading;
	}

	public bool HasEnoughFuel(float neededFuel)
	{
		if (fuel.HasValue)
		{
			return fuel.Value >= neededFuel;
		}
		return true;
	}

	public bool HasEnoughAmmo(EntityType ammoType, int noOfRounds)
	{
		if (ammoItems != null && ammoItems.TryGetValue(ammoType, out var value))
		{
			return value > noOfRounds;
		}
		return false;
	}

	public bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload)
	{
		return Entity.NeedsReload(sharedKnowledge, this, out itemToReload);
	}

	public bool NeedsRepair()
	{
		return repairPackage != null;
	}

	public RepairPackage ComputeBestRepairPackage()
	{
		return repairPackage;
	}

	public float GetRepairProgress(RepairAction repairAction)
	{
		switch (repairAction)
		{
		case RepairAction.Integrity:
			return Integrity.Value;
		case RepairAction.PartsCondition:
		case RepairAction.Condition:
			return (float)Condition.Value;
		default:
			throw new NotImplementedException();
		}
	}

	public int? GetTotalAmmo()
	{
		if (ammoItems != null)
		{
			return ammoItems.Sum((KeyValuePair<EntityType, int> a) => a.Value);
		}
		return null;
	}

	public bool HasEnergyForDuration(float jobDuration)
	{
		bool result = true;
		if (fuel.HasValue)
		{
			result = RequiresFuel.HasFuelForDuration(jobDuration, EntityType, fuel.Value);
		}
		return result;
	}

	public EntityAndRoot GetAsEntityAndRoot()
	{
		return new EntityAndRoot(EntityID, RootEntityID);
	}

	public bool IsCompleted()
	{
		return isCompleted;
	}

	public bool? IsStarted()
	{
		return isStarted;
	}

	public bool IsWeatherProof()
	{
		return isWeatherProof;
	}

	public bool IsTimeToShowStatusMarkerWindow()
	{
		return showStatusRegulator.IsReady();
	}

	public bool IsUnassigned(SharedKnowledge sharedKnowledge)
	{
		return IsUnassigned(this, sharedKnowledge);
	}

	public static bool IsUnassigned(IKnownEntityData entity, SharedKnowledge sharedKnowledge)
	{
		if (!entity.PartOfID.HasValue && !entity.AssignedToJob.HasValue)
		{
			return !sharedKnowledge.GetInUseBy(entity.EntityID).HasValue;
		}
		return false;
	}

	public bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge)
	{
		return IsUnassignedToAnythingButThisJob(this, job, sharedKnowledge);
	}

	public static bool IsUnassignedToAnythingButThisJob(IKnownEntityData entity, Job job, SharedKnowledge sharedKnowledge)
	{
		if (!entity.PartOfID.HasValue && (!entity.AssignedToJob.HasValue || entity.AssignedToJob == job.ID))
		{
			return !sharedKnowledge.GetInUseBy(entity.EntityID).HasValue;
		}
		return false;
	}

	public bool CanBeHauled()
	{
		return Entity.CanBeHauled(this);
	}

	public bool IsItemValidForHauling(Entity haulingEntity, Intelligence entityIntelligence, HaulingJob job)
	{
		return Entity.IsItemValidForHauling(haulingEntity, entityIntelligence, job, this);
	}

	public bool CanBeHunted(Allegiance byAllegiance)
	{
		return Entity.CanBeHunted(this, byAllegiance);
	}

	public bool IsVehicleValidForHauling(Entity entity, IKnownEntityData item)
	{
		if (HasItemStorage)
		{
			float? totalItemStorageCapacity = TotalItemStorageCapacity;
			float bulk = item.Bulk;
			if (totalItemStorageCapacity.GetValueOrDefault() >= bulk && totalItemStorageCapacity.HasValue && IsCompleted() && Entity.IsFunctional(this))
			{
				return true;
			}
		}
		return false;
	}

	public void ChangeOwnership(IOwner newOwner, Entity.GiveNewOwnerKnowledge giveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes)
	{
		Entity.ChangeOwnership(this, newOwner, giveNewOwnerKnowledge);
	}

	public static MemoryFact GetNew(Entity gameEntity, Allegiance allegiance)
	{
		MemoryFact memoryFact = new MemoryFact();
		memoryFact.AddToLookup();
		if (!memoryFact.Init(gameEntity, allegiance))
		{
			memoryFact.Destroy();
			return null;
		}
		return memoryFact;
	}

	public bool Init(Entity entity, Allegiance allegiance)
	{
		DebugLog.Add("[MF] MemoryFact created");
		_ = entity.EntityID;
		_ = 4991;
		isDeprecated = false;
		EntityID = entity.EntityID;
		EntityType = entity.EntityType;
		Renderable = null;
		The.Client.SetRenderableOnMemoryFact(this, entity, allegiance);
		sharedKnowledge = allegiance.SharedKnowledge;
		TimeStampInSecondsOfGameTime = The.Sim.TotalUnPausedGameTimeInSeconds;
		Location = entity.Location;
		MapPosition = entity.MapPosition;
		Site = ((IKnownEntityData)entity).Site;
		TopLeftMapPosition = entity.TopLeftMapPosition;
		AccessPoint = entity.AccessPoint;
		FlipHorizontally = entity.FlipHorizontally;
		IsMoving = entity.IsMoving;
		PartIsBroken = entity.PartIsBroken;
		showStatusRegulator = Entity.CreateShowStatusIconRegulator("MemoryFact", entity.EntityType);
		Bulk = entity.Bulk;
		AssignedToJob = entity.AssignedToJob;
		ContainedBy = entity.ContainedBy;
		ContainedUpgrades = entity.ContainedUpgrades;
		BoundingRadius3D = entity.BoundingRadius3D;
		GatheringSite = entity.GatheringSite;
		OwnedBy = entity.OwnedBy;
		Rotation = entity.Rotation;
		FacingNormal = entity.FacingNormal;
		if (entity.Find<Locomotor>(out var c))
		{
			CurrentMaximumSpeed = c.CurrentMaximumSpeed;
			loadedVehicleSpeed = c.CalculateSpeed(1f);
		}
		else
		{
			CurrentMaximumSpeed = null;
			loadedVehicleSpeed = 0f;
		}
		if (entity.EntityType.NonLivingType != null && entity.Find<NonLivingEntity>(out var c2))
		{
			Condition = c2.Condition;
			ConditionChangeSpeed = c2.ConditionChangeSpeed;
			Integrity = c2.Integrity;
			progress = c2.Progress;
			if (entity.EntityType.IsRepairable() && c2.NeedsRepair())
			{
				repairPackage = c2.ComputeBestRepairPackage();
			}
			else
			{
				repairPackage = null;
			}
		}
		else
		{
			Condition = null;
			Integrity = null;
			progress = null;
		}
		fuel = null;
		IsPrepared = null;
		if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType.GetRequiresReplenishType() != null)
		{
			ReplenishItems replenishItems = ((IHasReplenishItems)entity.Contains).ReplenishItems;
			if (replenishItems != null && replenishItems.RequiresFuel != null)
			{
				fuel = replenishItems.RequiresFuel.Fuel;
			}
		}
		if (entity.Find<UWGame.SimSide.Items.Tool>(out var c3))
		{
			IsPrepared = c3.IsPrepared;
		}
		ammoItems = null;
		if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType is MagazineContainerType && entity.Contains is MagazineContainer magazineContainer)
		{
			magazineContainer.GetAmmoStatus(ref ammoItems);
		}
		NoOfRounds = null;
		if (entity.EntityType.ItemType != null && entity.EntityType.ItemType.AmmunitionType != null)
		{
			NoOfRounds = entity.Item.Ammunition.NoOfRounds;
		}
		StrengthRating = entity.StrengthRating;
		FunctionalScore = entity.FunctionalScore;
		StoredPermanentlyIn = entity.StoredPermanentlyIn;
		if (entity.Processes != null)
		{
			Processes = new List<SimProcessID>(entity.Processes);
		}
		HasItemStorage = false;
		if (storageSpaces != null)
		{
			storageSpaces.Clear();
		}
		TotalItemStorageCapacity = null;
		TotalStored = null;
		contains = null;
		Container container = entity.Contains;
		if (container != null)
		{
			if (container is IStorage storage)
			{
				HasItemStorage = true;
				TotalItemStorageCapacity = storage.TotalItemStorageCapacity;
				TotalStored = storage.TotalStored;
				if (storageSpaces == null)
				{
					storageSpaces = new Dictionary<StorageCondition, Storage>();
				}
				foreach (KeyValuePair<StorageCondition, Storage> storageSpace in storage.GetStorageSpaces())
				{
					storageSpaces.Add(storageSpace.Key, new Storage(storageSpace.Value));
				}
			}
			if (container is TerminalContainer terminalContainer)
			{
				if (tradeOfferStorageSpaces == null)
				{
					tradeOfferStorageSpaces = new Dictionary<StorageCondition, Storage>();
				}
				foreach (KeyValuePair<StorageCondition, Storage> tradeOffersStorageSpace in terminalContainer.GetTradeOffersStorageSpaces())
				{
					tradeOfferStorageSpaces.Add(tradeOffersStorageSpace.Key, new Storage(tradeOffersStorageSpace.Value));
				}
			}
			container.IterateContained(GetContainedStatus);
			ContainedEntitiesByType = new Dictionary<EntityType, List<EntityID>>();
			container.IterateContained(delegate(Entity e)
			{
				Common.AddToMultiList(ContainedEntitiesByType, e.EntityType, e.ID);
			});
			if (entity.Contains is TerminalContainer terminalContainer2)
			{
				OfferedEntitiesByType = terminalContainer2.GetOfferedItems();
			}
		}
		PartOfID = entity.PartOfID;
		if (PartOfID.HasValue && LookUpIComposites.FindByID(PartOfID.Value) is Entity entity2)
		{
			ParentEntityID = entity2.EntityID;
		}
		PartIDs = entity.PartIDs;
		if (entity.GetRoot() is Entity entity3)
		{
			RootEntityID = entity3.ID;
		}
		isCompleted = entity.IsCompleted();
		isStarted = entity.IsStarted();
		isWeatherProof = entity.IsWeatherProof();
		Residents = entity.Residents;
		ComfortLevel = entity.ComfortLevel;
		List<HouseholdID> households = entity.Households;
		if (households != null)
		{
			Households = new List<HouseholdID>();
			Households.AddRange(households);
		}
		NutrientBulkAmounts = null;
		if (entity.EntityType.ItemType != null)
		{
			notOnboardDrivenVehicle = entity.NotOnboardDrivenVehicle;
			if (entity.EntityType.ItemType.FoodType != null)
			{
				NutrientBulkAmounts = entity.Item.Food.NutrientBulkAmounts.ToDictionary((KeyValuePair<FoodNutrientType, float> k) => k.Key, (KeyValuePair<FoodNutrientType, float> v) => v.Value);
			}
		}
		else
		{
			notOnboardDrivenVehicle = true;
		}
		Name = entity.Name;
		if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType is VehicleContainerType)
		{
			freeDriversSlot = entity.GetFreeDriversSlot();
			listOfCargoSlotsForLoading = entity.GetCargoSlotsForLoading(1f);
			listOfCargoSlotsForUnloading = entity.GetCargoSlotsForUnloading(1f);
		}
		else
		{
			freeDriversSlot = null;
			listOfCargoSlotsForLoading = null;
			listOfCargoSlotsForUnloading = null;
		}
		if (!entity.GetReplenishes(out var replenishes))
		{
			return false;
		}
		if (replenishes != null)
		{
			Replenishes = replenishes.EntityID;
		}
		else
		{
			Replenishes = null;
		}
		if (!entity.GetUpgradesFor(out var upgrades))
		{
			return false;
		}
		if (upgrades != null)
		{
			UpgradeFor = upgrades.EntityID;
		}
		else
		{
			UpgradeFor = null;
		}
		AvailableSharedSpecialActions = new List<ProcessType>();
		foreach (ProcessType availableSharedSpecialAction in entity.AvailableSharedSpecialActions)
		{
			AvailableSharedSpecialActions.Add(availableSharedSpecialAction);
		}
		AllegianceID = null;
		Body = null;
		if (entity.Find<BodyComponent>(out var c4))
		{
			Body = new Body(c4.Body);
			Body.ParentMemoryFact = this;
		}
		if (entity.Find<Intelligence>(out var c5))
		{
			AllegianceID = c5.Allegiance.ID;
			IntrinsicWeapons = c5.IntrinsicWeapons;
		}
		if (entity.Locomotor != null)
		{
			if (entity.Find<Vehicle>(out var c6))
			{
				_ = c6.Roll;
				_ = c6.Pitch;
			}
			Stance = entity.Stance;
		}
		if (entity.EntityType.BiologicalType != null)
		{
			entity.Find<BiologicalEntity>(out var c7);
			CasteType = c7.CasteType;
		}
		if (entity.EntityType.SubstancesType != null && entity.Find<SubstanceComponent>(out var c8))
		{
			SubstanceBulkAmounts = new Dictionary<SubstanceType, SubstanceAmount>();
			foreach (KeyValuePair<SubstanceType, SubstanceAmount> bulkAmount in c8.BulkAmounts)
			{
				SubstanceBulkAmounts.Add(bulkAmount.Key, new SubstanceAmount(bulkAmount.Value));
			}
		}
		if (entity.TooltipEntityData != null)
		{
			TooltipEntityData = new EntityTypeTooltipInstanceData(entity.TooltipEntityData);
		}
		if (entity.CustomFields != null)
		{
			CustomFields = new Dictionary<string, PropertyResult>();
			foreach (KeyValuePair<string, PropertyResult> customField in entity.CustomFields)
			{
				CustomFields.Add(customField.Key, customField.Value);
			}
		}
		return true;
	}

	public bool CanSetStockpileSettings(EntityGroupID byOwner)
	{
		return Entity.CanSetStockpileSettings(this, byOwner);
	}

	public bool CanSetTradeOfferSettings(EntityGroupID byOwner)
	{
		return Entity.CanSetTradeOfferSettings(this, byOwner);
	}

	public bool CanBeUpgraded(EntityGroupID byOwner)
	{
		return Entity.CanBeUpgraded(this, byOwner);
	}

	public void GetContainedStatus(Entity entity)
	{
		if (contains == null)
		{
			contains = new Dictionary<EntityID, EntityID>();
		}
		contains.Add(entity.EntityID, entity.EntityID);
	}

	static MemoryFact()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		IDCounter = MemoryFactID.First;
		exposedPropertyValueFunctions.Add("bulkForPresentation", GetBulkForPresentation);
		exposedPropertyValueFunctions.Add("hitpointLevel", GetHitpointsFraction);
		exposedPropertyValueFunctions.Add("itemPartCondition", GetCondition);
		exposedPropertyValueFunctions.Add("itemPartConditionTooltip", GetConditionTooltip);
		exposedPropertyValueFunctions.Add("integrity", GetIntegrity);
		exposedPropertyValueFunctions.Add("progress", GetProgress);
		exposedPropertyValueFunctions.Add("inAccessible", GetInaccessible);
		exposedPropertyValueFunctions.Add("homeComfortLevel", GetHomeComfortLevel);
		exposedPropertyValueFunctions.Add("replenishStatus", GetReplenishStatus);
		exposedPropertyValueFunctions.Add("replenishStatusTooltip", GetReplenishStatusTooltip);
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		PropertyResult? result = null;
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (CustomFields != null && CustomFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		if (!(key == "bodyParts"))
		{
			if (!(key == "residents"))
			{
				_ = key == "itemParts";
			}
			else
			{
				Entity.GetResidents(this, getterKnowledge, ref listOfChildren);
			}
		}
		else if (Body != null)
		{
			Body.GetBodyParts(ref listOfChildren);
		}
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
	}

	public string GetDisplayName()
	{
		return Entity.GetDisplayName(this);
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return EntityType.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = string.Concat(EntityID);
	}

	public EntityID? GetEntityID()
	{
		return EntityID;
	}

	public bool GetIsSeenDirectly()
	{
		return false;
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public void Destroy()
	{
		RemoveIDEntry();
		if (storageSpaces != null)
		{
			foreach (KeyValuePair<StorageCondition, Storage> storageSpace in storageSpaces)
			{
				storageSpace.Value.Destroy();
			}
		}
		if (MapPosition.HasValue)
		{
			The.Map.GetTile(MapPosition.Value).RemoveRememberedRootEntity(sharedKnowledge, this);
		}
		if (Entity.FindByID(EntityID) == null)
		{
			The.Client.DestroyAccessibility(EntityID);
			if (GatheringSite != null)
			{
				GatheringSite.Destroy();
			}
		}
	}

	public static PropertyResult? GetHitpointsFraction(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetHitpointsFractionValue(((MemoryFact)anObjectToGetValueFrom).Body);
	}

	public static PropertyResult? GetBulkForPresentation(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetItemBulkForPresentation((MemoryFact)anObjectToGetValueFrom);
	}

	public static PropertyResult? GetCondition(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetCondition((MemoryFact)anoObjectToGetValueFrom);
	}

	public static PropertyResult? GetConditionTooltip(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetConditionTooltip((MemoryFact)anoObjectToGetValueFrom);
	}

	public static PropertyResult? GetReplenishStatus(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return Entity.GetReplenishStatus(getterKnowledge, (IKnownEntityData)anObjectToGetValueFrom);
	}

	public static PropertyResult? GetReplenishStatusTooltip(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return Entity.GetReplenishStatusTooltip(getterKnowledge, (IKnownEntityData)anObjectToGetValueFrom);
	}

	public static PropertyResult? GetHomeComfortLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetHomeComfortLevel((MemoryFact)anoObjectToGetValueFrom);
	}

	public static PropertyResult? GetProgress(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((MemoryFact)anObjectToGetValueFrom).GetProgress();
	}

	private PropertyResult? GetProgress()
	{
		if (Progress.HasValue)
		{
			return new PropertyResult
			{
				NumberResult = Progress.Value
			};
		}
		return null;
	}

	public static PropertyResult? GetIntegrity(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return Entity.GetIntegrity((MemoryFact)anObjectToGetValueFrom);
	}

	public static PropertyResult? GetInaccessible(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((MemoryFact)anObjectToGetValueFrom).GetInaccessible();
	}

	public PropertyResult? GetInaccessible()
	{
		return GetInaccessibleStatus(EntityID);
	}

	public static PropertyResult GetInaccessibleStatus(EntityID entityID)
	{
		The.Client.GetFeedback(entityID, out var isInAccessible, out var isBlockedByThreat, out var isBlockedByBoldStance);
		PropertyResult result = default(PropertyResult);
		if (isBlockedByThreat || isBlockedByBoldStance)
		{
			result.NumberResult = 0.25f;
		}
		else if (isInAccessible)
		{
			result.NumberResult = 0.75f;
		}
		else
		{
			result.NumberResult = 0f;
		}
		return result;
	}

	public bool DeprecateIfNeeded(Entity detectingEntity, TerrainTile.DeprecateDistance? deprecateDistance)
	{
		if (!isDeprecated && (!deprecateDistance.HasValue || deprecateDistance == TerrainTile.DeprecateDistance.Near || EntityType.MemoryFactIsDeprecatedInstantly()))
		{
			IsDeprecated = true;
			if (!PartOfID.HasValue && detectingEntity != null && (EntityType.ItemType == null || !EntityType.Category.IsWaste))
			{
				string format = ((EntityType.IntelligenceType == null) ? "cannot see {0} where it used to be" : "no longer has {0} in view");
				The.Client.AddLogEvent(detectingEntity.Intelligence.Allegiance, The.Client.Log.GeneralEvent, detectingEntity, string.Format(format, EntityType.Name.ToLower(Config.Culture)));
			}
			return true;
		}
		return false;
	}

	public MemoryFactID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= MemoryFactID.Invalid)
		{
			throw new Exception("Astounding, MemoryFactID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public MemoryFactID SnapshotID(Snapshotter sn, MemoryFactID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MemoryFactID.Invalid)
		{
			LookUp<MemoryFact, MemoryFactID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MemoryFactID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<MemoryFact, MemoryFactID>.Remove(this);
	}

	void ILookUp<MemoryFact, MemoryFactID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MemoryFactID.First;
	}

	void ILookUp<MemoryFact, MemoryFactID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<MemoryFact, MemoryFactID>.Create();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		AccessPoint = sn.DoVector3Nullable(AccessPoint);
		ammoItems = sn.DoDictionary(ammoItems);
		AllegianceID = sn.DoEnumNullable(AllegianceID);
		DebugLog = (DebugLog)sn.DoISnapshot(DebugLog);
		Body = (Body)sn.DoISnapshot(Body);
		BoundingRadius3D = sn.DoFloat(BoundingRadius3D);
		Bulk = sn.DoFloat(Bulk);
		Condition = sn.DoDoubleNullable(Condition);
		ConditionChangeSpeed = sn.DoFloatNullable(ConditionChangeSpeed);
		Integrity = sn.DoFloatNullable(Integrity);
		ContainedBy = sn.DoEntityIDNullable(ContainedBy);
		ContainedEntitiesByType = sn.DoMultiMap(ContainedEntitiesByType);
		ContainedUpgrades = sn.DoDictionary(ContainedUpgrades);
		contains = sn.DoDictionary(contains);
		OfferedEntitiesByType = sn.DoMultiMap(OfferedEntitiesByType);
		CurrentMaximumSpeed = sn.DoFloatNullable(CurrentMaximumSpeed);
		EntityID = sn.DoEntityID(EntityID);
		EntityType = sn.DoGameData(EntityType);
		FacingNormal = sn.DoVector3(FacingNormal);
		flipHorizontally = sn.DoBool(flipHorizontally);
		CustomFields = sn.DoDictionary(CustomFields);
		fuel = sn.DoFloatNullable(fuel);
		FunctionalScore = sn.DoDoubleNullable(FunctionalScore);
		snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(GatheringSite);
		HasItemStorage = sn.DoBool(HasItemStorage);
		OwnedBy = sn.DoEnumNullable(OwnedBy);
		isCompleted = sn.DoBool(isCompleted);
		isStarted = sn.DoBoolNullable(isStarted);
		isDeprecated = sn.DoBool(isDeprecated);
		IsMoving = sn.DoBoolNullable(IsMoving);
		IsPrepared = sn.DoBoolNullable(IsPrepared);
		isWeatherProof = sn.DoBool(isWeatherProof);
		loadedVehicleSpeed = sn.DoFloat(loadedVehicleSpeed);
		location = sn.DoVector3Nullable(location);
		MapPosition = sn.DoPointNullable(MapPosition);
		Name = sn.DoString(Name);
		NoOfRounds = sn.DoInt32Nullable(NoOfRounds);
		notOnboardDrivenVehicle = sn.DoBool(notOnboardDrivenVehicle);
		NutrientBulkAmounts = sn.DoDictionary(NutrientBulkAmounts);
		Processes = sn.DoList(Processes);
		AvailableSharedSpecialActions = sn.DoList(AvailableSharedSpecialActions);
		IntrinsicWeapons = sn.DoDictionary(IntrinsicWeapons);
		Site = sn.DoEnumNullable(Site);
		PartIsBroken = sn.DoBool(PartIsBroken);
		PartOfID = sn.DoEnumNullable(PartOfID);
		PartIDs = sn.DoList(PartIDs);
		ParentEntityID = sn.DoEnumNullable(ParentEntityID);
		RootEntityID = sn.DoEnum(RootEntityID);
		progress = sn.DoFloatNullable(progress);
		Replenishes = sn.DoEntityIDNullable(Replenishes);
		UpgradeFor = sn.DoEntityIDNullable(UpgradeFor);
		Residents = sn.DoInt32Nullable(Residents);
		ComfortLevel = sn.DoFloatNullable(ComfortLevel);
		Households = sn.DoList(Households);
		Rotation = sn.DoFloat(Rotation);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotAllegianceID = sharedKnowledge.Allegiance.ID;
		}
		snapshotAllegianceID = sn.DoEnum(snapshotAllegianceID);
		assignedToJob = sn.DoEnumNullable(assignedToJob);
		Stance = sn.DoGameData(Stance);
		if (sn.mode != Snapshotter.Mode.Load && CasteType != null)
		{
			casteTypeKey = CasteType.KeyName;
		}
		casteTypeKey = sn.DoString(casteTypeKey);
		storageSpaces = sn.DoDictionary(storageSpaces);
		tradeOfferStorageSpaces = sn.DoDictionary(tradeOfferStorageSpaces);
		if (sn.mode != Snapshotter.Mode.Load && The.Client != null && Renderable != null)
		{
			snapshotRenderable = Renderable.GetFieldsToSnapshot();
		}
		snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);
		StoredPermanentlyIn = sn.DoStorageTargetNullable(StoredPermanentlyIn);
		StrengthRating = sn.DoFloatNullable(StrengthRating);
		SubstanceBulkAmounts = sn.DoDictionary(SubstanceBulkAmounts);
		snapshotThreatGroup = sn.SnapshotID<ThreatGroup, ThreatGroupID>(ThreatGroup);
		TimeStampInSecondsOfGameTime = sn.DoDouble(TimeStampInSecondsOfGameTime);
		ToBeDeletedOnTimeStampInSecondsOfGameTime = sn.DoDoubleNullable(ToBeDeletedOnTimeStampInSecondsOfGameTime);
		tooltipEntityData = (EntityTypeTooltipInstanceData)sn.DoISnapshot(tooltipEntityData);
		TopLeftMapPosition = sn.DoPointNullable(TopLeftMapPosition);
		TotalItemStorageCapacity = sn.DoFloatNullable(TotalItemStorageCapacity);
		TotalStored = sn.DoFloatNullable(TotalStored);
		repairPackage = (RepairPackage)sn.DoISnapshot(repairPackage);
		sn.Postpone(listOfCargoSlotsForUnloading);
		sn.Postpone(listOfCargoSlotsForLoading);
		sn.Postpone(freeDriversSlot);
		sn.Ignore(CasteType);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(sharedKnowledge);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroup);
		sharedKnowledge = LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID(snapshotAllegianceID).SharedKnowledge;
		if (snapshotGatheringSite.HasValue && snapshotGatheringSite.HasValue)
		{
			GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
		}
		if (Body != null)
		{
			Body.LoadPostProcess(sn);
		}
		GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
		if (tradeOfferStorageSpaces != null)
		{
			foreach (KeyValuePair<StorageCondition, Storage> tradeOfferStorageSpace in tradeOfferStorageSpaces)
			{
				tradeOfferStorageSpace.Value.LoadPostProcess(sn);
			}
		}
		if (storageSpaces != null)
		{
			foreach (KeyValuePair<StorageCondition, Storage> storageSpace in storageSpaces)
			{
				storageSpace.Value.LoadPostProcess(sn);
			}
		}
		if (EntityType.BiologicalType != null)
		{
			CasteType = EntityType.BiologicalType.Castes.FirstOrDefault((CasteType c) => c.KeyName.Equals(casteTypeKey));
		}
		showStatusRegulator = Entity.CreateShowStatusIconRegulator("MemoryFact", EntityType);
		if (snapshotRenderable != null)
		{
			Renderable = RenderableFactory.Produce(snapshotRenderable, this);
			Renderable.UpdateAnimationConditionState();
			Renderable.ComputeMatricesForDrawing();
		}
	}
}
