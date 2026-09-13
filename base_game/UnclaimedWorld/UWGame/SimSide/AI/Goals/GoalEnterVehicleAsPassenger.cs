using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AI.Goals;

internal class GoalEnterVehicleAsPassenger : CompositeGoal
{
	private Entity vehicle;

	private Vehicle vehicleComponent;

	private GoalFollowPath parentGoal;

	private PassengerOrCargoSlot slot;

	private float? previousDistanceToVehicle;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalEnterVehicleAsPassenger(Entity owner, Entity vehicle, GoalFollowPath parentGoal, float? previousDistanceToVehicle, List<EntityGroupID> ownersOfVehicles)
		: base(owner)
	{
		this.vehicle = vehicle;
		vehicle.Find<Vehicle>(out vehicleComponent);
		this.previousDistanceToVehicle = previousDistanceToVehicle;
		this.parentGoal = parentGoal;
		base.ownersOfVehicles = ownersOfVehicles;
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
		sn.DoUnknownObject(vehicle);
		sn.DoUnknownObject(vehicleComponent);
		sn.DoUnknownObject(parentGoal);
		sn.DoUnknownObject(slot);
		previousDistanceToVehicle = sn.DoFloatNullable(previousDistanceToVehicle);
		return this;
	}

	public GoalEnterVehicleAsPassenger()
	{
	}

	protected override void Activate()
	{
		float num = Common.DistanceOctile(entity.PlaySiteLocation, vehicle.Location.Value);
		bool flag = false;
		if (num < 90f)
		{
			if (!vehicle.Locomotor.IsMovingOrRotating)
			{
				if (vehicleComponent.Passengers.Count < ((VehicleContainerType)vehicle.EntityType.ContainerType).MaxPassengers)
				{
					base.Status = Status.Active;
					slot = vehicleComponent.GetPlaceForNewPassenger();
					if (slot != null)
					{
						slot.PassengerSlot.TargetedBy = entity;
						slot.GetEntryPoints(out var transformedEntry, out var transformedPointToFace);
						AddSubgoal(new GoalMoveToPosition(entity, transformedEntry, null, GoalMoveToPosition.VehicleUse.NoVehicle));
						AddSubgoal(new GoalTurnToFace(entity, transformedPointToFace.ToVector2()));
					}
					else
					{
						base.Status = Status.Failed;
					}
				}
				else
				{
					base.Status = Status.Failed;
				}
				return;
			}
			flag = true;
		}
		else if (!previousDistanceToVehicle.HasValue || previousDistanceToVehicle.Value > num)
		{
			previousDistanceToVehicle = num;
			flag = true;
		}
		else
		{
			flag = false;
		}
		if (flag)
		{
			parentGoal.AddSubgoal(new GoalWaitForRide(entity, 1.0));
			parentGoal.AddSubgoal(new GoalEnterVehicleAsPassenger(entity, vehicle, parentGoal, previousDistanceToVehicle, ownersOfVehicles));
			base.Status = Status.Completed;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Completed)
		{
			vehicleComponent.EnterVehicleAsPassenger(entity, slot);
			AttachPoint attachPointFromKeyName = vehicle.Renderable.RenderAsModel.ModelData.GetAttachPointFromKeyName(slot.PassengerOrCargoSlotType.AttachPointName);
			if (attachPointFromKeyName != null)
			{
				_ = attachPointFromKeyName.Translation;
				float finalModelScale = entity.Renderable.RenderAsModel.FinalModelScale;
				_ = attachPointFromKeyName.BoneName;
				_ = attachPointFromKeyName.AttachedAnimationName;
				entity.Renderable.SetAnimationStateFlag(AnimModifier.Passenger);
				RenderAsModel.GetAttachTransformations(entity.Renderable, attachPointFromKeyName, AttacheePoint.Bottom, null, out var _, out var translation, out var rotation);
				vehicle.Renderable.AttachEntityAndCreateLocalTransform(entity.Renderable, finalModelScale, attachPointFromKeyName, entity.Renderable.RenderAsModel.ModelData.BottomAttachee, AttacheePoint.Bottom, translation, rotation);
			}
			parentGoal.AddSubgoal(new GoalWaitAsPassenger(entity));
			parentGoal.AddSubgoal(new GoalExitVehicle(entity, vehicle.EntityID));
			parentGoal.Path.Clear();
			parentGoal.WaypointPath.Clear();
			parentGoal.AddSubgoal(new GoalMoveToPosition(entity, parentGoal.parentGoal.Destination.Value.ToVector3(), ownersOfVehicles));
			base.Status = Status.Completed;
			vehicleComponent.DrivenBy.SendMessage(new Message(entity, Message.MessageTypes.OKImOn, null));
		}
	}
}
