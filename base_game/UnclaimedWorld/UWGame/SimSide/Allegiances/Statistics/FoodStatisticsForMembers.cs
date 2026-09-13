using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class FoodStatisticsForMembers : FoodStatistics
{
	public Allegiance Allegiance;

	private AllegianceID snapshotAllegiance;

	private Snapshotter.Version version;

	public FoodStatisticsForMembers()
	{
	}

	public FoodStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
		: base(parent)
	{
		Allegiance = parentAllegiance;
	}

	protected override float ScoreRating()
	{
		Food food = GameData.Instance.AIConstants.Ratings.Food;
		int members = GetMembers();
		ScoreNeedsAndDeaths(members, out var hungerDeaths, out var starvingMembers, out var totalNeeds, out var deathsContribution);
		float num = ((FoodStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Food]).GetSharedRatings();
		FoodStatistics.CombineScores(food.BaseScore, num, totalNeeds, deathsContribution, out var rating, out var totalNeedsScore);
		if (composeBreakdown)
		{
			ComposeRatingBreakdown(food, rating, hungerDeaths, deathsContribution, members, starvingMembers, totalNeedsScore, num);
		}
		return rating;
	}

	public override void ChangeAllegiance(Allegiance newAllegiance)
	{
		Allegiance = newAllegiance;
	}

	private void ComposeRatingBreakdown(Food food, float rating, int deaths, float deathsContribution, int noOfMembers, Dictionary<NeedTypeID, float> starvingMemberPercentages, float starvingScore, float sharedScore)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, "and their personal experience of food conditions");
		AppendComponent(stringBuilder, "PERSONAL FOOD CONDITIONS:", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "BASELINE");
		stringBuilder.Append("Subscore: +");
		Common.AppendLine(stringBuilder, Common.PercentageToString(food.BaseScore, includePlusPrefix: false, useColoring: true));
		Common.AppendLine(stringBuilder);
		AppendComponent(stringBuilder, "COLONY FOOD CONDITIONS", sharedScore, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
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
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(Allegiance).Value;
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
	}
}
