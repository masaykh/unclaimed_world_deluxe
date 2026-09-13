using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AI.Goals;

public class GoalLand : Goal
{
	public Entity Aircraft;

	private Vehicle vehicleComponent;

	private Vector3 destinationPoint;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalLand(Entity owner)
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
		sn.DoUnknownObject(vehicleComponent);
		destinationPoint = sn.DoVector3(destinationPoint);
		return this;
	}

	public GoalLand()
	{
	}

	protected override void Activate()
	{
		if (!entity.GetDrivenVehicle(out Aircraft))
		{
			base.Status = Status.Failed;
			return;
		}
		Aircraft.Find<Vehicle>(out vehicleComponent);
		base.Status = Status.Active;
		destinationPoint = new Vector3(48f * (0.5f + (float)entity.MapPosition.Value.X), 48f * (0.5f + (float)entity.MapPosition.Value.Y), 0f);
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (GoalTakeoff.UpdateAircraftUpDown(0f - ((VehicleContainerType)Aircraft.EntityType.ContainerType).Aircraft.MaxVerticalMoveSpeed, elapsed, entity, Aircraft, vehicleComponent, destinationPoint))
		{
			Aircraft.Locomotor.MoveSpeed = 0f;
			vehicleComponent.Aircraft.VerticalSpeed = 0f;
			entity.Location = new Vector3(entity.PlaySiteLocation.X, entity.PlaySiteLocation.Y, 0f);
			Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_left", show: true);
			Aircraft.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.ShowMesh("propeller_right", show: true);
			Aircraft.Renderable.StartAdditionalAnimation("propeller_right_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal);
			Aircraft.Renderable.StartAdditionalAnimation("propeller_left_stop", Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal);
			base.Status = Status.Completed;
		}
		else
		{
			GoalTakeoff.AddSkimmerDust(Aircraft);
		}
	}
}
