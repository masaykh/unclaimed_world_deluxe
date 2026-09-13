using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class DataPoint<T> : ISnapshot, IComparable
{
	public DateAndTime.TimeDateYear Time;

	public T Value;

	private static Snapshotter.TypeInformation genericTypeInfo;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public DataPoint()
	{
	}

	public DataPoint(T value)
	{
		Value = value;
	}

	public DataPoint(T value, DateAndTime.TimeDateYear time)
	{
		Value = value;
		Time = time;
	}

	static DataPoint()
	{
		Snapshotter.GetStaticTypeInfo<T>(out genericTypeInfo);
	}

	public int CompareTo(object obj)
	{
		return Time.CompareTo(((DataPoint<T>)obj).Time);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Value = (T)sn.DoElement(genericTypeInfo, Value);
		Time = sn.DoTimeDateYear(Time);
		sn.Ignore(genericTypeInfo);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (genericTypeInfo.IsSnapshot)
		{
			((ISnapshot)(object)Value).LoadPostProcess(sn);
		}
	}
}
