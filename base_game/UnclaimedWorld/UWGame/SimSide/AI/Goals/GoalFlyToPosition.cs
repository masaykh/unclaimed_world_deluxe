using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AI.Goals;

public class GoalFlyToPosition : Goal
{
	public enum PropDirection
	{
		Start,
		Stop
	}

	private bool isAtTarget;

	public Entity Aircraft;

	private Vehicle vehicleComponent;

	private bool hasLoweredLandingGear;

	private Point destination;

	private Vector2 destinationPoint;

	private const float atDestinationLimit = 16f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Point Destination
	{
		get
		{
			return destination;
		}
		set
		{
			destination = value;
			destinationPoint = new Vector2(48f * (0.5f + (float)destination.X), 48f * (0.5f + (float)destination.Y));
		}
	}

	public GoalFlyToPosition(Entity owner, Point destination)
		: base(owner)
	{
		Destination = destination;
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
		isAtTarget = sn.DoBool(isAtTarget);
		sn.DoUnknownObject(Aircraft);
		sn.DoUnknownObject(vehicleComponent);
		hasLoweredLandingGear = sn.DoBool(hasLoweredLandingGear);
		destination = sn.DoPoint(destination);
		destinationPoint = sn.DoVector2(destinationPoint);
		return this;
	}

	public GoalFlyToPosition()
	{
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void Activate()
	{
		if (!entity.GetDrivenVehicle(out Aircraft))
		{
			base.Status = Status.Failed;
			return;
		}
		Aircraft.Find<Vehicle>(out vehicleComponent);
		SelectSteeringType();
		base.Status = Status.Active;
		Aircraft.Renderable.StartAdditionalAnimation("raiselandinggear", Playback.Forwards, StartingPoint.Current, BlendMode.Normal);
		Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", show: false);
		Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", show: false);
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		float num = (float)elapsed.ElapsedGameTime.TotalSeconds;
		if (!isAtTarget)
		{
			float moveSpeed = Aircraft.Locomotor.MoveSpeed;
			float value = 0f;
			float headingDifference = 0f;
			if (vehicleComponent.Aircraft.SteeringType == SteeringType.HardTurn)
			{
				value = FindMaxMoveSpeedForHardTurn(destinationPoint, num, out headingDifference);
			}
			else if (vehicleComponent.Aircraft.SteeringType == SteeringType.Pivot)
			{
				value = FindPivotSpeed(num);
			}
			else if (vehicleComponent.Aircraft.SteeringType == SteeringType.SlowTurn)
			{
				value = FindSlowCircleSpeed(num, out headingDifference);
			}
			double num2 = Vector2.Distance(destinationPoint, new Vector2(Aircraft.Location.Value.X, Aircraft.Location.Value.Y));
			if (num2 > 0.0)
			{
				VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;
				double val = MathHelper.Clamp(value, moveSpeed - vehicleContainerType.MaxAcceleration * num, moveSpeed + vehicleContainerType.MaxAcceleration * num);
				val = Math.Min((num2 + 16.0) / (double)vehicleContainerType.Deceleration, val);
				Aircraft.Locomotor.MoveSpeed = (float)val;
			}
			SetPitch(moveSpeed, num);
			if (Aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
			{
				SetFansAngle(moveSpeed, headingDifference, num);
			}
			entity.Location = Aircraft.Location + new Vector3(Aircraft.FacingNormal.X, Aircraft.FacingNormal.Y, 0f) * Aircraft.Locomotor.MoveSpeed * num;
			if (num2 < 180.0 && !hasLoweredLandingGear)
			{
				hasLoweredLandingGear = true;
				Aircraft.Renderable.StartAdditionalAnimation("raiselandinggear", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);
			}
			if (Vector2.DistanceSquared(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), destinationPoint) < 16f)
			{
				Aircraft.Locomotor.MoveSpeed = 0f;
				isAtTarget = true;
			}
			return;
		}
		if (Aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
		{
			SetLeftDuctAngle(Aircraft, vehicleComponent, 0f, num);
			SetRightDuctAngle(Aircraft, vehicleComponent, 0f, num);
		}
		bool flag = Common.IsEqual(vehicleComponent.Roll, 0f);
		bool flag2 = Common.IsEqual(vehicleComponent.Pitch, 0f);
		if (flag && flag2)
		{
			vehicleComponent.Roll = 0f;
			vehicleComponent.Pitch = 0f;
			base.Status = Status.Completed;
			return;
		}
		if (!flag)
		{
			float value2 = 0f - vehicleComponent.Roll;
			value2 = MathHelper.Clamp(value2, (0f - num) * 0.3f, num * 0.3f);
			vehicleComponent.Roll += value2;
		}
		if (!flag2)
		{
			float value3 = 0f - vehicleComponent.Pitch;
			value3 = MathHelper.Clamp(value3, (0f - num) * 0.3f, num * 0.3f);
			vehicleComponent.Pitch += value3;
		}
	}

	private void SetPitch(float previousMoveSpeed, float elapsedTime)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;
		float num = Aircraft.Locomotor.MoveSpeed - previousMoveSpeed;
		float num2 = 0.5f * Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed;
		float num3 = num / elapsedTime;
		float num4 = ((!(num3 + num2 > 0f)) ? MathHelper.Lerp(0f - vehicleContainerType.Aircraft.MaxPitchInRadians, 0f, (0f - num3) / 50f + num2) : MathHelper.Lerp(0f, vehicleContainerType.Aircraft.MaxPitchInRadians, num3 / 50f + num2));
		float value = num4 - vehicleComponent.Pitch;
		float num5 = elapsedTime * vehicleContainerType.Aircraft.PitchChangeSpeed;
		value = MathHelper.Clamp(value, 0f - num5, num5);
		float pitch = vehicleComponent.Pitch + value;
		vehicleComponent.Pitch = pitch;
	}

	private void SetFansAngle(float previousMoveSpeed, float headingDifference, float elapsedTime)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;
		float num = Aircraft.Locomotor.MoveSpeed - previousMoveSpeed;
		float num2 = 0.5f * Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed;
		float num3 = num / elapsedTime;
		float num4 = ((!(num3 + num2 > 0f)) ? MathHelper.Lerp(-(float)Math.PI / 4f, 0f, (0f - num3) / 50f + num2) : MathHelper.Lerp(0f, (float)Math.PI / 4f, num3 / 50f + num2));
		Common.IsEqual(headingDifference, 0f);
		headingDifference = MathHelper.Clamp(headingDifference, -0.3f, 0.3f);
		SetLeftDuctAngle(Aircraft, vehicleComponent, num4 + headingDifference, elapsedTime);
		SetRightDuctAngle(Aircraft, vehicleComponent, num4 - headingDifference, elapsedTime);
	}

	private static void SetPropellerAngle(Entity aircraft, ref Matrix rotateTransform, string boneName)
	{
		aircraft.Renderable.SetModelBoneRotation(boneName, rotateTransform);
	}

	public static void UpdatePropellers(Entity aircraft, Vehicle vehicleComponent, float elapsedTime, PropDirection direction)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;
		float num = elapsedTime * vehicleComponent.Aircraft.PropellerSpeed;
		float num2 = ((direction == PropDirection.Start) ? 1f : (-1f));
		vehicleComponent.Aircraft.PropellerSpeed = MathHelper.Clamp(vehicleComponent.Aircraft.PropellerSpeed + num2 * elapsedTime * vehicleContainerType.Aircraft.PropellerAcceleration, 0f, vehicleContainerType.Aircraft.MaxPropellerSpeed);
		Matrix rotateTransform = Matrix.CreateFromYawPitchRoll((float)Math.PI / 2f, 0f, num + (float)Math.PI / 2f);
		SetPropellerAngle(aircraft, ref rotateTransform, "propeller_joint_right");
		SetPropellerAngle(aircraft, ref rotateTransform, "propeller_joint_left");
	}

	public static void SetLeftDuctAngle(Entity aircraft, Vehicle vehicleComponent, float rotate, float elapsedTime)
	{
		rotate -= vehicleComponent.Pitch;
		vehicleComponent.Aircraft.LeftDuctFanAngle = SetDuctAngle(aircraft, vehicleComponent, vehicleComponent.Aircraft.LeftDuctFanAngle, rotate, "jet_engine_joint", elapsedTime);
	}

	public static void SetRightDuctAngle(Entity Aircraft, Vehicle vehicleComponent, float rotate, float elapsedTime)
	{
		rotate -= vehicleComponent.Pitch;
		vehicleComponent.Aircraft.RightDuctFanAngle = SetDuctAngle(Aircraft, vehicleComponent, vehicleComponent.Aircraft.RightDuctFanAngle, rotate, "jet_engine_joint_right", elapsedTime);
	}

	private static float SetDuctAngle(Entity aircraft, Vehicle vehicleComponent, float currentAngle, float rotate, string duct, float elapsedTime)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;
		float value = rotate - currentAngle;
		float num = elapsedTime * vehicleContainerType.Aircraft.DuctChangeAngleSpeed;
		value = MathHelper.Clamp(value, 0f - num, num);
		float num2 = currentAngle + value;
		aircraft.Renderable.SetModelBoneRotation(duct, Matrix.CreateFromYawPitchRoll((float)Math.PI / 2f, 0f, num2 + (float)Math.PI / 2f));
		return num2;
	}

	public void SelectSteeringType()
	{
		if (Vector2.Dot(destinationPoint - new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), new Vector2(Aircraft.FacingNormal.X, Aircraft.FacingNormal.Y)) > 0f)
		{
			if (IsWaypointWithinTurningRadiusAtMaxSpeed(destinationPoint))
			{
				vehicleComponent.Aircraft.SteeringType = SteeringType.SlowTurn;
			}
			else
			{
				vehicleComponent.Aircraft.SteeringType = SteeringType.HardTurn;
			}
		}
		else
		{
			vehicleComponent.Aircraft.SteeringType = SteeringType.Pivot;
		}
	}

	private float FindPivotSpeed(float elapsedTime)
	{
		Aircraft.Locomotor.MoveSpeed = 0f;
		float headingDifference;
		float num = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint, Aircraft.Rotation, Aircraft.EntityType.LocomotorType.MaxAngularSpeed * elapsedTime, out headingDifference);
		Aircraft.Rotation = num;
		Aircraft.FacingNormal = new Vector3((float)Math.Cos(num), (float)Math.Sin(num), 0f);
		if (Math.Abs(headingDifference) < 0.2f)
		{
			return ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxAirSpeed;
		}
		return 0f;
	}

	private float FindMaxMoveSpeedForHardTurn(Vector2 waypoint, float elapsedTime, out float headingDifference)
	{
		float result = ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxAirSpeed;
		if (!IsWaypointWithinTurningRadiusAtMaxSpeed(waypoint))
		{
			float num = Vector2.Distance(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), waypoint) / 2f;
			result = Aircraft.EntityType.LocomotorType.MaxAngularSpeed * num;
		}
		float num2 = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint, Aircraft.Rotation, Aircraft.EntityType.LocomotorType.MaxAngularSpeed * elapsedTime, out headingDifference);
		Aircraft.Rotation = num2;
		Aircraft.FacingNormal = new Vector3((float)Math.Cos(num2), (float)Math.Sin(num2), 0f);
		SetRoll(headingDifference, elapsedTime);
		return result;
	}

	private bool IsWaypointWithinTurningRadiusAtMaxSpeed(Vector2 waypoint)
	{
		VehicleContainerType obj = (VehicleContainerType)Aircraft.EntityType.ContainerType;
		_ = obj.Aircraft.MaxAirSpeed;
		float num = obj.Aircraft.MaxAirSpeed / Aircraft.EntityType.LocomotorType.MaxAngularSpeed;
		Vector2 vector = new Vector2(Aircraft.FacingNormal.Y, 0f - Aircraft.FacingNormal.X);
		Vector2 vector2 = Aircraft.PlaySiteLocation.ToVector2();
		return Math.Min(Vector2.Distance(waypoint, vector2 + vector * num), Vector2.Distance(waypoint, vector2 - vector * num)) >= num;
	}

	private float FindSlowCircleSpeed(float elapsedTime, out float headingDifference)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;
		if (!IsWaypointWithinTurningRadiusAtMaxSpeed(destinationPoint))
		{
			headingDifference = 0f;
			vehicleComponent.Aircraft.SteeringType = SteeringType.HardTurn;
			return vehicleContainerType.Aircraft.MaxAirSpeed;
		}
		float num = FindSlowCircleRadius(destinationPoint);
		float num2 = vehicleContainerType.Aircraft.MaxAirSpeed / num;
		float num3 = TurnToFace(Aircraft.PlaySiteLocation, destinationPoint, Aircraft.Rotation, num2 * elapsedTime, out headingDifference);
		Aircraft.Rotation = num3;
		Aircraft.FacingNormal = new Vector3((float)Math.Cos(num3), (float)Math.Sin(num3), 0f);
		SetRoll(headingDifference, elapsedTime);
		return vehicleContainerType.Aircraft.MaxAirSpeed;
	}

	private void SetRoll(float headingDifference, float elapsedTime)
	{
		VehicleContainerType vehicleContainerType = (VehicleContainerType)Aircraft.EntityType.ContainerType;
		float num = 5f * (Aircraft.Locomotor.MoveSpeed / vehicleContainerType.Aircraft.MaxAirSpeed) * headingDifference - vehicleComponent.Roll;
		if (!Common.IsEqual(num, 0f))
		{
			num = MathHelper.Clamp(num, -0.5f * elapsedTime, 0.5f * elapsedTime);
			vehicleComponent.Roll += num;
			vehicleComponent.Roll = MathHelper.Clamp(vehicleComponent.Roll, 0f - vehicleContainerType.Aircraft.MaxRollDegreeWhenTurning, vehicleContainerType.Aircraft.MaxRollDegreeWhenTurning);
		}
	}

	private float FindSlowCircleRadius(Vector2 waypoint)
	{
		Vector2 vector = new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y);
		Vector2 vector2 = new Vector2(0f - Aircraft.FacingNormal.Y, Aircraft.FacingNormal.X);
		Vector2 vector3 = (waypoint - vector) / 2f;
		Vector2 vector4 = new Vector2(0f - vector3.Y, vector3.X);
		Vector2 vector5 = vector + vector3;
		Vector2 result = Vector2.Zero;
		Common.IntersectionOfTwoLines(vector, vector + 10000f * vector2, vector5, vector5 + 10000f * vector4, ref result);
		return (vector - result).Length();
	}

	public static float TurnToFace(Vector3 position, Vector2 faceThis, float currentAngle, float turnSpeed, out float headingDifference)
	{
		float num = faceThis.X - position.X;
		float num2 = MathHelper.Clamp(headingDifference = Common.WrapAngleBetweenMinusPiAndPi((float)Math.Atan2(faceThis.Y - position.Y, num) - currentAngle), 0f - turnSpeed, turnSpeed);
		return Common.WrapAngleBetweenZeroAndTwoPi(currentAngle + num2);
	}

	public bool IsAtDestination()
	{
		return Vector2.DistanceSquared(new Vector2(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y), destinationPoint) < 16f;
	}
}
