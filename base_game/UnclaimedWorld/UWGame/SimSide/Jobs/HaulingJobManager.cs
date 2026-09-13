using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Jobs;

public class HaulingJobManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		CreateAllItemsList,
		GatherAllStorageLocations,
		CreateCombos,
		ScoreCombos,
		CreateHaulingJobs
	}

	public enum Result
	{
		Wait,
		OK
	}

	[DebuggerDisplay("{NormalStorage}, {GroundLocation}, Zone: {Zone}")]
	private class StorageLocation
	{
		public StorageTarget? NormalStorage;

		public StorageTarget? TradeOfferStorage;

		public Stockpile Stockpile;

		public Zone Zone;

		public float TotalCapacity;

		public float TotalStored;

		public Dictionary<EntityType, Pair<int, int>> TotalStoredItems;

		public Vector3 GroundLocation;

		public double TravelTimeToCenterScore;

		public StorageTarget? GetStorageTarget => NormalStorage ?? TradeOfferStorage;

		public bool StoredItemsAreAtMaximum(EntityType itemType, ref Pair<int, int> limit)
		{
			if (TotalStoredItems != null && TotalStoredItems.TryGetValue(itemType, out limit))
			{
				return limit.First >= limit.Second;
			}
			return false;
		}
	}

	[DebuggerDisplay("{Item} {StorageLocation}")]
	private class ItemStorageCombo
	{
		public EntityID Item;

		private StorageLocation location;

		public double Score;

		public float DamageScore;

		public StorageLocation StorageLocation
		{
			get
			{
				return location;
			}
			set
			{
				location = value;
			}
		}
	}

	private Regulator regulator;

	private static float oneOverBaseSpeedOnFoot;

	private static float oneOverBaseSpeedByAir;

	private static float takeOffAndLandTime;

	private double airliftCapacityScore;

	private Phase phase = Phase.CreateCombos;

	private int itemCounter;

	private const int itemsPerCycle = 30;

	private int comboCounter;

	private const int combosPerCycle = 30;

	private List<StorageLocation> AllStorageLocations = new List<StorageLocation>();

	private List<IKnownEntityData> allItems;

	private List<ItemStorageCombo> allCombos = new List<ItemStorageCombo>();

	private Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores = new Dictionary<EntityType, Dictionary<StorageLocation, float>>();

	private Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores = new Dictionary<Point, Dictionary<Point, double>>();

	private MethodID notifyWhenRegionSearchIsFinished;

	private EntityGroup parent;

	private EntityGroupID snapshotParent;

	private Point? storageCenterMapPosition;

	private List<HaulingJob> outdatedJobs = new List<HaulingJob>();

	public static double totalComputationAllInstancesInSeconds;

	private bool isWaiting;

	private Dictionary<EntityID, ItemStorageCombo> assignedItems = new Dictionary<EntityID, ItemStorageCombo>();

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double? UpdateInterval => 10.0;

	public bool IsPaused => isWaiting;

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public CyclableID ID
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

	public bool UnregisterBeforeSnapshot => true;

	public HaulingJobManager()
	{
	}

	public HaulingJobManager(EntityGroup owner)
	{
		AddToLookup();
		parent = owner;
		notifyWhenRegionSearchIsFinished = ActionLookup.AddWithNewID(NotifyWhenRegionSearchIsFinished);
		oneOverBaseSpeedOnFoot = 1f / (GameData.Instance.AllEntityTypes["entity:human"].LocomotorType.LeggedLocomotorType.WalkNormalSpeed * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None));
		if (GameData.Instance.AllEntityTypes.TryGetValue("entity:skimmer", out var value))
		{
			oneOverBaseSpeedByAir = 1f / value.LocomotorType.LeggedLocomotorType.WalkNormalSpeed;
			if (value.ContainerType == null)
			{
				takeOffAndLandTime = 17f;
			}
			else
			{
				takeOffAndLandTime = 2f * ((VehicleContainerType)value.ContainerType).Aircraft.EstimatedTakeOffLandingTime;
			}
		}
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "HaulingJobManager");
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				phase = Phase.CreateAllItemsList;
				itemCounter = 0;
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
			}
		}
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"HaulingJobManager {ID}: {phase}");
	}

	public bool CycleOnce()
	{
		switch (phase)
		{
		case Phase.CreateAllItemsList:
			if (CreateAllItemsList())
			{
				if (allItems.Count <= 0)
				{
					return true;
				}
				phase = Phase.GatherAllStorageLocations;
			}
			return false;
		case Phase.GatherAllStorageLocations:
			if (GatherAllStorageLocations(parent, AllStorageLocations, out storageCenterMapPosition))
			{
				if (!storageCenterMapPosition.HasValue)
				{
					return true;
				}
				phase = Phase.CreateCombos;
			}
			return false;
		case Phase.CreateCombos:
			if (CreateAllCombos())
			{
				airliftCapacityScore = CalculateAirliftCapacity(parent);
				cachedItemLocationScores.Clear();
				cachedItemStorageScores.Clear();
				comboCounter = 0;
				phase = Phase.ScoreCombos;
			}
			return false;
		case Phase.ScoreCombos:
			if (ScoreAllCombos())
			{
				phase = Phase.CreateHaulingJobs;
				assignedItems.Clear();
				comboCounter = 0;
			}
			return false;
		case Phase.CreateHaulingJobs:
			if (CreateAllHaulingJobs())
			{
				return true;
			}
			return false;
		default:
			return true;
		}
	}

	private static Result ScoreCombo(RegionMap regionMap, Allegiance allegiance, double airliftCapacityScore, Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores, Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores, MethodID? notifyWhenFinished, ItemStorageCombo combo, IKnownEntityData itemData, out double score, out float damageScore, bool getPathsNow)
	{
		score = 0.0;
		damageScore = 1f;
		float num = ScoreStockpileSettings(combo, itemData);
		if (itemData.EntityID == (EntityID)12L)
		{
			_ = combo.StorageLocation.NormalStorage.HasValue;
		}
		if (num > -1f)
		{
			damageScore = ScoreItemStorage(combo, itemData, cachedItemStorageScores, allegiance.SharedKnowledge);
			if (damageScore > -1f)
			{
				if (itemData.EntityID == (EntityID)3L)
				{
					_ = -1f;
				}
				if (ScoreItemLocation(regionMap, allegiance, combo, itemData, airliftCapacityScore, cachedItemLocationScores, notifyWhenFinished, out var result, getPathsNow) == Result.Wait)
				{
					score = 0.0;
					return Result.Wait;
				}
				if (combo.StorageLocation != null && combo.StorageLocation.TradeOfferStorage.HasValue)
				{
					_ = itemData.EntityID;
					_ = 24633;
					score = 0.5 * (double)damageScore + 0.15 * result + 0.1 * (double)num + 10.0;
				}
				else
				{
					score = 0.7 * (double)damageScore + 0.2 * result + 0.1 * (double)num;
				}
			}
		}
		return Result.OK;
	}

	private static float ScoreStockpileSettings(ItemStorageCombo combo, IKnownEntityData itemData)
	{
		if (combo.StorageLocation.Stockpile != null)
		{
			if (!combo.StorageLocation.Stockpile.MayStockpile(itemData.EntityType, out var _))
			{
				return -1f;
			}
			return 1f;
		}
		return 0.1f;
	}

	private static float ScoreNearnessToConsumers(IKnownEntityData item)
	{
		return 0f;
	}

	private static bool IsCurrentlyInThisStorage(StorageLocation storageLocation, IKnownEntityData itemData)
	{
		try
		{
			if ((storageLocation.NormalStorage.HasValue && storageLocation.NormalStorage == itemData.StoredPermanentlyIn) || (storageLocation.TradeOfferStorage.HasValue && storageLocation.TradeOfferStorage == itemData.StoredPermanentlyIn))
			{
				return true;
			}
			if (!storageLocation.NormalStorage.HasValue && !storageLocation.TradeOfferStorage.HasValue)
			{
				if (itemData.ContainedBy.HasValue)
				{
					return false;
				}
				if (storageLocation.Zone != null)
				{
					TerrainTile tile = The.Map.GetTile(itemData.MapPosition.Value);
					if (tile.Zones != null)
					{
						foreach (KeyValuePair<Allegiance, List<Zone>> zone in tile.Zones)
						{
							if (zone.Value.Exists((Zone z) => z == storageLocation.Zone))
							{
								return true;
							}
						}
					}
				}
				else if (MapManager.WorldPosToTile(storageLocation.GroundLocation) == MapManager.WorldPosToTile(itemData.PlaySiteLocation))
				{
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message + Entity.GetExceptionInformation(itemData));
		}
		return false;
	}

	private static Result ScoreItemLocation(RegionMap regionMap, Allegiance allegiance, ItemStorageCombo combo, IKnownEntityData itemData, double airliftCapacityScore, Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores, MethodID? notifyWhenFinished, out double result, bool getPathsNow)
	{
		if (IsCurrentlyInThisStorage(combo.StorageLocation, itemData))
		{
			result = 1.0;
			return Result.OK;
		}
		float distance = 0f;
		Point value = itemData.MapPosition.Value;
		Point point = MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation);
		if (cachedItemLocationScores.TryGetValue(value, out var value2) && value2.TryGetValue(point, out var value3))
		{
			result = value3;
			return Result.OK;
		}
		EntityID? toEntityID = null;
		Point? endingSubtile = null;
		if (combo.StorageLocation.GetStorageTarget.HasValue)
		{
			toEntityID = combo.StorageLocation.GetStorageTarget.Value.StorageEntity;
		}
		else
		{
			endingSubtile = MapManager.WorldPosToSubtile(combo.StorageLocation.GroundLocation);
		}
		EntityResult fromEntityKnowledgeResult;
		EntityResult toEntityKnowledgeResult;
		RegionMap.Result distanceToEntityUsingEntityType = regionMap.GetDistanceToEntityUsingEntityType(combo.Item, toEntityID, allegiance.SharedKnowledge, ref distance, allegiance, allegiance.RepresentativeEntityType, out fromEntityKnowledgeResult, out toEntityKnowledgeResult, null, endingSubtile, sendMessageToEntity: false, notifyWhenFinished);
		if (fromEntityKnowledgeResult == EntityResult.Destroyed || fromEntityKnowledgeResult == EntityResult.EntityStatusIsNowUnknown || toEntityKnowledgeResult == EntityResult.Destroyed || toEntityKnowledgeResult == EntityResult.EntityStatusIsNowUnknown)
		{
			result = ScoreItemLocationFinally(0.0, combo);
			return Result.OK;
		}
		switch (distanceToEntityUsingEntityType)
		{
		case RegionMap.Result.Wait:
			result = ScoreItemLocationFinally(0.0, combo);
			return Result.Wait;
		case RegionMap.Result.NoAccess:
			if (airliftCapacityScore == 0.0)
			{
				result = ScoreItemLocationFinally(0.0, combo);
				return Result.OK;
			}
			break;
		}
		double num = GoalEvaluator.ScoreTravelTime(oneOverBaseSpeedOnFoot * distance);
		if (airliftCapacityScore > 0.0)
		{
			float num2 = Common.DistanceOctile(MapManager.TileToWorldPos(value), MapManager.TileToWorldPos(point));
			double num3 = GoalEvaluator.ScoreTravelTime(oneOverBaseSpeedByAir * num2 + takeOffAndLandTime);
			double num4 = 0.8 * airliftCapacityScore;
			result = (1.0 - num4) * num + num4 * num3;
		}
		else
		{
			result = num;
		}
		result = ScoreItemLocationFinally(result, combo);
		if (!cachedItemLocationScores.TryGetValue(value, out value2))
		{
			value2 = new Dictionary<Point, double>();
			cachedItemLocationScores.Add(value, value2);
		}
		value2[point] = result;
		return Result.OK;
	}

	private static double ScoreItemLocationFinally(double score, ItemStorageCombo combo)
	{
		return 0.8 * score + 0.2 * combo.StorageLocation.TravelTimeToCenterScore;
	}

	public static double CalculateAirliftCapacity(EntityGroup owner)
	{
		float num = 0f;
		SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
		for (int num2 = owner.Vehicles.Count - 1; num2 >= 0; num2--)
		{
			EntityID entityID = owner.Vehicles[num2];
			if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, owner, out var entityData))
			{
				double? condition = entityData.Condition;
				double num3 = 0.1;
				if (condition.GetValueOrDefault() > num3 && condition.HasValue && ((VehicleContainerType)entityData.EntityType.ContainerType).Aircraft != null)
				{
					num += entityData.TotalItemStorageCapacity.Value;
				}
			}
		}
		return Math.Pow(Common.ClampTop((double)num / 20.0, 1.0), 2.0);
	}

	public static double EstimateAirTime(Entity vehicle, ref Point destination, ref Vector3 knownVehicleLocation, float estimatedLoadedVehicleSpeed, ref Vector3 knownItemLocation)
	{
		return (double)(Common.DistanceOctile(knownVehicleLocation, knownItemLocation) / vehicle.Locomotor.CurrentMaximumSpeedForEvaluator + Common.DistanceOctile(knownItemLocation, MapManager.TileToWorldPos(destination)) / estimatedLoadedVehicleSpeed) + (double)(2f * ((VehicleContainerType)vehicle.EntityType.ContainerType).Aircraft.EstimatedTakeOffLandingTime);
	}

	private static float ScoreItemStorage(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores, SharedKnowledge sharedKnowledge)
	{
		if (cachedItemStorageScores.TryGetValue(itemData.EntityType, out var value) && value.TryGetValue(combo.StorageLocation, out var value2))
		{
			return value2;
		}
		StorageTarget? getStorageTarget = combo.StorageLocation.GetStorageTarget;
		float score;
		if (getStorageTarget.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(getStorageTarget.Value.StorageEntity, out var data)))
			{
				return -1f;
			}
			if (Entity.IsFunctional(data))
			{
				Storage storage = data.FindStorage(getStorageTarget.Value.StorageID);
				if (storage == null)
				{
					return -1f;
				}
				ScoreIndoorStorage(combo, itemData, data, storage, out score);
			}
			else
			{
				ScoreOutdoorsStorage(combo, itemData, out score);
			}
		}
		else
		{
			ScoreOutdoorsStorage(combo, itemData, out score);
		}
		if (!cachedItemStorageScores.TryGetValue(itemData.EntityType, out value))
		{
			value = new Dictionary<StorageLocation, float>();
			cachedItemStorageScores.Add(itemData.EntityType, value);
		}
		value[combo.StorageLocation] = score;
		return score;
	}

	private static void ScoreIndoorStorage(ItemStorageCombo combo, IKnownEntityData itemData, IKnownEntityData storageEntity, Storage storage, out float score)
	{
		if (itemData.EntityType.ItemType.RequiredStorageTypesFinal != null && !itemData.EntityType.ItemType.RequiredStorageTypesFinal.Contains(storageEntity.EntityType))
		{
			score = -1f;
			return;
		}
		float d = NonLivingEntity.ComputeDegradeDamage(itemData, storage.StorageConditions, storage.IsPowered, MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation), 1.0);
		d = Common.ClampTop(d, 1f);
		float verminProtectionScore = ScoreVerminProtection(itemData, storageEntity);
		float conditionScore = 1f - d;
		score = CombineScore(conditionScore, verminProtectionScore);
	}

	private static void ScoreOutdoorsStorage(ItemStorageCombo combo, IKnownEntityData itemData, out float score)
	{
		ScoreOutdoorsStorage(combo.StorageLocation.GroundLocation, itemData, out score);
	}

	private static void ScoreOutdoorsStorage(Vector3 location, IKnownEntityData itemData, out float score)
	{
		if (itemData.EntityType.ItemType.RequiredStorageTypesFinal != null)
		{
			score = -1f;
			return;
		}
		float d = NonLivingEntity.ComputeDegradeDamage(itemData, null, null, MapManager.WorldPosToTile(location), 1.0);
		d = Common.ClampTop(d, 1f);
		float conditionScore = 1f - d;
		float verminProtectionScore = ScoreVerminProtection(itemData, null);
		score = CombineScore(conditionScore, verminProtectionScore);
	}

	private static float ScoreVerminProtection(IKnownEntityData itemData, IKnownEntityData storageEntity)
	{
		if (itemData.EntityType.ItemType != null && itemData.EntityType.ItemType.FoodType != null && (storageEntity == null || storageEntity.EntityType.ContainerType.VerminCanAccess))
		{
			return 0f;
		}
		return 1f;
	}

	private static float CombineScore(float conditionScore, float verminProtectionScore)
	{
		return 0.8f * conditionScore + 0.2f * verminProtectionScore;
	}

	private static bool StorageHasCapacity(StorageLocation storageLocation, IKnownEntityData item, ref Pair<int, int> limit)
	{
		if (storageLocation.TotalStored + item.Bulk < storageLocation.TotalCapacity)
		{
			if (storageLocation.StoredItemsAreAtMaximum(item.EntityType, ref limit))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CreateAllItemsList()
	{
		if (allItems == null)
		{
			allItems = new List<IKnownEntityData>();
		}
		allItems.Clear();
		if (parent.GetAllegiance() == null)
		{
			return true;
		}
		SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;
		foreach (KeyValuePair<EntityType, List<EntityID>> item in parent.Items)
		{
			bool flag = !ItemTypeIsNeededForProcess(parent, item.Key);
			for (int num = item.Value.Count - 1; num >= 0; num--)
			{
				EntityID entityID = item.Value[num];
				if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, parent, out var entityData))
				{
					if (entityData.AssignedToJob.HasValue && LookUp<Job, JobID>.FindByID(entityData.AssignedToJob) == null)
					{
						entityData.AssignedToJob = null;
					}
					if (flag)
					{
						allItems.Add(entityData);
					}
				}
			}
		}
		return true;
	}

	private static bool GatherAllStorageLocations(EntityGroup storageLocationsGroup, List<StorageLocation> allStorageLocations, out Point? storageCenterMapPosition)
	{
		SharedKnowledge sharedKnowledge = storageLocationsGroup.GetAllegiance().SharedKnowledge;
		allStorageLocations.Clear();
		storageCenterMapPosition = null;
		storageCenterMapPosition = MapManager.WorldPosToTile(storageLocationsGroup.Parent.Location.Value);
		IKnownEntityData data = null;
		Household household = storageLocationsGroup.Parent as Household;
		if (household != null)
		{
			if (!household.Home.HasValue)
			{
				storageCenterMapPosition = null;
			}
		}
		else if (storageLocationsGroup.Parent is Person person)
		{
			if (person.Household.Home.HasValue && GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(household.Home.Value, out data)))
			{
				person.Household.Home = null;
			}
			if (data != null)
			{
				storageCenterMapPosition = data.MapPosition;
			}
			else
			{
				storageCenterMapPosition = null;
			}
		}
		foreach (KeyValuePair<EntityType, List<EntityID>> structure in storageLocationsGroup.Structures)
		{
			for (int num = structure.Value.Count - 1; num >= 0; num--)
			{
				if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, structure.Value[num], storageLocationsGroup, out var entityData))
				{
					HandleStructureForStorage(storageLocationsGroup, allStorageLocations, storageCenterMapPosition, entityData);
				}
			}
		}
		if (data != null)
		{
			bool flag = false;
			if (storageLocationsGroup.Structures.TryGetValue(data.EntityType, out var value) && value.Contains(household.Home.Value))
			{
				flag = true;
			}
			if (!flag)
			{
				HandleStructureForStorage(storageLocationsGroup, allStorageLocations, storageCenterMapPosition, data);
			}
		}
		if (!storageCenterMapPosition.HasValue && allStorageLocations.Count > 0)
		{
			storageCenterMapPosition = MapManager.WorldPosToTile(allStorageLocations[0].GroundLocation);
		}
		if (storageCenterMapPosition.HasValue)
		{
			GatherGroundStorageLocations(GameData.Instance.Constants.ExpeditionStorageRadius, storageLocationsGroup, allStorageLocations, storageCenterMapPosition);
		}
		foreach (Zone zone in storageLocationsGroup.Zones)
		{
			if (zone.Stockpile != null)
			{
				TerrainTile first = zone.MapArea.GetFirst();
				Point point = new Point(first.X, first.Y);
				allStorageLocations.Add(new StorageLocation
				{
					GroundLocation = MapManager.TileToWorldPos(point),
					TotalStored = 0f,
					TotalCapacity = zone.GetBulkCapacity(),
					TravelTimeToCenterScore = ScoreTravelTimeToCenter(point, storageCenterMapPosition),
					Stockpile = zone.Stockpile,
					TotalStoredItems = ((zone.Stockpile != null) ? CreateItemCounters(zone.Stockpile.GetMaxLimits()) : null),
					Zone = zone
				});
			}
		}
		return true;
	}

	private static void HandleStructureForStorage(EntityGroup owner, List<StorageLocation> allStorageLocations, Point? storageCenterMapPosition, IKnownEntityData entityData)
	{
		if (!entityData.HasItemStorage || !Entity.IsPermanentStorage(entityData.EntityType) || !entityData.IsCompleted() || !GoalEvaluator.IsOnPlaySite(entityData) || !Entity.IsFunctional(entityData))
		{
			return;
		}
		owner.StructureStockpiles.TryGetValue(entityData.EntityID, out var value);
		foreach (KeyValuePair<StorageCondition, Storage> storageSpace in entityData.StorageSpaces)
		{
			Storage value2 = storageSpace.Value;
			AddStorageSpace(allStorageLocations, storageCenterMapPosition, entityData, value, value2, new StorageTarget(entityData.EntityID, value2.ID), null);
		}
		Dictionary<StorageCondition, Storage> tradeOffersStorageSpaces = entityData.TradeOffersStorageSpaces;
		if (tradeOffersStorageSpaces == null)
		{
			return;
		}
		owner.TerminalTradeOffers.TryGetValue(entityData.EntityID, out var value3);
		foreach (KeyValuePair<StorageCondition, Storage> item in tradeOffersStorageSpaces)
		{
			Storage value4 = item.Value;
			AddStorageSpace(allStorageLocations, storageCenterMapPosition, entityData, value3, value4, null, new StorageTarget(entityData.EntityID, value4.ID));
		}
	}

	private static void AddStorageSpace(List<StorageLocation> allStorageLocations, Point? storageCenterMapPosition, IKnownEntityData entityData, Stockpile stockpile, Storage storage, StorageTarget? normalStorage, StorageTarget? tradeOfferStorage)
	{
		_ = entityData.EntityID;
		_ = 3970;
		allStorageLocations.Add(new StorageLocation
		{
			NormalStorage = normalStorage,
			TradeOfferStorage = tradeOfferStorage,
			TotalStored = 0f,
			TotalCapacity = storage.TotalCapacity,
			GroundLocation = entityData.PlaySiteLocation,
			Stockpile = stockpile,
			TotalStoredItems = ((stockpile != null) ? CreateItemCounters(stockpile.GetMaxLimits()) : null),
			TravelTimeToCenterScore = ScoreTravelTimeToCenter(entityData.MapPosition.Value, storageCenterMapPosition)
		});
	}

	private static Dictionary<EntityType, Pair<int, int>> CreateItemCounters(Dictionary<EntityType, int> maxLimits)
	{
		Dictionary<EntityType, Pair<int, int>> list = null;
		if (maxLimits != null && maxLimits.Count > 0)
		{
			foreach (KeyValuePair<EntityType, int> maxLimit in maxLimits)
			{
				Common.AddToDictionary(ref list, maxLimit.Key, new Pair<int, int>(0, maxLimit.Value));
			}
		}
		return list;
	}

	private static void GatherGroundStorageLocations(int radius, EntityGroup owner, List<StorageLocation> allStorageLocations, Point? storageCenterMapPosition)
	{
		MapManager.GetClampedMapAreaUsingTiles(new TilePos(storageCenterMapPosition.Value.X, storageCenterMapPosition.Value.Y), radius, out var minX, out var maxX, out var minY, out var maxY);
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		owner.GetAllegiance();
		for (int i = minX; i < maxX; i++)
		{
			for (int j = minY; j < maxY; j++)
			{
				Point point = new Point(i, j);
				Point point2 = MapManager.TileCenterToSubTile(point);
				if (!The.Map.TileIsCompletelyBlocked(mapCosts, point) && !The.Map.SubtileIsCompletelyBlocked(mapCosts, point2) && !The.Map.FlagIsSet(point2, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved))
				{
					TerrainTile tile = The.Map.GetTile(point);
					if (!tile.Owner.HasValue || tile.Owner == owner.ID)
					{
						allStorageLocations.Add(new StorageLocation
						{
							GroundLocation = MapManager.TileToWorldPos(point),
							TotalStored = 0f,
							TotalCapacity = GameData.Instance.Constants.BulkCapacityForSingleTile,
							TravelTimeToCenterScore = ScoreTravelTimeToCenter(point, storageCenterMapPosition)
						});
					}
				}
			}
		}
	}

	public static double ScoreTravelTimeToCenter(Point mapPos, Point? centerMapPosition)
	{
		float num = 48f * Common.DistanceOctile(centerMapPosition.Value, mapPos);
		return GoalEvaluator.ScoreTravelTime(oneOverBaseSpeedOnFoot * num);
	}

	private bool CreateAllCombos()
	{
		int num = itemCounter;
		int num2 = Common.Min(itemCounter + 30, allItems.Count);
		if (itemCounter == 0)
		{
			allCombos.Clear();
		}
		parent.GetAllegiance();
		for (int i = num; i < num2 && i < allItems.Count; i++)
		{
			IKnownEntityData knownEntityData = allItems[i];
			_ = knownEntityData.EntityID;
			_ = 4555;
			CreateCombosForItem(AllStorageLocations, allCombos, knownEntityData);
		}
		itemCounter = num2;
		if (num2 == allItems.Count)
		{
			itemCounter = 0;
			allItems.Clear();
			phase = Phase.ScoreCombos;
			return true;
		}
		return false;
	}

	private static bool ItemAssignmentOK(IKnownEntityData item)
	{
		if (item.AssignedToJob.HasValue)
		{
			Job job = null;
			HaulingJob haulingJob = null;
			if (item.AssignedToJob.HasValue)
			{
				job = LookUp<Job, JobID>.FindByID(item.AssignedToJob.Value);
				if (job == null)
				{
					item.AssignedToJob = null;
				}
				else
				{
					haulingJob = job as HaulingJob;
				}
			}
			if (haulingJob == null)
			{
				return false;
			}
			if (haulingJob.RequiredByProcessJob != null)
			{
				return false;
			}
		}
		return true;
	}

	private static bool ItemIsOK(IKnownEntityData item)
	{
		if (item.CanBeHauled())
		{
			return ItemAssignmentOK(item);
		}
		return false;
	}

	private static void CreateCombosForItem(List<StorageLocation> allStorageLocations, List<ItemStorageCombo> allCombos, IKnownEntityData item)
	{
		if (!ItemIsOK(item))
		{
			return;
		}
		foreach (StorageLocation allStorageLocation in allStorageLocations)
		{
			allCombos.Add(new ItemStorageCombo
			{
				Item = item.EntityID,
				StorageLocation = allStorageLocation
			});
		}
	}

	private static bool IsComboValid(EntityGroup entityGroup, SharedKnowledge sharedKnowledge, List<ItemStorageCombo> combos, ref int index, out ItemStorageCombo combo, out IKnownEntityData itemData)
	{
		combo = combos[index];
		if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(combo.Item, out itemData)))
		{
			combos.RemoveAt(index);
			index--;
			return false;
		}
		if (itemData.EntityType.KeyName.ToLower().Contains("vinegar"))
		{
			_ = itemData.ContainedBy.HasValue;
		}
		if (ItemTypeIsNeededForProcess(entityGroup, itemData.EntityType))
		{
			combos.RemoveAt(index);
			index--;
			return false;
		}
		return true;
	}

	private bool ScoreAllCombos()
	{
		double score = -1.0;
		float damageScore = 0f;
		int num = comboCounter;
		int num2 = Common.Min(comboCounter + 30, allCombos.Count);
		Allegiance allegiance = parent.GetAllegiance();
		RegionMap regionMapToUse = GetRegionMapToUse(allegiance);
		for (int i = num; i < num2 && i < allCombos.Count; i++)
		{
			if (IsComboValid(parent, allegiance.SharedKnowledge, allCombos, ref i, out var combo, out var itemData))
			{
				if (ScoreCombo(regionMapToUse, allegiance, airliftCapacityScore, cachedItemStorageScores, cachedItemLocationScores, notifyWhenRegionSearchIsFinished, combo, itemData, out score, out damageScore, getPathsNow: false) != Result.OK)
				{
					isWaiting = true;
					return false;
				}
				float currentHaulTargetScore = GetCurrentHaulTargetScore(combo);
				score = (combo.Score = score + (double)(0.1f * currentHaulTargetScore));
				combo.DamageScore = damageScore;
			}
		}
		comboCounter = num2;
		if (num2 == allCombos.Count)
		{
			SortCombos(allCombos);
			return true;
		}
		return false;
	}

	private static RegionMap GetRegionMapToUse(Allegiance allegiance)
	{
		return allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, allegiance.RepresentativeEntityType, ThreatStance.Normal).Layers[SurfaceType.TransportType.Foot].RegionMap;
	}

	private static void SortCombos(List<ItemStorageCombo> combos)
	{
		combos.RemoveAll((ItemStorageCombo c) => Common.IsZero(c.Score));
		combos.Sort((ItemStorageCombo a, ItemStorageCombo b) => b.Score.CompareTo(a.Score));
	}

	public void NotifyWhenRegionSearchIsFinished()
	{
		isWaiting = false;
	}

	private static bool ItemTypeIsNeededForProcess(EntityGroup entityGroup, EntityType entityType)
	{
		List<ProcessJob> value2;
		if (entityGroup.HaulingJobsAnyItemOfType.TryGetValue(entityType, out var value) && value.Count > 0)
		{
			if (value.Exists((HaulingJobAnyItemOfType h) => !h.Item.HasValue))
			{
				return true;
			}
		}
		else if (entityGroup.ProductionJobsByInput != null && entityGroup.ProductionJobsByInput.TryGetValue(entityType, out value2))
		{
			for (int num = value2.Count - 1; num >= 0; num--)
			{
				ProcessJob processJob = value2[num];
				if (processJob.GetCurrentJobLocation(out var location, out var processData) && !location.HasValue && !processData.HasFixedLocation() && processJob.ProcessType.NeedsImmovableInput())
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CleanupHaulingJobs()
	{
		Allegiance allegiance = parent.GetAllegiance();
		for (int i = 0; i < parent.HaulingJobs.Count; i++)
		{
			Job job = parent.HaulingJobs[i];
			if (!(job is HaulingJob { Item: not null } haulingJob))
			{
				continue;
			}
			IKnownEntityData data;
			EntityResult knownData = allegiance.SharedKnowledge.GetKnownData(haulingJob.Item.Value, out data);
			if (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown)
			{
				RecordOutdatedJob(ref outdatedJobs, haulingJob);
				continue;
			}
			EntityID? inUseBy = allegiance.SharedKnowledge.GetInUseBy(data.EntityID);
			if (!data.CanBeHauled() || (job.TakenBy.Count == 0 && ((data.AssignedToJob.HasValue && data.AssignedToJob != haulingJob.ID) || inUseBy.HasValue)))
			{
				RecordOutdatedJob(ref outdatedJobs, haulingJob);
			}
			else if (!data.OwnedBy.HasValue)
			{
				RecordOutdatedJob(ref outdatedJobs, haulingJob);
			}
			else if (job is HaulingJobSpecificItem { IsHaulJobToStorage: not false } haulingJobSpecificItem && (ItemTypeIsNeededForProcess(parent, data.EntityType) || !Entity.StorageHasRoomForItem(allegiance.SharedKnowledge, haulingJobSpecificItem, data)))
			{
				RecordOutdatedJob(ref outdatedJobs, haulingJob);
			}
		}
		return true;
	}

	private void DestroyJobs()
	{
		if (outdatedJobs == null)
		{
			return;
		}
		for (int i = 0; i < outdatedJobs.Count; i++)
		{
			HaulingJob haulingJob = outdatedJobs[i];
			if (haulingJob is HaulingJobAnyItemOfType anyItemJob)
			{
				ReplaceHaulingJobAnyItem(haulingJob, anyItemJob);
			}
			else
			{
				haulingJob.Destroy(cancelTakers: true);
			}
		}
	}

	private static void ReplaceHaulingJobAnyItem(HaulingJob job, HaulingJobAnyItemOfType anyItemJob)
	{
		job.Destroy(cancelTakers: true);
		ProcessJob requiredByProcessJob = anyItemJob.RequiredByProcessJob;
		if (requiredByProcessJob.IsStarted(out var isStarted) && !isStarted)
		{
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(anyItemJob.EntityGroupID);
			if (entityGroup != null)
			{
				new HaulingJobAnyItemOfType(anyItemJob.ToLocation.Value, entityGroup, anyItemJob.RequiredItemType, anyItemJob.ItemsToHaulGroup, anyItemJob.NewOwner)
				{
					RequiredByProcessJob = requiredByProcessJob,
					Priority = anyItemJob.Priority
				};
			}
		}
	}

	private static void RecordOutdatedJob(ref List<HaulingJob> outdatedJobs, HaulingJob job)
	{
		if (outdatedJobs != null)
		{
			outdatedJobs.Add(job);
			return;
		}
		outdatedJobs = new List<HaulingJob>();
		outdatedJobs.Add(job);
	}

	private bool CreateAllHaulingJobs()
	{
		int num = comboCounter;
		int num2 = Common.Min(comboCounter + 30, allCombos.Count);
		SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;
		for (int i = num; i < num2 && i < allCombos.Count; i++)
		{
			ItemStorageCombo combo = allCombos[i];
			if (IsComboValid(parent, sharedKnowledge, allCombos, ref i, out combo, out var itemData))
			{
				if (itemData.EntityType.KeyName.ToLower().Contains("vinegar"))
				{
					_ = itemData.ContainedBy.HasValue;
				}
				AssignItemToStorage(combo, itemData, assignedItems);
			}
		}
		comboCounter = num2;
		if (comboCounter == allCombos.Count)
		{
			EndCreateAllHaulingJobs();
			return true;
		}
		return false;
	}

	private void EndCreateAllHaulingJobs()
	{
		SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;
		outdatedJobs.Clear();
		CleanupHaulingJobs();
		DestroyJobs();
		CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(sharedKnowledge, assignedItems, parent);
		if (assignedItems.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<EntityID, ItemStorageCombo> assignedItem in assignedItems)
		{
			DestroyHaulingJobsForItem(assignedItem.Key, parent);
		}
		if (assignedItems.Count <= 0)
		{
			return;
		}
		Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData = new Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>>();
		foreach (KeyValuePair<EntityID, ItemStorageCombo> assignedItem2 in assignedItems)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(assignedItem2.Key, out var data)))
			{
				continue;
			}
			_ = data.EntityID;
			_ = 5293;
			bool haulToTrade = false;
			Vector3? toLocation;
			if (!assignedItem2.Value.StorageLocation.GetStorageTarget.HasValue)
			{
				toLocation = SelectGroundLocation(assignedItem2.Value, data, allTileData);
				if (!toLocation.HasValue)
				{
					continue;
				}
			}
			else
			{
				toLocation = null;
				haulToTrade = assignedItem2.Value.StorageLocation.TradeOfferStorage.HasValue;
			}
			new HaulingJobSpecificItem(toLocation, assignedItem2.Value.StorageLocation.GetStorageTarget, parent, data, null, haulToStorage: true, haulToTrade, parent);
		}
	}

	private static void DestroyHaulingJobsForItem(EntityID entityID, EntityGroup owner)
	{
		foreach (HaulingJob item in owner.HaulingJobs.FindAll((Job j) => ((HaulingJob)j).Item == entityID))
		{
			if (!(item is HaulingJobAnyItemOfType) && !item.IsCompleted)
			{
				item.Destroy(cancelTakers: true);
			}
		}
	}

	private static Vector3? SelectGroundLocation(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData)
	{
		if (combo.StorageLocation.Zone != null)
		{
			return combo.StorageLocation.Zone.MapArea.SelectBestGroundLocationForStorage(itemData, allTileData);
		}
		Point pos = MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation);
		TerrainTile tile = The.Map.GetTile(pos);
		return SelectGroundLocationOnTile(itemData, allTileData, tile);
	}

	public static Vector3? FindSimilarItemToStackWithInTile(IKnownEntityData item, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, TerrainTile tile)
	{
		Point tilePos = new Point(tile.X, tile.Y);
		float bulkCapacityForSubTile = GameData.Instance.Constants.BulkCapacityForSubTile;
		if (tile.EntitiesOnTile != null)
		{
			foreach (Entity item2 in tile.EntitiesOnTile)
			{
				Point relativeSubtilePos = MapManager.WorldPosToRelativeSubtile(item2.PlaySiteLocation);
				float num = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];
				num += item2.Bulk;
				tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = num;
				Vector3? vector = TestItemToStackWith(item, tilePos, tileData, bulkCapacityForSubTile, item2, relativeSubtilePos);
				if (vector.HasValue)
				{
					return vector.Value;
				}
			}
		}
		foreach (Tuple<IKnownEntityData, Point> item3 in tileData.Item1)
		{
			Vector3? vector = TestItemToStackWith(item, tilePos, tileData, bulkCapacityForSubTile, item3.Item1, item3.Item2);
			if (vector.HasValue)
			{
				return vector.Value;
			}
		}
		return null;
	}

	public static Vector3? SelectGroundLocationOnTile(IKnownEntityData item, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData, TerrainTile tile)
	{
		Point tilePos = new Point(tile.X, tile.Y);
		Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tuple = null;
		_ = GameData.Instance.Constants.BulkCapacityForSubTile;
		tuple = GetTileGroundStorageData(allTileData, tilePos);
		Vector3? vector = FindSimilarItemToStackWithInTile(item, tuple, tile);
		if (vector.HasValue)
		{
			return vector.Value;
		}
		vector = FindEmptySubtileForStorage(tilePos, item, tuple, leftMostSubtilesOnly: false);
		if (vector.HasValue)
		{
			return vector.Value;
		}
		vector = FindFirstSubtileWithRoomForStorage(item, tilePos, tuple);
		if (vector.HasValue)
		{
			return vector.Value;
		}
		return null;
	}

	public static Vector3? FindFirstSubtileWithRoomForStorage(IKnownEntityData item, Point tilePos, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData)
	{
		float bulkCapacityForSubTile = GameData.Instance.Constants.BulkCapacityForSubTile;
		Point point = MapManager.TileEdgeToSubtile(tilePos);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Point relativeSubtilePos = new Point(j, i);
				new Point(j + point.X, i + point.Y);
				Vector3? vector = TestSubtileAsGroundLocation(item, tilePos, relativeSubtilePos, tileData, bulkCapacityForSubTile);
				if (vector.HasValue)
				{
					return vector.Value;
				}
			}
		}
		return null;
	}

	public static Vector3? FindEmptySubtileForStorage(Point tilePos, IKnownEntityData item, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, bool leftMostSubtilesOnly)
	{
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		Point point = MapManager.TileEdgeToSubtile(tilePos);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Point item2 = new Point(j, i);
				float num = tileData.Item2[j][i];
				Point point2 = new Point(j + point.X, i + point.Y);
				if (Common.IsZero(num) && !The.Map.SubtileIsCompletelyBlocked(mapCosts, point2) && !The.Map.FlagIsSet(point2, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved))
				{
					tileData.Item2[item2.X][item2.Y] = num + item.Bulk;
					tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, item2));
					return new Vector3?(MapManager.SubTileToWorldPos3(point2)).Value;
				}
			}
		}
		return null;
	}

	public static Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> GetTileGroundStorageData(Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData, Point tilePos)
	{
		if (!allTileData.TryGetValue(tilePos, out var value))
		{
			float[][] map = null;
			Common.InitJaggedArray(ref map, 3, 3);
			value = new Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>(new List<Tuple<IKnownEntityData, Point>>(), map);
			allTileData.Add(tilePos, value);
		}
		return value;
	}

	private static Vector3? TestItemToStackWith(IKnownEntityData item, Point tilePos, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, float bulkCapacity, IKnownEntityData entity, Point relativeSubtilePos)
	{
		if (entity.EntityType.ItemType != null && entity.EntityType.RenderableTypeMode.DefaultClientState != null && item.EntityType.RenderableTypeMode.DefaultClientState != null && entity.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null && item.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null && entity.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName == item.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName)
		{
			return TestSubtileAsGroundLocation(item, tilePos, relativeSubtilePos, tileData, bulkCapacity);
		}
		return null;
	}

	private static Vector3? TestSubtileAsGroundLocation(IKnownEntityData item, Point tilePos, Point relativeSubtilePos, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, float bulkCapacity)
	{
		float num = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		Point point = MapManager.TileAndRelativeSubtileToAbsoluteSubtile(tilePos.X, tilePos.Y, relativeSubtilePos.X, relativeSubtilePos.Y);
		if (num + item.Bulk <= bulkCapacity && !The.Map.SubtileIsCompletelyBlocked(mapCosts, point) && !The.Map.FlagIsSet(point, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved))
		{
			tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = num + item.Bulk;
			tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, relativeSubtilePos));
			return MapManager.SubTileToWorldPos3(point);
		}
		return null;
	}

	private static void AssignItemToStorage(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<EntityID, ItemStorageCombo> assignedItems)
	{
		if (assignedItems.TryGetValue(itemData.EntityID, out var _))
		{
			return;
		}
		Pair<int, int> limit = null;
		if (StorageHasCapacity(combo.StorageLocation, itemData, ref limit))
		{
			assignedItems.Add(itemData.EntityID, combo);
			combo.StorageLocation.TotalStored += itemData.Bulk;
			if (limit != null)
			{
				limit.First++;
			}
		}
	}

	private static void CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(SharedKnowledge sharedKnowledge, Dictionary<EntityID, ItemStorageCombo> assignedItems, EntityGroup owner)
	{
		List<EntityID> list = null;
		foreach (KeyValuePair<EntityID, ItemStorageCombo> assignedItem in assignedItems)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(assignedItem.Key, out var data)))
			{
				Common.AddToList(ref list, assignedItem.Key);
			}
			else if (!ItemIsOK(data) || IsCurrentlyInThisStorage(assignedItem.Value.StorageLocation, data))
			{
				Common.AddToList(ref list, assignedItem.Key);
				DestroyHaulingJobsForItem(assignedItem.Key, owner);
			}
			else if (HaulingJobAlreadyExistsForThisCombo(assignedItem.Value, data, owner) || ItemTypeIsNeededForProcess(owner, data.EntityType))
			{
				Common.AddToList(ref list, assignedItem.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityID item in list)
		{
			assignedItems.Remove(item);
		}
	}

	private float GetCurrentHaulTargetScore(ItemStorageCombo combo)
	{
		if (parent.SpecificHaulingJobs.TryGetValue(combo.Item, out var value))
		{
			if (value.ToStorage.HasValue)
			{
				if (combo.StorageLocation != null)
				{
					StorageTarget value2 = value.ToStorage.Value;
					StorageTarget? getStorageTarget = combo.StorageLocation.GetStorageTarget;
					if (value2 == getStorageTarget)
					{
						return 1f;
					}
				}
			}
			else if (MapManager.WorldPosToTile(value.ToLocation.Value) == MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation))
			{
				return 1f;
			}
		}
		return 0f;
	}

	private static bool HaulingJobAlreadyExistsForThisCombo(ItemStorageCombo combo, IKnownEntityData itemData, EntityGroup owner)
	{
		owner.SpecificHaulingJobs.TryGetValue(combo.Item, out var value);
		if (value != null)
		{
			if (combo.StorageLocation.NormalStorage.HasValue)
			{
				if (!value.IsToTradeOfferStorage && value.ToStorage.HasValue && value.ToStorage.Value == combo.StorageLocation.NormalStorage.Value)
				{
					return true;
				}
			}
			else if (combo.StorageLocation.TradeOfferStorage.HasValue)
			{
				if (value.IsToTradeOfferStorage && value.ToStorage.HasValue && value.ToStorage.Value == combo.StorageLocation.TradeOfferStorage.Value)
				{
					return true;
				}
			}
			else if (value.ToLocation.HasValue)
			{
				if (value.ToLocation == combo.StorageLocation.GroundLocation)
				{
					return true;
				}
				if (MapManager.WorldPosToTile(value.ToLocation.Value) == MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation))
				{
					return true;
				}
				ScoreOutdoorsStorage(value.ToLocation.Value, itemData, out var score);
				if (Common.IsEqual(combo.DamageScore, score, 0.01f) && value.ToLocation.HasValue && Common.DistanceOctile(value.ToLocation.Value, combo.StorageLocation.GroundLocation) < 200f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void CreateHaulingJobsForItemOutOfBand(IKnownEntityData item, EntityGroup ownerOfHaulingJobs)
	{
		List<ItemStorageCombo> list = null;
		List<StorageLocation> storageLocations = null;
		Point? point = null;
		CreateCombosForItemOutOfBand(item, ownerOfHaulingJobs, ref list, ref storageLocations, ref point);
		ScoreCombosAndCreateJobsOutOfBand(ownerOfHaulingJobs, list);
	}

	public static void CreateHaulingJobsForAllCarriedItemsOutOfBand(Entity carriedByEntity, EntityGroup ownerOfHaulingJobs)
	{
		if (carriedByEntity.AgentStorage.ItemStorage.StoredItems.Count <= 0)
		{
			return;
		}
		List<ItemStorageCombo> list = null;
		List<StorageLocation> storageLocations = null;
		Point? point = null;
		Intelligence intelligence = carriedByEntity.Intelligence;
		for (int num = carriedByEntity.AgentStorage.ItemStorage.StoredItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = carriedByEntity.AgentStorage.ItemStorage.StoredItems[num];
			if (intelligence.GetKnownData(entityID, out var data) != EntityResult.SeenDirectly)
			{
				carriedByEntity.AgentStorage.ItemStorage.RemoveOutdatedItem(entityID);
			}
			else
			{
				CreateCombosForItemOutOfBand(data, ownerOfHaulingJobs, ref list, ref storageLocations, ref point);
			}
		}
		ScoreCombosAndCreateJobsOutOfBand(ownerOfHaulingJobs, list);
	}

	private static void CreateCombosForItemOutOfBand(IKnownEntityData item, EntityGroup storageLocationsGroup, ref List<ItemStorageCombo> allCombos, ref List<StorageLocation> storageLocations, ref Point? storageCenterMapPosition)
	{
		if (item.AssignedToJob.HasValue)
		{
			return;
		}
		if (allCombos == null)
		{
			allCombos = new List<ItemStorageCombo>();
		}
		if (storageLocations == null)
		{
			storageLocations = new List<StorageLocation>();
			if (!GatherAllStorageLocations(storageLocationsGroup, storageLocations, out storageCenterMapPosition) || !storageCenterMapPosition.HasValue)
			{
				return;
			}
		}
		CreateCombosForItem(storageLocations, allCombos, item);
	}

	private static void ScoreCombosAndCreateJobsOutOfBand(EntityGroup ownerOfHaulingJobs, List<ItemStorageCombo> allCombos)
	{
		if (allCombos == null || allCombos.Count <= 0)
		{
			return;
		}
		float damageScore = 0f;
		Allegiance allegiance = ownerOfHaulingJobs.GetAllegiance();
		SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;
		RegionMap regionMapToUse = GetRegionMapToUse(allegiance);
		Dictionary<EntityType, Dictionary<StorageLocation, float>> dictionary = new Dictionary<EntityType, Dictionary<StorageLocation, float>>();
		Dictionary<Point, Dictionary<Point, double>> dictionary2 = new Dictionary<Point, Dictionary<Point, double>>();
		double num = CalculateAirliftCapacity(ownerOfHaulingJobs);
		IKnownEntityData itemData;
		for (int i = 0; i < allCombos.Count; i++)
		{
			if (IsComboValid(ownerOfHaulingJobs, sharedKnowledge, allCombos, ref i, out var combo, out itemData))
			{
				if (ScoreCombo(regionMapToUse, allegiance, num, dictionary, dictionary2, null, combo, itemData, out var score, out damageScore, getPathsNow: true) == Result.OK)
				{
					combo.Score = score;
					combo.DamageScore = damageScore;
				}
				else
				{
					combo.Score = 0.0;
				}
			}
		}
		SortCombos(allCombos);
		Dictionary<EntityID, ItemStorageCombo> dictionary3 = new Dictionary<EntityID, ItemStorageCombo>();
		for (int j = 0; j < allCombos.Count; j++)
		{
			if (IsComboValid(ownerOfHaulingJobs, sharedKnowledge, allCombos, ref j, out var combo2, out itemData))
			{
				AssignItemToStorage(combo2, itemData, dictionary3);
			}
		}
		CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(sharedKnowledge, dictionary3, ownerOfHaulingJobs);
		Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData = new Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>>();
		foreach (KeyValuePair<EntityID, ItemStorageCombo> item in dictionary3)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item.Key, out itemData)))
			{
				continue;
			}
			bool haulToTrade = false;
			Vector3? toLocation;
			if (!item.Value.StorageLocation.GetStorageTarget.HasValue)
			{
				toLocation = SelectGroundLocation(item.Value, itemData, allTileData);
				if (!toLocation.HasValue)
				{
					continue;
				}
			}
			else
			{
				toLocation = null;
				haulToTrade = item.Value.StorageLocation.TradeOfferStorage.HasValue;
			}
			new HaulingJobSpecificItem(toLocation, item.Value.StorageLocation.GetStorageTarget, ownerOfHaulingJobs, itemData, null, haulToStorage: true, haulToTrade, ownerOfHaulingJobs);
		}
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		snapshotParent = sn.SnapshotID<EntityGroup, EntityGroupID>(parent).Value;
		notifyWhenRegionSearchIsFinished = sn.DoEnum(notifyWhenRegionSearchIsFinished);
		takeOffAndLandTime = sn.DoFloat(takeOffAndLandTime);
		oneOverBaseSpeedByAir = sn.DoFloat(oneOverBaseSpeedByAir);
		oneOverBaseSpeedOnFoot = sn.DoFloat(oneOverBaseSpeedOnFoot);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(cachedItemLocationScores);
		sn.Ignore(cachedItemStorageScores);
		sn.Ignore(airliftCapacityScore);
		sn.Ignore(allCombos);
		sn.Ignore(allItems);
		sn.Ignore(AllStorageLocations);
		sn.Ignore(assignedItems);
		sn.Ignore(comboCounter);
		sn.Ignore(itemCounter);
		sn.Ignore(outdatedJobs);
		sn.Ignore(storageCenterMapPosition);
		sn.Ignore(phase);
		sn.Ignore(regulator);
		sn.Ignore(isWaiting);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotParent);
		ActionLookup.Add(notifyWhenRegionSearchIsFinished, NotifyWhenRegionSearchIsFinished);
		CreateRegulators();
	}

	public void Destroy()
	{
		The.Sim.CycleManager.UnRegister(this);
		ActionLookup.Remove(notifyWhenRegionSearchIsFinished);
		RemoveIDEntry();
	}
}
