using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalScouting : CompositeGoal, ITopLevelGoal
{
	private Vector3 locationOfScoutPoint;

	private ScoutingJob job;

	private JobID? snapshotJob;

	private Vector3? locationInSearchArea;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalScouting(Entity owner, ScoutingJob job, List<EntityGroupID> ownersVehicles, Vector3? locationInSearchArea)
		: base(owner)
	{
		this.job = job;
		ownersOfVehicles = ownersVehicles;
		this.locationInSearchArea = locationInSearchArea;
	}

	public GoalScouting()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		job.TakeJob(entity);
		Vector3? vector = job.Location ?? locationInSearchArea;
		List<ItemType.TaskType> gearTasks = ((!job.Examine) ? new List<ItemType.TaskType> { ItemType.TaskType.Scouting } : new List<ItemType.TaskType> { ItemType.TaskType.Examining });
		CompositeGoal.AddNightActivityGear(ref gearTasks);
		FindOptionalEquipmentIfNeeded(vector.Value, job, equipWeapon: true, equipFood: true, mountWeapon: true, gearTasks);
		Common.DistanceOctile(vector.Value, entity.PlaySiteLocation);
		_ = 1500f;
		if (job.Location.HasValue)
		{
			locationOfScoutPoint = job.Location.Value;
			AddSubgoal(new GoalMoveToPosition(entity, locationOfScoutPoint, ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			locationOfScoutPoint = entityIntelligence.CurrentExpedition.Center.Value;
		}
		else
		{
			AddSubgoal(new GoalMoveToPosition(entity, locationInSearchArea.Value, ownersOfVehicles));
			AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, isStealthy: false, job.Examine));
		}
	}

	public double ScoreGoal()
	{
		if (entity.Name != null)
		{
			entity.Name.Contains("onlan");
		}
		return ScoreJobGoal(job);
	}

	protected override bool ArePreconditionsOK()
	{
		return true;
	}

	public override string GetStatus()
	{
		return "Scouting";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (entity.Name != null)
		{
			entity.Name.Contains("onlan");
		}
		if (!ArePreconditionsOK())
		{
			base.Status = Status.Failed;
		}
		else
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		if (base.Status != Status.Completed)
		{
			return;
		}
		if (job.Location.HasValue)
		{
			if (Common.DistanceOctile(entity.PlaySiteLocation, job.Location.Value) < 10f)
			{
				DestroyJobAndRemoveLocks(ref job);
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
		else
		{
			DestroyJobAndRemoveLocks(ref job);
			base.Status = Status.Completed;
		}
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
	}

	public override void Deactivate()
	{
		if (job != null)
		{
			RemoveLocksFromJob(job);
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				base.Status = Status.Failed;
				job.Abandon(entity);
				return true;
			}
			return false;
		}
		return true;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		locationInSearchArea = sn.DoVector3Nullable(locationInSearchArea);
		locationOfScoutPoint = sn.DoVector3(locationOfScoutPoint);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		sn.Ignore(job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			job = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
