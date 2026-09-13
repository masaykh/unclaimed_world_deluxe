using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

public class GoalTakeoff : CompositeGoal
{
	public Entity Aircraft;

	private Vehicle vehicleComponent;

	private Vector3 destinationPoint;

	private const float atDestinationLimit = 16f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalTakeoff(Entity owner)
		: base(owner)
	{
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
		sn.DoUnknownObject(Aircraft);
		destinationPoint = sn.DoVector3(destinationPoint);
		sn.DoUnknownObject(vehicleComponent);
		return this;
	}

	public GoalTakeoff()
	{
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		if (!entity.GetDrivenVehicle(out Aircraft))
		{
			base.Status = Status.Failed;
			return;
		}
		Aircraft.Find<Vehicle>(out vehicleComponent);
		base.Status = Status.Active;
		destinationPoint = new Vector3(Aircraft.PlaySiteLocation.X, Aircraft.PlaySiteLocation.Y, ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.CruiseAltitude);
		AddSubgoal(new GoalWait(entity, 3.0, null, AnimModifier.Pre, AnimModifier.Right, AnimModifier.Slow));
		AddSubgoal(new GoalWait(entity, 3.0, null, AnimModifier.Pre, AnimModifier.Left, AnimModifier.Slow));
		AddSubgoal(new GoalWait(entity, 3.0, AnimAction.Containing, AnimModifier.Crew));
		AddSubgoal(new GoalWait(entity, 3.0, AnimAction.Containing, AnimModifier.Passenger));
		AddSubgoal(new GoalWait(entity, 3.0, AnimAction.Containing, AnimModifier.Open));
	}

	public static bool UpdateAircraftUpDown(float desiredMoveSpeed, GameTime elapsed, Entity pilot, Entity aircraft, Vehicle vehicleComponent, Vector3 destinationPoint)
	{
		float num = (float)elapsed.ElapsedGameTime.TotalSeconds;
		Vector2 zero = Vector2.Zero;
		float verticalSpeed = vehicleComponent.Aircraft.VerticalSpeed;
		double num2 = Vector3.Distance(destinationPoint, aircraft.PlaySiteLocation);
		if (num2 > 0.0)
		{
			VehicleContainerType vehicleContainerType = (VehicleContainerType)aircraft.EntityType.ContainerType;
			double value = MathHelper.Clamp(desiredMoveSpeed, verticalSpeed - vehicleContainerType.Aircraft.MaxVerticalAcceleration * num, verticalSpeed + vehicleContainerType.Aircraft.MaxVerticalAcceleration * num);
			value = (double)Math.Sign(value) * Math.Min(num2 / (double)vehicleContainerType.Deceleration, Math.Abs(value));
			vehicleComponent.Aircraft.VerticalSpeed = (float)value;
		}
		float num3 = 10f * num;
		zero.X = MathHelper.Clamp(destinationPoint.X - aircraft.PlaySiteLocation.X, 0f - num3, num3);
		zero.Y = MathHelper.Clamp(destinationPoint.Y - aircraft.PlaySiteLocation.Y, 0f - num3, num3);
		pilot.Location = aircraft.Location + new Vector3(zero, vehicleComponent.Aircraft.VerticalSpeed * num);
		if (aircraft.Renderable.RenderAsModel.ModelData.HasAircraftDucts)
		{
			GoalFlyToPosition.SetLeftDuctAngle(aircraft, vehicleComponent, 0f, num);
			GoalFlyToPosition.SetRightDuctAngle(aircraft, vehicleComponent, 0f, num);
		}
		if (Vector3.DistanceSquared(aircraft.PlaySiteLocation, destinationPoint) < 16f)
		{
			return true;
		}
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Completed)
		{
			Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", show: false);
			Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", show: false);
			if (UpdateAircraftUpDown(((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxVerticalMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
			{
				Aircraft.Locomotor.MoveSpeed = 0f;
				vehicleComponent.Aircraft.VerticalSpeed = 0f;
				base.Status = Status.Completed;
			}
			else
			{
				AddSkimmerDust(Aircraft);
				base.Status = Status.Active;
			}
		}
	}

	public static void AddSkimmerDust(Entity aircraft)
	{
		float altitudeForDust = ((VehicleContainerType)aircraft.EntityType.ContainerType).Aircraft.AltitudeForDust;
		float num = Math.Max((altitudeForDust - aircraft.PlaySiteLocation.Z) / altitudeForDust, 0f);
		num *= The.Map.GetTile(aircraft.MapPosition.Value).GetDustFactor();
		The.Client.ParticleManager.AddDust(aircraft, aircraft.PlaySiteLocation, num, GameData.Instance.Constants.SkimmerDustScale);
	}
}
