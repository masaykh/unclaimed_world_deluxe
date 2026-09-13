using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Ledger;

public class SheetSettings<T> : ISnapshot where T : struct, IComparable, IFormattable, IConvertible
{
	public SortingSettings<T> SortingSettings;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		SortingSettings = (SortingSettings<T>)sn.DoISnapshot(SortingSettings);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		SortingSettings.LoadPostProcess(sn);
	}
}
