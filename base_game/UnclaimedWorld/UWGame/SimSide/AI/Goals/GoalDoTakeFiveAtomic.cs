using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalDoTakeFiveAtomic : Goal
{
	private double timeToRest;

	public double TimeAlreadyRested;

	private double startedAt;

	private const double maxPeriodInSeconds = 0.2;

	private static Pool<GoalDoTakeFiveAtomic> freeGoals = new Pool<GoalDoTakeFiveAtomic>(40);

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private void Init(Entity owner, double timeAlreadyRested, double timeToRest)
	{
		this.timeToRest = timeToRest;
		startedAt = timeAlreadyRested;
		TimeAlreadyRested = timeAlreadyRested;
		Init(owner);
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public static GoalDoTakeFiveAtomic GetGoal(Entity owner, double timeAlreadyRested, double timeToRest)
	{
		GoalDoTakeFiveAtomic goalDoTakeFiveAtomic = freeGoals.Get();
		goalDoTakeFiveAtomic.Init(owner, timeAlreadyRested, timeToRest);
		return goalDoTakeFiveAtomic;
	}

	public override void RetireGoal()
	{
		freeGoals.Retire(this);
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		TimeAlreadyRested += elapsed.ElapsedGameTime.TotalSeconds;
		if (TimeAlreadyRested > timeToRest)
		{
			base.Status = Status.Completed;
		}
		else if (TimeAlreadyRested - startedAt > 0.2)
		{
			base.Status = Status.Completed;
		}
	}

	public override float GetExertionLevel()
	{
		if (entity.HasStance())
		{
			return entity.Locomotor.Stance.CurrentStance.IdleExertionLevel;
		}
		return GameData.Instance.Constants.PhysicalWork.IdleExertionDefault;
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
		timeToRest = sn.DoDouble(timeToRest);
		TimeAlreadyRested = sn.DoDouble(TimeAlreadyRested);
		startedAt = sn.DoDouble(startedAt);
		sn.Ignore(freeGoals);
		sn.Ignore(0.2);
		return this;
	}

	public static void ClearPool()
	{
		freeGoals.Clear();
	}
}
