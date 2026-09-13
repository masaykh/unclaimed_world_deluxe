using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class ComfortStatisticsForAllegiance : ComfortStatistics
{
	public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();

	private Snapshotter.Version version;

	public ComfortStatisticsForAllegiance()
	{
	}

	public ComfortStatisticsForAllegiance(GroupStatistics parent)
		: base(parent)
	{
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

	public float GetSharedRatings()
	{
		if (SharedRating.Count > 0)
		{
			return SharedRating.Last().Value;
		}
		return 0f;
	}

	protected override float ScoreRating()
	{
		GatherHousingComfort(out var currentNoOfMembers, out var homeComfort, out var _);
		ScoreComfortNeeds(currentNoOfMembers, out var hasSatisifiedComfort, out var totalComfortEffects);
		ComfortStatistics.CombineScores(homeComfort, totalComfortEffects, 0f, out var rating);
		if (composeBreakdown)
		{
			ComposeRatingBreakdown(rating, homeComfort, hasSatisifiedComfort, totalComfortEffects);
		}
		return rating;
	}

	private void ComposeRatingBreakdown(float rating, float homeComfort, Dictionary<NeedTypeID, float> hasComfortNeedsMet, float needsRating)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, "How well the colony provides comfort and luxuries:");
		AppendComponent(stringBuilder, "COLONY COMFORT CONDITIONS", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "HOUSING; AVG. COMFORT VALUE");
		stringBuilder.Append("Subscore: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(homeComfort, includePlusPrefix: false, useColoring: true));
		ComposeRatingBreakdownForNeeds(hasComfortNeedsMet, needsRating, stringBuilder);
		ratingsBreakdown = stringBuilder.ToString();
	}

	private static bool AffectsHomeRating(EntityType entityType)
	{
		if (entityType.ContainerType != null && entityType.ContainerType.ResidenceType != null && entityType.ContainerType.ResidenceType.ComfortLevel > 0f)
		{
			return true;
		}
		if (entityType.Upgrader != null && entityType.Upgrader.EffectsFinal != null)
		{
			foreach (EffectProfileType item in entityType.Upgrader.EffectsFinal)
			{
				if (item.Affects(AffectsNumbers.OfferedComfort))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool AffectsNeedsRating(Allegiance allegiance, EntityType itemEntityType)
	{
		if (allegiance.RepresentativeEntityType.BiologicalType.IsEatable(itemEntityType) && itemEntityType.ItemType.FoodType.EffectTypes != null)
		{
			foreach (EffectProfileType effectType in itemEntityType.ItemType.FoodType.EffectTypes)
			{
				if (effectType.Affects(AffectsNumbers.AgentComfort) || effectType.Affects(AffectsNumbers.OfferedComfort))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AffectsComfortRating(Allegiance allegiance, EntityType entityType)
	{
		if (!AffectsHomeRating(entityType))
		{
			return AffectsNeedsRating(allegiance, entityType);
		}
		return true;
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
