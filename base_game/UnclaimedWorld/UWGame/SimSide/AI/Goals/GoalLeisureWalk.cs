using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalLeisureWalk : CompositeGoal, ITopLevelGoal
{
	private double timeToRest;

	private GroupMoveActivity activity;

	private double timeWaited;

	private const double maxTimeToWait = 6.0;

	public int minimumMembers = 2;

	private bool hasStarted;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalLeisureWalk(Entity owner, GroupMoveActivity activity)
		: base(owner)
	{
		this.activity = activity;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		throw new Exception("THIS CLASS IS OBSOLETE");
	}

	public GoalLeisureWalk()
	{
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (activity == null)
		{
			activity = new LeisureWalkActivity(personEntity.Household.OwnedEntities.Activities, new Vector3(90f, 40f, 0f));
		}
		activity.AddMember(entity);
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (activity.Members.Count >= minimumMembers)
		{
			if (!activity.HasStarted)
			{
				if (activity.Leader == null)
				{
					activity.Leader = entity;
				}
				activity.HasStarted = true;
			}
			if (!hasStarted)
			{
				GoalMoveToPosition goalMoveToPosition = new GoalMoveToPosition(entity, activity.Destination, null, GoalMoveToPosition.VehicleUse.NoVehicle);
				goalMoveToPosition.GroupMoveActivity = activity;
				AddSubgoal(goalMoveToPosition);
				hasStarted = true;
			}
			base.Status = ProcessSubgoals(elapsed);
		}
		else if (!activity.HasStarted && timeWaited < 6.0)
		{
			timeWaited += elapsed.ElapsedGameTime.TotalSeconds;
		}
		else
		{
			base.Status = Status.Failed;
			if (!activity.HasStarted)
			{
				entityIntelligence.Memory.TimePointOfFailedLeisureWalkAttempt = The.Sim.TotalUnPausedGameTime.TotalSeconds;
			}
		}
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public override void Deactivate()
	{
		activity.LeaveActivity(entity);
		if (!activity.HasEnded && activity.EveryoneElseIsReady(entity))
		{
			activity.SendMessageToEveryoneElse(entity, new Message(Message.MessageTypes.EndGroupMovement));
		}
	}
}
