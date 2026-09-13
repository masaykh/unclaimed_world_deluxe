using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.AI.Goals;

internal class GoalWaitForPassenger : Goal
{
	private double? maxPeriodInSeconds;

	private double waitProgress;

	private Entity passenger;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalWaitForPassenger(Entity owner, double? maxPeriod, Entity passenger)
		: base(owner)
	{
		maxPeriodInSeconds = maxPeriod;
		this.passenger = passenger;
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
		maxPeriodInSeconds = sn.DoDoubleNullable(maxPeriodInSeconds);
		waitProgress = sn.DoDouble(waitProgress);
		sn.DoUnknownObject(passenger);
		return this;
	}

	public GoalWaitForPassenger()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		passenger.SendMessage(new Message(entity, Message.MessageTypes.HopOnBoard, null));
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!entity.GetDrivenVehicle(out var vehicle) || vehicle == null)
		{
			base.Status = Status.Failed;
			return;
		}
		Vehicle vehicle2 = vehicle.Vehicle;
		if (vehicle2.WaitingForPassengers == 0)
		{
			base.Status = Status.Completed;
		}
		else if (maxPeriodInSeconds.HasValue)
		{
			waitProgress += elapsed.ElapsedGameTime.TotalSeconds;
			if (waitProgress > maxPeriodInSeconds)
			{
				SetNoLongerWaiting(vehicle2);
				base.Status = Status.Completed;
			}
		}
	}

	private void SetNoLongerWaiting(Vehicle vehicle)
	{
		vehicle.WaitingForPassengers--;
		if (vehicle.WaitingForPassengers < 0)
		{
			vehicle.WaitingForPassengers = 0;
		}
	}

	public override bool HandleMessage(Message message)
	{
		Message.MessageTypes messageType = message.MessageType;
		if (messageType == Message.MessageTypes.OKImOn)
		{
			if (!entity.GetDrivenVehicle(out var vehicle) || vehicle == null)
			{
				base.Status = Status.Failed;
				return true;
			}
			Vehicle vehicle2 = vehicle.Vehicle;
			SetNoLongerWaiting(vehicle2);
			base.Status = Status.Completed;
			return true;
		}
		return false;
	}
}
