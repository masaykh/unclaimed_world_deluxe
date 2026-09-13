using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class LookUpGoals : ILookUpCollectible, ISnapshot
{
	private static Dictionary<GoalID, Goal> collection;

	private static LookUpGoals instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 200;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpGoals();
			Sim.AddLookupCollectible(typeof(Goal), instance);
			collection = new Dictionary<GoalID, Goal>();
		}
	}

	public void ClearCollection()
	{
		collection.Clear();
	}

	public static Goal FindByID(GoalID? id)
	{
		if (id.HasValue)
		{
			collection.TryGetValue(id.Value, out var value);
			return value;
		}
		return null;
	}

	public static List<Goal> GetGoals(Entity entity)
	{
		return (from i in collection
			where i.Value.entity == entity
			select i.Value).ToList();
	}

	public static void Remove(Goal goal)
	{
		collection.Remove(goal.ID);
		goal.SetInvalid();
	}

	public static void Add(GoalID goalID, Goal goal)
	{
		collection.Add(goalID, goal);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		collection = sn.DoDictionary(collection);
		sn.Ignore(instance);
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
		foreach (KeyValuePair<GoalID, Goal> item in collection)
		{
			if (item.Value is GoalThink)
			{
				item.Value.LoadPostProcess(sn);
			}
		}
		foreach (KeyValuePair<GoalID, Goal> item2 in collection)
		{
			if (!(item2.Value is GoalThink))
			{
				item2.Value.LoadPostProcess(sn);
			}
		}
	}
}
