using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class SecurityStatisticsForMembers : SecurityStatistics
{
	public Allegiance Allegiance;

	private AllegianceID snapshotAllegiance;

	private Snapshotter.Version version;

	public SecurityStatisticsForMembers()
	{
	}

	public SecurityStatisticsForMembers(GroupStatistics parent, Allegiance parentAllegiance)
		: base(parent)
	{
		Allegiance = parentAllegiance;
	}

	public override void ChangeAllegiance(Allegiance newAllegiance)
	{
		Allegiance = newAllegiance;
	}

	protected override float ScoreRating()
	{
		int members = GetMembers();
		float assets = GetAssets();
		ComputeInjuriesAndDeaths(assets, out var relevantInjuries, out var relevantDeaths, out var totalInjuryContribution, out var totalDeathsContribution, out var finalInjuryContribution, out var finalDeathsContribution);
		float sharedRatings = ((SecurityStatisticsForAllegiance)Allegiance.Statistics.Ratings[RatingTypes.Security]).GetSharedRatings();
		float f = sharedRatings - finalInjuryContribution - finalDeathsContribution;
		f = Common.Clamp(f, 0f, 1f);
		if (composeBreakdown)
		{
			ComposeRatingBreakdown(f, members, relevantInjuries, relevantDeaths, totalInjuryContribution, totalDeathsContribution, finalInjuryContribution, finalDeathsContribution, sharedRatings);
		}
		return f;
	}

	private void ComposeRatingBreakdown(float rating, int noOfMembers, int injuries, int deaths, float totalInjuryContribution, float totalDeathsContribution, float finalInjuryContribution, float finalDeathsContribution, float sharedSecurity)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Security security = GameData.Instance.AIConstants.Ratings.Security;
		Common.AppendLine(stringBuilder, "and their personal experience of security conditions");
		AppendComponent(stringBuilder, "PERSONAL SECURITY CONDITIONS:", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		AppendComponent(stringBuilder, "COLONY SECURITY CONDITIONS", sharedSecurity, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		ComposeDeathsBreakdown(noOfMembers, injuries, deaths, finalInjuryContribution, finalDeathsContribution, stringBuilder, security);
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
