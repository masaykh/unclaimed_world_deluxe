using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateEat : GoalEvaluator
{
	private enum FoodLevels
	{
		Full,
		AlmostFull,
		HasNotEaten,
		Starving
	}

	private enum Progress
	{
		NotStarted,
		GetFoodItems,
		ScoreFood
	}

	private const float fullLevel = 0.9f;

	private const float almostFullLevel = 0.7f;

	private EntityGroup foodItemsGroup;

	private BiologicalEntity bioEntity;

	private List<Entity> needsToBeCancelled = new List<Entity>();

	private List<Entity> itemsToBeDropped = new List<Entity>();

	private List<EntityGroup> ownersOfVehicles;

	private List<EntityGroup> ownersOfGatheringPlaces;

	private float currentStomachRoom;

	private Progress progress;

	private int currentIndex;

	private bool scoringWasInterrupted;

	private List<Tuple<IKnownEntityData, ProcessType, double>> allScores = new List<Tuple<IKnownEntityData, ProcessType, double>>();

	private EntityID? bestFoodItem;

	private EntityID? gatheringPlaceToEat;

	private ProcessType bestExtractionProcess;

	private float priority;

	private List<ItemDistance> listOfGatheringPlaces = new List<ItemDistance>();

	private List<EntityID> allFoodItems = new List<EntityID>();

	private Dictionary<EntityType, double> cachedNutritionScores = new Dictionary<EntityType, double>();

	public override float Priority => priority;

	public EvaluateEat(Entity entity, List<EntityGroup> ownersOfVehicles, List<EntityGroup> ownersOfGatheringPlaces)
		: base(entity)
	{
		bioEntity = entity.BiologicalEntity;
		this.ownersOfVehicles = ownersOfVehicles;
		this.ownersOfGatheringPlaces = ownersOfGatheringPlaces;
		foodItemsGroup = GetFoodEntityGroup(entity);
		priority = GameData.Instance.AIConstants.PriorityOfNeeds;
	}

	public static EntityGroup GetFoodEntityGroup(Entity entity)
	{
		if (entity.PersonEntity != null)
		{
			return entity.Intelligence.CurrentExpedition.OwnedEntities;
		}
		return entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		if (progress == Progress.NotStarted)
		{
			if (entityIntelligence.Brain.IsSame(typeof(GoalEat)))
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			if (GetLowestFoodLevel() == FoodLevels.Full || entity.BiologicalEntity.StomachContents > 0.94f)
			{
				result = 0.0;
				return CalculateResult.Done;
			}
			currentStomachRoom = entity.AgentStorage.GetFreeStomachCapacity();
			allScores.Clear();
			cachedNutritionScores.Clear();
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			currentIndex = 0;
			scoringWasInterrupted = false;
			progress = Progress.GetFoodItems;
		}
		if (progress == Progress.GetFoodItems)
		{
			_ = entity.PersonEntity;
			GetAllFoodItems();
			progress = Progress.ScoreFood;
		}
		if (progress == Progress.ScoreFood)
		{
			if (ScoreEating(minimumRatingToConsider, ref result) == CalculateResult.Done)
			{
				progress = Progress.NotStarted;
				bestScore = result;
				return CalculateResult.Done;
			}
			return CalculateResult.Processing;
		}
		return CalculateResult.Done;
	}

	private CalculateResult FindGatheringPlaceToEatMeal()
	{
		SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
		RegionMap regionMap = sharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap;
		listOfGatheringPlaces.Clear();
		foreach (EntityGroup ownersOfGatheringPlace in ownersOfGatheringPlaces)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> structure in ownersOfGatheringPlace.Structures)
			{
				if (GoalEvaluator.GetSortedListOfEntities(entity, ownersOfGatheringPlace, sharedKnowledge, regionMap, structure.Value, ref listOfGatheringPlaces, (IKnownEntityData e) => e.GatheringSite != null, 240f) == CalculateResult.Processing)
				{
					return CalculateResult.Processing;
				}
			}
		}
		if (listOfGatheringPlaces.Count > 0)
		{
			listOfGatheringPlaces = listOfGatheringPlaces.OrderBy((ItemDistance e) => e.Distance).ToList();
			gatheringPlaceToEat = listOfGatheringPlaces[0].Entity.EntityID;
		}
		else
		{
			gatheringPlaceToEat = null;
		}
		return CalculateResult.Done;
	}

	private CalculateResult ScoreGatheringSite(RegionMap regionMapToUse, ThreatStance threatStanceToUse, IKnownEntityData food, ref double score)
	{
		score = 0.0;
		double travelTimeScore = 0.0;
		RegionMap.Result result = GoalEvaluator.ScoreTravelTime(regionMapToUse, threatStanceToUse, entity.AccessPoint.Value, food.PlaySiteLocation, entity, ref travelTimeScore);
		if (result == RegionMap.Result.Wait)
		{
			return CalculateResult.Processing;
		}
		_ = 2;
		return CalculateResult.Done;
	}

	private FoodLevels GetLowestFoodLevel()
	{
		FoodLevels result = FoodLevels.Full;
		float num = 1f;
		foreach (KeyValuePair<string, Need> needs in bioEntity.Needs.NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null && needs.Value.CurrentLevel < num)
			{
				num = needs.Value.CurrentLevel;
			}
		}
		if (num > 0.9f)
		{
			result = FoodLevels.Full;
		}
		else if (num > 0.7f)
		{
			result = FoodLevels.AlmostFull;
		}
		else if (num > 0f)
		{
			result = FoodLevels.HasNotEaten;
		}
		else if (Common.IsZero(num))
		{
			result = FoodLevels.Starving;
		}
		return result;
	}

	private void GetAllFoodItems()
	{
		allFoodItems.Clear();
		SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
		foreach (KeyValuePair<EntityType, List<EntityID>> item in foodItemsGroup.Food)
		{
			for (int num = item.Value.Count - 1; num >= 0; num--)
			{
				if (IsValidFoodItem(item.Value[num], entity, bioEntity, sharedKnowledge, foodItemsGroup, out var itemData))
				{
					allFoodItems.Add(itemData.EntityID);
				}
			}
		}
	}

	public static bool IsValidFoodItem(EntityID foodID, Entity entity, BiologicalEntity bioEntity, SharedKnowledge sharedKnowledge, EntityGroup foodItemsGroup, out IKnownEntityData itemData)
	{
		if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, foodID, foodItemsGroup, out itemData))
		{
			return false;
		}
		if (bioEntity.IsEatable(itemData) && GoalEvaluator.IsValidPlaysiteItem(entity, itemData, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
		{
			return true;
		}
		return false;
	}

	private bool HasRoomInStomach(IKnownEntityData item)
	{
		return currentStomachRoom > item.Bulk;
	}

	private double ScoreNutrientsInExtractedItems(IKnownEntityData foodSource, ref ProcessType extractionProcess)
	{
		List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes = null;
		if (extractionProcess != null)
		{
			bioEntity.GetConsumableFoodExtractionResult(foodSource, extractionProcess, returnListOfFoodTypes: true, ref listOfFoodTypes);
		}
		else
		{
			bioEntity.GetConsumableFoodExtractionResults(foodSource, returnListOfFoodTypes: true, ref listOfFoodTypes);
		}
		double num = 0.0;
		if (listOfFoodTypes != null)
		{
			Dictionary<FoodNutrientType, float> dictionary = new Dictionary<FoodNutrientType, float>();
			ProcessType processType = null;
			foreach (Tuple<ProcessType, EntityType, float> item in listOfFoodTypes)
			{
				dictionary.Clear();
				Food.UpdateNutrientAmounts(item.Item2, item.Item3, dictionary);
				double num2 = ScoreNutrients(item.Item2, item.Item3, dictionary);
				if (num2 > num)
				{
					num = num2;
					processType = item.Item1;
				}
			}
			if (extractionProcess == null)
			{
				extractionProcess = processType;
			}
		}
		return num;
	}

	private double ScoreNutrients(EntityType entityType, float bulk, Dictionary<FoodNutrientType, float> nutrientBulkAmounts)
	{
		bool flag = false;
		if (entityType.ItemType.MaximumBulk.HasValue && Common.IsEqual(bulk, entityType.ItemType.MaximumBulk.Value) && entityType.SubstancesType == null)
		{
			flag = true;
		}
		if (flag && cachedNutritionScores.TryGetValue(entityType, out var value))
		{
			return value;
		}
		float num = 0f;
		float unsatisfiedEssentialNeedsLength = bioEntity.Needs.UnsatisfiedEssentialNeedsLength;
		float unsatisfiedNonEssentialNeedsLength = bioEntity.Needs.UnsatisfiedNonEssentialNeedsLength;
		float num2 = 0f;
		float num3 = 0f;
		foreach (KeyValuePair<string, Need> needs in bioEntity.Needs.NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null)
			{
				FoodNutrientType foodNutrientType = needs.Value.NeedType.FoodNeedType.FoodNutrientType;
				float value2 = 0f;
				nutrientBulkAmounts.TryGetValue(foodNutrientType, out value2);
				float num4 = value2 / needs.Value.FoodNeed.TotalNeededNutrientBulk;
				if (needs.Value.NeedType.FoodNeedType.IsEssential)
				{
					num2 += num4;
				}
				else
				{
					num3 += num4;
				}
			}
		}
		num = ((!(num3 > 0f)) ? ScoreNutrients(countEssentialNeeds: true, nutrientBulkAmounts, unsatisfiedEssentialNeedsLength, num2, bioEntity.Needs.NoOfEssentialFoodNeeds) : ScoreNutrients(countEssentialNeeds: false, nutrientBulkAmounts, unsatisfiedNonEssentialNeedsLength, num3, bioEntity.Needs.NoOfNonEssentialFoodNeeds));
		value = Common.ClampTop(num, 1f);
		if (flag)
		{
			cachedNutritionScores.Add(entityType, value);
		}
		return value;
	}

	private float ScoreNutrients(bool countEssentialNeeds, Dictionary<FoodNutrientType, float> nutrientBulkAmounts, float needsLength, float nutrientsLength, int noOfNeeds)
	{
		float num = 0f;
		foreach (KeyValuePair<string, Need> needs in bioEntity.Needs.NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null && ((needs.Value.NeedType.FoodNeedType.IsEssential && countEssentialNeeds) || (!needs.Value.NeedType.FoodNeedType.IsEssential && !countEssentialNeeds)))
			{
				FoodNutrientType foodNutrientType = needs.Value.NeedType.FoodNeedType.FoodNutrientType;
				if (nutrientBulkAmounts.TryGetValue(foodNutrientType, out var value) && value > 0f)
				{
					float num2 = value / needs.Value.FoodNeed.TotalNeededNutrientBulk / nutrientsLength;
					float num3 = 1f - needs.Value.CurrentLevel;
					float num4 = num3 / needsLength;
					float num5 = Math.Abs(num2 - num4);
					float num6 = Common.ClampBottom(1f - num5, 0f);
					num6 = (float)Math.Pow(num6, 2.0);
					num6 = 0.5f * num6;
					float num7 = 3f * (float)Math.Pow(num3, 2.0) * num6;
					num += num7;
				}
			}
		}
		return num / (float)noOfNeeds;
	}

	private CalculateResult GetAllGatheringPlacesSortedByDistance(Entity entity, EntityType entityType, EntityGroup owner, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float? maxDistance = null)
	{
		if (owner.Structures.TryGetValue(entityType, out var value))
		{
			return GoalEvaluator.GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap, value, ref sortedList, (IKnownEntityData itemData) => itemData.EntityType.GatheringSiteType != null && itemData.IsCompleted(), maxDistance);
		}
		return CalculateResult.Done;
	}

	public CalculateResult ScoreEating(double minimumRatingToConsider, ref double bestScore)
	{
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		double score = 0.0;
		while (currentIndex < allFoodItems.Count)
		{
			EntityID foodID = allFoodItems[currentIndex];
			ProcessType extractionProcess = null;
			IKnownEntityData foodData;
			CalculateResult calculateResult = ScoreFoodItem(regionMapAndStanceForEvaluator, threatStanceToUse, foodID, out foodData, ref extractionProcess, ref score);
			if (calculateResult == CalculateResult.Processing)
			{
				return calculateResult;
			}
			allScores.Add(new Tuple<IKnownEntityData, ProcessType, double>(foodData, extractionProcess, score));
			currentIndex++;
		}
		allScores.RemoveAll((Tuple<IKnownEntityData, ProcessType, double> c) => c.Item3 == 0.0);
		allScores.Sort((Tuple<IKnownEntityData, ProcessType, double> a, Tuple<IKnownEntityData, ProcessType, double> b) => b.Item3.CompareTo(a.Item3));
		Tuple<IKnownEntityData, ProcessType, double> tuple = null;
		Tuple<IKnownEntityData, ProcessType, double> tuple2 = null;
		for (int num = 0; num < allScores.Count; num++)
		{
			tuple2 = allScores[num];
			if (!scoringWasInterrupted || GoalEvaluator.IsValidPlaysiteItem(entity, tuple2.Item1, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
			{
				if (tuple2.Item3 < minimumRatingToConsider)
				{
					break;
				}
				if (entity.IsOwnedByUs(tuple2.Item1))
				{
					GetItemUsersToCancel(tuple2.Item1, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: true);
				}
				if (needsToBeCancelled.Count == 0 && itemsToBeDropped.Count == 0)
				{
					tuple = tuple2;
					break;
				}
				if (GoalEvaluator.IsScoreBetterThanAllInvolveds(tuple2.Item3, needsToBeCancelled))
				{
					tuple = tuple2;
					break;
				}
			}
		}
		if (tuple != null)
		{
			if (entity.PersonEntity != null && FindGatheringPlaceToEatMeal() == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
			bestFoodItem = ((tuple.Item1 != null) ? new EntityID?(tuple.Item1.EntityID) : ((EntityID?)null));
			bestScore = tuple.Item3;
			bestExtractionProcess = tuple.Item2;
		}
		else
		{
			bestFoodItem = null;
			bestScore = 0.0;
		}
		return CalculateResult.Done;
	}

	private double ScoreFreshMeal(IKnownEntityData food)
	{
		if (food.EntityType.ItemType.FoodType.IsMeal && food.Condition.Value > 0.949999988079071)
		{
			return 1.0;
		}
		return 0.0;
	}

	public static double ScoreCondition(IKnownEntityData food, double? minimumDaysLeftUntilSpoiling)
	{
		if (food.Condition.HasValue)
		{
			if (food.ConditionChangeSpeed.HasValue && !Common.IsZero(food.ConditionChangeSpeed.Value))
			{
				double daysLeftUntilBreakdown = NonLivingEntity.GetDaysLeftUntilBreakdown(food.Condition.Value, food.ConditionChangeSpeed.Value);
				double num = 6.0;
				daysLeftUntilBreakdown = Common.Clamp(daysLeftUntilBreakdown, 0.0, num);
				if (minimumDaysLeftUntilSpoiling.HasValue && daysLeftUntilBreakdown < minimumDaysLeftUntilSpoiling.Value)
				{
					return 0.0;
				}
				return Common.Clamp(Math.Pow((num - daysLeftUntilBreakdown) / num, 2.0), 0.0, 1.0);
			}
			return 0.1;
		}
		return 0.0;
	}

	public CalculateResult ScoreFoodItem(RegionMap regionMapToUse, ThreatStance? threatStance, EntityID foodID, out IKnownEntityData foodData, ref ProcessType extractionProcess, ref double score)
	{
		score = 0.0;
		foodData = null;
		if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(foodID, out foodData)))
		{
			return CalculateResult.Done;
		}
		if (regionMapToUse == null || !threatStance.HasValue)
		{
			regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out var threatStanceToUse);
			threatStance = threatStanceToUse;
		}
		double travelTimeScore = 0.0;
		switch (GoalEvaluator.ScoreTravelTime(regionMapToUse, entity, foodData, ref travelTimeScore))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			return CalculateResult.Done;
		default:
		{
			if (entity.ID == (EntityID)4951L && !foodData.EntityType.KeyName.Contains("item:smokedStreakFin"))
			{
				foodData.EntityType.KeyName.Contains("item:hardtack");
			}
			double num = ((!bioEntity.ConsumeProcesses.ContainsKey(foodData.EntityType)) ? ScoreNutrientsInExtractedItems(foodData, ref extractionProcess) : ScoreNutrients(foodData.EntityType, foodData.Bulk, foodData.NutrientBulkAmounts));
			if (Common.IsZero(num))
			{
				score = 0.0;
				return CalculateResult.Done;
			}
			double conditionScore = ScoreCondition(foodData, null);
			double num2 = ScoreCanConsumeDirectly(extractionProcess);
			if (entity.PersonEntity != null)
			{
				if (num >= (double)GameData.Instance.AIConstants.NutrientsScoreToTriggerStarvedEating)
				{
					score = ComputePeopleStarvingScore(travelTimeScore, num, conditionScore, num2);
				}
				else if (num >= (double)GameData.Instance.AIConstants.NutrientsScoreForEating)
				{
					score = ComputePeopleEatScore(travelTimeScore, num, conditionScore, num2);
				}
				else
				{
					score = 0.0;
				}
				score = ApplyPriority(score);
				return CalculateResult.Done;
			}
			if (num < 0.5)
			{
				score = 0.0;
			}
			else
			{
				score = 0.45 * travelTimeScore + 0.4 * num + 0.15 * num2;
			}
			score = ApplyPriority(score);
			return CalculateResult.Done;
		}
		}
	}

	private double ScoreCanConsumeDirectly(ProcessType extractProcess)
	{
		if (extractProcess == null)
		{
			return 1.0;
		}
		return 0.0;
	}

	private static double ComputePeopleEatScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore)
	{
		return 0.45 * travelTimeScore + 0.4 * nutrientsScore + 0.1 * conditionScore + 0.05 * consumeDirectlyScore;
	}

	private static double ComputePeopleStarvingScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore)
	{
		return 0.05 * travelTimeScore + 0.85 * nutrientsScore + 0.02 * conditionScore + 0.08 * consumeDirectlyScore;
	}

	public override bool CancelCurrentTakers()
	{
		return CancelEntities(needsToBeCancelled, itemsToBeDropped);
	}

	public override bool CanTakeGoal()
	{
		if (bestFoodItem.HasValue)
		{
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			IKnownEntityData data = null;
			if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestFoodItem.Value, out data)))
			{
				if (entity.IsOwnedByUs(data))
				{
					GetItemUsersToCancel(data, ref needsToBeCancelled, ref itemsToBeDropped, clearLists: true);
				}
				if (needsToBeCancelled != null && GoalEvaluator.IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		entity.Intelligence.SetTopLevelGoal(new GoalEat(entity, bestFoodItem.Value, gatheringPlaceToEat, GoalEvaluator.GetOwnerID(foodItemsGroup), bestExtractionProcess, GetOwnerIDs(ownersOfVehicles))
		{
			GoalEvaluator = this
		}, bestScore);
		return true;
	}
}
