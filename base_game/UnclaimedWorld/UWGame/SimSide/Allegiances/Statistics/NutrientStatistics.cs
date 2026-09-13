using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class NutrientStatistics : ISnapshot
{
	public enum StatTypes
	{
		Produced,
		Consumed,
		Overconsumed
	}

	public Dictionary<StatTypes, Dictionary<FoodNutrientType, List<DataPoint<float>>>> Stats = new Dictionary<StatTypes, Dictionary<FoodNutrientType, List<DataPoint<float>>>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public NutrientStatistics()
	{
		if (Snapshotter.IsSnapshotting)
		{
			return;
		}
		foreach (object value in Enum.GetValues(typeof(StatTypes)))
		{
			Stats.Add((StatTypes)value, new Dictionary<FoodNutrientType, List<DataPoint<float>>>());
		}
	}

	public void AddEvent(StatTypes eventType, FoodNutrientType nutrientType, float amount)
	{
		Dictionary<FoodNutrientType, List<DataPoint<float>>> dictionary = Stats[eventType];
		if (!dictionary.TryGetValue(nutrientType, out var value))
		{
			value = new List<DataPoint<float>>();
			dictionary.Add(nutrientType, value);
		}
		float num = amount;
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		if (value.Count > 0)
		{
			DataPoint<float> dataPoint = value.Last();
			if (Common.IsEqual(dataPoint.Time.TotalDays, currentTimeDateYear.TotalDays))
			{
				num += dataPoint.Value;
				value.RemoveAt(value.Count - 1);
			}
		}
		value.Add(new DataPoint<float>(num, currentTimeDateYear));
		currentTimeDateYear.AddTime(0f - GameData.Instance.GUIConstants.TimeInDaysToKeepStatistics);
		Statistic.DiscardOldData(value, currentTimeDateYear);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Stats = sn.DoNestedMultiMap(Stats);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
