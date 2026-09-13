using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class IDActionEvent<T> : ISnapshot
{
	public Dictionary<MethodID, MethodID> Subscribers = new Dictionary<MethodID, MethodID>();

	private List<MethodID> subscriberList = new List<MethodID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void AddAndRegister(Action<T> method, IIDEventSubscriber client, out MethodID methodID)
	{
		methodID = ActionLookup<T>.AddWithNewID(method);
		Add(methodID, client);
	}

	public void AddAndRegister(Action<T> method, IIDEventSubscriber client, out MethodID? outMethodID)
	{
		AddAndRegister(method, client, out MethodID methodID);
		outMethodID = methodID;
	}

	public void Add(MethodID eventHandlerId, IIDEventSubscriber client)
	{
		if (!Subscribers.ContainsKey(eventHandlerId))
		{
			Subscribers.Add(eventHandlerId, eventHandlerId);
			subscriberList.Add(eventHandlerId);
		}
	}

	public void Remove(MethodID eventHandlerId)
	{
		if (Subscribers.ContainsKey(eventHandlerId))
		{
			Subscribers.Remove(eventHandlerId);
			subscriberList.Remove(eventHandlerId);
		}
	}

	public void Invoke(T invokeArgument)
	{
		for (int num = subscriberList.Count - 1; num >= 0; num--)
		{
			MethodID methodID = subscriberList[num];
			Action<T> action = ActionLookup<T>.FindByID(methodID);
			if (action != null)
			{
				action(invokeArgument);
			}
			else
			{
				Remove(methodID);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Subscribers = sn.DoDictionary(Subscribers);
		subscriberList = sn.DoList(subscriberList);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
public class IDActionEvent : ISnapshot
{
	public Dictionary<MethodID, MethodID> Subscribers = new Dictionary<MethodID, MethodID>();

	private List<MethodID> subscriberList = new List<MethodID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void AddAndRegister(Action method, IIDEventSubscriber client, out MethodID methodID)
	{
		methodID = ActionLookup.AddWithNewID(method);
		Add(methodID, client);
	}

	public void AddAndRegister(Action method, IIDEventSubscriber client, out MethodID? outMethodID)
	{
		AddAndRegister(method, client, out MethodID methodID);
		outMethodID = methodID;
	}

	public void Add(MethodID eventHandlerId, IIDEventSubscriber client)
	{
		if (!Subscribers.ContainsKey(eventHandlerId))
		{
			Subscribers.Add(eventHandlerId, eventHandlerId);
			subscriberList.Add(eventHandlerId);
		}
	}

	public void Remove(MethodID eventHandlerId)
	{
		if (Subscribers.ContainsKey(eventHandlerId))
		{
			Subscribers.Remove(eventHandlerId);
			subscriberList.Remove(eventHandlerId);
		}
	}

	public void Invoke()
	{
		for (int num = subscriberList.Count - 1; num >= 0; num--)
		{
			MethodID methodID = subscriberList[num];
			Action action = ActionLookup.FindByID(methodID);
			if (action != null)
			{
				action();
			}
			else
			{
				Remove(methodID);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Subscribers = sn.DoDictionary(Subscribers);
		subscriberList = sn.DoList(subscriberList);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
