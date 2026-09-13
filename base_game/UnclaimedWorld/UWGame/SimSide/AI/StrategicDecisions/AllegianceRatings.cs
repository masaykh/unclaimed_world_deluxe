using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.StrategicDecisions;

public class AllegianceRatings : ISnapshot, IScore, IEdge
{
	public AllegianceID AllegianceID;

	public float TotalStatScore;

	public float Desirability;

	public float Personal;

	public float Attraction;

	public Dictionary<RatingTypes, Pair<float, float>> Conditions = new Dictionary<RatingTypes, Pair<float, float>>();

	public Dictionary<RatingTypes, Pair<float, float>> ResultComponents = new Dictionary<RatingTypes, Pair<float, float>>();

	private string breakdown;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Score { get; set; }

	public float Edge { get; set; }

	public string Breakdown
	{
		get
		{
			if (breakdown == null)
			{
				breakdown = GetBreakdown();
			}
			return breakdown;
		}
	}

	public bool IsSnapshotted { get; set; }

	public void SetConditions(RatingTypes stat, float ownCondition, float otherCondition)
	{
		if (!Conditions.TryGetValue(stat, out var value))
		{
			value = new Pair<float, float>();
			Conditions.Add(stat, value);
		}
		value.First = ownCondition;
		value.Second = otherCondition;
		breakdown = null;
	}

	public void SetResults(RatingTypes stat, float diff, float score)
	{
		if (!ResultComponents.TryGetValue(stat, out var value))
		{
			value = new Pair<float, float>();
			ResultComponents.Add(stat, value);
		}
		value.First = diff;
		value.Second = score;
		breakdown = null;
	}

	private string GetBreakdown()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ATTRACTION: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(Desirability, includePlusPrefix: false, useColoring: true));
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "COMPARISON OF COLONY CONDITIONS: ");
		foreach (KeyValuePair<RatingTypes, Pair<float, float>> resultComponent in ResultComponents)
		{
			Pair<float, float> pair = Conditions[resultComponent.Key];
			stringBuilder.Append("   ");
			Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, resultComponent.Key);
			stringBuilder.Append(": ");
			stringBuilder.Append(Common.PercentageToString(pair.Second));
			stringBuilder.Append(" - ");
			stringBuilder.Append(Common.PercentageToString(pair.First));
			stringBuilder.Append(" = ");
			Common.AppendLine(stringBuilder, Common.PercentageToString(resultComponent.Value.First, includePlusPrefix: false, useColoring: true));
		}
		Common.AppendLine(stringBuilder);
		stringBuilder.Append("WEIGHTED TOTAL: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(TotalStatScore, includePlusPrefix: true, useColoring: true));
		Common.AppendLine(stringBuilder);
		stringBuilder.Append("PERSONAL CIRCUMSTANCES: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(Personal, includePlusPrefix: true, useColoring: true));
		Common.AppendLine(stringBuilder);
		stringBuilder.Append("PERSONAL INTEREST: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(Attraction, includePlusPrefix: true, useColoring: true));
		return stringBuilder.ToString();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ResultComponents = sn.DoDictionary(ResultComponents);
		Conditions = sn.DoDictionary(Conditions);
		Desirability = sn.DoFloat(Desirability);
		TotalStatScore = sn.DoFloat(TotalStatScore);
		Personal = sn.DoFloat(Personal);
		Score = sn.DoFloat(Score);
		Edge = sn.DoFloat(Edge);
		AllegianceID = sn.DoEnum(AllegianceID);
		Attraction = sn.DoFloat(Attraction);
		sn.Ignore(breakdown);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
