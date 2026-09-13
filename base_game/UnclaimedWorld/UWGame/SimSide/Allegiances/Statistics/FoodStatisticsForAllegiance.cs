using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics;

public class FoodStatisticsForAllegiance : FoodStatistics
{
	public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();

	private Snapshotter.Version version;

	public FoodStatisticsForAllegiance()
	{
	}

	public FoodStatisticsForAllegiance(GroupStatistics parent)
		: base(parent)
	{
	}

	public float GetSharedRatings()
	{
		if (SharedRating.Count > 0)
		{
			return SharedRating.Last().Value;
		}
		return 0f;
	}

	private void ScoreStockpiledFood(int noOfMembers, out int foodItems, out float stockpileScore)
	{
		Food food = GameData.Instance.AIConstants.Ratings.Food;
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		Allegiance allegiance = canIterateEntities.GetAllegiance;
		int usableFoodItems = 0;
		canIterateEntities.IterateOwnedItems(delegate(EntityGroup e)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> item in e.Food)
			{
				if (AffectsFoodRating(allegiance, item.Key))
				{
					foreach (EntityID item2 in item.Value)
					{
						if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(item2, out var data)) && IsAvailableFood(allegiance, data))
						{
							usableFoodItems++;
						}
					}
				}
			}
		});
		foodItems = usableFoodItems;
		stockpileScore = (float)foodItems * food.StockpiledFoodRating / (float)noOfMembers;
		stockpileScore = Common.ClampTop(stockpileScore, 1f);
	}

	public override void AddSharedRating(float ratingValue)
	{
		base.AddSharedRating(ratingValue);
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		SharedRating.Add(new DataPoint<float>
		{
			Time = currentTimeDateYear,
			Value = ratingValue
		});
	}

	public static bool AffectsFoodRating(Allegiance allegiance, EntityType item)
	{
		return allegiance.RepresentativeEntityType.BiologicalType.IsEatable(item);
	}

	private bool IsAvailableFood(Allegiance allegiance, IKnownEntityData foodData)
	{
		if (!foodData.IsCompleted())
		{
			return false;
		}
		if (!foodData.StoredPermanentlyIn.HasValue)
		{
			return true;
		}
		if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(foodData.StoredPermanentlyIn.Value.StorageEntity, out var data)))
		{
			if (data.IsTradeOfferStorage(foodData.StoredPermanentlyIn.Value.StorageID))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected override float ScoreRating()
	{
		Food food = GameData.Instance.AIConstants.Ratings.Food;
		int members = GetMembers();
		ScoreStockpiledFood(members, out var foodItems, out var stockpileScore);
		float f = stockpileScore;
		f = Common.Clamp(f, 0f, 1f);
		AddSharedRating(f);
		ScoreNeedsAndDeaths(members, out var hungerDeaths, out var starvingMembers, out var totalNeeds, out var deathsContribution);
		FoodStatistics.CombineScores(food.BaseScore, stockpileScore, totalNeeds, deathsContribution, out var rating, out var totalNeedsScore);
		if (composeBreakdown)
		{
			ComposeRatingBreakdown(food, rating, hungerDeaths, deathsContribution, members, starvingMembers, totalNeedsScore, foodItems, stockpileScore);
		}
		return rating;
	}

	private void ComposeRatingBreakdown(Food food, float rating, int deaths, float deathsContribution, int noOfMembers, Dictionary<NeedTypeID, float> starvingMemberPercentages, float starvingScore, int foodItems, float stockpileScore)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, "How well the colony provides food:");
		AppendComponent(stringBuilder, "COLONY FOOD CONDITIONS", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "BASELINE");
		stringBuilder.Append("Subscore: +");
		Common.AppendLine(stringBuilder, Common.PercentageToString(food.BaseScore, includePlusPrefix: false, useColoring: true));
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "STOCKPILED FOOD");
		Common.Append(stringBuilder, "Prepared food items ");
		Common.AppendLine(stringBuilder, Label.ToLabel(foodItems.ToString(), GameData.Instance.GUIConstants.ValueTintHex));
		stringBuilder.Append("Subscore: +");
		Common.AppendLine(stringBuilder, Common.PercentageToString(stockpileScore, includePlusPrefix: false, useColoring: true));
		ComposeRatingBreakdownForNeedsAndDeaths(food, deaths, deathsContribution, starvingMemberPercentages, starvingScore, stringBuilder);
		ratingsBreakdown = stringBuilder.ToString();
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
		SharedRating = sn.DoList(SharedRating);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
	}
}
