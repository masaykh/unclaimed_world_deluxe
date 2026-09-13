using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalSleep : CompositeGoal, ITopLevelGoal
{
	private EntityID? containerToSleepIn;

	private WorldLocation? groundLocationToSleepOn;

	private EvaluateSleep.GroundSleepArea? groundSleepArea;

	private ExpeditionID? expeditionToSleepAt;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalSleep(Entity owner, WorldLocation? groundLocationToSleepOn, ExpeditionID? expeditionToSleepAt, EntityID? containerToSleepIn, EvaluateSleep.GroundSleepArea? groundSleepArea, List<EntityGroupID> ownersOfVehicles)
		: base(owner)
	{
		this.expeditionToSleepAt = expeditionToSleepAt;
		this.groundLocationToSleepOn = groundLocationToSleepOn;
		this.containerToSleepIn = containerToSleepIn;
		this.groundSleepArea = groundSleepArea;
		base.ownersOfVehicles = ownersOfVehicles;
	}

	public GoalSleep()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (expeditionToSleepAt.HasValue || containerToSleepIn.HasValue)
		{
			GoToGatheringSite();
		}
		else if (groundLocationToSleepOn.HasValue)
		{
			AddSubgoal(new GoalMoveToPosition(entity, groundLocationToSleepOn.Value.ToVector3(), ownersOfVehicles)
			{
				IsFinalDestination = true
			});
			AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
		}
		else
		{
			AddSubgoal(new GoalDoSleep(entity, (EvaluateSleep)GoalEvaluator));
		}
	}

	private void GoToGatheringSite()
	{
		if (expeditionToSleepAt.HasValue)
		{
			if (Expedition.FindByID(expeditionToSleepAt.Value).GatheringSite.CanAddVisitor(ref base.entity))
			{
				AddSubgoal(new GoalArriveAsVisitor(base.entity, expeditionToSleepAt, ownersOfVehicles));
			}
			AddSubgoal(new GoalDoSleep(base.entity, (EvaluateSleep)GoalEvaluator));
			return;
		}
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(containerToSleepIn.Value, out var data)))
		{
			base.Status = Status.Failed;
			return;
		}
		if (data.GatheringSite != null)
		{
			if (data.GatheringSite.CanAddVisitor(ref base.entity))
			{
				AddSubgoal(new GoalArriveAsVisitor(base.entity, containerToSleepIn.Value, ownersOfVehicles));
			}
		}
		else
		{
			if (data.ContainsEntity(base.entity.ID) && !base.entity.ContainedBy.HasValue && data is Entity { Contains: HomeContainer contains })
			{
				contains.FixContainmentBug(base.entity.ID);
			}
			if (!data.ContainsEntity(base.entity.ID))
			{
				AddSubgoal(new GoalMoveToPosition(base.entity, data.AccessPoint.Value, ownersOfVehicles)
				{
					IsFinalDestination = true
				});
				AddSubgoal(new GoalEnter(base.entity, data.EntityID));
			}
		}
		AddSubgoal(new GoalDoSleep(base.entity, (EvaluateSleep)GoalEvaluator));
	}

	protected override bool ArePreconditionsOK()
	{
		if (containerToSleepIn.HasValue && EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(containerToSleepIn.Value, out var _)))
		{
			return false;
		}
		return true;
	}

	public double ScoreGoal()
	{
		double score = 0.0;
		if (((EvaluateSleep)GoalEvaluator).ScoreSleep(null, null, null, expeditionToSleepAt, containerToSleepIn, groundLocationToSleepOn, groundSleepArea, null, ref score) == GoalEvaluator.CalculateResult.Done)
		{
			return score;
		}
		return GetCurrentGoalScore();
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (entityIntelligence.IsIndependent())
		{
			Expedition currentExpedition = entityIntelligence.CurrentExpedition;
			if (currentExpedition.GetNumberOfSleepingIndependents() > currentExpedition.NoOfIndependentMembersAllowedToSleep() && entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
			{
				Message msg = new Message(Message.MessageTypes.StopGoalSleep);
				entity.SendMessage(msg);
			}
		}
		if (base.Status == Status.Active)
		{
			if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
			{
				base.Status = ProcessSubgoals(elapsed);
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
	}

	public override void Deactivate()
	{
		if (expeditionToSleepAt.HasValue)
		{
			Expedition.FindByID(expeditionToSleepAt.Value)?.GatheringSite.RemoveVisitor(entity.EntityID);
		}
		if (containerToSleepIn.HasValue)
		{
			entityIntelligence.GetKnownData(containerToSleepIn.Value, out var data);
			if (data != null && data.GatheringSite != null)
			{
				data.GatheringSite.RemoveVisitor(entity.EntityID);
			}
		}
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public override bool HandleMessage(Message message)
	{
		Message.MessageTypes messageType = message.MessageType;
		if (messageType == Message.MessageTypes.StopGoalSleep)
		{
			base.Status = Status.Completed;
			return true;
		}
		base.HandleMessage(message);
		return false;
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
		containerToSleepIn = sn.DoEntityIDNullable(containerToSleepIn);
		expeditionToSleepAt = sn.DoEnumNullable(expeditionToSleepAt);
		groundLocationToSleepOn = sn.DoWorldLocationNullable(groundLocationToSleepOn);
		groundSleepArea = sn.DoEnumNullable(groundSleepArea);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}
}
