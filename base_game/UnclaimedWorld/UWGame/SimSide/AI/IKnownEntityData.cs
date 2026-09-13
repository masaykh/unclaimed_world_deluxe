using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI;

public interface IKnownEntityData : IHasExposedProperties
{
	EntityID EntityID { get; }

	Vector3? Location { get; }

	Vector3 PlaySiteLocation { get; }

	Vector3 RenderedLocation { get; }

	Point? MapPosition { get; }

	SiteID? Site { get; }

	Point? TopLeftMapPosition { get; }

	bool PartIsBroken { get; }

	float? Progress { get; }

	float Rotation { get; }

	Vector3 FacingNormal { get; }

	bool? IsMoving { get; }

	string Name { get; }

	EntityType EntityType { get; }

	CasteType CasteType { get; }

	AllegianceID? AllegianceID { get; }

	ThreatGroup ThreatGroup { get; }

	Dictionary<EntityType, EntityID> IntrinsicWeapons { get; }

	List<ProcessType> AvailableSharedSpecialActions { get; }

	List<SimProcessID> Processes { get; }

	EntityTypeTooltipInstanceData TooltipEntityData { get; }

	OwnerID? OwnedBy { get; set; }

	EntityID? ContainedBy { get; }

	StorageTarget? StoredPermanentlyIn { get; }

	EntityID? Replenishes { get; }

	EntityID? UpgradeFor { get; }

	Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; }

	bool HasItemStorage { get; }

	Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType { get; }

	List<EntityID> ContainedEntities { get; }

	Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType { get; }

	Dictionary<StorageCondition, Storage> StorageSpaces { get; }

	Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces { get; }

	float? TotalItemStorageCapacity { get; }

	float? TotalStored { get; }

	float? CurrentMaximumSpeed { get; }

	double? Condition { get; }

	float? ConditionChangeSpeed { get; }

	float? Integrity { get; }

	double? FunctionalScore { get; }

	bool FlipHorizontally { get; }

	float Bulk { get; }

	Body Body { get; }

	Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts { get; }

	float? StrengthRating { get; }

	StanceType Stance { get; }

	bool NotOnboardDrivenVehicle { get; }

	GatheringSite GatheringSite { get; }

	JobID? AssignedToJob { get; set; }

	Vector3? AccessPoint { get; }

	bool? IsPrepared { get; }

	int? NoOfRounds { get; }

	float BoundingRadius3D { get; }

	CompositeID? PartOfID { get; }

	List<EntityID> PartIDs { get; }

	EntityID? ParentEntityID { get; }

	EntityID RootEntityID { get; }

	Dictionary<FoodNutrientType, float> NutrientBulkAmounts { get; }

	int? Residents { get; set; }

	float? ComfortLevel { get; }

	List<HouseholdID> Households { get; }

	EntityAndRoot GetAsEntityAndRoot();

	bool IsTimeToShowStatusMarkerWindow();

	bool ContainsEntity(EntityID entity);

	Storage FindStorage(StorageID storageID);

	bool IsTradeOfferStorage(StorageID storageID);

	float CalculateSpeed(float bulk);

	PassengerOrCargoSlot GetFreeDriversSlot();

	List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad);

	List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad);

	bool HasEnoughFuel(float neededFuel);

	bool HasEnergyForDuration(float durationInDays);

	bool HasEnoughAmmo(EntityType ammoType, int amount);

	bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload);

	bool NeedsRepair();

	RepairPackage ComputeBestRepairPackage();

	int? GetTotalAmmo();

	float GetRepairProgress(RepairAction repairAction);

	string GetDisplayName();

	bool IsCompleted();

	bool? IsStarted();

	bool IsWeatherProof();

	bool CanBeHunted(Allegiance byAllegiance);

	bool CanSetStockpileSettings(EntityGroupID byOwner);

	bool CanSetTradeOfferSettings(EntityGroupID byOwner);

	bool CanBeUpgraded(EntityGroupID byOwner);

	void ChangeOwnership(IOwner newOwner, Entity.GiveNewOwnerKnowledge giveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes);

	bool IsUnassigned(SharedKnowledge sharedKnowledge);

	bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge);

	bool CanBeHauled();

	bool IsItemValidForHauling(Entity entity, Intelligence entityIntelligence, HaulingJob job);

	bool IsVehicleValidForHauling(Entity entity, IKnownEntityData item);
}
