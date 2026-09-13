using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class DebugLog : ISnapshot
{
	public List<Tuple<double, string>> Entries = new List<Tuple<double, string>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void Add(string text)
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Entries = sn.DoList(Entries);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
