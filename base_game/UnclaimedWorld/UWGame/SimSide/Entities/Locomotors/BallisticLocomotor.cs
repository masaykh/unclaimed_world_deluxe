using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class BallisticLocomotor : ISnapshot
{
	private Vector3 direction;

	public Locomotor Parent;

	public EntityID? LaunchedByEntity;

	public Allegiance LaunchedByAllegiance;

	public AttackType AttackType;

	public OwnerID? OwnerOfCarcass;

	public AttackJob AttackJob;

	private Goal.MovementSpeeds currentMovementSpeedType = Goal.MovementSpeeds.Normal;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private double gravity = 10.0;

	private Vector3 velocity;

	public Goal.MovementSpeeds CurrentMovementSpeedType
	{
		get
		{
			return currentMovementSpeedType;
		}
		set
		{
			if (currentMovementSpeedType != value)
			{
				currentMovementSpeedType = value;
				Parent.CurrentMaximumSpeedIsDirty = true;
				Parent.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
				Parent.CurrentMaximumSpeedForEvaluatorIsDirty = true;
			}
		}
	}

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		gravity = sn.DoDouble(gravity);
		LaunchedByEntity = sn.DoEnumNullable(LaunchedByEntity);
		velocity = sn.DoVector3(velocity);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public BallisticLocomotor()
	{
	}

	public BallisticLocomotor(Locomotor parent)
	{
		Parent = parent;
	}

	public void Launch(Vector3 from, Vector3 velocity)
	{
		this.velocity = velocity;
		float rotationAndDir = Common.VectorToAngle(velocity.ToVector2());
		Parent.Parent.SetRotationAndDir(rotationAndDir);
		Parent.Parent.Location = from;
	}

	public void StartMoving(Vector3 target, float speed, float upwardsSpeed, EntityID? launchedByEntity, Allegiance launchedByAllegiance, AttackType attackType, OwnerID? ownerOfCarcass, AttackJob job)
	{
		LaunchedByEntity = launchedByEntity;
		LaunchedByAllegiance = launchedByAllegiance;
		AttackType = attackType;
		OwnerOfCarcass = ownerOfCarcass;
		AttackJob = job;
		direction = target - Parent.Parent.Location.Value;
		direction.Normalize();
		Vector3 value = Parent.Parent.Location.Value;
		value.Z = 20f;
		Parent.Parent.Location = value;
		velocity = speed * direction;
		velocity.Z = upwardsSpeed;
		float rotationAndDir = Common.VectorToAngle(velocity.ToVector2());
		Parent.Parent.SetRotationAndDir(rotationAndDir);
		Parent.Parent.Renderable.SetToParentLocation();
		if (Parent.Parent.Collidable == null)
		{
			Parent.Parent.Collidable = The.CollisionManager.AddCollidable(Parent.Parent, Parent.Parent.PlaySiteLocation.ToVector2(), new Vector2(2f, 2f));
		}
	}

	public void MoveTowardsLocation(GameTime elapsed)
	{
		double totalSeconds = elapsed.ElapsedGameTime.TotalSeconds;
		velocity.Z = (float)((double)velocity.Z - totalSeconds * gravity);
		Vector3 playSiteLocation = Parent.Parent.PlaySiteLocation;
		playSiteLocation += velocity * (float)totalSeconds;
		bool flag = false;
		if (playSiteLocation.Z <= 0f)
		{
			playSiteLocation.Z = 0f;
			flag = true;
		}
		Parent.Parent.Location = playSiteLocation;
		if (flag)
		{
			StopMoving();
		}
	}

	public void StopMoving()
	{
		Parent.CurrentMoveMode = Locomotor.Mode.None;
		Parent.OnDestinationReached();
	}
}
