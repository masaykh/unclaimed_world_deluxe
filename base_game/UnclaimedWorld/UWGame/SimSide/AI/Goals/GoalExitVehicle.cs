using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AI.Goals;

internal class GoalExitVehicle : CompositeGoal
{
	private EntityID vehicle;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalExitVehicle(Entity owner, EntityID vehicle)
		: base(owner)
	{
		this.vehicle = vehicle;
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
		vehicle = sn.DoEnum(vehicle);
		return this;
	}

	public GoalExitVehicle()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (!entityIntelligence.GetEntitySeenDirectly(vehicle, out var _))
		{
			base.Status = Status.Failed;
		}
		else if (base.entity.DrivingVehicle == vehicle || base.entity.PassengerInVehicle == vehicle)
		{
			AddSubgoal(new GoalWait(base.entity, 3.0, AnimAction.Containing, AnimModifier.Crew));
		}
		else
		{
			base.Status = Status.Completed;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (IsDelayed(elapsed))
		{
			return;
		}
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status != Status.Completed || (!base.entity.DrivingVehicle.HasValue && !base.entity.PassengerInVehicle.HasValue))
		{
			return;
		}
		if (!entityIntelligence.GetEntitySeenDirectly(this.vehicle, out var entity))
		{
			base.Status = Status.Failed;
			return;
		}
		bool flag = base.entity.DrivingVehicle == this.vehicle;
		if (flag)
		{
			base.entity.DeleteTriggerOfType(GameData.Instance.AllTriggerTypes["drivenVehicle"]);
		}
		Vehicle vehicle = entity.Vehicle;
		PassengerOrCargoSlot entityPlaceInVehicle = vehicle.GetEntityPlaceInVehicle(base.entity);
		entityPlaceInVehicle.GetEntryPoints(out var transformedEntry, out var _);
		if (vehicle.ExitVehicle(base.entity))
		{
			base.entity.Location = transformedEntry;
			base.entity.FacingNormal = entity.FacingNormal;
			base.entity.Rotation = entity.Rotation;
			AttachPoint attachPoint = ((!flag) ? entity.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(entityPlaceInVehicle.PassengerOrCargoSlotType.AttachPointName) : entity.Renderable.RenderAsModel.ModelData.DriverAttachor);
			if (attachPoint != null)
			{
				entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.DeattachObject(base.entity.Renderable.RenderAsModel, attachPoint.BoneName);
			}
		}
		if (flag && vehicle.Passengers.Count > 0)
		{
			for (int i = 0; i < vehicle.Passengers.Count; i++)
			{
				vehicle.TellPassengerToGetOff(base.entity, vehicle.Passengers[i]);
			}
		}
		entity.Renderable.StartAdditionalAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);
		base.Status = Status.Completed;
	}
}
