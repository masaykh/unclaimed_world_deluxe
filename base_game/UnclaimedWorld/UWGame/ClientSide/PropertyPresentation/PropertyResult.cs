using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.PropertyPresentation;

public struct PropertyResult : ISnapshot
{
	public string PropertyKeyName;

	public string StringResult;

	public float? NumberResult;

	public Pair<float, float> NumberPairResult;

	public bool? BoolResult;

	public Vector2? LocationResult;

	public DateAndTime.TimeDateYear? DateResult;

	public List<string> MultiResults;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public override string ToString()
	{
		if (StringResult != null)
		{
			return StringResult;
		}
		if (BoolResult.HasValue)
		{
			return BoolResult.ToString();
		}
		if (NumberResult.HasValue)
		{
			return NumberResult.Value.ToString();
		}
		if (LocationResult.HasValue)
		{
			return LocationResult.Value.ToString();
		}
		if (NumberPairResult != null)
		{
			return NumberPairResult.ToString();
		}
		if (MultiResults != null)
		{
			return string.Concat(MultiResults.ToArray());
		}
		if (DateResult.HasValue)
		{
			return DateResult.Value.ToString();
		}
		return "";
	}

	public int? GetIntegerResult()
	{
		if (NumberResult.HasValue)
		{
			return (int)Math.Round(NumberResult.Value);
		}
		return null;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		PropertyKeyName = sn.DoString(PropertyKeyName);
		BoolResult = sn.DoBoolNullable(BoolResult);
		LocationResult = sn.DoVector2Nullable(LocationResult);
		NumberResult = sn.DoFloatNullable(NumberResult);
		NumberPairResult = sn.DoPair(NumberPairResult);
		StringResult = sn.DoString(StringResult);
		MultiResults = sn.DoList(MultiResults);
		DateResult = sn.DoTimeDateYearNullable(DateResult);
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
