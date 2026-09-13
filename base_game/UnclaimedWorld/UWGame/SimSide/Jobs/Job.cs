using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public abstract class Job : ISnapshot, ILookUp<Job, JobID>
{
	public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

	public TakenBy TakenBy;

	public double DebugScore;

	public double DebugScoreNoTools;

	public EntityGroupID EntityGroupID;

	public double Timestamp;

	protected JobType jobType;

	private JobID id = JobID.Invalid;

	private static JobID IDCounter = JobID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public virtual Priority Priority { get; set; }

	public JobID ID
	{
		get
		{
			return id;
		}
		private set
		{
			_ = uint.MaxValue;
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public virtual int MaxJobPositions => 1;

	public virtual bool RequiresBoldStance => false;

	public Job(EntityGroup entityGroup, bool addToJobsGroupNow = true)
	{
		EntityGroupID = entityGroup.ID;
		AddToLookup();
		if (addToJobsGroupNow)
		{
			entityGroup.AddJob(this);
		}
		Timestamp = The.Sim.TotalUnPausedGameTime.TotalMilliseconds;
		TakenBy = new TakenBy(this);
		AddLog("Created");
	}

	protected void SetDefaultPriority(EntityGroup entityGroup)
	{
		JobType jobType = GetJobType();
		if (jobType != null)
		{
			Priority = entityGroup.Policy.GetPriority(jobType);
		}
	}

	public Job()
	{
	}

	public static string GetPriorityAsString(Priority priority)
	{
		return priority switch
		{
			Priority.Low => "LOW", 
			Priority.Normal => "NORMAL", 
			Priority.High => "HIGH", 
			_ => null, 
		};
	}

	public JobID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= JobID.Invalid)
		{
			throw new Exception("Astounding, JobID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public JobID SnapshotID(Snapshotter sn, JobID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		_ = ID;
		_ = 423;
		if (ID != JobID.Invalid)
		{
			LookUp<Job, JobID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = JobID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Job, JobID>.Remove(this);
	}

	void ILookUp<Job, JobID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = JobID.First;
	}

	void ILookUp<Job, JobID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Job, JobID>.Create();
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		Priority = sn.DoEnum(Priority);
		TakenBy = (TakenBy)sn.DoISnapshot(TakenBy);
		Timestamp = sn.DoDouble(Timestamp);
		EntityGroupID = sn.DoEnum(EntityGroupID);
		jobType = sn.DoGameData(jobType);
		Log = sn.DoList(Log);
		sn.Ignore(DebugScore);
		sn.Ignore(DebugScoreNoTools);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		TakenBy.LoadPostProcess(sn);
	}

	public void AddLog(string text)
	{
	}

	public virtual void TakeJob(Entity entity)
	{
		if (!TakenBy.Contains(entity))
		{
			TakenBy.Add(entity);
		}
	}

	protected virtual void ComputeJobType()
	{
		foreach (KeyValuePair<string, JobType> allJobType in GameData.Instance.AllJobTypes)
		{
			if (allJobType.Value.IsType(this))
			{
				jobType = allJobType.Value;
				break;
			}
		}
	}

	public JobType GetJobType()
	{
		return jobType;
	}

	private string GetAbandonedByText(Entity entity)
	{
		return "Abandoned by: " + entity.ToString();
	}

	private string GetTakenByText(Entity entity)
	{
		return "Taken by: " + entity.ToString();
	}

	public double? GetTakerScore()
	{
		if (TakenBy.Count > 0)
		{
			return TakenBy.Get(0).Intelligence.Brain.ScoreTopLevelGoal();
		}
		return null;
	}

	public virtual void Abandon(Entity entity, bool isDestroyingJob = false)
	{
		TakenBy.TryRemove(entity);
	}

	public virtual void Destroy(bool cancelTakers, Entity entityToExclude = null)
	{
		if (ID != JobID.Invalid)
		{
			AddLog("Destroyed");
			if (cancelTakers)
			{
				CancelAllTakers(entityToExclude);
			}
			if (ResolveOwner(out var owner))
			{
				owner.RemoveJob(this);
			}
			The.Client.DestroyAccessibility(ID);
			RemoveIDEntry();
		}
	}

	public void CancelAllTakers(Entity entityToExclude)
	{
		for (int num = TakenBy.Count - 1; num >= 0; num--)
		{
			Entity entity = TakenBy.Get(num);
			if (entity != entityToExclude)
			{
				AddLog("Sent Cancel Job message to: " + entity.ToString());
				entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
			}
		}
	}

	public bool ResolveOwner(out EntityGroup owner)
	{
		owner = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
		return owner != null;
	}

	public virtual bool UserCanCancel(out string tooltip)
	{
		tooltip = "";
		return true;
	}

	public Entity GetAssignedWorker()
	{
		if (TakenBy.Count > 0)
		{
			return TakenBy.Get(0);
		}
		return null;
	}

	public virtual void GetLocation(out Point? tilePos, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tilePos = null;
		targetEntity = null;
		zoneID = null;
	}

	public abstract Vector3? GetCircaLocation();

	public virtual string GetName()
	{
		return "";
	}

	public List<Entity> GetTakersSortedByDistance()
	{
		Vector3? circaLocation = GetCircaLocation();
		if (circaLocation.HasValue)
		{
			return TakenBy.GetSortedByDistance(circaLocation.Value);
		}
		List<Entity> takers = new List<Entity>();
		TakenBy.Iterate(delegate(Entity e)
		{
			takers.Add(e);
		});
		return takers;
	}
}
