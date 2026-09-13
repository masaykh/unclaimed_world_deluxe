using System.Linq;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs;

public class Need : ISnapshot
{
	public NeedType NeedType;

	private string snapshotNeedTypeKey;

	public PhysicalNeed PhysicalNeed;

	public FoodNeed FoodNeed;

	public float DecreasePerDay;

	public Needs Parent;

	private float currentLevel = 1f;

	public bool CurrentLevelIsDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float CurrentLevel
	{
		get
		{
			return currentLevel;
		}
		set
		{
			value = Common.Clamp(value, 0f, 1f);
			if (currentLevel != value)
			{
				currentLevel = value;
				Parent.SetNeedsDirty();
				if (FoodNeed != null)
				{
					FoodNeed.SetNeedDirty();
				}
			}
		}
	}

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		currentLevel = sn.DoFloat(currentLevel);
		CurrentLevelIsDirty = sn.DoBool(CurrentLevelIsDirty);
		DecreasePerDay = sn.DoFloat(DecreasePerDay);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotNeedTypeKey = NeedType.KeyName;
		}
		snapshotNeedTypeKey = sn.DoString(snapshotNeedTypeKey);
		PhysicalNeed = (PhysicalNeed)sn.DoISnapshot(PhysicalNeed);
		FoodNeed = (FoodNeed)sn.DoISnapshot(FoodNeed);
		sn.Ignore(Parent);
		sn.Ignore(NeedType);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		NeedType = Parent.Parent.AgeGroup.AgeGroupType.NeedTypes.First((NeedType n) => n.KeyName == snapshotNeedTypeKey);
		if (PhysicalNeed != null)
		{
			PhysicalNeed.Parent = this;
			PhysicalNeed.LoadPostProcess(sn);
		}
		if (FoodNeed != null)
		{
			FoodNeed.Parent = this;
			FoodNeed.LoadPostProcess(sn);
		}
	}

	public Need()
	{
	}

	public Need(Needs parent, NeedType needType)
	{
		Parent = parent;
		NeedType = needType;
		if (needType.PhysicalEffects != null)
		{
			PhysicalNeed = new PhysicalNeed(this);
		}
		if (needType.FoodNeedType != null)
		{
			FoodNeed = new FoodNeed(this);
		}
	}

	public float? GetEnergyFactor()
	{
		return NeedType.GetEnergyFactor(CurrentLevel);
	}

	public void Satisfy(float amount)
	{
		CurrentLevel += amount;
	}

	public void Update(double deltaTimeInSeconds)
	{
		float num = 1f;
		if (PhysicalNeed != null && NeedType.PhysicalEffects.UseExertionFactorToDecrease)
		{
			num = Parent.Parent.Parent.Intelligence.Brain.GetExertionLevelOfActivity();
		}
		CurrentLevel = Common.DecreaseValueBetweenZeroAndOne(CurrentLevel, num * DecreasePerDay, deltaTimeInSeconds);
		if (PhysicalNeed != null)
		{
			PhysicalNeed.Update(deltaTimeInSeconds);
		}
	}

	public float GetWeightedStatus()
	{
		if (PhysicalNeed != null)
		{
			return 0.5f * CurrentLevel + 0.5f * PhysicalNeed.GetStarvedToDeathFraction();
		}
		return CurrentLevel;
	}
}
