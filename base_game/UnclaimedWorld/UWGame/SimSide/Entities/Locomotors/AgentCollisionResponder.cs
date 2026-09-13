using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class AgentCollisionResponder : ICollisionResponder, ISnapshot
{
	public CollisionResponder Parent;

	private Entity parentEntity;

	private Vector2 pushDirection = Vector2.Zero;

	public List<EntityID> LatestResolvedMovingCollisions = new List<EntityID>();

	private Dictionary<EntityID, int> dislodgeCounters = new Dictionary<EntityID, int>();

	private Entity otherEntity;

	private EntityID? snapshotOtherEntity;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public AgentCollisionResponder(CollisionResponder parent)
	{
		Parent = parent;
		parentEntity = Parent.Parent.Parent;
	}

	public AgentCollisionResponder()
	{
	}

	public void BeginCollisionHandling()
	{
		pushDirection = Vector2.Zero;
		LatestResolvedMovingCollisions.Clear();
		otherEntity = null;
	}

	public void HandleSingleCollision(Collidable<Entity> collidee)
	{
		otherEntity = collidee.Parent;
		if (otherEntity.EntityType.IntelligenceType != null && (otherEntity.EntityType.IntelligenceType == null || otherEntity.Intelligence.Allegiance == parentEntity.Intelligence.Allegiance) && !WeAreStaticAndNotReadyToDislodge(otherEntity))
		{
			pushDirection += GetCollisionOffset(Parent.Parent.Parent.Collidable.Center - otherEntity.Collidable.Center);
			if (otherEntity.Locomotor != null && otherEntity.Intelligence != null)
			{
				ShouldSomeoneWait(otherEntity);
			}
			if (otherEntity.Locomotor != null && otherEntity.Locomotor.CollisionResponder != null && !CheckIfCollisionAlreadySolved(otherEntity) && Parent.Parent.IsMoving())
			{
				pushDirection += SidestepOtherMovingEntity(otherEntity);
			}
		}
	}

	private bool CheckIfCollisionAlreadySolved(Entity other)
	{
		for (int i = 0; i < other.Locomotor.CollisionResponder.AgentCollisionResponder.LatestResolvedMovingCollisions.Count; i++)
		{
			if (other.Locomotor.CollisionResponder.AgentCollisionResponder.LatestResolvedMovingCollisions[i] == parentEntity.EntityID)
			{
				return true;
			}
		}
		LatestResolvedMovingCollisions.Add(other.EntityID);
		return false;
	}

	private void ShouldSomeoneWait(Entity other)
	{
		bool num = other.Intelligence.IsIdle();
		bool flag = IsEntityNearMyDestination(other);
		bool flag2 = IsEntityHavingSameDestionationAsMe(other);
		if (!num && flag2 && flag)
		{
			Entity furthestToDestination = GetFurthestToDestination(other);
			if (!furthestToDestination.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent)
			{
				furthestToDestination.Intelligence.Brain.SendMessage(new Message(Message.MessageTypes.WaitAndMakeRoom));
			}
		}
	}

	private Entity GetFurthestToDestination(Entity other)
	{
		float num = Common.DistanceOctile(other.PlaySiteLocation, Parent.Parent.CurrentMoveTarget);
		if (Common.DistanceOctile(parentEntity.PlaySiteLocation, Parent.Parent.CurrentMoveTarget) > num)
		{
			return parentEntity;
		}
		return other;
	}

	private bool WeAreStaticAndNotReadyToDislodge(Entity collidee)
	{
		int num = 50;
		if (!dislodgeCounters.TryGetValue(collidee.ID, out var value))
		{
			value = num;
			dislodgeCounters.Add(collidee.ID, value);
		}
		if (!Parent.Parent.IsMoving() && collidee.Locomotor != null && collidee.Locomotor.IsMoving())
		{
			value--;
			value = Common.ClampBottom(value, 0);
			dislodgeCounters[collidee.ID] = value;
			if (value > 0)
			{
				return true;
			}
		}
		else if (value < num)
		{
			value++;
			dislodgeCounters[collidee.ID] = value;
		}
		else if (value >= num)
		{
			dislodgeCounters.Remove(collidee.ID);
		}
		return false;
	}

	private bool IsEntityHavingSameDestionationAsMe(Entity other)
	{
		float x = other.Locomotor.CurrentMoveTarget.X;
		float y = other.Locomotor.CurrentMoveTarget.Y;
		float x2 = Parent.Parent.CurrentMoveTarget.X;
		float y2 = Parent.Parent.CurrentMoveTarget.Y;
		if (Common.IsEqual(x, x2))
		{
			return Common.IsEqual(y, y2);
		}
		return false;
	}

	private bool IsEntityNearMyDestination(Entity other)
	{
		if (Common.IsEqual(other.PlaySiteLocation.X, Parent.Parent.CurrentMoveTarget.X, 10f))
		{
			return Common.IsEqual(other.PlaySiteLocation.Y, Parent.Parent.CurrentMoveTarget.Y, 10f);
		}
		return false;
	}

	private bool GetIsPositionLeftOfParent(Vector2 position)
	{
		Vector2 vector = Parent.Parent.Parent.PlaySiteLocation.ToVector2();
		Vector2 vector2 = Parent.Parent.direction.ToVector2();
		return (vector2.X - vector.X) * (position.Y - vector.Y) - (vector2.Y - position.Y) * (position.X - vector.X) > 0f;
	}

	private Vector2 GetCollisionOffset(Vector2 offsetBetweenOwnerAndOther)
	{
		Vector2 vector = offsetBetweenOwnerAndOther;
		if (vector == Vector2.Zero)
		{
			vector.X += (float)The.Sim.GameplayRandomGenerator.NextDouble("AgentCollisionResponder") - 0.5f;
			vector.Y += (float)The.Sim.GameplayRandomGenerator.NextDouble("AgentCollisionResponder") - 0.5f;
			if (vector == Vector2.Zero)
			{
				vector.Y = 1f;
			}
		}
		vector.Normalize();
		return vector;
	}

	private Vector2 SidestepOtherMovingEntity(Entity other)
	{
		Vector2 result = Vector2.Zero;
		Vector3 direction = other.Locomotor.direction;
		bool num = Common.IsEqual(Vector3.Dot(Parent.Parent.direction, direction), -1f, 0.1f) && other.IsMoving == true;
		bool flag = other.IsMoving == false;
		if (num || flag)
		{
			result = ((!GetIsPositionLeftOfParent(other.PlaySiteLocation.ToVector2())) ? new Vector2(0f - Parent.Parent.direction.Y, Parent.Parent.direction.X) : new Vector2(Parent.Parent.direction.Y, 0f - Parent.Parent.direction.X));
		}
		return result;
	}

	public void SetInterestInCollidedEntity(List<Collidable<Entity>> collidees)
	{
		if (collidees.Count <= 1)
		{
			return;
		}
		Entity entity = null;
		float closestAgentDistance = -1f;
		Entity entity2 = null;
		foreach (Collidable<Entity> collidee in collidees)
		{
			_ = collidee;
			if (entity != Parent.Parent.Parent && entity != null && CheckIfEntityIsClosestCollidingEntitySoFar(entity, closestAgentDistance))
			{
				entity2 = entity;
			}
		}
		if (entity2 != null)
		{
			Parent.Parent.Parent.SetInterestInCollidedEntity(entity2);
		}
	}

	private bool CheckIfEntityIsClosestCollidingEntitySoFar(Entity otherEntity, float closestAgentDistance)
	{
		float num = Common.DistanceOctile(Parent.Parent.Parent.Collidable.Center, otherEntity.Collidable.Center);
		if (num < closestAgentDistance || closestAgentDistance == -1f)
		{
			closestAgentDistance = num;
			return true;
		}
		return false;
	}

	public void EndCollisionHandling(List<Collidable<Entity>> collidees)
	{
		if (pushDirection != Vector2.Zero)
		{
			Parent.Parent.ApplyPushVector(otherEntity, pushDirection);
			parentEntity.SendMessage(new Message(Message.MessageTypes.WakeUpCombatAlert));
		}
		SetInterestInCollidedEntity(collidees);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		LatestResolvedMovingCollisions = sn.DoList(LatestResolvedMovingCollisions);
		pushDirection = sn.DoVector2(pushDirection);
		snapshotOtherEntity = sn.SnapshotID<Entity, EntityID>(otherEntity);
		dislodgeCounters = sn.DoDictionary(dislodgeCounters);
		sn.Ignore(Parent);
		sn.Ignore(parentEntity);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parentEntity = Parent.Parent.Parent;
		otherEntity = Entity.FindByID(snapshotOtherEntity);
	}
}
