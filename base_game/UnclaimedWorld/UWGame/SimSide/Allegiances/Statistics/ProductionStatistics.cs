using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.Steam;

namespace UWGame.SimSide.Allegiances.Statistics;

public class ProductionStatistics : ISnapshot
{
	public enum StatTypes
	{
		Produced,
		ConsumedFood,
		UsedAsInput,
		Degraded,
		EatenByCreatures,
		Disappeared
	}

	public Dictionary<StatTypes, Dictionary<EntityType, List<DataPoint<float>>>> Stats = new Dictionary<StatTypes, Dictionary<EntityType, List<DataPoint<float>>>>();

	public Dictionary<StatTypes, Dictionary<EntityType, int>> Totals = new Dictionary<StatTypes, Dictionary<EntityType, int>>();

	public Dictionary<EntityType, List<DataPoint<Productivity>>> ProductivityStats = new Dictionary<EntityType, List<DataPoint<Productivity>>>();

	public HashSet<EntityType> TypesWithStats = new HashSet<EntityType>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ProductionStatistics()
	{
		if (Snapshotter.IsSnapshotting)
		{
			return;
		}
		foreach (object value in Enum.GetValues(typeof(StatTypes)))
		{
			Stats.Add((StatTypes)value, new Dictionary<EntityType, List<DataPoint<float>>>());
			Totals.Add((StatTypes)value, new Dictionary<EntityType, int>());
		}
	}

	public static bool CountMemberConsumption(Entity entity)
	{
		return entity.Intelligence.IsIndependent();
	}

	private bool AddProductivityTimelineEvent(EntityType entityType, Productivity amount)
	{
		if (!ProductivityStats.TryGetValue(entityType, out var value))
		{
			value = new List<DataPoint<Productivity>>();
			ProductivityStats.Add(entityType, value);
			TypesWithStats.Add(entityType);
		}
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		if (value.Count > 0)
		{
			DataPoint<Productivity> dataPoint = value.Last();
			if (Common.IsEqual(dataPoint.Time.TotalDays, currentTimeDateYear.TotalDays))
			{
				amount.Merge(dataPoint.Value);
				value.RemoveAt(value.Count - 1);
			}
		}
		value.Add(new DataPoint<Productivity>(amount, currentTimeDateYear));
		currentTimeDateYear.AddTime(0f - GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
		Statistic.DiscardOldData(value, currentTimeDateYear);
		return true;
	}

	private bool AddTimelineEvent(StatTypes eventType, EntityType entityType, float amount)
	{
		if (Stats.TryGetValue(eventType, out var value))
		{
			if (!value.TryGetValue(entityType, out var value2))
			{
				value2 = new List<DataPoint<float>>();
				value.Add(entityType, value2);
				TypesWithStats.Add(entityType);
			}
			float num = amount;
			DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
			if (value2.Count > 0)
			{
				DataPoint<float> dataPoint = value2.Last();
				if (Common.IsEqual(dataPoint.Time.TotalDays, currentTimeDateYear.TotalDays))
				{
					num += dataPoint.Value;
					value2.RemoveAt(value2.Count - 1);
				}
			}
			value2.Add(new DataPoint<float>(num, currentTimeDateYear));
			currentTimeDateYear.AddTime(0f - GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
			Statistic.DiscardOldData(value2, currentTimeDateYear);
			return true;
		}
		return false;
	}

	public static void GetMeanOfDataPoints(Dictionary<EntityType, List<DataPoint<Productivity>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, out float? totalProductivity, out float? toolProductivity, out float? skillProductivity, out float? energyProductivity)
	{
		if (dict.TryGetValue(entityType, out var value))
		{
			List<DataPoint<Productivity>> dataPointsBetween = Statistic.GetDataPointsBetween(value, from, to);
			if (dataPointsBetween.Count > 0)
			{
				totalProductivity = dataPointsBetween.Average((DataPoint<Productivity> d) => d.Value.TotalProductivity);
				skillProductivity = dataPointsBetween.Average((DataPoint<Productivity> d) => d.Value.SkillProductivity);
				toolProductivity = dataPointsBetween.Average((DataPoint<Productivity> d) => d.Value.ToolProductivity);
				energyProductivity = dataPointsBetween.Average((DataPoint<Productivity> d) => d.Value.EnergyProductivity);
				return;
			}
		}
		totalProductivity = null;
		toolProductivity = null;
		skillProductivity = null;
		energyProductivity = null;
	}

	public void AddProductivityEvent(EntityType entityType, IKnownProcess processData)
	{
		AddProductivityTimelineEvent(entityType, processData.Productivity);
	}

	public void AddEvent(StatTypes eventType, EntityType entityType, int amount)
	{
		if (AddTimelineEvent(eventType, entityType, amount))
		{
			Common.AddToDictWithSums(Totals[eventType], entityType, amount, out var sum);
			CheckAchievements(eventType, entityType, sum);
		}
	}

	private void CheckAchievements(StatTypes eventType, EntityType entityType, int sum)
	{
		StartScenarioParams startScenarioParams = The.Sim.StartGameParams.StartScenarioParams;
		if (startScenarioParams == null || startScenarioParams.Scenario.Source != Source.RefactoredGames)
		{
			return;
		}
		_ = startScenarioParams.ScenarioName;
		if (eventType != StatTypes.Produced || The.Sim.StartGameParams.GetRGScenario() != StartGameParams.RGScenario.FieldsOfTauCeti)
		{
			return;
		}
		if (!The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.hideProducer) && (The.Sim.GetDifficultyKey() == "hard" || The.Sim.GetDifficultyKey() == "normal") && (entityType.KeyName == "item:whipjawTannedHide" || entityType.KeyName == "item:megapodTannedHide" || entityType.KeyName == "item:thunderChickenTannedHide"))
		{
			Dictionary<EntityType, int> dictionary = Totals[eventType];
			if (dictionary.TryGetValue(GameData.Instance.AllEntityTypes["item:whipjawTannedHide"], out var value) && value >= 40 && dictionary.TryGetValue(GameData.Instance.AllEntityTypes["item:megapodTannedHide"], out var value2) && value2 >= 20 && dictionary.TryGetValue(GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"], out var value3) && value3 >= 50)
			{
				The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.hideProducer);
			}
		}
		if (!The.Sim.Controller.StatsAndAchievements.IsAchievementUnlocked(AchievementID.gunsProducer) && (The.Sim.GetDifficultyKey() == "hard" || The.Sim.GetDifficultyKey() == "normal") && (entityType.KeyName == "item:gunpowderRifle" || entityType.KeyName == "item:blackPowderRifleAmmo"))
		{
			Dictionary<EntityType, int> dictionary2 = Totals[eventType];
			if (dictionary2.TryGetValue(GameData.Instance.AllEntityTypes["item:gunpowderRifle"], out var value4) && value4 >= 10 && dictionary2.TryGetValue(GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"], out var value5) && value5 >= 20)
			{
				The.Sim.Controller.StatsAndAchievements.UnlockAchievement(AchievementID.gunsProducer);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Stats = sn.DoNestedMultiMap(Stats);
		TypesWithStats = sn.DoHashSet(TypesWithStats);
		Totals = sn.DoNestedDictionary(Totals);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
