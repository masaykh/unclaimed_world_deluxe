using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalEat : CompositeGoal, IIDEventSubscriber, ITopLevelGoal
{
	private enum State
	{
		Moving,
		Extracting,
		Consuming
	}

	private State currentState;

	private EntityID? itemToConsumeID;

	private EntityID? itemToExtractFromID;

	private List<EntityID> additionalItemsToConsume = new List<EntityID>();

	private EntityID? extractedItemID;

	private EntityID? placeToEat;

	private EntityGroupID? ownerOfFoodItem;

	private bool itemIsDrunk;

	private ProcessType extractionProcessToUse;

	private float? maxAmountToExtract;

	private MethodID? extractionCompleteMethodID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalEat(Entity entity, EntityID foodItem, EntityID? placeToEat, EntityGroupID? ownerOfFoodItem, ProcessType extractionProcessToUse, List<EntityGroupID> ownersVehicles)
		: base(entity)
	{
		_ = entity.ID;
		_ = 4600;
		this.ownerOfFoodItem = ownerOfFoodItem;
		ownersOfVehicles = ownersVehicles;
		if (extractionProcessToUse == null)
		{
			itemToConsumeID = foodItem;
		}
		else
		{
			itemToExtractFromID = foodItem;
			this.extractionProcessToUse = extractionProcessToUse;
		}
		this.placeToEat = placeToEat;
	}

	public GoalEat()
	{
	}

	private bool IsConsuming()
	{
		return itemToConsumeID.HasValue;
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		EntityID? entityID = itemToConsumeID ?? itemToExtractFromID;
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityID.Value, out var data)))
		{
			return;
		}
		if (data.EntityType.ItemType.FoodType != null)
		{
			itemIsDrunk = data.EntityType.ItemType.FoodType.IsDrunk;
		}
		if (IsConsuming() && entity.EntityType.BiologicalType.HoldsFoodWhenEating)
		{
			IKnownEntityData knownEntityData = data;
			List<IKnownEntityData> list = new List<IKnownEntityData>();
			list.Add(knownEntityData);
			BiologicalEntity bioEntity = entity.BiologicalEntity;
			float totalFoodBulk;
			if (data.NutrientBulkAmounts.Any((KeyValuePair<FoodNutrientType, float> k) => bioEntity.Needs.IsEssential(k.Key)))
			{
				GetAdditionalItemsToConsume(knownEntityData, list, out totalFoodBulk);
			}
			else
			{
				totalFoodBulk = knownEntityData.Bulk;
			}
			foreach (IKnownEntityData item in list)
			{
				entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(item.EntityID, entity.EntityID);
				AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, item)
				{
					IsFinalDestination = true
				});
				if (item == knownEntityData)
				{
					DropUnneededItemsToMakeCapacity(totalFoodBulk, (Entity e) => entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(e.ID) != entity.ID, out var _, StorageCompartment.Haul);
				}
				if (!PickupItemOrUnloadFirst(item))
				{
					return;
				}
			}
		}
		else
		{
			DropUnneededItemsToMakeCapacity(data.Bulk, (Entity e) => entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(e.ID) != entity.ID, out var _, StorageCompartment.Haul);
			if (!UnloadOrDropUnToGround(data))
			{
				return;
			}
		}
		if (placeToEat.HasValue)
		{
			entityIntelligence.GetKnownData(placeToEat.Value, out var data2);
			if (data2 != null)
			{
				if (data2.GatheringSite != null)
				{
					if (data2.GatheringSite.CanAddVisitor(ref entity))
					{
						AddSubgoal(new GoalArriveAsVisitor(entity, placeToEat.Value, ownersOfVehicles));
					}
				}
				else
				{
					AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, data2)
					{
						IsFinalDestination = true
					});
				}
			}
		}
		currentState = State.Moving;
	}

	private GoalEvaluator.CalculateResult GetAdditionalItemsToConsume(IKnownEntityData firstItem, List<IKnownEntityData> allFoodItems, out float totalFoodBulk)
	{
		Dictionary<FoodNutrientType, float> essentialNeedBulkAmounts = entity.BiologicalEntity.Needs.GetEssentialNeedBulkAmounts();
		totalFoodBulk = firstItem.Bulk;
		SubtractNutrients(firstItem, essentialNeedBulkAmounts);
		if (!NeedsMoreFood(essentialNeedBulkAmounts))
		{
			return GoalEvaluator.CalculateResult.Done;
		}
		additionalItemsToConsume = new List<EntityID>();
		BiologicalEntity bioEntity = entity.BiologicalEntity;
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		EntityGroup foodItemsGroup = EvaluateEat.GetFoodEntityGroup(entity);
		ThreatStance threatStance = entityIntelligence.ThreatStance;
		RegionMap regionMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, entity.EntityType, threatStance).Layers[SurfaceType.TransportType.Foot].RegionMap;
		Dictionary<EntityID, float> dictionary = new Dictionary<EntityID, float>();
		List<IKnownEntityData> results = null;
		if (GetNearbyEntities(GameData.Instance.AIConstants.MaxDistanceToLookForAdditionalFood, out results, threatStance, regionMap, (EntityID foodID) => IsValidFoodItem(foodID, firstItem.EntityID, entity, bioEntity, sharedKnowledge, foodItemsGroup, out var foodData), dictionary) == GoalEvaluator.CalculateResult.Processing)
		{
			return GoalEvaluator.CalculateResult.Processing;
		}
		int num = 0;
		List<Tuple<IKnownEntityData, float>> list = new List<Tuple<IKnownEntityData, float>>();
		foreach (IKnownEntityData item2 in results)
		{
			if (dictionary.TryGetValue(item2.EntityID, out var value))
			{
				double travelScoreFromDistance = GoalEvaluator.GetTravelScoreFromDistance(entity, value);
				double num2 = EvaluateEat.ScoreCondition(item2, null);
				float item = (float)(travelScoreFromDistance * 0.6000000238418579 + num2 * 0.4000000059604645);
				list.Add(new Tuple<IKnownEntityData, float>(item2, item));
			}
		}
		// DECOMPILER ARTIFACT, not a port deviation. ILSpy renders the only declaration of
		// foodData as `out var foodData` inside the lambda passed to GetNearbyEntities above,
		// whose scope does not reach here, so decomp/ does not compile as written. The original
		// clearly declared it in this method's scope. Declared here rather than edited in
		// decomp/, which stays the untouched reference.
		IKnownEntityData foodData;
		foreach (Tuple<IKnownEntityData, float> item3 in list.OrderByDescending((Tuple<IKnownEntityData, float> s) => s.Item2))
		{
			foodData = item3.Item1;
			if (entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(foodData.Bulk + totalFoodBulk))
			{
				allFoodItems.Add(foodData);
				additionalItemsToConsume.Add(foodData.EntityID);
				totalFoodBulk += foodData.Bulk;
				SubtractNutrients(foodData, essentialNeedBulkAmounts);
				num++;
				if (!NeedsMoreFood(essentialNeedBulkAmounts) || Common.IsGreaterThan(totalFoodBulk, entity.AgentStorage.ItemStorage.TotalCapacity))
				{
					return GoalEvaluator.CalculateResult.Done;
				}
				if (num >= GameData.Instance.AIConstants.MaxAdditionalFoodItems)
				{
					break;
				}
			}
		}
		return GoalEvaluator.CalculateResult.Done;
	}

	private bool NeedsMoreFood(Dictionary<FoodNutrientType, float> essentialBulkNeeds)
	{
		return essentialBulkNeeds.Any((KeyValuePair<FoodNutrientType, float> n) => Common.IsGreaterThan(n.Value, 0f));
	}

	private void SubtractNutrients(IKnownEntityData foodData, Dictionary<FoodNutrientType, float> essentialBulkNeeds)
	{
		foreach (KeyValuePair<FoodNutrientType, float> nutrientBulkAmount in foodData.NutrientBulkAmounts)
		{
			if (essentialBulkNeeds.TryGetValue(nutrientBulkAmount.Key, out var value))
			{
				value -= nutrientBulkAmount.Value;
				value = Common.ClampBottom(value, 0f);
				essentialBulkNeeds[nutrientBulkAmount.Key] = value;
			}
		}
	}

	private bool IsValidFoodItem(EntityID foodID, EntityID firstFoodItem, Entity entity, BiologicalEntity bioEntity, SharedKnowledge sharedKnowledge, EntityGroup foodItemsGroup, out IKnownEntityData foodData)
	{
		foodData = null;
		if (foodID != firstFoodItem && EvaluateEat.IsValidFoodItem(foodID, entity, bioEntity, sharedKnowledge, foodItemsGroup, out foodData) && entity.IsOwnedByUs(foodData) && CompositeGoal.ItemIsNotAssignedToImportantJobs(foodData, sharedKnowledge))
		{
			return true;
		}
		return false;
	}

	public double ScoreGoal()
	{
		double score = 0.0;
		GoalEvaluator.CalculateResult? calculateResult = null;
		if (currentState == State.Moving)
		{
			IKnownEntityData foodData;
			if (itemToExtractFromID.HasValue)
			{
				calculateResult = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, itemToExtractFromID.Value, out foodData, ref extractionProcessToUse, ref score);
			}
			else if (itemToConsumeID.HasValue)
			{
				ProcessType extractionProcess = null;
				calculateResult = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, itemToConsumeID.Value, out foodData, ref extractionProcess, ref score);
			}
		}
		else if (currentState == State.Extracting)
		{
			calculateResult = GoalEvaluator.CalculateResult.Done;
			score = 2f * GoalEvaluator.Priority;
		}
		else if (currentState == State.Consuming)
		{
			calculateResult = GoalEvaluator.CalculateResult.Done;
			score = 2f * GoalEvaluator.Priority;
		}
		if (calculateResult == GoalEvaluator.CalculateResult.Done)
		{
			return score;
		}
		return GetCurrentGoalScore();
	}

	protected override bool ArePreconditionsOK()
	{
		if (currentState == State.Moving)
		{
			EntityID? entityID = itemToConsumeID ?? itemToExtractFromID;
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityID.Value, out var data)))
			{
				return false;
			}
			if (!GoalEvaluator.IsValidPlaysiteItem(entity, data, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
			{
				base.Status = Status.Failed;
				return false;
			}
		}
		return true;
	}

	public override string GetStatus()
	{
		if (itemIsDrunk)
		{
			return "Drinking";
		}
		return "Eating";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
			if (base.Status != Status.Completed)
			{
				return;
			}
			if (currentState == State.Moving && itemToExtractFromID.HasValue)
			{
				currentState = State.Extracting;
				if (!EntityIsNotSeenDirectly(itemToExtractFromID.Value, out var inputData))
				{
					maxAmountToExtract = entity.AgentStorage.GetFreeStomachCapacity();
					ChangeToEatingStance(extractionProcessToUse);
					GoalDoProduce goalDoProduce = new GoalDoProduce(entity, extractionProcessToUse, inputData, maxAmountToExtract, limitExtractionByNutrients: false, null, null, null, null, null);
					LookUp<SimProcess, SimProcessID>.FindByID(goalDoProduce.ProductionProcess).ProcessCompletedEvent.AddAndRegister((Action<SimProcess>)extractGoal_ProductionComplete, (IIDEventSubscriber)this, out extractionCompleteMethodID);
					AddSubgoal(goalDoProduce);
					base.Status = Status.Active;
				}
			}
			else if (currentState != State.Consuming)
			{
				currentState = State.Consuming;
				EntityID? entityID = itemToConsumeID ?? extractedItemID;
				if (!entityID.HasValue || EntityIsNotSeenDirectly(entityID.Value, out var itemToConsume))
				{
					return;
				}
				entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.Eating, out var value);
				Goal.FireEventActions(entity, null, value);
				AddConsumeGoals(itemToConsume);
				if (additionalItemsToConsume != null && additionalItemsToConsume.Count > 0)
				{
					foreach (EntityID item in additionalItemsToConsume)
					{
						if (!EntityIsNotSeenDirectly(item, out var itemToConsume2))
						{
							AddConsumeGoals(itemToConsume2);
						}
					}
				}
				base.Status = Status.Active;
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private void AddConsumeGoals(Entity itemToConsume)
	{
		ProcessType processType = entity.BiologicalEntity.ConsumeProcesses[itemToConsume.EntityType];
		ChangeToEatingStance(processType);
		AddSubgoal(new GoalDoProduce(entity, processType, itemToConsume, null, limitExtractionByNutrients: true, null, null, null, entity.EntityID, StorageCompartment.Stomach));
	}

	public static float CapAmountToConsume(Entity itemToConsume, Entity entity)
	{
		float? num = null;
		FoodNutrientType foodNutrientType;
		foreach (KeyValuePair<string, Need> needs in entity.BiologicalEntity.Needs.NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType == null)
			{
				continue;
			}
			foodNutrientType = needs.Value.NeedType.FoodNeedType.FoodNutrientType;
			if (itemToConsume.Item.Food.NutrientBulkAmounts.TryGetValue(foodNutrientType, out var value) && value > 0f)
			{
				float num2 = value - needs.Value.FoodNeed.CurrentNeededNutrientBulk;
				if (num2 <= 0f)
				{
					num = null;
					break;
				}
				float amount = itemToConsume.EntityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.FirstOrDefault((FoodNutrientAmount n) => n.Nutrient == foodNutrientType).Amount;
				float num3 = num2 / amount;
				if (!num.HasValue)
				{
					num = num3;
				}
				else if (num3 < num.Value)
				{
					num = num3;
				}
			}
		}
		float freeStomachCapacity = entity.AgentStorage.GetFreeStomachCapacity();
		if (num.HasValue)
		{
			return Math.Min(freeStomachCapacity, new float?(itemToConsume.Bulk - num.Value).Value);
		}
		return freeStomachCapacity;
	}

	private void ChangeToEatingStance(ProcessType processType)
	{
		if (entity.HasStance())
		{
			ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(processType.StanceTypes));
		}
	}

	private void extractGoal_ProductionComplete(IKnownProcess process)
	{
		if (process.OutputEntities != null && process.OutputEntities.Count > 0)
		{
			extractedItemID = process.OutputEntities[0];
			Entity entity = Entity.FindByID(extractedItemID.Value);
			if (entity != null && entity.EntityType.ItemType != null && entity.EntityType.ItemType.FoodType != null)
			{
				entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(entity.EntityID, base.entity.EntityID);
			}
		}
	}

	private bool IsInRangeOfFood(Entity food)
	{
		if (Common.DistanceOctile(entity.PlaySiteLocation, food.PlaySiteLocation) > 30f)
		{
			base.Status = Status.Failed;
			return false;
		}
		return true;
	}

	public override void Deactivate()
	{
		if (itemToConsumeID.HasValue)
		{
			ReleaseLockOnItem(itemToConsumeID.Value);
		}
		if (itemToExtractFromID.HasValue)
		{
			ReleaseLockOnItem(itemToExtractFromID.Value);
		}
		if (extractedItemID.HasValue)
		{
			ReleaseLockOnItem(extractedItemID.Value);
		}
		if (additionalItemsToConsume != null)
		{
			foreach (EntityID item in additionalItemsToConsume)
			{
				ReleaseLockOnItem(item);
			}
		}
		if (placeToEat.HasValue)
		{
			entityIntelligence.GetKnownData(placeToEat.Value, out var data);
			if (data != null && data.GatheringSite != null)
			{
				data.GatheringSite.RemoveVisitor(entity.EntityID);
			}
		}
		ConsumeStomachContents(entity);
		if (extractionCompleteMethodID.HasValue)
		{
			ActionLookup<List<EntityID>>.Remove(extractionCompleteMethodID.Value);
		}
	}

	private void ReleaseLockOnItem(EntityID itemID)
	{
		entityIntelligence.GetKnownData(itemID, out var data);
		if (data != null)
		{
			entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);
		}
	}

	public static void ConsumeStomachContents(Entity entity)
	{
		entity.AgentStorage.IterateContained(StorageCompartment.Stomach, delegate(Entity foodItem)
		{
			if (foodItem.EntityType.ItemType != null && foodItem.EntityType.ItemType.FoodType != null)
			{
				foodItem.Item.Food.ConsumeBy(entity);
				foodItem.Destroy();
			}
		});
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if (messageType == Message.MessageTypes.CancelJobOrItemInUse)
			{
				base.Status = Status.Failed;
				return true;
			}
			return false;
		}
		return true;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion((Snapshotter.Version)2u);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		itemToConsumeID = sn.DoEntityIDNullable(itemToConsumeID);
		itemToExtractFromID = sn.DoEntityIDNullable(itemToExtractFromID);
		placeToEat = sn.DoEntityIDNullable(placeToEat);
		extractedItemID = sn.DoEntityIDNullable(extractedItemID);
		currentState = sn.DoEnum(currentState);
		maxAmountToExtract = sn.DoFloatNullable(maxAmountToExtract);
		extractionProcessToUse = sn.DoGameData(extractionProcessToUse);
		itemIsDrunk = sn.DoBool(itemIsDrunk);
		extractionCompleteMethodID = sn.DoEnumNullable(extractionCompleteMethodID);
		ownerOfFoodItem = sn.DoEnumNullable(ownerOfFoodItem);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		if (version >= (Snapshotter.Version)2u)
		{
			additionalItemsToConsume = sn.DoList(additionalItemsToConsume);
		}
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		LoadPostProcessRegisterMethodIDs();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		if (extractionCompleteMethodID.HasValue)
		{
			ActionLookup<IKnownProcess>.Add(extractionCompleteMethodID.Value, extractGoal_ProductionComplete);
		}
	}
}
