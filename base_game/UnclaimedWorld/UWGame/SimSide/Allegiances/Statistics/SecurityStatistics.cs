using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public abstract class SecurityStatistics : Rating
{
	protected List<DataPoint<ViolentEvent>> violentDeaths = new List<DataPoint<ViolentEvent>>();

	protected List<DataPoint<ViolentEvent>> violentInjuries = new List<DataPoint<ViolentEvent>>();

	protected List<DataPoint<float>> violentDeathCounts = new List<DataPoint<float>>();

	protected List<DataPoint<float>> violentInjuriesCounts = new List<DataPoint<float>>();

	private Snapshotter.Version version;

	public SecurityStatistics()
	{
	}

	public SecurityStatistics(GroupStatistics parent)
		: base(0.2)
	{
		Parent = parent;
		StatType = StatTypes.Security;
	}

	public void AddViolentEvent(Entity victim, string description, ViolentEventType type)
	{
		if (GroupStatistics.GatherStatisticsForEntity(victim))
		{
			ViolentEvent violentEvent = new ViolentEvent
			{
				EventType = type,
				Victim = victim.EntityID,
				Name = victim.ToString(),
				Description = description,
				Time = The.Sim.DateAndTime.CurrentTimeDateYear
			};
			DataPoint<ViolentEvent> value = new DataPoint<ViolentEvent>
			{
				Value = violentEvent,
				Time = The.Sim.DateAndTime.CurrentTimeDateYear
			};
			if (violentEvent.EventType == ViolentEventType.Death)
			{
				Common.AddToList(ref violentDeaths, value);
			}
			else if (violentEvent.EventType == ViolentEventType.Injury)
			{
				Common.AddToList(ref violentInjuries, value);
			}
		}
	}

	public override string ToString()
	{
		return RatingStatisticToString(GetLatestValue());
	}

	protected void ComposeDeathsBreakdown(int noOfMembers, int injuries, int deaths, float injuryRating, float deathRating, StringBuilder text, Security sec)
	{
		if (injuries > 0)
		{
			Common.AppendLine(text);
			Common.AppendLine(text, "RECENT INJURIES");
			text.Append("Number of injuries: ");
			Common.Append(text, injuries.ToString(), tintAsValue: true);
			Common.Append(text, " in last ");
			Common.AppendFormat(text, "{0:N1}", true, sec.DaysForInjuriesToAffect);
			Common.Append(text, " days");
			Common.AppendLine(text);
			Common.Append(text, "Subscore: -");
			Common.AppendPercentage(text, injuryRating, useColoring: true, Common.ValueTint.Negative);
			Common.AppendLine(text);
		}
		if (deaths > 0)
		{
			Common.AppendLine(text);
			Common.AppendLine(text, "RECENT DEATHS");
			Common.Append(text, "Number of deaths: ");
			Common.Append(text, deaths.ToString(), tintAsValue: true);
			Common.Append(text, " in last ");
			Common.AppendFormat(text, "{0:N1}", true, sec.DaysForDeathsToAffect);
			Common.Append(text, " days");
			Common.AppendLine(text);
			Common.Append(text, "Subscore: -");
			Common.AppendPercentage(text, deathRating, useColoring: true, Common.ValueTint.Negative);
			Common.AppendLine(text);
		}
	}

	protected void ComputeInjuriesAndDeaths(float assets, out int relevantInjuries, out int relevantDeaths, out float totalInjuryContribution, out float totalDeathsContribution, out float finalInjuryContribution, out float finalDeathsContribution)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		Security security = GameData.Instance.AIConstants.Ratings.Security;
		DateAndTime.TimeDateYear timeDateYear = currentTimeDateYear;
		timeDateYear.AddTime(0f - security.DaysForDeathsToAffect);
		DateAndTime.TimeDateYear timeDateYear2 = currentTimeDateYear;
		timeDateYear2.AddTime(0f - security.DaysForInjuriesToAffect);
		relevantInjuries = Statistic.GetDataPointsBetween(violentInjuries, timeDateYear2, currentTimeDateYear).Count;
		relevantDeaths = Statistic.GetDataPointsBetween(violentDeaths, timeDateYear, currentTimeDateYear).Count;
		totalDeathsContribution = (float)relevantDeaths * security.DeathFactor;
		finalDeathsContribution = GetFinal(assets, totalDeathsContribution);
		finalDeathsContribution = Common.Clamp(finalDeathsContribution, 0f, 1f);
		totalInjuryContribution = (float)relevantInjuries * security.InjuryFactor;
		finalInjuryContribution = GetFinal(assets, totalInjuryContribution);
		totalInjuryContribution = Common.Clamp(totalInjuryContribution, 0f, 0.5f);
	}

	protected float GetAssets()
	{
		return GetMembers();
	}

	private float GetFinal(float assets, float total)
	{
		if (!Common.IsZero(assets))
		{
			return total / assets;
		}
		return total;
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
		violentDeathCounts = sn.DoList(violentDeathCounts);
		violentDeaths = sn.DoList(violentDeaths);
		violentInjuries = sn.DoList(violentInjuries);
		violentInjuriesCounts = sn.DoList(violentInjuriesCounts);
		return this;
	}
}
