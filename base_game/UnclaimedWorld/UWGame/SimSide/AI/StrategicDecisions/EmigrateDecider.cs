using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.StrategicDecisions;

public class EmigrateDecider : StrategyDecider, ISnapshot
{
	private Entity parent;

	private EntityID snapshotParent;

	private Regulator regulator;

	private Allegiance ownAllegiance;

	private bool isWaitingToEmigrate;

	private Dictionary<AllegianceID, AllegianceRatings> CurrentRatingsOfOtherAllegiances;

	public float MigrationRisk;

	public AllegianceID? PreferredMigrationTarget;

	public static int[] EmigrateRollFrequency;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public EmigrateDecider(Entity entity)
	{
		parent = entity;
		ownAllegiance = parent.Intelligence.Allegiance;
		CreateRegulators();
	}

	public EmigrateDecider()
	{
	}

	public double? GetUpdateInterval()
	{
		if (IsFirstTimeComputingRatings())
		{
			return 0.0;
		}
		return GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds;
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds, "EmigrateDecider");
	}

	public override void Update(GameTime gameTime)
	{
		if (IsFirstTimeComputingRatings() || regulator.IsReady())
		{
			ScoreDesireToEmigrate();
			RollForChanceToEmigrate();
		}
	}

	public AllegianceRatings GetRatings(AllegianceID allegiance)
	{
		AllegianceRatings value = null;
		if (CurrentRatingsOfOtherAllegiances != null)
		{
			CurrentRatingsOfOtherAllegiances.TryGetValue(allegiance, out value);
		}
		return value;
	}

	public string GetMigrateRiskTooltip()
	{
		string text = null;
		text = "Daily emigration risk: " + Common.PercentageToString(MigrationRisk);
		if (PreferredMigrationTarget.HasValue)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(PreferredMigrationTarget);
			if (allegiance != null)
			{
				AllegianceRatings ratingsForAllegiance = parent.Intelligence.GetRatingsForAllegiance(allegiance.ID);
				text = text + " \nPreferred migration target:  " + allegiance.Site.Name;
				text = text + " \n" + ratingsForAllegiance.Breakdown;
			}
		}
		return text;
	}

	public bool CanEmigrateProperty(List<Tuple<string, bool>> effectComponents = null)
	{
		return parent.GetEffect(AffectsFlags.CanEmigrate, baseValue: true, null, null, effectComponents);
	}

	public string GetCanEmigrateToTargetTooltip(Allegiance target)
	{
		bool typeCanEmigrate = true;
		bool isOnlyMember = false;
		bool recentlyJoined = false;
		bool isOverTargetPopCap = true;
		List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
		bool num = CanEmigrateToTarget(compileTooltip: true, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, list, target);
		StringBuilder stringBuilder = new StringBuilder();
		if (num)
		{
			Common.AppendLine(stringBuilder, "Can emigrate.");
		}
		else
		{
			Common.AppendLine(stringBuilder, "Cannot emigrate.");
		}
		Common.AppendLine(stringBuilder);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "A character can only leave if all of the below are " + Common.BoolToString(value: true, useColor: true) + ":");
		Common.AppendLine(stringBuilder);
		if (!typeCanEmigrate)
		{
			stringBuilder.Append("Character type can emigrate: ");
			Common.AppendLine(stringBuilder, Common.BoolToString(typeCanEmigrate, useColor: true));
		}
		if (isOverTargetPopCap)
		{
			stringBuilder.Append("The colony has room for more: ");
			Common.AppendLine(stringBuilder, Common.BoolToString(!isOverTargetPopCap, useColor: true));
		}
		stringBuilder.Append("Our colony has more than one member: ");
		Common.AppendLine(stringBuilder, Common.BoolToString(!isOnlyMember, useColor: true));
		stringBuilder.Append("Character joined some time ago: ");
		Common.AppendLine(stringBuilder, Common.BoolToString(!recentlyJoined, useColor: true));
		foreach (Tuple<string, bool> item in list)
		{
			stringBuilder.Append(item.Item1 + ": ");
			Common.AppendLine(stringBuilder, Common.BoolToString(item.Item2, useColor: true));
		}
		return stringBuilder.ToString();
	}

	public bool CanEmigrateToAnyTarget()
	{
		bool typeCanEmigrate = true;
		bool isOnlyMember = false;
		bool recentlyJoined = false;
		bool isOverTargetPopCap = true;
		return CanEmigrateToTarget(compileTooltip: false, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, null, null);
	}

	public bool CanEmigrateToTarget(Allegiance allegiance)
	{
		bool typeCanEmigrate = true;
		bool isOnlyMember = false;
		bool recentlyJoined = false;
		bool isOverTargetPopCap = true;
		return CanEmigrateToTarget(compileTooltip: false, ref typeCanEmigrate, ref isOnlyMember, ref recentlyJoined, ref isOverTargetPopCap, null, allegiance);
	}

	public bool CanEmigrateToTarget(bool compileTooltip, ref bool typeCanEmigrate, ref bool isOnlyMember, ref bool recentlyJoined, ref bool isOverTargetPopCap, List<Tuple<string, bool>> effectComponents, Allegiance targetAllegiance)
	{
		bool result = true;
		typeCanEmigrate = true;
		isOverTargetPopCap = false;
		if (!parent.Intelligence.CanEmigrate())
		{
			if (!compileTooltip)
			{
				return false;
			}
			typeCanEmigrate = false;
			result = false;
		}
		if (parent.Intelligence.Allegiance.Members.Count == 1 && parent.IsOnPlaySite())
		{
			if (!compileTooltip)
			{
				return false;
			}
			isOnlyMember = true;
			result = false;
		}
		if (parent.Intelligence.Memory.RecentlyJoinedExpedition())
		{
			if (!compileTooltip)
			{
				return false;
			}
			recentlyJoined = true;
			result = false;
		}
		if (!CanEmigrateProperty(effectComponents))
		{
			if (!compileTooltip)
			{
				return false;
			}
			result = false;
		}
		if (targetAllegiance != null && !targetAllegiance.IsWithinPopulationCap(1))
		{
			if (!compileTooltip)
			{
				return false;
			}
			isOverTargetPopCap = true;
			result = false;
		}
		return result;
	}

	private void ScoreDesireToEmigrate()
	{
		if (CurrentRatingsOfOtherAllegiances == null)
		{
			CurrentRatingsOfOtherAllegiances = new Dictionary<AllegianceID, AllegianceRatings>();
		}
		if (!CanEmigrateToAnyTarget())
		{
			PreferredMigrationTarget = null;
			MigrationRisk = 0f;
			return;
		}
		Personality personality = parent.PersonEntity.Personality;
		personality.Principles.TryGetValue(RatingTypes.Security, out var _);
		personality.Principles.TryGetValue(RatingTypes.Comfort, out var _);
		personality.Principles.TryGetValue(RatingTypes.Food, out var _);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float rating = parent.Intelligence.Statistics.GetRating(RatingTypes.Food);
		float rating2 = parent.Intelligence.Statistics.GetRating(RatingTypes.Security);
		float rating3 = parent.Intelligence.Statistics.GetRating(RatingTypes.Comfort);
		parent.Name.Contains("Darzi");
		if (parent.PersonEntity != null)
		{
			num = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Food], rating);
			num2 = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Security], rating2);
			num3 = Personality.ComputeHappinessComponent(personality.Principles[RatingTypes.Comfort], rating3);
		}
		float currentStability = personality.GetCurrentStability();
		Migration migration = GameData.Instance.AIConstants.Migration;
		List<AllegianceID> list = null;
		foreach (KeyValuePair<string, Site> allSite in The.Sim.World.AllSites)
		{
			foreach (Allegiance allegiance in allSite.Value.Allegiances)
			{
				if (allegiance != ownAllegiance && allegiance.Statistics != null && IsCompatible(allegiance))
				{
					Common.AddToList(ref list, allegiance.ID);
					if (!CurrentRatingsOfOtherAllegiances.TryGetValue(allegiance.ID, out var value4))
					{
						value4 = new AllegianceRatings();
						value4.AllegianceID = allegiance.ID;
						CurrentRatingsOfOtherAllegiances.Add(allegiance.ID, value4);
					}
					float rating4 = allegiance.Statistics.GetRating(RatingTypes.Food);
					float rating5 = allegiance.Statistics.GetRating(RatingTypes.Security);
					float rating6 = allegiance.Statistics.GetRating(RatingTypes.Comfort);
					float num4 = rating4 - rating;
					float num5 = rating5 - rating2;
					float num6 = rating6 - rating3;
					float num7 = num4 - num;
					float num8 = num6 - num3;
					float num9 = num5 - num2;
					value4.SetConditions(RatingTypes.Food, rating, rating4);
					value4.SetConditions(RatingTypes.Security, rating2, rating5);
					value4.SetConditions(RatingTypes.Comfort, rating3, rating6);
					value4.SetResults(RatingTypes.Food, num4, num7);
					value4.SetResults(RatingTypes.Security, num5, num9);
					value4.SetResults(RatingTypes.Comfort, num6, num8);
					float num10 = (num7 + num9 + num8) / 3f;
					num10 = (value4.TotalStatScore = (1f - migration.WeightOfPersonalTotal) * num10);
					float value5 = 0f;
					if (personality.Attraction != null)
					{
						personality.Attraction.TryGetValue(allegiance.ID, out value5);
					}
					float num11 = migration.WeightOfPersonalRandom * currentStability;
					num11 = (value4.Personal = migration.WeightOfPersonalTotal * num11);
					value4.Attraction = value5;
					value4.Desirability = num10 + num11 + value5;
				}
			}
		}
		foreach (AllegianceID item in CurrentRatingsOfOtherAllegiances.Keys.ToList())
		{
			if (list == null || !list.Contains(item))
			{
				CurrentRatingsOfOtherAllegiances.Remove(item);
			}
		}
		PreferredMigrationTarget = null;
		MigrationRisk = 0f;
		if (CurrentRatingsOfOtherAllegiances.Count > 0)
		{
			KeyValuePair<AllegianceID, AllegianceRatings> keyValuePair = (from i in CurrentRatingsOfOtherAllegiances
				where PermitsMigration(i.Key)
				orderby i.Value.Desirability descending
				select i).FirstOrDefault();
			if (keyValuePair.Value != null)
			{
				PreferredMigrationTarget = keyValuePair.Key;
				MigrationRisk = ConvertDesirabilityToMigrationChance(keyValuePair.Value.Desirability);
			}
		}
	}

	private bool IsFirstTimeComputingRatings()
	{
		if (CurrentRatingsOfOtherAllegiances != null)
		{
			return false;
		}
		return true;
	}

	private float ConvertDesirabilityToMigrationChance(float desirability)
	{
		if (desirability > 0f)
		{
			float maximumMigrateRisk = GameData.Instance.AIConstants.Migration.MaximumMigrateRisk;
			float minimumMigrateRisk = GameData.Instance.AIConstants.Migration.MinimumMigrateRisk;
			float desirabilityGivingMaximumMigrateRisk = GameData.Instance.AIConstants.Migration.DesirabilityGivingMaximumMigrateRisk;
			return MathHelper.Lerp(minimumMigrateRisk, maximumMigrateRisk, Math.Min(desirability, desirabilityGivingMaximumMigrateRisk));
		}
		return 0f;
	}

	private float ComputeIntervalChanceFromDailyChance(float dailyChance)
	{
		double num = DateAndTime.secondsPerDay / GameData.Instance.AIConstants.EmigrateDeciderUpdateIntervalInSeconds;
		return (float)((double)dailyChance / num);
	}

	private bool IsCompatible(Allegiance allegiance)
	{
		return allegiance.RepresentativeEntityType == parent.Intelligence.Allegiance.RepresentativeEntityType;
	}

	private bool PermitsMigration(AllegianceID allegianceID)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);
		if (allegiance != null && allegiance.PermitsImmigration)
		{
			return true;
		}
		return false;
	}

	private void RollForChanceToEmigrate()
	{
		if (!CanEmigrateToAnyTarget() || parent.Intelligence.Memory.EmigrateTarget.HasValue || !PreferredMigrationTarget.HasValue || CurrentRatingsOfOtherAllegiances.Count <= 0)
		{
			return;
		}
		_ = CurrentRatingsOfOtherAllegiances[PreferredMigrationTarget.Value];
		List<AllegianceRatings> list = new List<AllegianceRatings>();
		foreach (KeyValuePair<AllegianceID, AllegianceRatings> currentRatingsOfOtherAllegiance in CurrentRatingsOfOtherAllegiances)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(currentRatingsOfOtherAllegiance.Key);
			if (allegiance != null && allegiance.PermitsImmigration && allegiance.AllegianceType != AllegianceType.Player)
			{
				list.Add(currentRatingsOfOtherAllegiance.Value);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		AllegianceRatings allegianceRatings = list.OrderByDescending((AllegianceRatings i) => i.Desirability).FirstOrDefault();
		float num = ConvertDesirabilityToMigrationChance(allegianceRatings.Desirability);
		if (!Common.IsGreaterThan(num, 0f))
		{
			return;
		}
		float num2 = ComputeIntervalChanceFromDailyChance(num);
		if (The.Sim.GameplayRandomGenerator.NextDouble("emigrateDecider") <= (double)num2)
		{
			Common.BuildEdgesFromBucketSizes(list, doSort: false, out var totalScore);
			int stairstep;
			AllegianceRatings stairStepIndex = Common.GetStairStepIndex(list, out stairstep, The.Sim.GameplayRandomGenerator, totalScore);
			if (parent.IsOnPlaySite())
			{
				parent.Intelligence.Memory.SetEmigrateDecision(stairStepIndex.AllegianceID, parent);
			}
		}
	}

	private void Emigrate()
	{
		ownAllegiance.OtherSiteAllegianceManager.AddEmigrantToQueue(parent.EntityID);
		isWaitingToEmigrate = true;
	}

	public bool HasDesireToEmigrate(Allegiance toAllegiance)
	{
		AllegianceRatings ratings = GetRatings(toAllegiance.ID);
		if (ratings != null && ratings.Desirability > 0f)
		{
			return true;
		}
		return false;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Entity, EntityID>(parent).Value;
		isWaitingToEmigrate = sn.DoBool(isWaitingToEmigrate);
		CurrentRatingsOfOtherAllegiances = sn.DoDictionary(CurrentRatingsOfOtherAllegiances);
		PreferredMigrationTarget = sn.DoEnumNullable(PreferredMigrationTarget);
		MigrationRisk = sn.DoFloat(MigrationRisk);
		sn.Ignore(ownAllegiance);
		sn.Ignore(parent);
		sn.Ignore(EmigrateRollFrequency);
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
		parent = Entity.FindByID(snapshotParent);
		ownAllegiance = parent.Intelligence.Allegiance;
		CreateRegulators();
	}
}
