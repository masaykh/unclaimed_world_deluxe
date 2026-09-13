using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class ComfortStatisticsForMembers : ComfortStatistics
{
	public Allegiance Allegiance;

	private AllegianceID snapshotAllegiance;

	private Snapshotter.Version version;

	public ComfortStatisticsForMembers()
	{
	}

	public ComfortStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
		: base(parent)
	{
		Allegiance = parentAllegiance;
	}

	protected override float ScoreRating()
	{
		float num = ((ComfortStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Comfort]).GetSharedRatings();
		float rating;
		if (IsPlaySite())
		{
			GatherHousingComfort(out var currentNoOfMembers, out var homeComfort, out var homeName);
			ScoreComfortNeeds(currentNoOfMembers, out var hasSatisifiedComfort, out var totalComfortEffects);
			ComfortStatistics.CombineScores(homeComfort, totalComfortEffects, num, out rating);
			if (composeBreakdown)
			{
				ComposeRatingBreakdown(rating, num, homeComfort, hasSatisifiedComfort, totalComfortEffects, homeName);
			}
		}
		else
		{
			rating = num;
			if (composeBreakdown)
			{
				ComposeRatingBreakdownOtherSite(rating, num);
			}
		}
		return rating;
	}

	private void ComposeRatingBreakdownOtherSite(float rating, float sharedScore)
	{
		StringBuilder stringBuilder = ComposeSharedRatingBreakdown(rating, sharedScore);
		ratingsBreakdown = stringBuilder.ToString();
	}

	private void ComposeRatingBreakdown(float rating, float sharedScore, float homeComfort, Dictionary<NeedTypeID, float> hasComfortNeedsMet, float needsRating, string homeName)
	{
		StringBuilder stringBuilder = ComposeSharedRatingBreakdown(rating, sharedScore);
		Common.AppendLine(stringBuilder);
		stringBuilder.Append("HOUSING: ");
		Common.AppendLine(stringBuilder, homeName);
		stringBuilder.Append("Subscore: +");
		Common.AppendLine(stringBuilder, Common.PercentageToString(homeComfort, includePlusPrefix: false, useColoring: true));
		ComposeRatingBreakdownForNeeds(hasComfortNeedsMet, needsRating, stringBuilder, includeMemberPercentage: false);
		ratingsBreakdown = stringBuilder.ToString();
	}

	private StringBuilder ComposeSharedRatingBreakdown(float rating, float sharedScore)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, "and their personal experience of comfort");
		AppendComponent(stringBuilder, "PERSONAL COMFORT CONDITIONS:", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		AppendComponent(stringBuilder, "COLONY COMFORT CONDITIONS", sharedScore, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		return stringBuilder;
	}

	public override void ChangeAllegiance(Allegiance newAllegiance)
	{
		Allegiance = newAllegiance;
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
