using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalTurnToFace : Goal
{
	private Vector2 pointToFace;

	private EntityID? entityToFace;

	private bool setCenterOfAttentionToTurnTarget;

	private float interestLevelToSet;

	private bool turnCompletely;

	private float allowedRotationMargin;

	public static float TurnSpeedWhenTurningInPlace = 2.8f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalTurnToFace(Entity owner, Vector2? pointToFace, EntityID? entityToFace = null, bool turnCompletely = true, bool setCenterOfAttentionToTurnTarget = true, float? interestLevel = null)
		: base(owner)
	{
		if (pointToFace.HasValue)
		{
			this.pointToFace = pointToFace.Value;
		}
		this.entityToFace = entityToFace;
		this.turnCompletely = turnCompletely;
		if (!interestLevel.HasValue)
		{
			interestLevelToSet = GameData.Instance.Constants.InterestLevelForTurnToFace;
		}
		else
		{
			interestLevelToSet = interestLevel.Value;
		}
		this.setCenterOfAttentionToTurnTarget = setCenterOfAttentionToTurnTarget;
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public GoalTurnToFace()
	{
	}

	protected override void Activate()
	{
		if (entityToFace.HasValue)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToFace.Value, out var data)))
			{
				return;
			}
			pointToFace = data.PlaySiteLocation.ToVector2();
		}
		if (entity.EntityType.LocomotorType == null || Common.IsZero(entity.EntityType.LocomotorType.MaxAngularSpeed))
		{
			base.Status = Status.Completed;
			SetCenterOfAttention();
			return;
		}
		_ = entity.PlaySiteLocation;
		if (turnCompletely)
		{
			allowedRotationMargin = 0.05f;
		}
		else
		{
			allowedRotationMargin = MathHelper.Lerp(0.2f, 0.75f, (float)The.Sim.GameplayRandomGenerator.NextDouble("GoalTurnToFace"));
		}
		if (IsFacing(entity, pointToFace, out var _, out var _, allowedRotationMargin))
		{
			base.Status = Status.Completed;
			return;
		}
		base.Status = Status.Active;
		SetCenterOfAttention();
		GoalTraverseEdgeBetweenWaypoints.SelectMoveSpeed(entity);
	}

	private void SetCenterOfAttention()
	{
		if (setCenterOfAttentionToTurnTarget)
		{
			if (entityToFace.HasValue)
			{
				entityIntelligence.SetNewCenterOfAttention(entityToFace.Value, null, interestLevelToSet);
			}
			else
			{
				entityIntelligence.SetNewCenterOfAttention(null, pointToFace.ToVector3(), interestLevelToSet);
			}
		}
	}

	public static float FindClosestCorner(Entity entity, Vector3 position, Vector2 faceThis, float currentRotation, out int cornerNo)
	{
		cornerNo = 0;
		float result = 0f;
		if (entity.EntityType.LocomotorType.FourSidedSymmetry)
		{
			float num = 10000f;
			float num2 = currentRotation;
			float rotationToFace = ComputeDesiredAngle(position, faceThis);
			for (int i = 0; i < 4; i++)
			{
				float smallestAngleDistance = GetSmallestAngleDistance(rotationToFace, num2);
				if (smallestAngleDistance < num - 0.01f)
				{
					num = smallestAngleDistance;
					result = num2;
					cornerNo = i;
				}
				num2 += (float)Math.PI / 2f;
				num2 = Common.WrapAngleBetweenZeroAndTwoPi(num2);
			}
		}
		else
		{
			result = currentRotation;
		}
		return result;
	}

	public static bool IsOneCornerFacing(Entity entity, Vector3 location, Vector2 pointToFace, float currentRotation, out float rotationOfClosestCorner, out int cornerNo, float allowedRotationMargin = 0.05f)
	{
		rotationOfClosestCorner = FindClosestCorner(entity, location, pointToFace, currentRotation, out cornerNo);
		return IsFacing(location, pointToFace, rotationOfClosestCorner, allowedRotationMargin);
	}

	public static bool IsFacing(Entity entity, Vector2 pointToFace, out float rotationOfClosestCorner, out int cornerNo, float allowedRotationMargin = 0.05f, bool performSymmetricFlip = true)
	{
		Vector3 playSiteLocation = entity.PlaySiteLocation;
		if (entity.EntityType.LocomotorType.RotatorType != null)
		{
			rotationOfClosestCorner = entity.Locomotor.Rotator.AbsoluteRotation;
			cornerNo = 0;
			return IsFacing(playSiteLocation, pointToFace, entity.Locomotor.Rotator.AbsoluteRotation, allowedRotationMargin);
		}
		if (entity.EntityType.LocomotorType.FourSidedSymmetry)
		{
			bool result = IsOneCornerFacing(entity, playSiteLocation, pointToFace, entity.Rotation, out rotationOfClosestCorner, out cornerNo);
			if (performSymmetricFlip && cornerNo != 0)
			{
				entity.Locomotor.FlipFourSidedSymmetryCreature(rotationOfClosestCorner);
				rotationOfClosestCorner = entity.Rotation;
				cornerNo = 0;
			}
			return result;
		}
		rotationOfClosestCorner = entity.Rotation;
		cornerNo = 0;
		return IsFacing(playSiteLocation, pointToFace, entity.Rotation, allowedRotationMargin);
	}

	private static bool IsFacing(Vector3 location, Vector2 faceThis, float currentRotation, float allowedRotationMargin)
	{
		return GetSmallestAngleDistance(ComputeDesiredAngle(location, faceThis), currentRotation) < allowedRotationMargin;
	}

	public static float ComputeModelRotationFromCornerRotation(int cornerNo, float cornerRotation)
	{
		if (cornerNo == 0)
		{
			return cornerRotation;
		}
		return Common.WrapAngleBetweenZeroAndTwoPi(cornerRotation - (float)cornerNo * ((float)Math.PI / 2f));
	}

	public static float GetSmallestAngleDistance(float rotationToFace, float currentRotation)
	{
		float num = Math.Abs(rotationToFace - currentRotation);
		return Math.Min(num, (float)Math.PI * 2f - num);
	}

	public static float GetAngleDistance(float rotationToFace, float currentRotation)
	{
		return Math.Abs(rotationToFace - currentRotation);
	}

	private static float ComputeDesiredAngle(Vector3 position, Vector2 faceThis)
	{
		float num = faceThis.X - position.X;
		return Common.WrapAngleBetweenZeroAndTwoPi((float)Math.Atan2(faceThis.Y - position.Y, num));
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		Vector3 playSiteLocation = entity.PlaySiteLocation;
		if (IsFacing(entity, pointToFace, out var rotationOfClosestCorner, out var _, allowedRotationMargin))
		{
			base.Status = Status.Completed;
			return;
		}
		float num = Math.Min(TurnSpeedWhenTurningInPlace, entity.EntityType.LocomotorType.MaxAngularSpeed);
		float headingDifference;
		float num2 = GoalFlyToPosition.TurnToFace(playSiteLocation, pointToFace, rotationOfClosestCorner, (float)((double)num * elapsed.ElapsedGameTime.TotalSeconds), out headingDifference);
		if (entity.EntityType.LocomotorType.RotatorType == null)
		{
			entity.SetRotationAndDir(num2);
		}
		else
		{
			entity.Locomotor.Rotator.AbsoluteRotation = num2;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (entity.EntityType.LocomotorType.RotatorType != null)
		{
			entity.Locomotor.Rotator.DoneRotating();
		}
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
		pointToFace = sn.DoVector2(pointToFace);
		entityToFace = sn.DoEntityIDNullable(entityToFace);
		setCenterOfAttentionToTurnTarget = sn.DoBool(setCenterOfAttentionToTurnTarget);
		interestLevelToSet = sn.DoFloat(interestLevelToSet);
		turnCompletely = sn.DoBool(turnCompletely);
		allowedRotationMargin = sn.DoFloat(allowedRotationMargin);
		return this;
	}
}
