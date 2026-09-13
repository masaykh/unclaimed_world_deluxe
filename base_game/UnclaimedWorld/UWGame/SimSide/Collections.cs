using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class Collections : Dictionary<Type, ILookUpCollectible>, ISnapshot
{
	private Dictionary<Type, ILookUpCollectible> snapshotCollection = new Dictionary<Type, ILookUpCollectible>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<Type, ILookUpCollectible> current = enumerator.Current;
				snapshotCollection.Add(current.Key, current.Value);
			}
		}
		snapshotCollection = sn.DoDictionary(snapshotCollection);
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
		foreach (KeyValuePair<Type, ILookUpCollectible> item in snapshotCollection)
		{
			Add(item.Key, item.Value);
		}
		snapshotCollection.Clear();
		IOrderedEnumerable<ILookUpCollectible> orderedEnumerable = from l in base.Values.ToList()
			orderby l.LoadPostProcessOrder
			select l;
		int num = 0;
		foreach (ILookUpCollectible item2 in orderedEnumerable)
		{
			item2.LoadPostProcess(sn);
			num++;
		}
	}
}
