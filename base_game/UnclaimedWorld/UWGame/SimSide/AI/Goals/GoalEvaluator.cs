using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public abstract class GoalEvaluator
{
	public enum CalculateResult
	{
		Done,
		Processing
	}

	public struct WeightedRating
	{
		private double rating;

		private double weightTotal;

		public double Result
		{
			get
			{
				if (!Common.IsEqual(weightTotal, 1.0))
				{
					throw new Exception("WeightedRating weights do not total 1.0 at Result time.");
				}
				return rating;
			}
		}

		public void AddScore(double weight, double score)
		{
			rating += weight * score;
			weightTotal += weight;
		}
	}

	protected double bestScore;

	protected double debugRating;

	protected static float maxMapOctileDistance;

	public static double HighestTravelTimeOnFoot;

	public static double OneOverHighestTravelTimeOnFoot;

	protected const double IdleGoalDesirabilitySpread = 0.005;

	protected Entity entity;

	protected Intelligence entityIntelligence;

	protected Person personEntity;

	public bool IsActive = true;

	public Tuple<double, double> ActiveInTime;

	public Tuple<double, double> ActiveInAgeInterval;

	protected double ageFalloff;

	protected const double timeOfDayContributionWeight = 0.2;

	protected const double ageContributionWeight = 0.1;

	public const float TimePhaseFalloff = 0.06f;

	public virtual float Priority => 1f;

	public GoalEvaluator(Entity entity)
	{
		this.entity = entity;
		entityIntelligence = this.entity.Intelligence;
		personEntity = this.entity.PersonEntity;
	}

	public GoalEvaluator(Entity entity, float ageIntervalStart, float ageIntervalEnd, float maxAge)
		: this(entity)
	{
		ActiveInAgeInterval = new Tuple<double, double>(ageIntervalStart, ageIntervalEnd);
		ageFalloff = Math.Min(0.1 * (double)(ageIntervalEnd - ageIntervalStart), 0.02 * (double)maxAge);
	}

	static GoalEvaluator()
	{
		maxMapOctileDistance = Common.DistanceOctile(Point.Zero, new Point(128, 128));
		HighestTravelTimeOnFoot = 48f * maxMapOctileDistance / GameData.Instance.AllEntityTypes["entity:human"].LocomotorType.LeggedLocomotorType.WalkNormalSpeed;
		OneOverHighestTravelTimeOnFoot = 1.0 / HighestTravelTimeOnFoot;
	}

	public abstract CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result);

	public virtual void PreSetGoal()
	{
	}

	public virtual bool SetGoal()
	{
		entityIntelligence.Memory.ClearNeededItemsForNextGoal();
		return true;
	}

	public abstract bool CanTakeGoal();

	public abstract bool CancelCurrentTakers();

	public virtual bool IsIdleActivity()
	{
		return false;
	}

	public static double ScoreJobProgress(float progress)
	{
		return Math.Pow(progress, 2.0);
	}

	public static double ScoreNumberOfWorkers(int proposedNumber, int maxNumber)
	{
		return ProcessJob.GetMarginalLaborReturn(proposedNumber, maxNumber);
	}

	public static double ScoreSkill(float skill)
	{
		return Math.Pow(skill, 2.0);
	}

	public static double ScoreUniqueSkill(Intelligence intelligence, SkillType skillType)
	{
		if (intelligence.HasSkill(skillType) && intelligence.CurrentExpedition.SkillIsUnique(skillType))
		{
			return 1.0;
		}
		return 0.0;
	}

	public static double ScoreUniqueSkillPenalty(Entity agent)
	{
		if (agent.Intelligence.CurrentExpedition.HasUniqueSkill(agent))
		{
			return -1.0;
		}
		return 0.0;
	}

	public static RegionMap GetRegionMapAndStanceForEvaluator(Entity entity, Job job, out ThreatStance threatStanceToUse, bool? requiresBoldStance = null)
	{
		bool flag = false;
		if (job != null)
		{
			flag = job.RequiresBoldStance;
		}
		else if (requiresBoldStance == true)
		{
			flag = true;
		}
		if (flag)
		{
			threatStanceToUse = ThreatStance.Bold;
		}
		else
		{
			threatStanceToUse = MinimumThreatStance(entity.Intelligence.ThreatStance, ThreatStance.Normal);
		}
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			return entity.Intelligence.Allegiance.SharedKnowledge.GetRegionMapToUseForEntity(entity, threatStanceToUse);
		}
		return null;
	}

	private static ThreatStance MinimumThreatStance(ThreatStance stance1, ThreatStance stance2)
	{
		if ((int)stance1 < (int)stance2)
		{
			return stance1;
		}
		return stance2;
	}

	public static void UpdateJobAccessibility(Entity entity, Point fromSubile, Point toSubtile, Job job, IHasEntityGroup ownerOfJob, RegionMap.Result moveResult)
	{
		if (job == null || ownerOfJob == null)
		{
			return;
		}
		if (moveResult == RegionMap.Result.NoAccess && job != null && ownerOfJob != null)
		{
			The.Client.SetJobInaccessible(job, ownerOfJob, isInaccessible: true);
			bool? flag = ComputeIsBlockedByThreat(entity, fromSubile, toSubtile);
			if (flag.HasValue)
			{
				The.Client.SetJobBlockedByThreat(job, ownerOfJob, flag.Value);
			}
		}
		if (moveResult == RegionMap.Result.OK)
		{
			The.Client.SetJobInaccessible(job, ownerOfJob, isInaccessible: false);
		}
	}

	public static bool? ComputeIsBlockedByThreat(Entity activeEntity, IKnownEntityData toEntity)
	{
		float distance = 0f;
		return The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap.GetDistanceToEntity(activeEntity, activeEntity, toEntity, ref distance, null, null, sendMessageToEntity: false, null, giveClientFeedback: false) switch
		{
			RegionMap.Result.OK => true, 
			RegionMap.Result.NoAccess => false, 
			_ => null, 
		};
	}

	public static bool? ComputeIsBlockedByThreat(Entity entity, Point fromSubile, Point toSubtile)
	{
		float distance = 0f;
		return The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap.GetDistance(entity, fromSubile, toSubtile, ref distance, sendMessageToEntity: false) switch
		{
			RegionMap.Result.OK => true, 
			RegionMap.Result.NoAccess => false, 
			_ => null, 
		};
	}

	public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 from, Vector3 to, Entity entity, ref double travelTimeScore, Job job = null, IHasEntityGroup ownerOfJob = null)
	{
		Point point = MapManager.WorldPosToSubtile(from);
		Point toSubtile = MapManager.WorldPosToSubtile(to);
		float distance = -1f;
		if (!WorkSiteIsSafe(entity, to, threatStanceToUse))
		{
			if (job != null && ownerOfJob != null)
			{
				The.Client.SetJobInaccessible(job, ownerOfJob, isInaccessible: true);
			}
			return RegionMap.Result.NoAccess;
		}
		RegionMap.Result distance2 = regionMap.GetDistance(entity, point, toSubtile, ref distance);
		UpdateJobAccessibility(entity, point, toSubtile, job, ownerOfJob, distance2);
		if (distance2 != RegionMap.Result.OK)
		{
			return distance2;
		}
		travelTimeScore = GetTravelScoreFromDistance(entity, distance);
		return RegionMap.Result.OK;
	}

	public static double GetTravelScoreFromDistance(Entity entity, float distance)
	{
		return ScoreTravelTime(GetEvaluatorTimeCostOfDistance(entity, distance));
	}

	public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, Entity entity, IKnownEntityData targetEntityData, ref double travelTimeScore)
	{
		float distance = -1f;
		return ScoreTravelTime(regionMap, entity, targetEntityData, ref travelTimeScore, ref distance);
	}

	public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, Entity entity, IKnownEntityData targetEntityData, ref double travelTimeScore, ref float distance)
	{
		distance = -1f;
		RegionMap.Result distanceToEntity = regionMap.GetDistanceToEntity(entity, entity, targetEntityData, ref distance);
		if (distanceToEntity != RegionMap.Result.OK)
		{
			return distanceToEntity;
		}
		travelTimeScore = GetTravelScoreFromDistance(entity, distance);
		return RegionMap.Result.OK;
	}

	public static double GetEvaluatorTimeCostOfDistance(Entity entity, float distance)
	{
		if (!entity.GetDrivenVehicle(out var vehicle))
		{
			return 100.0;
		}
		if (vehicle != null)
		{
			return (double)distance / (double)vehicle.Locomotor.CurrentMaximumSpeedForEvaluator * (double)PlainsType.Instance.MovementFactor(((VehicleContainerType)vehicle.EntityType.ContainerType).Transport, SurfaceType.TerrainFeatures.None);
		}
		return (double)distance / (double)entity.Locomotor.CurrentMaximumSpeedForEvaluator * (double)PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);
	}

	public static bool HandleOwnerDataResult(SharedKnowledge sharedKnowledge, EntityID entityID, EntityGroup entityGroup, out IKnownEntityData entityData, EntityType entityType = null)
	{
		switch (sharedKnowledge.GetKnownData(entityID, out entityData))
		{
		case EntityResult.EntityStatusIsNowUnknown:
		case EntityResult.Destroyed:
			entityGroup?.DeleteEntity(entityID, entityType);
			return false;
		case EntityResult.NewUnknownEntity:
			entityData = null;
			return false;
		default:
			return true;
		}
	}

	public static bool HandleOwnerDataResult(SharedKnowledge sharedKnowledge, EntityID entityID, EntityGroup entityGroup, out IKnownEntityData entityData, ref List<EntityID> invalidEntities, EntityType entityType = null)
	{
		switch (sharedKnowledge.GetKnownData(entityID, out entityData))
		{
		case EntityResult.EntityStatusIsNowUnknown:
		case EntityResult.Destroyed:
			Common.AddToList(ref invalidEntities, entityID);
			return false;
		case EntityResult.NewUnknownEntity:
			entityData = null;
			return false;
		default:
			return true;
		}
	}

	public static void HandleInvalidJob(Job job)
	{
		job.Destroy(cancelTakers: true);
	}

	public static bool EntityDataResultCausesSkip(EntityResult result)
	{
		if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown || result == EntityResult.NewUnknownEntity)
		{
			return true;
		}
		return false;
	}

	public static bool ProcessDataResultCausesSkip(ProcessResult result)
	{
		if (result == ProcessResult.Destroyed)
		{
			return true;
		}
		return false;
	}

	protected void GetReplenishItemsToCancel(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTools, ref List<Entity> needsToBeCancelled, ref List<Entity> itemsToBeDropped)
	{
		if (replenishItemsForTools != null)
		{
			foreach (KeyValuePair<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTool in replenishItemsForTools)
			{
				foreach (Tuple<ProcessType, List<IKnownEntityData>> item in replenishItemsForTool.Value)
				{
					foreach (IKnownEntityData item2 in item.Item2)
					{
						GetItemUsersToCancel(item2, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: false);
					}
				}
			}
		}
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
	}

	protected CalculateResult FindReplenishItemsForToolOrWeapon(Entity entity, EntityGroup ownerOfItems, List<EntityGroup> listOfOwnersOfItems, ref Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> allFoundReplenishItems, double ourScore, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, IKnownEntityData entityToReplenish, Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity, out bool success, EntityType neededAmmoType = null, int? neededAmmoRounds = null, float? neededElectricalEnergy = null, float? neededFuelBulk = null)
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		success = false;
		if (neededFuelBulk.HasValue)
		{
			flag = entityToReplenish.HasEnoughFuel(neededFuelBulk.Value);
		}
		if (neededElectricalEnergy.HasValue)
		{
			throw new NotImplementedException("Power for tools");
		}
		if (neededAmmoRounds.HasValue)
		{
			flag3 = entityToReplenish.HasEnoughAmmo(neededAmmoType, neededAmmoRounds.Value);
		}
		if (flag && flag2 && flag3)
		{
			success = true;
			return CalculateResult.Done;
		}
		if (entity.EntityType.IntelligenceType.CanReplenish != true)
		{
			success = false;
			return CalculateResult.Done;
		}
		_ = GameData.Instance.AIConstants.MaxDistanceForReplenishItems;
		if (!flag && footRegionMap != null)
		{
			RequiresFuelType requiresFuelType = entityToReplenish.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType;
			float? availableFuelBulk = 0f;
			List<IKnownEntityData> foundReplenishItems = new List<IKnownEntityData>();
			foreach (EntityType fuelEntityType in requiresFuelType.FuelEntityTypes)
			{
				ProcessType action = entityToReplenish.EntityType.ContainerType.GetReplenishProcesses()[fuelEntityType];
				if (FindReplenishItemsForToolAndAction(entity, ownerOfItems, listOfOwnersOfItems, ref allFoundReplenishItems, ourScore, sharedKnowledge, footRegionMap, entityToReplenish, action, fuelEntityType, energyItemsSortedByDistanceToEntity, ref foundReplenishItems, ref availableFuelBulk, neededFuelBulk, null, out success) == CalculateResult.Processing)
				{
					return CalculateResult.Processing;
				}
				if (success)
				{
					break;
				}
			}
			if (!success)
			{
				return CalculateResult.Done;
			}
		}
		if (!flag3 && footRegionMap != null)
		{
			float? availableFuelBulk2 = null;
			List<IKnownEntityData> foundReplenishItems2 = new List<IKnownEntityData>();
			ProcessType action2 = entityToReplenish.EntityType.ContainerType.GetReplenishProcesses()[neededAmmoType];
			if (FindReplenishItemsForToolAndAction(entity, ownerOfItems, listOfOwnersOfItems, ref allFoundReplenishItems, ourScore, sharedKnowledge, footRegionMap, entityToReplenish, action2, neededAmmoType, energyItemsSortedByDistanceToEntity, ref foundReplenishItems2, ref availableFuelBulk2, null, neededAmmoRounds, out success) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			_ = success;
			return CalculateResult.Done;
		}
		return CalculateResult.Done;
	}

	public bool IsItemAlreadyAdded(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTools, ItemDistance item)
	{
		if (replenishItemsForTools != null)
		{
			foreach (KeyValuePair<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTool in replenishItemsForTools)
			{
				foreach (Tuple<ProcessType, List<IKnownEntityData>> item2 in replenishItemsForTool.Value)
				{
					foreach (IKnownEntityData item3 in item2.Item2)
					{
						if (item3 == item.Entity)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	protected static bool EntityResultCausesFailedGoal(EntityResult result)
	{
		if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
		{
			return true;
		}
		return false;
	}

	protected static bool IsReplenishItemOk(IKnownEntityData data, Intelligence entityIntelligence, EntityGroup owner, EntityType entityType)
	{
		if (data.EntityType == entityType && owner.Contains(data) && data.CanBeHauled() && Entity.IsFunctional(data))
		{
			if (!data.IsUnassigned(entityIntelligence.Allegiance.SharedKnowledge))
			{
				if (IsAssignedToUntakenHaulingJob(data))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsAssignedToUntakenHaulingJob(IKnownEntityData data)
	{
		if (data.AssignedToJob.HasValue && LookUp<Job, JobID>.FindByID(data.AssignedToJob) is HaulingJob { Item: var item } haulingJob)
		{
			EntityID entityID = data.EntityID;
			if (item.GetValueOrDefault() == entityID && item.HasValue && haulingJob.TakenBy.Count == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static CalculateResult GetAllReplenishItemsSortedByDistance(Entity entity, EntityType entityType, EntityGroup owner, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float? maxDistanceToWalk, Vector2 positionToSearchFrom, float rangeToSearchIn)
	{
		List<Pair<EntityID, Vector2>> resultsList = new List<Pair<EntityID, Vector2>>();
		sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(positionToSearchFrom, rangeToSearchIn, null, ref resultsList);
		List<EntityID> list = new List<EntityID>();
		foreach (Pair<EntityID, Vector2> item in resultsList)
		{
			list.Add(item.First);
		}
		return GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap, list, ref sortedList, (IKnownEntityData itemData) => IsReplenishItemOk(itemData, entity.Intelligence, owner, entityType), maxDistanceToWalk);
	}

	public static CalculateResult GetSortedListOfEntities(Entity entity, EntityGroup ownerOfEntitiesToSort, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, List<EntityID> items, ref List<ItemDistance> distanceList, Predicate<IKnownEntityData> entityPredicate, float? maxDistance = null)
	{
		for (int num = items.Count - 1; num >= 0; num--)
		{
			EntityID entityID = items[num];
			if (HandleOwnerDataResult(sharedKnowledge, entityID, ownerOfEntitiesToSort, out var entityData) && entityPredicate(entityData))
			{
				float distance = -1f;
				switch (footRegionMap.GetDistanceToEntity(entity, entity, entityData, ref distance))
				{
				case RegionMap.Result.Wait:
					return CalculateResult.Processing;
				default:
					if (!maxDistance.HasValue || distance < maxDistance.Value)
					{
						distanceList.Add(new ItemDistance
						{
							Entity = entityData,
							Distance = distance
						});
					}
					break;
				case RegionMap.Result.NoAccess:
					break;
				}
			}
		}
		distanceList = distanceList.OrderBy((ItemDistance d) => d.Distance).ToList();
		return CalculateResult.Done;
	}

	private CalculateResult GetAllEntitiesSortedByDistance(Entity entity, EntityType entityType, List<EntityGroup> listOfOwners, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float maxDistance)
	{
		foreach (EntityGroup listOfOwner in listOfOwners)
		{
			if (GetAllReplenishItemsSortedByDistance(entity, entityType, listOfOwner, sharedKnowledge, footRegionMap, ref sortedList, maxDistance, entity.PlaySiteLocation.ToVector2(), maxDistance) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
		}
		return CalculateResult.Done;
	}

	private CalculateResult FindReplenishItemsForToolAndAction(Entity entity, EntityGroup groupOfItems, List<EntityGroup> listOfGroupsOfItems, ref Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> allFoundReplenishItems, double ourScore, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, IKnownEntityData entityToReplenish, ProcessType action, EntityType replenishEntityType, Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity, ref List<IKnownEntityData> foundReplenishItems, ref float? availableFuelBulk, float? neededFuelBulk, int? neededAmmoRounds, out bool success)
	{
		success = false;
		if (!energyItemsSortedByDistanceToEntity.TryGetValue(replenishEntityType, out var value))
		{
			float maxDistanceForReplenishItems = GameData.Instance.AIConstants.MaxDistanceForReplenishItems;
			value = new List<ItemDistance>();
			CalculateResult calculateResult = ((groupOfItems == null) ? GetAllEntitiesSortedByDistance(entity, replenishEntityType, listOfGroupsOfItems, sharedKnowledge, footRegionMap, ref value, maxDistanceForReplenishItems) : GetAllReplenishItemsSortedByDistance(entity, replenishEntityType, groupOfItems, sharedKnowledge, footRegionMap, ref value, maxDistanceForReplenishItems, entity.Location.Value.ToVector2(), maxDistanceForReplenishItems));
			if (calculateResult == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			energyItemsSortedByDistanceToEntity.Add(replenishEntityType, value);
		}
		bool flag = false;
		List<Entity> needsToBeCancelled = null;
		List<Entity> itemsToBeDropped = null;
		int num = 0;
		foreach (ItemDistance item in value)
		{
			if (IsItemAlreadyAdded(allFoundReplenishItems, item))
			{
				continue;
			}
			GetItemUsersToCancel(item.Entity, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: true);
			if (!IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled))
			{
				continue;
			}
			_ = item.Distance;
			_ = 20f;
			foundReplenishItems.Add(item.Entity);
			if (action.ReplenishAction == GoalReplenish.ReplenishAction.Refuel)
			{
				availableFuelBulk += item.Entity.Bulk;
				if (availableFuelBulk >= neededFuelBulk)
				{
					flag = true;
				}
			}
			else if (action.ReplenishAction == GoalReplenish.ReplenishAction.Reload)
			{
				num += item.Entity.NoOfRounds.Value;
				if (num >= neededAmmoRounds)
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (allFoundReplenishItems == null)
				{
					allFoundReplenishItems = new Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>>();
				}
				allFoundReplenishItems[entityToReplenish] = new List<Tuple<ProcessType, List<IKnownEntityData>>>();
				allFoundReplenishItems[entityToReplenish].Add(new Tuple<ProcessType, List<IKnownEntityData>>(action, foundReplenishItems));
				success = true;
				return CalculateResult.Done;
			}
		}
		return CalculateResult.Done;
	}

	protected List<EntityGroupID> GetOwnerIDs(List<EntityGroup> owners)
	{
		return owners?.Select((EntityGroup o) => o.ID).ToList();
	}

	public static EntityGroupID? GetOwnerID(EntityGroup owner)
	{
		return owner?.ID;
	}

	protected void GetItemUsersToCancel(IKnownEntityData item, ref List<Entity> needsToBeCancelled, ref List<Entity> itemsToBeDropped, bool clearLists)
	{
		if (clearLists)
		{
			if (needsToBeCancelled == null)
			{
				needsToBeCancelled = new List<Entity>();
			}
			needsToBeCancelled.Clear();
			if (itemsToBeDropped == null)
			{
				itemsToBeDropped = new List<Entity>();
			}
			itemsToBeDropped.Clear();
		}
		GetItemUser(item, needsToBeCancelled);
		Job job = EvaluateJob.ResolveAssignedToJob(item);
		if (job != null && job.TakenBy.Count > 0)
		{
			if (job.TakenBy.Count == 1)
			{
				needsToBeCancelled.Add(job.TakenBy.Get(0));
			}
			else
			{
				EntityID? inUseBy = entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(item.EntityID);
				if (inUseBy.HasValue)
				{
					Entity entity = job.TakenBy.Get((Entity e) => e.ID == inUseBy);
					if (entity != null)
					{
						needsToBeCancelled.Add(entity);
					}
				}
			}
		}
		if (item is Entity entity2 && entity2.CarriedByAgent(out var carrier) && carrier != null && carrier != this.entity)
		{
			itemsToBeDropped.Add(entity2);
		}
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
	}

	protected void GetItemUser(IKnownEntityData itemData, List<Entity> listOfUsers)
	{
		EntityID? inUseBy = entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(itemData.EntityID);
		if (inUseBy.HasValue)
		{
			Entity entity = Entity.FindByID(inUseBy);
			if (entity != null)
			{
				_ = itemData.EntityID;
				_ = 18;
				listOfUsers.Add(entity);
			}
			else
			{
				entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(itemData.EntityID, inUseBy.Value);
			}
		}
	}

	public static double ScoreTravelTime(double time, double oneOverHighestTravelTimeToUse)
	{
		return Math.Pow(Common.Clamp(1.0 - time * oneOverHighestTravelTimeToUse, 0.0, 1.0), 2.0);
	}

	public static double ScoreTravelTime(double time)
	{
		return ScoreTravelTime(time, OneOverHighestTravelTimeOnFoot);
	}

	public static bool IsOnPlaySite(IKnownEntityData entityData)
	{
		return entityData.Location.HasValue;
	}

	public static bool IsValidPlaysiteItem(Entity entity, IKnownEntityData food, bool mustCarryItem)
	{
		if (food.NotOnboardDrivenVehicle && IsOnPlaySite(food) && (!mustCarryItem || (entity.AgentStorage != null && entity.AgentStorage.ItemStorage != null && entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(food.Bulk))) && Entity.IsFunctional(food))
		{
			return food.IsCompleted();
		}
		return false;
	}

	public static double ScoreIsEntityFunctional(IKnownEntityData entity)
	{
		if (Entity.IsFunctional(entity))
		{
			return 1.0;
		}
		return 0.0;
	}

	protected bool CancelEntities(List<Entity> entitiesToCancel, List<Entity> itemsToBeDropped)
	{
		if (entitiesToCancel.Count > 0)
		{
			Entity cancelThis;
			do
			{
				cancelThis = entitiesToCancel[0];
				if (cancelThis != this.entity && !cancelThis.SendMessage(new Message(this.entity, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.LeaveVehicle)))
				{
					return false;
				}
				entitiesToCancel.RemoveAll((Entity a) => a == cancelThis);
			}
			while (entitiesToCancel.Count > 0);
		}
		if (itemsToBeDropped.Count > 0)
		{
			IKnownEntityData dropThis;
			do
			{
				dropThis = itemsToBeDropped[0];
				if (dropThis is Entity entity)
				{
					if (!entity.CarriedByAgent(out var carrier))
					{
						return false;
					}
					if (carrier != null && carrier != this.entity && !carrier.SendMessage(new Message(this.entity, Message.MessageTypes.OtherAgentRequestsDropItem, dropThis)))
					{
						return false;
					}
					if (!entity.CarriedByAgent(out carrier))
					{
						return false;
					}
					if (carrier != null)
					{
						return false;
					}
					itemsToBeDropped.RemoveAll((Entity a) => a == dropThis);
				}
			}
			while (itemsToBeDropped.Count > 0);
		}
		return true;
	}

	public double ScoreTimeOfDay()
	{
		if (ActiveInTime != null)
		{
			return TimePhaseWithFalloff(ActiveInTime.Item1, ActiveInTime.Item2, 0.05999999865889549, The.Sim.DateAndTime.TimeOfDay);
		}
		return 1.0;
	}

	protected double ScoreAge(BiologicalEntity biologicalEntity)
	{
		if (ActiveInAgeInterval != null)
		{
			return TimePhaseWithFalloff(ActiveInAgeInterval.Item1, ActiveInAgeInterval.Item2, ageFalloff, biologicalEntity.AgeGroup.Age);
		}
		return 1.0;
	}

	public static double AddTimeAgeAndPriority(double rating, double timeOfDayContribution, double ageContribution, float priority)
	{
		return (double)priority * (0.7000000000000001 * rating + (0.2 * timeOfDayContribution + 0.1 * ageContribution));
	}

	protected double ApplyPriority(double rating)
	{
		return (double)Priority * rating;
	}

	public double GetAgeContribution()
	{
		double result = 1.0;
		if (entity.Find<BiologicalEntity>(out var c))
		{
			result = ScoreAge(c);
		}
		return result;
	}

	public static bool WorkSiteIsSafe(Entity entity, IKnownEntityData targetEntity, ThreatStance approachToUse, bool isAttacking = false)
	{
		if (!WorkSiteIsSafe(entity, targetEntity.PlaySiteLocation, approachToUse, isAttacking))
		{
			The.Client.SetEntityBlockedByThreat(entity.Intelligence.Allegiance, targetEntity, blockedByThreat: true);
			return false;
		}
		return true;
	}

	public static bool WorkSiteIsSafe(Entity entity, Vector3 location, ThreatStance approachToUse, bool isAttacking = false)
	{
		if (!isAttacking && approachToUse != ThreatStance.Bold)
		{
			DiscomfortMap discomfortMap = entity.Intelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entity.Intelligence.ProtectionLevel, entity.EntityType, approachToUse);
			Point pos = MapManager.WorldPosToTile(location);
			if (discomfortMap.Map.GetValue(pos) > GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue)
			{
				return false;
			}
		}
		return true;
	}

	protected bool CanTakeStanceForJob(Job job, ThreatStance normalThreatStance, out ThreatStance threatStanceToUse)
	{
		RegionMap regionMapToUse = null;
		return CanTakeStanceForJob(job, null, normalThreatStance, out regionMapToUse, out threatStanceToUse, getRegionMap: false);
	}

	protected bool CanTakeStanceForJob(Job job, RegionMap normalRegionMap, ThreatStance normalThreatStance, out RegionMap regionMapToUse, out ThreatStance threatStanceToUse, bool getRegionMap = true)
	{
		bool requiresBoldStance = job.RequiresBoldStance;
		regionMapToUse = normalRegionMap;
		threatStanceToUse = normalThreatStance;
		if (requiresBoldStance)
		{
			if (!entityIntelligence.StanceCanBeBold())
			{
				EvaluateAttackJobs.SetJobInaccessibleDueToBoldStance(job, IsBlocked: true);
				return false;
			}
			EvaluateAttackJobs.SetJobInaccessibleDueToBoldStance(job, IsBlocked: false);
			threatStanceToUse = ThreatStance.Bold;
			if (getRegionMap)
			{
				regionMapToUse = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
			}
		}
		return true;
	}

	private static bool TestWithinInterval(double intervalStart, double intervalEnd, double falloff, double input, ref double value)
	{
		if (input >= intervalStart && input <= intervalEnd)
		{
			if (input <= intervalStart + falloff)
			{
				value = MathHelper.SmoothStep(0f, 1f, (float)((input - intervalStart) / falloff));
				return true;
			}
			if (input >= intervalEnd - falloff)
			{
				value = MathHelper.SmoothStep(1f, 0f, (float)((input - (intervalEnd - falloff)) / falloff));
				return true;
			}
			value = 1.0;
			return true;
		}
		return false;
	}

	public static double TimePhaseWithFalloff(double phaseStart, double phaseEnd, double falloff, double currentTime)
	{
		double num = phaseStart - falloff;
		double num2 = phaseEnd + falloff;
		double value = 0.0;
		if (phaseStart > phaseEnd)
		{
			if (TestWithinInterval(num - 1.0, num2, falloff, currentTime, ref value))
			{
				return value;
			}
			if (TestWithinInterval(num, num2 + 1.0, falloff, currentTime, ref value))
			{
				return value;
			}
			return 0.0;
		}
		if (TestWithinInterval(num, num2, falloff, currentTime, ref value))
		{
			return value;
		}
		return 0.0;
	}

	protected bool CanForceDropItems(List<Entity> itemsToDrop)
	{
		foreach (Entity item in itemsToDrop)
		{
			if (!CarrierPermitsForceDrop(item) || (Common.DistanceOctile(entity.PlaySiteLocation, item.PlaySiteLocation) > GameData.Instance.AIConstants.MaxDistanceBetweenAgentsToAlwaysAllowForceDrop && IsNearThreats(item)))
			{
				return false;
			}
		}
		return true;
	}

	private bool CarrierPermitsForceDrop(Entity item)
	{
		if (item.CarriedByAgent(out var carrier))
		{
			return carrier?.Intelligence.CanDropRequestedItem(item) ?? true;
		}
		return false;
	}

	private bool IsNearThreats(Entity item)
	{
		ThreatMap threatMap = entity.Intelligence.Allegiance.SharedKnowledge.GetThreatMap(entity.EntityType, ThreatStance.Cautious);
		Point pos = MapManager.WorldPosToTile(item.PlaySiteLocation);
		if (threatMap.Map.GetValue(pos) > GameData.Instance.AIConstants.HighestThreatLevelToAllowForceDrop)
		{
			return true;
		}
		List<Pair<EntityID, Vector2>> resultsList = null;
		entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), GameData.Instance.AIConstants.MinimumDistanceToThreatsToAllowForceDrop, (EntityID e) => IsThreat(e), ref resultsList);
		if (resultsList != null && resultsList.Count > 0)
		{
			return true;
		}
		return false;
	}

	private bool IsThreat(EntityID entityID)
	{
		if (entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownThreatSources.ContainsKey(entityID))
		{
			return true;
		}
		return false;
	}

	protected static bool IsScoreBetterThanAllInvolveds(double ourScore, List<Entity> listOfEntities, Entity thisEntity = null)
	{
		foreach (Entity listOfEntity in listOfEntities)
		{
			if (listOfEntity.Intelligence.GetScore() > ourScore)
			{
				return false;
			}
			if (listOfEntity.PersonEntity != null)
			{
				listOfEntity.Name.Contains("Augustine Yeboah");
			}
		}
		return true;
	}
}
