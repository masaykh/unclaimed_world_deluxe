using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalChecking : CompositeGoal, ITopLevelGoal
{
	private Vector3 locationOfScoutPoint;

	private CheckProcessJob job;

	private JobID? snapshotJob;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalChecking(Entity owner, CheckProcessJob job, List<EntityGroupID> ownersVehicles)
		: base(owner)
	{
		this.job = job;
		ownersOfVehicles = ownersVehicles;
	}

	public GoalChecking()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		job.TakeJob(entity);
		List<ItemType.TaskType> gearTasks = null;
		CompositeGoal.AddNightActivityGear(ref gearTasks);
		FindOptionalEquipmentIfNeeded(job.Location, job, equipWeapon: true, equipFood: true, mountWeapon: true, gearTasks);
		locationOfScoutPoint = job.Location;
		AddSubgoal(new GoalMoveToPosition(entity, locationOfScoutPoint, ownersOfVehicles)
		{
			IsFinalDestination = true
		});
		locationOfScoutPoint = entityIntelligence.CurrentExpedition.Center.Value;
	}

	public double ScoreGoal()
	{
		return ScoreJobGoal(job);
	}

	public override string GetStatus()
	{
		return "Checking progress";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!ArePreconditionsOK())
		{
			base.Status = Status.Failed;
		}
		else
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		if (base.Status == Status.Completed)
		{
			if (Common.DistanceOctile(entity.PlaySiteLocation, job.Location) < 10f)
			{
				DestroyJobAndRemoveLocks(ref job);
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Failed;
			}
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
			job = (CheckProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
