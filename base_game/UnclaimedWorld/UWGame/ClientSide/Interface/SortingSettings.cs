using System;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SortingSettings<T> : ISnapshot where T : struct, IComparable, IFormattable, IConvertible
{
	public T SortedBy;

	public Grid.Sorting SortOrder;

	public const Grid.Sorting DefaultSortOrder = Grid.Sorting.Ascending;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public SortingSettings()
	{
	}

	public SortingSettings(T defaultSortColumn, Grid.Sorting initialSortingOrder)
	{
		SortedBy = defaultSortColumn;
		SortOrder = initialSortingOrder;
	}

	public void SetDefaultSortOrder()
	{
		SortOrder = Grid.Sorting.Ascending;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		SortedBy = sn.DoEnum(SortedBy);
		SortOrder = sn.DoEnum(SortOrder);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
