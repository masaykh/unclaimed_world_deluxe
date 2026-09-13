using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Personality : ISnapshot
{
	public Entity Parent;

	public PersonalityType PersonalityType;

	public Dictionary<RatingTypes, float> Principles;

	public float Adaptability;

	public Dictionary<AllegianceID, float> Attraction;

	public float Stability;

	private SimplexNoise stabilityNoise;

	private float happiness;

	private bool happinessIsDirty = true;

	private string happinessBreakdown;

	private RatingTypes? mostUnhappyRating;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Happiness
	{
		get
		{
			if (happinessIsDirty)
			{
				ComputeHappiness();
				happinessIsDirty = false;
				happinessBreakdown = null;
			}
			return happiness;
		}
	}

	public string HappinessBreakdown
	{
		get
		{
			if (happinessBreakdown == null)
			{
				happinessBreakdown = GetHappinessBreakdown();
			}
			return happinessBreakdown;
		}
	}

	public RatingTypes? MostUnhappyRating => mostUnhappyRating;

	public bool IsSnapshotted { get; set; }

	public Personality()
	{
	}

	public Personality(Entity entity, PersonalityType type)
	{
		PersonalityType = type;
		Parent = entity;
		stabilityNoise = new SimplexNoise();
		PersonalityType.FillEntity(this);
	}

	public void Initialize()
	{
		Parent.Intelligence.Statistics.RatingsChanged += Statistics_RatingsChanged;
	}

	public RatingTypes GetHighestUnhappiness()
	{
		_ = Principles.Count;
		float num = 10f;
		RatingTypes? ratingTypes = null;
		foreach (KeyValuePair<RatingTypes, float> principle in Principles)
		{
			float value = principle.Value;
			float rating = Parent.Intelligence.Statistics.GetRating(principle.Key);
			float num2 = ComputeHappinessComponent(value, rating);
			if (num2 < num)
			{
				num = num2;
				ratingTypes = principle.Key;
			}
		}
		return ratingTypes.Value;
	}

	public void SetPrinciplesToMinimum(RatingTypes rating, float minimum)
	{
		if (Principles[rating] < minimum)
		{
			Principles[rating] = minimum;
		}
	}

	private string GetHappinessBreakdown()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendLine(stringBuilder, "The person's satisfaction with their living situation.");
		stringBuilder.Append("Happiness: ");
		stringBuilder.Append(Common.PercentageToString(Happiness, includePlusPrefix: false, useColoring: true));
		string arg = ((!Common.IsPositive(Common.ToPercent(Happiness))) ? "Unhappy" : "Happy");
		Common.AppendLine(stringBuilder, $" ({arg})");
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		foreach (KeyValuePair<RatingTypes, float> principle in Principles)
		{
			float value = principle.Value;
			Statistic.AppendRatingsTypeToStringAndIcon(stringBuilder, principle.Key);
			Common.AppendLine(stringBuilder);
			stringBuilder.Append("   Personal conditions: ");
			float rating = Parent.Intelligence.Statistics.GetRating(principle.Key);
			stringBuilder.Append(Common.PercentageToString(rating));
			Common.AppendLine(stringBuilder);
			stringBuilder.Append("- Principles: ");
			Common.AppendLine(stringBuilder, Common.PercentageToString(value));
			stringBuilder.Append("   Difference: ");
			Common.AppendLine(stringBuilder, Common.PercentageToString(rating - value, includePlusPrefix: false, useColoring: true));
			Common.AppendLine(stringBuilder);
		}
		stringBuilder.Append("AVERAGE: ");
		Common.AppendLine(stringBuilder, Common.PercentageToString(happiness, includePlusPrefix: false, useColoring: true));
		return stringBuilder.ToString();
	}

	public bool CanComplainProperty(List<Tuple<string, bool>> effectComponents = null)
	{
		return Parent.GetEffect(AffectsFlags.CanComplain, baseValue: true, null, null, effectComponents);
	}

	public float GetCurrentStability()
	{
		float num = stabilityNoise.Generate1D((float)The.Sim.TotalUnPausedGameTimeInSeconds, GameData.Instance.AIConstants.MigrateStabilityFrequency);
		return (1f - Stability) * num;
	}

	public static float ComputeHappinessComponent(float principle, float rating)
	{
		return rating - principle;
	}

	public float ComputeHappinessComponent(RatingTypes ratingType)
	{
		float principle = Principles[ratingType];
		float rating = Parent.Intelligence.Statistics.GetRating(ratingType);
		return ComputeHappinessComponent(principle, rating);
	}

	private void ComputeHappiness()
	{
		int count = Principles.Count;
		float num = 0f;
		float num2 = 0f;
		RatingTypes? ratingTypes = null;
		foreach (KeyValuePair<RatingTypes, float> principle in Principles)
		{
			float value = principle.Value;
			float rating = Parent.Intelligence.Statistics.GetRating(principle.Key);
			float num3 = ComputeHappinessComponent(value, rating);
			if (num3 < num2)
			{
				num2 = num3;
				ratingTypes = principle.Key;
			}
			num += num3;
		}
		happiness = num / (float)count;
		mostUnhappyRating = ratingTypes;
	}

	public void Update(double? timeSinceLastUpdate)
	{
		UpdatePrinciples(timeSinceLastUpdate);
	}

	public float GetTimeForPrincipleToReachValue(RatingTypes rating, float value)
	{
		float num = Principles[rating];
		return (value - num) / (Adaptability * GameData.Instance.AIConstants.Ratings.PrinciplesAdaptationSpeedPerSecond);
	}

	private void UpdatePrinciples(double? timeSinceLastUpdate)
	{
		_ = Parent.Intelligence.Allegiance;
		foreach (RatingTypes item in Principles.Keys.ToList())
		{
			float rating = Parent.Intelligence.Statistics.GetRating(item) + GameData.Instance.AIConstants.Ratings.PrinciplesTargetDelta;
			float num = Principles[item];
			float num2 = ComputeHappinessComponent(num, rating);
			float num3 = 0f;
			if (!Common.IsZero(num2))
			{
				num3 = Adaptability * (float)((double)GameData.Instance.AIConstants.Ratings.PrinciplesAdaptationSpeedPerSecond * timeSinceLastUpdate.Value);
				num3 = Common.ClampTop(num3, Math.Abs(num2));
				if (Common.IsLessThanOrEqual(num2, 0f))
				{
					num3 *= -1f;
				}
				float f = num + num3;
				f = Common.Clamp(f, 0f, 1f);
				Principles[item] = f;
			}
		}
	}

	private void Statistics_RatingsChanged()
	{
		happinessIsDirty = true;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		PersonalityType = sn.DoGameData(PersonalityType);
		Principles = sn.DoDictionary(Principles);
		Stability = sn.DoFloat(Stability);
		stabilityNoise = (SimplexNoise)sn.DoISnapshot(stabilityNoise);
		Adaptability = sn.DoFloat(Adaptability);
		Attraction = sn.DoDictionary(Attraction);
		sn.Ignore(Parent);
		sn.Ignore(happinessIsDirty);
		sn.Ignore(happinessBreakdown);
		sn.Ignore(happiness);
		sn.Ignore(mostUnhappyRating);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		stabilityNoise.LoadPostProcess(sn);
		Parent.Intelligence.Statistics.RatingsChanged += Statistics_RatingsChanged;
	}
}
