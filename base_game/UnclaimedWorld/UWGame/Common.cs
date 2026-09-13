using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame;

public static class Common
{
	public enum ValueTint
	{
		Positive,
		Negative,
		Neutral
	}

	public enum Direction
	{
		North,
		East,
		South,
		West,
		NorthEast,
		SouthEast,
		SouthWest,
		NorthWest
	}

	public const double epsilon = 1E-07;

	public const float floatEpsilon = 0.0001f;

	public const decimal decimalEpsilon = 0.00001m;

	private const char indentCharacter = ' ';

	public const string indentString = "   ";

	private static int indentLength = "   ".Length;

	private const float locationEpsilon = 0.1f;

	private const float directionEpsilon = 0.001f;

	private const string thousandsPostfix = "k";

	public static U GetExistingEntryOrAddNew<T, U>(Dictionary<T, U> dictionary, T key) where U : new()
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = new U();
			dictionary.Add(key, value);
		}
		return value;
	}

	public static void AppendDivider(StringBuilder text)
	{
		AppendLine(text, "---------------");
	}

	public static void AppendDividerOnOwnLine(StringBuilder text)
	{
		AppendLine(text);
		AppendDivider(text);
	}

	public static void AppendHeader(StringBuilder text, string t)
	{
		text.Append(Label.ToLabel(t, "#COLORHEADER"));
		text.Append(" \n");
	}

	public static void AppendPossibleActionText(StringBuilder text, string t)
	{
		text.Append(Label.ToLabel(t, "#COLORPOSITIVE"));
	}

	public static void AppendImpossibleActionText(StringBuilder text, string t)
	{
		text.Append(Label.ToLabel(t, "#COLORNEGATIVE"));
	}

	public static string ComposeHeadingAndBlobText(string heading, string blob, bool lightBackground = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (lightBackground)
		{
			AppendHeaderOnLightBG(stringBuilder, heading);
		}
		else
		{
			AppendHeader(stringBuilder, heading);
		}
		Append(stringBuilder, blob);
		return stringBuilder.ToString();
	}

	public static void AppendHeaderOnLightBG(StringBuilder text, string t)
	{
		text.Append(Label.ToLabel(t, "#COLORHEADERDARK"));
		text.Append(" \n");
	}

	public static void Append(StringBuilder text, string t, bool tintAsValue = false)
	{
		if (tintAsValue)
		{
			text.Append(Label.ToLabel(t, GameData.Instance.GUIConstants.ValueTintHex));
		}
		else
		{
			text.Append(t);
		}
	}

	public static void Append(StringBuilder text, string t, ValueTint tintValue)
	{
		text.Append(Label.ToLabel(t, GetTint(tintValue)));
	}

	public static void AppendPercentage(StringBuilder text, double percentage, bool useColoring, ValueTint? valueTint)
	{
		text.Append(PercentageToString(percentage, includePlusPrefix: false, useColoring, valueTint));
	}

	public static void AppendFormat(StringBuilder text, string t, bool tintAsValue, params object[] args)
	{
		if (tintAsValue)
		{
			text.Append(Label.ToLabel(string.Format(t, args), GameData.Instance.GUIConstants.ValueTintHex));
		}
		else
		{
			text.AppendFormat(t, args);
		}
	}

	public static void AppendIndentedLine(StringBuilder text, string line)
	{
		text.Append("   ");
		if (line != null)
		{
			text.Append(line);
		}
		text.Append(" \n");
	}

	public static void AppendLine(StringBuilder text, string line = null)
	{
		if (line != null)
		{
			text.Append(line);
		}
		text.Append(" \n");
	}

	public static void AddToList<T>(ref List<T> list, T value)
	{
		if (list == null)
		{
			list = new List<T>();
		}
		list.Add(value);
	}

	public static void AddToList<T>(ref List<T> list, List<T> value)
	{
		if (list == null)
		{
			list = new List<T>();
		}
		list.AddRange(value);
	}

	public static void AddToList<T>(ref HashSet<T> list, T value)
	{
		if (list == null)
		{
			list = new HashSet<T>();
		}
		list.Add(value);
	}

	public static void AddRangeToList<T>(ref List<T> list, List<T> listToAdd)
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (listToAdd != null)
		{
			list.AddRange(listToAdd);
		}
	}

	public static void AddToSet<T>(ref HashSet<T> set, T value)
	{
		if (set == null)
		{
			set = new HashSet<T>();
		}
		set.Add(value);
	}

	public static void AddRangeToSet<T>(ref HashSet<T> set, List<T> listToAdd)
	{
		if (set == null)
		{
			set = new HashSet<T>();
		}
		foreach (T item in listToAdd)
		{
			set.Add(item);
		}
	}

	public static bool MultiListContains<T, U>(Dictionary<T, List<U>> dictionary, T typeKey, U valueKey)
	{
		if (typeKey != null)
		{
			if (dictionary.TryGetValue(typeKey, out var value))
			{
				return value.Contains(valueKey);
			}
		}
		else
		{
			foreach (KeyValuePair<T, List<U>> item in dictionary)
			{
				if (item.Value.Contains(valueKey))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AddToDictionary<T, U>(ref Dictionary<T, U> list, T key, U value)
	{
		if (list == null)
		{
			list = new Dictionary<T, U>();
		}
		if (!list.ContainsKey(key))
		{
			list.Add(key, value);
			return true;
		}
		return false;
	}

	public static void AddOrUpdateDictionary<T, U>(ref Dictionary<T, U> list, T key, U value)
	{
		if (list == null)
		{
			list = new Dictionary<T, U>();
		}
		if (!list.ContainsKey(key))
		{
			list.Add(key, value);
		}
		else
		{
			list[key] = value;
		}
	}

	public static int AddToMultiList<T, U>(Dictionary<T, List<U>> dictionary, T key, U value)
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new List<U>();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
		return value2.Count;
	}

	public static int AddToMultiList<T, U>(ref Dictionary<T, List<U>> dictionary, T key, U value)
	{
		if (dictionary == null)
		{
			dictionary = new Dictionary<T, List<U>>();
		}
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new List<U>();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
		return value2.Count;
	}

	internal static void AddToDictWithSums<T>(Dictionary<T, float> dictWithSums, T key, float amount)
	{
		if (!dictWithSums.TryGetValue(key, out var value))
		{
			value = amount;
			dictWithSums.Add(key, value);
		}
		else
		{
			value += amount;
			dictWithSums[key] = value;
		}
	}

	internal static void AddToDictWithSums<T>(Dictionary<T, int> dictWithSums, T key, int amount, out int sum)
	{
		if (!dictWithSums.TryGetValue(key, out sum))
		{
			sum = amount;
			dictWithSums.Add(key, sum);
		}
		else
		{
			sum += amount;
			dictWithSums[key] = sum;
		}
	}

	internal static void AddToDictWithSums<T>(Dictionary<T, int> dictWithSums, T key, bool addKeyIfNotExisting = true)
	{
		if (!dictWithSums.TryGetValue(key, out var value))
		{
			if (addKeyIfNotExisting)
			{
				value = 1;
				dictWithSums.Add(key, value);
			}
		}
		else
		{
			value++;
			dictWithSums[key] = value;
		}
	}

	internal static void RemoveFromDictWithSums<T>(Dictionary<T, int> dictWithSums, T key)
	{
		if (dictWithSums.TryGetValue(key, out var value))
		{
			value--;
			if (value <= 0)
			{
				dictWithSums.Remove(key);
			}
			else
			{
				dictWithSums[key] = value;
			}
		}
	}

	public static void AddToNestedDictionary<T, U, V>(Dictionary<T, Dictionary<U, V>> dictionary, T key1, U key2, V value)
	{
		if (!dictionary.TryGetValue(key1, out var value2))
		{
			value2 = new Dictionary<U, V>();
			dictionary.Add(key1, value2);
		}
		value2[key2] = value;
	}

	public static void AddToMultiList<T, U>(Dictionary<T, HashSet<U>> dictionary, T key, HashSet<U> value)
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new HashSet<U>();
			dictionary.Add(key, value2);
		}
		value2.UnionWith(value);
	}

	public static void AddToMultiList<T, U>(Dictionary<T, HashSet<U>> dictionary, T key, U value)
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = new HashSet<U>();
			dictionary.Add(key, value2);
		}
		value2.Add(value);
	}

	public static bool RemoveFromMultiList<T, U>(Dictionary<T, List<U>> typeList, T key, U value, bool removeEmptyList = false)
	{
		if (typeList != null)
		{
			if (key == null)
			{
				T key2 = default(T);
				bool result = false;
				bool flag = false;
				foreach (KeyValuePair<T, List<U>> type in typeList)
				{
					if (type.Value.Remove(value))
					{
						if (removeEmptyList && type.Value.Count == 0)
						{
							key2 = type.Key;
							flag = true;
						}
						result = true;
						break;
					}
				}
				if (flag)
				{
					typeList.Remove(key2);
				}
				return result;
			}
			if (typeList.TryGetValue(key, out var value2))
			{
				bool result2 = value2.Remove(value);
				if (removeEmptyList && value2.Count == 0)
				{
					typeList.Remove(key);
				}
				return result2;
			}
		}
		return false;
	}

	public static bool RemoveFromMultiSet<T, U>(Dictionary<T, HashSet<U>> typeList, T key, U value, bool removeEmptyList = false)
	{
		if (typeList != null)
		{
			if (key == null)
			{
				T key2 = default(T);
				bool result = false;
				bool flag = false;
				foreach (KeyValuePair<T, HashSet<U>> type in typeList)
				{
					if (type.Value.Remove(value))
					{
						if (removeEmptyList && type.Value.Count == 0)
						{
							key2 = type.Key;
							flag = true;
						}
						result = true;
						break;
					}
				}
				if (flag)
				{
					typeList.Remove(key2);
				}
				return result;
			}
			if (typeList.TryGetValue(key, out var value2))
			{
				bool result2 = value2.Remove(value);
				if (removeEmptyList && value2.Count == 0)
				{
					typeList.Remove(key);
				}
				return result2;
			}
		}
		return false;
	}

	public static bool RemoveFromNestedDictionary<T, U, V>(Dictionary<T, Dictionary<U, V>> typeList, T key, U key2, bool removeEmptyList = false)
	{
		if (typeList != null && key != null && typeList.TryGetValue(key, out var value))
		{
			bool result = value.Remove(key2);
			if (removeEmptyList && value.Count == 0)
			{
				typeList.Remove(key);
			}
			return result;
		}
		return false;
	}

	public static float GetMinimumValue(float mean, float deviation)
	{
		return mean - 3f * deviation;
	}

	public static float VectorToAngle(Vector2 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public static float VectorToAngle(Vector3 vector)
	{
		return (float)Math.Atan2(vector.Y, vector.X);
	}

	public static double GetAngleBetweenVectors(Vector2 a, Vector2 b)
	{
		float num = Vector2.Dot(a, b);
		float num2 = a.Length() * b.Length();
		return Math.Acos(Clamp(num / num2, 0f, 1f));
	}

	public static Vector2 AngleToVector(float angleInRadians)
	{
		return new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));
	}

	public static int GetStairStepIndex(float number, float[] stairSteps)
	{
		int num = stairSteps.Length - 1;
		int num2 = num;
		int num3 = 0;
		while (num2 == num && num3 < num)
		{
			if (number < stairSteps[num3])
			{
				num2 = num3;
			}
			num3++;
		}
		return num2;
	}

	public static T GetStairStepIndexComputeLastEdge<T>(IList<T> stairSteps, out int stairstep, RandomGenerator randomGenerator) where T : IEdge
	{
		float edge = stairSteps[stairSteps.Count - 1].Edge;
		return GetStairStepIndex(stairSteps, out stairstep, randomGenerator, edge);
	}

	public static T GetStairStepIndex<T>(IList<T> stairSteps, out int stairstep, RandomGenerator randomGenerator, float maxEdge = 1f) where T : IEdge
	{
		bool saveMessage = ((randomGenerator == The.Sim.GameplayRandomGenerator) ? true : false);
		return GetStairStepIndex(maxEdge * (float)randomGenerator.NextDouble("Common - GetStairStepIndex", saveMessage), stairSteps, out stairstep);
	}

	public static T GetStairStepIndex<T>(float number, IList<T> stairSteps, out int stairstep) where T : IEdge
	{
		int num = (stairstep = stairSteps.Count - 1);
		int num2 = 0;
		while (stairstep == num && num2 < num)
		{
			if (number <= stairSteps[num2].Edge)
			{
				stairstep = num2;
			}
			num2++;
		}
		return stairSteps[stairstep];
	}

	public static void BuildEdgesFromBucketSizes<T>(List<T> listOfBuckets, bool doSort, out float totalScore) where T : IScore, IEdge
	{
		totalScore = 0f;
		foreach (T listOfBucket in listOfBuckets)
		{
			totalScore += listOfBucket.Score;
		}
		if (doSort)
		{
			listOfBuckets.Sort((T a, T b) => b.Score.CompareTo(a.Score));
		}
		float num = 0f;
		for (int num2 = 0; num2 < listOfBuckets.Count; num2++)
		{
			T value = listOfBuckets[num2];
			float edge = value.Score + num;
			value.Edge = edge;
			listOfBuckets[num2] = value;
			num = value.Edge;
		}
	}

	public static float EaseInValueTowardsTarget(float currentValue, float targetValue, float lerpFactor)
	{
		currentValue = currentValue * (1f - lerpFactor) + targetValue * lerpFactor;
		return currentValue;
	}

	public static Matrix EaseInValueTowardsTarget(Matrix currentValue, Matrix targetValue, float lerpFactor)
	{
		currentValue = currentValue * (1f - lerpFactor) + targetValue * lerpFactor;
		return currentValue;
	}

	public static Vector2 ToVector2(this Vector3 location)
	{
		return new Vector2(location.X, location.Y);
	}

	public static Vector3 ToVector3(this Vector2 location)
	{
		return new Vector3(location.X, location.Y, 0f);
	}

	public static Point ToPoint(this Vector2 location)
	{
		return new Point((int)location.X, (int)location.Y);
	}

	public static Point ToPoint(this Vector3 location)
	{
		return new Point((int)location.X, (int)location.Y);
	}

	public static Vector2 ToVector2(this Point location)
	{
		return new Vector2(location.X, location.Y);
	}

	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
	{
		return new HashSet<T>(source);
	}

	public static string Truncate(this string value, int maxLength)
	{
		if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
		{
			return value.Substring(0, maxLength);
		}
		return value;
	}

	public static T GetMinimum<T>(List<T> list, Func<T, float> function)
	{
		float num = float.MaxValue;
		T result = default(T);
		foreach (T item in list)
		{
			float num2 = function(item);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static int ToPercent(double rating)
	{
		return (int)(Math.Round(rating, 2) * 100.0);
	}

	public static string ValueToIntegerString(float value, bool useColoring, ValueTint? valueTint)
	{
		string valueAsString = ((int)value).ToString();
		return ApplyColorLabel(useColoring: true, valueTint, (int)value, valueAsString);
	}

	public static string ValueToDecimalString(float value, bool useColoring, ValueTint? valueTint)
	{
		string text = DecimalToString(value);
		if (useColoring)
		{
			text = ApplyColorLabel(useColoring: true, valueTint, value, text);
		}
		return text;
	}

	public static string PercentageToString(double rating, bool includePlusPrefix = false, bool useColoring = false, ValueTint? valueTint = null)
	{
		int num = ToPercent(rating);
		string arg = "";
		if (includePlusPrefix && num >= 0)
		{
			arg = "+";
		}
		string valueAsString = string.Format("{1}{0}%", num, arg);
		return ApplyColorLabel(useColoring, valueTint, num, valueAsString);
	}

	private static string ApplyColorLabel(bool useColoring, ValueTint? valueTint, int valueAsNumber, string valueAsString)
	{
		if (useColoring)
		{
			string color = ((!valueTint.HasValue) ? GetTint(valueAsNumber) : GetTint(valueTint.Value));
			return Label.ToLabel(valueAsString, color);
		}
		return valueAsString;
	}

	private static string ApplyColorLabel(bool useColoring, ValueTint? valueTint, float valueAsNumber, string valueAsString)
	{
		if (useColoring)
		{
			string color = ((!valueTint.HasValue) ? GetTint(valueAsNumber) : GetTint(valueTint.Value));
			return Label.ToLabel(valueAsString, color);
		}
		return valueAsString;
	}

	public static bool IsPositive(int percent)
	{
		return percent >= 0;
	}

	public static string GetTint(ValueTint valueTint)
	{
		return valueTint switch
		{
			ValueTint.Positive => GameData.Instance.GUIConstants.PositiveTintHex, 
			ValueTint.Negative => GameData.Instance.GUIConstants.NegativeTintHex, 
			_ => GameData.Instance.GUIConstants.ValueTintHex, 
		};
	}

	public static string GetTint(int value)
	{
		if (IsPositive(value))
		{
			return GameData.Instance.GUIConstants.PositiveTintHex;
		}
		return GameData.Instance.GUIConstants.NegativeTintHex;
	}

	public static string GetTint(double value)
	{
		if (IsGreaterThanOrEqual(value, 0.0))
		{
			return GameData.Instance.GUIConstants.PositiveTintHex;
		}
		return GameData.Instance.GUIConstants.NegativeTintHex;
	}

	public static string GetTint(float value)
	{
		if (IsGreaterThanOrEqual(value, 0f))
		{
			return GameData.Instance.GUIConstants.PositiveTintHex;
		}
		return GameData.Instance.GUIConstants.NegativeTintHex;
	}

	public static string BoolToString(bool value, bool useColor)
	{
		string color = null;
		string text;
		if (value)
		{
			text = "True";
			if (useColor)
			{
				color = GameData.Instance.GUIConstants.PositiveTintHex;
			}
		}
		else
		{
			text = "False";
			if (useColor)
			{
				color = GameData.Instance.GUIConstants.NegativeTintHex;
			}
		}
		if (useColor)
		{
			return Label.ToLabel(text, color);
		}
		return text;
	}

	public static string DecimalToString(double rating)
	{
		return $"{Math.Round(rating, 1)}";
	}

	public static string DecimalToString(float value, int noOfDecimals)
	{
		return $"{Math.Round(value, noOfDecimals)}";
	}

	public static string DecimalToStringSignificant(float value, float? minimum = null, string minimumString = null, float? epsilon = null)
	{
		if (IsZero(value, epsilon))
		{
			return "0";
		}
		if (minimum.HasValue && value < minimum)
		{
			return "<" + minimumString;
		}
		if ((double)value < 0.1)
		{
			return value.ToString("G1");
		}
		return DecimalToString(value, 1);
	}

	public static void GetLocationAtDistance(Vector3 target, Vector3 from, float distanceFromTarget, out Vector3 standoffLocation)
	{
		if (DistanceOctile(target, from) > distanceFromTarget)
		{
			Vector3 vector = from - target;
			vector.Normalize();
			Vector3 vector2 = target + vector * distanceFromTarget;
			if (MapManager.IsPointReachableInStraightLine(target, vector2))
			{
				standoffLocation = vector2;
			}
			else
			{
				standoffLocation = target;
			}
		}
		else
		{
			standoffLocation = from;
		}
	}

	public static float GetInterpolatedFunctionValue(float number, Vector2[] dataPoints)
	{
		int num = dataPoints.Length - 1;
		int num2 = num;
		int num3 = 0;
		while (num2 == num && num3 <= num)
		{
			if (number <= dataPoints[num3].X)
			{
				num2 = num3;
				if (num3 > 0)
				{
					Vector2 vector = dataPoints[num3 - 1];
					Vector2 vector2 = dataPoints[num3];
					return MathHelper.Lerp(vector.Y, vector2.Y, (number - vector.X) / (vector2.X - vector.X));
				}
				return dataPoints[0].Y;
			}
			num3++;
		}
		return dataPoints[num].Y;
	}

	public static double MoveValueToNewNormalDistribution(double value, double oldMean, double oldStdDeviation, double newMean, double newStdDeviation)
	{
		double num = ((value - oldMean != 0.0 && 6.0 * oldStdDeviation != 0.0) ? Math.Abs((value - oldMean) / (6.0 * oldStdDeviation)) : 0.0);
		return newMean + (double)Math.Sign(value - oldMean) * (num * 6.0 * newStdDeviation);
	}

	public static float ShiftValue(float value)
	{
		return 2f * (value - 0.5f);
	}

	public static void GetNormalDistributionFromMinMaxValues(float min, float max, out float? mean, out float? stdDev)
	{
		mean = min + 0.5f * (max - min);
		stdDev = (max - mean) / 3f;
	}

	public static float RandomBetween(RandomGenerator random, float min, float max)
	{
		bool saveMessage = random.Type == RandomGenerator.GeneratorType.Sim;
		return min + (float)random.NextDouble("Common - RandomBetween", saveMessage) * (max - min);
	}

	public static int RandomBetween(RandomGenerator random, int min, int max)
	{
		bool saveMessage = random.Type == RandomGenerator.GeneratorType.Sim;
		return random.Next(min, max, "Common - RandomBetween", saveMessage);
	}

	public static int RandomSign(RandomGenerator random)
	{
		bool saveMessage = random.Type == RandomGenerator.GeneratorType.Sim;
		return 2 * random.Next(0, 1, "Common - RandomSign", saveMessage) - 1;
	}

	public static Point AddPoints(Point p1, Point p2)
	{
		return new Point(p1.X + p2.X, p1.Y + p2.Y);
	}

	public static float Average(float f1, float f2)
	{
		return (f1 + f2) * 0.5f;
	}

	public static float Distance(Point p1, Point p2)
	{
		return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2.0) + Math.Pow(p1.Y - p2.Y, 2.0));
	}

	public static float Distance(Vector3 p1, Vector3 p2)
	{
		return Vector3.Distance(p1, p2);
	}

	public static float DistanceOctile(WorldLocation p1, WorldLocation p2)
	{
		return DistanceOctile(p1.X, p1.Y, p2.X, p2.Y);
	}

	public static float DistanceOctile(float p1X, float p1Y, float p2X, float p2Y)
	{
		float num = Math.Abs(p1X - p2X);
		float num2 = Math.Abs(p1Y - p2Y);
		if (num > num2)
		{
			return num + num2 * 0.5f;
		}
		return num2 + num * 0.5f;
	}

	public static float DistanceOctile(Vector3 p1, WorldLocation p2)
	{
		float num = Math.Abs(p1.X - p2.X);
		float num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return num + num2 * 0.5f;
		}
		return num2 + num * 0.5f;
	}

	public static float DistanceOctile(Vector3 p1, Vector3 p2)
	{
		float num = Math.Abs(p1.X - p2.X);
		float num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return num + num2 * 0.5f;
		}
		return num2 + num * 0.5f;
	}

	public static float DistanceOctile(Vector3 p1, Vector2 p2)
	{
		float num = Math.Abs(p1.X - p2.X);
		float num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return num + num2 * 0.5f;
		}
		return num2 + num * 0.5f;
	}

	public static float DistanceOctile(Vector2 p1, Vector2 p2)
	{
		float num = Math.Abs(p1.X - p2.X);
		float num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return num + num2 * 0.5f;
		}
		return num2 + num * 0.5f;
	}

	public static float DistanceOctile(Point p1, Point p2)
	{
		int num = Math.Abs(p1.X - p2.X);
		int num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return (float)num + (float)num2 * 0.5f;
		}
		return (float)num2 + (float)num * 0.5f;
	}

	public static float DistanceOctile(TilePos p1, TilePos p2)
	{
		int num = Math.Abs(p1.X - p2.X);
		int num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return (float)num + (float)num2 * 0.5f;
		}
		return (float)num2 + (float)num * 0.5f;
	}

	public static float DistanceOctile(SubtilePos p1, SubtilePos p2)
	{
		int num = Math.Abs(p1.X - p2.X);
		int num2 = Math.Abs(p1.Y - p2.Y);
		if (num > num2)
		{
			return (float)num + (float)num2 * 0.5f;
		}
		return (float)num2 + (float)num * 0.5f;
	}

	public static bool IsZero(double? d1)
	{
		if (!d1.HasValue)
		{
			return false;
		}
		return IsZero(d1.Value);
	}

	public static bool IsZero(double d1)
	{
		if (d1 < 1E-07)
		{
			return d1 > -1E-07;
		}
		return false;
	}

	public static bool IsZero(decimal d1)
	{
		if (d1 < 0.00001m)
		{
			return d1 > -0.00001m;
		}
		return false;
	}

	public static bool IsZero(float d1, float? epsilon = null)
	{
		if (d1 < (epsilon ?? 0.0001f))
		{
			return d1 > 0f - (epsilon ?? 0.0001f);
		}
		return false;
	}

	public static bool IsEqual(double d1, double d2)
	{
		if (d1 < d2 + 1E-07)
		{
			return d1 > d2 - 1E-07;
		}
		return false;
	}

	public static bool IsEqual(double? d1, double? d2)
	{
		if (!d2.HasValue)
		{
			if (!d1.HasValue)
			{
				return true;
			}
			return false;
		}
		if (!d1.HasValue)
		{
			return false;
		}
		if (IsEqual(d2.Value, d1.Value))
		{
			return true;
		}
		return false;
	}

	public static bool IsEqual(decimal d1, decimal d2)
	{
		if (d1 < d2 + 0.00001m)
		{
			return d1 > d2 - 0.00001m;
		}
		return false;
	}

	public static bool IsEqual(float d1, float d2)
	{
		if (d1 < d2 + 0.0001f)
		{
			return d1 > d2 - 0.0001f;
		}
		return false;
	}

	public static bool IsLessThan(float valueToTest, float valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return false;
		}
		return valueToTest < valueToTestWith;
	}

	public static bool IsLessThanOrEqual(float valueToTest, float valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return true;
		}
		return valueToTest < valueToTestWith;
	}

	public static bool IsLessThanOrEqual(double valueToTest, double valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return true;
		}
		return valueToTest < valueToTestWith;
	}

	public static bool IsGreaterThan(float valueToTest, float valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return false;
		}
		return valueToTest > valueToTestWith;
	}

	public static bool IsGreaterThan(double valueToTest, double valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return false;
		}
		return valueToTest > valueToTestWith;
	}

	public static bool IsGreaterThan(decimal valueToTest, decimal valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return false;
		}
		return valueToTest > valueToTestWith;
	}

	public static bool IsGreaterThanOrEqual(float valueToTest, float valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return true;
		}
		return valueToTest > valueToTestWith;
	}

	public static bool IsGreaterThanOrEqual(double valueToTest, double valueToTestWith)
	{
		if (IsEqual(valueToTest, valueToTestWith))
		{
			return true;
		}
		return valueToTest > valueToTestWith;
	}

	public static bool IsEqual(float d1, float d2, float epsilonToUse)
	{
		if (d1 < d2 + epsilonToUse)
		{
			return d1 > d2 - epsilonToUse;
		}
		return false;
	}

	public static bool IsDirectionEqual(Vector3 v1, Vector3 v2)
	{
		if (IsEqual(v1.X, v2.X, 0.001f) && IsEqual(v1.Y, v2.Y, 0.001f))
		{
			return IsEqual(v1.Z, v2.Z, 0.001f);
		}
		return false;
	}

	public static bool IsLocationEqual(Vector3 v1, Vector3 v2)
	{
		if (IsEqual(v1.X, v2.X, 0.1f) && IsEqual(v1.Y, v2.Y, 0.1f))
		{
			return IsEqual(v1.Z, v2.Z, 0.1f);
		}
		return false;
	}

	public static bool IsLocationEqual(Vector2 v1, Vector2 v2)
	{
		if (IsEqual(v1.X, v2.X, 0.1f))
		{
			return IsEqual(v1.Y, v2.Y, 0.1f);
		}
		return false;
	}

	public static int Max(int v1, int v2)
	{
		if (v1 >= v2)
		{
			return v1;
		}
		return v2;
	}

	public static float Max(float v1, float v2)
	{
		if (v1 >= v2)
		{
			return v1;
		}
		return v2;
	}

	public static int Min(int v1, int v2)
	{
		if (v1 <= v2)
		{
			return v1;
		}
		return v2;
	}

	public static float Min(float v1, float v2)
	{
		if (v1 <= v2)
		{
			return v1;
		}
		return v2;
	}

	public static int ClampBottom(int f1, int bottom)
	{
		if (f1 <= bottom)
		{
			return bottom;
		}
		return f1;
	}

	public static float ClampBottom(float f1, float bottom)
	{
		if (!(f1 > bottom))
		{
			return bottom;
		}
		return f1;
	}

	public static byte ClampBottom(byte f1, byte bottom)
	{
		if (f1 <= bottom)
		{
			return bottom;
		}
		return f1;
	}

	public static double ClampBottom(double d1, double bottom)
	{
		if (!(d1 > bottom))
		{
			return bottom;
		}
		return d1;
	}

	public static float Clamp(float f1, float bottom, float top)
	{
		if (!(f1 > bottom))
		{
			return bottom;
		}
		if (!(f1 < top))
		{
			return top;
		}
		return f1;
	}

	public static double Clamp(double f1, double bottom, double top)
	{
		if (!(f1 > bottom))
		{
			return bottom;
		}
		if (!(f1 < top))
		{
			return top;
		}
		return f1;
	}

	public static int Clamp(int f1, int bottom, int top)
	{
		if (f1 <= bottom)
		{
			return bottom;
		}
		if (f1 >= top)
		{
			return top;
		}
		return f1;
	}

	public static Point ClampPositionToMap(int x, int y)
	{
		return new Point(Clamp(x, 0, The.Map.mapTileWidth), Clamp(y, 0, The.Map.mapTileHeight));
	}

	public static double ClampTop(double d1, double top)
	{
		if (!(d1 < top))
		{
			return top;
		}
		return d1;
	}

	public static float ClampTop(float d1, float top)
	{
		if (!(d1 < top))
		{
			return top;
		}
		return d1;
	}

	public static int ClampTop(int d1, int top)
	{
		if (d1 >= top)
		{
			return top;
		}
		return d1;
	}

	public static float WrapAngleBetweenMinusPiAndPi(float radians)
	{
		while (radians < -(float)Math.PI)
		{
			radians += (float)Math.PI * 2f;
		}
		while (radians > (float)Math.PI)
		{
			radians -= (float)Math.PI * 2f;
		}
		return radians;
	}

	public static Vector3 WrapVectorBetweenMinusNAndN(Vector3 vec, float N)
	{
		return new Vector3(WrapFloatBetweenMinusNAndN(vec.X, N), WrapFloatBetweenMinusNAndN(vec.Y, N), WrapFloatBetweenMinusNAndN(vec.Z, N));
	}

	public static float WrapFloatBetweenMinusNAndN(float flt, float N)
	{
		while (flt < 0f - N)
		{
			flt += N * 2f;
		}
		while (flt > N)
		{
			flt -= N * 2f;
		}
		return flt;
	}

	public static float WrapAngleBetweenZeroAndTwoPi(float radians)
	{
		while (radians < 0f)
		{
			radians += (float)Math.PI * 2f;
		}
		while (radians > (float)Math.PI * 2f)
		{
			radians -= (float)Math.PI * 2f;
		}
		return radians;
	}

	public static bool IntersectionOfTwoLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 result)
	{
		double num = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
		if (num == 0.0)
		{
			return false;
		}
		double num2 = (double)((a.Y - c.Y) * (d.X - c.X) - (a.X - c.X) * (d.Y - c.Y)) / num;
		result.X = (float)((double)a.X + num2 * (double)(b.X - a.X));
		result.Y = (float)((double)a.Y + num2 * (double)(b.Y - a.Y));
		return true;
	}

	public static Direction GetDirection(Point from, Point to)
	{
		int num = to.X - from.X;
		int num2 = to.Y - from.Y;
		if (num == 0 && num2 == -1)
		{
			return Direction.North;
		}
		if (num == 1 && num2 == -1)
		{
			return Direction.NorthEast;
		}
		if (num == 1 && num2 == 0)
		{
			return Direction.East;
		}
		if (num == 1 && num2 == 1)
		{
			return Direction.SouthEast;
		}
		if (num == 0 && num2 == 1)
		{
			return Direction.South;
		}
		if (num == -1 && num2 == 1)
		{
			return Direction.SouthWest;
		}
		if (num == -1 && num2 == 0)
		{
			return Direction.West;
		}
		if (num == -1 && num2 == -1)
		{
			return Direction.NorthWest;
		}
		return Direction.North;
	}

	public static Direction GetDirection(PathFinderNode from, PathFinderNode to)
	{
		int num = to.AbsoluteX - from.AbsoluteX;
		int num2 = to.AbsoluteY - from.AbsoluteY;
		if (num == 0 && num2 == -1)
		{
			return Direction.North;
		}
		if (num == 1 && num2 == -1)
		{
			return Direction.NorthEast;
		}
		if (num == 1 && num2 == 0)
		{
			return Direction.East;
		}
		if (num == 1 && num2 == 1)
		{
			return Direction.SouthEast;
		}
		if (num == 0 && num2 == 1)
		{
			return Direction.South;
		}
		if (num == -1 && num2 == 1)
		{
			return Direction.SouthWest;
		}
		if (num == -1 && num2 == 0)
		{
			return Direction.West;
		}
		if (num == -1 && num2 == -1)
		{
			return Direction.NorthWest;
		}
		return Direction.North;
	}

	public static Direction Mirror(Direction dir)
	{
		return dir switch
		{
			Direction.North => Direction.South, 
			Direction.NorthEast => Direction.SouthWest, 
			Direction.East => Direction.West, 
			Direction.SouthEast => Direction.NorthWest, 
			Direction.South => Direction.North, 
			Direction.SouthWest => Direction.NorthEast, 
			Direction.West => Direction.East, 
			Direction.NorthWest => Direction.SouthEast, 
			_ => Direction.South, 
		};
	}

	public static bool IsDiagonal(Direction dir)
	{
		if (dir != Direction.NorthEast && dir != Direction.NorthWest && dir != Direction.SouthEast)
		{
			return dir == Direction.SouthWest;
		}
		return true;
	}

	public static T GetRandomEnumValue<T>(T enumType, RandomGenerator randomGenerator)
	{
		Array values = Enum.GetValues(enumType.GetType());
		return (T)values.GetValue(randomGenerator.Next(saveMessage: randomGenerator.Type == RandomGenerator.GeneratorType.Sim, maximumValue: values.Length, getterMessage: "Common - GetRandomEnumValue"));
	}

	public static T GetRandomListMember<T>(IList<T> list, RandomGenerator randomGenerator)
	{
		return list[randomGenerator.Next(saveMessage: randomGenerator.Type == RandomGenerator.GeneratorType.Sim, maximumValue: list.Count, getterMessage: "Common - GetRandomEnumValue")];
	}

	public static int GetRandomListMemberIndex<T>(IList<T> list, RandomGenerator randomGenerator)
	{
		return randomGenerator.Next(saveMessage: randomGenerator.Type == RandomGenerator.GeneratorType.Sim, maximumValue: list.Count, getterMessage: "Common - GetRandomEnumValue");
	}

	public static bool IsAdjacent(Point p1, Point p2)
	{
		if (p1 == p2)
		{
			return false;
		}
		if (p1.X == p2.X)
		{
			return Math.Abs(p1.Y - p2.Y) <= 1;
		}
		if (p1.Y == p2.Y)
		{
			return Math.Abs(p1.X - p2.X) <= 1;
		}
		return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y) == 2;
	}

	public static Dictionary<EntityType, int> GroupItemsByType(List<IKnownEntityData> list)
	{
		Dictionary<EntityType, int> dictionary = new Dictionary<EntityType, int>();
		foreach (IKnownEntityData item in list)
		{
			int num = ((item.EntityType.ItemType == null || item.EntityType.ItemType.AmmunitionType == null) ? 1 : item.NoOfRounds.Value);
			if (dictionary.TryGetValue(item.EntityType, out var value))
			{
				dictionary[item.EntityType] = value + num;
			}
			else
			{
				dictionary.Add(item.EntityType, num);
			}
		}
		return dictionary;
	}

	public static int GetJaggedArrayWidth<T>(T[][] array)
	{
		return array.Length;
	}

	public static int GetJaggedArrayHeight<T>(T[][] array)
	{
		return array[0].Length;
	}

	public static void InitJaggedArray<T>(ref T[][] map, int width, int height)
	{
		map = new T[width][];
		for (int i = 0; i < width; i++)
		{
			map[i] = new T[height];
		}
	}

	public static bool TimepointIsOutDated(double timePoint, double max)
	{
		return The.Sim.TotalUnPausedGameTimeInSeconds - timePoint > max;
	}

	public static void CopyJaggedArray<T>(T[][] fromArray, T[][] toArray)
	{
		Parallel.For(0, fromArray.Length, delegate(int x)
		{
			T[] array = fromArray[x];
			T[] destinationArray = toArray[x];
			Array.Copy(array, destinationArray, array.Length);
		});
	}

	public static T[][] CloneJaggedArray<T>(T[][] fromArray)
	{
		T[][] clone = new T[fromArray.Length][];
		int height = GetJaggedArrayHeight(fromArray);
		Parallel.For(0, fromArray.Length, delegate(int x)
		{
			clone[x] = new T[height];
		});
		return clone;
	}

	public static void ClearJaggedArray<T>(T[][] map)
	{
		if (map.Length > 15)
		{
			Parallel.For(0, map.Length, delegate(int x)
			{
				T[] array3 = map[x];
				Array.Clear(array3, 0, array3.Length);
			});
			return;
		}
		T[][] array = map;
		foreach (T[] array2 in array)
		{
			Array.Clear(array2, 0, array2.Length);
		}
	}

	public static void ClearMap(byte[][] map)
	{
		byte[] column = map[0];
		int height = column.Length;
		Parallel.For(0, map.Length, delegate(int x)
		{
			column = map[x];
			for (int i = 0; i < height; i++)
			{
				column[i] = 0;
			}
		});
	}

	public static List<T> Randomize<T>(List<T> list, RandomGenerator rnd)
	{
		List<T> list2 = new List<T>();
		bool saveMessage = rnd.Type == RandomGenerator.GeneratorType.Sim;
		while (list.Count > 0)
		{
			int index = rnd.Next(0, list.Count, "Common - Randomize", saveMessage);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public static bool ListContainsRange<T>(List<T> containingList, List<T> otherList)
	{
		foreach (T other in otherList)
		{
			if (!containingList.Contains(other))
			{
				return false;
			}
		}
		return true;
	}

	public static float FlipOffset(float offset, float width, bool flip)
	{
		if (flip)
		{
			return width - 1f - offset;
		}
		return offset;
	}

	public static Vector3 GetAbsolutePosition(Vector3 location, Vector2 offset, bool flip)
	{
		if (flip)
		{
			return location + new Vector3(0f - offset.X, offset.Y, 0f);
		}
		return location + new Vector3(offset.X, offset.Y, 0f);
	}

	public static string ToHex(this Color color, bool includeHash)
	{
		string[] value = new string[4]
		{
			color.A.ToString("X2"),
			color.R.ToString("X2"),
			color.G.ToString("X2"),
			color.B.ToString("X2")
		};
		return (includeHash ? "#" : string.Empty) + string.Join(string.Empty, value);
	}

	public static Vector3 ToColorVector3(this string hexString)
	{
		return hexString.ColorFromHex().ToVector3();
	}

	public static Color ColorFromHex(this string hexString)
	{
		if (hexString.StartsWith("#"))
		{
			hexString = hexString.Substring(1);
		}
		uint num = uint.Parse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		Color white = Color.White;
		if (hexString.Length == 8)
		{
			white.A = (byte)(num >> 24);
			white.R = (byte)(num >> 16);
			white.G = (byte)(num >> 8);
			white.B = (byte)num;
		}
		else
		{
			if (hexString.Length != 6)
			{
				throw new InvalidOperationException("Invalid hex representation of an ARGB or RGB color value.");
			}
			white.R = (byte)(num >> 16);
			white.G = (byte)(num >> 8);
			white.B = (byte)num;
		}
		return white;
	}

	public static float DecreaseValueBetweenZeroAndOne(float value, float decreaseAmountPerDay, double deltaTimeInSeconds)
	{
		if (value > 0f)
		{
			value = (float)ClampBottom((double)value - (double)decreaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 0.0);
		}
		return value;
	}

	public static float DecreaseValueBetweenZeroAndOneBySecondsAmount(float value, float decreaseAmountPerSecond, double deltaTimeInSeconds)
	{
		if (value > 0f)
		{
			value = (float)ClampBottom((double)value - (double)decreaseAmountPerSecond * deltaTimeInSeconds, 0.0);
		}
		return value;
	}

	public static float IncreaseValueBetweenZeroAndOne(float value, float increaseAmountPerDay, double deltaTimeInSeconds)
	{
		if (value < 1f)
		{
			value = (float)ClampTop((double)value + (double)increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 1.0);
		}
		return value;
	}

	public static void IncreaseValueBetweenZeroAndOne(ref double value, float increaseAmountPerDay, double deltaTimeInSeconds)
	{
		if (value < 1.0)
		{
			value = ClampTop(value + (double)increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, 1.0);
		}
	}

	public static float IncreaseValueBetweenZeroAndTopLimit(float value, float increaseAmountPerDay, double deltaTimeInSeconds, float topLimit)
	{
		if (value < topLimit)
		{
			value = (float)ClampTop((double)value + (double)increaseAmountPerDay * deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond, topLimit);
		}
		return value;
	}

	public static float RoughVectorMagnitude(Vector3 vec)
	{
		float num = Math.Abs(vec.X);
		float num2 = Math.Abs(vec.Y);
		float num3 = Math.Abs(vec.Z);
		if (num < num2)
		{
			float num4 = num;
			num = num2;
			num2 = num4;
		}
		if (num < num3)
		{
			float num5 = num;
			num = num3;
			num3 = num5;
		}
		num2 += num3;
		return num + num2 * 0.25f;
	}

	public static string GetPriceAsString(decimal? price, bool useColoring = false, ValueTint? tintToUse = null)
	{
		if (price.HasValue)
		{
			string valueAsString = MoneyAsString(price.Value);
			return ApplyColorLabel(useColoring, tintToUse, (float)price.Value, valueAsString);
		}
		return "";
	}

	public static string MoneyAsString(decimal amount, bool abbreviate = false)
	{
		if (abbreviate)
		{
			if (amount >= 1000m)
			{
				return (amount / 1000m).ToString("G3") + "k";
			}
			return amount.ToString("N1");
		}
		return amount.ToString("N1");
	}

	public static string ListToCommaSeparatedString<T>(IList<T> items, Func<T, string> getName)
	{
		if (items == null || items.Count == 0)
		{
			return "";
		}
		string text = "";
		string text2 = "";
		for (int num = items.Count - 1; num >= 0; num--)
		{
			T arg = items[num];
			text = getName(arg) + text2 + text;
			if (text2 == "")
			{
				text2 = " and ";
			}
			else if (text2 == " and ")
			{
				text2 = ", ";
			}
		}
		return text;
	}

	public static Color GetRandomColorFromSeed(uint color)
	{
		return new Color((byte)(color * 100 % 255), (byte)(color * 23 % 255), (byte)(color * 7 % 255));
	}

	public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
	{
		IEnumerable<IEnumerable<T>> enumerable = new IEnumerable<T>[1] { Enumerable.Empty<T>() };
		foreach (IEnumerable<T> sequence in sequences)
		{
			IEnumerable<T> s = sequence;
			enumerable = from seq in enumerable
				from item in s
				select seq.Concat(new T[1] { item });
		}
		return enumerable;
	}

	public static float CalculateProgressDelta(double seconds, float gameDaysNeeded)
	{
		return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / (double)gameDaysNeeded);
	}
}
