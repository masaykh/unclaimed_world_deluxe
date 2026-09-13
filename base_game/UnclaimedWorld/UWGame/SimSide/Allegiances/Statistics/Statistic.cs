using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.SimSide.Allegiances.Statistics;

public abstract class Statistic : ISnapshot
{
	public GroupStatistics Parent;

	public StatTypes StatType;

	private Regulator pollRegulator;

	private double snapshotPollInterval;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	protected Statistic()
	{
	}

	public Statistic(double pollInterval)
	{
		snapshotPollInterval = pollInterval;
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		pollRegulator = new Regulator(The.Sim.GameplayRandomGenerator, snapshotPollInterval, "Statistics");
	}

	public virtual void Update(GameTime gameTime)
	{
		if (pollRegulator.IsReady())
		{
			GatherPolledData();
		}
	}

	public virtual void ChangeAllegiance(Allegiance newAllegiance)
	{
	}

	protected int GetMembers()
	{
		int noOfMembers = 0;
		LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID).IterateMembers(delegate
		{
			noOfMembers++;
		});
		return noOfMembers;
	}

	public virtual void GatherPolledData()
	{
	}

	public static string RatingsTypeToString(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => "Comfort", 
			RatingTypes.Food => "Food", 
			RatingTypes.Security => "Security", 
			_ => "", 
		};
	}

	public static string RatingsTypeToDescription(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => "Represents basic needs such as shelter against the environment, prevention of disease as well as other needs like entertainment and luxury items.", 
			RatingTypes.Food => "This area covers food availability, stock size, hunger risk and variety.", 
			RatingTypes.Security => "The safety of the colony or individual against living threats, human or alien.", 
			_ => "", 
		};
	}

	public static void DiscardOldData<T>(List<DataPoint<T>> itemGroup, DateAndTime.TimeDateYear oldestDataToKeep)
	{
		if (itemGroup.Count <= 0)
		{
			return;
		}
		DataPoint<T> dataPoint = itemGroup[0];
		while (dataPoint.Time.CompareTo(oldestDataToKeep) < 0)
		{
			itemGroup.RemoveAt(0);
			if (itemGroup.Count > 0)
			{
				dataPoint = itemGroup[0];
				continue;
			}
			break;
		}
	}

	public static void AppendRatingsTypeToStringAndIcon(StringBuilder text, RatingTypes rating)
	{
		text.Append(Icon.ToIcon(RatingsTypeToIcon(rating), RatingsTypeToColor(rating)));
		text.Append(RatingsTypeToString(rating).ToUpper(Config.Culture));
	}

	public static string AppendRatingsTypeToStringAndIcon(RatingTypes rating)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendRatingsTypeToStringAndIcon(stringBuilder, rating);
		return stringBuilder.ToString();
	}

	public static string RatingsTypeToIcon(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => "lcd_icon_comfort", 
			RatingTypes.Food => "lcd_icon_nutrition", 
			RatingTypes.Security => "lcd_icon_security", 
			_ => "", 
		};
	}

	public static string RatingsTypeToColor(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => GameData.Instance.GUIConstants.ComfortColorConstant, 
			RatingTypes.Food => GameData.Instance.GUIConstants.FoodColorConstant, 
			RatingTypes.Security => GameData.Instance.GUIConstants.SecurityColorConstant, 
			_ => "", 
		};
	}

	public static string RatingsTypeToKey(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => "comfort", 
			RatingTypes.Food => "food", 
			RatingTypes.Security => "security", 
			_ => "", 
		};
	}

	protected void AppendComponent(StringBuilder text, string caption, float value, string prefix = null, string suffix = null, bool indent = false, bool omitIfZero = true, bool formatAsPercentage = false, bool formatAsInteger = false, Common.ValueTint? valueTint = null)
	{
		if (!omitIfZero || !Common.IsZero(value))
		{
			if (indent)
			{
				text.Append("   ");
			}
			text.Append(caption);
			text.Append(": ");
			if (prefix != null)
			{
				text.Append(prefix);
			}
			string value2 = (formatAsPercentage ? Common.PercentageToString(value, includePlusPrefix: false, useColoring: true, valueTint) : ((!formatAsInteger) ? FormatRatingComponent(value) : Common.ValueToIntegerString(value, useColoring: true, valueTint)));
			text.Append(value2);
			if (suffix != null)
			{
				text.Append(suffix);
			}
			Common.AppendLine(text);
		}
	}

	protected string FormatRatingComponent(float value)
	{
		return value.ToString("N2");
	}

	public static int SumDataPoints(Dictionary<EntityType, List<DataPoint<float>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
	{
		if (dict.TryGetValue(entityType, out var value))
		{
			return (int)GetDataPointsBetween(value, from, to).Sum((DataPoint<float> d) => d.Value);
		}
		return 0;
	}

	public static float GetMeanOfDataPoints(Dictionary<EntityType, List<DataPoint<float>>> dict, EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
	{
		if (dict.TryGetValue(entityType, out var value))
		{
			return GetDataPointsBetween(value, from, to).Average((DataPoint<float> d) => d.Value);
		}
		return 0f;
	}

	public static List<DataPoint<T>> GetDataPointsBetween<T>(List<DataPoint<T>> dataPoints, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, bool extendLastValue = false)
	{
		List<DataPoint<T>> list = new List<DataPoint<T>>();
		if (dataPoints.Count == 0)
		{
			return list;
		}
		GetDataPointsBetween(dataPoints, from, to, out var indexFrom, out var indexTo, extendLastValue);
		if (indexFrom >= 0 && indexTo >= 0)
		{
			for (int i = indexFrom; i <= indexTo; i++)
			{
				list.Add(dataPoints[i]);
			}
		}
		return list;
	}

	public static bool GetDataPointsBetween<T>(List<DataPoint<T>> dataPoints, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, out int indexFrom, out int indexTo, bool extendLastValue)
	{
		indexFrom = dataPoints.BinarySearch(new DataPoint<T>
		{
			Time = from
		});
		if (indexFrom < 0)
		{
			int num = ~indexFrom;
			if (num == dataPoints.Count)
			{
				if (!extendLastValue)
				{
					indexTo = -1;
					return false;
				}
				indexFrom = dataPoints.Count - 1;
			}
			else if (num == 0)
			{
				indexFrom = 0;
			}
			else
			{
				indexFrom = num;
			}
		}
		indexTo = dataPoints.BinarySearch(new DataPoint<T>
		{
			Time = to
		});
		if (indexTo < 0)
		{
			int num2 = ~indexTo;
			if (num2 == dataPoints.Count)
			{
				indexTo = dataPoints.Count - 1;
			}
			else
			{
				if (num2 == 0)
				{
					return false;
				}
				indexTo = Math.Max(0, num2 - 1);
			}
		}
		return true;
	}

	public abstract float GetLatestValue();

	public abstract float GetChange();

	protected static float GetChange(List<DataPoint<float>> ratings)
	{
		if (ratings.Count > 1)
		{
			return ratings[ratings.Count - 1].Value - ratings[ratings.Count - 2].Value;
		}
		return 0f;
	}

	public float GetAverage(List<DataPoint<float>> list, DataPoint<float> lastAveragePoint, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to)
	{
		DateAndTime.TimeDateYear timeDateYear = from;
		float num = 0f;
		float num2 = 0f;
		if (lastAveragePoint != null && lastAveragePoint.Time.CompareTo(from) >= 0)
		{
			num2 = lastAveragePoint.Value;
			timeDateYear = lastAveragePoint.Time;
			float num3 = (float)lastAveragePoint.Time.TotalDays;
			float num4 = (float)from.TotalDays;
			float num5 = (float)to.TotalDays;
			num = (num3 - num4) / (num5 - num4);
			num = Math.Min(1f, num);
		}
		List<DataPoint<float>> dataPointsBetween = GetDataPointsBetween(list, timeDateYear, to);
		float num6 = 0f;
		if (dataPointsBetween.Count > 0)
		{
			num6 = dataPointsBetween.Select((DataPoint<float> d) => d.Value).Average();
		}
		return num2 * num + num6 * (1f - num);
	}

	protected string RatingStatisticToString(double rating)
	{
		return Common.PercentageToString(rating);
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotPollInterval = sn.DoDouble(snapshotPollInterval);
		StatType = sn.DoEnum(StatType);
		sn.Ignore(Parent);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CreateRegulators();
	}
}
