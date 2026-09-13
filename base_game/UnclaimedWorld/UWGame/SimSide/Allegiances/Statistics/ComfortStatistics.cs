using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public abstract class ComfortStatistics : Rating
{
	public Dictionary<NeedTypeID, List<DataPoint<float>>> HasSatisifedComfortNeedMemberPercentage = new Dictionary<NeedTypeID, List<DataPoint<float>>>();

	private Snapshotter.Version version;

	public ComfortStatistics()
	{
	}

	public ComfortStatistics(GroupStatistics parent)
		: base(0.2)
	{
		Parent = parent;
		StatType = StatTypes.Comfort;
	}

	public override string ToString()
	{
		return RatingStatisticToString(GetLatestValue());
	}

	protected void ScoreComfortNeeds(int noOfMembers, out Dictionary<NeedTypeID, float> hasSatisifiedComfort, out float totalComfortEffects)
	{
		_ = The.Sim.DateAndTime.CurrentTimeDateYear;
		_ = GameData.Instance.AIConstants.Ratings.Comfort;
		GatherComfortNeeds(out var currentDataPointsOut, out var currentNoOfMembers);
		AddToHasSatisifedComfortNeedDataLists(currentDataPointsOut, currentNoOfMembers);
		totalComfortEffects = 0f;
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		float tempTotal = 0f;
		int members = 0;
		canIterateEntities.IterateMembers(delegate(Entity e)
		{
			if (GroupStatistics.GatherStatisticsForEntity(e))
			{
				tempTotal += e.Intelligence.Comfort;
				members++;
			}
		});
		totalComfortEffects = tempTotal;
		if (members > 0)
		{
			totalComfortEffects /= members;
		}
		hasSatisifiedComfort = new Dictionary<NeedTypeID, float>();
		foreach (KeyValuePair<NeedTypeID, List<DataPoint<float>>> item in HasSatisifedComfortNeedMemberPercentage)
		{
			LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
			float value = item.Value.Last().Value;
			if (hasSatisifiedComfort != null)
			{
				float value2 = 0.01f * value;
				hasSatisifiedComfort[item.Key] = value2;
			}
		}
	}

	protected void ComposeRatingBreakdownForNeeds(Dictionary<NeedTypeID, float> hasComfortNeedsMet, float needsScore, StringBuilder text, bool includeMemberPercentage = true)
	{
		if (hasComfortNeedsMet == null)
		{
			return;
		}
		bool flag = false;
		foreach (KeyValuePair<NeedTypeID, float> item in hasComfortNeedsMet)
		{
			if (!flag)
			{
				Common.AppendLine(text);
				Common.AppendLine(text, "COMFORT NEEDS MET");
				flag = true;
			}
			NeedType needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);
			text.Append(needType.ToString());
			text.Append(": ");
			if (includeMemberPercentage)
			{
				text.Append("Members ");
				text.Append(Common.PercentageToString(item.Value));
			}
		}
		if (flag)
		{
			Common.AppendLine(text);
			text.Append("Subscore: +");
			Common.AppendLine(text, Common.PercentageToString(needsScore, includePlusPrefix: false, useColoring: true));
		}
	}

	protected static void CombineScores(float homeComfort, float totalNeeds, float sharedRating, out float rating)
	{
		rating = homeComfort + totalNeeds + sharedRating;
		rating = Common.Clamp(rating, 0f, 1f);
	}

	protected void GatherComfortNeeds(out Dictionary<NeedTypeID, DataPoint<List<EntityID>>> currentDataPointsOut, out int currentNoOfMembers)
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
					if (needs.Value.NeedType.GivesComfortEffects())
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
						if (needs.Value.CurrentLevel > 0f)
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

	private void AddToHasSatisifedComfortNeedDataLists(Dictionary<NeedTypeID, DataPoint<List<EntityID>>> hasSatisfiedComfortPerNeed, int currentNoOfMembers)
	{
		foreach (KeyValuePair<NeedTypeID, DataPoint<List<EntityID>>> item in hasSatisfiedComfortPerNeed)
		{
			float value = (float)item.Value.Value.Count / (float)currentNoOfMembers * 100f;
			Common.AddToMultiList(HasSatisifedComfortNeedMemberPercentage, item.Key, new DataPoint<float>
			{
				Time = The.Sim.DateAndTime.CurrentTimeDateYear,
				Value = value
			});
		}
	}

	protected void GatherHousingComfort(out int currentNoOfMembers, out float homeComfort, out string homeName)
	{
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		Allegiance allegiance = canIterateEntities.GetAllegiance;
		DataPoint<float> dataPoint = new DataPoint<float>();
		int noOfMembers = 0;
		homeName = null;
		string tempHomeName = null;
		float totalHousingComfort = 0f;
		canIterateEntities.IterateMembers(delegate(Entity e)
		{
			if (GroupStatistics.GatherStatisticsForEntity(e))
			{
				if (e.EntityType.Person != null)
				{
					Person personEntity = e.PersonEntity;
					float num;
					if (personEntity.Household != null && personEntity.Household.Home.HasValue)
					{
						if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(personEntity.Household.Home.Value, out var data)))
						{
							tempHomeName = data.EntityType.Name;
							num = data.ComfortLevel.Value;
						}
						else
						{
							personEntity.Household.Home = null;
							num = 0f;
						}
					}
					else
					{
						num = 0f;
					}
					totalHousingComfort += num;
					noOfMembers++;
				}
				else if (e.EntityType.BiologicalType != null)
				{
					totalHousingComfort += 1f;
					noOfMembers++;
				}
			}
		});
		homeName = tempHomeName ?? "Open air";
		dataPoint.Time = The.Sim.DateAndTime.CurrentTimeDateYear;
		homeComfort = totalHousingComfort;
		if (noOfMembers > 0)
		{
			homeComfort /= noOfMembers;
			dataPoint.Value = homeComfort;
		}
		else
		{
			dataPoint.Value = 0f;
		}
		currentNoOfMembers = noOfMembers;
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
		HasSatisifedComfortNeedMemberPercentage = sn.DoMultiMap(HasSatisifedComfortNeedMemberPercentage);
		return this;
	}
}
