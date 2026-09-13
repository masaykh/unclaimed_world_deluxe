using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalWander : CompositeGoal, ITopLevelGoal
{
	private Rectangle stayInside;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalWander(Entity owner, Rectangle stayInside)
		: base(owner)
	{
		this.stayInside = stayInside;
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

	public GoalWander()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkSlowly;
		GoalMoveToPosition goal = new GoalMoveToPosition(entity, MapManager.TileToWorldPos(new Point(The.Sim.GameplayRandomGenerator.Next(stayInside.Left, stayInside.Right, "GoalWander"), The.Sim.GameplayRandomGenerator.Next(stayInside.Top, stayInside.Bottom, "GoalWander"))), null, GoalMoveToPosition.VehicleUse.NoVehicle);
		AddSubgoal(goal);
	}

	public override void Deactivate()
	{
		entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				base.Status = Status.Failed;
				return true;
			}
			return false;
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		ReactivateIfFailed();
	}
}
