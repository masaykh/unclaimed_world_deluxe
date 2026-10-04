using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

/// <summary>
/// Performs an entity's reactions to collisions separately from its movement. The active behavior can vary by locomotion mode.
/// </summary>
public class CollisionResponder : ISnapshot
{
	private enum ActiveResponder
	{
		Agent,
		Ballistic
	}

	public Locomotor Parent;

	/// <summary>
	/// Only one collision responder is active at a time.
	/// </summary>
	private ICollisionResponder activeResponder;

	/// <summary>
	/// Simulates a walking agent.
	/// </summary>
	public AgentCollisionResponder AgentCollisionResponder;

	/// <summary>
	/// Simulates an object in flight.
	/// </summary>
	private BallisticCollisionResponder BallisticCollisionResponder;

	private List<Collidable<Entity>> collidees = new List<Collidable<Entity>>();

	public bool WaitsToGiveRoomToOtherAgent;

	private ActiveResponder snapshotActiveResponder;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public CollisionResponder()
	{
	}

	public CollisionResponder(Locomotor parent)
	{
		Parent = parent;
		CollisionResponderType collisionResponderType = parent.Parent.EntityType.LocomotorType.CollisionResponderType;
		if (collisionResponderType.AgentCollisionResponderType != null)
		{
			AgentCollisionResponder = new AgentCollisionResponder(this);
		}
		if (collisionResponderType.BallisticResponderType != null)
		{
			BallisticCollisionResponder = new BallisticCollisionResponder(this);
		}
	}

	public void ReactToCollisions()
	{
		if (activeResponder == null)
		{
			return;
		}
		collidees.Clear();
		activeResponder.BeginCollisionHandling();
		The.CollisionManager.GetCollidablesIntersectingBounds(Parent.Parent.Collidable.Bounds, ref collidees);
		Parent.SetIsColliding(aValue: false);
		if (collidees.Count <= 1)
		{
			return;
		}
		Entity entity = null;
		float overlap = 0f;
		foreach (Collidable<Entity> collidee in collidees)
		{
			if (collidee.Parent == Parent.Parent || collidee.Parent == null)
			{
				continue;
			}
			entity = collidee.Parent;
			if (entity.Collidable != null && entity.Collidable.Enabled)
			{
				Vector2 ctr = entity.Collidable.Center;
				if (Parent.Parent.Collidable.TestCollision(entity.Collidable, out overlap, out ctr))
				{
					Parent.SetIsColliding(aValue: true);
					activeResponder.HandleSingleCollision(collidee);
				}
			}
		}
		if (Parent.IsColliding && activeResponder != null)
		{
			activeResponder.EndCollisionHandling(collidees);
		}
	}

	public void SetCollisionResponse(Locomotor.Mode CurrentMoveMode)
	{
		switch (CurrentMoveMode)
		{
		case Locomotor.Mode.Legged:
			activeResponder = AgentCollisionResponder;
			break;
		case Locomotor.Mode.Ballistic:
			activeResponder = BallisticCollisionResponder;
			break;
		case Locomotor.Mode.None:
			activeResponder = null;
			break;
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		WaitsToGiveRoomToOtherAgent = sn.DoBool(WaitsToGiveRoomToOtherAgent);
		AgentCollisionResponder = (AgentCollisionResponder)sn.DoISnapshot(AgentCollisionResponder);
		BallisticCollisionResponder = (BallisticCollisionResponder)sn.DoISnapshot(BallisticCollisionResponder);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (activeResponder is AgentCollisionResponder)
			{
				snapshotActiveResponder = ActiveResponder.Agent;
			}
			else
			{
				snapshotActiveResponder = ActiveResponder.Ballistic;
			}
		}
		snapshotActiveResponder = sn.DoEnum(snapshotActiveResponder);
		sn.Ignore(Parent);
		sn.Ignore(collidees);
		sn.Ignore(activeResponder);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (AgentCollisionResponder != null)
		{
			AgentCollisionResponder.Parent = this;
			AgentCollisionResponder.LoadPostProcess(sn);
		}
		if (BallisticCollisionResponder != null)
		{
			BallisticCollisionResponder.LoadPostProcess(sn);
		}
		switch (snapshotActiveResponder)
		{
		case ActiveResponder.Agent:
			activeResponder = AgentCollisionResponder;
			break;
		case ActiveResponder.Ballistic:
			activeResponder = BallisticCollisionResponder;
			break;
		default:
			activeResponder = AgentCollisionResponder;
			break;
		}
	}
}
