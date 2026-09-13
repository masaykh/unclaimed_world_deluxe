using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class TakenBy : ISnapshot
{
	private Job job;

	private JobID snapshotJobID;

	private List<Entity> takenBy = new List<Entity>();

	private List<EntityID> takenByIDs = new List<EntityID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int Count => takenBy.Count;

	public bool IsSnapshotted { get; set; }

	public TakenBy()
	{
	}

	public TakenBy(Job job)
	{
		this.job = job;
	}

	public void Iterate(Action<Entity> function)
	{
		foreach (Entity item in takenBy)
		{
			function(item);
		}
	}

	private static int CompareCurrentUtility(Entity e1, Entity e2)
	{
		Intelligence intelligence = e1.Intelligence;
		Intelligence intelligence2 = e2.Intelligence;
		intelligence.GetScore();
		intelligence2.GetScore();
		if (!intelligence.GetCurrentGoalUtility().HasValue)
		{
			if (!intelligence2.GetCurrentGoalUtility().HasValue)
			{
				return 0;
			}
			return -1;
		}
		if (!intelligence2.GetCurrentGoalUtility().HasValue)
		{
			return 1;
		}
		if (intelligence.GetCurrentGoalUtility() > intelligence2.GetCurrentGoalUtility())
		{
			return 1;
		}
		if (intelligence.GetCurrentGoalUtility() == intelligence2.GetCurrentGoalUtility())
		{
			return 0;
		}
		return -1;
	}

	public bool TryRemove(Entity entity)
	{
		if (takenBy.Contains(entity))
		{
			Remove(entity);
			return true;
		}
		return false;
	}

	public void RemoveAll()
	{
		if (job.ID == (JobID)3288uL)
		{
		}
		while (takenBy.Count != 0)
		{
			Remove(takenBy[0]);
		}
	}

	public void Remove(Entity entity)
	{
		_ = job.ID;
		_ = 3288;
		takenBy.RemoveAll((Entity e) => e == entity);
		_ = job;
	}

	public bool Contains(Entity entity)
	{
		return takenBy.Contains(entity);
	}

	public bool Contains(EntityID entityID)
	{
		return takenBy.Exists((Entity e) => e.EntityID == entityID);
	}

	public void Add(Entity entity)
	{
		_ = job.ID;
		_ = 3288;
		takenBy.Add(entity);
	}

	public Entity GetLowestScorer()
	{
		takenBy.Sort(CompareCurrentUtility);
		return takenBy[0];
	}

	public List<Entity> GetSortedByDistance(Vector3 location)
	{
		return takenBy.OrderByDescending((Entity e) => (!e.IsOnPlaySite()) ? float.MaxValue : Common.DistanceOctile(e.PlaySiteLocation, location)).ToList();
	}

	public Entity Get(int index)
	{
		return takenBy[index];
	}

	public Entity Get(Func<Entity, bool> matches)
	{
		return takenBy.FirstOrDefault(matches);
	}

	public bool IsScoreGreaterThanAnyTaker(double utilityToTest)
	{
		if (takenBy.Count > 0)
		{
			for (int i = 0; i < takenBy.Count; i++)
			{
				Entity entity = takenBy[i];
				if (IsScoreGreater(utilityToTest, entity))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsScoreGreater(double utilityToTest, Entity entity)
	{
		if (entity.Intelligence.GetScore() < utilityToTest)
		{
			return true;
		}
		return false;
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
			takenByIDs = takenBy.Select((Entity e) => e.EntityID).ToList();
		}
		takenByIDs = sn.DoList(takenByIDs);
		snapshotJobID = sn.SnapshotID<Job, JobID>(job).Value;
		sn.Ignore(takenBy);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		takenBy = takenByIDs.Select((EntityID c) => LookUp<Entity, EntityID>.FindByID(c)).ToList();
		job = LookUp<Job, JobID>.FindByID(snapshotJobID);
		takenByIDs.Clear();
	}
}
