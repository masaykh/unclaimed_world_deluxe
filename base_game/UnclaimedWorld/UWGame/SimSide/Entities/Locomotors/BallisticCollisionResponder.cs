using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class BallisticCollisionResponder : ICollisionResponder, ISnapshot
{
	private CollisionResponder parent;

	private Entity parentEntity;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public BallisticCollisionResponder(CollisionResponder parent)
	{
		this.parent = parent;
		parentEntity = this.parent.Parent.Parent;
	}

	public BallisticCollisionResponder()
	{
	}

	public void HandleSingleCollision(Collidable<Entity> collidee)
	{
		Entity entity = collidee.Parent;
		BallisticLocomotor ballisticLocomotor = parent.Parent.BallisticLocomotor;
		if (entity.EntityType.IntelligenceType != null && entity.Intelligence.Allegiance != ballisticLocomotor.LaunchedByAllegiance)
		{
			BodyPart.AttackDirection attackDirection = GoalDoAttack.GetAttackDirection(parentEntity, entity.Location.Value, entity.Rotation);
			BodyPart randomBodyPartToHit = entity.Body.GetRandomBodyPartToHit(attackDirection);
			ballisticLocomotor.AttackType.HitTargets(Entity.FindByID(ballisticLocomotor.LaunchedByEntity), ballisticLocomotor.AttackJob, ballisticLocomotor.OwnerOfCarcass, randomBodyPartToHit.BodyPartID, entity);
			Vector3 value = parentEntity.Location.Value;
			value.Z = 0f;
			parentEntity.Location = value;
			parent.Parent.BallisticLocomotor.StopMoving();
		}
	}

	public void BeginCollisionHandling()
	{
	}

	public void EndCollisionHandling(List<Collidable<Entity>> collidees)
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
