using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public abstract class CompositeGoal : Goal
{
	public Queue<Goal> Subgoals = new Queue<Goal>();

	private Queue<GoalID> snapshotSubgoals = new Queue<GoalID>();

	protected List<EntityGroupID> ownersOfVehicles;

	private List<EntityID> optionalEquipmentAssignedToThisJob = new List<EntityID>();

	private const int searchLimit = 6000;

	private static List<ItemType.TaskType> longerJourneyTask = new List<ItemType.TaskType> { ItemType.TaskType.LongerJourneys };

	public bool SkipThisAssert;

	private Snapshotter.Version version;

	public CompositeGoal()
	{
	}

	public CompositeGoal(Entity entity)
		: base(entity)
	{
	}

	public override bool IsSame(Job job)
	{
		if (job != null && job.TakenBy.Contains(entity))
		{
			return true;
		}
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().IsSame(job);
		}
		return false;
	}

	public override string GetSkillInUseName()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetSkillInUseName();
		}
		return base.GetSkillInUseName();
	}

	public override string GetToolInUseName()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetToolInUseName();
		}
		return base.GetToolInUseName();
	}

	public override float? GetSkillProductivity()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetSkillProductivity();
		}
		return base.GetSkillProductivity();
	}

	public override float? GetCurrentTotalProductivity()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetCurrentTotalProductivity();
		}
		return base.GetCurrentTotalProductivity();
	}

	public override float? GetToolProductivity()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetToolProductivity();
		}
		return base.GetToolProductivity();
	}

	public override float GetExertionLevel()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetExertionLevel();
		}
		return base.GetExertionLevel();
	}

	public override bool CanDropRequestedItem(Entity item)
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().CanDropRequestedItem(item);
		}
		return true;
	}

	public override bool CanReactToInterest()
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().CanReactToInterest();
		}
		return base.CanReactToInterest();
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetDetectAgentsFactor(typeOfAgent, requiresExamineAction);
		}
		return base.GetDetectAgentsFactor(typeOfAgent, requiresExamineAction);
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (Subgoals.Count > 0 && Subgoals.Peek().Status == Status.Active)
		{
			return Subgoals.Peek().GetDetectResourcesFactor(resourceType, requiresExamineAction);
		}
		return base.GetDetectResourcesFactor(resourceType, requiresExamineAction);
	}

	public override Goal GetFrontMostGoal()
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek();
		}
		return this;
	}

	public override string ComposeIndentedString(string indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = "\r\n";
		stringBuilder.Append(indent);
		stringBuilder.Append(ToString());
		stringBuilder.Append(value);
		foreach (Goal subgoal in Subgoals)
		{
			stringBuilder.Append(subgoal.ComposeIndentedString(" " + indent));
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public List<Waypoint> GetWaypointPath()
	{
		foreach (Goal subgoal in Subgoals)
		{
			if (subgoal is GoalFollowPath)
			{
				return ((GoalFollowPath)subgoal).WaypointPath;
			}
			if (subgoal is CompositeGoal)
			{
				return ((CompositeGoal)subgoal).GetWaypointPath();
			}
		}
		return null;
	}

	protected void DropAllCarriedItems()
	{
		if (entity.AgentStorage != null)
		{
			entity.AgentStorage.IterateContained(delegate(Entity e)
			{
				AddSubgoal(new GoalDropItem(entity, e.EntityID));
			});
		}
	}

	protected void EnableCollisions(bool enable)
	{
		if (enable)
		{
			entity.EnableCollisions();
		}
		else
		{
			entity.DisableCollisions();
		}
	}

	public Status ProcessSubgoals(GameTime elapsed)
	{
		Status statusOfLastSubGoal = Status.Completed;
		bool hasFailedSubgoal = false;
		statusOfLastSubGoal = CleanupSubgoals(statusOfLastSubGoal, ref hasFailedSubgoal);
		if (hasFailedSubgoal)
		{
			return Status.Failed;
		}
		if (Subgoals.Count > 0)
		{
			Status status = Subgoals.Peek().Process(elapsed);
			if (status == Status.Completed && Subgoals.Count > 1)
			{
				return Status.Active;
			}
			return status;
		}
		return statusOfLastSubGoal switch
		{
			Status.Completed => Status.Completed, 
			Status.Failed => Status.Failed, 
			_ => Status.Completed, 
		};
	}

	protected void HandleSubstitutedGoalByArbitrator()
	{
		base.Status = Status.Failed;
	}

	protected Status CleanupSubgoals(Status statusOfLastSubGoal, ref bool hasFailedSubgoal)
	{
		while (Subgoals.Count > 0)
		{
			Goal goal = Subgoals.Peek();
			if (goal.IsCompleted() || goal.HasFailed())
			{
				if (goal.HasFailed())
				{
					hasFailedSubgoal = true;
				}
				statusOfLastSubGoal = goal.Status;
				goal.Terminate();
				RemoveFirstSubgoal();
				if (Subgoals.Count > 0)
				{
					Subgoals.Peek().EnterIfNew();
				}
				continue;
			}
			if (goal is CompositeGoal compositeGoal)
			{
				compositeGoal.CleanupSubgoals(statusOfLastSubGoal, ref hasFailedSubgoal);
			}
			break;
		}
		return statusOfLastSubGoal;
	}

	protected Goal RemoveFirstSubgoal()
	{
		Goal goal = Subgoals.Dequeue();
		DestroyGoal(goal);
		return goal;
	}

	private static void DestroyGoal(Goal subgoal)
	{
		subgoal.RemoveIDEntry();
		subgoal.RetireGoal();
	}

	public static List<PathFinderNode> FindPathToSafety(DiscomfortMap dMap, Entity entity)
	{
		AStarSearch.DijkstraTestNodeDelegate testNodePredicate = (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate), dMap, The.Map.InfluenceMapTileIsFreeInfo);
		return entity.Intelligence.PathPlanner.FindItemAndGetPath(entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Protected, entity.EntityType, ThreatStance.Bold).Layers[entity.GetTransportType()], testNodePredicate, 6000, entity.PlaySiteLocation);
	}

	public static List<PathFinderNode> FindPathToComfort(DiscomfortMap dMap, Entity entity, byte discomfortBelowValue)
	{
		AStarSearch.DijkstraTestNodeDelegate testNodePredicate = (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate), new InfluenceMap.DiscomfortTileIsBelowValueParameters(dMap, discomfortBelowValue), The.Map.InfluenceMapTileIsComfortableInfo);
		Intelligence intelligence = entity.Intelligence;
		SubtileLayers mapCosts = intelligence.Allegiance.SharedKnowledge.GetMovementMap(intelligence.ProtectionLevel, entity.EntityType, ThreatStance.Bold).Layers[entity.GetTransportType()];
		return intelligence.PathPlanner.FindItemAndGetPath(mapCosts, testNodePredicate, 6000, entity.PlaySiteLocation);
	}

	protected bool ValidateSafetyAndTakeAction(byte? discomfortLevelCausingFail)
	{
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			DiscomfortMap discomfortMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
			float num = (int)discomfortMap.Map.GetValue(entity.MapPosition.Value);
			if (entityIntelligence.ThreatStance != ThreatStance.Bold && num > (float)entity.Intelligence.PanicLevel)
			{
				return PerformPanicFleeing(discomfortMap);
			}
			if (discomfortLevelCausingFail.HasValue && num > (float)(int)discomfortLevelCausingFail.Value)
			{
				base.Status = Status.Failed;
				return false;
			}
		}
		return true;
	}

	protected bool PerformPanicFleeing(DiscomfortMap dMap)
	{
		if (entity.Locomotor.LeggedLocomotor != null)
		{
			entityIntelligence.Brain.RemoveAllSubgoals();
			List<PathFinderNode> list = FindPathToSafety(dMap, entity);
			if (list != null)
			{
				GoalFollowPath goalFollowPath = new GoalFollowPath(entity, list, null, null, null, null);
				goalFollowPath.MovingOutOfHarmsWay = true;
				entityIntelligence.Brain.AddSubgoal(goalFollowPath);
				PathFinderNode pathFinderNode = list[list.Count - 1];
				entity.Locomotor.CurrentMoveTarget = MapManager.SubTileEdgeToWorldPos3(new Point(pathFinderNode.AbsoluteX, pathFinderNode.AbsoluteY));
				if (entity.DrivingVehicle.HasValue)
				{
					entityIntelligence.Brain.AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
				}
				else
				{
					SetFleeingSpeedType(entity);
				}
				entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.Fleeing, out var value);
				Goal.FireEventActions(entity, null, value);
				base.Status = Status.Active;
				return false;
			}
		}
		GoalDoTakeFive goalDoTakeFive = new GoalDoTakeFive(entity, 2.0);
		goalDoTakeFive.TestForDanger = false;
		entityIntelligence.Brain.AddSubgoal(goalDoTakeFive);
		base.Status = Status.Active;
		return false;
	}

	protected static void SetFleeingSpeedType(Entity entity)
	{
		if (!entity.EntityType.LocomotorType.CanRun || entity.Locomotor.LeggedLocomotor == null)
		{
			return;
		}
		bool flag = false;
		if (entity.EntityType.BiologicalType != null)
		{
			if (entity.BiologicalEntity.OxygenAndMuscleEnergy > GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Run;
		}
		else
		{
			entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkFast;
		}
	}

	protected void ChangeStance(StanceType stanceToTake)
	{
		AddSubgoal(new GoalChangeStance(entity, stanceToTake));
	}

	protected void HandleStanceChange(Regulator changeStanceRegulator, Dictionary<StancesType, List<ChanceToTakeStance>> possibleStances)
	{
		if (entity.HasStance() && changeStanceRegulator.IsReady())
		{
			ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(possibleStances));
		}
	}

	protected bool ResolveTools(List<EntityID> tools, ref List<IKnownEntityData> toolsData)
	{
		if (tools != null && tools.Count > 0)
		{
			toolsData = new List<IKnownEntityData>();
			foreach (EntityID tool in tools)
			{
				if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(tool, out var data)))
				{
					return false;
				}
				toolsData.Add(data);
			}
		}
		return true;
	}

	protected bool CheckHandTools(List<IKnownEntityData> toolsData, Job job)
	{
		if (toolsData != null)
		{
			foreach (IKnownEntityData toolsDatum in toolsData)
			{
				if (!ToolType.IsImmovable(toolsDatum.EntityType))
				{
					ToolHandlingType? toolHandling = toolsDatum.EntityType.ToolType.ToolHandling;
					ToolHandlingType toolHandlingType = ToolHandlingType.HandTool;
					if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && (!entity.AgentStorage.Contains(toolsDatum.EntityID) || (job != null && toolsDatum.AssignedToJob != job.ID)))
					{
						base.Status = Status.Failed;
						return false;
					}
				}
			}
		}
		return true;
	}

	protected void MountTool(List<IKnownEntityData> toolsData)
	{
		if (entity.AgentStorage == null)
		{
			return;
		}
		IKnownEntityData knownEntityData = null;
		if (toolsData != null)
		{
			foreach (IKnownEntityData toolsDatum in toolsData)
			{
				if (!ToolType.IsImmovable(toolsDatum.EntityType))
				{
					ToolHandlingType? toolHandling = toolsDatum.EntityType.ToolType.ToolHandling;
					ToolHandlingType toolHandlingType = ToolHandlingType.HandTool;
					if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && (knownEntityData == null || knownEntityData.EntityType.ItemType.AnimStatesWhenAttached == null))
					{
						knownEntityData = toolsDatum;
					}
				}
			}
		}
		if (knownEntityData != null)
		{
			entity.AgentStorage.MountedToolOrWeapon = knownEntityData.EntityID;
		}
		else
		{
			entity.AgentStorage.MountedToolOrWeapon = null;
		}
	}

	protected void MountReplenishTarget(Entity replenishTarget)
	{
		if (entity.AgentStorage != null)
		{
			if (replenishTarget != null && replenishTarget.EntityType.IsMountable() && entity.ContainsEntity(replenishTarget.ID))
			{
				entity.AgentStorage.MountedToolOrWeapon = replenishTarget.ID;
			}
			else
			{
				entity.AgentStorage.MountedToolOrWeapon = null;
			}
		}
	}

	protected bool GetInputInsideContainer(IKnownEntityData itemData, out bool carriedBySelf, out bool insideContainerWeCannotUse, out IKnownEntityData buildingWeCanEnter, out IKnownEntityData containerWeCanUnloadFrom)
	{
		carriedBySelf = false;
		buildingWeCanEnter = null;
		containerWeCanUnloadFrom = null;
		insideContainerWeCannotUse = false;
		if (itemData.ContainedBy.HasValue)
		{
			EntityID value = itemData.ContainedBy.Value;
			if (!EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(value, out var data)))
			{
				if (data == entity)
				{
					carriedBySelf = true;
					return true;
				}
				if (data.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
				{
					buildingWeCanEnter = data;
					return true;
				}
				if (data.EntityType.ContainerType.CanTransactWithContainer(entity.EntityType))
				{
					containerWeCanUnloadFrom = data;
					return true;
				}
				insideContainerWeCannotUse = true;
				return true;
			}
			return false;
		}
		return true;
	}

	protected bool UnloadOrDropUnToGround(IKnownEntityData itemData)
	{
		bool carriedBySelf = false;
		bool insideContainerWeCannotUse = false;
		IKnownEntityData buildingWeCanEnter = null;
		IKnownEntityData containerWeCanUnloadFrom = null;
		bool inputInsideContainer = GetInputInsideContainer(itemData, out carriedBySelf, out insideContainerWeCannotUse, out buildingWeCanEnter, out containerWeCanUnloadFrom);
		if (inputInsideContainer)
		{
			if (carriedBySelf)
			{
				AddSubgoal(new GoalDropItem(entity, itemData.EntityID));
				return true;
			}
			if (insideContainerWeCannotUse)
			{
				base.Status = Status.Failed;
				return false;
			}
			if (containerWeCanUnloadFrom != null)
			{
				AddSubgoal(new GoalUnload(entity, itemData.ContainedBy.Value, itemData.EntityID));
			}
		}
		return inputInsideContainer;
	}

	protected bool PickupItemOrUnloadFirst(IKnownEntityData itemData, bool mountAfterPickup = false, bool bendDown = true, bool standUpAfterwards = true, StorageCompartment compartment = StorageCompartment.Haul)
	{
		if (itemData.ContainedBy.HasValue)
		{
			EntityID value = itemData.ContainedBy.Value;
			if (EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(value, out var data)))
			{
				return false;
			}
			if (data == entity)
			{
				if (mountAfterPickup)
				{
					entity.AgentStorage.MountedToolOrWeapon = itemData.EntityID;
				}
				return true;
			}
			if (!data.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
			{
				if (data.EntityType.ContainerType is AgentStorageType)
				{
					base.Status = Status.Failed;
					return false;
				}
				AddSubgoal(new GoalUnload(entity, itemData.ContainedBy.Value, itemData.EntityID));
			}
		}
		AddSubgoal(new GoalPickup(entity, itemData.EntityID, null, compartment, mountAfterPickup, bendDown, standUpAfterwards));
		return true;
	}

	private bool IsFoodForEquipment(EntityID entityID)
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out var data)) && data.EntityType.ItemType != null && data.EntityType.ItemType.FoodType != null && entity.IsOwnedByUs(data) && entity.BiologicalEntity.IsEatable(data) && GoalEvaluator.IsValidPlaysiteItem(entity, data, mustCarryItem: true))
		{
			return true;
		}
		return false;
	}

	private bool IsGadgetForTask(EntityID entityID, List<ItemType.TaskType> taskTypes, bool taskIsBeyondNormalRange)
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out var data)) && data.EntityType.ItemType != null && data.EntityType.ItemType.TaskAppropriateLevels != null && data.EntityType.ItemType.FinalEffectsWhenEquipped != null && (data.EntityType.ItemType.UseGearAtAnyDistanceFromExpedition || taskIsBeyondNormalRange) && entity.IsOwnedByUs(data) && GoalEvaluator.IsValidPlaysiteItem(entity, data, mustCarryItem: true))
		{
			return true;
		}
		return false;
	}

	private bool IsWeaponForEquipment(EntityID entityID)
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out var data)) && data.EntityType.ItemType != null && data.EntityType.ItemType.WeaponType != null && entity.IsOwnedByUs(data) && EvaluateAttackJobs.IsValidPlaysiteWeapon(entity, data, null, null, null, doNotCheckAmmunitionAndBulk: true))
		{
			return true;
		}
		return false;
	}

	protected GoalEvaluator.CalculateResult GetNearbyEntities(float radiusToLookIn, out List<IKnownEntityData> results, ThreatStance stanceToUse, RegionMap footRegionMap, Predicate<EntityID> filter, Dictionary<EntityID, float> cachedDistances)
	{
		results = new List<IKnownEntityData>();
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		List<Pair<EntityID, Vector2>> airDistanceResultList = null;
		sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entityIntelligence.CurrentExpedition.Location.Value.ToVector2(), radiusToLookIn, filter, ref airDistanceResultList);
		List<Pair<EntityID, Vector2>> resultsList = null;
		sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 80f, (EntityID e) => (airDistanceResultList == null || !airDistanceResultList.Exists((Pair<EntityID, Vector2> n) => n.First == e)) && filter(e), ref resultsList);
		if (resultsList != null)
		{
			if (airDistanceResultList != null)
			{
				airDistanceResultList.AddRange(resultsList);
			}
			else
			{
				airDistanceResultList = resultsList;
			}
		}
		entity.AgentStorage.IterateContained(delegate(Entity e)
		{
			if ((airDistanceResultList == null || !airDistanceResultList.Exists((Pair<EntityID, Vector2> n) => n.First == e.EntityID)) && filter(e.EntityID))
			{
				airDistanceResultList.Add(new Pair<EntityID, Vector2>(e.EntityID, e.PlaySiteLocation.ToVector2()));
			}
		});
		if (airDistanceResultList != null)
		{
			float distance = 0f;
			foreach (Pair<EntityID, Vector2> item in airDistanceResultList)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(item.First, out var data)))
				{
					switch (footRegionMap.GetDistanceToEntity(entity, entity, data, ref distance))
					{
					case RegionMap.Result.OK:
						results.Add(data);
						cachedDistances.Add(data.EntityID, distance);
						break;
					case RegionMap.Result.Wait:
						return GoalEvaluator.CalculateResult.Processing;
					}
				}
			}
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private bool GetCarriedAmmunition(Entity agent, List<ItemDistance> itemListToFill, EntityType ammunitionType)
	{
		foreach (EntityID storedItem in base.entity.AgentStorage.Equipment.StoredItems)
		{
			Entity entity = Entity.FindByID(storedItem);
			bool flag = false;
			foreach (ItemDistance item in itemListToFill)
			{
				if (item.Entity == entity)
				{
					flag = true;
				}
			}
			if (!flag && entity.EntityType == ammunitionType)
			{
				itemListToFill.Add(new ItemDistance
				{
					Entity = entity,
					Distance = 0f
				});
			}
		}
		return itemListToFill.Count > 0;
	}

	public static bool ItemIsNotAssignedToImportantJobs(IKnownEntityData data, SharedKnowledge sharedKnowledge)
	{
		if (!data.IsUnassigned(sharedKnowledge))
		{
			return false;
		}
		Job job = EvaluateJob.ResolveAssignedToJob(data);
		if (job != null)
		{
			if (job.TakenBy.Count == 0 && job is HaulingJob)
			{
				return (job as HaulingJob).RequiredByProcessJob == null;
			}
			return false;
		}
		return true;
	}

	private void ScoreOptionalFoodOrGear(List<IKnownEntityData> items, out IKnownEntityData bestItem, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, float radiusToLookIn, Dictionary<EntityID, float> cachedDistances, List<ItemType.TaskType> taskTypes)
	{
		bestItem = null;
		float value = 0f;
		double num = 0.0;
		foreach (IKnownEntityData item in items)
		{
			if (!cachedDistances.TryGetValue(item.EntityID, out value) || !ItemIsNotAssignedToImportantJobs(item, sharedKnowledge))
			{
				continue;
			}
			float num2 = ((item.EntityType.ItemType.FoodType != null) ? 0.5f : item.EntityType.ItemType.GetTaskAppropriateLevel(taskTypes));
			if (num2 > 0f)
			{
				double travelScoreFromDistance = GoalEvaluator.GetTravelScoreFromDistance(entity, value);
				double num3 = ((item.EntityType.ItemType.FoodType != null) ? EvaluateEat.ScoreCondition(item, 0.5) : 1.0);
				double num4 = travelScoreFromDistance * 0.30000001192092896 + (double)(num2 * 0.5f) + num3 * 0.20000000298023224;
				if (num4 >= num)
				{
					bestItem = item;
					num = num4;
				}
			}
		}
	}

	private void ScoreOptionalWeapons(List<IKnownEntityData> weapons, out List<EntityID> ammunitionToUse, out EntityType ammunitionType, out IKnownEntityData bestWeapon, RegionMap footRegionMap, float radiusToLookIn, Dictionary<EntityID, float> cachedDistances)
	{
		ammunitionToUse = null;
		ammunitionType = null;
		bestWeapon = null;
		if (weapons.Count == 0)
		{
			return;
		}
		float value = 0f;
		double num = 0.0;
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		bool flag = false;
		foreach (IKnownEntityData weapon in weapons)
		{
			if (!LookUpOwners.ResolveEntityOwner(weapon, out EntityGroup ownedEntities) || ownedEntities == null || !cachedDistances.TryGetValue(weapon.EntityID, out value))
			{
				continue;
			}
			flag = false;
			if (!ItemIsNotAssignedToImportantJobs(weapon, sharedKnowledge))
			{
				continue;
			}
			double travelScoreFromDistance = GoalEvaluator.GetTravelScoreFromDistance(entity, value);
			float taskAppropriateLevel = weapon.EntityType.ItemType.GetTaskAppropriateLevel(ItemType.TaskType.LongerJourneys);
			double num2 = travelScoreFromDistance * 0.30000001192092896 + (double)(taskAppropriateLevel * 0.7f);
			if (bestWeapon != null && !(num2 > num))
			{
				continue;
			}
			AttackType[] attackTypes = weapon.EntityType.ItemType.WeaponType.AttackTypes;
			foreach (AttackType attackType in attackTypes)
			{
				if (attackType.UsesAmmo == null)
				{
					continue;
				}
				flag = true;
				if (!weapon.HasEnoughAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value))
				{
					List<ItemDistance> sortedList = new List<ItemDistance>();
					GoalEvaluator.GetAllReplenishItemsSortedByDistance(entity, attackType.UsesAmmoType, ownedEntities, entityIntelligence.Allegiance.SharedKnowledge, footRegionMap, ref sortedList, radiusToLookIn, The.Sim.PlaySite.GetFirstPlayerExpedition().Location.Value.ToVector2(), radiusToLookIn);
					GetCarriedAmmunition(entity, sortedList, attackType.UsesAmmoType);
					if (sortedList.Count <= 0)
					{
						continue;
					}
					int num3 = 0;
					List<EntityID> list = new List<EntityID>();
					foreach (ItemDistance item in sortedList)
					{
						if (ItemIsNotAssignedToImportantJobs(item.Entity, sharedKnowledge))
						{
							num3 += item.Entity.NoOfRounds.Value;
							list.Add(item.Entity.EntityID);
							if (num3 >= attackType.RoundsToSpend)
							{
								break;
							}
						}
					}
					if (num3 >= attackType.RoundsToSpend)
					{
						ammunitionToUse = list;
						ammunitionType = attackType.UsesAmmoType;
						bestWeapon = weapon;
						num = num2;
					}
				}
				else
				{
					ammunitionToUse = null;
					bestWeapon = weapon;
					num = num2;
				}
			}
			if (!flag)
			{
				ammunitionToUse = null;
				num = num2;
				bestWeapon = weapon;
			}
		}
	}

	protected GoalEvaluator.CalculateResult FindOptionalEquipmentIfNeeded(Vector3 destination, Job job, bool equipWeapon = true, bool equipFood = true, bool mountWeapon = true, List<ItemType.TaskType> taskTypesForGear = null)
	{
		if (entity.AgentStorage == null || entity.AgentStorage.Equipment == null || !entity.EntityType.IntelligenceType.CanMountToolsOrWeapons() || job == null)
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		bool distanceOKToBringFood = false;
		bool distanceOKToBringEquipment = false;
		bool distanceOKToBringEquipment2 = false;
		ThreatStance threatStance = ((!RequiresBoldStance()) ? entity.Intelligence.ThreatStance : ThreatStance.Bold);
		float num = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, destination);
		float? num2 = Math.Min(num, GameData.Instance.AIConstants.MaxRadiusFromExpeditionToGatherOptionalEquipment);
		RegionMap regionMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, entity.EntityType, threatStance).Layers[SurfaceType.TransportType.Foot].RegionMap;
		IsEquipmentNeeded(num, out distanceOKToBringEquipment2, out distanceOKToBringFood, out distanceOKToBringEquipment);
		List<IKnownEntityData> results = null;
		List<IKnownEntityData> results2 = null;
		List<IKnownEntityData> results3 = null;
		Dictionary<EntityID, float> cachedDistances = new Dictionary<EntityID, float>();
		if (equipWeapon && distanceOKToBringFood)
		{
			bool? canUseWeapons = entity.EntityType.IntelligenceType.CanUseWeapons;
			bool flag = true;
			if (canUseWeapons == true == flag && canUseWeapons.HasValue && GetNearbyEntities(num2.Value, out results, threatStance, regionMap, IsWeaponForEquipment, cachedDistances) == GoalEvaluator.CalculateResult.Processing)
			{
				return GoalEvaluator.CalculateResult.Processing;
			}
		}
		if (taskTypesForGear != null)
		{
			bool? canUseWeapons = entity.EntityType.IntelligenceType.CanUseGadgets;
			bool flag = true;
			if (canUseWeapons == true == flag && canUseWeapons.HasValue && GetNearbyEntities(num2.Value, out results3, threatStance, regionMap, (EntityID e) => IsGadgetForTask(e, taskTypesForGear, distanceOKToBringEquipment2), cachedDistances) == GoalEvaluator.CalculateResult.Processing)
			{
				return GoalEvaluator.CalculateResult.Processing;
			}
		}
		if (equipFood && distanceOKToBringEquipment && entity.EntityType.BiologicalType != null && GetNearbyEntities(num2.Value, out results2, threatStance, regionMap, IsFoodForEquipment, cachedDistances) == GoalEvaluator.CalculateResult.Processing)
		{
			return GoalEvaluator.CalculateResult.Processing;
		}
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		if (results != null)
		{
			ScoreOptionalWeaponsAndEquipIt(threatStance, job, regionMap, num2.Value, results, cachedDistances, mountWeapon);
		}
		if (results2 != null)
		{
			ScoreOptionalFoodOrGearAndEquipIt(threatStance, job, regionMap, sharedKnowledge, num2.Value, results2, cachedDistances, longerJourneyTask);
		}
		if (results3 != null)
		{
			ScoreOptionalFoodOrGearAndEquipIt(threatStance, job, regionMap, sharedKnowledge, num2.Value, results3, cachedDistances, taskTypesForGear);
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	protected static void AddNightActivityGear(ref List<ItemType.TaskType> gearTasks)
	{
		if (!The.Sim.DateAndTime.SunIsUp)
		{
			Common.AddToList(ref gearTasks, ItemType.TaskType.NightActivities);
		}
	}

	private void ScoreOptionalFoodOrGearAndEquipIt(ThreatStance stanceToUse, Job job, RegionMap regionMapToUse, SharedKnowledge sharedKnowledge, float radiusToLookIn, List<IKnownEntityData> food, Dictionary<EntityID, float> cachedDistances, List<ItemType.TaskType> taskTypes)
	{
		ScoreOptionalFoodOrGear(food, out var bestItem, sharedKnowledge, regionMapToUse, radiusToLookIn, cachedDistances, taskTypes);
		if (bestItem != null && PickupItemOrUnloadFirst(bestItem, mountAfterPickup: false, bendDown: true, standUpAfterwards: true, StorageCompartment.Equipment))
		{
			SetLockOnOptionalEquipment(job, bestItem);
		}
	}

	private void ScoreOptionalWeaponsAndEquipIt(ThreatStance stanceToUse, Job job, RegionMap regionMapToUse, float radiusToLookIn, List<IKnownEntityData> weapons, Dictionary<EntityID, float> cachedDistances, bool mountWeapon)
	{
		ScoreOptionalWeapons(weapons, out var ammunitionToUse, out var ammunitionType, out var bestWeapon, regionMapToUse, radiusToLookIn, cachedDistances);
		if (bestWeapon == null || !PickupItemOrUnloadFirst(bestWeapon, mountWeapon, bendDown: true, standUpAfterwards: true, StorageCompartment.Equipment))
		{
			return;
		}
		if (ammunitionToUse != null)
		{
			ProcessType processType = bestWeapon.EntityType.ContainerType.GetReplenishProcesses()[ammunitionType];
			AddSubgoal(new GoalReplenish(entity, bestWeapon.GetAsEntityAndRoot(), ammunitionToUse, processType, null, job, StorageCompartment.Equipment));
			foreach (EntityID item in ammunitionToUse)
			{
				IKnownEntityData data = null;
				if (EntityResultCausesFailedGoal(entity.Intelligence.GetKnownData(item, out data)))
				{
					return;
				}
				SetLockOnOptionalEquipment(job, data);
			}
		}
		SetLockOnOptionalEquipment(job, bestWeapon);
	}

	private void SetLockOnOptionalEquipment(Job job, IKnownEntityData data)
	{
		if (!data.EntityType.IsIntrinsic())
		{
			data.AssignedToJob = job?.ID;
			optionalEquipmentAssignedToThisJob.Add(data.EntityID);
		}
	}

	private void IsEquipmentNeeded(float groundDistanceToDestinationFromCamp, out bool distanceOKToBringWeapons, out bool distanceOKToBringFood, out bool distanceOKToBringEquipment)
	{
		distanceOKToBringWeapons = false;
		distanceOKToBringFood = false;
		distanceOKToBringEquipment = false;
		if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalWeapons)
		{
			distanceOKToBringWeapons = true;
		}
		if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalFood)
		{
			distanceOKToBringFood = true;
		}
		if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalEquipment)
		{
			distanceOKToBringEquipment = true;
		}
	}

	protected void RemoveLocksOnOptionalEquipment(JobID? jobID)
	{
		if (optionalEquipmentAssignedToThisJob == null)
		{
			return;
		}
		foreach (EntityID item in optionalEquipmentAssignedToThisJob)
		{
			RemoveLockOnToolOrWeapon(jobID, item);
		}
	}

	protected bool IsToolOrWeaponOK(EntityID entityID, out IKnownEntityData weaponData, bool setStatusToFailed = true)
	{
		weaponData = null;
		EntityResult knownData = entityIntelligence.GetKnownData(entityID, out weaponData);
		if (UsedEntityResultShouldFailGoal(knownData))
		{
			if (setStatusToFailed)
			{
				base.Status = Status.Failed;
			}
			return false;
		}
		return IsToolOrWeaponOK(weaponData);
	}

	protected bool IsToolOrWeaponOK(IKnownEntityData entityData)
	{
		if (!Entity.IsFunctional(entityData) || !entityData.OwnedBy.HasValue)
		{
			return false;
		}
		return true;
	}

	protected bool IsToolOrWeaponOK(EntityAndRoot? entityID, out IKnownEntityData weaponData)
	{
		weaponData = null;
		if (entityID.HasValue)
		{
			IsToolOrWeaponOK(entityID.Value.Entity, out weaponData);
		}
		return true;
	}

	protected bool IsToolOrWeaponOK(EntityID? entityID, out IKnownEntityData weaponData)
	{
		weaponData = null;
		if (entityID.HasValue)
		{
			IsToolOrWeaponOK(entityID.Value, out weaponData);
		}
		return true;
	}

	protected bool AreToolsOK(List<EntityID> tools)
	{
		if (tools != null)
		{
			foreach (EntityID tool in tools)
			{
				if (!IsToolOrWeaponOK(tool, out var _))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected bool IsOutputOKAndNotCompleted(ProcessJob job, bool mustExist)
	{
		if (job != null)
		{
			if (!job.IsCompleted(out var isCompleted) || isCompleted)
			{
				return false;
			}
			if (mustExist && (!job.OutputExists(out var outputExists) || !outputExists))
			{
				return false;
			}
		}
		return true;
	}

	protected void DropUnneededItemsToMakeCapacity(float neededCapacity, Predicate<Entity> okToDrop, out float totalDroppedItems, StorageCompartment compartment)
	{
		totalDroppedItems = 0f;
		ItemStorage compartment2 = entity.AgentStorage.GetCompartment(compartment);
		if (!compartment2.HasCapacityForItem(neededCapacity))
		{
			float value = neededCapacity - compartment2.UnusedCapacity;
			DropUnneededItems(okToDrop, out var totalDropped, compartment, value);
			totalDroppedItems = totalDropped;
		}
	}

	protected bool GatherToolsOrWeapons(EntityAndRoot? weapon, List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment)
	{
		if (weapon.HasValue)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(weapon.Value.Entity, out var data)))
			{
				return false;
			}
			List<IKnownEntityData> list = new List<IKnownEntityData>();
			list.Add(data);
			GatherToolsOrWeapons(list, ownersOfVehicles, job, mountAfterPickup: true, compartment);
		}
		return true;
	}

	protected bool GatherToolsOrWeapons(List<IKnownEntityData> items, List<EntityGroupID> ownersOfVehicles, Job job, bool mountAfterPickup, StorageCompartment compartment)
	{
		if (items != null)
		{
			float? num = null;
			foreach (IKnownEntityData item in items)
			{
				if (item.EntityType.IsIntrinsic() || ToolType.IsImmovable(item.EntityType))
				{
					continue;
				}
				if (!entity.AgentStorage.Contains(item.EntityID))
				{
					AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, item)
					{
						IsFinalDestination = true
					});
					DropUnneededItemsToMakeCapacity(item.Bulk, (Entity e) => !items.Contains(e), out var totalDroppedItems, compartment);
					if (!num.HasValue)
					{
						num = entity.AgentStorage.GetCompartment(compartment).UnusedCapacity;
					}
					num = num - item.Bulk + totalDroppedItems;
					if (!PickupItemOrUnloadFirst(item, mountAfterPickup, bendDown: true, standUpAfterwards: true, compartment))
					{
						return false;
					}
				}
				else if (mountAfterPickup)
				{
					entity.AgentStorage.MountedToolOrWeapon = item.EntityID;
				}
				else
				{
					entity.AgentStorage.MoveCarriedItemToCompartment((Entity)item, compartment);
				}
			}
		}
		SetLocksOnToolsOrWeapons(items, job);
		return true;
	}

	protected bool AssignWeapon(Job job, EntityAndRoot? weapon)
	{
		if (weapon.HasValue)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(weapon.Value.Entity, out var data)))
			{
				return false;
			}
			if (job != null)
			{
				data.AssignedToJob = job.ID;
			}
			else
			{
				data.AssignedToJob = null;
			}
		}
		return true;
	}

	protected bool SetLocksOnReplenishItems(Job job, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActions)
	{
		if (replenishActions != null)
		{
			foreach (KeyValuePair<EntityAndRoot, List<ReplenishItemsForAction>> replenishAction in replenishActions)
			{
				if (!SetLocksOnReplenishItems(job, replenishAction.Value))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected bool RemoveLocksOnReplenishItems(JobID? job, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActions)
	{
		if (replenishActions != null)
		{
			foreach (KeyValuePair<EntityAndRoot, List<ReplenishItemsForAction>> replenishAction in replenishActions)
			{
				if (!RemoveLocksOnReplenishItems(job, replenishAction.Value))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected bool SetLocksOnReplenishItems(Job job, List<ReplenishItemsForAction> replenishActions)
	{
		if (replenishActions != null)
		{
			foreach (ReplenishItemsForAction replenishAction in replenishActions)
			{
				foreach (EntityID item in replenishAction.Items)
				{
					if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(item, out var data)))
					{
						return false;
					}
					if (data != null && job != null)
					{
						data.AssignedToJob = job.ID;
					}
				}
			}
		}
		return true;
	}

	protected bool RemoveLocksOnReplenishItems(JobID? job, List<ReplenishItemsForAction> replenishActions)
	{
		if (replenishActions != null)
		{
			foreach (ReplenishItemsForAction replenishAction in replenishActions)
			{
				foreach (EntityID item in replenishAction.Items)
				{
					RemoveLockOnToolOrWeapon(job, item);
				}
			}
			if (entity.Contains != null && job.HasValue)
			{
				entity.Contains.IterateContained(delegate(Entity e)
				{
					RemoveLocksOnSurplusItems(job.Value, replenishActions, e);
				});
			}
		}
		return true;
	}

	private void RemoveLocksOnSurplusItems(JobID job, List<ReplenishItemsForAction> replenishActions, Entity item)
	{
		JobID? assignedToJob = item.AssignedToJob;
		if (assignedToJob.GetValueOrDefault() == job && assignedToJob.HasValue && replenishActions.Exists((ReplenishItemsForAction p) => p.Action.InputsByType.ContainsKey(item.EntityType)))
		{
			RemoveLockOnToolOrWeapon(job, item.ID);
		}
	}

	private void DropUnneededItems(Predicate<Entity> okToDrop, out float totalDropped, StorageCompartment compartment, float? bulkNeededToDrop = null)
	{
		float totalDroppedParam = 0f;
		entity.AgentStorage.ItemStorage.IterateContainedBreakOnTrue((Entity itemEntity) => DropUnneededItem(itemEntity, entity, okToDrop, bulkNeededToDrop, ref totalDroppedParam));
		totalDropped = totalDroppedParam;
	}

	public bool DropUnneededItem(Entity itemEntity, Entity carrierEntity, Predicate<Entity> okToDrop, float? bulkNeededToDrop, ref float bulkAlreadyDropped)
	{
		if (okToDrop(itemEntity))
		{
			AddSubgoal(new GoalDropItem(carrierEntity, itemEntity.EntityID));
			bulkAlreadyDropped += itemEntity.Bulk;
			if (bulkNeededToDrop.HasValue && Common.IsLessThanOrEqual(bulkNeededToDrop.Value, bulkAlreadyDropped))
			{
				return true;
			}
		}
		return false;
	}

	protected void RemoveProcessToolLocks(ProcessJob jobToUnlock, List<EntityID> Tools = null, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null, List<ReplenishItemsForAction> replenishActionsList = null, EntityAndRoot? weapon = null)
	{
		GoalDoProduce.GetMobileTools(Tools, out var mobileTools);
		GoalDoProduce.GetHandTools(Tools, out var handTools);
		JobID? jobID = jobToUnlock?.ID;
		List<EntityID> tools;
		if (jobToUnlock.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded)
		{
			tools = mobileTools;
		}
		else
		{
			tools = ((!(jobToUnlock.IsStarted(out var isStarted) && isStarted)) ? mobileTools : handTools);
			ClearInUseBy(entity, mobileTools);
		}
		RemoveLocksFromJob(jobID, tools, replenishActionsDictionary, replenishActionsList, weapon);
		GoalDoProduce.GetImmobileTools(Tools, out var immobileTools);
		ClearInUseBy(entity, immobileTools);
	}

	protected void ClearInUseBy(Entity entity, List<EntityID> tools)
	{
		foreach (EntityID tool in tools)
		{
			entityIntelligence.GetKnownData(tool, out var data);
			if (data != null)
			{
				if (data.EntityType.IsIntrinsic())
				{
					break;
				}
				entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
			}
		}
	}

	protected void RemoveLocksFromJob(Job jobToUnlock, List<EntityID> Tools = null, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null, List<ReplenishItemsForAction> replenishActionsList = null, EntityAndRoot? weapon = null, bool isDestroyingJob = false)
	{
		RemoveLocksFromJob(jobToUnlock?.ID, Tools, replenishActionsDictionary, replenishActionsList, weapon, isDestroyingJob);
	}

	protected void RemoveLocksFromJob(JobID? jobID, List<EntityID> Tools = null, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null, List<ReplenishItemsForAction> replenishActionsList = null, EntityAndRoot? weapon = null, bool isDestroyingJob = false)
	{
		Job job = LookUp<Job, JobID>.FindByID(jobID);
		job?.Abandon(entity, isDestroyingJob);
		if (job is ProcessJob pJob)
		{
			RemoveLockOnActingOn(pJob);
		}
		RemoveLocksOnOptionalEquipment(jobID);
		if (Tools != null)
		{
			RemoveLocksOnTools(Tools, jobID);
		}
		if (replenishActionsDictionary != null)
		{
			RemoveLocksOnReplenishItems(jobID, replenishActionsDictionary);
		}
		else if (replenishActionsList != null)
		{
			RemoveLocksOnReplenishItems(jobID, replenishActionsList);
		}
		if (weapon.HasValue)
		{
			RemoveLockOnToolOrWeapon(jobID, weapon.Value);
		}
	}

	private void RemoveLockOnActingOn(ProcessJob pJob)
	{
		if (pJob.GetActingOnEntity(out EntityID? actingOnEntity) && actingOnEntity.HasValue)
		{
			entityIntelligence.GetKnownData(actingOnEntity.Value, out var data);
			if (data != null && data.AssignedToJob.HasValue && data.AssignedToJob == pJob.ID)
			{
				data.AssignedToJob = null;
			}
		}
	}

	protected void DestroyJobAndRemoveLocks<T>(ref T jobToDestroy, List<EntityID> Tools = null, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools = null, List<ReplenishItemsForAction> replenishActions = null, EntityAndRoot? weapon = null) where T : Job
	{
		RemoveLocksFromJob(jobToDestroy, Tools, replenishItemsForTools, replenishActions, weapon, isDestroyingJob: true);
		Entity entityToExclude = entity;
		jobToDestroy.Destroy(cancelTakers: true, entityToExclude);
		jobToDestroy = null;
	}

	protected void RemoveLocksOnTools(List<EntityID> tools, JobID? job)
	{
		foreach (EntityID tool in tools)
		{
			RemoveLockOnToolOrWeapon(job, tool);
		}
	}

	protected void RemoveLockOnToolOrWeapon(JobID? jobID, EntityAndRoot? toolOrWeapon)
	{
		if (toolOrWeapon.HasValue)
		{
			RemoveLockOnToolOrWeapon(jobID, toolOrWeapon.Value.Entity);
		}
	}

	protected void RemoveLockOnToolOrWeapon(JobID? jobID, EntityID? toolOrWeapon)
	{
		if (!toolOrWeapon.HasValue)
		{
			return;
		}
		entityIntelligence.GetKnownData(toolOrWeapon.Value, out var data);
		if (data == null || data.EntityType.IsIntrinsic())
		{
			return;
		}
		Job job = LookUp<Job, JobID>.FindByID(jobID);
		if (job != null && job is ProcessJob processJob)
		{
			processJob.GetKnownProgress(out var progress);
			if (!ToolType.IsImmovable(data.EntityType) || NonLivingEntity.IsCompleted(progress))
			{
				processJob.UnassignTool(data);
			}
		}
		if (data.AssignedToJob.HasValue && data.AssignedToJob == jobID)
		{
			data.AssignedToJob = null;
		}
		entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
	}

	protected void SetLocksOnToolsOrWeapons(List<IKnownEntityData> items, Job job)
	{
		if (items == null)
		{
			return;
		}
		foreach (IKnownEntityData item in items)
		{
			SetLocksOnToolOrWeapon(job, item);
		}
	}

	protected void SetLocksOnToolsOrWeapons(List<EntityID> toolsOrWeapons, Job job)
	{
		foreach (EntityID toolsOrWeapon in toolsOrWeapons)
		{
			SetLocksOnToolOrWeapon(job, toolsOrWeapon);
		}
	}

	protected void SetLocksOnToolOrWeapon(Job job, EntityID toolOrWeapon)
	{
		entityIntelligence.GetKnownData(toolOrWeapon, out var data);
		if (data != null)
		{
			SetLocksOnToolOrWeapon(job, data);
		}
	}

	private void SetLocksOnToolOrWeapon(Job job, IKnownEntityData toolOrWeapon)
	{
		if (!toolOrWeapon.EntityType.IsIntrinsic())
		{
			ProcessJob processJob = job as ProcessJob;
			entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(toolOrWeapon.EntityID, entity.EntityID);
			if (processJob != null && toolOrWeapon.EntityType.ToolType != null)
			{
				processJob.AssignTool(toolOrWeapon);
			}
			else
			{
				toolOrWeapon.AssignedToJob = job.ID;
			}
		}
	}

	protected void ReplenishToolsOrWeapons(Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment = StorageCompartment.Haul)
	{
		if (replenishItemsForTools == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTool in replenishItemsForTools)
		{
			foreach (ReplenishItemsForAction item in replenishItemsForTool.Value)
			{
				AddSubgoal(new GoalReplenish(entity, replenishItemsForTool.Key, item.Items, item.Action, ownersOfVehicles, job, compartment));
			}
		}
	}

	protected void PrepareTools(List<IKnownEntityData> tools, List<EntityGroupID> ownersOfVehicles)
	{
		if (tools == null)
		{
			return;
		}
		foreach (IKnownEntityData tool in tools)
		{
			if (tool.EntityType.ToolType.PrepareProcessType != null && tool.IsPrepared.HasValue && !tool.IsPrepared.Value)
			{
				AddSubgoal(new GoalProduce(entity, tool.EntityType.ToolType.PrepareProcessType, tool.GetAsEntityAndRoot(), null, ownersOfVehicles, null, null, null, null));
			}
		}
	}

	protected void PlaceStationaryToolsAtWorkSite(List<IKnownEntityData> tools, Vector3? location)
	{
		if (tools == null)
		{
			return;
		}
		foreach (IKnownEntityData tool in tools)
		{
			ToolHandlingType? toolHandling = tool.EntityType.ToolType.ToolHandling;
			ToolHandlingType toolHandlingType = ToolHandlingType.Stationary;
			if (toolHandling.GetValueOrDefault() == toolHandlingType && toolHandling.HasValue && !ToolType.IsImmovable(tool.EntityType))
			{
				AddSubgoal(new GoalDropItem(entity, tool.EntityID, null, location, null));
			}
		}
	}

	public override string GetStatus()
	{
		if (Subgoals.Count > 0)
		{
			string text = Subgoals.Peek().GetStatus();
			if (text != "")
			{
				return text;
			}
			return base.GetStatus();
		}
		return base.GetStatus();
	}

	public virtual void RemoveAllSubgoals()
	{
		while (Subgoals.Count > 0)
		{
			RemoveFirstSubgoal().Terminate();
		}
	}

	public override void Terminate()
	{
		RemoveAllSubgoals();
		base.Terminate();
	}

	public override void AddSubgoal(Goal g)
	{
		if (base.ID == GoalID.Invalid)
		{
			_ = SkipThisAssert;
			return;
		}
		if (entity.PersonEntity != null && g is GoalWait && Subgoals.Count > 2)
		{
			_ = Subgoals.Peek() is GoalWait;
		}
		bool num = Subgoals.Count == 0;
		Subgoals.Enqueue(g);
		if (num)
		{
			g.EnterIfNew();
		}
	}

	public override bool HandleMessage(Message message)
	{
		return ForwardMessageToFrontMostSubgoal(message);
	}

	protected bool ForwardMessageToFrontMostSubgoal(Message message)
	{
		if (The.Sim.TotalUnPausedGameTimeInSeconds > 24.0 && entity.ID == (EntityID)19L && message.MessageType != Message.MessageTypes.Hit)
		{
			_ = message.MessageType;
			_ = 28;
		}
		if (Subgoals.Count > 0 && Subgoals.Peek().isActive())
		{
			return Subgoals.Peek().HandleMessage(message);
		}
		return false;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		optionalEquipmentAssignedToThisJob = sn.DoList(optionalEquipmentAssignedToThisJob);
		ownersOfVehicles = sn.DoList(ownersOfVehicles);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (Goal subgoal in Subgoals)
			{
				snapshotSubgoals.Enqueue(subgoal.ID);
			}
		}
		snapshotSubgoals = sn.DoQueue(snapshotSubgoals);
		sn.Ignore(SkipThisAssert);
		sn.Ignore(Subgoals);
		sn.Ignore(longerJourneyTask);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		entity.ToString().Contains("Millet");
		foreach (GoalID snapshotSubgoal in snapshotSubgoals)
		{
			Subgoals.Enqueue(LookUpGoals.FindByID(snapshotSubgoal));
		}
		snapshotSubgoals.Clear();
	}
}
