using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public abstract class FoodStatistics : Rating
{
	public enum SetsOfData
	{
		StarvingAbsolute,
		StarvingPercentage,
		StarvingDeaths
	}

	public Dictionary<NeedTypeID, List<DataPoint<float>>> StarvingMemberPercentage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();

	public List<DataPoint<EntityID>> starvingDeaths = new List<DataPoint<EntityID>>();

	private Snapshotter.Version version;

	public FoodStatistics()
	{
	}

	public FoodStatistics(GroupStatistics parent)
		: base(0.2)
	{
		Parent = parent;
		StatType = StatTypes.Food;
	}

	protected void AddToStarvingDataLists(Dictionary<NeedTypeID, DataPoint<List<EntityID>>> entitiesStarvingPerNeed, int currentNoOfMembers)
	{
		foreach (KeyValuePair<NeedTypeID, DataPoint<List<EntityID>>> item in entitiesStarvingPerNeed)
		{
			float value = (float)item.Value.Value.Count / (float)currentNoOfMembers * 100f;
			Common.AddToMultiList(StarvingMemberPercentage, item.Key, new DataPoint<float>
			{
				Time = The.Sim.DateAndTime.CurrentTimeDateYear,
				Value = value
			});
		}
	}

	protected void ScoreNeedsAndDeaths(int noOfMembers, out int hungerDeaths, out Dictionary<NeedTypeID, float> starvingMembers, out float totalNeeds, out float deathsContribution)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		Food food = GameData.Instance.AIConstants.Ratings.Food;
		GatherStarving(out var currentDataPointsOut, out var currentNoOfMembers);
		AddToStarvingDataLists(currentDataPointsOut, currentNoOfMembers);
		starvingMembers = new Dictionary<NeedTypeID, float>();
		totalNeeds = 0f;
		int num = 0;
		foreach (KeyValuePair<NeedTypeID, List<DataPoint<float>>> item in StarvingMemberPercentage)
		{
			NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
			if (needType.FoodNeedType != null && needType.FoodNeedType.IsEssential)
			{
				num++;
				float value = item.Value.Last().Value;
				if (starvingMembers != null)
				{
					starvingMembers[item.Key] = value;
				}
				totalNeeds += value;
			}
		}
		totalNeeds /= num;
		totalNeeds *= food.StarvationPenaltyFactor;
		DateAndTime.TimeDateYear timeDateYear = currentTimeDateYear;
		timeDateYear.AddTime(0f - food.DaysForHungerDeathsToAffect);
		List<DataPoint<EntityID>> dataPointsBetween = Statistic.GetDataPointsBetween(starvingDeaths, timeDateYear, currentTimeDateYear);
		hungerDeaths = dataPointsBetween.Count;
		_ = timeDateYear.TotalDays;
		deathsContribution = 0f;
		deathsContribution = (float)hungerDeaths * food.HungerDeathRatingPenaltyFactor;
		if (noOfMembers > 0)
		{
			deathsContribution /= noOfMembers;
		}
		RemoveOldestData();
	}

	private void RemoveOldestData()
	{
		if (GroupStatistics.GatherStatisticsForDisplayOnly(LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID).GetAllegiance))
		{
			return;
		}
		foreach (KeyValuePair<NeedTypeID, List<DataPoint<float>>> item in StarvingMemberPercentage)
		{
			while (item.Value.Count > 1)
			{
				item.Value.RemoveAt(0);
			}
		}
	}

	protected static void CombineScores(float baseScore, float stockpileScore, float totalNeeds, float deathsContribution, out float rating, out float totalNeedsScore)
	{
		totalNeedsScore = totalNeeds / 100f;
		rating = baseScore + stockpileScore - totalNeedsScore;
		rating -= deathsContribution;
		rating = Common.Clamp(rating, 0f, 1f);
	}

	protected void ComposeRatingBreakdownForNeedsAndDeaths(Food food, int deaths, float deathsContribution, Dictionary<NeedTypeID, float> starvingMemberPercentages, float starvingScore, StringBuilder text)
	{
		if (starvingMemberPercentages != null)
		{
			bool flag = false;
			foreach (KeyValuePair<NeedTypeID, float> starvingMemberPercentage in starvingMemberPercentages)
			{
				if (!Common.IsZero(starvingMemberPercentage.Value))
				{
					if (!flag)
					{
						Common.AppendLine(text);
						Common.AppendLine(text, "STARVING");
						flag = true;
					}
					NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(starvingMemberPercentage.Key);
					AppendComponent(text, needType.ToString(), starvingMemberPercentage.Value, null, "%", indent: true, omitIfZero: true, formatAsPercentage: false, formatAsInteger: true, Common.ValueTint.Negative);
				}
			}
			if (flag)
			{
				text.Append("Subscore: -");
				Common.AppendLine(text, Common.PercentageToString(starvingScore, includePlusPrefix: false, useColoring: true, Common.ValueTint.Negative));
			}
		}
		if (deaths > 0)
		{
			Common.AppendLine(text);
			Common.AppendLine(text, "RECENT HUNGER DEATHS");
			Common.Append(text, "Number of deaths: ");
			Common.Append(text, deaths.ToString(), tintAsValue: true);
			Common.Append(text, " in last ");
			Common.AppendFormat(text, "{0:N1}", true, food.DaysForHungerDeathsToAffect);
			Common.AppendLine(text);
			text.Append("Subscore: -");
			Common.AppendLine(text, Common.PercentageToString(deathsContribution, includePlusPrefix: false, useColoring: true, Common.ValueTint.Negative));
			Common.AppendLine(text);
		}
	}

	protected void GatherStarving(out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, out int currentNoOfMembers)
	{
		Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPoints = new Dictionary<NeedTypeID, DataPoint<List<EntityID>>>();
		int noOfMembers = 0;
		DataPoint<List<EntityID>> needDataPoint;
		LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID).IterateMembers(delegate(Entity e)
		{
			if (e.EntityType.BiologicalType != null && GroupStatistics.GatherStatisticsForEntity(e) && e.Find<BiologicalEntity>(out var c))
			{
				NeedTypeID? needTypeID = null;
				noOfMembers++;
				foreach (KeyValuePair<string, Need> needs in c.Needs.NeedsList)
				{
					if (needs.Value.NeedType.FoodNeedType != null && needs.Value.NeedType.FoodNeedType.IsEssential)
					{
						needTypeID = needs.Value.NeedType.ID;
						if (!currentDataPoints.TryGetValue(needTypeID.Value, out needDataPoint))
						{
							needDataPoint = new DataPoint<List<EntityID>>
							{
								Time = The.Sim.DateAndTime.CurrentTimeDateYear,
								Value = new List<EntityID>()
							};
							currentDataPoints.Add(needTypeID.Value, needDataPoint);
						}
						if (Common.IsZero(needs.Value.CurrentLevel))
						{
							Common.AddToList(ref needDataPoint.Value, e.ID);
						}
					}
				}
			}
		});
		currentDataPointsOut = currentDataPoints;
		currentNoOfMembers = noOfMembers;
	}

	public override string ToString()
	{
		return RatingStatisticToString(GetLatestValue());
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
		starvingDeaths = sn.DoList(starvingDeaths);
		StarvingMemberPercentage = sn.DoMultiMap(StarvingMemberPercentage);
		return this;
	}
}
