using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AI.Goals;

internal class GoalEnterVehicleAsDriver : CompositeGoal
{
	private EntityID vehicle;

	private Trigger trigger;

	private PassengerOrCargoSlot driversSlot;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalEnterVehicleAsDriver(Entity owner, EntityID vehicle, Trigger trigger)
		: base(owner)
	{
		this.vehicle = vehicle;
		this.trigger = trigger;
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
		sn.DoUnknownObject(trigger);
		sn.DoUnknownObject(driversSlot);
		return this;
	}

	public GoalEnterVehicleAsDriver()
	{
	}

	protected override void Activate()
	{
		RemoveAllSubgoals();
		IKnownEntityData data;
		EntityResult knownData = entityIntelligence.GetKnownData(vehicle, out data);
		bool flag = false;
		if (knownData == EntityResult.SeenDirectly)
		{
			Container contains = ((Entity)data).Contains;
			if (contains != null && contains is ICrew crew)
			{
				if (crew.IsDriver(entity))
				{
					base.Status = Status.Completed;
					return;
				}
				if (crew.Driver.HasValue)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			base.Status = Status.Active;
			driversSlot = data.GetFreeDriversSlot();
			driversSlot.GetEntryPoints(out var transformedEntry, out var transformedPointToFace);
			AddSubgoal(new GoalMoveToPosition(entity, transformedEntry, null, GoalMoveToPosition.VehicleUse.NoVehicle));
			AddSubgoal(new GoalTurnToFace(entity, transformedPointToFace.ToVector2()));
			AddSubgoal(new GoalWait(entity, 3.0, AnimAction.Containing, AnimModifier.Crew));
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status != Status.Completed)
		{
			return;
		}
		if (!entityIntelligence.GetEntitySeenDirectly(this.vehicle, out var entity))
		{
			base.Status = Status.Failed;
			return;
		}
		Vehicle vehicle = entity.Vehicle;
		if (vehicle.DrivenBy == null && driversSlot.DriversSlot.Driver == null)
		{
			vehicle.EnterVehicleAsDriver(base.entity, driversSlot);
			if (entity.Renderable.RenderAsModel.ModelData.DriverAttachor != null)
			{
				AttachPoint driverAttachor = entity.Renderable.RenderAsModel.ModelData.DriverAttachor;
				_ = driverAttachor.Translation;
				_ = driverAttachor.BoneName;
				float finalModelScale = base.entity.Renderable.RenderAsModel.FinalModelScale;
				RenderAsModel.GetAttachTransformations(base.entity.Renderable, driverAttachor, AttacheePoint.Bottom, null, out var _, out var translation, out var rotation);
				entity.Renderable.AttachEntityAndCreateLocalTransform(base.entity.Renderable, finalModelScale, driverAttachor, base.entity.Renderable.RenderAsModel.ModelData.BottomAttachee, AttacheePoint.Bottom, translation, rotation);
			}
			entity.Renderable.StartAdditionalAnimation("driver_entry", Playback.Backwards, StartingPoint.Current, BlendMode.Normal);
			base.entity.Renderable.SetAnimationStateFlag(AnimModifier.Crew);
			if (trigger != null)
			{
				base.entity.AttachTrigger(trigger);
			}
			base.Status = Status.Completed;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}
}
