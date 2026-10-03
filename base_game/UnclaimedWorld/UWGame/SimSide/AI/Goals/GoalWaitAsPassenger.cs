using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalWaitAsPassenger : Goal, ITopLevelGoal
{
	private double? maxPeriodInSeconds;

	private double waitProgress;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalWaitAsPassenger(Entity owner)
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
		maxPeriodInSeconds = sn.DoDoubleNullable(maxPeriodInSeconds);
		waitProgress = sn.DoDouble(waitProgress);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}

	public GoalWaitAsPassenger()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
	}

	public double ScoreGoal()
	{
		return 1.0;
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	/// <summary>
	/// PORT FIX. Whether <paramref name="passenger"/> is still in something it could get off -
	/// a container (a transport's hold) or a vehicle seat. Out of both, there is nothing left to
	/// wait for, and <see cref="ProcessWhileActive"/> ends the wait.
	///
	/// The studio's wait ends only on a GetOff message ("// do nothing... just wait..."), and
	/// cargo never gets one. TravelAction.TransferToPlaySite gives everything in an arriving
	/// transport this goal, at a ScoreGoal of 1.0 that nothing outbids and with nothing calling
	/// ArbitrateWhileBusy; UnloadAction - which unloads dogs and robots as trade goods on purpose,
	/// the studio's "// dogs + robots" and "// NEW: dogs also" - then takes them out of the
	/// transport in one step, onto the ground at the dock. Colonists leave through
	/// DisembarkAction instead, whose Disembark message makes GoalThink drop this goal; cargo
	/// gets no message at all. So a dog or GOPHER bought at a Port arrived the player's property
	/// and a member of the colony, standing on the ground, and waited as a passenger forever.
	/// Kastuk: "buy dog, it's appear at dock and still stand mindlessly." His save has exactly one
	/// GoalWaitAsPassenger in it, the top-level goal of the dog standing at the dock, which is
	/// contained by nothing.
	///
	/// Decided here, from the state, rather than at the unload, so that a creature already stuck
	/// in a save gets up as soon as the save loads - no repair pass, nothing to run on load. A
	/// real passenger is always in one or the other, so this never cuts a ride short.
	/// </summary>
	private static bool IsAboard(Entity passenger)
	{
		return passenger.ContainedBy.HasValue || passenger.PassengerInVehicle.HasValue;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!IsAboard(entity))
		{
			base.Status = Status.Completed;
			return;
		}
		if (maxPeriodInSeconds.HasValue)
		{
			waitProgress += elapsed.ElapsedGameTime.TotalSeconds;
			if (waitProgress > maxPeriodInSeconds)
			{
				base.Status = Status.Completed;
			}
		}
	}

	public override bool HandleMessage(Message message)
	{
		Message.MessageTypes messageType = message.MessageType;
		if (messageType == Message.MessageTypes.GetOff)
		{
			base.Status = Status.Completed;
			return true;
		}
		return false;
	}
}
