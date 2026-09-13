using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class DateAndTime : ISnapshot
{
	public struct TimeDateYear : IComparable
	{
		public double TimeOfDay;

		public int Day;

		public int Year;

		public double TotalDays => TimeOfDay + (double)Day + (double)Year * 12.0;

		public double ToYearAndTimeOfYear()
		{
			return (double)Year + (double)Day / 12.0 + TimeOfDay;
		}

		public TimeDateYear(double totalDays)
		{
			TimeOfDay = 0.0;
			Day = 0;
			Year = 0;
			SetDateFromTotalDays(totalDays);
		}

		public TimeDateYear(double timeOfDay, int day, int year)
		{
			TimeOfDay = timeOfDay;
			Day = day;
			Year = year;
		}

		public void SetDateFromTotalDays(double totalDays)
		{
			double d = totalDays / 12.0;
			Year = (int)Math.Truncate(d);
			double d2 = totalDays - (double)Year * 12.0;
			Day = (int)Math.Truncate(d2);
			double timeOfDay = totalDays - Math.Floor(totalDays);
			TimeOfDay = timeOfDay;
		}

		public double ToRelativeDays()
		{
			return TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays;
		}

		public double ToRelativeSeconds()
		{
			double num = TotalDays - The.Sim.DateAndTime.StartTimeDateYear.TotalDays;
			return secondsPerDay * num;
		}

		public double ToSeconds()
		{
			double totalDays = TotalDays;
			return secondsPerDay * totalDays;
		}

		public string ToIntervalString()
		{
			string text = ((double)Day + TimeOfDay).ToString("F2");
			if (Year > 0)
			{
				return $"{Year} years, {text} days";
			}
			return $"{text} days";
		}

		public override string ToString()
		{
			string arg = TimeOfDay.ToString("F1", The.Sim.DateAndTime.numberFormat).Replace("0.", "");
			return $"DATE: {Year}.{Day}.{arg}";
		}

		public void ConvertToAbsoluteTime()
		{
			AddTime(The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays);
		}

		public void AddTime(double timeInDays)
		{
			double dateFromTotalDays = TotalDays + timeInDays;
			TimeOfDay = 0.0;
			Day = 0;
			Year = 0;
			SetDateFromTotalDays(dateFromTotalDays);
		}

		public string GetDateForJournal()
		{
			return $"{ToString()} ({GetEarthDate()})";
		}

		public string GetEarthDate()
		{
			int year = Year + GameData.Instance.Constants.StartingYear;
			double num = Common.Clamp(((double)Day + TimeOfDay) / 12.0, 0.0, 1.0);
			int month = (int)(num * 12.0);
			int num2 = DateTime.DaysInMonth(year, month);
			double num3 = num % 12.0;
			int day = Common.Clamp((int)((double)num2 * num3), 1, num2);
			return "EARTH DATE: " + new DateTime(year, month, day).ToShortDateString();
		}

		public int CompareTo(object obj)
		{
			return CompareDates(this, (TimeDateYear)obj);
		}
	}

	public TimeDateYear StartTimeDateYear;

	public double TimeOfDay = 0.4;

	public int Year = 15;

	public int Day = 1;

	public double TimeOfYear = 0.53;

	private Vector3? sunPosition = new Vector3(0f, 0f, -1f);

	public bool SunIsUp;

	public float SunAzimuth;

	public float UnshiftedAzimuth;

	public float SunElevation;

	private const float axialTilt = (float)Math.PI / 8f;

	private const float latitude = (float)Math.PI / 4f;

	private float cosLatitude;

	private float sinLatitude;

	private bool isOnNorthernHemisphere;

	private static string[] timeOfDayStrings = new string[6] { "Night", "Morning", "Noon", "Afternoon", "Evening", "Night" };

	private static float[] timesOfDay = new float[6] { 0.15f, 0.3f, 0.6f, 0.75f, 0.9f, 1f };

	private static string[] seasonStrings = new string[12]
	{
		"Late winter", "Start of spring", "Spring", "Late spring", "Start of summer", "Mid summer", "Late summer", "Start of autumn", "Mid autumn", "Late autumn",
		"Start of winter", "Mid winter"
	};

	public const float dawnSunElevation = -0.209f;

	public const float dawnSunElevationEnd = 0.104f;

	public const float sunsetElevationStart = 0.104f;

	public const float sunsetElevationEnd = -0.314f;

	public static double secondsPerDay = 1600.0;

	public const double DaysPerSeason = 3.0;

	public const double DaysPerYear = 12.0;

	private Regulator displayDateRegulator;

	public Sim.DayPhases CurrentPhase;

	private float? lightLevel;

	private NumberFormatInfo numberFormat;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public TimeDateYear CurrentTimeDateYear => new TimeDateYear(TimeOfDay, Day, Year);

	public double SecondsPerDay
	{
		get
		{
			return secondsPerDay;
		}
		set
		{
			secondsPerDay = value;
			ComputeSpeed();
		}
	}

	public double YearsPerSecond { get; private set; }

	public double DaysPerSecond { get; private set; }

	public float LightLevel
	{
		get
		{
			if (!lightLevel.HasValue)
			{
				lightLevel = GetLightLevel();
			}
			return lightLevel.Value;
		}
	}

	public Vector3 SunPosition => sunPosition.Value;

	public bool IsSnapshotted { get; set; }

	public DateAndTime()
	{
		cosLatitude = (float)Math.Cos(0.7853981852531433);
		sinLatitude = (float)Math.Sin(0.7853981852531433);
		isOnNorthernHemisphere = true;
		ComputeSpeed();
		if (!Snapshotter.IsSnapshotting)
		{
			CreateRegulators();
			CreateNumberFormat();
			UpdateAfterAdvancing();
		}
	}

	private void CreateNumberFormat()
	{
		numberFormat = new NumberFormatInfo();
		numberFormat.NumberDecimalSeparator = ".";
	}

	private void CreateRegulators()
	{
		displayDateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 4.0, "DataAndTimeDisplayData");
	}

	private void ComputeSpeed()
	{
		YearsPerSecond = 1.0 / (secondsPerDay * 4.0 * 3.0);
		DaysPerSecond = 1.0 / secondsPerDay;
	}

	public void ResetTimeOfYearAndTimeOfDay(TimeDateYear timeDateYear)
	{
		if ((double)timeDateYear.Day > 12.0)
		{
			throw new Exception("Day no. too high.");
		}
		StartTimeDateYear = timeDateYear;
		TimeOfDay = timeDateYear.TimeOfDay;
		TimeOfYear = (double)timeDateYear.Day / 12.0;
		Year = timeDateYear.Year;
		Day = timeDateYear.Day;
		ComputeSunAndLight();
	}

	public DateTime GetTime()
	{
		float num = 1f / 24f;
		double num2 = TimeOfDay % (double)num;
		return new DateTime(1, 1, 1, Common.Clamp((int)(24.0 * TimeOfDay), 0, 23), Common.Clamp((int)(num2 * 60.0), 0, 59), 0);
	}

	public void AdvanceTime(double seconds)
	{
		AddTime(seconds * DaysPerSecond);
		UpdateAfterAdvancing();
	}

	public void Update(GameTime gameTime)
	{
		AddTime(gameTime.ElapsedGameTime.TotalSeconds * DaysPerSecond);
		UpdateAfterAdvancing();
	}

	private void UpdateAfterAdvancing()
	{
		if (TimeOfDay >= 0.0 && TimeOfDay < 0.23)
		{
			CurrentPhase = Sim.DayPhases.Sleep;
		}
		else if (TimeOfDay >= 0.23 && TimeOfDay < 0.615)
		{
			CurrentPhase = Sim.DayPhases.Work;
		}
		else if (TimeOfDay >= 0.615 && TimeOfDay < 0.923)
		{
			CurrentPhase = Sim.DayPhases.Leisure;
		}
		else
		{
			CurrentPhase = Sim.DayPhases.Sleep;
		}
		UpdateDisplayDate();
		ComputeSunAndLight();
	}

	private void ComputeSunAndLight()
	{
		float num = (float)((TimeOfDay - 0.5) * 6.2831854820251465);
		float num2 = (float)Math.Cos(num);
		float num3 = (float)(0.39269909262657166 * (-1.0 + 2.0 * TimeOfYear));
		float num4 = (float)Math.Cos(num3);
		double num5 = (double)(num2 * num4 * cosLatitude) + Math.Sin(num3) * (double)sinLatitude;
		SunElevation = (float)Math.Asin(num5);
		bool sunIsUp = SunIsUp;
		SunIsUp = SunElevation > 0f && SunElevation < (float)Math.PI;
		// UNHIDDEN MOD: the "O" shadow toggle. Forcing the sun down is what suppresses shadows,
		// and it is applied before the change is compared below so the tree night states stay
		// consistent with it.
		if (UWGame.Mods.UnhiddenMod.Enabled && UWGame.Mods.UnhiddenMod.ShadowsDisabled)
		{
			SunIsUp = false;
		}
		if (SunIsUp != sunIsUp)
		{
			SetTreeStateChanges(!SunIsUp, StateModifier.Night);
		}
		if (SunIsUp)
		{
			double num6 = (0.0 - Math.Sin(num)) * (double)num4 / Math.Cos(SunElevation);
			if (num6 < -1.0 || num6 > 1.0)
			{
				num6 = Common.Clamp(num6, -1.0, 1.0);
			}
			SunAzimuth = (float)Math.Asin(num6);
			UnshiftedAzimuth = SunAzimuth;
			if (isOnNorthernHemisphere)
			{
				SunAzimuth = (float)Math.PI - SunAzimuth;
			}
			sunPosition = new Vector3((float)num6, 0f - (float)Math.Cos(SunAzimuth), 0f - (float)num5);
			sunPosition = Vector3.Normalize(sunPosition.Value);
		}
		lightLevel = GetLightLevel();
	}

	private void SetTreeStateChanges(bool value, StateModifier modifier)
	{
		Site playSite = The.Sim.PlaySite;
		if (playSite == null)
		{
			return;
		}
		for (int i = 0; i < playSite.Entities.Count; i++)
		{
			Entity entity = playSite.Entities[i];
			if (entity.EntityType.TreeType != null)
			{
				entity.Renderable.SetOrClearSpriteStateFlag(value, modifier);
			}
		}
	}

	public void UpdateDisplayDate()
	{
		if (displayDateRegulator.IsReady() && The.InGameUI != null)
		{
			string timeOfDayAsString = GetTimeOfDayAsString(TimeOfDay);
			string season = seasonStrings[(int)(TimeOfYear * (double)seasonStrings.Length)];
			The.InGameUI.SetTimeAndDate(timeOfDayAsString, season, CurrentTimeDateYear.ToString());
		}
	}

	public static string GetTimeOfDayAsString(double timeOfDay)
	{
		int num = 0;
		num = Common.GetStairStepIndex((float)timeOfDay, timesOfDay);
		return timeOfDayStrings[num];
	}

	public float GetLightLevel()
	{
		if (TimeOfDay < 0.5)
		{
			return 1f - MathHelper.Clamp(SunElevation / -0.209f, 0f, 1f);
		}
		return 1f - MathHelper.Clamp(SunElevation / -0.314f, 0f, 1f);
	}

	public float GetProgress(float elevationPoint)
	{
		return MathHelper.Distance(SunElevation, elevationPoint) / Math.Abs(elevationPoint);
	}

	private void AddTime(double timeInDays)
	{
		TimeOfDay += timeInDays;
		double num = timeInDays / DaysPerSecond * YearsPerSecond;
		TimeOfYear += num;
		if (TimeOfYear > 1.0)
		{
			TimeOfYear -= 1.0;
			Year++;
			Day = 1;
		}
		if (TimeOfDay > 1.0)
		{
			TimeOfDay -= 1.0;
			Day++;
		}
	}

	public static bool DateIsAfter(TimeDateYear date, TimeDateYear compareToDate)
	{
		if (date.Year > compareToDate.Year)
		{
			return true;
		}
		if (date.Year == compareToDate.Year)
		{
			if (date.Day > compareToDate.Day)
			{
				return true;
			}
			if (date.Day == compareToDate.Day)
			{
				return date.TimeOfDay > compareToDate.TimeOfDay;
			}
			return false;
		}
		return false;
	}

	public static double GetTimeDifferenceInDays(TimeDateYear from, TimeDateYear to)
	{
		double totalDays = to.TotalDays;
		double totalDays2 = from.TotalDays;
		return totalDays - totalDays2;
	}

	public static int CompareDates(TimeDateYear date1, TimeDateYear date2)
	{
		if (date1.Year > date2.Year)
		{
			return 1;
		}
		if (date1.Year == date2.Year)
		{
			if (date1.Day > date2.Day)
			{
				return 1;
			}
			if (date1.Day == date2.Day)
			{
				if (date1.TimeOfDay > date2.TimeOfDay)
				{
					return 1;
				}
				if (date1.TimeOfDay == date2.TimeOfDay)
				{
					return 0;
				}
				return -1;
			}
			return -1;
		}
		return -1;
	}

	public static TimeDateYear GetSecondsToIngameDays(float seconds)
	{
		float num = (float)The.Sim.DateAndTime.DaysPerSecond * seconds;
		return new TimeDateYear(num);
	}

	public static bool DateIsAfter(double timeOfDay, int date, double compareToTimeOfDay, int compareToDate)
	{
		if (date > compareToDate)
		{
			return true;
		}
		if (date == compareToDate)
		{
			return timeOfDay > compareToTimeOfDay;
		}
		return false;
	}

	public double ConvertDateToRealTimeSeconds(TimeDateYear date)
	{
		return (date.TimeOfDay + (double)date.Day) * SecondsPerDay + (double)date.Year / YearsPerSecond;
	}

	public double MillisecondsToDays(double milliSecondsSinceLastReady)
	{
		return milliSecondsSinceLastReady / 1000.0 * DaysPerSecond;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		TimeOfDay = sn.DoDouble(TimeOfDay);
		Day = sn.DoInt32(Day);
		Year = sn.DoInt32(Year);
		TimeOfYear = sn.DoDouble(TimeOfYear);
		secondsPerDay = sn.DoDouble(secondsPerDay);
		CurrentPhase = sn.DoEnum(CurrentPhase);
		StartTimeDateYear = sn.DoTimeDateYear(StartTimeDateYear);
		sn.Ignore(cosLatitude);
		sn.Ignore(CurrentPhase);
		sn.Ignore(sunPosition);
		sn.Ignore(SunIsUp);
		sn.Ignore(SunAzimuth);
		sn.Ignore(SunElevation);
		sn.Ignore(sinLatitude);
		sn.Ignore(cosLatitude);
		sn.Ignore(isOnNorthernHemisphere);
		sn.Ignore(YearsPerSecond);
		sn.Ignore(DaysPerSecond);
		sn.Ignore(timeOfDayStrings);
		sn.Ignore(timesOfDay);
		sn.Ignore(seasonStrings);
		sn.Ignore(lightLevel);
		sn.Ignore(numberFormat);
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
		ComputeSpeed();
		ComputeSunAndLight();
		CreateRegulators();
		CreateNumberFormat();
	}
}
