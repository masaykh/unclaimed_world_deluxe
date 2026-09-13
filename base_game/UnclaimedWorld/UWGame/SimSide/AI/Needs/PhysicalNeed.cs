using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs;

public class PhysicalNeed : ISnapshot
{
	public Need Parent;

	public float DaysAtZero;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public PhysicalNeed()
	{
	}

	public PhysicalNeed(Need parent)
	{
		Parent = parent;
	}

	public void Update(double deltaTimeInSeconds)
	{
		if (Parent.NeedType.PhysicalEffects.DaysAtZeroDecreaseFactor.HasValue)
		{
			if (Common.IsZero(Parent.CurrentLevel))
			{
				DaysAtZero += (float)(deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond);
			}
			else if (DaysAtZero > 0f)
			{
				DaysAtZero -= Parent.NeedType.PhysicalEffects.DaysAtZeroDecreaseFactor.Value * (float)(deltaTimeInSeconds * The.Sim.DateAndTime.DaysPerSecond);
				DaysAtZero = Common.ClampBottom(DaysAtZero, 0f);
			}
		}
	}

	public bool IsStarvedToDeath()
	{
		if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
		{
			return DaysAtZero > Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath;
		}
		return false;
	}

	public bool IsCollapsedFromStarvation()
	{
		if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.HasValue)
		{
			return DaysAtZero > Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse;
		}
		return false;
	}

	public float GetStarvedToDeathFraction()
	{
		if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
		{
			return Common.Clamp(1f - DaysAtZero / Parent.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.Value, 0f, 1f);
		}
		return 1f;
	}

	public float GetCollapsedFraction()
	{
		if (Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.HasValue)
		{
			return Common.Clamp(1f - DaysAtZero / Parent.NeedType.PhysicalEffects.DaysAtZeroCausingCollapse.Value, 0f, 1f);
		}
		return 1f;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		DaysAtZero = sn.DoFloat(DaysAtZero);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
