using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.GatheringSites;

public class Arc : ISnapshot
{
	public float Radius;

	public double MinAngle;

	public double MaxAngle;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Arc(float rad, int min, int max)
	{
		Radius = rad;
		MinAngle = Math.PI * (double)min / 180.0;
		MaxAngle = Math.PI * (double)max / 180.0;
	}

	public Arc()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Radius = sn.DoFloat(Radius);
		MinAngle = sn.DoDouble(MinAngle);
		MaxAngle = sn.DoDouble(MaxAngle);
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
