using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class ObservableList<T> : ISnapshot
{
	private List<T> list = new List<T>();

	public IDActionEvent<int> ListMemberRemoved = new IDActionEvent<int>();

	public IDActionEvent ListMemberAdded = new IDActionEvent();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int Count => list.Count;

	public T this[int key]
	{
		get
		{
			return list[key];
		}
		set
		{
			list[key] = value;
		}
	}

	public bool IsSnapshotted { get; set; }

	public List<T> GetAsList()
	{
		return list;
	}

	public bool Remove(T objectToRemove)
	{
		if (ListMemberRemoved != null)
		{
			int num = list.IndexOf(objectToRemove);
			if (num >= 0)
			{
				list.RemoveAt(num);
				ListMemberRemoved.Invoke(num);
			}
			return true;
		}
		return list.Remove(objectToRemove);
	}

	public void Add(T objectToRemove)
	{
		list.Add(objectToRemove);
		if (ListMemberAdded != null)
		{
			ListMemberAdded.Invoke();
		}
	}

	public void AddRange(IEnumerable<T> itemsToAdd)
	{
		list.AddRange(itemsToAdd);
		if (ListMemberAdded != null)
		{
			ListMemberAdded.Invoke();
		}
	}

	public static void UpdateCounterWhenItemIsRemoved(ref int itemCounter, int indexOfRemovedItem)
	{
		if (itemCounter > 0 && indexOfRemovedItem <= itemCounter)
		{
			itemCounter--;
		}
	}

	public void Clear()
	{
		list.Clear();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		list = sn.DoList(list);
		ListMemberAdded = (IDActionEvent)sn.DoISnapshot(ListMemberAdded);
		ListMemberRemoved = (IDActionEvent<int>)sn.DoISnapshot(ListMemberRemoved);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		ListMemberAdded.LoadPostProcess(sn);
		ListMemberRemoved.LoadPostProcess(sn);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
