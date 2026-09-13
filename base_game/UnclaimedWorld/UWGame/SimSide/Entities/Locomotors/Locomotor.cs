using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class Locomotor : Component
{
	public enum Mode
	{
		None,
		Legged,
		Ballistic
	}

	public enum State
	{
		Idle,
		Moving,
		Stationery,
		Arrived
	}

	public float BoundingRadius;

	private Mode currentMoveMode;

	public LeggedLocomotor LeggedLocomotor;

	public BallisticLocomotor BallisticLocomotor;

	public CollisionResponder CollisionResponder;

	public Stance Stance;

	public Rotator Rotator;

	private float? previousRotation;

	private float moveSpeed;

	private Vector3? previousLocation;

	public const float CommonLowestHaulingSpeed = 8f;

	public const float CommonCarryLimit = 1f;

	public Vector3 CurrentMoveTarget;

	private float moveAbility = 1f;

	public float SymmetricCreatureRealRotation;

	public bool RotateSlowly;

	public EntityID targetEntity;

	public Vector3 direction = new Vector3(0f);

	public float speed;

	public Vector3? targetLocation;

	public float optimumSpeed;

	public State state;

	private bool isColliding;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private float terrainModifier = 1f;

	public float? MaximumSpeedDebugOnly;

	private float currentMaximumSpeed = 0.5f;

	public bool CurrentMaximumSpeedIsDirty = true;

	private float currentMaximumSpeedNotAffectedByTerrain = 0.5f;

	public bool CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;

	private float currentMaximumSpeedForEvaluator = 0.5f;

	public bool CurrentMaximumSpeedForEvaluatorIsDirty = true;

	public bool IsMovingOrRotating;

	public static float resistance = 1.3f;

	public Mode CurrentMoveMode
	{
		get
		{
			return currentMoveMode;
		}
		set
		{
			currentMoveMode = value;
			if (CollisionResponder != null)
			{
				CollisionResponder.SetCollisionResponse(value);
			}
		}
	}

	public float MoveSpeed
	{
		get
		{
			return moveSpeed;
		}
		set
		{
			moveSpeed = value;
		}
	}

	public float MoveAbility => moveAbility;

	public bool IsColliding => isColliding;

	public float CurrentMaximumSpeed
	{
		get
		{
			if (CurrentMaximumSpeedIsDirty)
			{
				RecomputeCurrentMaximumSpeed();
			}
			return currentMaximumSpeed;
		}
	}

	public float CurrentMaximumSpeedNoTerrain
	{
		get
		{
			if (CurrentMaximumSpeedNotAffectedByTerrainIsDirty)
			{
				RecomputeCurrentMaximumSpeedNoTerrain();
			}
			return currentMaximumSpeedNotAffectedByTerrain;
		}
	}

	public float CurrentMaximumSpeedForEvaluator
	{
		get
		{
			if (CurrentMaximumSpeedForEvaluatorIsDirty)
			{
				RecomputeCurrentMaximumSpeedForEvaluator();
			}
			return currentMaximumSpeedForEvaluator;
		}
	}

	public override double? GetUpdateInterval()
	{
		if (!Parent.IsDead && Parent.IsCompleted() && Parent.IsOnPlaySite())
		{
			return 0.0;
		}
		return null;
	}

	public void SetIsColliding(bool aValue)
	{
		isColliding = aValue;
	}

	public void SetState(State newState)
	{
		state = newState;
	}

	public Locomotor(Entity parent)
		: base(parent)
	{
		if (Parent != null)
		{
			SetState(State.Idle);
		}
		if (parent.EntityType.LocomotorType.LeggedLocomotorType != null)
		{
			LeggedLocomotor = new LeggedLocomotor(this);
		}
		if (parent.EntityType.LocomotorType.BallisticLocomotorType != null)
		{
			BallisticLocomotor = new BallisticLocomotor(this);
		}
		if (parent.EntityType.LocomotorType.CollisionResponderType != null)
		{
			CollisionResponder = new CollisionResponder(this);
		}
		if (parent.EntityType.LocomotorType.RotatorType != null)
		{
			Rotator = new Rotator(this);
		}
		if (parent.EntityType.LocomotorType.StancesType != null)
		{
			Stance = new Stance(this);
		}
		SetState(State.Moving);
		targetLocation = new Vector3(0f);
		if (LeggedLocomotor != null)
		{
			CurrentMoveMode = Mode.Legged;
		}
		else
		{
			CurrentMoveMode = Mode.None;
		}
	}

	public Locomotor()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		BallisticLocomotor = (BallisticLocomotor)sn.DoISnapshot(BallisticLocomotor);
		BoundingRadius = sn.DoFloat(BoundingRadius);
		CollisionResponder = (CollisionResponder)sn.DoISnapshot(CollisionResponder);
		currentMaximumSpeed = sn.DoFloat(currentMaximumSpeed);
		currentMaximumSpeedNotAffectedByTerrain = sn.DoFloat(currentMaximumSpeedNotAffectedByTerrain);
		currentMaximumSpeedForEvaluator = sn.DoFloat(currentMaximumSpeedForEvaluator);
		CurrentMaximumSpeedIsDirty = sn.DoBool(CurrentMaximumSpeedIsDirty);
		CurrentMaximumSpeedNotAffectedByTerrainIsDirty = sn.DoBool(CurrentMaximumSpeedNotAffectedByTerrainIsDirty);
		CurrentMaximumSpeedForEvaluatorIsDirty = sn.DoBool(CurrentMaximumSpeedForEvaluatorIsDirty);
		currentMoveMode = sn.DoEnum(currentMoveMode);
		CurrentMoveTarget = sn.DoVector3(CurrentMoveTarget);
		direction = sn.DoVector3(direction);
		isColliding = sn.DoBool(isColliding);
		IsMovingOrRotating = sn.DoBool(IsMovingOrRotating);
		LeggedLocomotor = (LeggedLocomotor)sn.DoISnapshot(LeggedLocomotor);
		Stance = (Stance)sn.DoISnapshot(Stance);
		MaximumSpeedDebugOnly = sn.DoFloatNullable(MaximumSpeedDebugOnly);
		moveAbility = sn.DoFloat(moveAbility);
		MoveSpeed = sn.DoFloat(MoveSpeed);
		optimumSpeed = sn.DoFloat(optimumSpeed);
		previousLocation = sn.DoVector3Nullable(previousLocation);
		previousRotation = sn.DoFloatNullable(previousRotation);
		RotateSlowly = sn.DoBool(RotateSlowly);
		speed = sn.DoFloat(speed);
		state = sn.DoEnum(state);
		SymmetricCreatureRealRotation = sn.DoFloat(SymmetricCreatureRealRotation);
		targetEntity = sn.DoEntityID(targetEntity);
		targetLocation = sn.DoVector3Nullable(targetLocation);
		terrainModifier = sn.DoFloat(terrainModifier);
		Rotator = (Rotator)sn.DoISnapshot(Rotator);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (LeggedLocomotor != null)
		{
			LeggedLocomotor.Parent = this;
			LeggedLocomotor.LoadPostProcess(sn);
		}
		if (CollisionResponder != null)
		{
			CollisionResponder.Parent = this;
			CollisionResponder.LoadPostProcess(sn);
		}
		if (BallisticLocomotor != null)
		{
			BallisticLocomotor.Parent = this;
			BallisticLocomotor.LoadPostProcess(sn);
		}
		if (Rotator != null)
		{
			Rotator.Parent = this;
			Rotator.LoadPostProcess(sn);
		}
		if (Stance != null)
		{
			Stance.Parent = this;
			Stance.LoadPostProcess(sn);
		}
	}

	public void Initialize(Game game)
	{
	}

	public void OnDestinationReached()
	{
	}

	public void FlipFourSidedSymmetryCreature(float rotation)
	{
		if (Parent.Locomotor == null)
		{
			string name = Parent.Name;
			throw new Exception("no locomotor in " + name);
		}
		SymmetricCreatureRealRotation = Parent.Rotation;
		Parent.SetRotationAndDir(rotation);
	}

	public float CalculateSpeed(float? totalBulkForCalculation = null, bool calculateWithTerrain = true, bool calculateWithBurden = true)
	{
		float num = (calculateWithBurden ? GetTargetSpeed() : Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed);
		float result = num;
		ApplyDisabilityModifier(ref result);
		if (calculateWithBurden)
		{
			ApplyBurdenModifier(ref result, totalBulkForCalculation);
		}
		if (calculateWithTerrain)
		{
			ApplyTerrainModifier(ref result);
		}
		return result;
	}

	private void RecomputeCurrentMaximumSpeedForEvaluator()
	{
		currentMaximumSpeedForEvaluator = CalculateSpeed(null, calculateWithTerrain: false, calculateWithBurden: false);
		CurrentMaximumSpeedForEvaluatorIsDirty = false;
	}

	private void RecomputeCurrentMaximumSpeedNoTerrain()
	{
		currentMaximumSpeedNotAffectedByTerrain = CalculateSpeed(null, calculateWithTerrain: false);
		CurrentMaximumSpeedNotAffectedByTerrainIsDirty = false;
	}

	private void RecomputeCurrentMaximumSpeed()
	{
		currentMaximumSpeed = CalculateSpeed();
		CurrentMaximumSpeedIsDirty = false;
	}

	public void ImpairMovement(float aMovementPartToRemove)
	{
		moveAbility -= aMovementPartToRemove;
		if (moveAbility < 0f)
		{
			moveAbility = 0f;
		}
		CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
		CurrentMaximumSpeedForEvaluatorIsDirty = true;
		CurrentMaximumSpeedIsDirty = true;
	}

	public void ToggleImmobilize()
	{
		if (!Common.IsZero(moveAbility))
		{
			moveAbility = 0f;
		}
		else
		{
			moveAbility = 1f;
		}
		CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
		CurrentMaximumSpeedForEvaluatorIsDirty = true;
		CurrentMaximumSpeedIsDirty = true;
	}

	public float GetTargetSpeed()
	{
		float num = 0f;
		if (LeggedLocomotor == null)
		{
			return 1f;
		}
		return LeggedLocomotor.TargetSpeed switch
		{
			Goal.MovementSpeeds.WalkSlowly => Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkSlowSpeed, 
			Goal.MovementSpeeds.Normal => Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed, 
			Goal.MovementSpeeds.WalkFast => Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed, 
			Goal.MovementSpeeds.Run => Parent.EntityType.LocomotorType.LeggedLocomotorType.RunSpeed, 
			Goal.MovementSpeeds.Haul => Parent.EntityType.LocomotorType.LeggedLocomotorType.HaulSpeed, 
			_ => Parent.EntityType.LocomotorType.LeggedLocomotorType.WalkNormalSpeed, 
		};
	}

	private void ApplyDisabilityModifier(ref float speed)
	{
		speed *= moveAbility;
	}

	private void ApplyBurdenModifier(ref float speed, float? totalBulkForCalculation = null)
	{
		ItemStorage itemStorage = ((Parent.AgentStorage != null) ? Parent.AgentStorage.ItemStorage : null);
		float num = 0f;
		if (totalBulkForCalculation.HasValue)
		{
			num = totalBulkForCalculation.Value;
		}
		else if (itemStorage != null)
		{
			num = itemStorage.TotalStored;
		}
		if (Parent.Vehicle == null)
		{
			if (itemStorage != null && Parent.AgentStorage.GetCurrentBurdenState() == AgentStorage.BurdenState.HaulHeavy)
			{
				float num2 = MathHelper.Lerp(1f, 0.6f, num / itemStorage.TotalCapacity);
				speed = num2 * speed;
			}
		}
		else if (itemStorage != null)
		{
			speed *= 2f / 3f + (itemStorage.TotalCapacity - num) / itemStorage.TotalCapacity / 3f;
		}
	}

	public void SetTerrainModifier(Point sampleSubtile)
	{
		float num = The.Map.GetRoughness(sampleSubtile) * (1f - Parent.EntityType.LocomotorType.LeggedLocomotorType.TerrainNegateFactor);
		terrainModifier = 1f - num;
		CurrentMaximumSpeedIsDirty = true;
	}

	private void ApplyTerrainModifier(ref float speed)
	{
		if (speed > GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect)
		{
			float num = terrainModifier * speed;
			if (num < speed)
			{
				speed = Common.ClampBottom(num, GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect);
			}
		}
	}

	public float GetAcceleration()
	{
		if (LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run)
		{
			return 100f;
		}
		return 60f;
	}

	public bool IsMoving()
	{
		if (MoveSpeed > 0f)
		{
			return true;
		}
		return false;
	}

	public override void UpdatePlaySite(GameTime gameTime)
	{
		if (previousLocation != Parent.Location && MoveSpeed > 0f)
		{
			IsMovingOrRotating = true;
		}
		else if (previousRotation != Parent.Rotation)
		{
			IsMovingOrRotating = true;
		}
		else
		{
			IsMovingOrRotating = false;
		}
		previousLocation = Parent.Location;
		previousRotation = Parent.Rotation;
		Entity entity = Entity.FindByID(targetEntity);
		if (entity != null)
		{
			MoveTowardsEntity(entity, speed);
		}
		else if (targetLocation.HasValue)
		{
			MoveTowardsLocation(gameTime);
		}
	}

	public void ReactToCollisions()
	{
		if (CollisionResponder != null)
		{
			CollisionResponder.ReactToCollisions();
		}
	}

	public void ApplyPushVector(Entity other, Vector2 pushVector)
	{
		pushVector.Normalize();
		float num = ((other.EntityType.LocomotorType != null && other.EntityType.LocomotorType.CollisionResponderType != null) ? GameData.Instance.Constants.CollisionResistanceFromMovers : GameData.Instance.Constants.CollisionResistanceFromNonMovers);
		pushVector *= num;
		Vector3 position = Parent.PlaySiteLocation + pushVector.ToVector3();
		position = The.Map.ClampWorldPosition(position);
		Parent.Location = Parent.ModifyNewLocationToStayOnFreeTerrain(position, pushVector);
	}

	private bool IsEntityIdle(Entity entity)
	{
		return entity.Intelligence.IsIdle();
	}

	public void Launch(Vector3 from, Vector3 velocity)
	{
		if (BallisticLocomotor != null)
		{
			CurrentMoveMode = Mode.Ballistic;
			BallisticLocomotor.Launch(from, velocity);
		}
	}

	public void StartMoving(Mode mode, Vector3 target, float speed, float upwardsSpeed, EntityID? launchedByEntity, Allegiance launchedByAllegiance, AttackType attackType, OwnerID? ownerOfCarcass, AttackJob job)
	{
		CurrentMoveMode = mode;
		if ((uint)mode > 1u && mode == Mode.Ballistic)
		{
			BallisticLocomotor.StartMoving(target, speed, upwardsSpeed, launchedByEntity, launchedByAllegiance, attackType, ownerOfCarcass, job);
		}
	}

	public State MoveTowardsLocation(GameTime elapsed)
	{
		Mode mode = CurrentMoveMode;
		if ((uint)mode > 1u && mode == Mode.Ballistic)
		{
			BallisticLocomotor.MoveTowardsLocation(elapsed);
		}
		return state = State.Idle;
	}

	public State MoveTowardsEntity(Entity target, float optimumSpeed)
	{
		LocomotorType.Appearance appearance = Parent.EntityType.LocomotorType.appearance;
		if (appearance == LocomotorType.Appearance.Thrown)
		{
			return state = State.Moving;
		}
		return state = State.Idle;
	}
}
