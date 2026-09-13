using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.Triggers;

public class TriggerSystem : ISnapshot
{
	private SortedList<TriggerPriority, SleepyUpdater<Trigger>> allTriggers = new SortedList<TriggerPriority, SleepyUpdater<Trigger>>();

	private Dictionary<TriggerPriority, List<TriggerID>> snapshotAllTriggers = new Dictionary<TriggerPriority, List<TriggerID>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public TriggerSystem()
	{
		bool staggerUpdates = true;
		allTriggers.Add(TriggerPriority.Lowest, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
		allTriggers.Add(TriggerPriority.Low, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
		allTriggers.Add(TriggerPriority.Normal, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
		allTriggers.Add(TriggerPriority.High, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
		allTriggers.Add(TriggerPriority.Highest, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
	}

	public void RegisterTrigger(Trigger trigger)
	{
		allTriggers[trigger.TriggerType.Priority].Add(trigger);
	}

	public void DeleteTrigger(Trigger trigger)
	{
		allTriggers[trigger.TriggerType.Priority].Remove(trigger);
	}

	public void Update(GameTime elapsed)
	{
		UpdateTriggers(elapsed);
	}

	private void UpdateTriggers(GameTime gameTime)
	{
		for (int i = 0; i < allTriggers.Count; i++)
		{
			allTriggers[(TriggerPriority)i].Update(gameTime);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<TriggerPriority, SleepyUpdater<Trigger>> allTrigger in allTriggers)
			{
				List<TriggerID> triggerIDs = new List<TriggerID>();
				allTrigger.Value.IterateItems(delegate(Trigger t)
				{
					triggerIDs.Add(t.ID);
				});
				snapshotAllTriggers.Add(allTrigger.Key, triggerIDs);
			}
		}
		snapshotAllTriggers = sn.DoMultiMap(snapshotAllTriggers);
		sn.Ignore(allTriggers);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (KeyValuePair<TriggerPriority, List<TriggerID>> snapshotAllTrigger in snapshotAllTriggers)
		{
			foreach (TriggerID item in snapshotAllTrigger.Value)
			{
				RegisterTrigger(LookUp<Trigger, TriggerID>.FindByID(item));
			}
		}
		snapshotAllTriggers.Clear();
	}
}
