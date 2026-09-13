using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems;

public class SleepyUpdater<T> where T : ISleepingUpdatable
{
	private List<T> listOfNonSleepingItems = new List<T>();

	private Dictionary<T, T> lookupMap = new Dictionary<T, T>();

	private bool listIsDirty;

	private bool staggerUpdates;

	private Module belongsToModule;

	private HighResolutionTime timer;

	private SleepyUpdaterID id = SleepyUpdaterID.Invalid;

	private static SleepyUpdaterID IDCounter = SleepyUpdaterID.First;

	public int Count => listOfNonSleepingItems.Count;

	public SleepyUpdaterID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public SleepyUpdater()
	{
	}

	public SleepyUpdater(Module module, bool staggerUpdates = false)
	{
		this.staggerUpdates = staggerUpdates;
		belongsToModule = module;
		AddToLookup();
		timer = new HighResolutionTime();
	}

	private void SetDirty()
	{
		listIsDirty = true;
	}

	private void AddToUpdatables(T item)
	{
		if (!lookupMap.ContainsKey(item))
		{
			lookupMap.Add(item, item);
			listOfNonSleepingItems.Add(item);
			SetDirty();
		}
	}

	private void RemoveFromUpdatables(T item)
	{
		if (lookupMap.Remove(item))
		{
			listOfNonSleepingItems.Remove(item);
		}
	}

	public void Add(T itemToAdd, bool? staggerUpdate = null, bool keepExistingTimepoint = false)
	{
		itemToAdd.SleepyUpdater = id;
		bool flag = false;
		if (staggerUpdates && staggerUpdate != false)
		{
			flag = true;
		}
		SetNextTimePoint(itemToAdd, flag, intervalChanged: true, keepExistingTimepoint);
	}

	public void NotifyUpdateIntervalChanged(ISleepingUpdatable item)
	{
		SetNextTimePoint((T)item, staggerUpdates: false, intervalChanged: true);
	}

	public void Remove(T item)
	{
		RemoveFromUpdatables(item);
	}

	public void Update(GameTime gameTime)
	{
		if (listOfNonSleepingItems.Count == 0)
		{
			return;
		}
		if (listIsDirty)
		{
			SortTimedUpdatableList(listOfNonSleepingItems);
			listIsDirty = false;
		}
		int num = 0;
		while (listOfNonSleepingItems.Count > 0 && num < listOfNonSleepingItems.Count)
		{
			T updatable = listOfNonSleepingItems[num];
			if (!updatable.TimePointInSeconds.HasValue || !The.Sim.TimepointReached(updatable.TimePointInSeconds.Value))
			{
				break;
			}
			updatable.Update(gameTime, out var wasDestroyed);
			if (The.Sim == null)
			{
				break;
			}
			if (!wasDestroyed)
			{
				num++;
				updatable.RecomputeUpdateInterval(out var intervalChanged);
				if (!intervalChanged)
				{
					SetNextTimePoint(updatable, staggerUpdates: false, intervalChanged);
				}
			}
		}
	}

	public void IterateItems(Action<T> action)
	{
		foreach (T listOfNonSleepingItem in listOfNonSleepingItems)
		{
			action(listOfNonSleepingItem);
		}
	}

	public T GetItem(Func<T, bool> matches)
	{
		return listOfNonSleepingItems.FirstOrDefault(matches);
	}

	private void SetNextTimePoint(T updatable, bool staggerUpdates, bool intervalChanged, bool keepExistingTimePoint = false)
	{
		double? updateInterval = updatable.UpdateInterval;
		if (!updateInterval.HasValue)
		{
			updatable.SetNextTimepoint(null);
			RemoveFromUpdatables(updatable);
			return;
		}
		if (Common.IsZero(updateInterval))
		{
			double? nextTimepoint = UpdateTimePoints.ComputeTimePointFromInterval(0.0);
			updatable.SetNextTimepoint(nextTimepoint);
			if (intervalChanged)
			{
				AddToUpdatables(updatable);
			}
			return;
		}
		if (!keepExistingTimePoint)
		{
			double num = updateInterval.Value;
			if (staggerUpdates)
			{
				double num2 = ((belongsToModule != Module.Sim) ? The.Client.ClientRandomGenerator.NextDouble("") : The.Sim.GameplayRandomGenerator.NextDouble(""));
				num += (0.5 - num2) * num;
				num = Common.ClampBottom(num, 0.0);
			}
			double? nextTimepoint2 = UpdateTimePoints.ComputeTimePointFromInterval(num);
			updatable.SetNextTimepoint(nextTimepoint2);
		}
		if (intervalChanged)
		{
			AddToUpdatables(updatable);
		}
		SetDirty();
	}

	private void SortTimedUpdatableList<U>(List<U> list) where U : ISleepingUpdatable
	{
		list.Sort(delegate(U e1, U e2)
		{
			if (!e1.TimePointInSeconds.HasValue)
			{
				if (!e2.TimePointInSeconds.HasValue)
				{
					return 0;
				}
				return 1;
			}
			return (!e2.TimePointInSeconds.HasValue) ? (-1) : e1.TimePointInSeconds.Value.CompareTo(e2.TimePointInSeconds.Value);
		});
	}

	public SleepyUpdaterID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, SleepyUpdaterID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SleepyUpdaterID SnapshotID(Snapshotter sn, SleepyUpdaterID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		_ = typeof(T) == typeof(Entity);
		ID = GetUniqueID();
		if (ID != SleepyUpdaterID.Invalid)
		{
			LookUpSleepyUpdater<T>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUpSleepyUpdater<T>.Remove(this);
	}

	public static void ResetIDCounter()
	{
		IDCounter = SleepyUpdaterID.First;
	}

	public void SetInvalid()
	{
		id = SleepyUpdaterID.Invalid;
	}
}
