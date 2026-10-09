using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.Mods;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

/// <summary>
/// A predator walks to a building's door and wears it down until it breaks (HomeRaidMod).
///
/// tripleacoder's shape for it: an evaluator (EvaluateBreakIn) and a top-level goal, so the brain
/// weighs a raid against everything else it could do - eating, fighting back, fleeing - instead of a
/// driver outside the AI deciding for it. The brain drops the goal whenever something scores
/// higher, which is how "the raid stops if the predator is attacked" now happens.
///
/// In core rather than mods/ because goals are saved by type name (Snapshotter, Type.GetType): a
/// save taken mid-raid has to load in a build without the mod. There, and whenever the mod is
/// switched off, the goal fails on its first update and the brain moves on.
///
/// Two stages. On the way: a GoalMoveToPosition to the building's access point, given up after
/// HomeRaidMod.GiveUpAfterSeconds. At the door: Entity.DoDamage at HomeRaidMod.DamagePerSecond,
/// playing the attack animation. When its integrity reaches 0 the building is broken - Kastuk:
/// "Building will just be broken, just like abandoned unclaimed structures" - so it stays
/// standing and repairable, and Container.ThrowOutContents puts its occupants and stock on the
/// ground.
/// </summary>
internal class GoalBreakIn : CompositeGoal, ITopLevelGoal
{
	private EntityID target;

	private bool atDoor;

	private double secondsWalking;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalBreakIn(Entity owner, EntityID target)
		: base(owner)
	{
		this.target = target;
	}

	public GoalBreakIn()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		Entity building = Entity.FindByID(target);
		if (!HomeRaidMod.Enabled || building?.AccessPoint == null)
		{
			base.Status = Status.Failed;
			return;
		}
		if (!atDoor)
		{
			AddSubgoal(new GoalMoveToPosition(entity, building.AccessPoint.Value, null));
		}
		// For HomeRaidMod's once-a-day limit; a raid resumed after a load counts again, harmlessly.
		HomeRaidMod.RaidStarted(entity);
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		Entity building = Entity.FindByID(target);
		if (!HomeRaidMod.Enabled || building == null || !entity.Location.HasValue)
		{
			base.Status = Status.Failed;
			return;
		}
		double seconds = elapsed.ElapsedGameTime.TotalSeconds;
		if (!atDoor)
		{
			secondsWalking += seconds;
			float distance = Vector3.Distance(entity.PlaySiteLocation, building.AccessPoint ?? building.PlaySiteLocation);
			if (distance > HomeRaidMod.ReachDistance)
			{
				Status status = ProcessSubgoals(elapsed);
				if (status == Status.Failed || secondsWalking > HomeRaidMod.GiveUpAfterSeconds)
				{
					base.Status = Status.Failed;
				}
				else if (status == Status.Completed)
				{
					// Arrived, but not within reach of the door: nothing more to walk to.
					base.Status = Status.Failed;
				}
				return;
			}
			atDoor = true;
			RemoveAllSubgoals();
		}
		entity.Renderable?.SetAnimationActionStateFlag(AnimAction.Attacking);
		if (building.DoDamage(HomeRaidMod.DamagePerSecond() * (float)seconds) && Entity.FindByID(target) != null)
		{
			building.Contains?.ThrowOutContents();
			base.Status = Status.Completed;
		}
	}

	public override void Terminate()
	{
		entity?.Renderable?.ClearAnimationActionStateFlag(AnimAction.Attacking);
		base.Terminate();
	}

	public override string GetStatus()
	{
		return atDoor ? UWGame.Locale.Text("Breaking in") : UWGame.Locale.Text("Stalking a building");
	}

	public override bool IsSame(UWGame.SimSide.Jobs.Job job)
	{
		return false;
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
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
		target = sn.DoEnum(target);
		atDoor = sn.DoBool(atDoor);
		secondsWalking = sn.DoDouble(secondsWalking);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}
}
